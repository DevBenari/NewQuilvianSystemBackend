using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Services;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

public sealed partial class AssetRecordService
{
    public AssetMasterFilterMetadataResponse GetMetadata()
    {
        var fields = new List<AssetMasterFormFieldResponse>
        {
            FormField("assetName", "Nama Aset", "text", true, 1, 250),
            FormField("assetGroupId", "Kelompok Aset", "select", true, 2, optionsSource: "master-data/groups/options"),
            FormField("ownershipTypeId", "Jenis Kepemilikan", "select", true, 3, optionsSource: "master-data/ownership-types/options"),
            FormField("acquisitionType", "Jenis Perolehan", "select", true, 4, optionsSource: "acquisitionType"),
            FormField("acquisitionDate", "Tanggal Perolehan", "date", true, 5),
            FormField("readyForUseDate", "Tanggal Siap Pakai", "date", true, 6, description: "Dasar awal penyusutan"),
            FormField("registeredAcquisitionCost", "Harga Perolehan", "currency", true, 7, description: "Wajib lebih dari Rp 0 saat diajukan"),
            FormField("description", "Keterangan", "textarea", false, 8, 1000),
            FormField("brand", "Merek", "text", false, 9, 100, description: "Wajib untuk alat medis"),
            FormField("modelType", "Tipe/Model", "text", false, 10, 100, description: "Wajib untuk alat medis"),
            FormField("serialNumber", "Nomor Seri", "text", false, 11, 100, description: "Wajib untuk alat medis"),
            FormField("manufacturer", "Produsen", "text", false, 12, 150, description: "Wajib untuk alat medis"),
            FormField("distributionPermitNumber", "Nomor Izin Edar (AKL/AKD)", "text", false, 13, 60, description: "Wajib untuk alat medis"),
            FormField("manufactureYear", "Tahun Pembuatan", "number", false, 14),
            FormField("distributorName", "Distributor", "text", false, 15, 150),
            FormField("supplierName", "Nama Pemasok", "text", false, 16, 200),
            FormField("goodsReceiptId", "Penerimaan Barang", "select", false, 17),
            FormField("donationReference", "Nomor Berita Acara Hibah", "text", false, 18, 100, description: "Wajib untuk hibah masuk")
        };

        var createFields = new List<AssetMasterFormFieldResponse>(fields)
        {
            FormField("quantity", "Jumlah Unit", "number", true, 0, description: $"1 sampai {MaxDraftQuantity} draft sekaligus", example: "1")
        };

        return new AssetMasterFilterMetadataResponse
        {
            DefaultFilter = new AssetMasterDefaultFilterResponse { SortBy = "createDateTime", SortDirection = "desc" },
            SortOptions =
            [
                new() { Value = "createDateTime", Label = "Tanggal Dibuat" },
                new() { Value = "name", Label = "Nama Aset" },
                new() { Value = "inventoryNumber", Label = "Nomor Inventaris" },
                new() { Value = "acquisitionDate", Label = "Tanggal Perolehan" },
                new() { Value = "approvedAt", Label = "Tanggal Disetujui" },
                new() { Value = "recordStatus", Label = "Status" }
            ],
            FlagOptions =
            [
                EnumOptions("recordStatus", "Status Pencatatan", Enum.GetValues<AssetRecordStatus>(), AssetRegisterLabels.RecordStatus),
                EnumOptions("condition", "Kondisi", Enum.GetValues<AssetCondition>(), AssetRegisterLabels.Condition),
                EnumOptions("placementState", "Penempatan", Enum.GetValues<AssetPlacementState>(), AssetRegisterLabels.PlacementState),
                EnumOptions("acquisitionType", "Jenis Perolehan", Enum.GetValues<AssetAcquisitionType>(), AssetRegisterLabels.AcquisitionType)
            ],
            QueryParameters =
            [
                new() { Name = "search", Type = "string", Description = "Mencari pada nomor inventaris, nama, merek, dan nomor seri" },
                new() { Name = "organizationUnitId", Type = "guid", Description = "Unit penempatan saat ini" },
                new() { Name = "locationId", Type = "guid", Description = "Ruangan penempatan saat ini" },
                new() { Name = "categoryId", Type = "guid", Description = "Kategori aset" },
                new() { Name = "groupId", Type = "guid", Description = "Kelompok aset" },
                new() { Name = "recordStatus", Type = "integer", Description = "Lihat flagOptions recordStatus" },
                new() { Name = "condition", Type = "integer", Description = "Lihat flagOptions condition" },
                new() { Name = "placementState", Type = "integer", Description = "Lihat flagOptions placementState" },
                new() { Name = "acquisitionType", Type = "integer", Description = "Lihat flagOptions acquisitionType" },
                new() { Name = "createdByMe", Type = "boolean", Description = "true = hanya aset yang dibuat pengguna ini", Example = "true" },
                new() { Name = "approvedFrom", Type = "date", Description = "Tanggal disetujui paling awal (WIB), inklusif", Example = "2026-11-17" },
                new() { Name = "approvedTo", Type = "date", Description = "Tanggal disetujui paling akhir (WIB), inklusif", Example = "2026-11-17" },
                new() { Name = "isIdle", Type = "boolean", Description = "true = aset Terdaftar berkondisi Tidak Digunakan lebih lama dari batas hari menganggur di Pengaturan Aset", Example = "true" },
                new() { Name = "sortBy", Type = "string", Description = "Kolom pengurutan, lihat sortOptions", Example = "createDateTime" },
                new() { Name = "sortDirection", Type = "string", Description = "asc atau desc", Example = "desc" },
                new() { Name = "pageNumber", Type = "integer", Description = "Halaman, mulai 1", Example = "1" },
                new() { Name = "pageSize", Type = "integer", Description = $"Jumlah baris per halaman, maksimal {MaxPageSize}", Example = "25" }
            ],
            CreateFields = createFields.OrderBy(x => x.SortOrder).ToList(),
            UpdateFields = fields
        };
    }

