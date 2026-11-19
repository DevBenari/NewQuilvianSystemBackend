using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Services;

public sealed partial class AssetTransferService
{
    public const string ActionApproveUnit = "approveUnit";
    public const string ActionRejectUnit = "rejectUnit";
    public const string ActionApproveAsset = "approveAsset";
    public const string ActionRejectAsset = "rejectAsset";
    public const string ActionReceive = "receive";
    public const string ActionRejectReceipt = "rejectReceipt";
    public const string ActionCancel = "cancel";
    public const string ActionDocument = "document";

    public static AssetMasterFilterMetadataResponse GetMetadata() => new()
    {
        DefaultFilter = new AssetMasterDefaultFilterResponse { SortBy = "requestedAt", SortDirection = "desc" },
        SortOptions = [new() { Value = "requestedAt", Label = "Tanggal Diajukan" }],
        FlagOptions =
        [
            EnumOptions("status", "Status Mutasi", Enum.GetValues<TransferStatus>(), AssetMovementLabels.TransferStatus),
            EnumOptions("reason", "Alasan Mutasi", Enum.GetValues<TransferReason>(), AssetMovementLabels.TransferReason)
        ],
        QueryParameters =
        [
            new() { Name = "search", Type = "string", Description = "Mencari pada nomor mutasi, nomor inventaris, dan nama aset" },
            new() { Name = "status", Type = "integer", Description = "Lihat flagOptions status" },
            new() { Name = "reason", Type = "integer", Description = "Lihat flagOptions reason" },
            new() { Name = "sourceOrganizationUnitId", Type = "guid", Description = "Unit asal" },
            new() { Name = "targetOrganizationUnitId", Type = "guid", Description = "Unit tujuan" },
            new() { Name = "awaitingMe", Type = "boolean", Description = "true = hanya mutasi yang menunggu tindakan pengguna ini", Example = "true" },
            new() { Name = "from", Type = "date", Description = "Tanggal diajukan paling awal (WIB), inklusif", Example = "2026-11-19" },
            new() { Name = "to", Type = "date", Description = "Tanggal diajukan paling akhir (WIB), inklusif", Example = "2026-11-19" },
            new() { Name = "sortDirection", Type = "string", Description = "asc atau desc", Example = "desc" },
            new() { Name = "pageNumber", Type = "integer", Description = "Halaman, mulai 1", Example = "1" },
            new() { Name = "pageSize", Type = "integer", Description = $"Jumlah baris per halaman, maksimal {MaxPageSize}", Example = "25" }
        ]
    };

    public async Task<AssetTransferSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var actor = await RequireActorAsync(cancellationToken);
        var canApproveAsset = await CanApproveAssetAsync();
        var query = Scoped(actor);
        var counts = await query
            .GroupBy(x => x.Status)
            .Select(x => new { Status = x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);
        var awaiting = await AwaitingMe(query, actor, canApproveAsset).CountAsync(cancellationToken);

        int CountOf(TransferStatus status) => counts.FirstOrDefault(x => x.Status == status)?.Count ?? 0;
        return new AssetTransferSummaryResponse
        {
            Total = counts.Sum(x => x.Count),
            Submitted = CountOf(TransferStatus.Diajukan),
            UnitApproved = CountOf(TransferStatus.DisetujuiUnitAsal),
            AssetHeadApproved = CountOf(TransferStatus.DisetujuiBagianAset),
            Received = CountOf(TransferStatus.Diserahterimakan),
            Rejected = CountOf(TransferStatus.Ditolak),
            Cancelled = CountOf(TransferStatus.Dibatalkan),
            AwaitingMe = awaiting
        };
    }

