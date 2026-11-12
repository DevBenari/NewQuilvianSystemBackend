using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    /// <summary>
    /// Layanan eksekusi scheduler rutin perencanaan tenaga kerja rumah sakit (BE-HRD-036, BE-HRD-043).
    /// Mengelola langkah-langkah idempoten harian dan bulanan dengan isolasi transaksi per langkah.
    /// </summary>
    public sealed class HrdWorkforcePlanningSchedulerService : IHrdWorkforcePlanningSchedulerService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<HrdWorkforcePlanningSchedulerService> _logger;

        public HrdWorkforcePlanningSchedulerService(
            ApplicationDbContext dbContext,
            IServiceProvider serviceProvider,
            ILogger<HrdWorkforcePlanningSchedulerService> logger)
        {
            _dbContext = dbContext;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task<SchedulerExecutionSummary> ExecuteDailyCycleAsync(
            DateOnly? operationalDate = null,
            CancellationToken cancellationToken = default)
        {
            var effectiveDate = operationalDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var summary = new SchedulerExecutionSummary
            {
                OperationalDate = effectiveDate,
                ExecutedAtUtc = DateTime.UtcNow
            };

            _logger.LogInformation(
                "Memulai siklus harian scheduler perencanaan tenaga kerja untuk tanggal operasional {OperationalDate}",
                effectiveDate);

            // Langkah 1: Aktivasi Standar Kebutuhan Tenaga (BE-HRD-036)
            try
            {
                await StepActivateScheduledStandardsAsync(effectiveDate, summary, cancellationToken);
            }
            catch (Exception ex)
            {
                var errorMsg = $"Langkah 1 (Aktivasi Standar Kebutuhan) gagal: {ex.Message}";
                _logger.LogError(ex, "{ErrorMessage}", errorMsg);
                summary.Errors.Add(errorMsg);
            }

            // Langkah 2: Pemrosesan Pemenuhan Kursi dan Permintaan Overdue (BE-HRD-043)
            try
            {
                var fulfilmentService = _serviceProvider.GetService<IHrdHeadcountFulfilmentService>();
                if (fulfilmentService != null)
                {
                    await fulfilmentService.StepProcessFulfilmentAndOverdueAsync(effectiveDate, summary, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                var errorMsg = $"Langkah 2 (Pemenuhan Kursi & Overdue) gagal: {ex.Message}";
                _logger.LogError(ex, "{ErrorMessage}", errorMsg);
                summary.Errors.Add(errorMsg);
            }

            // Langkah 3: Potret Bulanan (BE-HRD-048)
            try
            {
                var snapshotService = _serviceProvider.GetService<IHrdStaffingGapSnapshotService>();
                if (snapshotService != null)
                {
                    await snapshotService.StepProcessMonthlySnapshotAsync(effectiveDate, summary, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                var errorMsg = $"Langkah 3 (Potret Gap Bulanan) gagal: {ex.Message}";
                _logger.LogError(ex, "{ErrorMessage}", errorMsg);
                summary.Errors.Add(errorMsg);
            }

            _logger.LogInformation(
                "Siklus harian scheduler perencanaan tenaga kerja selesai. Standar Aktif: {ActivatedCount}, Standar Digantikan: {SupersededCount}, Error: {ErrorCount}",
                summary.ActivatedStandardsCount,
                summary.SupersededStandardsCount,
                summary.Errors.Count);

            return summary;
        }

        public async Task<int> StepActivateScheduledStandardsAsync(
            DateOnly operationalDate,
            SchedulerExecutionSummary summary,
            CancellationToken cancellationToken = default)
        {
            // Ambil semua standar Scheduled yang tanggal mulainya telah tiba
            var dueStandards = await _dbContext.HrdStaffingStandards
                .Where(s => !s.IsDelete &&
                            s.StandardStatus == StaffingStandardStatus.Scheduled &&
                            s.EffectiveStartDate <= operationalDate)
                .OrderBy(s => s.OrganizationUnitId)
                .ThenBy(s => s.EffectiveStartDate)
                .ToListAsync(cancellationToken);

            if (dueStandards.Count == 0)
            {
                var logMsg = $"Tidak ada standar kebutuhan tenaga Scheduled yang jatuh tempo pada {operationalDate}.";
                _logger.LogDebug("{Message}", logMsg);
                summary.StepLogs.Add(logMsg);
                return 0;
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                int activatedCount = 0;
                int supersededCount = 0;
                var now = DateTime.UtcNow;

                foreach (var standard in dueStandards)
                {
                    // Cari standar aktif lama pada unit organisasi yang sama
                    var activeOldStandards = await _dbContext.HrdStaffingStandards
                        .Where(s => !s.IsDelete &&
                                    s.OrganizationUnitId == standard.OrganizationUnitId &&
                                    s.StandardStatus == StaffingStandardStatus.Active &&
                                    s.Id != standard.Id)
                        .ToListAsync(cancellationToken);

                    foreach (var old in activeOldStandards)
                    {
                        old.StandardStatus = StaffingStandardStatus.Superseded;
                        old.EffectiveEndDate = standard.EffectiveStartDate.AddDays(-1);
                        old.UpdateDateTime = now;
                        old.RowVersion = Guid.NewGuid();
                        supersededCount++;

                        _logger.LogInformation(
                            "Standar lama {OldNumber} (Unit: {UnitId}) diubah menjadi Superseded dengan EffectiveEndDate={EndDate}",
                            old.StandardNumber,
                            old.OrganizationUnitId,
                            old.EffectiveEndDate);
                    }

                    // Aktifkan standar scheduled
                    standard.StandardStatus = StaffingStandardStatus.Active;
                    standard.ActivatedAt = now;
                    standard.UpdateDateTime = now;
                    standard.RowVersion = Guid.NewGuid();
                    activatedCount++;

                    _logger.LogInformation(
                        "Standar {StandardNumber} (Unit: {UnitId}) berhasil diaktifkan menjadi Active",
                        standard.StandardNumber,
                        standard.OrganizationUnitId);
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                summary.ActivatedStandardsCount += activatedCount;
                summary.SupersededStandardsCount += supersededCount;
                var summaryLog = $"Aktivasi standar selesai: {activatedCount} standar menjadi Active, {supersededCount} standar lama menjadi Superseded.";
                summary.StepLogs.Add(summaryLog);
                _logger.LogInformation("{SummaryLog}", summaryLog);

                return activatedCount;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
