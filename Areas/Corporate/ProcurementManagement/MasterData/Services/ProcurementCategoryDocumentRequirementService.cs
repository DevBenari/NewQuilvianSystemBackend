using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Services;

/// <summary>
/// CRUD master dokumen wajib per kategori (BE-PBJ-VD-002; PBJ-DEC-045, PBJ-DEC-061). Satu pasangan kategori × jenis
/// dokumen di antara data yang belum dihapus.
/// </summary>
/// <remarks>
/// <para>
/// <b>Dimulai kosong, diaktifkan bertahap.</b> Kewajiban baru lahir nonaktif kecuali diminta aktif. Aktivasi pertama
/// mengisi <c>ActivatedAt</c> dan <c>ActivatedByUserId</c>; aktivasi berikutnya tidak menimpanya. Contoh: Izin PBF
/// untuk kategori Obat diaktifkan 01-03-2027; sejak saat itu vendor Obat tanpa izin terverifikasi tidak layak dipilih.
/// </para>
/// <para>
/// <b>Syarat aktif.</b> Kategori dan jenis dokumennya harus aktif, supaya vendor masih bisa mencatat dokumen yang
/// diwajibkan. Kewajiban yang pernah berlaku tidak dapat dihapus (400 <c>PBJ-VAL-071</c>) dan pasangan kategori × jenisnya
/// tidak dapat dipindah (409); cukup dinonaktifkan, supaya jejak "berlaku sejak" tetap benar.
/// </para>
/// </remarks>
public sealed class ProcurementCategoryDocumentRequirementService
{
    private const string LogCategory = "Corporate.ProcurementManagement.MasterData";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public ProcurementCategoryDocumentRequirementService(ApplicationDbContext dbContext, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    public async Task<ProcurementCategoryDocumentRequirementFilterMetadataResponse> GetFilterMetadataAsync(CancellationToken cancellationToken) =>
        new()
        {
            SortOptions =
            [
                new() { Value = "category", Label = "Kategori" },
                new() { Value = "documentType", Label = "Jenis dokumen" },
                new() { Value = "activatedAt", Label = "Berlaku sejak" },
                new() { Value = "isActive", Label = "Status aktif" }
            ],
            ItemCategoryOptions = await _dbContext.MstProcurementItemCategories.AsNoTracking()
                .Where(x => !x.IsDelete)
                .OrderBy(x => x.CategoryName)
                .Select(x => new ProcurementVendorCategoryOptionResponse
                {
                    Id = x.Id, Code = x.CategoryCode, Name = x.CategoryName, IsActive = x.IsActive
                })
                .ToListAsync(cancellationToken),
            DocumentTypeOptions = await _dbContext.MstProcurementVendorDocumentTypes.AsNoTracking()
                .Where(x => !x.IsDelete)
                .OrderBy(x => x.DocumentTypeName)
                .Select(x => new ProcurementVendorDocumentTypeOptionResponse
                {
                    Id = x.Id, Code = x.DocumentTypeCode, Name = x.DocumentTypeName,
                    RequiresExpiryDate = x.RequiresExpiryDate, IsActive = x.IsActive
                })
                .ToListAsync(cancellationToken)
        };

    public async Task<ProcurementCategoryDocumentRequirementSummaryResponse> GetSummaryAsync(Guid? itemCategoryId, CancellationToken cancellationToken)
    {
        var query = _dbContext.MstProcurementCategoryDocumentRequirements.AsNoTracking().Where(x => !x.IsDelete);
        if (itemCategoryId.HasValue) query = query.Where(x => x.ItemCategoryId == itemCategoryId.Value);
        return new ProcurementCategoryDocumentRequirementSummaryResponse
        {
            Total = await query.CountAsync(cancellationToken),
            Active = await query.CountAsync(x => x.IsActive, cancellationToken),
            Inactive = await query.CountAsync(x => !x.IsActive, cancellationToken),
            CategoriesWithActiveRequirement = await query.Where(x => x.IsActive)
                .Select(x => x.ItemCategoryId).Distinct().CountAsync(cancellationToken)
        };
    }

    public async Task<PagedResult<ProcurementCategoryDocumentRequirementResponse>> GetPagedAsync(
        ProcurementCategoryDocumentRequirementQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.MstProcurementCategoryDocumentRequirements.AsNoTracking().Where(x => !x.IsDelete);
        query = query.WhereCreatedWithin(ProcurementPeriodFilter.Resolve(request.StartDate, request.EndDate, request.CustomPeriod));
        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
        if (request.ItemCategoryId.HasValue) query = query.Where(x => x.ItemCategoryId == request.ItemCategoryId.Value);
        if (request.DocumentTypeId.HasValue) query = query.Where(x => x.DocumentTypeId == request.DocumentTypeId.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpper();
            query = query.Where(x =>
                x.ItemCategory!.CategoryCode.ToUpper().Contains(search) || x.ItemCategory.CategoryName.ToUpper().Contains(search)
                || x.DocumentType!.DocumentTypeCode.ToUpper().Contains(search) || x.DocumentType.DocumentTypeName.ToUpper().Contains(search));
        }

        var descending = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "documenttype" => descending
                ? query.OrderByDescending(x => x.DocumentType!.DocumentTypeName).ThenBy(x => x.ItemCategory!.CategoryName)
                : query.OrderBy(x => x.DocumentType!.DocumentTypeName).ThenBy(x => x.ItemCategory!.CategoryName),
            "activatedat" => descending
                ? query.OrderByDescending(x => x.ActivatedAt).ThenBy(x => x.ItemCategory!.CategoryName)
                : query.OrderBy(x => x.ActivatedAt).ThenBy(x => x.ItemCategory!.CategoryName),
            "isactive" => descending
                ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.ItemCategory!.CategoryName)
                : query.OrderBy(x => x.IsActive).ThenBy(x => x.ItemCategory!.CategoryName),
            _ => descending
                ? query.OrderByDescending(x => x.ItemCategory!.CategoryName).ThenBy(x => x.DocumentType!.DocumentTypeName)
                : query.OrderBy(x => x.ItemCategory!.CategoryName).ThenBy(x => x.DocumentType!.DocumentTypeName)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await Project(query.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProcurementCategoryDocumentRequirementResponse>
        {
            PageNumber = request.PageNumber, PageSize = request.PageSize, TotalData = total,
            TotalPage = ProcurementMasterDataSupport.TotalPage(total, request.PageSize), Items = items
        };
    }

    public async Task<List<ProcurementCategoryDocumentRequirementOptionResponse>> GetOptionsAsync(
        bool onlyActive, Guid? itemCategoryId, string? search, CancellationToken cancellationToken)
    {
        var query = _dbContext.MstProcurementCategoryDocumentRequirements.AsNoTracking().Where(x => !x.IsDelete);
        if (onlyActive) query = query.Where(x => x.IsActive);
        if (itemCategoryId.HasValue) query = query.Where(x => x.ItemCategoryId == itemCategoryId.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim().ToUpper();
            query = query.Where(x =>
                x.ItemCategory!.CategoryName.ToUpper().Contains(keyword)
                || x.DocumentType!.DocumentTypeCode.ToUpper().Contains(keyword)
                || x.DocumentType.DocumentTypeName.ToUpper().Contains(keyword));
        }

        return await query
            .OrderBy(x => x.ItemCategory!.CategoryName).ThenBy(x => x.DocumentType!.DocumentTypeName)
            .Select(x => new ProcurementCategoryDocumentRequirementOptionResponse
            {
                Id = x.Id, ItemCategoryId = x.ItemCategoryId, ItemCategoryName = x.ItemCategory!.CategoryName,
                DocumentTypeId = x.DocumentTypeId, DocumentTypeCode = x.DocumentType!.DocumentTypeCode,
                DocumentTypeName = x.DocumentType.DocumentTypeName, IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ProcurementCategoryDocumentRequirementResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await Project(_dbContext.MstProcurementCategoryDocumentRequirements.AsNoTracking().Where(x => x.Id == id && !x.IsDelete))
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw new KeyNotFoundException("Kewajiban dokumen tidak ditemukan atau sudah dihapus.");

    public async Task<ProcurementCategoryDocumentRequirementResponse> CreateAsync(
        UpsertProcurementCategoryDocumentRequirementRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        await ValidatePairAsync(request.ItemCategoryId, request.DocumentTypeId, null, cancellationToken);
        var now = DateTime.UtcNow;
        var entity = new MstProcurementCategoryDocumentRequirement
        {
            ItemCategoryId = request.ItemCategoryId, DocumentTypeId = request.DocumentTypeId, IsActive = false,
            CreateDateTime = now, CreateBy = actorUserId
        };
        if (request.IsActive) await ActivateAsync(entity, actorUserId, cancellationToken);
        _dbContext.MstProcurementCategoryDocumentRequirements.Add(entity);
        await SaveAsync(cancellationToken);
        await AuditAsync("ProcurementCategoryDocumentRequirement.Create", entity, actorUserId);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<ProcurementCategoryDocumentRequirementResponse> UpdateAsync(
        Guid id, UpsertProcurementCategoryDocumentRequirementRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        var pairChanged = entity.ItemCategoryId != request.ItemCategoryId || entity.DocumentTypeId != request.DocumentTypeId;
        if (pairChanged && entity.ActivatedAt.HasValue)
            throw new ProcurementMasterConflictException(
                "Kewajiban ini sudah pernah berlaku; kategori dan jenis dokumennya tidak dapat diganti. Nonaktifkan kewajiban ini lalu buat kewajiban baru.");
        if (pairChanged) await ValidatePairAsync(request.ItemCategoryId, request.DocumentTypeId, id, cancellationToken);

        entity.ItemCategoryId = request.ItemCategoryId;
        entity.DocumentTypeId = request.DocumentTypeId;
        if (request.IsActive && !entity.IsActive) await ActivateAsync(entity, actorUserId, cancellationToken);
        else entity.IsActive = request.IsActive;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;
        await SaveAsync(cancellationToken);
        await AuditAsync("ProcurementCategoryDocumentRequirement.Update", entity, actorUserId);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    /// <summary>
    /// Aktifkan atau nonaktifkan kewajiban (<c>PATCH /{id}/status</c>, PBJ-DEC-061). Aktivasi pertama mengisi
    /// <c>ActivatedAt</c>; menonaktifkan tidak menghapusnya.
    /// </summary>
    public async Task<ProcurementCategoryDocumentRequirementResponse> UpdateStatusAsync(
        Guid id, UpdateProcurementMasterStatusRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (request.IsActive && !entity.IsActive) await ActivateAsync(entity, actorUserId, cancellationToken);
        else entity.IsActive = request.IsActive;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;
        await SaveAsync(cancellationToken);
        await AuditAsync(request.IsActive ? "ProcurementCategoryDocumentRequirement.Activate" : "ProcurementCategoryDocumentRequirement.Deactivate", entity, actorUserId);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<ProcurementMasterDeleteResponse> DeleteAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.MstProcurementCategoryDocumentRequirements
            .Include(x => x.ItemCategory).Include(x => x.DocumentType)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Kewajiban dokumen tidak ditemukan atau sudah dihapus.");
        if (entity.ActivatedAt.HasValue)
            throw new ProcurementMasterInUseException(
                "Kewajiban ini tidak dapat dihapus karena sudah pernah berlaku dan dipakai menilai kelayakan vendor. Nonaktifkan saja bila tidak berlaku lagi.");

        entity.IsDelete = true;
        entity.IsActive = false;
        entity.DeleteDateTime = DateTime.UtcNow;
        entity.DeleteBy = actorUserId;
        await SaveAsync(cancellationToken);
        await AuditAsync("ProcurementCategoryDocumentRequirement.Delete", entity, actorUserId);
        return new ProcurementMasterDeleteResponse
        {
            Id = entity.Id,
            Code = entity.DocumentType?.DocumentTypeCode ?? string.Empty,
            Name = $"{entity.ItemCategory?.CategoryName} — {entity.DocumentType?.DocumentTypeName}",
            IsDelete = true
        };
    }

    private async Task ActivateAsync(MstProcurementCategoryDocumentRequirement entity, Guid actorUserId, CancellationToken cancellationToken)
    {
        var category = await _dbContext.MstProcurementItemCategories.AsNoTracking()
            .Where(x => x.Id == entity.ItemCategoryId && !x.IsDelete)
            .Select(x => new { x.CategoryName, x.IsActive })
            .FirstOrDefaultAsync(cancellationToken);
        var documentType = await _dbContext.MstProcurementVendorDocumentTypes.AsNoTracking()
            .Where(x => x.Id == entity.DocumentTypeId && !x.IsDelete)
            .Select(x => new { x.DocumentTypeName, x.IsActive })
            .FirstOrDefaultAsync(cancellationToken);
        if (category == null)
            throw new ProcurementMasterValidationException("Kategori kewajiban ini sudah dihapus; kewajiban tidak dapat diaktifkan.");
        if (!category.IsActive)
            throw new ProcurementMasterValidationException(
                $"Kategori {category.CategoryName} tidak aktif; kewajiban hanya dapat diaktifkan untuk kategori aktif.");
        if (documentType == null)
            throw new ProcurementMasterValidationException("Jenis dokumen kewajiban ini sudah dihapus; kewajiban tidak dapat diaktifkan.");
        if (!documentType.IsActive)
            throw new ProcurementMasterValidationException(
                $"Jenis dokumen {documentType.DocumentTypeName} tidak aktif; aktifkan jenis dokumennya lebih dulu.");

        entity.IsActive = true;
        if (!entity.ActivatedAt.HasValue)
        {
            entity.ActivatedAt = DateTimeOffset.UtcNow;
            entity.ActivatedByUserId = actorUserId;
        }
    }

    private async Task ValidatePairAsync(Guid itemCategoryId, Guid documentTypeId, Guid? excludedId, CancellationToken cancellationToken)
    {
        if (itemCategoryId == Guid.Empty || !await _dbContext.MstProcurementItemCategories.AnyAsync(x => x.Id == itemCategoryId && !x.IsDelete, cancellationToken))
            throw new ProcurementMasterValidationException("Kategori wajib dipilih dari kategori Pengadaan yang ada.");
        if (documentTypeId == Guid.Empty || !await _dbContext.MstProcurementVendorDocumentTypes.AnyAsync(x => x.Id == documentTypeId && !x.IsDelete, cancellationToken))
            throw new ProcurementMasterValidationException("Jenis dokumen wajib dipilih dari jenis dokumen legalitas yang ada.");
        if (await _dbContext.MstProcurementCategoryDocumentRequirements.AnyAsync(
                x => !x.IsDelete && x.Id != excludedId && x.ItemCategoryId == itemCategoryId && x.DocumentTypeId == documentTypeId, cancellationToken))
            throw new ProcurementMasterConflictException("Jenis dokumen ini sudah tercatat sebagai kewajiban kategori tersebut.");
    }

    private IQueryable<ProcurementCategoryDocumentRequirementResponse> Project(IQueryable<MstProcurementCategoryDocumentRequirement> query) =>
        query.Select(x => new ProcurementCategoryDocumentRequirementResponse
        {
            Id = x.Id,
            ItemCategoryId = x.ItemCategoryId, ItemCategoryCode = x.ItemCategory!.CategoryCode, ItemCategoryName = x.ItemCategory.CategoryName,
            DocumentTypeId = x.DocumentTypeId, DocumentTypeCode = x.DocumentType!.DocumentTypeCode,
            DocumentTypeName = x.DocumentType.DocumentTypeName, RequiresExpiryDate = x.DocumentType.RequiresExpiryDate,
            IsActive = x.IsActive, ActivatedAt = x.ActivatedAt, ActivatedByUserId = x.ActivatedByUserId,
            ActivatedByName = _dbContext.Users.Where(u => u.Id == x.ActivatedByUserId).Select(u => u.DisplayName).FirstOrDefault(),
            CreateDateTime = x.CreateDateTime, CreateBy = x.CreateBy, UpdateDateTime = x.UpdateDateTime, UpdateBy = x.UpdateBy
        });

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try { await _dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException exception) when (ProcurementMasterDataSupport.IsUniqueViolation(exception))
        {
            throw new ProcurementMasterConflictException("Jenis dokumen ini sudah tercatat sebagai kewajiban kategori tersebut.");
        }
    }

    private async Task<MstProcurementCategoryDocumentRequirement> FindAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.MstProcurementCategoryDocumentRequirements.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
        ?? throw new KeyNotFoundException("Kewajiban dokumen tidak ditemukan atau sudah dihapus.");

    private Task AuditAsync(string action, MstProcurementCategoryDocumentRequirement entity, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, action, "Perubahan master dokumen wajib per kategori vendor.", new
        {
            EntityId = entity.Id,
            entity.ItemCategoryId,
            entity.DocumentTypeId,
            entity.IsActive,
            entity.ActivatedAt,
            ActorUserId = actorUserId
        });
}
