using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

/// <summary>
/// P04 — NOT ACTIVATED. Relasi bundel Tukar Faktur ↔ Tanda Terima Barang: satu faktur supplier yang
/// mencakup beberapa GR dari SATU PO menghasilkan SATU invoice dan SATU utang, bukan utang per GR.
///
/// Tabel ini belum ada di basis data dan TIDAK dipakai oleh service/controller/query mana pun. Tidak ada
/// DbSet, tidak ada migration, dan tidak ada kolom baru pada <see cref="FinInvoiceExchange"/> (menambah
/// kolom pada entity yang sedang dipakai akan membuat setiap query existing memilih kolom yang belum
/// ada). Aktivasinya menunggu tiga hal: schema diterapkan pemilik, kebijakan basis tanggal jatuh tempo
/// multi-GR disahkan, dan penyelesai hubungan kanonik ditulis (lihat spesifikasi schema P04).
///
/// Representasi kanonik hubungan GR bila sudah aktif: himpunan GR sebuah Tukar Faktur = baris tabel ini
/// ∪ kolom lama <see cref="FinInvoiceExchange.GoodsReceiptId"/> (dibaca sebagai bundel beranggota satu),
/// diselesaikan di SATU tempat, supaya tidak ada dua sumber hubungan yang dapat berbeda.
///
/// Cakupan awal yang diusulkan: satu supplier, satu PO, beberapa GR aktif. Itu batas desain awal, bukan
/// kebijakan lintas-PO yang sudah disahkan.
/// </summary>
[Table("FinInvoiceExchangeGoodsReceipt", Schema = "public")]
public sealed class FinInvoiceExchangeGoodsReceipt : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid InvoiceExchangeId { get; set; }
    public FinInvoiceExchange? InvoiceExchange { get; set; }

    public Guid GoodsReceiptId { get; set; }
    public FinGoodsReceipt? GoodsReceipt { get; set; }
}
