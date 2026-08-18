namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services
{
    /// <summary>
    /// Pengaturan pemantau pelampauan target respons triage IGD.
    /// Frekuensi pemindaian belum ditetapkan pemilik proses, sehingga nilai di sini adalah
    /// nilai bawaan yang wajar dan wajib dapat diubah lewat konfigurasi tanpa mengubah kode.
    /// </summary>
    public class EmergencyTriageSlaMonitorOptions
    {
        /// <summary>
        /// Saklar mematikan pemantau tanpa deploy ulang. Pemantau hanya menandai keterlambatan
        /// dan tidak pernah menghalangi pelayanan, sehingga aman dimatikan kapan pun.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Jeda antar pemindaian. Nilai bawaan 60 detik dipilih karena target respons triage
        /// dihitung dalam satuan menit, sehingga memindai lebih sering dari itu tidak menambah
        /// ketelitian tetapi menambah beban basis data.
        /// </summary>
        public int PollIntervalSeconds { get; set; } = 60;

        /// <summary>
        /// Batas jumlah penilaian yang ditandai dalam satu pemindaian. Membatasi ukuran
        /// transaksi supaya lonjakan pasien tidak menghasilkan satu penulisan raksasa.
        /// Sisanya diproses pada pemindaian berikutnya.
        /// </summary>
        public int BatchSize { get; set; } = 500;
    }
}
