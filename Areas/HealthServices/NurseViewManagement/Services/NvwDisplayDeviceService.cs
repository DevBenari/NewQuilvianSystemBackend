using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using System.Security.Cryptography;

namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Services
{
    public class NvwDisplayDeviceService
    {
        public const string DeviceSequenceKey = "NVW_DISPLAY_DEVICE";
        public const string DeviceCodePrefix = "NVW";
        public const int DeviceSequenceDigits = 6;
        public const int OnlineWindowMinutes = 3;
        public const string LoginEmailDomain = "nurse-view.local";

        public const string ActionUpdate = "Update";
        public const string ActionCreateLogin = "CreateLogin";
        public const string ActionResetLogin = "ResetLogin";
        public const string ActionDeactivate = "Deactivate";

        private const int DefaultPageSize = 25;
        private const int MaxPageSize = 100;

        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly NumberSeriesAllocator _allocator;

        public NvwDisplayDeviceService(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            NumberSeriesAllocator allocator)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _allocator = allocator;
        }

        public static string StatusLabel(NvwDisplayDeviceStatus status) => status switch
        {
            NvwDisplayDeviceStatus.PendingLogin => "Belum ada login",
            NvwDisplayDeviceStatus.Active => "Aktif",
            NvwDisplayDeviceStatus.Inactive => "Nonaktif",
            _ => status.ToString()
        };

        public static List<string> AvailableActions(NvwDisplayDeviceStatus status) => status switch
        {
            NvwDisplayDeviceStatus.PendingLogin => new List<string> { ActionUpdate, ActionCreateLogin, ActionDeactivate },
            NvwDisplayDeviceStatus.Active => new List<string> { ActionUpdate, ActionResetLogin, ActionDeactivate },
            NvwDisplayDeviceStatus.Inactive => new List<string> { ActionUpdate, ActionResetLogin },
            _ => new List<string>()
        };

        public static string NormalizeFloorCode(string? floorCode) =>
            (floorCode ?? string.Empty).Trim().ToUpperInvariant();

        public async Task<PagedResult<NvwDisplayDeviceResponse>> GetListAsync(
            NvwDisplayDevicePagedQuery query,
            CancellationToken cancellationToken)
        {
            var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
            var pageSize = query.PageSize <= 0 ? DefaultPageSize : Math.Min(query.PageSize, MaxPageSize);

            var rows = _dbContext.NvwDisplayDevices.AsNoTracking().Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(query.FloorCode))
            {
                var floorCode = NormalizeFloorCode(query.FloorCode);
                rows = rows.Where(x => x.FloorCode == floorCode);
            }

            if (query.Status.HasValue)
                rows = rows.Where(x => x.DeviceStatus == query.Status.Value);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var keyword = query.Search.Trim().ToLower();
                rows = rows.Where(x =>
                    x.DeviceCode.ToLower().Contains(keyword) ||
                    x.DeviceName.ToLower().Contains(keyword) ||
                    (x.LocationName != null && x.LocationName.ToLower().Contains(keyword)));
            }

            var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            rows = (query.SortBy ?? "deviceCode").ToLowerInvariant() switch
            {
                "devicename" => descending ? rows.OrderByDescending(x => x.DeviceName) : rows.OrderBy(x => x.DeviceName),
                "floorcode" => descending ? rows.OrderByDescending(x => x.FloorCode) : rows.OrderBy(x => x.FloorCode),
                "devicestatus" => descending ? rows.OrderByDescending(x => x.DeviceStatus) : rows.OrderBy(x => x.DeviceStatus),
                "lastonlineat" => descending ? rows.OrderByDescending(x => x.LastOnlineAt) : rows.OrderBy(x => x.LastOnlineAt),
                _ => descending ? rows.OrderByDescending(x => x.DeviceCode) : rows.OrderBy(x => x.DeviceCode)
            };

            var total = await rows.CountAsync(cancellationToken);
            var devices = await rows.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
            var now = DateTime.UtcNow;

            return new PagedResult<NvwDisplayDeviceResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = (int)Math.Ceiling(total / (double)pageSize),
                Items = devices.Select(x => MapSummary(x, now)).ToList()
            };
        }

        public async Task<List<NvwFloorOptionResponse>> GetFloorsAsync(CancellationToken cancellationToken)
        {
            var rooms = await _dbContext.Set<MstRoom>().AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.FloorCode != null && x.FloorCode != string.Empty)
                .Select(x => new { x.Id, x.FloorCode })
                .ToListAsync(cancellationToken);

            var roomIds = rooms.Select(x => x.Id).ToList();
            var bedCounts = await _dbContext.Set<MstBed>().AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && roomIds.Contains(x.RoomId))
                .GroupBy(x => x.RoomId)
                .Select(g => new { RoomId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.RoomId, x => x.Count, cancellationToken);

            return rooms
                .GroupBy(x => NormalizeFloorCode(x.FloorCode))
                .Where(g => g.Key.Length > 0)
                .Select(g => new NvwFloorOptionResponse
                {
                    FloorCode = g.Key,
                    FloorName = FloorPlanRoomNaming.FloorName(g.Key),
                    RoomCount = g.Count(),
                    BedCount = g.Sum(r => bedCounts.GetValueOrDefault(r.Id))
                })
                .OrderBy(x => int.TryParse(x.FloorCode, out _) ? 0 : 1)
                .ThenBy(x => int.TryParse(x.FloorCode, out var number) ? number : 0)
                .ThenBy(x => x.FloorCode, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public async Task<NvwDisplayDeviceDetailResponse?> GetDetailAsync(Guid id, CancellationToken cancellationToken)
        {
            var device = await _dbContext.NvwDisplayDevices.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            return device == null ? null : await MapDetailAsync(device, cancellationToken);
        }

        public async Task<NvwResult<NvwDisplayDeviceDetailResponse>> CreateAsync(
            NvwDisplayDeviceCreateRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var floorCode = NormalizeFloorCode(request.FloorCode);
            if (!await IsKnownFloorAsync(floorCode, cancellationToken))
                return NvwResult<NvwDisplayDeviceDetailResponse>.Rule(NvwErrorCodes.FloorUnknown, NvwMessages.FloorUnknown);

            string deviceCode;
            try
            {
                deviceCode = await _allocator.AllocateAsync(
                    new NumberAllocationRequest(
                        SequenceKey: DeviceSequenceKey,
                        Prefix: DeviceCodePrefix,
                        ResetPolicy: NumberSeriesResetPolicies.Never,
                        SequenceDigits: DeviceSequenceDigits,
                        ActorUserId: actorUserId,
                        Instant: DateTimeOffset.UtcNow,
                        AllowSequenceOverflow: true),
                    cancellationToken);
            }
            catch (NumberSeriesAllocationException)
            {
                return NvwResult<NvwDisplayDeviceDetailResponse>.Conflict(NvwErrorCodes.CodeAllocationFailed, NvwMessages.CodeAllocationFailed);
            }

            var now = DateTime.UtcNow;
            var device = new NvwDisplayDevice
            {
                DeviceCode = deviceCode,
                DeviceName = request.DeviceName.Trim(),
                FloorCode = floorCode,
                LocationName = Normalize(request.LocationName),
                Description = Normalize(request.Description),
                DeviceStatus = NvwDisplayDeviceStatus.PendingLogin,
                Version = 0,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.NvwDisplayDevices.Add(device);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return NvwResult<NvwDisplayDeviceDetailResponse>.Created(await MapDetailAsync(device, cancellationToken));
        }

        public async Task<NvwResult<NvwDisplayDeviceDetailResponse>> UpdateAsync(
            Guid id,
            NvwDisplayDeviceUpdateRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var device = await _dbContext.NvwDisplayDevices.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (device == null)
                return NvwResult<NvwDisplayDeviceDetailResponse>.NotFound(NvwMessages.DeviceNotFound);

            if (device.Version != request.Version)
                return NvwResult<NvwDisplayDeviceDetailResponse>.Conflict(NvwErrorCodes.VersionMismatch, NvwMessages.VersionMismatch);

            var floorCode = NormalizeFloorCode(request.FloorCode);
            if (!await IsKnownFloorAsync(floorCode, cancellationToken))
                return NvwResult<NvwDisplayDeviceDetailResponse>.Rule(NvwErrorCodes.FloorUnknown, NvwMessages.FloorUnknown);

            device.DeviceName = request.DeviceName.Trim();
            device.FloorCode = floorCode;
            device.LocationName = Normalize(request.LocationName);
            device.Description = Normalize(request.Description);
            device.UpdateDateTime = DateTime.UtcNow;
            device.UpdateBy = actorUserId;
            device.Version++;

            if (!await TrySaveAsync(cancellationToken))
                return NvwResult<NvwDisplayDeviceDetailResponse>.Conflict(NvwErrorCodes.VersionMismatch, NvwMessages.VersionMismatch);

            return NvwResult<NvwDisplayDeviceDetailResponse>.Ok(await MapDetailAsync(device, cancellationToken));
        }

        public async Task<NvwResult<NvwDisplayDeviceLoginResponse>> CreateLoginAsync(
            Guid id,
            NvwDisplayDeviceLoginRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var device = await _dbContext.NvwDisplayDevices.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (device == null)
                return NvwResult<NvwDisplayDeviceLoginResponse>.NotFound(NvwMessages.DeviceNotFound);

            if (device.Version != request.Version)
                return NvwResult<NvwDisplayDeviceLoginResponse>.Conflict(NvwErrorCodes.VersionMismatch, NvwMessages.VersionMismatch);

            if (device.DeviceStatus != NvwDisplayDeviceStatus.PendingLogin || device.LoginUserId.HasValue)
                return NvwResult<NvwDisplayDeviceLoginResponse>.Conflict(NvwErrorCodes.LoginAlreadyExists, NvwMessages.LoginAlreadyExists);

            var userName = BuildLoginUserName(device.DeviceCode);
            var email = $"{userName}@{LoginEmailDomain}";
            if (await _userManager.FindByNameAsync(userName) != null || await _userManager.FindByEmailAsync(email) != null)
                return NvwResult<NvwDisplayDeviceLoginResponse>.Conflict(NvwErrorCodes.LoginAccountConflict, NvwMessages.LoginAccountConflict);

            var password = GeneratePassword();
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = userName,
                NormalizedUserName = userName.ToUpperInvariant(),
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                LockoutEnabled = true,
                UserCode = device.DeviceCode,
                DisplayName = device.DeviceName
            };

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var created = await _userManager.CreateAsync(user, password);
            if (!created.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                return NvwResult<NvwDisplayDeviceLoginResponse>.Invalid(NvwErrorCodes.LoginAccountConflict, IdentityErrorMessage(created));
            }

            var now = DateTime.UtcNow;
            device.LoginUserId = user.Id;
            device.LoginIssuedAt = now;
            device.LoginIssuedBy = actorUserId;
            device.DeviceStatus = NvwDisplayDeviceStatus.Active;
            device.UpdateDateTime = now;
            device.UpdateBy = actorUserId;
            device.Version++;

            if (!await TrySaveAsync(cancellationToken))
            {
                await transaction.RollbackAsync(cancellationToken);
                return NvwResult<NvwDisplayDeviceLoginResponse>.Conflict(NvwErrorCodes.VersionMismatch, NvwMessages.VersionMismatch);
            }

            await transaction.CommitAsync(cancellationToken);

            return NvwResult<NvwDisplayDeviceLoginResponse>.Ok(new NvwDisplayDeviceLoginResponse
            {
                DeviceId = device.Id,
                DeviceCode = device.DeviceCode,
                LoginUserName = userName,
                Password = password,
                Version = device.Version
            });
        }

        public async Task<NvwResult<NvwDisplayDeviceLoginResponse>> ResetLoginAsync(
            Guid id,
            NvwDisplayDeviceLoginRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var device = await _dbContext.NvwDisplayDevices.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (device == null)
                return NvwResult<NvwDisplayDeviceLoginResponse>.NotFound(NvwMessages.DeviceNotFound);

            if (device.Version != request.Version)
                return NvwResult<NvwDisplayDeviceLoginResponse>.Conflict(NvwErrorCodes.VersionMismatch, NvwMessages.VersionMismatch);

            if (device.DeviceStatus == NvwDisplayDeviceStatus.PendingLogin || !device.LoginUserId.HasValue)
                return NvwResult<NvwDisplayDeviceLoginResponse>.Conflict(NvwErrorCodes.LoginNotCreated, NvwMessages.LoginNotCreated);

            var user = await _userManager.FindByIdAsync(device.LoginUserId.Value.ToString());
            if (user == null)
                return NvwResult<NvwDisplayDeviceLoginResponse>.Conflict(NvwErrorCodes.LoginNotCreated, NvwMessages.LoginNotCreated);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var password = GeneratePassword();
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var reset = await _userManager.ResetPasswordAsync(user, resetToken, password);
            if (!reset.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                return NvwResult<NvwDisplayDeviceLoginResponse>.Invalid(NvwErrorCodes.LoginAccountConflict, IdentityErrorMessage(reset));
            }

            await _userManager.ResetAccessFailedCountAsync(user);
            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.UpdateSecurityStampAsync(user);

            var now = DateTime.UtcNow;
            device.DeviceStatus = NvwDisplayDeviceStatus.Active;
            device.LoginIssuedAt = now;
            device.LoginIssuedBy = actorUserId;
            device.UpdateDateTime = now;
            device.UpdateBy = actorUserId;
            device.Version++;

            if (!await TrySaveAsync(cancellationToken))
            {
                await transaction.RollbackAsync(cancellationToken);
                return NvwResult<NvwDisplayDeviceLoginResponse>.Conflict(NvwErrorCodes.VersionMismatch, NvwMessages.VersionMismatch);
            }

            await transaction.CommitAsync(cancellationToken);

            return NvwResult<NvwDisplayDeviceLoginResponse>.Ok(new NvwDisplayDeviceLoginResponse
            {
                DeviceId = device.Id,
                DeviceCode = device.DeviceCode,
                LoginUserName = user.UserName ?? string.Empty,
                Password = password,
                Version = device.Version
            });
        }

        public async Task<NvwResult<NvwDisplayDeviceDetailResponse>> DeactivateAsync(
            Guid id,
            NvwDisplayDeviceDeactivateRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var device = await _dbContext.NvwDisplayDevices.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (device == null)
                return NvwResult<NvwDisplayDeviceDetailResponse>.NotFound(NvwMessages.DeviceNotFound);

            if (device.Version != request.Version)
                return NvwResult<NvwDisplayDeviceDetailResponse>.Conflict(NvwErrorCodes.VersionMismatch, NvwMessages.VersionMismatch);

            if (device.DeviceStatus == NvwDisplayDeviceStatus.Inactive)
                return NvwResult<NvwDisplayDeviceDetailResponse>.Conflict(NvwErrorCodes.AlreadyInactive, NvwMessages.AlreadyInactive);

            var reason = Normalize(request.Reason);
            if (reason == null || reason.Length < 5 || reason.Length > 500)
                return NvwResult<NvwDisplayDeviceDetailResponse>.Invalid(NvwErrorCodes.DeactivationReason, NvwMessages.DeactivationReason);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            if (device.LoginUserId.HasValue)
            {
                var user = await _userManager.FindByIdAsync(device.LoginUserId.Value.ToString());
                if (user != null)
                {
                    await _userManager.SetLockoutEnabledAsync(user, true);
                    await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
                    await _userManager.UpdateSecurityStampAsync(user);
                }
            }

            var now = DateTime.UtcNow;
            device.DeviceStatus = NvwDisplayDeviceStatus.Inactive;
            device.DeactivatedAt = now;
            device.DeactivatedBy = actorUserId;
            device.DeactivationReason = reason;
            device.UpdateDateTime = now;
            device.UpdateBy = actorUserId;
            device.Version++;

            if (!await TrySaveAsync(cancellationToken))
            {
                await transaction.RollbackAsync(cancellationToken);
                return NvwResult<NvwDisplayDeviceDetailResponse>.Conflict(NvwErrorCodes.VersionMismatch, NvwMessages.VersionMismatch);
            }

            await transaction.CommitAsync(cancellationToken);

            return NvwResult<NvwDisplayDeviceDetailResponse>.Ok(await MapDetailAsync(device, cancellationToken));
        }

        private async Task<bool> IsKnownFloorAsync(string floorCode, CancellationToken cancellationToken)
        {
            if (floorCode.Length == 0)
                return false;

            return await _dbContext.Set<MstRoom>().AsNoTracking()
                .AnyAsync(x => !x.IsDelete && x.IsActive && x.FloorCode != null && x.FloorCode.Trim().ToUpper() == floorCode, cancellationToken);
        }

        private async Task<bool> TrySaveAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }

        private static NvwDisplayDeviceResponse MapSummary(NvwDisplayDevice device, DateTime nowUtc) => new()
        {
            Id = device.Id,
            DeviceCode = device.DeviceCode,
            DeviceName = device.DeviceName,
            FloorCode = device.FloorCode,
            FloorName = FloorPlanRoomNaming.FloorName(device.FloorCode),
            LocationName = device.LocationName,
            DeviceStatus = device.DeviceStatus,
            DeviceStatusLabel = StatusLabel(device.DeviceStatus),
            HasLogin = device.LoginUserId.HasValue,
            LastOnlineAt = device.LastOnlineAt,
            IsOnline = device.LastOnlineAt.HasValue && device.LastOnlineAt.Value >= nowUtc.AddMinutes(-OnlineWindowMinutes)
        };

        private async Task<NvwDisplayDeviceDetailResponse> MapDetailAsync(NvwDisplayDevice device, CancellationToken cancellationToken)
        {
            var userIds = new[] { device.LoginUserId, device.LoginIssuedBy, device.DeactivatedBy }
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList();

            var users = await _dbContext.Users.AsNoTracking()
                .Where(x => userIds.Contains(x.Id))
                .Select(x => new { x.Id, x.UserName, x.DisplayName })
                .ToDictionaryAsync(x => x.Id, cancellationToken);

            string? NameOf(Guid? userId) =>
                userId.HasValue && users.TryGetValue(userId.Value, out var user)
                    ? (string.IsNullOrWhiteSpace(user.DisplayName) ? user.UserName : user.DisplayName)
                    : null;

            string? UserNameOf(Guid? userId) =>
                userId.HasValue && users.TryGetValue(userId.Value, out var user) ? user.UserName : null;

            var summary = MapSummary(device, DateTime.UtcNow);
            return new NvwDisplayDeviceDetailResponse
            {
                Id = summary.Id,
                DeviceCode = summary.DeviceCode,
                DeviceName = summary.DeviceName,
                FloorCode = summary.FloorCode,
                FloorName = summary.FloorName,
                LocationName = summary.LocationName,
                DeviceStatus = summary.DeviceStatus,
                DeviceStatusLabel = summary.DeviceStatusLabel,
                HasLogin = summary.HasLogin,
                LastOnlineAt = summary.LastOnlineAt,
                IsOnline = summary.IsOnline,
                Description = device.Description,
                LoginUserName = UserNameOf(device.LoginUserId),
                LoginIssuedAt = device.LoginIssuedAt,
                LoginIssuedByName = NameOf(device.LoginIssuedBy),
                DeactivatedAt = device.DeactivatedAt,
                DeactivatedByName = NameOf(device.DeactivatedBy),
                DeactivationReason = device.DeactivationReason,
                Version = device.Version,
                AvailableActions = AvailableActions(device.DeviceStatus)
            };
        }

        private static string BuildLoginUserName(string deviceCode) =>
            deviceCode.Trim().ToLowerInvariant();

        private static string GeneratePassword()
        {
            var bytes = RandomNumberGenerator.GetBytes(8);
            return $"Nvw@{Convert.ToHexString(bytes)}aA1!";
        }

        private static string IdentityErrorMessage(IdentityResult result)
        {
            var errors = result.Errors
                .Select(x => x.Description)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();
            return errors.Count > 0 ? string.Join(" ", errors) : NvwMessages.LoginAccountConflict;
        }

        private static string? Normalize(string? value)
        {
            var trimmed = value?.Trim();
            return string.IsNullOrEmpty(trimmed) ? null : trimmed;
        }
    }
}
