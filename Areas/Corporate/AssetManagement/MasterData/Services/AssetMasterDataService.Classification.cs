using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Services;

public sealed partial class AssetMasterDataService
{
    private const string ClassificationLabel = "Klasifikasi aset";

    private static readonly Expression<Func<MstAssetClassification, AssetClassificationResponse>> ClassificationProjection = x => new AssetClassificationResponse
    {
        Id = x.Id,
        ClassificationCode = x.ClassificationCode,
        ClassificationName = x.ClassificationName,
        IsMedical = x.IsMedical,
        IsActive = x.IsActive,
        CreateDateTime = x.CreateDateTime,
        CreateBy = x.CreateBy,
        UpdateDateTime = x.UpdateDateTime,
        UpdateBy = x.UpdateBy
    };

    public Task<AssetMasterFilterMetadataResponse> GetClassificationMetadataAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(BuildMetadata(
            "Kode Klasifikasi",
            "Nama Klasifikasi",
            [new() { Value = "isMedical", Label = "Medis" }],
            [BooleanParameter("isMedical", "true = klasifikasi medis (atribut alat medis wajib), false = non-medis")],
            [YesNoFlag("isMedical", "Medis", "Medis", "Non-medis")],
            [
                Field("classificationCode", "Kode Klasifikasi", "text", true, 1, MaxCodeLength, example: "MEDIS"),
                Field("classificationName", "Nama Klasifikasi", "text", true, 2, MaxNameLength, example: "Medis"),
                Field("isMedical", "Medis", "switch", true, 3, description: "Aset medis wajib diisi merek, tipe, nomor seri, produsen, dan izin edar"),
                Field("isActive", "Aktif", "switch", true, 4)
            ]));

    public async Task<AssetClassificationSummaryResponse> GetClassificationSummaryAsync(CancellationToken cancellationToken = default)
    {
        var query = _context.MstAssetClassifications.AsNoTracking().Where(x => !x.IsDelete);
        return new AssetClassificationSummaryResponse
        {
            TotalClassification = await query.CountAsync(cancellationToken),
            ActiveClassification = await query.CountAsync(x => x.IsActive, cancellationToken),
            InactiveClassification = await query.CountAsync(x => !x.IsActive, cancellationToken),
            MedicalClassification = await query.CountAsync(x => x.IsMedical, cancellationToken)
        };
    }

    public Task<PagedResult<AssetClassificationResponse>> GetClassificationsAsync(AssetMasterPagedQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var (pageNumber, pageSize, descending, sortBy) = NormalizePaging(request);
        var query = _context.MstAssetClassifications.AsNoTracking().Where(x => !x.IsDelete);

        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
        if (request.IsMedical.HasValue) query = query.Where(x => x.IsMedical == request.IsMedical.Value);

        var search = NormalizeSearch(request.Search);
        if (search != null)
        {
            query = query.Where(x => x.ClassificationCode.ToUpper().Contains(search) || x.ClassificationName.ToUpper().Contains(search));
        }

        query = sortBy switch
        {
            "code" => Sort(query, x => x.ClassificationCode, descending),
            "isactive" => Sort(query, x => x.IsActive, descending),
            "createdatetime" => Sort(query, x => x.CreateDateTime, descending),
            "ismedical" => Sort(query, x => x.IsMedical, descending),
            _ => Sort(query, x => x.ClassificationName, descending)
        };

        return ToPagedAsync(query, pageNumber, pageSize, x => x.Select(ClassificationProjection), cancellationToken);
    }

    public async Task<List<AssetMasterOptionResponse>> GetClassificationOptionsAsync(bool onlyActive, string? search, CancellationToken cancellationToken = default)
    {
        var query = _context.MstAssetClassifications.AsNoTracking().Where(x => !x.IsDelete);
        if (onlyActive) query = query.Where(x => x.IsActive);

        var keyword = NormalizeSearch(search);
        if (keyword != null)
        {
            query = query.Where(x => x.ClassificationCode.ToUpper().Contains(keyword) || x.ClassificationName.ToUpper().Contains(keyword));
        }

        return await query
            .OrderBy(x => x.ClassificationName)
            .Select(x => new AssetMasterOptionResponse { Id = x.Id, Code = x.ClassificationCode, Name = x.ClassificationName, IsActive = x.IsActive })
            .ToListAsync(cancellationToken);
    }

