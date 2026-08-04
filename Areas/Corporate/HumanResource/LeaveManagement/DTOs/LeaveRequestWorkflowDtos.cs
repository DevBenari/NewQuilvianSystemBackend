using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs
{
    public class LeaveRequestWorkflowOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class LeaveRequestWorkflowMetadataResponse
    {
        public List<LeaveRequestWorkflowOptionResponse> WorkflowStatuses { get; set; } = new();
        public List<LeaveRequestWorkflowOptionResponse> LeaveRequestStatuses { get; set; } = new();
        public List<LeaveRequestWorkflowOptionResponse> ReservationTimings { get; set; } = new();
        public List<LeaveRequestWorkflowOptionResponse> DeductionTimings { get; set; } = new();
    }

    public class LeaveRequestWorkflowSynchronizeRequest
    {
        public bool AllowBalanceApply { get; set; } = true;

        [MaxLength(1000)]
        public string? Note { get; set; }
    }

    public class LeaveRequestWorkflowStatusResponse
    {
        public Guid LeaveRequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string LeaveRequestStatus { get; set; } = string.Empty;
        public string ExpectedLeaveRequestStatus { get; set; } = string.Empty;

        public Guid? WorkflowInstanceId { get; set; }
        public string? WorkflowRequestNumber { get; set; }
        public string? WorkflowStatus { get; set; }
        public int CurrentWorkflowStepOrder { get; set; }
        public string? CurrentWorkflowStepCode { get; set; }

        public Guid WorkforceProfileId { get; set; }
        public Guid LeaveTypeId { get; set; }
        public Guid? LeaveBalanceId { get; set; }
        public Guid? LeavePolicyId { get; set; }

        public string? ReservationTiming { get; set; }
        public string? DeductionTiming { get; set; }
        public decimal EstimatedBalanceDeduction { get; set; }
        public decimal ActualBalanceDeduction { get; set; }
        public decimal CurrentReservedDays { get; set; }
        public decimal CurrentUsedDays { get; set; }

        public bool IsStatusSynchronized { get; set; }
        public bool IsBalanceSynchronized { get; set; }
        public bool RequiresBalanceRetry { get; set; }
        public bool CanSynchronize { get; set; }

        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime? WorkflowLastActionAt { get; set; }

        public List<string> Issues { get; set; } = new();
        public List<string> AvailableActions { get; set; } = new();
    }

    public class LeaveRequestWorkflowSynchronizationResponse
    {
        public Guid LeaveRequestId { get; set; }
        public Guid WorkflowInstanceId { get; set; }
        public string WorkflowStatus { get; set; } = string.Empty;
        public string PreviousLeaveRequestStatus { get; set; } = string.Empty;
        public string CurrentLeaveRequestStatus { get; set; } = string.Empty;
        public bool StatusChanged { get; set; }
        public bool BalanceActionAttempted { get; set; }
        public bool BalanceActionSucceeded { get; set; }
        public string? BalanceActionType { get; set; }
        public decimal ReservationBeforeDays { get; set; }
        public decimal ReservationAfterDays { get; set; }
        public decimal UsedBeforeDays { get; set; }
        public decimal UsedAfterDays { get; set; }
        public bool IsIdempotent { get; set; }
        public string? WarningMessage { get; set; }
    }

    public class LeaveRequestBalanceLifecycleResponse
    {
        public Guid LeaveRequestId { get; set; }
        public Guid? LeaveBalanceId { get; set; }
        public string ActionType { get; set; } = "None";
        public bool ActionAttempted { get; set; }
        public bool IsIdempotent { get; set; }
        public decimal ReservationBeforeDays { get; set; }
        public decimal ReservationAfterDays { get; set; }
        public decimal UsedBeforeDays { get; set; }
        public decimal UsedAfterDays { get; set; }
        public decimal AvailableBeforeDays { get; set; }
        public decimal AvailableAfterDays { get; set; }
        public Guid? BalanceTransactionId { get; set; }
    }
}
