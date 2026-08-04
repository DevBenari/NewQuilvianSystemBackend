using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    public class LeaveCarryForwardSchedulerHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly LeaveCarryForwardSchedulerOptions _options;
        private readonly ILogger<LeaveCarryForwardSchedulerHostedService> _logger;
        private DateOnly? _lastDailyExecutionDate;

        public LeaveCarryForwardSchedulerHostedService(
            IServiceScopeFactory scopeFactory,
            IOptions<LeaveCarryForwardSchedulerOptions> options,
            ILogger<LeaveCarryForwardSchedulerHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.Enabled)
            {
                _logger.LogInformation("Leave Carry Forward Scheduler dinonaktifkan melalui konfigurasi.");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var scheduler = scope.ServiceProvider.GetRequiredService<LeaveCarryForwardSchedulerService>();
                    await scheduler.RecoverStaleRunsAsync(stoppingToken);

                    var localNow = GetLocalNow();
                    var localDate = DateOnly.FromDateTime(localNow.DateTime);
                    var scheduledAt = new TimeSpan(_options.DailyEnqueueHour, _options.DailyEnqueueMinute, 0);
                    if (_lastDailyExecutionDate != localDate && localNow.TimeOfDay >= scheduledAt)
                    {
                        var actor = _options.SystemActorUserId ?? Guid.Empty;
                        if (_options.AutoEnqueueDueCarryForward)
                        {
                            await scheduler.EnqueueDueRunsAsync(localDate, actor, true, stoppingToken);
                        }
                        if (_options.AutoProcessDueExpiry)
                        {
                            await scheduler.ProcessDueExpiryAsync(localDate, actor, stoppingToken);
                        }
                        _lastDailyExecutionDate = localDate;
                    }

                    await scheduler.ProcessNextQueuedRunAsync(
                        _options.SystemActorUserId ?? Guid.Empty,
                        stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Leave Carry Forward Scheduler mengalami error.");
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(Math.Max(10, _options.PollIntervalSeconds)),
                    stoppingToken);
            }
        }

        private DateTimeOffset GetLocalNow()
        {
            try
            {
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById(_options.TimeZoneId);
                return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone);
            }
            catch
            {
                try
                {
                    var windows = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                    return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, windows);
                }
                catch
                {
                    return DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7));
                }
            }
        }
    }
}
