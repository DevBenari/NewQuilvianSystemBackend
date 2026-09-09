using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;

/// <summary>
/// Pemakaian obat dan alat kesehatan untuk seorang pasien.
/// </summary>
/// <remarks>
/// <para>
/// Pasien, unit layanan, dan ruangannya tidak disalin ke sini; semuanya sudah melekat pada
/// kunjungan. Menyimpannya ulang hanya membuat dua tempat yang bisa berbeda isinya.
/// </para>
/// <para>
/// Stok diambil dari lokasi penyimpanan, bukan dari unit tempat pasien dirawat. Unit seperti
/// ICU atau kamar operasi tidak memiliki saldo sendiri; obat yang dipakai di sana tetap
/// dihitung keluar dari depo yang melayaninya.
/// </para>
/// <para>
/// Pencatatan ini berhenti sebagai transaksi yang <em>dapat</em> ditagihkan. Keputusan menagih
/// beserta aturannya milik Billing.
/// </para>
/// </remarks>
[Table("PhmDrugUsage", Schema = "public")]
public class PhmDrugUsage : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)]
    public string UsageNumber { get; set; } = string.Empty;

    /// <summary>Kunjungan pasien yang menjadi konteks pemakaian ini.</summary>
    [Required] public Guid EncounterId { get; set; }

    /// <summary>
    /// Resep yang diserahkan, bila pemakaian ini adalah penyerahan obat resep.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Kosong untuk pemakaian yang tidak berasal dari resep — obat yang dipakai di bangsal
    /// atau di kamar operasi. Karena itulah kolom ini nullable, dan pengisiannya tidak
    /// mengubah apa pun pada kedua alur tersebut.
    /// </para>
    /// <para>
    /// Tabel ini sengaja dipakai ulang alih-alih membuat tabel penyerahan tersendiri. Obat
    /// yang keluar dari stok untuk seorang pasien hanya boleh punya satu catatan yang
    /// authoritative; dua tabel untuk peristiwa yang sama akan membuat pertanyaan "berapa
    /// yang sudah diserahkan" punya dua jawaban yang bisa berbeda.
    /// </para>
    /// </remarks>
    public Guid? PrescriptionId { get; set; }

    /// <summary>Lokasi penyimpanan yang stoknya berkurang.</summary>
    [Required] public Guid StorageLocationId { get; set; }

    [Required] public Guid RecordedByWorkforceId { get; set; }

    public DrugUsageStatus Status { get; set; } = DrugUsageStatus.Draft;

    /// <summary>Kapan obatnya benar-benar dipakai, bukan kapan barisnya diketik.</summary>
    public DateTime UsedAt { get; set; }

    public DateTime? RecordedAt { get; set; }
    public DateTime? BilledAt { get; set; }

    [MaxLength(1000)] public string? Notes { get; set; }

    /// <summary>Alasan pembatalan. Wajib saat status berpindah ke sana.</summary>
    [MaxLength(1000)] public string? CancelReason { get; set; }

    public int ItemCount { get; set; }

    /// <summary>Token konkurensi; naik setiap perubahan.</summary>
    public int Version { get; set; }

    public TrxPatientEncounter? Encounter { get; set; }
    public MstDrugStorageLocation? StorageLocation { get; set; }
    public MstWorkforceProfile? RecordedByWorkforce { get; set; }
    public ICollection<PhmDrugUsageItem> Items { get; set; } = [];
}

/// <summary>Satu obat yang dipakai, beserta jumlah dan satuannya.</summary>
[Table("PhmDrugUsageItem", Schema = "public")]
public class PhmDrugUsageItem : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public Guid DrugUsageId { get; set; }
    [Required] public Guid DrugId { get; set; }
    [Required] public Guid MeasurementId { get; set; }

    /// <summary>
    /// Baris resep yang dipenuhi baris penyerahan ini.
    /// </summary>
    /// <remarks>
    /// Di sinilah, bukan di header, tautan ke resep menjadi berguna: satu kali penyerahan
    /// dapat memenuhi beberapa baris resep sekaligus, dan satu baris resep dapat diserahkan
    /// bertahap dalam beberapa kali penyerahan. Jumlah sisa sebuah baris resep dihitung
    /// dengan menjumlahkan seluruh baris penyerahan yang menunjuk ke sana.
    /// </remarks>
    public Guid? PrescriptionItemId { get; set; }

    [Required, MaxLength(50)] public string DrugCodeSnapshot { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string DrugNameSnapshot { get; set; } = string.Empty;
    [MaxLength(50)] public string? MeasurementNameSnapshot { get; set; }

    [Column(TypeName = "numeric(18,3)")]
    public decimal Quantity { get; set; }

    [MaxLength(500)] public string? Note { get; set; }

    public int LineNumber { get; set; }

    public PhmDrugUsage? DrugUsage { get; set; }
    public MstDrug? Drug { get; set; }
    public MstMeasurement? Measurement { get; set; }
    public ICollection<PhmDrugUsageAllocation> Allocations { get; set; } = [];
}

/// <summary>
/// Batch mana yang benar-benar dipakai untuk satu baris pemakaian.
/// </summary>
/// <remarks>
/// Inilah yang membuat obat dapat ditelusuri sampai ke pasien. Ketika sebuah batch ditarik
/// dari peredaran, pertanyaannya bukan "berapa sisanya di gudang" melainkan "siapa saja yang
/// sudah menerimanya" — dan hanya catatan ini yang dapat menjawabnya.
/// </remarks>
[Table("PhmDrugUsageAllocation", Schema = "public")]
public class PhmDrugUsageAllocation : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public Guid DrugUsageItemId { get; set; }
    [Required] public Guid DrugBatchId { get; set; }

    /// <summary>Urutan pengambilan batch, ditetapkan menurut kedaluwarsa terdekat.</summary>
    public int SequenceNumber { get; set; }

    [Column(TypeName = "numeric(18,3)")]
    public decimal Quantity { get; set; }

    public PhmDrugUsageItem? DrugUsageItem { get; set; }
    public PhmDrugBatch? DrugBatch { get; set; }
}
