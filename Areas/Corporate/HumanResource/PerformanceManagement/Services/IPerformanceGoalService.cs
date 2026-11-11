using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services
{
    /// <summary>
    /// Kontrak Service Cascading Sasaran Mutu Unit ke Individu & Validasi Bobot 100% (BE-HRD-258, PRF-B2, VR-PRF-02, VR-PRF-04)
    /// </summary>
    public interface IPerformanceGoalService
    {
        Task<List<PerformanceGoalDto>> GetGoalsByStaffAsync(Guid staffId, Guid cycleId, CancellationToken cancellationToken = default);
        Task<List<PerformanceGoalDto>> CascadeGoalsAsync(CascadePerformanceGoalsRequestDto dto, Guid managerId, CancellationToken cancellationToken = default);
        Task<bool> ApproveGoalsAsync(Guid cycleId, Guid staffId, Guid managerId, CancellationToken cancellationToken = default);
    }
}
