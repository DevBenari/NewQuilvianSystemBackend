using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Services;

// BE-BKC-035 / PC-DES-002 / PC-DEC-012: CRUD baseline master data kategori pengeluaran kas kecil,
// mengikuti TaxRuleService apa adanya (CRUD baris tunggal, tanpa transaction eksplisit).
public sealed class PettyCashCategoryService
{
    private const string LogCategory = "HealthServices.BillingManagement.MasterData";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public PettyCashCategoryService(ApplicationDbContext dbContext, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    public async Task<PagedResult<PettyCashCategoryResponse>> GetPagedAsync(PettyCashCategoryQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.MstPettyCashCategories.AsNoTracking().Where(x => !x.IsDelete);
        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpper();
            query = query.Where(x => x.CategoryCode.ToUpper().Contains(search) || x.CategoryName.ToUpper().Contains(search));
        }

        var descending = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = request.SortBy.Trim().ToLowerInvariant() switch
        {
            "categorycode" => descending ? query.OrderByDescending(x => x.CategoryCode) : query.OrderBy(x => x.CategoryCode),
            "isactive" => descending ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
            _ => descending ? query.OrderByDescending(x => x.CategoryName) : query.OrderBy(x => x.CategoryName)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .Select(x => Map(x)).ToListAsync(cancellationToken);
        return new PagedResult<PettyCashCategoryResponse>
        {
            PageNumber = request.PageNumber, PageSize = request.PageSize, TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)request.PageSize), Items = items
        };
    }

    public async Task<PettyCashCategoryResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        return Map(entity);
    }

    public async Task<List<PettyCashCategoryOptionResponse>> GetOptionsAsync(bool onlyActive, string? search, CancellationToken cancellationToken)
    {
        var query = _dbContext.MstPettyCashCategories.AsNoTracking().Where(x => !x.IsDelete);
        if (onlyActive) query = query.Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim().ToUpper();
            query = query.Where(x => x.CategoryCode.ToUpper().Contains(keyword) || x.CategoryName.ToUpper().Contains(keyword));
        }

        return await query
            .OrderBy(x => x.CategoryName)
            .Select(x => new PettyCashCategoryOptionResponse
            {
                Id = x.Id,
                CategoryCode = x.CategoryCode,
                CategoryName = x.CategoryName,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PettyCashCategorySummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var query = _dbContext.MstPettyCashCategories.AsNoTracking().Where(x => !x.IsDelete);
        return new PettyCashCategorySummaryResponse
        {
            TotalCategory = await query.CountAsync(cancellationToken),
            ActiveCategory = await query.CountAsync(x => x.IsActive, cancellationToken),
            InactiveCategory = await query.CountAsync(x => !x.IsActive, cancellationToken)
        };
    }

    public Task<PettyCashCategoryFilterMetadataResponse> GetFilterMetadataAsync(CancellationToken cancellationToken) =>
        Task.FromResult(new PettyCashCategoryFilterMetadataResponse
        {
            DefaultFilter = new PettyCashCategoryDefaultFilterResponse(),
            PageSizeOptions = [10, 25, 50, 100],
            SortableFields = ["categoryCode", "categoryName", "isActive"]
        });

    public async Task<PettyCashCategoryResponse> CreateAsync(CreatePettyCashCategoryRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var values = await ValidateAsync(request, null, cancellationToken);
        var entity = new MstPettyCashCategory
        {
            CategoryCode = values.Code,
            CategoryName = values.Name,
            Description = values.Description,
            IsActive = request.IsActive,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };
        _dbContext.MstPettyCashCategories.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("PettyCashCategory.Create", entity, actorUserId);
        return Map(entity);
    }

    public async Task<PettyCashCategoryResponse> UpdateAsync(Guid id, UpdatePettyCashCategoryRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        var values = await ValidateAsync(request, id, cancellationToken);
        entity.CategoryCode = values.Code;
        entity.CategoryName = values.Name;
        entity.Description = values.Description;
        entity.IsActive = request.IsActive;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("PettyCashCategory.Update", entity, actorUserId);
        return Map(entity);
    }

    public async Task<PettyCashCategoryResponse> UpdateStatusAsync(Guid id, UpdatePettyCashCategoryStatusRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        entity.IsActive = request.IsActive;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync(request.IsActive ? "PettyCashCategory.Activate" : "PettyCashCategory.Deactivate", entity, actorUserId);
        return Map(entity);
    }

    // Soft delete - baris tidak pernah dihapus fisik. Ditolak selama masih ada voucher (aktif
    // maupun histori) yang menunjuk kategori ini (BIL-VAL-053); nonaktifkan saja bila sudah tidak
    // dipakai lagi tetapi riwayatnya tetap harus ditelusuri.
    public async Task<PettyCashCategoryDeleteResponse> DeleteAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        var isUsed = await _dbContext.BilPettyCashVouchers.AnyAsync(x => x.CategoryId == id, cancellationToken);
        if (isUsed)
            throw new PettyCashCategoryInUseException(
                "Kategori ini tidak dapat dihapus karena sudah dipakai voucher. Nonaktifkan saja bila tidak dipakai lagi.");

        entity.IsDelete = true;
        entity.DeleteDateTime = DateTime.UtcNow;
        entity.DeleteBy = actorUserId;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("PettyCashCategory.Delete", entity, actorUserId);

        return new PettyCashCategoryDeleteResponse
        {
            Id = entity.Id,
            CategoryCode = entity.CategoryCode,
            CategoryName = entity.CategoryName,
            IsDelete = entity.IsDelete
        };
    }

    private async Task<(string Code, string Name, string? Description)> ValidateAsync(
        CreatePettyCashCategoryRequest request, Guid? excludedId, CancellationToken cancellationToken)
    {
        var code = Required(request.CategoryCode, "CategoryCode").ToUpperInvariant();
        var name = Required(request.CategoryName, "CategoryName");
        var description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        if (await _dbContext.MstPettyCashCategories.AnyAsync(x => !x.IsDelete && x.Id != excludedId && x.CategoryCode == code, cancellationToken))
            throw new PettyCashCategoryConflictException("Kode kategori sudah dipakai kategori lain. Gunakan kode yang berbeda.");
        return (code, name, description);
    }

    private async Task<MstPettyCashCategory> FindAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.MstPettyCashCategories.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
        ?? throw new KeyNotFoundException("Kategori petty cash tidak ditemukan.");

    private Task AuditAsync(string action, MstPettyCashCategory entity, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, action, "Perubahan kategori petty cash.", new
        {
            CategoryId = entity.Id,
            entity.CategoryCode,
            entity.CategoryName,
            entity.IsActive,
            ActorUserId = actorUserId
        });

    private static string Required(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new PettyCashCategoryValidationException($"{field} wajib diisi.");
        return value.Trim();
    }

    private static PettyCashCategoryResponse Map(MstPettyCashCategory entity) => new()
    {
        Id = entity.Id,
        CategoryCode = entity.CategoryCode,
        CategoryName = entity.CategoryName,
        Description = entity.Description,
        IsActive = entity.IsActive,
        CreateDateTime = entity.CreateDateTime,
        UpdateDateTime = entity.UpdateDateTime
    };
}

public sealed class PettyCashCategoryValidationException(string message) : Exception(message);
public sealed class PettyCashCategoryConflictException(string message) : Exception(message);
public sealed class PettyCashCategoryInUseException(string message) : Exception(message);
