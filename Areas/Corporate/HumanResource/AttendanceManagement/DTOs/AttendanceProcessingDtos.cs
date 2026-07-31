using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs
{
    public class AttendanceProcessingMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public int MaximumRangeDays { get; set; } = 31;
        public int MaximumWorkforcePerRequest { get; set; } = 500;
        public int MaximumProcessingItemPerRequest { get; set; } = 5000;
        public AttendanceProcessingDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<AttendanceProcessingStringOptionResponse> ProcessingModeOptions { get; set; } = new();
        public List<AttendanceProcessingStringOptionResponse> RunStatusOptions { get; set; } = new();
        public List<AttendanceProcessingStringOptionResponse> TriggerSourceOptions { get; set; } = new();
        public List<AttendanceProcessingStringOptionResponse> AttendanceStatusOptions { get; set; } = new();
        public List<AttendanceProcessingSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class AttendanceProcessingDefaultFilterResponse
    {
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? ProcessingMode { get; set; }
        public string? RunStatus { get; set; }
        public string? TriggerSource { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "startedAt";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class AttendanceProcessingStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class AttendanceProcessingSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class AttendanceProcessingSummaryResponse
    {
        public int TotalRun { get; set; }
        public int RunningRun { get; set; }
        public int CompletedRun { get; set; }
        public int CompletedWithErrorsRun { get; set; }
        public int FailedRun { get; set; }
        public int CancelledRun { get; set; }
        public int TotalTarget { get; set; }
        public int TotalSuccess { get; set; }
        public int TotalFailed { get; set; }
        public int TotalSkipped { get; set; }
        public int ProcessedToday { get; set; }
        public int ErrorToday { get; set; }
    }

    public class AttendanceProcessingRunQueryRequest
    {
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? ProcessingMode { get; set; }
        public string? RunStatus { get; set; }
        public string? TriggerSource { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "startedAt";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class AttendanceProcessingRunResponse
    {
        public Guid Id { get; set; }
        public string RunNumber { get; set; } = string.Empty;
        public string ProcessingMode { get; set; } = string.Empty;
        public string RunStatus { get; set; } = string.Empty;
        public string TriggerSource { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public Guid? TargetWorkforceProfileId { get; set; }
        public string? TargetWorkforceProfileCode { get; set; }
        public string? TargetWorkforceDisplayName { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public string? HospitalSiteName { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int ProcessingVersion { get; set; }
        public int TargetCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public int SkippedCount { get; set; }
        public decimal CompletionPercentage { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public Guid? TriggeredByUserId { get; set; }
        public string? TriggeredByUserName { get; set; }
        public string? CorrelationId { get; set; }
        public string? ErrorSummary { get; set; }
        public string? Notes { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class AttendanceProcessingRunDetailResponse : AttendanceProcessingRunResponse
    {
        public string? ParametersJson { get; set; }
        public Guid? CancelledByUserId { get; set; }
        public string? CancelledByUserName { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class AttendanceProcessingRunPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<AttendanceProcessingRunResponse> Items { get; set; } = new();
    }

    public class ProcessAttendanceSingleRequest
    {
        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public DateOnly WorkDate { get; set; }

        public bool ForceReprocess { get; set; }

        [MaxLength(30)]
        public string TriggerSource { get; set; } = "Api";

        [MaxLength(100)]
        public string? CorrelationId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class ProcessAttendanceRangeRequest
    {
        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }

        public Guid? WorkforceProfileId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public bool ForceReprocess { get; set; }

        [MaxLength(30)]
        public string TriggerSource { get; set; } = "Api";

        [MaxLength(100)]
        public string? CorrelationId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class ReprocessAttendanceDailyRequest
    {
        [MaxLength(100)]
        public string? CorrelationId { get; set; }

        [MaxLength(1000)]
        public string? Reason { get; set; }
    }

    public class AttendanceProcessingExecutionResponse
    {
        public Guid ProcessingRunId { get; set; }
        public string RunNumber { get; set; } = string.Empty;
        public string RunStatus { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int TargetCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public int SkippedCount { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? ErrorSummary { get; set; }
        public List<AttendanceProcessingItemResponse> Items { get; set; } = new();
    }

    public class AttendanceProcessingItemResponse
    {
        public Guid WorkforceProfileId { get; set; }
        public string? WorkforceProfileCode { get; set; }
        public string? WorkforceDisplayName { get; set; }
        public DateOnly WorkDate { get; set; }
        public bool Success { get; set; }
        public bool IsSkipped { get; set; }
        public bool IsCreated { get; set; }
        public bool IsReprocessed { get; set; }
        public Guid? AttendanceDailyId { get; set; }
        public string AttendanceStatus { get; set; } = string.Empty;
        public string ProcessingStatus { get; set; } = string.Empty;
        public string ScheduleSource { get; set; } = string.Empty;
        public DateTime? ScheduledCheckInAt { get; set; }
        public DateTime? ScheduledCheckOutAt { get; set; }
        public DateTime? FirstCheckInAt { get; set; }
        public DateTime? LastCheckOutAt { get; set; }
        public int RawLogCount { get; set; }
        public int SegmentCount { get; set; }
        public int ExceptionCount { get; set; }
        public int ActualWorkMinutes { get; set; }
        public int LateMinutes { get; set; }
        public int EarlyLeaveMinutes { get; set; }
        public int OvertimeMinutes { get; set; }
        public bool IsPayrollEligible { get; set; }
        public string PayrollInputStatus { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public List<string> ExceptionCodes { get; set; } = new();
    }
}
