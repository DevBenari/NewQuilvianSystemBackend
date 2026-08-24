using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Repositories;
using System.Data;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services
{
    public class AttendanceProcessingService
    {
        private const int MaximumRangeDays = 31;
        private const int MaximumWorkforcePerRequest = 500;
        private const int MaximumProcessingItems = 5000;
        private const int DefaultCaptureBeforeMinutes = 240;
        private const int DefaultCaptureAfterMinutes = 480;
        private const string DefaultTimeZoneId = "Asia/Jakarta";

        private static readonly string[] OpenRawLogStatuses =
        {
            AttendanceValueConstants.RawLogProcessingStatus.Matched,
            AttendanceValueConstants.RawLogProcessingStatus.Processed
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly AttendanceScheduleResolverService _scheduleResolver;

        public AttendanceProcessingService(
            ApplicationDbContext dbContext,
            AttendanceScheduleResolverService scheduleResolver)
        {
            _dbContext = dbContext;
            _scheduleResolver = scheduleResolver;
        }

        public AttendanceProcessingMetadataResponse GetMetadata()
        {
            return new AttendanceProcessingMetadataResponse
            {
                MaximumRangeDays = MaximumRangeDays,
                MaximumWorkforcePerRequest = MaximumWorkforcePerRequest,
                MaximumProcessingItemPerRequest = MaximumProcessingItems,
                DefaultFilter = new AttendanceProcessingDefaultFilterResponse(),
                ProcessingModeOptions = new List<AttendanceProcessingStringOptionResponse>
                {
                    new() { Value = AttendanceValueConstants.ProcessingRunMode.SingleDate, Label = "Satu workforce dan tanggal" },
                    new() { Value = AttendanceValueConstants.ProcessingRunMode.SingleWorkforce, Label = "Satu workforce dalam rentang tanggal" },
                    new() { Value = AttendanceValueConstants.ProcessingRunMode.Batch, Label = "Pemrosesan batch" },
                    new() { Value = AttendanceValueConstants.ProcessingRunMode.Reprocess, Label = "Pemrosesan ulang" }
                },
                RunStatusOptions = new List<AttendanceProcessingStringOptionResponse>
                {
                    new() { Value = AttendanceValueConstants.ProcessingRunStatus.Pending, Label = "Menunggu" },
                    new() { Value = AttendanceValueConstants.ProcessingRunStatus.Running, Label = "Sedang diproses" },
                    new() { Value = AttendanceValueConstants.ProcessingRunStatus.Completed, Label = "Selesai" },
                    new() { Value = AttendanceValueConstants.ProcessingRunStatus.CompletedWithErrors, Label = "Selesai dengan error" },
                    new() { Value = AttendanceValueConstants.ProcessingRunStatus.Failed, Label = "Gagal" },
                    new() { Value = AttendanceValueConstants.ProcessingRunStatus.Cancelled, Label = "Dibatalkan" }
                },
                TriggerSourceOptions = new List<AttendanceProcessingStringOptionResponse>
                {
                    new() { Value = AttendanceValueConstants.ProcessingTriggerSource.Api, Label = "API" },
                    new() { Value = AttendanceValueConstants.ProcessingTriggerSource.Manual, Label = "Manual" },
                    new() { Value = AttendanceValueConstants.ProcessingTriggerSource.Scheduler, Label = "Scheduler" },
                    new() { Value = AttendanceValueConstants.ProcessingTriggerSource.System, Label = "Sistem" }
                },
                AttendanceStatusOptions = new List<AttendanceProcessingStringOptionResponse>
                {
                    new() { Value = AttendanceValueConstants.AttendanceStatus.Present, Label = "Hadir" },
                    new() { Value = AttendanceValueConstants.AttendanceStatus.Absent, Label = "Tidak hadir" },
                    new() { Value = AttendanceValueConstants.AttendanceStatus.Late, Label = "Terlambat" },
                    new() { Value = AttendanceValueConstants.AttendanceStatus.EarlyLeave, Label = "Pulang awal" },
                    new() { Value = AttendanceValueConstants.AttendanceStatus.Incomplete, Label = "Data belum lengkap" },
                    new() { Value = AttendanceValueConstants.AttendanceStatus.Holiday, Label = "Hari libur" },
                    new() { Value = AttendanceValueConstants.AttendanceStatus.RestDay, Label = "Hari istirahat" },
                    new() { Value = AttendanceValueConstants.AttendanceStatus.Remote, Label = "Kerja jarak jauh" },
                    new() { Value = AttendanceValueConstants.AttendanceStatus.BusinessTrip, Label = "Perjalanan dinas" }
                },
                SortOptions = new List<AttendanceProcessingSortOptionResponse>
                {
                    new() { Value = "startedAt", Label = "Waktu mulai" },
                    new() { Value = "completedAt", Label = "Waktu selesai" },
                    new() { Value = "runNumber", Label = "Nomor proses" },
                    new() { Value = "runStatus", Label = "Status proses" },
                    new() { Value = "targetCount", Label = "Jumlah target" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };
        }

        public async Task<AttendanceProcessingSummaryResponse> GetSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            var runs = _dbContext.Set<HrdAttendanceProcessingRun>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            return new AttendanceProcessingSummaryResponse
            {
                TotalRun = await runs.CountAsync(cancellationToken),
                RunningRun = await runs.CountAsync(x => x.RunStatus == AttendanceValueConstants.ProcessingRunStatus.Running, cancellationToken),
                CompletedRun = await runs.CountAsync(x => x.RunStatus == AttendanceValueConstants.ProcessingRunStatus.Completed, cancellationToken),
                CompletedWithErrorsRun = await runs.CountAsync(x => x.RunStatus == AttendanceValueConstants.ProcessingRunStatus.CompletedWithErrors, cancellationToken),
                FailedRun = await runs.CountAsync(x => x.RunStatus == AttendanceValueConstants.ProcessingRunStatus.Failed, cancellationToken),
                CancelledRun = await runs.CountAsync(x => x.RunStatus == AttendanceValueConstants.ProcessingRunStatus.Cancelled, cancellationToken),
                TotalTarget = await runs.SumAsync(x => (int?)x.TargetCount, cancellationToken) ?? 0,
                TotalSuccess = await runs.SumAsync(x => (int?)x.SuccessCount, cancellationToken) ?? 0,
                TotalFailed = await runs.SumAsync(x => (int?)x.FailedCount, cancellationToken) ?? 0,
                TotalSkipped = await runs.SumAsync(x => (int?)x.SkippedCount, cancellationToken) ?? 0,
                ProcessedToday = await _dbContext.Set<HrdAttendanceDaily>()
                    .AsNoTracking()
                    .CountAsync(x =>
                        !x.IsDelete &&
                        x.ProcessedAt >= today &&
                        x.ProcessedAt < tomorrow &&
                        x.ProcessingStatus == AttendanceValueConstants.AttendanceProcessingStatus.Processed,
                        cancellationToken),
                ErrorToday = await _dbContext.Set<HrdAttendanceDaily>()
                    .AsNoTracking()
                    .CountAsync(x =>
                        !x.IsDelete &&
                        x.ProcessedAt >= today &&
                        x.ProcessedAt < tomorrow &&
                        x.ProcessingStatus == AttendanceValueConstants.AttendanceProcessingStatus.Error,
                        cancellationToken)
            };
        }

        public async Task<AttendanceProcessingRunPagedResponse> GetRunsAsync(
            AttendanceProcessingRunQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, 100);

            var query = BuildRunQuery(request);
            var totalData = await query.CountAsync(cancellationToken);

            var items = await ApplyRunSorting(query, request.SortBy, request.SortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new AttendanceProcessingRunResponse
                {
                    Id = x.Id,
                    RunNumber = x.RunNumber,
                    ProcessingMode = x.ProcessingMode,
                    RunStatus = x.RunStatus,
                    TriggerSource = x.TriggerSource,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    TargetWorkforceProfileId = x.TargetWorkforceProfileId,
                    TargetWorkforceProfileCode = x.TargetWorkforceProfile != null ? x.TargetWorkforceProfile.ProfileCode : null,
                    TargetWorkforceDisplayName = x.TargetWorkforceProfile != null ? x.TargetWorkforceProfile.DisplayName : null,
                    HospitalSiteId = x.HospitalSiteId,
                    HospitalSiteName = x.HospitalSite != null ? x.HospitalSite.SiteName : null,
                    OrganizationUnitId = x.OrganizationUnitId,
                    OrganizationUnitName = x.OrganizationUnit != null ? x.OrganizationUnit.UnitName : null,
                    DepartmentId = x.DepartmentId,
                    DepartmentName = x.Department != null ? x.Department.DepartmentName : null,
                    ProcessingVersion = x.ProcessingVersion,
                    TargetCount = x.TargetCount,
                    SuccessCount = x.SuccessCount,
                    FailedCount = x.FailedCount,
                    SkippedCount = x.SkippedCount,
                    CompletionPercentage = x.TargetCount <= 0
                        ? 0
                        : Math.Round((x.SuccessCount + x.FailedCount + x.SkippedCount) * 100m / x.TargetCount, 2),
                    StartedAt = x.StartedAt,
                    CompletedAt = x.CompletedAt,
                    CancelledAt = x.CancelledAt,
                    TriggeredByUserId = x.TriggeredByUserId,
                    TriggeredByUserName = x.TriggeredByUser != null
                        ? x.TriggeredByUser.DisplayName ?? x.TriggeredByUser.UserName ?? x.TriggeredByUser.Email ?? x.TriggeredByUser.UserCode
                        : null,
                    CorrelationId = x.CorrelationId,
                    ErrorSummary = x.ErrorSummary,
                    Notes = x.Notes,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            return new AttendanceProcessingRunPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task<AttendanceProcessingRunDetailResponse?> GetRunDetailAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<HrdAttendanceProcessingRun>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new AttendanceProcessingRunDetailResponse
                {
                    Id = x.Id,
                    RunNumber = x.RunNumber,
                    ProcessingMode = x.ProcessingMode,
                    RunStatus = x.RunStatus,
                    TriggerSource = x.TriggerSource,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    TargetWorkforceProfileId = x.TargetWorkforceProfileId,
                    TargetWorkforceProfileCode = x.TargetWorkforceProfile != null ? x.TargetWorkforceProfile.ProfileCode : null,
                    TargetWorkforceDisplayName = x.TargetWorkforceProfile != null ? x.TargetWorkforceProfile.DisplayName : null,
                    HospitalSiteId = x.HospitalSiteId,
                    HospitalSiteName = x.HospitalSite != null ? x.HospitalSite.SiteName : null,
                    OrganizationUnitId = x.OrganizationUnitId,
                    OrganizationUnitName = x.OrganizationUnit != null ? x.OrganizationUnit.UnitName : null,
                    DepartmentId = x.DepartmentId,
                    DepartmentName = x.Department != null ? x.Department.DepartmentName : null,
                    ProcessingVersion = x.ProcessingVersion,
                    TargetCount = x.TargetCount,
                    SuccessCount = x.SuccessCount,
                    FailedCount = x.FailedCount,
                    SkippedCount = x.SkippedCount,
                    CompletionPercentage = x.TargetCount <= 0
                        ? 0
                        : Math.Round((x.SuccessCount + x.FailedCount + x.SkippedCount) * 100m / x.TargetCount, 2),
                    StartedAt = x.StartedAt,
                    CompletedAt = x.CompletedAt,
                    CancelledAt = x.CancelledAt,
                    TriggeredByUserId = x.TriggeredByUserId,
                    TriggeredByUserName = x.TriggeredByUser != null
                        ? x.TriggeredByUser.DisplayName ?? x.TriggeredByUser.UserName ?? x.TriggeredByUser.Email ?? x.TriggeredByUser.UserCode
                        : null,
                    CancelledByUserId = x.CancelledByUserId,
                    CancelledByUserName = x.CancelledByUser != null
                        ? x.CancelledByUser.DisplayName ?? x.CancelledByUser.UserName ?? x.CancelledByUser.Email ?? x.CancelledByUser.UserCode
                        : null,
                    CorrelationId = x.CorrelationId,
                    ParametersJson = x.ParametersJson,
                    ErrorSummary = x.ErrorSummary,
                    Notes = x.Notes,
                    CreateDateTime = x.CreateDateTime,
                    UpdateDateTime = x.UpdateDateTime
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<AttendanceProcessingServiceResult<AttendanceProcessingExecutionResponse>> ProcessSingleAsync(
            ProcessAttendanceSingleRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (request.WorkforceProfileId == Guid.Empty)
            {
                return AttendanceProcessingServiceResult<AttendanceProcessingExecutionResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Workforce profile wajib dipilih.");
            }

            var triggerSource = NormalizeTriggerSource(request.TriggerSource);
            if (triggerSource == null)
            {
                return AttendanceProcessingServiceResult<AttendanceProcessingExecutionResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Trigger source tidak valid.");
            }

            var existingCorrelation = await FindExistingRunByCorrelationAsync(request.CorrelationId, cancellationToken);
            if (existingCorrelation != null)
            {
                return AttendanceProcessingServiceResult<AttendanceProcessingExecutionResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Correlation id sudah digunakan oleh processing run {existingCorrelation.RunNumber}.");
            }

            var workforce = await ResolveWorkforceAsync(request.WorkforceProfileId, request.WorkDate, cancellationToken);
            if (workforce == null)
            {
                return AttendanceProcessingServiceResult<AttendanceProcessingExecutionResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan, tidak aktif, atau belum mempunyai user account aktif.");
            }

            var run = await CreateRunAsync(
                request.ForceReprocess
                    ? AttendanceValueConstants.ProcessingRunMode.Reprocess
                    : AttendanceValueConstants.ProcessingRunMode.SingleDate,
                triggerSource,
                request.WorkDate,
                request.WorkDate,
                request.WorkforceProfileId,
                workforce.HospitalSiteId,
                workforce.OrganizationUnitId,
                workforce.DepartmentId,
                1,
                actorUserId,
                request.CorrelationId,
                JsonSerializer.Serialize(new
                {
                    request.WorkforceProfileId,
                    request.WorkDate,
                    request.ForceReprocess
                }),
                request.Notes,
                cancellationToken);

            var item = await ProcessOneWithTransactionAsync(
                workforce,
                request.WorkDate,
                request.ForceReprocess,
                actorUserId,
                cancellationToken);

            await CompleteRunAsync(run, new[] { item }, cancellationToken);

            return AttendanceProcessingServiceResult<AttendanceProcessingExecutionResponse>.Ok(
                MapExecutionResponse(run, new List<AttendanceProcessingItemResponse> { item }),
                item.Success
                    ? "Attendance berhasil diproses."
                    : item.IsSkipped
                        ? "Attendance dilewati karena tidak memenuhi syarat pemrosesan."
                        : "Attendance gagal diproses.");
        }

        public async Task<AttendanceProcessingServiceResult<AttendanceProcessingExecutionResponse>> ProcessRangeAsync(
            ProcessAttendanceRangeRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var validation = ValidateRange(request.StartDate, request.EndDate);
            if (validation != null)
            {
                return AttendanceProcessingServiceResult<AttendanceProcessingExecutionResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation);
            }

            var triggerSource = NormalizeTriggerSource(request.TriggerSource);
            if (triggerSource == null)
            {
                return AttendanceProcessingServiceResult<AttendanceProcessingExecutionResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Trigger source tidak valid.");
            }

            var existingCorrelation = await FindExistingRunByCorrelationAsync(request.CorrelationId, cancellationToken);
            if (existingCorrelation != null)
            {
                return AttendanceProcessingServiceResult<AttendanceProcessingExecutionResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Correlation id sudah digunakan oleh processing run {existingCorrelation.RunNumber}.");
            }

            var workforces = await ResolveTargetWorkforcesAsync(request, cancellationToken);
            if (workforces.Count == 0)
            {
                return AttendanceProcessingServiceResult<AttendanceProcessingExecutionResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Tidak ditemukan workforce aktif yang memenuhi filter pemrosesan.");
            }

            if (workforces.Count > MaximumWorkforcePerRequest)
            {
                return AttendanceProcessingServiceResult<AttendanceProcessingExecutionResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Jumlah workforce maksimal per request adalah {MaximumWorkforcePerRequest}. Persempit filter pemrosesan.");
            }

            var dayCount = request.EndDate.DayNumber - request.StartDate.DayNumber + 1;
            var targetCount = workforces.Count * dayCount;
            if (targetCount > MaximumProcessingItems)
            {
                return AttendanceProcessingServiceResult<AttendanceProcessingExecutionResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Jumlah item pemrosesan maksimal adalah {MaximumProcessingItems}. Persempit rentang tanggal atau filter workforce.");
            }

            var mode = request.ForceReprocess
                ? AttendanceValueConstants.ProcessingRunMode.Reprocess
                : request.WorkforceProfileId.HasValue
                    ? AttendanceValueConstants.ProcessingRunMode.SingleWorkforce
                    : AttendanceValueConstants.ProcessingRunMode.Batch;

            var run = await CreateRunAsync(
                mode,
                triggerSource,
                request.StartDate,
                request.EndDate,
                request.WorkforceProfileId,
                request.HospitalSiteId,
                request.OrganizationUnitId,
                request.DepartmentId,
                targetCount,
                actorUserId,
                request.CorrelationId,
                JsonSerializer.Serialize(new
                {
                    request.StartDate,
                    request.EndDate,
                    request.WorkforceProfileId,
                    request.HospitalSiteId,
                    request.OrganizationUnitId,
                    request.DepartmentId,
                    request.ForceReprocess
                }),
                request.Notes,
                cancellationToken);

            var items = new List<AttendanceProcessingItemResponse>(targetCount);
            try
            {
                foreach (var workforceTarget in workforces)
                {
                    for (var date = request.StartDate; date <= request.EndDate; date = date.AddDays(1))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var workforce = await ResolveWorkforceAsync(
                            workforceTarget.WorkforceProfileId,
                            date,
                            cancellationToken);

                        if (workforce == null)
                        {
                            items.Add(new AttendanceProcessingItemResponse
                            {
                                WorkforceProfileId = workforceTarget.WorkforceProfileId,
                                WorkforceProfileCode = workforceTarget.ProfileCode,
                                WorkforceDisplayName = workforceTarget.DisplayName,
                                WorkDate = date,
                                Success = false,
                                AttendanceStatus = AttendanceValueConstants.AttendanceStatus.Unprocessed,
                                ProcessingStatus = AttendanceValueConstants.AttendanceProcessingStatus.Error,
                                ScheduleSource = AttendanceValueConstants.ScheduleSource.Unresolved,
                                IsPayrollEligible = false,
                                PayrollInputStatus = AttendanceValueConstants.PayrollInputStatus.Blocked,
                                Message = "Workforce atau user account tidak aktif pada tanggal pemrosesan."
                            });
                            continue;
                        }

                        items.Add(await ProcessOneWithTransactionAsync(
                            workforce,
                            date,
                            request.ForceReprocess,
                            actorUserId,
                            cancellationToken));
                    }
                }
            }
            catch (OperationCanceledException)
            {
                run.RunStatus = AttendanceValueConstants.ProcessingRunStatus.Cancelled;
                run.CancelledAt = DateTime.UtcNow;
                run.CancelledByUserId = actorUserId;
                run.SuccessCount = items.Count(x => x.Success && !x.IsSkipped);
                run.FailedCount = items.Count(x => !x.Success && !x.IsSkipped);
                run.SkippedCount = items.Count(x => x.IsSkipped);
                run.UpdateDateTime = DateTime.UtcNow;
                run.UpdateBy = actorUserId;
                await _dbContext.SaveChangesAsync(CancellationToken.None);
                throw;
            }

            await CompleteRunAsync(run, items, cancellationToken);

            return AttendanceProcessingServiceResult<AttendanceProcessingExecutionResponse>.Ok(
                MapExecutionResponse(run, items),
                run.RunStatus == AttendanceValueConstants.ProcessingRunStatus.Completed
                    ? "Seluruh attendance berhasil diproses."
                    : "Pemrosesan attendance selesai dengan sebagian item gagal atau dilewati.");
        }

        public async Task<AttendanceProcessingServiceResult<AttendanceProcessingExecutionResponse>> ReprocessDailyAsync(
            Guid attendanceDailyId,
            ReprocessAttendanceDailyRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var existing = await _dbContext.Set<HrdAttendanceDaily>()
                .AsNoTracking()
                .Where(x => x.Id == attendanceDailyId && !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.WorkforceProfileId,
                    x.AttendanceDate
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (existing == null)
            {
                return AttendanceProcessingServiceResult<AttendanceProcessingExecutionResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Attendance daily tidak ditemukan.");
            }

            if (!existing.WorkforceProfileId.HasValue)
            {
                return AttendanceProcessingServiceResult<AttendanceProcessingExecutionResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Attendance daily belum terhubung ke workforce profile sehingga tidak dapat diproses ulang.");
            }

            return await ProcessSingleAsync(
                new ProcessAttendanceSingleRequest
                {
                    WorkforceProfileId = existing.WorkforceProfileId.Value,
                    WorkDate = existing.AttendanceDate,
                    ForceReprocess = true,
                    TriggerSource = AttendanceValueConstants.ProcessingTriggerSource.Api,
                    CorrelationId = request.CorrelationId,
                    Notes = request.Reason
                },
                actorUserId,
                cancellationToken);
        }

        private async Task<AttendanceProcessingItemResponse> ProcessOneWithTransactionAsync(
            WorkforceRuntime workforce,
            DateOnly workDate,
            bool forceReprocess,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                var result = await ProcessOneCoreAsync(
                    workforce,
                    workDate,
                    forceReprocess,
                    actorUserId,
                    cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                DetachProcessingItemEntries();
                return result;
            }
            catch (DbUpdateConcurrencyException exception)
            {
                var concurrencyConflict = DescribeConcurrencyConflict(exception);
                await transaction.RollbackAsync(cancellationToken);
                DetachChangedEntries();

                return new AttendanceProcessingItemResponse
                {
                    WorkforceProfileId = workforce.WorkforceProfileId,
                    WorkforceProfileCode = workforce.ProfileCode,
                    WorkforceDisplayName = workforce.DisplayName,
                    WorkDate = workDate,
                    Success = false,
                    IsSkipped = false,
                    AttendanceStatus = AttendanceValueConstants.AttendanceStatus.Unprocessed,
                    ProcessingStatus = AttendanceValueConstants.AttendanceProcessingStatus.Error,
                    ScheduleSource = AttendanceValueConstants.ScheduleSource.Unresolved,
                    Message = LimitMessage(concurrencyConflict, 1000),
                    PayrollInputStatus = AttendanceValueConstants.PayrollInputStatus.Blocked,
                    IsPayrollEligible = false
                };
            }
            catch (Exception exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                DetachChangedEntries();

                return new AttendanceProcessingItemResponse
                {
                    WorkforceProfileId = workforce.WorkforceProfileId,
                    WorkforceProfileCode = workforce.ProfileCode,
                    WorkforceDisplayName = workforce.DisplayName,
                    WorkDate = workDate,
                    Success = false,
                    IsSkipped = false,
                    AttendanceStatus = AttendanceValueConstants.AttendanceStatus.Unprocessed,
                    ProcessingStatus = AttendanceValueConstants.AttendanceProcessingStatus.Error,
                    ScheduleSource = AttendanceValueConstants.ScheduleSource.Unresolved,
                    Message = LimitMessage(exception.GetBaseException().Message, 1000),
                    PayrollInputStatus = AttendanceValueConstants.PayrollInputStatus.Blocked,
                    IsPayrollEligible = false
                };
            }
        }

        private async Task<AttendanceProcessingItemResponse> ProcessOneCoreAsync(
            WorkforceRuntime workforce,
            DateOnly workDate,
            bool forceReprocess,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            IQueryable<HrdAttendanceDaily> existingDailyQuery = _dbContext.Set<HrdAttendanceDaily>()
                .Include(x => x.Exceptions.Where(y => !y.IsDelete && y.IsAutoDetected));

            if (!forceReprocess)
            {
                existingDailyQuery = existingDailyQuery
                    .Include(x => x.Segments.Where(y => !y.IsDelete));
            }

            var existingDaily = await existingDailyQuery
                .FirstOrDefaultAsync(x =>
                    x.WorkforceProfileId == workforce.WorkforceProfileId &&
                    x.AttendanceDate == workDate &&
                    !x.IsDelete,
                    cancellationToken);

            if (existingDaily != null && !forceReprocess &&
                existingDaily.ProcessingStatus == AttendanceValueConstants.AttendanceProcessingStatus.Processed)
            {
                return MapSkipped(existingDaily, workforce, "Attendance sudah pernah diproses. Gunakan endpoint reprocess untuk menghitung ulang.");
            }

            if (existingDaily != null && existingDaily.IsLocked)
            {
                return MapSkipped(existingDaily, workforce, "Attendance sudah dikunci dan tidak dapat diproses ulang.");
            }

            if (existingDaily != null && existingDaily.IsCorrected)
            {
                return MapSkipped(existingDaily, workforce, "Attendance sudah dikoreksi. Gunakan alur attendance correction agar hasil koreksi tidak tertimpa processor.");
            }

            if (existingDaily != null &&
                existingDaily.PayrollInputStatus == AttendanceValueConstants.PayrollInputStatus.Processed)
            {
                return MapSkipped(existingDaily, workforce, "Attendance sudah diproses payroll dan tidak dapat dihitung ulang.");
            }

            var scheduleResult = await _scheduleResolver.ResolveAsync(
                workforce.WorkforceProfileId,
                workDate,
                cancellationToken);

            if (!scheduleResult.Success || scheduleResult.Data == null)
            {
                throw new InvalidOperationException(scheduleResult.Message);
            }

            var schedule = scheduleResult.Data;
            var policy = await ResolvePolicyRuntimeAsync(schedule.AttendancePolicyId, cancellationToken);
            var captureWindow = BuildCaptureWindow(schedule, policy, workDate);

            if (existingDaily != null && forceReprocess)
            {
                var previouslyLinked = await _dbContext.Set<HrdAttendanceRawLog>()
                    .Where(x =>
                        x.ProcessedAttendanceDailyId == existingDaily.Id &&
                        !x.IsDelete)
                    .ToListAsync(cancellationToken);

                foreach (var rawLog in previouslyLinked)
                {
                    rawLog.ProcessedAttendanceDailyId = null;
                    rawLog.ProcessedAt = null;
                    rawLog.ProcessingStatus = AttendanceValueConstants.RawLogProcessingStatus.Matched;
                    rawLog.ProcessingMessage = "Raw log dilepas dari attendance daily untuk proses ulang.";
                    rawLog.UpdateDateTime = DateTime.UtcNow;
                    rawLog.UpdateBy = actorUserId;
                }
            }

            var rawLogs = await _dbContext.Set<HrdAttendanceRawLog>()
                .Include(x => x.AttendanceLocation)
                .Where(x =>
                    x.WorkforceProfileId == workforce.WorkforceProfileId &&
                    x.EventAt >= captureWindow.StartAt &&
                    x.EventAt <= captureWindow.EndAt &&
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    OpenRawLogStatuses.Contains(x.ProcessingStatus) &&
                    (x.ProcessedAttendanceDailyId == null ||
                     (existingDaily != null && x.ProcessedAttendanceDailyId == existingDaily.Id)))
                .OrderBy(x => x.EventAt)
                .ThenBy(x => x.ReceivedAt)
                .ToListAsync(cancellationToken);

            var daily = existingDaily ?? CreateDaily(workforce, workDate, actorUserId);
            var isCreated = existingDaily == null;
            if (isCreated)
            {
                _dbContext.Set<HrdAttendanceDaily>().Add(daily);
            }

            daily.UserId = workforce.UserId;
            daily.WorkforceProfileId = workforce.WorkforceProfileId;
            daily.EmployeeId = workforce.EmployeeId;
            daily.DoctorId = workforce.DoctorId;
            daily.UserType = workforce.UserType;
            daily.OrganizationAssignmentId = workforce.OrganizationAssignmentId;
            daily.HospitalSiteId = schedule.HospitalSiteId ?? workforce.HospitalSiteId;
            daily.OrganizationUnitId = schedule.OrganizationUnitId ?? workforce.OrganizationUnitId;
            daily.DepartmentId = schedule.DepartmentId ?? workforce.DepartmentId;
            daily.PositionId = workforce.PositionId;
            daily.WorkLocationId = schedule.WorkLocationId ?? workforce.WorkLocationId;
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
            daily.SourceLogCount = rawLogs.Count;
            daily.ProcessingStatus = AttendanceValueConstants.AttendanceProcessingStatus.Processing;
            daily.ProcessingMessage = null;
            daily.IsActive = true;
            daily.UpdateDateTime = DateTime.UtcNow;
            daily.UpdateBy = actorUserId;

            await SoftDeleteProcessorSegmentsAsync(daily.Id, actorUserId, cancellationToken);

            var punchResult = BuildPunchResult(rawLogs, schedule, policy);
            ApplyPunchResult(daily, punchResult, schedule, policy);

            var desiredExceptions = BuildExceptions(
                daily,
                rawLogs,
                schedule,
                policy,
                punchResult);

            SynchronizeExceptions(daily, desiredExceptions, actorUserId);
            CreateSegments(daily, schedule, punchResult, actorUserId);

            daily.ExceptionCount = desiredExceptions.Count;
            daily.IsPayrollEligible = desiredExceptions.All(x => !x.IsPayrollBlocking);
            daily.PayrollInputStatus = daily.IsPayrollEligible
                ? AttendanceValueConstants.PayrollInputStatus.Ready
                : AttendanceValueConstants.PayrollInputStatus.Blocked;
            daily.ProcessingStatus = schedule.HasBlockingConflict || !schedule.IsResolved
                ? AttendanceValueConstants.AttendanceProcessingStatus.Error
                : AttendanceValueConstants.AttendanceProcessingStatus.Processed;
            daily.ProcessedAt = DateTime.UtcNow;
            daily.ProcessingVersion = Math.Max(1, daily.ProcessingVersion + (forceReprocess ? 1 : 0));
            daily.ProcessingMessage = BuildProcessingMessage(schedule, punchResult, desiredExceptions);

            foreach (var rawLog in rawLogs)
            {
                rawLog.ProcessedAttendanceDailyId = daily.Id;
                rawLog.ProcessedAt = DateTime.UtcNow;
                rawLog.ProcessingStatus = AttendanceValueConstants.RawLogProcessingStatus.Processed;
                rawLog.ProcessingMessage = $"Diproses ke attendance daily {daily.Id}.";
                rawLog.UpdateDateTime = DateTime.UtcNow;
                rawLog.UpdateBy = actorUserId;
            }

            return new AttendanceProcessingItemResponse
            {
                WorkforceProfileId = workforce.WorkforceProfileId,
                WorkforceProfileCode = workforce.ProfileCode,
                WorkforceDisplayName = workforce.DisplayName,
                WorkDate = workDate,
                Success = daily.ProcessingStatus != AttendanceValueConstants.AttendanceProcessingStatus.Error,
                IsSkipped = false,
                IsCreated = isCreated,
                IsReprocessed = forceReprocess,
                AttendanceDailyId = daily.Id,
                AttendanceStatus = daily.AttendanceStatus,
                ProcessingStatus = daily.ProcessingStatus,
                ScheduleSource = daily.ScheduleSource,
                ScheduledCheckInAt = daily.ScheduledCheckInAt,
                ScheduledCheckOutAt = daily.ScheduledCheckOutAt,
                FirstCheckInAt = daily.FirstCheckInAt,
                LastCheckOutAt = daily.LastCheckOutAt,
                RawLogCount = daily.SourceLogCount,
                SegmentCount = daily.Segments.Count(x => !x.IsDelete),
                ExceptionCount = daily.ExceptionCount,
                ActualWorkMinutes = daily.ActualWorkMinutes,
                LateMinutes = daily.LateMinutes,
                EarlyLeaveMinutes = daily.EarlyLeaveMinutes,
                OvertimeMinutes = daily.OvertimeMinutes,
                IsPayrollEligible = daily.IsPayrollEligible,
                PayrollInputStatus = daily.PayrollInputStatus,
                Message = daily.ProcessingMessage ?? "Attendance selesai diproses.",
                ExceptionCodes = desiredExceptions.Select(x => x.ExceptionCode).ToList()
            };
        }

        private static HrdAttendanceDaily CreateDaily(
            WorkforceRuntime workforce,
            DateOnly workDate,
            Guid actorUserId)
        {
            return new HrdAttendanceDaily
            {
                Id = Guid.NewGuid(),
                UserId = workforce.UserId,
                WorkforceProfileId = workforce.WorkforceProfileId,
                EmployeeId = workforce.EmployeeId,
                DoctorId = workforce.DoctorId,
                UserType = workforce.UserType,
                AttendanceDate = workDate,
                AttendanceStatus = AttendanceValueConstants.AttendanceStatus.Unprocessed,
                ProcessingStatus = AttendanceValueConstants.AttendanceProcessingStatus.Pending,
                PayrollInputStatus = AttendanceValueConstants.PayrollInputStatus.Pending,
                ScheduleSource = AttendanceValueConstants.ScheduleSource.Unresolved,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false,
                IsActive = true
            };
        }

        private static AttendanceProcessingItemResponse MapSkipped(
            HrdAttendanceDaily daily,
            WorkforceRuntime workforce,
            string message)
        {
            return new AttendanceProcessingItemResponse
            {
                WorkforceProfileId = workforce.WorkforceProfileId,
                WorkforceProfileCode = workforce.ProfileCode,
                WorkforceDisplayName = workforce.DisplayName,
                WorkDate = daily.AttendanceDate,
                Success = true,
                IsSkipped = true,
                AttendanceDailyId = daily.Id,
                AttendanceStatus = daily.AttendanceStatus,
                ProcessingStatus = daily.ProcessingStatus,
                ScheduleSource = daily.ScheduleSource,
                ScheduledCheckInAt = daily.ScheduledCheckInAt,
                ScheduledCheckOutAt = daily.ScheduledCheckOutAt,
                FirstCheckInAt = daily.FirstCheckInAt,
                LastCheckOutAt = daily.LastCheckOutAt,
                RawLogCount = daily.SourceLogCount,
                SegmentCount = daily.Segments.Count(x => !x.IsDelete),
                ExceptionCount = daily.ExceptionCount,
                ActualWorkMinutes = daily.ActualWorkMinutes,
                LateMinutes = daily.LateMinutes,
                EarlyLeaveMinutes = daily.EarlyLeaveMinutes,
                OvertimeMinutes = daily.OvertimeMinutes,
                IsPayrollEligible = daily.IsPayrollEligible,
                PayrollInputStatus = daily.PayrollInputStatus,
                Message = message
            };
        }

        private static PunchResult BuildPunchResult(
            List<HrdAttendanceRawLog> rawLogs,
            AttendanceScheduleResolutionResponse schedule,
            PolicyRuntime policy)
        {
            var checkIns = rawLogs
                .Where(x => string.Equals(x.EventType, AttendanceValueConstants.RawLogEventType.CheckIn, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.EventAt)
                .ToList();

            var checkOuts = rawLogs
                .Where(x => string.Equals(x.EventType, AttendanceValueConstants.RawLogEventType.CheckOut, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.EventAt)
                .ToList();

            var firstCheckIn = checkIns.FirstOrDefault();
            var lastCheckOut = checkOuts
                .Where(x => firstCheckIn == null || x.EventAt > firstCheckIn.EventAt)
                .LastOrDefault();

            var effectiveStart = firstCheckIn?.EventAt;
            var effectiveEnd = lastCheckOut?.EventAt;

            if (!policy.RequireCheckIn && !effectiveStart.HasValue)
            {
                effectiveStart = schedule.ScheduledStartAt;
            }

            if (!policy.RequireCheckOut && !effectiveEnd.HasValue)
            {
                effectiveEnd = schedule.ScheduledEndAt;
            }

            var breakPairs = BuildBreakPairs(rawLogs);
            var breakMinutes = breakPairs.Sum(x => x.Minutes);
            var grossMinutes = effectiveStart.HasValue && effectiveEnd.HasValue && effectiveEnd > effectiveStart
                ? (int)Math.Floor((effectiveEnd.Value - effectiveStart.Value).TotalMinutes)
                : 0;
            var actualWorkMinutes = Math.Max(0, grossMinutes - breakMinutes);

            return new PunchResult
            {
                FirstCheckIn = firstCheckIn,
                LastCheckOut = lastCheckOut,
                EffectiveStartAt = effectiveStart,
                EffectiveEndAt = effectiveEnd,
                BreakPairs = breakPairs,
                BreakMinutes = breakMinutes,
                ActualWorkMinutes = actualWorkMinutes,
                CheckInCount = checkIns.Count,
                CheckOutCount = checkOuts.Count
            };
        }

        private static List<BreakPair> BuildBreakPairs(List<HrdAttendanceRawLog> rawLogs)
        {
            var events = rawLogs
                .Where(x =>
                    string.Equals(x.EventType, AttendanceValueConstants.RawLogEventType.BreakStart, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(x.EventType, AttendanceValueConstants.RawLogEventType.BreakEnd, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.EventAt)
                .ToList();

            var pairs = new List<BreakPair>();
            HrdAttendanceRawLog? open = null;
            foreach (var current in events)
            {
                if (string.Equals(current.EventType, AttendanceValueConstants.RawLogEventType.BreakStart, StringComparison.OrdinalIgnoreCase))
                {
                    open = current;
                    continue;
                }

                if (open != null && current.EventAt > open.EventAt)
                {
                    pairs.Add(new BreakPair
                    {
                        Start = open,
                        End = current,
                        Minutes = Math.Max(0, (int)Math.Floor((current.EventAt - open.EventAt).TotalMinutes))
                    });
                    open = null;
                }
            }

            return pairs;
        }

        private static void ApplyPunchResult(
            HrdAttendanceDaily daily,
            PunchResult punch,
            AttendanceScheduleResolutionResponse schedule,
            PolicyRuntime policy)
        {
            daily.FirstCheckInAt = punch.FirstCheckIn?.EventAt;
            daily.LastCheckOutAt = punch.LastCheckOut?.EventAt;
            daily.BreakMinutes = punch.BreakMinutes;
            daily.ActualWorkMinutes = punch.ActualWorkMinutes;
            daily.PayableWorkMinutes = policy.MaximumWorkMinutes > 0
                ? Math.Min(punch.ActualWorkMinutes, policy.MaximumWorkMinutes)
                : punch.ActualWorkMinutes;

            daily.LateMinutes = 0;
            if (punch.FirstCheckIn != null && schedule.ScheduledStartAt.HasValue)
            {
                var lateThreshold = schedule.ScheduledStartAt.Value.AddMinutes(schedule.LateCheckInGraceMinutes);
                if (punch.FirstCheckIn.EventAt > lateThreshold)
                {
                    daily.LateMinutes = Math.Max(0, (int)Math.Floor((punch.FirstCheckIn.EventAt - schedule.ScheduledStartAt.Value).TotalMinutes));
                }
            }

            daily.EarlyLeaveMinutes = 0;
            if (punch.LastCheckOut != null && schedule.ScheduledEndAt.HasValue)
            {
                var earlyThreshold = schedule.ScheduledEndAt.Value.AddMinutes(-schedule.EarlyCheckOutGraceMinutes);
                if (punch.LastCheckOut.EventAt < earlyThreshold)
                {
                    daily.EarlyLeaveMinutes = Math.Max(0, (int)Math.Floor((schedule.ScheduledEndAt.Value - punch.LastCheckOut.EventAt).TotalMinutes));
                }
            }

            daily.OvertimeMinutes = policy.IsOvertimeEnabled
                ? Math.Max(0, daily.ActualWorkMinutes - daily.ScheduledWorkMinutes - Math.Max(0, policy.OvertimeThresholdMinutes))
                : 0;

            daily.NightWorkMinutes = CalculateNightWorkMinutes(
                punch.EffectiveStartAt,
                punch.EffectiveEndAt,
                schedule.TimeZoneId);

            daily.IsPresent = punch.FirstCheckIn != null || punch.LastCheckOut != null;
            daily.IsAbsent = false;
            daily.IsLate = daily.LateMinutes > 0;
            daily.IsEarlyLeave = daily.EarlyLeaveMinutes > 0;
            daily.HasMissingPunch =
                (policy.RequireCheckIn && punch.FirstCheckIn == null) ||
                (policy.RequireCheckOut && punch.LastCheckOut == null);
            daily.IsBusinessTrip = schedule.AdditionalAssignments.Any(x =>
                string.Equals(x.AssignmentType, "BusinessTrip", StringComparison.OrdinalIgnoreCase));
            daily.IsRemoteAttendance = schedule.AdditionalAssignments.Any(x =>
                string.Equals(x.AssignmentType, "Remote", StringComparison.OrdinalIgnoreCase));

            if (schedule.IsRestDay && !daily.IsPresent)
            {
                daily.AttendanceStatus = AttendanceValueConstants.AttendanceStatus.RestDay;
            }
            else if (schedule.IsHoliday && !daily.IsPresent)
            {
                daily.AttendanceStatus = AttendanceValueConstants.AttendanceStatus.Holiday;
            }
            else if (daily.IsBusinessTrip && !daily.IsPresent)
            {
                daily.AttendanceStatus = AttendanceValueConstants.AttendanceStatus.BusinessTrip;
            }
            else if (daily.IsRemoteAttendance && daily.IsPresent)
            {
                daily.AttendanceStatus = AttendanceValueConstants.AttendanceStatus.Remote;
            }
            else if (!daily.IsPresent && schedule.IsResolved && !schedule.IsRestDay && !schedule.IsHoliday)
            {
                daily.IsAbsent = true;
                daily.AttendanceStatus = AttendanceValueConstants.AttendanceStatus.Absent;
            }
            else if (daily.HasMissingPunch)
            {
                daily.AttendanceStatus = AttendanceValueConstants.AttendanceStatus.Incomplete;
            }
            else if (daily.IsLate)
            {
                daily.AttendanceStatus = AttendanceValueConstants.AttendanceStatus.Late;
            }
            else if (daily.IsEarlyLeave)
            {
                daily.AttendanceStatus = AttendanceValueConstants.AttendanceStatus.EarlyLeave;
            }
            else if (daily.IsPresent)
            {
                daily.AttendanceStatus = AttendanceValueConstants.AttendanceStatus.Present;
            }
            else
            {
                daily.AttendanceStatus = AttendanceValueConstants.AttendanceStatus.Unprocessed;
            }
        }

        private static List<ExceptionDraft> BuildExceptions(
            HrdAttendanceDaily daily,
            List<HrdAttendanceRawLog> rawLogs,
            AttendanceScheduleResolutionResponse schedule,
            PolicyRuntime policy,
            PunchResult punch)
        {
            var result = new List<ExceptionDraft>();

            if (!schedule.IsResolved)
            {
                result.Add(new ExceptionDraft
                {
                    ExceptionCode = "SCHEDULE_UNRESOLVED",
                    ExceptionType = AttendanceValueConstants.AttendanceExceptionType.ScheduleMismatch,
                    Severity = AttendanceValueConstants.AttendanceExceptionSeverity.Critical,
                    IsPayrollBlocking = true,
                    DetectionRule = "ScheduleResolver.IsResolved",
                    Message = "Jadwal attendance tidak dapat diselesaikan."
                });
            }

            if (schedule.HasBlockingConflict)
            {
                result.Add(new ExceptionDraft
                {
                    ExceptionCode = "SCHEDULE_CONFLICT",
                    ExceptionType = AttendanceValueConstants.AttendanceExceptionType.ScheduleConflict,
                    Severity = AttendanceValueConstants.AttendanceExceptionSeverity.Critical,
                    IsPayrollBlocking = true,
                    DetectionRule = "ScheduleResolver.HasBlockingConflict",
                    Message = schedule.ConflictCodes.Count == 0
                        ? "Terdapat konflik jadwal yang menghalangi pemrosesan attendance."
                        : $"Konflik jadwal: {string.Join(", ", schedule.ConflictCodes)}."
                });
            }

            if (policy.RequireCheckIn && punch.FirstCheckIn == null && !schedule.IsRestDay && !schedule.IsHoliday)
            {
                result.Add(new ExceptionDraft
                {
                    ExceptionCode = "MISSING_CHECK_IN",
                    ExceptionType = AttendanceValueConstants.AttendanceExceptionType.MissingCheckIn,
                    Severity = AttendanceValueConstants.AttendanceExceptionSeverity.High,
                    IsPayrollBlocking = true,
                    ExpectedAt = schedule.ScheduledStartAt,
                    DetectionRule = "AttendancePolicy.RequireCheckIn",
                    Message = "Check-in wajib tetapi tidak ditemukan pada attendance window."
                });
            }

            if (policy.RequireCheckOut && punch.LastCheckOut == null && !schedule.IsRestDay && !schedule.IsHoliday)
            {
                result.Add(new ExceptionDraft
                {
                    ExceptionCode = "MISSING_CHECK_OUT",
                    ExceptionType = AttendanceValueConstants.AttendanceExceptionType.MissingCheckOut,
                    Severity = AttendanceValueConstants.AttendanceExceptionSeverity.High,
                    IsPayrollBlocking = true,
                    ExpectedAt = schedule.ScheduledEndAt,
                    DetectionRule = "AttendancePolicy.RequireCheckOut",
                    Message = "Check-out wajib tetapi tidak ditemukan pada attendance window."
                });
            }

            if (daily.IsAbsent)
            {
                result.Add(new ExceptionDraft
                {
                    ExceptionCode = "ABSENT",
                    ExceptionType = AttendanceValueConstants.AttendanceExceptionType.Absent,
                    Severity = AttendanceValueConstants.AttendanceExceptionSeverity.Critical,
                    IsPayrollBlocking = true,
                    ExpectedAt = schedule.ScheduledStartAt,
                    DetectionRule = "NoAttendancePunch",
                    Message = "Tidak ditemukan event kehadiran pada jadwal kerja yang aktif."
                });
            }

            if (daily.IsLate)
            {
                result.Add(new ExceptionDraft
                {
                    ExceptionCode = "LATE",
                    ExceptionType = AttendanceValueConstants.AttendanceExceptionType.Late,
                    Severity = AttendanceValueConstants.AttendanceExceptionSeverity.Warning,
                    IsPayrollBlocking = false,
                    ExpectedAt = schedule.ScheduledStartAt,
                    ActualAt = daily.FirstCheckInAt,
                    DifferenceMinutes = daily.LateMinutes,
                    DetectionRule = "GracePeriod.LateCheckInGraceMinutes",
                    Message = $"Terlambat {daily.LateMinutes} menit dari jadwal check-in."
                });
            }

            if (daily.IsEarlyLeave)
            {
                result.Add(new ExceptionDraft
                {
                    ExceptionCode = "EARLY_LEAVE",
                    ExceptionType = AttendanceValueConstants.AttendanceExceptionType.EarlyLeave,
                    Severity = AttendanceValueConstants.AttendanceExceptionSeverity.Warning,
                    IsPayrollBlocking = false,
                    ExpectedAt = schedule.ScheduledEndAt,
                    ActualAt = daily.LastCheckOutAt,
                    DifferenceMinutes = daily.EarlyLeaveMinutes,
                    DetectionRule = "GracePeriod.EarlyCheckOutGraceMinutes",
                    Message = $"Pulang {daily.EarlyLeaveMinutes} menit lebih awal dari jadwal."
                });
            }

            if (policy.MinimumWorkMinutes > 0 && daily.IsPresent && daily.ActualWorkMinutes < policy.MinimumWorkMinutes)
            {
                result.Add(new ExceptionDraft
                {
                    ExceptionCode = "INSUFFICIENT_WORK_MINUTES",
                    ExceptionType = AttendanceValueConstants.AttendanceExceptionType.ExcessiveWorkHours,
                    Severity = AttendanceValueConstants.AttendanceExceptionSeverity.High,
                    IsPayrollBlocking = true,
                    DifferenceMinutes = policy.MinimumWorkMinutes - daily.ActualWorkMinutes,
                    DetectionRule = "AttendancePolicy.MinimumWorkMinutes",
                    Message = $"Jam kerja aktual kurang {policy.MinimumWorkMinutes - daily.ActualWorkMinutes} menit dari minimum policy."
                });
            }

            if (policy.MaximumWorkMinutes > 0 && daily.ActualWorkMinutes > policy.MaximumWorkMinutes)
            {
                result.Add(new ExceptionDraft
                {
                    ExceptionCode = "EXCESSIVE_WORK_HOURS",
                    ExceptionType = AttendanceValueConstants.AttendanceExceptionType.ExcessiveWorkHours,
                    Severity = AttendanceValueConstants.AttendanceExceptionSeverity.High,
                    IsPayrollBlocking = true,
                    DifferenceMinutes = daily.ActualWorkMinutes - policy.MaximumWorkMinutes,
                    DetectionRule = "AttendancePolicy.MaximumWorkMinutes",
                    Message = $"Jam kerja aktual melebihi maksimum policy sebesar {daily.ActualWorkMinutes - policy.MaximumWorkMinutes} menit."
                });
            }

            if (policy.IsAttendanceLocationRequired)
            {
                var outside = rawLogs.FirstOrDefault(x =>
                    x.DistanceMeters.HasValue &&
                    x.AttendanceLocation != null &&
                    x.DistanceMeters.Value > x.AttendanceLocation.RadiusMeters);

                if (outside != null)
                {
                    result.Add(new ExceptionDraft
                    {
                        ExceptionCode = "OUTSIDE_GEOFENCE",
                        ExceptionType = AttendanceValueConstants.AttendanceExceptionType.OutsideGeofence,
                        Severity = AttendanceValueConstants.AttendanceExceptionSeverity.High,
                        IsPayrollBlocking = true,
                        ActualAt = outside.EventAt,
                        DifferenceMinutes = (int)Math.Round(outside.DistanceMeters!.Value - outside.AttendanceLocation!.RadiusMeters),
                        DetectionRule = "AttendanceLocation.RadiusMeters",
                        Message = "Event attendance berada di luar radius lokasi yang diizinkan."
                    });
                }
            }

            if (!policy.AllowMultipleCheckInOut && (punch.CheckInCount > 1 || punch.CheckOutCount > 1))
            {
                result.Add(new ExceptionDraft
                {
                    ExceptionCode = "DUPLICATE_PUNCH",
                    ExceptionType = AttendanceValueConstants.AttendanceExceptionType.DuplicatePunch,
                    Severity = AttendanceValueConstants.AttendanceExceptionSeverity.Info,
                    IsPayrollBlocking = false,
                    DetectionRule = "AttendancePolicy.AllowMultipleCheckInOut",
                    Message = "Ditemukan lebih dari satu check-in atau check-out ketika policy tidak mengizinkan multiple punch."
                });
            }

            return result;
        }

        private void SynchronizeExceptions(
            HrdAttendanceDaily daily,
            List<ExceptionDraft> desired,
            Guid actorUserId)
        {
            var now = DateTime.UtcNow;
            var existing = daily.Exceptions
                .Where(x => !x.IsDelete && x.IsAutoDetected && x.CorrectionRequestId == null)
                .ToList();
            var handledCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var current in existing)
            {
                var draft = desired.FirstOrDefault(x =>
                    string.Equals(x.ExceptionCode, current.ExceptionCode, StringComparison.OrdinalIgnoreCase));
                if (draft == null || handledCodes.Contains(current.ExceptionCode))
                {
                    current.ExceptionStatus = AttendanceValueConstants.AttendanceExceptionStatus.Closed;
                    current.IsActive = false;
                    current.ResolvedAt = now;
                    current.ResolvedByUserId = actorUserId;
                    current.ResolutionNote = "Ditutup otomatis karena tidak lagi terdeteksi pada proses ulang.";
                    current.UpdateDateTime = now;
                    current.UpdateBy = actorUserId;
                    continue;
                }

                ApplyExceptionDraft(current, draft, actorUserId, now);
                handledCodes.Add(draft.ExceptionCode);
            }

            foreach (var draft in desired.Where(x => !handledCodes.Contains(x.ExceptionCode)))
            {
                var entity = new HrdAttendanceException
                {
                    Id = Guid.NewGuid(),
                    AttendanceDailyId = daily.Id,
                    WorkforceProfileId = daily.WorkforceProfileId,
                    ExceptionStatus = AttendanceValueConstants.AttendanceExceptionStatus.Open,
                    DetectedAt = now,
                    IsAutoDetected = true,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                ApplyExceptionDraft(entity, draft, actorUserId, now);
                AddNewException(daily, entity);
            }
        }

        private void AddNewException(
            HrdAttendanceDaily daily,
            HrdAttendanceException exceptionEntity)
        {
            daily.Exceptions.Add(exceptionEntity);

            // Exception hasil auto-detection yang baru harus menjadi INSERT.
            // Pada reprocess, parent Daily sudah existing/tracked sehingga entity
            // baru dengan GUID non-empty dapat salah diperlakukan sebagai Modified.
            _dbContext.Entry(exceptionEntity).State = EntityState.Added;
        }

        private static void ApplyExceptionDraft(
            HrdAttendanceException entity,
            ExceptionDraft draft,
            Guid actorUserId,
            DateTime now)
        {
            entity.ExceptionCode = draft.ExceptionCode;
            entity.ExceptionType = draft.ExceptionType;
            entity.Severity = draft.Severity;
            entity.ExceptionStatus = AttendanceValueConstants.AttendanceExceptionStatus.Open;
            entity.ExpectedAt = draft.ExpectedAt;
            entity.ActualAt = draft.ActualAt;
            entity.DifferenceMinutes = draft.DifferenceMinutes;
            entity.IsPayrollBlocking = draft.IsPayrollBlocking;
            entity.DetectionRule = draft.DetectionRule;
            entity.Message = draft.Message;
            entity.IsActive = true;
            entity.ResolvedAt = null;
            entity.ResolvedByUserId = null;
            entity.ResolutionNote = null;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
        }

        private async Task SoftDeleteProcessorSegmentsAsync(
            Guid attendanceDailyId,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            await _dbContext.Set<HrdAttendanceDailySegment>()
                .Where(x =>
                    x.AttendanceDailyId == attendanceDailyId &&
                    !x.IsDelete &&
                    !x.IsCorrected &&
                    (x.SegmentSource == AttendanceValueConstants.AttendanceSegmentSource.Processor ||
                     x.SegmentSource == AttendanceValueConstants.AttendanceSegmentSource.Roster))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.IsDelete, true)
                    .SetProperty(x => x.IsActive, false)
                    .SetProperty(x => x.DeleteDateTime, (DateTime?)now)
                    .SetProperty(x => x.DeleteBy, actorUserId)
                    .SetProperty(x => x.UpdateDateTime, (DateTime?)now)
                    .SetProperty(x => x.UpdateBy, actorUserId), cancellationToken);
        }

        private void AddNewSegment(
            HrdAttendanceDaily daily,
            HrdAttendanceDailySegment segment)
        {
            daily.Segments.Add(segment);

            // Semua segment hasil processor/roster pada proses ini adalah row baru.
            // Paksa state Added agar EF menghasilkan INSERT, bukan UPDATE terhadap
            // GUID baru yang belum pernah ada di database.
            _dbContext.Entry(segment).State = EntityState.Added;
        }

        private void CreateSegments(
            HrdAttendanceDaily daily,
            AttendanceScheduleResolutionResponse schedule,
            PunchResult punch,
            Guid actorUserId)
        {
            var order = 1;
            var workSegment = new HrdAttendanceDailySegment
            {
                Id = Guid.NewGuid(),
                AttendanceDailyId = daily.Id,
                ShiftAssignmentId = schedule.PrimaryShiftAssignmentId,
                SegmentOrder = order++,
                SegmentType = AttendanceValueConstants.AttendanceSegmentType.Work,
                SegmentSource = AttendanceValueConstants.AttendanceSegmentSource.Processor,
                ScheduledStartAt = schedule.ScheduledStartAt,
                ScheduledEndAt = schedule.ScheduledEndAt,
                ActualStartAt = punch.EffectiveStartAt,
                ActualEndAt = punch.EffectiveEndAt,
                StartRawLogId = punch.FirstCheckIn?.Id,
                EndRawLogId = punch.LastCheckOut?.Id,
                ScheduledMinutes = daily.ScheduledWorkMinutes,
                ActualMinutes = daily.ActualWorkMinutes,
                BreakMinutes = daily.BreakMinutes,
                PayableMinutes = daily.PayableWorkMinutes,
                LateMinutes = daily.LateMinutes,
                EarlyLeaveMinutes = daily.EarlyLeaveMinutes,
                OvertimeMinutes = daily.OvertimeMinutes,
                IsOvernight = daily.IsOvernightSchedule,
                SegmentStatus = AttendanceValueConstants.AttendanceSegmentStatus.Calculated,
                Notes = "Primary work segment hasil attendance processor.",
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };
            AddNewSegment(daily, workSegment);

            foreach (var pair in punch.BreakPairs)
            {
                var breakSegment = new HrdAttendanceDailySegment
                {
                    Id = Guid.NewGuid(),
                    AttendanceDailyId = daily.Id,
                    ShiftAssignmentId = schedule.PrimaryShiftAssignmentId,
                    SegmentOrder = order++,
                    SegmentType = AttendanceValueConstants.AttendanceSegmentType.Break,
                    SegmentSource = AttendanceValueConstants.AttendanceSegmentSource.Processor,
                    ActualStartAt = pair.Start.EventAt,
                    ActualEndAt = pair.End.EventAt,
                    StartRawLogId = pair.Start.Id,
                    EndRawLogId = pair.End.Id,
                    ActualMinutes = pair.Minutes,
                    BreakMinutes = pair.Minutes,
                    SegmentStatus = AttendanceValueConstants.AttendanceSegmentStatus.Calculated,
                    Notes = "Break segment hasil pasangan BreakStart dan BreakEnd.",
                    IsActive = true,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                AddNewSegment(daily, breakSegment);
            }

            foreach (var assignment in schedule.AdditionalAssignments.OrderBy(x => x.ScheduledStartAt))
            {
                var segmentType = MapAdditionalSegmentType(assignment.AssignmentType);
                if (segmentType == null)
                {
                    continue;
                }

                var additionalSegment = new HrdAttendanceDailySegment
                {
                    Id = Guid.NewGuid(),
                    AttendanceDailyId = daily.Id,
                    ShiftAssignmentId = assignment.ShiftAssignmentId,
                    SegmentOrder = order++,
                    SegmentType = segmentType,
                    SegmentSource = AttendanceValueConstants.AttendanceSegmentSource.Roster,
                    ScheduledStartAt = assignment.ScheduledStartAt,
                    ScheduledEndAt = assignment.ScheduledEndAt,
                    ScheduledMinutes = Math.Max(0, assignment.PlannedWorkMinutes),
                    IsOvernight = assignment.ScheduledEndAt.Date > assignment.ScheduledStartAt.Date,
                    SegmentStatus = AttendanceValueConstants.AttendanceSegmentStatus.Pending,
                    Notes = $"Additional assignment {assignment.AssignmentType} dari roster.",
                    IsActive = true,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                AddNewSegment(daily, additionalSegment);
            }
        }

        private static string? MapAdditionalSegmentType(string assignmentType)
        {
            if (string.Equals(assignmentType, "Overtime", StringComparison.OrdinalIgnoreCase))
                return AttendanceValueConstants.AttendanceSegmentType.Overtime;
            if (string.Equals(assignmentType, "OnCall", StringComparison.OrdinalIgnoreCase))
                return AttendanceValueConstants.AttendanceSegmentType.OnCall;
            if (string.Equals(assignmentType, "Remote", StringComparison.OrdinalIgnoreCase))
                return AttendanceValueConstants.AttendanceSegmentType.Remote;
            if (string.Equals(assignmentType, "BusinessTrip", StringComparison.OrdinalIgnoreCase))
                return AttendanceValueConstants.AttendanceSegmentType.BusinessTrip;
            return null;
        }

        private async Task<PolicyRuntime> ResolvePolicyRuntimeAsync(
            Guid? attendancePolicyId,
            CancellationToken cancellationToken)
        {
            if (!attendancePolicyId.HasValue)
            {
                return new PolicyRuntime();
            }

            return await _dbContext.Set<MstAttendancePolicy>()
                .AsNoTracking()
                .Where(x => x.Id == attendancePolicyId.Value && !x.IsDelete && x.IsActive)
                .Select(x => new PolicyRuntime
                {
                    RequireCheckIn = x.RequireCheckIn,
                    RequireCheckOut = x.RequireCheckOut,
                    AllowMultipleCheckInOut = x.AllowMultipleCheckInOut,
                    AutoCloseOpenAttendance = x.AutoCloseOpenAttendance,
                    AutoCloseAfterMinutes = x.AutoCloseAfterMinutes,
                    MinimumWorkMinutes = x.MinimumWorkMinutes,
                    MaximumWorkMinutes = x.MaximumWorkMinutes,
                    IsOvertimeEnabled = x.IsOvertimeEnabled,
                    OvertimeThresholdMinutes = x.OvertimeThresholdMinutes,
                    IsAttendanceLocationRequired = x.IsAttendanceLocationRequired
                })
                .FirstOrDefaultAsync(cancellationToken)
                ?? new PolicyRuntime();
        }

        private static CaptureWindow BuildCaptureWindow(
            AttendanceScheduleResolutionResponse schedule,
            PolicyRuntime policy,
            DateOnly workDate)
        {
            if (schedule.ScheduledStartAt.HasValue && schedule.ScheduledEndAt.HasValue)
            {
                var startAt = schedule.ScheduledStartAt.Value.AddMinutes(-Math.Max(DefaultCaptureBeforeMinutes, schedule.EarlyCheckInMinutes));
                var maximumPolicyEnd = policy.MaximumWorkMinutes > 0
                    ? schedule.ScheduledStartAt.Value.AddMinutes(policy.MaximumWorkMinutes + 60)
                    : schedule.ScheduledEndAt.Value.AddMinutes(DefaultCaptureAfterMinutes);
                var graceEnd = schedule.ScheduledEndAt.Value.AddMinutes(Math.Max(DefaultCaptureAfterMinutes, schedule.LateCheckOutMinutes));

                return new CaptureWindow
                {
                    StartAt = NormalizeUtc(startAt),
                    EndAt = NormalizeUtc(maximumPolicyEnd > graceEnd ? maximumPolicyEnd : graceEnd)
                };
            }

            var timeZone = ResolveTimeZone(schedule.TimeZoneId);
            var localStart = workDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
            var localEnd = workDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified).AddHours(6);

            return new CaptureWindow
            {
                StartAt = TimeZoneInfo.ConvertTimeToUtc(localStart, timeZone),
                EndAt = TimeZoneInfo.ConvertTimeToUtc(localEnd, timeZone)
            };
        }

        private async Task<WorkforceRuntime?> ResolveWorkforceAsync(
            Guid workforceProfileId,
            DateOnly workDate,
            CancellationToken cancellationToken)
        {
            var profile = await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .Include(x => x.UserAccount)
                .Include(x => x.Employee)
                .Include(x => x.Doctor)
                .FirstOrDefaultAsync(x =>
                    x.Id == workforceProfileId &&
                    !x.IsDelete &&
                    x.IsActive,
                    cancellationToken);

            if (profile == null || profile.UserAccount == null || !profile.UserAccount.IsActive)
            {
                return null;
            }

            var dateStart = workDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var dateEnd = dateStart.AddDays(1);
            var organization = await _dbContext.Set<WfpOrganizationAssignment>()
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.EffectiveStartDate < dateEnd &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= dateStart))
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.EffectiveStartDate)
                .FirstOrDefaultAsync(cancellationToken);

            return new WorkforceRuntime
            {
                WorkforceProfileId = profile.Id,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                UserId = profile.UserAccount.Id,
                UserType = profile.UserType,
                EmployeeId = profile.Employee?.Id,
                DoctorId = profile.Doctor?.Id,
                OrganizationAssignmentId = organization?.Id,
                HospitalSiteId = organization?.HospitalSiteId,
                OrganizationUnitId = organization?.OrganizationUnitId,
                DepartmentId = organization?.DepartmentId ?? profile.PrimaryDepartmentId,
                PositionId = organization?.PositionId ?? profile.PrimaryPositionId,
                WorkLocationId = organization?.WorkLocationId
            };
        }

        private async Task<List<WorkforceRuntime>> ResolveTargetWorkforcesAsync(
            ProcessAttendanceRangeRequest request,
            CancellationToken cancellationToken)
        {
            if (request.WorkforceProfileId.HasValue && request.WorkforceProfileId.Value != Guid.Empty)
            {
                var single = await ResolveWorkforceAsync(
                    request.WorkforceProfileId.Value,
                    request.StartDate,
                    cancellationToken);
                return single == null ? new List<WorkforceRuntime>() : new List<WorkforceRuntime> { single };
            }

            var startAt = request.StartDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var endExclusive = request.EndDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

            var query = _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    x.UserAccount != null &&
                    x.UserAccount.IsActive);

            if (request.DepartmentId.HasValue && request.DepartmentId.Value != Guid.Empty)
            {
                query = query.Where(x => x.OrganizationAssignments.Any(a =>
                    a.DepartmentId == request.DepartmentId.Value &&
                    a.IsActive &&
                    !a.IsDelete &&
                    !a.IsCancel &&
                    a.EffectiveStartDate < endExclusive &&
                    (!a.EffectiveEndDate.HasValue || a.EffectiveEndDate.Value >= startAt)));
            }

            if (request.OrganizationUnitId.HasValue && request.OrganizationUnitId.Value != Guid.Empty)
            {
                query = query.Where(x => x.OrganizationAssignments.Any(a =>
                    a.OrganizationUnitId == request.OrganizationUnitId.Value &&
                    a.IsActive &&
                    !a.IsDelete &&
                    !a.IsCancel &&
                    a.EffectiveStartDate < endExclusive &&
                    (!a.EffectiveEndDate.HasValue || a.EffectiveEndDate.Value >= startAt)));
            }

            if (request.HospitalSiteId.HasValue && request.HospitalSiteId.Value != Guid.Empty)
            {
                query = query.Where(x => x.OrganizationAssignments.Any(a =>
                    a.HospitalSiteId == request.HospitalSiteId.Value &&
                    a.IsActive &&
                    !a.IsDelete &&
                    !a.IsCancel &&
                    a.EffectiveStartDate < endExclusive &&
                    (!a.EffectiveEndDate.HasValue || a.EffectiveEndDate.Value >= startAt)));
            }

            var ids = await query
                .OrderBy(x => x.ProfileCode)
                .Select(x => x.Id)
                .Take(MaximumWorkforcePerRequest + 1)
                .ToListAsync(cancellationToken);

            var result = new List<WorkforceRuntime>();
            foreach (var id in ids)
            {
                var workforce = await ResolveWorkforceAsync(id, request.StartDate, cancellationToken);
                if (workforce != null)
                {
                    result.Add(workforce);
                }
            }

            return result;
        }

        private async Task<HrdAttendanceProcessingRun> CreateRunAsync(
            string mode,
            string triggerSource,
            DateOnly startDate,
            DateOnly endDate,
            Guid? targetWorkforceProfileId,
            Guid? hospitalSiteId,
            Guid? organizationUnitId,
            Guid? departmentId,
            int targetCount,
            Guid actorUserId,
            string? correlationId,
            string parametersJson,
            string? notes,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var run = new HrdAttendanceProcessingRun
            {
                Id = Guid.NewGuid(),
                RunNumber = $"ATT-{now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
                ProcessingMode = mode,
                RunStatus = AttendanceValueConstants.ProcessingRunStatus.Running,
                TriggerSource = triggerSource,
                StartDate = startDate,
                EndDate = endDate,
                TargetWorkforceProfileId = NormalizeGuid(targetWorkforceProfileId),
                HospitalSiteId = NormalizeGuid(hospitalSiteId),
                OrganizationUnitId = NormalizeGuid(organizationUnitId),
                DepartmentId = NormalizeGuid(departmentId),
                ProcessingVersion = 1,
                TargetCount = targetCount,
                StartedAt = now,
                TriggeredByUserId = actorUserId,
                CorrelationId = NormalizeNullableString(correlationId),
                ParametersJson = parametersJson,
                Notes = NormalizeNullableString(notes),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<HrdAttendanceProcessingRun>().Add(run);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return run;
        }

        private async Task CompleteRunAsync(
            HrdAttendanceProcessingRun run,
            IEnumerable<AttendanceProcessingItemResponse> items,
            CancellationToken cancellationToken)
        {
            var materialized = items.ToList();
            run.SuccessCount = materialized.Count(x => x.Success && !x.IsSkipped);
            run.FailedCount = materialized.Count(x => !x.Success && !x.IsSkipped);
            run.SkippedCount = materialized.Count(x => x.IsSkipped);
            run.CompletedAt = DateTime.UtcNow;
            run.RunStatus = run.FailedCount == 0 && run.SkippedCount == 0
                ? AttendanceValueConstants.ProcessingRunStatus.Completed
                : run.SuccessCount > 0 || run.SkippedCount > 0
                    ? AttendanceValueConstants.ProcessingRunStatus.CompletedWithErrors
                    : AttendanceValueConstants.ProcessingRunStatus.Failed;
            run.ErrorSummary = BuildErrorSummary(materialized);
            run.UpdateDateTime = DateTime.UtcNow;
            run.UpdateBy = run.TriggeredByUserId ?? Guid.Empty;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private IQueryable<HrdAttendanceProcessingRun> BuildRunQuery(
            AttendanceProcessingRunQueryRequest request)
        {
            var query = _dbContext.Set<HrdAttendanceProcessingRun>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (request.StartDate.HasValue)
                query = query.Where(x => x.EndDate >= request.StartDate.Value);
            if (request.EndDate.HasValue)
                query = query.Where(x => x.StartDate <= request.EndDate.Value);
            if (!string.IsNullOrWhiteSpace(request.ProcessingMode))
                query = query.Where(x => x.ProcessingMode == request.ProcessingMode.Trim());
            if (!string.IsNullOrWhiteSpace(request.RunStatus))
                query = query.Where(x => x.RunStatus == request.RunStatus.Trim());
            if (!string.IsNullOrWhiteSpace(request.TriggerSource))
                query = query.Where(x => x.TriggerSource == request.TriggerSource.Trim());
            if (request.WorkforceProfileId.HasValue && request.WorkforceProfileId.Value != Guid.Empty)
                query = query.Where(x => x.TargetWorkforceProfileId == request.WorkforceProfileId.Value);
            if (request.HospitalSiteId.HasValue && request.HospitalSiteId.Value != Guid.Empty)
                query = query.Where(x => x.HospitalSiteId == request.HospitalSiteId.Value);
            if (request.OrganizationUnitId.HasValue && request.OrganizationUnitId.Value != Guid.Empty)
                query = query.Where(x => x.OrganizationUnitId == request.OrganizationUnitId.Value);
            if (request.DepartmentId.HasValue && request.DepartmentId.Value != Guid.Empty)
                query = query.Where(x => x.DepartmentId == request.DepartmentId.Value);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim().ToLower();
                query = query.Where(x =>
                    x.RunNumber.ToLower().Contains(keyword) ||
                    (x.CorrelationId != null && x.CorrelationId.ToLower().Contains(keyword)) ||
                    (x.Notes != null && x.Notes.ToLower().Contains(keyword)) ||
                    (x.TargetWorkforceProfile != null && x.TargetWorkforceProfile.DisplayName.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<HrdAttendanceProcessingRun> ApplyRunSorting(
            IQueryable<HrdAttendanceProcessingRun> query,
            string? sortBy,
            string? sortDirection)
        {
            var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "startedAt").Trim().ToLowerInvariant() switch
            {
                "completedat" => descending ? query.OrderByDescending(x => x.CompletedAt) : query.OrderBy(x => x.CompletedAt),
                "runnumber" => descending ? query.OrderByDescending(x => x.RunNumber) : query.OrderBy(x => x.RunNumber),
                "runstatus" => descending ? query.OrderByDescending(x => x.RunStatus).ThenByDescending(x => x.StartedAt) : query.OrderBy(x => x.RunStatus).ThenByDescending(x => x.StartedAt),
                "targetcount" => descending ? query.OrderByDescending(x => x.TargetCount).ThenByDescending(x => x.StartedAt) : query.OrderBy(x => x.TargetCount).ThenByDescending(x => x.StartedAt),
                _ => descending ? query.OrderByDescending(x => x.StartedAt).ThenByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.StartedAt).ThenBy(x => x.CreateDateTime)
            };
        }

        private async Task<HrdAttendanceProcessingRun?> FindExistingRunByCorrelationAsync(
            string? correlationId,
            CancellationToken cancellationToken)
        {
            var normalized = NormalizeNullableString(correlationId);
            if (normalized == null)
                return null;

            return await _dbContext.Set<HrdAttendanceProcessingRun>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CorrelationId == normalized && !x.IsDelete, cancellationToken);
        }

        private static AttendanceProcessingExecutionResponse MapExecutionResponse(
            HrdAttendanceProcessingRun run,
            List<AttendanceProcessingItemResponse> items)
        {
            return new AttendanceProcessingExecutionResponse
            {
                ProcessingRunId = run.Id,
                RunNumber = run.RunNumber,
                RunStatus = run.RunStatus,
                StartDate = run.StartDate,
                EndDate = run.EndDate,
                TargetCount = run.TargetCount,
                SuccessCount = run.SuccessCount,
                FailedCount = run.FailedCount,
                SkippedCount = run.SkippedCount,
                StartedAt = run.StartedAt ?? run.CreateDateTime,
                CompletedAt = run.CompletedAt,
                ErrorSummary = run.ErrorSummary,
                Items = items
            };
        }

        private static string BuildProcessingMessage(
            AttendanceScheduleResolutionResponse schedule,
            PunchResult punch,
            List<ExceptionDraft> exceptions)
        {
            var parts = new List<string>
            {
                $"ScheduleSource={schedule.ScheduleSource}",
                $"RawPunch={punch.CheckInCount + punch.CheckOutCount}",
                $"Exception={exceptions.Count}"
            };

            if (!schedule.IsResolved)
                parts.Add("Schedule belum terselesaikan");
            if (schedule.HasBlockingConflict)
                parts.Add("Terdapat blocking schedule conflict");

            return LimitMessage(string.Join("; ", parts), 1000);
        }

        private static string? BuildErrorSummary(List<AttendanceProcessingItemResponse> items)
        {
            var failures = items
                .Where(x => !x.Success && !x.IsSkipped)
                .Take(10)
                .Select(x => $"{x.WorkforceProfileCode ?? x.WorkforceProfileId.ToString()} {x.WorkDate:yyyy-MM-dd}: {x.Message}")
                .ToList();

            if (failures.Count == 0)
                return null;

            var suffix = items.Count(x => !x.Success && !x.IsSkipped) > failures.Count
                ? "; dan kegagalan lainnya."
                : string.Empty;
            return LimitMessage(string.Join(" | ", failures) + suffix, 2000);
        }

        private static int CalculateNightWorkMinutes(
            DateTime? startAt,
            DateTime? endAt,
            string? timeZoneId)
        {
            if (!startAt.HasValue || !endAt.HasValue || endAt <= startAt)
                return 0;

            var timeZone = ResolveTimeZone(timeZoneId);
            var localStart = TimeZoneInfo.ConvertTimeFromUtc(NormalizeUtc(startAt.Value), timeZone);
            var localEnd = TimeZoneInfo.ConvertTimeFromUtc(NormalizeUtc(endAt.Value), timeZone);
            var total = 0;

            for (var date = localStart.Date.AddDays(-1); date <= localEnd.Date; date = date.AddDays(1))
            {
                var nightStart = date.AddHours(22);
                var nightEnd = date.AddDays(1).AddHours(6);
                var overlapStart = localStart > nightStart ? localStart : nightStart;
                var overlapEnd = localEnd < nightEnd ? localEnd : nightEnd;
                if (overlapEnd > overlapStart)
                {
                    total += (int)Math.Floor((overlapEnd - overlapStart).TotalMinutes);
                }
            }

            return Math.Max(0, total);
        }

        private static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
        {
            var requested = string.IsNullOrWhiteSpace(timeZoneId) ? DefaultTimeZoneId : timeZoneId.Trim();
            var candidates = new List<string> { requested };
            if (string.Equals(requested, "Asia/Jakarta", StringComparison.OrdinalIgnoreCase))
                candidates.Add("SE Asia Standard Time");
            else if (string.Equals(requested, "SE Asia Standard Time", StringComparison.OrdinalIgnoreCase))
                candidates.Add("Asia/Jakarta");

            foreach (var candidate in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(candidate);
                }
                catch (TimeZoneNotFoundException)
                {
                }
                catch (InvalidTimeZoneException)
                {
                }
            }

            return TimeZoneInfo.Utc;
        }

        private static string? ValidateRange(DateOnly startDate, DateOnly endDate)
        {
            if (endDate < startDate)
                return "Tanggal selesai tidak boleh lebih kecil daripada tanggal mulai.";
            var totalDays = endDate.DayNumber - startDate.DayNumber + 1;
            if (totalDays > MaximumRangeDays)
                return $"Rentang tanggal maksimal {MaximumRangeDays} hari.";
            return null;
        }

        private static string? NormalizeTriggerSource(string? value)
        {
            var candidates = new[]
            {
                AttendanceValueConstants.ProcessingTriggerSource.Api,
                AttendanceValueConstants.ProcessingTriggerSource.Manual,
                AttendanceValueConstants.ProcessingTriggerSource.Scheduler,
                AttendanceValueConstants.ProcessingTriggerSource.System
            };
            return candidates.FirstOrDefault(x => string.Equals(x, value?.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static Guid? NormalizeGuid(Guid? value) =>
            !value.HasValue || value.Value == Guid.Empty ? null : value.Value;

        private static string? NormalizeNullableString(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static DateTime NormalizeUtc(DateTime value) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        private static string LimitMessage(string value, int length) =>
            value.Length <= length ? value : value[..length];

        private static string DescribeConcurrencyConflict(DbUpdateConcurrencyException exception)
        {
            var entries = exception.Entries
                .Select(entry => $"{entry.Metadata.ClrType.Name} ({entry.State}, Id={entry.Property("Id").CurrentValue})")
                .ToList();

            return entries.Count == 0
                ? exception.GetBaseException().Message
                : $"Attendance concurrency conflict: {string.Join("; ", entries)}.";
        }

        private void DetachChangedEntries()
        {
            var entries = _dbContext.ChangeTracker.Entries().ToList();
            foreach (var entry in entries)
            {
                if (entry.Entity is HrdAttendanceProcessingRun)
                    continue;
                entry.State = EntityState.Detached;
            }
        }

        private void DetachProcessingItemEntries()
        {
            var entries = _dbContext.ChangeTracker.Entries().ToList();
            foreach (var entry in entries)
            {
                if (entry.Entity is HrdAttendanceProcessingRun)
                    continue;

                if (entry.Entity is HrdAttendanceDaily ||
                    entry.Entity is HrdAttendanceDailySegment ||
                    entry.Entity is HrdAttendanceException ||
                    entry.Entity is HrdAttendanceRawLog ||
                    entry.Entity is MstWorkforceProfile ||
                    entry.Entity is WfpOrganizationAssignment ||
                    entry.Entity is MstAttendancePolicy)
                {
                    entry.State = EntityState.Detached;
                }
            }
        }

        private sealed class WorkforceRuntime
        {
            public Guid WorkforceProfileId { get; set; }
            public string ProfileCode { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
            public Guid UserId { get; set; }
            public QuilvianSystemBackend.Enums.UserType UserType { get; set; }
            public Guid? EmployeeId { get; set; }
            public Guid? DoctorId { get; set; }
            public Guid? OrganizationAssignmentId { get; set; }
            public Guid? HospitalSiteId { get; set; }
            public Guid? OrganizationUnitId { get; set; }
            public Guid? DepartmentId { get; set; }
            public Guid? PositionId { get; set; }
            public Guid? WorkLocationId { get; set; }
        }

        private sealed class PolicyRuntime
        {
            public bool RequireCheckIn { get; set; } = true;
            public bool RequireCheckOut { get; set; } = true;
            public bool AllowMultipleCheckInOut { get; set; }
            public bool AutoCloseOpenAttendance { get; set; }
            public int? AutoCloseAfterMinutes { get; set; }
            public int MinimumWorkMinutes { get; set; }
            public int MaximumWorkMinutes { get; set; } = 1440;
            public bool IsOvertimeEnabled { get; set; } = true;
            public int OvertimeThresholdMinutes { get; set; }
            public bool IsAttendanceLocationRequired { get; set; }
        }

        private sealed class CaptureWindow
        {
            public DateTime StartAt { get; set; }
            public DateTime EndAt { get; set; }
        }

        private sealed class PunchResult
        {
            public HrdAttendanceRawLog? FirstCheckIn { get; set; }
            public HrdAttendanceRawLog? LastCheckOut { get; set; }
            public DateTime? EffectiveStartAt { get; set; }
            public DateTime? EffectiveEndAt { get; set; }
            public List<BreakPair> BreakPairs { get; set; } = new();
            public int BreakMinutes { get; set; }
            public int ActualWorkMinutes { get; set; }
            public int CheckInCount { get; set; }
            public int CheckOutCount { get; set; }
        }

        private sealed class BreakPair
        {
            public HrdAttendanceRawLog Start { get; set; } = null!;
            public HrdAttendanceRawLog End { get; set; } = null!;
            public int Minutes { get; set; }
        }

        private sealed class ExceptionDraft
        {
            public string ExceptionCode { get; set; } = string.Empty;
            public string ExceptionType { get; set; } = AttendanceValueConstants.AttendanceExceptionType.Unknown;
            public string Severity { get; set; } = AttendanceValueConstants.AttendanceExceptionSeverity.Warning;
            public DateTime? ExpectedAt { get; set; }
            public DateTime? ActualAt { get; set; }
            public int? DifferenceMinutes { get; set; }
            public bool IsPayrollBlocking { get; set; }
            public string? DetectionRule { get; set; }
            public string? Message { get; set; }
        }
    }
}
