using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;

/// <summary>Core existing ikut transaksi coordinator; hanya pemilik lease boleh commit.</summary>
internal sealed class InpBedTransactionLease : IAsyncDisposable
{
    private readonly IDbContextTransaction? owned;
    private InpBedTransactionLease(IDbContextTransaction? transaction) { owned = transaction; }
    public static async Task<InpBedTransactionLease> BeginAsync(ApplicationDbContext db, CancellationToken ct) =>
        new(db.Database.CurrentTransaction == null ? await db.Database.BeginTransactionAsync(ct) : null);
    public Task CommitAsync(CancellationToken ct = default) => owned?.CommitAsync(ct) ?? Task.CompletedTask;
    public Task RollbackAsync(CancellationToken ct = default) => owned?.RollbackAsync(ct) ?? Task.CompletedTask;
    public ValueTask DisposeAsync() => owned?.DisposeAsync() ?? ValueTask.CompletedTask;
}

