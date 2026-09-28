using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Services;

/// <summary>
/// Layanan Purchase Order (BE-FIN-032, FIN-DES-037, FIN-DEC-050, 02-backend-architecture.md
/// §C.1). Pola maker-checker dan struktur method meniru FinancePaymentService (BE-FIN-020)
/// persis — lihat PurchasingExceptions.cs untuk alasan keluarga exception dipakai bersama
/// dengan FinanceGoodsReceiptService.
/// </summary>
public sealed class FinancePurchaseOrderService
{
    private const string LogCategory = "Corporate.FinanceManagement.Purchasing";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;
    private readonly FinanceApprovalAuthorizationService _approvalAuthorizationService;

    public FinancePurchaseOrderService(
        ApplicationDbContext dbContext,
        LoggerService loggerService,
        FinanceApprovalAuthorizationService approvalAuthorizationService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
        _approvalAuthorizationService = approvalAuthorizationService;
    }

    // ------------------------------------------------------------------------------------
    // 1. Rincian
    // ------------------------------------------------------------------------------------

    public async Task<FinPurchaseOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.FinPurchaseOrders
            .Include(x => x.Items)
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

    // ------------------------------------------------------------------------------------
    // 2. Pembuatan Draft PO (FIN-VAL-100, state-transition-matrix.md §B.1)
    // ------------------------------------------------------------------------------------

    public async Task<FinPurchaseOrder> CreateAsync(
        Guid supplierId,
        IReadOnlyList<PurchaseOrderItemRequest> items,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        // FIN-VAL-100: PO wajib minimal satu baris item.
        if (items is not { Count: > 0 })
            throw new PurchasingBadRequestException("Purchase Order wajib memiliki minimal satu baris item.");

        var supplier = await _dbContext.MstSuppliers.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == supplierId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Supplier tidak ditemukan.");
        if (!supplier.IsActive)
            throw new PurchasingValidationException("Supplier sedang tidak aktif.");

        var (poItems, totalAmount) = BuildItems(items, actorUserId);

        var purchaseOrder = new FinPurchaseOrder
        {
            Id = Guid.NewGuid(),
            PONumber = GeneratePoNumber(),
            SupplierId = supplierId,
            Status = FinPurchaseOrderStatuses.Draft,
            TotalAmount = totalAmount,
            ApprovalTier = FinanceApprovalTierResolver.Resolve(totalAmount),
            RequestedByUserId = actorUserId,
            RequestedAt = DateTimeOffset.UtcNow,
            RowVersion = Guid.NewGuid(),
            CreateBy = actorUserId,
            UpdateBy = actorUserId,
            DeleteBy = actorUserId,
            CancelBy = actorUserId,
            Items = poItems
        };
        foreach (var item in poItems) item.PurchaseOrderId = purchaseOrder.Id;

        _dbContext.FinPurchaseOrders.Add(purchaseOrder);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await AuditAsync("Create", purchaseOrder.Id, actorUserId);
        return purchaseOrder;
    }

    // ------------------------------------------------------------------------------------
    // 3. Perubahan Rincian PO (DRAFT saja) — pola replace-wholesale FinancePaymentService
    // ------------------------------------------------------------------------------------

