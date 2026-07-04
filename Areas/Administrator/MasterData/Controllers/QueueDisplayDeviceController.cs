using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Models;
using System.Security.Claims;

using ResponseQueueDisplayDevicePagedResult = QuilvianSystemBackend.Responses.PagedResult<QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs.QueueDisplayDeviceResponse>;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/administrator/master-data/queue-display-devices")]
    [AccessController(moduleCode: "ADMINISTRATOR_MASTER_DATA", moduleName: "Administrator Master Data", displayName: "Queue Display Device", AreaName = "Administrator", ControllerName = "QueueDisplayDevice", Description = "Administrator master data queue display device", SortOrder = 104)]
    [Tags("Administrator / Master Data / Queue Display Device")]
    public class QueueDisplayDeviceController : ControllerBase
    {
        private const string LogCategory = "Administrator.MasterData";
        private const string CodePrefix = "QDD-RSMMC-";
        private const int CodeNumberLength = 5;
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly UserManager<ApplicationUser> _userManager;

        public QueueDisplayDeviceController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _userManager = userManager;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<QueueDisplayDeviceFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Queue Display Device", Description = "Melihat metadata filter display antrian", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("QueueDisplayDevice", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new QueueDisplayDeviceFilterMetadataResponse
            {
                SortOptions = new List<QueueDisplayDeviceSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" }, new() { Value = "createDateTime", Label = "Tanggal dibuat" }, new() { Value = "displayCode", Label = "Kode display" }, new() { Value = "displayName", Label = "Nama display" }, new() { Value = "clusterName", Label = "Nama cluster" }, new() { Value = "displayDeviceType", Label = "Tipe device" }, new() { Value = "layoutType", Label = "Tipe layout" }, new() { Value = "sessionExpireMinutes", Label = "Masa aktif session" }, new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                EnumOptions = BuildEnumMetadata()
            };
            return Ok(ApiResponse<QueueDisplayDeviceFilterMetadataResponse>.Ok(result, "Metadata filter display antrian berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<QueueDisplayDeviceSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Queue Display Device", Description = "Melihat ringkasan display antrian", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("QueueDisplayDevice", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var q = BuildBaseQuery();
            var result = new QueueDisplayDeviceSummaryResponse { TotalDevice = await q.CountAsync(), ActiveDevice = await q.CountAsync(x => x.IsActive), InactiveDevice = await q.CountAsync(x => !x.IsActive), VoiceCallingEnabledDevice = await q.CountAsync(x => x.EnableVoiceCalling), ShowPatientNameDevice = await q.CountAsync(x => x.ShowPatientName), WithPairingCodeDevice = await q.CountAsync(x => x.PairingCode != null && x.PairingCode != string.Empty), WithDeviceTokenDevice = await q.CountAsync(x => x.DeviceToken != null && x.DeviceToken != string.Empty) };
            return Ok(ApiResponse<QueueDisplayDeviceSummaryResponse>.Ok(result, "Ringkasan display antrian berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseQueueDisplayDevicePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Queue Display Device", Description = "Melihat data display antrian", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("QueueDisplayDevice", "Read")]
        public async Task<IActionResult> GetQueueDisplayDevices([FromQuery] Guid? nurseStationClusterId, [FromQuery] Guid? serviceUnitId, [FromQuery] QueueDisplayDeviceType? displayDeviceType, [FromQuery] QueueDisplayLayoutType? layoutType, [FromQuery] bool? isActive, [FromQuery] string? search, [FromQuery] string? sortBy = "sortOrder", [FromQuery] string? sortDirection = "asc", [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize); pageNumber = paging.PageNumber; pageSize = paging.PageSize;
            var query = ApplyFilter(BuildBaseQuery(), nurseStationClusterId, serviceUnitId, displayDeviceType, layoutType, isActive, search);
            var totalData = await query.CountAsync();
            var entities = await ApplySorting(query, sortBy, sortDirection).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            var actorNames = await GetActorNameMapAsync(entities.Select(x => x.CreateBy).Where(x => x != Guid.Empty));
            var result = new ResponseQueueDisplayDevicePagedResult { PageNumber = pageNumber, PageSize = pageSize, TotalData = totalData, TotalPage = (int)Math.Ceiling(totalData / (double)pageSize), Items = entities.Select(x => MapResponse(x, actorNames)).ToList() };
            return Ok(ApiResponse<ResponseQueueDisplayDevicePagedResult>.Ok(result, "Data display antrian berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<QueueDisplayDeviceOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Queue Display Device", Description = "Melihat data pilihan display antrian", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("QueueDisplayDevice", "Read")]
        public async Task<IActionResult> GetQueueDisplayDeviceOptions([FromQuery] Guid? nurseStationClusterId, [FromQuery] bool onlyActive = true, [FromQuery] string? search = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize); pageNumber = paging.PageNumber; pageSize = paging.PageSize;
            var query = ApplyFilter(BuildBaseQuery(), nurseStationClusterId, null, null, null, onlyActive ? true : null, search);
            var totalData = await query.CountAsync();
            var items = await query.OrderBy(x => x.SortOrder).ThenBy(x => x.DisplayName).Skip((pageNumber - 1) * pageSize).Take(pageSize).Select(x => MapOptionResponse(x)).ToListAsync();
            return Ok(ApiResponse<QueueDisplayDeviceOptionPagedResponse>.Ok(new QueueDisplayDeviceOptionPagedResponse { PageNumber = pageNumber, PageSize = pageSize, TotalData = totalData, TotalPage = (int)Math.Ceiling(totalData / (double)pageSize), Items = items }, "Data pilihan display antrian berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<QueueDisplayDeviceDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Queue Display Device", Description = "Melihat detail display antrian", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("QueueDisplayDevice", "Read")]
        public async Task<IActionResult> GetQueueDisplayDeviceById(Guid id)
        {
            var entity = await BuildBaseQuery().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Display antrian tidak ditemukan."));
            var actorNames = await GetActorNameMapAsync(new[] { entity.CreateBy, entity.UpdateBy });
            return Ok(ApiResponse<QueueDisplayDeviceDetailResponse>.Ok(MapDetailResponse(entity, actorNames), "Detail display antrian berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<QueueDisplayDeviceCreateResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create Queue Display Device", Description = "Membuat display antrian", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("QueueDisplayDevice", "Create")]
        public async Task<IActionResult> CreateQueueDisplayDevice([FromBody] CreateQueueDisplayDeviceRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);
            if (!validation.IsValid) return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data display antrian tidak valid."));
            var now = DateTime.UtcNow; var actorUserId = GetCurrentUserId();
            var entity = new MstQueueDisplayDevice { Id = Guid.NewGuid(), NurseStationClusterId = request.NurseStationClusterId, ServiceUnitId = request.ServiceUnitId, DisplayCode = await GenerateCodeAsync(), DisplayName = request.DisplayName.Trim(), DisplayDeviceType = request.DisplayDeviceType, LayoutType = request.LayoutType, LocationName = NormalizeNullableString(request.LocationName), FloorName = NormalizeNullableString(request.FloorName), RoomName = NormalizeNullableString(request.RoomName), IpAddress = NormalizeNullableString(request.IpAddress), MacAddress = NormalizeNullableString(request.MacAddress), PairingCode = NormalizeUpperNullableString(request.PairingCode), DeviceToken = NormalizeNullableString(request.DeviceToken), EnableVoiceCalling = request.EnableVoiceCalling, ShowPatientName = request.ShowPatientName, ShowDoctorName = request.ShowDoctorName, ShowClinicName = request.ShowClinicName, RefreshIntervalSeconds = NormalizeRefreshInterval(request.RefreshIntervalSeconds), SessionExpireMinutes = NormalizeSessionExpireMinutes(request.SessionExpireMinutes), SortOrder = request.SortOrder, Description = NormalizeNullableString(request.Description), IsActive = true, CreateDateTime = now, CreateBy = actorUserId, IsDelete = false, IsCancel = false };
            _dbContext.Set<MstQueueDisplayDevice>().Add(entity); await _dbContext.SaveChangesAsync();
            var result = new QueueDisplayDeviceCreateResponse { Id = entity.Id, DisplayCode = entity.DisplayCode, DisplayName = entity.DisplayName, SessionExpireMinutes = entity.SessionExpireMinutes, SessionExpireDescription = FormatSessionExpireDescription(entity.SessionExpireMinutes), IsActive = entity.IsActive };
            await _loggerService.InfoAsync(LogCategory, "QueueDisplayDevice.Create", "Membuat display antrian.", result);
            return Ok(ApiResponse<QueueDisplayDeviceCreateResponse>.Ok(result, "Display antrian berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Queue Display Device", Description = "Mengubah display antrian", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("QueueDisplayDevice", "Update")]
        public async Task<IActionResult> UpdateQueueDisplayDevice(Guid id, [FromBody] UpdateQueueDisplayDeviceRequest request)
        {
            var entity = await _dbContext.Set<MstQueueDisplayDevice>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Display antrian tidak ditemukan."));
            var validation = await ValidateRequestAsync(id, request);
            if (!validation.IsValid) return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data display antrian tidak valid."));
            entity.NurseStationClusterId = request.NurseStationClusterId; entity.ServiceUnitId = request.ServiceUnitId; entity.DisplayName = request.DisplayName.Trim(); entity.DisplayDeviceType = request.DisplayDeviceType; entity.LayoutType = request.LayoutType; entity.LocationName = NormalizeNullableString(request.LocationName); entity.FloorName = NormalizeNullableString(request.FloorName); entity.RoomName = NormalizeNullableString(request.RoomName); entity.IpAddress = NormalizeNullableString(request.IpAddress); entity.MacAddress = NormalizeNullableString(request.MacAddress); entity.PairingCode = NormalizeUpperNullableString(request.PairingCode); entity.DeviceToken = NormalizeNullableString(request.DeviceToken); entity.EnableVoiceCalling = request.EnableVoiceCalling; entity.ShowPatientName = request.ShowPatientName; entity.ShowDoctorName = request.ShowDoctorName; entity.ShowClinicName = request.ShowClinicName; entity.RefreshIntervalSeconds = NormalizeRefreshInterval(request.RefreshIntervalSeconds); entity.SessionExpireMinutes = NormalizeSessionExpireMinutes(request.SessionExpireMinutes); entity.SortOrder = request.SortOrder; entity.Description = NormalizeNullableString(request.Description); entity.IsActive = request.IsActive; entity.UpdateDateTime = DateTime.UtcNow; entity.UpdateBy = GetCurrentUserId(); await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Display antrian berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Queue Display Device Status", Description = "Mengubah status display antrian", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("QueueDisplayDevice", "Update")]
        public async Task<IActionResult> UpdateQueueDisplayDeviceStatus(Guid id, [FromBody] UpdateQueueDisplayDeviceStatusRequest request)
        {
            var entity = await _dbContext.Set<MstQueueDisplayDevice>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Display antrian tidak ditemukan."));
            entity.IsActive = request.IsActive; entity.UpdateDateTime = DateTime.UtcNow; entity.UpdateBy = GetCurrentUserId(); await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Status display antrian berhasil diperbarui."));
        }



        [HttpGet("login-summary")]
        [ProducesResponseType(typeof(ApiResponse<QueueDisplayDeviceLoginSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Queue Display Device Login Summary", Description = "Melihat ringkasan login display antrian", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("QueueDisplayDevice", "Read")]
        public async Task<IActionResult> GetQueueDisplayDeviceLoginSummary()
        {
            var devices = await BuildBaseQuery().ToListAsync();
            var users = await GetQueueDisplayLoginUsersAsync(devices.Select(x => x.DisplayCode));

            var loginInfos = devices
                .Select(x => BuildLoginInfoResponse(x, users.GetValueOrDefault(x.DisplayCode)))
                .ToList();

            var result = new QueueDisplayDeviceLoginSummaryResponse
            {
                TotalDevice = loginInfos.Count,
                DeviceWithLogin = loginInfos.Count(x => x.IsLoginCreated),
                DeviceWithoutLogin = loginInfos.Count(x => !x.IsLoginCreated),
                EnabledLogin = loginInfos.Count(x => x.IsLoginCreated && x.IsLoginEnabled && !x.IsLoginLocked),
                DisabledOrLockedLogin = loginInfos.Count(x => x.IsLoginCreated && (!x.IsLoginEnabled || x.IsLoginLocked)),
                ActiveDeviceWithoutLogin = loginInfos.Count(x => x.IsActive && !x.IsLoginCreated)
            };

            return Ok(ApiResponse<QueueDisplayDeviceLoginSummaryResponse>.Ok(result, "Ringkasan login display antrian berhasil diambil."));
        }

        [HttpGet("login-status")]
        [ProducesResponseType(typeof(ApiResponse<QueueDisplayDeviceLoginSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Queue Display Device Login Status Summary", Description = "Melihat ringkasan status login display antrian", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("QueueDisplayDevice", "Read")]
        public async Task<IActionResult> GetQueueDisplayDeviceLoginStatusSummary()
        {
            return await GetQueueDisplayDeviceLoginSummary();
        }

        [HttpGet("login-info")]
        [ProducesResponseType(typeof(ApiResponse<QueueDisplayDeviceLoginInfoPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Queue Display Device Login Info", Description = "Melihat informasi login display antrian", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("QueueDisplayDevice", "Read")]
        public async Task<IActionResult> GetQueueDisplayDeviceLoginInfo(
            [FromQuery] bool? hasLogin,
            [FromQuery] bool? isLoginEnabled,
            [FromQuery] bool? isLoginLocked,
            [FromQuery] bool? isActive,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.DisplayCode.ToLower().Contains(keyword) ||
                    x.DisplayName.ToLower().Contains(keyword) ||
                    x.NurseStationCluster!.ClusterName.ToLower().Contains(keyword) ||
                    (x.LocationName != null && x.LocationName.ToLower().Contains(keyword)) ||
                    (x.FloorName != null && x.FloorName.ToLower().Contains(keyword)) ||
                    (x.RoomName != null && x.RoomName.ToLower().Contains(keyword)) ||
                    (x.IpAddress != null && x.IpAddress.ToLower().Contains(keyword)));
            }

            var devices = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.DisplayName)
                .ToListAsync();

            var users = await GetQueueDisplayLoginUsersAsync(devices.Select(x => x.DisplayCode));

            var itemsQuery = devices
                .Select(x => BuildLoginInfoResponse(x, users.GetValueOrDefault(x.DisplayCode)))
                .AsEnumerable();

            if (hasLogin.HasValue)
            {
                itemsQuery = itemsQuery.Where(x => x.IsLoginCreated == hasLogin.Value);
            }

            if (isLoginEnabled.HasValue)
            {
                itemsQuery = itemsQuery.Where(x => x.IsLoginEnabled == isLoginEnabled.Value);
            }

            if (isLoginLocked.HasValue)
            {
                itemsQuery = itemsQuery.Where(x => x.IsLoginLocked == isLoginLocked.Value);
            }

            var filteredItems = itemsQuery.ToList();
            var totalData = filteredItems.Count;

            var items = filteredItems
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = new QueueDisplayDeviceLoginInfoPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<QueueDisplayDeviceLoginInfoPagedResponse>.Ok(result, "Informasi login display antrian berhasil diambil."));
        }

        [HttpGet("{id:guid}/login-info")]
        [ProducesResponseType(typeof(ApiResponse<QueueDisplayDeviceLoginInfoResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Queue Display Device Login Detail", Description = "Melihat detail login display antrian", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("QueueDisplayDevice", "Read")]
        public async Task<IActionResult> GetQueueDisplayDeviceLoginInfoById(Guid id)
        {
            var device = await BuildBaseQuery().FirstOrDefaultAsync(x => x.Id == id);

            if (device == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Display antrian tidak ditemukan."));
            }

            var user = await FindQueueDisplayLoginUserAsync(device.DisplayCode);
            var result = BuildLoginInfoResponse(device, user);

            return Ok(ApiResponse<QueueDisplayDeviceLoginInfoResponse>.Ok(result, "Detail login display antrian berhasil diambil."));
        }

        [HttpPost("{id:guid}/generate-login")]
        [ProducesResponseType(typeof(ApiResponse<QueueDisplayDeviceGenerateLoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Generate Queue Display Device Login", Description = "Membuat login untuk display antrian", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("QueueDisplayDevice", "Update")]
        public async Task<IActionResult> GenerateQueueDisplayDeviceLogin(Guid id, [FromBody] GenerateQueueDisplayDeviceLoginRequest? request = null)
        {
            var device = await _dbContext.Set<MstQueueDisplayDevice>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (device == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Display antrian tidak ditemukan."));
            }

            var existingUser = await FindQueueDisplayLoginUserAsync(device.DisplayCode);

            if (existingUser != null)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Login display antrian sudah pernah dibuat. Gunakan reset-login jika ingin mengganti password."));
            }

            var userName = NormalizeLoginUserName(request?.UserName, device.DisplayCode);
            var email = NormalizeLoginEmail(request?.Email, device.DisplayCode);
            var password = string.IsNullOrWhiteSpace(request?.Password) ? GenerateQueueDisplayPassword() : request!.Password.Trim();

            var duplicateUserName = await _userManager.FindByNameAsync(userName);

            if (duplicateUserName != null)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Username login display antrian sudah digunakan."));
            }

            var duplicateEmail = await _userManager.FindByEmailAsync(email);

            if (duplicateEmail != null)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Email login display antrian sudah digunakan."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

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
                UserCode = device.DisplayCode,
                DisplayName = device.DisplayName
            };

            var createResult = await _userManager.CreateAsync(user, password);

            if (!createResult.Succeeded)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, BuildIdentityErrorMessage(createResult, "Login display antrian gagal dibuat.")));
            }

            if (request != null && !request.IsEnabled)
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
            }

            device.UpdateDateTime = now;
            device.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
            var isLocked = await _userManager.IsLockedOutAsync(user);

            var result = new QueueDisplayDeviceGenerateLoginResponse
            {
                QueueDisplayDeviceId = device.Id,
                DisplayCode = device.DisplayCode,
                DisplayName = device.DisplayName,
                DeviceId = device.Id,
                DeviceCode = device.DisplayCode,
                DeviceName = device.DisplayName,
                LoginUserId = user.Id,
                LoginUserCode = user.UserCode,
                LoginUserName = user.UserName ?? string.Empty,
                LoginEmail = user.Email,
                GeneratedPassword = password,
                IsLoginEnabled = !isLocked,
                IsLoginLocked = isLocked,
                CreateDateTime = now,
                CreateBy = actorUserId == Guid.Empty ? null : actorUserId,
                CreateByName = GetActorName(actorNames, actorUserId)
            };

            await _loggerService.InfoAsync(LogCategory, "QueueDisplayDevice.GenerateQueueDisplayDeviceLogin", "Membuat login display antrian.", new { result.QueueDisplayDeviceId, result.DisplayCode, result.DisplayName, result.LoginUserId, result.LoginUserName, result.IsLoginEnabled });

            return Ok(ApiResponse<QueueDisplayDeviceGenerateLoginResponse>.Ok(result, "Login display antrian berhasil dibuat. Simpan password karena hanya ditampilkan sekali."));
        }

        [HttpPatch("{id:guid}/reset-login")]
        [ProducesResponseType(typeof(ApiResponse<QueueDisplayDeviceResetLoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Reset Queue Display Device Login", Description = "Reset password login display antrian", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("QueueDisplayDevice", "Update")]
        public async Task<IActionResult> ResetQueueDisplayDeviceLogin(Guid id, [FromBody] ResetQueueDisplayDeviceLoginRequest? request = null)
        {
            var device = await _dbContext.Set<MstQueueDisplayDevice>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (device == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Display antrian tidak ditemukan."));
            }

            var user = await FindQueueDisplayLoginUserAsync(device.DisplayCode);

            if (user == null)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Login display antrian belum dibuat. Gunakan generate-login terlebih dahulu."));
            }

            var newPassword = string.IsNullOrWhiteSpace(request?.NewPassword) ? GenerateQueueDisplayPassword() : request!.NewPassword.Trim();
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

            if (!resetResult.Succeeded)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, BuildIdentityErrorMessage(resetResult, "Password login display antrian gagal direset.")));
            }

            await _userManager.ResetAccessFailedCountAsync(user);
            await _userManager.SetLockoutEndDateAsync(user, null);

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            device.UpdateDateTime = now;
            device.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { actorUserId });

            var result = new QueueDisplayDeviceResetLoginResponse
            {
                QueueDisplayDeviceId = device.Id,
                DisplayCode = device.DisplayCode,
                DisplayName = device.DisplayName,
                DeviceId = device.Id,
                DeviceCode = device.DisplayCode,
                DeviceName = device.DisplayName,
                LoginUserId = user.Id,
                LoginUserName = user.UserName ?? string.Empty,
                NewPassword = newPassword,
                UpdateDateTime = now,
                UpdateBy = actorUserId == Guid.Empty ? null : actorUserId,
                UpdateByName = GetActorName(actorNames, actorUserId)
            };

            await _loggerService.InfoAsync(LogCategory, "QueueDisplayDevice.ResetQueueDisplayDeviceLogin", "Reset login display antrian.", new { result.QueueDisplayDeviceId, result.DisplayCode, result.DisplayName, result.LoginUserId, result.LoginUserName });

            return Ok(ApiResponse<QueueDisplayDeviceResetLoginResponse>.Ok(result, "Password login display antrian berhasil direset. Simpan password karena hanya ditampilkan sekali."));
        }

        [HttpPatch("{id:guid}/login-status")]
        [ProducesResponseType(typeof(ApiResponse<QueueDisplayDeviceLoginStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Queue Display Device Login Status", Description = "Mengubah status login display antrian", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("QueueDisplayDevice", "Update")]
        public async Task<IActionResult> UpdateQueueDisplayDeviceLoginStatus(Guid id, [FromBody] UpdateQueueDisplayDeviceLoginStatusRequest request)
        {
            var device = await _dbContext.Set<MstQueueDisplayDevice>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (device == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Display antrian tidak ditemukan."));
            }

            var user = await FindQueueDisplayLoginUserAsync(device.DisplayCode);

            if (user == null)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Login display antrian belum dibuat. Gunakan generate-login terlebih dahulu."));
            }

            if (request.IsEnabled)
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
                await _userManager.ResetAccessFailedCountAsync(user);
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            device.UpdateDateTime = now;
            device.UpdateBy = actorUserId;

            if (!string.IsNullOrWhiteSpace(request.Reason))
            {
                device.Description = request.Reason.Trim();
            }

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
            var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
            var isLocked = await _userManager.IsLockedOutAsync(user);

            var result = new QueueDisplayDeviceLoginStatusResponse
            {
                QueueDisplayDeviceId = device.Id,
                DisplayCode = device.DisplayCode,
                DisplayName = device.DisplayName,
                DeviceId = device.Id,
                DeviceCode = device.DisplayCode,
                DeviceName = device.DisplayName,
                LoginUserId = user.Id,
                LoginUserName = user.UserName,
                IsLoginCreated = true,
                IsLoginEnabled = !isLocked,
                IsLoginLocked = isLocked,
                LockoutEnd = lockoutEnd,
                UpdateDateTime = now,
                UpdateBy = actorUserId == Guid.Empty ? null : actorUserId,
                UpdateByName = GetActorName(actorNames, actorUserId)
            };

            return Ok(ApiResponse<QueueDisplayDeviceLoginStatusResponse>.Ok(result, request.IsEnabled ? "Login display antrian berhasil diaktifkan." : "Login display antrian berhasil dinonaktifkan."));
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<QueueDisplayDeviceLoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> LoginQueueDisplayDevice([FromBody] QueueDisplayDeviceLoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Username dan password wajib diisi."));
            }

            var user = await _userManager.FindByNameAsync(request.UserName.Trim());

            if (user == null)
            {
                return Ok(ApiResponse<QueueDisplayDeviceLoginResponse>.Ok(new QueueDisplayDeviceLoginResponse { IsSuccess = false, Message = "Username atau password tidak valid." }, "Login display antrian gagal."));
            }

            var isLockedBeforeCheck = await _userManager.IsLockedOutAsync(user);

            if (isLockedBeforeCheck)
            {
                var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                return Ok(ApiResponse<QueueDisplayDeviceLoginResponse>.Ok(new QueueDisplayDeviceLoginResponse { IsSuccess = false, Message = "Login display antrian sedang nonaktif atau terkunci.", LoginUserId = user.Id, LoginUserName = user.UserName, IsLoginEnabled = false, IsLoginLocked = true, LockoutEnd = lockoutEnd }, "Login display antrian gagal."));
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);

            if (!isPasswordValid)
            {
                await _userManager.AccessFailedAsync(user);
                return Ok(ApiResponse<QueueDisplayDeviceLoginResponse>.Ok(new QueueDisplayDeviceLoginResponse { IsSuccess = false, Message = "Username atau password tidak valid.", LoginUserId = user.Id, LoginUserName = user.UserName, IsLoginEnabled = true, IsLoginLocked = await _userManager.IsLockedOutAsync(user), LockoutEnd = await _userManager.GetLockoutEndDateAsync(user) }, "Login display antrian gagal."));
            }

            await _userManager.ResetAccessFailedCountAsync(user);
            var loginUserCode = ResolveDisplayCodeFromLoginUser(user);

            var deviceQuery = BuildBaseQuery().Where(x => x.DisplayCode == loginUserCode);

            var requestedDisplayCode = ResolveRequestedDisplayCode(request);

            if (!string.IsNullOrWhiteSpace(requestedDisplayCode))
            {
                deviceQuery = deviceQuery.Where(x => x.DisplayCode == requestedDisplayCode);
            }

            var device = await deviceQuery.FirstOrDefaultAsync();

            if (device == null)
            {
                return Ok(ApiResponse<QueueDisplayDeviceLoginResponse>.Ok(new QueueDisplayDeviceLoginResponse { IsSuccess = false, Message = "User login tidak terhubung dengan display antrian aktif.", LoginUserId = user.Id, LoginUserName = user.UserName, IsLoginEnabled = true, IsLoginLocked = false }, "Login display antrian gagal."));
            }

            if (!device.IsActive)
            {
                return Ok(ApiResponse<QueueDisplayDeviceLoginResponse>.Ok(new QueueDisplayDeviceLoginResponse { IsSuccess = false, Message = "Display antrian tidak aktif.", QueueDisplayDeviceId = device.Id, DisplayCode = device.DisplayCode, DisplayName = device.DisplayName, DeviceId = device.Id, DeviceCode = device.DisplayCode, DeviceName = device.DisplayName, NurseStationClusterId = device.NurseStationClusterId, ClusterName = device.NurseStationCluster?.ClusterName, ServiceUnitId = device.ServiceUnitId, ServiceUnitName = device.ServiceUnit?.ServiceUnitName, DisplayDeviceType = device.DisplayDeviceType, DisplayDeviceTypeName = BuildDisplayDeviceTypeLabel(device.DisplayDeviceType), LayoutType = device.LayoutType, LayoutTypeName = BuildLayoutTypeLabel(device.LayoutType), LoginUserId = user.Id, LoginUserName = user.UserName, IsLoginEnabled = true, IsLoginLocked = false }, "Login display antrian gagal."));
            }

            var now = DateTime.UtcNow;
            var trackedDevice = await _dbContext.Set<MstQueueDisplayDevice>().FirstOrDefaultAsync(x => x.Id == device.Id && !x.IsDelete);
            if (trackedDevice != null)
            {
                trackedDevice.LastOnlineDateTime = now;
                trackedDevice.UpdateDateTime = now;
                await _dbContext.SaveChangesAsync();
            }

            var response = new QueueDisplayDeviceLoginResponse
            {
                IsSuccess = true,
                Message = "Login display antrian berhasil.",
                QueueDisplayDeviceId = device.Id,
                DisplayCode = device.DisplayCode,
                DisplayName = device.DisplayName,
                DeviceId = device.Id,
                DeviceCode = device.DisplayCode,
                DeviceName = device.DisplayName,
                NurseStationClusterId = device.NurseStationClusterId,
                ClusterName = device.NurseStationCluster?.ClusterName,
                ServiceUnitId = device.ServiceUnitId,
                ServiceUnitName = device.ServiceUnit?.ServiceUnitName,
                DisplayDeviceType = device.DisplayDeviceType,
                DisplayDeviceTypeName = BuildDisplayDeviceTypeLabel(device.DisplayDeviceType),
                LayoutType = device.LayoutType,
                LayoutTypeName = BuildLayoutTypeLabel(device.LayoutType),
                EnableVoiceCalling = device.EnableVoiceCalling,
                ShowPatientName = device.ShowPatientName,
                ShowDoctorName = device.ShowDoctorName,
                ShowClinicName = device.ShowClinicName,
                RefreshIntervalSeconds = device.RefreshIntervalSeconds,
                LoginUserId = user.Id,
                LoginUserName = user.UserName,
                IsLoginEnabled = true,
                IsLoginLocked = false,
                LockoutEnd = await _userManager.GetLockoutEndDateAsync(user),
                RedirectPath = BuildQueueDisplayRedirectPath(device)
            };

            return Ok(ApiResponse<QueueDisplayDeviceLoginResponse>.Ok(response, "Login display antrian berhasil."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Delete", "Delete Queue Display Device", Description = "Menghapus display antrian", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("QueueDisplayDevice", "Delete")]
        public async Task<IActionResult> DeleteQueueDisplayDevice(Guid id, [FromBody] DeleteQueueDisplayDeviceRequest? request = null)
        {
            var entity = await _dbContext.Set<MstQueueDisplayDevice>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Display antrian tidak ditemukan."));
            var now = DateTime.UtcNow; var actorUserId = GetCurrentUserId(); entity.IsDelete = true; entity.IsActive = false; entity.DeleteDateTime = now; entity.DeleteBy = actorUserId; entity.UpdateDateTime = now; entity.UpdateBy = actorUserId; if (!string.IsNullOrWhiteSpace(request?.DeleteReason)) entity.Description = request.DeleteReason.Trim(); await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Display antrian berhasil dihapus."));
        }

        private IQueryable<MstQueueDisplayDevice> BuildBaseQuery() => _dbContext.Set<MstQueueDisplayDevice>().AsNoTracking().Include(x => x.NurseStationCluster).Include(x => x.ServiceUnit).Where(x => !x.IsDelete);
        private static IQueryable<MstQueueDisplayDevice> ApplyFilter(IQueryable<MstQueueDisplayDevice> q, Guid? clusterId, Guid? serviceUnitId, QueueDisplayDeviceType? deviceType, QueueDisplayLayoutType? layoutType, bool? isActive, string? search) { if (clusterId.HasValue && clusterId.Value != Guid.Empty) q = q.Where(x => x.NurseStationClusterId == clusterId.Value); if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty) q = q.Where(x => x.ServiceUnitId == serviceUnitId.Value); if (deviceType.HasValue) q = q.Where(x => x.DisplayDeviceType == deviceType.Value); if (layoutType.HasValue) q = q.Where(x => x.LayoutType == layoutType.Value); if (isActive.HasValue) q = q.Where(x => x.IsActive == isActive.Value); if (!string.IsNullOrWhiteSpace(search)) { var k = search.Trim().ToLower(); q = q.Where(x => x.DisplayCode.ToLower().Contains(k) || x.DisplayName.ToLower().Contains(k) || x.NurseStationCluster!.ClusterName.ToLower().Contains(k) || (x.LocationName != null && x.LocationName.ToLower().Contains(k)) || (x.FloorName != null && x.FloorName.ToLower().Contains(k)) || (x.RoomName != null && x.RoomName.ToLower().Contains(k)) || (x.IpAddress != null && x.IpAddress.ToLower().Contains(k)) || (x.MacAddress != null && x.MacAddress.ToLower().Contains(k)) || (x.PairingCode != null && x.PairingCode.ToLower().Contains(k)) || (x.Description != null && x.Description.ToLower().Contains(k))); } return q; }
        private static IOrderedQueryable<MstQueueDisplayDevice> ApplySorting(IQueryable<MstQueueDisplayDevice> q, string? sortBy, string? dir) { var d = string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase); return (sortBy ?? "sortOrder").Trim().ToLowerInvariant() switch { "createdatetime" => d ? q.OrderByDescending(x => x.CreateDateTime) : q.OrderBy(x => x.CreateDateTime), "displaycode" => d ? q.OrderByDescending(x => x.DisplayCode) : q.OrderBy(x => x.DisplayCode), "displayname" => d ? q.OrderByDescending(x => x.DisplayName) : q.OrderBy(x => x.DisplayName), "clustername" => d ? q.OrderByDescending(x => x.NurseStationCluster!.ClusterName) : q.OrderBy(x => x.NurseStationCluster!.ClusterName), "displaydevicetype" => d ? q.OrderByDescending(x => x.DisplayDeviceType) : q.OrderBy(x => x.DisplayDeviceType), "layouttype" => d ? q.OrderByDescending(x => x.LayoutType) : q.OrderBy(x => x.LayoutType), "sessionexpireminutes" => d ? q.OrderByDescending(x => x.SessionExpireMinutes) : q.OrderBy(x => x.SessionExpireMinutes), "isactive" => d ? q.OrderByDescending(x => x.IsActive) : q.OrderBy(x => x.IsActive), _ => d ? q.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.DisplayName) : q.OrderBy(x => x.SortOrder).ThenBy(x => x.DisplayName) }; }
        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid? excludeId, CreateQueueDisplayDeviceRequest r) { if (r.NurseStationClusterId == Guid.Empty) return (false, "Cluster wajib diisi."); if (string.IsNullOrWhiteSpace(r.DisplayName)) return (false, "Nama display wajib diisi."); if (!Enum.IsDefined(typeof(QueueDisplayDeviceType), r.DisplayDeviceType)) return (false, "Tipe device tidak valid. Gunakan nilai dari endpoint filters/metadata."); if (!Enum.IsDefined(typeof(QueueDisplayLayoutType), r.LayoutType)) return (false, "Tipe layout tidak valid. Gunakan nilai dari endpoint filters/metadata."); if (!await _dbContext.Set<MstNurseStationCluster>().AnyAsync(x => x.Id == r.NurseStationClusterId && !x.IsDelete)) return (false, "Cluster tidak ditemukan."); if (r.ServiceUnitId.HasValue && r.ServiceUnitId.Value != Guid.Empty && !await _dbContext.Set<QuilvianSystemBackend.Areas.HealthServices.MasterData.Models.MstServiceUnit>().AnyAsync(x => x.Id == r.ServiceUnitId.Value && !x.IsDelete)) return (false, "Unit layanan tidak ditemukan."); var n = r.DisplayName.Trim().ToLower(); var dup = _dbContext.Set<MstQueueDisplayDevice>().Where(x => !x.IsDelete && x.DisplayName.ToLower() == n); if (excludeId.HasValue) dup = dup.Where(x => x.Id != excludeId.Value); if (await dup.AnyAsync()) return (false, "Nama display sudah digunakan."); var pairing = NormalizeUpperNullableString(r.PairingCode); if (!string.IsNullOrWhiteSpace(pairing)) { var p = _dbContext.Set<MstQueueDisplayDevice>().Where(x => !x.IsDelete && x.PairingCode != null && x.PairingCode.ToLower() == pairing.ToLower()); if (excludeId.HasValue) p = p.Where(x => x.Id != excludeId.Value); if (await p.AnyAsync()) return (false, "Pairing code sudah digunakan."); } if (r.SessionExpireMinutes.HasValue && r.SessionExpireMinutes.Value <= 0) return (false, "Masa aktif session display harus lebih dari 0 menit atau dikosongkan untuk memakai fallback default."); return (true, null); }
        private async Task<string> GenerateCodeAsync() { var codes = await _dbContext.Set<MstQueueDisplayDevice>().IgnoreQueryFilters().AsNoTracking().Where(x => x.DisplayCode.StartsWith(CodePrefix)).Select(x => x.DisplayCode).ToListAsync(); var nums = codes.Select(x => x.Replace(CodePrefix, string.Empty)).Where(x => int.TryParse(x, out _)).Select(int.Parse).Where(x => x > 0).ToHashSet(); var next = 1; while (nums.Contains(next)) next++; return CodePrefix + next.ToString().PadLeft(CodeNumberLength, '0'); }
        private static QueueDisplayDeviceResponse MapResponse(MstQueueDisplayDevice e, IReadOnlyDictionary<Guid, string?> names) => new() { Id = e.Id, NurseStationClusterId = e.NurseStationClusterId, ClusterCode = e.NurseStationCluster?.ClusterCode, ClusterName = e.NurseStationCluster?.ClusterName, ServiceUnitId = e.ServiceUnitId, ServiceUnitCode = e.ServiceUnit?.ServiceUnitCode, ServiceUnitName = e.ServiceUnit?.ServiceUnitName, DisplayCode = e.DisplayCode, DisplayName = e.DisplayName, DisplayDeviceType = e.DisplayDeviceType, DisplayDeviceTypeName = BuildDisplayDeviceTypeLabel(e.DisplayDeviceType), LayoutType = e.LayoutType, LayoutTypeName = BuildLayoutTypeLabel(e.LayoutType), LocationName = e.LocationName, FloorName = e.FloorName, RoomName = e.RoomName, IpAddress = e.IpAddress, MacAddress = e.MacAddress, PairingCode = e.PairingCode, EnableVoiceCalling = e.EnableVoiceCalling, ShowPatientName = e.ShowPatientName, ShowDoctorName = e.ShowDoctorName, ShowClinicName = e.ShowClinicName, RefreshIntervalSeconds = e.RefreshIntervalSeconds, SessionExpireMinutes = e.SessionExpireMinutes, SessionExpireDescription = FormatSessionExpireDescription(e.SessionExpireMinutes), LastOnlineDateTime = e.LastOnlineDateTime, SortOrder = e.SortOrder, IsActive = e.IsActive, CreateDateTime = e.CreateDateTime, CreateBy = e.CreateBy == Guid.Empty ? null : e.CreateBy, CreateByName = GetActorName(names, e.CreateBy) };
        private static QueueDisplayDeviceDetailResponse MapDetailResponse(MstQueueDisplayDevice e, IReadOnlyDictionary<Guid, string?> names) { var d = new QueueDisplayDeviceDetailResponse(); var b = MapResponse(e, names); foreach (var p in typeof(QueueDisplayDeviceResponse).GetProperties()) p.SetValue(d, p.GetValue(b)); d.DeviceToken = e.DeviceToken; d.Description = e.Description; d.UpdateDateTime = e.UpdateDateTime; d.UpdateBy = e.UpdateBy == Guid.Empty ? null : e.UpdateBy; d.UpdateByName = GetActorName(names, e.UpdateBy); return d; }
        private static QueueDisplayDeviceOptionResponse MapOptionResponse(MstQueueDisplayDevice e) => new() { Id = e.Id, NurseStationClusterId = e.NurseStationClusterId, ClusterName = e.NurseStationCluster?.ClusterName, DisplayCode = e.DisplayCode, DisplayName = e.DisplayName, DisplayDeviceType = e.DisplayDeviceType, DisplayDeviceTypeName = BuildDisplayDeviceTypeLabel(e.DisplayDeviceType), LayoutType = e.LayoutType, LayoutTypeName = BuildLayoutTypeLabel(e.LayoutType), SessionExpireMinutes = e.SessionExpireMinutes, SessionExpireDescription = FormatSessionExpireDescription(e.SessionExpireMinutes), SortOrder = e.SortOrder };
        private static List<QueueDisplayDeviceEnumMetadataResponse> BuildEnumMetadata() => new() { new QueueDisplayDeviceEnumMetadataResponse { EnumName = nameof(QueueDisplayDeviceType), FieldName = "displayDeviceType", Options = Enum.GetValues<QueueDisplayDeviceType>().Select(x => new QueueDisplayDeviceEnumOptionResponse { Value = Convert.ToInt32(x), Name = x.ToString(), Label = BuildDisplayDeviceTypeLabel(x) }).ToList() }, new QueueDisplayDeviceEnumMetadataResponse { EnumName = nameof(QueueDisplayLayoutType), FieldName = "layoutType", Options = Enum.GetValues<QueueDisplayLayoutType>().Select(x => new QueueDisplayDeviceEnumOptionResponse { Value = Convert.ToInt32(x), Name = x.ToString(), Label = BuildLayoutTypeLabel(x) }).ToList() } };
        private static string BuildDisplayDeviceTypeLabel(QueueDisplayDeviceType v) => v switch { QueueDisplayDeviceType.TvDisplay => "TV Display", QueueDisplayDeviceType.PlasmaDisplay => "Plasma Display", QueueDisplayDeviceType.TabletDisplay => "Tablet Display", QueueDisplayDeviceType.WebDisplay => "Web Display", QueueDisplayDeviceType.VoiceDisplay => "Voice Display", QueueDisplayDeviceType.Other => "Other", _ => v.ToString() };
        private static string BuildLayoutTypeLabel(QueueDisplayLayoutType v) => v switch { QueueDisplayLayoutType.ClusterBoard => "Cluster Board", QueueDisplayLayoutType.ClinicBoard => "Clinic Board", QueueDisplayLayoutType.DoctorBoard => "Doctor Board", QueueDisplayLayoutType.CallingBoard => "Calling Board", QueueDisplayLayoutType.SummaryBoard => "Summary Board", _ => v.ToString() };


        private async Task<ApplicationUser?> FindQueueDisplayLoginUserAsync(string displayCode)
        {
            if (string.IsNullOrWhiteSpace(displayCode))
            {
                return null;
            }

            var normalizedDisplayCode = displayCode.Trim().ToUpperInvariant();
            var defaultUserName = NormalizeLoginUserName(null, normalizedDisplayCode);

            var userByCode = await _userManager.Users
                .FirstOrDefaultAsync(x => x.UserCode != null && x.UserCode.ToUpper() == normalizedDisplayCode);

            if (userByCode != null)
            {
                return userByCode;
            }

            var userByDefaultUserName = await _userManager.FindByNameAsync(defaultUserName);

            if (userByDefaultUserName != null)
            {
                return userByDefaultUserName;
            }

            return await _userManager.FindByNameAsync(normalizedDisplayCode);
        }

        private async Task<Dictionary<string, ApplicationUser?>> GetQueueDisplayLoginUsersAsync(IEnumerable<string> displayCodes)
        {
            var codes = displayCodes
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToUpperInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var result = new Dictionary<string, ApplicationUser?>(StringComparer.OrdinalIgnoreCase);

            if (!codes.Any())
            {
                return result;
            }

            var usersByCode = await _userManager.Users
                .Where(x => x.UserCode != null && codes.Contains(x.UserCode))
                .ToListAsync();

            foreach (var user in usersByCode)
            {
                if (string.IsNullOrWhiteSpace(user.UserCode))
                {
                    continue;
                }

                var normalizedUserCode = user.UserCode.Trim().ToUpperInvariant();

                if (!result.ContainsKey(normalizedUserCode))
                {
                    result[normalizedUserCode] = user;
                }
            }

            foreach (var code in codes)
            {
                if (result.ContainsKey(code))
                {
                    continue;
                }

                var defaultUserName = NormalizeLoginUserName(null, code);
                var userByUserName = await _userManager.FindByNameAsync(defaultUserName)
                    ?? await _userManager.FindByNameAsync(code);

                if (userByUserName != null)
                {
                    result[code] = userByUserName;
                }
            }

            return result;
        }

        private static QueueDisplayDeviceLoginInfoResponse BuildLoginInfoResponse(MstQueueDisplayDevice device, ApplicationUser? user)
        {
            var isLoginCreated = user != null;
            var isLoginLocked = user != null &&
                user.LockoutEnabled &&
                user.LockoutEnd.HasValue &&
                user.LockoutEnd.Value > DateTimeOffset.UtcNow;

            return new QueueDisplayDeviceLoginInfoResponse
            {
                QueueDisplayDeviceId = device.Id,
                DisplayCode = device.DisplayCode,
                DisplayName = device.DisplayName,
                DeviceId = device.Id,
                DeviceCode = device.DisplayCode,
                DeviceName = device.DisplayName,
                NurseStationClusterId = device.NurseStationClusterId,
                ClusterName = device.NurseStationCluster?.ClusterName,
                ServiceUnitId = device.ServiceUnitId,
                ServiceUnitName = device.ServiceUnit?.ServiceUnitName,
                DisplayDeviceType = device.DisplayDeviceType,
                DisplayDeviceTypeName = BuildDisplayDeviceTypeLabel(device.DisplayDeviceType),
                LayoutType = device.LayoutType,
                LayoutTypeName = BuildLayoutTypeLabel(device.LayoutType),
                LocationName = device.LocationName,
                FloorName = device.FloorName,
                RoomName = device.RoomName,
                IpAddress = device.IpAddress,
                IsActive = device.IsActive,
                LoginUserId = user?.Id,
                LoginUserCode = user?.UserCode,
                LoginUserName = user?.UserName,
                LoginEmail = user?.Email,
                LoginDisplayName = user?.DisplayName,
                IsLoginCreated = isLoginCreated,
                IsLoginEnabled = isLoginCreated && !isLoginLocked,
                IsLoginLocked = isLoginLocked,
                LockoutEnd = user?.LockoutEnd,
                SessionExpireMinutes = device.SessionExpireMinutes,
                SessionExpireDescription = FormatSessionExpireDescription(device.SessionExpireMinutes),
                AccessFailedCount = user?.AccessFailedCount ?? 0,
                CanLogin = device.IsActive && isLoginCreated && !isLoginLocked
            };
        }

        private static string ResolveDisplayCodeFromLoginUser(ApplicationUser user)
        {
            if (!string.IsNullOrWhiteSpace(user.UserCode))
            {
                return user.UserCode.Trim().ToUpperInvariant();
            }

            var userName = user.UserName?.Trim();

            if (string.IsNullOrWhiteSpace(userName))
            {
                return string.Empty;
            }

            const string defaultPrefix = "queue-display.";

            if (userName.StartsWith(defaultPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return userName[defaultPrefix.Length..].Trim().ToUpperInvariant();
            }

            return userName.ToUpperInvariant();
        }

        private static string? ResolveRequestedDisplayCode(QueueDisplayDeviceLoginRequest request)
        {
            var displayCode = !string.IsNullOrWhiteSpace(request.DisplayCode)
                ? request.DisplayCode
                : request.DeviceCode;

            return string.IsNullOrWhiteSpace(displayCode)
                ? null
                : displayCode.Trim().ToUpperInvariant();
        }

        private static string BuildQueueDisplayRedirectPath(MstQueueDisplayDevice device)
        {
            var query = new List<string>
            {
                $"queueDisplayDeviceId={Uri.EscapeDataString(device.Id.ToString())}",
                $"displayCode={Uri.EscapeDataString(device.DisplayCode)}",
                $"nurseStationClusterId={Uri.EscapeDataString(device.NurseStationClusterId.ToString())}"
            };

            if (device.ServiceUnitId.HasValue && device.ServiceUnitId.Value != Guid.Empty)
            {
                query.Add($"serviceUnitId={Uri.EscapeDataString(device.ServiceUnitId.Value.ToString())}");
            }

            return "/queue-display?" + string.Join("&", query);
        }

        private static string NormalizeLoginUserName(string? userName, string displayCode)
        {
            if (!string.IsNullOrWhiteSpace(userName))
            {
                return userName.Trim().ToLowerInvariant();
            }

            var normalizedDisplayCode = string.IsNullOrWhiteSpace(displayCode)
                ? Guid.NewGuid().ToString("N")
                : displayCode.Trim().ToUpperInvariant();

            return $"queue-display.{normalizedDisplayCode}".ToLowerInvariant();
        }

        private static string NormalizeLoginEmail(string? email, string displayCode)
        {
            if (!string.IsNullOrWhiteSpace(email))
            {
                return email.Trim().ToLowerInvariant();
            }

            var userName = NormalizeLoginUserName(null, displayCode)
                .Replace("@", ".")
                .Replace(" ", string.Empty);

            return $"{userName}@queue-display.local";
        }

        private static string GenerateQueueDisplayPassword()
        {
            var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(8);
            return $"Queue@{Convert.ToHexString(bytes)}aA1!";
        }

        private static string BuildIdentityErrorMessage(IdentityResult identityResult, string defaultMessage)
        {
            if (identityResult.Succeeded)
            {
                return defaultMessage;
            }

            var errors = identityResult.Errors
                .Select(x => x.Description)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            return errors.Any()
                ? defaultMessage + " " + string.Join(" ", errors)
                : defaultMessage;
        }

        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(IEnumerable<Guid> ids) { var list = ids.Where(x => x != Guid.Empty).Distinct().ToList(); if (!list.Any()) return new(); return await _dbContext.Users.AsNoTracking().Where(x => list.Contains(x.Id)).Select(x => new { x.Id, Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode }).ToDictionaryAsync(x => x.Id, x => x.Name); }
        private static string? GetActorName(IReadOnlyDictionary<Guid, string?> names, Guid id) => id == Guid.Empty ? null : names.TryGetValue(id, out var n) ? n : null;
        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize) { if (pageNumber < 1) pageNumber = 1; if (pageSize < 1) pageSize = 25; if (pageSize > 100) pageSize = 100; return (pageNumber, pageSize); }
        private static int? NormalizeSessionExpireMinutes(int? value) => value.HasValue && value.Value > 0 ? value.Value : null;

        private static string FormatSessionExpireDescription(int? minutes)
        {
            if (!minutes.HasValue || minutes.Value <= 0) return "Menggunakan fallback default device dari konfigurasi backend";
            var totalMinutes = minutes.Value;
            if (totalMinutes % 1440 == 0) return $"{totalMinutes} menit ({totalMinutes / 1440} hari)";
            if (totalMinutes % 60 == 0) return $"{totalMinutes} menit ({totalMinutes / 60} jam)";
            return $"{totalMinutes} menit";
        }

        private static string? NormalizeNullableString(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        private static string? NormalizeUpperNullableString(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();
        private static int NormalizeRefreshInterval(int value) => value < 3 ? 3 : value > 300 ? 300 : value;
        private Guid GetCurrentUserId() { var v = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id"); return Guid.TryParse(v, out var id) ? id : Guid.Empty; }
    }
}
