using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Enums.HumanResource;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Workers
{
    /// <summary>
    /// Background Worker Pemantau Siklus 3 Tahun SPK-RKK & Auto-Suspend Penugasan Klinis Kedaluwarsa (BE-HRD-220, CRD-B3)
    /// Berjalan secara periodik untuk memindai masa berlaku SPK-RKK yang telah melampaui siklus 3 tahun,
    /// memperbarui status SPK menjadi 'Expired', dan secara otomatis menangguhkan hak layanan klinis di SIMRS EMR.
    /// </summary>
    public class SpkRkkExpiryMonitoringWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SpkRkkExpiryMonitoringWorker> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(12); // Dipindai setiap 12 jam

        public SpkRkkExpiryMonitoringWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<SpkRkkExpiryMonitoringWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SpkRkkExpiryMonitoringWorker: Pemantau Siklus 3 Tahun SPK-RKK telah aktif.");

            // Delay awal agar aplikasi selesai bootstrapping
            await Task.Delay(TimeSpan.FromSeconds(25), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PerformSpkExpiryCheckAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("SpkRkkExpiryMonitoringWorker dihentikan karena aplikasi shutting down.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Terjadi kesalahan saat menjalankan pemantau kedaluwarsa SPK-RKK 3 tahun.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task PerformSpkExpiryCheckAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var now = DateTime.UtcNow;

            // Cari SPK aktif yang tanggal berlakunya telah habis (siklus 3 tahun terlampaui)
            var expiredSpkList = await context.TrxSpkRkkIssuances
                .Where(s => s.IsActive && !s.IsRevoked && s.IssuanceStatus == "Active" && s.ValidUntil < now)
                .ToListAsync(stoppingToken);

            if (!expiredSpkList.Any())
            {
                return;
            }

            var affectedProfileIds = expiredSpkList.Select(s => s.WorkforceProfileId).Distinct().ToList();

            foreach (var spk in expiredSpkList)
            {
                spk.IssuanceStatus = "Expired";
                spk.UpdateDateTime = now;
                spk.UpdateBy = Guid.Empty;

                _logger.LogWarning(
                    "SPK-RKK #{SpkNumber} milik nakes {ProfileId} telah kedaluwarsa (Masa berlaku 3 tahun habis pada {ValidUntil}).",
                    spk.SpkNumber, spk.WorkforceProfileId, spk.ValidUntil);
            }

            // Tangguhkan kewenangan klinis aktif nakes di WfpClinicalPrivilege
            var activePrivileges = await context.WfpClinicalPrivileges
                .Where(cp => affectedProfileIds.Contains(cp.WorkforceProfileId) && cp.PrivilegeStatus == ClinicalPrivilegeStatus.Active)
                .ToListAsync(stoppingToken);

            foreach (var cp in activePrivileges)
            {
                cp.PrivilegeStatus = ClinicalPrivilegeStatus.Expired;
                cp.IsClinicalServiceBlocked = true;
                cp.IsSchedulingBlocked = true;
                cp.ApprovalNotes = $"{cp.ApprovalNotes} | Ditangguhkan otomatis: Masa berlaku SPK-RKK 3 tahun telah habis.".Trim();
                cp.UpdateDateTime = now;
                cp.UpdateBy = Guid.Empty;
            }

            await context.SaveChangesAsync(stoppingToken);

            _logger.LogInformation(
                "Siklus Pemantau SPK Selesai: {SpkCount} SPK kedaluwarsa diproses. {PrivCount} kewenangan klinis aktif ditangguhkan (Auto-Suspend).",
                expiredSpkList.Count, activePrivileges.Count);
        }
    }
}
