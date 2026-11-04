using System;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public interface IHrdEmploymentReadinessReadService
    {
        Task<RecruitmentResult<SchedulingEligibilityResponse>> GetSchedulingEligibilityAsync(
            Guid workforceProfileId,
            DateOnly targetDate,
            CancellationToken cancellationToken = default);
    }
}
