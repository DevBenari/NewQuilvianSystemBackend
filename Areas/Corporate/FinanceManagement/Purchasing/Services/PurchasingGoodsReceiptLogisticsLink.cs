using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Services;

/// <summary>Fakta satu baris GR yang relevan bagi status penghubung. Kuantitas dalam satuan dasar (BLM); null untuk GR lokal.</summary>
public sealed record GoodsReceiptLinkItemFact(string? ConditionCode, decimal? BaseQuantity, decimal? AcceptedBaseQuantity);

/// <summary>
/// FIN-BE-LGS-LINK-001. Penurunan status penghubung GR Finance → Logistik. Fungsi murni + pembaca bukti; TIDAK menulis apa pun,
/// TIDAK memanggil layanan stok Farmasi, dan TIDAK menganggap GR Finance sebagai penerimaan Logistik yang sah.
///
/// Satu-satunya bukti hubungan yang diakui adalah mutasi stok StockIn (PhmDrugStockMutation) yang menunjuk GR itu sebagai dokumen
/// sumber: <c>SourceDocumentType = "GoodsReceipt"</c> (konstanta yang sudah ada pada <see cref="DrugStockSourceDocumentTypes"/>) dan
/// <c>SourceDocumentId</c> = id GR atau id barisnya. Saat ini tidak ada kode yang menulis mutasi seperti itu, sehingga seluruh GR
/// berstatus NOT_LINKED sampai keputusan LGS-OQ-017 diimplementasikan oleh pemilik persediaan. Konvensi id sumber (GR atau baris GR)
/// belum diputuskan, karena itu kedua bentuk dibaca.
/// </summary>
public static class PurchasingGoodsReceiptLogisticsLink
{
    public static string SourceDocumentType => DrugStockSourceDocumentTypes.GoodsReceipt;

    // ---------------------------------------------------------------- penurunan murni

    public static GoodsReceiptLogisticsLinkResponse Derive(
        string goodsReceiptStatus, string sourceSystem, IReadOnlyCollection<GoodsReceiptLinkItemFact> items,
        int postedMutationCount, DateTime? lastPostedAt)
    {
        var isExternal = string.Equals(sourceSystem, FinPurchasingSourceSystems.Blm, StringComparison.OrdinalIgnoreCase);
        var response = new GoodsReceiptLogisticsLinkResponse
        {
            ItemCount = items.Count,
            ConditionTracked = isExternal,
            PostedMutationCount = postedMutationCount,
            LastPostedAt = lastPostedAt
        };

        if (isExternal)
        {
            response.PendingInspectionItemCount = items.Count(x => IsCondition(x, FinGoodsReceiptConditionCodes.PendingInspection));
            response.DamagedItemCount = items.Count(x => IsCondition(x, FinGoodsReceiptConditionCodes.Damaged));
            response.PartiallyAcceptedItemCount = items.Count(x =>
                !IsCondition(x, FinGoodsReceiptConditionCodes.PendingInspection) && !IsCondition(x, FinGoodsReceiptConditionCodes.Damaged)
                && x.BaseQuantity.HasValue && x.AcceptedBaseQuantity.HasValue && x.AcceptedBaseQuantity.Value < x.BaseQuantity.Value);
        }

        if (string.Equals(goodsReceiptStatus, FinGoodsReceiptStatuses.Cancelled, StringComparison.OrdinalIgnoreCase))
        {
            response.Status = PurchasingLogisticsLinkStatuses.Cancelled;
            response.Reasons.Add("GR dibatalkan; tidak ada hubungan ke Logistik yang berlaku.");
            return response;
        }

        if (postedMutationCount > 0)
        {
            response.Status = PurchasingLogisticsLinkStatuses.PostedToStock;
            response.Reasons.Add($"Ada {postedMutationCount} mutasi persediaan resmi yang menunjuk GR ini sebagai dokumen sumber.");
        }
        else if (response.PendingInspectionItemCount > 0)
        {
            response.Status = PurchasingLogisticsLinkStatuses.AwaitingInspection;
            response.Reasons.Add($"{response.PendingInspectionItemCount} baris masih menunggu pemeriksaan; belum dihitung sebagai penerimaan sah.");
        }
        else if (response.DamagedItemCount > 0 || response.PartiallyAcceptedItemCount > 0)
        {
            response.Status = PurchasingLogisticsLinkStatuses.HasDiscrepancy;
            if (response.DamagedItemCount > 0) response.Reasons.Add($"{response.DamagedItemCount} baris rusak; bukan penerimaan sah.");
            if (response.PartiallyAcceptedItemCount > 0) response.Reasons.Add($"{response.PartiallyAcceptedItemCount} baris diterima sebagian (kuantitas sah lebih kecil dari fisik).");
        }
        else
        {
            response.Status = PurchasingLogisticsLinkStatuses.NotLinked;
        }

        if (postedMutationCount == 0)
        {
            response.Reasons.Add("Belum ada mutasi persediaan Logistik yang bersumber dari GR ini. GR Finance tidak otomatis menambah stok.");
            response.Reasons.Add(isExternal
                ? "Pemetaan produk BLM ke produk Quilvian belum tersedia; stok tidak boleh diposting dari tebakan produk."
                : "Baris GR lokal hanya memuat nama barang; belum ada identitas produk Logistik.");
            response.Reasons.Add("Konfirmasi penerimaan fisik oleh Logistik belum tersedia untuk dibaca Finance.");
        }

        return response;
    }

