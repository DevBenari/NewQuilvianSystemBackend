using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

[Table("BilWriteOffCase", Schema = "public")]
public sealed class BilWriteOffCase : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public bool IsFullSettlement { get; set; }
    [Required, MaxLength(30)] public string Status { get; set; } = BillingWriteOffCaseStatuses.Submitted;
    public Guid RequestedBy { get; set; }
    public Guid? ApprovedBy { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    public Guid IdempotencyKey { get; set; }
    [Required, MaxLength(64)] public string PayloadHash { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public BilInvoice Invoice { get; set; } = null!;
}

public static class BillingWriteOffCaseStatuses
{
    public const string Submitted = "SUBMITTED";
    public const string Posted = "POSTED";
    public const string Rejected = "REJECTED";
}
