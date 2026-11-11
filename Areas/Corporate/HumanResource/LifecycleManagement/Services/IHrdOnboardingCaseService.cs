using System;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public interface IHrdOnboardingCaseService
    {
        Task<RecruitmentResult<OnboardingCaseDetailResponse>> CreateFromHandoffAsync(
            Guid handoffId,
            Guid actorUserId,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<PagedResult<OnboardingCaseListItemResponse>>> GetPagedAsync(
            OnboardingCaseQuery query,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<OnboardingCaseSummaryResponse>> GetSummaryAsync(
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<OnboardingCaseDetailResponse>> GetByIdAsync(
            Guid id,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<OnboardingCaseDetailResponse>> RecordFirstDayAttendanceAsync(
            Guid caseId,
            RecordFirstDayAttendanceRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<OnboardingCaseDetailResponse>> CancelFirstDayAttendanceAsync(
            Guid caseId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<OnboardingCaseDetailResponse>> CompleteItemAsync(
            Guid caseId,
            Guid itemId,
            CompleteChecklistItemRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<OnboardingCaseDetailResponse>> UncompleteItemAsync(
            Guid caseId,
            Guid itemId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<OnboardingCaseDetailResponse>> CheckAutomaticItemsAsync(
            Guid caseId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<OnboardingCaseDetailResponse>> ReassignCoordinatorAsync(
            Guid caseId,
            ReassignCoordinatorRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<OnboardingCaseDetailResponse>> DeclareReadyForWorkAsync(
            Guid caseId,
            DeclareReadyForWorkRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<OnboardingCaseDetailResponse>> CancelJoinAsync(
            Guid caseId,
            CancelOnboardingJoinRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<(bool Exists, Stream? Stream, string? ContentType, string? FileName)> DownloadItemEvidenceAsync(
            Guid caseId,
            Guid itemId,
            CancellationToken cancellationToken = default);
    }
}
