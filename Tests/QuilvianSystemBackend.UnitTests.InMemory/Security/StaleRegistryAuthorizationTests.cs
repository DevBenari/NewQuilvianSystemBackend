using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Tests.Security;

/// <summary>
/// Keamanan registry usang dan kelayakan penempatan (Phase A0).
///
/// Registry yang sudah ditutup tidak boleh tetap mengotorisasi hanya karena baris
/// <c>SysAccessPolicy</c> lama masih menunjuknya. Rekonsiliasi generik menutup baris registry
/// tanpa menyentuh policy, jadi jalur otorisasi yang harus menolaknya.
/// </summary>
public sealed class StaleRegistryAuthorizationTests
{
    private const string Controller = "CashierShifts";
    private const string Action = "Close";

    private static ApplicationDbContext NewContext() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"stale-{Guid.NewGuid():N}")
            .Options);

    private static UserManager<ApplicationUser> CreateUserManager(ApplicationDbContext dbContext)
    {
        var services = new ServiceCollection();
        services.AddSingleton(dbContext);
        services.AddLogging();
        services.AddIdentityCore<ApplicationUser>()
                .AddRoles<ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

        return services.BuildServiceProvider().GetRequiredService<UserManager<ApplicationUser>>();
    }

    private static AccessPermissionService CreateService(ApplicationDbContext dbContext)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Security:Authorization:EnforceClinicalPolicyForSuperAdmin"] = "false"
            })
            .Build();

        return new AccessPermissionService(dbContext, CreateUserManager(dbContext), configuration);
    }

    private static async Task<ApplicationUser> SeedUserAsync(ApplicationDbContext dbContext)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = $"user-{Guid.NewGuid():N}",
            NormalizedUserName = $"USER-{Guid.NewGuid():N}",
            UserCode = "STALE-USER",
            DisplayName = "Stale Registry User",
            UserType = UserType.Employee,
            IsActive = true,
            SecurityStamp = Guid.NewGuid().ToString("N")
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return user;
    }

    private static async Task<(Guid DepartmentId, Guid PositionId)> GrantAsync(
        ApplicationDbContext dbContext,
        bool actionActive = true,
        bool actionDeleted = false,
        bool controllerActive = true,
        bool controllerDeleted = false)
    {
        var departmentId = Guid.NewGuid();
        var positionId = Guid.NewGuid();

        var controllerAccess = new SysControllerAccess
        {
            Id = Guid.NewGuid(),
            ModuleId = Guid.NewGuid(),
            ControllerName = Controller,
            DisplayName = Controller,
            IsActive = controllerActive,
            IsDelete = controllerDeleted
        };

        var actionAccess = new SysActionAccess
        {
            Id = Guid.NewGuid(),
            ControllerAccessId = controllerAccess.Id,
            ActionName = Action,
            DisplayName = Action,
            IsActive = actionActive,
            IsDelete = actionDeleted
        };

        dbContext.SysControllerAccesses.Add(controllerAccess);
        dbContext.SysActionAccesses.Add(actionAccess);

        // Policy sengaja tetap aktif — inilah kondisi setelah rekonsiliasi menutup registry.
        dbContext.SysAccessPolicies.Add(new SysAccessPolicy
        {
            Id = Guid.NewGuid(),
            DepartmentId = departmentId,
            PositionId = positionId,
            ControllerAccessId = controllerAccess.Id,
            ActionAccessId = actionAccess.Id,
            IsAllowed = true,
            IsActive = true,
            IsDelete = false
        });

        await dbContext.SaveChangesAsync();
        return (departmentId, positionId);
    }

    private static async Task AssignAsync(
        ApplicationDbContext dbContext,
        ApplicationUser user,
        Guid departmentId,
        Guid positionId,
        bool isActive = true,
        bool isCancel = false,
        bool isDelete = false,
        bool isPrimary = true)
    {
        dbContext.ApplicationUserOrganizations.Add(new ApplicationUserOrganization
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            DepartmentId = departmentId,
            PositionId = positionId,
            IsPrimary = isPrimary,
            IsActive = isActive,
            IsCancel = isCancel,
            IsDelete = isDelete,
            EffectiveStartDate = DateTime.UtcNow.AddDays(-10),
            EffectiveEndDate = null
        });

        await dbContext.SaveChangesAsync();
    }

    private static ClaimsPrincipal Principal(Guid userId) =>
        new(new ClaimsIdentity(new[] { new Claim("user_id", userId.ToString()) }, "TestAuth"));

    /// <summary>Baseline: kondisi sehat memang mengizinkan, supaya test lain bermakna.</summary>
    [Fact]
    public async Task ActiveRegistryWithPolicyGrantsAccess()
    {
        await using var db = NewContext();
        var user = await SeedUserAsync(db);
        var (departmentId, positionId) = await GrantAsync(db);
        await AssignAsync(db, user, departmentId, positionId);

        Assert.True(await CreateService(db).HasAccessAsync(Principal(user.Id), Controller, Action));
    }

    /// <summary>Registry action yang dinonaktifkan tidak boleh tetap mengotorisasi.</summary>
    [Fact]
    public async Task InactiveActionRegistryDeniesEvenWhenPolicyStillExists()
    {
        await using var db = NewContext();
        var user = await SeedUserAsync(db);
        var (departmentId, positionId) = await GrantAsync(db, actionActive: false);
        await AssignAsync(db, user, departmentId, positionId);

        Assert.False(await CreateService(db).HasAccessAsync(Principal(user.Id), Controller, Action));
    }

    /// <summary>Registry action yang ditutup rekonsiliasi tidak boleh tetap mengotorisasi.</summary>
    [Fact]
    public async Task DeletedActionRegistryDeniesEvenWhenPolicyStillExists()
    {
        await using var db = NewContext();
        var user = await SeedUserAsync(db);
        var (departmentId, positionId) = await GrantAsync(db, actionDeleted: true);
        await AssignAsync(db, user, departmentId, positionId);

        Assert.False(await CreateService(db).HasAccessAsync(Principal(user.Id), Controller, Action));
    }

    [Fact]
    public async Task ClosedControllerRegistryDeniesEvenWhenPolicyStillExists()
    {
        await using var db = NewContext();
        var user = await SeedUserAsync(db);
        var (departmentId, positionId) = await GrantAsync(db, controllerActive: false, controllerDeleted: true);
        await AssignAsync(db, user, departmentId, positionId);

        Assert.False(await CreateService(db).HasAccessAsync(Principal(user.Id), Controller, Action));
    }

    /// <summary>
    /// Test 15: penempatan yang dibatalkan tidak memberi izin. Sebelum Phase A0
    /// <c>IsCancel</c> tidak diperiksa sama sekali di jalur otorisasi.
    /// </summary>
    [Fact]
    public async Task CancelledOrganizationAssignmentDeniesAccess()
    {
        await using var db = NewContext();
        var user = await SeedUserAsync(db);
        var (departmentId, positionId) = await GrantAsync(db);
        await AssignAsync(db, user, departmentId, positionId, isCancel: true);

        Assert.False(await CreateService(db).HasAccessAsync(Principal(user.Id), Controller, Action));
    }

    /// <summary>
    /// Test 14: penempatan sekunder yang sah tetap memberi izin. <c>IsPrimary</c> hanyalah
    /// penanda, bukan syarat kelayakan.
    /// </summary>
    [Fact]
    public async Task NonPrimaryAssignmentStillGrantsAccess()
    {
        await using var db = NewContext();
        var user = await SeedUserAsync(db);
        var (departmentId, positionId) = await GrantAsync(db);
        await AssignAsync(db, user, departmentId, positionId, isPrimary: false);

        Assert.True(await CreateService(db).HasAccessAsync(Principal(user.Id), Controller, Action));
    }

    /// <summary>
    /// Test 7: izin efektif adalah gabungan seluruh penempatan aktif. Penempatan kedua memberi
    /// kemampuan yang tidak dimiliki penempatan pertama.
    /// </summary>
    [Fact]
    public async Task EffectivePermissionsAreUnionOfActiveAssignments()
    {
        await using var db = NewContext();
        var user = await SeedUserAsync(db);

        var first = await GrantAsync(db);
        await AssignAsync(db, user, first.DepartmentId, first.PositionId, isPrimary: true);

        // Kemampuan kedua di controller berbeda, hanya dimiliki penempatan kedua.
        var secondDepartment = Guid.NewGuid();
        var secondPosition = Guid.NewGuid();
        var otherController = new SysControllerAccess
        {
            Id = Guid.NewGuid(),
            ModuleId = Guid.NewGuid(),
            ControllerName = "LabSpecimen",
            DisplayName = "LabSpecimen",
            IsActive = true
        };
        var otherAction = new SysActionAccess
        {
            Id = Guid.NewGuid(),
            ControllerAccessId = otherController.Id,
            ActionName = "Accept",
            DisplayName = "Accept",
            IsActive = true
        };
        db.SysControllerAccesses.Add(otherController);
        db.SysActionAccesses.Add(otherAction);
        db.SysAccessPolicies.Add(new SysAccessPolicy
        {
            Id = Guid.NewGuid(),
            DepartmentId = secondDepartment,
            PositionId = secondPosition,
            ControllerAccessId = otherController.Id,
            ActionAccessId = otherAction.Id,
            IsAllowed = true,
            IsActive = true
        });
        await db.SaveChangesAsync();
        await AssignAsync(db, user, secondDepartment, secondPosition, isPrimary: false);

        var service = CreateService(db);

        Assert.True(await service.HasAccessAsync(Principal(user.Id), Controller, Action));
        Assert.True(await service.HasAccessAsync(Principal(user.Id), "LabSpecimen", "Accept"));
    }
}
