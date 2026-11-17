using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Services;

public sealed partial class AssetMasterDataService
{
    private const string OwnershipTypeLabel = "Jenis kepemilikan aset";

    private static readonly Expression<Func<MstAssetOwnershipType, AssetOwnershipTypeResponse>> OwnershipTypeProjection = x => new AssetOwnershipTypeResponse
    {
        Id = x.Id,
        OwnershipCode = x.OwnershipCode,
        OwnershipName = x.OwnershipName,
        IsCapitalizable = x.IsCapitalizable,
        IsActive = x.IsActive,
        CreateDateTime = x.CreateDateTime,
        CreateBy = x.CreateBy,
        UpdateDateTime = x.UpdateDateTime,
        UpdateBy = x.UpdateBy
    };

    public Task<AssetMasterFilterMetadataResponse> GetOwnershipTypeMetadataAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(BuildMetadata(
            "Kode Jenis Kepemilikan",
            "Nama Jenis Kepemilikan",
            [new() { Value = "isCapitalizable", Label = "Dapat Dikapitalisasi" }],
            [BooleanParameter("isCapitalizable", "true = aset milik RS yang dapat dikapitalisasi, false = sewa, KSO, pinjam pakai")],
            [YesNoFlag("isCapitalizable", "Dapat Dikapitalisasi", "Dapat dikapitalisasi", "Tidak dapat dikapitalisasi")],
            [
                Field("ownershipCode", "Kode Jenis Kepemilikan", "text", true, 1, MaxCodeLength, example: "MILIK"),
                Field("ownershipName", "Nama Jenis Kepemilikan", "text", true, 2, MaxNameLength, example: "Milik Rumah Sakit"),
                Field("isCapitalizable", "Dapat Dikapitalisasi", "switch", true, 3, description: "Hanya aset milik RS yang dapat dikapitalisasi dan dimanfaatkan ke pihak lain"),
                Field("isActive", "Aktif", "switch", true, 4)
            ]));

    public async Task<AssetOwnershipTypeSummaryResponse> GetOwnershipTypeSummaryAsync(CancellationToken cancellationToken = default)
    {
        var query = _context.MstAssetOwnershipTypes.AsNoTracking().Where(x => !x.IsDelete);
        return new AssetOwnershipTypeSummaryResponse
        {
            TotalOwnershipType = await query.CountAsync(cancellationToken),
            ActiveOwnershipType = await query.CountAsync(x => x.IsActive, cancellationToken),
            InactiveOwnershipType = await query.CountAsync(x => !x.IsActive, cancellationToken),
            CapitalizableOwnershipType = await query.CountAsync(x => x.IsCapitalizable, cancellationToken)
        };
    }

    public Task<PagedResult<AssetOwnershipTypeResponse>> GetOwnershipTypesAsync(AssetMasterPagedQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var (pageNumber, pageSize, descending, sortBy) = NormalizePaging(request);
        var query = _context.MstAssetOwnershipTypes.AsNoTracking().Where(x => !x.IsDelete);

        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
        if (request.IsCapitalizable.HasValue) query = query.Where(x => x.IsCapitalizable == request.IsCapitalizable.Value);

        var search = NormalizeSearch(request.Search);
        if (search != null)
        {
            query = query.Where(x => x.OwnershipCode.ToUpper().Contains(search) || x.OwnershipName.ToUpper().Contains(search));
        }

        query = sortBy switch
        {
            "code" => Sort(query, x => x.OwnershipCode, descending),
            "isactive" => Sort(query, x => x.IsActive, descending),
            "createdatetime" => Sort(query, x => x.CreateDateTime, descending),
            "iscapitalizable" => Sort(query, x => x.IsCapitalizable, descending),
            _ => Sort(query, x => x.OwnershipName, descending)
        };

        return ToPagedAsync(query, pageNumber, pageSize, x => x.Select(OwnershipTypeProjection), cancellationToken);
    }

    public async Task<List<AssetMasterOptionResponse>> GetOwnershipTypeOptionsAsync(bool onlyActive, string? search, CancellationToken cancellationToken = default)
    {
        var query = _context.MstAssetOwnershipTypes.AsNoTracking().Where(x => !x.IsDelete);
        if (onlyActive) query = query.Where(x => x.IsActive);

        var keyword = NormalizeSearch(search);
        if (keyword != null)
        {
            query = query.Where(x => x.OwnershipCode.ToUpper().Contains(keyword) || x.OwnershipName.ToUpper().Contains(keyword));
        }

        return await query
            .OrderBy(x => x.OwnershipName)
            .Select(x => new AssetMasterOptionResponse { Id = x.Id, Code = x.OwnershipCode, Name = x.OwnershipName, IsActive = x.IsActive })
            .ToListAsync(cancellationToken);
    }

