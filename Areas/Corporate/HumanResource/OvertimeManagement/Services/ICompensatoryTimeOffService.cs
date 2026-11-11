using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public interface ICompensatoryTimeOffService
    {
        Task<OvertimePlanningServiceResult<GenerateCompensatoryTimeOffResultDto>> GenerateCompensatoryTimeOffAsync(
            Guid realizationId,
            Guid hrAdminUserId,
            CancellationToken cancellationToken = default);

        Task<List<CompensatoryTimeOffTicketDto>> GetStaffTicketsAsync(
            Guid workforceProfileId,
            CancellationToken cancellationToken = default);
    }
}
