namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums
{
    /// <summary>
    /// Penanda satu peringatan penutupan episode. Lahir <c>BE-RWI-084</c> menyerap
    /// <c>FR-RI-201</c> dan <c>VAL-INP-13</c> sampai <c>VAL-INP-17</c>.
    /// </summary>
    /// <remarks>
    /// <b>Peringatan, bukan penghalang — dan perbedaan itu adalah inti seluruh enum ini.</b>
    /// Petugas admisi yang menekan tombol tutup bukan orang klinis, dan sering tidak tahu masih
    /// ada konsep catatan dokter yang belum ditandatangani. Sistem karena itu memberi tahu lebih
    /// dulu. Yang <b>tidak</b> dilakukan sistem adalah menahan penutupannya: pasien yang sudah
    /// pulang tetapi episodenya tetap terbuka adalah keadaan yang jauh lebih berbahaya daripada
    /// satu konsep yang terkunci tanpa tanda tangan — <c>RWI-DEC-129</c> (4), <c>RWI-DEC-138</c>
    /// (5), <c>RWI-DEC-143</c> (5).
    ///
    /// <para>
    /// <b>Tidak dipersistensi.</b> Nilai-nilai ini hanya hidup di dalam balasan endpoint
    /// kesiapan penutupan. Tidak ada kolom database yang menyimpannya, sehingga penambahan
    /// nilai baru tidak menuntut migration.
    /// </para>
    /// </remarks>
    public enum ClosureWarningCode
    {
        /// <summary>
        /// Konsep catatan dokter yang akan terkunci menjadi "Tidak Ditandatangani" begitu
        /// episode ditutup — <c>INT-INP-08</c>, langkah 4 penutupan.
        /// </summary>
        UnsignedDoctorDrafts = 1,

        /// <summary>
        /// Pesanan tindakan tertunda yang <b>belum ditagih</b> dan akan dibatalkan begitu
        /// episode ditutup — <c>INT-INP-09</c>, langkah 5 penutupan.
        /// </summary>
        PendingProcedureOrders = 2,

        /// <summary>
        /// Pesanan tindakan tertunda yang <b>sudah ditagih</b> dan karena itu <b>tidak</b>
        /// dibatalkan. Ia perlu ditindaklanjuti bersama Billing, dan muncul pada daftar pantau
        /// <c>GET /monitoring/billed-pending-procedure-orders</c> — <c>RWI-DEC-143</c> jalur
        /// tidak normal (c).
        /// </summary>
        BilledPendingProcedureOrders = 3,

        /// <summary>
        /// Dosis obat yang jadwalnya sudah lewat tetapi pemberiannya belum dicatat perawat —
        /// <c>INT-KEP-15</c>. Berbeda dari dosis masa depan yang dibatalkan langkah 6: yang ini
        /// adalah pencatatan yang tertinggal, bukan dosis yang tidak akan diberikan.
        /// </summary>
        UnrecordedPastDoses = 4
    }
}
