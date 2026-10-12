using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;

public partial class InpDischargeService
{
    public async Task<InpEpisodeOperationResult> RecordPatientDepartureAsync(Guid episodeId,
        RecordDepartureRequest? request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        if (request == null || request.ExpectedPlacementId == Guid.Empty || request.ExpectedBedVersion < 1)
            throw new InpBedOperationException(400, "INVALID_INPUT", "ExpectedPlacementId dan ExpectedBedVersion wajib.");
        await _bedAccess.RequireEpisodeAsync(episodeId, "InpatientDischarge:RecordDeparture", cancellationToken);
        var source = await _dbContext.Set<InpBedPlacement>().AsNoTracking().SingleOrDefaultAsync(x => x.Id == request.ExpectedPlacementId && x.EpisodeId == episodeId, cancellationToken)
            ?? throw new InpBedOperationException(409, "STALE_PLACEMENT", "Penempatan asal berubah.");
        InpEpisodeOperationResult? outcome = null;
        var meta = await _bedCoordinator.ExecuteAsync("InpatientDischarge:RecordDeparture", new { episodeId, request },
            [episodeId], [source.BedId], [new() { BedId = source.BedId, ExpectedVersion = request.ExpectedBedVersion }], async ct =>
            {
                outcome = await RecordPatientDepartureCoreAsync(episodeId, request, actorUserId, ct);
                if (outcome.Status != InpEpisodeOperationStatus.Success)
                    throw new InpBedOperationException(outcome.Status == InpEpisodeOperationStatus.Conflict ? 409 : outcome.Status == InpEpisodeOperationStatus.Invalid ? 400 : 422, outcome.Code ?? "BED_UNAVAILABLE", outcome.Message);
                return ("Placement", source.Id);
            }, cancellationToken);
        if (outcome == null)
        {
            var episode = await _dbContext.Set<InpEpisode>().AsNoTracking().SingleAsync(x => x.Id == episodeId, cancellationToken);
            outcome = InpEpisodeOperationResult.Success(episode, "Kepergian sudah tersimpan.");
            outcome.Departure = new() { EpisodeId = episode.Id, EpisodeStatus = episode.EpisodeStatus,
                PhysicallyLeftAt = episode.PhysicallyLeftAt!.Value,
                ClearanceObserved = episode.DepartureClearanceObserved!.Value,
                ClearanceObservedAt = episode.DepartureClearanceObservedAt!.Value };
        }
        outcome.Departure!.OperationMeta = meta;
        if (!meta.IsReplay)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                outcome.Departure.RunningEquipmentUsageClosedCount = await scope.ServiceProvider.GetRequiredService<CliEquipmentUsageService>()
                    .CloseRunningForDepartureAsync(episodeId, outcome.Departure.PhysicallyLeftAt, actorUserId, cancellationToken);
            }
            catch (Exception ex)
            {
                _dischargeLogger.LogError(ex, "Penutupan alat sesudah keluar ruangan gagal untuk {EpisodeId}.", episodeId);
                outcome.Departure.FollowUpWarnings.Add("Kepergian tersimpan. Penutupan alat perlu ditindaklanjuti.");
            }
        }
        return outcome;
    }
}

