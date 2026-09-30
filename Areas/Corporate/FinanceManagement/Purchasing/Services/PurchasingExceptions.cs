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

/// <summary>FIN-VAL-102: penyetuju tidak sesuai jenjang nominal (403), bukan 422 — checkpoint otorisasi, bukan validasi data.</summary>
public sealed class PurchasingForbiddenException(string message) : Exception(message);
