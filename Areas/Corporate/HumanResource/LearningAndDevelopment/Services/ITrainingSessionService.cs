using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Services
{
    public interface ITrainingSessionService
    {
        Task<List<TrainingSessionDto>> GetSessionsAsync(Guid? trainingPlanId, string? status, DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default);
        Task<TrainingSessionDto?> GetSessionByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<TrainingSessionDto> CreateSessionAsync(CreateTrainingSessionRequestDto request, string createdBy, CancellationToken cancellationToken = default);
        Task<TrainingSessionDto> UpdateSessionAsync(Guid id, UpdateTrainingSessionRequestDto request, string updatedBy, CancellationToken cancellationToken = default);
        Task<TrainingSessionDto> OpenRegistrationAsync(Guid id, string updatedBy, CancellationToken cancellationToken = default);
        Task<TrainingSessionDto> CloseRegistrationAsync(Guid id, string updatedBy, CancellationToken cancellationToken = default);
        Task<TrainingSessionDto> CompleteSessionAsync(Guid id, string updatedBy, CancellationToken cancellationToken = default);
        Task<bool> DeleteSessionAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default);
    }
}
