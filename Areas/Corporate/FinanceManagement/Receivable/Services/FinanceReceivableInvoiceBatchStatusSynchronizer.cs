using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;

/// <summary>
/// Layanan sinkronisasi status Batch Tagihan AR (FinReceivableInvoiceBatch) berdasarkan kondisi aktual
/// FinReceivable anggotanya setelah alokasi penerimaan (AllocateAsync) atau pembalikan (ReverseAllocationAsync).
/// Layanan ini independen dan TIDAK memanggil FinanceReceiptService untuk mencegah circular dependency.
/// </summary>
public sealed class FinanceReceivableInvoiceBatchStatusSynchronizer
{
    private readonly ApplicationDbContext _dbContext;

    public FinanceReceivableInvoiceBatchStatusSynchronizer(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Menyelaraskan status FinReceivableInvoiceBatch untuk seluruh piutang yang terpengaruh mutasi alokasi.
    /// Dipanggil di dalam transaksi database yang sama dengan AllocateAsync/ReverseAllocationAsync.
    /// </summary>
    public async Task SynchronizeByReceivableIdsAsync(
        IEnumerable<Guid> receivableIds,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var distinctReceivableIds = receivableIds.Distinct().ToList();
        if (distinctReceivableIds.Count == 0) return;

        // Cari active membership FinReceivableInvoiceBatchItem
        var batchIds = await _dbContext.FinReceivableInvoiceBatchItems
            .Where(item => distinctReceivableIds.Contains(item.ReceivableId) && !item.IsDelete && item.IsActiveMembership)
            .Select(item => item.BatchId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (batchIds.Count == 0) return;

        // Load seluruh batch terdampak beserta seluruh anggota aktifnya
        var batches = await _dbContext.FinReceivableInvoiceBatches
            .Include(b => b.Items.Where(i => !i.IsDelete && i.IsActiveMembership))
                .ThenInclude(i => i.Receivable)
            .Where(b => batchIds.Contains(b.Id) && !b.IsDelete)
            .ToListAsync(cancellationToken);

        foreach (var batch in batches)
        {
            SynchronizeBatch(batch, actorUserId);
        }
    }

    /// <summary>
    /// Menghitung ulang status satu batch dan menormalisasi legacy DRAFT jika sudah mempunyai invoice number.
    /// Return true jika ada perubahan status/metadata.
    /// </summary>
    public bool SynchronizeBatch(FinReceivableInvoiceBatch batch, Guid actorUserId)
    {
        // RULE 4: CANCELLED jangan otomatis diubah
        if (batch.Status == FinReceivableInvoiceBatchStatuses.Cancelled)
            return false;

        var activeReceivables = batch.Items
            .Where(i => !i.IsDelete && i.IsActiveMembership && i.Receivable != null && !i.Receivable.IsDelete)
            .Select(i => i.Receivable!)
            .ToList();

        if (activeReceivables.Count == 0) return false;

        var totalOutstanding = activeReceivables.Sum(r => r.OutstandingAmount);
        var totalOriginal = activeReceivables.Sum(r => r.OriginalAmount);
        var totalAllocated = activeReceivables.Sum(r => r.AllocatedAmount);
        var hasActivePayments = totalAllocated > 0 || totalOutstanding < totalOriginal;

        string targetStatus;
        // RULE 1: Jika seluruh OutstandingAmount <= 0
        if (activeReceivables.All(r => r.OutstandingAmount <= 0) && totalOriginal > 0)
        {
            targetStatus = FinReceivableInvoiceBatchStatuses.Paid;
        }
        // RULE 2: Jika masih ada OutstandingAmount > 0 tetapi sudah ada settlement/payment aktif
        else if (hasActivePayments)
        {
            targetStatus = FinReceivableInvoiceBatchStatuses.PartiallyPaid;
        }
        // RULE 3: Jika OutstandingAmount > 0 dan belum ada payment aktif
        else
        {
            targetStatus = FinReceivableInvoiceBatchStatuses.Issued;
        }

        bool changed = false;

        // Normalisasi IssuedAt jika batch belum memiliki IssuedAt (misal legacy DRAFT yang sudah punya nomor invoice)
        if (batch.IssuedAt == null)
        {
            DateTimeOffset effectiveIssuedAt;
            if (batch.InvoiceDate.HasValue)
            {
                effectiveIssuedAt = new DateTimeOffset(batch.InvoiceDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
            }
            else
            {
                effectiveIssuedAt = new DateTimeOffset(batch.CreateDateTime, TimeSpan.Zero);
            }
            batch.IssuedAt = effectiveIssuedAt;
            changed = true;
        }

        if (batch.Status != targetStatus)
        {
            batch.Status = targetStatus;
            changed = true;
        }

        if (changed)
        {
            batch.UpdateDateTime = DateTime.UtcNow;
            batch.UpdateBy = actorUserId;
            batch.RowVersion = Guid.NewGuid();
        }

        return changed;
    }
}
