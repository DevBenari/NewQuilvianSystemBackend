using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs
{
    public class WfpTransportAllowanceTransactionSummaryResponse
    {
        public int TotalData { get; set; }
        public int DraftData { get; set; }
        public int ApprovedData { get; set; }
        public int PostedData { get; set; }
        public int ReversedData { get; set; }
        public int CancelledData { get; set; }
        public decimal TotalAccrualAmount { get; set; }
        public decimal TotalPaymentAmount { get; set; }
        public decimal TotalAdjustmentAmount { get; set; }
    }

    public class WfpTransportAllowanceTransactionResponse
    {
        public Guid Id { get; set; }
        public Guid TransportAllowanceId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public Guid? PayrollPeriodId { get; set; }
        public string? PayrollPeriodCode { get; set; }
        public string? PayrollPeriodName { get; set; }
        public Guid? PayrollRunEmployeeId { get; set; }
        public Guid? AttendanceDailyId { get; set; }
        public string TransactionNumber { get; set; } = string.Empty;
        public DateOnly TransactionDate { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public string TransactionStatus { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAfterTransaction { get; set; }
        public string? SourceType { get; set; }
        public Guid? SourceId { get; set; }
        public DateTime? PostedAt { get; set; }
        public Guid? PostedByUserId { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class WfpTransportAllowanceTransactionDetailResponse : WfpTransportAllowanceTransactionResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class WfpTransportAllowanceTransactionFilterMetadataResponse
    {
        public WfpTransportAllowanceTransactionDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WfpTransportAllowanceTransactionStringOptionResponse> TransactionTypeOptions { get; set; } = new();
        public List<WfpTransportAllowanceTransactionStringOptionResponse> TransactionStatusOptions { get; set; } = new();
        public List<WfpTransportAllowanceTransactionSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class WfpTransportAllowanceTransactionDefaultFilterResponse
    {
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public Guid? PayrollPeriodId { get; set; }
        public string? TransactionType { get; set; }
        public string? TransactionStatus { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "transactionDate";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WfpTransportAllowanceTransactionStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WfpTransportAllowanceTransactionSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateWfpTransportAllowanceTransactionRequest
    {
        [Required]
        public Guid TransportAllowanceId { get; set; }

        public Guid? PayrollPeriodId { get; set; }
        public Guid? PayrollRunEmployeeId { get; set; }
        public Guid? AttendanceDailyId { get; set; }

        [Required]
        public DateOnly TransactionDate { get; set; }

        [Required]
        [MaxLength(30)]
        public string TransactionType { get; set; } = "Accrual";

        [Range(0, double.MaxValue)]
        public decimal Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Rate { get; set; }

        public decimal Amount { get; set; }
        public decimal BalanceAfterTransaction { get; set; }

        [MaxLength(50)]
        public string? SourceType { get; set; }

        public Guid? SourceId { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    public class UpdateWfpTransportAllowanceTransactionRequest : CreateWfpTransportAllowanceTransactionRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateWfpTransportAllowanceTransactionStatusRequest
    {
        [Required]
        [MaxLength(30)]
        public string TransactionStatus { get; set; } = "Draft";

        [MaxLength(1000)]
        public string? Description { get; set; }
    }
}
