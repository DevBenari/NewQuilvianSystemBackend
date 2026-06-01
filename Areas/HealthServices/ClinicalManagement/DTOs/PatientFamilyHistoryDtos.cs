using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs
{
    public class PatientFamilyHistoryResponse
    {
        public Guid Id { get; set; }

        public string FamilyHistoryRecordNumber { get; set; } = string.Empty;

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

        public Guid? FamilyMemberPatientId { get; set; }
        public string? FamilyMemberPatientName { get; set; }
        public string? FamilyMemberMedicalRecordNumber { get; set; }

        public Guid? DiagnosisId { get; set; }

        public PatientFamilyRelationshipType RelationshipType { get; set; }

        public PatientFamilyRelationshipSide RelationshipSide { get; set; }

        public string? FamilyMemberNameSnapshot { get; set; }

        public string? RelationshipDescription { get; set; }

        public bool IsFirstDegreeRelative { get; set; }

        public bool IsSecondDegreeRelative { get; set; }

        public bool IsSameHousehold { get; set; }

        public string? ConditionCode { get; set; }

        public string ConditionName { get; set; } = string.Empty;

        public string? ConditionGroupName { get; set; }

        public string ConditionMasterType { get; set; } = string.Empty;

        public string? IcdVersion { get; set; }

        public bool IsFromMasterDiagnosis { get; set; }

        public PatientFamilyHistoryStatus FamilyHistoryStatus { get; set; }

        public PatientFamilyHistoryCertainty Certainty { get; set; }

        public PatientFamilyRiskLevel RiskLevel { get; set; }

        public bool IsHereditaryDisease { get; set; }

        public bool IsGeneticRisk { get; set; }

        public bool IsChronicDisease { get; set; }

        public bool IsCancerRelated { get; set; }

        public bool IsCardiovascularRisk { get; set; }

        public bool IsMetabolicRisk { get; set; }

        public bool IsMentalHealthRelated { get; set; }

        public bool IsInfectiousDisease { get; set; }

        public bool IsHighRisk { get; set; }

        public bool IsScreeningRecommended { get; set; }

        public bool IsAlertEnabled { get; set; }

        public DateTime RecordedDateTime { get; set; }

        public DateTime? DiagnosedDate { get; set; }

        public int? AgeAtDiagnosisYear { get; set; }

        public bool IsFamilyMemberDeceased { get; set; }

        public DateTime? DeathDate { get; set; }

        public int? AgeAtDeathYear { get; set; }

        public string? CauseOfDeath { get; set; }

        public string? SourceOfInformation { get; set; }

        public bool IsVerified { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public Guid? VerifiedByUserId { get; set; }

        public string? VerifiedByUserName { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class PatientFamilyHistoryDetailResponse : PatientFamilyHistoryResponse
    {
        public string? ClinicalNote { get; set; }

        public string? RiskNote { get; set; }

        public string? ScreeningRecommendation { get; set; }

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

    public class PatientFamilyHistoryOptionResponse
    {
        public Guid Id { get; set; }

        public Guid PatientId { get; set; }

        public string FamilyHistoryRecordNumber { get; set; } = string.Empty;

        public PatientFamilyRelationshipType RelationshipType { get; set; }

        public PatientFamilyRelationshipSide RelationshipSide { get; set; }

        public string? RelationshipDescription { get; set; }

        public Guid? DiagnosisId { get; set; }

        public string? ConditionCode { get; set; }

        public string ConditionName { get; set; } = string.Empty;

        public string? ConditionGroupName { get; set; }

        public PatientFamilyHistoryStatus FamilyHistoryStatus { get; set; }

        public PatientFamilyHistoryCertainty Certainty { get; set; }

        public PatientFamilyRiskLevel RiskLevel { get; set; }

        public bool IsHereditaryDisease { get; set; }

        public bool IsGeneticRisk { get; set; }

        public bool IsHighRisk { get; set; }

        public bool IsScreeningRecommended { get; set; }

        public bool IsAlertEnabled { get; set; }

        public bool IsVerified { get; set; }
    }

    public class PatientFamilyHistoryAlertResponse
    {
        public Guid Id { get; set; }

        public Guid PatientId { get; set; }

        public string FamilyHistoryRecordNumber { get; set; } = string.Empty;

        public PatientFamilyRelationshipType RelationshipType { get; set; }

        public PatientFamilyRelationshipSide RelationshipSide { get; set; }

        public string? RelationshipDescription { get; set; }

        public Guid? DiagnosisId { get; set; }

        public string? ConditionCode { get; set; }

        public string ConditionName { get; set; } = string.Empty;

        public string? ConditionGroupName { get; set; }

        public PatientFamilyRiskLevel RiskLevel { get; set; }

        public bool IsFirstDegreeRelative { get; set; }

        public bool IsHereditaryDisease { get; set; }

        public bool IsGeneticRisk { get; set; }

        public bool IsCancerRelated { get; set; }

        public bool IsCardiovascularRisk { get; set; }

        public bool IsMetabolicRisk { get; set; }

        public bool IsHighRisk { get; set; }

        public bool IsScreeningRecommended { get; set; }

        public string? RiskNote { get; set; }

        public string? ScreeningRecommendation { get; set; }
    }

    public class PatientFamilyHistoryFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public PatientFamilyHistoryDefaultFilterResponse DefaultFilter { get; set; } = new();

        public List<PatientFamilyHistorySortOptionResponse> SortOptions { get; set; } = new();

        public List<string> SortDirections { get; set; } = new();

        public List<int> PageSizeOptions { get; set; } = new();

        public List<PatientFamilyHistoryEnumOptionResponse> RelationshipTypeOptions { get; set; } = new();

        public List<PatientFamilyHistoryEnumOptionResponse> RelationshipSideOptions { get; set; } = new();

        public List<PatientFamilyHistoryEnumOptionResponse> FamilyHistoryStatusOptions { get; set; } = new();

        public List<PatientFamilyHistoryEnumOptionResponse> CertaintyOptions { get; set; } = new();

        public List<PatientFamilyHistoryEnumOptionResponse> RiskLevelOptions { get; set; } = new();
    }

    public class PatientFamilyHistoryDefaultFilterResponse
    {
        public string? Search { get; set; }

        public Guid? PatientId { get; set; }

        public Guid? EncounterId { get; set; }

        public Guid? ConsultationId { get; set; }

        public Guid? AssessmentId { get; set; }

        public Guid? DoctorId { get; set; }

        public Guid? ServiceUnitId { get; set; }

        public Guid? ClinicId { get; set; }

        public Guid? FamilyMemberPatientId { get; set; }

        public Guid? DiagnosisId { get; set; }

        public PatientFamilyRelationshipType? RelationshipType { get; set; }

        public PatientFamilyRelationshipSide? RelationshipSide { get; set; }

        public PatientFamilyHistoryStatus? FamilyHistoryStatus { get; set; }

        public PatientFamilyHistoryCertainty? Certainty { get; set; }

        public PatientFamilyRiskLevel? RiskLevel { get; set; }

        public bool? IsFromMasterDiagnosis { get; set; }

        public bool? IsFirstDegreeRelative { get; set; }

        public bool? IsSecondDegreeRelative { get; set; }

        public bool? IsSameHousehold { get; set; }

        public bool? IsHereditaryDisease { get; set; }

        public bool? IsGeneticRisk { get; set; }

        public bool? IsChronicDisease { get; set; }

        public bool? IsCancerRelated { get; set; }

        public bool? IsCardiovascularRisk { get; set; }

        public bool? IsMetabolicRisk { get; set; }

        public bool? IsMentalHealthRelated { get; set; }

        public bool? IsInfectiousDisease { get; set; }

        public bool? IsHighRisk { get; set; }

        public bool? IsScreeningRecommended { get; set; }

        public bool? IsAlertEnabled { get; set; }

        public bool? IsVerified { get; set; }

        public bool? IsFamilyMemberDeceased { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string SortBy { get; set; } = "recordedDateTime";

        public string SortDirection { get; set; } = "desc";

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    public class PatientFamilyHistorySortOptionResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class PatientFamilyHistoryEnumOptionResponse
    {
        public int Value { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class CreatePatientFamilyHistoryRequest
    {
        [Required]
        public Guid PatientId { get; set; }

        public Guid? EncounterId { get; set; }

        public Guid? ConsultationId { get; set; }

        public Guid? AssessmentId { get; set; }

        public Guid? DoctorId { get; set; }

        public Guid? ServiceUnitId { get; set; }

        public Guid? ClinicId { get; set; }

        public Guid? FamilyMemberPatientId { get; set; }

        public Guid? DiagnosisId { get; set; }

        public PatientFamilyRelationshipType RelationshipType { get; set; } = PatientFamilyRelationshipType.Unknown;

        public PatientFamilyRelationshipSide RelationshipSide { get; set; } = PatientFamilyRelationshipSide.Unknown;

        [MaxLength(200)]
        public string? FamilyMemberNameSnapshot { get; set; }

        [MaxLength(100)]
        public string? RelationshipDescription { get; set; }

        public bool IsFirstDegreeRelative { get; set; } = false;

        public bool IsSecondDegreeRelative { get; set; } = false;

        public bool IsSameHousehold { get; set; } = false;

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

        public PatientFamilyHistoryStatus FamilyHistoryStatus { get; set; } = PatientFamilyHistoryStatus.Active;

        public PatientFamilyHistoryCertainty Certainty { get; set; } = PatientFamilyHistoryCertainty.Unknown;

        public PatientFamilyRiskLevel RiskLevel { get; set; } = PatientFamilyRiskLevel.Unknown;

        public bool IsHereditaryDisease { get; set; } = false;

        public bool IsGeneticRisk { get; set; } = false;

        public bool IsChronicDisease { get; set; } = false;

        public bool IsCancerRelated { get; set; } = false;

        public bool IsCardiovascularRisk { get; set; } = false;

        public bool IsMetabolicRisk { get; set; } = false;

        public bool IsMentalHealthRelated { get; set; } = false;

        public bool IsInfectiousDisease { get; set; } = false;

        public bool IsHighRisk { get; set; } = false;

        public bool IsScreeningRecommended { get; set; } = false;

        public bool IsAlertEnabled { get; set; } = false;

        public DateTime? RecordedDateTime { get; set; }

        public DateTime? DiagnosedDate { get; set; }

        public int? AgeAtDiagnosisYear { get; set; }

        public bool IsFamilyMemberDeceased { get; set; } = false;

        public DateTime? DeathDate { get; set; }

        public int? AgeAtDeathYear { get; set; }

        [MaxLength(250)]
        public string? CauseOfDeath { get; set; }

        [MaxLength(100)]
        public string? SourceOfInformation { get; set; }

        [MaxLength(1000)]
        public string? ClinicalNote { get; set; }

        [MaxLength(1000)]
        public string? RiskNote { get; set; }

        [MaxLength(1000)]
        public string? ScreeningRecommendation { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsVerified { get; set; } = false;
    }

    public class UpdatePatientFamilyHistoryRequest
    {
        public Guid? FamilyMemberPatientId { get; set; }

        public Guid? DiagnosisId { get; set; }

        public PatientFamilyRelationshipType RelationshipType { get; set; } = PatientFamilyRelationshipType.Unknown;

        public PatientFamilyRelationshipSide RelationshipSide { get; set; } = PatientFamilyRelationshipSide.Unknown;

        [MaxLength(200)]
        public string? FamilyMemberNameSnapshot { get; set; }

        [MaxLength(100)]
        public string? RelationshipDescription { get; set; }

        public bool IsFirstDegreeRelative { get; set; } = false;

        public bool IsSecondDegreeRelative { get; set; } = false;

        public bool IsSameHousehold { get; set; } = false;

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

        public PatientFamilyHistoryStatus FamilyHistoryStatus { get; set; } = PatientFamilyHistoryStatus.Active;

        public PatientFamilyHistoryCertainty Certainty { get; set; } = PatientFamilyHistoryCertainty.Unknown;

        public PatientFamilyRiskLevel RiskLevel { get; set; } = PatientFamilyRiskLevel.Unknown;

        public bool IsHereditaryDisease { get; set; } = false;

        public bool IsGeneticRisk { get; set; } = false;

        public bool IsChronicDisease { get; set; } = false;

        public bool IsCancerRelated { get; set; } = false;

        public bool IsCardiovascularRisk { get; set; } = false;

        public bool IsMetabolicRisk { get; set; } = false;

        public bool IsMentalHealthRelated { get; set; } = false;

        public bool IsInfectiousDisease { get; set; } = false;

        public bool IsHighRisk { get; set; } = false;

        public bool IsScreeningRecommended { get; set; } = false;

        public bool IsAlertEnabled { get; set; } = false;

        public DateTime? DiagnosedDate { get; set; }

        public int? AgeAtDiagnosisYear { get; set; }

        public bool IsFamilyMemberDeceased { get; set; } = false;

        public DateTime? DeathDate { get; set; }

        public int? AgeAtDeathYear { get; set; }

        [MaxLength(250)]
        public string? CauseOfDeath { get; set; }

        [MaxLength(100)]
        public string? SourceOfInformation { get; set; }

        [MaxLength(1000)]
        public string? ClinicalNote { get; set; }

        [MaxLength(1000)]
        public string? RiskNote { get; set; }

        [MaxLength(1000)]
        public string? ScreeningRecommendation { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class PatientFamilyHistoryCreateResponse
    {
        public Guid Id { get; set; }

        public string FamilyHistoryRecordNumber { get; set; } = string.Empty;

        public Guid PatientId { get; set; }

        public Guid? EncounterId { get; set; }

        public Guid? ConsultationId { get; set; }

        public Guid? AssessmentId { get; set; }

        public Guid? FamilyMemberPatientId { get; set; }

        public Guid? DiagnosisId { get; set; }

        public PatientFamilyRelationshipType RelationshipType { get; set; }

        public PatientFamilyRelationshipSide RelationshipSide { get; set; }

        public string? RelationshipDescription { get; set; }

        public string? ConditionCode { get; set; }

        public string ConditionName { get; set; } = string.Empty;

        public string? ConditionGroupName { get; set; }

        public string ConditionMasterType { get; set; } = string.Empty;

        public bool IsFromMasterDiagnosis { get; set; }

        public PatientFamilyHistoryStatus FamilyHistoryStatus { get; set; }

        public PatientFamilyHistoryCertainty Certainty { get; set; }

        public PatientFamilyRiskLevel RiskLevel { get; set; }

        public bool IsHereditaryDisease { get; set; }

        public bool IsGeneticRisk { get; set; }

        public bool IsHighRisk { get; set; }

        public bool IsScreeningRecommended { get; set; }

        public bool IsAlertEnabled { get; set; }

        public bool IsVerified { get; set; }
    }

    public class PatientFamilyHistoryUpdateResponse : PatientFamilyHistoryCreateResponse
    {
    }

    public class VerifyPatientFamilyHistoryRequest
    {
        [MaxLength(1000)]
        public string? ClinicalNote { get; set; }
    }

    public class ResolvePatientFamilyHistoryRequest
    {
        [Required]
        [MaxLength(250)]
        public string ResolvedReason { get; set; } = string.Empty;
    }

    public class CancelPatientFamilyHistoryRequest
    {
        [Required]
        [MaxLength(250)]
        public string CancelReason { get; set; } = string.Empty;
    }
}
