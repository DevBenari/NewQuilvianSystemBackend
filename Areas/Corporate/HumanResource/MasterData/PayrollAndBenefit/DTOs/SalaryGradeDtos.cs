using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.DTOs
{
    public class SalaryGradeSummaryResponse
    {
        public int TotalSalaryGrade { get; set; }
        public int ActiveSalaryGrade { get; set; }
        public int InactiveSalaryGrade { get; set; }
        public int LinkedEmployeeGrade { get; set; }
        public int UsedBySalaryStructure { get; set; }
    }

    public class SalaryGradeResponse
    {
        public Guid Id { get; set; }
        public Guid? EmployeeGradeId { get; set; }
        public string SalaryGradeCode { get; set; } = string.Empty;
        public string SalaryGradeName { get; set; } = string.Empty;
        public int GradeLevel { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal MinimumSalary { get; set; }
        public decimal MidpointSalary { get; set; }
        public decimal MaximumSalary { get; set; }
        public decimal AnnualIncrementPercentage { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public int SalaryStructureCount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class SalaryGradeDetailResponse : SalaryGradeResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class SalaryGradeOptionResponse
    {
        public Guid Id { get; set; }
        public string SalaryGradeCode { get; set; } = string.Empty;
        public string SalaryGradeName { get; set; } = string.Empty;
        public int GradeLevel { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal MinimumSalary { get; set; }
        public decimal MidpointSalary { get; set; }
        public decimal MaximumSalary { get; set; }
    }

    public class SalaryGradeOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<SalaryGradeOptionResponse> Items { get; set; } = new();
    }

    public class SalaryGradeFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public SalaryGradeDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<SalaryGradeCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<SalaryGradeSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class SalaryGradeDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? EmployeeGradeId { get; set; }
        public string? CurrencyCode { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "gradeLevel";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class SalaryGradeCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class SalaryGradeSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateSalaryGradeRequest
    {
        public Guid? EmployeeGradeId { get; set; }

        [Required]
        [MaxLength(150)]
        public string SalaryGradeName { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int GradeLevel { get; set; }

        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        [Range(0, double.MaxValue)]
        public decimal MinimumSalary { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MidpointSalary { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MaximumSalary { get; set; }

        [Range(0, 100)]
        public decimal AnnualIncrementPercentage { get; set; }

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int SortOrder { get; set; }
    }

    public class UpdateSalaryGradeRequest : CreateSalaryGradeRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateSalaryGradeStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class SalaryGradeCreateResponse
    {
        public Guid Id { get; set; }
        public string SalaryGradeCode { get; set; } = string.Empty;
        public string SalaryGradeName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
