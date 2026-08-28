using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

public sealed class CreateSettlementRequest
{
    public Guid? InvoiceId { get; set; }
    public Guid? DepositAccountId { get; set; }
    [Required, MaxLength(30)] public string Purpose { get; set; } = string.Empty;
    [Range(
        typeof(decimal),
        "0.01",
        "9999999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)]
    public decimal RequestedAmount { get; set; }
    [MaxLength(500)] public string? Note { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
}

public sealed class CreateTenderRequest
{
    public Guid PaymentMethodId { get; set; }
    [Range(
        typeof(decimal),
        "0.01",
        "9999999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)]
    public decimal Amount { get; set; }
    [MaxLength(150)] public string? CashierReferenceNote { get; set; }
    public Guid ExpectedRowVersion { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
}

public sealed class SettlementResponse
{
    public Guid? Id { get; set; }
    public Guid? InvoiceId { get; set; }
    public Guid? DepositAccountId { get; set; }
    public string Purpose { get; set; } = BillingSettlementPurposes.DepositTopUp;
    public string? Note { get; set; }
    public string Status { get; set; } = BillingSettlementStatuses.Draft;
    public decimal RequestedAmount { get; set; }
    public decimal SuccessfulAmount { get; set; }
    public decimal AllocatedAmount { get; set; }
    public decimal PendingAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public decimal CollectibleAmount { get; set; }
    public bool IsReplay { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid? RowVersion { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public Guid? DepositMovementId { get; set; }
    public DepositResponse? Deposit { get; set; }
    public IReadOnlyList<TenderResponse> Tenders { get; set; } = [];
}

public sealed class TenderResponse
{
    public Guid Id { get; set; }
    public Guid SettlementId { get; set; }
    public Guid PaymentMethodId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = BillingTenderStatuses.Created;
    public string? CashierReferenceNote { get; set; }
    public string? KwitansiNumber { get; set; }
    public string? ProviderReferenceMasked { get; set; }
    public string? ProviderStatusCode { get; set; }
    public DateTimeOffset AttemptedAt { get; set; }
    public DateTimeOffset? SettledAt { get; set; }
    public Guid? CashierShiftId { get; set; }
    public Guid RowVersion { get; set; }
    public bool IsReplay { get; set; }
}
