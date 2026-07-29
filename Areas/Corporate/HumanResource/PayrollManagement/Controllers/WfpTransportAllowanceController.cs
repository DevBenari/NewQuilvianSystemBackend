using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
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
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/transport-allowance")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_PAYROLL_MANAGEMENT",
        moduleName: "Human Resource Payroll Management",
        displayName: "Workforce Transport Allowance",
        AreaName = "Corporate",
        ControllerName = "WorkforceTransportAllowance",
        Description = "Corporate human resource workforce transport allowance",
        SortOrder = 11)]
    [Tags("Corporate / Human Resource / Payroll Management / Transport Allowance")]
    public class WfpTransportAllowanceController : ControllerBase
    {
        private static readonly HashSet<string> AllowedStatuses =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "Active",
                "Suspended",
                "Expired",
                "Terminated"
            };

        private const string LogCategory = "Corporate.HumanResource.PayrollManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WfpTransportAllowanceController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Workforce Transport Allowance", Description = "Melihat metadata filter transport allowance workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceTransportAllowance", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new WfpTransportAllowanceFilterMetadataResponse
            {
                DefaultFilter = new WfpTransportAllowanceDefaultFilterResponse(),
                AllowanceStatusOptions = AllowedStatuses
                    .OrderBy(x => x)
                    .Select(x => new WfpTransportAllowanceStringOptionResponse
                    {
                        Value = x,
                        Label = BuildStatusLabel(x)
                    })
                    .ToList(),
                SortOptions = new List<WfpTransportAllowanceSortOptionResponse>
                {
                    new() { Value = "allowanceStatus", Label = "Status allowance" },
                    new() { Value = "monthlyAmount", Label = "Nominal bulanan" },
                    new() { Value = "remainingAmount", Label = "Sisa allowance" },
                    new() { Value = "effectiveStartDate", Label = "Tanggal mulai berlaku" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            return Ok(ApiResponse<WfpTransportAllowanceFilterMetadataResponse>.Ok(
                result,
                "Metadata filter transport allowance workforce berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Workforce Transport Allowance", Description = "Melihat ringkasan transport allowance workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceTransportAllowance", "Read")]
        public async Task<IActionResult> GetSummary(
            Guid workforceProfileId,
            CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil tenaga kerja tidak ditemukan."));
            }

            var query = _dbContext.Set<WfpTransportAllowance>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            var result = new WfpTransportAllowanceSummaryResponse
            {
                TotalData = await query.CountAsync(cancellationToken),
                ActiveData = await query.CountAsync(x => x.IsActive, cancellationToken),
                InactiveData = await query.CountAsync(x => !x.IsActive, cancellationToken),
                TotalMonthlyAmount = await query.SumAsync(x => x.MonthlyAmount, cancellationToken),
                TotalAccruedAmount = await query.SumAsync(x => x.AccruedAmount, cancellationToken),
                TotalPaidAmount = await query.SumAsync(x => x.PaidAmount, cancellationToken),
                TotalRemainingAmount = await query.SumAsync(x => x.RemainingAmount, cancellationToken)
            };

            return Ok(ApiResponse<WfpTransportAllowanceSummaryResponse>.Ok(
                result,
                "Ringkasan transport allowance workforce berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Workforce Transport Allowance", Description = "Melihat transport allowance workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceTransportAllowance", "Read")]
        public async Task<IActionResult> GetData(
            Guid workforceProfileId,
            [FromQuery] string? allowanceStatus,
            [FromQuery] Guid? transportAllowancePolicyId,
            [FromQuery] Guid? payrollComponentId,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "createDateTime",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil tenaga kerja tidak ditemukan."));
            }

            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyFilter(
                BuildBaseQuery(workforceProfileId),
                allowanceStatus,
                transportAllowancePolicyId,
                payrollComponentId,
                isActive,
                search);

            var totalData = await query.CountAsync(cancellationToken);

            var rows = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var result = new PagedResult<WfpTransportAllowanceResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = rows.Select(MapResponse).ToList()
            };

            return Ok(ApiResponse<PagedResult<WfpTransportAllowanceResponse>>.Ok(
                result,
                "Data transport allowance workforce berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Workforce Transport Allowance", Description = "Melihat detail transport allowance workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceTransportAllowance", "Read")]
        public async Task<IActionResult> GetById(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await BuildBaseQuery(workforceProfileId)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Transport allowance workforce tidak ditemukan."));
            }

            return Ok(ApiResponse<WfpTransportAllowanceDetailResponse>.Ok(
                MapDetailResponse(entity),
                "Detail transport allowance workforce berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Workforce Transport Allowance", Description = "Membuat transport allowance workforce", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WorkforceTransportAllowance", "Create")]
        public async Task<IActionResult> Create(
            Guid workforceProfileId,
            [FromBody] CreateWfpTransportAllowanceRequest request,
            CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil tenaga kerja tidak ditemukan."));
            }

            if (await _dbContext.Set<WfpTransportAllowance>().AnyAsync(x =>
                    x.WorkforceProfileId == workforceProfileId && !x.IsDelete,
                    cancellationToken))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Transport allowance untuk workforce ini sudah tersedia."));
            }

            var validation = await ValidateRequestAsync(
                workforceProfileId,
                request,
                cancellationToken);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data transport allowance workforce tidak valid."));
            }

            var entity = new WfpTransportAllowance
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                EmployeeId = NormalizeGuid(request.EmployeeId),
                OrganizationAssignmentId = NormalizeGuid(request.OrganizationAssignmentId),
                TransportAllowancePolicyId = NormalizeGuid(request.TransportAllowancePolicyId),
                PayrollComponentId = NormalizeGuid(request.PayrollComponentId),
                AllowanceStatus = NormalizeStatus(request.AllowanceStatus),
                CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant(),
                MonthlyAmount = request.MonthlyAmount,
                PerAttendanceAmount = request.PerAttendanceAmount,
                MaximumMonthlyAmount = request.MaximumMonthlyAmount,
                AccruedAmount = request.AccruedAmount,
                UsedAmount = request.UsedAmount,
                PaidAmount = request.PaidAmount,
                RemainingAmount = request.RemainingAmount,
                EffectiveStartDate = request.EffectiveStartDate,
                EffectiveEndDate = request.EffectiveEndDate,
                Description = NormalizeText(request.Description),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<WfpTransportAllowance>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceTransportAllowance.Create",
                "Membuat transport allowance workforce.",
                new { entity.Id, entity.WorkforceProfileId });

            return await GetById(workforceProfileId, entity.Id, cancellationToken);
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Workforce Transport Allowance", Description = "Mengubah transport allowance workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceTransportAllowance", "Update")]
        public async Task<IActionResult> Update(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpTransportAllowanceRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpTransportAllowance>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Transport allowance workforce tidak ditemukan."));
            }

            var validation = await ValidateRequestAsync(
                workforceProfileId,
                request,
                cancellationToken);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data transport allowance workforce tidak valid."));
            }

            entity.EmployeeId = NormalizeGuid(request.EmployeeId);
            entity.OrganizationAssignmentId = NormalizeGuid(request.OrganizationAssignmentId);
            entity.TransportAllowancePolicyId = NormalizeGuid(request.TransportAllowancePolicyId);
            entity.PayrollComponentId = NormalizeGuid(request.PayrollComponentId);
            entity.AllowanceStatus = NormalizeStatus(request.AllowanceStatus);
            entity.CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant();
            entity.MonthlyAmount = request.MonthlyAmount;
            entity.PerAttendanceAmount = request.PerAttendanceAmount;
            entity.MaximumMonthlyAmount = request.MaximumMonthlyAmount;
            entity.AccruedAmount = request.AccruedAmount;
            entity.UsedAmount = request.UsedAmount;
            entity.PaidAmount = request.PaidAmount;
            entity.RemainingAmount = request.RemainingAmount;
            entity.EffectiveStartDate = request.EffectiveStartDate;
            entity.EffectiveEndDate = request.EffectiveEndDate;
            entity.Description = NormalizeText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetById(workforceProfileId, entity.Id, cancellationToken);
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Workforce Transport Allowance Status", Description = "Mengubah status transport allowance workforce", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("WorkforceTransportAllowance", "Update")]
        public async Task<IActionResult> UpdateStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpTransportAllowanceStatusRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpTransportAllowance>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Transport allowance workforce tidak ditemukan."));
            }

            if (!AllowedStatuses.Contains(request.AllowanceStatus.Trim()))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "AllowanceStatus tidak valid."));
            }

            entity.AllowanceStatus = NormalizeStatus(request.AllowanceStatus);
            entity.IsActive = request.IsActive;
            entity.EffectiveEndDate = request.EffectiveEndDate;
            entity.Description = NormalizeText(request.Description) ?? entity.Description;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status transport allowance workforce berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Workforce Transport Allowance", Description = "Menghapus transport allowance workforce", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("WorkforceTransportAllowance", "Delete")]
        public async Task<IActionResult> Delete(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpTransportAllowance>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Transport allowance workforce tidak ditemukan."));
            }

            if (await _dbContext.Set<WfpTransportAllowanceTransaction>().AnyAsync(x =>
                    x.TransportAllowanceId == id && !x.IsDelete,
                    cancellationToken))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Transport allowance tidak dapat dihapus karena sudah mempunyai transaksi."));
            }

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.AllowanceStatus = "Terminated";
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(
                null,
                "Transport allowance workforce berhasil dihapus."));
        }

        private IQueryable<WfpTransportAllowance> BuildBaseQuery(Guid workforceProfileId)
        {
            return _dbContext.Set<WfpTransportAllowance>()
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.Employee)
                .Include(x => x.TransportAllowancePolicy)
                .Include(x => x.PayrollComponent)
                .Include(x => x.Transactions)
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);
        }

        private static IQueryable<WfpTransportAllowance> ApplyFilter(
            IQueryable<WfpTransportAllowance> query,
            string? allowanceStatus,
            Guid? transportAllowancePolicyId,
            Guid? payrollComponentId,
            bool? isActive,
            string? search)
        {
            if (!string.IsNullOrWhiteSpace(allowanceStatus))
                query = query.Where(x => x.AllowanceStatus == allowanceStatus.Trim());

            if (transportAllowancePolicyId.HasValue && transportAllowancePolicyId.Value != Guid.Empty)
                query = query.Where(x => x.TransportAllowancePolicyId == transportAllowancePolicyId.Value);

            if (payrollComponentId.HasValue && payrollComponentId.Value != Guid.Empty)
                query = query.Where(x => x.PayrollComponentId == payrollComponentId.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.AllowanceStatus.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.TransportAllowancePolicy != null &&
                     x.TransportAllowancePolicy.PolicyName.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<WfpTransportAllowance> ApplySorting(
            IQueryable<WfpTransportAllowance> query,
            string? sortBy,
            string? sortDirection)
        {
            var desc = !string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "createDateTime").Trim().ToLowerInvariant() switch
            {
                "allowancestatus" => desc
                    ? query.OrderByDescending(x => x.AllowanceStatus)
                    : query.OrderBy(x => x.AllowanceStatus),
                "monthlyamount" => desc
                    ? query.OrderByDescending(x => x.MonthlyAmount)
                    : query.OrderBy(x => x.MonthlyAmount),
                "remainingamount" => desc
                    ? query.OrderByDescending(x => x.RemainingAmount)
                    : query.OrderBy(x => x.RemainingAmount),
                "effectivestartdate" => desc
                    ? query.OrderByDescending(x => x.EffectiveStartDate)
                    : query.OrderBy(x => x.EffectiveStartDate),
                _ => desc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime)
            };
        }

        private WfpTransportAllowanceResponse MapResponse(WfpTransportAllowance entity)
        {
            return new WfpTransportAllowanceResponse
            {
                Id = entity.Id,
                WorkforceProfileId = entity.WorkforceProfileId,
                WorkforceProfileCode = entity.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName = entity.WorkforceProfile?.DisplayName ?? string.Empty,
                EmployeeId = entity.EmployeeId,
                EmployeeCode = entity.Employee?.EmployeeCode,
                EmployeeName = entity.Employee?.FullName,
                OrganizationAssignmentId = entity.OrganizationAssignmentId,
                TransportAllowancePolicyId = entity.TransportAllowancePolicyId,
                TransportAllowancePolicyCode = entity.TransportAllowancePolicy?.PolicyCode,
                TransportAllowancePolicyName = entity.TransportAllowancePolicy?.PolicyName,
                PayrollComponentId = entity.PayrollComponentId,
                PayrollComponentCode = entity.PayrollComponent?.PayrollComponentCode,
                PayrollComponentName = entity.PayrollComponent?.PayrollComponentName,
                AllowanceStatus = entity.AllowanceStatus,
                CurrencyCode = entity.CurrencyCode,
                MonthlyAmount = entity.MonthlyAmount,
                PerAttendanceAmount = entity.PerAttendanceAmount,
                MaximumMonthlyAmount = entity.MaximumMonthlyAmount,
                AccruedAmount = entity.AccruedAmount,
                UsedAmount = entity.UsedAmount,
                PaidAmount = entity.PaidAmount,
                RemainingAmount = entity.RemainingAmount,
                EffectiveStartDate = entity.EffectiveStartDate,
                EffectiveEndDate = entity.EffectiveEndDate,
                Description = entity.Description,
                IsActive = entity.IsActive,
                TransactionCount = entity.Transactions.Count(x => !x.IsDelete),
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                CreateByName = GetUserDisplayName(entity.CreateBy)
            };
        }

        private WfpTransportAllowanceDetailResponse MapDetailResponse(
            WfpTransportAllowance entity)
        {
            var response = MapResponse(entity);

            return new WfpTransportAllowanceDetailResponse
            {
                Id = response.Id,
                WorkforceProfileId = response.WorkforceProfileId,
                WorkforceProfileCode = response.WorkforceProfileCode,
                WorkforceDisplayName = response.WorkforceDisplayName,
                EmployeeId = response.EmployeeId,
                EmployeeCode = response.EmployeeCode,
                EmployeeName = response.EmployeeName,
                OrganizationAssignmentId = response.OrganizationAssignmentId,
                TransportAllowancePolicyId = response.TransportAllowancePolicyId,
                TransportAllowancePolicyCode = response.TransportAllowancePolicyCode,
                TransportAllowancePolicyName = response.TransportAllowancePolicyName,
                PayrollComponentId = response.PayrollComponentId,
                PayrollComponentCode = response.PayrollComponentCode,
                PayrollComponentName = response.PayrollComponentName,
                AllowanceStatus = response.AllowanceStatus,
                CurrencyCode = response.CurrencyCode,
                MonthlyAmount = response.MonthlyAmount,
                PerAttendanceAmount = response.PerAttendanceAmount,
                MaximumMonthlyAmount = response.MaximumMonthlyAmount,
                AccruedAmount = response.AccruedAmount,
                UsedAmount = response.UsedAmount,
                PaidAmount = response.PaidAmount,
                RemainingAmount = response.RemainingAmount,
                EffectiveStartDate = response.EffectiveStartDate,
                EffectiveEndDate = response.EffectiveEndDate,
                Description = response.Description,
                IsActive = response.IsActive,
                TransactionCount = response.TransactionCount,
                CreateDateTime = response.CreateDateTime,
                CreateBy = response.CreateBy,
                CreateByName = response.CreateByName,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetUserDisplayName(entity.UpdateBy)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid workforceProfileId,
            CreateWfpTransportAllowanceRequest request,
            CancellationToken cancellationToken)
        {
            if (!AllowedStatuses.Contains(request.AllowanceStatus.Trim()))
                return (false, "AllowanceStatus tidak valid.");

            if (string.IsNullOrWhiteSpace(request.CurrencyCode) || request.CurrencyCode.Trim().Length != 3)
                return (false, "CurrencyCode harus terdiri dari tiga karakter.");

            if (request.EffectiveStartDate.HasValue &&
                request.EffectiveEndDate.HasValue &&
                request.EffectiveEndDate.Value < request.EffectiveStartDate.Value)
            {
                return (false, "EffectiveEndDate tidak boleh sebelum EffectiveStartDate.");
            }

            if (request.MaximumMonthlyAmount > 0 && request.MonthlyAmount > request.MaximumMonthlyAmount)
                return (false, "MonthlyAmount tidak boleh melebihi MaximumMonthlyAmount.");

            if (request.EmployeeId.HasValue && request.EmployeeId.Value != Guid.Empty &&
                !await _dbContext.Set<MstEmployee>().AnyAsync(x =>
                    x.Id == request.EmployeeId.Value &&
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete,
                    cancellationToken))
            {
                return (false, "Employee tidak valid atau tidak sesuai workforce profile.");
            }

            if (request.OrganizationAssignmentId.HasValue && request.OrganizationAssignmentId.Value != Guid.Empty &&
                !await _dbContext.Set<WfpOrganizationAssignment>().AnyAsync(x =>
                    x.Id == request.OrganizationAssignmentId.Value &&
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete,
                    cancellationToken))
            {
                return (false, "Organization assignment tidak valid atau tidak sesuai workforce profile.");
            }

            if (request.TransportAllowancePolicyId.HasValue && request.TransportAllowancePolicyId.Value != Guid.Empty &&
                !await _dbContext.Set<WfpTransportAllowancePolicy>().AnyAsync(x =>
                    x.Id == request.TransportAllowancePolicyId.Value &&
                    x.IsActive &&
                    !x.IsDelete,
                    cancellationToken))
            {
                return (false, "Transport allowance policy tidak ditemukan atau tidak aktif.");
            }

            if (request.PayrollComponentId.HasValue && request.PayrollComponentId.Value != Guid.Empty &&
                !await _dbContext.Set<MstPayrollComponent>().AnyAsync(x =>
                    x.Id == request.PayrollComponentId.Value &&
                    x.IsActive &&
                    !x.IsDelete,
                    cancellationToken))
            {
                return (false, "Payroll component tidak ditemukan atau tidak aktif.");
            }

            return (true, null);
        }

        private async Task<bool> WorkforceProfileExistsAsync(
            Guid workforceProfileId,
            CancellationToken cancellationToken)
        {
            return workforceProfileId != Guid.Empty &&
                   await _dbContext.MstWorkforceProfiles
                       .AsNoTracking()
                       .AnyAsync(x =>
                           x.Id == workforceProfileId &&
                           x.IsActive &&
                           !x.IsDelete,
                           cancellationToken);
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

        private static string NormalizeStatus(string value)
        {
            return AllowedStatuses.First(x =>
                x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static string BuildStatusLabel(string value)
        {
            return value switch
            {
                "Active" => "Aktif",
                "Suspended" => "Ditangguhkan",
                "Expired" => "Kedaluwarsa",
                "Terminated" => "Dihentikan",
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
