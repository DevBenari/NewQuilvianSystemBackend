using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Data;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Services;

/// <summary>
/// Layanan Tanda Terima Barang (BE-FIN-032, FIN-DES-037, 02-backend-architecture.md §C.1).
/// Setiap penerimaan menggerakkan status PO induknya (PARTIALLY_RECEIVED/FULLY_RECEIVED) —
/// dihitung ulang dari akumulasi seluruh GR aktif (bukan disimpan sebagai counter terpisah)
/// supaya pembatalan GR otomatis mengoreksi status PO tanpa logika pembalikan terpisah.
/// </summary>
public sealed class FinanceGoodsReceiptService
{
    private const string LogCategory = "Corporate.FinanceManagement.Purchasing";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public FinanceGoodsReceiptService(ApplicationDbContext dbContext, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    public async Task<FinGoodsReceipt?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.FinGoodsReceipts
            .Include(x => x.Items)
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

    // ------------------------------------------------------------------------------------
    // Pencatatan penerimaan (state-transition-matrix.md §B.2, FIN-VAL-104)
    // ------------------------------------------------------------------------------------

    public async Task<FinGoodsReceipt> CreateAsync(
        Guid purchaseOrderId,
        DateOnly receivedDate,
        IReadOnlyList<GoodsReceiptItemRequest> items,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        if (items is not { Count: > 0 })
            throw new PurchasingBadRequestException("Tanda Terima Barang wajib memiliki minimal satu baris item.");

        IDbContextTransaction? transaction = null;
        try
        {
            // Serializable + advisory lock per PO: mencegah dua GR berjalan bersamaan pada PO
            // yang sama melebihi sisa kuantitas baris tanpa saling mendeteksi (FIN-VAL-104).
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_PURCHASE_ORDER_{purchaseOrderId}", cancellationToken);

            var purchaseOrder = await _dbContext.FinPurchaseOrders
                .Include(x => x.Items)
                .SingleOrDefaultAsync(x => x.Id == purchaseOrderId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Purchase Order tidak ditemukan.");

            // state-transition-matrix.md §B.2: hanya PO APPROVED/PARTIALLY_RECEIVED yang boleh
            // menerima GR baru — FULLY_RECEIVED sengaja TIDAK termasuk (tidak ada sisa kuantitas).
            if (purchaseOrder.Status is not (FinPurchaseOrderStatuses.Approved or FinPurchaseOrderStatuses.PartiallyReceived))
                throw new PurchasingValidationException($"Purchase Order berstatus {purchaseOrder.Status} tidak dapat menerima Tanda Terima Barang.");

            var poItemsById = purchaseOrder.Items.ToDictionary(x => x.Id);

            // Akumulasi penerimaan sebelumnya per baris PO, dari seluruh GR aktif (belum dibatalkan).
            var receivedSoFar = await _dbContext.FinGoodsReceiptItems
                .Where(x => !x.IsDelete && x.GoodsReceipt!.PurchaseOrderId == purchaseOrderId && !x.GoodsReceipt.IsDelete
                    && x.GoodsReceipt.Status != FinGoodsReceiptStatuses.Cancelled)
                .GroupBy(x => x.PurchaseOrderItemId)
                .Select(g => new { PurchaseOrderItemId = g.Key, Total = g.Sum(x => x.ReceivedQuantity) })
                .ToDictionaryAsync(x => x.PurchaseOrderItemId, x => x.Total, cancellationToken);

            var grItems = new List<FinGoodsReceiptItem>();
            foreach (var item in items)
            {
                if (!poItemsById.TryGetValue(item.PurchaseOrderItemId, out var poItem))
                    throw new PurchasingBadRequestException($"Baris PO {item.PurchaseOrderItemId} tidak ditemukan pada Purchase Order ini.");
                if (item.ReceivedQuantity <= 0)
                    throw new PurchasingBadRequestException("Kuantitas diterima harus lebih dari nol.");

                var alreadyReceived = receivedSoFar.GetValueOrDefault(poItem.Id, 0m);
                // FIN-VAL-104: kuantitas diterima (akumulasi) tidak boleh melebihi kuantitas baris PO.
                if (alreadyReceived + item.ReceivedQuantity > poItem.Quantity)
                    throw new PurchasingValidationException("Kuantitas diterima melebihi kuantitas yang dipesan pada baris ini.");

                receivedSoFar[poItem.Id] = alreadyReceived + item.ReceivedQuantity;

                grItems.Add(new FinGoodsReceiptItem
                {
                    Id = Guid.NewGuid(),
                    PurchaseOrderItemId = poItem.Id,
                    ReceivedQuantity = item.ReceivedQuantity,
                    Notes = string.IsNullOrWhiteSpace(item.Notes) ? null : item.Notes.Trim(),
                    CreateBy = actorUserId,
                    UpdateBy = actorUserId,
                    DeleteBy = actorUserId,
                    CancelBy = actorUserId
                });
            }

            var goodsReceipt = new FinGoodsReceipt
            {
                Id = Guid.NewGuid(),
                GRNumber = GenerateGrNumber(),
                PurchaseOrderId = purchaseOrderId,
                ReceivedDate = receivedDate,
                Status = FinGoodsReceiptStatuses.Received,
                RowVersion = Guid.NewGuid(),
                CreateBy = actorUserId,
                UpdateBy = actorUserId,
                DeleteBy = actorUserId,
                CancelBy = actorUserId,
                Items = grItems
            };
            foreach (var grItem in grItems) grItem.GoodsReceiptId = goodsReceipt.Id;

            _dbContext.FinGoodsReceipts.Add(goodsReceipt);

            RecomputePurchaseOrderStatus(purchaseOrder, receivedSoFar, actorUserId);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);

            await AuditAsync("Create", goodsReceipt.Id, actorUserId);
            return goodsReceipt;
        }
        catch
        {
            await RollbackAsync(transaction);
            throw;
        }
    }

    // ------------------------------------------------------------------------------------
    // Pembatalan (RECEIVED -> CANCELLED) — status PO dihitung ulang tanpa GR yang dibatalkan
    // ------------------------------------------------------------------------------------

    public async Task<FinGoodsReceipt> CancelAsync(
        Guid goodsReceiptId,
        Guid expectedRowVersion,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            var goodsReceipt = await _dbContext.FinGoodsReceipts
                .SingleOrDefaultAsync(x => x.Id == goodsReceiptId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Tanda Terima Barang tidak ditemukan.");

            EnsureCurrent(goodsReceipt.RowVersion, expectedRowVersion);

            if (goodsReceipt.Status != FinGoodsReceiptStatuses.Received)
                throw new PurchasingValidationException($"Hanya GR berstatus RECEIVED yang dapat dibatalkan. Status saat ini: {goodsReceipt.Status}.");

            // state-transition-matrix.md §B.2: GR yang sudah dirujuk FinInvoiceExchange tidak dapat dibatalkan.
            var linkedToInvoiceExchange = await _dbContext.FinInvoiceExchanges
                .AnyAsync(x => x.GoodsReceiptId == goodsReceiptId && !x.IsDelete, cancellationToken);
            if (linkedToInvoiceExchange)
                throw new PurchasingValidationException("Tanda Terima Barang ini sudah menjadi Tukar Faktur dan tidak dapat dibatalkan.");

            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_PURCHASE_ORDER_{goodsReceipt.PurchaseOrderId}", cancellationToken);

            goodsReceipt.Status = FinGoodsReceiptStatuses.Cancelled;
            goodsReceipt.UpdateDateTime = DateTime.UtcNow;
            goodsReceipt.UpdateBy = actorUserId;
            goodsReceipt.CancelDateTime = DateTime.UtcNow;
            goodsReceipt.CancelBy = actorUserId;
            goodsReceipt.IsCancel = true;
            goodsReceipt.RowVersion = Guid.NewGuid();

            var purchaseOrder = await _dbContext.FinPurchaseOrders
                .Include(x => x.Items)
                .SingleOrDefaultAsync(x => x.Id == goodsReceipt.PurchaseOrderId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Purchase Order tidak ditemukan.");

            var receivedSoFar = await _dbContext.FinGoodsReceiptItems
                .Where(x => !x.IsDelete && x.GoodsReceipt!.PurchaseOrderId == purchaseOrder.Id && !x.GoodsReceipt.IsDelete
                    && x.GoodsReceipt.Status != FinGoodsReceiptStatuses.Cancelled && x.GoodsReceiptId != goodsReceiptId)
                .GroupBy(x => x.PurchaseOrderItemId)
                .Select(g => new { PurchaseOrderItemId = g.Key, Total = g.Sum(x => x.ReceivedQuantity) })
                .ToDictionaryAsync(x => x.PurchaseOrderItemId, x => x.Total, cancellationToken);

            RecomputePurchaseOrderStatus(purchaseOrder, receivedSoFar, actorUserId);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw Stale(ex);
            }

            await CommitAsync(transaction, cancellationToken);

            await AuditAsync("Cancel", goodsReceipt.Id, actorUserId);
            return goodsReceipt;
        }
        catch
        {
            await RollbackAsync(transaction);
            throw;
        }
    }

    // ------------------------------------------------------------------------------------
    // Helper
    // ------------------------------------------------------------------------------------

    /// <summary>
    /// Menyimpulkan status PO dari akumulasi kuantitas diterima non-dibatalkan. Dipanggil
    /// setelah GR dibuat MAUPUN dibatalkan supaya status PO selalu mencerminkan GR aktif saat
    /// itu (FIN-DES-037, state-transition-matrix.md §B.1).
    /// </summary>
    private static void RecomputePurchaseOrderStatus(
        FinPurchaseOrder purchaseOrder,
        IReadOnlyDictionary<Guid, decimal> receivedByItem,
        Guid actorUserId)
    {
        if (purchaseOrder.Status is not (FinPurchaseOrderStatuses.Approved
            or FinPurchaseOrderStatuses.PartiallyReceived or FinPurchaseOrderStatuses.FullyReceived))
            return;

        var totalOrdered = purchaseOrder.Items.Sum(x => x.Quantity);
        var totalReceived = purchaseOrder.Items.Sum(x => receivedByItem.GetValueOrDefault(x.Id, 0m));

        var newStatus = totalReceived <= 0
            ? FinPurchaseOrderStatuses.Approved
            : totalReceived >= totalOrdered
                ? FinPurchaseOrderStatuses.FullyReceived
                : FinPurchaseOrderStatuses.PartiallyReceived;

        if (newStatus == purchaseOrder.Status) return;

        purchaseOrder.Status = newStatus;
        purchaseOrder.UpdateDateTime = DateTime.UtcNow;
        purchaseOrder.UpdateBy = actorUserId;
        purchaseOrder.RowVersion = Guid.NewGuid();
    }

    private static string GenerateGrNumber()
    {
        // KNOWN ISSUE (bersama BE-FIN-029/030/032): belum memakai provider number-series atomik
        // (QBE-CODE-001..006) — lihat GeneratePoNumber pada FinancePurchaseOrderService.
        var candidate = $"GR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
        return candidate.Length <= 50 ? candidate : candidate[..50];
    }

    private static void EnsureCurrent(Guid actualRowVersion, Guid expectedRowVersion)
    {
        if (expectedRowVersion == Guid.Empty || actualRowVersion != expectedRowVersion) throw Stale();
    }

    private static PurchasingConflictException Stale(Exception? inner = null) =>
        new("Data telah berubah. Muat ulang sebelum melanjutkan.", inner);

    private async Task<IDbContextTransaction?> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        if (!_dbContext.Database.IsRelational()) return null;
        return await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
    }

    private Task AcquireLockAsync(string key, CancellationToken cancellationToken) =>
        _dbContext.Database.IsRelational()
            ? _dbContext.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(hashtext({0}));", [key], cancellationToken)
            : Task.CompletedTask;

    private static Task CommitAsync(IDbContextTransaction? transaction, CancellationToken cancellationToken) =>
        transaction is null ? Task.CompletedTask : transaction.CommitAsync(cancellationToken);

    private static Task RollbackAsync(IDbContextTransaction? transaction) =>
        transaction is null ? Task.CompletedTask : transaction.RollbackAsync(CancellationToken.None);

    private Task AuditAsync(string action, Guid goodsReceiptId, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, $"FinanceGoodsReceipt.{action}",
            $"Perubahan Tanda Terima Barang dicatat. GoodsReceiptId={goodsReceiptId}",
            new { GoodsReceiptId = goodsReceiptId, ActorUserId = actorUserId });
}

public sealed record GoodsReceiptItemRequest(Guid PurchaseOrderItemId, decimal ReceivedQuantity, string? Notes);
