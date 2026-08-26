using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

public sealed class FinalizeInvoiceRequest
{
    public Guid ExpectedRowVersion { get; set; }
    [MaxLength(30)] public string? DepartureReason { get; set; }
    [MaxLength(200)] public string? DebtorIdentity { get; set; }
    [MaxLength(100)] public string? DebtorRelationship { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
}

public sealed class FinalizationPreviewResponse
{
    public Guid InvoiceId { get; set; }
    public bool AllOrdersComplete { get; set; }
    public bool CalculationCurrent { get; set; }
    public decimal Outstanding { get; set; }
    public bool IsReadyForNormalFinalization { get; set; }
    public IReadOnlyList<string> BlockingReasons { get; set; } = [];
    public int CalculationVersion { get; set; }
    public Guid InvoiceRowVersion { get; set; }
}

public sealed class FinalizationResponse
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public int CalculationVersion { get; set; }
    public decimal OutstandingAtFinalization { get; set; }
    public bool IsDepartureException { get; set; }
    public string? DepartureReason { get; set; }
    public string InvoiceStatus { get; set; } = string.Empty;
    public DateTimeOffset FinalizedAt { get; set; }
    public Guid InvoiceRowVersion { get; set; }
    public Guid CorrelationId { get; set; }
    public bool IsReplay { get; set; }
}
