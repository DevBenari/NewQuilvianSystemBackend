using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    public interface IPayrollTaxCalculationService
    {
        Task<PayrollServiceResult<CalculatePph21ResultDto>> CalculatePph21TerAsync(
            Guid runId,
            Guid actorUserId,
            CancellationToken cancellationToken = default);

        Task<PayrollServiceResult<EmployeePph21CalculationDto>> CalculateEmployeePph21TerAsync(
            Guid runId,
            Guid employeeRunId,
            Guid actorUserId,
            CancellationToken cancellationToken = default);
    }
}
