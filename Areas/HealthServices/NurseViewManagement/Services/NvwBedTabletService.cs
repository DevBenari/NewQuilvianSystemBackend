using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
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
    public class NvwBedTabletService
    {
        public const string TabletSequenceKey = "NVW_BED_TABLET";
        public const string TabletCodePrefix = "NVT";
        public const int TabletSequenceDigits = 6;
        public const string BedInUseIndexName = "IX_NvwBedTablet_BedId_InUse";

        public const string ActionUpdate = "Update";
        public const string ActionCreateLogin = "CreateLogin";
        public const string ActionResetLogin = "ResetLogin";
        public const string ActionDeactivate = "Deactivate";

        private const int DefaultPageSize = 25;
        private const int MaxPageSize = 100;
        private const int MaxDeviceNameLength = 100;
        private const int MinReasonLength = 5;
        private const int MaxReasonLength = 500;

        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly NumberSeriesAllocator _allocator;
        private readonly NvwDisplayDeviceService _displayDevices;

        public NvwBedTabletService(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            NumberSeriesAllocator allocator,
            NvwDisplayDeviceService displayDevices)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _allocator = allocator;
            _displayDevices = displayDevices;
        }

        public static string StatusLabel(NvwBedTabletStatus status) => status switch
        {
            NvwBedTabletStatus.PendingLogin => "Belum ada login",
            NvwBedTabletStatus.Active => "Aktif",
            NvwBedTabletStatus.Inactive => "Nonaktif",
            _ => status.ToString()
        };

        public static List<string> AvailableActions(NvwBedTabletStatus status) => status switch
        {
            NvwBedTabletStatus.PendingLogin => new List<string> { ActionUpdate, ActionCreateLogin, ActionDeactivate },
            NvwBedTabletStatus.Active => new List<string> { ActionUpdate, ActionResetLogin, ActionDeactivate },
            NvwBedTabletStatus.Inactive => new List<string> { ActionUpdate, ActionResetLogin },
            _ => new List<string>()
        };

        public async Task<PagedResult<NvwBedTabletResponse>> GetListAsync(
            NvwBedTabletPagedQuery query,
            CancellationToken cancellationToken)
        {
            var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
            var pageSize = query.PageSize <= 0 ? DefaultPageSize : Math.Min(query.PageSize, MaxPageSize);

            var rows = _dbContext.NvwBedTablets.AsNoTracking().Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(query.FloorCode))
            {
                var floorCode = NvwDisplayDeviceService.NormalizeFloorCode(query.FloorCode);
                rows = rows.Where(x =>
                    x.Bed!.Room!.FloorCode != null &&
                    x.Bed!.Room!.FloorCode!.Trim().ToUpper() == floorCode);
            }

            if (query.Status.HasValue)
                rows = rows.Where(x => x.DeviceStatus == query.Status.Value);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var keyword = query.Search.Trim().ToLower();
                rows = rows.Where(x =>
                    x.DeviceCode.ToLower().Contains(keyword) ||
                    x.DeviceName.ToLower().Contains(keyword) ||
                    (x.LocationNote != null && x.LocationNote.ToLower().Contains(keyword)) ||
                    x.Bed!.BedCode.ToLower().Contains(keyword));
            }

            var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            rows = (query.SortBy ?? "deviceCode").ToLowerInvariant() switch
            {
                "devicename" => descending ? rows.OrderByDescending(x => x.DeviceName) : rows.OrderBy(x => x.DeviceName),
                "bedcode" => descending ? rows.OrderByDescending(x => x.Bed!.BedCode) : rows.OrderBy(x => x.Bed!.BedCode),
                "floorcode" => descending ? rows.OrderByDescending(x => x.Bed!.Room!.FloorCode) : rows.OrderBy(x => x.Bed!.Room!.FloorCode),
                "devicestatus" => descending ? rows.OrderByDescending(x => x.DeviceStatus) : rows.OrderBy(x => x.DeviceStatus),
                "lastonlineat" => descending ? rows.OrderByDescending(x => x.LastOnlineAt) : rows.OrderBy(x => x.LastOnlineAt),
                _ => descending ? rows.OrderByDescending(x => x.DeviceCode) : rows.OrderBy(x => x.DeviceCode)
            };

            var total = await rows.CountAsync(cancellationToken);
            var tablets = await rows
                .Include(x => x.Bed)
                .ThenInclude(x => x!.Room)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
            var now = DateTime.UtcNow;

            return new PagedResult<NvwBedTabletResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = (int)Math.Ceiling(total / (double)pageSize),
                Items = tablets.Select(x => MapSummary(x, LabelOf(x.Bed), now)).ToList()
            };
        }

        public Task<List<NvwFloorOptionResponse>> GetFloorsAsync(CancellationToken cancellationToken) =>
            _displayDevices.GetFloorsAsync(cancellationToken);

        public async Task<NvwResult<List<NvwBedOptionResponse>>> GetBedOptionsAsync(
            string? floorCode,
            CancellationToken cancellationToken)
        {
            var floor = NvwDisplayDeviceService.NormalizeFloorCode(floorCode);
            if (floor.Length == 0)
                return NvwResult<List<NvwBedOptionResponse>>.Rule(NvwErrorCodes.FloorUnknown, NvwMessages.FloorUnknown);

            var rooms = await _dbContext.MstRooms.AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.FloorCode != null && x.FloorCode.Trim().ToUpper() == floor)
                .Select(x => new { x.Id, x.RoomName })
                .ToListAsync(cancellationToken);

            if (rooms.Count == 0)
                return NvwResult<List<NvwBedOptionResponse>>.Rule(NvwErrorCodes.FloorUnknown, NvwMessages.FloorUnknown);

            var roomNames = rooms.ToDictionary(x => x.Id, x => x.RoomName);
            var roomIds = rooms.Select(x => x.Id).ToList();
            var beds = await _dbContext.MstBeds.AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && roomIds.Contains(x.RoomId))
                .Select(x => new { x.Id, x.RoomId, x.BedCode, x.BedName })
                .ToListAsync(cancellationToken);

            var bedIds = beds.Select(x => x.Id).ToList();
            var bedsInUse = (await _dbContext.NvwBedTablets.AsNoTracking()
                    .Where(x => !x.IsDelete && x.DeviceStatus != NvwBedTabletStatus.Inactive && bedIds.Contains(x.BedId))
                    .Select(x => x.BedId)
                    .ToListAsync(cancellationToken))
                .ToHashSet();

            return NvwResult<List<NvwBedOptionResponse>>.Ok(beds
                .Select(x => new NvwBedOptionResponse
                {
                    BedId = x.Id,
                    BedCode = x.BedCode,
                    BedName = x.BedName,
                    RoomName = roomNames.GetValueOrDefault(x.RoomId) ?? string.Empty,
                    FloorCode = floor,
                    HasTabletInUse = bedsInUse.Contains(x.Id)
                })
                .OrderBy(x => x.BedCode, StringComparer.OrdinalIgnoreCase)
                .ToList());
        }

        public async Task<NvwBedTabletDetailResponse?> GetDetailAsync(Guid id, CancellationToken cancellationToken)
        {
            var tablet = await _dbContext.NvwBedTablets.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            return tablet == null ? null : await MapDetailAsync(tablet, cancellationToken);
        }

        public async Task<NvwResult<NvwBedTabletDetailResponse>> CreateAsync(
            NvwBedTabletCreateRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var deviceName = NormalizeDeviceName(request.DeviceName);
            if (deviceName == null)
                return NvwResult<NvwBedTabletDetailResponse>.Invalid(NvwErrorCodes.TabletNameRequired, NvwMessages.TabletNameRequired);

            var bed = await FindAssignableBedAsync(request.BedId, cancellationToken);
            if (bed == null)
                return NvwResult<NvwBedTabletDetailResponse>.Rule(NvwErrorCodes.BedInvalid, NvwMessages.BedInvalid);

            if (await BedHasTabletInUseAsync(bed.BedId, null, cancellationToken))
                return NvwResult<NvwBedTabletDetailResponse>.Conflict(NvwErrorCodes.BedHasTablet, NvwMessages.BedHasTablet);

            string deviceCode;
            try
            {
                deviceCode = await _allocator.AllocateAsync(
                    new NumberAllocationRequest(
                        SequenceKey: TabletSequenceKey,
                        Prefix: TabletCodePrefix,
                        ResetPolicy: NumberSeriesResetPolicies.Never,
                        SequenceDigits: TabletSequenceDigits,
                        ActorUserId: actorUserId,
                        Instant: DateTimeOffset.UtcNow,
                        AllowSequenceOverflow: true),
                    cancellationToken);
            }
            catch (NumberSeriesAllocationException)
            {
                return NvwResult<NvwBedTabletDetailResponse>.Conflict(NvwErrorCodes.CodeAllocationFailed, NvwMessages.CodeAllocationFailed);
            }

            var now = DateTime.UtcNow;
            var tablet = new NvwBedTablet
            {
                DeviceCode = deviceCode,
                DeviceName = deviceName,
                BedId = bed.BedId,
                LocationNote = Normalize(request.LocationNote),
                Description = Normalize(request.Description),
                DeviceStatus = NvwBedTabletStatus.PendingLogin,
                Version = 0,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.NvwBedTablets.Add(tablet);

            var saved = await SaveAsync(cancellationToken);
            if (saved != SaveOutcome.Saved)
                return SaveFailure<NvwBedTabletDetailResponse>(saved);

            return NvwResult<NvwBedTabletDetailResponse>.Created(await MapDetailAsync(tablet, cancellationToken));
        }

        public async Task<NvwResult<NvwBedTabletDetailResponse>> UpdateAsync(
            Guid id,
            NvwBedTabletUpdateRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var tablet = await _dbContext.NvwBedTablets.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (tablet == null)
                return NvwResult<NvwBedTabletDetailResponse>.NotFound(NvwMessages.TabletNotFound);

            if (tablet.Version != request.Version)
                return NvwResult<NvwBedTabletDetailResponse>.Conflict(NvwErrorCodes.TabletVersionMismatch, NvwMessages.TabletVersionMismatch);

            var deviceName = NormalizeDeviceName(request.DeviceName);
            if (deviceName == null)
                return NvwResult<NvwBedTabletDetailResponse>.Invalid(NvwErrorCodes.TabletNameRequired, NvwMessages.TabletNameRequired);

            var bed = await FindAssignableBedAsync(request.BedId, cancellationToken);
            if (bed == null)
                return NvwResult<NvwBedTabletDetailResponse>.Rule(NvwErrorCodes.BedInvalid, NvwMessages.BedInvalid);

            string? previousBedCode = null;
            if (bed.BedId != tablet.BedId)
            {
                if (await BedHasActiveCallAsync(tablet.BedId, cancellationToken))
                    return NvwResult<NvwBedTabletDetailResponse>.Conflict(NvwErrorCodes.BedHasActiveCall, NvwMessages.BedHasActiveCall);

                if (await BedHasTabletInUseAsync(bed.BedId, tablet.Id, cancellationToken))
                    return NvwResult<NvwBedTabletDetailResponse>.Conflict(NvwErrorCodes.BedHasTablet, NvwMessages.BedHasTablet);

                previousBedCode = (await LoadBedLabelAsync(tablet.BedId, cancellationToken)).BedCode;
            }

            tablet.DeviceName = deviceName;
            tablet.BedId = bed.BedId;
            tablet.LocationNote = Normalize(request.LocationNote);
            tablet.Description = Normalize(request.Description);
            tablet.UpdateDateTime = DateTime.UtcNow;
            tablet.UpdateBy = actorUserId;
            tablet.Version++;

            var saved = await SaveAsync(cancellationToken);
            if (saved != SaveOutcome.Saved)
                return SaveFailure<NvwBedTabletDetailResponse>(saved);

            var detail = await MapDetailAsync(tablet, cancellationToken);
            detail.PreviousBedCode = previousBedCode;
            return NvwResult<NvwBedTabletDetailResponse>.Ok(detail);
        }

        public async Task<NvwResult<NvwBedTabletLoginResponse>> CreateLoginAsync(
            Guid id,
            NvwBedTabletLoginRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var tablet = await _dbContext.NvwBedTablets.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (tablet == null)
                return NvwResult<NvwBedTabletLoginResponse>.NotFound(NvwMessages.TabletNotFound);

            if (tablet.Version != request.Version)
                return NvwResult<NvwBedTabletLoginResponse>.Conflict(NvwErrorCodes.TabletVersionMismatch, NvwMessages.TabletVersionMismatch);

            if (tablet.DeviceStatus != NvwBedTabletStatus.PendingLogin || tablet.LoginUserId.HasValue)
                return NvwResult<NvwBedTabletLoginResponse>.Conflict(NvwErrorCodes.TabletLoginAlreadyExists, NvwMessages.TabletLoginAlreadyExists);

            var userName = BuildLoginUserName(tablet.DeviceCode);
            var email = $"{userName}@{NvwDisplayDeviceService.LoginEmailDomain}";
            if (await _userManager.FindByNameAsync(userName) != null || await _userManager.FindByEmailAsync(email) != null)
                return NvwResult<NvwBedTabletLoginResponse>.Conflict(NvwErrorCodes.LoginAccountConflict, NvwMessages.LoginAccountConflict);

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
                UserCode = tablet.DeviceCode,
                DisplayName = tablet.DeviceName
            };

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var created = await _userManager.CreateAsync(user, password);
            if (!created.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                return NvwResult<NvwBedTabletLoginResponse>.Invalid(NvwErrorCodes.LoginAccountConflict, IdentityErrorMessage(created));
            }

            var now = DateTime.UtcNow;
            tablet.LoginUserId = user.Id;
            tablet.LoginIssuedAt = now;
            tablet.LoginIssuedBy = actorUserId;
            tablet.DeviceStatus = NvwBedTabletStatus.Active;
            tablet.UpdateDateTime = now;
            tablet.UpdateBy = actorUserId;
            tablet.Version++;

            var saved = await SaveAsync(cancellationToken);
            if (saved != SaveOutcome.Saved)
            {
                await transaction.RollbackAsync(cancellationToken);
                return SaveFailure<NvwBedTabletLoginResponse>(saved);
            }

            await transaction.CommitAsync(cancellationToken);

            return NvwResult<NvwBedTabletLoginResponse>.Ok(
                await MapLoginAsync(tablet, userName, password, cancellationToken));
        }

        public async Task<NvwResult<NvwBedTabletLoginResponse>> ResetLoginAsync(
            Guid id,
            NvwBedTabletLoginRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var tablet = await _dbContext.NvwBedTablets.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (tablet == null)
                return NvwResult<NvwBedTabletLoginResponse>.NotFound(NvwMessages.TabletNotFound);

            if (tablet.Version != request.Version)
                return NvwResult<NvwBedTabletLoginResponse>.Conflict(NvwErrorCodes.TabletVersionMismatch, NvwMessages.TabletVersionMismatch);

            if (tablet.DeviceStatus == NvwBedTabletStatus.PendingLogin || !tablet.LoginUserId.HasValue)
                return NvwResult<NvwBedTabletLoginResponse>.Conflict(NvwErrorCodes.TabletLoginNotCreated, NvwMessages.TabletLoginNotCreated);

            if (tablet.DeviceStatus == NvwBedTabletStatus.Inactive &&
                await BedHasTabletInUseAsync(tablet.BedId, tablet.Id, cancellationToken))
                return NvwResult<NvwBedTabletLoginResponse>.Conflict(NvwErrorCodes.BedHasTablet, NvwMessages.BedHasTablet);

            var user = await _userManager.FindByIdAsync(tablet.LoginUserId.Value.ToString());
            if (user == null)
                return NvwResult<NvwBedTabletLoginResponse>.Conflict(NvwErrorCodes.TabletLoginNotCreated, NvwMessages.TabletLoginNotCreated);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var password = GeneratePassword();
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var reset = await _userManager.ResetPasswordAsync(user, resetToken, password);
            if (!reset.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                return NvwResult<NvwBedTabletLoginResponse>.Invalid(NvwErrorCodes.LoginAccountConflict, IdentityErrorMessage(reset));
            }

            await _userManager.ResetAccessFailedCountAsync(user);
            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.UpdateSecurityStampAsync(user);

            var now = DateTime.UtcNow;
            tablet.DeviceStatus = NvwBedTabletStatus.Active;
            tablet.LoginIssuedAt = now;
            tablet.LoginIssuedBy = actorUserId;
            tablet.UpdateDateTime = now;
            tablet.UpdateBy = actorUserId;
            tablet.Version++;

            var saved = await SaveAsync(cancellationToken);
            if (saved != SaveOutcome.Saved)
            {
                await transaction.RollbackAsync(cancellationToken);
                return SaveFailure<NvwBedTabletLoginResponse>(saved);
            }

            await transaction.CommitAsync(cancellationToken);

            return NvwResult<NvwBedTabletLoginResponse>.Ok(
                await MapLoginAsync(tablet, user.UserName ?? string.Empty, password, cancellationToken));
        }

        public async Task<NvwResult<NvwBedTabletDetailResponse>> DeactivateAsync(
            Guid id,
            NvwBedTabletDeactivateRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var tablet = await _dbContext.NvwBedTablets.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (tablet == null)
                return NvwResult<NvwBedTabletDetailResponse>.NotFound(NvwMessages.TabletNotFound);

            if (tablet.Version != request.Version)
                return NvwResult<NvwBedTabletDetailResponse>.Conflict(NvwErrorCodes.TabletVersionMismatch, NvwMessages.TabletVersionMismatch);

            if (tablet.DeviceStatus == NvwBedTabletStatus.Inactive)
                return NvwResult<NvwBedTabletDetailResponse>.Conflict(NvwErrorCodes.TabletAlreadyInactive, NvwMessages.TabletAlreadyInactive);

            var reason = Normalize(request.Reason);
            if (reason == null || reason.Length < MinReasonLength || reason.Length > MaxReasonLength)
                return NvwResult<NvwBedTabletDetailResponse>.Invalid(NvwErrorCodes.TabletDeactivationReason, NvwMessages.TabletDeactivationReason);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            if (tablet.LoginUserId.HasValue)
            {
                var user = await _userManager.FindByIdAsync(tablet.LoginUserId.Value.ToString());
                if (user != null)
                {
                    await _userManager.SetLockoutEnabledAsync(user, true);
                    await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
                    await _userManager.UpdateSecurityStampAsync(user);
                }
            }

            var now = DateTime.UtcNow;
            tablet.DeviceStatus = NvwBedTabletStatus.Inactive;
            tablet.DeactivatedAt = now;
            tablet.DeactivatedBy = actorUserId;
            tablet.DeactivationReason = reason;
            tablet.UpdateDateTime = now;
            tablet.UpdateBy = actorUserId;
            tablet.Version++;

            var saved = await SaveAsync(cancellationToken);
            if (saved != SaveOutcome.Saved)
            {
                await transaction.RollbackAsync(cancellationToken);
                return SaveFailure<NvwBedTabletDetailResponse>(saved);
            }

            await transaction.CommitAsync(cancellationToken);

            return NvwResult<NvwBedTabletDetailResponse>.Ok(await MapDetailAsync(tablet, cancellationToken));
        }

        private async Task<BedLabel?> FindAssignableBedAsync(Guid bedId, CancellationToken cancellationToken)
        {
            if (bedId == Guid.Empty)
                return null;

            var bed = await _dbContext.MstBeds.AsNoTracking()
                .Where(x =>
                    x.Id == bedId &&
                    !x.IsDelete &&
                    x.IsActive &&
                    x.Room != null &&
                    !x.Room.IsDelete &&
                    x.Room.IsActive &&
                    x.Room.FloorCode != null &&
                    x.Room.FloorCode != string.Empty)
                .Select(x => new BedLabel
                {
                    BedId = x.Id,
                    BedCode = x.BedCode,
                    BedName = x.BedName,
                    RoomName = x.Room!.RoomName,
                    FloorCode = x.Room!.FloorCode ?? string.Empty
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (bed == null)
                return null;

            bed.FloorCode = NvwDisplayDeviceService.NormalizeFloorCode(bed.FloorCode);
            return bed.FloorCode.Length == 0 ? null : bed;
        }

        private async Task<BedLabel> LoadBedLabelAsync(Guid bedId, CancellationToken cancellationToken)
        {
            var bed = await _dbContext.MstBeds.AsNoTracking()
                .Include(x => x.Room)
                .FirstOrDefaultAsync(x => x.Id == bedId, cancellationToken);
            return LabelOf(bed);
        }

        private Task<bool> BedHasTabletInUseAsync(Guid bedId, Guid? exceptTabletId, CancellationToken cancellationToken) =>
            _dbContext.NvwBedTablets.AsNoTracking().AnyAsync(x =>
                    x.BedId == bedId &&
                    !x.IsDelete &&
                    x.DeviceStatus != NvwBedTabletStatus.Inactive &&
                    (!exceptTabletId.HasValue || x.Id != exceptTabletId.Value),
                cancellationToken);

        private Task<bool> BedHasActiveCallAsync(Guid bedId, CancellationToken cancellationToken) =>
            _dbContext.NvwPatientCalls.AsNoTracking().AnyAsync(x =>
                    x.BedId == bedId &&
                    !x.IsDelete &&
                    (x.CallStatus == NvwPatientCallStatus.Waiting || x.CallStatus == NvwPatientCallStatus.Claimed),
                cancellationToken);

        private async Task<SaveOutcome> SaveAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                return SaveOutcome.Saved;
            }
            catch (DbUpdateConcurrencyException)
            {
                return SaveOutcome.VersionConflict;
            }
            catch (DbUpdateException exception) when (IsBedInUseViolation(exception))
            {
                return SaveOutcome.BedInUse;
            }
        }

        private static bool IsBedInUseViolation(DbUpdateException exception) =>
            exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } postgres &&
            string.Equals(postgres.ConstraintName, BedInUseIndexName, StringComparison.Ordinal);

        private static NvwResult<T> SaveFailure<T>(SaveOutcome outcome) => outcome == SaveOutcome.BedInUse
            ? NvwResult<T>.Conflict(NvwErrorCodes.BedHasTablet, NvwMessages.BedHasTablet)
            : NvwResult<T>.Conflict(NvwErrorCodes.TabletVersionMismatch, NvwMessages.TabletVersionMismatch);

        private static BedLabel LabelOf(MstBed? bed) => bed == null
            ? new BedLabel()
            : new BedLabel
            {
                BedId = bed.Id,
                BedCode = bed.BedCode,
                BedName = bed.BedName,
                RoomName = bed.Room?.RoomName ?? string.Empty,
                FloorCode = NvwDisplayDeviceService.NormalizeFloorCode(bed.Room?.FloorCode)
            };

        private static NvwBedTabletResponse MapSummary(NvwBedTablet tablet, BedLabel bed, DateTime nowUtc) => new()
        {
            Id = tablet.Id,
            DeviceCode = tablet.DeviceCode,
            DeviceName = tablet.DeviceName,
            BedId = tablet.BedId,
            BedCode = bed.BedCode,
            BedName = bed.BedName,
            RoomName = bed.RoomName,
            FloorCode = bed.FloorCode,
            DeviceStatus = tablet.DeviceStatus,
            DeviceStatusLabel = StatusLabel(tablet.DeviceStatus),
            HasLogin = tablet.LoginUserId.HasValue,
            LastOnlineAt = tablet.LastOnlineAt,
            IsOnline = tablet.LastOnlineAt.HasValue &&
                       tablet.LastOnlineAt.Value >= nowUtc.AddMinutes(-NvwDisplayDeviceService.OnlineWindowMinutes)
        };

        private async Task<NvwBedTabletDetailResponse> MapDetailAsync(NvwBedTablet tablet, CancellationToken cancellationToken)
        {
            var bed = await LoadBedLabelAsync(tablet.BedId, cancellationToken);
            var hasActiveCall = await BedHasActiveCallAsync(tablet.BedId, cancellationToken);

            var userIds = new[] { tablet.LoginUserId, tablet.LoginIssuedBy, tablet.DeactivatedBy }
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

            var summary = MapSummary(tablet, bed, DateTime.UtcNow);
            return new NvwBedTabletDetailResponse
            {
                Id = summary.Id,
                DeviceCode = summary.DeviceCode,
                DeviceName = summary.DeviceName,
                BedId = summary.BedId,
                BedCode = summary.BedCode,
                BedName = summary.BedName,
                RoomName = summary.RoomName,
                FloorCode = summary.FloorCode,
                DeviceStatus = summary.DeviceStatus,
                DeviceStatusLabel = summary.DeviceStatusLabel,
                HasLogin = summary.HasLogin,
                LastOnlineAt = summary.LastOnlineAt,
                IsOnline = summary.IsOnline,
                LocationNote = tablet.LocationNote,
                Description = tablet.Description,
                LoginUserName = UserNameOf(tablet.LoginUserId),
                LoginIssuedAt = tablet.LoginIssuedAt,
                LoginIssuedByName = NameOf(tablet.LoginIssuedBy),
                DeactivatedAt = tablet.DeactivatedAt,
                DeactivatedByName = NameOf(tablet.DeactivatedBy),
                DeactivationReason = tablet.DeactivationReason,
                HasActiveCall = hasActiveCall,
                Version = tablet.Version,
                AvailableActions = AvailableActions(tablet.DeviceStatus)
            };
        }

        private async Task<NvwBedTabletLoginResponse> MapLoginAsync(
            NvwBedTablet tablet,
            string loginUserName,
            string password,
            CancellationToken cancellationToken)
        {
            var bed = await LoadBedLabelAsync(tablet.BedId, cancellationToken);
            return new NvwBedTabletLoginResponse
            {
                TabletId = tablet.Id,
                DeviceCode = tablet.DeviceCode,
                LoginUserName = loginUserName,
                Password = password,
                Version = tablet.Version,
                BedCode = bed.BedCode,
                DeviceStatusLabel = StatusLabel(tablet.DeviceStatus)
            };
        }

        private static string? NormalizeDeviceName(string? value)
        {
            var name = Normalize(value);
            return name == null || name.Length > MaxDeviceNameLength ? null : name;
        }

        private static string BuildLoginUserName(string deviceCode) =>
            deviceCode.Trim().ToLowerInvariant();

        private static string GeneratePassword()
        {
            var bytes = RandomNumberGenerator.GetBytes(8);
            return $"Nvt@{Convert.ToHexString(bytes)}aA1!";
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

        private enum SaveOutcome
        {
            Saved = 1,
            VersionConflict = 2,
            BedInUse = 3
        }

        private sealed class BedLabel
        {
            public Guid BedId { get; set; }
            public string BedCode { get; set; } = string.Empty;
            public string BedName { get; set; } = string.Empty;
            public string RoomName { get; set; } = string.Empty;
            public string FloorCode { get; set; } = string.Empty;
        }
    }
}
