using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Services;

/// <summary>
/// CRUD master kategori Pengadaan (BE-PBJ-003, PBJ-DEC-037). Kode diisi pengguna, disimpan huruf besar,
/// dan unik di antara data yang belum dihapus (index unik terfilter + pemeriksaan service).
/// </summary>
public sealed class ProcurementItemCategoryService
{
    private const string LogCategory = "Corporate.ProcurementManagement.MasterData";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public ProcurementItemCategoryService(ApplicationDbContext dbContext, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    public Task<ProcurementReferenceFilterMetadataResponse> GetFilterMetadataAsync(CancellationToken cancellationToken) =>
        Task.FromResult(new ProcurementReferenceFilterMetadataResponse
        {
            DefaultFilter = new ProcurementReferenceDefaultFilterResponse(),
            SortOptions =
            [
                new() { Value = "code", Label = "Kode" },
                new() { Value = "name", Label = "Nama" },
                new() { Value = "isActive", Label = "Status aktif" }
            ]
        });

    public async Task<ProcurementReferenceSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var query = _dbContext.MstProcurementItemCategories.AsNoTracking().Where(x => !x.IsDelete);
        return new ProcurementReferenceSummaryResponse
        {
            Total = await query.CountAsync(cancellationToken),
            Active = await query.CountAsync(x => x.IsActive, cancellationToken),
            Inactive = await query.CountAsync(x => !x.IsActive, cancellationToken)
        };
    }

    public async Task<PagedResult<ProcurementReferenceResponse>> GetPagedAsync(ProcurementReferenceQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.MstProcurementItemCategories.AsNoTracking().Where(x => !x.IsDelete);
        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpper();
            query = query.Where(x => x.CategoryCode.ToUpper().Contains(search) || x.CategoryName.ToUpper().Contains(search));
        }

