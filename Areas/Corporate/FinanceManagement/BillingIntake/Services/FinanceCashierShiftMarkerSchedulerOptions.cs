namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.BillingIntake.Services;

/// <summary>
/// Konfigurasi <see cref="FinanceCashierShiftMarkerSchedulerHostedService"/> (BE-FIN-073, FIN-DES-078,
/// FIN-DEC-118). Dibaca lewat IOptions&lt;FinanceCashierShiftMarkerSchedulerOptions&gt;; diisi bagian
/// "Finance:CashierShiftMarkerScheduler" pada appsettings.json atau konfigurasi lingkungan.
///
/// Nilai bawaan: <see cref="Enabled"/> = false — penjadwal TIDAK memicu sinkronisasi apa pun sampai
/// pemilik menyalakannya secara eksplisit (FIN-DEC-118). Menambahkan kelas ini ke kodebase TIDAK
/// mengubah perilaku lingkungan mana pun.
/// </summary>
public sealed class FinanceCashierShiftMarkerSchedulerOptions
{
    /// <summary>
    /// Bila false, penjadwal berhenti setelah satu baris log informatif.
    /// Nilai bawaan: <c>false</c> — dibangun mati (FIN-DES-078).
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Jeda antar siklus sinkronisasi penanda shift dalam detik. Minimum 30 detik.
    /// Nilai bawaan: 60 detik.
    /// </summary>
    public int PollIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// Actor user id yang dicatat sebagai pembuat (<c>CreateBy</c>) kejadian outbox yang diterbitkan
    /// penjadwal ini. Kosong (<c>null</c>) berarti <see cref="Guid.Empty"/> — mengikuti pola
    /// <c>FinanceSubledgerSnapshotSchedulerOptions.SystemActorUserId</c> (BE-FIN-072) untuk
    /// pekerjaan tanpa aktor manusia.
    /// </summary>
    public Guid? SystemActorUserId { get; set; }
}
