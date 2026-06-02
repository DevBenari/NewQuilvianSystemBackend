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
    [Table("TrxClinicalNoteAttachment", Schema = "public")]
    public class TrxClinicalNoteAttachment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string AttachmentNumber { get; set; } = string.Empty;

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

        public Guid? ClinicalDocumentId { get; set; }

        public Guid? DoctorId { get; set; }

        public Guid? ServiceUnitId { get; set; }

        public Guid? ClinicId { get; set; }

        // =========================
        // ATTACHMENT CLASSIFICATION
        // =========================

        public ClinicalNoteAttachmentType AttachmentType { get; set; } =
            ClinicalNoteAttachmentType.Unknown;

        public ClinicalNoteAttachmentContext AttachmentContext { get; set; } =
            ClinicalNoteAttachmentContext.General;

        public ClinicalNoteAttachmentStatus AttachmentStatus { get; set; } =
            ClinicalNoteAttachmentStatus.Uploaded;

        public ClinicalNoteAttachmentConfidentialityLevel ConfidentialityLevel { get; set; } =
            ClinicalNoteAttachmentConfidentialityLevel.Normal;

        [Required]
        [MaxLength(250)]
        public string AttachmentTitle { get; set; } = string.Empty;
        // Contoh: Foto luka kaki kanan, Foto ruam kulit, Sketsa lokasi nyeri.

        [MaxLength(100)]
        public string? AttachmentCode { get; set; }

        [MaxLength(250)]
        public string? AttachmentCategoryName { get; set; }

        [MaxLength(1000)]
        public string? AttachmentDescription { get; set; }

        // =========================
        // CLINICAL NOTE CONTEXT
        // =========================

        [MaxLength(250)]
        public string? NoteSectionName { get; set; }
        // Contoh: SOAP.Subjective, SOAP.Objective, PhysicalExamination, ProcedureNote.

        [MaxLength(1000)]
        public string? ClinicalNote { get; set; }

        [MaxLength(1000)]
        public string? FindingNote { get; set; }

        [MaxLength(1000)]
        public string? InterpretationNote { get; set; }

        [MaxLength(1000)]
        public string? FollowUpNote { get; set; }

        // =========================
        // BODY / MEDIA CONTEXT
        // =========================

        [MaxLength(250)]
        public string? BodySite { get; set; }
        // Contoh: kaki kanan, abdomen, thorax, wajah, punggung.

        public ClinicalAttachmentBodySide BodySide { get; set; } =
            ClinicalAttachmentBodySide.Unknown;

        [MaxLength(250)]
        public string? BodySiteDescription { get; set; }

        [MaxLength(100)]
        public string? ViewPosition { get; set; }
        // Contoh: anterior, posterior, lateral, close-up.

        public DateTime? CapturedAt { get; set; }

        public Guid? CapturedByUserId { get; set; }

        [MaxLength(100)]
        public string? CaptureDeviceName { get; set; }

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

        public int? ImageWidth { get; set; }

        public int? ImageHeight { get; set; }

        public int? DurationSeconds { get; set; }

        // =========================
        // ACCESS / MEDICAL RECORD FLAG
        // =========================

        public bool IsConfidential { get; set; } = false;

        public bool IsPatientVisible { get; set; } = false;

        public bool IsShareable { get; set; } = false;

        public bool IsPartOfMedicalRecord { get; set; } = true;

        public bool IsClinicalMedia { get; set; } = true;

        public bool IsBeforeAfterComparison { get; set; } = false;

        public Guid? RelatedAttachmentId { get; set; }
        // Untuk before-after photo atau lampiran pembanding.

        public bool IsNeedReview { get; set; } = false;

        public bool IsReviewed { get; set; } = false;

        public DateTime? ReviewedAt { get; set; }

        public Guid? ReviewedByUserId { get; set; }

        [MaxLength(500)]
        public string? ReviewNote { get; set; }

        // =========================
        // VERIFICATION
        // =========================

        public bool IsVerified { get; set; } = false;

        public DateTime? VerifiedAt { get; set; }

        public Guid? VerifiedByUserId { get; set; }

        [MaxLength(500)]
        public string? VerificationNote { get; set; }

        public Guid? UploadedByUserId { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

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

        public TrxPatientClinicalDocument? ClinicalDocument { get; set; }

        public MstDoctor? Doctor { get; set; }

        public MstServiceUnit? ServiceUnit { get; set; }

        public MstClinic? Clinic { get; set; }

        public TrxClinicalNoteAttachment? RelatedAttachment { get; set; }

        public ApplicationUser? UploadedByUser { get; set; }

        public ApplicationUser? CapturedByUser { get; set; }

        public ApplicationUser? ReviewedByUser { get; set; }

        public ApplicationUser? VerifiedByUser { get; set; }

        public ApplicationUser? ArchivedByUser { get; set; }

        public ApplicationUser? CancelledByUser { get; set; }
    }
}
