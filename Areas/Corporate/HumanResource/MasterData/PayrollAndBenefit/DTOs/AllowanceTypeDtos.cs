using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.DTOs
{
    public class AllowanceTypeSummaryResponse
    {
        public int TotalAllowanceType { get; set; }
        public int ActiveAllowanceType { get; set; }
        public int InactiveAllowanceType { get; set; }
        public int RecurringAllowanceType { get; set; }
        public int TaxableAllowanceType { get; set; }
        public int AttendanceRequiredAllowanceType { get; set; }
        public int ApprovalRequiredAllowanceType { get; set; }
    }

    public class AllowanceTypeResponse
    {
        public Guid Id { get; set; }
        public Guid? PayrollComponentId { get; set; }
        public string? PayrollComponentCode { get; set; }
        public string? PayrollComponentName { get; set; }
        public string AllowanceTypeCode { get; set; } = string.Empty;
        public string AllowanceTypeName { get; set; } = string.Empty;
        public string AllowanceCategory { get; set; } = string.Empty;
        public string CalculationMethod { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal DefaultAmount { get; set; }
        public decimal DefaultPercentage { get; set; }
        public decimal? MaximumAmount { get; set; }
        public bool IsRecurring { get; set; }
        public bool IsTaxable { get; set; }
        public bool IsProrated { get; set; }
        public bool RequiresAttendance { get; set; }
        public bool RequiresApproval { get; set; }
        public bool IsIncludedInBaseSalary { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public int ShiftAllowancePolicyCount { get; set; }
        public int OnCallAllowancePolicyCount { get; set; }
        public int HazardAllowancePolicyCount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class AllowanceTypeDetailResponse : AllowanceTypeResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class AllowanceTypeOptionResponse
    {
        public Guid Id { get; set; }
        public Guid? PayrollComponentId { get; set; }
        public string? PayrollComponentName { get; set; }
        public string AllowanceTypeCode { get; set; } = string.Empty;
        public string AllowanceTypeName { get; set; } = string.Empty;
        public string AllowanceCategory { get; set; } = string.Empty;
        public string CalculationMethod { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal DefaultAmount { get; set; }
        public decimal DefaultPercentage { get; set; }
    }

    public class AllowanceTypeOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<AllowanceTypeOptionResponse> Items { get; set; } = new();
    }

    public class AllowanceTypeFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public AllowanceTypeDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<AllowanceTypeCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<AllowanceTypeStringOptionResponse> AllowanceCategoryOptions { get; set; } = new();
        public List<AllowanceTypeStringOptionResponse> CalculationMethodOptions { get; set; } = new();
        public List<AllowanceTypeSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class AllowanceTypeDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? PayrollComponentId { get; set; }
        public string? AllowanceCategory { get; set; }
        public string? CalculationMethod { get; set; }
        public bool? IsRecurring { get; set; }
        public bool? IsTaxable { get; set; }
        public bool? RequiresAttendance { get; set; }
        public bool? RequiresApproval { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "allowanceTypeName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class AllowanceTypeCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class AllowanceTypeStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class AllowanceTypeSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateAllowanceTypeRequest
    {
        public Guid? PayrollComponentId { get; set; }

        [Required]
        [MaxLength(150)]
        public string AllowanceTypeName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string AllowanceCategory { get; set; } = "Other";

        [Required]
        [MaxLength(50)]
        public string CalculationMethod { get; set; } = "Fixed";

        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        [Range(0, double.MaxValue)]
        public decimal DefaultAmount { get; set; }

        [Range(0, 100)]
        public decimal DefaultPercentage { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MaximumAmount { get; set; }

        public bool IsRecurring { get; set; } = true;
        public bool IsTaxable { get; set; } = true;
        public bool IsProrated { get; set; } = true;
        public bool RequiresAttendance { get; set; }
        public bool RequiresApproval { get; set; }
        public bool IsIncludedInBaseSalary { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int SortOrder { get; set; }
    }

    public class UpdateAllowanceTypeRequest : CreateAllowanceTypeRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateAllowanceTypeStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class AllowanceTypeCreateResponse
    {
        public Guid Id { get; set; }
        public string AllowanceTypeCode { get; set; } = string.Empty;
        public string AllowanceTypeName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
