using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

/// <summary>
/// Tanda Terima Barang (FIN-DES-037, 02-backend-architecture.md §C.1-C.2). PurchaseOrderId
/// WAJIB terisi — DEV_DISCRETION yang didokumentasikan eksplisit (§C.11): tidak ada keputusan
/// yang meminta penerimaan barang tanpa PO mendahului, berbeda dari Tukar Faktur yang eksplisit
/// diizinkan tanpa PO (FIN-DEC-051).
/// </summary>
[Table("FinGoodsReceipt", Schema = "public")]
public sealed class FinGoodsReceipt : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)] public string GRNumber { get; set; } = string.Empty;

    public Guid PurchaseOrderId { get; set; }
    public FinPurchaseOrder? PurchaseOrder { get; set; }

    public DateOnly ReceivedDate { get; set; }

    [Required, MaxLength(20)] public string Status { get; set; } = FinGoodsReceiptStatuses.Received;

    public Guid RowVersion { get; set; } = Guid.NewGuid();

    // ---- Penampung data BLM (BE-FIN-142, Revisi 23, FIN-DES-128, data-dictionary 13.4) ----

    /// <summary>Sistem asal dokumen: <see cref="FinPurchasingSourceSystems"/>. Dokumen lama bernilai QUILVIAN.</summary>
    [Required, MaxLength(20)] public string SourceSystem { get; set; } = FinPurchasingSourceSystems.Quilvian;

    /// <summary><c>ReceiveOrderId</c> milik BLM. Terisi jika dan hanya jika <see cref="SourceSystem"/> = BLM.</summary>
    public Guid? ExternalReceiveOrderId { get; set; }

    /// <summary>Waktu perubahan terakhir menurut BLM.</summary>
    public DateTimeOffset? ExternalUpdatedAt { get; set; }

    /// <summary>Waktu Quilvian terakhir menerapkan kiriman BLM.</summary>
    public DateTimeOffset? LastSyncedAt { get; set; }

    public ICollection<FinGoodsReceiptItem> Items { get; set; } = new List<FinGoodsReceiptItem>();
    public ICollection<FinInvoiceExchange> InvoiceExchanges { get; set; } = new List<FinInvoiceExchange>();
}

public static class FinGoodsReceiptStatuses
{
    public const string Received = "RECEIVED";
    public const string Cancelled = "CANCELLED";
}
