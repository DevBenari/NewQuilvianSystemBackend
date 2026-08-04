using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Repositories;
using System.Data;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    internal class LeaveCarryForwardRunParameters
    {
        public Guid? WorkforceProfileId { get; set; }
        public int MaximumPreviewItem { get; set; } = 250;
    }

    public class LeaveCarryForwardProcessorService
    {
        private const decimal Tolerance = 0.0001m;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LeaveCarryForwardPolicyResolverService _policyResolver;
        private readonly ILogger<LeaveCarryForwardProcessorService> _logger;

        public LeaveCarryForwardProcessorService(
            ApplicationDbContext dbContext,
            LeaveCarryForwardPolicyResolverService policyResolver,
            ILogger<LeaveCarryForwardProcessorService> logger)
        {
            _dbContext = dbContext;
            _policyResolver = policyResolver;
            _logger = logger;
        }

        public LeaveCarryForwardRunFilterMetadataResponse GetMetadata()
        {
            return new LeaveCarryForwardRunFilterMetadataResponse
            {
                DefaultFilter = new LeaveCarryForwardRunDefaultFilterResponse
                {
                    StartDate = new DateOnly(DateTime.UtcNow.Year, 1, 1),
                    EndDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                    SortBy = "createDateTime",
                    SortDirection = "desc",
                    PageNumber = 1,
                    PageSize = 25
                },
                RunStatuses = new List<LeaveCarryForwardOptionResponse>
                {
                    Option(LeaveValueConstants.BatchRunStatus.Draft),
                    Option(LeaveValueConstants.BatchRunStatus.Queued),
                    Option(LeaveValueConstants.BatchRunStatus.Running),
                    Option(LeaveValueConstants.BatchRunStatus.Completed),
                    Option(LeaveValueConstants.BatchRunStatus.CompletedWithErrors),
                    Option(LeaveValueConstants.BatchRunStatus.Failed),
                    Option(LeaveValueConstants.BatchRunStatus.Cancelled),
                    Option(LeaveValueConstants.BatchRunStatus.Reversed)
                },
                RunModes = new List<LeaveCarryForwardOptionResponse>
                {
                    Option(LeaveValueConstants.BatchRunMode.Scheduled),
                    Option(LeaveValueConstants.BatchRunMode.Manual),
                    Option(LeaveValueConstants.BatchRunMode.Reprocess),
                    Option(LeaveValueConstants.BatchRunMode.Preview)
                },
                CarryForwardStatuses = new List<LeaveCarryForwardOptionResponse>
                {
                    Option(LeaveValueConstants.CarryForwardStatus.Draft),
                    Option(LeaveValueConstants.CarryForwardStatus.Calculated),
                    Option(LeaveValueConstants.CarryForwardStatus.Posted),
                    Option(LeaveValueConstants.CarryForwardStatus.Reversed),
                    Option(LeaveValueConstants.CarryForwardStatus.Skipped),
                    Option(LeaveValueConstants.CarryForwardStatus.Failed),
                    Option(LeaveValueConstants.CarryForwardStatus.Cancelled)
                },
                SortOptions = new List<LeaveCarryForwardOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal Dibuat" },
                    new() { Value = "executionDate", Label = "Tanggal Eksekusi" },
                    new() { Value = "runNumber", Label = "Nomor Run" },
                    new() { Value = "runStatus", Label = "Status" },
                    new() { Value = "totalCarryForwardDays", Label = "Total Carry Forward" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100, 200 }
            };
        }

        public async Task<LeaveCarryForwardRunSummaryResponse> GetSummaryAsync(
            LeaveCarryForwardRunQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var query = BuildRunQuery(request);
            return new LeaveCarryForwardRunSummaryResponse
            {
                TotalRun = await query.CountAsync(cancellationToken),
                DraftRun = await query.CountAsync(x => x.RunStatus == LeaveValueConstants.BatchRunStatus.Draft, cancellationToken),
                QueuedRun = await query.CountAsync(x => x.RunStatus == LeaveValueConstants.BatchRunStatus.Queued, cancellationToken),
                RunningRun = await query.CountAsync(x => x.RunStatus == LeaveValueConstants.BatchRunStatus.Running, cancellationToken),
                CompletedRun = await query.CountAsync(x => x.RunStatus == LeaveValueConstants.BatchRunStatus.Completed, cancellationToken),
                CompletedWithErrorsRun = await query.CountAsync(x => x.RunStatus == LeaveValueConstants.BatchRunStatus.CompletedWithErrors, cancellationToken),
                FailedRun = await query.CountAsync(x => x.RunStatus == LeaveValueConstants.BatchRunStatus.Failed, cancellationToken),
                CancelledRun = await query.CountAsync(x => x.RunStatus == LeaveValueConstants.BatchRunStatus.Cancelled, cancellationToken),
                ReversedRun = await query.CountAsync(x => x.RunStatus == LeaveValueConstants.BatchRunStatus.Reversed, cancellationToken),
                TotalTarget = await query.SumAsync(x => (int?)x.TargetCount, cancellationToken) ?? 0,
                TotalPosted = await query.SumAsync(x => (int?)x.PostedCount, cancellationToken) ?? 0,
                TotalSkipped = await query.SumAsync(x => (int?)x.SkippedCount, cancellationToken) ?? 0,
                TotalFailed = await query.SumAsync(x => (int?)x.FailedCount, cancellationToken) ?? 0,
                TotalSourceAvailableDays = await query.SumAsync(x => (decimal?)x.TotalSourceAvailableDays, cancellationToken) ?? 0,
                TotalCarryForwardDays = await query.SumAsync(x => (decimal?)x.TotalCarryForwardDays, cancellationToken) ?? 0,
                TotalExpiredDays = await query.SumAsync(x => (decimal?)x.TotalExpiredDays, cancellationToken) ?? 0,
                TotalPayoutDays = await query.SumAsync(x => (decimal?)x.TotalPayoutDays, cancellationToken) ?? 0
            };
        }

        public async Task<LeaveCarryForwardRunPagedResponse> GetPagedAsync(
            LeaveCarryForwardRunQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            request.PageNumber = Math.Max(1, request.PageNumber);
            request.PageSize = Math.Clamp(request.PageSize, 1, 200);
            IQueryable<TrxLeaveCarryForwardRun> query = BuildRunQuery(request)
                .Include(x => x.SourceLeaveEntitlementPeriod)
                .Include(x => x.DestinationLeaveEntitlementPeriod)
                .Include(x => x.LeaveType)
                .Include(x => x.LeaveCarryForwardPolicy);

            query = ApplyRunSort(query, request.SortBy, request.SortDirection);
            var totalData = await query.CountAsync(cancellationToken);
            var rows = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);
            var items = rows.Select(MapRun).ToList();

            return new LeaveCarryForwardRunPagedResponse
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)request.PageSize),
                Items = items
            };
        }

        public async Task<LeaveCarryForwardServiceResult<LeaveCarryForwardRunDetailResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var run = await _dbContext.Set<TrxLeaveCarryForwardRun>()
                .AsNoTracking()
                .Include(x => x.SourceLeaveEntitlementPeriod)
                .Include(x => x.DestinationLeaveEntitlementPeriod)
                .Include(x => x.LeaveType)
                .Include(x => x.LeaveCarryForwardPolicy)
                .Include(x => x.TriggeredByUser)
                .Include(x => x.CancelledByUser)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (run == null)
            {
                return LeaveCarryForwardServiceResult<LeaveCarryForwardRunDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Leave carry-forward run tidak ditemukan.");
            }

            var response = new LeaveCarryForwardRunDetailResponse
            {
                Id = run.Id,
                RunNumber = run.RunNumber,
                RunMode = run.RunMode,
                RunStatus = run.RunStatus,
                SourceLeaveEntitlementPeriodId = run.SourceLeaveEntitlementPeriodId,
                SourcePeriodCode = run.SourceLeaveEntitlementPeriod?.PeriodCode,
                SourcePeriodName = run.SourceLeaveEntitlementPeriod?.PeriodName,
                DestinationLeaveEntitlementPeriodId = run.DestinationLeaveEntitlementPeriodId,
                DestinationPeriodCode = run.DestinationLeaveEntitlementPeriod?.PeriodCode,
                DestinationPeriodName = run.DestinationLeaveEntitlementPeriod?.PeriodName,
                LeaveTypeId = run.LeaveTypeId,
                LeaveTypeCode = run.LeaveType?.LeaveTypeCode,
                LeaveTypeName = run.LeaveType?.LeaveTypeName,
                LeaveCarryForwardPolicyId = run.LeaveCarryForwardPolicyId,
                CarryForwardPolicyCode = run.LeaveCarryForwardPolicy?.CarryForwardPolicyCode,
                CarryForwardPolicyName = run.LeaveCarryForwardPolicy?.CarryForwardPolicyName,
                ExecutionDate = run.ExecutionDate,
                IsDryRun = run.IsDryRun,
                ForceReprocess = run.ForceReprocess,
                RetryCount = run.RetryCount,
                MaximumRetryCount = run.MaximumRetryCount,
                TargetCount = run.TargetCount,
                CalculatedCount = run.CalculatedCount,
                PostedCount = run.PostedCount,
                SkippedCount = run.SkippedCount,
                FailedCount = run.FailedCount,
                TotalSourceAvailableDays = run.TotalSourceAvailableDays,
                TotalEligibleDays = run.TotalEligibleDays,
                TotalCarryForwardDays = run.TotalCarryForwardDays,
                TotalExpiredDays = run.TotalExpiredDays,
                TotalExcessDays = run.TotalExcessDays,
                TotalPayoutDays = run.TotalPayoutDays,
                StartedAt = run.StartedAt,
                CompletedAt = run.CompletedAt,
                CancelledAt = run.CancelledAt,
                TriggeredByUserId = run.TriggeredByUserId,
                TriggeredByName = run.TriggeredByUser?.DisplayName,
                CancelledByUserId = run.CancelledByUserId,
                CancelledByName = run.CancelledByUser?.DisplayName,
                CorrelationId = run.CorrelationId,
                ErrorSummary = run.ErrorSummary,
                ParametersJson = run.ParametersJson,
                ResultSummaryJson = run.ResultSummaryJson,
                Notes = run.Notes,
                CreateDateTime = run.CreateDateTime
            };

            response.CarryForwards = await _dbContext.Set<TrxLeaveCarryForward>()
                .AsNoTracking()
                .Where(x => x.LeaveCarryForwardRunId == id && !x.IsDelete)
                .OrderBy(x => x.CarryForwardNumber)
                .Select(x => new LeaveCarryForwardItemResponse
                {
                    Id = x.Id,
                    CarryForwardNumber = x.CarryForwardNumber,
                    WorkforceProfileId = x.WorkforceProfileId,
                    WorkforceProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : null,
                    WorkforceDisplayName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : null,
                    SourceLeaveTypeId = x.SourceLeaveTypeId,
                    SourceLeaveTypeName = x.SourceLeaveType != null ? x.SourceLeaveType.LeaveTypeName : null,
                    DestinationLeaveTypeId = x.DestinationLeaveTypeId,
                    DestinationLeaveTypeName = x.DestinationLeaveType != null ? x.DestinationLeaveType.LeaveTypeName : null,
                    SourceLeaveBalanceId = x.SourceLeaveBalanceId,
                    DestinationLeaveBalanceId = x.DestinationLeaveBalanceId,
                    SourceAvailableDays = x.SourceAvailableDays,
                    EligibleDays = x.EligibleDays,
                    CarryForwardDays = x.CarryForwardDays,
                    ExpiredDays = x.ExpiredDays,
                    ExcessDays = x.ExcessDays,
                    PayoutDays = x.PayoutDays,
                    CarryForwardExpiryDate = x.CarryForwardExpiryDate,
                    CarryForwardStatus = x.CarryForwardStatus,
                    SkipReasonCode = x.SkipReasonCode,
                    SkipReason = x.SkipReason,
                    PostedAt = x.PostedAt,
                    ReversedAt = x.ReversedAt
                })
                .ToListAsync(cancellationToken);

            return LeaveCarryForwardServiceResult<LeaveCarryForwardRunDetailResponse>.Ok(
                response,
                "Detail leave carry-forward run berhasil diambil.");
        }

        public async Task<LeaveCarryForwardServiceResult<LeaveCarryForwardPreviewResponse>> PreviewAsync(
            LeaveCarryForwardPreviewRequest request,
            CancellationToken cancellationToken = default)
        {
            var validation = await ValidatePeriodsAsync(
                request.SourceLeaveEntitlementPeriodId,
                request.DestinationLeaveEntitlementPeriodId,
                request.ExecutionDate,
                cancellationToken);
            if (!validation.Success)
            {
                return LeaveCarryForwardServiceResult<LeaveCarryForwardPreviewResponse>.Fail(
                    validation.StatusCode,
                    validation.Message);
            }

            var workItems = await BuildCandidatesAsync(
                request,
                validation.SourcePeriod!,
                validation.DestinationPeriod!,
                cancellationToken);
            var responses = workItems.Select(x => x.Response).ToList();

            return LeaveCarryForwardServiceResult<LeaveCarryForwardPreviewResponse>.Ok(
                new LeaveCarryForwardPreviewResponse
                {
                    SourceLeaveEntitlementPeriodId = validation.SourcePeriod!.Id,
                    SourcePeriodCode = validation.SourcePeriod.PeriodCode,
                    DestinationLeaveEntitlementPeriodId = validation.DestinationPeriod!.Id,
                    DestinationPeriodCode = validation.DestinationPeriod.PeriodCode,
                    ExecutionDate = request.ExecutionDate,
                    TotalCandidate = responses.Count,
                    EligibleCount = responses.Count(x => x.IsEligible),
                    SkippedCount = responses.Count(x => !x.IsEligible),
                    TotalSourceAvailableDays = responses.Sum(x => x.SourceAvailableDays),
                    TotalCarryForwardDays = responses.Sum(x => x.CarryForwardDays),
                    TotalExpiredDays = responses.Sum(x => x.ExpiredDays),
                    TotalPayoutDays = responses.Sum(x => x.PayoutDays),
                    IsTruncated = responses.Count >= request.MaximumPreviewItem,
                    Items = responses
                },
                "Preview leave carry forward berhasil dihitung.");
        }

        public async Task<LeaveCarryForwardServiceResult<LeaveCarryForwardRunActionResponse>> CreateRunAsync(
            CreateLeaveCarryForwardRunRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var validation = await ValidatePeriodsAsync(
                request.SourceLeaveEntitlementPeriodId,
                request.DestinationLeaveEntitlementPeriodId,
                request.ExecutionDate,
                cancellationToken);
            if (!validation.Success)
            {
                return LeaveCarryForwardServiceResult<LeaveCarryForwardRunActionResponse>.Fail(
                    validation.StatusCode,
                    validation.Message);
            }

            if (!IsValidRunMode(request.RunMode))
            {
                return LeaveCarryForwardServiceResult<LeaveCarryForwardRunActionResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "RunMode tidak valid.");
            }

            if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
            {
                var existing = await _dbContext.Set<TrxLeaveCarryForwardRun>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.IdempotencyKey == request.IdempotencyKey.Trim() &&
                        !x.IsDelete,
                        cancellationToken);
                if (existing != null)
                {
                    return LeaveCarryForwardServiceResult<LeaveCarryForwardRunActionResponse>.Ok(
                        MapAction(existing, true, "Carry-forward run dengan idempotency key tersebut sudah tersedia."),
                        "Carry-forward run sudah tersedia.");
                }
            }

            var run = new TrxLeaveCarryForwardRun
            {
                Id = Guid.NewGuid(),
                SourceLeaveEntitlementPeriodId = request.SourceLeaveEntitlementPeriodId,
                DestinationLeaveEntitlementPeriodId = request.DestinationLeaveEntitlementPeriodId,
                LeaveTypeId = request.LeaveTypeId,
                LeaveCarryForwardPolicyId = request.LeaveCarryForwardPolicyId,
                LegalEntityId = request.LegalEntityId,
                HospitalSiteId = request.HospitalSiteId,
                OrganizationUnitId = request.OrganizationUnitId,
                DepartmentId = request.DepartmentId,
                RunNumber = GenerateRunNumber(),
                RunMode = request.RunMode.Trim(),
                RunStatus = request.QueueForProcessing
                    ? LeaveValueConstants.BatchRunStatus.Queued
                    : LeaveValueConstants.BatchRunStatus.Draft,
                ExecutionDate = request.ExecutionDate,
                IsDryRun = request.IsDryRun,
                ForceReprocess = request.ForceReprocess,
                MaximumRetryCount = request.MaximumRetryCount,
                IdempotencyKey = NullIfWhiteSpace(request.IdempotencyKey),
                CorrelationId = NullIfWhiteSpace(request.CorrelationId),
                TriggeredByUserId = actorUserId == Guid.Empty ? null : actorUserId,
                ParametersJson = JsonSerializer.Serialize(new LeaveCarryForwardRunParameters
                {
                    WorkforceProfileId = request.WorkforceProfileId,
                    MaximumPreviewItem = request.MaximumPreviewItem
                }, JsonOptions),
                Notes = NullIfWhiteSpace(request.Notes),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };

            _dbContext.Add(run);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return LeaveCarryForwardServiceResult<LeaveCarryForwardRunActionResponse>.Ok(
                MapAction(run, false, "Leave carry-forward run berhasil dibuat."),
                "Leave carry-forward run berhasil dibuat.",
                StatusCodes.Status201Created);
        }

        public async Task<LeaveCarryForwardServiceResult<LeaveCarryForwardRunActionResponse>> ExecuteRunAsync(
            Guid runId,
            Guid actorUserId,
            bool forceReprocess,
            string? notes,
            CancellationToken cancellationToken = default,
            bool allowAlreadyRunning = false)
        {
            var run = await _dbContext.Set<TrxLeaveCarryForwardRun>()
                .FirstOrDefaultAsync(x => x.Id == runId && !x.IsDelete, cancellationToken);
            if (run == null)
            {
                return LeaveCarryForwardServiceResult<LeaveCarryForwardRunActionResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Leave carry-forward run tidak ditemukan.");
            }

            if (run.RunStatus == LeaveValueConstants.BatchRunStatus.Completed ||
                run.RunStatus == LeaveValueConstants.BatchRunStatus.Reversed)
            {
                return LeaveCarryForwardServiceResult<LeaveCarryForwardRunActionResponse>.Ok(
                    MapAction(run, true, "Leave carry-forward run sudah selesai."),
                    "Leave carry-forward run sudah selesai.");
            }

            if (run.RunStatus == LeaveValueConstants.BatchRunStatus.Cancelled)
            {
                return LeaveCarryForwardServiceResult<LeaveCarryForwardRunActionResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Run yang sudah dibatalkan tidak dapat dijalankan.");
            }

            if (run.RunStatus == LeaveValueConstants.BatchRunStatus.Running && !allowAlreadyRunning)
            {
                return LeaveCarryForwardServiceResult<LeaveCarryForwardRunActionResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Run sedang diproses oleh worker lain.");
            }

            var validation = await ValidatePeriodsAsync(
                run.SourceLeaveEntitlementPeriodId,
                run.DestinationLeaveEntitlementPeriodId,
                run.ExecutionDate,
                cancellationToken);
            if (!validation.Success)
            {
                return LeaveCarryForwardServiceResult<LeaveCarryForwardRunActionResponse>.Fail(
                    validation.StatusCode,
                    validation.Message);
            }

            run.RunStatus = LeaveValueConstants.BatchRunStatus.Running;
            run.StartedAt = DateTime.UtcNow;
            run.CompletedAt = null;
            run.ErrorSummary = null;
            run.ForceReprocess = run.ForceReprocess || forceReprocess;
            run.Notes = AppendNote(run.Notes, notes);
            run.TriggeredByUserId ??= actorUserId == Guid.Empty ? null : actorUserId;
            run.UpdateDateTime = DateTime.UtcNow;
            run.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            var parameters = DeserializeParameters(run.ParametersJson);
            var previewRequest = new LeaveCarryForwardPreviewRequest
            {
                SourceLeaveEntitlementPeriodId = run.SourceLeaveEntitlementPeriodId,
                DestinationLeaveEntitlementPeriodId = run.DestinationLeaveEntitlementPeriodId,
                LeaveTypeId = run.LeaveTypeId,
                LeaveCarryForwardPolicyId = run.LeaveCarryForwardPolicyId,
                WorkforceProfileId = parameters.WorkforceProfileId,
                LegalEntityId = run.LegalEntityId,
                HospitalSiteId = run.HospitalSiteId,
                OrganizationUnitId = run.OrganizationUnitId,
                DepartmentId = run.DepartmentId,
                ExecutionDate = run.ExecutionDate,
                ForceReprocess = run.ForceReprocess,
                MaximumPreviewItem = Math.Max(parameters.MaximumPreviewItem, 1000)
            };

            var candidates = await BuildCandidatesAsync(
                previewRequest,
                validation.SourcePeriod!,
                validation.DestinationPeriod!,
                cancellationToken);

            run.TargetCount = candidates.Count;
            run.CalculatedCount = candidates.Count;
            run.TotalSourceAvailableDays = candidates.Sum(x => x.Response.SourceAvailableDays);
            run.TotalEligibleDays = candidates.Sum(x => x.Response.EligibleDays);
            run.TotalCarryForwardDays = candidates.Sum(x => x.Response.CarryForwardDays);
            run.TotalExpiredDays = candidates.Sum(x => x.Response.ExpiredDays);
            run.TotalExcessDays = candidates.Sum(x => x.Response.ExcessDays);
            run.TotalPayoutDays = candidates.Sum(x => x.Response.PayoutDays);
            run.PostedCount = 0;
            run.SkippedCount = 0;
            run.FailedCount = 0;
            await _dbContext.SaveChangesAsync(cancellationToken);

            if (run.IsDryRun)
            {
                foreach (var candidate in candidates)
                {
                    if (candidate.Response.IsEligible)
                    {
                        await SaveCalculatedCandidateAsync(run, candidate, actorUserId, cancellationToken);
                    }
                    else
                    {
                        await SaveSkippedCandidateAsync(run, candidate, actorUserId, cancellationToken);
                    }
                }

                run.PostedCount = 0;
                run.SkippedCount = candidates.Count(x => !x.Response.IsEligible);
                run.FailedCount = 0;
                run.RunStatus = LeaveValueConstants.BatchRunStatus.Completed;
                run.CompletedAt = DateTime.UtcNow;
                run.ResultSummaryJson = JsonSerializer.Serialize(new
                {
                    dryRun = true,
                    run.TargetCount,
                    run.CalculatedCount,
                    run.SkippedCount,
                    run.TotalCarryForwardDays,
                    run.TotalExpiredDays,
                    run.TotalPayoutDays
                }, JsonOptions);
                run.UpdateDateTime = DateTime.UtcNow;
                run.UpdateBy = actorUserId;
                await _dbContext.SaveChangesAsync(cancellationToken);

                return LeaveCarryForwardServiceResult<LeaveCarryForwardRunActionResponse>.Ok(
                    MapAction(run, false, "Dry-run leave carry forward selesai dihitung tanpa posting saldo."),
                    "Dry-run leave carry forward selesai dihitung tanpa posting saldo.");
            }

            var errors = new List<string>();
            foreach (var candidate in candidates)
            {
                if (!candidate.Response.IsEligible)
                {
                    await SaveSkippedCandidateAsync(run, candidate, actorUserId, cancellationToken);
                    run.SkippedCount += 1;
                    continue;
                }

                try
                {
                    var itemResult = await PostCandidateAsync(
                        run.Id,
                        candidate,
                        actorUserId,
                        run.ForceReprocess,
                        cancellationToken);
                    if (itemResult.Success)
                    {
                        run.PostedCount += itemResult.Data?.IsIdempotent == true ? 0 : 1;
                    }
                    else
                    {
                        run.FailedCount += 1;
                        errors.Add(itemResult.Message);
                    }
                }
                catch (Exception ex)
                {
                    run.FailedCount += 1;
                    errors.Add($"{candidate.Response.WorkforceDisplayName ?? candidate.Response.WorkforceProfileId.ToString()}: {ex.Message}");
                    _logger.LogError(ex, "Posting carry forward gagal. RunId={RunId}, WorkforceProfileId={WorkforceProfileId}", run.Id, candidate.Response.WorkforceProfileId);
                }
            }

            var actualDetails = await _dbContext.Set<TrxLeaveCarryForward>()
                .AsNoTracking()
                .Where(x => x.LeaveCarryForwardRunId == run.Id && !x.IsDelete)
                .ToListAsync(cancellationToken);
            run.PostedCount = actualDetails.Count(x => x.CarryForwardStatus == LeaveValueConstants.CarryForwardStatus.Posted);
            run.SkippedCount = actualDetails.Count(x => x.CarryForwardStatus == LeaveValueConstants.CarryForwardStatus.Skipped);
            run.FailedCount = Math.Max(run.FailedCount, actualDetails.Count(x => x.CarryForwardStatus == LeaveValueConstants.CarryForwardStatus.Failed));

            run.CompletedAt = DateTime.UtcNow;
            run.RunStatus = run.FailedCount == 0
                ? LeaveValueConstants.BatchRunStatus.Completed
                : run.PostedCount > 0 || run.SkippedCount > 0
                    ? LeaveValueConstants.BatchRunStatus.CompletedWithErrors
                    : LeaveValueConstants.BatchRunStatus.Failed;
            run.ErrorSummary = errors.Count == 0 ? null : string.Join(" | ", errors.Take(20));
            run.ResultSummaryJson = JsonSerializer.Serialize(new
            {
                run.TargetCount,
                run.CalculatedCount,
                run.PostedCount,
                run.SkippedCount,
                run.FailedCount,
                run.TotalCarryForwardDays,
                run.TotalExpiredDays,
                run.TotalPayoutDays
            }, JsonOptions);
            run.UpdateDateTime = DateTime.UtcNow;
            run.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return LeaveCarryForwardServiceResult<LeaveCarryForwardRunActionResponse>.Ok(
                MapAction(run, false, "Leave carry-forward run selesai diproses."),
                "Leave carry-forward run selesai diproses.");
        }

        public async Task<LeaveCarryForwardServiceResult<LeaveCarryForwardRunActionResponse>> RetryAsync(
            Guid id,
            Guid actorUserId,
            string reason,
            CancellationToken cancellationToken = default)
        {
            var run = await _dbContext.Set<TrxLeaveCarryForwardRun>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (run == null)
            {
                return LeaveCarryForwardServiceResult<LeaveCarryForwardRunActionResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Leave carry-forward run tidak ditemukan.");
            }

            if (run.RunStatus != LeaveValueConstants.BatchRunStatus.Failed &&
                run.RunStatus != LeaveValueConstants.BatchRunStatus.CompletedWithErrors)
            {
                return LeaveCarryForwardServiceResult<LeaveCarryForwardRunActionResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Hanya run Failed atau CompletedWithErrors yang dapat dijadwalkan ulang.");
            }

            if (run.RetryCount >= run.MaximumRetryCount)
            {
                return LeaveCarryForwardServiceResult<LeaveCarryForwardRunActionResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Maximum retry count sudah tercapai.");
            }

            run.RetryCount += 1;
            run.RunMode = LeaveValueConstants.BatchRunMode.Reprocess;
            run.RunStatus = LeaveValueConstants.BatchRunStatus.Queued;
            run.ForceReprocess = true;
            run.StartedAt = null;
            run.CompletedAt = null;
            run.ErrorSummary = null;
            run.Notes = AppendNote(run.Notes, reason);
            run.UpdateDateTime = DateTime.UtcNow;
            run.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return LeaveCarryForwardServiceResult<LeaveCarryForwardRunActionResponse>.Ok(
                MapAction(run, false, "Leave carry-forward run berhasil dijadwalkan ulang."),
                "Leave carry-forward run berhasil dijadwalkan ulang.");
        }

        public async Task<LeaveCarryForwardServiceResult<LeaveCarryForwardRunActionResponse>> CancelAsync(
            Guid id,
            Guid actorUserId,
            string reason,
            CancellationToken cancellationToken = default)
        {
            var run = await _dbContext.Set<TrxLeaveCarryForwardRun>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (run == null)
            {
                return LeaveCarryForwardServiceResult<LeaveCarryForwardRunActionResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Leave carry-forward run tidak ditemukan.");
            }

            if (run.RunStatus != LeaveValueConstants.BatchRunStatus.Draft &&
                run.RunStatus != LeaveValueConstants.BatchRunStatus.Queued)
            {
                return LeaveCarryForwardServiceResult<LeaveCarryForwardRunActionResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Hanya run Draft atau Queued yang dapat dibatalkan.");
            }

            run.RunStatus = LeaveValueConstants.BatchRunStatus.Cancelled;
            run.CancelledAt = DateTime.UtcNow;
            run.CancelledByUserId = actorUserId == Guid.Empty ? null : actorUserId;
            run.Notes = AppendNote(run.Notes, reason);
            run.IsCancel = true;
            run.CancelDateTime = DateTime.UtcNow;
            run.CancelBy = actorUserId;
            run.UpdateDateTime = DateTime.UtcNow;
            run.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return LeaveCarryForwardServiceResult<LeaveCarryForwardRunActionResponse>.Ok(
                MapAction(run, false, "Leave carry-forward run berhasil dibatalkan."),
                "Leave carry-forward run berhasil dibatalkan.");
        }

        public async Task<LeaveCarryForwardServiceResult<LeaveCarryForwardRunActionResponse>> ReverseRunAsync(
            Guid id,
            Guid actorUserId,
            string reason,
            CancellationToken cancellationToken = default)
        {
            var run = await _dbContext.Set<TrxLeaveCarryForwardRun>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (run == null)
            {
                return LeaveCarryForwardServiceResult<LeaveCarryForwardRunActionResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Leave carry-forward run tidak ditemukan.");
            }

            if (run.RunStatus == LeaveValueConstants.BatchRunStatus.Reversed)
            {
                return LeaveCarryForwardServiceResult<LeaveCarryForwardRunActionResponse>.Ok(
                    MapAction(run, true, "Leave carry-forward run sudah direversal."),
                    "Leave carry-forward run sudah direversal.");
            }

            if (run.RunStatus != LeaveValueConstants.BatchRunStatus.Completed &&
                run.RunStatus != LeaveValueConstants.BatchRunStatus.CompletedWithErrors)
            {
                return LeaveCarryForwardServiceResult<LeaveCarryForwardRunActionResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Hanya run yang sudah selesai yang dapat direversal.");
            }

            var details = await _dbContext.Set<TrxLeaveCarryForward>()
                .AsNoTracking()
                .Where(x =>
                    x.LeaveCarryForwardRunId == id &&
                    x.CarryForwardStatus == LeaveValueConstants.CarryForwardStatus.Posted &&
                    !x.IsDelete)
                .OrderByDescending(x => x.PostedAt)
                .ToListAsync(cancellationToken);

            var failed = 0;
            var reversed = 0;
            var errors = new List<string>();
            foreach (var detail in details)
            {
                var result = await ReverseDetailAsync(detail.Id, actorUserId, reason, cancellationToken);
                if (result.Success)
                {
                    reversed += 1;
                }
                else
                {
                    failed += 1;
                    errors.Add(result.Message);
                }
            }

            var trackedRun = await _dbContext.Set<TrxLeaveCarryForwardRun>()
                .FirstAsync(x => x.Id == id, cancellationToken);
            if (failed == 0)
            {
                trackedRun.RunStatus = LeaveValueConstants.BatchRunStatus.Reversed;
            }
            else
            {
                trackedRun.RunStatus = LeaveValueConstants.BatchRunStatus.CompletedWithErrors;
                trackedRun.ErrorSummary = string.Join(" | ", errors.Take(20));
            }
            trackedRun.Notes = AppendNote(trackedRun.Notes, $"Reversal: {reason}. Reversed={reversed}, Failed={failed}.");
            trackedRun.UpdateDateTime = DateTime.UtcNow;
            trackedRun.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return failed == 0
                ? LeaveCarryForwardServiceResult<LeaveCarryForwardRunActionResponse>.Ok(
                    MapAction(trackedRun, false, "Leave carry-forward run berhasil direversal."),
                    "Leave carry-forward run berhasil direversal.")
                : LeaveCarryForwardServiceResult<LeaveCarryForwardRunActionResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Reversal belum selesai. Berhasil={reversed}, Gagal={failed}.");
        }

        public async Task<LeaveCarryForwardServiceResult<LeaveCarryForwardReconciliationResponse>> ReconcileAsync(
            Guid runId,
            CancellationToken cancellationToken = default)
        {
            var run = await _dbContext.Set<TrxLeaveCarryForwardRun>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == runId && !x.IsDelete, cancellationToken);
            if (run == null)
            {
                return LeaveCarryForwardServiceResult<LeaveCarryForwardReconciliationResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Leave carry-forward run tidak ditemukan.");
            }

            var details = await _dbContext.Set<TrxLeaveCarryForward>()
                .AsNoTracking()
                .Where(x => x.LeaveCarryForwardRunId == runId && !x.IsDelete)
                .ToListAsync(cancellationToken);
            var detailIds = details.Select(x => x.Id).ToList();
            var ledgers = await _dbContext.Set<TrxLeaveBalanceTransaction>()
                .AsNoTracking()
                .Where(x =>
                    x.PostingBatchType == LeaveValueConstants.PostingBatchType.CarryForwardRun &&
                    x.PostingBatchId == runId &&
                    !x.IsDelete)
                .ToListAsync(cancellationToken);

            var issues = new List<LeaveCarryForwardReconciliationIssueResponse>();
            foreach (var detail in details.Where(x => x.CarryForwardStatus == LeaveValueConstants.CarryForwardStatus.Posted))
            {
                var itemLedgers = ledgers.Where(x => x.LeaveCarryForwardId == detail.Id && x.TransactionStatus == LeaveValueConstants.TransactionStatus.Posted).ToList();
                if (detail.CarryForwardDays > Tolerance)
                {
                    var sourceOut = itemLedgers.Where(x => x.LeaveBalanceId == detail.SourceLeaveBalanceId && x.TransactionType == LeaveValueConstants.TransactionType.CarryForward).Sum(x => x.CarryForwardDelta);
                    var destinationIn = itemLedgers.Where(x => x.LeaveBalanceId == detail.DestinationLeaveBalanceId && x.TransactionType == LeaveValueConstants.TransactionType.CarryForward).Sum(x => x.CarryForwardDelta);
                    if (Math.Abs(sourceOut + detail.CarryForwardDays) > Tolerance ||
                        Math.Abs(destinationIn - detail.CarryForwardDays) > Tolerance)
                    {
                        issues.Add(Issue("CARRY_FORWARD_LEDGER_MISMATCH", "Critical", $"Carry forward {detail.CarryForwardNumber} tidak sesuai dengan ledger source/destination.", detail.Id));
                    }
                }

                var expiry = itemLedgers.Where(x => x.TransactionType == LeaveValueConstants.TransactionType.Expiry).Sum(x => x.ExpiredDelta);
                if (Math.Abs(expiry - detail.ExpiredDays) > Tolerance)
                {
                    issues.Add(Issue("EXPIRY_LEDGER_MISMATCH", "Critical", $"Expired days {detail.CarryForwardNumber} tidak sesuai ledger.", detail.Id));
                }

                var payout = itemLedgers.Where(x => x.TransactionType == LeaveValueConstants.TransactionType.Encashment).Sum(x => x.EncashmentDelta);
                if (Math.Abs(payout - detail.PayoutDays) > Tolerance)
                {
                    issues.Add(Issue("PAYOUT_LEDGER_MISMATCH", "Critical", $"Payout days {detail.CarryForwardNumber} tidak sesuai ledger.", detail.Id));
                }
            }

            foreach (var ledger in ledgers.Where(x => x.LeaveCarryForwardId.HasValue && !detailIds.Contains(x.LeaveCarryForwardId.Value)))
            {
                issues.Add(new LeaveCarryForwardReconciliationIssueResponse
                {
                    Code = "ORPHAN_CARRY_FORWARD_LEDGER",
                    Severity = "Critical",
                    Message = $"Ledger {ledger.TransactionNumber} tidak mempunyai carry-forward detail aktif.",
                    LeaveCarryForwardId = ledger.LeaveCarryForwardId,
                    LeaveBalanceTransactionId = ledger.Id
                });
            }

            var postedDetails = details.Where(x => x.CarryForwardStatus == LeaveValueConstants.CarryForwardStatus.Posted).ToList();
            var postedLedgers = ledgers.Where(x => x.TransactionStatus == LeaveValueConstants.TransactionStatus.Posted).ToList();
            var response = new LeaveCarryForwardReconciliationResponse
            {
                LeaveCarryForwardRunId = run.Id,
                RunNumber = run.RunNumber,
                RunStatus = run.RunStatus,
                RunPostedCount = run.PostedCount,
                ActualPostedDetailCount = postedDetails.Count,
                PostedLedgerCount = postedLedgers.Count,
                RunTotalCarryForwardDays = run.TotalCarryForwardDays,
                ActualCarryForwardDays = postedDetails.Sum(x => x.CarryForwardDays),
                DestinationCarryForwardLedgerDays = postedLedgers.Where(x => x.CarryForwardDelta > 0).Sum(x => x.CarryForwardDelta),
                SourceCarryForwardLedgerDays = Math.Abs(postedLedgers.Where(x => x.CarryForwardDelta < 0).Sum(x => x.CarryForwardDelta)),
                RunTotalExpiredDays = run.TotalExpiredDays,
                LedgerExpiredDays = postedLedgers.Sum(x => x.ExpiredDelta),
                RunTotalPayoutDays = run.TotalPayoutDays,
                LedgerPayoutDays = postedLedgers.Sum(x => x.EncashmentDelta),
                Issues = issues
            };

            if (run.PostedCount != response.ActualPostedDetailCount)
            {
                response.Issues.Add(Issue("RUN_POSTED_COUNT_MISMATCH", "Warning", "PostedCount run tidak sama dengan detail Posted."));
            }
            if (Math.Abs(run.TotalCarryForwardDays - response.ActualCarryForwardDays) > Tolerance)
            {
                response.Issues.Add(Issue("RUN_CARRY_FORWARD_TOTAL_MISMATCH", "Warning", "Total carry-forward run tidak sama dengan total detail Posted."));
            }
            response.IsBalanced = response.Issues.All(x => x.Severity != "Critical");

            return LeaveCarryForwardServiceResult<LeaveCarryForwardReconciliationResponse>.Ok(
                response,
                "Reconciliation leave carry-forward run berhasil dihitung.");
        }

        public async Task<LeaveCarryForwardServiceResult<LeaveCarryForwardExpiryResponse>> ProcessExpiryAsync(
            LeaveCarryForwardExpiryRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Set<TrxLeaveCarryForward>()
                .AsNoTracking()
                .Where(x =>
                    x.CarryForwardStatus == LeaveValueConstants.CarryForwardStatus.Posted &&
                    x.DestinationLeaveBalanceId.HasValue &&
                    x.CarryForwardExpiryDate.HasValue &&
                    x.CarryForwardExpiryDate.Value <= request.AsOfDate &&
                    x.CarryForwardDays > 0 &&
                    !x.IsDelete);

            if (request.DestinationLeaveEntitlementPeriodId.HasValue)
            {
                query = query.Where(x => x.DestinationLeaveEntitlementPeriodId == request.DestinationLeaveEntitlementPeriodId.Value);
            }
            if (request.LeaveTypeId.HasValue)
            {
                query = query.Where(x => x.DestinationLeaveTypeId == request.LeaveTypeId.Value);
            }
            if (request.WorkforceProfileId.HasValue)
            {
                query = query.Where(x => x.WorkforceProfileId == request.WorkforceProfileId.Value);
            }

            var candidates = await query
                .OrderBy(x => x.CarryForwardExpiryDate)
                .ThenBy(x => x.PostedAt)
                .Take(request.MaximumItem)
                .ToListAsync(cancellationToken);
            var response = new LeaveCarryForwardExpiryResponse
            {
                AsOfDate = request.AsOfDate,
                CandidateCount = candidates.Count,
                IsDryRun = request.IsDryRun,
                IsTruncated = candidates.Count >= request.MaximumItem
            };

            foreach (var candidate in candidates)
            {
                var item = await ProcessExpiryItemAsync(candidate, request, actorUserId, cancellationToken);
                response.Items.Add(item);
                if (item.Success)
                {
                    if (item.IsIdempotent || item.PostedExpiredDays <= 0)
                    {
                        response.SkippedCount += 1;
                    }
                    else
                    {
                        response.PostedCount += 1;
                        response.TotalExpiredDays += item.PostedExpiredDays;
                    }
                }
                else
                {
                    response.FailedCount += 1;
                }
            }

            return LeaveCarryForwardServiceResult<LeaveCarryForwardExpiryResponse>.Ok(
                response,
                request.IsDryRun
                    ? "Preview expiry carry-forward berhasil dihitung."
                    : "Expiry carry-forward selesai diproses.");
        }

        internal async Task<LeaveCarryForwardServiceResult<LeaveCarryForwardRunActionResponse>> PostCandidateAsync(
            Guid runId,
            LeaveCarryForwardCandidateWorkItem candidate,
            Guid actorUserId,
            bool forceReprocess,
            CancellationToken cancellationToken)
        {
            var key = BuildItemIdempotencyKey(runId, candidate.Response);
            await using IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                var existing = await _dbContext.Set<TrxLeaveCarryForward>()
                    .FirstOrDefaultAsync(x => x.IdempotencyKey == key && !x.IsDelete, cancellationToken);
                if (existing != null && existing.CarryForwardStatus == LeaveValueConstants.CarryForwardStatus.Posted)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return LeaveCarryForwardServiceResult<LeaveCarryForwardRunActionResponse>.Ok(
                        new LeaveCarryForwardRunActionResponse { Id = runId, IsIdempotent = true, Message = "Carry-forward item sudah pernah diposting." },
                        "Carry-forward item sudah pernah diposting.");
                }

                var source = await LockBalanceAsync(candidate.SourceBalance.Id, cancellationToken);
                if (source == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return LeaveCarryForwardServiceResult<LeaveCarryForwardRunActionResponse>.Fail(StatusCodes.Status409Conflict, "Source leave balance tidak ditemukan.");
                }
                if (source.IsLocked || source.BalanceStatus == LeaveValueConstants.BalanceStatus.Locked || source.BalanceStatus == LeaveValueConstants.BalanceStatus.Closed)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return LeaveCarryForwardServiceResult<LeaveCarryForwardRunActionResponse>.Fail(StatusCodes.Status409Conflict, "Source leave balance sedang dikunci atau ditutup.");
                }

                var destination = await GetOrCreateDestinationBalanceAsync(candidate, actorUserId, cancellationToken);
                if (destination.IsLocked || destination.BalanceStatus == LeaveValueConstants.BalanceStatus.Locked || destination.BalanceStatus == LeaveValueConstants.BalanceStatus.Closed)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return LeaveCarryForwardServiceResult<LeaveCarryForwardRunActionResponse>.Fail(StatusCodes.Status409Conflict, "Destination leave balance sedang dikunci atau ditutup.");
                }

                var detail = existing ?? new TrxLeaveCarryForward
                {
                    Id = Guid.NewGuid(),
                    LeaveCarryForwardRunId = runId,
                    LeaveCarryForwardPolicyId = candidate.Policy.Id,
                    SourceLeaveEntitlementPeriodId = candidate.SourcePeriod.Id,
                    DestinationLeaveEntitlementPeriodId = candidate.DestinationPeriod.Id,
                    WorkforceProfileId = source.WorkforceProfileId,
                    SourceLeaveTypeId = source.LeaveTypeId,
                    DestinationLeaveTypeId = candidate.Response.DestinationLeaveTypeId,
                    SourceLeaveBalanceId = source.Id,
                    CarryForwardNumber = GenerateDetailNumber(),
                    IdempotencyKey = key,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId,
                    IsActive = true
                };

                detail.DestinationLeaveBalanceId = destination.Id;
                detail.CalculationDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);
                detail.CarryForwardExpiryDate = candidate.Response.CarryForwardExpiryDate;
                detail.SourceAvailableDays = candidate.Response.SourceAvailableDays;
                detail.EligibleDays = candidate.Response.EligibleDays;
                detail.CarryForwardDays = candidate.Response.CarryForwardDays;
                detail.ExpiredDays = candidate.Response.ExpiredDays;
                detail.ExcessDays = candidate.Response.ExcessDays;
                detail.PayoutDays = candidate.Response.PayoutDays;
                detail.RoundingAdjustmentDays = candidate.Response.RoundingAdjustmentDays;
                detail.CarryForwardStatus = LeaveValueConstants.CarryForwardStatus.Calculated;
                detail.CalculatedAt = DateTime.UtcNow;
                detail.CalculatedByUserId = actorUserId == Guid.Empty ? null : actorUserId;
                detail.SourceBalanceSnapshotJson = JsonSerializer.Serialize(new
                {
                    source.Id,
                    source.WorkforceProfileId,
                    source.LeaveTypeId,
                    source.OpeningBalanceDays,
                    source.EntitlementDays,
                    source.AccruedDays,
                    source.CarriedForwardDays,
                    source.AdjustmentDays,
                    source.ReservedDays,
                    source.UsedDays,
                    source.ExpiredDays,
                    source.EncashmentDays,
                    source.RemainingDays,
                    source.AvailableDays,
                    source.BalanceVersion
                }, JsonOptions);
                detail.CalculationDetailJson = candidate.Response.CalculationDetailJson;
                detail.ErrorMessage = null;
                detail.UpdateDateTime = DateTime.UtcNow;
                detail.UpdateBy = actorUserId;
                if (existing == null)
                {
                    _dbContext.Add(detail);
                }
                await _dbContext.SaveChangesAsync(cancellationToken);

                if (candidate.Response.CarryForwardDays > 0)
                {
                    var sourceLedger = CreateLedger(
                        source,
                        runId,
                        detail,
                        LeaveValueConstants.TransactionType.CarryForward,
                        LeaveValueConstants.TransactionDirection.Debit,
                        candidate.Response.CarryForwardDays,
                        actorUserId,
                        $"Carry forward keluar ke periode {candidate.DestinationPeriod.PeriodCode}.",
                        $"{key}:SOURCE",
                        carryForwardDelta: -candidate.Response.CarryForwardDays,
                        availableDelta: -candidate.Response.CarryForwardDays);
                    ApplyLedgerToBalance(source, sourceLedger);
                    _dbContext.Add(sourceLedger);

                    var destinationLedger = CreateLedger(
                        destination,
                        runId,
                        detail,
                        LeaveValueConstants.TransactionType.CarryForward,
                        LeaveValueConstants.TransactionDirection.Credit,
                        candidate.Response.CarryForwardDays,
                        actorUserId,
                        $"Carry forward masuk dari periode {candidate.SourcePeriod.PeriodCode}.",
                        $"{key}:DESTINATION",
                        carryForwardDelta: candidate.Response.CarryForwardDays,
                        availableDelta: candidate.Response.CarryForwardDays);
                    ApplyLedgerToBalance(destination, destinationLedger);
                    _dbContext.Add(destinationLedger);

                    if (candidate.Response.CarryForwardExpiryDate.HasValue)
                    {
                        destination.CarryForwardExpiryDate = destination.CarryForwardExpiryDate.HasValue
                            ? (destination.CarryForwardExpiryDate.Value <= candidate.Response.CarryForwardExpiryDate.Value
                                ? destination.CarryForwardExpiryDate
                                : candidate.Response.CarryForwardExpiryDate)
                            : candidate.Response.CarryForwardExpiryDate;
                    }
                }

                if (candidate.Response.ExpiredDays > 0)
                {
                    var expiryLedger = CreateLedger(
                        source,
                        runId,
                        detail,
                        LeaveValueConstants.TransactionType.Expiry,
                        LeaveValueConstants.TransactionDirection.Debit,
                        candidate.Response.ExpiredDays,
                        actorUserId,
                        "Saldo berlebih hangus saat carry forward.",
                        $"{key}:EXPIRY",
                        expiredDelta: candidate.Response.ExpiredDays,
                        availableDelta: -candidate.Response.ExpiredDays);
                    ApplyLedgerToBalance(source, expiryLedger);
                    _dbContext.Add(expiryLedger);
                }

                if (candidate.Response.PayoutDays > 0)
                {
                    var payoutLedger = CreateLedger(
                        source,
                        runId,
                        detail,
                        LeaveValueConstants.TransactionType.Encashment,
                        LeaveValueConstants.TransactionDirection.Debit,
                        candidate.Response.PayoutDays,
                        actorUserId,
                        "Saldo berlebih dialihkan menjadi payout/encashment.",
                        $"{key}:PAYOUT",
                        encashmentDelta: candidate.Response.PayoutDays,
                        availableDelta: -candidate.Response.PayoutDays);
                    ApplyLedgerToBalance(source, payoutLedger);
                    _dbContext.Add(payoutLedger);
                }

                detail.CarryForwardStatus = LeaveValueConstants.CarryForwardStatus.Posted;
                detail.PostedAt = DateTime.UtcNow;
                detail.PostedByUserId = actorUserId == Guid.Empty ? null : actorUserId;
                source.LastCalculatedAt = DateTime.UtcNow;
                destination.LastCalculatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return LeaveCarryForwardServiceResult<LeaveCarryForwardRunActionResponse>.Ok(
                    new LeaveCarryForwardRunActionResponse { Id = runId, Message = "Carry-forward item berhasil diposting." },
                    "Carry-forward item berhasil diposting.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Posting carry-forward item gagal. RunId={RunId}, WorkforceProfileId={WorkforceProfileId}", runId, candidate.Response.WorkforceProfileId);
                return LeaveCarryForwardServiceResult<LeaveCarryForwardRunActionResponse>.Fail(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        private async Task<LeaveCarryForwardExpiryItemResponse> ProcessExpiryItemAsync(
            TrxLeaveCarryForward detail,
            LeaveCarryForwardExpiryRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var response = new LeaveCarryForwardExpiryItemResponse
            {
                LeaveCarryForwardId = detail.Id,
                CarryForwardNumber = detail.CarryForwardNumber,
                WorkforceProfileId = detail.WorkforceProfileId,
                DestinationLeaveBalanceId = detail.DestinationLeaveBalanceId!.Value,
                CarryForwardExpiryDate = detail.CarryForwardExpiryDate!.Value,
                OriginalCarryForwardDays = detail.CarryForwardDays
            };

            var previouslyExpired = await _dbContext.Set<TrxLeaveBalanceTransaction>()
                .AsNoTracking()
                .Where(x =>
                    x.LeaveCarryForwardId == detail.Id &&
                    x.TransactionType == LeaveValueConstants.TransactionType.Expiry &&
                    x.SourceType == "CarryForwardExpiry" &&
                    x.TransactionStatus == LeaveValueConstants.TransactionStatus.Posted &&
                    !x.IsDelete)
                .SumAsync(x => (decimal?)x.ExpiredDelta, cancellationToken) ?? 0;
            response.PreviouslyExpiredDays = previouslyExpired;

            var destination = await _dbContext.Set<WfpLeaveBalance>()
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .FirstOrDefaultAsync(x => x.Id == detail.DestinationLeaveBalanceId.Value && !x.IsDelete, cancellationToken);
            response.WorkforceDisplayName = destination?.WorkforceProfile?.DisplayName;
            if (destination == null)
            {
                response.ResultCode = "DESTINATION_BALANCE_NOT_FOUND";
                response.Message = "Destination leave balance tidak ditemukan.";
                return response;
            }

            var remainingLot = Math.Max(0, detail.CarryForwardDays - previouslyExpired);
            var expirable = Math.Min(remainingLot, Math.Max(0, Math.Min(destination.AvailableDays, destination.CarriedForwardDays)));
            response.ExpirableDays = expirable;
            if (expirable <= Tolerance)
            {
                response.Success = true;
                response.IsIdempotent = true;
                response.ResultCode = "NOTHING_TO_EXPIRE";
                response.Message = "Tidak ada saldo carry forward yang masih dapat di-expire.";
                return response;
            }

            if (request.IsDryRun)
            {
                response.Success = true;
                response.ResultCode = "PREVIEW";
                response.Message = "Saldo carry forward siap di-expire.";
                return response;
            }

            var key = $"CF-EXPIRY:{detail.Id:N}:{request.AsOfDate:yyyyMMdd}";
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                var existing = await _dbContext.Set<TrxLeaveBalanceTransaction>()
                    .AsNoTracking()
                    .AnyAsync(x => x.IdempotencyKey == key && !x.IsDelete, cancellationToken);
                if (existing)
                {
                    await transaction.CommitAsync(cancellationToken);
                    response.Success = true;
                    response.IsIdempotent = true;
                    response.ResultCode = "ALREADY_PROCESSED";
                    response.Message = "Expiry untuk carry-forward item dan tanggal tersebut sudah diproses.";
                    return response;
                }

                var lockedBalance = await LockBalanceAsync(detail.DestinationLeaveBalanceId.Value, cancellationToken);
                if (lockedBalance == null || lockedBalance.IsLocked || lockedBalance.BalanceStatus == LeaveValueConstants.BalanceStatus.Closed)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    response.ResultCode = "BALANCE_LOCKED";
                    response.Message = "Destination balance tidak ditemukan atau sedang dikunci.";
                    return response;
                }

                var amount = Math.Min(expirable, Math.Max(0, Math.Min(lockedBalance.AvailableDays, lockedBalance.CarriedForwardDays)));
                if (amount <= Tolerance)
                {
                    await transaction.CommitAsync(cancellationToken);
                    response.Success = true;
                    response.IsIdempotent = true;
                    response.ResultCode = "NOTHING_TO_EXPIRE";
                    response.Message = "Tidak ada saldo yang dapat di-expire setelah locking.";
                    return response;
                }

                var ledger = CreateLedger(
                    lockedBalance,
                    detail.LeaveCarryForwardRunId,
                    detail,
                    LeaveValueConstants.TransactionType.Expiry,
                    LeaveValueConstants.TransactionDirection.Debit,
                    amount,
                    actorUserId,
                    request.Notes ?? "Expiry saldo carry forward yang telah jatuh tempo.",
                    key,
                    expiredDelta: amount,
                    availableDelta: -amount,
                    sourceType: "CarryForwardExpiry");
                ApplyLedgerToBalance(lockedBalance, ledger);
                _dbContext.Add(ledger);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                response.Success = true;
                response.PostedExpiredDays = amount;
                response.ResultCode = "POSTED";
                response.Message = "Expiry saldo carry forward berhasil diposting.";
                return response;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                response.ResultCode = "ERROR";
                response.Message = ex.Message;
                return response;
            }
        }

        private async Task<LeaveCarryForwardServiceResult<bool>> ReverseDetailAsync(
            Guid detailId,
            Guid actorUserId,
            string reason,
            CancellationToken cancellationToken)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                var detail = await _dbContext.Set<TrxLeaveCarryForward>()
                    .FirstOrDefaultAsync(x => x.Id == detailId && !x.IsDelete, cancellationToken);
                if (detail == null)
                {
                    return LeaveCarryForwardServiceResult<bool>.Fail(StatusCodes.Status404NotFound, "Carry-forward detail tidak ditemukan.");
                }
                if (detail.CarryForwardStatus == LeaveValueConstants.CarryForwardStatus.Reversed)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return LeaveCarryForwardServiceResult<bool>.Ok(true, "Carry-forward detail sudah direversal.");
                }

                var ledgers = await _dbContext.Set<TrxLeaveBalanceTransaction>()
                    .Where(x =>
                        x.LeaveCarryForwardId == detail.Id &&
                        x.TransactionStatus == LeaveValueConstants.TransactionStatus.Posted &&
                        !x.IsDelete)
                    .OrderByDescending(x => x.TransactionSequence)
                    .ToListAsync(cancellationToken);

                foreach (var original in ledgers)
                {
                    var balance = await LockBalanceAsync(original.LeaveBalanceId, cancellationToken);
                    if (balance == null || balance.IsLocked || balance.BalanceStatus == LeaveValueConstants.BalanceStatus.Closed)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return LeaveCarryForwardServiceResult<bool>.Fail(StatusCodes.Status409Conflict, "Balance terkait reversal sedang dikunci atau tidak ditemukan.");
                    }

                    var reversal = CreateLedger(
                        balance,
                        detail.LeaveCarryForwardRunId,
                        detail,
                        LeaveValueConstants.TransactionType.Reversal,
                        original.Direction == LeaveValueConstants.TransactionDirection.Credit
                            ? LeaveValueConstants.TransactionDirection.Debit
                            : LeaveValueConstants.TransactionDirection.Credit,
                        Math.Abs(original.TransactionDays),
                        actorUserId,
                        $"Reversal {original.TransactionNumber}: {reason}",
                        $"CF-REVERSAL:{original.Id:N}",
                        carryForwardDelta: -original.CarryForwardDelta,
                        expiredDelta: -original.ExpiredDelta,
                        encashmentDelta: -original.EncashmentDelta,
                        availableDelta: -original.AvailableDelta,
                        sourceType: "CarryForwardReversal");
                    reversal.OriginalTransactionId = original.Id;
                    ApplyLedgerToBalance(balance, reversal);
                    _dbContext.Add(reversal);
                    await _dbContext.SaveChangesAsync(cancellationToken);

                    original.TransactionStatus = LeaveValueConstants.TransactionStatus.Reversed;
                    original.ReversedAt = DateTime.UtcNow;
                    original.ReversedByUserId = actorUserId == Guid.Empty ? null : actorUserId;
                    original.ReversedTransactionId = reversal.Id;
                    original.UpdateDateTime = DateTime.UtcNow;
                    original.UpdateBy = actorUserId;
                }

                detail.CarryForwardStatus = LeaveValueConstants.CarryForwardStatus.Reversed;
                detail.ReversedAt = DateTime.UtcNow;
                detail.ReversedByUserId = actorUserId == Guid.Empty ? null : actorUserId;
                detail.Notes = AppendNote(detail.Notes, reason);
                detail.UpdateDateTime = DateTime.UtcNow;
                detail.UpdateBy = actorUserId;
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return LeaveCarryForwardServiceResult<bool>.Ok(true, "Carry-forward detail berhasil direversal.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return LeaveCarryForwardServiceResult<bool>.Fail(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        private async Task SaveCalculatedCandidateAsync(
            TrxLeaveCarryForwardRun run,
            LeaveCarryForwardCandidateWorkItem candidate,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            if (candidate.Policy.Id == Guid.Empty)
            {
                return;
            }

            var key = BuildItemIdempotencyKey(run.Id, candidate.Response);
            var existing = await _dbContext.Set<TrxLeaveCarryForward>()
                .FirstOrDefaultAsync(x => x.IdempotencyKey == key && !x.IsDelete, cancellationToken);
            if (existing != null)
            {
                return;
            }

            _dbContext.Add(new TrxLeaveCarryForward
            {
                Id = Guid.NewGuid(),
                LeaveCarryForwardRunId = run.Id,
                LeaveCarryForwardPolicyId = candidate.Policy.Id,
                SourceLeaveEntitlementPeriodId = candidate.SourcePeriod.Id,
                DestinationLeaveEntitlementPeriodId = candidate.DestinationPeriod.Id,
                WorkforceProfileId = candidate.SourceBalance.WorkforceProfileId,
                SourceLeaveTypeId = candidate.SourceBalance.LeaveTypeId,
                DestinationLeaveTypeId = candidate.Response.DestinationLeaveTypeId,
                SourceLeaveBalanceId = candidate.SourceBalance.Id,
                CarryForwardNumber = GenerateDetailNumber(),
                CalculationDate = run.ExecutionDate,
                CarryForwardExpiryDate = candidate.Response.CarryForwardExpiryDate,
                SourceAvailableDays = candidate.Response.SourceAvailableDays,
                EligibleDays = candidate.Response.EligibleDays,
                CarryForwardDays = candidate.Response.CarryForwardDays,
                ExpiredDays = candidate.Response.ExpiredDays,
                ExcessDays = candidate.Response.ExcessDays,
                PayoutDays = candidate.Response.PayoutDays,
                RoundingAdjustmentDays = candidate.Response.RoundingAdjustmentDays,
                CarryForwardStatus = LeaveValueConstants.CarryForwardStatus.Calculated,
                IdempotencyKey = key,
                CalculatedAt = DateTime.UtcNow,
                CalculatedByUserId = actorUserId == Guid.Empty ? null : actorUserId,
                SourceBalanceSnapshotJson = JsonSerializer.Serialize(new
                {
                    candidate.SourceBalance.Id,
                    candidate.SourceBalance.AvailableDays,
                    candidate.SourceBalance.RemainingDays,
                    candidate.SourceBalance.BalanceVersion
                }, JsonOptions),
                CalculationDetailJson = candidate.Response.CalculationDetailJson,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            });
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private async Task SaveSkippedCandidateAsync(
            TrxLeaveCarryForwardRun run,
            LeaveCarryForwardCandidateWorkItem candidate,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            if (candidate.Policy.Id == Guid.Empty)
            {
                return;
            }

            var key = BuildItemIdempotencyKey(run.Id, candidate.Response);
            var existing = await _dbContext.Set<TrxLeaveCarryForward>()
                .FirstOrDefaultAsync(x => x.IdempotencyKey == key && !x.IsDelete, cancellationToken);
            if (existing != null)
            {
                return;
            }

            _dbContext.Add(new TrxLeaveCarryForward
            {
                Id = Guid.NewGuid(),
                LeaveCarryForwardRunId = run.Id,
                LeaveCarryForwardPolicyId = candidate.Policy.Id,
                SourceLeaveEntitlementPeriodId = candidate.SourcePeriod.Id,
                DestinationLeaveEntitlementPeriodId = candidate.DestinationPeriod.Id,
                WorkforceProfileId = candidate.SourceBalance.WorkforceProfileId,
                SourceLeaveTypeId = candidate.SourceBalance.LeaveTypeId,
                DestinationLeaveTypeId = candidate.Response.DestinationLeaveTypeId == Guid.Empty
                    ? candidate.SourceBalance.LeaveTypeId
                    : candidate.Response.DestinationLeaveTypeId,
                SourceLeaveBalanceId = candidate.SourceBalance.Id,
                CarryForwardNumber = GenerateDetailNumber(),
                CalculationDate = run.ExecutionDate,
                SourceAvailableDays = candidate.Response.SourceAvailableDays,
                EligibleDays = candidate.Response.EligibleDays,
                CarryForwardDays = candidate.Response.CarryForwardDays,
                ExpiredDays = candidate.Response.ExpiredDays,
                ExcessDays = candidate.Response.ExcessDays,
                PayoutDays = candidate.Response.PayoutDays,
                RoundingAdjustmentDays = candidate.Response.RoundingAdjustmentDays,
                CarryForwardExpiryDate = candidate.Response.CarryForwardExpiryDate,
                CarryForwardStatus = LeaveValueConstants.CarryForwardStatus.Skipped,
                SkipReasonCode = candidate.Response.ResultCode,
                SkipReason = candidate.Response.ResultMessage,
                IdempotencyKey = key,
                CalculatedAt = DateTime.UtcNow,
                CalculatedByUserId = actorUserId == Guid.Empty ? null : actorUserId,
                SourceBalanceSnapshotJson = JsonSerializer.Serialize(new
                {
                    candidate.SourceBalance.Id,
                    candidate.SourceBalance.AvailableDays,
                    candidate.SourceBalance.RemainingDays,
                    candidate.SourceBalance.BalanceVersion
                }, JsonOptions),
                CalculationDetailJson = candidate.Response.CalculationDetailJson,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            });
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private async Task<List<LeaveCarryForwardCandidateWorkItem>> BuildCandidatesAsync(
            LeaveCarryForwardPreviewRequest request,
            TrxLeaveEntitlementPeriod sourcePeriod,
            TrxLeaveEntitlementPeriod destinationPeriod,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.Set<WfpLeaveBalance>()
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.LeaveType)
                .Include(x => x.LeavePolicy)
                .Where(x =>
                    x.LeaveEntitlementPeriodId == sourcePeriod.Id &&
                    x.IsActive &&
                    !x.IsDelete);

            if (request.LeaveTypeId.HasValue)
            {
                query = query.Where(x => x.LeaveTypeId == request.LeaveTypeId.Value);
            }
            if (request.WorkforceProfileId.HasValue)
            {
                query = query.Where(x => x.WorkforceProfileId == request.WorkforceProfileId.Value);
            }
            if (request.LegalEntityId.HasValue)
            {
                query = query.Where(x => x.LeavePolicy != null && x.LeavePolicy.LegalEntityId == request.LegalEntityId.Value);
            }
            if (request.HospitalSiteId.HasValue)
            {
                query = query.Where(x => x.LeavePolicy != null && x.LeavePolicy.HospitalSiteId == request.HospitalSiteId.Value);
            }
            if (request.OrganizationUnitId.HasValue)
            {
                query = query.Where(x => x.LeavePolicy != null && x.LeavePolicy.OrganizationUnitId == request.OrganizationUnitId.Value);
            }
            if (request.DepartmentId.HasValue)
            {
                query = query.Where(x =>
                    (x.LeavePolicy != null && x.LeavePolicy.DepartmentId == request.DepartmentId.Value) ||
                    (x.WorkforceProfile != null && x.WorkforceProfile.PrimaryDepartmentId == request.DepartmentId.Value));
            }

            var balances = await query
                .OrderBy(x => x.WorkforceProfile!.DisplayName)
                .ThenBy(x => x.LeaveType!.LeaveTypeName)
                .Take(request.MaximumPreviewItem)
                .ToListAsync(cancellationToken);

            var result = new List<LeaveCarryForwardCandidateWorkItem>();
            foreach (var balance in balances)
            {
                result.Add(await _policyResolver.CalculateAsync(
                    balance,
                    sourcePeriod,
                    destinationPeriod,
                    request.LeaveCarryForwardPolicyId,
                    request.ExecutionDate,
                    request.ForceReprocess,
                    cancellationToken));
            }
            return result;
        }

        private async Task<(bool Success, int StatusCode, string Message, TrxLeaveEntitlementPeriod? SourcePeriod, TrxLeaveEntitlementPeriod? DestinationPeriod)> ValidatePeriodsAsync(
            Guid sourceId,
            Guid destinationId,
            DateOnly executionDate,
            CancellationToken cancellationToken)
        {
            if (sourceId == Guid.Empty || destinationId == Guid.Empty || sourceId == destinationId)
            {
                return (false, StatusCodes.Status400BadRequest, "Source dan destination entitlement period tidak valid.", null, null);
            }

            var periods = await _dbContext.Set<TrxLeaveEntitlementPeriod>()
                .AsNoTracking()
                .Where(x => (x.Id == sourceId || x.Id == destinationId) && x.IsActive && !x.IsDelete)
                .ToListAsync(cancellationToken);
            var source = periods.FirstOrDefault(x => x.Id == sourceId);
            var destination = periods.FirstOrDefault(x => x.Id == destinationId);
            if (source == null || destination == null)
            {
                return (false, StatusCodes.Status404NotFound, "Source atau destination entitlement period tidak ditemukan.", source, destination);
            }
            if (destination.StartDate <= source.StartDate || destination.EndDate <= source.EndDate)
            {
                return (false, StatusCodes.Status400BadRequest, "Destination period harus berada setelah source period.", source, destination);
            }
            if (source.IsLocked || source.PeriodStatus == LeaveValueConstants.PeriodStatus.Closed || source.PeriodStatus == LeaveValueConstants.PeriodStatus.Cancelled)
            {
                return (false, StatusCodes.Status409Conflict, "Source period harus dibuka atau diproses sebelum carry forward dijalankan.", source, destination);
            }
            if (destination.IsLocked || destination.PeriodStatus == LeaveValueConstants.PeriodStatus.Closed || destination.PeriodStatus == LeaveValueConstants.PeriodStatus.Cancelled)
            {
                return (false, StatusCodes.Status409Conflict, "Destination period sedang dikunci, ditutup, atau dibatalkan.", source, destination);
            }
            if (executionDate < source.StartDate)
            {
                return (false, StatusCodes.Status400BadRequest, "Execution date tidak boleh sebelum source period dimulai.", source, destination);
            }
            return (true, StatusCodes.Status200OK, string.Empty, source, destination);
        }

        private async Task<WfpLeaveBalance> GetOrCreateDestinationBalanceAsync(
            LeaveCarryForwardCandidateWorkItem candidate,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var existing = await _dbContext.Set<WfpLeaveBalance>()
                .FirstOrDefaultAsync(x =>
                    x.WorkforceProfileId == candidate.SourceBalance.WorkforceProfileId &&
                    x.LeaveTypeId == candidate.Response.DestinationLeaveTypeId &&
                    x.LeaveEntitlementPeriodId == candidate.DestinationPeriod.Id &&
                    !x.IsDelete,
                    cancellationToken);
            if (existing != null)
            {
                return existing;
            }

            var balance = new WfpLeaveBalance
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = candidate.SourceBalance.WorkforceProfileId,
                LeaveTypeId = candidate.Response.DestinationLeaveTypeId,
                LeavePolicyId = candidate.SourceBalance.LeavePolicyId,
                LeaveEntitlementPolicyId = candidate.SourceBalance.LeaveEntitlementPolicyId,
                LeaveEntitlementPeriodId = candidate.DestinationPeriod.Id,
                Year = candidate.DestinationPeriod.PeriodYear,
                PeriodStartDate = candidate.DestinationPeriod.StartDate,
                PeriodEndDate = candidate.DestinationPeriod.EndDate,
                BalanceStatus = LeaveValueConstants.BalanceStatus.Active,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _dbContext.Add(balance);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return balance;
        }

        private async Task<WfpLeaveBalance?> LockBalanceAsync(Guid balanceId, CancellationToken cancellationToken)
        {
            return await _dbContext.Set<WfpLeaveBalance>()
                .FromSqlInterpolated($@"
                    SELECT *
                    FROM public.""WfpLeaveBalance""
                    WHERE ""Id"" = {balanceId}
                      AND ""IsDelete"" = false
                    FOR UPDATE")
                .FirstOrDefaultAsync(cancellationToken);
        }

        private TrxLeaveBalanceTransaction CreateLedger(
            WfpLeaveBalance balance,
            Guid runId,
            TrxLeaveCarryForward detail,
            string transactionType,
            string direction,
            decimal transactionDays,
            Guid actorUserId,
            string remarks,
            string idempotencyKey,
            decimal carryForwardDelta = 0,
            decimal expiredDelta = 0,
            decimal encashmentDelta = 0,
            decimal availableDelta = 0,
            string sourceType = "CarryForward")
        {
            return new TrxLeaveBalanceTransaction
            {
                Id = Guid.NewGuid(),
                TransactionNumber = GenerateTransactionNumber(),
                LeaveBalanceId = balance.Id,
                WorkforceProfileId = balance.WorkforceProfileId,
                LeaveTypeId = balance.LeaveTypeId,
                LeaveEntitlementPeriodId = balance.LeaveEntitlementPeriodId,
                LeaveCarryForwardId = detail.Id,
                TransactionDateTime = DateTime.UtcNow,
                EffectiveDate = detail.CalculationDate,
                TransactionSequence = balance.LastTransactionSequence + 1,
                TransactionType = transactionType,
                Direction = direction,
                TransactionDays = transactionDays,
                CarryForwardDelta = carryForwardDelta,
                ExpiredDelta = expiredDelta,
                EncashmentDelta = encashmentDelta,
                AvailableDelta = availableDelta,
                PreviousOpeningBalanceDays = balance.OpeningBalanceDays,
                PreviousAvailableDays = balance.AvailableDays,
                PreviousReservedDays = balance.ReservedDays,
                NewAvailableDays = balance.AvailableDays + availableDelta,
                NewReservedDays = balance.ReservedDays,
                NewUsedDays = balance.UsedDays,
                IdempotencyKey = idempotencyKey,
                PostingBatchType = LeaveValueConstants.PostingBatchType.CarryForwardRun,
                PostingBatchId = runId,
                SourceType = sourceType,
                SourceReferenceId = detail.Id,
                SourceReferenceNumber = detail.CarryForwardNumber,
                TransactionStatus = LeaveValueConstants.TransactionStatus.Posted,
                PostedAt = DateTime.UtcNow,
                PostedByUserId = actorUserId == Guid.Empty ? null : actorUserId,
                Remarks = remarks,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
        }

        private static void ApplyLedgerToBalance(WfpLeaveBalance balance, TrxLeaveBalanceTransaction ledger)
        {
            balance.CarriedForwardDays += ledger.CarryForwardDelta;
            balance.ExpiredDays += ledger.ExpiredDelta;
            balance.EncashmentDays += ledger.EncashmentDelta;
            balance.RemainingDays = CalculateRemaining(balance);
            balance.AvailableDays = balance.RemainingDays - balance.ReservedDays;
            balance.LastTransactionId = ledger.Id;
            balance.LastTransactionSequence = ledger.TransactionSequence;
            balance.BalanceVersion += 1;
            balance.LastCalculatedAt = DateTime.UtcNow;
            balance.UpdateDateTime = DateTime.UtcNow;
            balance.UpdateBy = ledger.PostedByUserId ?? Guid.Empty;
            ledger.NewAvailableDays = balance.AvailableDays;
            ledger.NewReservedDays = balance.ReservedDays;
            ledger.NewUsedDays = balance.UsedDays;
        }

        private static decimal CalculateRemaining(WfpLeaveBalance balance)
        {
            return balance.OpeningBalanceDays +
                   balance.EntitlementDays +
                   balance.AccruedDays +
                   balance.CarriedForwardDays +
                   balance.AdjustmentDays +
                   balance.CompensatoryDays +
                   balance.RecalledDays -
                   balance.UsedDays -
                   balance.ExpiredDays -
                   balance.EncashmentDays;
        }

        private IQueryable<TrxLeaveCarryForwardRun> BuildRunQuery(LeaveCarryForwardRunQueryRequest request)
        {
            var query = _dbContext.Set<TrxLeaveCarryForwardRun>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
            if (request.StartDate.HasValue) query = query.Where(x => x.ExecutionDate >= request.StartDate.Value);
            if (request.EndDate.HasValue) query = query.Where(x => x.ExecutionDate <= request.EndDate.Value);
            if (request.SourceLeaveEntitlementPeriodId.HasValue) query = query.Where(x => x.SourceLeaveEntitlementPeriodId == request.SourceLeaveEntitlementPeriodId.Value);
            if (request.DestinationLeaveEntitlementPeriodId.HasValue) query = query.Where(x => x.DestinationLeaveEntitlementPeriodId == request.DestinationLeaveEntitlementPeriodId.Value);
            if (request.LeaveTypeId.HasValue) query = query.Where(x => x.LeaveTypeId == request.LeaveTypeId.Value);
            if (request.LeaveCarryForwardPolicyId.HasValue) query = query.Where(x => x.LeaveCarryForwardPolicyId == request.LeaveCarryForwardPolicyId.Value);
            if (request.LegalEntityId.HasValue) query = query.Where(x => x.LegalEntityId == request.LegalEntityId.Value);
            if (request.HospitalSiteId.HasValue) query = query.Where(x => x.HospitalSiteId == request.HospitalSiteId.Value);
            if (request.OrganizationUnitId.HasValue) query = query.Where(x => x.OrganizationUnitId == request.OrganizationUnitId.Value);
            if (request.DepartmentId.HasValue) query = query.Where(x => x.DepartmentId == request.DepartmentId.Value);
            if (!string.IsNullOrWhiteSpace(request.RunStatus)) query = query.Where(x => x.RunStatus == request.RunStatus);
            if (!string.IsNullOrWhiteSpace(request.RunMode)) query = query.Where(x => x.RunMode == request.RunMode);
            if (request.IsDryRun.HasValue) query = query.Where(x => x.IsDryRun == request.IsDryRun.Value);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToLower();
                query = query.Where(x =>
                    x.RunNumber.ToLower().Contains(search) ||
                    (x.SourceLeaveEntitlementPeriod != null && x.SourceLeaveEntitlementPeriod.PeriodCode.ToLower().Contains(search)) ||
                    (x.DestinationLeaveEntitlementPeriod != null && x.DestinationLeaveEntitlementPeriod.PeriodCode.ToLower().Contains(search)) ||
                    (x.LeaveType != null && x.LeaveType.LeaveTypeName.ToLower().Contains(search)));
            }
            return query;
        }

        private static IQueryable<TrxLeaveCarryForwardRun> ApplyRunSort(
            IQueryable<TrxLeaveCarryForwardRun> query,
            string? sortBy,
            string? sortDirection)
        {
            var desc = !string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "executiondate" => desc ? query.OrderByDescending(x => x.ExecutionDate) : query.OrderBy(x => x.ExecutionDate),
                "runnumber" => desc ? query.OrderByDescending(x => x.RunNumber) : query.OrderBy(x => x.RunNumber),
                "runstatus" => desc ? query.OrderByDescending(x => x.RunStatus) : query.OrderBy(x => x.RunStatus),
                "totalcarryforwarddays" => desc ? query.OrderByDescending(x => x.TotalCarryForwardDays) : query.OrderBy(x => x.TotalCarryForwardDays),
                _ => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime)
            };
        }

        private static LeaveCarryForwardRunResponse MapRun(TrxLeaveCarryForwardRun x)
        {
            return new LeaveCarryForwardRunResponse
            {
                Id = x.Id,
                RunNumber = x.RunNumber,
                RunMode = x.RunMode,
                RunStatus = x.RunStatus,
                SourceLeaveEntitlementPeriodId = x.SourceLeaveEntitlementPeriodId,
                SourcePeriodCode = x.SourceLeaveEntitlementPeriod != null ? x.SourceLeaveEntitlementPeriod.PeriodCode : null,
                SourcePeriodName = x.SourceLeaveEntitlementPeriod != null ? x.SourceLeaveEntitlementPeriod.PeriodName : null,
                DestinationLeaveEntitlementPeriodId = x.DestinationLeaveEntitlementPeriodId,
                DestinationPeriodCode = x.DestinationLeaveEntitlementPeriod != null ? x.DestinationLeaveEntitlementPeriod.PeriodCode : null,
                DestinationPeriodName = x.DestinationLeaveEntitlementPeriod != null ? x.DestinationLeaveEntitlementPeriod.PeriodName : null,
                LeaveTypeId = x.LeaveTypeId,
                LeaveTypeCode = x.LeaveType != null ? x.LeaveType.LeaveTypeCode : null,
                LeaveTypeName = x.LeaveType != null ? x.LeaveType.LeaveTypeName : null,
                LeaveCarryForwardPolicyId = x.LeaveCarryForwardPolicyId,
                CarryForwardPolicyCode = x.LeaveCarryForwardPolicy != null ? x.LeaveCarryForwardPolicy.CarryForwardPolicyCode : null,
                CarryForwardPolicyName = x.LeaveCarryForwardPolicy != null ? x.LeaveCarryForwardPolicy.CarryForwardPolicyName : null,
                ExecutionDate = x.ExecutionDate,
                IsDryRun = x.IsDryRun,
                ForceReprocess = x.ForceReprocess,
                RetryCount = x.RetryCount,
                MaximumRetryCount = x.MaximumRetryCount,
                TargetCount = x.TargetCount,
                CalculatedCount = x.CalculatedCount,
                PostedCount = x.PostedCount,
                SkippedCount = x.SkippedCount,
                FailedCount = x.FailedCount,
                TotalSourceAvailableDays = x.TotalSourceAvailableDays,
                TotalEligibleDays = x.TotalEligibleDays,
                TotalCarryForwardDays = x.TotalCarryForwardDays,
                TotalExpiredDays = x.TotalExpiredDays,
                TotalExcessDays = x.TotalExcessDays,
                TotalPayoutDays = x.TotalPayoutDays,
                StartedAt = x.StartedAt,
                CompletedAt = x.CompletedAt,
                CorrelationId = x.CorrelationId,
                ErrorSummary = x.ErrorSummary,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static LeaveCarryForwardRunActionResponse MapAction(TrxLeaveCarryForwardRun run, bool idempotent, string message)
        {
            return new LeaveCarryForwardRunActionResponse
            {
                Id = run.Id,
                RunNumber = run.RunNumber,
                RunStatus = run.RunStatus,
                TargetCount = run.TargetCount,
                PostedCount = run.PostedCount,
                SkippedCount = run.SkippedCount,
                FailedCount = run.FailedCount,
                TotalCarryForwardDays = run.TotalCarryForwardDays,
                TotalExpiredDays = run.TotalExpiredDays,
                TotalPayoutDays = run.TotalPayoutDays,
                IsIdempotent = idempotent,
                Message = message
            };
        }

        private static LeaveCarryForwardRunParameters DeserializeParameters(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new LeaveCarryForwardRunParameters();
            try
            {
                return JsonSerializer.Deserialize<LeaveCarryForwardRunParameters>(json, JsonOptions) ?? new LeaveCarryForwardRunParameters();
            }
            catch
            {
                return new LeaveCarryForwardRunParameters();
            }
        }

        private static bool IsValidRunMode(string? value)
        {
            return string.Equals(value, LeaveValueConstants.BatchRunMode.Manual, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, LeaveValueConstants.BatchRunMode.Scheduled, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, LeaveValueConstants.BatchRunMode.Reprocess, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, LeaveValueConstants.BatchRunMode.Preview, StringComparison.OrdinalIgnoreCase);
        }

        private static LeaveCarryForwardOptionResponse Option(string value)
        {
            return new LeaveCarryForwardOptionResponse { Value = value, Label = value };
        }

        private static LeaveCarryForwardReconciliationIssueResponse Issue(
            string code,
            string severity,
            string message,
            Guid? detailId = null)
        {
            return new LeaveCarryForwardReconciliationIssueResponse
            {
                Code = code,
                Severity = severity,
                Message = message,
                LeaveCarryForwardId = detailId
            };
        }

        private static string BuildItemIdempotencyKey(Guid runId, LeaveCarryForwardCandidateResponse response)
        {
            return $"CF:{runId:N}:{response.WorkforceProfileId:N}:{response.SourceLeaveTypeId:N}";
        }

        private static string GenerateRunNumber()
        {
            return $"LCF-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
        }

        private static string GenerateDetailNumber()
        {
            return $"CF-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
        }

        private static string GenerateTransactionNumber()
        {
            return $"LBT-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
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
