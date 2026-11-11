using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Workers
{
    /// <summary>
    /// WF-W1: Background Worker Pemindai Batas Waktu SLA & Auto-Eskalasi (BE-HRD-310, HRD-DEC-185, VR-WF-04)
    /// </summary>
    public class WorkflowSlaEscalationWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WorkflowSlaEscalationWorker> _logger;

        public WorkflowSlaEscalationWorker(IServiceProvider serviceProvider, ILogger<WorkflowSlaEscalationWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("WorkflowSlaEscalationWorker (WF-W1) dimulai. Memindai pelanggaran SLA setiap 15 menit.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var slaService = scope.ServiceProvider.GetRequiredService<IWorkflowSlaEnforcementService>();
                    var escalated = await slaService.ScanAndEscalateOverdueTicketsAsync();
                    if (escalated > 0)
                    {
                        _logger.LogWarning("WF-W1: Berhasil mengeskalasikan {Count} tiket workflow yang melampaui batas SLA.", escalated);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "WF-W1: Kesalahan saat memindai batas waktu SLA.");
                }

                // Berjalan periodik setiap 15 menit
                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
        }
    }

    /// <summary>
    /// WF-W2: Background Worker Pencabutan Otomatis Wewenang Delegasi Plh Kedaluwarsa (BE-HRD-308, HRD-DEC-184)
    /// </summary>
    public class ApprovalDelegationExpiryWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ApprovalDelegationExpiryWorker> _logger;

        public ApprovalDelegationExpiryWorker(IServiceProvider serviceProvider, ILogger<ApprovalDelegationExpiryWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ApprovalDelegationExpiryWorker (WF-W2) dimulai. Memindai masa berlaku Plh harian.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var now = DateTime.UtcNow;

                    var expiredDelegations = await db.TrxApprovalDelegations
                        .Where(d => d.IsActive && d.EndDate < now)
                        .ToListAsync(stoppingToken);

                    foreach (var d in expiredDelegations)
                    {
                        d.IsActive = false;
                        d.DelegationStatus = "Expired";
                    }

                    if (expiredDelegations.Count > 0)
                    {
                        await db.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation("WF-W2: Berhasil mencabut otomatis {Count} surat kuasa Plh yang telah kedaluwarsa.", expiredDelegations.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "WF-W2: Kesalahan saat mencabut delegasi Plh kedaluwarsa.");
                }

                // Berjalan setiap 6 jam
                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
        }
    }

    /// <summary>
    /// WF-W3: Background Worker Ringkasan Harian Permohonan Tertunda Atasan Pukul 07:00 Pagi (BE-HRD-314, WF-B7, INT-HR-96)
    /// </summary>
    public class DailyApprovalDigestWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DailyApprovalDigestWorker> _logger;

        public DailyApprovalDigestWorker(IServiceProvider serviceProvider, ILogger<DailyApprovalDigestWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DailyApprovalDigestWorker (WF-W3) dimulai. Menyiapkan ringkasan persetujuan 07:00 pagi.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var notifService = scope.ServiceProvider.GetRequiredService<IUnifiedNotificationGatewayService>();
                    await notifService.SendApprovalDigestAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "WF-W3: Kesalahan saat mengirimkan ringkasan harian persetujuan.");
                }

                // Berjalan setiap 12 jam
                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }
    }

    /// <summary>
    /// WF-W4: Background Worker Sirene Peringatan Dini H-4 Jam SLA (BE-HRD-311, WF-B4)
    /// </summary>
    public class WorkflowSlaEarlyWarningWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WorkflowSlaEarlyWarningWorker> _logger;

        public WorkflowSlaEarlyWarningWorker(IServiceProvider serviceProvider, ILogger<WorkflowSlaEarlyWarningWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("WorkflowSlaEarlyWarningWorker (WF-W4) dimulai. Memindai tiket H-4 jam SLA setiap 30 menit.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var slaService = scope.ServiceProvider.GetRequiredService<IWorkflowSlaEnforcementService>();
                    var warningCount = await slaService.ScanEarlyWarningTicketsAsync();
                    if (warningCount > 0)
                    {
                        _logger.LogInformation("WF-W4: Terdeteksi {Count} tiket pending yang mendekati batas SLA H-4 jam.", warningCount);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "WF-W4: Kesalahan saat memindai peringatan dini H-4 jam SLA.");
                }

                // Berjalan periodik setiap 30 menit
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }
    }

    /// <summary>
    /// WF-W5: Background Worker Pemrosesan Antrean Persetujuan Massal Latar Belakang (BE-HRD-313, WF-B6, HRD-DEC-187)
    /// </summary>
    public class BulkApprovalProcessingWorker : BackgroundService
    {
        private readonly ILogger<BulkApprovalProcessingWorker> _logger;

        public BulkApprovalProcessingWorker(ILogger<BulkApprovalProcessingWorker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BulkApprovalProcessingWorker (WF-W5) dimulai. Memantau antrean batch persetujuan massal.");

            while (!stoppingToken.IsCancellationRequested)
            {
                // Standby worker untuk pemrosesan batch antrean asinkronus jika antrean masuk
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }

    /// <summary>
    /// WF-W6: Background Worker Pembatalan Otomatis Tiket Revisi Kedaluwarsa > 7 Hari (BE-HRD-315, VR-WF-06)
    /// </summary>
    public class ReturnedRequestExpiryWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ReturnedRequestExpiryWorker> _logger;

        public ReturnedRequestExpiryWorker(IServiceProvider serviceProvider, ILogger<ReturnedRequestExpiryWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ReturnedRequestExpiryWorker (WF-W6) dimulai. Memindai tiket revisi > 7 hari.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var resubmitService = scope.ServiceProvider.GetRequiredService<IWorkflowResubmitService>();
                    var cancelled = await resubmitService.CancelExpiredReturnedRequestsAsync();
                    if (cancelled > 0)
                    {
                        _logger.LogInformation("WF-W6: Membatalkan otomatis {Count} tiket revisi yang diabaikan > 7 hari.", cancelled);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "WF-W6: Kesalahan saat memindai tiket revisi kedaluwarsa.");
                }

                // Berjalan setiap 6 jam
                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
        }
    }
}
