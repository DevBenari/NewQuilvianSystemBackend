namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Dtos;

// ===========================================================================================
// FIN-BE-LGS-LINK-001 — Status penghubung Goods Receipt Finance → Logistik (READ MODEL).
// Seluruhnya DITURUNKAN dari fakta yang sudah ada (data GR Finance dan, bila ada, mutasi stok yang menunjuk GR itu).
// Tidak ada tabel/kolom baru, tidak ada posting stok, dan status ini BUKAN bukti penerimaan Logistik yang sah:
// Goods Receipt Finance tercatat tidak berarti stok bertambah (LGS-CAP-07 / LGS-OQ-017 masih terbuka).
// ===========================================================================================

/// <summary>Kode status penghubung. Label tampilan dimiliki klien; kode ini yang stabil.</summary>
public static class PurchasingLogisticsLinkStatuses
{
    /// <summary>Tidak ada bukti mutasi persediaan yang bersumber dari GR ini (termasuk: integrasi belum tersedia).</summary>
    public const string NotLinked = "NOT_LINKED";

    /// <summary>Ada baris GR bersumber BLM yang kondisinya PENDING_INSPECTION: belum penerimaan sah.</summary>
    public const string AwaitingInspection = "AWAITING_INSPECTION";

    /// <summary>Ada baris rusak atau kuantitas sah lebih kecil dari fisik: perlu ditinjau bersama Logistik.</summary>
    public const string HasDiscrepancy = "HAS_DISCREPANCY";

    /// <summary>Ada mutasi persediaan resmi (StockIn) yang menunjuk GR ini sebagai dokumen sumber.</summary>
    public const string PostedToStock = "POSTED_TO_STOCK";

    /// <summary>GR dibatalkan; tidak ada hubungan yang berlaku.</summary>
    public const string Cancelled = "CANCELLED";
}

/// <summary>Kemampuan yang belum tersedia di sistem sehingga statusnya tidak dapat ditentukan (bukan "0" dan bukan "berhasil").</summary>
public static class PurchasingLogisticsCapabilityStatuses
{
    public const string Unavailable = "UNAVAILABLE";
}

public sealed class GoodsReceiptLogisticsLinkResponse
{
    /// <summary><see cref="PurchasingLogisticsLinkStatuses"/>.</summary>
    public string Status { get; set; } = PurchasingLogisticsLinkStatuses.NotLinked;

    public int ItemCount { get; set; }

    /// <summary>True hanya untuk GR bersumber BLM (kondisi dan kuantitas sah dicatat). GR lokal: jumlah di bawah null (tidak diketahui).</summary>
    public bool ConditionTracked { get; set; }

    public int? PendingInspectionItemCount { get; set; }
    public int? DamagedItemCount { get; set; }

    /// <summary>Baris yang diterima sebagian: kuantitas sah &lt; kuantitas fisik tanpa kondisi rusak/menunggu periksa.</summary>
    public int? PartiallyAcceptedItemCount { get; set; }

    /// <summary>Pemetaan produk BLM→produk Quilvian: belum ada penyimpanan persisten (BLOCKED_BY_SCHEMA).</summary>
    public string ProductMappingStatus { get; set; } = PurchasingLogisticsCapabilityStatuses.Unavailable;

    /// <summary>Konfirmasi penerimaan oleh Logistik: belum ada dokumen/status yang dapat dibaca Finance.</summary>
    public string LogisticsConfirmationStatus { get; set; } = PurchasingLogisticsCapabilityStatuses.Unavailable;

    /// <summary>Jumlah mutasi StockIn yang menunjuk GR (atau barisnya) sebagai dokumen sumber. 0 = tidak ada bukti, bukan bukti ketiadaan.</summary>
    public int PostedMutationCount { get; set; }

    public DateTime? LastPostedAt { get; set; }

    /// <summary>Alasan siap tampil: mengapa status ini, dan apa yang belum tersedia.</summary>
    public List<string> Reasons { get; set; } = [];
}

public sealed class GoodsReceiptLogisticsSummaryQuery
{
    public Guid? SupplierId { get; set; }
    public DateOnly? ReceivedDateFrom { get; set; }
    public DateOnly? ReceivedDateTo { get; set; }
}

public sealed class GoodsReceiptLogisticsUndeterminedCategory
{
    public string Code { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

/// <summary>Ringkasan GR berstatus RECEIVED menurut status penghubung. Kategori yang tidak dapat ditentukan dilaporkan eksplisit, bukan 0.</summary>
public sealed class GoodsReceiptLogisticsSummaryResponse
{
    public int TotalReceived { get; set; }
    public int NotLinked { get; set; }
    public int AwaitingInspection { get; set; }
    public int HasDiscrepancy { get; set; }
    public int PostedToStock { get; set; }

    /// <summary>
    /// Kategori yang butuh data yang belum ada (pemetaan produk, konfirmasi Logistik, kegagalan sinkronisasi). Jumlahnya TIDAK
    /// diketahui; klien wajib menampilkannya sebagai "tidak dapat ditentukan", bukan 0.
    /// </summary>
    public List<GoodsReceiptLogisticsUndeterminedCategory> Undetermined { get; set; } = [];
}
