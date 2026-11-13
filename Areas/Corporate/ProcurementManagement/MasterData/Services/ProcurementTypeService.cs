using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.DTOs;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Services;

/// <summary>
/// Jenis pengadaan — hanya baca (PBJ-DEC-024, PBJ-DEC-037). Isinya berasal dari data awal
/// <c>ProcurementMasterDataSeeder</c>, karena jenis menentukan alur serah terima (barang lewat GR
/// BLM, jasa lewat BAST) dan tidak boleh diubah dari layar.
/// </summary>
public sealed class ProcurementTypeService
{
    private readonly ApplicationDbContext _dbContext;

    public ProcurementTypeService(ApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<PagedResult<ProcurementTypeResponse>> GetPagedAsync(ProcurementReferenceQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.MstProcurementTypes.AsNoTracking().Where(x => !x.IsDelete);
        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpper();
            query = query.Where(x => x.TypeCode.ToUpper().Contains(search) || x.TypeName.ToUpper().Contains(search));
        }

        var descending = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() == "code"
            ? (descending ? query.OrderByDescending(x => x.TypeCode) : query.OrderBy(x => x.TypeCode))
            : (descending ? query.OrderByDescending(x => x.TypeName) : query.OrderBy(x => x.TypeName));

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .Select(x => new ProcurementTypeResponse
            {
                Id = x.Id, TypeCode = x.TypeCode, TypeName = x.TypeName, IsGoods = x.IsGoods,
                Description = x.Description, IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ProcurementTypeResponse>
        {
            PageNumber = request.PageNumber, PageSize = request.PageSize, TotalData = total,
            TotalPage = ProcurementMasterDataSupport.TotalPage(total, request.PageSize), Items = items
        };
    }

    public Task<List<ProcurementTypeResponse>> GetOptionsAsync(bool onlyActive, CancellationToken cancellationToken)
    {
        var query = _dbContext.MstProcurementTypes.AsNoTracking().Where(x => !x.IsDelete);
        if (onlyActive) query = query.Where(x => x.IsActive);
        return query
            .OrderBy(x => x.TypeName)
            .Select(x => new ProcurementTypeResponse
            {
                Id = x.Id, TypeCode = x.TypeCode, TypeName = x.TypeName, IsGoods = x.IsGoods,
                Description = x.Description, IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ProcurementTypeResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.MstProcurementTypes.AsNoTracking()
            .Where(x => x.Id == id && !x.IsDelete)
            .Select(x => new ProcurementTypeResponse
            {
                Id = x.Id, TypeCode = x.TypeCode, TypeName = x.TypeName, IsGoods = x.IsGoods,
                Description = x.Description, IsActive = x.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw new KeyNotFoundException("Jenis pengadaan tidak ditemukan atau sudah dihapus.");
}
