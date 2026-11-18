using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Services
{
    public class NvwPatientCallReconciliationService
    {
        private const int MaxCorrectionHops = 10;
        private const string CorrectedToOtherBed = "CorrectedToOtherBed";

        private readonly ApplicationDbContext _dbContext;
        private readonly NvwRealtimeService _realtime;
        private readonly ILogger<NvwPatientCallReconciliationService> _logger;

        public NvwPatientCallReconciliationService(
            ApplicationDbContext dbContext,
            NvwRealtimeService realtime,
            ILogger<NvwPatientCallReconciliationService> logger)
        {
            _dbContext = dbContext;
            _realtime = realtime;
            _logger = logger;
        }

        private static NvwPatientCallCloseReason? ResolveCloseReason(InpBedPlacementEndReason? endReason) => endReason switch
        {
            InpBedPlacementEndReason.Transfer => NvwPatientCallCloseReason.PatientTransferred,
            InpBedPlacementEndReason.EpisodeClosed => NvwPatientCallCloseReason.PatientDischarged,
            InpBedPlacementEndReason.PatientDeparted => NvwPatientCallCloseReason.PatientDischarged,
            InpBedPlacementEndReason.AdmissionCancelled => NvwPatientCallCloseReason.AdmissionCancelled,
            _ => null
        };

        public async Task<NvwPatientCallReconciliationSummary> CloseCallsForDepartedPatientsAsync(CancellationToken cancellationToken)
        {
            var candidates = await _dbContext.NvwPatientCalls.AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    (x.CallStatus == NvwPatientCallStatus.Waiting || x.CallStatus == NvwPatientCallStatus.Claimed) &&
                    x.BedPlacement != null &&
                    x.BedPlacement.EndDateTime != null)
                .OrderBy(x => x.FirstPressedAt)
                .Select(x => new Candidate(
                    x.Id,
                    x.BedId,
                    x.Bed != null ? x.Bed.BedCode : string.Empty,
                    x.FloorCode,
                    x.InpBedPlacementId))
                .ToListAsync(cancellationToken);

            var closed = 0;
            var skipped = 0;
            var failed = 0;

            foreach (var candidate in candidates)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var departure = await ResolveDepartureAsync(candidate, cancellationToken);

                    if (departure != null && await CloseAsync(candidate, departure, cancellationToken))
                        closed++;
                    else
                        skipped++;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    failed++;
                    _logger.LogError(
                        exception,
                        "Nurse View gagal menutup otomatis panggilan {CallId} bed {BedCode} lantai {FloorCode}; dicoba lagi pada putaran berikutnya.",
                        candidate.CallId,
                        candidate.BedCode,
                        candidate.FloorCode);
                }
                finally
                {
                    _dbContext.ChangeTracker.Clear();
                }
            }

            return new NvwPatientCallReconciliationSummary(candidates.Count, closed, skipped, failed);
        }

        private async Task<Departure?> ResolveDepartureAsync(Candidate candidate, CancellationToken cancellationToken)
        {
            Guid? placementId = candidate.PlacementId;

            for (var hops = 0; hops <= MaxCorrectionHops && placementId.HasValue; hops++)
            {
                var step = await _dbContext.InpBedPlacements.AsNoTracking()
                    .Where(x => x.Id == placementId.Value)
                    .Select(x => new PlacementStep(x.Id, x.BedId, x.EndDateTime, x.EndReason, x.SupersededByCorrectionId, x.IsDelete))
                    .FirstOrDefaultAsync(cancellationToken);

                if (step == null || step.IsDelete)
                    return null;

                if (step.SupersededByCorrectionId.HasValue)
                {
                    placementId = step.SupersededByCorrectionId;
                    continue;
                }

                if (step.BedId != candidate.BedId)
                    return new Departure(NvwPatientCallCloseReason.PatientTransferred, CorrectedToOtherBed, step.Id, step.EndDateTime, hops);

                if (step.EndDateTime == null)
                    return null;

                var reason = ResolveCloseReason(step.EndReason);
                return reason == null
                    ? null
                    : new Departure(reason.Value, step.EndReason!.Value.ToString(), step.Id, step.EndDateTime, hops);
            }

            return null;
        }

        private async Task<bool> CloseAsync(Candidate candidate, Departure departure, CancellationToken cancellationToken)
        {
            var call = await _dbContext.NvwPatientCalls
                .FirstOrDefaultAsync(x => x.Id == candidate.CallId && !x.IsDelete, cancellationToken);

            if (call == null || !NvwPatientCallQueryService.IsActiveStatus(call.CallStatus))
                return false;

            var now = DateTime.UtcNow;
            var previousStatus = call.CallStatus;

            call.CallStatus = NvwPatientCallStatus.ClosedBySystem;
            call.CloseReason = departure.Reason;
            call.ClosedAt = now;
            call.ClosedByUserId = null;
            call.UpdateDateTime = now;
            call.UpdateBy = Guid.Empty;
            call.Version++;

            _dbContext.NvwPatientCallEvents.Add(new NvwPatientCallEvent
            {
                PatientCallId = call.Id,
                EventType = NvwPatientCallEventType.ClosedBySystem,
                ActorKind = NvwPatientCallActorKind.System,
                ActorUserId = null,
                CloseReason = departure.Reason,
                OccurredAt = now,
                CreateDateTime = now,
                CreateBy = Guid.Empty
            });

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogInformation(
                    "Nurse View menunda penutupan otomatis panggilan {CallId} bed {BedCode}: versi panggilan berubah, dicoba lagi pada putaran berikutnya.",
                    candidate.CallId,
                    candidate.BedCode);
                return false;
            }

            _logger.LogInformation(
                "Nurse View menutup otomatis panggilan {CallId} bed {BedCode} lantai {FloorCode}: status {PreviousStatus} -> ClosedBySystem, alasan {CloseReason}; dasar {EndReason} pada penempatan {DecidingPlacementId} (penempatan awal {PlacementId}, {CorrectionHops} koreksi), berakhir {PlacementEndedAtUtc:o}, ditutup {ClosedAtUtc:o}.",
                call.Id,
                candidate.BedCode,
                candidate.FloorCode,
                previousStatus,
                departure.Reason,
                departure.Basis,
                departure.DecidingPlacementId,
                candidate.PlacementId,
                departure.CorrectionHops,
                departure.PlacementEndedAt,
                now);

            await _realtime.NotifyPatientCallChangedAsync(call, NvwPatientCallEventType.ClosedBySystem, cancellationToken);

            return true;
        }

        private sealed record Candidate(
            Guid CallId,
            Guid BedId,
            string BedCode,
            string FloorCode,
            Guid PlacementId);

        private sealed record PlacementStep(
            Guid Id,
            Guid BedId,
            DateTime? EndDateTime,
            InpBedPlacementEndReason? EndReason,
            Guid? SupersededByCorrectionId,
            bool IsDelete);

        private sealed record Departure(
            NvwPatientCallCloseReason Reason,
            string Basis,
            Guid DecidingPlacementId,
            DateTime? PlacementEndedAt,
            int CorrectionHops);
    }

    public sealed record NvwPatientCallReconciliationSummary(int Candidates, int Closed, int Skipped, int Failed);
}
