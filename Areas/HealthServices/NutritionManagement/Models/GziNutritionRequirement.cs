using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Models;

/// <summary>
/// Zat gizi yang dihitung dan disimpan untuk setiap pasien (`GIZ-DEC-012`).
/// </summary>
/// <remarks>
/// <para>
/// Disimpan sebagai master, bukan lima kolom tetap. Bila kelak serat, natrium, atau kalium ikut
/// dihitung, penambahannya berupa satu baris master — bukan kolom baru, bukan migration, dan
/// bukan perombakan data historis. Bentuk inilah yang diminta pemilik proses.
/// </para>
/// <para>
/// <c>MinValue</c> dan <c>MaxValue</c> boleh kosong, dan kosong berarti <b>belum ditetapkan
/// siapa pun</b> — bukan berarti tak terbatas dalam arti klinis. Mengarang batas berarti sistem
/// menolak angka yang barangkali benar.
/// </para>
/// </remarks>
[Table("GziNutritionParameter", Schema = "public")]
public class GziNutritionParameter : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(30)] public string ParameterCode { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string ParameterName { get; set; } = string.Empty;

    /// <summary>Satuan yang berlaku bagi nilai kalkulasi maupun nilai final, misalnya <c>kkal/hari</c>.</summary>
    [Required, MaxLength(30)] public string UnitCode { get; set; } = string.Empty;

    /// <summary>Jumlah angka di belakang koma saat ditampilkan.</summary>
    public int ValueScale { get; set; }

    [Column(TypeName = "numeric(12,3)")] public decimal? MinValue { get; set; }
    [Column(TypeName = "numeric(12,3)")] public decimal? MaxValue { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Registry rumus kebutuhan nutrisi. Dibuat <b>KOSONG</b> sampai `GIZ-OQ-007` dijawab.
/// </summary>
/// <remarks>
/// <para>
/// Rumus tidak ditanam di dalam kode perhitungan. Rumus kebutuhan gizi berbeda antar rumah sakit
/// dan antar kondisi pasien; menanamkan satu rumus berarti satu kekeliruan berdampak pada seluruh
/// pasien sekaligus, dan kekeliruan itu sulit terlihat karena hasilnya tetap tampak masuk akal.
/// </para>
/// <para>
/// <c>ImplementationKey</c> menghubungkan baris ini ke kelas perhitungan yang terdaftar di kode.
/// Selama belum ada satu pun baris aktif, kalkulasi tidak dijalankan dan nilai kalkulasi
/// dibiarkan kosong — bukan diisi angka asal.
/// </para>
/// </remarks>
[Table("GziNutritionFormula", Schema = "public")]
public class GziNutritionFormula : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)] public string FormulaCode { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string FormulaName { get; set; } = string.Empty;

    /// <summary>Rumus yang direvisi didaftarkan sebagai baris baru, bukan menimpa baris lama.</summary>
    [Required, MaxLength(30)] public string FormulaVersion { get; set; } = string.Empty;

    [MaxLength(2000)] public string? Description { get; set; }

    /// <summary>Sumber resmi rumus. Diisi admin saat mendaftarkan, tidak diambil dari internet.</summary>
    [MaxLength(500)] public string? SourceReference { get; set; }

    [Required, MaxLength(100)] public string ImplementationKey { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Satu revisi kebutuhan nutrisi untuk satu order gizi (`GIZ-DEC-012`).
/// </summary>
/// <remarks>
/// Perubahan tidak menimpa nilai sebelumnya: tiap perubahan melahirkan revisi baru, dan revisi
/// lama tetap terbaca. Tanpa itu, angka yang dipakai merawat pasien kemarin tidak dapat
/// ditelusuri ketika hasilnya dipersoalkan.
/// </remarks>
[Table("GziNutritionRequirement", Schema = "public")]
public class GziNutritionRequirement : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public Guid NutritionOrderId { get; set; }

    /// <summary>Kunjungan yang melahirkan revisi ini. Kosong bila ditetapkan di luar kunjungan.</summary>
    public Guid? CareRecordId { get; set; }

    /// <summary>Revisi ke berapa pada order ini, mulai dari 1.</summary>
    public int RevisionNumber { get; set; }

    /// <summary>Paling banyak satu revisi berlaku per order; ditegakkan indeks tersaring.</summary>
    public bool IsCurrent { get; set; } = true;

    public DateTime EffectiveFrom { get; set; }

    /// <summary>Rumus yang dipakai. Kosong berarti nilai final ditetapkan tanpa kalkulasi.</summary>
    public Guid? CalculationFormulaId { get; set; }

    /// <summary>
    /// Salinan masukan rumus: berat, tinggi, umur, faktor aktivitas, dan seterusnya.
    /// </summary>
    /// <remarks>
    /// Berat dan tinggi pasien berubah selama perawatan. Tanpa salinan ini, angka kebutuhan lama
    /// tidak lagi dapat dijelaskan — hasilnya ada, tetapi tidak ada yang tahu dari mana.
    /// </remarks>
    [Column(TypeName = "jsonb")] public string? CalculationInput { get; set; }

    public DateTime? CalculationPerformedAt { get; set; }

    [Required] public Guid DeterminedByWorkforceId { get; set; }

    /// <summary>Wajib mulai revisi kedua (`GIZ017`).</summary>
    [MaxLength(1000)] public string? ChangeReason { get; set; }

    public int Version { get; set; }

    public GziNutritionOrder? NutritionOrder { get; set; }
    public GziNutritionCareRecord? CareRecord { get; set; }
    public GziNutritionFormula? CalculationFormula { get; set; }
    public MstWorkforceProfile? DeterminedByWorkforce { get; set; }
    public ICollection<GziNutritionRequirementItem> Items { get; set; } = new List<GziNutritionRequirementItem>();
}

/// <summary>
/// Nilai satu parameter nutrisi pada satu revisi kebutuhan.
/// </summary>
/// <remarks>
/// Nilai kalkulasi, nilai final, alasan perubahan, pelaku, dan waktunya disimpan <b>per
/// parameter</b>, bukan per revisi. Ahli gizi lazimnya mengoreksi satu atau dua parameter saja,
/// dan alasan yang menempel pada revisi tidak dapat menerangkan parameter mana yang dimaksud.
/// </remarks>
[Table("GziNutritionRequirementItem", Schema = "public")]
public class GziNutritionRequirementItem : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public Guid NutritionRequirementId { get; set; }
    [Required] public Guid NutritionParameterId { get; set; }

    /// <summary>Hasil rumus. Kosong bila tidak ada rumus terdaftar — bukan diisi nol.</summary>
    [Column(TypeName = "numeric(12,3)")] public decimal? CalculatedValue { get; set; }

    /// <summary>Nilai yang dipakai merawat pasien.</summary>
    [Column(TypeName = "numeric(12,3)")] public decimal FinalValue { get; set; }

    /// <summary>Wajib bila nilai final berbeda dari nilai kalkulasi (`GIZ016`).</summary>
    [MaxLength(1000)] public string? AdjustmentReason { get; set; }

    public Guid? AdjustedByWorkforceId { get; set; }
    public DateTime? AdjustedAt { get; set; }

    public GziNutritionRequirement? NutritionRequirement { get; set; }
    public GziNutritionParameter? NutritionParameter { get; set; }
    public MstWorkforceProfile? AdjustedByWorkforce { get; set; }
}
