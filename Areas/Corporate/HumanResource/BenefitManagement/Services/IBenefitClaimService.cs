using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services
{
    public interface IBenefitClaimService
    {
        Task<PagedResult<BenefitClaimSummaryDto>> GetClaimsPagedAsync(BenefitClaimListQuery query, CancellationToken cancellationToken);
        Task<List<BenefitClaimSummaryDto>> GetMyClaimsAsync(Guid workforceProfileId, CancellationToken cancellationToken);
        Task<BenefitClaimDetailDto> GetClaimDetailAsync(Guid id, CancellationToken cancellationToken);
        Task<BenefitClaimDetailDto> CreateEssClaimAsync(Guid workforceProfileId, CreateEssClaimRequestDto request, Guid actorUserId, CancellationToken cancellationToken);
        Task<ClaimDocumentDetailDto> AddClaimDocumentAsync(Guid claimId, ClaimDocumentUploadRequestDto request, Guid actorUserId, CancellationToken cancellationToken);
        Task<BenefitClaimDetailDto> VerifyClaimAsync(Guid claimId, VerifyClaimRequestDto request, Guid actorUserId, CancellationToken cancellationToken);
        Task<BenefitClaimDetailDto> SettleClaimAsync(Guid claimId, SettleClaimRequestDto request, Guid actorUserId, CancellationToken cancellationToken);
        Task<bool> TerminateBenefitEnrollmentAsync(Guid enrollmentId, TerminateBenefitRequestDto request, Guid actorUserId, CancellationToken cancellationToken);
        Task<ExitClearanceMedicalReceivableDto> CheckExitClearanceMedicalReceivableAsync(Guid workforceProfileId, CancellationToken cancellationToken);
    }
}
