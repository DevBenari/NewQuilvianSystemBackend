using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;

public partial class InpAdmissionTransferService
{
    public async Task<InpEpisodeOperationResult> ExecuteTransferAdmissionAsync(OpenAdmissionFromTransferRequest request,
        Guid actorUserId, CancellationToken cancellationToken = default)
    {
        if (request.BedId == Guid.Empty || request.ExpectedBedVersion < 1)
            throw new InpBedOperationException(400, "INVALID_INPUT", "Bed dan ExpectedBedVersion wajib.");
        if (!await _bedAccess.HasPermissionAsync("PatientEncounter:Create"))
            throw new InpBedOperationException(403, "ACCESS_DENIED", "Hak membuat kunjungan Registrasi diperlukan.");
        if (request.DepositAmount > 0 && request.DepositPaymentMethodId.HasValue && !await _bedAccess.HasPermissionAsync("BillingDeposit:Create"))
            throw new InpBedOperationException(403, "ACCESS_DENIED", "Hak menerima deposit Billing diperlukan.");
        if (request.RequiresIsolation && !await _bedAccess.HasPermissionAsync("InpatientEpisode:Update"))
            throw new InpBedOperationException(403, "ACCESS_DENIED", "Hak mencatat kebutuhan isolasi diperlukan.");
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
        if (!meta.IsReplay && request.DepositAmount > 0 && request.DepositPaymentMethodId.HasValue)
            await deposits.AuditAdmissionTopUpAsync(episode.EncounterId, episode.Id, actorUserId, cancellationToken);
        if (!meta.IsReplay)
            await loggerService.AuditAsync("HealthServices.InPatient.AdmissionTransfer", "AdmissionTransfer.Created",
                "Admisi transfer dan rencana deposit tersimpan atomik.",
                new { EpisodeId = episode.Id, episode.EncounterId, ActorUserId = actorUserId, meta.ResultEntityId });

        // BE-RWI-241: Otomasi pemicu pembuatan serah terima klinis transfer berstatus PendingSend (NotSent)
        if (!meta.IsReplay && _transferHandoverService != null)
        {
            try
            {
                var source = await GetTransferByIdAsync(request.TransferDispositionId, cancellationToken, request.SourceType);
                var fromUnitId = Guid.Empty;
                if (request.SourceType == "Emergency" && source?.EmergencyVisitId != null)
                {
                    fromUnitId = await _dbContext.Set<EmgVisit>()
                        .Where(v => v.Id == source.EmergencyVisitId.Value)
                        .Select(v => v.ServiceUnitId)
                        .FirstOrDefaultAsync(cancellationToken);
                }
                else if (source?.SourceEncounterId != null)
                {
                    fromUnitId = await _dbContext.RegPatientEncounters
                        .Where(e => e.Id == source.SourceEncounterId.Value)
                        .Select(e => e.ServiceUnitId)
                        .FirstOrDefaultAsync(cancellationToken);
                }

                if (fromUnitId == Guid.Empty)
                {
                    fromUnitId = await _dbContext.Set<QuilvianSystemBackend.Areas.HealthServices.MasterData.Models.MstServiceUnit>()
                        .Where(u => !u.IsDelete && u.IsActive &&
                            (request.SourceType == "Emergency"
                                ? u.ServiceUnitType == QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums.ServiceUnitType.Emergency
                                : u.ServiceUnitType == QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums.ServiceUnitType.Outpatient))
                        .Select(u => u.Id)
                        .FirstOrDefaultAsync(cancellationToken);
                }

                if (fromUnitId != Guid.Empty)
                {
                    await _transferHandoverService.CreateForAdmissionTransferAsync(
                        episode.Id,
                        fromUnitId,
                        request.ServiceUnitId,
                        actorUserId,
                        source?.SourceEncounterId,
                        cancellationToken);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                await loggerService.AuditAsync("HealthServices.InPatient.AdmissionTransfer", "AdmissionTransfer.HandoverTriggerFailed",
                    $"Pemicu dokumen serah terima transfer otomatis tertunda: {ex.Message}",
                    new { EpisodeId = episode.Id, ActorUserId = actorUserId, Exception = ex.Message });
            }
        }

        var outcome = InpEpisodeOperationResult.Success(episode, "Admisi transfer dan reservasi tersimpan. Lanjutkan persetujuan pasien.");
        outcome.OperationMeta = meta;
        return outcome;
    }

    public async Task<InpatientEpisodeDetailResponse?> GetAdmissionResultAsync(InpEpisodeOperationResult result, CancellationToken ct)
    {
        var detail = await episodeService.GetEpisodeDetailAsync(result.Episode!.Id, ct);
        if (detail == null) return null;
        detail.OperationMeta = result.OperationMeta;
        detail.TransferReservation = await bedOccupancyService.GetReservationAsync(result.OperationMeta!.ResultEntityId, ct);
        if (await _dbContext.BilDepositAccounts.AnyAsync(x => x.EncounterId == detail.EncounterId && !x.IsDelete, ct))
            detail.TransferDeposit = await deposits.GetByEncounterAsync(detail.EncounterId, ct);
        return detail;
    }
}

