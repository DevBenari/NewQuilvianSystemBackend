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

    [MaxLength(500)] public string? Notes { get; set; }
}
