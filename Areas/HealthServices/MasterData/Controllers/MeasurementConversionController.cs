using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseMeasurementConversionPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.MeasurementConversionResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/measurement-conversions")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Measurement Conversion",
        AreaName = "HealthServices",
        ControllerName = "MeasurementConversion",
        Description = "Health service master data measurement conversion",
        SortOrder = 11
    )]
    [Tags("Health Services / Master Data / Measurement Conversion")]
    public class MeasurementConversionController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public MeasurementConversionController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<MeasurementConversionFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Measurement Conversion", Description = "Melihat data measurement conversion", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MeasurementConversion", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new MeasurementConversionFilterMetadataResponse
            {
                DefaultFilter = new MeasurementConversionDefaultFilterResponse(),
                CustomPeriods = BuildCustomPeriodOptions(),
                SortOptions = new List<MeasurementConversionSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "fromMeasurementName", Label = "Dari satuan" },
                    new() { Value = "toMeasurementName", Label = "Ke satuan" },
                    new() { Value = "conversionFactor", Label = "Faktor konversi" },
                    new() { Value = "conversionGroupName", Label = "Grup konversi" },
                    new() { Value = "isBidirectional", Label = "Bidirectional" },
                    new() { Value = "isStandardConversion", Label = "Konversi standar" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateFieldMetadata(),
                UpdateFields = BuildUpdateFieldMetadata()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "MeasurementConversion.GetFilterMetadata",
                "Mengambil metadata filter measurement conversion.",
                result
            );

            return Ok(ApiResponse<MeasurementConversionFilterMetadataResponse>.Ok(
                result,
                "Metadata filter measurement conversion berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<MeasurementConversionSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Measurement Conversion", Description = "Melihat data measurement conversion", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MeasurementConversion", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();

            var result = new MeasurementConversionSummaryResponse
            {
                TotalMeasurementConversion = await query.CountAsync(),
                ActiveMeasurementConversion = await query.CountAsync(x => x.IsActive),
                InactiveMeasurementConversion = await query.CountAsync(x => !x.IsActive),
                BidirectionalConversion = await query.CountAsync(x => x.IsBidirectional),
                OneWayConversion = await query.CountAsync(x => !x.IsBidirectional),
                StandardConversion = await query.CountAsync(x => x.IsStandardConversion),
                NonStandardConversion = await query.CountAsync(x => !x.IsStandardConversion)
            };

            return Ok(ApiResponse<MeasurementConversionSummaryResponse>.Ok(
                result,
                "Ringkasan measurement conversion berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseMeasurementConversionPagedResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Measurement Conversion", Description = "Melihat data measurement conversion", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MeasurementConversion", "Read")]
        public async Task<IActionResult> GetMeasurementConversions(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? fromMeasurementId,
            [FromQuery] Guid? toMeasurementId,
            [FromQuery] bool? isActive,
            [FromQuery] bool? isBidirectional,
            [FromQuery] bool? isStandardConversion,
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
                fromMeasurementId,
                toMeasurementId,
                isActive,
                isBidirectional,
                isStandardConversion,
                search
            );

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorMap = await GetActorNameMapAsync(entities.Select(x => x.CreateBy));
            var items = entities.Select(x => ToResponse(x, actorMap)).ToList();

            var result = new ResponseMeasurementConversionPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseMeasurementConversionPagedResult>.Ok(
                result,
                "Data measurement conversion berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<MeasurementConversionOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Measurement Conversion", Description = "Melihat data pilihan measurement conversion", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MeasurementConversion", "Read")]
        public async Task<IActionResult> GetMeasurementConversionOptions(
            [FromQuery] Guid? fromMeasurementId,
            [FromQuery] Guid? toMeasurementId,
            [FromQuery] bool? isBidirectional,
            [FromQuery] bool? isStandardConversion,
            [FromQuery] bool onlyActive = true,
            [FromQuery] bool? activeOnly = null,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var isActive = activeOnly ?? (onlyActive ? true : null);

            var query = ApplyStandardFilter(
                BuildBaseQuery(),
                fromMeasurementId,
                toMeasurementId,
                isActive,
                isBidirectional,
                isStandardConversion,
                search
            );

            var totalData = await query.CountAsync();

            var entities = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.FromMeasurement != null ? x.FromMeasurement.MeasurementName : string.Empty)
                .ThenBy(x => x.ToMeasurement != null ? x.ToMeasurement.MeasurementName : string.Empty)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities.Select(ToOptionResponse).ToList();

            var result = new MeasurementConversionOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<MeasurementConversionOptionPagedResponse>.Ok(
                result,
                "Data pilihan measurement conversion berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<MeasurementConversionDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Measurement Conversion", Description = "Melihat detail measurement conversion", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MeasurementConversion", "Read")]
        public async Task<IActionResult> GetMeasurementConversionById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Measurement conversion tidak ditemukan."
                ));
            }

            var actorMap = await GetActorNameMapAsync(new[] { entity.CreateBy, entity.UpdateBy });
            var data = ToDetailResponse(entity, actorMap);

            return Ok(ApiResponse<MeasurementConversionDetailResponse>.Ok(
                data,
                "Detail measurement conversion berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<MeasurementConversionCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Measurement Conversion", Description = "Membuat data measurement conversion", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("MeasurementConversion", "Create")]
        public async Task<IActionResult> CreateMeasurementConversion([FromBody] CreateMeasurementConversionRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                fromMeasurementId: request.FromMeasurementId,
                toMeasurementId: request.ToMeasurementId,
                conversionFactor: request.ConversionFactor,
                isBidirectional: request.IsBidirectional
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data measurement conversion tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new MstMeasurementConversion
            {
                Id = Guid.NewGuid(),
                FromMeasurementId = request.FromMeasurementId,
                ToMeasurementId = request.ToMeasurementId,
                ConversionFactor = request.ConversionFactor,
                IsBidirectional = request.IsBidirectional,
                IsStandardConversion = request.IsStandardConversion,
                ConversionGroupName = NormalizeNullableText(request.ConversionGroupName),
                FormulaNote = NormalizeNullableText(request.FormulaNote),
                SortOrder = request.SortOrder,
                Description = NormalizeNullableText(request.Description),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstMeasurementConversion>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = await BuildCreateUpdateResponseAsync(entity.Id);

            await _loggerService.InfoAsync(
                LogCategory,
                "MeasurementConversion.CreateMeasurementConversion",
                "Membuat data measurement conversion.",
                response
            );

            return Ok(ApiResponse<MeasurementConversionCreateResponse>.Ok(
                response,
                "Measurement conversion berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<MeasurementConversionUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Update", "Update Measurement Conversion", Description = "Mengubah data measurement conversion", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("MeasurementConversion", "Update")]
        public async Task<IActionResult> UpdateMeasurementConversion(Guid id, [FromBody] UpdateMeasurementConversionRequest request)
        {
            var entity = await _dbContext.Set<MstMeasurementConversion>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Measurement conversion tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(
                excludeId: id,
                fromMeasurementId: request.FromMeasurementId,
                toMeasurementId: request.ToMeasurementId,
                conversionFactor: request.ConversionFactor,
                isBidirectional: request.IsBidirectional
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data measurement conversion tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.FromMeasurementId = request.FromMeasurementId;
            entity.ToMeasurementId = request.ToMeasurementId;
            entity.ConversionFactor = request.ConversionFactor;
            entity.IsBidirectional = request.IsBidirectional;
            entity.IsStandardConversion = request.IsStandardConversion;
            entity.ConversionGroupName = NormalizeNullableText(request.ConversionGroupName);
            entity.FormulaNote = NormalizeNullableText(request.FormulaNote);
            entity.SortOrder = request.SortOrder;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var response = (MeasurementConversionUpdateResponse)await BuildCreateUpdateResponseAsync(entity.Id, isUpdate: true);

            await _loggerService.InfoAsync(
                LogCategory,
                "MeasurementConversion.UpdateMeasurementConversion",
                "Mengubah data measurement conversion.",
                response
            );

            return Ok(ApiResponse<MeasurementConversionUpdateResponse>.Ok(
                response,
                "Measurement conversion berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<MeasurementConversionUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Measurement Conversion Status", Description = "Mengubah status aktif measurement conversion", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("MeasurementConversion", "Update")]
        public async Task<IActionResult> UpdateMeasurementConversionStatus(Guid id, [FromBody] UpdateMeasurementConversionStatusRequest request)
        {
            var entity = await _dbContext.Set<MstMeasurementConversion>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Measurement conversion tidak ditemukan."
                ));
            }

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            if (!string.IsNullOrWhiteSpace(request.Reason))
                entity.Description = request.Reason.Trim();

            await _dbContext.SaveChangesAsync();

            var response = (MeasurementConversionUpdateResponse)await BuildCreateUpdateResponseAsync(entity.Id, isUpdate: true);

            return Ok(ApiResponse<MeasurementConversionUpdateResponse>.Ok(
                response,
                request.IsActive ? "Measurement conversion berhasil diaktifkan." : "Measurement conversion berhasil dinonaktifkan."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<MeasurementConversionDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Measurement Conversion", Description = "Menghapus data measurement conversion", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("MeasurementConversion", "Delete")]
        public async Task<IActionResult> DeleteMeasurementConversion(Guid id, [FromBody] DeleteMeasurementConversionRequest? request = null)
        {
            var entity = await _dbContext.Set<MstMeasurementConversion>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Measurement conversion tidak ditemukan."
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
                entity.Description = request.DeleteReason.Trim();

            await _dbContext.SaveChangesAsync();

            var response = new MeasurementConversionDeleteResponse
            {
                Id = entity.Id,
                FromMeasurementId = entity.FromMeasurementId,
                ToMeasurementId = entity.ToMeasurementId,
                IsDelete = entity.IsDelete,
                IsActive = entity.IsActive,
                DeleteDateTime = entity.DeleteDateTime
            };

            return Ok(ApiResponse<MeasurementConversionDeleteResponse>.Ok(
                response,
                "Measurement conversion berhasil dihapus."
            ));
        }

        private IQueryable<MstMeasurementConversion> BuildBaseQuery()
        {
            return _dbContext.Set<MstMeasurementConversion>()
                .AsNoTracking()
                .Include(x => x.FromMeasurement)
                .Include(x => x.ToMeasurement)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstMeasurementConversion> ApplyDateFilter(
            IQueryable<MstMeasurementConversion> query,
            DateRangeResult dateRange)
        {
            if (dateRange.Start.HasValue)
                query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);

            if (dateRange.EndExclusive.HasValue)
                query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);

            return query;
        }

        private static IQueryable<MstMeasurementConversion> ApplyStandardFilter(
            IQueryable<MstMeasurementConversion> query,
            Guid? fromMeasurementId,
            Guid? toMeasurementId,
            bool? isActive,
            bool? isBidirectional,
            bool? isStandardConversion,
            string? search)
        {
            if (fromMeasurementId.HasValue && fromMeasurementId.Value != Guid.Empty)
                query = query.Where(x => x.FromMeasurementId == fromMeasurementId.Value);

            if (toMeasurementId.HasValue && toMeasurementId.Value != Guid.Empty)
                query = query.Where(x => x.ToMeasurementId == toMeasurementId.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (isBidirectional.HasValue)
                query = query.Where(x => x.IsBidirectional == isBidirectional.Value);

            if (isStandardConversion.HasValue)
                query = query.Where(x => x.IsStandardConversion == isStandardConversion.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    (x.FromMeasurement != null && x.FromMeasurement.MeasurementCode.ToLower().Contains(keyword)) ||
                    (x.FromMeasurement != null && x.FromMeasurement.MeasurementName.ToLower().Contains(keyword)) ||
                    (x.FromMeasurement != null && x.FromMeasurement.MeasurementSymbol != null && x.FromMeasurement.MeasurementSymbol.ToLower().Contains(keyword)) ||
                    (x.FromMeasurement != null && x.FromMeasurement.MeasurementType.ToLower().Contains(keyword)) ||
                    (x.ToMeasurement != null && x.ToMeasurement.MeasurementCode.ToLower().Contains(keyword)) ||
                    (x.ToMeasurement != null && x.ToMeasurement.MeasurementName.ToLower().Contains(keyword)) ||
                    (x.ToMeasurement != null && x.ToMeasurement.MeasurementSymbol != null && x.ToMeasurement.MeasurementSymbol.ToLower().Contains(keyword)) ||
                    (x.ToMeasurement != null && x.ToMeasurement.MeasurementType.ToLower().Contains(keyword)) ||
                    (x.ConversionGroupName != null && x.ConversionGroupName.ToLower().Contains(keyword)) ||
                    (x.FormulaNote != null && x.FormulaNote.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            return query;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            Guid fromMeasurementId,
            Guid toMeasurementId,
            decimal conversionFactor,
            bool isBidirectional)
        {
            if (fromMeasurementId == Guid.Empty)
                return (false, "From measurement wajib dipilih.");

            if (toMeasurementId == Guid.Empty)
                return (false, "To measurement wajib dipilih.");

            if (fromMeasurementId == toMeasurementId)
                return (false, "From measurement dan to measurement tidak boleh sama.");

            if (conversionFactor <= 0)
                return (false, "Conversion factor harus lebih dari 0.");

            var fromMeasurement = await _dbContext.Set<MstMeasurement>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == fromMeasurementId && x.IsActive && !x.IsDelete);

            if (fromMeasurement == null)
                return (false, "From measurement tidak valid atau tidak aktif.");

            var toMeasurement = await _dbContext.Set<MstMeasurement>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == toMeasurementId && x.IsActive && !x.IsDelete);

            if (toMeasurement == null)
                return (false, "To measurement tidak valid atau tidak aktif.");

            if (!string.Equals(fromMeasurement.MeasurementType, toMeasurement.MeasurementType, StringComparison.OrdinalIgnoreCase))
            {
                return (false, "Tipe from measurement dan to measurement berbeda. Gunakan tipe yang sama agar konversi aman.");
            }

            var duplicateExact = await _dbContext.Set<MstMeasurementConversion>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.FromMeasurementId == fromMeasurementId &&
                    x.ToMeasurementId == toMeasurementId &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateExact)
                return (false, "Konversi measurement tersebut sudah ada.");

            var duplicateReverse = await _dbContext.Set<MstMeasurementConversion>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.FromMeasurementId == toMeasurementId &&
                    x.ToMeasurementId == fromMeasurementId &&
                    (x.IsBidirectional || isBidirectional) &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateReverse)
                return (false, "Konversi measurement kebalikan sudah ada dan bersifat bidirectional.");

            return (true, null);
        }

        private async Task<MeasurementConversionCreateResponse> BuildCreateUpdateResponseAsync(Guid id, bool isUpdate = false)
        {
            var data = await BuildBaseQuery()
                .FirstAsync(x => x.Id == id);

            if (isUpdate)
            {
                return new MeasurementConversionUpdateResponse
                {
                    Id = data.Id,
                    FromMeasurementId = data.FromMeasurementId,
                    FromMeasurementName = data.FromMeasurement?.MeasurementName ?? string.Empty,
                    ToMeasurementId = data.ToMeasurementId,
                    ToMeasurementName = data.ToMeasurement?.MeasurementName ?? string.Empty,
                    ConversionFactor = data.ConversionFactor,
                    ReverseConversionFactor = CalculateReverseFactor(data.ConversionFactor, data.IsBidirectional),
                    IsBidirectional = data.IsBidirectional,
                    IsStandardConversion = data.IsStandardConversion,
                    IsActive = data.IsActive
                };
            }

            return new MeasurementConversionCreateResponse
            {
                Id = data.Id,
                FromMeasurementId = data.FromMeasurementId,
                FromMeasurementName = data.FromMeasurement?.MeasurementName ?? string.Empty,
                ToMeasurementId = data.ToMeasurementId,
                ToMeasurementName = data.ToMeasurement?.MeasurementName ?? string.Empty,
                ConversionFactor = data.ConversionFactor,
                ReverseConversionFactor = CalculateReverseFactor(data.ConversionFactor, data.IsBidirectional),
                IsBidirectional = data.IsBidirectional,
                IsStandardConversion = data.IsStandardConversion,
                IsActive = data.IsActive
            };
        }

        private static IQueryable<MstMeasurementConversion> ApplySorting(
            IQueryable<MstMeasurementConversion> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").ToLowerInvariant() switch
            {
                "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "frommeasurementname" => isDesc
                    ? query.OrderByDescending(x => x.FromMeasurement != null ? x.FromMeasurement.MeasurementName : string.Empty)
                    : query.OrderBy(x => x.FromMeasurement != null ? x.FromMeasurement.MeasurementName : string.Empty),
                "tomeasurementname" => isDesc
                    ? query.OrderByDescending(x => x.ToMeasurement != null ? x.ToMeasurement.MeasurementName : string.Empty)
                    : query.OrderBy(x => x.ToMeasurement != null ? x.ToMeasurement.MeasurementName : string.Empty),
                "conversionfactor" => isDesc ? query.OrderByDescending(x => x.ConversionFactor) : query.OrderBy(x => x.ConversionFactor),
                "conversiongroupname" => isDesc ? query.OrderByDescending(x => x.ConversionGroupName) : query.OrderBy(x => x.ConversionGroupName),
                "isbidirectional" => isDesc ? query.OrderByDescending(x => x.IsBidirectional) : query.OrderBy(x => x.IsBidirectional),
                "isstandardconversion" => isDesc ? query.OrderByDescending(x => x.IsStandardConversion) : query.OrderBy(x => x.IsStandardConversion),
                "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => isDesc
                    ? query.OrderByDescending(x => x.SortOrder)
                        .ThenByDescending(x => x.FromMeasurement != null ? x.FromMeasurement.MeasurementName : string.Empty)
                    : query.OrderBy(x => x.SortOrder)
                        .ThenBy(x => x.FromMeasurement != null ? x.FromMeasurement.MeasurementName : string.Empty)
            };
        }

        private static MeasurementConversionResponse ToResponse(MstMeasurementConversion x, Dictionary<Guid, string?> actorMap)
        {
            return new MeasurementConversionResponse
            {
                Id = x.Id,
                FromMeasurementId = x.FromMeasurementId,
                FromMeasurementCode = x.FromMeasurement?.MeasurementCode ?? string.Empty,
                FromMeasurementName = x.FromMeasurement?.MeasurementName ?? string.Empty,
                FromMeasurementSymbol = x.FromMeasurement?.MeasurementSymbol,
                FromMeasurementType = x.FromMeasurement?.MeasurementType ?? string.Empty,
                ToMeasurementId = x.ToMeasurementId,
                ToMeasurementCode = x.ToMeasurement?.MeasurementCode ?? string.Empty,
                ToMeasurementName = x.ToMeasurement?.MeasurementName ?? string.Empty,
                ToMeasurementSymbol = x.ToMeasurement?.MeasurementSymbol,
                ToMeasurementType = x.ToMeasurement?.MeasurementType ?? string.Empty,
                ConversionFactor = x.ConversionFactor,
                ReverseConversionFactor = CalculateReverseFactor(x.ConversionFactor, x.IsBidirectional),
                IsBidirectional = x.IsBidirectional,
                IsStandardConversion = x.IsStandardConversion,
                ConversionGroupName = x.ConversionGroupName,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime,
                CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                CreateByName = x.CreateBy == Guid.Empty ? null : actorMap.GetValueOrDefault(x.CreateBy)
            };
        }

        private static MeasurementConversionDetailResponse ToDetailResponse(MstMeasurementConversion x, Dictionary<Guid, string?> actorMap)
        {
            return new MeasurementConversionDetailResponse
            {
                Id = x.Id,
                FromMeasurementId = x.FromMeasurementId,
                FromMeasurementCode = x.FromMeasurement?.MeasurementCode ?? string.Empty,
                FromMeasurementName = x.FromMeasurement?.MeasurementName ?? string.Empty,
                FromMeasurementSymbol = x.FromMeasurement?.MeasurementSymbol,
                FromMeasurementType = x.FromMeasurement?.MeasurementType ?? string.Empty,
                ToMeasurementId = x.ToMeasurementId,
                ToMeasurementCode = x.ToMeasurement?.MeasurementCode ?? string.Empty,
                ToMeasurementName = x.ToMeasurement?.MeasurementName ?? string.Empty,
                ToMeasurementSymbol = x.ToMeasurement?.MeasurementSymbol,
                ToMeasurementType = x.ToMeasurement?.MeasurementType ?? string.Empty,
                ConversionFactor = x.ConversionFactor,
                ReverseConversionFactor = CalculateReverseFactor(x.ConversionFactor, x.IsBidirectional),
                IsBidirectional = x.IsBidirectional,
                IsStandardConversion = x.IsStandardConversion,
                ConversionGroupName = x.ConversionGroupName,
                FormulaNote = x.FormulaNote,
                SortOrder = x.SortOrder,
                Description = x.Description,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime,
                CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                CreateByName = x.CreateBy == Guid.Empty ? null : actorMap.GetValueOrDefault(x.CreateBy),
                UpdateDateTime = x.UpdateDateTime,
                UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                UpdateByName = x.UpdateBy == Guid.Empty ? null : actorMap.GetValueOrDefault(x.UpdateBy)
            };
        }

        private static MeasurementConversionOptionResponse ToOptionResponse(MstMeasurementConversion x)
        {
            return new MeasurementConversionOptionResponse
            {
                Id = x.Id,
                FromMeasurementId = x.FromMeasurementId,
                FromMeasurementCode = x.FromMeasurement?.MeasurementCode ?? string.Empty,
                FromMeasurementName = x.FromMeasurement?.MeasurementName ?? string.Empty,
                FromMeasurementSymbol = x.FromMeasurement?.MeasurementSymbol,
                ToMeasurementId = x.ToMeasurementId,
                ToMeasurementCode = x.ToMeasurement?.MeasurementCode ?? string.Empty,
                ToMeasurementName = x.ToMeasurement?.MeasurementName ?? string.Empty,
                ToMeasurementSymbol = x.ToMeasurement?.MeasurementSymbol,
                ConversionFactor = x.ConversionFactor,
                ReverseConversionFactor = CalculateReverseFactor(x.ConversionFactor, x.IsBidirectional),
                IsBidirectional = x.IsBidirectional,
                IsStandardConversion = x.IsStandardConversion,
                ConversionGroupName = x.ConversionGroupName,
                IsActive = x.IsActive
            };
        }

        private static decimal? CalculateReverseFactor(decimal conversionFactor, bool isBidirectional)
        {
            if (!isBidirectional || conversionFactor <= 0)
                return null;

            return 1 / conversionFactor;
        }

        private static DateRangeResult ResolveDateRange(DateTime? startDate, DateTime? endDate, string? customPeriod)
        {
            var today = DateTime.UtcNow.Date;
            var period = customPeriod?.Trim().ToLowerInvariant();

            if (!string.IsNullOrWhiteSpace(period) && period != "custom")
            {
                return period switch
                {
                    "all" => DateRangeResult.Valid(null, null),
                    "today" => DateRangeResult.Valid(today, today.AddDays(1)),
                    "yesterday" => DateRangeResult.Valid(today.AddDays(-1), today),
                    "last7days" => DateRangeResult.Valid(today.AddDays(-6), today.AddDays(1)),
                    "last30days" => DateRangeResult.Valid(today.AddDays(-29), today.AddDays(1)),
                    "thismonth" => DateRangeResult.Valid(new DateTime(today.Year, today.Month, 1), new DateTime(today.Year, today.Month, 1).AddMonths(1)),
                    "lastmonth" => DateRangeResult.Valid(new DateTime(today.Year, today.Month, 1).AddMonths(-1), new DateTime(today.Year, today.Month, 1)),
                    "thisyear" => DateRangeResult.Valid(new DateTime(today.Year, 1, 1), new DateTime(today.Year + 1, 1, 1)),
                    _ => DateRangeResult.Invalid("Custom period tidak dikenali.")
                };
            }

            var start = startDate?.Date;
            var endExclusive = endDate?.Date.AddDays(1);

            if (start.HasValue && endExclusive.HasValue && start.Value >= endExclusive.Value)
                return DateRangeResult.Invalid("StartDate tidak boleh lebih besar dari EndDate.");

            return DateRangeResult.Valid(start, endExclusive);
        }

        private static List<MeasurementConversionCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<MeasurementConversionCustomPeriodOptionResponse>
            {
                new() { Value = "all", Label = "Semua", Description = "Tampilkan semua data tanpa filter tanggal.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "today", Label = "Hari ini", Description = "Data yang dibuat hari ini.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "yesterday", Label = "Kemarin", Description = "Data yang dibuat kemarin.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last7days", Label = "7 hari terakhir", Description = "Data yang dibuat dalam 7 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last30days", Label = "30 hari terakhir", Description = "Data yang dibuat dalam 30 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thisMonth", Label = "Bulan ini", Description = "Data yang dibuat pada bulan berjalan.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "lastMonth", Label = "Bulan lalu", Description = "Data yang dibuat pada bulan sebelumnya.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thisYear", Label = "Tahun ini", Description = "Data yang dibuat pada tahun berjalan.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "custom", Label = "Custom", Description = "Gunakan startDate dan endDate.", UsesStartDate = true, UsesEndDate = true }
            };
        }

        private static List<MeasurementConversionQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<MeasurementConversionQueryParameterInfoResponse>
            {
                new() { Name = "startDate", Type = "date", Description = "Tanggal awal filter berdasarkan CreateDateTime.", Example = "2026-06-01" },
                new() { Name = "endDate", Type = "date", Description = "Tanggal akhir filter berdasarkan CreateDateTime.", Example = "2026-06-30" },
                new() { Name = "customPeriod", Type = "string", Description = "Preset periode: all, today, yesterday, last7days, last30days, thismonth, lastmonth, thisyear, custom.", Example = "thisMonth" },
                new() { Name = "fromMeasurementId", Type = "guid", Description = "Filter berdasarkan measurement asal." },
                new() { Name = "toMeasurementId", Type = "guid", Description = "Filter berdasarkan measurement tujuan." },
                new() { Name = "isActive", Type = "boolean", Description = "Filter status aktif." },
                new() { Name = "isBidirectional", Type = "boolean", Description = "Filter konversi dua arah." },
                new() { Name = "isStandardConversion", Type = "boolean", Description = "Filter konversi standar." },
                new() { Name = "search", Type = "string", Description = "Pencarian measurement asal/tujuan, group, formula note, atau deskripsi." },
                new() { Name = "sortBy", Type = "string", Description = "Kolom sorting." },
                new() { Name = "sortDirection", Type = "string", Description = "asc atau desc.", Example = "asc" },
                new() { Name = "pageNumber", Type = "integer", Description = "Nomor halaman.", Example = "1" },
                new() { Name = "pageSize", Type = "integer", Description = "Jumlah data per halaman.", Example = "25" }
            };
        }

        private static List<MeasurementConversionFormFieldMetadataResponse> BuildCreateFieldMetadata()
        {
            return new List<MeasurementConversionFormFieldMetadataResponse>
            {
                new() { Name = "fromMeasurementId", Label = "From Measurement", Section = "Basic", DataType = "guid", InputType = "select", Required = true, RequiredType = "Required", OptionsSource = "/api/v1/health-services/master-data/measurements/options", Description = "Satuan asal, contoh KG.", SortOrder = 1 },
                new() { Name = "toMeasurementId", Label = "To Measurement", Section = "Basic", DataType = "guid", InputType = "select", Required = true, RequiredType = "Required", OptionsSource = "/api/v1/health-services/master-data/measurements/options", Description = "Satuan tujuan, contoh G.", SortOrder = 2 },
                new() { Name = "conversionFactor", Label = "Conversion Factor", Section = "Basic", DataType = "decimal", InputType = "number", Required = true, RequiredType = "Required", Description = "Contoh: 1 KG = 1000 G, maka factor = 1000.", SortOrder = 3 },
                new() { Name = "isBidirectional", Label = "Bidirectional", Section = "Rule", DataType = "boolean", InputType = "switch", SortOrder = 4 },
                new() { Name = "isStandardConversion", Label = "Standard Conversion", Section = "Rule", DataType = "boolean", InputType = "switch", SortOrder = 5 },
                new() { Name = "conversionGroupName", Label = "Group Konversi", Section = "Additional", DataType = "string", InputType = "text", MaxLength = 100, Placeholder = "Weight, Volume, Pharmacy", SortOrder = 6 },
                new() { Name = "formulaNote", Label = "Formula Note", Section = "Additional", DataType = "string", InputType = "textarea", MaxLength = 250, SortOrder = 7 },
                new() { Name = "sortOrder", Label = "Urutan", Section = "Display", DataType = "integer", InputType = "number", SortOrder = 8 },
                new() { Name = "description", Label = "Deskripsi", Section = "Additional", DataType = "string", InputType = "textarea", MaxLength = 250, SortOrder = 9 }
            };
        }

        private static List<MeasurementConversionFormFieldMetadataResponse> BuildUpdateFieldMetadata()
        {
            var fields = BuildCreateFieldMetadata();

            fields.Add(new MeasurementConversionFormFieldMetadataResponse
            {
                Name = "isActive",
                Label = "Status Aktif",
                Section = "Status",
                DataType = "boolean",
                InputType = "switch",
                SortOrder = 99
            });

            return fields.OrderBy(x => x.SortOrder).ToList();
        }

        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(IEnumerable<Guid> actorIds)
        {
            var ids = actorIds.Where(x => x != Guid.Empty).Distinct().ToList();

            if (!ids.Any())
                return new Dictionary<Guid, string?>();

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode
                })
                .ToDictionaryAsync(x => x.Id, x => x.Name);
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private Guid GetCurrentUserId()
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
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

        private sealed class DateRangeResult
        {
            public bool IsValid { get; private init; }
            public DateTime? Start { get; private init; }
            public DateTime? EndExclusive { get; private init; }
            public string? ErrorMessage { get; private init; }

            public static DateRangeResult Valid(DateTime? start, DateTime? endExclusive)
            {
                return new DateRangeResult
                {
                    IsValid = true,
                    Start = start,
                    EndExclusive = endExclusive
                };
            }

            public static DateRangeResult Invalid(string errorMessage)
            {
                return new DateRangeResult
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
        }
    }
}
