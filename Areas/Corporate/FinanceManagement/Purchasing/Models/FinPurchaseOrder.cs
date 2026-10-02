using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

/// <summary>
/// Aggregate root Purchase Order (FIN-DES-037, FIN-DEC-045, 02-backend-architecture.md §C.1-C.2).
/// Awal siklus Purchasing/AP: PO → Tanda Terima Barang → Tukar Faktur → Purchasing Invoice
/// (FIN-DEC-051). ApprovalTier dihitung FinanceApprovalTierResolver (BE-FIN-028) dari TotalAmount,
/// ambang Rp 50.000.000 (FIN-DEC-052) — pola persis FinPayment.
/// </summary>
[Table("FinPurchaseOrder", Schema = "public")]
public sealed class FinPurchaseOrder : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)] public string PONumber { get; set; } = string.Empty;

    /// <summary>Milik Administrator (FIN-DEC-014) — Finance MUST NOT membuat master Supplier baru.</summary>
    public Guid SupplierId { get; set; }
    public MstSupplier? Supplier { get; set; }

    [Required, MaxLength(30)] public string Status { get; set; } = FinPurchaseOrderStatuses.Draft;

    public decimal TotalAmount { get; set; }

    /// <summary>TIER_1 atau TIER_2, diisi FinanceApprovalTierResolver.Resolve (FIN-DEC-052).</summary>
    [Required, MaxLength(10)] public string ApprovalTier { get; set; } = string.Empty;

    public Guid RequestedByUserId { get; set; }
    public DateTimeOffset RequestedAt { get; set; }

    public Guid? ApprovedByUserId { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public ICollection<FinPurchaseOrderItem> Items { get; set; } = new List<FinPurchaseOrderItem>();
    public ICollection<FinGoodsReceipt> GoodsReceipts { get; set; } = new List<FinGoodsReceipt>();
}

public static class FinPurchaseOrderStatuses
{
    public const string Draft = "DRAFT";
    public const string PendingApproval = "PENDING_APPROVAL";
    public const string Approved = "APPROVED";
    public const string Rejected = "REJECTED";
    public const string Cancelled = "CANCELLED";
    public const string PartiallyReceived = "PARTIALLY_RECEIVED";
    public const string FullyReceived = "FULLY_RECEIVED";
    public const string Closed = "CLOSED";
}
