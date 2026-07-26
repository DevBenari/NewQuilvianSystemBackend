using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models
{
    [Table("TrxCandidateAssessment", Schema = "public")]
    public class TrxCandidateAssessment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CandidateApplicationId { get; set; }

        [Required]
        public Guid AssessmentMethodId { get; set; }

        public Guid? RecruitmentStageId { get; set; }
        public Guid? EvaluatorUserId { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        [MaxLength(30)]
        public string AssessmentStatus { get; set; } = "Scheduled";

        public decimal? RawScore { get; set; }
        public decimal? FinalScore { get; set; }

        [MaxLength(30)]
        public string? AssessmentResult { get; set; }
        // Pass, Fail, Hold, NeedReview.

        [MaxLength(200)]
        public string? ExternalReferenceNumber { get; set; }

        [MaxLength(500)]
        public string? ResultFilePath { get; set; }

        [MaxLength(1500)]
        public string? EvaluatorNotes { get; set; }

        public string? ResultDetailJson { get; set; }
        public bool IsActive { get; set; } = true;

        public TrxCandidateApplication? CandidateApplication { get; set; }
        public MstAssessmentMethod? AssessmentMethod { get; set; }
        public MstRecruitmentStage? RecruitmentStage { get; set; }
        public ApplicationUser? EvaluatorUser { get; set; }
    }
}
