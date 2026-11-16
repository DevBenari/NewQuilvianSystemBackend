using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Services;

public sealed partial class AssetMasterDataService
{
    private const string DocumentTypeLabel = "Jenis dokumen aset";

    private static readonly Expression<Func<MstAssetDocumentType, AssetDocumentTypeResponse>> DocumentTypeProjection = x => new AssetDocumentTypeResponse
    {
        Id = x.Id,
        DocumentTypeCode = x.DocumentTypeCode,
        DocumentTypeName = x.DocumentTypeName,
        HasExpiry = x.HasExpiry,
        IsActive = x.IsActive,
        CreateDateTime = x.CreateDateTime,
        CreateBy = x.CreateBy,
        UpdateDateTime = x.UpdateDateTime,
        UpdateBy = x.UpdateBy
    };

    public Task<AssetMasterFilterMetadataResponse> GetDocumentTypeMetadataAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(BuildMetadata(
            "Kode Jenis Dokumen",
            "Nama Jenis Dokumen",
            [new() { Value = "hasExpiry", Label = "Punya Masa Berlaku" }],
            [BooleanParameter("hasExpiry", "true = dokumen punya tanggal berakhir (misalnya STNK, polis asuransi)")],
            [YesNoFlag("hasExpiry", "Punya Masa Berlaku", "Punya masa berlaku", "Tanpa masa berlaku")],
            [
                Field("documentTypeCode", "Kode Jenis Dokumen", "text", true, 1, MaxCodeLength, example: "STNK"),
                Field("documentTypeName", "Nama Jenis Dokumen", "text", true, 2, MaxNameLength, example: "Surat Tanda Nomor Kendaraan"),
                Field("hasExpiry", "Punya Masa Berlaku", "switch", true, 3, description: "Dokumen jenis ini wajib diisi tanggal berakhir"),
                Field("isActive", "Aktif", "switch", true, 4)
            ]));

    public async Task<AssetDocumentTypeSummaryResponse> GetDocumentTypeSummaryAsync(CancellationToken cancellationToken = default)
    {
        var query = _context.MstAssetDocumentTypes.AsNoTracking().Where(x => !x.IsDelete);
        return new AssetDocumentTypeSummaryResponse
        {
            TotalDocumentType = await query.CountAsync(cancellationToken),
            ActiveDocumentType = await query.CountAsync(x => x.IsActive, cancellationToken),
            InactiveDocumentType = await query.CountAsync(x => !x.IsActive, cancellationToken),
            ExpiringDocumentType = await query.CountAsync(x => x.HasExpiry, cancellationToken)
        };
    }

    public Task<PagedResult<AssetDocumentTypeResponse>> GetDocumentTypesAsync(AssetMasterPagedQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var (pageNumber, pageSize, descending, sortBy) = NormalizePaging(request);
        var query = _context.MstAssetDocumentTypes.AsNoTracking().Where(x => !x.IsDelete);

        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
        if (request.HasExpiry.HasValue) query = query.Where(x => x.HasExpiry == request.HasExpiry.Value);

        var search = NormalizeSearch(request.Search);
        if (search != null)
        {
            query = query.Where(x => x.DocumentTypeCode.ToUpper().Contains(search) || x.DocumentTypeName.ToUpper().Contains(search));
        }

        query = sortBy switch
        {
            "code" => Sort(query, x => x.DocumentTypeCode, descending),
            "isactive" => Sort(query, x => x.IsActive, descending),
            "createdatetime" => Sort(query, x => x.CreateDateTime, descending),
            "hasexpiry" => Sort(query, x => x.HasExpiry, descending),
            _ => Sort(query, x => x.DocumentTypeName, descending)
        };

        return ToPagedAsync(query, pageNumber, pageSize, x => x.Select(DocumentTypeProjection), cancellationToken);
    }

    public async Task<List<AssetMasterOptionResponse>> GetDocumentTypeOptionsAsync(bool onlyActive, string? search, CancellationToken cancellationToken = default)
    {
        var query = _context.MstAssetDocumentTypes.AsNoTracking().Where(x => !x.IsDelete);
        if (onlyActive) query = query.Where(x => x.IsActive);

        var keyword = NormalizeSearch(search);
        if (keyword != null)
        {
            query = query.Where(x => x.DocumentTypeCode.ToUpper().Contains(keyword) || x.DocumentTypeName.ToUpper().Contains(keyword));
        }

        return await query
            .OrderBy(x => x.DocumentTypeName)
            .Select(x => new AssetMasterOptionResponse { Id = x.Id, Code = x.DocumentTypeCode, Name = x.DocumentTypeName, IsActive = x.IsActive })
            .ToListAsync(cancellationToken);
    }

