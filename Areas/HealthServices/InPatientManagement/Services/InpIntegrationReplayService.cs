using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;

public sealed class InpIntegrationReplayService(ApplicationDbContext db, IInpIntegrationOutboxService outbox,
    LoggerService logger, IConfiguration configuration)
{
    public async Task<OutboxMonitorResponse> GetMonitorAsync(OutboxMonitorQuery request, CancellationToken ct)
    {
        if (request.From > request.To) throw new ArgumentException("Awal periode melewati akhir periode.");
        var query = db.InpIntegrationOutboxes.AsNoTracking().Where(x => !x.IsDelete);
        if (request.From.HasValue) query = query.Where(x => x.CreatedAtUtc >= request.From.Value);
        if (request.To.HasValue) query = query.Where(x => x.CreatedAtUtc <= request.To.Value);
        if (!string.IsNullOrWhiteSpace(request.EventType))
        {
            if (!InpIntegrationOutboxPayload.EventTypes.Contains(request.EventType)) throw new ArgumentException("Jenis pesan tidak dikenal.");
            query = query.Where(x => x.EventType == request.EventType);
        }
        var counts = await query.GroupBy(x => x.Status).Select(x => new { Status = x.Key, Count = x.Count() }).ToListAsync(ct);
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (!Enum.TryParse<OutboxStatus>(request.Status, true, out var status) || !Enum.IsDefined(status))
                throw new ArgumentException("Status pesan tidak dikenal.");
            query = query.Where(x => x.Status == status);
        }
        else
        {
            var expired = DateTime.UtcNow.AddSeconds(-LeaseSeconds);
            query = query.Where(x => x.Status == OutboxStatus.Failed || x.Status == OutboxStatus.DeadLetter
                || (x.Status == OutboxStatus.Processing && (x.ProcessingStartedAtUtc == null || x.ProcessingStartedAtUtc < expired)));
        }
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.CreatedAtUtc).ThenBy(x => x.Id)
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToListAsync(ct);
        return new OutboxMonitorResponse
        {
            CountsByStatus = Enum.GetValues<OutboxStatus>().ToDictionary(x => x.ToString(), x => counts.FirstOrDefault(c => c.Status == x)?.Count ?? 0),
            Messages = new PagedResult<OutboxMonitorItem>
            {
                PageNumber = request.PageNumber, PageSize = request.PageSize, TotalData = total,
                TotalPage = (int)Math.Ceiling(total / (double)request.PageSize),
                Items = rows.Select(x => new OutboxMonitorItem
                {
                    Id = x.Id, EventType = x.EventType, Status = x.Status.ToString(), IdempotencyKey = x.IdempotencyKey,
                    RetryCount = x.RetryCount, CreatedAtUtc = x.CreatedAtUtc, ProcessingStartedAtUtc = x.ProcessingStartedAtUtc,
                    NextRetryAtUtc = x.NextRetryAtUtc, AcknowledgedReceiptId = x.AcknowledgedReceiptId,
                    ReplayBatchId = x.ReplayBatchId, HasError = x.LastError != null
                }).ToList()
            }
        };
    }

    private int LeaseSeconds => Math.Max(1, configuration.GetValue("InpatientIntegrationOutbox:ProcessingLeaseSeconds", 300));

    public async Task<ReplayResultResponse> ReplayAsync(ReplayRequest request, Guid actorId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Reason) || !request.Reason.Any(char.IsLetterOrDigit))
            throw new ArgumentException("Alasan putar ulang wajib diisi dengan keterangan yang bermakna.");
        var result = new ReplayResultResponse { ReplayBatchId = Guid.NewGuid(), DryRun = request.DryRun };
        await using var transaction = request.DryRun ? null : await db.Database.BeginTransactionAsync(ct);
        if (!request.DryRun && db.Database.IsRelational())
            await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(hashtext({0}));", ["INP_BILLING_REPLAY"], ct);
        var episodes = await db.Set<InpEpisode>().AsNoTracking()
            .Where(x => !x.IsDelete && (x.EpisodeStatus == InpEpisodeStatus.Admitted || x.EpisodeStatus == InpEpisodeStatus.DischargePending))
            .OrderBy(x => x.Id).ToListAsync(ct);
        var sources = await db.InpIntegrationOutboxes.Where(x => !x.IsDelete && x.SourceDomain == "INPATIENT").ToListAsync(ct);
        var now = DateTime.UtcNow;
        foreach (var episode in episodes)
        {
            var item = new ReplayEpisodeItem { EpisodeId = episode.Id, EpisodeNumber = episode.EpisodeNumber };
            var events = sources.Where(x => InpIntegrationOutboxPayload.TryDeserialize(x.PayloadJson)?.EpisodeId == episode.Id).ToList();
            foreach (var message in events)
            {
                if (message.ReplayBatchId.HasValue) continue;
                if (message.Status == OutboxStatus.Processing && message.ProcessingStartedAtUtc >= now.AddSeconds(-LeaseSeconds)) continue;
                var missingLink = message.EventType == "ADMISSION_CONFIRMED" && await NeedsLinkAsync(episode.Id, episode.EncounterId, ct);
                if (message.Status == OutboxStatus.Published && message.AcknowledgedReceiptId.HasValue && !missingLink) continue;
                item.EventsQueued.Add(message.EventType);
                item.AlreadyPublishedRequeued |= message.Status == OutboxStatus.Published;
                if (!request.DryRun)
                {
                    var payload = InpIntegrationOutboxPayload.TryDeserialize(message.PayloadJson)
                        ?? throw new ArgumentException("Pesan lama tidak dapat dibaca; rekonsiliasi sumber diperlukan.");
                    payload.EventType = message.EventType; payload.SourceType = message.SourceType;
                    payload.SourceId = message.SourceDetailId; payload.IdempotencyKey = message.IdempotencyKey;
                    message.PayloadJson = InpIntegrationOutboxPayload.Serialize(payload);
                    message.Status = OutboxStatus.Pending; message.RetryCount = 0; message.NextRetryAtUtc = null;
                    message.ProcessingStartedAtUtc = null; message.PublishedAtUtc = null; message.LastError = null;
                    message.ReplayBatchId = result.ReplayBatchId; message.ReplayedAtUtc = now;
                    message.UpdateBy = actorId; message.UpdateDateTime = now;
                }
            }
            async Task AddMissing(string eventType, string sourceType, Guid sourceId, int version, DateTime occurredAt)
            {
                if (events.Any(x => x.SourceType == sourceType && x.SourceDetailId == sourceId.ToString())) return;
                item.EventsQueued.Add(eventType);
                if (request.DryRun) return;
                await outbox.EnqueueEventAsync(eventType, episode.Id, episode.EncounterId, sourceType, sourceId, version, occurredAt, ct);
                var added = db.ChangeTracker.Entries<InpIntegrationOutbox>().Single(x => x.State == EntityState.Added
                    && x.Entity.IdempotencyKey == InpIntegrationOutboxPayload.BuildIdempotencyKey(sourceType, sourceId, version)).Entity;
                added.ReplayBatchId = result.ReplayBatchId; added.ReplayedAtUtc = now; added.CreateBy = actorId;
            }
            await AddMissing("ADMISSION_CONFIRMED", "ADMISSION", episode.Id, 1, episode.AdmittedAt ?? now);
            var placements = await db.Set<InpBedPlacement>().AsNoTracking().Where(x => x.EpisodeId == episode.Id && !x.IsDelete
                && x.SupersededByCorrectionId == null).OrderBy(x => x.SequenceNumber).ToListAsync(ct);
            foreach (var placement in placements)
            {
                await AddMissing(placement.CorrectsPlacementId.HasValue ? "OCCUPANCY_CORRECTED" : "BED_OCCUPIED",
                    "ROOM_STAY", placement.Id, placement.Version, placement.StartDateTime);
                if (placement.PhysicallyLeftAt.HasValue)
                    await AddMissing("BED_RELEASED", "DISCHARGE", placement.Id, placement.Version, placement.PhysicallyLeftAt.Value);
            }
            var invoice = await db.BilInvoices.FirstOrDefaultAsync(x => x.EncounterId == episode.EncounterId && !x.IsDelete, ct);
            if (invoice != null && invoice.Status == BillingInvoiceStatuses.Open && placements.Count > 0 && !invoice.RequiresReview)
            {
                var invoiceItems = await db.BilInvoiceItems.AsNoTracking().Include(x => x.Category).Include(x => x.Tariff)
                    .Where(x => x.InvoiceId == invoice.Id && !x.IsDelete && x.Status == BillingInvoiceItemStatuses.Active).ToListAsync(ct);
                if (!request.DryRun && invoiceItems.Any(BillingCalculationService.IsManualRoomChargeItem))
                {
                    invoice.RequiresReview = true; invoice.ReviewReasonCode = BillingInvoiceReviewReasonCodes.ManualAndAutomaticRoomCharge;
                    invoice.ReviewFlaggedAt = DateTimeOffset.UtcNow; invoice.RowVersion = Guid.NewGuid();
                    invoice.UpdateBy = actorId; invoice.UpdateDateTime = now;
                }
            }
            if (item.EventsQueued.Count > 0) result.Items.Add(item);
        }
        result.EpisodeCount = result.Items.Count;
        if (!request.DryRun)
        {
            await db.SaveChangesAsync(ct);
            await transaction!.CommitAsync(ct);
        }
        await logger.AuditAsync("HealthServices.InPatientManagement", "IntegrationOutbox.Replay",
            "Putar ulang integrasi Rawat Inap dan Billing.",
            new { result.ReplayBatchId, result.DryRun, result.EpisodeCount, ActorUserId = actorId });
        return result;
    }

    private async Task<bool> NeedsLinkAsync(Guid episodeId, Guid encounterId, CancellationToken ct) =>
        await db.InpAdmissionReferrals.AsNoTracking().AnyAsync(r => r.CompletedEpisodeId == episodeId && !r.IsDelete
            && !db.BilInvoiceEncounterLinks.Any(l => l.RanapInvoice.EncounterId == encounterId
                && l.LinkedEncounterId == r.SourceEncounterId && !l.IsDelete), ct);
}
