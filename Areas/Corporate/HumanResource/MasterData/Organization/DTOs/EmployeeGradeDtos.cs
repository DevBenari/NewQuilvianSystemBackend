using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.DTOs
{
    public class EmployeeGradeSummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int WithoutJobLevelData { get; set; }
    }

    public class EmployeeGradeResponse
    {
        public Guid Id { get; set; }
        public Guid? JobLevelId { get; set; }
        public string? JobLevelCode { get; set; }
        public string? JobLevelName { get; set; }
        public string GradeCode { get; set; } = string.Empty;
        public string GradeName { get; set; } = string.Empty;
        public int GradeOrder { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
    }

    public class EmployeeGradeDetailResponse : EmployeeGradeResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
    }

    public class EmployeeGradeOptionResponse
    {
        public Guid Id { get; set; }
        public Guid? JobLevelId { get; set; }
        public string? JobLevelName { get; set; }
        public string GradeCode { get; set; } = string.Empty;
        public string GradeName { get; set; } = string.Empty;
        public int GradeOrder { get; set; }
    }

    public class EmployeeGradeOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<EmployeeGradeOptionResponse> Items { get; set; } = new();
    }

    public class EmployeeGradeFilterMetadataResponse
    {
        public EmployeeGradeDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<EmployeeGradeSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class EmployeeGradeDefaultFilterResponse
    {
        public Guid? JobLevelId { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "gradeOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class EmployeeGradeSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateEmployeeGradeRequest
    {
        public Guid? JobLevelId { get; set; }

        [Required, MaxLength(150)]
        public string GradeName { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int GradeOrder { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateEmployeeGradeRequest : CreateEmployeeGradeRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateEmployeeGradeStatusRequest
    {
        public bool IsActive { get; set; }
    }
}
