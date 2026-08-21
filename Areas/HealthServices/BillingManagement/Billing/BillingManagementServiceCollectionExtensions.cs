using Microsoft.Extensions.DependencyInjection;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Services;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing;

public static class BillingManagementServiceCollectionExtensions
{
    public static IServiceCollection AddBillingManagement(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<BillingModuleService>();
        services.AddScoped<BillingInvoiceService>();
        services.AddScoped<BillingNumberSeriesService>();
        services.AddScoped<IBillingChargeSourceAdapter, ContractBillingChargeSourceAdapter>();
        services.AddOptions<BillingInvoiceNumberOptions>()
            .BindConfiguration(BillingInvoiceNumberOptions.SectionName)
            .ValidateOnStart();
        services.AddScoped<AdministrationFeePolicyService>();
        services.AddScoped<DiscountPolicyService>();
        services.AddScoped<TaxRuleService>();
        services.AddScoped<RoomChargePolicyService>();

        return services;
    }
}
