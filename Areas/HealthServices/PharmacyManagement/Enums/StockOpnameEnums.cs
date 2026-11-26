namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;

/// <summary>
/// Status sesi Stock Opname (FIN-LGS-091).
/// </summary>
/// <remarks>
/// <code>
/// Counting ──ajukan──► Submitted ──setujui (posting sekali)──► Completed ──► berita acara
///    │                     └──tolak (alasan wajib, final)──► Rejected ──► sesi baru
///    └──batalkan──► Cancelled
/// </code>
/// Satu sesi hanya punya satu keputusan final. Hitung ulang sesudah penolakan memakai sesi baru
/// yang merujuk sesi yang ditolak, sehingga setiap keputusan tetap terbaca dari barisnya sendiri.
/// </remarks>
public enum StockOpnameStatus
{
    /// <summary>Sedang dihitung; hasil hitung, temuan, dan baris yang berubah masih dapat diperbarui.</summary>
    Counting = 1,

    /// <summary>Diajukan; seluruh baris dikunci menunggu keputusan.</summary>
    Submitted = 2,

    /// <summary>Disetujui dan seluruh penyesuaian sudah diposting ke kartu stok; berita acara terbit.</summary>
    Completed = 3,

    /// <summary>Ditolak dengan alasan; final.</summary>
    Rejected = 4,

    /// <summary>Dibatalkan sebelum diajukan.</summary>
    Cancelled = 5
}

/// <summary>Jenis baris detail perhitungan.</summary>
public enum StockOpnameLineType
{
    /// <summary>Baris dari saldo sistem pada lokasi itu (batch × status).</summary>
    Balance = 1,

    /// <summary>
    /// Barang atau batch yang ditemukan tetapi tidak punya saldo di lokasi itu. Dicatat sebagai
    /// temuan dan tidak pernah menambah stok secara otomatis (keputusan pemilik FIN-LGS-088).
    /// </summary>
    Finding = 2
}
