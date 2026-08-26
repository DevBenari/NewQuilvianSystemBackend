using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

public sealed class CreateWriteOffRequest
{
    public Guid InvoiceId { get; set; }
    [Range(typeof(decimal), "0.01", "9999999999999999.99")]
    public decimal Amount { get; set; }
    public Guid ExpectedInvoiceRowVersion { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
}

public sealed class WriteOffApprovalRequest
{
    public Guid ExpectedRowVersion { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
}

public sealed class WriteOffResponse
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public bool IsFullSettlement { get; set; }
    public decimal OutstandingBefore { get; set; }
    public decimal OutstandingAfter { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid RequestedBy { get; set; }
    public Guid? ApprovedBy { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public Guid RowVersion { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
    public bool IsReplay { get; set; }
}
