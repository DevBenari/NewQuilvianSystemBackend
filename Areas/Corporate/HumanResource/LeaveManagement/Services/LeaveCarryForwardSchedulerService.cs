using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    public class LeaveCarryForwardSchedulerService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly LeaveCarryForwardProcessorService _processor;
        private readonly LeaveCarryForwardSchedulerOptions _options;
        private readonly ILogger<LeaveCarryForwardSchedulerService> _logger;

        public LeaveCarryForwardSchedulerService(
            ApplicationDbContext dbContext,
            LeaveCarryForwardProcessorService processor,
            IOptions<LeaveCarryForwardSchedulerOptions> options,
            ILogger<LeaveCarryForwardSchedulerService> logger)
        {
            _dbContext = dbContext;
            _processor = processor;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<LeaveCarryForwardEnqueueResponse> EnqueueDueRunsAsync(
            DateOnly executionDate,
            Guid actorUserId,
            bool queueForProcessing,
            CancellationToken cancellationToken = default)
        {
            var response = new LeaveCarryForwardEnqueueResponse
            {
                ExecutionDate = executionDate
            };

            var policies = await _dbContext.Set<MstLeaveCarryForwardPolicy>()
                .AsNoTracking()
                .Include(x => x.LeaveEntitlementPolicy)
                    .ThenInclude(x => x!.LeavePolicy)
                .Where(x =>
                    x.IsCarryForwardEnabled &&
                    x.IsActive &&
                    !x.IsDelete &&
                    x.CarryForwardExecutionTiming != LeaveValueConstants.CarryForwardExecutionTiming.Manual &&
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= executionDate.ToDateTime(TimeOnly.MinValue).Date) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= executionDate.ToDateTime(TimeOnly.MinValue).Date))
                .ToListAsync(cancellationToken);
            response.EvaluatedPolicyCount = policies.Count;

            foreach (var policy in policies)
            {
                var entitlementPolicy = policy.LeaveEntitlementPolicy;
                var leavePolicy = entitlementPolicy?.LeavePolicy;
                if (entitlementPolicy == null || leavePolicy == null)
                {
                    response.SkippedCount += 1;
                    response.Messages.Add($"Policy {policy.CarryForwardPolicyCode} dilewati karena entitlement/leave policy tidak lengkap.");
                    continue;
                }

                var sourcePeriods = await _dbContext.Set<TrxLeaveEntitlementPeriod>()
                    .AsNoTracking()
                    .Where(x =>
                        x.IsActive &&
                        !x.IsDelete &&
                        !x.IsLocked &&
                        x.PeriodStatus != LeaveValueConstants.PeriodStatus.Closed &&
                        x.PeriodStatus != LeaveValueConstants.PeriodStatus.Cancelled &&
                        (!x.LeaveTypeId.HasValue || x.LeaveTypeId == leavePolicy.LeaveTypeId) &&
                        x.EndDate <= executionDate &&
                        x.EndDate >= executionDate.AddDays(-Math.Max(1, _options.LookBackDays)))
                    .OrderBy(x => x.EndDate)
                    .ToListAsync(cancellationToken);

                foreach (var source in sourcePeriods)
                {
                    var destination = await _dbContext.Set<TrxLeaveEntitlementPeriod>()
                        .AsNoTracking()
                        .Where(x =>
                            x.IsActive &&
                            !x.IsDelete &&
                            !x.IsLocked &&
                            x.PeriodStatus != LeaveValueConstants.PeriodStatus.Closed &&
                            x.PeriodStatus != LeaveValueConstants.PeriodStatus.Cancelled &&
                            x.StartDate > source.EndDate &&
                            (!x.LeaveTypeId.HasValue || x.LeaveTypeId == (policy.DestinationLeaveTypeId ?? leavePolicy.LeaveTypeId)) &&
                            x.LegalEntityId == source.LegalEntityId &&
                            x.HospitalSiteId == source.HospitalSiteId &&
                            x.OrganizationUnitId == source.OrganizationUnitId &&
                            x.DepartmentId == source.DepartmentId)
                        .OrderBy(x => x.StartDate)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (destination == null)
                    {
                        response.SkippedCount += 1;
                        response.Messages.Add($"Destination period untuk {source.PeriodCode} dan policy {policy.CarryForwardPolicyCode} tidak ditemukan.");
                        continue;
                    }

                    if (policy.CarryForwardExecutionTiming == LeaveValueConstants.CarryForwardExecutionTiming.NextPeriodOpen &&
                        destination.StartDate > executionDate)
                    {
                        response.SkippedCount += 1;
                        continue;
                    }

                    var hasBalance = await _dbContext.Set<WfpLeaveBalance>()
                        .AsNoTracking()
                        .AnyAsync(x =>
                            x.LeaveEntitlementPeriodId == source.Id &&
                            x.LeaveEntitlementPolicyId == entitlementPolicy.Id &&
                            x.LeaveTypeId == leavePolicy.LeaveTypeId &&
                            x.IsActive &&
                            !x.IsDelete,
                            cancellationToken);
                    if (!hasBalance)
                    {
                        response.SkippedCount += 1;
                        continue;
                    }

                    var key = $"CF-AUTO:{source.Id:N}:{destination.Id:N}:{policy.Id:N}";
                    var existing = await _dbContext.Set<TrxLeaveCarryForwardRun>()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.IdempotencyKey == key && !x.IsDelete, cancellationToken);
                    if (existing != null)
                    {
                        response.ExistingRunCount += 1;
                        continue;
                    }

                    var result = await _processor.CreateRunAsync(
                        new CreateLeaveCarryForwardRunRequest
                        {
                            SourceLeaveEntitlementPeriodId = source.Id,
                            DestinationLeaveEntitlementPeriodId = destination.Id,
                            LeaveTypeId = leavePolicy.LeaveTypeId,
                            LeaveCarryForwardPolicyId = policy.Id,
                            LegalEntityId = source.LegalEntityId,
                            HospitalSiteId = source.HospitalSiteId,
                            OrganizationUnitId = source.OrganizationUnitId,
                            DepartmentId = source.DepartmentId,
                            ExecutionDate = executionDate,
                            RunMode = LeaveValueConstants.BatchRunMode.Scheduled,
                            QueueForProcessing = queueForProcessing,
                            MaximumRetryCount = _options.DefaultMaximumRetryCount,
                            IdempotencyKey = key,
                            CorrelationId = $"leave-carry-forward-{executionDate:yyyyMMdd}",
                            Notes = "Dibuat otomatis oleh Leave Carry Forward Scheduler."
                        },
                        actorUserId,
                        cancellationToken);

                    if (result.Success && result.Data != null)
                    {
                        response.CreatedRunCount += 1;
                        response.CreatedRunIds.Add(result.Data.Id);
                    }
                    else
                    {
                        response.SkippedCount += 1;
                        response.Messages.Add(result.Message);
                    }
                }
            }

            return response;
        }

        public async Task RecoverStaleRunsAsync(CancellationToken cancellationToken = default)
        {
            var cutoff = DateTime.UtcNow.AddMinutes(-Math.Max(5, _options.RunningRunTimeoutMinutes));
            var stale = await _dbContext.Set<TrxLeaveCarryForwardRun>()
                .Where(x =>
                    x.RunStatus == LeaveValueConstants.BatchRunStatus.Running &&
                    x.StartedAt.HasValue &&
                    x.StartedAt.Value < cutoff &&
                    !x.IsDelete)
                .ToListAsync(cancellationToken);

            foreach (var run in stale)
            {
                if (run.RetryCount < run.MaximumRetryCount)
                {
                    run.RetryCount += 1;
                    run.RunMode = LeaveValueConstants.BatchRunMode.Reprocess;
                    run.RunStatus = LeaveValueConstants.BatchRunStatus.Queued;
                    run.StartedAt = null;
                    run.ErrorSummary = "Run dikembalikan ke queue karena heartbeat processing melewati timeout.";
                }
                else
                {
                    run.RunStatus = LeaveValueConstants.BatchRunStatus.Failed;
                    run.CompletedAt = DateTime.UtcNow;
                    run.ErrorSummary = "Run gagal karena processing timeout dan maximum retry count tercapai.";
                }
                run.UpdateDateTime = DateTime.UtcNow;
            }

            if (stale.Count > 0)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<bool> ProcessNextQueuedRunAsync(
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            Guid? runId = null;
            await using (var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken))
            {
                var run = await _dbContext.Set<TrxLeaveCarryForwardRun>()
                    .FromSqlRaw(@"
                        SELECT *
                        FROM public.""TrxLeaveCarryForwardRun""
                        WHERE ""RunStatus"" = 'Queued'
                          AND ""IsActive"" = true
                          AND ""IsDelete"" = false
                        ORDER BY ""CreateDateTime""
                        FOR UPDATE SKIP LOCKED
                        LIMIT 1")
                    .FirstOrDefaultAsync(cancellationToken);

                if (run == null)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return false;
                }

                run.RunStatus = LeaveValueConstants.BatchRunStatus.Running;
                run.StartedAt = DateTime.UtcNow;
                run.TriggeredByUserId ??= actorUserId == Guid.Empty ? null : actorUserId;
                run.UpdateDateTime = DateTime.UtcNow;
                run.UpdateBy = actorUserId;
                runId = run.Id;
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }

            if (!runId.HasValue)
            {
                return false;
            }

            var result = await _processor.ExecuteRunAsync(
                runId.Value,
                actorUserId,
                forceReprocess: false,
                notes: $"Diproses oleh worker {_options.WorkerInstanceName}.",
                cancellationToken: cancellationToken,
                allowAlreadyRunning: true);

            if (!result.Success)
            {
                _logger.LogWarning("Leave carry-forward queued run {RunId} gagal diproses: {Message}", runId, result.Message);
            }
            return true;
        }

        public async Task ProcessDueExpiryAsync(
            DateOnly asOfDate,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var result = await _processor.ProcessExpiryAsync(
                new LeaveCarryForwardExpiryRequest
                {
                    AsOfDate = asOfDate,
                    IsDryRun = false,
                    MaximumItem = Math.Clamp(_options.ExpiryBatchSize, 1, 2000),
                    CorrelationId = $"leave-carry-forward-expiry-{asOfDate:yyyyMMdd}",
                    Notes = "Diproses otomatis oleh Leave Carry Forward Scheduler."
                },
                actorUserId,
                cancellationToken);

            if (!result.Success)
            {
                _logger.LogWarning("Pemrosesan due carry-forward expiry gagal: {Message}", result.Message);
            }
        }
    }
}
