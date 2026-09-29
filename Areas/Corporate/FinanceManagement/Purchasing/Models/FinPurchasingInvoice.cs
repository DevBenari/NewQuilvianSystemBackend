using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

/// <summary>
/// Aggregate root Purchasing Invoice (FIN-DES-037, FIN-DEC-045, FIN-DEC-051,
/// 02-backend-architecture.md §C.1-C.2). Tepat satu invoice per FinInvoiceExchange
/// (UNIQUE InvoiceExchangeId, FIN-DEC-051). Saat Approved: FinanceSupplierPayableService
/// membuat FinSupplierPayable (SourcePurchasingInvoiceId terisi, BE-FIN-034) dan outbox
/// PPN-MASUKAN-PEMBELIAN ditulis PENDING sebesar PPNAmount (FIN-DES-043, worker tertahan
/// FIN-OQ-020/FIN-DEC-056).
///
/// Invariant: TotalAmount = SubtotalAmount - DiscountAmount + PPNAmount - DownPaymentAmount
/// - OtherDeductionAmount (FIN-VAL-107).
/// </summary>
[Table("FinPurchasingInvoice", Schema = "public")]
public sealed class FinPurchasingInvoice : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)] public string InvoiceNumber { get; set; } = string.Empty;

    public Guid InvoiceExchangeId { get; set; }
    public FinInvoiceExchange? InvoiceExchange { get; set; }

    public Guid SupplierId { get; set; }
    public MstSupplier? Supplier { get; set; }

    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; } = 0m;

    /// <summary>Pajak Masukan (FIN-DES-043) — kejadian PPN-MASUKAN-PEMBELIAN ditulis sebesar ini.</summary>
    public decimal PPNAmount { get; set; } = 0m;
    public decimal DownPaymentAmount { get; set; } = 0m;
    public decimal OtherDeductionAmount { get; set; } = 0m;
    public decimal TotalAmount { get; set; }

    [Required, MaxLength(30)] public string Status { get; set; } = FinPurchasingInvoiceStatuses.Draft;

    /// <summary>TIER_1 atau TIER_2, diisi FinanceApprovalTierResolver.Resolve (FIN-DEC-052).</summary>
    [Required, MaxLength(10)] public string ApprovalTier { get; set; } = string.Empty;

    public Guid RequestedByUserId { get; set; }
    public DateTimeOffset RequestedAt { get; set; }

    public Guid? ApprovedByUserId { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public ICollection<FinPurchasingInvoiceItem> Items { get; set; } = new List<FinPurchasingInvoiceItem>();
}

public static class FinPurchasingInvoiceStatuses
{
    public const string Draft = "DRAFT";
    public const string PendingApproval = "PENDING_APPROVAL";
    public const string Approved = "APPROVED";
    public const string Rejected = "REJECTED";
    public const string Cancelled = "CANCELLED";
}
