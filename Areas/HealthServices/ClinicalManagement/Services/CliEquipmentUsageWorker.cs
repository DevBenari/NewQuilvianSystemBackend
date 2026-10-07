namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
public sealed class CliEquipmentUsageWorker(IServiceScopeFactory scopes, ILogger<CliEquipmentUsageWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        do
        {
            try { using var scope = scopes.CreateScope(); await scope.ServiceProvider.GetRequiredService<CliEquipmentUsageService>().RepairDepartureAndDeliveryAsync(stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { logger.LogError(ex, "Pemulihan penutupan dan penyerahan pemakaian alat gagal."); }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
