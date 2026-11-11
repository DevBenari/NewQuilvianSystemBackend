using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    public interface IPayrollOvertimeAndShiftIngestionService
    {
        Task<PayrollServiceResult<IngestOvertimeAndShiftResultDto>> IngestOvertimeAndShiftAsync(
            Guid runId,
            Guid actorUserId,
            CancellationToken cancellationToken = default);

        Task<PayrollServiceResult<EmployeeVariableEarningSummaryDto>> IngestEmployeeOvertimeAndShiftAsync(
            Guid runId,
            Guid employeeRunId,
            Guid actorUserId,
            CancellationToken cancellationToken = default);
    }
}
