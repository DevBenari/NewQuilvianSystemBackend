using Microsoft.Extensions.DependencyInjection;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing;

public static class BillingManagementServiceCollectionExtensions
{
    public static IServiceCollection AddBillingManagement(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<BillingModuleService>();

        return services;
    }
}
