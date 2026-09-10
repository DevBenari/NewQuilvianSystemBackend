namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.RecurringJournal.Services
{
    /// <summary>
    /// Pengaturan penjadwal jurnal berulang, dibaca dari bagian
    /// <c>Accounting:RecurringJournalScheduler</c>.
    /// </summary>
    /// <remarks>
    /// Meniru <c>LeaveAccrualSchedulerOptions</c> yang sudah ada, termasuk tombol
    /// <see cref="Enabled"/>-nya. Yang penting dari tombol itu bukan kenyamanan: penjadwal yang
    /// tidak dapat dimatikan berarti sebuah pemasangan yang datanya belum siap akan menerbitkan
    /// jurnal ke buku besar sungguhan sejak hari pertama.
    /// </remarks>
    public class AccRecurringJournalSchedulerOptions
    {
        /// <summary>
        /// <b>Bawaannya mati.</b> Berbeda dari penjadwal cuti yang bawaannya hidup, penjadwal ini
        /// menulis ke buku besar. Menyalakannya adalah keputusan sadar pemilik proses akuntansi,
        /// bukan akibat sampingan dari memasang aplikasinya.
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>Jeda antar-siklus. Dibatasi minimum 60 detik oleh hosted service.</summary>
        public int PollIntervalSeconds { get; set; } = 3600;

        public string TimeZoneId { get; set; } = "Asia/Jakarta";

        /// <summary>
        /// Jam lokal paling awal penjadwal boleh menerbitkan. Bawaannya pagi hari, sebelum jam
        /// kerja, supaya draft-nya sudah menunggu saat petugas membuka layar.
        /// </summary>
        public int DailyRunHour { get; set; } = 2;

        public int DailyRunMinute { get; set; } = 0;

        /// <summary>
        /// Pelaku yang tercatat sebagai pembuat jurnal terbitan penjadwal. Kosong berarti
        /// <c>Guid.Empty</c>, dan jurnalnya tetap terbit — <c>CreateBy</c> kosong pada jurnal
        /// otomatis adalah keadaan yang sah, sekaligus penanda bahwa tidak ada manusia yang
        /// menyusunnya.
        /// </summary>
        public Guid? SystemActorUserId { get; set; }

        public string WorkerInstanceName { get; set; } = "quilvian-recurring-journal-scheduler";
    }
}
