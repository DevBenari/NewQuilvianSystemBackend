using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.DTOs
{
    public class ShiftGroupSummaryResponse
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
    public class ShiftGroupResponse
    {
        public Guid Id
        {
            get;
            set;
        }
        public string ShiftGroupCode
        {
            get;
            set;
        }
        = string.Empty;
        public string ShiftGroupName
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
        public bool IsRotating
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
    public class ShiftGroupDetailResponse : ShiftGroupResponse
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
    public class ShiftGroupOptionResponse
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
    public class ShiftGroupOptionPagedResponse
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
        public List<ShiftGroupOptionResponse> Items
        {
            get;
            set;
        }
        = new();
    }
    public class ShiftGroupFilterMetadataResponse
    {
        public ShiftGroupDefaultFilterResponse DefaultFilter
        {
            get;
            set;
        }
        = new();
        public List<ShiftGroupCustomPeriodOptionResponse> CustomPeriods
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
    public class ShiftGroupDefaultFilterResponse
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

    public class ShiftGroupCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }
    public class CreateShiftGroupRequest
    {
        [Required, MaxLength(150)]
        public string ShiftGroupName
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
        public bool IsRotating
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
    public class UpdateShiftGroupRequest : CreateShiftGroupRequest
    {
        public bool IsActive
        {
            get;
            set;
        }
        = true;
    }
    public class UpdateShiftGroupStatusRequest
    {
        public bool IsActive
        {
            get;
            set;
        }
    }
}
