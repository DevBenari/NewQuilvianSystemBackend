namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

public interface IBillingPaymentProviderAdapter
{
    Task<BillingPaymentProviderResult> SubmitAsync(
        BillingPaymentProviderRequest request,
        CancellationToken cancellationToken);
}

public sealed record BillingPaymentProviderRequest(
    Guid TenderId,
    string PaymentMethodCode,
    string? IntegrationCode,
    decimal Amount,
    Guid IdempotencyKey,
    Guid CorrelationId,
    Guid CausationId,
    string ContractVersion = BillingPaymentProviderContracts.CurrentVersion);

public sealed record BillingPaymentProviderResult(
    string EventId,
    BillingPaymentProviderOutcome Outcome,
    string? ProviderReference,
    string? ProviderStatusCode,
    DateTimeOffset OccurredAt,
    Guid CorrelationId,
    Guid CausationId,
    string ContractVersion = BillingPaymentProviderContracts.CurrentVersion);

public enum BillingPaymentProviderOutcome
{
    Pending,
    Succeeded,
    Failed,
    Expired,
    Reversed
}

public static class BillingPaymentProviderContracts
{
    public const string CurrentVersion = "0.4";
}

/// <summary>
/// Safe default until a provider-specific integration is registered by deployment.
/// It never fabricates a successful payment; the service persists the tender as pending.
/// </summary>
public sealed class DeferredBillingPaymentProviderAdapter : IBillingPaymentProviderAdapter
{
    public Task<BillingPaymentProviderResult> SubmitAsync(
        BillingPaymentProviderRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        throw new BillingPaymentProviderTimeoutException(
            "Status pembayaran belum dapat dipastikan dari provider.");
    }
}

public sealed class BillingPaymentProviderTimeoutException(string message) : Exception(message);
