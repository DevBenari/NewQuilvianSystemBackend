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
using System.Security.Claims;

using ResponseClinicPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.ClinicResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/clinics")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Clinic",
        AreaName = "HealthServices",
        ControllerName = "Clinic",
        Description = "Health service master data clinic",
        SortOrder = 2
    )]
    [Tags("Health Services / Master Data / Clinic")]
    public class ClinicController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData";
        private const string KioskReadPolicy = "KioskRead";
        private const string ClinicCodePrefix = "CL-RSMMC-";
        private const int ClinicCodeDigitLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public ClinicController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [Authorize(Policy = KioskReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<ClinicFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Clinic", Description = "Melihat metadata filter clinic", AccessType = AccessTypes.Read, SortOrder = 1)]        
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new ClinicFilterMetadataResponse
            {
                DefaultFilter = new ClinicDefaultFilterResponse(),
                CustomPeriods = BuildCustomPeriodOptions(),
                SortOptions = new List<ClinicSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "clinicCode", Label = "Kode clinic" },
                    new() { Value = "clinicName", Label = "Nama clinic" },
                    new() { Value = "clinicType", Label = "Tipe clinic" },
                    new() { Value = "serviceUnitName", Label = "Nama service unit" },
                    new() { Value = "shortName", Label = "Nama singkat" },
                    new() { Value = "locationName", Label = "Lokasi" },
                    new() { Value = "roomName", Label = "Nama ruang" },
                    new() { Value = "defaultEstimatedServiceMinutes", Label = "Estimasi pelayanan" },
                    new() { Value = "isAvailableForRegistration", Label = "Tersedia registrasi" },
                    new() { Value = "isAvailableForKiosk", Label = "Tersedia kiosk" },
                    new() { Value = "isAvailableForAppointment", Label = "Tersedia appointment" },
                    new() { Value = "isDoctorRequired", Label = "Butuh dokter" },
                    new() { Value = "isScreeningRequired", Label = "Butuh screening" },
                    new() { Value = "isQueueRequired", Label = "Butuh antrian" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                ClinicTypeOptions = BuildEnumOptions<ClinicType>(),
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateFieldMetadata(),
                UpdateFields = BuildUpdateFieldMetadata(),
                ResetButtonLabel = "Reset"
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Clinic.GetFilterMetadata",
                "Mengambil metadata filter clinic.",
                result
            );

            return Ok(ApiResponse<ClinicFilterMetadataResponse>.Ok(
                result,
                "Metadata filter clinic berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<ClinicSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Clinic", Description = "Melihat ringkasan clinic", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Clinic", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();

            var result = new ClinicSummaryResponse
            {
                TotalClinic = await query.CountAsync(),
                ActiveClinic = await query.CountAsync(x => x.IsActive),
                InactiveClinic = await query.CountAsync(x => !x.IsActive),
                RegistrationAvailableClinic = await query.CountAsync(x => x.IsAvailableForRegistration),
                KioskAvailableClinic = await query.CountAsync(x => x.IsAvailableForKiosk),
                AppointmentAvailableClinic = await query.CountAsync(x => x.IsAvailableForAppointment),
                QueueRequiredClinic = await query.CountAsync(x => x.IsQueueRequired),
                DoctorRequiredClinic = await query.CountAsync(x => x.IsDoctorRequired),
                ScreeningRequiredClinic = await query.CountAsync(x => x.IsScreeningRequired)
            };

            return Ok(ApiResponse<ClinicSummaryResponse>.Ok(
                result,
                "Ringkasan clinic berhasil diambil."
            ));
        }

        [HttpGet]
        [Authorize(Policy = KioskReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<ResponseClinicPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Clinic", Description = "Melihat data clinic", AccessType = AccessTypes.Read, SortOrder = 1)]        
        public async Task<IActionResult> GetClinics(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] bool? isActive,
            [FromQuery] ClinicType? clinicType,
            [FromQuery] bool? isAvailableForRegistration,
            [FromQuery] bool? isAvailableForKiosk,
            [FromQuery] bool? isAvailableForAppointment,
            [FromQuery] bool? isDoctorRequired,
            [FromQuery] bool? isScreeningRequired,
            [FromQuery] bool? isQueueRequired,
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
            query = ApplyDateFilter(query, dateRange);
            query = ApplyStandardFilter(
                query,
                serviceUnitId,
                isActive,
                clinicType,
                isAvailableForRegistration,
                isAvailableForKiosk,
                isAvailableForAppointment,
                isDoctorRequired,
                isScreeningRequired,
                isQueueRequired,
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

            var result = new ResponseClinicPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseClinicPagedResult>.Ok(
                result,
                "Data clinic berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [Authorize(Policy = KioskReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<ClinicOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Clinic", Description = "Melihat data pilihan clinic", AccessType = AccessTypes.Read, SortOrder = 1)]        
        public async Task<IActionResult> GetClinicOptions(
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] ClinicType? clinicType,
            [FromQuery] bool? isAvailableForRegistration,
            [FromQuery] bool? isAvailableForKiosk,
            [FromQuery] bool? isAvailableForAppointment,
            [FromQuery] bool? isDoctorRequired,
            [FromQuery] bool? isScreeningRequired,
            [FromQuery] bool? isQueueRequired,
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
                serviceUnitId,
                useOnlyActive ? true : null,
                clinicType,
                isAvailableForRegistration,
                isAvailableForKiosk,
                isAvailableForAppointment,
                isDoctorRequired,
                isScreeningRequired,
                isQueueRequired,
                search
            );

            var totalData = await query.CountAsync();

            var entities = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ClinicName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities
                .Select(MapOptionResponse)
                .ToList();

            var result = new ClinicOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ClinicOptionPagedResponse>.Ok(
                result,
                "Data pilihan clinic berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ClinicDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Clinic", Description = "Melihat detail clinic", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Clinic", "Read")]
        public async Task<IActionResult> GetClinicById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Clinic tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var data = MapDetailResponse(entity, actorNames);

            return Ok(ApiResponse<ClinicDetailResponse>.Ok(
                data,
                "Detail clinic berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ClinicCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Clinic", Description = "Membuat data clinic", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("Clinic", "Create")]
        public async Task<IActionResult> CreateClinic([FromBody] CreateClinicRequest request)
        {
            var validation = await ValidateCreateUpdateRequestAsync(
                excludeId: null,
                serviceUnitId: request.ServiceUnitId,
                clinicName: request.ClinicName,
                clinicType: request.ClinicType,
                defaultEstimatedServiceMinutes: request.DefaultEstimatedServiceMinutes
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data clinic tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var generatedClinicCode = await GenerateClinicCodeAsync();
            var codeValidation = await ValidateGeneratedClinicCodeAsync(generatedClinicCode);

            if (!codeValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    codeValidation.ErrorMessage ?? "Kode clinic otomatis tidak valid."
                ));
            }

            var entity = new MstClinic
            {
                Id = Guid.NewGuid(),
                ServiceUnitId = request.ServiceUnitId,
                ClinicCode = generatedClinicCode,
                ClinicName = request.ClinicName.Trim(),
                ClinicType = request.ClinicType,
                ShortName = NormalizeNullableText(request.ShortName),
                LocationName = NormalizeNullableText(request.LocationName),
                FloorName = NormalizeNullableText(request.FloorName),
                RoomName = NormalizeNullableText(request.RoomName),
                IsAvailableForRegistration = request.IsAvailableForRegistration,
                IsAvailableForKiosk = request.IsAvailableForKiosk,
                IsAvailableForAppointment = request.IsAvailableForAppointment,
                IsDoctorRequired = request.IsDoctorRequired,
                IsScreeningRequired = request.IsScreeningRequired,
                IsQueueRequired = request.IsQueueRequired,
                DefaultEstimatedServiceMinutes = request.DefaultEstimatedServiceMinutes,
                SortOrder = request.SortOrder,
                Description = NormalizeNullableText(request.Description),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstClinic>().Add(entity);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            var actorNames = await GetActorNameMapAsync(new[] { entity.CreateBy });

            var response = new ClinicCreateResponse
            {
                Id = entity.Id,
                ServiceUnitId = entity.ServiceUnitId,
                ClinicCode = entity.ClinicCode,
                ClinicName = entity.ClinicName,
                ClinicType = entity.ClinicType,
                ClinicTypeName = BuildEnumLabel(entity.ClinicType),
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Clinic.CreateClinic",
                "Membuat data clinic.",
                response
            );

            return Ok(ApiResponse<ClinicCreateResponse>.Ok(
                response,
                "Clinic berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ClinicUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Clinic", Description = "Mengubah data clinic", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Clinic", "Update")]
        public async Task<IActionResult> UpdateClinic(Guid id, [FromBody] UpdateClinicRequest request)
        {
            var entity = await _dbContext.Set<MstClinic>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Clinic tidak ditemukan."
                ));
            }

            var validation = await ValidateCreateUpdateRequestAsync(
                excludeId: id,
                serviceUnitId: request.ServiceUnitId,
                clinicName: request.ClinicName,
                clinicType: request.ClinicType,
                defaultEstimatedServiceMinutes: request.DefaultEstimatedServiceMinutes
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data clinic tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.ServiceUnitId = request.ServiceUnitId;
            entity.ClinicName = request.ClinicName.Trim();
            entity.ClinicType = request.ClinicType;
            entity.ShortName = NormalizeNullableText(request.ShortName);
            entity.LocationName = NormalizeNullableText(request.LocationName);
            entity.FloorName = NormalizeNullableText(request.FloorName);
            entity.RoomName = NormalizeNullableText(request.RoomName);
            entity.IsAvailableForRegistration = request.IsAvailableForRegistration;
            entity.IsAvailableForKiosk = request.IsAvailableForKiosk;
            entity.IsAvailableForAppointment = request.IsAvailableForAppointment;
            entity.IsDoctorRequired = request.IsDoctorRequired;
            entity.IsScreeningRequired = request.IsScreeningRequired;
            entity.IsQueueRequired = request.IsQueueRequired;
            entity.DefaultEstimatedServiceMinutes = request.DefaultEstimatedServiceMinutes;
            entity.SortOrder = request.SortOrder;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { entity.UpdateBy });

            var response = new ClinicUpdateResponse
            {
                Id = entity.Id,
                ServiceUnitId = entity.ServiceUnitId,
                ClinicCode = entity.ClinicCode,
                ClinicName = entity.ClinicName,
                ClinicType = entity.ClinicType,
                ClinicTypeName = BuildEnumLabel(entity.ClinicType),
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Clinic.UpdateClinic",
                "Mengubah data clinic.",
                response
            );

            return Ok(ApiResponse<ClinicUpdateResponse>.Ok(
                response,
                "Clinic berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<ClinicUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Clinic Status", Description = "Mengubah status clinic", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Clinic", "Update")]
        public async Task<IActionResult> UpdateClinicStatus(Guid id, [FromBody] UpdateClinicStatusRequest request)
        {
            var entity = await _dbContext.Set<MstClinic>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Clinic tidak ditemukan."
                ));
            }

            if (request.IsActive)
            {
                var serviceUnitExists = await _dbContext.Set<MstServiceUnit>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == entity.ServiceUnitId && x.IsActive && !x.IsDelete);

                if (!serviceUnitExists)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Clinic tidak bisa diaktifkan karena service unit tidak aktif atau tidak valid."
                    ));
                }
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { entity.UpdateBy });

            var response = new ClinicUpdateResponse
            {
                Id = entity.Id,
                ServiceUnitId = entity.ServiceUnitId,
                ClinicCode = entity.ClinicCode,
                ClinicName = entity.ClinicName,
                ClinicType = entity.ClinicType,
                ClinicTypeName = BuildEnumLabel(entity.ClinicType),
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };

            return Ok(ApiResponse<ClinicUpdateResponse>.Ok(
                response,
                "Status clinic berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ClinicDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Clinic", Description = "Menghapus data clinic", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("Clinic", "Delete")]
        public async Task<IActionResult> DeleteClinic(Guid id, [FromBody] DeleteClinicRequest? request = null)
        {
            var entity = await _dbContext.Set<MstClinic>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Clinic tidak ditemukan."
                ));
            }

            var isUsedByTariff = await _dbContext.Set<MstTariff>()
                .AnyAsync(x => x.ClinicId == id && !x.IsDelete);

            if (isUsedByTariff)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Clinic tidak dapat dihapus karena sudah digunakan oleh tariff."
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

            var response = new ClinicDeleteResponse
            {
                Id = entity.Id,
                ClinicCode = entity.ClinicCode,
                ClinicName = entity.ClinicName,
                DeleteDateTime = entity.DeleteDateTime,
                DeleteBy = entity.DeleteBy == Guid.Empty ? null : (Guid?)entity.DeleteBy,
                DeleteByName = GetActorName(actorNames, entity.DeleteBy)
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Clinic.DeleteClinic",
                "Menghapus data clinic.",
                response
            );

            return Ok(ApiResponse<ClinicDeleteResponse>.Ok(
                response,
                "Clinic berhasil dihapus."
            ));
        }

        private IQueryable<MstClinic> BuildBaseQuery()
        {
            return _dbContext.Set<MstClinic>()
                .AsNoTracking()
                .Include(x => x.ServiceUnit)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstClinic> ApplyDateFilter(
            IQueryable<MstClinic> query,
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

        private static IQueryable<MstClinic> ApplyStandardFilter(
            IQueryable<MstClinic> query,
            Guid? serviceUnitId,
            bool? isActive,
            ClinicType? clinicType,
            bool? isAvailableForRegistration,
            bool? isAvailableForKiosk,
            bool? isAvailableForAppointment,
            bool? isDoctorRequired,
            bool? isScreeningRequired,
            bool? isQueueRequired,
            string? search)
        {
            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
            {
                query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (clinicType.HasValue)
            {
                query = query.Where(x => x.ClinicType == clinicType.Value);
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

            if (isDoctorRequired.HasValue)
            {
                query = query.Where(x => x.IsDoctorRequired == isDoctorRequired.Value);
            }

            if (isScreeningRequired.HasValue)
            {
                query = query.Where(x => x.IsScreeningRequired == isScreeningRequired.Value);
            }

            if (isQueueRequired.HasValue)
            {
                query = query.Where(x => x.IsQueueRequired == isQueueRequired.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ClinicCode.ToLower().Contains(keyword) ||
                    x.ClinicName.ToLower().Contains(keyword) ||
                    (x.ShortName != null && x.ShortName.ToLower().Contains(keyword)) ||
                    (x.LocationName != null && x.LocationName.ToLower().Contains(keyword)) ||
                    (x.FloorName != null && x.FloorName.ToLower().Contains(keyword)) ||
                    (x.RoomName != null && x.RoomName.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.ServiceUnit != null && x.ServiceUnit.ServiceUnitCode.ToLower().Contains(keyword)) ||
                    (x.ServiceUnit != null && x.ServiceUnit.ServiceUnitName.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<MstClinic> ApplySorting(
            IQueryable<MstClinic> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => isDesc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "cliniccode" => isDesc
                    ? query.OrderByDescending(x => x.ClinicCode)
                    : query.OrderBy(x => x.ClinicCode),

                "clinicname" => isDesc
                    ? query.OrderByDescending(x => x.ClinicName)
                    : query.OrderBy(x => x.ClinicName),

                "clinictype" => isDesc
                    ? query.OrderByDescending(x => x.ClinicType).ThenBy(x => x.ClinicName)
                    : query.OrderBy(x => x.ClinicType).ThenBy(x => x.ClinicName),

                "serviceunitname" => isDesc
                    ? query.OrderByDescending(x => x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty).ThenBy(x => x.ClinicName)
                    : query.OrderBy(x => x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty).ThenBy(x => x.ClinicName),

                "shortname" => isDesc
                    ? query.OrderByDescending(x => x.ShortName).ThenBy(x => x.ClinicName)
                    : query.OrderBy(x => x.ShortName).ThenBy(x => x.ClinicName),

                "locationname" => isDesc
                    ? query.OrderByDescending(x => x.LocationName).ThenBy(x => x.ClinicName)
                    : query.OrderBy(x => x.LocationName).ThenBy(x => x.ClinicName),

                "roomname" => isDesc
                    ? query.OrderByDescending(x => x.RoomName).ThenBy(x => x.ClinicName)
                    : query.OrderBy(x => x.RoomName).ThenBy(x => x.ClinicName),

                "defaultestimatedserviceminutes" => isDesc
                    ? query.OrderByDescending(x => x.DefaultEstimatedServiceMinutes).ThenBy(x => x.ClinicName)
                    : query.OrderBy(x => x.DefaultEstimatedServiceMinutes).ThenBy(x => x.ClinicName),

                "isavailableforregistration" => isDesc
                    ? query.OrderByDescending(x => x.IsAvailableForRegistration).ThenBy(x => x.ClinicName)
                    : query.OrderBy(x => x.IsAvailableForRegistration).ThenBy(x => x.ClinicName),

                "isavailableforkiosk" => isDesc
                    ? query.OrderByDescending(x => x.IsAvailableForKiosk).ThenBy(x => x.ClinicName)
                    : query.OrderBy(x => x.IsAvailableForKiosk).ThenBy(x => x.ClinicName),

                "isavailableforappointment" => isDesc
                    ? query.OrderByDescending(x => x.IsAvailableForAppointment).ThenBy(x => x.ClinicName)
                    : query.OrderBy(x => x.IsAvailableForAppointment).ThenBy(x => x.ClinicName),

                "isdoctorrequired" => isDesc
                    ? query.OrderByDescending(x => x.IsDoctorRequired).ThenBy(x => x.ClinicName)
                    : query.OrderBy(x => x.IsDoctorRequired).ThenBy(x => x.ClinicName),

                "isscreeningrequired" => isDesc
                    ? query.OrderByDescending(x => x.IsScreeningRequired).ThenBy(x => x.ClinicName)
                    : query.OrderBy(x => x.IsScreeningRequired).ThenBy(x => x.ClinicName),

                "isqueuerequired" => isDesc
                    ? query.OrderByDescending(x => x.IsQueueRequired).ThenBy(x => x.ClinicName)
                    : query.OrderBy(x => x.IsQueueRequired).ThenBy(x => x.ClinicName),

                "isactive" => isDesc
                    ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.ClinicName)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.ClinicName),

                _ => isDesc
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.ClinicName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.ClinicName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCreateUpdateRequestAsync(
            Guid? excludeId,
            Guid serviceUnitId,
            string clinicName,
            ClinicType clinicType,
            int defaultEstimatedServiceMinutes)
        {
            if (serviceUnitId == Guid.Empty)
            {
                return (false, "Service unit wajib dipilih.");
            }

            if (string.IsNullOrWhiteSpace(clinicName))
            {
                return (false, "Nama clinic wajib diisi.");
            }

            if (!Enum.IsDefined(typeof(ClinicType), clinicType))
            {
                return (false, "Tipe clinic tidak valid.");
            }

            if (defaultEstimatedServiceMinutes <= 0)
            {
                return (false, "Estimasi menit pelayanan harus lebih besar dari 0.");
            }

            if (defaultEstimatedServiceMinutes > 1440)
            {
                return (false, "Estimasi menit pelayanan tidak boleh lebih dari 1440 menit.");
            }

            var serviceUnitExists = await _dbContext.Set<MstServiceUnit>()
                .AnyAsync(x => x.Id == serviceUnitId && x.IsActive && !x.IsDelete);

            if (!serviceUnitExists)
            {
                return (false, "Service unit tidak valid atau tidak aktif.");
            }

            var normalizedName = clinicName.Trim().ToLower();

            var duplicateNameInServiceUnit = await _dbContext.Set<MstClinic>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.ServiceUnitId == serviceUnitId &&
                    x.ClinicName.ToLower() == normalizedName &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateNameInServiceUnit)
            {
                return (false, "Nama clinic pada service unit tersebut sudah digunakan.");
            }

            return (true, null);
        }

        private async Task<string> GenerateClinicCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstClinic>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.ClinicCode.StartsWith(ClinicCodePrefix))
                .Select(x => x.ClinicCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(ExtractClinicCodeNumber)
                .Where(x => x.HasValue && x.Value > 0)
                .Select(x => x!.Value)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }

            return ClinicCodePrefix + nextNumber.ToString().PadLeft(ClinicCodeDigitLength, '0');
        }

        private static int? ExtractClinicCodeNumber(string clinicCode)
        {
            if (string.IsNullOrWhiteSpace(clinicCode))
            {
                return null;
            }

            if (!clinicCode.StartsWith(ClinicCodePrefix, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var numberText = clinicCode[ClinicCodePrefix.Length..];

            return int.TryParse(numberText, out var number)
                ? number
                : null;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateGeneratedClinicCodeAsync(string clinicCode)
        {
            if (string.IsNullOrWhiteSpace(clinicCode))
            {
                return (false, "Kode clinic otomatis gagal dibuat.");
            }

            var normalizedCode = clinicCode.Trim().ToUpperInvariant();

            var duplicateCode = await _dbContext.Set<MstClinic>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.ClinicCode.ToUpper() == normalizedCode);

            if (duplicateCode)
            {
                return (false, "Kode clinic otomatis sudah digunakan. Silakan ulangi proses create.");
            }

            return (true, null);
        }

        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(IEnumerable<Guid> actorIds)
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

        private static ClinicResponse MapResponse(
            MstClinic entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new ClinicResponse
            {
                Id = entity.Id,
                ServiceUnitId = entity.ServiceUnitId,
                ServiceUnitCode = entity.ServiceUnit != null ? entity.ServiceUnit.ServiceUnitCode : string.Empty,
                ServiceUnitName = entity.ServiceUnit != null ? entity.ServiceUnit.ServiceUnitName : string.Empty,
                ClinicCode = entity.ClinicCode,
                ClinicName = entity.ClinicName,
                ClinicType = entity.ClinicType,
                ClinicTypeName = BuildEnumLabel(entity.ClinicType),
                ShortName = entity.ShortName,
                LocationName = entity.LocationName,
                FloorName = entity.FloorName,
                RoomName = entity.RoomName,
                IsAvailableForRegistration = entity.IsAvailableForRegistration,
                IsAvailableForKiosk = entity.IsAvailableForKiosk,
                IsAvailableForAppointment = entity.IsAvailableForAppointment,
                IsDoctorRequired = entity.IsDoctorRequired,
                IsScreeningRequired = entity.IsScreeningRequired,
                IsQueueRequired = entity.IsQueueRequired,
                DefaultEstimatedServiceMinutes = entity.DefaultEstimatedServiceMinutes,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private static ClinicDetailResponse MapDetailResponse(
            MstClinic entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new ClinicDetailResponse
            {
                Id = entity.Id,
                ServiceUnitId = entity.ServiceUnitId,
                ServiceUnitCode = entity.ServiceUnit != null ? entity.ServiceUnit.ServiceUnitCode : string.Empty,
                ServiceUnitName = entity.ServiceUnit != null ? entity.ServiceUnit.ServiceUnitName : string.Empty,
                ClinicCode = entity.ClinicCode,
                ClinicName = entity.ClinicName,
                ClinicType = entity.ClinicType,
                ClinicTypeName = BuildEnumLabel(entity.ClinicType),
                ShortName = entity.ShortName,
                LocationName = entity.LocationName,
                FloorName = entity.FloorName,
                RoomName = entity.RoomName,
                IsAvailableForRegistration = entity.IsAvailableForRegistration,
                IsAvailableForKiosk = entity.IsAvailableForKiosk,
                IsAvailableForAppointment = entity.IsAvailableForAppointment,
                IsDoctorRequired = entity.IsDoctorRequired,
                IsScreeningRequired = entity.IsScreeningRequired,
                IsQueueRequired = entity.IsQueueRequired,
                DefaultEstimatedServiceMinutes = entity.DefaultEstimatedServiceMinutes,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                Description = entity.Description,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static ClinicOptionResponse MapOptionResponse(MstClinic entity)
        {
            return new ClinicOptionResponse
            {
                Id = entity.Id,
                ServiceUnitId = entity.ServiceUnitId,
                ServiceUnitCode = entity.ServiceUnit != null ? entity.ServiceUnit.ServiceUnitCode : string.Empty,
                ServiceUnitName = entity.ServiceUnit != null ? entity.ServiceUnit.ServiceUnitName : string.Empty,
                ClinicCode = entity.ClinicCode,
                ClinicName = entity.ClinicName,
                ClinicType = entity.ClinicType,
                ClinicTypeName = BuildEnumLabel(entity.ClinicType),
                ShortName = entity.ShortName,
                LocationName = entity.LocationName,
                FloorName = entity.FloorName,
                RoomName = entity.RoomName,
                IsAvailableForRegistration = entity.IsAvailableForRegistration,
                IsAvailableForKiosk = entity.IsAvailableForKiosk,
                IsAvailableForAppointment = entity.IsAvailableForAppointment,
                IsDoctorRequired = entity.IsDoctorRequired,
                IsScreeningRequired = entity.IsScreeningRequired,
                IsQueueRequired = entity.IsQueueRequired,
                DefaultEstimatedServiceMinutes = entity.DefaultEstimatedServiceMinutes,
                SortOrder = entity.SortOrder
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

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static DateRangeResolveResult ResolveDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            var today = AppDateTimeHelper.OperationalDate();

            if (!string.IsNullOrWhiteSpace(customPeriod) &&
                !string.Equals(customPeriod, "custom", StringComparison.OrdinalIgnoreCase))
            {
                return customPeriod.Trim().ToLowerInvariant() switch
                {
                    "today" => DateRangeResolveResult.Valid(today, today.AddDays(1)),
                    "last7days" => DateRangeResolveResult.Valid(today.AddDays(-6), today.AddDays(1)),
                    "last30days" => DateRangeResolveResult.Valid(today.AddDays(-29), today.AddDays(1)),
                    "thismonth" => DateRangeResolveResult.Valid(new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1)),
                    "lastmonth" => ResolveLastMonth(today),
                    _ => DateRangeResolveResult.Invalid("Custom period tidak valid.")
                };
            }

            if (startDate.HasValue && endDate.HasValue && startDate.Value.Date > endDate.Value.Date)
            {
                return DateRangeResolveResult.Invalid("Start date tidak boleh lebih besar dari end date.");
            }

            return DateRangeResolveResult.Valid(startDate?.Date, endDate?.Date.AddDays(1));
        }

        private static DateRangeResolveResult ResolveLastMonth(DateTime today)
        {
            var thisMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            return DateRangeResolveResult.Valid(thisMonthStart.AddMonths(-1), thisMonthStart);
        }

        private static List<ClinicCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<ClinicCustomPeriodOptionResponse>
            {
                new() { Value = "custom", Label = "Custom", Description = "Gunakan startDate dan endDate.", UsesStartDate = true, UsesEndDate = true },
                new() { Value = "today", Label = "Hari ini", Description = "Data yang dibuat hari ini.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last7days", Label = "7 hari terakhir", Description = "Data yang dibuat dalam 7 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last30days", Label = "30 hari terakhir", Description = "Data yang dibuat dalam 30 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thismonth", Label = "Bulan ini", Description = "Data yang dibuat pada bulan berjalan.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "lastmonth", Label = "Bulan lalu", Description = "Data yang dibuat pada bulan sebelumnya.", UsesStartDate = false, UsesEndDate = false }
            };
        }

        private static List<ClinicEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(x => new ClinicEnumOptionResponse
                {
                    Value = Convert.ToInt32(x),
                    Name = x.ToString(),
                    Label = BuildEnumLabel(x)
                })
                .ToList();
        }

        private static string BuildEnumLabel<TEnum>(TEnum value) where TEnum : Enum
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

        private static List<ClinicQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<ClinicQueryParameterInfoResponse>
            {
                new() { Name = "startDate", Type = "date", Description = "Filter tanggal mulai berdasarkan CreateDateTime.", Example = "2026-06-01" },
                new() { Name = "endDate", Type = "date", Description = "Filter tanggal akhir berdasarkan CreateDateTime.", Example = "2026-06-30" },
                new() { Name = "customPeriod", Type = "string", Description = "Preset periode. Pilihan: custom, today, last7days, last30days, thismonth, lastmonth.", Example = "last7days" },
                new() { Name = "serviceUnitId", Type = "guid", Description = "Filter clinic berdasarkan service unit.", Example = "3fa85f64-5717-4562-b3fc-2c963f66afa6" },
                new() { Name = "clinicType", Type = "enum", Description = "Filter berdasarkan tipe clinic.", Example = "1" },
                new() { Name = "isActive", Type = "bool", Description = "Filter status aktif.", Example = "true" },
                new() { Name = "search", Type = "string", Description = "Cari berdasarkan kode, nama, nama singkat, lokasi, ruang, deskripsi, atau service unit.", Example = "Poli Umum" },
                new() { Name = "sortBy", Type = "string", Description = "Kolom sorting.", Example = "sortOrder" },
                new() { Name = "sortDirection", Type = "string", Description = "Arah sorting: asc atau desc.", Example = "asc" },
                new() { Name = "pageNumber", Type = "int", Description = "Nomor halaman.", Example = "1" },
                new() { Name = "pageSize", Type = "int", Description = "Jumlah data per halaman.", Example = "25" }
            };
        }

        private static List<ClinicFormFieldMetadataResponse> BuildCreateFieldMetadata()
        {
            return BuildFieldMetadata(isUpdate: false);
        }

        private static List<ClinicFormFieldMetadataResponse> BuildUpdateFieldMetadata()
        {
            return BuildFieldMetadata(isUpdate: true);
        }

        private static List<ClinicFormFieldMetadataResponse> BuildFieldMetadata(bool isUpdate)
        {
            var fields = new List<ClinicFormFieldMetadataResponse>
            {
                new() { Name = "clinicCode", Label = "Kode Clinic", Section = "Basic", InputType = "readonly", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "AutoGenerated", MaxLength = 50, Description = "Digenerate otomatis oleh sistem dengan format CL-RSMMC-00001.", Example = "CL-RSMMC-00001", SortOrder = 1 },
                new() { Name = "serviceUnitId", Label = "Service Unit", Section = "Basic", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", OptionsSource = "/api/v1/health-services/master-data/service-units/options", SortOrder = 2 },
                new() { Name = "clinicName", Label = "Nama Clinic", Section = "Basic", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 150, Example = "Poli Umum", SortOrder = 3 },
                new() { Name = "clinicType", Label = "Tipe Clinic", Section = "Basic", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", OptionsSource = "clinicTypeOptions", SortOrder = 4 },
                new() { Name = "shortName", Label = "Nama Singkat", Section = "Basic", InputType = "text", MaxLength = 50, Example = "UMUM", SortOrder = 5 },
                new() { Name = "locationName", Label = "Lokasi", Section = "Location", InputType = "text", MaxLength = 100, Example = "Gedung Rawat Jalan", SortOrder = 6 },
                new() { Name = "floorName", Label = "Lantai", Section = "Location", InputType = "text", MaxLength = 50, Example = "Lantai 1", SortOrder = 7 },
                new() { Name = "roomName", Label = "Ruang", Section = "Location", InputType = "text", MaxLength = 50, Example = "Ruang 101", SortOrder = 8 },
                new() { Name = "isAvailableForRegistration", Label = "Tersedia Untuk Registrasi", Section = "Rule", InputType = "switch", SortOrder = 9 },
                new() { Name = "isAvailableForKiosk", Label = "Tampil di Kiosk", Section = "Rule", InputType = "switch", SortOrder = 10 },
                new() { Name = "isAvailableForAppointment", Label = "Tersedia Untuk Appointment", Section = "Rule", InputType = "switch", SortOrder = 11 },
                new() { Name = "isDoctorRequired", Label = "Butuh Dokter", Section = "Rule", InputType = "switch", SortOrder = 12 },
                new() { Name = "isScreeningRequired", Label = "Butuh Screening", Section = "Rule", InputType = "switch", SortOrder = 13 },
                new() { Name = "isQueueRequired", Label = "Butuh Antrian", Section = "Rule", InputType = "switch", SortOrder = 14 },
                new() { Name = "defaultEstimatedServiceMinutes", Label = "Estimasi Pelayanan", Section = "Rule", InputType = "number", Description = "Estimasi durasi pelayanan dalam menit.", Example = "15", SortOrder = 15 },
                new() { Name = "sortOrder", Label = "Urutan", Section = "Display", InputType = "number", SortOrder = 16 },
                new() { Name = "description", Label = "Deskripsi", Section = "Additional", InputType = "textarea", MaxLength = 250, SortOrder = 17 }
            };

            if (isUpdate)
            {
                fields.Add(new ClinicFormFieldMetadataResponse
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
