using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Alokasi nomor pesanan laboratorium yang dapat dibaca, dicetak, dan <b>disebut lewat
    /// telepon</b> (<c>LAB-DEC-072</c>).
    ///
    /// Bentuknya mengikuti <c>PatientEncounterNumberService</c> — awalan tetap dan nomor urut
    /// berpadding — tetapi <b>tiga hal sengaja berbeda</b>, dan ketiganya keputusan perancangan
    /// yang ditulis pada <c>backend-roadmap.md</c> bagian 6g, bukan detail implementasi.
    ///
    /// <list type="number">
    /// <item>
    /// <b>Celah tidak pernah diisi ulang.</b> Pola acuannya memindai celah pertama, sehingga
    /// nomor bekas baris yang hilang diberikan kepada baris baru. Untuk nomor yang dicetak pada
    /// amplop hasil pasien, perilaku itu berbahaya: amplop lama bernomor
    /// <c>LAB-RSMMC-000042</c> dapat berada di tangan satu pasien sementara nomor yang sama
    /// diberikan kepada pasien lain. Dua benda fisik, satu nomor, dan tidak ada yang melihat
    /// kesalahannya. Di sini dipakai <c>MAX + 1</c>, dan celah <b>dibiarkan ada</b>.
    ///
    /// <para>
    /// <b>Batas jaminannya ditulis apa adanya, karena ia pernah saya nyatakan terlalu kuat.</b>
    /// <c>MAX + 1</c> menjamin nomor tidak kembali <b>selama barisnya tetap ada di tabel</b> —
    /// termasuk baris ber-<c>IsDelete</c>, yang tetap terbaca oleh agregat ini. Aplikasi
    /// memang tidak pernah menghapus <c>LabOrder</c> secara fisik: nol <c>Remove</c>, nol
    /// endpoint <c>DELETE</c>, dan pembatalan hanya memindahkan status. Yang masih dapat
    /// mengembalikan sebuah nomor hanyalah penghapusan fisik dari luar aplikasi — dan hanya
    /// untuk nomor <b>tertinggi</b>, bukan celah di tengah. Dibuktikan saat <c>BE-LAB-36</c>
    /// diuji: empat pesanan uji dihapus lewat SQL, dan nomor berikutnya memang kembali ke
    /// bekasnya.
    /// </para>
    /// </item>
    /// <item>
    /// <b>Nol baris dimuat ke memori.</b> Pola acuannya memuat <b>seluruh</b> nomor terpakai lalu
    /// membangun <c>HashSet</c> di aplikasi; biayanya tumbuh seiring jumlah baris, dan pesanan
    /// laboratorium bertambah jauh lebih cepat daripada kunjungan. Di sini satu agregat.
    /// </item>
    /// <item>
    /// <b>Alokasi berblok.</b> Lihat <see cref="AllocateAsync"/>.
    /// </item>
    /// </list>
    ///
    /// <b>Kunci konkurensinya hanya berarti di dalam transaksi.</b> <c>pg_advisory_xact_lock</c>
    /// dilepas ketika transaksi berakhir; dipanggil di luar transaksi eksplisit, ia memperoleh
    /// dan melepas kuncinya seketika dan tidak menjaga apa pun. Pemanggil wajib membungkusnya.
    /// Index unik pada kolomnya tetap menjadi jaring pengaman terakhir — kunci mengurangi
    /// tabrakan, index yang membuatnya mustahil.
    /// </summary>
    public class LabOrderNumberService
    {
        public const string OrderCodePrefix = "LAB-RSMMC-";

        /// <summary>
        /// Enam digit, bukan lima seperti pola acuannya.
        ///
        /// Lima digit cukup untuk 99.999 kunjungan, tetapi satu kunjungan dapat melahirkan
        /// beberapa pesanan sekaligus sejak <c>BR-47</c> memecah pesanan per disiplin. Lima digit
        /// adalah utang yang jatuh temponya tidak terlihat sampai ia jatuh.
        ///
        /// Padding ini hanya menentukan <b>tampilan minimum</b>. Nomor yang melewati enam digit
        /// tetap benar karena pembacaan nilainya memakai perbandingan angka, bukan teks.
        /// </summary>
        public const int CodeNumberLength = 6;

        /// <summary>
        /// Penyaring baris yang bentuknya memang nomor pesanan.
        ///
        /// Bukan hiasan: tanpanya, satu baris berformat lain membuat <c>CAST</c> gagal dan
        /// <b>seluruh</b> pembuatan pesanan berhenti.
        /// </summary>
        private const string NumberPattern = "^" + OrderCodePrefix + "[0-9]+$";

        private readonly ApplicationDbContext _dbContext;

        public LabOrderNumberService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>Menyusun satu nomor dari nilai urutnya.</summary>
        public static string Format(long number) =>
            OrderCodePrefix + number.ToString().PadLeft(CodeNumberLength, '0');

        /// <summary>
        /// Mengalokasikan <paramref name="count"/> nomor berurutan sekaligus.
        ///
        /// <b>Kenapa berblok, dan kenapa ini bukan optimasi.</b>
        /// <c>POST /lab-orders/by-examinations</c> membentuk beberapa pesanan sekaligus dalam
        /// satu <c>SaveChangesAsync</c>. Entity yang belum tersimpan <b>tidak terlihat</b> oleh
        /// kueri SQL mentah, sehingga memanggil alokasi satu per satu di dalam transaksi yang
        /// sama akan mengembalikan <b>nomor yang sama berulang kali</b> — lalu ditolak index
        /// unik, dan seluruh permintaan gagal. Kegagalannya baru muncul ketika seorang pasien
        /// memesan pemeriksaan lintas disiplin, bukan pada pemakaian biasa.
        /// </summary>
        public async Task<IReadOnlyList<string>> AllocateAsync(
            int count,
            CancellationToken cancellationToken = default)
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(count),
                    "Jumlah nomor yang dialokasikan minimal satu.");
            }

            if (_dbContext.Database.IsNpgsql())
            {
                await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT pg_advisory_xact_lock(hashtext('LAB_ORDER_NUMBER')::bigint)",
                    cancellationToken);
            }

            var last = await ReadLastNumberAsync(cancellationToken);

            var numbers = new List<string>(count);
            for (var offset = 1; offset <= count; offset++)
            {
                numbers.Add(Format(last + offset));
            }

            return numbers;
        }

        /// <summary>Mengalokasikan satu nomor.</summary>
        public async Task<string> AllocateOneAsync(CancellationToken cancellationToken = default)
        {
            var numbers = await AllocateAsync(1, cancellationToken);
            return numbers[0];
        }

        /// <summary>
        /// Membaca nomor urut tertinggi yang sudah terpakai. Mengembalikan <c>0</c> bila belum
        /// ada satu pun.
        /// </summary>
        private async Task<long> ReadLastNumberAsync(CancellationToken cancellationToken)
        {
            if (_dbContext.Database.IsNpgsql())
            {
                // Bagian angkanya diambil lalu dibandingkan sebagai ANGKA, bukan sebagai teks.
                //
                // Perbandingan teks lebih murah dan tetap benar selama lebarnya seragam, tetapi
                // ia PECAH DIAM-DIAM pada digit ketujuh: `LAB-RSMMC-1000000` berurutan SEBELUM
                // `LAB-RSMMC-999999` secara leksikografis, sehingga nomor berikutnya akan mundur
                // dan bertabrakan. Kegagalannya tidak menimbulkan galat pada hari ia terjadi; ia
                // hanya mulai memberi nomor yang salah.
                var prefixLength = OrderCodePrefix.Length;

                var tertinggi = await _dbContext.Database
                    .SqlQuery<long?>(
                        $@"SELECT MAX(CAST(SUBSTRING(o.""OrderNumber"" FROM {prefixLength + 1}) AS BIGINT)) AS ""Value""
                           FROM public.""LabOrder"" o
                           WHERE o.""OrderNumber"" ~ {NumberPattern}")
                    .SingleAsync(cancellationToken);

                return tertinggi ?? 0;
            }

            // Jalur cadangan untuk penyedia selain PostgreSQL, yang tidak dipakai aplikasi ini
            // pada lingkungan mana pun hari ini. Ia mengambil SATU baris, bukan seluruhnya, dan
            // benar selama lebar nomornya seragam — batas yang sama dengan perbandingan teks di
            // atas, dan sengaja diterima di sini karena jalur ini bukan jalur produksi.
            var terakhir = await _dbContext.Set<LabOrder>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.OrderNumber != null && x.OrderNumber.StartsWith(OrderCodePrefix))
                .OrderByDescending(x => x.OrderNumber)
                .Select(x => x.OrderNumber)
                .FirstOrDefaultAsync(cancellationToken);

            if (string.IsNullOrEmpty(terakhir)) return 0;

            var angka = terakhir[OrderCodePrefix.Length..];

            return long.TryParse(angka, out var nilai) ? nilai : 0;
        }
    }
}
