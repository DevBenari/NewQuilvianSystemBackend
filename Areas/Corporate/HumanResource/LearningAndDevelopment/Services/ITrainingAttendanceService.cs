using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Services
{
    public interface ITrainingAttendanceService
    {
        Task<TrainingAttendanceDto> RecordCheckInAsync(Guid sessionId, RecordAttendanceCheckInRequestDto request, string currentUserId);
        Task<TrainingAttendanceDto> RecordCheckOutAsync(Guid sessionId, RecordAttendanceCheckOutRequestDto request, string currentUserId);
        Task<List<TrainingAttendanceDto>> GetSessionAttendancesAsync(Guid sessionId);
        Task<ParticipantAttendanceEligibilityDto> EvaluateEligibilityAsync(Guid sessionId, Guid workforceProfileId);
        Task<List<ParticipantAttendanceEligibilityDto>> EvaluateSessionAllEligibilitiesAsync(Guid sessionId);
    }
}
