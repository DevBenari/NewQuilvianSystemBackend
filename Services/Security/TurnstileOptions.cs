namespace QuilvianSystemBackend.Services.Security
{
    /// <summary>
    /// Konfigurasi pemeriksa captcha Cloudflare Turnstile untuk halaman publik Pendaftaran
    /// Pasien Online (kiosk Amendment PO, <c>KSK-DEC-044</c>, <c>INT-PO-01</c>).
    ///
    /// <para><c>SiteKey</c> dan terutama <c>SecretKey</c> diisi lewat user-secrets atau variabel
    /// lingkungan server (<c>Turnstile__SecretKey</c>), tidak pernah ditulis di berkas yang
    /// di-commit (<c>KSK-PO-OQ-016</c>).</para>
    /// </summary>
    public class TurnstileOptions
    {
        public const string SectionName = "Turnstile";

        /// <summary>
        /// <c>false</c> hanya untuk lingkungan pengembangan: setiap token dianggap lolos dan
        /// aplikasi mencatat peringatan saat menyala.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>Kunci publik widget. Bukan rahasia; dibaca frontend lewat variabel lingkungannya sendiri.</summary>
        public string SiteKey { get; set; } = string.Empty;

        /// <summary>Kunci rahasia untuk verifikasi server. <b>Rahasia.</b></summary>
        public string SecretKey { get; set; } = string.Empty;

        public string VerifyUrl { get; set; } = "https://challenges.cloudflare.com/turnstile/v0/siteverify";

        /// <summary>Batas waktu satu verifikasi. <c>INT-PO-01</c>: 5 detik, tanpa retry.</summary>
        public int TimeoutSeconds { get; set; } = 5;

        /// <summary>
        /// Perilaku saat Cloudflare tidak terjangkau. Bawaan <c>false</c> = permintaan ditolak
        /// (<c>GATE-PO-07</c>, <c>KSK-DSN-PO-006</c>).
        /// </summary>
        public bool FailOpen { get; set; }

        public TimeSpan ResolveTimeout() =>
            TimeSpan.FromSeconds(TimeoutSeconds is >= 1 and <= 30 ? TimeoutSeconds : 5);
    }
}
