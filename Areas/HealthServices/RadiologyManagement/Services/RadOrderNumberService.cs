namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services
{
    /// <summary>
    /// Membentuk nomor pesanan radiologi, misalnya <c>RAD-ORD-260911074012-A1B2C3</c>.
    /// Tidak memakai interface, mengikuti pola project.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Mengapa pesanan perlu nomornya sendiri.</b> Sampai sebelum ini, satu-satunya cara
    /// menunjuk sebuah pesanan adalah <c>Guid</c>-nya. Guid tidak dapat dibacakan lewat telepon,
    /// tidak muat pada label, dan tidak dapat diketik petugas yang memegang lembar permintaan.
    /// <c>RadStudy</c> sudah punya <c>StudyNumber</c> dan <c>RadReport</c> sudah punya
    /// <c>ReportNumber</c>; pesanan adalah satu-satunya yang tertinggal, padahal justru itulah
    /// satuan yang disebut orang ketika bertanya "pemeriksaan atas nama siapa".
    /// </para>
    /// <para>
    /// <b>Nomor tidak dibentuk dari hitungan baris.</b> <c>Count + 1</c> maupun <c>Max + 1</c>
    /// dilarang <c>QBE-CODE-003</c>. Dua dokter yang menyimpan pesanan pada detik yang sama akan
    /// membaca angka yang sama lalu menerbitkan dua pesanan bernomor kembar — dan nomor pesanan
    /// ikut tercetak pada label yang menempel di amplop citra.
    /// </para>
    /// <para>
    /// Enam huruf/angka acak dari Guid membuat dua permintaan bersamaan tetap berbeda, dan index
    /// unik <c>IX_RadOrder_OrderNumber</c> menjadi penjaga terakhirnya (<c>QBE-CODE-004</c>).
    /// </para>
    /// <para>
    /// Bentuknya sengaja dibuat sama persis dengan <see cref="RadReportNumberService"/> supaya
    /// petugas mengenali keduanya sebagai satu keluarga nomor, dan supaya panjangnya tetap
    /// 27 karakter sehingga muat pada kolom <c>varchar(64)</c>.
    /// </para>
    /// </remarks>
    public class RadOrderNumberService
    {
        /// <summary>Awalan bawaan nomor pesanan radiologi.</summary>
        public const string DefaultPrefix = "RAD-ORD";

        /// <summary>Membentuk satu nomor pesanan baru memakai waktu sekarang.</summary>
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
