using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

/// <summary>
/// Catatan konflik sinkronisasi PO/GR dari BLM (BE-FIN-143, Revisi 23, FIN-DES-136, data-dictionary 13.9).
/// Konflik dibuat bila kiriman tidak boleh diterapkan otomatis atau bila pembatalan di BLM berbenturan
/// dengan dokumen turunan AP. Konflik TIDAK PERNAH membalik utang otomatis (FIN-DEC-264); ia hanya mencatat
/// keadaan dan keputusan petugas Finance. Koreksi keuangan dicatat di modulnya sendiri. Paling banyak satu
/// konflik terbuka per (kiriman, jenis).
/// </summary>
[Table("FinPurchasingSyncConflict", Schema = "public")]
public sealed class FinPurchasingSyncConflict : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Kiriman yang memicu konflik.</summary>
    public Guid InboundMessageId { get; set; }
    public FinPurchasingInboundMessage? InboundMessage { get; set; }

    /// <summary><see cref="FinPurchasingSyncConflictTypes"/>.</summary>
    [Required, MaxLength(50)] public string ConflictType { get; set; } = string.Empty;

    public Guid? PurchaseOrderId { get; set; }
    public FinPurchaseOrder? PurchaseOrder { get; set; }

    public Guid? GoodsReceiptId { get; set; }
    public FinGoodsReceipt? GoodsReceipt { get; set; }

    /// <summary><see cref="FinPurchasingSyncConflictDependentTypes"/>. Dokumen turunan dirujuk tanpa FK
    /// karena jenisnya beragam.</summary>
    [MaxLength(30)] public string? DependentDocumentType { get; set; }

    /// <summary>Wajib bila <see cref="DependentDocumentType"/> terisi (check database).</summary>
    public Guid? DependentDocumentId { get; set; }

    /// <summary><see cref="FinPurchasingSyncConflictStatuses"/>; RESOLVED adalah status akhir.</summary>
    [Required, MaxLength(20)] public string Status { get; set; } = FinPurchasingSyncConflictStatuses.Open;

    /// <summary><see cref="FinPurchasingSyncConflictResolutionCodes"/>; wajib bila RESOLVED.</summary>
    [MaxLength(30)] public string? ResolutionCode { get; set; }

    /// <summary>Catatan petugas Finance; tanpa data pribadi.</summary>
    [MaxLength(1000)] public string? ResolutionNote { get; set; }

    public Guid? ResolvedByUserId { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();
}

public static class FinPurchasingSyncConflictTypes
{
    public const string GoodsReceiptCancelledWithDependents = "GR_CANCELLED_WITH_DEPENDENTS";
    public const string PurchaseOrderCancelledWithDependents = "PO_CANCELLED_WITH_DEPENDENTS";
    public const string NumberCollision = "NUMBER_COLLISION";
    public const string PayloadMismatch = "PAYLOAD_MISMATCH";

    /// <summary>Isi per satuan tidak ada, tidak sah (≤ 0), atau satuan dasar GR berbeda dari item PO (FIN-DEC-276).</summary>
    public const string ConversionUnresolved = "CONVERSION_UNRESOLVED";
}

public static class FinPurchasingSyncConflictStatuses
{
    public const string Open = "OPEN";
    public const string Resolved = "RESOLVED";
}

public static class FinPurchasingSyncConflictResolutionCodes
{
    public const string Adjustment = "ADJUSTMENT";
    public const string Reversal = "REVERSAL";
    public const string SupplierReturn = "SUPPLIER_RETURN";
    public const string SupplierCredit = "SUPPLIER_CREDIT";
    public const string NoFinancialImpact = "NO_FINANCIAL_IMPACT";
}

public static class FinPurchasingSyncConflictDependentTypes
{
    public const string InvoiceExchange = "INVOICE_EXCHANGE";
    public const string PurchasingInvoice = "PURCHASING_INVOICE";
    public const string SupplierPayable = "SUPPLIER_PAYABLE";
    public const string Payment = "PAYMENT";
}
