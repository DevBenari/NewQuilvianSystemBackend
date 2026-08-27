using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

[Table("BilPaymentAllocation", Schema = "public")]
public sealed class BilPaymentAllocation : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SettlementId { get; set; }
    [Required, MaxLength(30)] public string TargetType { get; set; } = BillingAllocationTargetTypes.Invoice;
    public Guid TargetId { get; set; }
    public decimal Amount { get; set; }
    public int? CalculationVersion { get; set; }
    public DateTimeOffset AllocatedAt { get; set; }
    public Guid? ReversesAllocationId { get; set; }
    public BilSettlement Settlement { get; set; } = null!;
}

public static class BillingAllocationTargetTypes
{
    public const string Invoice = "INVOICE";
}
