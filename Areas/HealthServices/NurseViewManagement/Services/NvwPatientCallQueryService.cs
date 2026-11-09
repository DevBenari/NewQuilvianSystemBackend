using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Helpers;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Models;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Services
{
    public class NvwPatientCallQueryService
    {
        public const string ActionClaim = "Claim";
        public const string ActionTakeOver = "TakeOver";
        public const string ActionRelease = "Release";
        public const string ActionClose = "Close";

        private readonly ApplicationDbContext _dbContext;
        private readonly NvwPatientCallWaitThresholdService _thresholds;

        public NvwPatientCallQueryService(ApplicationDbContext dbContext, NvwPatientCallWaitThresholdService thresholds)
        {
            _dbContext = dbContext;
            _thresholds = thresholds;
        }

        public static string PriorityLabel(NvwPatientCallPriority priority) => priority switch
        {
            NvwPatientCallPriority.Normal => "Panggilan",
            NvwPatientCallPriority.Emergency => "Darurat",
            _ => priority.ToString()
        };

        public static string StatusLabel(NvwPatientCallStatus status) => status switch
        {
            NvwPatientCallStatus.Waiting => "Menunggu",
            NvwPatientCallStatus.Claimed => "Diklaim",
            NvwPatientCallStatus.Handled => "Ditangani",
            NvwPatientCallStatus.Cancelled => "Dibatalkan",
            NvwPatientCallStatus.ClosedBySystem => "Ditutup sistem",
            _ => status.ToString()
        };

        public static bool IsActiveStatus(NvwPatientCallStatus status) =>
            status is NvwPatientCallStatus.Waiting or NvwPatientCallStatus.Claimed;

        public static int WaitingSeconds(DateTime firstPressedAtUtc, DateTime nowUtc) =>
            (int)Math.Max(0, Math.Floor((nowUtc - firstPressedAtUtc).TotalSeconds));

        public static NvwTabletCallResponse MapTabletCall(NvwPatientCall call, string? claimedByName, DateTime nowUtc)
        {
            var isActive = IsActiveStatus(call.CallStatus);

            return new NvwTabletCallResponse
            {
                CallId = call.Id,
                Priority = call.Priority,
                PriorityLabel = PriorityLabel(call.Priority),
                CallStatus = call.CallStatus,
                StatusLabel = StatusLabel(call.CallStatus),
                FirstPressedAt = call.FirstPressedAt,
                WaitingSeconds = WaitingSeconds(call.FirstPressedAt, nowUtc),
                PressCount = call.PressCount,
                ClaimedByName = claimedByName,
                CanCancel = isActive && call.CallStatus == NvwPatientCallStatus.Waiting && call.Priority == NvwPatientCallPriority.Normal,
                CanEscalate = isActive && call.Priority == NvwPatientCallPriority.Normal,
                CanConfirmNurseArrived = isActive,
                IsActive = isActive,
                Version = call.Version
            };
        }

        public async Task<NvwTabletCallResponse?> GetForTabletAsync(NvwBedTablet tablet, CancellationToken cancellationToken)
        {
            var call = await _dbContext.NvwPatientCalls.AsNoTracking()
                .Where(x =>
                    x.BedId == tablet.BedId &&
                    !x.IsDelete &&
                    (x.CallStatus == NvwPatientCallStatus.Waiting || x.CallStatus == NvwPatientCallStatus.Claimed))
                .OrderByDescending(x => x.FirstPressedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (call == null)
                return null;

            return await MapTabletCallAsync(call, cancellationToken);
        }

        public async Task<NvwTabletCallResponse> MapTabletCallAsync(NvwPatientCall call, CancellationToken cancellationToken)
        {
            string? claimedByName = null;
            if (call.ClaimedByUserId.HasValue)
            {
                var claimer = await _dbContext.Users.AsNoTracking()
                    .Where(x => x.Id == call.ClaimedByUserId.Value)
                    .Select(x => new { x.UserName, x.DisplayName })
                    .FirstOrDefaultAsync(cancellationToken);
                claimedByName = string.IsNullOrWhiteSpace(claimer?.DisplayName) ? claimer?.UserName : claimer.DisplayName;
            }

            return MapTabletCall(call, claimedByName, DateTime.UtcNow);
        }

        public static List<string> ConsoleActions(NvwPatientCall call, Guid userId, bool canUpdate)
        {
            if (!canUpdate || !IsActiveStatus(call.CallStatus))
                return new List<string>();

            if (call.CallStatus == NvwPatientCallStatus.Waiting)
                return new List<string> { ActionClaim, ActionClose };

            return call.ClaimedByUserId == userId
                ? new List<string> { ActionRelease, ActionClose }
                : new List<string> { ActionTakeOver, ActionClose };
        }

        public async Task<NvwResult<NvwConsoleCallListResponse>> GetActiveForConsoleAsync(
            string? floorCode,
            Guid userId,
            bool canUpdate,
            CancellationToken cancellationToken)
        {
            var floor = NvwDisplayDeviceService.NormalizeFloorCode(floorCode);

            if (!await IsKnownFloorAsync(floor, cancellationToken))
                return NvwResult<NvwConsoleCallListResponse>.Rule(NvwErrorCodes.ConsoleFloorUnknown, NvwMessages.ConsoleFloorUnknown);

            var threshold = await _thresholds.GetActiveAsync(cancellationToken);

            var calls = await _dbContext.NvwPatientCalls.AsNoTracking()
                .Where(x =>
                    x.FloorCode == floor &&
                    !x.IsDelete &&
                    (x.CallStatus == NvwPatientCallStatus.Waiting || x.CallStatus == NvwPatientCallStatus.Claimed))
                .OrderByDescending(x => x.Priority)
                .ThenBy(x => x.FirstPressedAt)
                .ThenBy(x => x.Id)
                .ToListAsync(cancellationToken);

            var now = DateTime.UtcNow;

            return NvwResult<NvwConsoleCallListResponse>.Ok(new NvwConsoleCallListResponse
            {
                FloorCode = floor,
                FloorName = FloorPlanRoomNaming.FloorName(floor),
                ThresholdsConfigured = threshold.IsConfigured,
                Items = await MapConsoleCallsAsync(calls, userId, canUpdate, threshold, now, cancellationToken),
                ServerTimeUtc = now
            });
        }

        public async Task<bool> IsKnownFloorAsync(string? floorCode, CancellationToken cancellationToken)
        {
            var floor = NvwDisplayDeviceService.NormalizeFloorCode(floorCode);

            return floor.Length > 0 && await _dbContext.MstRooms.AsNoTracking()
                .AnyAsync(x => !x.IsDelete && x.IsActive && x.FloorCode != null && x.FloorCode.Trim().ToUpper() == floor, cancellationToken);
        }

        public async Task<NvwConsoleCallResponse> MapConsoleCallAsync(
            NvwPatientCall call,
            Guid userId,
            CancellationToken cancellationToken)
        {
            var threshold = await _thresholds.GetActiveAsync(cancellationToken);
            var items = await MapConsoleCallsAsync(new List<NvwPatientCall> { call }, userId, true, threshold, DateTime.UtcNow, cancellationToken);
            return items[0];
        }

        private async Task<List<NvwConsoleCallResponse>> MapConsoleCallsAsync(
            List<NvwPatientCall> calls,
            Guid userId,
            bool canUpdate,
            NvwPatientCallWaitThresholdResponse threshold,
            DateTime nowUtc,
            CancellationToken cancellationToken)
        {
            if (calls.Count == 0)
                return new List<NvwConsoleCallResponse>();

            var bedIds = calls.Select(x => x.BedId).Distinct().ToList();
            var beds = await _dbContext.MstBeds.AsNoTracking()
                .Where(x => bedIds.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    x.BedCode,
                    x.BedName,
                    RoomName = x.Room != null ? x.Room.RoomName : string.Empty
                })
                .ToDictionaryAsync(x => x.Id, cancellationToken);

            var episodeIds = calls.Select(x => x.InpEpisodeId).Distinct().ToList();
            var episodes = await _dbContext.InpEpisodes.AsNoTracking()
                .Where(x => episodeIds.Contains(x.Id) && !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    FullName = x.Patient != null ? x.Patient.FullName : null,
                    Gender = x.Patient != null ? x.Patient.Gender : null,
                    BirthDate = x.Patient != null ? x.Patient.BirthDate : null,
                    HasPatient = x.Patient != null
                })
                .ToListAsync(cancellationToken);

            var today = AppDateTimeHelper.OperationalDate();
            var patients = episodes
                .Where(x => x.HasPatient)
                .ToDictionary(
                    x => x.Id,
                    x => new NvwMaskedPatientResponse
                    {
                        Initials = NvwPatientMasking.Initials(x.FullName),
                        GenderCode = NvwPatientMasking.GenderCode(x.Gender),
                        AgeLabel = NvwPatientMasking.AgeLabel(x.BirthDate, today)
                    });

            var claimerIds = calls.Where(x => x.ClaimedByUserId.HasValue).Select(x => x.ClaimedByUserId!.Value).Distinct().ToList();
            var claimers = claimerIds.Count == 0
                ? new Dictionary<Guid, string?>()
                : (await _dbContext.Users.AsNoTracking()
                    .Where(x => claimerIds.Contains(x.Id))
                    .Select(x => new { x.Id, x.UserName, x.DisplayName })
                    .ToListAsync(cancellationToken))
                    .ToDictionary(x => x.Id, x => string.IsNullOrWhiteSpace(x.DisplayName) ? x.UserName : x.DisplayName);

            return calls.Select(call =>
            {
                var isActive = IsActiveStatus(call.CallStatus);
                var waitingSeconds = WaitingSeconds(call.FirstPressedAt, isActive ? nowUtc : call.ClosedAt ?? nowUtc);
                beds.TryGetValue(call.BedId, out var bed);
                patients.TryGetValue(call.InpEpisodeId, out var patient);
                string? claimedByName = null;
                if (call.ClaimedByUserId.HasValue)
                    claimers.TryGetValue(call.ClaimedByUserId.Value, out claimedByName);

                return new NvwConsoleCallResponse
                {
                    CallId = call.Id,
                    BedId = call.BedId,
                    BedCode = bed?.BedCode ?? string.Empty,
                    BedName = bed?.BedName ?? string.Empty,
                    RoomName = bed?.RoomName ?? string.Empty,
                    Patient = patient,
                    Priority = call.Priority,
                    PriorityLabel = PriorityLabel(call.Priority),
                    CallStatus = call.CallStatus,
                    StatusLabel = StatusLabel(call.CallStatus),
                    FirstPressedAt = call.FirstPressedAt,
                    WaitingSeconds = waitingSeconds,
                    PressCount = call.PressCount,
                    ClaimedByUserId = call.ClaimedByUserId,
                    ClaimedByName = claimedByName,
                    ClaimedAt = call.ClaimedAt,
                    IsClaimedByMe = call.CallStatus == NvwPatientCallStatus.Claimed && call.ClaimedByUserId == userId,
                    WaitLevel = isActive
                        ? NvwPatientCallWaitThresholdService.ResolveWaitLevel(threshold, waitingSeconds)
                        : NvwPatientCallWaitThresholdService.WaitLevelNone,
                    AvailableActions = ConsoleActions(call, userId, canUpdate),
                    Version = call.Version
                };
            }).ToList();
        }
    }
}
