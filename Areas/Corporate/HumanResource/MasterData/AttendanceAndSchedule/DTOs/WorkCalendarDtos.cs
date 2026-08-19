using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.DTOs
{
    public class WorkCalendarSummaryResponse
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
    public class WorkCalendarResponse
    {
        public Guid Id
        {
            get;
            set;
        }
        public Guid? HospitalSiteId
        {
            get;
            set;
        }
        public string? HospitalSiteCode
        {
            get;
            set;
        }
        public string? HospitalSiteName
        {
            get;
            set;
        }
        public string WorkCalendarCode
        {
            get;
            set;
        }
        = string.Empty;
        public string WorkCalendarName
        {
            get;
            set;
        }
        = string.Empty;
        public int CalendarYear
        {
            get;
            set;
        }
        public DateTime StartDate
        {
            get;
            set;
        }
        public DateTime EndDate
        {
            get;
            set;
        }
        public string TimeZoneId
        {
            get;
            set;
        }
        = string.Empty;
        public string? Description
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
    public class WorkCalendarDetailResponse : WorkCalendarResponse
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
    public class WorkCalendarOptionResponse
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
    public class WorkCalendarOptionPagedResponse
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
        public List<WorkCalendarOptionResponse> Items
        {
            get;
            set;
        }
        = new();
    }
    public class WorkCalendarFilterMetadataResponse
    {
        public WorkCalendarDefaultFilterResponse DefaultFilter
        {
            get;
            set;
        }
        = new();
        public List<WorkCalendarCustomPeriodOptionResponse> CustomPeriods
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
    public class WorkCalendarDefaultFilterResponse
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

    public class WorkCalendarCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }
    public class CreateWorkCalendarRequest
    {
        public Guid? HospitalSiteId
        {
            get;
            set;
        }
        [Required, MaxLength(200)] public string WorkCalendarName
        {
            get;
            set;
        }
        = string.Empty;
        public int CalendarYear
        {
            get;
            set;
        }
        public DateTime StartDate
        {
            get;
            set;
        }
        public DateTime EndDate
        {
            get;
            set;
        }
        [Required, MaxLength(100)] public string TimeZoneId
        {
            get;
            set;
        }
        = string.Empty;
        public string? Description
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
    public class UpdateWorkCalendarRequest : CreateWorkCalendarRequest
    {
        public bool IsActive
        {
            get;
            set;
        }
        = true;
    }
    public class UpdateWorkCalendarStatusRequest
    {
        public bool IsActive
        {
            get;
            set;
        }
    }
}
