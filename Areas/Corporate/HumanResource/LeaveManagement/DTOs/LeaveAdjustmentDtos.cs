using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs
{
    public class LeaveAdjustmentQueryRequest
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public Guid? LeaveBalanceId { get; set; }
        public Guid? LeaveTypeId { get; set; }
        public Guid? LeaveEntitlementPeriodId { get; set; }
        public Guid? LeaveAdjustmentReasonId { get; set; }
        public string? AdjustmentType { get; set; }
        public string? Direction { get; set; }
        public string? AdjustmentStatus { get; set; }
        public bool? HasWorkflow { get; set; }
        public bool? RequiresApproval { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "requestedAt";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class LeaveAdjustmentFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public LeaveAdjustmentDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<LeaveAdjustmentOptionResponse> AdjustmentTypes { get; set; } = new();
        public List<LeaveAdjustmentOptionResponse> Directions { get; set; } = new();
        public List<LeaveAdjustmentOptionResponse> AdjustmentStatuses { get; set; } = new();
        public List<LeaveAdjustmentOptionResponse> CustomPeriods { get; set; } = new();
        public List<LeaveAdjustmentOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new() { "asc", "desc" };
        public List<int> PageSizeOptions { get; set; } = new() { 10, 25, 50, 100 };
    }

    public class LeaveAdjustmentDefaultFilterResponse
    {
        public string? CustomPeriod { get; set; } = "thismonth";
        public string? AdjustmentStatus { get; set; }
        public bool? IsActive { get; set; } = true;
        public string SortBy { get; set; } = "requestedAt";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class LeaveAdjustmentOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class LeaveAdjustmentReasonOptionResponse
    {
        public Guid Id { get; set; }
        public Guid? LeaveTypeId { get; set; }
        public string? LeaveTypeCode { get; set; }
        public string? LeaveTypeName { get; set; }
        public string ReasonCode { get; set; } = string.Empty;
        public string ReasonName { get; set; } = string.Empty;
        public string ReasonCategory { get; set; } = string.Empty;
        public string AllowedDirection { get; set; } = string.Empty;
        public bool AllowOpeningBalance { get; set; }
        public bool AllowManualAdjustment { get; set; }
        public bool AllowCorrection { get; set; }
        public bool AllowReversal { get; set; }
        public decimal? MaximumAdjustmentDays { get; set; }
        public bool RequiresComment { get; set; }
        public bool RequiresAttachment { get; set; }
        public bool RequiresApproval { get; set; }
        public string? ApprovalWorkflowCode { get; set; }
    }

    public class LeaveAdjustmentSummaryResponse
    {
        public int TotalAdjustment { get; set; }
        public int Draft { get; set; }
        public int WaitingApproval { get; set; }
        public int NeedRevision { get; set; }
        public int ApprovedPendingPost { get; set; }
        public int Posted { get; set; }
        public int Rejected { get; set; }
        public int Cancelled { get; set; }
        public int Reversed { get; set; }
        public int OpeningBalance { get; set; }
        public int ManualAdjustment { get; set; }
        public int CreditCount { get; set; }
        public int DebitCount { get; set; }
        public decimal TotalRequestedCreditDays { get; set; }
        public decimal TotalRequestedDebitDays { get; set; }
        public decimal TotalPostedCreditDays { get; set; }
        public decimal TotalPostedDebitDays { get; set; }
    }

    public class LeaveAdjustmentResponse
    {
        public Guid Id { get; set; }
        public string AdjustmentNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string? WorkforceProfileCode { get; set; }
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public Guid LeaveBalanceId { get; set; }
        public Guid LeaveTypeId { get; set; }
        public string LeaveTypeCode { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public Guid LeaveEntitlementPeriodId { get; set; }
        public string? PeriodCode { get; set; }
        public string? PeriodName { get; set; }
        public Guid LeaveAdjustmentReasonId { get; set; }
        public string ReasonCode { get; set; } = string.Empty;
        public string ReasonName { get; set; } = string.Empty;
        public string AdjustmentType { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
        public decimal RequestedDays { get; set; }
        public decimal? ApprovedDays { get; set; }
        public decimal PostedDays { get; set; }
        public DateOnly EffectiveDate { get; set; }
        public string AdjustmentStatus { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public Guid? WorkflowInstanceId { get; set; }
        public bool HasWorkflow { get; set; }
        public bool RequiresWorkflow { get; set; }
        public bool RequiresAttachment { get; set; }
        public bool RequiresApproval { get; set; }
        public int AttachmentCount { get; set; }
        public DateTime RequestedAt { get; set; }
        public Guid RequestedByUserId { get; set; }
        public string? RequestedByName { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? PostedAt { get; set; }
        public DateTime? ReversedAt { get; set; }
        public bool IsActive { get; set; }
        public List<string> AvailableActions { get; set; } = new();
    }

    public class LeaveAdjustmentDetailResponse : LeaveAdjustmentResponse
    {
        public Guid? OriginalAdjustmentId { get; set; }
        public string? OriginalAdjustmentNumber { get; set; }
        public Guid? ReversalAdjustmentId { get; set; }
        public string? ReversalAdjustmentNumber { get; set; }
        public string? RequestNote { get; set; }
        public string SourceType { get; set; } = string.Empty;
        public Guid? SourceReferenceId { get; set; }
        public string? SourceReferenceNumber { get; set; }
        public Guid? SubmittedByUserId { get; set; }
        public string? SubmittedByName { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public string? ApprovedByName { get; set; }
        public string? ApprovalNote { get; set; }
        public DateTime? RejectedAt { get; set; }
        public Guid? RejectedByUserId { get; set; }
        public string? RejectedByName { get; set; }
        public string? RejectionReason { get; set; }
        public Guid? PostedByUserId { get; set; }
        public string? PostedByName { get; set; }
        public Guid? ReversedByUserId { get; set; }
        public string? ReversedByName { get; set; }
        public string? ReversalReason { get; set; }
        public string? RequestSnapshotJson { get; set; }
        public string? ApprovalSnapshotJson { get; set; }
        public string? PostingSnapshotJson { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public LeaveAdjustmentBalanceSnapshotResponse Balance { get; set; } = new();
        public LeaveAdjustmentPostingResponse? Posting { get; set; }
        public WorkflowInstanceDetailResponse? Workflow { get; set; }
    }

    public class LeaveAdjustmentBalanceSnapshotResponse
    {
        public decimal OpeningBalanceDays { get; set; }
        public decimal AdjustmentDays { get; set; }
        public decimal ReservedDays { get; set; }
        public decimal UsedDays { get; set; }
        public decimal RemainingDays { get; set; }
        public decimal AvailableDays { get; set; }
        public string BalanceStatus { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
        public long BalanceVersion { get; set; }
        public long LastTransactionSequence { get; set; }
    }

    public class LeaveAdjustmentPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<LeaveAdjustmentResponse> Items { get; set; } = new();
    }

    public class CreateLeaveAdjustmentRequest
    {
        public Guid LeaveBalanceId { get; set; }
        public Guid LeaveAdjustmentReasonId { get; set; }

        [Required, MaxLength(30)]
        public string AdjustmentType { get; set; } = string.Empty;

        [Required, MaxLength(10)]
        public string Direction { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.0001", "999999999")]
        public decimal RequestedDays { get; set; }

        public DateOnly EffectiveDate { get; set; }

        [Required, MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? RequestNote { get; set; }

        [MaxLength(50)]
        public string? SourceType { get; set; }

        public Guid? SourceReferenceId { get; set; }

        [MaxLength(100)]
        public string? SourceReferenceNumber { get; set; }

        [MaxLength(150)]
        public string? IdempotencyKey { get; set; }

        public string? SourceChannel { get; set; }
        public string? RequestCorrelationId { get; set; }
        public List<Guid> SelectedApproverUserIds { get; set; } = new();
    }

    public class UpdateLeaveAdjustmentRequest
    {
        public Guid LeaveAdjustmentReasonId { get; set; }

        [Required, MaxLength(30)]
        public string AdjustmentType { get; set; } = string.Empty;

        [Required, MaxLength(10)]
        public string Direction { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.0001", "999999999")]
        public decimal RequestedDays { get; set; }

        public DateOnly EffectiveDate { get; set; }

        [Required, MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? RequestNote { get; set; }
    }

    public class PrepareLeaveAdjustmentWorkflowRequest
    {
        [MaxLength(30)]
        public string SourceChannel { get; set; } = "Web";

        [MaxLength(100)]
        public string? RequestCorrelationId { get; set; }

        public List<Guid> SelectedApproverUserIds { get; set; } = new();
    }

    public class SubmitLeaveAdjustmentRequest : PrepareLeaveAdjustmentWorkflowRequest
    {
        [MaxLength(1000)]
        public string? Note { get; set; }

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }
    }

    public class CancelLeaveAdjustmentRequest
    {
        [Required, MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }
    }

    public class PostLeaveAdjustmentRequest
    {
        [MaxLength(1000)]
        public string? Note { get; set; }

        [MaxLength(150)]
        public string? IdempotencyKey { get; set; }
    }

    public class ReverseLeaveAdjustmentRequest
    {
        public Guid LeaveAdjustmentReasonId { get; set; }
        public DateOnly EffectiveDate { get; set; }

        [Required, MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? IdempotencyKey { get; set; }
    }

    public class LeaveAdjustmentPostingResponse
    {
        public Guid LeaveAdjustmentId { get; set; }
        public string AdjustmentNumber { get; set; } = string.Empty;
        public Guid LeaveBalanceId { get; set; }
        public Guid BalanceTransactionId { get; set; }
        public string TransactionNumber { get; set; } = string.Empty;
        public long TransactionSequence { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
        public decimal PostedDays { get; set; }
        public decimal PreviousAvailableDays { get; set; }
        public decimal NewAvailableDays { get; set; }
        public decimal NewRemainingDays { get; set; }
        public long BalanceVersion { get; set; }
        public bool IsIdempotent { get; set; }
        public DateTime PostedAt { get; set; }
    }

    public class LeaveAdjustmentActionResponse
    {
        public LeaveAdjustmentDetailResponse Adjustment { get; set; } = new();
        public string? WarningMessage { get; set; }
    }
}
