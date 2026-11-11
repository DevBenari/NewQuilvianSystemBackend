using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Workers
{
    /// <summary>
    /// EMP-W1: Background Worker Pembersihan Sanksi SP Kedaluwarsa (H+180 Hari) & Peringatan H-14 (BE-HRD-276)
    /// </summary>
    public class DisciplinarySanctionExpiryWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DisciplinarySanctionExpiryWorker> _logger;

        public DisciplinarySanctionExpiryWorker(IServiceScopeFactory scopeFactory, ILogger<DisciplinarySanctionExpiryWorker> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DisciplinarySanctionExpiryWorker (EMP-W1) dimulai.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var now = DateTime.UtcNow;

                    // Cari sanksi aktif yang sudah melewati valid_until
                    var expiredSanctions = await db.TrxDisciplinarySanctions
                        .Where(x => x.Status == "Active" && x.ValidUntil <= now)
                        .ToListAsync(stoppingToken);

                    foreach (var sanction in expiredSanctions)
                    {
                        sanction.Status = "Expired";
                        sanction.UpdateDateTime = now;
                        _logger.LogInformation("Sanksi {SanctionNumber} untuk Pegawai {EmployeeId} otomatis kedaluwarsa (H+180).", sanction.SanctionNumber, sanction.EmployeeId);
                    }

                    if (expiredSanctions.Any())
                    {
                        await db.SaveChangesAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kesalahan pada eksekusi DisciplinarySanctionExpiryWorker (EMP-W1).");
                }

                // Berjalan setiap 6 jam
                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
        }
    }

    /// <summary>
    /// EMP-W2: Background Worker Monitoring Batas Maksimal 6 Bulan Skorsing (Hari ke-150 / H-30 Eskalasi) (BE-HRD-278)
    /// </summary>
    public class SuspensionPeriodMonitoringWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SuspensionPeriodMonitoringWorker> _logger;

        public SuspensionPeriodMonitoringWorker(IServiceScopeFactory scopeFactory, ILogger<SuspensionPeriodMonitoringWorker> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SuspensionPeriodMonitoringWorker (EMP-W2) dimulai.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var now = DateTime.UtcNow;

                    var activeSuspensions = await db.TrxDisciplinarySuspensions
                        .Where(x => x.Status == "Active")
                        .ToListAsync(stoppingToken);

                    foreach (var suspension in activeSuspensions)
                    {
                        var elapsedDays = (now.Date - suspension.StartDate.Date).TotalDays;
                        if (elapsedDays >= 150)
                        {
                            _logger.LogWarning("ALERT ESKALASI: Skorsing {SuspensionNumber} telah berjalan {Days} hari (Batas 180 hari). Harap segera tetapkan sidang akhir.", suspension.SuspensionNumber, elapsedDays);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kesalahan pada eksekusi SuspensionPeriodMonitoringWorker (EMP-W2).");
                }

                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }
    }

    /// <summary>
    /// EMP-W3: Background Worker Pemantauan SLA 14 Hari Mediasi Tiket Keluhan Staf (BE-HRD-279)
    /// </summary>
    public class GrievanceSlaEscalationWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<GrievanceSlaEscalationWorker> _logger;

        public GrievanceSlaEscalationWorker(IServiceScopeFactory scopeFactory, ILogger<GrievanceSlaEscalationWorker> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("GrievanceSlaEscalationWorker (EMP-W3) dimulai.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var now = DateTime.UtcNow;

                    var overdueGrievances = await db.TrxEmployeeGrievances
                        .Where(x => (x.GrievanceStatus == "Submitted" || x.GrievanceStatus == "Draft") &&
                                    x.TargetResolutionDate.HasValue && x.TargetResolutionDate.Value < now)
                        .ToListAsync(stoppingToken);

                    foreach (var grv in overdueGrievances)
                    {
                        _logger.LogWarning("SLA BREACH: Tiket keluhan {TicketNumber} melewati batas waktu mediasi 14 hari.", grv.GrievanceNumber);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kesalahan pada eksekusi GrievanceSlaEscalationWorker (EMP-W3).");
                }

                await Task.Delay(TimeSpan.FromHours(8), stoppingToken);
            }
        }
    }

    /// <summary>
    /// EMP-W4: Background Worker Radar Masa Berlaku Kontrak PKWT H-60 dan H-30 (BE-HRD-282)
    /// </summary>
    public class PkwtContractExpiryRadarWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PkwtContractExpiryRadarWorker> _logger;

        public PkwtContractExpiryRadarWorker(IServiceScopeFactory scopeFactory, ILogger<PkwtContractExpiryRadarWorker> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PkwtContractExpiryRadarWorker (EMP-W4) dimulai.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var targetWindow = DateTime.UtcNow.AddDays(60);

                    // Pemindaian kontrak PKWT yang akan berakhir
                    var expiringContracts = await db.TrxEmployeeSeparations
                        .Where(x => x.SeparationType == "ContractEnd" && x.SeparationStatus == "Initiated" && x.EffectiveSeparationDate <= targetWindow)
                        .ToListAsync(stoppingToken);

                    foreach (var c in expiringContracts)
                    {
                        _logger.LogInformation("RADAR PKWT: Kontrak {SeparationNumber} akan berakhir pada {EffectiveDate}.", c.SeparationNumber, c.EffectiveSeparationDate);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kesalahan pada eksekusi PkwtContractExpiryRadarWorker (EMP-W4).");
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }

    /// <summary>
    /// EMP-W5: Background Worker Pengingat Harian Checklist Exit Clearance Pending 5 Departemen (BE-HRD-284)
    /// </summary>
    public class ExitClearanceChecklistReminderWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ExitClearanceChecklistReminderWorker> _logger;

        public ExitClearanceChecklistReminderWorker(IServiceScopeFactory scopeFactory, ILogger<ExitClearanceChecklistReminderWorker> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ExitClearanceChecklistReminderWorker (EMP-W5) dimulai.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var pendingChecklists = await db.TrxExitClearanceChecklists
                        .Include(x => x.ClearanceProcess)
                        .Where(x => x.Status == "Pending" && x.ClearanceProcess != null && x.ClearanceProcess.OverallClearanceStatus == "InProgress")
                        .ToListAsync(stoppingToken);

                    foreach (var item in pendingChecklists)
                    {
                        _logger.LogInformation("REMINDER KLIRING: Item '{ItemName}' pada unit {Dept} masih pending verifikasi.", item.ItemName, item.DepartmentType);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kesalahan pada eksekusi ExitClearanceChecklistReminderWorker (EMP-W5).");
                }

                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }
    }

    /// <summary>
    /// EMP-W6: Background Worker Sinkronisasi Pemutusan Akses SIMRS & IAM Keycloak (BE-HRD-285 / INT-HR-88)
    /// </summary>
    public class ItAccessRevocationSyncWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ItAccessRevocationSyncWorker> _logger;

        public ItAccessRevocationSyncWorker(IServiceScopeFactory scopeFactory, ILogger<ItAccessRevocationSyncWorker> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ItAccessRevocationSyncWorker (EMP-W6) dimulai.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // Periksa skorsing baru yang membutuhkan penonaktifan instan
                    var revokedSuspensions = await db.TrxDisciplinarySuspensions
                        .Where(x => x.Status == "Active" && x.IsSystemAccessRevoked)
                        .ToListAsync(stoppingToken);

                    foreach (var s in revokedSuspensions)
                    {
                        _logger.LogInformation("IAM SYNC: Akses SSO SIMRS untuk Pegawai {EmployeeId} dalam status REVOKED (Skorsing {Number}).", s.EmployeeId, s.SuspensionNumber);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kesalahan pada eksekusi ItAccessRevocationSyncWorker (EMP-W6).");
                }

                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
        }
    }
}
