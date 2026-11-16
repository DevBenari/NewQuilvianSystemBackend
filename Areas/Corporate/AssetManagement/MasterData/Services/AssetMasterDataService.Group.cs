using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Services;

public sealed partial class AssetMasterDataService
{
    private const string GroupLabel = "Kelompok aset";
    private const int MaxInventoryCodeLength = 11;
    private const string GlobalNumberScope = "GLOBAL";

    public static readonly IReadOnlySet<string> ReservedInventoryCodes =
        new HashSet<string>(["PNY", "KND", "PMF", "IMP", "MUT", "PJM", "OPN", "NLI", "HPS", "PLH"], StringComparer.Ordinal);

    public Task<AssetMasterFilterMetadataResponse> GetGroupMetadataAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(BuildMetadata(
            "Kode Inventaris",
            "Nama Kelompok",
            [
                new() { Value = "category", Label = "Kategori" },
                new() { Value = "classification", Label = "Klasifikasi" }
            ],
            [
                GuidParameter("categoryId", "Menyaring kelompok satu kategori"),
                GuidParameter("classificationId", "Menyaring kelompok satu klasifikasi"),
                BooleanParameter("isMedical", "true = kelompok berklasifikasi medis")
            ],
            [YesNoFlag("isMedical", "Medis", "Medis", "Non-medis")],
            [
                Field("categoryId", "Kategori", "select", true, 1, optionsSource: "api/v1/corporate/asset-management/master-data/categories/options"),
                Field("classificationId", "Klasifikasi", "select", true, 2, optionsSource: "api/v1/corporate/asset-management/master-data/classifications/options"),
                Field("inventoryCode", "Kode Inventaris", "text", true, 3, MaxInventoryCodeLength,
                    description: "2–11 huruf besar atau angka; masuk ke nomor aset AST-<KODE>-######; terkunci setelah ada aset bernomor", example: "ALKMED"),
                Field("groupName", "Nama Kelompok", "text", true, 4, MaxNameLength, example: "Alat Kedokteran Umum"),
                Field("description", "Keterangan", "textarea", false, 5, MaxDescriptionLength),
                Field("isActive", "Aktif", "switch", true, 6)
            ]));

    public async Task<AssetGroupSummaryResponse> GetGroupSummaryAsync(CancellationToken cancellationToken = default)
    {
        var query = _context.MstAssetGroups.AsNoTracking().Where(x => !x.IsDelete);
        return new AssetGroupSummaryResponse
        {
            TotalGroup = await query.CountAsync(cancellationToken),
            ActiveGroup = await query.CountAsync(x => x.IsActive, cancellationToken),
            InactiveGroup = await query.CountAsync(x => !x.IsActive, cancellationToken),
            MedicalGroup = await query.CountAsync(x => x.Classification!.IsMedical, cancellationToken),
            LockedInventoryCodeGroup = await query.CountAsync(
                x => _context.AstAssets.Any(a => a.AssetGroupId == x.Id && a.InventoryNumber != null),
                cancellationToken)
        };
    }

    public Task<PagedResult<AssetGroupResponse>> GetGroupsAsync(AssetMasterPagedQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var (pageNumber, pageSize, descending, sortBy) = NormalizePaging(request);
        var query = FilterGroups(request);

        query = sortBy switch
        {
            "code" => Sort(query, x => x.InventoryCode, descending),
            "isactive" => Sort(query, x => x.IsActive, descending),
            "createdatetime" => Sort(query, x => x.CreateDateTime, descending),
            "category" => Sort(query, x => x.Category!.CategoryName, descending),
            "classification" => Sort(query, x => x.Classification!.ClassificationName, descending),
            _ => Sort(query, x => x.GroupName, descending)
        };

        return ToPagedAsync(query, pageNumber, pageSize, x => x.Select(GroupProjection()), cancellationToken);
    }

    public async Task<PagedResult<AssetInventoryCodeResponse>> GetInventoryCodesAsync(AssetMasterPagedQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var (pageNumber, pageSize, descending, sortBy) = NormalizePaging(request);
        var query = FilterGroups(request);

        query = sortBy switch
        {
            "name" => Sort(query, x => x.GroupName, descending),
            "isactive" => Sort(query, x => x.IsActive, descending),
            "category" => Sort(query, x => x.Category!.CategoryName, descending),
            _ => Sort(query, x => x.InventoryCode, descending)
        };

        var page = await ToPagedAsync(query, pageNumber, pageSize, x => x.Select(g => new AssetInventoryCodeResponse
        {
            GroupId = g.Id,
            InventoryCode = g.InventoryCode,
            GroupName = g.GroupName,
            CategoryName = g.Category!.CategoryName,
            IsActive = g.IsActive,
            IsInventoryCodeLocked = _context.AstAssets.Any(a => a.AssetGroupId == g.Id && a.InventoryNumber != null)
        }), cancellationToken);

        var sequenceKeys = page.Items.Select(x => AssetNumberingService.InventorySequenceKeyPrefix + x.InventoryCode).ToList();
        var series = await _context.NumNumberSeries
            .AsNoTracking()
            .Where(x => sequenceKeys.Contains(x.SequenceKey) && x.ScopeKey == GlobalNumberScope)
            .Select(x => new { x.SequenceKey, x.CurrentValue, x.LastAllocatedAt })
            .ToListAsync(cancellationToken);
        var seriesByKey = series.ToDictionary(x => x.SequenceKey, StringComparer.Ordinal);

        foreach (var item in page.Items)
        {
            if (seriesByKey.TryGetValue(AssetNumberingService.InventorySequenceKeyPrefix + item.InventoryCode, out var current) && current.CurrentValue > 0)
            {
                item.LastSequence = current.CurrentValue;
                item.LastIssuedNumber = $"{AssetNumberingService.InventoryNumberPrefix}{item.InventoryCode}-{current.CurrentValue.ToString().PadLeft(AssetNumberingService.SequenceDigits, '0')}";
                item.LastIssuedAt = current.LastAllocatedAt;
            }
        }

        return page;
    }

