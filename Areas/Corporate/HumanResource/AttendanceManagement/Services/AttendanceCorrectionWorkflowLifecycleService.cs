using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Repositories;
using System.Data;
using System.Globalization;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services
{
    /// <summary>
    /// Handler lifecycle khusus Attendance Correction. Workflow Engine tetap menjadi
    /// pemilik keputusan approval; service ini hanya menyinkronkan status transaksi
    /// sumber dan menerapkan nilai yang telah disetujui ke Attendance Daily.
    /// </summary>
    public class AttendanceCorrectionWorkflowLifecycleService
    {
        private static readonly string[] TerminalRequestStatuses =
        {
            AttendanceValueConstants.CorrectionRequestStatus.Applied,
            AttendanceValueConstants.CorrectionRequestStatus.Rejected,
            AttendanceValueConstants.CorrectionRequestStatus.Cancelled
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<AttendanceCorrectionWorkflowLifecycleService> _logger;

        public AttendanceCorrectionWorkflowLifecycleService(
            ApplicationDbContext dbContext,
            ILogger<AttendanceCorrectionWorkflowLifecycleService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<WorkflowReferenceLifecycleSynchronizationResult> SynchronizeAsync(
            TrxWorkflowInstance workflow,
            Guid actorUserId,
            bool allowAutoApply,
            CancellationToken cancellationToken = default)
        {
            var request = await _dbContext.Set<HrdAttendanceCorrectionRequest>()
                .FirstOrDefaultAsync(
                    x => x.Id == workflow.ReferenceId && !x.IsDelete,
                    cancellationToken);

            if (request == null)
            {
                throw new InvalidOperationException(
                    "Attendance correction yang menjadi reference workflow tidak ditemukan.");
            }

            var effectiveActorUserId = actorUserId != Guid.Empty
                ? actorUserId
                : workflow.RequestedByUserId;

            var now = DateTime.UtcNow;
            var previousStatus = request.RequestStatus;
            var targetStatus = MapRequestStatus(workflow.WorkflowStatus);

            request.WorkflowDefinitionId = workflow.WorkflowDefinitionId;
            request.WorkflowInstanceId = workflow.Id;
            request.RequestStatus = targetStatus;
            request.UpdateDateTime = now;
            request.UpdateBy = effectiveActorUserId;

            ApplyLifecycleAudit(
                request,
                workflow,
                targetStatus,
                effectiveActorUserId,
                now);

            await SynchronizeDetailStatusesAsync(
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

            var workflowCompleted =
                string.Equals(
                    workflow.WorkflowStatus,
                    WorkflowValueConstants.WorkflowStatus.Completed,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    workflow.WorkflowStatus,
                    WorkflowValueConstants.WorkflowStatus.Approved,
                    StringComparison.OrdinalIgnoreCase);

            if (!allowAutoApply ||
                !workflowCompleted ||
                string.Equals(
                    request.RequestStatus,
                    AttendanceValueConstants.CorrectionRequestStatus.Applied,
                    StringComparison.OrdinalIgnoreCase))
            {
                return result;
            }

            result.AutoApplyAttempted = true;
            _dbContext.Entry(request).State = EntityState.Detached;

            AttendanceCorrectionServiceResult<AttendanceCorrectionApplyResponse>
                applyResult;

            try
            {
                applyResult = await ApplyApprovedRequestAsync(
                    workflow.ReferenceId,
                    effectiveActorUserId,
                    "Diterapkan otomatis setelah workflow ATTENDANCE_CORRECTION selesai.",
                    cancellationToken);
            }
            catch (Exception ex)
            {
                result.WarningMessage =
                    $"Workflow selesai, tetapi auto-apply attendance correction gagal: {ex.Message}";

                _logger.LogError(
                    ex,
                    "Auto-apply attendance correction {CorrectionRequestId} gagal setelah workflow {WorkflowInstanceId} selesai.",
                    workflow.ReferenceId,
                    workflow.Id);

                return result;
            }

            result.AutoApplySucceeded = applyResult.Success;

            if (!applyResult.Success)
            {
                result.WarningMessage =
                    "Workflow selesai dan attendance correction sudah berstatus Approved, " +
                    $"tetapi auto-apply belum berhasil: {applyResult.Message}";

                _logger.LogWarning(
                    "Auto-apply attendance correction {CorrectionRequestId} belum berhasil. StatusCode={StatusCode}, Message={Message}",
                    workflow.ReferenceId,
                    applyResult.StatusCode,
                    applyResult.Message);

                return result;
            }

            result.CurrentReferenceStatus =
                AttendanceValueConstants.CorrectionRequestStatus.Applied;
            result.StatusChanged = !string.Equals(
                previousStatus,
                result.CurrentReferenceStatus,
                StringComparison.OrdinalIgnoreCase);

            return result;
        }

        public async Task<AttendanceCorrectionServiceResult<AttendanceCorrectionApplyResponse>>
            ApplyApprovedRequestAsync(
                Guid correctionRequestId,
                Guid actorUserId,
                string? note,
                CancellationToken cancellationToken = default)
        {
            if (correctionRequestId == Guid.Empty || actorUserId == Guid.Empty)
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionApplyResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Attendance correction id atau actor user id tidak valid.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                var request = await _dbContext.Set<HrdAttendanceCorrectionRequest>()
                    .Include(x => x.Details)
                    .Include(x => x.AttendanceDaily)
                    .ThenInclude(x => x!.AttendancePolicy)
                    .Include(x => x.Exceptions)
                    .FirstOrDefaultAsync(
                        x => x.Id == correctionRequestId && !x.IsDelete,
                        cancellationToken);

                if (request == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return AttendanceCorrectionServiceResult<AttendanceCorrectionApplyResponse>.Fail(
                        StatusCodes.Status404NotFound,
                        "Attendance correction tidak ditemukan.");
                }

                if (string.Equals(
                        request.RequestStatus,
                        AttendanceValueConstants.CorrectionRequestStatus.Applied,
                        StringComparison.OrdinalIgnoreCase))
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return AttendanceCorrectionServiceResult<AttendanceCorrectionApplyResponse>.Ok(
                        BuildAlreadyAppliedResponse(request),
                        "Attendance correction sudah pernah diterapkan.");
                }

                if (!string.Equals(
                        request.RequestStatus,
                        AttendanceValueConstants.CorrectionRequestStatus.Approved,
                        StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(
                        request.RequestStatus,
                        AttendanceValueConstants.CorrectionRequestStatus.PartiallyApproved,
                        StringComparison.OrdinalIgnoreCase))
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return AttendanceCorrectionServiceResult<AttendanceCorrectionApplyResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        "Attendance correction hanya dapat diterapkan dari status Approved atau PartiallyApproved.");
                }

                var daily = request.AttendanceDaily;
                if (daily == null || daily.IsDelete)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return AttendanceCorrectionServiceResult<AttendanceCorrectionApplyResponse>.Fail(
                        StatusCodes.Status404NotFound,
                        "Attendance daily sumber koreksi tidak ditemukan.");
                }

                if (daily.IsLocked)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return AttendanceCorrectionServiceResult<AttendanceCorrectionApplyResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        "Attendance daily sedang dikunci dan tidak dapat dikoreksi.");
                }

                if (string.Equals(
                        daily.PayrollInputStatus,
                        AttendanceValueConstants.PayrollInputStatus.Processed,
                        StringComparison.OrdinalIgnoreCase))
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return AttendanceCorrectionServiceResult<AttendanceCorrectionApplyResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        "Attendance daily sudah diproses ke payroll dan tidak dapat dikoreksi otomatis.");
                }

                var activeDetails = request.Details
                    .Where(x => !x.IsDelete && x.IsActive)
                    .OrderBy(x => x.SortOrder)
                    .ThenBy(x => x.CreateDateTime)
                    .ToList();

                if (activeDetails.Count == 0)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return AttendanceCorrectionServiceResult<AttendanceCorrectionApplyResponse>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Attendance correction tidak mempunyai detail aktif.");
                }

                var correctedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var appliedCount = 0;
                var now = DateTime.UtcNow;

                foreach (var detail in activeDetails)
                {
                    if (string.Equals(
                            detail.DetailStatus,
                            "Rejected",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var value = detail.ApprovedValue ?? detail.RequestedValue;
                    var applyError = ApplyFieldValue(
                        daily,
                        detail.FieldName,
                        value);

                    if (applyError != null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return AttendanceCorrectionServiceResult<AttendanceCorrectionApplyResponse>.Fail(
                            StatusCodes.Status400BadRequest,
                            applyError);
                    }

                    correctedFields.Add(detail.FieldName);
                    detail.ApprovedValue ??= detail.RequestedValue;
                    detail.DetailStatus = "Applied";
                    detail.IsApplied = true;
                    detail.AppliedAt = now;
                    detail.AppliedByUserId = actorUserId;
                    detail.UpdateDateTime = now;
                    detail.UpdateBy = actorUserId;
                    appliedCount++;
                }

                if (appliedCount == 0)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return AttendanceCorrectionServiceResult<AttendanceCorrectionApplyResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        "Tidak ada detail koreksi yang dapat diterapkan.");
                }

                RecalculateDaily(daily, correctedFields);

                if (correctedFields.Contains("ScheduledCheckInAt") ||
                    correctedFields.Contains("ScheduledCheckOutAt") ||
                    correctedFields.Contains("WorkScheduleId") ||
                    correctedFields.Contains("ShiftId"))
                {
                    daily.ScheduleSource =
                        AttendanceValueConstants.ScheduleSource.ManualOverride;
                }

                daily.IsCorrected = true;
                daily.ProcessingStatus =
                    AttendanceValueConstants.AttendanceProcessingStatus.Processed;
                daily.ProcessedAt = now;
                daily.ProcessingVersion += 1;
                daily.ProcessingMessage =
                    $"Attendance dikoreksi melalui request {request.RequestNumber}.";
                daily.UpdateDateTime = now;
                daily.UpdateBy = actorUserId;

                var linkedExceptions = await _dbContext.Set<TrxAttendanceException>()
                    .Where(x =>
                        x.CorrectionRequestId == request.Id &&
                        !x.IsDelete &&
                        x.IsActive)
                    .ToListAsync(cancellationToken);

                var closedExceptionCount = 0;
                foreach (var exception in linkedExceptions)
                {
                    if (string.Equals(
                            exception.ExceptionStatus,
                            AttendanceValueConstants.AttendanceExceptionStatus.Closed,
                            StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(
                            exception.ExceptionStatus,
                            AttendanceValueConstants.AttendanceExceptionStatus.Corrected,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    exception.ExceptionStatus =
                        AttendanceValueConstants.AttendanceExceptionStatus.Corrected;
                    exception.ResolvedByUserId = actorUserId;
                    exception.ResolvedAt = now;
                    exception.ResolutionNote =
                        $"Diselesaikan melalui attendance correction {request.RequestNumber}.";
                    exception.UpdateDateTime = now;
                    exception.UpdateBy = actorUserId;
                    closedExceptionCount++;
                }

                var linkedExceptionIds = linkedExceptions
                    .Select(x => x.Id)
                    .ToList();

                var hasPayrollBlockingException = await _dbContext
                    .Set<TrxAttendanceException>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.AttendanceDailyId == daily.Id &&
                        !linkedExceptionIds.Contains(x.Id) &&
                        !x.IsDelete &&
                        x.IsActive &&
                        x.IsPayrollBlocking &&
                        x.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Closed &&
                        x.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Corrected &&
                        x.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Waived,
                        cancellationToken);

                daily.ExceptionCount = await _dbContext.Set<TrxAttendanceException>()
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.AttendanceDailyId == daily.Id &&
                        !linkedExceptionIds.Contains(x.Id) &&
                        !x.IsDelete &&
                        x.IsActive &&
                        x.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Closed &&
                        x.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Corrected,
                        cancellationToken);

                daily.PayrollInputStatus =
                    daily.IsPayrollEligible && !hasPayrollBlockingException
                        ? AttendanceValueConstants.PayrollInputStatus.Ready
                        : AttendanceValueConstants.PayrollInputStatus.Blocked;

                var previousStatus = request.RequestStatus;
                request.RequestStatus =
                    AttendanceValueConstants.CorrectionRequestStatus.Applied;
                request.AppliedAt = now;
                request.AppliedByUserId = actorUserId;
                request.FinalNote = NormalizeOptionalText(note) ?? request.FinalNote;
                request.ApprovedSummaryJson = JsonSerializer.Serialize(new
                {
                    request.Id,
                    request.RequestNumber,
                    attendanceDailyId = daily.Id,
                    daily.AttendanceDate,
                    daily.AttendanceStatus,
                    daily.FirstCheckInAt,
                    daily.LastCheckOutAt,
                    daily.ActualWorkMinutes,
                    daily.PayableWorkMinutes,
                    daily.LateMinutes,
                    daily.EarlyLeaveMinutes,
                    daily.OvertimeMinutes,
                    appliedDetailCount = appliedCount,
                    closedExceptionCount,
                    appliedAt = now,
                    appliedByUserId = actorUserId
                });
                request.UpdateDateTime = now;
                request.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return AttendanceCorrectionServiceResult<AttendanceCorrectionApplyResponse>.Ok(
                    new AttendanceCorrectionApplyResponse
                    {
                        AttendanceCorrectionRequestId = request.Id,
                        AttendanceDailyId = daily.Id,
                        PreviousRequestStatus = previousStatus,
                        CurrentRequestStatus = request.RequestStatus,
                        AttendanceStatus = daily.AttendanceStatus,
                        AppliedDetailCount = appliedCount,
                        ClosedExceptionCount = closedExceptionCount,
                        AppliedAt = now
                    },
                    "Attendance correction berhasil diterapkan ke attendance daily.");
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(
                    ex,
                    "Apply attendance correction {CorrectionRequestId} gagal karena konflik database.",
                    correctionRequestId);

                return AttendanceCorrectionServiceResult<AttendanceCorrectionApplyResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Attendance correction gagal diterapkan karena terjadi konflik data.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(
                    ex,
                    "Apply attendance correction {CorrectionRequestId} gagal.",
                    correctionRequestId);

                return AttendanceCorrectionServiceResult<AttendanceCorrectionApplyResponse>.Fail(
                    StatusCodes.Status500InternalServerError,
                    $"Attendance correction gagal diterapkan: {ex.Message}");
            }
        }

        public static string MapRequestStatus(string workflowStatus)
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
                return AttendanceValueConstants.CorrectionRequestStatus.NeedRevision;
            }

            if (string.Equals(
                    workflowStatus,
                    WorkflowValueConstants.WorkflowStatus.Rejected,
                    StringComparison.OrdinalIgnoreCase))
            {
                return AttendanceValueConstants.CorrectionRequestStatus.Rejected;
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
                return AttendanceValueConstants.CorrectionRequestStatus.Cancelled;
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
                return AttendanceValueConstants.CorrectionRequestStatus.Approved;
            }

            if (string.Equals(
                    workflowStatus,
                    WorkflowValueConstants.WorkflowStatus.Draft,
                    StringComparison.OrdinalIgnoreCase))
            {
                return AttendanceValueConstants.CorrectionRequestStatus.Draft;
            }

            if (string.Equals(
                    workflowStatus,
                    WorkflowValueConstants.WorkflowStatus.Submitted,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    workflowStatus,
                    WorkflowValueConstants.WorkflowStatus.InProgress,
                    StringComparison.OrdinalIgnoreCase))
            {
                return AttendanceValueConstants.CorrectionRequestStatus.UnderReview;
            }

            return AttendanceValueConstants.CorrectionRequestStatus.Submitted;
        }

        private static void ApplyLifecycleAudit(
            HrdAttendanceCorrectionRequest request,
            TrxWorkflowInstance workflow,
            string targetStatus,
            Guid actorUserId,
            DateTime now)
        {
            if (string.Equals(
                    targetStatus,
                    AttendanceValueConstants.CorrectionRequestStatus.Submitted,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    targetStatus,
                    AttendanceValueConstants.CorrectionRequestStatus.UnderReview,
                    StringComparison.OrdinalIgnoreCase))
            {
                request.SubmittedAt ??= workflow.SubmittedAt ?? now;
                request.ApprovedAt = null;
                request.RejectedAt = null;
                return;
            }

            if (string.Equals(
                    targetStatus,
                    AttendanceValueConstants.CorrectionRequestStatus.NeedRevision,
                    StringComparison.OrdinalIgnoreCase))
            {
                request.ApprovedAt = null;
                request.RejectedAt = null;
                return;
            }

            if (string.Equals(
                    targetStatus,
                    AttendanceValueConstants.CorrectionRequestStatus.Approved,
                    StringComparison.OrdinalIgnoreCase))
            {
                request.ApprovedAt ??= workflow.CompletedAt ?? now;
                request.RejectedAt = null;
                return;
            }

            if (string.Equals(
                    targetStatus,
                    AttendanceValueConstants.CorrectionRequestStatus.Rejected,
                    StringComparison.OrdinalIgnoreCase))
            {
                request.RejectedAt ??= now;
                request.ApprovedAt = null;
                return;
            }

            if (string.Equals(
                    targetStatus,
                    AttendanceValueConstants.CorrectionRequestStatus.Cancelled,
                    StringComparison.OrdinalIgnoreCase))
            {
                request.IsCancel = true;
                request.CancelDateTime ??=
                    workflow.CancelledAt ?? workflow.WithdrawnAt ?? now;
                request.CancelBy = actorUserId;
            }
        }

        private async Task SynchronizeDetailStatusesAsync(
            Guid requestId,
            string requestStatus,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var details = await _dbContext.Set<HrdAttendanceCorrectionDetail>()
                .Where(x =>
                    x.AttendanceCorrectionRequestId == requestId &&
                    !x.IsDelete)
                .ToListAsync(cancellationToken);

            foreach (var detail in details)
            {
                var nextStatus = requestStatus switch
                {
                    var status when string.Equals(
                        status,
                        AttendanceValueConstants.CorrectionRequestStatus.Approved,
                        StringComparison.OrdinalIgnoreCase) => "Approved",
                    var status when string.Equals(
                        status,
                        AttendanceValueConstants.CorrectionRequestStatus.Rejected,
                        StringComparison.OrdinalIgnoreCase) => "Rejected",
                    var status when string.Equals(
                        status,
                        AttendanceValueConstants.CorrectionRequestStatus.Cancelled,
                        StringComparison.OrdinalIgnoreCase) => "Rejected",
                    _ when !detail.IsApplied => "Requested",
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

        private static string? ApplyFieldValue(
            TrxAttendanceDaily daily,
            string fieldName,
            string? value)
        {
            if (!AttendanceCorrectionFieldCatalog.TryGet(fieldName, out var definition))
            {
                return $"Field koreksi {fieldName} tidak didukung.";
            }

            var validation = AttendanceCorrectionFieldCatalog.ValidateRequestedValue(
                definition,
                value);
            if (validation != null)
            {
                return validation;
            }

            var normalized = string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();

            try
            {
                switch (fieldName)
                {
                    case "FirstCheckInAt":
                        daily.FirstCheckInAt = ParseNullableDateTime(normalized);
                        break;
                    case "LastCheckOutAt":
                        daily.LastCheckOutAt = ParseNullableDateTime(normalized);
                        break;
                    case "ScheduledCheckInAt":
                        daily.ScheduledCheckInAt = ParseNullableDateTime(normalized);
                        break;
                    case "ScheduledCheckOutAt":
                        daily.ScheduledCheckOutAt = ParseNullableDateTime(normalized);
                        break;
                    case "AttendanceStatus":
                        daily.AttendanceStatus = normalized!;
                        break;
                    case "BreakMinutes":
                        daily.BreakMinutes = ParseNonNegativeInt(normalized);
                        break;
                    case "ActualWorkMinutes":
                        daily.ActualWorkMinutes = ParseNonNegativeInt(normalized);
                        break;
                    case "PayableWorkMinutes":
                        daily.PayableWorkMinutes = ParseNonNegativeInt(normalized);
                        break;
                    case "LateMinutes":
                        daily.LateMinutes = ParseNonNegativeInt(normalized);
                        break;
                    case "EarlyLeaveMinutes":
                        daily.EarlyLeaveMinutes = ParseNonNegativeInt(normalized);
                        break;
                    case "OvertimeMinutes":
                        daily.OvertimeMinutes = ParseNonNegativeInt(normalized);
                        break;
                    case "IsPresent":
                        daily.IsPresent = bool.Parse(normalized!);
                        break;
                    case "IsAbsent":
                        daily.IsAbsent = bool.Parse(normalized!);
                        break;
                    case "IsLate":
                        daily.IsLate = bool.Parse(normalized!);
                        break;
                    case "IsEarlyLeave":
                        daily.IsEarlyLeave = bool.Parse(normalized!);
                        break;
                    case "HasMissingPunch":
                        daily.HasMissingPunch = bool.Parse(normalized!);
                        break;
                    case "IsBusinessTrip":
                        daily.IsBusinessTrip = bool.Parse(normalized!);
                        break;
                    case "IsRemoteAttendance":
                        daily.IsRemoteAttendance = bool.Parse(normalized!);
                        break;
                    case "WorkScheduleId":
                        daily.WorkScheduleId = ParseNullableGuid(normalized);
                        break;
                    case "ShiftId":
                        daily.ShiftId = ParseNullableGuid(normalized);
                        break;
                    default:
                        return $"Field koreksi {fieldName} tidak didukung.";
                }
            }
            catch (Exception ex) when (
                ex is FormatException ||
                ex is OverflowException)
            {
                return $"Nilai field {definition.Label} tidak valid.";
            }

            return null;
        }

        private static void RecalculateDaily(
            TrxAttendanceDaily daily,
            HashSet<string> correctedFields)
        {
            if (!correctedFields.Contains("ActualWorkMinutes"))
            {
                var grossMinutes = daily.FirstCheckInAt.HasValue &&
                                   daily.LastCheckOutAt.HasValue &&
                                   daily.LastCheckOutAt > daily.FirstCheckInAt
                    ? (int)Math.Floor(
                        (daily.LastCheckOutAt.Value - daily.FirstCheckInAt.Value)
                        .TotalMinutes)
                    : 0;

                daily.ActualWorkMinutes = Math.Max(
                    0,
                    grossMinutes - Math.Max(0, daily.BreakMinutes));
            }

            if (!correctedFields.Contains("PayableWorkMinutes"))
            {
                var maximumWorkMinutes = daily.AttendancePolicy?.MaximumWorkMinutes ?? 0;
                daily.PayableWorkMinutes = maximumWorkMinutes > 0
                    ? Math.Min(daily.ActualWorkMinutes, maximumWorkMinutes)
                    : daily.ActualWorkMinutes;
            }

            if (!correctedFields.Contains("LateMinutes"))
            {
                daily.LateMinutes = daily.FirstCheckInAt.HasValue &&
                                    daily.ScheduledCheckInAt.HasValue &&
                                    daily.FirstCheckInAt > daily.ScheduledCheckInAt
                    ? Math.Max(
                        0,
                        (int)Math.Floor(
                            (daily.FirstCheckInAt.Value - daily.ScheduledCheckInAt.Value)
                            .TotalMinutes))
                    : 0;
            }

            if (!correctedFields.Contains("EarlyLeaveMinutes"))
            {
                daily.EarlyLeaveMinutes = daily.LastCheckOutAt.HasValue &&
                                          daily.ScheduledCheckOutAt.HasValue &&
                                          daily.LastCheckOutAt < daily.ScheduledCheckOutAt
                    ? Math.Max(
                        0,
                        (int)Math.Floor(
                            (daily.ScheduledCheckOutAt.Value - daily.LastCheckOutAt.Value)
                            .TotalMinutes))
                    : 0;
            }

            if (!correctedFields.Contains("OvertimeMinutes"))
            {
                var policy = daily.AttendancePolicy;
                daily.OvertimeMinutes = policy?.IsOvertimeEnabled == true
                    ? Math.Max(
                        0,
                        daily.ActualWorkMinutes -
                        daily.ScheduledWorkMinutes -
                        Math.Max(0, policy.OvertimeThresholdMinutes))
                    : 0;
            }

            if (!correctedFields.Contains("IsPresent"))
            {
                daily.IsPresent =
                    daily.FirstCheckInAt.HasValue ||
                    daily.LastCheckOutAt.HasValue ||
                    daily.IsBusinessTrip ||
                    daily.IsRemoteAttendance;
            }

            if (!correctedFields.Contains("IsAbsent"))
            {
                daily.IsAbsent =
                    !daily.IsPresent &&
                    !daily.IsHoliday &&
                    !daily.IsRestDay;
            }

            if (!correctedFields.Contains("HasMissingPunch"))
            {
                daily.HasMissingPunch =
                    daily.FirstCheckInAt.HasValue != daily.LastCheckOutAt.HasValue;
            }

            if (!correctedFields.Contains("IsLate"))
            {
                daily.IsLate = daily.LateMinutes > 0;
            }

            if (!correctedFields.Contains("IsEarlyLeave"))
            {
                daily.IsEarlyLeave = daily.EarlyLeaveMinutes > 0;
            }

            if (!correctedFields.Contains("AttendanceStatus"))
            {
                daily.AttendanceStatus = ResolveAttendanceStatus(daily);
            }
        }

        private static string ResolveAttendanceStatus(TrxAttendanceDaily daily)
        {
            if (daily.IsBusinessTrip)
                return AttendanceValueConstants.AttendanceStatus.BusinessTrip;
            if (daily.IsRemoteAttendance)
                return AttendanceValueConstants.AttendanceStatus.Remote;
            if (daily.IsHoliday)
                return AttendanceValueConstants.AttendanceStatus.Holiday;
            if (daily.IsRestDay)
                return AttendanceValueConstants.AttendanceStatus.RestDay;
            if (daily.IsAbsent)
                return AttendanceValueConstants.AttendanceStatus.Absent;
            if (daily.HasMissingPunch)
                return AttendanceValueConstants.AttendanceStatus.Incomplete;
            if (daily.IsLate)
                return AttendanceValueConstants.AttendanceStatus.Late;
            if (daily.IsEarlyLeave)
                return AttendanceValueConstants.AttendanceStatus.EarlyLeave;
            if (daily.IsPresent)
                return AttendanceValueConstants.AttendanceStatus.Present;
            return AttendanceValueConstants.AttendanceStatus.Unprocessed;
        }

        private static AttendanceCorrectionApplyResponse BuildAlreadyAppliedResponse(
            HrdAttendanceCorrectionRequest request)
        {
            return new AttendanceCorrectionApplyResponse
            {
                AttendanceCorrectionRequestId = request.Id,
                AttendanceDailyId = request.AttendanceDailyId ?? Guid.Empty,
                PreviousRequestStatus = request.RequestStatus,
                CurrentRequestStatus = request.RequestStatus,
                AttendanceStatus = request.AttendanceDaily?.AttendanceStatus ?? string.Empty,
                AppliedDetailCount = request.Details.Count(x => !x.IsDelete && x.IsApplied),
                ClosedExceptionCount = request.Exceptions.Count(x =>
                    !x.IsDelete &&
                    (x.ExceptionStatus == AttendanceValueConstants.AttendanceExceptionStatus.Closed ||
                     x.ExceptionStatus == AttendanceValueConstants.AttendanceExceptionStatus.Corrected)),
                AppliedAt = request.AppliedAt ?? request.UpdateDateTime ?? DateTime.UtcNow
            };
        }

        private static DateTime? ParseNullableDateTime(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return DateTimeOffset.Parse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal)
                .UtcDateTime;
        }

        private static Guid? ParseNullableGuid(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            return Guid.Parse(value);
        }

        private static int ParseNonNegativeInt(string? value)
        {
            var parsed = int.Parse(
                value ?? "0",
                NumberStyles.Integer,
                CultureInfo.InvariantCulture);
            return Math.Max(0, parsed);
        }

        private static string? NormalizeOptionalText(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
