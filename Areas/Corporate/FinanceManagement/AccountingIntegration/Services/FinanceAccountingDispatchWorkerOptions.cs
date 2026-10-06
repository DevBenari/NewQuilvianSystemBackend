namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;

/// <summary>
/// Konfigurasi <see cref="FinanceAccountingDispatchWorker"/> (BE-FIN-071, FIN-DES-078).
/// Dibaca lewat IOptions&lt;FinanceAccountingDispatchWorkerOptions&gt;; diisi bagian
/// "Finance:AccountingDispatch" pada appsettings.json atau konfigurasi lingkungan.
///
/// Nilai bawaan: <see cref="Enabled"/> = false — worker TIDAK mengirim apa pun dan TIDAK
/// membuat thread tambahan sampai pemilik menyalakannya secara eksplisit (FIN-DEC-118).
/// Menambahkan kelas ini ke kodebase TIDAK mengubah perilaku lingkungan mana pun.
/// </summary>
public sealed class FinanceAccountingDispatchWorkerOptions
{
    /// <summary>
    /// Bila false, worker berhenti setelah satu baris log informatif.
    /// Nilai bawaan: <c>false</c> — dibangun mati (FIN-DES-078).
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Jeda antar siklus pengiriman dalam detik. Minimum 10 detik.
    /// Nilai bawaan: 30 detik.
    /// </summary>
    public int PollIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Jumlah maksimum baris yang diproses per siklus. Membatasi transaksi per siklus
    /// sehingga satu siklus tidak memblokir terlalu lama. Nilai bawaan: 50.
    /// </summary>
    public int BatchSize { get; set; } = 50;

    /// <summary>
    /// Jumlah maksimum percobaan kirim sebelum baris ditandai FAILED.
    /// Nilai bawaan: 5 kali.
    /// </summary>
    public int MaxAttempts { get; set; } = 5;

    /// <summary>
    /// URL kotak masuk Accounting (endpoint penerima kejadian).
    /// MUST diisi dari konfigurasi — MUST NOT ditanamkan di source (FIN-DES-078, G3).
    /// </summary>
    public string AccountingInboxUrl { get; set; } = string.Empty;

    /// <summary>
    /// Bearer token atau API key kotak masuk Accounting.
    /// MUST diisi dari konfigurasi — MUST NOT ditanamkan di source (FIN-DES-078, G3).
    /// </summary>
    public string AccountingInboxApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Batas waktu HTTP per permintaan dalam detik. Nilai bawaan: 30 detik.
    /// </summary>
    public int HttpTimeoutSeconds { get; set; } = 30;
}
