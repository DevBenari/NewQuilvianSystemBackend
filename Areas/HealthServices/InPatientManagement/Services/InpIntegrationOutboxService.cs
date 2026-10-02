using System.Text.Json;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Implementasi transactional outbox service untuk integrasi Rawat Inap ↔ Billing (BE-RWI-128).
    /// Menjamin bahwa event outbox didaftarkan pada DbContext dalam transaksi yang sama dengan entitas bisnis.
    /// </summary>
    public class InpIntegrationOutboxService : IInpIntegrationOutboxService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<InpIntegrationOutboxService> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public InpIntegrationOutboxService(
            ApplicationDbContext dbContext,
            ILogger<InpIntegrationOutboxService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public Task EnqueueEventAsync(
            string eventType,
            string idempotencyKey,
            string sourceDomain,
            string sourceType,
            string sourceDetailId,
            object payload,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(eventType))
            {
                throw new ArgumentException("EventType tidak boleh kosong.", nameof(eventType));
            }

            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload), "Payload event tidak boleh null.");
            }

            // VAL-INT-007 — Validasi format compound key: SourceDomain:SourceType:SourceDetailId:Version
            if (string.IsNullOrWhiteSpace(idempotencyKey))
            {
                throw new ArgumentException(
                    "Format IdempotencyKey tidak valid. Kunci harus mengikuti format baku gabungan domain, tipe, ID detail, dan versi.",
                    nameof(idempotencyKey));
            }

            var parts = idempotencyKey.Split(':');
            if (parts.Length < 4 || parts.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException(
                    "Format IdempotencyKey tidak valid. Kunci harus mengikuti format baku gabungan domain, tipe, ID detail, dan versi.",
                    nameof(idempotencyKey));
            }

            var domain = string.IsNullOrWhiteSpace(sourceDomain) ? "INPATIENT" : sourceDomain.Trim();
            var sType = string.IsNullOrWhiteSpace(sourceType) ? parts[1] : sourceType.Trim();
            var detailId = string.IsNullOrWhiteSpace(sourceDetailId) ? parts[2] : sourceDetailId.Trim();

            var payloadJson = JsonSerializer.Serialize(payload, JsonOptions);

            var now = DateTime.UtcNow;
            var outbox = new InpIntegrationOutbox
            {
                Id = Guid.NewGuid(),
                IdempotencyKey = idempotencyKey.Trim(),
                SourceDomain = domain,
                SourceType = sType,
                SourceDetailId = detailId,
                EventType = eventType.Trim(),
                PayloadJson = payloadJson,
                Status = OutboxStatus.Pending,
                RetryCount = 0,
                NextRetryAtUtc = null,
                PublishedAtUtc = null,
                LastError = null,
                CreatedAtUtc = now,
                CreateDateTime = now,
                CreateBy = Guid.Empty
            };

            _dbContext.InpIntegrationOutboxes.Add(outbox);

            _logger.LogInformation(
                "Enqueued outbox event {EventType} with IdempotencyKey {IdempotencyKey} for {SourceDomain}:{SourceType}:{SourceDetailId}",
                outbox.EventType,
                outbox.IdempotencyKey,
                outbox.SourceDomain,
                outbox.SourceType,
                outbox.SourceDetailId);

            return Task.CompletedTask;
        }
    }
}
