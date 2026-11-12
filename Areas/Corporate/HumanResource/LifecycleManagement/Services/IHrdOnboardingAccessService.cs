using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public interface IHrdOnboardingAccessService
    {
        Task<RecruitmentResult<PagedResult<OnboardingAccessListItemResponse>>> GetPagedAsync(
            OnboardingAccessQuery query,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<OnboardingAccessListItemResponse>> GetByIdAsync(
            Guid id,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<OnboardingAccessListItemResponse>> MarkCompletedAsync(
            Guid id,
            CompleteAccessRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<OnboardingAccessListItemResponse>> RetryAccountCreationAsync(
            Guid id,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);
    }
}
