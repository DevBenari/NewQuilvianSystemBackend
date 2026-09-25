using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.DTOs;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

/// <summary>
/// Kontrak layanan perhitungan biaya administrasi rawat inap dan rekonsiliasi alihan rawat jalan.
/// (BKC-DEC-113, BKC-DEC-119, BKC-DEC-121, BKC-DEC-122, BKC-DES-044, BKC-DES-049, BIL-VAL-120, BIL-VAL-126).
/// </summary>
public interface IAdministrationFeeCalculationService
{
    /// <summary>
    /// Menghitung biaya administrasi berdasarkan policy master teraktif, mendukung tipe PERCENTAGE_WITH_CAP (7% cap Rp6jt)
    /// dan pengecualian porsi pasien Rp 0 untuk penjamin paket BPJS (BKC-DEC-113, BKC-DEC-121, BKC-DEC-122).
    /// </summary>
    Task<AdminFeeCalculationResult> CalculateAdministrationFeeAsync(
        AdminFeeCalculationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Memproses rekonsiliasi penggantian biaya administrasi saat pasien rawat jalan dialihkan ke rawat inap (BKC-DEC-119, BIL-VAL-126):
    /// - Jika tagihan rajal belum dibayar: membatalkan (void) item administrasi rajal dengan alasan "SUPERSEDED_BY_INPATIENT_ADMISSION".
    /// - Jika tagihan rajal sudah dibayar: mengalihkan nominal admin rajal menjadi kredit pembayaran (BilRefundableCredit) pengurang tagihan ranap.
    /// </summary>
    Task<ReconcileReferredOutpatientAdminResult> ReconcileReferredOutpatientAdminFeeAsync(
        ReconcileReferredOutpatientAdminRequest request,
        CancellationToken cancellationToken = default);
}
