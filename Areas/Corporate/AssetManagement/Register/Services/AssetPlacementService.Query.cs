using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

public sealed partial class AssetPlacementService
{
    public AssetMasterFilterMetadataResponse GetHandoverMetadata() => new()
    {
        DefaultFilter = new AssetMasterDefaultFilterResponse { SortBy = "handedOverAt", SortDirection = "desc" },
        SortOptions = [new() { Value = "handedOverAt", Label = "Tanggal Diserahkan" }],
        FlagOptions =
        [
            EnumOptions("status", "Status Penyerahan", Enum.GetValues<HandoverStatus>(), AssetRegisterLabels.HandoverStatus),
            EnumOptions("purpose", "Tujuan Penyerahan", Enum.GetValues<HandoverPurpose>(), AssetRegisterLabels.HandoverPurpose)
        ],
        QueryParameters =
        [
            new() { Name = "search", Type = "string", Description = "Mencari pada nomor penyerahan, nomor inventaris, dan nama aset" },
            new() { Name = "status", Type = "integer", Description = "Lihat flagOptions status" },
            new() { Name = "purpose", Type = "integer", Description = "Lihat flagOptions purpose" },
            new() { Name = "targetOrganizationUnitId", Type = "guid", Description = "Unit tujuan" },
            new() { Name = "awaitingMyConfirmation", Type = "boolean", Description = "true = hanya penyerahan yang menunggu konfirmasi pengguna ini", Example = "true" },
            new() { Name = "from", Type = "date", Description = "Tanggal diserahkan paling awal (WIB), inklusif", Example = "2026-11-17" },
            new() { Name = "to", Type = "date", Description = "Tanggal diserahkan paling akhir (WIB), inklusif", Example = "2026-11-17" },
            new() { Name = "sortDirection", Type = "string", Description = "asc atau desc", Example = "desc" },
            new() { Name = "pageNumber", Type = "integer", Description = "Halaman, mulai 1", Example = "1" },
            new() { Name = "pageSize", Type = "integer", Description = $"Jumlah baris per halaman, maksimal {MaxPageSize}", Example = "25" }
        ]
    };

    public async Task<AssetHandoverSummaryResponse> GetHandoverSummaryAsync(CancellationToken cancellationToken = default)
    {
        var actor = await RequireActorAsync(cancellationToken);
        var query = ScopedHandovers(actor);
        var counts = await query
            .GroupBy(x => x.Status)
            .Select(x => new { Status = x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);
        var awaiting = await AwaitingConfirmation(query, actor).CountAsync(cancellationToken);

        int CountOf(HandoverStatus status) => counts.FirstOrDefault(x => x.Status == status)?.Count ?? 0;
        return new AssetHandoverSummaryResponse
        {
            Total = counts.Sum(x => x.Count),
            Submitted = CountOf(HandoverStatus.Diserahkan),
            Confirmed = CountOf(HandoverStatus.Dikonfirmasi),
            Rejected = CountOf(HandoverStatus.Ditolak),
            Cancelled = CountOf(HandoverStatus.Dibatalkan),
            AwaitingMyConfirmation = awaiting
        };
    }

    public async Task<PagedResult<AssetHandoverResponse>> GetHandoversAsync(AssetPlacementPagedQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var query = ScopedHandovers(actor);

        if (request.Status is { } status)
        {
            query = query.Where(x => x.Status == status);
        }

        if (request.Purpose is { } purpose)
        {
            query = query.Where(x => x.Purpose == purpose);
        }

        if (request.TargetOrganizationUnitId is { } unitId && unitId != Guid.Empty)
        {
            query = query.Where(x => x.TargetOrganizationUnitId == unitId);
        }

        if (request.AwaitingMyConfirmation == true)
        {
            query = AwaitingConfirmation(query, actor);
        }

        if (request.From is { } from)
        {
            var fromUtc = AppDateTimeHelper.OperationalDateToUtc(from.ToDateTime(TimeOnly.MinValue));
            query = query.Where(x => x.HandedOverAt >= fromUtc);
        }

        if (request.To is { } to)
        {
            var toUtcExclusive = AppDateTimeHelper.OperationalDateToUtc(to.AddDays(1).ToDateTime(TimeOnly.MinValue));
            query = query.Where(x => x.HandedOverAt < toUtcExclusive);
        }

        var keyword = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim().ToUpper();
        if (keyword != null)
        {
            query = query.Where(x =>
                x.HandoverNumber.ToUpper().Contains(keyword) ||
                x.Items.Any(i => !i.IsDelete && (
                    (i.Asset!.InventoryNumber != null && i.Asset.InventoryNumber.ToUpper().Contains(keyword)) ||
                    i.Asset.AssetName.ToUpper().Contains(keyword))));
        }

        var ordered = string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase)
            ? query.OrderBy(x => x.HandedOverAt)
            : query.OrderByDescending(x => x.HandedOverAt);

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, MaxPageSize);
        var total = await query.CountAsync(cancellationToken);
        var items = await ProjectHandover(ordered.ThenBy(x => x.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize))
            .ToListAsync(cancellationToken);
        await DescribeHandoversAsync(items, actor, cancellationToken);

