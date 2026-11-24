using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Seeders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Services;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Services;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Services;
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
        services.AddScoped<AssetSubledgerSetupService>();
        services.AddScoped<AssetAccountingEventService>();
        services.AddScoped<AssetBookService>();

        services.AddScoped<AssetRecordService>();
        services.AddScoped<AssetLabelDocumentService>();
        services.AddScoped<AssetStockTakeGate>();
        services.AddScoped<AssetPlacementService>();
        services.AddScoped<AssetConditionService>();
        services.AddScoped<AssetDocumentService>();
        services.AddScoped<AssetUtilizationService>();
        services.AddScoped<AssetAttachmentAccessService>();
        services.Configure<AssetImportOptions>(configuration.GetSection(AssetImportOptions.SectionName));
        services.AddScoped<AssetImportService>();
        services.AddScoped<AssetDashboardQueryService>();
        services.AddScoped<AssetReportQueryService>();
        services.AddScoped<AssetMovementDocumentService>();
        services.AddScoped<AssetTransferService>();
        services.AddScoped<AssetLoanService>();
        services.AddScoped<MovementQueryService>();
        services.AddScoped<StockTakeMatchingService>();
        services.AddScoped<StockTakeFollowUpService>();
        services.AddScoped<StockTakeDocumentService>();
        services.AddScoped<StockTakeSessionService>();
        services.AddScoped<StockTakeScanService>();
        services.AddScoped<StockTakeQueryService>();
        services.Configure<AssetDailyStatusWorkerOptions>(configuration.GetSection(AssetDailyStatusWorkerOptions.SectionName));
        services.AddHostedService<AssetDailyStatusWorker>();

        return services;
    }
}
