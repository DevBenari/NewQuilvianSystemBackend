using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Seeders;

/// <summary>
/// Data awal <c>E8</c> bagian hak akses (<c>BE-RWI-177</c>, <c>02-backend-architecture.md</c> 12.11,
/// 12.12): pemberian hak lama <c>OperatingRoomHandover : Update</c> disalin menjadi
/// <c>OperatingRoomHandover : Send</c> pada peran (departemen + posisi) yang sama.
/// </summary>
/// <remarks>
/// <para>
/// <b>Hak <c>: Receive</c> sengaja TIDAK disalin.</b> Dulu satu hak <c>Update</c> dipakai untuk
/// mengirim sekaligus menerima. Menyalinnya ke <c>Receive</c> akan membuat perawat OK tetap dapat
/// menerima serah terima kirimannya sendiri — persis yang ditutup <c>RWI-DEC-189</c>. Hak terima
/// diberikan admin hak akses pada peran perawat unit rawat inap dan ICU.
/// </para>
/// <para>
/// <b>Sekali jalan.</b> Seeder hanya menyalin bila kemampuan <c>Send</c> belum pernah punya baris
/// kebijakan sama sekali (termasuk yang sudah dicabut). Sesudah itu admin yang memegang kendali:
/// mencabut <c>Send</c> dari satu peran tidak akan dibatalkan seeder pada startup berikutnya.
/// Baris kebijakan <c>Update</c> lama tidak disentuh; kemampuannya sendiri ditutup
/// <c>AccessMenuSeeder</c> karena tidak lagi dideklarasikan endpoint mana pun.
/// </para>
/// <para>
/// Wajib dijalankan sesudah <c>AccessMenuSeeder</c>, karena baris kemampuan <c>Send</c> lahir di sana.
/// </para>
/// </remarks>
public static class OperatingRoomHandoverPermissionSeeder
{
    public const string ModuleCode = "HEALTH_SERVICE_OPERATING_ROOM_MANAGEMENT";
    public const string ResourceName = "OperatingRoomHandover";
    public const string LegacyActionName = "Update";
    public const string SendActionName = "Send";

    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(OperatingRoomHandoverPermissionSeeder));

        var copied = await SeedAsync(db, cancellationToken);
        if (copied.HasValue)
            logger.LogInformation(
                "Salinan hak {Resource} : {Legacy} → {Send}: {Count} kebijakan peran ditambahkan. Hak Receive tidak disalin.",
                ResourceName, LegacyActionName, SendActionName, copied.Value);
    }

    /// <returns>Jumlah kebijakan yang disalin; <c>null</c> bila tidak ada yang perlu dilakukan.</returns>
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
                (x.ActionName == LegacyActionName || x.ActionName == SendActionName))
            .Select(x => new { x.Id, x.ActionName, x.IsActive, x.IsDelete })
            .ToListAsync(cancellationToken);

        var legacy = actions.FirstOrDefault(x => x.ActionName == LegacyActionName);
        var send = actions.FirstOrDefault(x => x.ActionName == SendActionName && x.IsActive && !x.IsDelete);
        if (legacy == null || send == null) return null;

        // Sekali jalan: begitu Send pernah punya kebijakan, keputusan berikutnya milik admin.
        var sendHasPolicy = await db.SysAccessPolicies.AsNoTracking()
            .AnyAsync(x => x.ActionAccessId == send.Id, cancellationToken);
        if (sendHasPolicy) return null;

        var legacyGrants = await db.SysAccessPolicies.AsNoTracking()
            .Where(x => x.ActionAccessId == legacy.Id && x.IsAllowed && x.IsActive && !x.IsDelete)
            .Select(x => new { x.DepartmentId, x.PositionId })
            .Distinct()
            .ToListAsync(cancellationToken);
        if (legacyGrants.Count == 0) return null;

        var now = DateTime.UtcNow;
        foreach (var grant in legacyGrants)
        {
            db.SysAccessPolicies.Add(new SysAccessPolicy
            {
                Id = Guid.NewGuid(),
                DepartmentId = grant.DepartmentId,
                PositionId = grant.PositionId,
                ControllerAccessId = controllerAccessId.Value,
                ActionAccessId = send.Id,
                IsAllowed = true,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = Guid.Empty
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        return legacyGrants.Count;
    }
}
