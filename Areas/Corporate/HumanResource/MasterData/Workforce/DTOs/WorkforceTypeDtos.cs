using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.DTOs
{
    public class WorkforceTypeSummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int InternalData { get; set; }
        public int ClinicalData { get; set; }
    }

    public class WorkforceTypeResponse
    {
        public Guid Id { get; set; }
        public string WorkforceTypeCode { get; set; } = string.Empty;
        public string WorkforceTypeName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsInternal { get; set; }
        public bool IsClinical { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public int EmployeeCategoryCount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class WorkforceTypeDetailResponse : WorkforceTypeResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class WorkforceTypeOptionResponse
    {
        public Guid Id { get; set; }
        public string WorkforceTypeCode { get; set; } = string.Empty;
        public string WorkforceTypeName { get; set; } = string.Empty;
        public bool IsInternal { get; set; }
        public bool IsClinical { get; set; }
    }

    public class WorkforceTypeOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<WorkforceTypeOptionResponse> Items { get; set; } = new();
    }

    public class WorkforceTypeFilterMetadataResponse
    {
        public WorkforceTypeDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WorkforceTypeCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<WorkforceTypeSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class WorkforceTypeDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public bool? IsInternal { get; set; }
        public bool? IsClinical { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "workforceTypeName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WorkforceTypeCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WorkforceTypeSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateWorkforceTypeRequest
    {
        [Required, MaxLength(150)]
        public string WorkforceTypeName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsInternal { get; set; } = true;
        public bool IsClinical { get; set; }

        [Range(0, int.MaxValue)]
        public int? SortOrder { get; set; }
    }

    public class UpdateWorkforceTypeRequest : CreateWorkforceTypeRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceTypeStatusRequest
    {
        public bool IsActive { get; set; }
    }
}