    private static bool IsCondition(GoodsReceiptLinkItemFact item, string code) =>
        string.Equals(item.ConditionCode, code, StringComparison.OrdinalIgnoreCase);

    // ---------------------------------------------------------------- pembaca bukti (baca-saja)

    /// <summary>Status penghubung untuk sekumpulan GR (satu query item + satu query bukti).</summary>
    public static async Task<Dictionary<Guid, GoodsReceiptLogisticsLinkResponse>> GetLinksAsync(
        ApplicationDbContext dbContext, IReadOnlyCollection<Guid> goodsReceiptIds, CancellationToken cancellationToken)
    {
        var result = new Dictionary<Guid, GoodsReceiptLogisticsLinkResponse>();
        if (goodsReceiptIds.Count == 0) return result;

        var ids = goodsReceiptIds.Distinct().ToList();
        var receipts = await dbContext.FinGoodsReceipts.AsNoTracking()
            .Where(x => ids.Contains(x.Id) && !x.IsDelete)
            .Select(x => new { x.Id, x.Status, x.SourceSystem })
            .ToListAsync(cancellationToken);
        var items = await dbContext.FinGoodsReceiptItems.AsNoTracking()
            .Where(x => ids.Contains(x.GoodsReceiptId) && !x.IsDelete)
            .Select(x => new { x.Id, x.GoodsReceiptId, x.ConditionCode, x.BaseQuantity, x.AcceptedBaseQuantity })
            .ToListAsync(cancellationToken);
        var evidence = await LoadEvidenceAsync(dbContext, ids, items.Select(x => (x.Id, x.GoodsReceiptId)).ToList(), cancellationToken);

        foreach (var receipt in receipts)
        {
            var facts = items.Where(x => x.GoodsReceiptId == receipt.Id)
                .Select(x => new GoodsReceiptLinkItemFact(x.ConditionCode, x.BaseQuantity, x.AcceptedBaseQuantity)).ToList();
            evidence.TryGetValue(receipt.Id, out var posted);
            result[receipt.Id] = Derive(receipt.Status, receipt.SourceSystem, facts, posted.Count, posted.Last);
        }

        return result;
    }

