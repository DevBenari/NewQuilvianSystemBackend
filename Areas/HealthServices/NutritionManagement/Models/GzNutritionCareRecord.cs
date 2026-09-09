using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Models;

/// <summary>
/// Satu kunjungan ahli gizi: asesmen, diagnosis, intervensi, recall asupan, dan evaluasi.
/// </summary>
/// <remarks>
/// <para>
/// Kelima bagian itu disimpan pada satu entity, bukan lima, karena selalu dicatat bersama
/// dalam satu kunjungan dan tidak pernah berdiri sendiri. Memecahnya berarti lima baris yang
/// harus dijaga tetap sinkron tanpa manfaat apa pun.
/// </para>
/// <para>
/// Hampir seluruh kolom boleh kosong karena ahli gizi mengisinya bertahap selama kunjungan.
/// Memaksa semuanya terisi sekaligus membuat catatan tidak dapat disimpan di tengah
/// pekerjaan, dan petugas akan mengakalinya dengan mengisi nilai sembarangan.
/// </para>
/// </remarks>
[Table("GzNutritionCareRecord", Schema = "public")]
public class GzNutritionCareRecord : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public Guid NutritionOrderId { get; set; }

    /// <summary>Kunjungan ke berapa pada order ini, mulai dari 1.</summary>
    public int VisitSequence { get; set; }

    public DateTime VisitAt { get; set; }

    [Required] public Guid RecordedByWorkforceId { get; set; }

    public GzCareRecordType RecordType { get; set; } = GzCareRecordType.Initial;

    // --- Asesmen ---------------------------------------------------------------------
    [Column(TypeName = "numeric(6,2)")] public decimal? Weight { get; set; }
    [Column(TypeName = "numeric(6,2)")] public decimal? Height { get; set; }
    [Column(TypeName = "numeric(6,2)")] public decimal? Bmi { get; set; }

    [MaxLength(2000)] public string? AssessmentNote { get; set; }

    // --- Diagnosis gizi --------------------------------------------------------------
    /// <summary>
    /// Menunjuk <c>MstDiagnosis</c> bertipe <c>NUTRITION</c> (`GIZ-DEC-009`).
    /// </summary>
    public Guid? NutritionDiagnosisId { get; set; }
    [MaxLength(1000)] public string? DiagnosisNote { get; set; }

    // --- Intervensi ------------------------------------------------------------------
    [MaxLength(2000)] public string? InterventionNote { get; set; }
    [MaxLength(500)] public string? DietPrescription { get; set; }

    /// <summary>
    /// Kebutuhan energi harian dalam kkal, DIKETIK ahli gizi (`GIZ-DEC-012`).
    /// </summary>
    /// <remarks>
    /// Sistem sengaja tidak memuat rumus apa pun. Rumus kebutuhan gizi berbeda antar rumah
    /// sakit dan antar kondisi pasien; menanamkannya berarti satu rumus keliru berdampak
    /// pada seluruh pasien sekaligus, dan kekeliruan itu sulit terlihat karena hasilnya
    /// tetap tampak masuk akal.
    /// </remarks>
    public int? EnergyRequirementKcal { get; set; }

    // --- Recall asupan ---------------------------------------------------------------
    [MaxLength(2000)] public string? IntakeRecallNote { get; set; }

    /// <summary>Perkiraan asupan terhadap kebutuhan, 0 sampai 100 persen.</summary>
    public int? IntakePercent { get; set; }

    // --- Monitoring dan evaluasi -----------------------------------------------------
    [MaxLength(2000)] public string? EvaluationNote { get; set; }

    // --- Tautan CPPT -----------------------------------------------------------------
    /// <summary>
    /// Baris CPPT yang dibuat untuk kunjungan ini (`GIZ-DEC-010`). Kosong bila catatan
    /// CPPT-nya belum dibuat.
    /// </summary>
    public Guid? ProgressNoteId { get; set; }

    public int Version { get; set; }

    public GzNutritionOrder? NutritionOrder { get; set; }
    public MstWorkforceProfile? RecordedByWorkforce { get; set; }
    public MstDiagnosis? NutritionDiagnosis { get; set; }
    public TrxPatientIntegratedProgressNote? ProgressNote { get; set; }
}
