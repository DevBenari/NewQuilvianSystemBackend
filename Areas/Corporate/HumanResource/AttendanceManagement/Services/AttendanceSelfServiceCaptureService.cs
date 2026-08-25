using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Shared.HumanResource.DTOs;
using QuilvianSystemBackend.Shared.HumanResource.Services;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services
{
    public class AttendanceSelfServiceCaptureService
    {
        private const string DefaultTimeZoneId = "Asia/Jakarta";
        private const int OpenPunchLookbackHours = 48;
        private const decimal DefaultMaxAccuracyMeters = 250m;

        private readonly ApplicationDbContext _dbContext;
        private readonly HumanResourceContextService _humanResourceContextService;
        private readonly AttendanceRawLogService _rawLogService;
        private readonly AttendanceScheduleResolverService _scheduleResolverService;
        private readonly AttendanceProcessingService _processingService;
        private readonly IConfiguration _configuration;

        public AttendanceSelfServiceCaptureService(
            ApplicationDbContext dbContext,
            HumanResourceContextService humanResourceContextService,
            AttendanceRawLogService rawLogService,
            AttendanceScheduleResolverService scheduleResolverService,
            AttendanceProcessingService processingService,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _humanResourceContextService = humanResourceContextService;
            _rawLogService = rawLogService;
            _scheduleResolverService = scheduleResolverService;
            _processingService = processingService;
            _configuration = configuration;
        }

        public async Task<AttendanceRawLogServiceResult<AttendanceSelfServiceCaptureStatusResponse>> GetStatusAsync(
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var contextResult = await ResolveContextAsync(actorUserId, cancellationToken);
            if (!contextResult.Success || contextResult.Data == null)
            {
                return AttendanceRawLogServiceResult<AttendanceSelfServiceCaptureStatusResponse>.Fail(
                    contextResult.StatusCode,
                    contextResult.Message);
            }

            var context = contextResult.Data;
            var workforceProfileId = context.WorkforceProfileId!.Value;
            var nowUtc = DateTime.UtcNow;

            var bypass = await ResolveGeolocationBypassAsync(actorUserId, nowUtc, cancellationToken);

            var openPunchState = await GetOpenPunchStateAsync(
                actorUserId,
                workforceProfileId,
                nowUtc,
                cancellationToken);

            var locations = await GetAllowedLocationsAsync(context, bypass.IsActive, cancellationToken);
            var localNow = ConvertUtcToLocal(nowUtc);
            var localDate = DateOnly.FromDateTime(localNow);

            var currentDaily = await _dbContext.Set<HrdAttendanceDaily>()
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete &&
                    (x.AttendanceDate == localDate ||
                     x.AttendanceDate == localDate.AddDays(-1)))
                .OrderByDescending(x => x.AttendanceDate)
                .ThenByDescending(x => x.UpdateDateTime ?? x.CreateDateTime)
                .Select(x => new
                {
                    x.Id,
                    x.AttendanceDate,
                    x.AttendanceStatus,
                    x.ProcessingStatus
                })
                .FirstOrDefaultAsync(cancellationToken);

            var warnings = new List<string>();

            if (locations.Count == 0)
            {
                warnings.Add(
                    "Belum ada Attendance Location aktif yang mengizinkan attendance self-service untuk penempatan employee saat ini.");
            }

            if (openPunchState.IsCheckedIn &&
                openPunchState.LastCheckInAt.HasValue &&
                openPunchState.LastCheckInAt.Value < nowUtc.AddHours(-24))
            {
                warnings.Add(
                    "Check-in terakhir sudah lebih dari 24 jam dan belum mempunyai check-out. Periksa riwayat attendance atau ajukan koreksi bila diperlukan.");
            }

            var hasLocation = locations.Count > 0;

            return AttendanceRawLogServiceResult<AttendanceSelfServiceCaptureStatusResponse>.Ok(
                new AttendanceSelfServiceCaptureStatusResponse
                {
                    UserId = actorUserId,
                    WorkforceProfileId = workforceProfileId,
                    TimeZoneId = DefaultTimeZoneId,
                    ServerNowUtc = nowUtc,
                    LocalNow = localNow,
                    IsCheckedIn = openPunchState.IsCheckedIn,
                    CanCheckIn = hasLocation && !openPunchState.IsCheckedIn,
                    CanCheckOut = hasLocation && openPunchState.IsCheckedIn,
                    LastCheckInAt = openPunchState.LastCheckInAt,
                    LastCheckOutAt = openPunchState.LastCheckOutAt,
                    CurrentAttendanceDailyId = currentDaily?.Id,
                    CurrentAttendanceDate = currentDaily?.AttendanceDate,
                    AttendanceStatus = currentDaily?.AttendanceStatus,
                    AttendanceProcessingStatus = currentDaily?.ProcessingStatus,
                    GpsRequired = !bypass.IsActive,
                    IsGeolocationBypassActive = bypass.IsActive,
                    GeolocationBypassUntil = bypass.Until,
                    GeolocationBypassReason = bypass.IsActive ? bypass.Reason : null,
                    AllowedLocations = locations,
                    Warnings = warnings
                },
                "Status attendance self-service berhasil diambil.");
        }

        public Task<AttendanceRawLogServiceResult<AttendanceSelfServiceCaptureResponse>> CheckInAsync(
            AttendanceSelfServiceCaptureRequest request,
            Guid actorUserId,
            string? ipAddress,
            string? userAgent,
            CancellationToken cancellationToken = default)
        {
            return CaptureAsync(
                AttendanceValueConstants.RawLogEventType.CheckIn,
                request,
                actorUserId,
                ipAddress,
                userAgent,
                cancellationToken);
        }

        public Task<AttendanceRawLogServiceResult<AttendanceSelfServiceCaptureResponse>> CheckOutAsync(
            AttendanceSelfServiceCaptureRequest request,
            Guid actorUserId,
            string? ipAddress,
            string? userAgent,
            CancellationToken cancellationToken = default)
        {
            return CaptureAsync(
                AttendanceValueConstants.RawLogEventType.CheckOut,
                request,
                actorUserId,
                ipAddress,
                userAgent,
                cancellationToken);
        }

        private async Task<AttendanceRawLogServiceResult<AttendanceSelfServiceCaptureResponse>> CaptureAsync(
            string eventType,
            AttendanceSelfServiceCaptureRequest request,
            Guid actorUserId,
            string? ipAddress,
            string? userAgent,
            CancellationToken cancellationToken)
        {
            if (actorUserId == Guid.Empty)
            {
                return AttendanceRawLogServiceResult<AttendanceSelfServiceCaptureResponse>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Identitas user login tidak valid.");
            }

            if (request.ClientRequestId == Guid.Empty)
            {
                return AttendanceRawLogServiceResult<AttendanceSelfServiceCaptureResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "ClientRequestId wajib diisi untuk menjaga idempotency attendance.");
            }

            if (request.AttendanceLocationId == Guid.Empty)
            {
                return AttendanceRawLogServiceResult<AttendanceSelfServiceCaptureResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Attendance location wajib dipilih.");
            }

            var nowUtc = DateTime.UtcNow;
            var bypass = await ResolveGeolocationBypassAsync(actorUserId, nowUtc, cancellationToken);

            // Format sanity applies regardless of bypass: a coordinate that is
            // actually supplied must still be a real coordinate. This is not
            // the "GPS required" business rule (that part is skipped below
            // when bypass is active) — it just rejects garbage input.
            if (request.Latitude is < -90 or > 90 ||
                request.Longitude is < -180 or > 180)
            {
                return AttendanceRawLogServiceResult<AttendanceSelfServiceCaptureResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Koordinat GPS tidak valid.");
            }

            if (!bypass.IsActive)
            {
                if (!request.Latitude.HasValue || !request.Longitude.HasValue)
                {
                    return AttendanceRawLogServiceResult<AttendanceSelfServiceCaptureResponse>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Koordinat GPS wajib dikirim untuk mencatat attendance.");
                }

                var maxAccuracyMeters = _configuration.GetValue<decimal?>("AttendanceCapture:MaxAccuracyMeters")
                    ?? DefaultMaxAccuracyMeters;

                if (maxAccuracyMeters > 0)
                {
                    if (!request.AccuracyMeters.HasValue)
                    {
                        return AttendanceRawLogServiceResult<AttendanceSelfServiceCaptureResponse>.Fail(
                            StatusCodes.Status400BadRequest,
                            "Akurasi lokasi tidak terbaca. Aktifkan GPS/lokasi dengan akurasi tinggi.");
                    }

                    if (request.AccuracyMeters.Value > maxAccuracyMeters)
                    {
                        return AttendanceRawLogServiceResult<AttendanceSelfServiceCaptureResponse>.Fail(
                            StatusCodes.Status400BadRequest,
                            $"Akurasi lokasi terlalu rendah. Akurasi saat ini {request.AccuracyMeters.Value:0} meter, maksimal {maxAccuracyMeters:0} meter.");
                    }
                }
            }

            var externalLogId = request.ClientRequestId.ToString("N");

            var duplicateRawLog = await _dbContext.Set<HrdAttendanceRawLog>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.UserId == actorUserId &&
                    x.SourceType == AttendanceValueConstants.RawLogSourceType.WebLogin &&
                    x.ExternalLogId == externalLogId &&
                    !x.IsDelete,
                    cancellationToken);

            if (duplicateRawLog != null)
            {
                return await BuildDuplicateResponseAsync(
                    duplicateRawLog,
                    cancellationToken);
            }

            var contextResult = await ResolveContextAsync(actorUserId, cancellationToken);
            if (!contextResult.Success || contextResult.Data == null)
            {
                return AttendanceRawLogServiceResult<AttendanceSelfServiceCaptureResponse>.Fail(
                    contextResult.StatusCode,
                    contextResult.Message);
            }

            var context = contextResult.Data;
            var workforceProfileId = context.WorkforceProfileId!.Value;

            var openPunchState = await GetOpenPunchStateAsync(
                actorUserId,
                workforceProfileId,
                nowUtc,
                cancellationToken);

            if (eventType == AttendanceValueConstants.RawLogEventType.CheckIn &&
                openPunchState.IsCheckedIn)
            {
                return AttendanceRawLogServiceResult<AttendanceSelfServiceCaptureResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Employee masih memiliki check-in aktif. Lakukan Absen Pulang terlebih dahulu atau ajukan koreksi bila punch sebelumnya tidak sesuai.");
            }

            if (eventType == AttendanceValueConstants.RawLogEventType.CheckOut &&
                !openPunchState.IsCheckedIn)
            {
                return AttendanceRawLogServiceResult<AttendanceSelfServiceCaptureResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Tidak ditemukan check-in aktif dalam 48 jam terakhir. Absen Pulang tidak dapat dilakukan.");
            }

            var locationResult = await ResolveAndValidateLocationAsync(
                context,
                request,
                bypass,
                cancellationToken);

            if (!locationResult.Success || locationResult.Data == null)
            {
                return AttendanceRawLogServiceResult<AttendanceSelfServiceCaptureResponse>.Fail(
                    locationResult.StatusCode,
                    locationResult.Message);
            }

            var location = locationResult.Data.Location;
            var distanceMeters = locationResult.Data.DistanceMeters;

            var rawLogResult = await _rawLogService.CreateAsync(
                new CreateAttendanceRawLogRequest
                {
                    UserId = actorUserId,
                    AttendanceLocationId = location.Id,
                    ExternalLogId = externalLogId,
                    EventAt = new DateTimeOffset(nowUtc, TimeSpan.Zero),
                    EventType = eventType,
                    SourceType = AttendanceValueConstants.RawLogSourceType.WebLogin,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    AccuracyMeters = request.AccuracyMeters
                },
                actorUserId,
                ipAddress,
                userAgent);

            if (!rawLogResult.Success || rawLogResult.Data == null)
            {
                return AttendanceRawLogServiceResult<AttendanceSelfServiceCaptureResponse>.Fail(
                    rawLogResult.StatusCode,
                    rawLogResult.Message);
            }

            var rawLog = rawLogResult.Data;

            var workDateResolution = await ResolveWorkDateAsync(
                workforceProfileId,
                rawLog.EventAt,
                eventType,
                cancellationToken);

            var response = new AttendanceSelfServiceCaptureResponse
            {
                RawLogId = rawLog.Id,
                IsDuplicate = rawLog.IsDuplicate,
                EventType = rawLog.EventType,
                SourceType = rawLog.SourceType,
                EventAt = rawLog.EventAt,
                AttendanceLocationId = location.Id,
                AttendanceLocationName = location.AttendanceLocationName,
                DistanceMeters = distanceMeters,
                RadiusMeters = location.RadiusMeters,
                IsInsideGeofence = true,
                WorkDate = workDateResolution.WorkDate,
                RawLogProcessingStatus = rawLog.ProcessingStatus,
                ProcessingTriggered = false,
                ProcessingSucceeded = false,
                Message = rawLogResult.Message
            };

            if (!rawLog.WorkforceProfileId.HasValue ||
                !workDateResolution.IsResolved ||
                !workDateResolution.WorkDate.HasValue)
            {
                response.Message =
                    "Attendance berhasil direkam sebagai raw log. Jadwal kerja belum dapat diselesaikan sehingga attendance daily akan diproses oleh processing/scheduler setelah jadwal tersedia.";

                return AttendanceRawLogServiceResult<AttendanceSelfServiceCaptureResponse>.Ok(
                    response,
                    response.Message);
            }

            var processingResult = await _processingService.ProcessSingleAsync(
                new ProcessAttendanceSingleRequest
                {
                    WorkforceProfileId = rawLog.WorkforceProfileId.Value,
                    WorkDate = workDateResolution.WorkDate.Value,
                    ForceReprocess = true,
                    TriggerSource = AttendanceValueConstants.ProcessingTriggerSource.Api,
                    CorrelationId = $"ESS-{eventType}-{rawLog.Id:N}",
                    Notes = $"Employee self-service {eventType}."
                },
                actorUserId,
                cancellationToken);

            response.ProcessingTriggered = true;

            if (!processingResult.Success || processingResult.Data == null)
            {
                response.Message =
                    $"Attendance raw log berhasil direkam, tetapi processing attendance belum berhasil: {processingResult.Message}";

                return AttendanceRawLogServiceResult<AttendanceSelfServiceCaptureResponse>.Ok(
                    response,
                    response.Message);
            }

            var processingItem = processingResult.Data.Items
                .FirstOrDefault(x =>
                    x.WorkforceProfileId == rawLog.WorkforceProfileId.Value &&
                    x.WorkDate == workDateResolution.WorkDate.Value);

            if (processingItem != null)
            {
                response.ProcessingSucceeded = processingItem.Success;
                response.AttendanceDailyId = processingItem.AttendanceDailyId;
                response.AttendanceStatus = processingItem.AttendanceStatus;
                response.AttendanceProcessingStatus = processingItem.ProcessingStatus;
                response.Message = processingItem.Success
                    ? eventType == AttendanceValueConstants.RawLogEventType.CheckIn
                        ? "Absen Masuk berhasil direkam dan attendance berhasil diproses."
                        : "Absen Pulang berhasil direkam dan attendance berhasil diproses."
                    : $"Attendance raw log berhasil direkam, tetapi hasil processing memerlukan perhatian: {processingItem.Message}";
            }
            else
            {
                response.Message =
                    "Attendance raw log berhasil direkam. Processing dijalankan tetapi hasil item attendance tidak ditemukan pada response.";
            }

            return AttendanceRawLogServiceResult<AttendanceSelfServiceCaptureResponse>.Ok(
                response,
                response.Message);
        }

        // Effective bypass formula matches the canonical one already used by
        // the user-account read path (EmployeeController/DoctorController/
        // ExternalUserController BuildEmployeeUserAccountCompactResponseAsync
        // and equivalents): enabled AND (no expiry OR expiry not yet passed).
        private async Task<GeolocationBypassState> ResolveGeolocationBypassAsync(
            Guid actorUserId,
            DateTime nowUtc,
            CancellationToken cancellationToken)
        {
            var bypass = await _dbContext.Users
                .AsNoTracking()
                .Where(x => x.Id == actorUserId)
                .Select(x => new
                {
                    x.IsGeolocationBypassEnabled,
                    x.GeolocationBypassUntil,
                    x.GeolocationBypassReason
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (bypass == null)
            {
                return new GeolocationBypassState { IsActive = false };
            }

            var isActive =
                bypass.IsGeolocationBypassEnabled &&
                (!bypass.GeolocationBypassUntil.HasValue ||
                 bypass.GeolocationBypassUntil.Value >= nowUtc);

            return new GeolocationBypassState
            {
                IsActive = isActive,
                Enabled = bypass.IsGeolocationBypassEnabled,
                Until = bypass.GeolocationBypassUntil,
                Reason = bypass.GeolocationBypassReason
            };
        }

        private async Task<AttendanceRawLogServiceResult<HumanResourceUserContextDto>> ResolveContextAsync(
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            try
            {
                var context = await _humanResourceContextService.GetByUserIdAsync(
                    actorUserId,
                    cancellationToken);

                if (!context.WorkforceProfileId.HasValue ||
                    context.WorkforceProfileId.Value == Guid.Empty)
                {
                    return AttendanceRawLogServiceResult<HumanResourceUserContextDto>.Fail(
                        StatusCodes.Status409Conflict,
                        "Akun login belum terhubung ke workforce profile aktif.");
                }

                if (!context.HasOrganizationAssignment)
                {
                    return AttendanceRawLogServiceResult<HumanResourceUserContextDto>.Fail(
                        StatusCodes.Status409Conflict,
                        "Workforce profile belum memiliki organization assignment aktif.");
                }

                return AttendanceRawLogServiceResult<HumanResourceUserContextDto>.Ok(
                    context,
                    "Konteks workforce berhasil diselesaikan.");
            }
            catch (UnauthorizedAccessException exception)
            {
                return AttendanceRawLogServiceResult<HumanResourceUserContextDto>.Fail(
                    StatusCodes.Status401Unauthorized,
                    exception.Message);
            }
        }

        private async Task<List<AttendanceSelfServiceLocationResponse>> GetAllowedLocationsAsync(
            HumanResourceUserContextDto context,
            bool isBypassActive,
            CancellationToken cancellationToken)
        {
            return await BuildAllowedLocationQuery(context)
                .OrderByDescending(x =>
                    context.WorkLocationId.HasValue &&
                    x.WorkLocationId == context.WorkLocationId.Value)
                .ThenBy(x => x.AttendanceLocationName)
                .Select(x => new AttendanceSelfServiceLocationResponse
                {
                    Id = x.Id,
                    Code = x.AttendanceLocationCode,
                    Name = x.AttendanceLocationName,
                    HospitalSiteId = x.HospitalSiteId,
                    OrganizationUnitId = x.OrganizationUnitId,
                    WorkLocationId = x.WorkLocationId,
                    LocationType = x.LocationType,
                    RadiusMeters = x.RadiusMeters,
                    RequiresGeolocation = !isBypassActive,
                    AllowMobileAttendance = x.AllowMobileAttendance
                })
                .ToListAsync(cancellationToken);
        }

        private IQueryable<MstAttendanceLocation> BuildAllowedLocationQuery(
            HumanResourceUserContextDto context)
        {
            var query = _dbContext.Set<MstAttendanceLocation>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    x.AllowMobileAttendance &&
                    x.Latitude.HasValue &&
                    x.Longitude.HasValue &&
                    x.RadiusMeters > 0);

            if (context.WorkLocationId.HasValue)
            {
                var workLocationId = context.WorkLocationId.Value;

                query = query.Where(x =>
                    x.WorkLocationId == workLocationId ||
                    (!x.WorkLocationId.HasValue &&
                     context.HospitalSiteId.HasValue &&
                     x.HospitalSiteId == context.HospitalSiteId.Value));
            }
            else if (context.HospitalSiteId.HasValue)
            {
                var hospitalSiteId = context.HospitalSiteId.Value;
                query = query.Where(x => x.HospitalSiteId == hospitalSiteId);
            }
            else if (context.OrganizationUnitId.HasValue)
            {
                var organizationUnitId = context.OrganizationUnitId.Value;
                query = query.Where(x => x.OrganizationUnitId == organizationUnitId);
            }
            else
            {
                query = query.Where(x => false);
            }

            if (context.OrganizationUnitId.HasValue)
            {
                var organizationUnitId = context.OrganizationUnitId.Value;

                query = query.Where(x =>
                    !x.OrganizationUnitId.HasValue ||
                    x.OrganizationUnitId == organizationUnitId);
            }

            return query;
        }

        private async Task<AttendanceRawLogServiceResult<LocationValidationResult>> ResolveAndValidateLocationAsync(
            HumanResourceUserContextDto context,
            AttendanceSelfServiceCaptureRequest request,
            GeolocationBypassState bypass,
            CancellationToken cancellationToken)
        {
            // Attendance location authorization (organization/hospital/work-location
            // scope) always applies, bypass or not.
            var location = await BuildAllowedLocationQuery(context)
                .FirstOrDefaultAsync(
                    x => x.Id == request.AttendanceLocationId,
                    cancellationToken);

            if (location == null)
            {
                return AttendanceRawLogServiceResult<LocationValidationResult>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Attendance location tidak tersedia untuk employee atau tidak mengizinkan attendance self-service.");
            }

            decimal? distanceMeters = request.Latitude.HasValue && request.Longitude.HasValue
                ? CalculateDistanceMeters(
                    request.Latitude.Value,
                    request.Longitude.Value,
                    location.Latitude!.Value,
                    location.Longitude!.Value)
                : null;

            if (!bypass.IsActive)
            {
                // Non-bypass callers already had coordinates required earlier in
                // CaptureAsync; this guard is defensive only.
                if (!distanceMeters.HasValue)
                {
                    return AttendanceRawLogServiceResult<LocationValidationResult>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Koordinat GPS wajib dikirim untuk mencatat attendance.");
                }

                if (distanceMeters.Value > location.RadiusMeters)
                {
                    return AttendanceRawLogServiceResult<LocationValidationResult>.Fail(
                        StatusCodes.Status400BadRequest,
                        $"Posisi berada di luar area attendance. Jarak {distanceMeters.Value:0.##} meter dari titik attendance, sedangkan radius yang diizinkan {location.RadiusMeters} meter.");
                }
            }

            return AttendanceRawLogServiceResult<LocationValidationResult>.Ok(
                new LocationValidationResult
                {
                    Location = location,
                    DistanceMeters = distanceMeters
                },
                bypass.IsActive
                    ? "Attendance location dicatat dengan geolocation bypass aktif."
                    : "Lokasi GPS berada di dalam geofence attendance.");
        }

        private async Task<OpenPunchState> GetOpenPunchStateAsync(
            Guid actorUserId,
            Guid workforceProfileId,
            DateTime nowUtc,
            CancellationToken cancellationToken)
        {
            var startAt = nowUtc.AddHours(-OpenPunchLookbackHours);

            var events = await _dbContext.Set<HrdAttendanceRawLog>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive &&
                    x.EventAt >= startAt &&
                    x.EventAt <= nowUtc.AddMinutes(5) &&
                    (x.UserId == actorUserId ||
                     x.WorkforceProfileId == workforceProfileId) &&
                    (x.EventType == AttendanceValueConstants.RawLogEventType.CheckIn ||
                     x.EventType == AttendanceValueConstants.RawLogEventType.CheckOut))
                .OrderByDescending(x => x.EventAt)
                .ThenByDescending(x => x.ReceivedAt)
                .Select(x => new
                {
                    x.EventAt,
                    x.EventType
                })
                .Take(50)
                .ToListAsync(cancellationToken);

            var lastCheckIn = events
                .Where(x => x.EventType == AttendanceValueConstants.RawLogEventType.CheckIn)
                .Select(x => (DateTime?)x.EventAt)
                .FirstOrDefault();

            var lastCheckOut = events
                .Where(x => x.EventType == AttendanceValueConstants.RawLogEventType.CheckOut)
                .Select(x => (DateTime?)x.EventAt)
                .FirstOrDefault();

            var isCheckedIn =
                lastCheckIn.HasValue &&
                (!lastCheckOut.HasValue ||
                 lastCheckIn.Value > lastCheckOut.Value);

            return new OpenPunchState
            {
                IsCheckedIn = isCheckedIn,
                LastCheckInAt = lastCheckIn,
                LastCheckOutAt = lastCheckOut
            };
        }

        private async Task<WorkDateResolution> ResolveWorkDateAsync(
            Guid workforceProfileId,
            DateTime eventAtUtc,
            string eventType,
            CancellationToken cancellationToken)
        {
            var localDate = DateOnly.FromDateTime(
                ConvertUtcToLocal(eventAtUtc));

            var candidateDates = new[]
            {
                localDate.AddDays(-1),
                localDate,
                localDate.AddDays(1)
            };

            var candidates = new List<(DateOnly WorkDate, double Score)>();

            foreach (var candidateDate in candidateDates)
            {
                var scheduleResult = await _scheduleResolverService.ResolveAsync(
                    workforceProfileId,
                    candidateDate,
                    cancellationToken);

                if (!scheduleResult.Success ||
                    scheduleResult.Data == null ||
                    !scheduleResult.Data.IsResolved ||
                    scheduleResult.Data.IsRestDay ||
                    scheduleResult.Data.HasBlockingConflict)
                {
                    continue;
                }

                var schedule = scheduleResult.Data;

                var windowStart =
                    schedule.EarliestCheckInAt ??
                    schedule.ScheduledStartAt?.AddHours(-4);

                var windowEnd =
                    schedule.LatestCheckOutAt ??
                    schedule.ScheduledEndAt?.AddHours(8);

                if (!windowStart.HasValue || !windowEnd.HasValue)
                {
                    continue;
                }

                if (eventAtUtc < EnsureUtc(windowStart.Value) ||
                    eventAtUtc > EnsureUtc(windowEnd.Value))
                {
                    continue;
                }

                var referenceAt =
                    eventType == AttendanceValueConstants.RawLogEventType.CheckOut
                        ? schedule.ScheduledEndAt ?? windowEnd.Value
                        : schedule.ScheduledStartAt ?? windowStart.Value;

                var score = Math.Abs(
                    (eventAtUtc - EnsureUtc(referenceAt)).TotalMinutes);

                candidates.Add((candidateDate, score));
            }

            var selected = candidates
                .OrderBy(x => x.Score)
                .FirstOrDefault();

            if (selected != default)
            {
                return new WorkDateResolution
                {
                    WorkDate = selected.WorkDate,
                    IsResolved = true
                };
            }

            return new WorkDateResolution
            {
                WorkDate = localDate,
                IsResolved = false
            };
        }

        private async Task<AttendanceRawLogServiceResult<AttendanceSelfServiceCaptureResponse>> BuildDuplicateResponseAsync(
            HrdAttendanceRawLog rawLog,
            CancellationToken cancellationToken)
        {
            var location = rawLog.AttendanceLocationId.HasValue
                ? await _dbContext.Set<MstAttendanceLocation>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.Id == rawLog.AttendanceLocationId.Value,
                        cancellationToken)
                : null;

            var daily = rawLog.ProcessedAttendanceDailyId.HasValue
                ? await _dbContext.Set<HrdAttendanceDaily>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.Id == rawLog.ProcessedAttendanceDailyId.Value,
                        cancellationToken)
                : null;

            var radiusMeters = location?.RadiusMeters ?? 0;
            // Null distance means no GPS was captured for this raw log (e.g. an
            // active geolocation bypass at the time it was created) — that is
            // not the same as "0 meters away", so it is not coerced to 0m here.
            var distanceMeters = rawLog.DistanceMeters;

            var response = new AttendanceSelfServiceCaptureResponse
            {
                RawLogId = rawLog.Id,
                IsDuplicate = true,
                EventType = rawLog.EventType,
                SourceType = rawLog.SourceType,
                EventAt = rawLog.EventAt,
                AttendanceLocationId = rawLog.AttendanceLocationId ?? Guid.Empty,
                AttendanceLocationName = location?.AttendanceLocationName ?? string.Empty,
                DistanceMeters = distanceMeters,
                RadiusMeters = radiusMeters,
                IsInsideGeofence =
                    !distanceMeters.HasValue ||
                    (radiusMeters > 0 && distanceMeters.Value <= radiusMeters),
                WorkDate = daily?.AttendanceDate,
                ProcessingTriggered = false,
                ProcessingSucceeded =
                    rawLog.ProcessingStatus == AttendanceValueConstants.RawLogProcessingStatus.Processed,
                AttendanceDailyId = daily?.Id,
                AttendanceStatus = daily?.AttendanceStatus,
                AttendanceProcessingStatus = daily?.ProcessingStatus,
                RawLogProcessingStatus = rawLog.ProcessingStatus,
                Message = "Request attendance sudah pernah diterima. Data existing dikembalikan secara idempotent."
            };

            return AttendanceRawLogServiceResult<AttendanceSelfServiceCaptureResponse>.Ok(
                response,
                response.Message);
        }

        private static decimal CalculateDistanceMeters(
            decimal eventLatitude,
            decimal eventLongitude,
            decimal locationLatitude,
            decimal locationLongitude)
        {
            const double earthRadiusMeters = 6371000d;

            var lat1 = DegreesToRadians((double)eventLatitude);
            var lon1 = DegreesToRadians((double)eventLongitude);
            var lat2 = DegreesToRadians((double)locationLatitude);
            var lon2 = DegreesToRadians((double)locationLongitude);

            var deltaLat = lat2 - lat1;
            var deltaLon = lon2 - lon1;

            var a =
                Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return Math.Round(
                (decimal)(earthRadiusMeters * c),
                2);
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180d;
        }

        private static DateTime ConvertUtcToLocal(DateTime utcDateTime)
        {
            var utc = EnsureUtc(utcDateTime);

            try
            {
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById(DefaultTimeZoneId);
                return TimeZoneInfo.ConvertTimeFromUtc(utc, timeZone);
            }
            catch (TimeZoneNotFoundException)
            {
                var windowsTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(utc, windowsTimeZone);
            }
        }

        private static DateTime EnsureUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };
        }

        private sealed class LocationValidationResult
        {
            public MstAttendanceLocation Location { get; set; } = null!;
            public decimal? DistanceMeters { get; set; }
        }

        private sealed class GeolocationBypassState
        {
            public bool IsActive { get; set; }
            public bool Enabled { get; set; }
            public DateTime? Until { get; set; }
            public string? Reason { get; set; }
        }

        private sealed class OpenPunchState
        {
            public bool IsCheckedIn { get; set; }
            public DateTime? LastCheckInAt { get; set; }
            public DateTime? LastCheckOutAt { get; set; }
        }

        private sealed class WorkDateResolution
        {
            public DateOnly? WorkDate { get; set; }
            public bool IsResolved { get; set; }
        }
    }
}
