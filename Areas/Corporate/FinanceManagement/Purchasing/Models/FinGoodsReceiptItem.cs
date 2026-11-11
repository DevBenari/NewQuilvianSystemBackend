using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

/// <summary>
/// Baris Tanda Terima Barang (FIN-DES-037, 02-backend-architecture.md §C.1-C.2). ReceivedQuantity
/// boleh kurang dari Quantity baris PO (penerimaan sebagian) — akumulasinya menentukan status
/// FinPurchaseOrder (PARTIALLY_RECEIVED/FULLY_RECEIVED), ditegakkan FinanceGoodsReceiptService.
/// </summary>
[Table("FinGoodsReceiptItem", Schema = "public")]
public sealed class FinGoodsReceiptItem : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid GoodsReceiptId { get; set; }
    public FinGoodsReceipt? GoodsReceipt { get; set; }

    public Guid PurchaseOrderItemId { get; set; }
    public FinPurchaseOrderItem? PurchaseOrderItem { get; set; }

    public decimal ReceivedQuantity { get; set; }

    /// <summary>Nomor batch barang yang diterima (BE-FIN-125, AP-UAT-06). Opsional; satu baris PO
    /// yang diterima dalam beberapa batch dicatat sebagai beberapa baris GR dengan batch berbeda.</summary>
    [MaxLength(100)] public string? BatchNumber { get; set; }

    /// <summary>Tanggal kedaluwarsa batch (BE-FIN-125, AP-UAT-06). Opsional; hanya dicatat, tidak
    /// divalidasi terhadap tanggal terima karena aturan penolakan barang kedaluwarsa belum diputuskan.</summary>
    public DateOnly? ExpiredDate { get; set; }

    [MaxLength(500)] public string? Notes { get; set; }

    // ---- Penampung data BLM (BE-FIN-142, Revisi 23, FIN-DES-131/140, data-dictionary 13.5) ----
    // Kedelapan kolom di bawah ini semuanya kosong (baris GR lama / GR lokal) atau semuanya terisi
    // (baris GR bersumber BLM). Dokumen lama TIDAK diisi dan tidak di-backfill (FIN-DEC-277).
    // ReceivedQuantity di atas tetap numeric(18,2) dan artinya tidak berubah; untuk baris BLM ia berisi
    // jumlah SAH dalam satuan PO, dibulatkan hanya untuk tampilan. Perhitungan memakai kolom satuan dasar.

    /// <summary><c>ReceivedOrderItemId</c> BLM.</summary>
    public Guid? ExternalReceiveOrderItemId { get; set; }

    /// <summary>Kondisi barang: <see cref="FinGoodsReceiptConditionCodes"/>. Diturunkan dari
    /// <c>Remarks</c> BLM.</summary>
    [MaxLength(30)] public string? ConditionCode { get; set; }

    /// <summary>Kuantitas asli penerimaan (<c>QtyReceive</c> BLM) dalam satuan asli.</summary>
    public decimal? SourceQuantity { get; set; }

    /// <summary>Satuan asli penerimaan (<c>Measure</c> BLM).</summary>
    [MaxLength(30)] public string? SourceUomCode { get; set; }

    /// <summary>Faktor konversi yang dipakai: isi satu satuan asli dalam satuan dasar, snapshot dari BLM.
    /// Tidak pernah bernilai bawaan 1 (FIN-DEC-276).</summary>
    public decimal? QuantityPerBaseUnit { get; set; }

    /// <summary>Kode satuan dasar; harus sama dengan satuan dasar item PO yang ditunjuk (diperiksa pengolah).</summary>
    [MaxLength(30)] public string? BaseUomCode { get; set; }

    /// <summary>Hasil normalisasi: <see cref="SourceQuantity"/> × <see cref="QuantityPerBaseUnit"/> (jumlah fisik).</summary>
    public decimal? BaseQuantity { get; set; }

    /// <summary>Jumlah diterima SAH dalam satuan dasar setelah pemeriksaan kondisi barang.</summary>
    public decimal? AcceptedBaseQuantity { get; set; }
}

/// <summary>Kondisi barang pada baris GR bersumber BLM (FIN-DEC-268, FIN-DES-131). Barang rusak dan
/// menunggu pemeriksaan tidak dihitung sebagai penerimaan sah.</summary>
public static class FinGoodsReceiptConditionCodes
{
    public const string Good = "GOOD";
    public const string Damaged = "DAMAGED";
    public const string PendingInspection = "PENDING_INSPECTION";
}
