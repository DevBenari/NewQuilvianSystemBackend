using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingEvent.Services
{
    public class AccAccountingEventSchedulerHostedService : BackgroundService
    {
        private const int JedaPollingMinimumDetik = 15;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly AccAccountingEventSchedulerOptions _options;
        private readonly ILogger<AccAccountingEventSchedulerHostedService> _logger;

        public AccAccountingEventSchedulerHostedService(
            IServiceScopeFactory scopeFactory,
            IOptions<AccAccountingEventSchedulerOptions> options,
            ILogger<AccAccountingEventSchedulerHostedService> logger)
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
                    "Penjadwal coba ulang kejadian akuntansi dinonaktifkan melalui konfigurasi.");
                return;
            }

            var jeda = Math.Max(JedaPollingMinimumDetik, _options.PollIntervalSeconds);
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(jeda));

            _logger.LogInformation(
                "Penjadwal coba ulang kejadian akuntansi aktif. Poll={Jeda}s, Tenggang={Tenggang}s, Batas={Batas}.",
                jeda,
                _options.GracePeriodSeconds,
                AccAccountingEventService.BatasCobaUlangTerjadwal);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await JalankanSatuSiklusAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Siklus penjadwal coba ulang kejadian akuntansi gagal.");
                }

                if (!await timer.WaitForNextTickAsync(stoppingToken))
                {
                    break;
                }
            }
        }

        private async Task JalankanSatuSiklusAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<AccAccountingEventService>();

            var hasil = await service.CobaUlangTerjadwalAsync(DateTime.UtcNow, ct);

            if (hasil.Considered == 0) return;

            _logger.LogInformation(
                "Siklus coba ulang kejadian selesai. Diambil={Diambil}, Terjurnal={Terjurnal}, "
                + "Tertahan={Tertahan}, MasihDiterima={MasihDiterima}, MenjadiGagal={MenjadiGagal}, "
                + "Dilewati={Dilewati}, Galat={Galat}.",
                hasil.Considered,
                hasil.Journaled,
                hasil.Held,
                hasil.StillPending,
                hasil.MarkedFailed,
                hasil.Skipped,
                hasil.Errors.Count);

            foreach (var galat in hasil.Errors)
            {
                _logger.LogWarning("Coba ulang kejadian terjadwal bergalat: {Galat}", galat);
            }
        }
    }
}
