using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services
{
    public class ShiftSwapWorkflowLifecycleService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ShiftSwapService _shiftSwapService;
        private readonly ILogger<ShiftSwapWorkflowLifecycleService> _logger;

        public ShiftSwapWorkflowLifecycleService(
            ApplicationDbContext dbContext,
            ShiftSwapService shiftSwapService,
            ILogger<ShiftSwapWorkflowLifecycleService> logger)
        {
            _dbContext = dbContext;
            _shiftSwapService = shiftSwapService;
            _logger = logger;
        }

        public async Task<WorkflowReferenceLifecycleSynchronizationResult> SynchronizeAsync(
            TrxWorkflowInstance workflow,
            Guid actorUserId,
            bool allowAutoApply,
            CancellationToken cancellationToken = default)
        {
            var source = await _dbContext.WfpShiftSwapRequests
                .FirstOrDefaultAsync(x => x.Id == workflow.ReferenceId && !x.IsDelete, cancellationToken)
                ?? throw new InvalidOperationException("Pengajuan tukar shift tidak ditemukan.");

            var now = DateTime.UtcNow;
            var effectiveActor = actorUserId != Guid.Empty ? actorUserId : workflow.RequestedByUserId;
            var previousStatus = source.RequestStatus;
            var targetStatus = MapStatus(workflow.WorkflowStatus);

            if (workflow.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.InProgress ||
                workflow.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.Submitted)
            {
                targetStatus = SchedulingRequestValueConstants.ShiftSwapStatus.PendingApproval;
            }

            source.WorkflowDefinitionId = workflow.WorkflowDefinitionId;
            source.WorkflowInstanceId = workflow.Id;
            source.RequestStatus = targetStatus;
            source.UpdateDateTime = now;
            source.UpdateBy = effectiveActor;

            ApplyAudit(source, workflow, targetStatus, effectiveActor, now);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var result = new WorkflowReferenceLifecycleSynchronizationResult
            {
                IsHandled = true,
                WorkflowInstanceId = workflow.Id,
                ReferenceId = source.Id,
                PreviousReferenceStatus = previousStatus,
                CurrentReferenceStatus = source.RequestStatus,
                WorkflowStatus = workflow.WorkflowStatus,
                StatusChanged = !string.Equals(previousStatus, source.RequestStatus, StringComparison.OrdinalIgnoreCase)
            };

            if (!allowAutoApply ||
                workflow.WorkflowStatus != WorkflowValueConstants.WorkflowStatus.Completed ||
                source.RequestStatus == SchedulingRequestValueConstants.ShiftSwapStatus.Applied)
            {
                return result;
            }

            result.AutoApplyAttempted = true;
            _dbContext.Entry(source).State = EntityState.Detached;

            try
            {
                var apply = await _shiftSwapService.ApplyAsync(
                    workflow.ReferenceId,
                    effectiveActor,
                    "Applied automatically after shift swap workflow completed.",
                    cancellationToken);

                result.AutoApplySucceeded = apply.Success;
                if (apply.Success && apply.Data != null)
                {
                    result.CurrentReferenceStatus = apply.Data.RequestStatus;
                    result.StatusChanged = !string.Equals(previousStatus, apply.Data.RequestStatus, StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    result.WarningMessage = "Workflow selesai, tetapi tukar shift belum dapat diterapkan: " + apply.Message;
                    _logger.LogWarning(
                        "Auto apply shift swap {RequestId} gagal. {Message}",
                        workflow.ReferenceId,
                        apply.Message);
                }
            }
            catch (Exception ex)
            {
                result.WarningMessage = "Workflow selesai, tetapi auto apply tukar shift gagal: " + ex.Message;
                _logger.LogError(ex, "Auto apply shift swap {RequestId} gagal.", workflow.ReferenceId);
            }

            return result;
        }

        public static string MapStatus(string workflowStatus)
        {
            if (workflowStatus == WorkflowValueConstants.WorkflowStatus.RevisionRequested ||
                workflowStatus == WorkflowValueConstants.WorkflowStatus.Returned)
            {
                return SchedulingRequestValueConstants.ShiftSwapStatus.NeedRevision;
            }

            if (workflowStatus == WorkflowValueConstants.WorkflowStatus.Rejected)
            {
                return SchedulingRequestValueConstants.ShiftSwapStatus.Rejected;
            }

            if (workflowStatus == WorkflowValueConstants.WorkflowStatus.Cancelled ||
                workflowStatus == WorkflowValueConstants.WorkflowStatus.Withdrawn)
            {
                return SchedulingRequestValueConstants.ShiftSwapStatus.Cancelled;
            }

            if (workflowStatus == WorkflowValueConstants.WorkflowStatus.Completed ||
                workflowStatus == WorkflowValueConstants.WorkflowStatus.Approved)
            {
                return SchedulingRequestValueConstants.ShiftSwapStatus.Approved;
            }

            if (workflowStatus == WorkflowValueConstants.WorkflowStatus.Draft)
            {
                return SchedulingRequestValueConstants.ShiftSwapStatus.TargetAccepted;
            }

            return SchedulingRequestValueConstants.ShiftSwapStatus.PendingApproval;
        }

        private static void ApplyAudit(
            WfpShiftSwapRequest source,
            TrxWorkflowInstance workflow,
            string targetStatus,
            Guid actorUserId,
            DateTime now)
        {
            if (targetStatus == SchedulingRequestValueConstants.ShiftSwapStatus.PendingApproval)
            {
                source.ApprovedAt = null;
                source.ApprovedByUserId = null;
                source.RejectedAt = null;
                source.RejectedByUserId = null;
            }
            else if (targetStatus == SchedulingRequestValueConstants.ShiftSwapStatus.Approved)
            {
                source.ApprovedAt ??= workflow.CompletedAt ?? now;
                source.ApprovedByUserId ??= actorUserId;
                source.RejectedAt = null;
                source.RejectedByUserId = null;
            }
            else if (targetStatus == SchedulingRequestValueConstants.ShiftSwapStatus.Rejected)
            {
                source.RejectedAt ??= now;
                source.RejectedByUserId ??= actorUserId;
                source.ApprovedAt = null;
                source.ApprovedByUserId = null;
            }
            else if (targetStatus == SchedulingRequestValueConstants.ShiftSwapStatus.Cancelled)
            {
                source.IsCancel = true;
                source.CancelDateTime ??= workflow.CancelledAt ?? workflow.WithdrawnAt ?? now;
                source.CancelBy = actorUserId;
            }
        }
    }
}
