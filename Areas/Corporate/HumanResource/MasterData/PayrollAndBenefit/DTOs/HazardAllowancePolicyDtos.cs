using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.DTOs
{
    public class HazardAllowancePolicySummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int DefaultData { get; set; }
        public int HealthClearanceRequiredData { get; set; }
        public int ActiveAssignmentRequiredData { get; set; }
    }

    public class HazardAllowancePolicyResponse
    {
        public Guid Id { get; set; }
        public Guid AllowanceTypeId { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? WorkLocationId { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
        public Guid? EmploymentTypeId { get; set; }

        public string HazardAllowancePolicyCode { get; set; } = string.Empty;
        public string HazardAllowancePolicyName { get; set; } = string.Empty;
        public string HazardLevel { get; set; } = string.Empty;
        public string CalculationMethod { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;

        public decimal RateAmount { get; set; }
        public decimal PercentageOfBaseSalary { get; set; }
        public int MinimumExposureDays { get; set; }
        public decimal? MaximumAmountPerPeriod { get; set; }

        public bool RequireOccupationalHealthClearance { get; set; }
        public bool RequireActiveAssignment { get; set; }

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public string? Description { get; set; }
        public int Priority { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class HazardAllowancePolicyDetailResponse : HazardAllowancePolicyResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class HazardAllowancePolicyOptionResponse
    {
        public Guid Id { get; set; }
        public Guid AllowanceTypeId { get; set; }
        public string HazardAllowancePolicyCode { get; set; } = string.Empty;
        public string HazardAllowancePolicyName { get; set; } = string.Empty;
        public string HazardLevel { get; set; } = string.Empty;
        public string CalculationMethod { get; set; } = string.Empty;
        public decimal RateAmount { get; set; }
        public bool IsDefault { get; set; }
    }

    public class HazardAllowancePolicyFilterMetadataResponse
    {
        public HazardAllowancePolicyDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<HazardAllowancePolicyStringOptionResponse> HazardLevelOptions { get; set; } = new();
        public List<HazardAllowancePolicyStringOptionResponse> CalculationMethodOptions { get; set; } = new();
        public List<HazardAllowancePolicySortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class HazardAllowancePolicyDefaultFilterResponse
    {
        public Guid? AllowanceTypeId { get; set; }
        public string? HazardLevel { get; set; }
        public string? CalculationMethod { get; set; }
        public bool? IsDefault { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "priority";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CreateHazardAllowancePolicyRequest
    {
        [Required]
        public Guid AllowanceTypeId { get; set; }

        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? WorkLocationId { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
        public Guid? EmploymentTypeId { get; set; }

        [Required]
        [MaxLength(150)]
        public string HazardAllowancePolicyName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string HazardLevel { get; set; } = "Low";

        [Required]
        [MaxLength(50)]
        public string CalculationMethod { get; set; } = "FixedMonthly";

        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        [Range(0, double.MaxValue)]
        public decimal RateAmount { get; set; }

        [Range(0, 100)]
        public decimal PercentageOfBaseSalary { get; set; }

        [Range(0, int.MaxValue)]
        public int MinimumExposureDays { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MaximumAmountPerPeriod { get; set; }

        public bool RequireOccupationalHealthClearance { get; set; } = true;
        public bool RequireActiveAssignment { get; set; } = true;

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int Priority { get; set; }
        public bool IsDefault { get; set; }
    }

    public class UpdateHazardAllowancePolicyRequest : CreateHazardAllowancePolicyRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateHazardAllowancePolicyStatusRequest
    {
        public bool IsActive { get; set; }
        public bool? IsDefault { get; set; }
    }


    public class HazardAllowancePolicyStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class HazardAllowancePolicySortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class HazardAllowancePolicyOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<HazardAllowancePolicyOptionResponse> Items { get; set; } = new();
    }

}
