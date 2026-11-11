using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public interface IOvertimeApprovalWorkflowService
    {
        Task<PagedResult<OvertimeApprovalInboxItemDto>> GetInboxAsync(
            Guid approverUserId,
            string? status = null,
            Guid? unitId = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        Task<OvertimePlanningServiceResult<OvertimeApprovalResultDto>> ApprovePlanAsync(
            Guid planId,
            ApproveOvertimeCommand command,
            Guid approverUserId,
            CancellationToken cancellationToken = default);

        Task<OvertimePlanningServiceResult<OvertimeApprovalResultDto>> RejectPlanAsync(
            Guid planId,
            RejectOvertimeCommand command,
            Guid approverUserId,
            CancellationToken cancellationToken = default);

        Task<PagedResult<MedicalDirectorOvertimeEscalationDto>> GetMedicalDirectorInboxAsync(
            string? urgency = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        Task<OvertimePlanningServiceResult<OvertimeApprovalResultDto>> ApproveMedicalDirectorEscalationAsync(
            Guid realizationId,
            ApproveMedicalDirectorEscalationCommand command,
            Guid directorUserId,
            CancellationToken cancellationToken = default);

        Task<OvertimePlanningServiceResult<OvertimeApprovalResultDto>> RejectMedicalDirectorEscalationAsync(
            Guid realizationId,
            string rejectionReason,
            Guid directorUserId,
            CancellationToken cancellationToken = default);
    }
}
