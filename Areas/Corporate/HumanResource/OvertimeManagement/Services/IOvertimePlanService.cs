using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public interface IOvertimePlanService
    {
        Task<PagedResult<OvertimePlanSummaryDto>> GetMyPlansAsync(
            Guid workforceProfileId,
            string? status = null,
            int? year = null,
            int? month = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        Task<OvertimePlanDetailDto?> GetPlanByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<OvertimePlanningServiceResult<OvertimePlanResultDto>> CreatePlanAsync(
            CreateOvertimePlanCommand command,
            Guid actorUserId,
            CancellationToken cancellationToken = default);

        Task<OvertimePlanningServiceResult<OvertimePlanResultDto>> CancelPlanAsync(
            Guid id,
            CancelOvertimePlanCommand command,
            Guid actorUserId,
            CancellationToken cancellationToken = default);
    }
}
