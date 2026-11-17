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
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Workers
{
    /// <summary>
    /// Background Worker Pemantauan Batas Proctoring FPPE &amp; Evaluasi Anomali Komplikasi (OPP-W3, OPP-W6)
    /// </summary>
    /// <remarks>
    /// Remediasi Gelombang 2 (10 November 2026): peringatan H-14 sebelumnya hanya ditulis ke log. Kini dikirim ke
    /// pusat pemberitahuan dalam aplikasi untuk proctor yang ditugaskan, dokter yang dievaluasi, dan pemegang hak akses
    /// <c>FppeFocusedEvaluation : ApproveConclusion</c> (komite yang memutus kesimpulan FPPE), satu kali per penugasan
    /// per ambang (H-14, H-7, lewat tenggat).
    /// </remarks>
    public class FppeProctoringMonitoringWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FppeProctoringMonitoringWorker> _logger;

        public FppeProctoringMonitoringWorker(
            IServiceProvider serviceProvider,
            ILogger<FppeProctoringMonitoringWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("FppeProctoringMonitoringWorker diinisiasi dan mulai berjalan.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var notifier = scope.ServiceProvider.GetRequiredService<IHrdNotificationService>();

                    await CheckProctoringDeadlinesAsync(context, notifier, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Terjadi kesalahan pada siklus pemantauan FppeProctoringMonitoringWorker.");
                }

                // Berjalan setiap 12 jam (atau 1 jam di staging)
                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }

        private async Task CheckProctoringDeadlinesAsync(ApplicationDbContext context, IHrdNotificationService notifier, CancellationToken stoppingToken)
        {
            var now = DateTime.UtcNow;
            var h14Threshold = now.AddDays(14);

            // Cari penugasan FPPE aktif yang batas waktunya tersisa <= 14 hari
            var expiringEngagements = await context.HrdFppeEngagements
                .AsNoTracking()
                .Where(e => (e.Status == "ACTIVE" || e.Status == "EXTENDED") && e.TargetEndDate <= h14Threshold && !e.IsDelete)
                .ToListAsync(stoppingToken);

            foreach (var eng in expiringEngagements)
            {
                var remainingDays = (int)Math.Ceiling((eng.TargetEndDate - now).TotalDays);
                var stage = remainingDays < 0 ? "OVERDUE" : remainingDays <= 7 ? "H7" : "H14";
                var isOverdue = remainingDays < 0;

                var request = new HrdNotificationRequest(
                    Title: isOverdue
                        ? $"Masa observasi FPPE {eng.EngagementNumber} telah berakhir"
                        : $"Masa observasi FPPE {eng.EngagementNumber} tersisa {remainingDays} hari",
                    Body: $"Penugasan FPPE {eng.EngagementNumber}: target {eng.TargetCaseCount} kasus, tercatat {eng.CompletedCaseCount} kasus. " +
                          (isOverdue
                              ? "Tenggat observasi sudah terlampaui; kesimpulan FPPE atau perpanjangan masa observasi perlu segera diputuskan."
                              : "Mohon lengkapi logbook kasus dan verifikasi proctor sebelum tenggat berakhir."),
                    Category: HrdNotificationCategories.Performance,
                    Priority: isOverdue ? HrdNotificationPriorities.Critical : HrdNotificationPriorities.High,
                    DeepLinkUrl: $"/hrd/performance/fppe/engagements/{eng.Id}",
                    SourceModule: "Hrd.Performance.OPP-W3",
                    ReferenceType: nameof(HrdFppeEngagement),
                    ReferenceId: eng.Id,
                    DedupKey: $"OPP-W3|{eng.Id:N}|{stage}");

                var sentProctor = await notifier.NotifyWorkforceProfileAsync(eng.AssignedProctorWorkforceId, request, stoppingToken);
                var sentDoctor = await notifier.NotifyWorkforceProfileAsync(eng.WorkforceProfileId, request, stoppingToken);
                var sentCommittee = await notifier.NotifyPermissionHoldersAsync("FppeFocusedEvaluation", "ApproveConclusion", request, stoppingToken);

                if (sentProctor + sentDoctor + sentCommittee > 0)
                {
                    _logger.LogWarning("FPPE DEADLINE ALERT (OPP-W3): Penugasan {EngagementNumber} ({Stage}) dikirim ke {Proctor} proctor, {Doctor} dokter, {Committee} anggota komite.",
                        eng.EngagementNumber, stage, sentProctor, sentDoctor, sentCommittee);
                }
            }
        }
    }
}
