using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services
{
    public class WorkflowReferenceLifecycleSynchronizationResult
    {
        public bool IsHandled { get; set; }

        public Guid WorkflowInstanceId { get; set; }

        public Guid ReferenceId { get; set; }

        public string PreviousReferenceStatus { get; set; } = string.Empty;

        public string CurrentReferenceStatus { get; set; } = string.Empty;

        public string WorkflowStatus { get; set; } = string.Empty;

        public bool StatusChanged { get; set; }

        public bool AutoApplyAttempted { get; set; }

        public bool AutoApplySucceeded { get; set; }

        public string? WarningMessage { get; set; }
    }

    /// <summary>
    /// Menyinkronkan lifecycle Workflow Engine ke transaksi sumber.
    /// Service ini sengaja generik pada pintu masuknya. Penanganan per reference type
    /// ditempatkan sebagai handler terpisah agar modul lain dapat ditambahkan tanpa
    /// menduplikasi business rule approval.
    /// </summary>
    public class WorkflowReferenceLifecycleService
    {
        public const string EmployeeProfileChangeReferenceType =
            "EMPLOYEE_PROFILE_CHANGE";

        public const string AttendanceCorrectionReferenceType =
            "ATTENDANCE_CORRECTION";

        public const string LeaveAdjustmentReferenceType =
            "LEAVE_ADJUSTMENT";

        public const string LeaveRequestReferenceType =
            "LEAVE_REQUEST";

        public const string LeaveCancellationReferenceType =
            "LEAVE_CANCELLATION";

        public const string LeaveRecallReferenceType =
            "LEAVE_RECALL";

        public const string OvertimeRequestReferenceType =
            OvertimeValueConstants.Workflow.ReferenceType;

        public const string ScheduleChangeReferenceType =
            SchedulingRequestValueConstants.Workflow.ScheduleChangeReferenceType;

        public const string ShiftSwapReferenceType =
            SchedulingRequestValueConstants.Workflow.ShiftSwapReferenceType;

        public const string ResignationReferenceType =
            ResignationValueConstants.Workflow.ReferenceType;

        private static readonly HashSet<string> EmployeeProfileChangeReferenceAliases =
            new(StringComparer.OrdinalIgnoreCase)
            {
                EmployeeProfileChangeReferenceType,
                "EmployeeProfileChange",
                "TrxEmployeeProfileChangeRequest",
                "PROFILE_CHANGE"
            };

        private static readonly HashSet<string> AttendanceCorrectionReferenceAliases =
            new(StringComparer.OrdinalIgnoreCase)
            {
                AttendanceCorrectionReferenceType,
                "AttendanceCorrection",
                "TrxAttendanceCorrectionRequest",
                "ATTENDANCE_CORRECTION_REQUEST"
            };

        private static readonly HashSet<string> LeaveAdjustmentReferenceAliases =
            new(StringComparer.OrdinalIgnoreCase)
            {
                LeaveAdjustmentReferenceType,
                "LeaveAdjustment",
                "TrxLeaveAdjustment",
                "LEAVE_BALANCE_ADJUSTMENT"
            };

        private static readonly HashSet<string> LeaveRequestReferenceAliases =
            new(StringComparer.OrdinalIgnoreCase)
            {
                LeaveRequestReferenceType,
                "LeaveRequest",
                "WfpLeaveRequest",
                "LEAVE_REQUEST_APPROVAL"
            };

        private static readonly HashSet<string> LeaveCancellationReferenceAliases =
            new(StringComparer.OrdinalIgnoreCase)
            {
                LeaveCancellationReferenceType,
                "LeaveCancellation",
                "TrxLeaveCancellationRequest",
                "LEAVE_CANCELLATION_REQUEST"
            };

        private static readonly HashSet<string> LeaveRecallReferenceAliases =
            new(StringComparer.OrdinalIgnoreCase)
            {
                LeaveRecallReferenceType,
                "LEAVE_RETURN_TO_WORK",
                "LeaveRecall",
                "TrxLeaveRecall",
                "LeaveReturnToWork"
            };

        private static readonly HashSet<string> OvertimeRequestReferenceAliases =
            new(StringComparer.OrdinalIgnoreCase)
            {
                OvertimeRequestReferenceType,
                "OvertimeRequest",
                "WfpOvertimeRequest",
                "OVERTIME_REQUEST_APPROVAL"
            };

        private static readonly HashSet<string> ScheduleChangeReferenceAliases =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ScheduleChangeReferenceType,
                "ScheduleChange",
                "WfpScheduleChangeRequest"
            };

        private static readonly HashSet<string> ShiftSwapReferenceAliases =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ShiftSwapReferenceType,
                "ShiftSwap",
                "WfpShiftSwapRequest"
            };

        private static readonly HashSet<string> ResignationReferenceAliases =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ResignationReferenceType,
                "ResignationRequest",
                "TrxResignationRequest",
                "RESIGNATION"
            };

        private readonly ApplicationDbContext _dbContext;
        private readonly EmployeeProfileChangeService _employeeProfileChangeService;
        private readonly AttendanceCorrectionWorkflowLifecycleService
            _attendanceCorrectionWorkflowLifecycleService;
        private readonly LeaveAdjustmentWorkflowLifecycleService
            _leaveAdjustmentWorkflowLifecycleService;
        private readonly LeaveRequestWorkflowLifecycleService
            _leaveRequestWorkflowLifecycleService;
        private readonly LeaveCancellationWorkflowLifecycleService
            _leaveCancellationWorkflowLifecycleService;
        private readonly LeaveRecallWorkflowLifecycleService
            _leaveRecallWorkflowLifecycleService;
        private readonly OvertimeRequestWorkflowLifecycleService
            _overtimeRequestWorkflowLifecycleService;
        private readonly ScheduleChangeWorkflowLifecycleService
            _scheduleChangeWorkflowLifecycleService;
        private readonly ShiftSwapWorkflowLifecycleService
            _shiftSwapWorkflowLifecycleService;
        private readonly ResignationWorkflowLifecycleService
            _resignationWorkflowLifecycleService;
        private readonly ILogger<WorkflowReferenceLifecycleService> _logger;

        public WorkflowReferenceLifecycleService(
            ApplicationDbContext dbContext,
            EmployeeProfileChangeService employeeProfileChangeService,
            AttendanceCorrectionWorkflowLifecycleService
                attendanceCorrectionWorkflowLifecycleService,
            LeaveAdjustmentWorkflowLifecycleService
                leaveAdjustmentWorkflowLifecycleService,
            LeaveRequestWorkflowLifecycleService
                leaveRequestWorkflowLifecycleService,
            LeaveCancellationWorkflowLifecycleService
                leaveCancellationWorkflowLifecycleService,
            LeaveRecallWorkflowLifecycleService
                leaveRecallWorkflowLifecycleService,
            OvertimeRequestWorkflowLifecycleService
                overtimeRequestWorkflowLifecycleService,
            ScheduleChangeWorkflowLifecycleService
                scheduleChangeWorkflowLifecycleService,
            ShiftSwapWorkflowLifecycleService
                shiftSwapWorkflowLifecycleService,
            ResignationWorkflowLifecycleService
                resignationWorkflowLifecycleService,
            ILogger<WorkflowReferenceLifecycleService> logger)
        {
            _dbContext = dbContext;
            _employeeProfileChangeService = employeeProfileChangeService;
            _attendanceCorrectionWorkflowLifecycleService =
                attendanceCorrectionWorkflowLifecycleService;
            _leaveAdjustmentWorkflowLifecycleService =
                leaveAdjustmentWorkflowLifecycleService;
            _leaveRequestWorkflowLifecycleService =
                leaveRequestWorkflowLifecycleService;
            _leaveCancellationWorkflowLifecycleService =
                leaveCancellationWorkflowLifecycleService;
            _leaveRecallWorkflowLifecycleService =
                leaveRecallWorkflowLifecycleService;
            _overtimeRequestWorkflowLifecycleService =
                overtimeRequestWorkflowLifecycleService;
            _scheduleChangeWorkflowLifecycleService =
                scheduleChangeWorkflowLifecycleService;
            _shiftSwapWorkflowLifecycleService =
                shiftSwapWorkflowLifecycleService;
            _resignationWorkflowLifecycleService =
                resignationWorkflowLifecycleService;
            _logger = logger;
        }

        /// <summary>
        /// Hook aman yang dipanggil WorkflowService setelah transaksi workflow commit.
        /// Error sinkronisasi tidak membatalkan action workflow yang sudah sah.
        /// Perbaikan dapat dijalankan ulang melalui endpoint synchronize.
        /// </summary>
        public async Task HandleAsync(
            Guid workflowInstanceId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await SynchronizeAsync(
                    workflowInstanceId,
                    actorUserId,
                    allowAutoApply: true,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Sinkronisasi reference lifecycle gagal untuk workflow {WorkflowInstanceId}.",
                    workflowInstanceId);
            }
        }

        public async Task<WorkflowReferenceLifecycleSynchronizationResult> SynchronizeAsync(
            Guid workflowInstanceId,
            Guid actorUserId,
            bool allowAutoApply = true,
            CancellationToken cancellationToken = default)
        {
            if (workflowInstanceId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Workflow instance id tidak valid.",
                    nameof(workflowInstanceId));
            }

            var workflow = await _dbContext.Set<TrxWorkflowInstance>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == workflowInstanceId && !x.IsDelete,
                    cancellationToken);

            if (workflow == null)
            {
                throw new InvalidOperationException(
                    "Workflow instance tidak ditemukan.");
            }

            if (IsEmployeeProfileChangeReference(workflow.ReferenceType))
            {
                return await SynchronizeEmployeeProfileChangeAsync(
                    workflow,
                    actorUserId,
                    allowAutoApply,
                    cancellationToken);
            }

            if (IsAttendanceCorrectionReference(workflow.ReferenceType))
            {
                return await _attendanceCorrectionWorkflowLifecycleService
                    .SynchronizeAsync(
                        workflow,
                        actorUserId,
                        allowAutoApply,
                        cancellationToken);
            }

            if (IsLeaveAdjustmentReference(workflow.ReferenceType))
            {
                return await _leaveAdjustmentWorkflowLifecycleService
                    .SynchronizeAsync(
                        workflow,
                        actorUserId,
                        allowAutoApply,
                        cancellationToken);
            }

            if (IsLeaveRequestReference(workflow.ReferenceType))
            {
                return await _leaveRequestWorkflowLifecycleService
                    .SynchronizeAsync(
                        workflow,
                        actorUserId,
                        allowAutoApply,
                        cancellationToken);
            }

            if (IsLeaveCancellationReference(workflow.ReferenceType))
            {
                return await _leaveCancellationWorkflowLifecycleService
                    .SynchronizeAsync(
                        workflow,
                        actorUserId,
                        allowAutoApply,
                        cancellationToken);
            }

            if (IsLeaveRecallReference(workflow.ReferenceType))
            {
                return await _leaveRecallWorkflowLifecycleService
                    .SynchronizeAsync(
                        workflow,
                        actorUserId,
                        allowAutoApply,
                        cancellationToken);
            }

            if (IsOvertimeRequestReference(workflow.ReferenceType))
            {
                return await _overtimeRequestWorkflowLifecycleService
                    .SynchronizeAsync(
                        workflow,
                        actorUserId,
                        allowAutoApply,
                        cancellationToken);
            }

            if (IsScheduleChangeReference(workflow.ReferenceType))
            {
                return await _scheduleChangeWorkflowLifecycleService
                    .SynchronizeAsync(
                        workflow,
                        actorUserId,
                        allowAutoApply,
                        cancellationToken);
            }

            if (IsShiftSwapReference(workflow.ReferenceType))
            {
                return await _shiftSwapWorkflowLifecycleService
                    .SynchronizeAsync(
                        workflow,
                        actorUserId,
                        allowAutoApply,
                        cancellationToken);
            }

            if (IsResignationReference(workflow.ReferenceType))
            {
                return await _resignationWorkflowLifecycleService
                    .SynchronizeAsync(
                        workflow,
                        actorUserId,
                        allowAutoApply,
                        cancellationToken);
            }

            return new WorkflowReferenceLifecycleSynchronizationResult
            {
                IsHandled = false,
                WorkflowInstanceId = workflow.Id,
                ReferenceId = workflow.ReferenceId,
                WorkflowStatus = workflow.WorkflowStatus
            };
        }

        public static bool IsEmployeeProfileChangeReference(string? referenceType)
        {
            return !string.IsNullOrWhiteSpace(referenceType) &&
                   EmployeeProfileChangeReferenceAliases.Contains(
                       referenceType.Trim());
        }

        public static bool IsAttendanceCorrectionReference(string? referenceType)
        {
            return !string.IsNullOrWhiteSpace(referenceType) &&
                   AttendanceCorrectionReferenceAliases.Contains(
                       referenceType.Trim());
        }

        public static bool IsLeaveAdjustmentReference(string? referenceType)
        {
            return !string.IsNullOrWhiteSpace(referenceType) &&
                   LeaveAdjustmentReferenceAliases.Contains(
                       referenceType.Trim());
        }

        public static bool IsLeaveRequestReference(string? referenceType)
        {
            return !string.IsNullOrWhiteSpace(referenceType) &&
                   LeaveRequestReferenceAliases.Contains(
                       referenceType.Trim());
        }

        public static bool IsLeaveCancellationReference(string? referenceType)
        {
            return !string.IsNullOrWhiteSpace(referenceType) &&
                   LeaveCancellationReferenceAliases.Contains(
                       referenceType.Trim());
        }

        public static bool IsLeaveRecallReference(string? referenceType)
        {
            return !string.IsNullOrWhiteSpace(referenceType) &&
                   LeaveRecallReferenceAliases.Contains(
                       referenceType.Trim());
        }

        public static bool IsOvertimeRequestReference(string? referenceType)
        {
            return !string.IsNullOrWhiteSpace(referenceType) &&
                   OvertimeRequestReferenceAliases.Contains(
                       referenceType.Trim());
        }

        public static bool IsScheduleChangeReference(string? referenceType)
        {
            return !string.IsNullOrWhiteSpace(referenceType) &&
                   ScheduleChangeReferenceAliases.Contains(referenceType.Trim());
        }

        public static bool IsShiftSwapReference(string? referenceType)
        {
            return !string.IsNullOrWhiteSpace(referenceType) &&
                   ShiftSwapReferenceAliases.Contains(referenceType.Trim());
        }

        public static bool IsResignationReference(string? referenceType)
        {
            return !string.IsNullOrWhiteSpace(referenceType) &&
                   ResignationReferenceAliases.Contains(referenceType.Trim());
        }

        public static string MapProfileChangeStatus(string workflowStatus)
        {
            if (string.Equals(
                    workflowStatus,
                    WorkflowValueConstants.WorkflowStatus.RevisionRequested,
                    StringComparison.OrdinalIgnoreCase))
            {
                return "NeedRevision";
            }

            if (string.Equals(
                    workflowStatus,
                    WorkflowValueConstants.WorkflowStatus.Rejected,
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Rejected";
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
                return "Cancelled";
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
                return "Approved";
            }

            if (string.Equals(
                    workflowStatus,
                    WorkflowValueConstants.WorkflowStatus.Draft,
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Draft";
            }

            return "Submitted";
        }

        private async Task<WorkflowReferenceLifecycleSynchronizationResult>
            SynchronizeEmployeeProfileChangeAsync(
                TrxWorkflowInstance workflow,
                Guid actorUserId,
                bool allowAutoApply,
                CancellationToken cancellationToken)
        {
            var request = await _dbContext.Set<TrxEmployeeProfileChangeRequest>()
                .FirstOrDefaultAsync(
                    x => x.Id == workflow.ReferenceId && !x.IsDelete,
                    cancellationToken);

            if (request == null)
            {
                throw new InvalidOperationException(
                    "Employee profile change yang menjadi reference workflow tidak ditemukan.");
            }

            var now = DateTime.UtcNow;
            var effectiveActorUserId = actorUserId != Guid.Empty
                ? actorUserId
                : workflow.RequestedByUserId;

            var previousStatus = request.RequestStatus;
            var targetStatus = MapProfileChangeStatus(workflow.WorkflowStatus);

            if (string.Equals(
                    workflow.WorkflowStatus,
                    WorkflowValueConstants.WorkflowStatus.InProgress,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    workflow.WorkflowStatus,
                    WorkflowValueConstants.WorkflowStatus.Submitted,
                    StringComparison.OrdinalIgnoreCase))
            {
                var currentStepType = await _dbContext.Set<TrxWorkflowStepInstance>()
                    .AsNoTracking()
                    .Where(x =>
                        x.WorkflowInstanceId == workflow.Id &&
                        x.IsCurrentStep &&
                        x.IsActive &&
                        !x.IsDelete)
                    .OrderBy(x => x.StepOrder)
                    .Select(x => x.StepTypeSnapshot)
                    .FirstOrDefaultAsync(cancellationToken);

                if (string.Equals(
                        currentStepType,
                        WorkflowValueConstants.StepType.Verification,
                        StringComparison.OrdinalIgnoreCase))
                {
                    targetStatus = "UnderVerification";
                }
            }

            request.WorkflowDefinitionId = workflow.WorkflowDefinitionId;
            request.CurrentStepOrder = workflow.CurrentStepOrder;
            request.RequestStatus = targetStatus;
            request.UpdateDateTime = now;
            request.UpdateBy = effectiveActorUserId;

            ApplyProfileChangeLifecycleAudit(
                request,
                workflow,
                targetStatus,
                effectiveActorUserId,
                now);

            await SynchronizeProfileChangeDetailStatusesAsync(
                request.Id,
                targetStatus,
                effectiveActorUserId,
                now,
                cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            var result = new WorkflowReferenceLifecycleSynchronizationResult
            {
                IsHandled = true,
                WorkflowInstanceId = workflow.Id,
                ReferenceId = request.Id,
                PreviousReferenceStatus = previousStatus,
                CurrentReferenceStatus = request.RequestStatus,
                WorkflowStatus = workflow.WorkflowStatus,
                StatusChanged = !string.Equals(
                    previousStatus,
                    request.RequestStatus,
                    StringComparison.OrdinalIgnoreCase)
            };

            if (!allowAutoApply ||
                !string.Equals(
                    workflow.WorkflowStatus,
                    WorkflowValueConstants.WorkflowStatus.Completed,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    request.RequestStatus,
                    "Applied",
                    StringComparison.OrdinalIgnoreCase))
            {
                return result;
            }

            result.AutoApplyAttempted = true;

            // Hindari stale tracked entity ketika service domain memuat request yang sama.
            _dbContext.Entry(request).State = EntityState.Detached;

            EmployeeProfileChangeServiceResult<EmployeeProfileChangeApplyResponse>
                applyResult;

            try
            {
                applyResult = await _employeeProfileChangeService.ApplyAsync(
                    workflow.ReferenceId,
                    new ApplyEmployeeProfileChangeRequest
                    {
                        EnforceOldValueMatch = true,
                        Note =
                            "Diterapkan otomatis setelah generic workflow PROFILE_CHANGE selesai."
                    },
                    effectiveActorUserId,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                result.WarningMessage =
                    $"Workflow selesai, tetapi auto-apply gagal: {ex.Message}";

                _logger.LogError(
                    ex,
                    "Auto-apply employee profile change {ProfileChangeRequestId} gagal setelah workflow {WorkflowInstanceId} selesai.",
                    workflow.ReferenceId,
                    workflow.Id);

                return result;
            }

            result.AutoApplySucceeded = applyResult.Success;

            if (!applyResult.Success)
            {
                result.WarningMessage =
                    "Workflow selesai dan request sudah berstatus Approved, " +
                    $"tetapi auto-apply belum berhasil: {applyResult.Message}";

                _logger.LogWarning(
                    "Auto-apply employee profile change {ProfileChangeRequestId} belum berhasil. StatusCode={StatusCode}, Message={Message}",
                    workflow.ReferenceId,
                    applyResult.StatusCode,
                    applyResult.Message);

                return result;
            }

            result.CurrentReferenceStatus = "Applied";
            result.StatusChanged = !string.Equals(
                previousStatus,
                result.CurrentReferenceStatus,
                StringComparison.OrdinalIgnoreCase);

            return result;
        }

        private static void ApplyProfileChangeLifecycleAudit(
            TrxEmployeeProfileChangeRequest request,
            TrxWorkflowInstance workflow,
            string targetStatus,
            Guid actorUserId,
            DateTime now)
        {
            if (string.Equals(
                    targetStatus,
                    "Submitted",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    targetStatus,
                    "UnderVerification",
                    StringComparison.OrdinalIgnoreCase))
            {
                request.SubmittedAt ??= workflow.SubmittedAt ?? now;
                request.ApprovedAt = null;
                request.ApprovedByUserId = null;
                request.RejectedAt = null;
                request.RejectedByUserId = null;
                return;
            }

            if (string.Equals(
                    targetStatus,
                    "NeedRevision",
                    StringComparison.OrdinalIgnoreCase))
            {
                request.ApprovedAt = null;
                request.ApprovedByUserId = null;
                request.RejectedAt = null;
                request.RejectedByUserId = null;
                return;
            }

            if (string.Equals(
                    targetStatus,
                    "Approved",
                    StringComparison.OrdinalIgnoreCase))
            {
                request.ApprovedAt ??= workflow.CompletedAt ?? now;
                request.ApprovedByUserId ??= actorUserId;
                request.RejectedAt = null;
                request.RejectedByUserId = null;
                return;
            }

            if (string.Equals(
                    targetStatus,
                    "Rejected",
                    StringComparison.OrdinalIgnoreCase))
            {
                request.RejectedAt ??= now;
                request.RejectedByUserId ??= actorUserId;
                request.ApprovedAt = null;
                request.ApprovedByUserId = null;
                return;
            }

            if (string.Equals(
                    targetStatus,
                    "Cancelled",
                    StringComparison.OrdinalIgnoreCase))
            {
                request.IsCancel = true;
                request.CancelDateTime ??=
                    workflow.CancelledAt ?? workflow.WithdrawnAt ?? now;
                request.CancelBy = actorUserId;
            }
        }

        private async Task SynchronizeProfileChangeDetailStatusesAsync(
            Guid requestId,
            string requestStatus,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var details = await _dbContext.Set<TrxEmployeeProfileChangeDetail>()
                .Where(x =>
                    x.ProfileChangeRequestId == requestId &&
                    !x.IsDelete)
                .ToListAsync(cancellationToken);

            foreach (var detail in details)
            {
                var nextStatus = requestStatus switch
                {
                    "NeedRevision" => "NeedRevision",
                    "Rejected" => "Rejected",
                    "Cancelled" => "Cancelled",
                    "Approved" when !detail.RequiresVerification => "Verified",
                    "Approved" when string.Equals(
                        detail.DetailStatus,
                        "Verified",
                        StringComparison.OrdinalIgnoreCase) => "Verified",
                    "Approved" => detail.DetailStatus,
                    "Submitted" when string.Equals(
                        detail.DetailStatus,
                        "NeedRevision",
                        StringComparison.OrdinalIgnoreCase) => "Pending",
                    "UnderVerification" when string.Equals(
                        detail.DetailStatus,
                        "NeedRevision",
                        StringComparison.OrdinalIgnoreCase) => "Pending",
                    _ => detail.DetailStatus
                };

                if (string.Equals(
                        detail.DetailStatus,
                        nextStatus,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                detail.DetailStatus = nextStatus;
                detail.UpdateDateTime = now;
                detail.UpdateBy = actorUserId;
            }
        }
    }
}
