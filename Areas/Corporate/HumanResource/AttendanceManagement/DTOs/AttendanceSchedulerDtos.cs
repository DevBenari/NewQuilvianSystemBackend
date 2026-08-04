using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs
{
    public class AttendanceSchedulerMetadataResponse
    {
        public bool SchedulerEnabled { get; set; }
        public bool AutoClosePeriods { get; set; }
        public int PollIntervalSeconds { get; set; }
        public string TimeZoneId { get; set; } = string.Empty;
        public int DailyProcessingHour { get; set; }
        public int DailyProcessingMinute { get; set; }
        public int MaximumCatchUpDays { get; set; }
        public int RunningJobTimeoutMinutes { get; set; }
        public List<AttendanceStringOptionResponse> JobTypeOptions { get; set; } = new();
        public List<AttendanceStringOptionResponse> JobStatusOptions { get; set; } = new();
        public List<AttendanceSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class AttendanceSchedulerJobQueryRequest
    {
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? JobType { get; set; }
        public string? JobStatus { get; set; }
        public Guid? AttendancePeriodId { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "scheduledAt";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class AttendanceSchedulerSummaryResponse
    {
        public int TotalJob { get; set; }
        public int PendingJob { get; set; }
        public int RunningJob { get; set; }
        public int RetryScheduledJob { get; set; }
        public int CompletedJob { get; set; }
        public int CompletedWithErrorsJob { get; set; }
        public int FailedJob { get; set; }
        public int CancelledJob { get; set; }
        public int DueJob { get; set; }
    }

    public class AttendanceSchedulerJobResponse
    {
        public Guid Id { get; set; }
        public string JobNumber { get; set; } = string.Empty;
        public string JobType { get; set; } = string.Empty;
        public string JobStatus { get; set; } = string.Empty;
        public Guid? AttendancePeriodId { get; set; }
        public string? AttendancePeriodCode { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public string? WorkforceProfileCode { get; set; }
        public string? WorkforceDisplayName { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public string? HospitalSiteName { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public bool ForceReprocess { get; set; }
        public int Priority { get; set; }
        public int RetryCount { get; set; }
        public int MaxRetryCount { get; set; }
        public DateTime ScheduledAt { get; set; }
        public DateTime AvailableAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? FailedAt { get; set; }
        public DateTime? NextRetryAt { get; set; }
        public string? WorkerInstanceId { get; set; }
        public Guid? ProcessingRunId { get; set; }
        public string? ProcessingRunNumber { get; set; }
        public string? CorrelationId { get; set; }
        public string? LastError { get; set; }
        public string? Notes { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class AttendanceSchedulerJobDetailResponse : AttendanceSchedulerJobResponse
    {
        public string? ParametersJson { get; set; }
        public Guid? TriggeredByUserId { get; set; }
        public string? TriggeredByUserName { get; set; }
        public Guid? CancelledByUserId { get; set; }
        public string? CancelledByUserName { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime? HeartbeatAt { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class AttendanceSchedulerJobPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<AttendanceSchedulerJobResponse> Items { get; set; } = new();
    }

    public class EnqueueAttendanceProcessingJobRequest
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
        public int Priority { get; set; } = 100;
        public int? MaxRetryCount { get; set; }
        public DateTime? AvailableAt { get; set; }

        [MaxLength(100)]
        public string? CorrelationId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class EnqueueAttendancePeriodProcessingRequest
    {
        public bool ForceReprocess { get; set; }
        public int Priority { get; set; } = 50;

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class CancelAttendanceSchedulerJobRequest
    {
        [Required, MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }

    public class AttendanceSchedulerJobActionResponse
    {
        public Guid JobId { get; set; }
        public string JobNumber { get; set; } = string.Empty;
        public string PreviousStatus { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;
        public int RetryCount { get; set; }
        public DateTime ActionAt { get; set; }
    }
}
