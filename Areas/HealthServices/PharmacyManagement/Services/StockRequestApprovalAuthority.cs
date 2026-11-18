using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;

/// <summary>
/// Kewenangan memutus permintaan stok (<c>LGS-DEC-002</c>) yang diperiksa di dalam domain,
/// terpisah dari filter izin di controller.
/// </summary>
/// <remarks>
/// <para>
/// <b>Mengapa ada di domain, bukan cukup di controller.</b> Filter
/// <c>[AccessPermission]</c> mengikuti kebijakan platform: peran SuperAdmin lolos selama
/// <c>Security:Authorization:EnforceClinicalPolicyForSuperAdmin</c> bernilai false, dan seluruh
/// pemeriksaan dilepas bila <c>Security:Authorization:Enabled</c> bernilai false di luar
/// Production. Kebijakan itu tidak diubah. Dua aturan di bawah ini sengaja tidak ikut
/// pelepasan tersebut, karena keduanya bagian dari aturan bisnis permintaan stok, bukan
/// sekadar hak membuka endpoint.
/// </para>
/// <para>
/// <b>Atasan unit</b> dibaca dari struktur organisasi yang sudah ada, tanpa tabel baru:
/// <c>MstServiceUnit.OrganizationUnitId</c> menunjuk unit organisasi, dan atasan unit adalah
/// petugas dengan penempatan manajerial (<c>WfpOrganizationAssignment.IsManagerialAssignment</c>)
/// yang aktif dan berlaku pada unit organisasi itu. Bila tautan atau penempatannya belum diisi,
/// jawabannya "tidak berwenang" beserta sebabnya — tidak ada yang ditebak.
/// </para>
/// <para>
/// <b>Otorisasi CITO</b> (<c>LGS-DEC-004</c>) menuntut dua hal sekaligus: penempatan jabatan
/// yang aktif pada salah satu <b>jabatan yang ditunjuk manajemen</b>
/// (<see cref="StockRequestEmergencyAuthorizerDesignation"/>), dan izin
/// <c>StockRequest:AuthorizeEmergency</c> yang benar-benar diberikan kepada penempatan itu
/// (<c>ApplicationUserOrganization</c> × <c>SysAccessPolicy</c>). Penyaringnya sama dengan jalur
/// kebijakan pada <c>AccessPermissionService.HasAccessAsync</c> tetapi tanpa jalan pintas
/// SuperAdmin, tanpa jalan pintas aksi system-only, dan tanpa saklar pengembangan. Bila jalur
/// kebijakan di sana berubah, penyaring di sini wajib ditinjau ulang. Daftar jabatan yang kosong
/// berarti tidak ada yang berwenang.
/// </para>
/// </remarks>
public sealed class StockRequestApprovalAuthority(ApplicationDbContext dbContext,
    StockRequestEmergencyAuthorizerDesignation? emergencyDesignation = null)
{
    private readonly StockRequestEmergencyAuthorizerDesignation _emergencyDesignation =
        emergencyDesignation ?? StockRequestEmergencyAuthorizerDesignation.Empty;

    /// <summary>Penunjukan jabatan otorisasi CITO yang sedang berlaku.</summary>
    public StockRequestEmergencyAuthorizerDesignation EmergencyDesignation => _emergencyDesignation;

    public const string PermissionResource = "StockRequest";
    public const string ApprovePermissionAction = "Approve";
    public const string EmergencyPermissionAction = "AuthorizeEmergency";

    /// <summary>
    /// Benar bila pengguna adalah atasan unit dari unit layanan peminta.
    /// </summary>
    public async Task<StockRequestUnitAuthority> CheckUnitAuthorityAsync(Guid userId,
        Guid requestingServiceUnitId, CancellationToken cancellationToken = default)
    {
        var unit = await dbContext.MstServiceUnits.AsNoTracking()
            .Where(x => x.Id == requestingServiceUnitId && !x.IsDelete)
            .Select(x => new { x.ServiceUnitName, x.OrganizationUnitId })
            .FirstOrDefaultAsync(cancellationToken);

        if (unit == null)
            return StockRequestUnitAuthority.Denied(
                StockRequestUnitAuthorityReason.ServiceUnitNotFound,
                "Unit peminta tidak ditemukan, sehingga atasan unitnya tidak dapat ditentukan.");

        if (!unit.OrganizationUnitId.HasValue)
            return StockRequestUnitAuthority.Denied(
                StockRequestUnitAuthorityReason.ServiceUnitNotMapped,
                $"Unit layanan {unit.ServiceUnitName} belum ditautkan ke unit organisasi, " +
                "sehingga atasan unitnya belum dapat ditentukan. Tautan diisi pada master unit layanan.");

        var organizationUnitId = unit.OrganizationUnitId.Value;
        var now = DateTime.UtcNow;

        // Unit organisasi harus aktif dan sedang berlaku. Unit yang sudah berakhir tidak lagi
        // memberi kewenangan, walaupun penempatan di dalamnya lupa ditutup.
        var organizationUnitActive = await dbContext.MstOrganizationUnits.AsNoTracking()
            .AnyAsync(x =>
                x.Id == organizationUnitId &&
                x.IsActive &&
                !x.IsDelete &&
                (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value <= now) &&
                (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= now),
                cancellationToken);

        if (!organizationUnitActive)
            return StockRequestUnitAuthority.Denied(
                StockRequestUnitAuthorityReason.OrganizationUnitInactive,
                $"Unit organisasi yang ditautkan ke {unit.ServiceUnitName} tidak aktif atau sudah " +
                "tidak berlaku.");

        var workforceProfileId = await dbContext.Users.AsNoTracking()
            .Where(x => x.Id == userId && x.IsActive)
            .Select(x => x.WorkforceProfileId)
            .FirstOrDefaultAsync(cancellationToken);

        if (!workforceProfileId.HasValue)
            return StockRequestUnitAuthority.Denied(
                StockRequestUnitAuthorityReason.NoWorkforceProfile,
                "Akun Anda belum terhubung ke profil tenaga kerja, sehingga penempatan jabatan " +
                "Anda tidak dapat diperiksa.");

        // Profil tenaga kerja yang sudah dinonaktifkan HR tidak lagi memberi kewenangan,
        // walaupun akunnya masih dapat login.
        var workforceActive = await dbContext.MstWorkforceProfiles.AsNoTracking()
            .AnyAsync(x => x.Id == workforceProfileId.Value && x.IsActive && !x.IsDelete,
                cancellationToken);

        if (!workforceActive)
            return StockRequestUnitAuthority.Denied(
                StockRequestUnitAuthorityReason.WorkforceProfileInactive,
                "Profil tenaga kerja Anda tidak aktif, sehingga tidak memberi kewenangan atasan unit.");

        // Penyaring masa berlaku sama dengan yang dipakai alur persetujuan HR
        // (WorkflowService.ResolveByOrganizationAssignmentAsync).
        var isUnitSupervisor = await dbContext.WfpOrganizationAssignments.AsNoTracking()
            .AnyAsync(x =>
                x.WorkforceProfileId == workforceProfileId.Value &&
                x.OrganizationUnitId == organizationUnitId &&
                x.IsManagerialAssignment &&
                x.IsActive &&
                !x.IsDelete &&
                !x.IsCancel &&
                x.EffectiveStartDate <= now &&
                (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= now),
                cancellationToken);

        return isUnitSupervisor
            ? StockRequestUnitAuthority.Granted()
            : StockRequestUnitAuthority.Denied(
                StockRequestUnitAuthorityReason.NotUnitSupervisor,
                $"Anda tidak tercatat sebagai atasan unit {unit.ServiceUnitName}: tidak ada " +
                "penempatan manajerial aktif Anda pada unit organisasi tersebut.");
    }

    /// <summary>
    /// Benar bila izin <paramref name="resource"/> : <paramref name="action"/> diberikan
    /// secara eksplisit kepada salah satu penempatan jabatan pengguna yang masih berlaku.
    /// </summary>
    /// <remarks>
    /// Status SuperAdmin dan saklar pengembangan tidak dipertimbangkan sama sekali. SuperAdmin
    /// yang memang bertugas mengotorisasi CITO tetap dapat melakukannya, asalkan penempatan
    /// jabatannya diberi izin itu seperti petugas lain.
    /// </remarks>
    /// <param name="userId">Akun yang diperiksa; akun nonaktif selalu tidak berizin.</param>
    /// <param name="resource">Nama controller izin, mis. <c>StockRequest</c>.</param>
    /// <param name="action">Nama aksi izin, mis. <c>Approve</c>.</param>
    /// <param name="cancellationToken">Pembatalan operasi.</param>
    /// <param name="allowedPositionIds">
    /// Bila diisi, hanya penempatan pada jabatan-jabatan ini yang dihitung. Kumpulan kosong berarti
    /// tidak ada penempatan yang memenuhi.
    /// </param>
    public async Task<bool> HasExplicitGrantAsync(Guid userId, string resource, string action,
        CancellationToken cancellationToken = default,
        IReadOnlyCollection<Guid>? allowedPositionIds = null)
    {
        if (allowedPositionIds is { Count: 0 }) return false;

        var userActive = await dbContext.Users.AsNoTracking()
            .AnyAsync(x => x.Id == userId && x.IsActive, cancellationToken);
        if (!userActive) return false;

        var grantable = await FindGrantableActionAsync(resource, action, cancellationToken);
        if (grantable == null) return false;

        var placements = GrantingPlacements(grantable, DateTime.UtcNow)
            .Where(x => x.UserId == userId);
        if (allowedPositionIds != null)
        {
            var positions = allowedPositionIds.ToList();
            placements = placements.Where(x => positions.Contains(x.PositionId));
        }

        return await placements.AnyAsync(cancellationToken);
    }

    /// <summary>Aksi izin yang dapat diberikan lewat kebijakan akses.</summary>
    private sealed record GrantableAction(Guid ActionAccessId, Guid ControllerAccessId);

    /// <summary>
    /// Aksi <paramref name="resource"/> : <paramref name="action"/> yang aktif, tidak dihapus, dan
    /// bukan system-only, pada controller yang juga aktif dan bukan system-only. Null bila belum
    /// terdaftar — artinya tidak ada yang memegang izin itu.
    /// </summary>
    private Task<GrantableAction?> FindGrantableActionAsync(string resource, string action,
        CancellationToken cancellationToken) =>
        dbContext.SysActionAccesses.AsNoTracking()
            .Where(x =>
                x.ActionName == action &&
                x.IsActive &&
                !x.IsDelete &&
                !x.IsSystemOnly &&
                x.ControllerAccess != null &&
                x.ControllerAccess.ControllerName == resource &&
                x.ControllerAccess.IsActive &&
                !x.ControllerAccess.IsDelete &&
                !x.ControllerAccess.IsSystemOnly)
            .Select(x => new GrantableAction(x.Id, x.ControllerAccessId))
            .FirstOrDefaultAsync(cancellationToken);

    /// <summary>
    /// Penempatan jabatan yang aktif, sedang berlaku, dan diberi aksi <paramref name="grantable"/>
    /// oleh kebijakan akses yang aktif. Satu-satunya penyaring izin eksplisit di kelas ini: dipakai
    /// pemeriksaan perorangan (<see cref="HasExplicitGrantAsync"/>) maupun kumpulan
    /// (<see cref="GetUsersWithExplicitGrantAsync"/>), supaya laporan kesiapan tidak pernah memakai
    /// aturan yang berbeda dari penegakan.
    /// </summary>
    private IQueryable<QuilvianSystemBackend.Models.ApplicationUserOrganization> GrantingPlacements(
        GrantableAction grantable, DateTime now)
    {
        var controllerAccessId = grantable.ControllerAccessId;
        var actionAccessId = grantable.ActionAccessId;

        return
            from organization in dbContext.ApplicationUserOrganizations.AsNoTracking()
            join policy in dbContext.SysAccessPolicies.AsNoTracking()
                on new { organization.DepartmentId, organization.PositionId }
                equals new { policy.DepartmentId, policy.PositionId }
            where organization.IsActive
                  && !organization.IsDelete
                  && !organization.IsCancel
                  && (!organization.EffectiveStartDate.HasValue ||
                      organization.EffectiveStartDate.Value <= now)
                  && (!organization.EffectiveEndDate.HasValue ||
                      organization.EffectiveEndDate.Value >= now)
                  && policy.ControllerAccessId == controllerAccessId
                  && policy.ActionAccessId == actionAccessId
                  && policy.IsAllowed
                  && policy.IsActive
                  && !policy.IsDelete
            select organization;
    }

    /// <summary>
    /// Jumlah kebijakan akses aktif yang memberi aksi itu; bila <paramref name="positionIds"/> diisi,
    /// jumlah jabatan berbeda di antaranya yang memegangnya. Aksinya disaring sama dengan pemeriksaan
    /// izin (<see cref="FindGrantableActionAsync"/>), sehingga izin pada controller atau aksi yang
    /// nonaktif tidak ikut terhitung di laporan kesiapan.
    /// </summary>
    public async Task<int> CountPolicyGrantsAsync(string resource, string action,
        IReadOnlyCollection<Guid>? positionIds = null, CancellationToken cancellationToken = default)
    {
        var grantable = await FindGrantableActionAsync(resource, action, cancellationToken);
        if (grantable == null) return 0;

        var controllerAccessId = grantable.ControllerAccessId;
        var actionAccessId = grantable.ActionAccessId;
        var policies = dbContext.SysAccessPolicies.AsNoTracking()
            .Where(x => x.ControllerAccessId == controllerAccessId &&
                        x.ActionAccessId == actionAccessId &&
                        x.IsAllowed && x.IsActive && !x.IsDelete);

        if (positionIds == null) return await policies.CountAsync(cancellationToken);

        var positions = positionIds.ToList();
        return await policies.Where(x => positions.Contains(x.PositionId))
            .Select(x => x.PositionId)
            .Distinct()
            .CountAsync(cancellationToken);
    }

    /// <summary>
    /// Kewenangan otorisasi CITO: penempatan aktif pada jabatan yang ditunjuk manajemen dan izin
    /// <c>StockRequest:AuthorizeEmergency</c> eksplisit pada penempatan itu.
    /// </summary>
    public async Task<StockRequestEmergencyAuthority> CheckEmergencyAuthorityAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        var designatedPositionIds = await ResolveDesignatedPositionIdsAsync(cancellationToken);

        if (designatedPositionIds.Count == 0)
            return new(false, StockRequestEmergencyAuthorityReason.DesignationNotConfigured,
                "Daftar jabatan yang ditunjuk manajemen untuk otorisasi CITO belum ditetapkan, " +
                "sehingga otorisasi CITO belum dapat dilakukan siapa pun.");

        var granted = await HasExplicitGrantAsync(userId, PermissionResource,
            EmergencyPermissionAction, cancellationToken, designatedPositionIds);

        return granted
            ? new(true, StockRequestEmergencyAuthorityReason.None, null)
            : new(false, StockRequestEmergencyAuthorityReason.NotDesignatedOrNotGranted,
                "Otorisasi CITO hanya dapat dilakukan petugas dengan penempatan jabatan aktif pada " +
                "jabatan yang ditunjuk manajemen dan diberi izin StockRequest:AuthorizeEmergency " +
                "secara eksplisit. Peran SuperAdmin saja tidak cukup.");
    }

    /// <summary>Kewenangan otorisasi CITO sebagai nilai benar/salah.</summary>
    public async Task<bool> HasEmergencyGrantAsync(Guid userId, CancellationToken cancellationToken = default) =>
        (await CheckEmergencyAuthorityAsync(userId, cancellationToken)).IsAuthorized;

    /// <summary>
    /// Jabatan yang ditunjuk manajemen, dalam bentuk id. Entri id maupun kode hanya berlaku bila
    /// jabatannya ada di <c>MstPosition</c> dan masih aktif: jabatan yang sudah dinonaktifkan tidak
    /// lagi memberi kewenangan CITO, dan id atau kode yang tidak dikenal diabaikan, bukan ditebak.
    /// </summary>
    public async Task<IReadOnlyCollection<Guid>> ResolveDesignatedPositionIdsAsync(
        CancellationToken cancellationToken = default)
    {
        if (!_emergencyDesignation.IsConfigured) return [];

        var ids = _emergencyDesignation.PositionIds.ToList();
        var codes = _emergencyDesignation.PositionCodes.Select(x => x.ToUpper()).ToList();

        return await dbContext.MstPositions.AsNoTracking()
            .Where(x => x.IsActive && !x.IsDelete &&
                        (ids.Contains(x.Id) || codes.Contains(x.PositionCode.ToUpper())))
            .Select(x => x.Id)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Akun aktif yang memegang izin <paramref name="action"/> secara eksplisit lewat penempatan
    /// jabatan yang berlaku — versi kumpulan dari <see cref="HasExplicitGrantAsync"/>, dengan
    /// penyaring yang sama. Dipakai laporan kesiapan supaya "siap" berarti atasan unit benar-benar
    /// memegang izin, bukan sekadar izin dan atasan sama-sama ada di tempat berbeda.
    /// </summary>
    public async Task<HashSet<Guid>> GetUsersWithExplicitGrantAsync(string resource, string action,
        CancellationToken cancellationToken = default)
    {
        var grantable = await FindGrantableActionAsync(resource, action, cancellationToken);
        if (grantable == null) return [];

        var userIds = await (
            from organization in GrantingPlacements(grantable, DateTime.UtcNow)
            join user in dbContext.Users.AsNoTracking() on organization.UserId equals user.Id
            where user.IsActive
            select organization.UserId
        ).Distinct().ToListAsync(cancellationToken);

        return [.. userIds];
    }

    /// <summary>
    /// Unit layanan yang dipimpin pengguna: unit yang ditautkan ke unit organisasi tempat pengguna
    /// memiliki penempatan manajerial aktif dan berlaku (<c>LGS-DEC-003</c>). Hanya satu tingkat;
    /// unit organisasi turunan tidak ikut. Kosong bila data organisasi belum diisi.
    /// </summary>
    public async Task<IReadOnlyList<StockRequestSupervisedUnit>> GetSupervisedServiceUnitsAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        var workforceProfileId = await dbContext.Users.AsNoTracking()
            .Where(x => x.Id == userId && x.IsActive)
            .Select(x => x.WorkforceProfileId)
            .FirstOrDefaultAsync(cancellationToken);

        if (!workforceProfileId.HasValue) return [];

        var workforceActive = await dbContext.MstWorkforceProfiles.AsNoTracking()
            .AnyAsync(x => x.Id == workforceProfileId.Value && x.IsActive && !x.IsDelete,
                cancellationToken);
        if (!workforceActive) return [];

        var now = DateTime.UtcNow;

        var organizationUnitIds = await dbContext.WfpOrganizationAssignments.AsNoTracking()
            .Where(x =>
                x.WorkforceProfileId == workforceProfileId.Value &&
                x.OrganizationUnitId != null &&
                x.IsManagerialAssignment &&
                x.IsActive &&
                !x.IsDelete &&
                !x.IsCancel &&
                x.EffectiveStartDate <= now &&
                (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= now))
            .Select(x => x.OrganizationUnitId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (organizationUnitIds.Count == 0) return [];

        var activeOrganizationUnits = await dbContext.MstOrganizationUnits.AsNoTracking()
            .Where(x =>
                organizationUnitIds.Contains(x.Id) &&
                x.IsActive &&
                !x.IsDelete &&
                (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value <= now) &&
                (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= now))
            .Select(x => new { x.Id, x.UnitName })
            .ToListAsync(cancellationToken);

        if (activeOrganizationUnits.Count == 0) return [];

        var activeIds = activeOrganizationUnits.Select(x => x.Id).ToList();
        var names = activeOrganizationUnits.ToDictionary(x => x.Id, x => x.UnitName);

        var units = await dbContext.MstServiceUnits.AsNoTracking()
            .Where(x => !x.IsDelete && x.OrganizationUnitId != null &&
                        activeIds.Contains(x.OrganizationUnitId.Value))
            .OrderBy(x => x.ServiceUnitName)
            .Select(x => new { x.Id, x.ServiceUnitCode, x.ServiceUnitName, OrganizationUnitId = x.OrganizationUnitId!.Value })
            .ToListAsync(cancellationToken);

        return units
            .Select(x => new StockRequestSupervisedUnit(x.Id, x.ServiceUnitCode, x.ServiceUnitName,
                x.OrganizationUnitId, names.GetValueOrDefault(x.OrganizationUnitId) ?? string.Empty))
            .ToList();
    }

    /// <summary>
    /// Izin <c>StockRequest:Approve</c> yang diberikan eksplisit lewat penempatan jabatan. Dipakai
    /// untuk setuju, tolak, dan verifikasi susulan CITO, supaya izin itu tetap diperiksa walaupun
    /// filter controller diloloskan SuperAdmin atau dimatikan saklar pengembangan.
    /// </summary>
    public Task<bool> HasApproveGrantAsync(Guid userId, CancellationToken cancellationToken = default) =>
        HasExplicitGrantAsync(userId, PermissionResource, ApprovePermissionAction, cancellationToken);
}

