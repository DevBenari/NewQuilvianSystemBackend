namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs
{
    public class OvertimeCompensatoryLeaveQueryRequest
    {
        public string? Search { get; set; }
        public string? CompensatoryStatus { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public Guid? LeaveTypeId { get; set; }
        public Guid? OvertimeRealizationId { get; set; }
        public DateOnly? EarnedStartDate { get; set; }
        public DateOnly? EarnedEndDate { get; set; }
        public DateOnly? ExpiryStartDate { get; set; }
        public DateOnly? ExpiryEndDate { get; set; }
        public bool? HasLedger { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }
    }

    public class OvertimeCompensatoryLeaveFilterMetadataResponse
    {
        public IReadOnlyCollection<string> CompensatoryStatuses { get; set; } = Array.Empty<string>();
        public IReadOnlyCollection<string> SortFields { get; set; } = Array.Empty<string>();
        public IReadOnlyCollection<int> PageSizeOptions { get; set; } = Array.Empty<int>();
    }

    public class OvertimeCompensatoryLeaveSummaryResponse
    {
        public int TotalCredit { get; set; }
        public int Pending { get; set; }
        public int Available { get; set; }
        public int PartiallyUsed { get; set; }
        public int Used { get; set; }
        public int Expired { get; set; }
        public int Cancelled { get; set; }
        public int WithoutLedger { get; set; }
        public int ExpiringSoon { get; set; }
        public int TotalSourceMinutes { get; set; }
        public int TotalEarnedMinutes { get; set; }
        public int TotalRemainingMinutes { get; set; }
    }

    public class OvertimeCompensatoryLeaveListResponse
    {
        public Guid Id { get; set; }
        public string CreditNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public Guid? OvertimeRequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public Guid? OvertimeRealizationId { get; set; }
        public string RealizationNumber { get; set; } = string.Empty;
        public int RealizationVersion { get; set; }
        public Guid? OvertimeVerificationId { get; set; }
        public Guid? LeaveTypeId { get; set; }
        public string LeaveTypeCode { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public Guid? LeaveBalanceTransactionId { get; set; }
        public DateOnly EarnedDate { get; set; }
        public DateOnly EffectiveStartDate { get; set; }
        public DateOnly? ExpiryDate { get; set; }
        public int SourceOvertimeMinutes { get; set; }
        public decimal ConversionRate { get; set; }
        public int EarnedMinutes { get; set; }
        public int ReservedMinutes { get; set; }
        public int UsedMinutes { get; set; }
        public int ExpiredMinutes { get; set; }
        public int RemainingMinutes { get; set; }
        public string CompensatoryStatus { get; set; } = string.Empty;
        public DateTime? GeneratedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class OvertimeCompensatoryLeaveDetailResponse : OvertimeCompensatoryLeaveListResponse
    {
        public string? Notes { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public string? ApprovedByName { get; set; }
        public string? LedgerTransactionNumber { get; set; }
        public string? LedgerTransactionStatus { get; set; }
        public decimal? LedgerTransactionDays { get; set; }
        public decimal? LedgerAvailableDelta { get; set; }
        public Guid? LeaveBalanceId { get; set; }
        public decimal? BalanceCompensatoryDays { get; set; }
        public decimal? BalanceRemainingDays { get; set; }
        public decimal? BalanceAvailableDays { get; set; }
        public bool IsLedgerConsistent { get; set; }
    }

    public class OvertimeCompensatoryLeaveOptionResponse
    {
        public Guid Id { get; set; }
        public string CreditNumber { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public string CompensatoryStatus { get; set; } = string.Empty;
        public int RemainingMinutes { get; set; }
        public DateOnly? ExpiryDate { get; set; }
    }
}
