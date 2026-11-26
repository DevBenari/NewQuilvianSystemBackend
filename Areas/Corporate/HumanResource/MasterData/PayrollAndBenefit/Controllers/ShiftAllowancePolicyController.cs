using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
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
    [Route("api/v1/corporate/payroll-management/master-data/shift-allowance-policies")]
    // Alias kompatibilitas: rute di bawah ini adalah rute sebelum Payroll dipisah dari HRD; dihapus bertahap setelah seluruh consumer berpindah.
    [Route("api/v1/corporate/human-resource/master-data/shift-allowance-policies")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Shift Allowance Policy",
        AreaName = "Corporate",
        ControllerName = "ShiftAllowancePolicy",
        Description = "Corporate human resource master data shift allowance policy",
        SortOrder = 50)]
    [Tags("Corporate / Payroll Management / Master Data / Shift Allowance Policy")]
    public class ShiftAllowancePolicyController : ControllerBase
    {
        private static readonly HashSet<string> AllowedMethods =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "FixedPerShift",
                "PerHour",
                "PercentageOfBaseSalary"
            };

        private const string LogCategory = "Corporate.PayrollManagement.MasterData";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly AllowancePolicyCodeService _allowancePolicyCodeService;

        public ShiftAllowancePolicyController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            AllowancePolicyCodeService allowancePolicyCodeService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _allowancePolicyCodeService = allowancePolicyCodeService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Shift Allowance Policy", Description = "Melihat metadata filter shift allowance policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ShiftAllowancePolicy", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new ShiftAllowancePolicyFilterMetadataResponse
            {
                DefaultFilter = new ShiftAllowancePolicyDefaultFilterResponse(),
                CustomPeriods = BuildPeriodOptions(),
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
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? allowanceTypeId,
            [FromQuery] Guid? salaryGradeId,
            [FromQuery] Guid? salaryStructureId,
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

            // Filter cakupan TERSIMPAN (PAY-BE-FIX-019): Grade Gaji dan Struktur Gaji; kebijakan lama tanpa cakupan tidak ikut bila filter dipakai.
            if (salaryGradeId.HasValue)
                query = query.Where(x => x.SalaryGradeId == salaryGradeId.Value);

            if (salaryStructureId.HasValue)
                query = query.Where(x => x.SalaryStructureId == salaryStructureId.Value);

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

            query = WorkflowMasterDataSupport.ApplyDateFilter(query, startDate, endDate, customPeriod);
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
                    EmployeeGradeId = x.EmployeeGradeId,
                    EmployeeGradeName = x.EmployeeGrade != null ? x.EmployeeGrade.GradeName : null,
                    SalaryGradeId = x.SalaryGradeId,
                    SalaryGradeName = x.SalaryGrade != null ? x.SalaryGrade.SalaryGradeName : null,
                    SalaryStructureId = x.SalaryStructureId,
                    SalaryStructureName = x.SalaryStructure != null ? x.SalaryStructure.SalaryStructureName : null,
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
                .Include(x => x.EmployeeGrade)
                .Include(x => x.SalaryGrade)
                .Include(x => x.SalaryStructure)
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
                    EmployeeGradeId = entity.EmployeeGradeId,
                    EmployeeGradeName = entity.EmployeeGrade?.GradeName,
                    SalaryGradeId = entity.SalaryGradeId,
                    SalaryGradeName = entity.SalaryGrade?.SalaryGradeName,
                    SalaryStructureId = entity.SalaryStructureId,
                    SalaryStructureName = entity.SalaryStructure?.SalaryStructureName,
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
            [FromBody] CreateShiftAllowancePolicyRequest? request)
        {
            if (request == null)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Data kebijakan tunjangan shift wajib diisi."));
            }

            var validation = await ValidateRequestAsync(null, request);

            if (!validation.IsValid)
            {
                return BuildValidationErrorResult(
                    validation.StatusCode,
                    validation.ErrorMessage ?? "Data tidak valid.");
            }

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();

            var entity = new MstShiftAllowancePolicy
            {
                Id = Guid.NewGuid(),
                AllowanceTypeId = request.AllowanceTypeId,
                ShiftId = NormalizeGuid(request.ShiftId),
                ShiftGroupId = NormalizeGuid(request.ShiftGroupId),
                HospitalSiteId = NormalizeGuid(request.HospitalSiteId),
                OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId),
                EmployeeCategoryId = NormalizeGuid(request.EmployeeCategoryId),
                EmploymentTypeId = NormalizeGuid(request.EmploymentTypeId),
                EmployeeGradeId = NormalizeGuid(request.EmployeeGradeId),
                SalaryGradeId = NormalizeGuid(request.SalaryGradeId),
                SalaryStructureId = NormalizeGuid(request.SalaryStructureId),
                ShiftAllowancePolicyName = request.ShiftAllowancePolicyName.Trim(),
                CalculationMethod = NormalizeMethod(request.CalculationMethod),
                CurrencyCode = NormalizeCurrencyCode(request.CurrencyCode),
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
                EffectiveStartDate = NormalizeDateOnly(request.EffectiveStartDate),
                EffectiveEndDate = NormalizeDateOnly(request.EffectiveEndDate),
                Description = NormalizeText(request.Description),
                Priority = request.Priority,
                IsDefault = request.IsDefault,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actor,
                IsDelete = false,
                IsCancel = false
            };

            if (entity.IsDefault)
            {
                await UnsetOtherDefaultAsync(entity.Id, entity.AllowanceTypeId, now, actor);
            }

            try
            {
                entity.ShiftAllowancePolicyCode = await _allowancePolicyCodeService.AllocateShiftPolicyCodeAsync(actor);
            }
            catch (NumberSeriesAllocationException)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Kode kebijakan tunjangan shift gagal diterbitkan. Hubungi administrator sistem."));
            }

            _dbContext.Set<MstShiftAllowancePolicy>().Add(entity);

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                await TryLogErrorAsync("ShiftAllowancePolicy.Create", "Gagal menyimpan shift allowance policy.", ex, new { entity.Id, entity.ShiftAllowancePolicyCode });
                var mapped = TryMapDatabaseException(ex);
                if (mapped != null) return mapped;
                throw;
            }

            return Ok(ApiResponse<object>.Ok(
                new { entity.Id },
                "Data berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Shift Allowance Policy", Description = "Mengubah shift allowance policy", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("ShiftAllowancePolicy", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateShiftAllowancePolicyRequest? request)
        {
            if (request == null)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Data kebijakan tunjangan shift wajib diisi."));
            }

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
                return BuildValidationErrorResult(
                    validation.StatusCode,
                    validation.ErrorMessage ?? "Data tidak valid.");
            }

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();

            entity.AllowanceTypeId = request.AllowanceTypeId;
            entity.ShiftId = NormalizeGuid(request.ShiftId);
            entity.ShiftGroupId = NormalizeGuid(request.ShiftGroupId);
            entity.HospitalSiteId = NormalizeGuid(request.HospitalSiteId);
            entity.OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId);
            entity.EmployeeCategoryId = NormalizeGuid(request.EmployeeCategoryId);
            entity.EmploymentTypeId = NormalizeGuid(request.EmploymentTypeId);
            entity.EmployeeGradeId = NormalizeGuid(request.EmployeeGradeId);
            entity.SalaryGradeId = NormalizeGuid(request.SalaryGradeId);
            entity.SalaryStructureId = NormalizeGuid(request.SalaryStructureId);
            entity.ShiftAllowancePolicyName = request.ShiftAllowancePolicyName.Trim();
            entity.CalculationMethod = NormalizeMethod(request.CalculationMethod);
            entity.CurrencyCode = NormalizeCurrencyCode(request.CurrencyCode);
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
            entity.EffectiveStartDate = NormalizeDateOnly(request.EffectiveStartDate);
            entity.EffectiveEndDate = NormalizeDateOnly(request.EffectiveEndDate);
            entity.Description = NormalizeText(request.Description);
            entity.Priority = request.Priority;
            entity.IsDefault = request.IsActive && request.IsDefault;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;

            if (entity.IsDefault)
            {
                await UnsetOtherDefaultAsync(entity.Id, entity.AllowanceTypeId, now, actor);
            }

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                await TryLogErrorAsync("ShiftAllowancePolicy.Update", "Gagal memperbarui shift allowance policy.", ex, new { entity.Id, entity.ShiftAllowancePolicyCode });
                var mapped = TryMapDatabaseException(ex);
                if (mapped != null) return mapped;
                throw;
            }

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

        private async Task<(bool IsValid, int StatusCode, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateShiftAllowancePolicyRequest request)
        {
            if (request.AllowanceTypeId == Guid.Empty)
                return (false, StatusCodes.Status400BadRequest, "Allowance type wajib dipilih.");

            if (string.IsNullOrWhiteSpace(request.ShiftAllowancePolicyName))
                return (false, StatusCodes.Status400BadRequest, "Nama wajib diisi.");

            if (string.IsNullOrWhiteSpace(request.CalculationMethod))
                return (false, StatusCodes.Status400BadRequest, "Calculation method wajib diisi.");

            var calculationMethod = request.CalculationMethod.Trim();
            if (!AllowedMethods.Contains(calculationMethod))
                return (false, StatusCodes.Status400BadRequest, "Calculation method tidak valid.");

            var currencyCode = NormalizeCurrencyCode(request.CurrencyCode);
            if (currencyCode.Length != 3)
                return (false, StatusCodes.Status400BadRequest, "Kode mata uang harus terdiri dari 3 karakter.");

            if (request.RateAmount < 0 ||
                request.PercentageOfBaseSalary < 0 ||
                request.PercentageOfBaseSalary > 100 ||
                request.MinimumEligibleMinutes < 0 ||
                (request.MaximumAmountPerPeriod.HasValue && request.MaximumAmountPerPeriod.Value < 0) ||
                request.Priority < 0)
            {
                return (false, StatusCodes.Status400BadRequest, "Nilai tarif, persentase, durasi, batas maksimum, dan prioritas tidak valid.");
            }

            var startDate = NormalizeDateOnly(request.EffectiveStartDate);
            var endDate = NormalizeDateOnly(request.EffectiveEndDate);

            if (startDate.HasValue && endDate.HasValue && endDate.Value < startDate.Value)
            {
                return (false, StatusCodes.Status400BadRequest, "EffectiveEndDate tidak boleh sebelum EffectiveStartDate.");
            }

            if (!await _dbContext.Set<MstAllowanceType>().AnyAsync(x =>
                    x.Id == request.AllowanceTypeId &&
                    x.IsActive &&
                    !x.IsDelete))
            {
                return (false, StatusCodes.Status400BadRequest, "Allowance type tidak ditemukan atau tidak aktif.");
            }

            if (calculationMethod.Equals("PercentageOfBaseSalary", StringComparison.OrdinalIgnoreCase) &&
                request.PercentageOfBaseSalary <= 0)
            {
                return (false, StatusCodes.Status400BadRequest, "PercentageOfBaseSalary harus lebih dari 0.");
            }

            if (NormalizeGuid(request.ShiftId) is { } shiftId &&
                !await _dbContext.Set<MstShift>().AnyAsync(x => x.Id == shiftId && x.IsActive && !x.IsDelete))
            {
                return (false, StatusCodes.Status400BadRequest, "Shift tidak valid atau tidak aktif.");
            }

            if (NormalizeGuid(request.ShiftGroupId) is { } shiftGroupId &&
                !await _dbContext.Set<MstShiftGroup>().AnyAsync(x => x.Id == shiftGroupId && x.IsActive && !x.IsDelete))
            {
                return (false, StatusCodes.Status400BadRequest, "Shift group tidak valid atau tidak aktif.");
            }

            if (NormalizeGuid(request.HospitalSiteId) is { } hospitalSiteId &&
                !await _dbContext.Set<MstHospitalSite>().AnyAsync(x => x.Id == hospitalSiteId && x.IsActive && !x.IsDelete))
            {
                return (false, StatusCodes.Status400BadRequest, "Hospital site tidak ditemukan atau tidak aktif.");
            }

            if (NormalizeGuid(request.OrganizationUnitId) is { } organizationUnitId &&
                !await _dbContext.Set<MstOrganizationUnit>().AnyAsync(x => x.Id == organizationUnitId && x.IsActive && !x.IsDelete))
            {
                return (false, StatusCodes.Status400BadRequest, "Organization unit tidak ditemukan atau tidak aktif.");
            }

            if (NormalizeGuid(request.EmployeeCategoryId) is { } employeeCategoryId &&
                !await _dbContext.Set<MstEmployeeCategory>().AnyAsync(x => x.Id == employeeCategoryId && x.IsActive && !x.IsDelete))
            {
                return (false, StatusCodes.Status400BadRequest, "Kategori karyawan tidak ditemukan atau tidak aktif.");
            }

            if (NormalizeGuid(request.EmploymentTypeId) is { } employmentTypeId &&
                !await _dbContext.Set<MstEmploymentType>().AnyAsync(x => x.Id == employmentTypeId && x.IsActive && !x.IsDelete))
            {
                return (false, StatusCodes.Status400BadRequest, "Tipe kepegawaian tidak ditemukan atau tidak aktif.");
            }

            var scopeMessage = await AllowancePolicyScopeValidator.ValidateAsync(
                _dbContext,
                NormalizeGuid(request.EmployeeGradeId),
                NormalizeGuid(request.SalaryGradeId),
                NormalizeGuid(request.SalaryStructureId),
                null,
                NormalizeGuid(request.HospitalSiteId),
                startDate,
                endDate);
            if (scopeMessage is not null)
                return (false, StatusCodes.Status400BadRequest, scopeMessage);

            var normalizedName = request.ShiftAllowancePolicyName.Trim().ToLower();

            var duplicateQuery = _dbContext.Set<MstShiftAllowancePolicy>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.ShiftAllowancePolicyName.ToLower() == normalizedName);

            if (excludeId.HasValue)
                duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateQuery.AnyAsync())
                return (false, StatusCodes.Status409Conflict, "Nama kebijakan tunjangan shift sudah digunakan.");

            return (true, StatusCodes.Status200OK, null);
        }

        private IActionResult BuildValidationErrorResult(int statusCode, string message)
        {
            if (statusCode == StatusCodes.Status409Conflict)
            {
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, message));
            }

            return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, message));
        }

        private IActionResult? TryMapDatabaseException(DbUpdateException exception)
        {
            var pgException = exception.InnerException as PostgresException
                ?? exception.GetBaseException() as PostgresException;

            if (pgException == null)
                return null;

            return pgException.SqlState switch
            {
                PostgresErrorCodes.UniqueViolation => Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict,
                    "Data kebijakan tunjangan shift bentrok dengan data yang sudah ada. Muat ulang daftar sebelum mencoba kembali.")),
                "23503" => BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Referensi data terkait pada kebijakan tunjangan shift tidak valid atau tidak ditemukan.")),
                "22001" or "22003" or "22007" or "23502" => BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Format atau batas nilai pada data kebijakan tunjangan shift tidak memenuhi ketentuan.")),
                _ => null
            };
        }

        private async Task TryLogErrorAsync(string action, string message, Exception exception, object? data = null)
        {
            try
            {
                await _loggerService.ErrorAsync(LogCategory, action, message, exception, data);
            }
            catch
            {
                // Jangan biarkan kegagalan logger menutupi exception utama.
            }
        }

        private static DateTime? NormalizeDateOnly(DateTime? value) =>
            value.HasValue ? DateTime.SpecifyKind(value.Value.Date, DateTimeKind.Unspecified) : null;

        private static string NormalizeMethod(string value) =>
            AllowedMethods.First(x => x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));

        private static string NormalizeCurrencyCode(string? value) =>
            string.IsNullOrWhiteSpace(value) ? "IDR" : value.Trim().ToUpperInvariant();


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

        private static List<ShiftAllowancePolicyCustomPeriodOptionResponse> BuildPeriodOptions()
        {
            return new List<ShiftAllowancePolicyCustomPeriodOptionResponse>
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "lastmonth", Label = "Bulan lalu" }
            };
        }
    }
}
