using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Services
{
    public class NvwBedTabletContextService
    {
        public const string AccountClaim = "is_nurse_view_bed_tablet_account";
        public const string TabletIdClaim = "nurse_view_bed_tablet_id";
        public const string SessionStampClaim = "nurse_view_bed_tablet_session_stamp";
        public const string ProfileType = "NurseViewBedTablet";
        public const int ActiveCallRefreshIntervalSeconds = 15;
        public const int IdleRefreshIntervalSeconds = 60;

        private const int LastOnlineWriteGapSeconds = 60;

        private readonly ApplicationDbContext _dbContext;
        private readonly NvwPatientCallQueryService _calls;

        public NvwBedTabletContextService(ApplicationDbContext dbContext, NvwPatientCallQueryService calls)
        {
            _dbContext = dbContext;
            _calls = calls;
        }

        public async Task<NvwBedTabletLoginContext> ResolveLoginContextAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var tablet = await _dbContext.NvwBedTablets.AsNoTracking()
                .FirstOrDefaultAsync(x => x.LoginUserId == userId && !x.IsDelete, cancellationToken);

            if (tablet == null)
                return NvwBedTabletLoginContext.None();

            var canLogin = tablet.DeviceStatus == NvwBedTabletStatus.Active;

            return new NvwBedTabletLoginContext
            {
                IsBedTabletAccount = true,
                TabletId = tablet.Id,
                DeviceCode = tablet.DeviceCode,
                SessionStamp = NvwDeviceContextService.BuildSessionStamp(tablet.LoginIssuedAt),
                CanLogin = canLogin,
                BlockReason = canLogin ? null : NvwMessages.BedTabletInactive
            };
        }

        public async Task<NvwResult<NvwBedTablet>> ResolveActiveTabletAsync(
            Guid userId,
            string? tokenSessionStamp,
            CancellationToken cancellationToken)
        {
            var tablet = await _dbContext.NvwBedTablets
                .FirstOrDefaultAsync(x => x.LoginUserId == userId && !x.IsDelete, cancellationToken);

            if (tablet == null || tablet.DeviceStatus != NvwBedTabletStatus.Active)
                return NvwResult<NvwBedTablet>.Forbidden(NvwErrorCodes.BedTabletUnavailable, NvwMessages.BedTabletUnavailable);

            if (!NvwDeviceContextService.IsCurrentSession(
                    NvwDeviceContextService.BuildSessionStamp(tablet.LoginIssuedAt),
                    tokenSessionStamp))
                return NvwResult<NvwBedTablet>.Forbidden(NvwErrorCodes.SessionStale, NvwMessages.SessionStale);

            await TouchLastOnlineAsync(tablet, cancellationToken);

            return NvwResult<NvwBedTablet>.Ok(tablet);
        }

        public async Task<NvwResult<NvwBedTabletRuntimeResponse>> GetCurrentAsync(
            Guid userId,
            string? tokenSessionStamp,
            CancellationToken cancellationToken)
        {
            var resolved = await ResolveActiveTabletAsync(userId, tokenSessionStamp, cancellationToken);

            if (!resolved.IsSuccess || resolved.Value == null)
                return NvwResult<NvwBedTabletRuntimeResponse>.Forbidden(
                    resolved.Code ?? NvwErrorCodes.BedTabletUnavailable,
                    resolved.Message ?? NvwMessages.BedTabletUnavailable);

            var tablet = resolved.Value;

            var bed = await _dbContext.MstBeds.AsNoTracking()
                .Where(x => x.Id == tablet.BedId)
                .Select(x => new
                {
                    x.BedCode,
                    x.BedName,
                    RoomName = x.Room != null ? x.Room.RoomName : string.Empty,
                    FloorCode = x.Room != null ? x.Room.FloorCode : null
                })
                .FirstOrDefaultAsync(cancellationToken);

            var isBedOccupied = await _dbContext.InpBedPlacements.AsNoTracking()
                .AnyAsync(x => x.BedId == tablet.BedId && x.EndDateTime == null && !x.IsDelete, cancellationToken);

            var activeCall = await _calls.GetForTabletAsync(tablet, cancellationToken);

            return NvwResult<NvwBedTabletRuntimeResponse>.Ok(new NvwBedTabletRuntimeResponse
            {
                DeviceCode = tablet.DeviceCode,
                BedCode = bed?.BedCode ?? string.Empty,
                BedName = bed?.BedName ?? string.Empty,
                RoomName = bed?.RoomName ?? string.Empty,
                FloorCode = NvwDisplayDeviceService.NormalizeFloorCode(bed?.FloorCode),
                IsBedOccupied = isBedOccupied,
                ActiveCall = activeCall,
                RefreshIntervalSeconds = activeCall == null ? IdleRefreshIntervalSeconds : ActiveCallRefreshIntervalSeconds,
                ServerTimeUtc = DateTime.UtcNow
            });
        }

        private async Task TouchLastOnlineAsync(NvwBedTablet tablet, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            if (tablet.LastOnlineAt.HasValue &&
                (now - tablet.LastOnlineAt.Value).TotalSeconds < LastOnlineWriteGapSeconds)
                return;

            tablet.LastOnlineAt = now;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                _dbContext.Entry(tablet).State = EntityState.Detached;
            }
        }
    }
}
