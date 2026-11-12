using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Services;

/// <summary>Satu item PO bersumber BLM sebagaimana tersimpan: jumlah dipesan SUDAH dalam satuan dasar produk.</summary>
public sealed record ReceiptOrderItemInput(
    Guid PurchaseOrderItemId,
    decimal? OrderedBaseQuantity,
    decimal? QuantityPerBaseUnit,
    string? BaseUomCode);

/// <summary>
/// Satu baris GR bersumber BLM sebagaimana tersimpan. <see cref="IsDeleted"/> berlaku untuk baris maupun GR
/// induknya; <see cref="GoodsReceiptStatus"/> adalah status GR induk (<see cref="FinGoodsReceiptStatuses"/>).
/// </summary>
public sealed record ReceiptLineInput(
    Guid PurchaseOrderItemId,
    string GoodsReceiptStatus,
    bool IsDeleted,
    string? ConditionCode,
    decimal? BaseQuantity,
    decimal? AcceptedBaseQuantity,
    decimal? QuantityPerBaseUnit,
    string? BaseUomCode);

/// <summary>
/// Kemajuan penerimaan satu item PO, SEMUA dalam satuan dasar produk. Besaran yang tidak dapat dipastikan
/// bernilai <c>null</c>, bukan nol atau tebakan.
/// </summary>
public sealed record ReceiptItemProgress(
    Guid PurchaseOrderItemId,
    string? BaseUomCode,
    decimal? OrderedBaseQuantity,
    decimal PhysicalBaseQuantity,
    decimal AcceptedBaseQuantity,
    decimal PendingInspectionBaseQuantity,
    decimal DamagedBaseQuantity,
    decimal? RemainingBaseQuantity,
    bool IsConversionResolved,
    bool IsFullyReceived);

/// <summary>
/// Hasil hitung seluruh PO. <see cref="Status"/> bernilai salah satu dari <see cref="FinPurchaseOrderStatuses"/>
/// (APPROVED, PARTIALLY_RECEIVED, FULLY_RECEIVED). <see cref="UnmatchedLineCount"/> adalah jumlah baris GR aktif
/// yang tidak menunjuk item PO mana pun; baris seperti itu membuat <see cref="IsConversionResolved"/> bernilai false.
/// </summary>
public sealed record ReceiptProgress(
    IReadOnlyList<ReceiptItemProgress> Items,
    bool IsConversionResolved,
    int UnmatchedLineCount,
    string Status);

/// <summary>
/// BE-FIN-145 (Revisi 23, FIN-DES-142) — penghitung kemajuan penerimaan dan status PO bersumber BLM sebagai
/// fungsi murni: tidak membaca atau menulis basis data, tidak membuka transaksi, tidak punya state. Karena itu
/// dipakai sebagai kelas statis dan TIDAK didaftarkan di <c>Program.cs</c>.
///
/// Yang ditegakkan (02-backend-architecture.md W.11; FIN-DEC-268, 276, 277):
///  - semua besaran dalam SATUAN DASAR produk, presisi penuh, tanpa pembulatan dini;
///  - GR berstatus CANCELLED dan baris/GR yang dihapus tidak ikut dihitung;
///  - baris GR dicocokkan ke item PO HANYA lewat ID item (<see cref="ReceiptLineInput.PurchaseOrderItemId"/>),
///    tidak pernah lewat nama produk;
///  - konversi "pasti" bila isi per satuan &gt; 0 dan satuan dasar baris SAMA dengan satuan dasar item PO. Tidak
///    ada nilai bawaan 1: konversi yang tidak pasti tidak ikut dijumlahkan dan membuat PO tidak pernah
///    FULLY_RECEIVED;
///  - barang rusak dan menunggu pemeriksaan tidak dihitung sah; hanya baris GOOD yang boleh punya jumlah sah;
///  - FULLY_RECEIVED hanya bila SETIAP item sah &gt;= dipesan dan seluruh konversi pasti. Status dinilai
///    per item, bukan dari total lintas item (total lintas produk berbeda satuan tidak bermakna).
///
/// Yang TIDAK dilakukan di sini: menimpa <c>FinPurchaseOrder.Status</c> (pemanggil memutuskan, hanya untuk PO
/// berstatus APPROVED/PARTIALLY_RECEIVED/FULLY_RECEIVED), memuat data, mengambil kunci, atau menulis konflik
/// <c>CONVERSION_UNRESOLVED</c> (tugas pengolah kiriman pada slice integrasi). PO lama (QUILVIAN) tidak dihitung
/// di sini; rumus lama di <c>FinanceGoodsReceiptService</c> tetap berlaku untuknya.
/// </summary>
public static class PurchasingReceiptQuantityCalculator
{
    /// <summary>
    /// Hitung kemajuan PO dari entity. Mengembalikan <c>null</c> untuk PO yang bukan bersumber BLM, sehingga
    /// pemanggil jatuh ke rumus lama. <paramref name="purchaseOrder"/> wajib memuat <c>Items</c>; setiap GR wajib
    /// memuat <c>Items</c>. GR yang bukan milik PO ini diabaikan.
    /// </summary>
    public static ReceiptProgress? CalculateForPurchaseOrder(FinPurchaseOrder purchaseOrder, IEnumerable<FinGoodsReceipt> goodsReceipts)
    {
        ArgumentNullException.ThrowIfNull(purchaseOrder);
        ArgumentNullException.ThrowIfNull(goodsReceipts);

        if (!string.Equals(purchaseOrder.SourceSystem, FinPurchasingSourceSystems.Blm, StringComparison.Ordinal))
            return null;

        var orderItems = purchaseOrder.Items
            .Where(x => !x.IsDelete)
            .Select(x => new ReceiptOrderItemInput(x.Id, x.BaseQuantity, x.QuantityPerBaseUnit, x.BaseUomCode))
            .ToList();

        var lines = new List<ReceiptLineInput>();
        foreach (var goodsReceipt in goodsReceipts.Where(x => x.PurchaseOrderId == purchaseOrder.Id))
        {
            foreach (var item in goodsReceipt.Items)
            {
                lines.Add(new ReceiptLineInput(
                    item.PurchaseOrderItemId,
                    goodsReceipt.Status,
                    goodsReceipt.IsDelete || item.IsDelete,
                    item.ConditionCode,
                    item.BaseQuantity,
                    item.AcceptedBaseQuantity,
                    item.QuantityPerBaseUnit,
                    item.BaseUomCode));
            }
        }

        return Calculate(orderItems, lines);
    }

