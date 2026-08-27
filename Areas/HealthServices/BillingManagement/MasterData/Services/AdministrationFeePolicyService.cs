using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Services;

public sealed class AdministrationFeePolicyService
{
    public const string BusinessTimeZoneId = "Asia/Jakarta";
    private const string WindowsBusinessTimeZoneId = "SE Asia Standard Time";
    private const string LogCategory = "HealthServices.BillingManagement.MasterData";

    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public AdministrationFeePolicyService(ApplicationDbContext dbContext, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    public async Task<PagedResult<AdministrationFeePolicyResponse>> GetPagedAsync(
        AdministrationFeePolicyQuery request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.MstAdministrationFeePolicies.AsNoTracking().Where(x => !x.IsDelete);

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        if (!string.IsNullOrWhiteSpace(request.ServiceType))
        {
            var serviceType = NormalizeServiceType(request.ServiceType);
            query = query.Where(x => x.ServiceType == serviceType);
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
        var items = await query.OrderBy(x => x.ServiceType)
            .ThenByDescending(x => x.EffectiveFrom)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => Map(x))
            .ToListAsync(cancellationToken);

        return new PagedResult<AdministrationFeePolicyResponse>
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)request.PageSize),
            Items = items
        };
    }

    public async Task<AdministrationFeePolicyResponse> CreateAsync(
        CreateAdministrationFeePolicyRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var normalized = await ValidateAsync(request, null, cancellationToken);
        var entity = new MstAdministrationFeePolicy
        {
            Code = normalized.Code,
            Name = normalized.Name,
            ServiceType = normalized.ServiceType,
            Amount = request.Amount,
            OncePerPatientLocalDay = IsOncePerPatientLocalDay(normalized.ServiceType),
            ReplacementPriority = ReplacementPriority(normalized.ServiceType),
            Coverable = request.Coverable,
            Discountable = false,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            IsActive = request.IsActive,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };

        _dbContext.MstAdministrationFeePolicies.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("AdministrationFeePolicy.Create", entity, actorUserId, null);
        return Map(entity);
    }

    public async Task<AdministrationFeePolicyResponse> UpdateAsync(
        Guid id,
        UpdateAdministrationFeePolicyRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (entity.EffectiveFrom <= DateTimeOffset.UtcNow)
            throw new AdministrationFeePolicyValidationException("Policy yang sudah efektif tidak dapat diubah; buat versi baru.");

        var normalized = await ValidateAsync(request, id, cancellationToken);
        entity.Code = normalized.Code;
        entity.Name = normalized.Name;
        entity.ServiceType = normalized.ServiceType;
        entity.Amount = request.Amount;
        entity.OncePerPatientLocalDay = IsOncePerPatientLocalDay(normalized.ServiceType);
        entity.ReplacementPriority = ReplacementPriority(normalized.ServiceType);
        entity.Coverable = request.Coverable;
        entity.Discountable = false;
        entity.EffectiveFrom = request.EffectiveFrom;
        entity.EffectiveTo = request.EffectiveTo;
        entity.IsActive = request.IsActive;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("AdministrationFeePolicy.Update", entity, actorUserId, null);
        return Map(entity);
    }

    public async Task<AdministrationFeePolicyResponse> DeactivateAsync(
        Guid id,
        DeactivatePolicyRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new AdministrationFeePolicyValidationException("Alasan penonaktifan wajib diisi.");

        var entity = await FindAsync(id, cancellationToken);
        entity.IsActive = false;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("AdministrationFeePolicy.Deactivate", entity, actorUserId, request.Reason.Trim());
        return Map(entity);
    }

    public static DateOnly GetBusinessDate(DateTimeOffset instant)
    {
        var zone = ResolveBusinessTimeZone();
        var local = TimeZoneInfo.ConvertTime(instant, zone);
        return DateOnly.FromDateTime(local.DateTime);
    }

    private async Task<MstAdministrationFeePolicy> FindAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.MstAdministrationFeePolicies.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
        ?? throw new KeyNotFoundException("Policy biaya administrasi tidak ditemukan.");

    private async Task<(string Code, string Name, string ServiceType)> ValidateAsync(
        CreateAdministrationFeePolicyRequest request,
        Guid? excludedId,
        CancellationToken cancellationToken)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        var name = request.Name.Trim();
        var serviceType = NormalizeServiceType(request.ServiceType);

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            throw new AdministrationFeePolicyValidationException("Kode dan nama policy wajib diisi.");
        if (request.EffectiveTo.HasValue && request.EffectiveTo <= request.EffectiveFrom)
            throw new AdministrationFeePolicyValidationException("EffectiveTo harus lebih besar dari EffectiveFrom.");
        if (request.Amount < 0 || (request.IsActive && request.Amount <= 0))
            throw new AdministrationFeePolicyValidationException("Policy aktif harus memiliki nominal lebih besar dari nol.");

        var codeExists = await _dbContext.MstAdministrationFeePolicies.AnyAsync(
            x => !x.IsDelete && x.Id != excludedId && x.Code == code,
            cancellationToken);
        if (codeExists)
            throw new AdministrationFeePolicyConflictException("Kode policy biaya administrasi sudah digunakan.");

        var overlaps = await _dbContext.MstAdministrationFeePolicies.AnyAsync(
            x => !x.IsDelete && x.Id != excludedId && x.ServiceType == serviceType
                && x.EffectiveFrom < (request.EffectiveTo ?? DateTimeOffset.MaxValue)
                && (x.EffectiveTo == null || request.EffectiveFrom < x.EffectiveTo),
            cancellationToken);
        if (overlaps)
            throw new AdministrationFeePolicyConflictException("Periode policy biaya administrasi bertumpang tindih untuk jenis layanan yang sama.");

        return (code, name, serviceType);
    }

    private Task AuditAsync(string action, MstAdministrationFeePolicy entity, Guid actorUserId, string? reason) =>
        _loggerService.AuditAsync(LogCategory, action, "Perubahan policy biaya administrasi.", new
        {
            PolicyId = entity.Id,
            entity.Code,
            entity.ServiceType,
            entity.Amount,
            entity.EffectiveFrom,
            entity.EffectiveTo,
            entity.IsActive,
            ActorUserId = actorUserId,
            Reason = reason
        });

    private static string NormalizeServiceType(string value)
    {
        var normalized = value.Trim().ToUpperInvariant();
        if (!AdministrationFeeServiceTypes.All.Contains(normalized))
            throw new AdministrationFeePolicyValidationException("ServiceType harus salah satu dari RAJAL, IGD, OTC, atau RANAP.");
        return normalized;
    }

    private static int ReplacementPriority(string serviceType) =>
        serviceType == AdministrationFeeServiceTypes.Ranap ? 100 : 10;

    private static bool IsOncePerPatientLocalDay(string serviceType) =>
        serviceType != AdministrationFeeServiceTypes.Ranap;

    private static TimeZoneInfo ResolveBusinessTimeZone()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById(BusinessTimeZoneId); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.FindSystemTimeZoneById(WindowsBusinessTimeZoneId); }
    }

    private static AdministrationFeePolicyResponse Map(MstAdministrationFeePolicy entity) => new()
    {
        Id = entity.Id,
        Code = entity.Code,
        Name = entity.Name,
        ServiceType = entity.ServiceType,
        Amount = entity.Amount,
        OncePerPatientLocalDay = entity.OncePerPatientLocalDay,
        ReplacementPriority = entity.ReplacementPriority,
        Coverable = entity.Coverable,
        Discountable = entity.Discountable,
        EffectiveFrom = entity.EffectiveFrom,
        EffectiveTo = entity.EffectiveTo,
        IsActive = entity.IsActive,
        CreateDateTime = entity.CreateDateTime,
        UpdateDateTime = entity.UpdateDateTime
    };
}

public sealed class AdministrationFeePolicyValidationException(string message) : Exception(message);
public sealed class AdministrationFeePolicyConflictException(string message) : Exception(message);
