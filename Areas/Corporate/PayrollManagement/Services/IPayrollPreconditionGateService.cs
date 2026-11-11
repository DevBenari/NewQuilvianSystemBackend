using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    public interface IPayrollPreconditionGateService
    {
        Task<PreconditionGateCheckResponse> CheckPreconditionsAsync(
            int periodYear,
            int periodMonth,
            CancellationToken cancellationToken = default);
    }
}
