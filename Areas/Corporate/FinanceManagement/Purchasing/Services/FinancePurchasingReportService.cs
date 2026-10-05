using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Services;

/// <summary>
/// Empat laporan Purchasing/AP read-only dari data yang sudah ada (BE-FIN-037,
/// FIN-API-1.1 §B.6, FIN-DES-044, FIN-DEC-059). Nol tabel laporan baru — seluruh
/// proyeksi berasal dari entity BE-FIN-029/030 (FinPurchaseOrder, FinGoodsReceipt,
/// FinInvoiceExchange, FinPurchasingInvoice) dan FinSupplierPayable (BE-FIN-019/034).
///
/// /aging SENGAJA tidak ada: FIN-DEC-059 mencabut endpoint ini dari kontrak karena
/// GET api/finance/payable/aging (FinanceApController) sudah menghitung umur
/// FinSupplierPayable — termasuk yang bersumber Purchasing Invoice — sehingga endpoint
/// kedua akan menghasilkan dua angka umur utang yang bisa berbeda.
///
/// Nol perintah pengubah pada service ini. AsNoTracking seluruh query.
/// </summary>
public sealed class FinancePurchasingReportService
{
    private readonly ApplicationDbContext _dbContext;

    public FinancePurchasingReportService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // ──────────────────────────────────────────────────────────────────────────────────
    // GET /purchasing/reports/summary  (FIN-API-1.1 §B.6)
    // ──────────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Rekap Purchasing/AP periodik. Seluruh filter nullable; query tanpa filter mengembalikan
    /// agregat seluruh data aktif. SupplierId mempersempit kelompok yang relevan.
    /// FinGoodsReceipt tidak memiliki SupplierId langsung — filternya lewat PurchaseOrder.
    /// </summary>
    public async Task<PurchasingSummaryResponse> GetSummaryAsync(
        PurchasingSummaryQuery query,
        CancellationToken cancellationToken)
    {
        var from = query.PeriodFrom;
        var to   = query.PeriodTo;
        var sid  = query.SupplierId;

        // ── Purchase Order ──
        var poQuery = _dbContext.FinPurchaseOrders
            .AsNoTracking()
            .Where(x => !x.IsDelete);
        if (sid.HasValue)  poQuery = poQuery.Where(x => x.SupplierId == sid.Value);
        if (from.HasValue) poQuery = poQuery.Where(x => x.RequestedAt.Date >= from.Value.ToDateTime(TimeOnly.MinValue));
        if (to.HasValue)   poQuery = poQuery.Where(x => x.RequestedAt.Date <= to.Value.ToDateTime(TimeOnly.MaxValue));

        var poCount = await poQuery.CountAsync(cancellationToken);
        var poTotal = await poQuery.SumAsync(x => (decimal?)x.TotalAmount ?? 0m, cancellationToken);

        // ── Tanda Terima Barang (lewat PurchaseOrder untuk filter SupplierId) ──
        var grQuery = _dbContext.FinGoodsReceipts
            .AsNoTracking()
            .Where(x => !x.IsDelete);
        if (sid.HasValue)  grQuery = grQuery.Where(x => x.PurchaseOrder != null && x.PurchaseOrder.SupplierId == sid.Value);
        if (from.HasValue) grQuery = grQuery.Where(x => x.ReceivedDate >= from.Value);
        if (to.HasValue)   grQuery = grQuery.Where(x => x.ReceivedDate <= to.Value);

        var grCount = await grQuery.CountAsync(cancellationToken);

        // ── Tukar Faktur ──
        var ieQuery = _dbContext.FinInvoiceExchanges
            .AsNoTracking()
            .Where(x => !x.IsDelete && x.Status != FinInvoiceExchangeStatuses.Cancelled);
        if (sid.HasValue)  ieQuery = ieQuery.Where(x => x.SupplierId == sid.Value);
        if (from.HasValue) ieQuery = ieQuery.Where(x => x.ReceivedDate >= from.Value);
        if (to.HasValue)   ieQuery = ieQuery.Where(x => x.ReceivedDate <= to.Value);

        var ieCount      = await ieQuery.CountAsync(cancellationToken);
        var pendingCount = await ieQuery.CountAsync(x => x.Status == FinInvoiceExchangeStatuses.Received, cancellationToken);

        // ── Purchasing Invoice ──
        var invQuery = _dbContext.FinPurchasingInvoices
            .AsNoTracking()
            .Where(x => !x.IsDelete && x.Status != FinPurchasingInvoiceStatuses.Cancelled);
        if (sid.HasValue)  invQuery = invQuery.Where(x => x.SupplierId == sid.Value);
        if (from.HasValue) invQuery = invQuery.Where(x => x.RequestedAt.Date >= from.Value.ToDateTime(TimeOnly.MinValue));
        if (to.HasValue)   invQuery = invQuery.Where(x => x.RequestedAt.Date <= to.Value.ToDateTime(TimeOnly.MaxValue));

        var invCount = await invQuery.CountAsync(cancellationToken);
        var invTotal = await invQuery.SumAsync(x => (decimal?)x.TotalAmount ?? 0m, cancellationToken);

        var approvedInvGroup = await invQuery
            .Where(x => x.Status == FinPurchasingInvoiceStatuses.Approved)
            .GroupBy(_ => 1)
            .Select(g => new { Count = g.Count(), Total = g.Sum(x => x.TotalAmount) })
            .FirstOrDefaultAsync(cancellationToken);

        // ── Utang dari Purchasing Invoice (SourcePurchasingInvoiceId terisi) ──
        var payableQuery = _dbContext.FinSupplierPayables
            .AsNoTracking()
            .Where(x => !x.IsDelete
                     && x.SourcePurchasingInvoiceId.HasValue
                     && x.Status != FinSupplierPayableStatuses.Paid
                     && x.Status != FinSupplierPayableStatuses.Cancelled);
        if (sid.HasValue) payableQuery = payableQuery.Where(x => x.SupplierId == sid.Value);

        var outstandingTotal = await payableQuery
            .SumAsync(x => (decimal?)x.OutstandingAmount ?? 0m, cancellationToken);

        return new PurchasingSummaryResponse
        {
            PurchaseOrderCount      = poCount,
            PurchaseOrderTotal      = poTotal,
            GoodsReceiptCount       = grCount,
            InvoiceExchangeCount    = ieCount,
            PendingExchangeCount    = pendingCount,
            PurchasingInvoiceCount  = invCount,
            PurchasingInvoiceTotal  = invTotal,
            ApprovedInvoiceCount    = approvedInvGroup?.Count ?? 0,
            ApprovedInvoiceTotal    = approvedInvGroup?.Total ?? 0m,
            OutstandingPayableTotal = outstandingTotal
        };
    }