    public async Task<FinPurchaseOrder> UpdateAsync(
        Guid purchaseOrderId,
        Guid expectedRowVersion,
        IReadOnlyList<PurchaseOrderItemRequest> items,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var purchaseOrder = await _dbContext.FinPurchaseOrders
            .Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.Id == purchaseOrderId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Purchase Order tidak ditemukan.");

        EnsureCurrent(purchaseOrder.RowVersion, expectedRowVersion);

        if (purchaseOrder.Status != FinPurchaseOrderStatuses.Draft)
            throw new PurchasingValidationException($"Hanya PO berstatus DRAFT yang dapat diubah. Status saat ini: {purchaseOrder.Status}.");

        if (items is not { Count: > 0 })
            throw new PurchasingBadRequestException("Purchase Order wajib memiliki minimal satu baris item.");

        _dbContext.FinPurchaseOrderItems.RemoveRange(purchaseOrder.Items);

        var (poItems, totalAmount) = BuildItems(items, actorUserId);
        foreach (var item in poItems) item.PurchaseOrderId = purchaseOrder.Id;

        purchaseOrder.Items = poItems;
        purchaseOrder.TotalAmount = totalAmount;
        purchaseOrder.ApprovalTier = FinanceApprovalTierResolver.Resolve(totalAmount);
        purchaseOrder.UpdateDateTime = DateTime.UtcNow;
        purchaseOrder.UpdateBy = actorUserId;
        purchaseOrder.RowVersion = Guid.NewGuid();

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw Stale(ex);
        }

