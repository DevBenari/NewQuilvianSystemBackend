using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Services
{
    /// <summary>
    /// Layanan Evaluasi Kelaikan Kerja Operasional (Fitness to Work) & Validasi Dokter Hiperkes (BE-HRD-290, VR-OHS-03, VR-OHS-13, INT-HR-92)
    /// </summary>
    public interface IFitnessToWorkService
    {
        Task<EmployeeFitnessSummaryDto> AssessFitnessAsync(Guid employeeId, UpdateFitnessStatusRequestDto request, Guid doctorUserId, CancellationToken cancellationToken = default);
        Task<EmployeeFitnessSummaryDto> GetLatestAssessmentAsync(Guid employeeId, CancellationToken cancellationToken = default);
        Task<List<EmployeeFitnessSummaryDto>> GetAssessmentHistoryAsync(Guid employeeId, CancellationToken cancellationToken = default);
    }
}
