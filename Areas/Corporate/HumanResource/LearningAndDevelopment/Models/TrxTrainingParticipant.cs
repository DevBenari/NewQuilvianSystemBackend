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
    [Table("TrxTrainingParticipant", Schema = "public")]
    public class TrxTrainingParticipant : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid TrainingPlanId { get; set; }

        public Guid TrainingSessionId { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public Guid? EmployeeId { get; set; }

        public Guid? OrganizationAssignmentId { get; set; }

        public Guid? EnrollmentRequestId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ParticipantStatus { get; set; } = "Enrolled";

        public bool IsMandatory { get; set; } = false;

        public DateTime EnrollmentDate { get; set; }

        public Guid? NominatedByUserId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public Guid? ApprovedByUserId { get; set; }

        public decimal AttendancePercentage { get; set; } = 0m;

        public decimal? FinalScore { get; set; }

        public DateTime? CompletedAt { get; set; }

        [MaxLength(2000)]
        public string? CompletionNote { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxTrainingPlan? TrainingPlan { get; set; }
        public TrxTrainingSession? TrainingSession { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public TrxTrainingEnrollmentRequest? EnrollmentRequest { get; set; }
        public ApplicationUser? NominatedByUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }

        public ICollection<TrxTrainingAttendance> Attendances { get; set; } = new List<TrxTrainingAttendance>();
        public ICollection<TrxTrainingAssessment> Assessments { get; set; } = new List<TrxTrainingAssessment>();
        public ICollection<TrxTrainingEvaluation> Evaluations { get; set; } = new List<TrxTrainingEvaluation>();
        public ICollection<TrxTrainingCertificate> Certificates { get; set; } = new List<TrxTrainingCertificate>();
    }
}
