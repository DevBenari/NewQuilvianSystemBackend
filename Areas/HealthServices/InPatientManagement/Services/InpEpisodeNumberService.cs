namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Membentuk nomor episode Rawat Inap yang unik dan terbaca manusia, misalnya
    /// <c>RI-260824153012-A1B2C3</c>. Tidak menggunakan interface, mengikuti pola project.
    /// </summary>
    /// <remarks>
    /// Bentuk nomornya mengikuti <c>EmergencyDocumentNumberService</c> yang sudah ada:
    /// awalan, waktu pembuatan sampai detik, lalu enam huruf/angka acak.
    ///
    /// Dua sifat yang menentukan bentuk ini dipilih:
    ///
    /// 1. Awalan dibaca dari <c>MstInpatientSetting.EpisodeNumberPrefix</c>, tidak pernah
    ///    ditanam di kode. Rumah sakit yang memakai awalan selain <c>RI</c> cukup mengubah
    ///    satu baris master, tanpa aplikasi dibangun ulang.
    /// 2. Nomor TIDAK dibentuk dari hitungan baris. Cara seperti <c>Count + 1</c> atau
    ///    <c>Max + 1</c> dilarang QBE-CODE-003, karena dua petugas yang menekan Simpan pada
    ///    saat hampir bersamaan akan membaca angka yang sama lalu menghasilkan nomor kembar.
    ///    Enam huruf/angka acak dari Guid membuat dua permintaan bersamaan tetap berbeda,
    ///    dan index unik <c>IX_InpEpisode_EpisodeNumber</c> menjadi penjaga terakhirnya.
    ///
    /// <para>
    /// <b>Contoh.</b> Dua petugas admisi menekan Simpan pada detik yang sama, 24 Agustus 2026
    /// pukul 15:30:12. Keduanya mendapat bagian waktu yang sama persis, yaitu
    /// <c>260824153012</c>, tetapi bagian acaknya berbeda — misalnya <c>A1B2C3</c> dan
    /// <c>7F9E20</c> — sehingga nomor akhirnya tetap dua nomor yang berbeda.
    /// </para>
    /// </remarks>
    public class InpEpisodeNumberService
    {
        private readonly InpSettingService _settingService;

        public InpEpisodeNumberService(InpSettingService settingService)
        {
            _settingService = settingService;
        }

        /// <summary>
        /// Membentuk satu nomor episode baru memakai awalan yang berlaku pada master
        /// pengaturan. Ikut transaksi pemanggilnya; service ini tidak membuka transaksi
        /// sendiri dan tidak menulis apa pun ke database.
        /// </summary>
        public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
        {
            var prefix = await _settingService.GetEpisodeNumberPrefixAsync(cancellationToken);

            return Generate(prefix, DateTime.UtcNow);
        }

        /// <summary>
        /// Membentuk nomor dari awalan dan waktu yang diberikan. Dipisahkan supaya bentuk
        /// nomornya dapat diuji tanpa database.
        /// </summary>
        public string Generate(string? prefix, DateTime now)
        {
            var normalizedPrefix = string.IsNullOrWhiteSpace(prefix)
                ? InpatientSettingValues.Defaults.EpisodeNumberPrefix
                : prefix.Trim().ToUpperInvariant();

            return $"{normalizedPrefix}-{now:yyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
        }
    }
}
