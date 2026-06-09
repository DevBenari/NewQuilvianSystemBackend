using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
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

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public KioskDeviceController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
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
            var result = new KioskDeviceFilterMetadataResponse
            {
                DefaultFilter = new KioskDeviceDefaultFilterResponse(),
                CustomPeriods = new List<KioskDeviceCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" },
                    new() { Value = "lastmonth", Label = "Bulan lalu" }
                },
                SortOptions = new List<KioskDeviceSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "deviceCode", Label = "Kode device" },
                    new() { Value = "deviceName", Label = "Nama device" },
                    new() { Value = "deviceType", Label = "Tipe device" },
                    new() { Value = "deviceStatus", Label = "Status device" },
                    new() { Value = "serviceUnitName", Label = "Nama service unit" },
                    new() { Value = "clinicName", Label = "Nama clinic" },
                    new() { Value = "defaultScannerProfileName", Label = "Default scanner profile" },
                    new() { Value = "locationName", Label = "Lokasi" },
                    new() { Value = "floorName", Label = "Lantai" },
                    new() { Value = "ipAddress", Label = "IP address" },
                    new() { Value = "serialNumber", Label = "Serial number" },
                    new() { Value = "vendorName", Label = "Vendor" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                DeviceTypeOptions = BuildEnumOptions<KioskDeviceType>(),
                DeviceStatusOptions = BuildEnumOptions<KioskDeviceStatus>(),
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
                RegistrationAvailableDevice = await query.CountAsync(x => x.IsAvailableForRegistration),
                CheckInAvailableDevice = await query.CountAsync(x => x.IsAvailableForCheckIn),
                PaymentAvailableDevice = await query.CountAsync(x => x.IsAvailableForPayment),
                WalkInAllowedDevice = await query.CountAsync(x => x.IsAllowWalkIn),
                AppointmentAllowedDevice = await query.CountAsync(x => x.IsAllowAppointment),
                InsuranceRegistrationAllowedDevice = await query.CountAsync(x => x.IsAllowInsuranceRegistration),
                WithServiceUnitDevice = await query.CountAsync(x => x.ServiceUnitId.HasValue),
                WithClinicDevice = await query.CountAsync(x => x.ClinicId.HasValue),
                WithScannerProfileDevice = await query.CountAsync(x => x.DefaultScannerProfileId.HasValue)
            };

            return Ok(ApiResponse<KioskDeviceSummaryResponse>.Ok(
                result,
                "Ringkasan kiosk device berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseKioskDevicePagedResult>), StatusCodes.Status200OK)]
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
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? clinicId,
            [FromQuery] Guid? defaultScannerProfileId,
            [FromQuery] KioskDeviceType? deviceType,
            [FromQuery] KioskDeviceStatus? deviceStatus,
            [FromQuery] bool? isAvailableForRegistration,
            [FromQuery] bool? isAvailableForCheckIn,
            [FromQuery] bool? isAvailableForPayment,
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

            var query = BuildBaseQuery();

            query = ApplyDateFilter(query, startDate, endDate, customPeriod);
            query = ApplyStandardFilter(
                query,
                isActive,
                serviceUnitId,
                clinicId,
                defaultScannerProfileId,
                deviceType,
                deviceStatus,
                isAvailableForRegistration,
                isAvailableForCheckIn,
                isAvailableForPayment,
                isAllowWalkIn,
                isAllowAppointment,
                isAllowInsuranceRegistration,
                search
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
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? clinicId,
            [FromQuery] Guid? defaultScannerProfileId,
            [FromQuery] KioskDeviceType? deviceType,
            [FromQuery] KioskDeviceStatus? deviceStatus,
            [FromQuery] bool? isAvailableForRegistration,
            [FromQuery] bool? isAvailableForCheckIn,
            [FromQuery] bool? isAvailableForPayment,
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
                useOnlyActive ? true : null,
                serviceUnitId,
                clinicId,
                defaultScannerProfileId,
                deviceType,
                deviceStatus,
                isAvailableForRegistration,
                isAvailableForCheckIn,
                isAvailableForPayment,
                isAllowWalkIn,
                isAllowAppointment,
                isAllowInsuranceRegistration,
                search
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

            if (!data.CreateBy.HasValue || data.CreateBy.Value == Guid.Empty)
            {
                data.CreateBy = null;
                data.CreateByName = null;
            }

            if (!data.UpdateBy.HasValue || data.UpdateBy.Value == Guid.Empty)
            {
                data.UpdateBy = null;
                data.UpdateByName = null;
            }

            return Ok(ApiResponse<KioskDeviceDetailResponse>.Ok(
                data,
                "Detail kiosk device berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<KioskDeviceCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
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

            var entity = new MstKioskDevice
            {
                Id = Guid.NewGuid(),
                DeviceCode = request.DeviceCode.Trim().ToUpperInvariant(),
                DeviceName = request.DeviceName.Trim(),
                DeviceType = request.DeviceType,
                DeviceStatus = request.DeviceStatus,
                ServiceUnitId = NormalizeNullableGuid(request.ServiceUnitId),
                ClinicId = NormalizeNullableGuid(request.ClinicId),
                DefaultScannerProfileId = NormalizeNullableGuid(request.DefaultScannerProfileId),
                LocationName = NormalizeNullableString(request.LocationName),
                FloorName = NormalizeNullableString(request.FloorName),
                IpAddress = NormalizeNullableString(request.IpAddress),
                MacAddress = NormalizeMacAddress(request.MacAddress),
                SerialNumber = NormalizeNullableUpperString(request.SerialNumber),
                DeviceModel = NormalizeNullableString(request.DeviceModel),
                VendorName = NormalizeNullableString(request.VendorName),
                IsAvailableForRegistration = request.IsAvailableForRegistration,
                IsAvailableForCheckIn = request.IsAvailableForCheckIn,
                IsAvailableForPayment = request.IsAvailableForPayment,
                IsAllowWalkIn = request.IsAllowWalkIn,
                IsAllowAppointment = request.IsAllowAppointment,
                IsAllowInsuranceRegistration = request.IsAllowInsuranceRegistration,
                SortOrder = request.SortOrder,
                Description = NormalizeNullableString(request.Description),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstKioskDevice>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var result = new KioskDeviceCreateResponse
            {
                Id = entity.Id,
                DeviceCode = entity.DeviceCode,
                DeviceName = entity.DeviceName,
                DeviceType = entity.DeviceType,
                DeviceTypeName = BuildEnumLabel(entity.DeviceType.ToString()),
                DeviceStatus = entity.DeviceStatus,
                DeviceStatusName = BuildEnumLabel(entity.DeviceStatus.ToString()),
                ServiceUnitId = entity.ServiceUnitId,
                ClinicId = entity.ClinicId,
                DefaultScannerProfileId = entity.DefaultScannerProfileId,
                IsAvailableForRegistration = entity.IsAvailableForRegistration,
                IsAvailableForCheckIn = entity.IsAvailableForCheckIn,
                IsAvailableForPayment = entity.IsAvailableForPayment,
                IsActive = entity.IsActive
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

            entity.DeviceCode = request.DeviceCode.Trim().ToUpperInvariant();
            entity.DeviceName = request.DeviceName.Trim();
            entity.DeviceType = request.DeviceType;
            entity.DeviceStatus = request.DeviceStatus;
            entity.ServiceUnitId = NormalizeNullableGuid(request.ServiceUnitId);
            entity.ClinicId = NormalizeNullableGuid(request.ClinicId);
            entity.DefaultScannerProfileId = NormalizeNullableGuid(request.DefaultScannerProfileId);
            entity.LocationName = NormalizeNullableString(request.LocationName);
            entity.FloorName = NormalizeNullableString(request.FloorName);
            entity.IpAddress = NormalizeNullableString(request.IpAddress);
            entity.MacAddress = NormalizeMacAddress(request.MacAddress);
            entity.SerialNumber = NormalizeNullableUpperString(request.SerialNumber);
            entity.DeviceModel = NormalizeNullableString(request.DeviceModel);
            entity.VendorName = NormalizeNullableString(request.VendorName);
            entity.IsAvailableForRegistration = request.IsAvailableForRegistration;
            entity.IsAvailableForCheckIn = request.IsAvailableForCheckIn;
            entity.IsAvailableForPayment = request.IsAvailableForPayment;
            entity.IsAllowWalkIn = request.IsAllowWalkIn;
            entity.IsAllowAppointment = request.IsAllowAppointment;
            entity.IsAllowInsuranceRegistration = request.IsAllowInsuranceRegistration;
            entity.LastOnlineAt = NormalizeNullableUtcDateTime(request.LastOnlineAt);
            entity.LastOfflineAt = NormalizeNullableUtcDateTime(request.LastOfflineAt);
            entity.LastErrorMessage = NormalizeNullableString(request.LastErrorMessage);
            entity.SortOrder = request.SortOrder;
            entity.Description = NormalizeNullableString(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var result = new KioskDeviceUpdateResponse
            {
                Id = entity.Id,
                DeviceCode = entity.DeviceCode,
                DeviceName = entity.DeviceName,
                DeviceStatus = entity.DeviceStatus,
                DeviceStatusName = BuildEnumLabel(entity.DeviceStatus.ToString()),
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime
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

            var result = new KioskDeviceUpdateResponse
            {
                Id = entity.Id,
                DeviceCode = entity.DeviceCode,
                DeviceName = entity.DeviceName,
                DeviceStatus = entity.DeviceStatus,
                DeviceStatusName = BuildEnumLabel(entity.DeviceStatus.ToString()),
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime
            };

            return Ok(ApiResponse<KioskDeviceUpdateResponse>.Ok(
                result,
                "Status kiosk device berhasil diperbarui."
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

            var result = new KioskDeviceDeleteResponse
            {
                Id = entity.Id,
                DeviceCode = entity.DeviceCode,
                DeviceName = entity.DeviceName,
                DeleteDateTime = entity.DeleteDateTime
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
                .Include(x => x.ServiceUnit)
                .Include(x => x.Clinic)
                .Include(x => x.DefaultScannerProfile)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstKioskDevice> ApplyDateFilter(
            IQueryable<MstKioskDevice> query,
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            if (startDate.HasValue)
            {
                var start = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                query = query.Where(x => x.CreateDateTime >= start);
            }

            if (endDate.HasValue)
            {
                var end = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                query = query.Where(x => x.CreateDateTime < end);
            }

            if (!startDate.HasValue &&
                !endDate.HasValue &&
                !string.IsNullOrWhiteSpace(customPeriod))
            {
                var today = DateTime.UtcNow.Date;

                switch (customPeriod.Trim().ToLowerInvariant())
                {
                    case "today":
                        query = query.Where(x =>
                            x.CreateDateTime >= today &&
                            x.CreateDateTime < today.AddDays(1));
                        break;

                    case "last7days":
                        query = query.Where(x =>
                            x.CreateDateTime >= today.AddDays(-6) &&
                            x.CreateDateTime < today.AddDays(1));
                        break;

                    case "thismonth":
                        var thisMonthStart = new DateTime(
                            today.Year,
                            today.Month,
                            1,
                            0,
                            0,
                            0,
                            DateTimeKind.Utc
                        );

                        query = query.Where(x =>
                            x.CreateDateTime >= thisMonthStart &&
                            x.CreateDateTime < thisMonthStart.AddMonths(1));
                        break;

                    case "lastmonth":
                        var currentMonthStart = new DateTime(
                            today.Year,
                            today.Month,
                            1,
                            0,
                            0,
                            0,
                            DateTimeKind.Utc
                        );

                        var lastMonthStart = currentMonthStart.AddMonths(-1);

                        query = query.Where(x =>
                            x.CreateDateTime >= lastMonthStart &&
                            x.CreateDateTime < currentMonthStart);
                        break;
                }
            }

            return query;
        }

        private static IQueryable<MstKioskDevice> ApplyStandardFilter(
            IQueryable<MstKioskDevice> query,
            bool? isActive,
            Guid? serviceUnitId,
            Guid? clinicId,
            Guid? defaultScannerProfileId,
            KioskDeviceType? deviceType,
            KioskDeviceStatus? deviceStatus,
            bool? isAvailableForRegistration,
            bool? isAvailableForCheckIn,
            bool? isAvailableForPayment,
            bool? isAllowWalkIn,
            bool? isAllowAppointment,
            bool? isAllowInsuranceRegistration,
            string? search)
        {
            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
            {
                query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);
            }

            if (clinicId.HasValue && clinicId.Value != Guid.Empty)
            {
                query = query.Where(x => x.ClinicId == clinicId.Value);
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

            if (isAvailableForRegistration.HasValue)
            {
                query = query.Where(x => x.IsAvailableForRegistration == isAvailableForRegistration.Value);
            }

            if (isAvailableForCheckIn.HasValue)
            {
                query = query.Where(x => x.IsAvailableForCheckIn == isAvailableForCheckIn.Value);
            }

            if (isAvailableForPayment.HasValue)
            {
                query = query.Where(x => x.IsAvailableForPayment == isAvailableForPayment.Value);
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

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.DeviceCode.ToLower().Contains(keyword) ||
                    x.DeviceName.ToLower().Contains(keyword) ||
                    (x.LocationName != null && x.LocationName.ToLower().Contains(keyword)) ||
                    (x.FloorName != null && x.FloorName.ToLower().Contains(keyword)) ||
                    (x.IpAddress != null && x.IpAddress.ToLower().Contains(keyword)) ||
                    (x.MacAddress != null && x.MacAddress.ToLower().Contains(keyword)) ||
                    (x.SerialNumber != null && x.SerialNumber.ToLower().Contains(keyword)) ||
                    (x.DeviceModel != null && x.DeviceModel.ToLower().Contains(keyword)) ||
                    (x.VendorName != null && x.VendorName.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.ServiceUnit != null && x.ServiceUnit.ServiceUnitCode.ToLower().Contains(keyword)) ||
                    (x.ServiceUnit != null && x.ServiceUnit.ServiceUnitName.ToLower().Contains(keyword)) ||
                    (x.Clinic != null && x.Clinic.ClinicCode.ToLower().Contains(keyword)) ||
                    (x.Clinic != null && x.Clinic.ClinicName.ToLower().Contains(keyword)) ||
                    (x.DefaultScannerProfile != null && x.DefaultScannerProfile.ProfileCode.ToLower().Contains(keyword)) ||
                    (x.DefaultScannerProfile != null && x.DefaultScannerProfile.ProfileName.ToLower().Contains(keyword)));
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

                "serviceunitname" => isDescending
                    ? query.OrderByDescending(x => x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty).ThenBy(x => x.DeviceName)
                    : query.OrderBy(x => x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty).ThenBy(x => x.DeviceName),

                "clinicname" => isDescending
                    ? query.OrderByDescending(x => x.Clinic != null ? x.Clinic.ClinicName : string.Empty).ThenBy(x => x.DeviceName)
                    : query.OrderBy(x => x.Clinic != null ? x.Clinic.ClinicName : string.Empty).ThenBy(x => x.DeviceName),

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

                "serialnumber" => isDescending
                    ? query.OrderByDescending(x => x.SerialNumber).ThenBy(x => x.DeviceName)
                    : query.OrderBy(x => x.SerialNumber).ThenBy(x => x.DeviceName),

                "vendorname" => isDescending
                    ? query.OrderByDescending(x => x.VendorName).ThenBy(x => x.DeviceName)
                    : query.OrderBy(x => x.VendorName).ThenBy(x => x.DeviceName),

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
            if (string.IsNullOrWhiteSpace(request.DeviceCode))
            {
                return (false, "Kode kiosk device wajib diisi.");
            }

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

            var serviceUnitId = NormalizeNullableGuid(request.ServiceUnitId);
            var clinicId = NormalizeNullableGuid(request.ClinicId);
            var defaultScannerProfileId = NormalizeNullableGuid(request.DefaultScannerProfileId);

            if (serviceUnitId.HasValue)
            {
                var serviceUnitExists = await _dbContext.Set<MstServiceUnit>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == serviceUnitId.Value &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!serviceUnitExists)
                {
                    return (false, "Service unit tidak valid atau tidak aktif.");
                }
            }

            if (clinicId.HasValue)
            {
                var clinicQuery = _dbContext.Set<MstClinic>()
                    .AsNoTracking()
                    .Where(x =>
                        x.Id == clinicId.Value &&
                        x.IsActive &&
                        !x.IsDelete);

                if (serviceUnitId.HasValue)
                {
                    clinicQuery = clinicQuery.Where(x => x.ServiceUnitId == serviceUnitId.Value);
                }

                var clinicExists = await clinicQuery.AnyAsync();

                if (!clinicExists)
                {
                    return (false, "Clinic tidak valid, tidak aktif, atau tidak sesuai dengan service unit.");
                }
            }

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

            var normalizedCode = request.DeviceCode.Trim().ToUpperInvariant();
            var normalizedName = request.DeviceName.Trim().ToLower();

            var duplicateCodeQuery = _dbContext.Set<MstKioskDevice>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.DeviceCode.ToUpper() == normalizedCode);

            if (excludeId.HasValue)
            {
                duplicateCodeQuery = duplicateCodeQuery.Where(x => x.Id != excludeId.Value);
            }

            if (await duplicateCodeQuery.AnyAsync())
            {
                return (false, "Kode kiosk device sudah digunakan.");
            }

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

            var ipAddress = NormalizeNullableString(request.IpAddress);

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

            var serialNumber = NormalizeNullableUpperString(request.SerialNumber);

            if (!string.IsNullOrWhiteSpace(serialNumber))
            {
                var duplicateSerialNumberQuery = _dbContext.Set<MstKioskDevice>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.SerialNumber != null &&
                        x.SerialNumber.ToUpper() == serialNumber);

                if (excludeId.HasValue)
                {
                    duplicateSerialNumberQuery = duplicateSerialNumberQuery.Where(x => x.Id != excludeId.Value);
                }

                if (await duplicateSerialNumberQuery.AnyAsync())
                {
                    return (false, "Serial number kiosk device sudah digunakan.");
                }
            }

            return (true, null);
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
                ServiceUnitId = entity.ServiceUnitId,
                ServiceUnitCode = entity.ServiceUnit?.ServiceUnitCode,
                ServiceUnitName = entity.ServiceUnit?.ServiceUnitName,
                ClinicId = entity.ClinicId,
                ClinicCode = entity.Clinic?.ClinicCode,
                ClinicName = entity.Clinic?.ClinicName,
                DefaultScannerProfileId = entity.DefaultScannerProfileId,
                DefaultScannerProfileCode = entity.DefaultScannerProfile?.ProfileCode,
                DefaultScannerProfileName = entity.DefaultScannerProfile?.ProfileName,
                LocationName = entity.LocationName,
                FloorName = entity.FloorName,
                IpAddress = entity.IpAddress,
                SerialNumber = entity.SerialNumber,
                DeviceModel = entity.DeviceModel,
                VendorName = entity.VendorName,
                IsAvailableForRegistration = entity.IsAvailableForRegistration,
                IsAvailableForCheckIn = entity.IsAvailableForCheckIn,
                IsAvailableForPayment = entity.IsAvailableForPayment,
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
                ServiceUnitId = entity.ServiceUnitId,
                ServiceUnitCode = entity.ServiceUnit?.ServiceUnitCode,
                ServiceUnitName = entity.ServiceUnit?.ServiceUnitName,
                ClinicId = entity.ClinicId,
                ClinicCode = entity.Clinic?.ClinicCode,
                ClinicName = entity.Clinic?.ClinicName,
                DefaultScannerProfileId = entity.DefaultScannerProfileId,
                DefaultScannerProfileCode = entity.DefaultScannerProfile?.ProfileCode,
                DefaultScannerProfileName = entity.DefaultScannerProfile?.ProfileName,
                LocationName = entity.LocationName,
                FloorName = entity.FloorName,
                IpAddress = entity.IpAddress,
                MacAddress = entity.MacAddress,
                SerialNumber = entity.SerialNumber,
                DeviceModel = entity.DeviceModel,
                VendorName = entity.VendorName,
                IsAvailableForRegistration = entity.IsAvailableForRegistration,
                IsAvailableForCheckIn = entity.IsAvailableForCheckIn,
                IsAvailableForPayment = entity.IsAvailableForPayment,
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
                ServiceUnitId = entity.ServiceUnitId,
                ServiceUnitCode = entity.ServiceUnit?.ServiceUnitCode,
                ServiceUnitName = entity.ServiceUnit?.ServiceUnitName,
                ClinicId = entity.ClinicId,
                ClinicCode = entity.Clinic?.ClinicCode,
                ClinicName = entity.Clinic?.ClinicName,
                DefaultScannerProfileId = entity.DefaultScannerProfileId,
                DefaultScannerProfileCode = entity.DefaultScannerProfile?.ProfileCode,
                DefaultScannerProfileName = entity.DefaultScannerProfile?.ProfileName,
                IsAvailableForRegistration = entity.IsAvailableForRegistration,
                IsAvailableForCheckIn = entity.IsAvailableForCheckIn,
                IsAvailableForPayment = entity.IsAvailableForPayment,
                IsAllowWalkIn = entity.IsAllowWalkIn,
                IsAllowAppointment = entity.IsAllowAppointment,
                IsAllowInsuranceRegistration = entity.IsAllowInsuranceRegistration
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

        private static string? NormalizeNullableString(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static string? NormalizeNullableUpperString(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim().ToUpperInvariant();
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
    }
}
