using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

[Table("BilRefundCase", Schema = "public")]
public sealed class BilRefundCase : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid InvoiceId { get; set; }
    public Guid RefundableCreditId { get; set; }
    public decimal RequestedAmount { get; set; }
    [Required, MaxLength(30)] public string Status { get; set; } = BillingRefundCaseStatuses.Submitted;
    public Guid RequestedBy { get; set; }
    public Guid? ApprovedBy { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    public Guid? AdjustmentId { get; set; }
    public Guid IdempotencyKey { get; set; }
    [Required, MaxLength(64)] public string PayloadHash { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public BilInvoice Invoice { get; set; } = null!;
    public BilRefundableCredit RefundableCredit { get; set; } = null!;
    public ICollection<BilRefundLine> Lines { get; set; } = new List<BilRefundLine>();
}

public static class BillingRefundCaseStatuses
{
    public const string Submitted = "SUBMITTED";
    public const string Approved = "APPROVED";
    public const string Rejected = "REJECTED";
    public const string PartiallyExecuted = "PARTIALLY_EXECUTED";
    public const string Executed = "EXECUTED";
}
