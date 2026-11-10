using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    public interface IPayrollVarianceGuardService
    {
        Task<PayrollServiceResult<EvaluateVarianceResultDto>> EvaluateRunVariancesAsync(
            Guid runId,
            Guid actorUserId,
            CancellationToken cancellationToken = default);

        Task<PayrollServiceResult<List<PayrollVarianceAlertDto>>> GetRunVarianceAlertsAsync(
            Guid runId,
            CancellationToken cancellationToken = default);
    }
}
