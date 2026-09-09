using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;

/// <summary>
/// Saldo satu batch obat, pada satu lokasi, dalam satu status.
/// </summary>
/// <remarks>
/// <para>
/// Inilah satu-satunya sumber kebenaran jumlah stok. Angkanya tidak pernah diubah langsung:
/// setiap perubahan berjalan lewat <see cref="PhmDrugStockMutation"/>, sehingga saldo dan
/// riwayatnya selalu dapat dicocokkan.
/// </para>
/// <para>
/// Kuncinya adalah tiga serangkai batch, lokasi, dan status. Dengan begitu sebagian batch
/// boleh berada di karantina sementara sisanya tetap siap pakai, tanpa perlu memindahkan
/// barangnya ke lokasi lain.
/// </para>
/// </remarks>
[Table("PhmDrugStockBalance", Schema = "public")]
public class PhmDrugStockBalance : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Obat pemilik batch ini.
    /// </summary>
    /// <remarks>
    /// Sebenarnya dapat diturunkan dari batch, tetapi disimpan ulang karena hampir semua
    /// pertanyaan stok berbentuk "berapa obat X di lokasi Y" dan menempuh tabel batch pada
    /// setiap pertanyaan itu membuat jalur terpanas menjadi mahal. Layanan menjaga agar nilai
    /// ini selalu sama dengan milik batchnya.
    /// </remarks>
    [Required] public Guid DrugId { get; set; }

    [Required] public Guid DrugBatchId { get; set; }
    [Required] public Guid StorageLocationId { get; set; }

    public DrugStockStatus Status { get; set; } = DrugStockStatus.Available;

    /// <summary>Jumlah fisik yang benar-benar ada di lokasi ini.</summary>
    [Column(TypeName = "numeric(18,3)")]
    public decimal QuantityOnHand { get; set; }

    /// <summary>
    /// Bagian dari <see cref="QuantityOnHand"/> yang sudah ditahan untuk proses yang sedang
    /// berjalan, sehingga tidak boleh diambil proses lain.
    /// </summary>
    /// <remarks>
    /// Barangnya masih di rak dan masih terhitung fisik; yang berkurang hanyalah yang tersedia.
    /// Ketika penyerahan berhasil, keduanya berkurang bersama-sama. Ketika prosesnya batal atau
    /// gagal, reservasinya dilepas dan hanya angka ini yang turun.
    /// </remarks>
    [Column(TypeName = "numeric(18,3)")]
    public decimal QuantityReserved { get; set; }

    /// <summary>Jumlah yang benar-benar dapat dipakai: fisik dikurangi yang sudah ditahan.</summary>
    [NotMapped]
    public decimal QuantityAvailable => QuantityOnHand - QuantityReserved;

    /// <summary>Token konkurensi; menjaga dua transaksi tidak saling menimpa saldo.</summary>
    public int Version { get; set; }

    public MstDrug? Drug { get; set; }
    public PhmDrugBatch? DrugBatch { get; set; }
    public MstDrugStorageLocation? StorageLocation { get; set; }
}

/// <summary>
/// Kartu stok: satu baris untuk setiap pergerakan yang pernah terjadi.
/// </summary>
/// <remarks>
/// <para>
/// Baris kartu stok tidak pernah diubah dan tidak pernah dihapus. Kekeliruan diperbaiki dengan
/// menambah baris baru yang menunjuk baris yang salah, bukan dengan menyunting yang lama —
/// sehingga apa yang pernah tercatat tetap dapat dibaca sebagaimana adanya saat itu.
/// </para>
/// <para>
/// Setiap baris menyimpan saldo sebelum dan sesudah, bukan hanya selisihnya. Dengan begitu
/// kartu stok dapat dibaca dan dicetak apa adanya tanpa menghitung ulang seluruh riwayat, dan
/// selisih yang tidak wajar langsung terlihat pada barisnya sendiri.
/// </para>
/// </remarks>
[Table("PhmDrugStockMutation", Schema = "public")]
public class PhmDrugStockMutation : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public Guid DrugId { get; set; }
    [Required] public Guid DrugBatchId { get; set; }
    [Required] public Guid StorageLocationId { get; set; }

    public DrugStockStatus Status { get; set; } = DrugStockStatus.Available;
    public DrugStockMutationType MutationType { get; set; }

    /// <summary>
    /// Besar pergerakan, bertanda: positif menambah, negatif mengurangi.
    /// </summary>
    /// <remarks>
    /// Tanda dipakai alih-alih dua kolom masuk dan keluar, supaya penjumlahan seluruh baris
    /// selalu sama dengan saldo akhir tanpa perlu aturan tambahan.
    /// </remarks>
    [Column(TypeName = "numeric(18,3)")]
    public decimal QuantityChange { get; set; }

    [Column(TypeName = "numeric(18,3)")]
    public decimal BalanceBefore { get; set; }

    [Column(TypeName = "numeric(18,3)")]
    public decimal BalanceAfter { get; set; }

    /// <summary>Alasan pergerakan. Wajib untuk penyesuaian, perpindahan status, dan koreksi.</summary>
    [MaxLength(1000)]
    public string? Reason { get; set; }

    /// <summary>Jenis dokumen yang menyebabkan pergerakan ini.</summary>
    [MaxLength(50)]
    public string? SourceDocumentType { get; set; }

    /// <summary>Identitas dokumen asal, agar pergerakan dapat ditelusuri ke sumbernya.</summary>
    public Guid? SourceDocumentId { get; set; }

    /// <summary>
    /// Menunjuk baris yang sedang dikoreksi oleh baris ini.
    /// </summary>
    /// <remarks>
    /// Terisi hanya pada baris koreksi. Bersama saldo sebelum dan sesudah, alasan, pelaku, dan
    /// waktunya, inilah jejak lengkap yang dituntut aturan koreksi.
    /// </remarks>
    public Guid? CorrectionOfMutationId { get; set; }

    [Required] public Guid ActorUserId { get; set; }
    public DateTime OccurredAt { get; set; }

    /// <summary>Sidik jari isi perintah, dipakai menolak pengulangan yang tidak disengaja.</summary>
    [Required, MaxLength(100)]
    public string Source { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? CorrelationId { get; set; }

    public MstDrug? Drug { get; set; }
    public PhmDrugBatch? DrugBatch { get; set; }
    public MstDrugStorageLocation? StorageLocation { get; set; }
    public PhmDrugStockMutation? CorrectionOfMutation { get; set; }
}
