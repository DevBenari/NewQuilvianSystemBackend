using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

[Table("BilDepositOrderPayment", Schema = "public")]
public sealed class BilDepositOrderPayment : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid DepositOrderId { get; set; }
    public Guid DepositMovementId { get; set; }
    public Guid? SettlementId { get; set; }

    public decimal Amount { get; set; }

    [Required, MaxLength(30)]
    public string Status { get; set; } = DepositOrderPaymentStatuses.Success;

    public Guid? ReversalReferenceId { get; set; }

    public BilDepositPaymentOrder DepositOrder { get; set; } = null!;
    public BilDepositMovement DepositMovement { get; set; } = null!;
}

public static class DepositOrderPaymentStatuses
{
    public const string Success = "SUCCESS";
    public const string Reversed = "REVERSED";
}
