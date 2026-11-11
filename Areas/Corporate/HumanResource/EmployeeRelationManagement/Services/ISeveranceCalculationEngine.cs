using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Services
{
    public interface ISeveranceCalculationEngine
    {
        Task<SeveranceCalculationResponseDto> CalculateSeveranceAsync(CalculateSeveranceRequestDto request, string actorEmail, CancellationToken cancellationToken = default);
        Task<SeveranceCalculationDetailDto?> GetSeveranceBreakdownAsync(Guid separationId, CancellationToken cancellationToken = default);
        Task<SeveranceCalculationResponseDto> ApproveSeveranceAsync(Guid separationId, ApproveSeveranceRequestDto request, string actorEmail, CancellationToken cancellationToken = default);
    }
}
