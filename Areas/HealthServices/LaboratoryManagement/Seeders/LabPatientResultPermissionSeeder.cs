using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Seeders;

/// <summary>
/// Pemberian bawaan <c>LabPatientResult : Send</c> dan <c>LabPatientResult : Print</c>
/// (<c>BE-LAB-104</c>, <c>LAB-PERM-v1</c> revision 19 bagian 21.4, <c>02-backend-architecture.md</c>
/// 34.10 no. 8): setiap peran (departemen + jabatan) yang memegang kebijakan aktif
/// <c>LabOrder : Hold</c> — di devYoga hanya Analis Laboratorium — memperoleh kedua aksi itu.
/// </summary>
/// <remarks>
/// <para>
/// <b>Sekali jalan per aksi.</b> Satu aksi disalin hanya bila ia belum pernah punya baris kebijakan
/// sama sekali (termasuk yang sudah dicabut); sesudah itu admin yang memegang kendali.
/// <c>LabPatientResult : Approve</c> <b>tidak pernah</b> diberikan dari sini — penyetuju ditunjuk
/// Kepala Instalasi (<c>LAB-DEC-182</c>, <c>297</c>).
/// </para>
/// <para>
/// Aksi <c>Send</c>/<c>Print</c> baru terdaftar oleh <c>AccessMenuSeeder</c> ketika endpointnya ada
/// (<c>BE-LAB-106</c>, <c>BE-LAB-41</c>). Sebelum itu seeder ini tidak melakukan apa pun, lalu menyalin
/// pada startup pertama sesudah aksinya lahir. Wajib dijalankan sesudah <c>AccessMenuSeeder</c>.
/// Pola <see cref="LabOrderReschedulePermissionSeeder"/>.
/// </para>
/// </remarks>
public static class LabPatientResultPermissionSeeder
{
    public const string ModuleCode = "HEALTH_SERVICE_LABORATORY_MANAGEMENT";
    public const string SourceResourceName = "LabOrder";
    public const string SourceActionName = "Hold";
    public const string TargetResourceName = "LabPatientResult";
    public static readonly string[] TargetActionNames = { "Send", "Print" };

    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(LabPatientResultPermissionSeeder));

        var copied = await SeedAsync(db, cancellationToken);
        foreach (var (action, count) in copied)
            logger.LogInformation(
                "Salinan hak {SourceResource} : {Source} → {TargetResource} : {Target}: {Count} kebijakan peran ditambahkan.",
                SourceResourceName, SourceActionName, TargetResourceName, action, count);
    }

    /// <returns>Aksi yang disalin beserta jumlah kebijakan yang ditambahkan; kosong bila tidak ada.</returns>
    public static async Task<IReadOnlyDictionary<string, int>> SeedAsync(
        ApplicationDbContext db,
        CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string, int>();

        var sourceActionId = await FindActionIdAsync(db, SourceResourceName, SourceActionName, requireActive: false, cancellationToken);
        if (sourceActionId is null) return result;

        var targetControllerId = await FindControllerIdAsync(db, TargetResourceName, cancellationToken);
        if (targetControllerId is null) return result;

        var holdGrants = await db.SysAccessPolicies.AsNoTracking()
            .Where(x => x.ActionAccessId == sourceActionId.Value && x.IsAllowed && x.IsActive && !x.IsDelete)
            .Select(x => new { x.DepartmentId, x.PositionId })
            .Distinct()
            .ToListAsync(cancellationToken);
        if (holdGrants.Count == 0) return result;

        var now = DateTime.UtcNow;
        foreach (var actionName in TargetActionNames)
        {
            var targetActionId = await FindActionIdAsync(db, TargetResourceName, actionName, requireActive: true, cancellationToken);
            if (targetActionId is null) continue;

            // Sekali jalan: begitu aksi ini pernah punya kebijakan — aktif maupun dicabut — keputusan
            // berikutnya milik admin.
            var hasPolicy = await db.SysAccessPolicies.AsNoTracking()
                .AnyAsync(x => x.ActionAccessId == targetActionId.Value, cancellationToken);
            if (hasPolicy) continue;

            foreach (var grant in holdGrants)
            {
                db.SysAccessPolicies.Add(new SysAccessPolicy
                {
                    Id = Guid.NewGuid(),
                    DepartmentId = grant.DepartmentId,
                    PositionId = grant.PositionId,
                    ControllerAccessId = targetControllerId.Value,
                    ActionAccessId = targetActionId.Value,
                    IsAllowed = true,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = Guid.Empty
                });
            }

            result[actionName] = holdGrants.Count;
        }

        if (result.Count > 0)
            await db.SaveChangesAsync(cancellationToken);

        return result;
    }

    private static async Task<Guid?> FindControllerIdAsync(
        ApplicationDbContext db,
        string controllerName,
        CancellationToken cancellationToken) =>
        await db.SysControllerAccesses.AsNoTracking()
            .Where(x => x.ControllerName == controllerName &&
                db.SysApplicationModules.Any(m => m.Id == x.ModuleId && m.ModuleCode == ModuleCode))
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

    private static async Task<Guid?> FindActionIdAsync(
        ApplicationDbContext db,
        string controllerName,
        string actionName,
        bool requireActive,
        CancellationToken cancellationToken)
    {
        var controllerId = await FindControllerIdAsync(db, controllerName, cancellationToken);
        if (controllerId is null) return null;

        return await db.SysActionAccesses.AsNoTracking()
            .Where(x => x.ControllerAccessId == controllerId.Value && x.ActionName == actionName &&
                (!requireActive || (x.IsActive && !x.IsDelete)))
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