    /// <summary>True bila sudah ada mutasi persediaan StockIn yang menunjuk GR ini (atau barisnya). Dipakai guard koreksi/pembatalan GR.</summary>
    public static async Task<bool> HasPostedStockAsync(ApplicationDbContext dbContext, Guid goodsReceiptId, CancellationToken cancellationToken)
    {
        var itemIds = await dbContext.FinGoodsReceiptItems.AsNoTracking()
            .Where(x => x.GoodsReceiptId == goodsReceiptId).Select(x => x.Id).ToListAsync(cancellationToken);
        var referenced = itemIds.Append(goodsReceiptId).ToList();
        return await dbContext.PhmDrugStockMutations.AsNoTracking()
            .AnyAsync(x => !x.IsDelete && x.SourceDocumentType == SourceDocumentType && x.SourceDocumentId != null
                && x.QuantityChange > 0m && referenced.Contains(x.SourceDocumentId.Value), cancellationToken);
    }

    private static async Task<Dictionary<Guid, (int Count, DateTime? Last)>> LoadEvidenceAsync(
        ApplicationDbContext dbContext, IReadOnlyCollection<Guid> goodsReceiptIds, IReadOnlyCollection<(Guid ItemId, Guid GoodsReceiptId)> itemMap,
        CancellationToken cancellationToken)
    {
        var referenced = goodsReceiptIds.Concat(itemMap.Select(x => x.ItemId)).Distinct().ToList();
        var mutations = await dbContext.PhmDrugStockMutations.AsNoTracking()
            .Where(x => !x.IsDelete && x.SourceDocumentType == SourceDocumentType && x.SourceDocumentId != null
                && x.QuantityChange > 0m && referenced.Contains(x.SourceDocumentId.Value))
            .Select(x => new { SourceId = x.SourceDocumentId!.Value, x.CreateDateTime })
            .ToListAsync(cancellationToken);

        var itemToReceipt = itemMap.ToDictionary(x => x.ItemId, x => x.GoodsReceiptId);
        var evidence = new Dictionary<Guid, (int Count, DateTime? Last)>();
        foreach (var mutation in mutations)
        {
            var receiptId = itemToReceipt.TryGetValue(mutation.SourceId, out var mapped) ? mapped : mutation.SourceId;
            var current = evidence.GetValueOrDefault(receiptId);
            evidence[receiptId] = (current.Count + 1, current.Last is null || mutation.CreateDateTime > current.Last ? mutation.CreateDateTime : current.Last);
        }

        return evidence;
    }

    // ---------------------------------------------------------------- ringkasan monitoring

