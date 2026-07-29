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

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/allowance-types")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Allowance Type",
        AreaName = "Corporate",
        ControllerName = "AllowanceType",
        Description = "Corporate human resource master data allowance type",
        SortOrder = 42)]
    [Tags("Corporate / Human Resource / Master Data / Allowance Type")]
    public class AllowanceTypeController : ControllerBase
    {
        private static readonly HashSet<string> AllowedCategories =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "Fixed",
                "Variable",
                "Shift",
                "OnCall",
                "Hazard",
                "Transport",
                "Meal",
                "Communication",
                "Other"
            };

        private static readonly HashSet<string> AllowedCalculationMethods =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "Fixed",
                "Percentage",
                "PolicyBased",
                "ManualInput"
            };

        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "ALT-RSMMC-";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public AllowanceTypeController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<AllowanceTypeFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Allowance Type", Description = "Melihat metadata filter allowance type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AllowanceType", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new AllowanceTypeFilterMetadataResponse
            {
                DefaultFilter = new AllowanceTypeDefaultFilterResponse(),
                CustomPeriods = BuildPeriodOptions(),
                AllowanceCategoryOptions = AllowedCategories
                    .OrderBy(x => x)
                    .Select(x => new AllowanceTypeStringOptionResponse
                    {
                        Value = x,
                        Label = BuildCategoryLabel(x)
                    })
                    .ToList(),
                CalculationMethodOptions = AllowedCalculationMethods
                    .OrderBy(x => x)
                    .Select(x => new AllowanceTypeStringOptionResponse
                    {
                        Value = x,
                        Label = BuildCalculationMethodLabel(x)
                    })
                    .ToList(),
                SortOptions = new List<AllowanceTypeSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "allowanceTypeCode", Label = "Kode allowance" },
                    new() { Value = "allowanceTypeName", Label = "Nama allowance" },
                    new() { Value = "allowanceCategory", Label = "Kategori" },
                    new() { Value = "defaultAmount", Label = "Nominal default" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            return Ok(ApiResponse<AllowanceTypeFilterMetadataResponse>.Ok(
                result,
                "Metadata filter allowance type berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Allowance Type", Description = "Melihat ringkasan allowance type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AllowanceType", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
        {
            var query = BuildBaseQuery();

            var result = new AllowanceTypeSummaryResponse
            {
                TotalAllowanceType = await query.CountAsync(cancellationToken),
                ActiveAllowanceType = await query.CountAsync(x => x.IsActive, cancellationToken),
                InactiveAllowanceType = await query.CountAsync(x => !x.IsActive, cancellationToken),
                RecurringAllowanceType = await query.CountAsync(x => x.IsRecurring, cancellationToken),
                TaxableAllowanceType = await query.CountAsync(x => x.IsTaxable, cancellationToken),
                AttendanceRequiredAllowanceType = await query.CountAsync(x => x.RequiresAttendance, cancellationToken),
                ApprovalRequiredAllowanceType = await query.CountAsync(x => x.RequiresApproval, cancellationToken)
            };

            return Ok(ApiResponse<AllowanceTypeSummaryResponse>.Ok(
                result,
                "Ringkasan allowance type berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Allowance Type", Description = "Melihat data allowance type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AllowanceType", "Read")]
        public async Task<IActionResult> GetAllowanceTypes(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? payrollComponentId,
            [FromQuery] string? allowanceCategory,
            [FromQuery] string? calculationMethod,
            [FromQuery] bool? isRecurring,
            [FromQuery] bool? isTaxable,
            [FromQuery] bool? requiresAttendance,
            [FromQuery] bool? requiresApproval,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "allowanceTypeName",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyDateFilter(BuildBaseQuery(), startDate, endDate, customPeriod);
            query = ApplyFilter(
                query,
                payrollComponentId,
                allowanceCategory,
                calculationMethod,
                isRecurring,
                isTaxable,
                requiresAttendance,
                requiresApproval,
                isActive,
                search);

            var totalData = await query.CountAsync(cancellationToken);
            var rows = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var items = rows.Select(MapResponse).ToList();

            return Ok(ApiResponse<PagedResult<AllowanceTypeResponse>>.Ok(
                new PagedResult<AllowanceTypeResponse>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data allowance type berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Allowance Type", Description = "Melihat pilihan allowance type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AllowanceType", "Read")]
        public async Task<IActionResult> GetAllowanceTypeOptions(
            [FromQuery] Guid? payrollComponentId,
            [FromQuery] string? allowanceCategory,
            [FromQuery] string? calculationMethod,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyFilter(
                BuildBaseQuery(),
                payrollComponentId,
                allowanceCategory,
                calculationMethod,
                null,
                null,
                null,
                null,
                onlyActive ? true : null,
                search);

            var totalData = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(x => x.AllowanceTypeName)
                .ThenBy(x => x.AllowanceTypeCode)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new AllowanceTypeOptionResponse
                {
                    Id = x.Id,
                    PayrollComponentId = x.PayrollComponentId,
                    PayrollComponentName = x.PayrollComponent != null
                        ? x.PayrollComponent.PayrollComponentName
                        : null,
                    AllowanceTypeCode = x.AllowanceTypeCode,
                    AllowanceTypeName = x.AllowanceTypeName,
                    AllowanceCategory = x.AllowanceCategory,
                    CalculationMethod = x.CalculationMethod,
                    CurrencyCode = x.CurrencyCode,
                    DefaultAmount = x.DefaultAmount,
                    DefaultPercentage = x.DefaultPercentage
                })
                .ToListAsync(cancellationToken);

            return Ok(ApiResponse<AllowanceTypeOptionPagedResponse>.Ok(
                new AllowanceTypeOptionPagedResponse
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data pilihan allowance type berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Allowance Type", Description = "Melihat detail allowance type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AllowanceType", "Read")]
        public async Task<IActionResult> GetAllowanceTypeById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Allowance type tidak ditemukan."));

            var result = new AllowanceTypeDetailResponse
            {
                Id = entity.Id,
                PayrollComponentId = entity.PayrollComponentId,
                PayrollComponentCode = entity.PayrollComponent?.PayrollComponentCode,
                PayrollComponentName = entity.PayrollComponent?.PayrollComponentName,
                AllowanceTypeCode = entity.AllowanceTypeCode,
                AllowanceTypeName = entity.AllowanceTypeName,
                AllowanceCategory = entity.AllowanceCategory,
                CalculationMethod = entity.CalculationMethod,
                CurrencyCode = entity.CurrencyCode,
                DefaultAmount = entity.DefaultAmount,
                DefaultPercentage = entity.DefaultPercentage,
                MaximumAmount = entity.MaximumAmount,
                IsRecurring = entity.IsRecurring,
                IsTaxable = entity.IsTaxable,
                IsProrated = entity.IsProrated,
                RequiresAttendance = entity.RequiresAttendance,
                RequiresApproval = entity.RequiresApproval,
                IsIncludedInBaseSalary = entity.IsIncludedInBaseSalary,
                EffectiveStartDate = entity.EffectiveStartDate,
                EffectiveEndDate = entity.EffectiveEndDate,
                Description = entity.Description,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                ShiftAllowancePolicyCount = entity.ShiftAllowancePolicies.Count(x => !x.IsDelete),
                OnCallAllowancePolicyCount = entity.OnCallAllowancePolicies.Count(x => !x.IsDelete),
                HazardAllowancePolicyCount = entity.HazardAllowancePolicies.Count(x => !x.IsDelete),
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                CreateByName = GetUserName(entity.CreateBy),
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetUserName(entity.UpdateBy)
            };

            return Ok(ApiResponse<AllowanceTypeDetailResponse>.Ok(
                result,
                "Detail allowance type berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Allowance Type", Description = "Membuat data allowance type", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("AllowanceType", "Create")]
        public async Task<IActionResult> CreateAllowanceType(
            [FromBody] CreateAllowanceTypeRequest request,
            CancellationToken cancellationToken)
        {
            var validation = await ValidateRequestAsync(null, request, cancellationToken);

            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage ?? "Data allowance type tidak valid."));

            var entity = new MstAllowanceType
            {
                Id = Guid.NewGuid(),
                PayrollComponentId = NormalizeGuid(request.PayrollComponentId),
                AllowanceTypeCode = await GenerateCodeAsync(cancellationToken),
                AllowanceTypeName = request.AllowanceTypeName.Trim(),
                AllowanceCategory = NormalizeCategory(request.AllowanceCategory),
                CalculationMethod = NormalizeCalculationMethod(request.CalculationMethod),
                CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant(),
                DefaultAmount = request.DefaultAmount,
                DefaultPercentage = request.DefaultPercentage,
                MaximumAmount = request.MaximumAmount,
                IsRecurring = request.IsRecurring,
                IsTaxable = request.IsTaxable,
                IsProrated = request.IsProrated,
                RequiresAttendance = request.RequiresAttendance,
                RequiresApproval = request.RequiresApproval,
                IsIncludedInBaseSalary = request.IsIncludedInBaseSalary,
                EffectiveStartDate = request.EffectiveStartDate?.Date,
                EffectiveEndDate = request.EffectiveEndDate?.Date,
                Description = NormalizeNullableText(request.Description),
                SortOrder = request.SortOrder,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstAllowanceType>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<AllowanceTypeCreateResponse>.Ok(
                new AllowanceTypeCreateResponse
                {
                    Id = entity.Id,
                    AllowanceTypeCode = entity.AllowanceTypeCode,
                    AllowanceTypeName = entity.AllowanceTypeName,
                    IsActive = entity.IsActive
                },
                "Allowance type berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Allowance Type", Description = "Mengubah data allowance type", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("AllowanceType", "Update")]
        public async Task<IActionResult> UpdateAllowanceType(
            Guid id,
            [FromBody] UpdateAllowanceTypeRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<MstAllowanceType>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Allowance type tidak ditemukan."));

            var validation = await ValidateRequestAsync(id, request, cancellationToken);

            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage ?? "Data allowance type tidak valid."));

            entity.PayrollComponentId = NormalizeGuid(request.PayrollComponentId);
            entity.AllowanceTypeName = request.AllowanceTypeName.Trim();
            entity.AllowanceCategory = NormalizeCategory(request.AllowanceCategory);
            entity.CalculationMethod = NormalizeCalculationMethod(request.CalculationMethod);
            entity.CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant();
            entity.DefaultAmount = request.DefaultAmount;
            entity.DefaultPercentage = request.DefaultPercentage;
            entity.MaximumAmount = request.MaximumAmount;
            entity.IsRecurring = request.IsRecurring;
            entity.IsTaxable = request.IsTaxable;
            entity.IsProrated = request.IsProrated;
            entity.RequiresAttendance = request.RequiresAttendance;
            entity.RequiresApproval = request.RequiresApproval;
            entity.IsIncludedInBaseSalary = request.IsIncludedInBaseSalary;
            entity.EffectiveStartDate = request.EffectiveStartDate?.Date;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.Description = NormalizeNullableText(request.Description);
            entity.SortOrder = request.SortOrder;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(null, "Allowance type berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Allowance Type Status", Description = "Mengubah status allowance type", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("AllowanceType", "Update")]
        public async Task<IActionResult> UpdateAllowanceTypeStatus(
            Guid id,
            [FromBody] UpdateAllowanceTypeStatusRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<MstAllowanceType>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Allowance type tidak ditemukan."));

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(null, "Status allowance type berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Allowance Type", Description = "Menghapus allowance type", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("AllowanceType", "Delete")]
        public async Task<IActionResult> DeleteAllowanceType(
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<MstAllowanceType>()
                .Include(x => x.ShiftAllowancePolicies)
                .Include(x => x.OnCallAllowancePolicies)
                .Include(x => x.HazardAllowancePolicies)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Allowance type tidak ditemukan."));

            var isUsed =
                entity.ShiftAllowancePolicies.Any(x => !x.IsDelete) ||
                entity.OnCallAllowancePolicies.Any(x => !x.IsDelete) ||
                entity.HazardAllowancePolicies.Any(x => !x.IsDelete);

            if (isUsed)
                return BadRequest(ApiResponse<object>.Fail(400, "Allowance type tidak dapat dihapus karena sudah digunakan policy allowance."));

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(null, "Allowance type berhasil dihapus."));
        }

        private IQueryable<MstAllowanceType> BuildBaseQuery()
        {
            return _dbContext.Set<MstAllowanceType>()
                .AsNoTracking()
                .Include(x => x.PayrollComponent)
                .Include(x => x.ShiftAllowancePolicies)
                .Include(x => x.OnCallAllowancePolicies)
                .Include(x => x.HazardAllowancePolicies)
                .Where(x => !x.IsDelete);
        }

        private AllowanceTypeResponse MapResponse(MstAllowanceType x)
        {
            return new AllowanceTypeResponse
            {
                Id = x.Id,
                PayrollComponentId = x.PayrollComponentId,
                PayrollComponentCode = x.PayrollComponent?.PayrollComponentCode,
                PayrollComponentName = x.PayrollComponent?.PayrollComponentName,
                AllowanceTypeCode = x.AllowanceTypeCode,
                AllowanceTypeName = x.AllowanceTypeName,
                AllowanceCategory = x.AllowanceCategory,
                CalculationMethod = x.CalculationMethod,
                CurrencyCode = x.CurrencyCode,
                DefaultAmount = x.DefaultAmount,
                DefaultPercentage = x.DefaultPercentage,
                MaximumAmount = x.MaximumAmount,
                IsRecurring = x.IsRecurring,
                IsTaxable = x.IsTaxable,
                IsProrated = x.IsProrated,
                RequiresAttendance = x.RequiresAttendance,
                RequiresApproval = x.RequiresApproval,
                IsIncludedInBaseSalary = x.IsIncludedInBaseSalary,
                EffectiveStartDate = x.EffectiveStartDate,
                EffectiveEndDate = x.EffectiveEndDate,
                Description = x.Description,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                ShiftAllowancePolicyCount = x.ShiftAllowancePolicies.Count(p => !p.IsDelete),
                OnCallAllowancePolicyCount = x.OnCallAllowancePolicies.Count(p => !p.IsDelete),
                HazardAllowancePolicyCount = x.HazardAllowancePolicies.Count(p => !p.IsDelete),
                CreateDateTime = x.CreateDateTime,
                CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                CreateByName = GetUserName(x.CreateBy)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateAllowanceTypeRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.AllowanceTypeName))
                return (false, "Nama allowance type wajib diisi.");

            if (!AllowedCategories.Contains(request.AllowanceCategory.Trim()))
                return (false, "Allowance category tidak valid.");

            if (!AllowedCalculationMethods.Contains(request.CalculationMethod.Trim()))
                return (false, "Calculation method tidak valid.");

            if (string.IsNullOrWhiteSpace(request.CurrencyCode) || request.CurrencyCode.Trim().Length != 3)
                return (false, "Currency code harus terdiri dari tiga karakter.");

            if (request.CalculationMethod.Equals("Percentage", StringComparison.OrdinalIgnoreCase) &&
                request.DefaultPercentage <= 0)
            {
                return (false, "Default percentage harus lebih besar dari 0 untuk calculation method Percentage.");
            }

            if (request.MaximumAmount.HasValue && request.DefaultAmount > request.MaximumAmount.Value)
                return (false, "Default amount tidak boleh lebih besar dari maximum amount.");

            if (request.EffectiveStartDate.HasValue &&
                request.EffectiveEndDate.HasValue &&
                request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Value.Date)
            {
                return (false, "EffectiveEndDate tidak boleh lebih kecil dari EffectiveStartDate.");
            }

            var payrollComponentId = NormalizeGuid(request.PayrollComponentId);

            if (payrollComponentId.HasValue)
            {
                var component = await _dbContext.Set<MstPayrollComponent>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == payrollComponentId.Value &&
                        x.IsActive &&
                        !x.IsDelete,
                        cancellationToken);

                if (component == null)
                    return (false, "Payroll component tidak ditemukan atau tidak aktif.");

                if (!component.ComponentType.Equals("Earning", StringComparison.OrdinalIgnoreCase) &&
                    !component.ComponentType.Equals("EmployerContribution", StringComparison.OrdinalIgnoreCase))
                {
                    return (false, "Payroll component untuk allowance harus bertipe Earning atau EmployerContribution.");
                }
            }

            var normalizedName = request.AllowanceTypeName.Trim().ToLower();
            var duplicateQuery = _dbContext.Set<MstAllowanceType>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.AllowanceTypeName.ToLower() == normalizedName &&
                    x.PayrollComponentId == payrollComponentId);

            if (excludeId.HasValue)
                duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateQuery.AnyAsync(cancellationToken))
                return (false, "Allowance type dengan nama dan payroll component tersebut sudah digunakan.");

            return (true, null);
        }

        private static IQueryable<MstAllowanceType> ApplyFilter(
            IQueryable<MstAllowanceType> query,
            Guid? payrollComponentId,
            string? allowanceCategory,
            string? calculationMethod,
            bool? isRecurring,
            bool? isTaxable,
            bool? requiresAttendance,
            bool? requiresApproval,
            bool? isActive,
            string? search)
        {
            if (payrollComponentId.HasValue && payrollComponentId.Value != Guid.Empty)
                query = query.Where(x => x.PayrollComponentId == payrollComponentId.Value);

            if (!string.IsNullOrWhiteSpace(allowanceCategory))
                query = query.Where(x => x.AllowanceCategory == allowanceCategory.Trim());

            if (!string.IsNullOrWhiteSpace(calculationMethod))
                query = query.Where(x => x.CalculationMethod == calculationMethod.Trim());

            if (isRecurring.HasValue)
                query = query.Where(x => x.IsRecurring == isRecurring.Value);

            if (isTaxable.HasValue)
                query = query.Where(x => x.IsTaxable == isTaxable.Value);

            if (requiresAttendance.HasValue)
                query = query.Where(x => x.RequiresAttendance == requiresAttendance.Value);

            if (requiresApproval.HasValue)
                query = query.Where(x => x.RequiresApproval == requiresApproval.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.AllowanceTypeCode.ToLower().Contains(keyword) ||
                    x.AllowanceTypeName.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.PayrollComponent != null && x.PayrollComponent.PayrollComponentName.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<MstAllowanceType> ApplySorting(
            IQueryable<MstAllowanceType> query,
            string? sortBy,
            string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "allowanceTypeName").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "allowancetypecode" => desc ? query.OrderByDescending(x => x.AllowanceTypeCode) : query.OrderBy(x => x.AllowanceTypeCode),
                "allowancecategory" => desc ? query.OrderByDescending(x => x.AllowanceCategory) : query.OrderBy(x => x.AllowanceCategory),
                "defaultamount" => desc ? query.OrderByDescending(x => x.DefaultAmount) : query.OrderBy(x => x.DefaultAmount),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => desc ? query.OrderByDescending(x => x.AllowanceTypeName) : query.OrderBy(x => x.AllowanceTypeName)
            };
        }

        private async Task<string> GenerateCodeAsync(CancellationToken cancellationToken)
        {
            var codes = await _dbContext.Set<MstAllowanceType>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.AllowanceTypeCode.StartsWith(CodePrefix))
                .Select(x => x.AllowanceTypeCode)
                .ToListAsync(cancellationToken);

            return GenerateNextCode(codes, CodePrefix, 5);
        }

        private string? GetUserName(Guid userId)
        {
            if (userId == Guid.Empty) return null;

            return _dbContext.Users
                .Where(x => x.Id == userId)
                .Select(x => x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode)
                .FirstOrDefault();
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }

        private static Guid? NormalizeGuid(Guid? value)
        {
            return !value.HasValue || value.Value == Guid.Empty ? null : value.Value;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string NormalizeCategory(string value)
        {
            return AllowedCategories.First(x => x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static string NormalizeCalculationMethod(string value)
        {
            return AllowedCalculationMethods.First(x => x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static string BuildCategoryLabel(string value)
        {
            return value switch
            {
                "Fixed" => "Tetap",
                "Variable" => "Variabel",
                "Shift" => "Shift",
                "OnCall" => "On Call",
                "Hazard" => "Risiko",
                "Transport" => "Transportasi",
                "Meal" => "Makan",
                "Communication" => "Komunikasi",
                "Other" => "Lainnya",
                _ => value
            };
        }

        private static string BuildCalculationMethodLabel(string value)
        {
            return value switch
            {
                "Fixed" => "Nominal Tetap",
                "Percentage" => "Persentase",
                "PolicyBased" => "Berdasarkan Kebijakan",
                "ManualInput" => "Input Manual",
                _ => value
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            return (pageNumber < 1 ? 1 : pageNumber, pageSize < 1 ? 25 : Math.Min(pageSize, 100));
        }

        private static string GenerateNextCode(IEnumerable<string> codes, string prefix, int length)
        {
            var used = codes
                .Select(x => x.Replace(prefix, string.Empty))
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .ToHashSet();

            var next = 1;
            while (used.Contains(next)) next++;
            return prefix + next.ToString().PadLeft(length, '0');
        }

        private static IQueryable<MstAllowanceType> ApplyDateFilter(
            IQueryable<MstAllowanceType> query,
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            var range = ResolveDateRange(startDate, endDate, customPeriod);
            if (range.Start.HasValue) query = query.Where(x => x.CreateDateTime >= range.Start.Value);
            if (range.EndExclusive.HasValue) query = query.Where(x => x.CreateDateTime < range.EndExclusive.Value);
            return query;
        }

        private static (DateTime? Start, DateTime? EndExclusive) ResolveDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            if (startDate.HasValue || endDate.HasValue)
            {
                return (
                    startDate.HasValue ? DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc) : null,
                    endDate.HasValue ? DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc) : null);
            }

            var today = DateTime.UtcNow.Date;
            return customPeriod?.Trim().ToLowerInvariant() switch
            {
                "today" => (today, today.AddDays(1)),
                "last7days" => (today.AddDays(-6), today.AddDays(1)),
                "thismonth" => (new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1)),
                "lastmonth" => (new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1), new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc)),
                _ => (null, null)
            };
        }

        private static List<AllowanceTypeCustomPeriodOptionResponse> BuildPeriodOptions()
        {
            return new List<AllowanceTypeCustomPeriodOptionResponse>
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "lastmonth", Label = "Bulan lalu" }
            };
        }
    }
}
