using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

/// <summary>
/// Baris Purchasing Invoice (FIN-DES-037, 02-backend-architecture.md §C.1-C.2).
/// </summary>
[Table("FinPurchasingInvoiceItem", Schema = "public")]
public sealed class FinPurchasingInvoiceItem : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PurchasingInvoiceId { get; set; }
    public FinPurchasingInvoice? PurchasingInvoice { get; set; }

    [Required, MaxLength(300)] public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    /// <summary>Quantity × UnitPrice — dihitung service saat input, disalin (bukan computed column).</summary>
    public decimal LineTotal { get; set; }
}
