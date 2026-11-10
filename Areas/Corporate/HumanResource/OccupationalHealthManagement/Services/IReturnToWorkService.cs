using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Services
{
    /// <summary>
    /// Layanan Asesmen Kembali Bekerja (Return to Work / RTW) & Pembuka Gembok Roster Sub-06 (BE-HRD-293, VR-OHS-07, INT-HR-89)
    /// </summary>
    public interface IReturnToWorkService
    {
        Task<ReturnToWorkAssessmentDto> AssessReturnToWorkAsync(Guid restrictionId, RecordReturnToWorkRequestDto request, Guid doctorUserId, CancellationToken cancellationToken = default);
        Task<List<ReturnToWorkAssessmentDto>> GetRtwHistoryByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);
    }
}
