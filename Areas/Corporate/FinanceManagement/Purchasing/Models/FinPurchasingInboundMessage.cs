using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

/// <summary>
/// Kotak masuk kiriman PO/GR dari BLM sekaligus riwayat sinkronisasi (BE-FIN-143, Revisi 23,
/// FIN-DES-132, data-dictionary 13.6). Setiap kiriman disimpan apa adanya SEBELUM diolah, jadi kiriman yang
/// belum bisa diterapkan (pemetaan supplier belum ada, konversi satuan tidak pasti) tidak hilang.
/// Idempotensi lewat unik (SourceSystem, EventId); PayloadHash mendeteksi nomor kejadian sama dengan isi
/// berbeda. Baris tidak pernah dihapus (riwayat audit). Pengolah kiriman belum dibangun (slice integrasi).
/// </summary>
[Table("FinPurchasingInboundMessage", Schema = "public")]
public sealed class FinPurchasingInboundMessage : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Hanya BLM; QUILVIAN tidak masuk kotak masuk (check database).</summary>
    [Required, MaxLength(20)] public string SourceSystem { get; set; } = FinPurchasingSourceSystems.Blm;

    /// <summary>Nomor kejadian dari BLM; dasar idempotensi. Unik per SourceSystem.</summary>
    [Required, MaxLength(100)] public string EventId { get; set; } = string.Empty;

    /// <summary>Kode jenis kejadian dari BLM (usulan PO_UPSERTED, GR_UPSERTED, PO_CANCELLED, GR_CANCELLED).
    /// SENGAJA tanpa check constraint sampai kontrak BLM pasti (FIN-OQ-141), mengikuti EventTypeCode pada
    /// FinAccountingEventOutbox.</summary>
    [Required, MaxLength(50)] public string EventType { get; set; } = string.Empty;

    /// <summary><see cref="FinPurchasingDocumentTypes"/>.</summary>
    [Required, MaxLength(30)] public string DocumentType { get; set; } = string.Empty;

    /// <summary><c>PurchaseOrderId</c> atau <c>ReceiveOrderId</c> BLM. Bukan FK: BLM sistem luar.</summary>
    public Guid ExternalDocumentId { get; set; }

    /// <summary>Nomor dokumen BLM, untuk pencarian petugas.</summary>
    [MaxLength(50)] public string? ExternalDocumentNumber { get; set; }

    /// <summary>Versi data dari BLM bila dikirim; format menunggu kontrak.</summary>
    [MaxLength(50)] public string? SourceVersion { get; set; }

    /// <summary>Waktu kejadian menurut BLM (UTC).</summary>
    public DateTimeOffset SourceOccurredAt { get; set; }

    /// <summary>Waktu Quilvian menerima kiriman (UTC).</summary>
    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Isi kiriman utuh. SENSITIF: dapat memuat identitas dan nama pegawai BLM; MUST NOT masuk
    /// custom logger dan tidak dipakai sebagai contoh berisi data asli.</summary>
    [Required] public string PayloadJson { get; set; } = string.Empty;

    /// <summary>SHA-256 heksadesimal dari <see cref="PayloadJson"/>. EventId sama dengan hash berbeda
    /// menjadi konflik PAYLOAD_MISMATCH.</summary>
    [Required, MaxLength(64)] public string PayloadHash { get; set; } = string.Empty;

    /// <summary><see cref="FinPurchasingInboundProcessingStatuses"/>; transisi di state-transition-matrix W.1.</summary>
    [Required, MaxLength(30)] public string ProcessingStatus { get; set; } = FinPurchasingInboundProcessingStatuses.Received;

    public int AttemptCount { get; set; }
    public DateTimeOffset? LastAttemptAt { get; set; }

    /// <summary>Hanya untuk WAITING_MAPPING (pemetaan supplier belum ada).</summary>
    public DateTimeOffset? NextAttemptAt { get; set; }

    /// <summary>Kode saja (contoh SUPPLIER_NOT_MAPPED, PO_NOT_FOUND, CONVERSION_MISSING, BASE_UOM_MISMATCH),
    /// tanpa teks berisi data pribadi.</summary>
    [MaxLength(50)] public string? LastErrorCode { get; set; }

    /// <summary>Terisi saat APPLIED untuk kiriman PO.</summary>
    public Guid? AppliedPurchaseOrderId { get; set; }
    public FinPurchaseOrder? AppliedPurchaseOrder { get; set; }

    /// <summary>Terisi saat APPLIED untuk kiriman GR.</summary>
    public Guid? AppliedGoodsReceiptId { get; set; }
    public FinGoodsReceipt? AppliedGoodsReceipt { get; set; }

    /// <summary>Di-set ulang Guid.NewGuid() setiap perubahan; mencegah dua pengolah menerapkan kiriman yang sama bersamaan.</summary>
    public Guid RowVersion { get; set; } = Guid.NewGuid();
}

/// <summary>Jenis dokumen BLM pada kotak masuk dan catatan pembatalan.</summary>
public static class FinPurchasingDocumentTypes
{
    public const string PurchaseOrder = "PURCHASE_ORDER";
    public const string GoodsReceipt = "GOODS_RECEIPT";
}

/// <summary>Status pengolahan kiriman (state-transition-matrix W.1). APPLIED, SUPERSEDED, dan REJECTED adalah status akhir.</summary>
public static class FinPurchasingInboundProcessingStatuses
{
    public const string Received = "RECEIVED";
    public const string Applied = "APPLIED";
    public const string WaitingMapping = "WAITING_MAPPING";
    public const string Conflict = "CONFLICT";
    public const string Rejected = "REJECTED";
    public const string Superseded = "SUPERSEDED";
}
