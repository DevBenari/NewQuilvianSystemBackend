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
                    new() { Value = "isStandardConversion", Label = "Konversi standar" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
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
            var query = _dbContext.Set<MstMeasurementConversion>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

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
        [AccessAction("Read", "Read Measurement Conversion", Description = "Melihat data measurement conversion", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MeasurementConversion", "Read")]
        public async Task<IActionResult> GetMeasurementConversions(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? fromMeasurementId,
            [FromQuery] Guid? toMeasurementId,
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

            if (fromMeasurementId.HasValue && fromMeasurementId.Value != Guid.Empty)
                query = query.Where(x => x.FromMeasurementId == fromMeasurementId.Value);

            if (toMeasurementId.HasValue && toMeasurementId.Value != Guid.Empty)
                query = query.Where(x => x.ToMeasurementId == toMeasurementId.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    (x.FromMeasurement != null && x.FromMeasurement.MeasurementCode.ToLower().Contains(keyword)) ||
                    (x.FromMeasurement != null && x.FromMeasurement.MeasurementName.ToLower().Contains(keyword)) ||
                    (x.FromMeasurement != null && x.FromMeasurement.MeasurementSymbol != null && x.FromMeasurement.MeasurementSymbol.ToLower().Contains(keyword)) ||
                    (x.ToMeasurement != null && x.ToMeasurement.MeasurementCode.ToLower().Contains(keyword)) ||
                    (x.ToMeasurement != null && x.ToMeasurement.MeasurementName.ToLower().Contains(keyword)) ||
                    (x.ToMeasurement != null && x.ToMeasurement.MeasurementSymbol != null && x.ToMeasurement.MeasurementSymbol.ToLower().Contains(keyword)) ||
                    (x.ConversionGroupName != null && x.ConversionGroupName.ToLower().Contains(keyword)) ||
                    (x.FormulaNote != null && x.FormulaNote.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new MeasurementConversionResponse
                {
                    Id = x.Id,
                    FromMeasurementId = x.FromMeasurementId,
                    FromMeasurementCode = x.FromMeasurement != null ? x.FromMeasurement.MeasurementCode : string.Empty,
                    FromMeasurementName = x.FromMeasurement != null ? x.FromMeasurement.MeasurementName : string.Empty,
                    FromMeasurementSymbol = x.FromMeasurement != null ? x.FromMeasurement.MeasurementSymbol : null,
                    FromMeasurementType = x.FromMeasurement != null ? x.FromMeasurement.MeasurementType : string.Empty,
                    ToMeasurementId = x.ToMeasurementId,
                    ToMeasurementCode = x.ToMeasurement != null ? x.ToMeasurement.MeasurementCode : string.Empty,
                    ToMeasurementName = x.ToMeasurement != null ? x.ToMeasurement.MeasurementName : string.Empty,
                    ToMeasurementSymbol = x.ToMeasurement != null ? x.ToMeasurement.MeasurementSymbol : null,
                    ToMeasurementType = x.ToMeasurement != null ? x.ToMeasurement.MeasurementType : string.Empty,
                    ConversionFactor = x.ConversionFactor,
                    IsBidirectional = x.IsBidirectional,
                    IsStandardConversion = x.IsStandardConversion,
                    ConversionGroupName = x.ConversionGroupName,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

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
        [ProducesResponseType(typeof(ApiResponse<List<MeasurementConversionOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Measurement Conversion", Description = "Melihat data measurement conversion", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MeasurementConversion", "Read")]
        public async Task<IActionResult> GetMeasurementConversionOptions(
            [FromQuery] Guid? fromMeasurementId,
            [FromQuery] Guid? toMeasurementId,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = BuildBaseQuery();

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (fromMeasurementId.HasValue && fromMeasurementId.Value != Guid.Empty)
                query = query.Where(x => x.FromMeasurementId == fromMeasurementId.Value);

            if (toMeasurementId.HasValue && toMeasurementId.Value != Guid.Empty)
                query = query.Where(x => x.ToMeasurementId == toMeasurementId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    (x.FromMeasurement != null && x.FromMeasurement.MeasurementName.ToLower().Contains(keyword)) ||
                    (x.FromMeasurement != null && x.FromMeasurement.MeasurementSymbol != null && x.FromMeasurement.MeasurementSymbol.ToLower().Contains(keyword)) ||
                    (x.ToMeasurement != null && x.ToMeasurement.MeasurementName.ToLower().Contains(keyword)) ||
                    (x.ToMeasurement != null && x.ToMeasurement.MeasurementSymbol != null && x.ToMeasurement.MeasurementSymbol.ToLower().Contains(keyword)) ||
                    (x.ConversionGroupName != null && x.ConversionGroupName.ToLower().Contains(keyword)));
            }

            var data = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.FromMeasurement != null ? x.FromMeasurement.MeasurementName : string.Empty)
                .ThenBy(x => x.ToMeasurement != null ? x.ToMeasurement.MeasurementName : string.Empty)
                .Select(x => new MeasurementConversionOptionResponse
                {
                    Id = x.Id,
                    FromMeasurementId = x.FromMeasurementId,
                    FromMeasurementName = x.FromMeasurement != null ? x.FromMeasurement.MeasurementName : string.Empty,
                    FromMeasurementSymbol = x.FromMeasurement != null ? x.FromMeasurement.MeasurementSymbol : null,
                    ToMeasurementId = x.ToMeasurementId,
                    ToMeasurementName = x.ToMeasurement != null ? x.ToMeasurement.MeasurementName : string.Empty,
                    ToMeasurementSymbol = x.ToMeasurement != null ? x.ToMeasurement.MeasurementSymbol : null,
                    ConversionFactor = x.ConversionFactor,
                    IsBidirectional = x.IsBidirectional,
                    IsStandardConversion = x.IsStandardConversion,
                    ConversionGroupName = x.ConversionGroupName
                })
                .ToListAsync();

            return Ok(ApiResponse<List<MeasurementConversionOptionResponse>>.Ok(
                data,
                "Data pilihan measurement conversion berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<MeasurementConversionDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Measurement Conversion", Description = "Melihat data measurement conversion", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MeasurementConversion", "Read")]
        public async Task<IActionResult> GetMeasurementConversionById(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new MeasurementConversionDetailResponse
                {
                    Id = x.Id,
                    FromMeasurementId = x.FromMeasurementId,
                    FromMeasurementCode = x.FromMeasurement != null ? x.FromMeasurement.MeasurementCode : string.Empty,
                    FromMeasurementName = x.FromMeasurement != null ? x.FromMeasurement.MeasurementName : string.Empty,
                    FromMeasurementSymbol = x.FromMeasurement != null ? x.FromMeasurement.MeasurementSymbol : null,
                    FromMeasurementType = x.FromMeasurement != null ? x.FromMeasurement.MeasurementType : string.Empty,
                    ToMeasurementId = x.ToMeasurementId,
                    ToMeasurementCode = x.ToMeasurement != null ? x.ToMeasurement.MeasurementCode : string.Empty,
                    ToMeasurementName = x.ToMeasurement != null ? x.ToMeasurement.MeasurementName : string.Empty,
                    ToMeasurementSymbol = x.ToMeasurement != null ? x.ToMeasurement.MeasurementSymbol : null,
                    ToMeasurementType = x.ToMeasurement != null ? x.ToMeasurement.MeasurementType : string.Empty,
                    ConversionFactor = x.ConversionFactor,
                    IsBidirectional = x.IsBidirectional,
                    IsStandardConversion = x.IsStandardConversion,
                    ConversionGroupName = x.ConversionGroupName,
                    FormulaNote = x.FormulaNote,
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
                    "Measurement conversion tidak ditemukan."
                ));
            }

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

            var response = await BuildCreateResponseAsync(entity.Id);

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
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
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
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Measurement conversion berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Measurement Conversion", Description = "Menghapus data measurement conversion", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("MeasurementConversion", "Delete")]
        public async Task<IActionResult> DeleteMeasurementConversion(Guid id)
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

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Measurement conversion berhasil dihapus."
            ));
        }

        private IQueryable<MstMeasurementConversion> BuildBaseQuery()
        {
            return _dbContext.Set<MstMeasurementConversion>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
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

            var fromMeasurementExists = await _dbContext.Set<MstMeasurement>()
                .AnyAsync(x => x.Id == fromMeasurementId && x.IsActive && !x.IsDelete);

            if (!fromMeasurementExists)
                return (false, "From measurement tidak valid atau tidak aktif.");

            var toMeasurementExists = await _dbContext.Set<MstMeasurement>()
                .AnyAsync(x => x.Id == toMeasurementId && x.IsActive && !x.IsDelete);

            if (!toMeasurementExists)
                return (false, "To measurement tidak valid atau tidak aktif.");

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

        private async Task<MeasurementConversionCreateResponse> BuildCreateResponseAsync(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new MeasurementConversionCreateResponse
                {
                    Id = x.Id,
                    FromMeasurementId = x.FromMeasurementId,
                    FromMeasurementName = x.FromMeasurement != null ? x.FromMeasurement.MeasurementName : string.Empty,
                    ToMeasurementId = x.ToMeasurementId,
                    ToMeasurementName = x.ToMeasurement != null ? x.ToMeasurement.MeasurementName : string.Empty,
                    ConversionFactor = x.ConversionFactor,
                    IsBidirectional = x.IsBidirectional,
                    IsStandardConversion = x.IsStandardConversion,
                    IsActive = x.IsActive
                })
                .FirstAsync();

            return data;
        }

        private static IQueryable<MstMeasurementConversion> ApplySorting(
            IQueryable<MstMeasurementConversion> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").ToLowerInvariant() switch
            {
                "createdatetime" => isDesc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "frommeasurementname" => isDesc
                    ? query.OrderByDescending(x => x.FromMeasurement != null ? x.FromMeasurement.MeasurementName : string.Empty)
                    : query.OrderBy(x => x.FromMeasurement != null ? x.FromMeasurement.MeasurementName : string.Empty),

                "tomeasurementname" => isDesc
                    ? query.OrderByDescending(x => x.ToMeasurement != null ? x.ToMeasurement.MeasurementName : string.Empty)
                    : query.OrderBy(x => x.ToMeasurement != null ? x.ToMeasurement.MeasurementName : string.Empty),

                "conversionfactor" => isDesc
                    ? query.OrderByDescending(x => x.ConversionFactor)
                    : query.OrderBy(x => x.ConversionFactor),

                "conversiongroupname" => isDesc
                    ? query.OrderByDescending(x => x.ConversionGroupName)
                    : query.OrderBy(x => x.ConversionGroupName),

                "isstandardconversion" => isDesc
                    ? query.OrderByDescending(x => x.IsStandardConversion)
                    : query.OrderBy(x => x.IsStandardConversion),

                "isactive" => isDesc
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => isDesc
                    ? query.OrderByDescending(x => x.SortOrder)
                        .ThenByDescending(x => x.FromMeasurement != null ? x.FromMeasurement.MeasurementName : string.Empty)
                    : query.OrderBy(x => x.SortOrder)
                        .ThenBy(x => x.FromMeasurement != null ? x.FromMeasurement.MeasurementName : string.Empty)
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static List<MeasurementConversionCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<MeasurementConversionCustomPeriodOptionResponse>
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

            var today = DateTime.UtcNow.Date;

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

        private class DateRangeResult
        {
            public bool IsValid { get; set; }
            public string? ErrorMessage { get; set; }
            public DateTime? Start { get; set; }
            public DateTime? EndExclusive { get; set; }
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
