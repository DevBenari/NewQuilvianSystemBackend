using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Models;

/// <summary>
/// Rincian satu invoice supplier, diisi bersamaan saat utang diinput (02-backend-architecture.md
/// §4.10). Jumlah seluruh Amount baris MUST sama dengan OriginalAmount induknya — ditegakkan
/// FinanceSupplierPayableService di dalam SaveChangesAsync yang sama, bukan check constraint
/// (invariant lintas baris tidak dapat dinyatakan sebagai table check constraint di Postgres).
/// </summary>
[Table("FinSupplierPayableItem", Schema = "public")]
public sealed class FinSupplierPayableItem : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PayableId { get; set; }
    public FinSupplierPayable? Payable { get; set; }

    [Required, MaxLength(300)] public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1m;
    public decimal UnitPrice { get; set; }

    /// <summary>Quantity × UnitPrice — dihitung service saat input, disalin (bukan computed column).</summary>
    public decimal Amount { get; set; }
}
