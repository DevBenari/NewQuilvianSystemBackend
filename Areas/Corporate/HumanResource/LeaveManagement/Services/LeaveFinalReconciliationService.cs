using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    public class LeaveFinalReconciliationService
    {
        private const decimal Tolerance = 0.0001m;

        private readonly ApplicationDbContext _dbContext;
        private readonly LeaveCancellationService _cancellationService;
        private readonly LeaveRecallService _recallService;
        private readonly LeaveExecutionProcessorService _executionProcessorService;

        public LeaveFinalReconciliationService(
            ApplicationDbContext dbContext,
            LeaveCancellationService cancellationService,
            LeaveRecallService recallService,
            LeaveExecutionProcessorService executionProcessorService)
        {
            _dbContext = dbContext;
            _cancellationService = cancellationService;
            _recallService = recallService;
            _executionProcessorService = executionProcessorService;
        }

        public async Task<LeaveRequestServiceResult<LeaveFinalReconciliationResponse>> GetAsync(
            Guid leaveRequestId,
            CancellationToken cancellationToken = default)
        {
            var request = await _dbContext.Set<WfpLeaveRequest>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == leaveRequestId && !x.IsDelete, cancellationToken);

            if (request == null)
                return LeaveRequestServiceResult<LeaveFinalReconciliationResponse>.Fail(StatusCodes.Status404NotFound, "Leave request tidak ditemukan.");

            var execution = await _dbContext.Set<TrxLeaveExecution>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.LeaveRequestId == leaveRequestId && !x.IsDelete, cancellationToken);

            var integrations = await _dbContext.Set<TrxLeaveAttendanceIntegration>().AsNoTracking()
                .Include(x => x.AttendanceDaily)
                .Where(x => x.LeaveRequestId == leaveRequestId && !x.IsDelete)
                .ToListAsync(cancellationToken);

            var ledgers = await _dbContext.Set<TrxLeaveBalanceTransaction>().AsNoTracking()
                .Where(x => x.LeaveRequestId == leaveRequestId &&
                    x.TransactionStatus == LeaveValueConstants.TransactionStatus.Posted && !x.IsDelete)
                .ToListAsync(cancellationToken);

            var cancellation = await _dbContext.Set<TrxLeaveCancellationRequest>().AsNoTracking()
                .Where(x => x.LeaveRequestId == leaveRequestId && !x.IsDelete)
                .OrderByDescending(x => x.CreateDateTime)
                .FirstOrDefaultAsync(cancellationToken);

            var recall = await _dbContext.Set<TrxLeaveRecall>().AsNoTracking()
                .Where(x => x.LeaveRequestId == leaveRequestId && !x.IsDelete)
                .OrderByDescending(x => x.CreateDateTime)
                .FirstOrDefaultAsync(cancellationToken);

            var workflowIds = new[] { request.WorkflowInstanceId, cancellation?.WorkflowInstanceId, recall?.WorkflowInstanceId }
                .Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
            var workflows = workflowIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _dbContext.Set<TrxWorkflowInstance>().AsNoTracking()
                    .Where(x => workflowIds.Contains(x.Id) && !x.IsDelete)
                    .ToDictionaryAsync(x => x.Id, x => x.WorkflowStatus, cancellationToken);

            string? GetWorkflowStatus(Guid? id) => id.HasValue && workflows.TryGetValue(id.Value, out var status) ? status : null;

            var response = new LeaveFinalReconciliationResponse
            {
                LeaveRequestId = request.Id,
                RequestNumber = request.RequestNumber,
                LeaveRequestStatus = request.LeaveRequestStatus,
                WorkflowStatus = GetWorkflowStatus(request.WorkflowInstanceId),
                ExecutionStatus = execution?.ExecutionStatus,
                AttendanceIntegrationStatus = execution?.AttendanceIntegrationStatus,
                BalanceExecutionStatus = execution?.BalanceExecutionStatus,
                CancellationStatus = cancellation?.CancellationStatus,
                CancellationWorkflowStatus = GetWorkflowStatus(cancellation?.WorkflowInstanceId),
                RecallStatus = recall?.RecallStatus,
                RecallWorkflowStatus = GetWorkflowStatus(recall?.WorkflowInstanceId),
                EstimatedBalanceDeduction = request.EstimatedBalanceDeduction,
                ActualBalanceDeduction = request.ActualBalanceDeduction,
                LedgerReservedDays = ledgers.Sum(x => x.ReservedDelta),
                LedgerUsedDays = ledgers.Sum(x => x.UsedDelta),
                LedgerRestoredDays = ledgers.Where(x => x.TransactionType == LeaveValueConstants.TransactionType.CancellationRestore).Sum(x => x.TransactionDays),
                IntegratedLeaveDays = integrations.Where(x => x.IntegrationStatus == LeaveExecutionValueConstants.AttendanceIntegrationStatus.Applied).Sum(x => x.RequestedLeaveDays),
                AppliedAttendanceDayCount = integrations.Count(x => x.IntegrationStatus == LeaveExecutionValueConstants.AttendanceIntegrationStatus.Applied),
                ConflictAttendanceDayCount = integrations.Count(x => x.IntegrationStatus == LeaveExecutionValueConstants.AttendanceIntegrationStatus.Conflict),
                FailedAttendanceDayCount = integrations.Count(x => x.IntegrationStatus == LeaveExecutionValueConstants.AttendanceIntegrationStatus.Failed),
                ReversedAttendanceDayCount = integrations.Count(x => x.IntegrationStatus == LeaveExecutionValueConstants.AttendanceIntegrationStatus.Reversed),
                PayrollProcessedAttendanceDayCount = integrations.Count(x => x.AttendanceDaily?.PayrollInputStatus == "Processed"),
                LockedAttendanceDayCount = integrations.Count(x => x.AttendanceDaily?.IsLocked == true)
            };

            response.IsTerminal = request.LeaveRequestStatus == LeaveRequestValueConstants.Status.Completed ||
                request.LeaveRequestStatus == LeaveRequestValueConstants.Status.Cancelled ||
                request.LeaveRequestStatus == LeaveRequestValueConstants.Status.Rejected ||
                request.LeaveRequestStatus == LeaveRequestValueConstants.Status.Recalled ||
                request.LeaveRequestStatus == LeaveRequestValueConstants.Status.Expired;

            if (request.WorkflowInstanceId.HasValue && string.IsNullOrWhiteSpace(response.WorkflowStatus))
                AddIssue(response, "REQUEST_WORKFLOW_MISSING", LeaveLifecycleValueConstants.ReconciliationSeverity.Critical, "Workflow utama leave request tidak ditemukan.", "Periksa workflow instance atau jalankan sinkronisasi data.");

            if (response.ConflictAttendanceDayCount > 0)
                AddIssue(response, "ATTENDANCE_CONFLICT", LeaveLifecycleValueConstants.ReconciliationSeverity.Critical, "Masih terdapat integrasi attendance berstatus Conflict.", "Selesaikan konflik punch/attendance sebelum finalisasi.");

            if (response.FailedAttendanceDayCount > 0)
                AddIssue(response, "ATTENDANCE_FAILED", LeaveLifecycleValueConstants.ReconciliationSeverity.Critical, "Masih terdapat integrasi attendance yang gagal.", "Jalankan retry leave execution.");

            if (execution != null && execution.ExpectedAttendanceDayCount != integrations.Count)
                AddIssue(response, "EXECUTION_DAY_COUNT_MISMATCH", LeaveLifecycleValueConstants.ReconciliationSeverity.Warning, "Expected attendance day count tidak sama dengan jumlah detail integrasi.", "Jalankan ulang leave execution dan reconciliation.");

            if (Math.Abs(response.LedgerUsedDays - request.ActualBalanceDeduction) > Tolerance)
                AddIssue(response, "BALANCE_USED_MISMATCH", LeaveLifecycleValueConstants.ReconciliationSeverity.Critical, "ActualBalanceDeduction tidak sama dengan total UsedDelta ledger.", "Jalankan balance reconciliation sebelum payroll final.");

            if (response.IsTerminal && Math.Abs(response.LedgerReservedDays) > Tolerance)
                AddIssue(response, "RESERVATION_NOT_RELEASED", LeaveLifecycleValueConstants.ReconciliationSeverity.Critical, "Request terminal masih mempunyai reservation saldo.", "Jalankan workflow/balance synchronization.");

            if (cancellation?.CancellationStatus == LeaveLifecycleValueConstants.CancellationStatus.Approved)
                AddIssue(response, "CANCELLATION_PENDING_APPLY", LeaveLifecycleValueConstants.ReconciliationSeverity.Critical, "Cancellation sudah Approved tetapi belum Applied.", "Jalankan apply cancellation.");

            if (recall?.RecallStatus == LeaveLifecycleValueConstants.RecallStatus.Approved)
                AddIssue(response, "RECALL_PENDING_APPLY", LeaveLifecycleValueConstants.ReconciliationSeverity.Critical, "Recall sudah Approved tetapi belum Applied.", "Jalankan apply recall/return-to-work.");

            if ((cancellation?.CancellationStatus == LeaveLifecycleValueConstants.CancellationStatus.Submitted ||
                 recall?.RecallStatus == LeaveLifecycleValueConstants.RecallStatus.Submitted) &&
                response.PayrollProcessedAttendanceDayCount > 0)
                AddIssue(response, "PAYROLL_ALREADY_PROCESSED", LeaveLifecycleValueConstants.ReconciliationSeverity.Critical, "Perubahan lifecycle masih berjalan tetapi attendance sudah diproses payroll.", "Rollback payroll handoff terlebih dahulu.");

            if (response.LockedAttendanceDayCount > 0 &&
                (cancellation?.CancellationStatus == LeaveLifecycleValueConstants.CancellationStatus.Approved ||
                 recall?.RecallStatus == LeaveLifecycleValueConstants.RecallStatus.Approved))
                AddIssue(response, "ATTENDANCE_LOCKED", LeaveLifecycleValueConstants.ReconciliationSeverity.Critical, "Attendance terkunci sehingga cancellation/recall tidak dapat diterapkan.", "Rollback payroll atau reopen attendance period secara terkontrol.");

            response.IsBalanced = response.Issues.All(x => x.Severity != LeaveLifecycleValueConstants.ReconciliationSeverity.Critical);
            response.RequiresAttention = response.Issues.Count > 0;
            response.AvailableRepairActions = ResolveRepairActions(response);

            return LeaveRequestServiceResult<LeaveFinalReconciliationResponse>.Ok(response, "Final reconciliation leave berhasil dihitung.");
        }

        public async Task<LeaveRequestServiceResult<RepairLeaveFinalReconciliationResponse>> RepairAsync(
            Guid leaveRequestId,
            RepairLeaveFinalReconciliationRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var result = new RepairLeaveFinalReconciliationResponse { LeaveRequestId = leaveRequestId };

            var leave = await _dbContext.Set<WfpLeaveRequest>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == leaveRequestId && !x.IsDelete, cancellationToken);
            if (leave == null)
                return LeaveRequestServiceResult<RepairLeaveFinalReconciliationResponse>.Fail(StatusCodes.Status404NotFound, "Leave request tidak ditemukan.");

            if (request.SynchronizeWorkflow)
            {
                var cancellation = await _dbContext.Set<TrxLeaveCancellationRequest>().AsNoTracking()
                    .Where(x => x.LeaveRequestId == leaveRequestId && x.WorkflowInstanceId.HasValue && !x.IsDelete)
                    .OrderByDescending(x => x.CreateDateTime).FirstOrDefaultAsync(cancellationToken);
                if (cancellation != null)
                    await RunActionAsync(result, "Synchronize cancellation", async () =>
                        await _cancellationService.SynchronizeAsync(cancellation.Id, actorUserId, request.ApplyApprovedCancellation, cancellationToken));

                var recall = await _dbContext.Set<TrxLeaveRecall>().AsNoTracking()
                    .Where(x => x.LeaveRequestId == leaveRequestId && x.WorkflowInstanceId.HasValue && !x.IsDelete)
                    .OrderByDescending(x => x.CreateDateTime).FirstOrDefaultAsync(cancellationToken);
                if (recall != null)
                    await RunActionAsync(result, "Synchronize recall", async () =>
                        await _recallService.SynchronizeAsync(recall.Id, actorUserId, request.ApplyApprovedRecall, cancellationToken));
            }

            if (request.ExecuteApprovedLeave &&
                (leave.LeaveRequestStatus == LeaveRequestValueConstants.Status.Approved || leave.LeaveRequestStatus == LeaveRequestValueConstants.Status.Taken))
            {
                result.AttemptedActionCount++;
                var execute = await _executionProcessorService.ExecuteAsync(leaveRequestId, new ExecuteLeaveRequestRequest
                {
                    AsOfDate = request.AsOfDate,
                    ForceRetry = true,
                    CorrelationId = $"LEAVE-FINAL-REPAIR:{leaveRequestId:N}",
                    Notes = request.Notes ?? "Repair dari final reconciliation."
                }, actorUserId, cancellationToken);

                if (execute.Success)
                {
                    result.SucceededActionCount++;
                    result.Messages.Add("Leave execution berhasil disinkronkan.");
                }
                else
                {
                    result.FailedActionCount++;
                    result.Messages.Add($"Leave execution gagal: {execute.Message}");
                }
            }

            var reconciliation = await GetAsync(leaveRequestId, cancellationToken);
            result.Reconciliation = reconciliation.Data;

            return LeaveRequestServiceResult<RepairLeaveFinalReconciliationResponse>.Ok(result, "Repair final reconciliation selesai dijalankan.");
        }

        private static async Task RunActionAsync(
            RepairLeaveFinalReconciliationResponse response,
            string actionName,
            Func<Task<LeaveRequestServiceResult<LeaveLifecycleActionResponse>>> action)
        {
            response.AttemptedActionCount++;
            var actionResult = await action();
            if (actionResult.Success)
            {
                response.SucceededActionCount++;
                response.Messages.Add($"{actionName}: berhasil.");
            }
            else
            {
                response.FailedActionCount++;
                response.Messages.Add($"{actionName}: {actionResult.Message}");
            }
        }

        private static void AddIssue(LeaveFinalReconciliationResponse response, string code, string severity, string message, string action)
            => response.Issues.Add(new LeaveFinalReconciliationIssueResponse
            {
                Code = code,
                Severity = severity,
                Message = message,
                RecommendedAction = action
            });

        private static List<string> ResolveRepairActions(LeaveFinalReconciliationResponse response)
        {
            var actions = new List<string>();
            if (response.CancellationStatus == LeaveLifecycleValueConstants.CancellationStatus.Approved) actions.Add("ApplyCancellation");
            if (response.RecallStatus == LeaveLifecycleValueConstants.RecallStatus.Approved) actions.Add("ApplyRecall");
            if (response.FailedAttendanceDayCount > 0 || response.ConflictAttendanceDayCount > 0) actions.Add("RetryExecution");
            if (!response.IsBalanced) actions.Add("SynchronizeWorkflow");
            if (response.PayrollProcessedAttendanceDayCount > 0) actions.Add("ReviewPayrollHandoff");
            return actions.Distinct().ToList();
        }
    }
}
