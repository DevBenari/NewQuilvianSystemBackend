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
        /// <summary>
        /// Daftar Tes/Wawancara. Tanpa <paramref name="canViewAll"/> (<c>CandidateEvaluation : ViewAll</c>) hasil dibatasi
        /// pada evaluasi yang <c>EvaluatorUserId</c>-nya pengguna yang login (<c>VR-PKB-07</c>).
        /// </summary>
        Task<RecruitmentResult<PagedResult<CandidateEvaluationListItemResponse>>> GetPagedAsync(
            CandidateEvaluationQuery query,
            ClaimsPrincipal user,
            bool canViewAll,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<PagedResult<EvaluationEligibleApplicationResponse>>> GetEligibleApplicationsAsync(
            EvaluationEligibleQuery query,
            ClaimsPrincipal user,
            bool canViewAll,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Detail evaluasi. Tanpa <paramref name="canViewAll"/>, evaluasi milik penilai lain ditolak <c>403</c> (<c>VR-PKB-07</c>).
        /// </summary>
        Task<RecruitmentResult<CandidateEvaluationResponse>> GetByIdAsync(
            Guid id,
            ClaimsPrincipal user,
            bool canViewAll,
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
