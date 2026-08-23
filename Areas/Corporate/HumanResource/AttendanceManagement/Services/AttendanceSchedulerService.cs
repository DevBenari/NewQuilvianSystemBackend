using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Repositories;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services
{
    public class AttendanceSchedulerService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly AttendanceProcessingService _processingService;
        private readonly AttendancePeriodService _periodService;
        private readonly AttendanceSchedulerOptions _options;
        private readonly ILogger<AttendanceSchedulerService> _logger;

        public AttendanceSchedulerService(
            ApplicationDbContext dbContext,
            AttendanceProcessingService processingService,
            AttendancePeriodService periodService,
            IOptions<AttendanceSchedulerOptions> options,
            ILogger<AttendanceSchedulerService> logger)
        {
            _dbContext = dbContext;
            _processingService = processingService;
            _periodService = periodService;
            _options = options.Value;
            _logger = logger;
        }

        public AttendanceSchedulerMetadataResponse GetMetadata()
        {
            return new AttendanceSchedulerMetadataResponse
            {
                SchedulerEnabled = _options.Enabled,
                AutoClosePeriods = _options.AutoClosePeriods,
                PollIntervalSeconds = Math.Max(5, _options.PollIntervalSeconds),
                TimeZoneId = _options.TimeZoneId,
                DailyProcessingHour = Math.Clamp(_options.DailyProcessingHour, 0, 23),
                DailyProcessingMinute = Math.Clamp(_options.DailyProcessingMinute, 0, 59),
                MaximumCatchUpDays = Math.Clamp(_options.MaximumCatchUpDays, 1, 31),
                RunningJobTimeoutMinutes = Math.Clamp(_options.RunningJobTimeoutMinutes, 5, 1440),
                JobTypeOptions = new List<AttendanceStringOptionResponse>
                {
                    new() { Value = AttendanceValueConstants.AttendanceSchedulerJobType.ProcessRange, Label = "Process range" },
                    new() { Value = AttendanceValueConstants.AttendanceSchedulerJobType.ReprocessRange, Label = "Reprocess range" }
                },
                JobStatusOptions = new List<AttendanceStringOptionResponse>
                {
                    new() { Value = AttendanceValueConstants.AttendanceSchedulerJobStatus.Pending, Label = "Pending" },
                    new() { Value = AttendanceValueConstants.AttendanceSchedulerJobStatus.Running, Label = "Running" },
                    new() { Value = AttendanceValueConstants.AttendanceSchedulerJobStatus.Completed, Label = "Completed" },
                    new() { Value = AttendanceValueConstants.AttendanceSchedulerJobStatus.CompletedWithErrors, Label = "Completed with errors" },
                    new() { Value = AttendanceValueConstants.AttendanceSchedulerJobStatus.RetryScheduled, Label = "Retry scheduled" },
                    new() { Value = AttendanceValueConstants.AttendanceSchedulerJobStatus.Failed, Label = "Failed" },
                    new() { Value = AttendanceValueConstants.AttendanceSchedulerJobStatus.Cancelled, Label = "Cancelled" }
                },
                SortOptions = new List<AttendanceSortOptionResponse>
                {
                    new() { Value = "scheduledAt", Label = "Scheduled at" },
                    new() { Value = "availableAt", Label = "Available at" },
                    new() { Value = "jobNumber", Label = "Job number" },
                    new() { Value = "jobStatus", Label = "Status" },
                    new() { Value = "priority", Label = "Priority" },
                    new() { Value = "createDateTime", Label = "Created at" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };
        }

        public async Task<AttendanceSchedulerSummaryResponse> GetSummaryAsync(
            AttendanceSchedulerJobQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter(BuildBaseQuery(), request);
            var now = DateTime.UtcNow;
            return new AttendanceSchedulerSummaryResponse
            {
                TotalJob = await query.CountAsync(cancellationToken),
                PendingJob = await query.CountAsync(x => x.JobStatus == AttendanceValueConstants.AttendanceSchedulerJobStatus.Pending, cancellationToken),
                RunningJob = await query.CountAsync(x => x.JobStatus == AttendanceValueConstants.AttendanceSchedulerJobStatus.Running, cancellationToken),
                RetryScheduledJob = await query.CountAsync(x => x.JobStatus == AttendanceValueConstants.AttendanceSchedulerJobStatus.RetryScheduled, cancellationToken),
                CompletedJob = await query.CountAsync(x => x.JobStatus == AttendanceValueConstants.AttendanceSchedulerJobStatus.Completed, cancellationToken),
                CompletedWithErrorsJob = await query.CountAsync(x => x.JobStatus == AttendanceValueConstants.AttendanceSchedulerJobStatus.CompletedWithErrors, cancellationToken),
                FailedJob = await query.CountAsync(x => x.JobStatus == AttendanceValueConstants.AttendanceSchedulerJobStatus.Failed, cancellationToken),
                CancelledJob = await query.CountAsync(x => x.JobStatus == AttendanceValueConstants.AttendanceSchedulerJobStatus.Cancelled, cancellationToken),
                DueJob = await query.CountAsync(x =>
                    (x.JobStatus == AttendanceValueConstants.AttendanceSchedulerJobStatus.Pending ||
                     x.JobStatus == AttendanceValueConstants.AttendanceSchedulerJobStatus.RetryScheduled) &&
                    x.AvailableAt <= now,
                    cancellationToken)
            };
        }

        public async Task<AttendanceSchedulerJobPagedResponse> GetPagedAsync(
            AttendanceSchedulerJobQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            NormalizePaging(request);
            var query = ApplyFilter(BuildBaseQuery(), request);
            var totalData = await query.CountAsync(cancellationToken);
            var items = await ApplySorting(query, request.SortBy, request.SortDirection)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new AttendanceSchedulerJobResponse
                {
                    Id = x.Id,
                    JobNumber = x.JobNumber,
                    JobType = x.JobType,
                    JobStatus = x.JobStatus,
                    AttendancePeriodId = x.AttendancePeriodId,
                    AttendancePeriodCode = x.AttendancePeriod != null ? x.AttendancePeriod.PeriodCode : null,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    WorkforceProfileId = x.WorkforceProfileId,
                    WorkforceProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : null,
                    WorkforceDisplayName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : null,
                    HospitalSiteId = x.HospitalSiteId,
                    HospitalSiteName = x.HospitalSite != null ? x.HospitalSite.SiteName : null,
                    OrganizationUnitId = x.OrganizationUnitId,
                    OrganizationUnitName = x.OrganizationUnit != null ? x.OrganizationUnit.UnitName : null,
                    DepartmentId = x.DepartmentId,
                    DepartmentName = x.Department != null ? x.Department.DepartmentName : null,
                    ForceReprocess = x.ForceReprocess,
                    Priority = x.Priority,
                    RetryCount = x.RetryCount,
                    MaxRetryCount = x.MaxRetryCount,
                    ScheduledAt = x.ScheduledAt,
                    AvailableAt = x.AvailableAt,
                    StartedAt = x.StartedAt,
                    CompletedAt = x.CompletedAt,
                    FailedAt = x.FailedAt,
                    NextRetryAt = x.NextRetryAt,
                    WorkerInstanceId = x.WorkerInstanceId,
                    ProcessingRunId = x.ProcessingRunId,
                    ProcessingRunNumber = x.ProcessingRun != null ? x.ProcessingRun.RunNumber : null,
                    CorrelationId = x.CorrelationId,
                    LastError = x.LastError,
                    Notes = x.Notes,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            return new AttendanceSchedulerJobPagedResponse
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)request.PageSize),
                Items = items
            };
        }

        public async Task<AttendanceSchedulerJobDetailResponse?> GetDetailAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new AttendanceSchedulerJobDetailResponse
                {
                    Id = x.Id,
                    JobNumber = x.JobNumber,
                    JobType = x.JobType,
                    JobStatus = x.JobStatus,
                    AttendancePeriodId = x.AttendancePeriodId,
                    AttendancePeriodCode = x.AttendancePeriod != null ? x.AttendancePeriod.PeriodCode : null,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    WorkforceProfileId = x.WorkforceProfileId,
                    WorkforceProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : null,
                    WorkforceDisplayName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : null,
                    HospitalSiteId = x.HospitalSiteId,
                    HospitalSiteName = x.HospitalSite != null ? x.HospitalSite.SiteName : null,
                    OrganizationUnitId = x.OrganizationUnitId,
                    OrganizationUnitName = x.OrganizationUnit != null ? x.OrganizationUnit.UnitName : null,
                    DepartmentId = x.DepartmentId,
                    DepartmentName = x.Department != null ? x.Department.DepartmentName : null,
                    ForceReprocess = x.ForceReprocess,
                    Priority = x.Priority,
                    RetryCount = x.RetryCount,
                    MaxRetryCount = x.MaxRetryCount,
                    ScheduledAt = x.ScheduledAt,
                    AvailableAt = x.AvailableAt,
                    StartedAt = x.StartedAt,
                    HeartbeatAt = x.HeartbeatAt,
                    CompletedAt = x.CompletedAt,
                    FailedAt = x.FailedAt,
                    CancelledAt = x.CancelledAt,
                    NextRetryAt = x.NextRetryAt,
                    WorkerInstanceId = x.WorkerInstanceId,
                    ProcessingRunId = x.ProcessingRunId,
                    ProcessingRunNumber = x.ProcessingRun != null ? x.ProcessingRun.RunNumber : null,
                    TriggeredByUserId = x.TriggeredByUserId,
                    TriggeredByUserName = x.TriggeredByUser != null ? x.TriggeredByUser.DisplayName ?? x.TriggeredByUser.UserName ?? x.TriggeredByUser.Email ?? x.TriggeredByUser.UserCode : null,
                    CancelledByUserId = x.CancelledByUserId,
                    CancelledByUserName = x.CancelledByUser != null ? x.CancelledByUser.DisplayName ?? x.CancelledByUser.UserName ?? x.CancelledByUser.Email ?? x.CancelledByUser.UserCode : null,
                    CorrelationId = x.CorrelationId,
                    ParametersJson = x.ParametersJson,
                    LastError = x.LastError,
                    Notes = x.Notes,
                    CreateDateTime = x.CreateDateTime,
                    UpdateDateTime = x.UpdateDateTime
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<AttendancePeriodSchedulerServiceResult<AttendanceSchedulerJobDetailResponse>> EnqueueAsync(
            EnqueueAttendanceProcessingJobRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var validation = ValidateEnqueueRequest(request);
            if (validation != null)
            {
                return AttendancePeriodSchedulerServiceResult<AttendanceSchedulerJobDetailResponse>.Fail(StatusCodes.Status400BadRequest, validation);
            }

            if (!string.IsNullOrWhiteSpace(request.CorrelationId))
            {
                var existing = await _dbContext.Set<HrdAttendanceSchedulerJob>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => !x.IsDelete && x.CorrelationId == request.CorrelationId.Trim(), cancellationToken);
                if (existing != null)
                {
                    var existingDetail = await GetDetailAsync(existing.Id, cancellationToken);
                    return AttendancePeriodSchedulerServiceResult<AttendanceSchedulerJobDetailResponse>.Ok(existingDetail!, "Scheduler job dengan correlation id tersebut sudah tersedia.");
                }
            }

            var now = DateTime.UtcNow;
            var job = new HrdAttendanceSchedulerJob
            {
                Id = Guid.NewGuid(),
                JobNumber = await GenerateJobNumberAsync(cancellationToken),
                JobType = request.ForceReprocess
                    ? AttendanceValueConstants.AttendanceSchedulerJobType.ReprocessRange
                    : AttendanceValueConstants.AttendanceSchedulerJobType.ProcessRange,
                JobStatus = AttendanceValueConstants.AttendanceSchedulerJobStatus.Pending,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                WorkforceProfileId = NormalizeGuid(request.WorkforceProfileId),
                HospitalSiteId = NormalizeGuid(request.HospitalSiteId),
                OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId),
                DepartmentId = NormalizeGuid(request.DepartmentId),
                ForceReprocess = request.ForceReprocess,
                Priority = Math.Clamp(request.Priority, 1, 1000),
                RetryCount = 0,
                MaxRetryCount = Math.Clamp(request.MaxRetryCount ?? _options.DefaultMaximumRetryCount, 0, 10),
                ScheduledAt = now,
                AvailableAt = NormalizeUtc(request.AvailableAt) ?? now,
                TriggeredByUserId = actorUserId == Guid.Empty ? null : actorUserId,
                CorrelationId = NormalizeString(request.CorrelationId),
                ParametersJson = JsonSerializer.Serialize(new
                {
                    request.StartDate,
                    request.EndDate,
                    request.WorkforceProfileId,
                    request.HospitalSiteId,
                    request.OrganizationUnitId,
                    request.DepartmentId,
                    request.ForceReprocess
                }),
                Notes = NormalizeString(request.Notes),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                UpdateBy = actorUserId
            };

            _dbContext.Set<HrdAttendanceSchedulerJob>().Add(job);
            await _dbContext.SaveChangesAsync(cancellationToken);
            var detail = await GetDetailAsync(job.Id, cancellationToken);
            return AttendancePeriodSchedulerServiceResult<AttendanceSchedulerJobDetailResponse>.Ok(detail!, "Attendance scheduler job berhasil dibuat.");
        }

        public async Task<AttendancePeriodSchedulerServiceResult<AttendanceSchedulerJobDetailResponse>> EnqueuePeriodAsync(
            Guid periodId,
            EnqueueAttendancePeriodProcessingRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var period = await _dbContext.Set<HrdAttendancePeriod>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == periodId && !x.IsDelete, cancellationToken);
            if (period == null)
            {
                return AttendancePeriodSchedulerServiceResult<AttendanceSchedulerJobDetailResponse>.Fail(StatusCodes.Status404NotFound, "Attendance period tidak ditemukan.");
            }
            if (period.PeriodStatus == AttendanceValueConstants.AttendancePeriodStatus.Closed ||
                period.PeriodStatus == AttendanceValueConstants.AttendancePeriodStatus.Cancelled)
            {
                return AttendancePeriodSchedulerServiceResult<AttendanceSchedulerJobDetailResponse>.Fail(StatusCodes.Status409Conflict, "Attendance period Closed atau Cancelled tidak dapat diproses ulang melalui scheduler.");
            }

            var correlation = $"ATT-PERIOD-{period.Id:N}-{(request.ForceReprocess ? "REPROCESS" : "PROCESS")}-{period.ReopenCount}";
            var enqueueResult = await EnqueueAsync(
                new EnqueueAttendanceProcessingJobRequest
                {
                    StartDate = period.StartDate,
                    EndDate = period.EndDate,
                    HospitalSiteId = period.HospitalSiteId,
                    OrganizationUnitId = period.OrganizationUnitId,
                    DepartmentId = period.DepartmentId,
                    ForceReprocess = request.ForceReprocess,
                    Priority = request.Priority,
                    CorrelationId = correlation,
                    Notes = request.Notes ?? $"Processing untuk attendance period {period.PeriodCode}."
                },
                actorUserId,
                cancellationToken);

            if (enqueueResult.Success && enqueueResult.Data != null)
            {
                var entity = await _dbContext.Set<HrdAttendanceSchedulerJob>()
                    .FirstAsync(x => x.Id == enqueueResult.Data.Id, cancellationToken);
                entity.AttendancePeriodId = period.Id;
                entity.UpdateDateTime = DateTime.UtcNow;
                entity.UpdateBy = actorUserId;
                await _dbContext.SaveChangesAsync(cancellationToken);
                enqueueResult.Data.AttendancePeriodId = period.Id;
                enqueueResult.Data.AttendancePeriodCode = period.PeriodCode;
            }

            return enqueueResult;
        }

        public async Task<AttendancePeriodSchedulerServiceResult<AttendanceSchedulerJobActionResponse>> CancelAsync(
            Guid id,
            CancelAttendanceSchedulerJobRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return AttendancePeriodSchedulerServiceResult<AttendanceSchedulerJobActionResponse>.Fail(StatusCodes.Status400BadRequest, "Alasan pembatalan wajib diisi.");
            }

            var job = await _dbContext.Set<HrdAttendanceSchedulerJob>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (job == null)
            {
                return AttendancePeriodSchedulerServiceResult<AttendanceSchedulerJobActionResponse>.Fail(StatusCodes.Status404NotFound, "Scheduler job tidak ditemukan.");
            }
            if (job.JobStatus == AttendanceValueConstants.AttendanceSchedulerJobStatus.Running)
            {
                return AttendancePeriodSchedulerServiceResult<AttendanceSchedulerJobActionResponse>.Fail(StatusCodes.Status409Conflict, "Scheduler job yang sedang Running tidak dapat dibatalkan melalui endpoint ini.");
            }
            if (IsTerminal(job.JobStatus))
            {
                return AttendancePeriodSchedulerServiceResult<AttendanceSchedulerJobActionResponse>.Fail(StatusCodes.Status409Conflict, "Scheduler job sudah berada pada status terminal.");
            }

            var previous = job.JobStatus;
            var now = DateTime.UtcNow;
            job.JobStatus = AttendanceValueConstants.AttendanceSchedulerJobStatus.Cancelled;
            job.CancelledAt = now;
            job.CancelledByUserId = actorUserId;
            job.Notes = AppendNote(job.Notes, $"Cancelled: {request.Reason.Trim()}");
            job.UpdateDateTime = now;
            job.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return AttendancePeriodSchedulerServiceResult<AttendanceSchedulerJobActionResponse>.Ok(
                MapAction(job, previous, now),
                "Attendance scheduler job berhasil dibatalkan.");
        }

        public async Task<AttendancePeriodSchedulerServiceResult<AttendanceSchedulerJobActionResponse>> RetryAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var job = await _dbContext.Set<HrdAttendanceSchedulerJob>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (job == null)
            {
                return AttendancePeriodSchedulerServiceResult<AttendanceSchedulerJobActionResponse>.Fail(StatusCodes.Status404NotFound, "Scheduler job tidak ditemukan.");
            }
            if (job.JobStatus != AttendanceValueConstants.AttendanceSchedulerJobStatus.Failed &&
                job.JobStatus != AttendanceValueConstants.AttendanceSchedulerJobStatus.CompletedWithErrors &&
                job.JobStatus != AttendanceValueConstants.AttendanceSchedulerJobStatus.Cancelled)
            {
                return AttendancePeriodSchedulerServiceResult<AttendanceSchedulerJobActionResponse>.Fail(StatusCodes.Status409Conflict, "Hanya scheduler job Failed, CompletedWithErrors, atau Cancelled yang dapat dijadwalkan ulang.");
            }

            var previous = job.JobStatus;
            var now = DateTime.UtcNow;
            job.JobStatus = AttendanceValueConstants.AttendanceSchedulerJobStatus.Pending;
            job.AvailableAt = now;
            job.NextRetryAt = null;
            job.StartedAt = null;
            job.CompletedAt = null;
            job.FailedAt = null;
            job.CancelledAt = null;
            job.CancelledByUserId = null;
            job.WorkerInstanceId = null;
            job.LastError = null;
            job.ProcessingRunId = null;
            job.RetryCount += 1;
            job.UpdateDateTime = now;
            job.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return AttendancePeriodSchedulerServiceResult<AttendanceSchedulerJobActionResponse>.Ok(
                MapAction(job, previous, now),
                "Attendance scheduler job berhasil dijadwalkan ulang.");
        }

        public async Task<int> RecoverStaleRunningJobsAsync(
            DateTime utcNow,
            CancellationToken cancellationToken = default)
        {
            var timeoutMinutes = Math.Clamp(_options.RunningJobTimeoutMinutes, 5, 1440);
            var threshold = utcNow.AddMinutes(-timeoutMinutes);
            var jobs = await _dbContext.Set<HrdAttendanceSchedulerJob>()
                .Where(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    x.JobStatus == AttendanceValueConstants.AttendanceSchedulerJobStatus.Running &&
                    (!x.HeartbeatAt.HasValue || x.HeartbeatAt < threshold))
                .ToListAsync(cancellationToken);

            foreach (var job in jobs)
            {
                job.RetryCount += 1;
                job.LastError = Limit(
                    $"Running job dianggap stale karena heartbeat lebih lama dari {timeoutMinutes} menit.",
                    4000);
                job.WorkerInstanceId = null;
                job.FailedAt = utcNow;
                job.NextRetryAt = utcNow;
                job.AvailableAt = utcNow;
                job.JobStatus = job.RetryCount <= job.MaxRetryCount
                    ? AttendanceValueConstants.AttendanceSchedulerJobStatus.RetryScheduled
                    : AttendanceValueConstants.AttendanceSchedulerJobStatus.Failed;
                job.UpdateDateTime = utcNow;
            }

            if (jobs.Count > 0)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return jobs.Count;
        }

        public async Task<int> ProcessScheduledPeriodClosuresAsync(
            DateTime utcNow,
            CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled || !_options.AutoClosePeriods)
            {
                return 0;
            }

            var periodIds = await _dbContext.Set<HrdAttendancePeriod>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    x.ScheduledCloseAt.HasValue &&
                    x.ScheduledCloseAt <= utcNow &&
                    (x.PeriodStatus == AttendanceValueConstants.AttendancePeriodStatus.Open ||
                     x.PeriodStatus == AttendanceValueConstants.AttendancePeriodStatus.Reopened))
                .OrderBy(x => x.ScheduledCloseAt)
                .Select(x => x.Id)
                .Take(10)
                .ToListAsync(cancellationToken);

            var actorUserId = ParseSystemActorUserId();
            var closedCount = 0;
            foreach (var periodId in periodIds)
            {
                var result = await _periodService.CloseAsync(
                    periodId,
                    new CloseAttendancePeriodRequest
                    {
                        Reason = "Automatic scheduled attendance period closing."
                    },
                    actorUserId,
                    cancellationToken);

                if (result.Success)
                {
                    closedCount++;
                }
                else
                {
                    _logger.LogWarning(
                        "Automatic closing attendance period {AttendancePeriodId} belum berhasil: {Message}",
                        periodId,
                        result.Message);
                }
            }

            return closedCount;
        }

        public async Task<int> EnsureAutomaticDailyJobsAsync(
            DateTime utcNow,
            CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled || !_options.AutoEnqueueDailyProcessing)
            {
                return 0;
            }

            var timeZone = ResolveTimeZone(_options.TimeZoneId);
            var localNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone);
            var cutoff = new TimeOnly(
                Math.Clamp(_options.DailyProcessingHour, 0, 23),
                Math.Clamp(_options.DailyProcessingMinute, 0, 59));
            if (TimeOnly.FromDateTime(localNow) < cutoff)
            {
                return 0;
            }

            var created = 0;
            var maximumCatchUpDays = Math.Clamp(_options.MaximumCatchUpDays, 1, 31);
            var processDaysBack = Math.Clamp(_options.ProcessDaysBack, 1, maximumCatchUpDays);
            var actorUserId = ParseSystemActorUserId();

            for (var offset = processDaysBack; offset <= maximumCatchUpDays; offset++)
            {
                var date = DateOnly.FromDateTime(localNow.Date.AddDays(-offset));
                var correlationId = $"ATT-AUTO-DAILY-{date:yyyyMMdd}";
                var exists = await _dbContext.Set<HrdAttendanceSchedulerJob>()
                    .AsNoTracking()
                    .AnyAsync(x => !x.IsDelete && x.CorrelationId == correlationId, cancellationToken);
                if (exists)
                {
                    continue;
                }

                var result = await EnqueueAsync(
                    new EnqueueAttendanceProcessingJobRequest
                    {
                        StartDate = date,
                        EndDate = date,
                        ForceReprocess = false,
                        Priority = 100,
                        MaxRetryCount = _options.DefaultMaximumRetryCount,
                        CorrelationId = correlationId,
                        Notes = "Automatic daily attendance processing."
                    },
                    actorUserId,
                    cancellationToken);
                if (result.Success)
                {
                    created++;
                }
            }

            return created;
        }

        public async Task<HrdAttendanceSchedulerJob?> ClaimNextJobAsync(
            string workerInstanceId,
            CancellationToken cancellationToken = default)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, cancellationToken);
            var job = await _dbContext.Set<HrdAttendanceSchedulerJob>()
                .FromSqlRaw(
                    "SELECT * FROM \"HrdAttendanceSchedulerJob\" " +
                    "WHERE \"IsDelete\" = false AND \"IsActive\" = true " +
                    "AND (\"JobStatus\" = {0} OR \"JobStatus\" = {1}) " +
                    "AND \"AvailableAt\" <= CURRENT_TIMESTAMP " +
                    "ORDER BY \"Priority\" ASC, \"AvailableAt\" ASC, \"CreateDateTime\" ASC " +
                    "FOR UPDATE SKIP LOCKED LIMIT 1",
                    AttendanceValueConstants.AttendanceSchedulerJobStatus.Pending,
                    AttendanceValueConstants.AttendanceSchedulerJobStatus.RetryScheduled)
                .FirstOrDefaultAsync(cancellationToken);

            if (job == null)
            {
                await transaction.CommitAsync(cancellationToken);
                return null;
            }

            var now = DateTime.UtcNow;
            job.JobStatus = AttendanceValueConstants.AttendanceSchedulerJobStatus.Running;
            job.StartedAt = now;
            job.HeartbeatAt = now;
            job.WorkerInstanceId = Limit(workerInstanceId, 200);
            job.UpdateDateTime = now;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return job;
        }

        public async Task ExecuteClaimedJobAsync(
            Guid jobId,
            CancellationToken cancellationToken = default)
        {
            var job = await _dbContext.Set<HrdAttendanceSchedulerJob>()
                .FirstOrDefaultAsync(x => x.Id == jobId && !x.IsDelete, cancellationToken);
            if (job == null || job.JobStatus != AttendanceValueConstants.AttendanceSchedulerJobStatus.Running)
            {
                return;
            }

            try
            {
                job.HeartbeatAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);

                var processingCorrelation = $"{job.JobNumber}-TRY-{job.RetryCount + 1}";
                var result = await _processingService.ProcessRangeAsync(
                    new ProcessAttendanceRangeRequest
                    {
                        StartDate = job.StartDate,
                        EndDate = job.EndDate,
                        WorkforceProfileId = job.WorkforceProfileId,
                        HospitalSiteId = job.HospitalSiteId,
                        OrganizationUnitId = job.OrganizationUnitId,
                        DepartmentId = job.DepartmentId,
                        ForceReprocess = job.ForceReprocess,
                        TriggerSource = AttendanceValueConstants.ProcessingTriggerSource.Scheduler,
                        CorrelationId = processingCorrelation,
                        Notes = job.Notes
                    },
                    job.TriggeredByUserId ?? ParseSystemActorUserId(),
                    cancellationToken);

                var now = DateTime.UtcNow;
                if (!result.Success || result.Data == null)
                {
                    await HandleExecutionFailureAsync(job, result.Message, now, cancellationToken);
                    return;
                }

                job.ProcessingRunId = result.Data.ProcessingRunId;
                job.CompletedAt = now;
                job.HeartbeatAt = now;
                job.LastError = null;
                job.NextRetryAt = null;
                job.JobStatus = result.Data.RunStatus == AttendanceValueConstants.ProcessingRunStatus.Completed
                    ? AttendanceValueConstants.AttendanceSchedulerJobStatus.Completed
                    : AttendanceValueConstants.AttendanceSchedulerJobStatus.CompletedWithErrors;
                job.UpdateDateTime = now;
                await _dbContext.SaveChangesAsync(cancellationToken);

                if (job.AttendancePeriodId.HasValue)
                {
                    var period = await _dbContext.Set<HrdAttendancePeriod>()
                        .FirstOrDefaultAsync(x => x.Id == job.AttendancePeriodId.Value && !x.IsDelete, cancellationToken);
                    if (period != null)
                    {
                        period.LastProcessingRunId = result.Data.ProcessingRunId;
                        period.UpdateDateTime = now;
                        period.UpdateBy = job.TriggeredByUserId ?? Guid.Empty;
                        await _dbContext.SaveChangesAsync(cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Attendance scheduler job {JobId} gagal.", jobId);
                await HandleExecutionFailureAsync(job, ex.Message, DateTime.UtcNow, CancellationToken.None);
            }
        }

        private async Task HandleExecutionFailureAsync(
            HrdAttendanceSchedulerJob job,
            string error,
            DateTime now,
            CancellationToken cancellationToken)
        {
            job.RetryCount += 1;
            job.FailedAt = now;
            job.LastError = Limit(error, 4000);
            job.WorkerInstanceId = null;
            job.HeartbeatAt = now;
            if (job.RetryCount <= job.MaxRetryCount)
            {
                var retryAt = now.AddMinutes(Math.Max(1, _options.RetryDelayMinutes));
                job.JobStatus = AttendanceValueConstants.AttendanceSchedulerJobStatus.RetryScheduled;
                job.NextRetryAt = retryAt;
                job.AvailableAt = retryAt;
            }
            else
            {
                job.JobStatus = AttendanceValueConstants.AttendanceSchedulerJobStatus.Failed;
                job.NextRetryAt = null;
            }
            job.UpdateDateTime = now;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private IQueryable<HrdAttendanceSchedulerJob> BuildBaseQuery() =>
            _dbContext.Set<HrdAttendanceSchedulerJob>().AsNoTracking().Where(x => !x.IsDelete);

        private static IQueryable<HrdAttendanceSchedulerJob> ApplyFilter(
            IQueryable<HrdAttendanceSchedulerJob> query,
            AttendanceSchedulerJobQueryRequest request)
        {
            if (request.StartDate.HasValue) query = query.Where(x => x.EndDate >= request.StartDate.Value);
            if (request.EndDate.HasValue) query = query.Where(x => x.StartDate <= request.EndDate.Value);
            if (!string.IsNullOrWhiteSpace(request.JobType)) query = query.Where(x => x.JobType == request.JobType.Trim());
            if (!string.IsNullOrWhiteSpace(request.JobStatus)) query = query.Where(x => x.JobStatus == request.JobStatus.Trim());
            if (request.AttendancePeriodId.HasValue) query = query.Where(x => x.AttendancePeriodId == request.AttendancePeriodId);
            if (request.WorkforceProfileId.HasValue) query = query.Where(x => x.WorkforceProfileId == request.WorkforceProfileId);
            if (request.HospitalSiteId.HasValue) query = query.Where(x => x.HospitalSiteId == request.HospitalSiteId);
            if (request.OrganizationUnitId.HasValue) query = query.Where(x => x.OrganizationUnitId == request.OrganizationUnitId);
            if (request.DepartmentId.HasValue) query = query.Where(x => x.DepartmentId == request.DepartmentId);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim().ToLower();
                query = query.Where(x =>
                    x.JobNumber.ToLower().Contains(keyword) ||
                    (x.CorrelationId != null && x.CorrelationId.ToLower().Contains(keyword)) ||
                    (x.Notes != null && x.Notes.ToLower().Contains(keyword)));
            }
            return query;
        }

        private static IOrderedQueryable<HrdAttendanceSchedulerJob> ApplySorting(
            IQueryable<HrdAttendanceSchedulerJob> query,
            string? sortBy,
            string? direction)
        {
            var desc = string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "scheduledAt").Trim().ToLowerInvariant() switch
            {
                "availableat" => desc ? query.OrderByDescending(x => x.AvailableAt) : query.OrderBy(x => x.AvailableAt),
                "jobnumber" => desc ? query.OrderByDescending(x => x.JobNumber) : query.OrderBy(x => x.JobNumber),
                "jobstatus" => desc ? query.OrderByDescending(x => x.JobStatus) : query.OrderBy(x => x.JobStatus),
                "priority" => desc ? query.OrderByDescending(x => x.Priority) : query.OrderBy(x => x.Priority),
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => desc ? query.OrderByDescending(x => x.ScheduledAt) : query.OrderBy(x => x.ScheduledAt)
            };
        }

        private static string? ValidateEnqueueRequest(EnqueueAttendanceProcessingJobRequest request)
        {
            if (request.EndDate < request.StartDate) return "Tanggal selesai tidak boleh lebih kecil dari tanggal mulai.";
            var days = request.EndDate.DayNumber - request.StartDate.DayNumber + 1;
            if (days > 31) return "Rentang scheduler job maksimal 31 hari.";
            if (request.Priority < 1 || request.Priority > 1000) return "Priority harus berada pada rentang 1 sampai 1000.";
            return null;
        }

        private async Task<string> GenerateJobNumberAsync(CancellationToken cancellationToken)
        {
            var prefix = $"ATJ-{DateTime.UtcNow:yyyyMMdd}-";
            var count = await _dbContext.Set<HrdAttendanceSchedulerJob>()
                .AsNoTracking()
                .CountAsync(x => !x.IsDelete && x.JobNumber.StartsWith(prefix), cancellationToken);
            return prefix + (count + 1).ToString("D6") + "-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        }

        private Guid ParseSystemActorUserId()
        {
            return Guid.TryParse(_options.SystemActorUserId, out var id) ? id : Guid.Empty;
        }

        private static TimeZoneInfo ResolveTimeZone(string? id)
        {
            var candidates = new List<string>();
            if (!string.IsNullOrWhiteSpace(id)) candidates.Add(id.Trim());
            candidates.Add("Asia/Jakarta");
            candidates.Add("SE Asia Standard Time");
            foreach (var candidate in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                try { return TimeZoneInfo.FindSystemTimeZoneById(candidate); }
                catch (TimeZoneNotFoundException) { }
                catch (InvalidTimeZoneException) { }
            }
            return TimeZoneInfo.Utc;
        }

        private static bool IsTerminal(string status) =>
            status == AttendanceValueConstants.AttendanceSchedulerJobStatus.Completed ||
            status == AttendanceValueConstants.AttendanceSchedulerJobStatus.CompletedWithErrors ||
            status == AttendanceValueConstants.AttendanceSchedulerJobStatus.Failed ||
            status == AttendanceValueConstants.AttendanceSchedulerJobStatus.Cancelled;

        private static AttendanceSchedulerJobActionResponse MapAction(
            HrdAttendanceSchedulerJob job,
            string previousStatus,
            DateTime actionAt) => new()
        {
            JobId = job.Id,
            JobNumber = job.JobNumber,
            PreviousStatus = previousStatus,
            CurrentStatus = job.JobStatus,
            RetryCount = job.RetryCount,
            ActionAt = actionAt
        };

        private static void NormalizePaging(AttendanceSchedulerJobQueryRequest request)
        {
            request.PageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            request.PageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, 100);
        }

        private static Guid? NormalizeGuid(Guid? value) =>
            !value.HasValue || value.Value == Guid.Empty ? null : value.Value;

        private static string? NormalizeString(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static DateTime? NormalizeUtc(DateTime? value)
        {
            if (!value.HasValue) return null;
            return value.Value.Kind == DateTimeKind.Utc ? value.Value : value.Value.ToUniversalTime();
        }

        private static string AppendNote(string? existing, string note)
        {
            var value = string.IsNullOrWhiteSpace(existing) ? note : existing + Environment.NewLine + note;
            return Limit(value, 1000);
        }

        private static string Limit(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Length <= maxLength ? value : value[..maxLength];
        }
    }
}
