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
        /// Lebar urut yang dipakai ketika pengaturan disiplin belum ada sama sekali. Empat,
        /// mengikuti Mikrobiologi dan Patologi Anatomi pada <c>LAB-EVD-005</c>.
        /// </summary>
        public const int DefaultSequenceLength = 4;

        /// <summary>
        /// Pemisah yang dipakai ketika pengaturan disiplin belum ada sama sekali.
        /// </summary>
        public const string DefaultYearSeparator = "-";

        /// <summary>
        /// Batas lebar urut yang diterima, supaya angka yang salah ketik nol menghasilkan
        /// nomor sepanjang ratusan digit.
        /// </summary>
        public const int MinSequenceLength = 1;

        /// <inheritdoc cref="MinSequenceLength"/>
        public const int MaxSequenceLength = 12;

        /// <summary>
        /// Bentuk nomor satu disiplin, dibaca dari <c>LabDisciplineSetting</c>
        /// (<c>LAB-API-v1</c> <c>r29</c>, menutup <c>LAB-OPEN-043</c>).
        /// </summary>
        public readonly record struct LabReportNumberShape(string Prefix, string Separator, int Length);

        private readonly ApplicationDbContext _dbContext;

        public LabReportNumberService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Menyusun satu nomor: awalan pengaturan, dua digit tahun, pemisah, lalu nomor urut.
        /// </summary>
        public static string Format(LabReportNumberShape shape, int year, long sequence)
        {
            var duaDigitTahun = (year % 100).ToString("D2", CultureInfo.InvariantCulture);
            var urut = sequence.ToString(CultureInfo.InvariantCulture)
                .PadLeft(shape.Length, '0');

            return $"{shape.Prefix}{duaDigitTahun}{shape.Separator}{urut}";
        }

        /// <summary>Awalan tetap satu nomor, yaitu bagian di hadapan nomor urutnya.</summary>
        private static string BuildStem(LabReportNumberShape shape, int year)
            => $"{shape.Prefix}{(year % 100).ToString("D2", CultureInfo.InvariantCulture)}{shape.Separator}";

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

            var shape = await ReadShapeAsync(discipline, cancellationToken);
            var awalan = BuildStem(shape, year);

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
                numbers.Add(Format(shape, year, last + offset));
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
        /// Bentuk nomor milik disiplin ini — awalan, pemisah, dan lebar urut.
        ///
        /// <b>Pengaturan yang hilang nol boleh menghentikan pembuatan pesanan</b>, sehingga
        /// ketiadaannya jatuh ke bentuk bawaan alih-alih melempar galat.
        ///
        /// <b>Lebar yang di luar batas dijepit, bukan ditolak.</b> Alokasi nomor berjalan di
        /// tengah pembuatan pesanan; menggagalkannya karena satu angka pengaturan yang keliru
        /// berarti menahan pekerjaan atas bahan yang sudah diambil dari tubuh pasien. Nilai
        /// yang keliru dicegah di hulu oleh validasi layar pengaturan.
        /// </summary>
        private async Task<LabReportNumberShape> ReadShapeAsync(
            LabDiscipline discipline,
            CancellationToken cancellationToken)
        {
            var setting = await _dbContext.LabDisciplineSettings
                .AsNoTracking()
                .Where(x => x.Discipline == discipline && !x.IsDelete && x.IsActive)
                .Select(x => new
                {
                    x.ReportNumberPrefix,
                    x.ReportNumberSeparator,
                    x.ReportNumberLength
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (setting is null)
            {
                return new LabReportNumberShape(
                    string.Empty, DefaultYearSeparator, DefaultSequenceLength);
            }

            var lebar = Math.Clamp(setting.ReportNumberLength, MinSequenceLength, MaxSequenceLength);

            // Pemisah KOSONG adalah nilai yang sah, bukan nilai yang belum diisi — Patologi
            // Klinik memang menempelkan tahun langsung pada nomornya (25039254). Karena itu
            // null dan string kosong DIBEDAKAN: null berarti belum pernah disetel dan jatuh ke
            // bawaan, string kosong berarti sengaja tanpa pemisah.
            var pemisah = setting.ReportNumberSeparator ?? DefaultYearSeparator;

            return new LabReportNumberShape(setting.ReportNumberPrefix ?? string.Empty, pemisah, lebar);
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
