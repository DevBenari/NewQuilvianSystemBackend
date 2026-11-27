using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Seeders;

/// <summary>
/// Pemberian bawaan <c>LabOrder : Reschedule</c> (<c>BE-LAB-102</c>, <c>LAB-DEC-274</c>,
/// <c>LAB-PERM-v1</c> revision 18 bagian 20.4): setiap peran (departemen + jabatan) yang memegang
/// kebijakan aktif <c>LabOrder : Hold</c> memperoleh <c>LabOrder : Reschedule</c> — petugas yang
/// sudah boleh menahan pesanan boleh memindahkan tanggal rencananya.
/// </summary>
/// <remarks>
/// <para>
/// <b>Sekali jalan.</b> Hanya menyalin bila <c>Reschedule</c> belum pernah punya baris kebijakan sama
/// sekali (termasuk yang sudah dicabut). Sesudah itu admin yang memegang kendali: mencabut
/// <c>Reschedule</c> dari satu peran tidak dipulihkan pada startup berikutnya. Kebijakan
/// <c>Hold</c> tidak disentuh.
/// </para>
/// <para>
/// <i>Kepala Instalasi Laboratorium</i> <b>tidak</b> diberi dari sini — langkah rilis lewat Role
/// Access (6av.4 no. 3). Pola <c>OperatingRoomHandoverPermissionSeeder</c>. Wajib dijalankan
/// sesudah <c>AccessMenuSeeder</c>, karena baris kemampuan <c>Reschedule</c> lahir di sana.
/// </para>
/// </remarks>
public static class LabOrderReschedulePermissionSeeder
{
    public const string ModuleCode = "HEALTH_SERVICE_LABORATORY_MANAGEMENT";
    public const string ResourceName = "LabOrder";
    public const string SourceActionName = "Hold";
    public const string RescheduleActionName = "Reschedule";

    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(LabOrderReschedulePermissionSeeder));

        var copied = await SeedAsync(db, cancellationToken);
        if (copied.HasValue)
            logger.LogInformation(
                "Salinan hak {Resource} : {Source} → {Reschedule}: {Count} kebijakan peran ditambahkan.",
                ResourceName, SourceActionName, RescheduleActionName, copied.Value);
    }

    /// <returns>Jumlah kebijakan yang ditambahkan; <c>null</c> bila tidak ada yang perlu dilakukan.</returns>
    public static async Task<int?> SeedAsync(ApplicationDbContext db, CancellationToken cancellationToken = default)
    {
        var controllerAccessId = await db.SysControllerAccesses.AsNoTracking()
            .Where(x => x.ControllerName == ResourceName &&
                db.SysApplicationModules.Any(m => m.Id == x.ModuleId && m.ModuleCode == ModuleCode))
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (!controllerAccessId.HasValue) return null;

        var actions = await db.SysActionAccesses.AsNoTracking()
            .Where(x => x.ControllerAccessId == controllerAccessId.Value &&
                (x.ActionName == SourceActionName || x.ActionName == RescheduleActionName))
            .Select(x => new { x.Id, x.ActionName, x.IsActive, x.IsDelete })
            .ToListAsync(cancellationToken);

        var source = actions.FirstOrDefault(x => x.ActionName == SourceActionName);
        var reschedule = actions.FirstOrDefault(x =>
            x.ActionName == RescheduleActionName && x.IsActive && !x.IsDelete);
        if (source == null || reschedule == null) return null;

        // Sekali jalan: begitu Reschedule pernah punya kebijakan — aktif maupun dicabut — keputusan
        // berikutnya milik admin.
        var rescheduleHasPolicy = await db.SysAccessPolicies.AsNoTracking()
            .AnyAsync(x => x.ActionAccessId == reschedule.Id, cancellationToken);
        if (rescheduleHasPolicy) return null;

        var holdGrants = await db.SysAccessPolicies.AsNoTracking()
            .Where(x => x.ActionAccessId == source.Id && x.IsAllowed && x.IsActive && !x.IsDelete)
            .Select(x => new { x.DepartmentId, x.PositionId })
            .Distinct()
            .ToListAsync(cancellationToken);
        if (holdGrants.Count == 0) return null;

        var now = DateTime.UtcNow;
        foreach (var grant in holdGrants)
        {
            db.SysAccessPolicies.Add(new SysAccessPolicy
            {
                Id = Guid.NewGuid(),
                DepartmentId = grant.DepartmentId,
                PositionId = grant.PositionId,
                ControllerAccessId = controllerAccessId.Value,
                ActionAccessId = reschedule.Id,
                IsAllowed = true,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = Guid.Empty
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        return holdGrants.Count;
    }
}
