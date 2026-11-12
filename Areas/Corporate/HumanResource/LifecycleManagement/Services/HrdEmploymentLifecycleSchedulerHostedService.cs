using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public class HrdEmploymentLifecycleSchedulerHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<HrdEmploymentLifecycleSchedulerHostedService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

        public HrdEmploymentLifecycleSchedulerHostedService(
            IServiceProvider serviceProvider,
            ILogger<HrdEmploymentLifecycleSchedulerHostedService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("HrdEmploymentLifecycleSchedulerHostedService dimulai.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var scheduler = scope.ServiceProvider.GetRequiredService<IHrdEmploymentLifecycleSchedulerService>();
                    await scheduler.ExecuteDailyTickAsync(null, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Terjadi kesalahan saat mengeksekusi HrdEmploymentLifecycleSchedulerHostedService.");
                }

                try
                {
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation("HrdEmploymentLifecycleSchedulerHostedService berhenti.");
        }
    }
}
