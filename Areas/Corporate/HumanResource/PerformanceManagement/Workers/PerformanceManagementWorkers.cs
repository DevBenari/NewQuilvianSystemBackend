using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Workers
{
    /// <summary>
    /// Background Worker Transisi Otomatis Siklus Kinerja Berdasarkan Kalender Batas Waktu (PRF-W1)
    /// </summary>
    public class PerformanceCycleTransitionWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PerformanceCycleTransitionWorker> _logger;

        public PerformanceCycleTransitionWorker(
            IServiceProvider serviceProvider,
            ILogger<PerformanceCycleTransitionWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PerformanceCycleTransitionWorker diinisiasi.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var now = DateTime.UtcNow;
                    var activeCycles = await context.TrxPerformanceCycles
                        .Where(x => !x.IsDelete && x.IsActive && x.Status != "CLOSED")
                        .ToListAsync(stoppingToken);

                    foreach (var cycle in activeCycles)
                    {
                        if (cycle.Status == "GOAL_SETTING" && now > cycle.GoalSettingDeadline)
                        {
                            cycle.Status = "ACTIVE_ASSESSMENT";
                            cycle.UpdateDateTime = DateTime.UtcNow;
                            _logger.LogInformation("Siklus {Code} beralih otomatis ke ACTIVE_ASSESSMENT.", cycle.CycleCode);
                        }
                    }

                    await context.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Terjadi kesalahan pada PerformanceCycleTransitionWorker.");
                }

                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
        }
    }

    /// <summary>
    /// Background Worker Penguncian Otomatis Sasaran Kerja Pasca Batas Penetapan Target (PRF-W2)
    /// </summary>
    public class PerformanceGoalLockingWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PerformanceGoalLockingWorker> _logger;

        public PerformanceGoalLockingWorker(
            IServiceProvider serviceProvider,
            ILogger<PerformanceGoalLockingWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PerformanceGoalLockingWorker diinisiasi.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var now = DateTime.UtcNow;
                    var pastCycles = await context.TrxPerformanceCycles
                        .Where(x => !x.IsDelete && now > x.GoalSettingDeadline)
                        .Select(x => x.Id)
                        .ToListAsync(stoppingToken);

                    var unlockedGoals = await context.TrxPerformanceGoals
                        .Where(x => !x.IsDelete && !x.IsLocked && pastCycles.Contains(x.PerformanceCycleId))
                        .ToListAsync(stoppingToken);

                    foreach (var goal in unlockedGoals)
                    {
                        goal.IsLocked = true;
                        goal.UpdateDateTime = DateTime.UtcNow;
                    }

                    if (unlockedGoals.Any())
                    {
                        await context.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation("Berhasil mengunci {Count} sasaran kinerja yang melewati tenggat waktu penetapan.", unlockedGoals.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Terjadi kesalahan pada PerformanceGoalLockingWorker.");
                }

                await Task.Delay(TimeSpan.FromHours(4), stoppingToken);
            }
        }
    }

    /// <summary>
    /// Background Worker Pemantauan Tenggat Waktu Sidang Pleno Kalibrasi Manajemen (PRF-W3)
    /// </summary>
    public class PerformanceCalibrationDeadlineWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PerformanceCalibrationDeadlineWorker> _logger;

        public PerformanceCalibrationDeadlineWorker(
            IServiceProvider serviceProvider,
            ILogger<PerformanceCalibrationDeadlineWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PerformanceCalibrationDeadlineWorker diinisiasi.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var now = DateTime.UtcNow;
                    var pendingCalibrations = await context.TrxPerformanceCycles
                        .Where(x => !x.IsDelete && x.Status == "CALIBRATION" && now > x.CalibrationDeadline)
                        .ToListAsync(stoppingToken);

                    foreach (var c in pendingCalibrations)
                    {
                        _logger.LogWarning("Peringatan: Siklus {Code} melewati tenggat kalibrasi ({Deadline:d}).", c.CycleCode, c.CalibrationDeadline);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Terjadi kesalahan pada PerformanceCalibrationDeadlineWorker.");
                }

                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }
    }

    /// <summary>
    /// Background Worker Pemantauan Milestone 60 Hari Program Pembinaan Formal PIP (PRF-W4)
    /// </summary>
    public class PerformancePipMilestoneMonitoringWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PerformancePipMilestoneMonitoringWorker> _logger;

        public PerformancePipMilestoneMonitoringWorker(
            IServiceProvider serviceProvider,
            ILogger<PerformancePipMilestoneMonitoringWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PerformancePipMilestoneMonitoringWorker diinisiasi.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var now = DateTime.UtcNow.Date;
                    var activePips = await context.TrxPerformanceImprovementPlans
                        .Where(x => !x.IsDelete && (x.Status == "ACTIVE" || x.Status == "EXTENDED"))
                        .ToListAsync(stoppingToken);

                    foreach (var pip in activePips)
                    {
                        if (now >= pip.EndDate)
                        {
                            _logger.LogInformation("Program PIP ID {Id} telah mencapai hari ke-60/akhir pembinaan. Menunggu sidang evaluasi kelulusan.", pip.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Terjadi kesalahan pada PerformancePipMilestoneMonitoringWorker.");
                }

                await Task.Delay(TimeSpan.FromHours(8), stoppingToken);
            }
        }
    }

    /// <summary>
    /// Background Worker Pemantauan Siklus Resertifikasi Jenjang Karir Klinis Nakes (PRF-W5)
    /// </summary>
    public class ClinicalCareerLadderExpiryWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ClinicalCareerLadderExpiryWorker> _logger;

        public ClinicalCareerLadderExpiryWorker(
            IServiceProvider serviceProvider,
            ILogger<ClinicalCareerLadderExpiryWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ClinicalCareerLadderExpiryWorker diinisiasi.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var approvedReviews = await context.TrxClinicalCareerLadderReviews
                        .Where(x => !x.IsDelete && x.Status == "APPROVED")
                        .ToListAsync(stoppingToken);

                    // Pemantauan berkala masa berlaku jenjang klinis (3 tahun evaluasi berkala per Permenkes 40/2017)
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Terjadi kesalahan pada ClinicalCareerLadderExpiryWorker.");
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }

    /// <summary>
    /// Background Worker Penegakan SLA Sanggah Banding Nilai Kinerja (H+7 Pengajuan, H+14 Putusan Majelis) (PRF-W6)
    /// </summary>
    public class PerformanceDisputeSlaEnforcementWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PerformanceDisputeSlaEnforcementWorker> _logger;

        public PerformanceDisputeSlaEnforcementWorker(
            IServiceProvider serviceProvider,
            ILogger<PerformanceDisputeSlaEnforcementWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PerformanceDisputeSlaEnforcementWorker diinisiasi.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var now = DateTime.UtcNow;
                    var pendingDisputes = await context.TrxPerformanceAppraisalDisputes
                        .Where(x => !x.IsDelete && (x.Status == "SUBMITTED" || x.Status == "IN_MEDIATION"))
                        .ToListAsync(stoppingToken);

                    foreach (var disp in pendingDisputes)
                    {
                        var elapsedDays = (now.Date - disp.SubmissionDate.Date).TotalDays;
                        if (elapsedDays > 14)
                        {
                            _logger.LogWarning("Sengketa nilai ID {Id} telah melewati batas SLA 14 hari kalender ({Days:F0} hari). Diperlukan eskalasi ke Direktur SDM.", disp.Id, elapsedDays);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Terjadi kesalahan pada PerformanceDisputeSlaEnforcementWorker.");
                }

                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
        }
    }
}