    public async Task<AssetOwnershipTypeResponse> GetOwnershipTypeAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.MstAssetOwnershipTypes
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDelete)
            .Select(OwnershipTypeProjection)
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw NotFound(OwnershipTypeLabel);

    public async Task<AssetOwnershipTypeResponse> CreateOwnershipTypeAsync(AssetOwnershipTypeUpsertRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var code = RequiredCode(request.OwnershipCode, "Kode jenis kepemilikan");
        var name = RequiredText(request.OwnershipName, MaxNameLength, "Nama jenis kepemilikan");

        if (await _context.MstAssetOwnershipTypes.AnyAsync(x => !x.IsDelete && x.OwnershipCode == code, cancellationToken))
        {
            throw DuplicateCode();
        }

        var entity = new MstAssetOwnershipType
        {
            OwnershipCode = code,
            OwnershipName = name,
            IsCapitalizable = request.IsCapitalizable,
            IsActive = request.IsActive
        };
        StampCreate(entity, actorUserId);

        _context.MstAssetOwnershipTypes.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync("AssetOwnershipType.Create", new { entity.Id, entity.OwnershipCode, entity.IsActive }, actorUserId);
        return await GetOwnershipTypeAsync(entity.Id, cancellationToken);
    }

    public async Task<AssetOwnershipTypeResponse> UpdateOwnershipTypeAsync(Guid id, AssetOwnershipTypeUpsertRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var entity = await FindOwnershipTypeAsync(id, cancellationToken);
        var code = RequiredCode(request.OwnershipCode, "Kode jenis kepemilikan");
        var name = RequiredText(request.OwnershipName, MaxNameLength, "Nama jenis kepemilikan");

        if (await _context.MstAssetOwnershipTypes.AnyAsync(x => !x.IsDelete && x.Id != id && x.OwnershipCode == code, cancellationToken))
        {
            throw DuplicateCode();
        }

        entity.OwnershipCode = code;
        entity.OwnershipName = name;
        entity.IsCapitalizable = request.IsCapitalizable;
        entity.IsActive = request.IsActive;
        StampUpdate(entity, actorUserId);

        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync("AssetOwnershipType.Update", new { entity.Id, entity.OwnershipCode, entity.IsActive }, actorUserId);
        return await GetOwnershipTypeAsync(id, cancellationToken);
    }

    public async Task<AssetOwnershipTypeResponse> UpdateOwnershipTypeStatusAsync(Guid id, AssetMasterStatusRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var entity = await FindOwnershipTypeAsync(id, cancellationToken);
        entity.IsActive = request.IsActive;
        StampUpdate(entity, actorUserId);

        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync(request.IsActive ? "AssetOwnershipType.Activate" : "AssetOwnershipType.Deactivate", new { entity.Id, entity.OwnershipCode }, actorUserId);
        return await GetOwnershipTypeAsync(id, cancellationToken);
    }

    public async Task<AssetMasterDeleteResponse> DeleteOwnershipTypeAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var entity = await FindOwnershipTypeAsync(id, cancellationToken);

        if (await _context.AstAssets.AnyAsync(x => x.OwnershipTypeId == id, cancellationToken))
        {
            throw InUse("Jenis kepemilikan ini tidak dapat dihapus karena sudah dipakai aset. Nonaktifkan saja bila tidak dipakai lagi.");
        }

        entity.IsActive = false;
        StampDelete(entity, actorUserId);
        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync("AssetOwnershipType.Delete", new { entity.Id, entity.OwnershipCode }, actorUserId);
        return new AssetMasterDeleteResponse { Id = entity.Id, Code = entity.OwnershipCode, Name = entity.OwnershipName, IsDelete = true };
    }

    private async Task<MstAssetOwnershipType> FindOwnershipTypeAsync(Guid id, CancellationToken cancellationToken) =>
        await _context.MstAssetOwnershipTypes.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
        ?? throw NotFound(OwnershipTypeLabel);
}
