using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

/// <summary>
/// Audit pembatalan PO/GR lintas sistem sekaligus antrean permintaan batal Quilvian ke BLM (BE-FIN-143,
/// Revisi 23, FIN-DES-137, data-dictionary 13.10). Untuk permintaan dari Quilvian (OUTBOUND_REQUEST), baris
/// REQUESTED ditulis di transaksi yang sama dengan pemeriksaan ketergantungan AP, sehingga tidak ada
/// permintaan yang hilang karena gagal kirim. Status PO/GR di Quilvian baru menjadi CANCELLED setelah
/// CONFIRMED — tidak ada keberhasilan semu (FIN-DEC-262).
/// </summary>
[Table("FinPurchasingExternalCancellation", Schema = "public")]
public sealed class FinPurchasingExternalCancellation : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(20)] public string SourceSystem { get; set; } = FinPurchasingSourceSystems.Blm;

    /// <summary><see cref="FinPurchasingCancellationDirections"/>.</summary>
    [Required, MaxLength(20)] public string Direction { get; set; } = string.Empty;

    /// <summary><see cref="FinPurchasingDocumentTypes"/>.</summary>
    [Required, MaxLength(30)] public string DocumentType { get; set; } = string.Empty;

    /// <summary><c>PurchaseOrderId</c> atau <c>ReceiveOrderId</c> BLM. Bukan FK.</summary>
    public Guid ExternalDocumentId { get; set; }

    /// <summary>Wajib bila DocumentType = PURCHASE_ORDER (check database).</summary>
    public Guid? PurchaseOrderId { get; set; }
    public FinPurchaseOrder? PurchaseOrder { get; set; }

    /// <summary>Wajib bila DocumentType = GOODS_RECEIPT (check database).</summary>
    public Guid? GoodsReceiptId { get; set; }
    public FinGoodsReceipt? GoodsReceipt { get; set; }

    /// <summary>Untuk OUTBOUND_REQUEST dibuat Quilvian; untuk INBOUND_NOTICE dari BLM. Unik per (sumber, arah).</summary>
    [Required, MaxLength(100)] public string EventId { get; set; } = string.Empty;

    /// <summary>Sejajar PchHistoryCancelPurchaseOrder.CategoryCode di BLM. Panjang kolom adalah asumsi
    /// sampai kontrak BLM diterima (FIN-OQ-141).</summary>
    [Required, MaxLength(50)] public string CategoryCode { get; set; } = string.Empty;

    [Required, MaxLength(100)] public string CategoryName { get; set; } = string.Empty;

    /// <summary>Diisi bila kategori "lainnya".</summary>
    [MaxLength(300)] public string? OtherCategoryText { get; set; }

    /// <summary>Alasan batal; tanpa data pribadi.</summary>
    [Required, MaxLength(1000)] public string Reason { get; set; } = string.Empty;

    /// <summary>Pengguna Quilvian; wajib untuk OUTBOUND_REQUEST (check database).</summary>
    public Guid? RequestedByUserId { get; set; }

    /// <summary>Identitas pelaku menurut BLM (CancelOrderById). SENSITIF: tidak masuk custom logger.</summary>
    [MaxLength(100)] public string? CancelledBy { get; set; }

    /// <summary>Wajib bila Status = CONFIRMED (check database).</summary>
    public DateTimeOffset? CancelledAt { get; set; }

    /// <summary><see cref="FinPurchasingCancellationStatuses"/>. INBOUND_NOTICE langsung CONFIRMED.</summary>
    [Required, MaxLength(20)] public string Status { get; set; } = string.Empty;

    /// <summary>Kode penolakan BLM, atau AP_DEPENDENCY bila ditolak pemeriksaan Quilvian sendiri.</summary>
    [MaxLength(50)] public string? RejectionCode { get; set; }

    public int AttemptCount { get; set; }

    /// <summary>Hanya untuk REQUESTED/FAILED.</summary>
    public DateTimeOffset? NextAttemptAt { get; set; }

    /// <summary>Kiriman BLM yang mengonfirmasi atau memberi tahu pembatalan.</summary>
    public Guid? InboundMessageId { get; set; }
    public FinPurchasingInboundMessage? InboundMessage { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();
}

public static class FinPurchasingCancellationDirections
{
    /// <summary>Quilvian meminta BLM membatalkan.</summary>
    public const string OutboundRequest = "OUTBOUND_REQUEST";

    /// <summary>BLM memberi tahu pembatalan yang dilakukan di BLM.</summary>
    public const string InboundNotice = "INBOUND_NOTICE";
}

/// <summary>Status pembatalan eksternal (state-transition-matrix W.3). CONFIRMED dan REJECTED adalah status akhir.</summary>
public static class FinPurchasingCancellationStatuses
{
    public const string Requested = "REQUESTED";
    public const string Sent = "SENT";
    public const string Confirmed = "CONFIRMED";
    public const string Rejected = "REJECTED";
    public const string Failed = "FAILED";
}