        await AuditAsync("Update", purchaseOrder.Id, actorUserId);
        return purchaseOrder;
    }

    // ------------------------------------------------------------------------------------
    // 4. Pengajuan (DRAFT -> PENDING_APPROVAL) — ApprovalTier dihitung ulang di sini
    //    persis sesuai state-transition-matrix.md §B.1 ("Ajukan ... ApprovalTier dihitung
    //    dari TotalAmount"), terlepas dari nilai yang sudah dihitung saat Create/Update.
    // ------------------------------------------------------------------------------------

    public async Task<FinPurchaseOrder> SubmitAsync(
        Guid purchaseOrderId,
        Guid expectedRowVersion,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var purchaseOrder = await _dbContext.FinPurchaseOrders
            .SingleOrDefaultAsync(x => x.Id == purchaseOrderId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Purchase Order tidak ditemukan.");

        EnsureCurrent(purchaseOrder.RowVersion, expectedRowVersion);

        if (purchaseOrder.Status != FinPurchaseOrderStatuses.Draft)
            throw new PurchasingValidationException($"Hanya PO berstatus DRAFT yang dapat diajukan. Status saat ini: {purchaseOrder.Status}.");

        purchaseOrder.ApprovalTier = FinanceApprovalTierResolver.Resolve(purchaseOrder.TotalAmount);
        purchaseOrder.Status = FinPurchaseOrderStatuses.PendingApproval;
        purchaseOrder.UpdateDateTime = DateTime.UtcNow;
        purchaseOrder.UpdateBy = actorUserId;
        purchaseOrder.RowVersion = Guid.NewGuid();

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw Stale(ex);
        }

        await AuditAsync("Submit", purchaseOrder.Id, actorUserId);
        return purchaseOrder;
    }

    // ------------------------------------------------------------------------------------
    // 5. Persetujuan (PENDING_APPROVAL -> APPROVED) — FIN-VAL-101 (422, self-approval),
    //    FIN-VAL-102 (403, jenjang nominal)
    // ------------------------------------------------------------------------------------

    public async Task<FinPurchaseOrder> ApproveAsync(
        Guid purchaseOrderId,
        Guid expectedRowVersion,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var purchaseOrder = await _dbContext.FinPurchaseOrders
            .SingleOrDefaultAsync(x => x.Id == purchaseOrderId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Purchase Order tidak ditemukan.");

        EnsureCurrent(purchaseOrder.RowVersion, expectedRowVersion);

        if (purchaseOrder.Status != FinPurchaseOrderStatuses.PendingApproval)
            throw new PurchasingValidationException($"Hanya PO berstatus PENDING_APPROVAL yang dapat disetujui. Status saat ini: {purchaseOrder.Status}.");

        // FIN-VAL-101: pengaju tidak boleh menyetujui PO permohonannya sendiri.
        if (actorUserId == purchaseOrder.RequestedByUserId)
            throw new PurchasingValidationException("Pengaju tidak boleh menyetujui Purchase Order permohonannya sendiri.");

        // FIN-VAL-102: penyetuju harus sesuai jenjang nominal (ambang Rp 50.000.000, FIN-DEC-052).
        var canApprove = await _approvalAuthorizationService.CanApproveAsync(actorUserId, purchaseOrder.ApprovalTier, cancellationToken);
        if (!canApprove)
            throw new PurchasingForbiddenException("Nominal Purchase Order ini memerlukan persetujuan Manajer Finance.");

        purchaseOrder.Status = FinPurchaseOrderStatuses.Approved;
        purchaseOrder.ApprovedByUserId = actorUserId;
        purchaseOrder.ApprovedAt = DateTimeOffset.UtcNow;
        purchaseOrder.UpdateDateTime = DateTime.UtcNow;
        purchaseOrder.UpdateBy = actorUserId;
        purchaseOrder.RowVersion = Guid.NewGuid();

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw Stale(ex);
        }

        await AuditAsync("Approve", purchaseOrder.Id, actorUserId);
        return purchaseOrder;
    }

    // ------------------------------------------------------------------------------------
    // 6. Penolakan (PENDING_APPROVAL -> REJECTED)
    // ------------------------------------------------------------------------------------

    public async Task<FinPurchaseOrder> RejectAsync(
        Guid purchaseOrderId,
        Guid expectedRowVersion,
        string rejectionReason,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var purchaseOrder = await _dbContext.FinPurchaseOrders
            .SingleOrDefaultAsync(x => x.Id == purchaseOrderId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Purchase Order tidak ditemukan.");

        EnsureCurrent(purchaseOrder.RowVersion, expectedRowVersion);

        if (purchaseOrder.Status != FinPurchaseOrderStatuses.PendingApproval)
            throw new PurchasingValidationException($"Hanya PO berstatus PENDING_APPROVAL yang dapat ditolak. Status saat ini: {purchaseOrder.Status}.");

        var reason = ValidateText(rejectionReason, "Alasan penolakan", maxLength: 500);

        purchaseOrder.Status = FinPurchaseOrderStatuses.Rejected;
        purchaseOrder.ApprovedByUserId = actorUserId;
        purchaseOrder.ApprovedAt = DateTimeOffset.UtcNow;
        purchaseOrder.UpdateDateTime = DateTime.UtcNow;
        purchaseOrder.UpdateBy = actorUserId;
        purchaseOrder.RowVersion = Guid.NewGuid();
        _ = reason; // FinPurchaseOrder tidak punya kolom RejectionReason pada bentuk BE-FIN-029 — dicatat lewat audit log saja (lihat KNOWN ISSUES laporan).

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw Stale(ex);
        }

        await AuditAsync("Reject", purchaseOrder.Id, actorUserId, new { Reason = reason });
        return purchaseOrder;
    }

    // ------------------------------------------------------------------------------------
    // 7. Pembatalan (DRAFT/PENDING_APPROVAL -> CANCELLED) — FIN-VAL-103
    // ------------------------------------------------------------------------------------

    public async Task<FinPurchaseOrder> CancelAsync(
        Guid purchaseOrderId,
        Guid expectedRowVersion,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var purchaseOrder = await _dbContext.FinPurchaseOrders
            .SingleOrDefaultAsync(x => x.Id == purchaseOrderId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Purchase Order tidak ditemukan.");

        EnsureCurrent(purchaseOrder.RowVersion, expectedRowVersion);

        if (purchaseOrder.Status is not (FinPurchaseOrderStatuses.Draft or FinPurchaseOrderStatuses.PendingApproval))
            throw new PurchasingValidationException($"PO berstatus {purchaseOrder.Status} tidak dapat dibatalkan.");

        // FIN-VAL-103: PO yang sudah ada penerimaan barang tidak boleh dibatalkan. Secara alur
        // normal GR hanya tercatat setelah APPROVED, sehingga baris ini murni pengaman berlapis
        // (defense-in-depth) — bukan jalur yang seharusnya pernah tercapai.
        var hasGoodsReceipt = await _dbContext.FinGoodsReceipts
            .AnyAsync(x => x.PurchaseOrderId == purchaseOrderId && !x.IsDelete, cancellationToken);
        if (hasGoodsReceipt)
            throw new PurchasingValidationException("Purchase Order ini sudah memiliki penerimaan barang dan tidak dapat dibatalkan.");

        purchaseOrder.Status = FinPurchaseOrderStatuses.Cancelled;
        purchaseOrder.UpdateDateTime = DateTime.UtcNow;
        purchaseOrder.UpdateBy = actorUserId;
        purchaseOrder.CancelDateTime = DateTime.UtcNow;
        purchaseOrder.CancelBy = actorUserId;
        purchaseOrder.IsCancel = true;
        purchaseOrder.RowVersion = Guid.NewGuid();

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw Stale(ex);
        }

        await AuditAsync("Cancel", purchaseOrder.Id, actorUserId);
        return purchaseOrder;
    }

    // ------------------------------------------------------------------------------------
    // Helper
    // ------------------------------------------------------------------------------------

    private static (List<FinPurchaseOrderItem> Items, decimal TotalAmount) BuildItems(
        IReadOnlyList<PurchaseOrderItemRequest> items,
        Guid actorUserId)
    {
        var poItems = new List<FinPurchaseOrderItem>();
        decimal totalAmount = 0m;

        foreach (var item in items)
        {
            var category = ValidateText(item.ProductCategory, "Kategori produk", 100);
            var name = ValidateText(item.ProductName, "Nama produk", 300);
            var unit = ValidateText(item.Unit, "Satuan", 30);
            if (item.Quantity <= 0)
                throw new PurchasingBadRequestException("Kuantitas baris PO harus lebih dari nol.");
            if (item.UnitPrice < 0)
                throw new PurchasingBadRequestException("Harga satuan baris PO tidak boleh negatif.");

            var lineTotal = item.Quantity * item.UnitPrice;
            totalAmount += lineTotal;

            poItems.Add(new FinPurchaseOrderItem
            {
                Id = Guid.NewGuid(),
                ProductCategory = category,
                ProductName = name,
                Unit = unit,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                LineTotal = lineTotal,
                CreateBy = actorUserId,
                UpdateBy = actorUserId,
                DeleteBy = actorUserId,
                CancelBy = actorUserId
            });
        }

        return (poItems, totalAmount);
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

    private static string GeneratePoNumber()
    {
        // KNOWN ISSUE (bersama BE-FIN-029/030): belum memakai provider number-series atomik
        // (QBE-CODE-001..006). Pola GUID ini identik dengan GeneratePaymentNumber milik
        // FinancePaymentService — bukan Count/Max+1, jadi tidak melanggar QBE-CODE, tapi juga
        // bukan format nomor bisnis yang mudah dibaca manusia. Menyusul provider bersama.
        var candidate = $"PO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
        return candidate.Length <= 50 ? candidate : candidate[..50];
    }

    private static void EnsureCurrent(Guid actualRowVersion, Guid expectedRowVersion)
    {
        if (expectedRowVersion == Guid.Empty || actualRowVersion != expectedRowVersion) throw Stale();
    }

    private static PurchasingConflictException Stale(Exception? inner = null) =>
        new("Data telah berubah. Muat ulang sebelum melanjutkan.", inner);

    private Task AuditAsync(string action, Guid purchaseOrderId, Guid actorUserId, object? extra = null) =>
        _loggerService.AuditAsync(LogCategory, $"FinancePurchaseOrder.{action}",
            $"Perubahan Purchase Order dicatat. PurchaseOrderId={purchaseOrderId}",
            extra ?? new { PurchaseOrderId = purchaseOrderId, ActorUserId = actorUserId });
}

public sealed record PurchaseOrderItemRequest(string ProductCategory, string ProductName, string Unit, decimal Quantity, decimal UnitPrice);
