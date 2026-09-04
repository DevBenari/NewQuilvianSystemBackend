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
/// Stand-in sementara selama belum ada integrasi mesin pembayaran / tender bank.
///
/// Tender non-tunai langsung dianggap berhasil supaya kasir tidak menunggu status yang tidak
/// akan pernah datang. Ini MEMALSUKAN konfirmasi provider: yang tercatat hanyalah bahwa petugas
/// menyatakan uangnya diterima, bukan konfirmasi bank/QRIS. Karena itu setiap tender yang lewat
/// jalur ini diberi ProviderStatusCode "BYPASS_NO_PROVIDER" supaya bisa dipisahkan saat
/// rekonsiliasi, dan jalurnya dapat dimatikan lewat konfigurasi
/// Billing:PaymentProvider:AutoAcceptWithoutProvider begitu integrasi asli tersedia.
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
            BillingPaymentProviderOutcome.Succeeded,
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
    /// true  = tender non-tunai langsung diterima tanpa provider (default saat ini, karena belum
    ///         ada integrasi mesin pembayaran).
    /// false = perilaku aman: tender non-tunai berstatus Pending sampai provider mengonfirmasi.
    /// </summary>
    public bool AutoAcceptWithoutProvider { get; set; } = true;
}

public sealed class BillingPaymentProviderTimeoutException(string message) : Exception(message);
