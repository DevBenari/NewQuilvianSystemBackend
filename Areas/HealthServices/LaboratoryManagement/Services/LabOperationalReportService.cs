using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Constants;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Laporan operasional Laboratorium (<c>LAB-API-v1</c> <c>r37</c> bagian 32,
    /// <c>02-backend-architecture.md</c> 23.4) — <b>hanya membaca</b> fakta yang sudah tercatat.
    /// Nol tabel ringkasan, nol job terjadwal (A7.5), nol transaksi.
    ///
    /// <para>
    /// <b>Periode dibaca per tanggal WIB.</b> <see cref="LabQueryDateRange"/> mengubah tanggal polos
    /// menjadi tengah malam WIB, dan akhir periode dinaikkan ke penghabisan hari. Membandingkan
    /// kolom UTC dengan tanggal mentah membuat pemeriksaan pukul 00.00-06.59 WIB jatuh ke hari
    /// sebelumnya tanpa satu galat pun.
    /// </para>
    ///
    /// <para>
    /// <b>Tanpa identitas pasien</b> (23.11): laporan memuat angka dan nama jenis pemeriksaan saja.
    /// </para>
    /// </summary>
    public class LabOperationalReportService
    {
        /// <summary>Panjang periode paling banyak, inklusif (<c>VAL-149</c>, 23.10 butir 3).</summary>
        public const int MaxPeriodDays = 366;

        private const string BelumTergolong = "Belum tergolong";

        private readonly ApplicationDbContext _dbContext;

        public LabOperationalReportService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Jumlah pemeriksaan yang <b>dirilis</b> pada periode itu, per disiplin order lalu per jenis
        /// pemeriksaan (<c>LAB-DEC-159</c> butir 2, <c>INV-55</c>).
        ///
        /// <para>
        /// Dasarnya <c>ReleasedAt</c>, bukan <c>FinalizedAt</c> maupun status: hasil resmi adalah
        /// hasil yang dirilis (<c>LAB-DEC-155</c>). Pengelompokan dijalankan di basis data.
        /// </para>
        ///
        /// <para>
        /// Disiplin di luar <see cref="LabReleasableDisciplines"/> ditulis <i>belum dapat dihitung</i>
        /// beserta alasannya — <b>bukan 0</b> (<c>ARCH-GAP-LAB-11</c>): angka 0 menyatakan tidak ada
        /// pekerjaan, padahal jalur rilisnya yang belum ada. Order lama berdisiplin kosong
        /// dikelompokkan <i>Belum tergolong</i>, tidak dibuang.
        /// </para>
        /// </summary>
        /// <exception cref="LabOperationalReportValidationException"><c>VAL-147</c>..<c>VAL-149</c>.</exception>
        public async Task<LabExaminationCountReportResponse> GetExaminationCountAsync(
            LabOperationalReportQuery query,
            CancellationToken cancellationToken = default)
        {
            var periode = ResolvePeriod(query);

            var source = _dbContext.LabExaminations
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.ReleasedAt != null &&
                    x.ReleasedAt >= periode.StartUtc &&
                    x.ReleasedAt <= periode.EndUtc &&
                    x.LabOrder != null &&
                    !x.LabOrder.IsDelete);

            if (query.Discipline is LabDiscipline disiplinDipilih)
            {
                source = source.Where(x => x.LabOrder!.Discipline == disiplinDipilih);
            }

            var kelompok = await source
                .GroupBy(x => new
                {
                    x.LabOrder!.Discipline,
                    x.ProcedureId,
                    ProcedureName = x.ProcedureNameSnapshot ?? x.Procedure!.ProcedureName
                })
                .Select(g => new KelompokRilis(
                    g.Key.Discipline,
                    g.Key.ProcedureId,
                    g.Key.ProcedureName,
                    g.Count(),
                    g.Max(x => x.ReleasedAt)))
                .ToListAsync(cancellationToken);

            var disiplinDalamCakupan = query.Discipline is LabDiscipline satu
                ? new[] { satu }
                : Enum.GetValues<LabDiscipline>();

            var rows = new List<LabExaminationCountRow>();

            foreach (var disiplin in disiplinDalamCakupan)
            {
                var nama = NamaDisiplin(disiplin);

                if (!LabReleasableDisciplines.Contains(disiplin))
                {
                    rows.Add(new LabExaminationCountRow
                    {
                        Discipline = disiplin.ToString(),
                        DisciplineName = nama,
                        IsCountable = false,
                        NotCountableReason = $"Rilis hasil {nama} belum tersedia.",
                        Total = null
                    });

                    continue;
                }

                rows.Add(BarisTerhitung(disiplin.ToString(), nama, kelompok.Where(x => x.Discipline == disiplin)));
            }

            // Penyaring disiplin memilih disiplin ORDER, sehingga order berdisiplin kosong hanya
            // muncul ketika tidak ada penyaring — dan hanya bila memang ada yang dirilis.
            var tanpaDisiplin = kelompok.Where(x => x.Discipline == null).ToList();

            if (query.Discipline is null && tanpaDisiplin.Count > 0)
            {
                rows.Add(BarisTerhitung(null, BelumTergolong, tanpaDisiplin));
            }

            return new LabExaminationCountReportResponse
            {
                Period = periode.Response,
                Rows = rows,
                TotalCountable = rows.Where(x => x.IsCountable).Sum(x => x.Total ?? 0)
            };
        }

        /// <summary>
        /// <c>VAL-147</c> — periode wajib (<c>400</c>); <c>VAL-148</c> — awal tidak sesudah akhir,
        /// bunyi yang sama dengan endpoint Laboratorium lain (<c>400</c>); <c>VAL-149</c> — paling
        /// panjang 366 hari, inklusif (<c>422</c>). Satu aturan bagi ketiga laporan dan unduhannya.
        /// </summary>
        internal static ReportPeriod ResolvePeriod(LabOperationalReportQuery query)
        {
            if (query.StartDate is not DateTime awal || query.EndDate is not DateTime akhir)
            {
                throw new LabOperationalReportValidationException(
                    StatusCodes.Status400BadRequest,
                    "Periode laporan wajib diisi.");
            }

            var (startUtc, endUtc) = LabQueryDateRange.Normalize(awal, akhir);

            if (startUtc!.Value > endUtc!.Value)
            {
                throw new LabOperationalReportValidationException(
                    StatusCodes.Status400BadRequest,
                    LabQueryDateRange.InvertedRangeMessage);
            }

            var jumlahHari = (akhir.Date - awal.Date).Days + 1;

            if (jumlahHari > MaxPeriodDays)
            {
                throw new LabOperationalReportValidationException(
                    StatusCodes.Status422UnprocessableEntity,
                    "Periode laporan paling panjang 366 hari. Persempit rentang tanggalnya.");
            }

            return new ReportPeriod(
                startUtc.Value,
                endUtc.Value,
                new LabReportPeriodResponse
                {
                    StartDate = DateOnly.FromDateTime(awal),
                    EndDate = DateOnly.FromDateTime(akhir),
                    GeneratedAt = DateTime.UtcNow
                });
        }

        // Satu baris per jenis pemeriksaan. Kelompok basis data memisahkan nama tersimpan yang
        // berbeda untuk jenis yang sama (katalog diubah di antara dua pemesanan); di sini keduanya
        // disatukan, dan nama yang dipakai adalah nama pada rilis TERAKHIR — nama tersimpan yang
        // paling baru berlaku, bukan nama katalog hari ini.
        private static LabExaminationCountRow BarisTerhitung(
            string? discipline,
            string disciplineName,
            IEnumerable<KelompokRilis> kelompok)
        {
            var procedures = kelompok
                .GroupBy(x => x.ProcedureId)
                .Select(g => new LabExaminationCountProcedureRow
                {
                    ProcedureId = g.Key,
                    ProcedureName = g.OrderByDescending(x => x.TerakhirDirilis).First().ProcedureName ?? string.Empty,
                    Total = g.Sum(x => x.Total)
                })
                .OrderByDescending(x => x.Total)
                .ThenBy(x => x.ProcedureName)
                .ToList();

            return new LabExaminationCountRow
            {
                Discipline = discipline,
                DisciplineName = disciplineName,
                IsCountable = true,
                NotCountableReason = null,
                Total = procedures.Sum(x => x.Total),
                Procedures = procedures
            };
        }

        private static string NamaDisiplin(LabDiscipline discipline) => discipline switch
        {
            LabDiscipline.ClinicalPathology => "Patologi Klinik",
            LabDiscipline.AnatomicalPathology => "Patologi Anatomi",
            LabDiscipline.Microbiology => "Mikrobiologi",
            _ => discipline.ToString()
        };

        private sealed record KelompokRilis(
            LabDiscipline? Discipline,
            Guid ProcedureId,
            string? ProcedureName,
            int Total,
            DateTime? TerakhirDirilis);

        internal sealed record ReportPeriod(DateTime StartUtc, DateTime EndUtc, LabReportPeriodResponse Response);
    }

    /// <summary>
    /// Penolakan periode laporan (<c>VAL-147</c>..<c>VAL-149</c>) beserta kode status HTTP-nya —
    /// <c>400</c> bagi periode kosong atau terbalik, <c>422</c> bagi periode terlalu panjang.
    /// </summary>
    public sealed class LabOperationalReportValidationException(int statusCode, string message) : Exception(message)
    {
        public int StatusCode { get; } = statusCode;
    }
}
