using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Services;

/// <summary>
/// CRUD master jenis dokumen legalitas vendor (BE-PBJ-VD-002; PBJ-ASM-001). Kode disimpan huruf besar dan unik di
/// antara data yang belum dihapus.
/// </summary>
/// <remarks>
/// <para>
/// Jenis yang sudah dipakai dokumen vendor atau kewajiban kategori tidak dapat dihapus (400 <c>PBJ-VAL-071</c>);
/// cukup dinonaktifkan supaya tidak bisa dipilih untuk dokumen baru.
/// </para>
/// <para>
/// Jenis yang masih diwajibkan kewajiban aktif tidak dapat dinonaktifkan (409). Contoh: Izin PBF diwajibkan untuk
/// kategori Obat; bila jenisnya nonaktif, vendor Obat tidak bisa mencatat izin baru dan semuanya menjadi tidak layak.
/// Nonaktifkan kewajibannya lebih dulu.
/// </para>
/// </remarks>
public sealed class ProcurementVendorDocumentTypeService
{
    private const string LogCategory = "Corporate.ProcurementManagement.MasterData";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public ProcurementVendorDocumentTypeService(ApplicationDbContext dbContext, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    public Task<ProcurementVendorDocumentTypeFilterMetadataResponse> GetFilterMetadataAsync(CancellationToken cancellationToken) =>
        Task.FromResult(new ProcurementVendorDocumentTypeFilterMetadataResponse
        {
            SortOptions =
            [
                new() { Value = "code", Label = "Kode" },
                new() { Value = "name", Label = "Nama" },
                new() { Value = "isActive", Label = "Status aktif" }
            ]
        });

    public async Task<ProcurementVendorDocumentTypeSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var query = _dbContext.MstProcurementVendorDocumentTypes.AsNoTracking().Where(x => !x.IsDelete);
        return new ProcurementVendorDocumentTypeSummaryResponse
        {
            Total = await query.CountAsync(cancellationToken),
            Active = await query.CountAsync(x => x.IsActive, cancellationToken),
            Inactive = await query.CountAsync(x => !x.IsActive, cancellationToken),
            RequiresExpiryDate = await query.CountAsync(x => x.RequiresExpiryDate, cancellationToken)
        };
    }

    public async Task<PagedResult<ProcurementVendorDocumentTypeResponse>> GetPagedAsync(
        ProcurementVendorDocumentTypeQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.MstProcurementVendorDocumentTypes.AsNoTracking().Where(x => !x.IsDelete);
        query = query.WhereCreatedWithin(ProcurementPeriodFilter.Resolve(request.StartDate, request.EndDate, request.CustomPeriod));
        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
        if (request.RequiresExpiryDate.HasValue) query = query.Where(x => x.RequiresExpiryDate == request.RequiresExpiryDate.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpper();
            query = query.Where(x => x.DocumentTypeCode.ToUpper().Contains(search) || x.DocumentTypeName.ToUpper().Contains(search));
        }

        var descending = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "code" => descending ? query.OrderByDescending(x => x.DocumentTypeCode) : query.OrderBy(x => x.DocumentTypeCode),
            "isactive" => descending
                ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.DocumentTypeName)
                : query.OrderBy(x => x.IsActive).ThenBy(x => x.DocumentTypeName),
            _ => descending ? query.OrderByDescending(x => x.DocumentTypeName) : query.OrderBy(x => x.DocumentTypeName)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await Project(query.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProcurementVendorDocumentTypeResponse>
        {
            PageNumber = request.PageNumber, PageSize = request.PageSize, TotalData = total,
            TotalPage = ProcurementMasterDataSupport.TotalPage(total, request.PageSize), Items = items
        };
    }

    public async Task<List<ProcurementVendorDocumentTypeOptionResponse>> GetOptionsAsync(
        bool onlyActive, string? search, CancellationToken cancellationToken)
    {
        var query = _dbContext.MstProcurementVendorDocumentTypes.AsNoTracking().Where(x => !x.IsDelete);
        if (onlyActive) query = query.Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim().ToUpper();
            query = query.Where(x => x.DocumentTypeCode.ToUpper().Contains(keyword) || x.DocumentTypeName.ToUpper().Contains(keyword));
        }

