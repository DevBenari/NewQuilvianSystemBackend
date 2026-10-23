using Microsoft.Extensions.Options;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Services
{
    /// <summary>
    /// Menjalankan <see cref="McuNoShowService.CloseOverdueAsync"/> sekali saat aplikasi mulai (mengejar
    /// tanggal yang terlewat bila server mati), lalu setiap hari pada
    /// <see cref="McuNoShowOptions.NoShowCutoffLocalTime"/> WIB (<c>BE-MCU-012</c>). Polanya sama dengan
    /// <c>PatientMembershipLifecycleHostedService</c>, termasuk saklar <see cref="McuNoShowOptions.NoShowEnabled"/>.
    /// </summary>
    public class McuNoShowHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOptionsMonitor<McuNoShowOptions> _options;
        private readonly ILogger<McuNoShowHostedService> _logger;
        private readonly TimeProvider _timeProvider;

        public McuNoShowHostedService(
            IServiceScopeFactory scopeFactory,
            IOptionsMonitor<McuNoShowOptions> options,
            ILogger<McuNoShowHostedService> logger,
            TimeProvider? timeProvider = null)
        {
            _scopeFactory = scopeFactory;
            _options = options;
            _logger = logger;
            _timeProvider = timeProvider ?? TimeProvider.System;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.CurrentValue.NoShowEnabled)
            {
                _logger.LogInformation("Tutup hari MCU (Tidak Hadir) dinonaktifkan melalui konfigurasi.");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                var cutoff = _options.CurrentValue.NoShowCutoffLocalTime;

                try
                {
                    if (_options.CurrentValue.NoShowEnabled)
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var service = scope.ServiceProvider.GetRequiredService<McuNoShowService>();
                        var result = await service.CloseOverdueAsync(_timeProvider.GetUtcNow(), cutoff, stoppingToken);

                        _logger.LogInformation(
                            "Tutup hari MCU sampai {ClosedThrough}: {Marked} order menjadi Tidak Hadir, {Failures} gagal dan dicoba pada siklus berikutnya.",
                            result.ClosedThroughDate.ToString("dd-MM-yyyy"), result.MarkedNoShow, result.Failures);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Tutup hari MCU mengalami error; dicoba lagi pada jam tutup berikutnya.");
                }

                try
                {
                    await Task.Delay(DelayUntilNextRun(_timeProvider.GetUtcNow(), cutoff), _timeProvider, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Jeda sampai jam tutup berikutnya. Contoh: sekarang 27-10-2026 09.00 WIB dan jam tutup 23.59 →
        /// jeda 14 jam 59 menit.
        /// </summary>
        public static TimeSpan DelayUntilNextRun(DateTimeOffset nowUtc, TimeSpan cutoffLocalTime)
        {
            var timeZone = McuNoShowService.BusinessTimeZone;
            var localNow = TimeZoneInfo.ConvertTime(nowUtc, timeZone);
            var nextLocal = localNow.Date + cutoffLocalTime;
            if (nextLocal <= localNow.DateTime)
                nextLocal = nextLocal.AddDays(1);

            var nextUtc = new DateTimeOffset(nextLocal, timeZone.GetUtcOffset(nextLocal));
            var delay = nextUtc - nowUtc;
            return delay < TimeSpan.FromSeconds(1) ? TimeSpan.FromSeconds(1) : delay;
        }
    }
}
