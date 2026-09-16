namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services
{
    /// <summary>
    /// Membentuk nomor bacaan radiologi, misalnya <c>RAD-RPT-260911074012-A1B2C3</c>.
    /// Tidak memakai interface, mengikuti pola project.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Nomor tidak dibentuk dari hitungan baris.</b> <c>Count + 1</c> maupun <c>Max + 1</c>
    /// dilarang <c>QBE-CODE-003</c>, dan alasannya nyata pada radiologi: dua radiolog yang
    /// menyimpan draf pada detik yang sama akan membaca angka yang sama lalu menerbitkan dua
    /// bacaan bernomor kembar. Nomor bacaan ikut tertulis pada surat hasil dan rekam medis —
    /// dua berkas dengan nomor sama berarti tidak ada lagi cara menunjuk satu bacaan tertentu.
    /// </para>
    /// <para>
    /// Enam huruf/angka acak dari Guid membuat dua permintaan bersamaan tetap berbeda, dan index
    /// unik <c>IX_RadReport_ReportNumber</c> menjadi penjaga terakhirnya (<c>QBE-CODE-004</c>).
    /// </para>
    /// <para>
    /// <b>Kenapa bukan penyedia seri nomor milik Billing.</b> <c>BillingNumberSeriesService</c>
    /// beserta tabel <c>BilNumberSeries</c> dimiliki <c>BillingManagement</c>. Memanggilnya dari
    /// Radiologi berarti menulis ke tabel milik modul lain tanpa wewenang. Bentuk di sini sama
    /// dengan <c>PhysicianVisitNumberService</c> dan <c>InpEpisodeNumberService</c> — dua service
    /// nomor milik modulnya sendiri yang sudah berjalan pada repository ini.
    /// </para>
    /// <para>
    /// Panjang nomornya tetap 27 karakter, sehingga muat pada kolom <c>varchar(64)</c>.
    /// </para>
    /// </remarks>
    public class RadReportNumberService
    {
        /// <summary>Awalan bawaan nomor bacaan radiologi.</summary>
        public const string DefaultPrefix = "RAD-RPT";

        /// <summary>Membentuk satu nomor bacaan baru memakai waktu sekarang.</summary>
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
