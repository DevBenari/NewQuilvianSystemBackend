using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseWorkforceBankAccountPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs.WorkforceBankAccountResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/bank-accounts")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE",
        moduleName: "Human Resource Workforce",
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

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceBankAccountFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Bank Account", Description = "Melihat metadata filter rekening bank workforce profile", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceBankAccount", "Read")]
        public async Task<IActionResult> GetFilterMetadata(Guid workforceProfileId)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var result = new WorkforceBankAccountFilterMetadataResponse
            {
                DefaultFilter = new WorkforceBankAccountDefaultFilterResponse(),
                CustomPeriods = new List<WorkforceBankAccountCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" },
                    new() { Value = "lastmonth", Label = "Bulan lalu" }
                },
                SortOptions = new List<WorkforceBankAccountSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "bankCode", Label = "Kode bank" },
                    new() { Value = "bankName", Label = "Nama bank" },
                    new() { Value = "accountNumber", Label = "Nomor rekening" },
                    new() { Value = "accountHolderName", Label = "Nama pemilik rekening" },
                    new() { Value = "bankBranch", Label = "Cabang bank" },
                    new() { Value = "isPrimary", Label = "Primary" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                BankOptionsEndpoint = "/api/v1/administrator/master-data/banks/options"
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceBankAccount.GetFilterMetadata",
                "Mengambil metadata filter rekening bank workforce.",
                new { workforceProfileId, profile.ProfileCode }
            );

            return Ok(ApiResponse<WorkforceBankAccountFilterMetadataResponse>.Ok(
                result,
                "Metadata filter rekening bank workforce berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceBankAccountSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Bank Account", Description = "Melihat ringkasan rekening bank workforce profile", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceBankAccount", "Read")]
        public async Task<IActionResult> GetSummary(Guid workforceProfileId)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var query = BuildBaseQuery(workforceProfileId);

            var result = new WorkforceBankAccountSummaryResponse
            {
                WorkforceProfileId = profile.Id,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                TotalBankAccount = await query.CountAsync(),
                ActiveBankAccount = await query.CountAsync(x => x.IsActive),
                InactiveBankAccount = await query.CountAsync(x => !x.IsActive),
                PrimaryBankAccount = await query.CountAsync(x => x.IsPrimary && x.IsActive)
            };

            return Ok(ApiResponse<WorkforceBankAccountSummaryResponse>.Ok(
                result,
                "Ringkasan rekening bank workforce berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseWorkforceBankAccountPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Bank Account", Description = "Melihat rekening bank workforce profile", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceBankAccount", "Read")]
        public async Task<IActionResult> GetBankAccounts(
            Guid workforceProfileId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? bankId,
            [FromQuery] bool? isPrimary,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "createDateTime",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery(workforceProfileId);
            query = ApplyDateFilter(query, startDate, endDate, customPeriod);
            query = ApplyStandardFilter(query, bankId, isPrimary, isActive, search);

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(
                entities
                    .SelectMany(x => new[] { x.CreateBy })
                    .Where(x => x != Guid.Empty)
            );

            var items = entities
                .Select(x => MapResponse(x, profile, actorNames))
                .ToList();

            var result = new ResponseWorkforceBankAccountPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseWorkforceBankAccountPagedResult>.Ok(
                result,
                "Data rekening bank workforce berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceBankAccountOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Bank Account", Description = "Melihat pilihan rekening bank workforce profile", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceBankAccount", "Read")]
        public async Task<IActionResult> GetOptions(
            Guid workforceProfileId,
            [FromQuery] Guid? bankId,
            [FromQuery] bool? isPrimary,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var profileExists = await ProfileExistsAsync(workforceProfileId);

            if (!profileExists)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery(workforceProfileId);
            query = ApplyStandardFilter(query, bankId, isPrimary, onlyActive ? true : null, search);

            var totalData = await query.CountAsync();

            var entities = await query
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.IsActive)
                .ThenBy(x => x.Bank != null ? x.Bank.BankName : string.Empty)
                .ThenBy(x => x.AccountHolderName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities
                .Select(MapOptionResponse)
                .ToList();

            var result = new WorkforceBankAccountOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<WorkforceBankAccountOptionPagedResponse>.Ok(
                result,
                "Data pilihan rekening bank workforce berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceBankAccountDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Bank Account", Description = "Melihat detail rekening bank workforce profile", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceBankAccount", "Read")]
        public async Task<IActionResult> GetBankAccountById(Guid workforceProfileId, Guid id)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var entity = await BuildBaseQuery(workforceProfileId)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Rekening bank workforce tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var data = MapDetailResponse(entity, profile, actorNames);
            NormalizeAuditResponse(data);

            return Ok(ApiResponse<WorkforceBankAccountDetailResponse>.Ok(
                data,
                "Detail rekening bank workforce berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WorkforceBankAccountDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Workforce Bank Account", Description = "Menambah rekening bank workforce profile", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WorkforceBankAccount", "Create")]
        public async Task<IActionResult> CreateBankAccount(
            Guid workforceProfileId,
            [FromBody] CreateWorkforceBankAccountRequest request)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(
                workforceProfileId,
                excludeId: null,
                request.BankId,
                request.AccountNumber,
                request.AccountHolderName);

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
                .AsNoTracking()
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
                    BankId = request.BankId,
                    AccountNumber = NormalizeAccountNumber(request.AccountNumber),
                    AccountHolderName = NormalizeRequiredText(request.AccountHolderName),
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
                    await ApplyPrimaryBankAccountAsync(
                        workforceProfileId,
                        entity,
                        now,
                        actorUserId,
                        updateTargetAudit: false
                    );
                }

                await _dbContext.SaveChangesAsync();
                await _dbContext.Entry(entity).Reference(x => x.Bank).LoadAsync();
                await transaction.CommitAsync();

                var actorNames = await GetActorNameMapAsync(new[] { entity.CreateBy });
                var data = MapDetailResponse(entity, profile, actorNames);
                NormalizeAuditResponse(data);

                await _loggerService.InfoAsync(
                    LogCategory,
                    "WorkforceBankAccount.CreateBankAccount",
                    "Rekening bank workforce berhasil dibuat.",
                    new { workforceProfileId, entity.Id, entity.BankId, entity.IsPrimary }
                );

                return Ok(ApiResponse<WorkforceBankAccountDetailResponse>.Ok(
                    data,
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
                    ex,
                    new { workforceProfileId }
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
        [ProducesResponseType(typeof(ApiResponse<WorkforceBankAccountDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Bank Account", Description = "Mengubah rekening bank workforce profile", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceBankAccount", "Update")]
        public async Task<IActionResult> UpdateBankAccount(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWorkforceBankAccountRequest request)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var entity = await _dbContext.Set<WfpBankAccount>()
                .Include(x => x.Bank)
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

            var validation = await ValidateRequestAsync(
                workforceProfileId,
                id,
                request.BankId,
                request.AccountNumber,
                request.AccountHolderName);

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
                entity.BankId = request.BankId;
                entity.AccountNumber = NormalizeAccountNumber(request.AccountNumber);
                entity.AccountHolderName = NormalizeRequiredText(request.AccountHolderName);
                entity.BankBranch = NormalizeNullableText(request.BankBranch);
                entity.IsActive = request.IsActive;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                if (request.IsPrimary)
                {
                    await ApplyPrimaryBankAccountAsync(
                        workforceProfileId,
                        entity,
                        now,
                        actorUserId,
                        updateTargetAudit: true
                    );
                }

                await _dbContext.SaveChangesAsync();
                await _dbContext.Entry(entity).Reference(x => x.Bank).LoadAsync();
                await transaction.CommitAsync();

                var actorNames = await GetActorNameMapAsync(new[]
                {
                    entity.CreateBy,
                    entity.UpdateBy
                });

                var data = MapDetailResponse(entity, profile, actorNames);
                NormalizeAuditResponse(data);

                return Ok(ApiResponse<WorkforceBankAccountDetailResponse>.Ok(
                    data,
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
                    ex,
                    new { workforceProfileId, id }
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
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Bank Account", Description = "Mengubah status rekening bank workforce profile", AccessType = AccessTypes.Update, SortOrder = 4)]
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
                    "Rekening primary tidak boleh dinonaktifkan. Jadikan rekening lain sebagai primary terlebih dahulu."
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
        [ProducesResponseType(typeof(ApiResponse<WorkforceBankAccountDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Set Workforce Bank Account Primary", Description = "Menetapkan rekening bank primary workforce profile", AccessType = AccessTypes.Update, SortOrder = 5)]
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

            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var entity = await _dbContext.Set<WfpBankAccount>()
                .Include(x => x.Bank)
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
                await ApplyPrimaryBankAccountAsync(
                    workforceProfileId,
                    entity,
                    now,
                    actorUserId,
                    updateTargetAudit: true
                );

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                var actorNames = await GetActorNameMapAsync(new[]
                {
                    entity.CreateBy,
                    entity.UpdateBy
                });

                var data = MapDetailResponse(entity, profile, actorNames);
                NormalizeAuditResponse(data);

                return Ok(ApiResponse<WorkforceBankAccountDetailResponse>.Ok(
                    data,
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
                    ex,
                    new { workforceProfileId, id }
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
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Workforce Bank Account", Description = "Menghapus rekening bank workforce profile", AccessType = AccessTypes.Delete, SortOrder = 6)]
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

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = false;
            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Rekening bank workforce berhasil dihapus."
            ));
        }

        private IQueryable<WfpBankAccount> BuildBaseQuery(Guid workforceProfileId)
        {
            return _dbContext.Set<WfpBankAccount>()
                .AsNoTracking()
                .Include(x => x.Bank)
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);
        }

        private static IQueryable<WfpBankAccount> ApplyDateFilter(
            IQueryable<WfpBankAccount> query,
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            if (startDate.HasValue)
            {
                var start = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                query = query.Where(x => x.CreateDateTime >= start);
            }

            if (endDate.HasValue)
            {
                var end = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                query = query.Where(x => x.CreateDateTime < end);
            }

            if (!startDate.HasValue &&
                !endDate.HasValue &&
                !string.IsNullOrWhiteSpace(customPeriod))
            {
                var today = DateTime.UtcNow.Date;

                switch (customPeriod.Trim().ToLowerInvariant())
                {
                    case "today":
                        query = query.Where(x =>
                            x.CreateDateTime >= today &&
                            x.CreateDateTime < today.AddDays(1));
                        break;

                    case "last7days":
                        query = query.Where(x =>
                            x.CreateDateTime >= today.AddDays(-6) &&
                            x.CreateDateTime < today.AddDays(1));
                        break;

                    case "thismonth":
                        var thisMonthStart = new DateTime(
                            today.Year,
                            today.Month,
                            1,
                            0,
                            0,
                            0,
                            DateTimeKind.Utc
                        );

                        query = query.Where(x =>
                            x.CreateDateTime >= thisMonthStart &&
                            x.CreateDateTime < thisMonthStart.AddMonths(1));
                        break;

                    case "lastmonth":
                        var currentMonthStart = new DateTime(
                            today.Year,
                            today.Month,
                            1,
                            0,
                            0,
                            0,
                            DateTimeKind.Utc
                        );

                        var lastMonthStart = currentMonthStart.AddMonths(-1);

                        query = query.Where(x =>
                            x.CreateDateTime >= lastMonthStart &&
                            x.CreateDateTime < currentMonthStart);
                        break;
                }
            }

            return query;
        }

        private static IQueryable<WfpBankAccount> ApplyStandardFilter(
            IQueryable<WfpBankAccount> query,
            Guid? bankId,
            bool? isPrimary,
            bool? isActive,
            string? search)
        {
            if (bankId.HasValue && bankId.Value != Guid.Empty)
            {
                query = query.Where(x => x.BankId == bankId.Value);
            }

            if (isPrimary.HasValue)
            {
                query = query.Where(x => x.IsPrimary == isPrimary.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                var accountKeyword = NormalizeAccountNumber(search);

                query = query.Where(x =>
                    (x.Bank != null && x.Bank.BankCode.ToLower().Contains(keyword)) ||
                    (x.Bank != null && x.Bank.BankName.ToLower().Contains(keyword)) ||
                    (x.Bank != null && x.Bank.BankShortName != null && x.Bank.BankShortName.ToLower().Contains(keyword)) ||
                    x.AccountHolderName.ToLower().Contains(keyword) ||
                    (x.BankBranch != null && x.BankBranch.ToLower().Contains(keyword)) ||
                    (!string.IsNullOrWhiteSpace(accountKeyword) && x.AccountNumber.Contains(accountKeyword)));
            }

            return query;
        }

        private static IOrderedQueryable<WfpBankAccount> ApplySorting(
            IQueryable<WfpBankAccount> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "createDateTime").Trim().ToLowerInvariant() switch
            {
                "bankcode" => isDescending
                    ? query.OrderByDescending(x => x.Bank != null ? x.Bank.BankCode : string.Empty)
                    : query.OrderBy(x => x.Bank != null ? x.Bank.BankCode : string.Empty),

                "bankname" => isDescending
                    ? query.OrderByDescending(x => x.Bank != null ? x.Bank.BankName : string.Empty)
                    : query.OrderBy(x => x.Bank != null ? x.Bank.BankName : string.Empty),

                "accountnumber" => isDescending
                    ? query.OrderByDescending(x => x.AccountNumber)
                    : query.OrderBy(x => x.AccountNumber),

                "accountholdername" => isDescending
                    ? query.OrderByDescending(x => x.AccountHolderName)
                    : query.OrderBy(x => x.AccountHolderName),

                "bankbranch" => isDescending
                    ? query.OrderByDescending(x => x.BankBranch)
                    : query.OrderBy(x => x.BankBranch),

                "isprimary" => isDescending
                    ? query.OrderByDescending(x => x.IsPrimary).ThenBy(x => x.Bank != null ? x.Bank.BankName : string.Empty)
                    : query.OrderBy(x => x.IsPrimary).ThenBy(x => x.Bank != null ? x.Bank.BankName : string.Empty),

                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.Bank != null ? x.Bank.BankName : string.Empty)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.Bank != null ? x.Bank.BankName : string.Empty),

                _ => isDescending
                    ? query.OrderByDescending(x => x.CreateDateTime).ThenByDescending(x => x.IsPrimary)
                    : query.OrderBy(x => x.CreateDateTime).ThenByDescending(x => x.IsPrimary)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid workforceProfileId,
            Guid? excludeId,
            Guid bankId,
            string? accountNumber,
            string? accountHolderName)
        {
            if (bankId == Guid.Empty)
            {
                return (false, "Bank wajib dipilih.");
            }

            var bankExists = await _dbContext.Set<MstBank>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == bankId &&
                    x.IsActive &&
                    !x.IsDelete);

            if (!bankExists)
            {
                return (false, "Bank tidak valid atau tidak aktif.");
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

            if (string.IsNullOrWhiteSpace(normalizedAccountNumber))
            {
                return (false, "Nomor rekening wajib berisi angka.");
            }

            if (normalizedAccountNumber.Length < 5)
            {
                return (false, "Nomor rekening minimal 5 digit.");
            }

            var duplicateQuery = _dbContext.Set<WfpBankAccount>()
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.AccountNumber == normalizedAccountNumber &&
                    !x.IsDelete);

            if (excludeId.HasValue)
            {
                duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);
            }

            if (await duplicateQuery.AnyAsync())
            {
                return (false, "Nomor rekening sudah terdaftar pada workforce profile ini.");
            }

            return (true, null);
        }

        private async Task ApplyPrimaryBankAccountAsync(
            Guid workforceProfileId,
            WfpBankAccount bankAccount,
            DateTime now,
            Guid actorUserId,
            bool updateTargetAudit)
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

            if (updateTargetAudit)
            {
                bankAccount.UpdateDateTime = now;
                bankAccount.UpdateBy = actorUserId;
            }
        }

        private WorkforceBankAccountResponse MapResponse(
            WfpBankAccount entity,
            MstWorkforceProfile profile,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new WorkforceBankAccountResponse
            {
                Id = entity.Id,
                WorkforceProfileId = entity.WorkforceProfileId,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                BankId = entity.BankId,
                BankCode = entity.Bank != null ? entity.Bank.BankCode : string.Empty,
                BankName = entity.Bank != null ? entity.Bank.BankName : string.Empty,
                BankShortName = entity.Bank?.BankShortName,
                BankCategory = entity.Bank != null ? entity.Bank.BankCategory : BankCategory.Commercial,
                BankCategoryName = entity.Bank != null ? BuildBankCategoryLabel(entity.Bank.BankCategory) : string.Empty,
                AccountNumber = entity.AccountNumber,
                AccountNumberMasked = MaskAccountNumber(entity.AccountNumber),
                AccountHolderName = entity.AccountHolderName,
                BankBranch = entity.BankBranch,
                IsPrimary = entity.IsPrimary,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private WorkforceBankAccountDetailResponse MapDetailResponse(
            WfpBankAccount entity,
            MstWorkforceProfile profile,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var response = MapResponse(entity, profile, actorNames);

            return new WorkforceBankAccountDetailResponse
            {
                Id = response.Id,
                WorkforceProfileId = response.WorkforceProfileId,
                ProfileCode = response.ProfileCode,
                DisplayName = response.DisplayName,
                BankId = response.BankId,
                BankCode = response.BankCode,
                BankName = response.BankName,
                BankShortName = response.BankShortName,
                BankCategory = response.BankCategory,
                BankCategoryName = response.BankCategoryName,
                AccountNumber = response.AccountNumber,
                AccountNumberMasked = response.AccountNumberMasked,
                AccountHolderName = response.AccountHolderName,
                BankBranch = response.BankBranch,
                IsPrimary = response.IsPrimary,
                IsActive = response.IsActive,
                CreateDateTime = response.CreateDateTime,
                CreateBy = response.CreateBy,
                CreateByName = response.CreateByName,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static WorkforceBankAccountOptionResponse MapOptionResponse(WfpBankAccount entity)
        {
            return new WorkforceBankAccountOptionResponse
            {
                Id = entity.Id,
                BankId = entity.BankId,
                BankCode = entity.Bank != null ? entity.Bank.BankCode : string.Empty,
                BankName = entity.Bank != null ? entity.Bank.BankName : string.Empty,
                BankShortName = entity.Bank?.BankShortName,
                AccountNumberMasked = MaskAccountNumber(entity.AccountNumber),
                AccountHolderName = entity.AccountHolderName,
                BankBranch = entity.BankBranch,
                IsPrimary = entity.IsPrimary,
                IsActive = entity.IsActive
            };
        }

        private async Task<MstWorkforceProfile?> GetProfileAsync(Guid workforceProfileId)
        {
            return await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == workforceProfileId && !x.IsDelete);
        }

        private async Task<bool> ProfileExistsAsync(Guid workforceProfileId)
        {
            return await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == workforceProfileId && !x.IsDelete);
        }

        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(IEnumerable<Guid> actorIds)
        {
            var ids = actorIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (!ids.Any())
            {
                return new Dictionary<Guid, string?>();
            }

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    Name =
                        x.DisplayName ??
                        x.UserName ??
                        x.Email ??
                        x.UserCode
                })
                .ToDictionaryAsync(x => x.Id, x => x.Name);
        }

        private static void NormalizeAuditResponse(WorkforceBankAccountDetailResponse data)
        {
            if (data.UpdateDateTime.HasValue &&
                data.UpdateDateTime.Value == DateTime.MinValue)
            {
                data.UpdateDateTime = null;
            }

            if (!data.CreateBy.HasValue || data.CreateBy.Value == Guid.Empty)
            {
                data.CreateBy = null;
                data.CreateByName = null;
            }

            if (!data.UpdateBy.HasValue || data.UpdateBy.Value == Guid.Empty)
            {
                data.UpdateBy = null;
                data.UpdateByName = null;
            }
        }

        private static string? GetActorName(
            IReadOnlyDictionary<Guid, string?> actorNames,
            Guid actorId)
        {
            if (actorId == Guid.Empty)
            {
                return null;
            }

            return actorNames.TryGetValue(actorId, out var actorName)
                ? actorName
                : null;
        }

        private static string BuildBankCategoryLabel(BankCategory value)
        {
            return value switch
            {
                BankCategory.Commercial => "Commercial",
                BankCategory.Syariah => "Syariah",
                BankCategory.DigitalBank => "Digital Bank",
                BankCategory.RuralBank => "Rural Bank",
                BankCategory.Other => "Other",
                _ => value.ToString()
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 25;
            }

            if (pageSize > 100)
            {
                pageSize = 100;
            }

            return (pageNumber, pageSize);
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

        private static string NormalizeRequiredText(string value)
        {
            return value.Trim();
        }

        private static string NormalizeAccountNumber(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : new string(value.Where(char.IsDigit).ToArray());
        }

        private static string MaskAccountNumber(string? value)
        {
            var normalized = NormalizeAccountNumber(value);

            if (normalized.Length <= 4)
            {
                return normalized;
            }

            var suffix = normalized[^4..];
            var maskedLength = normalized.Length - 4;

            return new string('*', maskedLength) + suffix;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}
