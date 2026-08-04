using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs
{
    public class ApprovalInboxStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class ApprovalInboxSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class ApprovalInboxDefaultFilterResponse
    {
        public string Period { get; set; } = "last30days";

        public string View { get; set; } = "open";

        public string SortBy { get; set; } = "dueAt";

        public string SortDirection { get; set; } = "asc";

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    public class ApprovalInboxFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public ApprovalInboxDefaultFilterResponse DefaultFilter { get; set; } = new();

        public List<ApprovalInboxStringOptionResponse> PeriodOptions { get; set; } = new();

        public List<ApprovalInboxStringOptionResponse> ViewOptions { get; set; } = new();

        public List<ApprovalInboxStringOptionResponse> AssignmentStatusOptions { get; set; } = new();

        public List<ApprovalInboxStringOptionResponse> StepTypeOptions { get; set; } = new();

        public List<ApprovalInboxStringOptionResponse> DueStatusOptions { get; set; } = new();

        public List<ApprovalInboxSortOptionResponse> SortOptions { get; set; } = new();

        public List<string> SortDirections { get; set; } = new();

        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class ApprovalInboxSummaryResponse
    {
        public int TotalAssigned { get; set; }

        public int Open { get; set; }

        public int Pending { get; set; }

        public int Available { get; set; }

        public int InProgress { get; set; }

        public int DueToday { get; set; }

        public int Overdue { get; set; }

        public int DelegatedToMe { get; set; }

        public int CompletedToday { get; set; }

        public int ApprovedToday { get; set; }

        public int RejectedToday { get; set; }
    }

    public class ApprovalInboxItemResponse
    {
        public Guid AssignmentId { get; set; }

        public Guid WorkflowInstanceId { get; set; }

        public Guid WorkflowStepInstanceId { get; set; }

        public Guid? ApprovalMatrixId { get; set; }

        public Guid? ApprovalDelegationId { get; set; }

        public string RequestNumber { get; set; } = string.Empty;

        public Guid WorkflowDefinitionId { get; set; }

        public string WorkflowCode { get; set; } = string.Empty;

        public string WorkflowName { get; set; } = string.Empty;

        public int WorkflowVersion { get; set; }

        public string ReferenceType { get; set; } = string.Empty;

        public Guid ReferenceId { get; set; }

        public string? ExternalReferenceNumber { get; set; }

        public string WorkflowStatus { get; set; } = string.Empty;

        public string SourceChannel { get; set; } = string.Empty;

        public Guid RequestedByUserId { get; set; }

        public Guid? RequestedByWorkforceProfileId { get; set; }

        public string? RequestedByProfileCode { get; set; }

        public string RequestedByName { get; set; } = string.Empty;

        public int StepOrder { get; set; }

        public string StepCode { get; set; } = string.Empty;

        public string StepName { get; set; } = string.Empty;

        public string StepType { get; set; } = string.Empty;

        public string ApprovalMode { get; set; } = string.Empty;

        public string StepStatus { get; set; } = string.Empty;

        public bool IsCurrentStep { get; set; }

        public string ApproverSource { get; set; } = string.Empty;

        public string? AssignedApproverRoleCode { get; set; }

        public int AssignmentOrder { get; set; }

        public string AssignmentStatus { get; set; } = string.Empty;

        public bool IsRequired { get; set; }

        public bool IsCurrentAssignment { get; set; }

        public bool IsDelegated { get; set; }

        public Guid AssignedApproverUserId { get; set; }

        public Guid? AssignedApproverWorkforceProfileId { get; set; }

        public string? AssignedApproverProfileCode { get; set; }

        public string AssignedApproverName { get; set; } = string.Empty;

        public Guid? OriginalApproverUserId { get; set; }

        public Guid? OriginalApproverWorkforceProfileId { get; set; }

        public string? OriginalApproverName { get; set; }

        public DateTime AssignedAt { get; set; }

        public DateTime? AvailableAt { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? DueAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public DateTime? LastActionAt { get; set; }

        public string DueStatus { get; set; } = string.Empty;

        public double? DueInHours { get; set; }

        public List<string> AvailableActions { get; set; } = new();
    }

    public class ApprovalInboxActionHistoryResponse
    {
        public Guid Id { get; set; }

        public string ActionType { get; set; } = string.Empty;

        public DateTime ActionAt { get; set; }

        public Guid? ActualActionByUserId { get; set; }

        public Guid? ActualActionByWorkforceProfileId { get; set; }

        public string ActualActionByName { get; set; } = string.Empty;

        public string? Comment { get; set; }

        public bool IsDelegated { get; set; }

        public bool IsSystemAction { get; set; }

        public string ActionSource { get; set; } = string.Empty;

        public Guid? ActionReasonId { get; set; }

        public string? ActionReasonCode { get; set; }

        public string? ActionReasonName { get; set; }

        public string? PreviousWorkflowStatus { get; set; }

        public string? ResultingWorkflowStatus { get; set; }

        public string? PreviousStepStatus { get; set; }

        public string? ResultingStepStatus { get; set; }
    }

    public class ApprovalInboxRejectionReasonOptionResponse
    {
        public Guid Id { get; set; }

        public string ReasonCode { get; set; } = string.Empty;

        public string ReasonName { get; set; } = string.Empty;

        public string? ReasonCategory { get; set; }

        public string RejectAction { get; set; } = string.Empty;

        public string? ReturnToStepCode { get; set; }

        public bool IsCommentRequired { get; set; }

        public bool IsAttachmentRequired { get; set; }

        public bool AllowResubmit { get; set; }
    }

    public class ApprovalInboxDetailResponse
    {
        public ApprovalInboxItemResponse Assignment { get; set; } = new();

        public string? StepInstructions { get; set; }

        public string? ApprovalMatrixCode { get; set; }

        public string? ApprovalMatrixName { get; set; }

        public WorkflowInstanceDetailResponse Workflow { get; set; } = new();

        public List<ApprovalInboxActionHistoryResponse> ActionHistory { get; set; } = new();

        public List<ApprovalInboxRejectionReasonOptionResponse> RejectionReasons { get; set; } = new();
    }
}
