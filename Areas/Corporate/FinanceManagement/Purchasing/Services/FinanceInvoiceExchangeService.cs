using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

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
        if (purchaseOrderId.HasValue)
        {
            var purchaseOrderExists = await _dbContext.FinPurchaseOrders
                .AnyAsync(x => x.Id == purchaseOrderId.Value && !x.IsDelete, cancellationToken);
            if (!purchaseOrderExists)
                throw new KeyNotFoundException("Purchase Order yang dirujuk tidak ditemukan.");
        }

        if (goodsReceiptId.HasValue)
        {
            var goodsReceipt = await _dbContext.FinGoodsReceipts.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == goodsReceiptId.Value && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Tanda Terima Barang yang dirujuk tidak ditemukan.");

            // Konsistensi data dasar: bila keduanya diisi, GR MUST milik PO yang sama.
            if (purchaseOrderId.HasValue && goodsReceipt.PurchaseOrderId != purchaseOrderId.Value)
                throw new PurchasingBadRequestException("Tanda Terima Barang yang dirujuk bukan milik Purchase Order yang dirujuk.");
        }

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

        _dbContext.FinInvoiceExchanges.Add(invoiceExchange);
        await _dbContext.SaveChangesAsync(cancellationToken);

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

    private Task AuditAsync(string action, Guid invoiceExchangeId, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, $"FinanceInvoiceExchange.{action}",
            $"Perubahan Tukar Faktur dicatat. InvoiceExchangeId={invoiceExchangeId}",
            new { InvoiceExchangeId = invoiceExchangeId, ActorUserId = actorUserId });
}
