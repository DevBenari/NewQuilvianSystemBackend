using System;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Services
{
    /// <summary>
    /// Layanan Radar Kedaluwarsa Bertingkat H-90/60/30 & Pengamanan Shift Dinas Buddy System (VR-TRN-09..12, INT-HR-79, INT-HR-77)
    /// </summary>
    public interface ITrainingExpiryRadarService
    {
        /// <summary>
        /// Mengambil ringkasan dashboard radar pegawai berisiko kedaluwarsa pelatihan wajib (H-90, H-60, H-30, Overdue H-0)
        /// </summary>
        Task<CertificateExpiryRadarSummaryDto> GetExpiringRadarSummaryAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengirimkan peringatan dini massal melalui multi-kanal (WhatsApp, Email, ESS) ke pegawai di zona radar
        /// </summary>
        Task<BroadcastExpiryAlertResponseDto> BroadcastAlertsAsync(BroadcastExpiryAlertRequestDto request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Menegakkan safeguard penugasan shift dinas pendamping (Buddy System) & pembekuan wewenang Level 3 supervisi
        /// </summary>
        Task<ShiftBuddySafeguardResponseDto> EnforceShiftBuddySafeguardAsync(Guid staffId, AssignShiftBuddySafeguardRequestDto request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengambil portofolio seluruh sertifikat keahlian dan status kepatuhan seorang pegawai
        /// </summary>
        Task<StaffCertificatePortfolioDto?> GetStaffPortfolioAsync(Guid staffId, CancellationToken cancellationToken = default);
    }
}
