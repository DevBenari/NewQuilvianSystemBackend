namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;

/// <summary>
/// Daur hidup satu permintaan stok barang atau obat.
/// </summary>
/// <remarks>
/// <para>
/// <c>Draft</c> adalah satu-satunya keadaan yang isinya masih boleh diubah. Begitu
/// permintaan dikirim, isinya terkunci.
/// </para>
/// <para>
/// Sejak <c>LGS-DEC-002</c> (17 November 2026) permintaan melewati satu tingkat persetujuan
/// atasan unit sebelum gudang boleh menyerahkan. Nilai numerik tidak diubah; <see cref="Approved"/>
/// dan <see cref="Rejected"/> yang sempat tidak dipakai kini kembali berarti keputusan atasan.
/// Pemetaan ke istilah bisnis:
/// </para>
/// <list type="bullet">
/// <item><c>Draft</c> = Draft.</item>
/// <item><c>Submitted</c> = Menunggu Persetujuan.</item>
/// <item><c>Approved</c> = Disetujui (oleh atasan unit atau otorisasi CITO) — antrean gudang.</item>
/// <item><c>Rejected</c> = Ditolak; tidak dapat diproses lagi.</item>
/// <item><c>Completed</c> = Selesai; bila ada baris yang diserahkan kurang dari diminta, tanggapan
/// API menandainya <c>IsPartiallyFulfilled</c> (Dipenuhi Sebagian). Penyerahan sebagian tetap
/// menutup permintaan sampai <c>LGS-OQ-003</c>/<c>LGS-OQ-005</c> diputuskan.</item>
/// <item><c>Cancelled</c> = Dibatalkan.</item>
/// </list>
/// <para>
/// "Diproses Gudang" belum menjadi keadaan tersendiri: penyerahan saat ini satu langkah atomik
/// dari <c>Approved</c> ke <c>Completed</c>.
/// </para>
/// </remarks>
public enum StockRequestStatus
{
    /// <summary>Masih disusun peminta; boleh diubah dan dihapus barisnya.</summary>
    Draft = 1,

    /// <summary>Menunggu persetujuan atasan unit (atau otorisasi CITO); isinya tidak boleh diubah lagi.</summary>
    Submitted = 2,

    /// <summary>
    /// Disetujui atasan unit, atau diotorisasi petugas berwenang lewat jalur CITO. Hanya keadaan
    /// ini yang boleh diserahkan gudang. Riwayat lama yang menyebut nilai ini tetap terbaca.
    /// </summary>
    Approved = 3,

    /// <summary>Barang sudah diserahkan; permintaan ditutup.</summary>
    Completed = 4,

    /// <summary>
    /// Ditolak atasan unit dengan alasan wajib. Tidak dapat diserahkan, diubah, maupun dikirim
    /// ulang; bila masih dibutuhkan, unit membuat permintaan baru.
    /// </summary>
    Rejected = 5,

    /// <summary>Dibatalkan peminta sebelum diserahkan.</summary>
    Cancelled = 6
}

/// <summary>Kesegeraan permintaan, menentukan urutan pengerjaan di gudang.</summary>
public enum StockRequestPriority
{
    Routine = 1,

    /// <summary>Dibutuhkan lebih cepat dari antrean biasa.</summary>
    Urgent = 2,

    /// <summary>Untuk keadaan gawat darurat; menyalip seluruh antrean.</summary>
    Emergency = 3
}

/// <summary>
/// Hasil verifikasi administratif susulan atas permintaan CITO yang diotorisasi lewat jalur
/// darurat (<c>LGS-DEC-005</c>).
/// </summary>
/// <remarks>
/// Verifikasi tidak mengubah status permintaan dan tidak menyentuh stok. Hasilnya disimpan
/// sebagai baris riwayat dengan nama aksi tersendiri, sehingga tidak memerlukan tabel baru.
/// </remarks>
public enum StockRequestEmergencyReviewOutcome
{
    /// <summary>Atasan unit membenarkan otorisasi CITO.</summary>
    Confirmed = 1,

    /// <summary>
    /// Atasan unit mencatat temuan penyimpangan. Transaksi fisik yang sudah terjadi tidak dihapus
    /// atau dibalik; temuan menjadi catatan audit dan bahan tindak lanjut. Alasan wajib.
    /// </summary>
    Deviation = 2
}

/// <summary>
/// Keadaan verifikasi susulan CITO, dihitung dari riwayat — bukan nilai status baru.
/// </summary>
public enum StockRequestEmergencyReviewState
{
    /// <summary>Tidak diotorisasi lewat jalur CITO, sehingga tidak memerlukan verifikasi susulan.</summary>
    NotRequired = 0,

    /// <summary>Sudah diotorisasi lewat jalur CITO, belum diverifikasi atasan unit.</summary>
    Pending = 1,

    /// <summary>Diverifikasi; otorisasi CITO dibenarkan.</summary>
    Confirmed = 2,

    /// <summary>Diverifikasi; tercatat temuan penyimpangan.</summary>
    Deviation = 3
}
