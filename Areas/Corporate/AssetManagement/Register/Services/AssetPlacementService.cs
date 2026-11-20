using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Services;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

public sealed partial class AssetPlacementService
{
    public const string PlacementSourceHandover = "PENYERAHAN";
    public const string HandoverDocumentType = "AstAssetHandover";
    private const string LogCategory = "Corporate.AssetManagement.Placement";
    private const int MaxHandoverItems = 200;
    private const int MaxReasonLength = 500;
    private const int MaxNotesLength = 1000;
    private const int MaxPageSize = 100;

    private readonly ApplicationDbContext _context;
    private readonly AssetActorResolver _actorResolver;
    private readonly AssetEntityResolver _entityResolver;
    private readonly AssetNumberingService _numberingService;
    private readonly AssetProcessLockService _lockService;
    private readonly AssetHistoryService _historyService;
    private readonly AssetStockTakeGate _stockTakeGate;
    private readonly AssetLabelDocumentService _documentService;
    private readonly LoggerService _loggerService;

    public AssetPlacementService(
        ApplicationDbContext context,
        AssetActorResolver actorResolver,
        AssetEntityResolver entityResolver,
        AssetNumberingService numberingService,
        AssetProcessLockService lockService,
        AssetHistoryService historyService,
        AssetStockTakeGate stockTakeGate,
        AssetLabelDocumentService documentService,
        LoggerService loggerService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _actorResolver = actorResolver ?? throw new ArgumentNullException(nameof(actorResolver));
        _entityResolver = entityResolver ?? throw new ArgumentNullException(nameof(entityResolver));
        _numberingService = numberingService ?? throw new ArgumentNullException(nameof(numberingService));
        _lockService = lockService ?? throw new ArgumentNullException(nameof(lockService));
        _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        _stockTakeGate = stockTakeGate ?? throw new ArgumentNullException(nameof(stockTakeGate));
        _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
        _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
    }

    public async Task<AssetHandoverResponse> CreateHandoverAsync(AssetHandoverCreateRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);

