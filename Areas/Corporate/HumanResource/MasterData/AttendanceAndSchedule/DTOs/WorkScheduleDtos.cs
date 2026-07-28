using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.DTOs
{
    public class WorkScheduleSummaryResponse
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
    public class WorkScheduleResponse
    {
        public Guid Id
        {
            get;
            set;
        }
        public string ScheduleCode
        {
            get;
            set;
        }
        = string.Empty;
        public string ScheduleName
        {
            get;
            set;
        }
        = string.Empty;
        public string ScheduleType
        {
            get;
            set;
        }
        = string.Empty;
        public TimeOnly WorkStartTime
        {
            get;
            set;
        }
        public TimeOnly WorkEndTime
        {
            get;
            set;
        }
        public bool IsOvernight
        {
            get;
            set;
        }
        public int CheckInToleranceMinutes
        {
            get;
            set;
        }
        public int CheckOutToleranceMinutes
        {
            get;
            set;
        }
        public bool IsDefault
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
    public class WorkScheduleDetailResponse : WorkScheduleResponse
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
    public class WorkScheduleOptionResponse
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
    public class WorkScheduleOptionPagedResponse
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
        public List<WorkScheduleOptionResponse> Items
        {
            get;
            set;
        }
        = new();
    }
    public class WorkScheduleFilterMetadataResponse
    {
        public WorkScheduleDefaultFilterResponse DefaultFilter
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
    public class WorkScheduleDefaultFilterResponse
    {
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
    public class CreateWorkScheduleRequest
    {
        [Required, MaxLength(200)] public string ScheduleName
        {
            get;
            set;
        }
        = string.Empty;
        [Required, MaxLength(100)] public string ScheduleType
        {
            get;
            set;
        }
        = string.Empty;
        public TimeOnly WorkStartTime
        {
            get;
            set;
        }
        public TimeOnly WorkEndTime
        {
            get;
            set;
        }
        public bool IsOvernight
        {
            get;
            set;
        }
        public int CheckInToleranceMinutes
        {
            get;
            set;
        }
        public int CheckOutToleranceMinutes
        {
            get;
            set;
        }
        public bool IsDefault
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
    public class UpdateWorkScheduleRequest : CreateWorkScheduleRequest
    {
        public bool IsActive
        {
            get;
            set;
        }
        = true;
    }
    public class UpdateWorkScheduleStatusRequest
    {
        public bool IsActive
        {
            get;
            set;
        }
    }
}
