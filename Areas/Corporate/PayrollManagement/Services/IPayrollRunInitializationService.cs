using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    public interface IPayrollRunInitializationService
    {
        Task<PayrollServiceResult<PayrollRunOperationResultDto>> InitializePayrollRunAsync(
            CreatePayrollRunCommand command,
            Guid actorUserId,
            CancellationToken cancellationToken = default);

        Task<PayrollRunDetailDto?> GetPayrollRunDetailAsync(
            Guid runId,
            CancellationToken cancellationToken = default);

        Task<List<PayrollRunSummaryDto>> GetAllPayrollRunsAsync(
            CancellationToken cancellationToken = default);
    }
}
