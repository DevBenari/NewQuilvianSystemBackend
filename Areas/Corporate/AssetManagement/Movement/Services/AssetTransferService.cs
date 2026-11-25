using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Services;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Services;

public sealed partial class AssetTransferService
{
    public const string PermissionResource = "AssetTransfer";
    public const string ApproveAssetAction = "ApproveAsset";
    public const string TransferDocumentType = "AstTransfer";
    public const string PlacementSourceTransfer = "MUTASI";
    private const string LogCategory = "Corporate.AssetManagement.Movement";
    private const int MaxItems = 200;
    private const int MaxReasonNoteLength = 1000;
    private const int MaxReasonLength = 500;
    private const int MaxPageSize = 100;

    private static readonly AssetCondition[] ReceivableConditions =
        [AssetCondition.Aktif, AssetCondition.RusakRingan, AssetCondition.RusakBerat, AssetCondition.TidakDigunakan];

    private readonly ApplicationDbContext _context;
    private readonly AssetActorResolver _actorResolver;
    private readonly AssetEntityResolver _entityResolver;
    private readonly AssetNumberingService _numberingService;
    private readonly AssetProcessLockService _lockService;
    private readonly AssetStockTakeGate _stockTakeGate;
    private readonly AssetPlacementService _placementService;
    private readonly AssetConditionService _conditionService;
    private readonly AssetMovementDocumentService _documentService;
    private readonly AccessPermissionService _accessPermissionService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LoggerService _loggerService;
    private bool? _canApproveAsset;

    public AssetTransferService(
        ApplicationDbContext context,
        AssetActorResolver actorResolver,
        AssetEntityResolver entityResolver,
        AssetNumberingService numberingService,
        AssetProcessLockService lockService,
        AssetStockTakeGate stockTakeGate,
        AssetPlacementService placementService,
        AssetConditionService conditionService,
        AssetMovementDocumentService documentService,
        AccessPermissionService accessPermissionService,
        IHttpContextAccessor httpContextAccessor,
        LoggerService loggerService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _actorResolver = actorResolver ?? throw new ArgumentNullException(nameof(actorResolver));
        _entityResolver = entityResolver ?? throw new ArgumentNullException(nameof(entityResolver));
        _numberingService = numberingService ?? throw new ArgumentNullException(nameof(numberingService));
        _lockService = lockService ?? throw new ArgumentNullException(nameof(lockService));
        _stockTakeGate = stockTakeGate ?? throw new ArgumentNullException(nameof(stockTakeGate));
        _placementService = placementService ?? throw new ArgumentNullException(nameof(placementService));
        _conditionService = conditionService ?? throw new ArgumentNullException(nameof(conditionService));
        _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
        _accessPermissionService = accessPermissionService ?? throw new ArgumentNullException(nameof(accessPermissionService));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
    }

    public async Task<AssetTransferResponse> CreateAsync(AssetTransferCreateRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);

        if (request.Reason is not { } reason || reason is not (TransferReason.KebutuhanLayanan or TransferReason.KoreksiMutasi or TransferReason.Lainnya))
        {
            throw AssetManagementException.Invalid(AssetMovementErrorCodes.ReasonNote, AssetMovementMessages.ReasonManualOnly);
        }

        var note = RequiredNote(request.ReasonNote);
        var assetIds = (request.AssetIds ?? []).Where(x => x != Guid.Empty).Distinct().ToList();
        if (assetIds.Count is < 1 or > MaxItems)
        {
            throw AssetManagementException.Invalid(AssetMovementErrorCodes.ItemCount, AssetMovementMessages.ItemCount);
        }

        if (request.TargetOrganizationUnitId is not { } targetUnitId || targetUnitId == Guid.Empty)
        {
            throw AssetManagementException.Invalid(AssetMovementErrorCodes.Field, AssetMovementMessages.TargetUnitRequired);
        }

