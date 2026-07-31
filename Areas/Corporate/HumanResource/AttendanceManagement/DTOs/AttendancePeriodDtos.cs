using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs
{
    public class AttendancePeriodMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public List<AttendanceStringOptionResponse> PeriodStatusOptions { get; set; } = new();
        public List<AttendanceSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class AttendanceStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class AttendanceSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class AttendancePeriodQueryRequest
    {
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? PeriodStatus { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public bool? RequirePayrollHandoff { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "startDate";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class AttendancePeriodSummaryResponse
    {
        public int TotalPeriod { get; set; }
        public int OpenPeriod { get; set; }
        public int ClosedPeriod { get; set; }
        public int ReopenedPeriod { get; set; }
        public int CancelledPeriod { get; set; }
        public int ScheduledToClose { get; set; }
    }

    public class AttendancePeriodResponse
    {
        public Guid Id { get; set; }
        public string PeriodCode { get; set; } = string.Empty;
        public string PeriodName { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int TotalDays { get; set; }
        public Guid? LegalEntityId { get; set; }
        public string? LegalEntityName { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public string? HospitalSiteName { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string PeriodStatus { get; set; } = string.Empty;
        public bool RequirePayrollHandoff { get; set; }
        public DateTime? ScheduledCloseAt { get; set; }
        public DateTime? LastValidatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public DateTime? ReopenedAt { get; set; }
        public int ReopenCount { get; set; }
        public int AttendanceDailyCount { get; set; }
        public int SchedulerJobCount { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class AttendancePeriodDetailResponse : AttendancePeriodResponse
    {
        public Guid? LastProcessingRunId { get; set; }
        public string? LastProcessingRunNumber { get; set; }
        public string? ValidationSnapshotJson { get; set; }
        public Guid? ClosedByUserId { get; set; }
        public string? ClosedByUserName { get; set; }
        public string? CloseReason { get; set; }
        public Guid? ReopenedByUserId { get; set; }
        public string? ReopenedByUserName { get; set; }
        public string? ReopenReason { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class AttendancePeriodPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<AttendancePeriodResponse> Items { get; set; } = new();
    }

    public class AttendancePeriodOptionResponse
    {
        public Guid Id { get; set; }
        public string PeriodCode { get; set; } = string.Empty;
        public string PeriodName { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string PeriodStatus { get; set; } = string.Empty;
    }

    public class CreateAttendancePeriodRequest
    {
        [Required, MaxLength(200)]
        public string PeriodName { get; set; } = string.Empty;

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }

        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public bool RequirePayrollHandoff { get; set; } = true;
        public DateTime? ScheduledCloseAt { get; set; }
    }

    public class UpdateAttendancePeriodRequest : CreateAttendancePeriodRequest
    {
    }

    public class CloseAttendancePeriodRequest
    {
        [Required, MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }

    public class ReopenAttendancePeriodRequest
    {
        [Required, MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }

    public class CancelAttendancePeriodRequest
    {
        [Required, MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }

    public class AttendancePeriodValidationIssueResponse
    {
        public string Code { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int Count { get; set; }
        public bool IsBlocking { get; set; }
    }

    public class AttendancePeriodClosePreviewResponse
    {
        public Guid AttendancePeriodId { get; set; }
        public string PeriodCode { get; set; } = string.Empty;
        public string PeriodStatus { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public bool CanClose { get; set; }
        public int TotalAttendanceDaily { get; set; }
        public int ProcessedAttendanceDaily { get; set; }
        public int UnprocessedAttendanceDaily { get; set; }
        public int PendingPayrollHandoff { get; set; }
        public int OpenPayrollBlockingException { get; set; }
        public int ActiveCorrectionRequest { get; set; }
        public int PendingRawLog { get; set; }
        public int RunningProcessingRun { get; set; }
        public int RunningSchedulerJob { get; set; }
        public int LinkedToOtherPeriod { get; set; }
        public DateTime ValidatedAt { get; set; }
        public List<AttendancePeriodValidationIssueResponse> Issues { get; set; } = new();
    }

    public class AttendancePeriodActionResponse
    {
        public Guid AttendancePeriodId { get; set; }
        public string PeriodCode { get; set; } = string.Empty;
        public string PreviousStatus { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;
        public int AffectedAttendanceDailyCount { get; set; }
        public DateTime ActionAt { get; set; }
    }
}
