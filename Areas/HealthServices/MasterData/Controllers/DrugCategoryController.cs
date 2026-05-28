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

using ResponseDrugCategoryPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.DrugCategoryResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/drug-categories")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Drug Category",
        AreaName = "HealthServices",
        ControllerName = "DrugCategory",
        Description = "Health service master data drug category",
        SortOrder = 10
    )]
    [Tags("Health Services / Master Data / Drug Category")]
    public class DrugCategoryController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData";

        private static readonly HashSet<string> AllowedDrugCategoryTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "General",
            "Antibiotic",
            "Analgesic",
            "Antipyretic",
            "Antihypertensive",
            "Antidiabetic",
            "Vitamin",
            "Vaccine",
            "Consumable",
            "Other"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public DrugCategoryController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<DrugCategoryFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Drug Category", Description = "Melihat data drug category", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DrugCategory", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new DrugCategoryFilterMetadataResponse
            {
                DefaultFilter = new DrugCategoryDefaultFilterResponse(),
                SortOptions = new List<DrugCategorySortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "drugCategoryCode", Label = "Kode kategori obat" },
                    new() { Value = "drugCategoryName", Label = "Nama kategori obat" },
                    new() { Value = "drugGroupName", Label = "Group obat" },
                    new() { Value = "drugCategoryType", Label = "Tipe kategori obat" },
                    new() { Value = "isAntibiotic", Label = "Antibiotik" },
                    new() { Value = "isNarcotic", Label = "Narkotik" },
                    new() { Value = "isPsychotropic", Label = "Psikotropik" },
                    new() { Value = "isHighAlert", Label = "High alert" },
                    new() { Value = "isCoveredByInsuranceDefault", Label = "Default ditanggung asuransi" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                DrugCategoryTypeOptions = AllowedDrugCategoryTypes.OrderBy(x => x).ToList()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "DrugCategory.GetFilterMetadata",
                "Mengambil metadata filter drug category.",
                result
            );

            return Ok(ApiResponse<DrugCategoryFilterMetadataResponse>.Ok(
                result,
                "Metadata filter drug category berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<DrugCategorySummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Drug Category", Description = "Melihat data drug category", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DrugCategory", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = _dbContext.Set<MstDrugCategory>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new DrugCategorySummaryResponse
            {
                TotalDrugCategory = await query.CountAsync(),
                ActiveDrugCategory = await query.CountAsync(x => x.IsActive),
                InactiveDrugCategory = await query.CountAsync(x => !x.IsActive),
                AntibioticDrugCategory = await query.CountAsync(x => x.IsAntibiotic),
                NarcoticDrugCategory = await query.CountAsync(x => x.IsNarcotic),
                PsychotropicDrugCategory = await query.CountAsync(x => x.IsPsychotropic),
                HighAlertDrugCategory = await query.CountAsync(x => x.IsHighAlert),
                ChronicDiseaseDrugCategory = await query.CountAsync(x => x.IsChronicDiseaseDrug),
                VaccineDrugCategory = await query.CountAsync(x => x.IsVaccine),
                ConsumableDrugCategory = await query.CountAsync(x => x.IsConsumable),
                CoveredByInsuranceDefaultDrugCategory = await query.CountAsync(x => x.IsCoveredByInsuranceDefault)
            };

            return Ok(ApiResponse<DrugCategorySummaryResponse>.Ok(
                result,
                "Ringkasan drug category berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseDrugCategoryPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Drug Category", Description = "Melihat data drug category", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DrugCategory", "Read")]
        public async Task<IActionResult> GetDrugCategories(
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] string? drugGroupName,
            [FromQuery] string? drugCategoryType,
            [FromQuery] bool? isAntibiotic,
            [FromQuery] bool? isNarcotic,
            [FromQuery] bool? isPsychotropic,
            [FromQuery] bool? isHighAlert,
            [FromQuery] bool? isChronicDiseaseDrug,
            [FromQuery] bool? isVaccine,
            [FromQuery] bool? isConsumable,
            [FromQuery] bool? isCoveredByInsuranceDefault,
            [FromQuery] string? sortBy = "sortOrder",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyFilter(
                query,
                search,
                isActive,
                drugGroupName,
                drugCategoryType,
                isAntibiotic,
                isNarcotic,
                isPsychotropic,
                isHighAlert,
                isChronicDiseaseDrug,
                isVaccine,
                isConsumable,
                isCoveredByInsuranceDefault
            );

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => ToResponse(x))
                .ToListAsync();

            var result = new ResponseDrugCategoryPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseDrugCategoryPagedResult>.Ok(
                result,
                "Data drug category berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<DrugCategoryOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Drug Category", Description = "Melihat data drug category", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DrugCategory", "Read")]
        public async Task<IActionResult> GetDrugCategoryOptions(
            [FromQuery] string? drugGroupName,
            [FromQuery] string? drugCategoryType,
            [FromQuery] bool? isAntibiotic,
            [FromQuery] bool? isNarcotic,
            [FromQuery] bool? isPsychotropic,
            [FromQuery] bool? isHighAlert,
            [FromQuery] bool? isChronicDiseaseDrug,
            [FromQuery] bool? isVaccine,
            [FromQuery] bool? isConsumable,
            [FromQuery] bool? isCoveredByInsuranceDefault,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = BuildBaseQuery();

            query = ApplyFilter(
                query,
                search,
                onlyActive ? true : null,
                drugGroupName,
                drugCategoryType,
                isAntibiotic,
                isNarcotic,
                isPsychotropic,
                isHighAlert,
                isChronicDiseaseDrug,
                isVaccine,
                isConsumable,
                isCoveredByInsuranceDefault
            );

            var data = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.DrugCategoryName)
                .Select(x => new DrugCategoryOptionResponse
                {
                    Id = x.Id,
                    DrugCategoryCode = x.DrugCategoryCode,
                    DrugCategoryName = x.DrugCategoryName,
                    DrugGroupName = x.DrugGroupName,
                    DrugCategoryType = x.DrugCategoryType,
                    IsAntibiotic = x.IsAntibiotic,
                    IsNarcotic = x.IsNarcotic,
                    IsPsychotropic = x.IsPsychotropic,
                    IsHighAlert = x.IsHighAlert,
                    IsChronicDiseaseDrug = x.IsChronicDiseaseDrug,
                    IsVaccine = x.IsVaccine,
                    IsConsumable = x.IsConsumable,
                    IsCoveredByInsuranceDefault = x.IsCoveredByInsuranceDefault
                })
                .ToListAsync();

            return Ok(ApiResponse<List<DrugCategoryOptionResponse>>.Ok(
                data,
                "Data pilihan drug category berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DrugCategoryDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Drug Category", Description = "Melihat data drug category", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DrugCategory", "Read")]
        public async Task<IActionResult> GetDrugCategoryById(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new DrugCategoryDetailResponse
                {
                    Id = x.Id,
                    DrugCategoryCode = x.DrugCategoryCode,
                    DrugCategoryName = x.DrugCategoryName,
                    DrugGroupName = x.DrugGroupName,
                    DrugCategoryType = x.DrugCategoryType,
                    IsAntibiotic = x.IsAntibiotic,
                    IsNarcotic = x.IsNarcotic,
                    IsPsychotropic = x.IsPsychotropic,
                    IsHighAlert = x.IsHighAlert,
                    IsChronicDiseaseDrug = x.IsChronicDiseaseDrug,
                    IsVaccine = x.IsVaccine,
                    IsConsumable = x.IsConsumable,
                    IsCoveredByInsuranceDefault = x.IsCoveredByInsuranceDefault,
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
                    "Drug category tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<DrugCategoryDetailResponse>.Ok(
                data,
                "Detail drug category berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DrugCategoryCreateResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create Drug Category", Description = "Membuat data drug category", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("DrugCategory", "Create")]
        public async Task<IActionResult> CreateDrugCategory([FromBody] CreateDrugCategoryRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                drugCategoryCode: request.DrugCategoryCode,
                drugCategoryName: request.DrugCategoryName,
                drugCategoryType: request.DrugCategoryType
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data drug category tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new MstDrugCategory
            {
                Id = Guid.NewGuid(),
                DrugCategoryCode = request.DrugCategoryCode.Trim().ToUpperInvariant(),
                DrugCategoryName = request.DrugCategoryName.Trim(),
                DrugGroupName = NormalizeNullableString(request.DrugGroupName),
                DrugCategoryType = NormalizeDrugCategoryType(request.DrugCategoryType),
                IsAntibiotic = request.IsAntibiotic,
                IsNarcotic = request.IsNarcotic,
                IsPsychotropic = request.IsPsychotropic,
                IsHighAlert = request.IsHighAlert,
                IsChronicDiseaseDrug = request.IsChronicDiseaseDrug,
                IsVaccine = request.IsVaccine,
                IsConsumable = request.IsConsumable,
                IsCoveredByInsuranceDefault = request.IsCoveredByInsuranceDefault,
                SortOrder = request.SortOrder,
                Description = NormalizeNullableString(request.Description),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstDrugCategory>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var result = new DrugCategoryCreateResponse
            {
                Id = entity.Id,
                DrugCategoryCode = entity.DrugCategoryCode,
                DrugCategoryName = entity.DrugCategoryName
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "DrugCategory.CreateDrugCategory",
                "Membuat data drug category.",
                result
            );

            return Ok(ApiResponse<DrugCategoryCreateResponse>.Ok(
                result,
                "Drug category berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DrugCategoryUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Drug Category", Description = "Mengubah data drug category", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DrugCategory", "Update")]
        public async Task<IActionResult> UpdateDrugCategory(Guid id, [FromBody] UpdateDrugCategoryRequest request)
        {
            var entity = await _dbContext.Set<MstDrugCategory>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Drug category tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(
                excludeId: id,
                drugCategoryCode: request.DrugCategoryCode,
                drugCategoryName: request.DrugCategoryName,
                drugCategoryType: request.DrugCategoryType
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data drug category tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.DrugCategoryCode = request.DrugCategoryCode.Trim().ToUpperInvariant();
            entity.DrugCategoryName = request.DrugCategoryName.Trim();
            entity.DrugGroupName = NormalizeNullableString(request.DrugGroupName);
            entity.DrugCategoryType = NormalizeDrugCategoryType(request.DrugCategoryType);
            entity.IsAntibiotic = request.IsAntibiotic;
            entity.IsNarcotic = request.IsNarcotic;
            entity.IsPsychotropic = request.IsPsychotropic;
            entity.IsHighAlert = request.IsHighAlert;
            entity.IsChronicDiseaseDrug = request.IsChronicDiseaseDrug;
            entity.IsVaccine = request.IsVaccine;
            entity.IsConsumable = request.IsConsumable;
            entity.IsCoveredByInsuranceDefault = request.IsCoveredByInsuranceDefault;
            entity.SortOrder = request.SortOrder;
            entity.Description = NormalizeNullableString(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var result = new DrugCategoryUpdateResponse
            {
                Id = entity.Id,
                DrugCategoryCode = entity.DrugCategoryCode,
                DrugCategoryName = entity.DrugCategoryName,
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "DrugCategory.UpdateDrugCategory",
                "Mengubah data drug category.",
                result
            );

            return Ok(ApiResponse<DrugCategoryUpdateResponse>.Ok(
                result,
                "Drug category berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/activate")]
        [ProducesResponseType(typeof(ApiResponse<DrugCategoryStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Drug Category", Description = "Mengaktifkan data drug category", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DrugCategory", "Update")]
        public async Task<IActionResult> ActivateDrugCategory(Guid id)
        {
            return await UpdateStatusAsync(id, true, "Drug category berhasil diaktifkan.");
        }

        [HttpPatch("{id:guid}/deactivate")]
        [ProducesResponseType(typeof(ApiResponse<DrugCategoryStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Drug Category", Description = "Menonaktifkan data drug category", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DrugCategory", "Update")]
        public async Task<IActionResult> DeactivateDrugCategory(Guid id)
        {
            return await UpdateStatusAsync(id, false, "Drug category berhasil dinonaktifkan.");
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DrugCategoryDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Drug Category", Description = "Menghapus data drug category", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("DrugCategory", "Delete")]
        public async Task<IActionResult> DeleteDrugCategory(Guid id)
        {
            var entity = await _dbContext.Set<MstDrugCategory>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Drug category tidak ditemukan."
                ));
            }

            var hasDrug = await _dbContext.Set<MstDrug>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.DrugCategoryId == id &&
                    !x.IsDelete);

            if (hasDrug)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Drug category tidak dapat dihapus karena sudah digunakan pada data drug."
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

            var result = new DrugCategoryDeleteResponse
            {
                Id = entity.Id,
                DrugCategoryCode = entity.DrugCategoryCode,
                DrugCategoryName = entity.DrugCategoryName,
                DeleteDateTime = entity.DeleteDateTime
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "DrugCategory.DeleteDrugCategory",
                "Menghapus data drug category.",
                result
            );

            return Ok(ApiResponse<DrugCategoryDeleteResponse>.Ok(
                result,
                "Drug category berhasil dihapus."
            ));
        }

        private async Task<IActionResult> UpdateStatusAsync(Guid id, bool isActive, string successMessage)
        {
            var entity = await _dbContext.Set<MstDrugCategory>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Drug category tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = isActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var result = new DrugCategoryStatusResponse
            {
                Id = entity.Id,
                DrugCategoryCode = entity.DrugCategoryCode,
                DrugCategoryName = entity.DrugCategoryName,
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "DrugCategory.UpdateStatus",
                successMessage,
                result
            );

            return Ok(ApiResponse<DrugCategoryStatusResponse>.Ok(
                result,
                successMessage
            ));
        }

        private IQueryable<MstDrugCategory> BuildBaseQuery()
        {
            return _dbContext.Set<MstDrugCategory>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstDrugCategory> ApplyFilter(
            IQueryable<MstDrugCategory> query,
            string? search,
            bool? isActive,
            string? drugGroupName,
            string? drugCategoryType,
            bool? isAntibiotic,
            bool? isNarcotic,
            bool? isPsychotropic,
            bool? isHighAlert,
            bool? isChronicDiseaseDrug,
            bool? isVaccine,
            bool? isConsumable,
            bool? isCoveredByInsuranceDefault)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.DrugCategoryCode.ToLower().Contains(keyword) ||
                    x.DrugCategoryName.ToLower().Contains(keyword) ||
                    x.DrugCategoryType.ToLower().Contains(keyword) ||
                    (x.DrugGroupName != null && x.DrugGroupName.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(drugGroupName))
            {
                var keyword = drugGroupName.Trim().ToLower();
                query = query.Where(x => x.DrugGroupName != null && x.DrugGroupName.ToLower().Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(drugCategoryType))
            {
                var normalizedType = drugCategoryType.Trim().ToLower();
                query = query.Where(x => x.DrugCategoryType.ToLower() == normalizedType);
            }

            if (isAntibiotic.HasValue)
                query = query.Where(x => x.IsAntibiotic == isAntibiotic.Value);

            if (isNarcotic.HasValue)
                query = query.Where(x => x.IsNarcotic == isNarcotic.Value);

            if (isPsychotropic.HasValue)
                query = query.Where(x => x.IsPsychotropic == isPsychotropic.Value);

            if (isHighAlert.HasValue)
                query = query.Where(x => x.IsHighAlert == isHighAlert.Value);

            if (isChronicDiseaseDrug.HasValue)
                query = query.Where(x => x.IsChronicDiseaseDrug == isChronicDiseaseDrug.Value);

            if (isVaccine.HasValue)
                query = query.Where(x => x.IsVaccine == isVaccine.Value);

            if (isConsumable.HasValue)
                query = query.Where(x => x.IsConsumable == isConsumable.Value);

            if (isCoveredByInsuranceDefault.HasValue)
                query = query.Where(x => x.IsCoveredByInsuranceDefault == isCoveredByInsuranceDefault.Value);

            return query;
        }

        private static IOrderedQueryable<MstDrugCategory> ApplySorting(
            IQueryable<MstDrugCategory> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => isDescending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "drugcategorycode" => isDescending
                    ? query.OrderByDescending(x => x.DrugCategoryCode)
                    : query.OrderBy(x => x.DrugCategoryCode),

                "drugcategoryname" => isDescending
                    ? query.OrderByDescending(x => x.DrugCategoryName)
                    : query.OrderBy(x => x.DrugCategoryName),

                "druggroupname" => isDescending
                    ? query.OrderByDescending(x => x.DrugGroupName)
                    : query.OrderBy(x => x.DrugGroupName),

                "drugcategorytype" => isDescending
                    ? query.OrderByDescending(x => x.DrugCategoryType)
                    : query.OrderBy(x => x.DrugCategoryType),

                "isantibiotic" => isDescending
                    ? query.OrderByDescending(x => x.IsAntibiotic)
                    : query.OrderBy(x => x.IsAntibiotic),

                "isnarcotic" => isDescending
                    ? query.OrderByDescending(x => x.IsNarcotic)
                    : query.OrderBy(x => x.IsNarcotic),

                "ispsychotropic" => isDescending
                    ? query.OrderByDescending(x => x.IsPsychotropic)
                    : query.OrderBy(x => x.IsPsychotropic),

                "ishighalert" => isDescending
                    ? query.OrderByDescending(x => x.IsHighAlert)
                    : query.OrderBy(x => x.IsHighAlert),

                "iscoveredbyinsurancedefault" => isDescending
                    ? query.OrderByDescending(x => x.IsCoveredByInsuranceDefault)
                    : query.OrderBy(x => x.IsCoveredByInsuranceDefault),

                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => isDescending
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.DrugCategoryName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.DrugCategoryName)
            };
        }

        private static DrugCategoryResponse ToResponse(MstDrugCategory x)
        {
            return new DrugCategoryResponse
            {
                Id = x.Id,
                DrugCategoryCode = x.DrugCategoryCode,
                DrugCategoryName = x.DrugCategoryName,
                DrugGroupName = x.DrugGroupName,
                DrugCategoryType = x.DrugCategoryType,
                IsAntibiotic = x.IsAntibiotic,
                IsNarcotic = x.IsNarcotic,
                IsPsychotropic = x.IsPsychotropic,
                IsHighAlert = x.IsHighAlert,
                IsChronicDiseaseDrug = x.IsChronicDiseaseDrug,
                IsVaccine = x.IsVaccine,
                IsConsumable = x.IsConsumable,
                IsCoveredByInsuranceDefault = x.IsCoveredByInsuranceDefault,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            string drugCategoryCode,
            string drugCategoryName,
            string drugCategoryType)
        {
            if (string.IsNullOrWhiteSpace(drugCategoryCode))
                return (false, "Kode drug category wajib diisi.");

            if (string.IsNullOrWhiteSpace(drugCategoryName))
                return (false, "Nama drug category wajib diisi.");

            if (string.IsNullOrWhiteSpace(drugCategoryType))
                return (false, "Tipe drug category wajib diisi.");

            if (!AllowedDrugCategoryTypes.Contains(drugCategoryType.Trim()))
            {
                return (false, "Tipe drug category tidak valid. Gunakan salah satu: General, Antibiotic, Analgesic, Antipyretic, Antihypertensive, Antidiabetic, Vitamin, Vaccine, Consumable, Other.");
            }

            var normalizedCode = drugCategoryCode.Trim().ToUpperInvariant();
            var normalizedName = drugCategoryName.Trim().ToLower();

            var duplicateCodeQuery = _dbContext.Set<MstDrugCategory>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.DrugCategoryCode.ToUpper() == normalizedCode);

            if (excludeId.HasValue)
                duplicateCodeQuery = duplicateCodeQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateCodeQuery.AnyAsync())
                return (false, "Kode drug category sudah digunakan.");

            var duplicateNameQuery = _dbContext.Set<MstDrugCategory>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.DrugCategoryName.ToLower() == normalizedName);

            if (excludeId.HasValue)
                duplicateNameQuery = duplicateNameQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateNameQuery.AnyAsync())
                return (false, "Nama drug category sudah digunakan.");

            return (true, null);
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            if (pageSize < 1)
                pageSize = 25;

            if (pageSize > 100)
                pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static string NormalizeDrugCategoryType(string value)
        {
            var trimmed = value.Trim();

            var matched = AllowedDrugCategoryTypes
                .FirstOrDefault(x => string.Equals(x, trimmed, StringComparison.OrdinalIgnoreCase));

            return matched ?? "General";
        }

        private static string? NormalizeNullableString(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private Guid GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdValue, out var userId)
                ? userId
                : Guid.Empty;
        }
    }
}