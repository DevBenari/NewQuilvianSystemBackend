using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;

/// <summary>
/// Permintaan stok barang atau obat dari satu unit layanan kepada satu lokasi penyimpanan.
/// </summary>
/// <remarks>
/// <para>
/// Seluruh data induk dipakai ulang, tidak disalin: obat dari <c>MstDrug</c>, satuan dari
/// <c>MstMeasurement</c>, gudang dari <c>MstDrugStorageLocation</c>, dan unit peminta dari
/// <c>MstServiceUnit</c>. Modul ini hanya menyimpan penunjuknya.
/// </para>
/// <para>
/// Permintaan tidak menyimpan stok maupun mengurangi persediaan. Ia mencatat apa yang
/// diminta dan apa keputusannya; pergerakan stok yang sebenarnya milik modul persediaan
/// yang belum ada di sistem.
/// </para>
/// </remarks>
[Table("TrxStockRequest", Schema = "public")]
public class TrxStockRequest : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)]
    public string RequestNumber { get; set; } = string.Empty;

    /// <summary>Unit layanan yang meminta, misalnya depo farmasi rawat inap.</summary>
    [Required] public Guid RequestingServiceUnitId { get; set; }

    /// <summary>Lokasi penyimpanan yang diminta memenuhi, misalnya gudang farmasi utama.</summary>
    [Required] public Guid StorageLocationId { get; set; }

    [Required] public Guid RequestedByWorkforceId { get; set; }

    public StockRequestStatus Status { get; set; } = StockRequestStatus.Draft;
    public StockRequestPriority Priority { get; set; } = StockRequestPriority.Routine;

    /// <summary>Kapan barang dibutuhkan. Membantu gudang mengurutkan pengerjaan.</summary>
    public DateTime? NeededAt { get; set; }

    [MaxLength(1000)] public string? Notes { get; set; }

    public DateTime RequestedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? DecidedAt { get; set; }

    /// <summary>Alasan penolakan atau pembatalan. Wajib saat status berubah ke sana.</summary>
    [MaxLength(1000)] public string? DecisionReason { get; set; }

    /// <summary>Jumlah baris item; disimpan agar daftar riwayat tidak perlu menghitung ulang.</summary>
    public int ItemCount { get; set; }

    /// <summary>Token konkurensi; naik setiap perubahan.</summary>
    public int Version { get; set; }

    public MstServiceUnit? RequestingServiceUnit { get; set; }
    public MstDrugStorageLocation? StorageLocation { get; set; }
    public MstWorkforceProfile? RequestedByWorkforce { get; set; }
    public ICollection<TrxStockRequestItem> Items { get; set; } = [];
    public ICollection<TrxStockRequestHistory> Histories { get; set; } = [];
}

/// <summary>
/// Satu baris barang pada permintaan: obat apa, berapa banyak, dalam satuan apa.
/// </summary>
/// <remarks>
/// Nama dan kode obat disalin sebagai snapshot pada saat permintaan dibuat. Nama obat di
/// master dapat berubah kemudian; riwayat permintaan harus tetap menunjukkan apa yang
/// tertulis saat itu, bukan apa yang tertulis sekarang.
/// </remarks>
[Table("TrxStockRequestItem", Schema = "public")]
public class TrxStockRequestItem : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public Guid StockRequestId { get; set; }
    [Required] public Guid DrugId { get; set; }
    [Required] public Guid MeasurementId { get; set; }

    [Required, MaxLength(50)] public string DrugCodeSnapshot { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string DrugNameSnapshot { get; set; } = string.Empty;
    [MaxLength(50)] public string? MeasurementNameSnapshot { get; set; }

    [Column(TypeName = "numeric(18,3)")]
    public decimal RequestedQuantity { get; set; }

    /// <summary>
    /// Jumlah yang benar-benar diserahkan gudang. Kosong selama belum ada penyerahan;
    /// dibedakan dari nol, karena nol berarti diserahkan tetapi tidak ada satu pun.
    /// </summary>
    [Column(TypeName = "numeric(18,3)")]
    public decimal? FulfilledQuantity { get; set; }

    [MaxLength(500)] public string? Note { get; set; }

    public int LineNumber { get; set; }

    public TrxStockRequest? StockRequest { get; set; }
    public MstDrug? Drug { get; set; }
    public MstMeasurement? Measurement { get; set; }
}

/// <summary>
/// Jejak perubahan status permintaan, sekaligus penyimpan kunci idempotensi.
/// </summary>
/// <remarks>
/// <c>Source</c> menyimpan sidik jari isi permintaan dalam bentuk <c>API:{fingerprint}</c>.
/// Permintaan berulang dengan kunci sama tetapi isi berbeda ditolak, sehingga tombol yang
/// tertekan dua kali tidak menghasilkan dua permintaan.
/// </remarks>
[Table("TrxStockRequestHistory", Schema = "public")]
public class TrxStockRequestHistory : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public Guid StockRequestId { get; set; }

    public StockRequestStatus? FromStatus { get; set; }
    public StockRequestStatus ToStatus { get; set; }

    [Required, MaxLength(50)] public string Action { get; set; } = string.Empty;
    [MaxLength(1000)] public string? Reason { get; set; }

    [Required] public Guid ActorUserId { get; set; }
    public DateTime OccurredAt { get; set; }

    [Required, MaxLength(100)] public string Source { get; set; } = string.Empty;
    [MaxLength(100)] public string? CorrelationId { get; set; }

    public TrxStockRequest? StockRequest { get; set; }
}