        var purpose = request.Purpose;
        if (purpose is not (HandoverPurpose.PenempatanAwal or HandoverPurpose.PindahDalamUnit))
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.Field, AssetRegisterMessages.HandoverPurposeManual);
        }

        var assetIds = (request.AssetIds ?? []).Where(x => x != Guid.Empty).Distinct().ToList();
        if (assetIds.Count is < 1 or > MaxHandoverItems)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.Field, AssetRegisterMessages.HandoverBatchSize);
        }

        if (request.TargetOrganizationUnitId is not { } unitId || unitId == Guid.Empty ||
            request.TargetLocationId is not { } locationId || locationId == Guid.Empty ||
            request.TargetCustodianEmployeeId is not { } custodianId || custodianId == Guid.Empty)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.Field, AssetRegisterMessages.HandoverTargetRequired);
        }

        var notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        if (notes is { Length: > MaxNotesLength })
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.Field, AssetRegisterMessages.HandoverNotes);
        }

        var target = new AssetPlacementTarget(unitId, locationId, custodianId);
        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        await EnsureTargetValidAsync(target, cancellationToken);

        var created = await InTransactionAsync(async () =>
        {
            var assets = await LockAssetsAsync(assetIds, entity.LegalEntityId, cancellationToken);
            foreach (var asset in assets)
            {
                actor.EnsureCanAccessUnit(asset.CurrentOrganizationUnitId);
                EnsureHandoverEligible(asset, purpose.Value, target);
            }

            var unitIds = assets
                .Where(x => x.CurrentOrganizationUnitId.HasValue)
                .Select(x => x.CurrentOrganizationUnitId!.Value)
                .Append(target.OrganizationUnitId)
                .Distinct()
                .ToList();
            await _stockTakeGate.EnsureUnitsNotInStockTakeAsync(unitIds, cancellationToken);

            var now = DateTime.UtcNow;
            var number = await _numberingService.AllocateDocumentNumberAsync(AssetDocumentNumberSeries.Handover, actor.UserId, cancellationToken);
            var handover = new AstAssetHandover
            {
                HandoverNumber = number,
                Purpose = purpose.Value,
                Status = HandoverStatus.Diserahkan,
                TargetOrganizationUnitId = target.OrganizationUnitId,
                TargetLocationId = target.LocationId,
                TargetCustodianEmployeeId = target.CustodianEmployeeId,
                Notes = notes,
                HandedOverBy = actor.UserId,
                HandedOverAt = now,
                CreateBy = actor.UserId,
                CreateDateTime = now
            };

            foreach (var asset in assets)
            {
                _lockService.Acquire(asset, AssetActiveProcessKind.Penyerahan, handover.Id);
                Touch(asset, actor.UserId, now);
                handover.Items.Add(new AstAssetHandoverItem
                {
                    HandoverId = handover.Id,
                    AssetId = asset.Id,
                    ConditionAtHandover = asset.Condition,
                    CreateBy = actor.UserId,
                    CreateDateTime = now
                });
            }

            _context.AstAssetHandovers.Add(handover);
            await _context.SaveChangesAsync(cancellationToken);
            return (handover.Id, number);
        }, cancellationToken);

        await AuditAsync("AssetPlacement.Handover", $"Penyerahan {created.number} ({AssetRegisterLabels.HandoverPurpose(purpose.Value)}, {assetIds.Count} aset) dibuat oleh {actor.UserId}.");
        return await GetHandoverAsync(created.Id, cancellationToken);
    }

    public async Task<AssetHandoverResponse> ConfirmAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var actor = await RequireActorAsync(cancellationToken);

        var number = await InTransactionAsync(async () =>
        {
            var handover = await LockHandoverAsync(id, cancellationToken);
            EnsureRecipient(handover, actor);
            EnsurePending(handover, "dikonfirmasi");

            var assets = await LockHandoverAssetsAsync(handover.Id, cancellationToken);
            var target = new AssetPlacementTarget(handover.TargetOrganizationUnitId, handover.TargetLocationId, handover.TargetCustodianEmployeeId);
            var eventType = handover.Purpose == HandoverPurpose.PindahDalamUnit
                ? AssetHistoryEventType.PindahDalamUnit
                : AssetHistoryEventType.Ditempatkan;
            var now = DateTime.UtcNow;

            foreach (var asset in assets)
            {
                await ApplyPlacementAsync(
                    asset,
                    target,
                    PlacementSourceHandover,
                    HandoverDocumentType,
                    handover.Id,
                    handover.HandoverNumber,
                    eventType,
                    AssetSubModules.Register,
                    actor.UserId,
                    now,
                    cancellationToken);
                _lockService.Release(asset, AssetActiveProcessKind.Penyerahan, handover.Id);
            }

            handover.Status = HandoverStatus.Dikonfirmasi;
            handover.ConfirmedBy = actor.UserId;
            handover.ConfirmedAt = now;
            TouchHandover(handover, actor.UserId, now);
            await _context.SaveChangesAsync(cancellationToken);
            return handover.HandoverNumber;
        }, cancellationToken);

        await AuditAsync("AssetPlacement.Confirm", $"Penyerahan {number} dikonfirmasi oleh {actor.UserId}.");
        return await GetHandoverAsync(id, cancellationToken);
    }

    public async Task<AssetHandoverResponse> RejectAsync(Guid id, AssetHandoverDecisionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var reason = RequiredReason(request.Reason);

        var number = await InTransactionAsync(async () =>
        {
            var handover = await LockHandoverAsync(id, cancellationToken);
            EnsureRecipient(handover, actor);
            EnsurePending(handover, "ditolak");
            await ReleaseHandoverLocksAsync(handover, actor.UserId, cancellationToken);

            var now = DateTime.UtcNow;
            handover.Status = HandoverStatus.Ditolak;
            handover.RejectedBy = actor.UserId;
            handover.RejectedAt = now;
            handover.DecisionReason = reason;
            TouchHandover(handover, actor.UserId, now);
            await _context.SaveChangesAsync(cancellationToken);
            return handover.HandoverNumber;
        }, cancellationToken);

        await AuditAsync("AssetPlacement.Reject", $"Penyerahan {number} ditolak penerima {actor.UserId}.");
        return await GetHandoverAsync(id, cancellationToken);
    }

    public async Task<AssetHandoverResponse> CancelAsync(Guid id, AssetHandoverDecisionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var reason = RequiredReason(request.Reason);

        var number = await InTransactionAsync(async () =>
        {
            var handover = await LockHandoverAsync(id, cancellationToken);
            if (!CanCancel(handover.HandedOverBy, actor))
            {
                throw AssetManagementException.Forbidden(AssetRegisterErrorCodes.OutsideUnit, AssetRegisterMessages.HandoverOutsideScope);
            }

            EnsurePending(handover, "dibatalkan");
            await ReleaseHandoverLocksAsync(handover, actor.UserId, cancellationToken);

            var now = DateTime.UtcNow;
            handover.Status = HandoverStatus.Dibatalkan;
            handover.CancelledBy = actor.UserId;
            handover.CancelledAt = now;
            handover.DecisionReason = reason;
            TouchHandover(handover, actor.UserId, now);
            await _context.SaveChangesAsync(cancellationToken);
            return handover.HandoverNumber;
        }, cancellationToken);

        await AuditAsync("AssetPlacement.Cancel", $"Penyerahan {number} dibatalkan oleh {actor.UserId}.");
        return await GetHandoverAsync(id, cancellationToken);
    }

    public async Task ApplyPlacementAsync(
        AstAsset asset,
        AssetPlacementTarget target,
        string sourceType,
        string sourceDocumentType,
        Guid? sourceDocumentId,
        string? sourceDocumentNumber,
        AssetHistoryEventType eventType,
        string subModule,
        Guid actorUserId,
        DateTime at,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(asset);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceType);

        var fromLabel = await DescribePlacementAsync(asset.CurrentOrganizationUnitId, asset.CurrentLocationId, cancellationToken);

        var current = await _context.AstAssetPlacements
            .FirstOrDefaultAsync(x => x.AssetId == asset.Id && x.EndAt == null && !x.IsDelete, cancellationToken);
        if (current != null)
        {
            current.EndAt = at;
            current.UpdateBy = actorUserId;
            current.UpdateDateTime = at;
            await _context.SaveChangesAsync(cancellationToken);
        }

        _context.AstAssetPlacements.Add(new AstAssetPlacement
        {
            AssetId = asset.Id,
            OrganizationUnitId = target.OrganizationUnitId,
            LocationId = target.LocationId,
            CustodianEmployeeId = target.CustodianEmployeeId,
            SourceType = sourceType,
            SourceDocumentId = sourceDocumentId,
            StartAt = at,
            CreateBy = actorUserId,
            CreateDateTime = at
        });

        asset.CurrentOrganizationUnitId = target.OrganizationUnitId;
        asset.CurrentLocationId = target.LocationId;
        asset.CurrentCustodianEmployeeId = target.CustodianEmployeeId;
        asset.PlacementState = AssetPlacementState.Ditempatkan;
        Touch(asset, actorUserId, at);

        var toLabel = await DescribePlacementAsync(target.OrganizationUnitId, target.LocationId, cancellationToken);
        _historyService.Append(new AssetHistoryEntry(
            asset.Id,
            eventType,
            subModule,
            actorUserId,
            FromValue: fromLabel,
            ToValue: toLabel,
            SourceDocumentType: sourceDocumentType,
            SourceDocumentId: sourceDocumentId,
            SourceDocumentNumber: sourceDocumentNumber,
            OccurredAt: at));
    }

    private async Task EnsureTargetValidAsync(AssetPlacementTarget target, CancellationToken cancellationToken)
    {
        var unitValid = await _context.MstOrganizationUnits
            .AsNoTracking()
            .AnyAsync(x => x.Id == target.OrganizationUnitId && x.IsActive && !x.IsDelete, cancellationToken);
        var locationValid = await _context.MstAssetLocations
            .AsNoTracking()
            .AnyAsync(x => x.Id == target.LocationId && x.IsActive && !x.IsDelete && x.OrganizationUnitId == target.OrganizationUnitId, cancellationToken);
        var custodianValid = await _context.MstEmployees
            .AsNoTracking()
            .AnyAsync(x => x.Id == target.CustodianEmployeeId && x.IsActive && !x.IsDelete, cancellationToken);

        if (!unitValid || !locationValid || !custodianValid)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.HandoverTargetInvalid, AssetRegisterMessages.HandoverTargetInvalid);
        }
    }

    private static void EnsureHandoverEligible(AstAsset asset, HandoverPurpose purpose, AssetPlacementTarget target)
    {
        if (asset.RecordStatus != AssetRecordStatus.Terdaftar)
        {
            throw AssetManagementException.Conflict(AssetRegisterErrorCodes.HandoverNotRegistered, AssetRegisterMessages.HandoverNotRegistered);
        }

        if (asset.Condition == AssetCondition.Hilang)
        {
            throw AssetManagementException.Conflict(AssetRegisterErrorCodes.HandoverAssetLost, AssetRegisterMessages.HandoverAssetLost);
        }

        if (purpose == HandoverPurpose.PenempatanAwal && asset.PlacementState != AssetPlacementState.BelumDitempatkan)
        {
            throw AssetManagementException.Conflict(AssetRegisterErrorCodes.HandoverPurposeMismatch, AssetRegisterMessages.HandoverPurposeMismatch);
        }

        if (purpose == HandoverPurpose.PindahDalamUnit && asset.PlacementState != AssetPlacementState.Ditempatkan)
        {
            throw AssetManagementException.Conflict(AssetRegisterErrorCodes.HandoverPurposeMismatch, AssetRegisterMessages.HandoverInitialOnly);
        }

        if (purpose == HandoverPurpose.PindahDalamUnit && asset.CurrentOrganizationUnitId != target.OrganizationUnitId)
        {
            throw AssetManagementException.Conflict(AssetRegisterErrorCodes.HandoverPurposeMismatch, AssetRegisterMessages.HandoverPurposeMismatch);
        }

        if (AssetProcessLockService.IsLocked(asset))
        {
            throw AssetManagementException.Conflict(
                AssetRegisterErrorCodes.ProcessLocked,
                string.Format(
                    AssetRegisterMessages.ProcessLockedFormat,
                    asset.InventoryNumber ?? asset.AssetName,
                    AssetProcessLockService.ProcessName(asset.ActiveProcessKind)));
        }
    }

    private static void EnsureRecipient(AstAssetHandover handover, AssetActor actor)
    {
        if (!IsRecipient(handover.TargetOrganizationUnitId, handover.TargetCustodianEmployeeId, actor))
        {
            throw AssetManagementException.Forbidden(AssetRegisterErrorCodes.HandoverRecipient, AssetRegisterMessages.HandoverRecipient);
        }
    }

    private static bool IsRecipient(Guid targetUnitId, Guid targetCustodianEmployeeId, AssetActor actor) =>
        actor.IsHeadOfUnit(targetUnitId) ||
        (actor.EmployeeId is { } employeeId && employeeId == targetCustodianEmployeeId);

    private static bool CanCancel(Guid handedOverBy, AssetActor actor) =>
        actor.CanReadAllUnits || handedOverBy == actor.UserId;

    private static void EnsurePending(AstAssetHandover handover, string actionLabel)
    {
        if (handover.Status != HandoverStatus.Diserahkan)
        {
            throw AssetManagementException.Conflict(
                AssetRegisterErrorCodes.HandoverStatus,
                string.Format(AssetRegisterMessages.HandoverStatusFormat, AssetRegisterLabels.HandoverStatus(handover.Status).ToLowerInvariant(), actionLabel));
        }
    }

    private async Task ReleaseHandoverLocksAsync(AstAssetHandover handover, Guid actorUserId, CancellationToken cancellationToken)
    {
        var assets = await LockHandoverAssetsAsync(handover.Id, cancellationToken);
        var now = DateTime.UtcNow;
        foreach (var asset in assets)
        {
            _lockService.Release(asset, AssetActiveProcessKind.Penyerahan, handover.Id);
            Touch(asset, actorUserId, now);
        }
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

    private async Task<List<AstAsset>> LockHandoverAssetsAsync(Guid handoverId, CancellationToken cancellationToken)
    {
        var assetIds = await _context.AstAssetHandoverItems
            .AsNoTracking()
            .Where(x => x.HandoverId == handoverId && !x.IsDelete)
            .Select(x => x.AssetId)
            .ToListAsync(cancellationToken);
        return await LockAssetsAsync(assetIds, null, cancellationToken);
    }

    private async Task<AstAssetHandover> LockHandoverAsync(Guid id, CancellationToken cancellationToken) =>
        await _context.AstAssetHandovers
            .FromSqlInterpolated($"SELECT * FROM public.\"AstAssetHandover\" WHERE \"Id\" = {id} AND \"IsDelete\" = false FOR UPDATE")
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw HandoverNotFound();

    private async Task<string> DescribePlacementAsync(Guid? organizationUnitId, Guid? locationId, CancellationToken cancellationToken)
    {
        if (!organizationUnitId.HasValue)
        {
            return AssetRegisterLabels.PlacementState(AssetPlacementState.BelumDitempatkan);
        }

        var unitName = await _context.MstOrganizationUnits
            .AsNoTracking()
            .Where(x => x.Id == organizationUnitId.Value)
            .Select(x => x.UnitName)
            .FirstOrDefaultAsync(cancellationToken);
        var locationName = locationId.HasValue
            ? await _context.MstAssetLocations
                .AsNoTracking()
                .Where(x => x.Id == locationId.Value)
                .Select(x => x.LocationName)
                .FirstOrDefaultAsync(cancellationToken)
            : null;

        var label = string.IsNullOrWhiteSpace(locationName) ? unitName ?? "-" : $"{unitName} / {locationName}";
        return label.Length > 250 ? label[..250] : label;
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
            throw AssetManagementException.Conflict(AssetRegisterErrorCodes.VersionMismatch, AssetRegisterMessages.VersionMismatch, exception);
        }
        catch
        {
            _context.ChangeTracker.Clear();
            throw;
        }
    }

    private static string RequiredReason(string? value)
    {
        var reason = value?.Trim();
        if (string.IsNullOrEmpty(reason) || reason.Length > MaxReasonLength)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.ReasonRequired, AssetRegisterMessages.ReasonRequired);
        }

        return reason;
    }

    private static void Touch(AstAsset asset, Guid actorUserId, DateTime now)
    {
        asset.UpdateBy = actorUserId;
        asset.UpdateDateTime = now;
        asset.RowVersion = Guid.NewGuid();
    }

    private static void TouchHandover(AstAssetHandover handover, Guid actorUserId, DateTime now)
    {
        handover.UpdateBy = actorUserId;
        handover.UpdateDateTime = now;
        handover.RowVersion = Guid.NewGuid();
    }

    private Task AuditAsync(string action, string message) =>
        _loggerService.AuditAsync(LogCategory, action, message);

    private static AssetManagementException HandoverNotFound() =>
        AssetManagementException.NotFound(AssetRegisterErrorCodes.HandoverNotFound, AssetRegisterMessages.HandoverNotFound);
}
