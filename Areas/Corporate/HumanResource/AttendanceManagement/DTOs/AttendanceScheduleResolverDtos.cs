using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs
{
    public class AttendanceScheduleResolverMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public int MaximumRangeDays { get; set; } = 31;
        public List<AttendanceScheduleStringOptionResponse> ScheduleSourceOptions { get; set; } = new();
        public List<AttendanceScheduleStringOptionResponse> ShiftAssignmentStatusOptions { get; set; } = new();
        public List<AttendanceScheduleStringOptionResponse> AssignmentTypeOptions { get; set; } = new();
    }

    public class AttendanceScheduleStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class AttendanceScheduleResolveRequest
    {
        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public DateOnly WorkDate { get; set; }
    }

    public class AttendanceScheduleRangeRequest
    {
        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }
    }

    public class AttendanceScheduleRangeResponse
    {
        public Guid WorkforceProfileId { get; set; }
        public string? WorkforceProfileCode { get; set; }
        public string? WorkforceDisplayName { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int TotalDate { get; set; }
        public int ResolvedDate { get; set; }
        public int UnresolvedDate { get; set; }
        public int RestDayCount { get; set; }
        public int HolidayCount { get; set; }
        public int BlockingConflictCount { get; set; }
        public List<AttendanceScheduleResolutionResponse> Items { get; set; } = new();
    }

    public class AttendanceScheduleResolutionResponse
    {
        public Guid WorkforceProfileId { get; set; }
        public string? WorkforceProfileCode { get; set; }
        public string? WorkforceDisplayName { get; set; }
        public DateOnly WorkDate { get; set; }

        public bool IsResolved { get; set; }
        public string ScheduleSource { get; set; } = "Unresolved";

        public Guid? PrimaryShiftAssignmentId { get; set; }
        public Guid? WorkScheduleAssignmentId { get; set; }
        public Guid? WorkScheduleId { get; set; }
        public string? WorkScheduleCode { get; set; }
        public string? WorkScheduleName { get; set; }
        public string? WorkScheduleType { get; set; }
        public Guid? ShiftId { get; set; }
        public string? ShiftCode { get; set; }
        public string? ShiftName { get; set; }

        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? WorkLocationId { get; set; }

        public string? PrimaryAssignmentType { get; set; }
        public string? PrimaryAssignmentStatus { get; set; }
        public string? PrimaryAssignmentSource { get; set; }

        public DateTime? ScheduledStartAt { get; set; }
        public DateTime? ScheduledEndAt { get; set; }
        public bool IsOvernight { get; set; }
        public bool IsRestDay { get; set; }
        public int BreakDurationMinutes { get; set; }
        public int PlannedWorkMinutes { get; set; }

        public Guid? WorkCalendarId { get; set; }
        public string? WorkCalendarCode { get; set; }
        public string? WorkCalendarName { get; set; }
        public string TimeZoneId { get; set; } = "Asia/Jakarta";
        public bool IsHoliday { get; set; }
        public List<AttendanceScheduleHolidayResponse> Holidays { get; set; } = new();

        public Guid? AttendancePolicyId { get; set; }
        public string? AttendancePolicyCode { get; set; }
        public string? AttendancePolicyName { get; set; }
        public bool RequireCheckIn { get; set; } = true;
        public bool RequireCheckOut { get; set; } = true;
        public bool AllowMultipleCheckInOut { get; set; }
        public bool IsOvertimeEnabled { get; set; }
        public int OvertimeThresholdMinutes { get; set; }
        public bool IsAttendanceLocationRequired { get; set; }
        public bool AllowManualCorrection { get; set; }
        public int CorrectionRequestLimitDays { get; set; }

        public Guid? GracePeriodPolicyId { get; set; }
        public string? GracePeriodPolicyCode { get; set; }
        public string? GracePeriodPolicyName { get; set; }
        public int EarlyCheckInMinutes { get; set; }
        public int LateCheckInGraceMinutes { get; set; }
        public int EarlyCheckOutGraceMinutes { get; set; }
        public int LateCheckOutMinutes { get; set; }

        public DateTime? EarliestCheckInAt { get; set; }
        public DateTime? LatestGraceCheckInAt { get; set; }
        public DateTime? EarliestGraceCheckOutAt { get; set; }
        public DateTime? LatestCheckOutAt { get; set; }

        public bool HasBlockingConflict { get; set; }
        public List<string> ConflictCodes { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public List<AttendanceScheduleAssignmentResponse> AdditionalAssignments { get; set; } = new();

        public string ResolutionSnapshotJson { get; set; } = "{}";
    }

    public class AttendanceScheduleAssignmentResponse
    {
        public Guid ShiftAssignmentId { get; set; }
        public Guid? WorkScheduleId { get; set; }
        public string? WorkScheduleCode { get; set; }
        public string? WorkScheduleName { get; set; }
        public Guid? ShiftId { get; set; }
        public string? ShiftCode { get; set; }
        public string? ShiftName { get; set; }
        public string AssignmentType { get; set; } = string.Empty;
        public string AssignmentStatus { get; set; } = string.Empty;
        public string AssignmentSource { get; set; } = string.Empty;
        public DateTime ScheduledStartAt { get; set; }
        public DateTime ScheduledEndAt { get; set; }
        public int BreakDurationMinutes { get; set; }
        public int PlannedWorkMinutes { get; set; }
        public bool IsNightShift { get; set; }
        public bool IsOnCall { get; set; }
        public bool IsDayOff { get; set; }
        public bool IsManualOverride { get; set; }
        public bool HasBlockingConflict { get; set; }
    }

    public class AttendanceScheduleHolidayResponse
    {
        public Guid Id { get; set; }
        public string HolidayCode { get; set; } = string.Empty;
        public string HolidayName { get; set; } = string.Empty;
        public string HolidayType { get; set; } = string.Empty;
        public bool IsNationalHoliday { get; set; }
        public bool IsPaidHoliday { get; set; }
    }
}
