namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>
    /// Konfigurasi penutupan otomatis kunjungan kiosk yang tidak dilanjutkan
    /// (<c>BE-EXT-05</c>).
    ///
    /// <c>LAB-DEC-059</c> menetapkan hari layanan berakhir <b>21:00 WIB</b> dan meminta angka
    /// itu ditulis sebagai konfigurasi, bukan sebagai konstanta, supaya dapat diubah tanpa
    /// rilis ulang. Angkanya ditetapkan pemilik modul Laboratorium dan
    /// <b>belum dikonfirmasi</b> terhadap jam operasional resmi rumah sakit; itu sebabnya ia
    /// berupa pengaturan.
    /// </summary>
    public class KioskEncounterClosureOptions
    {
        public bool Enabled { get; set; } = true;

        /// <summary>Jarak antar-pemeriksaan. Penutupan hanya terjadi sekali sehari.</summary>
        public int PollIntervalSeconds { get; set; } = 300;

        /// <summary>Pukul berakhirnya hari layanan dalam WIB. <c>LAB-DEC-059</c>.</summary>
        public int ServiceDayEndHour { get; set; } = 21;

        public int ServiceDayEndMinute { get; set; } = 0;

        /// <summary>
        /// Berapa hari ke belakang ikut disapu. Menjaga hari yang terlewat — aplikasi mati,
        /// listrik padam — tetap tertutup pada hari berikutnya, bukan menggantung selamanya.
        /// </summary>
        public int LookBackDays { get; set; } = 3;

        /// <summary>Banyaknya kunjungan yang diperiksa dalam satu putaran.</summary>
        public int BatchSize { get; set; } = 200;

        /// <summary>
        /// Sebab yang tercatat pada kunjungan. <c>LAB-DEC-054</c> menyebutnya
        /// "tidak dilanjutkan".
        /// </summary>
        public string NoShowReason { get; set; } = "Tidak dilanjutkan sampai hari layanan berakhir.";

        /// <summary>
        /// Pelaku yang distempel pada jejak audit. Dibiarkan kosong berarti
        /// <see cref="System.Guid.Empty"/> — penutupan ini memang tidak dilakukan orang.
        /// </summary>
        public Guid? SystemActorUserId { get; set; }

        public string WorkerInstanceName { get; set; } = "quilvian-kiosk-encounter-closure";
    }
}
