using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public class WorkforceCompetencyAssessmentResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public UserType UserType { get; set; }

        public Guid CompetencyId { get; set; }

        public string CompetencyCode { get; set; } = string.Empty;

        public string CompetencyName { get; set; } = string.Empty;

        public CompetencyCategory CompetencyCategory { get; set; }

        public DateTime AssessmentDate { get; set; }

        public CompetencyLevel CompetencyLevel { get; set; }

        public CompetencyAssessmentResultStatus ResultStatus { get; set; }

        public Guid? AssessedByUserId { get; set; }

        public string? AssessedByUserName { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public bool IsExpired { get; set; }

        public string? FilePath { get; set; }

        public string? FileContentType { get; set; }

        public bool HasFile { get; set; }

        public string? Notes { get; set; }

        public bool IsVerified { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceCompetencyAssessmentListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int VerifiedData { get; set; }

        public int PassedData { get; set; }

        public int FailedData { get; set; }

        public int NeedTrainingData { get; set; }

        public int ExpiredData { get; set; }

        public List<WorkforceCompetencyAssessmentResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceCompetencyAssessmentRequest
    {
        [Required]
        public Guid CompetencyId { get; set; }

        [Required]
        public DateTime AssessmentDate { get; set; }

        public CompetencyLevel CompetencyLevel { get; set; } = CompetencyLevel.None;

        public CompetencyAssessmentResultStatus ResultStatus { get; set; } = CompetencyAssessmentResultStatus.NotAssessed;

        public Guid? AssessedByUserId { get; set; }

        public DateTime? ExpiredDate { get; set; }

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(100)]
        public string? FileContentType { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceCompetencyAssessmentRequest
    {
        [Required]
        public Guid CompetencyId { get; set; }

        [Required]
        public DateTime AssessmentDate { get; set; }

        public CompetencyLevel CompetencyLevel { get; set; } = CompetencyLevel.None;

        public CompetencyAssessmentResultStatus ResultStatus { get; set; } = CompetencyAssessmentResultStatus.NotAssessed;

        public Guid? AssessedByUserId { get; set; }

        public DateTime? ExpiredDate { get; set; }

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(100)]
        public string? FileContentType { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceCompetencyAssessmentStatusRequest
    {
        public bool IsActive { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class VerifyWorkforceCompetencyAssessmentRequest
    {
        public bool IsVerified { get; set; } = true;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class WorkforceCompetencyMatrixItemResponse
    {
        public Guid RequirementId { get; set; }

        public Guid PositionId { get; set; }

        public string PositionName { get; set; } = string.Empty;

        public Guid CompetencyId { get; set; }

        public string CompetencyCode { get; set; } = string.Empty;

        public string CompetencyName { get; set; } = string.Empty;

        public CompetencyCategory CompetencyCategory { get; set; }

        public bool IsRequired { get; set; }

        public CompetencyLevel MinimumLevel { get; set; }

        public bool IsCertificationRequired { get; set; }

        public bool IsTrainingRequired { get; set; }

        public Guid? LatestAssessmentId { get; set; }

        public CompetencyLevel? LatestCompetencyLevel { get; set; }

        public CompetencyAssessmentResultStatus? LatestResultStatus { get; set; }

        public DateTime? LatestAssessmentDate { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public bool IsVerified { get; set; }

        public bool IsExpired { get; set; }

        public bool IsPassed { get; set; }

        public bool IsLevelMet { get; set; }

        public string Status { get; set; } = "Missing";
    }

    public class WorkforceCompetencyMatrixResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public UserType UserType { get; set; }

        public Guid? PrimaryPositionId { get; set; }

        public string? PrimaryPositionName { get; set; }

        public int TotalRequirement { get; set; }

        public int CompletedRequirement { get; set; }

        public int MissingRequirement { get; set; }

        public int NeedTrainingRequirement { get; set; }

        public int ExpiredRequirement { get; set; }

        public List<WorkforceCompetencyMatrixItemResponse> Items { get; set; } = new();
    }
}
