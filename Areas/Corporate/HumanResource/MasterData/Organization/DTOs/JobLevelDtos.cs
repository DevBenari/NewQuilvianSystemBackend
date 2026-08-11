using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.DTOs
{
    public class JobLevelSummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int WithEmployeeGradeData { get; set; }
    }

    public class JobLevelResponse
    {
        public Guid Id { get; set; }
        public string JobLevelCode { get; set; } = string.Empty;
        public string JobLevelName { get; set; } = string.Empty;
        public int LevelOrder { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int EmployeeGradeCount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
    }

    public class JobLevelDetailResponse : JobLevelResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
    }

    public class JobLevelOptionResponse
    {
        public Guid Id { get; set; }
        public string JobLevelCode { get; set; } = string.Empty;
        public string JobLevelName { get; set; } = string.Empty;
        public int LevelOrder { get; set; }
    }

    public class JobLevelOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<JobLevelOptionResponse> Items { get; set; } = new();
    }

    public class JobLevelFilterMetadataResponse
    {
        public JobLevelDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<JobLevelCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<JobLevelSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class JobLevelDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "levelOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class JobLevelCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class JobLevelSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateJobLevelRequest
    {
        [Required, MaxLength(150)]
        public string JobLevelName { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int LevelOrder { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateJobLevelRequest : CreateJobLevelRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateJobLevelStatusRequest
    {
        public bool IsActive { get; set; }
    }
}
