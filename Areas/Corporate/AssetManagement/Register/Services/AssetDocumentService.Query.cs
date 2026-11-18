using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

public sealed partial class AssetDocumentService
{
    public AssetMasterFilterMetadataResponse GetMetadata() => new()
    {
        DefaultFilter = new AssetMasterDefaultFilterResponse { SortBy = "expiryDate", SortDirection = "asc" },
        SortOptions = [new() { Value = "expiryDate", Label = "Tanggal Berakhir" }],
        FlagOptions =
        [
            AssetPlacementService.EnumOptions("expiryState", "Masa Berlaku", Enum.GetValues<AssetDocumentExpiryState>(), AssetRegisterLabels.DocumentExpiryState)
        ],
        QueryParameters =
        [
            new() { Name = "search", Type = "string", Description = "Mencari pada nomor inventaris, nama aset, dan nama jenis dokumen" },
            new() { Name = "assetId", Type = "guid", Description = "Dokumen untuk satu aset" },
            new() { Name = "documentTypeId", Type = "guid", Description = "Jenis dokumen" },
            new() { Name = "categoryId", Type = "guid", Description = "Kategori aset" },
            new() { Name = "organizationUnitId", Type = "guid", Description = "Unit aset saat ini" },
            new() { Name = "expiryState", Type = "integer", Description = "Lihat flagOptions expiryState; dihitung dari pengaturan peringatan dokumen" },
            new() { Name = "sortDirection", Type = "string", Description = "asc atau desc", Example = "asc" },
            new() { Name = "pageNumber", Type = "integer", Description = "Halaman, mulai 1", Example = "1" },
            new() { Name = "pageSize", Type = "integer", Description = $"Jumlah baris per halaman, maksimal {MaxPageSize}", Example = "25" }
        ]
    };

    public async Task<AssetDocumentSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var actor = await RequireActorAsync(cancellationToken);
        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var settings = await AssetRegisterSettingReader.ReadAsync(_context, entity.LegalEntityId, cancellationToken);
        var today = AssetRegisterSettingReader.Today();
        var warnUntil = today.AddDays(settings.DocumentExpiryWarningDays);
        var documents = ScopedDocuments(actor, entity.LegalEntityId);

