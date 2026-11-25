using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Repositories;
using Outcome = QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Constants.EmploymentLifecycleWorkflowCodes.Outcome;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    /// <summary>
    /// Penangan siklus rujukan workflow untuk dokumen T15–T18 (<c>INT-HR-30</c>; planning
    /// <c>PLANNING_PERSETUJUAN_T15_T18_KONTRAK_DAN_MASA_KERJA.md</c>). Pola sama dengan
    /// <c>BusinessTravelWorkflowLifecycleHandler</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Dipanggil <c>WorkflowReferenceLifecycleService</c> <b>sesudah</b> aksi workflow tersimpan (ajukan, setujui,
    /// minta revisi, kembalikan, tolak, tarik) dan oleh penyelarasan terjadwal. Galat di sini dicatat mesin tanpa
    /// membatalkan aksi workflow.
    /// </para>
    /// <para>
    /// <b>Idempoten.</b> Status dokumen diturunkan dari status instance saat ini. Dokumen yang sudah final tidak
    /// diubah lagi; efek persetujuan (kontrak lanjutan, koreksi, batch, pemberhentian) dijalankan service domain yang
    /// memiliki penjaga sudah-diterapkan dan pembuat ≠ penyetuju. Instance lama yang bukan instance dokumen saat ini
    /// diabaikan.
    /// </para>
    /// </remarks>
    public sealed class HrdEmploymentLifecycleWorkflowLifecycleHandler : IWorkflowReferenceLifecycleHandler
    {
        private static readonly string[] FinalDecisionActions =
        {
            WorkflowValueConstants.ActionType.Approve,
            WorkflowValueConstants.ActionType.AutoApprove,
            WorkflowValueConstants.ActionType.Reject,
            WorkflowValueConstants.ActionType.AutoReject
        };

        private static readonly string[] RevisionActions =
        {
            WorkflowValueConstants.ActionType.RequestRevision,
            WorkflowValueConstants.ActionType.Return
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<HrdEmploymentLifecycleWorkflowLifecycleHandler> _logger;

        public HrdEmploymentLifecycleWorkflowLifecycleHandler(
            ApplicationDbContext dbContext,
            IServiceProvider serviceProvider,
            ILogger<HrdEmploymentLifecycleWorkflowLifecycleHandler> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool CanHandle(string referenceType) =>
            EmploymentLifecycleWorkflowCodes.IsLifecycleReference(referenceType);

        public async Task<WorkflowReferenceLifecycleSynchronizationResult> SynchronizeAsync(
            TrxWorkflowInstance workflow,
            Guid actorUserId,
            bool allowAutoApply,
            CancellationToken cancellationToken)
        {
            var outcome = EmploymentLifecycleWorkflowCodes.MapOutcome(workflow.WorkflowStatus);
            var referenceType = workflow.ReferenceType?.Trim();

            if (string.Equals(referenceType, EmploymentLifecycleWorkflowCodes.ContractDecisionReferenceType, StringComparison.OrdinalIgnoreCase))
            {
                return await SynchronizeDecisionAsync(workflow, outcome, actorUserId, allowAutoApply, cancellationToken);
            }

            if (string.Equals(referenceType, EmploymentLifecycleWorkflowCodes.ProbationAssessmentReferenceType, StringComparison.OrdinalIgnoreCase))
            {
                return await SynchronizeAssessmentAsync(workflow, outcome, actorUserId, allowAutoApply, cancellationToken);
            }

            if (string.Equals(referenceType, EmploymentLifecycleWorkflowCodes.ContractCorrectionReferenceType, StringComparison.OrdinalIgnoreCase))
            {
                return await SynchronizeCorrectionAsync(workflow, outcome, actorUserId, allowAutoApply, cancellationToken);
            }

            return await SynchronizeBatchAsync(workflow, outcome, actorUserId, allowAutoApply, cancellationToken);
        }

        private async Task<WorkflowReferenceLifecycleSynchronizationResult> SynchronizeDecisionAsync(
            TrxWorkflowInstance workflow,
            Outcome outcome,
            Guid actorUserId,
            bool allowAutoApply,
            CancellationToken cancellationToken)
        {
            var decision = await _dbContext.HrdContractDecisions
                .FirstOrDefaultAsync(x => x.Id == workflow.ReferenceId && !x.IsDelete, cancellationToken);
            if (decision == null)
            {
                return Result(workflow, string.Empty, string.Empty, false, isHandled: false, "Keputusan kontrak tidak ditemukan.");
            }

            var previous = decision.DecisionStatus.ToString();
            if (!IsCurrentInstance(decision.WorkflowInstanceId, workflow.Id))
            {
                return Result(workflow, previous, previous, false, isHandled: true, "Instance ini bukan instance keputusan kontrak saat ini.");
            }

            decision.WorkflowInstanceId ??= workflow.Id;
            var now = DateTime.UtcNow;

            switch (outcome)
            {
                case Outcome.InReview when decision.DecisionStatus == ContractDecisionStatus.Returned:
                    decision.DecisionStatus = ContractDecisionStatus.Submitted;
                    decision.SubmittedAt = workflow.SubmittedAt ?? now;
                    Touch(decision, workflow.RequestedByUserId, now);
                    break;

                case Outcome.Returned when decision.DecisionStatus == ContractDecisionStatus.Submitted:
                    decision.DecisionStatus = ContractDecisionStatus.Returned;
                    decision.DecisionNote = await ResolveLatestCommentAsync(workflow.Id, RevisionActions, cancellationToken) ?? decision.DecisionNote;
                    Touch(decision, actorUserId, now);
                    break;

                case Outcome.Withdrawn when decision.DecisionStatus is ContractDecisionStatus.Submitted or ContractDecisionStatus.Returned:
                    decision.DecisionStatus = ContractDecisionStatus.Withdrawn;
                    Touch(decision, actorUserId, now);
                    break;

                case Outcome.Approved or Outcome.Rejected when decision.DecisionStatus is ContractDecisionStatus.Submitted or ContractDecisionStatus.Approved:
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    return await ApplyFinalAsync(workflow, outcome, actorUserId, allowAutoApply, previous,
                        async () => (await _dbContext.HrdContractDecisions.AsNoTracking()
                            .Where(x => x.Id == decision.Id).Select(x => x.DecisionStatus).FirstAsync(cancellationToken)).ToString(),
                        cancellationToken);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result(workflow, previous, decision.DecisionStatus.ToString(), false);
        }

        private async Task<WorkflowReferenceLifecycleSynchronizationResult> SynchronizeAssessmentAsync(
            TrxWorkflowInstance workflow,
            Outcome outcome,
            Guid actorUserId,
            bool allowAutoApply,
            CancellationToken cancellationToken)
        {
            var assessment = await _dbContext.HrdProbationAssessments
                .Include(a => a.Probation)
                .FirstOrDefaultAsync(x => x.Id == workflow.ReferenceId && !x.IsDelete, cancellationToken);
            if (assessment == null)
            {
                return Result(workflow, string.Empty, string.Empty, false, isHandled: false, "Penilaian masa percobaan tidak ditemukan.");
            }

            var previous = assessment.AssessmentStatus.ToString();
            if (!IsCurrentInstance(assessment.WorkflowInstanceId, workflow.Id))
            {
                return Result(workflow, previous, previous, false, isHandled: true, "Instance ini bukan instance penilaian saat ini.");
            }

            assessment.WorkflowInstanceId ??= workflow.Id;
            var now = DateTime.UtcNow;
            var probation = assessment.Probation;

            switch (outcome)
            {
                case Outcome.InReview when assessment.AssessmentStatus == ProbationAssessmentStatus.Returned:
                    assessment.AssessmentStatus = ProbationAssessmentStatus.Submitted;
                    assessment.SubmittedAt = workflow.SubmittedAt ?? now;
                    TouchAssessment(assessment, workflow.RequestedByUserId, now);
                    break;

                case Outcome.Returned when assessment.AssessmentStatus == ProbationAssessmentStatus.Submitted:
                    assessment.AssessmentStatus = ProbationAssessmentStatus.Returned;
                    assessment.DecisionNote = await ResolveLatestCommentAsync(workflow.Id, RevisionActions, cancellationToken) ?? assessment.DecisionNote;
                    TouchAssessment(assessment, actorUserId, now);
                    break;

                case Outcome.Withdrawn when assessment.AssessmentStatus is ProbationAssessmentStatus.Submitted or ProbationAssessmentStatus.Returned:
                    assessment.AssessmentStatus = ProbationAssessmentStatus.Withdrawn;
                    TouchAssessment(assessment, actorUserId, now);
                    if (probation != null && probation.ProbationStatus == ProbationStatus.AwaitingDecision)
                    {
                        probation.ProbationStatus = ProbationStatus.Running;
                        probation.RowVersion = Guid.NewGuid();
                        probation.UpdateDateTime = now;
                        probation.UpdateBy = actorUserId;
                    }
                    break;

                case Outcome.Approved or Outcome.Rejected when assessment.AssessmentStatus == ProbationAssessmentStatus.Submitted:
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    return await ApplyFinalAsync(workflow, outcome, actorUserId, allowAutoApply, previous,
                        async () => (await _dbContext.HrdProbationAssessments.AsNoTracking()
                            .Where(x => x.Id == assessment.Id).Select(x => x.AssessmentStatus).FirstAsync(cancellationToken)).ToString(),
                        cancellationToken);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result(workflow, previous, assessment.AssessmentStatus.ToString(), false);
        }

        private async Task<WorkflowReferenceLifecycleSynchronizationResult> SynchronizeCorrectionAsync(
            TrxWorkflowInstance workflow,
            Outcome outcome,
            Guid actorUserId,
            bool allowAutoApply,
            CancellationToken cancellationToken)
        {
            var correction = await _dbContext.HrdContractCorrections
                .FirstOrDefaultAsync(x => x.Id == workflow.ReferenceId && !x.IsDelete, cancellationToken);
            if (correction == null)
            {
                return Result(workflow, string.Empty, string.Empty, false, isHandled: false, "Pengajuan koreksi kontrak tidak ditemukan.");
            }

            var previous = correction.CorrectionStatus.ToString();
            if (!IsCurrentInstance(correction.WorkflowInstanceId, workflow.Id))
            {
                return Result(workflow, previous, previous, false, isHandled: true, "Instance ini bukan instance koreksi saat ini.");
            }

            correction.WorkflowInstanceId ??= workflow.Id;
            var now = DateTime.UtcNow;

            switch (outcome)
            {
                case Outcome.InReview when correction.CorrectionStatus == ContractCorrectionStatus.Returned:
                    correction.CorrectionStatus = ContractCorrectionStatus.Submitted;
                    TouchCorrection(correction, workflow.RequestedByUserId, now);
                    break;

                case Outcome.Returned when correction.CorrectionStatus == ContractCorrectionStatus.Submitted:
                    correction.CorrectionStatus = ContractCorrectionStatus.Returned;
                    correction.DecisionNote = await ResolveLatestCommentAsync(workflow.Id, RevisionActions, cancellationToken) ?? correction.DecisionNote;
                    TouchCorrection(correction, actorUserId, now);
                    break;

                case Outcome.Withdrawn when correction.CorrectionStatus is ContractCorrectionStatus.Submitted or ContractCorrectionStatus.Returned:
                    correction.CorrectionStatus = ContractCorrectionStatus.Withdrawn;
                    TouchCorrection(correction, actorUserId, now);
                    break;

                case Outcome.Approved or Outcome.Rejected when correction.CorrectionStatus == ContractCorrectionStatus.Submitted:
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    return await ApplyFinalAsync(workflow, outcome, actorUserId, allowAutoApply, previous,
                        async () => (await _dbContext.HrdContractCorrections.AsNoTracking()
                            .Where(x => x.Id == correction.Id).Select(x => x.CorrectionStatus).FirstAsync(cancellationToken)).ToString(),
                        cancellationToken);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result(workflow, previous, correction.CorrectionStatus.ToString(), false);
        }

        private async Task<WorkflowReferenceLifecycleSynchronizationResult> SynchronizeBatchAsync(
            TrxWorkflowInstance workflow,
            Outcome outcome,
            Guid actorUserId,
            bool allowAutoApply,
            CancellationToken cancellationToken)
        {
            var batch = await _dbContext.HrdInitialContractBatches
                .FirstOrDefaultAsync(x => x.Id == workflow.ReferenceId && !x.IsDelete, cancellationToken);
            if (batch == null)
            {
                return Result(workflow, string.Empty, string.Empty, false, isHandled: false, "Batch data awal kontrak tidak ditemukan.");
            }

            var previous = batch.BatchStatus.ToString();
            if (!IsCurrentInstance(batch.WorkflowInstanceId, workflow.Id))
            {
                return Result(workflow, previous, previous, false, isHandled: true, "Instance ini bukan instance batch saat ini.");
            }

            batch.WorkflowInstanceId ??= workflow.Id;
            var now = DateTime.UtcNow;

            switch (outcome)
            {
                case Outcome.InReview when batch.BatchStatus == InitialContractBatchStatus.Returned:
                    batch.BatchStatus = InitialContractBatchStatus.Submitted;
                    batch.SubmittedAt = workflow.SubmittedAt ?? now;
                    TouchBatch(batch, workflow.RequestedByUserId, now);
                    break;

                case Outcome.Returned when batch.BatchStatus == InitialContractBatchStatus.Submitted:
                    batch.BatchStatus = InitialContractBatchStatus.Returned;
                    TouchBatch(batch, actorUserId, now);
                    break;

                case Outcome.Withdrawn when batch.BatchStatus is InitialContractBatchStatus.Submitted or InitialContractBatchStatus.Returned:
                    batch.BatchStatus = InitialContractBatchStatus.Cancelled;
                    TouchBatch(batch, actorUserId, now);
                    break;

                case Outcome.Approved or Outcome.Rejected when batch.BatchStatus == InitialContractBatchStatus.Submitted:
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    return await ApplyFinalAsync(workflow, outcome, actorUserId, allowAutoApply, previous,
                        async () => (await _dbContext.HrdInitialContractBatches.AsNoTracking()
                            .Where(x => x.Id == batch.Id).Select(x => x.BatchStatus).FirstAsync(cancellationToken)).ToString(),
                        cancellationToken);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result(workflow, previous, batch.BatchStatus.ToString(), false);
        }

        private async Task<WorkflowReferenceLifecycleSynchronizationResult> ApplyFinalAsync(
            TrxWorkflowInstance workflow,
            Outcome outcome,
            Guid actorUserId,
            bool allowAutoApply,
            string previous,
            Func<Task<string>> readCurrentStatus,
            CancellationToken cancellationToken)
        {
            if (!allowAutoApply)
            {
                return Result(workflow, previous, previous, false, isHandled: true,
                    "Workflow sudah final; penerapan otomatis dimatikan pada panggilan ini.");
            }

            var (deciderUserId, note) = await ResolveFinalActionAsync(workflow, actorUserId, cancellationToken);
            var integration = _serviceProvider.GetRequiredService<IHrdEmploymentLifecycleWorkflowIntegrationService>();
            var applied = await integration.HandleWorkflowCompletedAsync(
                workflow.Id,
                outcome == Outcome.Approved,
                deciderUserId,
                note,
                cancellationToken);

            var current = await readCurrentStatus();
            return new WorkflowReferenceLifecycleSynchronizationResult
            {
                IsHandled = true,
                WorkflowInstanceId = workflow.Id,
                ReferenceId = workflow.ReferenceId,
                PreviousReferenceStatus = previous,
                CurrentReferenceStatus = current,
                WorkflowStatus = workflow.WorkflowStatus,
                StatusChanged = !string.Equals(previous, current, StringComparison.OrdinalIgnoreCase),
                AutoApplyAttempted = true,
                AutoApplySucceeded = applied,
                WarningMessage = applied
                    ? null
                    : "Keputusan workflow tidak dapat diterapkan ke dokumen; lihat log server untuk alasannya."
            };
        }

        /// <summary>
        /// Penyetuju dan catatan diambil dari aksi Setujui/Tolak terakhir pada instance, sehingga penyelarasan terjadwal
        /// (tanpa pelaku) tetap mencatat penyetuju yang sebenarnya.
        /// </summary>
        private async Task<(Guid DeciderUserId, string? Note)> ResolveFinalActionAsync(
            TrxWorkflowInstance workflow,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var action = await _dbContext.TrxApprovalActions
                .AsNoTracking()
                .Where(x => x.WorkflowInstanceId == workflow.Id && !x.IsDelete && FinalDecisionActions.Contains(x.ActionType))
                .OrderByDescending(x => x.ActionAt)
                .Select(x => new { x.ActualActionByUserId, x.Comment, x.ReasonComment })
                .FirstOrDefaultAsync(cancellationToken);

            var decider = action?.ActualActionByUserId ?? (actorUserId != Guid.Empty ? actorUserId : Guid.Empty);
            if (decider == Guid.Empty)
            {
                _logger.LogWarning("Penyetuju instance {WorkflowInstanceId} tidak ditemukan pada riwayat aksi.", workflow.Id);
            }

            var note = string.IsNullOrWhiteSpace(action?.Comment) ? action?.ReasonComment : action.Comment;
            return (decider, note);
        }

        private async Task<string?> ResolveLatestCommentAsync(
            Guid workflowInstanceId,
            string[] actionTypes,
            CancellationToken cancellationToken)
        {
            var action = await _dbContext.TrxApprovalActions
                .AsNoTracking()
                .Where(x => x.WorkflowInstanceId == workflowInstanceId && !x.IsDelete && actionTypes.Contains(x.ActionType))
                .OrderByDescending(x => x.ActionAt)
                .Select(x => new { x.Comment, x.ReasonComment })
                .FirstOrDefaultAsync(cancellationToken);

            return string.IsNullOrWhiteSpace(action?.Comment) ? action?.ReasonComment : action.Comment;
        }

        private static bool IsCurrentInstance(Guid? documentInstanceId, Guid workflowInstanceId) =>
            !documentInstanceId.HasValue || documentInstanceId.Value == workflowInstanceId;

                private static void Touch(HrdContractDecision decision, Guid actorUserId, DateTime now)
        {
            decision.UpdateDateTime = now;
            decision.UpdateBy = actorUserId;
        }

        private static void TouchAssessment(HrdProbationAssessment assessment, Guid actorUserId, DateTime now)
        {
            assessment.RowVersion = Guid.NewGuid();
            assessment.UpdateDateTime = now;
            assessment.UpdateBy = actorUserId;
        }

        private static void TouchCorrection(HrdContractCorrection correction, Guid actorUserId, DateTime now)
        {
            correction.UpdateDateTime = now;
            correction.UpdateBy = actorUserId;
        }

        private static void TouchBatch(HrdInitialContractBatch batch, Guid actorUserId, DateTime now)
        {
            batch.UpdateDateTime = now;
            batch.UpdateBy = actorUserId;
        }

        private static WorkflowReferenceLifecycleSynchronizationResult Result(
            TrxWorkflowInstance workflow,
            string previous,
            string current,
            bool autoApplyAttempted,
            bool isHandled = true,
            string? warning = null) => new()
            {
                IsHandled = isHandled,
                WorkflowInstanceId = workflow.Id,
                ReferenceId = workflow.ReferenceId,
                PreviousReferenceStatus = previous,
                CurrentReferenceStatus = current,
                WorkflowStatus = workflow.WorkflowStatus,
                StatusChanged = !string.Equals(previous, current, StringComparison.OrdinalIgnoreCase),
                AutoApplyAttempted = autoApplyAttempted,
                WarningMessage = warning
            };
    }
}
