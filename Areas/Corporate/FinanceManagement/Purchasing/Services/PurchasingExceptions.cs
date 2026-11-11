namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Services;

/// <summary>
/// Keluarga exception submodul Purchasing (BE-FIN-032), pola persis
/// PaymentBadRequestException/PaymentValidationException/PaymentConflictException milik
/// FinancePaymentService (BE-FIN-020) — dipakai bersama FinancePurchaseOrderService dan
/// FinanceGoodsReceiptService karena keduanya saling bergantung erat (satu PO, banyak GR).
/// </summary>
public sealed class PurchasingBadRequestException(string message) : Exception(message);
public sealed class PurchasingValidationException(string message) : Exception(message);
public sealed class PurchasingConflictException(string message, Exception? innerException = null) : Exception(message, innerException);

/// <summary>
/// FIN-VAL-315 (BE-FIN-146, Revisi 23): dokumen PO/GR bersumber BLM tidak boleh dibuat, diubah, atau dibatalkan
/// dari Quilvian (FIN-DEC-265). Dipetakan ke 409 dengan kode <see cref="Code"/> pada <c>errors.code</c>. Ini aturan
/// kelayakan data, bukan izin: pengguna yang berhak mencatat GR lokal tetap berhak, tetapi tidak untuk dokumen BLM.
/// </summary>
public sealed class PurchasingExternalDocumentException() : Exception(DefaultMessage)
{
    public const string Code = "PURCHASING_EXTERNAL_DOCUMENT";
    public const string DefaultMessage = "Penerimaan barang untuk PO dari BLM dicatat di BLM. Perubahan dan pembatalannya juga dilakukan dari BLM.";
}

/// <summary>FIN-VAL-102: penyetuju tidak sesuai jenjang nominal (403), bukan 422 — checkpoint otorisasi, bukan validasi data.</summary>
public sealed class PurchasingForbiddenException(string message) : Exception(message);
