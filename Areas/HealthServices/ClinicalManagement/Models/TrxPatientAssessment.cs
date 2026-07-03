using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models
{
    [Table("TrxPatientAssessment", Schema = "public")]
    public class TrxPatientAssessment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string AssessmentNumber { get; set; } = string.Empty;

        [Required]
        public Guid EncounterId { get; set; }

        [Required]
        public Guid QueueId { get; set; }

        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public Guid ServiceUnitId { get; set; }

        public Guid? ClinicId { get; set; }

        public Guid? DoctorId { get; set; }

        public DateTime AssessmentDateTime { get; set; } = DateTime.UtcNow;

        public PatientAssessmentStatus AssessmentStatus { get; set; } = PatientAssessmentStatus.Draft;

        public Guid? AssessmentByUserId { get; set; }

        // =========================
        // CHIEF COMPLAINT / SUBJECTIVE
        // =========================
        [MaxLength(500)]
        public string? ChiefComplaint { get; set; }

        [MaxLength(1000)]
        public string? CurrentIllnessHistory { get; set; }

        [MaxLength(1000)]
        public string? MedicationHistory { get; set; }

        // =========================
        // VITAL SIGN
        // =========================
        public int? BloodPressureSystolic { get; set; }

        public int? BloodPressureDiastolic { get; set; }

        public int? PulseRate { get; set; }

        public bool IsPulseReadable { get; set; } = true;

        public int? RespiratoryRate { get; set; }

        public decimal? Temperature { get; set; }

        public decimal? OxygenSaturation { get; set; }

        public bool IsUsingOxygen { get; set; } = false;

        public OxygenSupportType OxygenSupportType { get; set; } = OxygenSupportType.None;

        public decimal? OxygenFlowRate { get; set; }

        [MaxLength(100)]
        public string? OxygenSupportNote { get; set; }

        public ConsciousnessStatus ConsciousnessStatus { get; set; } = ConsciousnessStatus.Unknown;

        public decimal? Weight { get; set; }

        public decimal? Height { get; set; }

        public decimal? BMI { get; set; }

        public decimal? MeanArterialPressure { get; set; }

        public MapStatus MapStatus { get; set; } = MapStatus.Unknown;

        public int? EarlyWarningScore { get; set; }

        public EwsRiskLevel EwsRiskLevel { get; set; } = EwsRiskLevel.Unknown;

        [MaxLength(250)]
        public string? EwsMonitoringRecommendation { get; set; }

        // =========================
        // PAIN ASSESSMENT
        // =========================
        public bool HasPain { get; set; } = false;

        public int? PainScale { get; set; }

        [MaxLength(250)]
        public string? PainTrigger { get; set; }

        [MaxLength(250)]
        public string? PainQuality { get; set; }

        [MaxLength(250)]
        public string? PainLocation { get; set; }

        [MaxLength(250)]
        public string? PainFrequency { get; set; }

        [MaxLength(250)]
        public string? PainManagement { get; set; }

        [MaxLength(500)]
        public string? PainNote { get; set; }

        // =========================
        // HEREDITARY DISEASE
        // =========================
        public bool HasHereditaryDisease { get; set; } = false;

        [MaxLength(500)]
        public string? HereditaryDiseaseNote { get; set; }

        // =========================
        // ALLERGY
        // =========================
        public bool HasAllergy { get; set; } = false;

        [MaxLength(250)]
        public string? AllergyType { get; set; }

        [MaxLength(500)]
        public string? AllergyNote { get; set; }

        // =========================
        // IMMUNIZATION HISTORY (PEDIATRIC / BABY)
        // =========================
        public bool HasBcgImmunization { get; set; } = false;

        public bool HasHepatitisBImmunization { get; set; } = false;

        public bool HasPolioImmunization { get; set; } = false;

        public bool HasDptImmunization { get; set; } = false;

        public bool HasMeaslesImmunization { get; set; } = false;

        [MaxLength(500)]
        public string? ImmunizationNote { get; set; }

        // =========================
        // NUTRITION
        // =========================
        public AppetiteStatus AppetiteStatus { get; set; } = AppetiteStatus.Unknown;

        public bool HasNausea { get; set; } = false;

        public bool HasVomiting { get; set; } = false;

        public NutritionRiskStatus NutritionRiskStatus { get; set; } = NutritionRiskStatus.Unknown;

        public int? NutritionRiskScore { get; set; }

        [MaxLength(500)]
        public string? NutritionNote { get; set; }

        // =========================
        // FALL RISK
        // =========================
        public bool HasFallRisk { get; set; } = false;

        public bool HasAtaxia { get; set; } = false;

        public bool HasPosturalInstability { get; set; } = false;

        public FallRiskStatus FallRiskStatus { get; set; } = FallRiskStatus.Unknown;

        public int? FallRiskScore { get; set; }

        [MaxLength(500)]
        public string? FallRiskNote { get; set; }

        // =========================
        // FUNCTIONAL / PSYCHOSOCIAL / EDUCATION
        // =========================
        public FunctionalStatus FunctionalStatus { get; set; } = FunctionalStatus.Unknown;

        [MaxLength(500)]
        public string? FunctionalNote { get; set; }

        [MaxLength(500)]
        public string? PsychosocialNote { get; set; }

        [MaxLength(500)]
        public string? EducationNote { get; set; }

        [MaxLength(500)]
        public string? NurseNote { get; set; }

        // =========================
        // WORKFLOW
        // =========================
        public DateTime? StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public Guid? CompletedByUserId { get; set; }

        public DateTime? CancelledAt { get; set; }

        public Guid? CancelledByUserId { get; set; }

        [MaxLength(250)]
        public string? CancelReason { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxPatientEncounter? Encounter { get; set; }

        public TrxQueue? Queue { get; set; }

        public MstPatient? Patient { get; set; }

        public MstServiceUnit? ServiceUnit { get; set; }

        public MstClinic? Clinic { get; set; }

        public ApplicationUser? AssessmentByUser { get; set; }

        public ApplicationUser? CompletedByUser { get; set; }

        public ApplicationUser? CancelledByUser { get; set; }
    }
}
