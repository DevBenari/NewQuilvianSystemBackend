using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs
{
    public class WorkflowStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class WorkflowSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class WorkflowFilterDefaultResponse
    {
        public string Period { get; set; } = "last30days";

        public string SortBy { get; set; } = "createDateTime";

        public string SortDirection { get; set; } = "desc";

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    public class WorkflowFilterMetadataResponse
    {
        public WorkflowFilterDefaultResponse DefaultFilter { get; set; } = new();

        public List<WorkflowStringOptionResponse> PeriodOptions { get; set; } = new();

        public List<WorkflowStringOptionResponse> WorkflowStatusOptions { get; set; } = new();

        public List<WorkflowStringOptionResponse> StepStatusOptions { get; set; } = new();

        public List<WorkflowStringOptionResponse> AssignmentStatusOptions { get; set; } = new();

        public List<WorkflowStringOptionResponse> ActionTypeOptions { get; set; } = new();

        public List<WorkflowStringOptionResponse> SourceChannelOptions { get; set; } = new();

        public List<WorkflowSortOptionResponse> SortOptions { get; set; } = new();

        public List<string> SortDirections { get; set; } = new();

        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class WorkflowSummaryResponse
    {
        public int TotalData { get; set; }

        public int Draft { get; set; }

        public int Submitted { get; set; }

        public int InProgress { get; set; }

        public int RevisionRequested { get; set; }

        public int Returned { get; set; }

        public int Completed { get; set; }

        public int Rejected { get; set; }

        public int Cancelled { get; set; }

        public int Withdrawn { get; set; }
    }

    public class WorkflowInstanceListResponse
    {
        public Guid Id { get; set; }

        public string RequestNumber { get; set; } = string.Empty;

        public Guid WorkflowDefinitionId { get; set; }

        public string WorkflowCode { get; set; } = string.Empty;

        public string WorkflowName { get; set; } = string.Empty;

        public int WorkflowVersion { get; set; }

        public string ReferenceType { get; set; } = string.Empty;

        public Guid ReferenceId { get; set; }

        public string? ExternalReferenceNumber { get; set; }

        public Guid RequestedByUserId { get; set; }

        public Guid? RequestedByWorkforceProfileId { get; set; }

        public string? RequestedByProfileCode { get; set; }

        public string RequestedByName { get; set; } = string.Empty;

        public string WorkflowStatus { get; set; } = string.Empty;

        public int CurrentStepOrder { get; set; }

        public string? CurrentStepCode { get; set; }

        public string SourceChannel { get; set; } = string.Empty;

        public DateTime CreateDateTime { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public DateTime? LastActionAt { get; set; }

        public DateTime? CompletedAt { get; set; }
    }

    public class WorkflowRequesterResponse
    {
        public Guid UserId { get; set; }

        public Guid? WorkforceProfileId { get; set; }

        public Guid? EmployeeId { get; set; }

        public string? ProfileCode { get; set; }

        public string DisplayName { get; set; } = string.Empty;
    }

    public class WorkflowOrganizationSnapshotResponse
    {
        public Guid? OrganizationAssignmentId { get; set; }

        public Guid? LegalEntityId { get; set; }

        public Guid? HospitalSiteId { get; set; }

        public Guid? OrganizationUnitId { get; set; }

        public Guid? DepartmentId { get; set; }

        public Guid? CostCenterId { get; set; }
    }

    public class WorkflowApproverAssignmentResponse
    {
        public Guid Id { get; set; }

        public Guid WorkflowStepInstanceId { get; set; }

        public Guid? ApprovalMatrixId { get; set; }

        public Guid? ApprovalDelegationId { get; set; }

        public Guid AssignedApproverUserId { get; set; }

        public Guid? AssignedApproverWorkforceProfileId { get; set; }

        public string? AssignedApproverProfileCode { get; set; }

        public string AssignedApproverName { get; set; } = string.Empty;

        public Guid? OriginalApproverUserId { get; set; }

        public Guid? OriginalApproverWorkforceProfileId { get; set; }

        public string? OriginalApproverName { get; set; }

        public string? AssignedApproverRoleCode { get; set; }

        public string ApproverSource { get; set; } = string.Empty;

        public int AssignmentOrder { get; set; }

        public string AssignmentStatus { get; set; } = string.Empty;

        public DateTime AssignedAt { get; set; }

        public DateTime? AvailableAt { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? DueAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public bool IsRequired { get; set; }

        public bool IsCurrentAssignment { get; set; }

        public bool IsDelegated { get; set; }

        public List<string> AvailableActions { get; set; } = new();
    }

    public class WorkflowStepInstanceResponse
    {
        public Guid Id { get; set; }

        public Guid WorkflowStepId { get; set; }

        public Guid? ApprovalMatrixId { get; set; }

        public int StepOrder { get; set; }

        public string StepCode { get; set; } = string.Empty;

        public string StepName { get; set; } = string.Empty;

        public string StepType { get; set; } = string.Empty;

        public string ApprovalMode { get; set; } = string.Empty;

        public string ApproverSource { get; set; } = string.Empty;

        public int RequiredApprovalCount { get; set; }

        public decimal? RequiredApprovalPercentage { get; set; }

        public int TotalAssignmentCount { get; set; }

        public int ApprovedActionCount { get; set; }

        public int RejectedActionCount { get; set; }

        public string StepStatus { get; set; } = string.Empty;

        public DateTime? AvailableAt { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? DueAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? SkippedAt { get; set; }

        public bool IsCurrentStep { get; set; }

        public bool IsDelegationAllowed { get; set; }

        public bool IsAutoAction { get; set; }

        public string? Instructions { get; set; }

        public List<WorkflowApproverAssignmentResponse> Assignments { get; set; } = new();
    }

    public class WorkflowApprovalActionResponse
    {
        public Guid Id { get; set; }

        public Guid? WorkflowStepInstanceId { get; set; }

        public Guid? WorkflowApproverAssignmentId { get; set; }

        public Guid? ApprovalDelegationId { get; set; }

        public string ActionType { get; set; } = string.Empty;

        public DateTime ActionAt { get; set; }

        public Guid? ActualActionByUserId { get; set; }

        public Guid? ActualActionByWorkforceProfileId { get; set; }

        public string? ActualActionByName { get; set; }

        public bool IsDelegated { get; set; }

        public bool IsSystemAction { get; set; }

        public string ActionSource { get; set; } = string.Empty;

        public string? Comment { get; set; }

        public Guid? ActionReasonId { get; set; }

        public string? ActionReasonType { get; set; }

        public string? ActionReasonCode { get; set; }

        public string? ActionReasonName { get; set; }

        public string? PreviousWorkflowStatus { get; set; }

        public string? ResultingWorkflowStatus { get; set; }

        public string? PreviousStepStatus { get; set; }

        public string? ResultingStepStatus { get; set; }
    }

    public class WorkflowStatusHistoryResponse
    {
        public Guid Id { get; set; }

        public Guid? WorkflowStepInstanceId { get; set; }

        public int SequenceNumber { get; set; }

        public string? FromWorkflowStatus { get; set; }

        public string ToWorkflowStatus { get; set; } = string.Empty;

        public string? FromStepStatus { get; set; }

        public string? ToStepStatus { get; set; }

        public string ActionType { get; set; } = string.Empty;

        public DateTime ChangedAt { get; set; }

        public Guid? ChangedByUserId { get; set; }

        public Guid? ChangedByWorkforceProfileId { get; set; }

        public string? ChangedByName { get; set; }

        public string? Comment { get; set; }

        public bool IsSystemGenerated { get; set; }
    }

    public class WorkflowCommentResponse
    {
        public Guid Id { get; set; }

        public Guid? WorkflowStepInstanceId { get; set; }

        public Guid? ParentCommentId { get; set; }

        public string CommentType { get; set; } = string.Empty;

        public string CommentText { get; set; } = string.Empty;

        public DateTime CommentedAt { get; set; }

        public Guid? CommentByUserId { get; set; }

        public Guid? CommentByWorkforceProfileId { get; set; }

        public string? CommentByName { get; set; }

        public bool IsRequesterVisible { get; set; }

        public bool IsInternalComment { get; set; }

        public bool IsSystemGenerated { get; set; }
    }

    public class WorkflowAttachmentResponse
    {
        public Guid Id { get; set; }

        public Guid? WorkflowStepInstanceId { get; set; }

        public Guid? ApprovalActionId { get; set; }

        public Guid? WorkflowCommentId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string DownloadUrl { get; set; } = string.Empty;

        public string? ContentType { get; set; }

        public long FileSizeBytes { get; set; }

        public string? AttachmentCategory { get; set; }

        public string? Description { get; set; }

        public DateTime UploadedAt { get; set; }

        public string? UploadedByName { get; set; }

        public bool IsRequesterVisible { get; set; }

        public bool IsConfidential { get; set; }
    }

    public class WorkflowInstanceDetailResponse
    {
        public Guid Id { get; set; }

        public string RequestNumber { get; set; } = string.Empty;

        public Guid WorkflowDefinitionId { get; set; }

        public string WorkflowCode { get; set; } = string.Empty;

        public string WorkflowName { get; set; } = string.Empty;

        public int WorkflowVersion { get; set; }

        public string ReferenceType { get; set; } = string.Empty;

        public Guid ReferenceId { get; set; }

        public string? ExternalReferenceNumber { get; set; }

        public string WorkflowStatus { get; set; } = string.Empty;

        public int CurrentStepOrder { get; set; }

        public string? CurrentStepCode { get; set; }

        public string SourceChannel { get; set; } = string.Empty;

        public WorkflowRequesterResponse Requester { get; set; } = new();

        public WorkflowOrganizationSnapshotResponse Organization { get; set; } = new();

        public DateTime StartedAt { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public DateTime? DueAt { get; set; }

        public DateTime? LastActionAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? CancelledAt { get; set; }

        public DateTime? WithdrawnAt { get; set; }

        public string? RequestCorrelationId { get; set; }

        public string? IdempotencyKey { get; set; }

        public string? CompletionNote { get; set; }

        public string? CancellationReason { get; set; }

        public string? WorkflowDefinitionSnapshotJson { get; set; }

        public string? RequestContextJson { get; set; }

        public DateTime CreateDateTime { get; set; }

        public DateTime? UpdateDateTime { get; set; }

        public List<string> AvailableActions { get; set; } = new();

        public List<WorkflowStepInstanceResponse> Steps { get; set; } = new();

        public List<WorkflowApprovalActionResponse> ApprovalActions { get; set; } = new();

        public List<WorkflowStatusHistoryResponse> StatusHistories { get; set; } = new();

        public List<WorkflowCommentResponse> Comments { get; set; } = new();

        public List<WorkflowAttachmentResponse> Attachments { get; set; } = new();
    }

    public class CreateWorkflowInstanceRequest
    {
        [Required]
        [MaxLength(50)]
        public string WorkflowDefinitionCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string ReferenceType { get; set; } = string.Empty;

        public Guid ReferenceId { get; set; }

        [MaxLength(100)]
        public string? ExternalReferenceNumber { get; set; }

        [MaxLength(30)]
        public string SourceChannel { get; set; } = "Web";

        [MaxLength(100)]
        public string? RequestCorrelationId { get; set; }

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }

        public JsonElement? RequestContext { get; set; }

        public List<Guid> SelectedApproverUserIds { get; set; } = new();
    }

    public class WorkflowSubmitRequest
    {
        [MaxLength(4000)]
        public string? Comment { get; set; }

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }
    }

    public class WorkflowApproveRequest
    {
        [MaxLength(4000)]
        public string? Comment { get; set; }

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }
    }
}
