using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Services
{
    public interface ITrainingBondingService
    {
        Task<TrainingBondContractDto> GenerateBondContractAsync(GenerateBondContractRequestDto request, string currentUserId);
        Task<TrainingBondContractDto> CalculateAndApplyPenaltyAsync(Guid contractId, ProcessBreachPenaltyRequestDto request, string currentUserId);
        Task<TrainingBondContractDto?> GetContractByIdAsync(Guid contractId);
        Task<List<TrainingBondContractDto>> GetContractsByStaffAsync(Guid workforceProfileId);
        Task<List<TrainingBondContractDto>> GetActiveContractsAsync();
    }
}
