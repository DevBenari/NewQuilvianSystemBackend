using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs
{
    public class LeavePayrollStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class LeavePayrollIntegrationDefaultFilterResponse
    {
        public Guid? WorkforceProfileId { get; set; }
        public Guid? LeaveTypeId { get; set; }
        public string? ReadinessStatus { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "leaveDate";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class LeavePayrollIntegrationQueryRequest : LeavePayrollIntegrationDefaultFilterResponse
    {
    }

    public class LeavePayrollIntegrationMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public int MaximumItemPerExecution { get; set; }
        public LeavePayrollIntegrationDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<LeavePayrollStringOptionResponse> ReadinessStatusOptions { get; set; } = new();
        public List<LeavePayrollStringOptionResponse> IssueTypeOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public bool AutoCreateAttendanceInputs { get; set; }
        public bool AutoCreateLeaveAllowanceVariableInputs { get; set; }
        public bool AutoCreateEncashmentVariableInputs { get; set; }
        public bool SubmitVariableInputs { get; set; }
        public string? LeaveAllowancePayrollComponentCode { get; set; }
        public string? LeaveEncashmentPayrollComponentCode { get; set; }
        public string BoundaryInfo { get; set; } = string.Empty;
    }

    public class LeavePayrollRunOptionResponse
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

    public class LeavePayrollComponentOptionResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ComponentType { get; set; } = string.Empty;
        public string CalculationMethod { get; set; } = string.Empty;
        public bool IsTaxable { get; set; }
    }

    public class LeavePayrollIntegrationSummaryResponse
    {
        public Guid PayrollRunId { get; set; }
        public string RunNumber { get; set; } = string.Empty;
        public string RunStatus { get; set; } = string.Empty;
        public bool IsPayrollRunLocked { get; set; }
        public Guid PayrollPeriodId { get; set; }
        public string PayrollPeriodCode { get; set; } = string.Empty;
        public DateOnly PeriodStartDate { get; set; }
        public DateOnly PeriodEndDate { get; set; }
        public string PayrollPeriodStatus { get; set; } = string.Empty;
        public bool IsPayrollPeriodLocked { get; set; }
        public int LeaveIntegrationCount { get; set; }
        public int DistinctEmployeeCount { get; set; }
        public decimal PaidLeaveDays { get; set; }
        public decimal UnpaidLeaveDays { get; set; }
        public int ReadyCount { get; set; }
        public int SynchronizedCount { get; set; }
        public int MissingPayrollRunEmployeeCount { get; set; }
        public int MissingPayrollAttendanceInputCount { get; set; }
        public decimal EncashmentPayoutDays { get; set; }
        public int EncashmentCandidateCount { get; set; }
        public int LeaveAllowanceCandidateCount { get; set; }
        public int LeaveVariableInputCount { get; set; }
        public bool CanExecute { get; set; }
        public List<string> BlockingReasons { get; set; } = new();
    }

    public class LeavePayrollIntegrationPreviewItemResponse
    {
        public Guid LeaveAttendanceIntegrationId { get; set; }
        public Guid LeaveExecutionId { get; set; }
        public Guid LeaveRequestId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string? WorkforceProfileCode { get; set; }
        public string? WorkforceDisplayName { get; set; }
        public Guid LeaveTypeId { get; set; }
        public string? LeaveTypeCode { get; set; }
        public string? LeaveTypeName { get; set; }
        public DateOnly LeaveDate { get; set; }
        public decimal ExpectedPaidLeaveDays { get; set; }
        public decimal ExpectedUnpaidLeaveDays { get; set; }
        public int PayableLeaveMinutes { get; set; }
        public Guid? PayrollRunEmployeeId { get; set; }
        public Guid? PayrollAttendanceInputId { get; set; }
        public decimal ActualPaidLeaveDays { get; set; }
        public decimal ActualUnpaidLeaveDays { get; set; }
        public bool IsPayrollEmployeeFrozen { get; set; }
        public string ReadinessStatus { get; set; } = string.Empty;
        public bool IsReady { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class LeavePayrollIntegrationPreviewPagedResponse
    {
        public Guid PayrollRunId { get; set; }
        public string RunNumber { get; set; } = string.Empty;
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<LeavePayrollIntegrationPreviewItemResponse> Items { get; set; } = new();
    }

    public class ExecuteLeavePayrollIntegrationRequest
    {
        public List<Guid>? LeaveAttendanceIntegrationIds { get; set; }
        public List<Guid>? WorkforceProfileIds { get; set; }
        public bool? EnsureAttendanceInputs { get; set; }
        public bool? CreateLeaveAllowanceInputs { get; set; }
        public bool? CreateEncashmentInputs { get; set; }
        public bool? SubmitVariableInputs { get; set; }
        public bool ContinueOnValidationError { get; set; } = true;

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class LeavePayrollIntegrationExecutionItemResponse
    {
        public Guid? LeaveAttendanceIntegrationId { get; set; }
        public Guid? SourceId { get; set; }
        public string SourceType { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public DateOnly? EffectiveDate { get; set; }
        public Guid? PayrollRunEmployeeId { get; set; }
        public Guid? PayrollAttendanceInputId { get; set; }
        public Guid? PayrollVariableInputId { get; set; }
        public decimal Quantity { get; set; }
        public bool Success { get; set; }
        public bool IsCreated { get; set; }
        public bool IsUpdated { get; set; }
        public bool IsIdempotent { get; set; }
        public string ResultStatus { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class LeavePayrollIntegrationExecutionResponse
    {
        public Guid PayrollRunId { get; set; }
        public string RunNumber { get; set; } = string.Empty;
        public Guid PayrollPeriodId { get; set; }
        public string PayrollPeriodCode { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public DateTime CompletedAt { get; set; }
        public int TotalTarget { get; set; }
        public int AttendanceInputUpdatedCount { get; set; }
        public int VariableInputCreatedCount { get; set; }
        public int VariableInputUpdatedCount { get; set; }
        public int IdempotentCount { get; set; }
        public int FailedCount { get; set; }
        public decimal PaidLeaveDays { get; set; }
        public decimal UnpaidLeaveDays { get; set; }
        public decimal EncashmentPayoutDays { get; set; }
        public List<string> Warnings { get; set; } = new();
        public List<LeavePayrollIntegrationExecutionItemResponse> Items { get; set; } = new();
    }

    public class LeavePayrollReconciliationQueryRequest
    {
        public string? IssueType { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public string? Search { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class LeavePayrollReconciliationItemResponse
    {
        public string IssueType { get; set; } = string.Empty;
        public string Severity { get; set; } = "Warning";
        public Guid WorkforceProfileId { get; set; }
        public string? WorkforceProfileCode { get; set; }
        public string? WorkforceDisplayName { get; set; }
        public DateOnly? EffectiveDate { get; set; }
        public Guid? SourceId { get; set; }
        public Guid? PayrollAttendanceInputId { get; set; }
        public Guid? PayrollVariableInputId { get; set; }
        public decimal ExpectedQuantity { get; set; }
        public decimal ActualQuantity { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class LeavePayrollReconciliationResponse
    {
        public Guid PayrollRunId { get; set; }
        public string RunNumber { get; set; } = string.Empty;
        public int ExpectedAttendanceGroupCount { get; set; }
        public int MatchedAttendanceGroupCount { get; set; }
        public int ExpectedVariableInputCount { get; set; }
        public int MatchedVariableInputCount { get; set; }
        public int IssueCount { get; set; }
        public bool IsBalanced { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPage { get; set; }
        public List<LeavePayrollReconciliationItemResponse> Issues { get; set; } = new();
    }

    public class RollbackLeavePayrollIntegrationRequest
    {
        [Required, MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
        public bool IncludeAttendanceLeaveDays { get; set; } = true;
        public bool IncludeVariableInputs { get; set; } = true;
        public bool AllowSubmittedVariableInputRollback { get; set; } = false;
    }

    public class LeavePayrollIntegrationRollbackResponse
    {
        public Guid PayrollRunId { get; set; }
        public string RunNumber { get; set; } = string.Empty;
        public int AttendanceInputResetCount { get; set; }
        public int VariableInputDeletedCount { get; set; }
        public int BlockedVariableInputCount { get; set; }
        public int PayrollRunEmployeeRecalculatedCount { get; set; }
        public DateTime RolledBackAt { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
