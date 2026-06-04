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
        [AccessAction("Read", "Read Service Unit", Description = "Melihat data service unit", AccessType = AccessTypes.Read, SortOrder = 1)]
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
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                ServiceUnitTypeOptions = BuildEnumOptions<ServiceUnitType>(),
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateFieldMetadata(),
                UpdateFields = BuildUpdateFieldMetadata()
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
        [AccessAction("Read", "Read Service Unit", Description = "Melihat data service unit", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ServiceUnit", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = _dbContext.Set<MstServiceUnit>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new ServiceUnitSummaryResponse
            {
                TotalServiceUnit = await query.CountAsync(),
                ActiveServiceUnit = await query.CountAsync(x => x.IsActive),
                InactiveServiceUnit = await query.CountAsync(x => !x.IsActive),
                RegistrationAvailableServiceUnit = await query.CountAsync(x => x.IsAvailableForRegistration),
                KioskAvailableServiceUnit = await query.CountAsync(x => x.IsAvailableForKiosk),
                AppointmentAvailableServiceUnit = await query.CountAsync(x => x.IsAvailableForAppointment),
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
        [AccessAction("Read", "Read Service Unit", Description = "Melihat data service unit", AccessType = AccessTypes.Read, SortOrder = 1)]
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

            var query = _dbContext.Set<MstServiceUnit>()
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

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ServiceUnitCode.ToLower().Contains(keyword) ||
                    x.ServiceUnitName.ToLower().Contains(keyword) ||
                    (x.ShortName != null && x.ShortName.ToLower().Contains(keyword)) ||
                    (x.LocationName != null && x.LocationName.ToLower().Contains(keyword)) ||
                    (x.FloorName != null && x.FloorName.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (serviceUnitType.HasValue)
                query = query.Where(x => x.ServiceUnitType == serviceUnitType.Value);

            if (isAvailableForRegistration.HasValue)
                query = query.Where(x => x.IsAvailableForRegistration == isAvailableForRegistration.Value);

            if (isAvailableForKiosk.HasValue)
                query = query.Where(x => x.IsAvailableForKiosk == isAvailableForKiosk.Value);

            if (isAvailableForAppointment.HasValue)
                query = query.Where(x => x.IsAvailableForAppointment == isAvailableForAppointment.Value);

            if (isDoctorRequired.HasValue)
                query = query.Where(x => x.IsDoctorRequired == isDoctorRequired.Value);

            if (isScreeningRequired.HasValue)
                query = query.Where(x => x.IsScreeningRequired == isScreeningRequired.Value);

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ServiceUnitResponse
                {
                    Id = x.Id,
                    ServiceUnitCode = x.ServiceUnitCode,
                    ServiceUnitName = x.ServiceUnitName,
                    ServiceUnitType = x.ServiceUnitType,
                    ShortName = x.ShortName,
                    LocationName = x.LocationName,
                    FloorName = x.FloorName,
                    IsAvailableForRegistration = x.IsAvailableForRegistration,
                    IsAvailableForKiosk = x.IsAvailableForKiosk,
                    IsAvailableForAppointment = x.IsAvailableForAppointment,
                    IsQueueRequired = x.IsQueueRequired,
                    IsDoctorRequired = x.IsDoctorRequired,
                    IsScreeningRequired = x.IsScreeningRequired,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

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
        [AccessAction("Read", "Read Service Unit", Description = "Melihat data service unit", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ServiceUnit", "Read")]
        public async Task<IActionResult> GetServiceUnitOptions(
    [FromQuery] bool onlyActive = true,
    [FromQuery] string? search = null,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = _dbContext.Set<MstServiceUnit>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ServiceUnitCode.ToLower().Contains(keyword) ||
                    x.ServiceUnitName.ToLower().Contains(keyword) ||
                    (x.ShortName != null && x.ShortName.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ServiceUnitName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ServiceUnitOptionResponse
                {
                    Id = x.Id,
                    ServiceUnitCode = x.ServiceUnitCode,
                    ServiceUnitName = x.ServiceUnitName,
                    ServiceUnitType = x.ServiceUnitType,
                    ShortName = x.ShortName,
                    IsAvailableForRegistration = x.IsAvailableForRegistration,
                    IsAvailableForKiosk = x.IsAvailableForKiosk,
                    IsAvailableForAppointment = x.IsAvailableForAppointment
                })
                .ToListAsync();

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
        [AccessAction("Read", "Read Service Unit", Description = "Melihat data service unit", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ServiceUnit", "Read")]
        public async Task<IActionResult> GetServiceUnitById(Guid id)
        {
            var data = await _dbContext.Set<MstServiceUnit>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new ServiceUnitDetailResponse
                {
                    Id = x.Id,
                    ServiceUnitCode = x.ServiceUnitCode,
                    ServiceUnitName = x.ServiceUnitName,
                    ServiceUnitType = x.ServiceUnitType,
                    ShortName = x.ShortName,
                    LocationName = x.LocationName,
                    FloorName = x.FloorName,
                    IsAvailableForRegistration = x.IsAvailableForRegistration,
                    IsAvailableForKiosk = x.IsAvailableForKiosk,
                    IsAvailableForAppointment = x.IsAvailableForAppointment,
                    IsQueueRequired = x.IsQueueRequired,
                    IsDoctorRequired = x.IsDoctorRequired,
                    IsScreeningRequired = x.IsScreeningRequired,
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
                    "Service unit tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<ServiceUnitDetailResponse>.Ok(
                data,
                "Detail service unit berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ServiceUnitCreateResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create Service Unit", Description = "Membuat data service unit", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("ServiceUnit", "Create")]
        public async Task<IActionResult> CreateServiceUnit([FromBody] CreateServiceUnitRequest request)
        {
            var validation = await ValidateRequestAsync(null, request.ServiceUnitName);

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

            var generatedServiceUnitCode = await GenerateReusableServiceUnitCodeAsync();

            var codeValidation = await ValidateGeneratedCodeIsAvailableAsync(generatedServiceUnitCode);
            if (!codeValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    codeValidation.ErrorMessage ?? "Kode service unit otomatis tidak valid."
                ));
            }

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

            var response = new ServiceUnitCreateResponse
            {
                Id = entity.Id,
                ServiceUnitCode = entity.ServiceUnitCode,
                ServiceUnitName = entity.ServiceUnitName,
                ServiceUnitType = entity.ServiceUnitType,
                IsActive = entity.IsActive
            };

            return Ok(ApiResponse<ServiceUnitCreateResponse>.Ok(
                response,
                "Service unit berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Service Unit", Description = "Mengubah data service unit", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("ServiceUnit", "Update")]
        public async Task<IActionResult> UpdateServiceUnit(Guid id, [FromBody] UpdateServiceUnitRequest request)
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

            var validation = await ValidateRequestAsync(id, request.ServiceUnitName);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data service unit tidak valid."
                ));
            }

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
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Service unit berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Service Unit", Description = "Menghapus data service unit", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("ServiceUnit", "Delete")]
        public async Task<IActionResult> DeleteServiceUnit(Guid id)
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

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Service unit berhasil dihapus."
            ));
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            string serviceUnitName)
        {
            if (string.IsNullOrWhiteSpace(serviceUnitName))
            {
                return (false, "Nama service unit wajib diisi.");
            }

            var normalizedName = serviceUnitName.Trim().ToLower();

            var duplicateName = await _dbContext.Set<MstServiceUnit>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.ServiceUnitName.ToLower() == normalizedName &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateName)
            {
                return (false, "Nama service unit sudah digunakan.");
            }

            return (true, null);
        }

        private async Task<string> GenerateReusableServiceUnitCodeAsync()
        {
            var usedCodes = await _dbContext.Set<MstServiceUnit>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.ServiceUnitCode.StartsWith(ServiceUnitCodePrefix))
                .Select(x => x.ServiceUnitCode)
                .ToListAsync();

            var usedNumbers = new HashSet<int>();

            foreach (var code in usedCodes)
            {
                var suffix = code[ServiceUnitCodePrefix.Length..];

                if (int.TryParse(suffix, NumberStyles.None, CultureInfo.InvariantCulture, out var number) && number > 0)
                {
                    usedNumbers.Add(number);
                }
            }

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }

            return $"{ServiceUnitCodePrefix}{nextNumber.ToString($"D{ServiceUnitCodeDigitLength}", CultureInfo.InvariantCulture)}";
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateGeneratedCodeIsAvailableAsync(string serviceUnitCode)
        {
            var normalizedCode = serviceUnitCode.Trim().ToUpperInvariant();

            var duplicateCode = await _dbContext.Set<MstServiceUnit>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.ServiceUnitCode.ToUpper() == normalizedCode);

            if (duplicateCode)
            {
                return (false, "Kode service unit otomatis sudah digunakan. Silakan ulangi proses create.");
            }

            return (true, null);
        }

        private static IQueryable<MstServiceUnit> ApplySorting(
            IQueryable<MstServiceUnit> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").ToLowerInvariant() switch
            {
                "createdatetime" => isDesc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "serviceunitcode" => isDesc
                    ? query.OrderByDescending(x => x.ServiceUnitCode)
                    : query.OrderBy(x => x.ServiceUnitCode),

                "serviceunitname" => isDesc
                    ? query.OrderByDescending(x => x.ServiceUnitName)
                    : query.OrderBy(x => x.ServiceUnitName),

                "serviceunittype" => isDesc
                    ? query.OrderByDescending(x => x.ServiceUnitType)
                    : query.OrderBy(x => x.ServiceUnitType),

                "isactive" => isDesc
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => isDesc
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.ServiceUnitName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.ServiceUnitName)
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

        private static List<ServiceUnitCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<ServiceUnitCustomPeriodOptionResponse>
            {
                new() { Value = "custom", Label = "Custom", Description = "Gunakan startDate dan endDate.", UsesStartDate = true, UsesEndDate = true },
                new() { Value = "today", Label = "Hari ini", Description = "Data yang dibuat hari ini.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last7days", Label = "7 hari terakhir", Description = "Data yang dibuat dalam 7 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last30days", Label = "30 hari terakhir", Description = "Data yang dibuat dalam 30 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thismonth", Label = "Bulan ini", Description = "Data yang dibuat pada bulan berjalan.", UsesStartDate = false, UsesEndDate = false }
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

        private static string SplitPascalCase(string value)
        {
            return string.Concat(value.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
        }

        private static List<ServiceUnitQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<ServiceUnitQueryParameterInfoResponse>
            {
                new() { Name = "search", Type = "string", Description = "Cari berdasarkan kode, nama, lokasi, lantai, atau deskripsi.", Example = "Rawat Jalan" },
                new() { Name = "serviceUnitType", Type = "enum", Description = "Filter berdasarkan tipe service unit.", Example = "1" },
                new() { Name = "isActive", Type = "bool", Description = "Filter status aktif.", Example = "true" },
                new() { Name = "pageNumber", Type = "int", Description = "Nomor halaman.", Example = "1" },
                new() { Name = "pageSize", Type = "int", Description = "Jumlah data per halaman.", Example = "25" }
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
                new() { Name = "serviceUnitCode", Label = "Kode Service Unit", Section = "Basic", InputType = "readonly", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "AutoGenerated", MaxLength = 50, Description = "Digenerate otomatis oleh sistem dengan format SU-RSMMC-00001. Nomor terkecil yang kosong dari data aktif akan dipakai kembali.", Example = "SU-RSMMC-00001", SortOrder = 1 },
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