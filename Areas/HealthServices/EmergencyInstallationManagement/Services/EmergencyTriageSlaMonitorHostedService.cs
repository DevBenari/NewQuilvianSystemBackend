using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services
{
    /// <summary>
    /// Memindai penilaian triage yang batas waktu responsnya sudah terlampaui sementara
    /// pasiennya belum ditangani, lalu menandainya.
    ///
    /// Tiga sifat wajib menurut integration contract dipenuhi di sini:
    /// idempotent (penilaian yang sudah bertanda tidak pernah disentuh lagi),
    /// tidak memblokir pelayanan (kegagalan hanya dicatat lalu pemindaian diulang),
    /// dan tidak mengubah data klinis (hanya dua kolom penanda yang ditulis).
    /// </summary>
    public class EmergencyTriageSlaMonitorHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly EmergencyTriageSlaMonitorOptions _options;
        private readonly ILogger<EmergencyTriageSlaMonitorHostedService> _logger;

        public EmergencyTriageSlaMonitorHostedService(
            IServiceScopeFactory scopeFactory,
            IOptions<EmergencyTriageSlaMonitorOptions> options,
            ILogger<EmergencyTriageSlaMonitorHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.Enabled)
            {
                _logger.LogInformation(
                    "Emergency Triage SLA Monitor dinonaktifkan melalui konfigurasi.");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var triageService = scope.ServiceProvider
                        .GetRequiredService<EmergencyTriageService>();

                    var ditandai = await triageService.MarkSlaBreachesAsync(
                        _options.BatchSize,
                        stoppingToken);

                    if (ditandai > 0)
                    {
                        _logger.LogInformation(
                            "Emergency Triage SLA Monitor menandai {Jumlah} penilaian melampaui batas waktu respons.",
                            ditandai);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // Kegagalan pemindaian tidak boleh merambat ke jalur pelayanan. Pemantau
                    // berjalan di proses latar sendiri, sehingga triage, penanganan, dan
                    // penyelesaian kunjungan tetap dilayani walaupun blok ini tercapai.
                    _logger.LogError(ex, "Emergency Triage SLA Monitor mengalami error.");
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(Math.Max(10, _options.PollIntervalSeconds)),
                    stoppingToken);
            }
        }
    }
}
