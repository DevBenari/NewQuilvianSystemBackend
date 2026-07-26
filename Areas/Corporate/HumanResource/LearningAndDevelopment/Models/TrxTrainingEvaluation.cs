using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models
{
    [Table("TrxTrainingEvaluation", Schema = "public")]
    public class TrxTrainingEvaluation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid TrainingSessionId { get; set; }

        public Guid TrainingParticipantId { get; set; }

        public Guid WorkforceProfileId { get; set; }

        [Required]
        [MaxLength(40)]
        public string EvaluationType { get; set; } = "Participant";

        public DateTime EvaluationDate { get; set; }

        public decimal? ContentRating { get; set; }

        public decimal? InstructorRating { get; set; }

        public decimal? FacilityRating { get; set; }

        public decimal? OverallRating { get; set; }

        public bool WouldRecommend { get; set; } = false;

        [MaxLength(2000)]
        public string? MostUsefulTopic { get; set; }

        [MaxLength(2000)]
        public string? ImprovementSuggestion { get; set; }

        [MaxLength(3000)]
        public string? Comments { get; set; }

        public string? EvaluationJson { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxTrainingSession? TrainingSession { get; set; }
        public TrxTrainingParticipant? TrainingParticipant { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
    }
}
