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

        /// <summary>
        /// Antrean asal pengkajian. <b>Boleh kosong</b> sejak <c>BE-IGD-026</c>.
        /// </summary>
        /// <remarks>
        /// Pasien IGD tidak pernah masuk antrean poli — kegawatan yang menentukan urutan
        /// penanganan, bukan nomor antrean. Selama kolom ini wajib terisi, pengkajian pasien
        /// IGD tidak dapat disimpan sama sekali (<c>IGD-DEC-068</c>, <c>FR-IGD-060</c>).
        ///
        /// <para>
        /// <b>Jalur rawat jalan tidak berubah.</b> Setiap pengkajian yang berasal dari antrean
        /// tetap mengisi kolom ini, dan seluruh baris lama tetap terisi — melepas kewajiban
        /// terisi tidak mengubah satu nilai pun.
        /// </para>
        ///
        /// <para>
        /// Tabel ini milik <c>ClinicalManagement</c> yang pemiliknya belum ditunjuk; perubahan
        /// dikerjakan IGD atas wewenang <c>IGD-DEC-107</c>.
        /// </para>
        /// </remarks>
        public Guid? QueueId { get; set; }

        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public Guid ServiceUnitId { get; set; }

        public Guid? ClinicId { get; set; }

        public Guid? DoctorId { get; set; }

        /// <summary>
        /// Perawatan rawat inap yang menaungi pengkajian ini. Boleh kosong — <c>INV-DOK-01</c>.
        /// </summary>
        /// <remarks>
        /// Kolom ini diminta bersama oleh sub-modul <c>keperawatan</c> (<c>INT-KEP-01</c>) dan
        /// <c>dokter-rawat-inap</c> (<c>INT-DOK-01</c>). Dibuat sekali oleh yang mendarat lebih
        /// dulu, lalu dipakai apa adanya oleh yang kedua — <c>INT-DOK-09</c>.
        /// </remarks>
        public Guid? InpEpisodeId { get; set; }

        /// <summary>
        /// Jenis pengkajian: pengkajian keperawatan atau kajian medis.
        /// </summary>
        /// <remarks>
        /// <c>BE-RWI-040</c>. Bawaannya <c>Initial</c>, sehingga seluruh baris lama milik
        /// poliklinik dan IGD terbaca sebagai pengkajian awal dan tidak perlu disentuh.
        /// Pembedaan antara kajian medis dan pengkajian keperawatan dijaga aturan bisnis lewat
        /// kolom ini; mesin hak akses hanya melihat satu sumber daya.
        /// </remarks>
        public PatientAssessmentType AssessmentType { get; set; } = PatientAssessmentType.Initial;

        /// <summary>
        /// Batas waktu penyelesaian pengkajian ini, dihitung saat pengkajian dibuat. Kosong
        /// bila belum ada kebijakan batas waktu yang berlaku.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>BE-RWI-054</c>, <c>FR-KEP-010</c>, PRD 16.2 aturan 11. Tenggat disimpan sebagai
        /// nilai, bukan dihitung ulang setiap dibaca. Sebabnya sederhana: kebijakan batas waktu
        /// boleh berubah, dan pengkajian yang dulu tepat waktu <b>tidak boleh</b> berubah
        /// menjadi terlambat hanya karena angkanya diperbarui hari ini.
        /// </para>
        /// <para>
        /// <b>Kosong bukan berarti terlambat.</b> Selama <c>MstClinicalAssessmentPolicy</c>
        /// belum berisi kebijakan yang berlaku, kolom ini dibiarkan kosong dan pengkajiannya
        /// terbaca sebagai <i>belum dipantau</i> - <c>VAL-KEP-17</c>. Pencatatan tetap berjalan
        /// penuh.
        /// </para>
        /// </remarks>
        public DateTime? DueAt { get; set; }

        /// <summary>
        /// Kebijakan batas waktu yang dipakai menghitung <see cref="DueAt"/>. Kosong bila tidak
        /// ada kebijakan yang berlaku saat pengkajian dibuat.
        /// </summary>
        /// <remarks>
        /// <c>BE-RWI-054</c>, <c>BE-RWI-055</c>. Menunjuk <c>MstClinicalAssessmentPolicy.Id</c>.
        /// Menyimpan penunjuknya - bukan hanya angkanya - membuat pertanyaan "menurut kebijakan
        /// yang mana pengkajian ini dinilai" terjawab bertahun-tahun kemudian, termasuk setelah
        /// kebijakannya diganti.
        /// </remarks>
        public Guid? PolicyId { get; set; }

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
        // ISIAN MEDIS — KAJIAN MEDIS DPJP
        // =========================
        // Ketiga kolom di bawah dipakai kajian medis (`AssessmentType` bernilai
        // `MedicalInitial` atau `MedicalReassessment`) dan dibiarkan kosong oleh pengkajian
        // keperawatan. Seluruhnya nullable, sehingga baris lama milik poliklinik, IGD, dan
        // keperawatan tidak perlu disentuh sama sekali.
        //
        // Tanpa ketiganya, `VAL-DOK-10` dan `VAL-DOK-11` tidak dapat ditegakkan: penyelesaian
        // kajian medis wajib menolak bila pemeriksaan, rencana, atau diagnosis masih kosong,
        // sedangkan kolom penampungnya sebelumnya tidak ada.

        /// <summary>
        /// Hasil pemeriksaan fisik yang ditulis DPJP pada kajian medis. Kosong untuk pengkajian
        /// keperawatan.
        /// </summary>
        /// <remarks>
        /// <c>BE-RWI-045</c>, <c>VAL-DOK-10</c>. Berbeda dari tanda vital, kesadaran, nyeri,
        /// gizi, dan risiko jatuh yang sudah ada di tabel ini — seluruhnya bercorak keperawatan
        /// dan terstruktur. Yang ini naratif, dan memang milik dokter.
        /// </remarks>
        [MaxLength(2000)]
        public string? PhysicalExamination { get; set; }

        /// <summary>
        /// Rencana terapi yang disusun DPJP pada kajian medis. Kosong untuk pengkajian
        /// keperawatan.
        /// </summary>
        /// <remarks>
        /// <c>BE-RWI-045</c>, <c>VAL-DOK-10</c>.
        /// </remarks>
        [MaxLength(2000)]
        public string? TherapyPlan { get; set; }

        /// <summary>
        /// Diagnosis kerja pada kajian medis awal. Kosong untuk pengkajian keperawatan.
        /// </summary>
        /// <remarks>
        /// <c>BE-RWI-045</c>, <c>VAL-DOK-11</c>. Ini <b>bukan</b> pengganti
        /// <c>TrxPatientDiagnosis</c>, yang tetap menjadi tempat diagnosis berkode ICD milik
        /// catatan dokter. Kolom ini menampung diagnosis kerja saat pemeriksaan pertama, ketika
        /// catatan dokter yang menaunginya belum ada — <c>TrxPatientDiagnosis</c> mewajibkan
        /// <c>ConsultationId</c>, sehingga diagnosis di sana selalu menggantung pada catatan.
        /// Melonggarkan kolom itu berarti menyentuh tabel yang sedang dipakai poliklinik, dan
        /// itu sengaja tidak dikerjakan di sini.
        /// </remarks>
        [MaxLength(500)]
        public string? WorkingDiagnosis { get; set; }

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
