using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Repositories;
using System.Globalization;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Alokasi nomor yang tercetak pada lembar hasil, <b>per disiplin per tahun</b>
    /// (<c>LAB-DEC-117</c>).
    ///
    /// <b>Menyalin pola <see cref="LabOrderNumberService"/> yang sudah berjalan</b> — kunci
    /// advisory di dalam transaksi, <c>MAX + 1</c>, alokasi berblok, dan index unik sebagai
    /// jaring pengaman terakhir. Satu perbedaan: penghitungnya <b>per disiplin per tahun</b>,
    /// bukan global, sehingga kuncinya pun harus per disiplin per tahun — kunci global akan
    /// membuat pesanan Mikrobiologi menunggu pesanan Patologi Anatomi tanpa sebab.
    ///
    /// <b>Celah tetap dibiarkan ada.</b> Alasannya sama persis dengan nomor order: nomor ini
    /// dicetak pada lembar yang dipegang pasien, dan dua lembar bernomor sama adalah kesalahan
    /// yang tidak terlihat oleh siapa pun.
    /// </summary>
    public class LabReportNumberService
    {
        /// <summary>
        /// Lebar minimum bagian urut. Bukti <c>LAB-EVD-005</c> menunjukkan <c>26-1129</c> dan
        /// <c>26.0919</c> — keduanya empat digit.
        /// </summary>
        public const int SequenceLength = 4;

        /// <summary>
        /// Pemisah antara tahun dan nomor urut.
        ///
        /// <b>Ia konstanta, dan itu batas yang diketahui — bukan kelalaian.</b> Bukti cetak
        /// memperlihatkan tiga bentuk berbeda: Mikrobiologi <c>26-1129</c>, Patologi Anatomi
        /// <c>26.0919</c>, dan Patologi Klinik <c>25039254</c> yang nol berpemisah serta
        /// berurut enam digit. <c>LAB-API-v1</c> <c>r27</c> bagian 22.7 hanya menyetujui satu
        /// ruas <c>reportNumberPrefix</c>, dan satu ruas awalan <b>tidak dapat</b> menyatakan
        /// pemisah maupun lebar. Menambah kolomnya sendiri berarti mengubah kontrak yang sudah
        /// disetujui secara sepihak, sehingga perbedaannya diangkat sebagai
        /// <c>LAB-OPEN-043</c>.
        /// </summary>
        public const string YearSeparator = "-";

        private readonly ApplicationDbContext _dbContext;

        public LabReportNumberService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Menyusun satu nomor: awalan pengaturan, dua digit tahun, pemisah, lalu nomor urut.
        /// </summary>
        public static string Format(string? prefix, int year, long sequence)
        {
            var duaDigitTahun = (year % 100).ToString("D2", CultureInfo.InvariantCulture);
            var urut = sequence.ToString(CultureInfo.InvariantCulture)
                .PadLeft(SequenceLength, '0');

            return $"{prefix}{duaDigitTahun}{YearSeparator}{urut}";
        }

        /// <summary>
        /// Mengalokasikan <paramref name="count"/> nomor berurutan untuk satu disiplin pada satu
        /// tahun.
        ///
        /// <b>Berblok karena satu permintaan dapat melahirkan beberapa pesanan sekaligus.</b>
        /// Entity yang belum tersimpan tidak terlihat oleh kueri SQL mentah, sehingga alokasi
        /// satu per satu di dalam transaksi yang sama akan mengembalikan nomor yang sama
        /// berulang kali.
        ///
        /// <b>Pemanggil WAJIB membungkusnya dalam transaksi eksplisit.</b>
        /// <c>pg_advisory_xact_lock</c> dilepas ketika transaksi berakhir; dipanggil di luar
        /// transaksi, ia memperoleh dan melepas kuncinya seketika dan nol menjaga apa pun.
        /// </summary>
        public async Task<IReadOnlyList<string>> AllocateAsync(
            LabDiscipline discipline,
            int year,
            int count,
            CancellationToken cancellationToken = default)
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(count),
                    "Jumlah nomor yang dialokasikan minimal satu.");
            }

            var prefix = await ReadPrefixAsync(discipline, cancellationToken);
            var awalan = $"{prefix}{(year % 100).ToString("D2", CultureInfo.InvariantCulture)}{YearSeparator}";

            if (_dbContext.Database.IsNpgsql())
            {
                // Kuncinya per disiplin per tahun — sama sempitnya dengan penghitungnya.
                var kunci = $"LAB_REPORT_NUMBER_{(int)discipline}_{year}";

                await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT pg_advisory_xact_lock(hashtext({kunci})::bigint)",
                    cancellationToken);
            }

            var last = await ReadLastSequenceAsync(discipline, awalan, cancellationToken);

            var numbers = new List<string>(count);
            for (var offset = 1; offset <= count; offset++)
            {
                numbers.Add(Format(prefix, year, last + offset));
            }

            return numbers;
        }

        /// <summary>Mengalokasikan satu nomor.</summary>
        public async Task<string> AllocateOneAsync(
            LabDiscipline discipline,
            int year,
            CancellationToken cancellationToken = default)
        {
            var numbers = await AllocateAsync(discipline, year, 1, cancellationToken);
            return numbers[0];
        }

        /// <summary>
        /// Awalan milik disiplin ini. Mengembalikan teks kosong bila pengaturannya belum ada —
        /// <b>pengaturan yang hilang nol boleh menghentikan pembuatan pesanan.</b>
        /// </summary>
        private async Task<string> ReadPrefixAsync(
            LabDiscipline discipline,
            CancellationToken cancellationToken)
        {
            var prefix = await _dbContext.LabDisciplineSettings
                .AsNoTracking()
                .Where(x => x.Discipline == discipline && !x.IsDelete && x.IsActive)
                .Select(x => x.ReportNumberPrefix)
                .FirstOrDefaultAsync(cancellationToken);

            return prefix ?? string.Empty;
        }

        /// <summary>
        /// Membaca nomor urut tertinggi yang sudah terpakai pada disiplin dan tahun itu.
        /// Mengembalikan <c>0</c> bila belum ada satu pun.
        /// </summary>
        private async Task<long> ReadLastSequenceAsync(
            LabDiscipline discipline,
            string awalan,
            CancellationToken cancellationToken)
        {
            if (_dbContext.Database.IsNpgsql())
            {
                // Bagian angkanya dibandingkan sebagai ANGKA, bukan sebagai teks — alasan yang
                // sama dengan LabOrderNumberService: perbandingan teks PECAH DIAM-DIAM pada
                // digit kelima, dan kegagalannya nol menimbulkan galat pada hari ia terjadi.
                var pola = "^" + System.Text.RegularExpressions.Regex.Escape(awalan) + "[0-9]+$";
                var panjangAwalan = awalan.Length;
                var kodeDisiplin = (int)discipline;

                var tertinggi = await _dbContext.Database
                    .SqlQuery<long?>(
                        $@"SELECT MAX(CAST(SUBSTRING(o.""LabReportNumber"" FROM {panjangAwalan + 1}) AS BIGINT)) AS ""Value""
                           FROM public.""LabOrder"" o
                           WHERE o.""Discipline"" = {kodeDisiplin}
                             AND o.""LabReportNumber"" ~ {pola}")
                    .SingleAsync(cancellationToken);

                return tertinggi ?? 0;
            }

            // Jalur cadangan bagi penyedia selain PostgreSQL, yang nol dipakai aplikasi ini pada
            // lingkungan mana pun hari ini. Benar selama lebar nomornya seragam.
            var terakhir = await _dbContext.LabOrders
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.Discipline == discipline
                            && x.LabReportNumber != null
                            && x.LabReportNumber.StartsWith(awalan))
                .OrderByDescending(x => x.LabReportNumber)
                .Select(x => x.LabReportNumber)
                .FirstOrDefaultAsync(cancellationToken);

            if (string.IsNullOrEmpty(terakhir)) return 0;

            return long.TryParse(terakhir[awalan.Length..], out var nilai) ? nilai : 0;
        }
    }
}
