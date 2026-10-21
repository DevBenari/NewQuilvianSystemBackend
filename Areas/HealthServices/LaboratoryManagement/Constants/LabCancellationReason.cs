namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Constants
{
    /// <summary>
    /// Alasan pembatalan pemeriksaan dan wadah — <c>VAL-155</c> (<c>LAB-VAL-v1</c> <c>r21</c>,
    /// <c>LAB-DEC-237</c>, <c>LAB-DEC-247</c>, <c>INV-60</c>).
    ///
    /// <para>
    /// Setiap pembatalan beralasan, dari pemanggil mana pun — layar, integrasi, maupun alat. Bunyi dan
    /// kode statusnya (<c>422</c>) <b>sama</b> dengan <c>VAL-74</c> pada pembatalan pesanan, supaya ketiga
    /// jalur batal menjawab dengan satu suara. Penjaga ini dipanggil <b>sesudah</b> penjaga keadaan:
    /// permintaan yang memang tidak boleh dibatalkan tidak diminta alasannya lebih dulu.
    /// </para>
    ///
    /// <para>
    /// <b>Tidak berlaku surut.</b> Pembatalan lama yang tersimpan tanpa alasan dibiarkan apa adanya;
    /// Laporan Pembatalan menampilkannya <i>Alasan tidak dicatat</i>.
    /// </para>
    /// </summary>
    public static class LabCancellationReason
    {
        /// <summary>Pesan penolakan — bunyi yang sama dengan <c>VAL-74</c>.</summary>
        public const string MissingMessage = "Alasan pembatalan wajib diisi.";

        /// <summary>
        /// Alasan yang sudah dirapikan, atau <c>null</c> bila kosong atau hanya spasi, tab, dan baris
        /// baru. Pemanggil menolak <c>null</c> dengan pengecualian validasinya sendiri (<c>422</c>).
        /// </summary>
        public static string? Require(string? reason) =>
            string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
    }
}
