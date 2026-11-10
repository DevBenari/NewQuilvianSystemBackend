using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    public interface IPayrollEarningsCalculationService
    {
        Task<PayrollServiceResult<PayrollEarningsCalculationResultDto>> CalculateFixedEarningsAsync(
            Guid runId,
            Guid actorUserId,
            CancellationToken cancellationToken = default);

        Task<PayrollServiceResult<PayrollEmployeeEarningsResultDto>> CalculateEmployeeFixedEarningsAsync(
            Guid runId,
            Guid employeeRunId,
            Guid actorUserId,
            CancellationToken cancellationToken = default);
    }
}
