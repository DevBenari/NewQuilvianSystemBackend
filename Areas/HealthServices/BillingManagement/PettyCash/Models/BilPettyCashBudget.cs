using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Models;

/// <summary>
/// Kolam anggaran kas kecil beserta saldo berjalannya (PC-DES-004, PC-DES-014).
/// Pada MVP tepat satu baris berkode HOSPITAL_MAIN (PC-DEC-010).
/// CurrentBalance MUST NOT ditulis di luar PettyCashBudgetService dan MUST NOT negatif.
/// </summary>
[Table("BilPettyCashBudget", Schema = "public")]
public sealed class BilPettyCashBudget : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(30)] public string PoolCode { get; set; } = string.Empty;

    [Required, MaxLength(100)] public string PoolName { get; set; } = string.Empty;

    /// <summary>Saldo berjalan — angka kartu "TOTAL PETTY CASH".</summary>
    public decimal CurrentBalance { get; set; }

    public decimal TotalTopUpAmount { get; set; }

    public decimal TotalDisbursedAmount { get; set; }

    [Required, MaxLength(30)] public string Status { get; set; } = PettyCashBudgetStatuses.Active;

    public DateTimeOffset? LastMovementAt { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public ICollection<BilPettyCashBudgetMovement> Movements { get; set; } = new List<BilPettyCashBudgetMovement>();
}

public static class PettyCashBudgetStatuses
{
    public const string Active = "ACTIVE";
    public const string Inactive = "INACTIVE";
}
