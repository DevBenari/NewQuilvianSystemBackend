using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services
{
    /// <summary>
    /// Layanan administrasi master paket manfaat 3-lapisan, matriks kelayakan grade staf RS, dan evaluasi eligibilitas terpadu (BE-HRD-177, BE-HRD-178, BE-HRD-180, HRD-DEC-134, FR-BEN-01 s.d. FR-BEN-05).
    /// </summary>
    public interface IBenefitPlanService
    {
        Task<List<BenefitPlanResponseDto>> GetBenefitPlansAsync(BenefitPlanFilterDto filter, CancellationToken cancellationToken = default);
        Task<BenefitPlanResponseDto?> GetBenefitPlanByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<BenefitPlanResponseDto> CreateBenefitPlanAsync(CreateBenefitPlanRequestDto request, Guid userId, CancellationToken cancellationToken = default);
        Task<BenefitPlanResponseDto> UpdateBenefitPlanAsync(Guid id, UpdateBenefitPlanRequestDto request, Guid userId, CancellationToken cancellationToken = default);
        Task<List<BenefitGradeEligibilityResponseDto>> GetGradeEligibilitiesAsync(Guid? benefitPlanId, string? gradeLevelCode, CancellationToken cancellationToken = default);
        Task<BenefitGradeEligibilityResponseDto> AddGradeEligibilityAsync(CreateBenefitGradeEligibilityRequestDto request, Guid userId, CancellationToken cancellationToken = default);
        Task<BenefitGradeEligibilityResponseDto> UpdateGradeEligibilityAsync(Guid id, UpdateBenefitGradeEligibilityRequestDto request, Guid userId, CancellationToken cancellationToken = default);
        Task<bool> DeleteGradeEligibilityAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
        Task<int> SeedDefaultHospitalBenefitPlansAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<EmployeeBenefitEligibilityEvaluationResultDto> EvaluateEmployeePlanEligibilityAsync(EvaluateEmployeeBenefitEligibilityRequestDto request, CancellationToken cancellationToken = default);
    }
}
