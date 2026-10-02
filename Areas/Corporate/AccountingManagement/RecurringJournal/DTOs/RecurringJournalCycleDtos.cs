namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.RecurringJournal.DTOs
{
    /// <summary>
    /// Hasil satu siklus penjadwal jurnal berulang.
    /// </summary>
    /// <remarks>
    /// Bukan kontrak API — tidak ada endpoint yang mengembalikannya. Bentuk ini ada supaya
    /// penjadwal dapat mencatat apa yang benar-benar terjadi tiap siklus, dan supaya pengujian
    /// dapat memeriksanya tanpa membaca log.
    /// </remarks>
    public class RecurringJournalCycleResult
    {
        public DateTime EvaluatedAt { get; set; }

        /// <summary>Jumlah template aktif yang jatuh tempo dan diperiksa siklus ini.</summary>
        public int Considered { get; set; }

        public List<RecurringJournalRunResponse> Published { get; set; } = new();

        public List<RecurringJournalCycleSkip> Skipped { get; set; } = new();

        /// <summary>
        /// Jumlah yang dilewati <b>karena memang sudah pernah terbit</b>. Dipisahkan dari
        /// <see cref="ProblemCount"/> karena ini keadaan normal: penjadwal berjalan berkali-kali
        /// sehari, dan sebagian besar siklus memang tidak menemukan apa pun yang perlu terbit.
        /// </summary>
        public int AlreadyPublishedCount => Skipped.Count(x => x.AlreadyPublished);

        /// <summary>
        /// Jumlah yang dilewati karena <b>ada yang salah</b> — periode tertutup, akun
        /// dinonaktifkan, unit biaya dipindahkan. Inilah satu-satunya angka yang layak
        /// diperingatkan ke log.
        /// </summary>
        public int ProblemCount => Skipped.Count(x => !x.AlreadyPublished);
    }

    /// <summary>Satu template yang dilewati siklus penjadwal, beserta sebabnya.</summary>
    public class RecurringJournalCycleSkip
    {
        public Guid TemplateId { get; set; }

        public string TemplateCode { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Benar bila sebabnya template memang sudah terbit untuk periode itu. Bukan masalah.
        /// </summary>
        public bool AlreadyPublished { get; set; }
    }
}
