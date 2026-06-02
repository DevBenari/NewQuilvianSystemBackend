using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models
{
    [Table("TrxPatientConsent", Schema = "public")]
    public class TrxPatientConsent : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string ConsentNumber { get; set; } = string.Empty;

        // =========================
        // CLINICAL CONTEXT
        // =========================

        [Required]
        public Guid PatientId { get; set; }

        public Guid? EncounterId { get; set; }

        public Guid? QueueId { get; set; }

        public Guid? AssessmentId { get; set; }

        public Guid? ConsultationId { get; set; }

        public Guid? PatientProcedureId { get; set; }

        public Guid? ClinicalDocumentId { get; set; }

        public Guid? DoctorId { get; set; }

        public Guid? ServiceUnitId { get; set; }

        public Guid? ClinicId { get; set; }

        // =========================
        // CONSENT CLASSIFICATION
        // =========================

        public PatientConsentType ConsentType { get; set; } = PatientConsentType.Unknown;

        public PatientConsentStatus ConsentStatus { get; set; } = PatientConsentStatus.Draft;

        public PatientConsentMethod ConsentMethod { get; set; } = PatientConsentMethod.WrittenPaper;

        [Required]
        [MaxLength(250)]
        public string ConsentTitle { get; set; } = string.Empty;
        // Contoh: Persetujuan Tindakan Operasi, Persetujuan Anestesi.

        [MaxLength(100)]
        public string? ConsentCode { get; set; }

        [MaxLength(250)]
        public string? ConsentCategoryName { get; set; }

        [MaxLength(1000)]
        public string? ConsentDescription { get; set; }

        // =========================
        // PROCEDURE / ACTION SNAPSHOT
        // =========================

        [MaxLength(50)]
        public string? ProcedureCodeSnapshot { get; set; }

        [MaxLength(250)]
        public string? ProcedureNameSnapshot { get; set; }

        [MaxLength(100)]
        public string? ProcedureTypeSnapshot { get; set; }

        public DateTime? PlannedProcedureDateTime { get; set; }

        [MaxLength(250)]
        public string? ProcedureLocation { get; set; }

        // =========================
        // INFORMED CONSENT CONTENT
        // =========================

        [MaxLength(2000)]
        public string? DiagnosisExplanation { get; set; }

        [MaxLength(2000)]
        public string? ProcedureExplanation { get; set; }

        [MaxLength(2000)]
        public string? BenefitExplanation { get; set; }

        [MaxLength(2000)]
        public string? RiskExplanation { get; set; }

        [MaxLength(2000)]
        public string? AlternativeExplanation { get; set; }

        [MaxLength(2000)]
        public string? ConsequenceExplanation { get; set; }

        [MaxLength(1000)]
        public string? PatientQuestionNote { get; set; }

        public bool IsDiagnosisExplained { get; set; } = false;

        public bool IsProcedureExplained { get; set; } = false;

        public bool IsRiskExplained { get; set; } = false;

        public bool IsAlternativeExplained { get; set; } = false;

        public bool IsPatientUnderstood { get; set; } = false;

        public bool IsPatientAgreed { get; set; } = false;

        public bool IsEmergencyConsent { get; set; } = false;

        public bool IsHighRiskConsent { get; set; } = false;

        public bool IsLegalDocument { get; set; } = true;

        public bool IsPartOfMedicalRecord { get; set; } = true;

        // =========================
        // SIGNER / CONSENTER
        // =========================

        public PatientConsentSignerType SignerType { get; set; } = PatientConsentSignerType.Patient;

        [Required]
        [MaxLength(200)]
        public string SignerName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? SignerRelationship { get; set; }
        // Contoh: Diri sendiri, Ayah, Ibu, Suami, Istri, Wali.

        [MaxLength(50)]
        public string? SignerIdentityType { get; set; }

        [MaxLength(100)]
        public string? SignerIdentityNumber { get; set; }

        [MaxLength(30)]
        public string? SignerPhoneNumber { get; set; }

        [MaxLength(500)]
        public string? SignerAddress { get; set; }

        public bool IsSignerPatient { get; set; } = true;

        public bool IsSignerLegalRepresentative { get; set; } = false;

        // =========================
        // EXPLAINED BY / WITNESS
        // =========================

        public Guid? ExplainedByDoctorId { get; set; }

        public Guid? ExplainedByUserId { get; set; }

        public DateTime? ExplainedAt { get; set; }

        [MaxLength(200)]
        public string? WitnessName { get; set; }

        [MaxLength(100)]
        public string? WitnessRelationship { get; set; }

        public Guid? WitnessByUserId { get; set; }

        // =========================
        // SIGNATURE / DOCUMENT FILE
        // =========================

        public DateTime? SignedAt { get; set; }

        [MaxLength(500)]
        public string? PatientSignaturePath { get; set; }

        [MaxLength(500)]
        public string? SignerSignaturePath { get; set; }

        [MaxLength(500)]
        public string? DoctorSignaturePath { get; set; }

        [MaxLength(500)]
        public string? WitnessSignaturePath { get; set; }

        [MaxLength(500)]
        public string? ConsentFilePath { get; set; }
        // File PDF hasil generate atau scan dokumen consent.

        [MaxLength(250)]
        public string? ConsentFileName { get; set; }

        [MaxLength(150)]
        public string? ConsentMimeType { get; set; }

        public long? ConsentFileSizeBytes { get; set; }

        [MaxLength(256)]
        public string? ConsentFileHash { get; set; }

        // =========================
        // VALIDITY
        // =========================

        public DateTime ConsentDateTime { get; set; } = DateTime.UtcNow;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public DateTime? ExpiredDate { get; set; }

        // =========================
        // VERIFICATION / APPROVAL
        // =========================

        public bool IsVerified { get; set; } = false;

        public DateTime? VerifiedAt { get; set; }

        public Guid? VerifiedByUserId { get; set; }

        [MaxLength(500)]
        public string? VerificationNote { get; set; }

        public bool IsApproved { get; set; } = false;

        public DateTime? ApprovedAt { get; set; }

        public Guid? ApprovedByUserId { get; set; }

        [MaxLength(500)]
        public string? ApprovalNote { get; set; }

        public bool IsRejected { get; set; } = false;

        public DateTime? RejectedAt { get; set; }

        public Guid? RejectedByUserId { get; set; }

        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        // =========================
        // WITHDRAWAL / CANCELLATION
        // =========================

        public bool IsWithdrawn { get; set; } = false;

        public DateTime? WithdrawnAt { get; set; }

        public Guid? WithdrawnByUserId { get; set; }

        [MaxLength(500)]
        public string? WithdrawalReason { get; set; }

        public DateTime? CancelledAt { get; set; }

        public Guid? CancelledByUserId { get; set; }

        [MaxLength(250)]
        public string? CancelReason { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        // =========================
        // NAVIGATION
        // =========================

        public MstPatient? Patient { get; set; }

        public TrxPatientEncounter? Encounter { get; set; }

        public TrxQueue? Queue { get; set; }

        public TrxPatientAssessment? Assessment { get; set; }

        public TrxDoctorConsultation? Consultation { get; set; }

        public TrxPatientProcedure? PatientProcedure { get; set; }

        public TrxPatientClinicalDocument? ClinicalDocument { get; set; }

        public MstDoctor? Doctor { get; set; }

        public MstDoctor? ExplainedByDoctor { get; set; }

        public MstServiceUnit? ServiceUnit { get; set; }

        public MstClinic? Clinic { get; set; }

        public ApplicationUser? ExplainedByUser { get; set; }

        public ApplicationUser? WitnessByUser { get; set; }

        public ApplicationUser? VerifiedByUser { get; set; }

        public ApplicationUser? ApprovedByUser { get; set; }

        public ApplicationUser? RejectedByUser { get; set; }

        public ApplicationUser? WithdrawnByUser { get; set; }

        public ApplicationUser? CancelledByUser { get; set; }
    }
}
