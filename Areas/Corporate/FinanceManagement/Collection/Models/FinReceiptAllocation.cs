using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Models;

/// <summary>
/// Satu-satunya jembatan penerimaan ke piutang (02-backend-architecture.md §4.8). Menjelaskan uang
/// pada satu FinReceipt dipakai untuk melunasi apa — inilah yang mencegah satu rupiah dihitung dua
/// kali (FR-FIN-035). ReceivableId boleh kosong: itu keadaan SAH untuk pasien yang bayar lunas
/// tanpa pernah punya piutang (FIN-DES-011). Pembalikan membuat baris baru IsReversal = true
/// (FIN-DES-012) — baris lama tetap utuh, tidak pernah dihapus atau diubah.
///
/// Belum ada penulis pada BE-FIN-016 — pembagian bayar-vs-piutang adalah tanggung jawab
/// FinanceReceiptService (BE-FIN-017, BLOCKED). Model dan tabel disiapkan lebih dulu supaya
/// FK FinReceipt/FinReceivable dapat dibangun bersamaan.
/// </summary>
[Table("FinReceiptAllocation", Schema = "public")]
public sealed class FinReceiptAllocation : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ReceiptId { get; set; }
    public FinReceipt? Receipt { get; set; }

    /// <summary>Kosong SAH bila uang langsung melunasi tagihan Billing, bukan piutang Finance.</summary>
    public Guid? ReceivableId { get; set; }
    public FinReceivable? Receivable { get; set; }

    /// <summary>Rujukan BilPaymentAllocation asal — bukan FK, lintas bounded context.</summary>
    public Guid? SourceAllocationId { get; set; }

    [Required, MaxLength(30)] public string TargetType { get; set; } = string.Empty;

    /// <summary>Selalu positif.</summary>
    public decimal Amount { get; set; }

    public bool IsReversal { get; set; } = false;

    /// <summary>Terisi hanya bila IsReversal = true.</summary>
    public Guid? ReversalOfAllocationId { get; set; }

    public Guid AllocatedBy { get; set; }
    public DateTimeOffset AllocatedAt { get; set; }
}

public static class FinReceiptAllocationTargetTypes
{
    public const string Receivable = "RECEIVABLE";
    public const string InvoiceDirect = "INVOICE_DIRECT";
}
