namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// Membentuk nomor bisnis kejadian visite dokter, misalnya
    /// <c>VST-260903074012-A1B2C3</c>. Tidak memakai interface, mengikuti pola project.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Nomor tidak dibentuk dari hitungan baris.</b> Cara seperti <c>Count + 1</c> atau
    /// <c>Max + 1</c> dilarang <c>QBE-CODE-003</c>: dua dokter yang menekan Simpan pada saat
    /// hampir bersamaan akan membaca angka yang sama lalu menghasilkan nomor kembar. Enam
    /// huruf/angka acak dari Guid membuat dua permintaan bersamaan tetap berbeda, dan index unik
    /// <c>IX_CliPhysicianVisit_PhysicianVisitNumber</c> menjadi penjaga terakhirnya
    /// (<c>QBE-CODE-004</c>).
    /// </para>
    /// <para>
    /// <b>Kenapa bukan penyedia seri nomor milik Billing.</b> <c>BillingNumberSeriesService</c>
    /// beserta tabel <c>BilNumberSeries</c> dimiliki <c>BillingManagement</c> dan sampai hari ini
    /// hanya dipakai di dalam modul itu. Memanggilnya dari <c>ClinicalManagement</c> berarti
    /// menulis ke tabel milik modul lain tanpa wewenang. Bentuk yang dipakai di sini adalah pola
    /// alokasi nomor yang sama dengan <c>InpEpisodeNumberService</c> dan
    /// <c>EmergencyDocumentNumberService</c> — dua service nomor milik modulnya sendiri yang
    /// sudah berjalan pada repository ini.
    /// </para>
    /// <para>
    /// Panjang nomornya tetap 23 karakter, sehingga muat pada kolom <c>varchar(30)</c>.
    /// </para>
    /// </remarks>
    public class PhysicianVisitNumberService
    {
        /// <summary>Awalan bawaan nomor kejadian visite.</summary>
        public const string DefaultPrefix = "VST";

        /// <summary>
        /// Membentuk satu nomor kejadian visite baru memakai waktu sekarang.
        /// </summary>
        /// <remarks>
        /// Tidak membuka transaksi dan tidak menulis apa pun ke database; ia ikut transaksi
        /// pemanggilnya.
        /// </remarks>
        public string Generate() => Generate(DefaultPrefix, DateTime.UtcNow);

        /// <summary>
        /// Membentuk nomor dari awalan dan waktu yang diberikan. Dipisahkan supaya bentuk
        /// nomornya dapat diuji tanpa database.
        /// </summary>
        /// <param name="prefix">Awalan nomor. Kosong berarti memakai <see cref="DefaultPrefix"/>.</param>
        /// <param name="now">Waktu yang dipakai membentuk bagian tanggal dan jam.</param>
        public string Generate(string? prefix, DateTime now)
        {
            var normalizedPrefix = string.IsNullOrWhiteSpace(prefix)
                ? DefaultPrefix
                : prefix.Trim().ToUpperInvariant();

            return $"{normalizedPrefix}-{now:yyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
        }
    }
}
