using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

/// <summary>
/// Aggregate root Batch Tagihan AR (FIN-DES-041, FIN-DEC-048, erd/data-dictionary.md §C.13-C.14).
/// Menggabungkan beberapa FinReceivable milik satu penjamin (DebtorReferenceId sama) menjadi satu
/// dokumen tagihan resmi ke penjamin. BUKAN pengganti FinReceivable — murni lapisan penagihan di
/// atasnya; status batch mengikuti/meringkas status anggotanya, tidak pernah menjadi sumber
/// kebenaran baru untuk pelunasan (FinanceReceivableService tetap satu-satunya penulis
/// OutstandingAmount).
/// </summary>
[Table("FinReceivableInvoiceBatch", Schema = "public")]
public sealed class FinReceivableInvoiceBatch : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)] public string BatchNumber { get; set; } = string.Empty;

    /// <summary>Tetap PAYER pada rilis ini (FIN-DEC-048).</summary>
    [Required, MaxLength(30)] public string DebtorType { get; set; } = FinReceivableInvoiceBatchDebtorTypes.Payer;

    /// <summary>Kunci pengelompokan — sama dengan FinReceivable.DebtorReferenceId anggotanya.</summary>
    public Guid DebtorReferenceId { get; set; }

    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }

    /// <summary>Jumlah OriginalAmount seluruh FinReceivable anggota.</summary>
    public decimal TotalAmount { get; set; }

    [Required, MaxLength(20)] public string Status { get; set; } = FinReceivableInvoiceBatchStatuses.Draft;

    /// <summary>Terisi saat ISSUED.</summary>
    public DateTimeOffset? IssuedAt { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public ICollection<FinReceivableInvoiceBatchItem> Items { get; set; } = new List<FinReceivableInvoiceBatchItem>();
}

public static class FinReceivableInvoiceBatchStatuses
{
    public const string Draft = "DRAFT";
    public const string Issued = "ISSUED";
    public const string PartiallyPaid = "PARTIALLY_PAID";
    public const string Paid = "PAID";
    public const string Cancelled = "CANCELLED";
}

public static class FinReceivableInvoiceBatchDebtorTypes
{
    public const string Payer = "PAYER";
}
