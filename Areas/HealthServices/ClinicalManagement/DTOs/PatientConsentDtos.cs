using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs
{
    public class PatientConsentResponse
    {
        public Guid Id { get; set; }
        public string ConsentNumber { get; set; } = string.Empty;

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

        public Guid? PatientProcedureId { get; set; }
        public string? ProcedureNameSnapshot { get; set; }

        public Guid? ClinicalDocumentId { get; set; }
        public string? ClinicalDocumentNumber { get; set; }

        public Guid? DoctorId { get; set; }
        public string? DoctorName { get; set; }

        public Guid? ServiceUnitId { get; set; }
        public string? ServiceUnitName { get; set; }

        public Guid? ClinicId { get; set; }
        public string? ClinicName { get; set; }

        public PatientConsentType ConsentType { get; set; }
        public PatientConsentStatus ConsentStatus { get; set; }
        public PatientConsentMethod ConsentMethod { get; set; }

        public string ConsentTitle { get; set; } = string.Empty;
        public string? ConsentCode { get; set; }
        public string? ConsentCategoryName { get; set; }

        public PatientConsentSignerType SignerType { get; set; }
        public string SignerName { get; set; } = string.Empty;
        public string? SignerRelationship { get; set; }
        public bool IsSignerPatient { get; set; }
        public bool IsSignerLegalRepresentative { get; set; }

        public bool IsPatientAgreed { get; set; }
        public bool IsEmergencyConsent { get; set; }
        public bool IsHighRiskConsent { get; set; }
        public bool IsLegalDocument { get; set; }
        public bool IsPartOfMedicalRecord { get; set; }

        public DateTime ConsentDateTime { get; set; }
        public DateTime? SignedAt { get; set; }
        public DateTime? ExpiredDate { get; set; }

        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public Guid? VerifiedByUserId { get; set; }
        public string? VerifiedByUserName { get; set; }

        public bool IsApproved { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public string? ApprovedByUserName { get; set; }

        public bool IsRejected { get; set; }
        public bool IsWithdrawn { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class PatientConsentDetailResponse : PatientConsentResponse
    {
        public string? ConsentDescription { get; set; }

        public string? ProcedureCodeSnapshot { get; set; }
        public string? ProcedureTypeSnapshot { get; set; }
        public DateTime? PlannedProcedureDateTime { get; set; }
        public string? ProcedureLocation { get; set; }

        public string? DiagnosisExplanation { get; set; }
        public string? ProcedureExplanation { get; set; }
        public string? BenefitExplanation { get; set; }
        public string? RiskExplanation { get; set; }
        public string? AlternativeExplanation { get; set; }
        public string? ConsequenceExplanation { get; set; }
        public string? PatientQuestionNote { get; set; }

        public bool IsDiagnosisExplained { get; set; }
        public bool IsProcedureExplained { get; set; }
        public bool IsRiskExplained { get; set; }
        public bool IsAlternativeExplained { get; set; }
        public bool IsPatientUnderstood { get; set; }

        public string? SignerIdentityType { get; set; }
        public string? SignerIdentityNumber { get; set; }
        public string? SignerPhoneNumber { get; set; }
        public string? SignerAddress { get; set; }

        public Guid? ExplainedByDoctorId { get; set; }
        public string? ExplainedByDoctorName { get; set; }
        public Guid? ExplainedByUserId { get; set; }
        public string? ExplainedByUserName { get; set; }
        public DateTime? ExplainedAt { get; set; }

        public string? WitnessName { get; set; }
        public string? WitnessRelationship { get; set; }
        public Guid? WitnessByUserId { get; set; }
        public string? WitnessByUserName { get; set; }

        public string? PatientSignaturePath { get; set; }
        public string? SignerSignaturePath { get; set; }
        public string? DoctorSignaturePath { get; set; }
        public string? WitnessSignaturePath { get; set; }

        public string? ConsentFilePath { get; set; }
        public string? ConsentFileName { get; set; }
        public string? ConsentMimeType { get; set; }
        public long? ConsentFileSizeBytes { get; set; }
        public string? ConsentFileHash { get; set; }

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        public string? VerificationNote { get; set; }
        public string? ApprovalNote { get; set; }

        public DateTime? RejectedAt { get; set; }
        public Guid? RejectedByUserId { get; set; }
        public string? RejectedByUserName { get; set; }
        public string? RejectionReason { get; set; }

        public DateTime? WithdrawnAt { get; set; }
        public Guid? WithdrawnByUserId { get; set; }
        public string? WithdrawnByUserName { get; set; }
        public string? WithdrawalReason { get; set; }

        public DateTime? CancelledAt { get; set; }
        public Guid? CancelledByUserId { get; set; }
        public string? CancelledByUserName { get; set; }
        public string? CancelReason { get; set; }

        public string? Notes { get; set; }
    }

    public class PatientConsentOptionResponse
    {
        public Guid Id { get; set; }
        public string ConsentNumber { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public Guid? EncounterId { get; set; }
        public Guid? ConsultationId { get; set; }
        public Guid? PatientProcedureId { get; set; }
        public PatientConsentType ConsentType { get; set; }
        public PatientConsentStatus ConsentStatus { get; set; }
        public PatientConsentMethod ConsentMethod { get; set; }
        public string ConsentTitle { get; set; } = string.Empty;
        public string SignerName { get; set; } = string.Empty;
        public DateTime ConsentDateTime { get; set; }
        public DateTime? SignedAt { get; set; }
        public bool IsPatientAgreed { get; set; }
        public bool IsVerified { get; set; }
        public bool IsApproved { get; set; }
    }

    public class PatientConsentFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public PatientConsentDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<PatientConsentSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<PatientConsentEnumOptionResponse> ConsentTypeOptions { get; set; } = new();
        public List<PatientConsentEnumOptionResponse> ConsentStatusOptions { get; set; } = new();
        public List<PatientConsentEnumOptionResponse> ConsentMethodOptions { get; set; } = new();
        public List<PatientConsentEnumOptionResponse> SignerTypeOptions { get; set; } = new();
    }

    public class PatientConsentDefaultFilterResponse
    {
        public string? Search { get; set; }
        public Guid? PatientId { get; set; }
        public Guid? EncounterId { get; set; }
        public Guid? QueueId { get; set; }
        public Guid? AssessmentId { get; set; }
        public Guid? ConsultationId { get; set; }
        public Guid? PatientProcedureId { get; set; }
        public Guid? ClinicalDocumentId { get; set; }
        public Guid? DoctorId { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public Guid? ClinicId { get; set; }
        public PatientConsentType? ConsentType { get; set; }
        public PatientConsentStatus? ConsentStatus { get; set; }
        public PatientConsentMethod? ConsentMethod { get; set; }
        public PatientConsentSignerType? SignerType { get; set; }
        public bool? IsPatientAgreed { get; set; }
        public bool? IsEmergencyConsent { get; set; }
        public bool? IsHighRiskConsent { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsApproved { get; set; }
        public bool? IsRejected { get; set; }
        public bool? IsWithdrawn { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SortBy { get; set; } = "consentDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class PatientConsentSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class PatientConsentEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreatePatientConsentRequest
    {
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

        public PatientConsentType ConsentType { get; set; } = PatientConsentType.Unknown;
        public PatientConsentStatus ConsentStatus { get; set; } = PatientConsentStatus.Draft;
        public PatientConsentMethod ConsentMethod { get; set; } = PatientConsentMethod.WrittenPaper;

        [Required]
        [MaxLength(250)]
        public string ConsentTitle { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ConsentCode { get; set; }

        [MaxLength(250)]
        public string? ConsentCategoryName { get; set; }

        [MaxLength(1000)]
        public string? ConsentDescription { get; set; }

        [MaxLength(50)]
        public string? ProcedureCodeSnapshot { get; set; }

        [MaxLength(250)]
        public string? ProcedureNameSnapshot { get; set; }

        [MaxLength(100)]
        public string? ProcedureTypeSnapshot { get; set; }

        public DateTime? PlannedProcedureDateTime { get; set; }

        [MaxLength(250)]
        public string? ProcedureLocation { get; set; }

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

        public PatientConsentSignerType SignerType { get; set; } = PatientConsentSignerType.Patient;

        [Required]
        [MaxLength(200)]
        public string SignerName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? SignerRelationship { get; set; }

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

        public Guid? ExplainedByDoctorId { get; set; }
        public Guid? ExplainedByUserId { get; set; }
        public DateTime? ExplainedAt { get; set; }

        [MaxLength(200)]
        public string? WitnessName { get; set; }

        [MaxLength(100)]
        public string? WitnessRelationship { get; set; }

        public Guid? WitnessByUserId { get; set; }

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

        [MaxLength(250)]
        public string? ConsentFileName { get; set; }

        [MaxLength(150)]
        public string? ConsentMimeType { get; set; }

        public long? ConsentFileSizeBytes { get; set; }

        [MaxLength(256)]
        public string? ConsentFileHash { get; set; }

        public DateTime? ConsentDateTime { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public DateTime? ExpiredDate { get; set; }

        public bool IsVerified { get; set; } = false;
        public bool IsApproved { get; set; } = false;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class UpdatePatientConsentRequest
    {
        public PatientConsentType ConsentType { get; set; } = PatientConsentType.Unknown;
        public PatientConsentStatus ConsentStatus { get; set; } = PatientConsentStatus.Draft;
        public PatientConsentMethod ConsentMethod { get; set; } = PatientConsentMethod.WrittenPaper;

        [Required]
        [MaxLength(250)]
        public string ConsentTitle { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ConsentCode { get; set; }

        [MaxLength(250)]
        public string? ConsentCategoryName { get; set; }

        [MaxLength(1000)]
        public string? ConsentDescription { get; set; }

        [MaxLength(50)]
        public string? ProcedureCodeSnapshot { get; set; }

        [MaxLength(250)]
        public string? ProcedureNameSnapshot { get; set; }

        [MaxLength(100)]
        public string? ProcedureTypeSnapshot { get; set; }

        public DateTime? PlannedProcedureDateTime { get; set; }

        [MaxLength(250)]
        public string? ProcedureLocation { get; set; }

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

        public PatientConsentSignerType SignerType { get; set; } = PatientConsentSignerType.Patient;

        [Required]
        [MaxLength(200)]
        public string SignerName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? SignerRelationship { get; set; }

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

        public Guid? ExplainedByDoctorId { get; set; }
        public Guid? ExplainedByUserId { get; set; }
        public DateTime? ExplainedAt { get; set; }

        [MaxLength(200)]
        public string? WitnessName { get; set; }

        [MaxLength(100)]
        public string? WitnessRelationship { get; set; }

        public Guid? WitnessByUserId { get; set; }

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

        [MaxLength(250)]
        public string? ConsentFileName { get; set; }

        [MaxLength(150)]
        public string? ConsentMimeType { get; set; }

        public long? ConsentFileSizeBytes { get; set; }

        [MaxLength(256)]
        public string? ConsentFileHash { get; set; }

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public DateTime? ExpiredDate { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class PatientConsentCreateResponse
    {
        public Guid Id { get; set; }
        public string ConsentNumber { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public Guid? EncounterId { get; set; }
        public Guid? ConsultationId { get; set; }
        public Guid? PatientProcedureId { get; set; }
        public Guid? ClinicalDocumentId { get; set; }
        public PatientConsentType ConsentType { get; set; }
        public PatientConsentStatus ConsentStatus { get; set; }
        public PatientConsentMethod ConsentMethod { get; set; }
        public string ConsentTitle { get; set; } = string.Empty;
        public string SignerName { get; set; } = string.Empty;
        public bool IsPatientAgreed { get; set; }
        public DateTime ConsentDateTime { get; set; }
        public DateTime? SignedAt { get; set; }
        public bool IsVerified { get; set; }
        public bool IsApproved { get; set; }
    }

    public class PatientConsentUpdateResponse : PatientConsentCreateResponse
    {
    }

    public class SignPatientConsentRequest
    {
        public bool IsPatientAgreed { get; set; } = true;

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

        [MaxLength(250)]
        public string? ConsentFileName { get; set; }

        [MaxLength(150)]
        public string? ConsentMimeType { get; set; }

        public long? ConsentFileSizeBytes { get; set; }

        [MaxLength(256)]
        public string? ConsentFileHash { get; set; }
    }

    public class VerifyPatientConsentRequest
    {
        [MaxLength(500)]
        public string? VerificationNote { get; set; }
    }

    public class ApprovePatientConsentRequest
    {
        [MaxLength(500)]
        public string? ApprovalNote { get; set; }
    }

    public class RejectPatientConsentRequest
    {
        [Required]
        [MaxLength(500)]
        public string RejectionReason { get; set; } = string.Empty;
    }

    public class WithdrawPatientConsentRequest
    {
        [Required]
        [MaxLength(500)]
        public string WithdrawalReason { get; set; } = string.Empty;
    }

    public class CancelPatientConsentRequest
    {
        [Required]
        [MaxLength(250)]
        public string CancelReason { get; set; } = string.Empty;
    }
}
