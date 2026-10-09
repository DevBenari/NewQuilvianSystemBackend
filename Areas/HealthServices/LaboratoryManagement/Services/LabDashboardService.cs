using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Data ringkasan Beranda Laboratorium (<c>LAB-API-v1</c> <c>r44</c> bagian 39, BR-140,
    /// <c>02-backend-architecture.md</c> bagian 28).
    ///
    /// <para>
    /// <b>Hanya membaca.</b> Nol tabel ringkasan, nol job terjadwal, nol transaksi — setiap angka
    /// dihitung saat diminta dari pesanan, permintaan pemeriksaan, dan pemeriksaan yang sudah ada.
    /// Tabel ringkasan akan basi tanpa satu galat pun.
    /// </para>
    ///
    /// <para>
    /// <b>Waktu acuan sebuah pesanan</b> = <c>RequestedAt</c>, jatuh ke <c>CreateDateTime</c> bagi
    /// pesanan lama yang belum punya waktu diminta — sama dengan urutan daftar pantau.
    /// <b>Hari dibaca dalam WIB</b>: pesanan pukul 00.00–06.59 WIB tersimpan pada tanggal UTC
    /// sebelumnya, sehingga membandingkan tanggal UTC mentah memindahkannya ke hari yang keliru.
    /// </para>
    /// </summary>
    public class LabDashboardService
    {
        /// <summary>Jumlah baris tabel pesanan terbaru (<c>LAB-DEC-209</c>).</summary>
        public const int RecentOrderLimit = 10;

        /// <summary>Tahun paling awal yang boleh dipilih (<c>VAL-154</c>).</summary>
        public const int MinYear = 2000;

        private readonly ApplicationDbContext _dbContext;
        private readonly LabWorklistService _labWorklistService;

        public LabDashboardService(
            ApplicationDbContext dbContext,
            LabWorklistService labWorklistService)
        {
            _dbContext = dbContext;
            _labWorklistService = labWorklistService;
        }

        /// <summary>
        /// Kartu <i>hari ini</i> dan <i>fokus operasional</i> (<c>r44</c> 39.3).
        /// <paramref name="asOf"/> hanya untuk uji; kosong berarti saat ini.
        /// </summary>
        public async Task<LabDashboardTodayResponse> GetTodayAsync(
            DateTime? asOf = null,
            CancellationToken cancellationToken = default)
        {
            var sekarang = asOf ?? DateTime.UtcNow;
            var hariIni = WibDateOf(sekarang);
            var awal = AppDateTimeHelper.OperationalDateToUtc(hariIni);
            var akhir = AppDateTimeHelper.OperationalDateToUtc(hariIni.AddDays(1));

            var pesananHariIni = _dbContext.LabOrders
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    (x.RequestedAt ?? x.CreateDateTime) >= awal &&
                    (x.RequestedAt ?? x.CreateDateTime) < akhir);

            // Satu perjalanan untuk ketiga hitungan status, cara yang sama dengan rekap pesanan.
            var rekap = await pesananHariIni
                .GroupBy(x => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Menunggu = g.Count(x =>
                        x.OrderStatus != LabOrderStatus.Completed &&
                        x.OrderStatus != LabOrderStatus.Cancelled),
                    Selesai = g.Count(x => x.OrderStatus == LabOrderStatus.Completed)
                })
                .FirstOrDefaultAsync(cancellationToken);

            var total = rekap?.Total ?? 0;
            var selesai = rekap?.Selesai ?? 0;

            // CITO ikut menghitung pesanan yang sudah dibatalkan, sejalan dengan "Pesanan hari ini"
            // (LAB-REQ-021 butir 3).
            var cito = await DenganCito(pesananHariIni).CountAsync(cancellationToken);

            var totalTercatat = await _dbContext.LabOrders
                .AsNoTracking()
                .CountAsync(x => !x.IsDelete, cancellationToken);

            var menungguValidasi = await _labWorklistService.CountAwaitingValidationAsync(cancellationToken);

            return new LabDashboardTodayResponse
            {
                OperationalDate = hariIni.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                GeneratedAt = sekarang,
                TodayOrderCount = total,
                WaitingOrderCount = rekap?.Menunggu ?? 0,
                CompletedOrderCount = selesai,
                CompletionPercent = total == 0
                    ? 0
                    : (int)Math.Round(selesai * 100m / total, MidpointRounding.AwayFromZero),
                CitoOrderCount = cito,
                TotalRecordedOrderCount = totalTercatat,
                AwaitingValidationCount = menungguValidasi
            };
        }

        /// <summary>
        /// Sepuluh pesanan terakhir diminta dari seluruh disiplin dan <b>seluruh status</b>,
        /// termasuk yang dibatalkan (<c>r44</c> 39.5, <c>LAB-REQ-021</c> butir 9).
        /// </summary>
        public async Task<List<LabDashboardRecentOrderResponse>> GetRecentOrdersAsync(
            CancellationToken cancellationToken = default)
        {
            var rows = await _dbContext.LabOrders
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .OrderByDescending(x => x.RequestedAt ?? x.CreateDateTime)
                .ThenByDescending(x => x.CreateDateTime)
                .ThenBy(x => x.Id)
                .Take(RecentOrderLimit)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNumber,
                    x.Discipline,
                    x.OrderStatus,
                    RequestedAt = x.RequestedAt ?? x.CreateDateTime,
                    PatientId = x.Encounter != null ? (Guid?)x.Encounter.PatientId : null,
                    ProcedureName = x.Procedure != null ? x.Procedure.ProcedureName : null
                })
                .ToListAsync(cancellationToken);

            if (rows.Count == 0)
            {
                return new List<LabDashboardRecentOrderResponse>();
            }

            // Pasien dan permintaan pemeriksaan — masing-masing SATU kueri untuk kesepuluh baris.
            // Hanya dua ruas pasien yang dibaca; data pribadi lain tidak pernah dimuat.
            var patientIds = rows
                .Where(x => x.PatientId.HasValue)
                .Select(x => x.PatientId!.Value)
                .Distinct()
                .ToList();

            var pasien = await _dbContext.MstPatients
                .AsNoTracking()
                .Where(p => patientIds.Contains(p.Id))
                .Select(p => new { p.Id, p.FullName, p.MedicalRecordNumber })
                .ToDictionaryAsync(p => p.Id, cancellationToken);

            var orderIds = rows.Select(x => x.Id).ToList();

            var permintaan = await _dbContext.LabOrderedProcedures
                .AsNoTracking()
                .Where(p =>
                    orderIds.Contains(p.LabOrderId) &&
                    !p.IsDelete &&
                    p.OrderedStatus != LabOrderedProcedureStatus.Cancelled)
                .OrderBy(p => p.CreateDateTime)
                .ThenBy(p => p.Id)
                .Select(p => new
                {
                    p.LabOrderId,
                    Name = p.ProcedureNameSnapshot ?? (p.Procedure != null ? p.Procedure.ProcedureName : null)
                })
                .ToListAsync(cancellationToken);

            var namaPerPesanan = permintaan
                .Where(p => !string.IsNullOrWhiteSpace(p.Name))
                .GroupBy(p => p.LabOrderId)
                .ToDictionary(g => g.Key, g => g.Select(p => p.Name!).ToList());

            return rows.Select(x =>
            {
                var p = x.PatientId is Guid pid && pasien.TryGetValue(pid, out var ketemu) ? ketemu : null;

                // Pesanan sebelum BE-LAB-26 tidak punya baris permintaan; pemeriksaan utama pesanan
                // menjadi satu-satunya nama yang dapat ditampilkan.
                var nama = namaPerPesanan.TryGetValue(x.Id, out var daftar)
                    ? daftar
                    : string.IsNullOrWhiteSpace(x.ProcedureName)
                        ? new List<string>()
                        : new List<string> { x.ProcedureName };

                return new LabDashboardRecentOrderResponse
                {
                    LabOrderId = x.Id,
                    OrderNumber = x.OrderNumber,
                    MedicalRecordNumber = p?.MedicalRecordNumber,
                    PatientName = p?.FullName,
                    ProcedureNames = nama,
                    Discipline = x.Discipline?.ToString(),
                    OrderStatus = x.OrderStatus.ToString(),
                    RequestedAt = x.RequestedAt
                };
            }).ToList();
        }

        /// <summary>
        /// Grafik per disiplin, sebaran, <i>Jenis laboratorium</i>, dan tren bulanan untuk satu tahun
        /// (<c>r44</c> 39.4, 39.8). Kosong = tahun berjalan WIB.
        /// </summary>
        /// <exception cref="LabDashboardValidationException"><c>VAL-154</c> — tahun di luar 2000 sampai tahun berjalan.</exception>
        public async Task<LabDashboardYearlyResponse> GetYearlyAsync(
            int? year,
            DateTime? asOf = null,
            CancellationToken cancellationToken = default)
        {
            var sekarang = asOf ?? DateTime.UtcNow;

            // VAL-154. "Tahun berjalan" dibaca WIB: 2026-12-31 17.30 UTC sudah tahun 2027.
            var tahunBerjalan = WibDateOf(sekarang).Year;
            var tahun = year ?? tahunBerjalan;

            if (tahun < MinYear || tahun > tahunBerjalan)
            {
                throw new LabDashboardValidationException(
                    "Tahun tidak sah. Pilih tahun 2000 sampai tahun berjalan.");
            }

            var awal = AppDateTimeHelper.OperationalDateToUtc(new DateTime(tahun, 1, 1));
            var akhir = AppDateTimeHelper.OperationalDateToUtc(new DateTime(tahun + 1, 1, 1));

            // r44 39.8 — pemeriksaan yang tidak gugur/batal ditambah permintaan yang belum masuk wadah,
            // pada pesanan yang TIDAK dibatalkan. Membatalkan pesanan tidak ikut membatalkan pemeriksaan
            // maupun permintaannya, sehingga syarat pesanan wajib ditulis di sini. Permintaan Fulfilled
            // tidak dihitung lagi: ia sudah terwakili barisnya di pemeriksaan. Disiplin dibaca sama
            // dengan VAL-126 — pesanan, lalu katalog.
            var pemeriksaan = await _dbContext.LabExaminations
                .AsNoTracking()
                .Where(e =>
                    !e.IsDelete &&
                    e.ExaminationStatus != LabExaminationStatus.Voided &&
                    e.ExaminationStatus != LabExaminationStatus.Cancelled &&
                    e.LabOrder != null &&
                    !e.LabOrder.IsDelete &&
                    e.LabOrder.OrderStatus != LabOrderStatus.Cancelled &&
                    (e.LabOrder.RequestedAt ?? e.LabOrder.CreateDateTime) >= awal &&
                    (e.LabOrder.RequestedAt ?? e.LabOrder.CreateDateTime) < akhir)
                .GroupBy(e => e.LabOrder!.Discipline ?? (e.Procedure != null ? e.Procedure.LabDiscipline : null))
                .Select(g => new { Discipline = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var belumMasukWadah = await _dbContext.LabOrderedProcedures
                .AsNoTracking()
                .Where(p =>
                    !p.IsDelete &&
                    p.OrderedStatus == LabOrderedProcedureStatus.Ordered &&
                    p.LabOrder != null &&
                    !p.LabOrder.IsDelete &&
                    p.LabOrder.OrderStatus != LabOrderStatus.Cancelled &&
                    (p.LabOrder.RequestedAt ?? p.LabOrder.CreateDateTime) >= awal &&
                    (p.LabOrder.RequestedAt ?? p.LabOrder.CreateDateTime) < akhir)
                .GroupBy(p => p.LabOrder!.Discipline ?? (p.Procedure != null ? p.Procedure.LabDiscipline : null))
                .Select(g => new { Discipline = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            int Jumlah(LabDiscipline? disiplin) =>
                pemeriksaan.Where(x => x.Discipline == disiplin).Sum(x => x.Count) +
                belumMasukWadah.Where(x => x.Discipline == disiplin).Sum(x => x.Count);

            var disiplin = Enum.GetValues<LabDiscipline>()
                .OrderBy(x => (int)x)
                .Select(x => new LabDashboardDisciplineCountResponse
                {
                    Discipline = x.ToString(),
                    ExaminationCount = Jumlah(x)
                })
                .ToList();

            // Tren: SATU kolom waktu untuk dua tahun, dikelompokkan per bulan WIB di memori. Aritmetika
            // tanggal di SQL berbeda bentuk antar-provider (alasan yang sama dengan pantau cito).
            var awalPembanding = AppDateTimeHelper.OperationalDateToUtc(new DateTime(tahun - 1, 1, 1));

            var waktuPesanan = await _dbContext.LabOrders
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.OrderStatus != LabOrderStatus.Cancelled &&
                    x.OrderStatus != LabOrderStatus.Draft &&
                    (x.RequestedAt ?? x.CreateDateTime) >= awalPembanding &&
                    (x.RequestedAt ?? x.CreateDateTime) < akhir)
                .Select(x => x.RequestedAt ?? x.CreateDateTime)
                .ToListAsync(cancellationToken);

            var perBulan = waktuPesanan
                .Select(WibDateOf)
                .GroupBy(x => (x.Year, x.Month))
                .ToDictionary(g => g.Key, g => g.Count());

            var bulanan = Enumerable.Range(1, 12)
                .Select(bulan => new LabDashboardMonthlyOrderResponse
                {
                    Month = bulan,
                    OrderCount = perBulan.GetValueOrDefault((tahun, bulan)),
                    PreviousYearOrderCount = perBulan.GetValueOrDefault((tahun - 1, bulan))
                })
                .ToList();

            return new LabDashboardYearlyResponse
            {
                Year = tahun,
                PreviousYear = tahun - 1,
                GeneratedAt = sekarang,
                Disciplines = disiplin,
                UnclassifiedExaminationCount = Jumlah(null),
                ActiveDisciplineCount = disiplin.Count(x => x.ExaminationCount > 0),
                MonthlyOrders = bulanan
            };
        }

        // =================================================================
        // Pembantu
        // =================================================================

        /// <summary>
        /// Pesanan yang memuat cito (<c>r44</c> 39.6, <c>02-backend-architecture.md</c> 28.2 bahan 1).
        ///
        /// <b>Dua sumber, sengaja.</b> Tanda yang dipilih saat memesan hanya ada pada
        /// <see cref="LabOrderedProcedure"/> sampai wadahnya dibuat; tanda <i>Tandai Cito</i> sesudah
        /// wadah hanya ada pada <see cref="LabExamination"/>. Membaca salah satu saja membuat kartu cito
        /// pagi hari hampir selalu nol. Daftar pantau (<c>hasCito</c>) sengaja <b>tidak</b> diubah di
        /// sini — perbaikannya menunggu keputusan atas 28.8.
        /// </summary>
        private IQueryable<LabOrder> DenganCito(IQueryable<LabOrder> source) =>
            source.Where(x =>
                _dbContext.LabOrderedProcedures.Any(p =>
                    p.LabOrderId == x.Id &&
                    !p.IsDelete &&
                    p.OrderedStatus != LabOrderedProcedureStatus.Cancelled &&
                    p.Urgency == LabExaminationUrgency.Cito) ||
                _dbContext.LabExaminations.Any(e =>
                    e.LabOrderId == x.Id &&
                    !e.IsDelete &&
                    e.ExaminationStatus != LabExaminationStatus.Voided &&
                    e.ExaminationStatus != LabExaminationStatus.Cancelled &&
                    e.Urgency == LabExaminationUrgency.Cito));

        /// <summary>
        /// Tanggal WIB dari satu saat UTC, memakai zona yang sama dengan
        /// <see cref="AppDateTimeHelper.OperationalDateToUtc"/>. WIB berada di depan UTC, sehingga
        /// tanggalnya hanya mungkin tanggal UTC itu sendiri atau sehari sesudahnya.
        /// </summary>
        private static DateTime WibDateOf(DateTime utc)
        {
            var tanggalUtc = utc.Date;
            var esok = tanggalUtc.AddDays(1);

            return AppDateTimeHelper.OperationalDateToUtc(esok) <= utc ? esok : tanggalUtc;
        }
    }

    /// <summary>Penolakan permintaan ringkasan Beranda — dijawab <c>422</c> (<c>VAL-154</c>).</summary>
    public sealed class LabDashboardValidationException(string message) : Exception(message);
}
