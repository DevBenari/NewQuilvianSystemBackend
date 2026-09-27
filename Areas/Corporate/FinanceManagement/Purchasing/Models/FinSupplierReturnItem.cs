using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

/// <summary>
/// Baris Retur Pembelian (FIN-DES-038, 02-backend-architecture.md §C.1-C.2).
/// </summary>
[Table("FinSupplierReturnItem", Schema = "public")]
public sealed class FinSupplierReturnItem : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SupplierReturnId { get; set; }
    public FinSupplierReturn? SupplierReturn { get; set; }

    [Required, MaxLength(300)] public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }

    /// <summary>Quantity × harga satuan — dihitung service saat input, disalin (bukan computed column).</summary>
    public decimal LineTotal { get; set; }
}
