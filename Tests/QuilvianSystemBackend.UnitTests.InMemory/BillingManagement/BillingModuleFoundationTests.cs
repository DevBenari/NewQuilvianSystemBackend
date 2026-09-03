using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Tests.BillingManagement;

public sealed class BillingModuleFoundationTests
{
    [Fact]
    public void AddBillingManagement_RegistersBillingModuleServiceAsScoped()
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => IsolatedBillingDbContextFactory.Create());
        services.AddBillingManagement();

        using var provider = services.BuildServiceProvider(validateScopes: true);
        using var firstScope = provider.CreateScope();
        using var secondScope = provider.CreateScope();

        var first = firstScope.ServiceProvider.GetRequiredService<BillingModuleService>();
        var firstAgain = firstScope.ServiceProvider.GetRequiredService<BillingModuleService>();
        var second = secondScope.ServiceProvider.GetRequiredService<BillingModuleService>();

        Assert.Same(first, firstAgain);
        Assert.NotSame(first, second);
    }

    [Fact]
    public void TransactionControllers_DoNotReceiveApplicationDbContextDirectly()
    {
        const string transactionControllerNamespace =
            "QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Controllers";

        var violations = typeof(BillingModuleService).Assembly
            .GetTypes()
            .Where(type =>
                type.Namespace?.StartsWith(transactionControllerNamespace, StringComparison.Ordinal) == true &&
                typeof(ControllerBase).IsAssignableFrom(type))
            .SelectMany(type => type.GetConstructors())
            .Where(constructor => constructor
                .GetParameters()
                .Any(parameter => parameter.ParameterType == typeof(ApplicationDbContext)))
            .Select(constructor => constructor.DeclaringType?.FullName)
            .Where(name => name is not null)
            .ToArray();

        Assert.Empty(violations);
    }
}
