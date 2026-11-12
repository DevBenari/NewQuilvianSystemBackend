using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services
{
    /// <summary>
    /// Layanan Pendeteksi Ambang Batas Pemicu Kritis FPPE (INT-HR-74)
    /// </summary>
    public interface IFppeTriggerDetectionService
    {
        /// <summary>
        /// Mengambil daftar feed pemicu kritis FPPE yang aktif dari data SIMRS/KMKP
        /// </summary>
        Task<List<FppeTriggerFeedDto>> GetActiveTriggerFeedAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Menerima feed pemicu asinkronus (Event Hook) dari SIMRS/KMKP
        /// </summary>
        Task<FppeTriggerFeedDto> ReceiveTriggerEventAsync(FppeTriggerFeedDto feedItem, CancellationToken cancellationToken = default);
    }
}
