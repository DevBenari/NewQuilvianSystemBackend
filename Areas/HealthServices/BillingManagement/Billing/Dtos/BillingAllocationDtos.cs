using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

public sealed class DepositAllocationRequest
{
    public Guid InvoiceId { get; set; }
    [Range(
        typeof(decimal),
        "0.01",
        "9999999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)]
    public decimal Amount { get; set; }
    public Guid ExpectedDepositRowVersion { get; set; }
    public Guid ExpectedInvoiceRowVersion { get; set; }
    [Range(1, int.MaxValue)] public int ExpectedCalculationVersion { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
}

public sealed class AllocationResponse
{
    public Guid Id { get; set; }
    public Guid SettlementId { get; set; }
    public Guid InvoiceId { get; set; }
    public string TargetType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int CalculationVersion { get; set; }
    public DateTimeOffset AllocatedAt { get; set; }
    public decimal DepositBalance { get; set; }
    public decimal InvoiceOutstanding { get; set; }
    public decimal RefundableCredit { get; set; }
    public Guid DepositRowVersion { get; set; }
    public Guid InvoiceRowVersion { get; set; }
    public Guid CorrelationId { get; set; }
    public bool IsReplay { get; set; }
}
