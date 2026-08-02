using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Repositories;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    /// <summary>
    /// Handler lifecycle khusus reference LEAVE_ADJUSTMENT.
    /// Workflow Engine tetap menjadi pemilik keputusan approval, sedangkan service ini
    /// menyelaraskan status transaksi sumber dan menjalankan auto-post setelah workflow selesai.
    /// </summary>
    public class LeaveAdjustmentWorkflowLifecycleService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly LeaveAdjustmentPostingService _postingService;

        public LeaveAdjustmentWorkflowLifecycleService(
            ApplicationDbContext dbContext,
            LeaveAdjustmentPostingService postingService)
        {
            _dbContext = dbContext;
            _postingService = postingService;
        }

        public async Task<WorkflowReferenceLifecycleSynchronizationResult> SynchronizeAsync(
            TrxWorkflowInstance workflow,
            Guid actorUserId,
            bool allowAutoPost,
            CancellationToken cancellationToken = default)
        {
            var adjustment = await _dbContext.Set<TrxLeaveAdjustment>()
                .FirstOrDefaultAsync(
                    x => x.Id == workflow.ReferenceId && !x.IsDelete,
                    cancellationToken);

            if (adjustment == null)
            {
                throw new InvalidOperationException(
                    "Leave adjustment yang menjadi reference workflow tidak ditemukan.");
            }

            var now = DateTime.UtcNow;
            var effectiveActor = actorUserId != Guid.Empty
                ? actorUserId
                : workflow.RequestedByUserId;
            var previousStatus = adjustment.AdjustmentStatus;
            var targetStatus = MapStatus(workflow.WorkflowStatus);

            adjustment.WorkflowInstanceId = workflow.Id;
            adjustment.AdjustmentStatus = targetStatus;
            adjustment.UpdateDateTime = now;
            adjustment.UpdateBy = effectiveActor;

            if (string.Equals(
                    targetStatus,
                    LeaveValueConstants.AdjustmentStatus.UnderReview,
                    StringComparison.OrdinalIgnoreCase))
            {
                adjustment.SubmittedAt ??= workflow.SubmittedAt ?? now;
                adjustment.SubmittedByUserId ??= workflow.RequestedByUserId;
                adjustment.RejectedAt = null;
                adjustment.RejectedByUserId = null;
                adjustment.RejectionReason = null;
            }
            else if (string.Equals(
                         targetStatus,
                         LeaveValueConstants.AdjustmentStatus.NeedRevision,
                         StringComparison.OrdinalIgnoreCase))
            {
                adjustment.ApprovedAt = null;
                adjustment.ApprovedByUserId = null;
                adjustment.RejectedAt = null;
                adjustment.RejectedByUserId = null;
            }
            else if (string.Equals(
                         targetStatus,
                         LeaveValueConstants.AdjustmentStatus.Approved,
                         StringComparison.OrdinalIgnoreCase))
            {
                adjustment.ApprovedDays ??= adjustment.RequestedDays;
                adjustment.ApprovedAt ??= workflow.CompletedAt ?? now;
                adjustment.ApprovedByUserId ??= effectiveActor;
                adjustment.RejectedAt = null;
                adjustment.RejectedByUserId = null;
                adjustment.RejectionReason = null;
                adjustment.ApprovalSnapshotJson = JsonSerializer.Serialize(new
                {
                    workflowInstanceId = workflow.Id,
                    workflow.RequestNumber,
                    workflow.WorkflowStatus,
                    approvedDays = adjustment.ApprovedDays,
                    approvedAt = adjustment.ApprovedAt,
                    approvedByUserId = adjustment.ApprovedByUserId
                });
            }
            else if (string.Equals(
                         targetStatus,
                         LeaveValueConstants.AdjustmentStatus.Rejected,
                         StringComparison.OrdinalIgnoreCase))
            {
                adjustment.RejectedAt ??= now;
                adjustment.RejectedByUserId ??= effectiveActor;
                adjustment.ApprovedAt = null;
                adjustment.ApprovedByUserId = null;
            }
            else if (string.Equals(
                         targetStatus,
                         LeaveValueConstants.AdjustmentStatus.Cancelled,
                         StringComparison.OrdinalIgnoreCase))
            {
                adjustment.IsCancel = true;
                adjustment.CancelDateTime ??=
                    workflow.CancelledAt ?? workflow.WithdrawnAt ?? now;
                adjustment.CancelBy = effectiveActor;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            var result = new WorkflowReferenceLifecycleSynchronizationResult
            {
                IsHandled = true,
                WorkflowInstanceId = workflow.Id,
                ReferenceId = adjustment.Id,
                PreviousReferenceStatus = previousStatus,
                CurrentReferenceStatus = adjustment.AdjustmentStatus,
                WorkflowStatus = workflow.WorkflowStatus,
                StatusChanged = !string.Equals(
                    previousStatus,
                    adjustment.AdjustmentStatus,
                    StringComparison.OrdinalIgnoreCase)
            };

            if (!allowAutoPost ||
                !string.Equals(
                    adjustment.AdjustmentStatus,
                    LeaveValueConstants.AdjustmentStatus.Approved,
                    StringComparison.OrdinalIgnoreCase))
            {
                return result;
            }

            result.AutoApplyAttempted = true;
            var postResult = await _postingService.PostAsync(
                adjustment.Id,
                effectiveActor,
                "Diposting otomatis setelah generic workflow LEAVE_ADJUSTMENT selesai.",
                $"leave-adjustment:{adjustment.Id:N}:workflow-post",
                cancellationToken);

            result.AutoApplySucceeded = postResult.Success;
            if (!postResult.Success)
            {
                result.WarningMessage =
                    "Workflow selesai dan adjustment sudah berstatus Approved, " +
                    $"tetapi auto-post belum berhasil: {postResult.Message}";
                return result;
            }

            result.CurrentReferenceStatus = LeaveValueConstants.AdjustmentStatus.Posted;
            result.StatusChanged = !string.Equals(
                previousStatus,
                result.CurrentReferenceStatus,
                StringComparison.OrdinalIgnoreCase);
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
                return LeaveValueConstants.AdjustmentStatus.NeedRevision;
            }

            if (string.Equals(
                    workflowStatus,
                    WorkflowValueConstants.WorkflowStatus.Rejected,
                    StringComparison.OrdinalIgnoreCase))
            {
                return LeaveValueConstants.AdjustmentStatus.Rejected;
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
                return LeaveValueConstants.AdjustmentStatus.Cancelled;
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
                return LeaveValueConstants.AdjustmentStatus.Approved;
            }

            if (string.Equals(
                    workflowStatus,
                    WorkflowValueConstants.WorkflowStatus.Draft,
                    StringComparison.OrdinalIgnoreCase))
            {
                return LeaveValueConstants.AdjustmentStatus.Draft;
            }

            return LeaveValueConstants.AdjustmentStatus.UnderReview;
        }
    }
}
