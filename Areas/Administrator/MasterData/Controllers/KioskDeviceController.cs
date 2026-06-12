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
using System.Data;
using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;

using ResponseKioskDevicePagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs.KioskDeviceResponse>;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/administrator/master-data/kiosk-devices")]
    [AccessController(
        moduleCode: "ADMINISTRATOR_MASTER_DATA",
        moduleName: "Administrator Master Data",
        displayName: "Kiosk Device",
        AreaName = "Administrator",
        ControllerName = "KioskDevice",
        Description = "Administrator master data kiosk device",
        SortOrder = 8
    )]
    [Tags("Administrator / Master Data / Kiosk Device")]
    public class KioskDeviceController : ControllerBase
    {
        private const string LogCategory = "Administrator.MasterData";
        private const string KioskDeviceCodePrefix = "KSK-RSMMC-";
        private const int KioskDeviceCodeDigitLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly UserManager<ApplicationUser> _userManager;

        public KioskDeviceController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _userManager = userManager;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<KioskDeviceFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Kiosk Device",
            Description = "Melihat metadata filter kiosk device",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("KioskDevice", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var deviceTypeOptions = BuildEnumOptions<KioskDeviceType>();
            var deviceStatusOptions = BuildEnumOptions<KioskDeviceStatus>();

            var result = new KioskDeviceFilterMetadataResponse
            {
                DefaultFilter = new KioskDeviceDefaultFilterResponse(),
                CustomPeriods = BuildCustomPeriodOptions(),
                SortOptions = new List<KioskDeviceSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "deviceCode", Label = "Kode device" },
                    new() { Value = "deviceName", Label = "Nama device" },
                    new() { Value = "deviceType", Label = "Tipe device" },
                    new() { Value = "deviceStatus", Label = "Status device" },
                    new() { Value = "defaultScannerProfileName", Label = "Default scanner profile" },
                    new() { Value = "locationName", Label = "Lokasi" },
                    new() { Value = "floorName", Label = "Lantai" },
                    new() { Value = "ipAddress", Label = "IP address" },
                    new() { Value = "macAddress", Label = "MAC address" },
                    new() { Value = "isAllowWalkIn", Label = "Izinkan walk-in" },
                    new() { Value = "isAllowAppointment", Label = "Izinkan appointment" },
                    new() { Value = "isAllowInsuranceRegistration", Label = "Izinkan registrasi asuransi" },
                    new() { Value = "lastOnlineAt", Label = "Terakhir online" },
                    new() { Value = "lastOfflineAt", Label = "Terakhir offline" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                EnumOptions = BuildEnumMetadataOptions(deviceTypeOptions, deviceStatusOptions),
                DeviceTypeOptions = deviceTypeOptions,
                DeviceStatusOptions = deviceStatusOptions,
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateFieldMetadata(),
                UpdateFields = BuildUpdateFieldMetadata(),
                ResetButtonLabel = "Reset"
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "KioskDevice.GetFilterMetadata",
                "Mengambil metadata filter kiosk device.",
                result
            );

            return Ok(ApiResponse<KioskDeviceFilterMetadataResponse>.Ok(
                result,
                "Metadata filter kiosk device berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<KioskDeviceSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Kiosk Device",
            Description = "Melihat ringkasan kiosk device",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("KioskDevice", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();

            var result = new KioskDeviceSummaryResponse
            {
                TotalDevice = await query.CountAsync(),
                ActiveDevice = await query.CountAsync(x => x.IsActive),
                InactiveDevice = await query.CountAsync(x => !x.IsActive),
                OnlineDevice = await query.CountAsync(x => x.DeviceStatus == KioskDeviceStatus.Active),
                OfflineDevice = await query.CountAsync(x => x.DeviceStatus == KioskDeviceStatus.Offline),
                MaintenanceDevice = await query.CountAsync(x => x.DeviceStatus == KioskDeviceStatus.Maintenance),
                WalkInAllowedDevice = await query.CountAsync(x => x.IsAllowWalkIn),
                AppointmentAllowedDevice = await query.CountAsync(x => x.IsAllowAppointment),
                InsuranceRegistrationAllowedDevice = await query.CountAsync(x => x.IsAllowInsuranceRegistration),
                WithScannerProfileDevice = await query.CountAsync(x => x.DefaultScannerProfileId.HasValue)
            };

            return Ok(ApiResponse<KioskDeviceSummaryResponse>.Ok(
                result,
                "Ringkasan kiosk device berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseKioskDevicePagedResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Read",
            "Read Kiosk Device",
            Description = "Melihat data kiosk device",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("KioskDevice", "Read")]
        public async Task<IActionResult> GetKioskDevices(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] Guid? defaultScannerProfileId,
            [FromQuery] KioskDeviceType? deviceType,
            [FromQuery] KioskDeviceStatus? deviceStatus,
            [FromQuery] bool? isAllowWalkIn,
            [FromQuery] bool? isAllowAppointment,
            [FromQuery] bool? isAllowInsuranceRegistration,
            [FromQuery] string? sortBy = "sortOrder",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var dateRange = ResolveDateRange(startDate, endDate, customPeriod);

            if (!dateRange.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    dateRange.ErrorMessage ?? "Filter tanggal tidak valid."
                ));
            }

            var query = BuildBaseQuery();

            query = ApplyDateFilter(query, dateRange);
            query = ApplyStandardFilter(
                query,
                search,
                isActive,
                defaultScannerProfileId,
                deviceType,
                deviceStatus,
                isAllowWalkIn,
                isAllowAppointment,
                isAllowInsuranceRegistration
            );

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(
                entities
                    .Select(x => x.CreateBy)
                    .Where(x => x != Guid.Empty)
            );

            var items = entities
                .Select(x => MapResponse(x, actorNames))
                .ToList();

            var result = new ResponseKioskDevicePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseKioskDevicePagedResult>.Ok(
                result,
                "Data kiosk device berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<KioskDeviceOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Kiosk Device",
            Description = "Melihat data pilihan kiosk device",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("KioskDevice", "Read")]
        public async Task<IActionResult> GetKioskDeviceOptions(
            [FromQuery] Guid? defaultScannerProfileId,
            [FromQuery] KioskDeviceType? deviceType,
            [FromQuery] KioskDeviceStatus? deviceStatus,
            [FromQuery] bool? isAllowWalkIn,
            [FromQuery] bool? isAllowAppointment,
            [FromQuery] bool? isAllowInsuranceRegistration,
            [FromQuery] bool onlyActive = true,
            [FromQuery] bool? activeOnly = null,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var useOnlyActive = activeOnly ?? onlyActive;

            var query = BuildBaseQuery();

            query = ApplyStandardFilter(
                query,
                search,
                useOnlyActive ? true : null,
                defaultScannerProfileId,
                deviceType,
                deviceStatus,
                isAllowWalkIn,
                isAllowAppointment,
                isAllowInsuranceRegistration
            );

            var totalData = await query.CountAsync();

            var entities = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.DeviceName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities
                .Select(MapOptionResponse)
                .ToList();

            var result = new KioskDeviceOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<KioskDeviceOptionPagedResponse>.Ok(
                result,
                "Data pilihan kiosk device berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<KioskDeviceDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Kiosk Device",
            Description = "Melihat detail kiosk device",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("KioskDevice", "Read")]
        public async Task<IActionResult> GetKioskDeviceById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Kiosk device tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var data = MapDetailResponse(entity, actorNames);

            return Ok(ApiResponse<KioskDeviceDetailResponse>.Ok(
                data,
                "Detail kiosk device berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<KioskDeviceCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [AccessAction(
            "Create",
            "Create Kiosk Device",
            Description = "Membuat data kiosk device",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("KioskDevice", "Create")]
        public async Task<IActionResult> CreateKioskDevice([FromBody] CreateKioskDeviceRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                request: request
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data kiosk device tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var generatedKioskDeviceCode = await GenerateKioskDeviceCodeAsync();

                var entity = new MstKioskDevice
                {
                    Id = Guid.NewGuid(),
                    DeviceCode = generatedKioskDeviceCode,
                    DeviceName = request.DeviceName.Trim(),
                    DeviceType = request.DeviceType,
                    DeviceStatus = request.DeviceStatus,
                    DefaultScannerProfileId = NormalizeNullableGuid(request.DefaultScannerProfileId),
                    LocationName = NormalizeNullableText(request.LocationName),
                    FloorName = NormalizeNullableText(request.FloorName),
                    IpAddress = NormalizeNullableText(request.IpAddress),
                    MacAddress = NormalizeMacAddress(request.MacAddress),
                    IsAllowWalkIn = request.IsAllowWalkIn,
                    IsAllowAppointment = request.IsAllowAppointment,
                    IsAllowInsuranceRegistration = request.IsAllowInsuranceRegistration,
                    SortOrder = request.SortOrder,
                    Description = NormalizeNullableText(request.Description),
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<MstKioskDevice>().Add(entity);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                var actorNames = await GetActorNameMapAsync(new[] { entity.CreateBy });

                var result = new KioskDeviceCreateResponse
                {
                    Id = entity.Id,
                    DeviceCode = entity.DeviceCode,
                    DeviceName = entity.DeviceName,
                    DeviceType = entity.DeviceType,
                    DeviceTypeName = BuildEnumLabel(entity.DeviceType.ToString()),
                    DeviceStatus = entity.DeviceStatus,
                    DeviceStatusName = BuildEnumLabel(entity.DeviceStatus.ToString()),
                    DefaultScannerProfileId = entity.DefaultScannerProfileId,
                    IsActive = entity.IsActive,
                    CreateDateTime = entity.CreateDateTime,
                    CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                    CreateByName = GetActorName(actorNames, entity.CreateBy)
                };

                await _loggerService.InfoAsync(
                    LogCategory,
                    "KioskDevice.CreateKioskDevice",
                    "Membuat data kiosk device.",
                    result
                );

                return Ok(ApiResponse<KioskDeviceCreateResponse>.Ok(
                    result,
                    "Kiosk device berhasil dibuat."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "KioskDevice.CreateKioskDevice",
                    "Gagal membuat data kiosk device.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat membuat kiosk device."
                    )
                );
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<KioskDeviceUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Kiosk Device",
            Description = "Mengubah data kiosk device",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("KioskDevice", "Update")]
        public async Task<IActionResult> UpdateKioskDevice(
            Guid id,
            [FromBody] UpdateKioskDeviceRequest request)
        {
            var entity = await _dbContext.Set<MstKioskDevice>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Kiosk device tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(
                excludeId: id,
                request: request
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data kiosk device tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.DeviceName = request.DeviceName.Trim();
            entity.DeviceType = request.DeviceType;
            entity.DeviceStatus = request.DeviceStatus;
            entity.DefaultScannerProfileId = NormalizeNullableGuid(request.DefaultScannerProfileId);
            entity.LocationName = NormalizeNullableText(request.LocationName);
            entity.FloorName = NormalizeNullableText(request.FloorName);
            entity.IpAddress = NormalizeNullableText(request.IpAddress);
            entity.MacAddress = NormalizeMacAddress(request.MacAddress);
            entity.IsAllowWalkIn = request.IsAllowWalkIn;
            entity.IsAllowAppointment = request.IsAllowAppointment;
            entity.IsAllowInsuranceRegistration = request.IsAllowInsuranceRegistration;
            entity.LastOnlineAt = NormalizeNullableUtcDateTime(request.LastOnlineAt);
            entity.LastOfflineAt = NormalizeNullableUtcDateTime(request.LastOfflineAt);
            entity.LastErrorMessage = NormalizeNullableText(request.LastErrorMessage);
            entity.SortOrder = request.SortOrder;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { entity.UpdateBy });

            var result = new KioskDeviceUpdateResponse
            {
                Id = entity.Id,
                DeviceCode = entity.DeviceCode,
                DeviceName = entity.DeviceName,
                DeviceType = entity.DeviceType,
                DeviceTypeName = BuildEnumLabel(entity.DeviceType.ToString()),
                DeviceStatus = entity.DeviceStatus,
                DeviceStatusName = BuildEnumLabel(entity.DeviceStatus.ToString()),
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "KioskDevice.UpdateKioskDevice",
                "Mengubah data kiosk device.",
                result
            );

            return Ok(ApiResponse<KioskDeviceUpdateResponse>.Ok(
                result,
                "Kiosk device berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<KioskDeviceUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Kiosk Device Status",
            Description = "Mengubah status aktif kiosk device",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("KioskDevice", "Update")]
        public async Task<IActionResult> UpdateKioskDeviceStatus(
            Guid id,
            [FromBody] UpdateKioskDeviceStatusRequest request)
        {
            var entity = await _dbContext.Set<MstKioskDevice>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Kiosk device tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { entity.UpdateBy });

            var result = new KioskDeviceUpdateResponse
            {
                Id = entity.Id,
                DeviceCode = entity.DeviceCode,
                DeviceName = entity.DeviceName,
                DeviceType = entity.DeviceType,
                DeviceTypeName = BuildEnumLabel(entity.DeviceType.ToString()),
                DeviceStatus = entity.DeviceStatus,
                DeviceStatusName = BuildEnumLabel(entity.DeviceStatus.ToString()),
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };

            return Ok(ApiResponse<KioskDeviceUpdateResponse>.Ok(
                result,
                "Status kiosk device berhasil diperbarui."
            ));
        }


        [HttpGet("login-summary")]
        [ProducesResponseType(typeof(ApiResponse<KioskDeviceLoginSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Kiosk Device Login Summary",
            Description = "Melihat ringkasan login kiosk device",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("KioskDevice", "Read")]
        public async Task<IActionResult> GetKioskDeviceLoginSummary()
        {
            var devices = await BuildBaseQuery().ToListAsync();
            var users = await GetKioskLoginUsersAsync(devices.Select(x => x.DeviceCode));

            var loginInfos = devices
                .Select(x => BuildLoginInfoResponse(x, users.GetValueOrDefault(x.DeviceCode)))
                .ToList();

            var result = new KioskDeviceLoginSummaryResponse
            {
                TotalDevice = loginInfos.Count,
                DeviceWithLogin = loginInfos.Count(x => x.IsLoginCreated),
                DeviceWithoutLogin = loginInfos.Count(x => !x.IsLoginCreated),
                EnabledLogin = loginInfos.Count(x => x.IsLoginCreated && x.IsLoginEnabled && !x.IsLoginLocked),
                DisabledOrLockedLogin = loginInfos.Count(x => x.IsLoginCreated && (!x.IsLoginEnabled || x.IsLoginLocked)),
                ActiveDeviceWithoutLogin = loginInfos.Count(x => x.IsActive && !x.IsLoginCreated)
            };

            return Ok(ApiResponse<KioskDeviceLoginSummaryResponse>.Ok(
                result,
                "Ringkasan login kiosk device berhasil diambil."
            ));
        }

        [HttpGet("login-status")]
        [ProducesResponseType(typeof(ApiResponse<KioskDeviceLoginSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Kiosk Device Login Status Summary",
            Description = "Melihat ringkasan status login kiosk device",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("KioskDevice", "Read")]
        public async Task<IActionResult> GetKioskDeviceLoginStatusSummary()
        {
            return await GetKioskDeviceLoginSummary();
        }

        [HttpGet("login-info")]
        [ProducesResponseType(typeof(ApiResponse<KioskDeviceLoginInfoPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Kiosk Device Login Info",
            Description = "Melihat informasi login kiosk device",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("KioskDevice", "Read")]
        public async Task<IActionResult> GetKioskDeviceLoginInfo(
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
                    x.DeviceCode.ToLower().Contains(keyword) ||
                    x.DeviceName.ToLower().Contains(keyword) ||
                    (x.LocationName != null && x.LocationName.ToLower().Contains(keyword)) ||
                    (x.FloorName != null && x.FloorName.ToLower().Contains(keyword)) ||
                    (x.IpAddress != null && x.IpAddress.ToLower().Contains(keyword)));
            }

            var devices = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.DeviceName)
                .ToListAsync();

            var users = await GetKioskLoginUsersAsync(devices.Select(x => x.DeviceCode));

            var itemsQuery = devices
                .Select(x => BuildLoginInfoResponse(x, users.GetValueOrDefault(x.DeviceCode)))
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

            var result = new KioskDeviceLoginInfoPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<KioskDeviceLoginInfoPagedResponse>.Ok(
                result,
                "Informasi login kiosk device berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}/login-info")]
        [ProducesResponseType(typeof(ApiResponse<KioskDeviceLoginInfoResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Kiosk Device Login Detail",
            Description = "Melihat detail login kiosk device",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("KioskDevice", "Read")]
        public async Task<IActionResult> GetKioskDeviceLoginInfoById(Guid id)
        {
            var device = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (device == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Kiosk device tidak ditemukan."
                ));
            }

            var user = await FindKioskLoginUserAsync(device.DeviceCode);
            var result = BuildLoginInfoResponse(device, user);

            return Ok(ApiResponse<KioskDeviceLoginInfoResponse>.Ok(
                result,
                "Detail login kiosk device berhasil diambil."
            ));
        }

        [HttpPost("{id:guid}/generate-login")]
        [ProducesResponseType(typeof(ApiResponse<KioskDeviceGenerateLoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Generate Kiosk Device Login",
            Description = "Membuat login untuk kiosk device",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("KioskDevice", "Update")]
        public async Task<IActionResult> GenerateKioskDeviceLogin(
            Guid id,
            [FromBody] GenerateKioskDeviceLoginRequest? request = null)
        {
            var device = await _dbContext.Set<MstKioskDevice>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (device == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Kiosk device tidak ditemukan."
                ));
            }

            var existingUser = await FindKioskLoginUserAsync(device.DeviceCode);

            if (existingUser != null)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Login kiosk device sudah pernah dibuat. Gunakan reset-login jika ingin mengganti password."
                ));
            }

            var userName = NormalizeLoginUserName(request?.UserName, device.DeviceCode);
            var email = NormalizeLoginEmail(request?.Email, device.DeviceCode);
            var password = string.IsNullOrWhiteSpace(request?.Password)
                ? GenerateKioskPassword()
                : request!.Password.Trim();

            var duplicateUserName = await _userManager.FindByNameAsync(userName);

            if (duplicateUserName != null)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Username login kiosk device sudah digunakan."
                ));
            }

            var duplicateEmail = await _userManager.FindByEmailAsync(email);

            if (duplicateEmail != null)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Email login kiosk device sudah digunakan."
                ));
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
                UserCode = device.DeviceCode,
                DisplayName = device.DeviceName
            };

            var createResult = await _userManager.CreateAsync(user, password);

            if (!createResult.Succeeded)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    BuildIdentityErrorMessage(createResult, "Login kiosk device gagal dibuat.")
                ));
            }

            if (request != null && !request.IsEnabled)
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
            }

            device.UpdateDateTime = now;
            device.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
            var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
            var isLocked = await _userManager.IsLockedOutAsync(user);

            var result = new KioskDeviceGenerateLoginResponse
            {
                KioskDeviceId = device.Id,
                DeviceCode = device.DeviceCode,
                DeviceName = device.DeviceName,
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

            await _loggerService.InfoAsync(
                LogCategory,
                "KioskDevice.GenerateKioskDeviceLogin",
                "Membuat login kiosk device.",
                new
                {
                    result.KioskDeviceId,
                    result.DeviceCode,
                    result.DeviceName,
                    result.LoginUserId,
                    result.LoginUserName,
                    result.IsLoginEnabled
                }
            );

            return Ok(ApiResponse<KioskDeviceGenerateLoginResponse>.Ok(
                result,
                "Login kiosk device berhasil dibuat. Simpan password karena hanya ditampilkan sekali."
            ));
        }

        [HttpPatch("{id:guid}/reset-login")]
        [ProducesResponseType(typeof(ApiResponse<KioskDeviceResetLoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Reset Kiosk Device Login",
            Description = "Reset password login kiosk device",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("KioskDevice", "Update")]
        public async Task<IActionResult> ResetKioskDeviceLogin(
            Guid id,
            [FromBody] ResetKioskDeviceLoginRequest? request = null)
        {
            var device = await _dbContext.Set<MstKioskDevice>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (device == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Kiosk device tidak ditemukan."
                ));
            }

            var user = await FindKioskLoginUserAsync(device.DeviceCode);

            if (user == null)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Login kiosk device belum dibuat. Gunakan generate-login terlebih dahulu."
                ));
            }

            var newPassword = string.IsNullOrWhiteSpace(request?.NewPassword)
                ? GenerateKioskPassword()
                : request!.NewPassword.Trim();

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

            if (!resetResult.Succeeded)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    BuildIdentityErrorMessage(resetResult, "Password login kiosk device gagal direset.")
                ));
            }

            await _userManager.ResetAccessFailedCountAsync(user);
            await _userManager.SetLockoutEndDateAsync(user, null);

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            device.UpdateDateTime = now;
            device.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { actorUserId });

            var result = new KioskDeviceResetLoginResponse
            {
                KioskDeviceId = device.Id,
                DeviceCode = device.DeviceCode,
                DeviceName = device.DeviceName,
                LoginUserId = user.Id,
                LoginUserName = user.UserName ?? string.Empty,
                NewPassword = newPassword,
                UpdateDateTime = now,
                UpdateBy = actorUserId == Guid.Empty ? null : actorUserId,
                UpdateByName = GetActorName(actorNames, actorUserId)
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "KioskDevice.ResetKioskDeviceLogin",
                "Reset login kiosk device.",
                new
                {
                    result.KioskDeviceId,
                    result.DeviceCode,
                    result.DeviceName,
                    result.LoginUserId,
                    result.LoginUserName
                }
            );

            return Ok(ApiResponse<KioskDeviceResetLoginResponse>.Ok(
                result,
                "Password login kiosk device berhasil direset. Simpan password karena hanya ditampilkan sekali."
            ));
        }

        [HttpPatch("{id:guid}/login-status")]
        [ProducesResponseType(typeof(ApiResponse<KioskDeviceLoginStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Kiosk Device Login Status",
            Description = "Mengubah status login kiosk device",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("KioskDevice", "Update")]
        public async Task<IActionResult> UpdateKioskDeviceLoginStatus(
            Guid id,
            [FromBody] UpdateKioskDeviceLoginStatusRequest request)
        {
            var device = await _dbContext.Set<MstKioskDevice>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (device == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Kiosk device tidak ditemukan."
                ));
            }

            var user = await FindKioskLoginUserAsync(device.DeviceCode);

            if (user == null)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Login kiosk device belum dibuat. Gunakan generate-login terlebih dahulu."
                ));
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

            var result = new KioskDeviceLoginStatusResponse
            {
                KioskDeviceId = device.Id,
                DeviceCode = device.DeviceCode,
                DeviceName = device.DeviceName,
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

            return Ok(ApiResponse<KioskDeviceLoginStatusResponse>.Ok(
                result,
                request.IsEnabled
                    ? "Login kiosk device berhasil diaktifkan."
                    : "Login kiosk device berhasil dinonaktifkan."
            ));
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<KioskDeviceLoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> LoginKioskDevice([FromBody] KioskDeviceLoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Username dan password wajib diisi."
                ));
            }

            var user = await _userManager.FindByNameAsync(request.UserName.Trim());

            if (user == null)
            {
                return Ok(ApiResponse<KioskDeviceLoginResponse>.Ok(
                    new KioskDeviceLoginResponse
                    {
                        IsSuccess = false,
                        Message = "Username atau password tidak valid."
                    },
                    "Login kiosk device gagal."
                ));
            }

            var isLockedBeforeCheck = await _userManager.IsLockedOutAsync(user);

            if (isLockedBeforeCheck)
            {
                var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);

                return Ok(ApiResponse<KioskDeviceLoginResponse>.Ok(
                    new KioskDeviceLoginResponse
                    {
                        IsSuccess = false,
                        Message = "Login kiosk device sedang nonaktif atau terkunci.",
                        LoginUserId = user.Id,
                        LoginUserName = user.UserName,
                        IsLoginEnabled = false,
                        IsLoginLocked = true,
                        LockoutEnd = lockoutEnd
                    },
                    "Login kiosk device gagal."
                ));
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);

            if (!isPasswordValid)
            {
                await _userManager.AccessFailedAsync(user);

                return Ok(ApiResponse<KioskDeviceLoginResponse>.Ok(
                    new KioskDeviceLoginResponse
                    {
                        IsSuccess = false,
                        Message = "Username atau password tidak valid.",
                        LoginUserId = user.Id,
                        LoginUserName = user.UserName,
                        IsLoginEnabled = true,
                        IsLoginLocked = await _userManager.IsLockedOutAsync(user),
                        LockoutEnd = await _userManager.GetLockoutEndDateAsync(user)
                    },
                    "Login kiosk device gagal."
                ));
            }

            await _userManager.ResetAccessFailedCountAsync(user);

            var loginUserCode = user.UserCode ?? user.UserName ?? string.Empty;

            var deviceQuery = _dbContext.Set<MstKioskDevice>()
                .Where(x => !x.IsDelete && x.DeviceCode == loginUserCode);

            if (!string.IsNullOrWhiteSpace(request.DeviceCode))
            {
                var deviceCode = request.DeviceCode.Trim().ToUpperInvariant();
                deviceQuery = deviceQuery.Where(x => x.DeviceCode == deviceCode);
            }

            var device = await deviceQuery.FirstOrDefaultAsync();

            if (device == null)
            {
                return Ok(ApiResponse<KioskDeviceLoginResponse>.Ok(
                    new KioskDeviceLoginResponse
                    {
                        IsSuccess = false,
                        Message = "User login tidak terhubung dengan kiosk device aktif.",
                        LoginUserId = user.Id,
                        LoginUserName = user.UserName,
                        IsLoginEnabled = true,
                        IsLoginLocked = false
                    },
                    "Login kiosk device gagal."
                ));
            }

            if (!device.IsActive)
            {
                return Ok(ApiResponse<KioskDeviceLoginResponse>.Ok(
                    new KioskDeviceLoginResponse
                    {
                        IsSuccess = false,
                        Message = "Kiosk device tidak aktif.",
                        KioskDeviceId = device.Id,
                        DeviceCode = device.DeviceCode,
                        DeviceName = device.DeviceName,
                        LoginUserId = user.Id,
                        LoginUserName = user.UserName,
                        IsLoginEnabled = true,
                        IsLoginLocked = false
                    },
                    "Login kiosk device gagal."
                ));
            }

            device.LastOnlineAt = DateTime.UtcNow;
            device.DeviceStatus = KioskDeviceStatus.Active;
            device.UpdateDateTime = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            var response = new KioskDeviceLoginResponse
            {
                IsSuccess = true,
                Message = "Login kiosk device berhasil.",
                KioskDeviceId = device.Id,
                DeviceCode = device.DeviceCode,
                DeviceName = device.DeviceName,
                LoginUserId = user.Id,
                LoginUserName = user.UserName,
                IsLoginEnabled = true,
                IsLoginLocked = false,
                LockoutEnd = await _userManager.GetLockoutEndDateAsync(user)
            };

            return Ok(ApiResponse<KioskDeviceLoginResponse>.Ok(
                response,
                "Login kiosk device berhasil."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<KioskDeviceDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Kiosk Device",
            Description = "Menghapus data kiosk device",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("KioskDevice", "Delete")]
        public async Task<IActionResult> DeleteKioskDevice(
            Guid id,
            [FromBody] DeleteKioskDeviceRequest? request = null)
        {
            var entity = await _dbContext.Set<MstKioskDevice>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Kiosk device tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            if (!string.IsNullOrWhiteSpace(request?.DeleteReason))
            {
                entity.Description = request.DeleteReason.Trim();
            }

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { entity.DeleteBy });

            var result = new KioskDeviceDeleteResponse
            {
                Id = entity.Id,
                DeviceCode = entity.DeviceCode,
                DeviceName = entity.DeviceName,
                DeleteDateTime = entity.DeleteDateTime,
                DeleteBy = entity.DeleteBy == Guid.Empty ? null : (Guid?)entity.DeleteBy,
                DeleteByName = GetActorName(actorNames, entity.DeleteBy)
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "KioskDevice.DeleteKioskDevice",
                "Menghapus data kiosk device.",
                result
            );

            return Ok(ApiResponse<KioskDeviceDeleteResponse>.Ok(
                result,
                "Kiosk device berhasil dihapus."
            ));
        }

        private IQueryable<MstKioskDevice> BuildBaseQuery()
        {
            return _dbContext.Set<MstKioskDevice>()
                .AsNoTracking()
                .Include(x => x.DefaultScannerProfile)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstKioskDevice> ApplyDateFilter(
            IQueryable<MstKioskDevice> query,
            DateRangeResolveResult dateRange)
        {
            if (dateRange.Start.HasValue)
            {
                query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);
            }

            if (dateRange.EndExclusive.HasValue)
            {
                query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);
            }

            return query;
        }

        private static IQueryable<MstKioskDevice> ApplyStandardFilter(
            IQueryable<MstKioskDevice> query,
            string? search,
            bool? isActive,
            Guid? defaultScannerProfileId,
            KioskDeviceType? deviceType,
            KioskDeviceStatus? deviceStatus,
            bool? isAllowWalkIn,
            bool? isAllowAppointment,
            bool? isAllowInsuranceRegistration)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                var matchedDeviceTypes = Enum.GetValues(typeof(KioskDeviceType))
                    .Cast<KioskDeviceType>()
                    .Where(x =>
                        x.ToString().ToLower().Contains(keyword) ||
                        BuildEnumLabel(x.ToString()).ToLower().Contains(keyword))
                    .ToList();

                var matchedDeviceStatuses = Enum.GetValues(typeof(KioskDeviceStatus))
                    .Cast<KioskDeviceStatus>()
                    .Where(x =>
                        x.ToString().ToLower().Contains(keyword) ||
                        BuildEnumLabel(x.ToString()).ToLower().Contains(keyword))
                    .ToList();

                query = query.Where(x =>
                    x.DeviceCode.ToLower().Contains(keyword) ||
                    x.DeviceName.ToLower().Contains(keyword) ||
                    (x.LocationName != null && x.LocationName.ToLower().Contains(keyword)) ||
                    (x.FloorName != null && x.FloorName.ToLower().Contains(keyword)) ||
                    (x.IpAddress != null && x.IpAddress.ToLower().Contains(keyword)) ||
                    (x.MacAddress != null && x.MacAddress.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.LastErrorMessage != null && x.LastErrorMessage.ToLower().Contains(keyword)) ||
                    (x.DefaultScannerProfile != null && x.DefaultScannerProfile.ProfileCode.ToLower().Contains(keyword)) ||
                    (x.DefaultScannerProfile != null && x.DefaultScannerProfile.ProfileName.ToLower().Contains(keyword)) ||
                    matchedDeviceTypes.Contains(x.DeviceType) ||
                    matchedDeviceStatuses.Contains(x.DeviceStatus));
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (defaultScannerProfileId.HasValue && defaultScannerProfileId.Value != Guid.Empty)
            {
                query = query.Where(x => x.DefaultScannerProfileId == defaultScannerProfileId.Value);
            }

            if (deviceType.HasValue)
            {
                query = query.Where(x => x.DeviceType == deviceType.Value);
            }

            if (deviceStatus.HasValue)
            {
                query = query.Where(x => x.DeviceStatus == deviceStatus.Value);
            }

            if (isAllowWalkIn.HasValue)
            {
                query = query.Where(x => x.IsAllowWalkIn == isAllowWalkIn.Value);
            }

            if (isAllowAppointment.HasValue)
            {
                query = query.Where(x => x.IsAllowAppointment == isAllowAppointment.Value);
            }

            if (isAllowInsuranceRegistration.HasValue)
            {
                query = query.Where(x => x.IsAllowInsuranceRegistration == isAllowInsuranceRegistration.Value);
            }

            return query;
        }

        private static IOrderedQueryable<MstKioskDevice> ApplySorting(
            IQueryable<MstKioskDevice> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(
                sortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase
            );

            return (sortBy ?? "sortOrder").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => isDescending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "devicecode" => isDescending
                    ? query.OrderByDescending(x => x.DeviceCode)
                    : query.OrderBy(x => x.DeviceCode),

                "devicename" => isDescending
                    ? query.OrderByDescending(x => x.DeviceName)
                    : query.OrderBy(x => x.DeviceName),

                "devicetype" => isDescending
                    ? query.OrderByDescending(x => x.DeviceType).ThenBy(x => x.DeviceName)
                    : query.OrderBy(x => x.DeviceType).ThenBy(x => x.DeviceName),

                "devicestatus" => isDescending
                    ? query.OrderByDescending(x => x.DeviceStatus).ThenBy(x => x.DeviceName)
                    : query.OrderBy(x => x.DeviceStatus).ThenBy(x => x.DeviceName),

                "defaultscannerprofilename" => isDescending
                    ? query.OrderByDescending(x => x.DefaultScannerProfile != null ? x.DefaultScannerProfile.ProfileName : string.Empty).ThenBy(x => x.DeviceName)
                    : query.OrderBy(x => x.DefaultScannerProfile != null ? x.DefaultScannerProfile.ProfileName : string.Empty).ThenBy(x => x.DeviceName),

                "locationname" => isDescending
                    ? query.OrderByDescending(x => x.LocationName).ThenBy(x => x.DeviceName)
                    : query.OrderBy(x => x.LocationName).ThenBy(x => x.DeviceName),

                "floorname" => isDescending
                    ? query.OrderByDescending(x => x.FloorName).ThenBy(x => x.DeviceName)
                    : query.OrderBy(x => x.FloorName).ThenBy(x => x.DeviceName),

                "ipaddress" => isDescending
                    ? query.OrderByDescending(x => x.IpAddress).ThenBy(x => x.DeviceName)
                    : query.OrderBy(x => x.IpAddress).ThenBy(x => x.DeviceName),

                "macaddress" => isDescending
                    ? query.OrderByDescending(x => x.MacAddress).ThenBy(x => x.DeviceName)
                    : query.OrderBy(x => x.MacAddress).ThenBy(x => x.DeviceName),

                "isallowwalkin" => isDescending
                    ? query.OrderByDescending(x => x.IsAllowWalkIn).ThenBy(x => x.DeviceName)
                    : query.OrderBy(x => x.IsAllowWalkIn).ThenBy(x => x.DeviceName),

                "isallowappointment" => isDescending
                    ? query.OrderByDescending(x => x.IsAllowAppointment).ThenBy(x => x.DeviceName)
                    : query.OrderBy(x => x.IsAllowAppointment).ThenBy(x => x.DeviceName),

                "isallowinsuranceregistration" => isDescending
                    ? query.OrderByDescending(x => x.IsAllowInsuranceRegistration).ThenBy(x => x.DeviceName)
                    : query.OrderBy(x => x.IsAllowInsuranceRegistration).ThenBy(x => x.DeviceName),

                "lastonlineat" => isDescending
                    ? query.OrderByDescending(x => x.LastOnlineAt).ThenBy(x => x.DeviceName)
                    : query.OrderBy(x => x.LastOnlineAt).ThenBy(x => x.DeviceName),

                "lastofflineat" => isDescending
                    ? query.OrderByDescending(x => x.LastOfflineAt).ThenBy(x => x.DeviceName)
                    : query.OrderBy(x => x.LastOfflineAt).ThenBy(x => x.DeviceName),

                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.DeviceName)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.DeviceName),

                _ => isDescending
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.DeviceName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.DeviceName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateKioskDeviceRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.DeviceName))
            {
                return (false, "Nama kiosk device wajib diisi.");
            }

            if (!Enum.IsDefined(typeof(KioskDeviceType), request.DeviceType))
            {
                return (false, "Tipe kiosk device tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            }

            if (!Enum.IsDefined(typeof(KioskDeviceStatus), request.DeviceStatus))
            {
                return (false, "Status kiosk device tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            }

            if (request.SortOrder < 0)
            {
                return (false, "Urutan tidak boleh kurang dari 0.");
            }

            var defaultScannerProfileId = NormalizeNullableGuid(request.DefaultScannerProfileId);

            if (defaultScannerProfileId.HasValue)
            {
                var scannerProfileExists = await _dbContext.Set<MstIdentityScannerProfile>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == defaultScannerProfileId.Value &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!scannerProfileExists)
                {
                    return (false, "Default scanner profile tidak valid atau tidak aktif.");
                }
            }

            if (!string.IsNullOrWhiteSpace(request.IpAddress) &&
                !IPAddress.TryParse(request.IpAddress.Trim(), out _))
            {
                return (false, "Format IP address tidak valid.");
            }

            if (!string.IsNullOrWhiteSpace(request.MacAddress) &&
                !IsValidMacAddress(request.MacAddress))
            {
                return (false, "Format MAC address tidak valid.");
            }

            var normalizedName = request.DeviceName.Trim().ToLower();

            var duplicateNameQuery = _dbContext.Set<MstKioskDevice>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.DeviceName.ToLower() == normalizedName);

            if (excludeId.HasValue)
            {
                duplicateNameQuery = duplicateNameQuery.Where(x => x.Id != excludeId.Value);
            }

            if (await duplicateNameQuery.AnyAsync())
            {
                return (false, "Nama kiosk device sudah digunakan.");
            }

            var ipAddress = NormalizeNullableText(request.IpAddress);

            if (!string.IsNullOrWhiteSpace(ipAddress))
            {
                var normalizedIpAddress = ipAddress.ToLower();

                var duplicateIpAddressQuery = _dbContext.Set<MstKioskDevice>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.IpAddress != null &&
                        x.IpAddress.ToLower() == normalizedIpAddress);

                if (excludeId.HasValue)
                {
                    duplicateIpAddressQuery = duplicateIpAddressQuery.Where(x => x.Id != excludeId.Value);
                }

                if (await duplicateIpAddressQuery.AnyAsync())
                {
                    return (false, "IP address kiosk device sudah digunakan.");
                }
            }

            var macAddress = NormalizeMacAddress(request.MacAddress);

            if (!string.IsNullOrWhiteSpace(macAddress))
            {
                var duplicateMacAddressQuery = _dbContext.Set<MstKioskDevice>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.MacAddress != null &&
                        x.MacAddress.ToUpper() == macAddress);

                if (excludeId.HasValue)
                {
                    duplicateMacAddressQuery = duplicateMacAddressQuery.Where(x => x.Id != excludeId.Value);
                }

                if (await duplicateMacAddressQuery.AnyAsync())
                {
                    return (false, "MAC address kiosk device sudah digunakan.");
                }
            }

            return (true, null);
        }

        private async Task<string> GenerateKioskDeviceCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstKioskDevice>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.DeviceCode.StartsWith(KioskDeviceCodePrefix))
                .Select(x => x.DeviceCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(TryExtractKioskDeviceSequenceNumber)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Where(x => x > 0)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }

            return KioskDeviceCodePrefix + nextNumber.ToString("D" + KioskDeviceCodeDigitLength, CultureInfo.InvariantCulture);
        }

        private static int? TryExtractKioskDeviceSequenceNumber(string kioskDeviceCode)
        {
            if (string.IsNullOrWhiteSpace(kioskDeviceCode))
            {
                return null;
            }

            if (!kioskDeviceCode.StartsWith(KioskDeviceCodePrefix, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var numberText = kioskDeviceCode[KioskDeviceCodePrefix.Length..];

            return int.TryParse(numberText, NumberStyles.None, CultureInfo.InvariantCulture, out var number)
                ? number
                : null;
        }

        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(
            IEnumerable<Guid> actorIds)
        {
            var ids = actorIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (!ids.Any())
            {
                return new Dictionary<Guid, string?>();
            }

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    Name =
                        x.DisplayName ??
                        x.UserName ??
                        x.Email ??
                        x.UserCode
                })
                .ToDictionaryAsync(x => x.Id, x => x.Name);
        }

        private static KioskDeviceResponse MapResponse(
            MstKioskDevice entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new KioskDeviceResponse
            {
                Id = entity.Id,
                DeviceCode = entity.DeviceCode,
                DeviceName = entity.DeviceName,
                DeviceType = entity.DeviceType,
                DeviceTypeName = BuildEnumLabel(entity.DeviceType.ToString()),
                DeviceStatus = entity.DeviceStatus,
                DeviceStatusName = BuildEnumLabel(entity.DeviceStatus.ToString()),
                DefaultScannerProfileId = entity.DefaultScannerProfileId,
                DefaultScannerProfileCode = entity.DefaultScannerProfile?.ProfileCode,
                DefaultScannerProfileName = entity.DefaultScannerProfile?.ProfileName,
                LocationName = entity.LocationName,
                FloorName = entity.FloorName,
                IpAddress = entity.IpAddress,
                IsAllowWalkIn = entity.IsAllowWalkIn,
                IsAllowAppointment = entity.IsAllowAppointment,
                IsAllowInsuranceRegistration = entity.IsAllowInsuranceRegistration,
                LastOnlineAt = entity.LastOnlineAt,
                LastOfflineAt = entity.LastOfflineAt,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private static KioskDeviceDetailResponse MapDetailResponse(
            MstKioskDevice entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new KioskDeviceDetailResponse
            {
                Id = entity.Id,
                DeviceCode = entity.DeviceCode,
                DeviceName = entity.DeviceName,
                DeviceType = entity.DeviceType,
                DeviceTypeName = BuildEnumLabel(entity.DeviceType.ToString()),
                DeviceStatus = entity.DeviceStatus,
                DeviceStatusName = BuildEnumLabel(entity.DeviceStatus.ToString()),
                DefaultScannerProfileId = entity.DefaultScannerProfileId,
                DefaultScannerProfileCode = entity.DefaultScannerProfile?.ProfileCode,
                DefaultScannerProfileName = entity.DefaultScannerProfile?.ProfileName,
                LocationName = entity.LocationName,
                FloorName = entity.FloorName,
                IpAddress = entity.IpAddress,
                MacAddress = entity.MacAddress,
                IsAllowWalkIn = entity.IsAllowWalkIn,
                IsAllowAppointment = entity.IsAllowAppointment,
                IsAllowInsuranceRegistration = entity.IsAllowInsuranceRegistration,
                LastOnlineAt = entity.LastOnlineAt,
                LastOfflineAt = entity.LastOfflineAt,
                LastErrorMessage = entity.LastErrorMessage,
                SortOrder = entity.SortOrder,
                Description = entity.Description,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static KioskDeviceOptionResponse MapOptionResponse(MstKioskDevice entity)
        {
            return new KioskDeviceOptionResponse
            {
                Id = entity.Id,
                DeviceCode = entity.DeviceCode,
                DeviceName = entity.DeviceName,
                DeviceType = entity.DeviceType,
                DeviceTypeName = BuildEnumLabel(entity.DeviceType.ToString()),
                DeviceStatus = entity.DeviceStatus,
                DeviceStatusName = BuildEnumLabel(entity.DeviceStatus.ToString()),
                DefaultScannerProfileId = entity.DefaultScannerProfileId,
                DefaultScannerProfileCode = entity.DefaultScannerProfile?.ProfileCode,
                DefaultScannerProfileName = entity.DefaultScannerProfile?.ProfileName,
                LocationName = entity.LocationName,
                FloorName = entity.FloorName,
                IsAllowWalkIn = entity.IsAllowWalkIn,
                IsAllowAppointment = entity.IsAllowAppointment,
                IsAllowInsuranceRegistration = entity.IsAllowInsuranceRegistration,
                SortOrder = entity.SortOrder
            };
        }

        private static List<KioskDeviceEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(x => new KioskDeviceEnumOptionResponse
                {
                    Value = Convert.ToInt32(x),
                    Name = x.ToString(),
                    Label = BuildEnumLabel(x.ToString())
                })
                .ToList();
        }

        private static List<KioskDeviceEnumMetadataResponse> BuildEnumMetadataOptions(
            List<KioskDeviceEnumOptionResponse> deviceTypeOptions,
            List<KioskDeviceEnumOptionResponse> deviceStatusOptions)
        {
            return new List<KioskDeviceEnumMetadataResponse>
            {
                new()
                {
                    EnumName = nameof(KioskDeviceType),
                    FieldName = "deviceType",
                    OptionsSource = "deviceTypeOptions",
                    Description = "Enum tipe kiosk device untuk field deviceType pada create, update, filter, response, dan option.",
                    Options = deviceTypeOptions
                        .Select(x => new KioskDeviceEnumMetadataOptionResponse
                        {
                            Value = x.Value,
                            Name = x.Name,
                            Label = x.Label
                        })
                        .ToList()
                },
                new()
                {
                    EnumName = nameof(KioskDeviceStatus),
                    FieldName = "deviceStatus",
                    OptionsSource = "deviceStatusOptions",
                    Description = "Enum status operasional kiosk device untuk field deviceStatus pada create, update, filter, response, dan option.",
                    Options = deviceStatusOptions
                        .Select(x => new KioskDeviceEnumMetadataOptionResponse
                        {
                            Value = x.Value,
                            Name = x.Name,
                            Label = x.Label
                        })
                        .ToList()
                }
            };
        }

        private static string BuildEnumLabel(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return string.Concat(value.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
        }

        private static string? GetActorName(
            IReadOnlyDictionary<Guid, string?> actorNames,
            Guid actorId)
        {
            if (actorId == Guid.Empty)
            {
                return null;
            }

            return actorNames.TryGetValue(actorId, out var actorName)
                ? actorName
                : null;
        }

        private static (int PageNumber, int PageSize) NormalizePaging(
            int pageNumber,
            int pageSize)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 25;
            }

            if (pageSize > 100)
            {
                pageSize = 100;
            }

            return (pageNumber, pageSize);
        }

        private static DateRangeResolveResult ResolveDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            var period = customPeriod?.Trim().ToLowerInvariant();
            var today = DateTime.UtcNow.Date;

            DateTime? start = null;
            DateTime? endExclusive = null;

            switch (period)
            {
                case null:
                case "":
                case "custom":
                    if (startDate.HasValue)
                    {
                        start = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                    }

                    if (endDate.HasValue)
                    {
                        endExclusive = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                    }

                    break;

                case "today":
                    start = today;
                    endExclusive = today.AddDays(1);
                    break;

                case "last7days":
                    start = today.AddDays(-6);
                    endExclusive = today.AddDays(1);
                    break;

                case "last30days":
                    start = today.AddDays(-29);
                    endExclusive = today.AddDays(1);
                    break;

                case "thismonth":
                    start = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    endExclusive = start.Value.AddMonths(1);
                    break;

                case "lastmonth":
                    var currentMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    start = currentMonthStart.AddMonths(-1);
                    endExclusive = currentMonthStart;
                    break;

                default:
                    return DateRangeResolveResult.Invalid($"customPeriod '{customPeriod}' tidak valid.");
            }

            if (start.HasValue && endExclusive.HasValue && start.Value >= endExclusive.Value)
            {
                return DateRangeResolveResult.Invalid("startDate tidak boleh lebih besar atau sama dengan endDate.");
            }

            return DateRangeResolveResult.Valid(start, endExclusive);
        }

        private static List<KioskDeviceCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<KioskDeviceCustomPeriodOptionResponse>
            {
                new() { Value = "custom", Label = "Custom", Description = "Gunakan startDate dan endDate.", UsesStartDate = true, UsesEndDate = true },
                new() { Value = "today", Label = "Hari ini", Description = "Data yang dibuat hari ini.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last7days", Label = "7 hari terakhir", Description = "Data yang dibuat dalam 7 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last30days", Label = "30 hari terakhir", Description = "Data yang dibuat dalam 30 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thismonth", Label = "Bulan ini", Description = "Data yang dibuat pada bulan berjalan.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "lastmonth", Label = "Bulan lalu", Description = "Data yang dibuat pada bulan sebelumnya.", UsesStartDate = false, UsesEndDate = false }
            };
        }

        private static List<KioskDeviceQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<KioskDeviceQueryParameterInfoResponse>
            {
                new() { Name = "startDate", Type = "DateTime?", Description = "Tanggal awal filter berdasarkan CreateDateTime.", Example = "2026-06-01" },
                new() { Name = "endDate", Type = "DateTime?", Description = "Tanggal akhir filter berdasarkan CreateDateTime.", Example = "2026-06-30" },
                new() { Name = "customPeriod", Type = "string", Description = "Filter periode cepat: custom, today, last7days, last30days, thismonth, lastmonth.", Example = "thismonth" },
                new() { Name = "search", Type = "string", Description = "Cari berdasarkan kode, nama, lokasi, lantai, IP, MAC, tipe, status, scanner profile, error, atau deskripsi.", Example = "Lobby" },
                new() { Name = "isActive", Type = "bool", Description = "Filter status aktif.", Example = "true" },
                new() { Name = "defaultScannerProfileId", Type = "Guid?", Description = "Filter berdasarkan default scanner profile.", Example = "3fa85f64-5717-4562-b3fc-2c963f66afa6" },
                new() { Name = "deviceType", Type = "enum", Description = "Filter berdasarkan tipe kiosk device.", Example = "1" },
                new() { Name = "deviceStatus", Type = "enum", Description = "Filter berdasarkan status operasional kiosk device.", Example = "1" },
                new() { Name = "isAllowWalkIn", Type = "bool", Description = "Filter kiosk yang mengizinkan walk-in.", Example = "true" },
                new() { Name = "isAllowAppointment", Type = "bool", Description = "Filter kiosk yang mengizinkan appointment.", Example = "true" },
                new() { Name = "isAllowInsuranceRegistration", Type = "bool", Description = "Filter kiosk yang mengizinkan registrasi asuransi.", Example = "true" },
                new() { Name = "sortBy", Type = "string", Description = "Kolom sorting.", Example = "sortOrder" },
                new() { Name = "sortDirection", Type = "string", Description = "Arah sorting: asc atau desc.", Example = "asc" },
                new() { Name = "pageNumber", Type = "int", Description = "Nomor halaman.", Example = "1" },
                new() { Name = "pageSize", Type = "int", Description = "Jumlah data per halaman, maksimal 100.", Example = "25" }
            };
        }

        private static List<KioskDeviceFormFieldMetadataResponse> BuildCreateFieldMetadata()
        {
            return BuildFieldMetadata(isUpdate: false);
        }

        private static List<KioskDeviceFormFieldMetadataResponse> BuildUpdateFieldMetadata()
        {
            return BuildFieldMetadata(isUpdate: true);
        }

        private static List<KioskDeviceFormFieldMetadataResponse> BuildFieldMetadata(bool isUpdate)
        {
            var fields = new List<KioskDeviceFormFieldMetadataResponse>
            {
                new() { Name = "deviceCode", Label = "Kode Kiosk Device", Section = "Basic", InputType = "readonly", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "AutoGenerated", MaxLength = 50, Description = "Digenerate otomatis oleh sistem dengan format KSK-RSMMC-00001.", Example = "KSK-RSMMC-00001", SortOrder = 1 },
                new() { Name = "deviceName", Label = "Nama Kiosk Device", Section = "Basic", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 150, Example = "Kiosk Lobby Utama", SortOrder = 2 },
                new() { Name = "deviceType", Label = "Tipe Device", Section = "Basic", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", OptionsSource = "deviceTypeOptions", SortOrder = 3 },
                new() { Name = "deviceStatus", Label = "Status Device", Section = "Basic", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", OptionsSource = "deviceStatusOptions", SortOrder = 4 },
                new() { Name = "defaultScannerProfileId", Label = "Default Scanner Profile", Section = "Scanner", InputType = "select", OptionsSource = "identityScannerProfileOptions", Description = "Opsional. Profil scanner default yang digunakan kiosk.", SortOrder = 5 },
                new() { Name = "locationName", Label = "Lokasi", Section = "Location", InputType = "text", MaxLength = 100, Example = "Lobby Utama", SortOrder = 6 },
                new() { Name = "floorName", Label = "Lantai", Section = "Location", InputType = "text", MaxLength = 50, Example = "Lantai 1", SortOrder = 7 },
                new() { Name = "ipAddress", Label = "IP Address", Section = "Network", InputType = "text", MaxLength = 100, Example = "192.168.1.50", SortOrder = 8 },
                new() { Name = "macAddress", Label = "MAC Address", Section = "Network", InputType = "text", MaxLength = 100, Example = "A1:B2:C3:D4:E5:F6", SortOrder = 9 },
                new() { Name = "isAllowWalkIn", Label = "Izinkan Walk-In", Section = "Rule", InputType = "switch", SortOrder = 10 },
                new() { Name = "isAllowAppointment", Label = "Izinkan Appointment", Section = "Rule", InputType = "switch", SortOrder = 11 },
                new() { Name = "isAllowInsuranceRegistration", Label = "Izinkan Registrasi Asuransi", Section = "Rule", InputType = "switch", SortOrder = 12 },
                new() { Name = "sortOrder", Label = "Urutan", Section = "Display", InputType = "number", SortOrder = 13 },
                new() { Name = "description", Label = "Deskripsi", Section = "Additional", InputType = "textarea", MaxLength = 250, SortOrder = 14 }
            };

            if (isUpdate)
            {
                fields.AddRange(new[]
                {
                    new KioskDeviceFormFieldMetadataResponse { Name = "lastOnlineAt", Label = "Terakhir Online", Section = "Monitoring", InputType = "datetime", SortOrder = 90 },
                    new KioskDeviceFormFieldMetadataResponse { Name = "lastOfflineAt", Label = "Terakhir Offline", Section = "Monitoring", InputType = "datetime", SortOrder = 91 },
                    new KioskDeviceFormFieldMetadataResponse { Name = "lastErrorMessage", Label = "Pesan Error Terakhir", Section = "Monitoring", InputType = "textarea", MaxLength = 250, SortOrder = 92 },
                    new KioskDeviceFormFieldMetadataResponse { Name = "isActive", Label = "Status Aktif", Section = "Status", InputType = "switch", SortOrder = 99 }
                });
            }

            return fields.OrderBy(x => x.SortOrder).ToList();
        }


        private async Task<ApplicationUser?> FindKioskLoginUserAsync(string deviceCode)
        {
            if (string.IsNullOrWhiteSpace(deviceCode))
            {
                return null;
            }

            var normalizedDeviceCode = deviceCode.Trim().ToUpperInvariant();
            var defaultUserName = NormalizeLoginUserName(null, normalizedDeviceCode);

            var userByCode = await _userManager.Users
                .FirstOrDefaultAsync(x =>
                    x.UserCode != null &&
                    x.UserCode.ToUpper() == normalizedDeviceCode);

            if (userByCode != null)
            {
                return userByCode;
            }

            var userByDefaultUserName = await _userManager.FindByNameAsync(defaultUserName);

            if (userByDefaultUserName != null)
            {
                return userByDefaultUserName;
            }

            return await _userManager.FindByNameAsync(normalizedDeviceCode);
        }

        private async Task<Dictionary<string, ApplicationUser?>> GetKioskLoginUsersAsync(
            IEnumerable<string> deviceCodes)
        {
            var codes = deviceCodes
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

        private static KioskDeviceLoginInfoResponse BuildLoginInfoResponse(
            MstKioskDevice device,
            ApplicationUser? user)
        {
            var isLoginCreated = user != null;
            var isLoginLocked = user != null &&
                user.LockoutEnabled &&
                user.LockoutEnd.HasValue &&
                user.LockoutEnd.Value > DateTimeOffset.UtcNow;

            return new KioskDeviceLoginInfoResponse
            {
                KioskDeviceId = device.Id,
                DeviceCode = device.DeviceCode,
                DeviceName = device.DeviceName,
                DeviceType = device.DeviceType,
                DeviceTypeName = BuildEnumLabel(device.DeviceType.ToString()),
                DeviceStatus = device.DeviceStatus,
                DeviceStatusName = BuildEnumLabel(device.DeviceStatus.ToString()),
                LocationName = device.LocationName,
                FloorName = device.FloorName,
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
                AccessFailedCount = user?.AccessFailedCount ?? 0,
                CanLogin = device.IsActive && isLoginCreated && !isLoginLocked
            };
        }

        private static string NormalizeLoginUserName(string? userName, string deviceCode)
        {
            if (!string.IsNullOrWhiteSpace(userName))
            {
                return userName.Trim().ToLowerInvariant();
            }

            var normalizedDeviceCode = string.IsNullOrWhiteSpace(deviceCode)
                ? Guid.NewGuid().ToString("N")
                : deviceCode.Trim().ToUpperInvariant();

            return $"kiosk.{normalizedDeviceCode}".ToLowerInvariant();
        }

        private static string NormalizeLoginEmail(string? email, string deviceCode)
        {
            if (!string.IsNullOrWhiteSpace(email))
            {
                return email.Trim().ToLowerInvariant();
            }

            var userName = NormalizeLoginUserName(null, deviceCode)
                .Replace("@", ".")
                .Replace(" ", string.Empty);

            return $"{userName}@kiosk.local";
        }

        private static string GenerateKioskPassword()
        {
            var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(8);
            return $"Kiosk@{Convert.ToHexString(bytes)}aA1!";
        }

        private static string BuildIdentityErrorMessage(
            IdentityResult identityResult,
            string defaultMessage)
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

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            return value.HasValue && value.Value != Guid.Empty
                ? value.Value
                : null;
        }

        private static DateTime? NormalizeNullableUtcDateTime(DateTime? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            return value.Value.Kind switch
            {
                DateTimeKind.Utc => value.Value,
                DateTimeKind.Local => value.Value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
            };
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static string? NormalizeMacAddress(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var cleaned = value
                .Trim()
                .Replace("-", string.Empty)
                .Replace(":", string.Empty)
                .ToUpperInvariant();

            if (cleaned.Length != 12 || !cleaned.All(Uri.IsHexDigit))
            {
                return value.Trim().ToUpperInvariant();
            }

            return string.Join(":", Enumerable.Range(0, 6).Select(i => cleaned.Substring(i * 2, 2)));
        }

        private static bool IsValidMacAddress(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            var trimmed = value.Trim();

            return Regex.IsMatch(trimmed, "^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$") ||
                   Regex.IsMatch(trimmed, "^[0-9A-Fa-f]{12}$");
        }

        private Guid GetCurrentUserId()
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("user_id");

            return Guid.TryParse(userIdValue, out var userId)
                ? userId
                : Guid.Empty;
        }

        private sealed class DateRangeResolveResult
        {
            public bool IsValid { get; private set; }
            public string? ErrorMessage { get; private set; }
            public DateTime? Start { get; private set; }
            public DateTime? EndExclusive { get; private set; }

            public static DateRangeResolveResult Valid(DateTime? start, DateTime? endExclusive)
            {
                return new DateRangeResolveResult
                {
                    IsValid = true,
                    Start = start,
                    EndExclusive = endExclusive
                };
            }

            public static DateRangeResolveResult Invalid(string errorMessage)
            {
                return new DateRangeResolveResult
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
        }
    }
}