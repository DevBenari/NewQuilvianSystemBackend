using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs
{
    public class ScheduleChangeFilterMetadataResponse
    {
        public List<string> RequestTypeOptions { get; set; } = new();
        public List<string> RequestStatusOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class ScheduleChangeSummaryResponse
    {
        public int TotalData { get; set; }
        public int Draft { get; set; }
        public int WaitingApproval { get; set; }
        public int NeedRevision { get; set; }
        public int Approved { get; set; }
        public int Applied { get; set; }
        public int Rejected { get; set; }
        public int Cancelled { get; set; }
    }

    public class ScheduleChangeListResponse
    {
        public Guid Id { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public string RequestType { get; set; } = string.Empty;
        public DateOnly RequestedDate { get; set; }
        public DateOnly EffectiveStartDate { get; set; }
        public DateOnly? EffectiveEndDate { get; set; }
        public string? CurrentScheduleName { get; set; }
        public string? RequestedScheduleName { get; set; }
        public string? CurrentShiftName { get; set; }
        public string? RequestedShiftName { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string RequestStatus { get; set; } = string.Empty;
        public Guid? WorkflowInstanceId { get; set; }
        public bool IsAppliedToRoster { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public bool CanEdit { get; set; }
        public bool CanSubmit { get; set; }
        public bool CanCancel { get; set; }
        public bool CanDelete { get; set; }
    }

    public class ScheduleChangeDetailResponse : ScheduleChangeListResponse
    {
        public Guid? WorkScheduleAssignmentId { get; set; }
        public Guid? RosterPeriodId { get; set; }
        public Guid? CurrentShiftAssignmentId { get; set; }
        public Guid? RequestedShiftAssignmentId { get; set; }
        public Guid? CurrentWorkScheduleId { get; set; }
        public Guid? RequestedWorkScheduleId { get; set; }
        public Guid? CurrentShiftId { get; set; }
        public Guid? RequestedShiftId { get; set; }
        public Guid? RequestReasonId { get; set; }
        public Guid? RejectionReasonId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public string? AttachmentPath { get; set; }
        public string? ApprovalNotes { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime? AppliedAt { get; set; }
    }

    public class ScheduleChangeValidationResponse
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public Guid? ResolvedCurrentWorkScheduleAssignmentId { get; set; }
        public Guid? ResolvedCurrentShiftAssignmentId { get; set; }
        public Guid? ResolvedCurrentWorkScheduleId { get; set; }
        public Guid? ResolvedCurrentShiftId { get; set; }
    }

    public class ScheduleChangeOptionResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateOnly? Date { get; set; }
        public string? AdditionalInfo { get; set; }
    }

    public class CreateScheduleChangeSelfServiceRequest
    {
        [Required, MaxLength(40)]
        public string RequestType { get; set; } = "ScheduleChange";
        public Guid? CurrentShiftAssignmentId { get; set; }
        public Guid? RequestedShiftAssignmentId { get; set; }
        public Guid? RequestedWorkScheduleId { get; set; }
        public Guid? RequestedShiftId { get; set; }
        public Guid? RequestReasonId { get; set; }
        [Required] public DateOnly RequestedDate { get; set; }
        [Required] public DateOnly EffectiveStartDate { get; set; }
        public DateOnly? EffectiveEndDate { get; set; }
        [Required, MaxLength(1000)] public string Reason { get; set; } = string.Empty;
        [MaxLength(500)] public string? AttachmentPath { get; set; }
    }

    public class UpdateScheduleChangeSelfServiceRequest : CreateScheduleChangeSelfServiceRequest
    {
    }

    public class ScheduleChangeSubmitRequest
    {
        [MaxLength(4000)] public string? Note { get; set; }
        [MaxLength(30)] public string? SourceChannel { get; set; }
        [MaxLength(100)] public string? RequestCorrelationId { get; set; }
        [MaxLength(100)] public string? IdempotencyKey { get; set; }
    }

    public class ScheduleChangeCancelRequest
    {
        [Required, MaxLength(1000)] public string Reason { get; set; } = string.Empty;
        [MaxLength(100)] public string? IdempotencyKey { get; set; }
    }

    public class ScheduleChangeApplyRequest
    {
        [MaxLength(1000)] public string? Notes { get; set; }
    }

    public class ScheduleChangeWorkflowResponse
    {
        public Guid ScheduleChangeRequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string RequestStatus { get; set; } = string.Empty;
        public bool HasWorkflow { get; set; }
        public bool IsSynchronized { get; set; }
        public bool IsAutoApplyPending { get; set; }
        public ScheduleChangeDetailResponse? ScheduleChange { get; set; }
        public WorkflowInstanceDetailResponse? Workflow { get; set; }
    }
}
