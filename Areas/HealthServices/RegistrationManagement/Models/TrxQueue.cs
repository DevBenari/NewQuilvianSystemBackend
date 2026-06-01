using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models
{
    [Table("TrxQueue", Schema = "public")]
    public class TrxQueue : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EncounterId { get; set; }

        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public Guid ServiceUnitId { get; set; }

        public Guid? ClinicId { get; set; }

        public Guid? DoctorId { get; set; }

        public Guid? DoctorScheduleId { get; set; }

        [Required]
        public DateTime QueueDate { get; set; } = DateTime.UtcNow;

        [Required]
        public int QueueNumber { get; set; }

        [Required]
        [MaxLength(50)]
        public string QueueCode { get; set; } = string.Empty;

        public QueueStatus QueueStatus { get; set; } = QueueStatus.WaitingForNurse;

        public int NurseCallAttemptCount { get; set; } = 0;

        public DateTime? LastNurseCalledAt { get; set; }

        public Guid? LastNurseCalledByUserId { get; set; }

        public DateTime? NurseCallExpiresAt { get; set; }

        public int DoctorCallAttemptCount { get; set; } = 0;

        public DateTime? LastDoctorCalledAt { get; set; }

        public Guid? LastDoctorCalledByUserId { get; set; }

        public DateTime? DoctorCallExpiresAt { get; set; }

        public DateTime? ScreeningStartedAt { get; set; }

        public DateTime? ScreeningCompletedAt { get; set; }

        public DateTime? ConsultationStartedAt { get; set; }

        public DateTime? ConsultationCompletedAt { get; set; }

        public int SkipCount { get; set; } = 0;

        public DateTime? LastSkippedAt { get; set; }

        public Guid? LastSkippedByUserId { get; set; }

        [MaxLength(250)]
        public string? SkipReason { get; set; }

        public int RequeueCount { get; set; } = 0;

        public DateTime? LastRequeuedAt { get; set; }

        public Guid? LastRequeuedByUserId { get; set; }

        [MaxLength(250)]
        public string? RequeueReason { get; set; }

        public DateTime? NoShowAt { get; set; }

        public Guid? NoShowByUserId { get; set; }

        [MaxLength(250)]
        public string? NoShowReason { get; set; }

        public DateTime? CancelledAt { get; set; }

        public Guid? CancelledByUserId { get; set; }

        [MaxLength(250)]
        public string? CancelReason { get; set; }

        public DateTime? CompletedAt { get; set; }

        public Guid? CompletedByUserId { get; set; }

        public bool IsPriorityQueue { get; set; } = false;

        public bool IsFromKiosk { get; set; } = false;

        public bool IsWalkIn { get; set; } = true;

        public bool IsAppointment { get; set; } = false;

        public bool IsScreeningRequired { get; set; } = true;

        public bool IsDoctorRequired { get; set; } = true;

        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public TrxPatientEncounter? Encounter { get; set; }

        public MstPatient? Patient { get; set; }

        public MstServiceUnit? ServiceUnit { get; set; }

        public MstClinic? Clinic { get; set; }

        public MstDoctor? Doctor { get; set; }

        public MstDoctorSchedule? DoctorSchedule { get; set; }

        public ApplicationUser? LastNurseCalledByUser { get; set; }

        public ApplicationUser? LastDoctorCalledByUser { get; set; }

        public ApplicationUser? LastSkippedByUser { get; set; }

        public ApplicationUser? LastRequeuedByUser { get; set; }

        public ApplicationUser? NoShowByUser { get; set; }

        public ApplicationUser? CancelledByUser { get; set; }

        public ApplicationUser? CompletedByUser { get; set; }
    }
}
