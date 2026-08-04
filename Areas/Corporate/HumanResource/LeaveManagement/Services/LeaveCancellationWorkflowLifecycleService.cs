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
    public class LeaveCancellationWorkflowLifecycleService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly LeaveExecutionProcessorService _executionProcessorService;
        private readonly ILogger<LeaveCancellationWorkflowLifecycleService> _logger;

        public LeaveCancellationWorkflowLifecycleService(
            ApplicationDbContext dbContext,
            LeaveExecutionProcessorService executionProcessorService,
            ILogger<LeaveCancellationWorkflowLifecycleService> logger)
        {
            _dbContext = dbContext;
            _executionProcessorService = executionProcessorService;
            _logger = logger;
        }

        public async Task<WorkflowReferenceLifecycleSynchronizationResult> SynchronizeAsync(
            TrxWorkflowInstance workflow,
            Guid actorUserId,
            bool allowAutoApply,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxLeaveCancellationRequest>()
                .Include(x => x.LeaveRequest)
                .FirstOrDefaultAsync(x => x.Id == workflow.ReferenceId && !x.IsDelete, cancellationToken);

            if (entity?.LeaveRequest == null)
            {
                throw new InvalidOperationException("Leave cancellation yang menjadi reference workflow tidak ditemukan.");
            }

            var previousStatus = entity.CancellationStatus;
            var targetStatus = MapStatus(workflow.WorkflowStatus);
            var now = DateTime.UtcNow;
            var effectiveActor = actorUserId != Guid.Empty ? actorUserId : workflow.RequestedByUserId;

            entity.WorkflowDefinitionId = workflow.WorkflowDefinitionId;
            entity.WorkflowInstanceId = workflow.Id;
            entity.CancellationStatus = targetStatus;
            entity.UpdateDateTime = now;
            entity.UpdateBy = effectiveActor;

            if (targetStatus == LeaveLifecycleValueConstants.CancellationStatus.Approved)
            {
                entity.ApprovedAt ??= workflow.CompletedAt ?? now;
                entity.ApprovedByUserId ??= effectiveActor;
                entity.RejectedAt = null;
                entity.RejectedByUserId = null;
            }
            else if (targetStatus == LeaveLifecycleValueConstants.CancellationStatus.Rejected)
            {
                entity.RejectedAt ??= now;
                entity.RejectedByUserId ??= effectiveActor;
                entity.ApprovedAt = null;
                entity.ApprovedByUserId = null;
            }
            else if (targetStatus == LeaveLifecycleValueConstants.CancellationStatus.Cancelled)
            {
                entity.IsCancel = true;
                entity.CancelDateTime ??= workflow.CancelledAt ?? workflow.WithdrawnAt ?? now;
                entity.CancelBy = effectiveActor;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            var result = new WorkflowReferenceLifecycleSynchronizationResult
            {
                IsHandled = true,
                WorkflowInstanceId = workflow.Id,
                ReferenceId = entity.Id,
                PreviousReferenceStatus = previousStatus,
                CurrentReferenceStatus = entity.CancellationStatus,
                WorkflowStatus = workflow.WorkflowStatus,
                StatusChanged = !string.Equals(previousStatus, entity.CancellationStatus, StringComparison.OrdinalIgnoreCase)
            };

            if (!allowAutoApply || targetStatus != LeaveLifecycleValueConstants.CancellationStatus.Approved)
            {
                return result;
            }

            result.AutoApplyAttempted = true;

            try
            {
                var apply = await _executionProcessorService.ApplyApprovedCancellationAsync(
                    entity.Id,
                    new ApplyLeaveCancellationRequest
                    {
                        Notes = "Diterapkan otomatis setelah workflow LEAVE_CANCELLATION selesai."
                    },
                    effectiveActor,
                    cancellationToken);

                result.AutoApplySucceeded = apply.Success;

                if (apply.Success)
                {
                    result.CurrentReferenceStatus = LeaveLifecycleValueConstants.CancellationStatus.Applied;
                }
                else
                {
                    result.WarningMessage = apply.Message;
                }
            }
            catch (Exception ex)
            {
                result.AutoApplySucceeded = false;
                result.WarningMessage = ex.Message;
                _logger.LogError(ex, "Auto-apply leave cancellation {CancellationId} gagal.", entity.Id);
            }

            return result;
        }

        public static string MapStatus(string workflowStatus)
        {
            if (workflowStatus == WorkflowValueConstants.WorkflowStatus.RevisionRequested ||
                workflowStatus == WorkflowValueConstants.WorkflowStatus.Returned)
                return LeaveLifecycleValueConstants.CancellationStatus.NeedRevision;

            if (workflowStatus == WorkflowValueConstants.WorkflowStatus.Rejected)
                return LeaveLifecycleValueConstants.CancellationStatus.Rejected;

            if (workflowStatus == WorkflowValueConstants.WorkflowStatus.Cancelled ||
                workflowStatus == WorkflowValueConstants.WorkflowStatus.Withdrawn)
                return LeaveLifecycleValueConstants.CancellationStatus.Cancelled;

            if (workflowStatus == WorkflowValueConstants.WorkflowStatus.Completed ||
                workflowStatus == WorkflowValueConstants.WorkflowStatus.Approved)
                return LeaveLifecycleValueConstants.CancellationStatus.Approved;

            if (workflowStatus == WorkflowValueConstants.WorkflowStatus.Draft)
                return LeaveLifecycleValueConstants.CancellationStatus.Draft;

            return LeaveLifecycleValueConstants.CancellationStatus.WaitingApproval;
        }
    }
}
