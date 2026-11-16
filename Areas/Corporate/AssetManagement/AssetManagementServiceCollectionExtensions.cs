using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement;

public static class AssetManagementServiceCollectionExtensions
{
    public static IServiceCollection AddAssetManagement(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<AssetAttachmentOptions>(configuration.GetSection(AssetAttachmentOptions.SectionName));

        services.AddScoped<AssetActorResolver>();
        services.AddScoped<AssetHistoryService>();
        services.AddScoped<AssetProcessLockService>();
        services.AddScoped<AssetAttachmentService>();
        services.AddScoped<AssetNumberingService>();

        return services;
    }
}
