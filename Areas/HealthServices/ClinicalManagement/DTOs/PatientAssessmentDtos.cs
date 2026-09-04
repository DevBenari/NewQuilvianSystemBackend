using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs
{
    public class PatientAssessmentResponse
    {
        public Guid Id { get; set; }
        public string AssessmentNumber { get; set; } = string.Empty;

        public Guid EncounterId { get; set; }
        public string EncounterNumber { get; set; } = string.Empty;

        /// <summary>
        /// Antrean asal pengkajian. <c>null</c> untuk pengkajian pasien IGD, yang memang tidak
        /// pernah berantre (<c>BE-IGD-026</c>).
        /// </summary>
        public Guid? QueueId { get; set; }
        public string QueueCode { get; set; } = string.Empty;

        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string MedicalRecordNumber { get; set; } = string.Empty;

        public Guid ServiceUnitId { get; set; }
        public string ServiceUnitName { get; set; } = string.Empty;

        public Guid? ClinicId { get; set; }
        public string? ClinicName { get; set; }

        public Guid? DoctorId { get; set; }
        public string? DoctorName { get; set; }

        /// <summary>
        /// Perawatan rawat inap yang menaungi pengkajian ini. <c>null</c> bagi pengkajian
        /// poliklinik, IGD, dan medical check-up.
        /// </summary>
        public Guid? InpEpisodeId { get; set; }

        /// <summary>
        /// Jenis pengkajian. Pembeda antara pengkajian keperawatan dan <b>kajian medis</b>
        /// milik DPJP - <c>BE-RWI-045</c>, <c>CAP-022</c>.
        /// </summary>
        public PatientAssessmentType AssessmentType { get; set; }

        public DateTime AssessmentDateTime { get; set; }
        public PatientAssessmentStatus AssessmentStatus { get; set; }

        public Guid? AssessmentByUserId { get; set; }
        public string? AssessmentByUserName { get; set; }

        public string? ChiefComplaint { get; set; }

        public int? BloodPressureSystolic { get; set; }
        public int? BloodPressureDiastolic { get; set; }
        public int? PulseRate { get; set; }
        public bool IsPulseReadable { get; set; }
        public int? RespiratoryRate { get; set; }
        public decimal? Temperature { get; set; }
        public decimal? OxygenSaturation { get; set; }

        public bool IsUsingOxygen { get; set; }
        public OxygenSupportType OxygenSupportType { get; set; }
        public decimal? OxygenFlowRate { get; set; }

        public ConsciousnessStatus ConsciousnessStatus { get; set; }

        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
        public decimal? BMI { get; set; }

        public decimal? MeanArterialPressure { get; set; }
        public MapStatus MapStatus { get; set; }

        public int? EarlyWarningScore { get; set; }
        public EwsRiskLevel EwsRiskLevel { get; set; }

        public bool HasPain { get; set; }
        public int? PainScale { get; set; }

        public bool HasHereditaryDisease { get; set; }

        public bool HasAllergy { get; set; }
        public string? AllergyType { get; set; }

        public bool HasBcgImmunization { get; set; }
        public bool HasHepatitisBImmunization { get; set; }
        public bool HasPolioImmunization { get; set; }
        public bool HasDptImmunization { get; set; }
        public bool HasMeaslesImmunization { get; set; }

        public AppetiteStatus AppetiteStatus { get; set; }
        public bool HasNausea { get; set; }
        public bool HasVomiting { get; set; }

        public bool HasFallRisk { get; set; }
        public FallRiskStatus FallRiskStatus { get; set; }

        public NutritionRiskStatus NutritionRiskStatus { get; set; }
        public FunctionalStatus FunctionalStatus { get; set; }

        /// <summary>
        /// Catatan perawat, sengaja ikut pada respons <b>daftar</b> dan bukan hanya detail.
        /// </summary>
        /// <remarks>
        /// Riwayat Assesmen Awal pada layar pengkajian IGD menampilkan kolom ini langsung
        /// dari hasil daftar, tanpa membuka detail satu per satu. Selama properti ini hanya
        /// ada di <see cref="PatientAssessmentDetailResponse"/>, kolom tersebut selalu kosong
        /// meskipun datanya tersimpan utuh di basis data.
        /// </remarks>
        public string? NurseNote { get; set; }

        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class PatientAssessmentDetailResponse : PatientAssessmentResponse
    {
        public string? CurrentIllnessHistory { get; set; }

        public string? MedicationHistory { get; set; }

        public string? OxygenSupportNote { get; set; }

        public string? EwsMonitoringRecommendation { get; set; }

        public string? PainTrigger { get; set; }
        public string? PainQuality { get; set; }
        public string? PainLocation { get; set; }
        public string? PainFrequency { get; set; }
        public string? PainManagement { get; set; }
        public string? PainNote { get; set; }

        public string? HereditaryDiseaseNote { get; set; }

        public string? AllergyNote { get; set; }

        public string? ImmunizationNote { get; set; }

        public int? NutritionRiskScore { get; set; }
        public string? NutritionNote { get; set; }

        public bool HasAtaxia { get; set; }
        public bool HasPosturalInstability { get; set; }
        public int? FallRiskScore { get; set; }
        public string? FallRiskNote { get; set; }

        public string? FunctionalNote { get; set; }
        public string? PsychosocialNote { get; set; }
        public string? EducationNote { get; set; }

        public Guid? CompletedByUserId { get; set; }
        public string? CompletedByUserName { get; set; }

        public DateTime? CancelledAt { get; set; }
        public Guid? CancelledByUserId { get; set; }
        public string? CancelledByUserName { get; set; }
        public string? CancelReason { get; set; }
    }

    public class CreatePatientAssessmentRequest
    {
        [Required]
        public Guid EncounterId { get; set; }

        /// <summary>
        /// Antrean asal pengkajian. <b>Boleh kosong</b> sejak <c>BE-IGD-027</c>: pasien IGD
        /// tidak punya baris antrean, sehingga pengkajiannya dibentuk dari encounter.
        /// </summary>
        /// <remarks>
        /// Jalur rawat jalan <b>tidak berubah</b> — bila kolom ini terisi, seluruh penjagaan
        /// antrean yang lama tetap berlaku persis seperti sebelumnya. Jalur tanpa antrean
        /// hanya terbuka untuk encounter yang punya kunjungan IGD.
        /// </remarks>
        public Guid? QueueId { get; set; }

        /// <summary>
        /// Perawatan rawat inap yang menaungi pengkajian ini. <b>Boleh kosong</b>: backend
        /// menurunkannya dari kunjungan bila tidak disebutkan.
        /// </summary>
        /// <remarks>
        /// <c>BE-RWI-044</c>, <c>VAL-DOK-26</c>. Bila terisi tetapi tidak cocok dengan perawatan
        /// milik <see cref="EncounterId"/>, permintaan ditolak <c>400</c>.
        /// </remarks>
        public Guid? InpEpisodeId { get; set; }

        /// <summary>
        /// Jenis pengkajian yang hendak dibuat. Bawaannya <c>Initial</c>, sehingga seluruh
        /// pengirim lama - poliklinik dan IGD - tidak perlu berubah sedikit pun.
        /// </summary>
        /// <remarks>
        /// <c>BE-RWI-045</c>, <c>CAP-022</c>, <c>AC-CAP022-02</c>. Nilai <c>MedicalInitial</c>
        /// dan <c>MedicalReassessment</c> adalah kajian medis: hanya boleh dibuat pengguna yang
        /// benar-benar terhubung ke data dokter, dan hanya di atas perawatan rawat inap yang
        /// berjalan - <c>VAL-DOK-01</c>, <c>VAL-DOK-05</c>.
        /// </remarks>
        public PatientAssessmentType AssessmentType { get; set; } = PatientAssessmentType.Initial;

        [MaxLength(500)]
        public string? ChiefComplaint { get; set; }

        [MaxLength(1000)]
        public string? CurrentIllnessHistory { get; set; }

        [MaxLength(1000)]
        public string? MedicationHistory { get; set; }

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

        public bool HasHereditaryDisease { get; set; } = false;

        [MaxLength(500)]
        public string? HereditaryDiseaseNote { get; set; }

        public bool HasAllergy { get; set; } = false;

        [MaxLength(250)]
        public string? AllergyType { get; set; }

        [MaxLength(500)]
        public string? AllergyNote { get; set; }

        public bool HasBcgImmunization { get; set; } = false;

        public bool HasHepatitisBImmunization { get; set; } = false;

        public bool HasPolioImmunization { get; set; } = false;

        public bool HasDptImmunization { get; set; } = false;

        public bool HasMeaslesImmunization { get; set; } = false;

        [MaxLength(500)]
        public string? ImmunizationNote { get; set; }

        public AppetiteStatus AppetiteStatus { get; set; } = AppetiteStatus.Unknown;

        public bool HasNausea { get; set; } = false;

        public bool HasVomiting { get; set; } = false;

        public NutritionRiskStatus NutritionRiskStatus { get; set; } = NutritionRiskStatus.Unknown;

        public int? NutritionRiskScore { get; set; }

        [MaxLength(500)]
        public string? NutritionNote { get; set; }

        public bool HasFallRisk { get; set; } = false;

        public bool HasAtaxia { get; set; } = false;

        public bool HasPosturalInstability { get; set; } = false;

        public FallRiskStatus FallRiskStatus { get; set; } = FallRiskStatus.Unknown;

        public int? FallRiskScore { get; set; }

        [MaxLength(500)]
        public string? FallRiskNote { get; set; }

        public FunctionalStatus FunctionalStatus { get; set; } = FunctionalStatus.Unknown;

        [MaxLength(500)]
        public string? FunctionalNote { get; set; }

        [MaxLength(500)]
        public string? PsychosocialNote { get; set; }

        [MaxLength(500)]
        public string? EducationNote { get; set; }

        [MaxLength(500)]
        public string? NurseNote { get; set; }

        public bool CompleteImmediately { get; set; } = false;
    }

    public class UpdatePatientAssessmentRequest
    {
        [MaxLength(500)]
        public string? ChiefComplaint { get; set; }

        [MaxLength(1000)]
        public string? CurrentIllnessHistory { get; set; }

        [MaxLength(1000)]
        public string? MedicationHistory { get; set; }

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

        public bool HasHereditaryDisease { get; set; } = false;

        [MaxLength(500)]
        public string? HereditaryDiseaseNote { get; set; }

        public bool HasAllergy { get; set; } = false;

        [MaxLength(250)]
        public string? AllergyType { get; set; }

        [MaxLength(500)]
        public string? AllergyNote { get; set; }

        public bool HasBcgImmunization { get; set; } = false;

        public bool HasHepatitisBImmunization { get; set; } = false;

        public bool HasPolioImmunization { get; set; } = false;

        public bool HasDptImmunization { get; set; } = false;

        public bool HasMeaslesImmunization { get; set; } = false;

        [MaxLength(500)]
        public string? ImmunizationNote { get; set; }

        public AppetiteStatus AppetiteStatus { get; set; } = AppetiteStatus.Unknown;

        public bool HasNausea { get; set; } = false;

        public bool HasVomiting { get; set; } = false;

        public NutritionRiskStatus NutritionRiskStatus { get; set; } = NutritionRiskStatus.Unknown;

        public int? NutritionRiskScore { get; set; }

        [MaxLength(500)]
        public string? NutritionNote { get; set; }

        public bool HasFallRisk { get; set; } = false;

        public bool HasAtaxia { get; set; } = false;

        public bool HasPosturalInstability { get; set; } = false;

        public FallRiskStatus FallRiskStatus { get; set; } = FallRiskStatus.Unknown;

        public int? FallRiskScore { get; set; }

        [MaxLength(500)]
        public string? FallRiskNote { get; set; }

        public FunctionalStatus FunctionalStatus { get; set; } = FunctionalStatus.Unknown;

        [MaxLength(500)]
        public string? FunctionalNote { get; set; }

        [MaxLength(500)]
        public string? PsychosocialNote { get; set; }

        [MaxLength(500)]
        public string? EducationNote { get; set; }

        [MaxLength(500)]
        public string? NurseNote { get; set; }
    }

    public class PatientAssessmentCreateResponse
    {
        public Guid Id { get; set; }
        public string AssessmentNumber { get; set; } = string.Empty;
        public Guid EncounterId { get; set; }
        /// <summary>
        /// Antrean asal pengkajian. <c>null</c> untuk pengkajian pasien IGD, yang memang tidak
        /// pernah berantre (<c>BE-IGD-026</c>).
        /// </summary>
        public Guid? QueueId { get; set; }
        public Guid? InpEpisodeId { get; set; }
        public PatientAssessmentType AssessmentType { get; set; }
        public PatientAssessmentStatus AssessmentStatus { get; set; }
        public DateTime AssessmentDateTime { get; set; }
        public DateTime? CompletedAt { get; set; }

        public decimal? BMI { get; set; }
        public decimal? MeanArterialPressure { get; set; }
        public MapStatus MapStatus { get; set; }
        public int? EarlyWarningScore { get; set; }
        public EwsRiskLevel EwsRiskLevel { get; set; }
        public string? EwsMonitoringRecommendation { get; set; }
    }

    public class CompletePatientAssessmentRequest
    {
        [MaxLength(500)]
        public string? NurseNote { get; set; }
    }

    public class PatientAssessmentCompleteResponse
    {
        public Guid Id { get; set; }
        public string AssessmentNumber { get; set; } = string.Empty;
        public Guid EncounterId { get; set; }
        /// <summary>
        /// Antrean asal pengkajian. <c>null</c> untuk pengkajian pasien IGD, yang memang tidak
        /// pernah berantre (<c>BE-IGD-026</c>).
        /// </summary>
        public Guid? QueueId { get; set; }
        public Guid? InpEpisodeId { get; set; }
        public PatientAssessmentType AssessmentType { get; set; }
        public PatientAssessmentStatus AssessmentStatus { get; set; }
        public DateTime? CompletedAt { get; set; }
        public Guid? CompletedByUserId { get; set; }
        public bool IsAlreadyCompleted { get; set; }

        /// <summary>
        /// Benar bila kajian yang baru diselesaikan ikut terdaftar pada mesin keutuhan rekam
        /// medis - <c>BE-RWI-045</c>, <c>RWI-AC-157</c>.
        /// </summary>
        public bool IsRegisteredToIntegrity { get; set; }
    }

    public class CancelPatientAssessmentRequest
    {
        [Required]
        [MaxLength(250)]
        public string CancelReason { get; set; } = string.Empty;
    }
}