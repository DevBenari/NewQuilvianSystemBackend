using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs
{
    public class LeaveCarryForwardOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class LeaveCarryForwardRunFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public LeaveCarryForwardRunDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<LeaveCarryForwardOptionResponse> RunStatuses { get; set; } = new();
        public List<LeaveCarryForwardOptionResponse> RunModes { get; set; } = new();
        public List<LeaveCarryForwardOptionResponse> CarryForwardStatuses { get; set; } = new();
        public List<LeaveCarryForwardOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class LeaveCarryForwardRunDefaultFilterResponse
    {
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? RunStatus { get; set; }
        public string? RunMode { get; set; }
        public Guid? SourceLeaveEntitlementPeriodId { get; set; }
        public Guid? DestinationLeaveEntitlementPeriodId { get; set; }
        public Guid? LeaveTypeId { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "createDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class LeaveCarryForwardRunQueryRequest
    {
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public Guid? SourceLeaveEntitlementPeriodId { get; set; }
        public Guid? DestinationLeaveEntitlementPeriodId { get; set; }
        public Guid? LeaveTypeId { get; set; }
        public Guid? LeaveCarryForwardPolicyId { get; set; }
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

    public class LeaveCarryForwardRunSummaryResponse
    {
        public int TotalRun { get; set; }
        public int DraftRun { get; set; }
        public int QueuedRun { get; set; }
        public int RunningRun { get; set; }
        public int CompletedRun { get; set; }
        public int CompletedWithErrorsRun { get; set; }
        public int FailedRun { get; set; }
        public int CancelledRun { get; set; }
        public int ReversedRun { get; set; }
        public int TotalTarget { get; set; }
        public int TotalPosted { get; set; }
        public int TotalSkipped { get; set; }
        public int TotalFailed { get; set; }
        public decimal TotalSourceAvailableDays { get; set; }
        public decimal TotalCarryForwardDays { get; set; }
        public decimal TotalExpiredDays { get; set; }
        public decimal TotalPayoutDays { get; set; }
    }

    public class LeaveCarryForwardRunResponse
    {
        public Guid Id { get; set; }
        public string RunNumber { get; set; } = string.Empty;
        public string RunMode { get; set; } = string.Empty;
        public string RunStatus { get; set; } = string.Empty;
        public Guid SourceLeaveEntitlementPeriodId { get; set; }
        public string? SourcePeriodCode { get; set; }
        public string? SourcePeriodName { get; set; }
        public Guid DestinationLeaveEntitlementPeriodId { get; set; }
        public string? DestinationPeriodCode { get; set; }
        public string? DestinationPeriodName { get; set; }
        public Guid? LeaveTypeId { get; set; }
        public string? LeaveTypeCode { get; set; }
        public string? LeaveTypeName { get; set; }
        public Guid? LeaveCarryForwardPolicyId { get; set; }
        public string? CarryForwardPolicyCode { get; set; }
        public string? CarryForwardPolicyName { get; set; }
        public DateOnly ExecutionDate { get; set; }
        public bool IsDryRun { get; set; }
        public bool ForceReprocess { get; set; }
        public int RetryCount { get; set; }
        public int MaximumRetryCount { get; set; }
        public int TargetCount { get; set; }
        public int CalculatedCount { get; set; }
        public int PostedCount { get; set; }
        public int SkippedCount { get; set; }
        public int FailedCount { get; set; }
        public decimal TotalSourceAvailableDays { get; set; }
        public decimal TotalEligibleDays { get; set; }
        public decimal TotalCarryForwardDays { get; set; }
        public decimal TotalExpiredDays { get; set; }
        public decimal TotalExcessDays { get; set; }
        public decimal TotalPayoutDays { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? CorrelationId { get; set; }
        public string? ErrorSummary { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class LeaveCarryForwardRunPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<LeaveCarryForwardRunResponse> Items { get; set; } = new();
    }

    public class LeaveCarryForwardRunDetailResponse : LeaveCarryForwardRunResponse
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
        public string? ParametersJson { get; set; }
        public string? ResultSummaryJson { get; set; }
        public string? Notes { get; set; }
        public List<LeaveCarryForwardItemResponse> CarryForwards { get; set; } = new();
    }

    public class LeaveCarryForwardItemResponse
    {
        public Guid Id { get; set; }
        public string CarryForwardNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string? WorkforceProfileCode { get; set; }
        public string? WorkforceDisplayName { get; set; }
        public Guid SourceLeaveTypeId { get; set; }
        public string? SourceLeaveTypeName { get; set; }
        public Guid DestinationLeaveTypeId { get; set; }
        public string? DestinationLeaveTypeName { get; set; }
        public Guid SourceLeaveBalanceId { get; set; }
        public Guid? DestinationLeaveBalanceId { get; set; }
        public decimal SourceAvailableDays { get; set; }
        public decimal EligibleDays { get; set; }
        public decimal CarryForwardDays { get; set; }
        public decimal ExpiredDays { get; set; }
        public decimal ExcessDays { get; set; }
        public decimal PayoutDays { get; set; }
        public DateOnly? CarryForwardExpiryDate { get; set; }
        public string CarryForwardStatus { get; set; } = string.Empty;
        public string? SkipReasonCode { get; set; }
        public string? SkipReason { get; set; }
        public DateTime? PostedAt { get; set; }
        public DateTime? ReversedAt { get; set; }
    }

    public class LeaveCarryForwardPreviewRequest
    {
        [Required]
        public Guid SourceLeaveEntitlementPeriodId { get; set; }

        [Required]
        public Guid DestinationLeaveEntitlementPeriodId { get; set; }

        public Guid? LeaveTypeId { get; set; }
        public Guid? LeaveCarryForwardPolicyId { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }

        [Required]
        public DateOnly ExecutionDate { get; set; }

        public bool ForceReprocess { get; set; }

        [Range(1, 1000)]
        public int MaximumPreviewItem { get; set; } = 250;
    }

    public class LeaveCarryForwardPreviewResponse
    {
        public Guid SourceLeaveEntitlementPeriodId { get; set; }
        public string? SourcePeriodCode { get; set; }
        public Guid DestinationLeaveEntitlementPeriodId { get; set; }
        public string? DestinationPeriodCode { get; set; }
        public DateOnly ExecutionDate { get; set; }
        public int TotalCandidate { get; set; }
        public int EligibleCount { get; set; }
        public int SkippedCount { get; set; }
        public decimal TotalSourceAvailableDays { get; set; }
        public decimal TotalCarryForwardDays { get; set; }
        public decimal TotalExpiredDays { get; set; }
        public decimal TotalPayoutDays { get; set; }
        public bool IsTruncated { get; set; }
        public List<LeaveCarryForwardCandidateResponse> Items { get; set; } = new();
    }

    public class LeaveCarryForwardCandidateResponse
    {
        public Guid WorkforceProfileId { get; set; }
        public string? WorkforceProfileCode { get; set; }
        public string? WorkforceDisplayName { get; set; }
        public Guid SourceLeaveBalanceId { get; set; }
        public Guid SourceLeaveTypeId { get; set; }
        public string? SourceLeaveTypeCode { get; set; }
        public string? SourceLeaveTypeName { get; set; }
        public Guid DestinationLeaveTypeId { get; set; }
        public string? DestinationLeaveTypeCode { get; set; }
        public string? DestinationLeaveTypeName { get; set; }
        public Guid LeaveCarryForwardPolicyId { get; set; }
        public string? CarryForwardPolicyCode { get; set; }
        public decimal SourceAvailableDays { get; set; }
        public decimal EligibleDays { get; set; }
        public decimal CarryForwardDays { get; set; }
        public decimal ExpiredDays { get; set; }
        public decimal ExcessDays { get; set; }
        public decimal PayoutDays { get; set; }
        public decimal RoundingAdjustmentDays { get; set; }
        public DateOnly? CarryForwardExpiryDate { get; set; }
        public bool IsEligible { get; set; }
        public string ResultCode { get; set; } = string.Empty;
        public string ResultMessage { get; set; } = string.Empty;
        public string? CalculationDetailJson { get; set; }
    }

    public class CreateLeaveCarryForwardRunRequest : LeaveCarryForwardPreviewRequest
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

    public class ExecuteLeaveCarryForwardRunRequest
    {
        public bool ForceReprocess { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class RetryLeaveCarryForwardRunRequest
    {
        [Required, MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }

    public class CancelLeaveCarryForwardRunRequest
    {
        [Required, MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }

    public class ReverseLeaveCarryForwardRunRequest
    {
        [Required, MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }

    public class LeaveCarryForwardRunActionResponse
    {
        public Guid Id { get; set; }
        public string RunNumber { get; set; } = string.Empty;
        public string RunStatus { get; set; } = string.Empty;
        public int TargetCount { get; set; }
        public int PostedCount { get; set; }
        public int SkippedCount { get; set; }
        public int FailedCount { get; set; }
        public decimal TotalCarryForwardDays { get; set; }
        public decimal TotalExpiredDays { get; set; }
        public decimal TotalPayoutDays { get; set; }
        public bool IsIdempotent { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class LeaveCarryForwardReconciliationResponse
    {
        public Guid LeaveCarryForwardRunId { get; set; }
        public string RunNumber { get; set; } = string.Empty;
        public string RunStatus { get; set; } = string.Empty;
        public int RunPostedCount { get; set; }
        public int ActualPostedDetailCount { get; set; }
        public int PostedLedgerCount { get; set; }
        public decimal RunTotalCarryForwardDays { get; set; }
        public decimal ActualCarryForwardDays { get; set; }
        public decimal DestinationCarryForwardLedgerDays { get; set; }
        public decimal SourceCarryForwardLedgerDays { get; set; }
        public decimal RunTotalExpiredDays { get; set; }
        public decimal LedgerExpiredDays { get; set; }
        public decimal RunTotalPayoutDays { get; set; }
        public decimal LedgerPayoutDays { get; set; }
        public bool IsBalanced { get; set; }
        public List<LeaveCarryForwardReconciliationIssueResponse> Issues { get; set; } = new();
    }

    public class LeaveCarryForwardReconciliationIssueResponse
    {
        public string Code { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Guid? LeaveCarryForwardId { get; set; }
        public Guid? LeaveBalanceTransactionId { get; set; }
    }

    public class EnqueueDueLeaveCarryForwardRequest
    {
        public DateOnly? ExecutionDate { get; set; }
        public bool QueueForProcessing { get; set; } = true;
    }

    public class LeaveCarryForwardEnqueueResponse
    {
        public DateOnly ExecutionDate { get; set; }
        public int EvaluatedPolicyCount { get; set; }
        public int CreatedRunCount { get; set; }
        public int ExistingRunCount { get; set; }
        public int SkippedCount { get; set; }
        public List<Guid> CreatedRunIds { get; set; } = new();
        public List<string> Messages { get; set; } = new();
    }

    public class LeaveCarryForwardExpiryRequest
    {
        [Required]
        public DateOnly AsOfDate { get; set; }

        public Guid? DestinationLeaveEntitlementPeriodId { get; set; }
        public Guid? LeaveTypeId { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public bool IsDryRun { get; set; }

        [Range(1, 2000)]
        public int MaximumItem { get; set; } = 500;

        [MaxLength(100)]
        public string? CorrelationId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class LeaveCarryForwardExpiryResponse
    {
        public DateOnly AsOfDate { get; set; }
        public int CandidateCount { get; set; }
        public int PostedCount { get; set; }
        public int SkippedCount { get; set; }
        public int FailedCount { get; set; }
        public decimal TotalExpiredDays { get; set; }
        public bool IsDryRun { get; set; }
        public bool IsTruncated { get; set; }
        public List<LeaveCarryForwardExpiryItemResponse> Items { get; set; } = new();
    }

    public class LeaveCarryForwardExpiryItemResponse
    {
        public Guid LeaveCarryForwardId { get; set; }
        public string CarryForwardNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string? WorkforceDisplayName { get; set; }
        public Guid DestinationLeaveBalanceId { get; set; }
        public DateOnly CarryForwardExpiryDate { get; set; }
        public decimal OriginalCarryForwardDays { get; set; }
        public decimal PreviouslyExpiredDays { get; set; }
        public decimal ExpirableDays { get; set; }
        public decimal PostedExpiredDays { get; set; }
        public bool Success { get; set; }
        public bool IsIdempotent { get; set; }
        public string ResultCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
