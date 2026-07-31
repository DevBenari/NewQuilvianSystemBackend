using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services
{
    public class AttendanceSchedulerHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly AttendanceSchedulerOptions _options;
        private readonly ILogger<AttendanceSchedulerHostedService> _logger;
        private readonly string _workerInstanceId;

        public AttendanceSchedulerHostedService(
            IServiceScopeFactory scopeFactory,
            IOptions<AttendanceSchedulerOptions> options,
            ILogger<AttendanceSchedulerHostedService> logger)
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
                _logger.LogInformation("Attendance scheduler disabled by configuration.");
                return;
            }

            var interval = TimeSpan.FromSeconds(Math.Max(5, _options.PollIntervalSeconds));
            _logger.LogInformation(
                "Attendance scheduler started. Worker={WorkerInstanceId}, PollInterval={PollIntervalSeconds}s.",
                _workerInstanceId,
                interval.TotalSeconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var scheduler = scope.ServiceProvider.GetRequiredService<AttendanceSchedulerService>();

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
                    _logger.LogError(ex, "Attendance scheduler worker cycle failed.");
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

            _logger.LogInformation("Attendance scheduler stopped. Worker={WorkerInstanceId}.", _workerInstanceId);
        }

        private static string BuildWorkerInstanceId(string? configuredName)
        {
            var prefix = string.IsNullOrWhiteSpace(configuredName)
                ? "quilvian-attendance-scheduler"
                : configuredName.Trim();
            return $"{prefix}:{Environment.MachineName}:{Environment.ProcessId}:{Guid.NewGuid():N}";
        }
    }
}
