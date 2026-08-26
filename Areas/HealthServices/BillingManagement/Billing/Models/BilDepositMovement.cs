using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

[Table("BilDepositMovement", Schema = "public")]
public sealed class BilDepositMovement : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DepositAccountId { get; set; }
    [Required, MaxLength(30)] public string MovementType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public Guid? SettlementId { get; set; }
    public Guid? PaymentMethodId { get; set; }
    public Guid? CashierShiftId { get; set; }
    public Guid IdempotencyKey { get; set; }
    [Required, MaxLength(64)] public string PayloadHash { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    public Guid? ReversesMovementId { get; set; }
    public BilDepositAccount DepositAccount { get; set; } = null!;
}

public static class BillingDepositMovementTypes
{
    public const string TopUp = "TOP_UP";
    public const string Allocation = "ALLOCATION";
    public const string Release = "RELEASE";
    public const string Reversal = "REVERSAL";
}
