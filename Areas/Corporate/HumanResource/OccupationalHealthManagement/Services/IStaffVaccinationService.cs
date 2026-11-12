using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Services
{
    /// <summary>
    /// Layanan Radar Imunisasi Staf, Log Batch Vaksin & Pemantauan Titer Protektif (BE-HRD-297, VR-OHS-14..16)
    /// </summary>
    public interface IStaffVaccinationService
    {
        Task<VaccinationRecordDto> RecordVaccinationAsync(RecordVaccinationRequestDto request, Guid recordedByUserId, CancellationToken cancellationToken = default);
        Task<List<VaccinationRecordDto>> GetVaccinationsByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);
        Task<HealthSurveillanceResponseDto> RecordAntibodyTiterAsync(RecordAntibodyTiterRequestDto request, Guid recordedByUserId, CancellationToken cancellationToken = default);
        Task<PagedResult<NakesVaccinationRecallDto>> GetBoosterRecallListAsync(string? vaccineType, Guid? departmentId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    }
}
