using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs
{
    public class ShiftSwapFilterMetadataResponse
    {
        public List<string> RequestStatusOptions { get; set; } = new();
        public List<string> ViewModeOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class ShiftSwapSummaryResponse
    {
        public int TotalData { get; set; }
        public int Draft { get; set; }
        public int WaitingTarget { get; set; }
        public int WaitingApproval { get; set; }
        public int NeedRevision { get; set; }
        public int Approved { get; set; }
        public int Applied { get; set; }
        public int Rejected { get; set; }
        public int Cancelled { get; set; }
    }

    public class ShiftSwapListResponse
    {
        public Guid Id { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public Guid RequesterWorkforceProfileId { get; set; }
        public string RequesterName { get; set; } = string.Empty;
        public Guid TargetWorkforceProfileId { get; set; }
        public string TargetName { get; set; } = string.Empty;
        public Guid RequesterShiftAssignmentId { get; set; }
        public Guid TargetShiftAssignmentId { get; set; }
        public DateOnly RequesterShiftDate { get; set; }
        public DateOnly TargetShiftDate { get; set; }
        public string? RequesterShiftName { get; set; }
        public string? TargetShiftName { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string RequestStatus { get; set; } = string.Empty;
        public bool? IsAcceptedByTarget { get; set; }
        public Guid? WorkflowInstanceId { get; set; }
        public bool IsAppliedToRoster { get; set; }
        public DateTime? RequestedAt { get; set; }
        public DateTime? TargetRespondedAt { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public bool IsRequester { get; set; }
        public bool IsTarget { get; set; }
        public bool CanEdit { get; set; }
        public bool CanSubmitToTarget { get; set; }
        public bool CanRespondAsTarget { get; set; }
        public bool CanCancel { get; set; }
        public bool CanDelete { get; set; }
    }

    public class ShiftSwapDetailResponse : ShiftSwapListResponse
    {
        public Guid? RosterPeriodId { get; set; }
        public Guid? RequestReasonId { get; set; }
        public Guid? RejectionReasonId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public string? AttachmentPath { get; set; }
        public string? TargetResponseNotes { get; set; }
        public string? ApprovalNotes { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime? AppliedAt { get; set; }
    }

    public class ShiftSwapValidationResponse
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public string? RequesterShiftSummary { get; set; }
        public string? TargetShiftSummary { get; set; }
    }


    public class ShiftSwapTargetOptionResponse
    {
        public Guid WorkforceProfileId { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? PositionName { get; set; }
    }

    public class ShiftSwapAssignmentOptionResponse
    {
        public Guid ShiftAssignmentId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceName { get; set; } = string.Empty;
        public DateOnly ShiftDate { get; set; }
        public Guid? ShiftId { get; set; }
        public string? ShiftName { get; set; }
        public DateTime ScheduledStartAt { get; set; }
        public DateTime ScheduledEndAt { get; set; }
    }

    public class CreateShiftSwapSelfServiceRequest
    {
        [Required] public Guid TargetWorkforceProfileId { get; set; }
        [Required] public Guid RequesterShiftAssignmentId { get; set; }
        [Required] public Guid TargetShiftAssignmentId { get; set; }
        public Guid? RequestReasonId { get; set; }
        [Required, MaxLength(1000)] public string Reason { get; set; } = string.Empty;
        [MaxLength(500)] public string? AttachmentPath { get; set; }
    }

    public class UpdateShiftSwapSelfServiceRequest : CreateShiftSwapSelfServiceRequest
    {
    }

    public class ShiftSwapSubmitToTargetRequest
    {
        [MaxLength(1000)] public string? Note { get; set; }
    }

    public class ShiftSwapTargetResponseRequest
    {
        public bool Accept { get; set; }
        [MaxLength(1000)] public string? Notes { get; set; }
    }

    public class ShiftSwapWorkflowSubmitRequest
    {
        [MaxLength(4000)] public string? Note { get; set; }
        [MaxLength(30)] public string? SourceChannel { get; set; }
        [MaxLength(100)] public string? RequestCorrelationId { get; set; }
        [MaxLength(100)] public string? IdempotencyKey { get; set; }
    }

    public class ShiftSwapCancelRequest
    {
        [Required, MaxLength(1000)] public string Reason { get; set; } = string.Empty;
        [MaxLength(100)] public string? IdempotencyKey { get; set; }
    }

    public class ShiftSwapApplyRequest
    {
        [MaxLength(1000)] public string? Notes { get; set; }
    }

    public class ShiftSwapWorkflowResponse
    {
        public Guid ShiftSwapRequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string RequestStatus { get; set; } = string.Empty;
        public bool HasWorkflow { get; set; }
        public bool IsSynchronized { get; set; }
        public bool IsAutoApplyPending { get; set; }
        public ShiftSwapDetailResponse? ShiftSwap { get; set; }
        public WorkflowInstanceDetailResponse? Workflow { get; set; }
    }
}
