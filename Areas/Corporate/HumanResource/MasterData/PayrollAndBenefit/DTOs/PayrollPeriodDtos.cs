using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.DTOs
{
    public class PayrollPeriodSummaryResponse
    {
        public int TotalPayrollPeriod { get; set; }
        public int ActivePayrollPeriod { get; set; }
        public int InactivePayrollPeriod { get; set; }
        public int DraftPayrollPeriod { get; set; }
        public int OpenPayrollPeriod { get; set; }
        public int ProcessingPayrollPeriod { get; set; }
        public int ApprovedPayrollPeriod { get; set; }
        public int ClosedPayrollPeriod { get; set; }
        public int LockedPayrollPeriod { get; set; }
    }

    public class PayrollPeriodResponse
    {
        public Guid Id { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public string PayrollPeriodCode { get; set; } = string.Empty;
        public string PayrollPeriodName { get; set; } = string.Empty;
        public string PeriodType { get; set; } = string.Empty;
        public int FiscalYear { get; set; }
        public int PeriodNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? AttendanceCutoffDate { get; set; }
        public DateTime? VariableInputCutoffDate { get; set; }
        public DateTime? ApprovalDueDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string PayrollPeriodStatus { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
        public DateTime? LockedAt { get; set; }
        public Guid? LockedByUserId { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public int WorkforcePayrollCount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class PayrollPeriodDetailResponse : PayrollPeriodResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class PayrollPeriodOptionResponse
    {
        public Guid Id { get; set; }
        public string PayrollPeriodCode { get; set; } = string.Empty;
        public string PayrollPeriodName { get; set; } = string.Empty;
        public string PeriodType { get; set; } = string.Empty;
        public int FiscalYear { get; set; }
        public int PeriodNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string PayrollPeriodStatus { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
    }

    public class PayrollPeriodOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<PayrollPeriodOptionResponse> Items { get; set; } = new();
    }

    public class PayrollPeriodFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public PayrollPeriodDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<PayrollMasterStringOptionResponse> PeriodTypeOptions { get; set; } = new();
        public List<PayrollMasterStringOptionResponse> PayrollPeriodStatusOptions { get; set; } = new();
        public List<PayrollMasterCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<PayrollMasterSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class PayrollPeriodDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public string? PeriodType { get; set; }
        public int? FiscalYear { get; set; }
        public string? PayrollPeriodStatus { get; set; }
        public bool? IsLocked { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "startDate";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CreatePayrollPeriodRequest
    {
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }

        [Required]
        [MaxLength(150)]
        public string PayrollPeriodName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string PeriodType { get; set; } = "Monthly";

        [Range(2000, 2200)]
        public int FiscalYear { get; set; }

        [Range(1, 366)]
        public int PeriodNumber { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public DateTime? AttendanceCutoffDate { get; set; }
        public DateTime? VariableInputCutoffDate { get; set; }
        public DateTime? ApprovalDueDate { get; set; }
        public DateTime? PaymentDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string PayrollPeriodStatus { get; set; } = "Draft";

        [MaxLength(500)]
        public string? Description { get; set; }

        public int SortOrder { get; set; }
    }

    public class UpdatePayrollPeriodRequest : CreatePayrollPeriodRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdatePayrollPeriodStatusRequest
    {
        [Required]
        [MaxLength(50)]
        public string PayrollPeriodStatus { get; set; } = "Draft";

        public bool IsActive { get; set; } = true;
    }

    public class UpdatePayrollPeriodLockRequest
    {
        public bool IsLocked { get; set; }
    }

    public class PayrollPeriodCreateResponse
    {
        public Guid Id { get; set; }
        public string PayrollPeriodCode { get; set; } = string.Empty;
        public string PayrollPeriodName { get; set; } = string.Empty;
        public string PayrollPeriodStatus { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
