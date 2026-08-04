using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    public class LeaveAccrualSchedulerHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly LeaveAccrualSchedulerOptions _options;
        private readonly ILogger<LeaveAccrualSchedulerHostedService> _logger;
        private DateOnly? _lastAutoEnqueueDate;

        public LeaveAccrualSchedulerHostedService(
            IServiceScopeFactory scopeFactory,
            IOptions<LeaveAccrualSchedulerOptions> options,
            ILogger<LeaveAccrualSchedulerHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.Enabled)
            {
                _logger.LogInformation("Leave Accrual Scheduler dinonaktifkan melalui konfigurasi.");
                return;
            }

            var pollSeconds = Math.Max(10, _options.PollIntervalSeconds);
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(pollSeconds));

            _logger.LogInformation(
                "Leave Accrual Scheduler aktif. Worker={Worker}, Poll={PollSeconds}s.",
                _options.WorkerInstanceName,
                pollSeconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RunIterationAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Iterasi Leave Accrual Scheduler gagal.");
                }

                if (!await timer.WaitForNextTickAsync(stoppingToken))
                {
                    break;
                }
            }
        }

        private async Task RunIterationAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var scheduler = scope.ServiceProvider.GetRequiredService<LeaveAccrualSchedulerService>();
            var actorUserId = _options.SystemActorUserId ?? Guid.Empty;

            await scheduler.RecoverStaleRunsAsync(actorUserId, cancellationToken);

            var localNow = ConvertToConfiguredTimeZone(DateTime.UtcNow);
            var localDate = DateOnly.FromDateTime(localNow);
            var dueTimeReached = localNow.TimeOfDay >= new TimeSpan(
                Math.Clamp(_options.DailyEnqueueHour, 0, 23),
                Math.Clamp(_options.DailyEnqueueMinute, 0, 59),
                0);

            if (_options.AutoEnqueueDueAccruals &&
                dueTimeReached &&
                _lastAutoEnqueueDate != localDate)
            {
                var lookBack = Math.Clamp(_options.LookBackDays, 0, 31);
                for (var offset = lookBack; offset >= 0; offset--)
                {
                    await scheduler.EnqueueDueRunsAsync(
                        localDate.AddDays(-offset),
                        actorUserId,
                        queueForProcessing: true,
                        cancellationToken);
                }
                _lastAutoEnqueueDate = localDate;
            }

            var result = await scheduler.ProcessNextQueuedRunAsync(actorUserId, cancellationToken);
            if (result != null && !result.Success)
            {
                _logger.LogWarning(
                    "Queued leave accrual run gagal diproses: {Message}",
                    result.Message);
            }
        }

        private DateTime ConvertToConfiguredTimeZone(DateTime utcNow)
        {
            try
            {
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById(_options.TimeZoneId);
                return TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.SpecifyKind(utcNow, DateTimeKind.Utc),
                    timeZone);
            }
            catch (TimeZoneNotFoundException)
            {
                var fallbackId = string.Equals(
                    _options.TimeZoneId,
                    "Asia/Jakarta",
                    StringComparison.OrdinalIgnoreCase)
                    ? "SE Asia Standard Time"
                    : "Asia/Jakarta";

                try
                {
                    var fallback = TimeZoneInfo.FindSystemTimeZoneById(fallbackId);
                    return TimeZoneInfo.ConvertTimeFromUtc(
                        DateTime.SpecifyKind(utcNow, DateTimeKind.Utc),
                        fallback);
                }
                catch
                {
                    return utcNow.AddHours(7);
                }
            }
        }
    }
}
