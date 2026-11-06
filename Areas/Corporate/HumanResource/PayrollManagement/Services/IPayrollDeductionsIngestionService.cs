using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    public interface IPayrollDeductionsIngestionService
    {
        Task<PayrollServiceResult<IngestDeductionsResultDto>> IngestDeductionsAsync(
            Guid runId,
            Guid actorUserId,
            CancellationToken cancellationToken = default);

        Task<PayrollServiceResult<EmployeeDeductionSummaryDto>> IngestEmployeeDeductionsAsync(
            Guid runId,
            Guid employeeRunId,
            Guid actorUserId,
            CancellationToken cancellationToken = default);
    }
}
