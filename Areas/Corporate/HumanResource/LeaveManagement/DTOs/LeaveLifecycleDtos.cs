using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs
{
    public class LeaveLifecycleQueryRequest
    {
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public string? Status { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "createDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class LeaveLifecyclePagedResponse<T>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<T> Items { get; set; } = new();
    }

    public class CreateLeaveCancellationRequest
    {
        public Guid LeaveRequestId { get; set; }
        public Guid? RequestReasonId { get; set; }
        public DateOnly? EffectiveCancellationDate { get; set; }

        [Required, MaxLength(2000)]
        public string CancellationReason { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }
    }

    public class PrepareLeaveLifecycleWorkflowRequest
    {
        [MaxLength(30)]
        public string SourceChannel { get; set; } = "Web";

        [MaxLength(100)]
        public string? CorrelationId { get; set; }

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }

        public List<Guid> SelectedApproverUserIds { get; set; } = new();
    }

    public class SubmitLeaveLifecycleWorkflowRequest : PrepareLeaveLifecycleWorkflowRequest
    {
        [MaxLength(4000)]
        public string? Comment { get; set; }
    }

    public class CancelLeaveLifecycleRequest
    {
        [Required, MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }
    }

    public class LeaveCancellationResponse
    {
        public Guid Id { get; set; }
        public string CancellationNumber { get; set; } = string.Empty;
        public Guid LeaveRequestId { get; set; }
        public string? LeaveRequestNumber { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string? WorkforceProfileCode { get; set; }
        public string? WorkforceDisplayName { get; set; }
        public string? LeaveTypeName { get; set; }
        public DateOnly LeaveStartDate { get; set; }
        public DateOnly LeaveEndDate { get; set; }
        public DateOnly? EffectiveCancellationDate { get; set; }
        public decimal RestoredDays { get; set; }
        public string CancellationReason { get; set; } = string.Empty;
        public string CancellationStatus { get; set; } = string.Empty;
        public Guid? WorkflowInstanceId { get; set; }
        public string? WorkflowStatus { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? AppliedAt { get; set; }
        public string? ApprovalNotes { get; set; }
        public List<string> AvailableActions { get; set; } = new();
    }

    public class CreateLeaveRecallRequest
    {
        public Guid LeaveRequestId { get; set; }
        public Guid? ReplacementWorkforceProfileId { get; set; }
        public DateOnly RecallEffectiveDate { get; set; }

        [Required, MaxLength(2000)]
        public string RecallReason { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }
    }

    public class AcknowledgeReturnToWorkRequest
    {
        public DateOnly ActualReturnToWorkDate { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    public class LeaveRecallResponse
    {
        public Guid Id { get; set; }
        public string RecallNumber { get; set; } = string.Empty;
        public Guid LeaveRequestId { get; set; }
        public string? LeaveRequestNumber { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string? WorkforceProfileCode { get; set; }
        public string? WorkforceDisplayName { get; set; }
        public string? LeaveTypeName { get; set; }
        public DateOnly OriginalLeaveEndDate { get; set; }
        public DateOnly RecallEffectiveDate { get; set; }
        public DateOnly? ActualReturnToWorkDate { get; set; }
        public decimal RecalledLeaveDays { get; set; }
        public decimal RestoredBalanceDays { get; set; }
        public string RecallReason { get; set; } = string.Empty;
        public string RecallStatus { get; set; } = string.Empty;
        public Guid? WorkflowInstanceId { get; set; }
        public string? WorkflowStatus { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? AppliedAt { get; set; }
        public string? Notes { get; set; }
        public List<string> AvailableActions { get; set; } = new();
    }

    public class LeaveLifecycleActionResponse
    {
        public Guid Id { get; set; }
        public Guid LeaveRequestId { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string ReferenceStatus { get; set; } = string.Empty;
        public Guid? WorkflowInstanceId { get; set; }
        public string? WorkflowStatus { get; set; }
        public bool IsIdempotent { get; set; }
        public bool ApplyAttempted { get; set; }
        public bool ApplySucceeded { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class LeaveFinalReconciliationIssueResponse
    {
        public string Code { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? RecommendedAction { get; set; }
    }

    public class LeaveFinalReconciliationResponse
    {
        public Guid LeaveRequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string LeaveRequestStatus { get; set; } = string.Empty;
        public string? WorkflowStatus { get; set; }
        public string? ExecutionStatus { get; set; }
        public string? AttendanceIntegrationStatus { get; set; }
        public string? BalanceExecutionStatus { get; set; }
        public string? CancellationStatus { get; set; }
        public string? CancellationWorkflowStatus { get; set; }
        public string? RecallStatus { get; set; }
        public string? RecallWorkflowStatus { get; set; }
        public decimal EstimatedBalanceDeduction { get; set; }
        public decimal ActualBalanceDeduction { get; set; }
        public decimal LedgerReservedDays { get; set; }
        public decimal LedgerUsedDays { get; set; }
        public decimal LedgerRestoredDays { get; set; }
        public decimal IntegratedLeaveDays { get; set; }
        public int AppliedAttendanceDayCount { get; set; }
        public int ConflictAttendanceDayCount { get; set; }
        public int FailedAttendanceDayCount { get; set; }
        public int ReversedAttendanceDayCount { get; set; }
        public int PayrollProcessedAttendanceDayCount { get; set; }
        public int LockedAttendanceDayCount { get; set; }
        public bool IsTerminal { get; set; }
        public bool IsBalanced { get; set; }
        public bool RequiresAttention { get; set; }
        public List<LeaveFinalReconciliationIssueResponse> Issues { get; set; } = new();
        public List<string> AvailableRepairActions { get; set; } = new();
    }

    public class RepairLeaveFinalReconciliationRequest
    {
        public bool SynchronizeWorkflow { get; set; } = true;
        public bool ApplyApprovedCancellation { get; set; } = true;
        public bool ApplyApprovedRecall { get; set; } = true;
        public bool ExecuteApprovedLeave { get; set; } = true;
        public DateOnly? AsOfDate { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class RepairLeaveFinalReconciliationResponse
    {
        public Guid LeaveRequestId { get; set; }
        public int AttemptedActionCount { get; set; }
        public int SucceededActionCount { get; set; }
        public int FailedActionCount { get; set; }
        public List<string> Messages { get; set; } = new();
        public LeaveFinalReconciliationResponse? Reconciliation { get; set; }
    }
}
