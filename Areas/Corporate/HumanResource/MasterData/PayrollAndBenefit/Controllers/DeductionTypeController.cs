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

using DeductionTypePagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.DTOs.DeductionTypeResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/deduction-types")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Deduction Type",
        AreaName = "Corporate",
        ControllerName = "DeductionType",
        Description = "Corporate human resource master data deduction type",
        SortOrder = 43)]
    [Tags("Corporate / Human Resource / Master Data / Deduction Type")]
    public class DeductionTypeController : ControllerBase
    {

        private static readonly HashSet<string> AllowedDeductionCategories =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "Statutory", "Tax", "Insurance", "Loan", "Attendance", "Benefit", "Other"
            };

        private static readonly HashSet<string> AllowedCalculationMethods =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "Fixed", "Percentage", "Formula", "BalanceBased", "ManualInput"
            };

        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "DCT-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public DeductionTypeController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<DeductionTypeFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Deduction Type", Description = "Melihat metadata filter deduction type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DeductionType", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new DeductionTypeFilterMetadataResponse
            {
                DefaultFilter = new DeductionTypeDefaultFilterResponse(),
                CustomPeriods = BuildPeriodOptions(),
                SortOptions = new List<DeductionTypeSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "deductionTypeCode", Label = "Kode" },
                    new() { Value = "deductionTypeName", Label = "Nama" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "DeductionType.GetFilterMetadata",
                "Mengambil metadata filter deduction type.",
                result);

            return Ok(ApiResponse<DeductionTypeFilterMetadataResponse>.Ok(
                result,
                "Metadata filter deduction type berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<DeductionTypeSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Deduction Type", Description = "Melihat ringkasan deduction type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DeductionType", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();

            var result = new DeductionTypeSummaryResponse
            {
                TotalDeductionType = await query.CountAsync(),
                ActiveDeductionType = await query.CountAsync(x => x.IsActive),
                InactiveDeductionType = await query.CountAsync(x => !x.IsActive),
                RecurringDeductionType = await query.CountAsync(x => x.IsRecurring),
                StatutoryDeductionType = await query.CountAsync(x => x.IsStatutory),
                PreTaxDeductionType = await query.CountAsync(x => x.IsPreTax),
                ApprovalRequiredDeductionType = await query.CountAsync(x => x.RequiresApproval)
            };

            return Ok(ApiResponse<DeductionTypeSummaryResponse>.Ok(
                result,
                "Ringkasan deduction type berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<DeductionTypePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Deduction Type", Description = "Melihat data deduction type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DeductionType", "Read")]
        public async Task<IActionResult> GetDeductionTypes(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "deductionTypeName",
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
                    x.DeductionTypeCode.ToLower().Contains(keyword) ||
                    x.DeductionTypeName.ToLower().Contains(keyword) ||
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
                    ? query.OrderByDescending(x => x.DeductionTypeName)
                    : query.OrderBy(x => x.DeductionTypeName)
            };

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new DeductionTypeResponse
                {
                    Id = x.Id,
                    PayrollComponentId = x.PayrollComponentId,
                    PayrollComponentCode = x.PayrollComponent != null ? x.PayrollComponent.PayrollComponentCode : null,
                    PayrollComponentName = x.PayrollComponent != null ? x.PayrollComponent.PayrollComponentName : null,
                    DeductionTypeCode = x.DeductionTypeCode,
                    DeductionTypeName = x.DeductionTypeName,
                    DeductionCategory = x.DeductionCategory,
                    CalculationMethod = x.CalculationMethod,
                    CurrencyCode = x.CurrencyCode,
                    DefaultAmount = x.DefaultAmount,
                    DefaultPercentage = x.DefaultPercentage,
                    MaximumAmount = x.MaximumAmount,
                    IsRecurring = x.IsRecurring,
                    IsStatutory = x.IsStatutory,
                    IsPreTax = x.IsPreTax,
                    RequiresApproval = x.RequiresApproval,
                    AllowPartialDeduction = x.AllowPartialDeduction,
                    Priority = x.Priority,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    Description = x.Description,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty ? null : _dbContext.Users.Where(u => u.Id == x.CreateBy).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault()
                })
                .ToListAsync();

            var result = new DeductionTypePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<DeductionTypePagedResult>.Ok(
                result,
                "Data deduction type berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<DeductionTypeOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Deduction Type", Description = "Melihat pilihan deduction type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DeductionType", "Read")]
        public async Task<IActionResult> GetDeductionTypeOptions(
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
                    x.DeductionTypeCode.ToLower().Contains(keyword) ||
                    x.DeductionTypeName.ToLower().Contains(keyword));
            }

            var totalData = await query.CountAsync();

            var rows = await query
                .OrderBy(x => x.DeductionTypeName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = rows
                .Select(x => new DeductionTypeOptionResponse
                {
                    Id = x.Id,
                    DeductionTypeCode = x.DeductionTypeCode,
                    DeductionTypeName = x.DeductionTypeName,
                    DeductionCategory = x.DeductionCategory,
                    CalculationMethod = x.CalculationMethod,
                    PayrollComponentId = x.PayrollComponentId,
                    PayrollComponentCode = x.PayrollComponent != null ? x.PayrollComponent.PayrollComponentCode : null,
                    PayrollComponentName = x.PayrollComponent != null ? x.PayrollComponent.PayrollComponentName : null
                })
                .ToList();

            return Ok(ApiResponse<DeductionTypeOptionPagedResponse>.Ok(
                new DeductionTypeOptionPagedResponse
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data pilihan deduction type berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DeductionTypeDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Deduction Type", Description = "Melihat detail deduction type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DeductionType", "Read")]
        public async Task<IActionResult> GetDeductionTypeById(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new DeductionTypeDetailResponse
                {
                    Id = x.Id,
                    PayrollComponentId = x.PayrollComponentId,
                    PayrollComponentCode = x.PayrollComponent != null ? x.PayrollComponent.PayrollComponentCode : null,
                    PayrollComponentName = x.PayrollComponent != null ? x.PayrollComponent.PayrollComponentName : null,
                    DeductionTypeCode = x.DeductionTypeCode,
                    DeductionTypeName = x.DeductionTypeName,
                    DeductionCategory = x.DeductionCategory,
                    CalculationMethod = x.CalculationMethod,
                    CurrencyCode = x.CurrencyCode,
                    DefaultAmount = x.DefaultAmount,
                    DefaultPercentage = x.DefaultPercentage,
                    MaximumAmount = x.MaximumAmount,
                    IsRecurring = x.IsRecurring,
                    IsStatutory = x.IsStatutory,
                    IsPreTax = x.IsPreTax,
                    RequiresApproval = x.RequiresApproval,
                    AllowPartialDeduction = x.AllowPartialDeduction,
                    Priority = x.Priority,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    Description = x.Description,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
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
                    "Deduction Type tidak ditemukan."));
            }

            return Ok(ApiResponse<DeductionTypeDetailResponse>.Ok(
                data,
                "Detail deduction type berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DeductionTypeCreateResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create Deduction Type", Description = "Membuat data deduction type", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("DeductionType", "Create")]
        public async Task<IActionResult> CreateDeductionType(
            [FromBody] CreateDeductionTypeRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data deduction type tidak valid."));
            }

            var entity = new MstDeductionType
            {
                Id = Guid.NewGuid(),
                DeductionTypeCode = await GenerateCodeAsync(),
                PayrollComponentId = NormalizeGuid(request.PayrollComponentId),
                DeductionTypeName = request.DeductionTypeName.Trim(),
                DeductionCategory = NormalizeDeductionCategory(request.DeductionCategory),
                CalculationMethod = NormalizeCalculationMethod(request.CalculationMethod),
                CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant(),
                DefaultAmount = request.DefaultAmount,
                DefaultPercentage = request.DefaultPercentage,
                MaximumAmount = request.MaximumAmount,
                IsRecurring = request.IsRecurring,
                IsStatutory = request.IsStatutory,
                IsPreTax = request.IsPreTax,
                RequiresApproval = request.RequiresApproval,
                AllowPartialDeduction = request.AllowPartialDeduction,
                Priority = request.Priority,
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

            _dbContext.Set<MstDeductionType>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var result = new DeductionTypeCreateResponse
            {
                Id = entity.Id,
                DeductionTypeCode = entity.DeductionTypeCode,
                DeductionTypeName = entity.DeductionTypeName,
                IsActive = entity.IsActive
            };

            return Ok(ApiResponse<DeductionTypeCreateResponse>.Ok(
                result,
                "Deduction Type berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Deduction Type", Description = "Mengubah data deduction type", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DeductionType", "Update")]
        public async Task<IActionResult> UpdateDeductionType(
            Guid id,
            [FromBody] UpdateDeductionTypeRequest request)
        {
            var entity = await _dbContext.Set<MstDeductionType>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Deduction Type tidak ditemukan."));
            }

            var validation = await ValidateRequestAsync(id, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data deduction type tidak valid."));
            }

            entity.PayrollComponentId = NormalizeGuid(request.PayrollComponentId);
            entity.DeductionTypeName = request.DeductionTypeName.Trim();
            entity.DeductionCategory = NormalizeDeductionCategory(request.DeductionCategory);
            entity.CalculationMethod = NormalizeCalculationMethod(request.CalculationMethod);
            entity.CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant();
            entity.DefaultAmount = request.DefaultAmount;
            entity.DefaultPercentage = request.DefaultPercentage;
            entity.MaximumAmount = request.MaximumAmount;
            entity.IsRecurring = request.IsRecurring;
            entity.IsStatutory = request.IsStatutory;
            entity.IsPreTax = request.IsPreTax;
            entity.RequiresApproval = request.RequiresApproval;
            entity.AllowPartialDeduction = request.AllowPartialDeduction;
            entity.Priority = request.Priority;
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
                "Deduction Type berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Deduction Type Status", Description = "Mengubah status deduction type", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("DeductionType", "Update")]
        public async Task<IActionResult> UpdateDeductionTypeStatus(
            Guid id,
            [FromBody] UpdateDeductionTypeStatusRequest request)
        {
            var entity = await _dbContext.Set<MstDeductionType>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Deduction Type tidak ditemukan."));
            }

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status deduction type berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Delete", "Delete Deduction Type", Description = "Menghapus deduction type", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("DeductionType", "Delete")]
        public async Task<IActionResult> DeleteDeductionType(Guid id)
        {
            var entity = await _dbContext.Set<MstDeductionType>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Deduction Type tidak ditemukan."));
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
                "Deduction Type berhasil dihapus."));
        }

        private IQueryable<MstDeductionType> BuildBaseQuery()
        {
            return _dbContext.Set<MstDeductionType>()
                .AsNoTracking()
                .Include(x => x.PayrollComponent)
                .Where(x => !x.IsDelete);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateDeductionTypeRequest request)
        {

            if (string.IsNullOrWhiteSpace(request.DeductionTypeName))
                return (false, "Nama deduction type wajib diisi.");

            if (!AllowedDeductionCategories.Contains(request.DeductionCategory.Trim()))
                return (false, "Deduction category tidak valid.");

            if (!AllowedCalculationMethods.Contains(request.CalculationMethod.Trim()))
                return (false, "Calculation method tidak valid.");

            if (string.IsNullOrWhiteSpace(request.CurrencyCode) || request.CurrencyCode.Trim().Length != 3)
                return (false, "Currency code harus terdiri dari tiga karakter.");

            if (request.CalculationMethod.Equals("Percentage", StringComparison.OrdinalIgnoreCase) &&
                request.DefaultPercentage <= 0)
                return (false, "Default percentage harus lebih besar dari 0.");

            if (request.MaximumAmount.HasValue && request.DefaultAmount > request.MaximumAmount.Value)
                return (false, "Default amount tidak boleh lebih besar dari maximum amount.");

            if (request.EffectiveStartDate.HasValue &&
                request.EffectiveEndDate.HasValue &&
                request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Value.Date)
                return (false, "EffectiveEndDate tidak boleh lebih kecil dari EffectiveStartDate.");

            var payrollComponentId = NormalizeGuid(request.PayrollComponentId);

            if (payrollComponentId.HasValue)
            {
                var component = await _dbContext.Set<MstPayrollComponent>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == payrollComponentId.Value &&
                        x.IsActive &&
                        !x.IsDelete);

                if (component == null)
                    return (false, "Payroll component tidak ditemukan atau tidak aktif.");

                if (!component.ComponentType.Equals("Deduction", StringComparison.OrdinalIgnoreCase))
                    return (false, "Payroll component untuk deduction harus bertipe Deduction.");
            }

            var normalizedName = request.DeductionTypeName.Trim().ToLower();

            var duplicateQuery = _dbContext.Set<MstDeductionType>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.DeductionTypeName.ToLower() == normalizedName &&
                    x.PayrollComponentId == payrollComponentId);

            if (excludeId.HasValue)
                duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateQuery.AnyAsync())
                return (false, "Deduction type dengan nama dan payroll component tersebut sudah digunakan.");

            return (true, null);
        }

        private async Task<string> GenerateCodeAsync()
        {
            var codes = await _dbContext.Set<MstDeductionType>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.DeductionTypeCode.StartsWith(CodePrefix))
                .Select(x => x.DeductionTypeCode)
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


        private static string NormalizeDeductionCategory(string value)
        {
            return AllowedDeductionCategories.First(x =>
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

        private static List<DeductionTypeCustomPeriodOptionResponse> BuildPeriodOptions()
        {
            return new List<DeductionTypeCustomPeriodOptionResponse>
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "lastmonth", Label = "Bulan lalu" }
            };
        }
    }
}
