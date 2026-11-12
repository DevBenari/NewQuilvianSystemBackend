using System;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Services
{
    /// <summary>
    /// Layanan Domain Rekam Medis K3RS Terenkripsi & Field-Level Access Control (BE-HRD-289, HRD-DEC-176, VR-OHS-01, VR-OHS-02)
    /// </summary>
    public interface IHealthRecordService
    {
        Task<HealthRecordClinicalDetailDto> GetClinicalDetailAsync(Guid employeeId, bool isDoctor, CancellationToken cancellationToken = default);
        Task<EmployeeFitnessSummaryDto> GetFitnessSummaryAsync(Guid employeeId, CancellationToken cancellationToken = default);
        Task<EmployeeFitnessSummaryDto> UpdateFitnessStatusAsync(Guid employeeId, UpdateFitnessStatusRequestDto dto, Guid updatedBy, CancellationToken cancellationToken = default);
        Task<MyHealthProfileDto> GetMyHealthProfileAsync(Guid employeeId, CancellationToken cancellationToken = default);
        Task<OhsExecutiveDashboardDto> GetExecutiveDashboardAsync(int? year, Guid? departmentId, CancellationToken cancellationToken = default);
    }
}
