using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs
{
    /// <summary>
    /// Filter pencarian master paket manfaat rumah sakit (BE-HRD-177).
    /// </summary>
    public class BenefitPlanFilterDto
    {
        public string? BenefitCategory { get; set; }
        public string? CoverageTier { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
    }

    /// <summary>
    /// Permintaan pembuatan master paket manfaat 3-lapisan (BE-HRD-177, HRD-DEC-134, FR-BEN-01, FR-BEN-02).
    /// </summary>
    public class CreateBenefitPlanRequestDto
    {
        [Required]
        public Guid BenefitTypeId { get; set; }

        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }

        [Required, MaxLength(50)]
        public string BenefitPlanCode { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string BenefitPlanName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string BenefitCategory { get; set; } = "BPJS_Mandatory";
        // 'BPJS_Mandatory', 'Private_Insurance', 'InHouse_HospitalCare'

        [Required, MaxLength(50)]
        public string CoverageTier { get; set; } = "Basic";
        // 'Basic', 'Silver', 'Gold', 'Platinum', 'Executive'

        [MaxLength(200)]
        public string? ProviderName { get; set; }

        [MaxLength(100)]
        public string? ExternalPlanCode { get; set; }

        [MaxLength(100)]
        public string? PolicyNumber { get; set; }

        [Required, MaxLength(50)]
        public string CoverageType { get; set; } = "Individual";

        [Range(0, 1000000000)]
        public decimal? CoverageLimitAmount { get; set; }

        [Range(0, 100)]
        public decimal HospitalDiscountPercentage { get; set; } = 0.00m;

        public bool RequiresBPJSPrimaryCOB { get; set; } = true;

        [Range(0, 36)]
        public int MinimumServiceMonths { get; set; } = 3;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsDefault { get; set; } = false;
        public int SortOrder { get; set; } = 0;
    }

    /// <summary>
    /// Permintaan pembaruan data master paket manfaat (BE-HRD-177).
    /// </summary>
    public class UpdateBenefitPlanRequestDto
    {
        [Required, MaxLength(150)]
        public string BenefitPlanName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string BenefitCategory { get; set; } = "BPJS_Mandatory";

        [Required, MaxLength(50)]
        public string CoverageTier { get; set; } = "Basic";

        [MaxLength(200)]
        public string? ProviderName { get; set; }

        [Range(0, 1000000000)]
        public decimal? CoverageLimitAmount { get; set; }

        [Range(0, 100)]
        public decimal HospitalDiscountPercentage { get; set; } = 0.00m;

        public bool RequiresBPJSPrimaryCOB { get; set; } = true;

        [Range(0, 36)]
        public int MinimumServiceMonths { get; set; } = 3;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Respons informasi paket manfaat rumah sakit (BE-HRD-177).
    /// </summary>
    public class BenefitPlanResponseDto
    {
        public Guid Id { get; set; }
        public Guid BenefitTypeId { get; set; }
        public string? BenefitTypeName { get; set; }
        public string BenefitPlanCode { get; set; } = string.Empty;
        public string BenefitPlanName { get; set; } = string.Empty;
        public string BenefitCategory { get; set; } = string.Empty;
        public string CoverageTier { get; set; } = string.Empty;
        public string? ProviderName { get; set; }
        public string? ExternalPlanCode { get; set; }
        public string? PolicyNumber { get; set; }
        public string CoverageType { get; set; } = "Individual";
        public string CurrencyCode { get; set; } = "IDR";
        public decimal? CoverageLimitAmount { get; set; }
        public decimal HospitalDiscountPercentage { get; set; }
        public bool RequiresBPJSPrimaryCOB { get; set; }
        public int MinimumServiceMonths { get; set; }
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public List<BenefitGradeEligibilityResponseDto> GradeEligibilities { get; set; } = new();
    }

    /// <summary>
    /// Permintaan pembuatan aturan kelayakan paket benefit berdasarkan grade staf RS (BE-HRD-178, FR-BEN-03, FR-BEN-04).
    /// </summary>
    public class CreateBenefitGradeEligibilityRequestDto
    {
        [Required]
        public Guid BenefitPlanId { get; set; }

        [Required, MaxLength(50)]
        public string GradeLevelCode { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string EmploymentTypeCode { get; set; } = "Permanent";

        [Range(0, 1000000000)]
        public decimal? SpecificAnnualLimit { get; set; }

        [Required, MaxLength(50)]
        public string InpatientRoomClass { get; set; } = "Kelas 1";
    }

    /// <summary>
    /// Permintaan pembaruan aturan kelayakan grade (BE-HRD-178).
    /// </summary>
    public class UpdateBenefitGradeEligibilityRequestDto
    {
        [Range(0, 1000000000)]
        public decimal? SpecificAnnualLimit { get; set; }

        [Required, MaxLength(50)]
        public string InpatientRoomClass { get; set; } = "Kelas 1";

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Respons aturan kelayakan grade staf rumah sakit (BE-HRD-178).
    /// </summary>
    public class BenefitGradeEligibilityResponseDto
    {
        public Guid Id { get; set; }
        public Guid BenefitPlanId { get; set; }
        public string? BenefitPlanCode { get; set; }
        public string? BenefitPlanName { get; set; }
        public string GradeLevelCode { get; set; } = string.Empty;
        public string EmploymentTypeCode { get; set; } = string.Empty;
        public decimal? SpecificAnnualLimit { get; set; }
        public string InpatientRoomClass { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Permintaan evaluasi eligibilitas paket manfaat staf (BE-HRD-180, AT-HRD-BEN-01, AT-HRD-BEN-02).
    /// </summary>
    public class EvaluateEmployeeBenefitEligibilityRequestDto
    {
        [Required]
        public Guid EmployeeId { get; set; }

        public int? SimulatedServiceMonths { get; set; }
    }

    /// <summary>
    /// Hasil evaluasi eligibilitas terpadu paket manfaat staf rumah sakit (BE-HRD-180).
    /// </summary>
    public class EmployeeBenefitEligibilityEvaluationResultDto
    {
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNik { get; set; }
        public string? GradeLevelCode { get; set; }
        public string? EmploymentTypeCode { get; set; }
        public DateTime? JoinDate { get; set; }
        public int ActualServiceMonths { get; set; }
        public bool IsProbation { get; set; }
        public List<EligiblePlanSummaryDto> EligiblePlans { get; set; } = new();
        public List<IneligiblePlanSummaryDto> IneligiblePlans { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }

    public class EligiblePlanSummaryDto
    {
        public Guid PlanId { get; set; }
        public string PlanCode { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string BenefitCategory { get; set; } = string.Empty;
        public string CoverageTier { get; set; } = string.Empty;
        public decimal? AnnualLimitAmount { get; set; }
        public string InpatientRoomClass { get; set; } = string.Empty;
        public decimal HospitalDiscountPercentage { get; set; }
        public bool RequiresBPJSPrimaryCOB { get; set; }
        public bool IsActive { get; set; }
    }

    public class IneligiblePlanSummaryDto
    {
        public Guid PlanId { get; set; }
        public string PlanCode { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string IneligibilityReason { get; set; } = string.Empty;
    }
}
