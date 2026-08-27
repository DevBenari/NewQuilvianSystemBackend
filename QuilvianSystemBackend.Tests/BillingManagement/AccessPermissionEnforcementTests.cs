using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Filters;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Tests.BillingManagement;

// BE-BKC-017 / BIL-AT-022: RBAC diuji end-to-end lewat AccessPermissionService/AccessPermissionFilter
// SUNGGUHAN (bukan reflection atas atribut [AccessPermission]) - lihat readiness audit 26 Agustus 2026
// yang menemukan seluruh "test permission" sebelumnya hanya mengecek argumen atribut, tidak pernah
// memanggil jalur otorisasi dengan principal ber-role salah. UserManager/RoleManager di sini adalah
// instance ASP.NET Core Identity SUNGGUHAN yang didukung DbContext InMemory yang sama, bukan mock -
// tidak menambah dependency baru karena Identity sudah direferensikan lewat project utama.
public sealed class AccessPermissionEnforcementTests
{
    private const string BillingWriteOffController = "BillingWriteOff";
    private const string ApproveAction = "Approve";
    private const string CashierShiftController = "CashierShift";
    private const string ReopenAction = "Reopen";
    private const string BillingInvoiceController = "BillingInvoice";
    private const string UpdateAction = "Update";

    private static UserManager<ApplicationUser> CreateUserManager(ApplicationDbContext dbContext)
    {
        var services = new ServiceCollection();
        services.AddSingleton(dbContext);
        services.AddLogging();
        services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        return services.BuildServiceProvider().GetRequiredService<UserManager<ApplicationUser>>();
    }

    private static RoleManager<ApplicationRole> CreateRoleManager(ApplicationDbContext dbContext)
    {
        var services = new ServiceCollection();
        services.AddSingleton(dbContext);
        services.AddLogging();
        services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        return services.BuildServiceProvider().GetRequiredService<RoleManager<ApplicationRole>>();
    }

