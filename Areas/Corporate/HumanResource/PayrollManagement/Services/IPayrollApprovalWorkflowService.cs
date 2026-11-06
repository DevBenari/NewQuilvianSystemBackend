using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    public interface IPayrollApprovalWorkflowService
    {
        Task<PayrollServiceResult<PayrollApprovalResultDto>> SubmitRunForReviewAsync(
            Guid runId,
            SubmitPayrollRunCommand command,
            Guid actorUserId,
            CancellationToken cancellationToken = default);

        Task<PayrollServiceResult<PayrollApprovalResultDto>> ReviewRunAsync(
            Guid runId,
            ReviewPayrollRunCommand command,
            Guid actorUserId,
            CancellationToken cancellationToken = default);

        Task<PayrollServiceResult<PayrollApprovalResultDto>> ApproveRunFinalAsync(
            Guid runId,
            ApprovePayrollRunCommand command,
            Guid actorUserId,
            CancellationToken cancellationToken = default);

        Task<PayrollServiceResult<PayrollApprovalResultDto>> RejectRunAsync(
            Guid runId,
            RejectPayrollRunCommand command,
            Guid actorUserId,
            CancellationToken cancellationToken = default);

        Task<PayrollServiceResult<List<PayrollApprovalHistoryDto>>> GetApprovalHistoryAsync(
            Guid runId,
            CancellationToken cancellationToken = default);
    }
}
