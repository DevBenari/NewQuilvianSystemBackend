using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Workers
{
    /// <summary>
    /// Background Worker Penerapan SK Terjadwal (Scheduled Personnel Action Order Apply Worker).
    /// Memindai secara berkala dokumen SK berstatus 'Scheduled' yang telah memasuki atau melewati
    /// tanggal efektif (EffectiveDate &lt;= UtcNow.Date), memvalidasi kembali integritas periode,
    /// dan menerapkan perubahan unit, jabatan, serta atasan secara atomik dan idempoten (HRD-DEC-048 / FIN-HRD-07-BE-005).
    /// </summary>
    public class PersonnelActionOrderScheduledApplyWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PersonnelActionOrderScheduledApplyWorker> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);

        public PersonnelActionOrderScheduledApplyWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<PersonnelActionOrderScheduledApplyWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PersonnelActionOrderScheduledApplyWorker: Pemantau SK terjadwal telah aktif.");

            // Initial delay agar bootstrapping aplikasi selesai
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var orderService = scope.ServiceProvider.GetRequiredService<IHrdPersonnelActionOrderService>();
                    var applied = await orderService.ProcessScheduledOrdersAsync(stoppingToken);

                    if (applied > 0)
                    {
                        _logger.LogInformation("PersonnelActionOrderScheduledApplyWorker: Sukses menerapkan {Count} SK terjadwal yang jatuh tempo.", applied);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("PersonnelActionOrderScheduledApplyWorker dihentikan karena aplikasi shutting down.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Terjadi kesalahan saat memproses penerapan SK terjadwal.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }
}
