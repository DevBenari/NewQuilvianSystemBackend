using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponsePayrollPeriodPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.DTOs.PayrollPeriodResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/payroll-periods")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Payroll Period",
        AreaName = "Corporate",
        ControllerName = "PayrollPeriod",
        Description = "Corporate human resource master data payroll period",
        SortOrder = 42)]
    [Tags("Corporate / Human Resource / Master Data / Payroll Period")]
    public class PayrollPeriodController : ControllerBase
    {
        private static readonly HashSet<string> AllowedPeriodTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "Monthly",
            "BiWeekly",
            "Weekly",
            "Special",
            "Adjustment"
        };

        private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "Draft",
            "Open",
            "Processing",
            "Review",
            "Approved",
            "Closed",
            "Posted",
            "Cancelled"
        };

        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "PRD-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public PayrollPeriodController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<PayrollPeriodFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Payroll Period", Description = "Melihat metadata filter payroll period", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PayrollPeriod", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new PayrollPeriodFilterMetadataResponse
            {
                DefaultFilter = new PayrollPeriodDefaultFilterResponse(),
                PeriodTypeOptions = AllowedPeriodTypes
                    .OrderBy(x => x)
                    .Select(x => new PayrollMasterStringOptionResponse
                    {
                        Value = x,
                        Label = BuildPeriodTypeLabel(x)
                    })
                    .ToList(),
                PayrollPeriodStatusOptions = AllowedStatuses
                    .OrderBy(x => x)
                    .Select(x => new PayrollMasterStringOptionResponse
                    {
                        Value = x,
                        Label = BuildStatusLabel(x)
                    })
                    .ToList(),
                CustomPeriods = BuildPeriodOptions(),
                SortOptions = new List<PayrollMasterSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "payrollPeriodCode", Label = "Kode payroll period" },
                    new() { Value = "payrollPeriodName", Label = "Nama payroll period" },
                    new() { Value = "startDate", Label = "Tanggal mulai" },
                    new() { Value = "paymentDate", Label = "Tanggal pembayaran" },
                    new() { Value = "payrollPeriodStatus", Label = "Status payroll period" },
                    new() { Value = "isLocked", Label = "Status kunci" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PayrollPeriod.GetFilterMetadata",
                "Mengambil metadata filter payroll period.",
                result);

            return Ok(ApiResponse<PayrollPeriodFilterMetadataResponse>.Ok(
                result,
                "Metadata filter payroll period berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<PayrollPeriodSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Payroll Period", Description = "Melihat ringkasan payroll period", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PayrollPeriod", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();

            var result = new PayrollPeriodSummaryResponse
            {
                TotalPayrollPeriod = await query.CountAsync(),
                ActivePayrollPeriod = await query.CountAsync(x => x.IsActive),
                InactivePayrollPeriod = await query.CountAsync(x => !x.IsActive),
                DraftPayrollPeriod = await query.CountAsync(x => x.PayrollPeriodStatus == "Draft"),
                OpenPayrollPeriod = await query.CountAsync(x => x.PayrollPeriodStatus == "Open"),
                ProcessingPayrollPeriod = await query.CountAsync(x => x.PayrollPeriodStatus == "Processing"),
                ApprovedPayrollPeriod = await query.CountAsync(x => x.PayrollPeriodStatus == "Approved"),
                ClosedPayrollPeriod = await query.CountAsync(x => x.PayrollPeriodStatus == "Closed"),
                LockedPayrollPeriod = await query.CountAsync(x => x.IsLocked)
            };

            return Ok(ApiResponse<PayrollPeriodSummaryResponse>.Ok(
                result,
                "Ringkasan payroll period berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponsePayrollPeriodPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Payroll Period", Description = "Melihat data payroll period", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PayrollPeriod", "Read")]
        public async Task<IActionResult> GetPayrollPeriods(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? legalEntityId,
            [FromQuery] Guid? hospitalSiteId,
            [FromQuery] string? periodType,
            [FromQuery] int? fiscalYear,
            [FromQuery] string? payrollPeriodStatus,
            [FromQuery] bool? isLocked,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "startDate",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyDateFilter(BuildBaseQuery(), startDate, endDate, customPeriod);
            query = ApplyStandardFilter(
                query,
                legalEntityId,
                hospitalSiteId,
                periodType,
                fiscalYear,
                payrollPeriodStatus,
                isLocked,
                isActive,
                search);

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new PayrollPeriodResponse
                {
                    Id = x.Id,
                    LegalEntityId = x.LegalEntityId,
                    HospitalSiteId = x.HospitalSiteId,
                    PayrollPeriodCode = x.PayrollPeriodCode,
                    PayrollPeriodName = x.PayrollPeriodName,
                    PeriodType = x.PeriodType,
                    FiscalYear = x.FiscalYear,
                    PeriodNumber = x.PeriodNumber,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    AttendanceCutoffDate = x.AttendanceCutoffDate,
                    VariableInputCutoffDate = x.VariableInputCutoffDate,
                    ApprovalDueDate = x.ApprovalDueDate,
                    PaymentDate = x.PaymentDate,
                    PayrollPeriodStatus = x.PayrollPeriodStatus,
                    IsLocked = x.IsLocked,
                    LockedAt = x.LockedAt,
                    LockedByUserId = x.LockedByUserId,
                    Description = x.Description,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    WorkforcePayrollCount = _dbContext.Set<WfpPayroll>()
                        .Count(p => p.LastPayrollPeriodId == x.Id && !p.IsDelete),
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

            var result = new ResponsePayrollPeriodPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponsePayrollPeriodPagedResult>.Ok(
                result,
                "Data payroll period berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<PayrollPeriodOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Payroll Period", Description = "Melihat pilihan payroll period", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PayrollPeriod", "Read")]
        public async Task<IActionResult> GetPayrollPeriodOptions(
            [FromQuery] Guid? legalEntityId,
            [FromQuery] Guid? hospitalSiteId,
            [FromQuery] string? periodType,
            [FromQuery] int? fiscalYear,
            [FromQuery] string? payrollPeriodStatus,
            [FromQuery] bool includeLocked = true,
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
                legalEntityId,
                hospitalSiteId,
                periodType,
                fiscalYear,
                payrollPeriodStatus,
                includeLocked ? null : false,
                onlyActive ? true : null,
                search);

            var totalData = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.FiscalYear)
                .ThenByDescending(x => x.PeriodNumber)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new PayrollPeriodOptionResponse
                {
                    Id = x.Id,
                    PayrollPeriodCode = x.PayrollPeriodCode,
                    PayrollPeriodName = x.PayrollPeriodName,
                    PeriodType = x.PeriodType,
                    FiscalYear = x.FiscalYear,
                    PeriodNumber = x.PeriodNumber,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    PayrollPeriodStatus = x.PayrollPeriodStatus,
                    IsLocked = x.IsLocked
                })
                .ToListAsync();

            return Ok(ApiResponse<PayrollPeriodOptionPagedResponse>.Ok(
                new PayrollPeriodOptionPagedResponse
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data pilihan payroll period berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PayrollPeriodDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Payroll Period", Description = "Melihat detail payroll period", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PayrollPeriod", "Read")]
        public async Task<IActionResult> GetPayrollPeriodById(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new PayrollPeriodDetailResponse
                {
                    Id = x.Id,
                    LegalEntityId = x.LegalEntityId,
                    HospitalSiteId = x.HospitalSiteId,
                    PayrollPeriodCode = x.PayrollPeriodCode,
                    PayrollPeriodName = x.PayrollPeriodName,
                    PeriodType = x.PeriodType,
                    FiscalYear = x.FiscalYear,
                    PeriodNumber = x.PeriodNumber,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    AttendanceCutoffDate = x.AttendanceCutoffDate,
                    VariableInputCutoffDate = x.VariableInputCutoffDate,
                    ApprovalDueDate = x.ApprovalDueDate,
                    PaymentDate = x.PaymentDate,
                    PayrollPeriodStatus = x.PayrollPeriodStatus,
                    IsLocked = x.IsLocked,
                    LockedAt = x.LockedAt,
                    LockedByUserId = x.LockedByUserId,
                    Description = x.Description,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    WorkforcePayrollCount = _dbContext.Set<WfpPayroll>()
                        .Count(p => p.LastPayrollPeriodId == x.Id && !p.IsDelete),
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
                    "Payroll period tidak ditemukan."));
            }

            return Ok(ApiResponse<PayrollPeriodDetailResponse>.Ok(
                data,
                "Detail payroll period berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PayrollPeriodCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Payroll Period", Description = "Membuat payroll period", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PayrollPeriod", "Create")]
        public async Task<IActionResult> CreatePayrollPeriod(
            [FromBody] CreatePayrollPeriodRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data payroll period tidak valid."));
            }

            var entity = new MstPayrollPeriod
            {
                Id = Guid.NewGuid(),
                LegalEntityId = NormalizeGuid(request.LegalEntityId),
                HospitalSiteId = NormalizeGuid(request.HospitalSiteId),
                PayrollPeriodCode = await GenerateCodeAsync(),
                PayrollPeriodName = request.PayrollPeriodName.Trim(),
                PeriodType = NormalizePeriodType(request.PeriodType),
                FiscalYear = request.FiscalYear,
                PeriodNumber = request.PeriodNumber,
                StartDate = request.StartDate.Date,
                EndDate = request.EndDate.Date,
                AttendanceCutoffDate = request.AttendanceCutoffDate?.Date,
                VariableInputCutoffDate = request.VariableInputCutoffDate?.Date,
                ApprovalDueDate = request.ApprovalDueDate?.Date,
                PaymentDate = request.PaymentDate?.Date,
                PayrollPeriodStatus = NormalizeStatus(request.PayrollPeriodStatus),
                IsLocked = false,
                LockedAt = null,
                LockedByUserId = null,
                Description = NormalizeNullableString(request.Description),
                SortOrder = request.SortOrder,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstPayrollPeriod>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var result = new PayrollPeriodCreateResponse
            {
                Id = entity.Id,
                PayrollPeriodCode = entity.PayrollPeriodCode,
                PayrollPeriodName = entity.PayrollPeriodName,
                PayrollPeriodStatus = entity.PayrollPeriodStatus,
                IsActive = entity.IsActive
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PayrollPeriod.Create",
                "Membuat payroll period.",
                result);

            return Ok(ApiResponse<PayrollPeriodCreateResponse>.Ok(
                result,
                "Payroll period berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Update", "Update Payroll Period", Description = "Mengubah payroll period", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PayrollPeriod", "Update")]
        public async Task<IActionResult> UpdatePayrollPeriod(
            Guid id,
            [FromBody] UpdatePayrollPeriodRequest request)
        {
            var entity = await _dbContext.Set<MstPayrollPeriod>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Payroll period tidak ditemukan."));
            }

            if (entity.IsLocked)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Payroll period yang terkunci tidak dapat diubah."));
            }

            var validation = await ValidateRequestAsync(id, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data payroll period tidak valid."));
            }

            entity.LegalEntityId = NormalizeGuid(request.LegalEntityId);
            entity.HospitalSiteId = NormalizeGuid(request.HospitalSiteId);
            entity.PayrollPeriodName = request.PayrollPeriodName.Trim();
            entity.PeriodType = NormalizePeriodType(request.PeriodType);
            entity.FiscalYear = request.FiscalYear;
            entity.PeriodNumber = request.PeriodNumber;
            entity.StartDate = request.StartDate.Date;
            entity.EndDate = request.EndDate.Date;
            entity.AttendanceCutoffDate = request.AttendanceCutoffDate?.Date;
            entity.VariableInputCutoffDate = request.VariableInputCutoffDate?.Date;
            entity.ApprovalDueDate = request.ApprovalDueDate?.Date;
            entity.PaymentDate = request.PaymentDate?.Date;
            entity.PayrollPeriodStatus = NormalizeStatus(request.PayrollPeriodStatus);
            entity.Description = NormalizeNullableString(request.Description);
            entity.SortOrder = request.SortOrder;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Payroll period berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Update", "Update Payroll Period Status", Description = "Mengubah status payroll period", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("PayrollPeriod", "Update")]
        public async Task<IActionResult> UpdatePayrollPeriodStatus(
            Guid id,
            [FromBody] UpdatePayrollPeriodStatusRequest request)
        {
            var entity = await _dbContext.Set<MstPayrollPeriod>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Payroll period tidak ditemukan."));
            }

            if (!AllowedStatuses.Contains(request.PayrollPeriodStatus.Trim()))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "PayrollPeriodStatus tidak valid."));
            }

            if (entity.IsLocked && !request.PayrollPeriodStatus.Equals("Closed", StringComparison.OrdinalIgnoreCase) &&
                !request.PayrollPeriodStatus.Equals("Posted", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Payroll period terkunci hanya dapat berada pada status Closed atau Posted."));
            }

            entity.PayrollPeriodStatus = NormalizeStatus(request.PayrollPeriodStatus);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status payroll period berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/lock")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Update", "Lock Payroll Period", Description = "Mengunci atau membuka payroll period", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("PayrollPeriod", "Update")]
        public async Task<IActionResult> UpdatePayrollPeriodLock(
            Guid id,
            [FromBody] UpdatePayrollPeriodLockRequest request)
        {
            var entity = await _dbContext.Set<MstPayrollPeriod>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Payroll period tidak ditemukan."));
            }

            if (request.IsLocked &&
                !entity.PayrollPeriodStatus.Equals("Approved", StringComparison.OrdinalIgnoreCase) &&
                !entity.PayrollPeriodStatus.Equals("Closed", StringComparison.OrdinalIgnoreCase) &&
                !entity.PayrollPeriodStatus.Equals("Posted", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Payroll period hanya dapat dikunci setelah berstatus Approved, Closed, atau Posted."));
            }

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();

            entity.IsLocked = request.IsLocked;
            entity.LockedAt = request.IsLocked ? now : null;
            entity.LockedByUserId = request.IsLocked ? actor : null;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                request.IsLocked
                    ? "Payroll period berhasil dikunci."
                    : "Kunci payroll period berhasil dibuka."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Delete", "Delete Payroll Period", Description = "Menghapus payroll period", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("PayrollPeriod", "Delete")]
        public async Task<IActionResult> DeletePayrollPeriod(Guid id)
        {
            var entity = await _dbContext.Set<MstPayrollPeriod>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Payroll period tidak ditemukan."));
            }

            if (entity.IsLocked)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Payroll period yang terkunci tidak dapat dihapus."));
            }

            var isUsed = await _dbContext.Set<WfpPayroll>()
                .AsNoTracking()
                .AnyAsync(x => x.LastPayrollPeriodId == id && !x.IsDelete);

            if (isUsed)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Payroll period tidak dapat dihapus karena sudah digunakan pada workforce payroll."));
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
                "Payroll period berhasil dihapus."));
        }

        private IQueryable<MstPayrollPeriod> BuildBaseQuery()
        {
            return _dbContext.Set<MstPayrollPeriod>()
                .AsNoTracking()
                .Include(x => x.LegalEntity)
                .Include(x => x.HospitalSite)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstPayrollPeriod> ApplyDateFilter(
            IQueryable<MstPayrollPeriod> query,
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            var range = ResolveDateRange(startDate, endDate, customPeriod);

            if (range.Start.HasValue)
                query = query.Where(x => x.StartDate >= range.Start.Value.Date);

            if (range.EndExclusive.HasValue)
                query = query.Where(x => x.StartDate < range.EndExclusive.Value.Date);

            return query;
        }

        private static IQueryable<MstPayrollPeriod> ApplyStandardFilter(
            IQueryable<MstPayrollPeriod> query,
            Guid? legalEntityId,
            Guid? hospitalSiteId,
            string? periodType,
            int? fiscalYear,
            string? payrollPeriodStatus,
            bool? isLocked,
            bool? isActive,
            string? search)
        {
            if (legalEntityId.HasValue && legalEntityId.Value != Guid.Empty)
                query = query.Where(x => x.LegalEntityId == legalEntityId.Value);

            if (hospitalSiteId.HasValue && hospitalSiteId.Value != Guid.Empty)
                query = query.Where(x => x.HospitalSiteId == hospitalSiteId.Value);

            if (!string.IsNullOrWhiteSpace(periodType))
                query = query.Where(x => x.PeriodType == periodType.Trim());

            if (fiscalYear.HasValue)
                query = query.Where(x => x.FiscalYear == fiscalYear.Value);

            if (!string.IsNullOrWhiteSpace(payrollPeriodStatus))
                query = query.Where(x => x.PayrollPeriodStatus == payrollPeriodStatus.Trim());

            if (isLocked.HasValue)
                query = query.Where(x => x.IsLocked == isLocked.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.PayrollPeriodCode.ToLower().Contains(keyword) ||
                    x.PayrollPeriodName.ToLower().Contains(keyword) ||
                    x.PeriodType.ToLower().Contains(keyword) ||
                    x.PayrollPeriodStatus.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<MstPayrollPeriod> ApplySorting(
            IQueryable<MstPayrollPeriod> query,
            string? sortBy,
            string? sortDirection)
        {
            var descending = string.Equals(
                sortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "startDate").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => descending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "payrollperiodcode" => descending
                    ? query.OrderByDescending(x => x.PayrollPeriodCode)
                    : query.OrderBy(x => x.PayrollPeriodCode),

                "payrollperiodname" => descending
                    ? query.OrderByDescending(x => x.PayrollPeriodName)
                    : query.OrderBy(x => x.PayrollPeriodName),

                "paymentdate" => descending
                    ? query.OrderByDescending(x => x.PaymentDate)
                    : query.OrderBy(x => x.PaymentDate),

                "payrollperiodstatus" => descending
                    ? query.OrderByDescending(x => x.PayrollPeriodStatus)
                    : query.OrderBy(x => x.PayrollPeriodStatus),

                "islocked" => descending
                    ? query.OrderByDescending(x => x.IsLocked)
                    : query.OrderBy(x => x.IsLocked),

                "isactive" => descending
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => descending
                    ? query.OrderByDescending(x => x.StartDate)
                    : query.OrderBy(x => x.StartDate)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreatePayrollPeriodRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PayrollPeriodName))
                return (false, "Nama payroll period wajib diisi.");

            if (!AllowedPeriodTypes.Contains(request.PeriodType.Trim()))
                return (false, "PeriodType tidak valid.");

            if (!AllowedStatuses.Contains(request.PayrollPeriodStatus.Trim()))
                return (false, "PayrollPeriodStatus tidak valid.");

            if (request.FiscalYear < 2000 || request.FiscalYear > 2200)
                return (false, "FiscalYear tidak valid.");

            if (request.PeriodNumber <= 0)
                return (false, "PeriodNumber harus lebih besar dari 0.");

            if (request.EndDate.Date < request.StartDate.Date)
                return (false, "EndDate tidak boleh lebih kecil dari StartDate.");

            if (request.AttendanceCutoffDate.HasValue &&
                request.AttendanceCutoffDate.Value.Date > request.EndDate.Date)
            {
                return (false, "AttendanceCutoffDate tidak boleh melewati EndDate.");
            }

            if (request.VariableInputCutoffDate.HasValue &&
                request.VariableInputCutoffDate.Value.Date > request.EndDate.Date)
            {
                return (false, "VariableInputCutoffDate tidak boleh melewati EndDate.");
            }

            var legalEntityId = NormalizeGuid(request.LegalEntityId);
            var hospitalSiteId = NormalizeGuid(request.HospitalSiteId);

            if (legalEntityId.HasValue)
            {
                var exists = await _dbContext.Set<MstLegalEntity>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == legalEntityId.Value &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!exists)
                    return (false, "Legal entity tidak ditemukan atau tidak aktif.");
            }

            if (hospitalSiteId.HasValue)
            {
                var exists = await _dbContext.Set<MstHospitalSite>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == hospitalSiteId.Value &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!exists)
                    return (false, "Hospital site tidak ditemukan atau tidak aktif.");
            }

            var normalizedName = request.PayrollPeriodName.Trim().ToLower();
            var normalizedType = request.PeriodType.Trim();

            var duplicateQuery = _dbContext.Set<MstPayrollPeriod>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.PayrollPeriodName.ToLower() == normalizedName &&
                    x.PeriodType == normalizedType &&
                    x.FiscalYear == request.FiscalYear &&
                    x.PeriodNumber == request.PeriodNumber &&
                    x.LegalEntityId == legalEntityId &&
                    x.HospitalSiteId == hospitalSiteId);

            if (excludeId.HasValue)
                duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateQuery.AnyAsync())
                return (false, "Payroll period dengan periode dan scope tersebut sudah tersedia.");

            var overlapQuery = _dbContext.Set<MstPayrollPeriod>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.PeriodType == normalizedType &&
                    x.LegalEntityId == legalEntityId &&
                    x.HospitalSiteId == hospitalSiteId &&
                    x.StartDate <= request.EndDate.Date &&
                    x.EndDate >= request.StartDate.Date);

            if (excludeId.HasValue)
                overlapQuery = overlapQuery.Where(x => x.Id != excludeId.Value);

            if (await overlapQuery.AnyAsync())
                return (false, "Rentang payroll period bertabrakan dengan periode lain pada scope yang sama.");

            return (true, null);
        }

        private async Task<string> GenerateCodeAsync()
        {
            var codes = await _dbContext.Set<MstPayrollPeriod>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.PayrollPeriodCode.StartsWith(CodePrefix))
                .Select(x => x.PayrollPeriodCode)
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

        private static string NormalizePeriodType(string value)
        {
            return AllowedPeriodTypes.First(x =>
                x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static string NormalizeStatus(string value)
        {
            return AllowedStatuses.First(x =>
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

        private static string BuildPeriodTypeLabel(string value)
        {
            return value switch
            {
                "Monthly" => "Bulanan",
                "BiWeekly" => "Dua Mingguan",
                "Weekly" => "Mingguan",
                "Special" => "Khusus",
                "Adjustment" => "Penyesuaian",
                _ => value
            };
        }

        private static string BuildStatusLabel(string value)
        {
            return value switch
            {
                "Draft" => "Draft",
                "Open" => "Dibuka",
                "Processing" => "Diproses",
                "Review" => "Review",
                "Approved" => "Disetujui",
                "Closed" => "Ditutup",
                "Posted" => "Diposting",
                "Cancelled" => "Dibatalkan",
                _ => value
            };
        }
    }
}
