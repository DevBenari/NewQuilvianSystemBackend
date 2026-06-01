using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models
{
    [Table("TrxPatientAllergy", Schema = "public")]
    public class TrxPatientAllergy : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string AllergyRecordNumber { get; set; } = string.Empty;

        // =========================
        // CLINICAL CONTEXT
        // =========================

        [Required]
        public Guid PatientId { get; set; }

        public Guid? EncounterId { get; set; }

        public Guid? ConsultationId { get; set; }

        public Guid? AssessmentId { get; set; }

        public Guid? DoctorId { get; set; }

        public Guid? ServiceUnitId { get; set; }

        public Guid? ClinicId { get; set; }

        // Optional relasi ke master obat.
        // Tetap nullable karena alergi bisa berupa makanan, latex, debu, kontras, dll.
        public Guid? DrugId { get; set; }

        // =========================
        // ALLERGY INFORMATION
        // =========================

        public PatientAllergyCategory AllergyCategory { get; set; } = PatientAllergyCategory.Unknown;

        [MaxLength(100)]
        public string? AllergenCode { get; set; }

        [Required]
        [MaxLength(250)]
        public string AllergenName { get; set; } = string.Empty;
        // Contoh: Penicillin, Amoxicillin, Seafood, Latex, Iodine Contrast.

        [MaxLength(250)]
        public string? AllergenGroupName { get; set; }
        // Contoh: Beta-lactam, NSAID, Seafood, Contrast Media.

        [MaxLength(100)]
        public string? ReactionType { get; set; }
        // Contoh: Rash, Itching, Swelling, Shortness of breath, Anaphylaxis.

        [MaxLength(1000)]
        public string? ReactionDescription { get; set; }

        public PatientAllergySeverity Severity { get; set; } = PatientAllergySeverity.Unknown;

        public PatientAllergyCertainty Certainty { get; set; } = PatientAllergyCertainty.Unknown;

        public PatientAllergyStatus AllergyStatus { get; set; } = PatientAllergyStatus.Active;

        public DateTime? FirstReactionDate { get; set; }

        public DateTime? LastReactionDate { get; set; }

        public DateTime ReportedDateTime { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? SourceOfInformation { get; set; }
        // Patient, Family, OldMedicalRecord, Doctor, Nurse, ExternalDocument.

        // =========================
        // PATIENT SAFETY FLAG
        // =========================

        public bool IsHighRisk { get; set; } = false;

        public bool IsLifeThreatening { get; set; } = false;

        public bool IsAlertEnabled { get; set; } = true;
        // Dipakai nanti untuk alert resep / pharmacy / CDSS.

        [MaxLength(1000)]
        public string? PatientSafetyNote { get; set; }

        // =========================
        // VERIFICATION
        // =========================

        public bool IsVerified { get; set; } = false;

        public DateTime? VerifiedAt { get; set; }

        public Guid? VerifiedByUserId { get; set; }

        [MaxLength(1000)]
        public string? ClinicalNote { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        // =========================
        // RESOLUTION / CANCELLATION
        // =========================

        public DateTime? ResolvedAt { get; set; }

        public Guid? ResolvedByUserId { get; set; }

        [MaxLength(250)]
        public string? ResolvedReason { get; set; }

        public DateTime? CancelledAt { get; set; }

        public Guid? CancelledByUserId { get; set; }

        [MaxLength(250)]
        public string? CancelReason { get; set; }

        public bool IsActive { get; set; } = true;

        // =========================
        // NAVIGATION
        // =========================

        public MstPatient? Patient { get; set; }

        public TrxPatientEncounter? Encounter { get; set; }

        public TrxDoctorConsultation? Consultation { get; set; }

        public TrxPatientAssessment? Assessment { get; set; }

        public MstDoctor? Doctor { get; set; }

        public MstServiceUnit? ServiceUnit { get; set; }

        public MstClinic? Clinic { get; set; }

        public MstDrug? Drug { get; set; }

        public ApplicationUser? VerifiedByUser { get; set; }

        public ApplicationUser? ResolvedByUser { get; set; }

        public ApplicationUser? CancelledByUser { get; set; }
    }
}
