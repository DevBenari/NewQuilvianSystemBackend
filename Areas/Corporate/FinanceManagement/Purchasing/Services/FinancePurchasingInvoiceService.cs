using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Data;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Services;

/// <summary>
/// Layanan Purchasing Invoice (BE-FIN-034, FIN-DEC-045, 046, 053; 02-backend-architecture.md
/// §C.1, D). Titik paling sensitif rumpun Purchasing/AP: menyetujui invoice ini SATU-SATUNYA
/// jalur yang membuat FinSupplierPayable bersumber Purchasing (lewat FinanceSupplierPayableService
/// yang sudah berjalan, BE-FIN-019 — TIDAK ditulis ulang) sekaligus menulis kejadian PPN Masukan.
/// Pola maker-checker meniru FinancePurchaseOrderService (BE-FIN-032) persis.
/// </summary>
public sealed class FinancePurchasingInvoiceService
{
    private const string LogCategory = "Corporate.FinanceManagement.Purchasing";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;
    private readonly FinanceApprovalAuthorizationService _approvalAuthorizationService;
    private readonly FinanceSupplierPayableService _supplierPayableService;
    private readonly FinanceAccountingOutboxService _accountingOutboxService;

    public FinancePurchasingInvoiceService(
        ApplicationDbContext dbContext,
        LoggerService loggerService,
        FinanceApprovalAuthorizationService approvalAuthorizationService,
        FinanceSupplierPayableService supplierPayableService,
        FinanceAccountingOutboxService accountingOutboxService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
        _approvalAuthorizationService = approvalAuthorizationService;
        _supplierPayableService = supplierPayableService;
        _accountingOutboxService = accountingOutboxService;
    }

    public async Task<FinPurchasingInvoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.FinPurchasingInvoices
            .Include(x => x.Items)
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

    // ------------------------------------------------------------------------------------
    // Pembuatan Draft (FIN-VAL-105, FIN-VAL-106, FIN-VAL-107, state-transition-matrix.md §B.4)
    // ------------------------------------------------------------------------------------

    public async Task<FinPurchasingInvoice> CreateAsync(
        Guid invoiceExchangeId,
        decimal discountAmount, decimal ppnAmount, decimal downPaymentAmount, decimal otherDeductionAmount, decimal totalAmount,
        IReadOnlyList<PurchasingInvoiceItemRequest> items,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var invoiceExchange = await _dbContext.FinInvoiceExchanges.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == invoiceExchangeId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Tukar Faktur tidak ditemukan.");

        // FIN-VAL-106: Tukar Faktur harus RECEIVED (bukan sudah LINKED_TO_INVOICE/CANCELLED).
        if (invoiceExchange.Status != FinInvoiceExchangeStatuses.Received)
            throw new PurchasingValidationException("Tukar Faktur ini sudah dibatalkan atau sudah punya invoice.");

        // FIN-VAL-105: satu Tukar Faktur menghasilkan tepat satu Purchasing Invoice. Pengecekan
        // eksplisit ini melengkapi (bukan menggantikan) unique index InvoiceExchangeId — race
        // murni antara dua permintaan hampir bersamaan tetap ditangkap constraint database.
        var alreadyUsed = await _dbContext.FinPurchasingInvoices
            .AnyAsync(x => x.InvoiceExchangeId == invoiceExchangeId && !x.IsDelete, cancellationToken);
        if (alreadyUsed)
            throw new PurchasingConflictException("Tukar Faktur ini sudah memiliki Purchasing Invoice.");

        var (invoiceItems, subtotalAmount) = BuildItems(items, actorUserId);

        // Bukan FIN-VAL bernomor — pola sama seperti FIN-VAL-100 milik Purchase Order (BE-FIN-032),
        // diterapkan lagi di sini demi konsistensi ("SubtotalAmount" tidak bermakna tanpa baris).
        if (invoiceItems.Count == 0)
            throw new PurchasingBadRequestException("Purchasing Invoice wajib memiliki minimal satu baris item.");

        ValidateBalance(subtotalAmount, discountAmount, ppnAmount, downPaymentAmount, otherDeductionAmount, totalAmount);

        var purchasingInvoice = new FinPurchasingInvoice
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = GenerateInvoiceNumber(),
            InvoiceExchangeId = invoiceExchangeId,
            SupplierId = invoiceExchange.SupplierId,
            SubtotalAmount = subtotalAmount,
            DiscountAmount = discountAmount,
            PPNAmount = ppnAmount,
            DownPaymentAmount = downPaymentAmount,
            OtherDeductionAmount = otherDeductionAmount,
            TotalAmount = totalAmount,
            Status = FinPurchasingInvoiceStatuses.Draft,
            ApprovalTier = FinanceApprovalTierResolver.Resolve(totalAmount),
            RequestedByUserId = actorUserId,
            RequestedAt = DateTimeOffset.UtcNow,
            RowVersion = Guid.NewGuid(),
            CreateBy = actorUserId,
            UpdateBy = actorUserId,
            DeleteBy = actorUserId,
            CancelBy = actorUserId,
            Items = invoiceItems
        };
        foreach (var item in invoiceItems) item.PurchasingInvoiceId = purchasingInvoice.Id;

