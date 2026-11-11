using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Services
{
    public interface IDisciplinarySuspensionService
    {
        Task<SuspensionResponseDto> IssueSuspensionAsync(IssueSuspensionRequestDto request, string actorEmail, CancellationToken cancellationToken = default);
        Task<SuspensionResponseDto> ReinstateEmployeeAsync(Guid suspensionId, ReinstateEmployeeRequestDto request, string actorEmail, CancellationToken cancellationToken = default);
        Task<SuspensionResponseDto?> GetSuspensionByIdAsync(Guid suspensionId, CancellationToken cancellationToken = default);
    }
}