    public static async Task<GoodsReceiptLogisticsSummaryResponse> GetSummaryAsync(
        ApplicationDbContext dbContext, GoodsReceiptLogisticsSummaryQuery query, CancellationToken cancellationToken)
    {
        var received = dbContext.FinGoodsReceipts.AsNoTracking()
            .Where(x => !x.IsDelete && x.Status == FinGoodsReceiptStatuses.Received);
        if (query.SupplierId.HasValue && query.SupplierId.Value != Guid.Empty)
            received = received.Where(x => x.PurchaseOrder!.SupplierId == query.SupplierId.Value);
        if (query.ReceivedDateFrom.HasValue) received = received.Where(x => x.ReceivedDate >= query.ReceivedDateFrom.Value);
        if (query.ReceivedDateTo.HasValue) received = received.Where(x => x.ReceivedDate <= query.ReceivedDateTo.Value);

        var total = await received.CountAsync(cancellationToken);

        var items = dbContext.FinGoodsReceiptItems.AsNoTracking().Where(x => !x.IsDelete);
        var pending = (await items.Where(x => received.Any(r => r.Id == x.GoodsReceiptId) && x.ConditionCode == FinGoodsReceiptConditionCodes.PendingInspection)
            .Select(x => x.GoodsReceiptId).Distinct().ToListAsync(cancellationToken)).ToHashSet();
        var discrepancy = (await items.Where(x => received.Any(r => r.Id == x.GoodsReceiptId)
                && (x.ConditionCode == FinGoodsReceiptConditionCodes.Damaged
                    || (x.ConditionCode != FinGoodsReceiptConditionCodes.PendingInspection
                        && x.BaseQuantity != null && x.AcceptedBaseQuantity != null && x.AcceptedBaseQuantity < x.BaseQuantity)))
            .Select(x => x.GoodsReceiptId).Distinct().ToListAsync(cancellationToken)).ToHashSet();

        // Bukti mutasi: himpunan id sumber yang sangat kecil (nol selama integrasi belum ada), lalu dipetakan ke GR.
        var sourceIds = (await dbContext.PhmDrugStockMutations.AsNoTracking()
            .Where(x => !x.IsDelete && x.SourceDocumentType == SourceDocumentType && x.SourceDocumentId != null && x.QuantityChange > 0m)
            .Select(x => x.SourceDocumentId!.Value).Distinct().ToListAsync(cancellationToken)).ToList();
        var posted = new HashSet<Guid>();
        if (sourceIds.Count > 0)
        {
            var direct = await received.Where(x => sourceIds.Contains(x.Id)).Select(x => x.Id).ToListAsync(cancellationToken);
            var viaItems = await items.Where(x => sourceIds.Contains(x.Id) && received.Any(r => r.Id == x.GoodsReceiptId))
                .Select(x => x.GoodsReceiptId).Distinct().ToListAsync(cancellationToken);
            posted = direct.Concat(viaItems).ToHashSet();
        }

        // Precedensi sama dengan Derive: bukti mutasi > menunggu periksa > selisih > belum terhubung.
        var awaiting = pending.Count(id => !posted.Contains(id));
        var discrepant = discrepancy.Count(id => !posted.Contains(id) && !pending.Contains(id));
        var response = new GoodsReceiptLogisticsSummaryResponse
        {
            TotalReceived = total,
            PostedToStock = posted.Count,
            AwaitingInspection = awaiting,
            HasDiscrepancy = discrepant
        };
        response.NotLinked = Math.Max(0, total - response.PostedToStock - awaiting - discrepant);
        response.Undetermined =
        [
            new() { Code = "AWAITING_PRODUCT_MAPPING", Reason = "Pemetaan produk BLM ke produk Quilvian belum tersedia sehingga jumlahnya tidak dapat ditentukan." },
            new() { Code = "AWAITING_LOGISTICS_CONFIRMATION", Reason = "Konfirmasi penerimaan Logistik belum tersedia untuk dibaca Finance." },
            new() { Code = "SYNC_FAILED", Reason = "Belum ada integrasi sinkronisasi (pengolah kiriman BLM belum dibangun), sehingga kegagalan sinkronisasi tidak dapat dilacak." }
        ];
        return response;
    }
}

/// <summary>Pembungkus DI untuk controller (controller tidak boleh memakai DbContext langsung). Murni baca.</summary>
public sealed class FinanceGoodsReceiptLogisticsLinkService
{
    private readonly ApplicationDbContext _dbContext;

    public FinanceGoodsReceiptLogisticsLinkService(ApplicationDbContext dbContext) => _dbContext = dbContext;

    public Task<Dictionary<Guid, GoodsReceiptLogisticsLinkResponse>> GetLinksAsync(
        IReadOnlyCollection<Guid> goodsReceiptIds, CancellationToken cancellationToken) =>
        PurchasingGoodsReceiptLogisticsLink.GetLinksAsync(_dbContext, goodsReceiptIds, cancellationToken);

    public Task<GoodsReceiptLogisticsSummaryResponse> GetSummaryAsync(
        GoodsReceiptLogisticsSummaryQuery query, CancellationToken cancellationToken) =>
        PurchasingGoodsReceiptLogisticsLink.GetSummaryAsync(_dbContext, query, cancellationToken);
}
