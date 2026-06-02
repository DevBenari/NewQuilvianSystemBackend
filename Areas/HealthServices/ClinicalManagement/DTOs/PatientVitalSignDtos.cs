using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs
{
    public class PatientVitalSignResponse
    {
        public Guid Id { get; set; }
        public string VitalSignRecordNumber { get; set; } = string.Empty;

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

        public Guid? DoctorId { get; set; }
        public string? DoctorName { get; set; }

        public Guid? ServiceUnitId { get; set; }
        public string? ServiceUnitName { get; set; }

        public Guid? ClinicId { get; set; }
        public string? ClinicName { get; set; }

        public DateTime ObservationDateTime { get; set; }
        public PatientVitalSignSource VitalSignSource { get; set; }
        public PatientVitalSignStatus VitalSignStatus { get; set; }
        public Guid? ObservedByUserId { get; set; }
        public string? ObservedByUserName { get; set; }
        public string? ObservationLocation { get; set; }
        public PatientPosition PatientPosition { get; set; }
        public string? MeasurementMethod { get; set; }

        public int? BloodPressureSystolic { get; set; }
        public int? BloodPressureDiastolic { get; set; }
        public decimal? MeanArterialPressure { get; set; }
        public MapStatus MapStatus { get; set; }
        public int? PulseRate { get; set; }
        public bool IsPulseReadable { get; set; }
        public bool IsPulseRegular { get; set; }
        public int? RespiratoryRate { get; set; }
        public decimal? Temperature { get; set; }
        public decimal? OxygenSaturation { get; set; }
        public bool IsUsingOxygen { get; set; }
        public OxygenSupportType OxygenSupportType { get; set; }
        public decimal? OxygenFlowRate { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
        public decimal? BMI { get; set; }
        public ConsciousnessStatus ConsciousnessStatus { get; set; }
        public int? GcsTotal { get; set; }
        public bool HasPain { get; set; }
        public int? PainScale { get; set; }
        public int? EarlyWarningScore { get; set; }
        public EwsRiskLevel EwsRiskLevel { get; set; }

        public bool IsAbnormal { get; set; }
        public bool IsCritical { get; set; }
        public bool NeedDoctorNotification { get; set; }
        public DateTime? DoctorNotifiedAt { get; set; }

        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public Guid? VerifiedByUserId { get; set; }
        public string? VerifiedByUserName { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class PatientVitalSignDetailResponse : PatientVitalSignResponse
    {
        public string? DeviceName { get; set; }
        public string? DeviceSerialNumber { get; set; }
        public string? BloodPressureLocation { get; set; }
        public string? PulseRhythmNote { get; set; }
        public string? TemperatureRoute { get; set; }
        public string? OxygenSupportNote { get; set; }
        public string? WeightMeasurementNote { get; set; }
        public int? GcsEye { get; set; }
        public int? GcsVerbal { get; set; }
        public int? GcsMotor { get; set; }
        public string? NeurologicalNote { get; set; }
        public string? PainLocation { get; set; }
        public string? PainNote { get; set; }
        public string? EwsMonitoringRecommendation { get; set; }
        public Guid? DoctorNotifiedByUserId { get; set; }
        public string? DoctorNotifiedByUserName { get; set; }
        public string? DoctorNotificationNote { get; set; }
        public string? ClinicalNote { get; set; }
        public string? Notes { get; set; }
        public DateTime? CancelledAt { get; set; }
        public Guid? CancelledByUserId { get; set; }
        public string? CancelledByUserName { get; set; }
        public string? CancelReason { get; set; }
    }

    public class PatientVitalSignOptionResponse
    {
        public Guid Id { get; set; }
        public string VitalSignRecordNumber { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public DateTime ObservationDateTime { get; set; }
        public PatientVitalSignSource VitalSignSource { get; set; }
        public PatientVitalSignStatus VitalSignStatus { get; set; }
        public int? BloodPressureSystolic { get; set; }
        public int? BloodPressureDiastolic { get; set; }
        public int? PulseRate { get; set; }
        public int? RespiratoryRate { get; set; }
        public decimal? Temperature { get; set; }
        public decimal? OxygenSaturation { get; set; }
        public int? EarlyWarningScore { get; set; }
        public EwsRiskLevel EwsRiskLevel { get; set; }
        public bool IsAbnormal { get; set; }
        public bool IsCritical { get; set; }
        public bool IsVerified { get; set; }
    }

    public class PatientVitalSignAlertResponse
    {
        public Guid Id { get; set; }
        public string VitalSignRecordNumber { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public Guid? EncounterId { get; set; }
        public Guid? ConsultationId { get; set; }
        public DateTime ObservationDateTime { get; set; }
        public int? BloodPressureSystolic { get; set; }
        public int? BloodPressureDiastolic { get; set; }
        public int? PulseRate { get; set; }
        public int? RespiratoryRate { get; set; }
        public decimal? Temperature { get; set; }
        public decimal? OxygenSaturation { get; set; }
        public int? EarlyWarningScore { get; set; }
        public EwsRiskLevel EwsRiskLevel { get; set; }
        public bool IsAbnormal { get; set; }
        public bool IsCritical { get; set; }
        public bool NeedDoctorNotification { get; set; }
        public string? EwsMonitoringRecommendation { get; set; }
        public string? ClinicalNote { get; set; }
    }

    public class PatientVitalSignFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public PatientVitalSignDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<PatientVitalSignSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<PatientVitalSignEnumOptionResponse> VitalSignSourceOptions { get; set; } = new();
        public List<PatientVitalSignEnumOptionResponse> VitalSignStatusOptions { get; set; } = new();
        public List<PatientVitalSignEnumOptionResponse> PatientPositionOptions { get; set; } = new();
        public List<PatientVitalSignEnumOptionResponse> OxygenSupportTypeOptions { get; set; } = new();
        public List<PatientVitalSignEnumOptionResponse> ConsciousnessStatusOptions { get; set; } = new();
        public List<PatientVitalSignEnumOptionResponse> MapStatusOptions { get; set; } = new();
        public List<PatientVitalSignEnumOptionResponse> EwsRiskLevelOptions { get; set; } = new();
    }

    public class PatientVitalSignDefaultFilterResponse
    {
        public string? Search { get; set; }
        public Guid? PatientId { get; set; }
        public Guid? EncounterId { get; set; }
        public Guid? QueueId { get; set; }
        public Guid? AssessmentId { get; set; }
        public Guid? ConsultationId { get; set; }
        public Guid? DoctorId { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public Guid? ClinicId { get; set; }
        public PatientVitalSignSource? VitalSignSource { get; set; }
        public PatientVitalSignStatus? VitalSignStatus { get; set; }
        public PatientPosition? PatientPosition { get; set; }
        public ConsciousnessStatus? ConsciousnessStatus { get; set; }
        public MapStatus? MapStatus { get; set; }
        public EwsRiskLevel? EwsRiskLevel { get; set; }
        public bool? IsUsingOxygen { get; set; }
        public bool? HasPain { get; set; }
        public bool? IsAbnormal { get; set; }
        public bool? IsCritical { get; set; }
        public bool? NeedDoctorNotification { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SortBy { get; set; } = "observationDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class PatientVitalSignSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class PatientVitalSignEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreatePatientVitalSignRequest
    {
        [Required]
        public Guid PatientId { get; set; }
        public Guid? EncounterId { get; set; }
        public Guid? QueueId { get; set; }
        public Guid? AssessmentId { get; set; }
        public Guid? ConsultationId { get; set; }
        public Guid? DoctorId { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public Guid? ClinicId { get; set; }

        public DateTime? ObservationDateTime { get; set; }
        public PatientVitalSignSource VitalSignSource { get; set; } = PatientVitalSignSource.ManualEntry;
        public PatientVitalSignStatus VitalSignStatus { get; set; } = PatientVitalSignStatus.Recorded;
        [MaxLength(100)] public string? ObservationLocation { get; set; }
        public PatientPosition PatientPosition { get; set; } = PatientPosition.Unknown;
        [MaxLength(100)] public string? MeasurementMethod { get; set; }
        [MaxLength(100)] public string? DeviceName { get; set; }
        [MaxLength(100)] public string? DeviceSerialNumber { get; set; }

        public int? BloodPressureSystolic { get; set; }
        public int? BloodPressureDiastolic { get; set; }
        [MaxLength(50)] public string? BloodPressureLocation { get; set; }
        public int? PulseRate { get; set; }
        public bool IsPulseReadable { get; set; } = true;
        public bool IsPulseRegular { get; set; } = true;
        [MaxLength(100)] public string? PulseRhythmNote { get; set; }
        public int? RespiratoryRate { get; set; }
        public decimal? Temperature { get; set; }
        [MaxLength(50)] public string? TemperatureRoute { get; set; }
        public decimal? OxygenSaturation { get; set; }
        public bool IsUsingOxygen { get; set; } = false;
        public OxygenSupportType OxygenSupportType { get; set; } = OxygenSupportType.None;
        public decimal? OxygenFlowRate { get; set; }
        [MaxLength(100)] public string? OxygenSupportNote { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
        [MaxLength(100)] public string? WeightMeasurementNote { get; set; }
        public ConsciousnessStatus ConsciousnessStatus { get; set; } = ConsciousnessStatus.Unknown;
        public int? GcsEye { get; set; }
        public int? GcsVerbal { get; set; }
        public int? GcsMotor { get; set; }
        [MaxLength(250)] public string? NeurologicalNote { get; set; }
        public bool HasPain { get; set; } = false;
        public int? PainScale { get; set; }
        [MaxLength(250)] public string? PainLocation { get; set; }
        [MaxLength(250)] public string? PainNote { get; set; }
        public bool NeedDoctorNotification { get; set; } = false;
        [MaxLength(500)] public string? ClinicalNote { get; set; }
        [MaxLength(500)] public string? Notes { get; set; }
        public bool IsVerified { get; set; } = false;
    }

    public class UpdatePatientVitalSignRequest
    {
        public DateTime? ObservationDateTime { get; set; }
        public PatientVitalSignSource VitalSignSource { get; set; } = PatientVitalSignSource.ManualEntry;
        public PatientVitalSignStatus VitalSignStatus { get; set; } = PatientVitalSignStatus.Recorded;
        [MaxLength(100)] public string? ObservationLocation { get; set; }
        public PatientPosition PatientPosition { get; set; } = PatientPosition.Unknown;
        [MaxLength(100)] public string? MeasurementMethod { get; set; }
        [MaxLength(100)] public string? DeviceName { get; set; }
        [MaxLength(100)] public string? DeviceSerialNumber { get; set; }

        public int? BloodPressureSystolic { get; set; }
        public int? BloodPressureDiastolic { get; set; }
        [MaxLength(50)] public string? BloodPressureLocation { get; set; }
        public int? PulseRate { get; set; }
        public bool IsPulseReadable { get; set; } = true;
        public bool IsPulseRegular { get; set; } = true;
        [MaxLength(100)] public string? PulseRhythmNote { get; set; }
        public int? RespiratoryRate { get; set; }
        public decimal? Temperature { get; set; }
        [MaxLength(50)] public string? TemperatureRoute { get; set; }
        public decimal? OxygenSaturation { get; set; }
        public bool IsUsingOxygen { get; set; } = false;
        public OxygenSupportType OxygenSupportType { get; set; } = OxygenSupportType.None;
        public decimal? OxygenFlowRate { get; set; }
        [MaxLength(100)] public string? OxygenSupportNote { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
        [MaxLength(100)] public string? WeightMeasurementNote { get; set; }
        public ConsciousnessStatus ConsciousnessStatus { get; set; } = ConsciousnessStatus.Unknown;
        public int? GcsEye { get; set; }
        public int? GcsVerbal { get; set; }
        public int? GcsMotor { get; set; }
        [MaxLength(250)] public string? NeurologicalNote { get; set; }
        public bool HasPain { get; set; } = false;
        public int? PainScale { get; set; }
        [MaxLength(250)] public string? PainLocation { get; set; }
        [MaxLength(250)] public string? PainNote { get; set; }
        public bool NeedDoctorNotification { get; set; } = false;
        [MaxLength(500)] public string? ClinicalNote { get; set; }
        [MaxLength(500)] public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class PatientVitalSignCreateResponse
    {
        public Guid Id { get; set; }
        public string VitalSignRecordNumber { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public Guid? EncounterId { get; set; }
        public Guid? QueueId { get; set; }
        public Guid? AssessmentId { get; set; }
        public Guid? ConsultationId { get; set; }
        public DateTime ObservationDateTime { get; set; }
        public PatientVitalSignSource VitalSignSource { get; set; }
        public PatientVitalSignStatus VitalSignStatus { get; set; }
        public decimal? BMI { get; set; }
        public decimal? MeanArterialPressure { get; set; }
        public MapStatus MapStatus { get; set; }
        public int? GcsTotal { get; set; }
        public int? EarlyWarningScore { get; set; }
        public EwsRiskLevel EwsRiskLevel { get; set; }
        public bool IsAbnormal { get; set; }
        public bool IsCritical { get; set; }
        public bool NeedDoctorNotification { get; set; }
        public bool IsVerified { get; set; }
    }

    public class PatientVitalSignUpdateResponse : PatientVitalSignCreateResponse
    {
    }

    public class VerifyPatientVitalSignRequest
    {
        [MaxLength(500)]
        public string? ClinicalNote { get; set; }
    }

    public class NotifyDoctorPatientVitalSignRequest
    {
        [MaxLength(250)]
        public string? DoctorNotificationNote { get; set; }
    }

    public class CancelPatientVitalSignRequest
    {
        [Required]
        [MaxLength(250)]
        public string CancelReason { get; set; } = string.Empty;
    }
}
