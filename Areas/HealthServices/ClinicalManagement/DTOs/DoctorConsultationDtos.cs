using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs
{
    public class DoctorConsultationResponse
    {
        public Guid Id { get; set; }
        public string ConsultationNumber { get; set; } = string.Empty;

        public Guid EncounterId { get; set; }
        public string EncounterNumber { get; set; } = string.Empty;

        public Guid QueueId { get; set; }
        public string QueueCode { get; set; } = string.Empty;

        public Guid? AssessmentId { get; set; }
        public string? AssessmentNumber { get; set; }

        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string MedicalRecordNumber { get; set; } = string.Empty;

        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;

        public Guid ServiceUnitId { get; set; }
        public string ServiceUnitName { get; set; } = string.Empty;

        public Guid? ClinicId { get; set; }
        public string? ClinicName { get; set; }

        public DateTime ConsultationDateTime { get; set; }
        public DoctorConsultationStatus ConsultationStatus { get; set; }

        public bool IsVitalSignCopiedFromAssessment { get; set; }

        public string? ChiefComplaint { get; set; }

        public string? DiagnosisText { get; set; }
        public string? PrimaryDiagnosisText { get; set; }
        public string? SecondaryDiagnosisText { get; set; }
        public int DiagnosisCount { get; set; }
        public bool HasPrimaryDiagnosis { get; set; }

        public string? ProcedureText { get; set; }
        public int ProcedureCount { get; set; }
        public bool HasProcedure { get; set; }

        public string? PrescriptionText { get; set; }
        public int PrescriptionCount { get; set; }
        public bool HasPrescription { get; set; }

        public string? SupportingOrderText { get; set; }
        public int SupportingOrderCount { get; set; }
        public bool HasSupportingOrder { get; set; }

        public int MedicalCertificateCount { get; set; }
        public int ClinicalDocumentCount { get; set; }
        public int ConsentCount { get; set; }

        public DateTime? StartedAt { get; set; }
        public Guid? StartedByUserId { get; set; }
        public string? StartedByUserName { get; set; }

        public DateTime? CompletedAt { get; set; }
        public Guid? CompletedByUserId { get; set; }
        public string? CompletedByUserName { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class DoctorConsultationDetailResponse : DoctorConsultationResponse
    {
        public int? BloodPressureSystolic { get; set; }
        public int? BloodPressureDiastolic { get; set; }
        public int? PulseRate { get; set; }
        public int? RespiratoryRate { get; set; }
        public decimal? Temperature { get; set; }
        public decimal? OxygenSaturation { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
        public decimal? BMI { get; set; }

        public string? HistoryOfPresentIllness { get; set; }
        public string? PhysicalExamination { get; set; }

        public string? Subjective { get; set; }
        public string? Objective { get; set; }
        public string? Assessment { get; set; }
        public string? Plan { get; set; }

        public string? ProcedurePlan { get; set; }
        public string? PrescriptionPlan { get; set; }
        public string? SupportingExamPlan { get; set; }
        public string? ReferralPlan { get; set; }
        public string? EducationPlan { get; set; }

        public DateTime? FollowUpDate { get; set; }
        public string? FollowUpNote { get; set; }

        public DateTime? CancelledAt { get; set; }
        public Guid? CancelledByUserId { get; set; }
        public string? CancelledByUserName { get; set; }
        public string? CancelReason { get; set; }

        public string? DoctorNote { get; set; }
    }

    public class DoctorConsultationFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public DoctorConsultationDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<DoctorConsultationSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<DoctorConsultationEnumOptionResponse> ConsultationStatusOptions { get; set; } = new();
    }

    public class DoctorConsultationDefaultFilterResponse
    {
        public string? Search { get; set; }
        public Guid? EncounterId { get; set; }
        public Guid? QueueId { get; set; }
        public Guid? AssessmentId { get; set; }
        public Guid? PatientId { get; set; }
        public Guid? DoctorId { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public Guid? ClinicId { get; set; }
        public DoctorConsultationStatus? ConsultationStatus { get; set; }
        public bool? HasPrimaryDiagnosis { get; set; }
        public bool? HasProcedure { get; set; }
        public bool? HasPrescription { get; set; }
        public bool? HasSupportingOrder { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SortBy { get; set; } = "consultationDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class DoctorConsultationSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class DoctorConsultationEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateDoctorConsultationRequest
    {
        [Required]
        public Guid EncounterId { get; set; }

        [Required]
        public Guid QueueId { get; set; }

        public Guid? AssessmentId { get; set; }

        public bool IsVitalSignCopiedFromAssessment { get; set; } = true;

        public int? BloodPressureSystolic { get; set; }

        public int? BloodPressureDiastolic { get; set; }

        public int? PulseRate { get; set; }

        public int? RespiratoryRate { get; set; }

        public decimal? Temperature { get; set; }

        public decimal? OxygenSaturation { get; set; }

        public decimal? Weight { get; set; }

        public decimal? Height { get; set; }

        [MaxLength(1000)]
        public string? ChiefComplaint { get; set; }

        [MaxLength(4000)]
        public string? HistoryOfPresentIllness { get; set; }

        [MaxLength(4000)]
        public string? PhysicalExamination { get; set; }

        [MaxLength(4000)]
        public string? Subjective { get; set; }

        [MaxLength(4000)]
        public string? Objective { get; set; }

        [MaxLength(4000)]
        public string? Assessment { get; set; }

        [MaxLength(4000)]
        public string? Plan { get; set; }

        [MaxLength(2000)]
        public string? ProcedurePlan { get; set; }

        [MaxLength(2000)]
        public string? PrescriptionPlan { get; set; }

        [MaxLength(2000)]
        public string? SupportingExamPlan { get; set; }

        [MaxLength(2000)]
        public string? ReferralPlan { get; set; }

        [MaxLength(2000)]
        public string? EducationPlan { get; set; }

        public DateTime? FollowUpDate { get; set; }

        [MaxLength(500)]
        public string? FollowUpNote { get; set; }

        [MaxLength(1000)]
        public string? DoctorNote { get; set; }

        public bool CompleteImmediately { get; set; } = false;
    }

    public class UpdateDoctorConsultationRequest
    {
        public bool IsVitalSignCopiedFromAssessment { get; set; } = true;

        public int? BloodPressureSystolic { get; set; }

        public int? BloodPressureDiastolic { get; set; }

        public int? PulseRate { get; set; }

        public int? RespiratoryRate { get; set; }

        public decimal? Temperature { get; set; }

        public decimal? OxygenSaturation { get; set; }

        public decimal? Weight { get; set; }

        public decimal? Height { get; set; }

        [MaxLength(1000)]
        public string? ChiefComplaint { get; set; }

        [MaxLength(4000)]
        public string? HistoryOfPresentIllness { get; set; }

        [MaxLength(4000)]
        public string? PhysicalExamination { get; set; }

        [MaxLength(4000)]
        public string? Subjective { get; set; }

        [MaxLength(4000)]
        public string? Objective { get; set; }

        [MaxLength(4000)]
        public string? Assessment { get; set; }

        [MaxLength(4000)]
        public string? Plan { get; set; }

        [MaxLength(2000)]
        public string? ProcedurePlan { get; set; }

        [MaxLength(2000)]
        public string? PrescriptionPlan { get; set; }

        [MaxLength(2000)]
        public string? SupportingExamPlan { get; set; }

        [MaxLength(2000)]
        public string? ReferralPlan { get; set; }

        [MaxLength(2000)]
        public string? EducationPlan { get; set; }

        public DateTime? FollowUpDate { get; set; }

        [MaxLength(500)]
        public string? FollowUpNote { get; set; }

        [MaxLength(1000)]
        public string? DoctorNote { get; set; }
    }

    public class CompleteDoctorConsultationRequest
    {
        [MaxLength(1000)]
        public string? ChiefComplaint { get; set; }

        [MaxLength(4000)]
        public string? HistoryOfPresentIllness { get; set; }

        [MaxLength(4000)]
        public string? PhysicalExamination { get; set; }

        [MaxLength(4000)]
        public string? Subjective { get; set; }

        [MaxLength(4000)]
        public string? Objective { get; set; }

        [MaxLength(4000)]
        public string? Assessment { get; set; }

        [MaxLength(4000)]
        public string? Plan { get; set; }

        [MaxLength(2000)]
        public string? ProcedurePlan { get; set; }

        [MaxLength(2000)]
        public string? PrescriptionPlan { get; set; }

        [MaxLength(2000)]
        public string? SupportingExamPlan { get; set; }

        [MaxLength(2000)]
        public string? ReferralPlan { get; set; }

        [MaxLength(2000)]
        public string? EducationPlan { get; set; }

        public DateTime? FollowUpDate { get; set; }

        [MaxLength(500)]
        public string? FollowUpNote { get; set; }

        [MaxLength(1000)]
        public string? DoctorNote { get; set; }
    }

    public class UpdateDoctorConsultationSoapRequest
    {
        [MaxLength(4000)]
        public string? Subjective { get; set; }

        [MaxLength(4000)]
        public string? Objective { get; set; }

        [MaxLength(4000)]
        public string? Assessment { get; set; }

        [MaxLength(4000)]
        public string? Plan { get; set; }

        [MaxLength(2000)]
        public string? ProcedurePlan { get; set; }

        [MaxLength(2000)]
        public string? PrescriptionPlan { get; set; }

        [MaxLength(2000)]
        public string? SupportingExamPlan { get; set; }

        [MaxLength(2000)]
        public string? ReferralPlan { get; set; }

        [MaxLength(2000)]
        public string? EducationPlan { get; set; }

        public DateTime? FollowUpDate { get; set; }

        public bool ClearFollowUpDate { get; set; } = false;

        [MaxLength(500)]
        public string? FollowUpNote { get; set; }

        [MaxLength(1000)]
        public string? DoctorNote { get; set; }
    }

    public class DoctorConsultationCreateResponse
    {
        public Guid Id { get; set; }
        public string ConsultationNumber { get; set; } = string.Empty;

        public Guid EncounterId { get; set; }
        public Guid QueueId { get; set; }
        public Guid? AssessmentId { get; set; }

        public DoctorConsultationStatus ConsultationStatus { get; set; }

        public DateTime ConsultationDateTime { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public bool IsVitalSignCopiedFromAssessment { get; set; }

        public int DiagnosisCount { get; set; }
        public bool HasPrimaryDiagnosis { get; set; }

        public int ProcedureCount { get; set; }
        public bool HasProcedure { get; set; }

        public int PrescriptionCount { get; set; }
        public bool HasPrescription { get; set; }

        public int SupportingOrderCount { get; set; }
        public bool HasSupportingOrder { get; set; }
    }

    public class DoctorConsultationUpdateResponse : DoctorConsultationCreateResponse
    {
    }

    public class DoctorConsultationSoapUpdateResponse : DoctorConsultationUpdateResponse
    {
        public string? Subjective { get; set; }
        public string? Objective { get; set; }
        public string? Assessment { get; set; }
        public string? Plan { get; set; }

        public string? ProcedurePlan { get; set; }
        public string? PrescriptionPlan { get; set; }
        public string? SupportingExamPlan { get; set; }
        public string? ReferralPlan { get; set; }
        public string? EducationPlan { get; set; }

        public DateTime? FollowUpDate { get; set; }
        public string? FollowUpNote { get; set; }
        public string? DoctorNote { get; set; }

        public DateTime SavedAt { get; set; }
    }

    public class CancelDoctorConsultationRequest
    {
        [Required]
        [MaxLength(250)]
        public string CancelReason { get; set; } = string.Empty;
    }
}