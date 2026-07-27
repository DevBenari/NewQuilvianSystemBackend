using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/bank-accounts")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE_CORE",
        moduleName: "Human Resource Workforce Core",
        displayName: "Workforce Bank Account",
        AreaName = "Corporate",
        ControllerName = "WorkforceBankAccount",
        Description = "Corporate human resource workforce bank account",
        SortOrder = 2
    )]
    [Tags("Corporate / Human Resource / Workforce Core / Bank Account")]
    public class WfpBankAccountController : ControllerBase
    {
        private static readonly HashSet<string> AllowedAccountTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "Savings", "Current", "Giro", "Payroll", "Other"
        };

        private static readonly string[] CurrencyOptions = { "IDR", "USD", "SGD", "EUR", "JPY", "AUD" };
        private const string LogCategory = "Corporate.HumanResource.WorkforceCore";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WfpBankAccountController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<WfpBankAccountFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Bank Account", Description = "Melihat metadata filter rekening bank workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceBankAccount", "Read")]
        public async Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken)
        {
            var bankOptions = await _dbContext.MstBanks
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDelete)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.BankName)
                .Select(x => new WfpBankOptionResponse
                {
                    Id = x.Id,
                    BankCode = x.BankCode,
                    BankName = x.BankName,
                    Label = x.BankName
                })
                .ToListAsync(cancellationToken);

            var result = new WfpBankAccountFilterMetadataResponse
            {
                DefaultFilter = new WfpBankAccountDefaultFilterResponse(),
                BankOptions = bankOptions,
                AccountTypeOptions = AllowedAccountTypes
                    .OrderBy(x => x)
                    .Select(x => new WfpBankAccountStringOptionResponse
                    {
                        Value = x,
                        Label = BuildAccountTypeLabel(x)
                    })
                    .ToList(),
                CurrencyOptions = CurrencyOptions
                    .Select(x => new WfpBankAccountStringOptionResponse { Value = x, Label = x })
                    .ToList(),
                SortOptions = new List<WfpBankAccountSortOptionResponse>
                {
                    new() { Value = "isPrimary", Label = "Rekening utama" },
                    new() { Value = "bankName", Label = "Nama bank" },
                    new() { Value = "accountHolderName", Label = "Nama pemilik rekening" },
                    new() { Value = "accountType", Label = "Jenis rekening" },
                    new() { Value = "isPayrollAccount", Label = "Rekening payroll" },
                    new() { Value = "isVerified", Label = "Status verifikasi" },
                    new() { Value = "isActive", Label = "Status aktif" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            return Ok(ApiResponse<WfpBankAccountFilterMetadataResponse>.Ok(
                result,
                "Metadata filter rekening bank workforce berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WfpBankAccountSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Bank Account", Description = "Melihat ringkasan rekening bank workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceBankAccount", "Read")]
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

            var query = _dbContext.Set<WfpBankAccount>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            var result = new WfpBankAccountSummaryResponse
            {
                TotalBankAccount = await query.CountAsync(cancellationToken),
                ActiveBankAccount = await query.CountAsync(x => x.IsActive, cancellationToken),
                InactiveBankAccount = await query.CountAsync(x => !x.IsActive, cancellationToken),
                PrimaryBankAccount = await query.CountAsync(x => x.IsPrimary, cancellationToken),
                PayrollBankAccount = await query.CountAsync(x => x.IsPayrollAccount, cancellationToken),
                VerifiedBankAccount = await query.CountAsync(x => x.IsVerified, cancellationToken),
                UnverifiedBankAccount = await query.CountAsync(x => !x.IsVerified, cancellationToken)
            };

            return Ok(ApiResponse<WfpBankAccountSummaryResponse>.Ok(
                result,
                "Ringkasan rekening bank workforce berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<WfpBankAccountResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Bank Account", Description = "Melihat data rekening bank workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceBankAccount", "Read")]
        public async Task<IActionResult> GetBankAccounts(
            Guid workforceProfileId,
            [FromQuery] Guid? bankId,
            [FromQuery] string? accountType,
            [FromQuery] bool? isPayrollAccount,
            [FromQuery] bool? isPrimary,
            [FromQuery] bool? isVerified,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "isPrimary",
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

            var query = BuildBaseQuery(workforceProfileId);
            query = ApplyFilter(
                query,
                bankId,
                accountType,
                isPayrollAccount,
                isPrimary,
                isVerified,
                isActive,
                search);

            var totalData = await query.CountAsync(cancellationToken);

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new WfpBankAccountResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    WorkforceProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : string.Empty,
                    WorkforceDisplayName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty,
                    BankId = x.BankId,
                    BankCode = x.Bank != null ? x.Bank.BankCode : null,
                    BankName = x.Bank != null ? x.Bank.BankName : x.BankName ?? string.Empty,
                    AccountNumber = x.AccountNumber,
                    AccountHolderName = x.AccountHolderName,
                    BankBranch = x.BankBranch,
                    AccountType = x.AccountType,
                    CurrencyCode = x.CurrencyCode,
                    IsPayrollAccount = x.IsPayrollAccount,
                    IsPrimary = x.IsPrimary,
                    IsVerified = x.IsVerified,
                    VerifiedAt = x.VerifiedAt,
                    VerifiedByUserId = x.VerifiedByUserId,
                    VerifiedByUserName = x.VerifiedByUserId.HasValue
                        ? _dbContext.Users
                            .Where(u => u.Id == x.VerifiedByUserId.Value)
                            .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
                            .FirstOrDefault()
                        : null,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty
                        ? null
                        : _dbContext.Users
                            .Where(u => u.Id == x.CreateBy)
                            .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
                            .FirstOrDefault()
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<WfpBankAccountResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<PagedResult<WfpBankAccountResponse>>.Ok(
                result,
                "Data rekening bank workforce berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WfpBankAccountDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Bank Account", Description = "Melihat detail rekening bank workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceBankAccount", "Read")]
        public async Task<IActionResult> GetBankAccountById(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            var data = await BuildBaseQuery(workforceProfileId)
                .Where(x => x.Id == id)
                .Select(x => new WfpBankAccountDetailResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    WorkforceProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : string.Empty,
                    WorkforceDisplayName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty,
                    BankId = x.BankId,
                    BankCode = x.Bank != null ? x.Bank.BankCode : null,
                    BankName = x.Bank != null ? x.Bank.BankName : x.BankName ?? string.Empty,
                    AccountNumber = x.AccountNumber,
                    AccountHolderName = x.AccountHolderName,
                    BankBranch = x.BankBranch,
                    AccountType = x.AccountType,
                    CurrencyCode = x.CurrencyCode,
                    IsPayrollAccount = x.IsPayrollAccount,
                    IsPrimary = x.IsPrimary,
                    IsVerified = x.IsVerified,
                    VerifiedAt = x.VerifiedAt,
                    VerifiedByUserId = x.VerifiedByUserId,
                    VerifiedByUserName = x.VerifiedByUserId.HasValue
                        ? _dbContext.Users
                            .Where(u => u.Id == x.VerifiedByUserId.Value)
                            .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
                            .FirstOrDefault()
                        : null,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    Description = x.Description,
                    IsActive = x.IsActive,
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
                .FirstOrDefaultAsync(cancellationToken);

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Rekening bank workforce tidak ditemukan."));
            }

            return Ok(ApiResponse<WfpBankAccountDetailResponse>.Ok(
                data,
                "Detail rekening bank workforce berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WfpBankAccountDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Create", "Create Workforce Bank Account", Description = "Membuat rekening bank workforce", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WorkforceBankAccount", "Create")]
        public async Task<IActionResult> CreateBankAccount(
            Guid workforceProfileId,
            [FromBody] CreateWfpBankAccountRequest request,
            CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil tenaga kerja tidak ditemukan."));
            }

            var validation = await ValidateRequestAsync(
                workforceProfileId,
                null,
                request,
                cancellationToken);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data rekening bank workforce tidak valid."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (request.IsPrimary)
                {
                    await UnsetOtherPrimaryAsync(workforceProfileId, null, now, actorUserId, cancellationToken);
                }

                var entity = new WfpBankAccount
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = workforceProfileId,
                    BankId = NormalizeNullableGuid(request.BankId),
                    BankName = validation.BankName,
                    AccountNumber = NormalizeAccountNumber(request.AccountNumber),
                    AccountHolderName = request.AccountHolderName.Trim(),
                    BankBranch = NormalizeNullableText(request.BankBranch),
                    AccountType = NormalizeAccountType(request.AccountType),
                    CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant(),
                    IsPayrollAccount = request.IsPayrollAccount,
                    IsPrimary = request.IsPrimary,
                    IsVerified = request.IsVerified,
                    VerifiedAt = request.IsVerified ? now : null,
                    VerifiedByUserId = request.IsVerified ? actorUserId : null,
                    EffectiveStartDate = request.EffectiveStartDate?.Date,
                    EffectiveEndDate = request.EffectiveEndDate?.Date,
                    Description = NormalizeNullableText(request.Description),
                    IsActive = request.IsActive,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<WfpBankAccount>().Add(entity);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                await _loggerService.InfoAsync(
                    LogCategory,
                    "WorkforceBankAccount.CreateBankAccount",
                    "Membuat rekening bank workforce.",
                    new { entity.Id, entity.WorkforceProfileId, entity.BankId, entity.IsPrimary, entity.IsPayrollAccount });

                return await GetBankAccountById(workforceProfileId, entity.Id, cancellationToken);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceBankAccount.CreateBankAccount",
                    "Gagal membuat rekening bank workforce.",
                    ex);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat membuat rekening bank workforce."));
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WfpBankAccountDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Bank Account", Description = "Mengubah rekening bank workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceBankAccount", "Update")]
        public async Task<IActionResult> UpdateBankAccount(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpBankAccountRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpBankAccount>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Rekening bank workforce tidak ditemukan."));
            }

            var validation = await ValidateRequestAsync(
                workforceProfileId,
                id,
                request,
                cancellationToken);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data rekening bank workforce tidak valid."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (request.IsPrimary)
                {
                    await UnsetOtherPrimaryAsync(workforceProfileId, id, now, actorUserId, cancellationToken);
                }

                entity.BankId = NormalizeNullableGuid(request.BankId);
                entity.BankName = validation.BankName;
                entity.AccountNumber = NormalizeAccountNumber(request.AccountNumber);
                entity.AccountHolderName = request.AccountHolderName.Trim();
                entity.BankBranch = NormalizeNullableText(request.BankBranch);
                entity.AccountType = NormalizeAccountType(request.AccountType);
                entity.CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant();
                entity.IsPayrollAccount = request.IsPayrollAccount;
                entity.IsPrimary = request.IsPrimary && request.IsActive;

                if (entity.IsVerified != request.IsVerified)
                {
                    entity.IsVerified = request.IsVerified;
                    entity.VerifiedAt = request.IsVerified ? now : null;
                    entity.VerifiedByUserId = request.IsVerified ? actorUserId : null;
                }

                entity.EffectiveStartDate = request.EffectiveStartDate?.Date;
                entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
                entity.Description = NormalizeNullableText(request.Description);
                entity.IsActive = request.IsActive;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                await _loggerService.InfoAsync(
                    LogCategory,
                    "WorkforceBankAccount.UpdateBankAccount",
                    "Mengubah rekening bank workforce.",
                    new { entity.Id, entity.WorkforceProfileId, entity.BankId, entity.IsPrimary, entity.IsActive });

                return await GetBankAccountById(workforceProfileId, entity.Id, cancellationToken);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceBankAccount.UpdateBankAccount",
                    "Gagal mengubah rekening bank workforce.",
                    ex);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat mengubah rekening bank workforce."));
            }
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Bank Account", Description = "Mengubah status rekening bank workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceBankAccount", "Update")]
        public async Task<IActionResult> UpdateBankAccountStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpBankAccountStatusRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpBankAccount>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Rekening bank workforce tidak ditemukan."));
            }

            if (request.EffectiveEndDate.HasValue &&
                entity.EffectiveStartDate.HasValue &&
                request.EffectiveEndDate.Value.Date < entity.EffectiveStartDate.Value.Date)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "EffectiveEndDate tidak boleh lebih kecil dari EffectiveStartDate."));
            }

            entity.IsActive = request.IsActive;
            entity.IsPrimary = request.IsActive && entity.IsPrimary;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.Description = NormalizeNullableText(request.Description) ?? entity.Description;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status rekening bank workforce berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/primary")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Bank Account", Description = "Menetapkan rekening utama workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceBankAccount", "Update")]
        public async Task<IActionResult> SetPrimaryBankAccount(
            Guid workforceProfileId,
            Guid id,
            [FromBody] SetWfpBankAccountPrimaryRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpBankAccount>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Rekening bank workforce tidak ditemukan."));
            }

            if (request.IsPrimary && !entity.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Rekening bank tidak aktif tidak dapat dijadikan rekening utama."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (request.IsPrimary)
                {
                    await UnsetOtherPrimaryAsync(workforceProfileId, id, now, actorUserId, cancellationToken);
                }

                entity.IsPrimary = request.IsPrimary;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Ok(ApiResponse<object>.Ok(
                    null,
                    request.IsPrimary
                        ? "Rekening utama workforce berhasil ditetapkan."
                        : "Status rekening utama workforce berhasil dilepas."));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceBankAccount.SetPrimaryBankAccount",
                    "Gagal menetapkan rekening utama workforce.",
                    ex);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat menetapkan rekening utama workforce."));
            }
        }

        [HttpPatch("{id:guid}/verify")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Bank Account", Description = "Memverifikasi rekening bank workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceBankAccount", "Update")]
        public async Task<IActionResult> VerifyBankAccount(
            Guid workforceProfileId,
            Guid id,
            [FromBody] VerifyWfpBankAccountRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpBankAccount>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Rekening bank workforce tidak ditemukan."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsVerified = request.IsVerified;
            entity.VerifiedAt = request.IsVerified ? now : null;
            entity.VerifiedByUserId = request.IsVerified ? actorUserId : null;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(
                null,
                request.IsVerified
                    ? "Rekening bank workforce berhasil diverifikasi."
                    : "Verifikasi rekening bank workforce berhasil dibatalkan."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Workforce Bank Account", Description = "Menghapus rekening bank workforce", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("WorkforceBankAccount", "Delete")]
        public async Task<IActionResult> DeleteBankAccount(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpBankAccount>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Rekening bank workforce tidak ditemukan."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.IsPrimary = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceBankAccount.DeleteBankAccount",
                "Menghapus rekening bank workforce.",
                new { entity.Id, entity.WorkforceProfileId, entity.BankId });

            return Ok(ApiResponse<object>.Ok(
                null,
                "Rekening bank workforce berhasil dihapus."));
        }

        private IQueryable<WfpBankAccount> BuildBaseQuery(Guid workforceProfileId)
        {
            return _dbContext.Set<WfpBankAccount>()
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.Bank)
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);
        }

        private static IQueryable<WfpBankAccount> ApplyFilter(
            IQueryable<WfpBankAccount> query,
            Guid? bankId,
            string? accountType,
            bool? isPayrollAccount,
            bool? isPrimary,
            bool? isVerified,
            bool? isActive,
            string? search)
        {
            bankId = NormalizeNullableGuid(bankId);
            if (bankId.HasValue)
                query = query.Where(x => x.BankId == bankId.Value);

            if (!string.IsNullOrWhiteSpace(accountType))
            {
                var normalizedType = NormalizeAccountType(accountType);
                query = query.Where(x => x.AccountType == normalizedType);
            }

            if (isPayrollAccount.HasValue)
                query = query.Where(x => x.IsPayrollAccount == isPayrollAccount.Value);

            if (isPrimary.HasValue)
                query = query.Where(x => x.IsPrimary == isPrimary.Value);

            if (isVerified.HasValue)
                query = query.Where(x => x.IsVerified == isVerified.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.AccountNumber.ToLower().Contains(keyword) ||
                    x.AccountHolderName.ToLower().Contains(keyword) ||
                    (x.BankName != null && x.BankName.ToLower().Contains(keyword)) ||
                    (x.BankBranch != null && x.BankBranch.ToLower().Contains(keyword)) ||
                    x.AccountType.ToLower().Contains(keyword) ||
                    x.CurrencyCode.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.Bank != null && x.Bank.BankCode.ToLower().Contains(keyword)) ||
                    (x.Bank != null && x.Bank.BankName.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<WfpBankAccount> ApplySorting(
            IQueryable<WfpBankAccount> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = !string.Equals(
                sortDirection?.Trim(),
                "asc",
                StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "isPrimary").Trim().ToLowerInvariant() switch
            {
                "bankname" => isDescending
                    ? query.OrderByDescending(x => x.Bank != null ? x.Bank.BankName : x.BankName).ThenBy(x => x.AccountHolderName)
                    : query.OrderBy(x => x.Bank != null ? x.Bank.BankName : x.BankName).ThenBy(x => x.AccountHolderName),

                "accountholdername" => isDescending
                    ? query.OrderByDescending(x => x.AccountHolderName)
                    : query.OrderBy(x => x.AccountHolderName),

                "accounttype" => isDescending
                    ? query.OrderByDescending(x => x.AccountType).ThenBy(x => x.AccountHolderName)
                    : query.OrderBy(x => x.AccountType).ThenBy(x => x.AccountHolderName),

                "ispayrollaccount" => isDescending
                    ? query.OrderByDescending(x => x.IsPayrollAccount).ThenByDescending(x => x.IsPrimary)
                    : query.OrderBy(x => x.IsPayrollAccount).ThenBy(x => x.IsPrimary),

                "isverified" => isDescending
                    ? query.OrderByDescending(x => x.IsVerified).ThenByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.IsVerified).ThenBy(x => x.CreateDateTime),

                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.CreateDateTime),

                "createdatetime" => isDescending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                _ => isDescending
                    ? query.OrderByDescending(x => x.IsPrimary).ThenByDescending(x => x.IsPayrollAccount).ThenByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.IsPrimary).ThenBy(x => x.IsPayrollAccount).ThenBy(x => x.CreateDateTime)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage, string? BankName)> ValidateRequestAsync(
            Guid workforceProfileId,
            Guid? excludeId,
            CreateWfpBankAccountRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.AccountNumber))
                return (false, "Nomor rekening wajib diisi.", null);

            if (string.IsNullOrWhiteSpace(request.AccountHolderName))
                return (false, "Nama pemilik rekening wajib diisi.", null);

            if (string.IsNullOrWhiteSpace(request.AccountType))
                return (false, "Jenis rekening wajib diisi.", null);

            if (!AllowedAccountTypes.Contains(request.AccountType.Trim()))
            {
                return (false, "Jenis rekening tidak valid. Gunakan Savings, Current, Giro, Payroll, atau Other.", null);
            }

            if (string.IsNullOrWhiteSpace(request.CurrencyCode) ||
                request.CurrencyCode.Trim().Length != 3 ||
                !request.CurrencyCode.Trim().All(char.IsLetter))
            {
                return (false, "CurrencyCode harus terdiri dari 3 huruf.", null);
            }

            if (request.EffectiveStartDate.HasValue &&
                request.EffectiveEndDate.HasValue &&
                request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Value.Date)
            {
                return (false, "EffectiveEndDate tidak boleh lebih kecil dari EffectiveStartDate.", null);
            }

            var normalizedAccountNumber = NormalizeAccountNumber(request.AccountNumber);

            var duplicateQuery = _dbContext.Set<WfpBankAccount>()
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.AccountNumber == normalizedAccountNumber &&
                    !x.IsDelete);

            if (excludeId.HasValue)
                duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateQuery.AnyAsync(cancellationToken))
                return (false, "Nomor rekening sudah digunakan pada profil tenaga kerja ini.", null);

            var bankId = NormalizeNullableGuid(request.BankId);
            string? bankName;

            if (bankId.HasValue)
            {
                var bank = await _dbContext.MstBanks
                    .AsNoTracking()
                    .Where(x => x.Id == bankId.Value && x.IsActive && !x.IsDelete)
                    .Select(x => new { x.BankName })
                    .FirstOrDefaultAsync(cancellationToken);

                if (bank == null)
                    return (false, "Bank tidak ditemukan atau sudah tidak aktif.", null);

                bankName = bank.BankName;
            }
            else
            {
                bankName = NormalizeNullableText(request.BankName);

                if (string.IsNullOrWhiteSpace(bankName))
                    return (false, "BankId atau BankName wajib diisi.", null);
            }

            return (true, null, bankName);
        }

        private async Task UnsetOtherPrimaryAsync(
            Guid workforceProfileId,
            Guid? exceptId,
            DateTime now,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.Set<WfpBankAccount>()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsPrimary &&
                    x.IsActive &&
                    !x.IsDelete);

            if (exceptId.HasValue)
                query = query.Where(x => x.Id != exceptId.Value);

            var existingPrimaries = await query.ToListAsync(cancellationToken);

            foreach (var item in existingPrimaries)
            {
                item.IsPrimary = false;
                item.UpdateDateTime = now;
                item.UpdateBy = actorUserId;
            }
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

        private Guid GetCurrentUserId()
        {
            var userIdText =
                User.FindFirstValue("user_id") ??
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
        }

        private static (int PageNumber, int PageSize) NormalizePaging(
            int pageNumber,
            int pageSize)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 25 : pageSize;
            pageSize = pageSize > 100 ? 100 : pageSize;
            return (pageNumber, pageSize);
        }

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            return !value.HasValue || value.Value == Guid.Empty
                ? null
                : value.Value;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static string NormalizeAccountNumber(string value)
        {
            return new string(value
                .Where(x => char.IsLetterOrDigit(x))
                .ToArray())
                .ToUpperInvariant();
        }

        private static string NormalizeAccountType(string value)
        {
            var selected = AllowedAccountTypes
                .FirstOrDefault(x => x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));

            return selected ?? value.Trim();
        }

        private static string BuildAccountTypeLabel(string value)
        {
            return value switch
            {
                "Savings" => "Tabungan",
                "Current" => "Current Account",
                "Giro" => "Giro",
                "Payroll" => "Payroll",
                "Other" => "Lainnya",
                _ => value
            };
        }
    }
}
