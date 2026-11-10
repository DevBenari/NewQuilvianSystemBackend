using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    public interface IPayrollVarianceResolutionService
    {
        Task<PayrollServiceResult<PayrollVarianceAlertDto>> ResolveAlertAsync(
            Guid alertId,
            ResolveVarianceAlertCommand command,
            Guid actorUserId,
            CancellationToken cancellationToken = default);

        Task<PayrollServiceResult<BatchVarianceSubmissionCheckDto>> CheckBatchCanBeSubmittedAsync(
            Guid runId,
            CancellationToken cancellationToken = default);
    }
}
