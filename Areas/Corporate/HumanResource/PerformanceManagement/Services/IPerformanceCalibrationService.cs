using System;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services
{
    /// <summary>
    /// Kontrak Layanan Sidang Pleno Kalibrasi Manajemen & Guiding Distribution (BE-HRD-261, PRF-B6, VR-PRF-05, VR-PRF-12)
    /// </summary>
    public interface IPerformanceCalibrationService
    {
        Task<CalibrationDistributionMatrixDto> GetDistributionMatrixAsync(Guid cycleId, CancellationToken cancellationToken = default);
        Task<CalibrationSessionDto> CreateSessionAsync(CreateCalibrationSessionRequestDto dto, Guid createdBy, CancellationToken cancellationToken = default);
        Task<bool> OverrideScoreAsync(OverrideCalibrationScoreRequestDto dto, Guid committeeMemberId, CancellationToken cancellationToken = default);
        Task<bool> FinalizeSessionAsync(Guid sessionId, FinalizeCalibrationSessionRequestDto dto, Guid finalizedBy, CancellationToken cancellationToken = default);
    }
}
