using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

public sealed class AssetActorResolver
{
    public const string ReadAllUnitsResource = "AssetRegister";
    public const string ReadAllUnitsAction = "ReadAllUnits";

    private readonly ApplicationDbContext _context;
    private readonly AccessPermissionService _accessPermissionService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private AssetActor? _current;

    public AssetActorResolver(
        ApplicationDbContext context,
        AccessPermissionService accessPermissionService,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _accessPermissionService = accessPermissionService ?? throw new ArgumentNullException(nameof(accessPermissionService));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public async Task<AssetActor> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        if (_current != null)
        {
            return _current;
        }

        return _current = await ResolveAsync(_httpContextAccessor.HttpContext?.User, cancellationToken);
    }

    public async Task<AssetActor> ResolveAsync(ClaimsPrincipal? user, CancellationToken cancellationToken = default)
    {
        if (user?.Identity?.IsAuthenticated != true)
        {
            return AssetActor.Anonymous;
        }

        var userIdText = user.FindFirstValue("user_id") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdText, out var userId) || userId == Guid.Empty)
        {
            return AssetActor.Anonymous;
        }

        var canReadAllUnits = await _accessPermissionService.HasAccessAsync(user, ReadAllUnitsResource, ReadAllUnitsAction);

        var account = await _context.Users
            .AsNoTracking()
            .Where(x => x.Id == userId && x.IsActive)
            .Select(x => new { x.WorkforceProfileId, x.EmployeeId })
            .FirstOrDefaultAsync(cancellationToken);

        if (account?.WorkforceProfileId is not { } workforceProfileId)
        {
            return AssetActor.Create(userId, null, account?.EmployeeId, [], [], canReadAllUnits);
        }

        var today = DateTime.SpecifyKind(AppDateTimeHelper.OperationalDate().Date, DateTimeKind.Utc);

        var assignments = await _context.WfpOrganizationAssignments
            .AsNoTracking()
            .Where(x =>
                x.WorkforceProfileId == workforceProfileId &&
                x.IsActive &&
                !x.IsDelete &&
                x.OrganizationUnitId.HasValue &&
                x.EffectiveStartDate <= today &&
                (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= today))
            .Select(x => new { UnitId = x.OrganizationUnitId!.Value, x.IsManagerialAssignment })
            .ToListAsync(cancellationToken);

        var unitIds = assignments.Select(x => x.UnitId);
        var headOfUnitIds = assignments.Where(x => x.IsManagerialAssignment).Select(x => x.UnitId);

        return AssetActor.Create(userId, workforceProfileId, account.EmployeeId, unitIds, headOfUnitIds, canReadAllUnits);
    }
}

public sealed class AssetActor
{
    public static readonly AssetActor Anonymous = new(Guid.Empty, null, null, new HashSet<Guid>(), new HashSet<Guid>(), false);

    private AssetActor(
        Guid userId,
        Guid? workforceProfileId,
        Guid? employeeId,
        IReadOnlySet<Guid> unitIds,
        IReadOnlySet<Guid> headOfUnitIds,
        bool canReadAllUnits)
    {
        UserId = userId;
        WorkforceProfileId = workforceProfileId;
        EmployeeId = employeeId;
        UnitIds = unitIds;
        HeadOfUnitIds = headOfUnitIds;
        CanReadAllUnits = canReadAllUnits;
    }

    public Guid UserId { get; }

    public Guid? WorkforceProfileId { get; }

    public Guid? EmployeeId { get; }

    public IReadOnlySet<Guid> UnitIds { get; }

    public IReadOnlySet<Guid> HeadOfUnitIds { get; }

    public bool CanReadAllUnits { get; }

    public bool IsAuthenticated => UserId != Guid.Empty;

    public bool CanAccessUnit(Guid? organizationUnitId) =>
        IsAuthenticated &&
        (CanReadAllUnits || (organizationUnitId.HasValue && UnitIds.Contains(organizationUnitId.Value)));

    public bool IsHeadOfUnit(Guid? organizationUnitId) =>
        IsAuthenticated && organizationUnitId.HasValue && HeadOfUnitIds.Contains(organizationUnitId.Value);

    public void EnsureCanAccessUnit(Guid? organizationUnitId)
    {
        if (!CanAccessUnit(organizationUnitId))
        {
            throw AssetManagementException.Forbidden(AssetRegisterErrorCodes.OutsideUnit, AssetRegisterMessages.OutsideUnit);
        }
    }

    internal static AssetActor Create(
        Guid userId,
        Guid? workforceProfileId,
        Guid? employeeId,
        IEnumerable<Guid> unitIds,
        IEnumerable<Guid> headOfUnitIds,
        bool canReadAllUnits)
    {
        var units = new HashSet<Guid>(unitIds);
        var heads = new HashSet<Guid>(headOfUnitIds.Where(units.Contains));
        return new AssetActor(userId, workforceProfileId, employeeId, units, heads, canReadAllUnits);
    }
}
