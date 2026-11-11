using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Workers
{
    /// <summary>
    /// Background Worker Pemantau Radar Kedaluwarsa SIP 5 Tahun & Pelaksana Tindakan Fail-Closed H-0 (BE-HRD-209, CRD-B2)
    /// Berjalan secara periodik untuk memindai masa berlaku STR/SIP seluruh tenaga kerja di faskes,
    /// memperbarui tier peringatan (H-90 s.d. H-7), serta mengeksekusi pemblokiran SIMRS dan Roster (Fail-Closed) pada H-0.
    /// </summary>
    public class LicenseExpiryMonitoringWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<LicenseExpiryMonitoringWorker> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(6); // Dipindai setiap 6 jam

        public LicenseExpiryMonitoringWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<LicenseExpiryMonitoringWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("LicenseExpiryMonitoringWorker: Pemantau Radar SIP & Fail-Closed H-0 telah aktif.");

            // Delay awal agar aplikasi selesai bootstrapping
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PerformLicenseMonitoringCycleAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("LicenseExpiryMonitoringWorker dihentikan karena aplikasi shutting down.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Terjadi kesalahan saat menjalankan siklus radar pemantau lisensi nakes.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task PerformLicenseMonitoringCycleAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var verificationService = scope.ServiceProvider.GetRequiredService<ICredentialVerificationService>();

            var today = DateTime.UtcNow.Date;

            // 1. Ambil seluruh lisensi aktif non-seumur-hidup
            var activeLicenses = await context.WfpCredentialLicenses
                .Where(l => l.IsActive && !l.IsRevoked && !l.IsLifetimeStr)
                .ToListAsync(stoppingToken);

            int h90Count = 0;
            int h60Count = 0;
            int h30Count = 0;
            int h7Count = 0;
            int expiredCount = 0;
            bool anyStatusUpdated = false;

            foreach (var license in activeLicenses)
            {
                var daysRemaining = (license.ExpiredDate.Date - today).Days;
                string newRadarStatus;

                if (daysRemaining <= 0)
                {
                    newRadarStatus = "ExpiredH0";
                    expiredCount++;
                }
                else if (daysRemaining <= 7)
                {
                    newRadarStatus = "H7";
                    h7Count++;
                }
                else if (daysRemaining <= 30)
                {
                    newRadarStatus = "H30";
                    h30Count++;
                }
                else if (daysRemaining <= 60)
                {
                    newRadarStatus = "H60";
                    h60Count++;
                }
                else if (daysRemaining <= 90)
                {
                    newRadarStatus = "H90";
                    h90Count++;
                }
                else
                {
                    newRadarStatus = "Normal";
                }

                if (license.ExpiryRadarStatus != newRadarStatus)
                {
                    license.ExpiryRadarStatus = newRadarStatus;
                    license.UpdateDateTime = DateTime.UtcNow;
                    license.UpdateBy = Guid.Empty;
                    anyStatusUpdated = true;
                }
            }

            if (anyStatusUpdated)
            {
                await context.SaveChangesAsync(stoppingToken);
            }

            // 2. Eksekusi tindakan Fail-Closed untuk lisensi yang telah mencapai H-0
            var failClosedExecutedCount = await verificationService.ExecuteFailClosedForExpiredLicensesAsync(stoppingToken);

            _logger.LogInformation(
                "Siklus Radar Selesai: {Total} lisensi dipindai. Status: [H-90: {H90}, H-60: {H60}, H-30: {H30}, H-7: {H7}, H-0/Expired: {Expired}]. Tindakan Fail-Closed baru dieksekusi: {FailClosedCount}",
                activeLicenses.Count, h90Count, h60Count, h30Count, h7Count, expiredCount, failClosedExecutedCount);
        }
    }
}
