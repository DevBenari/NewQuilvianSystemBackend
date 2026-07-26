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
    [Table("TrxTrainingSession", Schema = "public")]
    public class TrxTrainingSession : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid TrainingPlanId { get; set; }

        public Guid? TrainingCatalogId { get; set; }

        [Required]
        [MaxLength(60)]
        public string SessionCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string SessionName { get; set; } = string.Empty;

        public int Sequence { get; set; } = 1;

        public DateTime StartDateTime { get; set; }

        public DateTime EndDateTime { get; set; }

        public DateTime? RegistrationOpenAt { get; set; }

        public DateTime? RegistrationCloseAt { get; set; }

        [Required]
        [MaxLength(50)]
        public string DeliveryMode { get; set; } = "Classroom";

        [MaxLength(500)]
        public string? Venue { get; set; }

        [MaxLength(1000)]
        public string? MeetingUrl { get; set; }

        [MaxLength(250)]
        public string? InstructorName { get; set; }

        public Guid? InstructorUserId { get; set; }

        public int Capacity { get; set; } = 0;

        public int MinimumParticipant { get; set; } = 0;

        [Required]
        [MaxLength(40)]
        public string SessionStatus { get; set; } = "Draft";

        public bool RequiresAttendance { get; set; } = true;

        public bool RequiresPreTest { get; set; } = false;

        public bool RequiresPostTest { get; set; } = false;

        public bool GeneratesCertificate { get; set; } = false;

        public decimal MinimumPassingScore { get; set; } = 0m;

        public string? AgendaJson { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxTrainingPlan? TrainingPlan { get; set; }
        public MstTrainingCatalog? TrainingCatalog { get; set; }
        public ApplicationUser? InstructorUser { get; set; }

        public ICollection<TrxTrainingParticipant> Participants { get; set; } = new List<TrxTrainingParticipant>();
        public ICollection<TrxTrainingAttendance> Attendances { get; set; } = new List<TrxTrainingAttendance>();
        public ICollection<TrxTrainingAssessment> Assessments { get; set; } = new List<TrxTrainingAssessment>();
        public ICollection<TrxTrainingEvaluation> Evaluations { get; set; } = new List<TrxTrainingEvaluation>();
    }
}
