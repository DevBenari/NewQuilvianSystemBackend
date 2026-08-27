namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums
{
    /// <summary>
    /// Sikap atas satu pesanan yang belum selesai ketika pasien pergi dari IGD.
    /// </summary>
    /// <remarks>
    /// <c>IGD-DEC-100</c>. Daftar sikap <b>hanya memuat pesanan yang belum selesai</b>,
    /// sehingga tidak ada nilai untuk pesanan yang sudah tuntas — pesanan seperti itu tidak
    /// pernah muncul di sana.
    /// </remarks>
    public enum EmergencyOrderAction
    {
        /// <summary>
        /// Pesanan sengaja dibiarkan berjalan sampai hasil final meski pasien sudah pergi.
        /// Tidak menahan penutupan kunjungan — validation bagian 5.1.
        /// </summary>
        Continue = 1,

        /// <summary>
        /// Pesanan diserahkan ke unit penerima, dan menunggu <b>penerimaan eksplisit</b>
        /// per pesanan — <c>IGD-DEC-102</c>.
        /// </summary>
        Handover = 2,

        /// <summary>
        /// Pesanan dibatalkan. Wajib beralasan, hanya untuk pesanan yang belum dimulai, dan
        /// hanya oleh klinisi berwenang — <c>IGD-DEC-100</c> butir (a) dan (c).
        /// </summary>
        Cancel = 9
    }
}
