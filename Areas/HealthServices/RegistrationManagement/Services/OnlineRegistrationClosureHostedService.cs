using System.Globalization;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Services;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>
    /// Penutupan harian pendaftaran online Tidak Datang (kiosk Amendment PO, <c>BE-KSK-PO-008</c>,
    /// <c>KSK-DEC-037</c>, <c>KSK-DSN-PO-015</c>).
    /// </summary>
    /// <remarks>
    /// Pola disalin dari <c>OutpatientPriorityReservationClosureHostedService</c>, bukan memperluasnya,
    /// supaya kedua penutupan dapat dimatikan sendiri-sendiri. Putaran pertama berjalan saat aplikasi
    /// menyala, sehingga hari yang terlewat ketika server mati ikut diproses; berikutnya setiap hari pada
    /// <c>HealthServices:Registration:OnlineRegistration:Closure:RunAtLocalTime</c> WIB (bawaan 00:05).
    /// Tidak bergantung pada <c>OnlineRegistration:Enabled</c>: halaman publik boleh dimatikan, tetapi
    /// pendaftaran yang sudah ada tetap harus ditutup.
    /// </remarks>
    public class OnlineRegistrationClosureHostedService : BackgroundService
    {
        private static readonly TimeSpan DefaultRunAtLocalTime = new(0, 5, 0);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOptionsMonitor<OnlineRegistrationOptions> _options;
        private readonly ILogger<OnlineRegistrationClosureHostedService> _logger;
        private readonly TimeProvider _timeProvider;

        public OnlineRegistrationClosureHostedService(
            IServiceScopeFactory scopeFactory,
            IOptionsMonitor<OnlineRegistrationOptions> options,
            ILogger<OnlineRegistrationClosureHostedService> logger,
            TimeProvider? timeProvider = null)
        {
            _scopeFactory = scopeFactory;
            _options = options;
            _logger = logger;
            _timeProvider = timeProvider ?? TimeProvider.System;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!IsEnabled())
            {
                _logger.LogInformation("Penutupan pendaftaran online Tidak Datang dinonaktifkan melalui konfigurasi.");
                return;
            }

            var timeZone = ResolveBusinessTimeZone();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (IsEnabled())
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var service = scope.ServiceProvider.GetRequiredService<OnlineRegistrationService>();
                        var closed = await service.CloseExpiredAsync(stoppingToken);
                        _logger.LogInformation(
                            "Penutupan pendaftaran online selesai: {Closed} pendaftaran ditutup sebagai Tidak Datang.",
                            closed);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Penutupan pendaftaran online Tidak Datang mengalami error.");
                }

                try
                {
                    await Task.Delay(
                        PatientMembershipLifecycleHostedService.DelayUntilNextRun(
                            _timeProvider.GetUtcNow(),
                            timeZone,
                            ResolveRunAtLocalTime(_options.CurrentValue.Closure.RunAtLocalTime)),
                        _timeProvider,
                        stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private bool IsEnabled() => _options.CurrentValue.Closure.Enabled;

        public static TimeSpan ResolveRunAtLocalTime(string? value) =>
            TimeSpan.TryParseExact(value?.Trim(), @"hh\:mm", CultureInfo.InvariantCulture, out var parsed) &&
            parsed >= TimeSpan.Zero && parsed < TimeSpan.FromDays(1)
                ? parsed
                : DefaultRunAtLocalTime;

        private static TimeZoneInfo ResolveBusinessTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.CreateCustomTimeZone("WIB", TimeSpan.FromHours(7), "WIB", "WIB");
            }
        }
    }
}
