using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public interface IHrdLifecycleWorklistService
    {
        Task<RecruitmentResult<PagedResult<LifecycleWorklistItemResponse>>> GetWorklistAsync(
            LifecycleWorklistQuery query,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<LifecycleWorklistCountResponse>> GetCountsAsync(
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);
    }
}
