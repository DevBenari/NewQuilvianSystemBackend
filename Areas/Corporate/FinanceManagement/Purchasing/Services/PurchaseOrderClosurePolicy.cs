using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Services;

/// <summary>GR milik satu PO (hanya yang belum dihapus lunak). Dipakai <see cref="PurchaseOrderClosurePolicy"/>.</summary>
public sealed record ClosureGoodsReceipt(Guid Id, string Number, string Status);

/// <summary>Tukar Faktur yang merujuk PO atau salah satu GR-nya (hanya yang belum dihapus lunak).</summary>
public sealed record ClosureExchange(Guid Id, string Number, Guid? GoodsReceiptId, string Status);

/// <summary>Purchasing Invoice milik salah satu Tukar Faktur di atas (hanya yang belum dihapus lunak).</summary>
public sealed record ClosureInvoice(Guid InvoiceExchangeId, string Number, string Status);

public sealed record PurchaseOrderClosureAssessment(bool CanClose, IReadOnlyList<string> BlockingReasons);

/// <summary>
/// Aturan penutupan administratif Purchase Order (state-transition-matrix.md §B.1: FULLY_RECEIVED →
/// CLOSED). Fungsi murni tanpa akses data supaya aturan yang sama dipakai penutupan sungguhan
/// (<c>FinancePurchaseOrderService.CloseAsync</c>) dan kesiapan penutupan pada jejak dokumen, dan
/// mudah diuji.
///
/// Syarat matriks "seluruh Tukar Faktur turunan sudah LINKED_TO_INVOICE atau CANCELLED" TIDAK cukup
/// dengan sendirinya: PO yang punya GR tetapi belum satu pun Tukar Faktur memiliki himpunan Tukar
/// Faktur kosong, dan pemeriksaan "semua selesai" pada himpunan kosong selalu benar. Karena itu
/// setiap GR aktif WAJIB tertutup oleh minimal satu Tukar Faktur aktif yang sudah menjadi invoice
/// APPROVED, dan PO tanpa GR aktif tidak dapat ditutup lewat jalur ini.
///
/// Menutup PO hanyalah penutupan administratif pengadaan. Aturan ini tidak membuat pembayaran,
/// tidak menyentuh saldo utang, dan tidak membatalkan dokumen apa pun.
/// </summary>
public static class PurchaseOrderClosurePolicy
{
    public static PurchaseOrderClosureAssessment Assess(
        string purchaseOrderStatus,
        IReadOnlyCollection<ClosureGoodsReceipt> goodsReceipts,
        IReadOnlyCollection<ClosureExchange> exchanges,
        IReadOnlyCollection<ClosureInvoice> invoices)
    {
        var reasons = new List<string>();

        if (purchaseOrderStatus == FinPurchaseOrderStatuses.Closed)
        {
            reasons.Add("Purchase Order sudah ditutup.");
            return new PurchaseOrderClosureAssessment(false, reasons);
        }

        if (purchaseOrderStatus != FinPurchaseOrderStatuses.FullyReceived)
        {
            reasons.Add($"Purchase Order berstatus {purchaseOrderStatus}; hanya PO yang sudah diterima penuh (FULLY_RECEIVED) yang dapat ditutup.");
            return new PurchaseOrderClosureAssessment(false, reasons);
        }

        var activeGoodsReceipts = goodsReceipts
            .Where(x => x.Status != FinGoodsReceiptStatuses.Cancelled)
            .ToList();
        if (activeGoodsReceipts.Count == 0)
            reasons.Add("Purchase Order tidak memiliki Tanda Terima Barang aktif.");

        var activeExchanges = exchanges
            .Where(x => x.Status != FinInvoiceExchangeStatuses.Cancelled)
            .ToList();

        foreach (var goodsReceipt in activeGoodsReceipts)
        {
            var covering = activeExchanges.Where(x => x.GoodsReceiptId == goodsReceipt.Id).ToList();
            if (covering.Count == 0)
            {
                reasons.Add($"Tanda Terima Barang {goodsReceipt.Number} belum memiliki Tukar Faktur.");
                continue;
            }

            if (!covering.Any(x => x.Status == FinInvoiceExchangeStatuses.LinkedToInvoice))
                reasons.Add($"Tanda Terima Barang {goodsReceipt.Number} belum tertutup oleh Tukar Faktur yang sudah menjadi invoice.");
        }

        foreach (var exchange in activeExchanges)
        {
            if (exchange.Status == FinInvoiceExchangeStatuses.Received)
            {
                reasons.Add($"Tukar Faktur {exchange.Number} belum menjadi Purchasing Invoice.");
                continue;
            }

            var exchangeInvoices = invoices.Where(x => x.InvoiceExchangeId == exchange.Id).ToList();
            if (!exchangeInvoices.Any(x => x.Status == FinPurchasingInvoiceStatuses.Approved))
                reasons.Add($"Tukar Faktur {exchange.Number} berstatus terhubung tetapi tidak memiliki Purchasing Invoice yang disetujui.");
        }

        foreach (var invoice in invoices.Where(x =>
                     x.Status is FinPurchasingInvoiceStatuses.Draft or FinPurchasingInvoiceStatuses.PendingApproval))
        {
            reasons.Add($"Purchasing Invoice {invoice.Number} masih berstatus {invoice.Status}.");
        }

        return new PurchaseOrderClosureAssessment(reasons.Count == 0, reasons);
    }
}
