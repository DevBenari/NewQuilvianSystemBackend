using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Services;
using QuilvianSystemBackend.Areas.Corporate.LogisticsManagement.Receiving.Dtos;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.LogisticsManagement.Receiving.Services;

/// <summary>Filter tidak sah (nilai di luar daftar, rentang tanggal terbalik). Dipetakan controller menjadi 400.</summary>
public sealed class LogisticsReceivingValidationException : Exception
{
    public LogisticsReceivingValidationException(string message) : base(message) { }
}

/// <summary>
/// LGS-BE-REC-INSP-001 — pembaca "Pemeriksaan Hasil Pengadaan" untuk Logistik. MURNI BACA:
///  - hanya <c>AsNoTracking</c>; tidak ada <c>SaveChanges</c>, transaksi, atau panggilan ke layanan stok;
///  - sumber data = <c>FinGoodsReceipt</c>/<c>FinGoodsReceiptItem</c>/<c>FinPurchaseOrder</c>/<c>FinPurchaseOrderItem</c> yang sudah ada,
///    dicocokkan lewat ID item PO (tidak pernah lewat nama produk);
///  - kuantitas turunan (sah, menunggu periksa, rusak) memakai <see cref="PurchasingReceiptQuantityCalculator"/> yang sama dengan Finance,
///    bukan formula baru. Besaran yang tidak dapat dipastikan bernilai null, bukan 0;
///  - tidak mengirim harga atau total PO, dan tidak mengubah arti kontrak Finance.
///
/// Halaman ini BUKAN pencatat hasil pemeriksaan dan BUKAN penambah stok. Pencatatan hasil pemeriksaan, status pemeriksaan, dan
/// penambahan saldo gudang menunggu LGS-OQ-015/017/018 dan memerlukan skema baru (BLOCKED).
/// </summary>
public sealed class LogisticsProcurementInspectionQueryService
{
    // Karakter pelarian LIKE yang tidak memerlukan backslash.
    private const string LikeEscape = "!";

    private readonly ApplicationDbContext _dbContext;

    public LogisticsProcurementInspectionQueryService(ApplicationDbContext dbContext) => _dbContext = dbContext;

    // ------------------------------------------------------------------ daftar

