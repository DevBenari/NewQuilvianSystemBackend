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

    /// <summary>TIER_1 atau TIER_2, diisi FinanceApprovalTierResolver.Resolve (FIN-DEC-052). PO bersumber BLM
    /// bernilai EXTERNAL (FIN-DES-130): persetujuannya terjadi di BLM, bukan di Quilvian.</summary>
    [Required, MaxLength(10)] public string ApprovalTier { get; set; } = string.Empty;

    public Guid RequestedByUserId { get; set; }
    public DateTimeOffset RequestedAt { get; set; }

    public Guid? ApprovedByUserId { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();

    // ---- Penampung data BLM (BE-FIN-142, Revisi 23, FIN-DES-128/130, data-dictionary 13.2) ----

    /// <summary>Sistem asal dokumen: <see cref="FinPurchasingSourceSystems"/>. Dokumen lama dan dokumen
    /// yang dibuat di Quilvian bernilai QUILVIAN. Dokumen BLM tidak boleh dibuat, diubah, atau dibatalkan
    /// secara lokal (FIN-DEC-265, ditegakkan layanan pada BE-FIN-146).</summary>
    [Required, MaxLength(20)] public string SourceSystem { get; set; } = FinPurchasingSourceSystems.Quilvian;

    /// <summary><c>PurchaseOrderId</c> milik BLM. Terisi jika dan hanya jika <see cref="SourceSystem"/> = BLM
    /// (check database). Bukan kunci utama dan bukan FK: BLM sistem luar.</summary>
    public Guid? ExternalPurchaseOrderId { get; set; }

    /// <summary>Waktu perubahan terakhir menurut BLM; dipakai menolak kiriman yang lebih lama.</summary>
    public DateTimeOffset? ExternalUpdatedAt { get; set; }

    /// <summary>Waktu Quilvian terakhir menerapkan kiriman BLM.</summary>
    public DateTimeOffset? LastSyncedAt { get; set; }

    public ICollection<FinPurchaseOrderItem> Items { get; set; } = new List<FinPurchaseOrderItem>();
    public ICollection<FinGoodsReceipt> GoodsReceipts { get; set; } = new List<FinGoodsReceipt>();
}

/// <summary>Sistem asal dokumen Purchasing (FIN-DES-128). Kelas konstanta, bukan enum, mengikuti
/// <see cref="FinPurchaseOrderStatuses"/>.</summary>
public static class FinPurchasingSourceSystems
{
    public const string Quilvian = "QUILVIAN";
    public const string Blm = "BLM";
}

/// <summary>Jenjang persetujuan tambahan untuk PO bersumber BLM (FIN-DES-130). TIER_1 dan TIER_2 tetap
/// milik <c>FinanceApprovalTierResolver</c>; EXTERNAL berarti persetujuan terjadi di BLM, bukan di Quilvian.</summary>
public static class FinPurchaseOrderExternalApprovalTier
{
    public const string External = "EXTERNAL";
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
