namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums
{
    /// <summary>
    /// Keadaan <b>dokumen serah terima</b> pada satu catatan kepergian dari IGD.
    /// </summary>
    /// <remarks>
    /// <c>BE-IGD-032</c>, keputusan <c>IGD-DEC-070</c> dan <c>IGD-DEC-090</c>.
    ///
    /// <para>
    /// Rangkaian ini berjalan <b>sendiri</b>: tidak menahan dan tidak ditahan keadaan fisik
    /// pasien. Kombinasi fisik <c>Arrived</c> dengan dokumen <c>Pending</c> adalah keadaan
    /// sah — justru itulah alasan kedua rangkaian dipisah, dan <c>IGD-DEC-106</c> menegaskan
    /// bahwa dokumen yang belum final tidak menahan penutupan kunjungan.
    /// </para>
    ///
    /// <para>
    /// <c>Rejected</c> <b>bukan</b> status terminal: serah terima yang ditolak tetap wajib
    /// dituntaskan (<c>IGD-DEC-062</c>), sehingga ia dapat kembali menjadi <c>Pending</c>
    /// setelah dokumennya diperbaiki.
    /// </para>
    /// </remarks>
    public enum EmergencyHandoverStatus
    {
        /// <summary>Dokumen diajukan unit pengirim.</summary>
        Submitted = 1,

        /// <summary>Dokumen menunggu peninjauan unit penerima.</summary>
        Pending = 2,

        /// <summary>Unit penerima menerima serah terima pasiennya. Final.</summary>
        Accepted = 3,

        /// <summary>
        /// Unit penerima menolak dokumennya, wajib menyebut bagian mana yang kurang.
        /// Dapat kembali menjadi <c>Pending</c> setelah diperbaiki.
        /// </summary>
        Rejected = 4,

        /// <summary>Dokumen dibatalkan, mengikuti pembatalan kepergiannya.</summary>
        Cancelled = 9
    }
}
