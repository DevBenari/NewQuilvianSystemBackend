using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.DTOs
{
    public class CompetencyResponse
    {
        public Guid Id { get; set; }

        public string CompetencyCode { get; set; } = string.Empty;

        public string CompetencyName { get; set; } = string.Empty;

        public CompetencyCategory CompetencyCategory { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public int PositionRequirementCount { get; set; }

        public int AssessmentCount { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class CompetencyListResponse
    {
        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int InactiveData { get; set; }

        public List<CompetencyResponse> Items { get; set; } = new();
    }

    public class CompetencyOptionResponse
    {
        public Guid Id { get; set; }

        public string CompetencyCode { get; set; } = string.Empty;

        public string CompetencyName { get; set; } = string.Empty;

        public CompetencyCategory CompetencyCategory { get; set; }
    }

    public class CreateCompetencyRequest
    {
        [Required]
        [MaxLength(100)]
        public string CompetencyCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string CompetencyName { get; set; } = string.Empty;

        public CompetencyCategory CompetencyCategory { get; set; } = CompetencyCategory.Other;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateCompetencyRequest
    {
        [Required]
        [MaxLength(100)]
        public string CompetencyCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string CompetencyName { get; set; } = string.Empty;

        public CompetencyCategory CompetencyCategory { get; set; } = CompetencyCategory.Other;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateCompetencyStatusRequest
    {
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

        public int TotalData { get; set; }

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
    {
        [Required]
        public Guid CompetencyId { get; set; }

        public bool IsRequired { get; set; } = true;

        public CompetencyLevel MinimumLevel { get; set; } = CompetencyLevel.Basic;

        public bool IsCertificationRequired { get; set; } = false;

        public bool IsTrainingRequired { get; set; } = false;

        public bool IsActive { get; set; } = true;
    }

    public class UpdatePositionCompetencyRequirementStatusRequest
    {
        public bool IsActive { get; set; }
    }
}
