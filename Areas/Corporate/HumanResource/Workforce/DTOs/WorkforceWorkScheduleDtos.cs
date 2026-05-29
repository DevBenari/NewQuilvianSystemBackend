using QuilvianSystemBackend.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public class WorkforceWorkScheduleSummaryResponse
    {
        public int TotalSchedule { get; set; }
        public int ActiveSchedule { get; set; }
        public int InactiveSchedule { get; set; }

        public int WorkingDaySchedule { get; set; }
        public int OffDaySchedule { get; set; }
        public int OvertimePlannedSchedule { get; set; }
        public int OnCallSchedule { get; set; }
        public int OvernightSchedule { get; set; }

        public int TodaySchedule { get; set; }
        public int UpcomingSchedule { get; set; }
        public int PastSchedule { get; set; }
    }

    public class WorkforceWorkScheduleListResponse
    {
        public Guid WorkforceProfileId { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public UserType UserType { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int WorkingDayData { get; set; }
        public int OffDayData { get; set; }
        public int OvertimePlannedData { get; set; }
        public int OnCallData { get; set; }

        public List<WorkforceWorkScheduleResponse> Items { get; set; } = new();
    }

    public class WorkforceWorkScheduleResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public UserType UserType { get; set; }

        public Guid WorkScheduleId { get; set; }
        public string WorkScheduleCode { get; set; } = string.Empty;
        public string WorkScheduleName { get; set; } = string.Empty;
        public string ScheduleType { get; set; } = string.Empty;

        public DateTime ScheduleDate { get; set; }

        public string WorkStartTime { get; set; } = string.Empty;
        public string WorkEndTime { get; set; } = string.Empty;

        public bool IsOvernight { get; set; }
        public int CheckInToleranceMinutes { get; set; }
        public int CheckOutToleranceMinutes { get; set; }

        public DateTime? ScheduledCheckInAt { get; set; }
        public DateTime? ScheduledCheckOutAt { get; set; }

        public bool IsOffDay { get; set; }
        public bool IsOvertimePlanned { get; set; }
        public bool IsOnCall { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceWorkScheduleDetailResponse : WorkforceWorkScheduleResponse
    {
        public DateTime? UpdateDateTime { get; set; }

        public Guid CreateBy { get; set; }
        public Guid UpdateBy { get; set; }
    }

    public class WorkforceWorkScheduleCalendarResponse
    {
        public Guid WorkforceProfileId { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public UserType UserType { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }

        public int TotalData { get; set; }
        public int WorkingDayData { get; set; }
        public int OffDayData { get; set; }
        public int OvertimePlannedData { get; set; }
        public int OnCallData { get; set; }

        public List<WorkforceWorkScheduleResponse> Items { get; set; } = new();
    }

    public class WorkforceWorkScheduleOptionResponse
    {
        public Guid Id { get; set; }

        public string WorkScheduleCode { get; set; } = string.Empty;
        public string WorkScheduleName { get; set; } = string.Empty;
        public string ScheduleType { get; set; } = string.Empty;

        public string WorkStartTime { get; set; } = string.Empty;
        public string WorkEndTime { get; set; } = string.Empty;

        public bool IsOvernight { get; set; }
        public bool IsDefault { get; set; }

        public int CheckInToleranceMinutes { get; set; }
        public int CheckOutToleranceMinutes { get; set; }
    }

    public class WorkforceWorkScheduleFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string TimeFormat { get; set; } = "HH:mm:ss";

        public WorkforceWorkScheduleDefaultFilterResponse DefaultFilter { get; set; } = new();

        public List<WorkforceWorkScheduleSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();

        public List<string> ScheduleTypes { get; set; } = new()
        {
            "Shift",
            "NonShift",
            "OnCall",
            "Off"
        };
    }

    public class WorkforceWorkScheduleDefaultFilterResponse
    {
        public Guid? WorkforceProfileId { get; set; }
        public Guid? WorkScheduleId { get; set; }

        public string? Search { get; set; }
        public string? ScheduleType { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public bool? IsOffDay { get; set; }
        public bool? IsOvertimePlanned { get; set; }
        public bool? IsOnCall { get; set; }
        public bool? IsActive { get; set; }

        public string SortBy { get; set; } = "scheduleDate";
        public string SortDirection { get; set; } = "asc";

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 31;
    }

    public class WorkforceWorkScheduleSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateWorkforceWorkScheduleRequest
    {
        [Required]
        public Guid WorkScheduleId { get; set; }

        [Required]
        public DateTime ScheduleDate { get; set; }

        public bool IsOffDay { get; set; } = false;

        public bool IsOvertimePlanned { get; set; } = false;

        public bool IsOnCall { get; set; } = false;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceWorkScheduleRequest
    {
        [Required]
        public Guid WorkScheduleId { get; set; }

        [Required]
        public DateTime ScheduleDate { get; set; }

        public bool IsOffDay { get; set; } = false;

        public bool IsOvertimePlanned { get; set; } = false;

        public bool IsOnCall { get; set; } = false;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class BulkCreateWorkforceWorkScheduleRequest
    {
        public bool ReplaceExistingSameDate { get; set; } = false;

        [Required]
        [MinLength(1)]
        public List<CreateWorkforceWorkScheduleRequest> Items { get; set; } = new();
    }

    public class BulkUpdateWorkforceWorkScheduleRequest
    {
        [Required]
        [MinLength(1)]
        public List<BulkUpdateWorkforceWorkScheduleItemRequest> Items { get; set; } = new();
    }

    public class BulkUpdateWorkforceWorkScheduleItemRequest
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid WorkScheduleId { get; set; }

        [Required]
        public DateTime ScheduleDate { get; set; }

        public bool IsOffDay { get; set; } = false;

        public bool IsOvertimePlanned { get; set; } = false;

        public bool IsOnCall { get; set; } = false;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class GenerateWorkforceWorkScheduleRequest
    {
        [Required]
        public Guid WorkScheduleId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool ApplyMonday { get; set; } = true;
        public bool ApplyTuesday { get; set; } = true;
        public bool ApplyWednesday { get; set; } = true;
        public bool ApplyThursday { get; set; } = true;
        public bool ApplyFriday { get; set; } = true;
        public bool ApplySaturday { get; set; } = false;
        public bool ApplySunday { get; set; } = false;

        public bool IsOffDay { get; set; } = false;

        public bool IsOvertimePlanned { get; set; } = false;

        public bool IsOnCall { get; set; } = false;

        public bool ReplaceExistingSameDate { get; set; } = false;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class CopyWorkforceWorkScheduleRequest
    {
        [Required]
        public DateTime SourceStartDate { get; set; }

        [Required]
        public DateTime SourceEndDate { get; set; }

        [Required]
        public DateTime TargetStartDate { get; set; }

        public bool ReplaceExistingSameDate { get; set; } = false;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class DeleteWorkforceWorkScheduleRequest
    {
        [MaxLength(250)]
        public string? DeleteReason { get; set; }
    }
}