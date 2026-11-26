using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;

/// <summary>
/// Kewenangan memutus permintaan stok yang diperiksa di dalam domain, terpisah dari filter izin
/// di controller (<c>FIN-LGS-070</c>).
/// </summary>
/// <remarks>
/// <para>
/// <b>Sumbernya hanya akun yang login dan izin aktifnya.</b> Sejak FIN-LGS-070 persetujuan tidak
/// lagi membaca struktur organisasi HRD (tautan unit layanan ke unit organisasi maupun penempatan
/// manajerial), dan otorisasi CITO tidak lagi membutuhkan daftar jabatan yang ditunjuk. Izin yang
/// sudah ada menjadi satu-satunya penentu:
/// </para>
/// <list type="bullet">
/// <item><c>StockRequest:Approve</c> — setuju, tolak, dan verifikasi susulan CITO atas permintaan
/// unit mana pun;</item>
/// <item><c>StockRequest:AuthorizeEmergency</c> — otorisasi jalur CITO.</item>
/// </list>
/// <para>
/// <b>Mengapa diperiksa lagi di domain.</b> Filter <c>[AccessPermission]</c> mengikuti kebijakan
/// platform: peran SuperAdmin lolos selama
/// <c>Security:Authorization:EnforceClinicalPolicyForSuperAdmin</c> bernilai false, dan seluruh
/// pemeriksaan dilepas bila <c>Security:Authorization:Enabled</c> bernilai false di luar
/// Production. Kebijakan itu tidak diubah. Di sini izin harus benar-benar diberikan lewat Akses
/// Role: penempatan akses pengguna (<c>ApplicationUserOrganization</c>) × kebijakan akses
/// (<c>SysAccessPolicy</c>). Penyaringnya sama dengan jalur kebijakan pada
/// <c>AccessPermissionService.HasAccessAsync</c>, tetapi tanpa jalan pintas SuperAdmin, tanpa jalan
/// pintas aksi system-only, dan tanpa saklar pengembangan. Bila jalur kebijakan di sana berubah,
/// penyaring di sini wajib ditinjau ulang.
/// </para>
/// </remarks>
public sealed class StockRequestApprovalAuthority(ApplicationDbContext dbContext)
{
    public const string PermissionResource = "StockRequest";
    public const string ApprovePermissionAction = "Approve";
    public const string EmergencyPermissionAction = "AuthorizeEmergency";

    /// <summary>
    /// Benar bila izin <paramref name="resource"/> : <paramref name="action"/> diberikan
    /// secara eksplisit kepada salah satu penempatan akses pengguna yang masih berlaku.
    /// </summary>
    /// <remarks>
    /// Status SuperAdmin dan saklar pengembangan tidak dipertimbangkan sama sekali. SuperAdmin
    /// yang memang bertugas memutus tetap dapat melakukannya, asalkan penempatan aksesnya diberi
    /// izin itu seperti petugas lain.
    /// </remarks>
    /// <param name="userId">Akun yang diperiksa; akun nonaktif selalu tidak berizin.</param>
    /// <param name="resource">Nama controller izin, mis. <c>StockRequest</c>.</param>
    /// <param name="action">Nama aksi izin, mis. <c>Approve</c>.</param>
    /// <param name="cancellationToken">Pembatalan operasi.</param>
    public async Task<bool> HasExplicitGrantAsync(Guid userId, string resource, string action,
        CancellationToken cancellationToken = default)
    {
        var userActive = await dbContext.Users.AsNoTracking()
            .AnyAsync(x => x.Id == userId && x.IsActive, cancellationToken);
        if (!userActive) return false;

        var grantable = await FindGrantableActionAsync(resource, action, cancellationToken);
        if (grantable == null) return false;

        return await GrantingPlacements(grantable, DateTime.UtcNow)
            .Where(x => x.UserId == userId)
            .AnyAsync(cancellationToken);
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
    /// Penempatan akses yang aktif, sedang berlaku, dan diberi aksi <paramref name="grantable"/>
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
    /// Jumlah kebijakan akses aktif yang memberi aksi itu. Aksinya disaring sama dengan pemeriksaan
    /// izin (<see cref="FindGrantableActionAsync"/>), sehingga izin pada controller atau aksi yang
    /// nonaktif tidak ikut terhitung di laporan kesiapan.
    /// </summary>
    public async Task<int> CountPolicyGrantsAsync(string resource, string action,
        CancellationToken cancellationToken = default)
    {
        var grantable = await FindGrantableActionAsync(resource, action, cancellationToken);
        if (grantable == null) return 0;

        var controllerAccessId = grantable.ControllerAccessId;
        var actionAccessId = grantable.ActionAccessId;
        return await dbContext.SysAccessPolicies.AsNoTracking()
            .Where(x => x.ControllerAccessId == controllerAccessId &&
                        x.ActionAccessId == actionAccessId &&
                        x.IsAllowed && x.IsActive && !x.IsDelete)
            .CountAsync(cancellationToken);
    }

    /// <summary>
    /// Kewenangan otorisasi CITO: izin <c>StockRequest:AuthorizeEmergency</c> yang diberikan
    /// eksplisit lewat Akses Role. Tidak ada syarat jabatan atau penempatan HRD.
    /// </summary>
    public async Task<StockRequestEmergencyAuthority> CheckEmergencyAuthorityAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        var granted = await HasExplicitGrantAsync(userId, PermissionResource,
            EmergencyPermissionAction, cancellationToken);

        return granted
            ? new(true, StockRequestEmergencyAuthorityReason.None, null)
            : new(false, StockRequestEmergencyAuthorityReason.NotGranted,
                "Otorisasi CITO hanya dapat dilakukan petugas yang diberi izin " +
                "StockRequest:AuthorizeEmergency secara eksplisit lewat Akses Role. " +
                "Peran SuperAdmin saja tidak cukup.");
    }

    /// <summary>Kewenangan otorisasi CITO sebagai nilai benar/salah.</summary>
    public async Task<bool> HasEmergencyGrantAsync(Guid userId, CancellationToken cancellationToken = default) =>
        (await CheckEmergencyAuthorityAsync(userId, cancellationToken)).IsAuthorized;

    /// <summary>
    /// Akun aktif yang memegang izin <paramref name="action"/> secara eksplisit lewat penempatan
    /// akses yang berlaku — versi kumpulan dari <see cref="HasExplicitGrantAsync"/>, dengan
    /// penyaring yang sama. Dipakai laporan kesiapan supaya "siap" berarti ada akun yang benar-benar
    /// dapat memutus, bukan sekadar kebijakan akses yang tercatat.
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
    /// Izin <c>StockRequest:Approve</c> yang diberikan eksplisit lewat Akses Role. Dipakai untuk
    /// setuju, tolak, dan verifikasi susulan CITO, supaya izin itu tetap diperiksa walaupun filter
    /// controller diloloskan SuperAdmin atau dimatikan saklar pengembangan.
    /// </summary>
    public Task<bool> HasApproveGrantAsync(Guid userId, CancellationToken cancellationToken = default) =>
        HasExplicitGrantAsync(userId, PermissionResource, ApprovePermissionAction, cancellationToken);
}

/// <summary>Sebab seorang pengguna tidak berwenang mengotorisasi CITO.</summary>
public enum StockRequestEmergencyAuthorityReason
{
    None = 0,
    NotGranted = 1
}

/// <summary>Hasil pemeriksaan kewenangan otorisasi CITO.</summary>
public sealed record StockRequestEmergencyAuthority(bool IsAuthorized,
    StockRequestEmergencyAuthorityReason Reason, string? Message);
