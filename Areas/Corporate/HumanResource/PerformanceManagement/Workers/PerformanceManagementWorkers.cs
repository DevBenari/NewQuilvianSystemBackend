using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Workers
{
    // Remediasi Gelombang 2 (10 November 2026, kelompok D backend-unfinished-work.md):
    // PRF-W3..PRF-W6 sebelumnya hanya menulis log (PRF-W5 bahkan tidak memeriksa apa pun). Kini temuan diteruskan
    // ke pusat pemberitahuan dalam aplikasi (IHrdNotificationService) dengan kunci anti-duplikat; penerima jabatan
    // ditentukan dari hak akses yang diberikan admin lewat layar Akses Role.

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
                    var activeCycles = await context.HrdPerformanceCycles
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
                    var pastCycles = await context.HrdPerformanceCycles
                        .Where(x => !x.IsDelete && now > x.GoalSettingDeadline)
                        .Select(x => x.Id)
                        .ToListAsync(stoppingToken);

                    var unlockedGoals = await context.HrdPerformanceGoals
                        .Where(x => !x.IsDelete && x.Status != "Approved_Locked" && pastCycles.Contains(x.PerformanceCycleId))
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
                    var notifier = scope.ServiceProvider.GetRequiredService<IHrdNotificationService>();

                    var now = DateTime.UtcNow;
                    var pendingCalibrations = await context.HrdPerformanceCycles
                        .AsNoTracking()
                        .Where(x => !x.IsDelete && x.Status == "CALIBRATION" && now > x.CalibrationDeadline)
                        .ToListAsync(stoppingToken);

                    foreach (var c in pendingCalibrations)
                    {
                        var overdueDays = (int)(now.Date - c.CalibrationDeadline.Date).TotalDays;
                        var sent = await notifier.NotifyPermissionHoldersAsync(
                            "PerformanceCalibration",
                            "ManageCalibration",
                            new HrdNotificationRequest(
                                Title: $"Siklus {c.CycleCode} melewati tenggat kalibrasi",
                                Body: $"Sidang pleno kalibrasi siklus kinerja {c.CycleCode} sudah melewati tenggat {c.CalibrationDeadline:dd MMMM yyyy} ({overdueDays} hari). Mohon selesaikan kalibrasi agar nilai akhir dapat dipublikasikan.",
                                Category: HrdNotificationCategories.Performance,
                                Priority: HrdNotificationPriorities.High,
                                DeepLinkUrl: $"/hrd/performance/calibration?cycleId={c.Id}",
                                SourceModule: "Hrd.Performance.PRF-W3",
                                ReferenceType: "HrdPerformanceCycle",
                                ReferenceId: c.Id,
                                DedupKey: $"PRF-W3|{c.Id:N}|{now:yyyyMMdd}"),
                            stoppingToken);

                        if (sent > 0)
                        {
                            _logger.LogWarning("PRF-W3: Peringatan tenggat kalibrasi siklus {Code} dikirim ke {Count} pejabat.", c.CycleCode, sent);
                        }
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
                    var notifier = scope.ServiceProvider.GetRequiredService<IHrdNotificationService>();

                    var today = DateTime.UtcNow.Date;
                    var activePips = await context.HrdPerformanceImprovementPlans
                        .AsNoTracking()
                        .Where(x => !x.IsDelete && (x.Status == "ACTIVE" || x.Status == "EXTENDED") && x.EndDate <= today)
                        .ToListAsync(stoppingToken);

                    foreach (var pip in activePips)
                    {
                        var request = new HrdNotificationRequest(
                            Title: $"Program PIP {pip.PipNumber} mencapai akhir masa pembinaan",
                            Body: $"Program pembinaan formal {pip.PipNumber} telah mencapai tanggal akhir {pip.EndDate:dd MMMM yyyy}. Sidang evaluasi kelulusan perlu dijadwalkan untuk memutuskan lulus, diperpanjang, atau ditindaklanjuti.",
                            Category: HrdNotificationCategories.Performance,
                            Priority: HrdNotificationPriorities.High,
                            DeepLinkUrl: $"/hrd/performance/pip/{pip.Id}",
                            SourceModule: "Hrd.Performance.PRF-W4",
                            ReferenceType: "HrdPerformanceImprovementPlan",
                            ReferenceId: pip.Id,
                            DedupKey: $"PRF-W4|{pip.Id:N}|{pip.EndDate:yyyyMMdd}");

                        var sentMentor = await notifier.NotifyWorkforceProfileAsync(pip.MentorWorkforceProfileId, request, stoppingToken);
                        var sentStaff = await notifier.NotifyPermissionHoldersAsync("PerformancePipAndCareer", "ManagePip", request, stoppingToken);

                        if (sentMentor + sentStaff > 0)
                        {
                            _logger.LogInformation("PRF-W4: Milestone akhir PIP {Number} dikirim ke {Mentor} mentor dan {Staff} petugas.", pip.PipNumber, sentMentor, sentStaff);
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
    /// <remarks>
    /// Jenjang klinis yang disetujui dievaluasi ulang setiap 3 tahun (Permenkes 40/2017). Worker memberi tahu
    /// nakes dan pengelola jenjang karir pada H-90 sebelum 3 tahun sejak persetujuan terakhir, dan sekali lagi saat
    /// masa 3 tahun terlampaui.
    /// </remarks>
    public class ClinicalCareerLadderExpiryWorker : BackgroundService
    {
        private const int RecertificationYears = 3;
        private const int ReminderDaysBefore = 90;

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
                    var notifier = scope.ServiceProvider.GetRequiredService<IHrdNotificationService>();

                    var now = DateTime.UtcNow;
                    var approvedReviews = await context.HrdClinicalCareerLadderReviews
                        .AsNoTracking()
                        .Where(x => !x.IsDelete && x.FinalStatus == "APPROVED" && x.ApprovedAt.HasValue)
                        .ToListAsync(stoppingToken);

                    // Hanya persetujuan terakhir per nakes yang menentukan tanggal resertifikasi
                    var latestPerStaff = approvedReviews
                        .GroupBy(x => x.WorkforceProfileId)
                        .Select(g => g.OrderByDescending(x => x.ApprovedAt).First())
                        .ToList();

                    foreach (var review in latestPerStaff)
                    {
                        var dueDate = review.ApprovedAt!.Value.AddYears(RecertificationYears);
                        var daysLeft = (int)(dueDate.Date - now.Date).TotalDays;
                        if (daysLeft > ReminderDaysBefore) continue;

                        var isExpired = daysLeft < 0;
                        var request = new HrdNotificationRequest(
                            Title: isExpired
                                ? $"Masa jenjang karir klinis {review.ReviewNumber} telah lewat 3 tahun"
                                : $"Resertifikasi jenjang karir klinis {review.ReviewNumber} dalam {daysLeft} hari",
                            Body: isExpired
                                ? $"Penetapan jenjang karir klinis {review.ReviewNumber} disetujui pada {review.ApprovedAt:dd MMMM yyyy} dan sudah melewati masa evaluasi berkala 3 tahun. Pengajuan resertifikasi perlu segera dilakukan."
                                : $"Penetapan jenjang karir klinis {review.ReviewNumber} akan mencapai masa evaluasi berkala 3 tahun pada {dueDate:dd MMMM yyyy}. Mohon siapkan berkas resertifikasi.",
                            Category: HrdNotificationCategories.Performance,
                            Priority: isExpired ? HrdNotificationPriorities.High : HrdNotificationPriorities.Normal,
                            DeepLinkUrl: $"/hrd/performance/clinical-career/{review.Id}",
                            SourceModule: "Hrd.Performance.PRF-W5",
                            ReferenceType: "HrdClinicalCareerLadderReview",
                            ReferenceId: review.Id,
                            DedupKey: $"PRF-W5|{review.Id:N}|{(isExpired ? "EXPIRED" : "H90")}");

                        var sentStaff = await notifier.NotifyWorkforceProfileAsync(review.WorkforceProfileId, request, stoppingToken);
                        var sentManager = await notifier.NotifyPermissionHoldersAsync("PerformancePipAndCareer", "ManageClinicalCareer", request, stoppingToken);

                        if (sentStaff + sentManager > 0)
                        {
                            _logger.LogInformation("PRF-W5: Pengingat resertifikasi {Number} dikirim ke {Staff} akun nakes dan {Mgr} pengelola.", review.ReviewNumber, sentStaff, sentManager);
                        }
                    }
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
                    var notifier = scope.ServiceProvider.GetRequiredService<IHrdNotificationService>();

                    var now = DateTime.UtcNow;
                    var pendingDisputes = await context.HrdPerformanceAppraisalDisputes
                        .AsNoTracking()
                        .Where(x => !x.IsDelete && (x.Status == "SUBMITTED" || x.Status == "IN_MEDIATION"))
                        .ToListAsync(stoppingToken);

                    foreach (var disp in pendingDisputes)
                    {
                        var elapsedDays = (int)(now.Date - disp.SubmissionDate.Date).TotalDays;
                        if (elapsedDays <= 14) continue;

                        var sent = await notifier.NotifyPermissionHoldersAsync(
                            "PerformanceCalibration",
                            "ManageDispute",
                            new HrdNotificationRequest(
                                Title: $"Sengketa nilai {disp.DisputeNumber} melewati SLA 14 hari",
                                Body: $"Sanggahan nilai kinerja {disp.DisputeNumber} diajukan {elapsedDays} hari lalu dan belum diputus majelis (batas 14 hari kalender). Mohon segera tetapkan putusan atau eskalasikan ke Direktur SDM.",
                                Category: HrdNotificationCategories.Performance,
                                Priority: HrdNotificationPriorities.High,
                                DeepLinkUrl: $"/hrd/performance/disputes/{disp.Id}",
                                SourceModule: "Hrd.Performance.PRF-W6",
                                ReferenceType: "HrdPerformanceAppraisalDispute",
                                ReferenceId: disp.Id,
                                DedupKey: $"PRF-W6|{disp.Id:N}|{now:yyyyMMdd}"),
                            stoppingToken);

                        if (sent > 0)
                        {
                            _logger.LogWarning("PRF-W6: Eskalasi SLA sengketa {Number} ({Days} hari) dikirim ke {Count} pejabat.", disp.DisputeNumber, elapsedDays, sent);
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
