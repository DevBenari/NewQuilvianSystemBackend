using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Models;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Data;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Services;

/// <summary>
/// Service CRUD mata uang dan kurs harian milik Finance (BE-FIN-002/004, FIN-DEC-018).
/// Kurs (MstExchangeRate) bersifat riwayat append-only: tidak ada update/delete kurs, sesuai
/// FIN-API-1.0 yang hanya mengunci GET dan POST untuk sub-resource ini.
/// FIN-BE-CUR-RATE-001: daftar/detail mata uang kini memuat "Kurs ke IDR" terkini dan "Update Terakhir", dan
/// mata uang yang belum pernah dipakai boleh dihapus (soft delete) setelah pemeriksaan referensi menyeluruh.
/// </summary>
public sealed class CurrencyService
{
    private const string LogCategory = "Corporate.FinanceManagement.MasterData";

    // "Kurs ke IDR": nilai MstExchangeRate.MiddleRate dibaca sebagai Rupiah per satu unit mata uang asing. Semantik ini
    // sudah dipakai konsumen nyata (TRV-W4: BaseCurrencyAmount = ClaimedAmount x MiddleRate) dan dinyatakan pemilik Finance.
    private const string RateReferenceCurrencyCode = "IDR";
    private const int DefaultDecimalPlaces = 2;
    private const int MaxBlockerLabelsInMessage = 3;

    // Nama ramah untuk tabel yang lazim menyimpan kode mata uang. Tabel lain memakai nama tabelnya apa adanya.
    private static readonly Dictionary<string, string> ReferenceLabels = new(StringComparer.Ordinal)
    {
        ["MstBankAccount"] = "rekening bank",
        ["FinAccountingEventOutbox"] = "kejadian akuntansi Finance",
        ["AccAccountingEvent"] = "kejadian akuntansi"
    };

