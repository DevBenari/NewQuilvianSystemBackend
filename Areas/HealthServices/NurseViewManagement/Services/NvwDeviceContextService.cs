using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Models;
using QuilvianSystemBackend.Repositories;
using System.Globalization;

namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Services
{
    public class NvwDeviceContextService
    {
        public const string DeviceIdClaim = "nurse_view_device_id";
        public const string SessionStampClaim = "nurse_view_session_stamp";
        public const int RefreshIntervalSeconds = 60;
        public const int StaleAfterSeconds = 120;

        private const int LastOnlineWriteGapSeconds = 60;

        private readonly ApplicationDbContext _dbContext;

        public NvwDeviceContextService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public static string BuildSessionStamp(DateTime? loginIssuedAt) =>
            loginIssuedAt?.Ticks.ToString(CultureInfo.InvariantCulture) ?? string.Empty;

        public static bool IsCurrentSession(string expectedStamp, string? tokenStamp) =>
            string.Equals(expectedStamp, tokenStamp, StringComparison.Ordinal);

        public async Task<NvwDeviceLoginContext> ResolveLoginContextAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var device = await _dbContext.NvwDisplayDevices.AsNoTracking()
                .FirstOrDefaultAsync(x => x.LoginUserId == userId && !x.IsDelete, cancellationToken);

            if (device == null)
                return NvwDeviceLoginContext.None();

            var canLogin = device.DeviceStatus == NvwDisplayDeviceStatus.Active;

            return new NvwDeviceLoginContext
            {
                IsNurseViewAccount = true,
                DeviceId = device.Id,
                DeviceCode = device.DeviceCode,
                SessionStamp = BuildSessionStamp(device.LoginIssuedAt),
                CanLogin = canLogin,
                BlockReason = canLogin ? null : NvwMessages.DeviceInactive
            };
        }

        public async Task<NvwResult<NvwDisplayDevice>> ResolveActiveDeviceAsync(
            Guid userId,
            string? tokenSessionStamp,
            CancellationToken cancellationToken)
        {
            var device = await _dbContext.NvwDisplayDevices
                .FirstOrDefaultAsync(x => x.LoginUserId == userId && !x.IsDelete, cancellationToken);

            if (device == null)
                return NvwResult<NvwDisplayDevice>.Forbidden(NvwErrorCodes.NotDisplayAccount, NvwMessages.NotDisplayAccount);

            if (device.DeviceStatus != NvwDisplayDeviceStatus.Active)
                return NvwResult<NvwDisplayDevice>.Forbidden(NvwErrorCodes.DeviceInactive, NvwMessages.DeviceInactive);

            if (!IsCurrentSession(BuildSessionStamp(device.LoginIssuedAt), tokenSessionStamp))
                return NvwResult<NvwDisplayDevice>.Forbidden(NvwErrorCodes.SessionStale, NvwMessages.SessionStale);

            await TouchLastOnlineAsync(device, cancellationToken);

            return NvwResult<NvwDisplayDevice>.Ok(device);
        }

        public async Task<string?> FindFloorCodeAsync(Guid userId, CancellationToken cancellationToken) =>
            await _dbContext.NvwDisplayDevices.AsNoTracking()
                .Where(x => x.LoginUserId == userId && !x.IsDelete)
                .Select(x => x.FloorCode)
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<NvwResult<NvwDisplayRuntimeCurrentResponse>> GetCurrentAsync(
            Guid userId,
            string? tokenSessionStamp,
            CancellationToken cancellationToken)
        {
            var resolved = await ResolveActiveDeviceAsync(userId, tokenSessionStamp, cancellationToken);

            if (!resolved.IsSuccess || resolved.Value == null)
                return NvwResult<NvwDisplayRuntimeCurrentResponse>.Forbidden(
                    resolved.Code ?? NvwErrorCodes.NotDisplayAccount,
                    resolved.Message ?? NvwMessages.NotDisplayAccount);

            var device = resolved.Value;

            return NvwResult<NvwDisplayRuntimeCurrentResponse>.Ok(new NvwDisplayRuntimeCurrentResponse
            {
                DeviceCode = device.DeviceCode,
                DeviceName = device.DeviceName,
                FloorCode = device.FloorCode,
                FloorName = FloorPlanRoomNaming.FloorName(device.FloorCode),
                RefreshIntervalSeconds = RefreshIntervalSeconds,
                StaleAfterSeconds = StaleAfterSeconds,
                ServerTimeUtc = DateTime.UtcNow
            });
        }

        private async Task TouchLastOnlineAsync(NvwDisplayDevice device, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            if (device.LastOnlineAt.HasValue &&
                (now - device.LastOnlineAt.Value).TotalSeconds < LastOnlineWriteGapSeconds)
                return;

            device.LastOnlineAt = now;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                _dbContext.Entry(device).State = EntityState.Detached;
            }
        }
    }
}
