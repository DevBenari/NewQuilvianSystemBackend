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

using ResponseDiagnosisChapterPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.DiagnosisChapterResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/diagnosis-chapters")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Diagnosis Chapter",
        AreaName = "HealthServices",
        ControllerName = "DiagnosisChapter",
        Description = "Health service master data diagnosis chapter",
        SortOrder = 10
    )]
    [Tags("Health Services / Master Data / Diagnosis Chapter")]
    public class DiagnosisChapterController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public DiagnosisChapterController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisChapterFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Diagnosis Chapter", Description = "Melihat data diagnosis chapter", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DiagnosisChapter", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new DiagnosisChapterFilterMetadataResponse
            {
                DefaultFilter = new DiagnosisChapterDefaultFilterResponse(),
                CustomPeriods = BuildCustomPeriodOptions(),
                SortOptions = new List<DiagnosisChapterSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "chapterCode", Label = "Kode chapter" },
                    new() { Value = "chapterName", Label = "Nama chapter" },
                    new() { Value = "diagnosisCodeRangeStart", Label = "Awal range kode diagnosis" },
                    new() { Value = "diagnosisCodeRangeEnd", Label = "Akhir range kode diagnosis" },
                    new() { Value = "icdVersion", Label = "Versi ICD" },
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
                "DiagnosisChapter.GetFilterMetadata",
                "Mengambil metadata filter diagnosis chapter.",
                result
            );

            return Ok(ApiResponse<DiagnosisChapterFilterMetadataResponse>.Ok(
                result,
                "Metadata filter diagnosis chapter berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisChapterSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Diagnosis Chapter", Description = "Melihat data diagnosis chapter", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DiagnosisChapter", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = _dbContext.Set<MstDiagnosisChapter>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new DiagnosisChapterSummaryResponse
            {
                TotalDiagnosisChapter = await query.CountAsync(),
                ActiveDiagnosisChapter = await query.CountAsync(x => x.IsActive),
                InactiveDiagnosisChapter = await query.CountAsync(x => !x.IsActive),
                HasDiagnosisCodeRangeChapter = await query.CountAsync(x =>
                    x.DiagnosisCodeRangeStart != null || x.DiagnosisCodeRangeEnd != null),
                WithoutDiagnosisCodeRangeChapter = await query.CountAsync(x =>
                    x.DiagnosisCodeRangeStart == null && x.DiagnosisCodeRangeEnd == null)
            };

            return Ok(ApiResponse<DiagnosisChapterSummaryResponse>.Ok(
                result,
                "Ringkasan diagnosis chapter berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseDiagnosisChapterPagedResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Diagnosis Chapter", Description = "Melihat data diagnosis chapter", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DiagnosisChapter", "Read")]
        public async Task<IActionResult> GetDiagnosisChapters(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
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

            var query = _dbContext.Set<MstDiagnosisChapter>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (dateRange.Start.HasValue)
                query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);

            if (dateRange.EndExclusive.HasValue)
                query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ChapterCode.ToLower().Contains(keyword) ||
                    x.ChapterName.ToLower().Contains(keyword) ||
                    x.IcdVersion.ToLower().Contains(keyword) ||
                    (x.DiagnosisCodeRangeStart != null && x.DiagnosisCodeRangeStart.ToLower().Contains(keyword)) ||
                    (x.DiagnosisCodeRangeEnd != null && x.DiagnosisCodeRangeEnd.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new DiagnosisChapterResponse
                {
                    Id = x.Id,
                    ChapterCode = x.ChapterCode,
                    ChapterName = x.ChapterName,
                    DiagnosisCodeRangeStart = x.DiagnosisCodeRangeStart,
                    DiagnosisCodeRangeEnd = x.DiagnosisCodeRangeEnd,
                    IcdVersion = x.IcdVersion,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new ResponseDiagnosisChapterPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseDiagnosisChapterPagedResult>.Ok(
                result,
                "Data diagnosis chapter berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisChapterOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Diagnosis Chapter", Description = "Melihat data pilihan diagnosis chapter", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DiagnosisChapter", "Read")]
        public async Task<IActionResult> GetDiagnosisChapterOptions(
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = _dbContext.Set<MstDiagnosisChapter>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ChapterCode.ToLower().Contains(keyword) ||
                    x.ChapterName.ToLower().Contains(keyword) ||
                    x.IcdVersion.ToLower().Contains(keyword) ||
                    (x.DiagnosisCodeRangeStart != null && x.DiagnosisCodeRangeStart.ToLower().Contains(keyword)) ||
                    (x.DiagnosisCodeRangeEnd != null && x.DiagnosisCodeRangeEnd.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ChapterCode)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new DiagnosisChapterOptionResponse
                {
                    Id = x.Id,
                    ChapterCode = x.ChapterCode,
                    ChapterName = x.ChapterName,
                    DiagnosisCodeRangeStart = x.DiagnosisCodeRangeStart,
                    DiagnosisCodeRangeEnd = x.DiagnosisCodeRangeEnd,
                    IcdVersion = x.IcdVersion
                })
                .ToListAsync();

            var result = new DiagnosisChapterOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<DiagnosisChapterOptionPagedResponse>.Ok(
                result,
                "Data pilihan diagnosis chapter berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisChapterDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Diagnosis Chapter", Description = "Melihat data diagnosis chapter", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DiagnosisChapter", "Read")]
        public async Task<IActionResult> GetDiagnosisChapterById(Guid id)
        {
            var data = await _dbContext.Set<MstDiagnosisChapter>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new DiagnosisChapterDetailResponse
                {
                    Id = x.Id,
                    ChapterCode = x.ChapterCode,
                    ChapterName = x.ChapterName,
                    DiagnosisCodeRangeStart = x.DiagnosisCodeRangeStart,
                    DiagnosisCodeRangeEnd = x.DiagnosisCodeRangeEnd,
                    IcdVersion = x.IcdVersion,
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
                    "Diagnosis chapter tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<DiagnosisChapterDetailResponse>.Ok(
                data,
                "Detail diagnosis chapter berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisChapterCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Diagnosis Chapter", Description = "Membuat data diagnosis chapter", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("DiagnosisChapter", "Create")]
        public async Task<IActionResult> CreateDiagnosisChapter([FromBody] CreateDiagnosisChapterRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                chapterCode: request.ChapterCode,
                chapterName: request.ChapterName,
                icdVersion: request.IcdVersion,
                diagnosisCodeRangeStart: request.DiagnosisCodeRangeStart,
                diagnosisCodeRangeEnd: request.DiagnosisCodeRangeEnd
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data diagnosis chapter tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new MstDiagnosisChapter
            {
                Id = Guid.NewGuid(),
                ChapterCode = request.ChapterCode.Trim().ToUpperInvariant(),
                ChapterName = request.ChapterName.Trim(),
                DiagnosisCodeRangeStart = NormalizeNullableText(request.DiagnosisCodeRangeStart)?.ToUpperInvariant(),
                DiagnosisCodeRangeEnd = NormalizeNullableText(request.DiagnosisCodeRangeEnd)?.ToUpperInvariant(),
                IcdVersion = NormalizeIcdVersion(request.IcdVersion),
                SortOrder = request.SortOrder,
                Description = NormalizeNullableText(request.Description),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstDiagnosisChapter>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = ToCreateUpdateResponse(entity);

            await _loggerService.InfoAsync(
                LogCategory,
                "DiagnosisChapter.CreateDiagnosisChapter",
                "Membuat data diagnosis chapter.",
                response
            );

            return Ok(ApiResponse<DiagnosisChapterCreateResponse>.Ok(
                response,
                "Diagnosis chapter berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisChapterUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Diagnosis Chapter", Description = "Mengubah data diagnosis chapter", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DiagnosisChapter", "Update")]
        public async Task<IActionResult> UpdateDiagnosisChapter(Guid id, [FromBody] UpdateDiagnosisChapterRequest request)
        {
            var entity = await _dbContext.Set<MstDiagnosisChapter>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Diagnosis chapter tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(
                excludeId: id,
                chapterCode: request.ChapterCode,
                chapterName: request.ChapterName,
                icdVersion: request.IcdVersion,
                diagnosisCodeRangeStart: request.DiagnosisCodeRangeStart,
                diagnosisCodeRangeEnd: request.DiagnosisCodeRangeEnd
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data diagnosis chapter tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.ChapterCode = request.ChapterCode.Trim().ToUpperInvariant();
            entity.ChapterName = request.ChapterName.Trim();
            entity.DiagnosisCodeRangeStart = NormalizeNullableText(request.DiagnosisCodeRangeStart)?.ToUpperInvariant();
            entity.DiagnosisCodeRangeEnd = NormalizeNullableText(request.DiagnosisCodeRangeEnd)?.ToUpperInvariant();
            entity.IcdVersion = NormalizeIcdVersion(request.IcdVersion);
            entity.SortOrder = request.SortOrder;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var response = new DiagnosisChapterUpdateResponse
            {
                Id = entity.Id,
                ChapterCode = entity.ChapterCode,
                ChapterName = entity.ChapterName,
                DiagnosisCodeRangeStart = entity.DiagnosisCodeRangeStart,
                DiagnosisCodeRangeEnd = entity.DiagnosisCodeRangeEnd,
                IcdVersion = entity.IcdVersion,
                IsActive = entity.IsActive
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "DiagnosisChapter.UpdateDiagnosisChapter",
                "Mengubah data diagnosis chapter.",
                response
            );

            return Ok(ApiResponse<DiagnosisChapterUpdateResponse>.Ok(
                response,
                "Diagnosis chapter berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Diagnosis Chapter", Description = "Menghapus data diagnosis chapter", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("DiagnosisChapter", "Delete")]
        public async Task<IActionResult> DeleteDiagnosisChapter(Guid id)
        {
            var entity = await _dbContext.Set<MstDiagnosisChapter>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Diagnosis chapter tidak ditemukan."
                ));
            }

            var isUsedByDiagnosis = await _dbContext.Set<MstDiagnosis>()
                .AnyAsync(x => x.DiagnosisChapterId == id && !x.IsDelete);

            if (isUsedByDiagnosis)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Diagnosis chapter tidak dapat dihapus karena sudah digunakan oleh diagnosis."
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

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Diagnosis chapter berhasil dihapus."
            ));
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            string chapterCode,
            string chapterName,
            string icdVersion,
            string? diagnosisCodeRangeStart,
            string? diagnosisCodeRangeEnd)
        {
            if (string.IsNullOrWhiteSpace(chapterCode))
                return (false, "Kode chapter wajib diisi.");

            if (string.IsNullOrWhiteSpace(chapterName))
                return (false, "Nama chapter wajib diisi.");

            if (string.IsNullOrWhiteSpace(icdVersion))
                return (false, "Versi ICD wajib diisi.");

            var normalizedCode = chapterCode.Trim().ToUpperInvariant();
            var normalizedName = chapterName.Trim().ToLower();
            var normalizedIcdVersion = NormalizeIcdVersion(icdVersion);
            var normalizedRangeStart = NormalizeNullableText(diagnosisCodeRangeStart)?.ToUpperInvariant();
            var normalizedRangeEnd = NormalizeNullableText(diagnosisCodeRangeEnd)?.ToUpperInvariant();

            if (!string.IsNullOrWhiteSpace(normalizedRangeStart) && !string.IsNullOrWhiteSpace(normalizedRangeEnd))
            {
                var comparison = string.Compare(normalizedRangeStart, normalizedRangeEnd, StringComparison.OrdinalIgnoreCase);

                if (comparison > 0)
                    return (false, "Range awal kode diagnosis tidak boleh lebih besar dari range akhir.");
            }

            var duplicateCodeQuery = _dbContext.Set<MstDiagnosisChapter>()
                .AsNoTracking()
                .Where(x => x.ChapterCode.ToUpper() == normalizedCode);

            if (excludeId.HasValue)
                duplicateCodeQuery = duplicateCodeQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateCodeQuery.AnyAsync())
                return (false, "Kode chapter sudah digunakan.");

            var duplicateNameQuery = _dbContext.Set<MstDiagnosisChapter>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.ChapterName.ToLower() == normalizedName &&
                    x.IcdVersion.ToUpper() == normalizedIcdVersion.ToUpper());

            if (excludeId.HasValue)
                duplicateNameQuery = duplicateNameQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateNameQuery.AnyAsync())
                return (false, "Nama chapter pada versi ICD tersebut sudah digunakan.");

            return (true, null);
        }

        private static DiagnosisChapterCreateResponse ToCreateUpdateResponse(MstDiagnosisChapter entity)
        {
            return new DiagnosisChapterCreateResponse
            {
                Id = entity.Id,
                ChapterCode = entity.ChapterCode,
                ChapterName = entity.ChapterName,
                DiagnosisCodeRangeStart = entity.DiagnosisCodeRangeStart,
                DiagnosisCodeRangeEnd = entity.DiagnosisCodeRangeEnd,
                IcdVersion = entity.IcdVersion,
                IsActive = entity.IsActive
            };
        }

        private static IQueryable<MstDiagnosisChapter> ApplySorting(
            IQueryable<MstDiagnosisChapter> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").ToLowerInvariant() switch
            {
                "createdatetime" => isDesc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "chaptercode" => isDesc
                    ? query.OrderByDescending(x => x.ChapterCode)
                    : query.OrderBy(x => x.ChapterCode),

                "chaptername" => isDesc
                    ? query.OrderByDescending(x => x.ChapterName)
                    : query.OrderBy(x => x.ChapterName),

                "diagnosiscoderangestart" => isDesc
                    ? query.OrderByDescending(x => x.DiagnosisCodeRangeStart)
                    : query.OrderBy(x => x.DiagnosisCodeRangeStart),

                "diagnosiscoderangeend" => isDesc
                    ? query.OrderByDescending(x => x.DiagnosisCodeRangeEnd)
                    : query.OrderBy(x => x.DiagnosisCodeRangeEnd),

                "icdversion" => isDesc
                    ? query.OrderByDescending(x => x.IcdVersion)
                    : query.OrderBy(x => x.IcdVersion),

                "isactive" => isDesc
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => isDesc
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.ChapterCode)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.ChapterCode)
            };
        }

        private static DateRangeResult ResolveDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            if (!string.IsNullOrWhiteSpace(customPeriod) &&
                !string.Equals(customPeriod, "custom", StringComparison.OrdinalIgnoreCase))
            {
                var today = DateTime.UtcNow.Date;
                var period = customPeriod.Trim().ToLowerInvariant();

                return period switch
                {
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

        private static List<DiagnosisChapterCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<DiagnosisChapterCustomPeriodOptionResponse>
            {
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

        private static List<DiagnosisChapterQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<DiagnosisChapterQueryParameterInfoResponse>
            {
                new() { Name = "startDate", Type = "date", Description = "Tanggal awal filter berdasarkan CreateDateTime.", Example = "2026-06-01" },
                new() { Name = "endDate", Type = "date", Description = "Tanggal akhir filter berdasarkan CreateDateTime.", Example = "2026-06-30" },
                new() { Name = "customPeriod", Type = "string", Description = "Preset periode. Jika bukan custom, startDate dan endDate diabaikan.", Example = "thisMonth" },
                new() { Name = "isActive", Type = "boolean", Description = "Filter status aktif." },
                new() { Name = "search", Type = "string", Description = "Pencarian kode chapter, nama chapter, range kode diagnosis, versi ICD, atau deskripsi." },
                new() { Name = "sortBy", Type = "string", Description = "Kolom sorting." },
                new() { Name = "sortDirection", Type = "string", Description = "asc atau desc.", Example = "asc" },
                new() { Name = "pageNumber", Type = "integer", Description = "Nomor halaman.", Example = "1" },
                new() { Name = "pageSize", Type = "integer", Description = "Jumlah data per halaman.", Example = "25" }
            };
        }

        private static List<DiagnosisChapterFormFieldMetadataResponse> BuildCreateFieldMetadata()
        {
            return new List<DiagnosisChapterFormFieldMetadataResponse>
            {
                new() { Name = "chapterCode", Label = "Kode chapter", DataType = "string", InputType = "text", Required = true, IsReadonly = false, Placeholder = "I", Description = "Kode resmi chapter ICD, contoh: I, II, III." },
                new() { Name = "chapterName", Label = "Nama chapter", DataType = "string", InputType = "text", Required = true, IsReadonly = false, Placeholder = "Certain infectious and parasitic diseases" },
                new() { Name = "diagnosisCodeRangeStart", Label = "Range kode diagnosis awal", DataType = "string", InputType = "text", Required = false, IsReadonly = false, Placeholder = "A00" },
                new() { Name = "diagnosisCodeRangeEnd", Label = "Range kode diagnosis akhir", DataType = "string", InputType = "text", Required = false, IsReadonly = false, Placeholder = "B99" },
                new() { Name = "icdVersion", Label = "Versi ICD", DataType = "string", InputType = "text", Required = true, IsReadonly = false, Placeholder = "ICD-10" },
                new() { Name = "sortOrder", Label = "Urutan", DataType = "integer", InputType = "number", Required = false, IsReadonly = false },
                new() { Name = "description", Label = "Deskripsi", DataType = "string", InputType = "textarea", Required = false, IsReadonly = false }
            };
        }

        private static List<DiagnosisChapterFormFieldMetadataResponse> BuildUpdateFieldMetadata()
        {
            var fields = BuildCreateFieldMetadata();

            fields.Add(new DiagnosisChapterFormFieldMetadataResponse
            {
                Name = "isActive",
                Label = "Status aktif",
                DataType = "boolean",
                InputType = "switch",
                Required = false,
                IsReadonly = false
            });

            return fields;
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static string NormalizeIcdVersion(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "ICD-10"
                : value.Trim().ToUpperInvariant();
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