/// <summary>Sebab seorang pengguna tidak berwenang sebagai atasan unit.</summary>
public enum StockRequestUnitAuthorityReason
{
    None = 0,
    ServiceUnitNotFound = 1,
    ServiceUnitNotMapped = 2,
    OrganizationUnitInactive = 3,
    NoWorkforceProfile = 4,
    NotUnitSupervisor = 5,
    WorkforceProfileInactive = 6
}

/// <summary>Sebab seorang pengguna tidak berwenang mengotorisasi CITO.</summary>
public enum StockRequestEmergencyAuthorityReason
{
    None = 0,
    DesignationNotConfigured = 1,
    NotDesignatedOrNotGranted = 2
}

/// <summary>Hasil pemeriksaan kewenangan otorisasi CITO.</summary>
public sealed record StockRequestEmergencyAuthority(bool IsAuthorized,
    StockRequestEmergencyAuthorityReason Reason, string? Message);

/// <summary>Satu unit layanan yang dipimpin pengguna, beserta unit organisasi asalnya.</summary>
public sealed record StockRequestSupervisedUnit(Guid ServiceUnitId, string ServiceUnitCode,
    string ServiceUnitName, Guid OrganizationUnitId, string OrganizationUnitName);

/// <summary>Hasil pemeriksaan atasan unit beserta sebab penolakannya.</summary>
public sealed record StockRequestUnitAuthority(bool IsAuthorized,
    StockRequestUnitAuthorityReason Reason, string? Message)
{
    public static StockRequestUnitAuthority Granted() =>
        new(true, StockRequestUnitAuthorityReason.None, null);

    public static StockRequestUnitAuthority Denied(StockRequestUnitAuthorityReason reason,
        string message) => new(false, reason, message);
}
