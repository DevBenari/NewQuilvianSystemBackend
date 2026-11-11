using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Services
{
    public interface ICandidateScreeningService
    {
        Task<RecruitmentResult<PagedResult<CandidateScreeningListItemResponse>>> GetPagedAsync(
            CandidateScreeningQuery query,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<CandidateScreeningResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<CandidateScreeningResponse>> InitiateScreeningAsync(
            InitiateScreeningRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<CandidateScreeningResponse>> EvaluateScreeningAsync(
            EvaluateScreeningRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<List<CandidateDocumentResponse>>> GetCandidateDocumentsAsync(
            Guid candidateId,
            CancellationToken cancellationToken = default);
    }
}
