using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/payroll-management/transport-allowance-policies")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_PAYROLL_MANAGEMENT",
        moduleName: "Human Resource Payroll Management",
        displayName: "Transport Allowance Policy",
        AreaName = "Corporate",
        ControllerName = "TransportAllowancePolicy",
        Description = "Corporate human resource transport allowance policy",
        SortOrder = 10)]
    [Tags("Corporate / Human Resource / Payroll Management / Transport Allowance Policy")]
    public class WfpTransportAllowancePolicyController : ControllerBase
    {
        private static readonly HashSet<string> AllowedCalculationMethods =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "FixedMonthly",
                "PerAttendance",
                "PerDay",
                "Reimbursement"
            };

        private const string CodePrefix = "TAP-RSMMC-";
        private const int CodeNumberLength = 5;
        private const string LogCategory = "Corporate.HumanResource.PayrollManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WfpTransportAllowancePolicyController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Transport Allowance Policy", Description = "Melihat metadata filter transport allowance policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("TransportAllowancePolicy", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new WfpTransportAllowancePolicyFilterMetadataResponse
            {
                DefaultFilter = new WfpTransportAllowancePolicyDefaultFilterResponse(),
                CalculationMethodOptions = AllowedCalculationMethods
                    .OrderBy(x => x)
                    .Select(x => new WfpTransportAllowancePolicyStringOptionResponse
                    {
                        Value = x,
                        Label = BuildCalculationMethodLabel(x)
                    })
                    .ToList(),
                SortOptions = new List<WfpTransportAllowancePolicySortOptionResponse>
                {
                    new() { Value = "policyCode", Label = "Kode policy" },
                    new() { Value = "policyName", Label = "Nama policy" },
                    new() { Value = "calculationMethod", Label = "Metode perhitungan" },
                    new() { Value = "effectiveStartDate", Label = "Tanggal mulai berlaku" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            return Ok(ApiResponse<WfpTransportAllowancePolicyFilterMetadataResponse>.Ok(
                result,
                "Metadata filter transport allowance policy berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Transport Allowance Policy", Description = "Melihat ringkasan transport allowance policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("TransportAllowancePolicy", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
        {
            var query = BuildBaseQuery();

            var result = new WfpTransportAllowancePolicySummaryResponse
            {
                TotalData = await query.CountAsync(cancellationToken),
                ActiveData = await query.CountAsync(x => x.IsActive, cancellationToken),
                InactiveData = await query.CountAsync(x => !x.IsActive, cancellationToken),
                AttendanceBasedData = await query.CountAsync(x => x.IsAttendanceBased, cancellationToken),
                PayrollIncludedData = await query.CountAsync(x => x.IsIncludedInPayroll, cancellationToken),
                TaxableData = await query.CountAsync(x => x.IsTaxable, cancellationToken)
            };

            return Ok(ApiResponse<WfpTransportAllowancePolicySummaryResponse>.Ok(
                result,
                "Ringkasan transport allowance policy berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Transport Allowance Policy", Description = "Melihat data transport allowance policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("TransportAllowancePolicy", "Read")]
        public async Task<IActionResult> GetPolicies(
            [FromQuery] Guid? legalEntityId,
            [FromQuery] Guid? hospitalSiteId,
            [FromQuery] Guid? employeeGradeId,
            [FromQuery] Guid? payrollComponentId,
            [FromQuery] string? calculationMethod,
            [FromQuery] bool? isAttendanceBased,
            [FromQuery] bool? isIncludedInPayroll,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "policyName",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyFilter(
                BuildBaseQuery(),
                legalEntityId,
                hospitalSiteId,
                employeeGradeId,
                payrollComponentId,
                calculationMethod,
                isAttendanceBased,
                isIncludedInPayroll,
                isActive,
                search);

            var totalData = await query.CountAsync(cancellationToken);

            var rows = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var items = rows.Select(MapResponse).ToList();

            var result = new PagedResult<WfpTransportAllowancePolicyResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<PagedResult<WfpTransportAllowancePolicyResponse>>.Ok(
                result,
                "Data transport allowance policy berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Transport Allowance Policy", Description = "Melihat pilihan transport allowance policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("TransportAllowancePolicy", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] Guid? legalEntityId,
            [FromQuery] Guid? hospitalSiteId,
            [FromQuery] Guid? employeeGradeId,
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
                legalEntityId,
                hospitalSiteId,
                employeeGradeId,
                null,
                null,
                null,
                null,
                onlyActive ? true : null,
                search);

            var totalData = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(x => x.PolicyName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new WfpTransportAllowancePolicyOptionResponse
                {
                    Id = x.Id,
                    PolicyCode = x.PolicyCode,
                    PolicyName = x.PolicyName,
                    CalculationMethod = x.CalculationMethod,
                    FixedMonthlyAmount = x.FixedMonthlyAmount,
                    PerAttendanceAmount = x.PerAttendanceAmount
                })
                .ToListAsync(cancellationToken);

            var result = new WfpTransportAllowancePolicyOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<WfpTransportAllowancePolicyOptionPagedResponse>.Ok(
                result,
                "Pilihan transport allowance policy berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Transport Allowance Policy", Description = "Melihat detail transport allowance policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("TransportAllowancePolicy", "Read")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Transport allowance policy tidak ditemukan."));
            }

            var result = MapDetailResponse(entity);

            return Ok(ApiResponse<WfpTransportAllowancePolicyDetailResponse>.Ok(
                result,
                "Detail transport allowance policy berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Transport Allowance Policy", Description = "Membuat transport allowance policy", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("TransportAllowancePolicy", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateWfpTransportAllowancePolicyRequest request,
            CancellationToken cancellationToken)
        {
            var validation = await ValidateRequestAsync(null, request, cancellationToken);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data transport allowance policy tidak valid."));
            }

            var entity = new WfpTransportAllowancePolicy
            {
                Id = Guid.NewGuid(),
                LegalEntityId = NormalizeGuid(request.LegalEntityId),
                HospitalSiteId = NormalizeGuid(request.HospitalSiteId),
                EmployeeGradeId = NormalizeGuid(request.EmployeeGradeId),
                PayrollComponentId = NormalizeGuid(request.PayrollComponentId),
                PolicyCode = await GenerateCodeAsync(cancellationToken),
                PolicyName = request.PolicyName.Trim(),
                CalculationMethod = NormalizeCalculationMethod(request.CalculationMethod),
                FixedMonthlyAmount = request.FixedMonthlyAmount,
                PerAttendanceAmount = request.PerAttendanceAmount,
                DailyLimitAmount = request.DailyLimitAmount,
                MonthlyLimitAmount = request.MonthlyLimitAmount,
                MinimumAttendanceMinutes = request.MinimumAttendanceMinutes,
                IsAttendanceBased = request.IsAttendanceBased,
                IsProrated = request.IsProrated,
                IsTaxable = request.IsTaxable,
                IsIncludedInPayroll = request.IsIncludedInPayroll,
                IncludeBusinessTravelDay = request.IncludeBusinessTravelDay,
                IncludePaidLeaveDay = request.IncludePaidLeaveDay,
                IncludeHoliday = request.IncludeHoliday,
                EffectiveStartDate = request.EffectiveStartDate,
                EffectiveEndDate = request.EffectiveEndDate,
                Description = NormalizeText(request.Description),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<WfpTransportAllowancePolicy>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "TransportAllowancePolicy.Create",
                "Membuat transport allowance policy.",
                new { entity.Id, entity.PolicyCode });

            return await GetById(entity.Id, cancellationToken);
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Transport Allowance Policy", Description = "Mengubah transport allowance policy", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("TransportAllowancePolicy", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateWfpTransportAllowancePolicyRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpTransportAllowancePolicy>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Transport allowance policy tidak ditemukan."));
            }

            var validation = await ValidateRequestAsync(id, request, cancellationToken);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data transport allowance policy tidak valid."));
            }

            entity.LegalEntityId = NormalizeGuid(request.LegalEntityId);
            entity.HospitalSiteId = NormalizeGuid(request.HospitalSiteId);
            entity.EmployeeGradeId = NormalizeGuid(request.EmployeeGradeId);
            entity.PayrollComponentId = NormalizeGuid(request.PayrollComponentId);
            entity.PolicyName = request.PolicyName.Trim();
            entity.CalculationMethod = NormalizeCalculationMethod(request.CalculationMethod);
            entity.FixedMonthlyAmount = request.FixedMonthlyAmount;
            entity.PerAttendanceAmount = request.PerAttendanceAmount;
            entity.DailyLimitAmount = request.DailyLimitAmount;
            entity.MonthlyLimitAmount = request.MonthlyLimitAmount;
            entity.MinimumAttendanceMinutes = request.MinimumAttendanceMinutes;
            entity.IsAttendanceBased = request.IsAttendanceBased;
            entity.IsProrated = request.IsProrated;
            entity.IsTaxable = request.IsTaxable;
            entity.IsIncludedInPayroll = request.IsIncludedInPayroll;
            entity.IncludeBusinessTravelDay = request.IncludeBusinessTravelDay;
            entity.IncludePaidLeaveDay = request.IncludePaidLeaveDay;
            entity.IncludeHoliday = request.IncludeHoliday;
            entity.EffectiveStartDate = request.EffectiveStartDate;
            entity.EffectiveEndDate = request.EffectiveEndDate;
            entity.Description = NormalizeText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetById(entity.Id, cancellationToken);
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Transport Allowance Policy Status", Description = "Mengubah status transport allowance policy", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("TransportAllowancePolicy", "Update")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] UpdateWfpTransportAllowancePolicyStatusRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpTransportAllowancePolicy>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Transport allowance policy tidak ditemukan."));
            }

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status transport allowance policy berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Transport Allowance Policy", Description = "Menghapus transport allowance policy", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("TransportAllowancePolicy", "Delete")]
        public async Task<IActionResult> Delete(
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpTransportAllowancePolicy>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Transport allowance policy tidak ditemukan."));
            }

            if (await _dbContext.Set<WfpTransportAllowance>()
                    .AnyAsync(x => x.TransportAllowancePolicyId == id && !x.IsDelete, cancellationToken))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Policy tidak dapat dihapus karena sudah digunakan transport allowance workforce."));
            }

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(
                null,
                "Transport allowance policy berhasil dihapus."));
        }

        private IQueryable<WfpTransportAllowancePolicy> BuildBaseQuery()
        {
            return _dbContext.Set<WfpTransportAllowancePolicy>()
                .AsNoTracking()
                .Include(x => x.PayrollComponent)
                .Include(x => x.TransportAllowances)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<WfpTransportAllowancePolicy> ApplyFilter(
            IQueryable<WfpTransportAllowancePolicy> query,
            Guid? legalEntityId,
            Guid? hospitalSiteId,
            Guid? employeeGradeId,
            Guid? payrollComponentId,
            string? calculationMethod,
            bool? isAttendanceBased,
            bool? isIncludedInPayroll,
            bool? isActive,
            string? search)
        {
            if (legalEntityId.HasValue && legalEntityId.Value != Guid.Empty)
                query = query.Where(x => x.LegalEntityId == legalEntityId.Value);

            if (hospitalSiteId.HasValue && hospitalSiteId.Value != Guid.Empty)
                query = query.Where(x => x.HospitalSiteId == hospitalSiteId.Value);

            if (employeeGradeId.HasValue && employeeGradeId.Value != Guid.Empty)
                query = query.Where(x => x.EmployeeGradeId == employeeGradeId.Value);

            if (payrollComponentId.HasValue && payrollComponentId.Value != Guid.Empty)
                query = query.Where(x => x.PayrollComponentId == payrollComponentId.Value);

            if (!string.IsNullOrWhiteSpace(calculationMethod))
                query = query.Where(x => x.CalculationMethod == calculationMethod.Trim());

            if (isAttendanceBased.HasValue)
                query = query.Where(x => x.IsAttendanceBased == isAttendanceBased.Value);

            if (isIncludedInPayroll.HasValue)
                query = query.Where(x => x.IsIncludedInPayroll == isIncludedInPayroll.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.PolicyCode.ToLower().Contains(keyword) ||
                    x.PolicyName.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<WfpTransportAllowancePolicy> ApplySorting(
            IQueryable<WfpTransportAllowancePolicy> query,
            string? sortBy,
            string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "policyName").Trim().ToLowerInvariant() switch
            {
                "policycode" => desc
                    ? query.OrderByDescending(x => x.PolicyCode)
                    : query.OrderBy(x => x.PolicyCode),
                "calculationmethod" => desc
                    ? query.OrderByDescending(x => x.CalculationMethod)
                    : query.OrderBy(x => x.CalculationMethod),
                "effectivestartdate" => desc
                    ? query.OrderByDescending(x => x.EffectiveStartDate)
                    : query.OrderBy(x => x.EffectiveStartDate),
                "createdatetime" => desc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),
                "isactive" => desc
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),
                _ => desc
                    ? query.OrderByDescending(x => x.PolicyName)
                    : query.OrderBy(x => x.PolicyName)
            };
        }

        private WfpTransportAllowancePolicyResponse MapResponse(
            WfpTransportAllowancePolicy entity)
        {
            return new WfpTransportAllowancePolicyResponse
            {
                Id = entity.Id,
                LegalEntityId = entity.LegalEntityId,
                HospitalSiteId = entity.HospitalSiteId,
                EmployeeGradeId = entity.EmployeeGradeId,
                PayrollComponentId = entity.PayrollComponentId,
                PayrollComponentCode = entity.PayrollComponent?.PayrollComponentCode,
                PayrollComponentName = entity.PayrollComponent?.PayrollComponentName,
                PolicyCode = entity.PolicyCode,
                PolicyName = entity.PolicyName,
                CalculationMethod = entity.CalculationMethod,
                FixedMonthlyAmount = entity.FixedMonthlyAmount,
                PerAttendanceAmount = entity.PerAttendanceAmount,
                DailyLimitAmount = entity.DailyLimitAmount,
                MonthlyLimitAmount = entity.MonthlyLimitAmount,
                MinimumAttendanceMinutes = entity.MinimumAttendanceMinutes,
                IsAttendanceBased = entity.IsAttendanceBased,
                IsProrated = entity.IsProrated,
                IsTaxable = entity.IsTaxable,
                IsIncludedInPayroll = entity.IsIncludedInPayroll,
                IncludeBusinessTravelDay = entity.IncludeBusinessTravelDay,
                IncludePaidLeaveDay = entity.IncludePaidLeaveDay,
                IncludeHoliday = entity.IncludeHoliday,
                EffectiveStartDate = entity.EffectiveStartDate,
                EffectiveEndDate = entity.EffectiveEndDate,
                Description = entity.Description,
                IsActive = entity.IsActive,
                TransportAllowanceCount = entity.TransportAllowances.Count(x => !x.IsDelete),
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                CreateByName = GetUserDisplayName(entity.CreateBy)
            };
        }

        private WfpTransportAllowancePolicyDetailResponse MapDetailResponse(
            WfpTransportAllowancePolicy entity)
        {
            var response = MapResponse(entity);

            return new WfpTransportAllowancePolicyDetailResponse
            {
                Id = response.Id,
                LegalEntityId = response.LegalEntityId,
                HospitalSiteId = response.HospitalSiteId,
                EmployeeGradeId = response.EmployeeGradeId,
                PayrollComponentId = response.PayrollComponentId,
                PayrollComponentCode = response.PayrollComponentCode,
                PayrollComponentName = response.PayrollComponentName,
                PolicyCode = response.PolicyCode,
                PolicyName = response.PolicyName,
                CalculationMethod = response.CalculationMethod,
                FixedMonthlyAmount = response.FixedMonthlyAmount,
                PerAttendanceAmount = response.PerAttendanceAmount,
                DailyLimitAmount = response.DailyLimitAmount,
                MonthlyLimitAmount = response.MonthlyLimitAmount,
                MinimumAttendanceMinutes = response.MinimumAttendanceMinutes,
                IsAttendanceBased = response.IsAttendanceBased,
                IsProrated = response.IsProrated,
                IsTaxable = response.IsTaxable,
                IsIncludedInPayroll = response.IsIncludedInPayroll,
                IncludeBusinessTravelDay = response.IncludeBusinessTravelDay,
                IncludePaidLeaveDay = response.IncludePaidLeaveDay,
                IncludeHoliday = response.IncludeHoliday,
                EffectiveStartDate = response.EffectiveStartDate,
                EffectiveEndDate = response.EffectiveEndDate,
                Description = response.Description,
                IsActive = response.IsActive,
                TransportAllowanceCount = response.TransportAllowanceCount,
                CreateDateTime = response.CreateDateTime,
                CreateBy = response.CreateBy,
                CreateByName = response.CreateByName,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetUserDisplayName(entity.UpdateBy)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateWfpTransportAllowancePolicyRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.PolicyName))
                return (false, "Nama policy wajib diisi.");

            if (!AllowedCalculationMethods.Contains(request.CalculationMethod.Trim()))
                return (false, "CalculationMethod tidak valid.");

            if (request.EffectiveStartDate.HasValue &&
                request.EffectiveEndDate.HasValue &&
                request.EffectiveEndDate.Value < request.EffectiveStartDate.Value)
            {
                return (false, "EffectiveEndDate tidak boleh sebelum EffectiveStartDate.");
            }

            if (request.CalculationMethod.Equals("FixedMonthly", StringComparison.OrdinalIgnoreCase) &&
                request.FixedMonthlyAmount <= 0)
            {
                return (false, "FixedMonthlyAmount harus lebih besar dari 0 untuk metode FixedMonthly.");
            }

            if (request.CalculationMethod.Equals("PerAttendance", StringComparison.OrdinalIgnoreCase) &&
                request.PerAttendanceAmount <= 0)
            {
                return (false, "PerAttendanceAmount harus lebih besar dari 0 untuk metode PerAttendance.");
            }

            if (request.LegalEntityId.HasValue && request.LegalEntityId.Value != Guid.Empty &&
                !await _dbContext.Set<MstLegalEntity>().AnyAsync(x =>
                    x.Id == request.LegalEntityId.Value && x.IsActive && !x.IsDelete,
                    cancellationToken))
            {
                return (false, "Legal entity tidak ditemukan atau tidak aktif.");
            }

            if (request.HospitalSiteId.HasValue && request.HospitalSiteId.Value != Guid.Empty &&
                !await _dbContext.Set<MstHospitalSite>().AnyAsync(x =>
                    x.Id == request.HospitalSiteId.Value && x.IsActive && !x.IsDelete,
                    cancellationToken))
            {
                return (false, "Hospital site tidak ditemukan atau tidak aktif.");
            }

            if (request.EmployeeGradeId.HasValue && request.EmployeeGradeId.Value != Guid.Empty &&
                !await _dbContext.Set<MstEmployeeGrade>().AnyAsync(x =>
                    x.Id == request.EmployeeGradeId.Value && x.IsActive && !x.IsDelete,
                    cancellationToken))
            {
                return (false, "Employee grade tidak ditemukan atau tidak aktif.");
            }

            if (request.PayrollComponentId.HasValue && request.PayrollComponentId.Value != Guid.Empty &&
                !await _dbContext.Set<MstPayrollComponent>().AnyAsync(x =>
                    x.Id == request.PayrollComponentId.Value && x.IsActive && !x.IsDelete,
                    cancellationToken))
            {
                return (false, "Payroll component tidak ditemukan atau tidak aktif.");
            }

            var normalizedName = request.PolicyName.Trim().ToLower();
            var duplicateQuery = _dbContext.Set<WfpTransportAllowancePolicy>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.PolicyName.ToLower() == normalizedName);

            if (excludeId.HasValue)
                duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateQuery.AnyAsync(cancellationToken))
                return (false, "Nama transport allowance policy sudah digunakan.");

            return (true, null);
        }

        private async Task<string> GenerateCodeAsync(CancellationToken cancellationToken)
        {
            var codes = await _dbContext.Set<WfpTransportAllowancePolicy>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.PolicyCode.StartsWith(CodePrefix))
                .Select(x => x.PolicyCode)
                .ToListAsync(cancellationToken);

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

        private string? GetUserDisplayName(Guid userId)
        {
            if (userId == Guid.Empty)
                return null;

            return _dbContext.Users
                .Where(x => x.Id == userId)
                .Select(x => x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode)
                .FirstOrDefault();
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

        private static string NormalizeCalculationMethod(string value)
        {
            return AllowedCalculationMethods.First(x =>
                x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static string BuildCalculationMethodLabel(string value)
        {
            return value switch
            {
                "FixedMonthly" => "Tetap Bulanan",
                "PerAttendance" => "Per Kehadiran",
                "PerDay" => "Per Hari",
                "Reimbursement" => "Reimbursement",
                _ => value
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(
            int pageNumber,
            int pageSize)
        {
            return (
                pageNumber < 1 ? 1 : pageNumber,
                pageSize < 1 ? 25 : Math.Min(pageSize, 100));
        }
    }
}
