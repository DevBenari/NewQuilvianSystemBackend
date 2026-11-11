using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Services
{
    public interface IClinicalOsceAssessmentService
    {
        Task<ClinicalOsceResultDto> SubmitClinicalOsceAsync(Guid sessionId, SubmitClinicalOsceRequestDto request, string currentUserId);
        Task<ClinicalOsceResultDto?> GetParticipantOsceAsync(Guid sessionId, Guid workforceProfileId);
        Task<List<ClinicalOsceResultDto>> GetSessionOscesAsync(Guid sessionId);
    }
}
