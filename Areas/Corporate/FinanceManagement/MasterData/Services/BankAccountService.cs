using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Services;

/// <summary>
/// Service CRUD rekening bank rumah sakit milik Finance (BE-FIN-002/004). BankId merujuk MstBank
/// existing milik Administrator/MasterData — service ini tidak pernah menulis MstBank.
/// </summary>
public sealed class BankAccountService
{
    private const string LogCategory = "Corporate.FinanceManagement.MasterData";
    private static readonly string[] ValidAccountTypes = ["OPERATIONAL", "COLLECTION", "PAYMENT"];
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public BankAccountService(ApplicationDbContext dbContext, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    public async Task<PagedResult<BankAccountResponse>> GetPagedAsync(BankAccountQuery request, CancellationToken cancellationToken)
    {
        var query = BaseQuery();
        if (request.BankId.HasValue) query = query.Where(x => x.Entity.BankId == request.BankId.Value);
        if (!string.IsNullOrWhiteSpace(request.AccountType)) query = query.Where(x => x.Entity.AccountType == request.AccountType);
        if (request.IsActive.HasValue) query = query.Where(x => x.Entity.IsActive == request.IsActive.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpper();
            query = query.Where(x => x.Entity.AccountName.ToUpper().Contains(search)
                || x.Entity.AccountNumber.ToUpper().Contains(search)
                || x.Bank.BankName.ToUpper().Contains(search));
        }

        var descending = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = request.SortBy.Trim().ToLowerInvariant() switch
        {
            "accountnumber" => descending ? query.OrderByDescending(x => x.Entity.AccountNumber) : query.OrderBy(x => x.Entity.AccountNumber),
            "bankname" => descending ? query.OrderByDescending(x => x.Bank.BankName) : query.OrderBy(x => x.Bank.BankName),
            "isactive" => descending ? query.OrderByDescending(x => x.Entity.IsActive) : query.OrderBy(x => x.Entity.IsActive),
            _ => descending ? query.OrderByDescending(x => x.Entity.AccountName) : query.OrderBy(x => x.Entity.AccountName)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .Select(x => Map(x.Entity, x.Bank)).ToListAsync(cancellationToken);
        return new PagedResult<BankAccountResponse>
        {
            PageNumber = request.PageNumber, PageSize = request.PageSize, TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)request.PageSize), Items = items
        };
    }

