using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs
{
    public class MedicalCertificateResponse
    {
        public Guid Id { get; set; }
        public string MedicalCertificateNumber { get; set; } = string.Empty;

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
        public Guid? DiagnosisId { get; set; }

        public Guid? ClinicalDocumentId { get; set; }
        public string? ClinicalDocumentNumber { get; set; }

        public Guid? DoctorId { get; set; }
        public string? DoctorName { get; set; }

        public Guid? ServiceUnitId { get; set; }
        public string? ServiceUnitName { get; set; }

        public Guid? ClinicId { get; set; }
        public string? ClinicName { get; set; }

        public MedicalCertificateType CertificateType { get; set; }
        public MedicalCertificateStatus CertificateStatus { get; set; }
        public MedicalCertificateRecipientType RecipientType { get; set; }
        public MedicalCertificateDeliveryMethod DeliveryMethod { get; set; }

        public string CertificateTitle { get; set; } = string.Empty;
        public string? CertificateCode { get; set; }
        public string? CertificateCategoryName { get; set; }
        public string? CertificatePurpose { get; set; }

        public string? DiagnosisCodeSnapshot { get; set; }
        public string? DiagnosisNameSnapshot { get; set; }
        public string DiagnosisMasterType { get; set; } = string.Empty;

        public DateTime CertificateDateTime { get; set; }
        public DateTime? IssuedAt { get; set; }
        public Guid? IssuedByDoctorId { get; set; }
        public string? IssuedByDoctorName { get; set; }
        public Guid? IssuedByUserId { get; set; }
        public string? IssuedByUserName { get; set; }

        public DateTime? SickLeaveStartDate { get; set; }
        public DateTime? SickLeaveEndDate { get; set; }
        public int? SickLeaveDays { get; set; }

        public DateTime? ControlDate { get; set; }
        public DateTime? ReferralDate { get; set; }
        public DateTime? ExpiredDate { get; set; }

        public MedicalFitnessStatus FitnessStatus { get; set; }

        public bool IsIssued { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public Guid? VerifiedByUserId { get; set; }
        public string? VerifiedByUserName { get; set; }

        public bool IsApproved { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public string? ApprovedByUserName { get; set; }

        public bool IsRejected { get; set; }
        public bool IsRevoked { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class MedicalCertificateDetailResponse : MedicalCertificateResponse
    {
        public string? IcdVersion { get; set; }
        public string? ClinicalSummary { get; set; }
        public string? MedicalRecommendation { get; set; }
        public string? CertificateStatement { get; set; }
        public string? AdditionalStatement { get; set; }
        public string? RestrictionNote { get; set; }
        public string? FollowUpInstruction { get; set; }

        public string? SickLeaveReason { get; set; }
        public string? ControlClinicName { get; set; }
        public string? ReferralToProviderName { get; set; }
        public string? ReferralToDepartmentName { get; set; }
        public string? ReferralReason { get; set; }

        public DateTime? AdmissionDate { get; set; }
        public DateTime? DischargeDate { get; set; }
        public DateTime? DeathDateTime { get; set; }
        public string? CauseOfDeath { get; set; }

        public string? FitnessAssessmentNote { get; set; }
        public string? WorkRestrictionNote { get; set; }

        public string? RequestedByName { get; set; }
        public string? RequestedByRelationship { get; set; }
        public string? RecipientName { get; set; }
        public string? RecipientInstitutionName { get; set; }
        public string? RecipientAddress { get; set; }

        public string? IssuePlace { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        public string? CertificateFilePath { get; set; }
        public string? CertificateFileName { get; set; }
        public string? CertificateMimeType { get; set; }
        public long? CertificateFileSizeBytes { get; set; }
        public string? CertificateFileHash { get; set; }

        public string? QrCodePath { get; set; }
        public string? VerificationCode { get; set; }
        public string? VerificationUrl { get; set; }

        public string? VerificationNote { get; set; }
        public string? ApprovalNote { get; set; }

        public DateTime? RejectedAt { get; set; }
        public Guid? RejectedByUserId { get; set; }
        public string? RejectedByUserName { get; set; }
        public string? RejectionReason { get; set; }

        public DateTime? RevokedAt { get; set; }
        public Guid? RevokedByUserId { get; set; }
        public string? RevokedByUserName { get; set; }
        public string? RevocationReason { get; set; }

        public DateTime? CancelledAt { get; set; }
        public Guid? CancelledByUserId { get; set; }
        public string? CancelledByUserName { get; set; }
        public string? CancelReason { get; set; }
        public string? Notes { get; set; }
    }

    public class MedicalCertificateOptionResponse
    {
        public Guid Id { get; set; }
        public string MedicalCertificateNumber { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public Guid? EncounterId { get; set; }
        public Guid? ConsultationId { get; set; }
        public MedicalCertificateType CertificateType { get; set; }
        public MedicalCertificateStatus CertificateStatus { get; set; }
        public string CertificateTitle { get; set; } = string.Empty;
        public DateTime CertificateDateTime { get; set; }
        public DateTime? IssuedAt { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public bool IsIssued { get; set; }
        public bool IsVerified { get; set; }
        public bool IsApproved { get; set; }
    }

    public class MedicalCertificateFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public MedicalCertificateDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<MedicalCertificateSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<MedicalCertificateEnumOptionResponse> CertificateTypeOptions { get; set; } = new();
        public List<MedicalCertificateEnumOptionResponse> CertificateStatusOptions { get; set; } = new();
        public List<MedicalCertificateEnumOptionResponse> RecipientTypeOptions { get; set; } = new();
        public List<MedicalCertificateEnumOptionResponse> DeliveryMethodOptions { get; set; } = new();
        public List<MedicalCertificateEnumOptionResponse> FitnessStatusOptions { get; set; } = new();
    }

    public class MedicalCertificateDefaultFilterResponse
    {
        public string? Search { get; set; }
        public Guid? PatientId { get; set; }
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
        public MedicalCertificateType? CertificateType { get; set; }
        public MedicalCertificateStatus? CertificateStatus { get; set; }
        public MedicalCertificateRecipientType? RecipientType { get; set; }
        public MedicalCertificateDeliveryMethod? DeliveryMethod { get; set; }
        public MedicalFitnessStatus? FitnessStatus { get; set; }
        public bool? IsIssued { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsApproved { get; set; }
        public bool? IsRejected { get; set; }
        public bool? IsRevoked { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public string SortBy { get; set; } = "certificateDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class MedicalCertificateSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class MedicalCertificateEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateMedicalCertificateRequest
    {
        [Required] public Guid PatientId { get; set; }
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

        public MedicalCertificateType CertificateType { get; set; } = MedicalCertificateType.Unknown;
        public MedicalCertificateStatus CertificateStatus { get; set; } = MedicalCertificateStatus.Draft;
        public MedicalCertificateRecipientType RecipientType { get; set; } = MedicalCertificateRecipientType.Patient;
        public MedicalCertificateDeliveryMethod DeliveryMethod { get; set; } = MedicalCertificateDeliveryMethod.Printed;

        [Required, MaxLength(250)] public string CertificateTitle { get; set; } = string.Empty;
        [MaxLength(100)] public string? CertificateCode { get; set; }
        [MaxLength(250)] public string? CertificateCategoryName { get; set; }
        [MaxLength(250)] public string? CertificatePurpose { get; set; }

        [MaxLength(50)] public string? DiagnosisCodeSnapshot { get; set; }
        [MaxLength(500)] public string? DiagnosisNameSnapshot { get; set; }
        [MaxLength(50)] public string? DiagnosisMasterType { get; set; }
        [MaxLength(100)] public string? IcdVersion { get; set; }
        [MaxLength(1000)] public string? ClinicalSummary { get; set; }
        [MaxLength(1000)] public string? MedicalRecommendation { get; set; }
        [MaxLength(4000)] public string? CertificateStatement { get; set; }
        [MaxLength(2000)] public string? AdditionalStatement { get; set; }
        [MaxLength(1000)] public string? RestrictionNote { get; set; }
        [MaxLength(1000)] public string? FollowUpInstruction { get; set; }

        public DateTime? SickLeaveStartDate { get; set; }
        public DateTime? SickLeaveEndDate { get; set; }
        public int? SickLeaveDays { get; set; }
        [MaxLength(250)] public string? SickLeaveReason { get; set; }

        public DateTime? ControlDate { get; set; }
        [MaxLength(250)] public string? ControlClinicName { get; set; }
        public DateTime? ReferralDate { get; set; }
        [MaxLength(250)] public string? ReferralToProviderName { get; set; }
        [MaxLength(250)] public string? ReferralToDepartmentName { get; set; }
        [MaxLength(1000)] public string? ReferralReason { get; set; }

        public DateTime? AdmissionDate { get; set; }
        public DateTime? DischargeDate { get; set; }
        public DateTime? DeathDateTime { get; set; }
        [MaxLength(500)] public string? CauseOfDeath { get; set; }
        public MedicalFitnessStatus FitnessStatus { get; set; } = MedicalFitnessStatus.NotAssessed;
        [MaxLength(1000)] public string? FitnessAssessmentNote { get; set; }
        [MaxLength(1000)] public string? WorkRestrictionNote { get; set; }

        [MaxLength(200)] public string? RequestedByName { get; set; }
        [MaxLength(100)] public string? RequestedByRelationship { get; set; }
        [MaxLength(250)] public string? RecipientName { get; set; }
        [MaxLength(250)] public string? RecipientInstitutionName { get; set; }
        [MaxLength(500)] public string? RecipientAddress { get; set; }

        public DateTime? CertificateDateTime { get; set; }
        public DateTime? IssuedAt { get; set; }
        public Guid? IssuedByDoctorId { get; set; }
        public Guid? IssuedByUserId { get; set; }
        [MaxLength(250)] public string? IssuePlace { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public DateTime? ExpiredDate { get; set; }

        [MaxLength(500)] public string? CertificateFilePath { get; set; }
        [MaxLength(250)] public string? CertificateFileName { get; set; }
        [MaxLength(150)] public string? CertificateMimeType { get; set; }
        public long? CertificateFileSizeBytes { get; set; }
        [MaxLength(256)] public string? CertificateFileHash { get; set; }
        [MaxLength(500)] public string? QrCodePath { get; set; }
        [MaxLength(250)] public string? VerificationCode { get; set; }
        [MaxLength(500)] public string? VerificationUrl { get; set; }

        public bool IsIssued { get; set; } = false;
        public bool IsVerified { get; set; } = false;
        public bool IsApproved { get; set; } = false;
        [MaxLength(500)] public string? Notes { get; set; }
    }

    public class UpdateMedicalCertificateRequest : CreateMedicalCertificateRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class MedicalCertificateCreateResponse
    {
        public Guid Id { get; set; }
        public string MedicalCertificateNumber { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public Guid? EncounterId { get; set; }
        public Guid? ConsultationId { get; set; }
        public MedicalCertificateType CertificateType { get; set; }
        public MedicalCertificateStatus CertificateStatus { get; set; }
        public string CertificateTitle { get; set; } = string.Empty;
        public DateTime CertificateDateTime { get; set; }
        public DateTime? IssuedAt { get; set; }
        public bool IsIssued { get; set; }
        public bool IsVerified { get; set; }
        public bool IsApproved { get; set; }
    }

    public class MedicalCertificateUpdateResponse : MedicalCertificateCreateResponse { }

    public class IssueMedicalCertificateRequest
    {
        public Guid? IssuedByDoctorId { get; set; }
        [MaxLength(250)] public string? IssuePlace { get; set; }
        [MaxLength(500)] public string? CertificateFilePath { get; set; }
        [MaxLength(250)] public string? CertificateFileName { get; set; }
        [MaxLength(150)] public string? CertificateMimeType { get; set; }
        public long? CertificateFileSizeBytes { get; set; }
        [MaxLength(256)] public string? CertificateFileHash { get; set; }
        [MaxLength(500)] public string? QrCodePath { get; set; }
        [MaxLength(250)] public string? VerificationCode { get; set; }
        [MaxLength(500)] public string? VerificationUrl { get; set; }
    }

    public class VerifyMedicalCertificateRequest
    {
        [MaxLength(500)] public string? VerificationNote { get; set; }
    }

    public class ApproveMedicalCertificateRequest
    {
        [MaxLength(500)] public string? ApprovalNote { get; set; }
    }

    public class RejectMedicalCertificateRequest
    {
        [Required, MaxLength(500)] public string RejectionReason { get; set; } = string.Empty;
    }

    public class RevokeMedicalCertificateRequest
    {
        [Required, MaxLength(500)] public string RevocationReason { get; set; } = string.Empty;
    }

    public class CancelMedicalCertificateRequest
    {
        [Required, MaxLength(250)] public string CancelReason { get; set; } = string.Empty;
    }
}
