using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.DTOs
{
    public class ShiftSummaryResponse
    {
        public int TotalData
        {
            get;
            set;
        }
        public int ActiveData
        {
            get;
            set;
        }
        public int InactiveData
        {
            get;
            set;
        }
        public int DefaultData
        {
            get;
            set;
        }
    }
    public class ShiftResponse
    {
        public Guid Id
        {
            get;
            set;
        }
        public Guid? WorkScheduleId
        {
            get;
            set;
        }
        public string? WorkScheduleCode
        {
            get;
            set;
        }
        public string? WorkScheduleName
        {
            get;
            set;
        }
        public Guid? ShiftGroupId
        {
            get;
            set;
        }
        public string? ShiftGroupCode
        {
            get;
            set;
        }
        public string? ShiftGroupName
        {
            get;
            set;
        }
        public Guid? OnCallTypeId
        {
            get;
            set;
        }
        public string ShiftCode
        {
            get;
            set;
        }
        = string.Empty;
        public string ShiftName
        {
            get;
            set;
        }
        = string.Empty;
        public TimeOnly StartTime
        {
            get;
            set;
        }
        public TimeOnly EndTime
        {
            get;
            set;
        }
        public int BreakDurationMinutes
        {
            get;
            set;
        }
        public int PaidWorkMinutes
        {
            get;
            set;
        }
        public bool IsOvernight
        {
            get;
            set;
        }
        public bool IsNightShift
        {
            get;
            set;
        }
        public bool IsOnCall
        {
            get;
            set;
        }
        public bool IsOffShift
        {
            get;
            set;
        }
        public bool AllowOvertime
        {
            get;
            set;
        }
        public string? ColorCode
        {
            get;
            set;
        }
        public string? Description
        {
            get;
            set;
        }
        public bool IsActive
        {
            get;
            set;
        }
        public DateTime CreateDateTime
        {
            get;
            set;
        }
        public Guid? CreateBy
        {
            get;
            set;
        }
        public string? CreateByName
        {
            get;
            set;
        }
    }
    public class ShiftDetailResponse : ShiftResponse
    {
        public DateTime? UpdateDateTime
        {
            get;
            set;
        }
        public Guid? UpdateBy
        {
            get;
            set;
        }
        public string? UpdateByName
        {
            get;
            set;
        }
    }
    public class ShiftOptionResponse
    {
        public Guid Id
        {
            get;
            set;
        }
        public string Code
        {
            get;
            set;
        }
        = string.Empty;
        public string Name
        {
            get;
            set;
        }
        = string.Empty;
    }
    public class ShiftOptionPagedResponse
    {
        public int PageNumber
        {
            get;
            set;
        }
        public int PageSize
        {
            get;
            set;
        }
        public int TotalData
        {
            get;
            set;
        }
        public int TotalPage
        {
            get;
            set;
        }
        public List<ShiftOptionResponse> Items
        {
            get;
            set;
        }
        = new();
    }
    public class ShiftFilterMetadataResponse
    {
        public ShiftDefaultFilterResponse DefaultFilter
        {
            get;
            set;
        }
        = new();
        public List<ShiftCustomPeriodOptionResponse> CustomPeriods
        {
            get;
            set;
        }
        = new();
        public List<string> SortDirections
        {
            get;
            set;
        }
        = new();
        public List<int> PageSizeOptions
        {
            get;
            set;
        }
        = new();
    }
    public class ShiftDefaultFilterResponse
    {
        public DateTime? StartDate
        {
            get;
            set;
        }
        public DateTime? EndDate
        {
            get;
            set;
        }
        public string? CustomPeriod
        {
            get;
            set;
        }
        public bool? IsActive
        {
            get;
            set;
        }
        public string? Search
        {
            get;
            set;
        }
        public string SortBy
        {
            get;
            set;
        }
        = "name";
        public string SortDirection
        {
            get;
            set;
        }
        = "asc";
        public int PageNumber
        {
            get;
            set;
        }
        = 1;
        public int PageSize
        {
            get;
            set;
        }
        = 25;
    }

    public class ShiftCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }
    public class CreateShiftRequest
    {
        public Guid? WorkScheduleId
        {
            get;
            set;
        }
        public Guid? ShiftGroupId
        {
            get;
            set;
        }
        public Guid? OnCallTypeId
        {
            get;
            set;
        }
        [Required, MaxLength(200)] public string ShiftName
        {
            get;
            set;
        }
        = string.Empty;
        public TimeOnly StartTime
        {
            get;
            set;
        }
        public TimeOnly EndTime
        {
            get;
            set;
        }
        public int BreakDurationMinutes
        {
            get;
            set;
        }
        public int PaidWorkMinutes
        {
            get;
            set;
        }
        public bool IsOvernight
        {
            get;
            set;
        }
        public bool IsNightShift
        {
            get;
            set;
        }
        public bool IsOnCall
        {
            get;
            set;
        }
        public bool IsOffShift
        {
            get;
            set;
        }
        public bool AllowOvertime
        {
            get;
            set;
        }
        public string? ColorCode
        {
            get;
            set;
        }
        public string? Description
        {
            get;
            set;
        }
        public bool IsActive
        {
            get;
            set;
        }
    }
    public class UpdateShiftRequest : CreateShiftRequest
    {
        public bool IsActive
        {
            get;
            set;
        }
        = true;
    }
    public class UpdateShiftStatusRequest
    {
        public bool IsActive
        {
            get;
            set;
        }
    }
}
