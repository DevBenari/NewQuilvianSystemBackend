using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Workers
{
    /// <summary>
    /// Background Worker Pemantauan Batas Proctoring FPPE & Evaluasi Anomali Komplikasi (OPP-W3, OPP-W6)
    /// </summary>
    public class FppeProctoringMonitoringWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FppeProctoringMonitoringWorker> _logger;

        public FppeProctoringMonitoringWorker(
            IServiceProvider serviceProvider,
            ILogger<FppeProctoringMonitoringWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("FppeProctoringMonitoringWorker diinisiasi dan mulai berjalan.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    await CheckProctoringDeadlinesAsync(context, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Terjadi kesalahan pada siklus pemantauan FppeProctoringMonitoringWorker.");
                }

                // Berjalan setiap 12 jam (atau 1 jam di staging)
                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }

        private async Task CheckProctoringDeadlinesAsync(ApplicationDbContext context, CancellationToken stoppingToken)
        {
            var now = DateTime.UtcNow;
            var h14Threshold = now.AddDays(14);

            // Cari penugasan FPPE aktif yang batas waktunya tersisa <= 14 hari
            var expiringEngagements = await context.TrxFppeEngagements
                .Where(e => (e.Status == "ACTIVE" || e.Status == "EXTENDED") && e.TargetEndDate <= h14Threshold)
                .ToListAsync(stoppingToken);

            foreach (var eng in expiringEngagements)
            {
                var remainingDays = (int)Math.Max(0, (eng.TargetEndDate - now).TotalDays);
                _logger.LogWarning("FPPE DEADLINE ALERT (OPP-W3): Penugasan FPPE No. {EngagementNumber} tersisa {Days} hari (Target: {TargetCount} kasus, Selesai: {CompletedCount} kasus).",
                    eng.EngagementNumber, remainingDays, eng.TargetCaseCount, eng.CompletedCaseCount);
            }
        }
    }
}
