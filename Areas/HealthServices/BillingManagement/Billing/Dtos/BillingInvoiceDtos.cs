using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

public sealed class BillingInvoiceQuery
{
    public Guid? EncounterId { get; set; }
    public string? Status { get; set; }
    public string? ServiceType { get; set; }
    public string? Search { get; set; }
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed class UpsertChargeRequest
{
    public Guid EncounterId { get; set; }
    [Required, MaxLength(50)] public string SourceDomain { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string SourceDetailId { get; set; } = string.Empty;
    [Range(1, long.MaxValue)] public long SourceVersion { get; set; }
    [Required, MaxLength(30)] public string SourceStatus { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; }
    public Guid CategoryId { get; set; }
    [Required, MaxLength(250)] public string DescriptionSnapshot { get; set; } = string.Empty;
    [Range(typeof(decimal), "0.0001", "99999999999999.9999")] public decimal Quantity { get; set; }
    [Range(typeof(decimal), "0", "9999999999999999.99")] public decimal UnitPrice { get; set; }
    [Range(typeof(decimal), "0", "9999999999999999.99")] public decimal DoctorShare { get; set; }
    [Required, MaxLength(30)] public string ContractVersion { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
}

public class InvoiceSummaryResponse
{
    public Guid Id { get; set; }
    public Guid EncounterId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int CurrentCalculationVersion { get; set; }
    public decimal RunningGrossAmount { get; set; }
    public int ActiveItemCount { get; set; }
    public DateTime CreateDateTime { get; set; }
    public Guid RowVersion { get; set; }
}

public sealed class InvoiceDetailResponse : InvoiceSummaryResponse
{
    public DateTimeOffset? InvoiceDate { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public bool IsReplay { get; set; }
    public IReadOnlyList<InvoiceItemResponse> Items { get; set; } = [];
}

public sealed class InvoiceItemResponse
{
    public Guid Id { get; set; }
    public string SourceDomain { get; set; } = string.Empty;
    public string SourceDetailId { get; set; } = string.Empty;
    public long SourceVersion { get; set; }
    public string SourceContractVersion { get; set; } = string.Empty;
    public string SourceStatus { get; set; } = string.Empty;
    public DateTimeOffset SourceOccurredAt { get; set; }
    public Guid CategoryId { get; set; }
    public string DescriptionSnapshot { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DoctorShare { get; set; }
    public decimal GrossAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}
