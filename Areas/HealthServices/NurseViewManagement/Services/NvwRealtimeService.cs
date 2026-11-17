using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Models;
using QuilvianSystemBackend.Hubs;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Services
{
    public class NvwRealtimeService
    {
        public const string BoardChangedEventName = "NurseViewBoardChanged";
        public const string VitalSignChangedEventType = "VitalSignChanged";
        public const string PatientCallChangedEventName = "NurseViewPatientCallChanged";

        private const string FloorGroupPrefix = "nurse-view-floor";
        private const string CallConsoleGroupPrefix = "nurse-view-call-console";
        private const string BedTabletGroupPrefix = "nurse-view-bed-tablet";

        private readonly ApplicationDbContext _dbContext;
        private readonly IHubContext<QueueHub> _hubContext;
        private readonly ILogger<NvwRealtimeService> _logger;

        public NvwRealtimeService(
            ApplicationDbContext dbContext,
            IHubContext<QueueHub> hubContext,
            ILogger<NvwRealtimeService> logger)
        {
            _dbContext = dbContext;
            _hubContext = hubContext;
            _logger = logger;
        }

        public static string BuildFloorGroupName(string? floorCode) =>
            $"{FloorGroupPrefix}:{NvwDisplayDeviceService.NormalizeFloorCode(floorCode)}";

        public static string BuildCallConsoleGroupName(string? floorCode) =>
            $"{CallConsoleGroupPrefix}:{NvwDisplayDeviceService.NormalizeFloorCode(floorCode)}";

        public static string BuildBedTabletGroupName(Guid tabletId) =>
            $"{BedTabletGroupPrefix}:{tabletId:D}";

        public static string ChangeTypeFor(NvwPatientCallEventType eventType) => eventType switch
        {
            NvwPatientCallEventType.Pressed => "Created",
            NvwPatientCallEventType.PressedAgain => "PressedAgain",
            NvwPatientCallEventType.EscalatedToEmergency => "Escalated",
            NvwPatientCallEventType.Claimed => "Claimed",
            NvwPatientCallEventType.TakenOver => "TakenOver",
            NvwPatientCallEventType.Released => "Released",
            NvwPatientCallEventType.NurseArrived => "Handled",
            NvwPatientCallEventType.ClosedFromConsole => "Handled",
            NvwPatientCallEventType.CancelledByPatient => "Cancelled",
            NvwPatientCallEventType.ClosedBySystem => "ClosedBySystem",
            _ => eventType.ToString()
        };

        public async Task NotifyPatientCallChangedAsync(
            NvwPatientCall call,
            NvwPatientCallEventType eventType,
            CancellationToken cancellationToken = default)
        {
            var occurredAtUtc = DateTime.UtcNow;
            var changeType = ChangeTypeFor(eventType);

            try
            {
                var floorCode = NvwDisplayDeviceService.NormalizeFloorCode(call.FloorCode);

                var tabletIds = await _dbContext.NvwBedTablets.AsNoTracking()
                    .Where(x => x.BedId == call.BedId && !x.IsDelete && x.DeviceStatus == NvwBedTabletStatus.Active)
                    .Select(x => x.Id)
                    .ToListAsync(cancellationToken);

                var groups = tabletIds.Select(BuildBedTabletGroupName).ToList();

                if (floorCode.Length > 0)
                {
                    groups.Add(BuildFloorGroupName(floorCode));
                    groups.Add(BuildCallConsoleGroupName(floorCode));
                }

                if (groups.Count == 0)
                    return;

                await _hubContext.Clients
                    .Groups(groups)
                    .SendAsync(
                        PatientCallChangedEventName,
                        new NvwPatientCallChangedEvent
                        {
                            CallId = call.Id,
                            BedId = call.BedId,
                            FloorCode = floorCode,
                            ChangeType = changeType,
                            OccurredAtUtc = occurredAtUtc
                        },
                        cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Gagal mengirim sinyal panggilan Nurse View. CallId={CallId} BedId={BedId} FloorCode={FloorCode} ChangeType={ChangeType} OccurredAtUtc={OccurredAtUtc}",
                    call.Id,
                    call.BedId,
                    call.FloorCode,
                    changeType,
                    occurredAtUtc);
            }
        }

        public async Task NotifyVitalSignChangedAsync(Guid? inpEpisodeId, CancellationToken cancellationToken = default)
        {
            if (inpEpisodeId is not Guid episodeId || episodeId == Guid.Empty)
                return;

            var occurredAtUtc = DateTime.UtcNow;

            try
            {
                var beds = await (
                    from placement in _dbContext.Set<InpBedPlacement>().AsNoTracking()
                    join bed in _dbContext.MstBeds.AsNoTracking() on placement.BedId equals bed.Id
                    join room in _dbContext.MstRooms.AsNoTracking() on bed.RoomId equals room.Id
                    where placement.EpisodeId == episodeId &&
                          placement.EndDateTime == null &&
                          !placement.IsDelete &&
                          !bed.IsDelete &&
                          bed.IsActive &&
                          !room.IsDelete &&
                          room.IsActive &&
                          room.FloorCode != null
                    select new { BedId = bed.Id, room.FloorCode })
                    .ToListAsync(cancellationToken);

                foreach (var bed in beds)
                {
                    var floorCode = NvwDisplayDeviceService.NormalizeFloorCode(bed.FloorCode);

                    if (floorCode.Length == 0)
                        continue;

                    await _hubContext.Clients
                        .Group(BuildFloorGroupName(floorCode))
                        .SendAsync(
                            BoardChangedEventName,
                            new NvwBoardChangedEvent
                            {
                                EventType = VitalSignChangedEventType,
                                BedId = bed.BedId,
                                FloorCode = floorCode,
                                OccurredAtUtc = occurredAtUtc
                            },
                            cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Gagal mengirim sinyal papan Nurse View. EventType={EventType} InpEpisodeId={InpEpisodeId} OccurredAtUtc={OccurredAtUtc}",
                    VitalSignChangedEventType,
                    episodeId,
                    occurredAtUtc);
            }
        }
    }
}
