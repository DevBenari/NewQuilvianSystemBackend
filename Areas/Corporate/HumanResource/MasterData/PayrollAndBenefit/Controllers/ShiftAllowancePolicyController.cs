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
    [Route("api/v1/corporate/human-resource/master-data/shift-allowance-policies")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Shift Allowance Policy",
        AreaName = "Corporate",
        ControllerName = "ShiftAllowancePolicy",
        Description = "Corporate human resource master data shift allowance policy",
        SortOrder = 50)]
    [Tags("Corporate / Human Resource / Master Data / Shift Allowance Policy")]
    public class ShiftAllowancePolicyController : ControllerBase
    {
        private static readonly HashSet<string> AllowedMethods =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "FixedPerShift",
                "PerHour",
                "PercentageOfBaseSalary"
            };

        private const string CodePrefix = "SAP-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public ShiftAllowancePolicyController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Shift Allowance Policy", Description = "Melihat metadata filter shift allowance policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ShiftAllowancePolicy", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new ShiftAllowancePolicyFilterMetadataResponse
            {
                DefaultFilter = new ShiftAllowancePolicyDefaultFilterResponse(),
                CalculationMethodOptions = AllowedMethods
                    .OrderBy(x => x)
                    .Select(x => new ShiftAllowancePolicyStringOptionResponse
                    {
                        Value = x,
                        Label = x
                    })
                    .ToList(),

                SortOptions = new List<ShiftAllowancePolicySortOptionResponse>
                {
                    new() { Value = "priority", Label = "Prioritas" },
                    new() { Value = "name", Label = "Nama" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            return Ok(ApiResponse<ShiftAllowancePolicyFilterMetadataResponse>.Ok(
                result,
                "Metadata filter berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Shift Allowance Policy", Description = "Melihat ringkasan shift allowance policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ShiftAllowancePolicy", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();

            var result = new ShiftAllowancePolicySummaryResponse
            {
                TotalData = await query.CountAsync(),
                ActiveData = await query.CountAsync(x => x.IsActive),
                InactiveData = await query.CountAsync(x => !x.IsActive),
                DefaultData = await query.CountAsync(x => x.IsDefault),
                NightShiftOnlyData = await query.CountAsync(x => x.ApplyOnlyNightShift),
                AttendanceMatchRequiredData = await query.CountAsync(x => x.RequireAttendanceMatch)
            };

            return Ok(ApiResponse<ShiftAllowancePolicySummaryResponse>.Ok(
                result,
                "Ringkasan berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Shift Allowance Policy", Description = "Melihat data shift allowance policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ShiftAllowancePolicy", "Read")]
        public async Task<IActionResult> GetData(
            [FromQuery] Guid? allowanceTypeId,
            [FromQuery] Guid? shiftId,
            [FromQuery] Guid? shiftGroupId,
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

            if (shiftId.HasValue)
                query = query.Where(x => x.ShiftId == shiftId.Value);

            if (shiftGroupId.HasValue)
                query = query.Where(x => x.ShiftGroupId == shiftGroupId.Value);

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
                    x.ShiftAllowancePolicyName.ToLower().Contains(keyword) ||
                    x.ShiftAllowancePolicyCode.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();

            var ordered = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderByDescending(x => x.Priority).ThenByDescending(x => x.ShiftAllowancePolicyName)
                : query.OrderBy(x => x.Priority).ThenBy(x => x.ShiftAllowancePolicyName);

            var items = await ordered
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ShiftAllowancePolicyResponse
                {
                    Id = x.Id,
                    AllowanceTypeId = x.AllowanceTypeId,
                    ShiftId = x.ShiftId,
                    ShiftGroupId = x.ShiftGroupId,
                    HospitalSiteId = x.HospitalSiteId,
                    OrganizationUnitId = x.OrganizationUnitId,
                    EmployeeCategoryId = x.EmployeeCategoryId,
                    EmploymentTypeId = x.EmploymentTypeId,
                    ShiftAllowancePolicyCode = x.ShiftAllowancePolicyCode,
                    ShiftAllowancePolicyName = x.ShiftAllowancePolicyName,
                    CalculationMethod = x.CalculationMethod,
                    CurrencyCode = x.CurrencyCode,
                    RateAmount = x.RateAmount,
                    PercentageOfBaseSalary = x.PercentageOfBaseSalary,
                    MinimumEligibleMinutes = x.MinimumEligibleMinutes,
                    MaximumAmountPerPeriod = x.MaximumAmountPerPeriod,
                    ApplyOnWorkday = x.ApplyOnWorkday,
                    ApplyOnWeekend = x.ApplyOnWeekend,
                    ApplyOnHoliday = x.ApplyOnHoliday,
                    ApplyOnlyNightShift = x.ApplyOnlyNightShift,
                    RequireAttendanceMatch = x.RequireAttendanceMatch,
                    RequireCompletedShift = x.RequireCompletedShift,
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

            var result = new PagedResult<ShiftAllowancePolicyResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<PagedResult<ShiftAllowancePolicyResponse>>.Ok(
                result,
                "Data berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Shift Allowance Policy", Description = "Melihat pilihan shift allowance policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ShiftAllowancePolicy", "Read")]
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
                    x.ShiftAllowancePolicyName.ToLower().Contains(keyword) ||
                    x.ShiftAllowancePolicyCode.ToLower().Contains(keyword));
            }

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.Priority)
                .ThenBy(x => x.ShiftAllowancePolicyName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ShiftAllowancePolicyOptionResponse
                {
                    Id = x.Id,
                    AllowanceTypeId = x.AllowanceTypeId,
                    ShiftAllowancePolicyCode = x.ShiftAllowancePolicyCode,
                    ShiftAllowancePolicyName = x.ShiftAllowancePolicyName,
                    CalculationMethod = x.CalculationMethod,
                    RateAmount = x.RateAmount,
                    IsDefault = x.IsDefault
                })
                .ToListAsync();

            var result = new ShiftAllowancePolicyOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ShiftAllowancePolicyOptionPagedResponse>.Ok(
                result,
                "Pilihan data berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Shift Allowance Policy", Description = "Melihat detail shift allowance policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ShiftAllowancePolicy", "Read")]
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

            var result = new ShiftAllowancePolicyDetailResponse
            {
                    Id = entity.Id,
                    AllowanceTypeId = entity.AllowanceTypeId,
                    ShiftId = entity.ShiftId,
                    ShiftGroupId = entity.ShiftGroupId,
                    HospitalSiteId = entity.HospitalSiteId,
                    OrganizationUnitId = entity.OrganizationUnitId,
                    EmployeeCategoryId = entity.EmployeeCategoryId,
                    EmploymentTypeId = entity.EmploymentTypeId,
                    ShiftAllowancePolicyCode = entity.ShiftAllowancePolicyCode,
                    ShiftAllowancePolicyName = entity.ShiftAllowancePolicyName,
                    CalculationMethod = entity.CalculationMethod,
                    CurrencyCode = entity.CurrencyCode,
                    RateAmount = entity.RateAmount,
                    PercentageOfBaseSalary = entity.PercentageOfBaseSalary,
                    MinimumEligibleMinutes = entity.MinimumEligibleMinutes,
                    MaximumAmountPerPeriod = entity.MaximumAmountPerPeriod,
                    ApplyOnWorkday = entity.ApplyOnWorkday,
                    ApplyOnWeekend = entity.ApplyOnWeekend,
                    ApplyOnHoliday = entity.ApplyOnHoliday,
                    ApplyOnlyNightShift = entity.ApplyOnlyNightShift,
                    RequireAttendanceMatch = entity.RequireAttendanceMatch,
                    RequireCompletedShift = entity.RequireCompletedShift,
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

            return Ok(ApiResponse<ShiftAllowancePolicyDetailResponse>.Ok(
                result,
                "Detail berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Shift Allowance Policy", Description = "Membuat shift allowance policy", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("ShiftAllowancePolicy", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateShiftAllowancePolicyRequest request)
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

            var entity = new MstShiftAllowancePolicy
            {
                Id = Guid.NewGuid(),
                ShiftAllowancePolicyCode = await GenerateCodeAsync(),
                AllowanceTypeId = request.AllowanceTypeId,
                ShiftId = NormalizeGuid(request.ShiftId),
                ShiftGroupId = NormalizeGuid(request.ShiftGroupId),
                HospitalSiteId = NormalizeGuid(request.HospitalSiteId),
                OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId),
                EmployeeCategoryId = NormalizeGuid(request.EmployeeCategoryId),
                EmploymentTypeId = NormalizeGuid(request.EmploymentTypeId),
                ShiftAllowancePolicyName = request.ShiftAllowancePolicyName.Trim(),
                CalculationMethod = request.CalculationMethod.Trim(),
                CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant(),
                RateAmount = request.RateAmount,
                PercentageOfBaseSalary = request.PercentageOfBaseSalary,
                MinimumEligibleMinutes = request.MinimumEligibleMinutes,
                MaximumAmountPerPeriod = request.MaximumAmountPerPeriod,
                ApplyOnWorkday = request.ApplyOnWorkday,
                ApplyOnWeekend = request.ApplyOnWeekend,
                ApplyOnHoliday = request.ApplyOnHoliday,
                ApplyOnlyNightShift = request.ApplyOnlyNightShift,
                RequireAttendanceMatch = request.RequireAttendanceMatch,
                RequireCompletedShift = request.RequireCompletedShift,
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

            _dbContext.Set<MstShiftAllowancePolicy>().Add(entity);
            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                new { entity.Id },
                "Data berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Shift Allowance Policy", Description = "Mengubah shift allowance policy", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("ShiftAllowancePolicy", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateShiftAllowancePolicyRequest request)
        {
            var entity = await _dbContext.Set<MstShiftAllowancePolicy>()
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
            entity.ShiftId = NormalizeGuid(request.ShiftId);
            entity.ShiftGroupId = NormalizeGuid(request.ShiftGroupId);
            entity.HospitalSiteId = NormalizeGuid(request.HospitalSiteId);
            entity.OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId);
            entity.EmployeeCategoryId = NormalizeGuid(request.EmployeeCategoryId);
            entity.EmploymentTypeId = NormalizeGuid(request.EmploymentTypeId);
            entity.ShiftAllowancePolicyName = request.ShiftAllowancePolicyName.Trim();
            entity.CalculationMethod = request.CalculationMethod.Trim();
            entity.CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant();
            entity.RateAmount = request.RateAmount;
            entity.PercentageOfBaseSalary = request.PercentageOfBaseSalary;
            entity.MinimumEligibleMinutes = request.MinimumEligibleMinutes;
            entity.MaximumAmountPerPeriod = request.MaximumAmountPerPeriod;
            entity.ApplyOnWorkday = request.ApplyOnWorkday;
            entity.ApplyOnWeekend = request.ApplyOnWeekend;
            entity.ApplyOnHoliday = request.ApplyOnHoliday;
            entity.ApplyOnlyNightShift = request.ApplyOnlyNightShift;
            entity.RequireAttendanceMatch = request.RequireAttendanceMatch;
            entity.RequireCompletedShift = request.RequireCompletedShift;
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
        [AccessAction("Update", "Update Shift Allowance Policy Status", Description = "Mengubah status shift allowance policy", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("ShiftAllowancePolicy", "Update")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] UpdateShiftAllowancePolicyStatusRequest request)
        {
            var entity = await _dbContext.Set<MstShiftAllowancePolicy>()
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
        [AccessAction("Delete", "Delete Shift Allowance Policy", Description = "Menghapus shift allowance policy", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("ShiftAllowancePolicy", "Delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var entity = await _dbContext.Set<MstShiftAllowancePolicy>()
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

        private IQueryable<MstShiftAllowancePolicy> BuildBaseQuery()
        {
            return _dbContext.Set<MstShiftAllowancePolicy>()
                .AsNoTracking()

                .Where(x => !x.IsDelete);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateShiftAllowancePolicyRequest request)
        {
            if (request.AllowanceTypeId == Guid.Empty)
                return (false, "Allowance type wajib dipilih.");

            if (string.IsNullOrWhiteSpace(request.ShiftAllowancePolicyName))
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

            if (!AllowedMethods.Contains(request.CalculationMethod.Trim()))
                return (false, "Calculation method tidak valid.");

            if (request.CalculationMethod.Equals("PercentageOfBaseSalary", StringComparison.OrdinalIgnoreCase) &&
                request.PercentageOfBaseSalary <= 0)
            {
                return (false, "PercentageOfBaseSalary harus lebih dari 0.");
            }

            if (request.ShiftId.HasValue &&
                !await _dbContext.Set<MstShift>().AnyAsync(x =>
                    x.Id == request.ShiftId.Value &&
                    x.IsActive &&
                    !x.IsDelete))
            {
                return (false, "Shift tidak valid.");
            }

            if (request.ShiftGroupId.HasValue &&
                !await _dbContext.Set<MstShiftGroup>().AnyAsync(x =>
                    x.Id == request.ShiftGroupId.Value &&
                    x.IsActive &&
                    !x.IsDelete))
            {
                return (false, "Shift group tidak valid.");
            }

            var normalizedName = request.ShiftAllowancePolicyName.Trim().ToLower();

            var duplicateQuery = _dbContext.Set<MstShiftAllowancePolicy>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.ShiftAllowancePolicyName.ToLower() == normalizedName);

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
            var rows = await _dbContext.Set<MstShiftAllowancePolicy>()
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
            var codes = await _dbContext.Set<MstShiftAllowancePolicy>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.ShiftAllowancePolicyCode.StartsWith(CodePrefix))
                .Select(x => x.ShiftAllowancePolicyCode)
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
