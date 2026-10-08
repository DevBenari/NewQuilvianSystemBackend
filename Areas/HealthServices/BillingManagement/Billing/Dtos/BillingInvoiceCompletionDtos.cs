using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

public sealed class CompleteInvoiceRequest
{
    public Guid ExpectedRowVersion { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
}

public sealed class CompleteInvoiceResponse
{
    public Guid InvoiceId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal PatientAmount { get; set; }
    public decimal PatientOutstanding { get; set; }
    public bool PaymentRequired { get; set; }
    public bool PaymentProcessed { get; set; }
    public bool AutoCompleted { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public Guid? FinalizationRecordId { get; set; }
    public List<BilArHandoffSummaryDto> PayerHandoffs { get; set; } = [];
    public bool IsReplay { get; set; }
}

public sealed class BilArHandoffSummaryDto
{
    public Guid Id { get; set; }
    public string DebtorType { get; set; } = string.Empty;
    public Guid? DebtorReferenceId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
}
