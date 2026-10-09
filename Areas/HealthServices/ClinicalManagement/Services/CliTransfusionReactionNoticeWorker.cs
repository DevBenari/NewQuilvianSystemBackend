using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Repositories;
namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
public class CliTransfusionReactionNoticeWorker(IServiceScopeFactory scopes, ILogger<CliTransfusionReactionNoticeWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                List<Guid> ids;
                using (var scope = scopes.CreateScope()) ids = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Set<CliTransfusionReaction>().AsNoTracking().Where(x => !x.IsDelete && x.NoticeDelivery != CliReactionNoticeDelivery.Delivered).OrderBy(x => x.UpdateDateTime ?? x.CreateDateTime).Take(100).Select(x => x.Id).ToListAsync(stoppingToken);
                foreach (var id in ids) { using var scope = scopes.CreateScope(); await scope.ServiceProvider.GetRequiredService<CliTransfusionReactionDeliveryService>().DeliverAsync(id, stoppingToken); }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { logger.LogError(ex, "Putaran pengiriman ulang reaksi transfusi gagal."); }
            try { if (!await timer.WaitForNextTickAsync(stoppingToken)) break; } catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
        }
    }
}