        await EnsureUnitActiveAsync(targetUnitId, cancellationToken);
        var targetLocationId = NullIfEmpty(request.TargetLocationId);
        var targetCustodianId = NullIfEmpty(request.TargetCustodianEmployeeId);
        await EnsureOptionalTargetAsync(targetUnitId, targetLocationId, targetCustodianId, cancellationToken);

        Guid? correctsTransferId = null;
        if (reason == TransferReason.KoreksiMutasi)
        {
            correctsTransferId = await RequireCorrectableTransferAsync(request.CorrectsTransferId, cancellationToken);
        }

        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var created = await InTransactionAsync(async () =>
        {
            var assets = await LockAssetsAsync(assetIds, entity.LegalEntityId, cancellationToken);
            var sourceUnitId = EnsureEligibleAndSingleSource(assets, targetUnitId);
            actor.EnsureCanAccessUnit(sourceUnitId);
            await AssetMovementStockTake.EnsureNotHeldAsync(_stockTakeGate, [sourceUnitId, targetUnitId], cancellationToken);

            var now = DateTime.UtcNow;
            var autoUnitApproval = actor.IsHeadOfUnit(sourceUnitId);
            var transfer = new AstTransfer
            {
                TransferNumber = await _numberingService.AllocateDocumentNumberAsync(AssetDocumentNumberSeries.Transfer, actor.UserId, cancellationToken),
                SourceOrganizationUnitId = sourceUnitId,
                TargetOrganizationUnitId = targetUnitId,
                TargetLocationId = targetLocationId,
                TargetCustodianEmployeeId = targetCustodianId,
                Reason = reason,
                ReasonNote = note,
                CorrectsTransferId = correctsTransferId,
                Status = autoUnitApproval ? TransferStatus.DisetujuiUnitAsal : TransferStatus.Diajukan,
                RequestedBy = actor.UserId,
                RequestedAt = now,
                UnitApprovedBy = autoUnitApproval ? actor.UserId : null,
                UnitApprovedAt = autoUnitApproval ? now : null,
                IsUnitApprovalByRequester = autoUnitApproval,
                CreateBy = actor.UserId,
                CreateDateTime = now
            };

            AddItems(transfer, assets, actor.UserId, now);
            _context.AstTransfers.Add(transfer);
            await _context.SaveChangesAsync(cancellationToken);
            return (transfer.Id, transfer.TransferNumber, autoUnitApproval);
        }, cancellationToken);

