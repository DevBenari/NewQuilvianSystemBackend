using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Dtos;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.LogisticsManagement.Receiving.Dtos;

/// <summary>
/// LGS-BE-REC-INSP-001 — kontrak BACA Logistik untuk "Pemeriksaan Hasil Pengadaan". Seluruh isinya diturunkan dari dokumen
/// Purchase Order dan Goods Receipt Finance yang sudah ada; tidak ada tabel, kolom, atau status pemeriksaan baru, dan tidak ada
/// nilai finansial (harga, total) yang dikirim.
/// </summary>
public static class ProcurementInspectionConditionFilters
{
    public const string Good = "GOOD";
    public const string Damaged = "DAMAGED";
    public const string PendingInspection = "PENDING_INSPECTION";

    /// <summary>Baris GR yang tidak membawa kode kondisi (GR lokal/legacy). BUKAN GOOD; tidak pernah diasumsikan baik.</summary>
    public const string NotRecorded = "NOT_RECORDED";

    public static readonly IReadOnlyList<string> All = [Good, Damaged, PendingInspection, NotRecorded];
}

/// <summary>Keadaan kuantitas satu baris. Selain RESOLVED, besaran turunan (sah, menunggu periksa, rusak) bernilai null — bukan 0.</summary>
public static class ProcurementInspectionQuantityStates
{
    /// <summary>Metadata kondisi ada dan konversi ke satuan dasar pasti; angka mengikuti PurchasingReceiptQuantityCalculator.</summary>
    public const string Resolved = "RESOLVED";

    /// <summary>Metadata kondisi ada tetapi konversi satuan/kuantitas tidak pasti; jumlah sah tidak dihitung dan tidak ditebak.</summary>
    public const string Unresolved = "UNRESOLVED";

    /// <summary>Tidak ada metadata pemeriksaan (GR lokal/legacy). Jumlah sah dan kondisi tidak diketahui.</summary>
    public const string NotRecorded = "NOT_RECORDED";

    /// <summary>GR dibatalkan di Finance; tidak ada jumlah yang dihitung.</summary>
    public const string Cancelled = "CANCELLED";
}

/// <summary>Satuan acuan seluruh kuantitas pada satu baris (satu satuan per baris; tidak pernah dicampur).</summary>
public static class ProcurementInspectionQuantityBases
{
    public const string BaseUnit = "BASE_UNIT";
    public const string PurchaseOrderUnit = "PURCHASE_ORDER_UNIT";
}

public sealed class ProcurementInspectionQuery
{
    public DateOnly? ReceivedDateFrom { get; set; }
    public DateOnly? ReceivedDateTo { get; set; }

    public Guid? SupplierId { get; set; }

    /// <summary>GOOD | DAMAGED | PENDING_INSPECTION | NOT_RECORDED. NOT_RECORDED = baris tanpa kode kondisi.</summary>
    [MaxLength(30)] public string? ConditionCode { get; set; }

    /// <summary>QUILVIAN | BLM.</summary>
    [MaxLength(20)] public string? SourceSystem { get; set; }

    /// <summary>RECEIVED | CANCELLED (status DOKUMEN GR, bukan hasil pemeriksaan fisik).</summary>
    [MaxLength(20)] public string? DocumentStatus { get; set; }

    /// <summary>
    /// Pencarian bebas (mengandung, tanpa membedakan huruf besar/kecil) pada nama barang, nomor Goods Receipt, atau nomor Purchase Order.
    /// Satu kotak cari menggantikan tiga saringan teks terpisah supaya halaman memakai kontrol saringan standar.
    /// </summary>
    [MaxLength(200)] public string? Search { get; set; }

    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed class ProcurementInspectionSupplierOptionsQuery
{
    [MaxLength(120)] public string? Search { get; set; }
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed class ProcurementInspectionSupplierOptionResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
}

/// <summary>
/// Satu baris pemeriksaan = satu baris Goods Receipt. Semua kuantitas dinyatakan dalam <see cref="QuantityUnit"/> (satu satuan per baris).
/// Besaran yang tidak dapat dipastikan bernilai null, bukan 0.
/// </summary>
public class ProcurementInspectionItemResponse
{
    public Guid GoodsReceiptId { get; set; }
    public Guid GoodsReceiptItemId { get; set; }
    public string GoodsReceiptNumber { get; set; } = string.Empty;
    public DateOnly ReceivedDate { get; set; }

    /// <summary>Status DOKUMEN GR (RECEIVED/CANCELLED). Bukan hasil pemeriksaan fisik.</summary>
    public string DocumentStatus { get; set; } = string.Empty;

    /// <summary>QUILVIAN | BLM.</summary>
    public string SourceSystem { get; set; } = string.Empty;