        return new AssetDocumentSummaryResponse
        {
            Total = await documents.CountAsync(cancellationToken),
            Expired = await documents.CountAsync(x => x.ExpiryDate != null && x.ExpiryDate < today, cancellationToken),
            ExpiringSoon = await documents.CountAsync(x => x.ExpiryDate != null && x.ExpiryDate >= today && x.ExpiryDate <= warnUntil, cancellationToken),
            IncompleteAssets = await IncompleteAssets(ScopedRegisteredAssets(actor, entity.LegalEntityId)).CountAsync(cancellationToken),
            WarningDays = settings.DocumentExpiryWarningDays
        };
    }

    public Task<PagedResult<AssetDocumentResponse>> GetDocumentsAsync(AssetDocumentPagedQuery request, CancellationToken cancellationToken = default) =>
        QueryDocumentsAsync(request, false, cancellationToken);

    public Task<PagedResult<AssetDocumentResponse>> GetExpiringAsync(AssetDocumentPagedQuery request, CancellationToken cancellationToken = default) =>
        QueryDocumentsAsync(request, true, cancellationToken);

    public async Task<AssetDocumentResponse> GetDocumentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var actor = await RequireActorAsync(cancellationToken);
        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var settings = await AssetRegisterSettingReader.ReadAsync(_context, entity.LegalEntityId, cancellationToken);

        var document = await Project(_context.AstAssetDocuments
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete && x.Asset!.LegalEntityId == entity.LegalEntityId))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw DocumentNotFound();

        actor.EnsureCanAccessUnit(document.OrganizationUnitId);
        await DescribeAsync([document], settings, cancellationToken);
        return document;
    }

    public async Task<PagedResult<AssetDocumentCompletenessResponse>> GetCompletenessAsync(AssetDocumentPagedQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var assets = ScopedRegisteredAssets(actor, entity.LegalEntityId);

        if (request.AssetId is { } assetId && assetId != Guid.Empty)
        {
            assets = assets.Where(x => x.Id == assetId);
        }

        if (request.CategoryId is { } categoryId && categoryId != Guid.Empty)
        {
            assets = assets.Where(x => x.AssetGroup!.CategoryId == categoryId);
        }

        if (request.OrganizationUnitId is { } unitId && unitId != Guid.Empty)
        {
            assets = assets.Where(x => x.CurrentOrganizationUnitId == unitId);
        }

        if (request.DocumentTypeId is { } documentTypeId && documentTypeId != Guid.Empty)
        {
            assets = assets.Where(a => !_context.AstAssetDocuments.Any(d => d.AssetId == a.Id && d.DocumentTypeId == documentTypeId && !d.IsDelete) &&
                _context.MstAssetCategoryDocumentRequirements.Any(r => r.CategoryId == a.AssetGroup!.CategoryId && r.DocumentTypeId == documentTypeId && !r.IsDelete));
        }

        var keyword = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim().ToUpper();
        if (keyword != null)
        {
            assets = assets.Where(x =>
                x.AssetName.ToUpper().Contains(keyword) ||
                (x.InventoryNumber != null && x.InventoryNumber.ToUpper().Contains(keyword)));
        }

        var incomplete = IncompleteAssets(assets);
        var ordered = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase)
            ? incomplete.OrderByDescending(x => x.InventoryNumber)
            : incomplete.OrderBy(x => x.InventoryNumber);

        var (pageNumber, pageSize) = Paging(request);
        var total = await incomplete.CountAsync(cancellationToken);
        var items = await ordered
            .ThenBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AssetDocumentCompletenessResponse
            {
                AssetId = x.Id,
                InventoryNumber = x.InventoryNumber,
                AssetName = x.AssetName,
                CategoryId = x.AssetGroup!.CategoryId,
                CategoryName = x.AssetGroup.Category!.CategoryName,
                GroupName = x.AssetGroup.GroupName,
                OrganizationUnitId = x.CurrentOrganizationUnitId,
                OrganizationUnitName = x.CurrentOrganizationUnit != null ? x.CurrentOrganizationUnit.UnitName : null
            })
            .ToListAsync(cancellationToken);

        await DescribeCompletenessAsync(items, cancellationToken);
        return Page(items, total, pageNumber, pageSize);
    }

    private async Task<PagedResult<AssetDocumentResponse>> QueryDocumentsAsync(AssetDocumentPagedQuery request, bool expiringOnly, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var settings = await AssetRegisterSettingReader.ReadAsync(_context, entity.LegalEntityId, cancellationToken);
        var today = AssetRegisterSettingReader.Today();
        var warnUntil = today.AddDays(settings.DocumentExpiryWarningDays);
        var query = ScopedDocuments(actor, entity.LegalEntityId);

        if (expiringOnly)
        {
            query = query.Where(x => x.ExpiryDate != null && x.ExpiryDate <= warnUntil);
        }

        if (request.AssetId is { } assetId && assetId != Guid.Empty)
        {
            query = query.Where(x => x.AssetId == assetId);
        }

        if (request.DocumentTypeId is { } documentTypeId && documentTypeId != Guid.Empty)
        {
            query = query.Where(x => x.DocumentTypeId == documentTypeId);
        }

        if (request.CategoryId is { } categoryId && categoryId != Guid.Empty)
        {
            query = query.Where(x => x.Asset!.AssetGroup!.CategoryId == categoryId);
        }

        if (request.OrganizationUnitId is { } unitId && unitId != Guid.Empty)
        {
            query = query.Where(x => x.Asset!.CurrentOrganizationUnitId == unitId);
        }

        query = request.ExpiryState switch
        {
            AssetDocumentExpiryState.TanpaMasaBerlaku => query.Where(x => x.ExpiryDate == null),
            AssetDocumentExpiryState.Berlaku => query.Where(x => x.ExpiryDate != null && x.ExpiryDate > warnUntil),
            AssetDocumentExpiryState.HampirHabis => query.Where(x => x.ExpiryDate != null && x.ExpiryDate >= today && x.ExpiryDate <= warnUntil),
            AssetDocumentExpiryState.Habis => query.Where(x => x.ExpiryDate != null && x.ExpiryDate < today),
            _ => query
        };

        var keyword = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim().ToUpper();
        if (keyword != null)
        {
            query = query.Where(x =>
                x.Asset!.AssetName.ToUpper().Contains(keyword) ||
                (x.Asset.InventoryNumber != null && x.Asset.InventoryNumber.ToUpper().Contains(keyword)) ||
                x.DocumentType!.DocumentTypeName.ToUpper().Contains(keyword));
        }

        var ordered = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase)
            ? query.OrderByDescending(x => x.ExpiryDate == null).ThenByDescending(x => x.ExpiryDate)
            : query.OrderBy(x => x.ExpiryDate == null).ThenBy(x => x.ExpiryDate);

        var (pageNumber, pageSize) = Paging(request);
        var total = await query.CountAsync(cancellationToken);
        var items = await Project(ordered.ThenBy(x => x.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize))
            .ToListAsync(cancellationToken);
        await DescribeAsync(items, settings, cancellationToken);
        return Page(items, total, pageNumber, pageSize);
    }

    private IQueryable<AstAssetDocument> ScopedDocuments(AssetActor actor, Guid legalEntityId)
    {
        var query = _context.AstAssetDocuments
            .AsNoTracking()
            .Where(x => !x.IsDelete && !x.Asset!.IsDelete && x.Asset.LegalEntityId == legalEntityId);
        if (actor.CanReadAllUnits)
        {
            return query;
        }

        var unitIds = actor.UnitIds.ToList();
        return query.Where(x => x.Asset!.CurrentOrganizationUnitId.HasValue && unitIds.Contains(x.Asset.CurrentOrganizationUnitId.Value));
    }

    private IQueryable<AstAsset> ScopedRegisteredAssets(AssetActor actor, Guid legalEntityId)
    {
        var query = _context.AstAssets
            .AsNoTracking()
            .Where(x => !x.IsDelete && x.LegalEntityId == legalEntityId && x.RecordStatus == AssetRecordStatus.Terdaftar);
        if (actor.CanReadAllUnits)
        {
            return query;
        }

        var unitIds = actor.UnitIds.ToList();
        return query.Where(x => x.CurrentOrganizationUnitId.HasValue && unitIds.Contains(x.CurrentOrganizationUnitId.Value));
    }

    private IQueryable<AstAsset> IncompleteAssets(IQueryable<AstAsset> assets) =>
        assets.Where(a => _context.MstAssetCategoryDocumentRequirements.Any(r =>
            r.CategoryId == a.AssetGroup!.CategoryId &&
            !r.IsDelete &&
            !_context.AstAssetDocuments.Any(d => d.AssetId == a.Id && d.DocumentTypeId == r.DocumentTypeId && !d.IsDelete)));

    private static IQueryable<AssetDocumentResponse> Project(IQueryable<AstAssetDocument> query) =>
        query.Select(x => new AssetDocumentResponse
        {
            Id = x.Id,
            AssetId = x.AssetId,
            InventoryNumber = x.Asset!.InventoryNumber,
            AssetName = x.Asset.AssetName,
            CategoryName = x.Asset.AssetGroup != null && x.Asset.AssetGroup.Category != null ? x.Asset.AssetGroup.Category.CategoryName : null,
            GroupName = x.Asset.AssetGroup != null ? x.Asset.AssetGroup.GroupName : null,
            OrganizationUnitId = x.Asset.CurrentOrganizationUnitId,
            OrganizationUnitName = x.Asset.CurrentOrganizationUnit != null ? x.Asset.CurrentOrganizationUnit.UnitName : null,
            DocumentTypeId = x.DocumentTypeId,
            DocumentTypeCode = x.DocumentType!.DocumentTypeCode,
            DocumentTypeName = x.DocumentType.DocumentTypeName,
            HasExpiry = x.DocumentType.HasExpiry,
            DocumentNumber = x.DocumentNumber,
            IssuedDate = x.IssuedDate,
            ExpiryDate = x.ExpiryDate,
            HolderName = x.HolderName,
            Notes = x.Notes,
            Attachment = x.Attachment != null
                ? new AssetDocumentAttachmentResponse
                {
                    Id = x.Attachment.Id,
                    FileName = x.Attachment.OriginalFileName,
                    ContentType = x.Attachment.ContentType,
                    SizeBytes = x.Attachment.SizeBytes
                }
                : null,
            CreateDateTime = x.CreateDateTime,
            UpdateDateTime = x.UpdateDateTime
        });

    private async Task DescribeAsync(IReadOnlyCollection<AssetDocumentResponse> documents, AssetRegisterSettingValues settings, CancellationToken cancellationToken)
    {
        var today = AssetRegisterSettingReader.Today();
        var permissions = await GetPermissionsAsync();
        var assetIds = documents.Select(x => x.AssetId).Distinct().ToList();
        var editableAssetIds = assetIds.Count == 0
            ? new List<Guid>()
            : await _context.AstAssets
                .AsNoTracking()
                .Where(x => assetIds.Contains(x.Id) && x.RecordStatus == AssetRecordStatus.Terdaftar)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

        foreach (var document in documents)
        {
            if (document.ExpiryDate is { } expiry)
            {
                document.DaysToExpiry = expiry.DayNumber - today.DayNumber;
                document.ExpiryState = expiry < today
                    ? AssetDocumentExpiryState.Habis
                    : document.DaysToExpiry <= settings.DocumentExpiryWarningDays
                        ? AssetDocumentExpiryState.HampirHabis
                        : AssetDocumentExpiryState.Berlaku;
            }
            else
            {
                document.DaysToExpiry = null;
                document.ExpiryState = AssetDocumentExpiryState.TanpaMasaBerlaku;
            }

            document.ExpiryStateName = AssetRegisterLabels.DocumentExpiryState(document.ExpiryState);
            document.AvailableActions = [];
            if (permissions.CanUpdate && editableAssetIds.Contains(document.AssetId))
            {
                document.AvailableActions.Add("update");
            }

            if (permissions.CanDelete)
            {
                document.AvailableActions.Add("delete");
            }

            if (document.Attachment != null)
            {
                document.AvailableActions.Add("download");
            }
        }
    }

    private async Task DescribeCompletenessAsync(IReadOnlyCollection<AssetDocumentCompletenessResponse> items, CancellationToken cancellationToken)
    {
        if (items.Count == 0)
        {
            return;
        }

        var categoryIds = items.Select(x => x.CategoryId).Distinct().ToList();
        var assetIds = items.Select(x => x.AssetId).ToList();
        var requirements = await _context.MstAssetCategoryDocumentRequirements
            .AsNoTracking()
            .Where(x => categoryIds.Contains(x.CategoryId) && !x.IsDelete)
            .Select(x => new
            {
                x.CategoryId,
                Type = new AssetDocumentTypeBriefResponse
                {
                    Id = x.DocumentTypeId,
                    Code = x.DocumentType!.DocumentTypeCode,
                    Name = x.DocumentType.DocumentTypeName
                }
            })
            .ToListAsync(cancellationToken);
        var owned = await _context.AstAssetDocuments
            .AsNoTracking()
            .Where(x => assetIds.Contains(x.AssetId) && !x.IsDelete)
            .Select(x => new { x.AssetId, x.DocumentTypeId })
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var item in items)
        {
            var required = requirements.Where(x => x.CategoryId == item.CategoryId).Select(x => x.Type).ToList();
            var ownedTypes = owned.Where(x => x.AssetId == item.AssetId).Select(x => x.DocumentTypeId).ToHashSet();
            item.RequiredCount = required.Count;
            item.OwnedCount = required.Count(x => ownedTypes.Contains(x.Id));
            item.MissingDocumentTypes = required.Where(x => !ownedTypes.Contains(x.Id)).OrderBy(x => x.Name).ToList();
        }
    }

    private static (int PageNumber, int PageSize) Paging(AssetDocumentPagedQuery request) =>
        (request.PageNumber < 1 ? 1 : request.PageNumber, request.PageSize < 1 ? 25 : Math.Min(request.PageSize, MaxPageSize));

    private static PagedResult<T> Page<T>(List<T> items, int total, int pageNumber, int pageSize) => new()
    {
        PageNumber = pageNumber,
        PageSize = pageSize,
        TotalData = total,
        TotalPage = (int)Math.Ceiling(total / (double)pageSize),
        Items = items
    };
}
