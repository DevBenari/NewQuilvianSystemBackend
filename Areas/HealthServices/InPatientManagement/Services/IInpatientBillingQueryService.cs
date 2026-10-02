using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Kontrak layanan kueri integrasi status penagihan kasir untuk modul rawat inap.
    /// Memisahkan tampilan status operasional bebas rupiah dari rincian finansial.
    /// </summary>
    public interface IInpatientBillingQueryService
    {
        /// <summary>
        /// Mengambil ringkasan status operasional penagihan kasir tanpa nominal rupiah untuk perawat bangsal.
        /// </summary>
        Task<InpatientBillingStatusResponseDto?> GetOperationalBillingStatusAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengambil rincian finansial nominal tagihan kasir (khusus pengguna dengan wewenang keuangan).
        /// </summary>
        Task<InpatientBillingDetailsResponseDto?> GetFinancialDetailsAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default);
    }
}