        await AuditAsync("AssetTransfer.Create", $"Mutasi {created.TransferNumber} ({assetIds.Count} aset) diajukan oleh {actor.UserId}{(created.autoUnitApproval ? ", tahap unit asal otomatis" : string.Empty)}.");
        return await GetTransferAsync(created.Id, cancellationToken);
    }

    public async Task<Guid> CreateFromStockTakeAsync(AssetTransferStockTakeRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.ActorUserId == Guid.Empty || request.SourceStockTakeResultId == Guid.Empty || request.AssetId == Guid.Empty)
        {
            throw new ArgumentException("Aset, hasil opname, dan pelaku mutasi koreksi wajib diisi.", nameof(request));
        }

        var note = RequiredNote(request.ReasonNote);
        await EnsureUnitActiveAsync(request.TargetOrganizationUnitId, cancellationToken);
        await EnsureOptionalTargetAsync(request.TargetOrganizationUnitId, NullIfEmpty(request.TargetLocationId), NullIfEmpty(request.TargetCustodianEmployeeId), cancellationToken);

        var assets = await LockAssetsAsync([request.AssetId], null, cancellationToken);
        var sourceUnitId = EnsureEligibleAndSingleSource(assets, request.TargetOrganizationUnitId);

        var now = DateTime.UtcNow;
        var transfer = new AstTransfer
        {
            TransferNumber = await _numberingService.AllocateDocumentNumberAsync(AssetDocumentNumberSeries.Transfer, request.ActorUserId, cancellationToken),
            SourceOrganizationUnitId = sourceUnitId,
            TargetOrganizationUnitId = request.TargetOrganizationUnitId,
            TargetLocationId = NullIfEmpty(request.TargetLocationId),
            TargetCustodianEmployeeId = NullIfEmpty(request.TargetCustodianEmployeeId),
            Reason = TransferReason.KoreksiOpname,
            ReasonNote = note,
            SourceStockTakeResultId = request.SourceStockTakeResultId,
            Status = TransferStatus.Diajukan,
            RequestedBy = request.ActorUserId,
            RequestedAt = now,
            CreateBy = request.ActorUserId,
            CreateDateTime = now
        };

        AddItems(transfer, assets, request.ActorUserId, now);
        _context.AstTransfers.Add(transfer);
        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync("AssetTransfer.CreateFromStockTake", $"Mutasi koreksi opname {transfer.TransferNumber} dibuat untuk hasil {request.SourceStockTakeResultId}.");
        return transfer.Id;
    }

    public async Task<AssetTransferResponse> ApproveUnitAsync(Guid id, AssetTransferDecisionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var reason = request.Approve ? OptionalReason(request.Reason) : RequiredReason(request.Reason);

        var number = await InTransactionAsync(async () =>
        {
            var transfer = await LockTransferAsync(id, cancellationToken);
            EnsureVersion(transfer, request.RowVersion);
            EnsureStatus(transfer, TransferStatus.Diajukan);
            if (!actor.IsHeadOfUnit(transfer.SourceOrganizationUnitId))
            {
                throw AssetManagementException.Forbidden(
                    AssetMovementErrorCodes.UnitApprover,
                    string.Format(AssetMovementMessages.UnitApproverFormat, await UnitNameAsync(transfer.SourceOrganizationUnitId, cancellationToken)));
            }

            EnsureNotRequester(transfer, actor);
            var now = DateTime.UtcNow;
            if (request.Approve)
            {
                transfer.Status = TransferStatus.DisetujuiUnitAsal;
                transfer.UnitApprovedBy = actor.UserId;
                transfer.UnitApprovedAt = now;
                transfer.DecisionReason = reason;
            }
            else
            {
                await RejectAsync(transfer, actor.UserId, reason!, now, cancellationToken);
            }

            Touch(transfer, actor.UserId, now);
            await _context.SaveChangesAsync(cancellationToken);
            return transfer.TransferNumber;
        }, cancellationToken);

        await AuditAsync(request.Approve ? "AssetTransfer.ApproveUnit" : "AssetTransfer.RejectUnit", $"Mutasi {number} {(request.Approve ? "disetujui" : "ditolak")} Kepala Unit asal {actor.UserId}.");
        return await GetTransferAsync(id, cancellationToken);
    }

    public async Task<AssetTransferResponse> ApproveAssetAsync(Guid id, AssetTransferDecisionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var reason = request.Approve ? OptionalReason(request.Reason) : RequiredReason(request.Reason);

        var number = await InTransactionAsync(async () =>
        {
            var transfer = await LockTransferAsync(id, cancellationToken);
            EnsureVersion(transfer, request.RowVersion);
            EnsureStatus(transfer, TransferStatus.DisetujuiUnitAsal);
            EnsureNotRequester(transfer, actor);

            var now = DateTime.UtcNow;
            if (request.Approve)
            {
                transfer.Status = TransferStatus.DisetujuiBagianAset;
                transfer.AssetHeadApprovedBy = actor.UserId;
                transfer.AssetHeadApprovedAt = now;
                transfer.DecisionReason = reason;
            }
            else
            {
                await RejectAsync(transfer, actor.UserId, reason!, now, cancellationToken);
            }

            Touch(transfer, actor.UserId, now);
            await _context.SaveChangesAsync(cancellationToken);
            return transfer.TransferNumber;
        }, cancellationToken);

        await AuditAsync(request.Approve ? "AssetTransfer.ApproveAsset" : "AssetTransfer.RejectAsset", $"Mutasi {number} {(request.Approve ? "disetujui" : "ditolak")} Bagian Aset {actor.UserId}.");
        return await GetTransferAsync(id, cancellationToken);
    }

    public async Task<AssetTransferResponse> ReceiveAsync(Guid id, AssetTransferReceiveRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);

        var result = await InTransactionAsync(async () =>
        {
            var transfer = await LockTransferAsync(id, cancellationToken);
            EnsureVersion(transfer, request.RowVersion);
            EnsureStatus(transfer, TransferStatus.DisetujuiBagianAset);
            await EnsureReceiverAsync(transfer, actor, cancellationToken);

            var locationId = NullIfEmpty(request.TargetLocationId) ?? transfer.TargetLocationId;
            var custodianId = NullIfEmpty(request.TargetCustodianEmployeeId) ?? transfer.TargetCustodianEmployeeId;
            if (locationId is not { } location || custodianId is not { } custodian)
            {
                throw AssetManagementException.Invalid(AssetMovementErrorCodes.ReceiveTarget, AssetMovementMessages.ReceiveTarget);
            }

            await EnsureReceiveTargetAsync(transfer.TargetOrganizationUnitId, location, custodian, cancellationToken);
            var target = new AssetPlacementTarget(transfer.TargetOrganizationUnitId, location, custodian);

            var items = await _context.AstTransferItems.Where(x => x.TransferId == transfer.Id && !x.IsDelete).ToListAsync(cancellationToken);
            var received = ReceivedConditions(request.Items, items);
            var assets = await LockAssetsAsync(items.Select(x => x.AssetId).ToList(), null, cancellationToken);
            var now = DateTime.UtcNow;
            var openedReports = new List<string>();

            foreach (var asset in assets)
            {
                var item = items.First(x => x.AssetId == asset.Id);
                item.ConditionAtReceipt = received.TryGetValue(asset.Id, out var condition) ? condition : item.ConditionAtSubmit;
                item.UpdateBy = actor.UserId;
                item.UpdateDateTime = now;

                await _placementService.ApplyPlacementAsync(
                    asset,
                    target,
                    PlacementSourceTransfer,
                    TransferDocumentType,
                    transfer.Id,
                    transfer.TransferNumber,
                    AssetHistoryEventType.Dimutasi,
                    AssetSubModules.Movement,
                    actor.UserId,
                    now,
                    cancellationToken);
                _lockService.Release(asset, AssetActiveProcessKind.Mutasi, transfer.Id);
            }

            transfer.TargetLocationId = location;
            transfer.TargetCustodianEmployeeId = custodian;
            transfer.Status = TransferStatus.Diserahterimakan;
            transfer.ReceivedBy = actor.UserId;
            transfer.ReceivedAt = now;
            Touch(transfer, actor.UserId, now);
            await _context.SaveChangesAsync(cancellationToken);

            foreach (var item in items.Where(x => x.ConditionAtReceipt is { } atReceipt && atReceipt != x.ConditionAtSubmit))
            {
                var report = await _conditionService.OpenReportAsync(
                    new AssetConditionOpenRequest(
                        item.AssetId,
                        ConditionReportSource.PenerimaanMutasi,
                        item.ConditionAtReceipt!.Value,
                        transfer.Id,
                        $"Kondisi saat diterima pada mutasi {transfer.TransferNumber} berbeda dari saat diajukan.",
                        actor.UserId),
                    cancellationToken);
                openedReports.Add(report.ReportNumber);
            }

            return (transfer.TransferNumber, Reports: openedReports);
        }, cancellationToken);

        await AuditAsync("AssetTransfer.Receive", $"Mutasi {result.TransferNumber} diterima oleh {actor.UserId}; laporan kondisi dibuka: {(result.Reports.Count == 0 ? "-" : string.Join(", ", result.Reports))}.");
        return await GetTransferAsync(id, cancellationToken);
    }

    public async Task<AssetTransferResponse> RejectReceiptAsync(Guid id, AssetTransferReasonRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var reason = RequiredReason(request.Reason);

        var number = await InTransactionAsync(async () =>
        {
            var transfer = await LockTransferAsync(id, cancellationToken);
            EnsureVersion(transfer, request.RowVersion);
            EnsureStatus(transfer, TransferStatus.DisetujuiBagianAset);
            await EnsureReceiverAsync(transfer, actor, cancellationToken);

            var now = DateTime.UtcNow;
            await RejectAsync(transfer, actor.UserId, reason, now, cancellationToken);
            Touch(transfer, actor.UserId, now);
            await _context.SaveChangesAsync(cancellationToken);
            return transfer.TransferNumber;
        }, cancellationToken);

        await AuditAsync("AssetTransfer.RejectReceipt", $"Mutasi {number} ditolak saat serah terima oleh {actor.UserId}.");
        return await GetTransferAsync(id, cancellationToken);
    }

    public async Task<AssetTransferResponse> CancelAsync(Guid id, AssetTransferReasonRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var reason = RequiredReason(request.Reason);
        var canApproveAsset = await CanApproveAssetAsync();

        var number = await InTransactionAsync(async () =>
        {
            var transfer = await LockTransferAsync(id, cancellationToken);
            EnsureVersion(transfer, request.RowVersion);
            if (transfer.Status is not (TransferStatus.Diajukan or TransferStatus.DisetujuiUnitAsal or TransferStatus.DisetujuiBagianAset))
            {
                throw StatusConflict(transfer.Status);
            }

            if (!CanCancel(transfer.RequestedBy, actor, canApproveAsset))
            {
                throw AssetManagementException.Forbidden(AssetMovementErrorCodes.OutsideScope, AssetMovementMessages.CancelScope);
            }

            var now = DateTime.UtcNow;
            await ReleaseLocksAsync(transfer, actor.UserId, now, cancellationToken);
            transfer.Status = TransferStatus.Dibatalkan;
            transfer.CancelledBy = actor.UserId;
            transfer.CancelledAt = now;
            transfer.DecisionReason = reason;
            Touch(transfer, actor.UserId, now);
            await _context.SaveChangesAsync(cancellationToken);
            return transfer.TransferNumber;
        }, cancellationToken);

        await AuditAsync("AssetTransfer.Cancel", $"Mutasi {number} dibatalkan oleh {actor.UserId}.");
        return await GetTransferAsync(id, cancellationToken);
    }

    private void AddItems(AstTransfer transfer, List<AstAsset> assets, Guid actorUserId, DateTime now)
    {
        foreach (var asset in assets)
        {
            _lockService.Acquire(asset, AssetActiveProcessKind.Mutasi, transfer.Id);
            TouchAsset(asset, actorUserId, now);
            transfer.Items.Add(new AstTransferItem
            {
                TransferId = transfer.Id,
                AssetId = asset.Id,
                ConditionAtSubmit = asset.Condition,
                CreateBy = actorUserId,
                CreateDateTime = now
            });
        }
    }

    private async Task RejectAsync(AstTransfer transfer, Guid actorUserId, string reason, DateTime now, CancellationToken cancellationToken)
    {
        await ReleaseLocksAsync(transfer, actorUserId, now, cancellationToken);
        transfer.Status = TransferStatus.Ditolak;
        transfer.RejectedBy = actorUserId;
        transfer.RejectedAt = now;
        transfer.DecisionReason = reason;
    }

    private async Task ReleaseLocksAsync(AstTransfer transfer, Guid actorUserId, DateTime now, CancellationToken cancellationToken)
    {
        var assetIds = await _context.AstTransferItems
            .AsNoTracking()
            .Where(x => x.TransferId == transfer.Id && !x.IsDelete)
            .Select(x => x.AssetId)
            .ToListAsync(cancellationToken);
        var assets = await LockAssetsAsync(assetIds, null, cancellationToken);
        foreach (var asset in assets)
        {
            _lockService.Release(asset, AssetActiveProcessKind.Mutasi, transfer.Id);
            TouchAsset(asset, actorUserId, now);
        }
    }

    private static Guid EnsureEligibleAndSingleSource(List<AstAsset> assets, Guid targetUnitId)
    {
        foreach (var asset in assets)
        {
            var label = asset.InventoryNumber ?? asset.AssetName;
            if (asset.RecordStatus != AssetRecordStatus.Terdaftar || asset.PlacementState != AssetPlacementState.Ditempatkan || !asset.CurrentOrganizationUnitId.HasValue)
            {
                throw AssetManagementException.Conflict(AssetMovementErrorCodes.AssetNotPlaced, string.Format(AssetMovementMessages.AssetNotPlacedFormat, label));
            }

            if (asset.Condition is AssetCondition.Hilang or AssetCondition.RusakBerat)
            {
                throw AssetManagementException.Conflict(
                    AssetMovementErrorCodes.AssetCondition,
                    string.Format(AssetMovementMessages.AssetConditionFormat, label, AssetRegisterLabels.Condition(asset.Condition)));
            }

            if (AssetProcessLockService.IsLocked(asset))
            {
                throw AssetManagementException.Conflict(
                    AssetMovementErrorCodes.ProcessLocked,
                    string.Format(AssetMovementMessages.ProcessLockedFormat, label, AssetProcessLockService.ProcessName(asset.ActiveProcessKind).ToLowerInvariant()));
            }
        }

        var sourceUnits = assets.Select(x => x.CurrentOrganizationUnitId!.Value).Distinct().ToList();
        if (sourceUnits.Count != 1)
        {
            throw AssetManagementException.Invalid(AssetMovementErrorCodes.SingleSourceUnit, AssetMovementMessages.SingleSourceUnit);
        }

        if (sourceUnits[0] == targetUnitId)
        {
            throw AssetManagementException.Invalid(AssetMovementErrorCodes.SameUnit, AssetMovementMessages.SameUnit);
        }

        return sourceUnits[0];
    }

    private static Dictionary<Guid, AssetCondition> ReceivedConditions(List<AssetTransferReceiveItemRequest>? requested, List<AstTransferItem> items)
    {
        var result = new Dictionary<Guid, AssetCondition>();
        foreach (var row in requested ?? [])
        {
            if (!items.Any(x => x.AssetId == row.AssetId))
            {
                throw AssetManagementException.Invalid(AssetMovementErrorCodes.Field, AssetMovementMessages.ReceiveItems);
            }

            if (row.ConditionAtReceipt is not { } condition)
            {
                continue;
            }

            if (!ReceivableConditions.Contains(condition))
            {
                throw AssetManagementException.Invalid(AssetMovementErrorCodes.Field, AssetMovementMessages.ReceiveCondition);
            }

            result[row.AssetId] = condition;
        }

        return result;
    }

    private async Task EnsureReceiverAsync(AstTransfer transfer, AssetActor actor, CancellationToken cancellationToken)
    {
        if (!actor.IsHeadOfUnit(transfer.TargetOrganizationUnitId))
        {
            throw AssetManagementException.Forbidden(
                AssetMovementErrorCodes.Receiver,
                string.Format(AssetMovementMessages.ReceiverFormat, await UnitNameAsync(transfer.TargetOrganizationUnitId, cancellationToken)));
        }
    }

    private static void EnsureNotRequester(AstTransfer transfer, AssetActor actor)
    {
        if (transfer.RequestedBy == actor.UserId)
        {
            throw AssetManagementException.Forbidden(AssetMovementErrorCodes.SelfApproval, AssetMovementMessages.SelfApproval);
        }
    }

    private static void EnsureStatus(AstTransfer transfer, TransferStatus expected)
    {
        if (transfer.Status != expected)
        {
            throw StatusConflict(transfer.Status);
        }
    }

    private static AssetManagementException StatusConflict(TransferStatus status) =>
        AssetManagementException.Conflict(
            AssetMovementErrorCodes.Status,
            string.Format(AssetMovementMessages.StatusFormat, AssetMovementLabels.TransferStatus(status).ToLowerInvariant()));

    private static void EnsureVersion(AstTransfer transfer, Guid? rowVersion)
    {
        if (rowVersion is not { } version || version == Guid.Empty)
        {
            throw AssetManagementException.Invalid(AssetMovementErrorCodes.Field, AssetMovementMessages.RowVersionRequired);
        }

        if (transfer.RowVersion != version)
        {
            throw AssetManagementException.Conflict(AssetMovementErrorCodes.VersionMismatch, AssetMovementMessages.VersionMismatch);
        }
    }

    private async Task EnsureUnitActiveAsync(Guid unitId, CancellationToken cancellationToken)
    {
        var active = await _context.MstOrganizationUnits.AsNoTracking().AnyAsync(x => x.Id == unitId && x.IsActive && !x.IsDelete, cancellationToken);
        if (!active)
        {
            throw AssetManagementException.Invalid(AssetMovementErrorCodes.Field, AssetMovementMessages.TargetUnitRequired);
        }
    }

    private async Task EnsureOptionalTargetAsync(Guid unitId, Guid? locationId, Guid? custodianId, CancellationToken cancellationToken)
    {
        if (locationId is { } location &&
            !await _context.MstAssetLocations.AsNoTracking().AnyAsync(x => x.Id == location && x.IsActive && !x.IsDelete && x.OrganizationUnitId == unitId, cancellationToken))
        {
            throw AssetManagementException.Invalid(AssetMovementErrorCodes.ReceiveTarget, AssetMovementMessages.ReceiveTarget);
        }

        if (custodianId is { } custodian &&
            !await _context.MstEmployees.AsNoTracking().AnyAsync(x => x.Id == custodian && x.IsActive && !x.IsDelete, cancellationToken))
        {
            throw AssetManagementException.Invalid(AssetMovementErrorCodes.ReceiveTarget, AssetMovementMessages.ReceiveTarget);
        }
    }

    private async Task EnsureReceiveTargetAsync(Guid unitId, Guid locationId, Guid custodianId, CancellationToken cancellationToken)
    {
        await EnsureOptionalTargetAsync(unitId, locationId, custodianId, cancellationToken);
    }

    private async Task<Guid> RequireCorrectableTransferAsync(Guid? correctsTransferId, CancellationToken cancellationToken)
    {
        if (correctsTransferId is not { } id || id == Guid.Empty ||
            !await _context.AstTransfers.AsNoTracking().AnyAsync(x => x.Id == id && !x.IsDelete && x.Status == TransferStatus.Diserahterimakan, cancellationToken))
        {
            throw AssetManagementException.Invalid(AssetMovementErrorCodes.ReasonNote, AssetMovementMessages.CorrectsTransferRequired);
        }

        return id;
    }

    private async Task<List<AstAsset>> LockAssetsAsync(List<Guid> ids, Guid? legalEntityId, CancellationToken cancellationToken)
    {
        var ordered = ids.Distinct().OrderBy(x => x).ToArray();
        var assets = await _context.AstAssets
            .FromSqlInterpolated($"SELECT * FROM public.\"AstAsset\" WHERE \"Id\" = ANY({ordered}) AND \"IsDelete\" = false ORDER BY \"Id\" FOR UPDATE")
            .ToListAsync(cancellationToken);

        if (assets.Count != ordered.Length || (legalEntityId.HasValue && assets.Any(x => x.LegalEntityId != legalEntityId.Value)))
        {
            throw AssetManagementException.NotFound(AssetRegisterErrorCodes.NotFound, AssetRegisterMessages.NotFound);
        }

        return assets;
    }

    private async Task<AstTransfer> LockTransferAsync(Guid id, CancellationToken cancellationToken) =>
        await _context.AstTransfers
            .FromSqlInterpolated($"SELECT * FROM public.\"AstTransfer\" WHERE \"Id\" = {id} AND \"IsDelete\" = false FOR UPDATE")
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw TransferNotFound();

    private async Task<string> UnitNameAsync(Guid unitId, CancellationToken cancellationToken) =>
        await _context.MstOrganizationUnits.AsNoTracking().Where(x => x.Id == unitId).Select(x => x.UnitName).FirstOrDefaultAsync(cancellationToken) ?? "-";

    private async Task<bool> CanApproveAssetAsync()
    {
        if (_canApproveAsset.HasValue)
        {
            return _canApproveAsset.Value;
        }

        var user = _httpContextAccessor.HttpContext?.User;
        _canApproveAsset = user != null && await _accessPermissionService.HasAccessAsync(user, PermissionResource, ApproveAssetAction);
        return _canApproveAsset.Value;
    }

    private static bool CanCancel(Guid requestedBy, AssetActor actor, bool canApproveAsset) =>
        requestedBy == actor.UserId || canApproveAsset;

    private async Task<AssetActor> RequireActorAsync(CancellationToken cancellationToken)
    {
        var actor = await _actorResolver.GetCurrentAsync(cancellationToken);
        if (!actor.IsAuthenticated)
        {
            throw AssetManagementException.Forbidden(AssetRegisterErrorCodes.OutsideUnit, AssetRegisterMessages.OutsideUnit);
        }

        return actor;
    }

    private async Task<T> InTransactionAsync<T>(Func<Task<T>> work, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await work();
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch (DbUpdateConcurrencyException exception)
        {
            _context.ChangeTracker.Clear();
            throw AssetManagementException.Conflict(AssetMovementErrorCodes.VersionMismatch, AssetMovementMessages.VersionMismatch, exception);
        }
        catch
        {
            _context.ChangeTracker.Clear();
            throw;
        }
    }

    private static string RequiredNote(string? value)
    {
        var note = value?.Trim();
        if (string.IsNullOrEmpty(note) || note.Length > MaxReasonNoteLength)
        {
            throw AssetManagementException.Invalid(AssetMovementErrorCodes.ReasonNote, AssetMovementMessages.ReasonNote);
        }

        return note;
    }

    private static string RequiredReason(string? value)
    {
        var reason = value?.Trim();
        if (string.IsNullOrEmpty(reason) || reason.Length > MaxReasonLength)
        {
            throw AssetManagementException.Invalid(AssetMovementErrorCodes.ReasonRequired, AssetMovementMessages.ReasonRequired);
        }

        return reason;
    }

    private static string? OptionalReason(string? value)
    {
        var reason = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        if (reason is { Length: > MaxReasonLength })
        {
            throw AssetManagementException.Invalid(AssetMovementErrorCodes.ReasonRequired, AssetMovementMessages.ReasonRequired);
        }

        return reason;
    }

    private static Guid? NullIfEmpty(Guid? value) => value is { } id && id != Guid.Empty ? id : null;

    private static void Touch(AstTransfer transfer, Guid actorUserId, DateTime now)
    {
        transfer.UpdateBy = actorUserId;
        transfer.UpdateDateTime = now;
        transfer.RowVersion = Guid.NewGuid();
    }

    private static void TouchAsset(AstAsset asset, Guid actorUserId, DateTime now)
    {
        asset.UpdateBy = actorUserId;
        asset.UpdateDateTime = now;
        asset.RowVersion = Guid.NewGuid();
    }

    private Task AuditAsync(string action, string message) =>
        _loggerService.AuditAsync(LogCategory, action, message);

    private static AssetManagementException TransferNotFound() =>
        AssetManagementException.NotFound(AssetMovementErrorCodes.NotFound, AssetMovementMessages.NotFound);
}
