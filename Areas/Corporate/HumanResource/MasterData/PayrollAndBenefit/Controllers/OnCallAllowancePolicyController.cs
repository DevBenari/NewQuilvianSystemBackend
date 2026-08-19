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
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Controllers;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/on-call-allowance-policies")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "On Call Allowance Policy",
        AreaName = "Corporate",
        ControllerName = "OnCallAllowancePolicy",
        Description = "Corporate human resource master data on call allowance policy",
        SortOrder = 51)]
    [Tags("Corporate / Human Resource / Master Data / On Call Allowance Policy")]
    public class OnCallAllowancePolicyController : ControllerBase
    {
        private static readonly HashSet<string> AllowedMethods =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "FixedPerAssignment",
                "PerHour",
                "PerDay",
                "PerActualCall",
                "PercentageOfBaseSalary"
            };

        private const string CodePrefix = "OAP-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public OnCallAllowancePolicyController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read On Call Allowance Policy", Description = "Melihat metadata filter on call allowance policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OnCallAllowancePolicy", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new OnCallAllowancePolicyFilterMetadataResponse
            {
                DefaultFilter = new OnCallAllowancePolicyDefaultFilterResponse(),
                CustomPeriods = BuildPeriodOptions(),
                CalculationMethodOptions = AllowedMethods
                    .OrderBy(x => x)
                    .Select(x => new OnCallAllowancePolicyStringOptionResponse
                    {
                        Value = x,
                        Label = x
                    })
                    .ToList(),

                SortOptions = new List<OnCallAllowancePolicySortOptionResponse>
                {
                    new() { Value = "priority", Label = "Prioritas" },
                    new() { Value = "name", Label = "Nama" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            return Ok(ApiResponse<OnCallAllowancePolicyFilterMetadataResponse>.Ok(
                result,
                "Metadata filter berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read On Call Allowance Policy", Description = "Melihat ringkasan on call allowance policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OnCallAllowancePolicy", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();

            var result = new OnCallAllowancePolicySummaryResponse
            {
                TotalData = await query.CountAsync(),
                ActiveData = await query.CountAsync(x => x.IsActive),
                InactiveData = await query.CountAsync(x => !x.IsActive),
                DefaultData = await query.CountAsync(x => x.IsDefault),
                AttendanceEvidenceRequiredData = await query.CountAsync(x => x.RequireAttendanceEvidence),
                SupervisorVerificationRequiredData = await query.CountAsync(x => x.RequireSupervisorVerification)
            };

            return Ok(ApiResponse<OnCallAllowancePolicySummaryResponse>.Ok(
                result,
                "Ringkasan berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read On Call Allowance Policy", Description = "Melihat data on call allowance policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OnCallAllowancePolicy", "Read")]
        public async Task<IActionResult> GetData(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? allowanceTypeId,
            [FromQuery] Guid? onCallTypeId,
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

            if (onCallTypeId.HasValue)
                query = query.Where(x => x.OnCallTypeId == onCallTypeId.Value);

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
                    x.OnCallAllowancePolicyName.ToLower().Contains(keyword) ||
                    x.OnCallAllowancePolicyCode.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            query = WorkflowMasterDataSupport.ApplyDateFilter(query, startDate, endDate, customPeriod);
            var totalData = await query.CountAsync();

            var ordered = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderByDescending(x => x.Priority).ThenByDescending(x => x.OnCallAllowancePolicyName)
                : query.OrderBy(x => x.Priority).ThenBy(x => x.OnCallAllowancePolicyName);

            var items = await ordered
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new OnCallAllowancePolicyResponse
                {
                    Id = x.Id,
                    AllowanceTypeId = x.AllowanceTypeId,
                    OnCallTypeId = x.OnCallTypeId,
                    HospitalSiteId = x.HospitalSiteId,
                    OrganizationUnitId = x.OrganizationUnitId,
                    EmployeeCategoryId = x.EmployeeCategoryId,
                    EmploymentTypeId = x.EmploymentTypeId,
                    OnCallAllowancePolicyCode = x.OnCallAllowancePolicyCode,
                    OnCallAllowancePolicyName = x.OnCallAllowancePolicyName,
                    CalculationMethod = x.CalculationMethod,
                    CurrencyCode = x.CurrencyCode,
                    BaseRateAmount = x.BaseRateAmount,
                    ActualCallRateAmount = x.ActualCallRateAmount,
                    HourlyRateAmount = x.HourlyRateAmount,
                    PercentageOfBaseSalary = x.PercentageOfBaseSalary,
                    MinimumOnCallHours = x.MinimumOnCallHours,
                    MaximumAmountPerPeriod = x.MaximumAmountPerPeriod,
                    WeekendMultiplier = x.WeekendMultiplier,
                    HolidayMultiplier = x.HolidayMultiplier,
                    RequireAttendanceEvidence = x.RequireAttendanceEvidence,
                    RequireSupervisorVerification = x.RequireSupervisorVerification,
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

            var result = new PagedResult<OnCallAllowancePolicyResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<PagedResult<OnCallAllowancePolicyResponse>>.Ok(
                result,
                "Data berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read On Call Allowance Policy", Description = "Melihat pilihan on call allowance policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OnCallAllowancePolicy", "Read")]
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
                    x.OnCallAllowancePolicyName.ToLower().Contains(keyword) ||
                    x.OnCallAllowancePolicyCode.ToLower().Contains(keyword));
            }

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.Priority)
                .ThenBy(x => x.OnCallAllowancePolicyName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new OnCallAllowancePolicyOptionResponse
                {
                    Id = x.Id,
                    AllowanceTypeId = x.AllowanceTypeId,
                    OnCallAllowancePolicyCode = x.OnCallAllowancePolicyCode,
                    OnCallAllowancePolicyName = x.OnCallAllowancePolicyName,
                    CalculationMethod = x.CalculationMethod,
                    BaseRateAmount = x.BaseRateAmount,
                    IsDefault = x.IsDefault
                })
                .ToListAsync();

            var result = new OnCallAllowancePolicyOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<OnCallAllowancePolicyOptionPagedResponse>.Ok(
                result,
                "Pilihan data berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read On Call Allowance Policy", Description = "Melihat detail on call allowance policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OnCallAllowancePolicy", "Read")]
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

            var result = new OnCallAllowancePolicyDetailResponse
            {
                    Id = entity.Id,
                    AllowanceTypeId = entity.AllowanceTypeId,
                    OnCallTypeId = entity.OnCallTypeId,
                    HospitalSiteId = entity.HospitalSiteId,
                    OrganizationUnitId = entity.OrganizationUnitId,
                    EmployeeCategoryId = entity.EmployeeCategoryId,
                    EmploymentTypeId = entity.EmploymentTypeId,
                    OnCallAllowancePolicyCode = entity.OnCallAllowancePolicyCode,
                    OnCallAllowancePolicyName = entity.OnCallAllowancePolicyName,
                    CalculationMethod = entity.CalculationMethod,
                    CurrencyCode = entity.CurrencyCode,
                    BaseRateAmount = entity.BaseRateAmount,
                    ActualCallRateAmount = entity.ActualCallRateAmount,
                    HourlyRateAmount = entity.HourlyRateAmount,
                    PercentageOfBaseSalary = entity.PercentageOfBaseSalary,
                    MinimumOnCallHours = entity.MinimumOnCallHours,
                    MaximumAmountPerPeriod = entity.MaximumAmountPerPeriod,
                    WeekendMultiplier = entity.WeekendMultiplier,
                    HolidayMultiplier = entity.HolidayMultiplier,
                    RequireAttendanceEvidence = entity.RequireAttendanceEvidence,
                    RequireSupervisorVerification = entity.RequireSupervisorVerification,
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

            return Ok(ApiResponse<OnCallAllowancePolicyDetailResponse>.Ok(
                result,
                "Detail berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create On Call Allowance Policy", Description = "Membuat on call allowance policy", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("OnCallAllowancePolicy", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateOnCallAllowancePolicyRequest request)
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

            var entity = new MstOnCallAllowancePolicy
            {
                Id = Guid.NewGuid(),
                OnCallAllowancePolicyCode = await GenerateCodeAsync(),
                AllowanceTypeId = request.AllowanceTypeId,
                OnCallTypeId = NormalizeGuid(request.OnCallTypeId),
                HospitalSiteId = NormalizeGuid(request.HospitalSiteId),
                OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId),
                EmployeeCategoryId = NormalizeGuid(request.EmployeeCategoryId),
                EmploymentTypeId = NormalizeGuid(request.EmploymentTypeId),
                OnCallAllowancePolicyName = request.OnCallAllowancePolicyName.Trim(),
                CalculationMethod = request.CalculationMethod.Trim(),
                CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant(),
                BaseRateAmount = request.BaseRateAmount,
                ActualCallRateAmount = request.ActualCallRateAmount,
                HourlyRateAmount = request.HourlyRateAmount,
                PercentageOfBaseSalary = request.PercentageOfBaseSalary,
                MinimumOnCallHours = request.MinimumOnCallHours,
                MaximumAmountPerPeriod = request.MaximumAmountPerPeriod,
                WeekendMultiplier = request.WeekendMultiplier,
                HolidayMultiplier = request.HolidayMultiplier,
                RequireAttendanceEvidence = request.RequireAttendanceEvidence,
                RequireSupervisorVerification = request.RequireSupervisorVerification,
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

            _dbContext.Set<MstOnCallAllowancePolicy>().Add(entity);
            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                new { entity.Id },
                "Data berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update On Call Allowance Policy", Description = "Mengubah on call allowance policy", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("OnCallAllowancePolicy", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateOnCallAllowancePolicyRequest request)
        {
            var entity = await _dbContext.Set<MstOnCallAllowancePolicy>()
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
            entity.OnCallTypeId = NormalizeGuid(request.OnCallTypeId);
            entity.HospitalSiteId = NormalizeGuid(request.HospitalSiteId);
            entity.OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId);
            entity.EmployeeCategoryId = NormalizeGuid(request.EmployeeCategoryId);
            entity.EmploymentTypeId = NormalizeGuid(request.EmploymentTypeId);
            entity.OnCallAllowancePolicyName = request.OnCallAllowancePolicyName.Trim();
            entity.CalculationMethod = request.CalculationMethod.Trim();
            entity.CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant();
            entity.BaseRateAmount = request.BaseRateAmount;
            entity.ActualCallRateAmount = request.ActualCallRateAmount;
            entity.HourlyRateAmount = request.HourlyRateAmount;
            entity.PercentageOfBaseSalary = request.PercentageOfBaseSalary;
            entity.MinimumOnCallHours = request.MinimumOnCallHours;
            entity.MaximumAmountPerPeriod = request.MaximumAmountPerPeriod;
            entity.WeekendMultiplier = request.WeekendMultiplier;
            entity.HolidayMultiplier = request.HolidayMultiplier;
            entity.RequireAttendanceEvidence = request.RequireAttendanceEvidence;
            entity.RequireSupervisorVerification = request.RequireSupervisorVerification;
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
        [AccessAction("Update", "Update On Call Allowance Policy Status", Description = "Mengubah status on call allowance policy", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("OnCallAllowancePolicy", "Update")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] UpdateOnCallAllowancePolicyStatusRequest request)
        {
            var entity = await _dbContext.Set<MstOnCallAllowancePolicy>()
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
        [AccessAction("Delete", "Delete On Call Allowance Policy", Description = "Menghapus on call allowance policy", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("OnCallAllowancePolicy", "Delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var entity = await _dbContext.Set<MstOnCallAllowancePolicy>()
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

        private IQueryable<MstOnCallAllowancePolicy> BuildBaseQuery()
        {
            return _dbContext.Set<MstOnCallAllowancePolicy>()
                .AsNoTracking()

                .Where(x => !x.IsDelete);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateOnCallAllowancePolicyRequest request)
        {
            if (request.AllowanceTypeId == Guid.Empty)
                return (false, "Allowance type wajib dipilih.");

            if (string.IsNullOrWhiteSpace(request.OnCallAllowancePolicyName))
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

            if (request.WeekendMultiplier < 0 || request.HolidayMultiplier < 0)
                return (false, "Multiplier tidak boleh negatif.");

            if (request.OnCallTypeId.HasValue &&
                !await _dbContext.Set<MstOnCallType>().AnyAsync(x =>
                    x.Id == request.OnCallTypeId.Value &&
                    x.IsActive &&
                    !x.IsDelete))
            {
                return (false, "On-call type tidak valid.");
            }

            var normalizedName = request.OnCallAllowancePolicyName.Trim().ToLower();

            var duplicateQuery = _dbContext.Set<MstOnCallAllowancePolicy>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.OnCallAllowancePolicyName.ToLower() == normalizedName);

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
            var rows = await _dbContext.Set<MstOnCallAllowancePolicy>()
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
            var codes = await _dbContext.Set<MstOnCallAllowancePolicy>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.OnCallAllowancePolicyCode.StartsWith(CodePrefix))
                .Select(x => x.OnCallAllowancePolicyCode)
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

        private static List<OnCallAllowancePolicyCustomPeriodOptionResponse> BuildPeriodOptions()
        {
            return new List<OnCallAllowancePolicyCustomPeriodOptionResponse>
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "lastmonth", Label = "Bulan lalu" }
            };
        }
    }
}
