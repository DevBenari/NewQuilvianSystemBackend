using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    public interface IPayrollBpjsCalculationService
    {
        Task<PayrollServiceResult<CalculateBpjsResultDto>> CalculateBpjsContributionsAsync(
            Guid runId,
            Guid actorUserId,
            CancellationToken cancellationToken = default);

        Task<PayrollServiceResult<EmployeeBpjsCalculationDto>> CalculateEmployeeBpjsAsync(
            Guid runId,
            Guid employeeRunId,
            Guid actorUserId,
            CancellationToken cancellationToken = default);
    }
}
