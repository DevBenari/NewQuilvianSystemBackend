using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Statistik Pesanan Laboratorium (<c>LAB-API-v1</c> <c>r49</c> 44.3, BR-152, BR-153,
    /// <c>02-backend-architecture.md</c> 32.4, <c>BE-LAB-98</c>) — <b>beban permintaan</b> per jenis kunjungan →
    /// unit layanan → disiplin. <b>Hanya membaca</b>; nol tabel ringkasan, nol transaksi.
    ///
    /// <para>
    /// <b>Satu definisi hitung dengan Beranda</b> (<c>INV-59</c>): seluruh baris datang dari
    /// <see cref="LabOrderDemandQuery"/> — pesanan bukan batal/<c>Draft</c>, pemeriksaan bukan batal/gugur,
    /// permintaan yang belum masuk wadah — menurut waktu pesanan diminta (WIB). Laporan ini tidak punya rumus
    /// sendiri.
    /// </para>
    ///
    /// <para>
    /// <b>Tanpa identitas pasien</b> (<c>AC-335</c>): pasien tidak pernah dibaca.
    /// </para>
    /// </summary>
    public class LabOrderStatisticsReportService
    {
        public const string BelumTerklasifikasi = "Belum terklasifikasi";
        public const string UnitTidakTercatat = "Unit tidak tercatat";

        private readonly ApplicationDbContext _dbContext;
        private readonly LabOrderDemandQuery _labOrderDemandQuery;

        public LabOrderStatisticsReportService(
            ApplicationDbContext dbContext,
            LabOrderDemandQuery labOrderDemandQuery)
        {
            _dbContext = dbContext;
            _labOrderDemandQuery = labOrderDemandQuery;
        }

        /// <summary>Penyaring laporan; unit layanan aktif dibaca dari data induk.</summary>
        public async Task<LabOrderStatisticsReportFilterMetadataResponse> GetFilterMetadataAsync(
            CancellationToken cancellationToken = default)
        {
            var result = LabFilterMetadataFactory.LabOrderStatisticsReport();

            result.ServiceUnits = await _dbContext.MstServiceUnits
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive)
                .OrderBy(x => x.ServiceUnitName)
                .Select(x => new LabServiceUnitOptionResponse
                {
                    Id = x.Id,
                    Code = x.ServiceUnitCode,
                    Name = x.ServiceUnitName
                })
                .ToListAsync(cancellationToken);

            return result;
        }

        /// <exception cref="LabOperationalReportValidationException"><c>VAL-147</c>..<c>VAL-149</c>.</exception>
        public async Task<LabOrderStatisticsReportResponse> GetAsync(
            LabOrderStatisticsReportQuery query,
            CancellationToken cancellationToken = default)
        {
            var periode = LabOperationalReportService.ResolvePeriod(query);

            var pesanan = await Hitung(
                Saring(_labOrderDemandQuery.CountedOrders(periode.StartUtc, periode.EndUtc)
                    .Select(LabOrderDemandQuery.OrderDemand), query),
                cancellationToken);

            var pemeriksaan = await Hitung(
                Saring(_labOrderDemandQuery.CountedExaminations(periode.StartUtc, periode.EndUtc)
                    .Select(LabOrderDemandQuery.ExaminationDemand), query),
                cancellationToken);

            var belumMasukWadah = await Hitung(
                Saring(_labOrderDemandQuery.PendingOrderedProcedures(periode.StartUtc, periode.EndUtc)
                    .Select(LabOrderDemandQuery.OrderedProcedureDemand), query),
                cancellationToken);

            // Satu kunci (jenis kunjungan, unit, disiplin) → (pesanan, pemeriksaan).
            var sel = new Dictionary<(EncounterType, Guid?, LabDiscipline?), (int Pesanan, int Pemeriksaan)>();
            void Tambah(IEnumerable<Kelompok> rows, bool isPesanan)
            {
                foreach (var r in rows)
                {
                    var key = (r.EncounterType ?? EncounterType.Unknown, r.ServiceUnitId, r.Discipline);
                    sel.TryGetValue(key, out var v);
                    sel[key] = isPesanan ? (v.Pesanan + r.Count, v.Pemeriksaan) : (v.Pesanan, v.Pemeriksaan + r.Count);
                }
            }
            Tambah(pesanan, true);
            Tambah(pemeriksaan, false);
            Tambah(belumMasukWadah, false);

            // Nama unit apa adanya dari data induk — termasuk unit yang kini tidak aktif, supaya angka lama
            // tetap bernama. Tidak ditemukan → "Unit tidak tercatat" (AC-335).
            var unitIds = sel.Keys.Where(k => k.Item2.HasValue).Select(k => k.Item2!.Value).Distinct().ToList();
            var namaUnit = unitIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _dbContext.MstServiceUnits
                    .AsNoTracking()
                    .Where(x => unitIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.ServiceUnitName, cancellationToken);

            // Kolom disiplin tetap: ketiga disiplin, ditambah "Belum terklasifikasi" bila ada isinya.
            var kolom = Enum.GetValues<LabDiscipline>().Select(x => (LabDiscipline?)x).ToList();
            if (sel.Keys.Any(k => k.Item3 is null))
                kolom.Add(null);

            LabOrderStatisticsCell Sel(LabDiscipline? d, int pesananN, int pemeriksaanN) => new()
            {
                Discipline = d?.ToString(),
                DisciplineName = d is LabDiscipline x ? LabFilterMetadataFactory.LabelDisiplin(x) : BelumTerklasifikasi,
                OrderCount = pesananN,
                ExaminationCount = pemeriksaanN
            };

            LabOrderStatisticsTotals Jumlahkan(IEnumerable<KeyValuePair<(EncounterType, Guid?, LabDiscipline?), (int Pesanan, int Pemeriksaan)>> isi)
            {
                var daftar = isi.ToList();
                return new LabOrderStatisticsTotals
                {
                    OrderCount = daftar.Sum(x => x.Value.Pesanan),
                    ExaminationCount = daftar.Sum(x => x.Value.Pemeriksaan),
                    Cells = kolom.Select(d => Sel(
                        d,
                        daftar.Where(x => x.Key.Item3 == d).Sum(x => x.Value.Pesanan),
                        daftar.Where(x => x.Key.Item3 == d).Sum(x => x.Value.Pemeriksaan))).ToList()
                };
            }

            var groups = sel
                .GroupBy(x => x.Key.Item1)
                .OrderBy(g => g.Key == EncounterType.Unknown ? int.MaxValue : (int)g.Key)
                .Select(g => new LabOrderStatisticsGroup
                {
                    EncounterType = g.Key.ToString(),
                    EncounterTypeName = LabFilterMetadataFactory.LabelJenisKunjungan(g.Key),
                    Units = g
                        .GroupBy(x => x.Key.Item2)
                        .Select(u =>
                        {
                            var total = Jumlahkan(u);
                            return new LabOrderStatisticsUnitRow
                            {
                                ServiceUnitId = u.Key,
                                ServiceUnitName = u.Key is Guid id && namaUnit.TryGetValue(id, out var nama)
                                    ? nama
                                    : UnitTidakTercatat,
                                Cells = total.Cells,
                                OrderCount = total.OrderCount,
                                ExaminationCount = total.ExaminationCount
                            };
                        })
                        .OrderBy(u => u.ServiceUnitName == UnitTidakTercatat ? 1 : 0)
                        .ThenBy(u => u.ServiceUnitName)
                        .ToList(),
                    Subtotal = Jumlahkan(g)
                })
                .ToList();

            return new LabOrderStatisticsReportResponse
            {
                Period = periode.Response,
                Groups = groups,
                Total = Jumlahkan(sel)
            };
        }

        /// <summary>
        /// Penyaring dalam kalimat untuk log unduhan dan cetak — <i>"Rawat Inap — Kamar Bersalin — Patologi
        /// Klinik"</i>. Tanpa data pasien (<c>LAB-PERM-v1</c> 19.5).
        /// </summary>
        public async Task<string> DescribeFiltersAsync(
            LabOrderStatisticsReportQuery query,
            CancellationToken cancellationToken = default)
        {
            var jenis = query.EncounterType is EncounterType j
                ? LabFilterMetadataFactory.LabelJenisKunjungan(j)
                : "seluruh jenis kunjungan";

            var unit = "seluruh unit";
            if (query.ServiceUnitId is Guid id)
            {
                unit = await _dbContext.MstServiceUnits
                    .AsNoTracking()
                    .Where(x => x.Id == id)
                    .Select(x => x.ServiceUnitName)
                    .FirstOrDefaultAsync(cancellationToken) ?? UnitTidakTercatat;
            }

            var disiplin = query.Discipline is LabDiscipline d
                ? LabFilterMetadataFactory.LabelDisiplin(d)
                : "seluruh disiplin";

            return $"{jenis} — {unit} — {disiplin}";
        }

        private static IQueryable<LabDemandRow> Saring(IQueryable<LabDemandRow> rows, LabOrderStatisticsReportQuery query)
        {
            if (query.EncounterType is EncounterType jenis)
                rows = rows.Where(x => x.EncounterType == jenis);

            if (query.ServiceUnitId is Guid unit)
                rows = rows.Where(x => x.ServiceUnitId == unit);

            if (query.Discipline is LabDiscipline disiplin)
                rows = rows.Where(x => x.Discipline == disiplin);

            return rows;
        }

        private static Task<List<Kelompok>> Hitung(IQueryable<LabDemandRow> rows, CancellationToken cancellationToken) =>
            rows
                .GroupBy(x => new { x.EncounterType, x.ServiceUnitId, x.Discipline })
                .Select(g => new Kelompok
                {
                    EncounterType = g.Key.EncounterType,
                    ServiceUnitId = g.Key.ServiceUnitId,
                    Discipline = g.Key.Discipline,
                    Count = g.Count()
                })
                .ToListAsync(cancellationToken);

        private sealed class Kelompok
        {
            public EncounterType? EncounterType { get; set; }
            public Guid? ServiceUnitId { get; set; }
            public LabDiscipline? Discipline { get; set; }
            public int Count { get; set; }
        }
    }
}
