using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.DTOs
{
    public class WorkScheduleSummaryResponse
    {
        public int TotalWorkSchedule { get; set; }

        public int ActiveWorkSchedule { get; set; }

        public int InactiveWorkSchedule { get; set; }

        public int DefaultWorkSchedule { get; set; }

        public int ShiftSchedule { get; set; }

        public int NonShiftSchedule { get; set; }

        public int OnCallSchedule { get; set; }

        public int OffSchedule { get; set; }

        public int OvernightSchedule { get; set; }

        public int WithCheckInToleranceSchedule { get; set; }

        public int WithCheckOutToleranceSchedule { get; set; }
    }

    public class WorkScheduleResponse
    {
        public Guid Id { get; set; }

        public string ScheduleCode { get; set; } = string.Empty;

        public string ScheduleName { get; set; } = string.Empty;

        public string ScheduleType { get; set; } = string.Empty;

        public string WorkStartTime { get; set; } = string.Empty;

        public string WorkEndTime { get; set; } = string.Empty;

        public bool IsOvernight { get; set; }

        public int CheckInToleranceMinutes { get; set; }

        public int CheckOutToleranceMinutes { get; set; }

        public bool IsDefault { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkScheduleDetailResponse : WorkScheduleResponse
    {
        public DateTime? UpdateDateTime { get; set; }

        public Guid CreateBy { get; set; }

        public Guid UpdateBy { get; set; }
    }

    public class WorkScheduleOptionResponse
    {
        public Guid Id { get; set; }

        public string ScheduleCode { get; set; } = string.Empty;

        public string ScheduleName { get; set; } = string.Empty;

        public string ScheduleType { get; set; } = string.Empty;

        public string WorkStartTime { get; set; } = string.Empty;

        public string WorkEndTime { get; set; } = string.Empty;

        public bool IsOvernight { get; set; }

        public int CheckInToleranceMinutes { get; set; }

        public int CheckOutToleranceMinutes { get; set; }

        public bool IsDefault { get; set; }
    }

    public class WorkScheduleFilterMetadataResponse
    {
        public string TimeFormat { get; set; } = "HH:mm:ss";

        public WorkScheduleDefaultFilterResponse DefaultFilter { get; set; } = new();

        public List<WorkScheduleSortOptionResponse> SortOptions { get; set; } = new();

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

    public class WorkScheduleDefaultFilterResponse
    {
        public string? Search { get; set; }

        public string? ScheduleType { get; set; }

        public bool? IsOvernight { get; set; }

        public bool? IsDefault { get; set; }

        public bool? IsActive { get; set; }

        public string SortBy { get; set; } = "scheduleCode";

        public string SortDirection { get; set; } = "asc";

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    public class WorkScheduleSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class CreateWorkScheduleRequest
    {
        [Required]
        [MaxLength(50)]
        public string ScheduleCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string ScheduleName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ScheduleType { get; set; } = "Shift";

        [Required]
        [RegularExpression(
            @"^([01]\d|2[0-3]):[0-5]\d(:[0-5]\d)?$",
            ErrorMessage = "WorkStartTime harus menggunakan format HH:mm atau HH:mm:ss."
        )]
        public string WorkStartTime { get; set; } = "08:00:00";

        [Required]
        [RegularExpression(
            @"^([01]\d|2[0-3]):[0-5]\d(:[0-5]\d)?$",
            ErrorMessage = "WorkEndTime harus menggunakan format HH:mm atau HH:mm:ss."
        )]
        public string WorkEndTime { get; set; } = "17:00:00";

        public bool IsOvernight { get; set; } = false;

        [Range(0, 1440)]
        public int CheckInToleranceMinutes { get; set; } = 0;

        [Range(0, 1440)]
        public int CheckOutToleranceMinutes { get; set; } = 0;

        public bool IsDefault { get; set; } = false;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkScheduleRequest
    {
        [Required]
        [MaxLength(50)]
        public string ScheduleCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string ScheduleName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ScheduleType { get; set; } = "Shift";

        [Required]
        [RegularExpression(
            @"^([01]\d|2[0-3]):[0-5]\d(:[0-5]\d)?$",
            ErrorMessage = "WorkStartTime harus menggunakan format HH:mm atau HH:mm:ss."
        )]
        public string WorkStartTime { get; set; } = "08:00:00";

        [Required]
        [RegularExpression(
            @"^([01]\d|2[0-3]):[0-5]\d(:[0-5]\d)?$",
            ErrorMessage = "WorkEndTime harus menggunakan format HH:mm atau HH:mm:ss."
        )]
        public string WorkEndTime { get; set; } = "17:00:00";

        public bool IsOvernight { get; set; } = false;

        [Range(0, 1440)]
        public int CheckInToleranceMinutes { get; set; } = 0;

        [Range(0, 1440)]
        public int CheckOutToleranceMinutes { get; set; } = 0;

        public bool IsDefault { get; set; } = false;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkScheduleStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class SetWorkScheduleDefaultRequest
    {
        public bool IsDefault { get; set; } = true;
    }

    public class DeleteWorkScheduleRequest
    {
        [MaxLength(250)]
        public string? DeleteReason { get; set; }
    }
}