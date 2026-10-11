using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;

public partial class InpAdmissionTransferService
{
    public async Task<InpEpisodeOperationResult> ExecuteTransferAdmissionAsync(OpenAdmissionFromTransferRequest request,
        Guid actorUserId, CancellationToken cancellationToken = default)
    {
        if (request.BedId == Guid.Empty || request.ExpectedBedVersion < 1)
            throw new InpBedOperationException(400, "INVALID_INPUT", "Bed dan ExpectedBedVersion wajib.");
        var units = await _bedAccess.UnitsAsync("InpatientEpisode:Create", cancellationToken);
        if (!units.Contains(request.ServiceUnitId))
            throw new InpBedOperationException(403, "ACCESS_DENIED", "Unit admisi di luar scope.");
        var meta = await _bedCoordinator.ExecuteAsync("InpatientEpisode:Create", request, [], [request.BedId],
            [new() { BedId = request.BedId, ExpectedVersion = request.ExpectedBedVersion }], async ct =>
            {
                var result = await ExecuteTransferAdmissionCoreAsync(request, actorUserId, ct);
                if (result.Status != InpEpisodeOperationStatus.Success || result.Episode == null)
                    throw new InpBedOperationException(result.Status == InpEpisodeOperationStatus.Conflict ? 409 : result.Status == InpEpisodeOperationStatus.NotFound ? 404 : 422,
                        result.Code ?? "BED_NOT_READY", result.Message);
                var reservation = await _dbContext.Set<InpBedReservation>().SingleAsync(x => x.EpisodeId == result.Episode.Id && x.BedId == request.BedId, ct);
                return ("Reservation", reservation.Id);
            }, cancellationToken);
        var epId = await _dbContext.Set<InpBedReservation>().Where(x => x.Id == meta.ResultEntityId).Select(x => x.EpisodeId).SingleAsync(cancellationToken);
        var episode = await _dbContext.Set<InpEpisode>().AsNoTracking().SingleAsync(x => x.Id == epId, cancellationToken);
        var outcome = InpEpisodeOperationResult.Success(episode, "Pendaftaran rawat inap dari transfer IGD tersimpan.");
        outcome.OperationMeta = meta;
        return outcome;
    }
}

