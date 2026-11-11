using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Services
{
    public interface IAnnualTrainingPlanService
    {
        Task<List<AnnualTrainingPlanDto>> GetAnnualPlansAsync(int? planYear, string? planStatus, Guid? departmentId, CancellationToken cancellationToken = default);
        Task<AnnualTrainingPlanDto?> GetAnnualPlanByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<AnnualTrainingPlanDto> CreateAnnualPlanAsync(CreateAnnualTrainingPlanRequestDto request, string createdBy, CancellationToken cancellationToken = default);
        Task<AnnualTrainingPlanDto> UpdateAnnualPlanAsync(Guid id, UpdateAnnualTrainingPlanRequestDto request, string updatedBy, CancellationToken cancellationToken = default);
        Task<AnnualTrainingPlanDto> SubmitAnnualPlanAsync(Guid id, SubmitTrainingPlanRequestDto? request, string submittedBy, CancellationToken cancellationToken = default);
        Task<AnnualTrainingPlanDto> ApproveAnnualPlanAsync(Guid id, ApproveTrainingPlanRequestDto request, string approvedBy, CancellationToken cancellationToken = default);
        Task<AnnualTrainingPlanDto> RejectAnnualPlanAsync(Guid id, RejectTrainingPlanRequestDto request, string rejectedBy, CancellationToken cancellationToken = default);
        Task<bool> DeleteAnnualPlanAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default);
    }
}
