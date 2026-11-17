using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

public sealed partial class AssetRecordService
{
    public async Task<AssetDetailResponse> ApproveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var actor = await RequireActorAsync(cancellationToken);
        var inventoryNumber = await InTransactionAsync(() => ApproveCoreAsync(id, actor, cancellationToken), cancellationToken);
        await AuditAsync("AssetRegister.Approve", $"Aset {id} disetujui oleh {actor.UserId}; nomor {inventoryNumber}.");
        return await GetAssetAsync(id, cancellationToken);
    }

    public async Task<List<AssetDecisionResultResponse>> ApproveBatchAsync(AssetBatchApproveRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);

        var ids = (request.AssetIds ?? []).Where(x => x != Guid.Empty).Distinct().ToList();
        if (ids.Count is < 1 or > MaxBatchSize)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.Field, AssetRegisterMessages.BatchSize);
        }

        var results = new List<AssetDecisionResultResponse>(ids.Count);
        foreach (var id in ids)
        {
            try
            {
                var inventoryNumber = await InTransactionAsync(() => ApproveCoreAsync(id, actor, cancellationToken), cancellationToken);
                _context.ChangeTracker.Clear();
                results.Add(new AssetDecisionResultResponse
                {
                    AssetId = id,
                    IsSuccess = true,
                    InventoryNumber = inventoryNumber,
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Aset disetujui."
                });
            }
            catch (AssetManagementException exception)
            {
                results.Add(new AssetDecisionResultResponse
                {
                    AssetId = id,
                    IsSuccess = false,
                    StatusCode = exception.StatusCode,
                    Code = exception.Code,
                    Message = exception.Message
                });
            }
        }

        var approved = results.Count(x => x.IsSuccess);
        await AuditAsync("AssetRegister.ApproveBatch", $"Persetujuan massal oleh {actor.UserId}: {approved} dari {results.Count} aset disetujui.");
        return results;
    }

    public async Task<AssetDetailResponse> RejectAsync(Guid id, AssetDecisionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var reason = RequiredReason(request.Reason);

        await InTransactionAsync(async () =>
        {
            var asset = await LockAssetAsync(id, actor, cancellationToken);
            EnsurePendingDecision(asset);
            EnsureNotSubmitter(asset, actor);

            var now = DateTime.UtcNow;
            asset.RecordStatus = AssetRecordStatus.Draft;
            asset.RejectedBy = actor.UserId;
            asset.RejectedAt = now;
            asset.DecisionReason = reason;
            Touch(asset, actor.UserId, now);
            await _context.SaveChangesAsync(cancellationToken);
            return asset.Id;
        }, cancellationToken);

        await AuditAsync("AssetRegister.Reject", $"Aset {id} dikembalikan ke draft oleh {actor.UserId}.");
        return await GetAssetAsync(id, cancellationToken);
    }

    private async Task<string> ApproveCoreAsync(Guid id, AssetActor actor, CancellationToken cancellationToken)
    {
        var asset = await LockAssetAsync(id, actor, cancellationToken);
        EnsurePendingDecision(asset);
        EnsureNotSubmitter(asset, actor);

        var group = await _context.MstAssetGroups
            .AsNoTracking()
            .Where(x => x.Id == asset.AssetGroupId)
            .Select(x => new { x.InventoryCode, x.IsActive, x.IsDelete })
            .FirstAsync(cancellationToken);

        if (!group.IsActive || group.IsDelete)
        {
            throw AssetManagementException.Conflict(AssetRegisterErrorCodes.GroupInactiveAtApproval, AssetRegisterMessages.GroupInactiveAtApproval);
        }

        var now = DateTime.UtcNow;
        await _parameterQuery.GetRequiredParameterAtAsync(asset.LegalEntityId, FinanceBusinessDate.ToDateOnly(now), cancellationToken);

        var inventoryNumber = await _numberingService.AllocateInventoryNumberAsync(group.InventoryCode, actor.UserId, cancellationToken);

        asset.InventoryNumber = inventoryNumber;
        asset.RecordStatus = AssetRecordStatus.Terdaftar;
        asset.Condition = AssetCondition.Aktif;
        asset.ConditionChangedAt = now;
        asset.PlacementState = AssetPlacementState.BelumDitempatkan;
        asset.ApprovedBy = actor.UserId;
        asset.ApprovedAt = now;
        asset.DecisionReason = null;
        Touch(asset, actor.UserId, now);

        await _bookService.CreateForRegisteredAssetAsync(asset, actor.UserId, now, cancellationToken);

        _historyService.Append(new AssetHistoryEntry(
            asset.Id,
            AssetHistoryEventType.Didaftarkan,
            AssetSubModules.Register,
            actor.UserId,
            FromValue: AssetRegisterLabels.RecordStatus(AssetRecordStatus.Diajukan),
            ToValue: AssetRegisterLabels.RecordStatus(AssetRecordStatus.Terdaftar),
            SourceDocumentType: AssetDocumentType,
            SourceDocumentId: asset.Id,
            SourceDocumentNumber: inventoryNumber,
            OccurredAt: now));

        await _context.SaveChangesAsync(cancellationToken);
        return inventoryNumber;
    }

    private static void EnsurePendingDecision(AstAsset asset)
    {
        if (asset.RecordStatus == AssetRecordStatus.Diajukan)
        {
            return;
        }

        if (asset.RecordStatus is AssetRecordStatus.Terdaftar || asset.RejectedAt.HasValue)
        {
            throw AssetManagementException.Conflict(AssetRegisterErrorCodes.VersionMismatch, AssetRegisterMessages.VersionMismatch);
        }

        EnsureStatus(asset, AssetRecordStatus.Diajukan, "diputuskan");
    }

    private static void EnsureNotSubmitter(AstAsset asset, AssetActor actor)
    {
        if (asset.SubmittedBy == actor.UserId)
        {
            throw AssetManagementException.Forbidden(AssetRegisterErrorCodes.SelfApproval, AssetRegisterMessages.SelfApproval);
        }
    }
}
