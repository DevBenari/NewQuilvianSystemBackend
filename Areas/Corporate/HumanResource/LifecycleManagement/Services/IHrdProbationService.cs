using System;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public interface IHrdProbationService
    {
        Task<HrdProbation?> CreateForContractAsync(WfpContractHistory contract, Guid hospitalSiteId, Guid userId, CancellationToken cancellationToken = default);
        Task<RecruitmentResult<PagedResult<HrdProbationResponse>>> GetPagedAsync(HrdProbationQuery query, CancellationToken cancellationToken = default);
        Task<RecruitmentResult<HrdProbationResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        // BE-HRD-079: Penilaian masa percobaan T15
        Task<RecruitmentResult<ProbationAssessmentResponse>> SubmitAssessmentAsync(Guid probationId, SubmitProbationAssessmentRequest request, ClaimsPrincipal user, CancellationToken cancellationToken = default);
        Task<RecruitmentResult<ProbationAssessmentResponse>> UpdateAssessmentAsync(Guid probationId, Guid assessmentId, UpdateProbationAssessmentRequest request, ClaimsPrincipal user, CancellationToken cancellationToken = default);
        Task<RecruitmentResult<ProbationAssessmentResponse>> WithdrawAssessmentAsync(Guid probationId, Guid assessmentId, WithdrawAssessmentRequest request, ClaimsPrincipal user, CancellationToken cancellationToken = default);
        Task<(bool Exists, Stream? Stream, string? ContentType, string? FileName)> DownloadEvidenceAsync(Guid probationId, Guid assessmentId, CancellationToken cancellationToken = default);
        Task<RecruitmentResult<bool>> ApplyAssessmentDecisionAsync(Guid assessmentId, Guid deciderUserId, bool isApproved, string? decisionNote, CancellationToken cancellationToken = default);
    }
}
