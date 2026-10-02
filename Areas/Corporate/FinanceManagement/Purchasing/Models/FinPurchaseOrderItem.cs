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

    public ICollection<FinGoodsReceiptItem> GoodsReceiptItems { get; set; } = new List<FinGoodsReceiptItem>();
}
