using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    /// <summary>
    /// Engine validasi hard-cap 50% Gross Pay (DED-B1) dan alokasi prioritas 1-4, pemangkasan parsial & carry-over (DED-B2, DED-B3, PP 36/2021) (BE-HRD-164, BE-HRD-165).
    /// </summary>
    public interface IPayrollDeductionEngineService
    {
        decimal CalculateSalaryCapLimit(decimal grossPay, decimal maxDeductionPercentage = 50.0m);
        Task<DeductionSimulationResultDto> SimulateEmployeeDeductionsAsync(Guid employeeId, decimal grossPay, CancellationToken cancellationToken = default);
        Task<DeductionSimulationResultDto> ExecutePayrollDeductionsAsync(Guid payrollRunId, Guid employeeId, decimal grossPay, Guid userId, CancellationToken cancellationToken = default);
        Task<List<DeductionSimulationResultDto>> ExecuteBatchPayrollDeductionsAsync(Guid payrollRunId, Guid userId, CancellationToken cancellationToken = default);
    }
}
