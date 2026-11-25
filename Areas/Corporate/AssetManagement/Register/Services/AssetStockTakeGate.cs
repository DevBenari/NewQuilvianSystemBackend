using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

public sealed class AssetStockTakeGate
{
    public static readonly StockTakeStatus[] HoldingStatuses =
        [StockTakeStatus.Berjalan, StockTakeStatus.Pencocokan, StockTakeStatus.MenungguVerifikasi];

    private readonly ApplicationDbContext _context;

    public AssetStockTakeGate(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IReadOnlyCollection<Guid>> FindUnitsInStockTakeAsync(IEnumerable<Guid> organizationUnitIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(organizationUnitIds);
        var ids = organizationUnitIds.Where(x => x != Guid.Empty).Distinct().ToList();
        if (ids.Count == 0)
        {
            return Array.Empty<Guid>();
        }

        return await _context.AstStockTakeSessions
            .AsNoTracking()
            .Where(x => !x.IsDelete && ids.Contains(x.OrganizationUnitId) && HoldingStatuses.Contains(x.Status))
            .Select(x => x.OrganizationUnitId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsUnitUnderStockTakeAsync(Guid organizationUnitId, CancellationToken cancellationToken = default) =>
        (await FindUnitsInStockTakeAsync([organizationUnitId], cancellationToken)).Count > 0;

    public async Task<string?> FindUnitNameInStockTakeAsync(IEnumerable<Guid> organizationUnitIds, CancellationToken cancellationToken = default)
    {
        var busy = await FindUnitsInStockTakeAsync(organizationUnitIds, cancellationToken);
        if (busy.Count == 0)
        {
            return null;
        }

        var first = busy.First();
        return await _context.MstOrganizationUnits
            .AsNoTracking()
            .Where(x => x.Id == first)
            .Select(x => x.UnitName)
            .FirstOrDefaultAsync(cancellationToken) ?? "-";
    }

    public async Task EnsureUnitsNotInStockTakeAsync(IEnumerable<Guid> organizationUnitIds, CancellationToken cancellationToken = default)
    {
        var busy = await FindUnitsInStockTakeAsync(organizationUnitIds, cancellationToken);
        if (busy.Count > 0)
        {
            throw AssetManagementException.Conflict(AssetRegisterErrorCodes.UnitInStockTake, AssetRegisterMessages.UnitInStockTake);
        }
    }
}
