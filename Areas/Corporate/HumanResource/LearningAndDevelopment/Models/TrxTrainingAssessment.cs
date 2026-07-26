using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models
{
    [Table("TrxTrainingAssessment", Schema = "public")]
    public class TrxTrainingAssessment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid TrainingSessionId { get; set; }

        public Guid TrainingParticipantId { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public Guid? CompetencyId { get; set; }

        [Required]
        [MaxLength(40)]
        public string AssessmentType { get; set; } = "PostTest";

        public int AttemptNumber { get; set; } = 1;

        public DateTime? StartedAt { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public decimal Score { get; set; } = 0m;

        public decimal MaximumScore { get; set; } = 100m;

        public decimal PassingScore { get; set; } = 0m;

        public bool IsPassed { get; set; } = false;

        public string? AnswerSnapshotJson { get; set; }

        public string? AssessmentResultJson { get; set; }

        public Guid? AssessedByUserId { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxTrainingSession? TrainingSession { get; set; }
        public TrxTrainingParticipant? TrainingParticipant { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstCompetency? Competency { get; set; }
        public ApplicationUser? AssessedByUser { get; set; }
    }
}