        return new PagedResult<AssetHandoverResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    public async Task<AssetHandoverResponse> GetHandoverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var actor = await RequireActorAsync(cancellationToken);
        var handover = await ProjectHandover(_context.AstAssetHandovers.AsNoTracking().Where(x => x.Id == id && !x.IsDelete))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw HandoverNotFound();

        if (!CanSeeHandover(handover.TargetOrganizationUnitId, handover.TargetCustodianEmployeeId, handover.HandedOver?.UserId ?? Guid.Empty, actor))
        {
            throw AssetManagementException.Forbidden(AssetRegisterErrorCodes.OutsideUnit, AssetRegisterMessages.HandoverOutsideScope);
        }

        handover.Items = await _context.AstAssetHandoverItems
            .AsNoTracking()
            .Where(x => x.HandoverId == id && !x.IsDelete)
            .OrderBy(x => x.Asset!.InventoryNumber)
            .Select(x => new AssetHandoverItemResponse
            {
                Id = x.Id,
                AssetId = x.AssetId,
                InventoryNumber = x.Asset!.InventoryNumber,
                AssetName = x.Asset.AssetName,
                GroupName = x.Asset.AssetGroup!.GroupName,
                ConditionAtHandover = x.ConditionAtHandover
            })
            .ToListAsync(cancellationToken);
        handover.Items.ForEach(x => x.ConditionAtHandoverName = AssetRegisterLabels.Condition(x.ConditionAtHandover));

