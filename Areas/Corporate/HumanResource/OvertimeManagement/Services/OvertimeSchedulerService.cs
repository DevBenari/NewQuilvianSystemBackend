using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using System.Data;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public class OvertimeSchedulerService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly OvertimeActualCalculationService _calculationService;
        private readonly OvertimeCompensatoryExpiryService _expiryService;
        private readonly OvertimeFinalReconciliationService _reconciliationService;
        private readonly OvertimePeriodService _periodService;
        private readonly OvertimeSchedulerOptions _options;
        private readonly ILogger<OvertimeSchedulerService> _logger;

        public OvertimeSchedulerService(
            ApplicationDbContext dbContext,
            OvertimeActualCalculationService calculationService,
            OvertimeCompensatoryExpiryService expiryService,
            OvertimeFinalReconciliationService reconciliationService,
            OvertimePeriodService periodService,
            IOptions<OvertimeSchedulerOptions> options,
            ILogger<OvertimeSchedulerService> logger)
        {
            _dbContext = dbContext;
            _calculationService = calculationService;
            _expiryService = expiryService;
            _reconciliationService = reconciliationService;
            _periodService = periodService;
            _options = options.Value;
            _logger = logger;
        }

        public OvertimeSchedulerFilterMetadataResponse GetMetadata() => new()
        {
            JobTypes = OvertimeValueConstants.SchedulerJobType.All.ToList(),
            JobStatuses = OvertimeValueConstants.SchedulerJobStatus.All.ToList(),
            SortFields = new List<string>
            {
                "scheduledAt", "availableAt", "jobNumber", "jobType", "jobStatus", "priority", "createDateTime"
            },
            PageSizeOptions = new List<int> { 10, 25, 50, 100 }
        };

        public async Task<OvertimeSchedulerSummaryResponse> GetSummaryAsync(
            OvertimeSchedulerJobQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter(BuildBaseQuery(), request);
            return new OvertimeSchedulerSummaryResponse
            {
                TotalJob = await query.CountAsync(cancellationToken),
                Pending = await query.CountAsync(x => x.JobStatus == OvertimeValueConstants.SchedulerJobStatus.Pending, cancellationToken),
                Running = await query.CountAsync(x => x.JobStatus == OvertimeValueConstants.SchedulerJobStatus.Running, cancellationToken),
                RetryScheduled = await query.CountAsync(x => x.JobStatus == OvertimeValueConstants.SchedulerJobStatus.RetryScheduled, cancellationToken),
                Completed = await query.CountAsync(x => x.JobStatus == OvertimeValueConstants.SchedulerJobStatus.Completed, cancellationToken),
                CompletedWithIssues = await query.CountAsync(x => x.JobStatus == OvertimeValueConstants.SchedulerJobStatus.CompletedWithIssues, cancellationToken),
                Failed = await query.CountAsync(x => x.JobStatus == OvertimeValueConstants.SchedulerJobStatus.Failed, cancellationToken),
                Cancelled = await query.CountAsync(x => x.JobStatus == OvertimeValueConstants.SchedulerJobStatus.Cancelled, cancellationToken)
            };
        }

        public async Task<PagedResult<OvertimeSchedulerJobListResponse>> GetPagedAsync(
            OvertimeSchedulerJobQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            NormalizePaging(request);
            var query = ApplySorting(ApplyFilter(BuildBaseQuery(), request), request.SortBy, request.SortDirection);
            var totalData = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new OvertimeSchedulerJobListResponse
                {
                    Id = x.Id,
                    JobNumber = x.JobNumber,
                    JobType = x.JobType,
                    JobStatus = x.JobStatus,
                    OvertimePeriodId = x.OvertimePeriodId,
                    OvertimePeriodCode = x.OvertimePeriod != null ? x.OvertimePeriod.PeriodCode : null,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    WorkforceProfileId = x.WorkforceProfileId,
                    HospitalSiteId = x.HospitalSiteId,
                    OrganizationUnitId = x.OrganizationUnitId,
                    DepartmentId = x.DepartmentId,
                    AllowRepair = x.AllowRepair,
                    ForceRecalculate = x.ForceRecalculate,
                    Priority = x.Priority,
                    RetryCount = x.RetryCount,
                    MaxRetryCount = x.MaxRetryCount,
                    ScheduledAt = x.ScheduledAt,
                    AvailableAt = x.AvailableAt,
                    StartedAt = x.StartedAt,
                    CompletedAt = x.CompletedAt,
                    FailedAt = x.FailedAt,
                    NextRetryAt = x.NextRetryAt,
                    CorrelationId = x.CorrelationId,
                    LastError = x.LastError
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<OvertimeSchedulerJobListResponse>
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)request.PageSize),
                Items = items
            };
        }

        public async Task<OvertimeSchedulerJobDetailResponse?> GetDetailAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new OvertimeSchedulerJobDetailResponse
                {
                    Id = x.Id,
                    JobNumber = x.JobNumber,
                    JobType = x.JobType,
                    JobStatus = x.JobStatus,
                    OvertimePeriodId = x.OvertimePeriodId,
                    OvertimePeriodCode = x.OvertimePeriod != null ? x.OvertimePeriod.PeriodCode : null,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    WorkforceProfileId = x.WorkforceProfileId,
                    HospitalSiteId = x.HospitalSiteId,
                    OrganizationUnitId = x.OrganizationUnitId,
                    DepartmentId = x.DepartmentId,
                    AllowRepair = x.AllowRepair,
                    ForceRecalculate = x.ForceRecalculate,
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
                    TriggeredByUserId = x.TriggeredByUserId,
                    TriggeredByUserName = x.TriggeredByUser != null ? x.TriggeredByUser.DisplayName ?? x.TriggeredByUser.UserName ?? x.TriggeredByUser.Email : null,
                    CancelledByUserId = x.CancelledByUserId,
                    CancelledByUserName = x.CancelledByUser != null ? x.CancelledByUser.DisplayName ?? x.CancelledByUser.UserName ?? x.CancelledByUser.Email : null,
                    CorrelationId = x.CorrelationId,
                    ParametersJson = x.ParametersJson,
                    ResultJson = x.ResultJson,
                    LastError = x.LastError,
                    Notes = x.Notes,
                    CreateDateTime = x.CreateDateTime,
                    UpdateDateTime = x.UpdateDateTime
                })
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<OvertimeClosingServiceResult<OvertimeSchedulerJobDetailResponse>> EnqueueAsync(
            EnqueueOvertimeSchedulerJobRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var error = ValidateEnqueue(request);
            if (error != null) return OvertimeClosingServiceResult<OvertimeSchedulerJobDetailResponse>.Fail(StatusCodes.Status400BadRequest, error);

            if (!string.IsNullOrWhiteSpace(request.CorrelationId))
            {
                var existing = await _dbContext.Set<TrxOvertimeSchedulerJob>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => !x.IsDelete && x.CorrelationId == request.CorrelationId.Trim(), cancellationToken);
                if (existing != null)
                {
                    var existingDetail = await GetDetailAsync(existing.Id, cancellationToken);
                    return OvertimeClosingServiceResult<OvertimeSchedulerJobDetailResponse>.Ok(existingDetail!, "Scheduler job dengan correlation id tersebut sudah tersedia.");
                }
            }

            var now = DateTime.UtcNow;
            var entity = new TrxOvertimeSchedulerJob
            {
                Id = Guid.NewGuid(),
                JobNumber = await GenerateJobNumberAsync(cancellationToken),
                JobType = NormalizeJobType(request.JobType)!,
                JobStatus = OvertimeValueConstants.SchedulerJobStatus.Pending,
                OvertimePeriodId = NormalizeGuid(request.OvertimePeriodId),
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                WorkforceProfileId = NormalizeGuid(request.WorkforceProfileId),
                LegalEntityId = NormalizeGuid(request.LegalEntityId),
                HospitalSiteId = NormalizeGuid(request.HospitalSiteId),
                OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId),
                DepartmentId = NormalizeGuid(request.DepartmentId),
                AllowRepair = request.AllowRepair,
                ForceRecalculate = request.ForceRecalculate,
                Priority = Math.Clamp(request.Priority, 1, 1000),
                RetryCount = 0,
                MaxRetryCount = Math.Clamp(request.MaxRetryCount ?? _options.DefaultMaximumRetryCount, 0, 10),
                ScheduledAt = now,
                AvailableAt = NormalizeUtc(request.AvailableAt) ?? now,
                TriggeredByUserId = actorUserId == Guid.Empty ? null : actorUserId,
                CorrelationId = NormalizeText(request.CorrelationId, 120),
                ParametersJson = JsonSerializer.Serialize(new
                {
                    request.JobType,
                    request.OvertimePeriodId,
                    request.StartDate,
                    request.EndDate,
                    request.WorkforceProfileId,
                    request.LegalEntityId,
                    request.HospitalSiteId,
                    request.OrganizationUnitId,
                    request.DepartmentId,
                    request.AllowRepair,
                    request.ForceRecalculate
                }),
                Notes = NormalizeText(request.Notes, 1000),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                UpdateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<TrxOvertimeSchedulerJob>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            var detail = await GetDetailAsync(entity.Id, cancellationToken);
            return OvertimeClosingServiceResult<OvertimeSchedulerJobDetailResponse>.Ok(
                detail!,
                "Overtime scheduler job berhasil dibuat.",
                StatusCodes.Status201Created);
        }

        public async Task<OvertimeClosingServiceResult<OvertimeSchedulerJobDetailResponse>> EnqueuePeriodAsync(
            Guid periodId,
            EnqueueOvertimePeriodJobRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var period = await _dbContext.Set<TrxOvertimePeriod>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == periodId && !x.IsDelete, cancellationToken);
            if (period == null) return OvertimeClosingServiceResult<OvertimeSchedulerJobDetailResponse>.Fail(StatusCodes.Status404NotFound, "Overtime period tidak ditemukan.");

            var jobType = NormalizeJobType(request.JobType);
            if (jobType == null) return OvertimeClosingServiceResult<OvertimeSchedulerJobDetailResponse>.Fail(StatusCodes.Status400BadRequest, "JobType tidak valid.");
            var correlation = NormalizeText(request.CorrelationId, 120);
            correlation ??= jobType == OvertimeValueConstants.SchedulerJobType.ClosePeriod
                ? $"OTP-PERIOD-CLOSE-{period.Id:N}-{period.CloseVersion}-{period.ReopenCount}"
                : $"OTP-PERIOD-{period.Id:N}-{jobType}-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6]}";
            return await EnqueueAsync(new EnqueueOvertimeSchedulerJobRequest
            {
                JobType = jobType,
                OvertimePeriodId = period.Id,
                StartDate = period.StartDate,
                EndDate = period.EndDate,
                LegalEntityId = period.LegalEntityId,
                HospitalSiteId = period.HospitalSiteId,
                OrganizationUnitId = period.OrganizationUnitId,
                DepartmentId = period.DepartmentId,
                AllowRepair = request.AllowRepair,
                ForceRecalculate = request.ForceRecalculate,
                Priority = request.Priority,
                CorrelationId = correlation,
                Notes = request.Notes ?? $"Scheduler job {jobType} untuk overtime period {period.PeriodCode}."
            }, actorUserId, cancellationToken);
        }

        public async Task<OvertimeClosingServiceResult<OvertimeSchedulerJobActionResponse>> CancelAsync(
            Guid id,
            CancelOvertimeSchedulerJobRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
                return OvertimeClosingServiceResult<OvertimeSchedulerJobActionResponse>.Fail(StatusCodes.Status400BadRequest, "Alasan pembatalan wajib diisi.");
            var job = await _dbContext.Set<TrxOvertimeSchedulerJob>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (job == null) return OvertimeClosingServiceResult<OvertimeSchedulerJobActionResponse>.Fail(StatusCodes.Status404NotFound, "Scheduler job tidak ditemukan.");
            if (job.JobStatus == OvertimeValueConstants.SchedulerJobStatus.Running)
                return OvertimeClosingServiceResult<OvertimeSchedulerJobActionResponse>.Fail(StatusCodes.Status409Conflict, "Job yang sedang Running tidak dapat dibatalkan melalui endpoint ini.");
            if (IsTerminal(job.JobStatus))
                return OvertimeClosingServiceResult<OvertimeSchedulerJobActionResponse>.Fail(StatusCodes.Status409Conflict, "Scheduler job sudah berada pada status terminal.");

            var previous = job.JobStatus;
            var now = DateTime.UtcNow;
            job.JobStatus = OvertimeValueConstants.SchedulerJobStatus.Cancelled;
            job.CancelledAt = now;
            job.CancelledByUserId = actorUserId;
            job.IsCancel = true;
            job.CancelDateTime = now;
            job.CancelBy = actorUserId;
            job.IsActive = false;
            job.Notes = AppendText(job.Notes, "Cancelled: " + request.Reason.Trim(), 1000);
            job.UpdateDateTime = now;
            job.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return OvertimeClosingServiceResult<OvertimeSchedulerJobActionResponse>.Ok(MapAction(job, previous, now), "Scheduler job berhasil dibatalkan.");
        }

        public async Task<OvertimeClosingServiceResult<OvertimeSchedulerJobActionResponse>> RetryAsync(
            Guid id,
            RetryOvertimeSchedulerJobRequest? request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            request ??= new RetryOvertimeSchedulerJobRequest();
            var job = await _dbContext.Set<TrxOvertimeSchedulerJob>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (job == null) return OvertimeClosingServiceResult<OvertimeSchedulerJobActionResponse>.Fail(StatusCodes.Status404NotFound, "Scheduler job tidak ditemukan.");
            if (job.JobStatus != OvertimeValueConstants.SchedulerJobStatus.Failed && job.JobStatus != OvertimeValueConstants.SchedulerJobStatus.CompletedWithIssues)
                return OvertimeClosingServiceResult<OvertimeSchedulerJobActionResponse>.Fail(StatusCodes.Status409Conflict, "Hanya job Failed atau CompletedWithIssues yang dapat dijadwalkan ulang.");

            var previous = job.JobStatus;
            var now = DateTime.UtcNow;
            job.JobStatus = OvertimeValueConstants.SchedulerJobStatus.RetryScheduled;
            job.AvailableAt = NormalizeUtc(request.AvailableAt) ?? now;
            job.NextRetryAt = job.AvailableAt;
            job.WorkerInstanceId = null;
            job.StartedAt = null;
            job.CompletedAt = null;
            job.FailedAt = null;
            job.LastError = null;
            job.IsActive = true;
            job.IsCancel = false;
            job.Notes = AppendText(job.Notes, request.Notes ?? "Manual retry.", 1000);
            job.UpdateDateTime = now;
            job.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return OvertimeClosingServiceResult<OvertimeSchedulerJobActionResponse>.Ok(MapAction(job, previous, now), "Scheduler job berhasil dijadwalkan ulang.");
        }

        public async Task<int> RecoverStaleRunningJobsAsync(
            DateTime utcNow,
            CancellationToken cancellationToken = default)
        {
            var threshold = utcNow.AddMinutes(-Math.Clamp(_options.RunningJobTimeoutMinutes, 5, 1440));
            var jobs = await _dbContext.Set<TrxOvertimeSchedulerJob>()
                .Where(x =>
                    !x.IsDelete && x.IsActive &&
                    x.JobStatus == OvertimeValueConstants.SchedulerJobStatus.Running &&
                    (!x.HeartbeatAt.HasValue || x.HeartbeatAt < threshold))
                .ToListAsync(cancellationToken);

            foreach (var job in jobs)
            {
                job.RetryCount += 1;
                job.LastError = Limit($"Running job dianggap stale karena heartbeat lebih lama dari {_options.RunningJobTimeoutMinutes} menit.", 4000);
                job.WorkerInstanceId = null;
                job.FailedAt = utcNow;
                job.NextRetryAt = utcNow;
                job.AvailableAt = utcNow;
                job.JobStatus = job.RetryCount <= job.MaxRetryCount
                    ? OvertimeValueConstants.SchedulerJobStatus.RetryScheduled
                    : OvertimeValueConstants.SchedulerJobStatus.Failed;
                job.UpdateDateTime = utcNow;
            }
            if (jobs.Count > 0) await _dbContext.SaveChangesAsync(cancellationToken);
            return jobs.Count;
        }

        public async Task<int> EnsureAutomaticDailyJobsAsync(
            DateTime utcNow,
            CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled || !_options.AutoEnqueueDailyCycle) return 0;
            var timeZone = ResolveTimeZone(_options.TimeZoneId);
            var localNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone);
            var cutoff = new TimeOnly(Math.Clamp(_options.DailyCycleHour, 0, 23), Math.Clamp(_options.DailyCycleMinute, 0, 59));
            if (TimeOnly.FromDateTime(localNow) < cutoff) return 0;

            var created = 0;
            var actor = ParseSystemActorUserId();
            var maxCatchUp = Math.Clamp(_options.MaximumCatchUpDays, 1, 31);
            var startBack = Math.Clamp(_options.ProcessDaysBack, 1, maxCatchUp);
            for (var offset = startBack; offset <= maxCatchUp; offset++)
            {
                var date = DateOnly.FromDateTime(localNow.Date.AddDays(-offset));
                var correlation = $"OTP-AUTO-DAILY-{date:yyyyMMdd}";
                var exists = await _dbContext.Set<TrxOvertimeSchedulerJob>()
                    .AsNoTracking()
                    .AnyAsync(x => !x.IsDelete && x.CorrelationId == correlation, cancellationToken);
                if (exists) continue;

                var result = await EnqueueAsync(new EnqueueOvertimeSchedulerJobRequest
                {
                    JobType = OvertimeValueConstants.SchedulerJobType.FullCycle,
                    StartDate = date,
                    EndDate = date,
                    AllowRepair = true,
                    Priority = 100,
                    CorrelationId = correlation,
                    Notes = "Automatic daily overtime lifecycle cycle."
                }, actor, cancellationToken);
                if (result.Success) created++;
            }
            return created;
        }

        public async Task<int> ProcessScheduledPeriodClosuresAsync(
            DateTime utcNow,
            CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled || !_options.AutoClosePeriods) return 0;
            var periods = await _dbContext.Set<TrxOvertimePeriod>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete && x.IsActive && x.ScheduledCloseAt.HasValue && x.ScheduledCloseAt <= utcNow &&
                    (x.PeriodStatus == OvertimeValueConstants.PeriodStatus.Open || x.PeriodStatus == OvertimeValueConstants.PeriodStatus.Reopened))
                .OrderBy(x => x.ScheduledCloseAt)
                .Take(10)
                .ToListAsync(cancellationToken);

            var created = 0;
            var actor = ParseSystemActorUserId();
            foreach (var period in periods)
            {
                var result = await EnqueuePeriodAsync(period.Id, new EnqueueOvertimePeriodJobRequest
                {
                    JobType = OvertimeValueConstants.SchedulerJobType.ClosePeriod,
                    AllowRepair = true,
                    Priority = 10,
                    CorrelationId = $"OTP-PERIOD-AUTO-CLOSE-{period.Id:N}-{period.CloseVersion}-{period.ReopenCount}",
                    Notes = "Automatic scheduled overtime period closing."
                }, actor, cancellationToken);
                if (result.Success) created++;
            }
            return created;
        }

        public async Task<TrxOvertimeSchedulerJob?> ClaimNextJobAsync(
            string workerInstanceId,
            CancellationToken cancellationToken = default)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            var job = await _dbContext.Set<TrxOvertimeSchedulerJob>()
                .FromSqlRaw(
                    "SELECT * FROM \"TrxOvertimeSchedulerJob\" " +
                    "WHERE \"IsDelete\" = false AND \"IsActive\" = true " +
                    "AND (\"JobStatus\" = {0} OR \"JobStatus\" = {1}) " +
                    "AND \"AvailableAt\" <= CURRENT_TIMESTAMP " +
                    "ORDER BY \"Priority\" ASC, \"AvailableAt\" ASC, \"CreateDateTime\" ASC " +
                    "FOR UPDATE SKIP LOCKED LIMIT 1",
                    OvertimeValueConstants.SchedulerJobStatus.Pending,
                    OvertimeValueConstants.SchedulerJobStatus.RetryScheduled)
                .FirstOrDefaultAsync(cancellationToken);

            if (job == null)
            {
                await transaction.CommitAsync(cancellationToken);
                return null;
            }

            var now = DateTime.UtcNow;
            job.JobStatus = OvertimeValueConstants.SchedulerJobStatus.Running;
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
            var job = await _dbContext.Set<TrxOvertimeSchedulerJob>()
                .FirstOrDefaultAsync(x => x.Id == jobId && !x.IsDelete, cancellationToken);
            if (job == null || job.JobStatus != OvertimeValueConstants.SchedulerJobStatus.Running) return;

            try
            {
                job.HeartbeatAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);
                var actor = job.TriggeredByUserId ?? ParseSystemActorUserId();
                if (actor == Guid.Empty) throw new InvalidOperationException("HumanResource:OvertimeScheduler:SystemActorUserId belum dikonfigurasi.");

                var result = await ExecuteJobCoreAsync(job, actor, cancellationToken);
                var now = DateTime.UtcNow;
                job.ResultJson = JsonSerializer.Serialize(result);
                job.CompletedAt = now;
                job.HeartbeatAt = now;
                job.LastError = null;
                job.NextRetryAt = null;
                job.WorkerInstanceId = null;
                job.JobStatus = result.FailedCount > 0 || result.WarningCount > 0 || result.Reconciliation?.BlockingCount > 0
                    ? OvertimeValueConstants.SchedulerJobStatus.CompletedWithIssues
                    : OvertimeValueConstants.SchedulerJobStatus.Completed;
                job.UpdateDateTime = now;
                job.UpdateBy = actor;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Overtime scheduler job {JobId} gagal.", jobId);
                await HandleFailureAsync(job, ex.Message, DateTime.UtcNow, CancellationToken.None);
            }
        }

        private async Task<OvertimeSchedulerExecutionResponse> ExecuteJobCoreAsync(
            TrxOvertimeSchedulerJob job,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var response = new OvertimeSchedulerExecutionResponse
            {
                JobId = job.Id,
                JobNumber = job.JobNumber,
                JobType = job.JobType
            };

            if (job.JobType == OvertimeValueConstants.SchedulerJobType.AutoCalculate ||
                job.JobType == OvertimeValueConstants.SchedulerJobType.FullCycle)
            {
                var calculation = await ProcessAutoCalculationAsync(job, actorUserId, cancellationToken);
                Merge(response, calculation);
            }

            if ((job.JobType == OvertimeValueConstants.SchedulerJobType.ExpireCompensatory ||
                 job.JobType == OvertimeValueConstants.SchedulerJobType.FullCycle) &&
                _options.AutoExpireCompensatory)
            {
                var localToday = DateOnly.FromDateTime(
                    TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ResolveTimeZone(_options.TimeZoneId)));
                var expiry = await _expiryService.ExpireDueCreditsAsync(
                    localToday,
                    actorUserId,
                    _options.MaximumItemsPerJob,
                    cancellationToken);
                response.CandidateCount += expiry.CandidateCount;
                response.ProcessedCount += expiry.ExpiredCount + expiry.SkippedCount + expiry.FailedCount;
                response.SucceededCount += expiry.ExpiredCount;
                response.SkippedCount += expiry.SkippedCount;
                response.FailedCount += expiry.FailedCount;
                response.Messages.AddRange(expiry.Messages.Take(50));
            }

            if (job.JobType == OvertimeValueConstants.SchedulerJobType.Reconcile ||
                job.JobType == OvertimeValueConstants.SchedulerJobType.Monitor ||
                job.JobType == OvertimeValueConstants.SchedulerJobType.FullCycle)
            {
                response.Reconciliation = await _reconciliationService.ReconcileAsync(
                    ToReconciliationRequest(job),
                    actorUserId,
                    cancellationToken);
                response.WarningCount += response.Reconciliation.WarningCount;
            }

            if (job.JobType == OvertimeValueConstants.SchedulerJobType.ClosePeriod)
            {
                if (!job.OvertimePeriodId.HasValue) throw new InvalidOperationException("ClosePeriod job membutuhkan OvertimePeriodId.");
                var close = await _periodService.CloseAsync(
                    job.OvertimePeriodId.Value,
                    new CloseOvertimePeriodRequest
                    {
                        Reason = job.Notes ?? "Automatic overtime period closing.",
                        AllowRepair = job.AllowRepair,
                        ForceClose = false
                    },
                    actorUserId,
                    cancellationToken);
                if (!close.Success)
                {
                    response.FailedCount++;
                    response.Messages.Add(close.Message);
                }
                else
                {
                    response.SucceededCount++;
                    response.ProcessedCount++;
                    response.Reconciliation = close.Data?.Reconciliation;
                }
            }

            return response;
        }

        private async Task<OvertimeSchedulerExecutionResponse> ProcessAutoCalculationAsync(
            TrxOvertimeSchedulerJob job,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var cutoff = DateTime.UtcNow.AddMinutes(-Math.Max(0, _options.CalculationDelayMinutes));
            var query = _dbContext.WfpOvertimeRequests
                .AsNoTracking()
                .Include(x => x.Realizations.Where(r => !r.IsDelete && !r.IsCancel && r.IsActive))
                .Where(x =>
                    !x.IsDelete && !x.IsCancel && x.IsActive &&
                    x.OvertimeDate >= job.StartDate && x.OvertimeDate <= job.EndDate &&
                    x.PlannedEndAt.HasValue && x.PlannedEndAt <= cutoff &&
                    (x.OvertimeRequestStatus == OvertimeValueConstants.RequestStatus.ApprovedForWork ||
                     x.OvertimeRequestStatus == OvertimeValueConstants.RequestStatus.InProgress ||
                     x.OvertimeRequestStatus == OvertimeValueConstants.RequestStatus.WaitingRealization ||
                     (job.ForceRecalculate && x.OvertimeRequestStatus == OvertimeValueConstants.RequestStatus.WaitingVerification)));

            if (job.WorkforceProfileId.HasValue) query = query.Where(x => x.WorkforceProfileId == job.WorkforceProfileId);
            if (job.HospitalSiteId.HasValue) query = query.Where(x => x.HospitalSiteId == job.HospitalSiteId);
            if (job.OrganizationUnitId.HasValue) query = query.Where(x => x.OrganizationUnitId == job.OrganizationUnitId);
            if (job.DepartmentId.HasValue) query = query.Where(x => x.DepartmentId == job.DepartmentId);
            if (job.LegalEntityId.HasValue) query = query.Where(x => x.HospitalSite != null && x.HospitalSite.LegalEntityId == job.LegalEntityId);

            var candidates = await query
                .OrderBy(x => x.OvertimeDate)
                .ThenBy(x => x.PlannedEndAt)
                .Take(Math.Clamp(_options.MaximumItemsPerJob, 1, 5000))
                .ToListAsync(cancellationToken);

            var response = new OvertimeSchedulerExecutionResponse
            {
                JobId = job.Id,
                JobNumber = job.JobNumber,
                JobType = job.JobType,
                CandidateCount = candidates.Count
            };

            foreach (var candidate in candidates)
            {
                var latest = candidate.Realizations.OrderByDescending(x => x.RealizationVersion).FirstOrDefault();
                if (latest != null && !job.ForceRecalculate)
                {
                    response.SkippedCount++;
                    response.ProcessedCount++;
                    continue;
                }
                if (latest != null && latest.RealizationStatus != OvertimeValueConstants.RealizationStatus.NeedRevision)
                {
                    response.SkippedCount++;
                    response.ProcessedCount++;
                    continue;
                }

                var result = await _calculationService.CalculateAsync(
                    candidate.Id,
                    new CalculateOvertimeRealizationRequest
                    {
                        Notes = $"Automatic calculation by scheduler {job.JobNumber}.",
                        SubmitForVerification = true,
                        ForceNewVersion = job.ForceRecalculate,
                        IdempotencyKey = $"{job.JobNumber}-{candidate.Id:N}"
                    },
                    actorUserId,
                    cancellationToken);
                response.ProcessedCount++;
                if (result.Success)
                {
                    response.SucceededCount++;
                }
                else if (result.StatusCode == StatusCodes.Status409Conflict || result.StatusCode == StatusCodes.Status400BadRequest)
                {
                    response.SkippedCount++;
                    response.Messages.Add($"{candidate.RequestNumber}: {result.Message}");
                }
                else
                {
                    response.FailedCount++;
                    response.Messages.Add($"{candidate.RequestNumber}: {result.Message}");
                }
                if (response.Messages.Count >= 100) break;
            }

            return response;
        }

        private async Task HandleFailureAsync(
            TrxOvertimeSchedulerJob job,
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
                job.JobStatus = OvertimeValueConstants.SchedulerJobStatus.RetryScheduled;
                job.NextRetryAt = retryAt;
                job.AvailableAt = retryAt;
            }
            else
            {
                job.JobStatus = OvertimeValueConstants.SchedulerJobStatus.Failed;
                job.NextRetryAt = null;
            }
            job.UpdateDateTime = now;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private IQueryable<TrxOvertimeSchedulerJob> BuildBaseQuery() =>
            _dbContext.Set<TrxOvertimeSchedulerJob>()
                .AsNoTracking()
                .Include(x => x.OvertimePeriod)
                .Include(x => x.TriggeredByUser)
                .Include(x => x.CancelledByUser)
                .Where(x => !x.IsDelete);

        private static IQueryable<TrxOvertimeSchedulerJob> ApplyFilter(
            IQueryable<TrxOvertimeSchedulerJob> query,
            OvertimeSchedulerJobQueryRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.JobType)) query = query.Where(x => x.JobType == request.JobType.Trim());
            if (!string.IsNullOrWhiteSpace(request.JobStatus)) query = query.Where(x => x.JobStatus == request.JobStatus.Trim());
            if (request.OvertimePeriodId.HasValue) query = query.Where(x => x.OvertimePeriodId == request.OvertimePeriodId);
            if (request.WorkforceProfileId.HasValue) query = query.Where(x => x.WorkforceProfileId == request.WorkforceProfileId);
            if (request.HospitalSiteId.HasValue) query = query.Where(x => x.HospitalSiteId == request.HospitalSiteId);
            if (request.OrganizationUnitId.HasValue) query = query.Where(x => x.OrganizationUnitId == request.OrganizationUnitId);
            if (request.DepartmentId.HasValue) query = query.Where(x => x.DepartmentId == request.DepartmentId);
            if (request.StartDate.HasValue) query = query.Where(x => x.EndDate >= request.StartDate.Value);
            if (request.EndDate.HasValue) query = query.Where(x => x.StartDate <= request.EndDate.Value);
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

        private static IOrderedQueryable<TrxOvertimeSchedulerJob> ApplySorting(
            IQueryable<TrxOvertimeSchedulerJob> query,
            string? sortBy,
            string? direction)
        {
            var desc = string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "scheduledAt").Trim().ToLowerInvariant() switch
            {
                "availableat" => desc ? query.OrderByDescending(x => x.AvailableAt) : query.OrderBy(x => x.AvailableAt),
                "jobnumber" => desc ? query.OrderByDescending(x => x.JobNumber) : query.OrderBy(x => x.JobNumber),
                "jobtype" => desc ? query.OrderByDescending(x => x.JobType) : query.OrderBy(x => x.JobType),
                "jobstatus" => desc ? query.OrderByDescending(x => x.JobStatus) : query.OrderBy(x => x.JobStatus),
                "priority" => desc ? query.OrderByDescending(x => x.Priority) : query.OrderBy(x => x.Priority),
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => desc ? query.OrderByDescending(x => x.ScheduledAt) : query.OrderBy(x => x.ScheduledAt)
            };
        }

        private static OvertimeReconciliationRequest ToReconciliationRequest(TrxOvertimeSchedulerJob job) => new()
        {
            OvertimePeriodId = job.OvertimePeriodId,
            StartDate = job.StartDate,
            EndDate = job.EndDate,
            LegalEntityId = job.LegalEntityId,
            HospitalSiteId = job.HospitalSiteId,
            OrganizationUnitId = job.OrganizationUnitId,
            DepartmentId = job.DepartmentId,
            AllowRepair = job.AllowRepair,
            VerificationOverdueHours = 24
        };

        private static void Merge(OvertimeSchedulerExecutionResponse target, OvertimeSchedulerExecutionResponse source)
        {
            target.CandidateCount += source.CandidateCount;
            target.ProcessedCount += source.ProcessedCount;
            target.SucceededCount += source.SucceededCount;
            target.SkippedCount += source.SkippedCount;
            target.FailedCount += source.FailedCount;
            target.WarningCount += source.WarningCount;
            target.Messages.AddRange(source.Messages.Take(Math.Max(0, 100 - target.Messages.Count)));
        }

        private static string? ValidateEnqueue(EnqueueOvertimeSchedulerJobRequest request)
        {
            if (NormalizeJobType(request.JobType) == null) return "JobType tidak valid.";
            if (request.EndDate < request.StartDate) return "Tanggal selesai tidak boleh lebih kecil dari tanggal mulai.";
            if (request.EndDate.DayNumber - request.StartDate.DayNumber + 1 > 366) return "Rentang scheduler job maksimal 366 hari.";
            if (request.Priority < 1 || request.Priority > 1000) return "Priority harus antara 1 sampai 1000.";
            return null;
        }

        private async Task<string> GenerateJobNumberAsync(CancellationToken cancellationToken)
        {
            var prefix = $"OTJ-{DateTime.UtcNow:yyyyMMdd}-";
            var count = await _dbContext.Set<TrxOvertimeSchedulerJob>()
                .AsNoTracking()
                .CountAsync(x => !x.IsDelete && x.JobNumber.StartsWith(prefix), cancellationToken);
            return prefix + (count + 1).ToString("D6") + "-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        }

        private Guid ParseSystemActorUserId() => Guid.TryParse(_options.SystemActorUserId, out var id) ? id : Guid.Empty;

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

        private static string? NormalizeJobType(string? value) =>
            string.IsNullOrWhiteSpace(value)
                ? null
                : OvertimeValueConstants.SchedulerJobType.All.FirstOrDefault(x => x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));

        private static bool IsTerminal(string status) =>
            status == OvertimeValueConstants.SchedulerJobStatus.Completed ||
            status == OvertimeValueConstants.SchedulerJobStatus.CompletedWithIssues ||
            status == OvertimeValueConstants.SchedulerJobStatus.Failed ||
            status == OvertimeValueConstants.SchedulerJobStatus.Cancelled;

        private static OvertimeSchedulerJobActionResponse MapAction(TrxOvertimeSchedulerJob job, string previous, DateTime now) => new()
        {
            JobId = job.Id,
            JobNumber = job.JobNumber,
            PreviousStatus = previous,
            CurrentStatus = job.JobStatus,
            RetryCount = job.RetryCount,
            ActionAt = now
        };

        private static void NormalizePaging(OvertimeSchedulerJobQueryRequest request)
        {
            request.PageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            request.PageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, 100);
        }

        private static Guid? NormalizeGuid(Guid? value) => value.HasValue && value.Value != Guid.Empty ? value.Value : null;
        private static DateTime? NormalizeUtc(DateTime? value) => value.HasValue
            ? value.Value.Kind == DateTimeKind.Utc ? value.Value : value.Value.ToUniversalTime()
            : null;
        private static string? NormalizeText(string? value, int maxLength) => string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim().Length <= maxLength ? value.Trim() : value.Trim()[..maxLength];
        private static string AppendText(string? existing, string addition, int maxLength)
        {
            var combined = string.IsNullOrWhiteSpace(existing) ? addition.Trim() : existing.Trim() + Environment.NewLine + addition.Trim();
            return combined.Length <= maxLength ? combined : combined[..maxLength];
        }
        private static string Limit(string value, int maxLength) => value.Length <= maxLength ? value : value[..maxLength];
    }
}
