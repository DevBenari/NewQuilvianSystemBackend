using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Services;

/// <summary>
/// Katalog barang/jasa Pengadaan (BE-PBJ-003, PBJ-DEC-022). Kode <c>KTL-000045</c> dibuat lewat
/// <see cref="NumberSeriesAllocator"/> (QBE-CODE-002/003, PBJ-DEC-031).
/// </summary>
/// <remarks>
/// <para>Aturan yang dijaga service:</para>
/// <list type="bullet">
/// <item><c>PBJ-VAL-070</c> — barang serupa ditolak: nama sama (tanpa beda huruf besar/kecil),
/// jenis, kategori, dan satuan sama. Contoh: "Kursi Roda Standar" / Aset / Alat Bantu / Unit
/// tidak bisa dibuat dua kali.</item>
/// <item><c>PBJ-VAL-072</c> — rujukan obat hanya untuk jenis barang, bukan jasa.</item>
/// <item><c>PBJ-VAL-071</c> — barang yang dipakai dokumen hanya dapat dinonaktifkan. Tabel
/// pemakainya (baris PR, rencana) dibuat BE-PBJ-006/009, dan task itu menambah pemeriksaannya
/// di <see cref="DeleteAsync"/>.</item>
/// </list>
/// </remarks>
public sealed class ProcurementItemService
{
    private const string LogCategory = "Corporate.ProcurementManagement.MasterData";
    private readonly ApplicationDbContext _dbContext;
    private readonly NumberSeriesAllocator _numberAllocator;
    private readonly LoggerService _loggerService;

    public ProcurementItemService(
        ApplicationDbContext dbContext,
        NumberSeriesAllocator numberAllocator,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _numberAllocator = numberAllocator;
        _loggerService = loggerService;
    }

