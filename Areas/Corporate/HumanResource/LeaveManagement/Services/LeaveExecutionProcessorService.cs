using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Repositories;
using System.Data;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    public class LeaveExecutionProcessorService
    {
        private const decimal Tolerance = 0.0001m;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        private static readonly string[] ExecutableRequestStatuses =
        {
            LeaveRequestValueConstants.Status.Approved,
            LeaveRequestValueConstants.Status.Taken,
            LeaveRequestValueConstants.Status.Completed
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly AttendanceScheduleResolverService _scheduleResolver;
        private readonly AttendanceProcessingService _attendanceProcessingService;
        private readonly LeaveExecutionBalanceService _balanceService;
        private readonly ILogger<LeaveExecutionProcessorService> _logger;

        public LeaveExecutionProcessorService(
            ApplicationDbContext dbContext,
            AttendanceScheduleResolverService scheduleResolver,
            AttendanceProcessingService attendanceProcessingService,
            LeaveExecutionBalanceService balanceService,
            ILogger<LeaveExecutionProcessorService> logger)
        {
            _dbContext = dbContext;
            _scheduleResolver = scheduleResolver;
            _attendanceProcessingService = attendanceProcessingService;
            _balanceService = balanceService;
            _logger = logger;
        }

        public async Task<LeaveRequestServiceResult<LeaveExecutionActionResponse>> ExecuteAsync(
            Guid leaveRequestId,
            ExecuteLeaveRequestRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var asOfDate = request.AsOfDate ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);
            var ensure = await EnsureExecutionAsync(
                leaveRequestId,
                actorUserId,
                request.CorrelationId,
                request.Notes,
                cancellationToken);

            if (!ensure.Success || ensure.Data == null)
            {
                return ensure;
            }

            var execution = await LoadExecutionAsync(ensure.Data.LeaveExecutionId!.Value, cancellationToken);
            if (execution == null || execution.LeaveRequest == null)
            {
                return LeaveRequestServiceResult<LeaveExecutionActionResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Leave execution tidak ditemukan setelah preparation.");
            }

            var leaveRequest = execution.LeaveRequest;
            execution.LastAttemptAt = DateTime.UtcNow;
            execution.CorrelationId = NullIfWhiteSpace(request.CorrelationId) ?? execution.CorrelationId;
            execution.Notes = AppendNote(execution.Notes, request.Notes);
            execution.UpdateDateTime = DateTime.UtcNow;
            execution.UpdateBy = actorUserId;

            var processed = 0;
            var errors = new List<string>();

            if (asOfDate >= leaveRequest.StartDate &&
                execution.ExecutionStatus != LeaveExecutionValueConstants.ExecutionStatus.Completed &&
                execution.ExecutionStatus != LeaveExecutionValueConstants.ExecutionStatus.Reversed)
            {
                if (leaveRequest.LeavePolicy != null &&
                    string.Equals(
                        leaveRequest.LeavePolicy.DeductionTiming,
                        LeaveValueConstants.DeductionTiming.OnLeaveStart,
                        StringComparison.OrdinalIgnoreCase))
                {
                    var balanceResult = await _balanceService.ApplyDeductionStageAsync(
                        leaveRequest.Id,
                        LeaveExecutionValueConstants.BalanceStage.OnLeaveStart,
                        actorUserId,
                        cancellationToken);

                    execution.BalanceExecutionStatus = balanceResult.Success
                        ? "Applied"
                        : "Failed";

                    if (!balanceResult.Success)
                    {
                        errors.Add(balanceResult.Message);
                    }
                }

                var dueIntegrations = execution.AttendanceIntegrations
                    .Where(x =>
                        x.LeaveDate <= asOfDate &&
                        (x.IntegrationStatus == LeaveExecutionValueConstants.AttendanceIntegrationStatus.Pending ||
                         x.IntegrationStatus == LeaveExecutionValueConstants.AttendanceIntegrationStatus.Failed ||
                         (request.ForceRetry && x.IntegrationStatus == LeaveExecutionValueConstants.AttendanceIntegrationStatus.Conflict)))
                    .OrderBy(x => x.LeaveDate)
                    .ToList();

                foreach (var integration in dueIntegrations)
                {
                    var result = await ApplyAttendanceAsync(
                        integration.Id,
                        actorUserId,
                        request.ForceRetry,
                        cancellationToken);

                    processed += 1;
                    if (!result.Success)
                    {
                        errors.Add($"{integration.LeaveDate:yyyy-MM-dd}: {result.Message}");
                    }
                }

                var trackedRequest = await _dbContext.Set<WfpLeaveRequest>()
                    .FirstAsync(x => x.Id == leaveRequest.Id, cancellationToken);

                if (trackedRequest.LeaveRequestStatus == LeaveRequestValueConstants.Status.Approved)
                {
                    trackedRequest.LeaveRequestStatus = LeaveRequestValueConstants.Status.Taken;
                    trackedRequest.TakenAt ??= DateTime.UtcNow;
                    trackedRequest.UpdateDateTime = DateTime.UtcNow;
                    trackedRequest.UpdateBy = actorUserId;
                }

                execution.ExecutionStatus = LeaveExecutionValueConstants.ExecutionStatus.Active;
                execution.StartedAt ??= DateTime.UtcNow;
                execution.StartedByUserId ??= actorUserId == Guid.Empty ? null : actorUserId;
            }

            await RefreshExecutionCountsAsync(execution, cancellationToken);

            if (asOfDate > leaveRequest.EndDate &&
                execution.ExecutionStatus != LeaveExecutionValueConstants.ExecutionStatus.Reversed)
            {
                var remaining = execution.AttendanceIntegrations
                    .Where(x =>
                        x.IntegrationStatus != LeaveExecutionValueConstants.AttendanceIntegrationStatus.Applied &&
                        x.IntegrationStatus != LeaveExecutionValueConstants.AttendanceIntegrationStatus.Skipped &&
                        x.IntegrationStatus != LeaveExecutionValueConstants.AttendanceIntegrationStatus.Reversed)
                    .OrderBy(x => x.LeaveDate)
                    .ToList();

                foreach (var integration in remaining)
                {
                    var result = await ApplyAttendanceAsync(
                        integration.Id,
                        actorUserId,
                        request.ForceRetry,
                        cancellationToken);

                    processed += 1;
                    if (!result.Success)
                    {
                        errors.Add($"{integration.LeaveDate:yyyy-MM-dd}: {result.Message}");
                    }
                }

                await RefreshExecutionCountsAsync(execution, cancellationToken);

                if (execution.ConflictAttendanceDayCount == 0 &&
                    execution.FailedAttendanceDayCount == 0 &&
                    execution.AppliedAttendanceDayCount == execution.ExpectedAttendanceDayCount)
                {
                    if (leaveRequest.LeavePolicy != null &&
                        string.Equals(
                            leaveRequest.LeavePolicy.DeductionTiming,
                            LeaveValueConstants.DeductionTiming.OnCompletion,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        var balanceResult = await _balanceService.ApplyDeductionStageAsync(
                            leaveRequest.Id,
                            LeaveExecutionValueConstants.BalanceStage.OnCompletion,
                            actorUserId,
                            cancellationToken);

                        execution.BalanceExecutionStatus = balanceResult.Success
                            ? "Applied"
                            : "Failed";

                        if (!balanceResult.Success)
                        {
                            errors.Add(balanceResult.Message);
                        }
                    }

                    if (errors.Count == 0)
                    {
                        var trackedRequest = await _dbContext.Set<WfpLeaveRequest>()
                            .FirstAsync(x => x.Id == leaveRequest.Id, cancellationToken);
                        trackedRequest.LeaveRequestStatus = LeaveRequestValueConstants.Status.Completed;
                        trackedRequest.CompletedAt ??= DateTime.UtcNow;
                        trackedRequest.UpdateDateTime = DateTime.UtcNow;
                        trackedRequest.UpdateBy = actorUserId;

                        execution.ExecutionStatus = LeaveExecutionValueConstants.ExecutionStatus.Completed;
                        execution.CompletedAt ??= DateTime.UtcNow;
                        execution.CompletedByUserId ??= actorUserId == Guid.Empty ? null : actorUserId;
                    }
                }
            }

            await RefreshExecutionCountsAsync(execution, cancellationToken);

            execution.ExecutionStatus = errors.Count > 0 &&
                                        execution.ExecutionStatus != LeaveExecutionValueConstants.ExecutionStatus.Completed
                ? LeaveExecutionValueConstants.ExecutionStatus.Failed
                : execution.ExecutionStatus;

            execution.ErrorSummary = errors.Count == 0
                ? null
                : string.Join(" | ", errors.Take(20));

            execution.ResultSnapshotJson = JsonSerializer.Serialize(new
            {
                asOfDate,
                processed,
                execution.ExpectedAttendanceDayCount,
                execution.AppliedAttendanceDayCount,
                execution.ConflictAttendanceDayCount,
                execution.FailedAttendanceDayCount,
                execution.ExecutionStatus,
                execution.AttendanceIntegrationStatus,
                execution.BalanceExecutionStatus,
                errors
            }, JsonOptions);

            execution.UpdateDateTime = DateTime.UtcNow;
            execution.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return errors.Count == 0
                ? LeaveRequestServiceResult<LeaveExecutionActionResponse>.Ok(
                    MapAction(execution, leaveRequest, processed, false, "Leave execution berhasil diproses."),
                    "Leave execution berhasil diproses.")
                : LeaveRequestServiceResult<LeaveExecutionActionResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Leave execution selesai dengan masalah: {execution.ErrorSummary}");
        }

        public async Task<LeaveRequestServiceResult<LeaveExecutionBatchResponse>> ProcessDueAsync(
            ProcessDueLeaveRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            request.MaximumItem = Math.Clamp(request.MaximumItem, 1, 1000);
            var asOfDate = request.AsOfDate ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);

            var query = _dbContext.Set<WfpLeaveRequest>()
                .AsNoTracking()
                .Where(x =>
                    (x.LeaveRequestStatus == LeaveRequestValueConstants.Status.Approved ||
                     x.LeaveRequestStatus == LeaveRequestValueConstants.Status.Taken) &&
                    x.StartDate <= asOfDate &&
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel);

            if (request.WorkforceProfileId.HasValue)
            {
                query = query.Where(x => x.WorkforceProfileId == request.WorkforceProfileId.Value);
            }
            if (request.HospitalSiteId.HasValue)
            {
                query = query.Where(x => x.HospitalSiteId == request.HospitalSiteId.Value);
            }
            if (request.OrganizationUnitId.HasValue)
            {
                query = query.Where(x => x.OrganizationUnitId == request.OrganizationUnitId.Value);
            }
            if (request.DepartmentId.HasValue)
            {
                query = query.Where(x => x.DepartmentId == request.DepartmentId.Value);
            }

            var rows = await query
                .OrderBy(x => x.StartDate)
                .ThenBy(x => x.RequestNumber)
                .Take(request.MaximumItem)
                .Select(x => new { x.Id, x.RequestNumber })
                .ToListAsync(cancellationToken);

            var response = new LeaveExecutionBatchResponse
            {
                AsOfDate = asOfDate,
                TotalItem = rows.Count
            };

            foreach (var row in rows)
            {
                var result = await ExecuteAsync(
                    row.Id,
                    new ExecuteLeaveRequestRequest
                    {
                        AsOfDate = asOfDate,
                        ForceRetry = request.ForceRetry,
                        CorrelationId = request.CorrelationId,
                        Notes = request.Notes
                    },
                    actorUserId,
                    cancellationToken);

                var item = new LeaveExecutionBatchItemResponse
                {
                    LeaveRequestId = row.Id,
                    RequestNumber = row.RequestNumber,
                    Success = result.Success,
                    StatusCode = result.StatusCode,
                    Message = result.Message,
                    ExecutionStatus = result.Data?.ExecutionStatus
                };
                response.Items.Add(item);

                if (result.Success)
                {
                    response.SuccessCount += 1;
                }
                else if (result.StatusCode == StatusCodes.Status409Conflict)
                {
                    response.SkippedCount += 1;
                }
                else
                {
                    response.FailedCount += 1;
                }
            }

            return LeaveRequestServiceResult<LeaveExecutionBatchResponse>.Ok(
                response,
                "Pemrosesan leave execution due selesai.");
        }

        public async Task<LeaveRequestServiceResult<LeaveExecutionActionResponse>> ReverseAsync(
            Guid leaveRequestId,
            ReverseLeaveExecutionRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var execution = await _dbContext.Set<TrxLeaveExecution>()
                .Include(x => x.LeaveRequest)
                    .ThenInclude(x => x!.LeavePolicy)
                .Include(x => x.AttendanceIntegrations)
                .FirstOrDefaultAsync(x => x.LeaveRequestId == leaveRequestId && !x.IsDelete, cancellationToken);

            if (execution?.LeaveRequest == null)
            {
                return LeaveRequestServiceResult<LeaveExecutionActionResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Leave execution tidak ditemukan.");
            }

            if (execution.ExecutionStatus == LeaveExecutionValueConstants.ExecutionStatus.Reversed)
            {
                return LeaveRequestServiceResult<LeaveExecutionActionResponse>.Ok(
                    MapAction(execution, execution.LeaveRequest, 0, true, "Leave execution sudah direversal."),
                    "Leave execution sudah direversal.");
            }

            var effectiveDate = request.EffectiveDate ?? execution.StartDate;
            var targets = execution.AttendanceIntegrations
                .Where(x =>
                    x.LeaveDate >= effectiveDate &&
                    x.IntegrationStatus == LeaveExecutionValueConstants.AttendanceIntegrationStatus.Applied)
                .OrderByDescending(x => x.LeaveDate)
                .ToList();

            var errors = new List<string>();
            foreach (var integration in targets)
            {
                var result = await ReverseAttendanceAsync(integration.Id, actorUserId, request.Reason, cancellationToken);
                if (!result.Success)
                {
                    errors.Add($"{integration.LeaveDate:yyyy-MM-dd}: {result.Message}");
                }
            }

            var restoreDays = request.RestoreDays ?? targets.Sum(x => x.RequestedLeaveDays);
            var restoreResult = await _balanceService.RestoreAsync(
                leaveRequestId,
                restoreDays,
                actorUserId,
                request.Reason,
                $"EXECUTION-{effectiveDate:yyyyMMdd}",
                cancellationToken);

            if (!restoreResult.Success)
            {
                errors.Add(restoreResult.Message);
            }

            if (errors.Count > 0)
            {
                execution.ExecutionStatus = LeaveExecutionValueConstants.ExecutionStatus.Failed;
                execution.ErrorSummary = string.Join(" | ", errors.Take(20));
                execution.RetryCount += 1;
                execution.LastAttemptAt = DateTime.UtcNow;
                execution.UpdateDateTime = DateTime.UtcNow;
                execution.UpdateBy = actorUserId;
                await _dbContext.SaveChangesAsync(cancellationToken);

                return LeaveRequestServiceResult<LeaveExecutionActionResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    execution.ErrorSummary);
            }

            var trackedRequest = execution.LeaveRequest;
            var isFullReversal = effectiveDate <= execution.StartDate;
            var restoredDays = request.RestoreDays ?? targets.Sum(x => x.RequestedLeaveDays);
            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

            if (isFullReversal)
            {
                trackedRequest.LeaveRequestStatus = LeaveRequestValueConstants.Status.Cancelled;
                trackedRequest.CancelledAt ??= DateTime.UtcNow;
                trackedRequest.CancelledByUserId ??= actorUserId == Guid.Empty ? null : actorUserId;
                execution.ExecutionStatus = LeaveExecutionValueConstants.ExecutionStatus.Reversed;
                execution.AttendanceIntegrationStatus = LeaveExecutionValueConstants.AttendanceIntegrationStatus.Reversed;
                execution.BalanceExecutionStatus = "Reversed";
                execution.ReversedAt = DateTime.UtcNow;
                execution.ReversedByUserId = actorUserId == Guid.Empty ? null : actorUserId;
            }
            else
            {
                var revisedEndDate = effectiveDate.AddDays(-1);
                trackedRequest.EndDate = revisedEndDate;
                trackedRequest.RequestedDays = Math.Max(0, trackedRequest.RequestedDays - restoredDays);
                trackedRequest.CalculatedWorkingDays = Math.Max(0, trackedRequest.CalculatedWorkingDays - restoredDays);
                trackedRequest.EstimatedBalanceDeduction = Math.Max(0, trackedRequest.EstimatedBalanceDeduction - restoredDays);
                trackedRequest.EstimatedBalanceAfterRequest = trackedRequest.BalanceBeforeRequest - trackedRequest.EstimatedBalanceDeduction;
                trackedRequest.LeaveRequestStatus = revisedEndDate < today
                    ? LeaveRequestValueConstants.Status.Completed
                    : LeaveRequestValueConstants.Status.Taken;
                trackedRequest.CompletedAt = revisedEndDate < today
                    ? DateTime.UtcNow
                    : trackedRequest.CompletedAt;

                execution.EndDate = revisedEndDate;
                execution.RequestedDays = trackedRequest.RequestedDays;
                execution.ExecutedDays = execution.AttendanceIntegrations
                    .Where(x => x.IntegrationStatus == LeaveExecutionValueConstants.AttendanceIntegrationStatus.Applied)
                    .Sum(x => x.RequestedLeaveDays);
                execution.ExecutionStatus = revisedEndDate < today
                    ? LeaveExecutionValueConstants.ExecutionStatus.Completed
                    : LeaveExecutionValueConstants.ExecutionStatus.Active;
                execution.AttendanceIntegrationStatus = execution.AttendanceIntegrations.Any(x =>
                    x.IntegrationStatus == LeaveExecutionValueConstants.AttendanceIntegrationStatus.Conflict)
                    ? LeaveExecutionValueConstants.AttendanceIntegrationStatus.Conflict
                    : execution.AttendanceIntegrations.Any(x =>
                        x.IntegrationStatus == LeaveExecutionValueConstants.AttendanceIntegrationStatus.Failed)
                        ? LeaveExecutionValueConstants.AttendanceIntegrationStatus.Failed
                        : LeaveExecutionValueConstants.AttendanceIntegrationStatus.Applied;
                execution.BalanceExecutionStatus = "RestoredPartially";
            }

            trackedRequest.ApprovalNotes = request.Reason;
            trackedRequest.UpdateDateTime = DateTime.UtcNow;
            trackedRequest.UpdateBy = actorUserId;
            execution.ErrorSummary = null;
            execution.Notes = AppendNote(execution.Notes, request.Reason);
            execution.UpdateDateTime = DateTime.UtcNow;
            execution.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            var message = isFullReversal
                ? "Leave execution berhasil direversal."
                : "Leave execution berhasil dibatalkan sebagian dan periode leave diperpendek.";

            return LeaveRequestServiceResult<LeaveExecutionActionResponse>.Ok(
                MapAction(execution, trackedRequest, targets.Count, false, message),
                message);
        }

        public async Task<LeaveRequestServiceResult<LeaveExecutionActionResponse>> ApplyApprovedCancellationAsync(
            Guid cancellationRequestId,
            ApplyLeaveCancellationRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var cancellation = await _dbContext.Set<TrxLeaveCancellationRequest>()
                .Include(x => x.LeaveRequest)
                .FirstOrDefaultAsync(x => x.Id == cancellationRequestId && !x.IsDelete, cancellationToken);

            if (cancellation?.LeaveRequest == null)
            {
                return LeaveRequestServiceResult<LeaveExecutionActionResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Leave cancellation request tidak ditemukan.");
            }

            if (!string.Equals(cancellation.CancellationStatus, "Approved", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(cancellation.CancellationStatus, "Applied", StringComparison.OrdinalIgnoreCase))
            {
                return LeaveRequestServiceResult<LeaveExecutionActionResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Hanya leave cancellation berstatus Approved yang dapat diterapkan.");
            }

            if (string.Equals(cancellation.CancellationStatus, "Applied", StringComparison.OrdinalIgnoreCase))
            {
                var existingExecution = await _dbContext.Set<TrxLeaveExecution>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.LeaveRequestId == cancellation.LeaveRequestId && !x.IsDelete, cancellationToken);

                return LeaveRequestServiceResult<LeaveExecutionActionResponse>.Ok(
                    new LeaveExecutionActionResponse
                    {
                        LeaveRequestId = cancellation.LeaveRequestId,
                        LeaveExecutionId = existingExecution?.Id,
                        RequestNumber = cancellation.LeaveRequest.RequestNumber,
                        LeaveRequestStatus = cancellation.LeaveRequest.LeaveRequestStatus,
                        ExecutionStatus = existingExecution?.ExecutionStatus,
                        IsIdempotent = true,
                        Message = "Leave cancellation sudah diterapkan."
                    },
                    "Leave cancellation sudah diterapkan.");
            }

            var effectiveDate = cancellation.EffectiveCancellationDate ?? cancellation.LeaveRequest.StartDate;
            var reverse = await ReverseAsync(
                cancellation.LeaveRequestId,
                new ReverseLeaveExecutionRequest
                {
                    Reason = cancellation.CancellationReason,
                    EffectiveDate = effectiveDate,
                    RestoreDays = cancellation.RestoredDays > 0 ? cancellation.RestoredDays : null
                },
                actorUserId,
                cancellationToken);

            if (!reverse.Success)
            {
                return reverse;
            }

            cancellation.CancellationStatus = "Applied";
            cancellation.AppliedAt = DateTime.UtcNow;
            cancellation.ApprovalNotes = AppendNote(cancellation.ApprovalNotes, request.Notes);
            cancellation.UpdateDateTime = DateTime.UtcNow;
            cancellation.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return reverse;
        }

        private async Task<LeaveRequestServiceResult<LeaveExecutionActionResponse>> EnsureExecutionAsync(
            Guid leaveRequestId,
            Guid actorUserId,
            string? correlationId,
            string? notes,
            CancellationToken cancellationToken)
        {
            var existing = await _dbContext.Set<TrxLeaveExecution>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.LeaveRequestId == leaveRequestId && !x.IsDelete, cancellationToken);

            if (existing != null)
            {
                var request = await _dbContext.Set<WfpLeaveRequest>()
                    .AsNoTracking()
                    .FirstAsync(x => x.Id == leaveRequestId, cancellationToken);

                return LeaveRequestServiceResult<LeaveExecutionActionResponse>.Ok(
                    MapAction(existing, request, 0, true, "Leave execution sudah tersedia."),
                    "Leave execution sudah tersedia.");
            }

            var leaveRequest = await _dbContext.Set<WfpLeaveRequest>()
                .AsNoTracking()
                .Include(x => x.LeaveType)
                .Include(x => x.LeavePolicy)
                .FirstOrDefaultAsync(x => x.Id == leaveRequestId && !x.IsDelete, cancellationToken);

            if (leaveRequest == null)
            {
                return LeaveRequestServiceResult<LeaveExecutionActionResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan cuti tidak ditemukan.");
            }

            if (!ExecutableRequestStatuses.Contains(leaveRequest.LeaveRequestStatus))
            {
                return LeaveRequestServiceResult<LeaveExecutionActionResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Leave execution hanya dapat dibuat untuk pengajuan Approved, Taken, atau Completed.");
            }

            if (leaveRequest.LeaveType == null)
            {
                return LeaveRequestServiceResult<LeaveExecutionActionResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Jenis cuti pengajuan tidak tersedia.");
            }

            var daysResult = await ResolveExecutionDaysAsync(leaveRequest, cancellationToken);
            if (!daysResult.Success || daysResult.Data == null)
            {
                return LeaveRequestServiceResult<LeaveExecutionActionResponse>.Fail(
                    daysResult.StatusCode,
                    daysResult.Message);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                existing = await _dbContext.Set<TrxLeaveExecution>()
                    .FirstOrDefaultAsync(x => x.LeaveRequestId == leaveRequestId && !x.IsDelete, cancellationToken);

                if (existing != null)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return LeaveRequestServiceResult<LeaveExecutionActionResponse>.Ok(
                        MapAction(existing, leaveRequest, 0, true, "Leave execution sudah tersedia."),
                        "Leave execution sudah tersedia.");
                }

                var now = DateTime.UtcNow;
                var execution = new TrxLeaveExecution
                {
                    Id = Guid.NewGuid(),
                    ExecutionNumber = GenerateExecutionNumber(),
                    LeaveRequestId = leaveRequest.Id,
                    WorkforceProfileId = leaveRequest.WorkforceProfileId,
                    LeaveTypeId = leaveRequest.LeaveTypeId,
                    LeaveBalanceId = leaveRequest.LeaveBalanceId,
                    StartDate = leaveRequest.StartDate,
                    EndDate = leaveRequest.EndDate,
                    RequestedDays = leaveRequest.RequestedDays,
                    ExecutionStatus = LeaveExecutionValueConstants.ExecutionStatus.Scheduled,
                    AttendanceIntegrationStatus = LeaveExecutionValueConstants.AttendanceIntegrationStatus.Pending,
                    BalanceExecutionStatus = ResolveInitialBalanceStatus(leaveRequest),
                    ExpectedAttendanceDayCount = daysResult.Data.Count,
                    TotalScheduledMinutes = daysResult.Data.Sum(x => x.PlannedWorkMinutes),
                    TotalPayableLeaveMinutes = daysResult.Data.Sum(x => CalculatePayableMinutes(x, leaveRequest.LeaveType.IsPaidLeave)),
                    CorrelationId = NullIfWhiteSpace(correlationId),
                    IdempotencyKey = $"LEAVE-EXECUTION:{leaveRequest.Id:N}",
                    ExecutionSnapshotJson = JsonSerializer.Serialize(new
                    {
                        leaveRequest.Id,
                        leaveRequest.RequestNumber,
                        leaveRequest.WorkforceProfileId,
                        leaveRequest.LeaveTypeId,
                        leaveRequest.StartDate,
                        leaveRequest.EndDate,
                        leaveRequest.RequestedDays,
                        leaveRequest.IsHalfDay,
                        leaveRequest.IsHourly,
                        leaveRequest.RequestedMinutes,
                        leaveRequest.LeavePolicyId,
                        leaveRequest.LeavePolicy?.ReservationTiming,
                        leaveRequest.LeavePolicy?.DeductionTiming,
                        days = daysResult.Data
                    }, JsonOptions),
                    Notes = NullIfWhiteSpace(notes),
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };

                _dbContext.Add(execution);

                foreach (var day in daysResult.Data)
                {
                    _dbContext.Add(new TrxLeaveAttendanceIntegration
                    {
                        Id = Guid.NewGuid(),
                        LeaveExecutionId = execution.Id,
                        LeaveRequestId = leaveRequest.Id,
                        WorkforceProfileId = leaveRequest.WorkforceProfileId,
                        LeaveTypeId = leaveRequest.LeaveTypeId,
                        LeaveDate = day.Date,
                        RequestedLeaveDays = day.CountedDays,
                        RequestedMinutes = leaveRequest.IsHourly ? leaveRequest.RequestedMinutes : null,
                        IsHalfDay = leaveRequest.IsHalfDay,
                        IsHourly = leaveRequest.IsHourly,
                        IsPaidLeave = leaveRequest.LeaveType.IsPaidLeave,
                        ScheduledMinutes = Math.Max(0, day.PlannedWorkMinutes),
                        PayableLeaveMinutes = CalculatePayableMinutes(day, leaveRequest.LeaveType.IsPaidLeave),
                        IntegrationStatus = LeaveExecutionValueConstants.AttendanceIntegrationStatus.Pending,
                        IdempotencyKey = $"LEAVE-ATTENDANCE:{leaveRequest.Id:N}:{day.Date:yyyyMMdd}",
                        ScheduleSnapshotJson = JsonSerializer.Serialize(day, JsonOptions),
                        IsActive = true,
                        CreateDateTime = now,
                        CreateBy = actorUserId
                    });
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return LeaveRequestServiceResult<LeaveExecutionActionResponse>.Ok(
                    MapAction(execution, leaveRequest, 0, false, "Leave execution berhasil disiapkan."),
                    "Leave execution berhasil disiapkan.",
                    StatusCodes.Status201Created);
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync(cancellationToken);
                var concurrent = await _dbContext.Set<TrxLeaveExecution>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.LeaveRequestId == leaveRequestId && !x.IsDelete, cancellationToken);

                if (concurrent != null)
                {
                    return LeaveRequestServiceResult<LeaveExecutionActionResponse>.Ok(
                        MapAction(concurrent, leaveRequest, 0, true, "Leave execution sudah dibuat oleh proses lain."),
                        "Leave execution sudah dibuat oleh proses lain.");
                }

                return LeaveRequestServiceResult<LeaveExecutionActionResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Pembuatan leave execution gagal karena konflik transaksi.");
            }
        }

        private async Task<LeaveRequestServiceResult<bool>> ApplyAttendanceAsync(
            Guid integrationId,
            Guid actorUserId,
            bool forceRetry,
            CancellationToken cancellationToken)
        {
            var integrationSnapshot = await _dbContext.Set<TrxLeaveAttendanceIntegration>()
                .AsNoTracking()
                .Include(x => x.LeaveRequest)
                    .ThenInclude(x => x!.LeaveType)
                .FirstOrDefaultAsync(x => x.Id == integrationId && !x.IsDelete, cancellationToken);

            if (integrationSnapshot?.LeaveRequest?.LeaveType == null)
            {
                return LeaveRequestServiceResult<bool>.Fail(
                    StatusCodes.Status404NotFound,
                    "Leave attendance integration tidak ditemukan.");
            }

            if (integrationSnapshot.IntegrationStatus == LeaveExecutionValueConstants.AttendanceIntegrationStatus.Applied)
            {
                return LeaveRequestServiceResult<bool>.Ok(true, "Attendance leave sudah diterapkan.");
            }

            if (integrationSnapshot.IntegrationStatus == LeaveExecutionValueConstants.AttendanceIntegrationStatus.Conflict && !forceRetry)
            {
                return LeaveRequestServiceResult<bool>.Fail(
                    StatusCodes.Status409Conflict,
                    integrationSnapshot.ErrorMessage ?? "Attendance integration masih memiliki conflict.");
            }

            var scheduleResult = await _scheduleResolver.ResolveAsync(
                integrationSnapshot.WorkforceProfileId,
                integrationSnapshot.LeaveDate,
                cancellationToken);

            if (!scheduleResult.Success || scheduleResult.Data == null)
            {
                await MarkIntegrationFailedAsync(integrationId, scheduleResult.Message, actorUserId, cancellationToken);
                return LeaveRequestServiceResult<bool>.Fail(scheduleResult.StatusCode, scheduleResult.Message);
            }

            var schedule = scheduleResult.Data;
            if (!schedule.IsResolved || schedule.HasBlockingConflict)
            {
                var message = schedule.HasBlockingConflict
                    ? $"Jadwal memiliki blocking conflict: {string.Join(", ", schedule.ConflictCodes)}"
                    : "Jadwal employee belum dapat diselesaikan.";
                await MarkIntegrationConflictAsync(integrationId, message, actorUserId, cancellationToken);
                return LeaveRequestServiceResult<bool>.Fail(StatusCodes.Status409Conflict, message);
            }

            var user = await _dbContext.Users
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == integrationSnapshot.WorkforceProfileId && x.IsActive)
                .Select(x => new
                {
                    x.Id,
                    x.EmployeeId,
                    x.DoctorId,
                    x.UserType
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null)
            {
                await MarkIntegrationFailedAsync(integrationId, "Workforce belum mempunyai user account aktif.", actorUserId, cancellationToken);
                return LeaveRequestServiceResult<bool>.Fail(
                    StatusCodes.Status409Conflict,
                    "Workforce belum mempunyai user account aktif.");
            }

            await using IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                var integration = await _dbContext.Set<TrxLeaveAttendanceIntegration>()
                    .Include(x => x.LeaveRequest)
                        .ThenInclude(x => x!.LeaveType)
                    .FirstAsync(x => x.Id == integrationId, cancellationToken);

                if (integration.IntegrationStatus == LeaveExecutionValueConstants.AttendanceIntegrationStatus.Applied)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return LeaveRequestServiceResult<bool>.Ok(true, "Attendance leave sudah diterapkan.");
                }

                var daily = await _dbContext.Set<TrxAttendanceDaily>()
                    .Include(x => x.Segments)
                    .Include(x => x.Exceptions)
                    .Include(x => x.RawLogs)
                    .FirstOrDefaultAsync(x =>
                        x.UserId == user.Id &&
                        x.AttendanceDate == integration.LeaveDate &&
                        !x.IsDelete,
                        cancellationToken);

                var isCreated = daily == null;
                if (daily == null)
                {
                    daily = new TrxAttendanceDaily
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        WorkforceProfileId = integration.WorkforceProfileId,
                        EmployeeId = user.EmployeeId,
                        DoctorId = user.DoctorId,
                        UserType = user.UserType,
                        AttendanceDate = integration.LeaveDate,
                        AttendanceStatus = AttendanceValueConstants.AttendanceStatus.Unprocessed,
                        ProcessingStatus = AttendanceValueConstants.AttendanceProcessingStatus.Pending,
                        PayrollInputStatus = AttendanceValueConstants.PayrollInputStatus.Pending,
                        ScheduleSource = schedule.ScheduleSource,
                        IsActive = true,
                        CreateDateTime = DateTime.UtcNow,
                        CreateBy = actorUserId
                    };
                    _dbContext.Add(daily);
                }

                if (daily.IsLocked ||
                    daily.AttendancePeriodId.HasValue ||
                    daily.PayrollInputStatus == AttendanceValueConstants.PayrollInputStatus.Processed)
                {
                    integration.IntegrationStatus = LeaveExecutionValueConstants.AttendanceIntegrationStatus.Conflict;
                    integration.ErrorMessage = "Attendance daily sudah dikunci atau diproses ke payroll.";
                    integration.UpdateDateTime = DateTime.UtcNow;
                    integration.UpdateBy = actorUserId;
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    return LeaveRequestServiceResult<bool>.Fail(StatusCodes.Status409Conflict, integration.ErrorMessage);
                }

                var fullDay = integration.RequestedLeaveDays >= 0.999m && !integration.IsHourly;
                var hasActualAttendance = daily.RawLogs.Any(x => !x.IsDelete) ||
                                          daily.FirstCheckInAt.HasValue ||
                                          daily.LastCheckOutAt.HasValue ||
                                          daily.IsPresent ||
                                          daily.ActualWorkMinutes > 0;

                if (fullDay && hasActualAttendance)
                {
                    integration.IntegrationStatus = LeaveExecutionValueConstants.AttendanceIntegrationStatus.Conflict;
                    integration.AttendanceDailyId = daily.Id;
                    integration.AttendanceStatusBefore = daily.AttendanceStatus;
                    integration.ProcessingStatusBefore = daily.ProcessingStatus;
                    integration.ErrorMessage = "Attendance aktual sudah tersedia pada tanggal cuti penuh. Diperlukan review admin.";
                    integration.UpdateDateTime = DateTime.UtcNow;
                    integration.UpdateBy = actorUserId;
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    return LeaveRequestServiceResult<bool>.Fail(StatusCodes.Status409Conflict, integration.ErrorMessage);
                }

                integration.AttendanceStatusBefore ??= daily.AttendanceStatus;
                integration.ProcessingStatusBefore ??= daily.ProcessingStatus;

                daily.WorkforceProfileId = integration.WorkforceProfileId;
                daily.EmployeeId = user.EmployeeId;
                daily.DoctorId = user.DoctorId;
                daily.UserType = user.UserType;
                daily.HospitalSiteId = schedule.HospitalSiteId;
                daily.OrganizationUnitId = schedule.OrganizationUnitId;
                daily.DepartmentId = schedule.DepartmentId;
                daily.WorkLocationId = schedule.WorkLocationId;
                daily.WorkScheduleId = schedule.WorkScheduleId;
                daily.WorkScheduleAssignmentId = schedule.WorkScheduleAssignmentId;
                daily.PrimaryShiftAssignmentId = schedule.PrimaryShiftAssignmentId;
                daily.ShiftId = schedule.ShiftId;
                daily.AttendancePolicyId = schedule.AttendancePolicyId;
                daily.GracePeriodPolicyId = schedule.GracePeriodPolicyId;
                daily.ScheduleSource = schedule.ScheduleSource;
                daily.ScheduleResolutionJson = schedule.ResolutionSnapshotJson;
                daily.ScheduledCheckInAt = schedule.ScheduledStartAt;
                daily.ScheduledCheckOutAt = schedule.ScheduledEndAt;
                daily.IsOvernightSchedule = schedule.IsOvernight;
                daily.IsHoliday = schedule.IsHoliday;
                daily.IsRestDay = schedule.IsRestDay;
                daily.ScheduledWorkMinutes = Math.Max(0, schedule.PlannedWorkMinutes);

                var leaveSegmentMarker = $"LeaveRequestId={integration.LeaveRequestId:N}";
                var segment = daily.Segments.FirstOrDefault(x =>
                    x.SegmentSource == LeaveExecutionValueConstants.AttendanceSegment.Source &&
                    x.Notes != null &&
                    x.Notes.Contains(leaveSegmentMarker) &&
                    !x.IsDelete);

                if (segment == null)
                {
                    segment = new TrxAttendanceDailySegment
                    {
                        Id = Guid.NewGuid(),
                        AttendanceDailyId = daily.Id,
                        SegmentOrder = daily.Segments.Where(x => !x.IsDelete).Select(x => x.SegmentOrder).DefaultIfEmpty(0).Max() + 1,
                        SegmentType = LeaveExecutionValueConstants.AttendanceSegment.Type,
                        SegmentSource = LeaveExecutionValueConstants.AttendanceSegment.Source,
                        IsActive = true,
                        CreateDateTime = DateTime.UtcNow,
                        CreateBy = actorUserId
                    };
                    daily.Segments.Add(segment);
                }

                segment.ScheduledStartAt = schedule.ScheduledStartAt;
                segment.ScheduledEndAt = schedule.ScheduledEndAt;
                segment.ScheduledMinutes = integration.ScheduledMinutes;
                segment.ActualMinutes = integration.PayableLeaveMinutes;
                segment.PayableMinutes = integration.PayableLeaveMinutes;
                segment.IsOvernight = schedule.IsOvernight;
                segment.SegmentStatus = AttendanceValueConstants.AttendanceSegmentStatus.Calculated;
                segment.Notes = $"{leaveSegmentMarker}; RequestNumber={integration.LeaveRequest!.RequestNumber}; LeaveDays={integration.RequestedLeaveDays:0.####}";
                segment.UpdateDateTime = DateTime.UtcNow;
                segment.UpdateBy = actorUserId;

                if (fullDay)
                {
                    daily.AttendanceStatus = AttendanceValueConstants.AttendanceStatus.Leave;
                    daily.FirstCheckInAt = null;
                    daily.LastCheckOutAt = null;
                    daily.IsPresent = false;
                    daily.IsAbsent = false;
                    daily.IsLate = false;
                    daily.IsEarlyLeave = false;
                    daily.HasMissingPunch = false;
                    daily.ActualWorkMinutes = 0;
                    daily.BreakMinutes = 0;
                    daily.LateMinutes = 0;
                    daily.EarlyLeaveMinutes = 0;
                    daily.OvertimeMinutes = 0;
                    daily.PayableWorkMinutes = integration.PayableLeaveMinutes;

                    foreach (var exception in daily.Exceptions.Where(x =>
                                 x.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Closed &&
                                 x.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Corrected &&
                                 !x.IsDelete))
                    {
                        exception.ExceptionStatus = AttendanceValueConstants.AttendanceExceptionStatus.Waived;
                        exception.IsPayrollBlocking = false;
                        exception.ResolvedAt = DateTime.UtcNow;
                        exception.ResolvedByUserId = actorUserId == Guid.Empty ? null : actorUserId;
                        exception.ResolutionNote = $"Di-waive oleh approved leave request {integration.LeaveRequest.RequestNumber}.";
                        exception.UpdateDateTime = DateTime.UtcNow;
                        exception.UpdateBy = actorUserId;
                    }
                }
                else
                {
                    daily.AttendanceStatus = hasActualAttendance
                        ? AttendanceValueConstants.AttendanceStatus.Present
                        : AttendanceValueConstants.AttendanceStatus.Leave;
                    daily.PayableWorkMinutes = Math.Min(
                        Math.Max(daily.ScheduledWorkMinutes, integration.PayableLeaveMinutes),
                        Math.Max(0, daily.ActualWorkMinutes) + integration.PayableLeaveMinutes);
                }

                daily.ExceptionCount = daily.Exceptions.Count(x =>
                    !x.IsDelete &&
                    x.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Closed &&
                    x.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Corrected &&
                    x.ExceptionStatus != AttendanceValueConstants.AttendanceExceptionStatus.Waived);
                daily.IsPayrollEligible = daily.Exceptions.All(x => x.IsDelete || !x.IsPayrollBlocking);
                daily.PayrollInputStatus = daily.IsPayrollEligible
                    ? AttendanceValueConstants.PayrollInputStatus.Ready
                    : AttendanceValueConstants.PayrollInputStatus.Blocked;
                daily.ProcessingStatus = AttendanceValueConstants.AttendanceProcessingStatus.Processed;
                daily.ProcessedAt = DateTime.UtcNow;
                daily.ProcessingVersion = Math.Max(1, daily.ProcessingVersion + (isCreated ? 0 : 1));
                daily.ProcessingMessage = $"Attendance ditetapkan sebagai leave berdasarkan {integration.LeaveRequest.RequestNumber}.";
                daily.UpdateDateTime = DateTime.UtcNow;
                daily.UpdateBy = actorUserId;

                integration.AttendanceDailyId = daily.Id;
                integration.IntegrationStatus = LeaveExecutionValueConstants.AttendanceIntegrationStatus.Applied;
                integration.AttendanceStatusAfter = daily.AttendanceStatus;
                integration.ProcessingStatusAfter = daily.ProcessingStatus;
                integration.AppliedAt = DateTime.UtcNow;
                integration.AppliedByUserId = actorUserId == Guid.Empty ? null : actorUserId;
                integration.ScheduleSnapshotJson = schedule.ResolutionSnapshotJson;
                integration.ResultSnapshotJson = JsonSerializer.Serialize(new
                {
                    daily.Id,
                    daily.AttendanceStatus,
                    daily.ProcessingStatus,
                    daily.ScheduledWorkMinutes,
                    daily.PayableWorkMinutes,
                    integration.RequestedLeaveDays,
                    integration.PayableLeaveMinutes,
                    isCreated
                }, JsonOptions);
                integration.ErrorMessage = null;
                integration.UpdateDateTime = DateTime.UtcNow;
                integration.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return LeaveRequestServiceResult<bool>.Ok(true, "Attendance leave berhasil diterapkan.");
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync(cancellationToken);
                await MarkIntegrationFailedAsync(integrationId, "Attendance integration gagal karena konflik database.", actorUserId, cancellationToken);
                return LeaveRequestServiceResult<bool>.Fail(
                    StatusCodes.Status409Conflict,
                    "Attendance integration gagal karena konflik database.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                await MarkIntegrationFailedAsync(integrationId, ex.Message, actorUserId, cancellationToken);
                _logger.LogError(ex, "Leave attendance integration failed. IntegrationId={IntegrationId}", integrationId);
                return LeaveRequestServiceResult<bool>.Fail(
                    StatusCodes.Status500InternalServerError,
                    $"Attendance integration gagal: {ex.Message}");
            }
        }

        private async Task<LeaveRequestServiceResult<bool>> ReverseAttendanceAsync(
            Guid integrationId,
            Guid actorUserId,
            string reason,
            CancellationToken cancellationToken)
        {
            var integration = await _dbContext.Set<TrxLeaveAttendanceIntegration>()
                .Include(x => x.LeaveRequest)
                .FirstOrDefaultAsync(x => x.Id == integrationId && !x.IsDelete, cancellationToken);

            if (integration == null)
            {
                return LeaveRequestServiceResult<bool>.Fail(StatusCodes.Status404NotFound, "Leave attendance integration tidak ditemukan.");
            }

            if (integration.IntegrationStatus == LeaveExecutionValueConstants.AttendanceIntegrationStatus.Reversed)
            {
                return LeaveRequestServiceResult<bool>.Ok(true, "Leave attendance integration sudah direversal.");
            }

            if (!integration.AttendanceDailyId.HasValue)
            {
                integration.IntegrationStatus = LeaveExecutionValueConstants.AttendanceIntegrationStatus.Reversed;
                integration.ReversedAt = DateTime.UtcNow;
                integration.ReversedByUserId = actorUserId == Guid.Empty ? null : actorUserId;
                integration.UpdateDateTime = DateTime.UtcNow;
                integration.UpdateBy = actorUserId;
                await _dbContext.SaveChangesAsync(cancellationToken);
                return LeaveRequestServiceResult<bool>.Ok(true, "Integration tanpa attendance daily ditandai reversed.");
            }

            var daily = await _dbContext.Set<TrxAttendanceDaily>()
                .Include(x => x.Segments)
                .FirstOrDefaultAsync(x => x.Id == integration.AttendanceDailyId.Value && !x.IsDelete, cancellationToken);

            if (daily == null)
            {
                integration.IntegrationStatus = LeaveExecutionValueConstants.AttendanceIntegrationStatus.Reversed;
                integration.ReversedAt = DateTime.UtcNow;
                integration.ReversedByUserId = actorUserId == Guid.Empty ? null : actorUserId;
                integration.ErrorMessage = "Attendance daily sudah tidak ditemukan saat reversal.";
                await _dbContext.SaveChangesAsync(cancellationToken);
                return LeaveRequestServiceResult<bool>.Ok(true, "Attendance daily tidak ditemukan; integration ditandai reversed.");
            }

            if (daily.IsLocked ||
                daily.AttendancePeriodId.HasValue ||
                daily.PayrollInputStatus == AttendanceValueConstants.PayrollInputStatus.Processed)
            {
                return LeaveRequestServiceResult<bool>.Fail(
                    StatusCodes.Status409Conflict,
                    "Attendance daily sudah dikunci atau diproses ke payroll sehingga tidak dapat direversal.");
            }

            var marker = $"LeaveRequestId={integration.LeaveRequestId:N}";
            foreach (var segment in daily.Segments.Where(x =>
                         x.SegmentSource == LeaveExecutionValueConstants.AttendanceSegment.Source &&
                         x.Notes != null &&
                         x.Notes.Contains(marker) &&
                         !x.IsDelete))
            {
                segment.IsActive = false;
                segment.IsDelete = true;
                segment.DeleteDateTime = DateTime.UtcNow;
                segment.DeleteBy = actorUserId;
                segment.UpdateDateTime = DateTime.UtcNow;
                segment.UpdateBy = actorUserId;
            }

            daily.AttendanceStatus = AttendanceValueConstants.AttendanceStatus.Unprocessed;
            daily.ProcessingStatus = AttendanceValueConstants.AttendanceProcessingStatus.ReprocessRequired;
            daily.PayrollInputStatus = AttendanceValueConstants.PayrollInputStatus.Pending;
            daily.IsPayrollEligible = true;
            daily.ProcessingMessage = $"Leave integration direversal: {reason}";
            daily.UpdateDateTime = DateTime.UtcNow;
            daily.UpdateBy = actorUserId;

            integration.IntegrationStatus = LeaveExecutionValueConstants.AttendanceIntegrationStatus.Reversed;
            integration.ReversedAt = DateTime.UtcNow;
            integration.ReversedByUserId = actorUserId == Guid.Empty ? null : actorUserId;
            integration.ResultSnapshotJson = JsonSerializer.Serialize(new
            {
                reversedAt = DateTime.UtcNow,
                reason,
                attendanceDailyId = daily.Id
            }, JsonOptions);
            integration.UpdateDateTime = DateTime.UtcNow;
            integration.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
            if (daily.AttendanceDate <= today)
            {
                var reprocess = await _attendanceProcessingService.ReprocessDailyAsync(
                    daily.Id,
                    new ReprocessAttendanceDailyRequest
                    {
                        CorrelationId = $"LEAVE-REVERSAL-{integration.Id:N}",
                        Reason = reason
                    },
                    actorUserId,
                    cancellationToken);

                if (!reprocess.Success)
                {
                    return LeaveRequestServiceResult<bool>.Fail(reprocess.StatusCode, reprocess.Message);
                }
            }

            return LeaveRequestServiceResult<bool>.Ok(true, "Leave attendance integration berhasil direversal.");
        }

        private async Task<LeaveRequestServiceResult<List<LeaveRequestCalculationDayResponse>>> ResolveExecutionDaysAsync(
            WfpLeaveRequest leaveRequest,
            CancellationToken cancellationToken)
        {
            var parsed = new List<LeaveRequestCalculationDayResponse>();
            if (!string.IsNullOrWhiteSpace(leaveRequest.RosterImpactJson))
            {
                try
                {
                    parsed = JsonSerializer.Deserialize<List<LeaveRequestCalculationDayResponse>>(
                                 leaveRequest.RosterImpactJson,
                                 JsonOptions) ?? new();
                }
                catch
                {
                    parsed = new();
                }
            }

            parsed = parsed
                .Where(x => x.IsCounted && x.CountedDays > Tolerance)
                .OrderBy(x => x.Date)
                .ToList();

            if (parsed.Count > 0)
            {
                return LeaveRequestServiceResult<List<LeaveRequestCalculationDayResponse>>.Ok(
                    parsed,
                    "Hari leave execution diambil dari calculation snapshot.");
            }

            var result = new List<LeaveRequestCalculationDayResponse>();
            for (var date = leaveRequest.StartDate; date <= leaveRequest.EndDate; date = date.AddDays(1))
            {
                var scheduleResult = await _scheduleResolver.ResolveAsync(
                    leaveRequest.WorkforceProfileId,
                    date,
                    cancellationToken);

                if (!scheduleResult.Success || scheduleResult.Data == null)
                {
                    return LeaveRequestServiceResult<List<LeaveRequestCalculationDayResponse>>.Fail(
                        scheduleResult.StatusCode,
                        scheduleResult.Message);
                }

                var schedule = scheduleResult.Data;
                var counted = leaveRequest.LeavePolicy?.DayCalculationMethod == LeaveValueConstants.DayCalculationMethod.CalendarDays ||
                              (schedule.IsResolved &&
                               !schedule.HasBlockingConflict &&
                               !(leaveRequest.LeavePolicy?.ExcludeHoliday == true && schedule.IsHoliday) &&
                               !(leaveRequest.LeavePolicy?.ExcludeWeeklyOff == true && schedule.IsRestDay));

                if (!counted)
                {
                    continue;
                }

                var countedDays = 1m;
                if (leaveRequest.IsHalfDay && leaveRequest.StartDate == leaveRequest.EndDate)
                {
                    countedDays = 0.5m;
                }
                else if (leaveRequest.IsHourly && leaveRequest.RequestedMinutes.HasValue)
                {
                    countedDays = Math.Round(
                        leaveRequest.RequestedMinutes.Value / (decimal)Math.Max(1, schedule.PlannedWorkMinutes),
                        4,
                        MidpointRounding.AwayFromZero);
                }

                result.Add(new LeaveRequestCalculationDayResponse
                {
                    Date = date,
                    IsResolved = schedule.IsResolved,
                    IsCounted = true,
                    IsRestDay = schedule.IsRestDay,
                    IsHoliday = schedule.IsHoliday,
                    HasBlockingConflict = schedule.HasBlockingConflict,
                    ScheduleSource = schedule.ScheduleSource,
                    ShiftCode = schedule.ShiftCode,
                    ShiftName = schedule.ShiftName,
                    ScheduledStartAt = schedule.ScheduledStartAt,
                    ScheduledEndAt = schedule.ScheduledEndAt,
                    PlannedWorkMinutes = schedule.PlannedWorkMinutes,
                    CountedDays = countedDays,
                    HolidayNames = schedule.Holidays.Select(x => x.HolidayName).ToList(),
                    Warnings = schedule.Warnings.ToList(),
                    ConflictCodes = schedule.ConflictCodes.ToList()
                });
            }

            if (result.Count == 0)
            {
                return LeaveRequestServiceResult<List<LeaveRequestCalculationDayResponse>>.Fail(
                    StatusCodes.Status409Conflict,
                    "Tidak ada hari leave execution yang dapat dihitung.");
            }

            return LeaveRequestServiceResult<List<LeaveRequestCalculationDayResponse>>.Ok(
                result,
                "Hari leave execution berhasil dihitung ulang.");
        }

        private async Task<TrxLeaveExecution?> LoadExecutionAsync(Guid executionId, CancellationToken cancellationToken)
        {
            return await _dbContext.Set<TrxLeaveExecution>()
                .Include(x => x.LeaveRequest)
                    .ThenInclude(x => x!.LeavePolicy)
                .Include(x => x.LeaveRequest)
                    .ThenInclude(x => x!.LeaveType)
                .Include(x => x.AttendanceIntegrations)
                .FirstOrDefaultAsync(x => x.Id == executionId && !x.IsDelete, cancellationToken);
        }

        private async Task RefreshExecutionCountsAsync(
            TrxLeaveExecution execution,
            CancellationToken cancellationToken)
        {
            var rows = await _dbContext.Set<TrxLeaveAttendanceIntegration>()
                .AsNoTracking()
                .Where(x => x.LeaveExecutionId == execution.Id && !x.IsDelete)
                .ToListAsync(cancellationToken);

            execution.ExpectedAttendanceDayCount = rows.Count;
            execution.AppliedAttendanceDayCount = rows.Count(x => x.IntegrationStatus == LeaveExecutionValueConstants.AttendanceIntegrationStatus.Applied);
            execution.ConflictAttendanceDayCount = rows.Count(x => x.IntegrationStatus == LeaveExecutionValueConstants.AttendanceIntegrationStatus.Conflict);
            execution.FailedAttendanceDayCount = rows.Count(x => x.IntegrationStatus == LeaveExecutionValueConstants.AttendanceIntegrationStatus.Failed);
            execution.ExecutedDays = rows
                .Where(x => x.IntegrationStatus == LeaveExecutionValueConstants.AttendanceIntegrationStatus.Applied)
                .Sum(x => x.RequestedLeaveDays);

            execution.AttendanceIntegrationStatus = execution.ExpectedAttendanceDayCount == 0
                ? LeaveExecutionValueConstants.AttendanceIntegrationStatus.Skipped
                : execution.ConflictAttendanceDayCount > 0
                    ? LeaveExecutionValueConstants.AttendanceIntegrationStatus.Conflict
                    : execution.FailedAttendanceDayCount > 0
                        ? LeaveExecutionValueConstants.AttendanceIntegrationStatus.Failed
                        : execution.AppliedAttendanceDayCount == execution.ExpectedAttendanceDayCount
                            ? LeaveExecutionValueConstants.AttendanceIntegrationStatus.Applied
                            : LeaveExecutionValueConstants.AttendanceIntegrationStatus.Pending;
        }

        private async Task MarkIntegrationFailedAsync(
            Guid integrationId,
            string message,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var integration = await _dbContext.Set<TrxLeaveAttendanceIntegration>()
                .FirstOrDefaultAsync(x => x.Id == integrationId, cancellationToken);
            if (integration == null)
            {
                return;
            }
            integration.IntegrationStatus = LeaveExecutionValueConstants.AttendanceIntegrationStatus.Failed;
            integration.ErrorMessage = message;
            integration.UpdateDateTime = DateTime.UtcNow;
            integration.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private async Task MarkIntegrationConflictAsync(
            Guid integrationId,
            string message,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var integration = await _dbContext.Set<TrxLeaveAttendanceIntegration>()
                .FirstOrDefaultAsync(x => x.Id == integrationId, cancellationToken);
            if (integration == null)
            {
                return;
            }
            integration.IntegrationStatus = LeaveExecutionValueConstants.AttendanceIntegrationStatus.Conflict;
            integration.ErrorMessage = message;
            integration.UpdateDateTime = DateTime.UtcNow;
            integration.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private static int CalculatePayableMinutes(LeaveRequestCalculationDayResponse day, bool isPaidLeave)
        {
            if (!isPaidLeave)
            {
                return 0;
            }
            return (int)Math.Round(
                Math.Max(0, day.PlannedWorkMinutes) * Math.Max(0, day.CountedDays),
                MidpointRounding.AwayFromZero);
        }

        private static string ResolveInitialBalanceStatus(WfpLeaveRequest request)
        {
            if (!request.LeaveBalanceId.HasValue || request.LeaveType?.IsBalanceDeducted != true)
            {
                return "NotRequired";
            }
            if (request.LeavePolicy?.DeductionTiming == LeaveValueConstants.DeductionTiming.OnApproval &&
                request.ActualBalanceDeduction >= request.EstimatedBalanceDeduction)
            {
                return "Applied";
            }
            return "Pending";
        }

        private static LeaveExecutionActionResponse MapAction(
            TrxLeaveExecution execution,
            WfpLeaveRequest request,
            int processed,
            bool idempotent,
            string message)
        {
            return new LeaveExecutionActionResponse
            {
                LeaveRequestId = request.Id,
                LeaveExecutionId = execution.Id,
                RequestNumber = request.RequestNumber,
                LeaveRequestStatus = request.LeaveRequestStatus,
                ExecutionStatus = execution.ExecutionStatus,
                AttendanceIntegrationStatus = execution.AttendanceIntegrationStatus,
                BalanceExecutionStatus = execution.BalanceExecutionStatus,
                ProcessedDayCount = processed,
                AppliedDayCount = execution.AppliedAttendanceDayCount,
                ConflictDayCount = execution.ConflictAttendanceDayCount,
                FailedDayCount = execution.FailedAttendanceDayCount,
                IsIdempotent = idempotent,
                Message = message
            };
        }

        private static string GenerateExecutionNumber()
        {
            return $"LEX-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
        }

        private static string? NullIfWhiteSpace(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string? AppendNote(string? existing, string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return existing;
            return string.IsNullOrWhiteSpace(existing)
                ? value.Trim()
                : $"{existing}\n[{DateTime.UtcNow:O}] {value.Trim()}";
        }
    }
}
