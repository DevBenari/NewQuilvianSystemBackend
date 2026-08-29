namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums
{
    /// <summary>
    /// Keadaan <b>fisik pasien</b> pada satu catatan kepergian dari IGD.
    /// </summary>
    /// <remarks>
    /// <c>BE-IGD-032</c>, keputusan <c>IGD-DEC-070</c> yang diperluas <c>IGD-DEC-090</c>.
    /// Salah satu dari dua rangkaian yang menggantikan <c>EmergencyTransferStatus</c>.
    /// Rangkaian ini menjawab satu pertanyaan saja: pasiennya sudah berangkat dan tiba?
    ///
    /// <para>
    /// Nilai terminal diberi angka <c>9</c> supaya penambahan nilai antara kelak tidak
    /// menggesernya.
    /// </para>
    /// </remarks>
    public enum EmergencyPhysicalStatus
    {
        /// <summary>Kepergian disiapkan; pasien masih di IGD.</summary>
        Prepared = 1,

        /// <summary>Pasien sudah berangkat dari IGD. Pemilik klinisnya <b>masih</b> IGD.</summary>
        Departed = 2,

        /// <summary>
        /// Pasien sudah tiba di unit tujuan; pemilik klinisnya berpindah ke unit penerima.
        /// Bersifat final: koreksinya lewat kejadian <c>Amended</c> atau <c>Reversed</c>,
        /// bukan lewat transisi.
        /// </summary>
        Arrived = 3,

        /// <summary>Kepergian dibatalkan. Membatalkan dokumen serah terimanya sekaligus.</summary>
        Cancelled = 9
    }
}
