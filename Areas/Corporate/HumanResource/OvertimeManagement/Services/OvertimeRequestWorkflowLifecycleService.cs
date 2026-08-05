using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    /// <summary>
    /// Sinkronisasi status Generic Workflow ke transaksi Overtime Request.
    /// Generic Workflow tetap menjadi source of truth approval, sedangkan
    /// WfpOvertimeRequest menyimpan status domain dan snapshot current step.
    /// </summary>
    public class OvertimeRequestWorkflowLifecycleService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly OvertimePeriodGuardService _periodGuard;
        private readonly ILogger<OvertimeRequestWorkflowLifecycleService> _logger;

        public OvertimeRequestWorkflowLifecycleService(
            ApplicationDbContext dbContext,
            OvertimePeriodGuardService periodGuard,
            ILogger<OvertimeRequestWorkflowLifecycleService> logger)
        {
            _dbContext = dbContext;
            _periodGuard = periodGuard;
            _logger = logger;
        }

        public async Task<WorkflowReferenceLifecycleSynchronizationResult> SynchronizeAsync(
            TrxWorkflowInstance workflow,
            Guid actorUserId,
            bool allowAutoApply = true,
            CancellationToken cancellationToken = default)
        {
            var request = await _dbContext.WfpOvertimeRequests
                .Include(x => x.Details.Where(d => !d.IsDelete))
                .Include(x => x.SourceOvertimePlanDetail)
                .FirstOrDefaultAsync(
                    x => x.Id == workflow.ReferenceId && !x.IsDelete,
                    cancellationToken);

            if (request == null)
            {
                return new WorkflowReferenceLifecycleSynchronizationResult
                {
                    IsHandled = true,
                    WorkflowInstanceId = workflow.Id,
                    ReferenceId = workflow.ReferenceId,
                    WorkflowStatus = workflow.WorkflowStatus,
                    WarningMessage = "Overtime request sumber workflow tidak ditemukan."
                };
            }

            if (allowAutoApply)
            {
                var periodGuard = await _periodGuard.CheckDateAsync(
                    request.OvertimeDate,
                    null,
                    request.HospitalSiteId,
                    request.OrganizationUnitId,
                    request.DepartmentId,
                    cancellationToken);
                if (!periodGuard.IsWritable)
                {
                    return new WorkflowReferenceLifecycleSynchronizationResult
                    {
                        IsHandled = true,
                        WorkflowInstanceId = workflow.Id,
                        ReferenceId = request.Id,
                        PreviousReferenceStatus = request.OvertimeRequestStatus,
                        CurrentReferenceStatus = request.OvertimeRequestStatus,
                        WorkflowStatus = workflow.WorkflowStatus,
                        StatusChanged = false,
                        AutoApplyAttempted = true,
                        AutoApplySucceeded = false,
                        WarningMessage = periodGuard.Message
                    };
                }
            }

            var previousStatus = request.OvertimeRequestStatus;
            var targetStatus = MapRequestStatus(workflow.WorkflowStatus, previousStatus);
            var now = DateTime.UtcNow;

            request.WorkflowDefinitionId = workflow.WorkflowDefinitionId;
            request.WorkflowInstanceId = workflow.Id;
            request.CurrentApprovalStep = IsTerminalWorkflowStatus(workflow.WorkflowStatus)
                ? 0
                : Math.Max(0, workflow.CurrentStepOrder);

            if (!string.Equals(previousStatus, targetStatus, StringComparison.OrdinalIgnoreCase))
            {
                ApplyRequestStatus(
                    request,
                    targetStatus,
                    actorUserId,
                    now);
            }

            foreach (var detail in request.Details)
            {
                detail.DetailStatus = targetStatus;
                detail.UpdateDateTime = now;
                detail.UpdateBy = actorUserId;

                if (targetStatus == OvertimeValueConstants.RequestStatus.ApprovedForWork)
                {
                    detail.ApprovedMinutes = detail.RequestedMinutes;
                    detail.ApprovedStartAt ??= detail.PlannedStartAt;
                    detail.ApprovedEndAt ??= detail.PlannedEndAt;
                }

                if (targetStatus == OvertimeValueConstants.RequestStatus.Cancelled)
                {
                    detail.IsCancel = true;
                    detail.IsActive = false;
                    detail.CancelDateTime ??= now;
                    detail.CancelBy = actorUserId;
                }
                else if (targetStatus == OvertimeValueConstants.RequestStatus.NeedRevision)
                {
                    detail.IsCancel = false;
                    detail.IsActive = true;
                }
            }

            if (request.SourceOvertimePlanDetail != null)
            {
                request.SourceOvertimePlanDetail.DetailStatus = MapPlanDetailStatus(targetStatus);
                request.SourceOvertimePlanDetail.UpdateDateTime = now;
                request.SourceOvertimePlanDetail.UpdateBy = actorUserId;
            }

            request.UpdateDateTime = now;
            request.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Overtime workflow {WorkflowInstanceId} disinkronkan ke request {OvertimeRequestId}: {PreviousStatus} -> {CurrentStatus}.",
                workflow.Id,
                request.Id,
                previousStatus,
                request.OvertimeRequestStatus);

            return new WorkflowReferenceLifecycleSynchronizationResult
            {
                IsHandled = true,
                WorkflowInstanceId = workflow.Id,
                ReferenceId = request.Id,
                PreviousReferenceStatus = previousStatus,
                CurrentReferenceStatus = request.OvertimeRequestStatus,
                WorkflowStatus = workflow.WorkflowStatus,
                StatusChanged = !string.Equals(
                    previousStatus,
                    request.OvertimeRequestStatus,
                    StringComparison.OrdinalIgnoreCase),
                AutoApplyAttempted = allowAutoApply,
                AutoApplySucceeded = true
            };
        }

        public static string MapRequestStatus(
            string workflowStatus,
            string currentRequestStatus)
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
                return OvertimeValueConstants.RequestStatus.NeedRevision;
            }

            if (string.Equals(
                    workflowStatus,
                    WorkflowValueConstants.WorkflowStatus.Rejected,
                    StringComparison.OrdinalIgnoreCase))
            {
                return OvertimeValueConstants.RequestStatus.Rejected;
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
                return OvertimeValueConstants.RequestStatus.Cancelled;
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
                return OvertimeValueConstants.RequestStatus.ApprovedForWork;
            }

            if (string.Equals(
                    workflowStatus,
                    WorkflowValueConstants.WorkflowStatus.Draft,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    workflowStatus,
                    WorkflowValueConstants.WorkflowStatus.Submitted,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    workflowStatus,
                    WorkflowValueConstants.WorkflowStatus.InProgress,
                    StringComparison.OrdinalIgnoreCase))
            {
                return OvertimeValueConstants.RequestStatus.Submitted;
            }

            return currentRequestStatus;
        }

        private static void ApplyRequestStatus(
            WfpOvertimeRequest request,
            string targetStatus,
            Guid actorUserId,
            DateTime now)
        {
            request.OvertimeRequestStatus = targetStatus;

            switch (targetStatus)
            {
                case OvertimeValueConstants.RequestStatus.Submitted:
                    request.SubmittedAt ??= now;
                    request.SubmittedByUserId ??= actorUserId;
                    request.IsCancel = false;
                    request.IsActive = true;
                    break;

                case OvertimeValueConstants.RequestStatus.NeedRevision:
                    request.IsCancel = false;
                    request.IsActive = true;
                    break;

                case OvertimeValueConstants.RequestStatus.ApprovedForWork:
                    request.ApprovedMinutes = request.RequestedMinutes;
                    request.ApprovedAt ??= now;
                    request.ApprovedByUserId = actorUserId;
                    request.RejectedAt = null;
                    request.RejectedByUserId = null;
                    request.IsCancel = false;
                    request.IsActive = true;
                    break;

                case OvertimeValueConstants.RequestStatus.Rejected:
                    request.RejectedAt ??= now;
                    request.RejectedByUserId = actorUserId;
                    request.IsActive = true;
                    break;

                case OvertimeValueConstants.RequestStatus.Cancelled:
                    request.CancelledAt ??= now;
                    request.CancelledByUserId = actorUserId;
                    request.IsCancel = true;
                    request.IsActive = false;
                    request.CancelDateTime ??= now;
                    request.CancelBy = actorUserId;
                    break;
            }

        }

        private static string MapPlanDetailStatus(string requestStatus) => requestStatus switch
        {
            OvertimeValueConstants.RequestStatus.Cancelled =>
                OvertimeValueConstants.PlanDetailStatus.Cancelled,
            _ => OvertimeValueConstants.PlanDetailStatus.RequestGenerated
        };

        private static bool IsTerminalWorkflowStatus(string workflowStatus) =>
            string.Equals(workflowStatus, WorkflowValueConstants.WorkflowStatus.Completed, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(workflowStatus, WorkflowValueConstants.WorkflowStatus.Approved, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(workflowStatus, WorkflowValueConstants.WorkflowStatus.Rejected, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(workflowStatus, WorkflowValueConstants.WorkflowStatus.Cancelled, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(workflowStatus, WorkflowValueConstants.WorkflowStatus.Withdrawn, StringComparison.OrdinalIgnoreCase);


    }
}
