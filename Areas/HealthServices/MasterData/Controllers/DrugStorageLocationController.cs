using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseDrugStorageLocationPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.DrugStorageLocationResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/drug-storage-locations")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Drug Storage Location",
        AreaName = "HealthServices",
        ControllerName = "DrugStorageLocation",
        Description = "Health service master data drug storage location",
        SortOrder = 14
    )]
    [Tags("Health Services / Master Data / Drug Storage Location")]
    public class DrugStorageLocationController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData";
        private const string CodePrefix = "DSL-RSMMC-";
        private const int CodeNumberLength = 5;

        private static readonly HashSet<string> AllowedStorageLocationTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "General",
            "MainWarehouse",
            "Pharmacy",
            "Clinic",
            "Emergency",
            "OperatingRoom",
            "ColdStorage",
            "NarcoticStorage",
            "Quarantine"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public DrugStorageLocationController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<DrugStorageLocationFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Drug Storage Location", Description = "Melihat data drug storage location", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DrugStorageLocation", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new DrugStorageLocationFilterMetadataResponse
            {
                DefaultFilter = new DrugStorageLocationDefaultFilterResponse(),
                CustomPeriods = BuildCustomPeriodOptions(),
                SortOptions = new List<DrugStorageLocationSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "storageLocationCode", Label = "Kode lokasi" },
                    new() { Value = "storageLocationName", Label = "Nama lokasi" },
                    new() { Value = "storageLocationType", Label = "Tipe lokasi" },
                    new() { Value = "serviceUnitName", Label = "Service unit" },
                    new() { Value = "parentStorageLocationName", Label = "Parent lokasi" },
                    new() { Value = "locationGroupName", Label = "Grup lokasi" },
                    new() { Value = "isDefault", Label = "Default" },
                    new() { Value = "isMainWarehouse", Label = "Main warehouse" },
                    new() { Value = "isPharmacyLocation", Label = "Lokasi farmasi" },
                    new() { Value = "isColdChain", Label = "Cold chain" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                StorageLocationTypeOptions = AllowedStorageLocationTypes.OrderBy(x => x).ToList()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "DrugStorageLocation.GetFilterMetadata",
                "Mengambil metadata filter drug storage location.",
                result
            );

            return Ok(ApiResponse<DrugStorageLocationFilterMetadataResponse>.Ok(
                result,
                "Metadata filter drug storage location berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<DrugStorageLocationSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Drug Storage Location", Description = "Melihat data drug storage location", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DrugStorageLocation", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = _dbContext.Set<MstDrugStorageLocation>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new DrugStorageLocationSummaryResponse
            {
                TotalStorageLocation = await query.CountAsync(),
                ActiveStorageLocation = await query.CountAsync(x => x.IsActive),
                InactiveStorageLocation = await query.CountAsync(x => !x.IsActive),
                DefaultStorageLocation = await query.CountAsync(x => x.IsDefault),
                MainWarehouseLocation = await query.CountAsync(x => x.IsMainWarehouse),
                PharmacyLocation = await query.CountAsync(x => x.IsPharmacyLocation),
                ColdChainLocation = await query.CountAsync(x => x.IsColdChain),
                ControlledDrugStorageLocation = await query.CountAsync(x => x.IsControlledDrugStorage),
                HighAlertStorageLocation = await query.CountAsync(x => x.IsHighAlertStorage),
                QuarantineLocation = await query.CountAsync(x => x.IsQuarantineLocation),
                AllowReceivingLocation = await query.CountAsync(x => x.IsAllowReceiving),
                AllowDispensingLocation = await query.CountAsync(x => x.IsAllowDispensing),
                AllowTransferInLocation = await query.CountAsync(x => x.IsAllowTransferIn),
                AllowTransferOutLocation = await query.CountAsync(x => x.IsAllowTransferOut)
            };

            return Ok(ApiResponse<DrugStorageLocationSummaryResponse>.Ok(
                result,
                "Ringkasan drug storage location berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseDrugStorageLocationPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Drug Storage Location", Description = "Melihat data drug storage location", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DrugStorageLocation", "Read")]
        public async Task<IActionResult> GetDrugStorageLocations(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? parentStorageLocationId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
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

            if (dateRange.Start.HasValue)
                query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);

            if (dateRange.EndExclusive.HasValue)
                query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);

            if (parentStorageLocationId.HasValue && parentStorageLocationId.Value != Guid.Empty)
                query = query.Where(x => x.ParentStorageLocationId == parentStorageLocationId.Value);

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            query = ApplySearch(query, search);

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => ToResponse(x))
                .ToListAsync();

            var result = new ResponseDrugStorageLocationPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseDrugStorageLocationPagedResult>.Ok(
                result,
                "Data drug storage location berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<DrugStorageLocationOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Drug Storage Location", Description = "Melihat data pilihan drug storage location", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DrugStorageLocation", "Read")]
        public async Task<IActionResult> GetDrugStorageLocationOptions(
    [FromQuery] Guid? parentStorageLocationId,
    [FromQuery] Guid? serviceUnitId,
    [FromQuery] bool onlyActive = true,
    [FromQuery] string? search = null,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (parentStorageLocationId.HasValue && parentStorageLocationId.Value != Guid.Empty)
                query = query.Where(x => x.ParentStorageLocationId == parentStorageLocationId.Value);

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);

            query = ApplySearch(query, search);

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.StorageLocationName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new DrugStorageLocationOptionResponse
                {
                    Id = x.Id,
                    ParentStorageLocationId = x.ParentStorageLocationId,
                    ParentStorageLocationName = x.ParentStorageLocation != null
                        ? x.ParentStorageLocation.StorageLocationName
                        : null,

                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitName = x.ServiceUnit != null
                        ? x.ServiceUnit.ServiceUnitName
                        : null,

                    StorageLocationCode = x.StorageLocationCode,
                    StorageLocationName = x.StorageLocationName,
                    StorageLocationType = x.StorageLocationType,
                    LocationGroupName = x.LocationGroupName,

                    IsDefault = x.IsDefault,
                    IsMainWarehouse = x.IsMainWarehouse,
                    IsPharmacyLocation = x.IsPharmacyLocation,
                    IsColdChain = x.IsColdChain,
                    IsControlledDrugStorage = x.IsControlledDrugStorage,
                    IsHighAlertStorage = x.IsHighAlertStorage,
                    IsQuarantineLocation = x.IsQuarantineLocation,
                    IsAllowReceiving = x.IsAllowReceiving,
                    IsAllowDispensing = x.IsAllowDispensing,
                    IsAllowTransferIn = x.IsAllowTransferIn,
                    IsAllowTransferOut = x.IsAllowTransferOut
                })
                .ToListAsync();

            var result = new DrugStorageLocationOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<DrugStorageLocationOptionPagedResponse>.Ok(
                result,
                "Data pilihan drug storage location berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DrugStorageLocationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Drug Storage Location", Description = "Melihat detail drug storage location", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DrugStorageLocation", "Read")]
        public async Task<IActionResult> GetDrugStorageLocationById(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new DrugStorageLocationDetailResponse
                {
                    Id = x.Id,
                    ParentStorageLocationId = x.ParentStorageLocationId,
                    ParentStorageLocationCode = x.ParentStorageLocation != null ? x.ParentStorageLocation.StorageLocationCode : null,
                    ParentStorageLocationName = x.ParentStorageLocation != null ? x.ParentStorageLocation.StorageLocationName : null,
                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitCode = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitCode : null,
                    ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                    ClinicId = x.ClinicId,
                    ClinicCode = x.Clinic != null ? x.Clinic.ClinicCode : null,
                    ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                    RoomId = x.RoomId,
                    RoomCode = x.Room != null ? x.Room.RoomCode : null,
                    MasterRoomName = x.Room != null ? x.Room.RoomName : null,
                    StorageLocationCode = x.StorageLocationCode,
                    StorageLocationName = x.StorageLocationName,
                    StorageLocationType = x.StorageLocationType,
                    LocationGroupName = x.LocationGroupName,
                    FloorName = x.FloorName,
                    RoomName = x.RoomName,
                    RackCode = x.RackCode,
                    ShelfCode = x.ShelfCode,
                    BinCode = x.BinCode,
                    MinimumTemperatureCelsius = x.MinimumTemperatureCelsius,
                    MaximumTemperatureCelsius = x.MaximumTemperatureCelsius,
                    MinimumHumidityPercent = x.MinimumHumidityPercent,
                    MaximumHumidityPercent = x.MaximumHumidityPercent,
                    IsDefault = x.IsDefault,
                    IsMainWarehouse = x.IsMainWarehouse,
                    IsPharmacyLocation = x.IsPharmacyLocation,
                    IsColdChain = x.IsColdChain,
                    IsControlledDrugStorage = x.IsControlledDrugStorage,
                    IsHighAlertStorage = x.IsHighAlertStorage,
                    IsQuarantineLocation = x.IsQuarantineLocation,
                    IsAllowReceiving = x.IsAllowReceiving,
                    IsAllowDispensing = x.IsAllowDispensing,
                    IsAllowTransferIn = x.IsAllowTransferIn,
                    IsAllowTransferOut = x.IsAllowTransferOut,
                    SortOrder = x.SortOrder,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : (Guid?)x.CreateBy
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Drug storage location tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<DrugStorageLocationDetailResponse>.Ok(
                data,
                "Detail drug storage location berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DrugStorageLocationCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Drug Storage Location", Description = "Membuat data drug storage location", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("DrugStorageLocation", "Create")]
        public async Task<IActionResult> CreateDrugStorageLocation([FromBody] CreateDrugStorageLocationRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                parentStorageLocationId: request.ParentStorageLocationId,
                serviceUnitId: request.ServiceUnitId,
                clinicId: request.ClinicId,
                roomId: request.RoomId,
                storageLocationName: request.StorageLocationName,
                storageLocationType: request.StorageLocationType,
                minimumTemperatureCelsius: request.MinimumTemperatureCelsius,
                maximumTemperatureCelsius: request.MaximumTemperatureCelsius,
                minimumHumidityPercent: request.MinimumHumidityPercent,
                maximumHumidityPercent: request.MaximumHumidityPercent
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data drug storage location tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            if (request.IsDefault)
            {
                await ResetDefaultStorageLocationAsync(actorUserId, now);
            }

            var entity = new MstDrugStorageLocation
            {
                Id = Guid.NewGuid(),
                ParentStorageLocationId = NormalizeNullableGuid(request.ParentStorageLocationId),
                ServiceUnitId = NormalizeNullableGuid(request.ServiceUnitId),
                ClinicId = NormalizeNullableGuid(request.ClinicId),
                RoomId = NormalizeNullableGuid(request.RoomId),
                StorageLocationCode = await GenerateStorageLocationCodeAsync(),
                StorageLocationName = request.StorageLocationName.Trim(),
                StorageLocationType = NormalizeStorageLocationType(request.StorageLocationType),
                LocationGroupName = NormalizeNullableText(request.LocationGroupName),
                FloorName = NormalizeNullableText(request.FloorName),
                RoomName = NormalizeNullableText(request.RoomName),
                RackCode = NormalizeNullableText(request.RackCode),
                ShelfCode = NormalizeNullableText(request.ShelfCode),
                BinCode = NormalizeNullableText(request.BinCode),
                MinimumTemperatureCelsius = request.MinimumTemperatureCelsius,
                MaximumTemperatureCelsius = request.MaximumTemperatureCelsius,
                MinimumHumidityPercent = request.MinimumHumidityPercent,
                MaximumHumidityPercent = request.MaximumHumidityPercent,
                IsDefault = request.IsDefault,
                IsMainWarehouse = request.IsMainWarehouse,
                IsPharmacyLocation = request.IsPharmacyLocation,
                IsColdChain = request.IsColdChain,
                IsControlledDrugStorage = request.IsControlledDrugStorage,
                IsHighAlertStorage = request.IsHighAlertStorage,
                IsQuarantineLocation = request.IsQuarantineLocation,
                IsAllowReceiving = request.IsAllowReceiving,
                IsAllowDispensing = request.IsAllowDispensing,
                IsAllowTransferIn = request.IsAllowTransferIn,
                IsAllowTransferOut = request.IsAllowTransferOut,
                SortOrder = request.SortOrder,
                Description = NormalizeNullableText(request.Description),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstDrugStorageLocation>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = ToCreateResponse(entity);

            await _loggerService.InfoAsync(
                LogCategory,
                "DrugStorageLocation.CreateDrugStorageLocation",
                "Membuat data drug storage location.",
                response
            );

            return Ok(ApiResponse<DrugStorageLocationCreateResponse>.Ok(
                response,
                "Drug storage location berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DrugStorageLocationUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Drug Storage Location", Description = "Mengubah data drug storage location", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DrugStorageLocation", "Update")]
        public async Task<IActionResult> UpdateDrugStorageLocation(Guid id, [FromBody] UpdateDrugStorageLocationRequest request)
        {
            var entity = await _dbContext.Set<MstDrugStorageLocation>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Drug storage location tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(
                excludeId: id,
                parentStorageLocationId: request.ParentStorageLocationId,
                serviceUnitId: request.ServiceUnitId,
                clinicId: request.ClinicId,
                roomId: request.RoomId,
                storageLocationName: request.StorageLocationName,
                storageLocationType: request.StorageLocationType,
                minimumTemperatureCelsius: request.MinimumTemperatureCelsius,
                maximumTemperatureCelsius: request.MaximumTemperatureCelsius,
                minimumHumidityPercent: request.MinimumHumidityPercent,
                maximumHumidityPercent: request.MaximumHumidityPercent
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data drug storage location tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            if (request.IsDefault && !entity.IsDefault)
            {
                await ResetDefaultStorageLocationAsync(actorUserId, now);
            }

            entity.ParentStorageLocationId = NormalizeNullableGuid(request.ParentStorageLocationId);
            entity.ServiceUnitId = NormalizeNullableGuid(request.ServiceUnitId);
            entity.ClinicId = NormalizeNullableGuid(request.ClinicId);
            entity.RoomId = NormalizeNullableGuid(request.RoomId);
            entity.StorageLocationName = request.StorageLocationName.Trim();
            entity.StorageLocationType = NormalizeStorageLocationType(request.StorageLocationType);
            entity.LocationGroupName = NormalizeNullableText(request.LocationGroupName);
            entity.FloorName = NormalizeNullableText(request.FloorName);
            entity.RoomName = NormalizeNullableText(request.RoomName);
            entity.RackCode = NormalizeNullableText(request.RackCode);
            entity.ShelfCode = NormalizeNullableText(request.ShelfCode);
            entity.BinCode = NormalizeNullableText(request.BinCode);
            entity.MinimumTemperatureCelsius = request.MinimumTemperatureCelsius;
            entity.MaximumTemperatureCelsius = request.MaximumTemperatureCelsius;
            entity.MinimumHumidityPercent = request.MinimumHumidityPercent;
            entity.MaximumHumidityPercent = request.MaximumHumidityPercent;
            entity.IsDefault = request.IsDefault;
            entity.IsMainWarehouse = request.IsMainWarehouse;
            entity.IsPharmacyLocation = request.IsPharmacyLocation;
            entity.IsColdChain = request.IsColdChain;
            entity.IsControlledDrugStorage = request.IsControlledDrugStorage;
            entity.IsHighAlertStorage = request.IsHighAlertStorage;
            entity.IsQuarantineLocation = request.IsQuarantineLocation;
            entity.IsAllowReceiving = request.IsAllowReceiving;
            entity.IsAllowDispensing = request.IsAllowDispensing;
            entity.IsAllowTransferIn = request.IsAllowTransferIn;
            entity.IsAllowTransferOut = request.IsAllowTransferOut;
            entity.SortOrder = request.SortOrder;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var response = ToUpdateResponse(entity);

            return Ok(ApiResponse<DrugStorageLocationUpdateResponse>.Ok(
                response,
                "Drug storage location berhasil diperbarui."
            ));
        }


        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Drug Storage Location Status", Description = "Mengubah status drug storage location", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DrugStorageLocation", "Update")]
        public async Task<IActionResult> UpdateDrugStorageLocationStatus(
            Guid id,
            [FromBody] UpdateDrugStorageLocationStatusRequest request)
        {
            var entity = await _dbContext.Set<MstDrugStorageLocation>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Drug storage location tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;
            if (!request.IsActive)
            {
                entity.IsDefault = false;
            }
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status drug storage location berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Drug Storage Location", Description = "Menghapus data drug storage location", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("DrugStorageLocation", "Delete")]
        public async Task<IActionResult> DeleteDrugStorageLocation(Guid id, [FromBody] DeleteDrugStorageLocationRequest? request = null)
        {
            var entity = await _dbContext.Set<MstDrugStorageLocation>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Drug storage location tidak ditemukan."
                ));
            }

            var hasChild = await _dbContext.Set<MstDrugStorageLocation>()
                .AnyAsync(x => x.ParentStorageLocationId == id && !x.IsDelete);

            if (hasChild)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Drug storage location tidak dapat dihapus karena masih memiliki child storage location."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.IsDefault = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            if (!string.IsNullOrWhiteSpace(request?.DeleteReason))
            {
                entity.Description = request.DeleteReason.Trim();
            }

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Drug storage location berhasil dihapus."
            ));
        }

        private IQueryable<MstDrugStorageLocation> BuildBaseQuery()
        {
            return _dbContext.Set<MstDrugStorageLocation>()
                .AsNoTracking()
                .Include(x => x.ParentStorageLocation)
                .Include(x => x.ServiceUnit)
                .Include(x => x.Clinic)
                .Include(x => x.Room)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstDrugStorageLocation> ApplySearch(
            IQueryable<MstDrugStorageLocation> query,
            string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            var keyword = search.Trim().ToLower();

            return query.Where(x =>
                x.StorageLocationCode.ToLower().Contains(keyword) ||
                x.StorageLocationName.ToLower().Contains(keyword) ||
                x.StorageLocationType.ToLower().Contains(keyword) ||
                (x.LocationGroupName != null && x.LocationGroupName.ToLower().Contains(keyword)) ||
                (x.FloorName != null && x.FloorName.ToLower().Contains(keyword)) ||
                (x.RoomName != null && x.RoomName.ToLower().Contains(keyword)) ||
                (x.RackCode != null && x.RackCode.ToLower().Contains(keyword)) ||
                (x.ShelfCode != null && x.ShelfCode.ToLower().Contains(keyword)) ||
                (x.BinCode != null && x.BinCode.ToLower().Contains(keyword)) ||
                (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                (x.ParentStorageLocation != null && x.ParentStorageLocation.StorageLocationCode.ToLower().Contains(keyword)) ||
                (x.ParentStorageLocation != null && x.ParentStorageLocation.StorageLocationName.ToLower().Contains(keyword)) ||
                (x.ServiceUnit != null && x.ServiceUnit.ServiceUnitCode.ToLower().Contains(keyword)) ||
                (x.ServiceUnit != null && x.ServiceUnit.ServiceUnitName.ToLower().Contains(keyword)) ||
                (x.Clinic != null && x.Clinic.ClinicCode.ToLower().Contains(keyword)) ||
                (x.Clinic != null && x.Clinic.ClinicName.ToLower().Contains(keyword)) ||
                (x.Room != null && x.Room.RoomCode.ToLower().Contains(keyword)) ||
                (x.Room != null && x.Room.RoomName.ToLower().Contains(keyword)));
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            Guid? parentStorageLocationId,
            Guid? serviceUnitId,
            Guid? clinicId,
            Guid? roomId,
            string storageLocationName,
            string storageLocationType,
            decimal? minimumTemperatureCelsius,
            decimal? maximumTemperatureCelsius,
            decimal? minimumHumidityPercent,
            decimal? maximumHumidityPercent)
        {
            if (string.IsNullOrWhiteSpace(storageLocationName))
                return (false, "Nama storage location wajib diisi.");

            if (string.IsNullOrWhiteSpace(storageLocationType))
                return (false, "Tipe storage location wajib diisi.");

            if (!AllowedStorageLocationTypes.Contains(storageLocationType.Trim()))
            {
                return (false, "Tipe storage location tidak valid. Gunakan salah satu: General, MainWarehouse, Pharmacy, Clinic, Emergency, OperatingRoom, ColdStorage, NarcoticStorage, Quarantine.");
            }

            if (minimumTemperatureCelsius.HasValue && maximumTemperatureCelsius.HasValue && maximumTemperatureCelsius.Value < minimumTemperatureCelsius.Value)
                return (false, "Maximum temperature tidak boleh lebih kecil dari minimum temperature.");

            if (minimumHumidityPercent.HasValue && (minimumHumidityPercent.Value < 0 || minimumHumidityPercent.Value > 100))
                return (false, "Minimum humidity harus berada pada rentang 0 sampai 100 persen.");

            if (maximumHumidityPercent.HasValue && (maximumHumidityPercent.Value < 0 || maximumHumidityPercent.Value > 100))
                return (false, "Maximum humidity harus berada pada rentang 0 sampai 100 persen.");

            if (minimumHumidityPercent.HasValue && maximumHumidityPercent.HasValue && maximumHumidityPercent.Value < minimumHumidityPercent.Value)
                return (false, "Maximum humidity tidak boleh lebih kecil dari minimum humidity.");

            var normalizedParentStorageLocationId = NormalizeNullableGuid(parentStorageLocationId);
            var normalizedServiceUnitId = NormalizeNullableGuid(serviceUnitId);
            var normalizedClinicId = NormalizeNullableGuid(clinicId);
            var normalizedRoomId = NormalizeNullableGuid(roomId);

            if (excludeId.HasValue && normalizedParentStorageLocationId.HasValue && normalizedParentStorageLocationId.Value == excludeId.Value)
                return (false, "Parent storage location tidak boleh sama dengan data yang sedang diubah.");

            if (excludeId.HasValue && await WouldCreateCircularReferenceAsync(excludeId.Value, normalizedParentStorageLocationId))
                return (false, "Parent storage location tidak valid karena membentuk relasi berulang.");

            if (normalizedParentStorageLocationId.HasValue)
            {
                var parentExists = await _dbContext.Set<MstDrugStorageLocation>()
                    .AnyAsync(x => x.Id == normalizedParentStorageLocationId.Value && x.IsActive && !x.IsDelete);

                if (!parentExists)
                    return (false, "Parent storage location tidak valid atau tidak aktif.");
            }

            if (normalizedServiceUnitId.HasValue)
            {
                var serviceUnitExists = await _dbContext.Set<MstServiceUnit>()
                    .AnyAsync(x => x.Id == normalizedServiceUnitId.Value && x.IsActive && !x.IsDelete);

                if (!serviceUnitExists)
                    return (false, "Service unit tidak valid atau tidak aktif.");
            }

            if (normalizedClinicId.HasValue)
            {
                var clinic = await _dbContext.Set<MstClinic>()
                    .AsNoTracking()
                    .Where(x => x.Id == normalizedClinicId.Value && x.IsActive && !x.IsDelete)
                    .Select(x => new { x.Id, x.ServiceUnitId })
                    .FirstOrDefaultAsync();

                if (clinic == null)
                    return (false, "Clinic tidak valid atau tidak aktif.");

                if (normalizedServiceUnitId.HasValue && clinic.ServiceUnitId != normalizedServiceUnitId.Value)
                    return (false, "Clinic tidak sesuai dengan service unit yang dipilih.");
            }

            if (normalizedRoomId.HasValue)
            {
                var room = await _dbContext.Set<MstRoom>()
                    .AsNoTracking()
                    .Where(x => x.Id == normalizedRoomId.Value && x.IsActive && !x.IsDelete)
                    .Select(x => new { x.Id, x.ServiceUnitId })
                    .FirstOrDefaultAsync();

                if (room == null)
                    return (false, "Room tidak valid atau tidak aktif.");

                if (normalizedServiceUnitId.HasValue && room.ServiceUnitId != normalizedServiceUnitId.Value)
                    return (false, "Room tidak sesuai dengan service unit yang dipilih.");
            }

            var normalizedName = storageLocationName.Trim().ToLower();

            var duplicateName = await _dbContext.Set<MstDrugStorageLocation>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.ParentStorageLocationId == normalizedParentStorageLocationId &&
                    x.StorageLocationName.ToLower() == normalizedName &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateName)
                return (false, "Nama storage location pada parent tersebut sudah digunakan.");

            return (true, null);
        }

        private async Task<bool> WouldCreateCircularReferenceAsync(Guid entityId, Guid? parentStorageLocationId)
        {
            var visited = new HashSet<Guid>();
            var currentParentId = NormalizeNullableGuid(parentStorageLocationId);

            while (currentParentId.HasValue)
            {
                if (currentParentId.Value == entityId)
                    return true;

                if (!visited.Add(currentParentId.Value))
                    return true;

                var parentIdValue = currentParentId.Value;

                currentParentId = await _dbContext.Set<MstDrugStorageLocation>()
                    .AsNoTracking()
                    .Where(x => x.Id == parentIdValue && !x.IsDelete)
                    .Select(x => x.ParentStorageLocationId)
                    .FirstOrDefaultAsync();
            }

            return false;
        }

        private async Task ResetDefaultStorageLocationAsync(Guid actorUserId, DateTime now)
        {
            var defaultLocations = await _dbContext.Set<MstDrugStorageLocation>()
                .Where(x => x.IsDefault && !x.IsDelete)
                .ToListAsync();

            foreach (var item in defaultLocations)
            {
                item.IsDefault = false;
                item.UpdateDateTime = now;
                item.UpdateBy = actorUserId;
            }
        }

        private async Task<string> GenerateStorageLocationCodeAsync()
        {
            var codes = await _dbContext.Set<MstDrugStorageLocation>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.StorageLocationCode.StartsWith(CodePrefix))
                .Select(x => x.StorageLocationCode)
                .ToListAsync();

            var usedNumbers = new HashSet<int>();

            foreach (var code in codes)
            {
                if (string.IsNullOrWhiteSpace(code) || code.Length <= CodePrefix.Length)
                    continue;

                var numberPart = code[CodePrefix.Length..];

                if (int.TryParse(numberPart, out var number) && number > 0)
                    usedNumbers.Add(number);
            }

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }

            return $"{CodePrefix}{nextNumber.ToString().PadLeft(CodeNumberLength, '0')}";
        }

        private static IQueryable<MstDrugStorageLocation> ApplySorting(
            IQueryable<MstDrugStorageLocation> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").ToLowerInvariant() switch
            {
                "createdatetime" => isDesc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "storagelocationcode" => isDesc
                    ? query.OrderByDescending(x => x.StorageLocationCode)
                    : query.OrderBy(x => x.StorageLocationCode),

                "storagelocationname" => isDesc
                    ? query.OrderByDescending(x => x.StorageLocationName)
                    : query.OrderBy(x => x.StorageLocationName),

                "storagelocationtype" => isDesc
                    ? query.OrderByDescending(x => x.StorageLocationType)
                    : query.OrderBy(x => x.StorageLocationType),

                "serviceunitname" => isDesc
                    ? query.OrderByDescending(x => x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty)
                    : query.OrderBy(x => x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty),

                "parentstoragelocationname" => isDesc
                    ? query.OrderByDescending(x => x.ParentStorageLocation != null ? x.ParentStorageLocation.StorageLocationName : string.Empty)
                    : query.OrderBy(x => x.ParentStorageLocation != null ? x.ParentStorageLocation.StorageLocationName : string.Empty),

                "locationgroupname" => isDesc
                    ? query.OrderByDescending(x => x.LocationGroupName)
                    : query.OrderBy(x => x.LocationGroupName),

                "isdefault" => isDesc
                    ? query.OrderByDescending(x => x.IsDefault)
                    : query.OrderBy(x => x.IsDefault),

                "ismainwarehouse" => isDesc
                    ? query.OrderByDescending(x => x.IsMainWarehouse)
                    : query.OrderBy(x => x.IsMainWarehouse),

                "ispharmacylocation" => isDesc
                    ? query.OrderByDescending(x => x.IsPharmacyLocation)
                    : query.OrderBy(x => x.IsPharmacyLocation),

                "iscoldchain" => isDesc
                    ? query.OrderByDescending(x => x.IsColdChain)
                    : query.OrderBy(x => x.IsColdChain),

                "isactive" => isDesc
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => isDesc
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.StorageLocationName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.StorageLocationName)
            };
        }

        private static DrugStorageLocationResponse ToResponse(MstDrugStorageLocation x)
        {
            return new DrugStorageLocationResponse
            {
                Id = x.Id,
                ParentStorageLocationId = x.ParentStorageLocationId,
                ParentStorageLocationCode = x.ParentStorageLocation != null ? x.ParentStorageLocation.StorageLocationCode : null,
                ParentStorageLocationName = x.ParentStorageLocation != null ? x.ParentStorageLocation.StorageLocationName : null,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitCode = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitCode : null,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                ClinicId = x.ClinicId,
                ClinicCode = x.Clinic != null ? x.Clinic.ClinicCode : null,
                ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                RoomId = x.RoomId,
                RoomCode = x.Room != null ? x.Room.RoomCode : null,
                MasterRoomName = x.Room != null ? x.Room.RoomName : null,
                StorageLocationCode = x.StorageLocationCode,
                StorageLocationName = x.StorageLocationName,
                StorageLocationType = x.StorageLocationType,
                LocationGroupName = x.LocationGroupName,
                FloorName = x.FloorName,
                RoomName = x.RoomName,
                RackCode = x.RackCode,
                ShelfCode = x.ShelfCode,
                BinCode = x.BinCode,
                MinimumTemperatureCelsius = x.MinimumTemperatureCelsius,
                MaximumTemperatureCelsius = x.MaximumTemperatureCelsius,
                MinimumHumidityPercent = x.MinimumHumidityPercent,
                MaximumHumidityPercent = x.MaximumHumidityPercent,
                IsDefault = x.IsDefault,
                IsMainWarehouse = x.IsMainWarehouse,
                IsPharmacyLocation = x.IsPharmacyLocation,
                IsColdChain = x.IsColdChain,
                IsControlledDrugStorage = x.IsControlledDrugStorage,
                IsHighAlertStorage = x.IsHighAlertStorage,
                IsQuarantineLocation = x.IsQuarantineLocation,
                IsAllowReceiving = x.IsAllowReceiving,
                IsAllowDispensing = x.IsAllowDispensing,
                IsAllowTransferIn = x.IsAllowTransferIn,
                IsAllowTransferOut = x.IsAllowTransferOut,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime,
                CreateBy = x.CreateBy == Guid.Empty ? null : (Guid?)x.CreateBy
            };
        }

        private static DrugStorageLocationCreateResponse ToCreateResponse(MstDrugStorageLocation x)
        {
            return new DrugStorageLocationCreateResponse
            {
                Id = x.Id,
                StorageLocationCode = x.StorageLocationCode,
                StorageLocationName = x.StorageLocationName,
                StorageLocationType = x.StorageLocationType,
                IsDefault = x.IsDefault,
                IsMainWarehouse = x.IsMainWarehouse,
                IsPharmacyLocation = x.IsPharmacyLocation,
                IsActive = x.IsActive
            };
        }

        private static DrugStorageLocationUpdateResponse ToUpdateResponse(MstDrugStorageLocation x)
        {
            return new DrugStorageLocationUpdateResponse
            {
                Id = x.Id,
                StorageLocationCode = x.StorageLocationCode,
                StorageLocationName = x.StorageLocationName,
                StorageLocationType = x.StorageLocationType,
                IsDefault = x.IsDefault,
                IsMainWarehouse = x.IsMainWarehouse,
                IsPharmacyLocation = x.IsPharmacyLocation,
                IsActive = x.IsActive,
                UpdateDateTime = x.UpdateDateTime
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static List<DrugStorageLocationCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<DrugStorageLocationCustomPeriodOptionResponse>
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "yesterday", Label = "Kemarin" },
                new() { Value = "last7Days", Label = "7 hari terakhir" },
                new() { Value = "last30Days", Label = "30 hari terakhir" },
                new() { Value = "thisMonth", Label = "Bulan ini" },
                new() { Value = "lastMonth", Label = "Bulan lalu" },
                new() { Value = "thisYear", Label = "Tahun ini" }
            };
        }

        private static DateRangeResult ResolveDateRange(DateTime? startDate, DateTime? endDate, string? customPeriod)
        {
            if (string.IsNullOrWhiteSpace(customPeriod))
            {
                return BuildDateRange(startDate, endDate);
            }

            var today = AppDateTimeHelper.OperationalDate();

            return customPeriod.Trim().ToLowerInvariant() switch
            {
                "today" => BuildDateRange(today, today),
                "yesterday" => BuildDateRange(today.AddDays(-1), today.AddDays(-1)),
                "last7days" => BuildDateRange(today.AddDays(-6), today),
                "last30days" => BuildDateRange(today.AddDays(-29), today),
                "thismonth" => BuildDateRange(new DateTime(today.Year, today.Month, 1), today),
                "lastmonth" => BuildDateRange(
                    new DateTime(today.Year, today.Month, 1).AddMonths(-1),
                    new DateTime(today.Year, today.Month, 1).AddDays(-1)),
                "thisyear" => BuildDateRange(new DateTime(today.Year, 1, 1), today),
                _ => new DateRangeResult
                {
                    IsValid = false,
                    ErrorMessage = "Custom period tidak valid."
                }
            };
        }

        private static DateRangeResult BuildDateRange(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue && endDate.Value.Date < startDate.Value.Date)
            {
                return new DateRangeResult
                {
                    IsValid = false,
                    ErrorMessage = "End date tidak boleh lebih kecil dari start date."
                };
            }

            return new DateRangeResult
            {
                IsValid = true,
                Start = startDate?.Date,
                EndExclusive = endDate?.Date.AddDays(1)
            };
        }

        private static string NormalizeStorageLocationType(string value)
        {
            var trimmed = value.Trim();

            var matched = AllowedStorageLocationTypes
                .FirstOrDefault(x => string.Equals(x, trimmed, StringComparison.OrdinalIgnoreCase));

            return matched ?? "General";
        }

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            if (!value.HasValue || value.Value == Guid.Empty)
                return null;

            return value.Value;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private Guid GetCurrentUserId()
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
        }

        private class DateRangeResult
        {
            public bool IsValid { get; set; }
            public string? ErrorMessage { get; set; }
            public DateTime? Start { get; set; }
            public DateTime? EndExclusive { get; set; }
        }
    }
}
