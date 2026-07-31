using Microsoft.AspNetCore.Http;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs
{
    public class AttendanceCorrectionStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class AttendanceCorrectionFieldOptionResponse
    {
        public string FieldName { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public List<string> AllowedCorrectionTypes { get; set; } = new();
    }

    public class AttendanceCorrectionFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public AttendanceCorrectionDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<AttendanceCorrectionStringOptionResponse> CustomPeriods { get; set; } = new();
        public List<AttendanceCorrectionStringOptionResponse> CorrectionTypeOptions { get; set; } = new();
        public List<AttendanceCorrectionStringOptionResponse> RequestStatusOptions { get; set; } = new();
        public List<AttendanceCorrectionStringOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<AttendanceCorrectionFieldOptionResponse> EditableFields { get; set; } = new();
    }

    public class AttendanceCorrectionDefaultFilterResponse
    {
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public Guid? AttendanceDailyId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public string? CorrectionType { get; set; }
        public string? RequestStatus { get; set; }
        public bool? HasEvidence { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "createDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class AttendanceCorrectionQueryRequest : AttendanceCorrectionDefaultFilterResponse
    {
    }

    public class AttendanceCorrectionSummaryResponse
    {
        public int TotalRequest { get; set; }
        public int DraftRequest { get; set; }
        public int SubmittedRequest { get; set; }
        public int UnderReviewRequest { get; set; }
        public int NeedRevisionRequest { get; set; }
        public int ApprovedRequest { get; set; }
        public int RejectedRequest { get; set; }
        public int AppliedRequest { get; set; }
        public int CancelledRequest { get; set; }
        public int RequestWithEvidence { get; set; }
        public int RequestCreatedToday { get; set; }
    }

    public class AttendanceCorrectionListResponse
    {
        public Guid Id { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string? WorkforceProfileCode { get; set; }
        public string? WorkforceDisplayName { get; set; }
        public Guid? AttendanceDailyId { get; set; }
        public DateOnly AttendanceDate { get; set; }
        public string CorrectionType { get; set; } = string.Empty;
        public string RequestStatus { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public Guid? RequestReasonId { get; set; }
        public string? RequestReasonCode { get; set; }
        public string? RequestReasonName { get; set; }
        public Guid? WorkflowInstanceId { get; set; }
        public string? WorkflowRequestNumber { get; set; }
        public string? WorkflowStatus { get; set; }
        public int DetailCount { get; set; }
        public int LinkedExceptionCount { get; set; }
        public bool HasEvidence { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime? AppliedAt { get; set; }
        public DateTime CreateDateTime { get; set; }
        public string? RequestedByUserName { get; set; }
    }

    public class AttendanceCorrectionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<AttendanceCorrectionListResponse> Items { get; set; } = new();
    }

    public class AttendanceCorrectionDetailItemResponse
    {
        public Guid Id { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string FieldLabel { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string? OriginalValue { get; set; }
        public string? RequestedValue { get; set; }
        public string? ApprovedValue { get; set; }
        public string DetailStatus { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public bool IsApplied { get; set; }
        public DateTime? AppliedAt { get; set; }
        public int SortOrder { get; set; }
    }

    public class AttendanceCorrectionExceptionResponse
    {
        public Guid Id { get; set; }
        public string ExceptionCode { get; set; } = string.Empty;
        public string ExceptionType { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string ExceptionStatus { get; set; } = string.Empty;
        public string? Message { get; set; }
        public bool IsPayrollBlocking { get; set; }
        public DateTime DetectedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolutionNote { get; set; }
    }

    public class AttendanceCorrectionEvidenceResponse
    {
        public bool HasEvidence { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public string? DownloadUrl { get; set; }
    }

    public class AttendanceCorrectionWorkflowLinkResponse
    {
        public Guid AttendanceCorrectionRequestId { get; set; }
        public string AttendanceCorrectionRequestNumber { get; set; } = string.Empty;
        public string AttendanceCorrectionStatus { get; set; } = string.Empty;
        public bool HasWorkflow { get; set; }
        public bool IsSynchronized { get; set; }
        public bool IsAutoApplyPending { get; set; }
        public WorkflowInstanceDetailResponse? Workflow { get; set; }
    }

    public class AttendanceCorrectionDetailResponse : AttendanceCorrectionListResponse
    {
        public Guid? RequestedByWorkforceProfileId { get; set; }
        public Guid? RequestedByUserId { get; set; }
        public string? OriginalSummaryJson { get; set; }
        public string? RequestedSummaryJson { get; set; }
        public string? ApprovedSummaryJson { get; set; }
        public string? FinalNote { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public bool CanEdit { get; set; }
        public bool CanSubmit { get; set; }
        public bool CanCancel { get; set; }
        public bool CanDelete { get; set; }
        public bool CanUploadEvidence { get; set; }
        public bool CanApply { get; set; }
        public List<string> AvailableActions { get; set; } = new();
        public List<AttendanceCorrectionDetailItemResponse> Details { get; set; } = new();
        public List<AttendanceCorrectionExceptionResponse> Exceptions { get; set; } = new();
        public AttendanceCorrectionEvidenceResponse Evidence { get; set; } = new();
        public AttendanceCorrectionWorkflowLinkResponse? WorkflowLink { get; set; }
    }

    public class AttendanceCorrectionDetailInputRequest
    {
        [Required, MaxLength(100)]
        public string FieldName { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? RequestedValue { get; set; }

        [MaxLength(1000)]
        public string? Reason { get; set; }
    }

    public class CreateAttendanceCorrectionRequest
    {
        public Guid AttendanceDailyId { get; set; }
        public Guid? RequestReasonId { get; set; }

        [Required, MaxLength(50)]
        public string CorrectionType { get; set; } = string.Empty;

        [Required, MaxLength(1500)]
        public string Reason { get; set; } = string.Empty;

        public List<Guid> ExceptionIds { get; set; } = new();

        [MinLength(1)]
        public List<AttendanceCorrectionDetailInputRequest> Details { get; set; } = new();
    }

    public class UpdateAttendanceCorrectionRequest
    {
        public Guid? RequestReasonId { get; set; }

        [Required, MaxLength(50)]
        public string CorrectionType { get; set; } = string.Empty;

        [Required, MaxLength(1500)]
        public string Reason { get; set; } = string.Empty;

        public List<Guid> ExceptionIds { get; set; } = new();

        [MinLength(1)]
        public List<AttendanceCorrectionDetailInputRequest> Details { get; set; } = new();
    }

    public class AttendanceCorrectionCreateResponse
    {
        public Guid Id { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string RequestStatus { get; set; } = string.Empty;
        public Guid AttendanceDailyId { get; set; }
        public DateOnly AttendanceDate { get; set; }
    }

    public class AttendanceCorrectionSubmitRequest
    {
        [MaxLength(4000)]
        public string? Comment { get; set; }

        [MaxLength(30)]
        public string SourceChannel { get; set; } = "Web";

        [MaxLength(100)]
        public string? RequestCorrelationId { get; set; }

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }

        public List<Guid> SelectedApproverUserIds { get; set; } = new();
    }

    public class AttendanceCorrectionCancelRequest
    {
        [Required, MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }
    }

    public class AttendanceCorrectionWorkflowResponse
    {
        public Guid AttendanceCorrectionRequestId { get; set; }
        public string AttendanceCorrectionRequestNumber { get; set; } = string.Empty;
        public string AttendanceCorrectionStatus { get; set; } = string.Empty;
        public Guid? WorkflowInstanceId { get; set; }
        public string? WorkflowRequestNumber { get; set; }
        public string? WorkflowStatus { get; set; }
        public string? CurrentStepCode { get; set; }
        public int CurrentStepOrder { get; set; }
        public bool IsSynchronized { get; set; }
        public bool IsAutoApplyPending { get; set; }
        public WorkflowInstanceDetailResponse? Workflow { get; set; }
    }

    public class AttendanceCorrectionSynchronizationResponse
    {
        public Guid AttendanceCorrectionRequestId { get; set; }
        public Guid WorkflowInstanceId { get; set; }
        public string PreviousAttendanceCorrectionStatus { get; set; } = string.Empty;
        public string CurrentAttendanceCorrectionStatus { get; set; } = string.Empty;
        public string WorkflowStatus { get; set; } = string.Empty;
        public bool StatusChanged { get; set; }
        public bool AutoApplyAttempted { get; set; }
        public bool AutoApplySucceeded { get; set; }
        public string? WarningMessage { get; set; }
    }

    public class AttendanceCorrectionApplyRequest
    {
        [MaxLength(1000)]
        public string? Note { get; set; }
    }

    public class AttendanceCorrectionApplyResponse
    {
        public Guid AttendanceCorrectionRequestId { get; set; }
        public Guid AttendanceDailyId { get; set; }
        public string PreviousRequestStatus { get; set; } = string.Empty;
        public string CurrentRequestStatus { get; set; } = string.Empty;
        public string AttendanceStatus { get; set; } = string.Empty;
        public int AppliedDetailCount { get; set; }
        public int ClosedExceptionCount { get; set; }
        public DateTime AppliedAt { get; set; }
        public string? WarningMessage { get; set; }
    }

    public class AttendanceCorrectionEvidenceUploadRequest
    {
        [Required]
        public IFormFile File { get; set; } = null!;
    }
}
