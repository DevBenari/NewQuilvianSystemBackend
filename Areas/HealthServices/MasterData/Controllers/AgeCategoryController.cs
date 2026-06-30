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

using ResponseAgeCategoryPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.AgeCategoryResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/age-categories")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Age Category",
        AreaName = "HealthServices",
        ControllerName = "AgeCategory",
        Description = "Master kategori usia pasien",
        SortOrder = 30
    )]
    [Tags("Health Services / Master Data / Age Category")]
    public class AgeCategoryController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData";
        private const string AgeCategoryCodePrefix = "AGE-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public AgeCategoryController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<AgeCategoryFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Age Category", Description = "Melihat metadata filter kategori usia", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AgeCategory", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new AgeCategoryFilterMetadataResponse
            {
                DefaultFilter = new AgeCategoryDefaultFilterResponse(),
                CustomPeriods = BuildCustomPeriodOptions(),
                SortOptions = new List<AgeCategorySortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "ageCategoryCode", Label = "Kode kategori" },
                    new() { Value = "ageCategoryName", Label = "Nama kategori" },
                    new() { Value = "minAgeDays", Label = "Usia minimum hari" },
                    new() { Value = "maxAgeDays", Label = "Usia maksimum hari" },
                    new() { Value = "isDefault", Label = "Default" },
                    new() { Value = "isSelectableInKiosk", Label = "Tampil di kiosk" },
                    new() { Value = "isSelectableInRegistration", Label = "Tampil di registrasi" },
                    new() { Value = "isUsedForClinicalRule", Label = "Dipakai aturan klinis" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateFieldMetadata(),
                UpdateFields = BuildUpdateFieldMetadata(),
                ResetButtonLabel = "Reset"
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "AgeCategory.GetFilterMetadata",
                "Mengambil metadata filter kategori usia.",
                result
            );

            return Ok(ApiResponse<AgeCategoryFilterMetadataResponse>.Ok(
                result,
                "Metadata filter kategori usia berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<AgeCategorySummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Age Category", Description = "Melihat ringkasan kategori usia", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AgeCategory", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();

            var result = new AgeCategorySummaryResponse
            {
                TotalAgeCategory = await query.CountAsync(),
                ActiveAgeCategory = await query.CountAsync(x => x.IsActive),
                InactiveAgeCategory = await query.CountAsync(x => !x.IsActive),
                DefaultAgeCategory = await query.CountAsync(x => x.IsDefault),
                SelectableInKiosk = await query.CountAsync(x => x.IsSelectableInKiosk),
                SelectableInRegistration = await query.CountAsync(x => x.IsSelectableInRegistration),
                UsedForClinicalRule = await query.CountAsync(x => x.IsUsedForClinicalRule),
                WithOpenEndedRange = await query.CountAsync(x => x.MaxAgeDays == null)
            };

            return Ok(ApiResponse<AgeCategorySummaryResponse>.Ok(
                result,
                "Ringkasan kategori usia berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseAgeCategoryPagedResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Age Category", Description = "Melihat data kategori usia", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AgeCategory", "Read")]
        public async Task<IActionResult> GetAgeCategories(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] bool? isDefault,
            [FromQuery] bool? isSelectableInKiosk,
            [FromQuery] bool? isSelectableInRegistration,
            [FromQuery] bool? isUsedForClinicalRule,
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
                isDefault,
                isSelectableInKiosk,
                isSelectableInRegistration,
                isUsedForClinicalRule
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

            var result = new ResponseAgeCategoryPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseAgeCategoryPagedResult>.Ok(
                result,
                "Data kategori usia berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<AgeCategoryOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Age Category", Description = "Melihat data pilihan kategori usia", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AgeCategory", "Read")]
        public async Task<IActionResult> GetAgeCategoryOptions(
            [FromQuery] bool onlyActive = true,
            [FromQuery] bool onlySelectableInRegistration = false,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            if (onlyActive)
            {
                query = query.Where(x => x.IsActive);
            }

            if (onlySelectableInRegistration)
            {
                query = query.Where(x => x.IsSelectableInRegistration);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.AgeCategoryCode.ToLower().Contains(keyword) ||
                    x.AgeCategoryName.ToLower().Contains(keyword) ||
                    (x.AgeCategoryShortName != null && x.AgeCategoryShortName.ToLower().Contains(keyword)) ||
                    (x.StandardReference != null && x.StandardReference.ToLower().Contains(keyword))
                );
            }

            var totalData = await query.CountAsync();

            var entities = await query
                .OrderByDescending(x => x.IsDefault)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.MinAgeDays)
                .ThenBy(x => x.AgeCategoryName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities.Select(MapOptionResponse).ToList();

            var result = new AgeCategoryOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<AgeCategoryOptionPagedResponse>.Ok(
                result,
                "Data pilihan kategori usia berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<AgeCategoryDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Age Category", Description = "Melihat detail kategori usia", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AgeCategory", "Read")]
        public async Task<IActionResult> GetAgeCategoryById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Kategori usia tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            return Ok(ApiResponse<AgeCategoryDetailResponse>.Ok(
                MapDetailResponse(entity, actorNames),
                "Detail kategori usia berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<AgeCategoryCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Age Category", Description = "Membuat data kategori usia", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("AgeCategory", "Create")]
        public async Task<IActionResult> CreateAgeCategory([FromBody] CreateAgeCategoryRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                request: request,
                isActive: true
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data kategori usia tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (request.IsDefault)
                {
                    await UnsetOtherDefaultAgeCategoriesAsync(null, now, actorUserId);
                }

                var entity = new MstAgeCategory
                {
                    Id = Guid.NewGuid(),
                    AgeCategoryCode = await GenerateAgeCategoryCodeAsync(),
                    AgeCategoryName = request.AgeCategoryName.Trim(),
                    AgeCategoryShortName = NormalizeNullableText(request.AgeCategoryShortName),
                    MinAgeDays = request.MinAgeDays,
                    MaxAgeDays = request.MaxAgeDays,
                    IsDefault = request.IsDefault,
                    IsSelectableInKiosk = request.IsSelectableInKiosk,
                    IsSelectableInRegistration = request.IsSelectableInRegistration,
                    IsUsedForClinicalRule = request.IsUsedForClinicalRule,
                    StandardReference = NormalizeNullableText(request.StandardReference),
                    EffectiveStartDate = NormalizeNullableUtcDate(request.EffectiveStartDate),
                    EffectiveEndDate = NormalizeNullableUtcDate(request.EffectiveEndDate),
                    SortOrder = request.SortOrder,
                    Description = NormalizeNullableText(request.Description),
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<MstAgeCategory>().Add(entity);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
                var result = new AgeCategoryCreateResponse
                {
                    Id = entity.Id,
                    AgeCategoryCode = entity.AgeCategoryCode,
                    AgeCategoryName = entity.AgeCategoryName,
                    MinAgeDays = entity.MinAgeDays,
                    MaxAgeDays = entity.MaxAgeDays,
                    AgeRangeLabel = BuildAgeRangeLabel(entity.MinAgeDays, entity.MaxAgeDays),
                    IsDefault = entity.IsDefault,
                    IsActive = entity.IsActive,
                    CreateDateTime = entity.CreateDateTime,
                    CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                    CreateByName = GetActorName(actorNames, entity.CreateBy)
                };

                await _loggerService.InfoAsync(
                    LogCategory,
                    "AgeCategory.CreateAgeCategory",
                    "Membuat data kategori usia.",
                    result
                );

                return Ok(ApiResponse<AgeCategoryCreateResponse>.Ok(
                    result,
                    "Kategori usia berhasil dibuat."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "AgeCategory.CreateAgeCategory",
                    "Gagal membuat data kategori usia.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat membuat kategori usia."
                    )
                );
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<AgeCategoryUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Update", "Update Age Category", Description = "Mengubah data kategori usia", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("AgeCategory", "Update")]
        public async Task<IActionResult> UpdateAgeCategory(
            Guid id,
            [FromBody] UpdateAgeCategoryRequest request)
        {
            var entity = await _dbContext.Set<MstAgeCategory>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Kategori usia tidak ditemukan."
                ));
            }

            if (request.IsDefault && !request.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Kategori usia default harus aktif."
                ));
            }

            var validation = await ValidateRequestAsync(
                excludeId: id,
                request: request,
                isActive: request.IsActive
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data kategori usia tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (request.IsDefault)
                {
                    await UnsetOtherDefaultAgeCategoriesAsync(id, now, actorUserId);
                }

                entity.AgeCategoryName = request.AgeCategoryName.Trim();
                entity.AgeCategoryShortName = NormalizeNullableText(request.AgeCategoryShortName);
                entity.MinAgeDays = request.MinAgeDays;
                entity.MaxAgeDays = request.MaxAgeDays;
                entity.IsDefault = request.IsActive ? request.IsDefault : false;
                entity.IsSelectableInKiosk = request.IsSelectableInKiosk;
                entity.IsSelectableInRegistration = request.IsSelectableInRegistration;
                entity.IsUsedForClinicalRule = request.IsUsedForClinicalRule;
                entity.StandardReference = NormalizeNullableText(request.StandardReference);
                entity.EffectiveStartDate = NormalizeNullableUtcDate(request.EffectiveStartDate);
                entity.EffectiveEndDate = NormalizeNullableUtcDate(request.EffectiveEndDate);
                entity.SortOrder = request.SortOrder;
                entity.Description = NormalizeNullableText(request.Description);
                entity.IsActive = request.IsActive;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
                var result = new AgeCategoryUpdateResponse
                {
                    Id = entity.Id,
                    AgeCategoryCode = entity.AgeCategoryCode,
                    AgeCategoryName = entity.AgeCategoryName,
                    IsDefault = entity.IsDefault,
                    IsActive = entity.IsActive,
                    UpdateDateTime = entity.UpdateDateTime,
                    UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                    UpdateByName = GetActorName(actorNames, entity.UpdateBy)
                };

                await _loggerService.InfoAsync(
                    LogCategory,
                    "AgeCategory.UpdateAgeCategory",
                    "Mengubah data kategori usia.",
                    result
                );

                return Ok(ApiResponse<AgeCategoryUpdateResponse>.Ok(
                    result,
                    "Kategori usia berhasil diperbarui."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "AgeCategory.UpdateAgeCategory",
                    "Gagal mengubah data kategori usia.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat memperbarui kategori usia."
                    )
                );
            }
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<AgeCategoryStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Age Category Status", Description = "Mengubah status kategori usia", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("AgeCategory", "Update")]
        public async Task<IActionResult> UpdateAgeCategoryStatus(
            Guid id,
            [FromBody] UpdateAgeCategoryStatusRequest request)
        {
            var entity = await _dbContext.Set<MstAgeCategory>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Kategori usia tidak ditemukan."
                ));
            }

            if (request.IsActive)
            {
                var overlapValidation = await ValidateNoOverlappingRangeAsync(
                    excludeId: id,
                    minAgeDays: entity.MinAgeDays,
                    maxAgeDays: entity.MaxAgeDays
                );

                if (!overlapValidation.IsValid)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        overlapValidation.ErrorMessage ?? "Rentang usia kategori tidak valid."
                    ));
                }
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;

            if (!request.IsActive)
            {
                entity.IsDefault = false;
            }

            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
            var result = new AgeCategoryStatusResponse
            {
                Id = entity.Id,
                AgeCategoryCode = entity.AgeCategoryCode,
                AgeCategoryName = entity.AgeCategoryName,
                IsDefault = entity.IsDefault,
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };

            return Ok(ApiResponse<AgeCategoryStatusResponse>.Ok(
                result,
                "Status kategori usia berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<AgeCategoryDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Age Category", Description = "Menghapus data kategori usia", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("AgeCategory", "Delete")]
        public async Task<IActionResult> DeleteAgeCategory(
            Guid id,
            [FromBody] DeleteAgeCategoryRequest? request = null)
        {
            var entity = await _dbContext.Set<MstAgeCategory>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Kategori usia tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.IsDefault = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            if (!string.IsNullOrWhiteSpace(request?.DeleteReason))
            {
                entity.Description = request.DeleteReason.Trim();
            }

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
            var result = new AgeCategoryDeleteResponse
            {
                Id = entity.Id,
                AgeCategoryCode = entity.AgeCategoryCode,
                AgeCategoryName = entity.AgeCategoryName,
                DeleteDateTime = entity.DeleteDateTime,
                DeleteBy = entity.DeleteBy == Guid.Empty ? null : entity.DeleteBy,
                DeleteByName = GetActorName(actorNames, entity.DeleteBy)
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "AgeCategory.DeleteAgeCategory",
                "Menghapus data kategori usia.",
                result
            );

            return Ok(ApiResponse<AgeCategoryDeleteResponse>.Ok(
                result,
                "Kategori usia berhasil dihapus."
            ));
        }

        private IQueryable<MstAgeCategory> BuildBaseQuery()
        {
            return _dbContext.Set<MstAgeCategory>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstAgeCategory> ApplyDateFilter(
            IQueryable<MstAgeCategory> query,
            AgeCategoryDateRangeFilter dateRange)
        {
            if (dateRange.StartDate.HasValue)
            {
                query = query.Where(x => x.CreateDateTime >= dateRange.StartDate.Value);
            }

            if (dateRange.EndDateExclusive.HasValue)
            {
                query = query.Where(x => x.CreateDateTime < dateRange.EndDateExclusive.Value);
            }

            return query;
        }

        private static IQueryable<MstAgeCategory> ApplyStandardFilter(
            IQueryable<MstAgeCategory> query,
            string? search,
            bool? isActive,
            bool? isDefault,
            bool? isSelectableInKiosk,
            bool? isSelectableInRegistration,
            bool? isUsedForClinicalRule)
        {
            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (isDefault.HasValue)
            {
                query = query.Where(x => x.IsDefault == isDefault.Value);
            }

            if (isSelectableInKiosk.HasValue)
            {
                query = query.Where(x => x.IsSelectableInKiosk == isSelectableInKiosk.Value);
            }

            if (isSelectableInRegistration.HasValue)
            {
                query = query.Where(x => x.IsSelectableInRegistration == isSelectableInRegistration.Value);
            }

            if (isUsedForClinicalRule.HasValue)
            {
                query = query.Where(x => x.IsUsedForClinicalRule == isUsedForClinicalRule.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.AgeCategoryCode.ToLower().Contains(keyword) ||
                    x.AgeCategoryName.ToLower().Contains(keyword) ||
                    (x.AgeCategoryShortName != null && x.AgeCategoryShortName.ToLower().Contains(keyword)) ||
                    (x.StandardReference != null && x.StandardReference.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword))
                );
            }

            return query;
        }

        private static IOrderedQueryable<MstAgeCategory> ApplySorting(
            IQueryable<MstAgeCategory> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => isDescending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "agecategorycode" => isDescending
                    ? query.OrderByDescending(x => x.AgeCategoryCode)
                    : query.OrderBy(x => x.AgeCategoryCode),

                "agecategoryname" => isDescending
                    ? query.OrderByDescending(x => x.AgeCategoryName)
                    : query.OrderBy(x => x.AgeCategoryName),

                "minagedays" => isDescending
                    ? query.OrderByDescending(x => x.MinAgeDays).ThenBy(x => x.AgeCategoryName)
                    : query.OrderBy(x => x.MinAgeDays).ThenBy(x => x.AgeCategoryName),

                "maxagedays" => isDescending
                    ? query.OrderByDescending(x => x.MaxAgeDays).ThenBy(x => x.AgeCategoryName)
                    : query.OrderBy(x => x.MaxAgeDays).ThenBy(x => x.AgeCategoryName),

                "isdefault" => isDescending
                    ? query.OrderByDescending(x => x.IsDefault).ThenBy(x => x.AgeCategoryName)
                    : query.OrderBy(x => x.IsDefault).ThenBy(x => x.AgeCategoryName),

                "isselectableinkiosk" => isDescending
                    ? query.OrderByDescending(x => x.IsSelectableInKiosk).ThenBy(x => x.AgeCategoryName)
                    : query.OrderBy(x => x.IsSelectableInKiosk).ThenBy(x => x.AgeCategoryName),

                "isselectableinregistration" => isDescending
                    ? query.OrderByDescending(x => x.IsSelectableInRegistration).ThenBy(x => x.AgeCategoryName)
                    : query.OrderBy(x => x.IsSelectableInRegistration).ThenBy(x => x.AgeCategoryName),

                "isusedforclinicalrule" => isDescending
                    ? query.OrderByDescending(x => x.IsUsedForClinicalRule).ThenBy(x => x.AgeCategoryName)
                    : query.OrderBy(x => x.IsUsedForClinicalRule).ThenBy(x => x.AgeCategoryName),

                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.AgeCategoryName)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.AgeCategoryName),

                _ => isDescending
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.MinAgeDays).ThenByDescending(x => x.AgeCategoryName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.MinAgeDays).ThenBy(x => x.AgeCategoryName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateAgeCategoryRequest request,
            bool isActive)
        {
            if (string.IsNullOrWhiteSpace(request.AgeCategoryName))
            {
                return (false, "Nama kategori usia wajib diisi.");
            }

            if (request.MinAgeDays < 0)
            {
                return (false, "Usia minimum hari tidak boleh kurang dari 0.");
            }

            if (request.MaxAgeDays.HasValue && request.MaxAgeDays.Value < request.MinAgeDays)
            {
                return (false, "Usia maksimum hari tidak boleh lebih kecil dari usia minimum hari.");
            }

            var effectiveStart = NormalizeNullableUtcDate(request.EffectiveStartDate);
            var effectiveEnd = NormalizeNullableUtcDate(request.EffectiveEndDate);

            if (effectiveStart.HasValue && effectiveEnd.HasValue && effectiveEnd.Value < effectiveStart.Value)
            {
                return (false, "Tanggal akhir efektif tidak boleh lebih kecil dari tanggal mulai efektif.");
            }

            var normalizedName = request.AgeCategoryName.Trim().ToLower();

            var duplicateNameQuery = _dbContext.Set<MstAgeCategory>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.AgeCategoryName.ToLower() == normalizedName);

            if (excludeId.HasValue)
            {
                duplicateNameQuery = duplicateNameQuery.Where(x => x.Id != excludeId.Value);
            }

            if (await duplicateNameQuery.AnyAsync())
            {
                return (false, "Nama kategori usia sudah digunakan.");
            }

            if (isActive)
            {
                var overlapValidation = await ValidateNoOverlappingRangeAsync(
                    excludeId,
                    request.MinAgeDays,
                    request.MaxAgeDays
                );

                if (!overlapValidation.IsValid)
                {
                    return overlapValidation;
                }
            }

            return (true, null);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateNoOverlappingRangeAsync(
            Guid? excludeId,
            int minAgeDays,
            int? maxAgeDays)
        {
            var newMax = maxAgeDays ?? int.MaxValue;

            var query = _dbContext.Set<MstAgeCategory>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    x.MinAgeDays <= newMax &&
                    (x.MaxAgeDays ?? int.MaxValue) >= minAgeDays);

            if (excludeId.HasValue)
            {
                query = query.Where(x => x.Id != excludeId.Value);
            }

            var overlap = await query
                .OrderBy(x => x.MinAgeDays)
                .FirstOrDefaultAsync();

            if (overlap != null)
            {
                return (
                    false,
                    $"Rentang usia bertabrakan dengan kategori {overlap.AgeCategoryName} ({BuildAgeRangeLabel(overlap.MinAgeDays, overlap.MaxAgeDays)})."
                );
            }

            return (true, null);
        }

        private async Task UnsetOtherDefaultAgeCategoriesAsync(
            Guid? exceptId,
            DateTime now,
            Guid actorUserId)
        {
            var query = _dbContext.Set<MstAgeCategory>()
                .Where(x =>
                    x.IsDefault &&
                    !x.IsDelete);

            if (exceptId.HasValue)
            {
                query = query.Where(x => x.Id != exceptId.Value);
            }

            var categories = await query.ToListAsync();

            foreach (var category in categories)
            {
                category.IsDefault = false;
                category.UpdateDateTime = now;
                category.UpdateBy = actorUserId;
            }
        }

        private async Task<string> GenerateAgeCategoryCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstAgeCategory>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.AgeCategoryCode.StartsWith(AgeCategoryCodePrefix))
                .Select(x => x.AgeCategoryCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(x => x.Replace(AgeCategoryCodePrefix, string.Empty))
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .Where(x => x > 0)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }

            return AgeCategoryCodePrefix + nextNumber.ToString().PadLeft(CodeNumberLength, '0');
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

        private static AgeCategoryResponse MapResponse(
            MstAgeCategory entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new AgeCategoryResponse
            {
                Id = entity.Id,
                AgeCategoryCode = entity.AgeCategoryCode,
                AgeCategoryName = entity.AgeCategoryName,
                AgeCategoryShortName = entity.AgeCategoryShortName,
                MinAgeDays = entity.MinAgeDays,
                MaxAgeDays = entity.MaxAgeDays,
                AgeRangeLabel = BuildAgeRangeLabel(entity.MinAgeDays, entity.MaxAgeDays),
                IsDefault = entity.IsDefault,
                IsSelectableInKiosk = entity.IsSelectableInKiosk,
                IsSelectableInRegistration = entity.IsSelectableInRegistration,
                IsUsedForClinicalRule = entity.IsUsedForClinicalRule,
                StandardReference = entity.StandardReference,
                EffectiveStartDate = entity.EffectiveStartDate,
                EffectiveEndDate = entity.EffectiveEndDate,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private static AgeCategoryDetailResponse MapDetailResponse(
            MstAgeCategory entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new AgeCategoryDetailResponse
            {
                Id = entity.Id,
                AgeCategoryCode = entity.AgeCategoryCode,
                AgeCategoryName = entity.AgeCategoryName,
                AgeCategoryShortName = entity.AgeCategoryShortName,
                MinAgeDays = entity.MinAgeDays,
                MaxAgeDays = entity.MaxAgeDays,
                AgeRangeLabel = BuildAgeRangeLabel(entity.MinAgeDays, entity.MaxAgeDays),
                IsDefault = entity.IsDefault,
                IsSelectableInKiosk = entity.IsSelectableInKiosk,
                IsSelectableInRegistration = entity.IsSelectableInRegistration,
                IsUsedForClinicalRule = entity.IsUsedForClinicalRule,
                StandardReference = entity.StandardReference,
                EffectiveStartDate = entity.EffectiveStartDate,
                EffectiveEndDate = entity.EffectiveEndDate,
                SortOrder = entity.SortOrder,
                Description = entity.Description,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static AgeCategoryOptionResponse MapOptionResponse(MstAgeCategory entity)
        {
            return new AgeCategoryOptionResponse
            {
                Id = entity.Id,
                AgeCategoryCode = entity.AgeCategoryCode,
                AgeCategoryName = entity.AgeCategoryName,
                AgeCategoryShortName = entity.AgeCategoryShortName,
                MinAgeDays = entity.MinAgeDays,
                MaxAgeDays = entity.MaxAgeDays,
                AgeRangeLabel = BuildAgeRangeLabel(entity.MinAgeDays, entity.MaxAgeDays),
                IsDefault = entity.IsDefault,
                SortOrder = entity.SortOrder
            };
        }

        private static string BuildAgeRangeLabel(int minAgeDays, int? maxAgeDays)
        {
            var minLabel = BuildAgeLabelFromDays(minAgeDays);

            if (!maxAgeDays.HasValue)
            {
                return $">= {minLabel}";
            }

            return $"{minLabel} - {BuildAgeLabelFromDays(maxAgeDays.Value)}";
        }

        private static string BuildAgeLabelFromDays(int totalDays)
        {
            if (totalDays < 30)
            {
                return $"{totalDays} hari";
            }

            if (totalDays < 365)
            {
                return $"{totalDays / 30} bulan";
            }

            return $"{totalDays / 365} tahun";
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

        private static List<AgeCategoryCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<AgeCategoryCustomPeriodOptionResponse>
            {
                new() { Value = "custom", Label = "Custom", Description = "Gunakan startDate dan endDate.", UsesStartDate = true, UsesEndDate = true },
                new() { Value = "today", Label = "Hari ini", Description = "Data yang dibuat hari ini.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last7days", Label = "7 hari terakhir", Description = "Data tujuh hari terakhir termasuk hari ini.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thismonth", Label = "Bulan ini", Description = "Data bulan berjalan.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "lastmonth", Label = "Bulan lalu", Description = "Data bulan sebelumnya.", UsesStartDate = false, UsesEndDate = false }
            };
        }

        private static List<AgeCategoryQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<AgeCategoryQueryParameterInfoResponse>
            {
                new() { Name = "startDate", Type = "DateTime?", Description = "Tanggal awal filter CreateDateTime.", Example = "2026-01-01" },
                new() { Name = "endDate", Type = "DateTime?", Description = "Tanggal akhir filter CreateDateTime.", Example = "2026-01-31" },
                new() { Name = "customPeriod", Type = "string?", Description = "today, last7days, thismonth, lastmonth, atau custom.", Example = "today" },
                new() { Name = "search", Type = "string?", Description = "Pencarian kode, nama, short name, referensi standar, atau deskripsi.", Example = "remaja" },
                new() { Name = "isActive", Type = "bool?", Description = "Filter status aktif.", Example = "true" },
                new() { Name = "isDefault", Type = "bool?", Description = "Filter kategori default.", Example = "false" },
                new() { Name = "sortBy", Type = "string?", Description = "Kolom sorting.", Example = "sortOrder" },
                new() { Name = "sortDirection", Type = "string?", Description = "asc atau desc.", Example = "asc" },
                new() { Name = "pageNumber", Type = "int", Description = "Nomor halaman.", Example = "1" },
                new() { Name = "pageSize", Type = "int", Description = "Ukuran halaman maksimal 100.", Example = "25" }
            };
        }

        private static List<AgeCategoryFormFieldMetadataResponse> BuildCreateFieldMetadata()
        {
            return BuildFieldMetadata(isUpdate: false);
        }

        private static List<AgeCategoryFormFieldMetadataResponse> BuildUpdateFieldMetadata()
        {
            return BuildFieldMetadata(isUpdate: true);
        }

        private static List<AgeCategoryFormFieldMetadataResponse> BuildFieldMetadata(bool isUpdate)
        {
            var fields = new List<AgeCategoryFormFieldMetadataResponse>
            {
                new() { Name = "ageCategoryName", Label = "Nama Kategori Usia", Section = "Informasi Utama", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 150, Description = "Nama kategori usia.", Example = "Remaja", SortOrder = 1 },
                new() { Name = "ageCategoryShortName", Label = "Nama Singkat", Section = "Informasi Utama", InputType = "text", MaxLength = 75, Description = "Nama singkat kategori.", Example = "Remaja", SortOrder = 2 },
                new() { Name = "minAgeDays", Label = "Usia Minimum Hari", Section = "Rentang Usia", InputType = "number", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", Description = "Batas bawah usia dalam hari.", Example = "3650", SortOrder = 3 },
                new() { Name = "maxAgeDays", Label = "Usia Maksimum Hari", Section = "Rentang Usia", InputType = "number", Description = "Batas atas usia dalam hari. Kosongkan untuk open ended.", Example = "6574", SortOrder = 4 },
                new() { Name = "isDefault", Label = "Default", Section = "Pengaturan", InputType = "checkbox", Description = "Menandai kategori default.", Example = "false", SortOrder = 5 },
                new() { Name = "isSelectableInKiosk", Label = "Tampil di Kiosk", Section = "Pengaturan", InputType = "checkbox", Description = "Dapat digunakan/ditampilkan untuk proses kiosk.", Example = "true", SortOrder = 6 },
                new() { Name = "isSelectableInRegistration", Label = "Tampil di Registrasi", Section = "Pengaturan", InputType = "checkbox", Description = "Dapat digunakan di proses registrasi/admission.", Example = "true", SortOrder = 7 },
                new() { Name = "isUsedForClinicalRule", Label = "Dipakai Aturan Klinis", Section = "Pengaturan", InputType = "checkbox", Description = "Dapat digunakan untuk aturan klinis/CDSS/report.", Example = "true", SortOrder = 8 },
                new() { Name = "standardReference", Label = "Referensi Standar", Section = "Referensi", InputType = "text", MaxLength = 250, Description = "Referensi aturan, misalnya Kemenkes atau kebijakan RS.", Example = "Kemenkes / Internal RS", SortOrder = 9 },
                new() { Name = "effectiveStartDate", Label = "Mulai Berlaku", Section = "Referensi", InputType = "date", Description = "Tanggal mulai efektif aturan.", Example = "2026-01-01", SortOrder = 10 },
                new() { Name = "effectiveEndDate", Label = "Akhir Berlaku", Section = "Referensi", InputType = "date", Description = "Tanggal akhir efektif aturan.", Example = "2026-12-31", SortOrder = 11 },
                new() { Name = "sortOrder", Label = "Urutan", Section = "Pengaturan", InputType = "number", Description = "Urutan tampilan.", Example = "1", SortOrder = 12 },
                new() { Name = "description", Label = "Deskripsi", Section = "Catatan", InputType = "textarea", MaxLength = 250, Description = "Catatan kategori usia.", Example = "Kategori usia untuk registrasi pasien.", SortOrder = 13 }
            };

            if (isUpdate)
            {
                fields.Add(new AgeCategoryFormFieldMetadataResponse
                {
                    Name = "isActive",
                    Label = "Status Aktif",
                    Section = "Pengaturan",
                    InputType = "checkbox",
                    Description = "Status aktif data.",
                    Example = "true",
                    SortOrder = 14
                });
            }

            return fields;
        }

        private static AgeCategoryDateRangeFilter ResolveDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            if (string.IsNullOrWhiteSpace(customPeriod) ||
                string.Equals(customPeriod, "custom", StringComparison.OrdinalIgnoreCase))
            {
                var customStart = startDate.HasValue ? ToUtcDate(startDate.Value) : (DateTime?)null;
                var customEndExclusive = endDate.HasValue ? ToUtcDate(endDate.Value).AddDays(1) : (DateTime?)null;

                if (customStart.HasValue && customEndExclusive.HasValue && customEndExclusive.Value <= customStart.Value)
                {
                    return AgeCategoryDateRangeFilter.Invalid("Tanggal akhir harus lebih besar atau sama dengan tanggal awal.");
                }

                return AgeCategoryDateRangeFilter.Valid(customStart, customEndExclusive);
            }

            var today = DateTime.UtcNow.Date;

            return customPeriod.Trim().ToLowerInvariant() switch
            {
                "today" => AgeCategoryDateRangeFilter.Valid(today, today.AddDays(1)),
                "last7days" => AgeCategoryDateRangeFilter.Valid(today.AddDays(-6), today.AddDays(1)),
                "thismonth" => AgeCategoryDateRangeFilter.Valid(new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1)),
                "lastmonth" => AgeCategoryDateRangeFilter.Valid(new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1), new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc)),
                _ => AgeCategoryDateRangeFilter.Invalid("customPeriod tidak valid. Gunakan today, last7days, thismonth, lastmonth, atau custom.")
            };
        }

        private static DateTime ToUtcDate(DateTime value)
        {
            return DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);
        }

        private static DateTime? NormalizeNullableUtcDate(DateTime? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            return DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
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

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
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

        private class AgeCategoryDateRangeFilter
        {
            public bool IsValid { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDateExclusive { get; set; }
            public string? ErrorMessage { get; set; }

            public static AgeCategoryDateRangeFilter Valid(DateTime? startDate, DateTime? endDateExclusive)
            {
                return new AgeCategoryDateRangeFilter
                {
                    IsValid = true,
                    StartDate = startDate,
                    EndDateExclusive = endDateExclusive
                };
            }

            public static AgeCategoryDateRangeFilter Invalid(string errorMessage)
            {
                return new AgeCategoryDateRangeFilter
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
        }
    }
}
