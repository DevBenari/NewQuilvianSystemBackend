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
    [Table("TrxTrainingResult", Schema = "public")]
    public class TrxTrainingResult : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid TrainingParticipantId { get; set; }

        public Guid TrainingSessionId { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public Guid? TrainingRecordId { get; set; }

        public DateTime ResultDate { get; set; }

        [Required]
        [MaxLength(40)]
        public string ResultStatus { get; set; } = "Completed";

        public decimal? PreTestScore { get; set; }

        public decimal? PostTestScore { get; set; }

        public decimal? FinalScore { get; set; }

        public decimal AttendancePercentage { get; set; } = 0m;

        public bool IsPassed { get; set; } = false;

        public bool IsCompleted { get; set; } = false;

        public decimal CreditPointEarned { get; set; } = 0m;

        public string? ResultSummaryJson { get; set; }

        public Guid? VerifiedByUserId { get; set; }

        public DateTime? VerifiedAt { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxTrainingParticipant? TrainingParticipant { get; set; }
        public TrxTrainingSession? TrainingSession { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public WfpTrainingRecord? TrainingRecord { get; set; }
        public ApplicationUser? VerifiedByUser { get; set; }
    }
}
