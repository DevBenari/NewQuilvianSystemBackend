using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Repositories;
using System.Data;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    public class LeaveAccrualSchedulerService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly LeaveAccrualProcessorService _processor;
        private readonly LeaveAccrualSchedulerOptions _options;
        private readonly ILogger<LeaveAccrualSchedulerService> _logger;

        public LeaveAccrualSchedulerService(
            ApplicationDbContext dbContext,
            LeaveAccrualProcessorService processor,
            IOptions<LeaveAccrualSchedulerOptions> options,
            ILogger<LeaveAccrualSchedulerService> logger)
        {
            _dbContext = dbContext;
            _processor = processor;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<LeaveAccrualEnqueueResponse> EnqueueDueRunsAsync(
            DateOnly accrualDate,
            Guid actorUserId,
            bool queueForProcessing = true,
            CancellationToken cancellationToken = default)
        {
            var dateStart = accrualDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var dateEnd = accrualDate.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

            var periods = await _dbContext.Set<TrxLeaveEntitlementPeriod>()
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsLocked &&
                    x.PeriodStatus != LeaveValueConstants.PeriodStatus.Closed &&
                    x.PeriodStatus != LeaveValueConstants.PeriodStatus.Cancelled &&
                    x.StartDate <= accrualDate &&
                    x.EndDate >= accrualDate)
                .ToListAsync(cancellationToken);

            var policies = await _dbContext.Set<MstLeaveEntitlementPolicy>()
                .AsNoTracking()
                .Include(x => x.LeavePolicy)
                .Where(x =>
                    x.IsActive &&
                    !x.IsDelete &&
                    x.LeavePolicy != null &&
                    x.LeavePolicy.IsActive &&
                    !x.LeavePolicy.IsDelete &&
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value <= dateEnd) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= dateStart))
                .ToListAsync(cancellationToken);

            var response = new LeaveAccrualEnqueueResponse
            {
                AccrualDate = accrualDate
            };

            foreach (var period in periods)
            {
                foreach (var policy in policies)
                {
                    if (policy.LeavePolicy == null)
                    {
                        continue;
                    }

                    if (period.LeaveTypeId.HasValue &&
                        period.LeaveTypeId.Value != policy.LeavePolicy.LeaveTypeId)
                    {
                        continue;
                    }

                    if (!IsScopeCompatible(period, policy.LeavePolicy))
                    {
                        continue;
                    }

                    if (!TryResolveWindow(
                            period,
                            policy,
                            accrualDate,
                            out var windowStart,
                            out var windowEnd,
                            out var scheduledDate))
                    {
                        continue;
                    }

                    response.EligiblePolicyCount++;
                    var idempotencyKey =
                        $"AUTO-ACCRUAL:{period.Id:N}:{policy.Id:N}:{scheduledDate:yyyyMMdd}:{windowStart:yyyyMMdd}:{windowEnd:yyyyMMdd}";

                    var createResult = await _processor.CreateRunAsync(
                        new CreateLeaveAccrualRunRequest
                        {
                            LeaveEntitlementPeriodId = period.Id,
                            LeaveTypeId = policy.LeavePolicy.LeaveTypeId,
                            LeaveEntitlementPolicyId = policy.Id,
                            LegalEntityId = period.LegalEntityId ?? policy.LeavePolicy.LegalEntityId,
                            HospitalSiteId = period.HospitalSiteId ?? policy.LeavePolicy.HospitalSiteId,
                            OrganizationUnitId = period.OrganizationUnitId ?? policy.LeavePolicy.OrganizationUnitId,
                            DepartmentId = period.DepartmentId ?? policy.LeavePolicy.DepartmentId,
                            ScheduledAccrualDate = scheduledDate,
                            AccrualPeriodStartDate = windowStart,
                            AccrualPeriodEndDate = windowEnd,
                            RunMode = LeaveValueConstants.BatchRunMode.Scheduled,
                            IsDryRun = false,
                            QueueForProcessing = queueForProcessing,
                            MaximumRetryCount = Math.Clamp(_options.DefaultMaximumRetryCount, 0, 10),
                            IdempotencyKey = idempotencyKey,
                            CorrelationId = $"AUTO-ACCRUAL-{scheduledDate:yyyyMMdd}",
                            Notes = "Dibuat otomatis oleh Leave Accrual Scheduler."
                        },
                        actorUserId,
                        cancellationToken);

                    if (!createResult.Success || createResult.Data == null)
                    {
                        _logger.LogWarning(
                            "Gagal membuat automatic leave accrual run untuk period {PeriodId}, policy {PolicyId}: {Message}",
                            period.Id,
                            policy.Id,
                            createResult.Message);
                        continue;
                    }

                    if (createResult.Data.IsIdempotent)
                    {
                        response.ExistingRunCount++;
                    }
                    else
                    {
                        response.CreatedRunCount++;
                    }
                    response.Runs.Add(createResult.Data);
                }
            }

            return response;
        }

        public async Task<int> RecoverStaleRunsAsync(
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var timeoutMinutes = Math.Max(5, _options.RunningRunTimeoutMinutes);
            var threshold = DateTime.UtcNow.AddMinutes(-timeoutMinutes);

            var staleRuns = await _dbContext.Set<TrxLeaveAccrualRun>()
                .Where(x =>
                    x.RunStatus == LeaveValueConstants.BatchRunStatus.Running &&
                    x.StartedAt.HasValue &&
                    x.StartedAt.Value < threshold &&
                    !x.IsDelete)
                .ToListAsync(cancellationToken);

            foreach (var run in staleRuns)
            {
                if (run.RetryCount < run.MaximumRetryCount)
                {
                    run.RetryCount++;
                    run.RunMode = LeaveValueConstants.BatchRunMode.Reprocess;
                    run.RunStatus = LeaveValueConstants.BatchRunStatus.Queued;
                    run.ForceReprocess = true;
                    run.ErrorSummary = "Run dipulihkan dari status Running yang melewati timeout worker.";
                    run.StartedAt = null;
                    run.CompletedAt = null;
                }
                else
                {
                    run.RunStatus = LeaveValueConstants.BatchRunStatus.Failed;
                    run.CompletedAt = DateTime.UtcNow;
                    run.ErrorSummary = "Run melewati timeout worker dan maksimum retry telah tercapai.";
                }

                run.UpdateDateTime = DateTime.UtcNow;
                run.UpdateBy = actorUserId;
            }

            if (staleRuns.Count > 0)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return staleRuns.Count;
        }

        public async Task<LeaveAccrualServiceResult<LeaveAccrualRunActionResponse>?> ProcessNextQueuedRunAsync(
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            Guid? claimedRunId = null;

            await using IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.ReadCommitted,
                cancellationToken);

            var run = await _dbContext.Set<TrxLeaveAccrualRun>()
                .FromSqlRaw(@"
                    SELECT *
                    FROM public.""TrxLeaveAccrualRun""
                    WHERE ""RunStatus"" = 'Queued'
                      AND ""IsDelete"" = false
                      AND ""IsActive"" = true
                    ORDER BY ""ScheduledAccrualDate"", ""CreateDateTime""
                    FOR UPDATE SKIP LOCKED
                    LIMIT 1")
                .FirstOrDefaultAsync(cancellationToken);

            if (run != null)
            {
                claimedRunId = run.Id;
                run.RunStatus = LeaveValueConstants.BatchRunStatus.Running;
                run.StartedAt = DateTime.UtcNow;
                run.UpdateDateTime = DateTime.UtcNow;
                run.UpdateBy = actorUserId;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            _dbContext.ChangeTracker.Clear();

            if (!claimedRunId.HasValue)
            {
                return null;
            }

            return await _processor.ExecuteRunAsync(
                claimedRunId.Value,
                actorUserId,
                forceReprocess: false,
                notes: $"Diproses oleh scheduler worker {_options.WorkerInstanceName}.",
                cancellationToken: cancellationToken,
                allowRunningClaim: true);
        }

        private static bool IsScopeCompatible(
            TrxLeaveEntitlementPeriod period,
            MstLeavePolicy policy)
        {
            if (period.LegalEntityId.HasValue && policy.LegalEntityId.HasValue &&
                period.LegalEntityId != policy.LegalEntityId)
            {
                return false;
            }
            if (period.HospitalSiteId.HasValue && policy.HospitalSiteId.HasValue &&
                period.HospitalSiteId != policy.HospitalSiteId)
            {
                return false;
            }
            if (period.OrganizationUnitId.HasValue && policy.OrganizationUnitId.HasValue &&
                period.OrganizationUnitId != policy.OrganizationUnitId)
            {
                return false;
            }
            if (period.DepartmentId.HasValue && policy.DepartmentId.HasValue &&
                period.DepartmentId != policy.DepartmentId)
            {
                return false;
            }
            return true;
        }

        private static bool TryResolveWindow(
            TrxLeaveEntitlementPeriod period,
            MstLeaveEntitlementPolicy policy,
            DateOnly evaluationDate,
            out DateOnly windowStart,
            out DateOnly windowEnd,
            out DateOnly scheduledDate)
        {
            windowStart = period.StartDate;
            windowEnd = period.EndDate;
            scheduledDate = period.EndDate;

            if (policy.AccrualStartMonth.HasValue && policy.AccrualStartDay.HasValue)
            {
                var day = Math.Min(
                    policy.AccrualStartDay.Value,
                    DateTime.DaysInMonth(period.PeriodYear, policy.AccrualStartMonth.Value));
                var configuredStart = new DateOnly(period.PeriodYear, policy.AccrualStartMonth.Value, day);
                if (evaluationDate < configuredStart)
                {
                    return false;
                }
            }

            var frequency = (policy.AccrualFrequency ?? string.Empty).Trim().ToLowerInvariant();
            var intervalMonths = frequency switch
            {
                "monthly" => 1,
                "quarterly" => 3,
                "semiannual" => 6,
                "semi-annual" => 6,
                "annual" => 12,
                _ => 0
            };

            if (intervalMonths > 0)
            {
                var monthsFromPeriodStart =
                    (evaluationDate.Year - period.StartDate.Year) * 12 +
                    evaluationDate.Month - period.StartDate.Month;

                if (monthsFromPeriodStart < 0)
                {
                    return false;
                }

                var intervalIndex = monthsFromPeriodStart / intervalMonths;
                windowStart = period.StartDate.AddMonths(intervalIndex * intervalMonths);
                windowEnd = windowStart.AddMonths(intervalMonths).AddDays(-1);
                if (windowEnd > period.EndDate)
                {
                    windowEnd = period.EndDate;
                }
            }
            else if (frequency == "weekly" || frequency == "biweekly" || frequency == "bi-weekly")
            {
                var intervalDays = frequency == "weekly" ? 7 : 14;
                var elapsedDays = evaluationDate.DayNumber - period.StartDate.DayNumber;
                if (elapsedDays < 0)
                {
                    return false;
                }
                var intervalIndex = elapsedDays / intervalDays;
                windowStart = period.StartDate.AddDays(intervalIndex * intervalDays);
                windowEnd = windowStart.AddDays(intervalDays - 1);
                if (windowEnd > period.EndDate)
                {
                    windowEnd = period.EndDate;
                }
            }
            else if (frequency == "daily")
            {
                windowStart = evaluationDate;
                windowEnd = evaluationDate;
            }
            else
            {
                windowStart = period.StartDate;
                windowEnd = period.EndDate;
            }

            scheduledDate = ResolveScheduledDate(policy, windowStart, windowEnd);
            return evaluationDate == scheduledDate;
        }

        private static DateOnly ResolveScheduledDate(
            MstLeaveEntitlementPolicy policy,
            DateOnly windowStart,
            DateOnly windowEnd)
        {
            if (policy.AccrualTiming == LeaveValueConstants.AccrualTiming.StartOfPeriod)
            {
                return windowStart;
            }

            if (policy.AccrualTiming == LeaveValueConstants.AccrualTiming.SpecificDay &&
                policy.AccrualDayOfMonth.HasValue)
            {
                var day = Math.Min(
                    Math.Max(1, policy.AccrualDayOfMonth.Value),
                    DateTime.DaysInMonth(windowEnd.Year, windowEnd.Month));
                var candidate = new DateOnly(windowEnd.Year, windowEnd.Month, day);
                if (candidate < windowStart)
                {
                    return windowStart;
                }
                return candidate > windowEnd ? windowEnd : candidate;
            }

            return windowEnd;
        }
    }
}
