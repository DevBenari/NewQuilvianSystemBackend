using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Security;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Services;

/// <summary>
/// Antrean Penerimaan Invoice (P02): daftar kerja kelengkapan faktur dan verifikasi. Seluruhnya
/// PROYEKSI dari GR, Tukar Faktur, dan Purchasing Invoice yang sudah ada — tidak ada tabel baru dan
/// tidak ada status baru pada dokumen mana pun (tidak mengarang status DB).
///
/// Yang BELUM dapat ditampilkan karena datanya tidak ada di basis data: nomor surat jalan (SJ)
/// sementara dan status kelengkapan SJ/faktur sebagai fakta tersendiri. "Faktur menyusul" di sini
/// diturunkan dari keadaan: GR aktif yang belum punya Tukar Faktur aktif. Penyimpanan SJ terpisah
/// menunggu schema (lihat spesifikasi schema P02/P03).
///
/// Hak akses: controller menjaga hak baca Purchasing Invoice; kategori yang bersumber dari dokumen
/// lain diperiksa di sini dengan pasangan resource:aksi endpoint baca dokumen tersebut.
/// </summary>
public sealed class FinancePurchasingIntakeQueueService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly AccessPermissionService _accessPermissionService;

    private static readonly IReadOnlyDictionary<string, string> CategoryResources = new Dictionary<string, string>
    {
        [PurchasingIntakeCategories.AwaitingInvoice] = "FinanceGoodsReceipt",
        [PurchasingIntakeCategories.ReadyForInvoice] = "FinanceInvoiceExchange",
        [PurchasingIntakeCategories.NeedsRebuild] = "FinanceInvoiceExchange",
        [PurchasingIntakeCategories.InVerification] = "FinancePurchasingInvoice"
    };

    public FinancePurchasingIntakeQueueService(
        ApplicationDbContext dbContext,
        AccessPermissionService accessPermissionService)
    {
        _dbContext = dbContext;
        _accessPermissionService = accessPermissionService;
    }

    public async Task<PurchasingIntakeSummaryResponse> GetSummaryAsync(
        Guid? supplierId, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var response = new PurchasingIntakeSummaryResponse();
        foreach (var category in PurchasingIntakeCategories.All)
        {
            if (!await CanReadAsync(user, category))
            {
                response.RestrictedCategories.Add(category);
                continue;
            }

            var count = await BuildRowsQuery(category, supplierId).CountAsync(cancellationToken);
            response.Items.Add(new PurchasingIntakeSummaryItem { Category = category, Count = count });
        }
        return response;
    }

    public async Task<PagedResult<PurchasingIntakeRow>> GetPagedAsync(
        PurchasingIntakeQuery query, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var category = string.IsNullOrWhiteSpace(query.Category)
            ? PurchasingIntakeCategories.AwaitingInvoice
            : query.Category.Trim().ToUpperInvariant();
        if (!PurchasingIntakeCategories.All.Contains(category))
            throw new PurchasingBadRequestException("Kategori antrean tidak dikenal.");

        if (!await CanReadAsync(user, category))
            throw new PurchasingForbiddenException("Anda tidak berhak membaca dokumen sumber kategori antrean ini.");

        var q = BuildRowsQuery(category, query.SupplierId);
        var totalCount = await q.CountAsync(cancellationToken);

        // Yang paling lama menunggu didahulukan; Id sebagai pemecah seri supaya paging stabil.
        var rows = await q.OrderBy(x => x.DocumentDate).ThenBy(x => x.DocumentId)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var todayWib = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
        foreach (var row in rows)
            row.AgeDays = Math.Max(0, todayWib.DayNumber - row.DocumentDate.DayNumber);

        return new PagedResult<PurchasingIntakeRow>
        {
            Items = rows,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalData = totalCount,
            TotalPage = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };
    }

    private Task<bool> CanReadAsync(ClaimsPrincipal user, string category) =>
        _accessPermissionService.HasAccessAsync(user, CategoryResources[category], "Read");

    private IQueryable<PurchasingIntakeRow> BuildRowsQuery(string category, Guid? supplierId)
    {
        var hasSupplier = supplierId.HasValue && supplierId.Value != Guid.Empty;
        var supplier = supplierId.GetValueOrDefault();

        switch (category)
        {
            case PurchasingIntakeCategories.AwaitingInvoice:
            {
                var goodsReceipts = _dbContext.FinGoodsReceipts.AsNoTracking()
                    .Where(g => !g.IsDelete
                        && g.Status == FinGoodsReceiptStatuses.Received
                        && !_dbContext.FinInvoiceExchanges.Any(t => !t.IsDelete
                            && t.GoodsReceiptId == g.Id
                            && t.Status != FinInvoiceExchangeStatuses.Cancelled));
                if (hasSupplier)
                    goodsReceipts = goodsReceipts.Where(g => g.PurchaseOrder!.SupplierId == supplier);

                return goodsReceipts.Select(g => new PurchasingIntakeRow
                {
                    Category = PurchasingIntakeCategories.AwaitingInvoice,
                    DocumentType = PurchasingDocumentTypes.GoodsReceipt,
                    DocumentId = g.Id,
                    DocumentNumber = g.GRNumber,
                    DocumentDate = g.ReceivedDate,
                    Status = g.Status,
                    SupplierId = g.PurchaseOrder!.SupplierId,
                    SupplierName = g.PurchaseOrder.Supplier!.SupplierName,
                    PurchaseOrderId = g.PurchaseOrderId,
                    PurchaseOrderNumber = g.PurchaseOrder.PONumber,
                    GoodsReceiptId = g.Id,
                    NextAction = PurchasingIntakeNextActions.RecordInvoiceExchange
                });
            }

            case PurchasingIntakeCategories.ReadyForInvoice:
            {
                var exchanges = _dbContext.FinInvoiceExchanges.AsNoTracking()
                    .Where(t => !t.IsDelete
                        && t.Status == FinInvoiceExchangeStatuses.Received
                        && !_dbContext.FinPurchasingInvoices.Any(i => i.InvoiceExchangeId == t.Id && !i.IsDelete));
                if (hasSupplier)
                    exchanges = exchanges.Where(t => t.SupplierId == supplier);

                return SelectExchangeRows(exchanges, PurchasingIntakeCategories.ReadyForInvoice, PurchasingIntakeNextActions.ComposeInvoice);
            }

            case PurchasingIntakeCategories.NeedsRebuild:
            {
                // Tukar Faktur RECEIVED yang sudah pernah punya invoice tetapi tidak satu pun masih berlaku.
                // Unique index InvoiceExchangeId masih terikat pada invoice lama, jadi invoice baru tidak
                // dapat disusun di atas Tukar Faktur yang sama: batalkan Tukar Faktur lalu catat ulang.
                var exchanges = _dbContext.FinInvoiceExchanges.AsNoTracking()
                    .Where(t => !t.IsDelete
                        && t.Status == FinInvoiceExchangeStatuses.Received
                        && _dbContext.FinPurchasingInvoices.Any(i => i.InvoiceExchangeId == t.Id && !i.IsDelete)
                        && !_dbContext.FinPurchasingInvoices.Any(i => i.InvoiceExchangeId == t.Id && !i.IsDelete
                            && i.Status != FinPurchasingInvoiceStatuses.Cancelled
                            && i.Status != FinPurchasingInvoiceStatuses.Rejected));
                if (hasSupplier)
                    exchanges = exchanges.Where(t => t.SupplierId == supplier);

                return SelectExchangeRows(exchanges, PurchasingIntakeCategories.NeedsRebuild, PurchasingIntakeNextActions.RebuildExchange);
            }

            case PurchasingIntakeCategories.InVerification:
            {
                var invoices = _dbContext.FinPurchasingInvoices.AsNoTracking()
                    .Where(i => !i.IsDelete
                        && (i.Status == FinPurchasingInvoiceStatuses.Draft
                            || i.Status == FinPurchasingInvoiceStatuses.PendingApproval));
                if (hasSupplier)
                    invoices = invoices.Where(i => i.SupplierId == supplier);

                return invoices.Select(i => new PurchasingIntakeRow
                {
                    Category = PurchasingIntakeCategories.InVerification,
                    DocumentType = PurchasingDocumentTypes.PurchasingInvoice,
                    DocumentId = i.Id,
                    DocumentNumber = i.InvoiceNumber,
                    DocumentDate = i.InvoiceExchange!.ReceivedDate,
                    Status = i.Status,
                    SupplierId = i.SupplierId,
                    SupplierName = i.Supplier!.SupplierName,
                    PurchaseOrderId = i.InvoiceExchange.PurchaseOrderId,
                    PurchaseOrderNumber = i.InvoiceExchange.PurchaseOrder != null ? i.InvoiceExchange.PurchaseOrder.PONumber : null,
                    GoodsReceiptId = i.InvoiceExchange.GoodsReceiptId,
                    SupplierInvoiceNumber = i.InvoiceExchange.SupplierInvoiceNumber,
                    EstimatedDueDate = i.InvoiceExchange.EstimatedDueDate,
                    NextAction = PurchasingIntakeNextActions.ContinueVerification
                });
            }

            default:
                throw new PurchasingBadRequestException("Kategori antrean tidak dikenal.");
        }
    }

    private static IQueryable<PurchasingIntakeRow> SelectExchangeRows(
        IQueryable<FinInvoiceExchange> exchanges, string category, string nextAction) =>
        exchanges.Select(t => new PurchasingIntakeRow
        {
            Category = category,
            DocumentType = PurchasingDocumentTypes.InvoiceExchange,
            DocumentId = t.Id,
            DocumentNumber = t.ExchangeNumber,
            DocumentDate = t.ReceivedDate,
            Status = t.Status,
            SupplierId = t.SupplierId,
            SupplierName = t.Supplier!.SupplierName,
            PurchaseOrderId = t.PurchaseOrderId,
            PurchaseOrderNumber = t.PurchaseOrder != null ? t.PurchaseOrder.PONumber : null,
            GoodsReceiptId = t.GoodsReceiptId,
            SupplierInvoiceNumber = t.SupplierInvoiceNumber,
            EstimatedDueDate = t.EstimatedDueDate,
            NextAction = nextAction
        });
}