    public async Task<PagedResult<AssetTransferResponse>> GetTransfersAsync(AssetTransferPagedQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var canApproveAsset = await CanApproveAssetAsync();
        var query = Scoped(actor);

        if (request.Status is { } status)
        {
            query = query.Where(x => x.Status == status);
        }

        if (request.Reason is { } reason)
        {
            query = query.Where(x => x.Reason == reason);
        }

        if (request.SourceOrganizationUnitId is { } sourceId && sourceId != Guid.Empty)
        {
            query = query.Where(x => x.SourceOrganizationUnitId == sourceId);
        }

        if (request.TargetOrganizationUnitId is { } targetId && targetId != Guid.Empty)
        {
            query = query.Where(x => x.TargetOrganizationUnitId == targetId);
        }

        if (request.AwaitingMe == true)
        {
            query = AwaitingMe(query, actor, canApproveAsset);
        }

        if (request.From is { } from)
        {
            var fromUtc = AppDateTimeHelper.OperationalDateToUtc(from.ToDateTime(TimeOnly.MinValue));
            query = query.Where(x => x.RequestedAt >= fromUtc);
        }

        if (request.To is { } to)
        {
            var toUtcExclusive = AppDateTimeHelper.OperationalDateToUtc(to.AddDays(1).ToDateTime(TimeOnly.MinValue));
            query = query.Where(x => x.RequestedAt < toUtcExclusive);
        }

        var keyword = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim().ToUpper();
        if (keyword != null)
        {
            query = query.Where(x =>
                x.TransferNumber.ToUpper().Contains(keyword) ||
                x.Items.Any(i => !i.IsDelete && (
                    (i.Asset!.InventoryNumber != null && i.Asset.InventoryNumber.ToUpper().Contains(keyword)) ||
                    i.Asset.AssetName.ToUpper().Contains(keyword))));
        }

        var ordered = string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase)
            ? query.OrderBy(x => x.RequestedAt)
            : query.OrderByDescending(x => x.RequestedAt);

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, MaxPageSize);
        var total = await query.CountAsync(cancellationToken);
        var items = await Project(ordered.ThenBy(x => x.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize)).ToListAsync(cancellationToken);
        await DescribeAsync(items, actor, canApproveAsset, cancellationToken);

        return new PagedResult<AssetTransferResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    public async Task<AssetTransferResponse> GetTransferAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var actor = await RequireActorAsync(cancellationToken);
        var canApproveAsset = await CanApproveAssetAsync();
        var transfer = await Project(_context.AstTransfers.AsNoTracking().Where(x => x.Id == id && !x.IsDelete))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw TransferNotFound();

        if (!CanSee(transfer.SourceOrganizationUnitId, transfer.TargetOrganizationUnitId, transfer.Requested?.UserId ?? Guid.Empty, actor))
        {
            throw AssetManagementException.Forbidden(AssetMovementErrorCodes.OutsideScope, AssetMovementMessages.OutsideScope);
        }

        transfer.Items = await _context.AstTransferItems
            .AsNoTracking()
            .Where(x => x.TransferId == id && !x.IsDelete)
            .OrderBy(x => x.Asset!.InventoryNumber)
            .Select(x => new AssetTransferItemResponse
            {
                Id = x.Id,
                AssetId = x.AssetId,
                InventoryNumber = x.Asset!.InventoryNumber,
                AssetName = x.Asset.AssetName,
                GroupName = x.Asset.AssetGroup != null ? x.Asset.AssetGroup.GroupName : null,
                ConditionAtSubmit = x.ConditionAtSubmit,
                ConditionAtReceipt = x.ConditionAtReceipt
            })
            .ToListAsync(cancellationToken);
        foreach (var item in transfer.Items)
        {
            item.ConditionAtSubmitName = AssetRegisterLabels.Condition(item.ConditionAtSubmit);
            item.ConditionAtReceiptName = item.ConditionAtReceipt is { } atReceipt ? AssetRegisterLabels.Condition(atReceipt) : null;
        }

        await DescribeAsync([transfer], actor, canApproveAsset, cancellationToken);
        return transfer;
    }

    public async Task<AssetLabelDocument> GetDocumentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var transfer = await GetTransferAsync(id, cancellationToken);
        if (transfer.Status != TransferStatus.Diserahterimakan)
        {
            throw StatusConflict(transfer.Status);
        }

        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var content = _documentService.RenderTransferReport(transfer, entity.LegalEntityName, entity.HospitalSiteName);
        return new AssetLabelDocument(content, $"berita-acara-{transfer.TransferNumber}.pdf", transfer.ItemCount);
    }

    private IQueryable<AstTransfer> Scoped(AssetActor actor)
    {
        var query = _context.AstTransfers.AsNoTracking().Where(x => !x.IsDelete);
        if (actor.CanReadAllUnits)
        {
            return query;
        }

        var unitIds = actor.UnitIds.ToList();
        var userId = actor.UserId;
        return query.Where(x =>
            unitIds.Contains(x.SourceOrganizationUnitId) ||
            unitIds.Contains(x.TargetOrganizationUnitId) ||
            x.RequestedBy == userId);
    }

    private static IQueryable<AstTransfer> AwaitingMe(IQueryable<AstTransfer> query, AssetActor actor, bool canApproveAsset)
    {
        var headUnitIds = actor.HeadOfUnitIds.ToList();
        var userId = actor.UserId;
        return query.Where(x =>
            (x.Status == TransferStatus.Diajukan && headUnitIds.Contains(x.SourceOrganizationUnitId) && x.RequestedBy != userId) ||
            (x.Status == TransferStatus.DisetujuiUnitAsal && canApproveAsset && x.RequestedBy != userId) ||
            (x.Status == TransferStatus.DisetujuiBagianAset && headUnitIds.Contains(x.TargetOrganizationUnitId)));
    }

    private static bool CanSee(Guid sourceUnitId, Guid targetUnitId, Guid requestedBy, AssetActor actor) =>
        actor.CanReadAllUnits ||
        actor.UnitIds.Contains(sourceUnitId) ||
        actor.UnitIds.Contains(targetUnitId) ||
        requestedBy == actor.UserId;

    private static IQueryable<AssetTransferResponse> Project(IQueryable<AstTransfer> query) =>
        query.Select(x => new AssetTransferResponse
        {
            Id = x.Id,
            TransferNumber = x.TransferNumber,
            SourceOrganizationUnitId = x.SourceOrganizationUnitId,
            SourceOrganizationUnitName = x.SourceOrganizationUnit != null ? x.SourceOrganizationUnit.UnitName : null,
            TargetOrganizationUnitId = x.TargetOrganizationUnitId,
            TargetOrganizationUnitName = x.TargetOrganizationUnit != null ? x.TargetOrganizationUnit.UnitName : null,
            TargetLocationId = x.TargetLocationId,
            TargetLocationName = x.TargetLocation != null ? x.TargetLocation.LocationName : null,
            TargetCustodianEmployeeId = x.TargetCustodianEmployeeId,
            TargetCustodianEmployeeName = x.TargetCustodianEmployee != null ? x.TargetCustodianEmployee.FullName : null,
            Reason = x.Reason,
            ReasonNote = x.ReasonNote,
            CorrectsTransferId = x.CorrectsTransferId,
            CorrectsTransferNumber = x.CorrectsTransfer != null ? x.CorrectsTransfer.TransferNumber : null,
            SourceStockTakeResultId = x.SourceStockTakeResultId,
            Status = x.Status,
            Requested = new AssetMovementActorResponse { UserId = x.RequestedBy, At = x.RequestedAt },
            UnitApproved = x.UnitApprovedBy != null ? new AssetMovementActorResponse { UserId = x.UnitApprovedBy.Value, At = x.UnitApprovedAt!.Value } : null,
            IsUnitApprovalByRequester = x.IsUnitApprovalByRequester,
            AssetHeadApproved = x.AssetHeadApprovedBy != null ? new AssetMovementActorResponse { UserId = x.AssetHeadApprovedBy.Value, At = x.AssetHeadApprovedAt!.Value } : null,
            Received = x.ReceivedBy != null ? new AssetMovementActorResponse { UserId = x.ReceivedBy.Value, At = x.ReceivedAt!.Value } : null,
            Rejected = x.RejectedBy != null ? new AssetMovementActorResponse { UserId = x.RejectedBy.Value, At = x.RejectedAt!.Value } : null,
            Cancelled = x.CancelledBy != null ? new AssetMovementActorResponse { UserId = x.CancelledBy.Value, At = x.CancelledAt!.Value } : null,
            DecisionReason = x.DecisionReason,
            ItemCount = x.Items.Count(i => !i.IsDelete),
            RowVersion = x.RowVersion
        });

    private async Task DescribeAsync(IReadOnlyCollection<AssetTransferResponse> transfers, AssetActor actor, bool canApproveAsset, CancellationToken cancellationToken)
    {
        var steps = transfers
            .SelectMany(x => new[] { x.Requested, x.UnitApproved, x.AssetHeadApproved, x.Received, x.Rejected, x.Cancelled })
            .OfType<AssetMovementActorResponse>()
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

        foreach (var transfer in transfers)
        {
            transfer.StatusName = AssetMovementLabels.TransferStatus(transfer.Status);
            transfer.ReasonName = AssetMovementLabels.TransferReason(transfer.Reason);
            var requestedBy = transfer.Requested?.UserId ?? Guid.Empty;
            var actions = new List<string>();

            switch (transfer.Status)
            {
                case TransferStatus.Diajukan:
                    transfer.AwaitingParty = $"Kepala Unit {transfer.SourceOrganizationUnitName}";
                    if (actor.IsHeadOfUnit(transfer.SourceOrganizationUnitId) && requestedBy != actor.UserId)
                    {
                        actions.Add(ActionApproveUnit);
                        actions.Add(ActionRejectUnit);
                    }

                    break;
                case TransferStatus.DisetujuiUnitAsal:
                    transfer.AwaitingParty = "Kepala Bagian Aset";
                    if (canApproveAsset && requestedBy != actor.UserId)
                    {
                        actions.Add(ActionApproveAsset);
                        actions.Add(ActionRejectAsset);
                    }

                    break;
                case TransferStatus.DisetujuiBagianAset:
                    transfer.AwaitingParty = $"Kepala Unit {transfer.TargetOrganizationUnitName}";
                    if (actor.IsHeadOfUnit(transfer.TargetOrganizationUnitId))
                    {
                        actions.Add(ActionReceive);
                        actions.Add(ActionRejectReceipt);
                    }

                    break;
                case TransferStatus.Diserahterimakan:
                    actions.Add(ActionDocument);
                    break;
            }

            if ((transfer.Status is TransferStatus.Diajukan or TransferStatus.DisetujuiUnitAsal or TransferStatus.DisetujuiBagianAset) &&
                CanCancel(requestedBy, actor, canApproveAsset))
            {
                actions.Add(ActionCancel);
            }

            transfer.IsAwaitingMe = actions.Any(x => x is not (ActionCancel or ActionDocument));
            transfer.AvailableActions = actions;
        }
    }

    private static AssetMasterOptionSourceResponse EnumOptions<TEnum>(string name, string label, IEnumerable<TEnum> values, Func<TEnum, string> describe)
        where TEnum : struct, Enum => new()
    {
        Name = name,
        Label = label,
        Options = values
            .Select(x => new AssetMasterSortOptionResponse { Value = Convert.ToInt32(x).ToString(), Label = describe(x) })
            .ToList()
    };
}
