using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Services
{
    public interface IEmployeeGrievanceService
    {
        Task<GrievanceTicketResponseDto> SubmitGrievanceAsync(SubmitGrievanceRequestDto request, string actorEmail, CancellationToken cancellationToken = default);
        Task<PagedResult<GrievanceTicketSummaryDto>> GetMyTicketsAsync(Guid employeeId, string? status, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<GrievanceTicketResponseDto> RecordMediationResolutionAsync(Guid grievanceId, RecordMediationResolutionRequestDto request, string actorEmail, CancellationToken cancellationToken = default);
    }
}
