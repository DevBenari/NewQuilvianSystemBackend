using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public interface IMedicalDirectorOvertimeEscalationService
    {
        Task<PagedResult<MedicalDirectorOvertimeEscalationDto>> GetEscalationQueueAsync(
            string? urgency = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        Task<OvertimePlanningServiceResult<OvertimeApprovalResultDto>> ApproveEscalationAsync(
            Guid realizationId,
            ApproveMedicalDirectorEscalationCommand command,
            Guid directorUserId,
            CancellationToken cancellationToken = default);

        Task<OvertimePlanningServiceResult<OvertimeApprovalResultDto>> RejectEscalationAsync(
            Guid realizationId,
            string rejectionReason,
            Guid directorUserId,
            CancellationToken cancellationToken = default);
    }
}
