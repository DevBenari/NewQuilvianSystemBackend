using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Services
{
    public interface IJobOfferApprovalService
    {
        Task<RecruitmentResult<JobOfferApprovalHistoryResponse>> GetApprovalsByOfferIdAsync(
            Guid offerId,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<JobOfferApprovalResponse>> ApproveOfferStepAsync(
            Guid offerId,
            ApproveJobOfferRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<JobOfferApprovalResponse>> RejectOfferStepAsync(
            Guid offerId,
            RejectJobOfferRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);
    }
}
