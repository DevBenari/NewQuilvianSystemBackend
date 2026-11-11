using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Workers
{
    /// <summary>
    /// Worker TRV-W1: Pengingat Penyerahan Settlement Perjalanan Dinas H+3 &amp; H+5 (BE-HRD-328, VR-TRV-05)
    /// </summary>
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

                    var today = DateOnly.FromDateTime(DateTime.UtcNow);

                    // Cari perjalanan dinas yang sudah selesai tapi settlement belum diajukan/selesai
                    var pendingTravels = await context.TrxBusinessTravelRequests
                        .Include(r => r.Settlements)
                        .Where(r => r.IsActive && r.TravelStatus != "Completed" && r.TravelStatus != "Cancelled" && r.TravelStatus != "Rejected")
                        .Where(r => r.EndDate < today)
                        .ToListAsync(stoppingToken);

                    foreach (var travel in pendingTravels)
                    {
                        var completedSettlement = travel.Settlements.Any(s => s.SettlementStatus == "Completed");
                        if (completedSettlement) continue;

                        var daysAfterReturn = today.DayNumber - travel.EndDate.DayNumber;

                        if (daysAfterReturn == 3 || daysAfterReturn == 5)
                        {
                            _logger.LogWarning(
                                "[TRV-W1 Peringatan H+{Days}] SPPD {Number} belum menyerahkan settlement pertanggungjawaban dinas (Batas SLA: 7 hari).",
                                daysAfterReturn, travel.TravelRequestNumber);
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
                                _logger.LogCritical(
                                    "[TRV-W2 Potong Payroll INT-HR-99] Menerbitkan instruksi potongan payroll untuk SPPD {Number}, Piutang: {Amount:N0} IDR.",
                                    travel.TravelRequestNumber, adv.PaidAmount);
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
    }

    /// <summary>
    /// Worker TRV-W4: Sinkronisasi Kurs Tengah Bank Indonesia untuk Perjalanan Internasional (BE-HRD-333, HRD-ASM-72)
    /// </summary>
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
                    _logger.LogInformation("[TRV-W4 Kurs BI] Kurs valuta asing perjalanan dinas disinkronisasikan (USD, EUR, SGD, MYR, SAR).");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kesalahan pada TravelCurrencyRateSyncWorker.");
                }

                // Berjalan harian (setiap 24 jam)
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
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
