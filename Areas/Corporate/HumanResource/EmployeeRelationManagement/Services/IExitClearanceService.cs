using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Services
{
    public interface IExitClearanceService
    {
        Task<ExitClearanceProcessDetailDto?> GetClearanceBySeparationIdAsync(Guid separationId, CancellationToken cancellationToken = default);
        Task<ExitClearanceChecklistDto> SignoffChecklistAsync(Guid checklistId, SignoffChecklistRequestDto request, string actorEmail, CancellationToken cancellationToken = default);
        Task<PayrollLockStatusResponseDto> GetPayrollLockStatusAsync(Guid employeeId, CancellationToken cancellationToken = default);
        Task<ExitClearanceProcessDetailDto> FinalizeExitClearanceAsync(Guid clearanceProcessId, FinalizeExitClearanceRequestDto request, string actorEmail, CancellationToken cancellationToken = default);
    }
}
