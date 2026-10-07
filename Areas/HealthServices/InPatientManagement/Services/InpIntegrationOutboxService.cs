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
    /// <remarks>
    /// <b>Daftar putih isi pesan (<c>BE-RWI-151</c>, <c>INV-RWF-05</c>).</b> Isi pesan hanya delapan
    /// field pada <see cref="InpIntegrationOutboxPayload.AllowedFields"/>. Pesan yang memuat field lain
    /// ditolak saat pendaftaran dengan <see cref="InvalidOperationException"/>, sehingga transaksi
    /// bisnis pemanggil ikut batal dan tidak ada event hantu (<c>VAL-RWF-13</c>).
    /// </remarks>
    public class InpIntegrationOutboxService : IInpIntegrationOutboxService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<InpIntegrationOutboxService> _logger;

        public InpIntegrationOutboxService(
            ApplicationDbContext dbContext,
            ILogger<InpIntegrationOutboxService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public Task EnqueueEventAsync(
            string eventType,
            Guid episodeId,
            Guid encounterId,
            string sourceType,
            Guid sourceId,
            int version,
            DateTime occurredAtUtc,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(eventType) ||
                !InpIntegrationOutboxPayload.EventTypes.Contains(eventType.Trim()))
            {
                throw new ArgumentException(
                    $"Jenis event outbox tidak dikenal: {eventType}.", nameof(eventType));
            }

            if (string.IsNullOrWhiteSpace(sourceType) ||
                !InpIntegrationOutboxPayload.SourceTypes.Contains(sourceType.Trim()))
            {
                throw new ArgumentException(
                    $"SourceType outbox tidak dikenal: {sourceType}.", nameof(sourceType));
            }

            if (episodeId == Guid.Empty || encounterId == Guid.Empty || sourceId == Guid.Empty)
            {
                throw new ArgumentException("EpisodeId, EncounterId, dan SourceId event outbox wajib diisi.");
            }

            if (version < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(version), "Versi entitas sumber minimal 1.");
            }

            var normalizedEventType = eventType.Trim();
            var normalizedSourceType = sourceType.Trim();

            // VAL-INT-007 — format compound key baku: SourceDomain:SourceType:SourceId:Version.
            var idempotencyKey = InpIntegrationOutboxPayload.BuildIdempotencyKey(
                normalizedSourceType, sourceId, version);

            var payloadJson = InpIntegrationOutboxPayload.Serialize(new InpIntegrationOutboxPayload
            {
                EventType = normalizedEventType,
                EpisodeId = episodeId,
                EncounterId = encounterId,
                SourceType = normalizedSourceType,
                SourceId = sourceId.ToString(),
                Version = version,
                OccurredAtUtc = DateTime.SpecifyKind(occurredAtUtc, DateTimeKind.Utc),
                IdempotencyKey = idempotencyKey
            });

            // Jaring pengaman terakhir: payload yang tersusun pun diperiksa ulang terhadap daftar
            // putih, supaya perubahan kelas payload di masa depan tidak diam-diam menambah field.
            if (!InpIntegrationOutboxPayload.IsWhitelisted(payloadJson))
            {
                throw new InvalidOperationException(
                    "Isi pesan outbox memuat field di luar daftar putih INV-RWF-05.");
            }

            var now = DateTime.UtcNow;
            var outbox = new InpIntegrationOutbox
            {
                Id = Guid.NewGuid(),
                IdempotencyKey = idempotencyKey,
                SourceDomain = InpIntegrationOutboxPayload.SourceDomain,
                SourceType = normalizedSourceType,
                SourceDetailId = sourceId.ToString(),
                EventType = normalizedEventType,
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

    /// <summary>
    /// Bentuk isi pesan outbox Rawat Inap — daftar putih kontrak <c>integrasi-billing</c> <c>1.1.0</c>
    /// integrasi 4.2 (<c>INV-RWF-05</c>).
    /// </summary>
    public sealed class InpIntegrationOutboxPayload
    {
        public const string SourceDomain = "INPATIENT";

        public static readonly IReadOnlySet<string> EventTypes = new HashSet<string>(StringComparer.Ordinal)
        {
            "ADMISSION_CONFIRMED", "BED_OCCUPIED", "OCCUPANCY_CORRECTED", "BED_RELEASED"
        };

        public static readonly IReadOnlySet<string> SourceTypes = new HashSet<string>(StringComparer.Ordinal)
        {
            "ADMISSION", "ROOM_STAY", "DISCHARGE"
        };

        /// <summary>Delapan nama field yang boleh ada di dalam pesan, dalam bentuk camelCase.</summary>
        public static readonly IReadOnlySet<string> AllowedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "eventType", "episodeId", "encounterId", "sourceType", "sourceId", "version", "occurredAtUtc", "idempotencyKey"
        };

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        public string EventType { get; set; } = string.Empty;

        public Guid EpisodeId { get; set; }

        public Guid EncounterId { get; set; }

        public string SourceType { get; set; } = string.Empty;

        public string SourceId { get; set; } = string.Empty;

        public int Version { get; set; }

        public DateTime OccurredAtUtc { get; set; }

        public string IdempotencyKey { get; set; } = string.Empty;

        public static string BuildIdempotencyKey(string sourceType, Guid sourceId, int version) =>
            $"{SourceDomain}:{sourceType}:{sourceId}:{version}";

        public static string Serialize(InpIntegrationOutboxPayload payload) =>
            JsonSerializer.Serialize(payload, JsonOptions);

        /// <summary>
        /// <c>true</c> bila objek JSON tingkat atas hanya memuat field daftar putih.
        /// </summary>
        public static bool IsWhitelisted(string? payloadJson)
        {
            if (string.IsNullOrWhiteSpace(payloadJson))
            {
                return false;
            }

            try
            {
                using var document = JsonDocument.Parse(payloadJson);
                if (document.RootElement.ValueKind != JsonValueKind.Object)
                {
                    return false;
                }

                foreach (var property in document.RootElement.EnumerateObject())
                {
                    if (!AllowedFields.Contains(property.Name))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        /// <summary>
        /// Membaca isi pesan. Pembacaan tidak peka huruf besar-kecil sehingga pesan lama yang
        /// tersimpan sebelum kontrak <c>1.1.0</c> tetap dapat diambil identitasnya.
        /// </summary>
        public static InpIntegrationOutboxPayload? TryDeserialize(string? payloadJson)
        {
            if (string.IsNullOrWhiteSpace(payloadJson))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<InpIntegrationOutboxPayload>(payloadJson, JsonOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}
