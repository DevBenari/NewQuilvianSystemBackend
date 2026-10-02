namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Services;

/// <summary>
/// Pelanggaran aturan bisnis/konfigurasi pada domain voucher kas kecil.
///
/// Diperkenalkan BE-BKC-034 karena <c>BillingNumberSeriesService.AllocatePettyCashVoucherNumberAsync</c>
/// wajib menyerahkan <c>Func&lt;string, Exception&gt;</c> milik domainnya sendiri, persis seperti
/// empat jenis nomor yang sudah ada menyerahkan <c>BillingInvoiceValidationException</c>,
/// <c>BillingDepositValidationException</c>, <c>BillingSettlementValidationException</c>, dan
/// <c>CashierShiftValidationException</c>.
///
/// Ditempatkan pada file tersendiri — bukan di dalam <c>PettyCashVoucherService.cs</c> seperti
/// keempat saudaranya — semata karena service itu baru lahir pada BE-BKC-037. BE-BKC-037
/// memakai ulang exception ini apa adanya dan TIDAK mendeklarasikannya kembali.
/// </summary>
public sealed class PettyCashVoucherValidationException(string message) : Exception(message);
