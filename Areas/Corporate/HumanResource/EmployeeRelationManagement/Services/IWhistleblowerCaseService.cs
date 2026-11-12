using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Services
{
    public interface IWhistleblowerCaseService
    {
        Task<WhistleblowerTicketReceiptDto> SubmitWhistleblowerCaseAsync(SubmitWhistleblowerRequestDto request, string actorEmail, CancellationToken cancellationToken = default);
        Task<PagedResult<WhistleblowerCaseDto>> GetComplianceInboxAsync(string? category, string? severityLevel, string? status, int page, int pageSize, CancellationToken cancellationToken = default);
    }
}
