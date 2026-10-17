using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Seeders;

/// <summary>
/// Seeder hak akses awal untuk peran Supervisor Kasir pada modul peninjauan void biaya administrasi
/// (BE-BKC-088, BKC-DEC-162, permission-audit-matrix revisi 2.3).
/// </summary>
/// <remarks>
/// Menjamin posisi Supervisor Kasir memiliki hak Read dan Decide pada resource BillingSupersededItemReview
/// begitu sistem dijalankan, tanpa menimpa konfigurasi manual yang disesuaikan administrator di masa mendatang.
/// </remarks>
public static class BillingSupersededItemReviewPermissionSeeder
{
    public const string ModuleCode = "HEALTH_SERVICE_BILLING_MANAGEMENT";
    public const string ResourceName = "BillingSupersededItemReview";
    public const string ReadActionName = "Read";
    public const string DecideActionName = "Decide";

    public static async Task SeedAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(BillingSupersededItemReviewPermissionSeeder));

        var seededCount = await SeedAsync(db, cancellationToken);
        if (seededCount.HasValue && seededCount.Value > 0)
        {
            logger.LogInformation(
                "Hak akses awal {Resource} (Read & Decide) berhasil diberikan ke {Count} kebijakan posisi Supervisor Kasir.",
                ResourceName, seededCount.Value);
        }
    }

    public static async Task<int?> SeedAsync(
        ApplicationDbContext db,
        CancellationToken cancellationToken = default)
    {
        // 1. Temukan ControllerAccess untuk BillingSupersededItemReview
        var controllerAccess = await db.SysControllerAccesses.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.ControllerName == ResourceName &&
                     db.SysApplicationModules.Any(m => m.Id == x.ModuleId && m.ModuleCode == ModuleCode),
                cancellationToken);

        if (controllerAccess == null)
        {
            return null;
        }

        // 2. Temukan ActionAccess untuk Read dan Decide
        var actions = await db.SysActionAccesses.AsNoTracking()
            .Where(x => x.ControllerAccessId == controllerAccess.Id &&
                        (x.ActionName == ReadActionName || x.ActionName == DecideActionName) &&
                        x.IsActive && !x.IsDelete)
            .ToListAsync(cancellationToken);

        if (actions.Count == 0)
        {
            return null;
        }

        // 3. Temukan posisi Supervisor Kasir
        var supervisorPositions = await db.MstPositions
            .Include(p => p.Department)
            .Where(p => !p.IsDelete && p.IsActive &&
                (p.PositionName.ToLower().Contains("supervisor kasir") ||
                 p.PositionCode.ToLower().Contains("spv-kasir") ||
                 (p.PositionName.ToLower().Contains("supervisor") &&
                  p.Department != null &&
                  (p.Department.DepartmentName.ToLower().Contains("kasir") ||
                   p.Department.DepartmentName.ToLower().Contains("billing")))))
            .Select(p => new { p.Id, p.DepartmentId })
            .ToListAsync(cancellationToken);

        if (supervisorPositions.Count == 0)
        {
            return null;
        }

        var now = DateTime.UtcNow;
        var addedPolicies = 0;

        foreach (var pos in supervisorPositions)
        {
            foreach (var action in actions)
            {
                var exists = await db.SysAccessPolicies.AsNoTracking()
                    .AnyAsync(
                        p => p.PositionId == pos.Id &&
                             p.DepartmentId == pos.DepartmentId &&
                             p.ActionAccessId == action.Id &&
                             !p.IsDelete,
                        cancellationToken);

                if (!exists)
                {
                    db.SysAccessPolicies.Add(new SysAccessPolicy
                    {
                        Id = Guid.NewGuid(),
                        DepartmentId = pos.DepartmentId,
                        PositionId = pos.Id,
                        ControllerAccessId = controllerAccess.Id,
                        ActionAccessId = action.Id,
                        IsAllowed = true,
                        IsActive = true,
                        CreateDateTime = now,
                        CreateBy = Guid.Empty,
                        IsDelete = false,
                        IsCancel = false
                    });
                    addedPolicies++;
                }
            }
        }

        if (addedPolicies > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
            return addedPolicies;
        }

        return 0;
    }
}
