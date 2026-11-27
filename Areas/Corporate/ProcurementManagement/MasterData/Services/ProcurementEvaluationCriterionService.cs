using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Services;

/// <summary>
/// CRUD master kriteria penilaian vendor per kategori (BE-PBJ-VD-002; PBJ-DEC-050). Nama unik per kategori tanpa beda
/// huruf; bobot 0 &lt; bobot ≤ 100 (<c>PBJ-VAL-113</c>, dijaga juga CHECK basis data).
/// </summary>
/// <remarks>
/// <para>
/// Total bobot kriteria aktif satu kategori tidak dipaksa 100 saat menyimpan, karena admin mengubah bobot satu per satu
/// (contoh: 40/40/20 menjadi 50/30/20 butuh dua simpanan). Ringkasan menampilkan total per kategori supaya layar
/// memperingatkan; pembuatan penilaian yang menolak bila total ≠ 100 (<c>PBJ-VAL-108</c>).
/// </para>
/// <para>
/// Nama dan bobot disalin ke nilai penilaian saat penilaian dibuat, jadi mengubah kriteria tidak mengubah nilai lama.
/// Kriteria yang sudah dipakai penilaian tidak dapat dihapus (400 <c>PBJ-VAL-071</c>); cukup dinonaktifkan.
/// </para>
/// </remarks>
public sealed class ProcurementEvaluationCriterionService
{
    private const string LogCategory = "Corporate.ProcurementManagement.MasterData";
    private const string WeightValidationCode = "PBJ-VAL-113";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public ProcurementEvaluationCriterionService(ApplicationDbContext dbContext, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    public async Task<ProcurementEvaluationCriterionFilterMetadataResponse> GetFilterMetadataAsync(CancellationToken cancellationToken) =>
        new()
        {
            SortOptions =
            [
                new() { Value = "name", Label = "Nama kriteria" },
                new() { Value = "category", Label = "Kategori" },
                new() { Value = "weightPercent", Label = "Bobot" },
                new() { Value = "isActive", Label = "Status aktif" }
            ],
            ItemCategoryOptions = await _dbContext.MstProcurementItemCategories.AsNoTracking()
                .Where(x => !x.IsDelete)
                .OrderBy(x => x.CategoryName)
                .Select(x => new ProcurementVendorCategoryOptionResponse
                {
                    Id = x.Id, Code = x.CategoryCode, Name = x.CategoryName, IsActive = x.IsActive
                })
                .ToListAsync(cancellationToken)
        };

    /// <summary>
    /// Ringkasan kriteria. Tanpa <paramref name="itemCategoryId"/>: hitungan seluruh kriteria dan total bobot aktif per
    /// kategori. Dengan <paramref name="itemCategoryId"/>: hitungan kategori itu dan <c>ActiveWeightTotal</c>-nya.
    /// </summary>
    public async Task<ProcurementEvaluationCriterionSummaryResponse> GetSummaryAsync(Guid? itemCategoryId, CancellationToken cancellationToken)
    {
        if (itemCategoryId.HasValue
            && !await _dbContext.MstProcurementItemCategories.AnyAsync(x => x.Id == itemCategoryId.Value && !x.IsDelete, cancellationToken))
            throw new KeyNotFoundException("Kategori tidak ditemukan atau sudah dihapus.");

        var query = _dbContext.MstProcurementEvaluationCriteria.AsNoTracking()
            .Where(x => !x.IsDelete && !x.ItemCategory!.IsDelete);
        if (itemCategoryId.HasValue) query = query.Where(x => x.ItemCategoryId == itemCategoryId.Value);

        var categories = await query
            .GroupBy(x => new { x.ItemCategoryId, x.ItemCategory!.CategoryCode, x.ItemCategory.CategoryName })
            .Select(g => new ProcurementEvaluationCategoryWeightResponse
            {
                ItemCategoryId = g.Key.ItemCategoryId,
                ItemCategoryCode = g.Key.CategoryCode,
                ItemCategoryName = g.Key.CategoryName,
                ActiveCriterionCount = g.Count(x => x.IsActive),
                ActiveWeightTotal = g.Where(x => x.IsActive).Sum(x => (decimal?)x.WeightPercent) ?? 0m
            })
            .OrderBy(x => x.ItemCategoryName)
            .ToListAsync(cancellationToken);
        foreach (var category in categories) category.IsComplete = category.ActiveWeightTotal == 100m;

        return new ProcurementEvaluationCriterionSummaryResponse
        {
            Total = await query.CountAsync(cancellationToken),
            Active = await query.CountAsync(x => x.IsActive, cancellationToken),
            Inactive = await query.CountAsync(x => !x.IsActive, cancellationToken),
            ActiveWeightTotal = itemCategoryId.HasValue
                ? categories.FirstOrDefault()?.ActiveWeightTotal ?? 0m
                : null,
            Categories = categories
        };
    }

    public async Task<PagedResult<ProcurementEvaluationCriterionResponse>> GetPagedAsync(
        ProcurementEvaluationCriterionQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.MstProcurementEvaluationCriteria.AsNoTracking().Where(x => !x.IsDelete);
        query = query.WhereCreatedWithin(ProcurementPeriodFilter.Resolve(request.StartDate, request.EndDate, request.CustomPeriod));
        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
        if (request.ItemCategoryId.HasValue) query = query.Where(x => x.ItemCategoryId == request.ItemCategoryId.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpper();
            query = query.Where(x => x.CriterionName.ToUpper().Contains(search) || x.ItemCategory!.CategoryName.ToUpper().Contains(search));
        }

        var descending = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "category" => descending
                ? query.OrderByDescending(x => x.ItemCategory!.CategoryName).ThenBy(x => x.CriterionName)
                : query.OrderBy(x => x.ItemCategory!.CategoryName).ThenBy(x => x.CriterionName),
            "weightpercent" => descending
                ? query.OrderByDescending(x => x.WeightPercent).ThenBy(x => x.CriterionName)
                : query.OrderBy(x => x.WeightPercent).ThenBy(x => x.CriterionName),
            "isactive" => descending
                ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.CriterionName)
                : query.OrderBy(x => x.IsActive).ThenBy(x => x.CriterionName),
            _ => descending ? query.OrderByDescending(x => x.CriterionName) : query.OrderBy(x => x.CriterionName)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await Project(query.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProcurementEvaluationCriterionResponse>
        {
            PageNumber = request.PageNumber, PageSize = request.PageSize, TotalData = total,
            TotalPage = ProcurementMasterDataSupport.TotalPage(total, request.PageSize), Items = items
        };
    }

    public async Task<List<ProcurementEvaluationCriterionOptionResponse>> GetOptionsAsync(
        bool onlyActive, Guid? itemCategoryId, string? search, CancellationToken cancellationToken)
    {
        var query = _dbContext.MstProcurementEvaluationCriteria.AsNoTracking().Where(x => !x.IsDelete);
        if (onlyActive) query = query.Where(x => x.IsActive);
        if (itemCategoryId.HasValue) query = query.Where(x => x.ItemCategoryId == itemCategoryId.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim().ToUpper();
            query = query.Where(x => x.CriterionName.ToUpper().Contains(keyword));
        }

        return await query
            .OrderBy(x => x.CriterionName)
            .Select(x => new ProcurementEvaluationCriterionOptionResponse
            {
                Id = x.Id, ItemCategoryId = x.ItemCategoryId, Name = x.CriterionName,
                WeightPercent = x.WeightPercent, IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ProcurementEvaluationCriterionResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await Project(_dbContext.MstProcurementEvaluationCriteria.AsNoTracking().Where(x => x.Id == id && !x.IsDelete))
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw new KeyNotFoundException("Kriteria penilaian tidak ditemukan atau sudah dihapus.");

    public async Task<ProcurementEvaluationCriterionResponse> CreateAsync(
        UpsertProcurementEvaluationCriterionRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var (name, description) = await ValidateAsync(request, null, cancellationToken);
        var entity = new MstProcurementEvaluationCriterion
        {
            ItemCategoryId = request.ItemCategoryId, CriterionName = name, WeightPercent = request.WeightPercent,
            Description = description, IsActive = request.IsActive, CreateDateTime = DateTime.UtcNow, CreateBy = actorUserId
        };
        _dbContext.MstProcurementEvaluationCriteria.Add(entity);
        await SaveAsync(cancellationToken);
        await AuditAsync("ProcurementEvaluationCriterion.Create", entity, actorUserId);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<ProcurementEvaluationCriterionResponse> UpdateAsync(
        Guid id, UpsertProcurementEvaluationCriterionRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        var (name, description) = await ValidateAsync(request, id, cancellationToken);
        if (entity.ItemCategoryId != request.ItemCategoryId
            && await _dbContext.PrcVendorEvaluationScores.AnyAsync(x => x.CriterionId == id && !x.IsDelete, cancellationToken))
            throw new ProcurementMasterConflictException(
                "Kriteria ini sudah dipakai penilaian; kategorinya tidak dapat dipindah. Nonaktifkan kriteria ini lalu buat kriteria baru di kategori tujuan.");

        entity.ItemCategoryId = request.ItemCategoryId;
        entity.CriterionName = name;
        entity.WeightPercent = request.WeightPercent;
        entity.Description = description;
        entity.IsActive = request.IsActive;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;
        await SaveAsync(cancellationToken);
        await AuditAsync("ProcurementEvaluationCriterion.Update", entity, actorUserId);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<ProcurementEvaluationCriterionResponse> UpdateStatusAsync(
        Guid id, UpdateProcurementMasterStatusRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        entity.IsActive = request.IsActive;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;
        await SaveAsync(cancellationToken);
        await AuditAsync(request.IsActive ? "ProcurementEvaluationCriterion.Activate" : "ProcurementEvaluationCriterion.Deactivate", entity, actorUserId);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<ProcurementMasterDeleteResponse> DeleteAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (await _dbContext.PrcVendorEvaluationScores.AnyAsync(x => x.CriterionId == id && !x.IsDelete, cancellationToken))
            throw new ProcurementMasterInUseException(
                "Kriteria ini tidak dapat dihapus karena sudah dipakai penilaian vendor. Nonaktifkan saja bila tidak dipakai lagi.");

        entity.IsDelete = true;
        entity.IsActive = false;
        entity.DeleteDateTime = DateTime.UtcNow;
        entity.DeleteBy = actorUserId;
        await SaveAsync(cancellationToken);
        await AuditAsync("ProcurementEvaluationCriterion.Delete", entity, actorUserId);
        return new ProcurementMasterDeleteResponse { Id = entity.Id, Code = string.Empty, Name = entity.CriterionName, IsDelete = true };
    }

    private async Task<(string Name, string? Description)> ValidateAsync(
        UpsertProcurementEvaluationCriterionRequest request, Guid? excludedId, CancellationToken cancellationToken)
    {
        if (request.ItemCategoryId == Guid.Empty
            || !await _dbContext.MstProcurementItemCategories.AnyAsync(x => x.Id == request.ItemCategoryId && !x.IsDelete, cancellationToken))
            throw new ProcurementMasterValidationException("Kategori wajib dipilih dari kategori Pengadaan yang ada.");
        var name = ProcurementMasterDataSupport.Required(request.Name, "Nama kriteria", 150);
        var description = ProcurementMasterDataSupport.Optional(request.Description, "Deskripsi", 500);
        if (request.WeightPercent <= 0m || request.WeightPercent > 100m)
            throw new ProcurementMasterValidationException("Bobot harus lebih dari 0 dan paling tinggi 100.", WeightValidationCode);
        if (decimal.Round(request.WeightPercent, 2) != request.WeightPercent)
            throw new ProcurementMasterValidationException("Bobot paling banyak dua angka di belakang koma.");

        var upperName = name.ToUpper();
        if (await _dbContext.MstProcurementEvaluationCriteria.AnyAsync(
                x => !x.IsDelete && x.Id != excludedId && x.ItemCategoryId == request.ItemCategoryId && x.CriterionName.ToUpper() == upperName,
                cancellationToken))
            throw new ProcurementMasterConflictException("Nama kriteria sudah dipakai di kategori ini. Gunakan nama yang berbeda.");
        return (name, description);
    }

    private static IQueryable<ProcurementEvaluationCriterionResponse> Project(IQueryable<MstProcurementEvaluationCriterion> query) =>
        query.Select(x => new ProcurementEvaluationCriterionResponse
        {
            Id = x.Id, ItemCategoryId = x.ItemCategoryId, ItemCategoryCode = x.ItemCategory!.CategoryCode,
            ItemCategoryName = x.ItemCategory.CategoryName, Name = x.CriterionName, WeightPercent = x.WeightPercent,
            Description = x.Description, IsActive = x.IsActive,
            CreateDateTime = x.CreateDateTime, CreateBy = x.CreateBy, UpdateDateTime = x.UpdateDateTime, UpdateBy = x.UpdateBy
        });

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try { await _dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException exception) when (ProcurementMasterDataSupport.IsUniqueViolation(exception))
        {
            throw new ProcurementMasterConflictException("Nama kriteria sudah dipakai di kategori ini. Gunakan nama yang berbeda.");
        }
    }

    private async Task<MstProcurementEvaluationCriterion> FindAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.MstProcurementEvaluationCriteria.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
        ?? throw new KeyNotFoundException("Kriteria penilaian tidak ditemukan atau sudah dihapus.");

    private Task AuditAsync(string action, MstProcurementEvaluationCriterion entity, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, action, "Perubahan master kriteria penilaian vendor.", new
        {
            EntityId = entity.Id,
            entity.ItemCategoryId,
            entity.WeightPercent,
            entity.IsActive,
            ActorUserId = actorUserId
        });
}
