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

        private static readonly string[] IcdVersionOptions =
        {
            "ICD-10",
            "ICD-11"
        };

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
                SortOptions = BuildSortOptions(),
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                IcdVersionOptions = IcdVersionOptions
                    .Select(x => new DiagnosisChapterStringOptionResponse { Value = x, Label = x })
                    .ToList(),
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateFieldMetadata(),
                UpdateFields = BuildUpdateFieldMetadata(),
                ResetButtonLabel = "Reset Filter"
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
            var query = BuildBaseQuery();

            var result = new DiagnosisChapterSummaryResponse
            {
                TotalDiagnosisChapter = await query.CountAsync(),
                ActiveDiagnosisChapter = await query.CountAsync(x => x.IsActive),
                InactiveDiagnosisChapter = await query.CountAsync(x => !x.IsActive),
                HasDiagnosisCodeRangeChapter = await query.CountAsync(x =>
                    x.DiagnosisCodeRangeStart != null || x.DiagnosisCodeRangeEnd != null),
                WithoutDiagnosisCodeRangeChapter = await query.CountAsync(x =>
                    x.DiagnosisCodeRangeStart == null && x.DiagnosisCodeRangeEnd == null),
                Icd10Chapter = await query.CountAsync(x => x.IcdVersion == "ICD-10"),
                Icd11Chapter = await query.CountAsync(x => x.IcdVersion == "ICD-11")
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
            [FromQuery] string? icdVersion,
            [FromQuery] bool? hasDiagnosisCodeRange,
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
            query = ApplyStandardFilter(query, isActive, icdVersion, hasDiagnosisCodeRange, search);

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(
                entities
                    .SelectMany(x => new[] { x.CreateBy, x.UpdateBy })
                    .Where(x => x != Guid.Empty)
            );

            var items = entities
                .Select(x => MapResponse(x, actorNames))
                .ToList();

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
            [FromQuery] string? icdVersion = null,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();
            query = ApplyStandardFilter(query, onlyActive ? true : null, icdVersion, null, search);

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
                    IcdVersion = x.IcdVersion,
                    IsActive = x.IsActive
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
        [AccessAction("Read", "Read Diagnosis Chapter", Description = "Melihat detail diagnosis chapter", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DiagnosisChapter", "Read")]
        public async Task<IActionResult> GetDiagnosisChapterById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Diagnosis chapter tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var data = MapDetailResponse(entity, actorNames);

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
            var validation = await ValidateRequestAsync(null, request);

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
                DiagnosisCodeRangeStart = NormalizeNullableUpperText(request.DiagnosisCodeRangeStart),
                DiagnosisCodeRangeEnd = NormalizeNullableUpperText(request.DiagnosisCodeRangeEnd),
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

            var response = ToCreateResponse(entity);

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

            var validation = await ValidateRequestAsync(id, request);

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
            entity.DiagnosisCodeRangeStart = NormalizeNullableUpperText(request.DiagnosisCodeRangeStart);
            entity.DiagnosisCodeRangeEnd = NormalizeNullableUpperText(request.DiagnosisCodeRangeEnd);
            entity.IcdVersion = NormalizeIcdVersion(request.IcdVersion);
            entity.SortOrder = request.SortOrder;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var response = ToUpdateResponse(entity);

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

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisChapterUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Diagnosis Chapter Status", Description = "Mengubah status diagnosis chapter", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DiagnosisChapter", "Update")]
        public async Task<IActionResult> UpdateDiagnosisChapterStatus(
            Guid id,
            [FromBody] UpdateDiagnosisChapterStatusRequest request)
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

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var response = ToUpdateResponse(entity);

            return Ok(ApiResponse<DiagnosisChapterUpdateResponse>.Ok(
                response,
                "Status diagnosis chapter berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisChapterDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Diagnosis Chapter", Description = "Menghapus data diagnosis chapter", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("DiagnosisChapter", "Delete")]
        public async Task<IActionResult> DeleteDiagnosisChapter(
            Guid id,
            [FromBody] DeleteDiagnosisChapterRequest? request = null)
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

            if (!string.IsNullOrWhiteSpace(request?.DeleteReason))
            {
                entity.Description = request.DeleteReason.Trim();
            }

            await _dbContext.SaveChangesAsync();

            var response = new DiagnosisChapterDeleteResponse
            {
                Id = entity.Id,
                ChapterCode = entity.ChapterCode,
                ChapterName = entity.ChapterName,
                DeleteDateTime = entity.DeleteDateTime
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "DiagnosisChapter.DeleteDiagnosisChapter",
                "Menghapus data diagnosis chapter.",
                response
            );

            return Ok(ApiResponse<DiagnosisChapterDeleteResponse>.Ok(
                response,
                "Diagnosis chapter berhasil dihapus."
            ));
        }

        private IQueryable<MstDiagnosisChapter> BuildBaseQuery()
        {
            return _dbContext.Set<MstDiagnosisChapter>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstDiagnosisChapter> ApplyDateFilter(
            IQueryable<MstDiagnosisChapter> query,
            DateRangeResult dateRange)
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

        private static IQueryable<MstDiagnosisChapter> ApplyStandardFilter(
            IQueryable<MstDiagnosisChapter> query,
            bool? isActive,
            string? icdVersion,
            bool? hasDiagnosisCodeRange,
            string? search)
        {
            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(icdVersion))
            {
                var normalizedIcdVersion = NormalizeIcdVersion(icdVersion);
                query = query.Where(x => x.IcdVersion == normalizedIcdVersion);
            }

            if (hasDiagnosisCodeRange.HasValue)
            {
                query = hasDiagnosisCodeRange.Value
                    ? query.Where(x => x.DiagnosisCodeRangeStart != null || x.DiagnosisCodeRangeEnd != null)
                    : query.Where(x => x.DiagnosisCodeRangeStart == null && x.DiagnosisCodeRangeEnd == null);
            }

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

            return query;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateDiagnosisChapterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ChapterCode))
            {
                return (false, "Kode chapter wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(request.ChapterName))
            {
                return (false, "Nama chapter wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(request.IcdVersion))
            {
                return (false, "Versi ICD wajib diisi.");
            }

            var normalizedCode = request.ChapterCode.Trim().ToUpperInvariant();
            var normalizedName = request.ChapterName.Trim().ToLower();
            var normalizedIcdVersion = NormalizeIcdVersion(request.IcdVersion);
            var rangeStart = NormalizeNullableUpperText(request.DiagnosisCodeRangeStart);
            var rangeEnd = NormalizeNullableUpperText(request.DiagnosisCodeRangeEnd);

            if (!string.IsNullOrWhiteSpace(rangeStart) &&
                !string.IsNullOrWhiteSpace(rangeEnd) &&
                string.Compare(rangeStart, rangeEnd, StringComparison.OrdinalIgnoreCase) > 0)
            {
                return (false, "Range awal kode diagnosis tidak boleh lebih besar dari range akhir.");
            }

            var duplicateCodeQuery = _dbContext.Set<MstDiagnosisChapter>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.ChapterCode.ToUpper() == normalizedCode &&
                    x.IcdVersion.ToUpper() == normalizedIcdVersion.ToUpper());

            if (excludeId.HasValue)
            {
                duplicateCodeQuery = duplicateCodeQuery.Where(x => x.Id != excludeId.Value);
            }

            if (await duplicateCodeQuery.AnyAsync())
            {
                return (false, "Kode chapter pada versi ICD tersebut sudah digunakan.");
            }

            var duplicateNameQuery = _dbContext.Set<MstDiagnosisChapter>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.ChapterName.ToLower() == normalizedName &&
                    x.IcdVersion.ToUpper() == normalizedIcdVersion.ToUpper());

            if (excludeId.HasValue)
            {
                duplicateNameQuery = duplicateNameQuery.Where(x => x.Id != excludeId.Value);
            }

            if (await duplicateNameQuery.AnyAsync())
            {
                return (false, "Nama chapter pada versi ICD tersebut sudah digunakan.");
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

        private static DiagnosisChapterResponse MapResponse(
            MstDiagnosisChapter entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new DiagnosisChapterResponse
            {
                Id = entity.Id,
                ChapterCode = entity.ChapterCode,
                ChapterName = entity.ChapterName,
                DiagnosisCodeRangeStart = entity.DiagnosisCodeRangeStart,
                DiagnosisCodeRangeEnd = entity.DiagnosisCodeRangeEnd,
                IcdVersion = entity.IcdVersion,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private static DiagnosisChapterDetailResponse MapDetailResponse(
            MstDiagnosisChapter entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new DiagnosisChapterDetailResponse
            {
                Id = entity.Id,
                ChapterCode = entity.ChapterCode,
                ChapterName = entity.ChapterName,
                DiagnosisCodeRangeStart = entity.DiagnosisCodeRangeStart,
                DiagnosisCodeRangeEnd = entity.DiagnosisCodeRangeEnd,
                IcdVersion = entity.IcdVersion,
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

        private static DiagnosisChapterCreateResponse ToCreateResponse(MstDiagnosisChapter entity)
        {
            return new DiagnosisChapterCreateResponse
            {
                Id = entity.Id,
                ChapterCode = entity.ChapterCode,
                ChapterName = entity.ChapterName,
                DiagnosisCodeRangeStart = entity.DiagnosisCodeRangeStart,
                DiagnosisCodeRangeEnd = entity.DiagnosisCodeRangeEnd,
                IcdVersion = entity.IcdVersion,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime
            };
        }

        private static DiagnosisChapterUpdateResponse ToUpdateResponse(MstDiagnosisChapter entity)
        {
            return new DiagnosisChapterUpdateResponse
            {
                Id = entity.Id,
                ChapterCode = entity.ChapterCode,
                ChapterName = entity.ChapterName,
                DiagnosisCodeRangeStart = entity.DiagnosisCodeRangeStart,
                DiagnosisCodeRangeEnd = entity.DiagnosisCodeRangeEnd,
                IcdVersion = entity.IcdVersion,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                UpdateDateTime = entity.UpdateDateTime
            };
        }

        private static IOrderedQueryable<MstDiagnosisChapter> ApplySorting(
            IQueryable<MstDiagnosisChapter> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").Trim().ToLowerInvariant() switch
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
                var today = AppDateTimeHelper.OperationalDate();
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
            {
                return DateRangeResult.Invalid("StartDate tidak boleh lebih besar dari EndDate.");
            }

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

        private static List<DiagnosisChapterSortOptionResponse> BuildSortOptions()
        {
            return new List<DiagnosisChapterSortOptionResponse>
            {
                new() { Value = "sortOrder", Label = "Urutan" },
                new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                new() { Value = "chapterCode", Label = "Kode chapter" },
                new() { Value = "chapterName", Label = "Nama chapter" },
                new() { Value = "diagnosisCodeRangeStart", Label = "Awal range kode diagnosis" },
                new() { Value = "diagnosisCodeRangeEnd", Label = "Akhir range kode diagnosis" },
                new() { Value = "icdVersion", Label = "Versi ICD" },
                new() { Value = "isActive", Label = "Status aktif" }
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
                new() { Name = "icdVersion", Type = "string", Description = "Filter versi ICD.", Example = "ICD-10" },
                new() { Name = "hasDiagnosisCodeRange", Type = "boolean", Description = "Filter chapter yang memiliki range kode diagnosis." },
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
                new() { Name = "chapterCode", Label = "Kode chapter", DataType = "string", InputType = "text", Required = true, IsReadonly = false, Placeholder = "I", Description = "Kode resmi chapter ICD, contoh: I, II, III.", SortOrder = 1 },
                new() { Name = "chapterName", Label = "Nama chapter", DataType = "string", InputType = "text", Required = true, IsReadonly = false, Placeholder = "Certain infectious and parasitic diseases", SortOrder = 2 },
                new() { Name = "diagnosisCodeRangeStart", Label = "Range kode diagnosis awal", DataType = "string", InputType = "text", Required = false, IsReadonly = false, Placeholder = "A00", SortOrder = 3 },
                new() { Name = "diagnosisCodeRangeEnd", Label = "Range kode diagnosis akhir", DataType = "string", InputType = "text", Required = false, IsReadonly = false, Placeholder = "B99", SortOrder = 4 },
                new() { Name = "icdVersion", Label = "Versi ICD", DataType = "string", InputType = "select", Required = true, IsReadonly = false, Placeholder = "ICD-10", SortOrder = 5 },
                new() { Name = "sortOrder", Label = "Urutan", DataType = "integer", InputType = "number", Required = false, IsReadonly = false, SortOrder = 6 },
                new() { Name = "description", Label = "Deskripsi", DataType = "string", InputType = "textarea", Required = false, IsReadonly = false, SortOrder = 7 }
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
                IsReadonly = false,
                SortOrder = 99
            });

            return fields.OrderBy(x => x.SortOrder).ToList();
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

        private static string? NormalizeNullableUpperText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim().ToUpperInvariant();
        }

        private Guid GetCurrentUserId()
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("user_id");

            return Guid.TryParse(userIdValue, out var userId)
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
