using Microsoft.Extensions.Options;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Services
{
    /// <summary>Konfigurasi <c>PatientMembership:Lifecycle</c>.</summary>
    public sealed class PatientMembershipLifecycleOptions
    {
        public const string SectionName = "PatientMembership:Lifecycle";

        /// <summary>
        /// Bawaan menyala: kedaluwarsa dan batal otomatis adalah aturan bisnis inti (CRM-DEC-005,
        /// CRM-DEC-014). Dimatikan hanya bila worker lain yang menjalankannya.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>Jam jalan harian menurut zona <c>PatientMembership:TimeZone</c> (bawaan 00.05 WIB).</summary>
        public TimeSpan RunAtLocalTime { get; set; } = new(0, 5, 0);
    }

    /// <summary>
    /// Menjalankan <see cref="PatientMembershipService.RunDailyLifecycleAsync"/> sekali saat aplikasi
    /// mulai (mengejar hari yang terlewat bila server mati), lalu setiap hari pada
    /// <see cref="PatientMembershipLifecycleOptions.RunAtLocalTime"/> (crm-membership `BE-CRM-020`).
    /// </summary>
    public class PatientMembershipLifecycleHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOptionsMonitor<PatientMembershipLifecycleOptions> _options;
        private readonly ILogger<PatientMembershipLifecycleHostedService> _logger;
        private readonly TimeProvider _timeProvider;

        public PatientMembershipLifecycleHostedService(
            IServiceScopeFactory scopeFactory,
            IOptionsMonitor<PatientMembershipLifecycleOptions> options,
            ILogger<PatientMembershipLifecycleHostedService> logger,
            TimeProvider? timeProvider = null)
        {
            _scopeFactory = scopeFactory;
            _options = options;
            _logger = logger;
            _timeProvider = timeProvider ?? TimeProvider.System;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.CurrentValue.Enabled)
            {
                _logger.LogInformation("Pekerjaan harian membership dinonaktifkan melalui konfigurasi.");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                TimeZoneInfo timeZone;

                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<PatientMembershipService>();
                    var resolver = scope.ServiceProvider.GetRequiredService<PatientMembershipStatusResolver>();
                    timeZone = resolver.TimeZone;

                    if (_options.CurrentValue.Enabled)
                    {
                        var result = await service.RunDailyLifecycleAsync(stoppingToken);
                        _logger.LogInformation(
                            "Pekerjaan harian membership selesai: kedaluwarsa {Expired}, Terjadwal mulai {Started}, batal tidak dibayar {Unpaid}, proyeksi diselaraskan {Synced}, gagal {Failures}.",
                            result.Expired, result.ScheduledStarted, result.UnpaidCancelled, result.ProjectionsSynced, result.Failures);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Pekerjaan harian membership mengalami error.");
                    timeZone = TimeZoneInfo.CreateCustomTimeZone("WIB", TimeSpan.FromHours(7), "WIB", "WIB");
                }

                try
                {
                    await Task.Delay(
                        DelayUntilNextRun(_timeProvider.GetUtcNow(), timeZone, _options.CurrentValue.RunAtLocalTime),
                        _timeProvider,
                        stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Jeda sampai jam jalan berikutnya. Contoh: sekarang 1 Des 2026 09.00 WIB dan jam jalan
        /// 00.05 → jeda sampai 2 Des 2026 00.05 WIB (15 jam 5 menit).
        /// </summary>
        public static TimeSpan DelayUntilNextRun(DateTimeOffset nowUtc, TimeZoneInfo timeZone, TimeSpan runAtLocalTime)
        {
            var localNow = TimeZoneInfo.ConvertTime(nowUtc, timeZone);
            var nextLocal = localNow.Date + runAtLocalTime;
            if (nextLocal <= localNow.DateTime)
                nextLocal = nextLocal.AddDays(1);

            var nextUtc = new DateTimeOffset(nextLocal, timeZone.GetUtcOffset(nextLocal));
            var delay = nextUtc - nowUtc;
            return delay < TimeSpan.FromSeconds(1) ? TimeSpan.FromSeconds(1) : delay;
        }
    }
}
