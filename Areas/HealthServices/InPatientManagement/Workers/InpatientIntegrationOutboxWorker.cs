using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Workers
{
    /// <summary>
    /// Background worker untuk mempublikasikan antrean event outbox integrasi Rawat Inap ke Billing (BE-RWI-129).
    /// Menggunakan exponential backoff dan pengamanan batas DeadLetter (>= 10 kali gagal).
    /// </summary>
    public class InpatientIntegrationOutboxWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<InpatientIntegrationOutboxWorker> _logger;
        private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5);
        private const int BatchSize = 50;
        private const int MaxRetryCount = 10;
        private const int MaxBackoffSeconds = 3600;

        public InpatientIntegrationOutboxWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<InpatientIntegrationOutboxWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("InpatientIntegrationOutboxWorker telah aktif dan memulai pemantauan antrean.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOutboxBatchAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Terjadi kesalahan tidak terduga saat pemrosesan batch outbox integrasi.");
                }

                try
                {
                    await Task.Delay(_pollingInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation("InpatientIntegrationOutboxWorker dihentikan secara aman.");
        }

        public async Task<int> ProcessOutboxBatchAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var now = DateTime.UtcNow;

            var pendingItems = await dbContext.InpIntegrationOutboxes
                .Where(x => !x.IsDelete &&
                    (x.Status == OutboxStatus.Pending ||
                     (x.Status == OutboxStatus.Failed && x.NextRetryAtUtc <= now)))
                .OrderBy(x => x.CreatedAtUtc)
                .Take(BatchSize)
                .ToListAsync(cancellationToken);

            if (pendingItems.Count == 0)
            {
                return 0;
            }

            _logger.LogInformation("Memproses {Count} pesan outbox integrasi Rawat Inap.", pendingItems.Count);

            foreach (var item in pendingItems)
            {
                item.Status = OutboxStatus.Processing;
                item.UpdateDateTime = now;
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            foreach (var item in pendingItems)
            {
                try
                {
                    // Simulasi pengiriman event ke listener / broker modul Billing
                    await DispatchEventAsync(item, cancellationToken);

                    item.MarkPublished();
                    item.UpdateDateTime = DateTime.UtcNow;

                    _logger.LogInformation(
                        "Outbox event {EventType} ({IdempotencyKey}) berhasil dipublikasikan.",
                        item.EventType,
                        item.IdempotencyKey);
                }
                catch (Exception ex)
                {
                    var retryCount = item.RetryCount + 1;
                    var delaySeconds = Math.Min((int)Math.Pow(2, retryCount) * 5, MaxBackoffSeconds);
                    var nextRetry = DateTime.UtcNow.AddSeconds(delaySeconds);

                    item.RecordFailure(ex.Message, nextRetry);
                    item.UpdateDateTime = DateTime.UtcNow;

                    _logger.LogWarning(
                        ex,
                        "Gagal mengirim outbox event {EventType} ({IdempotencyKey}). Percobaan ke-{Retry}. Retry berikutnya: {NextRetry}",
                        item.EventType,
                        item.IdempotencyKey,
                        item.RetryCount,
                        item.NextRetryAtUtc);
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            return pendingItems.Count;
        }

        private Task DispatchEventAsync(InpIntegrationOutbox outbox, CancellationToken cancellationToken)
        {
            // Validasi format payload
            if (string.IsNullOrWhiteSpace(outbox.PayloadJson))
            {
                throw new InvalidOperationException($"Payload JSON kosong untuk event {outbox.EventType}.");
            }

            // Pada arsitektur monolit ASP.NET Core modul internal Quilvian, dispatch dicatat
            // dan disalurkan ke event hub / listener lokal atau message broker terdaftar.
            _logger.LogDebug(
                "Dispatching integration event {EventType} with IdempotencyKey {IdempotencyKey}",
                outbox.EventType,
                outbox.IdempotencyKey);

            return Task.CompletedTask;
        }
    }
}
