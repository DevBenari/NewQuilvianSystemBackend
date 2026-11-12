using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Workers
{
    // Remediasi Gelombang 2 (10 November 2026, kelompok D backend-unfinished-work.md):
    // worker EMP-W2..EMP-W6 sebelumnya hanya menulis log. Kini setiap temuan diteruskan ke pusat pemberitahuan
    // dalam aplikasi (IHrdNotificationService) dengan kunci anti-duplikat, dan EMP-W6 benar-benar mengunci /
    // memulihkan akun aplikasi pegawai yang diskors. Penerima "petugas SDM" ditentukan dari hak akses yang
    // diberikan admin lewat layar Akses Role, bukan dari nama peran di kode.

    /// <summary>
    /// EMP-W1: Background Worker Pembersihan Sanksi SP Kedaluwarsa (H+180 Hari) &amp; Peringatan H-14 (BE-HRD-276)
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
    /// <remarks>
    /// Pada hari ke-150 skorsing aktif, worker mengirim eskalasi prioritas tinggi ke setiap pemegang hak akses
    /// <c>EmployeeDisciplinary : ManageSuspension</c> (petugas yang berwenang menetapkan sidang akhir), satu kali
    /// per surat skorsing. Pada hari ke-180 eskalasi diulang dengan prioritas kritis.
    /// </remarks>
    public class SuspensionPeriodMonitoringWorker : BackgroundService
    {
        private const int EscalationDay = 150;
        private const int MaximumDay = 180;

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
                    var notifier = scope.ServiceProvider.GetRequiredService<IHrdNotificationService>();
                    var now = DateTime.UtcNow;

                    var activeSuspensions = await db.TrxDisciplinarySuspensions
                        .AsNoTracking()
                        .Where(x => x.Status == "Active")
                        .ToListAsync(stoppingToken);

                    foreach (var suspension in activeSuspensions)
                    {
                        var elapsedDays = (int)(now.Date - suspension.StartDate.Date).TotalDays;
                        if (elapsedDays < EscalationDay) continue;

                        var isOverMaximum = elapsedDays >= MaximumDay;
                        var sent = await notifier.NotifyPermissionHoldersAsync(
                            "EmployeeDisciplinary",
                            "ManageSuspension",
                            new HrdNotificationRequest(
                                Title: isOverMaximum
                                    ? $"Skorsing {suspension.SuspensionNumber} melewati batas 6 bulan"
                                    : $"Skorsing {suspension.SuspensionNumber} memasuki hari ke-{elapsedDays}",
                                Body: isOverMaximum
                                    ? $"Skorsing investigasi {suspension.SuspensionNumber} sudah berjalan {elapsedDays} hari, melebihi batas maksimal 180 hari kalender. Sidang akhir (rehabilitasi atau penetapan PHK) wajib segera ditetapkan untuk mencegah pelanggaran ketentuan ketenagakerjaan."
                                    : $"Skorsing investigasi {suspension.SuspensionNumber} sudah berjalan {elapsedDays} hari dari batas maksimal 180 hari. Mohon jadwalkan sidang akhir sebelum hari ke-180.",
                                Category: HrdNotificationCategories.Discipline,
                                Priority: isOverMaximum ? HrdNotificationPriorities.Critical : HrdNotificationPriorities.High,
                                DeepLinkUrl: $"/hrd/employee-relations/suspensions/{suspension.Id}",
                                SourceModule: "Hrd.EmployeeRelation.EMP-W2",
                                ReferenceType: nameof(TrxDisciplinarySuspension),
                                ReferenceId: suspension.Id,
                                DedupKey: $"EMP-W2|{suspension.Id:N}|{(isOverMaximum ? "D180" : "D150")}"),
                            stoppingToken);

                        if (sent > 0)
                        {
                            _logger.LogWarning("EMP-W2: Eskalasi skorsing {Number} hari ke-{Days} dikirim ke {Count} pejabat.", suspension.SuspensionNumber, elapsedDays, sent);
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
    /// <remarks>
    /// Tiket keluhan yang melewati <c>TargetResolutionDate</c> tanpa penyelesaian dieskalasikan ke pemegang hak akses
    /// <c>EmployeeGrievance : Mediate</c> (petugas SDM yang berwenang memediasi), satu kali per tiket per hari.
    /// </remarks>
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
                    var notifier = scope.ServiceProvider.GetRequiredService<IHrdNotificationService>();
                    var now = DateTime.UtcNow;

                    var overdueGrievances = await db.TrxEmployeeGrievances
                        .AsNoTracking()
                        .Where(x => (x.GrievanceStatus == "Submitted" || x.GrievanceStatus == "Draft" || x.GrievanceStatus == "InMediation") &&
                                    x.TargetResolutionDate.HasValue && x.TargetResolutionDate.Value < now && !x.IsDelete)
                        .ToListAsync(stoppingToken);

                    foreach (var grv in overdueGrievances)
                    {
                        var overdueDays = (int)(now.Date - grv.TargetResolutionDate!.Value.Date).TotalDays;
                        var sent = await notifier.NotifyPermissionHoldersAsync(
                            "EmployeeGrievance",
                            "Mediate",
                            new HrdNotificationRequest(
                                Title: $"Tiket keluhan {grv.GrievanceNumber} melewati SLA mediasi 14 hari",
                                Body: $"Tiket keluhan {grv.GrievanceNumber} ({grv.Subject}) sudah melewati batas mediasi {overdueDays} hari (VR-EMP-14). Mohon segera catat hasil klarifikasi atau mediasi agar keluhan tidak berlarut.",
                                Category: HrdNotificationCategories.Grievance,
                                Priority: HrdNotificationPriorities.High,
                                DeepLinkUrl: $"/hrd/employee-relations/grievances/{grv.Id}",
                                SourceModule: "Hrd.EmployeeRelation.EMP-W3",
                                ReferenceType: nameof(TrxEmployeeGrievance),
                                ReferenceId: grv.Id,
                                DedupKey: $"EMP-W3|{grv.Id:N}|{now:yyyyMMdd}"),
                            stoppingToken);

                        if (sent > 0)
                        {
                            _logger.LogWarning("EMP-W3: Eskalasi SLA tiket keluhan {TicketNumber} dikirim ke {Count} petugas.", grv.GrievanceNumber, sent);
                        }
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
    /// <remarks>
    /// Pemberitahuan dikirim ke pegawai yang kontraknya akan berakhir dan ke pemegang hak akses
    /// <c>EmployeeSeparation : Initiate</c>, masing-masing satu kali pada jendela H-60 dan satu kali pada jendela H-30.
    /// </remarks>
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
                    var notifier = scope.ServiceProvider.GetRequiredService<IHrdNotificationService>();
                    var now = DateTime.UtcNow;
                    var targetWindow = now.AddDays(60);

                    // Pemindaian kontrak PKWT yang akan berakhir
                    var expiringContracts = await db.TrxEmployeeSeparations
                        .AsNoTracking()
                        .Where(x => x.SeparationType == "ContractEnd" && x.SeparationStatus == "Initiated" && x.EffectiveSeparationDate <= targetWindow && x.EffectiveSeparationDate >= now.Date && !x.IsDelete)
                        .ToListAsync(stoppingToken);

                    foreach (var c in expiringContracts)
                    {
                        var daysLeft = (int)(c.EffectiveSeparationDate.Date - now.Date).TotalDays;
                        var window = daysLeft <= 30 ? "H30" : "H60";
                        var request = new HrdNotificationRequest(
                            Title: $"Kontrak PKWT berakhir dalam {daysLeft} hari",
                            Body: $"Kontrak kerja waktu tertentu dengan nomor terminasi {c.SeparationNumber} akan berakhir pada {c.EffectiveSeparationDate:dd MMMM yyyy}. Keputusan perpanjangan atau pengakhiran perlu ditetapkan sebelum tanggal tersebut.",
                            Category: HrdNotificationCategories.Separation,
                            Priority: daysLeft <= 30 ? HrdNotificationPriorities.High : HrdNotificationPriorities.Normal,
                            DeepLinkUrl: $"/hrd/employee-relations/separations/{c.Id}",
                            SourceModule: "Hrd.EmployeeRelation.EMP-W4",
                            ReferenceType: "TrxEmployeeSeparation",
                            ReferenceId: c.Id,
                            DedupKey: $"EMP-W4|{c.Id:N}|{window}");

                        var sentHr = await notifier.NotifyPermissionHoldersAsync("EmployeeSeparation", "Initiate", request, stoppingToken);
                        var sentEmployee = await notifier.NotifyWorkforceProfileAsync(c.WorkforceProfileId, request, stoppingToken);

                        if (sentHr + sentEmployee > 0)
                        {
                            _logger.LogInformation("EMP-W4: Radar PKWT {SeparationNumber} ({Window}) dikirim ke {Hr} petugas dan {Emp} akun pegawai.", c.SeparationNumber, window, sentHr, sentEmployee);
                        }
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
    /// <remarks>
    /// Setiap butir checklist yang masih menunggu verifikasi dikirimkan sebagai pengingat harian ke pemegang hak akses
    /// <c>MultiDeptExitClearance : SignoffChecklist</c>, dikelompokkan per proses kliring supaya satu pegawai yang keluar
    /// menghasilkan satu pengingat per hari, bukan satu per butir.
    /// </remarks>
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
                    var notifier = scope.ServiceProvider.GetRequiredService<IHrdNotificationService>();
                    var today = DateTime.UtcNow.Date;

                    var pendingChecklists = await db.TrxExitClearanceChecklists
                        .AsNoTracking()
                        .Include(x => x.ClearanceProcess)
                        .Where(x => x.Status == "Pending" && x.ClearanceProcess != null && x.ClearanceProcess.OverallClearanceStatus == "InProgress")
                        .ToListAsync(stoppingToken);

                    foreach (var group in pendingChecklists.GroupBy(x => x.ClearanceProcessId))
                    {
                        var items = group.ToList();
                        var itemSummary = string.Join("; ", items.Select(i => $"{i.ItemName} ({i.DepartmentType})"));
                        if (itemSummary.Length > 1500) itemSummary = itemSummary[..1500] + "…";

                        var sent = await notifier.NotifyPermissionHoldersAsync(
                            "MultiDeptExitClearance",
                            "SignoffChecklist",
                            new HrdNotificationRequest(
                                Title: $"{items.Count} butir exit clearance masih menunggu verifikasi",
                                Body: $"Proses kliring keluar pegawai masih memiliki {items.Count} butir yang belum ditandatangani: {itemSummary}. Rilis gaji terakhir tertahan sampai seluruh butir selesai (INT-HR-87).",
                                Category: HrdNotificationCategories.Separation,
                                Priority: HrdNotificationPriorities.Normal,
                                DeepLinkUrl: $"/hrd/employee-relations/exit-clearance/{group.Key}",
                                SourceModule: "Hrd.EmployeeRelation.EMP-W5",
                                ReferenceType: nameof(TrxExitClearanceProcess),
                                ReferenceId: group.Key,
                                DedupKey: $"EMP-W5|{group.Key:N}|{today:yyyyMMdd}"),
                            stoppingToken);

                        if (sent > 0)
                        {
                            _logger.LogInformation("EMP-W5: Pengingat kliring {ProcessId} ({Count} butir) dikirim ke {Recipients} petugas.", group.Key, items.Count, sent);
                        }
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
    /// EMP-W6: Background Worker Sinkronisasi Pemutusan Akses SIMRS &amp; IAM (BE-HRD-285 / INT-HR-88)
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Keputusan Gelombang 2:</b> sistem mengunci dan memulihkan akses aplikasi secara otomatis, tanpa menunggu
    /// admin IT. SIMRS/EMR berjalan di aplikasi ini, dan pemeriksaan hak akses per request hanya menerima akun
    /// <c>IsActive</c>; karena itu penguncian dilakukan dengan menonaktifkan akun (<c>IsActive = false</c>) sekaligus
    /// memasang <c>LockoutEnd</c> sampai tanggal akhir skorsing. Akun yang dinonaktifkan tidak lagi lolos pemeriksaan
    /// hak akses pada request berikutnya, sehingga token yang masih dipegang pegawai menjadi tidak berguna.
    /// </para>
    /// <para>
    /// Pemulihan hanya menyentuh akun yang dikunci worker ini (ada baris <c>Locked</c> tanpa <c>Restored</c> pada
    /// <c>HrdSuspensionAccessAction</c>), dan mengembalikan keadaan <c>IsActive</c> semula. Setiap tindakan dicatat
    /// dan diberitahukan ke pemegang hak akses <c>EmployeeDisciplinary : ManageSuspension</c>. Akses fisik kartu
    /// pintar berada di sistem gedung yang tidak terhubung dan tetap di luar jangkauan worker ini.
    /// </para>
    /// </remarks>
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
                    var notifier = scope.ServiceProvider.GetRequiredService<IHrdNotificationService>();

                    await LockActiveSuspensionsAsync(db, notifier, stoppingToken);
                    await RestoreFinishedSuspensionsAsync(db, notifier, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kesalahan pada eksekusi ItAccessRevocationSyncWorker (EMP-W6).");
                }

                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
        }

        private async Task LockActiveSuspensionsAsync(ApplicationDbContext db, IHrdNotificationService notifier, CancellationToken ct)
        {
            var now = DateTime.UtcNow;
            var activeSuspensions = await db.TrxDisciplinarySuspensions
                .AsNoTracking()
                .Where(x => x.Status == "Active" && x.IsSystemAccessRevoked && !x.IsDelete)
                .ToListAsync(ct);

            foreach (var suspension in activeSuspensions)
            {
                var users = await FindEmployeeUsersAsync(db, suspension.EmployeeId, ct);
                if (users.Count == 0) continue;

                var actions = await db.HrdSuspensionAccessActions
                    .AsNoTracking()
                    .Where(a => a.SuspensionId == suspension.Id && !a.IsDelete)
                    .ToListAsync(ct);

                foreach (var user in users)
                {
                    var lastLock = actions.Where(a => a.UserId == user.Id && a.ActionType == HrdSuspensionAccessAction.ActionLocked).OrderByDescending(a => a.ActionAt).FirstOrDefault();
                    var lastRestore = actions.Where(a => a.UserId == user.Id && a.ActionType == HrdSuspensionAccessAction.ActionRestored).OrderByDescending(a => a.ActionAt).FirstOrDefault();
                    var alreadyLocked = lastLock != null && (lastRestore == null || lastRestore.ActionAt <= lastLock.ActionAt);
                    if (alreadyLocked) continue;

                    var lockoutUntil = suspension.ExpectedEndDate > now ? suspension.ExpectedEndDate : now.AddDays(1);
                    var wasActive = user.IsActive;

                    user.IsActive = false;
                    user.LockoutEnabled = true;
                    user.LockoutEnd = new DateTimeOffset(DateTime.SpecifyKind(lockoutUntil, DateTimeKind.Utc));

                    db.HrdSuspensionAccessActions.Add(new HrdSuspensionAccessAction
                    {
                        Id = Guid.NewGuid(),
                        SuspensionId = suspension.Id,
                        EmployeeId = suspension.EmployeeId,
                        UserId = user.Id,
                        ActionType = HrdSuspensionAccessAction.ActionLocked,
                        WasActiveBefore = wasActive,
                        ActionAt = now,
                        LockoutUntil = lockoutUntil,
                        Notes = $"Akun dikunci otomatis selama skorsing {suspension.SuspensionNumber} (INT-HR-88).",
                        CreateDateTime = now
                    });

                    await db.SaveChangesAsync(ct);
                    _logger.LogWarning("EMP-W6: Akun {UserId} dikunci sampai {Until:u} karena skorsing {Number}.", user.Id, lockoutUntil, suspension.SuspensionNumber);

                    await notifier.NotifyPermissionHoldersAsync(
                        "EmployeeDisciplinary",
                        "ManageSuspension",
                        new HrdNotificationRequest(
                            Title: $"Akses aplikasi pegawai dikunci (skorsing {suspension.SuspensionNumber})",
                            Body: $"Akun aplikasi pegawai yang diskors telah dikunci otomatis sampai {lockoutUntil:dd MMMM yyyy} sesuai INT-HR-88. Akses fisik kartu pintar tetap perlu diblokir manual oleh petugas keamanan gedung.",
                            Category: HrdNotificationCategories.Discipline,
                            Priority: HrdNotificationPriorities.High,
                            DeepLinkUrl: $"/hrd/employee-relations/suspensions/{suspension.Id}",
                            SourceModule: "Hrd.EmployeeRelation.EMP-W6",
                            ReferenceType: nameof(TrxDisciplinarySuspension),
                            ReferenceId: suspension.Id,
                            DedupKey: $"EMP-W6-LOCK|{suspension.Id:N}|{user.Id:N}"),
                        ct);
                }
            }
        }

        private async Task RestoreFinishedSuspensionsAsync(ApplicationDbContext db, IHrdNotificationService notifier, CancellationToken ct)
        {
            var now = DateTime.UtcNow;

            // Akun yang pernah dikunci worker ini dan belum dipulihkan
            var lockedActions = await db.HrdSuspensionAccessActions
                .AsNoTracking()
                .Where(a => a.ActionType == HrdSuspensionAccessAction.ActionLocked && !a.IsDelete)
                .ToListAsync(ct);

            if (lockedActions.Count == 0) return;

            var restoredPairs = (await db.HrdSuspensionAccessActions
                    .AsNoTracking()
                    .Where(a => a.ActionType == HrdSuspensionAccessAction.ActionRestored && !a.IsDelete)
                    .Select(a => new { a.SuspensionId, a.UserId, a.ActionAt })
                    .ToListAsync(ct))
                .ToList();

            var openLocks = lockedActions
                .Where(l => !restoredPairs.Any(r => r.SuspensionId == l.SuspensionId && r.UserId == l.UserId && r.ActionAt > l.ActionAt))
                .GroupBy(l => (l.SuspensionId, l.UserId))
                .Select(g => g.OrderByDescending(x => x.ActionAt).First())
                .ToList();

            foreach (var openLock in openLocks)
            {
                var suspension = await db.TrxDisciplinarySuspensions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.SuspensionId == openLock.SuspensionId, ct);

                var suspensionFinished = suspension == null
                    || suspension.Status != "Active"
                    || !suspension.IsSystemAccessRevoked
                    || (suspension.ActualEndDate.HasValue && suspension.ActualEndDate.Value <= now);

                // Skorsing yang berujung PHK tidak memulihkan akses; akun tetap terkunci sampai proses exit clearance.
                if (!suspensionFinished || (suspension != null && suspension.Status == "EscalatedToTermination")) continue;

                var user = await db.Users.FirstOrDefaultAsync(u => u.Id == openLock.UserId, ct);
                if (user == null) continue;

                user.IsActive = openLock.WasActiveBefore;
                user.LockoutEnd = null;

                db.HrdSuspensionAccessActions.Add(new HrdSuspensionAccessAction
                {
                    Id = Guid.NewGuid(),
                    SuspensionId = openLock.SuspensionId,
                    EmployeeId = openLock.EmployeeId,
                    UserId = user.Id,
                    ActionType = HrdSuspensionAccessAction.ActionRestored,
                    WasActiveBefore = openLock.WasActiveBefore,
                    ActionAt = now,
                    Notes = openLock.WasActiveBefore
                        ? "Akun dipulihkan otomatis karena skorsing berakhir atau pegawai direhabilitasi."
                        : "Skorsing berakhir; akun dibiarkan nonaktif karena sudah nonaktif sebelum skorsing.",
                    CreateDateTime = now
                });

                await db.SaveChangesAsync(ct);
                _logger.LogInformation("EMP-W6: Akun {UserId} dipulihkan (IsActive={Active}) setelah skorsing {SuspensionId} berakhir.", user.Id, user.IsActive, openLock.SuspensionId);

                await notifier.NotifyPermissionHoldersAsync(
                    "EmployeeDisciplinary",
                    "ManageSuspension",
                    new HrdNotificationRequest(
                        Title: "Akses aplikasi pegawai dipulihkan pasca skorsing",
                        Body: $"Skorsing {suspension?.SuspensionNumber ?? openLock.SuspensionId.ToString()} telah berakhir. Akun aplikasi pegawai dipulihkan otomatis ke keadaan semula.",
                        Category: HrdNotificationCategories.Discipline,
                        Priority: HrdNotificationPriorities.Normal,
                        DeepLinkUrl: $"/hrd/employee-relations/suspensions/{openLock.SuspensionId}",
                        SourceModule: "Hrd.EmployeeRelation.EMP-W6",
                        ReferenceType: nameof(TrxDisciplinarySuspension),
                        ReferenceId: openLock.SuspensionId,
                        DedupKey: $"EMP-W6-RESTORE|{openLock.SuspensionId:N}|{user.Id:N}|{openLock.ActionAt:yyyyMMddHHmm}"),
                    ct);
            }
        }

        /// <summary>
        /// Akun pegawai bisa terhubung lewat <c>EmployeeId</c>, atau lewat profil tenaga kerja milik karyawan itu.
        /// Nilai <c>EmployeeId</c> pada surat skorsing juga diterima sebagai profil tenaga kerja, karena data lama
        /// pernah mengisinya dari kedua sumber.
        /// </summary>
        private static async Task<List<ApplicationUser>> FindEmployeeUsersAsync(ApplicationDbContext db, Guid employeeId, CancellationToken ct)
        {
            var workforceProfileId = await db.MstEmployees
                .AsNoTracking()
                .Where(e => e.Id == employeeId)
                .Select(e => (Guid?)e.WorkforceProfileId)
                .FirstOrDefaultAsync(ct);

            return await db.Users
                .Where(u => u.EmployeeId == employeeId
                            || u.WorkforceProfileId == employeeId
                            || (workforceProfileId != null && u.WorkforceProfileId == workforceProfileId))
                .ToListAsync(ct);
        }
    }
}
