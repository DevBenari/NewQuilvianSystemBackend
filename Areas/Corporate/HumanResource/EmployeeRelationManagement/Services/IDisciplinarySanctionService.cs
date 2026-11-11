using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Services
{
    public interface IDisciplinarySanctionService
    {
        Task<SanctionResponseDto> IssueSanctionAsync(IssueSanctionRequestDto request, string actorEmail, CancellationToken cancellationToken = default);
        Task<List<SanctionResponseDto>> GetSanctionsByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default);
        Task<SanctionAppealResponseDto> AppealSanctionAsync(Guid sanctionId, AppealSanctionRequestDto request, string actorEmail, CancellationToken cancellationToken = default);
    }
}
