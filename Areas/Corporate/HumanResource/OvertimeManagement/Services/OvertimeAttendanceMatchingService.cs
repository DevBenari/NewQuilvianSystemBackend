using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Repositories;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public class OvertimeAttendanceMatchingService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<OvertimeAttendanceMatchingService> _logger;

        public OvertimeAttendanceMatchingService(
            ApplicationDbContext dbContext,
            ILogger<OvertimeAttendanceMatchingService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<OvertimeRealizationServiceResult<OvertimeRealizationPreviewResponse>> PreviewAsync(
            Guid overtimeRequestId,
            PreviewOvertimeRealizationRequest? request = null,
            CancellationToken cancellationToken = default)
        {
            request ??= new PreviewOvertimeRealizationRequest();

            var overtimeRequest = await _dbContext.WfpOvertimeRequests
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.Employee)
                .Include(x => x.OvertimePolicy)
                .Include(x => x.Details.Where(d => !d.IsDelete && !d.IsCancel && d.IsActive))
                .FirstOrDefaultAsync(
                    x => x.Id == overtimeRequestId && !x.IsDelete,
                    cancellationToken);

            if (overtimeRequest == null)
            {
                return OvertimeRealizationServiceResult<OvertimeRealizationPreviewResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Overtime request tidak ditemukan.");
            }

            if (overtimeRequest.IsCancel || !overtimeRequest.IsActive)
            {
                return OvertimeRealizationServiceResult<OvertimeRealizationPreviewResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Overtime request sudah dibatalkan atau tidak aktif.");
            }

            if (!CanEnterRealization(overtimeRequest.OvertimeRequestStatus))
            {
                return OvertimeRealizationServiceResult<OvertimeRealizationPreviewResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Status overtime request '{overtimeRequest.OvertimeRequestStatus}' belum dapat diproses menjadi realization.");
            }

            var policy = overtimeRequest.OvertimePolicy;
            if (policy == null || policy.IsDelete || policy.IsCancel || !policy.IsActive)
            {
                return OvertimeRealizationServiceResult<OvertimeRealizationPreviewResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Overtime policy request tidak ditemukan atau sudah tidak aktif.");
            }

            var details = overtimeRequest.Details
                .OrderBy(x => x.SequenceNumber)
                .ThenBy(x => x.OvertimeDate)
                .ToList();

            if (details.Count == 0)
            {
                return OvertimeRealizationServiceResult<OvertimeRealizationPreviewResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Overtime request tidak memiliki detail aktif.");
            }

            var detailWindows = details
                .Select(BuildApprovedWindow)
                .ToList();

            if (detailWindows.Any(x => x.EndAt <= x.StartAt))
            {
                return OvertimeRealizationServiceResult<OvertimeRealizationPreviewResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Terdapat detail overtime dengan periode approval yang tidak valid.");
            }

            var minDate = detailWindows.Min(x => DateOnly.FromDateTime(x.StartAt));
            var maxDate = detailWindows.Max(x => DateOnly.FromDateTime(x.EndAt));

            var attendanceDailies = await _dbContext.HrdAttendanceDailies
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == overtimeRequest.WorkforceProfileId &&
                    x.AttendanceDate >= minDate &&
                    x.AttendanceDate <= maxDate &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive)
                .OrderBy(x => x.AttendanceDate)
                .ThenByDescending(x => x.ProcessingVersion)
                .ToListAsync(cancellationToken);

            var selectedDailies = attendanceDailies
                .GroupBy(x => x.AttendanceDate)
                .Select(x => x
                    .OrderByDescending(d => d.ProcessingVersion)
                    .ThenByDescending(d => d.ProcessedAt)
                    .First())
                .ToList();

            var dailyIds = selectedDailies.Select(x => x.Id).ToList();

            var attendanceSegments = dailyIds.Count == 0
                ? new List<HrdAttendanceDailySegment>()
                : await _dbContext.HrdAttendanceDailySegments
                    .AsNoTracking()
                    .Where(x =>
                        dailyIds.Contains(x.AttendanceDailyId) &&
                        !x.IsDelete &&
                        !x.IsCancel &&
                        x.IsActive)
                    .OrderBy(x => x.AttendanceDailyId)
                    .ThenBy(x => x.SegmentOrder)
                    .ToListAsync(cancellationToken);

            var attendances = dailyIds.Count == 0
                ? new List<HrdAttendance>()
                : await _dbContext.HrdAttendances
                    .AsNoTracking()
                    .Where(x =>
                        x.AttendanceDailyId.HasValue &&
                        dailyIds.Contains(x.AttendanceDailyId.Value) &&
                        !x.IsDelete &&
                        !x.IsCancel)
                    .OrderByDescending(x => x.ProcessedAt)
                    .ToListAsync(cancellationToken);

            var attendanceByDaily = attendances
                .Where(x => x.AttendanceDailyId.HasValue)
                .GroupBy(x => x.AttendanceDailyId!.Value)
                .ToDictionary(x => x.Key, x => x.First());

            var rates = await _dbContext.MstOvertimeRates
                .AsNoTracking()
                .Where(x =>
                    x.OvertimePolicyId == policy.Id &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive)
                .ToListAsync(cancellationToken);

            var response = new OvertimeRealizationPreviewResponse
            {
                OvertimeRequestId = overtimeRequest.Id,
                RequestNumber = overtimeRequest.RequestNumber,
                RequestStatus = overtimeRequest.OvertimeRequestStatus,
                WorkforceProfileId = overtimeRequest.WorkforceProfileId,
                WorkforceProfileCode = overtimeRequest.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName = overtimeRequest.WorkforceProfile?.DisplayName ?? overtimeRequest.Employee?.FullName ?? string.Empty,
                OvertimePolicyId = policy.Id,
                OvertimePolicyCode = policy.OvertimePolicyCode,
                OvertimePolicyName = policy.OvertimePolicyName,
                RequireAttendanceMatch = policy.RequireAttendanceMatch,
                RequirePostVerification = policy.RequirePostVerification,
                RequestedMinutes = overtimeRequest.RequestedMinutes,
                ApprovedMinutes = overtimeRequest.ApprovedMinutes > 0
                    ? overtimeRequest.ApprovedMinutes
                    : details.Sum(x => x.ApprovedMinutes > 0 ? x.ApprovedMinutes : x.RequestedMinutes)
            };

            foreach (var detail in details)
            {
                var detailResponse = BuildDetailPreview(
                    detail,
                    policy,
                    selectedDailies,
                    attendanceSegments,
                    attendanceByDaily,
                    rates,
                    request);

                response.Details.Add(detailResponse);
                response.Issues.AddRange(detailResponse.Issues);
            }

            response.ActualMinutes = response.Details.Sum(x => x.RawMatchedMinutes);
            response.BreakMinutes = response.Details.Sum(x => x.AppliedBreakMinutes);
            response.EligibleMinutes = response.Details.Sum(x => x.EligibleMinutes);
            response.VarianceMinutes = response.EligibleMinutes - response.ApprovedMinutes;
            response.CanCalculate = !response.Issues.Any(x => x.IsBlocking);
            response.InputFingerprint = BuildFingerprint(
                overtimeRequest,
                policy,
                response);

            _logger.LogInformation(
                "Preview overtime realization untuk request {OvertimeRequestId} selesai. CanCalculate={CanCalculate}, ActualMinutes={ActualMinutes}, EligibleMinutes={EligibleMinutes}.",
                overtimeRequest.Id,
                response.CanCalculate,
                response.ActualMinutes,
                response.EligibleMinutes);

            return OvertimeRealizationServiceResult<OvertimeRealizationPreviewResponse>.Ok(
                response,
                response.CanCalculate
                    ? "Preview attendance matching dan actual overtime calculation berhasil dibuat."
                    : "Preview berhasil dibuat, tetapi masih terdapat masalah yang menghalangi calculation.");
        }

        private static OvertimeRequestDetailCalculationPreviewResponse BuildDetailPreview(
            TrxOvertimeRequestDetail detail,
            MstOvertimePolicy policy,
            IReadOnlyCollection<HrdAttendanceDaily> attendanceDailies,
            IReadOnlyCollection<HrdAttendanceDailySegment> attendanceSegments,
            IReadOnlyDictionary<Guid, HrdAttendance> attendanceByDaily,
            IReadOnlyCollection<MstOvertimeRate> allRates,
            PreviewOvertimeRealizationRequest request)
        {
            var window = BuildApprovedWindow(detail);
            var approvedMinutes = detail.ApprovedMinutes > 0
                ? detail.ApprovedMinutes
                : Math.Max(0, (int)Math.Floor((window.EndAt - window.StartAt).TotalMinutes));

            var result = new OvertimeRequestDetailCalculationPreviewResponse
            {
                OvertimeRequestDetailId = detail.Id,
                SequenceNumber = detail.SequenceNumber,
                OvertimeDate = detail.OvertimeDate,
                ApprovedStartAt = window.StartAt,
                ApprovedEndAt = window.EndAt,
                ApprovedMinutes = approvedMinutes,
                DayType = NormalizeDayType(detail.DayType),
                OvertimeCategory = detail.OvertimeCategory,
                MatchStatus = OvertimeValueConstants.AttendanceMatchStatus.Ready
            };

            var relevantDailies = attendanceDailies
                .Where(x => IsDateWithinWindow(x.AttendanceDate, window.StartAt, window.EndAt))
                .OrderBy(x => x.AttendanceDate)
                .ToList();

            if (relevantDailies.Count == 0 && policy.RequireAttendanceMatch)
            {
                result.MatchStatus = OvertimeValueConstants.AttendanceMatchStatus.AttendanceNotFound;
                result.Issues.Add(Issue(
                    "ATTENDANCE_NOT_FOUND",
                    "Error",
                    "Attendance daily untuk periode lembur tidak ditemukan.",
                    true,
                    detail.Id));
                return result;
            }

            foreach (var daily in relevantDailies)
            {
                attendanceByDaily.TryGetValue(daily.Id, out var attendance);
                var dailySegments = attendanceSegments
                    .Where(x => x.AttendanceDailyId == daily.Id)
                    .ToList();

                result.AttendanceEvidence.Add(new OvertimeAttendanceEvidenceResponse
                {
                    AttendanceDailyId = daily.Id,
                    AttendanceId = attendance?.Id,
                    AttendanceDate = daily.AttendanceDate,
                    AttendanceStatus = daily.AttendanceStatus,
                    ProcessingStatus = daily.ProcessingStatus,
                    ProcessingVersion = daily.ProcessingVersion,
                    ProcessedAt = daily.ProcessedAt,
                    FirstCheckInAt = daily.FirstCheckInAt,
                    LastCheckOutAt = daily.LastCheckOutAt,
                    HasMissingPunch = daily.HasMissingPunch,
                    IsCorrected = daily.IsCorrected,
                    IsLocked = daily.IsLocked,
                    SegmentIds = dailySegments.Select(x => x.Id).Distinct().ToList(),
                    RawLogIds = dailySegments
                        .SelectMany(x => new[] { x.StartRawLogId, x.EndRawLogId })
                        .Where(x => x.HasValue)
                        .Select(x => x!.Value)
                        .Distinct()
                        .ToList()
                });

                if (!string.Equals(
                        daily.ProcessingStatus,
                        AttendanceValueConstants.AttendanceProcessingStatus.Processed,
                        StringComparison.OrdinalIgnoreCase))
                {
                    var blocking = policy.RequireAttendanceMatch && !request.AllowUnprocessedAttendance;
                    result.MatchStatus = OvertimeValueConstants.AttendanceMatchStatus.AttendancePending;
                    result.Issues.Add(Issue(
                        "ATTENDANCE_NOT_PROCESSED",
                        blocking ? "Error" : "Warning",
                        $"Attendance tanggal {daily.AttendanceDate:yyyy-MM-dd} belum berstatus Processed.",
                        blocking,
                        daily.Id));
                }

                if (daily.HasMissingPunch || !daily.FirstCheckInAt.HasValue || !daily.LastCheckOutAt.HasValue)
                {
                    var blocking = policy.RequireAttendanceMatch;
                    result.MatchStatus = OvertimeValueConstants.AttendanceMatchStatus.IncompleteAttendance;
                    result.Issues.Add(Issue(
                        "ATTENDANCE_INCOMPLETE",
                        blocking ? "Error" : "Warning",
                        $"Attendance tanggal {daily.AttendanceDate:yyyy-MM-dd} tidak lengkap atau memiliki missing punch.",
                        blocking,
                        daily.Id));
                }
            }

            var sourceIntervals = BuildSourceIntervals(
                detail,
                window,
                relevantDailies,
                attendanceSegments,
                attendanceByDaily,
                policy.RequireAttendanceMatch);

            if (sourceIntervals.Count == 0 && !policy.RequireAttendanceMatch)
            {
                sourceIntervals.Add(new WorkingInterval
                {
                    OvertimeRequestDetailId = detail.Id,
                    StartAt = window.StartAt,
                    EndAt = window.EndAt,
                    SegmentType = "ApprovedScheduleFallback",
                    SegmentSource = "OvertimePolicy",
                    IsFallbackFromDaily = true
                });

                result.Issues.Add(Issue(
                    "ATTENDANCE_MATCH_NOT_REQUIRED",
                    "Info",
                    "Policy tidak mewajibkan attendance match; periode approval digunakan sebagai fallback calculation.",
                    false,
                    detail.Id));
            }

            var matchedSourceIntervals = sourceIntervals
                .Select(x => IntersectWithWindow(x, window.StartAt, window.EndAt))
                .Where(x => x != null)
                .Select(x => x!)
                .OrderBy(x => x.StartAt)
                .ToList();

            foreach (var interval in matchedSourceIntervals)
            {
                result.MatchedIntervals.Add(new OvertimeMatchedIntervalResponse
                {
                    OvertimeRequestDetailId = detail.Id,
                    AttendanceDailyId = interval.AttendanceDailyId,
                    AttendanceId = interval.AttendanceId,
                    AttendanceSegmentId = interval.AttendanceSegmentId,
                    ShiftAssignmentId = interval.ShiftAssignmentId,
                    StartRawLogId = interval.StartRawLogId,
                    EndRawLogId = interval.EndRawLogId,
                    SegmentType = interval.SegmentType,
                    SegmentSource = interval.SegmentSource,
                    SourceStartAt = interval.SourceStartAt ?? interval.StartAt,
                    SourceEndAt = interval.SourceEndAt ?? interval.EndAt,
                    MatchedStartAt = interval.StartAt,
                    MatchedEndAt = interval.EndAt,
                    MatchedMinutes = WholeMinutes(interval.StartAt, interval.EndAt),
                    IsCorrected = interval.IsCorrected,
                    IsFallbackFromDaily = interval.IsFallbackFromDaily
                });
            }

            var mergedWorkIntervals = MergeIntervals(matchedSourceIntervals);
            result.RawMatchedMinutes = mergedWorkIntervals.Sum(x => WholeMinutes(x.StartAt, x.EndAt));

            if (result.RawMatchedMinutes <= 0)
            {
                result.MatchStatus = OvertimeValueConstants.AttendanceMatchStatus.NoOverlap;
                result.Issues.Add(Issue(
                    "NO_ATTENDANCE_OVERLAP",
                    "Error",
                    "Tidak ditemukan waktu attendance aktual yang overlap dengan periode lembur yang disetujui.",
                    true,
                    detail.Id));
                return result;
            }

            var breakIntervals = BuildBreakIntervals(
                window,
                relevantDailies,
                attendanceSegments,
                attendanceByDaily);

            var workAfterExactBreak = SubtractIntervals(mergedWorkIntervals, breakIntervals);
            var exactBreakMinutes = Math.Max(
                0,
                result.RawMatchedMinutes - workAfterExactBreak.Sum(x => WholeMinutes(x.StartAt, x.EndAt)));

            result.ObservedBreakMinutes = Math.Min(
                result.RawMatchedMinutes,
                exactBreakMinutes);

            result.AppliedBreakMinutes = policy.DeductBreakMinutes
                ? Math.Max(result.ObservedBreakMinutes, Math.Max(0, policy.BreakDeductionMinutes))
                : result.ObservedBreakMinutes;
            result.AppliedBreakMinutes = Math.Min(result.RawMatchedMinutes, result.AppliedBreakMinutes);

            var additionalBreakToTrim = Math.Max(0, result.AppliedBreakMinutes - exactBreakMinutes);
            var afterBreakIntervals = TrimMinutesFromStart(workAfterExactBreak, additionalBreakToTrim);

            result.ThresholdMinutes = Math.Min(
                afterBreakIntervals.Sum(x => WholeMinutes(x.StartAt, x.EndAt)),
                Math.Max(0, policy.OvertimeThresholdMinutes));

            var afterThresholdIntervals = TrimMinutesFromStart(
                afterBreakIntervals,
                result.ThresholdMinutes);

            result.NetMinutesBeforeRounding = afterThresholdIntervals.Sum(x => WholeMinutes(x.StartAt, x.EndAt));

            var rounded = RoundMinutes(
                result.NetMinutesBeforeRounding,
                policy.RoundingIntervalMinutes,
                policy.RoundingMethod);

            // Actual realization tidak boleh menghasilkan menit melebihi evidence aktual.
            rounded = Math.Min(rounded, result.NetMinutesBeforeRounding);
            rounded = Math.Min(rounded, approvedMinutes);
            result.RoundedMinutes = Math.Max(0, rounded);

            if (result.NetMinutesBeforeRounding > approvedMinutes)
            {
                result.Issues.Add(Issue(
                    "ACTUAL_EXCEEDS_APPROVED",
                    "Warning",
                    "Waktu aktual melebihi menit yang disetujui; eligible minutes dibatasi sampai approved minutes.",
                    false,
                    detail.Id));
            }

            if (result.RoundedMinutes < policy.MinimumOvertimeMinutes)
            {
                result.Issues.Add(Issue(
                    "BELOW_POLICY_MINIMUM",
                    "Warning",
                    "Menit aktual setelah break, threshold, dan rounding berada di bawah minimum policy sehingga eligible minutes menjadi 0.",
                    false,
                    detail.Id));
                result.EligibleMinutes = 0;
            }
            else
            {
                result.EligibleMinutes = result.RoundedMinutes;
            }

            result.VarianceFromApprovedMinutes = result.EligibleMinutes - approvedMinutes;

            if (result.EligibleMinutes > 0 && request.IncludeRateBreakdown)
            {
                var eligibleIntervals = TakeMinutesFromStart(
                    afterThresholdIntervals,
                    result.EligibleMinutes);

                var rateResult = BuildRateBreakdown(
                    detail,
                    result.DayType,
                    result.EligibleMinutes,
                    eligibleIntervals,
                    allRates);

                result.RateBreakdown = rateResult.Breakdown;
                result.Issues.AddRange(rateResult.Issues);

                if (rateResult.Issues.Any(x => x.IsBlocking))
                {
                    result.MatchStatus = OvertimeValueConstants.AttendanceMatchStatus.RateNotResolved;
                }
            }

            if (!result.Issues.Any(x => x.IsBlocking))
            {
                result.MatchStatus = OvertimeValueConstants.AttendanceMatchStatus.Ready;
            }

            return result;
        }

        private static List<WorkingInterval> BuildSourceIntervals(
            TrxOvertimeRequestDetail detail,
            ApprovedWindow window,
            IReadOnlyCollection<HrdAttendanceDaily> relevantDailies,
            IReadOnlyCollection<HrdAttendanceDailySegment> attendanceSegments,
            IReadOnlyDictionary<Guid, HrdAttendance> attendanceByDaily,
            bool requireAttendanceMatch)
        {
            var result = new List<WorkingInterval>();
            var allowedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                AttendanceValueConstants.AttendanceSegmentType.Work,
                AttendanceValueConstants.AttendanceSegmentType.Overtime,
                AttendanceValueConstants.AttendanceSegmentType.OnCall,
                AttendanceValueConstants.AttendanceSegmentType.Remote,
                AttendanceValueConstants.AttendanceSegmentType.BusinessTrip
            };

            foreach (var daily in relevantDailies)
            {
                attendanceByDaily.TryGetValue(daily.Id, out var attendance);

                var segments = attendanceSegments
                    .Where(x =>
                        x.AttendanceDailyId == daily.Id &&
                        x.ActualStartAt.HasValue &&
                        x.ActualEndAt.HasValue &&
                        x.ActualEndAt.Value > x.ActualStartAt.Value &&
                        allowedTypes.Contains(x.SegmentType) &&
                        !string.Equals(x.SegmentStatus, AttendanceValueConstants.AttendanceSegmentStatus.Invalid, StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(x.SegmentStatus, AttendanceValueConstants.AttendanceSegmentStatus.Cancelled, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var segment in segments)
                {
                    result.Add(new WorkingInterval
                    {
                        OvertimeRequestDetailId = detail.Id,
                        AttendanceDailyId = daily.Id,
                        AttendanceId = attendance?.Id,
                        AttendanceSegmentId = segment.Id,
                        ShiftAssignmentId = segment.ShiftAssignmentId ?? detail.ShiftAssignmentId,
                        StartRawLogId = segment.StartRawLogId,
                        EndRawLogId = segment.EndRawLogId,
                        StartAt = NormalizeUtc(segment.ActualStartAt!.Value),
                        EndAt = NormalizeUtc(segment.ActualEndAt!.Value),
                        SourceStartAt = NormalizeUtc(segment.ActualStartAt.Value),
                        SourceEndAt = NormalizeUtc(segment.ActualEndAt.Value),
                        SegmentType = segment.SegmentType,
                        SegmentSource = segment.SegmentSource,
                        SourceBreakMinutes = Math.Max(0, segment.BreakMinutes),
                        IsCorrected = segment.IsCorrected || daily.IsCorrected
                    });
                }

                if (segments.Count == 0 && daily.FirstCheckInAt.HasValue && daily.LastCheckOutAt.HasValue)
                {
                    result.Add(new WorkingInterval
                    {
                        OvertimeRequestDetailId = detail.Id,
                        AttendanceDailyId = daily.Id,
                        AttendanceId = attendance?.Id,
                        ShiftAssignmentId = daily.PrimaryShiftAssignmentId ?? detail.ShiftAssignmentId,
                        StartAt = NormalizeUtc(daily.FirstCheckInAt.Value),
                        EndAt = NormalizeUtc(daily.LastCheckOutAt.Value),
                        SourceStartAt = NormalizeUtc(daily.FirstCheckInAt.Value),
                        SourceEndAt = NormalizeUtc(daily.LastCheckOutAt.Value),
                        SegmentType = "AttendanceDailyFallback",
                        SegmentSource = daily.ScheduleSource,
                        SourceBreakMinutes = Math.Max(0, daily.BreakMinutes),
                        IsCorrected = daily.IsCorrected,
                        IsFallbackFromDaily = true
                    });
                }
            }

            if (!requireAttendanceMatch && result.Count == 0)
            {
                result.Add(new WorkingInterval
                {
                    OvertimeRequestDetailId = detail.Id,
                    StartAt = window.StartAt,
                    EndAt = window.EndAt,
                    SourceStartAt = window.StartAt,
                    SourceEndAt = window.EndAt,
                    SegmentType = "ApprovedScheduleFallback",
                    SegmentSource = "OvertimePolicy",
                    IsFallbackFromDaily = true
                });
            }

            return result;
        }

        private static List<WorkingInterval> BuildBreakIntervals(
            ApprovedWindow window,
            IReadOnlyCollection<HrdAttendanceDaily> relevantDailies,
            IReadOnlyCollection<HrdAttendanceDailySegment> attendanceSegments,
            IReadOnlyDictionary<Guid, HrdAttendance> attendanceByDaily)
        {
            var breaks = new List<WorkingInterval>();

            foreach (var daily in relevantDailies)
            {
                attendanceByDaily.TryGetValue(daily.Id, out var attendance);

                foreach (var segment in attendanceSegments.Where(x =>
                    x.AttendanceDailyId == daily.Id &&
                    string.Equals(x.SegmentType, AttendanceValueConstants.AttendanceSegmentType.Break, StringComparison.OrdinalIgnoreCase) &&
                    x.ActualStartAt.HasValue &&
                    x.ActualEndAt.HasValue &&
                    x.ActualEndAt.Value > x.ActualStartAt.Value &&
                    !string.Equals(x.SegmentStatus, AttendanceValueConstants.AttendanceSegmentStatus.Invalid, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(x.SegmentStatus, AttendanceValueConstants.AttendanceSegmentStatus.Cancelled, StringComparison.OrdinalIgnoreCase)))
                {
                    var item = new WorkingInterval
                    {
                        AttendanceDailyId = daily.Id,
                        AttendanceId = attendance?.Id,
                        AttendanceSegmentId = segment.Id,
                        StartRawLogId = segment.StartRawLogId,
                        EndRawLogId = segment.EndRawLogId,
                        StartAt = NormalizeUtc(segment.ActualStartAt!.Value),
                        EndAt = NormalizeUtc(segment.ActualEndAt!.Value),
                        SegmentType = segment.SegmentType,
                        SegmentSource = segment.SegmentSource,
                        IsCorrected = segment.IsCorrected || daily.IsCorrected
                    };

                    var intersected = IntersectWithWindow(item, window.StartAt, window.EndAt);
                    if (intersected != null)
                    {
                        breaks.Add(intersected);
                    }
                }
            }

            return MergeIntervals(breaks);
        }

        private static RateBreakdownBuildResult BuildRateBreakdown(
            TrxOvertimeRequestDetail detail,
            string dayType,
            int totalEligibleMinutes,
            IReadOnlyCollection<WorkingInterval> eligibleIntervals,
            IReadOnlyCollection<MstOvertimeRate> allRates)
        {
            var response = new RateBreakdownBuildResult();
            var effectiveDate = DateTime.SpecifyKind(
                detail.OvertimeDate.ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Utc);

            var rates = allRates
                .Where(x =>
                    string.Equals(x.DayType, dayType, StringComparison.OrdinalIgnoreCase) &&
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= effectiveDate.Date) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= effectiveDate.Date) &&
                    x.MinimumEligibleMinutes <= totalEligibleMinutes &&
                    (!x.MaximumEligibleMinutes.HasValue || x.MaximumEligibleMinutes.Value >= totalEligibleMinutes))
                .ToList();

            if (rates.Count == 0)
            {
                response.Issues.Add(Issue(
                    "OVERTIME_RATE_NOT_FOUND",
                    "Error",
                    $"Tidak ada overtime rate aktif untuk day type {dayType}.",
                    true,
                    detail.Id));
                return response;
            }

            var minutePosition = 0;
            OvertimeRateBreakdownResponse? current = null;

            foreach (var interval in eligibleIntervals.OrderBy(x => x.StartAt))
            {
                var cursor = interval.StartAt;
                var intervalMinutes = WholeMinutes(interval.StartAt, interval.EndAt);

                for (var index = 0; index < intervalMinutes; index++)
                {
                    var selectedRate = ResolveRate(
                        rates,
                        minutePosition,
                        TimeOnly.FromDateTime(cursor));

                    if (selectedRate == null)
                    {
                        response.Issues.Add(Issue(
                            "OVERTIME_RATE_NOT_RESOLVED",
                            "Error",
                            $"Overtime rate tidak dapat di-resolve pada posisi menit {minutePosition} ({cursor:O}).",
                            true,
                            detail.Id));
                        return response;
                    }

                    var next = cursor.AddMinutes(1);
                    var canAppend = current != null &&
                        current.OvertimeRateId == selectedRate.Id &&
                        current.OvertimeRequestDetailId == detail.Id &&
                        current.AttendanceDailyId == interval.AttendanceDailyId &&
                        current.EndAt == cursor;

                    if (canAppend)
                    {
                        current!.EndAt = next;
                        current.Minutes += 1;
                    }
                    else
                    {
                        current = new OvertimeRateBreakdownResponse
                        {
                            OvertimeRequestDetailId = detail.Id,
                            AttendanceDailyId = interval.AttendanceDailyId,
                            AttendanceId = interval.AttendanceId,
                            ShiftAssignmentId = interval.ShiftAssignmentId ?? detail.ShiftAssignmentId,
                            OvertimeDate = detail.OvertimeDate,
                            StartAt = cursor,
                            EndAt = next,
                            Minutes = 1,
                            MinutePositionStart = minutePosition,
                            DayType = dayType,
                            OvertimeRateId = selectedRate.Id,
                            OvertimeRateCode = selectedRate.OvertimeRateCode,
                            OvertimeRateName = selectedRate.OvertimeRateName,
                            TimeBand = selectedRate.TimeBand,
                            CalculationMethod = selectedRate.CalculationMethod,
                            RateMultiplier = selectedRate.RateMultiplier,
                            FixedAmount = selectedRate.FixedAmount,
                            NominalCalculationDeferredToPayroll = true
                        };
                        response.Breakdown.Add(current);
                    }

                    minutePosition++;
                    cursor = next;
                }
            }

            return response;
        }

        private static MstOvertimeRate? ResolveRate(
            IEnumerable<MstOvertimeRate> rates,
            int minutePosition,
            TimeOnly occurrenceTime) => rates
            .Where(x => IsRateApplicable(x, minutePosition, occurrenceTime))
            .OrderByDescending(x => x.Priority)
            .ThenByDescending(x => GetApplicabilityScore(x.TimeBand))
            .ThenByDescending(x => x.EffectiveStartDate ?? DateTime.MinValue)
            .ThenBy(x => x.OvertimeRateCode)
            .FirstOrDefault();

        private static bool IsRateApplicable(
            MstOvertimeRate rate,
            int minutePosition,
            TimeOnly occurrenceTime)
        {
            if (string.Equals(rate.TimeBand, OvertimeValueConstants.TimeBand.AllDay, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (OvertimeValueConstants.TimeBand.UsesMinuteRange(rate.TimeBand))
            {
                return minutePosition >= rate.StartMinute &&
                    (!rate.EndMinute.HasValue || minutePosition < rate.EndMinute.Value);
            }

            if (OvertimeValueConstants.TimeBand.UsesClockRange(rate.TimeBand))
            {
                if (!rate.StartTime.HasValue || !rate.EndTime.HasValue)
                {
                    return false;
                }

                var start = rate.StartTime.Value;
                var end = rate.EndTime.Value;

                if (start == end)
                {
                    return true;
                }

                return start < end
                    ? occurrenceTime >= start && occurrenceTime < end
                    : occurrenceTime >= start || occurrenceTime < end;
            }

            return false;
        }

        private static int GetApplicabilityScore(string timeBand)
        {
            if (string.Equals(timeBand, OvertimeValueConstants.TimeBand.Custom, StringComparison.OrdinalIgnoreCase)) return 50;
            if (string.Equals(timeBand, OvertimeValueConstants.TimeBand.Night, StringComparison.OrdinalIgnoreCase)) return 40;
            if (OvertimeValueConstants.TimeBand.UsesMinuteRange(timeBand)) return 30;
            if (string.Equals(timeBand, OvertimeValueConstants.TimeBand.AllDay, StringComparison.OrdinalIgnoreCase)) return 10;
            return 0;
        }

        private static List<WorkingInterval> MergeIntervals(IEnumerable<WorkingInterval> source)
        {
            var ordered = source
                .Where(x => x.EndAt > x.StartAt)
                .OrderBy(x => x.StartAt)
                .ThenBy(x => x.EndAt)
                .ToList();

            var result = new List<WorkingInterval>();

            foreach (var item in ordered)
            {
                if (result.Count == 0 || item.StartAt > result[^1].EndAt)
                {
                    result.Add(item.Clone());
                    continue;
                }

                if (item.EndAt > result[^1].EndAt)
                {
                    result[^1].EndAt = item.EndAt;
                }

                result[^1].SourceBreakMinutes = Math.Max(
                    result[^1].SourceBreakMinutes,
                    item.SourceBreakMinutes);
            }

            return result;
        }

        private static List<WorkingInterval> SubtractIntervals(
            IReadOnlyCollection<WorkingInterval> workIntervals,
            IReadOnlyCollection<WorkingInterval> breakIntervals)
        {
            var result = workIntervals.Select(x => x.Clone()).ToList();

            foreach (var breakInterval in breakIntervals.OrderBy(x => x.StartAt))
            {
                var next = new List<WorkingInterval>();

                foreach (var work in result)
                {
                    if (breakInterval.EndAt <= work.StartAt || breakInterval.StartAt >= work.EndAt)
                    {
                        next.Add(work);
                        continue;
                    }

                    if (breakInterval.StartAt > work.StartAt)
                    {
                        var left = work.Clone();
                        left.EndAt = breakInterval.StartAt;
                        if (left.EndAt > left.StartAt) next.Add(left);
                    }

                    if (breakInterval.EndAt < work.EndAt)
                    {
                        var right = work.Clone();
                        right.StartAt = breakInterval.EndAt;
                        if (right.EndAt > right.StartAt) next.Add(right);
                    }
                }

                result = next;
            }

            return result;
        }

        private static List<WorkingInterval> TrimMinutesFromStart(
            IReadOnlyCollection<WorkingInterval> source,
            int minutesToTrim)
        {
            var remaining = Math.Max(0, minutesToTrim);
            var result = new List<WorkingInterval>();

            foreach (var interval in source.OrderBy(x => x.StartAt))
            {
                var minutes = WholeMinutes(interval.StartAt, interval.EndAt);
                if (minutes <= 0) continue;

                if (remaining >= minutes)
                {
                    remaining -= minutes;
                    continue;
                }

                var item = interval.Clone();
                if (remaining > 0)
                {
                    item.StartAt = item.StartAt.AddMinutes(remaining);
                    remaining = 0;
                }

                if (item.EndAt > item.StartAt)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        private static List<WorkingInterval> TakeMinutesFromStart(
            IReadOnlyCollection<WorkingInterval> source,
            int minutesToTake)
        {
            var remaining = Math.Max(0, minutesToTake);
            var result = new List<WorkingInterval>();

            foreach (var interval in source.OrderBy(x => x.StartAt))
            {
                if (remaining <= 0) break;

                var available = WholeMinutes(interval.StartAt, interval.EndAt);
                if (available <= 0) continue;

                var take = Math.Min(available, remaining);
                var item = interval.Clone();
                item.EndAt = item.StartAt.AddMinutes(take);
                result.Add(item);
                remaining -= take;
            }

            return result;
        }

        private static WorkingInterval? IntersectWithWindow(
            WorkingInterval source,
            DateTime startAt,
            DateTime endAt)
        {
            var start = source.StartAt > startAt ? source.StartAt : startAt;
            var end = source.EndAt < endAt ? source.EndAt : endAt;

            if (end <= start || WholeMinutes(start, end) <= 0)
            {
                return null;
            }

            var item = source.Clone();
            item.StartAt = start;
            item.EndAt = end;
            return item;
        }

        private static ApprovedWindow BuildApprovedWindow(TrxOvertimeRequestDetail detail)
        {
            var start = NormalizeUtc(detail.ApprovedStartAt ?? detail.PlannedStartAt);
            var end = NormalizeUtc(detail.ApprovedEndAt ?? detail.PlannedEndAt);
            return new ApprovedWindow(start, end);
        }

        private static bool IsDateWithinWindow(
            DateOnly date,
            DateTime startAt,
            DateTime endAt)
        {
            var startDate = DateOnly.FromDateTime(startAt);
            var endDate = DateOnly.FromDateTime(endAt);
            return date >= startDate && date <= endDate;
        }

        private static int RoundMinutes(int minutes, int interval, string method)
        {
            if (minutes <= 0 || interval <= 1 ||
                string.Equals(method, OvertimeValueConstants.RoundingMethod.None, StringComparison.OrdinalIgnoreCase))
            {
                return Math.Max(0, minutes);
            }

            var quotient = minutes / (double)interval;
            return method switch
            {
                var value when string.Equals(value, OvertimeValueConstants.RoundingMethod.Up, StringComparison.OrdinalIgnoreCase) =>
                    (int)Math.Ceiling(quotient) * interval,
                var value when string.Equals(value, OvertimeValueConstants.RoundingMethod.Nearest, StringComparison.OrdinalIgnoreCase) =>
                    (int)Math.Round(quotient, MidpointRounding.AwayFromZero) * interval,
                _ => (int)Math.Floor(quotient) * interval
            };
        }

        private static int WholeMinutes(DateTime startAt, DateTime endAt) =>
            Math.Max(0, (int)Math.Floor((endAt - startAt).TotalMinutes));

        private static bool CanEnterRealization(string status) =>
            string.Equals(status, OvertimeValueConstants.RequestStatus.ApprovedForWork, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, OvertimeValueConstants.RequestStatus.InProgress, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, OvertimeValueConstants.RequestStatus.WaitingRealization, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, OvertimeValueConstants.RequestStatus.WaitingVerification, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, OvertimeValueConstants.RequestStatus.Realized, StringComparison.OrdinalIgnoreCase);

        private static string NormalizeDayType(string? value)
        {
            var normalized = OvertimeValueConstants.DayType.All
                .FirstOrDefault(x => string.Equals(x, value, StringComparison.OrdinalIgnoreCase));
            return normalized ?? OvertimeValueConstants.DayType.Workday;
        }

        private static DateTime NormalizeUtc(DateTime value) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        private static OvertimeRealizationIssueResponse Issue(
            string code,
            string severity,
            string message,
            bool blocking,
            Guid? referenceId = null,
            string? field = null) => new()
        {
            Code = code,
            Severity = severity,
            Message = message,
            IsBlocking = blocking,
            ReferenceId = referenceId,
            Field = field
        };

        private static string BuildFingerprint(
            WfpOvertimeRequest overtimeRequest,
            MstOvertimePolicy policy,
            OvertimeRealizationPreviewResponse response)
        {
            var payload = JsonSerializer.Serialize(new
            {
                OvertimeRequestId = overtimeRequest.Id,
                Policy = new
                {
                    policy.Id,
                    policy.UpdateDateTime,
                    policy.RequireAttendanceMatch,
                    policy.MinimumOvertimeMinutes,
                    policy.OvertimeThresholdMinutes,
                    policy.RoundingIntervalMinutes,
                    policy.RoundingMethod,
                    policy.DeductBreakMinutes,
                    policy.BreakDeductionMinutes
                },
                Details = response.Details.Select(x => new
                {
                    x.OvertimeRequestDetailId,
                    x.ApprovedStartAt,
                    x.ApprovedEndAt,
                    x.ApprovedMinutes,
                    x.DayType,
                    Attendance = x.AttendanceEvidence.Select(a => new
                    {
                        a.AttendanceDailyId,
                        a.ProcessingVersion,
                        a.ProcessedAt,
                        a.FirstCheckInAt,
                        a.LastCheckOutAt,
                        a.HasMissingPunch,
                        a.IsCorrected,
                        a.SegmentIds,
                        a.RawLogIds
                    }),
                    Rate = x.RateBreakdown.Select(r => new
                    {
                        r.OvertimeRateId,
                        r.StartAt,
                        r.EndAt,
                        r.Minutes,
                        r.RateMultiplier,
                        r.FixedAmount
                    }),
                    x.RawMatchedMinutes,
                    x.AppliedBreakMinutes,
                    x.ThresholdMinutes,
                    x.EligibleMinutes
                })
            });

            using var sha256 = SHA256.Create();
            return Convert.ToHexString(
                sha256.ComputeHash(Encoding.UTF8.GetBytes(payload)));
        }

        private sealed record ApprovedWindow(DateTime StartAt, DateTime EndAt);

        private sealed class RateBreakdownBuildResult
        {
            public List<OvertimeRateBreakdownResponse> Breakdown { get; set; } = new();
            public List<OvertimeRealizationIssueResponse> Issues { get; set; } = new();
        }

        private sealed class WorkingInterval
        {
            public Guid OvertimeRequestDetailId { get; set; }
            public Guid? AttendanceDailyId { get; set; }
            public Guid? AttendanceId { get; set; }
            public Guid? AttendanceSegmentId { get; set; }
            public Guid? ShiftAssignmentId { get; set; }
            public Guid? StartRawLogId { get; set; }
            public Guid? EndRawLogId { get; set; }
            public DateTime StartAt { get; set; }
            public DateTime EndAt { get; set; }
            public DateTime? SourceStartAt { get; set; }
            public DateTime? SourceEndAt { get; set; }
            public string SegmentType { get; set; } = string.Empty;
            public string SegmentSource { get; set; } = string.Empty;
            public int SourceBreakMinutes { get; set; }
            public bool IsCorrected { get; set; }
            public bool IsFallbackFromDaily { get; set; }

            public WorkingInterval Clone() => new()
            {
                OvertimeRequestDetailId = OvertimeRequestDetailId,
                AttendanceDailyId = AttendanceDailyId,
                AttendanceId = AttendanceId,
                AttendanceSegmentId = AttendanceSegmentId,
                ShiftAssignmentId = ShiftAssignmentId,
                StartRawLogId = StartRawLogId,
                EndRawLogId = EndRawLogId,
                StartAt = StartAt,
                EndAt = EndAt,
                SourceStartAt = SourceStartAt,
                SourceEndAt = SourceEndAt,
                SegmentType = SegmentType,
                SegmentSource = SegmentSource,
                SourceBreakMinutes = SourceBreakMinutes,
                IsCorrected = IsCorrected,
                IsFallbackFromDaily = IsFallbackFromDaily
            };
        }
    }
}
