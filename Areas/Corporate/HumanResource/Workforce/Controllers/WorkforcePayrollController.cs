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
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/payrolls")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Workforce Payroll",
        AreaName = "Corporate",
        ControllerName = "WorkforcePayroll",
        Description = "Workforce payroll management",
        SortOrder = 28
    )]
    [Tags("Corporate / Human Resource / Workforce / Payroll")]
    public class WorkforcePayrollController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Workforce.Payroll";

        private static readonly string[] PaymentMethods =
        {
            "Cash",
            "BankTransfer",
            "Cheque",
            "Other"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WorkforcePayrollController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<WorkforcePayrollListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Workforce Payroll",
            Description = "Melihat payroll workforce profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforcePayroll", "Read")]
        public async Task<IActionResult> GetPayrolls(Guid workforceProfileId)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var items = await _dbContext.Set<WfpPayroll>()
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete)
                .OrderByDescending(x => x.IsActive)
                .ThenByDescending(x => x.IsPayrollActive)
                .ThenByDescending(x => x.EffectiveStartDate)
                .Select(x => new WorkforcePayrollResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = profile.ProfileCode,
                    DisplayName = profile.DisplayName,
                    PayrollGroup = x.PayrollGroup,
                    PaymentMethod = x.PaymentMethod,
                    PrimaryBankAccountId = x.PrimaryBankAccountId,
                    PrimaryBankName = x.PrimaryBankAccount != null ? x.PrimaryBankAccount.BankName : null,
                    PrimaryBankAccountNumber = x.PrimaryBankAccount != null ? x.PrimaryBankAccount.AccountNumber : null,
                    PrimaryBankAccountHolderName = x.PrimaryBankAccount != null ? x.PrimaryBankAccount.AccountHolderName : null,
                    BasicSalary = x.BasicSalary,
                    FixedAllowance = x.FixedAllowance,
                    FixedDeduction = x.FixedDeduction,
                    NetFixedAmount = x.BasicSalary + x.FixedAllowance - x.FixedDeduction,
                    IsOvertimeEligible = x.IsOvertimeEligible,
                    IsPayrollActive = x.IsPayrollActive,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new WorkforcePayrollListResponse
            {
                WorkforceProfileId = profile.Id,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                TotalData = items.Count,
                ActiveData = items.Count(x => x.IsActive),
                PayrollActiveData = items.Count(x => x.IsPayrollActive),
                TotalBasicSalary = items.Where(x => x.IsActive).Sum(x => x.BasicSalary),
                TotalFixedAllowance = items.Where(x => x.IsActive).Sum(x => x.FixedAllowance),
                TotalFixedDeduction = items.Where(x => x.IsActive).Sum(x => x.FixedDeduction),
                Items = items
            };

            return Ok(ApiResponse<WorkforcePayrollListResponse>.Ok(
                result,
                "Data payroll workforce berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WorkforcePayrollResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Create",
            "Create Workforce Payroll",
            Description = "Menambah payroll workforce profile",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("WorkforcePayroll", "Create")]
        public async Task<IActionResult> CreatePayroll(
            Guid workforceProfileId,
            [FromBody] CreateWorkforcePayrollRequest request)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var validation = await ValidatePayrollRequestAsync(
                workforceProfileId,
                request.PayrollGroup,
                request.PaymentMethod,
                request.PrimaryBankAccountId,
                request.BasicSalary,
                request.FixedAllowance,
                request.FixedDeduction,
                request.EffectiveStartDate,
                request.EffectiveEndDate
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data payroll tidak valid."
                ));
            }

            var duplicate = await _dbContext.Set<WfpPayroll>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (duplicate)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Payroll workforce profile ini sudah tersedia. Gunakan endpoint update."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new WfpPayroll
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                PayrollGroup = request.PayrollGroup.Trim(),
                PaymentMethod = request.PaymentMethod.Trim(),
                PrimaryBankAccountId = NormalizeNullableGuid(request.PrimaryBankAccountId),
                BasicSalary = request.BasicSalary,
                FixedAllowance = request.FixedAllowance,
                FixedDeduction = request.FixedDeduction,
                IsOvertimeEligible = request.IsOvertimeEligible,
                IsPayrollActive = request.IsPayrollActive,
                EffectiveStartDate = request.EffectiveStartDate.Date,
                EffectiveEndDate = request.EffectiveEndDate?.Date,
                Description = NormalizeNullableText(request.Description),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<WfpPayroll>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = await BuildPayrollResponseAsync(entity.Id);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforcePayroll.CreatePayroll",
                "Payroll workforce berhasil dibuat.",
                new
                {
                    WorkforceProfileId = workforceProfileId,
                    entity.Id,
                    entity.PayrollGroup,
                    entity.PaymentMethod,
                    entity.BasicSalary,
                    entity.IsPayrollActive
                }
            );

            return Ok(ApiResponse<WorkforcePayrollResponse>.Ok(
                response!,
                "Payroll workforce berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforcePayrollResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Payroll",
            Description = "Mengubah payroll workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforcePayroll", "Update")]
        public async Task<IActionResult> UpdatePayroll(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWorkforcePayrollRequest request)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var entity = await _dbContext.Set<WfpPayroll>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Payroll workforce tidak ditemukan."
                ));
            }

            var validation = await ValidatePayrollRequestAsync(
                workforceProfileId,
                request.PayrollGroup,
                request.PaymentMethod,
                request.PrimaryBankAccountId,
                request.BasicSalary,
                request.FixedAllowance,
                request.FixedDeduction,
                request.EffectiveStartDate,
                request.EffectiveEndDate
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data payroll tidak valid."
                ));
            }

            entity.PayrollGroup = request.PayrollGroup.Trim();
            entity.PaymentMethod = request.PaymentMethod.Trim();
            entity.PrimaryBankAccountId = NormalizeNullableGuid(request.PrimaryBankAccountId);
            entity.BasicSalary = request.BasicSalary;
            entity.FixedAllowance = request.FixedAllowance;
            entity.FixedDeduction = request.FixedDeduction;
            entity.IsOvertimeEligible = request.IsOvertimeEligible;
            entity.IsPayrollActive = request.IsPayrollActive;
            entity.EffectiveStartDate = request.EffectiveStartDate.Date;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var response = await BuildPayrollResponseAsync(entity.Id);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforcePayroll.UpdatePayroll",
                "Payroll workforce berhasil diperbarui.",
                new
                {
                    WorkforceProfileId = workforceProfileId,
                    entity.Id,
                    entity.PayrollGroup,
                    entity.PaymentMethod,
                    entity.BasicSalary,
                    entity.IsPayrollActive
                }
            );

            return Ok(ApiResponse<WorkforcePayrollResponse>.Ok(
                response!,
                "Payroll workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Payroll",
            Description = "Mengubah status payroll workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforcePayroll", "Update")]
        public async Task<IActionResult> UpdatePayrollStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWorkforcePayrollStatusRequest request)
        {
            var entity = await _dbContext.Set<WfpPayroll>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Payroll workforce tidak ditemukan."
                ));
            }

            entity.IsActive = request.IsActive;
            entity.IsPayrollActive = request.IsPayrollActive;

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
                "Status payroll workforce berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Workforce Payroll",
            Description = "Menghapus payroll workforce",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("WorkforcePayroll", "Delete")]
        public async Task<IActionResult> DeletePayroll(
            Guid workforceProfileId,
            Guid id)
        {
            var entity = await _dbContext.Set<WfpPayroll>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Payroll workforce tidak ditemukan."
                ));
            }

            entity.IsActive = false;
            entity.IsPayrollActive = false;
            entity.IsDelete = true;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Payroll workforce berhasil dihapus."
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

        private async Task<(bool IsValid, string? ErrorMessage)> ValidatePayrollRequestAsync(
            Guid workforceProfileId,
            string? payrollGroup,
            string? paymentMethod,
            Guid? primaryBankAccountId,
            decimal basicSalary,
            decimal fixedAllowance,
            decimal fixedDeduction,
            DateTime effectiveStartDate,
            DateTime? effectiveEndDate)
        {
            if (string.IsNullOrWhiteSpace(payrollGroup))
            {
                return (false, "PayrollGroup wajib diisi.");
            }

            if (payrollGroup.Trim().Length > 50)
            {
                return (false, "PayrollGroup maksimal 50 karakter.");
            }

            if (string.IsNullOrWhiteSpace(paymentMethod))
            {
                return (false, "PaymentMethod wajib diisi.");
            }

            if (!PaymentMethods.Contains(paymentMethod.Trim()))
            {
                return (false, "PaymentMethod tidak valid.");
            }

            if (basicSalary < 0 ||
                fixedAllowance < 0 ||
                fixedDeduction < 0)
            {
                return (false, "Nominal payroll tidak boleh negatif.");
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

            if (primaryBankAccountId.HasValue &&
                primaryBankAccountId.Value != Guid.Empty)
            {
                var bankExists = await _dbContext.Set<WfpBankAccount>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == primaryBankAccountId.Value &&
                        x.WorkforceProfileId == workforceProfileId &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!bankExists)
                {
                    return (false, "PrimaryBankAccountId tidak valid atau tidak aktif untuk workforce profile ini.");
                }
            }

            return (true, null);
        }

        private async Task<WorkforcePayrollResponse?> BuildPayrollResponseAsync(Guid id)
        {
            return await _dbContext.Set<WfpPayroll>()
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    !x.IsDelete)
                .Select(x => new WorkforcePayrollResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = x.WorkforceProfile != null
                        ? x.WorkforceProfile.ProfileCode
                        : string.Empty,
                    DisplayName = x.WorkforceProfile != null
                        ? x.WorkforceProfile.DisplayName
                        : string.Empty,
                    PayrollGroup = x.PayrollGroup,
                    PaymentMethod = x.PaymentMethod,
                    PrimaryBankAccountId = x.PrimaryBankAccountId,
                    PrimaryBankName = x.PrimaryBankAccount != null
                        ? x.PrimaryBankAccount.BankName
                        : null,
                    PrimaryBankAccountNumber = x.PrimaryBankAccount != null
                        ? x.PrimaryBankAccount.AccountNumber
                        : null,
                    PrimaryBankAccountHolderName = x.PrimaryBankAccount != null
                        ? x.PrimaryBankAccount.AccountHolderName
                        : null,
                    BasicSalary = x.BasicSalary,
                    FixedAllowance = x.FixedAllowance,
                    FixedDeduction = x.FixedDeduction,
                    NetFixedAmount = x.BasicSalary + x.FixedAllowance - x.FixedDeduction,
                    IsOvertimeEligible = x.IsOvertimeEligible,
                    IsPayrollActive = x.IsPayrollActive,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    Description = x.Description,
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
    }
}