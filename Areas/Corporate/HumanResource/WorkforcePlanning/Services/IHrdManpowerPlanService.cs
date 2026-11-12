using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    public interface IHrdManpowerPlanService
    {
        Task<WorkforcePlanningResult<PagedResult<ManpowerPlanListItemResponse>>> GetPagedListAsync(
            ManpowerPlanPagedQuery query,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<WorkforcePlanningResult<ManpowerPlanResponse>> GetByIdAsync(
            Guid id,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<WorkforcePlanningResult<ManpowerPlanResponse>> CreateAnnualPlanAsync(
            CreateAnnualPlanRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<WorkforcePlanningResult<ManpowerPlanResponse>> CreatePlanAddendumAsync(
            CreatePlanAddendumRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<WorkforcePlanningResult<ManpowerPlanResponse>> UpdateDraftAsync(
            Guid id,
            UpdateManpowerPlanRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<WorkforcePlanningResult<ManpowerPlanResponse>> RecalculateAsync(
            Guid id,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<WorkforcePlanningResult<ManpowerPlanResponse>> SubmitAsync(
            Guid id,
            SubmitManpowerPlanRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<WorkforcePlanningResult<ManpowerPlanResponse>> FinanceAdjustmentAsync(
            Guid id,
            FinancePlanAdjustmentRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<WorkforcePlanningResult<List<StaffingGapSnapshotListItemResponse>>> GetGapSnapshotsAsync(
            Guid id,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        ManpowerPlanFilterMetadataResponse GetFilterMetadata();
    }
}
