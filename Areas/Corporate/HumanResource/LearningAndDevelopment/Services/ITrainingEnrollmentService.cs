using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Services
{
    public interface ITrainingEnrollmentService
    {
        Task<List<TrainingEnrollmentDto>> GetEnrollmentsAsync(Guid? sessionId, Guid? workforceProfileId, string? status, CancellationToken cancellationToken = default);
        Task<TrainingEnrollmentDto?> GetEnrollmentByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<TrainingEnrollmentDto> SubmitEssEnrollmentAsync(SubmitEnrollmentRequestDto request, string submittedBy, CancellationToken cancellationToken = default);
        Task<TrainingEnrollmentDto> ProcessApprovalWorkflowAsync(Guid id, ProcessEnrollmentApprovalDto request, string reviewerUser, CancellationToken cancellationToken = default);
        Task<bool> DeleteEnrollmentAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default);
    }
}
