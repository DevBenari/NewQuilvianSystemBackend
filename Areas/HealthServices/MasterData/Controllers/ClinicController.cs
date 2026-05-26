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
        [ProducesResponseType(typeof(ApiResponse<ClinicFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Clinic", Description = "Melihat data clinic", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Clinic", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new ClinicFilterMetadataResponse
            {
                DefaultFilter = new ClinicDefaultFilterResponse(),
                SortOptions = new List<ClinicSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "clinicCode", Label = "Kode clinic" },
                    new() { Value = "clinicName", Label = "Nama clinic" },
                    new() { Value = "serviceUnitName", Label = "Nama service unit" },
                    new() { Value = "clinicType", Label = "Tipe clinic" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                ClinicTypeOptions = BuildEnumOptions<ClinicType>()
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
        [AccessAction("Read", "Read Clinic", Description = "Melihat data clinic", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Clinic", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = _dbContext.Set<MstClinic>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new ClinicSummaryResponse
            {
                TotalClinic = await query.CountAsync(),
                ActiveClinic = await query.CountAsync(x => x.IsActive),
                InactiveClinic = await query.CountAsync(x => !x.IsActive),
                RegistrationAvailableClinic = await query.CountAsync(x => x.IsAvailableForRegistration),
                KioskAvailableClinic = await query.CountAsync(x => x.IsAvailableForKiosk),
                AppointmentAvailableClinic = await query.CountAsync(x => x.IsAvailableForAppointment),
                DoctorRequiredClinic = await query.CountAsync(x => x.IsDoctorRequired),
                ScreeningRequiredClinic = await query.CountAsync(x => x.IsScreeningRequired)
            };

            return Ok(ApiResponse<ClinicSummaryResponse>.Ok(
                result,
                "Ringkasan clinic berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseClinicPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Clinic", Description = "Melihat data clinic", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Clinic", "Read")]
        public async Task<IActionResult> GetClinics(
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] ClinicType? clinicType,
            [FromQuery] bool? isAvailableForRegistration,
            [FromQuery] bool? isAvailableForKiosk,
            [FromQuery] bool? isAvailableForAppointment,
            [FromQuery] string? sortBy = "sortOrder",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = _dbContext.Set<MstClinic>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

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

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);

            if (clinicType.HasValue)
                query = query.Where(x => x.ClinicType == clinicType.Value);

            if (isAvailableForRegistration.HasValue)
                query = query.Where(x => x.IsAvailableForRegistration == isAvailableForRegistration.Value);

            if (isAvailableForKiosk.HasValue)
                query = query.Where(x => x.IsAvailableForKiosk == isAvailableForKiosk.Value);

            if (isAvailableForAppointment.HasValue)
                query = query.Where(x => x.IsAvailableForAppointment == isAvailableForAppointment.Value);

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ClinicResponse
                {
                    Id = x.Id,
                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitCode = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitCode : string.Empty,
                    ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty,
                    ClinicCode = x.ClinicCode,
                    ClinicName = x.ClinicName,
                    ClinicType = x.ClinicType,
                    ShortName = x.ShortName,
                    LocationName = x.LocationName,
                    FloorName = x.FloorName,
                    RoomName = x.RoomName,
                    IsAvailableForRegistration = x.IsAvailableForRegistration,
                    IsAvailableForKiosk = x.IsAvailableForKiosk,
                    IsAvailableForAppointment = x.IsAvailableForAppointment,
                    IsDoctorRequired = x.IsDoctorRequired,
                    IsScreeningRequired = x.IsScreeningRequired,
                    IsQueueRequired = x.IsQueueRequired,
                    DefaultEstimatedServiceMinutes = x.DefaultEstimatedServiceMinutes,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

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
        [ProducesResponseType(typeof(ApiResponse<List<ClinicOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Clinic", Description = "Melihat data clinic", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Clinic", "Read")]
        public async Task<IActionResult> GetClinicOptions(
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] ClinicType? clinicType,
            [FromQuery] bool? isAvailableForRegistration,
            [FromQuery] bool? isAvailableForKiosk,
            [FromQuery] bool? isAvailableForAppointment,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = _dbContext.Set<MstClinic>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);

            if (clinicType.HasValue)
                query = query.Where(x => x.ClinicType == clinicType.Value);

            if (isAvailableForRegistration.HasValue)
                query = query.Where(x => x.IsAvailableForRegistration == isAvailableForRegistration.Value);

            if (isAvailableForKiosk.HasValue)
                query = query.Where(x => x.IsAvailableForKiosk == isAvailableForKiosk.Value);

            if (isAvailableForAppointment.HasValue)
                query = query.Where(x => x.IsAvailableForAppointment == isAvailableForAppointment.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ClinicCode.ToLower().Contains(keyword) ||
                    x.ClinicName.ToLower().Contains(keyword) ||
                    (x.ShortName != null && x.ShortName.ToLower().Contains(keyword)));
            }

            var data = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ClinicName)
                .Select(x => new ClinicOptionResponse
                {
                    Id = x.Id,
                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty,
                    ClinicCode = x.ClinicCode,
                    ClinicName = x.ClinicName,
                    ClinicType = x.ClinicType,
                    IsAvailableForRegistration = x.IsAvailableForRegistration,
                    IsAvailableForKiosk = x.IsAvailableForKiosk,
                    IsAvailableForAppointment = x.IsAvailableForAppointment
                })
                .ToListAsync();

            return Ok(ApiResponse<List<ClinicOptionResponse>>.Ok(
                data,
                "Data pilihan clinic berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ClinicDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Clinic", Description = "Melihat data clinic", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Clinic", "Read")]
        public async Task<IActionResult> GetClinicById(Guid id)
        {
            var data = await _dbContext.Set<MstClinic>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new ClinicDetailResponse
                {
                    Id = x.Id,
                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitCode = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitCode : string.Empty,
                    ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty,
                    ClinicCode = x.ClinicCode,
                    ClinicName = x.ClinicName,
                    ClinicType = x.ClinicType,
                    ShortName = x.ShortName,
                    LocationName = x.LocationName,
                    FloorName = x.FloorName,
                    RoomName = x.RoomName,
                    IsAvailableForRegistration = x.IsAvailableForRegistration,
                    IsAvailableForKiosk = x.IsAvailableForKiosk,
                    IsAvailableForAppointment = x.IsAvailableForAppointment,
                    IsDoctorRequired = x.IsDoctorRequired,
                    IsScreeningRequired = x.IsScreeningRequired,
                    IsQueueRequired = x.IsQueueRequired,
                    DefaultEstimatedServiceMinutes = x.DefaultEstimatedServiceMinutes,
                    SortOrder = x.SortOrder,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Clinic tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<ClinicDetailResponse>.Ok(
                data,
                "Detail clinic berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ClinicCreateResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create Clinic", Description = "Membuat data clinic", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("Clinic", "Create")]
        public async Task<IActionResult> CreateClinic([FromBody] CreateClinicRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                serviceUnitId: request.ServiceUnitId,
                clinicCode: request.ClinicCode,
                clinicName: request.ClinicName
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

            var entity = new MstClinic
            {
                Id = Guid.NewGuid(),
                ServiceUnitId = request.ServiceUnitId,
                ClinicCode = request.ClinicCode.Trim().ToUpperInvariant(),
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

            var response = new ClinicCreateResponse
            {
                Id = entity.Id,
                ServiceUnitId = entity.ServiceUnitId,
                ClinicCode = entity.ClinicCode,
                ClinicName = entity.ClinicName,
                ClinicType = entity.ClinicType,
                IsActive = entity.IsActive
            };

            return Ok(ApiResponse<ClinicCreateResponse>.Ok(
                response,
                "Clinic berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
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

            var validation = await ValidateRequestAsync(
                excludeId: id,
                serviceUnitId: request.ServiceUnitId,
                clinicCode: request.ClinicCode,
                clinicName: request.ClinicName
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data clinic tidak valid."
                ));
            }

            entity.ServiceUnitId = request.ServiceUnitId;
            entity.ClinicCode = request.ClinicCode.Trim().ToUpperInvariant();
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
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Clinic berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Clinic", Description = "Menghapus data clinic", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("Clinic", "Delete")]
        public async Task<IActionResult> DeleteClinic(Guid id)
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

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Clinic berhasil dihapus."
            ));
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            Guid serviceUnitId,
            string clinicCode,
            string clinicName)
        {
            if (serviceUnitId == Guid.Empty)
                return (false, "Service unit wajib dipilih.");

            if (string.IsNullOrWhiteSpace(clinicCode))
                return (false, "Kode clinic wajib diisi.");

            if (string.IsNullOrWhiteSpace(clinicName))
                return (false, "Nama clinic wajib diisi.");

            var serviceUnitExists = await _dbContext.Set<MstServiceUnit>()
                .AnyAsync(x => x.Id == serviceUnitId && x.IsActive && !x.IsDelete);

            if (!serviceUnitExists)
                return (false, "Service unit tidak valid atau tidak aktif.");

            var normalizedCode = clinicCode.Trim().ToUpperInvariant();
            var normalizedName = clinicName.Trim().ToLower();

            var duplicateCode = await _dbContext.Set<MstClinic>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.ClinicCode.ToUpper() == normalizedCode &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateCode)
                return (false, "Kode clinic sudah digunakan.");

            var duplicateNameInServiceUnit = await _dbContext.Set<MstClinic>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.ServiceUnitId == serviceUnitId &&
                    x.ClinicName.ToLower() == normalizedName &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateNameInServiceUnit)
                return (false, "Nama clinic pada service unit tersebut sudah digunakan.");

            return (true, null);
        }

        private static IQueryable<MstClinic> ApplySorting(
            IQueryable<MstClinic> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").ToLowerInvariant() switch
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

                "serviceunitname" => isDesc
                    ? query.OrderByDescending(x => x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : "")
                    : query.OrderBy(x => x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : ""),

                "clinictype" => isDesc
                    ? query.OrderByDescending(x => x.ClinicType)
                    : query.OrderBy(x => x.ClinicType),

                "isactive" => isDesc
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => isDesc
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.ClinicName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.ClinicName)
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static List<ClinicEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(x => new ClinicEnumOptionResponse
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

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}