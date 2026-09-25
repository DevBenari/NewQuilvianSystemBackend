using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>
    /// Menjalankan <see cref="KioskEncounterClosureService"/> satu kali setiap hari, sesudah
    /// hari layanan berakhir (<c>LAB-DEC-058</c>, <c>LAB-DEC-059</c>).
    ///
    /// <b>Penutupan berjalan sesudah pukulnya lewat, bukan tepat pada pukulnya.</b> Penjadwal
    /// yang menuntut ketepatan detik akan melewatkan harinya begitu aplikasi kebetulan sedang
    /// mati pada pukul itu, dan kunjungan hari itu menggantung selamanya. Karena itu syaratnya
    /// ditulis sebagai "hari ini belum ditutup <b>dan</b> pukulnya sudah lewat", dan
    /// <see cref="KioskEncounterClosureOptions.LookBackDays"/> ikut menyapu hari yang benar-benar
    /// terlewat.
    /// </summary>
    public class KioskEncounterClosureHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOptionsMonitor<KioskEncounterClosureOptions> _options;
        private readonly ILogger<KioskEncounterClosureHostedService> _logger;
        private DateOnly? _lastClosedServiceDate;

        public KioskEncounterClosureHostedService(
            IServiceScopeFactory scopeFactory,
            IOptionsMonitor<KioskEncounterClosureOptions> options,
            ILogger<KioskEncounterClosureHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.CurrentValue.Enabled)
            {
                _logger.LogInformation(
                    "Penutupan otomatis kunjungan kiosk dinonaktifkan melalui konfigurasi.");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                var options = _options.CurrentValue;

                try
                {
                    if (options.Enabled)
                    {
                        await RunOnceIfDueAsync(options, stoppingToken);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Penutupan otomatis kunjungan kiosk mengalami error.");
                }

                try
                {
                    await Task.Delay(
                        TimeSpan.FromSeconds(Math.Clamp(options.PollIntervalSeconds, 30, 3600)),
                        stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async Task RunOnceIfDueAsync(
            KioskEncounterClosureOptions options,
            CancellationToken stoppingToken)
        {
            // Hari layanan dihitung dalam WIB, sama seperti seluruh tanggal operasional yang
            // lain, supaya batas 21:00 berarti hal yang sama di mana pun aplikasi berjalan.
            var localNow = AppDateTimeHelper.LocalNow();
            var serviceDate = DateOnly.FromDateTime(localNow.Date);

            if (_lastClosedServiceDate == serviceDate)
            {
                return;
            }

            var cutoff = new TimeSpan(
                Math.Clamp(options.ServiceDayEndHour, 0, 23),
                Math.Clamp(options.ServiceDayEndMinute, 0, 59),
                0);

            if (localNow.TimeOfDay < cutoff)
            {
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var closureService = scope.ServiceProvider
                .GetRequiredService<KioskEncounterClosureService>();

            var result = await closureService.CloseAbandonedAsync(
                localNow.Date,
                options,
                stoppingToken);

            // Ditandai hanya sesudah putarannya benar-benar selesai. Bila ia melempar, hari ini
            // belum tertandai dan putaran berikutnya mencobanya lagi.
            _lastClosedServiceDate = serviceDate;

            if (result.TotalClosed > 0)
            {
                _logger.LogInformation(
                    "Penutupan otomatis kunjungan kiosk {ServiceDate}: {Total} kunjungan ditutup ({Detail}).",
                    serviceDate,
                    result.TotalClosed,
                    string.Join(", ", result.ClosedByTarget.Select(x => $"{x.Key}={x.Value}")));
            }
        }
    }
}
