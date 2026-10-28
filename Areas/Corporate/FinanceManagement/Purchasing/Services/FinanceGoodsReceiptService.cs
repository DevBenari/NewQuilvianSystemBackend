using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
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

    // Item yang digantikan koreksi (UpdateAsync) dihapus lunak dan tidak ikut dibaca — rincian GR
    // selalu mencerminkan isi yang berlaku, riwayat nilai lama tetap ada di basis data dan audit.
    public async Task<FinGoodsReceipt?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.FinGoodsReceipts
            .Include(x => x.Items.Where(item => !item.IsDelete))
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

    // ------------------------------------------------------------------------------------
    // Daftar GR — GET /, disaring PO/supplier/status (api-contract.md §B.2). SupplierId
    // disaring lewat join PurchaseOrder karena FinGoodsReceipt sendiri tidak punya kolom itu.
    // ------------------------------------------------------------------------------------

    public async Task<PagedResult<FinGoodsReceipt>> GetPagedAsync(GoodsReceiptQuery query, CancellationToken cancellationToken)
    {
        var q = _dbContext.FinGoodsReceipts.AsNoTracking().Where(x => !x.IsDelete);

        if (query.PurchaseOrderId.HasValue && query.PurchaseOrderId.Value != Guid.Empty)
            q = q.Where(x => x.PurchaseOrderId == query.PurchaseOrderId.Value);

        if (query.SupplierId.HasValue && query.SupplierId.Value != Guid.Empty)
            q = q.Where(x => x.PurchaseOrder!.SupplierId == query.SupplierId.Value);

        if (!string.IsNullOrWhiteSpace(query.Status))
            q = q.Where(x => x.Status == query.Status.Trim().ToUpperInvariant());

        var totalCount = await q.CountAsync(cancellationToken);

        var items = await q.OrderByDescending(x => x.CreateDateTime)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<FinGoodsReceipt>
        {
            Items = items,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalData = totalCount,
            TotalPage = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };
    }

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

                // BE-FIN-125: nomor batch hanya disimpan apa adanya (dipangkas); panjang dijaga
                // sebelum menyentuh basis data supaya galat berupa 400, bukan kegagalan simpan.
                var batchNumber = string.IsNullOrWhiteSpace(item.BatchNumber) ? null : item.BatchNumber.Trim();
                if (batchNumber is { Length: > 100 })
                    throw new PurchasingBadRequestException("Nomor batch maksimal 100 karakter.");

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
                    BatchNumber = batchNumber,
                    ExpiredDate = item.ExpiredDate,
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

            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_PURCHASE_ORDER_{goodsReceipt.PurchaseOrderId}", cancellationToken);

            // state-transition-matrix.md §B.2: GR yang sudah dirujuk FinInvoiceExchange tidak dapat dibatalkan.
            // Diperiksa SETELAH kunci PO diambil (bukan sebelum transaksi) karena FinanceInvoiceExchangeService.CreateAsync
            // mengambil kunci yang sama saat menyimpan Tukar Faktur yang merujuk GR — tanpa urutan ini pemeriksaan
            // dapat lulus tepat sebelum Tukar Faktur tersimpan, dan GR yang sudah dirujuk ikut terbatalkan.
            // Tukar Faktur yang sudah DIBATALKAN tidak lagi menahan GR, asalkan tidak menyisakan invoice
            // aktif (DRAFT/PENDING_APPROVAL/APPROVED) di bawahnya — dokumen yang masih hidup tetap
            // mengikat GR, riwayat yang sudah dibatalkan tidak.
            var linkedToInvoiceExchange = await _dbContext.FinInvoiceExchanges
                .AnyAsync(x => x.GoodsReceiptId == goodsReceiptId && !x.IsDelete
                    && (x.Status != FinInvoiceExchangeStatuses.Cancelled
                        || _dbContext.FinPurchasingInvoices.Any(i => i.InvoiceExchangeId == x.Id && !i.IsDelete
                            && i.Status != FinPurchasingInvoiceStatuses.Cancelled
                            && i.Status != FinPurchasingInvoiceStatuses.Rejected)),
                    cancellationToken);
            if (linkedToInvoiceExchange)
                throw new PurchasingValidationException("Tanda Terima Barang ini sudah menjadi Tukar Faktur dan tidak dapat dibatalkan.");

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
    // Koreksi isi GR (RECEIVED, belum dirujuk Tukar Faktur aktif). Seluruh isi diganti: baris lama
    // dihapus lunak (jejak nilai lama tetap di basis data dan audit), baris baru ditambahkan, lalu
    // akumulasi per baris PO divalidasi ulang tanpa GR ini dan status PO dihitung ulang. Kunci PO
    // yang sama dengan Create/Cancel/Tukar Faktur dipakai, sehingga koreksi tidak dapat berpapasan
    // dengan Tukar Faktur yang sedang merujuk GR ini (AC-17).
    // ------------------------------------------------------------------------------------

    public async Task<FinGoodsReceipt> UpdateAsync(
        Guid goodsReceiptId,
        Guid expectedRowVersion,
        DateOnly receivedDate,
        IReadOnlyList<GoodsReceiptItemRequest> items,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        if (items is not { Count: > 0 })
            throw new PurchasingBadRequestException("Tanda Terima Barang wajib memiliki minimal satu baris item.");

        // ID PO tidak berubah selama GR hidup, jadi boleh dibaca sebelum kunci diambil (pola yang sama
        // dengan CancelAsync). Seluruh keadaan lain dibaca ulang DI DALAM kunci.
        var purchaseOrderIdLookup = await _dbContext.FinGoodsReceipts.AsNoTracking()
            .Where(x => x.Id == goodsReceiptId && !x.IsDelete)
            .Select(x => (Guid?)x.PurchaseOrderId)
            .SingleOrDefaultAsync(cancellationToken);
        if (!purchaseOrderIdLookup.HasValue)
            throw new KeyNotFoundException("Tanda Terima Barang tidak ditemukan.");
        var purchaseOrderId = purchaseOrderIdLookup.Value;

        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_PURCHASE_ORDER_{purchaseOrderId}", cancellationToken);

            var goodsReceipt = await _dbContext.FinGoodsReceipts
                .Include(x => x.Items.Where(item => !item.IsDelete))
                .SingleOrDefaultAsync(x => x.Id == goodsReceiptId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Tanda Terima Barang tidak ditemukan.");

            EnsureCurrent(goodsReceipt.RowVersion, expectedRowVersion);

            if (goodsReceipt.Status != FinGoodsReceiptStatuses.Received)
                throw new PurchasingValidationException($"Hanya GR berstatus RECEIVED yang dapat diperbarui. Status saat ini: {goodsReceipt.Status}.");

            // Dependensi: Tukar Faktur aktif (bukan CANCELLED) yang merujuk GR ini mengikat isinya, karena
            // invoice dan utang yang lahir darinya bergantung pada kuantitas dan tanggal terima GR ini.
            var hasActiveExchange = await _dbContext.FinInvoiceExchanges
                .AnyAsync(x => x.GoodsReceiptId == goodsReceiptId && !x.IsDelete
                    && x.Status != FinInvoiceExchangeStatuses.Cancelled, cancellationToken);
            if (hasActiveExchange)
                throw new PurchasingValidationException("Tanda Terima Barang ini sudah dirujuk Tukar Faktur yang masih berlaku dan tidak dapat diperbarui. Batalkan Tukar Faktur terkait lebih dulu.");

            var purchaseOrder = await _dbContext.FinPurchaseOrders
                .Include(x => x.Items)
                .SingleOrDefaultAsync(x => x.Id == purchaseOrderId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Purchase Order tidak ditemukan.");

            if (purchaseOrder.Status is not (FinPurchaseOrderStatuses.Approved
                or FinPurchaseOrderStatuses.PartiallyReceived or FinPurchaseOrderStatuses.FullyReceived))
                throw new PurchasingValidationException($"Purchase Order berstatus {purchaseOrder.Status} tidak dapat menerima koreksi Tanda Terima Barang.");

            var poItemsById = purchaseOrder.Items.ToDictionary(x => x.Id);

            // Akumulasi seluruh GR aktif LAIN — GR ini digantikan isinya, jadi tidak ikut dihitung.
            var receivedElsewhere = await _dbContext.FinGoodsReceiptItems
                .Where(x => !x.IsDelete && x.GoodsReceipt!.PurchaseOrderId == purchaseOrderId && !x.GoodsReceipt.IsDelete
                    && x.GoodsReceipt.Status != FinGoodsReceiptStatuses.Cancelled && x.GoodsReceiptId != goodsReceiptId)
                .GroupBy(x => x.PurchaseOrderItemId)
                .Select(g => new { PurchaseOrderItemId = g.Key, Total = g.Sum(x => x.ReceivedQuantity) })
                .ToDictionaryAsync(x => x.PurchaseOrderItemId, x => x.Total, cancellationToken);

            var receivedAfter = new Dictionary<Guid, decimal>(receivedElsewhere);
            var newItems = new List<FinGoodsReceiptItem>();
            foreach (var item in items)
            {
                if (!poItemsById.TryGetValue(item.PurchaseOrderItemId, out var poItem))
                    throw new PurchasingBadRequestException($"Baris PO {item.PurchaseOrderItemId} tidak ditemukan pada Purchase Order ini.");
                if (item.ReceivedQuantity <= 0)
                    throw new PurchasingBadRequestException("Kuantitas diterima harus lebih dari nol.");

                var batchNumber = string.IsNullOrWhiteSpace(item.BatchNumber) ? null : item.BatchNumber.Trim();
                if (batchNumber is { Length: > 100 })
                    throw new PurchasingBadRequestException("Nomor batch maksimal 100 karakter.");

                var accumulated = receivedAfter.GetValueOrDefault(poItem.Id, 0m) + item.ReceivedQuantity;
                if (accumulated > poItem.Quantity)
                    throw new PurchasingValidationException("Kuantitas diterima melebihi kuantitas yang dipesan pada baris ini.");
                receivedAfter[poItem.Id] = accumulated;

                newItems.Add(new FinGoodsReceiptItem
                {
                    Id = Guid.NewGuid(),
                    GoodsReceiptId = goodsReceipt.Id,
                    PurchaseOrderItemId = poItem.Id,
                    ReceivedQuantity = item.ReceivedQuantity,
                    BatchNumber = batchNumber,
                    ExpiredDate = item.ExpiredDate,
                    Notes = string.IsNullOrWhiteSpace(item.Notes) ? null : item.Notes.Trim(),
                    CreateBy = actorUserId,
                    UpdateBy = actorUserId,
                    DeleteBy = actorUserId,
                    CancelBy = actorUserId
                });
            }

            var before = new
            {
                ReceivedDate = goodsReceipt.ReceivedDate,
                Items = goodsReceipt.Items.Select(x => new { x.PurchaseOrderItemId, x.ReceivedQuantity, x.BatchNumber, x.ExpiredDate }).ToList()
            };

            var now = DateTime.UtcNow;
            foreach (var oldItem in goodsReceipt.Items)
            {
                oldItem.IsDelete = true;
                oldItem.DeleteDateTime = now;
                oldItem.DeleteBy = actorUserId;
                oldItem.UpdateDateTime = now;
                oldItem.UpdateBy = actorUserId;
            }
            _dbContext.FinGoodsReceiptItems.AddRange(newItems);

            goodsReceipt.ReceivedDate = receivedDate;
            goodsReceipt.UpdateDateTime = now;
            goodsReceipt.UpdateBy = actorUserId;
            goodsReceipt.RowVersion = Guid.NewGuid();

            RecomputePurchaseOrderStatus(purchaseOrder, receivedAfter, actorUserId);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);

            var after = new
            {
                ReceivedDate = receivedDate,
                Items = newItems.Select(x => new { x.PurchaseOrderItemId, x.ReceivedQuantity, x.BatchNumber, x.ExpiredDate }).ToList()
            };
            await AuditAsync("Update", goodsReceipt.Id, actorUserId, new { GoodsReceiptId = goodsReceipt.Id, ActorUserId = actorUserId, Before = before, After = after });

            // Pemanggil membaca rincian terbaru lewat GetByIdAsync; koleksi Items pada entity terlacak ini
            // masih memuat baris lama yang sudah dihapus lunak, jadi tidak boleh dipetakan ke respons.
            return goodsReceipt;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await RollbackAsync(transaction);
            throw Stale(ex);
        }
        catch
        {
            await RollbackAsync(transaction);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
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

    private Task AuditAsync(string action, Guid goodsReceiptId, Guid actorUserId, object? extra = null) =>
        _loggerService.AuditAsync(LogCategory, $"FinanceGoodsReceipt.{action}",
            $"Perubahan Tanda Terima Barang dicatat. GoodsReceiptId={goodsReceiptId}",
            extra ?? new { GoodsReceiptId = goodsReceiptId, ActorUserId = actorUserId });
}

public sealed record GoodsReceiptItemRequest(
    Guid PurchaseOrderItemId, decimal ReceivedQuantity, string? Notes,
    string? BatchNumber = null, DateOnly? ExpiredDate = null);
