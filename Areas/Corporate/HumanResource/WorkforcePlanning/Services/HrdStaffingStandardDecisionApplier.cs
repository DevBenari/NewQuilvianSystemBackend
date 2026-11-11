using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    /// <summary>
    /// Penerap keputusan akhir workflow T9 untuk dokumen Standar Kebutuhan Tenaga (BE-HRD-035).
    /// Mengaktifkan standar (Active jika tanggal berlaku hari ini / Scheduled jika tanggal berlaku di masa depan)
    /// dan menutup versi aktif sebelumnya menjadi Superseded (kriteria 5).
    /// </summary>
    public sealed class HrdStaffingStandardDecisionApplier : IWorkforcePlanningDecisionApplier
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<HrdStaffingStandardDecisionApplier> _logger;

        public HrdStaffingStandardDecisionApplier(
            ApplicationDbContext dbContext,
            ILogger<HrdStaffingStandardDecisionApplier> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public string ReferenceType => WorkforcePlanningValueConstants.WorkflowReferenceTypes.WorkforceStaffingStandard;

        public async Task<WorkforcePlanningApplyResult> ApplyDecisionAsync(
            WorkforcePlanningDecisionContext context,
            CancellationToken cancellationToken = default)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var standard = await _dbContext.HrdStaffingStandards
                .FirstOrDefaultAsync(s => s.Id == context.ReferenceId && !s.IsDelete, cancellationToken);

            if (standard == null)
            {
                _logger.LogWarning("Standar kebutuhan tenaga {ReferenceId} tidak ditemukan untuk penerapan keputusan workflow.", context.ReferenceId);
                return WorkforcePlanningApplyResult.Unhandled($"Standar kebutuhan tenaga {context.ReferenceId} tidak ditemukan.");
            }

            var previousStatus = standard.StandardStatus.ToString();

            // Idempotensi (kriteria 5)
            if (standard.StandardStatus == StaffingStandardStatus.Active ||
                standard.StandardStatus == StaffingStandardStatus.Scheduled ||
                standard.StandardStatus == StaffingStandardStatus.Superseded)
            {
                _logger.LogInformation("Standar {StandardNumber} sudah berstatus final {Status}. Penerapan dilewati (idempoten).", standard.StandardNumber, standard.StandardStatus);
                return WorkforcePlanningApplyResult.AlreadyFinal(previousStatus);
            }

            if (standard.StandardStatus == StaffingStandardStatus.Rejected ||
                standard.StandardStatus == StaffingStandardStatus.Cancelled)
            {
                _logger.LogInformation("Standar {StandardNumber} sudah berstatus {Status}. Penerapan dilewati.", standard.StandardNumber, standard.StandardStatus);
                return WorkforcePlanningApplyResult.AlreadyFinal(previousStatus);
            }

            var normalizedStatus = (context.WorkflowStatus ?? string.Empty).Trim();

            if (string.Equals(normalizedStatus, "Approved", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalizedStatus, "FinalApproved", StringComparison.OrdinalIgnoreCase))
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var now = DateTime.UtcNow;

                // Kriteria 5: tanggal berlaku hari ini / masa lalu → Active dan versi aktif sebelumnya Superseded;
                // tanggal berlaku di masa depan → Scheduled
                if (standard.EffectiveStartDate <= today)
                {
                    standard.StandardStatus = StaffingStandardStatus.Active;
                    standard.ActivatedAt = now;

                    var previousActive = await _dbContext.HrdStaffingStandards
                        .FirstOrDefaultAsync(s => s.OrganizationUnitId == standard.OrganizationUnitId &&
                                                  s.StandardStatus == StaffingStandardStatus.Active &&
                                                  s.Id != standard.Id &&
                                                  !s.IsDelete, cancellationToken);

                    if (previousActive != null)
                    {
                        previousActive.StandardStatus = StaffingStandardStatus.Superseded;
                        previousActive.EffectiveEndDate = standard.EffectiveStartDate.AddDays(-1);
                        previousActive.UpdateDateTime = now;
                        previousActive.UpdateBy = context.ActorUserId;

                        standard.SupersedesStandardId = previousActive.Id;

                        _logger.LogInformation(
                            "Standar versi aktif sebelumnya {PrevNumber} digantikan (Superseded) dengan tanggal akhir {EndDate}",
                            previousActive.StandardNumber,
                            previousActive.EffectiveEndDate);
                    }
                }
                else
                {
                    standard.StandardStatus = StaffingStandardStatus.Scheduled;
                    _logger.LogInformation(
                        "Standar {StandardNumber} dijadwalkan (Scheduled) untuk berlaku pada {StartDate}",
                        standard.StandardNumber,
                        standard.EffectiveStartDate);
                }

                standard.ApprovedByUserId = context.ActorUserId;
                standard.ApprovedAt = now;
                standard.RowVersion = Guid.NewGuid();
                standard.UpdateDateTime = now;
                standard.UpdateBy = context.ActorUserId;

                await _dbContext.SaveChangesAsync(cancellationToken);

                return WorkforcePlanningApplyResult.Success(
                    previousStatus: previousStatus,
                    currentStatus: standard.StandardStatus.ToString(),
                    statusChanged: true,
                    autoApplyAttempted: true,
                    autoApplySucceeded: true);
            }
            else if (string.Equals(normalizedStatus, "Rejected", StringComparison.OrdinalIgnoreCase))
            {
                standard.StandardStatus = StaffingStandardStatus.Rejected;
                standard.RowVersion = Guid.NewGuid();
                standard.UpdateDateTime = DateTime.UtcNow;
                standard.UpdateBy = context.ActorUserId;

                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Standar {StandardNumber} ditolak (Rejected) melalui workflow.", standard.StandardNumber);

                return WorkforcePlanningApplyResult.Success(
                    previousStatus: previousStatus,
                    currentStatus: standard.StandardStatus.ToString(),
                    statusChanged: true);
            }
            else if (string.Equals(normalizedStatus, "Returned", StringComparison.OrdinalIgnoreCase))
            {
                standard.StandardStatus = StaffingStandardStatus.Returned;
                standard.RowVersion = Guid.NewGuid();
                standard.UpdateDateTime = DateTime.UtcNow;
                standard.UpdateBy = context.ActorUserId;

                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Standar {StandardNumber} dikembalikan (Returned) melalui workflow.", standard.StandardNumber);

                return WorkforcePlanningApplyResult.Success(
                    previousStatus: previousStatus,
                    currentStatus: standard.StandardStatus.ToString(),
                    statusChanged: true);
            }

            _logger.LogWarning("Status workflow {WorkflowStatus} tidak dikenali untuk penerapan standar {StandardNumber}.", normalizedStatus, standard.StandardNumber);
            return WorkforcePlanningApplyResult.Unhandled($"Status workflow {context.WorkflowStatus} tidak mengubah status standar kebutuhan tenaga.");
        }
    }
}
