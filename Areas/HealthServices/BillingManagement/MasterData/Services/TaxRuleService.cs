using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Services;

public sealed class TaxRuleService
{
    private const string LogCategory = "HealthServices.BillingManagement.MasterData";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public TaxRuleService(ApplicationDbContext dbContext, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    public async Task<PagedResult<TaxRuleResponse>> GetPagedAsync(TaxRuleQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.MstTaxRules.AsNoTracking().Where(x => !x.IsDelete);
        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
        if (!string.IsNullOrWhiteSpace(request.TaxableCategory))
        {
            var category = Required(request.TaxableCategory, "TaxableCategory").ToUpperInvariant();
            query = query.Where(x => x.TaxableCategory == category);
        }
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpper();
            query = query.Where(x => x.Code.ToUpper().Contains(search) || x.Name.ToUpper().Contains(search));
        }
        if (request.EffectiveAt.HasValue)
        {
            var instant = request.EffectiveAt.Value;
            query = query.Where(x => x.EffectiveFrom <= instant && (x.EffectiveTo == null || instant < x.EffectiveTo));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(x => x.TaxableCategory).ThenByDescending(x => x.EffectiveFrom)
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .Select(x => Map(x)).ToListAsync(cancellationToken);
        return new PagedResult<TaxRuleResponse>
        {
            PageNumber = request.PageNumber, PageSize = request.PageSize, TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)request.PageSize), Items = items
        };
    }

    public async Task<TaxRuleResponse> CreateAsync(CreateTaxRuleRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var values = await ValidateAsync(request, null, cancellationToken);
        var entity = new MstTaxRule
        {
            Code = values.Code, Name = values.Name, TaxableCategory = values.Category, Rate = request.Rate,
            RoundingMode = values.RoundingMode, AllocationRule = values.AllocationRule,
            EffectiveFrom = request.EffectiveFrom, EffectiveTo = request.EffectiveTo, IsActive = request.IsActive,
            CreateDateTime = DateTime.UtcNow, CreateBy = actorUserId
        };
        _dbContext.MstTaxRules.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("TaxRule.Create", entity, actorUserId, null);
        return Map(entity);
    }

    public async Task<TaxRuleResponse> UpdateAsync(Guid id, UpdateTaxRuleRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (entity.EffectiveFrom <= DateTimeOffset.UtcNow)
            throw new TaxRuleValidationException("Tax rule yang sudah efektif tidak dapat diubah; buat versi baru.");
        var values = await ValidateAsync(request, id, cancellationToken);
        entity.Code = values.Code; entity.Name = values.Name; entity.TaxableCategory = values.Category;
        entity.Rate = request.Rate; entity.RoundingMode = values.RoundingMode; entity.AllocationRule = values.AllocationRule;
        entity.EffectiveFrom = request.EffectiveFrom; entity.EffectiveTo = request.EffectiveTo; entity.IsActive = request.IsActive;
        entity.UpdateDateTime = DateTime.UtcNow; entity.UpdateBy = actorUserId;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("TaxRule.Update", entity, actorUserId, null);
        return Map(entity);
    }

    public async Task<TaxRuleResponse> DeactivateAsync(Guid id, DeactivatePolicyRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Reason)) throw new TaxRuleValidationException("Alasan penonaktifan wajib diisi.");
        var entity = await FindAsync(id, cancellationToken);
        entity.IsActive = false; entity.UpdateDateTime = DateTime.UtcNow; entity.UpdateBy = actorUserId;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("TaxRule.Deactivate", entity, actorUserId, request.Reason.Trim());
        return Map(entity);
    }

    // Mengaktifkan kembali tax rule yang sebelumnya dinonaktifkan tanpa perlu membuat versi baru.
    // Tidak mensyaratkan alasan (berbeda dari DeactivateAsync).
    public async Task<TaxRuleResponse> ActivateAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (entity.IsActive) return Map(entity);

        var overlaps = await _dbContext.MstTaxRules.AnyAsync(x => !x.IsDelete && x.IsActive && x.Id != entity.Id
            && x.TaxableCategory == entity.TaxableCategory
            && x.EffectiveFrom < (entity.EffectiveTo ?? DateTimeOffset.MaxValue)
            && (x.EffectiveTo == null || entity.EffectiveFrom < x.EffectiveTo), cancellationToken);
        if (overlaps)
            throw new TaxRuleConflictException("Tidak dapat mengaktifkan; ada tax rule lain yang masih aktif dan bertumpang tindih untuk taxable category yang sama.");

        entity.IsActive = true;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("TaxRule.Activate", entity, actorUserId, null);
        return Map(entity);
    }

    // Soft delete - baris tidak pernah dihapus fisik agar riwayat TaxCalculationResponse yang
    // sudah menyimpan snapshot TaxRuleId tetap dapat ditelusuri. Hanya tax rule nonaktif yang
    // boleh dihapus.
    public async Task<TaxRuleDeleteResponse> DeleteAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (entity.IsActive)
            throw new TaxRuleValidationException("Tax rule yang masih aktif tidak dapat dihapus; nonaktifkan terlebih dahulu.");

        entity.IsDelete = true;
        entity.DeleteDateTime = DateTime.UtcNow;
        entity.DeleteBy = actorUserId;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("TaxRule.Delete", entity, actorUserId, null);

        return new TaxRuleDeleteResponse
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            IsDelete = entity.IsDelete
        };
    }

    public static decimal CalculateTax(decimal grossAmount, decimal itemDiscount, decimal rate, string roundingMode, int decimalPlaces = 2)
    {
        if (grossAmount < 0 || itemDiscount < 0 || itemDiscount > grossAmount)
            throw new TaxRuleValidationException("Gross amount dan item discount tidak valid.");
        if (rate <= 0 || rate > 100) throw new TaxRuleValidationException("Rate pajak harus lebih dari 0 sampai 100 persen.");
        if (decimalPlaces is < 0 or > 6) throw new TaxRuleValidationException("Decimal places harus antara 0 dan 6.");
        var mode = Normalize(roundingMode, TaxRuleValues.RoundingModes, "RoundingMode");
        var unrounded = (grossAmount - itemDiscount) * rate / 100m;
        var factor = Pow10(decimalPlaces);
        var scaled = unrounded * factor;
        var rounded = mode switch
        {
            TaxRuleValues.HalfUp => decimal.Floor(scaled + 0.5m),
            TaxRuleValues.HalfEven => decimal.Round(scaled, 0, MidpointRounding.ToEven),
            TaxRuleValues.Up => decimal.Ceiling(scaled),
            TaxRuleValues.Down => decimal.Floor(scaled),
            _ => throw new TaxRuleValidationException("RoundingMode tidak didukung.")
        };
        return rounded / factor;
    }

    private async Task<(string Code, string Name, string Category, string RoundingMode, string AllocationRule)> ValidateAsync(
        CreateTaxRuleRequest request, Guid? excludedId, CancellationToken cancellationToken)
    {
        var code = Required(request.Code, "Code").ToUpperInvariant();
        var name = Required(request.Name, "Name");
        var category = Required(request.TaxableCategory, "TaxableCategory").ToUpperInvariant();
        var rounding = Normalize(request.RoundingMode, TaxRuleValues.RoundingModes, "RoundingMode");
        var allocation = Normalize(request.AllocationRule, TaxRuleValues.AllocationRules, "AllocationRule");
        if (request.Rate <= 0 || request.Rate > 100) throw new TaxRuleValidationException("Rate pajak harus lebih dari 0 sampai 100 persen.");
        if (request.EffectiveTo.HasValue && request.EffectiveTo <= request.EffectiveFrom)
            throw new TaxRuleValidationException("EffectiveTo harus lebih besar dari EffectiveFrom.");
        if (await _dbContext.MstTaxRules.AnyAsync(x => !x.IsDelete && x.Id != excludedId && x.Code == code, cancellationToken))
            throw new TaxRuleConflictException("Kode tax rule sudah digunakan.");
        if (request.IsActive && await _dbContext.MstTaxRules.AnyAsync(x => !x.IsDelete && x.IsActive && x.Id != excludedId && x.TaxableCategory == category
            && x.EffectiveFrom < (request.EffectiveTo ?? DateTimeOffset.MaxValue)
            && (x.EffectiveTo == null || request.EffectiveFrom < x.EffectiveTo), cancellationToken))
            throw new TaxRuleConflictException("Periode tax rule bertumpang tindih untuk taxable category yang sama.");
        return (code, name, category, rounding, allocation);
    }

    private async Task<MstTaxRule> FindAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.MstTaxRules.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
        ?? throw new KeyNotFoundException("Tax rule tidak ditemukan.");
    private Task AuditAsync(string action, MstTaxRule entity, Guid actorUserId, string? reason) =>
        _loggerService.AuditAsync(LogCategory, action, "Perubahan tax rule.", new
        {
            PolicyId = entity.Id, entity.Code, entity.TaxableCategory, entity.Rate, entity.RoundingMode,
            entity.AllocationRule, entity.EffectiveFrom, entity.EffectiveTo, entity.IsActive,
            ActorUserId = actorUserId, Reason = reason
        });
    private static decimal Pow10(int places) { var value = 1m; for (var i = 0; i < places; i++) value *= 10m; return value; }
    private static string Required(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new TaxRuleValidationException($"{field} wajib diisi.");
        return value.Trim();
    }
    private static string Normalize(string? value, IReadOnlySet<string> allowed, string field)
    {
        var normalized = Required(value, field).ToUpperInvariant();
        if (!allowed.Contains(normalized)) throw new TaxRuleValidationException($"{field} tidak didukung.");
        return normalized;
    }
    private static TaxRuleResponse Map(MstTaxRule entity) => new()
    {
        Id = entity.Id, Code = entity.Code, Name = entity.Name, TaxableCategory = entity.TaxableCategory,
        Rate = entity.Rate, RoundingMode = entity.RoundingMode, AllocationRule = entity.AllocationRule,
        EffectiveFrom = entity.EffectiveFrom, EffectiveTo = entity.EffectiveTo, IsActive = entity.IsActive,
        CreateDateTime = entity.CreateDateTime, UpdateDateTime = entity.UpdateDateTime
    };
}

public sealed class TaxRuleValidationException(string message) : Exception(message);
public sealed class TaxRuleConflictException(string message) : Exception(message);
