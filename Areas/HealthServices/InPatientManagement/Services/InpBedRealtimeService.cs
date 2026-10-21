using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Hubs;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;

/// <summary>
/// Publisher best-effort untuk invalidasi layar pemilihan bed. Seluruh query dan pengiriman
/// terjadi sesudah transaksi mutasi commit dan tidak boleh mengubah hasil mutasi.
/// </summary>
public sealed class InpBedRealtimeService(
    ApplicationDbContext db,
    IHubContext<QueueHub> hubContext,
    ILogger<InpBedRealtimeService> logger)
{
    public const string RealtimeEventName = "InpatientBedSelectionChanged";
    private const string SelectionGroupPrefix = "inpatient-bed-selection";

    public static string BuildSelectionGroupName(Guid serviceUnitId)
        => $"{SelectionGroupPrefix}:{serviceUnitId:D}";

    public async Task NotifyBedAcquiredAsync(
        string eventType,
        Guid episodeId,
        Guid bedId,
        Guid actorUserId,
        DateTime occurredAtUtc,
        CancellationToken ct = default)
    {
        var eventId = Guid.NewGuid();
        Guid serviceUnitId = Guid.Empty;

        try
        {
            var snapshot = await (
                from bed in db.Set<MstBed>().AsNoTracking()
                join room in db.Set<MstRoom>().AsNoTracking() on bed.RoomId equals room.Id
                join unit in db.Set<MstServiceUnit>().AsNoTracking() on room.ServiceUnitId equals unit.Id
                join episode in db.Set<InpEpisode>().AsNoTracking() on episodeId equals episode.Id
                join patient in db.Set<MstPatient>().AsNoTracking() on episode.PatientId equals patient.Id
                join actor in db.Users.AsNoTracking() on actorUserId equals actor.Id
                join readiness in db.Set<InpBedReadiness>().AsNoTracking()
                    on bed.Id equals readiness.BedId into readinessRows
                from readiness in readinessRows.DefaultIfEmpty()
                where bed.Id == bedId
                select new
                {
                    Bed = bed,
                    Room = room,
                    Unit = unit,
                    Episode = episode,
                    PatientName = patient.FullName,
                    ActorDisplayName = string.IsNullOrWhiteSpace(actor.DisplayName)
                        ? actor.UserName ?? actor.UserCode
                        : actor.DisplayName,
                    BedOperationalVersion = readiness == null ? 0 : readiness.Version
                })
                .SingleAsync(ct);

            serviceUnitId = snapshot.Unit.Id;
            var payload = new InpatientBedRealtimeEventResponse
            {
                EventId = eventId,
                EventType = eventType,
                BedId = snapshot.Bed.Id,
                BedCode = snapshot.Bed.BedCode,
                BedName = snapshot.Bed.BedName,
                RoomId = snapshot.Room.Id,
                RoomName = snapshot.Room.RoomName,
                ServiceUnitId = snapshot.Unit.Id,
                ServiceUnitName = snapshot.Unit.ServiceUnitName,
                EpisodeId = snapshot.Episode.Id,
                PatientName = snapshot.PatientName,
                ActorUserId = actorUserId,
                ActorDisplayName = snapshot.ActorDisplayName,
                OccurredAtUtc = occurredAtUtc,
                BedOperationalVersion = snapshot.BedOperationalVersion
            };

            await hubContext.Clients
                .Group(BuildSelectionGroupName(serviceUnitId))
                .SendAsync(RealtimeEventName, payload, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Gagal mengirim event realtime bed. EventId={EventId} EventType={EventType} EpisodeId={EpisodeId} BedId={BedId} ServiceUnitId={ServiceUnitId} OccurredAtUtc={OccurredAtUtc}",
                eventId,
                eventType,
                episodeId,
                bedId,
                serviceUnitId,
                occurredAtUtc);
        }
    }
}
