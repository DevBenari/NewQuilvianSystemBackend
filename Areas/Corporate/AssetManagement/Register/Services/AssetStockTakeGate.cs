namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

public sealed class AssetStockTakeGate
{
    public Task<IReadOnlyCollection<Guid>> FindUnitsInStockTakeAsync(IEnumerable<Guid> organizationUnitIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(organizationUnitIds);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyCollection<Guid>>(Array.Empty<Guid>());
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
