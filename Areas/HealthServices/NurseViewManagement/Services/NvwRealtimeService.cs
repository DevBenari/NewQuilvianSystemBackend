using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.DTOs;
using QuilvianSystemBackend.Hubs;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Services
{
    public class NvwRealtimeService
    {
        public const string BoardChangedEventName = "NurseViewBoardChanged";
        public const string VitalSignChangedEventType = "VitalSignChanged";

        private const string FloorGroupPrefix = "nurse-view-floor";

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
