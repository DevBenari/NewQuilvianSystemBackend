using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Models;
using System.Data;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    internal class LeaveAccrualRunParameters
    {
        public Guid? WorkforceProfileId { get; set; }
        public int MaximumPreviewItem { get; set; } = 250;
    }

    internal class LeaveAccrualCandidateWorkItem
    {
        public LeaveAccrualCandidateResponse Response { get; set; } = new();
        public MstEmployee Employee { get; set; } = null!;
        public TrxLeaveEntitlement Entitlement { get; set; } = null!;
        public WfpLeaveBalance Balance { get; set; } = null!;
        public MstLeavePolicy LeavePolicy { get; set; } = null!;
        public MstLeaveEntitlementPolicy EntitlementPolicy { get; set; } = null!;
        public DateOnly ScheduledAccrualDate { get; set; }
        public DateOnly AccrualPeriodStartDate { get; set; }
        public DateOnly AccrualPeriodEndDate { get; set; }
    }

    public class LeaveAccrualProcessorService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LeaveAccrualPolicyResolverService _policyResolver;
        private readonly ILogger<LeaveAccrualProcessorService> _logger;

        public LeaveAccrualProcessorService(
            ApplicationDbContext dbContext,
            LeaveAccrualPolicyResolverService policyResolver,
            ILogger<LeaveAccrualProcessorService> logger)
        {
            _dbContext = dbContext;
            _policyResolver = policyResolver;
            _logger = logger;
        }

        public LeaveAccrualRunFilterMetadataResponse GetMetadata()
        {
            return new LeaveAccrualRunFilterMetadataResponse
            {
                DefaultFilter = new LeaveAccrualRunDefaultFilterResponse
                {
                    StartDate = new DateOnly(DateTime.UtcNow.Year, 1, 1),
                    EndDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                    PageNumber = 1,
                    PageSize = 25,
                    SortBy = "createDateTime",
                    SortDirection = "desc"
                },
                RunStatuses = new List<LeaveAccrualOptionResponse>
                {
                    Option(LeaveValueConstants.BatchRunStatus.Draft),
                    Option(LeaveValueConstants.BatchRunStatus.Queued),
                    Option(LeaveValueConstants.BatchRunStatus.Running),
                    Option(LeaveValueConstants.BatchRunStatus.Completed),
                    Option(LeaveValueConstants.BatchRunStatus.CompletedWithErrors),
                    Option(LeaveValueConstants.BatchRunStatus.Failed),
                    Option(LeaveValueConstants.BatchRunStatus.Cancelled)
                },
                RunModes = new List<LeaveAccrualOptionResponse>
                {
                    Option(LeaveValueConstants.BatchRunMode.Scheduled),
                    Option(LeaveValueConstants.BatchRunMode.Manual),
                    Option(LeaveValueConstants.BatchRunMode.Reprocess),
                    Option(LeaveValueConstants.BatchRunMode.Preview)
                },
                SortOptions = new List<LeaveAccrualOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal Dibuat" },
                    new() { Value = "scheduledAccrualDate", Label = "Tanggal Accrual" },
                    new() { Value = "runNumber", Label = "Nomor Run" },
                    new() { Value = "runStatus", Label = "Status" },
                    new() { Value = "totalPostedDays", Label = "Total Hari Diposting" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100, 200 }
            };
        }

        public async Task<LeaveAccrualRunSummaryResponse> GetSummaryAsync(
            LeaveAccrualRunQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var query = BuildRunQuery(request);

            return new LeaveAccrualRunSummaryResponse
            {
                TotalRun = await query.CountAsync(cancellationToken),
                DraftRun = await query.CountAsync(x => x.RunStatus == LeaveValueConstants.BatchRunStatus.Draft, cancellationToken),
                QueuedRun = await query.CountAsync(x => x.RunStatus == LeaveValueConstants.BatchRunStatus.Queued, cancellationToken),
                RunningRun = await query.CountAsync(x => x.RunStatus == LeaveValueConstants.BatchRunStatus.Running, cancellationToken),
                CompletedRun = await query.CountAsync(x => x.RunStatus == LeaveValueConstants.BatchRunStatus.Completed, cancellationToken),
                CompletedWithErrorsRun = await query.CountAsync(x => x.RunStatus == LeaveValueConstants.BatchRunStatus.CompletedWithErrors, cancellationToken),
                FailedRun = await query.CountAsync(x => x.RunStatus == LeaveValueConstants.BatchRunStatus.Failed, cancellationToken),
                CancelledRun = await query.CountAsync(x => x.RunStatus == LeaveValueConstants.BatchRunStatus.Cancelled, cancellationToken),
                TotalTarget = await query.SumAsync(x => (int?)x.TargetCount, cancellationToken) ?? 0,
                TotalPosted = await query.SumAsync(x => (int?)x.PostedCount, cancellationToken) ?? 0,
                TotalSkipped = await query.SumAsync(x => (int?)x.SkippedCount, cancellationToken) ?? 0,
                TotalFailed = await query.SumAsync(x => (int?)x.FailedCount, cancellationToken) ?? 0,
                TotalCalculatedDays = await query.SumAsync(x => (decimal?)x.TotalCalculatedDays, cancellationToken) ?? 0,
                TotalPostedDays = await query.SumAsync(x => (decimal?)x.TotalPostedDays, cancellationToken) ?? 0
            };
        }

        public async Task<LeaveAccrualRunPagedResponse> GetPagedAsync(
            LeaveAccrualRunQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            request.PageNumber = Math.Max(1, request.PageNumber);
            request.PageSize = Math.Clamp(request.PageSize, 1, 200);

            var query = BuildRunQuery(request);
            var totalData = await query.CountAsync(cancellationToken);
            query = ApplyRunSort(query, request.SortBy, request.SortDirection);

            var rows = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new LeaveAccrualRunResponse
                {
                    Id = x.Id,
                    RunNumber = x.RunNumber,
                    RunMode = x.RunMode,
                    RunStatus = x.RunStatus,
                    LeaveEntitlementPeriodId = x.LeaveEntitlementPeriodId,
                    LeaveEntitlementPeriodCode = x.LeaveEntitlementPeriod != null ? x.LeaveEntitlementPeriod.PeriodCode : null,
                    LeaveEntitlementPeriodName = x.LeaveEntitlementPeriod != null ? x.LeaveEntitlementPeriod.PeriodName : null,
                    LeaveTypeId = x.LeaveTypeId,
                    LeaveTypeCode = x.LeaveType != null ? x.LeaveType.LeaveTypeCode : null,
                    LeaveTypeName = x.LeaveType != null ? x.LeaveType.LeaveTypeName : null,
                    LeaveEntitlementPolicyId = x.LeaveEntitlementPolicyId,
                    LeaveEntitlementPolicyCode = x.LeaveEntitlementPolicy != null ? x.LeaveEntitlementPolicy.EntitlementPolicyCode : null,
                    LeaveEntitlementPolicyName = x.LeaveEntitlementPolicy != null ? x.LeaveEntitlementPolicy.EntitlementPolicyName : null,
                    ScheduledAccrualDate = x.ScheduledAccrualDate,
                    AccrualPeriodStartDate = x.AccrualPeriodStartDate,
                    AccrualPeriodEndDate = x.AccrualPeriodEndDate,
                    IsDryRun = x.IsDryRun,
                    ForceReprocess = x.ForceReprocess,
                    RetryCount = x.RetryCount,
                    MaximumRetryCount = x.MaximumRetryCount,
                    TargetCount = x.TargetCount,
                    CalculatedCount = x.CalculatedCount,
                    PostedCount = x.PostedCount,
                    SkippedCount = x.SkippedCount,
                    FailedCount = x.FailedCount,
                    TotalCalculatedDays = x.TotalCalculatedDays,
                    TotalPostedDays = x.TotalPostedDays,
                    StartedAt = x.StartedAt,
                    CompletedAt = x.CompletedAt,
                    ErrorSummary = x.ErrorSummary,
                    CorrelationId = x.CorrelationId,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            return new LeaveAccrualRunPagedResponse
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)request.PageSize),
                Items = rows
            };
        }

        public async Task<LeaveAccrualServiceResult<LeaveAccrualRunDetailResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var run = await _dbContext.Set<TrxLeaveAccrualRun>()
                .AsNoTracking()
                .Include(x => x.LeaveEntitlementPeriod)
                .Include(x => x.LeaveType)
                .Include(x => x.LeaveEntitlementPolicy)
                .Include(x => x.TriggeredByUser)
                .Include(x => x.CancelledByUser)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (run == null)
            {
                return LeaveAccrualServiceResult<LeaveAccrualRunDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Leave accrual run tidak ditemukan.");
            }

            var accruals = await _dbContext.Set<TrxLeaveAccrual>()
                .AsNoTracking()
                .Where(x => x.LeaveAccrualRunId == id && !x.IsDelete)
                .OrderByDescending(x => x.CreateDateTime)
                .Take(250)
                .Select(x => new LeaveAccrualItemResponse
                {
                    Id = x.Id,
                    AccrualNumber = x.AccrualNumber,
                    WorkforceProfileId = x.WorkforceProfileId,
                    WorkforceProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : null,
                    WorkforceDisplayName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : null,
                    LeaveTypeId = x.LeaveTypeId,
                    LeaveTypeName = x.LeaveType != null ? x.LeaveType.LeaveTypeName : null,
                    LeaveBalanceId = x.LeaveBalanceId,
                    LeaveEntitlementId = x.LeaveEntitlementId,
                    AccrualSequence = x.AccrualSequence,
                    AccrualDate = x.AccrualDate,
                    AccrualPeriodStartDate = x.AccrualPeriodStartDate,
                    AccrualPeriodEndDate = x.AccrualPeriodEndDate,
                    AccrualAmountDays = x.AccrualAmountDays,
                    BalanceBeforeAccrual = x.BalanceBeforeAccrual,
                    BalanceAfterAccrual = x.BalanceAfterAccrual,
                    IsProrated = x.IsProrated,
                    AccrualStatus = x.AccrualStatus,
                    Notes = x.Notes,
                    PostedAt = x.PostedAt
                })
                .ToListAsync(cancellationToken);

            return LeaveAccrualServiceResult<LeaveAccrualRunDetailResponse>.Ok(
                new LeaveAccrualRunDetailResponse
                {
                    Id = run.Id,
                    RunNumber = run.RunNumber,
                    RunMode = run.RunMode,
                    RunStatus = run.RunStatus,
                    LeaveEntitlementPeriodId = run.LeaveEntitlementPeriodId,
                    LeaveEntitlementPeriodCode = run.LeaveEntitlementPeriod?.PeriodCode,
                    LeaveEntitlementPeriodName = run.LeaveEntitlementPeriod?.PeriodName,
                    LeaveTypeId = run.LeaveTypeId,
                    LeaveTypeCode = run.LeaveType?.LeaveTypeCode,
                    LeaveTypeName = run.LeaveType?.LeaveTypeName,
                    LeaveEntitlementPolicyId = run.LeaveEntitlementPolicyId,
                    LeaveEntitlementPolicyCode = run.LeaveEntitlementPolicy?.EntitlementPolicyCode,
                    LeaveEntitlementPolicyName = run.LeaveEntitlementPolicy?.EntitlementPolicyName,
                    ScheduledAccrualDate = run.ScheduledAccrualDate,
                    AccrualPeriodStartDate = run.AccrualPeriodStartDate,
                    AccrualPeriodEndDate = run.AccrualPeriodEndDate,
                    IsDryRun = run.IsDryRun,
                    ForceReprocess = run.ForceReprocess,
                    RetryCount = run.RetryCount,
                    MaximumRetryCount = run.MaximumRetryCount,
                    TargetCount = run.TargetCount,
                    CalculatedCount = run.CalculatedCount,
                    PostedCount = run.PostedCount,
                    SkippedCount = run.SkippedCount,
                    FailedCount = run.FailedCount,
                    TotalCalculatedDays = run.TotalCalculatedDays,
                    TotalPostedDays = run.TotalPostedDays,
                    StartedAt = run.StartedAt,
                    CompletedAt = run.CompletedAt,
                    CancelledAt = run.CancelledAt,
                    ErrorSummary = run.ErrorSummary,
                    CorrelationId = run.CorrelationId,
                    CreateDateTime = run.CreateDateTime,
                    LegalEntityId = run.LegalEntityId,
                    HospitalSiteId = run.HospitalSiteId,
                    OrganizationUnitId = run.OrganizationUnitId,
                    DepartmentId = run.DepartmentId,
                    TriggeredByUserId = run.TriggeredByUserId,
                    TriggeredByName = GetUserDisplayName(run.TriggeredByUser),
                    CancelledByUserId = run.CancelledByUserId,
                    CancelledByName = GetUserDisplayName(run.CancelledByUser),
                    Notes = run.Notes,
                    ParametersJson = run.ParametersJson,
                    ResultSummaryJson = run.ResultSummaryJson,
                    Accruals = accruals
                },
                "Detail leave accrual run berhasil diambil.");
        }

        public async Task<LeaveAccrualServiceResult<LeaveAccrualPreviewResponse>> PreviewAsync(
            LeaveAccrualPreviewRequest request,
            CancellationToken cancellationToken = default)
        {
            var validation = await ValidateRequestAsync(request, cancellationToken);
            if (!validation.Success)
            {
                return LeaveAccrualServiceResult<LeaveAccrualPreviewResponse>.Fail(
                    validation.StatusCode,
                    validation.Message);
            }

            var buildResult = await BuildCandidatesAsync(request, cancellationToken);
            if (!buildResult.Success || buildResult.Data == null)
            {
                return LeaveAccrualServiceResult<LeaveAccrualPreviewResponse>.Fail(
                    buildResult.StatusCode,
                    buildResult.Message);
            }

            var candidates = buildResult.Data;
            var period = validation.Period!;
            var maximum = Math.Clamp(request.MaximumPreviewItem, 1, 1000);

            return LeaveAccrualServiceResult<LeaveAccrualPreviewResponse>.Ok(
                new LeaveAccrualPreviewResponse
                {
                    LeaveEntitlementPeriodId = period.Id,
                    PeriodCode = period.PeriodCode,
                    ScheduledAccrualDate = request.ScheduledAccrualDate,
                    AccrualPeriodStartDate = request.AccrualPeriodStartDate,
                    AccrualPeriodEndDate = request.AccrualPeriodEndDate,
                    TotalCandidate = candidates.Count,
                    EligibleCount = candidates.Count(x => x.Response.IsEligible),
                    SkippedCount = candidates.Count(x => !x.Response.IsEligible),
                    TotalCalculatedDays = candidates.Where(x => x.Response.IsEligible).Sum(x => x.Response.CalculatedAccrualDays),
                    IsTruncated = candidates.Count > maximum,
                    Items = candidates.Take(maximum).Select(x => x.Response).ToList()
                },
                "Preview leave accrual berhasil dihitung.");
        }

        public async Task<LeaveAccrualServiceResult<LeaveAccrualRunActionResponse>> CreateRunAsync(
            CreateLeaveAccrualRunRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var validation = await ValidateRequestAsync(request, cancellationToken);
            if (!validation.Success)
            {
                return LeaveAccrualServiceResult<LeaveAccrualRunActionResponse>.Fail(
                    validation.StatusCode,
                    validation.Message);
            }

            var normalizedIdempotencyKey = NormalizeOptional(request.IdempotencyKey);
            if (normalizedIdempotencyKey != null)
            {
                var existing = await _dbContext.Set<TrxLeaveAccrualRun>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.IdempotencyKey == normalizedIdempotencyKey &&
                        !x.IsDelete,
                        cancellationToken);

                if (existing != null)
                {
                    return LeaveAccrualServiceResult<LeaveAccrualRunActionResponse>.Ok(
                        MapAction(existing, true, "Leave accrual run dengan idempotency key yang sama sudah tersedia."),
                        "Leave accrual run sudah tersedia.");
                }
            }

            var period = validation.Period!;
            var resolvedLeaveTypeId = validation.LeaveTypeId;
            var runMode = NormalizeRunMode(request.IsDryRun
                ? LeaveValueConstants.BatchRunMode.Preview
                : request.RunMode);

            var run = new TrxLeaveAccrualRun
            {
                Id = Guid.NewGuid(),
                LeaveEntitlementPeriodId = period.Id,
                LeaveTypeId = resolvedLeaveTypeId,
                LeaveEntitlementPolicyId = request.LeaveEntitlementPolicyId,
                LegalEntityId = request.LegalEntityId ?? period.LegalEntityId,
                HospitalSiteId = request.HospitalSiteId ?? period.HospitalSiteId,
                OrganizationUnitId = request.OrganizationUnitId ?? period.OrganizationUnitId,
                DepartmentId = request.DepartmentId ?? period.DepartmentId,
                RunNumber = GenerateNumber("LAR"),
                RunMode = runMode,
                RunStatus = request.QueueForProcessing
                    ? LeaveValueConstants.BatchRunStatus.Queued
                    : LeaveValueConstants.BatchRunStatus.Draft,
                ScheduledAccrualDate = request.ScheduledAccrualDate,
                AccrualPeriodStartDate = request.AccrualPeriodStartDate,
                AccrualPeriodEndDate = request.AccrualPeriodEndDate,
                IsDryRun = request.IsDryRun,
                ForceReprocess = request.ForceReprocess,
                MaximumRetryCount = Math.Clamp(request.MaximumRetryCount, 0, 10),
                IdempotencyKey = normalizedIdempotencyKey,
                CorrelationId = NormalizeOptional(request.CorrelationId),
                TriggeredByUserId = actorUserId == Guid.Empty ? null : actorUserId,
                Notes = NormalizeOptional(request.Notes),
                ParametersJson = JsonSerializer.Serialize(
                    new LeaveAccrualRunParameters
                    {
                        WorkforceProfileId = request.WorkforceProfileId,
                        MaximumPreviewItem = request.MaximumPreviewItem
                    },
                    JsonOptions),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId,
                UpdateBy = actorUserId,
                DeleteBy = Guid.Empty,
                CancelBy = Guid.Empty
            };

            _dbContext.Add(run);
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException) when (normalizedIdempotencyKey != null)
            {
                _dbContext.Entry(run).State = EntityState.Detached;
                var concurrentExisting = await _dbContext.Set<TrxLeaveAccrualRun>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.IdempotencyKey == normalizedIdempotencyKey &&
                        !x.IsDelete,
                        cancellationToken);

                if (concurrentExisting != null)
                {
                    return LeaveAccrualServiceResult<LeaveAccrualRunActionResponse>.Ok(
                        MapAction(concurrentExisting, true, "Leave accrual run dibuat oleh proses lain dengan idempotency key yang sama."),
                        "Leave accrual run sudah tersedia.");
                }

                throw;
            }

            return LeaveAccrualServiceResult<LeaveAccrualRunActionResponse>.Ok(
                MapAction(run, false, request.QueueForProcessing
                    ? "Leave accrual run berhasil dibuat dan masuk antrean."
                    : "Draft leave accrual run berhasil dibuat."),
                "Leave accrual run berhasil dibuat.",
                StatusCodes.Status201Created);
        }

        public async Task<LeaveAccrualServiceResult<LeaveAccrualRunActionResponse>> ExecuteRunAsync(
            Guid runId,
            Guid actorUserId,
            bool forceReprocess = false,
            string? notes = null,
            CancellationToken cancellationToken = default,
            bool allowRunningClaim = false)
        {
            var run = await _dbContext.Set<TrxLeaveAccrualRun>()
                .FirstOrDefaultAsync(x => x.Id == runId && !x.IsDelete, cancellationToken);

            if (run == null)
            {
                return LeaveAccrualServiceResult<LeaveAccrualRunActionResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Leave accrual run tidak ditemukan.");
            }

            if (run.RunStatus == LeaveValueConstants.BatchRunStatus.Completed &&
                !forceReprocess &&
                !run.ForceReprocess)
            {
                return LeaveAccrualServiceResult<LeaveAccrualRunActionResponse>.Ok(
                    MapAction(run, true, "Leave accrual run sudah selesai."),
                    "Leave accrual run sudah selesai.");
            }

            if (run.RunStatus == LeaveValueConstants.BatchRunStatus.Cancelled)
            {
                return LeaveAccrualServiceResult<LeaveAccrualRunActionResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Leave accrual run yang sudah dibatalkan tidak dapat dijalankan.");
            }

            if (run.RunStatus == LeaveValueConstants.BatchRunStatus.Running && !allowRunningClaim)
            {
                return LeaveAccrualServiceResult<LeaveAccrualRunActionResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Leave accrual run sedang diproses oleh worker lain.");
            }

            run.RunStatus = LeaveValueConstants.BatchRunStatus.Running;
            run.StartedAt = DateTime.UtcNow;
            run.CompletedAt = null;
            run.CancelledAt = null;
            run.ErrorSummary = null;
            run.TargetCount = 0;
            run.CalculatedCount = 0;
            run.PostedCount = 0;
            run.SkippedCount = 0;
            run.FailedCount = 0;
            run.TotalCalculatedDays = 0;
            run.TotalPostedDays = 0;
            run.ForceReprocess = run.ForceReprocess || forceReprocess;
            run.TriggeredByUserId ??= actorUserId == Guid.Empty ? null : actorUserId;
            run.Notes = string.IsNullOrWhiteSpace(notes) ? run.Notes : notes.Trim();
            run.UpdateDateTime = DateTime.UtcNow;
            run.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            try
            {
                var request = MapRunToPreviewRequest(run);
                var buildResult = await BuildCandidatesAsync(request, cancellationToken);
                if (!buildResult.Success || buildResult.Data == null)
                {
                    run.RunStatus = LeaveValueConstants.BatchRunStatus.Failed;
                    run.ErrorSummary = buildResult.Message;
                    run.CompletedAt = DateTime.UtcNow;
                    run.UpdateDateTime = DateTime.UtcNow;
                    run.UpdateBy = actorUserId;
                    await _dbContext.SaveChangesAsync(cancellationToken);

                    return LeaveAccrualServiceResult<LeaveAccrualRunActionResponse>.Fail(
                        buildResult.StatusCode,
                        buildResult.Message);
                }

                var candidates = buildResult.Data;
                run.TargetCount = candidates.Count;
                run.CalculatedCount = candidates.Count(x => x.Response.CalculatedAccrualDays > 0);
                run.TotalCalculatedDays = candidates.Sum(x => x.Response.CalculatedAccrualDays);
                await _dbContext.SaveChangesAsync(cancellationToken);

                var itemResults = new List<object>();
                foreach (var candidate in candidates)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (!candidate.Response.IsEligible)
                    {
                        run.SkippedCount++;
                        itemResults.Add(new
                        {
                            candidate.Response.WorkforceProfileId,
                            candidate.Response.ResultCode,
                            candidate.Response.ResultMessage,
                            amount = candidate.Response.CalculatedAccrualDays
                        });
                        continue;
                    }

                    if (run.IsDryRun)
                    {
                        itemResults.Add(new
                        {
                            candidate.Response.WorkforceProfileId,
                            resultCode = "DRY_RUN_CALCULATED",
                            resultMessage = "Perhitungan dry-run berhasil tanpa posting ledger.",
                            amount = candidate.Response.CalculatedAccrualDays
                        });
                        continue;
                    }

                    var postResult = await PostCandidateAsync(
                        run.Id,
                        candidate,
                        actorUserId,
                        run.ForceReprocess,
                        cancellationToken);

                    if (postResult.Success && postResult.Data != null)
                    {
                        if (postResult.Data.IsIdempotent)
                        {
                            run.SkippedCount++;
                        }
                        else
                        {
                            run.PostedCount++;
                            run.TotalPostedDays += postResult.Data.TotalPostedDays;
                        }
                    }
                    else
                    {
                        run.FailedCount++;
                    }

                    itemResults.Add(new
                    {
                        candidate.Response.WorkforceProfileId,
                        success = postResult.Success,
                        message = postResult.Message,
                        amount = candidate.Response.CalculatedAccrualDays,
                        isIdempotent = postResult.Data?.IsIdempotent ?? false
                    });
                }

                run.CompletedAt = DateTime.UtcNow;
                run.RunStatus = run.FailedCount > 0
                    ? LeaveValueConstants.BatchRunStatus.CompletedWithErrors
                    : LeaveValueConstants.BatchRunStatus.Completed;
                run.ResultSummaryJson = JsonSerializer.Serialize(new
                {
                    run.TargetCount,
                    run.CalculatedCount,
                    run.PostedCount,
                    run.SkippedCount,
                    run.FailedCount,
                    run.TotalCalculatedDays,
                    run.TotalPostedDays,
                    items = itemResults.Take(1000)
                }, JsonOptions);
                run.UpdateDateTime = DateTime.UtcNow;
                run.UpdateBy = actorUserId;
                await _dbContext.SaveChangesAsync(cancellationToken);

                return LeaveAccrualServiceResult<LeaveAccrualRunActionResponse>.Ok(
                    MapAction(run, false, run.RunStatus == LeaveValueConstants.BatchRunStatus.Completed
                        ? "Leave accrual run selesai."
                        : "Leave accrual run selesai dengan beberapa kegagalan."),
                    "Leave accrual run selesai diproses.");
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Leave accrual run {RunId} gagal.", run.Id);
                run.RunStatus = LeaveValueConstants.BatchRunStatus.Failed;
                run.ErrorSummary = ex.Message.Length > 4000 ? ex.Message[..4000] : ex.Message;
                run.CompletedAt = DateTime.UtcNow;
                run.UpdateDateTime = DateTime.UtcNow;
                run.UpdateBy = actorUserId;
                await _dbContext.SaveChangesAsync(CancellationToken.None);

                return LeaveAccrualServiceResult<LeaveAccrualRunActionResponse>.Fail(
                    StatusCodes.Status500InternalServerError,
                    $"Leave accrual run gagal: {ex.Message}");
            }
        }

        public async Task<LeaveAccrualServiceResult<LeaveAccrualRunActionResponse>> RetryAsync(
            Guid runId,
            Guid actorUserId,
            string? reason,
            CancellationToken cancellationToken = default)
        {
            var run = await _dbContext.Set<TrxLeaveAccrualRun>()
                .FirstOrDefaultAsync(x => x.Id == runId && !x.IsDelete, cancellationToken);

            if (run == null)
            {
                return LeaveAccrualServiceResult<LeaveAccrualRunActionResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Leave accrual run tidak ditemukan.");
            }

            var retryable = run.RunStatus == LeaveValueConstants.BatchRunStatus.Failed ||
                            run.RunStatus == LeaveValueConstants.BatchRunStatus.CompletedWithErrors;
            if (!retryable)
            {
                return LeaveAccrualServiceResult<LeaveAccrualRunActionResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Hanya run Failed atau CompletedWithErrors yang dapat dijadwalkan ulang.");
            }

            if (run.RetryCount >= run.MaximumRetryCount)
            {
                return LeaveAccrualServiceResult<LeaveAccrualRunActionResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Batas maksimum retry leave accrual run telah tercapai.");
            }

            run.RetryCount++;
            run.RunMode = LeaveValueConstants.BatchRunMode.Reprocess;
            run.RunStatus = LeaveValueConstants.BatchRunStatus.Queued;
            run.ForceReprocess = true;
            run.ErrorSummary = null;
            run.CompletedAt = null;
            run.Notes = AppendNote(run.Notes, reason);
            run.UpdateDateTime = DateTime.UtcNow;
            run.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return LeaveAccrualServiceResult<LeaveAccrualRunActionResponse>.Ok(
                MapAction(run, false, "Leave accrual run berhasil dijadwalkan ulang."),
                "Leave accrual run berhasil dijadwalkan ulang.");
        }

        public async Task<LeaveAccrualServiceResult<LeaveAccrualRunActionResponse>> CancelAsync(
            Guid runId,
            Guid actorUserId,
            string reason,
            CancellationToken cancellationToken = default)
        {
            var run = await _dbContext.Set<TrxLeaveAccrualRun>()
                .FirstOrDefaultAsync(x => x.Id == runId && !x.IsDelete, cancellationToken);

            if (run == null)
            {
                return LeaveAccrualServiceResult<LeaveAccrualRunActionResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Leave accrual run tidak ditemukan.");
            }

            if (run.RunStatus != LeaveValueConstants.BatchRunStatus.Draft &&
                run.RunStatus != LeaveValueConstants.BatchRunStatus.Queued)
            {
                return LeaveAccrualServiceResult<LeaveAccrualRunActionResponse>.Fail(
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

            return LeaveAccrualServiceResult<LeaveAccrualRunActionResponse>.Ok(
                MapAction(run, false, "Leave accrual run berhasil dibatalkan."),
                "Leave accrual run berhasil dibatalkan.");
        }

        public async Task<LeaveAccrualServiceResult<LeaveAccrualReconciliationResponse>> ReconcileAsync(
            Guid runId,
            CancellationToken cancellationToken = default)
        {
            var run = await _dbContext.Set<TrxLeaveAccrualRun>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == runId && !x.IsDelete, cancellationToken);

            if (run == null)
            {
                return LeaveAccrualServiceResult<LeaveAccrualReconciliationResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Leave accrual run tidak ditemukan.");
            }

            var accruals = await _dbContext.Set<TrxLeaveAccrual>()
                .AsNoTracking()
                .Where(x => x.LeaveAccrualRunId == runId && !x.IsDelete)
                .ToListAsync(cancellationToken);

            var accrualIds = accruals.Select(x => x.Id).ToList();
            var ledgers = await _dbContext.Set<TrxLeaveBalanceTransaction>()
                .AsNoTracking()
                .Where(x =>
                    x.PostingBatchId == runId &&
                    x.PostingBatchType == LeaveValueConstants.PostingBatchType.AccrualRun &&
                    !x.IsDelete)
                .ToListAsync(cancellationToken);

            var issues = new List<LeaveAccrualReconciliationIssueResponse>();
            foreach (var accrual in accruals.Where(x => x.AccrualStatus == LeaveValueConstants.AccrualStatus.Posted))
            {
                var ledger = ledgers.FirstOrDefault(x => x.LeaveAccrualId == accrual.Id);
                if (ledger == null)
                {
                    issues.Add(new LeaveAccrualReconciliationIssueResponse
                    {
                        Code = "POSTED_ACCRUAL_WITHOUT_LEDGER",
                        Severity = "Critical",
                        Message = $"Accrual {accrual.AccrualNumber} berstatus Posted tetapi ledger tidak ditemukan.",
                        LeaveAccrualId = accrual.Id
                    });
                    continue;
                }

                if (ledger.TransactionStatus != LeaveValueConstants.TransactionStatus.Posted)
                {
                    issues.Add(new LeaveAccrualReconciliationIssueResponse
                    {
                        Code = "LEDGER_NOT_POSTED",
                        Severity = "Critical",
                        Message = $"Ledger {ledger.TransactionNumber} belum berstatus Posted.",
                        LeaveAccrualId = accrual.Id,
                        LeaveBalanceTransactionId = ledger.Id
                    });
                }

                if (Math.Abs(ledger.AccruedDelta - accrual.AccrualAmountDays) > 0.0001m)
                {
                    issues.Add(new LeaveAccrualReconciliationIssueResponse
                    {
                        Code = "ACCRUAL_LEDGER_AMOUNT_MISMATCH",
                        Severity = "Critical",
                        Message = $"Nilai accrual {accrual.AccrualAmountDays} tidak sama dengan AccruedDelta ledger {ledger.AccruedDelta}.",
                        LeaveAccrualId = accrual.Id,
                        LeaveBalanceTransactionId = ledger.Id
                    });
                }
            }

            foreach (var ledger in ledgers.Where(x => x.LeaveAccrualId.HasValue && !accrualIds.Contains(x.LeaveAccrualId.Value)))
            {
                issues.Add(new LeaveAccrualReconciliationIssueResponse
                {
                    Code = "ORPHAN_ACCRUAL_LEDGER",
                    Severity = "Critical",
                    Message = $"Ledger {ledger.TransactionNumber} tidak mempunyai accrual aktif.",
                    LeaveAccrualId = ledger.LeaveAccrualId,
                    LeaveBalanceTransactionId = ledger.Id
                });
            }

            var postedAccruals = accruals
                .Where(x => x.AccrualStatus == LeaveValueConstants.AccrualStatus.Posted)
                .ToList();
            var postedLedgers = ledgers
                .Where(x => x.TransactionStatus == LeaveValueConstants.TransactionStatus.Posted)
                .ToList();

            var response = new LeaveAccrualReconciliationResponse
            {
                LeaveAccrualRunId = run.Id,
                RunNumber = run.RunNumber,
                RunStatus = run.RunStatus,
                RunPostedCount = run.PostedCount,
                ActualPostedAccrualCount = postedAccruals.Count,
                PostedLedgerCount = postedLedgers.Count,
                RunTotalPostedDays = run.TotalPostedDays,
                ActualPostedAccrualDays = postedAccruals.Sum(x => x.AccrualAmountDays),
                LedgerAccruedDeltaDays = postedLedgers.Sum(x => x.AccruedDelta),
                Issues = issues
            };

            if (run.PostedCount != response.ActualPostedAccrualCount)
            {
                response.Issues.Add(new LeaveAccrualReconciliationIssueResponse
                {
                    Code = "RUN_POSTED_COUNT_MISMATCH",
                    Severity = "Warning",
                    Message = "PostedCount pada run tidak sama dengan jumlah accrual Posted."
                });
            }

            if (Math.Abs(run.TotalPostedDays - response.ActualPostedAccrualDays) > 0.0001m)
            {
                response.Issues.Add(new LeaveAccrualReconciliationIssueResponse
                {
                    Code = "RUN_POSTED_DAYS_MISMATCH",
                    Severity = "Warning",
                    Message = "TotalPostedDays pada run tidak sama dengan total accrual Posted."
                });
            }

            response.IsBalanced = response.Issues.All(x => x.Severity != "Critical");

            return LeaveAccrualServiceResult<LeaveAccrualReconciliationResponse>.Ok(
                response,
                "Reconciliation leave accrual run berhasil dihitung.");
        }

        internal async Task<LeaveAccrualServiceResult<LeaveAccrualRunActionResponse>> PostCandidateAsync(
            Guid runId,
            LeaveAccrualCandidateWorkItem candidate,
            Guid actorUserId,
            bool forceReprocess,
            CancellationToken cancellationToken)
        {
            var idempotencyKey = BuildItemIdempotencyKey(
                runId,
                candidate.Response.WorkforceProfileId,
                candidate.Response.LeaveTypeId,
                candidate.Response.AccrualSequence,
                candidate.AccrualPeriodStartDate,
                candidate.AccrualPeriodEndDate);

            await using IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                var existing = await _dbContext.Set<TrxLeaveAccrual>()
                    .FirstOrDefaultAsync(x =>
                        x.IdempotencyKey == idempotencyKey &&
                        !x.IsDelete,
                        cancellationToken);

                if (existing != null &&
                    existing.AccrualStatus == LeaveValueConstants.AccrualStatus.Posted)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return LeaveAccrualServiceResult<LeaveAccrualRunActionResponse>.Ok(
                        new LeaveAccrualRunActionResponse
                        {
                            Id = runId,
                            IsIdempotent = true,
                            Message = "Accrual item sudah pernah diposting."
                        },
                        "Accrual item sudah pernah diposting.");
                }

                var balance = await _dbContext.Set<WfpLeaveBalance>()
                    .FromSqlInterpolated($@"
                        SELECT *
                        FROM public.""WfpLeaveBalance""
                        WHERE ""Id"" = {candidate.Balance.Id}
                          AND ""IsDelete"" = false
                        FOR UPDATE")
                    .FirstOrDefaultAsync(cancellationToken);

                if (balance == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return LeaveAccrualServiceResult<LeaveAccrualRunActionResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        "Leave balance tidak ditemukan saat posting accrual.");
                }

                if (balance.IsLocked ||
                    balance.BalanceStatus == LeaveValueConstants.BalanceStatus.Locked ||
                    balance.BalanceStatus == LeaveValueConstants.BalanceStatus.Closed)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return LeaveAccrualServiceResult<LeaveAccrualRunActionResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        "Leave balance sedang dikunci atau sudah ditutup.");
                }

                if (candidate.EntitlementPolicy.MaximumBalanceDays.HasValue)
                {
                    var remainingCapacity = candidate.EntitlementPolicy.MaximumBalanceDays.Value - balance.RemainingDays;
                    if (remainingCapacity <= 0)
                    {
                        await transaction.CommitAsync(cancellationToken);
                        return LeaveAccrualServiceResult<LeaveAccrualRunActionResponse>.Ok(
                            new LeaveAccrualRunActionResponse
                            {
                                Id = runId,
                                IsIdempotent = true,
                                Message = "Maximum balance telah tercapai."
                            },
                            "Maximum balance telah tercapai.");
                    }
                }

                var amount = candidate.Response.CalculatedAccrualDays;
                if (amount <= 0)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return LeaveAccrualServiceResult<LeaveAccrualRunActionResponse>.Ok(
                        new LeaveAccrualRunActionResponse
                        {
                            Id = runId,
                            IsIdempotent = true,
                            Message = "Nilai accrual nol sehingga tidak ada ledger yang diposting."
                        },
                        "Nilai accrual nol.");
                }

                if (candidate.EntitlementPolicy.MaximumBalanceDays.HasValue)
                {
                    amount = Math.Min(
                        amount,
                        Math.Max(0, candidate.EntitlementPolicy.MaximumBalanceDays.Value - balance.RemainingDays));
                }

                amount = RoundDays(amount, candidate.EntitlementPolicy.RoundingMethod);
                if (amount <= 0)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return LeaveAccrualServiceResult<LeaveAccrualRunActionResponse>.Ok(
                        new LeaveAccrualRunActionResponse
                        {
                            Id = runId,
                            IsIdempotent = true,
                            Message = "Nilai accrual menjadi nol setelah pembulatan atau pembatasan saldo."
                        },
                        "Nilai accrual nol.");
                }

                var now = DateTime.UtcNow;
                var previousAvailable = balance.AvailableDays;
                var previousRemaining = balance.RemainingDays;
                var nextSequence = balance.LastTransactionSequence + 1;

                TrxLeaveAccrual accrual;
                if (existing != null)
                {
                    accrual = existing;
                    accrual.AccrualAmountDays = amount;
                    accrual.BalanceBeforeAccrual = previousRemaining;
                    accrual.BalanceAfterAccrual = previousRemaining + amount;
                    accrual.AccrualStatus = LeaveValueConstants.AccrualStatus.Posted;
                    accrual.PostedAt = now;
                    accrual.PostedByUserId = actorUserId == Guid.Empty ? null : actorUserId;
                    accrual.UpdateDateTime = now;
                    accrual.UpdateBy = actorUserId;
                }
                else
                {
                    accrual = new TrxLeaveAccrual
                    {
                        Id = Guid.NewGuid(),
                        AccrualNumber = GenerateNumber("LAC"),
                        WorkforceProfileId = candidate.Response.WorkforceProfileId,
                        LeaveTypeId = candidate.Response.LeaveTypeId,
                        LeaveBalanceId = balance.Id,
                        LeaveEntitlementId = candidate.Entitlement.Id,
                        LeaveEntitlementPolicyId = candidate.EntitlementPolicy.Id,
                        LeaveAccrualRunId = runId,
                        AccrualDate = candidate.ScheduledAccrualDate,
                        ScheduledAccrualDate = candidate.ScheduledAccrualDate,
                        AccrualPeriodStartDate = candidate.AccrualPeriodStartDate,
                        AccrualPeriodEndDate = candidate.AccrualPeriodEndDate,
                        AccrualSequence = candidate.Response.AccrualSequence,
                        AccrualAmountDays = amount,
                        BalanceBeforeAccrual = previousRemaining,
                        BalanceAfterAccrual = previousRemaining + amount,
                        IsProrated = candidate.Response.IsProrated,
                        IdempotencyKey = idempotencyKey,
                        AccrualStatus = LeaveValueConstants.AccrualStatus.Posted,
                        AccrualFrequency = candidate.EntitlementPolicy.AccrualFrequency,
                        SourceType = "AccrualRun",
                        SourceReferenceId = runId,
                        CalculatedAt = now,
                        CalculatedByUserId = actorUserId == Guid.Empty ? null : actorUserId,
                        PostedAt = now,
                        PostedByUserId = actorUserId == Guid.Empty ? null : actorUserId,
                        CalculationDetailJson = candidate.Response.CalculationDetailJson,
                        Notes = candidate.Response.ResultMessage,
                        IsActive = true,
                        CreateDateTime = now,
                        CreateBy = actorUserId,
                        UpdateBy = actorUserId,
                        DeleteBy = Guid.Empty,
                        CancelBy = Guid.Empty
                    };
                    _dbContext.Add(accrual);
                }

                var ledger = new TrxLeaveBalanceTransaction
                {
                    Id = Guid.NewGuid(),
                    TransactionNumber = GenerateNumber("LBT"),
                    LeaveBalanceId = balance.Id,
                    WorkforceProfileId = balance.WorkforceProfileId,
                    LeaveTypeId = balance.LeaveTypeId,
                    LeaveEntitlementPeriodId = balance.LeaveEntitlementPeriodId,
                    LeaveEntitlementId = candidate.Entitlement.Id,
                    LeaveAccrualId = accrual.Id,
                    TransactionDateTime = now,
                    EffectiveDate = candidate.ScheduledAccrualDate,
                    TransactionSequence = nextSequence,
                    TransactionType = LeaveValueConstants.TransactionType.Accrual,
                    Direction = LeaveValueConstants.TransactionDirection.Credit,
                    TransactionDays = amount,
                    AccruedDelta = amount,
                    AvailableDelta = amount,
                    PreviousOpeningBalanceDays = balance.OpeningBalanceDays,
                    PreviousAvailableDays = previousAvailable,
                    PreviousReservedDays = balance.ReservedDays,
                    NewAvailableDays = previousAvailable + amount,
                    NewReservedDays = balance.ReservedDays,
                    NewUsedDays = balance.UsedDays,
                    IdempotencyKey = $"LEDGER:{idempotencyKey}",
                    PostingBatchType = LeaveValueConstants.PostingBatchType.AccrualRun,
                    PostingBatchId = runId,
                    SourceType = "AccrualRun",
                    SourceReferenceId = runId,
                    TransactionStatus = LeaveValueConstants.TransactionStatus.Posted,
                    PostedAt = now,
                    PostedByUserId = actorUserId == Guid.Empty ? null : actorUserId,
                    Remarks = candidate.Response.ResultMessage,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    UpdateBy = actorUserId,
                    DeleteBy = Guid.Empty,
                    CancelBy = Guid.Empty
                };

                balance.AccruedDays += amount;
                balance.RemainingDays += amount;
                balance.AvailableDays += amount;
                balance.LastTransactionId = ledger.Id;
                balance.LastTransactionSequence = nextSequence;
                balance.BalanceVersion++;
                balance.LastCalculatedAt = now;
                balance.UpdateDateTime = now;
                balance.UpdateBy = actorUserId;

                _dbContext.Add(ledger);
                await _dbContext.SaveChangesAsync(cancellationToken);

                accrual.BalanceTransactionId = ledger.Id;
                accrual.UpdateDateTime = now;
                accrual.UpdateBy = actorUserId;
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return LeaveAccrualServiceResult<LeaveAccrualRunActionResponse>.Ok(
                    new LeaveAccrualRunActionResponse
                    {
                        Id = runId,
                        IsIdempotent = false,
                        TotalPostedDays = amount,
                        Message = "Accrual berhasil diposting ke immutable leave balance ledger."
                    },
                    "Accrual berhasil diposting.");
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync(CancellationToken.None);
                DetachTrackedEntitiesExceptRun(runId);

                var duplicate = await _dbContext.Set<TrxLeaveAccrual>()
                    .AsNoTracking()
                    .AnyAsync(x => x.IdempotencyKey == idempotencyKey && !x.IsDelete, CancellationToken.None);
                if (duplicate)
                {
                    return LeaveAccrualServiceResult<LeaveAccrualRunActionResponse>.Ok(
                        new LeaveAccrualRunActionResponse
                        {
                            Id = runId,
                            IsIdempotent = true,
                            Message = "Accrual sudah diposting oleh proses lain."
                        },
                        "Accrual sudah diposting.");
                }

                _logger.LogError(ex, "Posting leave accrual gagal untuk workforce {WorkforceProfileId}.", candidate.Response.WorkforceProfileId);
                return LeaveAccrualServiceResult<LeaveAccrualRunActionResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Posting accrual gagal: {ex.GetBaseException().Message}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(CancellationToken.None);
                DetachTrackedEntitiesExceptRun(runId);
                _logger.LogError(ex, "Posting leave accrual gagal untuk workforce {WorkforceProfileId}.", candidate.Response.WorkforceProfileId);
                return LeaveAccrualServiceResult<LeaveAccrualRunActionResponse>.Fail(
                    StatusCodes.Status500InternalServerError,
                    $"Posting accrual gagal: {ex.Message}");
            }
            finally
            {
                DetachTrackedEntitiesExceptRun(runId);
            }
        }

        private async Task<LeaveAccrualServiceResult<List<LeaveAccrualCandidateWorkItem>>> BuildCandidatesAsync(
            LeaveAccrualPreviewRequest request,
            CancellationToken cancellationToken)
        {
            var validation = await ValidateRequestAsync(request, cancellationToken);
            if (!validation.Success)
            {
                return LeaveAccrualServiceResult<List<LeaveAccrualCandidateWorkItem>>.Fail(
                    validation.StatusCode,
                    validation.Message);
            }

            var period = validation.Period!;
            var leaveTypeId = validation.LeaveTypeId!.Value;
            var policies = await _policyResolver.LoadCandidatePoliciesAsync(
                leaveTypeId,
                request.ScheduledAccrualDate,
                cancellationToken);

            if (policies.Count == 0)
            {
                return LeaveAccrualServiceResult<List<LeaveAccrualCandidateWorkItem>>.Fail(
                    StatusCodes.Status409Conflict,
                    "Tidak terdapat leave policy aktif untuk leave type yang dipilih.");
            }

            var employeesQuery = _dbContext.Set<MstEmployee>()
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Where(x =>
                    x.IsActive &&
                    !x.IsDelete &&
                    x.WorkforceProfile != null &&
                    x.WorkforceProfile.IsActive &&
                    !x.WorkforceProfile.IsDelete &&
                    x.JoinDate <= request.AccrualPeriodEndDate.ToDateTime(TimeOnly.MaxValue) &&
                    (!x.ResignDate.HasValue || x.ResignDate.Value >= request.AccrualPeriodStartDate.ToDateTime(TimeOnly.MinValue)));

            if (request.WorkforceProfileId.HasValue)
            {
                employeesQuery = employeesQuery.Where(x => x.WorkforceProfileId == request.WorkforceProfileId.Value);
            }

            var employees = await employeesQuery
                .OrderBy(x => x.EmployeeNumber)
                .Take(10000)
                .ToListAsync(cancellationToken);

            var workforceIds = employees.Select(x => x.WorkforceProfileId).Distinct().ToList();
            if (workforceIds.Count == 0)
            {
                return LeaveAccrualServiceResult<List<LeaveAccrualCandidateWorkItem>>.Ok(
                    new List<LeaveAccrualCandidateWorkItem>(),
                    "Tidak ada workforce yang memenuhi scope awal.");
            }

            var evaluationStart = request.ScheduledAccrualDate
                .ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var evaluationEnd = request.ScheduledAccrualDate
                .ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

            var assignments = await _dbContext.Set<WfpOrganizationAssignment>()
                .AsNoTracking()
                .Where(x =>
                    workforceIds.Contains(x.WorkforceProfileId) &&
                    x.IsActive &&
                    !x.IsDelete &&
                    x.EffectiveStartDate <= evaluationEnd &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= evaluationStart))
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.EffectiveStartDate)
                .ToListAsync(cancellationToken);

            var assignmentByWorkforce = assignments
                .GroupBy(x => x.WorkforceProfileId)
                .ToDictionary(x => x.Key, x => x.First());

            employees = employees
                .Where(employee => IsWithinRequestedScope(
                    assignmentByWorkforce.GetValueOrDefault(employee.WorkforceProfileId),
                    request))
                .ToList();

            workforceIds = employees.Select(x => x.WorkforceProfileId).Distinct().ToList();

            var entitlements = await _dbContext.Set<TrxLeaveEntitlement>()
                .AsNoTracking()
                .Where(x =>
                    workforceIds.Contains(x.WorkforceProfileId) &&
                    x.LeaveTypeId == leaveTypeId &&
                    x.LeaveEntitlementPeriodId == period.Id &&
                    x.IsActive &&
                    !x.IsDelete &&
                    x.EntitlementStatus != LeaveValueConstants.EntitlementStatus.Cancelled &&
                    x.EntitlementStatus != LeaveValueConstants.EntitlementStatus.Expired)
                .OrderByDescending(x => x.PostedAt)
                .ThenByDescending(x => x.CreateDateTime)
                .ToListAsync(cancellationToken);

            var entitlementByWorkforce = entitlements
                .GroupBy(x => x.WorkforceProfileId)
                .ToDictionary(x => x.Key, x => x.First());

            var balances = await _dbContext.Set<WfpLeaveBalance>()
                .AsNoTracking()
                .Where(x =>
                    workforceIds.Contains(x.WorkforceProfileId) &&
                    x.LeaveTypeId == leaveTypeId &&
                    x.LeaveEntitlementPeriodId == period.Id &&
                    x.IsActive &&
                    !x.IsDelete)
                .ToListAsync(cancellationToken);

            var balanceByWorkforce = balances
                .GroupBy(x => x.WorkforceProfileId)
                .ToDictionary(x => x.Key, x => x.First());

            var entitlementIds = entitlements.Select(x => x.Id).ToList();
            var existingAccruals = await _dbContext.Set<TrxLeaveAccrual>()
                .AsNoTracking()
                .Where(x =>
                    x.LeaveEntitlementId.HasValue &&
                    entitlementIds.Contains(x.LeaveEntitlementId.Value) &&
                    x.AccrualPeriodStartDate == request.AccrualPeriodStartDate &&
                    x.AccrualPeriodEndDate == request.AccrualPeriodEndDate &&
                    x.IsActive &&
                    !x.IsDelete)
                .ToListAsync(cancellationToken);

            var result = new List<LeaveAccrualCandidateWorkItem>();
            foreach (var employee in employees)
            {
                var profile = employee.WorkforceProfile!;
                entitlementByWorkforce.TryGetValue(employee.WorkforceProfileId, out var entitlement);
                balanceByWorkforce.TryGetValue(employee.WorkforceProfileId, out var balance);
                assignmentByWorkforce.TryGetValue(employee.WorkforceProfileId, out var assignment);

                if (entitlement == null)
                {
                    result.Add(Skipped(profile, leaveTypeId, "MISSING_ENTITLEMENT", "Entitlement pada periode tersebut belum tersedia."));
                    continue;
                }

                if (balance == null)
                {
                    result.Add(Skipped(profile, leaveTypeId, "MISSING_BALANCE", "Leave balance pada periode tersebut belum tersedia."));
                    continue;
                }

                if (balance.IsLocked ||
                    balance.BalanceStatus == LeaveValueConstants.BalanceStatus.Locked ||
                    balance.BalanceStatus == LeaveValueConstants.BalanceStatus.Closed)
                {
                    result.Add(Skipped(profile, leaveTypeId, "BALANCE_LOCKED", "Leave balance sedang dikunci atau sudah ditutup.", entitlement, balance));
                    continue;
                }

                var resolution = _policyResolver.Resolve(
                    new LeaveAccrualPolicyContext
                    {
                        WorkforceProfile = profile,
                        Employee = employee,
                        OrganizationAssignment = assignment,
                        LeaveTypeId = leaveTypeId,
                        EvaluationDate = request.ScheduledAccrualDate,
                        RequestedEntitlementPolicyId = request.LeaveEntitlementPolicyId
                    },
                    policies);

                if (!resolution.Success || resolution.LeavePolicy == null || resolution.EntitlementPolicy == null)
                {
                    result.Add(Skipped(
                        profile,
                        leaveTypeId,
                        resolution.Code,
                        resolution.Message,
                        entitlement,
                        balance,
                        resolution.LeavePolicy,
                        resolution.EntitlementPolicy));
                    continue;
                }

                var existingForWindow = existingAccruals
                    .Where(x => x.LeaveEntitlementId == entitlement.Id)
                    .OrderByDescending(x => x.AccrualSequence)
                    .ToList();

                var postedExisting = existingForWindow.FirstOrDefault(
                    x => x.AccrualStatus == LeaveValueConstants.AccrualStatus.Posted);
                if (postedExisting != null)
                {
                    result.Add(Skipped(
                        profile,
                        leaveTypeId,
                        "ALREADY_POSTED",
                        $"Accrual untuk window ini sudah diposting melalui {postedExisting.AccrualNumber}.",
                        entitlement,
                        balance,
                        resolution.LeavePolicy,
                        resolution.EntitlementPolicy));
                    continue;
                }

                var retryableExisting = existingForWindow
                    .OrderByDescending(x => x.AccrualSequence)
                    .FirstOrDefault();
                var sequence = retryableExisting?.AccrualSequence
                    ?? await GetNextAccrualSequenceAsync(entitlement.Id, cancellationToken);

                var calculation = CalculateAccrual(
                    employee,
                    entitlement,
                    balance,
                    resolution.EntitlementPolicy,
                    request.AccrualPeriodStartDate,
                    request.AccrualPeriodEndDate,
                    request.ScheduledAccrualDate,
                    sequence,
                    existingAccruals.Where(x =>
                        x.LeaveEntitlementId == entitlement.Id &&
                        x.AccrualStatus == LeaveValueConstants.AccrualStatus.Posted));

                result.Add(new LeaveAccrualCandidateWorkItem
                {
                    Employee = employee,
                    Entitlement = entitlement,
                    Balance = balance,
                    LeavePolicy = resolution.LeavePolicy,
                    EntitlementPolicy = resolution.EntitlementPolicy,
                    ScheduledAccrualDate = request.ScheduledAccrualDate,
                    AccrualPeriodStartDate = request.AccrualPeriodStartDate,
                    AccrualPeriodEndDate = request.AccrualPeriodEndDate,
                    Response = new LeaveAccrualCandidateResponse
                    {
                        WorkforceProfileId = profile.Id,
                        WorkforceProfileCode = profile.ProfileCode,
                        WorkforceDisplayName = profile.DisplayName,
                        LeaveTypeId = leaveTypeId,
                        LeaveTypeCode = resolution.LeavePolicy.LeaveType?.LeaveTypeCode,
                        LeaveTypeName = resolution.LeavePolicy.LeaveType?.LeaveTypeName,
                        LeavePolicyId = resolution.LeavePolicy.Id,
                        LeavePolicyCode = resolution.LeavePolicy.LeavePolicyCode,
                        LeaveEntitlementPolicyId = resolution.EntitlementPolicy.Id,
                        LeaveEntitlementPolicyCode = resolution.EntitlementPolicy.EntitlementPolicyCode,
                        LeaveEntitlementId = entitlement.Id,
                        LeaveBalanceId = balance.Id,
                        CurrentRemainingDays = balance.RemainingDays,
                        CurrentAvailableDays = balance.AvailableDays,
                        CalculatedAccrualDays = calculation.Amount,
                        AccrualSequence = sequence,
                        IsProrated = calculation.IsProrated,
                        IsEligible = calculation.Amount > 0,
                        ResultCode = calculation.Code,
                        ResultMessage = calculation.Message,
                        CalculationDetailJson = calculation.DetailJson
                    }
                });
            }

            return LeaveAccrualServiceResult<List<LeaveAccrualCandidateWorkItem>>.Ok(
                result,
                "Kandidat leave accrual berhasil dihitung.");
        }

        private AccrualCalculationResult CalculateAccrual(
            MstEmployee employee,
            TrxLeaveEntitlement entitlement,
            WfpLeaveBalance balance,
            MstLeaveEntitlementPolicy policy,
            DateOnly windowStart,
            DateOnly windowEnd,
            DateOnly scheduledDate,
            int sequence,
            IEnumerable<TrxLeaveAccrual> previousPostedAccruals)
        {
            var amount = policy.AccrualAmountDays > 0
                ? policy.AccrualAmountDays
                : CalculateDefaultAccrualAmount(policy);

            var isProrated = false;
            var factor = 1m;
            var joinDate = DateOnly.FromDateTime(employee.JoinDate);
            var separationDate = employee.ResignDate.HasValue
                ? DateOnly.FromDateTime(employee.ResignDate.Value)
                : (DateOnly?)null;

            if (joinDate > windowEnd)
            {
                return AccrualCalculationResult.Skip("JOIN_AFTER_WINDOW", "Tanggal bergabung berada setelah accrual window.");
            }

            if (separationDate.HasValue && separationDate.Value < windowStart)
            {
                return AccrualCalculationResult.Skip("SEPARATED_BEFORE_WINDOW", "Tanggal berakhir kerja berada sebelum accrual window.");
            }

            if (policy.IsProratedOnJoin && joinDate > windowStart && joinDate <= windowEnd)
            {
                var firstFactor = policy.FirstAccrualRule switch
                {
                    LeaveValueConstants.FirstAccrualRule.Full => 1m,
                    LeaveValueConstants.FirstAccrualRule.Prorated => CalculateOverlapFactor(joinDate, separationDate, windowStart, windowEnd),
                    LeaveValueConstants.FirstAccrualRule.NextFullPeriod => 0m,
                    LeaveValueConstants.FirstAccrualRule.None => 0m,
                    _ => CalculateOverlapFactor(joinDate, separationDate, windowStart, windowEnd)
                };
                factor = Math.Min(factor, firstFactor);
                isProrated = firstFactor < 1m;
            }

            if (policy.IsProratedOnSeparation &&
                separationDate.HasValue &&
                separationDate.Value >= windowStart &&
                separationDate.Value < windowEnd)
            {
                var finalFactor = policy.FinalAccrualRule switch
                {
                    LeaveValueConstants.FinalAccrualRule.Full => 1m,
                    LeaveValueConstants.FinalAccrualRule.Prorated => CalculateOverlapFactor(joinDate, separationDate, windowStart, windowEnd),
                    LeaveValueConstants.FinalAccrualRule.PreviousFullPeriod => 0m,
                    LeaveValueConstants.FinalAccrualRule.None => 0m,
                    _ => CalculateOverlapFactor(joinDate, separationDate, windowStart, windowEnd)
                };
                factor = Math.Min(factor, finalFactor);
                isProrated = isProrated || finalFactor < 1m;
            }

            amount *= factor;

            if (policy.AccrualMaximumPerPeriodDays.HasValue)
            {
                var postedInPeriod = previousPostedAccruals.Sum(x => x.AccrualAmountDays);
                amount = Math.Min(
                    amount,
                    Math.Max(0, policy.AccrualMaximumPerPeriodDays.Value - postedInPeriod));
            }

            if (policy.MaximumBalanceDays.HasValue)
            {
                amount = Math.Min(
                    amount,
                    Math.Max(0, policy.MaximumBalanceDays.Value - balance.RemainingDays));
            }

            amount = RoundDays(amount, policy.RoundingMethod);
            var code = amount > 0 ? "ELIGIBLE" : "NO_ACCRUAL_AMOUNT";
            var message = amount > 0
                ? "Workforce memenuhi syarat accrual."
                : "Tidak ada nilai accrual yang dapat diposting setelah prorata, maksimum periode, maksimum saldo, dan pembulatan.";

            var detail = JsonSerializer.Serialize(new
            {
                policyId = policy.Id,
                policyCode = policy.EntitlementPolicyCode,
                policy.AccrualFrequency,
                baseAmount = policy.AccrualAmountDays > 0
                    ? policy.AccrualAmountDays
                    : CalculateDefaultAccrualAmount(policy),
                factor,
                isProrated,
                joinDate,
                separationDate,
                windowStart,
                windowEnd,
                scheduledDate,
                sequence,
                currentRemainingDays = balance.RemainingDays,
                policy.MaximumBalanceDays,
                policy.AccrualMaximumPerPeriodDays,
                policy.RoundingMethod,
                resultAmount = amount
            }, JsonOptions);

            return new AccrualCalculationResult
            {
                Amount = amount,
                IsProrated = isProrated,
                Code = code,
                Message = message,
                DetailJson = detail
            };
        }

        private async Task<int> GetNextAccrualSequenceAsync(
            Guid entitlementId,
            CancellationToken cancellationToken)
        {
            var max = await _dbContext.Set<TrxLeaveAccrual>()
                .AsNoTracking()
                .Where(x => x.LeaveEntitlementId == entitlementId && !x.IsDelete)
                .MaxAsync(x => (int?)x.AccrualSequence, cancellationToken);
            return (max ?? 0) + 1;
        }

        private async Task<AccrualValidationResult> ValidateRequestAsync(
            LeaveAccrualPreviewRequest request,
            CancellationToken cancellationToken)
        {
            if (request.LeaveEntitlementPeriodId == Guid.Empty)
            {
                return AccrualValidationResult.Fail(
                    StatusCodes.Status400BadRequest,
                    "Leave entitlement period wajib dipilih.");
            }

            if (request.AccrualPeriodStartDate > request.AccrualPeriodEndDate)
            {
                return AccrualValidationResult.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tanggal awal accrual tidak boleh lebih besar dari tanggal akhir.");
            }

            if (request.ScheduledAccrualDate < request.AccrualPeriodStartDate ||
                request.ScheduledAccrualDate > request.AccrualPeriodEndDate)
            {
                return AccrualValidationResult.Fail(
                    StatusCodes.Status400BadRequest,
                    "Scheduled accrual date harus berada dalam accrual window.");
            }

            var period = await _dbContext.Set<TrxLeaveEntitlementPeriod>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == request.LeaveEntitlementPeriodId &&
                    x.IsActive &&
                    !x.IsDelete,
                    cancellationToken);

            if (period == null)
            {
                return AccrualValidationResult.Fail(
                    StatusCodes.Status404NotFound,
                    "Leave entitlement period tidak ditemukan.");
            }

            if (period.IsLocked ||
                period.PeriodStatus == LeaveValueConstants.PeriodStatus.Closed ||
                period.PeriodStatus == LeaveValueConstants.PeriodStatus.Cancelled)
            {
                return AccrualValidationResult.Fail(
                    StatusCodes.Status409Conflict,
                    "Leave entitlement period sedang dikunci, sudah ditutup, atau dibatalkan.");
            }

            if (request.AccrualPeriodStartDate < period.StartDate ||
                request.AccrualPeriodEndDate > period.EndDate)
            {
                return AccrualValidationResult.Fail(
                    StatusCodes.Status400BadRequest,
                    "Accrual window harus berada di dalam leave entitlement period.");
            }

            Guid? leaveTypeId = request.LeaveTypeId ?? period.LeaveTypeId;
            if (request.LeaveEntitlementPolicyId.HasValue)
            {
                var policy = await _dbContext.Set<MstLeaveEntitlementPolicy>()
                    .AsNoTracking()
                    .Include(x => x.LeavePolicy)
                    .FirstOrDefaultAsync(x =>
                        x.Id == request.LeaveEntitlementPolicyId.Value &&
                        x.IsActive &&
                        !x.IsDelete,
                        cancellationToken);

                if (policy == null || policy.LeavePolicy == null || policy.LeavePolicy.IsDelete)
                {
                    return AccrualValidationResult.Fail(
                        StatusCodes.Status404NotFound,
                        "Leave entitlement policy tidak ditemukan.");
                }

                leaveTypeId ??= policy.LeavePolicy.LeaveTypeId;
                if (leaveTypeId != policy.LeavePolicy.LeaveTypeId)
                {
                    return AccrualValidationResult.Fail(
                        StatusCodes.Status400BadRequest,
                        "Leave type tidak sesuai dengan entitlement policy.");
                }
            }

            if (!leaveTypeId.HasValue || leaveTypeId == Guid.Empty)
            {
                return AccrualValidationResult.Fail(
                    StatusCodes.Status400BadRequest,
                    "Leave type atau leave entitlement policy wajib dipilih.");
            }

            return AccrualValidationResult.Ok(period, leaveTypeId.Value);
        }

        private IQueryable<TrxLeaveAccrualRun> BuildRunQuery(LeaveAccrualRunQueryRequest request)
        {
            var query = _dbContext.Set<TrxLeaveAccrualRun>()
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .Include(x => x.LeaveEntitlementPeriod)
                .Include(x => x.LeaveType)
                .Include(x => x.LeaveEntitlementPolicy)
                .AsQueryable();

            if (request.StartDate.HasValue)
            {
                query = query.Where(x => x.ScheduledAccrualDate >= request.StartDate.Value);
            }
            if (request.EndDate.HasValue)
            {
                query = query.Where(x => x.ScheduledAccrualDate <= request.EndDate.Value);
            }
            if (request.LeaveEntitlementPeriodId.HasValue)
            {
                query = query.Where(x => x.LeaveEntitlementPeriodId == request.LeaveEntitlementPeriodId.Value);
            }
            if (request.LeaveTypeId.HasValue)
            {
                query = query.Where(x => x.LeaveTypeId == request.LeaveTypeId.Value);
            }
            if (request.LeaveEntitlementPolicyId.HasValue)
            {
                query = query.Where(x => x.LeaveEntitlementPolicyId == request.LeaveEntitlementPolicyId.Value);
            }
            if (request.LegalEntityId.HasValue)
            {
                query = query.Where(x => x.LegalEntityId == request.LegalEntityId.Value);
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
            if (!string.IsNullOrWhiteSpace(request.RunStatus))
            {
                query = query.Where(x => x.RunStatus == request.RunStatus.Trim());
            }
            if (!string.IsNullOrWhiteSpace(request.RunMode))
            {
                query = query.Where(x => x.RunMode == request.RunMode.Trim());
            }
            if (request.IsDryRun.HasValue)
            {
                query = query.Where(x => x.IsDryRun == request.IsDryRun.Value);
            }
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToLower();
                query = query.Where(x =>
                    x.RunNumber.ToLower().Contains(search) ||
                    (x.LeaveEntitlementPeriod != null && x.LeaveEntitlementPeriod.PeriodCode.ToLower().Contains(search)) ||
                    (x.LeaveType != null && x.LeaveType.LeaveTypeName.ToLower().Contains(search)) ||
                    (x.LeaveEntitlementPolicy != null && x.LeaveEntitlementPolicy.EntitlementPolicyName.ToLower().Contains(search)));
            }

            return query;
        }

        private static IQueryable<TrxLeaveAccrualRun> ApplyRunSort(
            IQueryable<TrxLeaveAccrualRun> query,
            string? sortBy,
            string? sortDirection)
        {
            var descending = !string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "scheduledaccrualdate" => descending
                    ? query.OrderByDescending(x => x.ScheduledAccrualDate).ThenByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.ScheduledAccrualDate).ThenBy(x => x.CreateDateTime),
                "runnumber" => descending
                    ? query.OrderByDescending(x => x.RunNumber)
                    : query.OrderBy(x => x.RunNumber),
                "runstatus" => descending
                    ? query.OrderByDescending(x => x.RunStatus).ThenByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.RunStatus).ThenBy(x => x.CreateDateTime),
                "totalposteddays" => descending
                    ? query.OrderByDescending(x => x.TotalPostedDays)
                    : query.OrderBy(x => x.TotalPostedDays),
                _ => descending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime)
            };
        }

        private static LeaveAccrualPreviewRequest MapRunToPreviewRequest(TrxLeaveAccrualRun run)
        {
            LeaveAccrualRunParameters? parameters = null;
            if (!string.IsNullOrWhiteSpace(run.ParametersJson))
            {
                try
                {
                    parameters = JsonSerializer.Deserialize<LeaveAccrualRunParameters>(run.ParametersJson, JsonOptions);
                }
                catch
                {
                    parameters = null;
                }
            }

            return new LeaveAccrualPreviewRequest
            {
                LeaveEntitlementPeriodId = run.LeaveEntitlementPeriodId,
                LeaveTypeId = run.LeaveTypeId,
                LeaveEntitlementPolicyId = run.LeaveEntitlementPolicyId,
                WorkforceProfileId = parameters?.WorkforceProfileId,
                LegalEntityId = run.LegalEntityId,
                HospitalSiteId = run.HospitalSiteId,
                OrganizationUnitId = run.OrganizationUnitId,
                DepartmentId = run.DepartmentId,
                ScheduledAccrualDate = run.ScheduledAccrualDate,
                AccrualPeriodStartDate = run.AccrualPeriodStartDate,
                AccrualPeriodEndDate = run.AccrualPeriodEndDate,
                ForceReprocess = run.ForceReprocess,
                MaximumPreviewItem = parameters?.MaximumPreviewItem ?? 250
            };
        }

        private static bool IsWithinRequestedScope(
            WfpOrganizationAssignment? assignment,
            LeaveAccrualPreviewRequest request)
        {
            if (request.LegalEntityId.HasValue && request.LegalEntityId != assignment?.LegalEntityId)
            {
                return false;
            }
            if (request.HospitalSiteId.HasValue && request.HospitalSiteId != assignment?.HospitalSiteId)
            {
                return false;
            }
            if (request.OrganizationUnitId.HasValue && request.OrganizationUnitId != assignment?.OrganizationUnitId)
            {
                return false;
            }
            if (request.DepartmentId.HasValue && request.DepartmentId != assignment?.DepartmentId)
            {
                return false;
            }
            return true;
        }

        private static LeaveAccrualCandidateWorkItem Skipped(
            MstWorkforceProfile profile,
            Guid leaveTypeId,
            string code,
            string message,
            TrxLeaveEntitlement? entitlement = null,
            WfpLeaveBalance? balance = null,
            MstLeavePolicy? leavePolicy = null,
            MstLeaveEntitlementPolicy? entitlementPolicy = null)
        {
            return new LeaveAccrualCandidateWorkItem
            {
                Response = new LeaveAccrualCandidateResponse
                {
                    WorkforceProfileId = profile.Id,
                    WorkforceProfileCode = profile.ProfileCode,
                    WorkforceDisplayName = profile.DisplayName,
                    LeaveTypeId = leaveTypeId,
                    LeavePolicyId = leavePolicy?.Id,
                    LeavePolicyCode = leavePolicy?.LeavePolicyCode,
                    LeaveEntitlementPolicyId = entitlementPolicy?.Id,
                    LeaveEntitlementPolicyCode = entitlementPolicy?.EntitlementPolicyCode,
                    LeaveEntitlementId = entitlement?.Id,
                    LeaveBalanceId = balance?.Id,
                    CurrentRemainingDays = balance?.RemainingDays ?? 0,
                    CurrentAvailableDays = balance?.AvailableDays ?? 0,
                    CalculatedAccrualDays = 0,
                    IsEligible = false,
                    ResultCode = code,
                    ResultMessage = message
                }
            };
        }

        private static decimal CalculateDefaultAccrualAmount(MstLeaveEntitlementPolicy policy)
        {
            var divisor = policy.AccrualFrequency.Trim().ToLowerInvariant() switch
            {
                "monthly" => 12m,
                "quarterly" => 4m,
                "semiannual" => 2m,
                "semi-annual" => 2m,
                "biweekly" => 26m,
                "bi-weekly" => 26m,
                "weekly" => 52m,
                "daily" => 365m,
                _ => 1m
            };

            return divisor <= 0 ? policy.AnnualEntitlementDays : policy.AnnualEntitlementDays / divisor;
        }

        private static decimal CalculateOverlapFactor(
            DateOnly joinDate,
            DateOnly? separationDate,
            DateOnly windowStart,
            DateOnly windowEnd)
        {
            var actualStart = joinDate > windowStart ? joinDate : windowStart;
            var actualEnd = separationDate.HasValue && separationDate.Value < windowEnd
                ? separationDate.Value
                : windowEnd;

            if (actualEnd < actualStart)
            {
                return 0;
            }

            var totalDays = windowEnd.DayNumber - windowStart.DayNumber + 1;
            var activeDays = actualEnd.DayNumber - actualStart.DayNumber + 1;
            return totalDays <= 0 ? 0 : Math.Clamp(activeDays / (decimal)totalDays, 0, 1);
        }

        internal static decimal RoundDays(decimal value, string? roundingMethod)
        {
            return (roundingMethod ?? string.Empty).Trim() switch
            {
                LeaveValueConstants.RoundingMethod.Up => Math.Ceiling(value),
                LeaveValueConstants.RoundingMethod.Down => Math.Floor(value),
                LeaveValueConstants.RoundingMethod.NearestHalfDay => Math.Round(value * 2, MidpointRounding.AwayFromZero) / 2,
                LeaveValueConstants.RoundingMethod.NearestDay => Math.Round(value, MidpointRounding.AwayFromZero),
                _ => Math.Round(value, 4, MidpointRounding.AwayFromZero)
            };
        }


        private void DetachTrackedEntitiesExceptRun(Guid runId)
        {
            foreach (var entry in _dbContext.ChangeTracker.Entries().ToList())
            {
                if (entry.Entity is TrxLeaveAccrualRun trackedRun && trackedRun.Id == runId)
                {
                    continue;
                }

                entry.State = EntityState.Detached;
            }
        }

        private static LeaveAccrualOptionResponse Option(string value)
        {
            return new LeaveAccrualOptionResponse { Value = value, Label = value };
        }

        private static string GenerateNumber(string prefix)
        {
            return $"{prefix}-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
        }

        private static string BuildItemIdempotencyKey(
            Guid runId,
            Guid workforceProfileId,
            Guid leaveTypeId,
            int sequence,
            DateOnly periodStart,
            DateOnly periodEnd)
        {
            return $"ACCRUAL:{runId:N}:{workforceProfileId:N}:{leaveTypeId:N}:{periodStart:yyyyMMdd}:{periodEnd:yyyyMMdd}:{sequence}";
        }

        private static string NormalizeRunMode(string? value)
        {
            var candidate = (value ?? string.Empty).Trim();
            var allowed = new[]
            {
                LeaveValueConstants.BatchRunMode.Scheduled,
                LeaveValueConstants.BatchRunMode.Manual,
                LeaveValueConstants.BatchRunMode.Reprocess,
                LeaveValueConstants.BatchRunMode.Preview
            };
            return allowed.FirstOrDefault(x => string.Equals(x, candidate, StringComparison.OrdinalIgnoreCase))
                ?? LeaveValueConstants.BatchRunMode.Manual;
        }

        private static string? NormalizeOptional(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string? AppendNote(string? current, string? addition)
        {
            if (string.IsNullOrWhiteSpace(addition))
            {
                return current;
            }
            return string.IsNullOrWhiteSpace(current)
                ? addition.Trim()
                : $"{current}\n{addition.Trim()}";
        }

        private static LeaveAccrualRunActionResponse MapAction(
            TrxLeaveAccrualRun run,
            bool isIdempotent,
            string message)
        {
            return new LeaveAccrualRunActionResponse
            {
                Id = run.Id,
                RunNumber = run.RunNumber,
                RunStatus = run.RunStatus,
                IsIdempotent = isIdempotent,
                TargetCount = run.TargetCount,
                PostedCount = run.PostedCount,
                SkippedCount = run.SkippedCount,
                FailedCount = run.FailedCount,
                TotalPostedDays = run.TotalPostedDays,
                Message = message
            };
        }

        private static string? GetUserDisplayName(ApplicationUser? user)
        {
            if (user == null)
            {
                return null;
            }
            return !string.IsNullOrWhiteSpace(user.DisplayName)
                ? user.DisplayName
                : !string.IsNullOrWhiteSpace(user.UserName)
                    ? user.UserName
                    : user.Email;
        }

        private sealed class AccrualCalculationResult
        {
            public decimal Amount { get; set; }
            public bool IsProrated { get; set; }
            public string Code { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public string? DetailJson { get; set; }

            public static AccrualCalculationResult Skip(string code, string message)
            {
                return new AccrualCalculationResult
                {
                    Amount = 0,
                    Code = code,
                    Message = message
                };
            }
        }

        private sealed class AccrualValidationResult
        {
            public bool Success { get; set; }
            public int StatusCode { get; set; }
            public string Message { get; set; } = string.Empty;
            public TrxLeaveEntitlementPeriod? Period { get; set; }
            public Guid? LeaveTypeId { get; set; }

            public static AccrualValidationResult Ok(
                TrxLeaveEntitlementPeriod period,
                Guid leaveTypeId)
            {
                return new AccrualValidationResult
                {
                    Success = true,
                    StatusCode = StatusCodes.Status200OK,
                    Period = period,
                    LeaveTypeId = leaveTypeId
                };
            }

            public static AccrualValidationResult Fail(int statusCode, string message)
            {
                return new AccrualValidationResult
                {
                    Success = false,
                    StatusCode = statusCode,
                    Message = message
                };
            }
        }
    }
}