    /// <summary>Fungsi murni inti; tidak bergantung pada entity.</summary>
    public static ReceiptProgress Calculate(
        IReadOnlyCollection<ReceiptOrderItemInput> orderItems,
        IReadOnlyCollection<ReceiptLineInput> lines)
    {
        ArgumentNullException.ThrowIfNull(orderItems);
        ArgumentNullException.ThrowIfNull(lines);

        // GR dibatalkan dan baris/GR terhapus keluar dari semua jumlah.
        var activeLines = lines
            .Where(x => !x.IsDeleted
                && !string.Equals(x.GoodsReceiptStatus, FinGoodsReceiptStatuses.Cancelled, StringComparison.Ordinal))
            .ToList();

        var knownItemIds = orderItems.Select(x => x.PurchaseOrderItemId).ToHashSet();
        var unmatchedLineCount = activeLines.Count(x => !knownItemIds.Contains(x.PurchaseOrderItemId));

        var items = new List<ReceiptItemProgress>(orderItems.Count);
        foreach (var orderItem in orderItems)
        {
            var itemLines = activeLines.Where(x => x.PurchaseOrderItemId == orderItem.PurchaseOrderItemId).ToList();
            items.Add(CalculateItem(orderItem, itemLines));
        }

        var conversionResolved = unmatchedLineCount == 0 && items.All(x => x.IsConversionResolved);
        var anyAccepted = items.Any(x => x.AcceptedBaseQuantity > 0m);
        var fullyReceived = items.Count > 0 && conversionResolved && items.All(x => x.IsFullyReceived);

        var status = fullyReceived
            ? FinPurchaseOrderStatuses.FullyReceived
            : anyAccepted
                ? FinPurchaseOrderStatuses.PartiallyReceived
                : FinPurchaseOrderStatuses.Approved;

        return new ReceiptProgress(items, conversionResolved, unmatchedLineCount, status);
    }

    /// <summary>
    /// Normalisasi ke satuan dasar: <c>kuantitas asli × isi per satuan</c> (rumus FIN-DEC-276 dengan kedua sisi
    /// dinyatakan dalam satuan dasar). Mengembalikan <c>null</c> bila isi per satuan tidak ada atau tidak positif;
    /// TIDAK pernah memakai 1 sebagai bawaan. Tidak membulatkan.
    /// </summary>
    public static decimal? NormalizeToBase(decimal? sourceQuantity, decimal? quantityPerBaseUnit)
    {
        if (sourceQuantity is null or < 0m) return null;
        if (quantityPerBaseUnit is not > 0m) return null;
        return sourceQuantity.Value * quantityPerBaseUnit.Value;
    }

