using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    /// <summary>
    /// Ringkasan hasil eksekusi langkah-langkah scheduler perencanaan tenaga kerja.
    /// </summary>
    public sealed class SchedulerExecutionSummary
    {
        public DateOnly OperationalDate { get; set; }
        public DateTime ExecutedAtUtc { get; set; } = DateTime.UtcNow;
        public int ActivatedStandardsCount { get; set; }
        public int SupersededStandardsCount { get; set; }
        public List<string> StepLogs { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public bool HasFailures => Errors.Count > 0;
    }

    /// <summary>
    /// Antarmuka layanan eksekusi scheduler rutin perencanaan tenaga kerja (BE-HRD-036).
    /// </summary>
    public interface IHrdWorkforcePlanningSchedulerService
    {
        /// <summary>
        /// Menjalankan seluruh siklus scheduler harian secara berurutan dan idempoten.
        /// Tiap langkah dieksekusi dalam transaksi terpisah agar kegagalan satu langkah tidak menggagalkan langkah lain.
        /// </summary>
        Task<SchedulerExecutionSummary> ExecuteDailyCycleAsync(
            DateOnly? operationalDate = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Langkah 1: Aktivasi standar kebutuhan tenaga kerja berstatus Scheduled yang tanggal berlakunya tiba (EffectiveStartDate &lt;= operationalDate).
        /// Standar aktif lama pada unit yang sama ditandai Superseded dengan EffectiveEndDate = kemarin (kriteria 1-3).
        /// </summary>
        Task<int> StepActivateScheduledStandardsAsync(
            DateOnly operationalDate,
            SchedulerExecutionSummary summary,
            CancellationToken cancellationToken = default);
    }
}
