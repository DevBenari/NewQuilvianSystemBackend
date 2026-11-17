using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Services;

public sealed partial class AssetMasterDataService
{
    private const string CategoryLabel = "Kategori aset";

    private static readonly Expression<Func<MstAssetCategory, AssetCategoryResponse>> CategoryProjection = x => new AssetCategoryResponse
    {
        Id = x.Id,
        CategoryCode = x.CategoryCode,
        CategoryName = x.CategoryName,
        IsDepreciable = x.IsDepreciable,
        Description = x.Description,
        IsActive = x.IsActive,
        CreateDateTime = x.CreateDateTime,
        CreateBy = x.CreateBy,
        UpdateDateTime = x.UpdateDateTime,
        UpdateBy = x.UpdateBy
    };

    public Task<AssetMasterFilterMetadataResponse> GetCategoryMetadataAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(BuildMetadata(
            "Kode Kategori",
            "Nama Kategori",
            [new() { Value = "isDepreciable", Label = "Disusutkan" }],
            [BooleanParameter("isDepreciable", "true = kategori disusutkan, false = tidak disusutkan (misalnya Tanah)")],
            [YesNoFlag("isDepreciable", "Disusutkan", "Disusutkan", "Tidak disusutkan")],
            [
                Field("categoryCode", "Kode Kategori", "text", true, 1, MaxCodeLength, example: "ALAT_MEDIS"),
                Field("categoryName", "Nama Kategori", "text", true, 2, MaxNameLength, example: "Alat Medis"),
                Field("isDepreciable", "Disusutkan", "switch", true, 3, description: "Matikan untuk kategori yang tidak disusutkan, misalnya Tanah"),
                Field("description", "Keterangan", "textarea", false, 4, MaxDescriptionLength),
                Field("isActive", "Aktif", "switch", true, 5)
            ]));

    public async Task<AssetCategorySummaryResponse> GetCategorySummaryAsync(CancellationToken cancellationToken = default)
    {
        var query = _context.MstAssetCategories.AsNoTracking().Where(x => !x.IsDelete);
        return new AssetCategorySummaryResponse
        {
            TotalCategory = await query.CountAsync(cancellationToken),
            ActiveCategory = await query.CountAsync(x => x.IsActive, cancellationToken),
            InactiveCategory = await query.CountAsync(x => !x.IsActive, cancellationToken),
            DepreciableCategory = await query.CountAsync(x => x.IsDepreciable, cancellationToken),
            NonDepreciableCategory = await query.CountAsync(x => !x.IsDepreciable, cancellationToken)
        };
    }

    public Task<PagedResult<AssetCategoryResponse>> GetCategoriesAsync(AssetMasterPagedQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var (pageNumber, pageSize, descending, sortBy) = NormalizePaging(request);
        var query = _context.MstAssetCategories.AsNoTracking().Where(x => !x.IsDelete);

        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
        if (request.IsDepreciable.HasValue) query = query.Where(x => x.IsDepreciable == request.IsDepreciable.Value);

        var search = NormalizeSearch(request.Search);
        if (search != null)
        {
            query = query.Where(x => x.CategoryCode.ToUpper().Contains(search) || x.CategoryName.ToUpper().Contains(search));
        }

        query = sortBy switch
        {
            "code" => Sort(query, x => x.CategoryCode, descending),
            "isactive" => Sort(query, x => x.IsActive, descending),
            "createdatetime" => Sort(query, x => x.CreateDateTime, descending),
            "isdepreciable" => Sort(query, x => x.IsDepreciable, descending),
            _ => Sort(query, x => x.CategoryName, descending)
        };

        return ToPagedAsync(query, pageNumber, pageSize, x => x.Select(CategoryProjection), cancellationToken);
    }

    public async Task<List<AssetMasterOptionResponse>> GetCategoryOptionsAsync(bool onlyActive, string? search, CancellationToken cancellationToken = default)
    {
        var query = _context.MstAssetCategories.AsNoTracking().Where(x => !x.IsDelete);
        if (onlyActive) query = query.Where(x => x.IsActive);

        var keyword = NormalizeSearch(search);
        if (keyword != null)
        {
            query = query.Where(x => x.CategoryCode.ToUpper().Contains(keyword) || x.CategoryName.ToUpper().Contains(keyword));
        }

        return await query
            .OrderBy(x => x.CategoryName)
            .Select(x => new AssetMasterOptionResponse { Id = x.Id, Code = x.CategoryCode, Name = x.CategoryName, IsActive = x.IsActive })
            .ToListAsync(cancellationToken);
    }