    private static AccessPermissionService CreateService(
        ApplicationDbContext dbContext,
        bool enforceClinicalPolicyForSuperAdmin = false)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Security:Authorization:EnforceClinicalPolicyForSuperAdmin"] =
                    enforceClinicalPolicyForSuperAdmin ? "true" : "false",
            })
            .Build();

        return new AccessPermissionService(dbContext, CreateUserManager(dbContext), configuration);
    }

    private static LoggerService CreateLogger() =>
        new(NullLogger<LoggerService>.Instance, new HttpContextAccessor());

    private static async Task<ApplicationUser> SeedUserAsync(
        ApplicationDbContext dbContext,
        UserType userType = UserType.Employee,
        bool isActive = true)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = $"user-{Guid.NewGuid():N}",
            NormalizedUserName = $"USER-{Guid.NewGuid():N}",
            UserCode = "TEST-USER",
            DisplayName = "Test User",
            UserType = userType,
            IsActive = isActive,
            // UserManager.AddToRoleAsync memvalidasi SecurityStamp; wajib diisi karena user di sini
            // di-seed langsung lewat DbContext, bukan lewat UserManager.CreateAsync.
            SecurityStamp = Guid.NewGuid().ToString("N"),
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return user;
    }

    // Menyiapkan satu policy akses lengkap (controller+action+department/position+kebijakan) dan,
    // opsional, menugaskan user ke department/position tersebut - meniru struktur nyata
    // SysControllerAccess/SysActionAccess/SysAccessPolicy/AspNetUserOrganization tanpa bergantung
    // pada proses seeding produksi manapun (yang tidak relevan untuk membuktikan resolusi izin).
    private static async Task<(Guid DepartmentId, Guid PositionId)> GrantPolicyAsync(
        ApplicationDbContext dbContext,
        string controllerName,
        string actionName,
        bool isAllowed = true,
        bool controllerActive = true,
        bool actionActive = true,
        bool actionSystemOnly = false,
        bool controllerSystemOnly = false)
    {
        var departmentId = Guid.NewGuid();
        var positionId = Guid.NewGuid();

        var controllerAccess = new SysControllerAccess
        {
            Id = Guid.NewGuid(),
            ModuleId = Guid.NewGuid(),
            ControllerName = controllerName,
            DisplayName = controllerName,
            IsActive = controllerActive,
            IsSystemOnly = controllerSystemOnly,
        };
        var actionAccess = new SysActionAccess
        {
            Id = Guid.NewGuid(),
            ControllerAccessId = controllerAccess.Id,
            ActionName = actionName,
            DisplayName = actionName,
            IsActive = actionActive,
            IsSystemOnly = actionSystemOnly,
        };
        dbContext.SysControllerAccesses.Add(controllerAccess);
        dbContext.SysActionAccesses.Add(actionAccess);
        dbContext.SysAccessPolicies.Add(new SysAccessPolicy
        {
            Id = Guid.NewGuid(),
            DepartmentId = departmentId,
            PositionId = positionId,
            ControllerAccessId = controllerAccess.Id,
            ActionAccessId = actionAccess.Id,
            IsAllowed = isAllowed,
        });
        await dbContext.SaveChangesAsync();

        return (departmentId, positionId);
    }

    private static async Task AssignUserToOrganizationAsync(
        ApplicationDbContext dbContext,
        ApplicationUser user,
        Guid departmentId,
        Guid positionId,
        bool isActive = true,
        DateTime? effectiveStartDate = null,
        DateTime? effectiveEndDate = null)
    {
        dbContext.ApplicationUserOrganizations.Add(new ApplicationUserOrganization
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            DepartmentId = departmentId,
            PositionId = positionId,
            IsActive = isActive,
            EffectiveStartDate = effectiveStartDate,
            EffectiveEndDate = effectiveEndDate,
        });
        await dbContext.SaveChangesAsync();
    }

    private static ClaimsPrincipal AuthenticatedPrincipal(Guid userId) =>
        new(new ClaimsIdentity(
            new[] { new Claim("user_id", userId.ToString()) },
            authenticationType: "TestAuth"));

    private static ClaimsPrincipal UnauthenticatedPrincipal() => new(new ClaimsIdentity());

    // --- Resolusi izin inti (AccessPermissionService.HasAccessAsync) ---

    [Fact]
    public async Task HasAccessAsync_AllowsUserWithMatchingOrganizationAndPolicy()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var user = await SeedUserAsync(db);
        var (departmentId, positionId) = await GrantPolicyAsync(db, BillingWriteOffController, ApproveAction);
        await AssignUserToOrganizationAsync(db, user, departmentId, positionId);
        var service = CreateService(db);

        var hasAccess = await service.HasAccessAsync(
            AuthenticatedPrincipal(user.Id), BillingWriteOffController, ApproveAction);

        Assert.True(hasAccess);
    }

    [Fact]
    public async Task HasAccessAsync_DeniesUserWithNoPolicyForAction()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var user = await SeedUserAsync(db);
        // User punya organisasi tapi organisasi itu tidak pernah diberi policy untuk action ini -
        // meniru "role/jabatan yang benar tapi tidak berwenang untuk aksi sensitif ini".
        var (departmentId, positionId) = await GrantPolicyAsync(db, "SomeOtherController", "SomeOtherAction");
        await AssignUserToOrganizationAsync(db, user, departmentId, positionId);
        var service = CreateService(db);

        var hasAccess = await service.HasAccessAsync(
            AuthenticatedPrincipal(user.Id), BillingWriteOffController, ApproveAction);

        Assert.False(hasAccess);
    }

    [Fact]
    public async Task HasAccessAsync_DeniesWhenPolicyExplicitlyNotAllowed()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var user = await SeedUserAsync(db);
        var (departmentId, positionId) = await GrantPolicyAsync(
            db, CashierShiftController, ReopenAction, isAllowed: false);
        await AssignUserToOrganizationAsync(db, user, departmentId, positionId);
        var service = CreateService(db);

        var hasAccess = await service.HasAccessAsync(
            AuthenticatedPrincipal(user.Id), CashierShiftController, ReopenAction);

        Assert.False(hasAccess);
    }

    [Fact]
    public async Task HasAccessAsync_DeniesWhenOrganizationAssignmentHasExpired()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var user = await SeedUserAsync(db);
        var (departmentId, positionId) = await GrantPolicyAsync(db, CashierShiftController, ReopenAction);
        await AssignUserToOrganizationAsync(
            db, user, departmentId, positionId,
            effectiveEndDate: DateTime.UtcNow.AddDays(-1));
        var service = CreateService(db);

        var hasAccess = await service.HasAccessAsync(
            AuthenticatedPrincipal(user.Id), CashierShiftController, ReopenAction);

        Assert.False(hasAccess);
    }

    [Fact]
    public async Task HasAccessAsync_DeniesUnauthenticatedPrincipal()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);

        var hasAccess = await service.HasAccessAsync(
            UnauthenticatedPrincipal(), BillingInvoiceController, UpdateAction);

        Assert.False(hasAccess);
    }

    [Fact]
    public async Task HasAccessAsync_DeniesInactiveUserEvenWithMatchingPolicy()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var user = await SeedUserAsync(db, isActive: false);
        var (departmentId, positionId) = await GrantPolicyAsync(db, BillingInvoiceController, UpdateAction);
        await AssignUserToOrganizationAsync(db, user, departmentId, positionId);
        var service = CreateService(db);

        var hasAccess = await service.HasAccessAsync(
            AuthenticatedPrincipal(user.Id), BillingInvoiceController, UpdateAction);

        Assert.False(hasAccess);
    }

    [Fact]
    public async Task HasAccessAsync_SuperAdminBypassesPolicyWhenClinicalPolicyNotEnforced()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var roleManager = CreateRoleManager(db);
        await roleManager.CreateAsync(new ApplicationRole { Name = "SuperAdmin", NormalizedName = "SUPERADMIN" });
        var userManager = CreateUserManager(db);
        var user = await SeedUserAsync(db, userType: UserType.SuperAdmin);
        await userManager.AddToRoleAsync(user, "SuperAdmin");
        // Sengaja TIDAK memberi policy apa pun - bypass harus berlaku murni dari role, bukan dari policy.
        var service = CreateService(db, enforceClinicalPolicyForSuperAdmin: false);

        var hasAccess = await service.HasAccessAsync(
            AuthenticatedPrincipal(user.Id), BillingWriteOffController, ApproveAction);

        Assert.True(hasAccess);
    }

    [Fact]
    public async Task HasAccessAsync_SuperAdminWithClinicalPolicyEnforcedFallsThroughToNormalPolicyWhenNotSystemOnly()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var roleManager = CreateRoleManager(db);
        await roleManager.CreateAsync(new ApplicationRole { Name = "SuperAdmin", NormalizedName = "SUPERADMIN" });
        var userManager = CreateUserManager(db);
        var user = await SeedUserAsync(db, userType: UserType.SuperAdmin);
        await userManager.AddToRoleAsync(user, "SuperAdmin");
        // Action ada tapi tidak ditandai IsSystemOnly, dan SuperAdmin tidak diberi policy department/
        // position apa pun - dengan clinical policy DIPAKSAKAN, SuperAdmin tidak lagi otomatis lolos.
        await GrantPolicyAsync(db, BillingWriteOffController, ApproveAction, actionSystemOnly: false);
        var service = CreateService(db, enforceClinicalPolicyForSuperAdmin: true);

        var hasAccess = await service.HasAccessAsync(
            AuthenticatedPrincipal(user.Id), BillingWriteOffController, ApproveAction);

        Assert.False(hasAccess);
    }

    [Fact]
    public async Task HasAccessAsync_SuperAdminWithClinicalPolicyEnforcedAllowedWhenActionIsSystemOnly()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var roleManager = CreateRoleManager(db);
        await roleManager.CreateAsync(new ApplicationRole { Name = "SuperAdmin", NormalizedName = "SUPERADMIN" });
        var userManager = CreateUserManager(db);
        var user = await SeedUserAsync(db, userType: UserType.SuperAdmin);
        await userManager.AddToRoleAsync(user, "SuperAdmin");
        await GrantPolicyAsync(db, CashierShiftController, ReopenAction, actionSystemOnly: true);
        var service = CreateService(db, enforceClinicalPolicyForSuperAdmin: true);

        var hasAccess = await service.HasAccessAsync(
            AuthenticatedPrincipal(user.Id), CashierShiftController, ReopenAction);

        Assert.True(hasAccess);
    }

    // --- Skenario representatif billing-kasir (BIL-AT-022) ---

    [Fact]
    public async Task HasAccessAsync_KasirWithoutFinanceRoleCannotApproveWriteOff()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var kasir = await SeedUserAsync(db);
        // Kasir hanya diberi izin operasi kasir sehari-hari, bukan approval financial exception.
        var (departmentId, positionId) = await GrantPolicyAsync(db, CashierShiftController, "Create");
        await AssignUserToOrganizationAsync(db, kasir, departmentId, positionId);
        var service = CreateService(db);

        var hasAccess = await service.HasAccessAsync(
            AuthenticatedPrincipal(kasir.Id), BillingWriteOffController, ApproveAction);

        Assert.False(hasAccess);
    }

    [Fact]
    public async Task HasAccessAsync_RegularCashierCannotReopenShift()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var kasir = await SeedUserAsync(db);
        var (departmentId, positionId) = await GrantPolicyAsync(db, CashierShiftController, "Create");
        await AssignUserToOrganizationAsync(db, kasir, departmentId, positionId);
        var service = CreateService(db);

        var hasAccess = await service.HasAccessAsync(
            AuthenticatedPrincipal(kasir.Id), CashierShiftController, ReopenAction);

        Assert.False(hasAccess);
    }

    [Fact]
    public async Task HasAccessAsync_KepalaKasirCanReopenShiftWhenExplicitlyGranted()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var kepalaKasir = await SeedUserAsync(db);
        var (departmentId, positionId) = await GrantPolicyAsync(db, CashierShiftController, ReopenAction);
        await AssignUserToOrganizationAsync(db, kepalaKasir, departmentId, positionId);
        var service = CreateService(db);

        var hasAccess = await service.HasAccessAsync(
            AuthenticatedPrincipal(kepalaKasir.Id), CashierShiftController, ReopenAction);

        Assert.True(hasAccess);
    }

    // --- Pipeline filter penuh (AccessPermissionFilter) - bentuk 401/403 yang dikembalikan ke HTTP caller ---

    private static AuthorizationFilterContext BuildFilterContext(ClaimsPrincipal principal)
    {
        var httpContext = new DefaultHttpContext { User = principal };
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
    }

    [Fact]
    public async Task Filter_ReturnsUnauthorizedWhenCallerNotAuthenticated()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var filter = new AccessPermissionFilter(
            CreateService(db), CreateLogger(), BillingWriteOffController, ApproveAction);
        var context = BuildFilterContext(UnauthenticatedPrincipal());

        await filter.OnAuthorizationAsync(context);

        var result = Assert.IsType<UnauthorizedObjectResult>(context.Result);
        Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
        var body = Assert.IsType<ApiResponse<object>>(result.Value);
        Assert.False(body.Success);
    }

    [Fact]
    public async Task Filter_ReturnsForbiddenWhenAuthenticatedButNotAuthorized()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var user = await SeedUserAsync(db);
        // Tidak ada policy sama sekali untuk user ini - simulasi role salah/tidak berwenang.
        var filter = new AccessPermissionFilter(
            CreateService(db), CreateLogger(), BillingWriteOffController, ApproveAction);
        var context = BuildFilterContext(AuthenticatedPrincipal(user.Id));

        await filter.OnAuthorizationAsync(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(StatusCodes.Status403Forbidden, result.StatusCode);
        var body = Assert.IsType<ApiResponse<object>>(result.Value);
        Assert.False(body.Success);
    }

    [Fact]
    public async Task Filter_AllowsRequestThroughWhenAuthorized()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var user = await SeedUserAsync(db);
        var (departmentId, positionId) = await GrantPolicyAsync(db, CashierShiftController, ReopenAction);
        await AssignUserToOrganizationAsync(db, user, departmentId, positionId);
        var filter = new AccessPermissionFilter(
            CreateService(db), CreateLogger(), CashierShiftController, ReopenAction);
        var context = BuildFilterContext(AuthenticatedPrincipal(user.Id));

        await filter.OnAuthorizationAsync(context);

        Assert.Null(context.Result);
    }
}
