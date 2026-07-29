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

using ResponsePayrollComponentPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.DTOs.PayrollComponentResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/payroll-components")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Payroll Component",
        AreaName = "Corporate",
        ControllerName = "PayrollComponent",
        Description = "Corporate human resource master data payroll component",
        SortOrder = 41)]
    [Tags("Corporate / Human Resource / Master Data / Payroll Component")]
    public class PayrollComponentController : ControllerBase
    {
        private static readonly HashSet<string> AllowedComponentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "Earning",
            "Deduction",
            "EmployerContribution",
            "Information"
        };

        private static readonly HashSet<string> AllowedCalculationMethods = new(StringComparer.OrdinalIgnoreCase)
        {
            "Fixed",
            "Percentage",
            "Formula",
            "ManualInput",
            "Attendance",
            "Overtime",
            "Benefit"
        };

        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "PCO-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public PayrollComponentController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<PayrollComponentFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Payroll Component", Description = "Melihat metadata filter payroll component", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PayrollComponent", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new PayrollComponentFilterMetadataResponse
            {
                DefaultFilter = new PayrollComponentDefaultFilterResponse(),
                ComponentTypeOptions = AllowedComponentTypes
                    .OrderBy(x => x)
                    .Select(x => new PayrollMasterStringOptionResponse
                    {
                        Value = x,
                        Label = BuildComponentTypeLabel(x)
                    })
                    .ToList(),
                CalculationMethodOptions = AllowedCalculationMethods
                    .OrderBy(x => x)
                    .Select(x => new PayrollMasterStringOptionResponse
                    {
                        Value = x,
                        Label = BuildCalculationMethodLabel(x)
                    })
                    .ToList(),
                CustomPeriods = BuildPeriodOptions(),
                SortOptions = new List<PayrollMasterSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "payrollComponentCode", Label = "Kode komponen" },
                    new() { Value = "payrollComponentName", Label = "Nama komponen" },
                    new() { Value = "categoryName", Label = "Kategori" },
                    new() { Value = "componentType", Label = "Tipe komponen" },
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PayrollComponent.GetFilterMetadata",
                "Mengambil metadata filter payroll component.",
                result);

            return Ok(ApiResponse<PayrollComponentFilterMetadataResponse>.Ok(
                result,
                "Metadata filter payroll component berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<PayrollComponentSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Payroll Component", Description = "Melihat ringkasan payroll component", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PayrollComponent", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();

            var result = new PayrollComponentSummaryResponse
            {
                TotalComponent = await query.CountAsync(),
                ActiveComponent = await query.CountAsync(x => x.IsActive),
                InactiveComponent = await query.CountAsync(x => !x.IsActive),
                EarningComponent = await query.CountAsync(x => x.ComponentType == "Earning"),
                DeductionComponent = await query.CountAsync(x => x.ComponentType == "Deduction"),
                EmployerContributionComponent = await query.CountAsync(x => x.ComponentType == "EmployerContribution"),
                RecurringComponent = await query.CountAsync(x => x.IsRecurring),
                TaxableComponent = await query.CountAsync(x => x.IsTaxable)
            };

            return Ok(ApiResponse<PayrollComponentSummaryResponse>.Ok(
                result,
                "Ringkasan payroll component berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponsePayrollComponentPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Payroll Component", Description = "Melihat data payroll component", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PayrollComponent", "Read")]
        public async Task<IActionResult> GetPayrollComponents(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? payrollComponentCategoryId,
            [FromQuery] string? componentType,
            [FromQuery] string? calculationMethod,
            [FromQuery] bool? isRecurring,
            [FromQuery] bool? isTaxable,
            [FromQuery] bool? isProrated,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "payrollComponentName",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyDateFilter(BuildBaseQuery(), startDate, endDate, customPeriod);
            query = ApplyStandardFilter(
                query,
                payrollComponentCategoryId,
                componentType,
                calculationMethod,
                isRecurring,
                isTaxable,
                isProrated,
                isActive,
                search);

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new PayrollComponentResponse
                {
                    Id = x.Id,
                    PayrollComponentCategoryId = x.PayrollComponentCategoryId,
                    PayrollComponentCategoryCode = x.PayrollComponentCategory != null
                        ? x.PayrollComponentCategory.ComponentCategoryCode
                        : string.Empty,
                    PayrollComponentCategoryName = x.PayrollComponentCategory != null
                        ? x.PayrollComponentCategory.ComponentCategoryName
                        : string.Empty,
                    BaseComponentId = x.BaseComponentId,
                    BaseComponentCode = x.BaseComponent != null
                        ? x.BaseComponent.PayrollComponentCode
                        : null,
                    BaseComponentName = x.BaseComponent != null
                        ? x.BaseComponent.PayrollComponentName
                        : null,
                    PayrollComponentCode = x.PayrollComponentCode,
                    PayrollComponentName = x.PayrollComponentName,
                    ComponentType = x.ComponentType,
                    CalculationMethod = x.CalculationMethod,
                    FormulaExpression = x.FormulaExpression,
                    DefaultAmount = x.DefaultAmount,
                    DefaultPercentage = x.DefaultPercentage,
                    IsRecurring = x.IsRecurring,
                    IsTaxable = x.IsTaxable,
                    IsProrated = x.IsProrated,
                    IsAttendanceBased = x.IsAttendanceBased,
                    IsOvertimeBased = x.IsOvertimeBased,
                    IsBenefitBased = x.IsBenefitBased,
                    IsEmployerContribution = x.IsEmployerContribution,
                    IsEmployeeContribution = x.IsEmployeeContribution,
                    IsDisplayedOnPayslip = x.IsDisplayedOnPayslip,
                    IsEditableDuringPayroll = x.IsEditableDuringPayroll,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    Description = x.Description,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    DerivedComponentCount = _dbContext.Set<MstPayrollComponent>()
                        .Count(c => c.BaseComponentId == x.Id && !c.IsDelete),
                    AllowanceTypeCount = _dbContext.Set<MstAllowanceType>()
                        .Count(c => c.PayrollComponentId == x.Id && !c.IsDelete),
                    DeductionTypeCount = _dbContext.Set<MstDeductionType>()
                        .Count(c => c.PayrollComponentId == x.Id && !c.IsDelete),
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty
                        ? null
                        : _dbContext.Users
                            .Where(u => u.Id == x.CreateBy)
                            .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
                            .FirstOrDefault()
                })
                .ToListAsync();

            var result = new ResponsePayrollComponentPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponsePayrollComponentPagedResult>.Ok(
                result,
                "Data payroll component berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<PayrollComponentOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Payroll Component", Description = "Melihat pilihan payroll component", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PayrollComponent", "Read")]
        public async Task<IActionResult> GetPayrollComponentOptions(
            [FromQuery] Guid? payrollComponentCategoryId,
            [FromQuery] string? componentType,
            [FromQuery] string? calculationMethod,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyStandardFilter(
                BuildBaseQuery(),
                payrollComponentCategoryId,
                componentType,
                calculationMethod,
                null,
                null,
                null,
                onlyActive ? true : null,
                search);

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.PayrollComponentName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new PayrollComponentOptionResponse
                {
                    Id = x.Id,
                    PayrollComponentCategoryId = x.PayrollComponentCategoryId,
                    PayrollComponentCode = x.PayrollComponentCode,
                    PayrollComponentName = x.PayrollComponentName,
                    ComponentType = x.ComponentType,
                    CalculationMethod = x.CalculationMethod
                })
                .ToListAsync();

            return Ok(ApiResponse<PayrollComponentOptionPagedResponse>.Ok(
                new PayrollComponentOptionPagedResponse
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data pilihan payroll component berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PayrollComponentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Payroll Component", Description = "Melihat detail payroll component", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PayrollComponent", "Read")]
        public async Task<IActionResult> GetPayrollComponentById(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new PayrollComponentDetailResponse
                {
                    Id = x.Id,
                    PayrollComponentCategoryId = x.PayrollComponentCategoryId,
                    PayrollComponentCategoryCode = x.PayrollComponentCategory != null
                        ? x.PayrollComponentCategory.ComponentCategoryCode
                        : string.Empty,
                    PayrollComponentCategoryName = x.PayrollComponentCategory != null
                        ? x.PayrollComponentCategory.ComponentCategoryName
                        : string.Empty,
                    BaseComponentId = x.BaseComponentId,
                    BaseComponentCode = x.BaseComponent != null
                        ? x.BaseComponent.PayrollComponentCode
                        : null,
                    BaseComponentName = x.BaseComponent != null
                        ? x.BaseComponent.PayrollComponentName
                        : null,
                    PayrollComponentCode = x.PayrollComponentCode,
                    PayrollComponentName = x.PayrollComponentName,
                    ComponentType = x.ComponentType,
                    CalculationMethod = x.CalculationMethod,
                    FormulaExpression = x.FormulaExpression,
                    DefaultAmount = x.DefaultAmount,
                    DefaultPercentage = x.DefaultPercentage,
                    IsRecurring = x.IsRecurring,
                    IsTaxable = x.IsTaxable,
                    IsProrated = x.IsProrated,
                    IsAttendanceBased = x.IsAttendanceBased,
                    IsOvertimeBased = x.IsOvertimeBased,
                    IsBenefitBased = x.IsBenefitBased,
                    IsEmployerContribution = x.IsEmployerContribution,
                    IsEmployeeContribution = x.IsEmployeeContribution,
                    IsDisplayedOnPayslip = x.IsDisplayedOnPayslip,
                    IsEditableDuringPayroll = x.IsEditableDuringPayroll,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    Description = x.Description,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    DerivedComponentCount = _dbContext.Set<MstPayrollComponent>()
                        .Count(c => c.BaseComponentId == x.Id && !c.IsDelete),
                    AllowanceTypeCount = _dbContext.Set<MstAllowanceType>()
                        .Count(c => c.PayrollComponentId == x.Id && !c.IsDelete),
                    DeductionTypeCount = _dbContext.Set<MstDeductionType>()
                        .Count(c => c.PayrollComponentId == x.Id && !c.IsDelete),
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty
                        ? null
                        : _dbContext.Users
                            .Where(u => u.Id == x.CreateBy)
                            .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
                            .FirstOrDefault(),
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
                    "Payroll component tidak ditemukan."));
            }

            return Ok(ApiResponse<PayrollComponentDetailResponse>.Ok(
                data,
                "Detail payroll component berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PayrollComponentCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Payroll Component", Description = "Membuat payroll component", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PayrollComponent", "Create")]
        public async Task<IActionResult> CreatePayrollComponent(
            [FromBody] CreatePayrollComponentRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data payroll component tidak valid."));
            }

            var entity = new MstPayrollComponent
            {
                Id = Guid.NewGuid(),
                PayrollComponentCategoryId = request.PayrollComponentCategoryId,
                BaseComponentId = NormalizeGuid(request.BaseComponentId),
                PayrollComponentCode = await GenerateCodeAsync(),
                PayrollComponentName = request.PayrollComponentName.Trim(),
                ComponentType = NormalizeComponentType(request.ComponentType),
                CalculationMethod = NormalizeCalculationMethod(request.CalculationMethod),
                FormulaExpression = NormalizeNullableString(request.FormulaExpression),
                DefaultAmount = request.DefaultAmount,
                DefaultPercentage = request.DefaultPercentage,
                IsRecurring = request.IsRecurring,
                IsTaxable = request.IsTaxable,
                IsProrated = request.IsProrated,
                IsAttendanceBased = request.IsAttendanceBased,
                IsOvertimeBased = request.IsOvertimeBased,
                IsBenefitBased = request.IsBenefitBased,
                IsEmployerContribution = request.IsEmployerContribution,
                IsEmployeeContribution = request.IsEmployeeContribution,
                IsDisplayedOnPayslip = request.IsDisplayedOnPayslip,
                IsEditableDuringPayroll = request.IsEditableDuringPayroll,
                EffectiveStartDate = request.EffectiveStartDate?.Date,
                EffectiveEndDate = request.EffectiveEndDate?.Date,
                Description = NormalizeNullableString(request.Description),
                SortOrder = request.SortOrder,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstPayrollComponent>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var result = new PayrollComponentCreateResponse
            {
                Id = entity.Id,
                PayrollComponentCode = entity.PayrollComponentCode,
                PayrollComponentName = entity.PayrollComponentName,
                IsActive = entity.IsActive
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PayrollComponent.Create",
                "Membuat payroll component.",
                result);

            return Ok(ApiResponse<PayrollComponentCreateResponse>.Ok(
                result,
                "Payroll component berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Update", "Update Payroll Component", Description = "Mengubah payroll component", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PayrollComponent", "Update")]
        public async Task<IActionResult> UpdatePayrollComponent(
            Guid id,
            [FromBody] UpdatePayrollComponentRequest request)
        {
            var entity = await _dbContext.Set<MstPayrollComponent>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Payroll component tidak ditemukan."));
            }

            var validation = await ValidateRequestAsync(id, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data payroll component tidak valid."));
            }

            entity.PayrollComponentCategoryId = request.PayrollComponentCategoryId;
            entity.BaseComponentId = NormalizeGuid(request.BaseComponentId);
            entity.PayrollComponentName = request.PayrollComponentName.Trim();
            entity.ComponentType = NormalizeComponentType(request.ComponentType);
            entity.CalculationMethod = NormalizeCalculationMethod(request.CalculationMethod);
            entity.FormulaExpression = NormalizeNullableString(request.FormulaExpression);
            entity.DefaultAmount = request.DefaultAmount;
            entity.DefaultPercentage = request.DefaultPercentage;
            entity.IsRecurring = request.IsRecurring;
            entity.IsTaxable = request.IsTaxable;
            entity.IsProrated = request.IsProrated;
            entity.IsAttendanceBased = request.IsAttendanceBased;
            entity.IsOvertimeBased = request.IsOvertimeBased;
            entity.IsBenefitBased = request.IsBenefitBased;
            entity.IsEmployerContribution = request.IsEmployerContribution;
            entity.IsEmployeeContribution = request.IsEmployeeContribution;
            entity.IsDisplayedOnPayslip = request.IsDisplayedOnPayslip;
            entity.IsEditableDuringPayroll = request.IsEditableDuringPayroll;
            entity.EffectiveStartDate = request.EffectiveStartDate?.Date;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.Description = NormalizeNullableString(request.Description);
            entity.SortOrder = request.SortOrder;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Payroll component berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Payroll Component Status", Description = "Mengubah status payroll component", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("PayrollComponent", "Update")]
        public async Task<IActionResult> UpdatePayrollComponentStatus(
            Guid id,
            [FromBody] UpdatePayrollComponentStatusRequest request)
        {
            var entity = await _dbContext.Set<MstPayrollComponent>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Payroll component tidak ditemukan."));
            }

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status payroll component berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Delete", "Delete Payroll Component", Description = "Menghapus payroll component", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("PayrollComponent", "Delete")]
        public async Task<IActionResult> DeletePayrollComponent(Guid id)
        {
            var entity = await _dbContext.Set<MstPayrollComponent>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Payroll component tidak ditemukan."));
            }

            var isUsed =
                await _dbContext.Set<MstPayrollComponent>()
                    .AsNoTracking()
                    .AnyAsync(x => x.BaseComponentId == id && !x.IsDelete) ||
                await _dbContext.Set<MstAllowanceType>()
                    .AsNoTracking()
                    .AnyAsync(x => x.PayrollComponentId == id && !x.IsDelete) ||
                await _dbContext.Set<MstDeductionType>()
                    .AsNoTracking()
                    .AnyAsync(x => x.PayrollComponentId == id && !x.IsDelete);

            if (isUsed)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Payroll component tidak dapat dihapus karena sudah digunakan oleh komponen turunan, allowance type, atau deduction type."));
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
                "Payroll component berhasil dihapus."));
        }

        private IQueryable<MstPayrollComponent> BuildBaseQuery()
        {
            return _dbContext.Set<MstPayrollComponent>()
                .AsNoTracking()
                .Include(x => x.PayrollComponentCategory)
                .Include(x => x.BaseComponent)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstPayrollComponent> ApplyDateFilter(
            IQueryable<MstPayrollComponent> query,
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            var range = ResolveDateRange(startDate, endDate, customPeriod);

            if (range.Start.HasValue)
                query = query.Where(x => x.CreateDateTime >= range.Start.Value);

            if (range.EndExclusive.HasValue)
                query = query.Where(x => x.CreateDateTime < range.EndExclusive.Value);

            return query;
        }

        private static IQueryable<MstPayrollComponent> ApplyStandardFilter(
            IQueryable<MstPayrollComponent> query,
            Guid? payrollComponentCategoryId,
            string? componentType,
            string? calculationMethod,
            bool? isRecurring,
            bool? isTaxable,
            bool? isProrated,
            bool? isActive,
            string? search)
        {
            if (payrollComponentCategoryId.HasValue && payrollComponentCategoryId.Value != Guid.Empty)
                query = query.Where(x => x.PayrollComponentCategoryId == payrollComponentCategoryId.Value);

            if (!string.IsNullOrWhiteSpace(componentType))
                query = query.Where(x => x.ComponentType == componentType.Trim());

            if (!string.IsNullOrWhiteSpace(calculationMethod))
                query = query.Where(x => x.CalculationMethod == calculationMethod.Trim());

            if (isRecurring.HasValue)
                query = query.Where(x => x.IsRecurring == isRecurring.Value);

            if (isTaxable.HasValue)
                query = query.Where(x => x.IsTaxable == isTaxable.Value);

            if (isProrated.HasValue)
                query = query.Where(x => x.IsProrated == isProrated.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.PayrollComponentCode.ToLower().Contains(keyword) ||
                    x.PayrollComponentName.ToLower().Contains(keyword) ||
                    x.ComponentType.ToLower().Contains(keyword) ||
                    x.CalculationMethod.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.PayrollComponentCategory != null &&
                     x.PayrollComponentCategory.ComponentCategoryName.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<MstPayrollComponent> ApplySorting(
            IQueryable<MstPayrollComponent> query,
            string? sortBy,
            string? sortDirection)
        {
            var descending = string.Equals(
                sortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "payrollComponentName").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => descending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "payrollcomponentcode" => descending
                    ? query.OrderByDescending(x => x.PayrollComponentCode)
                    : query.OrderBy(x => x.PayrollComponentCode),

                "categoryname" => descending
                    ? query.OrderByDescending(x => x.PayrollComponentCategory != null
                        ? x.PayrollComponentCategory.ComponentCategoryName
                        : string.Empty)
                    : query.OrderBy(x => x.PayrollComponentCategory != null
                        ? x.PayrollComponentCategory.ComponentCategoryName
                        : string.Empty),

                "componenttype" => descending
                    ? query.OrderByDescending(x => x.ComponentType)
                    : query.OrderBy(x => x.ComponentType),

                "sortorder" => descending
                    ? query.OrderByDescending(x => x.SortOrder)
                    : query.OrderBy(x => x.SortOrder),

                "isactive" => descending
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => descending
                    ? query.OrderByDescending(x => x.PayrollComponentName)
                    : query.OrderBy(x => x.PayrollComponentName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreatePayrollComponentRequest request)
        {
            if (request.PayrollComponentCategoryId == Guid.Empty)
                return (false, "Payroll component category wajib dipilih.");

            if (string.IsNullOrWhiteSpace(request.PayrollComponentName))
                return (false, "Nama payroll component wajib diisi.");

            if (!AllowedComponentTypes.Contains(request.ComponentType.Trim()))
                return (false, "ComponentType tidak valid.");

            if (!AllowedCalculationMethods.Contains(request.CalculationMethod.Trim()))
                return (false, "CalculationMethod tidak valid.");

            if (request.DefaultAmount < 0 || request.DefaultPercentage < 0)
                return (false, "Default amount dan default percentage tidak boleh negatif.");

            if (request.DefaultPercentage > 100)
                return (false, "Default percentage tidak boleh lebih dari 100.");

            if (request.CalculationMethod.Equals("Formula", StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrWhiteSpace(request.FormulaExpression))
            {
                return (false, "FormulaExpression wajib diisi untuk calculation method Formula.");
            }

            if (request.EffectiveStartDate.HasValue &&
                request.EffectiveEndDate.HasValue &&
                request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Value.Date)
            {
                return (false, "EffectiveEndDate tidak boleh lebih kecil dari EffectiveStartDate.");
            }

            var categoryExists = await _dbContext.Set<MstPayrollComponentCategory>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == request.PayrollComponentCategoryId &&
                    x.IsActive &&
                    !x.IsDelete);

            if (!categoryExists)
                return (false, "Payroll component category tidak ditemukan atau tidak aktif.");

            var baseComponentId = NormalizeGuid(request.BaseComponentId);

            if (baseComponentId.HasValue)
            {
                if (excludeId.HasValue && baseComponentId.Value == excludeId.Value)
                    return (false, "Payroll component tidak dapat menjadi base component untuk dirinya sendiri.");

                var baseExists = await _dbContext.Set<MstPayrollComponent>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == baseComponentId.Value &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!baseExists)
                    return (false, "Base component tidak ditemukan atau tidak aktif.");

                if (excludeId.HasValue)
                {
                    var createsCycle = await CreatesCircularReferenceAsync(
                        excludeId.Value,
                        baseComponentId.Value);

                    if (createsCycle)
                        return (false, "Base component membentuk relasi berulang.");
                }
            }

            var normalizedName = request.PayrollComponentName.Trim().ToLower();

            var duplicateQuery = _dbContext.Set<MstPayrollComponent>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.PayrollComponentCategoryId == request.PayrollComponentCategoryId &&
                    x.PayrollComponentName.ToLower() == normalizedName);

            if (excludeId.HasValue)
                duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateQuery.AnyAsync())
                return (false, "Nama payroll component sudah digunakan pada kategori tersebut.");

            return (true, null);
        }

        private async Task<bool> CreatesCircularReferenceAsync(
            Guid currentId,
            Guid proposedBaseId)
        {
            var visited = new HashSet<Guid>();
            var cursor = proposedBaseId;

            while (cursor != Guid.Empty && visited.Add(cursor))
            {
                if (cursor == currentId)
                    return true;

                var parentId = await _dbContext.Set<MstPayrollComponent>()
                    .AsNoTracking()
                    .Where(x => x.Id == cursor && !x.IsDelete)
                    .Select(x => x.BaseComponentId)
                    .FirstOrDefaultAsync();

                if (!parentId.HasValue)
                    return false;

                cursor = parentId.Value;
            }

            return false;
        }

        private async Task<string> GenerateCodeAsync()
        {
            var codes = await _dbContext.Set<MstPayrollComponent>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.PayrollComponentCode.StartsWith(CodePrefix))
                .Select(x => x.PayrollComponentCode)
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

        private static string NormalizeComponentType(string value)
        {
            return AllowedComponentTypes.First(x =>
                x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static string NormalizeCalculationMethod(string value)
        {
            return AllowedCalculationMethods.First(x =>
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

        private static (DateTime? Start, DateTime? EndExclusive) ResolveDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            if (startDate.HasValue || endDate.HasValue)
            {
                return (
                    startDate.HasValue
                        ? DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc)
                        : null,
                    endDate.HasValue
                        ? DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc)
                        : null);
            }

            var today = DateTime.UtcNow.Date;
            var monthStart = new DateTime(
                today.Year,
                today.Month,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc);

            return customPeriod?.Trim().ToLowerInvariant() switch
            {
                "today" => (today, today.AddDays(1)),
                "last7days" => (today.AddDays(-6), today.AddDays(1)),
                "thismonth" => (monthStart, monthStart.AddMonths(1)),
                "lastmonth" => (monthStart.AddMonths(-1), monthStart),
                _ => (null, null)
            };
        }

        private static List<PayrollMasterCustomPeriodOptionResponse> BuildPeriodOptions()
        {
            return new List<PayrollMasterCustomPeriodOptionResponse>
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "lastmonth", Label = "Bulan lalu" }
            };
        }

        private static string BuildComponentTypeLabel(string value)
        {
            return value switch
            {
                "Earning" => "Pendapatan",
                "Deduction" => "Potongan",
                "EmployerContribution" => "Kontribusi Pemberi Kerja",
                "Information" => "Informasi",
                _ => value
            };
        }

        private static string BuildCalculationMethodLabel(string value)
        {
            return value switch
            {
                "Fixed" => "Nilai Tetap",
                "Percentage" => "Persentase",
                "Formula" => "Formula",
                "ManualInput" => "Input Manual",
                "Attendance" => "Kehadiran",
                "Overtime" => "Lembur",
                "Benefit" => "Benefit",
                _ => value
            };
        }
    }
}
