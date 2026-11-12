using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Services
{
    public interface IJobRequisitionService
    {
        Task<RecruitmentResult<PagedResult<JobRequisitionListItemResponse>>> GetPagedAsync(
            JobRequisitionQuery query,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<JobRequisitionResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<JobRequisitionResponse>> CreateAsync(
            CreateJobRequisitionRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<JobRequisitionResponse>> UpdateAsync(
            Guid id,
            UpdateJobRequisitionRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<JobRequisitionResponse>> SubmitAsync(
            Guid id,
            SubmitJobRequisitionRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<JobRequisitionResponse>> CancelAsync(
            Guid id,
            CancelJobRequisitionRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);
    }
}
