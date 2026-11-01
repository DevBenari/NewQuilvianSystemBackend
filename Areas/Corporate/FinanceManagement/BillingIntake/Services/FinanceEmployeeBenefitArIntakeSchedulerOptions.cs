namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.BillingIntake.Services;

/// <summary>
/// Pengaturan penjadwal intake otomatis AR Karyawan (EMPLOYEE_BENEFIT). Dibaca dari bagian
/// <c>Finance:EmployeeBenefitArIntakeScheduler</c>. Dibangun MATI: tanpa konfigurasi eksplisit penjadwal tidak
/// memicu apa pun dan perilaku lingkungan mana pun tidak berubah.
/// </summary>
public sealed class FinanceEmployeeBenefitArIntakeSchedulerOptions
{
    /// <summary>Bila false, penjadwal berhenti setelah satu baris log. Nilai bawaan: <c>false</c>.</summary>
    public bool Enabled { get; set; } = false;

    /// <summary>Jeda antar siklus dalam detik. Minimum 30. Nilai bawaan: 300 (5 menit).</summary>
    public int PollIntervalSeconds { get; set; } = 300;

    /// <summary>
    /// Masa tunggu sejak handoff AR dibuat Billing sebelum Finance mengakui piutangnya, dalam menit. Memberi ruang bagi
    /// koreksi sesudah finalisasi (mis. salah orang) agar tidak langsung menghasilkan piutang yang harus dibatalkan lagi.
    /// Keputusan bisnis pemilik; 0 berarti tanpa masa tunggu. Nilai bawaan: 15.
    /// </summary>
    public int GraceMinutes { get; set; } = 15;

    /// <summary>Batas fakta yang ditemukan dan diolah per siklus (1..500). Nilai bawaan: 50.</summary>
    public int MaxPerCycle { get; set; } = 50;

    /// <summary>
    /// Aktor yang dicatat pada piutang, baris intake, dan jejak audit hasil penjadwal ini. Kosong berarti
    /// <see cref="Guid.Empty"/> — pola yang sama dengan penjadwal Finance lain untuk pekerjaan tanpa aktor manusia.
    /// Disarankan diisi dengan akun layanan khusus agar jejak audit dapat dibedakan dari petugas.
    /// </summary>
    public Guid? SystemActorUserId { get; set; }
}
