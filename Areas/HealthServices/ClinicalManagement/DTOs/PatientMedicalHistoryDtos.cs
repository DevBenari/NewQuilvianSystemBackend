using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs
{
    public class PatientMedicalHistoryResponse
    {
        public Guid Id { get; set; }

        public string MedicalHistoryRecordNumber { get; set; } = string.Empty;

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

        public Guid? DiagnosisId { get; set; }

        public PatientMedicalHistoryType HistoryType { get; set; }

        public PatientMedicalHistoryStatus HistoryStatus { get; set; }

        public PatientMedicalHistorySeverity Severity { get; set; }

        public PatientMedicalHistoryCertainty Certainty { get; set; }

        public string? ConditionCode { get; set; }

        public string ConditionName { get; set; } = string.Empty;

        public string? ConditionGroupName { get; set; }

        public string ConditionMasterType { get; set; } = string.Empty;

        public string? IcdVersion { get; set; }

        public bool IsFromMasterDiagnosis { get; set; }

        public bool IsCurrentProblem { get; set; }

        public bool IsChronic { get; set; }

        public bool IsComorbidity { get; set; }

        public bool IsUnderTreatment { get; set; }

        public bool IsControlled { get; set; }

        public bool IsInfectiousDisease { get; set; }

        public bool IsHereditaryRelated { get; set; }

        public bool IsMentalHealthRelated { get; set; }

        public bool IsPregnancyRelated { get; set; }

        public bool IsSurgicalHistory { get; set; }

        public bool IsHospitalizationHistory { get; set; }

        public bool IsHighRisk { get; set; }

        public bool IsAlertEnabled { get; set; }

        public DateTime RecordedDateTime { get; set; }

        public DateTime? OnsetDate { get; set; }

        public int? OnsetAgeYear { get; set; }

        public DateTime? DiagnosedDate { get; set; }

        public DateTime? LastTreatmentDate { get; set; }

        public DateTime? LastControlDate { get; set; }

        public string? SourceOfInformation { get; set; }

        public bool IsVerified { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public Guid? VerifiedByUserId { get; set; }

        public string? VerifiedByUserName { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class PatientMedicalHistoryDetailResponse : PatientMedicalHistoryResponse
    {
        public string? TreatmentHistory { get; set; }

        public string? MedicationHistory { get; set; }

        public string? SurgeryHistory { get; set; }

        public string? HospitalizationHistory { get; set; }

        public string? ComplicationNote { get; set; }

        public string? ClinicalNote { get; set; }

        public string? RiskNote { get; set; }

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

    public class PatientMedicalHistoryOptionResponse
    {
        public Guid Id { get; set; }

        public Guid PatientId { get; set; }

        public string MedicalHistoryRecordNumber { get; set; } = string.Empty;

        public Guid? DiagnosisId { get; set; }

        public PatientMedicalHistoryType HistoryType { get; set; }

        public PatientMedicalHistoryStatus HistoryStatus { get; set; }

        public PatientMedicalHistorySeverity Severity { get; set; }

        public PatientMedicalHistoryCertainty Certainty { get; set; }

        public string? ConditionCode { get; set; }

        public string ConditionName { get; set; } = string.Empty;

        public string? ConditionGroupName { get; set; }

        public bool IsCurrentProblem { get; set; }

        public bool IsChronic { get; set; }

        public bool IsComorbidity { get; set; }

        public bool IsHighRisk { get; set; }

        public bool IsAlertEnabled { get; set; }

        public bool IsVerified { get; set; }
    }

    public class PatientMedicalHistoryAlertResponse
    {
        public Guid Id { get; set; }

        public Guid PatientId { get; set; }

        public string MedicalHistoryRecordNumber { get; set; } = string.Empty;

        public Guid? DiagnosisId { get; set; }

        public PatientMedicalHistoryType HistoryType { get; set; }

        public PatientMedicalHistoryStatus HistoryStatus { get; set; }

        public PatientMedicalHistorySeverity Severity { get; set; }

        public string? ConditionCode { get; set; }

        public string ConditionName { get; set; } = string.Empty;

        public string? ConditionGroupName { get; set; }

        public bool IsCurrentProblem { get; set; }

        public bool IsChronic { get; set; }

        public bool IsComorbidity { get; set; }

        public bool IsInfectiousDisease { get; set; }

        public bool IsMentalHealthRelated { get; set; }

        public bool IsPregnancyRelated { get; set; }

        public bool IsHighRisk { get; set; }

        public string? RiskNote { get; set; }

        public string? ClinicalNote { get; set; }
    }

    public class PatientMedicalHistoryFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public PatientMedicalHistoryDefaultFilterResponse DefaultFilter { get; set; } = new();

        public List<PatientMedicalHistorySortOptionResponse> SortOptions { get; set; } = new();

        public List<string> SortDirections { get; set; } = new();

        public List<int> PageSizeOptions { get; set; } = new();

        public List<PatientMedicalHistoryEnumOptionResponse> HistoryTypeOptions { get; set; } = new();

        public List<PatientMedicalHistoryEnumOptionResponse> HistoryStatusOptions { get; set; } = new();

        public List<PatientMedicalHistoryEnumOptionResponse> SeverityOptions { get; set; } = new();

        public List<PatientMedicalHistoryEnumOptionResponse> CertaintyOptions { get; set; } = new();
    }

    public class PatientMedicalHistoryDefaultFilterResponse
    {
        public string? Search { get; set; }

        public Guid? PatientId { get; set; }

        public Guid? EncounterId { get; set; }

        public Guid? ConsultationId { get; set; }

        public Guid? AssessmentId { get; set; }

        public Guid? DoctorId { get; set; }

        public Guid? ServiceUnitId { get; set; }

        public Guid? ClinicId { get; set; }

        public Guid? DiagnosisId { get; set; }

        public PatientMedicalHistoryType? HistoryType { get; set; }

        public PatientMedicalHistoryStatus? HistoryStatus { get; set; }

        public PatientMedicalHistorySeverity? Severity { get; set; }

        public PatientMedicalHistoryCertainty? Certainty { get; set; }

        public bool? IsFromMasterDiagnosis { get; set; }

        public bool? IsCurrentProblem { get; set; }

        public bool? IsChronic { get; set; }

        public bool? IsComorbidity { get; set; }

        public bool? IsUnderTreatment { get; set; }

        public bool? IsControlled { get; set; }

        public bool? IsInfectiousDisease { get; set; }

        public bool? IsHereditaryRelated { get; set; }

        public bool? IsMentalHealthRelated { get; set; }

        public bool? IsPregnancyRelated { get; set; }

        public bool? IsSurgicalHistory { get; set; }

        public bool? IsHospitalizationHistory { get; set; }

        public bool? IsHighRisk { get; set; }

        public bool? IsAlertEnabled { get; set; }

        public bool? IsVerified { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string SortBy { get; set; } = "recordedDateTime";

        public string SortDirection { get; set; } = "desc";

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    public class PatientMedicalHistorySortOptionResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class PatientMedicalHistoryEnumOptionResponse
    {
        public int Value { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class CreatePatientMedicalHistoryRequest
    {
        [Required]
        public Guid PatientId { get; set; }

        public Guid? EncounterId { get; set; }

        public Guid? ConsultationId { get; set; }

        public Guid? AssessmentId { get; set; }

        public Guid? DoctorId { get; set; }

        public Guid? ServiceUnitId { get; set; }

        public Guid? ClinicId { get; set; }

        public Guid? DiagnosisId { get; set; }

        public PatientMedicalHistoryType HistoryType { get; set; } = PatientMedicalHistoryType.Disease;

        public PatientMedicalHistoryStatus HistoryStatus { get; set; } = PatientMedicalHistoryStatus.Active;

        public PatientMedicalHistorySeverity Severity { get; set; } = PatientMedicalHistorySeverity.Unknown;

        public PatientMedicalHistoryCertainty Certainty { get; set; } = PatientMedicalHistoryCertainty.Unknown;

        [MaxLength(50)]
        public string? ConditionCode { get; set; }

        [MaxLength(500)]
        public string? ConditionName { get; set; }

        [MaxLength(250)]
        public string? ConditionGroupName { get; set; }

        [MaxLength(50)]
        public string? ConditionMasterType { get; set; }

        [MaxLength(100)]
        public string? IcdVersion { get; set; }

        public bool IsCurrentProblem { get; set; } = false;

        public bool IsChronic { get; set; } = false;

        public bool IsComorbidity { get; set; } = false;

        public bool IsUnderTreatment { get; set; } = false;

        public bool IsControlled { get; set; } = false;

        public bool IsInfectiousDisease { get; set; } = false;

        public bool IsHereditaryRelated { get; set; } = false;

        public bool IsMentalHealthRelated { get; set; } = false;

        public bool IsPregnancyRelated { get; set; } = false;

        public bool IsSurgicalHistory { get; set; } = false;

        public bool IsHospitalizationHistory { get; set; } = false;

        public bool IsHighRisk { get; set; } = false;

        public bool IsAlertEnabled { get; set; } = false;

        public DateTime? RecordedDateTime { get; set; }

        public DateTime? OnsetDate { get; set; }

        public int? OnsetAgeYear { get; set; }

        public DateTime? DiagnosedDate { get; set; }

        public DateTime? LastTreatmentDate { get; set; }

        public DateTime? LastControlDate { get; set; }

        [MaxLength(100)]
        public string? SourceOfInformation { get; set; }

        [MaxLength(1000)]
        public string? TreatmentHistory { get; set; }

        [MaxLength(1000)]
        public string? MedicationHistory { get; set; }

        [MaxLength(1000)]
        public string? SurgeryHistory { get; set; }

        [MaxLength(1000)]
        public string? HospitalizationHistory { get; set; }

        [MaxLength(1000)]
        public string? ComplicationNote { get; set; }

        [MaxLength(1000)]
        public string? ClinicalNote { get; set; }

        [MaxLength(1000)]
        public string? RiskNote { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsVerified { get; set; } = false;
    }

    public class UpdatePatientMedicalHistoryRequest
    {
        public Guid? DiagnosisId { get; set; }

        public PatientMedicalHistoryType HistoryType { get; set; } = PatientMedicalHistoryType.Disease;

        public PatientMedicalHistoryStatus HistoryStatus { get; set; } = PatientMedicalHistoryStatus.Active;

        public PatientMedicalHistorySeverity Severity { get; set; } = PatientMedicalHistorySeverity.Unknown;

        public PatientMedicalHistoryCertainty Certainty { get; set; } = PatientMedicalHistoryCertainty.Unknown;

        [MaxLength(50)]
        public string? ConditionCode { get; set; }

        [MaxLength(500)]
        public string? ConditionName { get; set; }

        [MaxLength(250)]
        public string? ConditionGroupName { get; set; }

        [MaxLength(50)]
        public string? ConditionMasterType { get; set; }

        [MaxLength(100)]
        public string? IcdVersion { get; set; }

        public bool IsCurrentProblem { get; set; } = false;

        public bool IsChronic { get; set; } = false;

        public bool IsComorbidity { get; set; } = false;

        public bool IsUnderTreatment { get; set; } = false;

        public bool IsControlled { get; set; } = false;

        public bool IsInfectiousDisease { get; set; } = false;

        public bool IsHereditaryRelated { get; set; } = false;

        public bool IsMentalHealthRelated { get; set; } = false;

        public bool IsPregnancyRelated { get; set; } = false;

        public bool IsSurgicalHistory { get; set; } = false;

        public bool IsHospitalizationHistory { get; set; } = false;

        public bool IsHighRisk { get; set; } = false;

        public bool IsAlertEnabled { get; set; } = false;

        public DateTime? OnsetDate { get; set; }

        public int? OnsetAgeYear { get; set; }

        public DateTime? DiagnosedDate { get; set; }

        public DateTime? LastTreatmentDate { get; set; }

        public DateTime? LastControlDate { get; set; }

        [MaxLength(100)]
        public string? SourceOfInformation { get; set; }

        [MaxLength(1000)]
        public string? TreatmentHistory { get; set; }

        [MaxLength(1000)]
        public string? MedicationHistory { get; set; }

        [MaxLength(1000)]
        public string? SurgeryHistory { get; set; }

        [MaxLength(1000)]
        public string? HospitalizationHistory { get; set; }

        [MaxLength(1000)]
        public string? ComplicationNote { get; set; }

        [MaxLength(1000)]
        public string? ClinicalNote { get; set; }

        [MaxLength(1000)]
        public string? RiskNote { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class PatientMedicalHistoryCreateResponse
    {
        public Guid Id { get; set; }

        public string MedicalHistoryRecordNumber { get; set; } = string.Empty;

        public Guid PatientId { get; set; }

        public Guid? EncounterId { get; set; }

        public Guid? ConsultationId { get; set; }

        public Guid? AssessmentId { get; set; }

        public Guid? DiagnosisId { get; set; }

        public PatientMedicalHistoryType HistoryType { get; set; }

        public PatientMedicalHistoryStatus HistoryStatus { get; set; }

        public PatientMedicalHistorySeverity Severity { get; set; }

        public PatientMedicalHistoryCertainty Certainty { get; set; }

        public string? ConditionCode { get; set; }

        public string ConditionName { get; set; } = string.Empty;

        public string? ConditionGroupName { get; set; }

        public string ConditionMasterType { get; set; } = string.Empty;

        public bool IsFromMasterDiagnosis { get; set; }

        public bool IsCurrentProblem { get; set; }

        public bool IsChronic { get; set; }

        public bool IsComorbidity { get; set; }

        public bool IsHighRisk { get; set; }

        public bool IsAlertEnabled { get; set; }

        public bool IsVerified { get; set; }
    }

    public class PatientMedicalHistoryUpdateResponse : PatientMedicalHistoryCreateResponse
    {
    }

    public class VerifyPatientMedicalHistoryRequest
    {
        [MaxLength(1000)]
        public string? ClinicalNote { get; set; }
    }

    public class ResolvePatientMedicalHistoryRequest
    {
        [Required]
        [MaxLength(250)]
        public string ResolvedReason { get; set; } = string.Empty;
    }

    public class CancelPatientMedicalHistoryRequest
    {
        [Required]
        [MaxLength(250)]
        public string CancelReason { get; set; } = string.Empty;
    }
}
