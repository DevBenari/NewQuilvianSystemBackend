using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Services
{
    public interface IJobVacancyService
    {
        Task<RecruitmentResult<PagedResult<JobVacancyListItemResponse>>> GetPagedAsync(
            JobVacancyQuery query,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<JobVacancyResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<JobVacancyResponse>> CreateAsync(
            CreateJobVacancyRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<JobVacancyResponse>> UpdateAsync(
            Guid id,
            UpdateJobVacancyRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<JobVacancyResponse>> PublishAsync(
            Guid id,
            PublishJobVacancyRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<JobVacancyResponse>> CloseAsync(
            Guid id,
            CloseJobVacancyRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);
    }
}
