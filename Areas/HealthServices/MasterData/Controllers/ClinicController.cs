using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
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
        [ProducesResponseType(typeof(ApiResponse<ClinicFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Clinic", Description = "Melihat data clinic", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Clinic", "Read")]
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
                    new() { Value = "serviceUnitName", Label = "Nama service unit" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                ClinicTypeOptions = BuildEnumOptions<ClinicType>(),
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateFieldMetadata(),
                UpdateFields = BuildUpdateFieldMetadata()
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
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
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

            var query = _dbContext.Set<MstClinic>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (dateRange.Start.HasValue)
            {
                query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);
            }

            if (dateRange.EndExclusive.HasValue)
            {
                query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);
            }

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

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
        [ProducesResponseType(typeof(ApiResponse<ClinicOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Clinic", Description = "Melihat data pilihan clinic", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Clinic", "Read")]
        public async Task<IActionResult> GetClinicOptions(
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = _dbContext.Set<MstClinic>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);

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
                    (x.ServiceUnit != null && x.ServiceUnit.ServiceUnitCode.ToLower().Contains(keyword)) ||
                    (x.ServiceUnit != null && x.ServiceUnit.ServiceUnitName.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ClinicName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ClinicOptionResponse
                {
                    Id = x.Id,
                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitName = x.ServiceUnit != null
                        ? x.ServiceUnit.ServiceUnitName
                        : string.Empty,
                    ClinicCode = x.ClinicCode,
                    ClinicName = x.ClinicName,
                    ClinicType = x.ClinicType,
                    ShortName = x.ShortName,
                    IsAvailableForRegistration = x.IsAvailableForRegistration,
                    IsAvailableForKiosk = x.IsAvailableForKiosk,
                    IsAvailableForAppointment = x.IsAvailableForAppointment
                })
                .ToListAsync();

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
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Clinic", Description = "Membuat data clinic", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("Clinic", "Create")]
        public async Task<IActionResult> CreateClinic([FromBody] CreateClinicRequest request)
        {
            var validation = await ValidateCreateUpdateRequestAsync(
                excludeId: null,
                serviceUnitId: request.ServiceUnitId,
                clinicName: request.ClinicName,
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

            var response = new ClinicCreateResponse
            {
                Id = entity.Id,
                ServiceUnitId = entity.ServiceUnitId,
                ClinicCode = entity.ClinicCode,
                ClinicName = entity.ClinicName,
                ClinicType = entity.ClinicType,
                IsActive = entity.IsActive
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

            var response = new ClinicUpdateResponse
            {
                Id = entity.Id,
                ServiceUnitId = entity.ServiceUnitId,
                ClinicCode = entity.ClinicCode,
                ClinicName = entity.ClinicName,
                ClinicType = entity.ClinicType,
                IsActive = entity.IsActive
            };

            return Ok(ApiResponse<ClinicUpdateResponse>.Ok(
                response,
                "Clinic berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
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

            return $"{ClinicCodePrefix}{nextNumber.ToString().PadLeft(ClinicCodeDigitLength, '0')}";
        }

        private static int? ExtractClinicCodeNumber(string clinicCode)
        {
            if (string.IsNullOrWhiteSpace(clinicCode))
                return null;

            if (!clinicCode.StartsWith(ClinicCodePrefix, StringComparison.OrdinalIgnoreCase))
                return null;

            var numberText = clinicCode[ClinicCodePrefix.Length..];

            return int.TryParse(numberText, out var number)
                ? number
                : null;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCreateUpdateRequestAsync(
            Guid? excludeId,
            Guid serviceUnitId,
            string clinicName,
            int defaultEstimatedServiceMinutes)
        {
            if (serviceUnitId == Guid.Empty)
                return (false, "Service unit wajib dipilih.");

            if (string.IsNullOrWhiteSpace(clinicName))
                return (false, "Nama clinic wajib diisi.");

            if (defaultEstimatedServiceMinutes <= 0)
                return (false, "Estimasi menit pelayanan harus lebih besar dari 0.");

            if (defaultEstimatedServiceMinutes > 1440)
                return (false, "Estimasi menit pelayanan tidak boleh lebih dari 1440 menit.");

            var serviceUnitExists = await _dbContext.Set<MstServiceUnit>()
                .AnyAsync(x => x.Id == serviceUnitId && x.IsActive && !x.IsDelete);

            if (!serviceUnitExists)
                return (false, "Service unit tidak valid atau tidak aktif.");

            var normalizedName = clinicName.Trim().ToLower();

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

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateGeneratedClinicCodeAsync(string clinicCode)
        {
            if (string.IsNullOrWhiteSpace(clinicCode))
                return (false, "Kode clinic otomatis gagal dibuat.");

            var normalizedCode = clinicCode.Trim().ToUpperInvariant();

            var duplicateCode = await _dbContext.Set<MstClinic>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.ClinicCode.ToUpper() == normalizedCode);

            if (duplicateCode)
                return (false, "Kode clinic otomatis sudah digunakan. Silakan ulangi proses create.");

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

        private static (bool IsValid, DateTime? Start, DateTime? EndExclusive, string? ErrorMessage)
            ResolveDateRange(DateTime? startDate, DateTime? endDate, string? customPeriod)
        {
            var today = DateTime.UtcNow.Date;

            if (!string.IsNullOrWhiteSpace(customPeriod) &&
                !string.Equals(customPeriod, "custom", StringComparison.OrdinalIgnoreCase))
            {
                return customPeriod.ToLowerInvariant() switch
                {
                    "today" => (true, today, today.AddDays(1), null),
                    "last7days" => (true, today.AddDays(-6), today.AddDays(1), null),
                    "last30days" => (true, today.AddDays(-29), today.AddDays(1), null),
                    "thismonth" => (true, new DateTime(today.Year, today.Month, 1), new DateTime(today.Year, today.Month, 1).AddMonths(1), null),
                    _ => (false, null, null, "Custom period tidak valid.")
                };
            }

            if (startDate.HasValue && endDate.HasValue && startDate.Value.Date > endDate.Value.Date)
            {
                return (false, null, null, "Start date tidak boleh lebih besar dari end date.");
            }

            return (
                true,
                startDate?.Date,
                endDate?.Date.AddDays(1),
                null
            );
        }

        private static List<ClinicCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<ClinicCustomPeriodOptionResponse>
            {
                new() { Value = "custom", Label = "Custom", Description = "Gunakan startDate dan endDate.", UsesStartDate = true, UsesEndDate = true },
                new() { Value = "today", Label = "Hari ini", Description = "Data yang dibuat hari ini.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last7days", Label = "7 hari terakhir", Description = "Data yang dibuat dalam 7 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last30days", Label = "30 hari terakhir", Description = "Data yang dibuat dalam 30 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thismonth", Label = "Bulan ini", Description = "Data yang dibuat pada bulan berjalan.", UsesStartDate = false, UsesEndDate = false }
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
                    Label = SplitPascalCase(x.ToString())
                })
                .ToList();
        }

        private static string SplitPascalCase(string value)
        {
            return string.Concat(value.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
        }

        private static List<ClinicQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<ClinicQueryParameterInfoResponse>
            {
                new() { Name = "startDate", Type = "date", Description = "Filter tanggal mulai berdasarkan CreateDateTime.", Example = "2026-06-01" },
                new() { Name = "endDate", Type = "date", Description = "Filter tanggal akhir berdasarkan CreateDateTime.", Example = "2026-06-30" },
                new() { Name = "customPeriod", Type = "string", Description = "Preset periode. Pilihan: custom, today, last7days, last30days, thismonth.", Example = "last7days" },
                new() { Name = "serviceUnitId", Type = "guid", Description = "Relasi table 1. Filter clinic berdasarkan service unit.", Example = "3fa85f64-5717-4562-b3fc-2c963f66afa6" },
                new() { Name = "isActive", Type = "bool", Description = "Filter status aktif.", Example = "true" },
                new() { Name = "search", Type = "string", Description = "Cari berdasarkan kode clinic, nama clinic, nama singkat, lokasi, ruangan, deskripsi, atau service unit.", Example = "Poli Umum" },
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
                new() { Name = "clinicCode", Label = "Kode Clinic", Section = "Basic", InputType = "readonly", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "AutoGenerated", MaxLength = 50, Description = "Digenerate otomatis oleh sistem dengan format CL-RSMMC-00001. Nomor terkecil yang kosong dari data aktif akan dipakai kembali.", Example = "CL-RSMMC-00001", SortOrder = 1 },
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