    public Guid PurchaseOrderId { get; set; }
    public string PurchaseOrderNumber { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public string? SupplierName { get; set; }

    public Guid PurchaseOrderItemId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCategory { get; set; } = string.Empty;

    /// <summary>Kode satuan acuan baris ini: satuan dasar (BLM) atau satuan PO (GR lokal).</summary>
    public string? QuantityUnit { get; set; }

    /// <summary><see cref="ProcurementInspectionQuantityBases"/>.</summary>
    public string? QuantityBasis { get; set; }

    /// <summary><see cref="ProcurementInspectionQuantityStates"/>.</summary>
    public string QuantityState { get; set; } = ProcurementInspectionQuantityStates.NotRecorded;

    /// <summary>Penjelasan singkat bila <see cref="QuantityState"/> bukan RESOLVED. Null bila tidak ada yang perlu dijelaskan.</summary>
    public string? QuantityNote { get; set; }

    public decimal? OrderedQuantity { get; set; }
    public decimal? PhysicalQuantity { get; set; }

    /// <summary>Jumlah diterima sah. Hanya baris GOOD; barang rusak dan menunggu periksa tidak termasuk (FIN-DEC-268).</summary>
    public decimal? AcceptedQuantity { get; set; }

    public decimal? PendingInspectionQuantity { get; set; }
    public decimal? DamagedQuantity { get; set; }

    /// <summary>Kuantitas dan satuan asli dari sistem sumber (BLM). Null untuk GR lokal.</summary>
    public decimal? SourceQuantity { get; set; }
    public string? SourceUomCode { get; set; }

    /// <summary>Kode kondisi dari sistem sumber; null bila tidak ada. Null TIDAK berarti GOOD.</summary>
    public string? ConditionCode { get; set; }

    /// <summary>GOOD | DAMAGED | PENDING_INSPECTION | NOT_RECORDED.</summary>
    public string ConditionStatus { get; set; } = ProcurementInspectionConditionFilters.NotRecorded;

    /// <summary>True bila baris membawa metadata pemeriksaan dari sistem sumber.</summary>
    public bool InspectionDataAvailable { get; set; }

    public string? BatchNumber { get; set; }
    public DateOnly? ExpiredDate { get; set; }
    public string? Notes { get; set; }
}

/// <summary>Baris detail: sama dengan baris daftar, ditambah kemajuan kumulatif item PO (lintas GR aktif). Null untuk PO non-BLM.</summary>
public sealed class ProcurementInspectionDetailItemResponse : ProcurementInspectionItemResponse
{
    /// <summary>Jumlah sah kumulatif item PO ini dari seluruh GR aktif (satuan dasar). Null bila tidak dapat dipastikan.</summary>
    public decimal? CumulativeAcceptedQuantity { get; set; }

    /// <summary>Sisa terhadap jumlah dipesan (satuan dasar). Null bila tidak dapat dipastikan.</summary>
    public decimal? RemainingQuantity { get; set; }

    /// <summary>Satuan dasar untuk dua besaran kumulatif di atas.</summary>
    public string? CumulativeUnit { get; set; }
}

public sealed class ProcurementInspectionDetailResponse
{
    public Guid GoodsReceiptId { get; set; }
    public string GoodsReceiptNumber { get; set; } = string.Empty;
    public DateOnly ReceivedDate { get; set; }
    public string DocumentStatus { get; set; } = string.Empty;
    public string SourceSystem { get; set; } = string.Empty;
    public DateTimeOffset? LastSyncedAt { get; set; }

    public Guid PurchaseOrderId { get; set; }
    public string PurchaseOrderNumber { get; set; } = string.Empty;
    public string PurchaseOrderStatus { get; set; } = string.Empty;
    public string PurchaseOrderSourceSystem { get; set; } = string.Empty;

    public Guid SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public string? SupplierCode { get; set; }

    /// <summary>
    /// Status penghubung GR ke persediaan Logistik (read-only, diturunkan dari mutasi stok bersumber GR). Pemeriksaan di halaman ini
    /// TIDAK menambah stok; ini hanya menunjukkan apakah ada bukti penerimaan stok yang menunjuk GR tersebut.
    /// </summary>
    public GoodsReceiptLogisticsLinkResponse? StockLink { get; set; }

    public IReadOnlyList<ProcurementInspectionDetailItemResponse> Items { get; set; } = [];
}

/// <summary>
/// Ringkasan menurut saringan yang sama dengan daftar. Seluruhnya hitungan item/dokumen (bukan jumlah kuantitas), karena kuantitas
/// lintas barang berbeda satuan tidak bermakna. Hanya dokumen RECEIVED yang masuk hitungan item; GR dibatalkan dihitung terpisah.
/// </summary>
public sealed class ProcurementInspectionSummaryResponse
{
    public int TotalDocuments { get; set; }
    public int CancelledDocuments { get; set; }
    public int TotalItems { get; set; }
    public int GoodItems { get; set; }
    public int PendingInspectionItems { get; set; }
    public int DamagedItems { get; set; }

    /// <summary>Item tanpa kode kondisi (GR lokal/legacy). Bukan item baik.</summary>
    public int ConditionNotRecordedItems { get; set; }
}
