using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Enums
{
    /// <summary>
    /// Apakah sebuah butir daftar periksa penutupan benar-benar dapat dihitung sekarang.
    ///
    /// <para>
    /// Ini <b>bukan</b> hasil pemeriksaannya, melainkan keterangan apakah pemeriksaannya sempat
    /// dijalankan. Dua hal yang berbeda: butir bernilai nol berarti "sudah diperiksa, tidak ada
    /// masalah", sedangkan <see cref="NotYetAvailable"/> berarti "belum dapat diperiksa sama
    /// sekali".
    /// </para>
    ///
    /// <para>
    /// Perbedaan itu penting karena beberapa penghalang <c>ACC-DEC-051</c> dan
    /// <c>ACC-DEC-065</c> bergantung pada kotak masuk kejadian keuangan yang baru berdiri pada
    /// gelombang <c>P2-1</c>. Menampilkan keduanya sama-sama sebagai angka nol akan membuat
    /// pengguna mengira periode sudah bersih, padahal separuh pemeriksaannya belum berjalan.
    /// </para>
    /// </summary>
    public enum PeriodChecklistItemState
    {
        /// <summary>
        /// Sudah diperiksa. Angkanya dapat dipercaya.
        /// </summary>
        [Display(Name = "Sudah Diperiksa")]
        Evaluated = 1,

        /// <summary>
        /// Belum dapat diperiksa karena sumber datanya belum berdiri. Angkanya selalu nol dan
        /// <b>tidak boleh</b> dibaca sebagai "aman".
        /// </summary>
        [Display(Name = "Belum Dapat Diperiksa")]
        NotYetAvailable = 2
    }
}
