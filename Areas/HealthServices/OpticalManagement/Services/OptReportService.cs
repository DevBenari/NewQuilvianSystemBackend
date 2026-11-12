using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Services
{
    /// <summary>
    /// Empat laporan inti Optik (<c>BE-OPT-052</c>, <c>OPT-DEC-025</c>, <c>OPT-API-v1</c> grup Opt Report). Baca-saja.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Periode</b> dalam tanggal WIB, termasuk kedua ujung, paling panjang 366 hari. Kosong = awal bulan berjalan s.d. hari ini.
    /// </para>
    /// <para>
    /// <b>Penjualan dibaca dari Billing</b>: baris tagihan sumber <c>OPTICAL</c> yang aktif (tidak di-void) dan pertama kali
    /// ditagih pada periode. Optik tidak menyimpan harga (<c>QBE-ENT-003</c>), sehingga angka laporan sama dengan baris tagihan.
    /// Diskon, adjustment, dan pembayaran berada di tingkat tagihan Billing dan tidak dipotong di sini.
    /// </para>
    /// </remarks>
    public class OptReportService
    {
        public const int MaxPeriodDays = 366;
        public const string ExamCategoryCode = "EXAM";
        public const string ExamCategoryLabel = "Pemeriksaan Refraksi";

        private static readonly OptOrderStatus[] StatusTerminal = [OptOrderStatus.HandedOver, OptOrderStatus.Cancelled, OptOrderStatus.NotCollected];

        private readonly ApplicationDbContext _dbContext;
        private readonly TimeProvider _time;

        public OptReportService(ApplicationDbContext dbContext, TimeProvider? timeProvider = null)
        {
            _dbContext = dbContext;
            _time = timeProvider ?? TimeProvider.System;
        }

        // ---------------------------------------------------------------- (1) kunjungan per pemeriksa

        public async Task<OptResult<OptVisitReportResponse>> GetVisitsByExaminerAsync(OptReportPeriodQuery query, CancellationToken cancellationToken = default)
        {
            var periode = ResolvePeriod(query);
            if (periode.Error is not null)
                return OptResult<OptVisitReportResponse>.Invalid(OptErrorCodes.InvalidRequest, periode.Error);
            var (awal, akhir) = Utc(periode.From, periode.To);

            var totalKunjungan = await (
                from e in _dbContext.Set<RegPatientEncounter>().AsNoTracking()
                join u in _dbContext.Set<MstServiceUnit>().AsNoTracking() on e.ServiceUnitId equals u.Id
                where u.ServiceUnitType == ServiceUnitType.Optical && !e.IsDelete && e.EncounterStatus != EncounterStatus.Cancelled
                      && e.EncounterDate >= awal && e.EncounterDate < akhir
                select e.Id).CountAsync(cancellationToken);

            var periksa = await _dbContext.OptRefractionExams.AsNoTracking()
                .Where(x => !x.IsDelete && x.Status == OptRefractionExamStatus.Final && x.FinalizedAt != null
                            && x.FinalizedAt >= awal && x.FinalizedAt < akhir)
                .Select(x => new { x.ExaminerWorkforceProfileId, x.EncounterId, x.CurrentRevisionNo })
                .ToListAsync(cancellationToken);

            var pemeriksaIds = periksa.Select(x => x.ExaminerWorkforceProfileId).Distinct().ToList();
            var nama = await _dbContext.Set<MstWorkforceProfile>().AsNoTracking()
                .Where(x => pemeriksaIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.DisplayName, cancellationToken);

            return OptResult<OptVisitReportResponse>.Ok(new OptVisitReportResponse
            {
                From = periode.From,
                To = periode.To,
                TotalVisits = totalKunjungan,
                TotalExams = periksa.Count,
                Examiners = periksa.GroupBy(x => x.ExaminerWorkforceProfileId)
                    .Select(g => new OptVisitReportRow
                    {
                        ExaminerWorkforceProfileId = g.Key,
                        ExaminerName = nama.GetValueOrDefault(g.Key) ?? string.Empty,
                        VisitCount = g.Select(x => x.EncounterId).Distinct().Count(),
                        ExamCount = g.Count(),
                        RevisedExamCount = g.Count(x => x.CurrentRevisionNo > 1),
                    })
                    .OrderByDescending(x => x.ExamCount).ThenBy(x => x.ExaminerName)
                    .ToList(),
            });
        }

        // ---------------------------------------------------------------- (2) penjualan

        public async Task<OptResult<OptSalesReportResponse>> GetSalesAsync(OptReportPeriodQuery query, CancellationToken cancellationToken = default)
        {
            var periode = ResolvePeriod(query);
            if (periode.Error is not null)
                return OptResult<OptSalesReportResponse>.Invalid(OptErrorCodes.InvalidRequest, periode.Error);
            var (awal, akhir) = Utc(periode.From, periode.To);

            var baris = await (
                from i in _dbContext.Set<BilInvoiceItem>().AsNoTracking()
                join inv in _dbContext.Set<BilInvoice>().AsNoTracking() on i.InvoiceId equals inv.Id
                join e in _dbContext.Set<RegPatientEncounter>().AsNoTracking() on inv.EncounterId equals e.Id
                where i.SourceDomain == BillingBridgeSourceDomains.Optical && i.Status == BillingInvoiceItemStatuses.Active && !i.IsDelete
                      && !inv.IsDelete && i.CreateDateTime >= awal && i.CreateDateTime < akhir
                select new { i.SourceDetailId, i.Quantity, i.UnitPrice, e.PaymentType })
                .ToListAsync(cancellationToken);

            var detailIds = baris.Select(x => Guid.TryParse(x.SourceDetailId, out var id) ? id : Guid.Empty).Where(x => x != Guid.Empty).Distinct().ToList();
            var kategoriBarang = await _dbContext.OptOrderItems.AsNoTracking()
                .Where(x => detailIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.ItemCategory, cancellationToken);

            string Kode(string detailId) =>
                Guid.TryParse(detailId, out var id) && kategoriBarang.TryGetValue(id, out var kategori) ? kategori.ToString() : ExamCategoryCode;
            string Label(string kode) =>
                Enum.TryParse<OptItemCategory>(kode, out var kategori) ? OptLabels.ItemCategory(kategori) : ExamCategoryLabel;

            return OptResult<OptSalesReportResponse>.Ok(new OptSalesReportResponse
            {
                From = periode.From,
                To = periode.To,
                LineCount = baris.Count,
                TotalAmount = baris.Sum(x => x.Quantity * x.UnitPrice),
                ByCategory = baris.GroupBy(x => Kode(x.SourceDetailId))
                    .Select(g => new OptSalesByCategoryRow
                    {
                        CategoryCode = g.Key,
                        CategoryLabel = Label(g.Key),
                        Quantity = g.Sum(x => x.Quantity),
                        Amount = g.Sum(x => x.Quantity * x.UnitPrice),
                    })
                    .OrderByDescending(x => x.Amount).ToList(),
                ByPaymentType = baris.GroupBy(x => x.PaymentType)
                    .Select(g => new OptSalesByPaymentRow
                    {
                        PaymentType = g.Key,
                        PaymentTypeLabel = PaymentTypeLabel(g.Key),
                        LineCount = g.Count(),
                        Amount = g.Sum(x => x.Quantity * x.UnitPrice),
                    })
                    .OrderBy(x => x.PaymentType).ToList(),
            });
        }

        // ---------------------------------------------------------------- (3) pesanan terbuka

        public async Task<OptResult<OptOpenOrderReportResponse>> GetOpenOrdersAsync(OptOpenOrderReportQuery query, CancellationToken cancellationToken = default)
        {
            var acuan = query.AsOf ?? DateOnly.FromDateTime(Today());
            var hariPengingat = await _dbContext.OptSettings.AsNoTracking().Where(x => !x.IsDelete)
                .OrderBy(x => x.CreateDateTime).Select(x => (int?)x.PickupReminderDays).FirstOrDefaultAsync(cancellationToken) ?? 30;

            var pesanan = await _dbContext.OptOrders.AsNoTracking()
                .Where(x => !x.IsDelete && !StatusTerminal.Contains(x.Status))
                .Select(x => new
                {
                    x.Id, x.OrderNumber, x.PatientId, x.OrderKind, x.Status, x.WorkshopType, x.ExternalLabSupplierId,
                    x.EstimatedReadyDate, x.ReadyAt, x.CreateDateTime,
                })
                .ToListAsync(cancellationToken);

            var pasienIds = pesanan.Select(x => x.PatientId).Distinct().ToList();
            var pasien = await _dbContext.Set<MstPatient>().AsNoTracking().Where(x => pasienIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => new { x.MedicalRecordNumber, x.FullName }, cancellationToken);
            var pemasokIds = pesanan.Where(x => x.ExternalLabSupplierId != null).Select(x => x.ExternalLabSupplierId!.Value).Distinct().ToList();
            var pemasok = await _dbContext.Set<MstSupplier>().AsNoTracking().Where(x => pemasokIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.SupplierName, cancellationToken);

            var rows = pesanan.Select(x =>
            {
                var siap = x.ReadyAt is { } r ? DateOnly.FromDateTime(OptPractitionerLicenseReader.ToOperationalDate(new DateTimeOffset(DateTime.SpecifyKind(r, DateTimeKind.Utc)))) : (DateOnly?)null;
                var terlambat = x.Status == OptOrderStatus.InFabrication && x.EstimatedReadyDate is { } e && e < acuan;
                var hubungi = x.Status == OptOrderStatus.ReadyForPickup && siap is { } s && s.AddDays(hariPengingat) <= acuan;
                return new OptOpenOrderRow
                {
                    OrderId = x.Id,
                    OrderNumber = x.OrderNumber,
                    MedicalRecordNumber = pasien.GetValueOrDefault(x.PatientId)?.MedicalRecordNumber ?? string.Empty,
                    FullName = pasien.GetValueOrDefault(x.PatientId)?.FullName ?? string.Empty,
                    OrderKind = x.OrderKind,
                    Status = x.Status,
                    StatusLabel = OptLabels.OrderStatus(x.Status),
                    WorkshopTypeLabel = x.WorkshopType is { } w ? OptLabels.WorkshopType(w) : null,
                    ExternalLabSupplierName = x.ExternalLabSupplierId is { } sid ? pemasok.GetValueOrDefault(sid) : null,
                    EstimatedReadyDate = x.EstimatedReadyDate,
                    ReadyDate = siap,
                    IsOverdue = terlambat,
                    DaysOverdue = terlambat ? acuan.DayNumber - x.EstimatedReadyDate!.Value.DayNumber : null,
                    NeedsContact = hubungi,
                    DaysSinceReady = siap is { } d ? acuan.DayNumber - d.DayNumber : null,
                };
            })
            .OrderByDescending(x => x.IsOverdue || x.NeedsContact).ThenByDescending(x => x.DaysOverdue ?? x.DaysSinceReady ?? 0).ThenBy(x => x.OrderNumber)
            .ToList();

            return OptResult<OptOpenOrderReportResponse>.Ok(new OptOpenOrderReportResponse
            {
                AsOf = acuan,
                PickupReminderDays = hariPengingat,
                OpenOrderCount = rows.Count,
                OverdueCount = rows.Count(x => x.IsOverdue),
                NeedsContactCount = rows.Count(x => x.NeedsContact),
                Orders = rows,
            });
        }

        // ---------------------------------------------------------------- (4) garansi

        public async Task<OptResult<OptWarrantyReportResponse>> GetWarrantyAsync(OptReportPeriodQuery query, CancellationToken cancellationToken = default)
        {
            var periode = ResolvePeriod(query);
            if (periode.Error is not null)
                return OptResult<OptWarrantyReportResponse>.Invalid(OptErrorCodes.InvalidRequest, periode.Error);
            var (awal, akhir) = Utc(periode.From, periode.To);

            var klaim = await _dbContext.OptWarrantyClaims.AsNoTracking()
                .Where(x => !x.IsDelete && x.CreateDateTime >= awal && x.CreateDateTime < akhir)
                .Select(x => new { x.ComplaintType, x.IsWithinWarranty, x.Status, x.RemakeOrderId })
                .ToListAsync(cancellationToken);
            var remakeIds = klaim.Where(x => x.RemakeOrderId != null).Select(x => x.RemakeOrderId!.Value).ToList();
            var remakeDiserahkan = await _dbContext.OptOrders.AsNoTracking()
                .CountAsync(x => remakeIds.Contains(x.Id) && x.Status == OptOrderStatus.HandedOver, cancellationToken);

            return OptResult<OptWarrantyReportResponse>.Ok(new OptWarrantyReportResponse
            {
                From = periode.From,
                To = periode.To,
                ClaimCount = klaim.Count,
                WithinWarrantyCount = klaim.Count(x => x.IsWithinWarranty),
                OutOfWarrantyCount = klaim.Count(x => !x.IsWithinWarranty),
                ApprovedCount = klaim.Count(x => x.Status == OptWarrantyClaimStatus.Approved),
                RejectedCount = klaim.Count(x => x.Status == OptWarrantyClaimStatus.Rejected),
                PendingCount = klaim.Count(x => x.Status is OptWarrantyClaimStatus.Submitted or OptWarrantyClaimStatus.Examined),
                RemakeCount = remakeIds.Count,
                RemakeHandedOverCount = remakeDiserahkan,
                ByComplaintType = klaim.GroupBy(x => x.ComplaintType)
                    .Select(g => new OptWarrantyByTypeRow
                    {
                        ComplaintType = g.Key,
                        ComplaintTypeLabel = OptLabels.ComplaintType(g.Key),
                        ClaimCount = g.Count(),
                        WithinWarrantyCount = g.Count(x => x.IsWithinWarranty),
                        RemakeCount = g.Count(x => x.RemakeOrderId != null),
                    })
                    .OrderBy(x => x.ComplaintType).ToList(),
            });
        }

        // ---------------------------------------------------------------- penolong

        private sealed record Periode(DateOnly From, DateOnly To, string? Error);

        private Periode ResolvePeriod(OptReportPeriodQuery query)
        {
            var hariIni = DateOnly.FromDateTime(Today());
            var dari = query.From ?? new DateOnly(hariIni.Year, hariIni.Month, 1);
            var sampai = query.To ?? hariIni;
            if (dari > sampai)
                return new Periode(dari, sampai, "Tanggal awal periode tidak boleh sesudah tanggal akhir.");
            if (sampai.DayNumber - dari.DayNumber + 1 > MaxPeriodDays)
                return new Periode(dari, sampai, $"Periode laporan paling panjang {MaxPeriodDays} hari.");
            return new Periode(dari, sampai, null);
        }

        /// <summary>Rentang UTC [awal hari pertama WIB, awal hari sesudah hari terakhir WIB).</summary>
        private static (DateTime Awal, DateTime Akhir) Utc(DateOnly dari, DateOnly sampai) => (
            Helpers.QuilvianSystemBackend.Helpers.AppDateTimeHelper.OperationalDateToUtc(dari.ToDateTime(TimeOnly.MinValue)),
            Helpers.QuilvianSystemBackend.Helpers.AppDateTimeHelper.OperationalDateToUtc(sampai.AddDays(1).ToDateTime(TimeOnly.MinValue)));

        private static string PaymentTypeLabel(EncounterPaymentType value) => value switch
        {
            EncounterPaymentType.Cash => "Tunai",
            EncounterPaymentType.Insurance => "Asuransi",
            EncounterPaymentType.CompanyGuarantor => "Penjamin Perusahaan",
            _ => "Tidak diketahui",
        };

        private DateTime Today() => OptPractitionerLicenseReader.ToOperationalDate(_time.GetUtcNow());
    }
}
