using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Services;
using System.Globalization;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>
    /// Menutup reservasi prioritas yang tidak datang (rawat-jalan Amendment RP, <c>RJ-PRI-DEC-013</c>,
    /// <c>INT-RP-08</c>). Jalan sekali saat aplikasi mulai (mengejar hari yang terlewat bila server
    /// mati), lalu setiap hari pada <c>Closure:RunAtLocalTime</c> WIB (bawaan 00.05).
    /// </summary>
    /// <remarks>
    /// Didaftarkan di blok proses terjadwal <c>Program.cs</c>, sehingga hanya berjalan pada
    /// <c>Runtime:Role</c> <c>Worker</c>/<c>All</c>. Proses ini tidak mengirim kabar realtime karena
    /// container Worker tidak memegang SignalR; layar admisi memuat ulang daftar.
    /// </remarks>
    public class OutpatientPriorityReservationClosureHostedService : BackgroundService
    {
        private static readonly TimeSpan DefaultRunAtLocalTime = new(0, 5, 0);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOptionsMonitor<PriorityReservationOptions> _options;
        private readonly ILogger<OutpatientPriorityReservationClosureHostedService> _logger;
        private readonly TimeProvider _timeProvider;

        public OutpatientPriorityReservationClosureHostedService(
            IServiceScopeFactory scopeFactory,
            IOptionsMonitor<PriorityReservationOptions> options,
            ILogger<OutpatientPriorityReservationClosureHostedService> logger,
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
                _logger.LogInformation("Penutupan reservasi prioritas Tidak Datang dinonaktifkan melalui konfigurasi.");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                TimeZoneInfo timeZone;

                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<OutpatientPriorityReservationService>();
                    timeZone = scope.ServiceProvider.GetRequiredService<PatientMembershipStatusResolver>().TimeZone;

                    if (IsEnabled())
                    {
                        var closed = await service.CloseExpiredAsync(stoppingToken);
                        _logger.LogInformation(
                            "Penutupan reservasi prioritas selesai: {Closed} reservasi ditutup sebagai Tidak Datang.",
                            closed);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Penutupan reservasi prioritas Tidak Datang mengalami error.");
                    timeZone = TimeZoneInfo.CreateCustomTimeZone("WIB", TimeSpan.FromHours(7), "WIB", "WIB");
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

        private bool IsEnabled() => _options.CurrentValue.Enabled && _options.CurrentValue.Closure.Enabled;

        /// <summary>Format <c>HH:mm</c>; nilai kosong atau salah memakai 00.05.</summary>
        public static TimeSpan ResolveRunAtLocalTime(string? value) =>
            TimeSpan.TryParseExact(value?.Trim(), @"hh\:mm", CultureInfo.InvariantCulture, out var parsed) &&
            parsed >= TimeSpan.Zero && parsed < TimeSpan.FromDays(1)
                ? parsed
                : DefaultRunAtLocalTime;
    }
}
