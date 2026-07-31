using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs
{
    public class AttendanceCorrectionMonitoringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class AttendanceCorrectionMonitoringFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public AttendanceCorrectionMonitoringDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<AttendanceCorrectionMonitoringOptionResponse> CustomPeriods { get; set; } = new();
        public List<AttendanceCorrectionMonitoringOptionResponse> CorrectionTypeOptions { get; set; } = new();
        public List<AttendanceCorrectionMonitoringOptionResponse> RequestStatusOptions { get; set; } = new();
        public List<AttendanceCorrectionMonitoringOptionResponse> WorkflowStatusOptions { get; set; } = new();
        public List<AttendanceCorrectionMonitoringOptionResponse> MonitoringStatusOptions { get; set; } = new();
        public List<AttendanceCorrectionMonitoringOptionResponse> DueStatusOptions { get; set; } = new();
        public List<AttendanceCorrectionMonitoringOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class AttendanceCorrectionMonitoringDefaultFilterResponse
    {
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? CustomPeriod { get; set; } = "thismonth";
        public Guid? WorkforceProfileId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? CorrectionType { get; set; }
        public string? RequestStatus { get; set; }
        public string? WorkflowStatus { get; set; }
        public string? MonitoringStatus { get; set; }
        public string? DueStatus { get; set; }
        public bool? HasWorkflow { get; set; }
        public bool? HasEvidence { get; set; }
        public bool? IsPayrollBlocking { get; set; }
        public bool? IsSynchronized { get; set; }
        public bool? IsAutoApplyPending { get; set; }
        public bool? RequiresAttention { get; set; }
        public int StaleAfterHours { get; set; } = 24;
        public string? Search { get; set; }
        public string SortBy { get; set; } = "createDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class AttendanceCorrectionMonitoringQueryRequest : AttendanceCorrectionMonitoringDefaultFilterResponse
    {
    }

    public class AttendanceCorrectionMonitoringSummaryResponse
    {
        public int TotalRequest { get; set; }
        public int DraftRequest { get; set; }
        public int WaitingApproval { get; set; }
        public int NeedRevision { get; set; }
        public int ApprovedPendingApply { get; set; }
        public int Applied { get; set; }
        public int Rejected { get; set; }
        public int Cancelled { get; set; }
        public int MissingWorkflow { get; set; }
        public int WorkflowMismatch { get; set; }
        public int AutoApplyPending { get; set; }
        public int StaleRequest { get; set; }
        public int OverdueApproval { get; set; }
        public int PayrollBlocking { get; set; }
        public int RequiresAttention { get; set; }
        public int CreatedToday { get; set; }
    }

    public class AttendanceCorrectionMonitoringIssueResponse
    {
        public string Code { get; set; } = string.Empty;
        public string Severity { get; set; } = "Warning";
        public string Message { get; set; } = string.Empty;
        public string? SuggestedAction { get; set; }
    }

    public class AttendanceCorrectionMonitoringListResponse
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
        public bool HasEvidence { get; set; }
        public int DetailCount { get; set; }
        public int LinkedExceptionCount { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime? AppliedAt { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public string? RequestedByUserName { get; set; }

        public Guid? HospitalSiteId { get; set; }
        public string? HospitalSiteName { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }

        public string? AttendanceStatus { get; set; }
        public string? AttendanceProcessingStatus { get; set; }
        public string? PayrollInputStatus { get; set; }
        public bool IsAttendanceLocked { get; set; }
        public bool IsAttendanceCorrected { get; set; }
        public int PayrollBlockingExceptionCount { get; set; }

        public bool HasWorkflow { get; set; }
        public Guid? WorkflowInstanceId { get; set; }
        public string? WorkflowRequestNumber { get; set; }
        public string? WorkflowStatus { get; set; }
        public string? CurrentStepCode { get; set; }
        public int CurrentStepOrder { get; set; }
        public DateTime? WorkflowSubmittedAt { get; set; }
        public DateTime? WorkflowDueAt { get; set; }
        public DateTime? WorkflowLastActionAt { get; set; }
        public DateTime? WorkflowCompletedAt { get; set; }

        public int OpenAssignmentCount { get; set; }
        public int OverdueAssignmentCount { get; set; }
        public bool IsSynchronized { get; set; }
        public bool IsAutoApplyPending { get; set; }
        public bool IsStale { get; set; }
        public bool RequiresAttention { get; set; }
        public string MonitoringStatus { get; set; } = string.Empty;
        public string DueStatus { get; set; } = "NoDueDate";
        public int AgeHours { get; set; }
        public List<string> AttentionReasonCodes { get; set; } = new();
    }

    public class AttendanceCorrectionMonitoringPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<AttendanceCorrectionMonitoringListResponse> Items { get; set; } = new();
    }

    public class AttendanceCorrectionMonitoringAssignmentResponse
    {
        public Guid AssignmentId { get; set; }
        public Guid WorkflowStepInstanceId { get; set; }
        public int StepOrder { get; set; }
        public string StepCode { get; set; } = string.Empty;
        public string StepName { get; set; } = string.Empty;
        public string StepType { get; set; } = string.Empty;
        public string AssignmentStatus { get; set; } = string.Empty;
        public int AssignmentOrder { get; set; }
        public Guid AssignedApproverUserId { get; set; }
        public string? AssignedApproverName { get; set; }
        public Guid? OriginalApproverUserId { get; set; }
        public string? OriginalApproverName { get; set; }
        public bool IsDelegated { get; set; }
        public bool IsCurrentAssignment { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime? AvailableAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? DueAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string DueStatus { get; set; } = "NoDueDate";
    }

    public class AttendanceCorrectionMonitoringStatusHistoryResponse
    {
        public Guid Id { get; set; }
        public int SequenceNumber { get; set; }
        public string? FromWorkflowStatus { get; set; }
        public string ToWorkflowStatus { get; set; } = string.Empty;
        public string? FromStepStatus { get; set; }
        public string? ToStepStatus { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
        public string? ChangedByName { get; set; }
        public string? Comment { get; set; }
        public bool IsSystemGenerated { get; set; }
    }

    public class AttendanceCorrectionMonitoringWorkflowHealthResponse
    {
        public Guid AttendanceCorrectionRequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string RequestStatus { get; set; } = string.Empty;
        public Guid? WorkflowInstanceId { get; set; }
        public string? WorkflowRequestNumber { get; set; }
        public string? WorkflowStatus { get; set; }
        public bool HasWorkflow { get; set; }
        public bool IsSynchronized { get; set; }
        public bool IsAutoApplyPending { get; set; }
        public bool IsStale { get; set; }
        public bool RequiresAttention { get; set; }
        public string MonitoringStatus { get; set; } = string.Empty;
        public int OpenAssignmentCount { get; set; }
        public int OverdueAssignmentCount { get; set; }
        public List<AttendanceCorrectionMonitoringIssueResponse> Issues { get; set; } = new();
        public List<AttendanceCorrectionMonitoringAssignmentResponse> Assignments { get; set; } = new();
        public List<AttendanceCorrectionMonitoringStatusHistoryResponse> StatusHistories { get; set; } = new();
    }

    public class AttendanceCorrectionMonitoringDetailResponse
    {
        public AttendanceCorrectionDetailResponse Correction { get; set; } = new();
        public AttendanceCorrectionMonitoringListResponse Monitoring { get; set; } = new();
        public AttendanceCorrectionWorkflowLinkResponse? WorkflowLink { get; set; }
        public List<AttendanceCorrectionMonitoringIssueResponse> Issues { get; set; } = new();
        public List<AttendanceCorrectionMonitoringAssignmentResponse> Assignments { get; set; } = new();
        public List<AttendanceCorrectionMonitoringStatusHistoryResponse> StatusHistories { get; set; } = new();
        public List<string> AvailableAdminActions { get; set; } = new();
    }

    public class AttendanceCorrectionMonitoringBatchRequest
    {
        [MinLength(1), MaxLength(100)]
        public List<Guid> AttendanceCorrectionRequestIds { get; set; } = new();

        [MaxLength(1000)]
        public string? Note { get; set; }
    }

    public class AttendanceCorrectionMonitoringOperationItemResponse
    {
        public Guid AttendanceCorrectionRequestId { get; set; }
        public string? RequestNumber { get; set; }
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? PreviousRequestStatus { get; set; }
        public string? CurrentRequestStatus { get; set; }
        public string? WorkflowStatus { get; set; }
        public bool? AutoApplyAttempted { get; set; }
        public bool? AutoApplySucceeded { get; set; }
    }

    public class AttendanceCorrectionMonitoringBatchResponse
    {
        public int TotalItem { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<AttendanceCorrectionMonitoringOperationItemResponse> Items { get; set; } = new();
    }
}