    public async Task<AssetClassificationResponse> GetClassificationAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.MstAssetClassifications
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDelete)
            .Select(ClassificationProjection)
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw NotFound(ClassificationLabel);

    public async Task<AssetClassificationResponse> CreateClassificationAsync(AssetClassificationUpsertRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var code = RequiredCode(request.ClassificationCode, "Kode klasifikasi");
        var name = RequiredText(request.ClassificationName, MaxNameLength, "Nama klasifikasi");

        if (await _context.MstAssetClassifications.AnyAsync(x => !x.IsDelete && x.ClassificationCode == code, cancellationToken))
        {
            throw DuplicateCode();
        }

        var entity = new MstAssetClassification
        {
            ClassificationCode = code,
            ClassificationName = name,
            IsMedical = request.IsMedical,
            IsActive = request.IsActive
        };
        StampCreate(entity, actorUserId);

        _context.MstAssetClassifications.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync("AssetClassification.Create", new { entity.Id, entity.ClassificationCode, entity.IsActive }, actorUserId);
        return await GetClassificationAsync(entity.Id, cancellationToken);
    }

    public async Task<AssetClassificationResponse> UpdateClassificationAsync(Guid id, AssetClassificationUpsertRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var entity = await FindClassificationAsync(id, cancellationToken);
        var code = RequiredCode(request.ClassificationCode, "Kode klasifikasi");
        var name = RequiredText(request.ClassificationName, MaxNameLength, "Nama klasifikasi");

        if (await _context.MstAssetClassifications.AnyAsync(x => !x.IsDelete && x.Id != id && x.ClassificationCode == code, cancellationToken))
        {
            throw DuplicateCode();
        }

        entity.ClassificationCode = code;
        entity.ClassificationName = name;
        entity.IsMedical = request.IsMedical;
        entity.IsActive = request.IsActive;
        StampUpdate(entity, actorUserId);

        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync("AssetClassification.Update", new { entity.Id, entity.ClassificationCode, entity.IsActive }, actorUserId);
        return await GetClassificationAsync(id, cancellationToken);
    }

    public async Task<AssetClassificationResponse> UpdateClassificationStatusAsync(Guid id, AssetMasterStatusRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var entity = await FindClassificationAsync(id, cancellationToken);
        entity.IsActive = request.IsActive;
        StampUpdate(entity, actorUserId);

        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync(request.IsActive ? "AssetClassification.Activate" : "AssetClassification.Deactivate", new { entity.Id, entity.ClassificationCode }, actorUserId);
        return await GetClassificationAsync(id, cancellationToken);
    }

    public async Task<AssetMasterDeleteResponse> DeleteClassificationAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var entity = await FindClassificationAsync(id, cancellationToken);

        if (await _context.MstAssetGroups.AnyAsync(x => x.ClassificationId == id && !x.IsDelete, cancellationToken))
        {
            throw InUse("Klasifikasi ini tidak dapat dihapus karena sudah dipakai kelompok aset. Nonaktifkan saja bila tidak dipakai lagi.");
        }

        entity.IsActive = false;
        StampDelete(entity, actorUserId);
        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync("AssetClassification.Delete", new { entity.Id, entity.ClassificationCode }, actorUserId);
        return new AssetMasterDeleteResponse { Id = entity.Id, Code = entity.ClassificationCode, Name = entity.ClassificationName, IsDelete = true };
    }

    private async Task<MstAssetClassification> FindClassificationAsync(Guid id, CancellationToken cancellationToken) =>
        await _context.MstAssetClassifications.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
        ?? throw NotFound(ClassificationLabel);
}
