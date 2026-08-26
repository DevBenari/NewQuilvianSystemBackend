using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.DTOs
{

    public class PerformanceTemplateSummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int DefaultData { get; set; }
        public int SelfAssessmentRequiredData { get; set; }
        public int CalibrationRequiredData { get; set; }
    }

    public class PerformanceTemplateResponse
    {
        public Guid Id { get; set; }
        public Guid? PerformanceCycleId { get; set; }
        public Guid RatingScaleId { get; set; }
        public string? CycleName { get; set; }
        public string? RatingScaleName { get; set; }
        public string TemplateCode { get; set; } = string.Empty;
        public string TemplateName { get; set; } = string.Empty;
        public string TemplateType { get; set; } = string.Empty;
        public decimal TotalWeight { get; set; }
        public decimal? MinimumPassingScore { get; set; }
        public bool IsSelfAssessmentRequired { get; set; }
        public bool IsManagerAssessmentRequired { get; set; }
        public bool IsCalibrationRequired { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public int DetailCount { get; set; }
        public string? Description { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class PerformanceTemplateByIdResponse : PerformanceTemplateResponse
    {
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
        public Guid? EmploymentTypeId { get; set; }
        public Guid? ProfessionId { get; set; }
        public bool IsPeerAssessmentAllowed { get; set; }
        public bool IsSubordinateAssessmentAllowed { get; set; }
        public string? EmployeeInstructions { get; set; }
        public string? ReviewerInstructions { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class PerformanceTemplateOptionResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string TemplateType { get; set; } = string.Empty;
        public Guid RatingScaleId { get; set; }
        public decimal TotalWeight { get; set; }
        public decimal? MinimumPassingScore { get; set; }
    }

    public class PerformanceTemplateOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<PerformanceTemplateOptionResponse> Items { get; set; } = new();
    }

    public class PerformanceTemplateFilterMetadataResponse
    {
        public PerformanceTemplateDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<PerformanceCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<PerformanceStringOptionResponse> TemplateTypeOptions { get; set; } = new();
        public List<PerformanceSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class PerformanceTemplateDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? PerformanceCycleId { get; set; }
        public Guid? RatingScaleId { get; set; }
        public string? TemplateType { get; set; }
        public bool? IsDefault { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "templateName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CreatePerformanceTemplateRequest
    {
        public Guid? PerformanceCycleId { get; set; }
        [Required]
        public Guid RatingScaleId { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
        public Guid? EmploymentTypeId { get; set; }
        public Guid? ProfessionId { get; set; }
        [Required, MaxLength(200)]
        public string TemplateName { get; set; } = string.Empty;
        [Required, MaxLength(50)]
        public string TemplateType { get; set; } = "EmployeePerformance";
        public decimal TotalWeight { get; set; } = 100m;
        public decimal? MinimumPassingScore { get; set; }
        public bool IsSelfAssessmentRequired { get; set; } = true;
        public bool IsManagerAssessmentRequired { get; set; } = true;
        public bool IsPeerAssessmentAllowed { get; set; }
        public bool IsSubordinateAssessmentAllowed { get; set; }
        public bool IsCalibrationRequired { get; set; }
        [MaxLength(2000)]
        public string? EmployeeInstructions { get; set; }
        [MaxLength(2000)]
        public string? ReviewerInstructions { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public bool IsDefault { get; set; }
        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    public class UpdatePerformanceTemplateRequest : CreatePerformanceTemplateRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdatePerformanceTemplateStatusRequest
    {
        public bool IsActive { get; set; }
        public bool? IsDefault { get; set; }
    }
}
