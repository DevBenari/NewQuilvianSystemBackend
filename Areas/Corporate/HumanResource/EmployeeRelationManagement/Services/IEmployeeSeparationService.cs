using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Services
{
    public interface IEmployeeSeparationService
    {
        Task<SeparationResponseDto> InitiateSeparationAsync(InitiateSeparationRequestDto request, string actorEmail, CancellationToken cancellationToken = default);
        Task<PagedResult<SeparationResponseDto>> GetSeparationsAsync(string? status, string? separationReason, Guid? departmentId, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<SeparationResponseDto?> GetSeparationByIdAsync(Guid separationId, CancellationToken cancellationToken = default);
    }
}
