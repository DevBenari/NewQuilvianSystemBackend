using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

[Table("BilRefundLine", Schema = "public")]
public sealed class BilRefundLine : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RefundCaseId { get; set; }
    public Guid OriginalTenderId { get; set; }
    public Guid PaymentMethodId { get; set; }
    public decimal Amount { get; set; }
    [Required, MaxLength(30)] public string Status { get; set; } = BillingRefundLineStatuses.Pending;
    [MaxLength(150)] public string? ProviderReference { get; set; }
    [MaxLength(50)] public string? ProviderStatusCode { get; set; }
    public Guid IdempotencyKey { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
    public DateTimeOffset? AttemptedAt { get; set; }
    public DateTimeOffset? SettledAt { get; set; }

    public BilRefundCase RefundCase { get; set; } = null!;
    public BilTender OriginalTender { get; set; } = null!;
}

public static class BillingRefundLineStatuses
{
    public const string Pending = "PENDING";
    public const string Succeeded = "SUCCEEDED";
    public const string Failed = "FAILED";
}
