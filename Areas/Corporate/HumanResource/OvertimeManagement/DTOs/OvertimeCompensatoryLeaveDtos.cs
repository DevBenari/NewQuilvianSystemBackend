using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs
{
    public class PreviewOvertimeCompensatoryLeaveRequest
    {
        [Required]
        public Guid LeaveTypeId { get; set; }
        public decimal ConversionRate { get; set; } = 1m;
        public int MinutesPerDay { get; set; } = 480;
        public DateOnly? EffectiveStartDate { get; set; }
        public DateOnly? ExpiryDate { get; set; }
    }

    public class PostOvertimeCompensatoryLeaveRequest : PreviewOvertimeCompensatoryLeaveRequest
    {
        [MaxLength(1000)]
        public string? Notes { get; set; }

        [MaxLength(120)]
        public string? IdempotencyKey { get; set; }
    }

    public class ReverseOvertimeCompensatoryLeaveRequest
    {
        [Required, MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(120)]
        public string? IdempotencyKey { get; set; }
    }

    public class ReconcileOvertimeCompensatoryLeaveRequest
    {
        public bool AllowRepair { get; set; } = false;
    }

    public class OvertimeCompensatoryLeavePreviewResponse
    {
        public Guid OvertimeRealizationId { get; set; }
        public string RealizationNumber { get; set; } = string.Empty;
        public int RealizationVersion { get; set; }
        public Guid OvertimeRequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public Guid? OvertimeVerificationId { get; set; }
        public int VerifiedMinutes { get; set; }
        public Guid LeaveTypeId { get; set; }
        public string LeaveTypeCode { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public decimal ConversionRate { get; set; }
        public int EarnedMinutes { get; set; }
        public int MinutesPerDay { get; set; }
        public decimal EarnedDays { get; set; }
        public DateOnly EarnedDate { get; set; }
        public DateOnly EffectiveStartDate { get; set; }
        public DateOnly? ExpiryDate { get; set; }
        public Guid? ExistingCompensatoryTimeOffId { get; set; }
        public string? ExistingCreditNumber { get; set; }
        public string? ExistingStatus { get; set; }
        public bool IsIdempotentResult { get; set; }
        public bool CanPost { get; set; }
        public IReadOnlyCollection<string> ValidationMessages { get; set; } = Array.Empty<string>();
    }

    public class OvertimeCompensatoryLeaveMutationResponse
    {
        public Guid CompensatoryTimeOffId { get; set; }
        public string CreditNumber { get; set; } = string.Empty;
        public Guid OvertimeRealizationId { get; set; }
        public string RealizationNumber { get; set; } = string.Empty;
        public Guid OvertimeRequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public Guid LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public Guid? LeaveBalanceId { get; set; }
        public Guid? LeaveBalanceTransactionId { get; set; }
        public string CompensatoryStatus { get; set; } = string.Empty;
        public int SourceOvertimeMinutes { get; set; }
        public decimal ConversionRate { get; set; }
        public int EarnedMinutes { get; set; }
        public decimal PostedDays { get; set; }
        public decimal BalanceCompensatoryDays { get; set; }
        public decimal BalanceAvailableDays { get; set; }
        public DateOnly EffectiveStartDate { get; set; }
        public DateOnly? ExpiryDate { get; set; }
        public bool IsIdempotentResult { get; set; }
        public bool IsReversed { get; set; }
        public DateTime ActionAt { get; set; }
    }

    public class OvertimeCompensatoryLeaveReconciliationResponse
    {
        public Guid CompensatoryTimeOffId { get; set; }
        public string CreditNumber { get; set; } = string.Empty;
        public Guid OvertimeRealizationId { get; set; }
        public Guid? LeaveBalanceTransactionId { get; set; }
        public Guid? ResolvedLeaveBalanceTransactionId { get; set; }
        public bool CreditMatchesRealization { get; set; }
        public bool LedgerExists { get; set; }
        public bool LedgerSourceMatches { get; set; }
        public bool StatusMatches { get; set; }
        public bool IsConsistent { get; set; }
        public bool WasRepaired { get; set; }
        public IReadOnlyCollection<string> Findings { get; set; } = Array.Empty<string>();
    }
}
