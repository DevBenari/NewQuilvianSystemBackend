using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs
{
    public class PatientAllergyResponse
    {
        public Guid Id { get; set; }

        public string AllergyRecordNumber { get; set; } = string.Empty;

        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string MedicalRecordNumber { get; set; } = string.Empty;

        public Guid? EncounterId { get; set; }
        public string? EncounterNumber { get; set; }

        public Guid? ConsultationId { get; set; }
        public string? ConsultationNumber { get; set; }

        public Guid? AssessmentId { get; set; }
        public string? AssessmentNumber { get; set; }

        public Guid? DoctorId { get; set; }
        public string? DoctorName { get; set; }

        public Guid? ServiceUnitId { get; set; }
        public string? ServiceUnitName { get; set; }

        public Guid? ClinicId { get; set; }
        public string? ClinicName { get; set; }

        public Guid? DrugId { get; set; }

        public PatientAllergyCategory AllergyCategory { get; set; }

        public string? AllergenCode { get; set; }

        public string AllergenName { get; set; } = string.Empty;

        public string? AllergenGroupName { get; set; }

        public string? ReactionType { get; set; }

        public PatientAllergySeverity Severity { get; set; }

        public PatientAllergyCertainty Certainty { get; set; }

        public PatientAllergyStatus AllergyStatus { get; set; }

        public DateTime? FirstReactionDate { get; set; }

        public DateTime? LastReactionDate { get; set; }

        public DateTime ReportedDateTime { get; set; }

        public string? SourceOfInformation { get; set; }

        public bool IsHighRisk { get; set; }

        public bool IsLifeThreatening { get; set; }

        public bool IsAlertEnabled { get; set; }

        public bool IsVerified { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public Guid? VerifiedByUserId { get; set; }

        public string? VerifiedByUserName { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class PatientAllergyDetailResponse : PatientAllergyResponse
    {
        public string? ReactionDescription { get; set; }

        public string? PatientSafetyNote { get; set; }

        public string? ClinicalNote { get; set; }

        public string? Notes { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public Guid? ResolvedByUserId { get; set; }

        public string? ResolvedByUserName { get; set; }

        public string? ResolvedReason { get; set; }

        public DateTime? CancelledAt { get; set; }

        public Guid? CancelledByUserId { get; set; }

        public string? CancelledByUserName { get; set; }

        public string? CancelReason { get; set; }
    }

    public class PatientAllergyOptionResponse
    {
        public Guid Id { get; set; }

        public Guid PatientId { get; set; }

        public string AllergyRecordNumber { get; set; } = string.Empty;

        public PatientAllergyCategory AllergyCategory { get; set; }

        public string AllergenName { get; set; } = string.Empty;

        public string? AllergenGroupName { get; set; }

        public string? ReactionType { get; set; }

        public PatientAllergySeverity Severity { get; set; }

        public PatientAllergyCertainty Certainty { get; set; }

        public PatientAllergyStatus AllergyStatus { get; set; }

        public bool IsHighRisk { get; set; }

        public bool IsLifeThreatening { get; set; }

        public bool IsAlertEnabled { get; set; }

        public bool IsVerified { get; set; }
    }

    public class PatientAllergyAlertResponse
    {
        public Guid Id { get; set; }

        public Guid PatientId { get; set; }

        public string AllergyRecordNumber { get; set; } = string.Empty;

        public PatientAllergyCategory AllergyCategory { get; set; }

        public Guid? DrugId { get; set; }

        public string? AllergenCode { get; set; }

        public string AllergenName { get; set; } = string.Empty;

        public string? AllergenGroupName { get; set; }

        public string? ReactionType { get; set; }

        public string? ReactionDescription { get; set; }

        public PatientAllergySeverity Severity { get; set; }

        public PatientAllergyCertainty Certainty { get; set; }

        public bool IsHighRisk { get; set; }

        public bool IsLifeThreatening { get; set; }

        public string? PatientSafetyNote { get; set; }
    }

    public class PatientAllergyFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public PatientAllergyDefaultFilterResponse DefaultFilter { get; set; } = new();

        public List<PatientAllergySortOptionResponse> SortOptions { get; set; } = new();

        public List<string> SortDirections { get; set; } = new();

        public List<int> PageSizeOptions { get; set; } = new();

        public List<PatientAllergyEnumOptionResponse> AllergyCategoryOptions { get; set; } = new();

        public List<PatientAllergyEnumOptionResponse> SeverityOptions { get; set; } = new();

        public List<PatientAllergyEnumOptionResponse> CertaintyOptions { get; set; } = new();

        public List<PatientAllergyEnumOptionResponse> AllergyStatusOptions { get; set; } = new();
    }

    public class PatientAllergyDefaultFilterResponse
    {
        public string? Search { get; set; }

        public Guid? PatientId { get; set; }

        public Guid? EncounterId { get; set; }

        public Guid? ConsultationId { get; set; }

        public Guid? AssessmentId { get; set; }

        public Guid? DoctorId { get; set; }

        public Guid? ServiceUnitId { get; set; }

        public Guid? ClinicId { get; set; }

        public Guid? DrugId { get; set; }

        public PatientAllergyCategory? AllergyCategory { get; set; }

        public PatientAllergySeverity? Severity { get; set; }

        public PatientAllergyCertainty? Certainty { get; set; }

        public PatientAllergyStatus? AllergyStatus { get; set; }

        public bool? IsHighRisk { get; set; }

        public bool? IsLifeThreatening { get; set; }

        public bool? IsAlertEnabled { get; set; }

        public bool? IsVerified { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string SortBy { get; set; } = "reportedDateTime";

        public string SortDirection { get; set; } = "desc";

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    public class PatientAllergySortOptionResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class PatientAllergyEnumOptionResponse
    {
        public int Value { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class CreatePatientAllergyRequest
    {
        [Required]
        public Guid PatientId { get; set; }

        public Guid? EncounterId { get; set; }

        public Guid? ConsultationId { get; set; }

        public Guid? AssessmentId { get; set; }

        public Guid? DoctorId { get; set; }

        public Guid? ServiceUnitId { get; set; }

        public Guid? ClinicId { get; set; }

        public Guid? DrugId { get; set; }

        public PatientAllergyCategory AllergyCategory { get; set; } = PatientAllergyCategory.Unknown;

        [MaxLength(100)]
        public string? AllergenCode { get; set; }

        [Required]
        [MaxLength(250)]
        public string AllergenName { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? AllergenGroupName { get; set; }

        [MaxLength(100)]
        public string? ReactionType { get; set; }

        [MaxLength(1000)]
        public string? ReactionDescription { get; set; }

        public PatientAllergySeverity Severity { get; set; } = PatientAllergySeverity.Unknown;

        public PatientAllergyCertainty Certainty { get; set; } = PatientAllergyCertainty.Unknown;

        public DateTime? FirstReactionDate { get; set; }

        public DateTime? LastReactionDate { get; set; }

        public DateTime? ReportedDateTime { get; set; }

        [MaxLength(100)]
        public string? SourceOfInformation { get; set; }

        public bool IsHighRisk { get; set; } = false;

        public bool IsLifeThreatening { get; set; } = false;

        public bool IsAlertEnabled { get; set; } = true;

        [MaxLength(1000)]
        public string? PatientSafetyNote { get; set; }

        public bool IsVerified { get; set; } = false;

        [MaxLength(1000)]
        public string? ClinicalNote { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class UpdatePatientAllergyRequest
    {
        public Guid? DrugId { get; set; }

        public PatientAllergyCategory AllergyCategory { get; set; } = PatientAllergyCategory.Unknown;

        [MaxLength(100)]
        public string? AllergenCode { get; set; }

        [Required]
        [MaxLength(250)]
        public string AllergenName { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? AllergenGroupName { get; set; }

        [MaxLength(100)]
        public string? ReactionType { get; set; }

        [MaxLength(1000)]
        public string? ReactionDescription { get; set; }

        public PatientAllergySeverity Severity { get; set; } = PatientAllergySeverity.Unknown;

        public PatientAllergyCertainty Certainty { get; set; } = PatientAllergyCertainty.Unknown;

        public PatientAllergyStatus AllergyStatus { get; set; } = PatientAllergyStatus.Active;

        public DateTime? FirstReactionDate { get; set; }

        public DateTime? LastReactionDate { get; set; }

        [MaxLength(100)]
        public string? SourceOfInformation { get; set; }

        public bool IsHighRisk { get; set; } = false;

        public bool IsLifeThreatening { get; set; } = false;

        public bool IsAlertEnabled { get; set; } = true;

        [MaxLength(1000)]
        public string? PatientSafetyNote { get; set; }

        [MaxLength(1000)]
        public string? ClinicalNote { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class PatientAllergyCreateResponse
    {
        public Guid Id { get; set; }

        public string AllergyRecordNumber { get; set; } = string.Empty;

        public Guid PatientId { get; set; }

        public Guid? EncounterId { get; set; }

        public Guid? ConsultationId { get; set; }

        public Guid? AssessmentId { get; set; }

        public Guid? DrugId { get; set; }

        public PatientAllergyCategory AllergyCategory { get; set; }

        public string AllergenName { get; set; } = string.Empty;

        public string? AllergenGroupName { get; set; }

        public string? ReactionType { get; set; }

        public PatientAllergySeverity Severity { get; set; }

        public PatientAllergyCertainty Certainty { get; set; }

        public PatientAllergyStatus AllergyStatus { get; set; }

        public bool IsHighRisk { get; set; }

        public bool IsLifeThreatening { get; set; }

        public bool IsAlertEnabled { get; set; }

        public bool IsVerified { get; set; }
    }

    public class PatientAllergyUpdateResponse : PatientAllergyCreateResponse
    {
    }

    public class VerifyPatientAllergyRequest
    {
        [MaxLength(1000)]
        public string? ClinicalNote { get; set; }
    }

    public class ResolvePatientAllergyRequest
    {
        [Required]
        [MaxLength(250)]
        public string ResolvedReason { get; set; } = string.Empty;
    }

    public class CancelPatientAllergyRequest
    {
        [Required]
        [MaxLength(250)]
        public string CancelReason { get; set; } = string.Empty;
    }
}