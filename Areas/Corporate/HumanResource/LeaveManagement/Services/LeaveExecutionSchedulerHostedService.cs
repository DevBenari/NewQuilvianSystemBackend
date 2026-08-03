using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    public class LeaveExecutionSchedulerHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOptionsMonitor<LeaveExecutionSchedulerOptions> _options;
        private readonly ILogger<LeaveExecutionSchedulerHostedService> _logger;
        private DateOnly? _lastProcessedLocalDate;

        public LeaveExecutionSchedulerHostedService(
            IServiceScopeFactory scopeFactory,
            IOptionsMonitor<LeaveExecutionSchedulerOptions> options,
            ILogger<LeaveExecutionSchedulerHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var options = _options.CurrentValue;
                var delay = TimeSpan.FromSeconds(Math.Clamp(options.PollIntervalSeconds, 15, 3600));

                try
                {
                    if (options.Enabled && options.AutoProcessDueLeave)
                    {
                        var localNow = ConvertToLocal(DateTime.UtcNow, options.TimeZoneId);
                        var localDate = DateOnly.FromDateTime(localNow.Date);
                        var dueTime = new TimeOnly(
                            Math.Clamp(options.DailyProcessingHour, 0, 23),
                            Math.Clamp(options.DailyProcessingMinute, 0, 59));

                        if (TimeOnly.FromDateTime(localNow) >= dueTime &&
                            _lastProcessedLocalDate != localDate)
                        {
                            using var scope = _scopeFactory.CreateScope();
                            var processor = scope.ServiceProvider.GetRequiredService<LeaveExecutionProcessorService>();
                            var actorUserId = options.SystemActorUserId ?? Guid.Empty;

                            var result = await processor.ProcessDueAsync(
                                new ProcessDueLeaveRequest
                                {
                                    AsOfDate = localDate,
                                    MaximumItem = Math.Clamp(options.MaximumBatchSize, 1, 1000),
                                    ForceRetry = false,
                                    CorrelationId = $"LEAVE-EXECUTION-SCHEDULER-{localDate:yyyyMMdd}",
                                    Notes = $"Automatic leave execution by {options.WorkerInstanceName}."
                                },
                                actorUserId,
                                stoppingToken);

                            if (result.Success)
                            {
                                _lastProcessedLocalDate = localDate;
                                _logger.LogInformation(
                                    "Leave execution scheduler completed. Date={Date}, Total={Total}, Success={Success}, Failed={Failed}, Skipped={Skipped}",
                                    localDate,
                                    result.Data?.TotalItem ?? 0,
                                    result.Data?.SuccessCount ?? 0,
                                    result.Data?.FailedCount ?? 0,
                                    result.Data?.SkippedCount ?? 0);
                            }
                            else
                            {
                                _logger.LogWarning(
                                    "Leave execution scheduler failed. Date={Date}, Message={Message}",
                                    localDate,
                                    result.Message);
                            }
                        }
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Leave execution scheduler iteration failed.");
                }

                await Task.Delay(delay, stoppingToken);
            }
        }

        private static DateTime ConvertToLocal(DateTime utcNow, string timeZoneId)
        {
            var candidates = new[]
            {
                timeZoneId,
                string.Equals(timeZoneId, "Asia/Jakarta", StringComparison.OrdinalIgnoreCase)
                    ? "SE Asia Standard Time"
                    : timeZoneId,
                string.Equals(timeZoneId, "SE Asia Standard Time", StringComparison.OrdinalIgnoreCase)
                    ? "Asia/Jakarta"
                    : timeZoneId
            };

            foreach (var candidate in candidates.Distinct())
            {
                try
                {
                    var timeZone = TimeZoneInfo.FindSystemTimeZoneById(candidate);
                    return TimeZoneInfo.ConvertTimeFromUtc(
                        DateTime.SpecifyKind(utcNow, DateTimeKind.Utc),
                        timeZone);
                }
                catch
                {
                    // Try the next Windows/Linux identifier.
                }
            }

            return utcNow.AddHours(7);
        }
    }
}
