using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Services
{
    /// <summary>
    /// Layanan Fast-Track Pajanan Okupasional & Surveilans Serologi Serial 1-3-6 Bulan (BE-HRD-295, VR-OHS-09..11, INT-HR-91)
    /// </summary>
    public interface IOccupationalExposureService
    {
        Task<OccupationalExposureDto> RecordSerologyFollowupAsync(Guid exposureId, RecordSerologyFollowupRequestDto request, Guid doctorUserId, CancellationToken cancellationToken = default);
        Task<List<ActiveExposureRadarSummaryDto>> GetActiveExposureRadarAsync(CancellationToken cancellationToken = default);
        Task<OccupationalExposureDto> GetExposureDetailAsync(Guid exposureId, CancellationToken cancellationToken = default);
    }
}
