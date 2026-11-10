using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs
{
    public class LeaveBalanceLedgerSummaryDto
    {
        public Guid WorkforceProfileId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public Guid LeaveTypeId { get; set; }
        public string LeaveTypeCode { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public int PeriodYear { get; set; }
        public decimal TotalEntitlementDays { get; set; }
        public decimal ActiveCarryForwardDays { get; set; }
        public DateTime? CarryForwardExpiryDate { get; set; }
        public decimal TotalTakenDays { get; set; }
        public decimal TotalRestoredDays { get; set; }
        public decimal TotalForfeitedDays { get; set; }
        public decimal TotalAdjustedDays { get; set; }
        public decimal CurrentBalanceDays { get; set; }
        public decimal ReservedDays { get; set; }
        public decimal AvailableBalanceDays { get; set; }
        public bool IsCarryForwardExpiringSoon { get; set; }
        public string? ExpiryWarningMessage { get; set; }
    }

    public class LeaveBalanceLedgerItemDto
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string? EmployeeName { get; set; }
        public Guid LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public int PeriodYear { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public decimal AmountDays { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public DateTime TransactionDate { get; set; }
        public Guid? ReferenceRequestId { get; set; }
        public string? ReferenceRequestNumber { get; set; }
        public Guid? ReferenceRecallId { get; set; }
        public string? ReferenceRecallNumber { get; set; }
        public Guid? ReferenceAdjustmentId { get; set; }
        public string? AuditDocumentNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsForfeited { get; set; }
        public string? Notes { get; set; }
    }

    public class LeaveBalanceLedgerFilterRequest
    {
        public Guid? WorkforceProfileId { get; set; }
        public Guid? LeaveTypeId { get; set; }
        public int? PeriodYear { get; set; }
        public string? TransactionType { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class LeaveBalanceLedgerPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<LeaveBalanceLedgerItemDto> Items { get; set; } = new();
    }

    public class PostLeaveEntitlementRequest
    {
        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public Guid LeaveTypeId { get; set; }

        [Required]
        public int PeriodYear { get; set; }

        public decimal QuotaDays { get; set; } = 12.0m;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class ManualLeaveAdjustmentRequest
    {
        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public Guid LeaveTypeId { get; set; }

        [Required]
        public int PeriodYear { get; set; }

        [Required]
        public decimal AdjustmentDays { get; set; }

        /// <summary>
        /// Increase (+) atau Decrease (-)
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Direction { get; set; } = "Increase";

        /// <summary>
        /// Nomor dokumen audit resmi / SK Direksi (wajib per HRD-DEC-115)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string AuditDocumentNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }

    public class CarryForwardRunResultDto
    {
        public int TargetYear { get; set; }
        public int ProcessedWorkforceCount { get; set; }
        public int CarriedForwardCount { get; set; }
        public decimal TotalCarriedForwardDays { get; set; }
        public int ForfeitedCount { get; set; }
        public decimal TotalForfeitedDays { get; set; }
        public DateTime ExecutedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
