using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/hazard-allowance-policies")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Hazard Allowance Policy",
        AreaName = "Corporate",
        ControllerName = "HazardAllowancePolicy",
        Description = "Corporate human resource master data hazard allowance policy",
        SortOrder = 52)]
    [Tags("Corporate / Human Resource / Master Data / Hazard Allowance Policy")]
    public class HazardAllowancePolicyController : ControllerBase
    {
        private static readonly HashSet<string> AllowedHazardLevels =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "Low",
                "Medium",
                "High",
                "Critical"
            };

        private static readonly HashSet<string> AllowedMethods =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "FixedMonthly",
                "PerDay",
                "PerShift",
                "PercentageOfBaseSalary"
            };

        private const string CodePrefix = "HAP-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public HazardAllowancePolicyController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Hazard Allowance Policy", Description = "Melihat metadata filter hazard allowance policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("HazardAllowancePolicy", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new HazardAllowancePolicyFilterMetadataResponse
            {
                DefaultFilter = new HazardAllowancePolicyDefaultFilterResponse(),
                HazardLevelOptions = AllowedHazardLevels
                    .OrderBy(x => x)
                    .Select(x => new HazardAllowancePolicyStringOptionResponse
                    {
                        Value = x,
                        Label = x
                    })
                    .ToList(),
                CalculationMethodOptions = AllowedMethods
                    .OrderBy(x => x)
                    .Select(x => new HazardAllowancePolicyStringOptionResponse
                    {
                        Value = x,
                        Label = x
                    })
                    .ToList(),

                SortOptions = new List<HazardAllowancePolicySortOptionResponse>
                {
                    new() { Value = "priority", Label = "Prioritas" },
                    new() { Value = "name", Label = "Nama" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            return Ok(ApiResponse<HazardAllowancePolicyFilterMetadataResponse>.Ok(
                result,
                "Metadata filter berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Hazard Allowance Policy", Description = "Melihat ringkasan hazard allowance policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("HazardAllowancePolicy", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();

            var result = new HazardAllowancePolicySummaryResponse
            {
                TotalData = await query.CountAsync(),
                ActiveData = await query.CountAsync(x => x.IsActive),
                InactiveData = await query.CountAsync(x => !x.IsActive),
                DefaultData = await query.CountAsync(x => x.IsDefault),
                HealthClearanceRequiredData = await query.CountAsync(x => x.RequireOccupationalHealthClearance),
                ActiveAssignmentRequiredData = await query.CountAsync(x => x.RequireActiveAssignment)
            };

            return Ok(ApiResponse<HazardAllowancePolicySummaryResponse>.Ok(
                result,
                "Ringkasan berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Hazard Allowance Policy", Description = "Melihat data hazard allowance policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("HazardAllowancePolicy", "Read")]
        public async Task<IActionResult> GetData(
            [FromQuery] Guid? allowanceTypeId,
            [FromQuery] string? hazardLevel,
            [FromQuery] string? calculationMethod,
            [FromQuery] bool? isDefault,

            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "priority",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 25 : Math.Min(pageSize, 100);

            var query = BuildBaseQuery();

            if (allowanceTypeId.HasValue)
                query = query.Where(x => x.AllowanceTypeId == allowanceTypeId.Value);

            if (!string.IsNullOrWhiteSpace(hazardLevel))
                query = query.Where(x => x.HazardLevel == hazardLevel.Trim());

            if (!string.IsNullOrWhiteSpace(calculationMethod))
                query = query.Where(x => x.CalculationMethod == calculationMethod.Trim());

            if (isDefault.HasValue)
                query = query.Where(x => x.IsDefault == isDefault.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.HazardAllowancePolicyName.ToLower().Contains(keyword) ||
                    x.HazardAllowancePolicyCode.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();

            var ordered = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderByDescending(x => x.Priority).ThenByDescending(x => x.HazardAllowancePolicyName)
                : query.OrderBy(x => x.Priority).ThenBy(x => x.HazardAllowancePolicyName);

            var items = await ordered
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new HazardAllowancePolicyResponse
                {
                    Id = x.Id,
                    AllowanceTypeId = x.AllowanceTypeId,
                    LegalEntityId = x.LegalEntityId,
                    HospitalSiteId = x.HospitalSiteId,
                    OrganizationUnitId = x.OrganizationUnitId,
                    WorkLocationId = x.WorkLocationId,
                    EmployeeCategoryId = x.EmployeeCategoryId,
                    EmploymentTypeId = x.EmploymentTypeId,
                    HazardAllowancePolicyCode = x.HazardAllowancePolicyCode,
                    HazardAllowancePolicyName = x.HazardAllowancePolicyName,
                    HazardLevel = x.HazardLevel,
                    CalculationMethod = x.CalculationMethod,
                    CurrencyCode = x.CurrencyCode,
                    RateAmount = x.RateAmount,
                    PercentageOfBaseSalary = x.PercentageOfBaseSalary,
                    MinimumExposureDays = x.MinimumExposureDays,
                    MaximumAmountPerPeriod = x.MaximumAmountPerPeriod,
                    RequireOccupationalHealthClearance = x.RequireOccupationalHealthClearance,
                    RequireActiveAssignment = x.RequireActiveAssignment,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    Description = x.Description,
                    Priority = x.Priority,
                    IsDefault = x.IsDefault,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy
                })
                .ToListAsync();

            var result = new PagedResult<HazardAllowancePolicyResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<PagedResult<HazardAllowancePolicyResponse>>.Ok(
                result,
                "Data berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Hazard Allowance Policy", Description = "Melihat pilihan hazard allowance policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("HazardAllowancePolicy", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 25 : Math.Min(pageSize, 100);

            var query = BuildBaseQuery();

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.HazardAllowancePolicyName.ToLower().Contains(keyword) ||
                    x.HazardAllowancePolicyCode.ToLower().Contains(keyword));
            }

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.Priority)
                .ThenBy(x => x.HazardAllowancePolicyName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new HazardAllowancePolicyOptionResponse
                {
                    Id = x.Id,
                    AllowanceTypeId = x.AllowanceTypeId,
                    HazardAllowancePolicyCode = x.HazardAllowancePolicyCode,
                    HazardAllowancePolicyName = x.HazardAllowancePolicyName,
                    HazardLevel = x.HazardLevel,
                    CalculationMethod = x.CalculationMethod,
                    RateAmount = x.RateAmount,
                    IsDefault = x.IsDefault
                })
                .ToListAsync();

            var result = new HazardAllowancePolicyOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<HazardAllowancePolicyOptionPagedResponse>.Ok(
                result,
                "Pilihan data berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Hazard Allowance Policy", Description = "Melihat detail hazard allowance policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("HazardAllowancePolicy", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Data tidak ditemukan."));
            }

            var result = new HazardAllowancePolicyDetailResponse
            {
                    Id = entity.Id,
                    AllowanceTypeId = entity.AllowanceTypeId,
                    LegalEntityId = entity.LegalEntityId,
                    HospitalSiteId = entity.HospitalSiteId,
                    OrganizationUnitId = entity.OrganizationUnitId,
                    WorkLocationId = entity.WorkLocationId,
                    EmployeeCategoryId = entity.EmployeeCategoryId,
                    EmploymentTypeId = entity.EmploymentTypeId,
                    HazardAllowancePolicyCode = entity.HazardAllowancePolicyCode,
                    HazardAllowancePolicyName = entity.HazardAllowancePolicyName,
                    HazardLevel = entity.HazardLevel,
                    CalculationMethod = entity.CalculationMethod,
                    CurrencyCode = entity.CurrencyCode,
                    RateAmount = entity.RateAmount,
                    PercentageOfBaseSalary = entity.PercentageOfBaseSalary,
                    MinimumExposureDays = entity.MinimumExposureDays,
                    MaximumAmountPerPeriod = entity.MaximumAmountPerPeriod,
                    RequireOccupationalHealthClearance = entity.RequireOccupationalHealthClearance,
                    RequireActiveAssignment = entity.RequireActiveAssignment,
                    EffectiveStartDate = entity.EffectiveStartDate,
                    EffectiveEndDate = entity.EffectiveEndDate,
                    Description = entity.Description,
                    Priority = entity.Priority,
                    IsDefault = entity.IsDefault,
                    IsActive = entity.IsActive,
                    CreateDateTime = entity.CreateDateTime,
                    CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy
            };

            return Ok(ApiResponse<HazardAllowancePolicyDetailResponse>.Ok(
                result,
                "Detail berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Hazard Allowance Policy", Description = "Membuat hazard allowance policy", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("HazardAllowancePolicy", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateHazardAllowancePolicyRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data tidak valid."));
            }

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();

            var entity = new MstHazardAllowancePolicy
            {
                Id = Guid.NewGuid(),
                HazardAllowancePolicyCode = await GenerateCodeAsync(),
                AllowanceTypeId = request.AllowanceTypeId,
                LegalEntityId = NormalizeGuid(request.LegalEntityId),
                HospitalSiteId = NormalizeGuid(request.HospitalSiteId),
                OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId),
                WorkLocationId = NormalizeGuid(request.WorkLocationId),
                EmployeeCategoryId = NormalizeGuid(request.EmployeeCategoryId),
                EmploymentTypeId = NormalizeGuid(request.EmploymentTypeId),
                HazardAllowancePolicyName = request.HazardAllowancePolicyName.Trim(),
                HazardLevel = request.HazardLevel.Trim(),
                CalculationMethod = request.CalculationMethod.Trim(),
                CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant(),
                RateAmount = request.RateAmount,
                PercentageOfBaseSalary = request.PercentageOfBaseSalary,
                MinimumExposureDays = request.MinimumExposureDays,
                MaximumAmountPerPeriod = request.MaximumAmountPerPeriod,
                RequireOccupationalHealthClearance = request.RequireOccupationalHealthClearance,
                RequireActiveAssignment = request.RequireActiveAssignment,
                EffectiveStartDate = request.EffectiveStartDate?.Date,
                EffectiveEndDate = request.EffectiveEndDate?.Date,
                Description = NormalizeText(request.Description),
                Priority = request.Priority,
                IsDefault = request.IsDefault,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actor,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstHazardAllowancePolicy>().Add(entity);
            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                new { entity.Id },
                "Data berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Hazard Allowance Policy", Description = "Mengubah hazard allowance policy", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("HazardAllowancePolicy", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateHazardAllowancePolicyRequest request)
        {
            var entity = await _dbContext.Set<MstHazardAllowancePolicy>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Data tidak ditemukan."));
            }

            var validation = await ValidateRequestAsync(id, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data tidak valid."));
            }

            entity.AllowanceTypeId = request.AllowanceTypeId;
            entity.LegalEntityId = NormalizeGuid(request.LegalEntityId);
            entity.HospitalSiteId = NormalizeGuid(request.HospitalSiteId);
            entity.OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId);
            entity.WorkLocationId = NormalizeGuid(request.WorkLocationId);
            entity.EmployeeCategoryId = NormalizeGuid(request.EmployeeCategoryId);
            entity.EmploymentTypeId = NormalizeGuid(request.EmploymentTypeId);
            entity.HazardAllowancePolicyName = request.HazardAllowancePolicyName.Trim();
            entity.HazardLevel = request.HazardLevel.Trim();
            entity.CalculationMethod = request.CalculationMethod.Trim();
            entity.CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant();
            entity.RateAmount = request.RateAmount;
            entity.PercentageOfBaseSalary = request.PercentageOfBaseSalary;
            entity.MinimumExposureDays = request.MinimumExposureDays;
            entity.MaximumAmountPerPeriod = request.MaximumAmountPerPeriod;
            entity.RequireOccupationalHealthClearance = request.RequireOccupationalHealthClearance;
            entity.RequireActiveAssignment = request.RequireActiveAssignment;
            entity.EffectiveStartDate = request.EffectiveStartDate?.Date;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.Description = NormalizeText(request.Description);
            entity.Priority = request.Priority;
            entity.IsDefault = request.IsDefault;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Data berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Hazard Allowance Policy Status", Description = "Mengubah status hazard allowance policy", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("HazardAllowancePolicy", "Update")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] UpdateHazardAllowancePolicyStatusRequest request)
        {
            var entity = await _dbContext.Set<MstHazardAllowancePolicy>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Data tidak ditemukan."));
            }

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();

            entity.IsActive = request.IsActive;

            if (request.IsDefault == true)
            {
                await UnsetOtherDefaultAsync(id, entity.AllowanceTypeId, now, actor);
            }

            entity.IsDefault = request.IsDefault ?? entity.IsDefault;
            if (!request.IsActive)
                entity.IsDefault = false;

            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Hazard Allowance Policy", Description = "Menghapus hazard allowance policy", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("HazardAllowancePolicy", "Delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var entity = await _dbContext.Set<MstHazardAllowancePolicy>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Data tidak ditemukan."));
            }

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Data berhasil dihapus."));
        }

        private IQueryable<MstHazardAllowancePolicy> BuildBaseQuery()
        {
            return _dbContext.Set<MstHazardAllowancePolicy>()
                .AsNoTracking()

                .Where(x => !x.IsDelete);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateHazardAllowancePolicyRequest request)
        {
            if (request.AllowanceTypeId == Guid.Empty)
                return (false, "Allowance type wajib dipilih.");

            if (string.IsNullOrWhiteSpace(request.HazardAllowancePolicyName))
                return (false, "Nama wajib diisi.");

            if (request.EffectiveStartDate.HasValue &&
                request.EffectiveEndDate.HasValue &&
                request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Value.Date)
            {
                return (false, "EffectiveEndDate tidak boleh sebelum EffectiveStartDate.");
            }

            if (!await _dbContext.Set<MstAllowanceType>().AnyAsync(x =>
                    x.Id == request.AllowanceTypeId &&
                    x.IsActive &&
                    !x.IsDelete))
            {
                return (false, "Allowance type tidak ditemukan atau tidak aktif.");
            }

            if (!AllowedHazardLevels.Contains(request.HazardLevel.Trim()))
                return (false, "Hazard level tidak valid.");

            if (!AllowedMethods.Contains(request.CalculationMethod.Trim()))
                return (false, "Calculation method tidak valid.");

            if (request.WorkLocationId.HasValue &&
                !await _dbContext.Set<MstWorkLocation>().AnyAsync(x =>
                    x.Id == request.WorkLocationId.Value &&
                    x.IsActive &&
                    !x.IsDelete))
            {
                return (false, "Work location tidak valid.");
            }

            var normalizedName = request.HazardAllowancePolicyName.Trim().ToLower();

            var duplicateQuery = _dbContext.Set<MstHazardAllowancePolicy>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.HazardAllowancePolicyName.ToLower() == normalizedName);

            if (excludeId.HasValue)
                duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateQuery.AnyAsync())
                return (false, "Nama sudah digunakan.");

            return (true, null);
        }


        private async Task UnsetOtherDefaultAsync(
            Guid exceptId,
            Guid allowanceTypeId,
            DateTime now,
            Guid actor)
        {
            var rows = await _dbContext.Set<MstHazardAllowancePolicy>()
                .Where(x =>
                    !x.IsDelete &&
                    x.IsDefault &&
                    x.AllowanceTypeId == allowanceTypeId &&
                    x.Id != exceptId)
                .ToListAsync();

            foreach (var row in rows)
            {
                row.IsDefault = false;
                row.UpdateDateTime = now;
                row.UpdateBy = actor;
            }
        }

        private async Task<string> GenerateCodeAsync()
        {
            var codes = await _dbContext.Set<MstHazardAllowancePolicy>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.HazardAllowancePolicyCode.StartsWith(CodePrefix))
                .Select(x => x.HazardAllowancePolicyCode)
                .ToListAsync();

            var used = codes
                .Select(x => x.Replace(CodePrefix, string.Empty))
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .ToHashSet();

            var next = 1;

            while (used.Contains(next))
                next++;

            return CodePrefix + next.ToString().PadLeft(CodeNumberLength, '0');
        }

        private Guid GetCurrentUserId()
        {
            var value =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("user_id");

            return Guid.TryParse(value, out var id)
                ? id
                : Guid.Empty;
        }

        private static Guid? NormalizeGuid(Guid? value)
        {
            return !value.HasValue || value.Value == Guid.Empty
                ? null
                : value.Value;
        }

        private static string? NormalizeText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}
