using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Services
{
    public interface IEmployeeGrievanceService
    {
        /// <summary>Pelapor selalu pengguna yang login (<paramref name="complainant"/>); nilai pelapor pada body diabaikan (remediasi kelompok G).</summary>
        Task<GrievanceTicketResponseDto> SubmitGrievanceAsync(SubmitGrievanceRequestDto request, string actorEmail, GrievanceComplainantIdentity complainant, CancellationToken cancellationToken = default);
        Task<PagedResult<GrievanceTicketSummaryDto>> GetMyTicketsAsync(GrievanceComplainantIdentity complainant, string? status, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<GrievanceTicketResponseDto> RecordMediationResolutionAsync(Guid grievanceId, RecordMediationResolutionRequestDto request, string actorEmail, CancellationToken cancellationToken = default);
    }
}
