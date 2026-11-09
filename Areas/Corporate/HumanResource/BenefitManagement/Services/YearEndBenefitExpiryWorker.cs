using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services
{
    /// <summary>
    /// Background Worker Pembersihan Saldo Plafon Tahunan Per 1 Januari (Slice S-D3, BE-HRD-190).
    /// Mengimplementasikan aturan BEN-B6, VR-BEN-22, dan FR-BEN-24:
    /// Sisa saldo plafon subsidi berobat internal RS tahun lalu otomatis hangus (forfeited)
    /// dan di-reset menjadi pagu baru per 1 Januari pukul 00:00 WIB (no carry-forward).
    /// </summary>
    public class YearEndBenefitExpiryWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<YearEndBenefitExpiryWorker> _logger;

        public YearEndBenefitExpiryWorker(
            IServiceProvider serviceProvider,
            ILogger<YearEndBenefitExpiryWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("YearEndBenefitExpiryWorker dimulai.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.UtcNow;

                    // Jalankan audit reset hanya jika tanggal hari ini adalah 1 Januari (atau trigger awal)
                    if (now.Month == 1 && now.Day == 1)
                    {
                        await RunYearEndExpiryResetAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Terjadi kesalahan pada eksekusi YearEndBenefitExpiryWorker.");
                }

                // Periksa setiap 12 jam
                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }

        public async Task RunYearEndExpiryResetAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var currentYear = DateTime.UtcNow.Year;

            // Cari seluruh enrollment aktif yang memiliki sisa saldo berjalan dari tahun lalu
            var activeEnrollments = await dbContext.TrxEmployeeBenefitEnrollments
                .Include(x => x.BenefitPlan)
                .Where(x => !x.IsDelete && x.EnrollmentStatus == "Active" && x.RemainingAmount > 0)
                .ToListAsync(cancellationToken);

            var systemUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

            foreach (var enrollment in activeEnrollments)
            {
                var sisaSaldo = enrollment.RemainingAmount;

                // 1. Catat mutasi YearEndExpiryReset (VR-BEN-22)
                var expiryMovement = new HrdBenefitBalanceMovement
                {
                    Id = Guid.NewGuid(),
                    EmployeeBenefitEnrollmentId = enrollment.Id,
                    WorkforceProfileId = enrollment.WorkforceProfileId,
                    BenefitPersonKind = "EMPLOYEE",
                    MovementType = "YearEndExpiryReset",
                    Amount = -sisaSaldo,
                    BalanceBefore = sisaSaldo,
                    BalanceAfter = 0.00m,
                    ReferenceNumber = $"RESET-EOY-{currentYear}",
                    Description = $"Pembersihan sisa saldo plafon tahun lalu sebesar Rp {sisaSaldo:N0} (Hangus per 1 Januari, BEN-B6, VR-BEN-22).",
                    OccurredAt = DateTimeOffset.UtcNow,
                    ActorUserId = systemUserId,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = systemUserId
                };

                dbContext.HrdBenefitBalanceMovements.Add(expiryMovement);

                // 2. Alokasi pagu tahun baru
                var newLimit = enrollment.CoverageLimitAmount > 0 ? enrollment.CoverageLimitAmount : 15000000m;
                enrollment.RemainingAmount = newLimit;
                enrollment.UsedAmount = 0m;
                enrollment.UpdateDateTime = DateTime.UtcNow;
                enrollment.UpdateBy = systemUserId;

                var allocationMovement = new HrdBenefitBalanceMovement
                {
                    Id = Guid.NewGuid(),
                    EmployeeBenefitEnrollmentId = enrollment.Id,
                    WorkforceProfileId = enrollment.WorkforceProfileId,
                    BenefitPersonKind = "EMPLOYEE",
                    MovementType = "AnnualAllocation",
                    Amount = newLimit,
                    BalanceBefore = 0.00m,
                    BalanceAfter = newLimit,
                    ReferenceNumber = $"ALLOC-{currentYear}",
                    Description = $"Alokasi pagu subsidi plafon baru tahun {currentYear} sebesar Rp {newLimit:N0}.",
                    OccurredAt = DateTimeOffset.UtcNow,
                    ActorUserId = systemUserId,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = systemUserId
                };

                dbContext.HrdBenefitBalanceMovements.Add(allocationMovement);
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "[BEN-B6] Reset hangus tahunan 1 Januari selesai untuk {Count} enrollment aktif.",
                activeEnrollments.Count);
        }
    }
}
