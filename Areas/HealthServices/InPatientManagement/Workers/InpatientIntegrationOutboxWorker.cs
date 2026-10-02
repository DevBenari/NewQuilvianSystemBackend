using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Options;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Workers
{
    /// <summary>
    /// Background worker untuk mempublikasikan antrean event outbox integrasi Rawat Inap ke Billing (BE-RWI-129).
    /// Menggunakan exponential backoff dan pengamanan batas DeadLetter.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Outbox jujur (<c>BE-RWI-151</c>, kontrak <c>integrasi-billing</c> <c>1.1.0</c>).</b> Worker
    /// memanggil penerima Billing di dalam aplikasi
    /// (<see cref="BillingInpatientEventReceiver"/>) dan menandai pesan <c>Published</c>
    /// <b>hanya</b> bila Billing mengembalikan tanda terima <c>Accepted = true</c>
    /// (<c>INV-RWF-04</c>). Versi sebelumnya menandai <c>Published</c> tanpa penerima
    /// (<c>FIN-FACT-06</c>).
    /// </para>
    /// <para>
    /// <b>Sewa pemrosesan.</b> Pesan yang diambil ditandai <c>Processing</c> beserta
    /// <c>ProcessingStartedAtUtc</c>. Bila aplikasi mati di tengah pengiriman, pesan itu diambil ulang
    /// setelah masa sewa (<c>ProcessingLeaseSeconds</c>) lewat, sehingga tidak tersangkut selamanya.
    /// </para>
    /// <para>
    /// <b>Penerima memakai scope sendiri.</b> Setiap pesan dikirim lewat scope dependency injection
    /// terpisah supaya transaksi Billing yang gagal tidak mengotori pelacakan baris outbox.
    /// </para>
    /// </remarks>
    public class InpatientIntegrationOutboxWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<InpatientIntegrationOutboxWorker> _logger;
        private readonly IOptionsMonitor<InpatientIntegrationOutboxOptions> _options;
        private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5);
        private const int MaxBackoffSeconds = 3600;

        public InpatientIntegrationOutboxWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<InpatientIntegrationOutboxWorker> logger,
            IOptionsMonitor<InpatientIntegrationOutboxOptions> options)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _options = options;
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
            var options = _options.CurrentValue;
            var batchSize = options.BatchSize > 0 ? options.BatchSize : 50;
            var maxRetry = options.MaxRetry > 0 ? options.MaxRetry : 10;
            var leaseSeconds = options.ProcessingLeaseSeconds > 0 ? options.ProcessingLeaseSeconds : 300;

            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var now = DateTime.UtcNow;
            var leaseExpiredBefore = now.AddSeconds(-leaseSeconds);

            // Tiga sumber pesan: belum pernah dikirim, gagal dan sudah jatuh tempo coba ulang, atau
            // tersangkut Processing melewati masa sewa (state 5.1).
            var pendingItems = await dbContext.InpIntegrationOutboxes
                .Where(x => !x.IsDelete &&
                    (x.Status == OutboxStatus.Pending ||
                     (x.Status == OutboxStatus.Failed && x.NextRetryAtUtc <= now) ||
                     (x.Status == OutboxStatus.Processing &&
                      (x.ProcessingStartedAtUtc == null || x.ProcessingStartedAtUtc < leaseExpiredBefore))))
                .OrderBy(x => x.CreatedAtUtc)
                .Take(batchSize)
                .ToListAsync(cancellationToken);

            if (pendingItems.Count == 0)
            {
                return 0;
            }

            _logger.LogInformation("Memproses {Count} pesan outbox integrasi Rawat Inap.", pendingItems.Count);

            foreach (var item in pendingItems)
            {
                item.Status = OutboxStatus.Processing;
                item.ProcessingStartedAtUtc = now;
                item.UpdateDateTime = now;
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            foreach (var item in pendingItems)
            {
                try
                {
                    var receipt = await DispatchEventAsync(item, cancellationToken);

                    if (receipt.Accepted)
                    {
                        item.MarkPublished(receipt.ReceiptId);
                        item.UpdateDateTime = DateTime.UtcNow;

                        _logger.LogInformation(
                            "Outbox event {EventType} ({IdempotencyKey}) diterima Billing dengan hasil {Outcome}.",
                            item.EventType,
                            item.IdempotencyKey,
                            receipt.Outcome);
                    }
                    else
                    {
                        RecordFailure(item, $"{receipt.Outcome}: {receipt.Message}", maxRetry);

                        _logger.LogWarning(
                            "Outbox event {EventType} ({IdempotencyKey}) ditolak Billing ({Outcome}). Percobaan ke-{Retry}.",
                            item.EventType,
                            item.IdempotencyKey,
                            receipt.Outcome,
                            item.RetryCount);
                    }
                }
                catch (Exception ex)
                {
                    RecordFailure(item, ex.Message, maxRetry);

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

        /// <summary>
        /// Backoff <c>min(2^n × 5 detik, 3600 detik)</c> (kontrak integrasi 4.2), lalu <c>DeadLetter</c>
        /// setelah <paramref name="maxRetry"/> kali gagal.
        /// </summary>
        private static void RecordFailure(InpIntegrationOutbox item, string error, int maxRetry)
        {
            var retryCount = item.RetryCount + 1;
            var delaySeconds = (int)Math.Min(Math.Pow(2, retryCount) * 5, MaxBackoffSeconds);
            var nextRetry = DateTime.UtcNow.AddSeconds(delaySeconds);

            item.RecordFailure(error, nextRetry, maxRetry);
            item.UpdateDateTime = DateTime.UtcNow;
        }

        private async Task<InpatientEventReceipt> DispatchEventAsync(
            InpIntegrationOutbox outbox,
            CancellationToken cancellationToken)
        {
            var envelope = BuildEnvelope(outbox);

            using var receiverScope = _scopeFactory.CreateScope();
            var receiver = receiverScope.ServiceProvider.GetRequiredService<BillingInpatientEventReceiver>();

            _logger.LogDebug(
                "Dispatching integration event {EventType} with IdempotencyKey {IdempotencyKey}",
                outbox.EventType,
                outbox.IdempotencyKey);

            return await receiver.ReceiveAsync(envelope, cancellationToken);
        }

        /// <summary>
        /// Menyusun amplop untuk Billing dari kolom baris outbox dan isi pesan daftar putih.
        /// Pesan lama yang tersimpan sebelum kontrak <c>1.1.0</c> tetap dapat dikirim karena
        /// identitas episode dan kunjungannya dibaca tanpa peka huruf besar-kecil.
        /// </summary>
        private static InpatientBillingEventEnvelope BuildEnvelope(InpIntegrationOutbox outbox)
        {
            var payload = InpIntegrationOutboxPayload.TryDeserialize(outbox.PayloadJson)
                ?? throw new InvalidOperationException(
                    $"Isi pesan outbox {outbox.IdempotencyKey} tidak dapat dibaca.");

            if (payload.EpisodeId == Guid.Empty || payload.EncounterId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    $"Isi pesan outbox {outbox.IdempotencyKey} tidak memuat EpisodeId atau EncounterId.");
            }

            var keyParts = outbox.IdempotencyKey.Split(':');
            var version = payload.Version > 0
                ? payload.Version
                : keyParts.Length >= 4 && int.TryParse(keyParts[^1], out var parsed) ? parsed : 1;

            return new InpatientBillingEventEnvelope
            {
                EventType = outbox.EventType,
                EpisodeId = payload.EpisodeId,
                EncounterId = payload.EncounterId,
                SourceType = outbox.SourceType,
                SourceId = outbox.SourceDetailId,
                Version = version,
                OccurredAtUtc = payload.OccurredAtUtc == default ? outbox.CreatedAtUtc : payload.OccurredAtUtc,
                IdempotencyKey = outbox.IdempotencyKey
            };
        }
    }
}