    /// <summary>
    /// Ubah jumlah satuan dasar ke satuan PO UNTUK TAMPILAN dan kolom lama <c>ReceivedQuantity</c> (numeric 18,2).
    /// Pembagian dan pembulatan 2 desimal hanya di sini (FIN-DES-140); hasil ini tidak boleh dipakai untuk
    /// perhitungan status. Mengembalikan <c>null</c> bila isi per satuan item PO tidak ada atau tidak positif.
    /// </summary>
    public static decimal? ToPurchaseOrderUnit(decimal? baseQuantity, decimal? purchaseOrderQuantityPerBaseUnit)
    {
        if (baseQuantity is null) return null;
        if (purchaseOrderQuantityPerBaseUnit is not > 0m) return null;
        return Math.Round(baseQuantity.Value / purchaseOrderQuantityPerBaseUnit.Value, 2, MidpointRounding.AwayFromZero);
    }

    private static ReceiptItemProgress CalculateItem(ReceiptOrderItemInput orderItem, IReadOnlyList<ReceiptLineInput> itemLines)
    {
        var itemUom = NormalizeUom(orderItem.BaseUomCode);

        // Item PO sendiri harus punya jumlah dipesan, isi per satuan, dan satuan dasar yang sah; bila tidak,
        // tidak ada yang dapat dibandingkan dan semua barisnya dianggap tidak pasti.
        var orderedResolved = orderItem.OrderedBaseQuantity is > 0m
            && orderItem.QuantityPerBaseUnit is > 0m
            && itemUom is not null;

        var conversionResolved = orderedResolved;
        var physical = 0m;
        var accepted = 0m;
        var pendingInspection = 0m;
        var damaged = 0m;

        foreach (var line in itemLines)
        {
            if (!orderedResolved || !IsLineResolved(line, itemUom!))
            {
                // Tidak dijumlahkan dan tidak ditebak.
                conversionResolved = false;
                continue;
            }

            physical += line.BaseQuantity!.Value;
            accepted += line.AcceptedBaseQuantity!.Value;

            if (string.Equals(line.ConditionCode, FinGoodsReceiptConditionCodes.PendingInspection, StringComparison.Ordinal))
                pendingInspection += line.BaseQuantity.Value;
            else if (string.Equals(line.ConditionCode, FinGoodsReceiptConditionCodes.Damaged, StringComparison.Ordinal))
                damaged += line.BaseQuantity.Value;
        }

        decimal? ordered = orderedResolved ? orderItem.OrderedBaseQuantity : null;
        decimal? remaining = ordered is null ? null : Math.Max(0m, ordered.Value - accepted);
        var fullyReceived = ordered is not null && conversionResolved && accepted >= ordered.Value;

        return new ReceiptItemProgress(
            orderItem.PurchaseOrderItemId,
            itemUom,
            ordered,
            physical,
            accepted,
            pendingInspection,
            damaged,
            remaining,
            conversionResolved,
            fullyReceived);
    }

    /// <summary>
    /// Baris pasti bila: isi per satuan &gt; 0; satuan dasar sama dengan satuan dasar item PO; jumlah fisik dan
    /// jumlah sah terisi, tidak negatif, dan sah &lt;= fisik; kondisi dikenal; dan hanya baris GOOD yang boleh
    /// punya jumlah sah (selaras dengan <c>CK_FinGoodsReceiptItem_AcceptedBaseQuantity</c>).
    /// </summary>
    private static bool IsLineResolved(ReceiptLineInput line, string itemUom)
    {
        if (line.QuantityPerBaseUnit is not > 0m) return false;

        var lineUom = NormalizeUom(line.BaseUomCode);
        if (lineUom is null || !string.Equals(lineUom, itemUom, StringComparison.Ordinal)) return false;

        if (line.BaseQuantity is not >= 0m) return false;
        if (line.AcceptedBaseQuantity is not >= 0m) return false;
        if (line.AcceptedBaseQuantity > line.BaseQuantity) return false;

        return line.ConditionCode switch
        {
            FinGoodsReceiptConditionCodes.Good => true,
            FinGoodsReceiptConditionCodes.PendingInspection => line.AcceptedBaseQuantity == 0m,
            FinGoodsReceiptConditionCodes.Damaged => line.AcceptedBaseQuantity == 0m,
            _ => false
        };
    }

    /// <summary>Pangkas spasi dan seragamkan huruf besar (kontrak: satuan dibandingkan sebagai kode, bukan nama). Kosong menjadi null.</summary>
    private static string? NormalizeUom(string? uom)
    {
        if (string.IsNullOrWhiteSpace(uom)) return null;
        return uom.Trim().ToUpperInvariant();
    }
}
