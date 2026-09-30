using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Models;

/// <summary>
/// Kelompok besar diagnosis gizi, misalnya <c>NI</c>, <c>NC</c>, dan <c>NB</c> pada IDNT.
/// </summary>
/// <remarks>
/// Dibuat sebagai tabel, bukan enum. Enum menjadikan daftar domain sebagai ketetapan kode:
/// domain keempat menuntut rilis ulang aplikasi, dan nilai lama yang dihapus meninggalkan
/// angka tak bertuan pada data historis. Sebagai baris master, penambahannya cukup satu
/// perintah admin (`GIZ-DEC-011`).
/// </remarks>
[Table("GziNutritionDiagnosisDomain", Schema = "public")]
public class GziNutritionDiagnosisDomain : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(20)] public string DomainCode { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string DomainName { get; set; } = string.Empty;
    [MaxLength(1000)] public string? Description { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<GziNutritionDiagnosis> Diagnoses { get; set; } = new List<GziNutritionDiagnosis>();
}

/// <summary>
/// Master diagnosis gizi berkode, dengan standar IDNT sebagai baseline (`GIZ-DEC-011`).
/// </summary>
/// <remarks>
/// <para>
/// Master ini dibuat <b>KOSONG</b>. Menyebut IDNT sebagai baseline menetapkan standar yang
/// dipakai, bukan membebaskan sistem mengarang isinya. Daftar barisnya diimpor admin gizi;
/// daftar karangan akan terlihat resmi padahal tidak pernah disahkan siapa pun, dan diagnosis
/// yang salah menempel pada rekam medis pasien.
/// </para>
/// <para>
/// Master ini berdiri sendiri, tidak menumpang <c>MstDiagnosis</c>. <c>MstDiagnosis</c>
/// berbentuk ICD — ia memuat <c>IcdVersion</c>, <c>DiagnosisChapterId</c>, dan
/// <c>IsPrimaryDiagnosisAllowed</c> yang tak satu pun berlaku bagi diagnosis gizi, dan tidak
/// menyediakan tempat bagi <c>Domain</c>. Keputusan ini mengganti `GIZ-DEC-009`.
/// </para>
/// </remarks>
[Table("GziNutritionDiagnosis", Schema = "public")]
public class GziNutritionDiagnosis : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public Guid DiagnosisDomainId { get; set; }

    /// <summary>Kode IDNT berjenjang: <c>NI-5.2</c> berada di bawah <c>NI-5</c>.</summary>
    public Guid? ParentDiagnosisId { get; set; }

    [Required, MaxLength(30)] public string DiagnosisCode { get; set; } = string.Empty;
    [Required, MaxLength(300)] public string DiagnosisName { get; set; } = string.Empty;
    [MaxLength(1000)] public string? Description { get; set; }

    /// <summary>Asal baris, supaya penggantian standar kelak terlihat pada datanya sendiri.</summary>
    [Required, MaxLength(50)] public string Standard { get; set; } = "IDNT";
    [MaxLength(30)] public string? StandardVersion { get; set; }

    /// <summary>
    /// Baris yang hanya berfungsi sebagai kelompok dimatikan pilihannya, sehingga ahli gizi
    /// tidak dapat menegakkan diagnosis pada tingkat yang terlalu umum.
    /// </summary>
    public bool IsSelectable { get; set; } = true;

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public GziNutritionDiagnosisDomain? DiagnosisDomain { get; set; }
    public GziNutritionDiagnosis? ParentDiagnosis { get; set; }
    public ICollection<GziNutritionDiagnosis> ChildDiagnoses { get; set; } = new List<GziNutritionDiagnosis>();
}

/// <summary>
/// Diagnosis gizi yang ditegakkan pada satu kunjungan ahli gizi.
/// </summary>
/// <remarks>
/// Tabel anak, bukan satu kolom pada catatan asuhan. Praktik IDNT lazim menegakkan lebih dari
/// satu diagnosis dalam satu kunjungan; menyimpannya sebagai satu kolom berarti perpindahan ke
/// banyak diagnosis kelak menuntut pemindahan data historis.
/// </remarks>
[Table("GziNutritionCareRecordDiagnosis", Schema = "public")]
public class GziNutritionCareRecordDiagnosis : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public Guid CareRecordId { get; set; }
    [Required] public Guid NutritionDiagnosisId { get; set; }

    /// <summary>Paling banyak satu per kunjungan; ditegakkan indeks tersaring, bukan hanya service.</summary>
    public bool IsPrimary { get; set; }

    [MaxLength(1000)] public string? Note { get; set; }

    public int SortOrder { get; set; }

    public GziNutritionCareRecord? CareRecord { get; set; }
    public GziNutritionDiagnosis? NutritionDiagnosis { get; set; }
}
