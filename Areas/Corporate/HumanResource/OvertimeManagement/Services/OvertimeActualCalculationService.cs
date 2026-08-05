using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Repositories;
using System.Data;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public class OvertimeActualCalculationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly OvertimeAttendanceMatchingService _matchingService;
        private readonly OvertimePeriodGuardService _periodGuard;
        private readonly ILogger<OvertimeActualCalculationService> _logger;

        public OvertimeActualCalculationService(
            ApplicationDbContext dbContext,
            OvertimeAttendanceMatchingService matchingService,
            OvertimePeriodGuardService periodGuard,
            ILogger<OvertimeActualCalculationService> logger)
        {
            _dbContext = dbContext;
            _matchingService = matchingService;
            _periodGuard = periodGuard;
            _logger = logger;
        }

        public Task<OvertimeRealizationServiceResult<OvertimeRealizationPreviewResponse>> PreviewAsync(
            Guid overtimeRequestId,
            PreviewOvertimeRealizationRequest? request,
            CancellationToken cancellationToken = default) =>
            _matchingService.PreviewAsync(
                overtimeRequestId,
                request,
                cancellationToken);

        public async Task<OvertimeRealizationServiceResult<OvertimeRealizationMutationResponse>> CalculateAsync(
            Guid overtimeRequestId,
            CalculateOvertimeRealizationRequest? request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            request ??= new CalculateOvertimeRealizationRequest();

            if (actorUserId == Guid.Empty)
            {
                return OvertimeRealizationServiceResult<OvertimeRealizationMutationResponse>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Identitas user login tidak valid.");
            }

            var previewResult = await _matchingService.PreviewAsync(
                overtimeRequestId,
                new PreviewOvertimeRealizationRequest
                {
                    AllowUnprocessedAttendance = false,
                    IncludeRateBreakdown = true
                },
                cancellationToken);

            if (!previewResult.Success || previewResult.Data == null)
            {
                return OvertimeRealizationServiceResult<OvertimeRealizationMutationResponse>.Fail(
                    previewResult.StatusCode,
                    previewResult.Message);
            }

            var preview = previewResult.Data;
            if (!preview.CanCalculate)
            {
                var blockingMessages = preview.Issues
                    .Where(x => x.IsBlocking)
                    .Select(x => x.Message)
                    .Distinct()
                    .ToList();

                return OvertimeRealizationServiceResult<OvertimeRealizationMutationResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    blockingMessages.Count == 0
                        ? "Attendance matching belum siap untuk calculation."
                        : string.Join(" ", blockingMessages));
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                var overtimeRequest = await _dbContext.WfpOvertimeRequests
                    .Include(x => x.OvertimePolicy)
                    .Include(x => x.Details.Where(d => !d.IsDelete && !d.IsCancel && d.IsActive))
                    .FirstOrDefaultAsync(
                        x => x.Id == overtimeRequestId && !x.IsDelete,
                        cancellationToken);

                if (overtimeRequest == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return OvertimeRealizationServiceResult<OvertimeRealizationMutationResponse>.Fail(
                        StatusCodes.Status404NotFound,
                        "Overtime request tidak ditemukan.");
                }

                var periodGuard = await CheckPeriodAsync(overtimeRequest, cancellationToken);
                if (!periodGuard.IsWritable)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return OvertimeRealizationServiceResult<OvertimeRealizationMutationResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        periodGuard.Message);
                }

                var existingRealizations = await _dbContext.TrxOvertimeRealizations
                    .Include(x => x.Details.Where(d => !d.IsDelete))
                    .Where(x => x.OvertimeRequestId == overtimeRequestId && !x.IsDelete)
                    .OrderByDescending(x => x.RealizationVersion)
                    .ToListAsync(cancellationToken);

                var latest = existingRealizations.FirstOrDefault();
                var latestFingerprint = ExtractFingerprint(latest?.CalculationResultJson);
                var hasActiveLatest = latest != null &&
                    latest.IsActive &&
                    !latest.IsCancel &&
                    !string.Equals(
                        latest.RealizationStatus,
                        OvertimeValueConstants.RealizationStatus.Cancelled,
                        StringComparison.OrdinalIgnoreCase);

                if (hasActiveLatest &&
                    string.Equals(latestFingerprint, preview.InputFingerprint, StringComparison.OrdinalIgnoreCase))
                {
                    await transaction.CommitAsync(cancellationToken);
                    return OvertimeRealizationServiceResult<OvertimeRealizationMutationResponse>.Ok(
                        MapMutation(latest!, overtimeRequest, latestFingerprint, true),
                        "Calculation dengan attendance dan policy yang sama sudah tersedia; hasil existing dikembalikan secara idempotent.");
                }

                if (hasActiveLatest && IsFinalOrPosted(latest!.RealizationStatus))
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return OvertimeRealizationServiceResult<OvertimeRealizationMutationResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        "Realization yang sudah verified atau posted ke payroll tidak dapat dihitung ulang pada Tahap 4E.");
                }

                if (hasActiveLatest && !request.ForceNewVersion)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return OvertimeRealizationServiceResult<OvertimeRealizationMutationResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        "Attendance, policy, atau approval snapshot berubah. Gunakan endpoint recalculate untuk membuat realization version baru.");
                }

                var now = DateTime.UtcNow;
                var version = existingRealizations.Count == 0
                    ? 1
                    : existingRealizations.Max(x => x.RealizationVersion) + 1;

                if (hasActiveLatest)
                {
                    latest!.IsActive = false;
                    latest.UpdateDateTime = now;
                    latest.UpdateBy = actorUserId;

                    foreach (var previousDetail in latest.Details)
                    {
                        previousDetail.IsActive = false;
                        previousDetail.UpdateDateTime = now;
                        previousDetail.UpdateBy = actorUserId;
                    }
                }

                var targetStatus = ResolveTargetStatus(
                    request.SubmitForVerification,
                    preview.RequirePostVerification);

                var actualStartAt = preview.Details
                    .SelectMany(x => x.MatchedIntervals)
                    .Select(x => (DateTime?)x.MatchedStartAt)
                    .DefaultIfEmpty()
                    .Min() ?? overtimeRequest.PlannedStartAt ?? now;

                var actualEndAt = preview.Details
                    .SelectMany(x => x.MatchedIntervals)
                    .Select(x => (DateTime?)x.MatchedEndAt)
                    .DefaultIfEmpty()
                    .Max() ?? overtimeRequest.PlannedEndAt ?? actualStartAt;

                var distinctAttendanceDailyIds = preview.Details
                    .SelectMany(x => x.AttendanceEvidence)
                    .Select(x => x.AttendanceDailyId)
                    .Distinct()
                    .ToList();

                var realization = new TrxOvertimeRealization
                {
                    Id = Guid.NewGuid(),
                    RealizationNumber = await GenerateRealizationNumberAsync(
                        DateOnly.FromDateTime(actualStartAt),
                        cancellationToken),
                    OvertimeRequestId = overtimeRequest.Id,
                    WorkforceProfileId = overtimeRequest.WorkforceProfileId,
                    EmployeeId = overtimeRequest.EmployeeId,
                    OrganizationAssignmentId = overtimeRequest.OrganizationAssignmentId,
                    HospitalSiteId = overtimeRequest.HospitalSiteId,
                    OrganizationUnitId = overtimeRequest.OrganizationUnitId,
                    DepartmentId = overtimeRequest.DepartmentId,
                    PositionId = overtimeRequest.PositionId,
                    CostCenterId = overtimeRequest.CostCenterId,
                    AttendanceDailyId = distinctAttendanceDailyIds.Count == 1
                        ? distinctAttendanceDailyIds[0]
                        : null,
                    RealizationVersion = version,
                    ActualStartDate = DateOnly.FromDateTime(actualStartAt),
                    ActualEndDate = DateOnly.FromDateTime(actualEndAt),
                    ActualStartAt = actualStartAt,
                    ActualEndAt = actualEndAt,
                    RequestedMinutesSnapshot = preview.RequestedMinutes,
                    ApprovedMinutesSnapshot = preview.ApprovedMinutes,
                    ActualMinutes = preview.ActualMinutes,
                    ActualBreakMinutes = preview.BreakMinutes,
                    EligibleMinutes = preview.EligibleMinutes,
                    VerifiedMinutes = targetStatus == OvertimeValueConstants.RealizationStatus.Verified
                        ? preview.EligibleMinutes
                        : 0,
                    PostedMinutes = 0,
                    VarianceMinutes = preview.VarianceMinutes,
                    CalculatedAmount = 0,
                    VerifiedAmount = 0,
                    PostedAmount = 0,
                    CurrencyCode = overtimeRequest.CurrencyCode,
                    RealizationNotes = NormalizeText(request.Notes),
                    EvidenceSummaryJson = BuildEvidenceSummaryJson(preview),
                    CalculationResultJson = BuildCalculationResultJson(
                        preview,
                        request,
                        version),
                    RealizationStatus = targetStatus,
                    SubmittedAt = targetStatus == OvertimeValueConstants.RealizationStatus.WaitingVerification ||
                                  targetStatus == OvertimeValueConstants.RealizationStatus.Verified
                        ? now
                        : null,
                    SubmittedByUserId = targetStatus == OvertimeValueConstants.RealizationStatus.WaitingVerification ||
                                        targetStatus == OvertimeValueConstants.RealizationStatus.Verified
                        ? actorUserId
                        : null,
                    VerifiedAt = targetStatus == OvertimeValueConstants.RealizationStatus.Verified
                        ? now
                        : null,
                    VerifiedByUserId = targetStatus == OvertimeValueConstants.RealizationStatus.Verified
                        ? actorUserId
                        : null,
                    IsPayrollPosted = false,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    UpdateBy = actorUserId
                };

                _dbContext.TrxOvertimeRealizations.Add(realization);
                BuildRealizationDetails(
                    realization,
                    overtimeRequest,
                    preview,
                    targetStatus,
                    actorUserId,
                    now);

                ApplyRequestLifecycle(
                    overtimeRequest,
                    preview,
                    targetStatus,
                    actorUserId,
                    now);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation(
                    "Overtime realization {RealizationNumber} version {Version} dibuat untuk request {OvertimeRequestId}. Status={Status}, EligibleMinutes={EligibleMinutes}.",
                    realization.RealizationNumber,
                    realization.RealizationVersion,
                    overtimeRequest.Id,
                    realization.RealizationStatus,
                    realization.EligibleMinutes);

                return OvertimeRealizationServiceResult<OvertimeRealizationMutationResponse>.Ok(
                    MapMutation(
                        realization,
                        overtimeRequest,
                        preview.InputFingerprint,
                        false),
                    version == 1
                        ? "Actual overtime calculation berhasil dibuat."
                        : "Actual overtime calculation version baru berhasil dibuat.",
                    StatusCodes.Status201Created);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<OvertimeRealizationServiceResult<OvertimeRealizationMutationResponse>> SubmitForVerificationAsync(
            Guid realizationId,
            SubmitOvertimeRealizationRequest? request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty)
            {
                return OvertimeRealizationServiceResult<OvertimeRealizationMutationResponse>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Identitas user login tidak valid.");
            }

            var realization = await _dbContext.TrxOvertimeRealizations
                .Include(x => x.OvertimeRequest)
                    .ThenInclude(x => x.OvertimePolicy)
                .Include(x => x.Details.Where(d => !d.IsDelete))
                .FirstOrDefaultAsync(
                    x => x.Id == realizationId && !x.IsDelete && !x.IsCancel && x.IsActive,
                    cancellationToken);

            if (realization?.OvertimeRequest == null)
            {
                return OvertimeRealizationServiceResult<OvertimeRealizationMutationResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Overtime realization tidak ditemukan.");
            }

            var periodGuard = await CheckPeriodAsync(realization.OvertimeRequest, cancellationToken);
            if (!periodGuard.IsWritable)
            {
                return OvertimeRealizationServiceResult<OvertimeRealizationMutationResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    periodGuard.Message);
            }

            if (!string.Equals(realization.RealizationStatus, OvertimeValueConstants.RealizationStatus.Draft, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(realization.RealizationStatus, OvertimeValueConstants.RealizationStatus.NeedRevision, StringComparison.OrdinalIgnoreCase))
            {
                return OvertimeRealizationServiceResult<OvertimeRealizationMutationResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Hanya realization berstatus Draft atau NeedRevision yang dapat dikirim ke verifikasi.");
            }

            var now = DateTime.UtcNow;
            var requireVerification = realization.OvertimeRequest.OvertimePolicy?.RequirePostVerification ?? true;
            var targetStatus = requireVerification
                ? OvertimeValueConstants.RealizationStatus.WaitingVerification
                : OvertimeValueConstants.RealizationStatus.Verified;

            realization.RealizationStatus = targetStatus;
            realization.SubmittedAt = now;
            realization.SubmittedByUserId = actorUserId;
            realization.RealizationNotes = AppendNote(
                realization.RealizationNotes,
                request?.Notes,
                2000);
            realization.UpdateDateTime = now;
            realization.UpdateBy = actorUserId;

            if (!requireVerification)
            {
                realization.VerifiedMinutes = realization.EligibleMinutes;
                realization.VerifiedAt = now;
                realization.VerifiedByUserId = actorUserId;
            }

            foreach (var detail in realization.Details)
            {
                detail.DetailStatus = requireVerification
                    ? OvertimeValueConstants.RealizationDetailStatus.Submitted
                    : OvertimeValueConstants.RealizationDetailStatus.Verified;
                detail.VerifiedMinutes = requireVerification ? 0 : detail.EligibleMinutes;
                detail.UpdateDateTime = now;
                detail.UpdateBy = actorUserId;
            }

            realization.OvertimeRequest.OvertimeRequestStatus = requireVerification
                ? OvertimeValueConstants.RequestStatus.WaitingVerification
                : OvertimeValueConstants.RequestStatus.Realized;
            realization.OvertimeRequest.RealizedAt = requireVerification
                ? null
                : now;
            realization.OvertimeRequest.UpdateDateTime = now;
            realization.OvertimeRequest.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return OvertimeRealizationServiceResult<OvertimeRealizationMutationResponse>.Ok(
                MapMutation(
                    realization,
                    realization.OvertimeRequest,
                    ExtractFingerprint(realization.CalculationResultJson),
                    false),
                requireVerification
                    ? "Overtime realization berhasil dikirim ke verifikasi."
                    : "Policy tidak mewajibkan post verification; realization langsung diselesaikan sebagai Verified.");
        }

        public async Task<OvertimeRealizationServiceResult<OvertimeRealizationMutationResponse>> CancelAsync(
            Guid realizationId,
            CancelOvertimeRealizationRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty)
            {
                return OvertimeRealizationServiceResult<OvertimeRealizationMutationResponse>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Identitas user login tidak valid.");
            }

            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return OvertimeRealizationServiceResult<OvertimeRealizationMutationResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan pembatalan wajib diisi.");
            }

            var realization = await _dbContext.TrxOvertimeRealizations
                .Include(x => x.OvertimeRequest)
                .Include(x => x.Details.Where(d => !d.IsDelete))
                .FirstOrDefaultAsync(
                    x => x.Id == realizationId && !x.IsDelete && !x.IsCancel && x.IsActive,
                    cancellationToken);

            if (realization?.OvertimeRequest == null)
            {
                return OvertimeRealizationServiceResult<OvertimeRealizationMutationResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Overtime realization tidak ditemukan.");
            }

            var periodGuard = await CheckPeriodAsync(realization.OvertimeRequest, cancellationToken);
            if (!periodGuard.IsWritable)
            {
                return OvertimeRealizationServiceResult<OvertimeRealizationMutationResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    periodGuard.Message);
            }

            if (IsFinalOrPosted(realization.RealizationStatus))
            {
                return OvertimeRealizationServiceResult<OvertimeRealizationMutationResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Realization yang sudah verified atau posted ke payroll tidak dapat dibatalkan pada Tahap 4E.");
            }

            var now = DateTime.UtcNow;
            realization.RealizationStatus = OvertimeValueConstants.RealizationStatus.Cancelled;
            realization.CancelledAt = now;
            realization.CancelledByUserId = actorUserId;
            realization.IsCancel = true;
            realization.IsActive = false;
            realization.CancelDateTime = now;
            realization.CancelBy = actorUserId;
            realization.RealizationNotes = AppendNote(
                realization.RealizationNotes,
                "Cancelled: " + request.Reason.Trim(),
                2000);
            realization.UpdateDateTime = now;
            realization.UpdateBy = actorUserId;

            foreach (var detail in realization.Details)
            {
                detail.DetailStatus = OvertimeValueConstants.RealizationDetailStatus.Cancelled;
                detail.IsCancel = true;
                detail.IsActive = false;
                detail.CancelDateTime = now;
                detail.CancelBy = actorUserId;
                detail.UpdateDateTime = now;
                detail.UpdateBy = actorUserId;
            }

            realization.OvertimeRequest.OvertimeRequestStatus =
                OvertimeValueConstants.RequestStatus.WaitingRealization;
            realization.OvertimeRequest.WaitingRealizationAt ??= now;
            realization.OvertimeRequest.UpdateDateTime = now;
            realization.OvertimeRequest.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return OvertimeRealizationServiceResult<OvertimeRealizationMutationResponse>.Ok(
                MapMutation(
                    realization,
                    realization.OvertimeRequest,
                    ExtractFingerprint(realization.CalculationResultJson),
                    false),
                "Overtime realization berhasil dibatalkan. Calculate ulang dapat dilakukan setelah attendance diperbaiki.");
        }

        private void BuildRealizationDetails(
            TrxOvertimeRealization realization,
            WfpOvertimeRequest overtimeRequest,
            OvertimeRealizationPreviewResponse preview,
            string targetStatus,
            Guid actorUserId,
            DateTime now)
        {
            var requestDetails = overtimeRequest.Details.ToDictionary(x => x.Id);
            var sequence = 1;

            foreach (var detailPreview in preview.Details.OrderBy(x => x.SequenceNumber))
            {
                requestDetails.TryGetValue(
                    detailPreview.OvertimeRequestDetailId,
                    out var requestDetail);

                var attendanceCheckIn = detailPreview.AttendanceEvidence
                    .Select(x => x.FirstCheckInAt)
                    .Where(x => x.HasValue)
                    .DefaultIfEmpty()
                    .Min();
                var attendanceCheckOut = detailPreview.AttendanceEvidence
                    .Select(x => x.LastCheckOutAt)
                    .Where(x => x.HasValue)
                    .DefaultIfEmpty()
                    .Max();

                var breakdown = detailPreview.RateBreakdown;
                if (breakdown.Count == 0)
                {
                    var startAt = detailPreview.MatchedIntervals
                        .Select(x => (DateTime?)x.MatchedStartAt)
                        .DefaultIfEmpty()
                        .Min() ?? detailPreview.ApprovedStartAt;
                    var endAt = detailPreview.MatchedIntervals
                        .Select(x => (DateTime?)x.MatchedEndAt)
                        .DefaultIfEmpty()
                        .Max() ?? detailPreview.ApprovedEndAt;

                    var fallback = new TrxOvertimeRealizationDetail
                    {
                        Id = Guid.NewGuid(),
                        OvertimeRealizationId = realization.Id,
                        OvertimeRequestDetailId = requestDetail?.Id,
                        ShiftAssignmentId = requestDetail?.ShiftAssignmentId,
                        AttendanceId = detailPreview.AttendanceEvidence.FirstOrDefault()?.AttendanceId,
                        AttendanceDailyId = detailPreview.AttendanceEvidence.FirstOrDefault()?.AttendanceDailyId,
                        OvertimeRateId = null,
                        SequenceNumber = sequence++,
                        OvertimeDate = detailPreview.OvertimeDate,
                        AttendanceCheckInAt = attendanceCheckIn,
                        AttendanceCheckOutAt = attendanceCheckOut,
                        ActualStartAt = startAt,
                        ActualEndAt = endAt,
                        ActualMinutes = detailPreview.RawMatchedMinutes,
                        BreakMinutes = detailPreview.AppliedBreakMinutes,
                        EligibleMinutes = detailPreview.EligibleMinutes,
                        VerifiedMinutes = targetStatus == OvertimeValueConstants.RealizationStatus.Verified
                            ? detailPreview.EligibleMinutes
                            : 0,
                        VarianceFromApprovedMinutes = detailPreview.VarianceFromApprovedMinutes,
                        DayType = detailPreview.DayType,
                        RateBandSnapshot = null,
                        CalculationMethodSnapshot = null,
                        RateMultiplierSnapshot = 1,
                        FixedAmountSnapshot = null,
                        BaseHourlyRateSnapshot = requestDetail?.BaseHourlyRateSnapshot ?? 0,
                        CalculatedAmount = 0,
                        VerifiedAmount = 0,
                        Notes = "Tidak ada payable rate breakdown karena eligible minutes bernilai 0.",
                        DetailStatus = MapDetailStatus(targetStatus),
                        IsActive = true,
                        CreateDateTime = now,
                        CreateBy = actorUserId,
                        UpdateBy = actorUserId
                    };

                    realization.Details.Add(fallback);
                    continue;
                }

                var firstBand = true;
                foreach (var rate in breakdown)
                {
                    var detail = new TrxOvertimeRealizationDetail
                    {
                        Id = Guid.NewGuid(),
                        OvertimeRealizationId = realization.Id,
                        OvertimeRequestDetailId = requestDetail?.Id,
                        ShiftAssignmentId = rate.ShiftAssignmentId ?? requestDetail?.ShiftAssignmentId,
                        AttendanceId = rate.AttendanceId,
                        AttendanceDailyId = rate.AttendanceDailyId,
                        OvertimeRateId = rate.OvertimeRateId,
                        SequenceNumber = sequence++,
                        OvertimeDate = rate.OvertimeDate,
                        AttendanceCheckInAt = attendanceCheckIn,
                        AttendanceCheckOutAt = attendanceCheckOut,
                        ActualStartAt = rate.StartAt,
                        ActualEndAt = rate.EndAt,
                        ActualMinutes = rate.Minutes,
                        BreakMinutes = firstBand ? detailPreview.AppliedBreakMinutes : 0,
                        EligibleMinutes = rate.Minutes,
                        VerifiedMinutes = targetStatus == OvertimeValueConstants.RealizationStatus.Verified
                            ? rate.Minutes
                            : 0,
                        VarianceFromApprovedMinutes = firstBand
                            ? detailPreview.VarianceFromApprovedMinutes
                            : 0,
                        DayType = rate.DayType,
                        RateBandSnapshot = rate.TimeBand,
                        CalculationMethodSnapshot = rate.CalculationMethod,
                        RateMultiplierSnapshot = rate.RateMultiplier,
                        FixedAmountSnapshot = rate.FixedAmount,
                        BaseHourlyRateSnapshot = requestDetail?.BaseHourlyRateSnapshot ?? 0,
                        CalculatedAmount = 0,
                        VerifiedAmount = 0,
                        Notes = "Nominal calculation ditunda ke Payroll; Overtime menyimpan approved minutes dan rate snapshot.",
                        DetailStatus = MapDetailStatus(targetStatus),
                        IsActive = true,
                        CreateDateTime = now,
                        CreateBy = actorUserId,
                        UpdateBy = actorUserId
                    };

                    realization.Details.Add(detail);
                    firstBand = false;
                }
            }
        }

        private static void ApplyRequestLifecycle(
            WfpOvertimeRequest overtimeRequest,
            OvertimeRealizationPreviewResponse preview,
            string targetStatus,
            Guid actorUserId,
            DateTime now)
        {
            var firstEvidence = preview.Details
                .SelectMany(x => x.AttendanceEvidence)
                .OrderBy(x => x.AttendanceDate)
                .FirstOrDefault();

            overtimeRequest.AttendanceDailyId = firstEvidence?.AttendanceDailyId;
            overtimeRequest.AttendanceId = firstEvidence?.AttendanceId;
            overtimeRequest.StartedAt ??= preview.Details
                .SelectMany(x => x.MatchedIntervals)
                .Select(x => (DateTime?)x.MatchedStartAt)
                .DefaultIfEmpty()
                .Min();
            overtimeRequest.WaitingRealizationAt ??= now;

            if (targetStatus == OvertimeValueConstants.RealizationStatus.WaitingVerification)
            {
                overtimeRequest.OvertimeRequestStatus = OvertimeValueConstants.RequestStatus.WaitingVerification;
            }
            else if (targetStatus == OvertimeValueConstants.RealizationStatus.Verified)
            {
                overtimeRequest.OvertimeRequestStatus = OvertimeValueConstants.RequestStatus.Realized;
                overtimeRequest.RealizedAt = now;
            }
            else
            {
                overtimeRequest.OvertimeRequestStatus = OvertimeValueConstants.RequestStatus.WaitingRealization;
            }

            overtimeRequest.UpdateDateTime = now;
            overtimeRequest.UpdateBy = actorUserId;

            var detailPreviewMap = preview.Details.ToDictionary(x => x.OvertimeRequestDetailId);
            foreach (var detail in overtimeRequest.Details)
            {
                if (!detailPreviewMap.TryGetValue(detail.Id, out var detailPreview)) continue;

                var evidence = detailPreview.AttendanceEvidence.FirstOrDefault();
                detail.AttendanceDailyId = evidence?.AttendanceDailyId;
                detail.AttendanceId = evidence?.AttendanceId;
                detail.OvertimeRateId = detailPreview.RateBreakdown.Count == 1
                    ? detailPreview.RateBreakdown[0].OvertimeRateId
                    : null;
                detail.DetailStatus = overtimeRequest.OvertimeRequestStatus;
                detail.UpdateDateTime = now;
                detail.UpdateBy = actorUserId;
            }
        }

        private Task<OvertimePeriodGuardResult> CheckPeriodAsync(
            WfpOvertimeRequest request,
            CancellationToken cancellationToken) =>
            _periodGuard.CheckDateAsync(
                request.OvertimeDate,
                null,
                request.HospitalSiteId,
                request.OrganizationUnitId,
                request.DepartmentId,
                cancellationToken);

        private async Task<string> GenerateRealizationNumberAsync(
            DateOnly date,
            CancellationToken cancellationToken)
        {
            var prefix = $"OTR-RLZ-{date:yyyyMMdd}-";
            await _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock(hashtext({0}));",
                new object[] { "OVERTIME_REALIZATION_" + date.ToString("yyyyMMdd") },
                cancellationToken);

            var last = await _dbContext.TrxOvertimeRealizations
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.RealizationNumber.StartsWith(prefix))
                .OrderByDescending(x => x.RealizationNumber)
                .Select(x => x.RealizationNumber)
                .FirstOrDefaultAsync(cancellationToken);

            var sequence = 0;
            if (!string.IsNullOrWhiteSpace(last) &&
                last.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                int.TryParse(last[prefix.Length..], out sequence);
            }

            return prefix + (sequence + 1).ToString("D5");
        }

        private static string ResolveTargetStatus(
            bool submitForVerification,
            bool requirePostVerification)
        {
            if (!submitForVerification)
            {
                return OvertimeValueConstants.RealizationStatus.Draft;
            }

            return requirePostVerification
                ? OvertimeValueConstants.RealizationStatus.WaitingVerification
                : OvertimeValueConstants.RealizationStatus.Verified;
        }

        private static string MapDetailStatus(string realizationStatus) => realizationStatus switch
        {
            var status when status == OvertimeValueConstants.RealizationStatus.WaitingVerification =>
                OvertimeValueConstants.RealizationDetailStatus.Submitted,
            var status when status == OvertimeValueConstants.RealizationStatus.Verified =>
                OvertimeValueConstants.RealizationDetailStatus.Verified,
            var status when status == OvertimeValueConstants.RealizationStatus.NeedRevision =>
                OvertimeValueConstants.RealizationDetailStatus.NeedRevision,
            var status when status == OvertimeValueConstants.RealizationStatus.Rejected =>
                OvertimeValueConstants.RealizationDetailStatus.Rejected,
            var status when status == OvertimeValueConstants.RealizationStatus.Cancelled =>
                OvertimeValueConstants.RealizationDetailStatus.Cancelled,
            _ => OvertimeValueConstants.RealizationDetailStatus.Draft
        };

        private static bool IsFinalOrPosted(string status) =>
            string.Equals(status, OvertimeValueConstants.RealizationStatus.Verified, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, OvertimeValueConstants.RealizationStatus.PostedToPayroll, StringComparison.OrdinalIgnoreCase);

        private static string BuildEvidenceSummaryJson(
            OvertimeRealizationPreviewResponse preview) =>
            JsonSerializer.Serialize(new
            {
                SchemaVersion = "4E.1",
                preview.InputFingerprint,
                Attendance = preview.Details.SelectMany(x => x.AttendanceEvidence),
                MatchedIntervals = preview.Details.SelectMany(x => x.MatchedIntervals)
            });

        private static string BuildCalculationResultJson(
            OvertimeRealizationPreviewResponse preview,
            CalculateOvertimeRealizationRequest request,
            int version) =>
            JsonSerializer.Serialize(new
            {
                SchemaVersion = "4E.1",
                InputFingerprint = preview.InputFingerprint,
                Trigger = request.ForceNewVersion
                    ? OvertimeValueConstants.CalculationTrigger.Recalculate
                    : OvertimeValueConstants.CalculationTrigger.Manual,
                request.IdempotencyKey,
                RealizationVersion = version,
                CalculatedAt = DateTime.UtcNow,
                NominalCalculationDeferredToPayroll = true,
                Totals = new
                {
                    preview.RequestedMinutes,
                    preview.ApprovedMinutes,
                    preview.ActualMinutes,
                    preview.BreakMinutes,
                    preview.EligibleMinutes,
                    preview.VarianceMinutes
                },
                Details = preview.Details
            });

        private static string ExtractFingerprint(string? calculationResultJson)
        {
            if (string.IsNullOrWhiteSpace(calculationResultJson)) return string.Empty;

            try
            {
                using var document = JsonDocument.Parse(calculationResultJson);
                return document.RootElement.TryGetProperty("InputFingerprint", out var value)
                    ? value.GetString() ?? string.Empty
                    : string.Empty;
            }
            catch (JsonException)
            {
                return string.Empty;
            }
        }

        private static OvertimeRealizationMutationResponse MapMutation(
            TrxOvertimeRealization realization,
            WfpOvertimeRequest overtimeRequest,
            string fingerprint,
            bool idempotent) => new()
        {
            OvertimeRequestId = overtimeRequest.Id,
            RequestNumber = overtimeRequest.RequestNumber,
            RequestStatus = overtimeRequest.OvertimeRequestStatus,
            OvertimeRealizationId = realization.Id,
            RealizationNumber = realization.RealizationNumber,
            RealizationVersion = realization.RealizationVersion,
            RealizationStatus = realization.RealizationStatus,
            InputFingerprint = fingerprint,
            ActualMinutes = realization.ActualMinutes,
            BreakMinutes = realization.ActualBreakMinutes,
            EligibleMinutes = realization.EligibleMinutes,
            VarianceMinutes = realization.VarianceMinutes,
            IsIdempotentResult = idempotent,
            SubmittedAt = realization.SubmittedAt
        };

        private static string? NormalizeText(string? value) =>
            string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();

        private static string? AppendNote(
            string? existing,
            string? value,
            int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value)) return existing;

            var combined = string.IsNullOrWhiteSpace(existing)
                ? value.Trim()
                : existing.Trim() + Environment.NewLine + value.Trim();

            return combined.Length <= maxLength
                ? combined
                : combined[..maxLength];
        }
    }
}
