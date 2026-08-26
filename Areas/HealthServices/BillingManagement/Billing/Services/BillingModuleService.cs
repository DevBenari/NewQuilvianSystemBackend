using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

/// <summary>
/// Titik masuk service untuk capability transaksi Billing.
/// Operasi bisnis ditambahkan per vertical slice agar controller baru tidak
/// mengorkestrasi <see cref="ApplicationDbContext"/> secara langsung.
/// </summary>
public sealed class BillingModuleService
{
    public BillingModuleService(ApplicationDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
    }
}
