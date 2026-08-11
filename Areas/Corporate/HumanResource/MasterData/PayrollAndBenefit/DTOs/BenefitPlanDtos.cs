using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.DTOs
{
    public class BenefitPlanSummaryResponse
    {
        public int TotalBenefitPlan { get; set; }
        public int ActiveBenefitPlan { get; set; }
        public int InactiveBenefitPlan { get; set; }
        public int DefaultBenefitPlan { get; set; }
        public int FamilyCoveragePlan { get; set; }
        public int EnrollmentOpenPlan { get; set; }
    }

    public class BenefitPlanResponse
    {
        public Guid Id { get; set; }
        public Guid BenefitTypeId { get; set; }
        public string? BenefitTypeCode { get; set; }
        public string? BenefitTypeName { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
        public Guid? EmploymentTypeId { get; set; }
        public string BenefitPlanCode { get; set; } = string.Empty;
        public string BenefitPlanName { get; set; } = string.Empty;
        public string? ProviderName { get; set; }
        public string? ExternalPlanCode { get; set; }
        public string? PolicyNumber { get; set; }
        public string CoverageType { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal? CoverageLimitAmount { get; set; }
        public decimal EmployerContributionAmount { get; set; }
        public decimal EmployerContributionPercentage { get; set; }
        public decimal EmployeeContributionAmount { get; set; }
        public decimal EmployeeContributionPercentage { get; set; }
        public int WaitingPeriodMonths { get; set; }
        public int MaximumDependents { get; set; }
        public DateTime? EnrollmentStartDate { get; set; }
        public DateTime? EnrollmentEndDate { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public int EligibilityRuleCount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class BenefitPlanDetailResponse : BenefitPlanResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class BenefitPlanOptionResponse
    {
        public Guid Id { get; set; }
        public string BenefitPlanCode { get; set; } = string.Empty;
        public string BenefitPlanName { get; set; } = string.Empty;
        public Guid BenefitTypeId { get; set; }
        public string? BenefitTypeCode { get; set; }
        public string? BenefitTypeName { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
    }

    public class BenefitPlanOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<BenefitPlanOptionResponse> Items { get; set; } = new();
    }

    public class BenefitPlanFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public BenefitPlanDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<BenefitPlanStringOptionResponse> PrimaryOptions { get; set; } = new();
        public List<BenefitPlanStringOptionResponse> SecondaryOptions { get; set; } = new();
        public List<BenefitPlanSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class BenefitPlanDefaultFilterResponse
    {
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "createDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class BenefitPlanStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class BenefitPlanSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateBenefitPlanRequest
    {
        [Required]
        public Guid BenefitTypeId { get; set; }

        public Guid? LegalEntityId { get; set; }

        public Guid? HospitalSiteId { get; set; }

        public Guid? OrganizationUnitId { get; set; }

        public Guid? EmployeeCategoryId { get; set; }

        public Guid? EmploymentTypeId { get; set; }

        [Required]
        [MaxLength(150)]
        public string BenefitPlanName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? ProviderName { get; set; }

        [MaxLength(100)]
        public string? ExternalPlanCode { get; set; }

        [MaxLength(100)]
        public string? PolicyNumber { get; set; }

        [Required]
        [MaxLength(50)]
        public string CoverageType { get; set; } = "Individual";

        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        [Range(0, double.MaxValue)]
        public decimal? CoverageLimitAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal EmployerContributionAmount { get; set; }

        [Range(0, 100)]
        public decimal EmployerContributionPercentage { get; set; }

        [Range(0, double.MaxValue)]
        public decimal EmployeeContributionAmount { get; set; }

        [Range(0, 100)]
        public decimal EmployeeContributionPercentage { get; set; }

        [Range(0, int.MaxValue)]
        public int WaitingPeriodMonths { get; set; }

        [Range(0, int.MaxValue)]
        public int MaximumDependents { get; set; }

        public DateTime? EnrollmentStartDate { get; set; }

        public DateTime? EnrollmentEndDate { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsDefault { get; set; }

        public int SortOrder { get; set; }

    }

    public class UpdateBenefitPlanRequest : CreateBenefitPlanRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateBenefitPlanStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class BenefitPlanCreateResponse
    {
        public Guid Id { get; set; }
        public string BenefitPlanCode { get; set; } = string.Empty;
        public string BenefitPlanName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}