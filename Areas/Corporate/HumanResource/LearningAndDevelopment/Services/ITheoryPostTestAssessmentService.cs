using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Services
{
    public interface ITheoryPostTestAssessmentService
    {
        Task<TheoryPostTestResultDto> SubmitPostTestAsync(Guid sessionId, SubmitTheoryPostTestRequestDto request, string currentUserId);
        Task<ParticipantAssessmentSummaryDto> GetParticipantAssessmentSummaryAsync(Guid sessionId, Guid workforceProfileId);
        Task<List<TheoryPostTestResultDto>> GetSessionPostTestsAsync(Guid sessionId);
    }
}
