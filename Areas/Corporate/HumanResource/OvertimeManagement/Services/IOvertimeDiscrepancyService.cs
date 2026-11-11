using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public interface IOvertimeDiscrepancyService
    {
        Task<PagedResult<OvertimeDiscrepancyQueueItemDto>> GetDiscrepancyQueueAsync(
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        Task<OvertimePlanningServiceResult<ResolveOvertimeDiscrepancyResultDto>> ResolveDiscrepancyAsync(
            Guid verificationId,
            ResolveOvertimeDiscrepancyCommand command,
            Guid hrAdminUserId,
            CancellationToken cancellationToken = default);
    }
}