        return await query
            .OrderBy(x => x.DocumentTypeName)
            .Select(x => new ProcurementVendorDocumentTypeOptionResponse
            {
                Id = x.Id, Code = x.DocumentTypeCode, Name = x.DocumentTypeName,
                RequiresExpiryDate = x.RequiresExpiryDate, IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ProcurementVendorDocumentTypeResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await Project(_dbContext.MstProcurementVendorDocumentTypes.AsNoTracking().Where(x => x.Id == id && !x.IsDelete))
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw new KeyNotFoundException("Jenis dokumen tidak ditemukan atau sudah dihapus.");

    public async Task<ProcurementVendorDocumentTypeResponse> CreateAsync(
        UpsertProcurementVendorDocumentTypeRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var (code, name, description) = await ValidateAsync(request, null, cancellationToken);
        var entity = new MstProcurementVendorDocumentType
        {
            DocumentTypeCode = code, DocumentTypeName = name, RequiresExpiryDate = request.RequiresExpiryDate,
            Description = description, IsActive = request.IsActive, CreateDateTime = DateTime.UtcNow, CreateBy = actorUserId
        };
        _dbContext.MstProcurementVendorDocumentTypes.Add(entity);
        await SaveAsync(cancellationToken);
        await AuditAsync("ProcurementVendorDocumentType.Create", entity, actorUserId);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<ProcurementVendorDocumentTypeResponse> UpdateAsync(
        Guid id, UpsertProcurementVendorDocumentTypeRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        var (code, name, description) = await ValidateAsync(request, id, cancellationToken);
        if (entity.IsActive && !request.IsActive) await EnsureNotRequiredAsync(entity, cancellationToken);
        entity.DocumentTypeCode = code;
        entity.DocumentTypeName = name;
        entity.RequiresExpiryDate = request.RequiresExpiryDate;
        entity.Description = description;
        entity.IsActive = request.IsActive;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;
        await SaveAsync(cancellationToken);
        await AuditAsync("ProcurementVendorDocumentType.Update", entity, actorUserId);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<ProcurementVendorDocumentTypeResponse> UpdateStatusAsync(
        Guid id, UpdateProcurementMasterStatusRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (entity.IsActive && !request.IsActive) await EnsureNotRequiredAsync(entity, cancellationToken);
        entity.IsActive = request.IsActive;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;
        await SaveAsync(cancellationToken);
        await AuditAsync(request.IsActive ? "ProcurementVendorDocumentType.Activate" : "ProcurementVendorDocumentType.Deactivate", entity, actorUserId);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<ProcurementMasterDeleteResponse> DeleteAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (await _dbContext.PrcVendorDocuments.AnyAsync(x => x.DocumentTypeId == id && !x.IsDelete, cancellationToken))
            throw new ProcurementMasterInUseException(
                "Jenis dokumen ini tidak dapat dihapus karena sudah dipakai dokumen vendor. Nonaktifkan saja bila tidak dipakai lagi.");
        if (await _dbContext.MstProcurementCategoryDocumentRequirements.AnyAsync(x => x.DocumentTypeId == id && !x.IsDelete, cancellationToken))
            throw new ProcurementMasterInUseException(
                "Jenis dokumen ini tidak dapat dihapus karena masih tercatat sebagai dokumen wajib kategori. Hapus atau nonaktifkan kewajibannya, atau nonaktifkan jenis ini saja.");

        entity.IsDelete = true;
        entity.IsActive = false;
        entity.DeleteDateTime = DateTime.UtcNow;
        entity.DeleteBy = actorUserId;
        await SaveAsync(cancellationToken);
        await AuditAsync("ProcurementVendorDocumentType.Delete", entity, actorUserId);
        return new ProcurementMasterDeleteResponse { Id = entity.Id, Code = entity.DocumentTypeCode, Name = entity.DocumentTypeName, IsDelete = true };
    }

    private async Task EnsureNotRequiredAsync(MstProcurementVendorDocumentType entity, CancellationToken cancellationToken)
    {
        var categories = await _dbContext.MstProcurementCategoryDocumentRequirements.AsNoTracking()
            .Where(x => x.DocumentTypeId == entity.Id && x.IsActive && !x.IsDelete)
            .Select(x => x.ItemCategory!.CategoryName)
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);
        if (categories.Count > 0)
            throw new ProcurementMasterConflictException(
                $"Jenis dokumen {entity.DocumentTypeName} masih diwajibkan untuk kategori {string.Join(", ", categories)}. Nonaktifkan kewajibannya lebih dulu.");
    }

    private async Task<(string Code, string Name, string? Description)> ValidateAsync(
        UpsertProcurementVendorDocumentTypeRequest request, Guid? excludedId, CancellationToken cancellationToken)
    {
        var code = ProcurementMasterDataSupport.Required(request.Code, "Kode", 30).ToUpperInvariant();
        var name = ProcurementMasterDataSupport.Required(request.Name, "Nama", 150);
        var description = ProcurementMasterDataSupport.Optional(request.Description, "Deskripsi", 500);
        if (await _dbContext.MstProcurementVendorDocumentTypes.AnyAsync(
                x => !x.IsDelete && x.Id != excludedId && x.DocumentTypeCode == code, cancellationToken))
            throw new ProcurementMasterConflictException("Kode sudah dipakai jenis dokumen lain. Gunakan kode yang berbeda.");
        return (code, name, description);
    }

    private IQueryable<ProcurementVendorDocumentTypeResponse> Project(IQueryable<MstProcurementVendorDocumentType> query) =>
        query.Select(x => new ProcurementVendorDocumentTypeResponse
        {
            Id = x.Id, Code = x.DocumentTypeCode, Name = x.DocumentTypeName, RequiresExpiryDate = x.RequiresExpiryDate,
            Description = x.Description, IsActive = x.IsActive,
            ActiveRequirementCount = _dbContext.MstProcurementCategoryDocumentRequirements
                .Count(r => r.DocumentTypeId == x.Id && r.IsActive && !r.IsDelete),
            CreateDateTime = x.CreateDateTime, CreateBy = x.CreateBy, UpdateDateTime = x.UpdateDateTime, UpdateBy = x.UpdateBy
        });

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try { await _dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException exception) when (ProcurementMasterDataSupport.IsUniqueViolation(exception))
        {
            throw new ProcurementMasterConflictException("Kode sudah dipakai jenis dokumen lain. Gunakan kode yang berbeda.");
        }
    }

    private async Task<MstProcurementVendorDocumentType> FindAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.MstProcurementVendorDocumentTypes.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
        ?? throw new KeyNotFoundException("Jenis dokumen tidak ditemukan atau sudah dihapus.");

    private Task AuditAsync(string action, MstProcurementVendorDocumentType entity, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, action, "Perubahan master jenis dokumen legalitas vendor.", new
        {
            EntityId = entity.Id,
            entity.IsActive,
            entity.RequiresExpiryDate,
            ActorUserId = actorUserId
        });
}
