using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services
{
    /// <summary>
    /// Penjadwal rekonsiliasi berkala sinkronisasi kartu penjamin internal pasien (Benefit Karyawan RS).
    /// Mengolah enrollment baru yang belum memiliki catatan, mengulang status FAILED_RETRYABLE / WAITING_PATIENT jatuh tempo,
    /// mendeteksi pencabutan kartu (REVOKED), dan menonaktifkan kartu pada enrollment yang dibatalkan.
    /// 
    /// DIBANGUN MATI: <see cref="EmployeeBenefitCardSyncSchedulerOptions.Enabled"/> = false adalah nilai bawaan.
    /// Sesuai aturan FIN-DES-125, penjadwal mewajibkan konfigurasi <see cref="EmployeeBenefitCardSyncSchedulerOptions.ActivationCutoffUtc"/>
    /// bila dinyalakan untuk mencegah pemasangan massal tanpa batas terhadap data histori lama.
    /// (BE-FIN-136; FR-FIN-265, FR-FIN-268; FIN-DEC-253; FIN-DES-113, 117, 125; FIN-INTEGRATION-1.11 V.3).
    /// </summary>
    public sealed class EmployeeBenefitCardSyncSchedulerHostedService : BackgroundService
    {
        private const int MinimumPollIntervalSeconds = 30;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly EmployeeBenefitCardSyncSchedulerOptions _options;
        private readonly ILogger<EmployeeBenefitCardSyncSchedulerHostedService> _logger;

        public EmployeeBenefitCardSyncSchedulerHostedService(
            IServiceScopeFactory scopeFactory,
            IOptions<EmployeeBenefitCardSyncSchedulerOptions> options,
            ILogger<EmployeeBenefitCardSyncSchedulerHostedService> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _options = options?.Value ?? new EmployeeBenefitCardSyncSchedulerOptions();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.Enabled)
            {
                _logger.LogInformation(
                    "Penjadwal sinkronisasi kartu penjamin internal pasien dinonaktifkan melalui konfigurasi " +
                    "(PatientManagement:EmployeeBenefitCardSyncScheduler:Enabled = false). Rekonsiliasi otomatis tidak dijalankan.");
                return;
            }

            if (!_options.ActivationCutoffUtc.HasValue)
            {
                _logger.LogWarning(
                    "Penjadwal sinkronisasi kartu pasien aktif (Enabled=true) tetapi ActivationCutoffUtc belum ditentukan. " +
                    "Sesuai aturan FIN-DES-125, rekonsiliasi massal tidak akan memproses data apa pun demi mencegah pemasangan massal tanpa batas.");
            }

            var intervalSeconds = Math.Max(MinimumPollIntervalSeconds, _options.PollIntervalSeconds);
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(intervalSeconds));

            _logger.LogInformation(
                "Penjadwal sinkronisasi kartu penjamin internal pasien aktif. Poll={Interval}s, MaxPerCycle={Max}, " +
                "ActivationCutoffUtc={Cutoff}, SystemActor={Actor}.",
                intervalSeconds, Math.Clamp(_options.MaxPerCycle, 1, 500),
                _options.ActivationCutoffUtc?.ToString("yyyy-MM-ddTHH:mm:ssZ") ?? "(kosong: pemrosesan massal dicegah)",
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
                    _logger.LogError(ex, "Siklus penjadwal sinkronisasi kartu pasien gagal secara keseluruhan.");
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
            var syncService = scope.ServiceProvider.GetRequiredService<EmployeeBenefitCardSyncService>();

            var hasil = await syncService.RunReconciliationAsync(_options, stoppingToken);

            if (hasil.WarningCutoffMissing)
            {
                _logger.LogWarning(
                    "Siklus rekonsiliasi sinkronisasi kartu dilewati karena ActivationCutoffUtc belum ditentukan (FIN-DES-125).");
                return;
            }

            if (hasil.TotalProcessed > 0)
            {
                _logger.LogInformation(
                    "Siklus rekonsiliasi sinkronisasi kartu selesai. BaruDitemukan={NewDiscovered}, BaruDiproses={NewProcessed}, " +
                    "BaruSukses={NewSuccess}, DiulangDitemukan={RetryDiscovered}, DiulangDiproses={RetryProcessed}, " +
                    "DiulangSukses={RetrySuccess}, Dicabut={Revoked}, Dibatalkan={Cancelled}.",
                    hasil.Step1NewDiscovered, hasil.Step1NewProcessed, hasil.Step1NewSuccess,
                    hasil.Step2RetryDiscovered, hasil.Step2RetryProcessed, hasil.Step2RetrySuccess,
                    hasil.Step3RevokedCount, hasil.Step4CancelledCount);
            }
        }
    }
}

