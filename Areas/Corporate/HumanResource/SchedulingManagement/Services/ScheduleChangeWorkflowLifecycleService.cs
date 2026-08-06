using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services
{
    public class ScheduleChangeWorkflowLifecycleService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ScheduleChangeService _scheduleChangeService;
        private readonly ILogger<ScheduleChangeWorkflowLifecycleService> _logger;

        public ScheduleChangeWorkflowLifecycleService(
            ApplicationDbContext dbContext,
            ScheduleChangeService scheduleChangeService,
            ILogger<ScheduleChangeWorkflowLifecycleService> logger)
        {
            _dbContext = dbContext;
            _scheduleChangeService = scheduleChangeService;
            _logger = logger;
        }

        public async Task<WorkflowReferenceLifecycleSynchronizationResult> SynchronizeAsync(
            TrxWorkflowInstance workflow,
            Guid actorUserId,
            bool allowAutoApply,
            CancellationToken cancellationToken = default)
        {
            var source = await _dbContext.WfpScheduleChangeRequests
                .FirstOrDefaultAsync(x => x.Id == workflow.ReferenceId && !x.IsDelete, cancellationToken)
                ?? throw new InvalidOperationException("Pengajuan perubahan jadwal tidak ditemukan.");

            var now = DateTime.UtcNow;
            var effectiveActor = actorUserId != Guid.Empty ? actorUserId : workflow.RequestedByUserId;
            var previousStatus = source.RequestStatus;
            var targetStatus = MapStatus(workflow.WorkflowStatus);

            if (workflow.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.InProgress ||
                workflow.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.Submitted)
            {
                targetStatus = SchedulingRequestValueConstants.ScheduleChangeStatus.UnderReview;
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
                source.RequestStatus == SchedulingRequestValueConstants.ScheduleChangeStatus.Applied)
            {
                return result;
            }

            result.AutoApplyAttempted = true;
            _dbContext.Entry(source).State = EntityState.Detached;

            try
            {
                var apply = await _scheduleChangeService.ApplyAsync(
                    workflow.ReferenceId,
                    effectiveActor,
                    "Applied automatically after schedule change workflow completed.",
                    cancellationToken);

                result.AutoApplySucceeded = apply.Success;
                if (apply.Success && apply.Data != null)
                {
                    result.CurrentReferenceStatus = apply.Data.RequestStatus;
                    result.StatusChanged = !string.Equals(previousStatus, apply.Data.RequestStatus, StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    result.WarningMessage = "Workflow selesai, tetapi perubahan jadwal belum dapat diterapkan: " + apply.Message;
                    _logger.LogWarning(
                        "Auto apply schedule change {RequestId} gagal. {Message}",
                        workflow.ReferenceId,
                        apply.Message);
                }
            }
            catch (Exception ex)
            {
                result.WarningMessage = "Workflow selesai, tetapi auto apply perubahan jadwal gagal: " + ex.Message;
                _logger.LogError(ex, "Auto apply schedule change {RequestId} gagal.", workflow.ReferenceId);
            }

            return result;
        }

        public static string MapStatus(string workflowStatus)
        {
            if (workflowStatus == WorkflowValueConstants.WorkflowStatus.RevisionRequested ||
                workflowStatus == WorkflowValueConstants.WorkflowStatus.Returned)
            {
                return SchedulingRequestValueConstants.ScheduleChangeStatus.NeedRevision;
            }

            if (workflowStatus == WorkflowValueConstants.WorkflowStatus.Rejected)
            {
                return SchedulingRequestValueConstants.ScheduleChangeStatus.Rejected;
            }

            if (workflowStatus == WorkflowValueConstants.WorkflowStatus.Cancelled ||
                workflowStatus == WorkflowValueConstants.WorkflowStatus.Withdrawn)
            {
                return SchedulingRequestValueConstants.ScheduleChangeStatus.Cancelled;
            }

            if (workflowStatus == WorkflowValueConstants.WorkflowStatus.Completed ||
                workflowStatus == WorkflowValueConstants.WorkflowStatus.Approved)
            {
                return SchedulingRequestValueConstants.ScheduleChangeStatus.Approved;
            }

            if (workflowStatus == WorkflowValueConstants.WorkflowStatus.Draft)
            {
                return SchedulingRequestValueConstants.ScheduleChangeStatus.Draft;
            }

            return SchedulingRequestValueConstants.ScheduleChangeStatus.Submitted;
        }

        private static void ApplyAudit(
            WfpScheduleChangeRequest source,
            TrxWorkflowInstance workflow,
            string targetStatus,
            Guid actorUserId,
            DateTime now)
        {
            if (targetStatus == SchedulingRequestValueConstants.ScheduleChangeStatus.Submitted ||
                targetStatus == SchedulingRequestValueConstants.ScheduleChangeStatus.UnderReview)
            {
                source.SubmittedAt ??= workflow.SubmittedAt ?? now;
                source.ApprovedAt = null;
                source.ApprovedByUserId = null;
                source.RejectedAt = null;
                source.RejectedByUserId = null;
            }
            else if (targetStatus == SchedulingRequestValueConstants.ScheduleChangeStatus.Approved)
            {
                source.ApprovedAt ??= workflow.CompletedAt ?? now;
                source.ApprovedByUserId ??= actorUserId;
                source.RejectedAt = null;
                source.RejectedByUserId = null;
            }
            else if (targetStatus == SchedulingRequestValueConstants.ScheduleChangeStatus.Rejected)
            {
                source.RejectedAt ??= now;
                source.RejectedByUserId ??= actorUserId;
                source.ApprovedAt = null;
                source.ApprovedByUserId = null;
            }
            else if (targetStatus == SchedulingRequestValueConstants.ScheduleChangeStatus.Cancelled)
            {
                source.IsCancel = true;
                source.CancelDateTime ??= workflow.CancelledAt ?? workflow.WithdrawnAt ?? now;
                source.CancelBy = actorUserId;
            }
        }
    }
}
