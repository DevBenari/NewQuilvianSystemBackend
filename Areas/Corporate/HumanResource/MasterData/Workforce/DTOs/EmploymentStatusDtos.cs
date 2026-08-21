using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.DTOs
{
    public class EmploymentStatusSummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int ActiveEmploymentData { get; set; }
        public int TerminalStatusData { get; set; }
    }

    public class EmploymentStatusResponse
    {
        public Guid Id { get; set; }
        public string EmploymentStatusCode { get; set; } = string.Empty;
        public string EmploymentStatusName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActiveEmployment { get; set; }
        public bool IsSchedulable { get; set; }
        public bool IsPayrollEligible { get; set; }
        public bool IsTerminalStatus { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class EmploymentStatusDetailResponse : EmploymentStatusResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class EmploymentStatusOptionResponse
    {
        public Guid Id { get; set; }
        public string EmploymentStatusCode { get; set; } = string.Empty;
        public string EmploymentStatusName { get; set; } = string.Empty;
        public bool IsActiveEmployment { get; set; }
        public bool IsTerminalStatus { get; set; }
    }

    public class EmploymentStatusOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<EmploymentStatusOptionResponse> Items { get; set; } = new();
    }

    public class EmploymentStatusFilterMetadataResponse
    {
        public EmploymentStatusDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<EmploymentStatusCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<EmploymentStatusSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class EmploymentStatusDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public bool? IsActiveEmployment { get; set; }
        public bool? IsTerminalStatus { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "employmentStatusName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class EmploymentStatusCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class EmploymentStatusSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateEmploymentStatusRequest
    {
        [Required, MaxLength(150)]
        public string EmploymentStatusName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActiveEmployment { get; set; } = true;
        public bool IsSchedulable { get; set; } = true;
        public bool IsPayrollEligible { get; set; } = true;
        public bool IsTerminalStatus { get; set; }

        [Range(0, int.MaxValue)]
        public int? SortOrder { get; set; }
    }

    public class UpdateEmploymentStatusRequest : CreateEmploymentStatusRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateEmploymentStatusStatusRequest
    {
        public bool IsActive { get; set; }
    }
}
