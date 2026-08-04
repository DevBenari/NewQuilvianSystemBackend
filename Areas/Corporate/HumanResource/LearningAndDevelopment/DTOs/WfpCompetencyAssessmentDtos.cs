using QuilvianSystemBackend.Enums.HumanResource;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs
{
    public class WfpCompetencyAssessmentSummaryResponse
    {
        public int TotalAssessment { get; set; }
        public int ActiveAssessment { get; set; }
        public int InactiveAssessment { get; set; }
        public int VerifiedAssessment { get; set; }
        public int UnverifiedAssessment { get; set; }
        public int ExpiredAssessment { get; set; }
        public int ExpiringSoonAssessment { get; set; }
        public decimal? AverageScorePercentage { get; set; }
    }

    public class WfpCompetencyAssessmentResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public Guid CompetencyId { get; set; }
        public string CompetencyCode { get; set; } = string.Empty;
        public string CompetencyName { get; set; } = string.Empty;
        public Guid? SourceTrainingAssessmentId { get; set; }
        public Guid? SourceTrainingResultId { get; set; }
        public DateTime AssessmentDate { get; set; }
        public CompetencyLevel CompetencyLevel { get; set; }
        public CompetencyAssessmentResultStatus ResultStatus { get; set; }
        public Guid? AssessedByUserId { get; set; }
        public string? AssessedByUserName { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public bool IsExpired { get; set; }
        public int? DaysUntilExpiry { get; set; }
        public decimal? Score { get; set; }
        public decimal? MaximumScore { get; set; }
        public decimal? ScorePercentage { get; set; }
        public string? FilePath { get; set; }
        public string? FileContentType { get; set; }
        public bool HasFile { get; set; }
        public string? Notes { get; set; }
        public bool IsVerified { get; set; }
        public Guid? VerifiedByUserId { get; set; }
        public string? VerifiedByUserName { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class WfpCompetencyAssessmentDetailResponse : WfpCompetencyAssessmentResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class WfpCompetencyAssessmentFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public WfpCompetencyAssessmentDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WfpCompetencyAssessmentStringOptionResponse> CustomPeriods { get; set; } = new();
        public List<WfpCompetencyAssessmentEnumOptionResponse> CompetencyLevelOptions { get; set; } = new();
        public List<WfpCompetencyAssessmentEnumOptionResponse> ResultStatusOptions { get; set; } = new();
        public List<WfpCompetencyAssessmentMasterOptionResponse> CompetencyOptions { get; set; } = new();
        public List<WfpCompetencyAssessmentSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class WfpCompetencyAssessmentDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? CompetencyId { get; set; }
        public CompetencyLevel? CompetencyLevel { get; set; }
        public CompetencyAssessmentResultStatus? ResultStatus { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsExpired { get; set; }
        public bool? IsActive { get; set; }
        public int? ExpiringWithinDays { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "assessmentDate";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WfpCompetencyAssessmentStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WfpCompetencyAssessmentEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WfpCompetencyAssessmentSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WfpCompetencyAssessmentMasterOptionResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class CreateWfpCompetencyAssessmentRequest
    {
        [Required]
        public Guid CompetencyId { get; set; }

        public Guid? SourceTrainingAssessmentId { get; set; }
        public Guid? SourceTrainingResultId { get; set; }

        [Required]
        public DateTime AssessmentDate { get; set; }

        public CompetencyLevel CompetencyLevel { get; set; } = CompetencyLevel.Basic;
        public CompetencyAssessmentResultStatus ResultStatus { get; set; }
        public Guid? AssessedByUserId { get; set; }
        public DateTime? ExpiredDate { get; set; }

        [Range(typeof(decimal), "0", "99999999999999.9999")]
        public decimal? Score { get; set; }

        [Range(typeof(decimal), "0.0001", "99999999999999.9999")]
        public decimal? MaximumScore { get; set; }

        [MaxLength(1000)]
        public string? FilePath { get; set; }

        [MaxLength(150)]
        public string? FileContentType { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsVerified { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateWfpCompetencyAssessmentRequest : CreateWfpCompetencyAssessmentRequest { }

    public class UpdateWfpCompetencyAssessmentStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class VerifyWfpCompetencyAssessmentRequest
    {
        public bool IsVerified { get; set; } = true;
    }
}
