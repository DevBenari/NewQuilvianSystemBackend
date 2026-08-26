using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    [Route("api/v1/corporate/human-resource/master-data/benefit-eligibility-rules")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Benefit Eligibility Rule",
        AreaName = "Corporate",
        ControllerName = "BenefitEligibilityRule",
        Description = "Corporate human resource master data benefit eligibility rule",
        SortOrder = 53)]
    [Tags("Corporate / Human Resource / Master Data / Benefit Eligibility Rule")]
    public class BenefitEligibilityRuleController : ControllerBase
    {
        private const string CodePrefix = "BER-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;

        public BenefitEligibilityRuleController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Benefit Eligibility Rule", Description = "Melihat metadata filter benefit eligibility rule", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BenefitEligibilityRule", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new BenefitEligibilityRuleFilterMetadataResponse
            {
                DefaultFilter = new BenefitEligibilityRuleDefaultFilterResponse(),
                CustomPeriods = BuildPeriodOptions(),
                SortOptions = new List<BenefitEligibilityRuleSortOptionResponse>
                {
                    new() { Value = "priority", Label = "Prioritas" },
                    new() { Value = "name", Label = "Nama" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            return Ok(ApiResponse<BenefitEligibilityRuleFilterMetadataResponse>.Ok(
                result,
                "Metadata filter berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Benefit Eligibility Rule", Description = "Melihat ringkasan benefit eligibility rule", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BenefitEligibilityRule", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();

            var result = new BenefitEligibilityRuleSummaryResponse
            {
                TotalData = await query.CountAsync(),
                ActiveData = await query.CountAsync(x => x.IsActive),
                InactiveData = await query.CountAsync(x => !x.IsActive),
                ProbationAllowedData = await query.CountAsync(x => x.AllowProbationEmployee),
                ContractAllowedData = await query.CountAsync(x => x.AllowContractEmployee),
                ManagerApprovalRequiredData = await query.CountAsync(x => x.RequireManagerApproval),
                HrVerificationRequiredData = await query.CountAsync(x => x.RequireHrVerification)
            };

            return Ok(ApiResponse<BenefitEligibilityRuleSummaryResponse>.Ok(
                result,
                "Ringkasan berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Benefit Eligibility Rule", Description = "Melihat data benefit eligibility rule", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BenefitEligibilityRule", "Read")]
        public async Task<IActionResult> GetData(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? benefitPlanId,
            [FromQuery] bool? allowProbationEmployee,
            [FromQuery] bool? allowContractEmployee,
            [FromQuery] bool? requireFullTimeEmployment,
            [FromQuery] bool? requireManagerApproval,
            [FromQuery] bool? requireHrVerification,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 25 : Math.Min(pageSize, 100);

            var query = BuildBaseQuery();

            if (benefitPlanId.HasValue)
                query = query.Where(x => x.BenefitPlanId == benefitPlanId.Value);

            if (allowProbationEmployee.HasValue)
                query = query.Where(x => x.AllowProbationEmployee == allowProbationEmployee.Value);

            if (allowContractEmployee.HasValue)
                query = query.Where(x => x.AllowContractEmployee == allowContractEmployee.Value);

            if (requireFullTimeEmployment.HasValue)
                query = query.Where(x => x.RequireFullTimeEmployment == requireFullTimeEmployment.Value);

            if (requireManagerApproval.HasValue)
                query = query.Where(x => x.RequireManagerApproval == requireManagerApproval.Value);

            if (requireHrVerification.HasValue)
                query = query.Where(x => x.RequireHrVerification == requireHrVerification.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.EligibilityRuleName.ToLower().Contains(keyword) ||
                    x.EligibilityRuleCode.ToLower().Contains(keyword));
            }

            query = WorkflowMasterDataSupport.ApplyDateFilter(query, startDate, endDate, customPeriod);
            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.Priority)
                .ThenBy(x => x.EligibilityRuleName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => MapResponse(x))
                .ToListAsync();

            var result = new PagedResult<BenefitEligibilityRuleResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<PagedResult<BenefitEligibilityRuleResponse>>.Ok(
                result,
                "Data berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Benefit Eligibility Rule", Description = "Melihat pilihan benefit eligibility rule", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BenefitEligibilityRule", "Read")]
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
                    x.EligibilityRuleName.ToLower().Contains(keyword) ||
                    x.EligibilityRuleCode.ToLower().Contains(keyword));
            }

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.Priority)
                .ThenBy(x => x.EligibilityRuleName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new BenefitEligibilityRuleOptionResponse
                {
                    Id = x.Id,
                    BenefitPlanId = x.BenefitPlanId,
                    EligibilityRuleCode = x.EligibilityRuleCode,
                    EligibilityRuleName = x.EligibilityRuleName,
                    Priority = x.Priority
                })
                .ToListAsync();

            var result = new BenefitEligibilityRuleOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<BenefitEligibilityRuleOptionPagedResponse>.Ok(
                result,
                "Pilihan data berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Benefit Eligibility Rule", Description = "Melihat detail benefit eligibility rule", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BenefitEligibilityRule", "Read")]
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

            var result = new BenefitEligibilityRuleDetailResponse
            {
                Id = entity.Id,
                BenefitPlanId = entity.BenefitPlanId,
                LegalEntityId = entity.LegalEntityId,
                HospitalSiteId = entity.HospitalSiteId,
                OrganizationUnitId = entity.OrganizationUnitId,
                EmployeeCategoryId = entity.EmployeeCategoryId,
                EmploymentTypeId = entity.EmploymentTypeId,
                EmployeeGradeId = entity.EmployeeGradeId,
                SalaryGradeId = entity.SalaryGradeId,
                EligibilityRuleCode = entity.EligibilityRuleCode,
                EligibilityRuleName = entity.EligibilityRuleName,
                MinimumServiceMonths = entity.MinimumServiceMonths,
                MinimumAge = entity.MinimumAge,
                MaximumAge = entity.MaximumAge,
                AllowProbationEmployee = entity.AllowProbationEmployee,
                AllowContractEmployee = entity.AllowContractEmployee,
                RequireFullTimeEmployment = entity.RequireFullTimeEmployment,
                MinimumWeeklyHours = entity.MinimumWeeklyHours,
                CoverageStartOffsetDays = entity.CoverageStartOffsetDays,
                CoverageEndAfterTerminationDays = entity.CoverageEndAfterTerminationDays,
                RequireManagerApproval = entity.RequireManagerApproval,
                RequireHrVerification = entity.RequireHrVerification,
                Priority = entity.Priority,
                EffectiveStartDate = entity.EffectiveStartDate,
                EffectiveEndDate = entity.EffectiveEndDate,
                Description = entity.Description,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy
            };

            return Ok(ApiResponse<BenefitEligibilityRuleDetailResponse>.Ok(
                result,
                "Detail berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Benefit Eligibility Rule", Description = "Membuat benefit eligibility rule", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("BenefitEligibilityRule", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateBenefitEligibilityRuleRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data tidak valid."));
            }

            var entity = new MstBenefitEligibilityRule
            {
                Id = Guid.NewGuid(),
                BenefitPlanId = request.BenefitPlanId,
                LegalEntityId = NormalizeGuid(request.LegalEntityId),
                HospitalSiteId = NormalizeGuid(request.HospitalSiteId),
                OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId),
                EmployeeCategoryId = NormalizeGuid(request.EmployeeCategoryId),
                EmploymentTypeId = NormalizeGuid(request.EmploymentTypeId),
                EmployeeGradeId = NormalizeGuid(request.EmployeeGradeId),
                SalaryGradeId = NormalizeGuid(request.SalaryGradeId),
                EligibilityRuleCode = await GenerateCodeAsync(),
                EligibilityRuleName = request.EligibilityRuleName.Trim(),
                MinimumServiceMonths = request.MinimumServiceMonths,
                MinimumAge = request.MinimumAge,
                MaximumAge = request.MaximumAge,
                AllowProbationEmployee = request.AllowProbationEmployee,
                AllowContractEmployee = request.AllowContractEmployee,
                RequireFullTimeEmployment = request.RequireFullTimeEmployment,
                MinimumWeeklyHours = request.MinimumWeeklyHours,
                CoverageStartOffsetDays = request.CoverageStartOffsetDays,
                CoverageEndAfterTerminationDays = request.CoverageEndAfterTerminationDays,
                RequireManagerApproval = request.RequireManagerApproval,
                RequireHrVerification = request.RequireHrVerification,
                Priority = request.Priority,
                EffectiveStartDate = request.EffectiveStartDate?.Date,
                EffectiveEndDate = request.EffectiveEndDate?.Date,
                Description = NormalizeText(request.Description),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstBenefitEligibilityRule>().Add(entity);
            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                new { entity.Id },
                "Data berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Benefit Eligibility Rule", Description = "Mengubah benefit eligibility rule", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("BenefitEligibilityRule", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateBenefitEligibilityRuleRequest request)
        {
            var entity = await _dbContext.Set<MstBenefitEligibilityRule>()
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

            entity.BenefitPlanId = request.BenefitPlanId;
            entity.LegalEntityId = NormalizeGuid(request.LegalEntityId);
            entity.HospitalSiteId = NormalizeGuid(request.HospitalSiteId);
            entity.OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId);
            entity.EmployeeCategoryId = NormalizeGuid(request.EmployeeCategoryId);
            entity.EmploymentTypeId = NormalizeGuid(request.EmploymentTypeId);
            entity.EmployeeGradeId = NormalizeGuid(request.EmployeeGradeId);
            entity.SalaryGradeId = NormalizeGuid(request.SalaryGradeId);
            entity.EligibilityRuleName = request.EligibilityRuleName.Trim();
            entity.MinimumServiceMonths = request.MinimumServiceMonths;
            entity.MinimumAge = request.MinimumAge;
            entity.MaximumAge = request.MaximumAge;
            entity.AllowProbationEmployee = request.AllowProbationEmployee;
            entity.AllowContractEmployee = request.AllowContractEmployee;
            entity.RequireFullTimeEmployment = request.RequireFullTimeEmployment;
            entity.MinimumWeeklyHours = request.MinimumWeeklyHours;
            entity.CoverageStartOffsetDays = request.CoverageStartOffsetDays;
            entity.CoverageEndAfterTerminationDays = request.CoverageEndAfterTerminationDays;
            entity.RequireManagerApproval = request.RequireManagerApproval;
            entity.RequireHrVerification = request.RequireHrVerification;
            entity.Priority = request.Priority;
            entity.EffectiveStartDate = request.EffectiveStartDate?.Date;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.Description = NormalizeText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Data berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Benefit Eligibility Rule Status", Description = "Mengubah status benefit eligibility rule", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("BenefitEligibilityRule", "Update")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] UpdateBenefitEligibilityRuleStatusRequest request)
        {
            var entity = await _dbContext.Set<MstBenefitEligibilityRule>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Data tidak ditemukan."));
            }

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Benefit Eligibility Rule", Description = "Menghapus benefit eligibility rule", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("BenefitEligibilityRule", "Delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var entity = await _dbContext.Set<MstBenefitEligibilityRule>()
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

        private IQueryable<MstBenefitEligibilityRule> BuildBaseQuery()
        {
            return _dbContext.Set<MstBenefitEligibilityRule>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static BenefitEligibilityRuleResponse MapResponse(
            MstBenefitEligibilityRule x)
        {
            return new BenefitEligibilityRuleResponse
            {
                Id = x.Id,
                BenefitPlanId = x.BenefitPlanId,
                LegalEntityId = x.LegalEntityId,
                HospitalSiteId = x.HospitalSiteId,
                OrganizationUnitId = x.OrganizationUnitId,
                EmployeeCategoryId = x.EmployeeCategoryId,
                EmploymentTypeId = x.EmploymentTypeId,
                EmployeeGradeId = x.EmployeeGradeId,
                SalaryGradeId = x.SalaryGradeId,
                EligibilityRuleCode = x.EligibilityRuleCode,
                EligibilityRuleName = x.EligibilityRuleName,
                MinimumServiceMonths = x.MinimumServiceMonths,
                MinimumAge = x.MinimumAge,
                MaximumAge = x.MaximumAge,
                AllowProbationEmployee = x.AllowProbationEmployee,
                AllowContractEmployee = x.AllowContractEmployee,
                RequireFullTimeEmployment = x.RequireFullTimeEmployment,
                MinimumWeeklyHours = x.MinimumWeeklyHours,
                CoverageStartOffsetDays = x.CoverageStartOffsetDays,
                CoverageEndAfterTerminationDays = x.CoverageEndAfterTerminationDays,
                RequireManagerApproval = x.RequireManagerApproval,
                RequireHrVerification = x.RequireHrVerification,
                Priority = x.Priority,
                EffectiveStartDate = x.EffectiveStartDate,
                EffectiveEndDate = x.EffectiveEndDate,
                Description = x.Description,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime,
                CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateBenefitEligibilityRuleRequest request)
        {
            if (request.BenefitPlanId == Guid.Empty)
                return (false, "Benefit plan wajib dipilih.");

            if (string.IsNullOrWhiteSpace(request.EligibilityRuleName))
                return (false, "Nama rule wajib diisi.");

            if (request.MinimumAge.HasValue &&
                request.MaximumAge.HasValue &&
                request.MaximumAge.Value < request.MinimumAge.Value)
            {
                return (false, "MaximumAge tidak boleh lebih kecil dari MinimumAge.");
            }

            if (request.EffectiveStartDate.HasValue &&
                request.EffectiveEndDate.HasValue &&
                request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Value.Date)
            {
                return (false, "EffectiveEndDate tidak boleh sebelum EffectiveStartDate.");
            }

            if (!await _dbContext.Set<MstBenefitPlan>().AnyAsync(x =>
                    x.Id == request.BenefitPlanId &&
                    x.IsActive &&
                    !x.IsDelete))
            {
                return (false, "Benefit plan tidak valid.");
            }

            var normalizedName = request.EligibilityRuleName.Trim().ToLower();

            var duplicateQuery = _dbContext.Set<MstBenefitEligibilityRule>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.BenefitPlanId == request.BenefitPlanId &&
                    x.EligibilityRuleName.ToLower() == normalizedName);

            if (excludeId.HasValue)
                duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateQuery.AnyAsync())
                return (false, "Nama rule sudah digunakan pada benefit plan tersebut.");

            return (true, null);
        }

        private async Task<string> GenerateCodeAsync()
        {
            var codes = await _dbContext.Set<MstBenefitEligibilityRule>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.EligibilityRuleCode.StartsWith(CodePrefix))
                .Select(x => x.EligibilityRuleCode)
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

        private static List<BenefitEligibilityRuleCustomPeriodOptionResponse> BuildPeriodOptions()
        {
            return new List<BenefitEligibilityRuleCustomPeriodOptionResponse>
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "lastmonth", Label = "Bulan lalu" }
            };
        }
    }
}
