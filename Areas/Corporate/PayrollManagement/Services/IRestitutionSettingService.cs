using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    /// <summary>
    /// Layanan konfigurasi kebijakan plafon tahunan benefit restitusi dan ambang batas pemotongan upah (BE-HRD-162, HRD-DEC-128, HRD-DEC-130).
    /// </summary>
    public interface IRestitutionSettingService
    {
        Task<List<RestitutionSettingResponseDto>> GetAllSettingsAsync(RestitutionSettingFilterDto? filter = null, CancellationToken cancellationToken = default);
        Task<RestitutionSettingResponseDto?> GetSettingByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<RestitutionSettingResponseDto?> GetPolicyForGradeAndCategoryAsync(string gradeLevelCode, string benefitCategory, CancellationToken cancellationToken = default);
        Task<decimal> GetMaxDeductionPercentageAsync(string? gradeLevelCode = null, CancellationToken cancellationToken = default);
        Task<RestitutionSettingResponseDto> CreateOrUpdateSettingAsync(CreateRestitutionSettingRequestDto dto, Guid userId, CancellationToken cancellationToken = default);
        Task<int> SeedDefaultHospitalSettingsAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
