using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public interface IOvertimePeriodManagementService
    {
        Task<OvertimePeriodDto> GetOrCreateCurrentPeriodAsync(CancellationToken cancellationToken = default);
        Task<List<OvertimePeriodDto>> GetAllPeriodsAsync(CancellationToken cancellationToken = default);
        Task<OvertimePlanningServiceResult<OvertimePeriodOperationResultDto>> ClosePeriodAsync(
            Guid periodId,
            CloseOvertimePeriodCommand command,
            Guid actorUserId,
            CancellationToken cancellationToken = default);
        Task<OvertimePlanningServiceResult<OvertimePeriodOperationResultDto>> ReopenPeriodAsync(
            Guid periodId,
            ReopenOvertimePeriodCommand command,
            Guid actorUserId,
            CancellationToken cancellationToken = default);
    }
}
