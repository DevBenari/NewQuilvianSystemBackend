using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Services;

/// <summary>
/// Service CRUD mata uang dan kurs harian milik Finance (BE-FIN-002/004, FIN-DEC-018).
/// Kurs (MstExchangeRate) bersifat riwayat append-only: tidak ada update/delete, sesuai
/// FIN-API-1.0 yang hanya mengunci GET dan POST untuk sub-resource ini.
/// </summary>
public sealed class CurrencyService
{
    private const string LogCategory = "Corporate.FinanceManagement.MasterData";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public CurrencyService(ApplicationDbContext dbContext, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    public async Task<PagedResult<CurrencyResponse>> GetPagedAsync(CurrencyQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.MstCurrencies.AsNoTracking().Where(x => !x.IsDelete);
        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpper();
            query = query.Where(x => x.CurrencyCode.ToUpper().Contains(search) || x.CurrencyName.ToUpper().Contains(search));
        }

        var descending = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = request.SortBy.Trim().ToLowerInvariant() switch
        {
            "currencycode" => descending ? query.OrderByDescending(x => x.CurrencyCode) : query.OrderBy(x => x.CurrencyCode),
            "isactive" => descending ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
            _ => descending ? query.OrderByDescending(x => x.CurrencyName) : query.OrderBy(x => x.CurrencyName)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .Select(x => Map(x)).ToListAsync(cancellationToken);
        return new PagedResult<CurrencyResponse>
        {
            PageNumber = request.PageNumber, PageSize = request.PageSize, TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)request.PageSize), Items = items
        };
    }

    public async Task<CurrencyResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Map(await FindAsync(id, cancellationToken));

    public async Task<List<CurrencyOptionResponse>> GetOptionsAsync(bool onlyActive, CancellationToken cancellationToken)
    {
        var query = _dbContext.MstCurrencies.AsNoTracking().Where(x => !x.IsDelete);
        if (onlyActive) query = query.Where(x => x.IsActive);

        return await query
            .OrderByDescending(x => x.IsBaseCurrency).ThenBy(x => x.CurrencyName)
            .Select(x => new CurrencyOptionResponse
            {
                Id = x.Id,
                CurrencyCode = x.CurrencyCode,
                CurrencyName = x.CurrencyName,
                Symbol = x.Symbol,
                IsBaseCurrency = x.IsBaseCurrency
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<CurrencySummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var query = _dbContext.MstCurrencies.AsNoTracking().Where(x => !x.IsDelete);
        return new CurrencySummaryResponse
        {
            TotalCurrency = await query.CountAsync(cancellationToken),
            ActiveCurrency = await query.CountAsync(x => x.IsActive, cancellationToken),
            InactiveCurrency = await query.CountAsync(x => !x.IsActive, cancellationToken)
        };
    }

    public Task<CurrencyFilterMetadataResponse> GetFilterMetadataAsync(CancellationToken cancellationToken) =>
        Task.FromResult(new CurrencyFilterMetadataResponse
        {
            DefaultFilter = new CurrencyDefaultFilterResponse(),
            PageSizeOptions = [10, 25, 50, 100],
            SortableFields = ["currencyCode", "currencyName", "isActive"]
        });

    public async Task<CurrencyResponse> CreateAsync(CreateCurrencyRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var values = await ValidateAsync(request, null, cancellationToken);
        var entity = new MstCurrency
        {
            CurrencyCode = values.Code,
            CurrencyName = values.Name,
            Symbol = values.Symbol,
            DecimalPlaces = request.DecimalPlaces,
            IsBaseCurrency = request.IsBaseCurrency,
            IsActive = request.IsActive,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };
        _dbContext.MstCurrencies.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("Currency.Create", entity, actorUserId);
        return Map(entity);
    }

    public async Task<CurrencyResponse> UpdateAsync(Guid id, UpdateCurrencyRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        var values = await ValidateAsync(request, id, cancellationToken);
        entity.CurrencyCode = values.Code;
        entity.CurrencyName = values.Name;
        entity.Symbol = values.Symbol;
        entity.DecimalPlaces = request.DecimalPlaces;
        entity.IsBaseCurrency = request.IsBaseCurrency;
        entity.IsActive = request.IsActive;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("Currency.Update", entity, actorUserId);
        return Map(entity);
    }

    public async Task<CurrencyResponse> UpdateStatusAsync(Guid id, UpdateCurrencyStatusRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        entity.IsActive = request.IsActive;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync(request.IsActive ? "Currency.Activate" : "Currency.Deactivate", entity, actorUserId);
        return Map(entity);
    }

    public async Task<PagedResult<ExchangeRateResponse>> GetExchangeRatesAsync(Guid currencyId, ExchangeRateQuery request, CancellationToken cancellationToken)
    {
        var currency = await FindAsync(currencyId, cancellationToken);
        var query = _dbContext.MstExchangeRates.AsNoTracking().Where(x => !x.IsDelete && x.CurrencyId == currencyId);
        if (request.StartDate.HasValue) query = query.Where(x => x.RateDate >= request.StartDate.Value);
        if (request.EndDate.HasValue) query = query.Where(x => x.RateDate <= request.EndDate.Value);

        var descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        query = descending ? query.OrderByDescending(x => x.RateDate) : query.OrderBy(x => x.RateDate);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .Select(x => MapRate(x, currency.CurrencyCode)).ToListAsync(cancellationToken);
        return new PagedResult<ExchangeRateResponse>
        {
            PageNumber = request.PageNumber, PageSize = request.PageSize, TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)request.PageSize), Items = items
        };
    }

    public async Task<ExchangeRateResponse> CreateExchangeRateAsync(
        Guid currencyId, CreateExchangeRateRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var currency = await FindAsync(currencyId, cancellationToken);
        if (await _dbContext.MstExchangeRates.AnyAsync(
                x => !x.IsDelete && x.CurrencyId == currencyId && x.RateDate == request.RateDate, cancellationToken))
            throw new CurrencyConflictException(
                $"Kurs {currency.CurrencyCode} untuk tanggal {request.RateDate:yyyy-MM-dd} sudah pernah dicatat.");

        var entity = new MstExchangeRate
        {
            CurrencyId = currencyId,
            RateDate = request.RateDate,
            BuyRate = request.BuyRate,
            SellRate = request.SellRate,
            MiddleRate = request.MiddleRate,
            Source = string.IsNullOrWhiteSpace(request.Source) ? null : request.Source.Trim(),
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };
        _dbContext.MstExchangeRates.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _loggerService.AuditAsync(LogCategory, "Currency.ExchangeRate.Create", "Pencatatan kurs harian.", new
        {
            ExchangeRateId = entity.Id,
            entity.CurrencyId,
            CurrencyCode = currency.CurrencyCode,
            RateDate = entity.RateDate.ToString("yyyy-MM-dd"),
            ActorUserId = actorUserId
        });
        return MapRate(entity, currency.CurrencyCode);
    }

    private async Task<(string Code, string Name, string? Symbol)> ValidateAsync(
        CreateCurrencyRequest request, Guid? excludedId, CancellationToken cancellationToken)
    {
        var code = Required(request.CurrencyCode, "CurrencyCode").ToUpperInvariant();
        if (code.Length != 3) throw new CurrencyValidationException("Kode mata uang harus terdiri dari 3 huruf, contoh IDR.");
        var name = Required(request.CurrencyName, "CurrencyName");
        var symbol = string.IsNullOrWhiteSpace(request.Symbol) ? null : request.Symbol.Trim();

        if (await _dbContext.MstCurrencies.AnyAsync(x => !x.IsDelete && x.Id != excludedId && x.CurrencyCode == code, cancellationToken))
            throw new CurrencyConflictException("Kode mata uang sudah dipakai mata uang lain. Gunakan kode yang berbeda.");

        // FR-FIN-003: hanya satu mata uang dasar boleh aktif — ditolak, bukan menggeser otomatis.
        if (request.IsBaseCurrency
            && await _dbContext.MstCurrencies.AnyAsync(x => !x.IsDelete && x.Id != excludedId && x.IsBaseCurrency, cancellationToken))
            throw new CurrencyValidationException(
                "Sudah ada mata uang dasar lain yang aktif. Nonaktifkan status mata uang dasar itu terlebih dahulu sebelum menandai mata uang ini.");

        return (code, name, symbol);
    }

    private async Task<MstCurrency> FindAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.MstCurrencies.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
        ?? throw new KeyNotFoundException("Mata uang tidak ditemukan.");

    private Task AuditAsync(string action, MstCurrency entity, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, action, "Perubahan data mata uang.", new
        {
            CurrencyId = entity.Id,
            entity.CurrencyCode,
            entity.IsBaseCurrency,
            entity.IsActive,
            ActorUserId = actorUserId
        });

    private static string Required(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new CurrencyValidationException($"{field} wajib diisi.");
        return value.Trim();
    }

    private static CurrencyResponse Map(MstCurrency entity) => new()
    {
        Id = entity.Id,
        CurrencyCode = entity.CurrencyCode,
        CurrencyName = entity.CurrencyName,
        Symbol = entity.Symbol,
        DecimalPlaces = entity.DecimalPlaces,
        IsBaseCurrency = entity.IsBaseCurrency,
        IsActive = entity.IsActive,
        CreateDateTime = entity.CreateDateTime,
        UpdateDateTime = entity.UpdateDateTime
    };

    private static ExchangeRateResponse MapRate(MstExchangeRate entity, string currencyCode) => new()
    {
        Id = entity.Id,
        CurrencyId = entity.CurrencyId,
        CurrencyCode = currencyCode,
        RateDate = entity.RateDate,
        BuyRate = entity.BuyRate,
        SellRate = entity.SellRate,
        MiddleRate = entity.MiddleRate,
        Source = entity.Source,
        CreateDateTime = entity.CreateDateTime
    };
}

public sealed class CurrencyValidationException(string message) : Exception(message);
public sealed class CurrencyConflictException(string message) : Exception(message);
