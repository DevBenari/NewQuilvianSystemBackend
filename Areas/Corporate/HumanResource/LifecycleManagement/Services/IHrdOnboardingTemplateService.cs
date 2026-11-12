using System;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public interface IHrdOnboardingTemplateService
    {
        Task<RecruitmentResult<PagedResult<OnboardingTemplateListItemResponse>>> GetPagedAsync(OnboardingTemplateQuery query, CancellationToken cancellationToken = default);
        Task<RecruitmentResult<OnboardingTemplateResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<RecruitmentResult<OnboardingTemplateResponse>> CreateAsync(CreateOnboardingTemplateRequest request, Guid userId, CancellationToken cancellationToken = default);
        Task<RecruitmentResult<OnboardingTemplateResponse>> UpdateAsync(Guid id, UpdateOnboardingTemplateRequest request, Guid userId, CancellationToken cancellationToken = default);
        Task<RecruitmentResult<bool>> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    }
}
