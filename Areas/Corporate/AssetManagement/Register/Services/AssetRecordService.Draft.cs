using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

public sealed partial class AssetRecordService
{
    private const int MinManufactureYear = 1900;

    public async Task<List<AssetListItemResponse>> CreateDraftsAsync(AssetDraftCreateRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);

        if (request.Quantity is < 1 or > MaxDraftQuantity)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.DraftQuantity, AssetRegisterMessages.DraftQuantity);
        }

        var values = await ValidateDraftAsync(request, cancellationToken);
        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var now = DateTime.UtcNow;

        var assets = Enumerable.Range(0, request.Quantity)
            .Select(_ =>
            {
                var asset = new AstAsset
                {
                    LegalEntityId = entity.LegalEntityId,
                    HospitalSiteId = entity.HospitalSiteId,
                    RecordStatus = AssetRecordStatus.Draft,
                    CreateBy = actor.UserId,
                    CreateDateTime = now
                };
                Apply(asset, values);
                return asset;
            })
            .ToList();

        _context.AstAssets.AddRange(assets);
        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync("AssetRegister.CreateDrafts", $"{assets.Count} draft aset dibuat oleh {actor.UserId}; kelompok {values.AssetGroupId}.");

        var ids = assets.Select(x => x.Id).ToList();
        return await ListByIdsAsync(ids, cancellationToken);
    }

    public async Task<AssetDetailResponse> UpdateDraftAsync(Guid id, AssetDraftUpdateRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var values = await ValidateDraftAsync(request, cancellationToken);

        await InTransactionAsync(async () =>
        {
            var asset = await LockAssetAsync(id, actor, cancellationToken);
            if (asset.RecordStatus != AssetRecordStatus.Draft)
            {
                throw AssetManagementException.Conflict(AssetRegisterErrorCodes.DraftOnly, AssetRegisterMessages.DraftOnly);
            }

            if (request.RowVersion != asset.RowVersion)
            {
                throw AssetManagementException.Conflict(AssetRegisterErrorCodes.VersionMismatch, AssetRegisterMessages.VersionMismatch);
            }

            Apply(asset, values);
            Touch(asset, actor.UserId, DateTime.UtcNow);
            await _context.SaveChangesAsync(cancellationToken);
            return asset.Id;
        }, cancellationToken);

        await AuditAsync("AssetRegister.UpdateDraft", $"Draft aset {id} diubah oleh {actor.UserId}.");
        return await GetAssetAsync(id, cancellationToken);
    }

    public async Task<AssetDetailResponse> SubmitAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var actor = await RequireActorAsync(cancellationToken);

        await InTransactionAsync(async () =>
        {
            var asset = await LockAssetAsync(id, actor, cancellationToken);
            EnsureStatus(asset, AssetRecordStatus.Draft, "diajukan");
            await EnsureReadyForSubmissionAsync(asset, cancellationToken);

            var now = DateTime.UtcNow;
            asset.RecordStatus = AssetRecordStatus.Diajukan;
            asset.SubmittedBy = actor.UserId;
            asset.SubmittedAt = now;
            asset.DecisionReason = null;
            Touch(asset, actor.UserId, now);
            await _context.SaveChangesAsync(cancellationToken);
            return asset.Id;
        }, cancellationToken);

        await AuditAsync("AssetRegister.Submit", $"Aset {id} diajukan oleh {actor.UserId}.");
        return await GetAssetAsync(id, cancellationToken);
    }

    public async Task<AssetDetailResponse> CancelAsync(Guid id, AssetDecisionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var reason = RequiredReason(request.Reason);

        await InTransactionAsync(async () =>
        {
            var asset = await LockAssetAsync(id, actor, cancellationToken);
            EnsureStatus(asset, AssetRecordStatus.Draft, "dibatalkan");

            var now = DateTime.UtcNow;
            asset.RecordStatus = AssetRecordStatus.Dibatalkan;
            asset.DecisionReason = reason;
            asset.IsCancel = true;
            asset.CancelBy = actor.UserId;
            asset.CancelDateTime = now;
            Touch(asset, actor.UserId, now);
            await _context.SaveChangesAsync(cancellationToken);
            return asset.Id;
        }, cancellationToken);

        await AuditAsync("AssetRegister.Cancel", $"Draft aset {id} dibatalkan oleh {actor.UserId}.");
        return await GetAssetAsync(id, cancellationToken);
    }

    private async Task<AssetDraftValues> ValidateDraftAsync(AssetDraftFieldsRequest request, CancellationToken cancellationToken)
    {
        var assetName = request.AssetName?.Trim();
        if (string.IsNullOrEmpty(assetName) || assetName.Length > 250)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.AssetName, AssetRegisterMessages.AssetName);
        }

        if (request.AssetGroupId is not { } groupId || groupId == Guid.Empty ||
            !await _context.MstAssetGroups.AnyAsync(x => x.Id == groupId && x.IsActive && !x.IsDelete, cancellationToken))
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.GroupInactive, AssetRegisterMessages.GroupInactive);
        }

        if (request.OwnershipTypeId is not { } ownershipTypeId || ownershipTypeId == Guid.Empty ||
            !await _context.MstAssetOwnershipTypes.AnyAsync(x => x.Id == ownershipTypeId && x.IsActive && !x.IsDelete, cancellationToken))
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.OwnershipInactive, AssetRegisterMessages.OwnershipInactive);
        }

        var acquisitionType = request.AcquisitionType.GetValueOrDefault();
        if (acquisitionType is not (AssetAcquisitionType.Pembelian or AssetAcquisitionType.HibahMasuk))
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.Field, AssetRegisterMessages.AcquisitionTypeRequired);
        }

        if (request.AcquisitionDate is not { } acquisitionDate || request.ReadyForUseDate is not { } readyForUseDate)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.Dates, AssetRegisterMessages.Dates);
        }

        EnsureDates(acquisitionDate, readyForUseDate);

        if (request.RegisteredAcquisitionCost < 0m || !AssetValuationMoney.HasAtMostTwoDecimals(request.RegisteredAcquisitionCost))
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.AcquisitionCost, AssetRegisterMessages.AcquisitionCost);
        }

        if (request.ManufactureYear is { } year && (year < MinManufactureYear || year > FinanceBusinessDate.Today().Year))
        {
            throw AssetManagementException.Invalid(
                AssetRegisterErrorCodes.Field,
                $"Tahun pembuatan {MinManufactureYear} sampai {FinanceBusinessDate.Today().Year}.");
        }

        Guid? linkedGoodsReceiptId = null;
        Guid? purchaseOrderId = null;
        string? goodsReceiptNumber = null;
        if (request.GoodsReceiptId is { } goodsReceiptId && goodsReceiptId != Guid.Empty)
        {
            var receipt = await FindActiveGoodsReceiptAsync(goodsReceiptId, cancellationToken)
                ?? throw AssetManagementException.Invalid(AssetRegisterErrorCodes.GoodsReceipt, AssetRegisterMessages.GoodsReceipt);
            linkedGoodsReceiptId = receipt.Id;
            purchaseOrderId = receipt.PurchaseOrderId;
            goodsReceiptNumber = receipt.GRNumber;
        }

        return new AssetDraftValues(
            assetName,
            OptionalText(request.Description, 1000, "Keterangan"),
            groupId,
            ownershipTypeId,
            OptionalText(request.Brand, 100, "Merek"),
            OptionalText(request.ModelType, 100, "Tipe/model"),
            OptionalText(request.SerialNumber, 100, "Nomor seri"),
            OptionalText(request.Manufacturer, 150, "Produsen"),
            OptionalText(request.DistributionPermitNumber, 60, "Nomor izin edar"),
            request.ManufactureYear,
            OptionalText(request.DistributorName, 150, "Distributor"),
            acquisitionType,
            acquisitionDate,
            readyForUseDate,
            request.RegisteredAcquisitionCost,
            OptionalText(request.SupplierName, 200, "Nama pemasok"),
            purchaseOrderId,
            linkedGoodsReceiptId,
            goodsReceiptNumber,
            OptionalText(request.DonationReference, 100, "Nomor berita acara hibah"));
    }

    private async Task EnsureReadyForSubmissionAsync(AstAsset asset, CancellationToken cancellationToken)
    {
        var group = await _context.MstAssetGroups
            .AsNoTracking()
            .Where(x => x.Id == asset.AssetGroupId)
            .Select(x => new { x.IsActive, x.IsDelete, x.Classification!.IsMedical })
            .FirstOrDefaultAsync(cancellationToken);

        if (group == null || !group.IsActive || group.IsDelete)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.GroupInactive, AssetRegisterMessages.GroupInactive);
        }

        if (!await _context.MstAssetOwnershipTypes.AnyAsync(x => x.Id == asset.OwnershipTypeId && x.IsActive && !x.IsDelete, cancellationToken))
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.OwnershipInactive, AssetRegisterMessages.OwnershipInactive);
        }

        if (asset.AcquisitionType == AssetAcquisitionType.HibahMasuk && string.IsNullOrWhiteSpace(asset.DonationReference))
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.DonationReference, AssetRegisterMessages.DonationReference);
        }

        EnsureDates(asset.AcquisitionDate, asset.ReadyForUseDate);

        if (asset.RegisteredAcquisitionCost <= 0m || !AssetValuationMoney.HasAtMostTwoDecimals(asset.RegisteredAcquisitionCost))
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.AcquisitionCost, AssetRegisterMessages.AcquisitionCost);
        }

        if (group.IsMedical &&
            new[] { asset.Brand, asset.ModelType, asset.SerialNumber, asset.Manufacturer, asset.DistributionPermitNumber }
                .Any(string.IsNullOrWhiteSpace))
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.MedicalAttributes, AssetRegisterMessages.MedicalAttributes);
        }

        if (asset.GoodsReceiptId is { } goodsReceiptId && await FindActiveGoodsReceiptAsync(goodsReceiptId, cancellationToken) == null)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.GoodsReceipt, AssetRegisterMessages.GoodsReceipt);
        }
    }

    private Task<FinGoodsReceipt?> FindActiveGoodsReceiptAsync(Guid goodsReceiptId, CancellationToken cancellationToken) =>
        _context.FinGoodsReceipts
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == goodsReceiptId && !x.IsDelete && !x.IsCancel && x.Status != FinGoodsReceiptStatuses.Cancelled,
                cancellationToken);

    private static void EnsureDates(DateOnly acquisitionDate, DateOnly readyForUseDate)
    {
        var today = FinanceBusinessDate.Today();
        if (acquisitionDate > today || readyForUseDate < acquisitionDate || readyForUseDate > today)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.Dates, AssetRegisterMessages.Dates);
        }
    }

    private static string? OptionalText(string? value, int maxLength, string label)
    {
        var text = value?.Trim();
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        if (text.Length > maxLength)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.Field, $"{label} paling panjang {maxLength} karakter.");
        }

        return text;
    }

    private static void Apply(AstAsset asset, AssetDraftValues values)
    {
        asset.AssetName = values.AssetName;
        asset.Description = values.Description;
        asset.AssetGroupId = values.AssetGroupId;
        asset.OwnershipTypeId = values.OwnershipTypeId;
        asset.Brand = values.Brand;
        asset.ModelType = values.ModelType;
        asset.SerialNumber = values.SerialNumber;
        asset.Manufacturer = values.Manufacturer;
        asset.DistributionPermitNumber = values.DistributionPermitNumber;
        asset.ManufactureYear = values.ManufactureYear;
        asset.DistributorName = values.DistributorName;
        asset.AcquisitionType = values.AcquisitionType;
        asset.AcquisitionDate = values.AcquisitionDate;
        asset.ReadyForUseDate = values.ReadyForUseDate;
        asset.RegisteredAcquisitionCost = values.RegisteredAcquisitionCost;
        asset.SupplierName = values.SupplierName;
        asset.PurchaseOrderId = values.PurchaseOrderId;
        asset.GoodsReceiptId = values.GoodsReceiptId;
        asset.GoodsReceiptNumber = values.GoodsReceiptNumber;
        asset.DonationReference = values.DonationReference;
    }

    private sealed record AssetDraftValues(
        string AssetName,
        string? Description,
        Guid AssetGroupId,
        Guid OwnershipTypeId,
        string? Brand,
        string? ModelType,
        string? SerialNumber,
        string? Manufacturer,
        string? DistributionPermitNumber,
        int? ManufactureYear,
        string? DistributorName,
        AssetAcquisitionType AcquisitionType,
        DateOnly AcquisitionDate,
        DateOnly ReadyForUseDate,
        decimal RegisteredAcquisitionCost,
        string? SupplierName,
        Guid? PurchaseOrderId,
        Guid? GoodsReceiptId,
        string? GoodsReceiptNumber,
        string? DonationReference);
}
