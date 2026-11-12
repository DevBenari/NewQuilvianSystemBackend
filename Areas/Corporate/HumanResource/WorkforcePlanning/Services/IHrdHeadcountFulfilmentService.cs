using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    public interface IHrdHeadcountFulfilmentService
    {
        Task<WorkforcePlanningResult<FulfilmentLinkResponse>> CreateFulfilmentLinkAsync(
            Guid headcountRequestId,
            Guid seatId,
            CreateFulfilmentLinkRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<WorkforcePlanningResult<FulfilmentLinkResponse>> RemoveFulfilmentLinkAsync(
            Guid headcountRequestId,
            Guid seatId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<int> StepProcessFulfilmentAndOverdueAsync(
            DateOnly operationalDate,
            SchedulerExecutionSummary summary,
            CancellationToken cancellationToken = default);
    }
}
