using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Services;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing;

public static class BillingManagementServiceCollectionExtensions
{
    public static IServiceCollection AddBillingManagement(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<BillingModuleService>();
        services.AddScoped<BillingInvoiceService>();
        services.AddScoped<BillingCalculationService>();
        services.AddScoped<BillingDiscountService>();
        services.AddScoped<BillingDepositService>();
        services.AddScoped<BillingAllocationService>();
        services.AddScoped<BillingSettlementService>();
        services.AddScoped<BillingRefundService>();
        services.AddScoped<BillingFinancialExceptionService>();
        services.AddScoped<BillingArApHandoffService>();
        services.AddScoped<BillingFinalizationService>();
        services.AddScoped<BillingNumberSeriesService>();
        services.AddScoped<CashierShiftService>();
        services.AddScoped<IBillingChargeSourceAdapter, ContractBillingChargeSourceAdapter>();
        services.AddScoped<IBillingCoverageAdapter, RegistrationBillingCoverageAdapter>();
        services.AddOptions<BillingPaymentProviderOptions>()
            .BindConfiguration(BillingPaymentProviderOptions.SectionName);

        // Selama belum ada integrasi mesin pembayaran/tender bank, tender non-tunai diterima
        // langsung supaya kasir tidak menunggu konfirmasi yang tidak akan datang. Setel
        // Billing:PaymentProvider:AutoAcceptWithoutProvider = false untuk kembali ke perilaku
        // aman (tender non-tunai berstatus Pending) begitu provider tersambung.
        services.AddScoped<IBillingPaymentProviderAdapter>(provider =>
            provider.GetRequiredService<IOptions<BillingPaymentProviderOptions>>()
                .Value.AutoAcceptWithoutProvider
                ? new AutoAcceptBillingPaymentProviderAdapter()
                : new DeferredBillingPaymentProviderAdapter());
        services.AddOptions<BillingInvoiceNumberOptions>()
            .BindConfiguration(BillingInvoiceNumberOptions.SectionName)
            .ValidateOnStart();
        services.AddOptions<BillingDepositAccountNumberOptions>()
            .BindConfiguration(BillingDepositAccountNumberOptions.SectionName)
            .ValidateOnStart();
        services.AddOptions<BillingCashierShiftNumberOptions>()
            .BindConfiguration(BillingCashierShiftNumberOptions.SectionName)
            .ValidateOnStart();
        services.AddScoped<AdministrationFeePolicyService>();
        services.AddScoped<DiscountPolicyService>();
        services.AddScoped<TaxRuleService>();
        services.AddScoped<RoomChargePolicyService>();
        services.AddScoped<RegisterService>();

        return services;
    }
}
