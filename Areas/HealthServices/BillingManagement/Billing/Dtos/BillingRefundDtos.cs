using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

public sealed class CreateRefundRequest
{
    public Guid InvoiceId { get; set; }
    public Guid RefundableCreditId { get; set; }
    [Range(typeof(decimal), "0.01", "9999999999999999.99")]
    public decimal RequestedAmount { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
}

public sealed class RefundApprovalRequest
{
    public Guid ExpectedRowVersion { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
}

public sealed class RefundResponse
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid RefundableCreditId { get; set; }
    public decimal RequestedAmount { get; set; }
    public decimal ExecutedAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid RequestedBy { get; set; }
    public Guid? ApprovedBy { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid? AdjustmentId { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid RowVersion { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public bool IsReplay { get; set; }
    public IReadOnlyList<RefundLineResponse> Lines { get; set; } = [];
}

public sealed class RefundLineResponse
{
    public Guid Id { get; set; }
    public Guid OriginalTenderId { get; set; }
    public Guid PaymentMethodId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ProviderReferenceMasked { get; set; }
    public string? ProviderStatusCode { get; set; }
    public DateTimeOffset? AttemptedAt { get; set; }
    public DateTimeOffset? SettledAt { get; set; }
}

public sealed class RefundableCreditResponse
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public Guid SourceId { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal AvailableAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset RecognizedAt { get; set; }
}
