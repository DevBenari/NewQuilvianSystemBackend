using QuilvianSystemBackend.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs
{
    public class AttendanceDailyFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public AttendanceDailyDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<AttendanceDailyOptionResponse> CustomPeriods { get; set; } = new();
        public List<AttendanceDailyOptionResponse> AttendanceStatusOptions { get; set; } = new();
        public List<AttendanceDailyOptionResponse> ProcessingStatusOptions { get; set; } = new();
        public List<AttendanceDailyOptionResponse> PayrollInputStatusOptions { get; set; } = new();
        public List<AttendanceDailyOptionResponse> ScheduleSourceOptions { get; set; } = new();
        public List<AttendanceDailyOptionResponse> DueExceptionOptions { get; set; } = new();
        public List<AttendanceDailyOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class AttendanceDailyDefaultFilterResponse
    {
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? WorkLocationId { get; set; }
        public Guid? WorkScheduleId { get; set; }
        public Guid? ShiftId { get; set; }
        public string? AttendanceStatus { get; set; }
        public string? ProcessingStatus { get; set; }
        public string? PayrollInputStatus { get; set; }
        public string? ScheduleSource { get; set; }
        public bool? IsLate { get; set; }
        public bool? IsEarlyLeave { get; set; }
        public bool? HasMissingPunch { get; set; }
        public bool? IsHoliday { get; set; }
        public bool? IsRestDay { get; set; }
        public bool? IsCorrected { get; set; }
        public bool? IsLocked { get; set; }
        public bool? IsPayrollEligible { get; set; }
        public bool? HasOpenException { get; set; }
        public bool? HasPayrollBlockingException { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "attendanceDate";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class AttendanceDailyOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class AttendanceDailyQueryRequest : AttendanceDailyDefaultFilterResponse
    {
    }

    public class AttendanceDailySummaryResponse
    {
        public int TotalAttendance { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public int EarlyLeaveCount { get; set; }
        public int MissingPunchCount { get; set; }
        public int HolidayCount { get; set; }
        public int RestDayCount { get; set; }
        public int BusinessTripCount { get; set; }
        public int RemoteAttendanceCount { get; set; }
        public int CorrectedCount { get; set; }
        public int LockedCount { get; set; }
        public int OpenExceptionCount { get; set; }
        public int PayrollBlockingCount { get; set; }
        public int PayrollReadyCount { get; set; }
        public int PayrollProcessedCount { get; set; }
        public int PayrollPendingCount { get; set; }
        public long TotalScheduledWorkMinutes { get; set; }
        public long TotalActualWorkMinutes { get; set; }
        public long TotalPayableWorkMinutes { get; set; }
        public long TotalOvertimeMinutes { get; set; }
        public decimal AttendanceRatePercentage { get; set; }
        public decimal LateRatePercentage { get; set; }
    }

    public class AttendanceDailyResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? DoctorId { get; set; }
        public UserType UserType { get; set; }
        public string UserTypeName { get; set; } = string.Empty;
        public string? ProfileCode { get; set; }
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public string? EmployeeCode { get; set; }
        public string? EmployeeNumber { get; set; }
        public string? DoctorCode { get; set; }
        public string? DoctorNumber { get; set; }

        public Guid? HospitalSiteId { get; set; }
        public string? HospitalSiteName { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? WorkLocationId { get; set; }

        public Guid? WorkScheduleId { get; set; }
        public string? WorkScheduleName { get; set; }
        public Guid? ShiftId { get; set; }
        public string? ShiftName { get; set; }
        public Guid? PrimaryShiftAssignmentId { get; set; }
        public DateOnly AttendanceDate { get; set; }
        public string ScheduleSource { get; set; } = string.Empty;

        public DateTime? ScheduledCheckInAt { get; set; }
        public DateTime? ScheduledCheckOutAt { get; set; }
        public DateTime? FirstCheckInAt { get; set; }
        public DateTime? LastCheckOutAt { get; set; }

        public bool IsOvernightSchedule { get; set; }
        public bool IsHoliday { get; set; }
        public bool IsRestDay { get; set; }
        public bool IsPresent { get; set; }
        public bool IsAbsent { get; set; }
        public bool IsLate { get; set; }
        public bool IsEarlyLeave { get; set; }
        public bool HasMissingPunch { get; set; }
        public bool IsBusinessTrip { get; set; }
        public bool IsRemoteAttendance { get; set; }
        public bool IsCorrected { get; set; }
        public bool IsLocked { get; set; }

        public int ScheduledWorkMinutes { get; set; }
        public int ActualWorkMinutes { get; set; }
        public int BreakMinutes { get; set; }
        public int PayableWorkMinutes { get; set; }
        public int LateMinutes { get; set; }
        public int EarlyLeaveMinutes { get; set; }
        public int OvertimeMinutes { get; set; }
        public int NightWorkMinutes { get; set; }

        public string AttendanceStatus { get; set; } = string.Empty;
        public string ProcessingStatus { get; set; } = string.Empty;
        public int ProcessingVersion { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? ProcessingMessage { get; set; }

        public bool IsPayrollEligible { get; set; }
        public string PayrollInputStatus { get; set; } = string.Empty;
        public DateTime? PayrollProcessedAt { get; set; }
        public int SegmentCount { get; set; }
        public int SourceLogCount { get; set; }
        public int ExceptionCount { get; set; }
        public int OpenExceptionCount { get; set; }
        public int PayrollBlockingExceptionCount { get; set; }
        public int CorrectionRequestCount { get; set; }
        public bool IsPayrollReady { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class AttendanceDailyPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<AttendanceDailyResponse> Items { get; set; } = new();
    }

    public class AttendanceDailyDetailResponse : AttendanceDailyResponse
    {
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? WorkScheduleAssignmentId { get; set; }
        public Guid? AttendancePolicyId { get; set; }
        public string? AttendancePolicyName { get; set; }
        public Guid? GracePeriodPolicyId { get; set; }
        public string? GracePeriodPolicyName { get; set; }
        public Guid? PayrollPeriodId { get; set; }
        public string? ScheduleResolutionJson { get; set; }
        public bool CanRequestCorrection { get; set; }
        public string? CorrectionRestrictionReason { get; set; }
        public List<AttendanceDailySegmentResponse> Segments { get; set; } = new();
        public List<AttendanceDailyExceptionResponse> Exceptions { get; set; } = new();
        public List<AttendanceDailyRawLogResponse> RawLogs { get; set; } = new();
        public List<AttendanceDailyCorrectionRequestResponse> CorrectionRequests { get; set; } = new();
    }

    public class AttendanceDailySegmentResponse
    {
        public Guid Id { get; set; }
        public Guid AttendanceDailyId { get; set; }
        public Guid? ShiftAssignmentId { get; set; }
        public int SegmentOrder { get; set; }
        public string SegmentType { get; set; } = string.Empty;
        public string SegmentSource { get; set; } = string.Empty;
        public DateTime? ScheduledStartAt { get; set; }
        public DateTime? ScheduledEndAt { get; set; }
        public DateTime? ActualStartAt { get; set; }
        public DateTime? ActualEndAt { get; set; }
        public Guid? StartRawLogId { get; set; }
        public Guid? EndRawLogId { get; set; }
        public int ScheduledMinutes { get; set; }
        public int ActualMinutes { get; set; }
        public int BreakMinutes { get; set; }
        public int PayableMinutes { get; set; }
        public int LateMinutes { get; set; }
        public int EarlyLeaveMinutes { get; set; }
        public int OvertimeMinutes { get; set; }
        public bool IsOvernight { get; set; }
        public bool IsCorrected { get; set; }
        public string SegmentStatus { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class AttendanceDailyExceptionResponse
    {
        public Guid Id { get; set; }
        public Guid AttendanceDailyId { get; set; }
        public Guid? CorrectionRequestId { get; set; }
        public string ExceptionCode { get; set; } = string.Empty;
        public string ExceptionType { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string ExceptionStatus { get; set; } = string.Empty;
        public DateTime DetectedAt { get; set; }
        public DateTime? ExpectedAt { get; set; }
        public DateTime? ActualAt { get; set; }
        public int? DifferenceMinutes { get; set; }
        public bool IsAutoDetected { get; set; }
        public bool IsPayrollBlocking { get; set; }
        public string? DetectionRule { get; set; }
        public string? Message { get; set; }
        public Guid? ResolvedByUserId { get; set; }
        public string? ResolvedByUserName { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolutionNote { get; set; }
    }

    public class AttendanceDailyRawLogResponse
    {
        public Guid Id { get; set; }
        public Guid? AttendanceDeviceId { get; set; }
        public string? AttendanceDeviceName { get; set; }
        public Guid? AttendanceLocationId { get; set; }
        public string? AttendanceLocationName { get; set; }
        public string? ExternalLogId { get; set; }
        public string? ExternalDeviceId { get; set; }
        public string? DeviceUserKey { get; set; }
        public DateTime EventAt { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string SourceType { get; set; } = string.Empty;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? AccuracyMeters { get; set; }
        public decimal? DistanceMeters { get; set; }
        public string ProcessingStatus { get; set; } = string.Empty;
        public DateTime ReceivedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? ProcessingMessage { get; set; }
    }

    public class AttendanceDailyCorrectionRequestResponse
    {
        public Guid Id { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string CorrectionType { get; set; } = string.Empty;
        public string RequestStatus { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public Guid? WorkflowInstanceId { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime? AppliedAt { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class AttendancePayrollReadinessSummaryResponse
    {
        public int TotalAttendance { get; set; }
        public int EligibleCount { get; set; }
        public int ReadyCount { get; set; }
        public int PendingCount { get; set; }
        public int BlockedCount { get; set; }
        public int ProcessedCount { get; set; }
        public int ExcludedCount { get; set; }
        public int LockedCount { get; set; }
        public int PayrollBlockingExceptionCount { get; set; }
        public long TotalPayableWorkMinutes { get; set; }
        public long TotalOvertimeMinutes { get; set; }
    }

    public class AttendancePayrollReadinessResponse
    {
        public Guid AttendanceDailyId { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public string? ProfileCode { get; set; }
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public DateOnly AttendanceDate { get; set; }
        public string AttendanceStatus { get; set; } = string.Empty;
        public string ProcessingStatus { get; set; } = string.Empty;
        public bool IsPayrollEligible { get; set; }
        public string PayrollInputStatus { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
        public int PayableWorkMinutes { get; set; }
        public int OvertimeMinutes { get; set; }
        public int OpenExceptionCount { get; set; }
        public int PayrollBlockingExceptionCount { get; set; }
        public bool IsPayrollReady { get; set; }
        public List<string> BlockingReasons { get; set; } = new();
    }

    public class AttendancePayrollReadinessPagedResponse
    {
        public AttendancePayrollReadinessSummaryResponse Summary { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<AttendancePayrollReadinessResponse> Items { get; set; } = new();
    }

    public class AttendanceSelfServiceSummaryResponse
    {
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int TotalDay { get; set; }
        public int PresentDay { get; set; }
        public int AbsentDay { get; set; }
        public int LateDay { get; set; }
        public int EarlyLeaveDay { get; set; }
        public int MissingPunchDay { get; set; }
        public int HolidayDay { get; set; }
        public int RestDay { get; set; }
        public int CorrectedDay { get; set; }
        public int OpenExceptionCount { get; set; }
        public long TotalPayableWorkMinutes { get; set; }
        public long TotalOvertimeMinutes { get; set; }
        public decimal AttendanceRatePercentage { get; set; }
    }
}