        var descending = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "code" => descending ? query.OrderByDescending(x => x.CategoryCode) : query.OrderBy(x => x.CategoryCode),
            "isactive" => descending
                ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.CategoryName)
                : query.OrderBy(x => x.IsActive).ThenBy(x => x.CategoryName),
            _ => descending ? query.OrderByDescending(x => x.CategoryName) : query.OrderBy(x => x.CategoryName)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .Select(x => new ProcurementReferenceResponse
            {
                Id = x.Id, Code = x.CategoryCode, Name = x.CategoryName, Description = x.Description, IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime, CreateBy = x.CreateBy, UpdateDateTime = x.UpdateDateTime, UpdateBy = x.UpdateBy
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ProcurementReferenceResponse>
        {
            PageNumber = request.PageNumber, PageSize = request.PageSize, TotalData = total,
            TotalPage = ProcurementMasterDataSupport.TotalPage(total, request.PageSize), Items = items
        };
    }

    public async Task<List<ProcurementReferenceOptionResponse>> GetOptionsAsync(bool onlyActive, string? search, CancellationToken cancellationToken)
    {
        var query = _dbContext.MstProcurementItemCategories.AsNoTracking().Where(x => !x.IsDelete);
        if (onlyActive) query = query.Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim().ToUpper();
            query = query.Where(x => x.CategoryCode.ToUpper().Contains(keyword) || x.CategoryName.ToUpper().Contains(keyword));
        }

        return await query
            .OrderBy(x => x.CategoryName)
            .Select(x => new ProcurementReferenceOptionResponse { Id = x.Id, Code = x.CategoryCode, Name = x.CategoryName, IsActive = x.IsActive })
            .ToListAsync(cancellationToken);
    }

    public async Task<ProcurementReferenceResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Map(await FindAsync(id, cancellationToken));

    public async Task<ProcurementReferenceResponse> CreateAsync(UpsertProcurementReferenceRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var (code, name, description) = await ValidateAsync(request, null, cancellationToken);
        var entity = new MstProcurementItemCategory
        {
            CategoryCode = code, CategoryName = name, Description = description, IsActive = request.IsActive,
            CreateDateTime = DateTime.UtcNow, CreateBy = actorUserId
        };
        _dbContext.MstProcurementItemCategories.Add(entity);
        await SaveAsync(cancellationToken);
        await AuditAsync("ProcurementItemCategory.Create", entity, actorUserId);
        return Map(entity);
    }

    public async Task<ProcurementReferenceResponse> UpdateAsync(Guid id, UpsertProcurementReferenceRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        var (code, name, description) = await ValidateAsync(request, id, cancellationToken);
        entity.CategoryCode = code;
        entity.CategoryName = name;
        entity.Description = description;
        entity.IsActive = request.IsActive;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;
        await SaveAsync(cancellationToken);
        await AuditAsync("ProcurementItemCategory.Update", entity, actorUserId);
        return Map(entity);
    }

    public async Task<ProcurementReferenceResponse> UpdateStatusAsync(Guid id, UpdateProcurementMasterStatusRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        entity.IsActive = request.IsActive;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;
        await SaveAsync(cancellationToken);
        await AuditAsync(request.IsActive ? "ProcurementItemCategory.Activate" : "ProcurementItemCategory.Deactivate", entity, actorUserId);
        return Map(entity);
    }

    public async Task<ProcurementMasterDeleteResponse> DeleteAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        // Dipakai katalog. Pagu (BE-PBJ-010) menambah pemeriksaannya di sini saat tabelnya ada.
        var usedByItem = await _dbContext.MstProcurementItems
            .AnyAsync(x => x.ItemCategoryId == id && !x.IsDelete, cancellationToken);
        if (usedByItem)
            throw new ProcurementMasterInUseException(
                "Kategori ini tidak dapat dihapus karena masih dipakai katalog barang. Nonaktifkan saja bila tidak dipakai lagi.");

        entity.IsDelete = true;
        entity.IsActive = false;
        entity.DeleteDateTime = DateTime.UtcNow;
        entity.DeleteBy = actorUserId;
        await SaveAsync(cancellationToken);
        await AuditAsync("ProcurementItemCategory.Delete", entity, actorUserId);
        return new ProcurementMasterDeleteResponse { Id = entity.Id, Code = entity.CategoryCode, Name = entity.CategoryName, IsDelete = true };
    }

    private async Task<(string Code, string Name, string? Description)> ValidateAsync(
        UpsertProcurementReferenceRequest request, Guid? excludedId, CancellationToken cancellationToken)
    {
        var code = ProcurementMasterDataSupport.Required(request.Code, "Kode", 30).ToUpperInvariant();
        var name = ProcurementMasterDataSupport.Required(request.Name, "Nama", 150);
        var description = ProcurementMasterDataSupport.Optional(request.Description, "Deskripsi", 500);
        if (await _dbContext.MstProcurementItemCategories.AnyAsync(x => !x.IsDelete && x.Id != excludedId && x.CategoryCode == code, cancellationToken))
            throw new ProcurementMasterConflictException("Kode sudah dipakai data lain. Gunakan kode yang berbeda.");
        return (code, name, description);
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try { await _dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException exception) when (ProcurementMasterDataSupport.IsUniqueViolation(exception))
        {
            throw new ProcurementMasterConflictException("Kode sudah dipakai data lain. Gunakan kode yang berbeda.");
        }
    }

    private async Task<MstProcurementItemCategory> FindAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.MstProcurementItemCategories.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
        ?? throw new KeyNotFoundException("Kategori tidak ditemukan atau sudah dihapus.");

    private Task AuditAsync(string action, MstProcurementItemCategory entity, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, action, "Perubahan master kategori Pengadaan.", new
        {
            EntityId = entity.Id,
            entity.IsActive,
            ActorUserId = actorUserId
        });

    private static ProcurementReferenceResponse Map(MstProcurementItemCategory x) => new()
    {
        Id = x.Id, Code = x.CategoryCode, Name = x.CategoryName, Description = x.Description, IsActive = x.IsActive,
        CreateDateTime = x.CreateDateTime, CreateBy = x.CreateBy, UpdateDateTime = x.UpdateDateTime, UpdateBy = x.UpdateBy
    };
}
