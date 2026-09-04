using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;

/// <summary>
/// Perpindahan stok dari satu lokasi penyimpanan ke lokasi penyimpanan lain.
/// </summary>
/// <remarks>
/// <para>
/// Kedua ujungnya wajib lokasi yang memang menyimpan saldo sendiri. Unit pelayanan seperti
/// ICU atau kamar operasi bukan lokasi penyimpanan dan tidak pernah menjadi ujung transfer;
/// unit semacam itu memperoleh obat lewat permintaan kepada depo, bukan lewat perpindahan
/// saldo.
/// </para>
/// <para>
/// Barang keluar dan barang masuk dicatat sebagai dua langkah, bukan satu. Di antara
/// keduanya barang sedang di jalan: sudah tidak ada di asal, belum ada di tujuan. Bila
/// keduanya digabung, selisih akibat barang hilang atau rusak dalam perjalanan tidak akan
/// pernah muncul di mana pun.
/// </para>
/// </remarks>
[Table("TrxStockTransfer", Schema = "public")]
public class TrxStockTransfer : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)]
    public string TransferNumber { get; set; } = string.Empty;

    [Required] public Guid SourceStorageLocationId { get; set; }
    [Required] public Guid DestinationStorageLocationId { get; set; }

    public StockTransferStatus Status { get; set; } = StockTransferStatus.Draft;

    [Required] public Guid RequestedByWorkforceId { get; set; }

    [MaxLength(1000)] public string? Notes { get; set; }

    public DateTime RequestedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? IssuedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }

    /// <summary>Alasan penolakan atau pembatalan. Wajib saat status berpindah ke sana.</summary>
    [MaxLength(1000)] public string? DecisionReason { get; set; }

    public int ItemCount { get; set; }

    /// <summary>Token konkurensi; naik setiap perubahan.</summary>
    public int Version { get; set; }

    public MstDrugStorageLocation? SourceStorageLocation { get; set; }
    public MstDrugStorageLocation? DestinationStorageLocation { get; set; }
    public ICollection<TrxStockTransferItem> Items { get; set; } = [];
    public ICollection<TrxStockTransferHistory> Histories { get; set; } = [];
}

/// <summary>Satu obat yang dipindahkan, beserta jumlah yang diminta dan yang benar-benar keluar.</summary>
[Table("TrxStockTransferItem", Schema = "public")]
public class TrxStockTransferItem : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public Guid StockTransferId { get; set; }
    [Required] public Guid DrugId { get; set; }

    [Required, MaxLength(50)] public string DrugCodeSnapshot { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string DrugNameSnapshot { get; set; } = string.Empty;

    [Column(TypeName = "numeric(18,3)")]
    public decimal RequestedQuantity { get; set; }

    /// <summary>
    /// Jumlah yang benar-benar keluar dari lokasi asal.
    /// </summary>
    /// <remarks>
    /// Kosong selama barang belum dikeluarkan. Dapat lebih kecil daripada yang diminta bila
    /// stok yang tersedia ternyata kurang saat penyiapan.
    /// </remarks>
    [Column(TypeName = "numeric(18,3)")]
    public decimal? IssuedQuantity { get; set; }

    /// <summary>
    /// Jumlah yang benar-benar diterima lokasi tujuan.
    /// </summary>
    /// <remarks>
    /// Selisihnya terhadap <see cref="IssuedQuantity"/> adalah barang yang hilang atau rusak
    /// di perjalanan. Selisih itu sengaja dibiarkan terbaca, bukan disamakan diam-diam.
    /// </remarks>
    [Column(TypeName = "numeric(18,3)")]
    public decimal? ReceivedQuantity { get; set; }

    [MaxLength(500)] public string? Note { get; set; }

    public int LineNumber { get; set; }

    public TrxStockTransfer? StockTransfer { get; set; }
    public MstDrug? Drug { get; set; }
    public ICollection<TrxStockTransferAllocation> Allocations { get; set; } = [];
}

/// <summary>
/// Batch mana saja yang dipakai memenuhi satu baris transfer.
/// </summary>
/// <remarks>
/// Dicatat saat persetujuan, ketika stok ditahan menurut FEFO. Tanpa catatan ini, lokasi
/// tujuan tidak tahu batch apa yang harus diterimanya, dan penelusuran obat sampai ke
/// pasien akan terputus di tengah perjalanan.
/// </remarks>
[Table("TrxStockTransferAllocation", Schema = "public")]
public class TrxStockTransferAllocation : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public Guid StockTransferItemId { get; set; }
    [Required] public Guid DrugBatchId { get; set; }

    /// <summary>
    /// Urutan pengambilan batch, ditetapkan saat persetujuan menurut kedaluwarsa terdekat.
    /// </summary>
    /// <remarks>
    /// Disimpan sebagai angka tersendiri, bukan disandarkan pada waktu pembuatan baris:
    /// seluruh alokasi satu transfer dibuat pada saat yang sama persis, sehingga waktu tidak
    /// dapat membedakan urutannya. Urutan ini menentukan batch mana yang lebih dahulu
    /// menerima kredit ketika barang yang sampai lebih sedikit daripada yang dikirim.
    /// </remarks>
    public int SequenceNumber { get; set; }

    [Column(TypeName = "numeric(18,3)")]
    public decimal Quantity { get; set; }

    /// <summary>Benar bila reservasi atas batch ini sudah dilepas, baik karena dikeluarkan maupun dibatalkan.</summary>
    public bool IsReleased { get; set; }

    public TrxStockTransferItem? StockTransferItem { get; set; }
    public MstDrugBatch? DrugBatch { get; set; }
}

/// <summary>Jejak perpindahan status transfer, sekaligus penyimpan kunci idempotensi.</summary>
[Table("TrxStockTransferHistory", Schema = "public")]
public class TrxStockTransferHistory : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public Guid StockTransferId { get; set; }

    public StockTransferStatus? FromStatus { get; set; }
    public StockTransferStatus ToStatus { get; set; }

    [Required, MaxLength(50)] public string Action { get; set; } = string.Empty;
    [MaxLength(1000)] public string? Reason { get; set; }

    [Required] public Guid ActorUserId { get; set; }
    public DateTime OccurredAt { get; set; }

    [Required, MaxLength(100)] public string Source { get; set; } = string.Empty;
    [MaxLength(100)] public string? CorrelationId { get; set; }

    public TrxStockTransfer? StockTransfer { get; set; }
}
