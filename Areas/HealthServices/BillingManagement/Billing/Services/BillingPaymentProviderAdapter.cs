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

/// <summary>
/// Stand-in sementara selama belum ada integrasi mesin pembayaran / gateway perbankan otomatis.
///
/// Sesuai audit keselamatan transaksi finansial, adapter provider tidak boleh memalsukan status
/// Succeeded tanpa ProviderReference sah dari bank/gateway (yang akan memicu error 422 pada
/// ReconcileTenderAsync atau menghasilkan pengakuan sukses tanpa dasar).
///
/// Setiap tender yang lewat jalur ini diberi status Pending dengan ProviderStatusCode
/// "BYPASS_NO_PROVIDER" agar kasir dapat melakukan verifikasi bukti transaksi secara eksplisit
/// melalui jalur konfirmasi manual (MANUAL_CASHIER) atau menunggu webhook callback provider asli.
/// </summary>
public sealed class AutoAcceptBillingPaymentProviderAdapter : IBillingPaymentProviderAdapter
{
    public const string BypassStatusCode = "BYPASS_NO_PROVIDER";

    public Task<BillingPaymentProviderResult> SubmitAsync(
        BillingPaymentProviderRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(new BillingPaymentProviderResult(
            $"bypass:{request.TenderId:N}",
            BillingPaymentProviderOutcome.Pending,
            null,
            BypassStatusCode,
            DateTimeOffset.UtcNow,
            request.CorrelationId,
            request.CausationId));
    }
}

public sealed class BillingPaymentProviderOptions
{
    public const string SectionName = "Billing:PaymentProvider";

    /// <summary>
    /// true  = tender non-tunai langsung diproses adapter bypass (menghasilkan status Pending tanpa konfirmasi palsu).
    /// false = perilaku aman: tender non-tunai berstatus Pending via Deferred adapter sampai provider mengonfirmasi.
    /// </summary>
    public bool AutoAcceptWithoutProvider { get; set; } = false;
}

public sealed class BillingPaymentProviderTimeoutException(string message) : Exception(message);
