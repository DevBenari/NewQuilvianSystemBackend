using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
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
[Table("GziNutritionCareRecord", Schema = "public")]
public class GziNutritionCareRecord : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public Guid NutritionOrderId { get; set; }

    /// <summary>Kunjungan ke berapa pada order ini, mulai dari 1.</summary>
    public int VisitSequence { get; set; }

    public DateTime VisitAt { get; set; }

    [Required] public Guid RecordedByWorkforceId { get; set; }

    public GziCareRecordType RecordType { get; set; } = GziCareRecordType.Initial;

    // --- Asesmen ---------------------------------------------------------------------
    [Column(TypeName = "numeric(6,2)")] public decimal? Weight { get; set; }
    [Column(TypeName = "numeric(6,2)")] public decimal? Height { get; set; }
    [Column(TypeName = "numeric(6,2)")] public decimal? Bmi { get; set; }

    [MaxLength(2000)] public string? AssessmentNote { get; set; }

    // --- Diagnosis gizi --------------------------------------------------------------
    /// <summary>
    /// Diagnosis IDNT yang ditegakkan pada kunjungan ini (`GIZ-DEC-011`). Boleh lebih dari
    /// satu, karena itu berupa tabel anak dan bukan satu kolom.
    /// </summary>
    public ICollection<GziNutritionCareRecordDiagnosis> Diagnoses { get; set; }
        = new List<GziNutritionCareRecordDiagnosis>();

    [MaxLength(1000)] public string? DiagnosisNote { get; set; }

    // --- Intervensi ------------------------------------------------------------------
    [MaxLength(2000)] public string? InterventionNote { get; set; }

    /// <summary>
    /// Diet yang ditetapkan pada kunjungan ini, menunjuk baris <c>GziPatientDiet</c>.
    /// </summary>
    /// <remarks>
    /// Menggantikan kolom teks bebas <c>DietPrescription</c>. `GIZ-DEC-012` menetapkan diet
    /// dipilih dari master diet rumah sakit; diet yang diketik bebas tidak dapat dipakai dapur
    /// untuk merekap produksi, dan dua ejaan berbeda bagi diet yang sama menjadi dua diet.
    /// </remarks>
    public Guid? PatientDietId { get; set; }

    /// <summary>
    /// Revisi kebutuhan nutrisi yang berlaku bagi kunjungan ini (`GIZ-DEC-012`).
    /// </summary>
    /// <remarks>
    /// Menggantikan kolom tunggal <c>EnergyRequirementKcal</c>. Kebutuhan nutrisi kini memuat
    /// lima parameter beserta nilai kalkulasi, nilai final, dan alasan koreksinya, sehingga
    /// pemiliknya adalah tabel kebutuhan — bukan satu angka yang menempel di sini.
    /// </remarks>
    public Guid? NutritionRequirementId { get; set; }

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

    public GziNutritionOrder? NutritionOrder { get; set; }
    public MstWorkforceProfile? RecordedByWorkforce { get; set; }
    public GziPatientDiet? PatientDiet { get; set; }
    public GziNutritionRequirement? NutritionRequirement { get; set; }
    public TrxPatientIntegratedProgressNote? ProgressNote { get; set; }
}
