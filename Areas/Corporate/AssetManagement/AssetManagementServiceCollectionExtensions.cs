using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Seeders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Services;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Services;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement;

public static class AssetManagementServiceCollectionExtensions
{
    public static IServiceCollection AddAssetManagement(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<AssetAttachmentOptions>(configuration.GetSection(AssetAttachmentOptions.SectionName));
        services.Configure<AssetEntityOptions>(configuration.GetSection(AssetEntityOptions.SectionName));

        services.AddScoped<AssetActorResolver>();
        services.AddScoped<AssetHistoryService>();
        services.AddScoped<AssetProcessLockService>();
        services.AddScoped<AssetAttachmentService>();
        services.AddScoped<AssetNumberingService>();

        services.AddScoped<AssetEntityResolver>();
        services.AddScoped<AssetMasterDataService>();
        services.AddHostedService<AssetMasterDataSeeder>();

        services.Configure<AssetAccountingDispatchOptions>(configuration.GetSection(AssetAccountingDispatchOptions.SectionName));
        services.AddScoped<AssetValuationParameterQueryService>();
        services.AddScoped<AssetValuationParameterService>();
        services.AddScoped<AssetAccountingEventService>();
        services.AddScoped<AssetBookService>();

        services.AddScoped<AssetRecordService>();
        services.AddScoped<AssetLabelDocumentService>();
        services.AddScoped<AssetStockTakeGate>();
        services.AddScoped<AssetPlacementService>();
        services.AddScoped<AssetConditionService>();

        return services;
    }
}
