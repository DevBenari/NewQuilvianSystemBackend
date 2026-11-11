using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Services
{
    public interface ITrainingBudgetControlService
    {
        Task<TrainingBudgetSummaryDto?> GetDepartmentBudgetStatusAsync(Guid departmentId, int? fiscalYear, CancellationToken cancellationToken = default);
        Task<List<TrainingBudgetSummaryDto>> GetPlanBudgetsAsync(Guid trainingPlanId, CancellationToken cancellationToken = default);
        Task<TrainingBudgetSummaryDto> AllocateBudgetAsync(AllocateDepartmentBudgetRequestDto request, string allocatedBy, CancellationToken cancellationToken = default);
        Task<DepartmentBudgetValidationResultDto> ValidateAndCommitBudgetAsync(CommitTrainingBudgetRequestDto request, string committedBy, CancellationToken cancellationToken = default);
        Task<TrainingBudgetSummaryDto> RealizeBudgetAsync(RealizeTrainingBudgetRequestDto request, string realizedBy, CancellationToken cancellationToken = default);
    }
}
