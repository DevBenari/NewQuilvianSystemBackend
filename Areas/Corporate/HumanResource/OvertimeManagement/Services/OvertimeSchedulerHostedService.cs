using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public class OvertimeSchedulerHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly OvertimeSchedulerOptions _options;
        private readonly ILogger<OvertimeSchedulerHostedService> _logger;
        private readonly string _workerInstanceId;

        public OvertimeSchedulerHostedService(
            IServiceScopeFactory scopeFactory,
            IOptions<OvertimeSchedulerOptions> options,
            ILogger<OvertimeSchedulerHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options.Value;
            _logger = logger;
            _workerInstanceId = BuildWorkerInstanceId(_options.WorkerInstanceName);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.Enabled)
            {
                _logger.LogInformation("Overtime scheduler disabled by configuration.");
                return;
            }

            var interval = TimeSpan.FromSeconds(Math.Max(5, _options.PollIntervalSeconds));
            _logger.LogInformation(
                "Overtime scheduler started. Worker={WorkerInstanceId}, PollInterval={PollIntervalSeconds}s.",
                _workerInstanceId,
                interval.TotalSeconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var scheduler = scope.ServiceProvider.GetRequiredService<OvertimeSchedulerService>();
                    var now = DateTime.UtcNow;
                    await scheduler.RecoverStaleRunningJobsAsync(now, stoppingToken);
                    await scheduler.EnsureAutomaticDailyJobsAsync(now, stoppingToken);
                    await scheduler.ProcessScheduledPeriodClosuresAsync(now, stoppingToken);
                    var job = await scheduler.ClaimNextJobAsync(_workerInstanceId, stoppingToken);
                    if (job != null)
                    {
                        await scheduler.ExecuteClaimedJobAsync(job.Id, stoppingToken);
                        continue;
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Overtime scheduler worker cycle failed.");
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

            _logger.LogInformation("Overtime scheduler stopped. Worker={WorkerInstanceId}.", _workerInstanceId);
        }

        private static string BuildWorkerInstanceId(string? configuredName)
        {
            var prefix = string.IsNullOrWhiteSpace(configuredName)
                ? "quilvian-overtime-scheduler"
                : configuredName.Trim();
            return $"{prefix}:{Environment.MachineName}:{Environment.ProcessId}:{Guid.NewGuid():N}";
        }
    }
}
