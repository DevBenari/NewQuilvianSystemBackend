using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services
{
    public interface ISchedulingSettingService
    {
        Task<RecruitmentResult<SchedulingSettingResponse>> GetSettingAsync(Guid hospitalSiteId, CancellationToken cancellationToken = default);
        Task<RecruitmentResult<SchedulingSettingResponse>> UpdateSettingAsync(Guid hospitalSiteId, UpdateSchedulingSettingRequest request, ClaimsPrincipal user, CancellationToken cancellationToken = default);
    }
}
