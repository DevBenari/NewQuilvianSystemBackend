using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Services
{
    /// <summary>
    /// Layanan Surveilans Bahaya Ruangan & Pemantauan Dosis Radiasi TLD (BE-HRD-298, VR-OHS-17)
    /// </summary>
    public interface IHazardSurveillanceService
    {
        Task<HealthSurveillanceResponseDto> RecordRadiationDoseAsync(RecordRadiationDoseRequestDto request, Guid recordedByUserId, CancellationToken cancellationToken = default);
        Task<List<RadiationSurveillanceRadarDto>> GetRadiationRadarAsync(int? year, bool? isNearThresholdOnly, CancellationToken cancellationToken = default);
    }
}
