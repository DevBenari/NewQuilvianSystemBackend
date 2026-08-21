using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.DTOs
{
    public class EmployeeCategorySummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int ClinicalData { get; set; }
        public int RequiresCredentialingData { get; set; }
        public int WithoutWorkforceTypeData { get; set; }
    }

    public class EmployeeCategoryResponse
    {
        public Guid Id { get; set; }
        public string EmployeeCategoryCode { get; set; } = string.Empty;
        public string EmployeeCategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? WorkforceTypeId { get; set; }
        public string? WorkforceTypeCode { get; set; }
        public string? WorkforceTypeName { get; set; }
        public bool IsClinical { get; set; }
        public bool RequiresCredentialing { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class EmployeeCategoryDetailResponse : EmployeeCategoryResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class EmployeeCategoryOptionResponse
    {
        public Guid Id { get; set; }
        public string EmployeeCategoryCode { get; set; } = string.Empty;
        public string EmployeeCategoryName { get; set; } = string.Empty;
        public Guid? WorkforceTypeId { get; set; }
        public string? WorkforceTypeName { get; set; }
        public bool IsClinical { get; set; }
    }

    public class EmployeeCategoryOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<EmployeeCategoryOptionResponse> Items { get; set; } = new();
    }

    public class EmployeeCategoryFilterMetadataResponse
    {
        public EmployeeCategoryDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<EmployeeCategoryCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<EmployeeCategorySortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class EmployeeCategoryDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? WorkforceTypeId { get; set; }
        public bool? IsClinical { get; set; }
        public bool? RequiresCredentialing { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "employeeCategoryName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class EmployeeCategoryCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class EmployeeCategorySortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateEmployeeCategoryRequest
    {
        [Required, MaxLength(150)]
        public string EmployeeCategoryName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public Guid? WorkforceTypeId { get; set; }

        public bool IsClinical { get; set; }
        public bool RequiresCredentialing { get; set; }

        [Range(0, int.MaxValue)]
        public int? SortOrder { get; set; }
    }

    public class UpdateEmployeeCategoryRequest : CreateEmployeeCategoryRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateEmployeeCategoryStatusRequest
    {
        public bool IsActive { get; set; }
    }
}
