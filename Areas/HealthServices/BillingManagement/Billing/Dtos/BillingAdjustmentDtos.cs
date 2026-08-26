using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

public sealed class CreateAdjustmentRequest
{
    public Guid InvoiceId { get; set; }
    [Required, MaxLength(10)] public string Direction { get; set; } = string.Empty;
    [Range(typeof(decimal), "0.01", "9999999999999999.99")]
    public decimal Amount { get; set; }
    public Guid ExpectedInvoiceRowVersion { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
}

public sealed class AdjustmentApprovalRequest
{
    public Guid ExpectedRowVersion { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
}

public sealed class ReverseExceptionRequest
{
    public Guid ExpectedRowVersion { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
}

public sealed class AdjustmentResponse
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public string Direction { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid RequestedBy { get; set; }
    public Guid? ApprovedBy { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid? ReversesAdjustmentId { get; set; }
    public Guid? ReversesWriteOffCaseId { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid RowVersion { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
    public bool IsReplay { get; set; }
}

public sealed class InvoiceFinancialExceptionsResponse
{
    public IReadOnlyList<AdjustmentResponse> Adjustments { get; set; } = [];
    public IReadOnlyList<WriteOffResponse> WriteOffs { get; set; } = [];
    public IReadOnlyList<RefundResponse> Refunds { get; set; } = [];
}
