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
    [Table("TrxTrainingAttendance", Schema = "public")]
    public class TrxTrainingAttendance : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid TrainingSessionId { get; set; }

        public Guid TrainingParticipantId { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public DateTime AttendanceDate { get; set; }

        public DateTime? CheckInAt { get; set; }

        public DateTime? CheckOutAt { get; set; }

        public int ScheduledMinutes { get; set; } = 0;

        public int AttendedMinutes { get; set; } = 0;

        [Required]
        [MaxLength(40)]
        public string AttendanceStatus { get; set; } = "Present";

        [Required]
        [MaxLength(40)]
        public string AttendanceSource { get; set; } = "Manual";

        [MaxLength(1000)]
        public string? ProofPath { get; set; }

        public Guid? RecordedByUserId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxTrainingSession? TrainingSession { get; set; }
        public TrxTrainingParticipant? TrainingParticipant { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public ApplicationUser? RecordedByUser { get; set; }
    }
}
