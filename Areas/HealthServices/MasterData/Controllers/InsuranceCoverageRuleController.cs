using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseInsuranceCoverageRulePagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.InsuranceCoverageRuleResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/insurance-coverage-rules")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Insurance Coverage Rule",
        AreaName = "HealthServices",
        ControllerName = "InsuranceCoverageRule",
        Description = "Health service master data insurance coverage rule",
        SortOrder = 12
    )]
    [Tags("Health Services / Master Data / Insurance Coverage Rule")]
    public class InsuranceCoverageRuleController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData";
        private const string RuleCodePrefix = "ICR-RSMMC-";
        private const int RuleCodeDigitLength = 5;

        private static readonly HashSet<string> AllowedItemTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "Tariff",
            "Drug",
            "DrugCategory",
            "Procedure",
            "ServiceCategory"
        };

        private static readonly HashSet<string> AllowedCoverageStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "Covered",
            "NotCovered",
            "PartialCovered",
            "NeedApproval"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public InsuranceCoverageRuleController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<InsuranceCoverageRuleFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Insurance Coverage Rule", Description = "Melihat data insurance coverage rule", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InsuranceCoverageRule", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new InsuranceCoverageRuleFilterMetadataResponse
            {
                DefaultFilter = new InsuranceCoverageRuleDefaultFilterResponse(),
                CustomPeriods = new List<InsuranceCoverageRuleCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7Days", Label = "7 hari terakhir" },
                    new() { Value = "thisMonth", Label = "Bulan ini" },
                    new() { Value = "lastMonth", Label = "Bulan lalu" }
                },
                SortOptions = new List<InsuranceCoverageRuleSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "ruleCode", Label = "Kode rule" },
                    new() { Value = "ruleName", Label = "Nama rule" },
                    new() { Value = "insuranceProviderName", Label = "Provider asuransi" },
                    new() { Value = "itemType", Label = "Tipe item" },
                    new() { Value = "coverageStatus", Label = "Status coverage" },
                    new() { Value = "coveragePercent", Label = "Persentase coverage" },
                    new() { Value = "effectiveStartDate", Label = "Tanggal mulai berlaku" },
                    new() { Value = "effectiveEndDate", Label = "Tanggal akhir berlaku" },
                    new() { Value = "isCovered", Label = "Ditanggung" },
                    new() { Value = "isExcluded", Label = "Excluded" },
                    new() { Value = "isNeedApproval", Label = "Butuh approval" },
                    new() { Value = "isNeedGuaranteeLetter", Label = "Butuh guarantee letter" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                ItemTypeOptions = AllowedItemTypes
                    .OrderBy(x => x)
                    .Select(x => new InsuranceCoverageRuleItemTypeOptionResponse
                    {
                        Value = x,
                        Label = SplitPascalCase(x)
                    })
                    .ToList(),
                CoverageStatusOptions = AllowedCoverageStatuses
                    .OrderBy(x => x)
                    .Select(x => new InsuranceCoverageRuleCoverageStatusOptionResponse
                    {
                        Value = x,
                        Label = SplitPascalCase(x)
                    })
                    .ToList()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "InsuranceCoverageRule.GetFilterMetadata",
                "Mengambil metadata filter insurance coverage rule.",
                result
            );

            return Ok(ApiResponse<InsuranceCoverageRuleFilterMetadataResponse>.Ok(
                result,
                "Metadata filter insurance coverage rule berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<InsuranceCoverageRuleSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Insurance Coverage Rule", Description = "Melihat data insurance coverage rule", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InsuranceCoverageRule", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var now = DateTime.UtcNow;

            var query = _dbContext.Set<MstInsuranceCoverageRule>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new InsuranceCoverageRuleSummaryResponse
            {
                TotalRule = await query.CountAsync(),
                ActiveRule = await query.CountAsync(x => x.IsActive),
                InactiveRule = await query.CountAsync(x => !x.IsActive),
                CoveredRule = await query.CountAsync(x => x.CoverageStatus == "Covered"),
                NotCoveredRule = await query.CountAsync(x => x.CoverageStatus == "NotCovered"),
                PartialCoveredRule = await query.CountAsync(x => x.CoverageStatus == "PartialCovered"),
                NeedApprovalStatusRule = await query.CountAsync(x => x.CoverageStatus == "NeedApproval"),
                ExcludedRule = await query.CountAsync(x => x.IsExcluded),
                NeedApprovalRule = await query.CountAsync(x => x.IsNeedApproval),
                NeedGuaranteeLetterRule = await query.CountAsync(x => x.IsNeedGuaranteeLetter),
                AllowExcessPaymentByPatientRule = await query.CountAsync(x => x.IsAllowExcessPaymentByPatient),
                EffectiveRule = await query.CountAsync(x =>
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value <= now) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= now)),
                ExpiredRule = await query.CountAsync(x =>
                    x.EffectiveEndDate.HasValue && x.EffectiveEndDate.Value < now),
                TariffRule = await query.CountAsync(x => x.ItemType == "Tariff"),
                DrugRule = await query.CountAsync(x => x.ItemType == "Drug"),
                DrugCategoryRule = await query.CountAsync(x => x.ItemType == "DrugCategory"),
                ProcedureRule = await query.CountAsync(x => x.ItemType == "Procedure"),
                ServiceCategoryRule = await query.CountAsync(x => x.ItemType == "ServiceCategory")
            };

            return Ok(ApiResponse<InsuranceCoverageRuleSummaryResponse>.Ok(
                result,
                "Ringkasan insurance coverage rule berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseInsuranceCoverageRulePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Insurance Coverage Rule", Description = "Melihat data insurance coverage rule", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InsuranceCoverageRule", "Read")]
        public async Task<IActionResult> GetInsuranceCoverageRules(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? insuranceProviderId,
            [FromQuery] string? itemType,
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

            var query = BuildBaseQuery();

            query = ApplyDateFilter(query, startDate, endDate, customPeriod);
            query = ApplySimpleFilter(query, insuranceProviderId, itemType, isActive, search);

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities
                .Select(ToResponse)
                .ToList();

            var result = new ResponseInsuranceCoverageRulePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseInsuranceCoverageRulePagedResult>.Ok(
                result,
                "Data insurance coverage rule berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<InsuranceCoverageRuleOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Insurance Coverage Rule", Description = "Melihat data pilihan insurance coverage rule", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InsuranceCoverageRule", "Read")]
        public async Task<IActionResult> GetInsuranceCoverageRuleOptions(
    [FromQuery] Guid? insuranceProviderId,
    [FromQuery] string? itemType,
    [FromQuery] bool onlyActive = true,
    [FromQuery] string? search = null,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplySimpleFilter(
                query,
                insuranceProviderId,
                itemType,
                onlyActive ? true : null,
                search
            );

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.RuleName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new InsuranceCoverageRuleOptionResponse
                {
                    Id = x.Id,

                    InsuranceProviderId = x.InsuranceProviderId,
                    InsuranceProviderName = x.InsuranceProvider != null
                        ? x.InsuranceProvider.InsuranceProviderName
                        : string.Empty,

                    RuleCode = x.RuleCode,
                    RuleName = x.RuleName,
                    ItemType = x.ItemType,

                    TariffId = x.TariffId,
                    TariffName = x.Tariff != null
                        ? x.Tariff.TariffName
                        : null,

                    DrugId = x.DrugId,
                    DrugName = x.Drug != null
                        ? x.Drug.DrugName
                        : null,

                    DrugCategoryId = x.DrugCategoryId,
                    DrugCategoryName = x.DrugCategory != null
                        ? x.DrugCategory.DrugCategoryName
                        : null,

                    ProcedureId = x.ProcedureId,
                    ProcedureName = x.Procedure != null
                        ? x.Procedure.ProcedureName
                        : null,

                    TariffCategoryId = x.TariffCategoryId,
                    TariffCategoryName = x.TariffCategory != null
                        ? x.TariffCategory.TariffCategoryName
                        : null,

                    BenefitPlanCode = x.BenefitPlanCode,
                    BenefitPlanName = x.BenefitPlanName,
                    PatientClassName = x.PatientClassName,

                    CoverageStatus = x.CoverageStatus,
                    CoveragePercent = x.CoveragePercent,
                    MaxCoverageAmount = x.MaxCoverageAmount,
                    CoPaymentPercent = x.CoPaymentPercent,
                    CoPaymentAmount = x.CoPaymentAmount,

                    IsCovered = x.IsCovered,
                    IsExcluded = x.IsExcluded,
                    IsNeedApproval = x.IsNeedApproval,
                    IsNeedGuaranteeLetter = x.IsNeedGuaranteeLetter,
                    IsAllowExcessPaymentByPatient = x.IsAllowExcessPaymentByPatient
                })
                .ToListAsync();

            var result = new InsuranceCoverageRuleOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<InsuranceCoverageRuleOptionPagedResponse>.Ok(
                result,
                "Data pilihan insurance coverage rule berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<InsuranceCoverageRuleDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Insurance Coverage Rule", Description = "Melihat detail insurance coverage rule", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InsuranceCoverageRule", "Read")]
        public async Task<IActionResult> GetInsuranceCoverageRuleById(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new InsuranceCoverageRuleDetailResponse
                {
                    Id = x.Id,
                    InsuranceProviderId = x.InsuranceProviderId,
                    InsuranceProviderCode = x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceProviderCode : string.Empty,
                    InsuranceProviderName = x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceProviderName : string.Empty,
                    InsuranceGroupName = x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceGroupName : null,
                    RuleCode = x.RuleCode,
                    RuleName = x.RuleName,
                    ItemType = x.ItemType,
                    TariffId = x.TariffId,
                    TariffCode = x.Tariff != null ? x.Tariff.TariffCode : null,
                    TariffName = x.Tariff != null ? x.Tariff.TariffName : null,
                    DrugId = x.DrugId,
                    DrugCode = x.Drug != null ? x.Drug.DrugCode : null,
                    DrugName = x.Drug != null ? x.Drug.DrugName : null,
                    DrugCategoryId = x.DrugCategoryId,
                    DrugCategoryCode = x.DrugCategory != null ? x.DrugCategory.DrugCategoryCode : null,
                    DrugCategoryName = x.DrugCategory != null ? x.DrugCategory.DrugCategoryName : null,
                    ProcedureId = x.ProcedureId,
                    ProcedureCode = x.Procedure != null ? x.Procedure.ProcedureCode : null,
                    ProcedureName = x.Procedure != null ? x.Procedure.ProcedureName : null,
                    TariffCategoryId = x.TariffCategoryId,
                    TariffCategoryCode = x.TariffCategory != null ? x.TariffCategory.TariffCategoryCode : null,
                    TariffCategoryName = x.TariffCategory != null ? x.TariffCategory.TariffCategoryName : null,
                    BenefitPlanCode = x.BenefitPlanCode,
                    BenefitPlanName = x.BenefitPlanName,
                    PatientClassName = x.PatientClassName,
                    CoverageStatus = x.CoverageStatus,
                    CoveragePercent = x.CoveragePercent,
                    MaxCoverageAmount = x.MaxCoverageAmount,
                    CoPaymentPercent = x.CoPaymentPercent,
                    CoPaymentAmount = x.CoPaymentAmount,
                    IsCovered = x.IsCovered,
                    IsExcluded = x.IsExcluded,
                    IsNeedApproval = x.IsNeedApproval,
                    IsNeedGuaranteeLetter = x.IsNeedGuaranteeLetter,
                    IsAllowExcessPaymentByPatient = x.IsAllowExcessPaymentByPatient,
                    MaxQuantityPerVisit = x.MaxQuantityPerVisit,
                    MaxQuantityPerMonth = x.MaxQuantityPerMonth,
                    MaxAmountPerVisit = x.MaxAmountPerVisit,
                    MaxAmountPerMonth = x.MaxAmountPerMonth,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    ApprovalInstruction = x.ApprovalInstruction,
                    BillingInstruction = x.BillingInstruction,
                    Description = x.Description,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Insurance coverage rule tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<InsuranceCoverageRuleDetailResponse>.Ok(
                data,
                "Detail insurance coverage rule berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<InsuranceCoverageRuleCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Insurance Coverage Rule", Description = "Membuat data insurance coverage rule", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("InsuranceCoverageRule", "Create")]
        public async Task<IActionResult> CreateInsuranceCoverageRule([FromBody] CreateInsuranceCoverageRuleRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data insurance coverage rule tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var itemType = NormalizeItemType(request.ItemType);

            var entity = new MstInsuranceCoverageRule
            {
                Id = Guid.NewGuid(),
                InsuranceProviderId = request.InsuranceProviderId,
                RuleCode = await GenerateRuleCodeAsync(),
                RuleName = request.RuleName.Trim(),
                ItemType = itemType,
                TariffId = itemType == "Tariff" ? NormalizeNullableGuid(request.TariffId) : null,
                DrugId = itemType == "Drug" ? NormalizeNullableGuid(request.DrugId) : null,
                DrugCategoryId = itemType == "DrugCategory" ? NormalizeNullableGuid(request.DrugCategoryId) : null,
                ProcedureId = itemType == "Procedure" ? NormalizeNullableGuid(request.ProcedureId) : null,
                TariffCategoryId = itemType == "ServiceCategory" ? NormalizeNullableGuid(request.TariffCategoryId) : null,
                BenefitPlanCode = NormalizeNullableString(request.BenefitPlanCode)?.ToUpperInvariant(),
                BenefitPlanName = NormalizeNullableString(request.BenefitPlanName),
                PatientClassName = NormalizeNullableString(request.PatientClassName),
                CoverageStatus = NormalizeCoverageStatus(request.CoverageStatus),
                CoveragePercent = request.CoveragePercent,
                MaxCoverageAmount = request.MaxCoverageAmount,
                CoPaymentPercent = request.CoPaymentPercent,
                CoPaymentAmount = request.CoPaymentAmount,
                IsCovered = request.IsCovered,
                IsExcluded = request.IsExcluded,
                IsNeedApproval = request.IsNeedApproval,
                IsNeedGuaranteeLetter = request.IsNeedGuaranteeLetter,
                IsAllowExcessPaymentByPatient = request.IsAllowExcessPaymentByPatient,
                MaxQuantityPerVisit = request.MaxQuantityPerVisit,
                MaxQuantityPerMonth = request.MaxQuantityPerMonth,
                MaxAmountPerVisit = request.MaxAmountPerVisit,
                MaxAmountPerMonth = request.MaxAmountPerMonth,
                EffectiveStartDate = request.EffectiveStartDate,
                EffectiveEndDate = request.EffectiveEndDate,
                ApprovalInstruction = NormalizeNullableString(request.ApprovalInstruction),
                BillingInstruction = NormalizeNullableString(request.BillingInstruction),
                Description = NormalizeNullableString(request.Description),
                SortOrder = request.SortOrder,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstInsuranceCoverageRule>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var result = new InsuranceCoverageRuleCreateResponse
            {
                Id = entity.Id,
                RuleCode = entity.RuleCode,
                RuleName = entity.RuleName,
                InsuranceProviderId = entity.InsuranceProviderId,
                ItemType = entity.ItemType,
                IsActive = entity.IsActive
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "InsuranceCoverageRule.CreateInsuranceCoverageRule",
                "Membuat data insurance coverage rule.",
                result
            );

            return Ok(ApiResponse<InsuranceCoverageRuleCreateResponse>.Ok(
                result,
                "Insurance coverage rule berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<InsuranceCoverageRuleUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Insurance Coverage Rule", Description = "Mengubah data insurance coverage rule", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("InsuranceCoverageRule", "Update")]
        public async Task<IActionResult> UpdateInsuranceCoverageRule(Guid id, [FromBody] UpdateInsuranceCoverageRuleRequest request)
        {
            var entity = await _dbContext.Set<MstInsuranceCoverageRule>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Insurance coverage rule tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(id, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data insurance coverage rule tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var itemType = NormalizeItemType(request.ItemType);

            entity.InsuranceProviderId = request.InsuranceProviderId;
            entity.RuleName = request.RuleName.Trim();
            entity.ItemType = itemType;
            entity.TariffId = itemType == "Tariff" ? NormalizeNullableGuid(request.TariffId) : null;
            entity.DrugId = itemType == "Drug" ? NormalizeNullableGuid(request.DrugId) : null;
            entity.DrugCategoryId = itemType == "DrugCategory" ? NormalizeNullableGuid(request.DrugCategoryId) : null;
            entity.ProcedureId = itemType == "Procedure" ? NormalizeNullableGuid(request.ProcedureId) : null;
            entity.TariffCategoryId = itemType == "ServiceCategory" ? NormalizeNullableGuid(request.TariffCategoryId) : null;
            entity.BenefitPlanCode = NormalizeNullableString(request.BenefitPlanCode)?.ToUpperInvariant();
            entity.BenefitPlanName = NormalizeNullableString(request.BenefitPlanName);
            entity.PatientClassName = NormalizeNullableString(request.PatientClassName);
            entity.CoverageStatus = NormalizeCoverageStatus(request.CoverageStatus);
            entity.CoveragePercent = request.CoveragePercent;
            entity.MaxCoverageAmount = request.MaxCoverageAmount;
            entity.CoPaymentPercent = request.CoPaymentPercent;
            entity.CoPaymentAmount = request.CoPaymentAmount;
            entity.IsCovered = request.IsCovered;
            entity.IsExcluded = request.IsExcluded;
            entity.IsNeedApproval = request.IsNeedApproval;
            entity.IsNeedGuaranteeLetter = request.IsNeedGuaranteeLetter;
            entity.IsAllowExcessPaymentByPatient = request.IsAllowExcessPaymentByPatient;
            entity.MaxQuantityPerVisit = request.MaxQuantityPerVisit;
            entity.MaxQuantityPerMonth = request.MaxQuantityPerMonth;
            entity.MaxAmountPerVisit = request.MaxAmountPerVisit;
            entity.MaxAmountPerMonth = request.MaxAmountPerMonth;
            entity.EffectiveStartDate = request.EffectiveStartDate;
            entity.EffectiveEndDate = request.EffectiveEndDate;
            entity.ApprovalInstruction = NormalizeNullableString(request.ApprovalInstruction);
            entity.BillingInstruction = NormalizeNullableString(request.BillingInstruction);
            entity.Description = NormalizeNullableString(request.Description);
            entity.SortOrder = request.SortOrder;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;


            await _dbContext.SaveChangesAsync();

            var result = new InsuranceCoverageRuleUpdateResponse
            {
                Id = entity.Id,
                RuleCode = entity.RuleCode,
                RuleName = entity.RuleName,
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "InsuranceCoverageRule.UpdateInsuranceCoverageRule",
                "Mengubah data insurance coverage rule.",
                result
            );

            return Ok(ApiResponse<InsuranceCoverageRuleUpdateResponse>.Ok(
                result,
                "Insurance coverage rule berhasil diperbarui."
            ));
        }


        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<InsuranceCoverageRuleStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Insurance Coverage Rule", Description = "Mengubah status insurance coverage rule", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("InsuranceCoverageRule", "Update")]
        public async Task<IActionResult> UpdateInsuranceCoverageRuleStatus(Guid id, [FromBody] UpdateInsuranceCoverageRuleStatusRequest request)
        {
            return await UpdateStatusAsync(
                id,
                request.IsActive,
                request.IsActive
                    ? "Insurance coverage rule berhasil diaktifkan."
                    : "Insurance coverage rule berhasil dinonaktifkan."
            );
        }

        [HttpPatch("{id:guid}/activate")]
        [ProducesResponseType(typeof(ApiResponse<InsuranceCoverageRuleStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Insurance Coverage Rule", Description = "Mengaktifkan data insurance coverage rule", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("InsuranceCoverageRule", "Update")]
        public async Task<IActionResult> ActivateInsuranceCoverageRule(Guid id)
        {
            return await UpdateStatusAsync(id, true, "Insurance coverage rule berhasil diaktifkan.");
        }

        [HttpPatch("{id:guid}/deactivate")]
        [ProducesResponseType(typeof(ApiResponse<InsuranceCoverageRuleStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Insurance Coverage Rule", Description = "Menonaktifkan data insurance coverage rule", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("InsuranceCoverageRule", "Update")]
        public async Task<IActionResult> DeactivateInsuranceCoverageRule(Guid id)
        {
            return await UpdateStatusAsync(id, false, "Insurance coverage rule berhasil dinonaktifkan.");
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<InsuranceCoverageRuleDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Insurance Coverage Rule", Description = "Menghapus data insurance coverage rule", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("InsuranceCoverageRule", "Delete")]
        public async Task<IActionResult> DeleteInsuranceCoverageRule(Guid id, [FromBody] DeleteInsuranceCoverageRuleRequest? deleteRequest = null)
        {
            var entity = await _dbContext.Set<MstInsuranceCoverageRule>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Insurance coverage rule tidak ditemukan."
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

            if (!string.IsNullOrWhiteSpace(deleteRequest?.DeleteReason))
                entity.Description = NormalizeNullableString(deleteRequest.DeleteReason);

            await _dbContext.SaveChangesAsync();

            var result = new InsuranceCoverageRuleDeleteResponse
            {
                Id = entity.Id,
                RuleCode = entity.RuleCode,
                RuleName = entity.RuleName,
                DeleteDateTime = entity.DeleteDateTime
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "InsuranceCoverageRule.DeleteInsuranceCoverageRule",
                "Menghapus data insurance coverage rule.",
                result
            );

            return Ok(ApiResponse<InsuranceCoverageRuleDeleteResponse>.Ok(
                result,
                "Insurance coverage rule berhasil dihapus."
            ));
        }

        private async Task<IActionResult> UpdateStatusAsync(Guid id, bool isActive, string successMessage)
        {
            var entity = await _dbContext.Set<MstInsuranceCoverageRule>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Insurance coverage rule tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = isActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var result = new InsuranceCoverageRuleStatusResponse
            {
                Id = entity.Id,
                RuleCode = entity.RuleCode,
                RuleName = entity.RuleName,
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "InsuranceCoverageRule.UpdateStatus",
                successMessage,
                result
            );

            return Ok(ApiResponse<InsuranceCoverageRuleStatusResponse>.Ok(
                result,
                successMessage
            ));
        }

        private IQueryable<MstInsuranceCoverageRule> BuildBaseQuery()
        {
            return _dbContext.Set<MstInsuranceCoverageRule>()
                .AsNoTracking()
                .Include(x => x.InsuranceProvider)
                .Include(x => x.Tariff)
                .Include(x => x.Drug)
                .Include(x => x.DrugCategory)
                .Include(x => x.Procedure)
                .Include(x => x.TariffCategory)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstInsuranceCoverageRule> ApplySimpleFilter(
            IQueryable<MstInsuranceCoverageRule> query,
            Guid? insuranceProviderId,
            string? itemType,
            bool? isActive,
            string? search)
        {
            if (insuranceProviderId.HasValue && insuranceProviderId.Value != Guid.Empty)
                query = query.Where(x => x.InsuranceProviderId == insuranceProviderId.Value);

            if (!string.IsNullOrWhiteSpace(itemType))
            {
                var normalizedItemType = itemType.Trim().ToLower();
                query = query.Where(x => x.ItemType.ToLower() == normalizedItemType);
            }

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.RuleCode.ToLower().Contains(keyword) ||
                    x.RuleName.ToLower().Contains(keyword) ||
                    x.ItemType.ToLower().Contains(keyword) ||
                    x.CoverageStatus.ToLower().Contains(keyword) ||
                    (x.BenefitPlanCode != null && x.BenefitPlanCode.ToLower().Contains(keyword)) ||
                    (x.BenefitPlanName != null && x.BenefitPlanName.ToLower().Contains(keyword)) ||
                    (x.PatientClassName != null && x.PatientClassName.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.InsuranceProvider != null && x.InsuranceProvider.InsuranceProviderCode.ToLower().Contains(keyword)) ||
                    (x.InsuranceProvider != null && x.InsuranceProvider.InsuranceProviderName.ToLower().Contains(keyword)) ||
                    (x.Tariff != null && x.Tariff.TariffCode.ToLower().Contains(keyword)) ||
                    (x.Tariff != null && x.Tariff.TariffName.ToLower().Contains(keyword)) ||
                    (x.Drug != null && x.Drug.DrugCode.ToLower().Contains(keyword)) ||
                    (x.Drug != null && x.Drug.DrugName.ToLower().Contains(keyword)) ||
                    (x.DrugCategory != null && x.DrugCategory.DrugCategoryCode.ToLower().Contains(keyword)) ||
                    (x.DrugCategory != null && x.DrugCategory.DrugCategoryName.ToLower().Contains(keyword)) ||
                    (x.Procedure != null && x.Procedure.ProcedureCode.ToLower().Contains(keyword)) ||
                    (x.Procedure != null && x.Procedure.ProcedureName.ToLower().Contains(keyword)) ||
                    (x.TariffCategory != null && x.TariffCategory.TariffCategoryCode.ToLower().Contains(keyword)) ||
                    (x.TariffCategory != null && x.TariffCategory.TariffCategoryName.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IQueryable<MstInsuranceCoverageRule> ApplyDateFilter(
            IQueryable<MstInsuranceCoverageRule> query,
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            var now = AppDateTimeHelper.OperationalDate();

            if (!string.IsNullOrWhiteSpace(customPeriod))
            {
                switch (customPeriod.Trim().ToLowerInvariant())
                {
                    case "today":
                        startDate = now;
                        endDate = now;
                        break;

                    case "last7days":
                        startDate = now.AddDays(-6);
                        endDate = now;
                        break;

                    case "thismonth":
                        startDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        endDate = startDate.Value.AddMonths(1).AddDays(-1);
                        break;

                    case "lastmonth":
                        var firstDayThisMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        startDate = firstDayThisMonth.AddMonths(-1);
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

        private static IOrderedQueryable<MstInsuranceCoverageRule> ApplySorting(
            IQueryable<MstInsuranceCoverageRule> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => isDescending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "rulecode" => isDescending
                    ? query.OrderByDescending(x => x.RuleCode)
                    : query.OrderBy(x => x.RuleCode),

                "rulename" => isDescending
                    ? query.OrderByDescending(x => x.RuleName)
                    : query.OrderBy(x => x.RuleName),

                "insuranceProviderName" => isDescending
                    ? query.OrderByDescending(x => x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceProviderName : string.Empty)
                    : query.OrderBy(x => x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceProviderName : string.Empty),

                "insuranceprovidername" => isDescending
                    ? query.OrderByDescending(x => x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceProviderName : string.Empty)
                    : query.OrderBy(x => x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceProviderName : string.Empty),

                "itemtype" => isDescending
                    ? query.OrderByDescending(x => x.ItemType)
                    : query.OrderBy(x => x.ItemType),

                "coveragestatus" => isDescending
                    ? query.OrderByDescending(x => x.CoverageStatus)
                    : query.OrderBy(x => x.CoverageStatus),

                "coveragepercent" => isDescending
                    ? query.OrderByDescending(x => x.CoveragePercent)
                    : query.OrderBy(x => x.CoveragePercent),

                "effectivestartdate" => isDescending
                    ? query.OrderByDescending(x => x.EffectiveStartDate)
                    : query.OrderBy(x => x.EffectiveStartDate),

                "effectiveenddate" => isDescending
                    ? query.OrderByDescending(x => x.EffectiveEndDate)
                    : query.OrderBy(x => x.EffectiveEndDate),

                "iscovered" => isDescending
                    ? query.OrderByDescending(x => x.IsCovered)
                    : query.OrderBy(x => x.IsCovered),

                "isexcluded" => isDescending
                    ? query.OrderByDescending(x => x.IsExcluded)
                    : query.OrderBy(x => x.IsExcluded),

                "isneedapproval" => isDescending
                    ? query.OrderByDescending(x => x.IsNeedApproval)
                    : query.OrderBy(x => x.IsNeedApproval),

                "isneedguaranteeletter" => isDescending
                    ? query.OrderByDescending(x => x.IsNeedGuaranteeLetter)
                    : query.OrderBy(x => x.IsNeedGuaranteeLetter),

                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => isDescending
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.RuleName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.RuleName)
            };
        }

        private static InsuranceCoverageRuleResponse ToResponse(MstInsuranceCoverageRule x)
        {
            return new InsuranceCoverageRuleResponse
            {
                Id = x.Id,
                InsuranceProviderId = x.InsuranceProviderId,
                InsuranceProviderCode = x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceProviderCode : string.Empty,
                InsuranceProviderName = x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceProviderName : string.Empty,
                InsuranceGroupName = x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceGroupName : null,
                RuleCode = x.RuleCode,
                RuleName = x.RuleName,
                ItemType = x.ItemType,
                TariffId = x.TariffId,
                TariffCode = x.Tariff != null ? x.Tariff.TariffCode : null,
                TariffName = x.Tariff != null ? x.Tariff.TariffName : null,
                DrugId = x.DrugId,
                DrugCode = x.Drug != null ? x.Drug.DrugCode : null,
                DrugName = x.Drug != null ? x.Drug.DrugName : null,
                DrugCategoryId = x.DrugCategoryId,
                DrugCategoryCode = x.DrugCategory != null ? x.DrugCategory.DrugCategoryCode : null,
                DrugCategoryName = x.DrugCategory != null ? x.DrugCategory.DrugCategoryName : null,
                ProcedureId = x.ProcedureId,
                ProcedureCode = x.Procedure != null ? x.Procedure.ProcedureCode : null,
                ProcedureName = x.Procedure != null ? x.Procedure.ProcedureName : null,
                TariffCategoryId = x.TariffCategoryId,
                TariffCategoryCode = x.TariffCategory != null ? x.TariffCategory.TariffCategoryCode : null,
                TariffCategoryName = x.TariffCategory != null ? x.TariffCategory.TariffCategoryName : null,
                BenefitPlanCode = x.BenefitPlanCode,
                BenefitPlanName = x.BenefitPlanName,
                PatientClassName = x.PatientClassName,
                CoverageStatus = x.CoverageStatus,
                CoveragePercent = x.CoveragePercent,
                MaxCoverageAmount = x.MaxCoverageAmount,
                CoPaymentPercent = x.CoPaymentPercent,
                CoPaymentAmount = x.CoPaymentAmount,
                IsCovered = x.IsCovered,
                IsExcluded = x.IsExcluded,
                IsNeedApproval = x.IsNeedApproval,
                IsNeedGuaranteeLetter = x.IsNeedGuaranteeLetter,
                IsAllowExcessPaymentByPatient = x.IsAllowExcessPaymentByPatient,
                MaxQuantityPerVisit = x.MaxQuantityPerVisit,
                MaxQuantityPerMonth = x.MaxQuantityPerMonth,
                MaxAmountPerVisit = x.MaxAmountPerVisit,
                MaxAmountPerMonth = x.MaxAmountPerMonth,
                EffectiveStartDate = x.EffectiveStartDate,
                EffectiveEndDate = x.EffectiveEndDate,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateInsuranceCoverageRuleRequest request)
        {
            if (request.InsuranceProviderId == Guid.Empty)
                return (false, "Insurance provider wajib dipilih.");

            if (string.IsNullOrWhiteSpace(request.RuleName))
                return (false, "Nama rule wajib diisi.");

            if (string.IsNullOrWhiteSpace(request.ItemType))
                return (false, "Item type wajib diisi.");

            if (!AllowedItemTypes.Contains(request.ItemType.Trim()))
                return (false, "Item type tidak valid. Gunakan salah satu: Tariff, Drug, DrugCategory, Procedure, ServiceCategory.");

            if (string.IsNullOrWhiteSpace(request.CoverageStatus))
                return (false, "Coverage status wajib diisi.");

            if (!AllowedCoverageStatuses.Contains(request.CoverageStatus.Trim()))
                return (false, "Coverage status tidak valid. Gunakan salah satu: Covered, NotCovered, PartialCovered, NeedApproval.");

            if (request.CoveragePercent < 0 || request.CoveragePercent > 100)
                return (false, "Coverage percent harus berada di antara 0 sampai 100.");

            if (request.MaxCoverageAmount.HasValue && request.MaxCoverageAmount.Value < 0)
                return (false, "Max coverage amount tidak boleh kurang dari 0.");

            if (request.CoPaymentPercent.HasValue && (request.CoPaymentPercent.Value < 0 || request.CoPaymentPercent.Value > 100))
                return (false, "Co payment percent harus berada di antara 0 sampai 100.");

            if (request.CoPaymentAmount.HasValue && request.CoPaymentAmount.Value < 0)
                return (false, "Co payment amount tidak boleh kurang dari 0.");

            if (request.MaxQuantityPerVisit.HasValue && request.MaxQuantityPerVisit.Value < 0)
                return (false, "Max quantity per visit tidak boleh kurang dari 0.");

            if (request.MaxQuantityPerMonth.HasValue && request.MaxQuantityPerMonth.Value < 0)
                return (false, "Max quantity per month tidak boleh kurang dari 0.");

            if (request.MaxAmountPerVisit.HasValue && request.MaxAmountPerVisit.Value < 0)
                return (false, "Max amount per visit tidak boleh kurang dari 0.");

            if (request.MaxAmountPerMonth.HasValue && request.MaxAmountPerMonth.Value < 0)
                return (false, "Max amount per month tidak boleh kurang dari 0.");

            if (request.EffectiveStartDate.HasValue &&
                request.EffectiveEndDate.HasValue &&
                request.EffectiveEndDate.Value < request.EffectiveStartDate.Value)
            {
                return (false, "Tanggal akhir berlaku tidak boleh lebih kecil dari tanggal mulai berlaku.");
            }

            var providerExists = await _dbContext.Set<MstInsuranceProvider>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == request.InsuranceProviderId &&
                    !x.IsDelete &&
                    x.IsActive);

            if (!providerExists)
                return (false, "Insurance provider tidak ditemukan atau tidak aktif.");

            var itemType = NormalizeItemType(request.ItemType);

            var tariffId = NormalizeNullableGuid(request.TariffId);
            var drugId = NormalizeNullableGuid(request.DrugId);
            var drugCategoryId = NormalizeNullableGuid(request.DrugCategoryId);
            var procedureId = NormalizeNullableGuid(request.ProcedureId);
            var tariffCategoryId = NormalizeNullableGuid(request.TariffCategoryId);

            if (itemType == "Tariff")
            {
                if (!tariffId.HasValue)
                    return (false, "Tariff wajib dipilih untuk item type Tariff.");

                var exists = await _dbContext.Set<MstTariff>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == tariffId.Value && !x.IsDelete && x.IsActive);

                if (!exists)
                    return (false, "Tariff tidak ditemukan atau tidak aktif.");
            }

            if (itemType == "Drug")
            {
                if (!drugId.HasValue)
                    return (false, "Drug wajib dipilih untuk item type Drug.");

                var exists = await _dbContext.Set<MstDrug>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == drugId.Value && !x.IsDelete && x.IsActive);

                if (!exists)
                    return (false, "Drug tidak ditemukan atau tidak aktif.");
            }

            if (itemType == "DrugCategory")
            {
                if (!drugCategoryId.HasValue)
                    return (false, "Drug category wajib dipilih untuk item type DrugCategory.");

                var exists = await _dbContext.Set<MstDrugCategory>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == drugCategoryId.Value && !x.IsDelete && x.IsActive);

                if (!exists)
                    return (false, "Drug category tidak ditemukan atau tidak aktif.");
            }

            if (itemType == "Procedure")
            {
                if (!procedureId.HasValue)
                    return (false, "Procedure wajib dipilih untuk item type Procedure.");

                var exists = await _dbContext.Set<MstProcedure>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == procedureId.Value && !x.IsDelete && x.IsActive);

                if (!exists)
                    return (false, "Procedure tidak ditemukan atau tidak aktif.");
            }

            if (itemType == "ServiceCategory")
            {
                if (!tariffCategoryId.HasValue)
                    return (false, "Tariff category wajib dipilih untuk item type ServiceCategory.");

                var exists = await _dbContext.Set<MstTariffCategory>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == tariffCategoryId.Value && !x.IsDelete && x.IsActive);

                if (!exists)
                    return (false, "Tariff category tidak ditemukan atau tidak aktif.");
            }

            var benefitPlanCode = NormalizeNullableString(request.BenefitPlanCode)?.ToUpperInvariant();
            var patientClassName = NormalizeNullableString(request.PatientClassName)?.ToLowerInvariant();
            var normalizedName = request.RuleName.Trim().ToLowerInvariant();

            var duplicateName = await _dbContext.Set<MstInsuranceCoverageRule>()
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.InsuranceProviderId == request.InsuranceProviderId &&
                    x.ItemType == itemType &&
                    x.TariffId == (itemType == "Tariff" ? tariffId : null) &&
                    x.DrugId == (itemType == "Drug" ? drugId : null) &&
                    x.DrugCategoryId == (itemType == "DrugCategory" ? drugCategoryId : null) &&
                    x.ProcedureId == (itemType == "Procedure" ? procedureId : null) &&
                    x.TariffCategoryId == (itemType == "ServiceCategory" ? tariffCategoryId : null) &&
                    x.BenefitPlanCode == benefitPlanCode &&
                    (x.PatientClassName == null ? patientClassName == null : x.PatientClassName.ToLower() == patientClassName) &&
                    x.RuleName.ToLower() == normalizedName &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateName)
                return (false, "Nama insurance coverage rule pada provider, item, benefit plan, dan kelas pasien tersebut sudah digunakan.");

            return (true, null);
        }

        private async Task<string> GenerateRuleCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstInsuranceCoverageRule>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.RuleCode.StartsWith(RuleCodePrefix))
                .Select(x => x.RuleCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(TryExtractRuleSequenceNumber)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
                nextNumber++;

            return $"{RuleCodePrefix}{nextNumber.ToString().PadLeft(RuleCodeDigitLength, '0')}";
        }

        private static int? TryExtractRuleSequenceNumber(string ruleCode)
        {
            if (string.IsNullOrWhiteSpace(ruleCode))
                return null;

            if (!ruleCode.StartsWith(RuleCodePrefix, StringComparison.OrdinalIgnoreCase))
                return null;

            var numberText = ruleCode[RuleCodePrefix.Length..];

            return int.TryParse(numberText, out var number)
                ? number
                : null;
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

        private static string NormalizeItemType(string value)
        {
            var trimmed = value.Trim();

            var matched = AllowedItemTypes
                .FirstOrDefault(x => string.Equals(x, trimmed, StringComparison.OrdinalIgnoreCase));

            return matched ?? "Tariff";
        }

        private static string NormalizeCoverageStatus(string value)
        {
            var trimmed = value.Trim();

            var matched = AllowedCoverageStatuses
                .FirstOrDefault(x => string.Equals(x, trimmed, StringComparison.OrdinalIgnoreCase));

            return matched ?? "Covered";
        }

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            return value.HasValue && value.Value != Guid.Empty
                ? value.Value
                : null;
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

        private static string SplitPascalCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var chars = new List<char> { value[0] };

            for (var i = 1; i < value.Length; i++)
            {
                if (char.IsUpper(value[i]) && !char.IsWhiteSpace(value[i - 1]))
                    chars.Add(' ');

                chars.Add(value[i]);
            }

            return new string(chars.ToArray());
        }
    }
}
