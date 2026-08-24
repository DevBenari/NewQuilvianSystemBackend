namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

public sealed class ArHandoffResponse
{
    public Guid Id { get; set; }
    public string DebtorType { get; set; } = string.Empty;
    public Guid? DebtorReferenceId { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class ApHandoffResponse
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public decimal Amount { get; set; }
    public string ReadinessStatus { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ReadyAt { get; set; }
}

public sealed class HandoffAdjustmentResponse
{
    public Guid Id { get; set; }
    public string Direction { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class HandoffStatusResponse
{
    public Guid InvoiceId { get; set; }
    public Guid FinalizationRecordId { get; set; }
    public IReadOnlyList<ArHandoffResponse> ArHandoffs { get; set; } = [];
    public IReadOnlyList<ApHandoffResponse> ApHandoffs { get; set; } = [];
    public IReadOnlyList<HandoffAdjustmentResponse> Adjustments { get; set; } = [];
}
