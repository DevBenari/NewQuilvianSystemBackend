using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Services
{
    public interface IBipartiteForumService
    {
        Task<BipartiteForumResponseDto> ScheduleMeetingAsync(ScheduleBipartiteMeetingRequestDto request, string actorEmail, CancellationToken cancellationToken = default);
        Task<PagedResult<BipartiteForumResponseDto>> GetBipartiteForumsAsync(string? disputeType, string? status, int? year, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<BipartiteForumResponseDto> RecordMinutesAsync(Guid forumId, RecordMinutesRequestDto request, string actorEmail, CancellationToken cancellationToken = default);
        Task<BipartiteForumResponseDto> UploadJointAgreementAsync(Guid forumId, UploadJointAgreementRequestDto request, string actorEmail, CancellationToken cancellationToken = default);
    }
}
