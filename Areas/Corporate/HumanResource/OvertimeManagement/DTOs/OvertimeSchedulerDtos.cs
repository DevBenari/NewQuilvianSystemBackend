using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs
{
    public class OvertimeSchedulerJobQueryRequest
    {
        public string? Search { get; set; }
        public string? JobType { get; set; }
        public string? JobStatus { get; set; }
        public Guid? OvertimePeriodId { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? SortBy { get; set; } = "scheduledAt";
        public string? SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class EnqueueOvertimeSchedulerJobRequest
    {
        [Required, MaxLength(40)]
        public string JobType { get; set; } = "FullCycle";

        public Guid? OvertimePeriodId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public bool AllowRepair { get; set; } = false;
        public bool ForceRecalculate { get; set; } = false;
        public int Priority { get; set; } = 100;
        public int? MaxRetryCount { get; set; }
        public DateTime? AvailableAt { get; set; }

        [MaxLength(120)]
        public string? CorrelationId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class EnqueueOvertimePeriodJobRequest
    {
        [Required, MaxLength(40)]
        public string JobType { get; set; } = "FullCycle";
        public bool AllowRepair { get; set; } = false;
        public bool ForceRecalculate { get; set; } = false;
        public int Priority { get; set; } = 100;

        [MaxLength(120)]
        public string? CorrelationId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class CancelOvertimeSchedulerJobRequest
    {
        [Required, MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }

    public class RetryOvertimeSchedulerJobRequest
    {
        public DateTime? AvailableAt { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class OvertimeSchedulerFilterMetadataResponse
    {
        public List<string> JobTypes { get; set; } = new();
        public List<string> JobStatuses { get; set; } = new();
        public List<string> SortFields { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class OvertimeSchedulerSummaryResponse
    {
        public int TotalJob { get; set; }
        public int Pending { get; set; }
        public int Running { get; set; }
        public int RetryScheduled { get; set; }
        public int Completed { get; set; }
        public int CompletedWithIssues { get; set; }
        public int Failed { get; set; }
        public int Cancelled { get; set; }
    }

    public class OvertimeSchedulerJobListResponse
    {
        public Guid Id { get; set; }
        public string JobNumber { get; set; } = string.Empty;
        public string JobType { get; set; } = string.Empty;
        public string JobStatus { get; set; } = string.Empty;
        public Guid? OvertimePeriodId { get; set; }
        public string? OvertimePeriodCode { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public bool AllowRepair { get; set; }
        public bool ForceRecalculate { get; set; }
        public int Priority { get; set; }
        public int RetryCount { get; set; }
        public int MaxRetryCount { get; set; }
        public DateTime ScheduledAt { get; set; }
        public DateTime AvailableAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? FailedAt { get; set; }
        public DateTime? NextRetryAt { get; set; }
        public string? CorrelationId { get; set; }
        public string? LastError { get; set; }
    }

    public class OvertimeSchedulerJobDetailResponse : OvertimeSchedulerJobListResponse
    {
        public DateTime? HeartbeatAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? WorkerInstanceId { get; set; }
        public Guid? TriggeredByUserId { get; set; }
        public string? TriggeredByUserName { get; set; }
        public Guid? CancelledByUserId { get; set; }
        public string? CancelledByUserName { get; set; }
        public string? ParametersJson { get; set; }
        public string? ResultJson { get; set; }
        public string? Notes { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class OvertimeSchedulerJobActionResponse
    {
        public Guid JobId { get; set; }
        public string JobNumber { get; set; } = string.Empty;
        public string PreviousStatus { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;
        public int RetryCount { get; set; }
        public DateTime ActionAt { get; set; }
    }

    public class OvertimeSchedulerExecutionResponse
    {
        public Guid JobId { get; set; }
        public string JobNumber { get; set; } = string.Empty;
        public string JobType { get; set; } = string.Empty;
        public int CandidateCount { get; set; }
        public int ProcessedCount { get; set; }
        public int SucceededCount { get; set; }
        public int SkippedCount { get; set; }
        public int FailedCount { get; set; }
        public int WarningCount { get; set; }
        public List<string> Messages { get; set; } = new();
        public OvertimeFinalReconciliationResponse? Reconciliation { get; set; }
    }
}
