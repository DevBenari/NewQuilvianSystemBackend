using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs
{
    public class AttendancePayrollHandoffStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class AttendancePayrollHandoffSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class AttendancePayrollHandoffDefaultFilterResponse
    {
        public Guid? WorkforceProfileId { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? AttendanceStatus { get; set; }
        public string? PayrollInputStatus { get; set; }
        public string? ReadinessStatus { get; set; }
        public bool? IsCorrected { get; set; }
        public bool? IsLocked { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "attendanceDate";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class AttendancePayrollHandoffQueryRequest : AttendancePayrollHandoffDefaultFilterResponse
    {
    }

    public class AttendancePayrollHandoffFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public int MaximumItemPerExecution { get; set; }
        public AttendancePayrollHandoffDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<AttendancePayrollHandoffStringOptionResponse> ReadinessStatusOptions { get; set; } = new();
        public List<AttendancePayrollHandoffStringOptionResponse> AttendanceStatusOptions { get; set; } = new();
        public List<AttendancePayrollHandoffStringOptionResponse> PayrollInputStatusOptions { get; set; } = new();
        public List<AttendancePayrollHandoffSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<string> TerminalPayrollRunStatuses { get; set; } = new();
        public string HandoffRuleInfo { get; set; } = string.Empty;
        public string LockRuleInfo { get; set; } = string.Empty;
    }

    public class AttendancePayrollRunOptionResponse
    {
        public Guid Id { get; set; }
        public Guid PayrollPeriodId { get; set; }
        public string RunNumber { get; set; } = string.Empty;
        public string RunStatus { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
        public string PayrollPeriodCode { get; set; } = string.Empty;
        public string PayrollPeriodName { get; set; } = string.Empty;
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public string PayrollPeriodStatus { get; set; } = string.Empty;
    }

    public class AttendancePayrollHandoffSummaryResponse
    {
        public Guid PayrollRunId { get; set; }
        public string RunNumber { get; set; } = string.Empty;
        public string RunStatus { get; set; } = string.Empty;
        public bool IsPayrollRunLocked { get; set; }
        public Guid PayrollPeriodId { get; set; }
        public string PayrollPeriodCode { get; set; } = string.Empty;
        public string PayrollPeriodName { get; set; } = string.Empty;
        public DateOnly PeriodStartDate { get; set; }
        public DateOnly PeriodEndDate { get; set; }
        public string PayrollPeriodStatus { get; set; } = string.Empty;
        public bool IsPayrollPeriodLocked { get; set; }
        public int TotalAttendanceDaily { get; set; }
        public int ReadyForHandoff { get; set; }
        public int AlreadyImported { get; set; }
        public int MissingPayrollProfile { get; set; }
        public int MissingPayrollRunEmployee { get; set; }
        public int UnprocessedAttendance { get; set; }
        public int PayrollBlockedAttendance { get; set; }
        public int LockedWithoutInput { get; set; }
        public int ExcludedAttendance { get; set; }
        public int PeriodMismatch { get; set; }
        public int CorrectedAttendance { get; set; }
        public int PayrollAttendanceInputCount { get; set; }
        public int DistinctImportedEmployeeCount { get; set; }
        public bool CanExecute { get; set; }
        public List<string> BlockingReasons { get; set; } = new();
    }

    public class AttendancePayrollHandoffReadinessReasonResponse
    {
        public string Code { get; set; } = string.Empty;
        public string Severity { get; set; } = "Warning";
        public string Message { get; set; } = string.Empty;
    }

    public class AttendancePayrollHandoffPreviewItemResponse
    {
        public Guid AttendanceDailyId { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public string? WorkforceProfileCode { get; set; }
        public string? WorkforceDisplayName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public DateOnly AttendanceDate { get; set; }
        public string AttendanceStatus { get; set; } = string.Empty;
        public string ProcessingStatus { get; set; } = string.Empty;
        public string PayrollInputStatus { get; set; } = string.Empty;
        public bool IsPayrollEligible { get; set; }
        public bool IsCorrected { get; set; }
        public bool IsLocked { get; set; }
        public int ScheduledWorkMinutes { get; set; }
        public int ActualWorkMinutes { get; set; }
        public int PayableWorkMinutes { get; set; }
        public int LateMinutes { get; set; }
        public int EarlyLeaveMinutes { get; set; }
        public int OvertimeMinutes { get; set; }
        public int ExceptionCount { get; set; }
        public int PayrollBlockingExceptionCount { get; set; }
        public bool HasPayrollProfile { get; set; }
        public bool HasPayrollRunEmployee { get; set; }
        public Guid? PayrollRunEmployeeId { get; set; }
        public bool HasExistingPayrollInput { get; set; }
        public Guid? PayrollAttendanceInputId { get; set; }
        public string ReadinessStatus { get; set; } = string.Empty;
        public bool IsReady { get; set; }
        public List<AttendancePayrollHandoffReadinessReasonResponse> Reasons { get; set; } = new();
    }

    public class AttendancePayrollHandoffPreviewPagedResponse
    {
        public Guid PayrollRunId { get; set; }
        public string RunNumber { get; set; } = string.Empty;
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<AttendancePayrollHandoffPreviewItemResponse> Items { get; set; } = new();
    }

    public class ExecuteAttendancePayrollHandoffRequest
    {
        public List<Guid>? AttendanceDailyIds { get; set; }

        public bool ForceRefreshExistingInput { get; set; } = false;

        public bool ContinueOnValidationError { get; set; } = true;

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class AttendancePayrollHandoffExecutionItemResponse
    {
        public Guid AttendanceDailyId { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public string? WorkforceProfileCode { get; set; }
        public string? WorkforceDisplayName { get; set; }
        public DateOnly AttendanceDate { get; set; }
        public Guid? PayrollRunEmployeeId { get; set; }
        public Guid? PayrollAttendanceInputId { get; set; }
        public bool Success { get; set; }
        public bool IsCreated { get; set; }
        public bool IsUpdated { get; set; }
        public bool IsIdempotent { get; set; }
        public string PreviousPayrollInputStatus { get; set; } = string.Empty;
        public string CurrentPayrollInputStatus { get; set; } = string.Empty;
        public string ResultStatus { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public List<AttendancePayrollHandoffReadinessReasonResponse> Reasons { get; set; } = new();
    }

    public class AttendancePayrollHandoffExecutionResponse
    {
        public Guid PayrollRunId { get; set; }
        public string RunNumber { get; set; } = string.Empty;
        public Guid PayrollPeriodId { get; set; }
        public string PayrollPeriodCode { get; set; } = string.Empty;
        public string HandoffStatus { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public DateTime CompletedAt { get; set; }
        public int TotalTarget { get; set; }
        public int CreatedCount { get; set; }
        public int UpdatedCount { get; set; }
        public int IdempotentCount { get; set; }
        public int FailedCount { get; set; }
        public int LockedAttendanceCount { get; set; }
        public List<AttendancePayrollHandoffExecutionItemResponse> Items { get; set; } = new();
    }

    public class AttendancePayrollHandoffReconciliationQueryRequest
    {
        public string? IssueType { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "attendanceDate";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class AttendancePayrollHandoffReconciliationItemResponse
    {
        public string IssueType { get; set; } = string.Empty;
        public string Severity { get; set; } = "Warning";
        public Guid? AttendanceDailyId { get; set; }
        public Guid? PayrollAttendanceInputId { get; set; }
        public Guid? PayrollRunEmployeeId { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public string? WorkforceProfileCode { get; set; }
        public string? WorkforceDisplayName { get; set; }
        public DateOnly? AttendanceDate { get; set; }
        public string? AttendanceStatus { get; set; }
        public string? AttendanceStatusSnapshot { get; set; }
        public bool? IsCorrectionApplied { get; set; }
        public bool? IsAttendanceCorrected { get; set; }
        public DateTime? ImportedAt { get; set; }
        public DateTime? AttendanceUpdatedAt { get; set; }
        public string Message { get; set; } = string.Empty;
        public string SuggestedAction { get; set; } = string.Empty;
    }

    public class AttendancePayrollHandoffReconciliationResponse
    {
        public Guid PayrollRunId { get; set; }
        public string RunNumber { get; set; } = string.Empty;
        public Guid PayrollPeriodId { get; set; }
        public string PayrollPeriodCode { get; set; } = string.Empty;
        public int ExpectedAttendanceCount { get; set; }
        public int PayrollAttendanceInputCount { get; set; }
        public int MatchedCount { get; set; }
        public int MissingInputCount { get; set; }
        public int ChangedAfterImportCount { get; set; }
        public int OrphanInputCount { get; set; }
        public int OutsidePeriodInputCount { get; set; }
        public bool IsBalanced { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalIssue { get; set; }
        public int TotalPage { get; set; }
        public List<AttendancePayrollHandoffReconciliationItemResponse> Items { get; set; } = new();
    }

    public class RepairAttendancePayrollHandoffRequest
    {
        public bool RefreshChangedInput { get; set; } = true;
        public bool CreateMissingInput { get; set; } = true;

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class RollbackAttendancePayrollHandoffRequest
    {
        public List<Guid>? AttendanceDailyIds { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }

    public class AttendancePayrollHandoffRollbackResponse
    {
        public Guid PayrollRunId { get; set; }
        public string RunNumber { get; set; } = string.Empty;
        public int RolledBackInputCount { get; set; }
        public int ReopenedAttendanceCount { get; set; }
        public int AttendanceStillReferencedCount { get; set; }
        public DateTime RolledBackAt { get; set; }
        public Guid RolledBackByUserId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
