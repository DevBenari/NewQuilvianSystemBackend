using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Services
{
    public interface ICandidateEvaluationService
    {
        Task<RecruitmentResult<PagedResult<CandidateEvaluationListItemResponse>>> GetPagedAsync(
            CandidateEvaluationQuery query,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<CandidateEvaluationResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<CandidateEvaluationResponse>> ScheduleAsync(
            ScheduleEvaluationRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<CandidateEvaluationResponse>> SubmitHrEvaluationAsync(
            Guid evaluationId,
            SubmitHrEvaluationRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<CandidateEvaluationResponse>> SubmitClinicalEvaluationAsync(
            Guid evaluationId,
            SubmitClinicalEvaluationRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);
    }
}
