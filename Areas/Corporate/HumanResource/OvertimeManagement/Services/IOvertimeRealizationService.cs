using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public interface IOvertimeRealizationService
    {
        Task<OvertimeRealizationServiceResult<OvertimeRealizationResultDto>> SubmitRealizationAsync(
            SubmitOvertimeRealizationCommand command,
            Guid actorUserId,
            CancellationToken cancellationToken = default);

        Task<OvertimeRealizationDetailDto?> GetRealizationByIdAsync(
            Guid realizationId,
            CancellationToken cancellationToken = default);

        Task<PagedResult<OvertimeRealizationSummaryDto>> GetMyRealizationsAsync(
            Guid workforceProfileId,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        Task<OvertimeRealizationServiceResult<bool>> CancelRealizationAsync(
            Guid realizationId,
            CancelOvertimeRealizationCommand command,
            Guid actorUserId,
            CancellationToken cancellationToken = default);
    }
}
