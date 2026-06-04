using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.DTOs
{
    public class CompetencySummaryResponse
    {
        public int TotalCompetency { get; set; }
        public int ActiveCompetency { get; set; }
        public int InactiveCompetency { get; set; }
        public int CompetencyWithPositionRequirement { get; set; }
        public int CompetencyWithoutPositionRequirement { get; set; }
        public int CompetencyWithAssessment { get; set; }
        public int RequiredPositionRequirement { get; set; }
        public int CertificationRequiredRequirement { get; set; }
        public int TrainingRequiredRequirement { get; set; }
    }

    public class CompetencyResponse
    {
        public Guid Id { get; set; }
        public string CompetencyCode { get; set; } = string.Empty;
        public string CompetencyName { get; set; } = string.Empty;
        public CompetencyCategory CompetencyCategory { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int PositionRequirementCount { get; set; }
        public int ActivePositionRequirementCount { get; set; }
        public int AssessmentCount { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class CompetencyDetailResponse : CompetencyResponse
    {
    }

    public class CompetencyOptionResponse
    {
        public Guid Id { get; set; }
        public string CompetencyCode { get; set; } = string.Empty;
        public string CompetencyName { get; set; } = string.Empty;
        public CompetencyCategory CompetencyCategory { get; set; }
    }

    public class CompetencyOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<CompetencyOptionResponse> Items { get; set; } = new();
    }

    public class CompetencyFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public CompetencyDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<CompetencyCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<CompetencySortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<CompetencyEnumOptionResponse> CompetencyCategoryOptions { get; set; } = new();
        public List<CompetencyEnumOptionResponse> CompetencyLevelOptions { get; set; } = new();
    }

    public class CompetencyDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? DepartmentId { get; set; }
        public CompetencyCategory? CompetencyCategory { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "competencyName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CompetencyCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CompetencySortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CompetencyEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateCompetencyRequest
    {
        [Required]
        [MaxLength(200)]
        public string CompetencyName { get; set; } = string.Empty;

        public CompetencyCategory CompetencyCategory { get; set; } = CompetencyCategory.Other;

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateCompetencyRequest : CreateCompetencyRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateCompetencyStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class CompetencyCreateResponse
    {
        public Guid Id { get; set; }
        public string CompetencyCode { get; set; } = string.Empty;
        public string CompetencyName { get; set; } = string.Empty;
        public CompetencyCategory CompetencyCategory { get; set; }
        public bool IsActive { get; set; }
    }

    public class PositionCompetencyRequirementResponse
    {
        public Guid Id { get; set; }

        public Guid PositionId { get; set; }
        public string PositionCode { get; set; } = string.Empty;
        public string PositionName { get; set; } = string.Empty;

        public Guid? DepartmentId { get; set; }
        public string? DepartmentCode { get; set; }
        public string? DepartmentName { get; set; }

        public Guid CompetencyId { get; set; }
        public string CompetencyCode { get; set; } = string.Empty;
        public string CompetencyName { get; set; } = string.Empty;
        public CompetencyCategory CompetencyCategory { get; set; }

        public bool IsRequired { get; set; }
        public CompetencyLevel MinimumLevel { get; set; }
        public bool IsCertificationRequired { get; set; }
        public bool IsTrainingRequired { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class PositionCompetencyRequirementListResponse
    {
        public Guid PositionId { get; set; }
        public string PositionCode { get; set; } = string.Empty;
        public string PositionName { get; set; } = string.Empty;
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }

        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public int ActiveData { get; set; }
        public int RequiredData { get; set; }
        public int CertificationRequiredData { get; set; }
        public int TrainingRequiredData { get; set; }

        public List<PositionCompetencyRequirementResponse> Items { get; set; } = new();
    }

    public class CreatePositionCompetencyRequirementRequest
    {
        [Required]
        public Guid CompetencyId { get; set; }

        public bool IsRequired { get; set; } = true;
        public CompetencyLevel MinimumLevel { get; set; } = CompetencyLevel.Basic;
        public bool IsCertificationRequired { get; set; } = false;
        public bool IsTrainingRequired { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }

    public class UpdatePositionCompetencyRequirementRequest
        : CreatePositionCompetencyRequirementRequest
    {
    }

    public class UpdatePositionCompetencyRequirementStatusRequest
    {
        public bool IsActive { get; set; }
    }
}