    public async Task<AssetCategoryResponse> GetCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _context.MstAssetCategories
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDelete)
            .Select(CategoryProjection)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw NotFound(CategoryLabel);

        response.DocumentRequirements = await _context.MstAssetCategoryDocumentRequirements
            .AsNoTracking()
            .Where(x => x.CategoryId == id && !x.IsDelete && !x.DocumentType!.IsDelete)
            .OrderBy(x => x.DocumentType!.DocumentTypeName)
            .Select(x => new AssetMasterOptionResponse
            {
                Id = x.DocumentTypeId,
                Code = x.DocumentType!.DocumentTypeCode,
                Name = x.DocumentType.DocumentTypeName,
                IsActive = x.DocumentType.IsActive
            })
            .ToListAsync(cancellationToken);

        return response;
    }

    public async Task<AssetCategoryResponse> CreateCategoryAsync(AssetCategoryUpsertRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var code = RequiredCode(request.CategoryCode, "Kode kategori");
        var name = RequiredText(request.CategoryName, MaxNameLength, "Nama kategori");
        var description = OptionalText(request.Description, MaxDescriptionLength, "Keterangan");

        if (await _context.MstAssetCategories.AnyAsync(x => !x.IsDelete && x.CategoryCode == code, cancellationToken))
        {
            throw DuplicateCode();
        }

        var entity = new MstAssetCategory
        {
            CategoryCode = code,
            CategoryName = name,
            IsDepreciable = request.IsDepreciable,
            Description = description,
            IsActive = request.IsActive
        };
        StampCreate(entity, actorUserId);

        _context.MstAssetCategories.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync("AssetCategory.Create", new { entity.Id, entity.CategoryCode, entity.IsActive }, actorUserId);
        return await GetCategoryAsync(entity.Id, cancellationToken);
    }

    public async Task<AssetCategoryResponse> UpdateCategoryAsync(Guid id, AssetCategoryUpsertRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var entity = await FindCategoryAsync(id, cancellationToken);
        var code = RequiredCode(request.CategoryCode, "Kode kategori");
        var name = RequiredText(request.CategoryName, MaxNameLength, "Nama kategori");
        var description = OptionalText(request.Description, MaxDescriptionLength, "Keterangan");

        if (await _context.MstAssetCategories.AnyAsync(x => !x.IsDelete && x.Id != id && x.CategoryCode == code, cancellationToken))
        {
            throw DuplicateCode();
        }

        entity.CategoryCode = code;
        entity.CategoryName = name;
        entity.IsDepreciable = request.IsDepreciable;
        entity.Description = description;
        entity.IsActive = request.IsActive;
        StampUpdate(entity, actorUserId);

        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync("AssetCategory.Update", new { entity.Id, entity.CategoryCode, entity.IsActive }, actorUserId);
        return await GetCategoryAsync(id, cancellationToken);
    }

    public async Task<AssetCategoryResponse> UpdateCategoryStatusAsync(Guid id, AssetMasterStatusRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var entity = await FindCategoryAsync(id, cancellationToken);
        entity.IsActive = request.IsActive;
        StampUpdate(entity, actorUserId);

        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync(request.IsActive ? "AssetCategory.Activate" : "AssetCategory.Deactivate", new { entity.Id, entity.CategoryCode }, actorUserId);
        return await GetCategoryAsync(id, cancellationToken);
    }

    public async Task<AssetCategoryResponse> SetCategoryDocumentRequirementsAsync(
        Guid id,
        IReadOnlyCollection<Guid>? documentTypeIds,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        var category = await FindCategoryAsync(id, cancellationToken);
        var wanted = (documentTypeIds ?? []).Where(x => x != Guid.Empty).Distinct().ToList();

        var existing = await _context.MstAssetCategoryDocumentRequirements
            .Where(x => x.CategoryId == id && !x.IsDelete)
            .ToListAsync(cancellationToken);

        var added = wanted.Where(typeId => existing.All(x => x.DocumentTypeId != typeId)).ToList();
        var validAdded = await _context.MstAssetDocumentTypes
            .CountAsync(x => added.Contains(x.Id) && x.IsActive && !x.IsDelete, cancellationToken);
        if (validAdded != added.Count)
        {
            throw AssetManagementException.Invalid(
                AssetMasterErrorCodes.Reference,
                "Jenis dokumen yang dipilih tidak ditemukan atau sudah tidak aktif.");
        }

        foreach (var row in existing.Where(x => !wanted.Contains(x.DocumentTypeId)))
        {
            StampDelete(row, actorUserId);
        }

        foreach (var typeId in added)
        {
            var requirement = new MstAssetCategoryDocumentRequirement { CategoryId = id, DocumentTypeId = typeId };
            StampCreate(requirement, actorUserId);
            _context.MstAssetCategoryDocumentRequirements.Add(requirement);
        }

        StampUpdate(category, actorUserId);
        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync("AssetCategory.SetDocumentRequirements", new { category.Id, category.CategoryCode, DocumentTypeIds = wanted }, actorUserId);
        return await GetCategoryAsync(id, cancellationToken);
    }

    public async Task<AssetMasterDeleteResponse> DeleteCategoryAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var entity = await FindCategoryAsync(id, cancellationToken);

        if (await _context.MstAssetGroups.AnyAsync(x => x.CategoryId == id && !x.IsDelete, cancellationToken))
        {
            throw InUse("Kategori ini tidak dapat dihapus karena sudah dipakai kelompok aset. Nonaktifkan saja bila tidak dipakai lagi.");
        }

        var requirements = await _context.MstAssetCategoryDocumentRequirements
            .Where(x => x.CategoryId == id && !x.IsDelete)
            .ToListAsync(cancellationToken);
        foreach (var requirement in requirements)
        {
            StampDelete(requirement, actorUserId);
        }

        entity.IsActive = false;
        StampDelete(entity, actorUserId);
        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync("AssetCategory.Delete", new { entity.Id, entity.CategoryCode }, actorUserId);
        return new AssetMasterDeleteResponse { Id = entity.Id, Code = entity.CategoryCode, Name = entity.CategoryName, IsDelete = true };
    }

    private async Task<MstAssetCategory> FindCategoryAsync(Guid id, CancellationToken cancellationToken) =>
        await _context.MstAssetCategories.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
        ?? throw NotFound(CategoryLabel);
}
