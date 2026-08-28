using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Services;

public sealed class DiscountPolicyService
{
    private const string LogCategory = "HealthServices.BillingManagement.MasterData";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public DiscountPolicyService(ApplicationDbContext dbContext, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    public async Task<PagedResult<DiscountPolicyResponse>> GetPagedAsync(
        DiscountPolicyQuery request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.MstDiscountPolicies.AsNoTracking().Where(x => !x.IsDelete);
        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        if (!string.IsNullOrWhiteSpace(request.DiscountType))
        {
            var discountType = Normalize(request.DiscountType, DiscountPolicyValues.DiscountTypes, "DiscountType");
            query = query.Where(x => x.DiscountType == discountType);
        }
        if (!string.IsNullOrWhiteSpace(request.TargetComponent))
        {
            var target = Normalize(request.TargetComponent, DiscountPolicyValues.TargetComponents, "TargetComponent");
            query = query.Where(x => x.TargetComponent == target);
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
        var items = await query.OrderBy(x => x.DiscountType)
            .ThenBy(x => x.TargetComponent)
            .ThenByDescending(x => x.EffectiveFrom)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => Map(x))
            .ToListAsync(cancellationToken);

        return new PagedResult<DiscountPolicyResponse>
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)request.PageSize),
            Items = items
        };
    }

    public async Task<DiscountPolicyResponse> CreateAsync(
        CreateDiscountPolicyRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var normalized = await ValidateAsync(request, null, cancellationToken);
        var approval = ResolveApproval(normalized.DiscountType);
        var entity = new MstDiscountPolicy
        {
            Code = normalized.Code,
            Name = normalized.Name,
            DiscountType = normalized.DiscountType,
            TargetComponent = normalized.TargetComponent,
            ValueType = normalized.ValueType,
            Value = request.Value,
            Limit = request.Limit,
            RequiresApproval = approval.RequiresApproval,
            ApproverRole = approval.ApproverRole,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            IsActive = request.IsActive,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };

        _dbContext.MstDiscountPolicies.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("DiscountPolicy.Create", entity, actorUserId, null);
        return Map(entity);
    }

    public async Task<DiscountPolicyResponse> UpdateAsync(
        Guid id,
        UpdateDiscountPolicyRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (entity.EffectiveFrom <= DateTimeOffset.UtcNow)
            throw new DiscountPolicyValidationException("Policy diskon yang sudah efektif tidak dapat diubah; buat versi baru.");

        var normalized = await ValidateAsync(request, id, cancellationToken);
        var approval = ResolveApproval(normalized.DiscountType);
        entity.Code = normalized.Code;
        entity.Name = normalized.Name;
        entity.DiscountType = normalized.DiscountType;
        entity.TargetComponent = normalized.TargetComponent;
        entity.ValueType = normalized.ValueType;
        entity.Value = request.Value;
        entity.Limit = request.Limit;
        entity.RequiresApproval = approval.RequiresApproval;
        entity.ApproverRole = approval.ApproverRole;
        entity.EffectiveFrom = request.EffectiveFrom;
        entity.EffectiveTo = request.EffectiveTo;
        entity.IsActive = request.IsActive;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("DiscountPolicy.Update", entity, actorUserId, null);
        return Map(entity);
    }

    public async Task<DiscountPolicyResponse> DeactivateAsync(
        Guid id,
        DeactivatePolicyRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new DiscountPolicyValidationException("Alasan penonaktifan wajib diisi.");

        var entity = await FindAsync(id, cancellationToken);
        entity.IsActive = false;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("DiscountPolicy.Deactivate", entity, actorUserId, request.Reason.Trim());
        return Map(entity);
    }

    // Mengaktifkan kembali policy yang sebelumnya dinonaktifkan tanpa perlu membuat versi baru.
    // Tidak mensyaratkan alasan (berbeda dari DeactivateAsync).
    public async Task<DiscountPolicyResponse> ActivateAsync(
        Guid id,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (entity.IsActive)
            return Map(entity);

        var overlaps = await _dbContext.MstDiscountPolicies.AnyAsync(
            x => !x.IsDelete && x.IsActive && x.Id != entity.Id
                && x.DiscountType == entity.DiscountType && x.TargetComponent == entity.TargetComponent
                && x.EffectiveFrom < (entity.EffectiveTo ?? DateTimeOffset.MaxValue)
                && (x.EffectiveTo == null || entity.EffectiveFrom < x.EffectiveTo),
            cancellationToken);
        if (overlaps)
            throw new DiscountPolicyConflictException(
                "Tidak dapat mengaktifkan; ada policy lain yang masih aktif dan bertumpang tindih untuk jenis dan target yang sama.");

        entity.IsActive = true;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("DiscountPolicy.Activate", entity, actorUserId, null);
        return Map(entity);
    }

    // Soft delete - baris tidak pernah dihapus fisik agar riwayat BilDiscountApplication yang
    // sudah menyimpan snapshot policy tetap dapat ditelusuri. Hanya policy nonaktif yang boleh
    // dihapus - mencegah penghapusan diam-diam atas policy yang masih hidup.
    public async Task<DiscountPolicyDeleteResponse> DeleteAsync(
        Guid id,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (entity.IsActive)
            throw new DiscountPolicyValidationException(
                "Policy yang masih aktif tidak dapat dihapus; nonaktifkan terlebih dahulu.");

        entity.IsDelete = true;
        entity.DeleteDateTime = DateTime.UtcNow;
        entity.DeleteBy = actorUserId;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("DiscountPolicy.Delete", entity, actorUserId, null);

        return new DiscountPolicyDeleteResponse
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            IsDelete = entity.IsDelete
        };
    }

    private async Task<MstDiscountPolicy> FindAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.MstDiscountPolicies.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
        ?? throw new KeyNotFoundException("Policy diskon tidak ditemukan.");

    private async Task<(string Code, string Name, string DiscountType, string TargetComponent, string ValueType)> ValidateAsync(
        CreateDiscountPolicyRequest request,
        Guid? excludedId,
        CancellationToken cancellationToken)
    {
        var code = Required(request.Code, "Code").ToUpperInvariant();
        var name = Required(request.Name, "Name");
        var type = Normalize(request.DiscountType, DiscountPolicyValues.DiscountTypes, "DiscountType");
        var target = Normalize(request.TargetComponent, DiscountPolicyValues.TargetComponents, "TargetComponent");
        var valueType = Normalize(request.ValueType, DiscountPolicyValues.ValueTypes, "ValueType");

        var expectedTarget = type switch
        {
            DiscountPolicyValues.PromoTotal => DiscountPolicyValues.PatientPortion,
            DiscountPolicyValues.PromoItem => DiscountPolicyValues.InvoiceItem,
            DiscountPolicyValues.Doctor => DiscountPolicyValues.DoctorShare,
            _ => throw new DiscountPolicyValidationException("DiscountType tidak didukung.")
        };
        if (target != expectedTarget)
            throw new DiscountPolicyValidationException($"{type} hanya boleh menargetkan {expectedTarget}.");
        if (request.Value <= 0 || (valueType == DiscountPolicyValues.Percentage && request.Value > 100))
            throw new DiscountPolicyValidationException("Nilai diskon harus lebih dari nol dan persentase tidak boleh melebihi 100.");
        if (request.Limit <= 0)
            throw new DiscountPolicyValidationException("Limit diskon harus lebih dari nol bila diisi.");
        if (request.EffectiveTo.HasValue && request.EffectiveTo <= request.EffectiveFrom)
            throw new DiscountPolicyValidationException("EffectiveTo harus lebih besar dari EffectiveFrom.");

        if (await _dbContext.MstDiscountPolicies.AnyAsync(
            x => !x.IsDelete && x.Id != excludedId && x.Code == code,
            cancellationToken))
            throw new DiscountPolicyConflictException("Kode policy diskon sudah digunakan.");

        if (await _dbContext.MstDiscountPolicies.AnyAsync(
            x => !x.IsDelete && x.Id != excludedId
                && x.DiscountType == type && x.TargetComponent == target
                && x.EffectiveFrom < (request.EffectiveTo ?? DateTimeOffset.MaxValue)
                && (x.EffectiveTo == null || request.EffectiveFrom < x.EffectiveTo),
            cancellationToken))
            throw new DiscountPolicyConflictException("Periode policy diskon bertumpang tindih untuk jenis dan target yang sama.");

        return (code, name, type, target, valueType);
    }

    private Task AuditAsync(string action, MstDiscountPolicy entity, Guid actorUserId, string? reason) =>
        _loggerService.AuditAsync(LogCategory, action, "Perubahan policy diskon.", new
        {
            PolicyId = entity.Id,
            entity.Code,
            entity.DiscountType,
            entity.TargetComponent,
            entity.ValueType,
            entity.Value,
            entity.Limit,
            entity.RequiresApproval,
            entity.ApproverRole,
            entity.EffectiveFrom,
            entity.EffectiveTo,
            entity.IsActive,
            ActorUserId = actorUserId,
            Reason = reason
        });

    private static (bool RequiresApproval, string? ApproverRole) ResolveApproval(string discountType) =>
        discountType == DiscountPolicyValues.Doctor
            ? (true, DiscountPolicyValues.DoctorApprover)
            : (false, null);

    private static string Required(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DiscountPolicyValidationException($"{field} wajib diisi.");
        return value.Trim();
    }

    private static string Normalize(string? value, IReadOnlySet<string> allowed, string field)
    {
        var normalized = Required(value, field).ToUpperInvariant();
        if (!allowed.Contains(normalized))
            throw new DiscountPolicyValidationException($"{field} tidak didukung.");
        return normalized;
    }

    private static DiscountPolicyResponse Map(MstDiscountPolicy entity) => new()
    {
        Id = entity.Id,
        Code = entity.Code,
        Name = entity.Name,
        DiscountType = entity.DiscountType,
        TargetComponent = entity.TargetComponent,
        ValueType = entity.ValueType,
        Value = entity.Value,
        Limit = entity.Limit,
        RequiresApproval = entity.RequiresApproval,
        ApproverRole = entity.ApproverRole,
        EffectiveFrom = entity.EffectiveFrom,
        EffectiveTo = entity.EffectiveTo,
        IsActive = entity.IsActive,
        CreateDateTime = entity.CreateDateTime,
        UpdateDateTime = entity.UpdateDateTime
    };
}

public sealed class DiscountPolicyValidationException(string message) : Exception(message);
public sealed class DiscountPolicyConflictException(string message) : Exception(message);
