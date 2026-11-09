namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Services
{
    public class NvwPatientCallReconciliationHostedService : BackgroundService
    {
        private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NvwPatientCallReconciliationHostedService> _logger;

        public NvwPatientCallReconciliationHostedService(
            IServiceScopeFactory scopeFactory,
            ILogger<NvwPatientCallReconciliationHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Pemeriksaan penutupan otomatis panggilan Nurse View berjalan tiap {PollIntervalSeconds} detik.",
                PollInterval.TotalSeconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var reconciliation = scope.ServiceProvider.GetRequiredService<NvwPatientCallReconciliationService>();
                    var summary = await reconciliation.CloseCallsForDepartedPatientsAsync(stoppingToken);

                    if (summary.Candidates > 0)
                    {
                        _logger.LogInformation(
                            "Pemeriksaan penutupan otomatis panggilan Nurse View: {Candidates} kandidat, {Closed} ditutup, {Skipped} dilewati, {Failed} gagal.",
                            summary.Candidates,
                            summary.Closed,
                            summary.Skipped,
                            summary.Failed);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    _logger.LogError(
                        exception,
                        "Pemeriksaan penutupan otomatis panggilan Nurse View gagal; putaran berikutnya tetap berjalan.");
                }

                try
                {
                    await Task.Delay(PollInterval, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }

            _logger.LogInformation("Pemeriksaan penutupan otomatis panggilan Nurse View berhenti.");
        }
    }
}
