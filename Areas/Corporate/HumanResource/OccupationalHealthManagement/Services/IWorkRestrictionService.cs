using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Services
{
    /// <summary>
    /// Layanan Pembatasan Tugas Medis (Work Restriction) & Hard-Stop Roster Sub-06 (BE-HRD-291, VR-OHS-04..06, INT-HR-89)
    /// </summary>
    public interface IWorkRestrictionService
    {
        Task<WorkRestrictionDto> IssueRestrictionAsync(IssueWorkRestrictionRequestDto request, Guid issuedByUserId, CancellationToken cancellationToken = default);
        Task<ScheduleValidationResultDto> ValidateScheduleAsync(Guid employeeId, DateTime shiftDate, string shiftType, Guid? unitId, CancellationToken cancellationToken = default);
        Task<List<WorkRestrictionDto>> GetActiveRestrictionsAsync(CancellationToken cancellationToken = default);
        Task<WorkRestrictionDto?> GetActiveRestrictionByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);
    }
}
