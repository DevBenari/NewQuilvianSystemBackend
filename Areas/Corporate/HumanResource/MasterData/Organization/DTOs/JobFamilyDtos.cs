using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.DTOs
{
    public class JobFamilySummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
    }

    public class JobFamilyResponse
    {
        public Guid Id { get; set; }
        public string JobFamilyCode { get; set; } = string.Empty;
        public string JobFamilyName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
    }

    public class JobFamilyDetailResponse : JobFamilyResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
    }

    public class JobFamilyOptionResponse
    {
        public Guid Id { get; set; }
        public string JobFamilyCode { get; set; } = string.Empty;
        public string JobFamilyName { get; set; } = string.Empty;
    }

    public class JobFamilyOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<JobFamilyOptionResponse> Items { get; set; } = new();
    }

    public class JobFamilyFilterMetadataResponse
    {
        public JobFamilyDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<JobFamilySortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class JobFamilyDefaultFilterResponse
    {
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "jobFamilyName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class JobFamilySortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateJobFamilyRequest
    {
        [Required, MaxLength(150)]
        public string JobFamilyName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateJobFamilyRequest : CreateJobFamilyRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateJobFamilyStatusRequest
    {
        public bool IsActive { get; set; }
    }
}
