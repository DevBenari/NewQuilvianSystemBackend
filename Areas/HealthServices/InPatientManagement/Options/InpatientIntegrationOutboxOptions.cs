namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Options
{
    /// <summary>
    /// Pengaturan teknis worker outbox Rawat Inap — kontrak <c>integrasi-billing</c> <c>1.1.0</c>
    /// bagian 9.11. Nilainya dapat diubah lewat konfigurasi tanpa rilis kode.
    /// </summary>
    public sealed class InpatientIntegrationOutboxOptions
    {
        public const string SectionName = "InpatientIntegrationOutbox";

        /// <summary>
        /// Masa sewa pemrosesan dalam detik. Pesan <c>Processing</c> yang lebih tua dari ini dianggap
        /// tertinggal (misalnya aplikasi mati di tengah pengiriman) dan diambil ulang.
        /// </summary>
        public int ProcessingLeaseSeconds { get; set; } = 300;

        /// <summary>Jumlah pesan yang diambil per putaran.</summary>
        public int BatchSize { get; set; } = 50;

        /// <summary>Batas percobaan sebelum pesan menjadi <c>DeadLetter</c>.</summary>
        public int MaxRetry { get; set; } = 10;
    }
}
