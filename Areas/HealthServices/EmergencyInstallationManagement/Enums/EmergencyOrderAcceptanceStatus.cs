namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums
{
    /// <summary>
    /// Penerimaan <b>per pesanan</b> oleh unit penerima.
    /// </summary>
    /// <remarks>
    /// <c>IGD-DEC-102</c>. Sengaja terpisah dari <c>EmergencyHandoverStatus</c>: penerimaan
    /// pasien, dokumen serah terima, dan tiap pesanan adalah tiga fakta yang berbeda.
    /// Menumpangkannya berarti satu pesanan yang ditolak akan menggagalkan penerimaan
    /// pasien — akibat yang secara tegas dilarang butir (d).
    /// </remarks>
    public enum EmergencyOrderAcceptanceStatus
    {
        /// <summary>
        /// Tidak ada yang perlu diterima. Nilai awal untuk sikap <c>Continue</c> dan
        /// <c>Cancel</c>, karena keduanya tidak melibatkan unit penerima.
        /// </summary>
        NotRequired = 1,

        /// <summary>Menunggu penerimaan unit penerima. Nilai awal untuk sikap <c>Handover</c>.</summary>
        Pending = 2,

        /// <summary>Unit penerima menerima pesanan ini. Final pada barisnya.</summary>
        Accepted = 3,

        /// <summary>
        /// Unit penerima menolak pesanan ini; wajib beralasan. Final pada barisnya — sikap
        /// penggantinya ditulis sebagai <b>baris baru</b> yang menunjuk baris ini.
        /// </summary>
        Rejected = 4
    }
}