    public async Task<AssetDocumentTypeResponse> GetDocumentTypeAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.MstAssetDocumentTypes
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDelete)
            .Select(DocumentTypeProjection)
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw NotFound(DocumentTypeLabel);

    public async Task<AssetDocumentTypeResponse> CreateDocumentTypeAsync(AssetDocumentTypeUpsertRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var code = RequiredCode(request.DocumentTypeCode, "Kode jenis dokumen");
        var name = RequiredText(request.DocumentTypeName, MaxNameLength, "Nama jenis dokumen");

        if (await _context.MstAssetDocumentTypes.AnyAsync(x => !x.IsDelete && x.DocumentTypeCode == code, cancellationToken))
        {
            throw DuplicateCode();
        }

        var entity = new MstAssetDocumentType
        {
            DocumentTypeCode = code,
            DocumentTypeName = name,
            HasExpiry = request.HasExpiry,
            IsActive = request.IsActive
        };
        StampCreate(entity, actorUserId);

        _context.MstAssetDocumentTypes.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync("AssetDocumentType.Create", new { entity.Id, entity.DocumentTypeCode, entity.IsActive }, actorUserId);
        return await GetDocumentTypeAsync(entity.Id, cancellationToken);
    }

    public async Task<AssetDocumentTypeResponse> UpdateDocumentTypeAsync(Guid id, AssetDocumentTypeUpsertRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var entity = await FindDocumentTypeAsync(id, cancellationToken);
        var code = RequiredCode(request.DocumentTypeCode, "Kode jenis dokumen");
        var name = RequiredText(request.DocumentTypeName, MaxNameLength, "Nama jenis dokumen");

        if (await _context.MstAssetDocumentTypes.AnyAsync(x => !x.IsDelete && x.Id != id && x.DocumentTypeCode == code, cancellationToken))
        {
            throw DuplicateCode();
        }

        entity.DocumentTypeCode = code;
        entity.DocumentTypeName = name;
        entity.HasExpiry = request.HasExpiry;
        entity.IsActive = request.IsActive;
        StampUpdate(entity, actorUserId);

        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync("AssetDocumentType.Update", new { entity.Id, entity.DocumentTypeCode, entity.IsActive }, actorUserId);
        return await GetDocumentTypeAsync(id, cancellationToken);
    }

    public async Task<AssetDocumentTypeResponse> UpdateDocumentTypeStatusAsync(Guid id, AssetMasterStatusRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var entity = await FindDocumentTypeAsync(id, cancellationToken);
        entity.IsActive = request.IsActive;
        StampUpdate(entity, actorUserId);

        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync(request.IsActive ? "AssetDocumentType.Activate" : "AssetDocumentType.Deactivate", new { entity.Id, entity.DocumentTypeCode }, actorUserId);
        return await GetDocumentTypeAsync(id, cancellationToken);
    }

    public async Task<AssetMasterDeleteResponse> DeleteDocumentTypeAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var entity = await FindDocumentTypeAsync(id, cancellationToken);

        if (await _context.MstAssetCategoryDocumentRequirements.AnyAsync(x => x.DocumentTypeId == id && !x.IsDelete, cancellationToken) ||
            await _context.AstAssetDocuments.AnyAsync(x => x.DocumentTypeId == id, cancellationToken))
        {
            throw InUse("Jenis dokumen ini tidak dapat dihapus karena sudah dipakai dokumen aset atau dokumen wajib kategori. Nonaktifkan saja bila tidak dipakai lagi.");
        }

        entity.IsActive = false;
        StampDelete(entity, actorUserId);
        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync("AssetDocumentType.Delete", new { entity.Id, entity.DocumentTypeCode }, actorUserId);
        return new AssetMasterDeleteResponse { Id = entity.Id, Code = entity.DocumentTypeCode, Name = entity.DocumentTypeName, IsDelete = true };
    }

    private async Task<MstAssetDocumentType> FindDocumentTypeAsync(Guid id, CancellationToken cancellationToken) =>
        await _context.MstAssetDocumentTypes.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
        ?? throw NotFound(DocumentTypeLabel);
}
