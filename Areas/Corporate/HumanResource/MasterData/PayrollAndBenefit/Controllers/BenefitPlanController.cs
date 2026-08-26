using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Controllers;

using BenefitPlanPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.DTOs.BenefitPlanResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/benefit-plans")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Benefit Plan",
        AreaName = "Corporate",
        ControllerName = "BenefitPlan",
        Description = "Corporate human resource master data benefit plan",
        SortOrder = 45)]
    [Tags("Corporate / Human Resource / Master Data / Benefit Plan")]
    public class BenefitPlanController : ControllerBase
    {

        private static readonly HashSet<string> AllowedCoverageTypes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "Individual", "Family", "EmployeeAndSpouse", "EmployeeAndChildren"
            };

        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "BFP-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public BenefitPlanController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<BenefitPlanFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Benefit Plan", Description = "Melihat metadata filter benefit plan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BenefitPlan", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new BenefitPlanFilterMetadataResponse
            {
                DefaultFilter = new BenefitPlanDefaultFilterResponse(),
                CustomPeriods = BuildPeriodOptions(),
                SortOptions = new List<BenefitPlanSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "benefitPlanCode", Label = "Kode" },
                    new() { Value = "benefitPlanName", Label = "Nama" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "BenefitPlan.GetFilterMetadata",
                "Mengambil metadata filter benefit plan.",
                result);

            return Ok(ApiResponse<BenefitPlanFilterMetadataResponse>.Ok(
                result,
                "Metadata filter benefit plan berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<BenefitPlanSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Benefit Plan", Description = "Melihat ringkasan benefit plan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BenefitPlan", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();

            var result = new BenefitPlanSummaryResponse
            {
                TotalBenefitPlan = await query.CountAsync(),
                ActiveBenefitPlan = await query.CountAsync(x => x.IsActive),
                InactiveBenefitPlan = await query.CountAsync(x => !x.IsActive),
                DefaultBenefitPlan = await query.CountAsync(x => x.IsDefault),
                FamilyCoveragePlan = await query.CountAsync(x => x.CoverageType != "Individual"),
                EnrollmentOpenPlan = await query.CountAsync(x => (!x.EnrollmentStartDate.HasValue || x.EnrollmentStartDate.Value <= DateTime.UtcNow.Date) && (!x.EnrollmentEndDate.HasValue || x.EnrollmentEndDate.Value >= DateTime.UtcNow.Date))
            };

            return Ok(ApiResponse<BenefitPlanSummaryResponse>.Ok(
                result,
                "Ringkasan benefit plan berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<BenefitPlanPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Benefit Plan", Description = "Melihat data benefit plan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BenefitPlan", "Read")]
        public async Task<IActionResult> GetBenefitPlans(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "benefitPlanName",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.BenefitPlanCode.ToLower().Contains(keyword) ||
                    x.BenefitPlanName.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            query = WorkflowMasterDataSupport.ApplyDateFilter(query, startDate, endDate, customPeriod);
            var totalData = await query.CountAsync();

            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            query = sortBy?.Trim().ToLowerInvariant() switch
            {
                "createdatetime" => desc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),
                "isactive" => desc
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),
                _ => desc
                    ? query.OrderByDescending(x => x.BenefitPlanName)
                    : query.OrderBy(x => x.BenefitPlanName)
            };

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new BenefitPlanResponse
                {
                    Id = x.Id,
                    BenefitTypeId = x.BenefitTypeId,
                    BenefitTypeCode = x.BenefitType != null ? x.BenefitType.BenefitTypeCode : null,
                    BenefitTypeName = x.BenefitType != null ? x.BenefitType.BenefitTypeName : null,
                    LegalEntityId = x.LegalEntityId,
                    HospitalSiteId = x.HospitalSiteId,
                    OrganizationUnitId = x.OrganizationUnitId,
                    EmployeeCategoryId = x.EmployeeCategoryId,
                    EmploymentTypeId = x.EmploymentTypeId,
                    BenefitPlanCode = x.BenefitPlanCode,
                    BenefitPlanName = x.BenefitPlanName,
                    ProviderName = x.ProviderName,
                    ExternalPlanCode = x.ExternalPlanCode,
                    PolicyNumber = x.PolicyNumber,
                    CoverageType = x.CoverageType,
                    CurrencyCode = x.CurrencyCode,
                    CoverageLimitAmount = x.CoverageLimitAmount,
                    EmployerContributionAmount = x.EmployerContributionAmount,
                    EmployerContributionPercentage = x.EmployerContributionPercentage,
                    EmployeeContributionAmount = x.EmployeeContributionAmount,
                    EmployeeContributionPercentage = x.EmployeeContributionPercentage,
                    WaitingPeriodMonths = x.WaitingPeriodMonths,
                    MaximumDependents = x.MaximumDependents,
                    EnrollmentStartDate = x.EnrollmentStartDate,
                    EnrollmentEndDate = x.EnrollmentEndDate,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    Description = x.Description,
                    IsDefault = x.IsDefault,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    EligibilityRuleCount = x.EligibilityRules.Count(r => !r.IsDelete),
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty ? null : _dbContext.Users.Where(u => u.Id == x.CreateBy).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault()
                })
                .ToListAsync();

            var result = new BenefitPlanPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<BenefitPlanPagedResult>.Ok(
                result,
                "Data benefit plan berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<BenefitPlanOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Benefit Plan", Description = "Melihat pilihan benefit plan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BenefitPlan", "Read")]
        public async Task<IActionResult> GetBenefitPlanOptions(
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.BenefitPlanCode.ToLower().Contains(keyword) ||
                    x.BenefitPlanName.ToLower().Contains(keyword));
            }

            var totalData = await query.CountAsync();

            var rows = await query
                .OrderBy(x => x.BenefitPlanName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = rows
                .Select(x => new BenefitPlanOptionResponse
                {
                    Id = x.Id,
                    BenefitPlanCode = x.BenefitPlanCode,
                    BenefitPlanName = x.BenefitPlanName,
                    BenefitTypeId = x.BenefitTypeId,
                    BenefitTypeCode = x.BenefitType != null ? x.BenefitType.BenefitTypeCode : null,
                    BenefitTypeName = x.BenefitType != null ? x.BenefitType.BenefitTypeName : null,
                    LegalEntityId = x.LegalEntityId,
                    HospitalSiteId = x.HospitalSiteId,
                    OrganizationUnitId = x.OrganizationUnitId,
                    EmployeeCategoryId = x.EmployeeCategoryId
                })
                .ToList();

            return Ok(ApiResponse<BenefitPlanOptionPagedResponse>.Ok(
                new BenefitPlanOptionPagedResponse
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data pilihan benefit plan berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<BenefitPlanDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Benefit Plan", Description = "Melihat detail benefit plan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BenefitPlan", "Read")]
        public async Task<IActionResult> GetBenefitPlanById(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new BenefitPlanDetailResponse
                {
                    Id = x.Id,
                    BenefitTypeId = x.BenefitTypeId,
                    BenefitTypeCode = x.BenefitType != null ? x.BenefitType.BenefitTypeCode : null,
                    BenefitTypeName = x.BenefitType != null ? x.BenefitType.BenefitTypeName : null,
                    LegalEntityId = x.LegalEntityId,
                    HospitalSiteId = x.HospitalSiteId,
                    OrganizationUnitId = x.OrganizationUnitId,
                    EmployeeCategoryId = x.EmployeeCategoryId,
                    EmploymentTypeId = x.EmploymentTypeId,
                    BenefitPlanCode = x.BenefitPlanCode,
                    BenefitPlanName = x.BenefitPlanName,
                    ProviderName = x.ProviderName,
                    ExternalPlanCode = x.ExternalPlanCode,
                    PolicyNumber = x.PolicyNumber,
                    CoverageType = x.CoverageType,
                    CurrencyCode = x.CurrencyCode,
                    CoverageLimitAmount = x.CoverageLimitAmount,
                    EmployerContributionAmount = x.EmployerContributionAmount,
                    EmployerContributionPercentage = x.EmployerContributionPercentage,
                    EmployeeContributionAmount = x.EmployeeContributionAmount,
                    EmployeeContributionPercentage = x.EmployeeContributionPercentage,
                    WaitingPeriodMonths = x.WaitingPeriodMonths,
                    MaximumDependents = x.MaximumDependents,
                    EnrollmentStartDate = x.EnrollmentStartDate,
                    EnrollmentEndDate = x.EnrollmentEndDate,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    Description = x.Description,
                    IsDefault = x.IsDefault,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    EligibilityRuleCount = x.EligibilityRules.Count(r => !r.IsDelete),
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty ? null : _dbContext.Users.Where(u => u.Id == x.CreateBy).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault(),
                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                    UpdateByName = x.UpdateBy == Guid.Empty
                        ? null
                        : _dbContext.Users
                            .Where(u => u.Id == x.UpdateBy)
                            .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
                            .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Benefit Plan tidak ditemukan."));
            }

            return Ok(ApiResponse<BenefitPlanDetailResponse>.Ok(
                data,
                "Detail benefit plan berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<BenefitPlanCreateResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create Benefit Plan", Description = "Membuat data benefit plan", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("BenefitPlan", "Create")]
        public async Task<IActionResult> CreateBenefitPlan(
            [FromBody] CreateBenefitPlanRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data benefit plan tidak valid."));
            }

            var entity = new MstBenefitPlan
            {
                Id = Guid.NewGuid(),
                BenefitPlanCode = await GenerateCodeAsync(),
                BenefitTypeId = request.BenefitTypeId,
                LegalEntityId = NormalizeGuid(request.LegalEntityId),
                HospitalSiteId = NormalizeGuid(request.HospitalSiteId),
                OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId),
                EmployeeCategoryId = NormalizeGuid(request.EmployeeCategoryId),
                EmploymentTypeId = NormalizeGuid(request.EmploymentTypeId),
                BenefitPlanName = request.BenefitPlanName.Trim(),
                ProviderName = NormalizeNullableString(request.ProviderName),
                ExternalPlanCode = NormalizeNullableString(request.ExternalPlanCode),
                PolicyNumber = NormalizeNullableString(request.PolicyNumber),
                CoverageType = NormalizeCoverageType(request.CoverageType),
                CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant(),
                CoverageLimitAmount = request.CoverageLimitAmount,
                EmployerContributionAmount = request.EmployerContributionAmount,
                EmployerContributionPercentage = request.EmployerContributionPercentage,
                EmployeeContributionAmount = request.EmployeeContributionAmount,
                EmployeeContributionPercentage = request.EmployeeContributionPercentage,
                WaitingPeriodMonths = request.WaitingPeriodMonths,
                MaximumDependents = request.MaximumDependents,
                EnrollmentStartDate = request.EnrollmentStartDate?.Date,
                EnrollmentEndDate = request.EnrollmentEndDate?.Date,
                EffectiveStartDate = request.EffectiveStartDate?.Date,
                EffectiveEndDate = request.EffectiveEndDate?.Date,
                Description = NormalizeNullableString(request.Description),
                IsDefault = request.IsDefault,
                SortOrder = request.SortOrder,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstBenefitPlan>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var result = new BenefitPlanCreateResponse
            {
                Id = entity.Id,
                BenefitPlanCode = entity.BenefitPlanCode,
                BenefitPlanName = entity.BenefitPlanName,
                IsActive = entity.IsActive
            };

            return Ok(ApiResponse<BenefitPlanCreateResponse>.Ok(
                result,
                "Benefit Plan berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Benefit Plan", Description = "Mengubah data benefit plan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("BenefitPlan", "Update")]
        public async Task<IActionResult> UpdateBenefitPlan(
            Guid id,
            [FromBody] UpdateBenefitPlanRequest request)
        {
            var entity = await _dbContext.Set<MstBenefitPlan>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Benefit Plan tidak ditemukan."));
            }

            var validation = await ValidateRequestAsync(id, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data benefit plan tidak valid."));
            }

            entity.BenefitTypeId = request.BenefitTypeId;
            entity.LegalEntityId = NormalizeGuid(request.LegalEntityId);
            entity.HospitalSiteId = NormalizeGuid(request.HospitalSiteId);
            entity.OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId);
            entity.EmployeeCategoryId = NormalizeGuid(request.EmployeeCategoryId);
            entity.EmploymentTypeId = NormalizeGuid(request.EmploymentTypeId);
            entity.BenefitPlanName = request.BenefitPlanName.Trim();
            entity.ProviderName = NormalizeNullableString(request.ProviderName);
            entity.ExternalPlanCode = NormalizeNullableString(request.ExternalPlanCode);
            entity.PolicyNumber = NormalizeNullableString(request.PolicyNumber);
            entity.CoverageType = NormalizeCoverageType(request.CoverageType);
            entity.CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant();
            entity.CoverageLimitAmount = request.CoverageLimitAmount;
            entity.EmployerContributionAmount = request.EmployerContributionAmount;
            entity.EmployerContributionPercentage = request.EmployerContributionPercentage;
            entity.EmployeeContributionAmount = request.EmployeeContributionAmount;
            entity.EmployeeContributionPercentage = request.EmployeeContributionPercentage;
            entity.WaitingPeriodMonths = request.WaitingPeriodMonths;
            entity.MaximumDependents = request.MaximumDependents;
            entity.EnrollmentStartDate = request.EnrollmentStartDate?.Date;
            entity.EnrollmentEndDate = request.EnrollmentEndDate?.Date;
            entity.EffectiveStartDate = request.EffectiveStartDate?.Date;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.Description = NormalizeNullableString(request.Description);
            entity.IsDefault = request.IsDefault;
            entity.SortOrder = request.SortOrder;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Benefit Plan berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Benefit Plan Status", Description = "Mengubah status benefit plan", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("BenefitPlan", "Update")]
        public async Task<IActionResult> UpdateBenefitPlanStatus(
            Guid id,
            [FromBody] UpdateBenefitPlanStatusRequest request)
        {
            var entity = await _dbContext.Set<MstBenefitPlan>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Benefit Plan tidak ditemukan."));
            }

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status benefit plan berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Delete", "Delete Benefit Plan", Description = "Menghapus benefit plan", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("BenefitPlan", "Delete")]
        public async Task<IActionResult> DeleteBenefitPlan(Guid id)
        {
            var entity = await _dbContext.Set<MstBenefitPlan>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Benefit Plan tidak ditemukan."));
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
                "Benefit Plan berhasil dihapus."));
        }

        private IQueryable<MstBenefitPlan> BuildBaseQuery()
        {
            return _dbContext.Set<MstBenefitPlan>()
                .AsNoTracking()
                .Include(x => x.BenefitType).Include(x => x.EligibilityRules)
                .Where(x => !x.IsDelete);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateBenefitPlanRequest request)
        {

            if (request.BenefitTypeId == Guid.Empty)
                return (false, "Benefit type wajib dipilih.");

            if (string.IsNullOrWhiteSpace(request.BenefitPlanName))
                return (false, "Nama benefit plan wajib diisi.");

            if (!AllowedCoverageTypes.Contains(request.CoverageType.Trim()))
                return (false, "Coverage type tidak valid.");

            if (string.IsNullOrWhiteSpace(request.CurrencyCode) || request.CurrencyCode.Trim().Length != 3)
                return (false, "Currency code harus terdiri dari tiga karakter.");

            if (request.EnrollmentStartDate.HasValue &&
                request.EnrollmentEndDate.HasValue &&
                request.EnrollmentEndDate.Value.Date < request.EnrollmentStartDate.Value.Date)
                return (false, "EnrollmentEndDate tidak boleh lebih kecil dari EnrollmentStartDate.");

            if (request.EffectiveStartDate.HasValue &&
                request.EffectiveEndDate.HasValue &&
                request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Value.Date)
                return (false, "EffectiveEndDate tidak boleh lebih kecil dari EffectiveStartDate.");

            var benefitType = await _dbContext.Set<MstBenefitType>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == request.BenefitTypeId &&
                    x.IsActive &&
                    !x.IsDelete);

            if (benefitType == null)
                return (false, "Benefit type tidak ditemukan atau tidak aktif.");

            if (!benefitType.AllowsDependents && request.MaximumDependents > 0)
                return (false, "Benefit type ini tidak mengizinkan dependent.");

            var normalizedName = request.BenefitPlanName.Trim().ToLower();

            var duplicateQuery = _dbContext.Set<MstBenefitPlan>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.BenefitPlanName.ToLower() == normalizedName &&
                    x.BenefitTypeId == request.BenefitTypeId);

            if (excludeId.HasValue)
                duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateQuery.AnyAsync())
                return (false, "Benefit plan dengan nama dan benefit type tersebut sudah digunakan.");

            return (true, null);
        }

        private async Task<string> GenerateCodeAsync()
        {
            var codes = await _dbContext.Set<MstBenefitPlan>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.BenefitPlanCode.StartsWith(CodePrefix))
                .Select(x => x.BenefitPlanCode)
                .ToListAsync();

            return GenerateNextCode(codes, CodePrefix, CodeNumberLength);
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

        private static string? NormalizeNullableString(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }


        private static string NormalizeCoverageType(string value)
        {
            return AllowedCoverageTypes.First(x =>
                x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static (int PageNumber, int PageSize) NormalizePaging(
            int pageNumber,
            int pageSize)
        {
            return (
                pageNumber < 1 ? 1 : pageNumber,
                pageSize < 1 ? 25 : Math.Min(pageSize, 100));
        }

        private static string GenerateNextCode(
            IEnumerable<string> codes,
            string prefix,
            int length)
        {
            var used = codes
                .Select(x => x.Replace(prefix, string.Empty))
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .Where(x => x > 0)
                .ToHashSet();

            var next = 1;

            while (used.Contains(next))
                next++;

            return prefix + next.ToString().PadLeft(length, '0');
        }

        private static List<BenefitPlanCustomPeriodOptionResponse> BuildPeriodOptions()
        {
            return new List<BenefitPlanCustomPeriodOptionResponse>
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "lastmonth", Label = "Bulan lalu" }
            };
        }
    }
}