    // ──────────────────────────────────────────────────────────────────────────────────
    // GET /purchasing/reports/invoice-exchanges  (FIN-API-1.1 §B.6)
    // ──────────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Laporan Tukar Faktur beserta status keterkaitan ke Purchasing Invoice.
    /// Join ke PurchasingInvoice bersifat left-join — Tukar Faktur yang belum terhubung
    /// tetap muncul dengan kolom invoice bernilai null.
    /// </summary>
    public async Task<PagedResult<InvoiceExchangeReportResponse>> GetInvoiceExchangesAsync(
        InvoiceExchangeReportQuery query,
        CancellationToken cancellationToken)
    {
        var pageSize   = Math.Clamp(query.PageSize, 1, 100);
        var pageNumber = Math.Max(query.PageNumber, 1);

        var q = _dbContext.FinInvoiceExchanges
            .AsNoTracking()
            .Include(x => x.Supplier)
            .Include(x => x.PurchaseOrder)
            .Include(x => x.PurchasingInvoice)
            .Where(x => !x.IsDelete);

        if (query.SupplierId.HasValue)
            q = q.Where(x => x.SupplierId == query.SupplierId.Value);
        if (!string.IsNullOrWhiteSpace(query.Status))
            q = q.Where(x => x.Status == query.Status);
        if (query.ReceivedDateFrom.HasValue)
            q = q.Where(x => x.ReceivedDate >= query.ReceivedDateFrom.Value);
        if (query.ReceivedDateTo.HasValue)
            q = q.Where(x => x.ReceivedDate <= query.ReceivedDateTo.Value);

        var totalData = await q.CountAsync(cancellationToken);

        var items = await q
            .OrderByDescending(x => x.ReceivedDate)
            .ThenByDescending(x => x.ExchangeNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new InvoiceExchangeReportResponse
            {
                Id                      = x.Id,
                ExchangeNumber          = x.ExchangeNumber,
                SupplierId              = x.SupplierId,
                SupplierName            = x.Supplier != null ? x.Supplier.SupplierName : string.Empty,
                PurchaseOrderId         = x.PurchaseOrderId,
                PONumber                = x.PurchaseOrder != null ? x.PurchaseOrder.PONumber : null,
                GoodsReceiptId          = x.GoodsReceiptId,
                SupplierInvoiceNumber   = x.SupplierInvoiceNumber,
                SupplierInvoiceDate     = x.SupplierInvoiceDate,
                ReceivedDate            = x.ReceivedDate,
                EstimatedDueDate        = x.EstimatedDueDate,
                Status                  = x.Status,
                PurchasingInvoiceId     = x.PurchasingInvoice != null ? x.PurchasingInvoice.Id            : (Guid?)null,
                PurchasingInvoiceNumber = x.PurchasingInvoice != null ? x.PurchasingInvoice.InvoiceNumber : null,
                PurchasingInvoiceStatus = x.PurchasingInvoice != null ? x.PurchasingInvoice.Status        : null,
                PurchasingInvoiceTotal  = x.PurchasingInvoice != null ? x.PurchasingInvoice.TotalAmount   : (decimal?)null
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<InvoiceExchangeReportResponse>
        {
            Items      = items,
            PageNumber = pageNumber,
            PageSize   = pageSize,
            TotalData  = totalData,
            TotalPage  = (int)Math.Ceiling(totalData / (double)pageSize)
        };
    }

    // ──────────────────────────────────────────────────────────────────────────────────
    // GET /purchasing/reports/due-dates  (FIN-API-1.1 §B.6)
    // ──────────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Laporan Purchasing Invoice mendekati atau melewati jatuh tempo.
    /// EstimatedDueDate berasal dari Tukar Faktur yang terhubung; ActualDueDate dari
    /// FinSupplierPayable.DueDate bila invoice sudah APPROVED dan utangnya ada.
    /// DaysUntilDue dihitung service (bukan database) agar tidak bergantung DB timezone.
    /// </summary>
    public async Task<PagedResult<DueDateReportResponse>> GetDueDatesAsync(
        DueDateReportQuery query,
        CancellationToken cancellationToken)
    {
        var pageSize   = Math.Clamp(query.PageSize, 1, 100);
        var pageNumber = Math.Max(query.PageNumber, 1);
        var today      = FinanceBusinessDate.Today();

        var q = _dbContext.FinPurchasingInvoices
            .AsNoTracking()
            .Include(x => x.Supplier)
            .Include(x => x.InvoiceExchange)
            .Where(x => !x.IsDelete && x.Status != FinPurchasingInvoiceStatuses.Cancelled);

        if (query.SupplierId.HasValue)
            q = q.Where(x => x.SupplierId == query.SupplierId.Value);
        if (!string.IsNullOrWhiteSpace(query.Status))
            q = q.Where(x => x.Status == query.Status);
        if (query.DueDateFrom.HasValue)
            q = q.Where(x => x.InvoiceExchange != null && x.InvoiceExchange.EstimatedDueDate >= query.DueDateFrom.Value);
        if (query.DueDateTo.HasValue)
            q = q.Where(x => x.InvoiceExchange != null && x.InvoiceExchange.EstimatedDueDate <= query.DueDateTo.Value);

        var totalData = await q.CountAsync(cancellationToken);

        var invoices = await q
            .OrderBy(x => x.InvoiceExchange != null ? x.InvoiceExchange.EstimatedDueDate : DateOnly.MaxValue)
            .ThenBy(x => x.InvoiceNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Kumpulkan IDs untuk join ke FinSupplierPayable (satu query, bukan N+1)
        var invoiceIds = invoices.Select(x => x.Id).ToList();
        var payables = await _dbContext.FinSupplierPayables
            .AsNoTracking()
            .Where(x => x.SourcePurchasingInvoiceId.HasValue
                     && invoiceIds.Contains(x.SourcePurchasingInvoiceId!.Value)
                     && !x.IsDelete)
            .ToDictionaryAsync(x => x.SourcePurchasingInvoiceId!.Value, cancellationToken);

        var items = invoices.Select(inv =>
        {
            var estimatedDue = inv.InvoiceExchange?.EstimatedDueDate;
            payables.TryGetValue(inv.Id, out var payable);
            var actualDue = payable?.DueDate;

            // Pilih tanggal paling definitif untuk menghitung DaysUntilDue
            var referenceDue = actualDue ?? estimatedDue;
            int? daysUntilDue = referenceDue.HasValue
                ? referenceDue.Value.DayNumber - today.DayNumber
                : null;

            return new DueDateReportResponse
            {
                PurchasingInvoiceId     = inv.Id,
                PurchasingInvoiceNumber = inv.InvoiceNumber,
                SupplierId              = inv.SupplierId,
                SupplierName            = inv.Supplier?.SupplierName ?? string.Empty,
                TotalAmount             = inv.TotalAmount,
                Status                  = inv.Status,
                ApprovalTier            = inv.ApprovalTier,
                ApprovedAt              = inv.ApprovedAt,
                EstimatedDueDate        = estimatedDue,
                ActualDueDate           = actualDue,
                DaysUntilDue            = daysUntilDue,
                SupplierPayableId       = payable?.Id,
                OutstandingAmount       = payable?.OutstandingAmount
            };
        })
        .Where(r => !query.OverdueOnly || (r.DaysUntilDue.HasValue && r.DaysUntilDue.Value < 0))
        .ToList();

        return new PagedResult<DueDateReportResponse>
        {
            Items      = items,
            PageNumber = pageNumber,
            PageSize   = pageSize,
            TotalData  = totalData,
            TotalPage  = (int)Math.Ceiling(totalData / (double)pageSize)
        };
    }

    // ──────────────────────────────────────────────────────────────────────────────────
    // GET /purchasing/reports/reconciliation  (FIN-API-1.1 §B.6)
    // ──────────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Rekonsiliasi: Tukar Faktur berstatus RECEIVED yang belum menghasilkan Purchasing Invoice.
    /// Hanya menampilkan RECEIVED — status LINKED_TO_INVOICE dan CANCELLED sudah "selesai".
    /// </summary>
    public async Task<PagedResult<ReconciliationResponse>> GetReconciliationAsync(
        ReconciliationQuery query,
        CancellationToken cancellationToken)
    {
        var pageSize   = Math.Clamp(query.PageSize, 1, 100);
        var pageNumber = Math.Max(query.PageNumber, 1);
        var today      = FinanceBusinessDate.Today();

        var q = _dbContext.FinInvoiceExchanges
            .AsNoTracking()
            .Include(x => x.Supplier)
            .Include(x => x.PurchaseOrder)
            .Where(x => !x.IsDelete && x.Status == FinInvoiceExchangeStatuses.Received);

        if (query.SupplierId.HasValue)
            q = q.Where(x => x.SupplierId == query.SupplierId.Value);
        if (query.ReceivedDateFrom.HasValue)
            q = q.Where(x => x.ReceivedDate >= query.ReceivedDateFrom.Value);
        if (query.ReceivedDateTo.HasValue)
            q = q.Where(x => x.ReceivedDate <= query.ReceivedDateTo.Value);

        var totalData = await q.CountAsync(cancellationToken);

        var raw = await q
            .OrderBy(x => x.EstimatedDueDate)
            .ThenBy(x => x.ExchangeNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = raw
            .Select(x =>
            {
                var days = x.EstimatedDueDate.DayNumber - today.DayNumber;
                return new ReconciliationResponse
                {
                    InvoiceExchangeId     = x.Id,
                    ExchangeNumber        = x.ExchangeNumber,
                    SupplierId            = x.SupplierId,
                    SupplierName          = x.Supplier?.SupplierName ?? string.Empty,
                    PurchaseOrderId       = x.PurchaseOrderId,
                    PONumber              = x.PurchaseOrder?.PONumber,
                    SupplierInvoiceNumber = x.SupplierInvoiceNumber,
                    SupplierInvoiceDate   = x.SupplierInvoiceDate,
                    ReceivedDate          = x.ReceivedDate,
                    EstimatedDueDate      = x.EstimatedDueDate,
                    DaysUntilDue          = days
                };
            })
            .Where(r => !query.OverdueOnly || r.DaysUntilDue < 0)
            .ToList();

        return new PagedResult<ReconciliationResponse>
        {
            Items      = items,
            PageNumber = pageNumber,
            PageSize   = pageSize,
            TotalData  = totalData,
            TotalPage  = (int)Math.Ceiling(totalData / (double)pageSize)
        };
    }
}
