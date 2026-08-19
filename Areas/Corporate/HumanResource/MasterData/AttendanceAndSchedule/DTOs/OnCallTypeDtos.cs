using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.DTOs
{
    public class OnCallTypeSummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int RemoteAllowedData { get; set; }
        public int AllowanceEligibleData { get; set; }
    }

    public class OnCallTypeResponse
    {
        public Guid Id { get; set; }
        public string OnCallTypeCode { get; set; } = string.Empty;
        public string OnCallTypeName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int ResponseTimeMinutes { get; set; }
        public int MinimumCallHours { get; set; }
        public int MaximumCallHours { get; set; }
        public bool IsRemoteAllowed { get; set; }
        public bool RequiresOnSitePresence { get; set; }
        public bool CountsAsWorkingTime { get; set; }
        public bool IsAllowanceEligible { get; set; }
        public bool IsActive { get; set; }
        public int ShiftCount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class OnCallTypeDetailResponse : OnCallTypeResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class OnCallTypeOptionResponse
    {
        public Guid Id { get; set; }
        public string OnCallTypeCode { get; set; } = string.Empty;
        public string OnCallTypeName { get; set; } = string.Empty;
        public bool IsRemoteAllowed { get; set; }
        public bool IsAllowanceEligible { get; set; }
    }

    public class OnCallTypeOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<OnCallTypeOptionResponse> Items { get; set; } = new();
    }

    public class OnCallTypeFilterMetadataResponse
    {
        public OnCallTypeDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<OnCallTypeCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<OnCallTypeSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class OnCallTypeDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public bool? IsRemoteAllowed { get; set; }
        public bool? IsAllowanceEligible { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "onCallTypeName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class OnCallTypeCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class OnCallTypeSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateOnCallTypeRequest
    {
        [Required, MaxLength(150)]
        public string OnCallTypeName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Range(0, int.MaxValue)]
        public int ResponseTimeMinutes { get; set; }

        [Range(0, int.MaxValue)]
        public int MinimumCallHours { get; set; }

        [Range(0, int.MaxValue)]
        public int MaximumCallHours { get; set; } = 24;

        public bool IsRemoteAllowed { get; set; }
        public bool RequiresOnSitePresence { get; set; } = true;
        public bool CountsAsWorkingTime { get; set; } = true;
        public bool IsAllowanceEligible { get; set; } = true;
    }

    public class UpdateOnCallTypeRequest : CreateOnCallTypeRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateOnCallTypeStatusRequest
    {
        public bool IsActive { get; set; }
    }
}
