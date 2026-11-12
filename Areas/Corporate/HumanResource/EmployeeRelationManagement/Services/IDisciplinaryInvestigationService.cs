using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Services
{
    public interface IDisciplinaryInvestigationService
    {
        Task<InvestigationRecordResponseDto> InitiateInvestigationAsync(InitiateInvestigationRequestDto request, string actorEmail, CancellationToken cancellationToken = default);
        Task<InvestigationRecordResponseDto> RecordFindingsAsync(Guid investigationId, RecordBapFindingsRequestDto request, string actorEmail, CancellationToken cancellationToken = default);
        Task<InvestigationRecordResponseDto?> GetInvestigationByIdAsync(Guid investigationId, CancellationToken cancellationToken = default);
    }
}
