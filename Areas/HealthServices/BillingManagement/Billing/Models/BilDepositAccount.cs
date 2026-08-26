using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

[Table("BilDepositAccount", Schema = "public")]
public sealed class BilDepositAccount : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EncounterId { get; set; }
    [Required, MaxLength(50)] public string AccountNumber { get; set; } = string.Empty;
    public decimal AvailableBalance { get; set; }
    [Required, MaxLength(30)] public string Status { get; set; } = BillingDepositAccountStatuses.Active;
    public Guid RowVersion { get; set; } = Guid.NewGuid();
    public ICollection<BilDepositMovement> Movements { get; set; } = new List<BilDepositMovement>();
}

public static class BillingDepositAccountStatuses
{
    public const string Active = "ACTIVE";
    public const string Closed = "CLOSED";
}
