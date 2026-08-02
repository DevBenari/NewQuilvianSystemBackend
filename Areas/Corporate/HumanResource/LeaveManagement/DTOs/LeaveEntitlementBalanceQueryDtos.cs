using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs
{
    public class LeaveQueryOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class LeaveQueryCustomPeriodResponse : LeaveQueryOptionResponse
    {
    }

    public class LeaveEntitlementPeriodFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public LeaveEntitlementPeriodQueryRequest DefaultFilter { get; set; } = new();
        public List<LeaveQueryCustomPeriodResponse> CustomPeriods { get; set; } = new();
        public List<LeaveQueryOptionResponse> PeriodStatusOptions { get; set; } = new();
        public List<LeaveQueryOptionResponse> PeriodBasisOptions { get; set; } = new();
        public List<LeaveQueryOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class LeaveBalanceFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public LeaveBalanceQueryRequest DefaultFilter { get; set; } = new();
        public List<LeaveQueryOptionResponse> BalanceStatusOptions { get; set; } = new();
        public List<LeaveQueryOptionResponse> ReconciliationOptions { get; set; } = new();
        public List<LeaveQueryOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class LeaveEntitlementPeriodQueryRequest
    {
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? LeaveTypeId { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public int? PeriodYear { get; set; }
        public string? PeriodBasis { get; set; }
        public string? PeriodStatus { get; set; }
        public bool? IsLocked { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "startDate";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class LeaveBalanceQueryRequest
    {
        public Guid? LeaveEntitlementPeriodId { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public Guid? LeaveTypeId { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
        public int? Year { get; set; }
        public string? BalanceStatus { get; set; }
        public bool? IsLocked { get; set; }
        public bool? IsActive { get; set; }
        public bool? HasAvailableBalance { get; set; }
        public bool? HasReservedBalance { get; set; }
        public bool? HasExpiredBalance { get; set; }
        public string? ReconciliationStatus { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "workforceDisplayName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class LeaveEntitlementPeriodSummaryResponse
    {
        public int TotalPeriod { get; set; }
        public int OpenPeriod { get; set; }
        public int ProcessingPeriod { get; set; }
        public int ClosedPeriod { get; set; }
        public int ReopenedPeriod { get; set; }
        public int CancelledPeriod { get; set; }
        public int LockedPeriod { get; set; }
        public int ActivePeriod { get; set; }
        public int TotalBalance { get; set; }
        public int TotalWorkforce { get; set; }
        public decimal TotalRemainingDays { get; set; }
        public decimal TotalAvailableDays { get; set; }
        public decimal TotalReservedDays { get; set; }
    }

    public class LeaveEntitlementPeriodResponse
    {
        public Guid Id { get; set; }
        public Guid? LeaveTypeId { get; set; }
        public string? LeaveTypeCode { get; set; }
        public string? LeaveTypeName { get; set; }
        public Guid? LegalEntityId { get; set; }
        public string? LegalEntityName { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public string? HospitalSiteName { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string PeriodCode { get; set; } = string.Empty;
        public string PeriodName { get; set; } = string.Empty;
        public string PeriodBasis { get; set; } = string.Empty;
        public int PeriodYear { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string PeriodStatus { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
        public bool IsActive { get; set; }
        public int LeaveBalanceCount { get; set; }
        public int WorkforceCount { get; set; }
        public int EntitlementCount { get; set; }
        public int AccrualRunCount { get; set; }
        public int SourceCarryForwardRunCount { get; set; }
        public int DestinationCarryForwardRunCount { get; set; }
        public int AdjustmentCount { get; set; }
        public decimal TotalRemainingDays { get; set; }
        public decimal TotalAvailableDays { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class LeaveEntitlementPeriodDetailResponse : LeaveEntitlementPeriodResponse
    {
        public DateTime? ProcessingStartedAt { get; set; }
        public Guid? ProcessingStartedByUserId { get; set; }
        public string? ProcessingStartedByName { get; set; }
        public DateTime? ClosedAt { get; set; }
        public Guid? ClosedByUserId { get; set; }
        public string? ClosedByName { get; set; }
        public string? CloseReason { get; set; }
        public DateTime? ReopenedAt { get; set; }
        public Guid? ReopenedByUserId { get; set; }
        public string? ReopenedByName { get; set; }
        public string? ReopenReason { get; set; }
        public int ReopenCount { get; set; }
        public DateTime? LastReconciledAt { get; set; }
        public string? ValidationSnapshotJson { get; set; }
        public string? Description { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public List<LeavePeriodBalanceBreakdownResponse> BalanceBreakdown { get; set; } = new();
    }

    public class LeavePeriodBalanceBreakdownResponse
    {
        public Guid LeaveTypeId { get; set; }
        public string LeaveTypeCode { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public int BalanceCount { get; set; }
        public int WorkforceCount { get; set; }
        public decimal OpeningBalanceDays { get; set; }
        public decimal EntitlementDays { get; set; }
        public decimal AccruedDays { get; set; }
        public decimal CarriedForwardDays { get; set; }
        public decimal AdjustmentDays { get; set; }
        public decimal UsedDays { get; set; }
        public decimal ReservedDays { get; set; }
        public decimal RemainingDays { get; set; }
        public decimal AvailableDays { get; set; }
    }

    public class LeaveEntitlementPeriodOptionResponse
    {
        public Guid Id { get; set; }
        public string PeriodCode { get; set; } = string.Empty;
        public string PeriodName { get; set; } = string.Empty;
        public int PeriodYear { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string PeriodStatus { get; set; } = string.Empty;
        public Guid? LeaveTypeId { get; set; }
        public string? LeaveTypeName { get; set; }
        public bool IsLocked { get; set; }
    }

    public class LeaveEntitlementPeriodPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<LeaveEntitlementPeriodResponse> Items { get; set; } = new();
    }

    public class LeaveBalanceSummaryResponse
    {
        public int TotalBalance { get; set; }
        public int ActiveBalance { get; set; }
        public int LockedBalance { get; set; }
        public int ClosedBalance { get; set; }
        public int ExpiredBalance { get; set; }
        public int WorkforceCount { get; set; }
        public int LeaveTypeCount { get; set; }
        public int BalanceWithAvailableDays { get; set; }
        public int BalanceWithReservedDays { get; set; }
        public int BalanceWithMismatch { get; set; }
        public decimal TotalOpeningBalanceDays { get; set; }
        public decimal TotalEntitlementDays { get; set; }
        public decimal TotalAccruedDays { get; set; }
        public decimal TotalCarriedForwardDays { get; set; }
        public decimal TotalAdjustmentDays { get; set; }
        public decimal TotalReservedDays { get; set; }
        public decimal TotalUsedDays { get; set; }
        public decimal TotalExpiredDays { get; set; }
        public decimal TotalRemainingDays { get; set; }
        public decimal TotalAvailableDays { get; set; }
    }

    public class LeaveBalanceResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string? ProfileCode { get; set; }
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public string? EmployeeNumber { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public Guid? PositionId { get; set; }
        public string? PositionName { get; set; }
        public Guid LeaveTypeId { get; set; }
        public string LeaveTypeCode { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public string LeaveCategory { get; set; } = string.Empty;
        public Guid? LeaveEntitlementPeriodId { get; set; }
        public string? PeriodCode { get; set; }
        public string? PeriodName { get; set; }
        public int Year { get; set; }
        public DateOnly PeriodStartDate { get; set; }
        public DateOnly PeriodEndDate { get; set; }
        public decimal OpeningBalanceDays { get; set; }
        public decimal EntitlementDays { get; set; }
        public decimal AccruedDays { get; set; }
        public decimal CarriedForwardDays { get; set; }
        public decimal AdjustmentDays { get; set; }
        public decimal CompensatoryDays { get; set; }
        public decimal ReservedDays { get; set; }
        public decimal PendingDays { get; set; }
        public decimal UsedDays { get; set; }
        public decimal RecalledDays { get; set; }
        public decimal ExpiredDays { get; set; }
        public decimal EncashmentDays { get; set; }
        public decimal RemainingDays { get; set; }
        public decimal AvailableDays { get; set; }
        public string BalanceStatus { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
        public bool IsActive { get; set; }
        public long BalanceVersion { get; set; }
        public long LastTransactionSequence { get; set; }
        public DateTime? LastCalculatedAt { get; set; }
        public DateTime? LastReconciledAt { get; set; }
        public DateOnly? CarryForwardExpiryDate { get; set; }
        public bool IsFormulaBalanced { get; set; }
        public decimal FormulaDifferenceDays { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class LeaveBalanceDetailResponse : LeaveBalanceResponse
    {
        public Guid? LeavePolicyId { get; set; }
        public string? LeavePolicyCode { get; set; }
        public string? LeavePolicyName { get; set; }
        public Guid? LeaveEntitlementPolicyId { get; set; }
        public string? EntitlementPolicyCode { get; set; }
        public string? EntitlementPolicyName { get; set; }
        public Guid? LastTransactionId { get; set; }
        public string? Description { get; set; }
        public DateTime? LockedAt { get; set; }
        public Guid? LockedByUserId { get; set; }
        public string? LockedByName { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public LeaveBalanceReconciliationResponse Reconciliation { get; set; } = new();
        public LeaveBalanceHistorySummaryResponse HistorySummary { get; set; } = new();
    }

    public class LeaveBalanceHistorySummaryResponse
    {
        public int LedgerCount { get; set; }
        public int EntitlementCount { get; set; }
        public int AccrualCount { get; set; }
        public int SourceCarryForwardCount { get; set; }
        public int DestinationCarryForwardCount { get; set; }
        public int AdjustmentCount { get; set; }
    }

    public class LeaveBalancePagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<LeaveBalanceResponse> Items { get; set; } = new();
    }

    public class LeaveBalanceLedgerQueryRequest
    {
        public string? TransactionType { get; set; }
        public string? TransactionStatus { get; set; }
        public DateOnly? EffectiveStartDate { get; set; }
        public DateOnly? EffectiveEndDate { get; set; }
        public string? Search { get; set; }
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class LeaveBalanceTransactionResponse
    {
        public Guid Id { get; set; }
        public string TransactionNumber { get; set; } = string.Empty;
        public DateTime TransactionDateTime { get; set; }
        public DateOnly? EffectiveDate { get; set; }
        public long TransactionSequence { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
        public decimal TransactionDays { get; set; }
        public decimal OpeningBalanceDelta { get; set; }
        public decimal EntitlementDelta { get; set; }
        public decimal AccruedDelta { get; set; }
        public decimal CarryForwardDelta { get; set; }
        public decimal AdjustmentDelta { get; set; }
        public decimal CompensatoryDelta { get; set; }
        public decimal PendingDelta { get; set; }
        public decimal ReservedDelta { get; set; }
        public decimal UsedDelta { get; set; }
        public decimal RecalledDelta { get; set; }
        public decimal ExpiredDelta { get; set; }
        public decimal EncashmentDelta { get; set; }
        public decimal AvailableDelta { get; set; }
        public decimal PreviousAvailableDays { get; set; }
        public decimal NewAvailableDays { get; set; }
        public decimal PreviousReservedDays { get; set; }
        public decimal NewReservedDays { get; set; }
        public decimal NewUsedDays { get; set; }
        public string TransactionStatus { get; set; } = string.Empty;
        public string? PostingBatchType { get; set; }
        public Guid? PostingBatchId { get; set; }
        public string SourceType { get; set; } = string.Empty;
        public Guid? SourceReferenceId { get; set; }
        public string? SourceReferenceNumber { get; set; }
        public Guid? OriginalTransactionId { get; set; }
        public Guid? ReversedTransactionId { get; set; }
        public DateTime? PostedAt { get; set; }
        public Guid? PostedByUserId { get; set; }
        public string? PostedByName { get; set; }
        public string? Remarks { get; set; }
    }

    public class LeaveBalanceTransactionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<LeaveBalanceTransactionResponse> Items { get; set; } = new();
    }

    public class LeaveEntitlementHistoryResponse
    {
        public Guid Id { get; set; }
        public string EntitlementNumber { get; set; } = string.Empty;
        public int EntitlementYear { get; set; }
        public DateOnly PeriodStartDate { get; set; }
        public DateOnly PeriodEndDate { get; set; }
        public DateOnly? GrantDate { get; set; }
        public DateOnly? AvailableFromDate { get; set; }
        public DateOnly? ExpiryDate { get; set; }
        public decimal BaseEntitlementDays { get; set; }
        public decimal ProratedEntitlementDays { get; set; }
        public decimal AdditionalEntitlementDays { get; set; }
        public decimal CarryForwardEntitlementDays { get; set; }
        public decimal TotalEntitlementDays { get; set; }
        public bool IsProrated { get; set; }
        public string EntitlementStatus { get; set; } = string.Empty;
        public string SourceType { get; set; } = string.Empty;
        public DateTime? GeneratedAt { get; set; }
        public DateTime? PostedAt { get; set; }
        public string? Notes { get; set; }
    }

    public class LeaveAccrualHistoryResponse
    {
        public Guid Id { get; set; }
        public string AccrualNumber { get; set; } = string.Empty;
        public Guid? LeaveAccrualRunId { get; set; }
        public string? RunNumber { get; set; }
        public DateOnly AccrualDate { get; set; }
        public DateOnly? ScheduledAccrualDate { get; set; }
        public DateOnly AccrualPeriodStartDate { get; set; }
        public DateOnly AccrualPeriodEndDate { get; set; }
        public int AccrualSequence { get; set; }
        public decimal AccrualAmountDays { get; set; }
        public decimal BalanceBeforeAccrual { get; set; }
        public decimal BalanceAfterAccrual { get; set; }
        public bool IsProrated { get; set; }
        public string AccrualStatus { get; set; } = string.Empty;
        public string AccrualFrequency { get; set; } = string.Empty;
        public string SourceType { get; set; } = string.Empty;
        public DateTime? CalculatedAt { get; set; }
        public DateTime? PostedAt { get; set; }
        public string? Notes { get; set; }
    }

    public class LeaveCarryForwardHistoryResponse
    {
        public Guid Id { get; set; }
        public string CarryForwardNumber { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
        public Guid SourceLeaveEntitlementPeriodId { get; set; }
        public string? SourcePeriodCode { get; set; }
        public Guid DestinationLeaveEntitlementPeriodId { get; set; }
        public string? DestinationPeriodCode { get; set; }
        public Guid SourceLeaveTypeId { get; set; }
        public string? SourceLeaveTypeName { get; set; }
        public Guid DestinationLeaveTypeId { get; set; }
        public string? DestinationLeaveTypeName { get; set; }
        public DateOnly CalculationDate { get; set; }
        public DateOnly? CarryForwardExpiryDate { get; set; }
        public decimal SourceAvailableDays { get; set; }
        public decimal EligibleDays { get; set; }
        public decimal CarryForwardDays { get; set; }
        public decimal ExpiredDays { get; set; }
        public decimal ExcessDays { get; set; }
        public decimal PayoutDays { get; set; }
        public string CarryForwardStatus { get; set; } = string.Empty;
        public string? SkipReasonCode { get; set; }
        public string? SkipReason { get; set; }
        public DateTime? CalculatedAt { get; set; }
        public DateTime? PostedAt { get; set; }
        public string? Notes { get; set; }
    }

    public class LeaveAdjustmentHistoryResponse
    {
        public Guid Id { get; set; }
        public string AdjustmentNumber { get; set; } = string.Empty;
        public Guid LeaveAdjustmentReasonId { get; set; }
        public string? ReasonCode { get; set; }
        public string? ReasonName { get; set; }
        public string AdjustmentType { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
        public decimal RequestedDays { get; set; }
        public decimal? ApprovedDays { get; set; }
        public decimal PostedDays { get; set; }
        public DateOnly EffectiveDate { get; set; }
        public string AdjustmentStatus { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? PostedAt { get; set; }
        public DateTime? ReversedAt { get; set; }
        public string? RejectionReason { get; set; }
        public string? ReversalReason { get; set; }
    }

    public class LeaveBalanceReconciliationResponse
    {
        public Guid LeaveBalanceId { get; set; }
        public bool IsFormulaBalanced { get; set; }
        public decimal FormulaExpectedRemainingDays { get; set; }
        public decimal FormulaActualRemainingDays { get; set; }
        public decimal FormulaDifferenceDays { get; set; }
        public decimal FormulaExpectedAvailableDays { get; set; }
        public decimal FormulaActualAvailableDays { get; set; }
        public decimal FormulaAvailableDifferenceDays { get; set; }
        public bool IsLedgerBalanced { get; set; }
        public long LastTransactionSequence { get; set; }
        public long ActualMaximumTransactionSequence { get; set; }
        public decimal LedgerOpeningBalanceDays { get; set; }
        public decimal LedgerEntitlementDays { get; set; }
        public decimal LedgerAccruedDays { get; set; }
        public decimal LedgerCarryForwardDays { get; set; }
        public decimal LedgerAdjustmentDays { get; set; }
        public decimal LedgerReservedDays { get; set; }
        public decimal LedgerUsedDays { get; set; }
        public decimal LedgerExpiredDays { get; set; }
        public decimal LedgerAvailableDays { get; set; }
        public List<string> Issues { get; set; } = new();
    }

    public class LeaveSelfServiceSummaryResponse
    {
        public Guid WorkforceProfileId { get; set; }
        public int TotalBalance { get; set; }
        public int LeaveTypeCount { get; set; }
        public int ActiveBalance { get; set; }
        public int LockedBalance { get; set; }
        public decimal TotalRemainingDays { get; set; }
        public decimal TotalAvailableDays { get; set; }
        public decimal TotalReservedDays { get; set; }
        public decimal TotalPendingDays { get; set; }
        public decimal TotalUsedDays { get; set; }
        public List<LeaveSelfServiceBalanceCardResponse> BalanceCards { get; set; } = new();
    }

    public class LeaveSelfServiceBalanceCardResponse
    {
        public Guid LeaveBalanceId { get; set; }
        public Guid LeaveTypeId { get; set; }
        public string LeaveTypeCode { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public string? ColorCode { get; set; }
        public int Year { get; set; }
        public string? PeriodName { get; set; }
        public decimal RemainingDays { get; set; }
        public decimal AvailableDays { get; set; }
        public decimal ReservedDays { get; set; }
        public decimal PendingDays { get; set; }
        public decimal UsedDays { get; set; }
        public bool IsLocked { get; set; }
        public string BalanceStatus { get; set; } = string.Empty;
    }
}
