using System;
using System.Linq;
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
    /// Penerap keputusan akhir workflow T10 untuk dokumen Batch Formasi Awal (BE-HRD-033).
    /// Menerapkan formasi versi 1 untuk seluruh baris secara atomik di dalam satu transaksi (NFR-011).
    /// Memenuhi kriteria 6 (idempoten dan rollback jika salah satu baris gagal) serta kriteria 7 (Returned/Rejected).
    /// </summary>
    public sealed class HrdInitialFormationBatchDecisionApplier : IWorkforcePlanningDecisionApplier
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHrdPositionFormationService _formationService;
        private readonly ILogger<HrdInitialFormationBatchDecisionApplier> _logger;

        public HrdInitialFormationBatchDecisionApplier(
            ApplicationDbContext dbContext,
            IHrdPositionFormationService formationService,
            ILogger<HrdInitialFormationBatchDecisionApplier> logger)
        {
            _dbContext = dbContext;
            _formationService = formationService;
            _logger = logger;
        }

        public string ReferenceType => WorkforcePlanningValueConstants.WorkflowReferenceTypes.WorkforceInitialFormationBatch;

        public async Task<WorkforcePlanningApplyResult> ApplyDecisionAsync(
            WorkforcePlanningDecisionContext context,
            CancellationToken cancellationToken = default)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var batch = await _dbContext.HrdInitialFormationBatches
                .Include(b => b.Rows)
                .FirstOrDefaultAsync(b => b.Id == context.ReferenceId && !b.IsDelete, cancellationToken);

            if (batch == null)
            {
                _logger.LogWarning("Batch formasi awal {ReferenceId} tidak ditemukan untuk penerapan keputusan workflow.", context.ReferenceId);
                return WorkforcePlanningApplyResult.Unhandled($"Batch formasi awal {context.ReferenceId} tidak ditemukan.");
            }

            var previousStatus = batch.BatchStatus.ToString();

            // Idempotensi (kriteria 6): Keputusan terkirim ulang tidak membuat formasi versi ganda
            if (batch.BatchStatus == InitialFormationBatchStatus.Applied)
            {
                _logger.LogInformation("Batch formasi awal {BatchNumber} sudah berstatus Applied. Penerapan dilewati (idempoten).", batch.BatchNumber);
                return WorkforcePlanningApplyResult.AlreadyFinal(previousStatus);
            }

            if (batch.BatchStatus == InitialFormationBatchStatus.Rejected || batch.BatchStatus == InitialFormationBatchStatus.Cancelled)
            {
                _logger.LogInformation("Batch formasi awal {BatchNumber} sudah berstatus {Status}. Penerapan dilewati.", batch.BatchNumber, batch.BatchStatus);
                return WorkforcePlanningApplyResult.AlreadyFinal(previousStatus);
            }

            var normalizedStatus = (context.WorkflowStatus ?? string.Empty).Trim();

            if (string.Equals(normalizedStatus, "Draft", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalizedStatus, "Submitted", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalizedStatus, "InProgress", StringComparison.OrdinalIgnoreCase))
            {
                return WorkforcePlanningApplyResult.AlreadyFinal(previousStatus);
            }

            if (string.Equals(normalizedStatus, "Completed", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalizedStatus, "Approved", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalizedStatus, "FinalApproved", StringComparison.OrdinalIgnoreCase))
            {
                // Eksekusi atomik satu transaksi (NFR-011 & kriteria 6), hindari nested transaction jika pemanggil sudah membuka transaksi
                var ownTransaction = _dbContext.Database.CurrentTransaction == null;
                await using var transaction = ownTransaction
                    ? await _dbContext.Database.BeginTransactionAsync(cancellationToken)
                    : null;

                try
                {
                    var approvedAt = DateTime.UtcNow;
                    var effectiveActorId = context.ActorUserId != Guid.Empty
                        ? context.ActorUserId
                        : batch.SubmittedByUserId ?? Guid.Empty;

                    foreach (var row in batch.Rows.Where(r => !r.IsDelete))
                    {
                        if (row.CreatedFormationId.HasValue)
                        {
                            continue;
                        }

                        // Idempotensi tambahan: cek apakah formasi untuk batch & baris ini sudah terbentuk sebelumnya
                        var existingFormation = await _dbContext.HrdPositionFormations
                            .FirstOrDefaultAsync(f =>
                                !f.IsDelete &&
                                f.SourceType == PositionFormationSourceType.InitialBatch &&
                                f.SourceDocumentId == batch.Id &&
                                f.OrganizationUnitId == row.OrganizationUnitId &&
                                f.PositionId == row.PositionId &&
                                f.EffectiveStartDate == row.EffectiveStartDate,
                                cancellationToken);

                        var formation = existingFormation ?? await _formationService.CreateNewVersionAsync(
                            hospitalSiteId: batch.HospitalSiteId,
                            organizationUnitId: row.OrganizationUnitId,
                            positionId: row.PositionId,
                            formationQuantity: row.FormationQuantity,
                            effectiveStartDate: row.EffectiveStartDate,
                            sourceType: PositionFormationSourceType.InitialBatch,
                            sourceDocumentId: batch.Id,
                            sourceDocumentNumber: batch.BatchNumber,
                            approvedByUserId: effectiveActorId,
                            approvedAt: approvedAt,
                            cancellationToken: cancellationToken);

                        row.CreatedFormationId = formation.Id;
                        row.UpdateDateTime = approvedAt;
                        row.UpdateBy = effectiveActorId;
                    }

                    batch.BatchStatus = InitialFormationBatchStatus.Applied;
                    batch.ApprovedByUserId = effectiveActorId != Guid.Empty ? effectiveActorId : batch.ApprovedByUserId;
                    batch.ApprovedAt ??= approvedAt;
                    batch.AppliedAt ??= approvedAt;
                    batch.RowVersion = Guid.NewGuid();
                    batch.UpdateDateTime = approvedAt;
                    batch.UpdateBy = effectiveActorId;

                    await _dbContext.SaveChangesAsync(cancellationToken);
                    if (transaction != null)
                    {
                        await transaction.CommitAsync(cancellationToken);
                    }

                    _logger.LogInformation(
                        "Batch formasi awal {BatchNumber} berhasil diterapkan (Applied) dengan {RowCount} formasi versi baru.",
                        batch.BatchNumber,
                        batch.Rows.Count(r => !r.IsDelete));

                    return WorkforcePlanningApplyResult.Success(
                        previousStatus: previousStatus,
                        currentStatus: batch.BatchStatus.ToString(),
                        statusChanged: true,
                        autoApplyAttempted: true,
                        autoApplySucceeded: true);
                }
                catch (Exception ex)
                {
                    if (transaction != null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                    }
                    _logger.LogError(ex, "Gagal menerapkan keputusan persetujuan pada batch formasi awal {BatchNumber}. Transaksi dibatalkan (NFR-011).", batch.BatchNumber);
                    throw;
                }
            }
            else if (string.Equals(normalizedStatus, "Rejected", StringComparison.OrdinalIgnoreCase))
            {
                batch.BatchStatus = InitialFormationBatchStatus.Rejected;
                batch.RowVersion = Guid.NewGuid();
                batch.UpdateDateTime = DateTime.UtcNow;
                batch.UpdateBy = context.ActorUserId;

                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Batch formasi awal {BatchNumber} ditolak (Rejected) melalui workflow.", batch.BatchNumber);

                return WorkforcePlanningApplyResult.Success(
                    previousStatus: previousStatus,
                    currentStatus: batch.BatchStatus.ToString(),
                    statusChanged: true);
            }
            else if (string.Equals(normalizedStatus, "Returned", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(normalizedStatus, "ReturnedForRevision", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(normalizedStatus, "RevisionRequested", StringComparison.OrdinalIgnoreCase))
            {
                batch.BatchStatus = InitialFormationBatchStatus.Returned;
                batch.RowVersion = Guid.NewGuid();
                batch.UpdateDateTime = DateTime.UtcNow;
                batch.UpdateBy = context.ActorUserId;

                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Batch formasi awal {BatchNumber} dikembalikan (Returned) melalui workflow ({Status}).", batch.BatchNumber, normalizedStatus);

                return WorkforcePlanningApplyResult.Success(
                    previousStatus: previousStatus,
                    currentStatus: batch.BatchStatus.ToString(),
                    statusChanged: true);
            }

            _logger.LogWarning("Status workflow {WorkflowStatus} tidak dikenali untuk penerapan batch formasi awal {BatchNumber}.", normalizedStatus, batch.BatchNumber);
            return WorkforcePlanningApplyResult.Unhandled($"Status workflow {context.WorkflowStatus} tidak mengubah status batch formasi awal.");
        }
    }
}
