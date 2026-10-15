using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Repositories;
using MstDoctorHr = QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models.MstDoctor;
using MstTariff = QuilvianSystemBackend.Areas.HealthServices.MasterData.Models.MstTariff;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services
{
    /// <summary>
    /// Laporan operasional Radiologi — menu v1 Laporan (Statistik Pemeriksaan, Dokter Pengirim, Pasien
    /// Perusahaan), Pemeriksaan (Harian, Per Item), dan Setup Tarif Tindakan (<c>BE-RAD-24</c>,
    /// <c>RAD-DEC-021</c>). Di v1 tayang seluruhnya masih "Coming Soon"; isinya dibaca dari pesanan V2.
    /// </summary>
    /// <remarks>
    /// Baca saja. Rentang dalam tanggal WIB menurut <c>RequestedAt ?? CreateDateTime</c>, pesanan
    /// <c>Draft</c> tidak dihitung — sama dengan Beranda (<c>BE-RAD-19</c>). Rincian dibatasi
    /// <see cref="MaxDetailRows"/> baris dengan penanda <c>IsTruncated</c>.
    /// </remarks>
    public class RadOperationalReportService
    {
        public const int MaxRangeDays = 366;
        public const int DefaultRangeDays = 30;
        public const int MaxDetailRows = 5000;

        private static readonly TimeSpan Wib = TimeSpan.FromHours(7);

        private readonly ApplicationDbContext _dbContext;

        public RadOperationalReportService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private sealed record Baris(
            Guid Id,
            string OrderNumber,
            Guid ProcedureId,
            string ProcedureCode,
            string ProcedureName,
            string ModalityName,
            RadOrderStatus OrderStatus,
            bool IsCancel,
            bool IsUrgent,
            DateTime Waktu,
            EncounterType? EncounterType,
            EncounterPaymentType? PaymentType,
            string? CompanyName,
            Guid? PatientId,
            string? PatientName,
            string? MedicalRecordNumber,
            Guid? RequestedByUserId,
            Guid? InstructingDoctorId);

        /// <summary>Rentang WIB → batas UTC. <c>null</c> + pesan bila tidak sah.</summary>
        internal static (DateTime AwalUtc, DateTime AkhirUtc, DateTime AwalWib, DateTime AkhirWib, string? Galat) Rentang(
            RadOperationalReportQuery? query, DateTime sekarangUtc)
        {
            var hariIni = (sekarangUtc + Wib).Date;

            static DateTime? Baca(string? s) =>
                DateTime.TryParseExact(s?.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
                    ? d.Date : null;

            var akhir = Baca(query?.EndDate) ?? hariIni;
            var awal = Baca(query?.StartDate) ?? akhir.AddDays(-(DefaultRangeDays - 1));

            if (!string.IsNullOrWhiteSpace(query?.StartDate) && Baca(query?.StartDate) == null ||
                !string.IsNullOrWhiteSpace(query?.EndDate) && Baca(query?.EndDate) == null)
            {
                return (default, default, default, default, "Format tanggal harus yyyy-MM-dd.");
            }

            if (awal > akhir)
                return (default, default, default, default, "Tanggal awal tidak boleh sesudah tanggal akhir.");

            if ((akhir - awal).TotalDays + 1 > MaxRangeDays)
                return (default, default, default, default, $"Rentang laporan paling panjang {MaxRangeDays} hari.");

            var awalUtc = DateTime.SpecifyKind(awal - Wib, DateTimeKind.Utc);
            var akhirUtc = DateTime.SpecifyKind(akhir.AddDays(1) - Wib, DateTimeKind.Utc);
            return (awalUtc, akhirUtc, awal, akhir, null);
        }

        private async Task<List<Baris>> BacaAsync(DateTime awalUtc, DateTime akhirUtc, CancellationToken cancellationToken)
        {
            var mentah = await _dbContext.RadOrders
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.OrderStatus != RadOrderStatus.Draft)
                .Where(x => (x.RequestedAt ?? x.CreateDateTime) >= awalUtc && (x.RequestedAt ?? x.CreateDateTime) < akhirUtc)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNumber,
                    x.ProcedureId,
                    ProcedureCode = x.Procedure != null ? x.Procedure.ProcedureCode : string.Empty,
                    ProcedureName = x.Procedure != null ? x.Procedure.ProcedureName : string.Empty,
                    ModalityName = x.Modality != null ? x.Modality.ModalityName : string.Empty,
                    x.OrderStatus,
                    x.IsCancel,
                    x.IsUrgent,
                    Waktu = x.RequestedAt ?? x.CreateDateTime,
                    EncounterType = x.Encounter != null ? (EncounterType?)x.Encounter.EncounterType : null,
                    PaymentType = x.Encounter != null ? (EncounterPaymentType?)x.Encounter.PaymentType : null,
                    CompanyName = x.Encounter != null && x.Encounter.PaymentSource != null
                        ? x.Encounter.PaymentSource.PaymentSourceNameSnapshot : null,
                    PatientId = x.Encounter != null ? (Guid?)x.Encounter.PatientId : null,
                    PatientName = x.Encounter != null && x.Encounter.Patient != null ? x.Encounter.Patient.FullName : null,
                    MedicalRecordNumber = x.Encounter != null && x.Encounter.Patient != null
                        ? x.Encounter.Patient.MedicalRecordNumber : null,
                    x.RequestedByUserId,
                    x.InstructingDoctorId,
                })
                .ToListAsync(cancellationToken);

            return mentah.Select(x => new Baris(
                x.Id, x.OrderNumber, x.ProcedureId, x.ProcedureCode, x.ProcedureName, x.ModalityName,
                x.OrderStatus, x.IsCancel, x.IsUrgent, x.Waktu, x.EncounterType, x.PaymentType, x.CompanyName,
                x.PatientId, x.PatientName, x.MedicalRecordNumber, x.RequestedByUserId, x.InstructingDoctorId)).ToList();
        }

        private static bool Batal(Baris b) =>
            b.IsCancel || b.OrderStatus is RadOrderStatus.Cancelled or RadOrderStatus.Rejected;

        private static bool Selesai(Baris b) => !b.IsCancel && b.OrderStatus == RadOrderStatus.Completed;

        private static RadOperationalReportResponse<T> Bungkus<T>(DateTime awalWib, DateTime akhirWib, List<T> rows, bool terpotong = false) => new()
        {
            PeriodStartDate = awalWib.ToString("yyyy-MM-dd"),
            PeriodEndDate = akhirWib.ToString("yyyy-MM-dd"),
            GeneratedAt = DateTime.UtcNow,
            IsTruncated = terpotong,
            Rows = rows,
        };

        private async Task<RadOperationResult<RadOperationalReportResponse<T>>> JalankanAsync<T>(
            RadOperationalReportQuery? query,
            Func<List<Baris>, DateTime, DateTime, Task<(List<T> Rows, bool Terpotong)>> susun,
            CancellationToken cancellationToken)
        {
            var (awalUtc, akhirUtc, awalWib, akhirWib, galat) = Rentang(query, DateTime.UtcNow);
            if (galat != null)
                return RadOperationResult<RadOperationalReportResponse<T>>.Validation(RadErrorCodes.ValidationFailed, galat);

            var baris = await BacaAsync(awalUtc, akhirUtc, cancellationToken);
            var (rows, terpotong) = await susun(baris, awalWib, akhirWib);
            return RadOperationResult<RadOperationalReportResponse<T>>.Success(Bungkus(awalWib, akhirWib, rows, terpotong));
        }

        /// <summary>Statistik Pemeriksaan — per pemeriksaan, terbanyak di atas.</summary>
        public Task<RadOperationResult<RadOperationalReportResponse<RadExaminationStatisticRow>>> GetExaminationStatisticsAsync(
            RadOperationalReportQuery? query, CancellationToken cancellationToken = default) =>
            JalankanAsync(query, (baris, _, _) => Task.FromResult((baris
                .GroupBy(b => b.ProcedureId)
                .Select(g => new RadExaminationStatisticRow
                {
                    ProcedureId = g.Key,
                    ProcedureCode = g.First().ProcedureCode,
                    ProcedureName = g.First().ProcedureName,
                    ModalityName = string.Join(", ", g.Select(b => b.ModalityName).Distinct()),
                    Total = g.Count(),
                    Outpatient = g.Count(b => b.EncounterType == EncounterType.Outpatient),
                    Inpatient = g.Count(b => b.EncounterType == EncounterType.Inpatient),
                    Emergency = g.Count(b => b.EncounterType == EncounterType.Emergency),
                    Urgent = g.Count(b => b.IsUrgent),
                    Completed = g.Count(Selesai),
                    Cancelled = g.Count(Batal),
                })
                .OrderByDescending(r => r.Total).ThenBy(r => r.ProcedureName)
                .ToList(), false)), cancellationToken);

        /// <summary>
        /// Dokter Pengirim — dokter pemberi instruksi bila ada (rawat inap, <c>BE-RWI-104</c>), selain itu
        /// akun pemesan.
        /// </summary>
        public Task<RadOperationResult<RadOperationalReportResponse<RadReferrerRow>>> GetReferrersAsync(
            RadOperationalReportQuery? query, CancellationToken cancellationToken = default) =>
            JalankanAsync<RadReferrerRow>(query, async (baris, _, _) =>
            {
                var dokterIds = baris.Where(b => b.InstructingDoctorId.HasValue).Select(b => b.InstructingDoctorId!.Value).Distinct().ToList();
                var userIds = baris.Where(b => !b.InstructingDoctorId.HasValue && b.RequestedByUserId.HasValue)
                    .Select(b => b.RequestedByUserId!.Value).Distinct().ToList();

                var namaDokter = dokterIds.Count == 0 ? new Dictionary<Guid, string>() : await _dbContext.Set<MstDoctorHr>()
                    .AsNoTracking().Where(d => dokterIds.Contains(d.Id))
                    .ToDictionaryAsync(d => d.Id, d => d.FullName, cancellationToken);
                var namaUser = userIds.Count == 0 ? new Dictionary<Guid, string>() : await _dbContext.Users
                    .AsNoTracking().Where(u => userIds.Contains(u.Id))
                    .Select(u => new { u.Id, Nama = u.DisplayName ?? u.UserName ?? u.UserCode ?? "-" })
                    .ToDictionaryAsync(u => u.Id, u => u.Nama!, cancellationToken);

                var rows = baris
                    .GroupBy(b => b.InstructingDoctorId.HasValue
                        ? ("Doctor", namaDokter.GetValueOrDefault(b.InstructingDoctorId.Value) ?? "(dokter tidak dikenal)")
                        : b.RequestedByUserId.HasValue
                            ? ("User", namaUser.GetValueOrDefault(b.RequestedByUserId.Value) ?? "(pengguna tidak dikenal)")
                            : ("User", "(tanpa pemesan)"))
                    .Select(g => new RadReferrerRow
                    {
                        ReferrerSource = g.Key.Item1,
                        ReferrerName = g.Key.Item2,
                        Total = g.Count(),
                        PatientCount = g.Select(b => b.PatientId).Where(p => p.HasValue).Distinct().Count(),
                        Urgent = g.Count(b => b.IsUrgent),
                        Completed = g.Count(Selesai),
                        Cancelled = g.Count(Batal),
                    })
                    .OrderByDescending(r => r.Total).ThenBy(r => r.ReferrerName)
                    .ToList();
                return (rows, false);
            }, cancellationToken);

        /// <summary>Pasien Perusahaan — pesanan berpenjamin perusahaan, urut nama perusahaan lalu waktu.</summary>
        public Task<RadOperationResult<RadOperationalReportResponse<RadOrderReportRow>>> GetCompanyPatientsAsync(
            RadOperationalReportQuery? query, CancellationToken cancellationToken = default) =>
            JalankanAsync(query, (baris, _, _) => Task.FromResult(Rincian(
                baris.Where(b => b.PaymentType == EncounterPaymentType.CompanyGuarantor)
                    .OrderBy(b => b.CompanyName ?? "~").ThenBy(b => b.Waktu))), cancellationToken);

        /// <summary>Pemeriksaan Per Item — seluruh pesanan, urut pemeriksaan lalu waktu.</summary>
        public Task<RadOperationResult<RadOperationalReportResponse<RadOrderReportRow>>> GetPerItemAsync(
            RadOperationalReportQuery? query, CancellationToken cancellationToken = default) =>
            JalankanAsync(query, (baris, _, _) => Task.FromResult(Rincian(
                baris.OrderBy(b => b.ProcedureName).ThenBy(b => b.Waktu))), cancellationToken);

        private static (List<RadOrderReportRow> Rows, bool Terpotong) Rincian(IEnumerable<Baris> urut)
        {
            var semua = urut.Take(MaxDetailRows + 1).Select(b => new RadOrderReportRow
            {
                RadOrderId = b.Id,
                OrderNumber = b.OrderNumber,
                OrderedAt = b.Waktu,
                CompanyName = b.CompanyName,
                PatientName = b.PatientName,
                MedicalRecordNumber = b.MedicalRecordNumber,
                EncounterType = b.EncounterType?.ToString(),
                ProcedureName = b.ProcedureName,
                ModalityName = b.ModalityName,
                OrderStatus = b.OrderStatus.ToString(),
                IsUrgent = b.IsUrgent,
            }).ToList();

            return semua.Count > MaxDetailRows ? (semua.Take(MaxDetailRows).ToList(), true) : (semua, false);
        }

        /// <summary>Pemeriksaan Harian — satu baris per tanggal WIB dalam rentang, hari kosong ikut.</summary>
        public Task<RadOperationResult<RadOperationalReportResponse<RadDailyExaminationRow>>> GetDailyAsync(
            RadOperationalReportQuery? query, CancellationToken cancellationToken = default) =>
            JalankanAsync(query, (baris, awalWib, akhirWib) =>
            {
                var perHari = baris.GroupBy(b => (b.Waktu + Wib).Date).ToDictionary(g => g.Key, g => g.ToList());
                var rows = new List<RadDailyExaminationRow>();
                for (var hari = awalWib; hari <= akhirWib; hari = hari.AddDays(1))
                {
                    var g = perHari.GetValueOrDefault(hari) ?? new List<Baris>();
                    rows.Add(new RadDailyExaminationRow
                    {
                        Date = hari.ToString("yyyy-MM-dd"),
                        Total = g.Count,
                        Outpatient = g.Count(b => b.EncounterType == EncounterType.Outpatient),
                        Inpatient = g.Count(b => b.EncounterType == EncounterType.Inpatient),
                        Emergency = g.Count(b => b.EncounterType == EncounterType.Emergency),
                        Urgent = g.Count(b => b.IsUrgent),
                        Completed = g.Count(Selesai),
                        Cancelled = g.Count(Batal),
                    });
                }
                return Task.FromResult((rows, false));
            }, cancellationToken);

        /// <summary>
        /// Setup Tarif Tindakan — pemeriksaan Radiologi beserta tarifnya. Baca saja: tarif dikelola di
        /// master tarif Billing, bukan di Radiologi.
        /// </summary>
        public async Task<List<RadProcedureTariffRow>> GetProcedureTariffsAsync(CancellationToken cancellationToken = default)
        {
            var tindakan = await _dbContext.MstProcedures
                .AsNoTracking()
                .Where(p => p.IsRadiology && !p.IsDelete)
                .OrderBy(p => p.ProcedureName)
                .Select(p => new RadProcedureTariffRow
                {
                    ProcedureId = p.Id,
                    ProcedureCode = p.ProcedureCode,
                    ProcedureName = p.ProcedureName,
                    CategoryName = p.ProcedureCategoryName,
                    IsActive = p.IsActive,
                })
                .ToListAsync(cancellationToken);

            var ids = tindakan.Select(t => t.ProcedureId).ToList();
            if (ids.Count == 0) return tindakan;

            var tarif = await _dbContext.Set<MstTariff>()
                .AsNoTracking()
                .Where(t => t.ProcedureId != null && ids.Contains(t.ProcedureId.Value) && !t.IsDelete)
                .OrderBy(t => t.PatientClass != null ? t.PatientClass.PatientClassName : "")
                .Select(t => new
                {
                    ProcedureId = t.ProcedureId!.Value,
                    Item = new RadTariffItem
                    {
                        TariffCode = t.TariffCode,
                        TariffName = t.TariffName,
                        PatientClassName = t.PatientClass != null ? t.PatientClass.PatientClassName : null,
                        NormalPrice = t.NormalPrice,
                        EffectiveStartDate = t.EffectiveStartDate,
                        EffectiveEndDate = t.EffectiveEndDate,
                        IsActive = t.IsActive,
                    },
                })
                .ToListAsync(cancellationToken);

            foreach (var t in tindakan)
                t.Tariffs = tarif.Where(x => x.ProcedureId == t.ProcedureId).Select(x => x.Item).ToList();

            return tindakan;
        }
    }
}
