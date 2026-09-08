using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;

/// <summary>
/// Pengembalian obat yang sudah diserahkan, kembali ke lokasi penyimpanan.
/// </summary>
/// <remarks>
/// <para>
/// Setiap baris wajib menyebut batchnya. Obat yang kembali harus masuk kembali sebagai batch
/// yang sama dengan asalnya — kalau tidak, tanggal kedaluwarsanya menjadi keliru dan penarikan
/// obat tidak lagi dapat menemukannya.
/// </para>
/// <para>
/// Stok tidak bertambah saat retur diajukan, melainkan setelah diperiksa. Obat yang kembali
/// sudah pernah keluar dari pengawasan farmasi; pemeriksalah yang menentukan apakah ia layak
/// kembali ke rak, atau harus ditahan lebih dahulu.
/// </para>
/// </remarks>
[Table("PhmDrugReturn", Schema = "public")]
public class PhmDrugReturn : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)]
    public string ReturnNumber { get; set; } = string.Empty;

    /// <summary>Kunjungan pasien yang obatnya dikembalikan.</summary>
    [Required] public Guid EncounterId { get; set; }

    /// <summary>Lokasi penyimpanan yang menerima kembali barangnya.</summary>
    [Required] public Guid StorageLocationId { get; set; }

    [Required] public Guid ReturnedByWorkforceId { get; set; }

    /// <summary>Petugas yang memeriksa. Terisi saat retur diperiksa.</summary>
    public Guid? VerifiedByWorkforceId { get; set; }

    /// <summary>
    /// Pemakaian obat yang dikembalikan, bila diketahui.
    /// </summary>
    /// <remarks>
    /// Boleh kosong: obat dapat dikembalikan tanpa dokumen pemakaian yang jelas, misalnya
    /// sisa dari bangsal. Bila terisi, penelusuran dari pengeluaran sampai pengembaliannya
    /// menjadi utuh.
    /// </remarks>
    public Guid? SourceDrugUsageId { get; set; }

    /// <summary>
    /// Pemakaian material kamar operasi yang dikembalikan, bila retur ini berasal dari sana.
    /// </summary>
    /// <remarks>
    /// Retur dari kamar operasi memakai alur retur yang sama dengan retur dari bangsal —
    /// pemeriksaan apoteker yang menentukan barang kembali ke stok atau tidak adalah
    /// pemeriksaan yang sama, dan membuat alur kedua hanya akan menggandakan aturannya.
    /// Kolom ini yang membedakan asalnya, sekaligus menjaga penelusuran dari pemakaian di
    /// kamar operasi sampai pengembaliannya tetap utuh.
    /// </remarks>
    public Guid? SourceOprMaterialUsageId { get; set; }

    public DrugReturnStatus Status { get; set; } = DrugReturnStatus.Draft;

    public DateTime ReturnedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }

    /// <summary>Alasan pengembalian, diisi pengaju.</summary>
    [MaxLength(1000)] public string? Reason { get; set; }

    /// <summary>Keterangan pemeriksa; wajib saat retur ditolak.</summary>
    [MaxLength(1000)] public string? DecisionReason { get; set; }

    public int ItemCount { get; set; }

    /// <summary>Token konkurensi; naik setiap perubahan.</summary>
    public int Version { get; set; }

    public TrxPatientEncounter? Encounter { get; set; }
    public MstDrugStorageLocation? StorageLocation { get; set; }
    public MstWorkforceProfile? ReturnedByWorkforce { get; set; }
    public MstWorkforceProfile? VerifiedByWorkforce { get; set; }
    public PhmDrugUsage? SourceDrugUsage { get; set; }
    public ICollection<PhmDrugReturnItem> Items { get; set; } = [];
}

/// <summary>Satu batch obat yang dikembalikan, beserta keadaan yang ditetapkan pemeriksa.</summary>
[Table("PhmDrugReturnItem", Schema = "public")]
public class PhmDrugReturnItem : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public Guid DrugReturnId { get; set; }
    [Required] public Guid DrugId { get; set; }

    /// <summary>
    /// Batch yang dikembalikan. Wajib, karena stok tidak dapat bertambah tanpa mengetahui
    /// batch dan kedaluwarsanya.
    /// </summary>
    [Required] public Guid DrugBatchId { get; set; }

    [Required] public Guid MeasurementId { get; set; }

    [Required, MaxLength(50)] public string DrugCodeSnapshot { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string DrugNameSnapshot { get; set; } = string.Empty;
    [MaxLength(50)] public string? MeasurementNameSnapshot { get; set; }

    [Column(TypeName = "numeric(18,3)")]
    public decimal Quantity { get; set; }

    /// <summary>
    /// Jumlah yang benar-benar diterima kembali setelah diperiksa.
    /// </summary>
    /// <remarks>
    /// Boleh lebih kecil daripada yang diajukan; selisihnya adalah barang yang ditolak
    /// pemeriksa dan tidak pernah masuk kembali ke stok mana pun.
    /// </remarks>
    [Column(TypeName = "numeric(18,3)")]
    public decimal? AcceptedQuantity { get; set; }

    /// <summary>
    /// Keadaan barang saat masuk kembali, ditetapkan pemeriksa.
    /// </summary>
    /// <remarks>
    /// Obat yang penyimpanannya selama di luar diragukan tidak langsung kembali siap pakai;
    /// ia dapat ditahan sebagai karantina sampai ada keputusan lanjutan.
    /// </remarks>
    public DrugStockStatus AcceptedStatus { get; set; } = DrugStockStatus.Available;

    [MaxLength(500)] public string? Note { get; set; }

    public int LineNumber { get; set; }

    public PhmDrugReturn? DrugReturn { get; set; }
    public MstDrug? Drug { get; set; }
    public MstDrugBatch? DrugBatch { get; set; }
    public MstMeasurement? Measurement { get; set; }
}

/// <summary>Jejak perpindahan status retur, sekaligus penyimpan kunci idempotensi.</summary>
[Table("PhmDrugReturnHistory", Schema = "public")]
public class PhmDrugReturnHistory : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public Guid DrugReturnId { get; set; }

    public DrugReturnStatus? FromStatus { get; set; }
    public DrugReturnStatus ToStatus { get; set; }

    [Required, MaxLength(50)] public string Action { get; set; } = string.Empty;
    [MaxLength(1000)] public string? Reason { get; set; }

    [Required] public Guid ActorUserId { get; set; }
    public DateTime OccurredAt { get; set; }

    [Required, MaxLength(100)] public string Source { get; set; } = string.Empty;
    [MaxLength(100)] public string? CorrelationId { get; set; }

    public PhmDrugReturn? DrugReturn { get; set; }
}