        await DescribeHandoversAsync([handover], actor, cancellationToken);
        return handover;
    }

    public async Task<AssetLabelDocument> GetHandoverDocumentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var handover = await GetHandoverAsync(id, cancellationToken);
        if (handover.Status != HandoverStatus.Dikonfirmasi)
        {
            throw AssetManagementException.Conflict(
                AssetRegisterErrorCodes.HandoverStatus,
                string.Format(AssetRegisterMessages.HandoverStatusFormat, handover.StatusName.ToLowerInvariant(), "dicetak berita acaranya"));
        }

        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var content = _documentService.RenderHandoverReport(handover, entity.LegalEntityName, entity.HospitalSiteName);
        return new AssetLabelDocument(content, $"berita-acara-{handover.HandoverNumber}.pdf", handover.ItemCount);
    }

    public async Task<PagedResult<AssetPlacementListItemResponse>> GetPlacementsAsync(AssetPlacementPagedQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await GetPlacementPageAsync(request, currentOnly: true, cancellationToken);
    }

    public async Task<PagedResult<AssetPlacementListItemResponse>> GetPlacementHistoryAsync(AssetPlacementPagedQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await GetPlacementPageAsync(request, currentOnly: false, cancellationToken);
    }

    private async Task<PagedResult<AssetPlacementListItemResponse>> GetPlacementPageAsync(AssetPlacementPagedQuery request, bool currentOnly, CancellationToken cancellationToken)
    {
        var actor = await RequireActorAsync(cancellationToken);
        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var query = _context.AstAssetPlacements
            .AsNoTracking()
            .Where(x => !x.IsDelete && !x.Asset!.IsDelete && x.Asset.LegalEntityId == entity.LegalEntityId);

        if (currentOnly)
        {
            query = query.Where(x => x.EndAt == null);
        }

        if (!actor.CanReadAllUnits)
        {
            var unitIds = actor.UnitIds.ToList();
            var employeeId = actor.EmployeeId;
            query = query.Where(x => unitIds.Contains(x.OrganizationUnitId) || (employeeId.HasValue && x.CustodianEmployeeId == employeeId.Value));
        }

        if (request.OrganizationUnitId is { } unitId && unitId != Guid.Empty)
        {
            query = query.Where(x => x.OrganizationUnitId == unitId);
        }

        if (request.LocationId is { } locationId && locationId != Guid.Empty)
        {
            query = query.Where(x => x.LocationId == locationId);
        }

        if (request.CustodianEmployeeId is { } custodianId && custodianId != Guid.Empty)
        {
            query = query.Where(x => x.CustodianEmployeeId == custodianId);
        }

        if (request.AssetId is { } assetId && assetId != Guid.Empty)
        {
            query = query.Where(x => x.AssetId == assetId);
        }

        if (request.From is { } from)
        {
            var fromUtc = AppDateTimeHelper.OperationalDateToUtc(from.ToDateTime(TimeOnly.MinValue));
            query = query.Where(x => x.EndAt == null || x.EndAt >= fromUtc);
        }

        if (request.To is { } to)
        {
            var toUtcExclusive = AppDateTimeHelper.OperationalDateToUtc(to.AddDays(1).ToDateTime(TimeOnly.MinValue));
            query = query.Where(x => x.StartAt < toUtcExclusive);
        }

        var keyword = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim().ToUpper();
        if (keyword != null)
        {
            query = query.Where(x =>
                (x.Asset!.InventoryNumber != null && x.Asset.InventoryNumber.ToUpper().Contains(keyword)) ||
                x.Asset.AssetName.ToUpper().Contains(keyword));
        }

        var ordered = currentOnly
            ? query.OrderBy(x => x.OrganizationUnit!.UnitName).ThenBy(x => x.Location!.LocationName).ThenBy(x => x.Asset!.InventoryNumber)
            : string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(x => x.StartAt)
                : query.OrderByDescending(x => x.StartAt);

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, MaxPageSize);
        var total = await query.CountAsync(cancellationToken);
        var items = await ordered
            .ThenBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AssetPlacementListItemResponse
            {
                Id = x.Id,
                AssetId = x.AssetId,
                InventoryNumber = x.Asset!.InventoryNumber,
                AssetName = x.Asset.AssetName,
                GroupName = x.Asset.AssetGroup!.GroupName,
                Condition = x.Asset.Condition,
                OrganizationUnitId = x.OrganizationUnitId,
                OrganizationUnitName = x.OrganizationUnit != null ? x.OrganizationUnit.UnitName : null,
                LocationId = x.LocationId,
                LocationName = x.Location != null ? x.Location.LocationName : null,
                CustodianEmployeeId = x.CustodianEmployeeId,
                CustodianEmployeeName = x.CustodianEmployee != null ? x.CustodianEmployee.FullName : null,
                SourceType = x.SourceType,
                SourceDocumentId = x.SourceDocumentId,
                StartAt = x.StartAt,
                EndAt = x.EndAt,
                IsCurrent = x.EndAt == null
            })
            .ToListAsync(cancellationToken);
        items.ForEach(x => x.ConditionName = AssetRegisterLabels.Condition(x.Condition));

        return new PagedResult<AssetPlacementListItemResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    private IQueryable<AstAssetHandover> ScopedHandovers(AssetActor actor)
    {
        var query = _context.AstAssetHandovers.AsNoTracking().Where(x => !x.IsDelete);
        if (actor.CanReadAllUnits)
        {
            return query;
        }

        var unitIds = actor.UnitIds.ToList();
        var employeeId = actor.EmployeeId;
        var userId = actor.UserId;
        return query.Where(x =>
            unitIds.Contains(x.TargetOrganizationUnitId) ||
            (employeeId.HasValue && x.TargetCustodianEmployeeId == employeeId.Value) ||
            x.HandedOverBy == userId);
    }

    private static IQueryable<AstAssetHandover> AwaitingConfirmation(IQueryable<AstAssetHandover> query, AssetActor actor)
    {
        var headUnitIds = actor.HeadOfUnitIds.ToList();
        var employeeId = actor.EmployeeId;
        return query.Where(x =>
            x.Status == HandoverStatus.Diserahkan &&
            (headUnitIds.Contains(x.TargetOrganizationUnitId) || (employeeId.HasValue && x.TargetCustodianEmployeeId == employeeId.Value)));
    }

    private static bool CanSeeHandover(Guid targetUnitId, Guid targetCustodianEmployeeId, Guid handedOverBy, AssetActor actor) =>
        actor.CanReadAllUnits ||
        actor.UnitIds.Contains(targetUnitId) ||
        (actor.EmployeeId is { } employeeId && employeeId == targetCustodianEmployeeId) ||
        handedOverBy == actor.UserId;

    private static IQueryable<AssetHandoverResponse> ProjectHandover(IQueryable<AstAssetHandover> query) =>
        query.Select(x => new AssetHandoverResponse
        {
            Id = x.Id,
            HandoverNumber = x.HandoverNumber,
            Purpose = x.Purpose,
            Status = x.Status,
            TargetOrganizationUnitId = x.TargetOrganizationUnitId,
            TargetOrganizationUnitName = x.TargetOrganizationUnit != null ? x.TargetOrganizationUnit.UnitName : null,
            TargetLocationId = x.TargetLocationId,
            TargetLocationName = x.TargetLocation != null ? x.TargetLocation.LocationName : null,
            TargetCustodianEmployeeId = x.TargetCustodianEmployeeId,
            TargetCustodianEmployeeName = x.TargetCustodianEmployee != null ? x.TargetCustodianEmployee.FullName : null,
            Notes = x.Notes,
            HandedOver = new AssetHandoverActorResponse { UserId = x.HandedOverBy, At = x.HandedOverAt },
            Confirmed = x.ConfirmedBy != null ? new AssetHandoverActorResponse { UserId = x.ConfirmedBy.Value, At = x.ConfirmedAt!.Value } : null,
            Rejected = x.RejectedBy != null ? new AssetHandoverActorResponse { UserId = x.RejectedBy.Value, At = x.RejectedAt!.Value } : null,
            Cancelled = x.CancelledBy != null ? new AssetHandoverActorResponse { UserId = x.CancelledBy.Value, At = x.CancelledAt!.Value } : null,
            DecisionReason = x.DecisionReason,
            ItemCount = x.Items.Count(i => !i.IsDelete),
            RowVersion = x.RowVersion
        });

    private async Task DescribeHandoversAsync(IReadOnlyCollection<AssetHandoverResponse> handovers, AssetActor actor, CancellationToken cancellationToken)
    {
        var steps = handovers
            .SelectMany(x => new[] { x.HandedOver, x.Confirmed, x.Rejected, x.Cancelled })
            .OfType<AssetHandoverActorResponse>()
            .ToList();
        var userIds = steps.Select(x => x.UserId).Where(x => x != Guid.Empty).Distinct().ToList();
        var profiles = new Dictionary<Guid, (string DisplayName, string? PositionName)>();
        if (userIds.Count > 0)
        {
            var rows = await _context.Users
                .AsNoTracking()
                .Where(x => userIds.Contains(x.Id))
                .Select(x => new { x.Id, x.DisplayName, PositionName = x.PrimaryPosition != null ? x.PrimaryPosition.PositionName : null })
                .ToListAsync(cancellationToken);
            foreach (var row in rows)
            {
                profiles[row.Id] = (row.DisplayName, row.PositionName);
            }
        }

        foreach (var step in steps)
        {
            if (profiles.TryGetValue(step.UserId, out var profile))
            {
                step.Name = profile.DisplayName;
                step.PositionName = profile.PositionName;
            }
        }

        foreach (var handover in handovers)
        {
            handover.PurposeName = AssetRegisterLabels.HandoverPurpose(handover.Purpose);
            handover.StatusName = AssetRegisterLabels.HandoverStatus(handover.Status);
            var pending = handover.Status == HandoverStatus.Diserahkan;
            var recipient = IsRecipient(handover.TargetOrganizationUnitId, handover.TargetCustodianEmployeeId, actor);
            handover.IsAwaitingMyConfirmation = pending && recipient;
            handover.AvailableActions = [];
            if (pending && recipient)
            {
                handover.AvailableActions.Add("confirm");
                handover.AvailableActions.Add("reject");
            }

            if (pending && CanCancel(handover.HandedOver?.UserId ?? Guid.Empty, actor))
            {
                handover.AvailableActions.Add("cancel");
            }

            if (handover.Status == HandoverStatus.Dikonfirmasi)
            {
                handover.AvailableActions.Add("document");
            }
        }
    }

    internal static AssetMasterOptionSourceResponse EnumOptions<TEnum>(string name, string label, IEnumerable<TEnum> values, Func<TEnum, string> describe)
        where TEnum : struct, Enum => new()
    {
        Name = name,
        Label = label,
        Options = values
            .Select(x => new AssetMasterSortOptionResponse { Value = Convert.ToInt32(x).ToString(), Label = describe(x) })
            .ToList()
    };
}
