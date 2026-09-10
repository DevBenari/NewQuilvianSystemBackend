using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;

/// <summary>
/// Satu penerbitan copy resep.
/// </summary>
/// <remarks>
/// <para>
/// <b>Ini bukan sumber kebenaran penyerahan.</b> Kebenaran penyerahan tetap pada
/// <c>PhmDrugUsage</c> yang menunjuk resepnya. Yang disimpan di sini adalah <em>apa yang
/// tertulis pada lembar yang dibawa pasien</em> pada saat lembar itu dicetak.
/// </para>
/// <para>
/// Keduanya sengaja dibedakan. Copy resep adalah dokumen yang berpindah tangan: pasien
/// membawanya ke apotek lain, dan apotek itu bertindak atas angka yang tertera padanya. Bila
/// angkanya dihitung ulang setiap kali dibuka, lembar yang sudah beredar dan tampilan di
/// sistem dapat menyatakan dua hal yang berbeda tanpa ada yang tahu mana yang dipegang
/// pasien. Karena itu angkanya dibekukan saat terbit.
/// </para>
/// <para>
/// Snapshot ini <b>tidak pernah dibaca kembali</b> sebagai jumlah yang sudah diserahkan.
/// Pertanyaan "berapa sisa obat pasien ini sekarang" selalu dijawab histori penyerahan, bukan
/// dokumen ini.
/// </para>
/// </remarks>
[Table("PhmPrescriptionCopy", Schema = "public")]
public class PhmPrescriptionCopy : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)]
    public string CopyNumber { get; set; } = string.Empty;

    [Required] public Guid PrescriptionId { get; set; }

    public PrescriptionCopyStatus Status { get; set; } = PrescriptionCopyStatus.Issued;

    public DateTime IssuedAt { get; set; }

    /// <summary>Pengguna yang menerbitkan; selalu terisi.</summary>
    [Required] public Guid IssuedByUserId { get; set; }

    /// <summary>
    /// Apoteker penanggung jawab, bila petugas yang menerbitkan memang tercatat sebagai
    /// tenaga kerja.
    /// </summary>
    public Guid? PharmacistWorkforceId { get; set; }

    // ------------------------------------------------ identitas fasilitas saat terbit

    /// <summary>Fasilitas penerbit, diambil dari master lokasi rumah sakit.</summary>
    public Guid? HospitalSiteId { get; set; }

    [MaxLength(200)] public string? SiteNameSnapshot { get; set; }
    [MaxLength(500)] public string? SiteAddressSnapshot { get; set; }
    [MaxLength(50)] public string? SitePhoneSnapshot { get; set; }

    // -------------------------------------------------- identitas apoteker saat terbit

    [MaxLength(200)] public string? PharmacistNameSnapshot { get; set; }

    /// <summary>
    /// Nomor izin praktik apoteker, dibaca dari master kredensial.
    /// </summary>
    /// <remarks>
    /// Kosong bila kredensialnya belum terisi. Kekosongan itu ditampilkan apa adanya dan
    /// <b>tidak pernah diminta sebagai isian petugas</b>: nomor izin adalah fakta pada master
    /// kepegawaian, bukan sesuatu yang diketik ulang saat mencetak dokumen.
    /// </remarks>
    [MaxLength(100)] public string? PharmacistLicenseNumberSnapshot { get; set; }

    [MaxLength(100)] public string? PharmacistLicenseTypeSnapshot { get; set; }

    // ------------------------------------------------------------------------ lainnya

    [MaxLength(1000)] public string? Notes { get; set; }

    public DateTime? RevokedAt { get; set; }
    public Guid? RevokedByUserId { get; set; }
    [MaxLength(1000)] public string? RevokeReason { get; set; }

    public int ItemCount { get; set; }

    /// <summary>Token konkurensi; naik setiap perubahan.</summary>
    public int Version { get; set; }

    public PhmPrescription? Prescription { get; set; }
    public MstHospitalSite? HospitalSite { get; set; }
    public MstWorkforceProfile? PharmacistWorkforce { get; set; }
    public ICollection<PhmPrescriptionCopyItem> Items { get; set; } = [];
}

/// <summary>
/// Satu baris obat sebagaimana tertulis pada lembar copy resep.
/// </summary>
/// <remarks>
/// Isinya disalin dari snapshot resep dan dari perhitungan penyerahan saat terbit. Nama obat
/// pun disalin, bukan dibaca ulang dari master: master dapat berubah kemudian, sedangkan
/// lembar yang sudah dicetak tidak.
/// </remarks>
[Table("PhmPrescriptionCopyItem", Schema = "public")]
public class PhmPrescriptionCopyItem : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public Guid PrescriptionCopyId { get; set; }

    /// <summary>Baris resep asal, supaya lembar ini dapat ditelusuri balik.</summary>
    [Required] public Guid PrescriptionItemId { get; set; }

    public int LineNumber { get; set; }

    [Required, MaxLength(200)] public string DrugNameSnapshot { get; set; } = string.Empty;
    [MaxLength(200)] public string? GenericNameSnapshot { get; set; }
    [MaxLength(100)] public string? StrengthSnapshot { get; set; }
    [MaxLength(100)] public string? DrugFormSnapshot { get; set; }
    [MaxLength(50)] public string? DispenseUnitSnapshot { get; set; }

    /// <summary>Aturan pakai sebagaimana ditulis pemberi resep.</summary>
    [MaxLength(500)] public string? SignaSnapshot { get; set; }

    [MaxLength(500)] public string? AdministrationInstructionSnapshot { get; set; }

    public bool IsNarcoticSnapshot { get; set; }
    public bool IsPsychotropicSnapshot { get; set; }

    [Column(TypeName = "numeric(18,3)")]
    public decimal QuantityPrescribed { get; set; }

    [Column(TypeName = "numeric(18,3)")]
    public decimal QuantityDispensed { get; set; }

    [Column(TypeName = "numeric(18,3)")]
    public decimal QuantityRemaining { get; set; }

    /// <summary>
    /// Penanda kelengkapan penyerahan pada saat lembar ini terbit.
    /// </summary>
    /// <remarks>
    /// Diturunkan dari <see cref="QuantityRemaining"/> ketika dokumen dibuat, bukan diisi
    /// petugas. Yang disimpan adalah hasil turunan itu — karena lembar yang sudah beredar
    /// harus tetap menyatakan apa yang tercetak padanya, meskipun penyerahan berlanjut
    /// sesudahnya.
    /// </remarks>
    public PrescriptionCopyMark Mark { get; set; }

    public PhmPrescriptionCopy? PrescriptionCopy { get; set; }
    public TrxPrescriptionItem? PrescriptionItem { get; set; }
}
