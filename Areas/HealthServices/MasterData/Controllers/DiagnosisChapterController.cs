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

        private static readonly string[] IcdVersionOptions = { "ICD-9", "ICD-10" };

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
        [AccessAction("Read", "Read Diagnosis Chapter", Description = "Melihat metadata filter diagnosis chapter", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DiagnosisChapter", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new DiagnosisChapterFilterMetadataResponse
            {
                DefaultFilter = new DiagnosisChapterDefaultFilterResponse(),
                CustomPeriods = BuildCustomPeriodOptions(),
                SortOptions = BuildSortOptions(),
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                IcdVersionOptions = IcdVersionOptions
                    .Select(x => new DiagnosisChapterStringOptionResponse { Value = x, Label = x })
                    .ToList(),
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateFieldMetadata(),
                UpdateFields = BuildUpdateFieldMetadata(),
                ResetButtonLabel = "Reset"
            };

            await _loggerService.InfoAsync(LogCategory, "DiagnosisChapter.GetFilterMetadata", "Mengambil metadata filter diagnosis chapter.", result);

            return Ok(ApiResponse<DiagnosisChapterFilterMetadataResponse>.Ok(result, "Metadata filter diagnosis chapter berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisChapterSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Diagnosis Chapter", Description = "Melihat ringkasan diagnosis chapter", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DiagnosisChapter", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = _dbContext.Set<MstDiagnosisChapter>().AsNoTracking().Where(x => !x.IsDelete);

            var result = new DiagnosisChapterSummaryResponse
            {
                TotalDiagnosisChapter = await query.CountAsync(),
                ActiveDiagnosisChapter = await query.CountAsync(x => x.IsActive),
                InactiveDiagnosisChapter = await query.CountAsync(x => !x.IsActive),
                WithDiagnosisCodeRangeChapter = await query.CountAsync(x => x.DiagnosisCodeRangeStart != null || x.DiagnosisCodeRangeEnd != null),
                WithoutDiagnosisCodeRangeChapter = await query.CountAsync(x => x.DiagnosisCodeRangeStart == null && x.DiagnosisCodeRangeEnd == null),
                Icd10Chapter = await query.CountAsync(x => x.IcdVersion == "ICD-10"),
                Icd9Chapter = await query.CountAsync(x => x.IcdVersion == "ICD-9")
            };

            return Ok(ApiResponse<DiagnosisChapterSummaryResponse>.Ok(result, "Ringkasan diagnosis chapter berhasil diambil."));
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
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] string? icdVersion,
            [FromQuery] bool? hasDiagnosisCodeRange,
            [FromQuery] string? sortBy = "chapterCode",
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
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, dateRange.ErrorMessage ?? "Filter tanggal tidak valid."));
            }

            var query = BuildBaseQuery();
            query = ApplyDateFilter(query, dateRange);
            query = ApplyStandardFilter(query, search, isActive, icdVersion, hasDiagnosisCodeRange);

            var totalData = await query.CountAsync();
            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(entities.Select(x => x.CreateBy));
            var items = entities.Select(x => MapResponse(x, actorNames)).ToList();

            var result = new ResponseDiagnosisChapterPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseDiagnosisChapterPagedResult>.Ok(result, "Data diagnosis chapter berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisChapterOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Diagnosis Chapter", Description = "Melihat pilihan diagnosis chapter", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DiagnosisChapter", "Read")]
        public async Task<IActionResult> GetDiagnosisChapterOptions(
            [FromQuery] string? search,
            [FromQuery] string? icdVersion,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? sortBy = "chapterCode",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 100)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();
            if (onlyActive) query = query.Where(x => x.IsActive);
            query = ApplyStandardFilter(query, search, null, icdVersion, null);

            var totalData = await query.CountAsync();
            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new DiagnosisChapterOptionResponse
                {
                    Id = x.Id,
                    ChapterCode = x.ChapterCode,
                    ChapterName = x.ChapterName,
                    DiagnosisCodeRangeStart = x.DiagnosisCodeRangeStart,
                    DiagnosisCodeRangeEnd = x.DiagnosisCodeRangeEnd,
                    IcdVersion = x.IcdVersion,
                    IsActive = x.IsActive
                })
                .ToListAsync();

            return Ok(ApiResponse<DiagnosisChapterOptionPagedResponse>.Ok(new DiagnosisChapterOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Data pilihan diagnosis chapter berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisChapterDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Diagnosis Chapter", Description = "Melihat detail diagnosis chapter", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DiagnosisChapter", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BuildBaseQuery().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Diagnosis chapter tidak ditemukan."));
            }

            var actorNames = await GetActorNameMapAsync(new[] { entity.CreateBy, entity.UpdateBy });
            return Ok(ApiResponse<DiagnosisChapterDetailResponse>.Ok(MapDetailResponse(entity, actorNames), "Detail diagnosis chapter berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisChapterCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Diagnosis Chapter", Description = "Membuat diagnosis chapter", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("DiagnosisChapter", "Create")]
        public async Task<IActionResult> CreateDiagnosisChapter([FromBody] CreateDiagnosisChapterRequest request)
        {
            var normalized = NormalizeRequest(request);
            var validation = await ValidateCreateRequestAsync(normalized);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data diagnosis chapter tidak valid."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var entity = new MstDiagnosisChapter
            {
                Id = Guid.NewGuid(),
                ChapterCode = normalized.ChapterCode,
                ChapterName = normalized.ChapterName,
                DiagnosisCodeRangeStart = normalized.DiagnosisCodeRangeStart,
                DiagnosisCodeRangeEnd = normalized.DiagnosisCodeRangeEnd,
                IcdVersion = normalized.IcdVersion,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<MstDiagnosisChapter>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
            var result = ToCreateResponse(entity, actorNames);
            await _loggerService.InfoAsync(LogCategory, "DiagnosisChapter.CreateDiagnosisChapter", "Membuat diagnosis chapter.", result);

            return Ok(ApiResponse<DiagnosisChapterCreateResponse>.Ok(result, "Diagnosis chapter berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisChapterUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Diagnosis Chapter", Description = "Mengubah diagnosis chapter", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DiagnosisChapter", "Update")]
        public async Task<IActionResult> UpdateDiagnosisChapter(Guid id, [FromBody] UpdateDiagnosisChapterRequest request)
        {
            var entity = await _dbContext.Set<MstDiagnosisChapter>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Diagnosis chapter tidak ditemukan."));
            }

            var normalized = NormalizeRequest(request);
            var validation = await ValidateUpdateRequestAsync(id, normalized);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data diagnosis chapter tidak valid."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.ChapterCode = normalized.ChapterCode;
            entity.ChapterName = normalized.ChapterName;
            entity.DiagnosisCodeRangeStart = normalized.DiagnosisCodeRangeStart;
            entity.DiagnosisCodeRangeEnd = normalized.DiagnosisCodeRangeEnd;
            entity.IcdVersion = normalized.IcdVersion;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
            var result = ToUpdateResponse(entity, actorNames);
            await _loggerService.InfoAsync(LogCategory, "DiagnosisChapter.UpdateDiagnosisChapter", "Mengubah diagnosis chapter.", result);

            return Ok(ApiResponse<DiagnosisChapterUpdateResponse>.Ok(result, "Diagnosis chapter berhasil diubah."));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisChapterUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Diagnosis Chapter", Description = "Mengubah status diagnosis chapter", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DiagnosisChapter", "Update")]
        public async Task<IActionResult> UpdateDiagnosisChapterStatus(Guid id, [FromBody] UpdateDiagnosisChapterStatusRequest request)
        {
            var entity = await _dbContext.Set<MstDiagnosisChapter>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Diagnosis chapter tidak ditemukan."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
            return Ok(ApiResponse<DiagnosisChapterUpdateResponse>.Ok(ToUpdateResponse(entity, actorNames), "Status diagnosis chapter berhasil diubah."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisChapterDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Diagnosis Chapter", Description = "Menghapus diagnosis chapter", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("DiagnosisChapter", "Delete")]
        public async Task<IActionResult> DeleteDiagnosisChapter(Guid id, [FromBody] DeleteDiagnosisChapterRequest? request = null)
        {
            var entity = await _dbContext.Set<MstDiagnosisChapter>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Diagnosis chapter tidak ditemukan."));
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

            var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
            var result = new DiagnosisChapterDeleteResponse
            {
                Id = entity.Id,
                ChapterCode = entity.ChapterCode,
                ChapterName = entity.ChapterName,
                DeleteDateTime = entity.DeleteDateTime,
                DeleteBy = entity.DeleteBy == Guid.Empty ? null : entity.DeleteBy,
                DeleteByName = GetActorName(actorNames, entity.DeleteBy)
            };

            await _loggerService.InfoAsync(LogCategory, "DiagnosisChapter.DeleteDiagnosisChapter", "Menghapus diagnosis chapter.", new { result, request?.DeleteReason });
            return Ok(ApiResponse<DiagnosisChapterDeleteResponse>.Ok(result, "Diagnosis chapter berhasil dihapus."));
        }

        private IQueryable<MstDiagnosisChapter> BuildBaseQuery()
        {
            return _dbContext.Set<MstDiagnosisChapter>().AsNoTracking().Where(x => !x.IsDelete);
        }

        private static IQueryable<MstDiagnosisChapter> ApplyDateFilter(IQueryable<MstDiagnosisChapter> query, DateRangeResult dateRange)
        {
            if (dateRange.Start.HasValue) query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);
            if (dateRange.EndExclusive.HasValue) query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);
            return query;
        }

        private static IQueryable<MstDiagnosisChapter> ApplyStandardFilter(
            IQueryable<MstDiagnosisChapter> query,
            string? search,
            bool? isActive,
            string? icdVersion,
            bool? hasDiagnosisCodeRange)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.ChapterCode.ToLower().Contains(keyword) ||
                    x.ChapterName.ToLower().Contains(keyword) ||
                    (x.DiagnosisCodeRangeStart != null && x.DiagnosisCodeRangeStart.ToLower().Contains(keyword)) ||
                    (x.DiagnosisCodeRangeEnd != null && x.DiagnosisCodeRangeEnd.ToLower().Contains(keyword)) ||
                    x.IcdVersion.ToLower().Contains(keyword));
            }

            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
            if (!string.IsNullOrWhiteSpace(icdVersion)) query = query.Where(x => x.IcdVersion == NormalizeIcdVersion(icdVersion));
            if (hasDiagnosisCodeRange.HasValue)
            {
                query = hasDiagnosisCodeRange.Value
                    ? query.Where(x => x.DiagnosisCodeRangeStart != null || x.DiagnosisCodeRangeEnd != null)
                    : query.Where(x => x.DiagnosisCodeRangeStart == null && x.DiagnosisCodeRangeEnd == null);
            }

            return query;
        }

        private static IOrderedQueryable<MstDiagnosisChapter> ApplySorting(IQueryable<MstDiagnosisChapter> query, string? sortBy, string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "chapterCode").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "chaptercode" => isDesc ? query.OrderByDescending(x => x.ChapterCode) : query.OrderBy(x => x.ChapterCode),
                "chaptername" => isDesc ? query.OrderByDescending(x => x.ChapterName) : query.OrderBy(x => x.ChapterName),
                "icdversion" => isDesc ? query.OrderByDescending(x => x.IcdVersion).ThenBy(x => x.ChapterCode) : query.OrderBy(x => x.IcdVersion).ThenBy(x => x.ChapterCode),
                "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.ChapterCode) : query.OrderBy(x => x.IsActive).ThenBy(x => x.ChapterCode),
                _ => isDesc ? query.OrderByDescending(x => x.ChapterCode) : query.OrderBy(x => x.ChapterCode)
            };
        }

        private static DiagnosisChapterResponse MapResponse(MstDiagnosisChapter entity, IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new DiagnosisChapterResponse
            {
                Id = entity.Id,
                ChapterCode = entity.ChapterCode,
                ChapterName = entity.ChapterName,
                DiagnosisCodeRangeStart = entity.DiagnosisCodeRangeStart,
                DiagnosisCodeRangeEnd = entity.DiagnosisCodeRangeEnd,
                IcdVersion = entity.IcdVersion,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private static DiagnosisChapterDetailResponse MapDetailResponse(MstDiagnosisChapter entity, IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new DiagnosisChapterDetailResponse
            {
                Id = entity.Id,
                ChapterCode = entity.ChapterCode,
                ChapterName = entity.ChapterName,
                DiagnosisCodeRangeStart = entity.DiagnosisCodeRangeStart,
                DiagnosisCodeRangeEnd = entity.DiagnosisCodeRangeEnd,
                IcdVersion = entity.IcdVersion,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static DiagnosisChapterCreateResponse ToCreateResponse(MstDiagnosisChapter entity, IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new DiagnosisChapterCreateResponse
            {
                Id = entity.Id,
                ChapterCode = entity.ChapterCode,
                ChapterName = entity.ChapterName,
                IcdVersion = entity.IcdVersion,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private static DiagnosisChapterUpdateResponse ToUpdateResponse(MstDiagnosisChapter entity, IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new DiagnosisChapterUpdateResponse
            {
                Id = entity.Id,
                ChapterCode = entity.ChapterCode,
                ChapterName = entity.ChapterName,
                IcdVersion = entity.IcdVersion,
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static CreateDiagnosisChapterRequest NormalizeRequest(CreateDiagnosisChapterRequest request)
        {
            return new CreateDiagnosisChapterRequest
            {
                ChapterCode = NormalizeRequiredText(request.ChapterCode).ToUpperInvariant(),
                ChapterName = NormalizeRequiredText(request.ChapterName),
                DiagnosisCodeRangeStart = NormalizeNullableText(request.DiagnosisCodeRangeStart)?.ToUpperInvariant(),
                DiagnosisCodeRangeEnd = NormalizeNullableText(request.DiagnosisCodeRangeEnd)?.ToUpperInvariant(),
                IcdVersion = NormalizeIcdVersion(request.IcdVersion)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCreateRequestAsync(CreateDiagnosisChapterRequest request)
        {
            var validation = ValidateBasicRequest(request);
            if (!validation.IsValid) return validation;

            var exists = await _dbContext.Set<MstDiagnosisChapter>().AnyAsync(x => !x.IsDelete && x.IcdVersion == request.IcdVersion && x.ChapterCode == request.ChapterCode);
            if (exists) return (false, "Kode chapter untuk versi ICD tersebut sudah digunakan.");

            return (true, null);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateUpdateRequestAsync(Guid id, CreateDiagnosisChapterRequest request)
        {
            var validation = ValidateBasicRequest(request);
            if (!validation.IsValid) return validation;

            var duplicate = await _dbContext.Set<MstDiagnosisChapter>().AnyAsync(x => !x.IsDelete && x.Id != id && x.IcdVersion == request.IcdVersion && x.ChapterCode == request.ChapterCode);
            if (duplicate) return (false, "Kode chapter untuk versi ICD tersebut sudah digunakan.");

            return (true, null);
        }

        private static (bool IsValid, string? ErrorMessage) ValidateBasicRequest(CreateDiagnosisChapterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ChapterCode)) return (false, "Kode chapter wajib diisi.");
            if (string.IsNullOrWhiteSpace(request.ChapterName)) return (false, "Nama chapter wajib diisi.");
            if (!IcdVersionOptions.Contains(request.IcdVersion, StringComparer.OrdinalIgnoreCase)) return (false, "Versi ICD tidak valid.");
            return (true, null);
        }

        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(IEnumerable<Guid> actorIds)
        {
            var ids = actorIds.Where(x => x != Guid.Empty).Distinct().ToList();
            if (!ids.Any()) return new Dictionary<Guid, string?>();

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new { x.Id, Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode })
                .ToDictionaryAsync(x => x.Id, x => x.Name);
        }

        private static string? GetActorName(IReadOnlyDictionary<Guid, string?> actorNames, Guid actorId)
        {
            return actorId == Guid.Empty ? null : actorNames.TryGetValue(actorId, out var actorName) ? actorName : null;
        }

        private Guid GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(userIdValue, out var userId) ? userId : Guid.Empty;
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 25;
            if (pageSize > 100) pageSize = 100;
            return (pageNumber, pageSize);
        }

        private static DateRangeResult ResolveDateRange(DateTime? startDate, DateTime? endDate, string? customPeriod)
        {
            if (!string.IsNullOrWhiteSpace(customPeriod) && !string.Equals(customPeriod, "custom", StringComparison.OrdinalIgnoreCase))
            {
                var today = AppDateTimeHelper.OperationalDate();
                return customPeriod.Trim().ToLowerInvariant() switch
                {
                    "today" => DateRangeResult.Valid(today, today.AddDays(1)),
                    "last7days" => DateRangeResult.Valid(today.AddDays(-6), today.AddDays(1)),
                    "last30days" => DateRangeResult.Valid(today.AddDays(-29), today.AddDays(1)),
                    "thismonth" => DateRangeResult.Valid(new DateTime(today.Year, today.Month, 1), new DateTime(today.Year, today.Month, 1).AddMonths(1)),
                    "lastmonth" => DateRangeResult.Valid(new DateTime(today.Year, today.Month, 1).AddMonths(-1), new DateTime(today.Year, today.Month, 1)),
                    _ => DateRangeResult.Invalid("Custom period tidak dikenali.")
                };
            }

            var start = startDate?.Date;
            var endExclusive = endDate?.Date.AddDays(1);
            if (start.HasValue && endExclusive.HasValue && start.Value >= endExclusive.Value)
            {
                return DateRangeResult.Invalid("StartDate tidak boleh lebih besar atau sama dengan EndDate.");
            }

            return DateRangeResult.Valid(start, endExclusive);
        }

        private static List<DiagnosisChapterCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<DiagnosisChapterCustomPeriodOptionResponse>
            {
                new() { Value = "custom", Label = "Custom", Description = "Gunakan startDate dan endDate.", UsesStartDate = true, UsesEndDate = true },
                new() { Value = "today", Label = "Hari ini", Description = "Data yang dibuat hari ini.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last7days", Label = "7 hari terakhir", Description = "Data yang dibuat dalam 7 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last30days", Label = "30 hari terakhir", Description = "Data yang dibuat dalam 30 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thismonth", Label = "Bulan ini", Description = "Data yang dibuat pada bulan berjalan.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "lastmonth", Label = "Bulan lalu", Description = "Data yang dibuat pada bulan sebelumnya.", UsesStartDate = false, UsesEndDate = false }
            };
        }

        private static List<DiagnosisChapterSortOptionResponse> BuildSortOptions()
        {
            return new List<DiagnosisChapterSortOptionResponse>
            {
                new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                new() { Value = "chapterCode", Label = "Kode chapter" },
                new() { Value = "chapterName", Label = "Nama chapter" },
                new() { Value = "icdVersion", Label = "Versi ICD" },
                new() { Value = "isActive", Label = "Status aktif" }
            };
        }

        private static List<DiagnosisChapterQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<DiagnosisChapterQueryParameterInfoResponse>
            {
                new() { Name = "startDate", Type = "DateTime?", Description = "Tanggal awal filter berdasarkan CreateDateTime.", Example = "2026-06-01" },
                new() { Name = "endDate", Type = "DateTime?", Description = "Tanggal akhir filter berdasarkan CreateDateTime.", Example = "2026-06-30" },
                new() { Name = "customPeriod", Type = "string", Description = "Filter periode cepat: custom, today, last7days, last30days, thismonth, lastmonth.", Example = "thismonth" },
                new() { Name = "search", Type = "string", Description = "Cari berdasarkan kode chapter, nama chapter, range kode, atau versi ICD.", Example = "A00" },
                new() { Name = "isActive", Type = "bool?", Description = "Filter status aktif.", Example = "true" },
                new() { Name = "icdVersion", Type = "string", Description = "Filter versi ICD.", Example = "ICD-10" },
                new() { Name = "hasDiagnosisCodeRange", Type = "bool?", Description = "Filter chapter yang memiliki range kode diagnosis.", Example = "true" },
                new() { Name = "sortBy", Type = "string", Description = "Kolom sorting.", Example = "chapterCode" },
                new() { Name = "sortDirection", Type = "string", Description = "Arah sorting: asc atau desc.", Example = "asc" },
                new() { Name = "pageNumber", Type = "int", Description = "Nomor halaman.", Example = "1" },
                new() { Name = "pageSize", Type = "int", Description = "Jumlah data per halaman, maksimal 100.", Example = "25" }
            };
        }

        private static List<DiagnosisChapterFormFieldMetadataResponse> BuildCreateFieldMetadata()
        {
            return BuildFieldMetadata(isUpdate: false);
        }

        private static List<DiagnosisChapterFormFieldMetadataResponse> BuildUpdateFieldMetadata()
        {
            return BuildFieldMetadata(isUpdate: true);
        }

        private static List<DiagnosisChapterFormFieldMetadataResponse> BuildFieldMetadata(bool isUpdate)
        {
            var fields = new List<DiagnosisChapterFormFieldMetadataResponse>
            {
                new() { Name = "chapterCode", Label = "Kode Chapter", Section = "Basic", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 50, Example = "I", SortOrder = 1 },
                new() { Name = "chapterName", Label = "Nama Chapter", Section = "Basic", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 250, Example = "Certain infectious and parasitic diseases", SortOrder = 2 },
                new() { Name = "diagnosisCodeRangeStart", Label = "Range Kode Awal", Section = "Range", InputType = "text", MaxLength = 50, Example = "A00", SortOrder = 3 },
                new() { Name = "diagnosisCodeRangeEnd", Label = "Range Kode Akhir", Section = "Range", InputType = "text", MaxLength = 50, Example = "B99", SortOrder = 4 },
                new() { Name = "icdVersion", Label = "Versi ICD", Section = "Basic", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", OptionsSource = "icdVersionOptions", Example = "ICD-10", SortOrder = 5 }
            };

            if (isUpdate)
            {
                fields.Add(new DiagnosisChapterFormFieldMetadataResponse { Name = "isActive", Label = "Status Aktif", Section = "Status", InputType = "switch", SortOrder = 99 });
            }

            return fields.OrderBy(x => x.SortOrder).ToList();
        }

        private static string NormalizeIcdVersion(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "ICD-10";
            var normalized = value.Trim().ToUpperInvariant().Replace(" ", "");
            return normalized switch
            {
                "ICD9" => "ICD-9",
                "ICD-9" => "ICD-9",
                "ICD10" => "ICD-10",
                "ICD-10" => "ICD-10",
                _ => value.Trim()
            };
        }

        private static string NormalizeRequiredText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private sealed class DateRangeResult
        {
            public bool IsValid { get; private init; }
            public DateTime? Start { get; private init; }
            public DateTime? EndExclusive { get; private init; }
            public string? ErrorMessage { get; private init; }
            public static DateRangeResult Valid(DateTime? start, DateTime? endExclusive) => new() { IsValid = true, Start = start, EndExclusive = endExclusive };
            public static DateRangeResult Invalid(string errorMessage) => new() { IsValid = false, ErrorMessage = errorMessage };
        }
    }
}
