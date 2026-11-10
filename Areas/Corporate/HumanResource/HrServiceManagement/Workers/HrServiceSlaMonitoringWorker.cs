using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Workers
{
    public class HrServiceSlaMonitoringWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<HrServiceSlaMonitoringWorker> _logger;

        // Interval evaluasi timer SLA berkala tiap 15 menit (HRS-W3)
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(15);

        public HrServiceSlaMonitoringWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<HrServiceSlaMonitoringWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[SLA-WORKER-INIT] HrServiceSlaMonitoringWorker dimulai. Interval polling: {Interval} menit", _checkInterval.TotalMinutes);

            // Tunggu 30 detik pertama sebelum evaluasi perdana saat startup
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await MonitorActiveTicketsSlaAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[SLA-WORKER-ERR] Terjadi kesalahan tidak terduga saat memantau timer SLA tiket SDM");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("[SLA-WORKER-STOP] HrServiceSlaMonitoringWorker telah dihentikan");
        }

        public async Task MonitorActiveTicketsSlaAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var slaEngine = scope.ServiceProvider.GetRequiredService<IHrServiceSlaEngine>();

            var nowUtc = DateTime.UtcNow;

            // Ambil seluruh tiket bantuan aktif yang belum tuntas (bukan Resolved, Closed, Cancelled)
            var activeTickets = await dbContext.TrxHrServiceRequests
                .Where(x => !x.IsDelete && 
                            x.RequestStatus != "Resolved" && 
                            x.RequestStatus != "Closed" && 
                            x.RequestStatus != "Cancelled" &&
                            x.SlaDeadlineUtc != null)
                .ToListAsync(cancellationToken);

            if (activeTickets.Count == 0) return;

            int warning75Count = 0;
            int escalatedCount = 0;

            foreach (var ticket in activeTickets)
            {
                // Aturan Bisnis HRS-W2: Tiket yang sedang berstatus WaitingStaffResponse timer SLA-nya membeku
                if (ticket.RequestStatus == "WaitingStaffResponse")
                {
                    continue;
                }

                var (elapsedHours, percentage, isBreached) = slaEngine.EvaluateSlaProgress(ticket, nowUtc);

                // 1. Evaluasi Peringatan Dini 75% (HRS-W3)
                if (percentage >= 75.0 && percentage < 100.0 && ticket.SlaStatus == "WithinSla")
                {
                    ticket.SlaStatus = "Warning75";
                    ticket.UpdateDateTime = nowUtc;
                    warning75Count++;

                    _logger.LogWarning("[SLA-HRS-ALERT75] Tiket {TicketNumber} (Tier: {Tier}, SLA: {TargetHours} Jam, Elapsed: {Elapsed:F1} Jam / {Percentage:F1}%) telah mencapai 75% batas durasi SLA.",
                        ticket.RequestNumber, ticket.PriorityTier, ticket.SlaTargetHours, elapsedHours, percentage);
                }
                // 2. Evaluasi Pelanggaran SLA & Auto-Eskalasi 100% ke Manajer SDM (HRS-W3, INT-HR-68)
                else if (isBreached || percentage >= 100.0)
                {
                    ticket.IsSlaBreached = true;

                    if (ticket.SlaStatus != "Escalated")
                    {
                        ticket.SlaStatus = "Escalated";
                        ticket.UpdateDateTime = nowUtc;
                        escalatedCount++;

                        _logger.LogError("[SLA-HRS-ESCALATED] Keterlambatan SLA 100%: Tiket {TicketNumber} (Deadline: {DeadlineUtc}) telah melewati batas waktu SLA dan otomatis DIESKALASI ke Manajer SDM.",
                            ticket.RequestNumber, ticket.SlaDeadlineUtc);
                    }
                }
            }

            if (warning75Count > 0 || escalatedCount > 0)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("[SLA-WORKER-SUMMARY] Pemantauan SLA selesai. Tiket Peringatan 75%: {WarnCount}, Tiket Auto-Eskalasi: {EscCount}",
                    warning75Count, escalatedCount);
            }
        }
    }
}
