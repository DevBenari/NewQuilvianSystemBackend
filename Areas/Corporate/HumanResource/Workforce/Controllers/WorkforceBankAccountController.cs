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
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/bank-accounts")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Workforce Bank Account",
        AreaName = "Corporate",
        ControllerName = "WorkforceBankAccount",
        Description = "Workforce bank account management",
        SortOrder = 21
    )]
    [Tags("Corporate / Human Resource / Workforce / Bank Account")]
    public class WorkforceBankAccountController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Workforce.BankAccount";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WorkforceBankAccountController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<WorkforceBankAccountListResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Bank Account",
            Description = "Melihat rekening bank workforce profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceBankAccount", "Read")]
        public async Task<IActionResult> GetBankAccounts(Guid workforceProfileId)
        {
            var profile = await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == workforceProfileId && !x.IsDelete);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var items = await _dbContext.Set<WfpBankAccount>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete)
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.IsActive)
                .ThenBy(x => x.BankName)
                .Select(x => new WorkforceBankAccountResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = profile.ProfileCode,
                    DisplayName = profile.DisplayName,
                    BankName = x.BankName,
                    AccountNumber = x.AccountNumber,
                    AccountHolderName = x.AccountHolderName,
                    BankBranch = x.BankBranch,
                    IsPrimary = x.IsPrimary,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new WorkforceBankAccountListResponse
            {
                WorkforceProfileId = profile.Id,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                TotalData = items.Count,
                ActiveData = items.Count(x => x.IsActive),
                PrimaryData = items.Count(x => x.IsPrimary && x.IsActive),
                Items = items
            };

            return Ok(ApiResponse<WorkforceBankAccountListResponse>.Ok(
                result,
                "Data rekening bank workforce berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WorkforceBankAccountResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Workforce Bank Account",
            Description = "Menambah rekening bank workforce profile",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("WorkforceBankAccount", "Create")]
        public async Task<IActionResult> CreateBankAccount(
            Guid workforceProfileId,
            [FromBody] CreateWorkforceBankAccountRequest request)
        {
            var profileExists = await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == workforceProfileId && !x.IsDelete);

            if (!profileExists)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var validation = await ValidateBankAccountRequestAsync(
                workforceProfileId,
                request.BankName,
                request.AccountNumber,
                request.AccountHolderName,
                excludeBankAccountId: null);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data rekening bank tidak valid."
                ));
            }

            if (request.IsPrimary && !request.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Rekening primary harus aktif."
                ));
            }

            var hasAny = await _dbContext.Set<WfpBankAccount>()
                .AnyAsync(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            var shouldBePrimary = request.IsPrimary || !hasAny;

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var entity = new WfpBankAccount
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = workforceProfileId,
                    BankName = request.BankName.Trim(),
                    AccountNumber = NormalizeAccountNumber(request.AccountNumber),
                    AccountHolderName = request.AccountHolderName.Trim(),
                    BankBranch = NormalizeNullableText(request.BankBranch),
                    IsPrimary = false,
                    IsActive = request.IsActive,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<WfpBankAccount>().Add(entity);

                if (shouldBePrimary)
                {
                    await ApplyPrimaryBankAccountAsync(workforceProfileId, entity, now, actorUserId);
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                var response = await BuildBankAccountResponseAsync(entity.Id);

                await _loggerService.InfoAsync(
                    LogCategory,
                    "WorkforceBankAccount.CreateBankAccount",
                    "Rekening bank workforce berhasil dibuat.",
                    new
                    {
                        workforceProfileId,
                        entity.Id,
                        entity.BankName,
                        entity.AccountNumber,
                        entity.IsPrimary
                    }
                );

                return Ok(ApiResponse<WorkforceBankAccountResponse>.Ok(
                    response!,
                    "Rekening bank workforce berhasil dibuat."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceBankAccount.CreateBankAccount",
                    "Gagal membuat rekening bank workforce.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat membuat rekening bank workforce."
                    )
                );
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceBankAccountResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Bank Account",
            Description = "Mengubah rekening bank workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceBankAccount", "Update")]
        public async Task<IActionResult> UpdateBankAccount(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWorkforceBankAccountRequest request)
        {
            var entity = await _dbContext.Set<WfpBankAccount>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Rekening bank workforce tidak ditemukan."
                ));
            }

            if (entity.IsPrimary && !request.IsPrimary)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Rekening primary tidak boleh dilepas langsung. Jadikan rekening lain sebagai primary terlebih dahulu."
                ));
            }

            if ((entity.IsPrimary || request.IsPrimary) && !request.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Rekening primary harus aktif."
                ));
            }

            var validation = await ValidateBankAccountRequestAsync(
                workforceProfileId,
                request.BankName,
                request.AccountNumber,
                request.AccountHolderName,
                excludeBankAccountId: id);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data rekening bank tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                entity.BankName = request.BankName.Trim();
                entity.AccountNumber = NormalizeAccountNumber(request.AccountNumber);
                entity.AccountHolderName = request.AccountHolderName.Trim();
                entity.BankBranch = NormalizeNullableText(request.BankBranch);
                entity.IsActive = request.IsActive;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                if (request.IsPrimary)
                {
                    await ApplyPrimaryBankAccountAsync(workforceProfileId, entity, now, actorUserId);
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                var response = await BuildBankAccountResponseAsync(entity.Id);

                return Ok(ApiResponse<WorkforceBankAccountResponse>.Ok(
                    response!,
                    "Rekening bank workforce berhasil diperbarui."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceBankAccount.UpdateBankAccount",
                    "Gagal mengubah rekening bank workforce.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat mengubah rekening bank workforce."
                    )
                );
            }
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Bank Account",
            Description = "Mengubah status rekening bank workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceBankAccount", "Update")]
        public async Task<IActionResult> UpdateBankAccountStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWorkforceBankAccountStatusRequest request)
        {
            var entity = await _dbContext.Set<WfpBankAccount>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Rekening bank workforce tidak ditemukan."
                ));
            }

            if (entity.IsPrimary && !request.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Rekening primary tidak boleh dinonaktifkan."
                ));
            }

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status rekening bank workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/primary")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceBankAccountResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Bank Account",
            Description = "Menetapkan rekening bank primary workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceBankAccount", "Update")]
        public async Task<IActionResult> SetPrimaryBankAccount(
            Guid workforceProfileId,
            Guid id,
            [FromBody] SetWorkforceBankAccountPrimaryRequest request)
        {
            if (!request.IsPrimary)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "IsPrimary harus bernilai true."
                ));
            }

            var entity = await _dbContext.Set<WfpBankAccount>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Rekening bank workforce tidak ditemukan."
                ));
            }

            if (!entity.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Rekening tidak aktif tidak bisa dijadikan primary."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                await ApplyPrimaryBankAccountAsync(workforceProfileId, entity, now, actorUserId);

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                var response = await BuildBankAccountResponseAsync(entity.Id);

                return Ok(ApiResponse<WorkforceBankAccountResponse>.Ok(
                    response!,
                    "Rekening bank primary workforce berhasil diperbarui."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceBankAccount.SetPrimaryBankAccount",
                    "Gagal menetapkan rekening bank primary workforce.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat menetapkan rekening bank primary workforce."
                    )
                );
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Delete",
            "Delete Workforce Bank Account",
            Description = "Menghapus rekening bank workforce profile",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("WorkforceBankAccount", "Delete")]
        public async Task<IActionResult> DeleteBankAccount(Guid workforceProfileId, Guid id)
        {
            var entity = await _dbContext.Set<WfpBankAccount>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Rekening bank workforce tidak ditemukan."
                ));
            }

            if (entity.IsPrimary)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Rekening primary tidak boleh dihapus. Jadikan rekening lain sebagai primary terlebih dahulu."
                ));
            }

            entity.IsActive = false;
            entity.IsDelete = true;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Rekening bank workforce berhasil dihapus."
            ));
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateBankAccountRequestAsync(
            Guid workforceProfileId,
            string? bankName,
            string? accountNumber,
            string? accountHolderName,
            Guid? excludeBankAccountId)
        {
            if (string.IsNullOrWhiteSpace(bankName))
            {
                return (false, "Nama bank wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(accountNumber))
            {
                return (false, "Nomor rekening wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(accountHolderName))
            {
                return (false, "Nama pemilik rekening wajib diisi.");
            }

            var normalizedAccountNumber = NormalizeAccountNumber(accountNumber);

            var duplicateExists = await _dbContext.Set<WfpBankAccount>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.Id != excludeBankAccountId &&
                    x.AccountNumber == normalizedAccountNumber &&
                    !x.IsDelete);

            if (duplicateExists)
            {
                return (false, "Nomor rekening sudah terdaftar pada workforce profile ini.");
            }

            return (true, null);
        }

        private async Task ApplyPrimaryBankAccountAsync(
            Guid workforceProfileId,
            WfpBankAccount bankAccount,
            DateTime now,
            Guid actorUserId)
        {
            var currentPrimaries = await _dbContext.Set<WfpBankAccount>()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.Id != bankAccount.Id &&
                    x.IsPrimary &&
                    !x.IsDelete)
                .ToListAsync();

            foreach (var item in currentPrimaries)
            {
                item.IsPrimary = false;
                item.UpdateDateTime = now;
                item.UpdateBy = actorUserId;
            }

            bankAccount.IsPrimary = true;
            bankAccount.IsActive = true;
            bankAccount.UpdateDateTime = now;
            bankAccount.UpdateBy = actorUserId;
        }

        private async Task<WorkforceBankAccountResponse?> BuildBankAccountResponseAsync(Guid id)
        {
            return await _dbContext.Set<WfpBankAccount>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new WorkforceBankAccountResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : string.Empty,
                    DisplayName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty,
                    BankName = x.BankName,
                    AccountNumber = x.AccountNumber,
                    AccountHolderName = x.AccountHolderName,
                    BankBranch = x.BankBranch,
                    IsPrimary = x.IsPrimary,
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

        private static string NormalizeAccountNumber(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : new string(value.Where(char.IsDigit).ToArray());
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}