        _dbContext.FinPurchasingInvoices.Add(purchasingInvoice);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await AuditAsync("Create", purchasingInvoice.Id, actorUserId);
        return purchasingInvoice;
    }

    // ------------------------------------------------------------------------------------
    // Perubahan Rincian (DRAFT saja) — pola replace-wholesale FinancePurchaseOrderService
    // ------------------------------------------------------------------------------------

    public async Task<FinPurchasingInvoice> UpdateAsync(
        Guid purchasingInvoiceId, Guid expectedRowVersion,
        decimal discountAmount, decimal ppnAmount, decimal downPaymentAmount, decimal otherDeductionAmount, decimal totalAmount,
        IReadOnlyList<PurchasingInvoiceItemRequest> items,
        Guid actorUserId, CancellationToken cancellationToken)
    {
        var purchasingInvoice = await _dbContext.FinPurchasingInvoices
            .Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.Id == purchasingInvoiceId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Purchasing Invoice tidak ditemukan.");

        EnsureCurrent(purchasingInvoice.RowVersion, expectedRowVersion);

        if (purchasingInvoice.Status != FinPurchasingInvoiceStatuses.Draft)
            throw new PurchasingValidationException($"Hanya Purchasing Invoice berstatus DRAFT yang dapat diubah. Status saat ini: {purchasingInvoice.Status}.");

        var (invoiceItems, subtotalAmount) = BuildItems(items, actorUserId);
        if (invoiceItems.Count == 0)
            throw new PurchasingBadRequestException("Purchasing Invoice wajib memiliki minimal satu baris item.");

        ValidateBalance(subtotalAmount, discountAmount, ppnAmount, downPaymentAmount, otherDeductionAmount, totalAmount);

        _dbContext.FinPurchasingInvoiceItems.RemoveRange(purchasingInvoice.Items);
        foreach (var item in invoiceItems) item.PurchasingInvoiceId = purchasingInvoice.Id;

        purchasingInvoice.Items = invoiceItems;
        purchasingInvoice.SubtotalAmount = subtotalAmount;
        purchasingInvoice.DiscountAmount = discountAmount;
        purchasingInvoice.PPNAmount = ppnAmount;
        purchasingInvoice.DownPaymentAmount = downPaymentAmount;
        purchasingInvoice.OtherDeductionAmount = otherDeductionAmount;
        purchasingInvoice.TotalAmount = totalAmount;
        purchasingInvoice.ApprovalTier = FinanceApprovalTierResolver.Resolve(totalAmount);
        purchasingInvoice.UpdateDateTime = DateTime.UtcNow;
        purchasingInvoice.UpdateBy = actorUserId;
        purchasingInvoice.RowVersion = Guid.NewGuid();

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw Stale(ex);
        }

        await AuditAsync("Update", purchasingInvoice.Id, actorUserId);
        return purchasingInvoice;
    }

    // ------------------------------------------------------------------------------------
    // Pengajuan (DRAFT -> PENDING_APPROVAL) — ApprovalTier dihitung ulang, pola persis PO
    // ------------------------------------------------------------------------------------

    public async Task<FinPurchasingInvoice> SubmitAsync(
        Guid purchasingInvoiceId, Guid expectedRowVersion, Guid actorUserId, CancellationToken cancellationToken)
    {
        var purchasingInvoice = await _dbContext.FinPurchasingInvoices
            .SingleOrDefaultAsync(x => x.Id == purchasingInvoiceId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Purchasing Invoice tidak ditemukan.");

        EnsureCurrent(purchasingInvoice.RowVersion, expectedRowVersion);

        if (purchasingInvoice.Status != FinPurchasingInvoiceStatuses.Draft)
            throw new PurchasingValidationException($"Hanya Purchasing Invoice berstatus DRAFT yang dapat diajukan. Status saat ini: {purchasingInvoice.Status}.");

        purchasingInvoice.ApprovalTier = FinanceApprovalTierResolver.Resolve(purchasingInvoice.TotalAmount);
        purchasingInvoice.Status = FinPurchasingInvoiceStatuses.PendingApproval;
        purchasingInvoice.UpdateDateTime = DateTime.UtcNow;
        purchasingInvoice.UpdateBy = actorUserId;
        purchasingInvoice.RowVersion = Guid.NewGuid();

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw Stale(ex);
        }

        await AuditAsync("Submit", purchasingInvoice.Id, actorUserId);
        return purchasingInvoice;
    }

    // ------------------------------------------------------------------------------------
    // Persetujuan (PENDING_APPROVAL -> APPROVED) — FIN-VAL-108 (422), FIN-VAL-109 (403).
    // SATU TRANSAKSI (state-transition-matrix.md §B.4): FinSupplierPayable + Tukar Faktur
    // LINKED_TO_INVOICE + outbox PPN-MASUKAN-PEMBELIAN. Gagal di mana pun -> status TETAP
    // PENDING_APPROVAL (rollback penuh).
    // ------------------------------------------------------------------------------------

    public async Task<FinPurchasingInvoice> ApproveAsync(
        Guid purchasingInvoiceId, Guid expectedRowVersion, Guid actorUserId, CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_PURCHASING_INVOICE_{purchasingInvoiceId:N}", cancellationToken);

            var purchasingInvoice = await _dbContext.FinPurchasingInvoices
                .SingleOrDefaultAsync(x => x.Id == purchasingInvoiceId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Purchasing Invoice tidak ditemukan.");

            EnsureCurrent(purchasingInvoice.RowVersion, expectedRowVersion);

            if (purchasingInvoice.Status != FinPurchasingInvoiceStatuses.PendingApproval)
                throw new PurchasingValidationException($"Hanya Purchasing Invoice berstatus PENDING_APPROVAL yang dapat disetujui. Status saat ini: {purchasingInvoice.Status}.");

            // FIN-VAL-108: pengaju tidak boleh menyetujui permohonannya sendiri.
            if (actorUserId == purchasingInvoice.RequestedByUserId)
                throw new PurchasingValidationException("Pengaju tidak boleh menyetujui Purchasing Invoice permohonannya sendiri.");

            // FIN-VAL-109: penyetuju harus sesuai jenjang nominal (ambang sama dengan PO, FIN-DEC-052).
            var canApprove = await _approvalAuthorizationService.CanApproveAsync(actorUserId, purchasingInvoice.ApprovalTier, cancellationToken);
            if (!canApprove)
                throw new PurchasingForbiddenException("Nominal Purchasing Invoice ini memerlukan persetujuan Manajer Finance.");

            var invoiceExchange = await _dbContext.FinInvoiceExchanges
                .SingleOrDefaultAsync(x => x.Id == purchasingInvoice.InvoiceExchangeId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Tukar Faktur sumber tidak ditemukan.");

            // Pengaman berlapis: seharusnya mustahil berubah sejak Create (FIN-VAL-105/106 sudah
            // menutup jalurnya), tapi diperiksa ulang di dalam transaksi sebelum menulis apa pun.
            if (invoiceExchange.Status != FinInvoiceExchangeStatuses.Received)
                throw new PurchasingValidationException("Tukar Faktur sumber tidak lagi berstatus RECEIVED — Purchasing Invoice ini tidak dapat disetujui.");

            // FIN-DEC-045/046, FIN-DEC-053: FinanceSupplierPayableService (BE-FIN-019, SUDAH
            // BERJALAN) tetap SATU-SATUNYA pembuat FinSupplierPayable. Satu item sintetis
            // (Quantity=1, UnitPrice=TotalAmount) supaya OriginalAmount ledger = TotalAmount PENUH
            // (bukan Subtotal pra-diskon/PPN) TANPA mengubah invariant "OriginalAmount = jumlah
            // item" milik service itu — lihat ringkasan kelas CreateAsync di sana.
            var payableItems = new List<SupplierPayableItemRequest>
            {
                new($"Purchasing Invoice {purchasingInvoice.InvoiceNumber}", 1m, purchasingInvoice.TotalAmount)
            };

            var payable = await _supplierPayableService.CreateAsync(
                purchasingInvoice.SupplierId,
                invoiceExchange.SupplierInvoiceNumber,
                invoiceExchange.SupplierInvoiceDate,
                $"Utang dari Purchasing Invoice {purchasingInvoice.InvoiceNumber}",
                payableItems,
                actorUserId,
                cancellationToken,
                sourcePurchasingInvoiceId: purchasingInvoice.Id,
                accountingEventAmountOverride: purchasingInvoice.TotalAmount - purchasingInvoice.PPNAmount);

            // FIN-DEC-046, integration-contract.md §5.8: Amount = PPN SAJA, bukan nilai invoice
            // penuh. Dilewati bila PPNAmount = 0 — kejadian bernilai nol bukan fakta yang perlu
            // diakui (keputusan implementasi, kontrak tidak secara eksplisit membahas kasus ini).
            if (purchasingInvoice.PPNAmount > 0m)
            {
                await _accountingOutboxService.StageEventAsync(new AccountingOutboxEventRequest
                {
                    EventTypeCode = FinAccountingEventTypeCodes.PpnMasukanPembelian,
                    SourceTransactionId = purchasingInvoice.InvoiceNumber,
                    EventOccurredAt = DateTimeOffset.UtcNow,
                    AccountingDate = invoiceExchange.SupplierInvoiceDate,
                    Amount = purchasingInvoice.PPNAmount,
                    CorrelationId = purchasingInvoice.Id,
                    CausationId = payable.Id,
                    ActorUserId = actorUserId
                }, cancellationToken);
            }

            invoiceExchange.Status = FinInvoiceExchangeStatuses.LinkedToInvoice;
            invoiceExchange.UpdateDateTime = DateTime.UtcNow;
            invoiceExchange.UpdateBy = actorUserId;
            invoiceExchange.RowVersion = Guid.NewGuid();

            purchasingInvoice.Status = FinPurchasingInvoiceStatuses.Approved;
            purchasingInvoice.ApprovedByUserId = actorUserId;
            purchasingInvoice.ApprovedAt = DateTimeOffset.UtcNow;
            purchasingInvoice.UpdateDateTime = DateTime.UtcNow;
            purchasingInvoice.UpdateBy = actorUserId;
            purchasingInvoice.RowVersion = Guid.NewGuid();

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);

            await AuditAsync("Approve", purchasingInvoice.Id, actorUserId, new { PayableId = payable.Id });
            return purchasingInvoice;
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
    // Penolakan (PENDING_APPROVAL -> REJECTED) — alasan wajib, 400 bila kosong (bukan 422,
    // persis literal state-transition-matrix.md §B.4)
    // ------------------------------------------------------------------------------------

    public async Task<FinPurchasingInvoice> RejectAsync(
        Guid purchasingInvoiceId, Guid expectedRowVersion, string rejectionReason, Guid actorUserId, CancellationToken cancellationToken)
    {
        var purchasingInvoice = await _dbContext.FinPurchasingInvoices
            .SingleOrDefaultAsync(x => x.Id == purchasingInvoiceId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Purchasing Invoice tidak ditemukan.");

        EnsureCurrent(purchasingInvoice.RowVersion, expectedRowVersion);

        if (purchasingInvoice.Status != FinPurchasingInvoiceStatuses.PendingApproval)
            throw new PurchasingValidationException($"Hanya Purchasing Invoice berstatus PENDING_APPROVAL yang dapat ditolak. Status saat ini: {purchasingInvoice.Status}.");

        var reason = ValidateText(rejectionReason, "Alasan penolakan", maxLength: 500);

        purchasingInvoice.Status = FinPurchasingInvoiceStatuses.Rejected;
        purchasingInvoice.ApprovedByUserId = actorUserId;
        purchasingInvoice.ApprovedAt = DateTimeOffset.UtcNow;
        purchasingInvoice.UpdateDateTime = DateTime.UtcNow;
        purchasingInvoice.UpdateBy = actorUserId;
        purchasingInvoice.RowVersion = Guid.NewGuid();

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw Stale(ex);
        }

        // FinPurchasingInvoice tidak punya kolom RejectionReason (BE-FIN-029) — dicatat audit
        // log saja, pola sama seperti FinPurchaseOrder (BE-FIN-032), lihat KNOWN ISSUES laporan.
        await AuditAsync("Reject", purchasingInvoice.Id, actorUserId, new { Reason = reason });
        return purchasingInvoice;
    }

    // ------------------------------------------------------------------------------------
    // Helper
    // ------------------------------------------------------------------------------------

    private static (List<FinPurchasingInvoiceItem> Items, decimal SubtotalAmount) BuildItems(
        IReadOnlyList<PurchasingInvoiceItemRequest> items, Guid actorUserId)
    {
        var invoiceItems = new List<FinPurchasingInvoiceItem>();
        decimal subtotalAmount = 0m;

        foreach (var item in items ?? [])
        {
            var name = ValidateText(item.ProductName, "Nama produk", 300);
            if (item.Quantity <= 0)
                throw new PurchasingBadRequestException("Kuantitas baris invoice harus lebih dari nol.");
            if (item.UnitPrice < 0)
                throw new PurchasingBadRequestException("Harga satuan baris invoice tidak boleh negatif.");

            var lineTotal = item.Quantity * item.UnitPrice;
            subtotalAmount += lineTotal;

            invoiceItems.Add(new FinPurchasingInvoiceItem
            {
                Id = Guid.NewGuid(),
                ProductName = name,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                LineTotal = lineTotal,
                CreateBy = actorUserId,
                UpdateBy = actorUserId,
                DeleteBy = actorUserId,
                CancelBy = actorUserId
            });
        }

        return (invoiceItems, subtotalAmount);
    }

    /// <summary>FIN-VAL-107 — dibandingkan lewat pembulatan 2 desimal supaya sisa pembagian
    /// floating point pada perhitungan sisi klien tidak ditolak sebagai "tidak seimbang".</summary>
    private static void ValidateBalance(
        decimal subtotalAmount, decimal discountAmount, decimal ppnAmount, decimal downPaymentAmount, decimal otherDeductionAmount, decimal totalAmount)
    {
        var expected = subtotalAmount - discountAmount + ppnAmount - downPaymentAmount - otherDeductionAmount;
        if (Math.Round(expected, 2) != Math.Round(totalAmount, 2))
            throw new PurchasingValidationException("Rincian nilai invoice tidak seimbang dengan totalnya.");
    }

    private static string ValidateText(string? value, string label, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new PurchasingBadRequestException($"{label} wajib diisi.");
        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new PurchasingBadRequestException($"{label} maksimal {maxLength} karakter.");
        return trimmed;
    }

    private static string GenerateInvoiceNumber()
    {
        // KNOWN ISSUE (bersama BE-FIN-029/030/032/033): belum memakai provider number-series
        // atomik (QBE-CODE-001..006).
        var candidate = $"PI-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
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

    private Task AuditAsync(string action, Guid purchasingInvoiceId, Guid actorUserId, object? extra = null) =>
        _loggerService.AuditAsync(LogCategory, $"FinancePurchasingInvoice.{action}",
            $"Perubahan Purchasing Invoice dicatat. PurchasingInvoiceId={purchasingInvoiceId}",
            extra ?? new { PurchasingInvoiceId = purchasingInvoiceId, ActorUserId = actorUserId });
}

public sealed record PurchasingInvoiceItemRequest(string ProductName, decimal Quantity, decimal UnitPrice);
