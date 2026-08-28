using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Services;

public sealed class RoomChargePolicyService
{
    private const string LogCategory = "HealthServices.BillingManagement.MasterData";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public RoomChargePolicyService(ApplicationDbContext dbContext, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    public async Task<PagedResult<RoomChargePolicyResponse>> GetPagedAsync(RoomChargePolicyQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.MstRoomChargePolicies.AsNoTracking().Where(x => !x.IsDelete);
        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
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
        var items = await query.OrderByDescending(x => x.EffectiveFrom)
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .Select(x => Map(x)).ToListAsync(cancellationToken);
        return new PagedResult<RoomChargePolicyResponse>
        {
            PageNumber = request.PageNumber, PageSize = request.PageSize, TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)request.PageSize), Items = items
        };
    }

    public async Task<RoomChargePolicyResponse> CreateAsync(CreateRoomChargePolicyRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var values = await ValidateAsync(request, null, cancellationToken);
        var entity = new MstRoomChargePolicy
        {
            Code = values.Code, Name = values.Name, MinimumMinutes = request.MinimumMinutes, PeriodMinutes = request.PeriodMinutes,
            RemainderRounding = values.RemainderRounding, TariffMoment = values.TariffMoment, LeaveRule = values.LeaveRule,
            EffectiveFrom = request.EffectiveFrom, EffectiveTo = request.EffectiveTo, IsActive = request.IsActive,
            CreateDateTime = DateTime.UtcNow, CreateBy = actorUserId
        };
        _dbContext.MstRoomChargePolicies.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("RoomChargePolicy.Create", entity, actorUserId, null);
        return Map(entity);
    }

    public async Task<RoomChargePolicyResponse> UpdateAsync(Guid id, UpdateRoomChargePolicyRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (entity.EffectiveFrom <= DateTimeOffset.UtcNow)
            throw new RoomChargePolicyValidationException("Room charge policy yang sudah efektif tidak dapat diubah; buat versi baru.");
        var values = await ValidateAsync(request, id, cancellationToken);
        entity.Code = values.Code; entity.Name = values.Name; entity.MinimumMinutes = request.MinimumMinutes;
        entity.PeriodMinutes = request.PeriodMinutes; entity.RemainderRounding = values.RemainderRounding;
        entity.TariffMoment = values.TariffMoment; entity.LeaveRule = values.LeaveRule;
        entity.EffectiveFrom = request.EffectiveFrom; entity.EffectiveTo = request.EffectiveTo; entity.IsActive = request.IsActive;
        entity.UpdateDateTime = DateTime.UtcNow; entity.UpdateBy = actorUserId;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("RoomChargePolicy.Update", entity, actorUserId, null);
        return Map(entity);
    }

    public async Task<RoomChargePolicyResponse> DeactivateAsync(Guid id, DeactivatePolicyRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Reason)) throw new RoomChargePolicyValidationException("Alasan penonaktifan wajib diisi.");
        var entity = await FindAsync(id, cancellationToken);
        entity.IsActive = false; entity.UpdateDateTime = DateTime.UtcNow; entity.UpdateBy = actorUserId;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("RoomChargePolicy.Deactivate", entity, actorUserId, request.Reason.Trim());
        return Map(entity);
    }

    // Mengaktifkan kembali policy yang sebelumnya dinonaktifkan tanpa perlu membuat versi baru.
    // Tidak mensyaratkan alasan (berbeda dari DeactivateAsync).
    public async Task<RoomChargePolicyResponse> ActivateAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (entity.IsActive) return Map(entity);

        var overlaps = await _dbContext.MstRoomChargePolicies.AnyAsync(x => !x.IsDelete && x.IsActive && x.Id != entity.Id
            && x.EffectiveFrom < (entity.EffectiveTo ?? DateTimeOffset.MaxValue)
            && (x.EffectiveTo == null || entity.EffectiveFrom < x.EffectiveTo), cancellationToken);
        if (overlaps)
            throw new RoomChargePolicyConflictException("Tidak dapat mengaktifkan; ada room charge policy lain yang masih aktif dan bertumpang tindih periodenya.");

        entity.IsActive = true;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("RoomChargePolicy.Activate", entity, actorUserId, null);
        return Map(entity);
    }

    // Soft delete - baris tidak pernah dihapus fisik agar riwayat RoomChargeCalculationResponse
    // yang sudah menyimpan snapshot PolicyId/Code tetap dapat ditelusuri. Hanya policy nonaktif
    // yang boleh dihapus.
    public async Task<RoomChargePolicyDeleteResponse> DeleteAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (entity.IsActive)
            throw new RoomChargePolicyValidationException("Policy yang masih aktif tidak dapat dihapus; nonaktifkan terlebih dahulu.");

        entity.IsDelete = true;
        entity.DeleteDateTime = DateTime.UtcNow;
        entity.DeleteBy = actorUserId;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("RoomChargePolicy.Delete", entity, actorUserId, null);

        return new RoomChargePolicyDeleteResponse
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            IsDelete = entity.IsDelete
        };
    }

    public static decimal CalculateChargeUnits(int occupiedMinutes, int minimumMinutes, int periodMinutes, string remainderRounding)
    {
        if (occupiedMinutes < 0) throw new RoomChargePolicyValidationException("Occupied minutes tidak boleh negatif.");
        if (periodMinutes <= 0 || minimumMinutes < periodMinutes)
            throw new RoomChargePolicyValidationException("MinimumMinutes harus minimal satu PeriodMinutes dan keduanya harus positif.");
        var mode = Normalize(remainderRounding, RoomChargePolicyValues.RemainderRoundings, "RemainderRounding");
        var billableMinutes = Math.Max(occupiedMinutes, minimumMinutes);
        return mode switch
        {
            RoomChargePolicyValues.CeilingPeriod => decimal.Ceiling(billableMinutes / (decimal)periodMinutes),
            RoomChargePolicyValues.Proportional => billableMinutes / (decimal)periodMinutes,
            RoomChargePolicyValues.WholePeriods => decimal.Floor(billableMinutes / (decimal)periodMinutes),
            _ => throw new RoomChargePolicyValidationException("RemainderRounding tidak didukung.")
        };
    }

    private async Task<(string Code, string Name, string RemainderRounding, string TariffMoment, string LeaveRule)> ValidateAsync(
        CreateRoomChargePolicyRequest request, Guid? excludedId, CancellationToken cancellationToken)
    {
        var code = Required(request.Code, "Code").ToUpperInvariant();
        var name = Required(request.Name, "Name");
        var rounding = Normalize(request.RemainderRounding, RoomChargePolicyValues.RemainderRoundings, "RemainderRounding");
        var tariff = Normalize(request.TariffMoment, RoomChargePolicyValues.TariffMoments, "TariffMoment");
        var leave = Normalize(request.LeaveRule, RoomChargePolicyValues.LeaveRules, "LeaveRule");
        if (request.PeriodMinutes <= 0 || request.MinimumMinutes < request.PeriodMinutes)
            throw new RoomChargePolicyValidationException("MinimumMinutes harus minimal satu PeriodMinutes dan keduanya harus positif.");
        if (request.EffectiveTo.HasValue && request.EffectiveTo <= request.EffectiveFrom)
            throw new RoomChargePolicyValidationException("EffectiveTo harus lebih besar dari EffectiveFrom.");
        if (await _dbContext.MstRoomChargePolicies.AnyAsync(x => !x.IsDelete && x.Id != excludedId && x.Code == code, cancellationToken))
            throw new RoomChargePolicyConflictException("Kode room charge policy sudah digunakan.");
        if (request.IsActive && await _dbContext.MstRoomChargePolicies.AnyAsync(x => !x.IsDelete && x.IsActive && x.Id != excludedId
            && x.EffectiveFrom < (request.EffectiveTo ?? DateTimeOffset.MaxValue)
            && (x.EffectiveTo == null || request.EffectiveFrom < x.EffectiveTo), cancellationToken))
            throw new RoomChargePolicyConflictException("Periode room charge policy bertumpang tindih.");
        return (code, name, rounding, tariff, leave);
    }

    private async Task<MstRoomChargePolicy> FindAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.MstRoomChargePolicies.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
        ?? throw new KeyNotFoundException("Room charge policy tidak ditemukan.");
    private Task AuditAsync(string action, MstRoomChargePolicy entity, Guid actorUserId, string? reason) =>
        _loggerService.AuditAsync(LogCategory, action, "Perubahan room charge policy.", new
        {
            PolicyId = entity.Id, entity.Code, entity.MinimumMinutes, entity.PeriodMinutes, entity.RemainderRounding,
            entity.TariffMoment, entity.LeaveRule, entity.EffectiveFrom, entity.EffectiveTo, entity.IsActive,
            ActorUserId = actorUserId, Reason = reason
        });
    private static string Required(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new RoomChargePolicyValidationException($"{field} wajib diisi.");
        return value.Trim();
    }
    private static string Normalize(string? value, IReadOnlySet<string> allowed, string field)
    {
        var normalized = Required(value, field).ToUpperInvariant();
        if (!allowed.Contains(normalized)) throw new RoomChargePolicyValidationException($"{field} tidak didukung.");
        return normalized;
    }
    private static RoomChargePolicyResponse Map(MstRoomChargePolicy entity) => new()
    {
        Id = entity.Id, Code = entity.Code, Name = entity.Name, MinimumMinutes = entity.MinimumMinutes,
        PeriodMinutes = entity.PeriodMinutes, RemainderRounding = entity.RemainderRounding,
        TariffMoment = entity.TariffMoment, LeaveRule = entity.LeaveRule, EffectiveFrom = entity.EffectiveFrom,
        EffectiveTo = entity.EffectiveTo, IsActive = entity.IsActive, CreateDateTime = entity.CreateDateTime,
        UpdateDateTime = entity.UpdateDateTime
    };
}

public sealed class RoomChargePolicyValidationException(string message) : Exception(message);
public sealed class RoomChargePolicyConflictException(string message) : Exception(message);
