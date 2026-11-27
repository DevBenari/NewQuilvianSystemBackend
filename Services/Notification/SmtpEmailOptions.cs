namespace QuilvianSystemBackend.Services.Notification
{
    /// <summary>
    /// Konfigurasi akun SMTP rumah sakit untuk email otomatis (kiosk Amendment PO,
    /// <c>KSK-DEC-042</c>, <c>INT-PO-03</c>).
    ///
    /// <para><c>Password</c> diisi lewat user-secrets atau variabel lingkungan server
    /// (<c>Email__Smtp__Password</c>), tidak pernah ditulis di berkas yang di-commit
    /// (<c>KSK-PO-OQ-015</c>).</para>
    /// </summary>
    public class SmtpEmailOptions
    {
        public const string SectionName = "Email:Smtp";

        /// <summary><c>false</c> = tidak ada email yang dikirim; pemanggil mencatatnya sebagai gagal kirim.</summary>
        public bool Enabled { get; set; }

        public string Host { get; set; } = string.Empty;

        public int Port { get; set; } = 587;

        public bool EnableSsl { get; set; } = true;

        public string UserName { get; set; } = string.Empty;

        /// <summary><b>Rahasia.</b></summary>
        public string Password { get; set; } = string.Empty;

        public string FromAddress { get; set; } = string.Empty;

        public string FromName { get; set; } = string.Empty;

        /// <summary>Batas waktu satu pengiriman. <c>INT-PO-03</c>: 10 detik, satu percobaan.</summary>
        public int TimeoutSeconds { get; set; } = 10;

        public TimeSpan ResolveTimeout() =>
            TimeSpan.FromSeconds(TimeoutSeconds is >= 1 and <= 60 ? TimeoutSeconds : 10);
    }
}
