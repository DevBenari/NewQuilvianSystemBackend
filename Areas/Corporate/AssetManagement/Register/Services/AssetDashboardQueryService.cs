using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

public sealed class AssetDashboardQueryService
{
    private const int RecentDamagedCount = 10;

    private static readonly AssetCondition[] DamagedConditions = [AssetCondition.RusakRingan, AssetCondition.RusakBerat];

    private readonly ApplicationDbContext _context;
    private readonly AssetActorResolver _actorResolver;
    private readonly AssetRecordService _recordService;

    public AssetDashboardQueryService(ApplicationDbContext context, AssetActorResolver actorResolver, AssetRecordService recordService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _actorResolver = actorResolver ?? throw new ArgumentNullException(nameof(actorResolver));
        _recordService = recordService ?? throw new ArgumentNullException(nameof(recordService));
    }

    public async Task<AssetDashboardSummaryResponse> GetSummaryAsync(AssetDashboardQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var unitId = request.OrganizationUnitId is { } requested && requested != Guid.Empty ? requested : (Guid?)null;
        if (unitId.HasValue)
        {
            actor.EnsureCanAccessUnit(unitId);
        }

        var baseQuery = await _recordService.FilteredAssetsAsync(new AssetPagedQuery { OrganizationUnitId = unitId }, cancellationToken);
        var rows = await baseQuery
            .GroupBy(x => new { x.RecordStatus, x.Condition, x.PlacementState })
            .Select(g => new { g.Key.RecordStatus, g.Key.Condition, g.Key.PlacementState, Count = g.Count() })
            .ToListAsync(cancellationToken);

        int CountWhere(Func<AssetRecordStatus, AssetCondition, AssetPlacementState, bool> predicate) =>
            rows.Where(x => predicate(x.RecordStatus, x.Condition, x.PlacementState)).Sum(x => x.Count);

        var idleQuery = await _recordService.FilteredAssetsAsync(
            new AssetPagedQuery { OrganizationUnitId = unitId, RecordStatus = AssetRecordStatus.Terdaftar, IsIdle = true },
            cancellationToken);
        var idleCount = await idleQuery.CountAsync(cancellationToken);

        var registered = (int)AssetRecordStatus.Terdaftar;
        var cards = new List<AssetDashboardCardResponse>
        {
            Card("registered", "Total Aset", CountWhere((s, _, _) => s == AssetRecordStatus.Terdaftar), unitId,
                ("recordStatus", registered.ToString(CultureInfo.InvariantCulture))),
            Card("submitted", "Menunggu Nomor Inventaris", CountWhere((s, _, _) => s == AssetRecordStatus.Diajukan), unitId,
                ("recordStatus", ((int)AssetRecordStatus.Diajukan).ToString(CultureInfo.InvariantCulture))),
            Card("notPlaced", "Belum Ditempatkan", CountWhere((s, _, p) => s == AssetRecordStatus.Terdaftar && p == AssetPlacementState.BelumDitempatkan), unitId,
                ("recordStatus", registered.ToString(CultureInfo.InvariantCulture)),
                ("placementState", ((int)AssetPlacementState.BelumDitempatkan).ToString(CultureInfo.InvariantCulture))),
            Card("placed", "Sudah Ditempatkan", CountWhere((s, _, p) => s == AssetRecordStatus.Terdaftar && p == AssetPlacementState.Ditempatkan), unitId,
                ("recordStatus", registered.ToString(CultureInfo.InvariantCulture)),
                ("placementState", ((int)AssetPlacementState.Ditempatkan).ToString(CultureInfo.InvariantCulture))),
            Card("idle", "Aset Menganggur", idleCount, unitId,
                ("recordStatus", registered.ToString(CultureInfo.InvariantCulture)),
                ("isIdle", "true"))
        };

        var conditions = Enum.GetValues<AssetCondition>()
            .Select(condition => Card(
                ConditionKey(condition),
                AssetRegisterLabels.Condition(condition),
                CountWhere((s, c, _) => s == AssetRecordStatus.Terdaftar && c == condition),
                unitId,
                ("recordStatus", registered.ToString(CultureInfo.InvariantCulture)),
                ("condition", ((int)condition).ToString(CultureInfo.InvariantCulture))))
            .ToList();

        var damagedQuery = await _recordService.FilteredAssetsAsync(
            new AssetPagedQuery { OrganizationUnitId = unitId, RecordStatus = AssetRecordStatus.Terdaftar },
            cancellationToken);
        var recentDamaged = await damagedQuery
            .Where(x => DamagedConditions.Contains(x.Condition))
            .OrderByDescending(x => x.ConditionChangedAt ?? x.ApprovedAt ?? x.CreateDateTime)
            .ThenBy(x => x.Id)
            .Take(RecentDamagedCount)
            .Select(x => new AssetDamagedItemResponse
            {
                Id = x.Id,
                InventoryNumber = x.InventoryNumber,
                AssetName = x.AssetName,
                GroupName = x.AssetGroup != null ? x.AssetGroup.GroupName : string.Empty,
                CurrentOrganizationUnitName = x.CurrentOrganizationUnit != null ? x.CurrentOrganizationUnit.UnitName : null,
                CurrentLocationName = x.CurrentLocation != null ? x.CurrentLocation.LocationName : null,
                Condition = x.Condition,
                ConditionSince = x.ConditionChangedAt ?? x.ApprovedAt ?? x.CreateDateTime
            })
            .ToListAsync(cancellationToken);
        recentDamaged.ForEach(x => x.ConditionName = AssetRegisterLabels.Condition(x.Condition));

        return new AssetDashboardSummaryResponse
        {
            OrganizationUnitId = unitId,
            OrganizationUnitName = unitId is { } id
                ? await _context.MstOrganizationUnits.AsNoTracking().Where(x => x.Id == id).Select(x => x.UnitName).FirstOrDefaultAsync(cancellationToken)
                : null,
            CanReadAllUnits = actor.CanReadAllUnits,
            Cards = cards,
            Conditions = conditions,
            DamagedTotal = CountWhere((s, c, _) => s == AssetRecordStatus.Terdaftar && DamagedConditions.Contains(c)),
            RecentDamaged = recentDamaged,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<List<AssetUnitSummaryResponse>> GetByUnitAsync(CancellationToken cancellationToken = default)
    {
        await RequireActorAsync(cancellationToken);
        var query = await _recordService.FilteredAssetsAsync(new AssetPagedQuery { RecordStatus = AssetRecordStatus.Terdaftar }, cancellationToken);

        var rows = await query
            .Where(x => x.CurrentOrganizationUnitId != null)
            .GroupBy(x => new { UnitId = x.CurrentOrganizationUnitId!.Value, x.Condition })
            .Select(g => new { g.Key.UnitId, g.Key.Condition, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var unitIds = rows.Select(x => x.UnitId).Distinct().ToList();
        var names = await _context.MstOrganizationUnits
            .AsNoTracking()
            .Where(x => unitIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.UnitName, cancellationToken);

        var registered = ((int)AssetRecordStatus.Terdaftar).ToString(CultureInfo.InvariantCulture);
        return rows
            .GroupBy(x => x.UnitId)
            .Select(g =>
            {
                int Of(AssetCondition condition) => g.Where(x => x.Condition == condition).Sum(x => x.Count);
                return new AssetUnitSummaryResponse
                {
                    OrganizationUnitId = g.Key,
                    OrganizationUnitName = names.TryGetValue(g.Key, out var name) ? name : string.Empty,
                    Total = g.Sum(x => x.Count),
                    Active = Of(AssetCondition.Aktif),
                    LightlyDamaged = Of(AssetCondition.RusakRingan),
                    HeavilyDamaged = Of(AssetCondition.RusakBerat),
                    Unused = Of(AssetCondition.TidakDigunakan),
                    Lost = Of(AssetCondition.Hilang),
                    Filter = new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["recordStatus"] = registered,
                        ["organizationUnitId"] = g.Key.ToString()
                    }
                };
            })
            .OrderByDescending(x => x.Total)
            .ThenBy(x => x.OrganizationUnitName, StringComparer.CurrentCulture)
            .ToList();
    }

    private async Task<AssetActor> RequireActorAsync(CancellationToken cancellationToken)
    {
        var actor = await _actorResolver.GetCurrentAsync(cancellationToken);
        if (!actor.IsAuthenticated)
        {
            throw AssetManagementException.Forbidden(AssetRegisterErrorCodes.OutsideUnit, AssetRegisterMessages.OutsideUnit);
        }

        return actor;
    }

    private static AssetDashboardCardResponse Card(string key, string label, int count, Guid? unitId, params (string Name, string Value)[] filter)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (name, value) in filter)
        {
            values[name] = value;
        }

        if (unitId is { } id)
        {
            values["organizationUnitId"] = id.ToString();
        }

        return new AssetDashboardCardResponse { Key = key, Label = label, Count = count, Filter = values };
    }

    private static string ConditionKey(AssetCondition condition) => condition switch
    {
        AssetCondition.Aktif => "active",
        AssetCondition.RusakRingan => "lightlyDamaged",
        AssetCondition.RusakBerat => "heavilyDamaged",
        AssetCondition.TidakDigunakan => "unused",
        AssetCondition.Hilang => "lost",
        _ => condition.ToString()
    };
}
