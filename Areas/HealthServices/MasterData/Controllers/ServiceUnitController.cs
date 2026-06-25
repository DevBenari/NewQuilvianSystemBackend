using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Data;
using System.Globalization;
using System.Security.Claims;

using ResponseServiceUnitPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.ServiceUnitResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/service-units")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Service Unit",
        AreaName = "HealthServices",
        ControllerName = "ServiceUnit",
        Description = "Health service master data service unit",
        SortOrder = 1
    )]
    [Tags("Health Services / Master Data / Service Unit")]
    public class ServiceUnitController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData";
        private const string ServiceUnitCodePrefix = "SU-RSMMC-";
        private const int ServiceUnitCodeDigitLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public ServiceUnitController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<ServiceUnitFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Service Unit",
            Description = "Melihat metadata filter service unit",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("ServiceUnit", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new ServiceUnitFilterMetadataResponse
            {
                DefaultFilter = new ServiceUnitDefaultFilterResponse(),
                CustomPeriods = BuildCustomPeriodOptions(),
                SortOptions = new List<ServiceUnitSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "serviceUnitCode", Label = "Kode service unit" },
                    new() { Value = "serviceUnitName", Label = "Nama service unit" },
                    new() { Value = "serviceUnitType", Label = "Tipe service unit" },
                    new() { Value = "shortName", Label = "Nama singkat" },
                    new() { Value = "locationName", Label = "Lokasi" },
                    new() { Value = "floorName", Label = "Lantai" },
                    new() { Value = "isAvailableForRegistration", Label = "Tersedia registrasi" },
                    new() { Value = "isAvailableForKiosk", Label = "Tersedia kiosk" },
                    new() { Value = "isAvailableForAppointment", Label = "Tersedia appointment" },
                    new() { Value = "isQueueRequired", Label = "Butuh antrian" },
                    new() { Value = "isDoctorRequired", Label = "Butuh dokter" },
                    new() { Value = "isScreeningRequired", Label = "Butuh screening" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                ServiceUnitTypeOptions = BuildEnumOptions<ServiceUnitType>(),
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateFieldMetadata(),
                UpdateFields = BuildUpdateFieldMetadata(),
                ResetButtonLabel = "Reset"
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "ServiceUnit.GetFilterMetadata",
                "Mengambil metadata filter service unit.",
                result
            );

            return Ok(ApiResponse<ServiceUnitFilterMetadataResponse>.Ok(
                result,
                "Metadata filter service unit berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<ServiceUnitSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Service Unit",
            Description = "Melihat ringkasan service unit",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("ServiceUnit", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();

            var result = new ServiceUnitSummaryResponse
            {
                TotalServiceUnit = await query.CountAsync(),
                ActiveServiceUnit = await query.CountAsync(x => x.IsActive),
                InactiveServiceUnit = await query.CountAsync(x => !x.IsActive),
                RegistrationAvailableServiceUnit = await query.CountAsync(x => x.IsAvailableForRegistration),
                KioskAvailableServiceUnit = await query.CountAsync(x => x.IsAvailableForKiosk),
                AppointmentAvailableServiceUnit = await query.CountAsync(x => x.IsAvailableForAppointment),
                QueueRequiredServiceUnit = await query.CountAsync(x => x.IsQueueRequired),
                DoctorRequiredServiceUnit = await query.CountAsync(x => x.IsDoctorRequired),
                ScreeningRequiredServiceUnit = await query.CountAsync(x => x.IsScreeningRequired)
            };

            return Ok(ApiResponse<ServiceUnitSummaryResponse>.Ok(
                result,
                "Ringkasan service unit berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseServiceUnitPagedResult>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Service Unit",
            Description = "Melihat data service unit",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("ServiceUnit", "Read")]
        public async Task<IActionResult> GetServiceUnits(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] ServiceUnitType? serviceUnitType,
            [FromQuery] bool? isAvailableForRegistration,
            [FromQuery] bool? isAvailableForKiosk,
            [FromQuery] bool? isAvailableForAppointment,
            [FromQuery] bool? isQueueRequired,
            [FromQuery] bool? isDoctorRequired,
            [FromQuery] bool? isScreeningRequired,
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
                serviceUnitType,
                isAvailableForRegistration,
                isAvailableForKiosk,
                isAvailableForAppointment,
                isQueueRequired,
                isDoctorRequired,
                isScreeningRequired
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

            var result = new ResponseServiceUnitPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseServiceUnitPagedResult>.Ok(
                result,
                "Data service unit berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<ServiceUnitOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Service Unit",
            Description = "Melihat data pilihan service unit",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("ServiceUnit", "Read")]
        public async Task<IActionResult> GetServiceUnitOptions(
            [FromQuery] bool onlyActive = true,
            [FromQuery] ServiceUnitType? serviceUnitType = null,
            [FromQuery] bool? isAvailableForRegistration = null,
            [FromQuery] bool? isAvailableForKiosk = null,
            [FromQuery] bool? isAvailableForAppointment = null,
            [FromQuery] bool? isQueueRequired = null,
            [FromQuery] bool? isDoctorRequired = null,
            [FromQuery] bool? isScreeningRequired = null,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyStandardFilter(
                query,
                search,
                onlyActive ? true : null,
                serviceUnitType,
                isAvailableForRegistration,
                isAvailableForKiosk,
                isAvailableForAppointment,
                isQueueRequired,
                isDoctorRequired,
                isScreeningRequired
            );

            var totalData = await query.CountAsync();

            var optionEntities = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ServiceUnitName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = optionEntities
                .Select(x => new ServiceUnitOptionResponse
                {
                    Id = x.Id,
                    ServiceUnitCode = x.ServiceUnitCode,
                    ServiceUnitName = x.ServiceUnitName,
                    ServiceUnitType = x.ServiceUnitType,
                    ServiceUnitTypeName = BuildServiceUnitTypeLabel(x.ServiceUnitType),
                    ShortName = x.ShortName,
                    LocationName = x.LocationName,
                    FloorName = x.FloorName,
                    IsAvailableForRegistration = x.IsAvailableForRegistration,
                    IsAvailableForKiosk = x.IsAvailableForKiosk,
                    IsAvailableForAppointment = x.IsAvailableForAppointment,
                    IsQueueRequired = x.IsQueueRequired,
                    IsDoctorRequired = x.IsDoctorRequired,
                    IsScreeningRequired = x.IsScreeningRequired,
                    SortOrder = x.SortOrder
                })
                .ToList();

            var result = new ServiceUnitOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ServiceUnitOptionPagedResponse>.Ok(
                result,
                "Data pilihan service unit berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ServiceUnitDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Service Unit",
            Description = "Melihat detail service unit",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("ServiceUnit", "Read")]
        public async Task<IActionResult> GetServiceUnitById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Service unit tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var data = MapDetailResponse(entity, actorNames);

            return Ok(ApiResponse<ServiceUnitDetailResponse>.Ok(
                data,
                "Detail service unit berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ServiceUnitCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Create",
            "Create Service Unit",
            Description = "Membuat data service unit",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("ServiceUnit", "Create")]
        public async Task<IActionResult> CreateServiceUnit([FromBody] CreateServiceUnitRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                request: request
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data service unit tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var generatedServiceUnitCode = await GenerateServiceUnitCodeAsync();

            var entity = new MstServiceUnit
            {
                Id = Guid.NewGuid(),
                ServiceUnitCode = generatedServiceUnitCode,
                ServiceUnitName = request.ServiceUnitName.Trim(),
                ServiceUnitType = request.ServiceUnitType,
                ShortName = NormalizeNullableText(request.ShortName),
                LocationName = NormalizeNullableText(request.LocationName),
                FloorName = NormalizeNullableText(request.FloorName),
                IsAvailableForRegistration = request.IsAvailableForRegistration,
                IsAvailableForKiosk = request.IsAvailableForKiosk,
                IsAvailableForAppointment = request.IsAvailableForAppointment,
                IsQueueRequired = request.IsQueueRequired,
                IsDoctorRequired = request.IsDoctorRequired,
                IsScreeningRequired = request.IsScreeningRequired,
                SortOrder = request.SortOrder,
                Description = NormalizeNullableText(request.Description),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstServiceUnit>().Add(entity);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            var actorNames = await GetActorNameMapAsync(new[] { entity.CreateBy });

            var result = new ServiceUnitCreateResponse
            {
                Id = entity.Id,
                ServiceUnitCode = entity.ServiceUnitCode,
                ServiceUnitName = entity.ServiceUnitName,
                ServiceUnitType = entity.ServiceUnitType,
                ServiceUnitTypeName = BuildServiceUnitTypeLabel(entity.ServiceUnitType),
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "ServiceUnit.CreateServiceUnit",
                "Membuat data service unit.",
                result
            );

            return Ok(ApiResponse<ServiceUnitCreateResponse>.Ok(
                result,
                "Service unit berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ServiceUnitUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Service Unit",
            Description = "Mengubah data service unit",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("ServiceUnit", "Update")]
        public async Task<IActionResult> UpdateServiceUnit(
            Guid id,
            [FromBody] UpdateServiceUnitRequest request)
        {
            var entity = await _dbContext.Set<MstServiceUnit>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Service unit tidak ditemukan."
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
                    validation.ErrorMessage ?? "Data service unit tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.ServiceUnitName = request.ServiceUnitName.Trim();
            entity.ServiceUnitType = request.ServiceUnitType;
            entity.ShortName = NormalizeNullableText(request.ShortName);
            entity.LocationName = NormalizeNullableText(request.LocationName);
            entity.FloorName = NormalizeNullableText(request.FloorName);
            entity.IsAvailableForRegistration = request.IsAvailableForRegistration;
            entity.IsAvailableForKiosk = request.IsAvailableForKiosk;
            entity.IsAvailableForAppointment = request.IsAvailableForAppointment;
            entity.IsQueueRequired = request.IsQueueRequired;
            entity.IsDoctorRequired = request.IsDoctorRequired;
            entity.IsScreeningRequired = request.IsScreeningRequired;
            entity.SortOrder = request.SortOrder;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { entity.UpdateBy });

            var result = new ServiceUnitUpdateResponse
            {
                Id = entity.Id,
                ServiceUnitCode = entity.ServiceUnitCode,
                ServiceUnitName = entity.ServiceUnitName,
                ServiceUnitType = entity.ServiceUnitType,
                ServiceUnitTypeName = BuildServiceUnitTypeLabel(entity.ServiceUnitType),
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "ServiceUnit.UpdateServiceUnit",
                "Mengubah data service unit.",
                result
            );

            return Ok(ApiResponse<ServiceUnitUpdateResponse>.Ok(
                result,
                "Service unit berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<ServiceUnitUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Service Unit Status",
            Description = "Mengubah status service unit",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("ServiceUnit", "Update")]
        public async Task<IActionResult> UpdateServiceUnitStatus(
            Guid id,
            [FromBody] UpdateServiceUnitStatusRequest request)
        {
            var entity = await _dbContext.Set<MstServiceUnit>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Service unit tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { entity.UpdateBy });

            var result = new ServiceUnitUpdateResponse
            {
                Id = entity.Id,
                ServiceUnitCode = entity.ServiceUnitCode,
                ServiceUnitName = entity.ServiceUnitName,
                ServiceUnitType = entity.ServiceUnitType,
                ServiceUnitTypeName = BuildServiceUnitTypeLabel(entity.ServiceUnitType),
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };

            return Ok(ApiResponse<ServiceUnitUpdateResponse>.Ok(
                result,
                "Status service unit berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ServiceUnitDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Service Unit",
            Description = "Menghapus data service unit",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("ServiceUnit", "Delete")]
        public async Task<IActionResult> DeleteServiceUnit(
            Guid id,
            [FromBody] DeleteServiceUnitRequest? request = null)
        {
            var entity = await _dbContext.Set<MstServiceUnit>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Service unit tidak ditemukan."
                ));
            }

            var isUsedByClinic = await _dbContext.Set<MstClinic>()
                .AnyAsync(x => x.ServiceUnitId == id && !x.IsDelete);

            if (isUsedByClinic)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Service unit tidak dapat dihapus karena sudah digunakan oleh clinic."
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

            var result = new ServiceUnitDeleteResponse
            {
                Id = entity.Id,
                ServiceUnitCode = entity.ServiceUnitCode,
                ServiceUnitName = entity.ServiceUnitName,
                DeleteDateTime = entity.DeleteDateTime,
                DeleteBy = entity.DeleteBy == Guid.Empty ? null : (Guid?)entity.DeleteBy,
                DeleteByName = GetActorName(actorNames, entity.DeleteBy)
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "ServiceUnit.DeleteServiceUnit",
                "Menghapus data service unit.",
                result
            );

            return Ok(ApiResponse<ServiceUnitDeleteResponse>.Ok(
                result,
                "Service unit berhasil dihapus."
            ));
        }

        private IQueryable<MstServiceUnit> BuildBaseQuery()
        {
            return _dbContext.Set<MstServiceUnit>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstServiceUnit> ApplyDateFilter(
            IQueryable<MstServiceUnit> query,
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

        private static IQueryable<MstServiceUnit> ApplyStandardFilter(
            IQueryable<MstServiceUnit> query,
            string? search,
            bool? isActive,
            ServiceUnitType? serviceUnitType,
            bool? isAvailableForRegistration,
            bool? isAvailableForKiosk,
            bool? isAvailableForAppointment,
            bool? isQueueRequired,
            bool? isDoctorRequired,
            bool? isScreeningRequired)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                var matchedTypes = Enum.GetValues(typeof(ServiceUnitType))
                    .Cast<ServiceUnitType>()
                    .Where(x =>
                        x.ToString().ToLower().Contains(keyword) ||
                        BuildServiceUnitTypeLabel(x).ToLower().Contains(keyword))
                    .ToList();

                query = query.Where(x =>
                    x.ServiceUnitCode.ToLower().Contains(keyword) ||
                    x.ServiceUnitName.ToLower().Contains(keyword) ||
                    (x.ShortName != null && x.ShortName.ToLower().Contains(keyword)) ||
                    (x.LocationName != null && x.LocationName.ToLower().Contains(keyword)) ||
                    (x.FloorName != null && x.FloorName.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    matchedTypes.Contains(x.ServiceUnitType));
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (serviceUnitType.HasValue)
            {
                query = query.Where(x => x.ServiceUnitType == serviceUnitType.Value);
            }

            if (isAvailableForRegistration.HasValue)
            {
                query = query.Where(x => x.IsAvailableForRegistration == isAvailableForRegistration.Value);
            }

            if (isAvailableForKiosk.HasValue)
            {
                query = query.Where(x => x.IsAvailableForKiosk == isAvailableForKiosk.Value);
            }

            if (isAvailableForAppointment.HasValue)
            {
                query = query.Where(x => x.IsAvailableForAppointment == isAvailableForAppointment.Value);
            }

            if (isQueueRequired.HasValue)
            {
                query = query.Where(x => x.IsQueueRequired == isQueueRequired.Value);
            }

            if (isDoctorRequired.HasValue)
            {
                query = query.Where(x => x.IsDoctorRequired == isDoctorRequired.Value);
            }

            if (isScreeningRequired.HasValue)
            {
                query = query.Where(x => x.IsScreeningRequired == isScreeningRequired.Value);
            }

            return query;
        }

        private static IOrderedQueryable<MstServiceUnit> ApplySorting(
            IQueryable<MstServiceUnit> query,
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

                "serviceunitcode" => isDescending
                    ? query.OrderByDescending(x => x.ServiceUnitCode)
                    : query.OrderBy(x => x.ServiceUnitCode),

                "serviceunitname" => isDescending
                    ? query.OrderByDescending(x => x.ServiceUnitName)
                    : query.OrderBy(x => x.ServiceUnitName),

                "serviceunittype" => isDescending
                    ? query.OrderByDescending(x => x.ServiceUnitType).ThenBy(x => x.ServiceUnitName)
                    : query.OrderBy(x => x.ServiceUnitType).ThenBy(x => x.ServiceUnitName),

                "shortname" => isDescending
                    ? query.OrderByDescending(x => x.ShortName).ThenBy(x => x.ServiceUnitName)
                    : query.OrderBy(x => x.ShortName).ThenBy(x => x.ServiceUnitName),

                "locationname" => isDescending
                    ? query.OrderByDescending(x => x.LocationName).ThenBy(x => x.ServiceUnitName)
                    : query.OrderBy(x => x.LocationName).ThenBy(x => x.ServiceUnitName),

                "floorname" => isDescending
                    ? query.OrderByDescending(x => x.FloorName).ThenBy(x => x.ServiceUnitName)
                    : query.OrderBy(x => x.FloorName).ThenBy(x => x.ServiceUnitName),

                "isavailableforregistration" => isDescending
                    ? query.OrderByDescending(x => x.IsAvailableForRegistration).ThenBy(x => x.ServiceUnitName)
                    : query.OrderBy(x => x.IsAvailableForRegistration).ThenBy(x => x.ServiceUnitName),

                "isavailableforkiosk" => isDescending
                    ? query.OrderByDescending(x => x.IsAvailableForKiosk).ThenBy(x => x.ServiceUnitName)
                    : query.OrderBy(x => x.IsAvailableForKiosk).ThenBy(x => x.ServiceUnitName),

                "isavailableforappointment" => isDescending
                    ? query.OrderByDescending(x => x.IsAvailableForAppointment).ThenBy(x => x.ServiceUnitName)
                    : query.OrderBy(x => x.IsAvailableForAppointment).ThenBy(x => x.ServiceUnitName),

                "isqueuerequired" => isDescending
                    ? query.OrderByDescending(x => x.IsQueueRequired).ThenBy(x => x.ServiceUnitName)
                    : query.OrderBy(x => x.IsQueueRequired).ThenBy(x => x.ServiceUnitName),

                "isdoctorrequired" => isDescending
                    ? query.OrderByDescending(x => x.IsDoctorRequired).ThenBy(x => x.ServiceUnitName)
                    : query.OrderBy(x => x.IsDoctorRequired).ThenBy(x => x.ServiceUnitName),

                "isscreeningrequired" => isDescending
                    ? query.OrderByDescending(x => x.IsScreeningRequired).ThenBy(x => x.ServiceUnitName)
                    : query.OrderBy(x => x.IsScreeningRequired).ThenBy(x => x.ServiceUnitName),

                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.ServiceUnitName)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.ServiceUnitName),

                _ => isDescending
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.ServiceUnitName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.ServiceUnitName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateServiceUnitRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ServiceUnitName))
            {
                return (false, "Nama service unit wajib diisi.");
            }

            if (!Enum.IsDefined(typeof(ServiceUnitType), request.ServiceUnitType))
            {
                return (false, "Tipe service unit tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            }

            if (request.SortOrder < 0)
            {
                return (false, "Urutan tidak boleh kurang dari 0.");
            }

            var normalizedName = request.ServiceUnitName.Trim().ToLower();

            var duplicateNameQuery = _dbContext.Set<MstServiceUnit>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.ServiceUnitName.ToLower() == normalizedName);

            if (excludeId.HasValue)
            {
                duplicateNameQuery = duplicateNameQuery.Where(x => x.Id != excludeId.Value);
            }

            if (await duplicateNameQuery.AnyAsync())
            {
                return (false, "Nama service unit sudah digunakan.");
            }

            var shortName = NormalizeNullableText(request.ShortName);

            if (!string.IsNullOrWhiteSpace(shortName))
            {
                var normalizedShortName = shortName.ToLower();

                var duplicateShortNameQuery = _dbContext.Set<MstServiceUnit>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.ShortName != null &&
                        x.ShortName.ToLower() == normalizedShortName);

                if (excludeId.HasValue)
                {
                    duplicateShortNameQuery = duplicateShortNameQuery.Where(x => x.Id != excludeId.Value);
                }

                if (await duplicateShortNameQuery.AnyAsync())
                {
                    return (false, "Nama singkat service unit sudah digunakan.");
                }
            }

            return (true, null);
        }

        private async Task<string> GenerateServiceUnitCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstServiceUnit>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.ServiceUnitCode.StartsWith(ServiceUnitCodePrefix))
                .Select(x => x.ServiceUnitCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(TryExtractServiceUnitSequenceNumber)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Where(x => x > 0)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }

            return ServiceUnitCodePrefix + nextNumber.ToString("D" + ServiceUnitCodeDigitLength, CultureInfo.InvariantCulture);
        }

        private static int? TryExtractServiceUnitSequenceNumber(string serviceUnitCode)
        {
            if (string.IsNullOrWhiteSpace(serviceUnitCode))
            {
                return null;
            }

            if (!serviceUnitCode.StartsWith(ServiceUnitCodePrefix, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var numberText = serviceUnitCode[ServiceUnitCodePrefix.Length..];

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

        private static ServiceUnitResponse MapResponse(
            MstServiceUnit entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new ServiceUnitResponse
            {
                Id = entity.Id,
                ServiceUnitCode = entity.ServiceUnitCode,
                ServiceUnitName = entity.ServiceUnitName,
                ServiceUnitType = entity.ServiceUnitType,
                ServiceUnitTypeName = BuildServiceUnitTypeLabel(entity.ServiceUnitType),
                ShortName = entity.ShortName,
                LocationName = entity.LocationName,
                FloorName = entity.FloorName,
                IsAvailableForRegistration = entity.IsAvailableForRegistration,
                IsAvailableForKiosk = entity.IsAvailableForKiosk,
                IsAvailableForAppointment = entity.IsAvailableForAppointment,
                IsQueueRequired = entity.IsQueueRequired,
                IsDoctorRequired = entity.IsDoctorRequired,
                IsScreeningRequired = entity.IsScreeningRequired,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private static ServiceUnitDetailResponse MapDetailResponse(
            MstServiceUnit entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new ServiceUnitDetailResponse
            {
                Id = entity.Id,
                ServiceUnitCode = entity.ServiceUnitCode,
                ServiceUnitName = entity.ServiceUnitName,
                ServiceUnitType = entity.ServiceUnitType,
                ServiceUnitTypeName = BuildServiceUnitTypeLabel(entity.ServiceUnitType),
                ShortName = entity.ShortName,
                LocationName = entity.LocationName,
                FloorName = entity.FloorName,
                IsAvailableForRegistration = entity.IsAvailableForRegistration,
                IsAvailableForKiosk = entity.IsAvailableForKiosk,
                IsAvailableForAppointment = entity.IsAvailableForAppointment,
                IsQueueRequired = entity.IsQueueRequired,
                IsDoctorRequired = entity.IsDoctorRequired,
                IsScreeningRequired = entity.IsScreeningRequired,
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
            var today = AppDateTimeHelper.OperationalDate();

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

        private static List<ServiceUnitCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<ServiceUnitCustomPeriodOptionResponse>
            {
                new() { Value = "custom", Label = "Custom", Description = "Gunakan startDate dan endDate.", UsesStartDate = true, UsesEndDate = true },
                new() { Value = "today", Label = "Hari ini", Description = "Data yang dibuat hari ini.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last7days", Label = "7 hari terakhir", Description = "Data yang dibuat dalam 7 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last30days", Label = "30 hari terakhir", Description = "Data yang dibuat dalam 30 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thismonth", Label = "Bulan ini", Description = "Data yang dibuat pada bulan berjalan.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "lastmonth", Label = "Bulan lalu", Description = "Data yang dibuat pada bulan sebelumnya.", UsesStartDate = false, UsesEndDate = false }
            };
        }

        private static List<ServiceUnitEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(x => new ServiceUnitEnumOptionResponse
                {
                    Value = Convert.ToInt32(x),
                    Name = x.ToString(),
                    Label = SplitPascalCase(x.ToString())
                })
                .ToList();
        }

        private static string BuildServiceUnitTypeLabel(ServiceUnitType value)
        {
            return SplitPascalCase(value.ToString());
        }

        private static string SplitPascalCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return string.Concat(value.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
        }

        private static List<ServiceUnitQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<ServiceUnitQueryParameterInfoResponse>
            {
                new() { Name = "startDate", Type = "DateTime?", Description = "Tanggal awal filter berdasarkan CreateDateTime.", Example = "2026-06-01" },
                new() { Name = "endDate", Type = "DateTime?", Description = "Tanggal akhir filter berdasarkan CreateDateTime.", Example = "2026-06-30" },
                new() { Name = "customPeriod", Type = "string", Description = "Filter periode cepat: custom, today, last7days, last30days, thismonth, lastmonth.", Example = "thismonth" },
                new() { Name = "search", Type = "string", Description = "Cari berdasarkan kode, nama, nama singkat, lokasi, lantai, tipe, atau deskripsi.", Example = "Rawat Jalan" },
                new() { Name = "serviceUnitType", Type = "enum", Description = "Filter berdasarkan tipe service unit.", Example = "1" },
                new() { Name = "isActive", Type = "bool", Description = "Filter status aktif.", Example = "true" },
                new() { Name = "isAvailableForRegistration", Type = "bool", Description = "Filter tersedia untuk registrasi.", Example = "true" },
                new() { Name = "isAvailableForKiosk", Type = "bool", Description = "Filter tersedia untuk kiosk.", Example = "true" },
                new() { Name = "isAvailableForAppointment", Type = "bool", Description = "Filter tersedia untuk appointment.", Example = "true" },
                new() { Name = "isQueueRequired", Type = "bool", Description = "Filter membutuhkan antrian.", Example = "true" },
                new() { Name = "isDoctorRequired", Type = "bool", Description = "Filter membutuhkan dokter.", Example = "true" },
                new() { Name = "isScreeningRequired", Type = "bool", Description = "Filter membutuhkan screening.", Example = "true" },
                new() { Name = "sortBy", Type = "string", Description = "Kolom sorting.", Example = "sortOrder" },
                new() { Name = "sortDirection", Type = "string", Description = "Arah sorting: asc atau desc.", Example = "asc" },
                new() { Name = "pageNumber", Type = "int", Description = "Nomor halaman.", Example = "1" },
                new() { Name = "pageSize", Type = "int", Description = "Jumlah data per halaman, maksimal 100.", Example = "25" }
            };
        }

        private static List<ServiceUnitFormFieldMetadataResponse> BuildCreateFieldMetadata()
        {
            return BuildFieldMetadata(isUpdate: false);
        }

        private static List<ServiceUnitFormFieldMetadataResponse> BuildUpdateFieldMetadata()
        {
            return BuildFieldMetadata(isUpdate: true);
        }

        private static List<ServiceUnitFormFieldMetadataResponse> BuildFieldMetadata(bool isUpdate)
        {
            var fields = new List<ServiceUnitFormFieldMetadataResponse>
            {
                new() { Name = "serviceUnitCode", Label = "Kode Service Unit", Section = "Basic", InputType = "readonly", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "AutoGenerated", MaxLength = 50, Description = "Digenerate otomatis oleh sistem dengan format SU-RSMMC-00001.", Example = "SU-RSMMC-00001", SortOrder = 1 },
                new() { Name = "serviceUnitName", Label = "Nama Service Unit", Section = "Basic", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 150, Example = "Rawat Jalan", SortOrder = 2 },
                new() { Name = "serviceUnitType", Label = "Tipe Service Unit", Section = "Basic", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", OptionsSource = "serviceUnitTypeOptions", SortOrder = 3 },
                new() { Name = "shortName", Label = "Nama Singkat", Section = "Basic", InputType = "text", MaxLength = 50, Example = "RAJAL", SortOrder = 4 },
                new() { Name = "locationName", Label = "Lokasi", Section = "Location", InputType = "text", MaxLength = 100, Example = "Gedung Rawat Jalan", SortOrder = 5 },
                new() { Name = "floorName", Label = "Lantai", Section = "Location", InputType = "text", MaxLength = 50, Example = "Lantai 1", SortOrder = 6 },
                new() { Name = "isAvailableForRegistration", Label = "Tersedia Untuk Registrasi", Section = "Rule", InputType = "switch", SortOrder = 7 },
                new() { Name = "isAvailableForKiosk", Label = "Tampil di Kiosk", Section = "Rule", InputType = "switch", SortOrder = 8 },
                new() { Name = "isAvailableForAppointment", Label = "Tersedia Untuk Appointment", Section = "Rule", InputType = "switch", SortOrder = 9 },
                new() { Name = "isQueueRequired", Label = "Butuh Antrian", Section = "Rule", InputType = "switch", SortOrder = 10 },
                new() { Name = "isDoctorRequired", Label = "Butuh Dokter", Section = "Rule", InputType = "switch", SortOrder = 11 },
                new() { Name = "isScreeningRequired", Label = "Butuh Screening", Section = "Rule", InputType = "switch", SortOrder = 12 },
                new() { Name = "sortOrder", Label = "Urutan", Section = "Display", InputType = "number", SortOrder = 13 },
                new() { Name = "description", Label = "Deskripsi", Section = "Additional", InputType = "textarea", MaxLength = 250, SortOrder = 14 }
            };

            if (isUpdate)
            {
                fields.Add(new ServiceUnitFormFieldMetadataResponse
                {
                    Name = "isActive",
                    Label = "Status Aktif",
                    Section = "Status",
                    InputType = "switch",
                    SortOrder = 99
                });
            }

            return fields.OrderBy(x => x.SortOrder).ToList();
        }

        private Guid GetCurrentUserId()
        {
            var userIdText =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("user_id");

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
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