    public async Task<List<AssetMasterOptionResponse>> GetGroupOptionsAsync(bool onlyActive, string? search, Guid? categoryId, CancellationToken cancellationToken = default)
    {
        var query = _context.MstAssetGroups.AsNoTracking().Where(x => !x.IsDelete);
        if (onlyActive) query = query.Where(x => x.IsActive);
        if (categoryId.HasValue) query = query.Where(x => x.CategoryId == categoryId.Value);

        var keyword = NormalizeSearch(search);
        if (keyword != null)
        {
            query = query.Where(x => x.InventoryCode.ToUpper().Contains(keyword) || x.GroupName.ToUpper().Contains(keyword));
        }

        return await query
            .OrderBy(x => x.GroupName)
            .Select(x => new AssetMasterOptionResponse { Id = x.Id, Code = x.InventoryCode, Name = x.GroupName, IsActive = x.IsActive })
            .ToListAsync(cancellationToken);
    }

    public async Task<AssetGroupResponse> GetGroupAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.MstAssetGroups
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDelete)
            .Select(GroupProjection())
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw NotFound(GroupLabel);

    public async Task<AssetGroupResponse> CreateGroupAsync(AssetGroupUpsertRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var values = await ValidateGroupAsync(request, null, cancellationToken);

        var entity = new MstAssetGroup
        {
            CategoryId = values.CategoryId,
            ClassificationId = values.ClassificationId,
            InventoryCode = values.InventoryCode,
            GroupName = values.GroupName,
            Description = values.Description,
            IsActive = request.IsActive
        };
        StampCreate(entity, actorUserId);

        _context.MstAssetGroups.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync("AssetGroup.Create", new { entity.Id, entity.InventoryCode, entity.IsActive }, actorUserId);
        return await GetGroupAsync(entity.Id, cancellationToken);
    }

    public async Task<AssetGroupResponse> UpdateGroupAsync(Guid id, AssetGroupUpsertRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var entity = await FindGroupAsync(id, cancellationToken);
        var values = await ValidateGroupAsync(request, entity, cancellationToken);

        entity.CategoryId = values.CategoryId;
        entity.ClassificationId = values.ClassificationId;
        entity.InventoryCode = values.InventoryCode;
        entity.GroupName = values.GroupName;
        entity.Description = values.Description;
        entity.IsActive = request.IsActive;
        StampUpdate(entity, actorUserId);

        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync("AssetGroup.Update", new { entity.Id, entity.InventoryCode, entity.IsActive }, actorUserId);
        return await GetGroupAsync(id, cancellationToken);
    }

    public async Task<AssetGroupResponse> UpdateGroupStatusAsync(Guid id, AssetMasterStatusRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var entity = await FindGroupAsync(id, cancellationToken);
        entity.IsActive = request.IsActive;
        StampUpdate(entity, actorUserId);

        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync(request.IsActive ? "AssetGroup.Activate" : "AssetGroup.Deactivate", new { entity.Id, entity.InventoryCode }, actorUserId);
        return await GetGroupAsync(id, cancellationToken);
    }

    public async Task<AssetMasterDeleteResponse> DeleteGroupAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var entity = await FindGroupAsync(id, cancellationToken);

        if (await _context.AstAssets.AnyAsync(x => x.AssetGroupId == id, cancellationToken) ||
            await _context.AstDepreciationPolicies.AnyAsync(x => x.AssetGroupId == id && !x.IsDelete, cancellationToken))
        {
            throw InUse("Kelompok ini tidak dapat dihapus karena sudah dipakai aset atau kebijakan penyusutan. Nonaktifkan saja bila tidak dipakai lagi.");
        }

        entity.IsActive = false;
        StampDelete(entity, actorUserId);
        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync("AssetGroup.Delete", new { entity.Id, entity.InventoryCode }, actorUserId);
        return new AssetMasterDeleteResponse { Id = entity.Id, Code = entity.InventoryCode, Name = entity.GroupName, IsDelete = true };
    }

    public static string NormalizeInventoryCode(string? inventoryCode)
    {
        var code = inventoryCode?.Trim().ToUpperInvariant() ?? string.Empty;
        if (!InventoryCodePattern().IsMatch(code))
        {
            throw AssetManagementException.Invalid(AssetMasterErrorCodes.InventoryCodeFormat, AssetMasterMessages.InventoryCodeFormat);
        }

        if (ReservedInventoryCodes.Contains(code))
        {
            throw AssetManagementException.Invalid(AssetMasterErrorCodes.InventoryCodeReserved, AssetMasterMessages.InventoryCodeReserved);
        }

        return code;
    }

    private IQueryable<MstAssetGroup> FilterGroups(AssetMasterPagedQuery request)
    {
        var query = _context.MstAssetGroups.AsNoTracking().Where(x => !x.IsDelete);

        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
        if (request.CategoryId.HasValue) query = query.Where(x => x.CategoryId == request.CategoryId.Value);
        if (request.ClassificationId.HasValue) query = query.Where(x => x.ClassificationId == request.ClassificationId.Value);
        if (request.IsMedical.HasValue) query = query.Where(x => x.Classification!.IsMedical == request.IsMedical.Value);

        var search = NormalizeSearch(request.Search);
        if (search != null)
        {
            query = query.Where(x => x.InventoryCode.ToUpper().Contains(search) || x.GroupName.ToUpper().Contains(search));
        }

        return query;
    }

    private Expression<Func<MstAssetGroup, AssetGroupResponse>> GroupProjection() => x => new AssetGroupResponse
    {
        Id = x.Id,
        CategoryId = x.CategoryId,
        CategoryName = x.Category!.CategoryName,
        ClassificationId = x.ClassificationId,
        ClassificationName = x.Classification!.ClassificationName,
        IsMedical = x.Classification.IsMedical,
        InventoryCode = x.InventoryCode,
        GroupName = x.GroupName,
        Description = x.Description,
        IsInventoryCodeLocked = _context.AstAssets.Any(a => a.AssetGroupId == x.Id && a.InventoryNumber != null),
        IsActive = x.IsActive,
        CreateDateTime = x.CreateDateTime,
        CreateBy = x.CreateBy,
        UpdateDateTime = x.UpdateDateTime,
        UpdateBy = x.UpdateBy
    };

    private async Task<(Guid CategoryId, Guid ClassificationId, string InventoryCode, string GroupName, string? Description)> ValidateGroupAsync(
        AssetGroupUpsertRequest request,
        MstAssetGroup? current,
        CancellationToken cancellationToken)
    {
        var categoryId = RequiredId(request.CategoryId, "Kategori");
        var classificationId = RequiredId(request.ClassificationId, "Klasifikasi");
        var inventoryCode = NormalizeInventoryCode(request.InventoryCode);
        var groupName = RequiredText(request.GroupName, MaxNameLength, "Nama kelompok");
        var description = OptionalText(request.Description, MaxDescriptionLength, "Keterangan");

        if (current?.CategoryId != categoryId &&
            !await _context.MstAssetCategories.AnyAsync(x => x.Id == categoryId && x.IsActive && !x.IsDelete, cancellationToken))
        {
            throw AssetManagementException.Invalid(AssetMasterErrorCodes.Reference, "Kategori aset tidak ditemukan atau sudah tidak aktif.");
        }

        if (current?.ClassificationId != classificationId &&
            !await _context.MstAssetClassifications.AnyAsync(x => x.Id == classificationId && x.IsActive && !x.IsDelete, cancellationToken))
        {
            throw AssetManagementException.Invalid(AssetMasterErrorCodes.Reference, "Klasifikasi aset tidak ditemukan atau sudah tidak aktif.");
        }

        if (await _context.MstAssetGroups.AnyAsync(x => !x.IsDelete && x.Id != (current == null ? Guid.Empty : current.Id) && x.InventoryCode == inventoryCode, cancellationToken))
        {
            throw DuplicateCode();
        }

        if (current != null &&
            !string.Equals(current.InventoryCode, inventoryCode, StringComparison.Ordinal) &&
            await _context.AstAssets.AnyAsync(x => x.AssetGroupId == current.Id && x.InventoryNumber != null, cancellationToken))
        {
            throw AssetManagementException.Conflict(AssetMasterErrorCodes.InventoryCodeLocked, AssetMasterMessages.InventoryCodeLocked);
        }

        return (categoryId, classificationId, inventoryCode, groupName, description);
    }

    private async Task<MstAssetGroup> FindGroupAsync(Guid id, CancellationToken cancellationToken) =>
        await _context.MstAssetGroups.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
        ?? throw NotFound(GroupLabel);

    [GeneratedRegex("^[A-Z0-9]{2,11}$", RegexOptions.CultureInvariant)]
    private static partial Regex InventoryCodePattern();
}
