using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Services;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Workers
{
    /// <summary>
    /// Background Worker Harian Radar Kedaluwarsa Pelatihan Wajib STARKES KPS 8 (TRN-W1, VR-TRN-09)
    /// </summary>
    public class MandatoryTrainingExpiryRadarWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MandatoryTrainingExpiryRadarWorker> _logger;

        public MandatoryTrainingExpiryRadarWorker(
            IServiceProvider serviceProvider,
            ILogger<MandatoryTrainingExpiryRadarWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MandatoryTrainingExpiryRadarWorker (TRN-W1) diinisiasi dan mulai berjalan.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var radarService = scope.ServiceProvider.GetRequiredService<ITrainingExpiryRadarService>();

                    var summary = await radarService.GetExpiringRadarSummaryAsync(stoppingToken);

                    _logger.LogInformation(
                        "RADAR KEDALUWARSA DIKLAT (TRN-W1): Total {Total} sertifikat dipantau. Overdue: {Overdue}, H-30 Kritis: {H30}, H-60: {H60}, H-90: {H90}.",
                        summary.TotalMonitoredCertificates,
                        summary.OverdueH0Count,
                        summary.ZoneH30CriticalCount,
                        summary.ZoneH60Count,
                        summary.ZoneH90Count);

                    if (summary.OverdueH0Count > 0)
                    {
                        _logger.LogWarning("PERINGATAN KRITIS: Terdeteksi {Count} staf dengan sertifikat pelatihan wajib telah kedaluwarsa. Evaluasi penegakan Shift Buddy System (INT-HR-79) & eskalasi RKK (INT-HR-77) diperlukan.", summary.OverdueH0Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Terjadi kesalahan pada siklus pemindaian MandatoryTrainingExpiryRadarWorker.");
                }

                // Berjalan sekali sehari (atau interval 12 jam)
                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }
    }
}
