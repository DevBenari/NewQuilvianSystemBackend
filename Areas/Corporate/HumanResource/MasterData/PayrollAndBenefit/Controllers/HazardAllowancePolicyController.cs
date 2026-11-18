using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
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
    [Route("api/v1/corporate/payroll-management/master-data/hazard-allowance-policies")]
    // Alias kompatibilitas: rute di bawah ini adalah rute sebelum Payroll dipisah dari HRD; dihapus bertahap setelah seluruh consumer berpindah.
    [Route("api/v1/corporate/human-resource/master-data/hazard-allowance-policies")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Hazard Allowance Policy",
        AreaName = "Corporate",
        ControllerName = "HazardAllowancePolicy",
        Description = "Corporate human resource master data hazard allowance policy",
        SortOrder = 52)]
    [Tags("Corporate / Payroll Management / Master Data / Hazard Allowance Policy")]
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
        private const int MaxCodeGenerationAttempts = 3;
        private const string LogCategory = "Corporate.PayrollManagement.MasterData";

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
                CustomPeriods = BuildPeriodOptions(),
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
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
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

            query = WorkflowMasterDataSupport.ApplyDateFilter(query, startDate, endDate, customPeriod);
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
            [FromBody] CreateHazardAllowancePolicyRequest? request)
        {
            if (request == null)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Data kebijakan tunjangan risiko wajib diisi."));
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

            var entity = new MstHazardAllowancePolicy
            {
                Id = Guid.NewGuid(),
                AllowanceTypeId = request.AllowanceTypeId,
                LegalEntityId = NormalizeGuid(request.LegalEntityId),
                HospitalSiteId = NormalizeGuid(request.HospitalSiteId),
                OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId),
                WorkLocationId = NormalizeGuid(request.WorkLocationId),
                EmployeeCategoryId = NormalizeGuid(request.EmployeeCategoryId),
                EmploymentTypeId = NormalizeGuid(request.EmploymentTypeId),
                HazardAllowancePolicyName = request.HazardAllowancePolicyName.Trim(),
                HazardLevel = NormalizeHazardLevel(request.HazardLevel),
                CalculationMethod = NormalizeMethod(request.CalculationMethod),
                CurrencyCode = NormalizeCurrencyCode(request.CurrencyCode),
                RateAmount = request.RateAmount,
                PercentageOfBaseSalary = request.PercentageOfBaseSalary,
                MinimumExposureDays = request.MinimumExposureDays,
                MaximumAmountPerPeriod = request.MaximumAmountPerPeriod,
                RequireOccupationalHealthClearance = request.RequireOccupationalHealthClearance,
                RequireActiveAssignment = request.RequireActiveAssignment,
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

            for (var attempt = 1; attempt <= MaxCodeGenerationAttempts; attempt++)
            {
                entity.HazardAllowancePolicyCode = await GenerateCodeAsync();
                _dbContext.Set<MstHazardAllowancePolicy>().Add(entity);

                try
                {
                    await _dbContext.SaveChangesAsync();
                    break;
                }
                catch (DbUpdateException ex) when (IsPolicyCodeUniqueViolation(ex) && attempt < MaxCodeGenerationAttempts)
                {
                    _dbContext.Entry(entity).State = EntityState.Detached;
                }
                catch (DbUpdateException ex)
                {
                    await TryLogErrorAsync("HazardAllowancePolicy.Create", "Gagal menyimpan hazard allowance policy.", ex, new { entity.Id, entity.HazardAllowancePolicyCode });
                    var mapped = TryMapDatabaseException(ex);
                    if (mapped != null) return mapped;
                    throw;
                }
            }

            return Ok(ApiResponse<object>.Ok(
                new { entity.Id },
                "Data berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Hazard Allowance Policy", Description = "Mengubah hazard allowance policy", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("HazardAllowancePolicy", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateHazardAllowancePolicyRequest? request)
        {
            if (request == null)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Data kebijakan tunjangan risiko wajib diisi."));
            }

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
                return BuildValidationErrorResult(
                    validation.StatusCode,
                    validation.ErrorMessage ?? "Data tidak valid.");
            }

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();

            entity.AllowanceTypeId = request.AllowanceTypeId;
            entity.LegalEntityId = NormalizeGuid(request.LegalEntityId);
            entity.HospitalSiteId = NormalizeGuid(request.HospitalSiteId);
            entity.OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId);
            entity.WorkLocationId = NormalizeGuid(request.WorkLocationId);
            entity.EmployeeCategoryId = NormalizeGuid(request.EmployeeCategoryId);
            entity.EmploymentTypeId = NormalizeGuid(request.EmploymentTypeId);
            entity.HazardAllowancePolicyName = request.HazardAllowancePolicyName.Trim();
            entity.HazardLevel = NormalizeHazardLevel(request.HazardLevel);
            entity.CalculationMethod = NormalizeMethod(request.CalculationMethod);
            entity.CurrencyCode = NormalizeCurrencyCode(request.CurrencyCode);
            entity.RateAmount = request.RateAmount;
            entity.PercentageOfBaseSalary = request.PercentageOfBaseSalary;
            entity.MinimumExposureDays = request.MinimumExposureDays;
            entity.MaximumAmountPerPeriod = request.MaximumAmountPerPeriod;
            entity.RequireOccupationalHealthClearance = request.RequireOccupationalHealthClearance;
            entity.RequireActiveAssignment = request.RequireActiveAssignment;
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
                await TryLogErrorAsync("HazardAllowancePolicy.Update", "Gagal memperbarui hazard allowance policy.", ex, new { entity.Id, entity.HazardAllowancePolicyCode });
                var mapped = TryMapDatabaseException(ex);
                if (mapped != null) return mapped;
                throw;
            }

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

        private async Task<(bool IsValid, int StatusCode, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateHazardAllowancePolicyRequest request)
        {
            if (request.AllowanceTypeId == Guid.Empty)
                return (false, StatusCodes.Status400BadRequest, "Allowance type wajib dipilih.");

            if (string.IsNullOrWhiteSpace(request.HazardAllowancePolicyName))
                return (false, StatusCodes.Status400BadRequest, "Nama wajib diisi.");

            if (string.IsNullOrWhiteSpace(request.HazardLevel))
                return (false, StatusCodes.Status400BadRequest, "Hazard level wajib diisi.");

            if (!AllowedHazardLevels.Contains(request.HazardLevel.Trim()))
                return (false, StatusCodes.Status400BadRequest, "Hazard level tidak valid.");

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
                request.MinimumExposureDays < 0 ||
                (request.MaximumAmountPerPeriod.HasValue && request.MaximumAmountPerPeriod.Value < 0) ||
                request.Priority < 0)
            {
                return (false, StatusCodes.Status400BadRequest, "Nilai tarif, persentase, hari paparan minimum, batas maksimum, dan prioritas tidak valid.");
            }

            if (calculationMethod.Equals("PercentageOfBaseSalary", StringComparison.OrdinalIgnoreCase) &&
                request.PercentageOfBaseSalary <= 0)
            {
                return (false, StatusCodes.Status400BadRequest, "PercentageOfBaseSalary harus lebih dari 0.");
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

            if (NormalizeGuid(request.LegalEntityId) is { } legalEntityId &&
                !await _dbContext.Set<MstLegalEntity>().AnyAsync(x => x.Id == legalEntityId && x.IsActive && !x.IsDelete))
            {
                return (false, StatusCodes.Status400BadRequest, "Legal entity tidak ditemukan atau tidak aktif.");
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

            if (NormalizeGuid(request.WorkLocationId) is { } workLocationId &&
                !await _dbContext.Set<MstWorkLocation>().AnyAsync(x => x.Id == workLocationId && x.IsActive && !x.IsDelete))
            {
                return (false, StatusCodes.Status400BadRequest, "Work location tidak valid atau tidak aktif.");
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

            var normalizedName = request.HazardAllowancePolicyName.Trim().ToLower();

            var duplicateQuery = _dbContext.Set<MstHazardAllowancePolicy>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.HazardAllowancePolicyName.ToLower() == normalizedName);

            if (excludeId.HasValue)
                duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateQuery.AnyAsync())
                return (false, StatusCodes.Status409Conflict, "Nama kebijakan tunjangan risiko sudah digunakan.");

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

        private static bool IsPolicyCodeUniqueViolation(DbUpdateException exception)
        {
            var pgException = exception.InnerException as PostgresException
                ?? exception.GetBaseException() as PostgresException;

            return pgException != null &&
                pgException.SqlState == PostgresErrorCodes.UniqueViolation &&
                !string.IsNullOrWhiteSpace(pgException.ConstraintName) &&
                pgException.ConstraintName.Contains("HazardAllowancePolicyCode", StringComparison.OrdinalIgnoreCase);
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
                    "Data kebijakan tunjangan risiko bentrok dengan data yang sudah ada. Muat ulang daftar sebelum mencoba kembali.")),
                "23503" => BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Referensi data terkait pada kebijakan tunjangan risiko tidak valid atau tidak ditemukan.")),
                "22001" or "22003" or "22007" or "23502" => BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Format atau batas nilai pada data kebijakan tunjangan risiko tidak memenuhi ketentuan.")),
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

        private static string NormalizeHazardLevel(string value) =>
            AllowedHazardLevels.First(x => x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));

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

        private static List<HazardAllowancePolicyCustomPeriodOptionResponse> BuildPeriodOptions()
        {
            return new List<HazardAllowancePolicyCustomPeriodOptionResponse>
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "lastmonth", Label = "Bulan lalu" }
            };
        }
    }
}
