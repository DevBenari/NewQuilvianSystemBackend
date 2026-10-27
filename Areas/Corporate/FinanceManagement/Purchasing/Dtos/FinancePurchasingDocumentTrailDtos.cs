namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Dtos;

/// <summary>Jenis dokumen yang boleh menjadi jangkar jejak dokumen (P01). Nilai sama dengan yang dikirim klien.</summary>
public static class PurchasingDocumentTypes
{
    public const string PurchaseOrder = "PURCHASE_ORDER";
    public const string GoodsReceipt = "GOODS_RECEIPT";
    public const string InvoiceExchange = "INVOICE_EXCHANGE";
    public const string PurchasingInvoice = "PURCHASING_INVOICE";
}

/// <summary>Nama bagian jejak dokumen; dipakai juga pada <c>RestrictedSections</c>.</summary>
public static class PurchasingTrailSections
{
    public const string PurchaseOrder = "PURCHASE_ORDER";
    public const string GoodsReceipts = "GOODS_RECEIPTS";
    public const string InvoiceExchanges = "INVOICE_EXCHANGES";
    public const string Invoices = "INVOICES";
    public const string Payables = "PAYABLES";
    public const string Payments = "PAYMENTS";
    public const string SupplierReturns = "SUPPLIER_RETURNS";
}

public sealed class PurchasingTrailPurchaseOrder
{
    public Guid Id { get; set; }
    public string PONumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public Guid RowVersion { get; set; }
}

/// <summary>Kemajuan penerimaan per baris PO: dipesan, sudah diterima (GR aktif), dan sisa.</summary>
public sealed class PurchasingTrailItemProgress
{
    public Guid PurchaseOrderItemId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal OrderedQuantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal RemainingQuantity { get; set; }
}

public sealed class PurchasingTrailGoodsReceipt
{
    public Guid Id { get; set; }
    public string GRNumber { get; set; } = string.Empty;
    public DateOnly ReceivedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalReceivedQuantity { get; set; }
    public int ItemCount { get; set; }
}

public sealed class PurchasingTrailInvoiceExchange
{
    public Guid Id { get; set; }
    public string ExchangeNumber { get; set; } = string.Empty;
    public string SupplierInvoiceNumber { get; set; } = string.Empty;
    public DateOnly SupplierInvoiceDate { get; set; }
    public DateOnly ReceivedDate { get; set; }
    public DateOnly EstimatedDueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? PurchaseOrderId { get; set; }
    public Guid? GoodsReceiptId { get; set; }
}

public sealed class PurchasingTrailInvoice
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid InvoiceExchangeId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}

public sealed class PurchasingTrailPayable
{
    public Guid Id { get; set; }
    public string PayableNumber { get; set; } = string.Empty;
    public Guid? SourcePurchasingInvoiceId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal OriginalAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public DateOnly DueDate { get; set; }
    public string? DueDateBasis { get; set; }
}

/// <summary>Pembayaran yang mengalokasikan ke utang di jejak. Nilai alokasi bersih (alokasi dikurangi pembalik).</summary>
public sealed class PurchasingTrailPayment
{
    public Guid PaymentId { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid PayableId { get; set; }
    public decimal AllocatedAmount { get; set; }
}

public sealed class PurchasingTrailSupplierReturn
{
    public Guid Id { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public Guid PurchasingInvoiceId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}

/// <summary>Kesiapan penutupan administratif PO; aturan sama dengan <c>POST .../close</c>.</summary>
public sealed class PurchasingTrailClosure
{
    public bool CanClose { get; set; }
    public List<string> BlockingReasons { get; set; } = [];
}

/// <summary>
/// Jejak satu rantai dokumen pembelian: PO → GR → Tukar Faktur → Invoice → Utang → Pembayaran/Retur.
/// Hanya bagian yang boleh dibaca pemanggil yang diisi; bagian lain kosong dan namanya tercantum di
/// <see cref="RestrictedSections"/>, bukan diisi diam-diam (AC-20). Bagian yang kosong tanpa tercantum
/// pada <see cref="RestrictedSections"/> berarti memang belum ada dokumennya.
/// </summary>
public sealed class PurchasingDocumentTrailResponse
{
    public string FocusDocumentType { get; set; } = string.Empty;
    public Guid FocusDocumentId { get; set; }

    /// <summary>Kosong untuk rantai tanpa PO (Tukar Faktur berdiri sendiri, FIN-DEC-051).</summary>
    public PurchasingTrailPurchaseOrder? PurchaseOrder { get; set; }
    public List<PurchasingTrailItemProgress> ItemProgress { get; set; } = [];
    public List<PurchasingTrailGoodsReceipt> GoodsReceipts { get; set; } = [];
    public List<PurchasingTrailInvoiceExchange> InvoiceExchanges { get; set; } = [];
    public List<PurchasingTrailInvoice> Invoices { get; set; } = [];
    public List<PurchasingTrailPayable> Payables { get; set; } = [];
    public List<PurchasingTrailPayment> Payments { get; set; } = [];
    public List<PurchasingTrailSupplierReturn> SupplierReturns { get; set; } = [];

    public PurchasingTrailClosure? Closure { get; set; }
    public List<string> RestrictedSections { get; set; } = [];
}
