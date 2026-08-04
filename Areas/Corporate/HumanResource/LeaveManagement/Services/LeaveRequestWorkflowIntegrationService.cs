using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    public class LeaveRequestWorkflowIntegrationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly WorkflowReferenceLifecycleService _referenceLifecycleService;
        private readonly LeaveRequestBalanceLifecycleService _balanceLifecycleService;

        public LeaveRequestWorkflowIntegrationService(
            ApplicationDbContext dbContext,
            WorkflowReferenceLifecycleService referenceLifecycleService,
            LeaveRequestBalanceLifecycleService balanceLifecycleService)
        {
            _dbContext = dbContext;
            _referenceLifecycleService = referenceLifecycleService;
            _balanceLifecycleService = balanceLifecycleService;
        }

        public LeaveRequestWorkflowMetadataResponse GetMetadata()
        {
            return new LeaveRequestWorkflowMetadataResponse
            {
                WorkflowStatuses = new()
                {
                    Option(WorkflowValueConstants.WorkflowStatus.Draft),
                    Option(WorkflowValueConstants.WorkflowStatus.Submitted),
                    Option(WorkflowValueConstants.WorkflowStatus.InProgress),
                    Option(WorkflowValueConstants.WorkflowStatus.RevisionRequested),
                    Option(WorkflowValueConstants.WorkflowStatus.Returned),
                    Option(WorkflowValueConstants.WorkflowStatus.Approved),
                    Option(WorkflowValueConstants.WorkflowStatus.Completed),
                    Option(WorkflowValueConstants.WorkflowStatus.Rejected),
                    Option(WorkflowValueConstants.WorkflowStatus.Cancelled),
                    Option(WorkflowValueConstants.WorkflowStatus.Withdrawn)
                },
                LeaveRequestStatuses = new()
                {
                    Option(LeaveRequestValueConstants.Status.Draft),
                    Option(LeaveRequestValueConstants.Status.Submitted),
                    Option(LeaveRequestValueConstants.Status.WaitingApproval),
                    Option(LeaveRequestValueConstants.Status.NeedRevision),
                    Option(LeaveRequestValueConstants.Status.Approved),
                    Option(LeaveRequestValueConstants.Status.Rejected),
                    Option(LeaveRequestValueConstants.Status.Cancelled),
                    Option(LeaveRequestValueConstants.Status.Taken),
                    Option(LeaveRequestValueConstants.Status.Completed)
                },
                ReservationTimings = new()
                {
                    Option(LeaveValueConstants.ReservationTiming.OnSubmit),
                    Option(LeaveValueConstants.ReservationTiming.OnApproval),
                    Option(LeaveValueConstants.ReservationTiming.None)
                },
                DeductionTimings = new()
                {
                    Option(LeaveValueConstants.DeductionTiming.OnApproval),
                    Option(LeaveValueConstants.DeductionTiming.OnLeaveStart),
                    Option(LeaveValueConstants.DeductionTiming.OnCompletion)
                }
            };
        }

        public async Task<LeaveRequestServiceResult<LeaveRequestWorkflowStatusResponse>>
            GetStatusAsync(
                Guid leaveRequestId,
                CancellationToken cancellationToken = default)
        {
            var request = await _dbContext.Set<WfpLeaveRequest>()
                .AsNoTracking()
                .Include(x => x.LeavePolicy)
                .FirstOrDefaultAsync(
                    x => x.Id == leaveRequestId && !x.IsDelete,
                    cancellationToken);

            if (request == null)
            {
                return LeaveRequestServiceResult<LeaveRequestWorkflowStatusResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan cuti tidak ditemukan.");
            }

            TrxWorkflowInstance? workflow = null;

            if (request.WorkflowInstanceId.HasValue)
            {
                workflow = await _dbContext.Set<TrxWorkflowInstance>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.Id == request.WorkflowInstanceId.Value && !x.IsDelete,
                        cancellationToken);
            }

            var ledgerState = await _dbContext.Set<TrxLeaveBalanceTransaction>()
                .AsNoTracking()
                .Where(x =>
                    x.LeaveRequestId == request.Id &&
                    x.TransactionStatus == LeaveValueConstants.TransactionStatus.Posted &&
                    !x.IsDelete)
                .GroupBy(_ => 1)
                .Select(x => new
                {
                    Reserved = x.Sum(y => y.ReservedDelta),
                    Used = x.Sum(y => y.UsedDelta)
                })
                .FirstOrDefaultAsync(cancellationToken);

            var reserved = Math.Max(0, ledgerState?.Reserved ?? 0);
            var used = ledgerState?.Used ?? 0;
            var expectedStatus = workflow == null
                ? request.LeaveRequestStatus
                : LeaveRequestWorkflowLifecycleService.MapStatus(workflow.WorkflowStatus);

            var statusSynchronized = string.Equals(
                request.LeaveRequestStatus,
                expectedStatus,
                StringComparison.OrdinalIgnoreCase);

            var balanceSynchronized = IsBalanceSynchronized(
                request,
                reserved,
                used);

            var response = new LeaveRequestWorkflowStatusResponse
            {
                LeaveRequestId = request.Id,
                RequestNumber = request.RequestNumber,
                LeaveRequestStatus = request.LeaveRequestStatus,
                ExpectedLeaveRequestStatus = expectedStatus,
                WorkflowInstanceId = workflow?.Id,
                WorkflowRequestNumber = workflow?.RequestNumber,
                WorkflowStatus = workflow?.WorkflowStatus,
                CurrentWorkflowStepOrder = workflow?.CurrentStepOrder ?? 0,
                CurrentWorkflowStepCode = workflow?.CurrentStepCode,
                WorkforceProfileId = request.WorkforceProfileId,
                LeaveTypeId = request.LeaveTypeId,
                LeaveBalanceId = request.LeaveBalanceId,
                LeavePolicyId = request.LeavePolicyId,
                ReservationTiming = request.LeavePolicy?.ReservationTiming,
                DeductionTiming = request.LeavePolicy?.DeductionTiming,
                EstimatedBalanceDeduction = request.EstimatedBalanceDeduction,
                ActualBalanceDeduction = request.ActualBalanceDeduction,
                CurrentReservedDays = reserved,
                CurrentUsedDays = used,
                IsStatusSynchronized = statusSynchronized,
                IsBalanceSynchronized = balanceSynchronized,
                RequiresBalanceRetry = statusSynchronized && !balanceSynchronized,
                CanSynchronize = workflow != null,
                SubmittedAt = request.SubmittedAt,
                ApprovedAt = request.ApprovedAt,
                RejectedAt = request.RejectedAt,
                CancelledAt = request.CancelledAt,
                WorkflowLastActionAt = workflow?.LastActionAt
            };

            if (workflow == null)
            {
                response.Issues.Add("Workflow instance belum tersedia.");
            }

            if (!statusSynchronized)
            {
                response.Issues.Add(
                    $"Status source {request.LeaveRequestStatus} tidak sesuai status workflow {workflow?.WorkflowStatus}.");
            }

            if (!balanceSynchronized)
            {
                response.Issues.Add("Dampak reservation/deduction leave balance belum sesuai lifecycle workflow.");
            }

            if (workflow != null)
            {
                response.AvailableActions.Add("Synchronize");
            }

            if (response.RequiresBalanceRetry &&
                string.Equals(
                    request.LeaveRequestStatus,
                    LeaveRequestValueConstants.Status.Approved,
                    StringComparison.OrdinalIgnoreCase))
            {
                response.AvailableActions.Add("RetryBalance");
            }

            return LeaveRequestServiceResult<LeaveRequestWorkflowStatusResponse>.Ok(
                response,
                "Status integrasi workflow leave request berhasil diambil.");
        }

        public async Task<LeaveRequestServiceResult<LeaveRequestWorkflowSynchronizationResponse>>
            SynchronizeAsync(
                Guid leaveRequestId,
                Guid actorUserId,
                bool allowBalanceApply,
                CancellationToken cancellationToken = default)
        {
            var request = await _dbContext.Set<WfpLeaveRequest>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == leaveRequestId && !x.IsDelete,
                    cancellationToken);

            if (request == null)
            {
                return LeaveRequestServiceResult<LeaveRequestWorkflowSynchronizationResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan cuti tidak ditemukan.");
            }

            if (!request.WorkflowInstanceId.HasValue)
            {
                return LeaveRequestServiceResult<LeaveRequestWorkflowSynchronizationResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Pengajuan belum mempunyai workflow instance.");
            }

            var before = await GetLedgerStateAsync(request.Id, cancellationToken);

            WorkflowReferenceLifecycleSynchronizationResult sync;

            try
            {
                sync = await _referenceLifecycleService.SynchronizeAsync(
                    request.WorkflowInstanceId.Value,
                    actorUserId,
                    allowBalanceApply,
                    cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                return LeaveRequestServiceResult<LeaveRequestWorkflowSynchronizationResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    ex.Message);
            }
            catch (Exception ex)
            {
                return LeaveRequestServiceResult<LeaveRequestWorkflowSynchronizationResponse>.Fail(
                    StatusCodes.Status500InternalServerError,
                    $"Sinkronisasi workflow gagal: {ex.Message}");
            }

            var after = await GetLedgerStateAsync(request.Id, cancellationToken);

            return LeaveRequestServiceResult<LeaveRequestWorkflowSynchronizationResponse>.Ok(
                new LeaveRequestWorkflowSynchronizationResponse
                {
                    LeaveRequestId = request.Id,
                    WorkflowInstanceId = request.WorkflowInstanceId.Value,
                    WorkflowStatus = sync.WorkflowStatus,
                    PreviousLeaveRequestStatus = sync.PreviousReferenceStatus,
                    CurrentLeaveRequestStatus = sync.CurrentReferenceStatus,
                    StatusChanged = sync.StatusChanged,
                    BalanceActionAttempted = sync.AutoApplyAttempted,
                    BalanceActionSucceeded = sync.AutoApplySucceeded,
                    BalanceActionType = ResolveBalanceAction(before.Reserved, after.Reserved, before.Used, after.Used),
                    ReservationBeforeDays = before.Reserved,
                    ReservationAfterDays = after.Reserved,
                    UsedBeforeDays = before.Used,
                    UsedAfterDays = after.Used,
                    IsIdempotent = !sync.StatusChanged &&
                                   Math.Abs(before.Reserved - after.Reserved) < 0.0001m &&
                                   Math.Abs(before.Used - after.Used) < 0.0001m,
                    WarningMessage = sync.WarningMessage
                },
                sync.WarningMessage == null
                    ? "Workflow leave request berhasil disinkronkan."
                    : "Status workflow tersinkron dengan peringatan lifecycle balance.");
        }

        public async Task<LeaveRequestServiceResult<LeaveRequestBalanceLifecycleResponse>>
            RetryBalanceAsync(
                Guid leaveRequestId,
                Guid actorUserId,
                CancellationToken cancellationToken = default)
        {
            return await _balanceLifecycleService.RetryApprovedBalanceAsync(
                leaveRequestId,
                actorUserId,
                cancellationToken);
        }

        private static bool IsBalanceSynchronized(
            WfpLeaveRequest request,
            decimal reserved,
            decimal used)
        {
            if (!request.LeaveBalanceId.HasValue || request.EstimatedBalanceDeduction <= 0)
            {
                return true;
            }

            if (string.Equals(
                    request.LeaveRequestStatus,
                    LeaveRequestValueConstants.Status.NeedRevision,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    request.LeaveRequestStatus,
                    LeaveRequestValueConstants.Status.Rejected,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    request.LeaveRequestStatus,
                    LeaveRequestValueConstants.Status.Cancelled,
                    StringComparison.OrdinalIgnoreCase))
            {
                return reserved <= 0.0001m;
            }

            if (string.Equals(
                    request.LeaveRequestStatus,
                    LeaveRequestValueConstants.Status.WaitingApproval,
                    StringComparison.OrdinalIgnoreCase) &&
                string.Equals(
                    request.LeavePolicy?.ReservationTiming,
                    LeaveValueConstants.ReservationTiming.OnSubmit,
                    StringComparison.OrdinalIgnoreCase))
            {
                return Math.Abs(reserved - request.EstimatedBalanceDeduction) <= 0.0001m;
            }

            if (string.Equals(
                    request.LeaveRequestStatus,
                    LeaveRequestValueConstants.Status.Approved,
                    StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(
                        request.LeavePolicy?.DeductionTiming,
                        LeaveValueConstants.DeductionTiming.OnApproval,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return Math.Abs(used - request.EstimatedBalanceDeduction) <= 0.0001m &&
                           reserved <= 0.0001m;
                }

                if (!string.Equals(
                        request.LeavePolicy?.ReservationTiming,
                        LeaveValueConstants.ReservationTiming.None,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return Math.Abs(reserved - request.EstimatedBalanceDeduction) <= 0.0001m;
                }
            }

            return true;
        }

        private async Task<(decimal Reserved, decimal Used)> GetLedgerStateAsync(
            Guid leaveRequestId,
            CancellationToken cancellationToken)
        {
            var state = await _dbContext.Set<TrxLeaveBalanceTransaction>()
                .AsNoTracking()
                .Where(x =>
                    x.LeaveRequestId == leaveRequestId &&
                    x.TransactionStatus == LeaveValueConstants.TransactionStatus.Posted &&
                    !x.IsDelete)
                .GroupBy(_ => 1)
                .Select(x => new
                {
                    Reserved = x.Sum(y => y.ReservedDelta),
                    Used = x.Sum(y => y.UsedDelta)
                })
                .FirstOrDefaultAsync(cancellationToken);

            return (Math.Max(0, state?.Reserved ?? 0), state?.Used ?? 0);
        }

        private static string ResolveBalanceAction(
            decimal reservedBefore,
            decimal reservedAfter,
            decimal usedBefore,
            decimal usedAfter)
        {
            if (usedAfter > usedBefore)
            {
                return "Deduction";
            }

            if (reservedAfter > reservedBefore)
            {
                return "Reservation";
            }

            if (reservedAfter < reservedBefore)
            {
                return "ReservationRelease";
            }

            return "None";
        }

        private static LeaveRequestWorkflowOptionResponse Option(string value)
        {
            return new LeaveRequestWorkflowOptionResponse
            {
                Value = value,
                Label = value
            };
        }
    }
}
