using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

[Table("BilAdjustment", Schema = "public")]
public sealed class BilAdjustment : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid InvoiceId { get; set; }
    [Required, MaxLength(10)] public string Direction { get; set; } = BillingAdjustmentDirections.Credit;
    public decimal Amount { get; set; }
    [Required, MaxLength(30)] public string Status { get; set; } = BillingAdjustmentStatuses.Submitted;
    public Guid RequestedBy { get; set; }
    public Guid? ApprovedBy { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    public Guid? ReversesAdjustmentId { get; set; }
    public Guid? ReversesWriteOffCaseId { get; set; }
    public Guid IdempotencyKey { get; set; }
    [Required, MaxLength(64)] public string PayloadHash { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public BilInvoice Invoice { get; set; } = null!;
}

public static class BillingAdjustmentDirections
{
    public const string Debit = "DEBIT";
    public const string Credit = "CREDIT";
}

public static class BillingAdjustmentStatuses
{
    public const string Submitted = "SUBMITTED";
    public const string Posted = "POSTED";
    public const string Rejected = "REJECTED";
}
