using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    /// <summary>
    /// Background service yang menjalankan siklus scheduler perencanaan tenaga kerja secara berkala (BE-HRD-036).
    /// </summary>
    public sealed class HrdWorkforcePlanningSchedulerHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly HrdWorkforcePlanningSchedulerOptions _options;
        private readonly ILogger<HrdWorkforcePlanningSchedulerHostedService> _logger;

        public HrdWorkforcePlanningSchedulerHostedService(
            IServiceScopeFactory scopeFactory,
            IOptions<HrdWorkforcePlanningSchedulerOptions> options,
            ILogger<HrdWorkforcePlanningSchedulerHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.Enabled)
            {
                _logger.LogInformation("Workforce Planning scheduler dinonaktifkan oleh konfigurasi.");
                return;
            }

            var interval = TimeSpan.FromSeconds(Math.Max(10, _options.PollIntervalSeconds));
            _logger.LogInformation(
                "Workforce Planning scheduler hosted service dimulai. Interval={IntervalSeconds}s, WorkerInstance={WorkerInstance}",
                interval.TotalSeconds,
                _options.WorkerInstanceName);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var schedulerService = scope.ServiceProvider.GetRequiredService<IHrdWorkforcePlanningSchedulerService>();

                    await schedulerService.ExecuteDailyCycleAsync(null, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Siklus background worker Workforce Planning scheduler mengalami error.");
                }

                try
                {
                    await Task.Delay(interval, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }

            _logger.LogInformation("Workforce Planning scheduler hosted service berhenti.");
        }
    }
}
