using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseKioskDevicePagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.KioskDeviceResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/kiosk-devices")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Kiosk Device",
        AreaName = "HealthServices",
        ControllerName = "KioskDevice",
        Description = "Health service master data kiosk device",
        SortOrder = 6
    )]
    [Tags("Health Services / Master Data / Kiosk Device")]
    public class KioskDeviceController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData";

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
        [AccessAction("Read", "Read Kiosk Device", Description = "Melihat data kiosk device", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("KioskDevice", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new KioskDeviceFilterMetadataResponse
            {
                DefaultFilter = new KioskDeviceDefaultFilterResponse(),
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
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                DeviceTypeOptions = BuildEnumOptions<KioskDeviceType>(),
                DeviceStatusOptions = BuildEnumOptions<KioskDeviceStatus>()
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
        [AccessAction("Read", "Read Kiosk Device", Description = "Melihat data kiosk device", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("KioskDevice", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = _dbContext.Set<MstKioskDevice>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

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
                PaymentAvailableDevice = await query.CountAsync(x => x.IsAvailableForPayment)
            };

            return Ok(ApiResponse<KioskDeviceSummaryResponse>.Ok(
                result,
                "Ringkasan kiosk device berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseKioskDevicePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Kiosk Device", Description = "Melihat data kiosk device", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("KioskDevice", "Read")]
        public async Task<IActionResult> GetKioskDevices(
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
            [FromQuery] string? sortBy = "sortOrder",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = _dbContext.Set<MstKioskDevice>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

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

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);

            if (clinicId.HasValue && clinicId.Value != Guid.Empty)
                query = query.Where(x => x.ClinicId == clinicId.Value);

            if (defaultScannerProfileId.HasValue && defaultScannerProfileId.Value != Guid.Empty)
                query = query.Where(x => x.DefaultScannerProfileId == defaultScannerProfileId.Value);

            if (deviceType.HasValue)
                query = query.Where(x => x.DeviceType == deviceType.Value);

            if (deviceStatus.HasValue)
                query = query.Where(x => x.DeviceStatus == deviceStatus.Value);

            if (isAvailableForRegistration.HasValue)
                query = query.Where(x => x.IsAvailableForRegistration == isAvailableForRegistration.Value);

            if (isAvailableForCheckIn.HasValue)
                query = query.Where(x => x.IsAvailableForCheckIn == isAvailableForCheckIn.Value);

            if (isAvailableForPayment.HasValue)
                query = query.Where(x => x.IsAvailableForPayment == isAvailableForPayment.Value);

            var totalCount = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => ToResponse(x))
                .ToListAsync();

            var result = new ResponseKioskDevicePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalCount,
                TotalPage = (int)Math.Ceiling(totalCount / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseKioskDevicePagedResult>.Ok(
                result,
                "Data kiosk device berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<KioskDeviceOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Kiosk Device", Description = "Melihat data pilihan kiosk device", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("KioskDevice", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? clinicId,
            [FromQuery] Guid? defaultScannerProfileId,
            [FromQuery] KioskDeviceType? deviceType,
            [FromQuery] KioskDeviceStatus? deviceStatus,
            [FromQuery] bool? isAvailableForRegistration,
            [FromQuery] bool? isAvailableForCheckIn,
            [FromQuery] bool? isAvailableForPayment,
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

            var query = _dbContext.Set<MstKioskDevice>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (useOnlyActive)
                query = query.Where(x => x.IsActive);

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);

            if (clinicId.HasValue && clinicId.Value != Guid.Empty)
                query = query.Where(x => x.ClinicId == clinicId.Value);

            if (defaultScannerProfileId.HasValue && defaultScannerProfileId.Value != Guid.Empty)
                query = query.Where(x => x.DefaultScannerProfileId == defaultScannerProfileId.Value);

            if (deviceType.HasValue)
                query = query.Where(x => x.DeviceType == deviceType.Value);

            if (deviceStatus.HasValue)
                query = query.Where(x => x.DeviceStatus == deviceStatus.Value);

            if (isAvailableForRegistration.HasValue)
                query = query.Where(x => x.IsAvailableForRegistration == isAvailableForRegistration.Value);

            if (isAvailableForCheckIn.HasValue)
                query = query.Where(x => x.IsAvailableForCheckIn == isAvailableForCheckIn.Value);

            if (isAvailableForPayment.HasValue)
                query = query.Where(x => x.IsAvailableForPayment == isAvailableForPayment.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.DeviceCode.ToLower().Contains(keyword) ||
                    x.DeviceName.ToLower().Contains(keyword) ||
                    (x.SerialNumber != null && x.SerialNumber.ToLower().Contains(keyword)) ||
                    (x.LocationName != null && x.LocationName.ToLower().Contains(keyword)) ||
                    (x.ServiceUnit != null && x.ServiceUnit.ServiceUnitName.ToLower().Contains(keyword)) ||
                    (x.Clinic != null && x.Clinic.ClinicName.ToLower().Contains(keyword)) ||
                    (x.DefaultScannerProfile != null && x.DefaultScannerProfile.ProfileName.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.DeviceName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new KioskDeviceOptionResponse
                {
                    Id = x.Id,
                    DeviceCode = x.DeviceCode,
                    DeviceName = x.DeviceName,
                    DeviceType = x.DeviceType,
                    DeviceStatus = x.DeviceStatus,

                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitName = x.ServiceUnit != null
                        ? x.ServiceUnit.ServiceUnitName
                        : null,

                    ClinicId = x.ClinicId,
                    ClinicName = x.Clinic != null
                        ? x.Clinic.ClinicName
                        : null,

                    DefaultScannerProfileId = x.DefaultScannerProfileId,
                    DefaultScannerProfileName = x.DefaultScannerProfile != null
                        ? x.DefaultScannerProfile.ProfileName
                        : null,

                    IsAvailableForRegistration = x.IsAvailableForRegistration,
                    IsAvailableForCheckIn = x.IsAvailableForCheckIn,
                    IsAvailableForPayment = x.IsAvailableForPayment
                })
                .ToListAsync();

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
        [AccessAction("Read", "Read Kiosk Device", Description = "Melihat detail kiosk device", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("KioskDevice", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _dbContext.Set<MstKioskDevice>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new KioskDeviceDetailResponse
                {
                    Id = x.Id,
                    DeviceCode = x.DeviceCode,
                    DeviceName = x.DeviceName,
                    DeviceType = x.DeviceType,
                    DeviceStatus = x.DeviceStatus,
                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitCode = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitCode : null,
                    ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                    ClinicId = x.ClinicId,
                    ClinicCode = x.Clinic != null ? x.Clinic.ClinicCode : null,
                    ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                    DefaultScannerProfileId = x.DefaultScannerProfileId,
                    DefaultScannerProfileCode = x.DefaultScannerProfile != null ? x.DefaultScannerProfile.ProfileCode : null,
                    DefaultScannerProfileName = x.DefaultScannerProfile != null ? x.DefaultScannerProfile.ProfileName : null,
                    LocationName = x.LocationName,
                    FloorName = x.FloorName,
                    IpAddress = x.IpAddress,
                    MacAddress = x.MacAddress,
                    SerialNumber = x.SerialNumber,
                    DeviceModel = x.DeviceModel,
                    VendorName = x.VendorName,
                    IsAvailableForRegistration = x.IsAvailableForRegistration,
                    IsAvailableForCheckIn = x.IsAvailableForCheckIn,
                    IsAvailableForPayment = x.IsAvailableForPayment,
                    IsAllowWalkIn = x.IsAllowWalkIn,
                    IsAllowAppointment = x.IsAllowAppointment,
                    IsAllowInsuranceRegistration = x.IsAllowInsuranceRegistration,
                    LastOnlineAt = x.LastOnlineAt,
                    LastOfflineAt = x.LastOfflineAt,
                    LastErrorMessage = x.LastErrorMessage,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    Description = x.Description
                })
                .FirstOrDefaultAsync();

            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Kiosk device tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<KioskDeviceDetailResponse>.Ok(
                result,
                "Detail kiosk device berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<KioskDeviceCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Kiosk Device", Description = "Membuat data kiosk device", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("KioskDevice", "Create")]
        public async Task<IActionResult> CreateKioskDevice([FromBody] CreateKioskDeviceRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                serviceUnitId: request.ServiceUnitId,
                clinicId: request.ClinicId,
                defaultScannerProfileId: request.DefaultScannerProfileId,
                deviceCode: request.DeviceCode,
                deviceName: request.DeviceName,
                ipAddress: request.IpAddress,
                macAddress: request.MacAddress,
                serialNumber: request.SerialNumber
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
                LocationName = NormalizeNullableText(request.LocationName),
                FloorName = NormalizeNullableText(request.FloorName),
                IpAddress = NormalizeNullableText(request.IpAddress),
                MacAddress = NormalizeNullableText(request.MacAddress),
                SerialNumber = NormalizeNullableText(request.SerialNumber),
                DeviceModel = NormalizeNullableText(request.DeviceModel),
                VendorName = NormalizeNullableText(request.VendorName),
                IsAvailableForRegistration = request.IsAvailableForRegistration,
                IsAvailableForCheckIn = request.IsAvailableForCheckIn,
                IsAvailableForPayment = request.IsAvailableForPayment,
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

            var response = new KioskDeviceCreateResponse
            {
                Id = entity.Id,
                DeviceCode = entity.DeviceCode,
                DeviceName = entity.DeviceName,
                DeviceType = entity.DeviceType,
                DeviceStatus = entity.DeviceStatus,
                ServiceUnitId = entity.ServiceUnitId,
                ClinicId = entity.ClinicId,
                DefaultScannerProfileId = entity.DefaultScannerProfileId,
                IsAvailableForRegistration = entity.IsAvailableForRegistration,
                IsActive = entity.IsActive
            };

            return Ok(ApiResponse<KioskDeviceCreateResponse>.Ok(
                response,
                "Kiosk device berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Kiosk Device", Description = "Mengubah data kiosk device", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("KioskDevice", "Update")]
        public async Task<IActionResult> UpdateKioskDevice(Guid id, [FromBody] UpdateKioskDeviceRequest request)
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
                serviceUnitId: request.ServiceUnitId,
                clinicId: request.ClinicId,
                defaultScannerProfileId: request.DefaultScannerProfileId,
                deviceCode: request.DeviceCode,
                deviceName: request.DeviceName,
                ipAddress: request.IpAddress,
                macAddress: request.MacAddress,
                serialNumber: request.SerialNumber
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data kiosk device tidak valid."
                ));
            }

            entity.DeviceCode = request.DeviceCode.Trim().ToUpperInvariant();
            entity.DeviceName = request.DeviceName.Trim();
            entity.DeviceType = request.DeviceType;
            entity.DeviceStatus = request.DeviceStatus;
            entity.ServiceUnitId = NormalizeNullableGuid(request.ServiceUnitId);
            entity.ClinicId = NormalizeNullableGuid(request.ClinicId);
            entity.DefaultScannerProfileId = NormalizeNullableGuid(request.DefaultScannerProfileId);
            entity.LocationName = NormalizeNullableText(request.LocationName);
            entity.FloorName = NormalizeNullableText(request.FloorName);
            entity.IpAddress = NormalizeNullableText(request.IpAddress);
            entity.MacAddress = NormalizeNullableText(request.MacAddress);
            entity.SerialNumber = NormalizeNullableText(request.SerialNumber);
            entity.DeviceModel = NormalizeNullableText(request.DeviceModel);
            entity.VendorName = NormalizeNullableText(request.VendorName);
            entity.IsAvailableForRegistration = request.IsAvailableForRegistration;
            entity.IsAvailableForCheckIn = request.IsAvailableForCheckIn;
            entity.IsAvailableForPayment = request.IsAvailableForPayment;
            entity.IsAllowWalkIn = request.IsAllowWalkIn;
            entity.IsAllowAppointment = request.IsAllowAppointment;
            entity.IsAllowInsuranceRegistration = request.IsAllowInsuranceRegistration;
            entity.LastOnlineAt = request.LastOnlineAt;
            entity.LastOfflineAt = request.LastOfflineAt;
            entity.LastErrorMessage = NormalizeNullableText(request.LastErrorMessage);
            entity.SortOrder = request.SortOrder;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Kiosk device berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/activate")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Kiosk Device", Description = "Mengaktifkan data kiosk device", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("KioskDevice", "Update")]
        public async Task<IActionResult> ActivateKioskDevice(Guid id)
        {
            return await SetActiveStatusAsync(id, true, "Kiosk device berhasil diaktifkan.");
        }

        [HttpPatch("{id:guid}/deactivate")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Kiosk Device", Description = "Menonaktifkan data kiosk device", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("KioskDevice", "Update")]
        public async Task<IActionResult> DeactivateKioskDevice(Guid id)
        {
            return await SetActiveStatusAsync(id, false, "Kiosk device berhasil dinonaktifkan.");
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Kiosk Device", Description = "Menghapus data kiosk device", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("KioskDevice", "Delete")]
        public async Task<IActionResult> DeleteKioskDevice(Guid id)
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

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Kiosk device berhasil dihapus."
            ));
        }

        private async Task<IActionResult> SetActiveStatusAsync(Guid id, bool isActive, string successMessage)
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

            entity.IsActive = isActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                successMessage
            ));
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            Guid? serviceUnitId,
            Guid? clinicId,
            Guid? defaultScannerProfileId,
            string deviceCode,
            string deviceName,
            string? ipAddress,
            string? macAddress,
            string? serialNumber)
        {
            if (string.IsNullOrWhiteSpace(deviceCode))
                return (false, "Kode kiosk device wajib diisi.");

            if (string.IsNullOrWhiteSpace(deviceName))
                return (false, "Nama kiosk device wajib diisi.");

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
            {
                var serviceUnitExists = await _dbContext.Set<MstServiceUnit>()
                    .AnyAsync(x => x.Id == serviceUnitId.Value && x.IsActive && !x.IsDelete);

                if (!serviceUnitExists)
                    return (false, "Service unit tidak valid atau tidak aktif.");
            }

            if (clinicId.HasValue && clinicId.Value != Guid.Empty)
            {
                var clinicExists = await _dbContext.Set<MstClinic>()
                    .AnyAsync(x =>
                        x.Id == clinicId.Value &&
                        x.IsActive &&
                        !x.IsDelete &&
                        (!serviceUnitId.HasValue ||
                         serviceUnitId.Value == Guid.Empty ||
                         x.ServiceUnitId == serviceUnitId.Value));

                if (!clinicExists)
                    return (false, "Clinic tidak valid, tidak aktif, atau tidak sesuai dengan service unit.");
            }

            if (defaultScannerProfileId.HasValue && defaultScannerProfileId.Value != Guid.Empty)
            {
                var profileExists = await _dbContext.Set<MstIdentityScannerProfile>()
                    .AnyAsync(x => x.Id == defaultScannerProfileId.Value && x.IsActive && !x.IsDelete);

                if (!profileExists)
                    return (false, "Default scanner profile tidak valid atau tidak aktif.");
            }

            var normalizedCode = deviceCode.Trim().ToUpperInvariant();
            var normalizedName = deviceName.Trim().ToLower();

            var duplicateCode = await _dbContext.Set<MstKioskDevice>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.DeviceCode.ToUpper() == normalizedCode &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateCode)
                return (false, "Kode kiosk device sudah digunakan.");

            var duplicateName = await _dbContext.Set<MstKioskDevice>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.DeviceName.ToLower() == normalizedName &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateName)
                return (false, "Nama kiosk device sudah digunakan.");

            if (!string.IsNullOrWhiteSpace(ipAddress))
            {
                var normalizedIpAddress = ipAddress.Trim().ToLower();

                var duplicateIpAddress = await _dbContext.Set<MstKioskDevice>()
                    .AnyAsync(x =>
                        !x.IsDelete &&
                        x.IpAddress != null &&
                        x.IpAddress.ToLower() == normalizedIpAddress &&
                        (!excludeId.HasValue || x.Id != excludeId.Value));

                if (duplicateIpAddress)
                    return (false, "IP address kiosk device sudah digunakan.");
            }

            if (!string.IsNullOrWhiteSpace(macAddress))
            {
                var normalizedMacAddress = macAddress.Trim().ToLower();

                var duplicateMacAddress = await _dbContext.Set<MstKioskDevice>()
                    .AnyAsync(x =>
                        !x.IsDelete &&
                        x.MacAddress != null &&
                        x.MacAddress.ToLower() == normalizedMacAddress &&
                        (!excludeId.HasValue || x.Id != excludeId.Value));

                if (duplicateMacAddress)
                    return (false, "MAC address kiosk device sudah digunakan.");
            }

            if (!string.IsNullOrWhiteSpace(serialNumber))
            {
                var normalizedSerialNumber = serialNumber.Trim().ToLower();

                var duplicateSerialNumber = await _dbContext.Set<MstKioskDevice>()
                    .AnyAsync(x =>
                        !x.IsDelete &&
                        x.SerialNumber != null &&
                        x.SerialNumber.ToLower() == normalizedSerialNumber &&
                        (!excludeId.HasValue || x.Id != excludeId.Value));

                if (duplicateSerialNumber)
                    return (false, "Serial number kiosk device sudah digunakan.");
            }

            return (true, null);
        }

        private static IQueryable<MstKioskDevice> ApplySorting(
            IQueryable<MstKioskDevice> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").ToLowerInvariant() switch
            {
                "createdatetime" => isDesc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "devicecode" => isDesc
                    ? query.OrderByDescending(x => x.DeviceCode)
                    : query.OrderBy(x => x.DeviceCode),

                "devicename" => isDesc
                    ? query.OrderByDescending(x => x.DeviceName)
                    : query.OrderBy(x => x.DeviceName),

                "devicetype" => isDesc
                    ? query.OrderByDescending(x => x.DeviceType)
                    : query.OrderBy(x => x.DeviceType),

                "devicestatus" => isDesc
                    ? query.OrderByDescending(x => x.DeviceStatus)
                    : query.OrderBy(x => x.DeviceStatus),

                "serviceunitname" => isDesc
                    ? query.OrderByDescending(x => x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : "")
                    : query.OrderBy(x => x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : ""),

                "clinicname" => isDesc
                    ? query.OrderByDescending(x => x.Clinic != null ? x.Clinic.ClinicName : "")
                    : query.OrderBy(x => x.Clinic != null ? x.Clinic.ClinicName : ""),

                "defaultscannerprofilename" => isDesc
                    ? query.OrderByDescending(x => x.DefaultScannerProfile != null ? x.DefaultScannerProfile.ProfileName : "")
                    : query.OrderBy(x => x.DefaultScannerProfile != null ? x.DefaultScannerProfile.ProfileName : ""),

                "locationname" => isDesc
                    ? query.OrderByDescending(x => x.LocationName)
                    : query.OrderBy(x => x.LocationName),

                "isactive" => isDesc
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => isDesc
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.DeviceName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.DeviceName)
            };
        }

        private static KioskDeviceResponse ToResponse(MstKioskDevice x)
        {
            return new KioskDeviceResponse
            {
                Id = x.Id,
                DeviceCode = x.DeviceCode,
                DeviceName = x.DeviceName,
                DeviceType = x.DeviceType,
                DeviceStatus = x.DeviceStatus,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitCode = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitCode : null,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                ClinicId = x.ClinicId,
                ClinicCode = x.Clinic != null ? x.Clinic.ClinicCode : null,
                ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                DefaultScannerProfileId = x.DefaultScannerProfileId,
                DefaultScannerProfileCode = x.DefaultScannerProfile != null ? x.DefaultScannerProfile.ProfileCode : null,
                DefaultScannerProfileName = x.DefaultScannerProfile != null ? x.DefaultScannerProfile.ProfileName : null,
                LocationName = x.LocationName,
                FloorName = x.FloorName,
                IpAddress = x.IpAddress,
                SerialNumber = x.SerialNumber,
                DeviceModel = x.DeviceModel,
                VendorName = x.VendorName,
                IsAvailableForRegistration = x.IsAvailableForRegistration,
                IsAvailableForCheckIn = x.IsAvailableForCheckIn,
                IsAvailableForPayment = x.IsAvailableForPayment,
                IsAllowWalkIn = x.IsAllowWalkIn,
                IsAllowAppointment = x.IsAllowAppointment,
                IsAllowInsuranceRegistration = x.IsAllowInsuranceRegistration,
                LastOnlineAt = x.LastOnlineAt,
                LastOfflineAt = x.LastOfflineAt,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static List<KioskDeviceEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(x => new KioskDeviceEnumOptionResponse
                {
                    Value = Convert.ToInt32(x),
                    Name = x.ToString(),
                    Label = SplitPascalCase(x.ToString())
                })
                .ToList();
        }

        private static string SplitPascalCase(string value)
        {
            return string.Concat(value.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
        }

        private Guid GetCurrentUserId()
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
        }

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            return value.HasValue && value.Value != Guid.Empty
                ? value.Value
                : null;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}