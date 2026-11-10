using System;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public interface IHrdClinicalPreceptorshipService
    {
        Task<RecruitmentResult<PagedResult<ClinicalPreceptorshipListItemResponse>>> GetPagedAsync(
            ClinicalPreceptorshipQuery query,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<ClinicalPreceptorshipResponse>> GetByIdAsync(
            Guid id,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<ClinicalPreceptorshipResponse>> CreateForCaseAsync(
            Guid onboardingCaseId,
            Guid preceptorWorkforceProfileId,
            Guid actorUserId,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<ClinicalPreceptorshipResponse>> ChangePreceptorAsync(
            Guid id,
            ChangePreceptorRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<ClinicalPreceptorshipResponse>> DeclareIndependentAsync(
            Guid id,
            DeclareClinicalIndependentRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<bool> SetAccompaniedAsync(
            Guid onboardingCaseId,
            CancellationToken cancellationToken = default);

        Task<bool> CancelForCaseAsync(
            Guid onboardingCaseId,
            CancellationToken cancellationToken = default);

        Task<(bool Exists, Stream? Stream, string? ContentType, string? FileName)> DownloadEvidenceAsync(
            Guid id,
            CancellationToken cancellationToken = default);
    }
}
