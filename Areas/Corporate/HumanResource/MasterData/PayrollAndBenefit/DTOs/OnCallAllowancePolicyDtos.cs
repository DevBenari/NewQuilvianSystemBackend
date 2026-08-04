using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.DTOs
{
    public class OnCallAllowancePolicySummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int DefaultData { get; set; }
        public int AttendanceEvidenceRequiredData { get; set; }
        public int SupervisorVerificationRequiredData { get; set; }
    }

    public class OnCallAllowancePolicyResponse
    {
        public Guid Id { get; set; }
        public Guid AllowanceTypeId { get; set; }
        public Guid? OnCallTypeId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
        public Guid? EmploymentTypeId { get; set; }

        public string OnCallAllowancePolicyCode { get; set; } = string.Empty;
        public string OnCallAllowancePolicyName { get; set; } = string.Empty;
        public string CalculationMethod { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;

        public decimal BaseRateAmount { get; set; }
        public decimal ActualCallRateAmount { get; set; }
        public decimal HourlyRateAmount { get; set; }
        public decimal PercentageOfBaseSalary { get; set; }
        public int MinimumOnCallHours { get; set; }
        public decimal? MaximumAmountPerPeriod { get; set; }
        public decimal WeekendMultiplier { get; set; }
        public decimal HolidayMultiplier { get; set; }

        public bool RequireAttendanceEvidence { get; set; }
        public bool RequireSupervisorVerification { get; set; }

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

    public class OnCallAllowancePolicyDetailResponse : OnCallAllowancePolicyResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class OnCallAllowancePolicyOptionResponse
    {
        public Guid Id { get; set; }
        public Guid AllowanceTypeId { get; set; }
        public string OnCallAllowancePolicyCode { get; set; } = string.Empty;
        public string OnCallAllowancePolicyName { get; set; } = string.Empty;
        public string CalculationMethod { get; set; } = string.Empty;
        public decimal BaseRateAmount { get; set; }
        public bool IsDefault { get; set; }
    }

    public class OnCallAllowancePolicyFilterMetadataResponse
    {
        public OnCallAllowancePolicyDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<OnCallAllowancePolicyStringOptionResponse> CalculationMethodOptions { get; set; } = new();
        public List<OnCallAllowancePolicySortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class OnCallAllowancePolicyDefaultFilterResponse
    {
        public Guid? AllowanceTypeId { get; set; }
        public Guid? OnCallTypeId { get; set; }
        public string? CalculationMethod { get; set; }
        public bool? IsDefault { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "priority";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CreateOnCallAllowancePolicyRequest
    {
        [Required]
        public Guid AllowanceTypeId { get; set; }

        public Guid? OnCallTypeId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
        public Guid? EmploymentTypeId { get; set; }

        [Required]
        [MaxLength(150)]
        public string OnCallAllowancePolicyName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string CalculationMethod { get; set; } = "FixedPerAssignment";

        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        [Range(0, double.MaxValue)]
        public decimal BaseRateAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ActualCallRateAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal HourlyRateAmount { get; set; }

        [Range(0, 100)]
        public decimal PercentageOfBaseSalary { get; set; }

        [Range(0, int.MaxValue)]
        public int MinimumOnCallHours { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MaximumAmountPerPeriod { get; set; }

        [Range(0, double.MaxValue)]
        public decimal WeekendMultiplier { get; set; } = 1m;

        [Range(0, double.MaxValue)]
        public decimal HolidayMultiplier { get; set; } = 1m;

        public bool RequireAttendanceEvidence { get; set; }
        public bool RequireSupervisorVerification { get; set; } = true;

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int Priority { get; set; }
        public bool IsDefault { get; set; }
    }

    public class UpdateOnCallAllowancePolicyRequest : CreateOnCallAllowancePolicyRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateOnCallAllowancePolicyStatusRequest
    {
        public bool IsActive { get; set; }
        public bool? IsDefault { get; set; }
    }


    public class OnCallAllowancePolicyStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class OnCallAllowancePolicySortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class OnCallAllowancePolicyOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<OnCallAllowancePolicyOptionResponse> Items { get; set; } = new();
    }

}
