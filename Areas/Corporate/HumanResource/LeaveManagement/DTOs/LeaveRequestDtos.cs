using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs
{
    public class LeaveRequestOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class LeaveRequestFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string TimeFormat { get; set; } = "HH:mm";
        public DateOnly DefaultStartDate { get; set; } = new(DateTime.UtcNow.Year, 1, 1);
        public DateOnly DefaultEndDate { get; set; } = new(DateTime.UtcNow.Year, 12, 31);
        public string DefaultSortBy { get; set; } = "createDateTime";
        public string DefaultSortDirection { get; set; } = "desc";
        public int DefaultPageSize { get; set; } = 25;
        public List<LeaveRequestOptionResponse> Statuses { get; set; } = new();
        public List<LeaveRequestOptionResponse> HalfDayPeriods { get; set; } = new();
        public List<LeaveRequestOptionResponse> AttachmentTypes { get; set; } = new();
        public List<LeaveRequestOptionResponse> SourceChannels { get; set; } = new();
        public List<LeaveRequestOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new() { "asc", "desc" };
        public List<int> PageSizeOptions { get; set; } = new() { 10, 25, 50, 100 };
        public long MaximumAttachmentSizeBytes { get; set; }
        public List<string> AllowedAttachmentExtensions { get; set; } = new();
    }

    public class LeaveRequestBalanceOptionResponse
    {
        public Guid? LeaveBalanceId { get; set; }
        public Guid LeaveTypeId { get; set; }
        public string LeaveTypeCode { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public string LeaveCategory { get; set; } = string.Empty;
        public string? ColorCode { get; set; }
        public Guid? LeavePolicyId { get; set; }
        public string? LeavePolicyCode { get; set; }
        public string? LeavePolicyName { get; set; }
        public Guid? LeaveEntitlementPeriodId { get; set; }
        public string? EntitlementPeriodCode { get; set; }
        public string? EntitlementPeriodName { get; set; }
        public DateOnly? PeriodStartDate { get; set; }
        public DateOnly? PeriodEndDate { get; set; }
        public decimal RemainingDays { get; set; }
        public decimal AvailableDays { get; set; }
        public decimal ReservedDays { get; set; }
        public decimal PendingDays { get; set; }
        public bool IsPaidLeave { get; set; }
        public bool IsBalanceDeducted { get; set; }
        public bool AllowHalfDay { get; set; }
        public bool AllowHourly { get; set; }
        public bool RequiresAttachment { get; set; }
        public bool RequiresMedicalCertificate { get; set; }
        public bool IsLocked { get; set; }
        public bool CanRequest { get; set; }
        public string? RestrictionReason { get; set; }
    }

    public class LeaveRequestReasonOptionResponse
    {
        public Guid Id { get; set; }
        public string ReasonCode { get; set; } = string.Empty;
        public string ReasonName { get; set; } = string.Empty;
        public string? ReasonCategory { get; set; }
        public bool IsCommentRequired { get; set; }
        public bool IsAttachmentRequired { get; set; }
        public string? Description { get; set; }
    }

    public class LeaveRequestCalculationRequest
    {
        public Guid LeaveTypeId { get; set; }
        public Guid? LeaveBalanceId { get; set; }
        public Guid? RequestReasonId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public bool IsHalfDay { get; set; }
        [MaxLength(20)] public string? HalfDayPeriod { get; set; }
        public bool IsHourly { get; set; }
        public int? RequestedMinutes { get; set; }
        public Guid? ReplacementWorkforceProfileId { get; set; }
        public Guid? ExcludeLeaveRequestId { get; set; }
    }

    public class LeaveRequestCalculationDayResponse
    {
        public DateOnly Date { get; set; }
        public bool IsResolved { get; set; }
        public bool IsCounted { get; set; }
        public bool IsRestDay { get; set; }
        public bool IsHoliday { get; set; }
        public bool HasBlockingConflict { get; set; }
        public string ScheduleSource { get; set; } = string.Empty;
        public string? ShiftCode { get; set; }
        public string? ShiftName { get; set; }
        public DateTime? ScheduledStartAt { get; set; }
        public DateTime? ScheduledEndAt { get; set; }
        public int PlannedWorkMinutes { get; set; }
        public decimal CountedDays { get; set; }
        public List<string> HolidayNames { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public List<string> ConflictCodes { get; set; } = new();
    }

    public class LeaveRequestCalculationResponse
    {
        public Guid WorkforceProfileId { get; set; }
        public string? WorkforceProfileCode { get; set; }
        public string? WorkforceDisplayName { get; set; }
        public Guid LeaveTypeId { get; set; }
        public string LeaveTypeCode { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public Guid? LeaveBalanceId { get; set; }
        public Guid? LeavePolicyId { get; set; }
        public string? LeavePolicyCode { get; set; }
        public string? LeavePolicyName { get; set; }
        public string DayCalculationMethod { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal RequestedDays { get; set; }
        public int? RequestedMinutes { get; set; }
        public decimal CalculatedWorkingDays { get; set; }
        public decimal ExcludedHolidayDays { get; set; }
        public decimal ExcludedWeeklyOffDays { get; set; }
        public decimal BalanceBeforeRequest { get; set; }
        public decimal EstimatedBalanceDeduction { get; set; }
        public decimal EstimatedBalanceAfterRequest { get; set; }
        public bool IsBalanceSufficient { get; set; }
        public bool RequiresAttachment { get; set; }
        public bool RequiresMedicalCertificate { get; set; }
        public bool RequiresReplacement { get; set; }
        public bool HasOverlap { get; set; }
        public bool HasRosterConflict { get; set; }
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public List<LeaveRequestCalculationDayResponse> Days { get; set; } = new();
        public string CalculationSnapshotJson { get; set; } = "{}";
    }

    public class CreateLeaveRequestRequest : LeaveRequestCalculationRequest
    {
        [Required, MaxLength(2000)] public string Reason { get; set; } = string.Empty;
        [MaxLength(500)] public string? ContactAddressDuringLeave { get; set; }
        [MaxLength(50)] public string? ContactNumberDuringLeave { get; set; }
        [MaxLength(2000)] public string? HandoverNotes { get; set; }
    }

    public class UpdateLeaveRequestRequest : CreateLeaveRequestRequest { }

    public class PrepareLeaveRequestWorkflowRequest
    {
        [MaxLength(30)] public string SourceChannel { get; set; } = "Web";
        [MaxLength(100)] public string? RequestCorrelationId { get; set; }
        [MaxLength(100)] public string? IdempotencyKey { get; set; }
        public List<Guid> SelectedApproverUserIds { get; set; } = new();
    }

    public class SubmitLeaveRequestRequest : PrepareLeaveRequestWorkflowRequest
    {
        [MaxLength(4000)] public string? Comment { get; set; }
    }

    public class CancelLeaveRequestRequest
    {
        [Required, MaxLength(1000)] public string Reason { get; set; } = string.Empty;
        [MaxLength(100)] public string? IdempotencyKey { get; set; }
    }

    public class LeaveRequestQueryRequest
    {
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public Guid? LeaveTypeId { get; set; }
        public string? Status { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "createDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class LeaveRequestSummaryResponse
    {
        public int TotalRequest { get; set; }
        public int Draft { get; set; }
        public int WaitingApproval { get; set; }
        public int NeedRevision { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public int Cancelled { get; set; }
        public int UpcomingLeave { get; set; }
        public decimal TotalRequestedDays { get; set; }
        public decimal TotalApprovedDays { get; set; }
    }

    public class LeaveRequestListResponse
    {
        public Guid Id { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public Guid LeaveTypeId { get; set; }
        public string LeaveTypeCode { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public string? ColorCode { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public bool IsHalfDay { get; set; }
        public bool IsHourly { get; set; }
        public int? RequestedMinutes { get; set; }
        public decimal RequestedDays { get; set; }
        public decimal EstimatedBalanceDeduction { get; set; }
        public string LeaveRequestStatus { get; set; } = string.Empty;
        public string? WorkflowStatus { get; set; }
        public bool HasAttachment { get; set; }
        public int AttachmentCount { get; set; }
        public bool IsReserved { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime CreateDateTime { get; set; }
        public List<string> AvailableActions { get; set; } = new();
    }

    public class LeaveRequestPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<LeaveRequestListResponse> Items { get; set; } = new();
    }

    public class LeaveRequestAttachmentResponse
    {
        public Guid Id { get; set; }
        public string AttachmentType { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string? ContentType { get; set; }
        public long FileSizeBytes { get; set; }
        public string VerificationStatus { get; set; } = string.Empty;
        public bool IsRequiredDocument { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class LeaveRequestTimelineResponse
    {
        public DateTime At { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsWorkflowEvent { get; set; }
    }

    public class LeaveRequestDetailResponse : LeaveRequestListResponse
    {
        public Guid WorkforceProfileId { get; set; }
        public string? WorkforceProfileCode { get; set; }
        public string? WorkforceDisplayName { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? LeavePolicyId { get; set; }
        public string? LeavePolicyCode { get; set; }
        public string? LeavePolicyName { get; set; }
        public Guid? LeaveBalanceId { get; set; }
        public decimal BalanceBeforeRequest { get; set; }
        public decimal EstimatedBalanceAfterRequest { get; set; }
        public decimal ActualBalanceDeduction { get; set; }
        public decimal CalculatedWorkingDays { get; set; }
        public decimal ExcludedHolidayDays { get; set; }
        public decimal ExcludedWeeklyOffDays { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public string? HalfDayPeriod { get; set; }
        public string Reason { get; set; } = string.Empty;
        public Guid? RequestReasonId { get; set; }
        public string? RequestReasonName { get; set; }
        public string? ContactAddressDuringLeave { get; set; }
        public string? ContactNumberDuringLeave { get; set; }
        public string? HandoverNotes { get; set; }
        public bool RequiresReplacement { get; set; }
        public Guid? ReplacementWorkforceProfileId { get; set; }
        public string? ReplacementWorkforceName { get; set; }
        public bool HasRosterConflict { get; set; }
        public bool HasTrainingConflict { get; set; }
        public bool HasCriticalStaffingImpact { get; set; }
        public string? BalanceSimulationJson { get; set; }
        public string? RosterImpactJson { get; set; }
        public string? ValidationResultJson { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public List<LeaveRequestAttachmentResponse> Attachments { get; set; } = new();
        public List<LeaveRequestCalculationDayResponse> CalculationDays { get; set; } = new();
        public List<LeaveRequestTimelineResponse> Timeline { get; set; } = new();
    }

    public class LeaveRequestActionResponse
    {
        public Guid Id { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string LeaveRequestStatus { get; set; } = string.Empty;
        public Guid? WorkflowInstanceId { get; set; }
        public string? WorkflowStatus { get; set; }
        public bool IsReserved { get; set; }
        public bool IsIdempotent { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class LeaveRequestFileDownloadResponse
    {
        public string PhysicalPath { get; set; } = string.Empty;
        public string DownloadFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/octet-stream";
    }
}