    // Kolom teks yang dipakai modul lain untuk menyimpan kode mata uang (tanpa foreign key).
    private static readonly string[] CurrencyCodeColumnNames = ["CurrencyCode", "Currency"];

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
        var entities = await query
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Kurs terkini, jumlah riwayat, aktivitas kurs terakhir, dan nama pengguna diambil BERKELOMPOK untuk satu halaman
        // (4 query tetap, bukan satu per baris) dan hanya satu baris kurs per mata uang yang dimuat, bukan seluruh riwayat.
        var items = await ProjectAsync(entities, cancellationToken);
        return new PagedResult<CurrencyResponse>
        {
            PageNumber = request.PageNumber, PageSize = request.PageSize, TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)request.PageSize), Items = items
        };
    }

    public async Task<CurrencyResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await ProjectSingleAsync(await FindAsync(id, cancellationToken), cancellationToken);

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
        EnsureProtectedCurrencyStaysActive(values.Code, request.IsBaseCurrency, request.IsActive);

        var entity = new MstCurrency
        {
            CurrencyCode = values.Code,
            CurrencyName = values.Name,
            Symbol = values.Symbol,
            DecimalPlaces = request.DecimalPlaces ?? DefaultDecimalPlaces,
            IsBaseCurrency = request.IsBaseCurrency,
            IsActive = request.IsActive,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };
        _dbContext.MstCurrencies.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("Currency.Create", entity, actorUserId);
        return await ProjectSingleAsync(entity, cancellationToken);
    }

    public async Task<CurrencyResponse> UpdateAsync(Guid id, UpdateCurrencyRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        var values = await ValidateAsync(request, id, cancellationToken);
        EnsureProtectedCurrencyStaysActive(values.Code, request.IsBaseCurrency, request.IsActive);

        // Kode mata uang disimpan sebagai teks pada rekening bank dan dokumen lain (tanpa foreign key). Mengubahnya setelah
        // dipakai akan memutus rujukan itu diam-diam, jadi ditolak selama masih ada rujukan.
        if (!string.Equals(entity.CurrencyCode, values.Code, StringComparison.Ordinal))
        {
            var references = await FindCodeReferencesAsync(entity.CurrencyCode, cancellationToken);
            if (references.Count > 0)
                throw new CurrencyConflictException(
                    $"Kode mata uang {entity.CurrencyCode} tidak dapat diubah karena sudah dipakai pada {DescribeReferences(references)}.");
        }

        entity.CurrencyCode = values.Code;
        entity.CurrencyName = values.Name;
        entity.Symbol = values.Symbol;

        // Tidak dikirim (null) = nilai tersimpan dipertahankan; pembulatan transaksi tidak boleh berubah diam-diam.
        if (request.DecimalPlaces.HasValue) entity.DecimalPlaces = request.DecimalPlaces.Value;

        entity.IsBaseCurrency = request.IsBaseCurrency;
        entity.IsActive = request.IsActive;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("Currency.Update", entity, actorUserId);
        return await ProjectSingleAsync(entity, cancellationToken);
    }

    public async Task<CurrencyResponse> UpdateStatusAsync(Guid id, UpdateCurrencyStatusRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        EnsureProtectedCurrencyStaysActive(entity.CurrencyCode, entity.IsBaseCurrency, request.IsActive);

        entity.IsActive = request.IsActive;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync(request.IsActive ? "Currency.Activate" : "Currency.Deactivate", entity, actorUserId);
        return await ProjectSingleAsync(entity, cancellationToken);
    }

    /// <summary>
    /// Soft delete (IsDelete = true), tidak pernah hard delete. Ditolak bila mata uang dasar, sudah punya riwayat kurs
    /// (termasuk yang sudah ditandai terhapus), atau kodenya dipakai tabel mana pun yang menyimpan kode mata uang.
    /// Riwayat kurs, rekening bank, dan transaksi tidak pernah disentuh.
    /// </summary>
    public async Task<CurrencyDeleteResponse> DeleteAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
    {
        // Pemeriksaan referensi dan penandaan terhapus berada dalam satu transaksi agar atomik. Rollback otomatis saat
        // keluar tanpa Commit (mis. karena penolakan di bawah).
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var entity = await FindAsync(id, cancellationToken);
        var blockers = new List<string>();

        if (entity.IsBaseCurrency || IsRateReferenceCurrency(entity.CurrencyCode))
            blockers.Add("merupakan mata uang dasar/acuan kurs");

        var rateCount = await _dbContext.MstExchangeRates.CountAsync(x => x.CurrencyId == entity.Id, cancellationToken);
        if (rateCount > 0)
            blockers.Add($"sudah memiliki {rateCount} riwayat kurs");

        var references = await FindCodeReferencesAsync(entity.CurrencyCode, cancellationToken);
        if (references.Count > 0)
            blockers.Add($"dipakai pada {DescribeReferences(references)}");

        if (blockers.Count > 0)
            throw new CurrencyInUseException(
                $"Mata uang {entity.CurrencyCode} tidak dapat dihapus karena {string.Join("; ", blockers)}. Nonaktifkan saja bila tidak dipakai lagi.");

        entity.IsDelete = true;
        entity.IsActive = false;
        entity.DeleteDateTime = DateTime.UtcNow;
        entity.DeleteBy = actorUserId;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        await AuditAsync("Currency.Delete", entity, actorUserId);

        return new CurrencyDeleteResponse
        {
            Id = entity.Id,
            CurrencyCode = entity.CurrencyCode,
            CurrencyName = entity.CurrencyName,
            IsDelete = entity.IsDelete
        };
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

    // Mata uang dasar dan IDR (acuan "Kurs ke IDR") tidak boleh dinonaktifkan: pilihan mata uang aktif untuk transaksi baru
    // (GET /options?onlyActive=true) akan kehilangan mata uang dasarnya. Keputusan memindahkan dasar operasional ada pada pemilik.
    private static void EnsureProtectedCurrencyStaysActive(string code, bool isBaseCurrency, bool isActive)
    {
        if (!isActive && (isBaseCurrency || IsRateReferenceCurrency(code)))
            throw new CurrencyValidationException(
                $"Mata uang dasar/acuan kurs ({RateReferenceCurrencyCode}) tidak dapat dinonaktifkan karena masih dipakai sebagai dasar transaksi.");
    }

    private static bool IsRateReferenceCurrency(string? code) =>
        string.Equals(code?.Trim(), RateReferenceCurrencyCode, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Menelusuri SEMUA tabel yang diketahui model EF dan memiliki kolom teks berisi kode mata uang (CurrencyCode/Currency),
    /// bukan hanya foreign key ke MstCurrency.Id — rekening bank, kejadian akuntansi, dan modul lain menyimpan kode sebagai
    /// teks tanpa foreign key. Baris yang sudah ditandai terhapus ikut dihitung karena tetap menjadi riwayat.
    /// Hanya membaca; nama tabel/kolom berasal dari metadata model (bukan input pengguna) dan kode dikirim sebagai parameter.
    /// </summary>
    private async Task<List<string>> FindCodeReferencesAsync(string currencyCode, CancellationToken cancellationToken)
    {
        var labels = new List<string>();
        var visited = new HashSet<string>(StringComparer.Ordinal);

        foreach (var entityType in _dbContext.Model.GetEntityTypes())
        {
            if (entityType.ClrType == typeof(MstCurrency)) continue;

            var tableName = entityType.GetTableName();
            if (string.IsNullOrEmpty(tableName)) continue;
            var schema = entityType.GetSchema() ?? "public";

            foreach (var propertyName in CurrencyCodeColumnNames)
            {
                var property = entityType.FindProperty(propertyName);
                if (property is null || property.ClrType != typeof(string)) continue;

                var columnName = property.GetColumnName(StoreObjectIdentifier.Table(tableName, schema));
                if (string.IsNullOrEmpty(columnName)) continue;
                if (!visited.Add($"{schema}.{tableName}.{columnName}")) continue;

                var sql = $"SELECT 1 AS \"Value\" FROM \"{schema}\".\"{tableName}\" WHERE UPPER(\"{columnName}\") = UPPER({{0}}) LIMIT 1";
                var exists = await _dbContext.Database.SqlQueryRaw<int>(sql, currencyCode).AnyAsync(cancellationToken);
                if (!exists) continue;

                var label = ReferenceLabels.TryGetValue(entityType.ClrType.Name, out var friendly) ? friendly : tableName;
                if (!labels.Contains(label)) labels.Add(label);
            }
        }

        return labels;
    }

    private static string DescribeReferences(IReadOnlyList<string> labels)
    {
        var shown = labels.Take(MaxBlockerLabelsInMessage).ToList();
        var text = string.Join(", ", shown);
        return labels.Count > shown.Count ? $"{text}, dan {labels.Count - shown.Count} data lain" : text;
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

    private async Task<CurrencyResponse> ProjectSingleAsync(MstCurrency entity, CancellationToken cancellationToken) =>
        (await ProjectAsync(new List<MstCurrency> { entity }, cancellationToken))[0];

    /// <summary>
    /// Memetakan mata uang ke response beserta Kurs ke IDR terkini dan Update Terakhir. Selalu dipakai oleh list, detail,
    /// create, update, dan status agar frontend yang menggabungkan response ke baris tabel tidak menimpa kurs dengan kosong.
    /// Query tetap (tidak bergantung jumlah baris): kurs terkini, jumlah riwayat, aktivitas kurs terakhir, nama pengguna.
    /// </summary>
    private async Task<List<CurrencyResponse>> ProjectAsync(IReadOnlyCollection<MstCurrency> entities, CancellationToken cancellationToken)
    {
        if (entities.Count == 0) return new List<CurrencyResponse>();

        var ids = entities.Select(x => x.Id).ToList();
        var referenceDate = DateOnly.FromDateTime(AppDateTimeHelper.OperationalDate());

        // Kurs berlaku = RateDate terbaru yang tidak melampaui hari ini (zona waktu aplikasi), belum terhapus, dan bernilai > 0.
        // Kurs bertanggal masa depan tidak dianggap kurs saat ini. Sama dengan aturan konsumen TRV-W4.
        var rateRows = await _dbContext.MstExchangeRates.AsNoTracking()
            .Where(r => !r.IsDelete && ids.Contains(r.CurrencyId) && r.MiddleRate > 0 && r.RateDate <= referenceDate
                && r.RateDate == _dbContext.MstExchangeRates
                    .Where(x => !x.IsDelete && x.CurrencyId == r.CurrencyId && x.MiddleRate > 0 && x.RateDate <= referenceDate)
                    .Max(x => (DateOnly?)x.RateDate))
            .Select(r => new { r.CurrencyId, r.RateDate, r.MiddleRate, r.Source })
            .ToListAsync(cancellationToken);
        var currentRates = rateRows.GroupBy(r => r.CurrencyId).ToDictionary(g => g.Key, g => g.First());

        var countRows = await _dbContext.MstExchangeRates.AsNoTracking()
            .Where(r => !r.IsDelete && ids.Contains(r.CurrencyId))
            .GroupBy(r => r.CurrencyId)
            .Select(g => new { CurrencyId = g.Key, Total = g.Count() })
            .ToListAsync(cancellationToken);
        var rateCounts = countRows.ToDictionary(x => x.CurrencyId, x => x.Total);

        // Waktu pencatatan kurs (CreateDateTime), BUKAN RateDate: RateDate adalah tanggal berlaku, bukan saat pengguna bekerja.
        var activityRows = await _dbContext.MstExchangeRates.AsNoTracking()
            .Where(r => !r.IsDelete && ids.Contains(r.CurrencyId)
                && r.CreateDateTime == _dbContext.MstExchangeRates
                    .Where(x => !x.IsDelete && x.CurrencyId == r.CurrencyId)
                    .Max(x => (DateTime?)x.CreateDateTime))
            .Select(r => new { r.CurrencyId, r.CreateDateTime, r.CreateBy })
            .ToListAsync(cancellationToken);
        var rateActivities = activityRows
            .GroupBy(r => r.CurrencyId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.CreateDateTime).First());

        // Update Terakhir = aktivitas terbaru antara perubahan master (UpdateDateTime/UpdateBy, atau CreateDateTime/CreateBy
        // bila belum pernah diubah) dan pencatatan kurs. Pelaku diambil dari audit, bukan pengguna yang sedang login.
        var activities = new Dictionary<Guid, (DateTime At, Guid By, string Source)>();
        var actorIds = new HashSet<Guid>();
        foreach (var entity in entities)
        {
            var activity = (
                At: entity.UpdateDateTime ?? entity.CreateDateTime,
                By: entity.UpdateDateTime.HasValue ? entity.UpdateBy : entity.CreateBy,
                Source: CurrencyLastUpdatedSources.Currency);

            if (rateActivities.TryGetValue(entity.Id, out var rateActivity) && rateActivity.CreateDateTime > activity.At)
                activity = (rateActivity.CreateDateTime, rateActivity.CreateBy, CurrencyLastUpdatedSources.ExchangeRate);

            activities[entity.Id] = activity;
            if (activity.By != Guid.Empty) actorIds.Add(activity.By);
        }

        var actorNames = new Dictionary<Guid, string>();
        if (actorIds.Count > 0)
        {
            var actorIdList = actorIds.ToList();
            var users = await _dbContext.Users.AsNoTracking()
                .Where(u => actorIdList.Contains(u.Id))
                .Select(u => new { u.Id, u.DisplayName, u.UserName, u.UserCode })
                .ToListAsync(cancellationToken);
            foreach (var user in users)
            {
                var name = !string.IsNullOrWhiteSpace(user.DisplayName) ? user.DisplayName
                    : !string.IsNullOrWhiteSpace(user.UserName) ? user.UserName
                    : user.UserCode;
                if (!string.IsNullOrWhiteSpace(name)) actorNames[user.Id] = name;
            }
        }

        var responses = new List<CurrencyResponse>(entities.Count);
        foreach (var entity in entities)
        {
            var response = Map(entity);

            if (IsRateReferenceCurrency(entity.CurrencyCode))
            {
                response.CurrentRateToIdr = 1m;
                response.CurrentRateBasis = CurrencyRateBases.Identity;
            }
            else if (currentRates.TryGetValue(entity.Id, out var current))
            {
                response.CurrentRateToIdr = current.MiddleRate;
                response.CurrentRateDate = current.RateDate;
                response.CurrentRateSource = current.Source;
                response.CurrentRateBasis = CurrencyRateBases.Middle;
            }

            response.ExchangeRateCount = rateCounts.TryGetValue(entity.Id, out var count) ? count : 0;

            var last = activities[entity.Id];
            response.LastUpdatedAt = last.At;
            response.LastUpdatedSource = last.Source;
            response.LastUpdatedByName = actorNames.TryGetValue(last.By, out var actorName) ? actorName : null;

            responses.Add(response);
        }

        return responses;
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
public sealed class CurrencyInUseException(string message) : Exception(message);
