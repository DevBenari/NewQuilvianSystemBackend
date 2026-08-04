using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs
{
    public class WfpTransportAllowanceSummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public decimal TotalMonthlyAmount { get; set; }
        public decimal TotalAccruedAmount { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal TotalRemainingAmount { get; set; }
    }

    public class WfpTransportAllowanceResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public Guid? EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? TransportAllowancePolicyId { get; set; }
        public string? TransportAllowancePolicyCode { get; set; }
        public string? TransportAllowancePolicyName { get; set; }
        public Guid? PayrollComponentId { get; set; }
        public string? PayrollComponentCode { get; set; }
        public string? PayrollComponentName { get; set; }
        public string AllowanceStatus { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal MonthlyAmount { get; set; }
        public decimal PerAttendanceAmount { get; set; }
        public decimal MaximumMonthlyAmount { get; set; }
        public decimal AccruedAmount { get; set; }
        public decimal UsedAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int TransactionCount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class WfpTransportAllowanceDetailResponse : WfpTransportAllowanceResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class WfpTransportAllowanceFilterMetadataResponse
    {
        public WfpTransportAllowanceDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WfpTransportAllowanceStringOptionResponse> AllowanceStatusOptions { get; set; } = new();
        public List<WfpTransportAllowanceSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class WfpTransportAllowanceDefaultFilterResponse
    {
        public string? AllowanceStatus { get; set; }
        public Guid? TransportAllowancePolicyId { get; set; }
        public Guid? PayrollComponentId { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "createDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WfpTransportAllowanceStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WfpTransportAllowanceSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateWfpTransportAllowanceRequest
    {
        public Guid? EmployeeId { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? TransportAllowancePolicyId { get; set; }
        public Guid? PayrollComponentId { get; set; }

        [Required]
        [MaxLength(30)]
        public string AllowanceStatus { get; set; } = "Active";

        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        [Range(0, double.MaxValue)]
        public decimal MonthlyAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal PerAttendanceAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MaximumMonthlyAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal AccruedAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal UsedAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal PaidAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal RemainingAmount { get; set; }

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    public class UpdateWfpTransportAllowanceRequest : CreateWfpTransportAllowanceRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateWfpTransportAllowanceStatusRequest
    {
        [Required]
        [MaxLength(30)]
        public string AllowanceStatus { get; set; } = "Active";

        public bool IsActive { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
    }
}
