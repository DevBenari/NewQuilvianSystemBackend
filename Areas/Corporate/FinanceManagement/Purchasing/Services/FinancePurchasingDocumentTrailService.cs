using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Security;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Services;

/// <summary>
/// Jejak dokumen pembelian (P01): dari satu dokumen jangkar (PO, GR, Tukar Faktur, atau Purchasing
/// Invoice) menelusuri rantai PO → GR → Tukar Faktur → Invoice → Utang → Pembayaran/Retur tanpa
/// petugas menyalin ID. Murni baca, nol tabel baru; hanya memakai FK yang sudah ada.
///
/// Hak akses: controller jangkar sudah menjaga hak baca dokumen jangkarnya. Bagian rantai LAIN
/// diperiksa di sini dengan pasangan resource:aksi yang sama dengan endpoint bacanya, sehingga
/// jejak tidak menjadi jalan belakang untuk membaca dokumen yang tidak boleh dilihat pemanggil
/// (AC-20). Bagian yang tidak boleh dibaca dikosongkan dan dicantumkan pada RestrictedSections.
///
/// Hubungan TF → GR memakai kolom tunggal <c>GoodsReceiptId</c> yang ada. Relasi bundel banyak GR
/// BELUM ada di basis data (lihat spesifikasi schema P04); bila sudah aktif, penentuan "GR milik TF"
/// harus berpindah ke satu penyelesai kanonik yang menyatukan kolom lama dan relasi baru.
/// </summary>
public sealed class FinancePurchasingDocumentTrailService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly AccessPermissionService _accessPermissionService;

    public FinancePurchasingDocumentTrailService(
        ApplicationDbContext dbContext,
        AccessPermissionService accessPermissionService)
    {
        _dbContext = dbContext;
        _accessPermissionService = accessPermissionService;
    }

    // Pasangan bagian → resource hak baca. Harus identik dengan [AccessPermission] pada endpoint
    // baca masing-masing dokumen.
    private static readonly (string Section, string Resource)[] SectionResources =
    [
        (PurchasingTrailSections.PurchaseOrder, "FinancePurchaseOrder"),
        (PurchasingTrailSections.GoodsReceipts, "FinanceGoodsReceipt"),
        (PurchasingTrailSections.InvoiceExchanges, "FinanceInvoiceExchange"),
        (PurchasingTrailSections.Invoices, "FinancePurchasingInvoice"),
        (PurchasingTrailSections.Payables, "FinanceSupplierPayable"),
        (PurchasingTrailSections.Payments, "FinancePayment"),
        (PurchasingTrailSections.SupplierReturns, "FinanceSupplierReturn")
    ];

    public async Task<PurchasingDocumentTrailResponse> GetTrailAsync(
        string documentType, Guid documentId, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        // 1. Tentukan PO dan/atau Tukar Faktur jangkar dari dokumen yang diminta.
        Guid? purchaseOrderId;
        FinInvoiceExchange? anchorExchange = null;

        switch (documentType)
        {
            case PurchasingDocumentTypes.PurchaseOrder:
                purchaseOrderId = await _dbContext.FinPurchaseOrders.AsNoTracking()
                    .Where(x => x.Id == documentId && !x.IsDelete)
                    .Select(x => (Guid?)x.Id)
                    .SingleOrDefaultAsync(cancellationToken)
                    ?? throw new KeyNotFoundException("Purchase Order tidak ditemukan.");
                break;

            case PurchasingDocumentTypes.GoodsReceipt:
                purchaseOrderId = await _dbContext.FinGoodsReceipts.AsNoTracking()
                    .Where(x => x.Id == documentId && !x.IsDelete)
                    .Select(x => (Guid?)x.PurchaseOrderId)
                    .SingleOrDefaultAsync(cancellationToken)
                    ?? throw new KeyNotFoundException("Tanda Terima Barang tidak ditemukan.");
                break;

            case PurchasingDocumentTypes.InvoiceExchange:
                anchorExchange = await _dbContext.FinInvoiceExchanges.AsNoTracking()
                    .SingleOrDefaultAsync(x => x.Id == documentId && !x.IsDelete, cancellationToken)
                    ?? throw new KeyNotFoundException("Tukar Faktur tidak ditemukan.");
                purchaseOrderId = await ResolvePurchaseOrderIdAsync(anchorExchange, cancellationToken);
                break;

            case PurchasingDocumentTypes.PurchasingInvoice:
                var exchangeId = await _dbContext.FinPurchasingInvoices.AsNoTracking()
                    .Where(x => x.Id == documentId && !x.IsDelete)
                    .Select(x => (Guid?)x.InvoiceExchangeId)
                    .SingleOrDefaultAsync(cancellationToken)
                    ?? throw new KeyNotFoundException("Purchasing Invoice tidak ditemukan.");
                anchorExchange = await _dbContext.FinInvoiceExchanges.AsNoTracking()
                    .SingleOrDefaultAsync(x => x.Id == exchangeId && !x.IsDelete, cancellationToken)
                    ?? throw new KeyNotFoundException("Tukar Faktur sumber tidak ditemukan.");
                purchaseOrderId = await ResolvePurchaseOrderIdAsync(anchorExchange, cancellationToken);
                break;

            default:
                throw new PurchasingBadRequestException("Jenis dokumen jejak tidak dikenal.");
        }

        var allowed = await ResolveAllowedSectionsAsync(user);

        // 2. Muat rantai: PO + GR (bila ada PO), Tukar Faktur, Invoice, Utang, Pembayaran, Retur.
        FinPurchaseOrder? purchaseOrder = null;
        var goodsReceipts = new List<FinGoodsReceipt>();
        if (purchaseOrderId.HasValue)
        {
            var poId = purchaseOrderId.Value;
            purchaseOrder = await _dbContext.FinPurchaseOrders.AsNoTracking()
                .Include(x => x.Items.Where(item => !item.IsDelete))
                .SingleOrDefaultAsync(x => x.Id == poId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Purchase Order tidak ditemukan.");

            goodsReceipts = await _dbContext.FinGoodsReceipts.AsNoTracking()
                .Where(x => x.PurchaseOrderId == poId && !x.IsDelete)
                .OrderBy(x => x.ReceivedDate).ThenBy(x => x.CreateDateTime)
                .ToListAsync(cancellationToken);
        }

        var goodsReceiptIds = goodsReceipts.Select(x => x.Id).ToList();

        List<FinInvoiceExchange> exchanges;
        if (purchaseOrderId.HasValue)
        {
            var poId = purchaseOrderId.Value;
            exchanges = await _dbContext.FinInvoiceExchanges.AsNoTracking()
                .Where(x => !x.IsDelete
                    && (x.PurchaseOrderId == poId
                        || (x.GoodsReceiptId != null && goodsReceiptIds.Contains(x.GoodsReceiptId.Value))))
                .OrderBy(x => x.ReceivedDate).ThenBy(x => x.CreateDateTime)
                .ToListAsync(cancellationToken);
        }
        else
        {
            // Tukar Faktur berdiri sendiri (FIN-DEC-051): rantainya hanya TF itu sendiri.
            exchanges = new List<FinInvoiceExchange>();
            if (anchorExchange is not null) exchanges.Add(anchorExchange);
        }

        var exchangeIds = exchanges.Select(x => x.Id).ToList();
        var invoices = await _dbContext.FinPurchasingInvoices.AsNoTracking()
            .Where(x => !x.IsDelete && exchangeIds.Contains(x.InvoiceExchangeId))
            .OrderBy(x => x.CreateDateTime)
            .ToListAsync(cancellationToken);
        var invoiceIds = invoices.Select(x => x.Id).ToList();

        var payables = await _dbContext.FinSupplierPayables.AsNoTracking()
            .Where(x => !x.IsDelete && x.SourcePurchasingInvoiceId != null && invoiceIds.Contains(x.SourcePurchasingInvoiceId.Value))
            .OrderBy(x => x.CreateDateTime)
            .ToListAsync(cancellationToken);
        var payableIds = payables.Select(x => x.Id).ToList();

        var allocationRows = await _dbContext.FinPaymentAllocations.AsNoTracking()
            .Where(x => !x.IsDelete && x.SupplierPayableId != null && payableIds.Contains(x.SupplierPayableId.Value)
                && x.Payment != null && !x.Payment.IsDelete)
            .Select(x => new
            {
                x.PaymentId,
                PaymentNumber = x.Payment!.PaymentNumber,
                PaymentStatus = x.Payment.Status,
                PayableId = x.SupplierPayableId!.Value,
                x.Amount,
                x.IsReversal
            })
            .ToListAsync(cancellationToken);

        var supplierReturns = await _dbContext.FinSupplierReturns.AsNoTracking()
            .Where(x => !x.IsDelete && invoiceIds.Contains(x.PurchasingInvoiceId))
            .OrderBy(x => x.CreateDateTime)
            .ToListAsync(cancellationToken);

        // Kuantitas diterima per baris PO dan per GR — satu query tiap sumbu, bukan per baris.
        var receivedByPoItem = new Dictionary<Guid, decimal>();
        var receivedByGoodsReceipt = new Dictionary<Guid, (decimal Total, int Count)>();
        if (purchaseOrderId.HasValue)
        {
            var poId = purchaseOrderId.Value;
            var itemRows = await _dbContext.FinGoodsReceiptItems.AsNoTracking()
                .Where(x => !x.IsDelete && x.GoodsReceipt!.PurchaseOrderId == poId && !x.GoodsReceipt.IsDelete)
                .Select(x => new
                {
                    x.GoodsReceiptId,
                    x.PurchaseOrderItemId,
                    x.ReceivedQuantity,
                    GoodsReceiptStatus = x.GoodsReceipt!.Status
                })
                .ToListAsync(cancellationToken);

            foreach (var row in itemRows)
            {
                var current = receivedByGoodsReceipt.GetValueOrDefault(row.GoodsReceiptId);
                receivedByGoodsReceipt[row.GoodsReceiptId] = (current.Total + row.ReceivedQuantity, current.Count + 1);

                if (row.GoodsReceiptStatus == FinGoodsReceiptStatuses.Cancelled) continue;
                receivedByPoItem[row.PurchaseOrderItemId] =
                    receivedByPoItem.GetValueOrDefault(row.PurchaseOrderItemId) + row.ReceivedQuantity;
            }
        }

        // 3. Susun respons, kosongkan bagian yang tidak boleh dibaca.
        var response = new PurchasingDocumentTrailResponse
        {
            FocusDocumentType = documentType,
            FocusDocumentId = documentId
        };

        void Restrict(string section)
        {
            if (!response.RestrictedSections.Contains(section)) response.RestrictedSections.Add(section);
        }

        if (purchaseOrder is not null)
        {
            if (allowed.Contains(PurchasingTrailSections.PurchaseOrder))
            {
                var supplierName = await _dbContext.MstSuppliers.AsNoTracking()
                    .Where(x => x.Id == purchaseOrder.SupplierId)
                    .Select(x => x.SupplierName)
                    .SingleOrDefaultAsync(cancellationToken) ?? string.Empty;

                response.PurchaseOrder = new PurchasingTrailPurchaseOrder
                {
                    Id = purchaseOrder.Id,
                    PONumber = purchaseOrder.PONumber,
                    Status = purchaseOrder.Status,
                    SupplierId = purchaseOrder.SupplierId,
                    SupplierName = supplierName,
                    TotalAmount = purchaseOrder.TotalAmount,
                    RowVersion = purchaseOrder.RowVersion
                };

                response.ItemProgress = purchaseOrder.Items
                    .Select(item =>
                    {
                        var received = receivedByPoItem.GetValueOrDefault(item.Id);
                        return new PurchasingTrailItemProgress
                        {
                            PurchaseOrderItemId = item.Id,
                            ProductName = item.ProductName,
                            Unit = item.Unit,
                            OrderedQuantity = item.Quantity,
                            ReceivedQuantity = received,
                            RemainingQuantity = Math.Max(0m, item.Quantity - received)
                        };
                    })
                    .ToList();
            }
            else
            {
                Restrict(PurchasingTrailSections.PurchaseOrder);
            }
        }

        if (allowed.Contains(PurchasingTrailSections.GoodsReceipts))
        {
            response.GoodsReceipts = goodsReceipts
                .Select(x =>
                {
                    var aggregate = receivedByGoodsReceipt.GetValueOrDefault(x.Id);
                    return new PurchasingTrailGoodsReceipt
                    {
                        Id = x.Id,
                        GRNumber = x.GRNumber,
                        ReceivedDate = x.ReceivedDate,
                        Status = x.Status,
                        TotalReceivedQuantity = aggregate.Total,
                        ItemCount = aggregate.Count
                    };
                })
                .ToList();
        }
        else if (goodsReceipts.Count > 0)
        {
            Restrict(PurchasingTrailSections.GoodsReceipts);
        }

        if (allowed.Contains(PurchasingTrailSections.InvoiceExchanges))
        {
            response.InvoiceExchanges = exchanges
                .Select(x => new PurchasingTrailInvoiceExchange
                {
                    Id = x.Id,
                    ExchangeNumber = x.ExchangeNumber,
                    SupplierInvoiceNumber = x.SupplierInvoiceNumber,
                    SupplierInvoiceDate = x.SupplierInvoiceDate,
                    ReceivedDate = x.ReceivedDate,
                    EstimatedDueDate = x.EstimatedDueDate,
                    Status = x.Status,
                    PurchaseOrderId = x.PurchaseOrderId,
                    GoodsReceiptId = x.GoodsReceiptId
                })
                .ToList();
        }
        else if (exchanges.Count > 0)
        {
            Restrict(PurchasingTrailSections.InvoiceExchanges);
        }

        if (allowed.Contains(PurchasingTrailSections.Invoices))
        {
            response.Invoices = invoices
                .Select(x => new PurchasingTrailInvoice
                {
                    Id = x.Id,
                    InvoiceNumber = x.InvoiceNumber,
                    InvoiceExchangeId = x.InvoiceExchangeId,
                    Status = x.Status,
                    TotalAmount = x.TotalAmount
                })
                .ToList();
        }
        else if (invoices.Count > 0)
        {
            Restrict(PurchasingTrailSections.Invoices);
        }

        if (allowed.Contains(PurchasingTrailSections.Payables))
        {
            response.Payables = payables
                .Select(x => new PurchasingTrailPayable
                {
                    Id = x.Id,
                    PayableNumber = x.PayableNumber,
                    SourcePurchasingInvoiceId = x.SourcePurchasingInvoiceId,
                    Status = x.Status,
                    OriginalAmount = x.OriginalAmount,
                    OutstandingAmount = x.OutstandingAmount,
                    DueDate = x.DueDate,
                    DueDateBasis = x.DueDateBasis
                })
                .ToList();
        }
        else if (payables.Count > 0)
        {
            Restrict(PurchasingTrailSections.Payables);
        }

        if (allowed.Contains(PurchasingTrailSections.Payments))
        {
            response.Payments = allocationRows
                .GroupBy(x => new { x.PaymentId, x.PaymentNumber, x.PaymentStatus, x.PayableId })
                .Select(g => new PurchasingTrailPayment
                {
                    PaymentId = g.Key.PaymentId,
                    PaymentNumber = g.Key.PaymentNumber,
                    Status = g.Key.PaymentStatus,
                    PayableId = g.Key.PayableId,
                    AllocatedAmount = g.Sum(x => x.IsReversal ? -x.Amount : x.Amount)
                })
                .OrderBy(x => x.PaymentNumber)
                .ToList();
        }
        else if (allocationRows.Count > 0)
        {
            Restrict(PurchasingTrailSections.Payments);
        }

        if (allowed.Contains(PurchasingTrailSections.SupplierReturns))
        {
            response.SupplierReturns = supplierReturns
                .Select(x => new PurchasingTrailSupplierReturn
                {
                    Id = x.Id,
                    ReturnNumber = x.ReturnNumber,
                    PurchasingInvoiceId = x.PurchasingInvoiceId,
                    Status = x.Status,
                    TotalAmount = x.TotalAmount
                })
                .ToList();
        }
        else if (supplierReturns.Count > 0)
        {
            Restrict(PurchasingTrailSections.SupplierReturns);
        }

        // 4. Kesiapan penutupan PO — aturan yang sama dengan POST .../close. Alasan penghambat memuat
        //    nomor GR/TF/invoice, jadi hanya dirinci bila pemanggil boleh membaca ketiganya.
        if (purchaseOrder is not null && allowed.Contains(PurchasingTrailSections.PurchaseOrder))
        {
            var assessment = PurchaseOrderClosurePolicy.Assess(
                purchaseOrder.Status,
                goodsReceipts.Select(x => new ClosureGoodsReceipt(x.Id, x.GRNumber, x.Status)).ToList(),
                exchanges.Select(x => new ClosureExchange(x.Id, x.ExchangeNumber, x.GoodsReceiptId, x.Status)).ToList(),
                invoices.Select(x => new ClosureInvoice(x.InvoiceExchangeId, x.InvoiceNumber, x.Status)).ToList());

            var canSeeDetails = allowed.Contains(PurchasingTrailSections.GoodsReceipts)
                && allowed.Contains(PurchasingTrailSections.InvoiceExchanges)
                && allowed.Contains(PurchasingTrailSections.Invoices);

            var blockingReasons = new List<string>();
            if (!assessment.CanClose)
            {
                if (canSeeDetails)
                    blockingReasons.AddRange(assessment.BlockingReasons);
                else
                    blockingReasons.Add("Masih ada dokumen turunan yang belum selesai. Rinciannya tidak ditampilkan karena hak akses terbatas.");
            }

            response.Closure = new PurchasingTrailClosure
            {
                CanClose = assessment.CanClose,
                BlockingReasons = blockingReasons
            };
        }

        return response;
    }

    private async Task<Guid?> ResolvePurchaseOrderIdAsync(FinInvoiceExchange exchange, CancellationToken cancellationToken)
    {
        if (exchange.PurchaseOrderId.HasValue) return exchange.PurchaseOrderId;
        if (!exchange.GoodsReceiptId.HasValue) return null;

        var goodsReceiptId = exchange.GoodsReceiptId.Value;
        return await _dbContext.FinGoodsReceipts.AsNoTracking()
            .Where(x => x.Id == goodsReceiptId && !x.IsDelete)
            .Select(x => (Guid?)x.PurchaseOrderId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    private async Task<HashSet<string>> ResolveAllowedSectionsAsync(ClaimsPrincipal user)
    {
        var allowed = new HashSet<string>();
        foreach (var (section, resource) in SectionResources)
        {
            if (await _accessPermissionService.HasAccessAsync(user, resource, "Read"))
                allowed.Add(section);
        }
        return allowed;
    }
}
