using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Services
{
    public interface ICandidateService
    {
        Task<RecruitmentResult<PagedResult<CandidateListItemResponse>>> GetPagedAsync(
            CandidateQuery query,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<CandidateResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<CandidateResponse>> RegisterAsync(
            RegisterCandidateRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<CandidateResponse>> UpdateAsync(
            Guid id,
            UpdateCandidateRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<CandidateApplicationResponse>> ApplyVacancyAsync(
            Guid candidateId,
            ApplyVacancyRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<CandidateDocumentResponse>> UploadDocumentAsync(
            Guid candidateId,
            UploadCandidateDocumentRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);
    }
}
