namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;

/// <summary>
/// Konfigurasi <see cref="FinanceSubledgerSnapshotSchedulerHostedService"/> (BE-FIN-072, FIN-DES-078,
/// FIN-DEC-092, FIN-DEC-114). Dibaca lewat IOptions&lt;FinanceSubledgerSnapshotSchedulerOptions&gt;;
/// diisi bagian "Finance:SubledgerSnapshotScheduler" pada appsettings.json atau konfigurasi lingkungan.
///
/// Nilai bawaan: <see cref="Enabled"/> = false — penjadwal TIDAK memicu snapshot apa pun sampai
/// pemilik menyalakannya secara eksplisit (FIN-DEC-118). Menambahkan kelas ini ke kodebase TIDAK
/// mengubah perilaku lingkungan mana pun.
/// </summary>
public sealed class FinanceSubledgerSnapshotSchedulerOptions
{
    /// <summary>
    /// Bila false, penjadwal berhenti setelah satu baris log informatif.
    /// Nilai bawaan: <c>false</c> — dibangun mati (FIN-DES-078).
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Jeda pemeriksaan "apakah sudah waktunya jalan hari ini" dalam detik. Minimum 30 detik.
    /// Nilai bawaan: 60 detik. Ini bukan jeda antar siklus bisnis — siklus bisnisnya sendiri
    /// hanya berjalan sekali per hari WIB pada jam yang ditentukan <see cref="DailyRunHourWib"/>/<see cref="DailyRunMinuteWib"/>.
    /// </summary>
    public int PollIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// Jam (0-23) waktu WIB penjadwal dijalankan setiap hari. Nilai bawaan: 0 (FIN-DEC-092: pukul 00.05 WIB).
    /// </summary>
    public int DailyRunHourWib { get; set; } = 0;

    /// <summary>
    /// Menit (0-59) waktu WIB penjadwal dijalankan setiap hari. Nilai bawaan: 5 (FIN-DEC-092: pukul 00.05 WIB).
    /// </summary>
    public int DailyRunMinuteWib { get; set; } = 5;

    /// <summary>
    /// Actor user id yang dicatat sebagai pembuat (<c>CreateBy</c>) kejadian outbox yang diterbitkan
    /// penjadwal ini. Kosong (<c>null</c>) berarti <see cref="Guid.Empty"/> — mengikuti pola
    /// <c>LeaveCarryForwardSchedulerOptions.SystemActorUserId</c> untuk pekerjaan tanpa aktor manusia.
    /// </summary>
    public Guid? SystemActorUserId { get; set; }
}
