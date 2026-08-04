using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs
{
    public class LeaveExecutionOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class LeaveCalendarQueryRequest
    {
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public Guid? LeaveTypeId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
        public string? LeaveRequestStatus { get; set; }
        public bool IncludePending { get; set; }
        public string? Search { get; set; }
        public int MaximumItem { get; set; } = 500;
    }

    public class LeaveCalendarEntryResponse
    {
        public Guid LeaveRequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string? WorkforceProfileCode { get; set; }
        public string? WorkforceDisplayName { get; set; }
        public string? EmployeeNumber { get; set; }
        public Guid LeaveTypeId { get; set; }
        public string LeaveTypeCode { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public string LeaveCategory { get; set; } = string.Empty;
        public string? ColorCode { get; set; }
        public bool IsPaidLeave { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal RequestedDays { get; set; }
        public bool IsHalfDay { get; set; }
        public string? HalfDayPeriod { get; set; }
        public bool IsHourly { get; set; }
        public int? RequestedMinutes { get; set; }
        public string LeaveRequestStatus { get; set; } = string.Empty;
        public string? ExecutionStatus { get; set; }
        public string? AttendanceIntegrationStatus { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public Guid? PositionId { get; set; }
        public string? PositionName { get; set; }
        public Guid? ReplacementWorkforceProfileId { get; set; }
        public string? ReplacementWorkforceName { get; set; }
        public bool HasRosterConflict { get; set; }
        public bool HasTrainingConflict { get; set; }
        public bool HasCriticalStaffingImpact { get; set; }
        public bool IsPendingApproval { get; set; }
        public bool IsCurrentLeave { get; set; }
        public bool IsUpcomingLeave { get; set; }
    }

    public class LeaveCalendarSummaryResponse
    {
        public int TotalLeaveRequest { get; set; }
        public int DistinctEmployee { get; set; }
        public int Approved { get; set; }
        public int Active { get; set; }
        public int Completed { get; set; }
        public int PendingApproval { get; set; }
        public int PaidLeave { get; set; }
        public int UnpaidLeave { get; set; }
        public int RosterConflict { get; set; }
        public int CriticalStaffingImpact { get; set; }
        public decimal TotalLeaveDays { get; set; }
        public List<LeaveCalendarBreakdownResponse> ByLeaveType { get; set; } = new();
        public List<LeaveCalendarBreakdownResponse> ByDepartment { get; set; } = new();
    }

    public class LeaveCalendarBreakdownResponse
    {
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalDays { get; set; }
    }

    public class LeaveCalendarResponse
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int TotalItem { get; set; }
        public bool IsTruncated { get; set; }
        public LeaveCalendarSummaryResponse Summary { get; set; } = new();
        public List<LeaveCalendarEntryResponse> Items { get; set; } = new();
    }

    public class LeaveExecutionFilterMetadataResponse
    {
        public List<LeaveExecutionOptionResponse> ExecutionStatuses { get; set; } = new();
        public List<LeaveExecutionOptionResponse> IntegrationStatuses { get; set; } = new();
        public List<LeaveExecutionOptionResponse> MonitoringStatuses { get; set; } = new();
        public List<LeaveExecutionOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new() { "asc", "desc" };
        public List<int> PageSizeOptions { get; set; } = new() { 10, 25, 50, 100 };
    }

    public class LeaveExecutionQueryRequest
    {
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public Guid? LeaveTypeId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? LeaveRequestStatus { get; set; }
        public string? ExecutionStatus { get; set; }
        public string? IntegrationStatus { get; set; }
        public string? MonitoringStatus { get; set; }
        public bool? RequiresAttention { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "startDate";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class LeaveExecutionSummaryResponse
    {
        public int TotalApprovedRequest { get; set; }
        public int MissingExecution { get; set; }
        public int Scheduled { get; set; }
        public int StartDue { get; set; }
        public int Active { get; set; }
        public int CompletionDue { get; set; }
        public int Completed { get; set; }
        public int AttendanceConflict { get; set; }
        public int BalancePending { get; set; }
        public int Failed { get; set; }
        public int RequiresAttention { get; set; }
        public int PendingAttendanceDay { get; set; }
        public int AppliedAttendanceDay { get; set; }
        public int ConflictAttendanceDay { get; set; }
        public int FailedAttendanceDay { get; set; }
    }

    public class LeaveExecutionListResponse
    {
        public Guid LeaveRequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public Guid? LeaveExecutionId { get; set; }
        public string? ExecutionNumber { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string? WorkforceProfileCode { get; set; }
        public string? WorkforceDisplayName { get; set; }
        public string? EmployeeNumber { get; set; }
        public Guid LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal RequestedDays { get; set; }
        public string LeaveRequestStatus { get; set; } = string.Empty;
        public string? ExecutionStatus { get; set; }
        public string? AttendanceIntegrationStatus { get; set; }
        public string? BalanceExecutionStatus { get; set; }
        public string MonitoringStatus { get; set; } = string.Empty;
        public int ExpectedAttendanceDayCount { get; set; }
        public int AppliedAttendanceDayCount { get; set; }
        public int ConflictAttendanceDayCount { get; set; }
        public int FailedAttendanceDayCount { get; set; }
        public string? DeductionTiming { get; set; }
        public decimal EstimatedBalanceDeduction { get; set; }
        public decimal ActualBalanceDeduction { get; set; }
        public bool RequiresAttention { get; set; }
        public List<string> Issues { get; set; } = new();
        public List<string> AvailableActions { get; set; } = new();
    }

    public class LeaveExecutionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<LeaveExecutionListResponse> Items { get; set; } = new();
    }

    public class LeaveAttendanceIntegrationResponse
    {
        public Guid Id { get; set; }
        public DateOnly LeaveDate { get; set; }
        public Guid? AttendanceDailyId { get; set; }
        public decimal RequestedLeaveDays { get; set; }
        public int? RequestedMinutes { get; set; }
        public bool IsHalfDay { get; set; }
        public bool IsHourly { get; set; }
        public bool IsPaidLeave { get; set; }
        public int ScheduledMinutes { get; set; }
        public int PayableLeaveMinutes { get; set; }
        public string IntegrationStatus { get; set; } = string.Empty;
        public string? AttendanceStatusBefore { get; set; }
        public string? AttendanceStatusAfter { get; set; }
        public DateTime? AppliedAt { get; set; }
        public DateTime? ReversedAt { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class LeaveExecutionDetailResponse : LeaveExecutionListResponse
    {
        public Guid? LeaveBalanceId { get; set; }
        public Guid? LeavePolicyId { get; set; }
        public string? LeavePolicyCode { get; set; }
        public bool IsPaidLeave { get; set; }
        public bool IsHalfDay { get; set; }
        public bool IsHourly { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? ReversedAt { get; set; }
        public DateTime? LastAttemptAt { get; set; }
        public int RetryCount { get; set; }
        public string? CorrelationId { get; set; }
        public string? ErrorSummary { get; set; }
        public string? ExecutionSnapshotJson { get; set; }
        public string? ResultSnapshotJson { get; set; }
        public List<LeaveAttendanceIntegrationResponse> AttendanceIntegrations { get; set; } = new();
    }

    public class ExecuteLeaveRequestRequest
    {
        public DateOnly? AsOfDate { get; set; }
        public bool ForceRetry { get; set; }
        [MaxLength(120)]
        public string? CorrelationId { get; set; }
        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class ProcessDueLeaveRequest
    {
        public DateOnly? AsOfDate { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public int MaximumItem { get; set; } = 500;
        public bool ForceRetry { get; set; }
        [MaxLength(120)]
        public string? CorrelationId { get; set; }
        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class ReverseLeaveExecutionRequest
    {
        [Required, MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;
        public DateOnly? EffectiveDate { get; set; }
        public decimal? RestoreDays { get; set; }
    }

    public class ApplyLeaveCancellationRequest
    {
        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class LeaveExecutionActionResponse
    {
        public Guid LeaveRequestId { get; set; }
        public Guid? LeaveExecutionId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string LeaveRequestStatus { get; set; } = string.Empty;
        public string? ExecutionStatus { get; set; }
        public string? AttendanceIntegrationStatus { get; set; }
        public string? BalanceExecutionStatus { get; set; }
        public int ProcessedDayCount { get; set; }
        public int AppliedDayCount { get; set; }
        public int ConflictDayCount { get; set; }
        public int FailedDayCount { get; set; }
        public bool IsIdempotent { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class LeaveExecutionBatchItemResponse
    {
        public Guid LeaveRequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ExecutionStatus { get; set; }
    }

    public class LeaveExecutionBatchResponse
    {
        public DateOnly AsOfDate { get; set; }
        public int TotalItem { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public int SkippedCount { get; set; }
        public List<LeaveExecutionBatchItemResponse> Items { get; set; } = new();
    }

    public class LeaveExecutionReconciliationResponse
    {
        public Guid LeaveRequestId { get; set; }
        public Guid? LeaveExecutionId { get; set; }
        public string LeaveRequestStatus { get; set; } = string.Empty;
        public string? ExecutionStatus { get; set; }
        public decimal ExpectedLeaveDays { get; set; }
        public decimal IntegratedLeaveDays { get; set; }
        public decimal EstimatedBalanceDeduction { get; set; }
        public decimal ActualBalanceDeduction { get; set; }
        public decimal LedgerUsedDays { get; set; }
        public decimal LedgerReservedDays { get; set; }
        public int ExpectedAttendanceDayCount { get; set; }
        public int AppliedAttendanceDayCount { get; set; }
        public int ConflictAttendanceDayCount { get; set; }
        public int FailedAttendanceDayCount { get; set; }
        public bool IsBalanced { get; set; }
        public List<string> Issues { get; set; } = new();
    }
}