    public async Task<AssetRegisterSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var actor = await RequireActorAsync(cancellationToken);
        var scoped = await ScopedAssetsAsync(actor, cancellationToken);

        var rows = await scoped
            .GroupBy(x => new { x.RecordStatus, x.Condition, x.PlacementState })
            .Select(g => new { g.Key.RecordStatus, g.Key.Condition, g.Key.PlacementState, Count = g.Count() })
            .ToListAsync(cancellationToken);

        int CountWhere(Func<AssetRecordStatus, AssetCondition, AssetPlacementState, bool> predicate) =>
            rows.Where(x => predicate(x.RecordStatus, x.Condition, x.PlacementState)).Sum(x => x.Count);

        return new AssetRegisterSummaryResponse
        {
            Total = rows.Sum(x => x.Count),
            Draft = CountWhere((s, _, _) => s == AssetRecordStatus.Draft),
            Submitted = CountWhere((s, _, _) => s == AssetRecordStatus.Diajukan),
            Registered = CountWhere((s, _, _) => s == AssetRecordStatus.Terdaftar),
            Cancelled = CountWhere((s, _, _) => s == AssetRecordStatus.Dibatalkan),
            Disposed = CountWhere((s, _, _) => s == AssetRecordStatus.Dihapus),
            RegisteredNotPlaced = CountWhere((s, _, p) => s == AssetRecordStatus.Terdaftar && p == AssetPlacementState.BelumDitempatkan),
            RegisteredDamaged = CountWhere((s, c, _) => s == AssetRecordStatus.Terdaftar && (c == AssetCondition.RusakRingan || c == AssetCondition.RusakBerat)),
            RegisteredLost = CountWhere((s, c, _) => s == AssetRecordStatus.Terdaftar && c == AssetCondition.Hilang)
        };
    }

    public async Task<PagedResult<AssetListItemResponse>> GetAssetsAsync(AssetPagedQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var query = await FilteredAssetsAsync(request, cancellationToken);

        var descending = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        IOrderedQueryable<AstAsset> ordered = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "name" => descending ? query.OrderByDescending(x => x.AssetName) : query.OrderBy(x => x.AssetName),
            "inventorynumber" => descending ? query.OrderByDescending(x => x.InventoryNumber) : query.OrderBy(x => x.InventoryNumber),
            "acquisitiondate" => descending ? query.OrderByDescending(x => x.AcquisitionDate) : query.OrderBy(x => x.AcquisitionDate),
            "approvedat" => descending ? query.OrderByDescending(x => x.ApprovedAt) : query.OrderBy(x => x.ApprovedAt),
            "recordstatus" => descending ? query.OrderByDescending(x => x.RecordStatus) : query.OrderBy(x => x.RecordStatus),
            _ => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime)
        };

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, MaxPageSize);
        var total = await query.CountAsync(cancellationToken);
        var items = await ProjectList(ordered.ThenBy(x => x.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize)).ToListAsync(cancellationToken);
        items.ForEach(Describe);

        return new PagedResult<AssetListItemResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    internal async Task<IQueryable<AstAsset>> FilteredAssetsAsync(AssetPagedQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var query = await ScopedAssetsAsync(actor, cancellationToken);

        if (request.OrganizationUnitId is { } unitId && unitId != Guid.Empty)
        {
            query = query.Where(x => x.CurrentOrganizationUnitId == unitId);
        }

        if (request.LocationId is { } locationId && locationId != Guid.Empty)
        {
            query = query.Where(x => x.CurrentLocationId == locationId);
        }

        if (request.CategoryId is { } categoryId && categoryId != Guid.Empty)
        {
            query = query.Where(x => x.AssetGroup!.CategoryId == categoryId);
        }

        if (request.GroupId is { } groupId && groupId != Guid.Empty)
        {
            query = query.Where(x => x.AssetGroupId == groupId);
        }

        if (request.RecordStatus is { } recordStatus)
        {
            query = query.Where(x => x.RecordStatus == recordStatus);
        }

        if (request.Condition is { } condition)
        {
            query = query.Where(x => x.Condition == condition);
        }

        if (request.PlacementState is { } placementState)
        {
            query = query.Where(x => x.PlacementState == placementState);
        }

        if (request.AcquisitionType is { } acquisitionType)
        {
            query = query.Where(x => x.AcquisitionType == acquisitionType);
        }

        if (request.CreatedByMe == true)
        {
            query = query.Where(x => x.CreateBy == actor.UserId);
        }

        if (request.ApprovedFrom is { } approvedFrom)
        {
            var fromUtc = AppDateTimeHelper.OperationalDateToUtc(approvedFrom.ToDateTime(TimeOnly.MinValue));
            query = query.Where(x => x.ApprovedAt != null && x.ApprovedAt >= fromUtc);
        }

        if (request.ApprovedTo is { } approvedTo)
        {
            var toUtcExclusive = AppDateTimeHelper.OperationalDateToUtc(approvedTo.AddDays(1).ToDateTime(TimeOnly.MinValue));
            query = query.Where(x => x.ApprovedAt != null && x.ApprovedAt < toUtcExclusive);
        }

        if (request.IsIdle is { } isIdle)
        {
            var entity = await _entityResolver.ResolveAsync(cancellationToken);
            var settings = await AssetRegisterSettingReader.ReadAsync(_context, entity.LegalEntityId, cancellationToken);
            var cutoff = AssetRegisterSettingReader.IdleCutoffUtc(settings.IdleThresholdDays);
            query = isIdle
                ? Idle(query, cutoff)
                : query.Where(x => !(x.RecordStatus == AssetRecordStatus.Terdaftar &&
                    x.Condition == AssetCondition.TidakDigunakan &&
                    (x.ConditionChangedAt ?? x.ApprovedAt ?? x.CreateDateTime) < cutoff));
        }

        return Search(query, request.Search);
    }

    public async Task<PagedResult<AssetIdleItemResponse>> GetIdleAssetsAsync(AssetPagedQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var settings = await AssetRegisterSettingReader.ReadAsync(_context, entity.LegalEntityId, cancellationToken);
        var query = Idle(await ScopedAssetsAsync(actor, cancellationToken), AssetRegisterSettingReader.IdleCutoffUtc(settings.IdleThresholdDays));

        if (request.OrganizationUnitId is { } unitId && unitId != Guid.Empty)
        {
            query = query.Where(x => x.CurrentOrganizationUnitId == unitId);
        }

        if (request.LocationId is { } locationId && locationId != Guid.Empty)
        {
            query = query.Where(x => x.CurrentLocationId == locationId);
        }

        if (request.CategoryId is { } categoryId && categoryId != Guid.Empty)
        {
            query = query.Where(x => x.AssetGroup!.CategoryId == categoryId);
        }

        if (request.GroupId is { } groupId && groupId != Guid.Empty)
        {
            query = query.Where(x => x.AssetGroupId == groupId);
        }

        query = Search(query, request.Search);
        var ordered = string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase)
            ? query.OrderByDescending(x => x.ConditionChangedAt ?? x.ApprovedAt ?? x.CreateDateTime)
            : query.OrderBy(x => x.ConditionChangedAt ?? x.ApprovedAt ?? x.CreateDateTime);

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, MaxPageSize);
        var total = await query.CountAsync(cancellationToken);
        var items = await ordered
            .ThenBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AssetIdleItemResponse
            {
                Id = x.Id,
                InventoryNumber = x.InventoryNumber,
                AssetName = x.AssetName,
                GroupName = x.AssetGroup != null ? x.AssetGroup.GroupName : null,
                CategoryName = x.AssetGroup != null && x.AssetGroup.Category != null ? x.AssetGroup.Category.CategoryName : null,
                CurrentOrganizationUnitId = x.CurrentOrganizationUnitId,
                CurrentOrganizationUnitName = x.CurrentOrganizationUnit != null ? x.CurrentOrganizationUnit.UnitName : null,
                CurrentLocationName = x.CurrentLocation != null ? x.CurrentLocation.LocationName : null,
                CurrentCustodianEmployeeName = x.CurrentCustodianEmployee != null ? x.CurrentCustodianEmployee.FullName : null,
                Condition = x.Condition,
                ActiveProcessKind = x.ActiveProcessKind,
                IdleSince = x.ConditionChangedAt ?? x.ApprovedAt ?? x.CreateDateTime
            })
            .ToListAsync(cancellationToken);

        foreach (var item in items)
        {
            item.ConditionName = AssetRegisterLabels.Condition(item.Condition);
            item.IdleDays = AssetRegisterSettingReader.OperationalDaysSince(item.IdleSince);
            item.IdleThresholdDays = settings.IdleThresholdDays;
        }

        return new PagedResult<AssetIdleItemResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    private static IQueryable<AstAsset> Idle(IQueryable<AstAsset> query, DateTime cutoffUtc) =>
        query.Where(x =>
            x.RecordStatus == AssetRecordStatus.Terdaftar &&
            x.Condition == AssetCondition.TidakDigunakan &&
            (x.ConditionChangedAt ?? x.ApprovedAt ?? x.CreateDateTime) < cutoffUtc);

    private static IQueryable<AstAsset> Search(IQueryable<AstAsset> query, string? search)
    {
        var keyword = string.IsNullOrWhiteSpace(search) ? null : search.Trim().ToUpper();
        if (keyword == null)
        {
            return query;
        }

        return query.Where(x =>
            x.AssetName.ToUpper().Contains(keyword) ||
            (x.InventoryNumber != null && x.InventoryNumber.ToUpper().Contains(keyword)) ||
            (x.Brand != null && x.Brand.ToUpper().Contains(keyword)) ||
            (x.SerialNumber != null && x.SerialNumber.ToUpper().Contains(keyword)));
    }

    public async Task<AssetDetailResponse> GetAssetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var actor = await RequireActorAsync(cancellationToken);

        var detail = await _context.AstAssets
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDelete)
            .Select(x => new AssetDetailResponse
            {
                Id = x.Id,
                InventoryNumber = x.InventoryNumber,
                AssetName = x.AssetName,
                Description = x.Description,
                AssetGroupId = x.AssetGroupId,
                InventoryCode = x.AssetGroup!.InventoryCode,
                GroupName = x.AssetGroup.GroupName,
                CategoryId = x.AssetGroup.CategoryId,
                CategoryName = x.AssetGroup.Category!.CategoryName,
                ClassificationName = x.AssetGroup.Classification!.ClassificationName,
                IsMedical = x.AssetGroup.Classification.IsMedical,
                OwnershipTypeId = x.OwnershipTypeId,
                OwnershipTypeName = x.OwnershipType!.OwnershipName,
                Brand = x.Brand,
                ModelType = x.ModelType,
                SerialNumber = x.SerialNumber,
                Manufacturer = x.Manufacturer,
                DistributionPermitNumber = x.DistributionPermitNumber,
                ManufactureYear = x.ManufactureYear,
                DistributorName = x.DistributorName,
                AcquisitionType = x.AcquisitionType,
                AcquisitionDate = x.AcquisitionDate,
                ReadyForUseDate = x.ReadyForUseDate,
                SupplierName = x.SupplierName,
                PurchaseOrderId = x.PurchaseOrderId,
                GoodsReceiptId = x.GoodsReceiptId,
                GoodsReceiptNumber = x.GoodsReceiptNumber,
                DonationReference = x.DonationReference,
                RecordStatus = x.RecordStatus,
                Condition = x.Condition,
                PlacementState = x.PlacementState,
                CurrentOrganizationUnitId = x.CurrentOrganizationUnitId,
                CurrentOrganizationUnitName = x.CurrentOrganizationUnit != null ? x.CurrentOrganizationUnit.UnitName : null,
                CurrentLocationId = x.CurrentLocationId,
                CurrentLocationName = x.CurrentLocation != null ? x.CurrentLocation.LocationName : null,
                CurrentCustodianEmployeeId = x.CurrentCustodianEmployeeId,
                CurrentCustodianEmployeeName = x.CurrentCustodianEmployee != null ? x.CurrentCustodianEmployee.FullName : null,
                ActiveProcessKind = x.ActiveProcessKind,
                SubmittedBy = x.SubmittedBy,
                SubmittedAt = x.SubmittedAt,
                ApprovedBy = x.ApprovedBy,
                ApprovedAt = x.ApprovedAt,
                RejectedBy = x.RejectedBy,
                RejectedAt = x.RejectedAt,
                DecisionReason = x.DecisionReason,
                CreateBy = x.CreateBy,
                CreateDateTime = x.CreateDateTime,
                RowVersion = x.RowVersion
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw NotFound();

        actor.EnsureCanAccessUnit(detail.CurrentOrganizationUnitId);

        var names = await UserNamesAsync([detail.SubmittedBy, detail.ApprovedBy, detail.RejectedBy], cancellationToken);
        detail.SubmittedByName = NameOf(names, detail.SubmittedBy);
        detail.ApprovedByName = NameOf(names, detail.ApprovedBy);
        detail.RejectedByName = NameOf(names, detail.RejectedBy);
        detail.RecordStatusName = AssetRegisterLabels.RecordStatus(detail.RecordStatus);
        detail.ConditionName = AssetRegisterLabels.Condition(detail.Condition);
        detail.PlacementStateName = AssetRegisterLabels.PlacementState(detail.PlacementState);
        detail.AcquisitionTypeName = AssetRegisterLabels.AcquisitionType(detail.AcquisitionType);
        detail.AvailableActions = AvailableActions(detail.RecordStatus, detail.SubmittedBy, actor);
        return detail;
    }

    public async Task<AssetDetailResponse> GetAssetByNumberAsync(string inventoryNumber, CancellationToken cancellationToken = default)
    {
        var number = (inventoryNumber ?? string.Empty).Trim().ToUpperInvariant();
        if (number.Length is < 1 or > 30)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.Field, AssetRegisterMessages.InventoryNumberRequired);
        }

        await RequireActorAsync(cancellationToken);
        var entity = await _entityResolver.ResolveAsync(cancellationToken);

        var id = await _context.AstAssets
            .AsNoTracking()
            .Where(x => x.InventoryNumber == number && !x.IsDelete && x.LegalEntityId == entity.LegalEntityId)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw NotFound();

        return await GetAssetAsync(id, cancellationToken);
    }

    public async Task<AssetAmountResponse> GetAmountsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var actor = await RequireActorAsync(cancellationToken);

        var asset = await _context.AstAssets
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDelete)
            .Select(x => new { x.Id, x.InventoryNumber, x.RegisteredAcquisitionCost, x.CurrentOrganizationUnitId })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw NotFound();

        actor.EnsureCanAccessUnit(asset.CurrentOrganizationUnitId);

        var book = await _context.AstAssetBooks
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.AssetId == id && !x.IsDelete, cancellationToken);

        return new AssetAmountResponse
        {
            AssetId = asset.Id,
            InventoryNumber = asset.InventoryNumber,
            RegisteredAcquisitionCost = asset.RegisteredAcquisitionCost,
            Book = book == null
                ? null
                : new AssetBookSummaryResponse
                {
                    IsCapitalized = book.IsCapitalized,
                    IsDepreciable = book.IsDepreciable,
                    CapitalizedCost = book.CapitalizedCost,
                    ResidualValue = book.ResidualValue,
                    AccumulatedDepreciation = book.AccumulatedDepreciation,
                    AccumulatedImpairment = book.AccumulatedImpairment,
                    NetBookValue = AssetBookService.GetNetBookValue(book),
                    UsefulLifeMonths = book.UsefulLifeMonths,
                    DepreciatedMonths = book.DepreciatedMonths,
                    DepreciationStartPeriod = book.DepreciationStartPeriod,
                    LastDepreciatedPeriod = book.LastDepreciatedPeriod,
                    BookStatusName = book.BookStatus.ToString()
                }
        };
    }

    public async Task<PagedResult<AssetHistoryItemResponse>> GetHistoryAsync(Guid id, AssetHistoryPagedQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);

        var asset = await _context.AstAssets
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDelete)
            .Select(x => new { x.CurrentOrganizationUnitId })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw NotFound();

        actor.EnsureCanAccessUnit(asset.CurrentOrganizationUnitId);

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, MaxPageSize);
        var query = _context.AstAssetHistories.AsNoTracking().Where(x => x.AssetId == id && !x.IsDelete);
        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.OccurredAt)
            .ThenByDescending(x => x.CreateDateTime)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AssetHistoryItemResponse
            {
                Id = x.Id,
                EventType = x.EventType,
                SubModule = x.SubModule,
                FromValue = x.FromValue,
                ToValue = x.ToValue,
                SourceDocumentType = x.SourceDocumentType,
                SourceDocumentId = x.SourceDocumentId,
                SourceDocumentNumber = x.SourceDocumentNumber,
                ActorUserId = x.ActorUserId,
                Reason = x.Reason,
                OccurredAt = x.OccurredAt
            })
            .ToListAsync(cancellationToken);

        var names = await UserNamesAsync(items.Select(x => (Guid?)x.ActorUserId), cancellationToken);
        foreach (var item in items)
        {
            item.EventTypeName = AssetRegisterLabels.HistoryEvent(item.EventType);
            item.ActorName = NameOf(names, item.ActorUserId);
        }

        return new PagedResult<AssetHistoryItemResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    private async Task<IQueryable<AstAsset>> ScopedAssetsAsync(AssetActor actor, CancellationToken cancellationToken)
    {
        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var query = _context.AstAssets.AsNoTracking().Where(x => !x.IsDelete && x.LegalEntityId == entity.LegalEntityId);

        if (actor.CanReadAllUnits)
        {
            return query;
        }

        var unitIds = actor.UnitIds.ToList();
        return query.Where(x => x.CurrentOrganizationUnitId.HasValue && unitIds.Contains(x.CurrentOrganizationUnitId.Value));
    }

    private async Task<List<AssetListItemResponse>> ListByIdsAsync(List<Guid> ids, CancellationToken cancellationToken)
    {
        var items = await ProjectList(_context.AstAssets
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .OrderBy(x => x.CreateDateTime)
                .ThenBy(x => x.Id))
            .ToListAsync(cancellationToken);
        items.ForEach(Describe);
        return items;
    }

    internal static IQueryable<AssetListItemResponse> ProjectList(IQueryable<AstAsset> query) =>
        query.Select(x => new AssetListItemResponse
        {
            Id = x.Id,
            InventoryNumber = x.InventoryNumber,
            AssetName = x.AssetName,
            AssetGroupId = x.AssetGroupId,
            InventoryCode = x.AssetGroup!.InventoryCode,
            GroupName = x.AssetGroup.GroupName,
            CategoryName = x.AssetGroup.Category!.CategoryName,
            IsMedical = x.AssetGroup.Classification!.IsMedical,
            OwnershipTypeName = x.OwnershipType!.OwnershipName,
            Brand = x.Brand,
            SerialNumber = x.SerialNumber,
            AcquisitionType = x.AcquisitionType,
            AcquisitionDate = x.AcquisitionDate,
            ReadyForUseDate = x.ReadyForUseDate,
            SupplierName = x.SupplierName,
            GoodsReceiptNumber = x.GoodsReceiptNumber,
            DonationReference = x.DonationReference,
            RecordStatus = x.RecordStatus,
            Condition = x.Condition,
            PlacementState = x.PlacementState,
            CurrentOrganizationUnitId = x.CurrentOrganizationUnitId,
            CurrentOrganizationUnitName = x.CurrentOrganizationUnit != null ? x.CurrentOrganizationUnit.UnitName : null,
            CurrentLocationId = x.CurrentLocationId,
            CurrentLocationName = x.CurrentLocation != null ? x.CurrentLocation.LocationName : null,
            CurrentCustodianEmployeeId = x.CurrentCustodianEmployeeId,
            CurrentCustodianEmployeeName = x.CurrentCustodianEmployee != null ? x.CurrentCustodianEmployee.FullName : null,
            ActiveProcessKind = x.ActiveProcessKind,
            CreateBy = x.CreateBy,
            SubmittedBy = x.SubmittedBy,
            ApprovedAt = x.ApprovedAt,
            RejectedAt = x.RejectedAt,
            DecisionReason = x.DecisionReason,
            CreateDateTime = x.CreateDateTime
        });

    internal static void Describe(AssetListItemResponse item)
    {
        item.RecordStatusName = AssetRegisterLabels.RecordStatus(item.RecordStatus);
        item.ConditionName = AssetRegisterLabels.Condition(item.Condition);
        item.PlacementStateName = AssetRegisterLabels.PlacementState(item.PlacementState);
        item.AcquisitionTypeName = AssetRegisterLabels.AcquisitionType(item.AcquisitionType);
    }

    private static List<string> AvailableActions(AssetRecordStatus status, Guid? submittedBy, AssetActor actor) => status switch
    {
        AssetRecordStatus.Draft => ["Update", "Submit", "Cancel"],
        AssetRecordStatus.Diajukan when submittedBy != actor.UserId => ["Approve", "Reject"],
        _ => []
    };

    internal async Task<Dictionary<Guid, string>> UserNamesAsync(IEnumerable<Guid?> userIds, CancellationToken cancellationToken)
    {
        var ids = userIds.Where(x => x.HasValue && x.Value != Guid.Empty).Select(x => x!.Value).Distinct().ToList();
        if (ids.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }

        return await _context.Users
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.DisplayName, cancellationToken);
    }

    private static string? NameOf(Dictionary<Guid, string> names, Guid? userId) =>
        userId is { } id && names.TryGetValue(id, out var name) ? name : null;

    private static AssetMasterOptionSourceResponse EnumOptions<TEnum>(string name, string label, IEnumerable<TEnum> values, Func<TEnum, string> describe)
        where TEnum : struct, Enum => new()
    {
        Name = name,
        Label = label,
        Options = values
            .Select(x => new AssetMasterSortOptionResponse { Value = Convert.ToInt32(x).ToString(), Label = describe(x) })
            .ToList()
    };

    private static AssetMasterFormFieldResponse FormField(
        string name,
        string label,
        string inputType,
        bool required,
        int sortOrder,
        int? maxLength = null,
        string? optionsSource = null,
        string? description = null,
        string? example = null) => new()
    {
        Name = name,
        Label = label,
        Section = "Informasi Aset",
        InputType = inputType,
        IsRequiredOnCreate = required,
        IsRequiredOnUpdate = required,
        RequiredType = required ? "Required" : "Optional",
        MaxLength = maxLength,
        OptionsSource = optionsSource,
        Description = description,
        Example = example,
        SortOrder = sortOrder
    };
}
