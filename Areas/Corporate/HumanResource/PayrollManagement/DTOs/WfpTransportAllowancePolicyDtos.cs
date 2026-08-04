using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs
{
    public class WfpTransportAllowancePolicySummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int AttendanceBasedData { get; set; }
        public int PayrollIncludedData { get; set; }
        public int TaxableData { get; set; }
    }

    public class WfpTransportAllowancePolicyResponse
    {
        public Guid Id { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? EmployeeGradeId { get; set; }
        public Guid? PayrollComponentId { get; set; }
        public string? PayrollComponentCode { get; set; }
        public string? PayrollComponentName { get; set; }
        public string PolicyCode { get; set; } = string.Empty;
        public string PolicyName { get; set; } = string.Empty;
        public string CalculationMethod { get; set; } = string.Empty;
        public decimal FixedMonthlyAmount { get; set; }
        public decimal PerAttendanceAmount { get; set; }
        public decimal DailyLimitAmount { get; set; }
        public decimal MonthlyLimitAmount { get; set; }
        public int MinimumAttendanceMinutes { get; set; }
        public bool IsAttendanceBased { get; set; }
        public bool IsProrated { get; set; }
        public bool IsTaxable { get; set; }
        public bool IsIncludedInPayroll { get; set; }
        public bool IncludeBusinessTravelDay { get; set; }
        public bool IncludePaidLeaveDay { get; set; }
        public bool IncludeHoliday { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int TransportAllowanceCount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class WfpTransportAllowancePolicyDetailResponse : WfpTransportAllowancePolicyResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class WfpTransportAllowancePolicyOptionResponse
    {
        public Guid Id { get; set; }
        public string PolicyCode { get; set; } = string.Empty;
        public string PolicyName { get; set; } = string.Empty;
        public string CalculationMethod { get; set; } = string.Empty;
        public decimal FixedMonthlyAmount { get; set; }
        public decimal PerAttendanceAmount { get; set; }
    }

    public class WfpTransportAllowancePolicyOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<WfpTransportAllowancePolicyOptionResponse> Items { get; set; } = new();
    }

    public class WfpTransportAllowancePolicyFilterMetadataResponse
    {
        public WfpTransportAllowancePolicyDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WfpTransportAllowancePolicyStringOptionResponse> CalculationMethodOptions { get; set; } = new();
        public List<WfpTransportAllowancePolicySortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class WfpTransportAllowancePolicyDefaultFilterResponse
    {
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? EmployeeGradeId { get; set; }
        public Guid? PayrollComponentId { get; set; }
        public string? CalculationMethod { get; set; }
        public bool? IsAttendanceBased { get; set; }
        public bool? IsIncludedInPayroll { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "policyName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WfpTransportAllowancePolicyStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WfpTransportAllowancePolicySortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateWfpTransportAllowancePolicyRequest
    {
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? EmployeeGradeId { get; set; }
        public Guid? PayrollComponentId { get; set; }

        [Required]
        [MaxLength(150)]
        public string PolicyName { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string CalculationMethod { get; set; } = "FixedMonthly";

        [Range(0, double.MaxValue)]
        public decimal FixedMonthlyAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal PerAttendanceAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal DailyLimitAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MonthlyLimitAmount { get; set; }

        [Range(0, int.MaxValue)]
        public int MinimumAttendanceMinutes { get; set; }

        public bool IsAttendanceBased { get; set; }
        public bool IsProrated { get; set; } = true;
        public bool IsTaxable { get; set; } = true;
        public bool IsIncludedInPayroll { get; set; } = true;
        public bool IncludeBusinessTravelDay { get; set; }
        public bool IncludePaidLeaveDay { get; set; }
        public bool IncludeHoliday { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    public class UpdateWfpTransportAllowancePolicyRequest : CreateWfpTransportAllowancePolicyRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateWfpTransportAllowancePolicyStatusRequest
    {
        public bool IsActive { get; set; }
    }
}
