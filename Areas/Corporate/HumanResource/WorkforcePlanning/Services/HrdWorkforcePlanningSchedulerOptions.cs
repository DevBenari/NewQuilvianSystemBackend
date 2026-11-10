namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    /// <summary>
    /// Opsi konfigurasi untuk scheduler rutin perencanaan tenaga kerja rumah sakit (BE-HRD-036).
    /// </summary>
    public sealed class HrdWorkforcePlanningSchedulerOptions
    {
        public const string SectionName = "HumanResource:WorkforcePlanningScheduler";

        /// <summary>
        /// Menentukan apakah scheduler aktif berjalan di background. Bawaan true.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Interval pengecekan scheduler dalam detik. Bawaan 300 detik (5 menit).
        /// </summary>
        public int PollIntervalSeconds { get; set; } = 300;

        /// <summary>
        /// Nama identifikasi instance worker scheduler.
        /// </summary>
        public string? WorkerInstanceName { get; set; } = "quilvian-wfp-scheduler";
    }
}