    public async Task<BankAccountResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var (entity, bank) = await FindAsync(id, cancellationToken);
        return Map(entity, bank);
    }

    public async Task<List<BankAccountOptionResponse>> GetOptionsAsync(bool onlyActive, string? accountType, CancellationToken cancellationToken)
    {
        var query = BaseQuery();
        if (onlyActive) query = query.Where(x => x.Entity.IsActive);
        if (!string.IsNullOrWhiteSpace(accountType)) query = query.Where(x => x.Entity.AccountType == accountType);

        return await query
            .OrderBy(x => x.Bank.BankName).ThenBy(x => x.Entity.AccountName)
            .Select(x => new BankAccountOptionResponse
            {
                Id = x.Entity.Id,
                BankName = x.Bank.BankName,
                AccountNumber = x.Entity.AccountNumber,
                AccountName = x.Entity.AccountName,
                AccountType = x.Entity.AccountType
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<BankAccountSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var query = _dbContext.MstBankAccounts.AsNoTracking().Where(x => !x.IsDelete);
        return new BankAccountSummaryResponse
        {
            TotalAccount = await query.CountAsync(cancellationToken),
            ActiveAccount = await query.CountAsync(x => x.IsActive, cancellationToken),
            InactiveAccount = await query.CountAsync(x => !x.IsActive, cancellationToken)
        };
    }

    public Task<BankAccountFilterMetadataResponse> GetFilterMetadataAsync(CancellationToken cancellationToken) =>
        Task.FromResult(new BankAccountFilterMetadataResponse
        {
            DefaultFilter = new BankAccountDefaultFilterResponse(),
            PageSizeOptions = [10, 25, 50, 100],
            SortableFields = ["accountName", "accountNumber", "bankName", "isActive"],
            AccountTypeOptions = [.. ValidAccountTypes]
        });

    public async Task<BankAccountResponse> CreateAsync(CreateBankAccountRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var values = await ValidateAsync(request, null, cancellationToken);
        var entity = new MstBankAccount
        {
            BankId = values.BankId,
            AccountNumber = values.AccountNumber,
            AccountName = values.AccountName,
            AccountType = values.AccountType,
            CurrencyCode = values.CurrencyCode,
            IsActive = request.IsActive,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };
        _dbContext.MstBankAccounts.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("BankAccount.Create", entity, actorUserId);
        return Map(entity, values.Bank);
    }

    public async Task<BankAccountResponse> UpdateAsync(Guid id, UpdateBankAccountRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var (entity, _) = await FindAsync(id, cancellationToken);
        var values = await ValidateAsync(request, id, cancellationToken);
        entity.BankId = values.BankId;
        entity.AccountNumber = values.AccountNumber;
        entity.AccountName = values.AccountName;
        entity.AccountType = values.AccountType;
        entity.CurrencyCode = values.CurrencyCode;
        entity.IsActive = request.IsActive;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("BankAccount.Update", entity, actorUserId);
        return Map(entity, values.Bank);
    }

    public async Task<BankAccountResponse> UpdateStatusAsync(Guid id, UpdateBankAccountStatusRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var (entity, bank) = await FindAsync(id, cancellationToken);
        entity.IsActive = request.IsActive;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync(request.IsActive ? "BankAccount.Activate" : "BankAccount.Deactivate", entity, actorUserId);
        return Map(entity, bank);
    }

    public async Task<BankAccountDeleteResponse> DeleteAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
    {
        var (entity, _) = await FindAsync(id, cancellationToken);
        entity.IsDelete = true;
        entity.DeleteDateTime = DateTime.UtcNow;
        entity.DeleteBy = actorUserId;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("BankAccount.Delete", entity, actorUserId);

        return new BankAccountDeleteResponse
        {
            Id = entity.Id,
            AccountNumber = entity.AccountNumber,
            AccountName = entity.AccountName,
            IsDelete = entity.IsDelete
        };
    }

    private IQueryable<(MstBankAccount Entity, MstBank Bank)> BaseQuery() =>
        from entity in _dbContext.MstBankAccounts.AsNoTracking()
        join bank in _dbContext.MstBanks.AsNoTracking() on entity.BankId equals bank.Id
        where !entity.IsDelete
        select new ValueTuple<MstBankAccount, MstBank>(entity, bank);

    private async Task<(Guid BankId, string AccountNumber, string AccountName, string AccountType, string CurrencyCode, MstBank Bank)> ValidateAsync(
        CreateBankAccountRequest request, Guid? excludedId, CancellationToken cancellationToken)
    {
        var bank = await _dbContext.MstBanks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.BankId && !x.IsDelete, cancellationToken)
            ?? throw new BankAccountValidationException("Bank yang dipilih tidak ditemukan. Pilih bank lain.");

        var accountNumber = Required(request.AccountNumber, "AccountNumber");
        var accountName = Required(request.AccountName, "AccountName");
        var accountType = Required(request.AccountType, "AccountType").ToUpperInvariant();
        if (!ValidAccountTypes.Contains(accountType))
            throw new BankAccountValidationException(
                $"Jenis rekening tidak dikenali. Pilih salah satu: {string.Join(", ", ValidAccountTypes)}.");
        var currencyCode = string.IsNullOrWhiteSpace(request.CurrencyCode) ? "IDR" : request.CurrencyCode.Trim().ToUpperInvariant();
        if (currencyCode.Length != 3)
            throw new BankAccountValidationException("Kode mata uang harus terdiri dari 3 huruf, contoh IDR.");

        // BIL-VAL setara: nomor rekening ganda pada bank yang sama ditolak (roadmap BE-FIN-004 AC).
        if (await _dbContext.MstBankAccounts.AnyAsync(
                x => !x.IsDelete && x.Id != excludedId && x.BankId == request.BankId && x.AccountNumber == accountNumber, cancellationToken))
            throw new BankAccountConflictException("Nomor rekening ini sudah terdaftar untuk bank yang sama. Gunakan nomor rekening lain.");

        return (request.BankId, accountNumber, accountName, accountType, currencyCode, bank);
    }

    private async Task<(MstBankAccount Entity, MstBank Bank)> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await BaseQuery().FirstOrDefaultAsync(x => x.Entity.Id == id, cancellationToken);
        if (result.Entity is null) throw new KeyNotFoundException("Rekening bank tidak ditemukan.");
        return result;
    }

    // Sensitif: AccountNumber MUST NOT masuk custom logger (data-dictionary.md §Sensitif).
    private Task AuditAsync(string action, MstBankAccount entity, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, action, "Perubahan rekening bank Finance.", new
        {
            BankAccountId = entity.Id,
            entity.BankId,
            entity.AccountType,
            entity.IsActive,
            ActorUserId = actorUserId
        });

    private static string Required(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new BankAccountValidationException($"{field} wajib diisi.");
        return value.Trim();
    }

    private static BankAccountResponse Map(MstBankAccount entity, MstBank bank) => new()
    {
        Id = entity.Id,
        BankId = entity.BankId,
        BankCode = bank.BankCode,
        BankName = bank.BankName,
        AccountNumber = entity.AccountNumber,
        AccountName = entity.AccountName,
        AccountType = entity.AccountType,
        CurrencyCode = entity.CurrencyCode,
        IsActive = entity.IsActive,
        CreateDateTime = entity.CreateDateTime,
        UpdateDateTime = entity.UpdateDateTime
    };
}

public sealed class BankAccountValidationException(string message) : Exception(message);
public sealed class BankAccountConflictException(string message) : Exception(message);
