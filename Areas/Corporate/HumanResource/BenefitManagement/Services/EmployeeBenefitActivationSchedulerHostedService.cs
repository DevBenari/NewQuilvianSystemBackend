using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services
{
    /// <summary>
    /// Penjadwal background hosted service untuk aktivasi otomatis benefit karyawan berstatus Approved
    /// yang tanggal mulai berlakunya (EffectiveStartDate) sudah tiba berdasarkan hari operasional WIB (FIN-DES-111, FIN-DEC-235).
    /// 
    /// DIBANGUN MATI: <see cref="EmployeeBenefitActivationSchedulerOptions.Enabled"/> = false secara bawaan.
    /// Pola mengikuti penjadwal Finance dan HR lain di Quilvian (PeriodicTimer, isolasi IServiceScope per siklus, graceful shutdown).
    /// </summary>
    public sealed class EmployeeBenefitActivationSchedulerHostedService : BackgroundService
    {
        private const int MinimumPollIntervalSeconds = 30;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly EmployeeBenefitActivationSchedulerOptions _options;
        private readonly ILogger<EmployeeBenefitActivationSchedulerHostedService> _logger;

        public EmployeeBenefitActivationSchedulerHostedService(
            IServiceScopeFactory scopeFactory,
            IOptions<EmployeeBenefitActivationSchedulerOptions> options,
            ILogger<EmployeeBenefitActivationSchedulerHostedService> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _options = options?.Value ?? new EmployeeBenefitActivationSchedulerOptions();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.Enabled)
            {
                _logger.LogInformation(
                    "Penjadwal aktivasi benefit pegawai dinonaktifkan melalui konfigurasi " +
                    "(HumanResource:EmployeeBenefitActivationScheduler:Enabled = false). Nol benefit diaktifkan otomatis.");
                return;
            }

            var intervalSeconds = Math.Max(MinimumPollIntervalSeconds, _options.PollIntervalSeconds);
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(intervalSeconds));

            _logger.LogInformation(
                "Penjadwal aktivasi benefit pegawai aktif. Poll={Interval}s, MaxPerCycle={Max}, SystemActor={Actor}.",
                intervalSeconds,
                Math.Clamp(_options.MaxPerCycle, 1, 500),
                _options.SystemActorUserId?.ToString() ?? "(kosong: Guid.Empty)");

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
                    _logger.LogError(ex, "Siklus penjadwal aktivasi benefit pegawai gagal secara keseluruhan.");
                }

                if (!await timer.WaitForNextTickAsync(stoppingToken))
                {
                    break;
                }
            }
        }

        private async Task JalankanSatuSiklusAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<EmployeeBenefitEnrollmentService>();

            var maxPerCycle = Math.Clamp(_options.MaxPerCycle, 1, 500);
            var dueIds = await service.GetDueApprovedEnrollmentIdsAsync(maxPerCycle, stoppingToken);

            if (dueIds.Count == 0)
            {
                return;
            }

            var actorUserId = _options.SystemActorUserId ?? Guid.Empty;
            int activated = 0;
            int skippedExpired = 0;
            int alreadyActive = 0;
            int failedRequirements = 0;
            int errors = 0;

            foreach (var id in dueIds)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                var (success, outcome) = await service.ActivateDueAsync(id, actorUserId, stoppingToken);
                switch (outcome)
                {
                    case "ACTIVATED":
                        activated++;
                        break;
                    case "SKIPPED_PERIOD_EXPIRED":
                        skippedExpired++;
                        break;
                    case "ALREADY_ACTIVE":
                        alreadyActive++;
                        break;
                    case "FAILED_REQUIREMENTS":
                        failedRequirements++;
                        break;
                    default:
                        if (!success) errors++;
                        break;
                }
            }

            _logger.LogInformation(
                "Siklus aktivasi benefit pegawai selesai. Ditemukan={Total}, Diaktifkan={Activated}, " +
                "DilewatiKadaluarsa={Skipped}, SudahAktif={AlreadyActive}, GagalSyarat={Failed}, Galat={Errors}.",
                dueIds.Count, activated, skippedExpired, alreadyActive, failedRequirements, errors);

            if (failedRequirements > 0)
            {
                _logger.LogWarning(
                    "{Count} benefit pegawai gagal syarat aktivasi dan tetap berstatus Approved; akan dicoba ulang pada siklus berikutnya.",
                    failedRequirements);
            }
        }
    }
}

