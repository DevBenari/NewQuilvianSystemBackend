using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Services
{
    public interface IJobOfferService
    {
        Task<RecruitmentResult<PagedResult<JobOfferListItemResponse>>> GetPagedAsync(
            JobOfferQuery query,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<JobOfferResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<JobOfferResponse>> CreateAsync(
            CreateJobOfferRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<JobOfferResponse>> UpdateAsync(
            Guid id,
            UpdateJobOfferRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<JobOfferResponse>> SendConditionalOfferAsync(
            Guid id,
            SendConditionalOfferRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<JobOfferResponse>> CandidateRespondAsync(
            Guid id,
            CandidateOfferResponseRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);
    }
}
