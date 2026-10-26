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
/// Layanan Tukar Faktur (BE-FIN-033, FIN-DEC-051, FR-FIN-082, 02-backend-architecture.md §C.1).
/// Checkpoint serah-terima dokumen fisik faktur dari supplier — terpisah dari kapan barang
/// diterima (FinGoodsReceipt.ReceivedDate) maupun kapan PO dibuat. EstimatedDueDate SELALU
/// dihitung dari ReceivedDate milik Tukar Faktur ini sendiri, bukan tanggal dokumen lain.
/// Pola struktur meniru FinancePurchaseOrderService/FinanceGoodsReceiptService (BE-FIN-032)
/// persis — exception dipakai bersama lewat PurchasingExceptions.cs.
/// </summary>
public sealed class FinanceInvoiceExchangeService
{
    private const string LogCategory = "Corporate.FinanceManagement.Purchasing";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public FinanceInvoiceExchangeService(ApplicationDbContext dbContext, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    public async Task<FinInvoiceExchange?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.FinInvoiceExchanges
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

    // ------------------------------------------------------------------------------------
    // Daftar Tukar Faktur — GET /, disaring supplier/status (api-contract.md §B.3)
    // ------------------------------------------------------------------------------------

    public async Task<PagedResult<FinInvoiceExchange>> GetPagedAsync(InvoiceExchangeQuery query, CancellationToken cancellationToken)
    {
        var q = _dbContext.FinInvoiceExchanges.AsNoTracking().Where(x => !x.IsDelete);

        if (query.SupplierId.HasValue && query.SupplierId.Value != Guid.Empty)
            q = q.Where(x => x.SupplierId == query.SupplierId.Value);

        if (!string.IsNullOrWhiteSpace(query.Status))
            q = q.Where(x => x.Status == query.Status.Trim().ToUpperInvariant());

        var totalCount = await q.CountAsync(cancellationToken);

        var items = await q.OrderByDescending(x => x.CreateDateTime)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<FinInvoiceExchange>
        {
            Items = items,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalData = totalCount,
            TotalPage = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };
    }

    // ------------------------------------------------------------------------------------
    // Pencatatan (state-transition-matrix.md §B.3, FIN-DEC-051, FR-FIN-082)
    // ------------------------------------------------------------------------------------

    public async Task<FinInvoiceExchange> CreateAsync(
        Guid supplierId,
        Guid? purchaseOrderId,
        Guid? goodsReceiptId,
        string supplierInvoiceNumber,
        DateOnly supplierInvoiceDate,
        DateOnly receivedDate,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var supplier = await _dbContext.MstSuppliers.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == supplierId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Supplier tidak ditemukan.");
        if (!supplier.IsActive)
            throw new PurchasingValidationException("Supplier sedang tidak aktif.");

        var invoiceNumber = ValidateText(supplierInvoiceNumber, "Nomor faktur supplier", 100);

        // FIN-DEC-051: PO dan GR boleh kosong — Tukar Faktur boleh berdiri sendiri.
        // referencedSupplierId menampung supplier PO yang dirujuk (langsung, atau lewat GR) supaya
        // dapat dibandingkan dengan supplier Tukar Faktur ini (AP-UAT-09). Tukar Faktur tanpa PO
        // dan tanpa GR tidak punya pembanding, jadi tidak ditolak.
        Guid? referencedSupplierId = null;

        if (purchaseOrderId.HasValue)
        {
            referencedSupplierId = await _dbContext.FinPurchaseOrders.AsNoTracking()
                .Where(x => x.Id == purchaseOrderId.Value && !x.IsDelete)
                .Select(x => (Guid?)x.SupplierId)
                .SingleOrDefaultAsync(cancellationToken)
                ?? throw new KeyNotFoundException("Purchase Order yang dirujuk tidak ditemukan.");
        }

        FinGoodsReceipt? goodsReceipt = null;
        if (goodsReceiptId.HasValue)
        {
            goodsReceipt = await _dbContext.FinGoodsReceipts.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == goodsReceiptId.Value && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Tanda Terima Barang yang dirujuk tidak ditemukan.");

            // Konsistensi data dasar: bila keduanya diisi, GR MUST milik PO yang sama.
            if (purchaseOrderId.HasValue && goodsReceipt.PurchaseOrderId != purchaseOrderId.Value)
                throw new PurchasingBadRequestException("Tanda Terima Barang yang dirujuk bukan milik Purchase Order yang dirujuk.");

            // Hanya GR tanpa PO eksplisit yang perlu dicari supplier PO-nya; bila PO sudah diberikan,
            // GR di atas sudah dipastikan milik PO itu sehingga supplier-nya sama.
            if (referencedSupplierId is null)
            {
                var goodsReceiptPurchaseOrderId = goodsReceipt.PurchaseOrderId;
                referencedSupplierId = await _dbContext.FinPurchaseOrders.AsNoTracking()
                    .Where(x => x.Id == goodsReceiptPurchaseOrderId && !x.IsDelete)
                    .Select(x => (Guid?)x.SupplierId)
                    .SingleOrDefaultAsync(cancellationToken)
                    ?? throw new KeyNotFoundException("Purchase Order milik Tanda Terima Barang yang dirujuk tidak ditemukan.");
            }
        }

        // AP-UAT-09: supplier Tukar Faktur harus sama dengan supplier PO/GR yang dirujuk. Tanpa ini
        // faktur supplier A dapat ditempelkan ke penerimaan supplier B, dan utangnya tetap terbentuk
        // atas nama supplier A.
        if (referencedSupplierId.HasValue && referencedSupplierId.Value != supplierId)
            throw new PurchasingValidationException("Supplier Tukar Faktur harus sama dengan supplier Purchase Order yang dirujuk.");

        // FR-FIN-082/FIN-DEC-051: EstimatedDueDate dihitung backend dari ReceivedDate Tukar
        // Faktur ini SENDIRI + PaymentTermDays supplier — bukan dari tanggal PO/GR, dan tidak
        // pernah diambil dari nilai kiriman client (CreateInvoiceExchangeRequest sengaja tidak
        // punya field EstimatedDueDate sama sekali).
        var estimatedDueDate = receivedDate.AddDays(supplier.PaymentTermDays);

        var invoiceExchange = new FinInvoiceExchange
        {
            Id = Guid.NewGuid(),
            ExchangeNumber = GenerateExchangeNumber(),
            SupplierId = supplierId,
            PurchaseOrderId = purchaseOrderId,
            GoodsReceiptId = goodsReceiptId,
            SupplierInvoiceNumber = invoiceNumber,
            SupplierInvoiceDate = supplierInvoiceDate,
            ReceivedDate = receivedDate,
            EstimatedDueDate = estimatedDueDate,
            Status = FinInvoiceExchangeStatuses.Received,
            RowVersion = Guid.NewGuid(),
            CreateBy = actorUserId,
            UpdateBy = actorUserId,
            DeleteBy = actorUserId,
            CancelBy = actorUserId
        };

        // Pasangan kunci dengan FinanceGoodsReceiptService.CancelAsync: keduanya mengambil kunci PO
        // yang sama, sehingga GR tidak dapat dibatalkan tepat ketika Tukar Faktur yang merujuknya
        // sedang disimpan (dan sebaliknya). Pemeriksaan status GR dibaca ulang DI DALAM kunci.
        IDbContextTransaction? transaction = null;
        try
        {
            if (goodsReceipt is not null)
            {
                var lockedGoodsReceiptId = goodsReceipt.Id;
                var lockedPurchaseOrderId = goodsReceipt.PurchaseOrderId;

                transaction = await BeginTransactionAsync(cancellationToken);
                await AcquireLockAsync($"FIN_PURCHASE_ORDER_{lockedPurchaseOrderId}", cancellationToken);

                var currentGoodsReceiptStatus = await _dbContext.FinGoodsReceipts.AsNoTracking()
                    .Where(x => x.Id == lockedGoodsReceiptId && !x.IsDelete)
                    .Select(x => x.Status)
                    .SingleOrDefaultAsync(cancellationToken);
                if (currentGoodsReceiptStatus != FinGoodsReceiptStatuses.Received)
                    throw new PurchasingValidationException("Tanda Terima Barang yang dirujuk sudah dibatalkan dan tidak dapat dijadikan dasar Tukar Faktur.");
            }

            _dbContext.FinInvoiceExchanges.Add(invoiceExchange);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
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

        await AuditAsync("Create", invoiceExchange.Id, actorUserId);
        return invoiceExchange;
    }

    // ------------------------------------------------------------------------------------
    // Pembatalan (RECEIVED -> CANCELLED) — state-transition-matrix.md §B.3
    // ------------------------------------------------------------------------------------

    public async Task<FinInvoiceExchange> CancelAsync(
        Guid invoiceExchangeId,
        Guid expectedRowVersion,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var invoiceExchange = await _dbContext.FinInvoiceExchanges
            .SingleOrDefaultAsync(x => x.Id == invoiceExchangeId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Tukar Faktur tidak ditemukan.");

        EnsureCurrent(invoiceExchange.RowVersion, expectedRowVersion);

        // LINKED_TO_INVOICE dan CANCELLED adalah status akhir (§B.3) — pesan dikhususkan untuk
        // LINKED_TO_INVOICE karena itu satu-satunya acceptance criteria eksplisit BE-FIN-033.
        if (invoiceExchange.Status == FinInvoiceExchangeStatuses.LinkedToInvoice)
            throw new PurchasingValidationException("Tukar Faktur ini sudah menjadi Purchasing Invoice dan tidak dapat dibatalkan.");
        if (invoiceExchange.Status != FinInvoiceExchangeStatuses.Received)
            throw new PurchasingValidationException($"Hanya Tukar Faktur berstatus RECEIVED yang dapat dibatalkan. Status saat ini: {invoiceExchange.Status}.");

        invoiceExchange.Status = FinInvoiceExchangeStatuses.Cancelled;
        invoiceExchange.UpdateDateTime = DateTime.UtcNow;
        invoiceExchange.UpdateBy = actorUserId;
        invoiceExchange.CancelDateTime = DateTime.UtcNow;
        invoiceExchange.CancelBy = actorUserId;
        invoiceExchange.IsCancel = true;
        invoiceExchange.RowVersion = Guid.NewGuid();

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw Stale(ex);
        }

        await AuditAsync("Cancel", invoiceExchange.Id, actorUserId);
        return invoiceExchange;
    }

    // ------------------------------------------------------------------------------------
    // Helper
    // ------------------------------------------------------------------------------------

    private static string ValidateText(string? value, string label, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new PurchasingBadRequestException($"{label} wajib diisi.");
        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new PurchasingBadRequestException($"{label} maksimal {maxLength} karakter.");
        return trimmed;
    }

    private static string GenerateExchangeNumber()
    {
        // KNOWN ISSUE (bersama BE-FIN-029/030/032): belum memakai provider number-series atomik
        // (QBE-CODE-001..006) — pola identik GeneratePoNumber/GenerateGrNumber.
        var candidate = $"TF-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
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

    private Task AuditAsync(string action, Guid invoiceExchangeId, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, $"FinanceInvoiceExchange.{action}",
            $"Perubahan Tukar Faktur dicatat. InvoiceExchangeId={invoiceExchangeId}",
            new { InvoiceExchangeId = invoiceExchangeId, ActorUserId = actorUserId });
}
