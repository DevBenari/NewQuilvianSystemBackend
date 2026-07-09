using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models
{
    [Table("TrxPatientIntegratedProgressNote", Schema = "public")]
    public class TrxPatientIntegratedProgressNote : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string ProgressNoteNumber { get; set; } = string.Empty;

        // =========================
        // CLINICAL CONTEXT
        // =========================

        [Required]
        public Guid PatientId { get; set; }

        public Guid? EncounterId { get; set; }

        public Guid? QueueId { get; set; }

        public Guid? ConsultationId { get; set; }

        public Guid? AssessmentId { get; set; }

        public Guid? VitalSignId { get; set; }

        public Guid? DoctorId { get; set; }

        public Guid? ServiceUnitId { get; set; }

        public Guid? ClinicId { get; set; }

        // =========================
        // TIMELINE INFORMATION
        // =========================

        public DateTime NoteDateTime { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(50)]
        public string ProfessionType { get; set; } = string.Empty;
        // Doctor, Nurse, Pharmacist, Nutritionist, Midwife, Physiotherapist, Laboratory, Radiology, Other.

        [MaxLength(100)]
        public string? ProfessionName { get; set; }
        // Display label: Dokter, Perawat, Farmasi, Gizi, etc.

        public Guid? ProviderUserId { get; set; }

        [MaxLength(150)]
        public string? ProviderDisplayNameSnapshot { get; set; }

        [MaxLength(150)]
        public string? ProviderRoleSnapshot { get; set; }

        [MaxLength(150)]
        public string? ServiceUnitNameSnapshot { get; set; }

        [MaxLength(150)]
        public string? LocationSnapshot { get; set; }

        // =========================
        // SOURCE MODULE LINK
        // =========================

        [MaxLength(80)]
        public string? SourceModule { get; set; }
        // DoctorConsultation, PatientAssessment, PatientVitalSign, Prescription, Procedure, Pharmacy, Nutrition, ManualEntry.

        public Guid? SourceReferenceId { get; set; }

        [MaxLength(80)]
        public string? SourceReferenceNumber { get; set; }

        // =========================
        // NOTE CONTENT
        // =========================

        public string? SubjectiveSummary { get; set; }

        public string? ObjectiveSummary { get; set; }

        public string? AssessmentSummary { get; set; }

        public string? PlanSummary { get; set; }

        public string? Instruction { get; set; }

        public string? Evaluation { get; set; }

        public string? NoteText { get; set; }

        public string? PrivateNote { get; set; }

        // =========================
        // WORKFLOW
        // =========================

        public bool IsGeneratedFromSource { get; set; } = false;

        public bool IsReadOnlyGenerated { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime? CancelledAt { get; set; }

        public Guid? CancelledByUserId { get; set; }

        [MaxLength(250)]
        public string? CancelReason { get; set; }

        // =========================
        // NAVIGATION
        // =========================

        public MstPatient? Patient { get; set; }

        public TrxPatientEncounter? Encounter { get; set; }

        public TrxQueue? Queue { get; set; }

        public TrxDoctorConsultation? Consultation { get; set; }

        public TrxPatientAssessment? Assessment { get; set; }

        public TrxPatientVitalSign? VitalSign { get; set; }

        public MstDoctor? Doctor { get; set; }

        public MstServiceUnit? ServiceUnit { get; set; }

        public MstClinic? Clinic { get; set; }

        public ApplicationUser? ProviderUser { get; set; }

        public ApplicationUser? CancelledByUser { get; set; }
    }
}
