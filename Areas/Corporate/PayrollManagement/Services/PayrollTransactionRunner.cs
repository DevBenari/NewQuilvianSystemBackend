using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Repositories;
using System.Data;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// Satu transaksi Serializable dengan advisory lock per kunci; rollback penuh bila pekerjaan gagal. Polanya sama persis dengan
/// <c>PayrollRunService.InTransactionAsync</c>. Kunci run <see cref="RunLockKey"/> HARUS identik dengan yang dipakai
/// PayrollRunService agar aksi run dan aksi pembayaran/slip pada run yang sama saling berurutan, bukan berbarengan.
/// </summary>
internal static class PayrollTransactionRunner
{
    /// <summary>Kunci advisory lock per payroll run (sama dengan PayrollRunService).</summary>
    public static string RunLockKey(Guid runId) => $"HR_PAYROLL_RUN_{runId:N}";

    public static async Task<T> RunAsync<T>(
        ApplicationDbContext dbContext, string lockKey, Func<Task<T>> work, CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            if (dbContext.Database.IsRelational())
            {
                transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
                await dbContext.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(hashtext({0}));", [lockKey], cancellationToken);
            }

            var result = await work();
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }
}
