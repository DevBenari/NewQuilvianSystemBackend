using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.DTOs
{
    public class ShiftPatternSummaryResponse
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
    public class ShiftPatternResponse
    {
        public Guid Id
        {
            get;
            set;
        }
        public Guid ShiftGroupId
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
        public string ShiftPatternCode
        {
            get;
            set;
        }
        = string.Empty;
        public string ShiftPatternName
        {
            get;
            set;
        }
        = string.Empty;
        public int CycleLengthDays
        {
            get;
            set;
        }
        public string PatternDefinitionJson
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
    public class ShiftPatternDetailResponse : ShiftPatternResponse
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
    public class ShiftPatternOptionResponse
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
    public class ShiftPatternOptionPagedResponse
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
        public List<ShiftPatternOptionResponse> Items
        {
            get;
            set;
        }
        = new();
    }
    public class ShiftPatternFilterMetadataResponse
    {
        public ShiftPatternDefaultFilterResponse DefaultFilter
        {
            get;
            set;
        }
        = new();
        public List<ShiftPatternCustomPeriodOptionResponse> CustomPeriods
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
    public class ShiftPatternDefaultFilterResponse
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

    public class ShiftPatternCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }
    public class CreateShiftPatternRequest
    {
        public Guid ShiftGroupId
        {
            get;
            set;
        }
        [Required, MaxLength(200)] public string ShiftPatternName
        {
            get;
            set;
        }
        = string.Empty;
        public int CycleLengthDays
        {
            get;
            set;
        }
        [Required] public string PatternDefinitionJson
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
    public class UpdateShiftPatternRequest : CreateShiftPatternRequest
    {
        public bool IsActive
        {
            get;
            set;
        }
        = true;
    }
    public class UpdateShiftPatternStatusRequest
    {
        public bool IsActive
        {
            get;
            set;
        }
    }
}
