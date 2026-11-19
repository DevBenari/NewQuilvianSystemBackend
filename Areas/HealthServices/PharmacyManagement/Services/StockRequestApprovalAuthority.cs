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
/// <b>Otorisasi CITO</b> menuntut izin <c>StockRequest:AuthorizeEmergency</c> yang benar-benar
/// diberikan lewat penempatan jabatan (<c>ApplicationUserOrganization</c> ×
/// <c>SysAccessPolicy</c>), dengan penyaring yang sama dengan jalur kebijakan pada
/// <c>AccessPermissionService.HasAccessAsync</c> tetapi tanpa jalan pintas SuperAdmin, tanpa
/// jalan pintas aksi system-only, dan tanpa saklar pengembangan. Bila jalur kebijakan di sana
/// berubah, penyaring di sini wajib ditinjau ulang.
/// </para>
/// </remarks>
public sealed class StockRequestApprovalAuthority(ApplicationDbContext dbContext)
{
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
    public async Task<bool> HasExplicitGrantAsync(Guid userId, string resource, string action,
        CancellationToken cancellationToken = default)
    {
        var userActive = await dbContext.Users.AsNoTracking()
            .AnyAsync(x => x.Id == userId && x.IsActive, cancellationToken);
        if (!userActive) return false;

        var actionAccess = await dbContext.SysActionAccesses.AsNoTracking()
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
            .Select(x => new { ActionAccessId = x.Id, x.ControllerAccessId })
            .FirstOrDefaultAsync(cancellationToken);

        if (actionAccess == null) return false;

        var now = DateTime.UtcNow;

        return await (
            from organization in dbContext.ApplicationUserOrganizations.AsNoTracking()
            join policy in dbContext.SysAccessPolicies.AsNoTracking()
                on new { organization.DepartmentId, organization.PositionId }
                equals new { policy.DepartmentId, policy.PositionId }
            where organization.UserId == userId
                  && organization.IsActive
                  && !organization.IsDelete
                  && !organization.IsCancel
                  && (!organization.EffectiveStartDate.HasValue ||
                      organization.EffectiveStartDate.Value <= now)
                  && (!organization.EffectiveEndDate.HasValue ||
                      organization.EffectiveEndDate.Value >= now)
                  && policy.ControllerAccessId == actionAccess.ControllerAccessId
                  && policy.ActionAccessId == actionAccess.ActionAccessId
                  && policy.IsAllowed
                  && policy.IsActive
                  && !policy.IsDelete
            select policy.Id
        ).AnyAsync(cancellationToken);
    }

    /// <summary>Izin otorisasi CITO yang diberikan eksplisit lewat penempatan jabatan.</summary>
    public Task<bool> HasEmergencyGrantAsync(Guid userId, CancellationToken cancellationToken = default) =>
        HasExplicitGrantAsync(userId, PermissionResource, EmergencyPermissionAction, cancellationToken);

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

/// <summary>Hasil pemeriksaan atasan unit beserta sebab penolakannya.</summary>
public sealed record StockRequestUnitAuthority(bool IsAuthorized,
    StockRequestUnitAuthorityReason Reason, string? Message)
{
    public static StockRequestUnitAuthority Granted() =>
        new(true, StockRequestUnitAuthorityReason.None, null);

    public static StockRequestUnitAuthority Denied(StockRequestUnitAuthorityReason reason,
        string message) => new(false, reason, message);
}
