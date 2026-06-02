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
    [Table("TrxMedicalCertificate", Schema = "public")]
    public class TrxMedicalCertificate : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string MedicalCertificateNumber { get; set; } = string.Empty;

        // =========================
        // CLINICAL CONTEXT
        // =========================

        [Required]
        public Guid PatientId { get; set; }

        public Guid? EncounterId { get; set; }

        public Guid? QueueId { get; set; }

        public Guid? AssessmentId { get; set; }

        public Guid? ConsultationId { get; set; }

        public Guid? PatientDiagnosisId { get; set; }

        public Guid? DiagnosisId { get; set; }

        public Guid? ClinicalDocumentId { get; set; }

        public Guid? DoctorId { get; set; }

        public Guid? ServiceUnitId { get; set; }

        public Guid? ClinicId { get; set; }

        // =========================
        // CERTIFICATE CLASSIFICATION
        // =========================

        public MedicalCertificateType CertificateType { get; set; } =
            MedicalCertificateType.Unknown;

        public MedicalCertificateStatus CertificateStatus { get; set; } =
            MedicalCertificateStatus.Draft;

        public MedicalCertificateRecipientType RecipientType { get; set; } =
            MedicalCertificateRecipientType.Patient;

        public MedicalCertificateDeliveryMethod DeliveryMethod { get; set; } =
            MedicalCertificateDeliveryMethod.Printed;

        [Required]
        [MaxLength(250)]
        public string CertificateTitle { get; set; } = string.Empty;
        // Contoh: Surat Keterangan Sakit, Surat Keterangan Sehat, Surat Kontrol.

        [MaxLength(100)]
        public string? CertificateCode { get; set; }

        [MaxLength(250)]
        public string? CertificateCategoryName { get; set; }

        [MaxLength(250)]
        public string? CertificatePurpose { get; set; }
        // Contoh: Keperluan kantor, sekolah, asuransi, perjalanan, rujukan.

        // =========================
        // DIAGNOSIS SNAPSHOT
        // =========================

        [MaxLength(50)]
        public string? DiagnosisCodeSnapshot { get; set; }

        [MaxLength(500)]
        public string? DiagnosisNameSnapshot { get; set; }

        [MaxLength(50)]
        public string DiagnosisMasterType { get; set; } = "Manual";
        // ICD10, Local, Custom, Manual.

        [MaxLength(100)]
        public string? IcdVersion { get; set; }

        [MaxLength(1000)]
        public string? ClinicalSummary { get; set; }

        [MaxLength(1000)]
        public string? MedicalRecommendation { get; set; }

        // =========================
        // CERTIFICATE CONTENT
        // =========================

        [MaxLength(4000)]
        public string? CertificateStatement { get; set; }
        // Isi utama surat keterangan.

        [MaxLength(2000)]
        public string? AdditionalStatement { get; set; }

        [MaxLength(1000)]
        public string? RestrictionNote { get; set; }

        [MaxLength(1000)]
        public string? FollowUpInstruction { get; set; }

        // =========================
        // SICK LEAVE / REST PERIOD
        // =========================

        public DateTime? SickLeaveStartDate { get; set; }

        public DateTime? SickLeaveEndDate { get; set; }

        public int? SickLeaveDays { get; set; }

        [MaxLength(250)]
        public string? SickLeaveReason { get; set; }

        // =========================
        // CONTROL / REFERRAL
        // =========================

        public DateTime? ControlDate { get; set; }

        [MaxLength(250)]
        public string? ControlClinicName { get; set; }

        public DateTime? ReferralDate { get; set; }

        [MaxLength(250)]
        public string? ReferralToProviderName { get; set; }

        [MaxLength(250)]
        public string? ReferralToDepartmentName { get; set; }

        [MaxLength(1000)]
        public string? ReferralReason { get; set; }

        // =========================
        // INPATIENT / DEATH / FITNESS
        // =========================

        public DateTime? AdmissionDate { get; set; }

        public DateTime? DischargeDate { get; set; }

        public DateTime? DeathDateTime { get; set; }

        [MaxLength(500)]
        public string? CauseOfDeath { get; set; }

        public MedicalFitnessStatus FitnessStatus { get; set; } =
            MedicalFitnessStatus.NotAssessed;

        [MaxLength(1000)]
        public string? FitnessAssessmentNote { get; set; }

        [MaxLength(1000)]
        public string? WorkRestrictionNote { get; set; }

        // =========================
        // RECIPIENT / REQUESTER
        // =========================

        [MaxLength(200)]
        public string? RequestedByName { get; set; }

        [MaxLength(100)]
        public string? RequestedByRelationship { get; set; }

        [MaxLength(250)]
        public string? RecipientName { get; set; }

        [MaxLength(250)]
        public string? RecipientInstitutionName { get; set; }

        [MaxLength(500)]
        public string? RecipientAddress { get; set; }

        // =========================
        // ISSUE / VALIDITY
        // =========================

        public DateTime CertificateDateTime { get; set; } = DateTime.UtcNow;

        public DateTime? IssuedAt { get; set; }

        public Guid? IssuedByDoctorId { get; set; }

        public Guid? IssuedByUserId { get; set; }

        [MaxLength(250)]
        public string? IssuePlace { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public DateTime? ExpiredDate { get; set; }

        // =========================
        // FILE / DOCUMENT OUTPUT
        // =========================

        [MaxLength(500)]
        public string? CertificateFilePath { get; set; }

        [MaxLength(250)]
        public string? CertificateFileName { get; set; }

        [MaxLength(150)]
        public string? CertificateMimeType { get; set; }

        public long? CertificateFileSizeBytes { get; set; }

        [MaxLength(256)]
        public string? CertificateFileHash { get; set; }

        [MaxLength(500)]
        public string? QrCodePath { get; set; }

        [MaxLength(250)]
        public string? VerificationCode { get; set; }

        [MaxLength(500)]
        public string? VerificationUrl { get; set; }

        // =========================
        // VERIFICATION / APPROVAL
        // =========================

        public bool IsIssued { get; set; } = false;

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
        // REVOCATION / CANCELLATION
        // =========================

        public bool IsRevoked { get; set; } = false;

        public DateTime? RevokedAt { get; set; }

        public Guid? RevokedByUserId { get; set; }

        [MaxLength(500)]
        public string? RevocationReason { get; set; }

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

        public TrxPatientDiagnosis? PatientDiagnosis { get; set; }

        public MstDiagnosis? Diagnosis { get; set; }

        public TrxPatientClinicalDocument? ClinicalDocument { get; set; }

        public MstDoctor? Doctor { get; set; }

        public MstDoctor? IssuedByDoctor { get; set; }

        public MstServiceUnit? ServiceUnit { get; set; }

        public MstClinic? Clinic { get; set; }

        public ApplicationUser? IssuedByUser { get; set; }

        public ApplicationUser? VerifiedByUser { get; set; }

        public ApplicationUser? ApprovedByUser { get; set; }

        public ApplicationUser? RejectedByUser { get; set; }

        public ApplicationUser? RevokedByUser { get; set; }

        public ApplicationUser? CancelledByUser { get; set; }
    }
}
