using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

[Table("BilSettlement", Schema = "public")]
public sealed class BilSettlement : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? InvoiceId { get; set; }
    public Guid? DepositAccountId { get; set; }
    [Required, MaxLength(30)] public string Purpose { get; set; } = string.Empty;
    public decimal RequestedAmount { get; set; }
    public decimal SuccessfulAmount { get; set; }
    public decimal AllocatedAmount { get; set; }
    [Required, MaxLength(30)] public string Status { get; set; } = BillingSettlementStatuses.Draft;
    public Guid IdempotencyKey { get; set; }
    [Required, MaxLength(64)] public string PayloadHash { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public Guid RowVersion { get; set; } = Guid.NewGuid();
    public ICollection<BilTender> Tenders { get; set; } = new List<BilTender>();
    public ICollection<BilPaymentAllocation> Allocations { get; set; } = new List<BilPaymentAllocation>();
}

public static class BillingSettlementStatuses
{
    public const string Draft = "DRAFT";
    public const string InProgress = "IN_PROGRESS";
    public const string PartiallySettled = "PARTIALLY_SETTLED";
    public const string Settled = "SETTLED";
    public const string Failed = "FAILED";
}

public static class BillingSettlementPurposes
{
    public const string DepositTopUp = "DEPOSIT_TOP_UP";
    public const string InvoicePayment = "INVOICE_PAYMENT";
}