    public async Task<ProcurementItemFilterMetadataResponse> GetFilterMetadataAsync(CancellationToken cancellationToken) =>
        new()
        {
            DefaultFilter = new ProcurementItemDefaultFilterResponse(),
            SortOptions =
            [
                new() { Value = "itemCode", Label = "Kode" },
                new() { Value = "itemName", Label = "Nama barang/jasa" },
                new() { Value = "isActive", Label = "Status aktif" }
            ],
            ProcurementTypeOptions = await _dbContext.MstProcurementTypes.AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive)
                .OrderBy(x => x.TypeName)
                .Select(x => new ProcurementTypeResponse
                {
                    Id = x.Id, TypeCode = x.TypeCode, TypeName = x.TypeName, IsGoods = x.IsGoods,
                    Description = x.Description, IsActive = x.IsActive
                })
                .ToListAsync(cancellationToken)
        };

    public async Task<ProcurementItemSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var query = _dbContext.MstProcurementItems.AsNoTracking().Where(x => !x.IsDelete);
        return new ProcurementItemSummaryResponse
        {
            TotalItem = await query.CountAsync(cancellationToken),
            ActiveItem = await query.CountAsync(x => x.IsActive, cancellationToken),
            InactiveItem = await query.CountAsync(x => !x.IsActive, cancellationToken),
            GoodsItem = await query.CountAsync(x => x.ProcurementType!.IsGoods, cancellationToken),
            ServiceItem = await query.CountAsync(x => !x.ProcurementType!.IsGoods, cancellationToken),
            DrugLinkedItem = await query.CountAsync(x => x.DrugId != null, cancellationToken)
        };
    }

    public async Task<PagedResult<ProcurementItemResponse>> GetPagedAsync(ProcurementItemQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.MstProcurementItems.AsNoTracking().Where(x => !x.IsDelete);
        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
        if (request.ProcurementTypeId.HasValue) query = query.Where(x => x.ProcurementTypeId == request.ProcurementTypeId.Value);
        if (request.ItemCategoryId.HasValue) query = query.Where(x => x.ItemCategoryId == request.ItemCategoryId.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpper();
            query = query.Where(x => x.ItemCode.ToUpper().Contains(search) || x.ItemName.ToUpper().Contains(search));
        }

        var descending = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "itemcode" => descending ? query.OrderByDescending(x => x.ItemCode) : query.OrderBy(x => x.ItemCode),
            "isactive" => descending
                ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.ItemName)
                : query.OrderBy(x => x.IsActive).ThenBy(x => x.ItemName),
            _ => descending ? query.OrderByDescending(x => x.ItemName) : query.OrderBy(x => x.ItemName)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await Project(query.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProcurementItemResponse>
        {
            PageNumber = request.PageNumber, PageSize = request.PageSize, TotalData = total,
            TotalPage = ProcurementMasterDataSupport.TotalPage(total, request.PageSize), Items = items
        };
    }

    public async Task<PagedResult<ProcurementItemOptionResponse>> GetOptionsAsync(ProcurementItemOptionsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.MstProcurementItems.AsNoTracking().Where(x => !x.IsDelete);
        if (request.OnlyActive) query = query.Where(x => x.IsActive);
        if (request.ProcurementTypeId.HasValue) query = query.Where(x => x.ProcurementTypeId == request.ProcurementTypeId.Value);
        if (request.ItemCategoryId.HasValue) query = query.Where(x => x.ItemCategoryId == request.ItemCategoryId.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim().ToUpper();
            query = query.Where(x => x.ItemCode.ToUpper().Contains(keyword) || x.ItemName.ToUpper().Contains(keyword));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.ItemName).ThenBy(x => x.ItemCode)
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .Select(x => new ProcurementItemOptionResponse
            {
                Id = x.Id,
                ItemCode = x.ItemCode,
                ItemName = x.ItemName,
                ProcurementTypeId = x.ProcurementTypeId,
                ProcurementTypeCode = x.ProcurementType!.TypeCode,
                IsGoods = x.ProcurementType.IsGoods,
                ItemCategoryId = x.ItemCategoryId,
                ItemCategoryName = x.ItemCategory!.CategoryName,
                MeasurementId = x.MeasurementId,
                MeasurementName = x.Measurement!.MeasurementName,
                IsDecimalAllowed = x.Measurement.IsDecimalAllowed,
                DefaultSpecification = x.DefaultSpecification,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ProcurementItemOptionResponse>
        {
            PageNumber = request.PageNumber, PageSize = request.PageSize, TotalData = total,
            TotalPage = ProcurementMasterDataSupport.TotalPage(total, request.PageSize), Items = items
        };
    }

    public async Task<ProcurementItemResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await Project(_dbContext.MstProcurementItems.AsNoTracking().Where(x => x.Id == id && !x.IsDelete))
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw new KeyNotFoundException("Barang/jasa tidak ditemukan atau sudah dihapus.");

    public async Task<ProcurementItemResponse> CreateAsync(UpsertProcurementItemRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var values = await ValidateAsync(request, null, cancellationToken);

        string itemCode;
        try
        {
            itemCode = await _numberAllocator.AllocateAsync(
                new NumberAllocationRequest(
                    SequenceKey: ProcurementNumberSeries.ItemSequenceKey,
                    Prefix: ProcurementNumberSeries.ItemPrefix,
                    ResetPolicy: ProcurementNumberSeries.ResetPolicy,
                    SequenceDigits: ProcurementNumberSeries.SequenceDigits,
                    ActorUserId: actorUserId,
                    Instant: DateTimeOffset.UtcNow),
                cancellationToken);
        }
        catch (NumberSeriesAllocationException)
        {
            throw new ProcurementMasterConflictException("Kode katalog gagal dibuat. Silakan coba lagi.");
        }

        var entity = new MstProcurementItem
        {
            ItemCode = itemCode,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };
        Apply(entity, values, request.IsActive);
        _dbContext.MstProcurementItems.Add(entity);
        await SaveAsync(cancellationToken);
        await AuditAsync("ProcurementItem.Create", entity, actorUserId);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<ProcurementItemResponse> UpdateAsync(Guid id, UpsertProcurementItemRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        var values = await ValidateAsync(request, id, cancellationToken);
        Apply(entity, values, request.IsActive);
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;
        await SaveAsync(cancellationToken);
        await AuditAsync("ProcurementItem.Update", entity, actorUserId);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<ProcurementItemResponse> UpdateStatusAsync(Guid id, UpdateProcurementMasterStatusRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        entity.IsActive = request.IsActive;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;
        await SaveAsync(cancellationToken);
        await AuditAsync(request.IsActive ? "ProcurementItem.Activate" : "ProcurementItem.Deactivate", entity, actorUserId);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<ProcurementMasterDeleteResponse> DeleteAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);

        // PBJ-VAL-071: barang yang sudah dipakai baris PR tidak boleh dihapus — nonaktifkan saja.
        // Baris rencana kebutuhan (BE-PBJ-009) menambah pemeriksaannya di sini.
        if (await _dbContext.PrcPurchaseRequestLines.AnyAsync(x => x.ItemId == id && !x.IsDelete, cancellationToken))
            throw new ProcurementMasterInUseException(
                "Barang/jasa sudah dipakai pada permintaan pengadaan. Nonaktifkan saja agar tidak dipakai lagi.");

        entity.IsDelete = true;
        entity.IsActive = false;
        entity.DeleteDateTime = DateTime.UtcNow;
        entity.DeleteBy = actorUserId;
        await SaveAsync(cancellationToken);
        await AuditAsync("ProcurementItem.Delete", entity, actorUserId);
        return new ProcurementMasterDeleteResponse { Id = entity.Id, Code = entity.ItemCode, Name = entity.ItemName, IsDelete = true };
    }

    private sealed record ItemValues(
        string Name, Guid TypeId, Guid CategoryId, Guid MeasurementId, Guid? DrugId, string? Specification);

    private async Task<ItemValues> ValidateAsync(UpsertProcurementItemRequest request, Guid? excludedId, CancellationToken cancellationToken)
    {
        var name = ProcurementMasterDataSupport.Required(request.ItemName, "Nama barang/jasa", 200);
        var specification = ProcurementMasterDataSupport.Optional(request.DefaultSpecification, "Spesifikasi bawaan", 1000);

        var type = await _dbContext.MstProcurementTypes.AsNoTracking()
            .Where(x => x.Id == request.ProcurementTypeId && !x.IsDelete && x.IsActive)
            .Select(x => new { x.Id, x.IsGoods })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ProcurementMasterValidationException("Jenis pengadaan tidak ditemukan atau tidak aktif.");

        if (!await _dbContext.MstProcurementItemCategories.AnyAsync(
                x => x.Id == request.ItemCategoryId && !x.IsDelete && x.IsActive, cancellationToken))
            throw new ProcurementMasterValidationException("Kategori tidak ditemukan atau tidak aktif.");

        if (!await _dbContext.MstMeasurements.AnyAsync(
                x => x.Id == request.MeasurementId && !x.IsDelete && x.IsActive, cancellationToken))
            throw new ProcurementMasterValidationException("Satuan tidak ditemukan atau tidak aktif.");

        if (request.DrugId.HasValue)
        {
            // PBJ-VAL-072
            if (!type.IsGoods)
                throw new ProcurementMasterValidationException("Rujukan obat hanya untuk barang, bukan jasa.");
            if (!await _dbContext.MstDrugs.AnyAsync(x => x.Id == request.DrugId.Value && !x.IsDelete, cancellationToken))
                throw new ProcurementMasterValidationException("Obat yang dirujuk tidak ditemukan.");
        }

        // PBJ-VAL-070
        var lowered = name.ToLower();
        var duplicate = await _dbContext.MstProcurementItems.AsNoTracking()
            .Where(x => !x.IsDelete
                && x.Id != excludedId
                && x.ProcurementTypeId == request.ProcurementTypeId
                && x.ItemCategoryId == request.ItemCategoryId
                && x.MeasurementId == request.MeasurementId
                && x.ItemName.ToLower() == lowered)
            .Select(x => new { x.ItemCode, x.ItemName })
            .FirstOrDefaultAsync(cancellationToken);
        if (duplicate != null)
            throw new ProcurementMasterConflictException($"Barang serupa sudah ada: {duplicate.ItemCode} {duplicate.ItemName}.");

        return new ItemValues(name, request.ProcurementTypeId, request.ItemCategoryId, request.MeasurementId, request.DrugId, specification);
    }

    private static void Apply(MstProcurementItem entity, ItemValues values, bool isActive)
    {
        entity.ItemName = values.Name;
        entity.ProcurementTypeId = values.TypeId;
        entity.ItemCategoryId = values.CategoryId;
        entity.MeasurementId = values.MeasurementId;
        entity.DrugId = values.DrugId;
        entity.DefaultSpecification = values.Specification;
        entity.IsActive = isActive;
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try { await _dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException exception) when (ProcurementMasterDataSupport.IsUniqueViolation(exception))
        {
            throw new ProcurementMasterConflictException("Kode katalog bentrok dengan data lain. Silakan coba lagi.");
        }
    }

    private async Task<MstProcurementItem> FindAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.MstProcurementItems.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
        ?? throw new KeyNotFoundException("Barang/jasa tidak ditemukan atau sudah dihapus.");

    private Task AuditAsync(string action, MstProcurementItem entity, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, action, "Perubahan katalog barang/jasa Pengadaan.", new
        {
            EntityId = entity.Id,
            entity.ItemCode,
            entity.IsActive,
            ActorUserId = actorUserId
        });

    private static IQueryable<ProcurementItemResponse> Project(IQueryable<MstProcurementItem> query) =>
        query.Select(x => new ProcurementItemResponse
        {
            Id = x.Id,
            ItemCode = x.ItemCode,
            ItemName = x.ItemName,
            ProcurementTypeId = x.ProcurementTypeId,
            ProcurementTypeCode = x.ProcurementType!.TypeCode,
            ProcurementTypeName = x.ProcurementType.TypeName,
            IsGoods = x.ProcurementType.IsGoods,
            ItemCategoryId = x.ItemCategoryId,
            ItemCategoryName = x.ItemCategory!.CategoryName,
            MeasurementId = x.MeasurementId,
            MeasurementName = x.Measurement!.MeasurementName,
            IsDecimalAllowed = x.Measurement.IsDecimalAllowed,
            DrugId = x.DrugId,
            DrugName = x.Drug != null ? x.Drug.DrugName : null,
            DefaultSpecification = x.DefaultSpecification,
            IsActive = x.IsActive,
            CreateDateTime = x.CreateDateTime,
            CreateBy = x.CreateBy,
            UpdateDateTime = x.UpdateDateTime,
            UpdateBy = x.UpdateBy
        });
}
