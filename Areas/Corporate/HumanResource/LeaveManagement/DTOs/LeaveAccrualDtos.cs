using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs
{
    public class LeaveAccrualRunFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public LeaveAccrualRunDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<LeaveAccrualOptionResponse> RunStatuses { get; set; } = new();
        public List<LeaveAccrualOptionResponse> RunModes { get; set; } = new();
        public List<LeaveAccrualOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class LeaveAccrualRunDefaultFilterResponse
    {
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? RunStatus { get; set; }
        public string? RunMode { get; set; }
        public Guid? LeaveEntitlementPeriodId { get; set; }
        public Guid? LeaveTypeId { get; set; }
        public Guid? LeaveEntitlementPolicyId { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "createDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class LeaveAccrualOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class LeaveAccrualRunQueryRequest
    {
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public Guid? LeaveEntitlementPeriodId { get; set; }
        public Guid? LeaveTypeId { get; set; }
        public Guid? LeaveEntitlementPolicyId { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? RunStatus { get; set; }
        public string? RunMode { get; set; }
        public bool? IsDryRun { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "createDateTime";
        public string SortDirection { get; set; } = "desc";

        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 200)]
        public int PageSize { get; set; } = 25;
    }

    public class LeaveAccrualRunSummaryResponse
    {
        public int TotalRun { get; set; }
        public int DraftRun { get; set; }
        public int QueuedRun { get; set; }
        public int RunningRun { get; set; }
        public int CompletedRun { get; set; }
        public int CompletedWithErrorsRun { get; set; }
        public int FailedRun { get; set; }
        public int CancelledRun { get; set; }
        public int TotalTarget { get; set; }
        public int TotalPosted { get; set; }
        public int TotalSkipped { get; set; }
        public int TotalFailed { get; set; }
        public decimal TotalCalculatedDays { get; set; }
        public decimal TotalPostedDays { get; set; }
    }

    public class LeaveAccrualRunResponse
    {
        public Guid Id { get; set; }
        public string RunNumber { get; set; } = string.Empty;
        public string RunMode { get; set; } = string.Empty;
        public string RunStatus { get; set; } = string.Empty;
        public Guid LeaveEntitlementPeriodId { get; set; }
        public string? LeaveEntitlementPeriodCode { get; set; }
        public string? LeaveEntitlementPeriodName { get; set; }
        public Guid? LeaveTypeId { get; set; }
        public string? LeaveTypeCode { get; set; }
        public string? LeaveTypeName { get; set; }
        public Guid? LeaveEntitlementPolicyId { get; set; }
        public string? LeaveEntitlementPolicyCode { get; set; }
        public string? LeaveEntitlementPolicyName { get; set; }
        public DateOnly ScheduledAccrualDate { get; set; }
        public DateOnly AccrualPeriodStartDate { get; set; }
        public DateOnly AccrualPeriodEndDate { get; set; }
        public bool IsDryRun { get; set; }
        public bool ForceReprocess { get; set; }
        public int RetryCount { get; set; }
        public int MaximumRetryCount { get; set; }
        public int TargetCount { get; set; }
        public int CalculatedCount { get; set; }
        public int PostedCount { get; set; }
        public int SkippedCount { get; set; }
        public int FailedCount { get; set; }
        public decimal TotalCalculatedDays { get; set; }
        public decimal TotalPostedDays { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? ErrorSummary { get; set; }
        public string? CorrelationId { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class LeaveAccrualRunPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<LeaveAccrualRunResponse> Items { get; set; } = new();
    }

    public class LeaveAccrualRunDetailResponse : LeaveAccrualRunResponse
    {
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? TriggeredByUserId { get; set; }
        public string? TriggeredByName { get; set; }
        public Guid? CancelledByUserId { get; set; }
        public string? CancelledByName { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? Notes { get; set; }
        public string? ParametersJson { get; set; }
        public string? ResultSummaryJson { get; set; }
        public List<LeaveAccrualItemResponse> Accruals { get; set; } = new();
    }

    public class LeaveAccrualItemResponse
    {
        public Guid Id { get; set; }
        public string AccrualNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string? WorkforceProfileCode { get; set; }
        public string? WorkforceDisplayName { get; set; }
        public Guid LeaveTypeId { get; set; }
        public string? LeaveTypeName { get; set; }
        public Guid? LeaveBalanceId { get; set; }
        public Guid? LeaveEntitlementId { get; set; }
        public int AccrualSequence { get; set; }
        public DateOnly AccrualDate { get; set; }
        public DateOnly AccrualPeriodStartDate { get; set; }
        public DateOnly AccrualPeriodEndDate { get; set; }
        public decimal AccrualAmountDays { get; set; }
        public decimal BalanceBeforeAccrual { get; set; }
        public decimal BalanceAfterAccrual { get; set; }
        public bool IsProrated { get; set; }
        public string AccrualStatus { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime? PostedAt { get; set; }
    }

    public class LeaveAccrualPreviewRequest
    {
        [Required]
        public Guid LeaveEntitlementPeriodId { get; set; }

        public Guid? LeaveTypeId { get; set; }
        public Guid? LeaveEntitlementPolicyId { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }

        [Required]
        public DateOnly ScheduledAccrualDate { get; set; }

        [Required]
        public DateOnly AccrualPeriodStartDate { get; set; }

        [Required]
        public DateOnly AccrualPeriodEndDate { get; set; }

        public bool ForceReprocess { get; set; }

        [Range(1, 1000)]
        public int MaximumPreviewItem { get; set; } = 250;
    }

    public class LeaveAccrualPreviewResponse
    {
        public Guid LeaveEntitlementPeriodId { get; set; }
        public string? PeriodCode { get; set; }
        public DateOnly ScheduledAccrualDate { get; set; }
        public DateOnly AccrualPeriodStartDate { get; set; }
        public DateOnly AccrualPeriodEndDate { get; set; }
        public int TotalCandidate { get; set; }
        public int EligibleCount { get; set; }
        public int SkippedCount { get; set; }
        public decimal TotalCalculatedDays { get; set; }
        public bool IsTruncated { get; set; }
        public List<LeaveAccrualCandidateResponse> Items { get; set; } = new();
    }

    public class LeaveAccrualCandidateResponse
    {
        public Guid WorkforceProfileId { get; set; }
        public string? WorkforceProfileCode { get; set; }
        public string? WorkforceDisplayName { get; set; }
        public Guid LeaveTypeId { get; set; }
        public string? LeaveTypeCode { get; set; }
        public string? LeaveTypeName { get; set; }
        public Guid? LeavePolicyId { get; set; }
        public string? LeavePolicyCode { get; set; }
        public Guid? LeaveEntitlementPolicyId { get; set; }
        public string? LeaveEntitlementPolicyCode { get; set; }
        public Guid? LeaveEntitlementId { get; set; }
        public Guid? LeaveBalanceId { get; set; }
        public decimal CurrentRemainingDays { get; set; }
        public decimal CurrentAvailableDays { get; set; }
        public decimal CalculatedAccrualDays { get; set; }
        public int AccrualSequence { get; set; }
        public bool IsProrated { get; set; }
        public bool IsEligible { get; set; }
        public string ResultCode { get; set; } = string.Empty;
        public string ResultMessage { get; set; } = string.Empty;
        public string? CalculationDetailJson { get; set; }
    }

    public class CreateLeaveAccrualRunRequest : LeaveAccrualPreviewRequest
    {
        [MaxLength(30)]
        public string RunMode { get; set; } = "Manual";

        public bool IsDryRun { get; set; }
        public bool QueueForProcessing { get; set; } = true;

        [Range(0, 10)]
        public int MaximumRetryCount { get; set; } = 3;

        [MaxLength(150)]
        public string? IdempotencyKey { get; set; }

        [MaxLength(100)]
        public string? CorrelationId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class ExecuteLeaveAccrualRunRequest
    {
        public bool ForceReprocess { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class RetryLeaveAccrualRunRequest
    {
        [MaxLength(1000)]
        public string? Reason { get; set; }
    }

    public class CancelLeaveAccrualRunRequest
    {
        [Required, MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }

    public class EnqueueDueLeaveAccrualRequest
    {
        public DateOnly? AccrualDate { get; set; }
        public bool QueueForProcessing { get; set; } = true;
    }

    public class LeaveAccrualRunActionResponse
    {
        public Guid Id { get; set; }
        public string RunNumber { get; set; } = string.Empty;
        public string RunStatus { get; set; } = string.Empty;
        public bool IsIdempotent { get; set; }
        public int TargetCount { get; set; }
        public int PostedCount { get; set; }
        public int SkippedCount { get; set; }
        public int FailedCount { get; set; }
        public decimal TotalPostedDays { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class LeaveAccrualEnqueueResponse
    {
        public DateOnly AccrualDate { get; set; }
        public int EligiblePolicyCount { get; set; }
        public int CreatedRunCount { get; set; }
        public int ExistingRunCount { get; set; }
        public List<LeaveAccrualRunActionResponse> Runs { get; set; } = new();
    }

    public class LeaveAccrualReconciliationResponse
    {
        public Guid LeaveAccrualRunId { get; set; }
        public string RunNumber { get; set; } = string.Empty;
        public string RunStatus { get; set; } = string.Empty;
        public int RunPostedCount { get; set; }
        public int ActualPostedAccrualCount { get; set; }
        public int PostedLedgerCount { get; set; }
        public decimal RunTotalPostedDays { get; set; }
        public decimal ActualPostedAccrualDays { get; set; }
        public decimal LedgerAccruedDeltaDays { get; set; }
        public bool IsBalanced { get; set; }
        public List<LeaveAccrualReconciliationIssueResponse> Issues { get; set; } = new();
    }

    public class LeaveAccrualReconciliationIssueResponse
    {
        public string Code { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Guid? LeaveAccrualId { get; set; }
        public Guid? LeaveBalanceTransactionId { get; set; }
    }
}
