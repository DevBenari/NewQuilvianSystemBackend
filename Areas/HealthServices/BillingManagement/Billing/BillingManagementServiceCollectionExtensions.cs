using Microsoft.Extensions.DependencyInjection;
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
        services.AddScoped<IBillingPaymentProviderAdapter, DeferredBillingPaymentProviderAdapter>();
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

        return services;
    }
}
