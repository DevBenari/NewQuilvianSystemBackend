using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs
{
    public class ClinicalNoteAttachmentResponse
    {
        public Guid Id { get; set; }
        public string AttachmentNumber { get; set; } = string.Empty;

        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string MedicalRecordNumber { get; set; } = string.Empty;

        public Guid? EncounterId { get; set; }
        public string? EncounterNumber { get; set; }

        public Guid? QueueId { get; set; }
        public string? QueueCode { get; set; }

        public Guid? AssessmentId { get; set; }
        public string? AssessmentNumber { get; set; }

        public Guid? ConsultationId { get; set; }
        public string? ConsultationNumber { get; set; }

        public Guid? PatientDiagnosisId { get; set; }
        public Guid? PatientProcedureId { get; set; }

        public Guid? ClinicalDocumentId { get; set; }
        public string? ClinicalDocumentNumber { get; set; }

        public Guid? DoctorId { get; set; }
        public string? DoctorName { get; set; }

        public Guid? ServiceUnitId { get; set; }
        public string? ServiceUnitName { get; set; }

        public Guid? ClinicId { get; set; }
        public string? ClinicName { get; set; }

        public ClinicalNoteAttachmentType AttachmentType { get; set; }
        public ClinicalNoteAttachmentContext AttachmentContext { get; set; }
        public ClinicalNoteAttachmentStatus AttachmentStatus { get; set; }
        public ClinicalNoteAttachmentConfidentialityLevel ConfidentialityLevel { get; set; }

        public string AttachmentTitle { get; set; } = string.Empty;
        public string? AttachmentCode { get; set; }
        public string? AttachmentCategoryName { get; set; }
        public string? NoteSectionName { get; set; }

        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string? OriginalFileName { get; set; }
        public string? FileExtension { get; set; }
        public string? MimeType { get; set; }
        public long? FileSizeBytes { get; set; }
        public string? ThumbnailPath { get; set; }
        public string? PreviewPath { get; set; }

        public string? BodySite { get; set; }
        public ClinicalAttachmentBodySide BodySide { get; set; }
        public DateTime? CapturedAt { get; set; }
        public Guid? CapturedByUserId { get; set; }
        public string? CapturedByUserName { get; set; }

        public bool IsConfidential { get; set; }
        public bool IsPatientVisible { get; set; }
        public bool IsShareable { get; set; }
        public bool IsPartOfMedicalRecord { get; set; }
        public bool IsClinicalMedia { get; set; }
        public bool IsBeforeAfterComparison { get; set; }
        public Guid? RelatedAttachmentId { get; set; }
        public string? RelatedAttachmentNumber { get; set; }

        public bool IsNeedReview { get; set; }
        public bool IsReviewed { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public Guid? ReviewedByUserId { get; set; }
        public string? ReviewedByUserName { get; set; }

        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public Guid? VerifiedByUserId { get; set; }
        public string? VerifiedByUserName { get; set; }

        public Guid? UploadedByUserId { get; set; }
        public string? UploadedByUserName { get; set; }
        public DateTime UploadedAt { get; set; }

        public bool IsArchived { get; set; }
        public DateTime? ArchivedAt { get; set; }
        public Guid? ArchivedByUserId { get; set; }
        public string? ArchivedByUserName { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class ClinicalNoteAttachmentDetailResponse : ClinicalNoteAttachmentResponse
    {
        public string? AttachmentDescription { get; set; }
        public string? ClinicalNote { get; set; }
        public string? FindingNote { get; set; }
        public string? InterpretationNote { get; set; }
        public string? FollowUpNote { get; set; }

        public string? BodySiteDescription { get; set; }
        public string? ViewPosition { get; set; }
        public string? CaptureDeviceName { get; set; }

        public string? FileHash { get; set; }
        public string? StorageProvider { get; set; }
        public int? ImageWidth { get; set; }
        public int? ImageHeight { get; set; }
        public int? DurationSeconds { get; set; }

        public string? ReviewNote { get; set; }
        public string? VerificationNote { get; set; }
        public string? ArchiveReason { get; set; }

        public DateTime? CancelledAt { get; set; }
        public Guid? CancelledByUserId { get; set; }
        public string? CancelledByUserName { get; set; }
        public string? CancelReason { get; set; }

        public string? Notes { get; set; }
    }

    public class ClinicalNoteAttachmentOptionResponse
    {
        public Guid Id { get; set; }
        public string AttachmentNumber { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public Guid? EncounterId { get; set; }
        public Guid? ConsultationId { get; set; }
        public Guid? PatientDiagnosisId { get; set; }
        public Guid? PatientProcedureId { get; set; }
        public ClinicalNoteAttachmentType AttachmentType { get; set; }
        public ClinicalNoteAttachmentContext AttachmentContext { get; set; }
        public ClinicalNoteAttachmentStatus AttachmentStatus { get; set; }
        public string AttachmentTitle { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string? MimeType { get; set; }
        public DateTime UploadedAt { get; set; }
        public bool IsReviewed { get; set; }
        public bool IsVerified { get; set; }
        public bool IsArchived { get; set; }
    }

    public class ClinicalNoteAttachmentFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public ClinicalNoteAttachmentDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<ClinicalNoteAttachmentSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<ClinicalNoteAttachmentEnumOptionResponse> AttachmentTypeOptions { get; set; } = new();
        public List<ClinicalNoteAttachmentEnumOptionResponse> AttachmentContextOptions { get; set; } = new();
        public List<ClinicalNoteAttachmentEnumOptionResponse> AttachmentStatusOptions { get; set; } = new();
        public List<ClinicalNoteAttachmentEnumOptionResponse> ConfidentialityLevelOptions { get; set; } = new();
        public List<ClinicalNoteAttachmentEnumOptionResponse> BodySideOptions { get; set; } = new();
    }

    public class ClinicalNoteAttachmentDefaultFilterResponse
    {
        public string? Search { get; set; }
        public Guid? PatientId { get; set; }
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
        public Guid? RelatedAttachmentId { get; set; }
        public ClinicalNoteAttachmentType? AttachmentType { get; set; }
        public ClinicalNoteAttachmentContext? AttachmentContext { get; set; }
        public ClinicalNoteAttachmentStatus? AttachmentStatus { get; set; }
        public ClinicalNoteAttachmentConfidentialityLevel? ConfidentialityLevel { get; set; }
        public ClinicalAttachmentBodySide? BodySide { get; set; }
        public bool? IsConfidential { get; set; }
        public bool? IsPatientVisible { get; set; }
        public bool? IsPartOfMedicalRecord { get; set; }
        public bool? IsClinicalMedia { get; set; }
        public bool? IsBeforeAfterComparison { get; set; }
        public bool? IsNeedReview { get; set; }
        public bool? IsReviewed { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsArchived { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SortBy { get; set; } = "uploadedAt";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class ClinicalNoteAttachmentSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class ClinicalNoteAttachmentEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateClinicalNoteAttachmentRequest
    {
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

        public ClinicalNoteAttachmentType AttachmentType { get; set; } = ClinicalNoteAttachmentType.Unknown;
        public ClinicalNoteAttachmentContext AttachmentContext { get; set; } = ClinicalNoteAttachmentContext.General;
        public ClinicalNoteAttachmentStatus AttachmentStatus { get; set; } = ClinicalNoteAttachmentStatus.Uploaded;
        public ClinicalNoteAttachmentConfidentialityLevel ConfidentialityLevel { get; set; } = ClinicalNoteAttachmentConfidentialityLevel.Normal;

        [Required]
        [MaxLength(250)]
        public string AttachmentTitle { get; set; } = string.Empty;

        [MaxLength(100)] public string? AttachmentCode { get; set; }
        [MaxLength(250)] public string? AttachmentCategoryName { get; set; }
        [MaxLength(1000)] public string? AttachmentDescription { get; set; }
        [MaxLength(250)] public string? NoteSectionName { get; set; }
        [MaxLength(1000)] public string? ClinicalNote { get; set; }
        [MaxLength(1000)] public string? FindingNote { get; set; }
        [MaxLength(1000)] public string? InterpretationNote { get; set; }
        [MaxLength(1000)] public string? FollowUpNote { get; set; }

        [MaxLength(250)] public string? BodySite { get; set; }
        public ClinicalAttachmentBodySide BodySide { get; set; } = ClinicalAttachmentBodySide.Unknown;
        [MaxLength(250)] public string? BodySiteDescription { get; set; }
        [MaxLength(100)] public string? ViewPosition { get; set; }
        public DateTime? CapturedAt { get; set; }
        public Guid? CapturedByUserId { get; set; }
        [MaxLength(100)] public string? CaptureDeviceName { get; set; }

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string FileName { get; set; } = string.Empty;

        [MaxLength(250)] public string? OriginalFileName { get; set; }
        [MaxLength(100)] public string? FileExtension { get; set; }
        [MaxLength(150)] public string? MimeType { get; set; }
        public long? FileSizeBytes { get; set; }
        [MaxLength(256)] public string? FileHash { get; set; }
        [MaxLength(100)] public string? StorageProvider { get; set; }
        [MaxLength(500)] public string? ThumbnailPath { get; set; }
        [MaxLength(500)] public string? PreviewPath { get; set; }
        public int? ImageWidth { get; set; }
        public int? ImageHeight { get; set; }
        public int? DurationSeconds { get; set; }

        public bool IsConfidential { get; set; } = false;
        public bool IsPatientVisible { get; set; } = false;
        public bool IsShareable { get; set; } = false;
        public bool IsPartOfMedicalRecord { get; set; } = true;
        public bool IsClinicalMedia { get; set; } = true;
        public bool IsBeforeAfterComparison { get; set; } = false;
        public Guid? RelatedAttachmentId { get; set; }
        public bool IsNeedReview { get; set; } = false;
        public bool IsReviewed { get; set; } = false;
        public bool IsVerified { get; set; } = false;
        public DateTime? UploadedAt { get; set; }

        [MaxLength(500)] public string? Notes { get; set; }
    }

    public class UpdateClinicalNoteAttachmentRequest
    {
        public ClinicalNoteAttachmentType AttachmentType { get; set; } = ClinicalNoteAttachmentType.Unknown;
        public ClinicalNoteAttachmentContext AttachmentContext { get; set; } = ClinicalNoteAttachmentContext.General;
        public ClinicalNoteAttachmentStatus AttachmentStatus { get; set; } = ClinicalNoteAttachmentStatus.Uploaded;
        public ClinicalNoteAttachmentConfidentialityLevel ConfidentialityLevel { get; set; } = ClinicalNoteAttachmentConfidentialityLevel.Normal;

        [Required]
        [MaxLength(250)]
        public string AttachmentTitle { get; set; } = string.Empty;

        [MaxLength(100)] public string? AttachmentCode { get; set; }
        [MaxLength(250)] public string? AttachmentCategoryName { get; set; }
        [MaxLength(1000)] public string? AttachmentDescription { get; set; }
        [MaxLength(250)] public string? NoteSectionName { get; set; }
        [MaxLength(1000)] public string? ClinicalNote { get; set; }
        [MaxLength(1000)] public string? FindingNote { get; set; }
        [MaxLength(1000)] public string? InterpretationNote { get; set; }
        [MaxLength(1000)] public string? FollowUpNote { get; set; }

        [MaxLength(250)] public string? BodySite { get; set; }
        public ClinicalAttachmentBodySide BodySide { get; set; } = ClinicalAttachmentBodySide.Unknown;
        [MaxLength(250)] public string? BodySiteDescription { get; set; }
        [MaxLength(100)] public string? ViewPosition { get; set; }
        public DateTime? CapturedAt { get; set; }
        public Guid? CapturedByUserId { get; set; }
        [MaxLength(100)] public string? CaptureDeviceName { get; set; }

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string FileName { get; set; } = string.Empty;

        [MaxLength(250)] public string? OriginalFileName { get; set; }
        [MaxLength(100)] public string? FileExtension { get; set; }
        [MaxLength(150)] public string? MimeType { get; set; }
        public long? FileSizeBytes { get; set; }
        [MaxLength(256)] public string? FileHash { get; set; }
        [MaxLength(100)] public string? StorageProvider { get; set; }
        [MaxLength(500)] public string? ThumbnailPath { get; set; }
        [MaxLength(500)] public string? PreviewPath { get; set; }
        public int? ImageWidth { get; set; }
        public int? ImageHeight { get; set; }
        public int? DurationSeconds { get; set; }

        public bool IsConfidential { get; set; } = false;
        public bool IsPatientVisible { get; set; } = false;
        public bool IsShareable { get; set; } = false;
        public bool IsPartOfMedicalRecord { get; set; } = true;
        public bool IsClinicalMedia { get; set; } = true;
        public bool IsBeforeAfterComparison { get; set; } = false;
        public Guid? RelatedAttachmentId { get; set; }
        public bool IsNeedReview { get; set; } = false;

        [MaxLength(500)] public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class ClinicalNoteAttachmentCreateResponse
    {
        public Guid Id { get; set; }
        public string AttachmentNumber { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public Guid? EncounterId { get; set; }
        public Guid? ConsultationId { get; set; }
        public Guid? PatientDiagnosisId { get; set; }
        public Guid? PatientProcedureId { get; set; }
        public ClinicalNoteAttachmentType AttachmentType { get; set; }
        public ClinicalNoteAttachmentContext AttachmentContext { get; set; }
        public ClinicalNoteAttachmentStatus AttachmentStatus { get; set; }
        public string AttachmentTitle { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public bool IsReviewed { get; set; }
        public bool IsVerified { get; set; }
        public bool IsArchived { get; set; }
    }

    public class ClinicalNoteAttachmentUpdateResponse : ClinicalNoteAttachmentCreateResponse
    {
    }

    public class ReviewClinicalNoteAttachmentRequest
    {
        [MaxLength(500)]
        public string? ReviewNote { get; set; }
    }

    public class VerifyClinicalNoteAttachmentRequest
    {
        [MaxLength(500)]
        public string? VerificationNote { get; set; }
    }

    public class ArchiveClinicalNoteAttachmentRequest
    {
        [Required]
        [MaxLength(250)]
        public string ArchiveReason { get; set; } = string.Empty;
    }

    public class CancelClinicalNoteAttachmentRequest
    {
        [Required]
        [MaxLength(250)]
        public string CancelReason { get; set; } = string.Empty;
    }
}
