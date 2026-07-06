using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs
{
    public class PatientDiagnosisResponse
    {
        public Guid Id { get; set; }

        public Guid EncounterId { get; set; }
        public string EncounterNumber { get; set; } = string.Empty;

        public Guid ConsultationId { get; set; }
        public string ConsultationNumber { get; set; } = string.Empty;

        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string MedicalRecordNumber { get; set; } = string.Empty;

        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;

        public Guid? ServiceUnitId { get; set; }
        public string? ServiceUnitName { get; set; }

        public Guid? ClinicId { get; set; }
        public string? ClinicName { get; set; }

        public Guid? DiagnosisId { get; set; }
        public string DiagnosisCode { get; set; } = string.Empty;
        public string DiagnosisName { get; set; } = string.Empty;
        public string DiagnosisMasterType { get; set; } = string.Empty;
        public string? IcdVersion { get; set; }

        public PatientDiagnosisType DiagnosisType { get; set; }
        public PatientDiagnosisStatus DiagnosisStatus { get; set; }

        public bool IsPrimary { get; set; }
        public bool IsChronic { get; set; }
        public bool IsNewCase { get; set; }
        public bool IsConfirmed { get; set; }
        public bool IsFromMasterDiagnosis { get; set; }

        public DateTime DiagnosisDateTime { get; set; }
        public DateTime? OnsetDate { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class PatientDiagnosisDetailResponse : PatientDiagnosisResponse
    {
        public string? ClinicalNote { get; set; }
        public string? AssessmentNote { get; set; }
        public string? PlanNote { get; set; }
        public string? DifferentialDiagnosisNote { get; set; }
        public string? SupportingFindingNote { get; set; }

        public DateTime? ResolvedAt { get; set; }
        public Guid? ResolvedByUserId { get; set; }
        public string? ResolvedByUserName { get; set; }
        public string? ResolvedReason { get; set; }

        public DateTime? CancelledAt { get; set; }
        public Guid? CancelledByUserId { get; set; }
        public string? CancelledByUserName { get; set; }
        public string? CancelReason { get; set; }
    }

    public class PatientDiagnosisOptionResponse
    {
        public Guid Id { get; set; }
        public Guid? DiagnosisId { get; set; }
        public string DiagnosisCode { get; set; } = string.Empty;
        public string DiagnosisName { get; set; } = string.Empty;
        public string DiagnosisMasterType { get; set; } = string.Empty;
        public string? IcdVersion { get; set; }
        public PatientDiagnosisType DiagnosisType { get; set; }
        public PatientDiagnosisStatus DiagnosisStatus { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsConfirmed { get; set; }
        public bool IsFromMasterDiagnosis { get; set; }
    }

    public class PatientDiagnosisMasterOptionResponse
    {
        public Guid Id { get; set; }
        public string DiagnosisCode { get; set; } = string.Empty;
        public string DiagnosisName { get; set; } = string.Empty;
        public string DiagnosisType { get; set; } = string.Empty;
        public string IcdVersion { get; set; } = string.Empty;
        public bool IsSelectableForClinicalUse { get; set; }
        public bool IsPrimaryDiagnosisAllowed { get; set; }
        public bool IsSecondaryDiagnosisAllowed { get; set; }
    }

    public class PatientDiagnosisFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public PatientDiagnosisDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<PatientDiagnosisSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<PatientDiagnosisEnumOptionResponse> DiagnosisTypeOptions { get; set; } = new();
        public List<PatientDiagnosisEnumOptionResponse> DiagnosisStatusOptions { get; set; } = new();
    }

    public class PatientDiagnosisDefaultFilterResponse
    {
        public string? Search { get; set; }
        public Guid? EncounterId { get; set; }
        public Guid? ConsultationId { get; set; }
        public Guid? PatientId { get; set; }
        public Guid? DoctorId { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public Guid? ClinicId { get; set; }
        public Guid? DiagnosisId { get; set; }
        public PatientDiagnosisType? DiagnosisType { get; set; }
        public PatientDiagnosisStatus? DiagnosisStatus { get; set; }
        public bool? IsPrimary { get; set; }
        public bool? IsConfirmed { get; set; }
        public bool? IsFromMasterDiagnosis { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class PatientDiagnosisSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class PatientDiagnosisEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreatePatientDiagnosisRequest
    {
        [Required]
        public Guid EncounterId { get; set; }

        [Required]
        public Guid ConsultationId { get; set; }

        public Guid? DiagnosisId { get; set; }

        [MaxLength(50)]
        public string? DiagnosisCode { get; set; }

        [MaxLength(500)]
        public string? DiagnosisName { get; set; }

        [MaxLength(50)]
        public string? DiagnosisMasterType { get; set; }

        [MaxLength(100)]
        public string? IcdVersion { get; set; }

        public PatientDiagnosisType DiagnosisType { get; set; } = PatientDiagnosisType.Secondary;

        public bool IsPrimary { get; set; } = false;

        public bool IsChronic { get; set; } = false;

        public bool IsNewCase { get; set; } = true;

        public bool IsConfirmed { get; set; } = true;

        public DateTime? OnsetDate { get; set; }

        [MaxLength(1000)]
        public string? ClinicalNote { get; set; }

        [MaxLength(1000)]
        public string? AssessmentNote { get; set; }

        [MaxLength(1000)]
        public string? PlanNote { get; set; }

        [MaxLength(500)]
        public string? DifferentialDiagnosisNote { get; set; }

        [MaxLength(500)]
        public string? SupportingFindingNote { get; set; }

        public int SortOrder { get; set; } = 0;
    }

    public class UpdatePatientDiagnosisRequest
    {
        public Guid? DiagnosisId { get; set; }

        [MaxLength(50)]
        public string? DiagnosisCode { get; set; }

        [MaxLength(500)]
        public string? DiagnosisName { get; set; }

        [MaxLength(50)]
        public string? DiagnosisMasterType { get; set; }

        [MaxLength(100)]
        public string? IcdVersion { get; set; }

        public PatientDiagnosisType DiagnosisType { get; set; } = PatientDiagnosisType.Secondary;

        public bool IsPrimary { get; set; } = false;

        public bool IsChronic { get; set; } = false;

        public bool IsNewCase { get; set; } = true;

        public bool IsConfirmed { get; set; } = true;

        public DateTime? OnsetDate { get; set; }

        [MaxLength(1000)]
        public string? ClinicalNote { get; set; }

        [MaxLength(1000)]
        public string? AssessmentNote { get; set; }

        [MaxLength(1000)]
        public string? PlanNote { get; set; }

        [MaxLength(500)]
        public string? DifferentialDiagnosisNote { get; set; }

        [MaxLength(500)]
        public string? SupportingFindingNote { get; set; }

        public int SortOrder { get; set; } = 0;
    }

    public class PatientDiagnosisCreateResponse
    {
        public Guid Id { get; set; }

        public Guid EncounterId { get; set; }

        public Guid ConsultationId { get; set; }

        public Guid? DiagnosisId { get; set; }

        public string DiagnosisCode { get; set; } = string.Empty;

        public string DiagnosisName { get; set; } = string.Empty;

        public string DiagnosisMasterType { get; set; } = string.Empty;

        public string? IcdVersion { get; set; }

        public PatientDiagnosisType DiagnosisType { get; set; }

        public PatientDiagnosisStatus DiagnosisStatus { get; set; }

        public bool IsPrimary { get; set; }

        public bool IsConfirmed { get; set; }

        public bool IsFromMasterDiagnosis { get; set; }

        public int DiagnosisCount { get; set; }

        public bool HasPrimaryDiagnosis { get; set; }

        public string? DiagnosisText { get; set; }

        public string? PrimaryDiagnosisText { get; set; }

        public string? SecondaryDiagnosisText { get; set; }
    }

    public class PatientDiagnosisUpdateResponse : PatientDiagnosisCreateResponse
    {
    }

    public class SetPrimaryPatientDiagnosisRequest
    {
        public bool IsPrimary { get; set; } = true;
    }

    public class ResolvePatientDiagnosisRequest
    {
        [Required]
        [MaxLength(250)]
        public string ResolvedReason { get; set; } = string.Empty;
    }

    public class CancelPatientDiagnosisRequest
    {
        [Required]
        [MaxLength(250)]
        public string CancelReason { get; set; } = string.Empty;
    }
}