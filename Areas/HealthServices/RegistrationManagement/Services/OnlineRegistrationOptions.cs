namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>
    /// Konfigurasi Pendaftaran Pasien Online, section
    /// <c>HealthServices:Registration:OnlineRegistration</c> (kiosk Amendment PO,
    /// <c>02-backend-architecture.md</c> PO.11). Semua angka dapat diubah tanpa rilis ulang
    /// (<c>KSK-DEC-038</c>).
    /// </summary>
    public class OnlineRegistrationOptions
    {
        public const string SectionName = "HealthServices:Registration:OnlineRegistration";

        public bool Enabled { get; set; } = true;

        /// <summary>Tanggal kunjungan paling cepat H+1 (<c>KSK-DEC-047</c>).</summary>
        public int MinDaysAhead { get; set; } = 1;

        /// <summary>Tanggal kunjungan paling jauh H+30 (<c>KSK-DEC-038</c>).</summary>
        public int MaxDaysAhead { get; set; } = 30;

        /// <summary>Pengiriman pendaftaran per jam per IP (<c>KSK-DEC-038</c>).</summary>
        public int RegistrationPermitPerHour { get; set; } = 10;

        /// <summary>Percobaan pencocokan pasien per jam per IP (<c>KSK-DEC-038</c>).</summary>
        public int MatchPermitPerHour { get; set; } = 20;

        /// <summary>Permintaan opsi publik (poli, jadwal, penyedia, faskes) per menit per IP.</summary>
        public int OptionsPermitPerMinute { get; set; } = 60;

        /// <summary>Verifikasi check-in per menit per akun kiosk.</summary>
        public int CheckInPermitPerMinute { get; set; } = 10;

        /// <summary>Masa berlaku token check-in (<c>KSK-DSN-PO-008</c>).</summary>
        public int CheckInTokenMinutes { get; set; } = 15;

        /// <summary>Jumlah berkas surat rujukan per pendaftaran (<c>KSK-DSN-PO-011</c>).</summary>
        public int MaxReferralDocuments { get; set; } = 3;

        /// <summary>Ukuran gambar tanda tangan consent dalam KB (<c>KSK-VAL-PO-032</c>).</summary>
        public int MaxSignatureKb { get; set; } = 300;

        /// <summary>Alamat dasar halaman publik, dipakai di isi email dan pesan WhatsApp.</summary>
        public string PublicPageBaseUrl { get; set; } = "http://localhost:3000/online-registration";

        public OnlineRegistrationClosureOptions Closure { get; set; } = new();
    }
}
