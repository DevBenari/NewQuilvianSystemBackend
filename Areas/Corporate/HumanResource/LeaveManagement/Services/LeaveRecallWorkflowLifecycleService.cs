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
    public class LeaveRecallWorkflowLifecycleService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly LeaveExecutionProcessorService _executionProcessorService;
        private readonly ILogger<LeaveRecallWorkflowLifecycleService> _logger;

        public LeaveRecallWorkflowLifecycleService(
            ApplicationDbContext dbContext,
            LeaveExecutionProcessorService executionProcessorService,
            ILogger<LeaveRecallWorkflowLifecycleService> logger)
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
            var entity = await _dbContext.Set<TrxLeaveRecall>()
                .Include(x => x.LeaveRequest)
                .FirstOrDefaultAsync(x => x.Id == workflow.ReferenceId && !x.IsDelete, cancellationToken);

            if (entity?.LeaveRequest == null)
            {
                throw new InvalidOperationException("Leave recall yang menjadi reference workflow tidak ditemukan.");
            }

            var previousStatus = entity.RecallStatus;
            var targetStatus = MapStatus(workflow.WorkflowStatus);
            var now = DateTime.UtcNow;
            var effectiveActor = actorUserId != Guid.Empty ? actorUserId : workflow.RequestedByUserId;

            entity.WorkflowDefinitionId = workflow.WorkflowDefinitionId;
            entity.WorkflowInstanceId = workflow.Id;
            entity.RecallStatus = targetStatus;
            entity.UpdateDateTime = now;
            entity.UpdateBy = effectiveActor;

            if (targetStatus == LeaveLifecycleValueConstants.RecallStatus.Approved)
            {
                entity.ApprovedAt ??= workflow.CompletedAt ?? now;
                entity.ApprovedByUserId ??= effectiveActor;
            }
            else if (targetStatus == LeaveLifecycleValueConstants.RecallStatus.Cancelled)
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
                CurrentReferenceStatus = entity.RecallStatus,
                WorkflowStatus = workflow.WorkflowStatus,
                StatusChanged = !string.Equals(previousStatus, entity.RecallStatus, StringComparison.OrdinalIgnoreCase)
            };

            if (!allowAutoApply || targetStatus != LeaveLifecycleValueConstants.RecallStatus.Approved)
            {
                return result;
            }

            result.AutoApplyAttempted = true;
            var effectiveDate = entity.ActualReturnToWorkDate ?? entity.RecallEffectiveDate;

            try
            {
                var reverse = await _executionProcessorService.ReverseAsync(
                    entity.LeaveRequestId,
                    new ReverseLeaveExecutionRequest
                    {
                        Reason = entity.RecallReason,
                        EffectiveDate = effectiveDate,
                        RestoreDays = entity.RestoredBalanceDays > 0 ? entity.RestoredBalanceDays : null
                    },
                    effectiveActor,
                    cancellationToken);

                result.AutoApplySucceeded = reverse.Success;

                if (reverse.Success)
                {
                    entity = await _dbContext.Set<TrxLeaveRecall>()
                        .Include(x => x.LeaveRequest)
                        .FirstAsync(x => x.Id == entity.Id, cancellationToken);

                    entity.RecallStatus = LeaveLifecycleValueConstants.RecallStatus.Applied;
                    entity.AppliedAt ??= DateTime.UtcNow;
                    entity.ActualReturnToWorkDate ??= effectiveDate;
                    entity.RestoredBalanceDays = entity.RestoredBalanceDays > 0
                        ? entity.RestoredBalanceDays
                        : entity.RecalledLeaveDays;
                    entity.UpdateDateTime = DateTime.UtcNow;
                    entity.UpdateBy = effectiveActor;

                    if (entity.LeaveRequest != null)
                    {
                        entity.LeaveRequest.LeaveRequestStatus = LeaveRequestValueConstants.Status.Recalled;
                        entity.LeaveRequest.RecalledAt ??= DateTime.UtcNow;
                        entity.LeaveRequest.ApprovalNotes = entity.RecallReason;
                        entity.LeaveRequest.UpdateDateTime = DateTime.UtcNow;
                        entity.LeaveRequest.UpdateBy = effectiveActor;
                    }

                    await _dbContext.SaveChangesAsync(cancellationToken);
                    result.CurrentReferenceStatus = entity.RecallStatus;
                }
                else
                {
                    result.WarningMessage = reverse.Message;
                }
            }
            catch (Exception ex)
            {
                result.AutoApplySucceeded = false;
                result.WarningMessage = ex.Message;
                _logger.LogError(ex, "Auto-apply leave recall {RecallId} gagal.", entity.Id);
            }

            return result;
        }

        public static string MapStatus(string workflowStatus)
        {
            if (workflowStatus == WorkflowValueConstants.WorkflowStatus.RevisionRequested ||
                workflowStatus == WorkflowValueConstants.WorkflowStatus.Returned)
                return LeaveLifecycleValueConstants.RecallStatus.NeedRevision;

            if (workflowStatus == WorkflowValueConstants.WorkflowStatus.Rejected)
                return LeaveLifecycleValueConstants.RecallStatus.Rejected;

            if (workflowStatus == WorkflowValueConstants.WorkflowStatus.Cancelled ||
                workflowStatus == WorkflowValueConstants.WorkflowStatus.Withdrawn)
                return LeaveLifecycleValueConstants.RecallStatus.Cancelled;

            if (workflowStatus == WorkflowValueConstants.WorkflowStatus.Completed ||
                workflowStatus == WorkflowValueConstants.WorkflowStatus.Approved)
                return LeaveLifecycleValueConstants.RecallStatus.Approved;

            if (workflowStatus == WorkflowValueConstants.WorkflowStatus.Draft)
                return LeaveLifecycleValueConstants.RecallStatus.Draft;

            return LeaveLifecycleValueConstants.RecallStatus.WaitingApproval;
        }
    }
}
