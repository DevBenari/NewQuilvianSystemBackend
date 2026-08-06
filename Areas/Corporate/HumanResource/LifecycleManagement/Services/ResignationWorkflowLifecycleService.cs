using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public class ResignationWorkflowLifecycleService
    {
        private readonly ApplicationDbContext _dbContext;

        public ResignationWorkflowLifecycleService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<WorkflowReferenceLifecycleSynchronizationResult> SynchronizeAsync(
            TrxWorkflowInstance workflow,
            Guid actorUserId,
            bool allowAutoApply,
            CancellationToken cancellationToken = default)
        {
            var source = await _dbContext.TrxResignationRequests
                .FirstOrDefaultAsync(x => x.Id == workflow.ReferenceId && !x.IsDelete, cancellationToken)
                ?? throw new InvalidOperationException("Pengajuan resign tidak ditemukan.");

            var now = DateTime.UtcNow;
            var effectiveActor = actorUserId != Guid.Empty ? actorUserId : workflow.RequestedByUserId;
            var previousStatus = source.RequestStatus;
            var targetStatus = MapStatus(workflow.WorkflowStatus);

            if (workflow.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.InProgress ||
                workflow.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.Submitted)
            {
                targetStatus = ResignationValueConstants.Status.UnderReview;
            }

            source.WorkflowDefinitionId = workflow.WorkflowDefinitionId;
            source.WorkflowInstanceId = workflow.Id;
            source.RequestStatus = targetStatus;
            source.UpdateDateTime = now;
            source.UpdateBy = effectiveActor;

            if (targetStatus == ResignationValueConstants.Status.Submitted ||
                targetStatus == ResignationValueConstants.Status.UnderReview)
            {
                source.SubmittedAt ??= workflow.SubmittedAt ?? now;
                source.ApprovedAt = null;
                source.ApprovedByUserId = null;
                source.RejectedAt = null;
                source.RejectedByUserId = null;
            }
            else if (targetStatus == ResignationValueConstants.Status.Approved)
            {
                source.ApprovedAt ??= workflow.CompletedAt ?? now;
                source.ApprovedByUserId ??= effectiveActor;
                source.RejectedAt = null;
                source.RejectedByUserId = null;
            }
            else if (targetStatus == ResignationValueConstants.Status.Rejected)
            {
                source.RejectedAt ??= now;
                source.RejectedByUserId ??= effectiveActor;
                source.ApprovedAt = null;
                source.ApprovedByUserId = null;
            }
            else if (targetStatus == ResignationValueConstants.Status.Cancelled)
            {
                source.WithdrawnAt ??= workflow.WithdrawnAt ?? workflow.CancelledAt ?? now;
                source.WithdrawalReason ??= workflow.CancellationReason;
                source.IsCancel = true;
                source.CancelDateTime ??= source.WithdrawnAt;
                source.CancelBy = effectiveActor;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new WorkflowReferenceLifecycleSynchronizationResult
            {
                IsHandled = true,
                WorkflowInstanceId = workflow.Id,
                ReferenceId = source.Id,
                PreviousReferenceStatus = previousStatus,
                CurrentReferenceStatus = source.RequestStatus,
                WorkflowStatus = workflow.WorkflowStatus,
                StatusChanged = !string.Equals(previousStatus, source.RequestStatus, StringComparison.OrdinalIgnoreCase),
                AutoApplyAttempted = false,
                AutoApplySucceeded = false,
                WarningMessage = workflow.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.Completed
                    ? "Workflow resign selesai. HR wajib menjalankan lifecycle handoff; employee belum dinonaktifkan otomatis."
                    : null
            };
        }

        public static string MapStatus(string workflowStatus)
        {
            if (workflowStatus == WorkflowValueConstants.WorkflowStatus.RevisionRequested ||
                workflowStatus == WorkflowValueConstants.WorkflowStatus.Returned)
            {
                return ResignationValueConstants.Status.NeedRevision;
            }

            if (workflowStatus == WorkflowValueConstants.WorkflowStatus.Rejected)
            {
                return ResignationValueConstants.Status.Rejected;
            }

            if (workflowStatus == WorkflowValueConstants.WorkflowStatus.Cancelled ||
                workflowStatus == WorkflowValueConstants.WorkflowStatus.Withdrawn)
            {
                return ResignationValueConstants.Status.Cancelled;
            }

            if (workflowStatus == WorkflowValueConstants.WorkflowStatus.Completed ||
                workflowStatus == WorkflowValueConstants.WorkflowStatus.Approved)
            {
                return ResignationValueConstants.Status.Approved;
            }

            if (workflowStatus == WorkflowValueConstants.WorkflowStatus.Draft)
            {
                return ResignationValueConstants.Status.Draft;
            }

            return ResignationValueConstants.Status.Submitted;
        }
    }
}
