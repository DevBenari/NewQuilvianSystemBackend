using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;

/// <summary>
/// Satu batch obat: nomor batch, kedaluwarsanya, dan asal pemasoknya.
/// </summary>
/// <remarks>
/// <para>
/// Batch adalah identitas barang, bukan lokasi maupun jumlahnya. Satu batch yang sama dapat
/// tersebar di beberapa depo sekaligus; jumlahnya dicatat pada
/// <see cref="TrxDrugStockBalance"/>, satu baris per lokasi dan status.
/// </para>
/// <para>
/// Batch dan kedaluwarsa menjadi bagian inti persediaan karena empat hal bergantung padanya:
/// FEFO saat pengeluaran, penarikan obat (recall), pemantauan mendekati kedaluwarsa, dan
/// penelusuran obat yang sudah terlanjur dipakai pasien.
/// </para>
/// </remarks>
[Table("MstDrugBatch", Schema = "public")]
public class MstDrugBatch : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public Guid DrugId { get; set; }

    [Required, MaxLength(100)]
    public string BatchNumber { get; set; } = string.Empty;

    /// <summary>
    /// Tanggal kedaluwarsa batch ini.
    /// </summary>
    /// <remarks>
    /// Kolomnya dibuat boleh kosong, tetapi layanan menolak batch tanpa tanggal kedaluwarsa.
    /// Pemisahan ini disengaja: aturan bisnis menyebut kedaluwarsa wajib "untuk item yang
    /// memang memiliki expired", sehingga kelak dapat ada item non-expired tanpa perlu
    /// mengubah bentuk tabel yang sudah berisi transaksi. Selama pengecualian itu belum
    /// ditetapkan, seluruh batch wajib mengisinya.
    /// </remarks>
    public DateOnly? ExpiryDate { get; set; }

    /// <summary>
    /// Pemasok asal batch, bila diketahui. Dipakai untuk penarikan obat dan investigasi mutu.
    /// </summary>
    /// <remarks>
    /// Boleh kosong karena batch dapat tercatat lebih dahulu daripada dokumen pembeliannya,
    /// misalnya pada saldo pembuka ketika sistem mulai dipakai.
    /// </remarks>
    public Guid? SupplierId { get; set; }

    /// <summary>
    /// Nama principal atau pabrikan, ditulis apa adanya.
    /// </summary>
    /// <remarks>
    /// Sengaja teks bebas dan bukan relasi: master principal belum ada di sistem, dan membuat
    /// master baru hanya demi satu kolom akan menambah ketergantungan yang tidak sebanding.
    /// Bila kelak masternya dibuat, kolom ini yang menjadi bahan pemetaannya.
    /// </remarks>
    [MaxLength(200)]
    public string? PrincipalName { get; set; }

    /// <summary>Keterangan bebas, misalnya nomor sertifikat analisis.</summary>
    [MaxLength(500)]
    public string? Notes { get; set; }

    public MstDrug? Drug { get; set; }
    public MstSupplier? Supplier { get; set; }
    public ICollection<TrxDrugStockBalance> Balances { get; set; } = [];
}
