using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Enums.HumanResource;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Workers
{
    /// <summary>
    /// Background Worker Pemantau Siklus 3 Tahun SPK-RKK & Auto-Suspend Penugasan Klinis Kedaluwarsa (BE-HRD-220, CRD-B3)
    /// Berjalan secara periodik untuk memindai masa berlaku SPK-RKK yang telah melampaui siklus 3 tahun,
    /// memperbarui status SPK menjadi 'Expired', dan secara otomatis menangguhkan hak layanan klinis di SIMRS EMR.
    /// </summary>
    public class SpkRkkExpiryMonitoringWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SpkRkkExpiryMonitoringWorker> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(12); // Dipindai setiap 12 jam
        private const string TickLockKey = "HRD_SPK_RKK_EXPIRY_TICK";
        private const string RecredSequenceKey = "TRX_RECREDENTIALING_APPLICATION";
        private const string RecredSeriesPrefix = "RECRED";
        private static readonly string[] FinalOppeStatuses = { "FINALIZED", "ACKNOWLEDGED" };

        public SpkRkkExpiryMonitoringWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<SpkRkkExpiryMonitoringWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SpkRkkExpiryMonitoringWorker: Pemantau Siklus 3 Tahun SPK-RKK telah aktif.");

            // Delay awal agar aplikasi selesai bootstrapping
            await Task.Delay(TimeSpan.FromSeconds(25), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PerformSpkExpiryCheckAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("SpkRkkExpiryMonitoringWorker dihentikan karena aplikasi shutting down.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Terjadi kesalahan saat menjalankan pemantau kedaluwarsa SPK-RKK 3 tahun.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task PerformSpkExpiryCheckAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var allocator = scope.ServiceProvider.GetRequiredService<NumberSeriesAllocator>();

            // BE-HRD-437: hanya satu instance yang menjalankan tick (kunci sesi PostgreSQL), sehingga deployment
            // lebih dari satu instance tidak membuat draf rekredensial ganda.
            if (!context.Database.IsNpgsql())
            {
                await PerformSpkExpiryCheckCoreAsync(context, allocator, stoppingToken);
                return;
            }

            await context.Database.OpenConnectionAsync(stoppingToken);
            try
            {
                var acquired = await context.Database
                    .SqlQueryRaw<bool>("SELECT pg_try_advisory_lock(hashtext({0})) AS \"Value\"", TickLockKey)
                    .SingleAsync(stoppingToken);
                if (!acquired)
                {
                    _logger.LogInformation("SpkRkkExpiryMonitoringWorker: tick dilewati karena instance lain sedang berjalan.");
                    return;
                }

                try
                {
                    await PerformSpkExpiryCheckCoreAsync(context, allocator, stoppingToken);
                }
                finally
                {
                    await context.Database.ExecuteSqlRawAsync("SELECT pg_advisory_unlock(hashtext({0}));", [TickLockKey], CancellationToken.None);
                }
            }
            finally
            {
                await context.Database.CloseConnectionAsync();
            }
        }

        private async Task PerformSpkExpiryCheckCoreAsync(ApplicationDbContext context, NumberSeriesAllocator allocator, CancellationToken stoppingToken)
        {
            var now = DateTime.UtcNow;

            // Cari SPK aktif yang tanggal berlakunya telah habis (siklus 3 tahun terlampaui)
            var expiredSpkList = await context.TrxSpkRkkIssuances
                .Where(s => s.IsActive && !s.IsRevoked && s.IssuanceStatus == "Active" && s.ValidUntil < now)
                .ToListAsync(stoppingToken);

            if (!expiredSpkList.Any())
            {
                // BE-HRD-437: draf H-60 tetap diperiksa walau tidak ada SPK yang baru kedaluwarsa.
                await PerformH60RecredentialingCheckAsync(context, allocator, now, stoppingToken);
                return;
            }

            var affectedProfileIds = expiredSpkList.Select(s => s.WorkforceProfileId).Distinct().ToList();

            foreach (var spk in expiredSpkList)
            {
                spk.IssuanceStatus = "Expired";
                spk.UpdateDateTime = now;
                spk.UpdateBy = Guid.Empty;

                _logger.LogWarning(
                    "SPK-RKK #{SpkNumber} milik nakes {ProfileId} telah kedaluwarsa (Masa berlaku 3 tahun habis pada {ValidUntil}).",
                    spk.SpkNumber, spk.WorkforceProfileId, spk.ValidUntil);
            }

            // FIN-HRD-09-BE-012 / M09-P0-11: Jangan menangguhkan wewenang nakes yang sudah memiliki SPK baru yang aktif dan valid
            var profilesWithNewerActiveSpk = await context.TrxSpkRkkIssuances
                .Where(s => affectedProfileIds.Contains(s.WorkforceProfileId)
                    && s.IsActive 
                    && !s.IsRevoked 
                    && !s.IsSuspended
                    && s.IssuanceStatus == "Active" 
                    && s.ValidUntil >= now)
                .Select(s => s.WorkforceProfileId)
                .Distinct()
                .ToListAsync(stoppingToken);

            var genuinelyExpiredProfileIds = affectedProfileIds
                .Except(profilesWithNewerActiveSpk)
                .ToList();

            // Tangguhkan kewenangan klinis aktif nakes di WfpClinicalPrivilege HANYA untuk nakes yang tidak punya SPK aktif baru
            var activePrivileges = await context.WfpClinicalPrivileges
                .Where(cp => genuinelyExpiredProfileIds.Contains(cp.WorkforceProfileId) && cp.PrivilegeStatus == ClinicalPrivilegeStatus.Active && cp.EffectiveEndDate <= now)
                .ToListAsync(stoppingToken);

            foreach (var cp in activePrivileges)
            {
                cp.PrivilegeStatus = ClinicalPrivilegeStatus.Expired;
                cp.IsClinicalServiceBlocked = true;
                cp.IsSchedulingBlocked = true;
                cp.ApprovalNotes = $"{cp.ApprovalNotes} | Ditangguhkan otomatis: Masa berlaku SPK-RKK 3 tahun telah habis.".Trim();
                cp.UpdateDateTime = now;
                cp.UpdateBy = Guid.Empty;
            }

            await context.SaveChangesAsync(stoppingToken);

            _logger.LogInformation(
                "Siklus Pemantau SPK Selesai: {SpkCount} SPK kedaluwarsa diproses. {PrivCount} kewenangan klinis aktif ditangguhkan (Auto-Suspend).",
                expiredSpkList.Count, activePrivileges.Count);

            // FIN-HRD-09-BE-012: Inisiasi draf rekredensialing H-60 secara idempoten
            await PerformH60RecredentialingCheckAsync(context, allocator, now, stoppingToken);
        }

        private async Task PerformH60RecredentialingCheckAsync(ApplicationDbContext context, NumberSeriesAllocator allocator, DateTime now, CancellationToken stoppingToken)
        {
            var h60Horizon = now.AddDays(60);
            var approachingSpkList = await context.TrxSpkRkkIssuances
                .Where(s => s.IsActive && !s.IsRevoked && s.IssuanceStatus == "Active" && s.ValidUntil >= now && s.ValidUntil <= h60Horizon)
                .ToListAsync(stoppingToken);

            foreach (var spk in approachingSpkList)
            {
                // BE-HRD-437: satu draf per siklus. Siklus dikenali dari tanggal jatuh tempo (= ValidUntil SPK); draf yang
                // sudah selesai/ditutup untuk siklus yang sama tidak dibuat ulang. Draf/Submitted yang masih terbuka
                // juga menahan pembuatan draf baru.
                var hasDraftForCycle = await context.TrxRecredentialingApplications
                    .AnyAsync(r => r.WorkforceProfileId == spk.WorkforceProfileId && r.IsActive && !r.IsDelete
                                   && (r.DueDate == spk.ValidUntil
                                       || r.RecredentialingStatus == "Draft" || r.RecredentialingStatus == "Submitted"), stoppingToken);
                if (hasDraftForCycle)
                {
                    continue;
                }

                var previousCycle = await context.TrxRecredentialingApplications
                    .Where(r => r.WorkforceProfileId == spk.WorkforceProfileId && !r.IsDelete)
                    .Select(r => (int?)r.CycleNumber)
                    .MaxAsync(stoppingToken) ?? 0;

                // BE-HRD-437: kesiapan OPPE jujur — hanya evaluasi final yang periodenya berakhir di dalam siklus SPK ini.
                var oppeInCycle = await context.HrdOppeStaffEvaluations
                    .AsNoTracking()
                    .Where(e => e.WorkforceProfileId == spk.WorkforceProfileId && !e.IsDelete
                                && FinalOppeStatuses.Contains(e.Status)
                                && e.Period != null && e.Period.EndDate >= spk.ValidFrom && e.Period.EndDate <= spk.ValidUntil)
                    .Select(e => new { e.FinalCompositeScore, e.PerformanceGrade, e.Period!.PeriodName, e.Period.EndDate })
                    .OrderByDescending(e => e.EndDate)
                    .ToListAsync(stoppingToken);
                var latestOppe = oppeInCycle.FirstOrDefault();

                var recredApp = new TrxRecredentialingApplication
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = spk.WorkforceProfileId,
                    PreviousCredentialingApplicationId = spk.CredentialingApplicationId,
                    RecredentialingNumber = await AllocateRecredentialingNumberAsync(context, allocator, stoppingToken),
                    RecredentialingStatus = "Draft",
                    CycleNumber = previousCycle + 1,
                    DueDate = spk.ValidUntil,
                    ComplianceSnapshotJson = JsonSerializer.Serialize(new
                    {
                        SourceSpkRkkIssuanceId = spk.Id,
                        SourceSpkNumber = spk.SpkNumber,
                        CycleValidFrom = spk.ValidFrom,
                        CycleValidUntil = spk.ValidUntil,
                        OppeReadiness = oppeInCycle.Count > 0 ? OppeReadinessStatus.Ready : OppeReadinessStatus.NotEnoughData,
                        OppeEvaluationsInCycle = oppeInCycle.Count,
                        LatestOppeScore = latestOppe?.FinalCompositeScore,
                        LatestOppeGrade = latestOppe?.PerformanceGrade,
                        LatestOppePeriodName = latestOppe?.PeriodName,
                        EvaluatedAtUtc = now
                    }),
                    Notes = $"Draf otomatis H-60 peringatan rekredensialing siklus 3 tahun dari SPK #{spk.SpkNumber}.",
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = Guid.Empty
                };

                context.TrxRecredentialingApplications.Add(recredApp);
                // Simpan per draf agar nomor yang sudah dialokasikan langsung terikat ke barisnya.
                await context.SaveChangesAsync(stoppingToken);
                _logger.LogInformation("Draf rekredensialing H-60 #{Number} berhasil dibuat otomatis untuk nakes {ProfileId}.", recredApp.RecredentialingNumber, spk.WorkforceProfileId);
            }
        }

        /// <summary>
        /// BE-HRD-437: nomor RECRED/{tahun}/{NNNN} dari NumberSeriesAllocator (reset tahunan), bukan Count+1 yang
        /// menghasilkan nomor kembar bila dua SPK diproses dalam satu tick.
        /// </summary>
        private static async Task<string> AllocateRecredentialingNumberAsync(ApplicationDbContext context, NumberSeriesAllocator allocator, CancellationToken stoppingToken)
        {
            var year = DateTime.UtcNow.Year;
            var legacyPrefix = $"{RecredSeriesPrefix}/{year}/";
            var existing = await context.TrxRecredentialingApplications
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(r => r.RecredentialingNumber.StartsWith(legacyPrefix))
                .Select(r => r.RecredentialingNumber)
                .ToListAsync(stoppingToken);
            var watermark = existing
                .Select(n => long.TryParse(n[legacyPrefix.Length..], NumberStyles.None, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0L)
                .DefaultIfEmpty(0L)
                .Max();

            var allocated = await allocator.AllocateAsync(
                new NumberAllocationRequest(
                    SequenceKey: RecredSequenceKey,
                    Prefix: RecredSeriesPrefix,
                    ResetPolicy: NumberSeriesResetPolicies.Yearly,
                    SequenceDigits: 4,
                    ActorUserId: Guid.Empty,
                    Instant: DateTimeOffset.UtcNow,
                    MinimumValue: watermark),
                stoppingToken);

            // Keluaran allocator: RECRED-{yyyy}-{NNNN}; dikembalikan ke format lama RECRED/{yyyy}/{NNNN}.
            var parts = allocated.Split('-');
            return $"{RecredSeriesPrefix}/{parts[^2]}/{parts[^1]}";
        }
    }
}
