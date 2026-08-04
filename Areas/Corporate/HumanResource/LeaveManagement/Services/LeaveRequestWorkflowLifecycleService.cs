using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    /// <summary>
    /// Handler lifecycle khusus workflow reference LEAVE_REQUEST.
    /// Workflow Engine menjadi pemilik keputusan approval. Handler ini menyelaraskan
    /// status WfpLeaveRequest serta menerapkan reservation/deduction ke leave ledger.
    /// </summary>
    public class LeaveRequestWorkflowLifecycleService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly LeaveRequestBalanceLifecycleService _balanceLifecycleService;
        private readonly ILogger<LeaveRequestWorkflowLifecycleService> _logger;

        public LeaveRequestWorkflowLifecycleService(
            ApplicationDbContext dbContext,
            LeaveRequestBalanceLifecycleService balanceLifecycleService,
            ILogger<LeaveRequestWorkflowLifecycleService> logger)
        {
            _dbContext = dbContext;
            _balanceLifecycleService = balanceLifecycleService;
            _logger = logger;
        }

        public async Task<WorkflowReferenceLifecycleSynchronizationResult> SynchronizeAsync(
            TrxWorkflowInstance workflow,
            Guid actorUserId,
            bool allowBalanceApply,
            CancellationToken cancellationToken = default)
        {
            var request = await _dbContext.Set<WfpLeaveRequest>()
                .FirstOrDefaultAsync(
                    x => x.Id == workflow.ReferenceId && !x.IsDelete,
                    cancellationToken);

            if (request == null)
            {
                throw new InvalidOperationException(
                    "Leave request yang menjadi reference workflow tidak ditemukan.");
            }

            var now = DateTime.UtcNow;
            var effectiveActor = actorUserId != Guid.Empty
                ? actorUserId
                : workflow.RequestedByUserId;

            var previousStatus = request.LeaveRequestStatus;
            var targetStatus = MapStatus(workflow.WorkflowStatus);

            var latestAction = await _dbContext.Set<TrxApprovalAction>()
                .AsNoTracking()
                .Where(x =>
                    x.WorkflowInstanceId == workflow.Id &&
                    x.IsActive &&
                    !x.IsDelete)
                .OrderByDescending(x => x.ActionAt)
                .ThenByDescending(x => x.CreateDateTime)
                .FirstOrDefaultAsync(cancellationToken);

            request.WorkflowDefinitionId = workflow.WorkflowDefinitionId;
            request.WorkflowInstanceId = workflow.Id;
            request.CurrentApprovalStep = workflow.CurrentStepOrder;
            request.LeaveRequestStatus = targetStatus;
            request.UpdateDateTime = now;
            request.UpdateBy = effectiveActor;

            ApplyAudit(
                request,
                workflow,
                latestAction,
                targetStatus,
                effectiveActor,
                now);

            await _dbContext.SaveChangesAsync(cancellationToken);

            var result = new WorkflowReferenceLifecycleSynchronizationResult
            {
                IsHandled = true,
                WorkflowInstanceId = workflow.Id,
                ReferenceId = request.Id,
                PreviousReferenceStatus = previousStatus,
                CurrentReferenceStatus = request.LeaveRequestStatus,
                WorkflowStatus = workflow.WorkflowStatus,
                StatusChanged = !string.Equals(
                    previousStatus,
                    request.LeaveRequestStatus,
                    StringComparison.OrdinalIgnoreCase)
            };

            if (!allowBalanceApply)
            {
                return result;
            }

            result.AutoApplyAttempted = true;

            LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse> balanceResult;

            try
            {
                balanceResult = await _balanceLifecycleService.ApplyWorkflowStatusAsync(
                    request.Id,
                    request.LeaveRequestStatus,
                    effectiveActor,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                result.AutoApplySucceeded = false;
                result.WarningMessage =
                    $"Status workflow sudah tersinkron, tetapi lifecycle balance gagal: {ex.Message}";

                _logger.LogError(
                    ex,
                    "Lifecycle balance leave request {LeaveRequestId} gagal setelah workflow {WorkflowInstanceId} berubah.",
                    request.Id,
                    workflow.Id);

                return result;
            }

            result.AutoApplySucceeded = balanceResult.Success;

            if (!balanceResult.Success)
            {
                result.WarningMessage =
                    "Status workflow sudah tersinkron, tetapi lifecycle balance belum berhasil: " +
                    balanceResult.Message;
            }

            return result;
        }

        public static string MapStatus(string workflowStatus)
        {
            if (string.Equals(
                    workflowStatus,
                    WorkflowValueConstants.WorkflowStatus.RevisionRequested,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    workflowStatus,
                    WorkflowValueConstants.WorkflowStatus.Returned,
                    StringComparison.OrdinalIgnoreCase))
            {
                return LeaveRequestValueConstants.Status.NeedRevision;
            }

            if (string.Equals(
                    workflowStatus,
                    WorkflowValueConstants.WorkflowStatus.Rejected,
                    StringComparison.OrdinalIgnoreCase))
            {
                return LeaveRequestValueConstants.Status.Rejected;
            }

            if (string.Equals(
                    workflowStatus,
                    WorkflowValueConstants.WorkflowStatus.Cancelled,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    workflowStatus,
                    WorkflowValueConstants.WorkflowStatus.Withdrawn,
                    StringComparison.OrdinalIgnoreCase))
            {
                return LeaveRequestValueConstants.Status.Cancelled;
            }

            if (string.Equals(
                    workflowStatus,
                    WorkflowValueConstants.WorkflowStatus.Completed,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    workflowStatus,
                    WorkflowValueConstants.WorkflowStatus.Approved,
                    StringComparison.OrdinalIgnoreCase))
            {
                return LeaveRequestValueConstants.Status.Approved;
            }

            if (string.Equals(
                    workflowStatus,
                    WorkflowValueConstants.WorkflowStatus.Draft,
                    StringComparison.OrdinalIgnoreCase))
            {
                return LeaveRequestValueConstants.Status.Draft;
            }

            return LeaveRequestValueConstants.Status.WaitingApproval;
        }

        private static void ApplyAudit(
            WfpLeaveRequest request,
            TrxWorkflowInstance workflow,
            TrxApprovalAction? latestAction,
            string targetStatus,
            Guid actorUserId,
            DateTime now)
        {
            var note = latestAction?.Comment;

            if (string.Equals(
                    targetStatus,
                    LeaveRequestValueConstants.Status.WaitingApproval,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    targetStatus,
                    LeaveRequestValueConstants.Status.Submitted,
                    StringComparison.OrdinalIgnoreCase))
            {
                request.SubmittedAt ??= workflow.SubmittedAt ?? now;
                request.SubmittedByUserId ??= workflow.RequestedByUserId;
                request.ApprovedAt = null;
                request.ApprovedByUserId = null;
                request.RejectedAt = null;
                request.RejectedByUserId = null;
                request.RejectionReasonId = null;
                request.IsCancel = false;
                request.CancelDateTime = null;
                request.CancelBy = Guid.Empty;
                return;
            }

            if (string.Equals(
                    targetStatus,
                    LeaveRequestValueConstants.Status.NeedRevision,
                    StringComparison.OrdinalIgnoreCase))
            {
                request.ApprovedAt = null;
                request.ApprovedByUserId = null;
                request.RejectedAt = null;
                request.RejectedByUserId = null;
                request.ApprovalNotes = note;
                return;
            }

            if (string.Equals(
                    targetStatus,
                    LeaveRequestValueConstants.Status.Approved,
                    StringComparison.OrdinalIgnoreCase))
            {
                request.ApprovedAt ??= workflow.CompletedAt ?? now;
                request.ApprovedByUserId ??= actorUserId;
                request.RejectedAt = null;
                request.RejectedByUserId = null;
                request.RejectionReasonId = null;
                request.ApprovalNotes = note ?? workflow.CompletionNote;
                request.IsCancel = false;
                return;
            }

            if (string.Equals(
                    targetStatus,
                    LeaveRequestValueConstants.Status.Rejected,
                    StringComparison.OrdinalIgnoreCase))
            {
                request.RejectedAt ??= latestAction?.ActionAt ?? now;
                var latestActorUserId = latestAction?.ActualActionByUserId ?? Guid.Empty;
                request.RejectedByUserId ??=
                    latestActorUserId != Guid.Empty
                        ? latestActorUserId
                        : actorUserId;

                request.ApprovedAt = null;
                request.ApprovedByUserId = null;
                request.ApprovalNotes = note;
                return;
            }

            if (string.Equals(
                    targetStatus,
                    LeaveRequestValueConstants.Status.Cancelled,
                    StringComparison.OrdinalIgnoreCase))
            {
                request.CancelledAt ??=
                    workflow.CancelledAt ?? workflow.WithdrawnAt ?? now;

                request.CancelledByUserId ??= actorUserId;
                request.ApprovalNotes = workflow.CancellationReason ?? note;
                request.IsCancel = true;
                request.CancelDateTime ??= request.CancelledAt;
                request.CancelBy = actorUserId;
            }
        }
    }
}
