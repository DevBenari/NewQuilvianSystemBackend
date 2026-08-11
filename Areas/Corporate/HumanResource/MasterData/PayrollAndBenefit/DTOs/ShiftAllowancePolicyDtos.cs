using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.DTOs
{
    public class ShiftAllowancePolicySummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int DefaultData { get; set; }
        public int NightShiftOnlyData { get; set; }
        public int AttendanceMatchRequiredData { get; set; }
    }

    public class ShiftAllowancePolicyResponse
    {
        public Guid Id { get; set; }
        public Guid AllowanceTypeId { get; set; }
        public Guid? ShiftId { get; set; }
        public Guid? ShiftGroupId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
        public Guid? EmploymentTypeId { get; set; }

        public string ShiftAllowancePolicyCode { get; set; } = string.Empty;
        public string ShiftAllowancePolicyName { get; set; } = string.Empty;
        public string CalculationMethod { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;

        public decimal RateAmount { get; set; }
        public decimal PercentageOfBaseSalary { get; set; }
        public int MinimumEligibleMinutes { get; set; }
        public decimal? MaximumAmountPerPeriod { get; set; }

        public bool ApplyOnWorkday { get; set; }
        public bool ApplyOnWeekend { get; set; }
        public bool ApplyOnHoliday { get; set; }
        public bool ApplyOnlyNightShift { get; set; }
        public bool RequireAttendanceMatch { get; set; }
        public bool RequireCompletedShift { get; set; }

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

    public class ShiftAllowancePolicyDetailResponse : ShiftAllowancePolicyResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class ShiftAllowancePolicyOptionResponse
    {
        public Guid Id { get; set; }
        public Guid AllowanceTypeId { get; set; }
        public string ShiftAllowancePolicyCode { get; set; } = string.Empty;
        public string ShiftAllowancePolicyName { get; set; } = string.Empty;
        public string CalculationMethod { get; set; } = string.Empty;
        public decimal RateAmount { get; set; }
        public bool IsDefault { get; set; }
    }

    public class ShiftAllowancePolicyFilterMetadataResponse
    {
        public ShiftAllowancePolicyDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<ShiftAllowancePolicyCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<ShiftAllowancePolicyStringOptionResponse> CalculationMethodOptions { get; set; } = new();
        public List<ShiftAllowancePolicySortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class ShiftAllowancePolicyDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? AllowanceTypeId { get; set; }
        public Guid? ShiftId { get; set; }
        public Guid? ShiftGroupId { get; set; }
        public string? CalculationMethod { get; set; }
        public bool? IsDefault { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "priority";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class ShiftAllowancePolicyCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateShiftAllowancePolicyRequest
    {
        [Required]
        public Guid AllowanceTypeId { get; set; }

        public Guid? ShiftId { get; set; }
        public Guid? ShiftGroupId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
        public Guid? EmploymentTypeId { get; set; }

        [Required]
        [MaxLength(150)]
        public string ShiftAllowancePolicyName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string CalculationMethod { get; set; } = "FixedPerShift";

        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        [Range(0, double.MaxValue)]
        public decimal RateAmount { get; set; }

        [Range(0, 100)]
        public decimal PercentageOfBaseSalary { get; set; }

        [Range(0, int.MaxValue)]
        public int MinimumEligibleMinutes { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MaximumAmountPerPeriod { get; set; }

        public bool ApplyOnWorkday { get; set; } = true;
        public bool ApplyOnWeekend { get; set; } = true;
        public bool ApplyOnHoliday { get; set; } = true;
        public bool ApplyOnlyNightShift { get; set; }
        public bool RequireAttendanceMatch { get; set; } = true;
        public bool RequireCompletedShift { get; set; } = true;

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int Priority { get; set; }
        public bool IsDefault { get; set; }
    }

    public class UpdateShiftAllowancePolicyRequest : CreateShiftAllowancePolicyRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateShiftAllowancePolicyStatusRequest
    {
        public bool IsActive { get; set; }
        public bool? IsDefault { get; set; }
    }


    public class ShiftAllowancePolicyStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class ShiftAllowancePolicySortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class ShiftAllowancePolicyOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<ShiftAllowancePolicyOptionResponse> Items { get; set; } = new();
    }

}
