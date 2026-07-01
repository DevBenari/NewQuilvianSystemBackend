using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models
{
    [Table("TrxPatientVitalSign", Schema = "public")]
    public class TrxPatientVitalSign : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string VitalSignRecordNumber { get; set; } = string.Empty;

        // =========================
        // CLINICAL CONTEXT
        // =========================

        [Required]
        public Guid PatientId { get; set; }

        public Guid? EncounterId { get; set; }

        public Guid? QueueId { get; set; }

        public Guid? AssessmentId { get; set; }

        public Guid? ConsultationId { get; set; }

        public Guid? DoctorId { get; set; }

        public Guid? ServiceUnitId { get; set; }

        public Guid? ClinicId { get; set; }

        // =========================
        // RECORD INFORMATION
        // =========================

        public DateTime ObservationDateTime { get; set; } = DateTime.UtcNow;

        public PatientVitalSignSource VitalSignSource { get; set; } = PatientVitalSignSource.ManualEntry;

        public PatientVitalSignStatus VitalSignStatus { get; set; } = PatientVitalSignStatus.Recorded;

        public Guid? ObservedByUserId { get; set; }

        [MaxLength(100)]
        public string? ObservationLocation { get; set; }
        // Contoh: Nurse Station, IGD Bed 01, Poli Umum, Rawat Inap.

        public PatientPosition PatientPosition { get; set; } = PatientPosition.Unknown;

        [MaxLength(100)]
        public string? MeasurementMethod { get; set; }
        // Manual, DigitalMonitor, BedsideMonitor, Thermometer, PulseOximeter.

        [MaxLength(100)]
        public string? DeviceName { get; set; }

        [MaxLength(100)]
        public string? DeviceSerialNumber { get; set; }

        // =========================
        // BLOOD PRESSURE
        // =========================

        public int? BloodPressureSystolic { get; set; }

        public int? BloodPressureDiastolic { get; set; }

        public decimal? MeanArterialPressure { get; set; }

        public MapStatus MapStatus { get; set; } = MapStatus.Unknown;

        [MaxLength(50)]
        public string? BloodPressureLocation { get; set; }
        // LeftArm, RightArm, LeftLeg, RightLeg.

        // =========================
        // PULSE / RESPIRATION / TEMPERATURE
        // =========================

        public int? PulseRate { get; set; }

        public bool IsPulseReadable { get; set; } = true;

        public bool IsPulseRegular { get; set; } = true;

        [MaxLength(100)]
        public string? PulseRhythmNote { get; set; }

        public int? RespiratoryRate { get; set; }

        public decimal? Temperature { get; set; }

        [MaxLength(50)]
        public string? TemperatureRoute { get; set; }
        // Axillary, Oral, Tympanic, Rectal, Temporal.

        // =========================
        // OXYGENATION
        // =========================

        public decimal? OxygenSaturation { get; set; }

        public bool IsUsingOxygen { get; set; } = false;

        public OxygenSupportType OxygenSupportType { get; set; } = OxygenSupportType.None;

        public decimal? OxygenFlowRate { get; set; }

        [MaxLength(100)]
        public string? OxygenSupportNote { get; set; }

        // =========================
        // ANTHROPOMETRY
        // =========================

        public decimal? Weight { get; set; }

        public decimal? Height { get; set; }

        public decimal? HeadCircumference { get; set; }
        // Khusus bayi/anak sesuai kebutuhan klinis. Satuan: cm.

        public decimal? BMI { get; set; }

        [MaxLength(100)]
        public string? WeightMeasurementNote { get; set; }

        // =========================
        // CONSCIOUSNESS / NEUROLOGICAL
        // =========================

        public ConsciousnessStatus ConsciousnessStatus { get; set; } = ConsciousnessStatus.Unknown;

        public int? GcsEye { get; set; }

        public int? GcsVerbal { get; set; }

        public int? GcsMotor { get; set; }

        public int? GcsTotal { get; set; }

        [MaxLength(250)]
        public string? NeurologicalNote { get; set; }

        // =========================
        // PAIN
        // =========================

        public bool HasPain { get; set; } = false;

        public int? PainScale { get; set; }

        [MaxLength(250)]
        public string? PainLocation { get; set; }

        [MaxLength(250)]
        public string? PainNote { get; set; }

        // =========================
        // EARLY WARNING SCORE
        // =========================

        public int? EarlyWarningScore { get; set; }

        public EwsRiskLevel EwsRiskLevel { get; set; } = EwsRiskLevel.Unknown;

        [MaxLength(250)]
        public string? EwsMonitoringRecommendation { get; set; }

        // =========================
        // CLINICAL FLAG
        // =========================

        public bool IsAbnormal { get; set; } = false;

        public bool IsCritical { get; set; } = false;

        public bool NeedDoctorNotification { get; set; } = false;

        public DateTime? DoctorNotifiedAt { get; set; }

        public Guid? DoctorNotifiedByUserId { get; set; }

        [MaxLength(250)]
        public string? DoctorNotificationNote { get; set; }

        // =========================
        // VERIFICATION
        // =========================

        public bool IsVerified { get; set; } = false;

        public DateTime? VerifiedAt { get; set; }

        public Guid? VerifiedByUserId { get; set; }

        [MaxLength(500)]
        public string? ClinicalNote { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        // =========================
        // CANCELLATION
        // =========================

        public DateTime? CancelledAt { get; set; }

        public Guid? CancelledByUserId { get; set; }

        [MaxLength(250)]
        public string? CancelReason { get; set; }

        public bool IsActive { get; set; } = true;

        // =========================
        // NAVIGATION
        // =========================

        public MstPatient? Patient { get; set; }

        public TrxPatientEncounter? Encounter { get; set; }

        public TrxQueue? Queue { get; set; }

        public TrxPatientAssessment? Assessment { get; set; }

        public TrxDoctorConsultation? Consultation { get; set; }

        public MstDoctor? Doctor { get; set; }

        public MstServiceUnit? ServiceUnit { get; set; }

        public MstClinic? Clinic { get; set; }

        public ApplicationUser? ObservedByUser { get; set; }

        public ApplicationUser? DoctorNotifiedByUser { get; set; }

        public ApplicationUser? VerifiedByUser { get; set; }

        public ApplicationUser? CancelledByUser { get; set; }
    }
}
