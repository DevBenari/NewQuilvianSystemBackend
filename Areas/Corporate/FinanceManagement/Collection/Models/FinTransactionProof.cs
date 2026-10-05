using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Models;

/// <summary>
/// Metadata bukti pembayaran langsung (BE-FIN-074, FIN-DES-087, FIN-DEC-135). Menyimpan keterangan
/// berkas saja — jenis, nama asli, nama tersimpan, jalur relatif, tipe media, ukuran, dan pengunggah.
/// Berkasnya sendiri berada di luar database, mengikuti pola `FileStorage:UploadRootPath` yang sudah
/// berjalan (<c>Program.cs</c>), bukan kelas unggah milik HR (<c>WorkflowFileStorageService</c>).
///
/// Dipakai sebagai <c>ProofId</c> pada <c>FinReceivableMovement</c>/<c>FinSupplierPayableMovement</c>
/// (FIN-DES-079) — satu bukti hanya dapat dipakai tepat satu baris mutasi, dijaga unique index pada
/// <c>ProofId</c> di kedua tabel mutasi itu (sudah ada sejak BE-FIN-058). Constraint foreign key dari
/// kedua tabel mutasi itu ke tabel ini SENGAJA TIDAK dibuat pada task ini — lihat laporan task
/// BE-FIN-074 bagian 7 untuk alasan dan dampaknya.
///
/// Nol endpoint PUT/DELETE pada rumpun bukti (FIN-DEC-139) — penggantian dan penghapusan baris
/// berkas bukti tidak pernah dibangun; satu-satunya penghapusan yang dijinkan adalah pembersihan
/// berkas yatim ketika penulisan metadata gagal (FIN-DES-092), bukan di sini.
/// </summary>
[Table("FinTransactionProof", Schema = "public")]
public sealed class FinTransactionProof : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Misalnya BUKTI-TRANSFER, KUITANSI.</summary>
    [Required, MaxLength(30)]
    public string ProofType { get; set; } = string.Empty;

    /// <summary>Nama berkas dari pengguna.</summary>
    [Required, MaxLength(260)]
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>Nama hasil penormalan, mencegah tabrakan nama berkas di penyimpanan.</summary>
    [Required, MaxLength(260)]
    public string StoredFileName { get; set; } = string.Empty;

    /// <summary>Relatif terhadap FileStorage:UploadRootPath; MUST divalidasi berada di bawah akarnya.</summary>
    [Required, MaxLength(500)]
    public string RelativePath { get; set; } = string.Empty;

    /// <summary>
    /// application/pdf, image/jpeg, image/png — daftarnya dari
    /// FinanceManagement:TransactionProof:AllowedExtensions. Tipe media MUST diperiksa, bukan hanya ekstensinya (FIN-DES-092).
    /// </summary>
    [Required, MaxLength(100)]
    public string MediaType { get; set; } = string.Empty;

    /// <summary>Batas dari FinanceManagement:TransactionProof:MaxFileSizeBytes (FIN-OQ-082, belum ditetapkan).</summary>
    public long SizeBytes { get; set; }

    public Guid UploadedBy { get; set; }

    public DateTimeOffset UploadedAt { get; set; }
}

/// <summary>Daftar tertutup ProofType yang sudah dikenal. Daftar ini MUST diperluas, bukan ditebak bebas pengguna.</summary>
public static class FinTransactionProofTypes
{
    public const string BuktiTransfer = "BUKTI-TRANSFER";
    public const string Kuitansi = "KUITANSI";
}
