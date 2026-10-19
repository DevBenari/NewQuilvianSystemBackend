using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

/// <summary>
/// Kontrak layanan perhitungan biaya administrasi rawat inap.
/// (BKC-DEC-113, BKC-DEC-119, BKC-DEC-121, BKC-DEC-122, BKC-DES-044, BKC-DES-083, BKC-DES-084, BKC-DES-085, BIL-VAL-120, BIL-VAL-159).
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
    /// Mengevaluasi penggantian atau pengkreditan biaya administrasi poli berbasis tautan aktif (BKC-DEC-119, BKC-DES-084, BKC-DES-085).
    /// Baca saja (AsNoTracking), tidak membuka transaksi database.
    /// </summary>
    Task<ReferredAdminReplacementResult> ResolveReferredAdminReplacementAsync(
        BilInvoice invoice,
        RegPatientEncounter encounter,
        CancellationToken cancellationToken = default);
}
