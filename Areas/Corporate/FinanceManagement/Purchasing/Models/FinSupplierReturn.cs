using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

/// <summary>
/// Aggregate root Retur Pembelian (FIN-DES-038, FIN-DEC-047, 02-backend-architecture.md §C.1-C.2).
/// Saat Confirmed, FinanceSupplierReturnService menerbitkan satu FinSupplierReturnDeposit sebesar
/// TotalAmount dalam transaksi yang sama (FIN-DEC-047) — retur TIDAK mengubah FinSupplierPayable
/// apa pun secara langsung; efeknya ke utang lewat pemakaian deposit pada FinPayment (BE-FIN-036).
/// </summary>
[Table("FinSupplierReturn", Schema = "public")]
public sealed class FinSupplierReturn : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)] public string ReturnNumber { get; set; } = string.Empty;

    public Guid PurchasingInvoiceId { get; set; }
    public FinPurchasingInvoice? PurchasingInvoice { get; set; }

    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }

    [Required, MaxLength(20)] public string Status { get; set; } = FinSupplierReturnStatuses.Draft;

    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public ICollection<FinSupplierReturnItem> Items { get; set; } = new List<FinSupplierReturnItem>();
    public FinSupplierReturnDeposit? Deposit { get; set; }
}

public static class FinSupplierReturnStatuses
{
    public const string Draft = "DRAFT";
    public const string Confirmed = "CONFIRMED";
    public const string Cancelled = "CANCELLED";
}
