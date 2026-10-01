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
        private readonly LabCitoTurnaroundPolicy _labCitoTurnaroundPolicy;

        public LabOperationalReportService(
            ApplicationDbContext dbContext,
            LabCitoTurnaroundPolicy labCitoTurnaroundPolicy)
        {
            _dbContext = dbContext;
            _labCitoTurnaroundPolicy = labCitoTurnaroundPolicy;
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
        /// Angka penolakan wadah per disiplin order beserta rincian per alasan
        /// (<c>LAB-DEC-159</c> butir 3, <c>INV-56</c>).
        ///
        /// <para>
        /// <b>Dasarnya keputusan, bukan status wadah hari ini.</b> Diputuskan = <c>DecidedAt</c> dalam
        /// periode; tidak layak = <c>RejectionReasonCode</c> terisi. Wadah yang sesudah diputuskan
        /// berpindah status tetap terhitung pada hari keputusannya. Wadah pengganti adalah wadah
        /// tersendiri: tabung hemolisis yang ditolak lalu diganti menjadi <b>dua</b> keputusan dan
        /// <b>satu</b> penolakan.
        /// </para>
        ///
        /// <para>
        /// <b>Ketiga disiplin terhitung</b> — tidak memakai <see cref="LabReleasableDisciplines"/>,
        /// sebab keputusan kelayakan ada pada semuanya. Angka penolakan <b>kosong</b>, bukan 0, bila
        /// nol wadah diputuskan: pembaginya nol, dan 0 akan terbaca "tidak pernah menolak".
        /// </para>
        ///
        /// <para>
        /// Satu kueri berkelompok (disiplin × kode alasan × nama alasan); wadah layak membentuk
        /// kelompok tanpa kode, sehingga jumlah keputusan dan penolakan lahir dari kueri yang sama.
        /// </para>
        /// </summary>
        /// <exception cref="LabOperationalReportValidationException"><c>VAL-147</c>..<c>VAL-149</c>.</exception>
        public async Task<LabSpecimenRejectionReportResponse> GetSpecimenRejectionAsync(
            LabOperationalReportQuery query,
            CancellationToken cancellationToken = default)
        {
            var periode = ResolvePeriod(query);

            var source = _dbContext.LabSpecimens
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.DecidedAt != null &&
                    x.DecidedAt >= periode.StartUtc &&
                    x.DecidedAt <= periode.EndUtc &&
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
                    x.RejectionReasonCode,
                    ReasonName = x.RejectionReason != null ? x.RejectionReason.ReasonName : null
                })
                .Select(g => new KelompokKeputusan(
                    g.Key.Discipline,
                    g.Key.RejectionReasonCode,
                    g.Key.ReasonName,
                    g.Count()))
                .ToListAsync(cancellationToken);

            var disiplinDalamCakupan = query.Discipline is LabDiscipline satu
                ? new[] { satu }
                : Enum.GetValues<LabDiscipline>();

            var rows = disiplinDalamCakupan
                .Select(d => BarisPenolakan(d.ToString(), NamaDisiplin(d), kelompok.Where(x => x.Discipline == d).ToList()))
                .ToList();

            // Sama dengan laporan jumlah: order berdisiplin kosong hanya tanpa penyaring, dan hanya
            // bila memang ada keputusan.
            var tanpaDisiplin = kelompok.Where(x => x.Discipline == null).ToList();

            if (query.Discipline is null && tanpaDisiplin.Count > 0)
            {
                rows.Add(BarisPenolakan(null, BelumTergolong, tanpaDisiplin));
            }

            // Satu baris per disiplin dan kode alasan; urut disiplin (Belum tergolong terakhir), lalu
            // alasan terbanyak.
            var reasons = kelompok
                .Where(x => x.ReasonCode != null)
                .GroupBy(x => new { x.Discipline, x.ReasonCode })
                .OrderBy(g => g.Key.Discipline is null)
                .ThenBy(g => g.Key.Discipline)
                .ThenByDescending(g => g.Sum(x => x.Total))
                .ThenBy(g => g.Key.ReasonCode)
                .Select(g => new LabSpecimenRejectionReasonRow
                {
                    Discipline = g.Key.Discipline?.ToString(),
                    ReasonCode = g.Key.ReasonCode!,
                    ReasonName = g.Select(x => x.ReasonName).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? g.Key.ReasonCode!,
                    Count = g.Sum(x => x.Total)
                })
                .ToList();

            return new LabSpecimenRejectionReportResponse
            {
                Period = periode.Response,
                Rows = rows,
                Reasons = reasons
            };
        }

        private static LabSpecimenRejectionRow BarisPenolakan(
            string? discipline,
            string disciplineName,
            IReadOnlyCollection<KelompokKeputusan> kelompok)
        {
            var diputuskan = kelompok.Sum(x => x.Total);
            var ditolak = kelompok.Where(x => x.ReasonCode != null).Sum(x => x.Total);

            return new LabSpecimenRejectionRow
            {
                Discipline = discipline,
                DisciplineName = disciplineName,
                DecidedCount = diputuskan,
                RejectedCount = ditolak,
                RejectionRatePercent = diputuskan == 0
                    ? null
                    : Math.Round(ditolak * 100m / diputuskan, 1, MidpointRounding.AwayFromZero)
            };
        }

        /// <summary>
        /// Waktu penyelesaian per disiplin order × kesegeraan (<c>LAB-DEC-159</c> butir 4, <c>INV-57</c>,
        /// <c>ARCH-GAP-LAB-13</c>): pemeriksaan yang <b>dirilis</b> pada periode itu dan wadahnya pernah
        /// dinyatakan layak.
        ///
        /// <para>
        /// <b>Selangnya dari <c>ChargeEligibleAt</c> sampai <c>ReleasedAt</c></b> — bukan dari waktu
        /// pengambilan atau pemesanan: sebelum wadah layak, laboratorium belum punya apa pun untuk
        /// dikerjakan. Pengambilan ulang karena itu dihitung dari wadah pengganti; pemeriksaan yang
        /// digugurkan bersama wadah lamanya tidak pernah dirilis dan tidak ikut.
        /// </para>
        ///
        /// <para>
        /// <b>Terlambat = satu rumus dengan daftar pantau</b> (<c>INV-57</c>): batas dari
        /// <see cref="LabCitoTurnaroundPolicy"/> — batas yang berlaku <b>saat laporan dibuka</b>
        /// (23.10 butir 5) — dan perbandingan yang sama, <c>dirilis &gt; layak + batas</c>. Tepat di batas
        /// tidak terlambat. Cito tanpa batas tidak dinilai terlambat, tetap masuk rata-rata.
        /// </para>
        ///
        /// <para>
        /// Selisih waktu dihitung di memori atas proyeksi lima kolom, sebab batasnya per jenis
        /// pemeriksaan dan terlambat dinilai per baris. Jumlah barisnya dibatasi periode 366 hari
        /// (<c>VAL-149</c>).
        /// </para>
        /// </summary>
        /// <exception cref="LabOperationalReportValidationException"><c>VAL-147</c>..<c>VAL-149</c>.</exception>
        public async Task<LabTurnaroundTimeReportResponse> GetTurnaroundTimeAsync(
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
                    x.ChargeEligibleAt != null &&
                    x.LabOrder != null &&
                    !x.LabOrder.IsDelete);

            if (query.Discipline is LabDiscipline disiplinDipilih)
            {
                source = source.Where(x => x.LabOrder!.Discipline == disiplinDipilih);
            }

            var rilis = await source
                .Select(x => new RilisSelang(
                    x.LabOrder!.Discipline,
                    x.Urgency,
                    x.ProcedureId,
                    x.ChargeEligibleAt!.Value,
                    x.ReleasedAt!.Value))
                .ToListAsync(cancellationToken);

            var procedureCito = rilis
                .Where(x => x.Urgency == LabExaminationUrgency.Cito)
                .Select(x => x.ProcedureId)
                .Distinct()
                .ToList();

            var batasCito = procedureCito.Count == 0
                ? new Dictionary<Guid, int?>()
                : await _labCitoTurnaroundPolicy.GetLimitsAsync(procedureCito, cancellationToken);

            var disiplinDalamCakupan = query.Discipline is LabDiscipline satu
                ? new[] { satu }
                : Enum.GetValues<LabDiscipline>();

            var rows = new List<LabTurnaroundTimeRow>();

            foreach (var disiplin in disiplinDalamCakupan)
            {
                var nama = NamaDisiplin(disiplin);

                foreach (var urgensi in UrutanKesegeraan)
                {
                    if (!LabReleasableDisciplines.Contains(disiplin))
                    {
                        rows.Add(new LabTurnaroundTimeRow
                        {
                            Discipline = disiplin.ToString(),
                            DisciplineName = nama,
                            Urgency = urgensi.ToString(),
                            IsCountable = false,
                            NotCountableReason = $"Rilis hasil {nama} belum tersedia."
                        });

                        continue;
                    }

                    rows.Add(BarisSelang(
                        disiplin.ToString(), nama, urgensi,
                        rilis.Where(x => x.Discipline == disiplin && x.Urgency == urgensi).ToList(),
                        batasCito));
                }
            }

            // Sama dengan kedua laporan lain: order berdisiplin kosong hanya tanpa penyaring, dan
            // hanya bila memang ada yang dirilis.
            var tanpaDisiplin = rilis.Where(x => x.Discipline == null).ToList();

            if (query.Discipline is null && tanpaDisiplin.Count > 0)
            {
                foreach (var urgensi in UrutanKesegeraan)
                {
                    rows.Add(BarisSelang(null, BelumTergolong, urgensi,
                        tanpaDisiplin.Where(x => x.Urgency == urgensi).ToList(), batasCito));
                }
            }

            return new LabTurnaroundTimeReportResponse
            {
                Period = periode.Response,
                Rows = rows
            };
        }

        // Cito lebih dulu, sama dengan contoh respons r37 32.3 dan urutan daftar kerja.
        private static readonly LabExaminationUrgency[] UrutanKesegeraan =
        {
            LabExaminationUrgency.Cito,
            LabExaminationUrgency.Routine
        };

        private static LabTurnaroundTimeRow BarisSelang(
            string? discipline,
            string disciplineName,
            LabExaminationUrgency urgensi,
            IReadOnlyCollection<RilisSelang> rilis,
            IReadOnlyDictionary<Guid, int?> batasCito)
        {
            var cito = urgensi == LabExaminationUrgency.Cito;
            int? terlambat = cito ? 0 : null;
            int? tanpaBatas = cito ? 0 : null;

            if (cito)
            {
                foreach (var x in rilis)
                {
                    batasCito.TryGetValue(x.ProcedureId, out var menit);

                    if (menit is null)
                    {
                        tanpaBatas++;
                        continue;
                    }

                    // Perbandingan yang SAMA dengan LabWorklistService.GetCitoOverdueAsync:
                    // tenggat = layak + batas; terlambat hanya bila waktunya MELEWATI tenggat.
                    var tenggat = x.ChargeEligibleAt.AddMinutes(menit.Value);

                    if (x.ReleasedAt > tenggat)
                    {
                        terlambat++;
                    }
                }
            }

            return new LabTurnaroundTimeRow
            {
                Discipline = discipline,
                DisciplineName = disciplineName,
                Urgency = urgensi.ToString(),
                IsCountable = true,
                NotCountableReason = null,
                ReleasedCount = rilis.Count,
                AverageMinutes = rilis.Count == 0
                    ? null
                    : Math.Round(
                        (decimal)rilis.Average(x => (x.ReleasedAt - x.ChargeEligibleAt).TotalMinutes),
                        1,
                        MidpointRounding.AwayFromZero),
                OverdueCount = terlambat,
                WithoutLimitCount = tanpaBatas
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

        private sealed record RilisSelang(
            LabDiscipline? Discipline,
            LabExaminationUrgency Urgency,
            Guid ProcedureId,
            DateTime ChargeEligibleAt,
            DateTime ReleasedAt);

        private sealed record KelompokKeputusan(
            LabDiscipline? Discipline,
            string? ReasonCode,
            string? ReasonName,
            int Total);

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
