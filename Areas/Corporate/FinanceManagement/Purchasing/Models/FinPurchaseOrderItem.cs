using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

/// <summary>
/// Baris Purchase Order (FIN-DES-037, 02-backend-architecture.md §C.1-C.2). ProductCategory dan
/// ProductName teks bebas — sengaja tidak merujuk katalog produk/master (§C.11: tidak ada
/// permintaan eksplisit katalog produk terstruktur).
/// </summary>
[Table("FinPurchaseOrderItem", Schema = "public")]
public sealed class FinPurchaseOrderItem : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PurchaseOrderId { get; set; }
    public FinPurchaseOrder? PurchaseOrder { get; set; }

    [Required, MaxLength(100)] public string ProductCategory { get; set; } = string.Empty;
    [Required, MaxLength(300)] public string ProductName { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string Unit { get; set; } = string.Empty;

    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    /// <summary>Quantity × UnitPrice — dihitung service saat input, disalin (bukan computed column).</summary>
    public decimal LineTotal { get; set; }

    // ---- Penampung data BLM (BE-FIN-142, Revisi 23, data-dictionary 13.3) ----
    // Enam kolom pertama di bawah ini semuanya kosong (item lama) atau semuanya terisi (item BLM);
    // ExternalProductCode boleh kosong karena BLM tidak selalu punya kode produk eksternal.
    // Nama, kategori, satuan, dan kuantitas di atas adalah SNAPSHOT saat PO diterima dari BLM
    // (FIN-DEC-275). Tidak ada tabel pemetaan produk (FIN-DEC-274).

    /// <summary><c>PurchaseOrderItemId</c> BLM; satu-satunya kunci pencocokan baris GR ke item PO
    /// (FIN-DEC-275: dilarang mencocokkan lewat nama produk).</summary>
    public Guid? ExternalPurchaseOrderItemId { get; set; }

    /// <summary><c>ProductId</c> BLM.</summary>
    public Guid? ExternalProductId { get; set; }

    /// <summary><c>ProductExtCode</c> BLM bila ada.</summary>
    [MaxLength(100)] public string? ExternalProductCode { get; set; }

    /// <summary>Kode satuan PO asli dari BLM (contoh BOX).</summary>
    [MaxLength(30)] public string? SourceUomCode { get; set; }

    /// <summary>Isi satu satuan PO dalam satuan dasar produk, snapshot dari konversi BLM
    /// (<c>MstProductConversion.QuantityPerUnit</c>; satuan dasar = 1). Tidak pernah diisi 1 sebagai
    /// tebakan (FIN-DEC-276).</summary>
    public decimal? QuantityPerBaseUnit { get; set; }

    /// <summary>Kode satuan dasar produk menurut BLM (contoh PCS).</summary>
    [MaxLength(30)] public string? BaseUomCode { get; set; }

    /// <summary><see cref="Quantity"/> × <see cref="QuantityPerBaseUnit"/>, dihitung saat PO diterapkan.
    /// Presisi 6 desimal supaya tidak ada pembulatan dini.</summary>
    public decimal? BaseQuantity { get; set; }

    public ICollection<FinGoodsReceiptItem> GoodsReceiptItems { get; set; } = new List<FinGoodsReceiptItem>();
}
