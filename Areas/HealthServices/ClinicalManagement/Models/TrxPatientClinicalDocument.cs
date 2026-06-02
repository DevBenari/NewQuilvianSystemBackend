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
    [Table("TrxPatientClinicalDocument", Schema = "public")]
    public class TrxPatientClinicalDocument : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string ClinicalDocumentNumber { get; set; } = string.Empty;

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

        public Guid? PatientProcedureId { get; set; }

        public Guid? DoctorId { get; set; }

        public Guid? ServiceUnitId { get; set; }

        public Guid? ClinicId { get; set; }

        // =========================
        // DOCUMENT CLASSIFICATION
        // =========================

        public PatientClinicalDocumentType DocumentType { get; set; } =
            PatientClinicalDocumentType.Unknown;

        public PatientClinicalDocumentSource DocumentSource { get; set; } =
            PatientClinicalDocumentSource.InternalUpload;

        public PatientClinicalDocumentStatus DocumentStatus { get; set; } =
            PatientClinicalDocumentStatus.Uploaded;

        public PatientClinicalDocumentConfidentialityLevel ConfidentialityLevel { get; set; } =
            PatientClinicalDocumentConfidentialityLevel.Normal;

        [Required]
        [MaxLength(250)]
        public string DocumentTitle { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? DocumentCode { get; set; }

        [MaxLength(250)]
        public string? DocumentCategoryName { get; set; }

        [MaxLength(250)]
        public string? ExternalDocumentNumber { get; set; }

        [MaxLength(250)]
        public string? ExternalProviderName { get; set; }
        // Contoh: RS luar, lab luar, radiology center luar.

        [MaxLength(250)]
        public string? ExternalDoctorName { get; set; }

        // =========================
        // DOCUMENT DATE
        // =========================

        public DateTime DocumentDateTime { get; set; } = DateTime.UtcNow;

        public DateTime? ReceivedDateTime { get; set; }

        public DateTime? UploadedDateTime { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public DateTime? ExpiredDate { get; set; }

        // =========================
        // FILE INFORMATION
        // =========================

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string FileName { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? OriginalFileName { get; set; }

        [MaxLength(100)]
        public string? FileExtension { get; set; }

        [MaxLength(150)]
        public string? MimeType { get; set; }

        public long? FileSizeBytes { get; set; }

        [MaxLength(256)]
        public string? FileHash { get; set; }

        [MaxLength(100)]
        public string? StorageProvider { get; set; }
        // Local, S3, AzureBlob, Minio, GoogleCloudStorage.

        [MaxLength(500)]
        public string? ThumbnailPath { get; set; }

        [MaxLength(500)]
        public string? PreviewPath { get; set; }

        public int? PageCount { get; set; }

        // =========================
        // CONTENT SUMMARY
        // =========================

        [MaxLength(1000)]
        public string? DocumentSummary { get; set; }

        [MaxLength(1000)]
        public string? ClinicalFindingSummary { get; set; }

        [MaxLength(1000)]
        public string? Impression { get; set; }
        // Cocok untuk radiologi/lab/penunjang.

        [MaxLength(1000)]
        public string? Recommendation { get; set; }

        [MaxLength(500)]
        public string? Keywords { get; set; }

        // =========================
        // ACCESS & MEDICAL RECORD FLAG
        // =========================

        public bool IsConfidential { get; set; } = false;

        public bool IsPatientVisible { get; set; } = false;

        public bool IsShareable { get; set; } = false;

        public bool IsExternalDocument { get; set; } = false;

        public bool IsPartOfMedicalRecord { get; set; } = true;

        public bool IsLegalDocument { get; set; } = false;

        public bool IsNeedReview { get; set; } = false;

        public bool IsReviewed { get; set; } = false;

        public DateTime? ReviewedAt { get; set; }

        public Guid? ReviewedByUserId { get; set; }

        [MaxLength(500)]
        public string? ReviewNote { get; set; }

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

        public Guid? UploadedByUserId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        // =========================
        // ARCHIVE / CANCELLATION
        // =========================

        public bool IsArchived { get; set; } = false;

        public DateTime? ArchivedAt { get; set; }

        public Guid? ArchivedByUserId { get; set; }

        [MaxLength(250)]
        public string? ArchiveReason { get; set; }

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

        public TrxQueue? Queue { get; set; }

        public TrxPatientAssessment? Assessment { get; set; }

        public TrxDoctorConsultation? Consultation { get; set; }

        public TrxPatientDiagnosis? PatientDiagnosis { get; set; }

        public TrxPatientProcedure? PatientProcedure { get; set; }

        public MstDoctor? Doctor { get; set; }

        public MstServiceUnit? ServiceUnit { get; set; }

        public MstClinic? Clinic { get; set; }

        public ApplicationUser? UploadedByUser { get; set; }

        public ApplicationUser? ReviewedByUser { get; set; }

        public ApplicationUser? VerifiedByUser { get; set; }

        public ApplicationUser? ApprovedByUser { get; set; }

        public ApplicationUser? ArchivedByUser { get; set; }

        public ApplicationUser? CancelledByUser { get; set; }
    }
}
