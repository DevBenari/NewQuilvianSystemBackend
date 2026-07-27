using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs
{
    public class WfpSalaryAssignmentSummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int PrimaryData { get; set; }
        public int ApprovedData { get; set; }
        public int PendingApprovalData { get; set; }
    }

    public class WfpSalaryAssignmentResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public Guid SalaryStructureId { get; set; }
        public Guid SalaryGradeId { get; set; }
        public Guid? EmployeeGradeId { get; set; }
        public Guid? PayrollPeriodId { get; set; }
        public decimal BaseSalary { get; set; }
        public string CurrencyCode { get; set; } = "IDR";
        public string PaymentFrequency { get; set; } = "Monthly";
        public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsConfidential { get; set; }
        public bool IsActive { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public string? ApprovedByUserName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public bool IsApproved { get; set; }
        public string? Description { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class WfpSalaryAssignmentFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public WfpSalaryAssignmentDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WfpSalaryAssignmentStringOptionResponse> CustomPeriods { get; set; } = new();
        public List<WfpSalaryAssignmentStringOptionResponse> PaymentFrequencyOptions { get; set; } = new();
        public List<WfpSalaryAssignmentStringOptionResponse> CurrencyOptions { get; set; } = new();
        public List<WfpSalaryAssignmentStringOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class WfpSalaryAssignmentDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? SalaryStructureId { get; set; }
        public Guid? SalaryGradeId { get; set; }
        public string? PaymentFrequency { get; set; }
        public string? CurrencyCode { get; set; }
        public bool? IsPrimary { get; set; }
        public bool? IsApproved { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "effectiveStartDate";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WfpSalaryAssignmentStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateWfpSalaryAssignmentRequest
    {
        [Required]
        public Guid SalaryStructureId { get; set; }

        [Required]
        public Guid SalaryGradeId { get; set; }

        public Guid? EmployeeGradeId { get; set; }
        public Guid? PayrollPeriodId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal BaseSalary { get; set; }

        [Required, StringLength(3, MinimumLength = 3)]
        public string CurrencyCode { get; set; } = "IDR";

        [Required, MaxLength(50)]
        public string PaymentFrequency { get; set; } = "Monthly";

        [Required]
        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }
        public bool IsPrimary { get; set; } = true;
        public bool IsConfidential { get; set; } = true;
        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateWfpSalaryAssignmentRequest : CreateWfpSalaryAssignmentRequest { }

    public class UpdateWfpSalaryAssignmentStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class SetWfpSalaryAssignmentPrimaryRequest
    {
        public bool IsPrimary { get; set; } = true;
    }

    public class ApproveWfpSalaryAssignmentRequest
    {
        public bool IsApproved { get; set; } = true;
    }
}