    public async Task<PagedResult<ProcurementInspectionItemResponse>> GetPagedAsync(
        ProcurementInspectionQuery query, CancellationToken cancellationToken)
    {
        var filters = Normalize(query);
        var filtered = ApplyFilters(BaseItems(), filters);

        var totalCount = await filtered.CountAsync(cancellationToken);

        var rows = await Project(filtered
                .OrderByDescending(x => x.GoodsReceipt!.ReceivedDate)
                .ThenByDescending(x => x.GoodsReceipt!.CreateDateTime)
                .ThenBy(x => x.GoodsReceipt!.GRNumber)
                .ThenBy(x => x.PurchaseOrderItem!.ProductName)
                .ThenBy(x => x.Id)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProcurementInspectionItemResponse>
        {
            Items = rows.Select(row =>
            {
                var item = new ProcurementInspectionItemResponse();
                Populate(item, row);
                return item;
            }).ToList(),
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalData = totalCount,
            TotalPage = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };
    }

    // ------------------------------------------------------------------ ringkasan

    public async Task<ProcurementInspectionSummaryResponse> GetSummaryAsync(
        ProcurementInspectionQuery query, CancellationToken cancellationToken)
    {
        var filtered = ApplyFilters(BaseItems(), Normalize(query));

        var active = filtered.Where(x => x.GoodsReceipt!.Status == FinGoodsReceiptStatuses.Received);
        var cancelled = filtered.Where(x => x.GoodsReceipt!.Status == FinGoodsReceiptStatuses.Cancelled);

        var byCondition = await active
            .GroupBy(x => x.ConditionCode)
            .Select(g => new { Code = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var activeDocuments = await active.Select(x => x.GoodsReceiptId).Distinct().CountAsync(cancellationToken);
        var cancelledDocuments = await cancelled.Select(x => x.GoodsReceiptId).Distinct().CountAsync(cancellationToken);

        int CountOf(string? code) => byCondition.Where(x => x.Code == code).Sum(x => x.Count);

        return new ProcurementInspectionSummaryResponse
        {
            TotalDocuments = activeDocuments,
            CancelledDocuments = cancelledDocuments,
            TotalItems = byCondition.Sum(x => x.Count),
            GoodItems = CountOf(FinGoodsReceiptConditionCodes.Good),
            PendingInspectionItems = CountOf(FinGoodsReceiptConditionCodes.PendingInspection),
            DamagedItems = CountOf(FinGoodsReceiptConditionCodes.Damaged),
            // Tanpa kode kondisi (GR lokal/legacy): tidak pernah dihitung sebagai GOOD.
            ConditionNotRecordedItems = CountOf(null)
        };
    }

    // ------------------------------------------------------------------ opsi supplier

    /// <summary>
    /// Supplier yang memiliki GR. Disediakan di sini supaya halaman Logistik tidak bergantung pada izin master supplier
    /// atau izin Finance hanya untuk mengisi saringan.
    /// </summary>
    public async Task<PagedResult<ProcurementInspectionSupplierOptionResponse>> GetSupplierOptionsAsync(
        ProcurementInspectionSupplierOptionsQuery query, CancellationToken cancellationToken)
    {
        var supplierIds = _dbContext.FinGoodsReceipts.AsNoTracking()
            .Where(x => !x.IsDelete)
            .Select(x => x.PurchaseOrder!.SupplierId);

        var suppliers = _dbContext.MstSuppliers.AsNoTracking()
            .Where(x => !x.IsDelete && supplierIds.Contains(x.Id));

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = ContainsPattern(query.Search);
            suppliers = suppliers.Where(x =>
                EF.Functions.ILike(x.SupplierName, pattern, LikeEscape)
                || EF.Functions.ILike(x.SupplierCode, pattern, LikeEscape));
        }

        var totalCount = await suppliers.CountAsync(cancellationToken);

        var items = await suppliers
            .OrderBy(x => x.SupplierName)
            .ThenBy(x => x.Id)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new ProcurementInspectionSupplierOptionResponse { Id = x.Id, Name = x.SupplierName, Code = x.SupplierCode })
            .ToListAsync(cancellationToken);

        return new PagedResult<ProcurementInspectionSupplierOptionResponse>
        {
            Items = items,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalData = totalCount,
            TotalPage = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };
    }

    // ------------------------------------------------------------------ detail

    /// <summary>Detail satu GR beserta seluruh barisnya. Null bila GR tidak ada atau sudah dihapus.</summary>
    public async Task<ProcurementInspectionDetailResponse?> GetDetailAsync(Guid goodsReceiptId, CancellationToken cancellationToken)
    {
        var header = await _dbContext.FinGoodsReceipts.AsNoTracking()
            .Where(x => x.Id == goodsReceiptId && !x.IsDelete)
            .Select(x => new
            {
                x.Id,
                x.GRNumber,
                x.ReceivedDate,
                x.Status,
                x.SourceSystem,
                x.LastSyncedAt,
                x.PurchaseOrderId,
                PurchaseOrderNumber = x.PurchaseOrder!.PONumber,
                PurchaseOrderStatus = x.PurchaseOrder!.Status,
                PurchaseOrderSourceSystem = x.PurchaseOrder!.SourceSystem,
                SupplierId = x.PurchaseOrder!.SupplierId,
                SupplierName = x.PurchaseOrder!.Supplier!.SupplierName,
                SupplierCode = x.PurchaseOrder!.Supplier!.SupplierCode
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (header is null) return null;

        var rows = await Project(BaseItems()
                .Where(x => x.GoodsReceiptId == goodsReceiptId)
                .OrderBy(x => x.PurchaseOrderItem!.ProductName)
                .ThenBy(x => x.BatchNumber)
                .ThenBy(x => x.ExpiredDate)
                .ThenBy(x => x.Id))
            .ToListAsync(cancellationToken);

        var cumulative = await LoadCumulativeProgressAsync(header.PurchaseOrderId, cancellationToken);

        var items = rows.Select(row =>
        {
            var item = new ProcurementInspectionDetailItemResponse();
            Populate(item, row);

            // Kemajuan kumulatif hanya bila konversi pasti (aturan kalkulator Finance); selain itu null, bukan tebakan.
            if (cumulative is not null)
            {
                var progress = cumulative.Items.FirstOrDefault(x => x.PurchaseOrderItemId == row.PurchaseOrderItemId);
                if (progress is { IsConversionResolved: true })
                {
                    item.CumulativeAcceptedQuantity = progress.AcceptedBaseQuantity;
                    item.RemainingQuantity = progress.RemainingBaseQuantity;
                    item.CumulativeUnit = progress.BaseUomCode;
                }
            }

            return item;
        }).ToList();

        // Status penghubung persediaan: pembacaan bukti yang sudah ada (read-only). Tidak ada posting stok.
        var links = await PurchasingGoodsReceiptLogisticsLink.GetLinksAsync(_dbContext, [header.Id], cancellationToken);

        return new ProcurementInspectionDetailResponse
        {
            GoodsReceiptId = header.Id,
            GoodsReceiptNumber = header.GRNumber,
            ReceivedDate = header.ReceivedDate,
            DocumentStatus = header.Status,
            SourceSystem = header.SourceSystem,
            LastSyncedAt = header.LastSyncedAt,
            PurchaseOrderId = header.PurchaseOrderId,
            PurchaseOrderNumber = header.PurchaseOrderNumber,
            PurchaseOrderStatus = header.PurchaseOrderStatus,
            PurchaseOrderSourceSystem = header.PurchaseOrderSourceSystem,
            SupplierId = header.SupplierId,
            SupplierName = header.SupplierName,
            SupplierCode = header.SupplierCode,
            StockLink = links.GetValueOrDefault(header.Id),
            Items = items
        };
    }

    // ------------------------------------------------------------------ internal

    private async Task<ReceiptProgress?> LoadCumulativeProgressAsync(Guid purchaseOrderId, CancellationToken cancellationToken)
    {
        var purchaseOrder = await _dbContext.FinPurchaseOrders.AsNoTracking()
            .Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.Id == purchaseOrderId && !x.IsDelete, cancellationToken);
        if (purchaseOrder is null) return null;

        var goodsReceipts = await _dbContext.FinGoodsReceipts.AsNoTracking()
            .Include(x => x.Items)
            .Where(x => x.PurchaseOrderId == purchaseOrderId && !x.IsDelete)
            .ToListAsync(cancellationToken);

        // null untuk PO yang bukan bersumber BLM (kalkulator tidak menghitung PO lama).
        return PurchasingReceiptQuantityCalculator.CalculateForPurchaseOrder(purchaseOrder, goodsReceipts);
    }

    private IQueryable<FinGoodsReceiptItem> BaseItems() =>
        _dbContext.FinGoodsReceiptItems.AsNoTracking()
            .Where(x => !x.IsDelete && !x.GoodsReceipt!.IsDelete);

    private sealed record NormalizedFilters(
        DateOnly? ReceivedDateFrom,
        DateOnly? ReceivedDateTo,
        Guid? SupplierId,
        string? ConditionCode,
        string? SourceSystem,
        string? DocumentStatus,
        string? Search);

    private static NormalizedFilters Normalize(ProcurementInspectionQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.ReceivedDateFrom is { } dateFrom && query.ReceivedDateTo is { } dateTo && dateFrom > dateTo)
            throw new LogisticsReceivingValidationException("Tanggal awal penerimaan tidak boleh lebih besar dari tanggal akhir.");

        var condition = Upper(query.ConditionCode);
        if (condition is not null && !ProcurementInspectionConditionFilters.All.Contains(condition))
            throw new LogisticsReceivingValidationException("Kondisi barang tidak dikenal. Gunakan GOOD, DAMAGED, PENDING_INSPECTION, atau NOT_RECORDED.");

        var source = Upper(query.SourceSystem);
        if (source is not null && source != FinPurchasingSourceSystems.Quilvian && source != FinPurchasingSourceSystems.Blm)
            throw new LogisticsReceivingValidationException("Sumber dokumen tidak dikenal. Gunakan QUILVIAN atau BLM.");

        var documentStatus = Upper(query.DocumentStatus);
        if (documentStatus is not null
            && documentStatus != FinGoodsReceiptStatuses.Received
            && documentStatus != FinGoodsReceiptStatuses.Cancelled)
            throw new LogisticsReceivingValidationException("Status dokumen tidak dikenal. Gunakan RECEIVED atau CANCELLED.");

        Guid? supplier = query.SupplierId;
        if (supplier == Guid.Empty) supplier = null;

        return new NormalizedFilters(
            query.ReceivedDateFrom,
            query.ReceivedDateTo,
            supplier,
            condition,
            source,
            documentStatus,
            Trimmed(query.Search));
    }

    private static IQueryable<FinGoodsReceiptItem> ApplyFilters(IQueryable<FinGoodsReceiptItem> items, NormalizedFilters filters)
    {
        if (filters.ReceivedDateFrom is { } dateFrom) items = items.Where(x => x.GoodsReceipt!.ReceivedDate >= dateFrom);
        if (filters.ReceivedDateTo is { } dateTo) items = items.Where(x => x.GoodsReceipt!.ReceivedDate <= dateTo);

        if (filters.SupplierId is { } supplierId)
            items = items.Where(x => x.GoodsReceipt!.PurchaseOrder!.SupplierId == supplierId);

        if (filters.ConditionCode is { } condition)
        {
            items = condition == ProcurementInspectionConditionFilters.NotRecorded
                ? items.Where(x => x.ConditionCode == null)
                : items.Where(x => x.ConditionCode == condition);
        }

        if (filters.SourceSystem is { } source) items = items.Where(x => x.GoodsReceipt!.SourceSystem == source);
        if (filters.DocumentStatus is { } status) items = items.Where(x => x.GoodsReceipt!.Status == status);

        if (filters.Search is not null)
        {
            // Satu kotak cari: nama barang, nomor Goods Receipt, atau nomor Purchase Order.
            var pattern = ContainsPattern(filters.Search);
            items = items.Where(x =>
                EF.Functions.ILike(x.PurchaseOrderItem!.ProductName, pattern, LikeEscape)
                || EF.Functions.ILike(x.GoodsReceipt!.GRNumber, pattern, LikeEscape)
                || EF.Functions.ILike(x.GoodsReceipt!.PurchaseOrder!.PONumber, pattern, LikeEscape));
        }

        return items;
    }

    /// <summary>Hanya kolom yang dibutuhkan halaman; sengaja tanpa harga, total, atau nilai finansial PO.</summary>
    private static IQueryable<InspectionRow> Project(IQueryable<FinGoodsReceiptItem> items) =>
        items.Select(x => new InspectionRow
        {
            GoodsReceiptItemId = x.Id,
            GoodsReceiptId = x.GoodsReceiptId,
            GoodsReceiptNumber = x.GoodsReceipt!.GRNumber,
            ReceivedDate = x.GoodsReceipt!.ReceivedDate,
            DocumentStatus = x.GoodsReceipt!.Status,
            ReceiptSourceSystem = x.GoodsReceipt!.SourceSystem,
            PurchaseOrderId = x.GoodsReceipt!.PurchaseOrderId,
            PurchaseOrderNumber = x.GoodsReceipt!.PurchaseOrder!.PONumber,
            SupplierId = x.GoodsReceipt!.PurchaseOrder!.SupplierId,
            SupplierName = x.GoodsReceipt!.PurchaseOrder!.Supplier!.SupplierName,
            PurchaseOrderItemId = x.PurchaseOrderItemId,
            ProductName = x.PurchaseOrderItem!.ProductName,
            ProductCategory = x.PurchaseOrderItem!.ProductCategory,
            OrderUnit = x.PurchaseOrderItem!.Unit,
            OrderQuantity = x.PurchaseOrderItem!.Quantity,
            OrderBaseQuantity = x.PurchaseOrderItem!.BaseQuantity,
            OrderQuantityPerBaseUnit = x.PurchaseOrderItem!.QuantityPerBaseUnit,
            OrderBaseUomCode = x.PurchaseOrderItem!.BaseUomCode,
            ReceivedQuantity = x.ReceivedQuantity,
            BatchNumber = x.BatchNumber,
            ExpiredDate = x.ExpiredDate,
            Notes = x.Notes,
            ConditionCode = x.ConditionCode,
            SourceQuantity = x.SourceQuantity,
            SourceUomCode = x.SourceUomCode,
            QuantityPerBaseUnit = x.QuantityPerBaseUnit,
            BaseUomCode = x.BaseUomCode,
            BaseQuantity = x.BaseQuantity,
            AcceptedBaseQuantity = x.AcceptedBaseQuantity
        });

    /// <summary>
    /// Mengisi satu baris respons. Kuantitas turunan SELALU lewat <see cref="PurchasingReceiptQuantityCalculator"/>: satu item PO dan satu baris GR.
    /// Aturan yang dijaga di sini:
    ///  - GR dibatalkan: tidak ada kuantitas;
    ///  - tanpa kode kondisi (GR lokal/legacy): kondisi dan jumlah sah TIDAK diketahui — bukan GOOD, bukan 0;
    ///  - konversi tidak pasti: jumlah sah, menunggu periksa, dan rusak null; hanya fakta sumber yang ditampilkan.
    /// </summary>
    private static void Populate(ProcurementInspectionItemResponse target, InspectionRow row)
    {
        target.GoodsReceiptId = row.GoodsReceiptId;
        target.GoodsReceiptItemId = row.GoodsReceiptItemId;
        target.GoodsReceiptNumber = row.GoodsReceiptNumber;
        target.ReceivedDate = row.ReceivedDate;
        target.DocumentStatus = row.DocumentStatus;
        target.SourceSystem = row.ReceiptSourceSystem;
        target.PurchaseOrderId = row.PurchaseOrderId;
        target.PurchaseOrderNumber = row.PurchaseOrderNumber;
        target.SupplierId = row.SupplierId;
        target.SupplierName = row.SupplierName;
        target.PurchaseOrderItemId = row.PurchaseOrderItemId;
        target.ProductName = row.ProductName;
        target.ProductCategory = row.ProductCategory;
        target.BatchNumber = row.BatchNumber;
        target.ExpiredDate = row.ExpiredDate;
        target.Notes = row.Notes;

        var hasInspectionData = !string.IsNullOrWhiteSpace(row.ConditionCode);
        target.ConditionCode = row.ConditionCode;
        target.ConditionStatus = hasInspectionData ? row.ConditionCode! : ProcurementInspectionConditionFilters.NotRecorded;
        target.InspectionDataAvailable = hasInspectionData;

        if (string.Equals(row.DocumentStatus, FinGoodsReceiptStatuses.Cancelled, StringComparison.Ordinal))
        {
            target.QuantityState = ProcurementInspectionQuantityStates.Cancelled;
            target.QuantityNote = "Dokumen penerimaan dibatalkan di Finance; jumlah tidak dihitung.";
            return;
        }

        if (!hasInspectionData)
        {
            target.QuantityState = ProcurementInspectionQuantityStates.NotRecorded;
            target.QuantityNote = "Belum ada data pemeriksaan.";

            // Hanya GR lokal yang membawa jumlah terima satuan PO sebagai fakta; GR BLM tanpa metadata dianggap tidak lengkap.
            if (!string.Equals(row.ReceiptSourceSystem, FinPurchasingSourceSystems.Blm, StringComparison.Ordinal))
            {
                target.QuantityBasis = ProcurementInspectionQuantityBases.PurchaseOrderUnit;
                target.QuantityUnit = row.OrderUnit;
                target.OrderedQuantity = row.OrderQuantity;
                target.PhysicalQuantity = row.ReceivedQuantity;
            }

            return;
        }

        target.SourceQuantity = row.SourceQuantity;
        target.SourceUomCode = row.SourceUomCode;
        target.QuantityBasis = ProcurementInspectionQuantityBases.BaseUnit;

        var progress = PurchasingReceiptQuantityCalculator.Calculate(
            [new ReceiptOrderItemInput(row.PurchaseOrderItemId, row.OrderBaseQuantity, row.OrderQuantityPerBaseUnit, row.OrderBaseUomCode)],
            [new ReceiptLineInput(
                row.PurchaseOrderItemId,
                row.DocumentStatus,
                false,
                row.ConditionCode,
                row.BaseQuantity,
                row.AcceptedBaseQuantity,
                row.QuantityPerBaseUnit,
                row.BaseUomCode)]).Items[0];

        if (progress.IsConversionResolved)
        {
            target.QuantityState = ProcurementInspectionQuantityStates.Resolved;
            target.QuantityUnit = progress.BaseUomCode;
            target.OrderedQuantity = progress.OrderedBaseQuantity;
            target.PhysicalQuantity = progress.PhysicalBaseQuantity;
            target.AcceptedQuantity = progress.AcceptedBaseQuantity;
            target.PendingInspectionQuantity = progress.PendingInspectionBaseQuantity;
            target.DamagedQuantity = progress.DamagedBaseQuantity;
            return;
        }

        // Konversi tidak pasti: tampilkan fakta sumber apa adanya dalam satuan dasar barisnya sendiri; jangan hitung dan jangan tebak.
        target.QuantityState = ProcurementInspectionQuantityStates.Unresolved;
        target.QuantityNote = "Konversi satuan belum pasti; jumlah diterima sah tidak dihitung.";
        target.QuantityUnit = row.BaseUomCode;
        target.PhysicalQuantity = row.BaseQuantity;
    }

    private static string? Trimmed(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? Upper(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();

    private static string ContainsPattern(string term)
    {
        var escaped = term.Trim()
            .Replace(LikeEscape, LikeEscape + LikeEscape)
            .Replace("%", LikeEscape + "%")
            .Replace("_", LikeEscape + "_");
        return "%" + escaped + "%";
    }

    private sealed class InspectionRow
    {
        public Guid GoodsReceiptItemId { get; set; }
        public Guid GoodsReceiptId { get; set; }
        public string GoodsReceiptNumber { get; set; } = string.Empty;
        public DateOnly ReceivedDate { get; set; }
        public string DocumentStatus { get; set; } = string.Empty;
        public string ReceiptSourceSystem { get; set; } = string.Empty;
        public Guid PurchaseOrderId { get; set; }
        public string PurchaseOrderNumber { get; set; } = string.Empty;
        public Guid SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public Guid PurchaseOrderItemId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductCategory { get; set; } = string.Empty;
        public string OrderUnit { get; set; } = string.Empty;
        public decimal OrderQuantity { get; set; }
        public decimal? OrderBaseQuantity { get; set; }
        public decimal? OrderQuantityPerBaseUnit { get; set; }
        public string? OrderBaseUomCode { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public string? BatchNumber { get; set; }
        public DateOnly? ExpiredDate { get; set; }
        public string? Notes { get; set; }
        public string? ConditionCode { get; set; }
        public decimal? SourceQuantity { get; set; }
        public string? SourceUomCode { get; set; }
        public decimal? QuantityPerBaseUnit { get; set; }
        public string? BaseUomCode { get; set; }
        public decimal? BaseQuantity { get; set; }
        public decimal? AcceptedBaseQuantity { get; set; }
    }
}
