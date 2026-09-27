using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

/// <summary>
/// Tukar Faktur (FIN-DES-037, FIN-DEC-051, 02-backend-architecture.md §C.1-C.2). Checkpoint
/// serah-terima dokumen fisik faktur dari supplier, terjadi lebih dulu dari Purchasing Invoice.
/// PurchaseOrderId dan GoodsReceiptId sengaja nullable — boleh berasal dari PO/GR maupun berdiri
/// sendiri (FIN-DEC-051). EstimatedDueDate dihitung service dari MstSupplier.PaymentTermDays,
/// bukan dari nilai request (FinanceInvoiceExchangeService, BE-FIN-033).
/// </summary>
[Table("FinInvoiceExchange", Schema = "public")]
public sealed class FinInvoiceExchange : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)] public string ExchangeNumber { get; set; } = string.Empty;

    public Guid SupplierId { get; set; }
    public MstSupplier? Supplier { get; set; }

    public Guid? PurchaseOrderId { get; set; }
    public FinPurchaseOrder? PurchaseOrder { get; set; }

    public Guid? GoodsReceiptId { get; set; }
    public FinGoodsReceipt? GoodsReceipt { get; set; }

    [Required, MaxLength(100)] public string SupplierInvoiceNumber { get; set; } = string.Empty;
    public DateOnly SupplierInvoiceDate { get; set; }
    public DateOnly ReceivedDate { get; set; }

    /// <summary>ReceivedDate + MstSupplier.PaymentTermDays, dihitung backend (FIN-DES-037).</summary>
    public DateOnly EstimatedDueDate { get; set; }

    [Required, MaxLength(20)] public string Status { get; set; } = FinInvoiceExchangeStatuses.Received;

    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public FinPurchasingInvoice? PurchasingInvoice { get; set; }
}

public static class FinInvoiceExchangeStatuses
{
    public const string Received = "RECEIVED";
    public const string LinkedToInvoice = "LINKED_TO_INVOICE";
    public const string Cancelled = "CANCELLED";
}
