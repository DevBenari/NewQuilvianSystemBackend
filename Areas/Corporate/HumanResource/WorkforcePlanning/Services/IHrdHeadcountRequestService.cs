using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    /// <summary>
    /// Antarmuka layanan pengelolaan permintaan penambahan tenaga kerja / headcount request (BE-HRD-040).
    /// </summary>
    public interface IHrdHeadcountRequestService
    {
        Task<WorkforcePlanningResult<PagedResult<HeadcountRequestListItemResponse>>> GetPagedListAsync(
            HeadcountRequestPagedQuery query,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<WorkforcePlanningResult<HeadcountRequestResponse>> GetByIdAsync(
            Guid id,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RemainingSeatsResponse> GetRemainingSeatsAsync(
            Guid hospitalSiteId,
            Guid organizationUnitId,
            Guid positionId,
            DateOnly? asOfDate,
            CancellationToken cancellationToken = default);

        Task<WorkforcePlanningResult<HeadcountRequestResponse>> CreateDraftAsync(
            CreateHeadcountRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<WorkforcePlanningResult<HeadcountRequestResponse>> UpdateDraftAsync(
            Guid id,
            UpdateHeadcountRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<WorkforcePlanningResult<HeadcountRequestResponse>> SubmitAsync(
            Guid id,
            SubmitHeadcountRequestDto request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<WorkforcePlanningResult<HeadcountRequestResponse>> WithdrawAsync(
            Guid id,
            WithdrawHeadcountRequestDto request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<WorkforcePlanningResult<HeadcountRequestResponse>> FinanceAdjustmentAsync(
            Guid id,
            FinanceAdjustmentRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<WorkforcePlanningResult<FulfilmentPlanChangeResponse>> ChangeFulfilmentPlanAsync(
            Guid id,
            ChangeFulfilmentPlanRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<WorkforcePlanningResult<CancellationResponse>> RequestCancellationAsync(
            Guid id,
            CreateCancellationRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        HeadcountRequestFilterMetadataResponse GetFilterMetadata();
    }
}
