using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Workers
{
    /// <summary>
    /// Worker TRV-W1: Pengingat Penyerahan Settlement Perjalanan Dinas H+3 &amp; H+5 (BE-HRD-328, VR-TRV-05)
    /// </summary>
    /// <remarks>
    /// Remediasi Gelombang 2 (10 November 2026): pengingat sebelumnya hanya ditulis ke log. Kini dikirim ke pusat
    /// pemberitahuan dalam aplikasi untuk pegawai yang melakukan perjalanan (H+3 dan H+5), satu kali per SPPD per jendela.
    /// </remarks>
    public class TravelAdvanceSettlementReminderWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TravelAdvanceSettlementReminderWorker> _logger;

        public TravelAdvanceSettlementReminderWorker(IServiceProvider serviceProvider, ILogger<TravelAdvanceSettlementReminderWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("TravelAdvanceSettlementReminderWorker (TRV-W1) dimulai.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var notifier = scope.ServiceProvider.GetRequiredService<IHrdNotificationService>();

                    var today = DateOnly.FromDateTime(DateTime.UtcNow);

                    // Cari perjalanan dinas yang sudah selesai tapi settlement belum diajukan/selesai
                    var pendingTravels = await context.TrxBusinessTravelRequests
                        .AsNoTracking()
                        .Include(r => r.Settlements)
                        .Where(r => r.IsActive && r.TravelStatus != "Completed" && r.TravelStatus != "Cancelled" && r.TravelStatus != "Rejected")
                        .Where(r => r.EndDate < today)
                        .ToListAsync(stoppingToken);

                    foreach (var travel in pendingTravels)
                    {
                        var completedSettlement = travel.Settlements.Any(s => s.SettlementStatus == "Completed");
                        if (completedSettlement) continue;

                        var daysAfterReturn = today.DayNumber - travel.EndDate.DayNumber;
                        if (daysAfterReturn != 3 && daysAfterReturn != 5) continue;

                        var sent = await notifier.NotifyWorkforceProfileAsync(
                            travel.WorkforceProfileId,
                            new HrdNotificationRequest(
                                Title: $"Pengingat settlement SPPD {travel.TravelRequestNumber} (H+{daysAfterReturn})",
                                Body: $"Perjalanan dinas {travel.TravelRequestNumber} berakhir pada {travel.EndDate:dd MMMM yyyy}. Pertanggungjawaban (settlement) belum diserahkan; batas waktunya 7 hari setelah kepulangan (VR-TRV-05). Lewat 14 hari, hak pembuatan SPPD baru dibekukan.",
                                Category: HrdNotificationCategories.Travel,
                                Priority: daysAfterReturn >= 5 ? HrdNotificationPriorities.High : HrdNotificationPriorities.Normal,
                                DeepLinkUrl: $"/hrd/business-travel/requests/{travel.Id}/settlement",
                                SourceModule: "Hrd.BusinessTravel.TRV-W1",
                                ReferenceType: "TrxBusinessTravelRequest",
                                ReferenceId: travel.Id,
                                DedupKey: $"TRV-W1|{travel.Id:N}|H{daysAfterReturn}"),
                            stoppingToken);

                        if (sent > 0)
                        {
                            _logger.LogInformation("[TRV-W1 Peringatan H+{Days}] Pengingat settlement SPPD {Number} dikirim ke {Count} akun pegawai.", daysAfterReturn, travel.TravelRequestNumber, sent);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kesalahan pada TravelAdvanceSettlementReminderWorker.");
                }

                // Berjalan setiap 4 jam
                await Task.Delay(TimeSpan.FromHours(4), stoppingToken);
            }
        }
    }

    /// <summary>
    /// Worker TRV-W2: Penegakan Disiplin Settlement Overdue, Pembekuan SPPD H+14 &amp; Potong Payroll H+30 (BE-HRD-329, INT-HR-99, VR-TRV-14, VR-TRV-15)
    /// </summary>
    public class TravelSettlementOverdueWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TravelSettlementOverdueWorker> _logger;

        public TravelSettlementOverdueWorker(IServiceProvider serviceProvider, ILogger<TravelSettlementOverdueWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("TravelSettlementOverdueWorker (TRV-W2) dimulai.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var today = DateOnly.FromDateTime(DateTime.UtcNow);
                    var now = DateTime.UtcNow;

                    var travels = await context.TrxBusinessTravelRequests
                        .Include(r => r.Settlements)
                        .Include(r => r.AdvanceRequests)
                        .Where(r => r.IsActive && r.TravelStatus != "Completed" && r.TravelStatus != "Cancelled" && r.TravelStatus != "Rejected")
                        .Where(r => r.EndDate < today)
                        .ToListAsync(stoppingToken);

                    foreach (var travel in travels)
                    {
                        var daysAfter = today.DayNumber - travel.EndDate.DayNumber;
                        var stl = travel.Settlements.OrderByDescending(s => s.CreateDateTime).FirstOrDefault();

                        if (daysAfter > 7)
                        {
                            // Tandai IsOverdue = true
                            if (stl != null && !stl.IsOverdue && stl.SettledAt == null)
                            {
                                stl.IsOverdue = true;
                                stl.UpdateBy = Guid.Empty;
                                stl.UpdateDateTime = now;
                            }
                        }

                        if (daysAfter > 14)
                        {
                            // VR-TRV-14: Pembekuan hak pembuatan SPPD baru
                            if (stl != null && stl.DefaultedAt == null)
                            {
                                stl.DefaultedAt = now;
                                stl.UpdateBy = Guid.Empty;
                                stl.UpdateDateTime = now;
                                _logger.LogWarning("[TRV-W2 Freeze] Hak SPPD pegawai {ProfileId} dibekukan akibat tunggakan settlement > 14 hari.", travel.WorkforceProfileId);
                            }
                        }

                        if (daysAfter > 30)
                        {
                            // VR-TRV-15 & INT-HR-99: Meneruskan ke modul payroll Sub-10 untuk pemotongan piutang
                            var adv = travel.AdvanceRequests.FirstOrDefault(a => a.AdvanceStatus == "Paid");
                            if (adv != null && adv.PaidAmount > 0)
                            {
                                await ScheduleOverdueAdvanceDeductionAsync(scope.ServiceProvider, context, travel, adv, stl, stoppingToken);
                            }
                        }
                    }

                    await context.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kesalahan pada TravelSettlementOverdueWorker.");
                }

                // Berjalan setiap 6 jam
                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
        }

        /// <summary>
        /// VR-TRV-15 / INT-HR-99: membuat jadwal potongan piutang uang muka pada payroll, satu kali per uang muka.
        /// Mesin potongan payroll menegakkan batas 50% gaji dan membawa sisanya ke periode berikut.
        /// </summary>
        private async Task ScheduleOverdueAdvanceDeductionAsync(
            IServiceProvider services,
            ApplicationDbContext context,
            Models.TrxBusinessTravelRequest travel,
            Models.TrxTravelAdvanceRequest advance,
            Models.TrxTravelSettlement? settlement,
            CancellationToken cancellationToken)
        {
            // Sesudah settlement diverifikasi, piutangnya hanya selisih lebih; sebelum itu seluruh uang muka.
            var outstanding = settlement?.VerifiedAt != null ? settlement.EmployeeRefundAmount : advance.PaidAmount;
            if (outstanding <= 0) return;

            var externalReference = $"TRV-ADV-{advance.Id:N}";
            var alreadyScheduled = await context.TrxPayrollDeductionSchedules
                .AnyAsync(s => s.ExternalReferenceId == externalReference && !s.IsDelete, cancellationToken);
            if (alreadyScheduled) return;

            var employeeId = travel.EmployeeId ?? await context.MstEmployees.AsNoTracking()
                .Where(e => e.WorkforceProfileId == travel.WorkforceProfileId && !e.IsDelete)
                .Select(e => (Guid?)e.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (employeeId == null)
            {
                _logger.LogError("[TRV-W2] SPPD {Number} tidak punya data karyawan; potongan payroll tidak dapat dijadwalkan.", travel.TravelRequestNumber);
                return;
            }

            var allocator = services.GetRequiredService<NumberSeriesAllocator>();
            var scheduleNumber = await allocator.AllocateAsync(new NumberAllocationRequest(
                SequenceKey: BusinessTravelNumberSeries.PayrollDeductionKey,
                Prefix: BusinessTravelNumberSeries.PayrollDeductionPrefix,
                ResetPolicy: BusinessTravelNumberSeries.PayrollDeductionResetPolicy,
                SequenceDigits: BusinessTravelNumberSeries.SequenceDigits,
                ActorUserId: BusinessTravelNumberSeries.SystemFallbackUserId,
                Instant: DateTimeOffset.UtcNow), cancellationToken);

            var now = DateTime.UtcNow;
            context.TrxPayrollDeductionSchedules.Add(new TrxPayrollDeductionSchedule
            {
                Id = Guid.NewGuid(),
                ScheduleNumber = scheduleNumber,
                EmployeeId = employeeId.Value,
                DeductionCategory = "KasbonPinjamanRS",
                PriorityLevel = 2,
                ExternalReferenceId = externalReference,
                TotalLoanPrincipal = outstanding,
                MonthlyInstallmentAmount = outstanding,
                TotalInstallmentMonths = 1,
                PaidInstallmentCount = 0,
                RemainingPrincipal = outstanding,
                StartDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                ScheduleStatus = "Active",
                AutoCarryOverOnCapExceeded = true,
                CreateDateTime = now
            });

            var notifier = services.GetRequiredService<IHrdNotificationService>();
            await notifier.NotifyWorkforceProfileAsync(
                travel.WorkforceProfileId,
                new HrdNotificationRequest(
                    Title: $"Uang muka SPPD {travel.TravelRequestNumber} dijadwalkan potong gaji",
                    Body: $"Settlement SPPD {travel.TravelRequestNumber} menunggak lebih dari 30 hari. Piutang {outstanding:N0} IDR dijadwalkan dipotong dari payroll (VR-TRV-15), dengan batas potongan 50% gaji per bulan.",
                    Category: HrdNotificationCategories.Travel,
                    Priority: HrdNotificationPriorities.Critical,
                    DeepLinkUrl: $"/hrd/business-travel/requests/{travel.Id}/settlement",
                    SourceModule: "Hrd.BusinessTravel.TRV-W2",
                    ReferenceType: "TrxBusinessTravelRequest",
                    ReferenceId: travel.Id,
                    DedupKey: $"TRV-W2-DEDUCT|{advance.Id:N}"),
                cancellationToken);

            _logger.LogWarning("[TRV-W2 Potong Payroll INT-HR-99] Jadwal {Schedule} dibuat untuk SPPD {Number}, piutang {Amount:N0} IDR.", scheduleNumber, travel.TravelRequestNumber, outstanding);
        }
    }

    /// <summary>
    /// Worker TRV-W4: Sinkronisasi Kurs Perjalanan Internasional dari Master Kurs Keuangan (BE-HRD-333, HRD-ASM-72)
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Keputusan sumber kurs (Gelombang 2, 10 November 2026):</b> master kurs milik modul Keuangan
    /// (<c>MstExchangeRate</c>, kurs tengah per tanggal) dipakai sebagai satu-satunya sumber, bukan pemanggilan API
    /// Bank Indonesia yang belum tersedia. Petugas Keuangan memasukkan kurs tengah BI ke master itu; worker ini
    /// menerapkannya ke butir klaim perjalanan dinas bermata uang asing yang belum dikonversi.
    /// </para>
    /// <para>
    /// Untuk setiap <c>TrxTravelExpenseItem</c> berstatus <c>Draft</c> atau <c>Submitted</c> dengan mata uang selain
    /// mata uang dasar dan <c>ExchangeRate</c> masih 1 (belum dikonversi), worker mengambil kurs tengah pada tanggal
    /// transaksi (atau kurs terakhir sebelum tanggal itu) lalu mengisi <c>ExchangeRate</c> dan
    /// <c>BaseCurrencyAmount</c>. Butir yang kursnya belum ada di master dilewati dan dilaporkan sebagai pemberitahuan
    /// ke pemegang hak akses <c>TravelRatePolicy : ManageRate</c>.
    /// </para>
    /// </remarks>
    public class TravelCurrencyRateSyncWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TravelCurrencyRateSyncWorker> _logger;

        public TravelCurrencyRateSyncWorker(IServiceProvider serviceProvider, ILogger<TravelCurrencyRateSyncWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("TravelCurrencyRateSyncWorker (TRV-W4) dimulai.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var notifier = scope.ServiceProvider.GetRequiredService<IHrdNotificationService>();

                    var result = await ApplyMasterRatesAsync(context, stoppingToken);

                    if (result.Converted > 0)
                    {
                        _logger.LogInformation("[TRV-W4 Kurs] {Converted} butir klaim dinas dikonversi memakai kurs tengah master Keuangan.", result.Converted);
                    }

                    if (result.MissingCurrencies.Count > 0)
                    {
                        var missing = string.Join(", ", result.MissingCurrencies.OrderBy(x => x));
                        await notifier.NotifyPermissionHoldersAsync(
                            "TravelRatePolicy",
                            "ManageRate",
                            new HrdNotificationRequest(
                                Title: "Kurs belum tersedia untuk klaim perjalanan dinas",
                                Body: $"Sebanyak {result.Skipped} butir klaim perjalanan dinas bermata uang {missing} belum dapat dikonversi karena master kurs Keuangan belum memuat kurs tengah pada tanggal transaksinya (HRD-ASM-72). Mohon lengkapi master kurs.",
                                Category: HrdNotificationCategories.Travel,
                                Priority: HrdNotificationPriorities.Normal,
                                DeepLinkUrl: "/finance/master/exchange-rates",
                                SourceModule: "Hrd.BusinessTravel.TRV-W4",
                                DedupKey: $"TRV-W4|MISSING|{missing}|{DateTime.UtcNow:yyyyMMdd}"),
                            stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kesalahan pada TravelCurrencyRateSyncWorker.");
                }

                // Berjalan harian (setiap 24 jam)
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private static async Task<(int Converted, int Skipped, HashSet<string> MissingCurrencies)> ApplyMasterRatesAsync(ApplicationDbContext context, CancellationToken ct)
        {
            var baseCurrencyCode = await context.MstCurrencies
                .AsNoTracking()
                .Where(c => c.IsBaseCurrency && c.IsActive && !c.IsDelete)
                .Select(c => c.CurrencyCode)
                .FirstOrDefaultAsync(ct) ?? "IDR";

            var pendingItems = await context.TrxTravelExpenseItems
                .Where(i => i.IsActive && !i.IsDelete
                            && (i.ItemStatus == "Draft" || i.ItemStatus == "Submitted")
                            && i.CurrencyCode != baseCurrencyCode
                            && i.ExchangeRate == 1m)
                .ToListAsync(ct);

            var converted = 0;
            var skipped = 0;
            var missing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (pendingItems.Count == 0)
            {
                return (converted, skipped, missing);
            }

            var currencyCodes = pendingItems.Select(i => i.CurrencyCode.ToUpperInvariant()).Distinct().ToList();
            var currencies = await context.MstCurrencies
                .AsNoTracking()
                .Where(c => currencyCodes.Contains(c.CurrencyCode.ToUpper()) && !c.IsDelete)
                .Select(c => new { c.Id, Code = c.CurrencyCode.ToUpper() })
                .ToListAsync(ct);
            var currencyIds = currencies.Select(c => c.Id).ToList();

            var rates = await context.MstExchangeRates
                .AsNoTracking()
                .Where(r => currencyIds.Contains(r.CurrencyId) && !r.IsDelete && r.MiddleRate > 0)
                .Select(r => new { r.CurrencyId, r.RateDate, r.MiddleRate })
                .ToListAsync(ct);

            var now = DateTime.UtcNow;
            foreach (var item in pendingItems)
            {
                var code = item.CurrencyCode.ToUpperInvariant();
                var currency = currencies.FirstOrDefault(c => c.Code == code);
                var rate = currency == null
                    ? null
                    : rates.Where(r => r.CurrencyId == currency.Id && r.RateDate <= item.ExpenseDate)
                        .OrderByDescending(r => r.RateDate)
                        .FirstOrDefault();

                if (rate == null)
                {
                    skipped++;
                    missing.Add(code);
                    continue;
                }

                item.ExchangeRate = rate.MiddleRate;
                item.BaseCurrencyAmount = Math.Round(item.ClaimedAmount * rate.MiddleRate, 2, MidpointRounding.AwayFromZero);
                item.UpdateDateTime = now;
                converted++;
            }

            if (converted > 0)
            {
                await context.SaveChangesAsync(ct);
            }

            return (converted, skipped, missing);
        }
    }

    /// <summary>
    /// Worker Pelepasan Gembok Roster Otomatis Pasca Kepulangan Dinas Nakes
    /// </summary>
    public class TravelRosterLockExpiryWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TravelRosterLockExpiryWorker> _logger;

        public TravelRosterLockExpiryWorker(IServiceProvider serviceProvider, ILogger<TravelRosterLockExpiryWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("TravelRosterLockExpiryWorker dimulai.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var lockService = scope.ServiceProvider.GetRequiredService<ITravelAttendanceAndRosterLockService>();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var today = DateOnly.FromDateTime(DateTime.UtcNow);

                    var completedTravels = await context.TrxBusinessTravelRequests
                        .Where(r => r.IsActive && r.HasScheduleConflict && r.EndDate < today)
                        .Select(r => r.Id)
                        .ToListAsync(stoppingToken);

                    foreach (var travelId in completedTravels)
                    {
                        await lockService.ReleaseRosterLockAsync(travelId);
                        _logger.LogInformation("[RosterLock Release] Gembok roster SPPD {Id} dilepas pasca berakhirnya dinas luar.", travelId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kesalahan pada TravelRosterLockExpiryWorker.");
                }

                // Berjalan setiap 1 jam
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
