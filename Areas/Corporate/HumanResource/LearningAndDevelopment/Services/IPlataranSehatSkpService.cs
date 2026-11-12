using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Services
{
    public interface IPlataranSehatSkpService
    {
        Task<PlataranSehatSkpRecordDto> RecordSkpActivityAsync(RecordSkpActivityRequestDto request, string currentUserId);
        Task<SkpPortfolioSummaryDto> GetStaffSkpPortfolioAsync(Guid workforceProfileId);
        Task<SyncOppeResponseDto> SyncStaffSkpToOppeAsync(Guid workforceProfileId, string currentUserId);
        Task<PlataranSehatSkpRecordDto> VerifyPlataranSehatAsync(Guid skpRecordId, string currentUserId);
    }
}
