using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Globalization;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/transport-allowances")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Workforce Transport Allowance",
        AreaName = "Corporate",
        ControllerName = "WorkforceTransportAllowance",
        Description = "Workforce transport allowance management",
        SortOrder = 27
    )]
    [Tags("Corporate / Human Resource / Workforce / Transport Allowance")]
    public class WorkforceTransportAllowanceController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Workforce.TransportAllowance";

        private static readonly string[] AllowanceModes =
        {
            "None",
            "FixedMonthly",
            "DailyAttendance",
            "NightShift",
            "MonthlyAndNightShift",
            "Manual"
        };

        private static readonly string[] AllowanceTypes =
        {
            "Regular",
            "Monthly",
            "Night",
            "Adjustment",
            "Manual"
        };

        private static readonly string[] TransactionStatuses =
        {
            "Draft",
            "Calculated",
            "Approved",
            "PostedToPayroll",
            "Cancelled"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WorkforceTransportAllowanceController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<WorkforceTransportAllowanceListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Workforce Transport Allowance",
            Description = "Melihat transport allowance workforce profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceTransportAllowance", "Read")]
        public async Task<IActionResult> GetTransportAllowances(Guid workforceProfileId)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var items = await _dbContext.Set<WfpTransportAllowance>()
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete)
                .OrderByDescending(x => x.IsActive)
                .ThenByDescending(x => x.IsEligible)
                .ThenBy(x => x.EffectiveStartDate)
                .Select(x => new WorkforceTransportAllowanceResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = profile.ProfileCode,
                    DisplayName = profile.DisplayName,
                    TransportAllowancePolicyId = x.TransportAllowancePolicyId,
                    PolicyCode = x.TransportAllowancePolicy != null
                        ? x.TransportAllowancePolicy.PolicyCode
                        : null,
                    PolicyName = x.TransportAllowancePolicy != null
                        ? x.TransportAllowancePolicy.PolicyName
                        : null,
                    IsEligible = x.IsEligible,
                    IsRegularTransportEligible = x.IsRegularTransportEligible,
                    IsNightTransportEligible = x.IsNightTransportEligible,
                    AllowanceMode = x.AllowanceMode,
                    MonthlyAmount = x.MonthlyAmount,
                    DailyAmount = x.DailyAmount,
                    NightAmount = x.NightAmount,
                    IsProrated = x.IsProrated,
                    IsTaxable = x.IsTaxable,
                    IsPayrollComponent = x.IsPayrollComponent,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new WorkforceTransportAllowanceListResponse
            {
                WorkforceProfileId = profile.Id,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                TotalData = items.Count,
                ActiveData = items.Count(x => x.IsActive),
                EligibleData = items.Count(x => x.IsEligible),
                RegularEligibleData = items.Count(x => x.IsRegularTransportEligible),
                NightEligibleData = items.Count(x => x.IsNightTransportEligible),
                Items = items
            };

            return Ok(ApiResponse<WorkforceTransportAllowanceListResponse>.Ok(
                result,
                "Data transport allowance workforce berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WorkforceTransportAllowanceResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Create",
            "Create Workforce Transport Allowance",
            Description = "Menambah transport allowance workforce profile",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("WorkforceTransportAllowance", "Create")]
        public async Task<IActionResult> CreateTransportAllowance(
            Guid workforceProfileId,
            [FromBody] CreateWorkforceTransportAllowanceRequest request)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var validation = await ValidateTransportAllowanceRequestAsync(
                request.TransportAllowancePolicyId,
                request.AllowanceMode,
                request.EffectiveStartDate,
                request.EffectiveEndDate,
                request.MonthlyAmount,
                request.DailyAmount,
                request.NightAmount
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data transport allowance tidak valid."
                ));
            }

            var duplicate = await _dbContext.Set<WfpTransportAllowance>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (duplicate)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Transport allowance workforce profile ini sudah tersedia. Gunakan endpoint update."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new WfpTransportAllowance
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                TransportAllowancePolicyId = NormalizeNullableGuid(request.TransportAllowancePolicyId),
                IsEligible = request.IsEligible,
                IsRegularTransportEligible = request.IsRegularTransportEligible,
                IsNightTransportEligible = request.IsNightTransportEligible,
                AllowanceMode = request.AllowanceMode.Trim(),
                MonthlyAmount = request.MonthlyAmount,
                DailyAmount = request.DailyAmount,
                NightAmount = request.NightAmount,
                IsProrated = request.IsProrated,
                IsTaxable = request.IsTaxable,
                IsPayrollComponent = request.IsPayrollComponent,
                EffectiveStartDate = request.EffectiveStartDate.Date,
                EffectiveEndDate = request.EffectiveEndDate?.Date,
                Description = NormalizeNullableText(request.Description),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<WfpTransportAllowance>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = await BuildTransportAllowanceResponseAsync(entity.Id);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceTransportAllowance.CreateTransportAllowance",
                "Transport allowance workforce berhasil dibuat.",
                new
                {
                    WorkforceProfileId = workforceProfileId,
                    entity.Id,
                    entity.AllowanceMode,
                    entity.IsEligible,
                    entity.IsRegularTransportEligible,
                    entity.IsNightTransportEligible
                }
            );

            return Ok(ApiResponse<WorkforceTransportAllowanceResponse>.Ok(
                response!,
                "Transport allowance workforce berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceTransportAllowanceResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Transport Allowance",
            Description = "Mengubah transport allowance workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceTransportAllowance", "Update")]
        public async Task<IActionResult> UpdateTransportAllowance(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWorkforceTransportAllowanceRequest request)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var entity = await _dbContext.Set<WfpTransportAllowance>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Transport allowance workforce tidak ditemukan."
                ));
            }

            var validation = await ValidateTransportAllowanceRequestAsync(
                request.TransportAllowancePolicyId,
                request.AllowanceMode,
                request.EffectiveStartDate,
                request.EffectiveEndDate,
                request.MonthlyAmount,
                request.DailyAmount,
                request.NightAmount
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data transport allowance tidak valid."
                ));
            }

            entity.TransportAllowancePolicyId = NormalizeNullableGuid(request.TransportAllowancePolicyId);
            entity.IsEligible = request.IsEligible;
            entity.IsRegularTransportEligible = request.IsRegularTransportEligible;
            entity.IsNightTransportEligible = request.IsNightTransportEligible;
            entity.AllowanceMode = request.AllowanceMode.Trim();
            entity.MonthlyAmount = request.MonthlyAmount;
            entity.DailyAmount = request.DailyAmount;
            entity.NightAmount = request.NightAmount;
            entity.IsProrated = request.IsProrated;
            entity.IsTaxable = request.IsTaxable;
            entity.IsPayrollComponent = request.IsPayrollComponent;
            entity.EffectiveStartDate = request.EffectiveStartDate.Date;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var response = await BuildTransportAllowanceResponseAsync(entity.Id);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceTransportAllowance.UpdateTransportAllowance",
                "Transport allowance workforce berhasil diperbarui.",
                new
                {
                    WorkforceProfileId = workforceProfileId,
                    entity.Id,
                    entity.AllowanceMode,
                    entity.IsEligible,
                    entity.IsRegularTransportEligible,
                    entity.IsNightTransportEligible
                }
            );

            return Ok(ApiResponse<WorkforceTransportAllowanceResponse>.Ok(
                response!,
                "Transport allowance workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Transport Allowance",
            Description = "Mengubah status transport allowance workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceTransportAllowance", "Update")]
        public async Task<IActionResult> UpdateTransportAllowanceStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWorkforceTransportAllowanceStatusRequest request)
        {
            var entity = await _dbContext.Set<WfpTransportAllowance>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Transport allowance workforce tidak ditemukan."
                ));
            }

            entity.IsActive = request.IsActive;

            if (request.EffectiveEndDate.HasValue)
            {
                entity.EffectiveEndDate = request.EffectiveEndDate.Value.Date;
            }

            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                entity.Description = NormalizeNullableText(request.Description);
            }

            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status transport allowance workforce berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Workforce Transport Allowance",
            Description = "Menghapus transport allowance workforce",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("WorkforceTransportAllowance", "Delete")]
        public async Task<IActionResult> DeleteTransportAllowance(
            Guid workforceProfileId,
            Guid id)
        {
            var entity = await _dbContext.Set<WfpTransportAllowance>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Transport allowance workforce tidak ditemukan."
                ));
            }

            entity.IsActive = false;
            entity.IsDelete = true;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Transport allowance workforce berhasil dihapus."
            ));
        }

        [HttpGet("transactions")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceTransportAllowanceTransactionListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Workforce Transport Allowance",
            Description = "Melihat transaksi transport allowance workforce profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceTransportAllowance", "Read")]
        public async Task<IActionResult> GetTransactions(
            Guid workforceProfileId,
            [FromQuery] string? periodYearMonth,
            [FromQuery] string? status,
            [FromQuery] string? allowanceType,
            [FromQuery] bool? isNightShift)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var query = _dbContext.Set<WfpTransportAllowanceTransaction>()
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(periodYearMonth))
            {
                query = query.Where(x => x.PeriodYearMonth == periodYearMonth.Trim());
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x => x.TransactionStatus == status.Trim());
            }

            if (!string.IsNullOrWhiteSpace(allowanceType))
            {
                query = query.Where(x => x.AllowanceType == allowanceType.Trim());
            }

            if (isNightShift.HasValue)
            {
                query = query.Where(x => x.IsNightShift == isNightShift.Value);
            }

            var items = await query
                .OrderByDescending(x => x.TransactionDate)
                .ThenByDescending(x => x.CreateDateTime)
                .Select(x => new WorkforceTransportAllowanceTransactionResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = profile.ProfileCode,
                    DisplayName = profile.DisplayName,
                    TransportAllowanceId = x.TransportAllowanceId,
                    TransportAllowancePolicyId = x.TransportAllowancePolicyId,
                    AttendanceId = x.AttendanceId,
                    TransactionDate = x.TransactionDate,
                    PeriodYearMonth = x.PeriodYearMonth,
                    AllowanceType = x.AllowanceType,
                    Amount = x.Amount,
                    IsGeneratedFromAttendance = x.IsGeneratedFromAttendance,
                    IsNightShift = x.IsNightShift,
                    TransactionStatus = x.TransactionStatus,
                    Notes = x.Notes,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new WorkforceTransportAllowanceTransactionListResponse
            {
                WorkforceProfileId = profile.Id,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                TotalData = items.Count,
                ActiveData = items.Count(x => x.IsActive),
                DraftData = items.Count(x => x.TransactionStatus == "Draft"),
                CalculatedData = items.Count(x => x.TransactionStatus == "Calculated"),
                ApprovedData = items.Count(x => x.TransactionStatus == "Approved"),
                PostedToPayrollData = items.Count(x => x.TransactionStatus == "PostedToPayroll"),
                CancelledData = items.Count(x => x.TransactionStatus == "Cancelled"),
                TotalAmount = items
                    .Where(x => x.TransactionStatus != "Cancelled")
                    .Sum(x => x.Amount),
                Items = items
            };

            return Ok(ApiResponse<WorkforceTransportAllowanceTransactionListResponse>.Ok(
                result,
                "Data transaksi transport allowance workforce berhasil diambil."
            ));
        }

        [HttpPost("transactions")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceTransportAllowanceTransactionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Create",
            "Create Workforce Transport Allowance",
            Description = "Menambah transaksi transport allowance workforce profile",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("WorkforceTransportAllowance", "Create")]
        public async Task<IActionResult> CreateTransaction(
            Guid workforceProfileId,
            [FromBody] CreateWorkforceTransportAllowanceTransactionRequest request)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var validation = await ValidateTransactionRequestAsync(workforceProfileId, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data transaksi transport allowance tidak valid."
                ));
            }

            var transactionDate = request.TransactionDate.Date;

            var entity = new WfpTransportAllowanceTransaction
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                TransportAllowanceId = NormalizeNullableGuid(request.TransportAllowanceId),
                TransportAllowancePolicyId = NormalizeNullableGuid(request.TransportAllowancePolicyId),
                AttendanceId = NormalizeNullableGuid(request.AttendanceId),
                TransactionDate = transactionDate,
                PeriodYearMonth = ResolvePeriodYearMonth(request.PeriodYearMonth, transactionDate),
                AllowanceType = request.AllowanceType.Trim(),
                Amount = request.Amount,
                IsGeneratedFromAttendance = request.IsGeneratedFromAttendance,
                IsNightShift = request.IsNightShift,
                TransactionStatus = string.IsNullOrWhiteSpace(request.TransactionStatus)
                    ? "Draft"
                    : request.TransactionStatus.Trim(),
                Notes = NormalizeNullableText(request.Notes),
                IsActive = request.IsActive,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<WfpTransportAllowanceTransaction>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = await BuildTransactionResponseAsync(entity.Id);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceTransportAllowance.CreateTransaction",
                "Transaksi transport allowance workforce berhasil dibuat.",
                new
                {
                    WorkforceProfileId = workforceProfileId,
                    entity.Id,
                    entity.AllowanceType,
                    entity.Amount,
                    entity.TransactionStatus,
                    entity.IsNightShift
                }
            );

            return Ok(ApiResponse<WorkforceTransportAllowanceTransactionResponse>.Ok(
                response!,
                "Transaksi transport allowance workforce berhasil dibuat."
            ));
        }

        [HttpPatch("transactions/{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Transport Allowance",
            Description = "Mengubah status transaksi transport allowance workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceTransportAllowance", "Update")]
        public async Task<IActionResult> UpdateTransactionStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWorkforceTransportAllowanceTransactionStatusRequest request)
        {
            var entity = await _dbContext.Set<WfpTransportAllowanceTransaction>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Transaksi transport allowance workforce tidak ditemukan."
                ));
            }

            var status = request.TransactionStatus.Trim();

            if (!TransactionStatuses.Contains(status))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "TransactionStatus tidak valid."
                ));
            }

            entity.TransactionStatus = status;
            entity.IsActive = request.IsActive;

            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                entity.Notes = NormalizeNullableText(request.Notes);
            }

            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status transaksi transport allowance workforce berhasil diperbarui."
            ));
        }

        [HttpDelete("transactions/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Workforce Transport Allowance",
            Description = "Menghapus transaksi transport allowance workforce profile",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("WorkforceTransportAllowance", "Delete")]
        public async Task<IActionResult> DeleteTransaction(
            Guid workforceProfileId,
            Guid id)
        {
            var entity = await _dbContext.Set<WfpTransportAllowanceTransaction>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Transaksi transport allowance workforce tidak ditemukan."
                ));
            }

            entity.IsActive = false;
            entity.IsDelete = true;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Transaksi transport allowance workforce berhasil dihapus."
            ));
        }

        private async Task<MstWorkforceProfile?> GetProfileAsync(Guid workforceProfileId)
        {
            return await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == workforceProfileId &&
                    !x.IsDelete);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateTransportAllowanceRequestAsync(
            Guid? transportAllowancePolicyId,
            string? allowanceMode,
            DateTime effectiveStartDate,
            DateTime? effectiveEndDate,
            decimal monthlyAmount,
            decimal dailyAmount,
            decimal nightAmount)
        {
            if (string.IsNullOrWhiteSpace(allowanceMode))
            {
                return (false, "AllowanceMode wajib diisi.");
            }

            if (!AllowanceModes.Contains(allowanceMode.Trim()))
            {
                return (false, "AllowanceMode tidak valid.");
            }

            if (effectiveStartDate == default)
            {
                return (false, "EffectiveStartDate wajib diisi.");
            }

            if (effectiveEndDate.HasValue &&
                effectiveEndDate.Value.Date < effectiveStartDate.Date)
            {
                return (false, "EffectiveEndDate tidak boleh lebih kecil dari EffectiveStartDate.");
            }

            if (monthlyAmount < 0 ||
                dailyAmount < 0 ||
                nightAmount < 0)
            {
                return (false, "Nominal transport allowance tidak boleh negatif.");
            }

            if (transportAllowancePolicyId.HasValue &&
                transportAllowancePolicyId.Value != Guid.Empty)
            {
                var exists = await _dbContext.Set<WfpTransportAllowancePolicy>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == transportAllowancePolicyId.Value &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!exists)
                {
                    return (false, "TransportAllowancePolicyId tidak valid atau tidak aktif.");
                }
            }

            return (true, null);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateTransactionRequestAsync(
            Guid workforceProfileId,
            CreateWorkforceTransportAllowanceTransactionRequest request)
        {
            if (request.TransactionDate == default)
            {
                return (false, "TransactionDate wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(request.AllowanceType))
            {
                return (false, "AllowanceType wajib diisi.");
            }

            if (!AllowanceTypes.Contains(request.AllowanceType.Trim()))
            {
                return (false, "AllowanceType tidak valid.");
            }

            if (request.Amount < 0)
            {
                return (false, "Amount tidak boleh negatif.");
            }

            if (!string.IsNullOrWhiteSpace(request.TransactionStatus) &&
                !TransactionStatuses.Contains(request.TransactionStatus.Trim()))
            {
                return (false, "TransactionStatus tidak valid.");
            }

            if (request.TransportAllowanceId.HasValue &&
                request.TransportAllowanceId.Value != Guid.Empty)
            {
                var validTransportAllowance = await _dbContext.Set<WfpTransportAllowance>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == request.TransportAllowanceId.Value &&
                        x.WorkforceProfileId == workforceProfileId &&
                        !x.IsDelete);

                if (!validTransportAllowance)
                {
                    return (false, "TransportAllowanceId tidak valid untuk workforce profile ini.");
                }
            }

            if (request.TransportAllowancePolicyId.HasValue &&
                request.TransportAllowancePolicyId.Value != Guid.Empty)
            {
                var validPolicy = await _dbContext.Set<WfpTransportAllowancePolicy>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == request.TransportAllowancePolicyId.Value &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!validPolicy)
                {
                    return (false, "TransportAllowancePolicyId tidak valid atau tidak aktif.");
                }
            }

            return (true, null);
        }

        private async Task<WorkforceTransportAllowanceResponse?> BuildTransportAllowanceResponseAsync(Guid id)
        {
            return await _dbContext.Set<WfpTransportAllowance>()
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    !x.IsDelete)
                .Select(x => new WorkforceTransportAllowanceResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = x.WorkforceProfile != null
                        ? x.WorkforceProfile.ProfileCode
                        : string.Empty,
                    DisplayName = x.WorkforceProfile != null
                        ? x.WorkforceProfile.DisplayName
                        : string.Empty,
                    TransportAllowancePolicyId = x.TransportAllowancePolicyId,
                    PolicyCode = x.TransportAllowancePolicy != null
                        ? x.TransportAllowancePolicy.PolicyCode
                        : null,
                    PolicyName = x.TransportAllowancePolicy != null
                        ? x.TransportAllowancePolicy.PolicyName
                        : null,
                    IsEligible = x.IsEligible,
                    IsRegularTransportEligible = x.IsRegularTransportEligible,
                    IsNightTransportEligible = x.IsNightTransportEligible,
                    AllowanceMode = x.AllowanceMode,
                    MonthlyAmount = x.MonthlyAmount,
                    DailyAmount = x.DailyAmount,
                    NightAmount = x.NightAmount,
                    IsProrated = x.IsProrated,
                    IsTaxable = x.IsTaxable,
                    IsPayrollComponent = x.IsPayrollComponent,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();
        }

        private async Task<WorkforceTransportAllowanceTransactionResponse?> BuildTransactionResponseAsync(Guid id)
        {
            return await _dbContext.Set<WfpTransportAllowanceTransaction>()
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    !x.IsDelete)
                .Select(x => new WorkforceTransportAllowanceTransactionResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = x.WorkforceProfile != null
                        ? x.WorkforceProfile.ProfileCode
                        : string.Empty,
                    DisplayName = x.WorkforceProfile != null
                        ? x.WorkforceProfile.DisplayName
                        : string.Empty,
                    TransportAllowanceId = x.TransportAllowanceId,
                    TransportAllowancePolicyId = x.TransportAllowancePolicyId,
                    AttendanceId = x.AttendanceId,
                    TransactionDate = x.TransactionDate,
                    PeriodYearMonth = x.PeriodYearMonth,
                    AllowanceType = x.AllowanceType,
                    Amount = x.Amount,
                    IsGeneratedFromAttendance = x.IsGeneratedFromAttendance,
                    IsNightShift = x.IsNightShift,
                    TransactionStatus = x.TransactionStatus,
                    Notes = x.Notes,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();
        }

        private Guid GetCurrentUserId()
        {
            var userIdText =
                User.FindFirstValue("user_id") ??
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
        }

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            if (!value.HasValue || value.Value == Guid.Empty)
            {
                return null;
            }

            return value.Value;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static string ResolvePeriodYearMonth(
            string? value,
            DateTime transactionDate)
        {
            return string.IsNullOrWhiteSpace(value)
                ? transactionDate.ToString("yyyy-MM", CultureInfo.InvariantCulture)
                : value.Trim();
        }
    }
}