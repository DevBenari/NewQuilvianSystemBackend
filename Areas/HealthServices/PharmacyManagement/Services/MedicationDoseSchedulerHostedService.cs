using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services
{
    /// <summary>
    /// Membentuk dosis MAR terjadwal untuk seluruh episode berjalan — <c>BE-RWI-114</c> kriteria 4,
    /// <c>INT-KEP-08</c> pemicu (2). Mengikuti pola <c>EmergencyTriageSlaMonitorHostedService</c> dan
    /// <c>AttendanceSchedulerHostedService</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Kegagalan satu episode tidak menghentikan episode lain.</b> Galat dicatat per episode dan putaran berlanjut;
    /// percobaan berikutnya pada putaran selanjutnya. MAR yang dibuka tetap memanggil pembentukan, sehingga tidak ada
    /// dosis yang hilang karena galat terjadwal.
    /// </para>
    /// <para>
    /// <b>Setiap episode memakai scope sendiri</b>, supaya pelacak perubahan tidak membesar sepanjang putaran dan satu
    /// galat simpan tidak meninggalkan baris setengah jadi bagi episode berikutnya. Pembentukannya idempoten, sehingga
    /// dua instans aplikasi yang berjalan bersamaan tidak menggandakan dosis.
    /// </para>
    /// </remarks>
    public class MedicationDoseSchedulerHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly MedicationDoseSchedulerOptions _options;
        private readonly ILogger<MedicationDoseSchedulerHostedService> _logger;

        public MedicationDoseSchedulerHostedService(
            IServiceScopeFactory scopeFactory,
            IOptions<MedicationDoseSchedulerOptions> options,
            ILogger<MedicationDoseSchedulerHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.Enabled)
            {
                _logger.LogInformation("Pembentukan dosis MAR terjadwal dinonaktifkan melalui konfigurasi.");
                return;
            }

            var jeda = TimeSpan.FromSeconds(Math.Max(60, _options.PollIntervalSeconds));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RunCycleAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Putaran pembentukan dosis MAR gagal.");
                }

                try
                {
                    await Task.Delay(jeda, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        private async Task RunCycleAsync(CancellationToken stoppingToken)
        {
            List<Guid> episodeIds;
            Guid pelaku;

            using (var scope = _scopeFactory.CreateScope())
            {
                var service = scope.ServiceProvider.GetRequiredService<MedicationAdministrationService>();

                pelaku = _options.SystemActorUserId ?? await service.ResolveSystemActorUserIdAsync(stoppingToken);

                if (pelaku == Guid.Empty)
                {
                    _logger.LogWarning("Pembentukan dosis MAR terjadwal dilewati: akun pelaku sistem tidak ditemukan.");
                    return;
                }

                episodeIds = await service.GetEpisodesForDoseGenerationAsync(stoppingToken);
            }

            var dibentuk = 0;

            foreach (var episodeId in episodeIds)
            {
                stoppingToken.ThrowIfCancellationRequested();

                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<MedicationAdministrationService>();

                    var hasil = await service.EnsureDosesAsync(episodeId, pelaku, stoppingToken);
                    dibentuk += hasil.CreatedCount;

                    if (hasil.Warning != null)
                        _logger.LogWarning("Pembentukan dosis MAR episode {EpisodeId}: {Warning}", episodeId, hasil.Warning);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Pembentukan dosis MAR episode {EpisodeId} gagal; dilanjutkan ke episode berikutnya.", episodeId);
                }
            }

            if (dibentuk > 0)
                _logger.LogInformation("Pembentukan dosis MAR terjadwal: {Jumlah} dosis baru pada {Episode} episode.", dibentuk, episodeIds.Count);
        }
    }
}
