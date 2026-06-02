using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs
{
    public class PatientClinicalDocumentResponse
    {
        public Guid Id { get; set; }
        public string ClinicalDocumentNumber { get; set; } = string.Empty;

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
        public string? DiagnosisCode { get; set; }
        public string? DiagnosisName { get; set; }

        public Guid? PatientProcedureId { get; set; }
        public string? ProcedureCode { get; set; }
        public string? ProcedureName { get; set; }

        public Guid? DoctorId { get; set; }
        public string? DoctorName { get; set; }

        public Guid? ServiceUnitId { get; set; }
        public string? ServiceUnitName { get; set; }

        public Guid? ClinicId { get; set; }
        public string? ClinicName { get; set; }

        public PatientClinicalDocumentType DocumentType { get; set; }
        public PatientClinicalDocumentSource DocumentSource { get; set; }
        public PatientClinicalDocumentStatus DocumentStatus { get; set; }
        public PatientClinicalDocumentConfidentialityLevel ConfidentialityLevel { get; set; }

        public string DocumentTitle { get; set; } = string.Empty;
        public string? DocumentCode { get; set; }
        public string? DocumentCategoryName { get; set; }
        public string? ExternalDocumentNumber { get; set; }
        public string? ExternalProviderName { get; set; }
        public string? ExternalDoctorName { get; set; }

        public DateTime DocumentDateTime { get; set; }
        public DateTime? ReceivedDateTime { get; set; }
        public DateTime? UploadedDateTime { get; set; }
        public DateTime? ExpiredDate { get; set; }

        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string? OriginalFileName { get; set; }
        public string? FileExtension { get; set; }
        public string? MimeType { get; set; }
        public long? FileSizeBytes { get; set; }
        public string? StorageProvider { get; set; }

        public bool IsConfidential { get; set; }
        public bool IsPatientVisible { get; set; }
        public bool IsShareable { get; set; }
        public bool IsExternalDocument { get; set; }
        public bool IsPartOfMedicalRecord { get; set; }
        public bool IsLegalDocument { get; set; }
        public bool IsNeedReview { get; set; }
        public bool IsReviewed { get; set; }
        public bool IsVerified { get; set; }
        public bool IsApproved { get; set; }
        public bool IsArchived { get; set; }

        public Guid? UploadedByUserId { get; set; }
        public string? UploadedByUserName { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class PatientClinicalDocumentDetailResponse : PatientClinicalDocumentResponse
    {
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        public string? FileHash { get; set; }
        public string? ThumbnailPath { get; set; }
        public string? PreviewPath { get; set; }
        public int? PageCount { get; set; }

        public string? DocumentSummary { get; set; }
        public string? ClinicalFindingSummary { get; set; }
        public string? Impression { get; set; }
        public string? Recommendation { get; set; }
        public string? Keywords { get; set; }

        public DateTime? ReviewedAt { get; set; }
        public Guid? ReviewedByUserId { get; set; }
        public string? ReviewedByUserName { get; set; }
        public string? ReviewNote { get; set; }

        public DateTime? VerifiedAt { get; set; }
        public Guid? VerifiedByUserId { get; set; }
        public string? VerifiedByUserName { get; set; }
        public string? VerificationNote { get; set; }

        public DateTime? ApprovedAt { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public string? ApprovedByUserName { get; set; }
        public string? ApprovalNote { get; set; }

        public DateTime? ArchivedAt { get; set; }
        public Guid? ArchivedByUserId { get; set; }
        public string? ArchivedByUserName { get; set; }
        public string? ArchiveReason { get; set; }

        public DateTime? CancelledAt { get; set; }
        public Guid? CancelledByUserId { get; set; }
        public string? CancelledByUserName { get; set; }
        public string? CancelReason { get; set; }

        public string? Notes { get; set; }
    }

    public class PatientClinicalDocumentOptionResponse
    {
        public Guid Id { get; set; }
        public string ClinicalDocumentNumber { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public PatientClinicalDocumentType DocumentType { get; set; }
        public PatientClinicalDocumentStatus DocumentStatus { get; set; }
        public string DocumentTitle { get; set; } = string.Empty;
        public DateTime DocumentDateTime { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string? FileExtension { get; set; }
        public bool IsConfidential { get; set; }
        public bool IsPartOfMedicalRecord { get; set; }
        public bool IsVerified { get; set; }
        public bool IsApproved { get; set; }
        public bool IsArchived { get; set; }
    }

    public class PatientClinicalDocumentFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public PatientClinicalDocumentDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<PatientClinicalDocumentSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<PatientClinicalDocumentEnumOptionResponse> DocumentTypeOptions { get; set; } = new();
        public List<PatientClinicalDocumentEnumOptionResponse> DocumentSourceOptions { get; set; } = new();
        public List<PatientClinicalDocumentEnumOptionResponse> DocumentStatusOptions { get; set; } = new();
        public List<PatientClinicalDocumentEnumOptionResponse> ConfidentialityLevelOptions { get; set; } = new();
    }

    public class PatientClinicalDocumentDefaultFilterResponse
    {
        public string? Search { get; set; }
        public Guid? PatientId { get; set; }
        public Guid? EncounterId { get; set; }
        public Guid? QueueId { get; set; }
        public Guid? AssessmentId { get; set; }
        public Guid? ConsultationId { get; set; }
        public Guid? PatientDiagnosisId { get; set; }
        public Guid? PatientProcedureId { get; set; }
        public Guid? DoctorId { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public Guid? ClinicId { get; set; }
        public PatientClinicalDocumentType? DocumentType { get; set; }
        public PatientClinicalDocumentSource? DocumentSource { get; set; }
        public PatientClinicalDocumentStatus? DocumentStatus { get; set; }
        public PatientClinicalDocumentConfidentialityLevel? ConfidentialityLevel { get; set; }
        public bool? IsConfidential { get; set; }
        public bool? IsPatientVisible { get; set; }
        public bool? IsShareable { get; set; }
        public bool? IsExternalDocument { get; set; }
        public bool? IsPartOfMedicalRecord { get; set; }
        public bool? IsLegalDocument { get; set; }
        public bool? IsNeedReview { get; set; }
        public bool? IsReviewed { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsApproved { get; set; }
        public bool? IsArchived { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SortBy { get; set; } = "documentDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class PatientClinicalDocumentSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class PatientClinicalDocumentEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreatePatientClinicalDocumentRequest
    {
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

        public PatientClinicalDocumentType DocumentType { get; set; } = PatientClinicalDocumentType.Unknown;
        public PatientClinicalDocumentSource DocumentSource { get; set; } = PatientClinicalDocumentSource.InternalUpload;
        public PatientClinicalDocumentStatus DocumentStatus { get; set; } = PatientClinicalDocumentStatus.Uploaded;
        public PatientClinicalDocumentConfidentialityLevel ConfidentialityLevel { get; set; } = PatientClinicalDocumentConfidentialityLevel.Normal;

        [Required]
        [MaxLength(250)]
        public string DocumentTitle { get; set; } = string.Empty;

        [MaxLength(100)] public string? DocumentCode { get; set; }
        [MaxLength(250)] public string? DocumentCategoryName { get; set; }
        [MaxLength(250)] public string? ExternalDocumentNumber { get; set; }
        [MaxLength(250)] public string? ExternalProviderName { get; set; }
        [MaxLength(250)] public string? ExternalDoctorName { get; set; }

        public DateTime? DocumentDateTime { get; set; }
        public DateTime? ReceivedDateTime { get; set; }
        public DateTime? UploadedDateTime { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public DateTime? ExpiredDate { get; set; }

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
        public int? PageCount { get; set; }

        [MaxLength(1000)] public string? DocumentSummary { get; set; }
        [MaxLength(1000)] public string? ClinicalFindingSummary { get; set; }
        [MaxLength(1000)] public string? Impression { get; set; }
        [MaxLength(1000)] public string? Recommendation { get; set; }
        [MaxLength(500)] public string? Keywords { get; set; }

        public bool IsConfidential { get; set; } = false;
        public bool IsPatientVisible { get; set; } = false;
        public bool IsShareable { get; set; } = false;
        public bool IsExternalDocument { get; set; } = false;
        public bool IsPartOfMedicalRecord { get; set; } = true;
        public bool IsLegalDocument { get; set; } = false;
        public bool IsNeedReview { get; set; } = false;
        public bool IsVerified { get; set; } = false;
        public bool IsApproved { get; set; } = false;
        [MaxLength(500)] public string? Notes { get; set; }
    }

    public class UpdatePatientClinicalDocumentRequest
    {
        public Guid? EncounterId { get; set; }
        public Guid? QueueId { get; set; }
        public Guid? AssessmentId { get; set; }
        public Guid? ConsultationId { get; set; }
        public Guid? PatientDiagnosisId { get; set; }
        public Guid? PatientProcedureId { get; set; }
        public Guid? DoctorId { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public Guid? ClinicId { get; set; }

        public PatientClinicalDocumentType DocumentType { get; set; } = PatientClinicalDocumentType.Unknown;
        public PatientClinicalDocumentSource DocumentSource { get; set; } = PatientClinicalDocumentSource.InternalUpload;
        public PatientClinicalDocumentStatus DocumentStatus { get; set; } = PatientClinicalDocumentStatus.Uploaded;
        public PatientClinicalDocumentConfidentialityLevel ConfidentialityLevel { get; set; } = PatientClinicalDocumentConfidentialityLevel.Normal;

        [Required]
        [MaxLength(250)]
        public string DocumentTitle { get; set; } = string.Empty;

        [MaxLength(100)] public string? DocumentCode { get; set; }
        [MaxLength(250)] public string? DocumentCategoryName { get; set; }
        [MaxLength(250)] public string? ExternalDocumentNumber { get; set; }
        [MaxLength(250)] public string? ExternalProviderName { get; set; }
        [MaxLength(250)] public string? ExternalDoctorName { get; set; }

        public DateTime? DocumentDateTime { get; set; }
        public DateTime? ReceivedDateTime { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public DateTime? ExpiredDate { get; set; }

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
        public int? PageCount { get; set; }

        [MaxLength(1000)] public string? DocumentSummary { get; set; }
        [MaxLength(1000)] public string? ClinicalFindingSummary { get; set; }
        [MaxLength(1000)] public string? Impression { get; set; }
        [MaxLength(1000)] public string? Recommendation { get; set; }
        [MaxLength(500)] public string? Keywords { get; set; }

        public bool IsConfidential { get; set; } = false;
        public bool IsPatientVisible { get; set; } = false;
        public bool IsShareable { get; set; } = false;
        public bool IsExternalDocument { get; set; } = false;
        public bool IsPartOfMedicalRecord { get; set; } = true;
        public bool IsLegalDocument { get; set; } = false;
        public bool IsNeedReview { get; set; } = false;
        public bool IsActive { get; set; } = true;
        [MaxLength(500)] public string? Notes { get; set; }
    }

    public class PatientClinicalDocumentCreateResponse
    {
        public Guid Id { get; set; }
        public string ClinicalDocumentNumber { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public Guid? EncounterId { get; set; }
        public Guid? ConsultationId { get; set; }
        public Guid? PatientDiagnosisId { get; set; }
        public Guid? PatientProcedureId { get; set; }
        public PatientClinicalDocumentType DocumentType { get; set; }
        public PatientClinicalDocumentStatus DocumentStatus { get; set; }
        public string DocumentTitle { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public bool IsPartOfMedicalRecord { get; set; }
        public bool IsVerified { get; set; }
        public bool IsApproved { get; set; }
        public int? ClinicalDocumentCount { get; set; }
    }

    public class PatientClinicalDocumentUpdateResponse : PatientClinicalDocumentCreateResponse
    {
    }

    public class ReviewPatientClinicalDocumentRequest
    {
        [MaxLength(500)]
        public string? ReviewNote { get; set; }
    }

    public class VerifyPatientClinicalDocumentRequest
    {
        [MaxLength(500)]
        public string? VerificationNote { get; set; }
    }

    public class ApprovePatientClinicalDocumentRequest
    {
        [MaxLength(500)]
        public string? ApprovalNote { get; set; }
    }

    public class ArchivePatientClinicalDocumentRequest
    {
        [Required]
        [MaxLength(250)]
        public string ArchiveReason { get; set; } = string.Empty;
    }

    public class CancelPatientClinicalDocumentRequest
    {
        [Required]
        [MaxLength(250)]
        public string CancelReason { get; set; } = string.Empty;
    }
}
