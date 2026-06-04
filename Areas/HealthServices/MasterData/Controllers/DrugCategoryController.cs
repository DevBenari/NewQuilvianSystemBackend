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
        private const string DrugCategoryCodePrefix = "DGC-RSMMC-";
        private const int DrugCategoryCodeDigitLength = 5;

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
                CustomPeriods = BuildCustomPeriodOptions(),
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
                DrugCategoryTypeOptions = AllowedDrugCategoryTypes.OrderBy(x => x).ToList(),
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateFieldMetadata(),
                UpdateFields = BuildUpdateFieldMetadata()
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

            var query = _dbContext.Set<MstDrugCategory>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            query = ApplyDateFilter(query, startDate, endDate, customPeriod);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

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
        [ProducesResponseType(typeof(ApiResponse<DrugCategoryOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Drug Category", Description = "Melihat data pilihan drug category", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DrugCategory", "Read")]
        public async Task<IActionResult> GetDrugCategoryOptions(
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = _dbContext.Set<MstDrugCategory>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.DrugCategoryCode.ToLower().Contains(keyword) ||
                    x.DrugCategoryName.ToLower().Contains(keyword) ||
                    x.DrugCategoryType.ToLower().Contains(keyword) ||
                    (x.DrugGroupName != null && x.DrugGroupName.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.DrugCategoryName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
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

            var result = new DrugCategoryOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<DrugCategoryOptionPagedResponse>.Ok(
                result,
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
            var data = await _dbContext.Set<MstDrugCategory>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
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
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Drug Category", Description = "Membuat data drug category", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("DrugCategory", "Create")]
        public async Task<IActionResult> CreateDrugCategory([FromBody] CreateDrugCategoryRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
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
                DrugCategoryCode = await GenerateDrugCategoryCodeAsync(),
                DrugCategoryName = request.DrugCategoryName.Trim(),
                DrugGroupName = NormalizeNullableText(request.DrugGroupName),
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
                Description = NormalizeNullableText(request.Description),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstDrugCategory>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = ToCreateUpdateResponse(entity);

            await _loggerService.InfoAsync(
                LogCategory,
                "DrugCategory.CreateDrugCategory",
                "Membuat data drug category.",
                response
            );

            return Ok(ApiResponse<DrugCategoryCreateResponse>.Ok(
                response,
                "Drug category berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DrugCategoryUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
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

            entity.DrugCategoryName = request.DrugCategoryName.Trim();
            entity.DrugGroupName = NormalizeNullableText(request.DrugGroupName);
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
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var response = ToUpdateResponse(entity);

            return Ok(ApiResponse<DrugCategoryUpdateResponse>.Ok(
                response,
                "Drug category berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
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

            var isUsedByDrug = await _dbContext.Set<MstDrug>()
                .AsNoTracking()
                .AnyAsync(x => x.DrugCategoryId == id && !x.IsDelete);

            if (isUsedByDrug)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Drug category tidak dapat dihapus karena sudah digunakan oleh data drug."
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
                "Drug category berhasil dihapus."
            ));
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            string drugCategoryName,
            string drugCategoryType)
        {
            if (string.IsNullOrWhiteSpace(drugCategoryName))
                return (false, "Nama drug category wajib diisi.");

            if (string.IsNullOrWhiteSpace(drugCategoryType))
                return (false, "Tipe drug category wajib diisi.");

            if (!AllowedDrugCategoryTypes.Contains(drugCategoryType.Trim()))
            {
                return (false, "Tipe drug category tidak valid. Gunakan salah satu: General, Antibiotic, Analgesic, Antipyretic, Antihypertensive, Antidiabetic, Vitamin, Vaccine, Consumable, Other.");
            }

            var normalizedName = drugCategoryName.Trim().ToLower();

            var duplicateName = await _dbContext.Set<MstDrugCategory>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.DrugCategoryName.ToLower() == normalizedName &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateName)
                return (false, "Nama drug category sudah digunakan.");

            return (true, null);
        }

        private async Task<string> GenerateDrugCategoryCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstDrugCategory>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.DrugCategoryCode.StartsWith(DrugCategoryCodePrefix))
                .Select(x => x.DrugCategoryCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(ParseDrugCategoryCodeNumber)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToHashSet();

            var nextNumber = 1;
            while (usedNumbers.Contains(nextNumber))
                nextNumber++;

            return $"{DrugCategoryCodePrefix}{nextNumber.ToString($"D{DrugCategoryCodeDigitLength}")}";
        }

        private static int? ParseDrugCategoryCodeNumber(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;

            if (!code.StartsWith(DrugCategoryCodePrefix, StringComparison.OrdinalIgnoreCase))
                return null;

            var numberText = code[DrugCategoryCodePrefix.Length..];

            return int.TryParse(numberText, out var number)
                ? number
                : null;
        }

        private static IQueryable<MstDrugCategory> ApplyDateFilter(
            IQueryable<MstDrugCategory> query,
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            var period = customPeriod?.Trim().ToLowerInvariant();
            var today = DateTime.UtcNow.Date;

            if (!string.IsNullOrWhiteSpace(period) && period != "custom")
            {
                switch (period)
                {
                    case "today":
                        startDate = today;
                        endDate = today;
                        break;
                    case "yesterday":
                        startDate = today.AddDays(-1);
                        endDate = today.AddDays(-1);
                        break;
                    case "last7days":
                        startDate = today.AddDays(-6);
                        endDate = today;
                        break;
                    case "last30days":
                        startDate = today.AddDays(-29);
                        endDate = today;
                        break;
                    case "thismonth":
                        startDate = new DateTime(today.Year, today.Month, 1);
                        endDate = today;
                        break;
                    case "lastmonth":
                        var firstDayThisMonth = new DateTime(today.Year, today.Month, 1);
                        var firstDayLastMonth = firstDayThisMonth.AddMonths(-1);
                        startDate = firstDayLastMonth;
                        endDate = firstDayThisMonth.AddDays(-1);
                        break;
                }
            }

            if (startDate.HasValue)
                query = query.Where(x => x.CreateDateTime >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(x => x.CreateDateTime < endDate.Value.Date.AddDays(1));

            return query;
        }

        private static IQueryable<MstDrugCategory> ApplySorting(
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

        private static DrugCategoryCreateResponse ToCreateUpdateResponse(MstDrugCategory entity)
        {
            return new DrugCategoryCreateResponse
            {
                Id = entity.Id,
                DrugCategoryCode = entity.DrugCategoryCode,
                DrugCategoryName = entity.DrugCategoryName,
                DrugGroupName = entity.DrugGroupName,
                DrugCategoryType = entity.DrugCategoryType,
                IsActive = entity.IsActive
            };
        }

        private static DrugCategoryUpdateResponse ToUpdateResponse(MstDrugCategory entity)
        {
            return new DrugCategoryUpdateResponse
            {
                Id = entity.Id,
                DrugCategoryCode = entity.DrugCategoryCode,
                DrugCategoryName = entity.DrugCategoryName,
                DrugGroupName = entity.DrugGroupName,
                DrugCategoryType = entity.DrugCategoryType,
                IsActive = entity.IsActive
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static string NormalizeDrugCategoryType(string value)
        {
            var trimmed = value.Trim();

            var matched = AllowedDrugCategoryTypes
                .FirstOrDefault(x => string.Equals(x, trimmed, StringComparison.OrdinalIgnoreCase));

            return matched ?? "General";
        }

        private Guid GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdValue, out var userId)
                ? userId
                : Guid.Empty;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static List<DrugCategoryCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<DrugCategoryCustomPeriodOptionResponse>
            {
                new() { Value = "custom", Label = "Custom", Description = "Gunakan startDate dan endDate manual.", UsesStartDate = true, UsesEndDate = true },
                new() { Value = "today", Label = "Hari ini", Description = "Data yang dibuat hari ini.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "yesterday", Label = "Kemarin", Description = "Data yang dibuat kemarin.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last7days", Label = "7 hari terakhir", Description = "Data yang dibuat dalam 7 hari terakhir termasuk hari ini.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last30days", Label = "30 hari terakhir", Description = "Data yang dibuat dalam 30 hari terakhir termasuk hari ini.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thismonth", Label = "Bulan ini", Description = "Data yang dibuat dari awal bulan ini sampai hari ini.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "lastmonth", Label = "Bulan lalu", Description = "Data yang dibuat pada bulan sebelumnya.", UsesStartDate = false, UsesEndDate = false }
            };
        }

        private static List<DrugCategoryQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<DrugCategoryQueryParameterInfoResponse>
            {
                new() { Name = "startDate", Type = "DateTime?", Description = "Tanggal awal berdasarkan CreateDateTime. Dipakai jika customPeriod kosong atau custom.", Example = "2026-01-01" },
                new() { Name = "endDate", Type = "DateTime?", Description = "Tanggal akhir berdasarkan CreateDateTime. Dipakai jika customPeriod kosong atau custom.", Example = "2026-01-31" },
                new() { Name = "customPeriod", Type = "string", Description = "Pilihan periode cepat: custom, today, yesterday, last7days, last30days, thismonth, lastmonth.", Example = "last30days" },
                new() { Name = "isActive", Type = "bool?", Description = "Filter status aktif." },
                new() { Name = "search", Type = "string", Description = "Pencarian text berdasarkan kode, nama, tipe, group, dan deskripsi." },
                new() { Name = "sortBy", Type = "string", Description = "Kolom sorting." },
                new() { Name = "sortDirection", Type = "string", Description = "Arah sorting: asc atau desc." },
                new() { Name = "pageNumber", Type = "int", Description = "Nomor halaman." },
                new() { Name = "pageSize", Type = "int", Description = "Jumlah data per halaman, maksimal 100." }
            };
        }

        private static List<DrugCategoryFormFieldMetadataResponse> BuildCreateFieldMetadata()
        {
            return new List<DrugCategoryFormFieldMetadataResponse>
            {
                new() { Name = "drugCategoryCode", Label = "Kode kategori obat", DataType = "string", InputType = "text", Required = false, IsReadonly = true, Placeholder = "Auto generated", Description = "Dibuat otomatis oleh sistem dengan format DGC-RSMMC-00001." },
                new() { Name = "drugCategoryName", Label = "Nama kategori obat", DataType = "string", InputType = "text", Required = true, IsReadonly = false },
                new() { Name = "drugGroupName", Label = "Group obat", DataType = "string", InputType = "text", Required = false, IsReadonly = false },
                new() { Name = "drugCategoryType", Label = "Tipe kategori obat", DataType = "string", InputType = "select", Required = true, IsReadonly = false, Description = "Ambil pilihan dari DrugCategoryTypeOptions." },
                new() { Name = "isAntibiotic", Label = "Antibiotik", DataType = "bool", InputType = "switch", Required = false, IsReadonly = false },
                new() { Name = "isNarcotic", Label = "Narkotik", DataType = "bool", InputType = "switch", Required = false, IsReadonly = false },
                new() { Name = "isPsychotropic", Label = "Psikotropik", DataType = "bool", InputType = "switch", Required = false, IsReadonly = false },
                new() { Name = "isHighAlert", Label = "High alert", DataType = "bool", InputType = "switch", Required = false, IsReadonly = false },
                new() { Name = "isChronicDiseaseDrug", Label = "Obat penyakit kronis", DataType = "bool", InputType = "switch", Required = false, IsReadonly = false },
                new() { Name = "isVaccine", Label = "Vaksin", DataType = "bool", InputType = "switch", Required = false, IsReadonly = false },
                new() { Name = "isConsumable", Label = "Consumable", DataType = "bool", InputType = "switch", Required = false, IsReadonly = false },
                new() { Name = "isCoveredByInsuranceDefault", Label = "Default ditanggung asuransi", DataType = "bool", InputType = "switch", Required = false, IsReadonly = false },
                new() { Name = "sortOrder", Label = "Urutan", DataType = "int", InputType = "number", Required = false, IsReadonly = false },
                new() { Name = "description", Label = "Deskripsi", DataType = "string", InputType = "textarea", Required = false, IsReadonly = false }
            };
        }

        private static List<DrugCategoryFormFieldMetadataResponse> BuildUpdateFieldMetadata()
        {
            var fields = BuildCreateFieldMetadata();

            fields.Add(new DrugCategoryFormFieldMetadataResponse
            {
                Name = "isActive",
                Label = "Status aktif",
                DataType = "bool",
                InputType = "switch",
                Required = false,
                IsReadonly = false
            });

            return fields;
        }
    }
}
