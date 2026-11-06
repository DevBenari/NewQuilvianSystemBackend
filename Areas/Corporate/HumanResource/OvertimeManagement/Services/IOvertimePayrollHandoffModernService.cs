using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public interface IOvertimePayrollHandoffModernService
    {
        Task<OvertimePayrollHandoffPackageDto> PreviewHandoffPackageAsync(
            Guid periodId,
            CancellationToken cancellationToken = default);

        Task<OvertimePlanningServiceResult<OvertimePayrollHandoffPackageDto>> ExecuteHandoffAsync(
            Guid periodId,
            ExecutePayrollHandoffCommand command,
            Guid actorUserId,
            CancellationToken cancellationToken = default);
    }
}
