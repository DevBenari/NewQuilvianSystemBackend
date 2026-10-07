namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
public class CliSurgicalSiteSurveillanceWorker(IServiceScopeFactory scopes, IConfiguration config, ILogger<CliSurgicalSiteSurveillanceWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromMinutes(Math.Clamp(config.GetValue("Clinical:SurgicalSiteSurveillance:WorkerIntervalMinutes", 5), 1, 60));
        using var timer = new PeriodicTimer(interval);
        while (!stoppingToken.IsCancellationRequested)
        {
            try { using var scope = scopes.CreateScope(); await scope.ServiceProvider.GetRequiredService<CliSurgicalSiteSurveillanceService>().ProcessAsync(stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { logger.LogError(ex, "Pemrosesan surveilans operasi gagal; akan dicoba pada putaran berikutnya."); }
            try { if (!await timer.WaitForNextTickAsync(stoppingToken)) break; } catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
        }
    }
}
