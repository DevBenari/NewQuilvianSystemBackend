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

using ResponsePayrollComponentCategoryPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.DTOs.PayrollComponentCategoryResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/payroll-component-categories")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Payroll Component Category",
        AreaName = "Corporate",
        ControllerName = "PayrollComponentCategory",
        Description = "Corporate human resource master data payroll component category",
        SortOrder = 40)]
    [Tags("Corporate / Human Resource / Master Data / Payroll Component Category")]
    public class PayrollComponentCategoryController : ControllerBase
    {
        private static readonly HashSet<string> AllowedComponentGroups = new(StringComparer.OrdinalIgnoreCase)
        {
            "Earning",
            "Deduction",
            "EmployerContribution",
            "Information"
        };

        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "PCC-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public PayrollComponentCategoryController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<PayrollComponentCategoryFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Payroll Component Category", Description = "Melihat metadata filter payroll component category", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PayrollComponentCategory", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new PayrollComponentCategoryFilterMetadataResponse
            {
                DefaultFilter = new PayrollComponentCategoryDefaultFilterResponse(),
                ComponentGroupOptions = AllowedComponentGroups
                    .OrderBy(x => x)
                    .Select(x => new PayrollMasterStringOptionResponse
                    {
                        Value = x,
                        Label = BuildComponentGroupLabel(x)
                    })
                    .ToList(),
                CustomPeriods = BuildPeriodOptions(),
                SortOptions = new List<PayrollMasterSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "componentCategoryCode", Label = "Kode kategori" },
                    new() { Value = "componentCategoryName", Label = "Nama kategori" },
                    new() { Value = "componentGroup", Label = "Kelompok komponen" },
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PayrollComponentCategory.GetFilterMetadata",
                "Mengambil metadata filter payroll component category.",
                result);

            return Ok(ApiResponse<PayrollComponentCategoryFilterMetadataResponse>.Ok(
                result,
                "Metadata filter payroll component category berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<PayrollComponentCategorySummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Payroll Component Category", Description = "Melihat ringkasan payroll component category", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PayrollComponentCategory", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();

            var result = new PayrollComponentCategorySummaryResponse
            {
                TotalCategory = await query.CountAsync(),
                ActiveCategory = await query.CountAsync(x => x.IsActive),
                InactiveCategory = await query.CountAsync(x => !x.IsActive),
                EarningCategory = await query.CountAsync(x => x.ComponentGroup == "Earning"),
                DeductionCategory = await query.CountAsync(x => x.ComponentGroup == "Deduction"),
                EmployerContributionCategory = await query.CountAsync(x => x.ComponentGroup == "EmployerContribution"),
                InformationCategory = await query.CountAsync(x => x.ComponentGroup == "Information")
            };

            return Ok(ApiResponse<PayrollComponentCategorySummaryResponse>.Ok(
                result,
                "Ringkasan payroll component category berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponsePayrollComponentCategoryPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Payroll Component Category", Description = "Melihat data payroll component category", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PayrollComponentCategory", "Read")]
        public async Task<IActionResult> GetPayrollComponentCategories(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] string? componentGroup,
            [FromQuery] bool? affectsGrossPay,
            [FromQuery] bool? affectsTaxableIncome,
            [FromQuery] bool? affectsTakeHomePay,
            [FromQuery] bool? isEmployerCost,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "componentCategoryName",
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
                componentGroup,
                affectsGrossPay,
                affectsTaxableIncome,
                affectsTakeHomePay,
                isEmployerCost,
                isActive,
                search);

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new PayrollComponentCategoryResponse
                {
                    Id = x.Id,
                    ComponentCategoryCode = x.ComponentCategoryCode,
                    ComponentCategoryName = x.ComponentCategoryName,
                    ComponentGroup = x.ComponentGroup,
                    AffectsGrossPay = x.AffectsGrossPay,
                    AffectsTaxableIncome = x.AffectsTaxableIncome,
                    AffectsTakeHomePay = x.AffectsTakeHomePay,
                    IsEmployerCost = x.IsEmployerCost,
                    Description = x.Description,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    PayrollComponentCount = _dbContext.Set<MstPayrollComponent>()
                        .Count(c => c.PayrollComponentCategoryId == x.Id && !c.IsDelete),
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

            var result = new ResponsePayrollComponentCategoryPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponsePayrollComponentCategoryPagedResult>.Ok(
                result,
                "Data payroll component category berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<PayrollComponentCategoryOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Payroll Component Category", Description = "Melihat pilihan payroll component category", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PayrollComponentCategory", "Read")]
        public async Task<IActionResult> GetPayrollComponentCategoryOptions(
            [FromQuery] string? componentGroup,
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
                componentGroup,
                null,
                null,
                null,
                null,
                onlyActive ? true : null,
                search);

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ComponentCategoryName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new PayrollComponentCategoryOptionResponse
                {
                    Id = x.Id,
                    ComponentCategoryCode = x.ComponentCategoryCode,
                    ComponentCategoryName = x.ComponentCategoryName,
                    ComponentGroup = x.ComponentGroup
                })
                .ToListAsync();

            return Ok(ApiResponse<PayrollComponentCategoryOptionPagedResponse>.Ok(
                new PayrollComponentCategoryOptionPagedResponse
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data pilihan payroll component category berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PayrollComponentCategoryDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Payroll Component Category", Description = "Melihat detail payroll component category", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PayrollComponentCategory", "Read")]
        public async Task<IActionResult> GetPayrollComponentCategoryById(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new PayrollComponentCategoryDetailResponse
                {
                    Id = x.Id,
                    ComponentCategoryCode = x.ComponentCategoryCode,
                    ComponentCategoryName = x.ComponentCategoryName,
                    ComponentGroup = x.ComponentGroup,
                    AffectsGrossPay = x.AffectsGrossPay,
                    AffectsTaxableIncome = x.AffectsTaxableIncome,
                    AffectsTakeHomePay = x.AffectsTakeHomePay,
                    IsEmployerCost = x.IsEmployerCost,
                    Description = x.Description,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    PayrollComponentCount = _dbContext.Set<MstPayrollComponent>()
                        .Count(c => c.PayrollComponentCategoryId == x.Id && !c.IsDelete),
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
                    "Payroll component category tidak ditemukan."));
            }

            return Ok(ApiResponse<PayrollComponentCategoryDetailResponse>.Ok(
                data,
                "Detail payroll component category berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PayrollComponentCategoryCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Payroll Component Category", Description = "Membuat payroll component category", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PayrollComponentCategory", "Create")]
        public async Task<IActionResult> CreatePayrollComponentCategory(
            [FromBody] CreatePayrollComponentCategoryRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data payroll component category tidak valid."));
            }

            var entity = new MstPayrollComponentCategory
            {
                Id = Guid.NewGuid(),
                ComponentCategoryCode = await GenerateCodeAsync(),
                ComponentCategoryName = request.ComponentCategoryName.Trim(),
                ComponentGroup = NormalizeComponentGroup(request.ComponentGroup),
                AffectsGrossPay = request.AffectsGrossPay,
                AffectsTaxableIncome = request.AffectsTaxableIncome,
                AffectsTakeHomePay = request.AffectsTakeHomePay,
                IsEmployerCost = request.IsEmployerCost,
                Description = NormalizeNullableString(request.Description),
                SortOrder = request.SortOrder,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstPayrollComponentCategory>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var result = new PayrollComponentCategoryCreateResponse
            {
                Id = entity.Id,
                ComponentCategoryCode = entity.ComponentCategoryCode,
                ComponentCategoryName = entity.ComponentCategoryName,
                IsActive = entity.IsActive
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PayrollComponentCategory.Create",
                "Membuat payroll component category.",
                result);

            return Ok(ApiResponse<PayrollComponentCategoryCreateResponse>.Ok(
                result,
                "Payroll component category berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Update", "Update Payroll Component Category", Description = "Mengubah payroll component category", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PayrollComponentCategory", "Update")]
        public async Task<IActionResult> UpdatePayrollComponentCategory(
            Guid id,
            [FromBody] UpdatePayrollComponentCategoryRequest request)
        {
            var entity = await _dbContext.Set<MstPayrollComponentCategory>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Payroll component category tidak ditemukan."));
            }

            var validation = await ValidateRequestAsync(id, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data payroll component category tidak valid."));
            }

            entity.ComponentCategoryName = request.ComponentCategoryName.Trim();
            entity.ComponentGroup = NormalizeComponentGroup(request.ComponentGroup);
            entity.AffectsGrossPay = request.AffectsGrossPay;
            entity.AffectsTaxableIncome = request.AffectsTaxableIncome;
            entity.AffectsTakeHomePay = request.AffectsTakeHomePay;
            entity.IsEmployerCost = request.IsEmployerCost;
            entity.Description = NormalizeNullableString(request.Description);
            entity.SortOrder = request.SortOrder;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Payroll component category berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Payroll Component Category Status", Description = "Mengubah status payroll component category", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("PayrollComponentCategory", "Update")]
        public async Task<IActionResult> UpdatePayrollComponentCategoryStatus(
            Guid id,
            [FromBody] UpdatePayrollComponentCategoryStatusRequest request)
        {
            var entity = await _dbContext.Set<MstPayrollComponentCategory>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Payroll component category tidak ditemukan."));
            }

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status payroll component category berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Delete", "Delete Payroll Component Category", Description = "Menghapus payroll component category", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("PayrollComponentCategory", "Delete")]
        public async Task<IActionResult> DeletePayrollComponentCategory(Guid id)
        {
            var entity = await _dbContext.Set<MstPayrollComponentCategory>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Payroll component category tidak ditemukan."));
            }

            var isUsed = await _dbContext.Set<MstPayrollComponent>()
                .AsNoTracking()
                .AnyAsync(x => x.PayrollComponentCategoryId == id && !x.IsDelete);

            if (isUsed)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Payroll component category tidak dapat dihapus karena sudah digunakan oleh payroll component."));
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
                "Payroll component category berhasil dihapus."));
        }

        private IQueryable<MstPayrollComponentCategory> BuildBaseQuery()
        {
            return _dbContext.Set<MstPayrollComponentCategory>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstPayrollComponentCategory> ApplyDateFilter(
            IQueryable<MstPayrollComponentCategory> query,
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

        private static IQueryable<MstPayrollComponentCategory> ApplyStandardFilter(
            IQueryable<MstPayrollComponentCategory> query,
            string? componentGroup,
            bool? affectsGrossPay,
            bool? affectsTaxableIncome,
            bool? affectsTakeHomePay,
            bool? isEmployerCost,
            bool? isActive,
            string? search)
        {
            if (!string.IsNullOrWhiteSpace(componentGroup))
                query = query.Where(x => x.ComponentGroup == componentGroup.Trim());

            if (affectsGrossPay.HasValue)
                query = query.Where(x => x.AffectsGrossPay == affectsGrossPay.Value);

            if (affectsTaxableIncome.HasValue)
                query = query.Where(x => x.AffectsTaxableIncome == affectsTaxableIncome.Value);

            if (affectsTakeHomePay.HasValue)
                query = query.Where(x => x.AffectsTakeHomePay == affectsTakeHomePay.Value);

            if (isEmployerCost.HasValue)
                query = query.Where(x => x.IsEmployerCost == isEmployerCost.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ComponentCategoryCode.ToLower().Contains(keyword) ||
                    x.ComponentCategoryName.ToLower().Contains(keyword) ||
                    x.ComponentGroup.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<MstPayrollComponentCategory> ApplySorting(
            IQueryable<MstPayrollComponentCategory> query,
            string? sortBy,
            string? sortDirection)
        {
            var descending = string.Equals(
                sortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "componentCategoryName").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => descending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "componentcategorycode" => descending
                    ? query.OrderByDescending(x => x.ComponentCategoryCode)
                    : query.OrderBy(x => x.ComponentCategoryCode),

                "componentgroup" => descending
                    ? query.OrderByDescending(x => x.ComponentGroup)
                    : query.OrderBy(x => x.ComponentGroup),

                "sortorder" => descending
                    ? query.OrderByDescending(x => x.SortOrder)
                    : query.OrderBy(x => x.SortOrder),

                "isactive" => descending
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => descending
                    ? query.OrderByDescending(x => x.ComponentCategoryName)
                    : query.OrderBy(x => x.ComponentCategoryName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreatePayrollComponentCategoryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ComponentCategoryName))
                return (false, "Nama payroll component category wajib diisi.");

            if (!AllowedComponentGroups.Contains(request.ComponentGroup.Trim()))
                return (false, "ComponentGroup tidak valid.");

            var normalizedName = request.ComponentCategoryName.Trim().ToLower();

            var duplicateQuery = _dbContext.Set<MstPayrollComponentCategory>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.ComponentCategoryName.ToLower() == normalizedName &&
                    x.ComponentGroup == request.ComponentGroup.Trim());

            if (excludeId.HasValue)
                duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateQuery.AnyAsync())
                return (false, "Nama kategori sudah digunakan pada kelompok komponen tersebut.");

            return (true, null);
        }

        private async Task<string> GenerateCodeAsync()
        {
            var codes = await _dbContext.Set<MstPayrollComponentCategory>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.ComponentCategoryCode.StartsWith(CodePrefix))
                .Select(x => x.ComponentCategoryCode)
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

        private static string NormalizeComponentGroup(string value)
        {
            return AllowedComponentGroups.First(x =>
                x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static string? NormalizeNullableString(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
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

        private static string BuildComponentGroupLabel(string value)
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
    }
}
