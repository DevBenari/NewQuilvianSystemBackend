using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

/// <summary>
/// P04 — NOT ACTIVATED. Alokasi baris invoice ke baris GR (dan lewat GR item ke baris PO): menjawab
/// "kuantitas barang mana yang ditagihkan oleh baris invoice ini". Satu baris invoice dapat dialokasikan
/// ke beberapa baris GR/batch, dan satu baris GR dapat dialokasikan sebagian ke beberapa invoice —
/// karenanya hubungan banyak-ke-banyak dengan kuantitas dan nilai, bukan satu FK.
///
/// Tabel ini belum ada di basis data dan TIDAK dipakai oleh service/controller/query mana pun; lihat
/// <see cref="FinInvoiceExchangeGoodsReceipt"/> untuk alasan isolasi dan syarat aktivasi.
///
/// Perlindungan kuantitas ganda TIDAK dapat dipikul unique index saja (unik pasangan tidak mencegah dua
/// invoice menagih sisa yang sama). Yang wajib saat aktif: kunci per PO yang sama dengan Create/Cancel/
/// Update GR dan Tukar Faktur (<c>FIN_PURCHASE_ORDER_{id}</c>), penjumlahan alokasi aktif per baris GR di
/// dalam kunci itu, dan penolakan bila total melebihi <c>ReceivedQuantity</c>. Aturan murninya ada di
/// <c>PurchasingInvoiceAllocationPlanner</c>.
///
/// Pencocokan baris selalu memakai ID (<see cref="PurchasingInvoiceItemId"/>, <see cref="GoodsReceiptItemId"/>),
/// bukan nama produk (AC-06).
/// </summary>
[Table("FinPurchasingInvoiceItemAllocation", Schema = "public")]
public sealed class FinPurchasingInvoiceItemAllocation : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PurchasingInvoiceItemId { get; set; }
    public FinPurchasingInvoiceItem? PurchasingInvoiceItem { get; set; }

    public Guid GoodsReceiptItemId { get; set; }
    public FinGoodsReceiptItem? GoodsReceiptItem { get; set; }

    /// <summary>Kuantitas GR yang ditagihkan oleh baris invoice ini; selalu positif.</summary>
    public decimal AllocatedQuantity { get; set; }

    /// <summary>Nilai alokasi = AllocatedQuantity × harga satuan baris invoice, dibulatkan 2 desimal. Selalu tidak negatif.</summary>
    public decimal AllocatedAmount { get; set; }
}
