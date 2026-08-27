using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Repositories;
using System.Data;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services
{
    public class AttendancePayrollHandoffService
    {
        private const int MaximumItemPerExecution = 10000;
        private const int MaximumOptionTake = 200;

        private readonly ApplicationDbContext _dbContext;

        public AttendancePayrollHandoffService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public AttendancePayrollHandoffFilterMetadataResponse GetMetadata()
        {
            return new AttendancePayrollHandoffFilterMetadataResponse
            {
                MaximumItemPerExecution = MaximumItemPerExecution,
                DefaultFilter = new AttendancePayrollHandoffDefaultFilterResponse(),
                ReadinessStatusOptions = new List<AttendancePayrollHandoffStringOptionResponse>
                {
                    Option(AttendancePayrollHandoffValueConstants.ReadinessStatus.Ready, "Siap dikirim"),
                    Option(AttendancePayrollHandoffValueConstants.ReadinessStatus.AlreadyImported, "Sudah dikirim"),
                    Option(AttendancePayrollHandoffValueConstants.ReadinessStatus.MissingPayrollProfile, "Profil payroll belum tersedia"),
                    Option(AttendancePayrollHandoffValueConstants.ReadinessStatus.MissingPayrollRunEmployee, "Employee payroll run belum tersedia"),
                    Option(AttendancePayrollHandoffValueConstants.ReadinessStatus.Unprocessed, "Attendance belum selesai diproses"),
                    Option(AttendancePayrollHandoffValueConstants.ReadinessStatus.PayrollBlocked, "Diblokir untuk payroll"),
                    Option(AttendancePayrollHandoffValueConstants.ReadinessStatus.Locked, "Attendance terkunci"),
                    Option(AttendancePayrollHandoffValueConstants.ReadinessStatus.Excluded, "Dikecualikan dari payroll"),
                    Option(AttendancePayrollHandoffValueConstants.ReadinessStatus.PeriodMismatch, "Periode payroll tidak sesuai"),
                    Option(AttendancePayrollHandoffValueConstants.ReadinessStatus.InvalidWorkforce, "Workforce tidak valid")
                },
                AttendanceStatusOptions = new List<AttendancePayrollHandoffStringOptionResponse>
                {
                    Option(AttendanceValueConstants.AttendanceStatus.Present, "Hadir"),
                    Option(AttendanceValueConstants.AttendanceStatus.Absent, "Tidak hadir"),
                    Option(AttendanceValueConstants.AttendanceStatus.Late, "Terlambat"),
                    Option(AttendanceValueConstants.AttendanceStatus.EarlyLeave, "Pulang awal"),
                    Option(AttendanceValueConstants.AttendanceStatus.Incomplete, "Data belum lengkap"),
                    Option(AttendanceValueConstants.AttendanceStatus.Holiday, "Hari libur"),
                    Option(AttendanceValueConstants.AttendanceStatus.RestDay, "Hari istirahat"),
                    Option(AttendanceValueConstants.AttendanceStatus.Leave, "Cuti/izin"),
                    Option(AttendanceValueConstants.AttendanceStatus.BusinessTrip, "Perjalanan dinas"),
                    Option(AttendanceValueConstants.AttendanceStatus.Remote, "Kerja jarak jauh")
                },
                PayrollInputStatusOptions = new List<AttendancePayrollHandoffStringOptionResponse>
                {
                    Option(AttendanceValueConstants.PayrollInputStatus.Pending, "Menunggu"),
                    Option(AttendanceValueConstants.PayrollInputStatus.Ready, "Siap"),
                    Option(AttendanceValueConstants.PayrollInputStatus.Processed, "Sudah diproses payroll"),
                    Option(AttendanceValueConstants.PayrollInputStatus.Blocked, "Diblokir"),
                    Option(AttendanceValueConstants.PayrollInputStatus.Excluded, "Dikecualikan")
                },
                SortOptions = new List<AttendancePayrollHandoffSortOptionResponse>
                {
                    new() { Value = "attendanceDate", Label = "Tanggal attendance" },
                    new() { Value = "workforceDisplayName", Label = "Nama workforce" },
                    new() { Value = "workforceProfileCode", Label = "Kode workforce" },
                    new() { Value = "departmentName", Label = "Departemen" },
                    new() { Value = "attendanceStatus", Label = "Status attendance" },
                    new() { Value = "payrollInputStatus", Label = "Status input payroll" },
                    new() { Value = "payableWorkMinutes", Label = "Menit kerja dibayar" },
                    new() { Value = "overtimeMinutes", Label = "Menit lembur" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                TerminalPayrollRunStatuses = AttendancePayrollHandoffValueConstants.TerminalPayrollRunStatuses.ToList(),
                HandoffRuleInfo = "Handoff hanya membuat atau memperbarui snapshot TrxPayrollAttendanceInput untuk employee yang sudah tersedia di payroll run.",
                LockRuleInfo = "Attendance daily dikunci setelah snapshot berhasil dibuat. MstPayrollPeriod tidak dikunci oleh endpoint ini karena lock periode payroll merupakan kewenangan proses finalisasi payroll."
            };
        }

        public async Task<List<AttendancePayrollRunOptionResponse>> GetPayrollRunOptionsAsync(
            string? search,
            int take = 100,
            CancellationToken cancellationToken = default)
        {
            take = take <= 0 ? 100 : Math.Min(take, MaximumOptionTake);

            var query =
                from run in _dbContext.Set<TrxPayrollRun>().AsNoTracking()
                join period in _dbContext.Set<MstPayrollPeriod>().AsNoTracking()
                    on run.PayrollPeriodId equals period.Id
                where !run.IsDelete && run.IsActive && !period.IsDelete && period.IsActive
                select new { run, period };

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.run.RunNumber.ToLower().Contains(keyword) ||
                    x.run.RunStatus.ToLower().Contains(keyword) ||
                    x.period.PayrollPeriodCode.ToLower().Contains(keyword) ||
                    x.period.PayrollPeriodName.ToLower().Contains(keyword));
            }

            return await query
                .OrderByDescending(x => x.period.StartDate)
                .ThenByDescending(x => x.run.CreateDateTime)
                .Take(take)
                .Select(x => new AttendancePayrollRunOptionResponse
                {
                    Id = x.run.Id,
                    PayrollPeriodId = x.run.PayrollPeriodId,
                    RunNumber = x.run.RunNumber,
                    RunStatus = x.run.RunStatus,
                    IsLocked = x.run.IsLocked,
                    PayrollPeriodCode = x.period.PayrollPeriodCode,
                    PayrollPeriodName = x.period.PayrollPeriodName,
                    PeriodStartDate = x.period.StartDate,
                    PeriodEndDate = x.period.EndDate,
                    PayrollPeriodStatus = x.period.PayrollPeriodStatus
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<AttendancePayrollHandoffServiceResult<AttendancePayrollHandoffSummaryResponse>> GetSummaryAsync(
            Guid payrollRunId,
            CancellationToken cancellationToken = default)
        {
            var contextResult = await LoadContextAsync(payrollRunId, cancellationToken);
            if (!contextResult.Success || contextResult.Context == null)
            {
                return AttendancePayrollHandoffServiceResult<AttendancePayrollHandoffSummaryResponse>.Fail(
                    contextResult.StatusCode,
                    contextResult.Message);
            }

            var context = contextResult.Context;
            var queries = BuildReadinessQueries(context);
            var baseQuery = BuildAttendanceBaseQuery(context);

            var readyQuery = BuildReadyQuery(baseQuery, context, queries);
            var alreadyImportedQuery = baseQuery.Where(x => queries.ImportedAttendanceDailyIds.Contains(x.Id));

            var totalAttendance = await baseQuery.CountAsync(cancellationToken);
            var readyCount = await readyQuery
                .Where(x => !queries.ImportedAttendanceDailyIds.Contains(x.Id))
                .CountAsync(cancellationToken);
            var alreadyImportedCount = await alreadyImportedQuery.CountAsync(cancellationToken);

            var missingPayrollProfileCount = await baseQuery.CountAsync(x =>
                x.WorkforceProfileId.HasValue &&
                x.ProcessingStatus == AttendanceValueConstants.AttendanceProcessingStatus.Processed &&
                x.IsPayrollEligible &&
                x.PayrollInputStatus != AttendanceValueConstants.PayrollInputStatus.Excluded &&
                !queries.EligiblePayrollProfileIds.Contains(x.WorkforceProfileId.Value),
                cancellationToken);

            var missingRunEmployeeCount = await baseQuery.CountAsync(x =>
                x.WorkforceProfileId.HasValue &&
                x.ProcessingStatus == AttendanceValueConstants.AttendanceProcessingStatus.Processed &&
                x.IsPayrollEligible &&
                queries.EligiblePayrollProfileIds.Contains(x.WorkforceProfileId.Value) &&
                !queries.PayrollRunWorkforceProfileIds.Contains(x.WorkforceProfileId.Value),
                cancellationToken);

            var unprocessedCount = await baseQuery.CountAsync(x =>
                x.ProcessingStatus != AttendanceValueConstants.AttendanceProcessingStatus.Processed,
                cancellationToken);

            var payrollBlockedCount = await baseQuery.CountAsync(x =>
                x.PayrollInputStatus == AttendanceValueConstants.PayrollInputStatus.Blocked ||
                queries.BlockingAttendanceDailyIds.Contains(x.Id),
                cancellationToken);

            var lockedWithoutInputCount = await baseQuery.CountAsync(x =>
                x.IsLocked && !queries.ImportedAttendanceDailyIds.Contains(x.Id),
                cancellationToken);

            var excludedCount = await baseQuery.CountAsync(x =>
                !x.IsPayrollEligible ||
                x.PayrollInputStatus == AttendanceValueConstants.PayrollInputStatus.Excluded,
                cancellationToken);

            var periodMismatchCount = await baseQuery.CountAsync(x =>
                x.PayrollPeriodId.HasValue && x.PayrollPeriodId != context.Period.Id,
                cancellationToken);

            var inputQuery = _dbContext.Set<TrxPayrollAttendanceInput>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && queries.PayrollRunEmployeeIds.Contains(x.PayrollRunEmployeeId));

            var distinctImportedEmployeeCount = await (
                    from input in inputQuery
                    join runEmployee in _dbContext.Set<TrxPayrollRunEmployee>().AsNoTracking()
                        on input.PayrollRunEmployeeId equals runEmployee.Id
                    select runEmployee.WorkforceProfileId)
                .Distinct()
                .CountAsync(cancellationToken);

            var blockingReasons = BuildContextBlockingReasons(context);
            if (readyCount == 0)
            {
                blockingReasons.Add("Tidak ada attendance yang berstatus siap untuk handoff.");
            }

            var response = new AttendancePayrollHandoffSummaryResponse
            {
                PayrollRunId = context.Run.Id,
                RunNumber = context.Run.RunNumber,
                RunStatus = context.Run.RunStatus,
                IsPayrollRunLocked = context.Run.IsLocked,
                PayrollPeriodId = context.Period.Id,
                PayrollPeriodCode = context.Period.PayrollPeriodCode,
                PayrollPeriodName = context.Period.PayrollPeriodName,
                PeriodStartDate = context.StartDate,
                PeriodEndDate = context.EndDate,
                PayrollPeriodStatus = context.Period.PayrollPeriodStatus,
                IsPayrollPeriodLocked = context.Period.IsLocked,
                TotalAttendanceDaily = totalAttendance,
                ReadyForHandoff = readyCount,
                AlreadyImported = alreadyImportedCount,
                MissingPayrollProfile = missingPayrollProfileCount,
                MissingPayrollRunEmployee = missingRunEmployeeCount,
                UnprocessedAttendance = unprocessedCount,
                PayrollBlockedAttendance = payrollBlockedCount,
                LockedWithoutInput = lockedWithoutInputCount,
                ExcludedAttendance = excludedCount,
                PeriodMismatch = periodMismatchCount,
                CorrectedAttendance = await baseQuery.CountAsync(x => x.IsCorrected, cancellationToken),
                PayrollAttendanceInputCount = await inputQuery.CountAsync(cancellationToken),
                DistinctImportedEmployeeCount = distinctImportedEmployeeCount,
                CanExecute = blockingReasons.Count == 0 && readyCount > 0,
                BlockingReasons = blockingReasons
            };

            return AttendancePayrollHandoffServiceResult<AttendancePayrollHandoffSummaryResponse>.Ok(
                response,
                "Ringkasan kesiapan attendance payroll handoff berhasil diambil.");
        }

        public async Task<AttendancePayrollHandoffServiceResult<AttendancePayrollHandoffPreviewPagedResponse>> GetPreviewAsync(
            Guid payrollRunId,
            AttendancePayrollHandoffQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var contextResult = await LoadContextAsync(payrollRunId, cancellationToken);
            if (!contextResult.Success || contextResult.Context == null)
            {
                return AttendancePayrollHandoffServiceResult<AttendancePayrollHandoffPreviewPagedResponse>.Fail(
                    contextResult.StatusCode,
                    contextResult.Message);
            }

            var context = contextResult.Context;
            var queries = BuildReadinessQueries(context);
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, 100);

            IQueryable<HrdAttendanceDaily> query = BuildAttendanceBaseQuery(context)
                .Include(x => x.WorkforceProfile)
                .Include(x => x.Department);

            query = ApplyStandardFilters(query, request);
            query = ApplyReadinessFilter(query, request.ReadinessStatus, context, queries);

            var totalData = await query.CountAsync(cancellationToken);
            var entities = await ApplySorting(query, request.SortBy, request.SortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var support = await LoadSupportDataAsync(context, entities, cancellationToken);
            var items = entities
                .Select(x => MapPreviewItem(x, context, support))
                .ToList();

            return AttendancePayrollHandoffServiceResult<AttendancePayrollHandoffPreviewPagedResponse>.Ok(
                new AttendancePayrollHandoffPreviewPagedResponse
                {
                    PayrollRunId = context.Run.Id,
                    RunNumber = context.Run.RunNumber,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Preview attendance payroll handoff berhasil diambil.");
        }

        public async Task<AttendancePayrollHandoffServiceResult<AttendancePayrollHandoffExecutionResponse>> ExecuteAsync(
            Guid payrollRunId,
            ExecuteAttendancePayrollHandoffRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty)
            {
                return AttendancePayrollHandoffServiceResult<AttendancePayrollHandoffExecutionResponse>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Identitas user login tidak valid.");
            }

            if (request.AttendanceDailyIds != null && request.AttendanceDailyIds.Count > MaximumItemPerExecution)
            {
                return AttendancePayrollHandoffServiceResult<AttendancePayrollHandoffExecutionResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Maksimal {MaximumItemPerExecution} attendance per proses handoff.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                var contextResult = await LoadContextAsync(payrollRunId, cancellationToken, tracking: true);
                if (!contextResult.Success || contextResult.Context == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return AttendancePayrollHandoffServiceResult<AttendancePayrollHandoffExecutionResponse>.Fail(
                        contextResult.StatusCode,
                        contextResult.Message);
                }

                var context = contextResult.Context;
                var contextBlockingReasons = BuildContextBlockingReasons(context);
                if (contextBlockingReasons.Count > 0)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return AttendancePayrollHandoffServiceResult<AttendancePayrollHandoffExecutionResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        string.Join(" ", contextBlockingReasons));
                }

                IQueryable<HrdAttendanceDaily> targetQuery = BuildAttendanceBaseQuery(context)
                    .Include(x => x.WorkforceProfile)
                    .Include(x => x.Department);

                if (request.AttendanceDailyIds != null && request.AttendanceDailyIds.Count > 0)
                {
                    var ids = request.AttendanceDailyIds
                        .Where(x => x != Guid.Empty)
                        .Distinct()
                        .ToList();
                    targetQuery = targetQuery.Where(x => ids.Contains(x.Id));
                }

                var targetCount = await targetQuery.CountAsync(cancellationToken);
                if (targetCount == 0)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return AttendancePayrollHandoffServiceResult<AttendancePayrollHandoffExecutionResponse>.Fail(
                        StatusCodes.Status404NotFound,
                        "Tidak ada attendance yang ditemukan untuk payroll run tersebut.");
                }

                if (targetCount > MaximumItemPerExecution)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return AttendancePayrollHandoffServiceResult<AttendancePayrollHandoffExecutionResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        $"Jumlah target {targetCount} melebihi batas {MaximumItemPerExecution}. Kirim AttendanceDailyIds secara bertahap.");
                }

                var targets = await targetQuery
                    .OrderBy(x => x.AttendanceDate)
                    .ThenBy(x => x.WorkforceProfileId)
                    .ToListAsync(cancellationToken);

                var support = await LoadSupportDataAsync(context, targets, cancellationToken, trackingInputs: true);
                var startedAt = DateTime.UtcNow;
                var executionItems = new List<AttendancePayrollHandoffExecutionItemResponse>(targets.Count);
                var validItems = new List<(HrdAttendanceDaily Daily, TrxPayrollRunEmployee RunEmployee, TrxPayrollAttendanceInput? ExistingInput)>();

                foreach (var daily in targets)
                {
                    var evaluation = EvaluateReadiness(daily, context, support);
                    var profile = daily.WorkforceProfile;
                    var previousStatus = daily.PayrollInputStatus;

                    if (!evaluation.IsReady)
                    {
                        executionItems.Add(new AttendancePayrollHandoffExecutionItemResponse
                        {
                            AttendanceDailyId = daily.Id,
                            WorkforceProfileId = daily.WorkforceProfileId,
                            WorkforceProfileCode = profile?.ProfileCode,
                            WorkforceDisplayName = profile?.DisplayName,
                            AttendanceDate = daily.AttendanceDate,
                            PayrollRunEmployeeId = evaluation.RunEmployee?.Id,
                            PayrollAttendanceInputId = evaluation.ExistingInput?.Id,
                            Success = false,
                            PreviousPayrollInputStatus = previousStatus,
                            CurrentPayrollInputStatus = previousStatus,
                            ResultStatus = AttendancePayrollHandoffValueConstants.ExecutionResultStatus.ValidationFailed,
                            Message = evaluation.Reasons.FirstOrDefault()?.Message ?? "Attendance belum memenuhi aturan payroll handoff.",
                            Reasons = evaluation.Reasons
                        });
                        continue;
                    }

                    if (evaluation.RunEmployee == null)
                    {
                        continue;
                    }

                    validItems.Add((daily, evaluation.RunEmployee, evaluation.ExistingInput));
                }

                if (!request.ContinueOnValidationError && executionItems.Any(x => !x.Success))
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return AttendancePayrollHandoffServiceResult<AttendancePayrollHandoffExecutionResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        "Handoff dibatalkan karena terdapat attendance yang belum memenuhi validasi dan ContinueOnValidationError bernilai false.");
                }

                foreach (var item in validItems)
                {
                    var daily = item.Daily;
                    var existingInput = item.ExistingInput;
                    var previousStatus = daily.PayrollInputStatus;
                    var profile = daily.WorkforceProfile;

                    if (existingInput != null &&
                        !request.ForceRefreshExistingInput &&
                        existingInput.AttendanceDailyId == daily.Id)
                    {
                        daily.PayrollPeriodId = context.Period.Id;
                        daily.PayrollInputStatus = AttendanceValueConstants.PayrollInputStatus.Processed;
                        daily.PayrollProcessedAt ??= existingInput.ImportedAt;
                        daily.IsLocked = true;
                        daily.UpdateDateTime = startedAt;
                        daily.UpdateBy = actorUserId;

                        executionItems.Add(new AttendancePayrollHandoffExecutionItemResponse
                        {
                            AttendanceDailyId = daily.Id,
                            WorkforceProfileId = daily.WorkforceProfileId,
                            WorkforceProfileCode = profile?.ProfileCode,
                            WorkforceDisplayName = profile?.DisplayName,
                            AttendanceDate = daily.AttendanceDate,
                            PayrollRunEmployeeId = item.RunEmployee.Id,
                            PayrollAttendanceInputId = existingInput.Id,
                            Success = true,
                            IsIdempotent = true,
                            PreviousPayrollInputStatus = previousStatus,
                            CurrentPayrollInputStatus = daily.PayrollInputStatus,
                            ResultStatus = AttendancePayrollHandoffValueConstants.ExecutionResultStatus.Idempotent,
                            Message = "Snapshot payroll attendance sudah tersedia. Status attendance diselaraskan tanpa membuat data duplikat."
                        });
                        continue;
                    }

                    var input = existingInput ?? new TrxPayrollAttendanceInput
                    {
                        Id = Guid.NewGuid(),
                        PayrollRunEmployeeId = item.RunEmployee.Id,
                        CreateDateTime = startedAt,
                        CreateBy = actorUserId,
                        UpdateBy = actorUserId,
                        IsActive = true
                    };

                    ApplySnapshot(input, daily, item.RunEmployee.Id, actorUserId, startedAt, request.Notes);

                    if (existingInput == null)
                    {
                        _dbContext.Set<TrxPayrollAttendanceInput>().Add(input);
                    }

                    daily.PayrollPeriodId = context.Period.Id;
                    daily.PayrollInputStatus = AttendanceValueConstants.PayrollInputStatus.Processed;
                    daily.PayrollProcessedAt = startedAt;
                    daily.IsLocked = true;
                    daily.UpdateDateTime = startedAt;
                    daily.UpdateBy = actorUserId;

                    executionItems.Add(new AttendancePayrollHandoffExecutionItemResponse
                    {
                        AttendanceDailyId = daily.Id,
                        WorkforceProfileId = daily.WorkforceProfileId,
                        WorkforceProfileCode = profile?.ProfileCode,
                        WorkforceDisplayName = profile?.DisplayName,
                        AttendanceDate = daily.AttendanceDate,
                        PayrollRunEmployeeId = item.RunEmployee.Id,
                        PayrollAttendanceInputId = input.Id,
                        Success = true,
                        IsCreated = existingInput == null,
                        IsUpdated = existingInput != null,
                        PreviousPayrollInputStatus = previousStatus,
                        CurrentPayrollInputStatus = daily.PayrollInputStatus,
                        ResultStatus = existingInput == null
                            ? AttendancePayrollHandoffValueConstants.ExecutionResultStatus.Created
                            : AttendancePayrollHandoffValueConstants.ExecutionResultStatus.Updated,
                        Message = existingInput == null
                            ? "Snapshot payroll attendance berhasil dibuat."
                            : "Snapshot payroll attendance berhasil diperbarui."
                    });
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                var completedAt = DateTime.UtcNow;
                var failedCount = executionItems.Count(x => !x.Success);
                var response = new AttendancePayrollHandoffExecutionResponse
                {
                    PayrollRunId = context.Run.Id,
                    RunNumber = context.Run.RunNumber,
                    PayrollPeriodId = context.Period.Id,
                    PayrollPeriodCode = context.Period.PayrollPeriodCode,
                    HandoffStatus = failedCount == 0
                        ? AttendancePayrollHandoffValueConstants.HandoffStatus.Completed
                        : AttendancePayrollHandoffValueConstants.HandoffStatus.CompletedWithErrors,
                    StartedAt = startedAt,
                    CompletedAt = completedAt,
                    TotalTarget = targets.Count,
                    CreatedCount = executionItems.Count(x => x.IsCreated),
                    UpdatedCount = executionItems.Count(x => x.IsUpdated),
                    IdempotentCount = executionItems.Count(x => x.IsIdempotent),
                    FailedCount = failedCount,
                    LockedAttendanceCount = executionItems.Count(x => x.Success),
                    Items = executionItems
                        .OrderBy(x => x.AttendanceDate)
                        .ThenBy(x => x.WorkforceDisplayName)
                        .ToList()
                };

                return AttendancePayrollHandoffServiceResult<AttendancePayrollHandoffExecutionResponse>.Ok(
                    response,
                    failedCount == 0
                        ? "Attendance payroll handoff berhasil diselesaikan."
                        : "Attendance payroll handoff selesai dengan sebagian item gagal validasi.");
            }
            catch (OperationCanceledException)
            {
                await transaction.RollbackAsync(CancellationToken.None);
                throw;
            }
            catch (DbUpdateException exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                return AttendancePayrollHandoffServiceResult<AttendancePayrollHandoffExecutionResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Gagal menyimpan payroll attendance input. Periksa kemungkinan proses paralel atau data duplikat. {LimitMessage(exception.GetBaseException().Message, 500)}");
            }
            catch (Exception exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                return AttendancePayrollHandoffServiceResult<AttendancePayrollHandoffExecutionResponse>.Fail(
                    StatusCodes.Status500InternalServerError,
                    $"Attendance payroll handoff gagal diproses. {LimitMessage(exception.GetBaseException().Message, 500)}");
            }
        }

        public async Task<AttendancePayrollHandoffServiceResult<AttendancePayrollHandoffReconciliationResponse>> GetReconciliationAsync(
            Guid payrollRunId,
            AttendancePayrollHandoffReconciliationQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var contextResult = await LoadContextAsync(payrollRunId, cancellationToken);
            if (!contextResult.Success || contextResult.Context == null)
            {
                return AttendancePayrollHandoffServiceResult<AttendancePayrollHandoffReconciliationResponse>.Fail(
                    contextResult.StatusCode,
                    contextResult.Message);
            }

            var context = contextResult.Context;
            var runEmployees = await _dbContext.Set<TrxPayrollRunEmployee>()
                .AsNoTracking()
                .Where(x => x.PayrollRunId == payrollRunId && !x.IsDelete)
                .Select(x => new { x.Id, x.WorkforceProfileId })
                .ToListAsync(cancellationToken);

            var runEmployeeIds = runEmployees.Select(x => x.Id).ToList();
            var runEmployeeProfileMap = runEmployees.ToDictionary(x => x.Id, x => x.WorkforceProfileId);

            var attendanceRows = await BuildAttendanceBaseQuery(context)
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Where(x =>
                    x.PayrollInputStatus == AttendanceValueConstants.PayrollInputStatus.Processed ||
                    x.IsLocked)
                .ToListAsync(cancellationToken);

            var inputRows = await _dbContext.Set<TrxPayrollAttendanceInput>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && runEmployeeIds.Contains(x.PayrollRunEmployeeId))
                .ToListAsync(cancellationToken);

            var attendanceById = attendanceRows.ToDictionary(x => x.Id);
            var linkedAttendanceDailyIds = inputRows
                .Where(x => x.AttendanceDailyId.HasValue)
                .Select(x => x.AttendanceDailyId!.Value)
                .Distinct()
                .ToList();

            var linkedAttendanceRows = await _dbContext.Set<HrdAttendanceDaily>()
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Where(x => linkedAttendanceDailyIds.Contains(x.Id) && !x.IsDelete)
                .ToListAsync(cancellationToken);
            var linkedAttendanceById = linkedAttendanceRows.ToDictionary(x => x.Id);

            var inputByAttendanceId = inputRows
                .Where(x => x.AttendanceDailyId.HasValue)
                .GroupBy(x => x.AttendanceDailyId!.Value)
                .ToDictionary(x => x.Key, x => x.OrderByDescending(y => y.ImportedAt).First());

            var issues = new List<AttendancePayrollHandoffReconciliationItemResponse>();
            var matchedCount = 0;
            var changedCount = 0;
            var missingCount = 0;
            var orphanCount = 0;
            var outsidePeriodCount = 0;

            foreach (var daily in attendanceRows)
            {
                if (!inputByAttendanceId.TryGetValue(daily.Id, out var input))
                {
                    missingCount++;
                    issues.Add(BuildMissingInputIssue(daily));
                    continue;
                }

                if (HasSnapshotChanged(daily, input))
                {
                    changedCount++;
                    issues.Add(BuildChangedInputIssue(daily, input));
                }
                else
                {
                    matchedCount++;
                }
            }

            foreach (var input in inputRows)
            {
                if (!input.AttendanceDailyId.HasValue ||
                    !linkedAttendanceById.TryGetValue(input.AttendanceDailyId.Value, out var linkedDaily))
                {
                    orphanCount++;
                    issues.Add(BuildOrphanInputIssue(input, runEmployeeProfileMap));
                    continue;
                }

                if (linkedDaily.AttendanceDate < context.StartDate || linkedDaily.AttendanceDate > context.EndDate)
                {
                    outsidePeriodCount++;
                    issues.Add(BuildOutsidePeriodIssue(linkedDaily, input));
                    continue;
                }

                if (!attendanceById.ContainsKey(linkedDaily.Id))
                {
                    orphanCount++;
                    issues.Add(BuildOrphanInputIssue(input, runEmployeeProfileMap));
                }
            }

            if (request.WorkforceProfileId.HasValue && request.WorkforceProfileId.Value != Guid.Empty)
            {
                issues = issues
                    .Where(x => x.WorkforceProfileId == request.WorkforceProfileId.Value)
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(request.IssueType))
            {
                issues = issues
                    .Where(x => string.Equals(x.IssueType, request.IssueType.Trim(), StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim();
                issues = issues.Where(x =>
                        ContainsIgnoreCase(x.WorkforceProfileCode, keyword) ||
                        ContainsIgnoreCase(x.WorkforceDisplayName, keyword) ||
                        ContainsIgnoreCase(x.AttendanceStatus, keyword) ||
                        ContainsIgnoreCase(x.AttendanceStatusSnapshot, keyword) ||
                        ContainsIgnoreCase(x.Message, keyword))
                    .ToList();
            }

            issues = ApplyReconciliationSorting(issues, request.SortBy, request.SortDirection);

            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, 100);
            var totalIssue = issues.Count;
            var pagedIssues = issues
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new AttendancePayrollHandoffReconciliationResponse
            {
                PayrollRunId = context.Run.Id,
                RunNumber = context.Run.RunNumber,
                PayrollPeriodId = context.Period.Id,
                PayrollPeriodCode = context.Period.PayrollPeriodCode,
                ExpectedAttendanceCount = attendanceRows.Count,
                PayrollAttendanceInputCount = inputRows.Count,
                MatchedCount = matchedCount,
                MissingInputCount = missingCount,
                ChangedAfterImportCount = changedCount,
                OrphanInputCount = orphanCount,
                OutsidePeriodInputCount = outsidePeriodCount,
                IsBalanced = missingCount == 0 && changedCount == 0 && orphanCount == 0 && outsidePeriodCount == 0,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalIssue = totalIssue,
                TotalPage = (int)Math.Ceiling(totalIssue / (double)pageSize),
                Items = pagedIssues
            };

            return AttendancePayrollHandoffServiceResult<AttendancePayrollHandoffReconciliationResponse>.Ok(
                response,
                response.IsBalanced
                    ? "Rekonsiliasi attendance payroll handoff seimbang."
                    : "Rekonsiliasi menemukan perbedaan yang perlu ditindaklanjuti.");
        }

        public async Task<AttendancePayrollHandoffServiceResult<AttendancePayrollHandoffExecutionResponse>> RepairAsync(
            Guid payrollRunId,
            RepairAttendancePayrollHandoffRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (!request.CreateMissingInput && !request.RefreshChangedInput)
            {
                return AttendancePayrollHandoffServiceResult<AttendancePayrollHandoffExecutionResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Minimal satu opsi repair harus diaktifkan.");
            }

            var reconciliation = await GetReconciliationAsync(
                payrollRunId,
                new AttendancePayrollHandoffReconciliationQueryRequest
                {
                    PageNumber = 1,
                    PageSize = 100
                },
                cancellationToken);

            if (!reconciliation.Success || reconciliation.Data == null)
            {
                return AttendancePayrollHandoffServiceResult<AttendancePayrollHandoffExecutionResponse>.Fail(
                    reconciliation.StatusCode,
                    reconciliation.Message);
            }

            var contextResult = await LoadContextAsync(payrollRunId, cancellationToken);
            if (!contextResult.Success || contextResult.Context == null)
            {
                return AttendancePayrollHandoffServiceResult<AttendancePayrollHandoffExecutionResponse>.Fail(
                    contextResult.StatusCode,
                    contextResult.Message);
            }

            var context = contextResult.Context;
            var runEmployeeIds = _dbContext.Set<TrxPayrollRunEmployee>()
                .Where(x => x.PayrollRunId == payrollRunId && !x.IsDelete)
                .Select(x => x.Id);

            var importedDailyIds = _dbContext.Set<TrxPayrollAttendanceInput>()
                .Where(x => !x.IsDelete &&
                            runEmployeeIds.Contains(x.PayrollRunEmployeeId) &&
                            x.AttendanceDailyId.HasValue)
                .Select(x => x.AttendanceDailyId!.Value);

            var repairIds = new List<Guid>();

            if (request.CreateMissingInput)
            {
                var missingIds = await BuildAttendanceBaseQuery(context)
                    .Where(x =>
                        x.PayrollInputStatus == AttendanceValueConstants.PayrollInputStatus.Processed &&
                        !importedDailyIds.Contains(x.Id))
                    .Select(x => x.Id)
                    .Take(MaximumItemPerExecution)
                    .ToListAsync(cancellationToken);
                repairIds.AddRange(missingIds);
            }

            if (request.RefreshChangedInput)
            {
                var inputRows = await _dbContext.Set<TrxPayrollAttendanceInput>()
                    .AsNoTracking()
                    .Where(x => !x.IsDelete &&
                                runEmployeeIds.Contains(x.PayrollRunEmployeeId) &&
                                x.AttendanceDailyId.HasValue)
                    .ToListAsync(cancellationToken);

                var dailyIds = inputRows.Select(x => x.AttendanceDailyId!.Value).Distinct().ToList();
                var dailies = await _dbContext.Set<HrdAttendanceDaily>()
                    .AsNoTracking()
                    .Where(x => dailyIds.Contains(x.Id) && !x.IsDelete)
                    .ToListAsync(cancellationToken);
                var dailyMap = dailies.ToDictionary(x => x.Id);

                foreach (var input in inputRows)
                {
                    if (input.AttendanceDailyId.HasValue &&
                        dailyMap.TryGetValue(input.AttendanceDailyId.Value, out var daily) &&
                        HasSnapshotChanged(daily, input))
                    {
                        repairIds.Add(daily.Id);
                    }
                }
            }

            repairIds = repairIds.Distinct().Take(MaximumItemPerExecution).ToList();
            if (repairIds.Count == 0)
            {
                return AttendancePayrollHandoffServiceResult<AttendancePayrollHandoffExecutionResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Tidak ditemukan payroll attendance input yang perlu diperbaiki.");
            }

            return await ExecuteAsync(
                payrollRunId,
                new ExecuteAttendancePayrollHandoffRequest
                {
                    AttendanceDailyIds = repairIds,
                    ForceRefreshExistingInput = true,
                    ContinueOnValidationError = true,
                    Notes = string.IsNullOrWhiteSpace(request.Notes)
                        ? "Repair attendance payroll handoff."
                        : request.Notes
                },
                actorUserId,
                cancellationToken);
        }

        public async Task<AttendancePayrollHandoffServiceResult<AttendancePayrollHandoffRollbackResponse>> RollbackAsync(
            Guid payrollRunId,
            RollbackAttendancePayrollHandoffRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty)
            {
                return AttendancePayrollHandoffServiceResult<AttendancePayrollHandoffRollbackResponse>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Identitas user login tidak valid.");
            }

            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return AttendancePayrollHandoffServiceResult<AttendancePayrollHandoffRollbackResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan rollback wajib diisi.");
            }

            if (request.AttendanceDailyIds != null && request.AttendanceDailyIds.Count > MaximumItemPerExecution)
            {
                return AttendancePayrollHandoffServiceResult<AttendancePayrollHandoffRollbackResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Maksimal {MaximumItemPerExecution} attendance per rollback.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                var contextResult = await LoadContextAsync(payrollRunId, cancellationToken, tracking: true);
                if (!contextResult.Success || contextResult.Context == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return AttendancePayrollHandoffServiceResult<AttendancePayrollHandoffRollbackResponse>.Fail(
                        contextResult.StatusCode,
                        contextResult.Message);
                }

                var context = contextResult.Context;
                if (context.Run.IsLocked || context.Period.IsLocked ||
                    AttendancePayrollHandoffValueConstants.TerminalPayrollRunStatuses.Contains(context.Run.RunStatus, StringComparer.OrdinalIgnoreCase) ||
                    AttendancePayrollHandoffValueConstants.TerminalPayrollPeriodStatuses.Contains(context.Period.PayrollPeriodStatus, StringComparer.OrdinalIgnoreCase))
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return AttendancePayrollHandoffServiceResult<AttendancePayrollHandoffRollbackResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        "Rollback tidak diizinkan karena payroll run atau payroll period sudah dikunci/final.");
                }

                var runEmployeeIds = await _dbContext.Set<TrxPayrollRunEmployee>()
                    .Where(x => x.PayrollRunId == payrollRunId && !x.IsDelete)
                    .Select(x => x.Id)
                    .ToListAsync(cancellationToken);

                var inputQuery = _dbContext.Set<TrxPayrollAttendanceInput>()
                    .Where(x => !x.IsDelete && runEmployeeIds.Contains(x.PayrollRunEmployeeId));

                if (request.AttendanceDailyIds != null && request.AttendanceDailyIds.Count > 0)
                {
                    var ids = request.AttendanceDailyIds
                        .Where(x => x != Guid.Empty)
                        .Distinct()
                        .ToList();
                    inputQuery = inputQuery.Where(x => x.AttendanceDailyId.HasValue && ids.Contains(x.AttendanceDailyId.Value));
                }

                var inputs = await inputQuery
                    .Take(MaximumItemPerExecution + 1)
                    .ToListAsync(cancellationToken);

                if (inputs.Count == 0)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return AttendancePayrollHandoffServiceResult<AttendancePayrollHandoffRollbackResponse>.Fail(
                        StatusCodes.Status404NotFound,
                        "Payroll attendance input yang akan di-rollback tidak ditemukan.");
                }

                if (inputs.Count > MaximumItemPerExecution)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return AttendancePayrollHandoffServiceResult<AttendancePayrollHandoffRollbackResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        $"Jumlah input melebihi batas {MaximumItemPerExecution}. Lakukan rollback secara bertahap.");
                }

                var now = DateTime.UtcNow;
                var dailyIds = inputs
                    .Where(x => x.AttendanceDailyId.HasValue)
                    .Select(x => x.AttendanceDailyId!.Value)
                    .Distinct()
                    .ToList();
                var rollbackInputIds = inputs.Select(x => x.Id).ToList();

                foreach (var input in inputs)
                {
                    input.IsDelete = true;
                    input.IsActive = false;
                    input.DeleteDateTime = now;
                    input.DeleteBy = actorUserId;
                    input.UpdateDateTime = now;
                    input.UpdateBy = actorUserId;
                    input.Notes = LimitMessage($"Rollback: {request.Reason}", 1000);
                }

                var dailies = await _dbContext.Set<HrdAttendanceDaily>()
                    .Where(x => dailyIds.Contains(x.Id) && !x.IsDelete)
                    .ToListAsync(cancellationToken);

                var stillReferencedDailyIds = await _dbContext.Set<TrxPayrollAttendanceInput>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.AttendanceDailyId.HasValue &&
                        dailyIds.Contains(x.AttendanceDailyId.Value) &&
                        !rollbackInputIds.Contains(x.Id))
                    .Select(x => x.AttendanceDailyId!.Value)
                    .Distinct()
                    .ToListAsync(cancellationToken);
                var stillReferencedSet = stillReferencedDailyIds.ToHashSet();

                var reopenedCount = 0;
                foreach (var daily in dailies)
                {
                    if (stillReferencedSet.Contains(daily.Id))
                    {
                        continue;
                    }

                    daily.PayrollPeriodId = null;
                    daily.PayrollInputStatus = AttendanceValueConstants.PayrollInputStatus.Ready;
                    daily.PayrollProcessedAt = null;
                    daily.IsLocked = daily.AttendancePeriodId.HasValue;
                    daily.UpdateDateTime = now;
                    daily.UpdateBy = actorUserId;
                    reopenedCount++;
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return AttendancePayrollHandoffServiceResult<AttendancePayrollHandoffRollbackResponse>.Ok(
                    new AttendancePayrollHandoffRollbackResponse
                    {
                        PayrollRunId = context.Run.Id,
                        RunNumber = context.Run.RunNumber,
                        RolledBackInputCount = inputs.Count,
                        ReopenedAttendanceCount = reopenedCount,
                        AttendanceStillReferencedCount = dailies.Count - reopenedCount,
                        RolledBackAt = now,
                        RolledBackByUserId = actorUserId,
                        Reason = request.Reason.Trim()
                    },
                    "Attendance payroll handoff berhasil di-rollback.");
            }
            catch (OperationCanceledException)
            {
                await transaction.RollbackAsync(CancellationToken.None);
                throw;
            }
            catch (Exception exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                return AttendancePayrollHandoffServiceResult<AttendancePayrollHandoffRollbackResponse>.Fail(
                    StatusCodes.Status500InternalServerError,
                    $"Rollback attendance payroll handoff gagal. {LimitMessage(exception.GetBaseException().Message, 500)}");
            }
        }

        private async Task<ContextLoadResult> LoadContextAsync(
            Guid payrollRunId,
            CancellationToken cancellationToken,
            bool tracking = false)
        {
            if (payrollRunId == Guid.Empty)
            {
                return ContextLoadResult.Fail(StatusCodes.Status400BadRequest, "PayrollRunId wajib diisi.");
            }

            IQueryable<TrxPayrollRun> runQuery = _dbContext.Set<TrxPayrollRun>();
            IQueryable<MstPayrollPeriod> periodQuery = _dbContext.Set<MstPayrollPeriod>();
            if (!tracking)
            {
                runQuery = runQuery.AsNoTracking();
                periodQuery = periodQuery.AsNoTracking();
            }

            var run = await runQuery.FirstOrDefaultAsync(x => x.Id == payrollRunId && !x.IsDelete, cancellationToken);
            if (run == null)
            {
                return ContextLoadResult.Fail(StatusCodes.Status404NotFound, "Payroll run tidak ditemukan.");
            }

            var period = await periodQuery.FirstOrDefaultAsync(x => x.Id == run.PayrollPeriodId && !x.IsDelete, cancellationToken);
            if (period == null)
            {
                return ContextLoadResult.Fail(StatusCodes.Status409Conflict, "Payroll period untuk payroll run tersebut tidak ditemukan.");
            }

            return ContextLoadResult.Ok(new PayrollRunContext
            {
                Run = run,
                Period = period,
                StartDate = DateOnly.FromDateTime(period.StartDate),
                EndDate = DateOnly.FromDateTime(period.EndDate)
            });
        }

        private IQueryable<HrdAttendanceDaily> BuildAttendanceBaseQuery(PayrollRunContext context)
        {
            var query = _dbContext.Set<HrdAttendanceDaily>()
                .Where(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    x.AttendanceDate >= context.StartDate &&
                    x.AttendanceDate <= context.EndDate);

            if (context.Run.HospitalSiteId.HasValue)
            {
                query = query.Where(x => x.HospitalSiteId == context.Run.HospitalSiteId.Value);
            }

            return query;
        }

        private ReadinessQueries BuildReadinessQueries(PayrollRunContext context)
        {
            var runEmployees = _dbContext.Set<TrxPayrollRunEmployee>()
                .AsNoTracking()
                .Where(x => x.PayrollRunId == context.Run.Id && !x.IsDelete);

            var runEmployeeIds = runEmployees.Select(x => x.Id);
            var runWorkforceProfileIds = runEmployees.Select(x => x.WorkforceProfileId);

            var eligiblePayrollProfileIds = _dbContext.Set<WfpPayroll>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.IsPayrollEligible)
                .Select(x => x.WorkforceProfileId);

            var blockingDailyIds = _dbContext.Set<HrdAttendanceException>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    x.IsPayrollBlocking &&
                    x.ExceptionStatus != "Closed" &&
                    x.ExceptionStatus != "Corrected" &&
                    x.ExceptionStatus != "Waived")
                .Select(x => x.AttendanceDailyId);

            var importedDailyIds = _dbContext.Set<TrxPayrollAttendanceInput>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    runEmployeeIds.Contains(x.PayrollRunEmployeeId) &&
                    x.AttendanceDailyId.HasValue)
                .Select(x => x.AttendanceDailyId!.Value);

            return new ReadinessQueries
            {
                PayrollRunEmployeeIds = runEmployeeIds,
                PayrollRunWorkforceProfileIds = runWorkforceProfileIds,
                EligiblePayrollProfileIds = eligiblePayrollProfileIds,
                BlockingAttendanceDailyIds = blockingDailyIds,
                ImportedAttendanceDailyIds = importedDailyIds
            };
        }

        private static IQueryable<HrdAttendanceDaily> BuildReadyQuery(
            IQueryable<HrdAttendanceDaily> query,
            PayrollRunContext context,
            ReadinessQueries readiness)
        {
            return query.Where(x =>
                x.WorkforceProfileId.HasValue &&
                x.ProcessingStatus == AttendanceValueConstants.AttendanceProcessingStatus.Processed &&
                x.IsPayrollEligible &&
                x.PayrollInputStatus != AttendanceValueConstants.PayrollInputStatus.Excluded &&
                x.PayrollInputStatus != AttendanceValueConstants.PayrollInputStatus.Blocked &&
                (x.PayrollInputStatus == AttendanceValueConstants.PayrollInputStatus.Ready ||
                 x.PayrollInputStatus == AttendanceValueConstants.PayrollInputStatus.Processed) &&
                (!x.PayrollPeriodId.HasValue || x.PayrollPeriodId == context.Period.Id) &&
                readiness.EligiblePayrollProfileIds.Contains(x.WorkforceProfileId.Value) &&
                readiness.PayrollRunWorkforceProfileIds.Contains(x.WorkforceProfileId.Value) &&
                !readiness.BlockingAttendanceDailyIds.Contains(x.Id) &&
                (!x.IsLocked || readiness.ImportedAttendanceDailyIds.Contains(x.Id)));
        }

        private static IQueryable<HrdAttendanceDaily> ApplyStandardFilters(
            IQueryable<HrdAttendanceDaily> query,
            AttendancePayrollHandoffQueryRequest request)
        {
            if (request.WorkforceProfileId.HasValue && request.WorkforceProfileId.Value != Guid.Empty)
            {
                query = query.Where(x => x.WorkforceProfileId == request.WorkforceProfileId.Value);
            }

            if (request.DepartmentId.HasValue && request.DepartmentId.Value != Guid.Empty)
            {
                query = query.Where(x => x.DepartmentId == request.DepartmentId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.AttendanceStatus))
            {
                var value = request.AttendanceStatus.Trim();
                query = query.Where(x => x.AttendanceStatus == value);
            }

            if (!string.IsNullOrWhiteSpace(request.PayrollInputStatus))
            {
                var value = request.PayrollInputStatus.Trim();
                query = query.Where(x => x.PayrollInputStatus == value);
            }

            if (request.IsCorrected.HasValue)
            {
                query = query.Where(x => x.IsCorrected == request.IsCorrected.Value);
            }

            if (request.IsLocked.HasValue)
            {
                query = query.Where(x => x.IsLocked == request.IsLocked.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim().ToLower();
                query = query.Where(x =>
                    x.AttendanceStatus.ToLower().Contains(keyword) ||
                    x.ProcessingStatus.ToLower().Contains(keyword) ||
                    x.PayrollInputStatus.ToLower().Contains(keyword) ||
                    (x.WorkforceProfile != null && x.WorkforceProfile.ProfileCode.ToLower().Contains(keyword)) ||
                    (x.WorkforceProfile != null && x.WorkforceProfile.DisplayName.ToLower().Contains(keyword)) ||
                    (x.Department != null && x.Department.DepartmentName.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IQueryable<HrdAttendanceDaily> ApplyReadinessFilter(
            IQueryable<HrdAttendanceDaily> query,
            string? readinessStatus,
            PayrollRunContext context,
            ReadinessQueries readiness)
        {
            if (string.IsNullOrWhiteSpace(readinessStatus))
            {
                return query;
            }

            return readinessStatus.Trim() switch
            {
                AttendancePayrollHandoffValueConstants.ReadinessStatus.Ready =>
                    BuildReadyQuery(query, context, readiness)
                        .Where(x => !readiness.ImportedAttendanceDailyIds.Contains(x.Id)),

                AttendancePayrollHandoffValueConstants.ReadinessStatus.AlreadyImported =>
                    query.Where(x => readiness.ImportedAttendanceDailyIds.Contains(x.Id)),

                AttendancePayrollHandoffValueConstants.ReadinessStatus.MissingPayrollProfile =>
                    query.Where(x =>
                        x.WorkforceProfileId.HasValue &&
                        !readiness.EligiblePayrollProfileIds.Contains(x.WorkforceProfileId.Value)),

                AttendancePayrollHandoffValueConstants.ReadinessStatus.MissingPayrollRunEmployee =>
                    query.Where(x =>
                        x.WorkforceProfileId.HasValue &&
                        readiness.EligiblePayrollProfileIds.Contains(x.WorkforceProfileId.Value) &&
                        !readiness.PayrollRunWorkforceProfileIds.Contains(x.WorkforceProfileId.Value)),

                AttendancePayrollHandoffValueConstants.ReadinessStatus.Unprocessed =>
                    query.Where(x => x.ProcessingStatus != AttendanceValueConstants.AttendanceProcessingStatus.Processed),

                AttendancePayrollHandoffValueConstants.ReadinessStatus.PayrollBlocked =>
                    query.Where(x =>
                        x.PayrollInputStatus == AttendanceValueConstants.PayrollInputStatus.Blocked ||
                        readiness.BlockingAttendanceDailyIds.Contains(x.Id)),

                AttendancePayrollHandoffValueConstants.ReadinessStatus.Locked =>
                    query.Where(x => x.IsLocked && !readiness.ImportedAttendanceDailyIds.Contains(x.Id)),

                AttendancePayrollHandoffValueConstants.ReadinessStatus.Excluded =>
                    query.Where(x => !x.IsPayrollEligible || x.PayrollInputStatus == AttendanceValueConstants.PayrollInputStatus.Excluded),

                AttendancePayrollHandoffValueConstants.ReadinessStatus.PeriodMismatch =>
                    query.Where(x => x.PayrollPeriodId.HasValue && x.PayrollPeriodId != context.Period.Id),

                AttendancePayrollHandoffValueConstants.ReadinessStatus.InvalidWorkforce =>
                    query.Where(x => !x.WorkforceProfileId.HasValue),

                _ => query
            };
        }

        private static IOrderedQueryable<HrdAttendanceDaily> ApplySorting(
            IQueryable<HrdAttendanceDaily> query,
            string? sortBy,
            string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "attendanceDate").Trim().ToLowerInvariant() switch
            {
                "workforcedisplayname" => desc
                    ? query.OrderByDescending(x => x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty).ThenByDescending(x => x.AttendanceDate)
                    : query.OrderBy(x => x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty).ThenBy(x => x.AttendanceDate),
                "workforceprofilecode" => desc
                    ? query.OrderByDescending(x => x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : string.Empty).ThenByDescending(x => x.AttendanceDate)
                    : query.OrderBy(x => x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : string.Empty).ThenBy(x => x.AttendanceDate),
                "departmentname" => desc
                    ? query.OrderByDescending(x => x.Department != null ? x.Department.DepartmentName : string.Empty).ThenByDescending(x => x.AttendanceDate)
                    : query.OrderBy(x => x.Department != null ? x.Department.DepartmentName : string.Empty).ThenBy(x => x.AttendanceDate),
                "attendancestatus" => desc
                    ? query.OrderByDescending(x => x.AttendanceStatus).ThenByDescending(x => x.AttendanceDate)
                    : query.OrderBy(x => x.AttendanceStatus).ThenBy(x => x.AttendanceDate),
                "payrollinputstatus" => desc
                    ? query.OrderByDescending(x => x.PayrollInputStatus).ThenByDescending(x => x.AttendanceDate)
                    : query.OrderBy(x => x.PayrollInputStatus).ThenBy(x => x.AttendanceDate),
                "payableworkminutes" => desc
                    ? query.OrderByDescending(x => x.PayableWorkMinutes).ThenByDescending(x => x.AttendanceDate)
                    : query.OrderBy(x => x.PayableWorkMinutes).ThenBy(x => x.AttendanceDate),
                "overtimeminutes" => desc
                    ? query.OrderByDescending(x => x.OvertimeMinutes).ThenByDescending(x => x.AttendanceDate)
                    : query.OrderBy(x => x.OvertimeMinutes).ThenBy(x => x.AttendanceDate),
                _ => desc
                    ? query.OrderByDescending(x => x.AttendanceDate).ThenByDescending(x => x.WorkforceProfileId)
                    : query.OrderBy(x => x.AttendanceDate).ThenBy(x => x.WorkforceProfileId)
            };
        }

        private async Task<SupportData> LoadSupportDataAsync(
            PayrollRunContext context,
            IReadOnlyCollection<HrdAttendanceDaily> dailies,
            CancellationToken cancellationToken,
            bool trackingInputs = false)
        {
            var workforceIds = dailies
                .Where(x => x.WorkforceProfileId.HasValue)
                .Select(x => x.WorkforceProfileId!.Value)
                .Distinct()
                .ToList();
            var dailyIds = dailies.Select(x => x.Id).Distinct().ToList();

            var runEmployees = await _dbContext.Set<TrxPayrollRunEmployee>()
                .Where(x =>
                    x.PayrollRunId == context.Run.Id &&
                    workforceIds.Contains(x.WorkforceProfileId) &&
                    !x.IsDelete)
                .ToListAsync(cancellationToken);

            var payrollProfiles = await _dbContext.Set<WfpPayroll>()
                .AsNoTracking()
                .Where(x => workforceIds.Contains(x.WorkforceProfileId) && !x.IsDelete)
                .ToListAsync(cancellationToken);

            var blockingExceptions = await _dbContext.Set<HrdAttendanceException>()
                .AsNoTracking()
                .Where(x =>
                    dailyIds.Contains(x.AttendanceDailyId) &&
                    !x.IsDelete &&
                    x.IsActive &&
                    x.IsPayrollBlocking &&
                    x.ExceptionStatus != "Closed" &&
                    x.ExceptionStatus != "Corrected" &&
                    x.ExceptionStatus != "Waived")
                .ToListAsync(cancellationToken);

            var runEmployeeIds = runEmployees.Select(x => x.Id).ToList();
            IQueryable<TrxPayrollAttendanceInput> inputQuery = _dbContext.Set<TrxPayrollAttendanceInput>()
                .Where(x => !x.IsDelete && runEmployeeIds.Contains(x.PayrollRunEmployeeId));
            if (!trackingInputs)
            {
                inputQuery = inputQuery.AsNoTracking();
            }

            var inputs = await inputQuery.ToListAsync(cancellationToken);

            return new SupportData
            {
                RunEmployeeByWorkforceProfileId = runEmployees
                    .GroupBy(x => x.WorkforceProfileId)
                    .ToDictionary(x => x.Key, x => x.First()),
                PayrollProfileByWorkforceProfileId = payrollProfiles
                    .GroupBy(x => x.WorkforceProfileId)
                    .ToDictionary(x => x.Key, x => x.OrderByDescending(y => y.EffectiveStartDate).First()),
                BlockingExceptionsByDailyId = blockingExceptions
                    .GroupBy(x => x.AttendanceDailyId)
                    .ToDictionary(x => x.Key, x => x.ToList()),
                InputByAttendanceDailyId = inputs
                    .Where(x => x.AttendanceDailyId.HasValue)
                    .GroupBy(x => x.AttendanceDailyId!.Value)
                    .ToDictionary(x => x.Key, x => x.OrderByDescending(y => y.ImportedAt).First()),
                InputByRunEmployeeAndDate = inputs
                    .GroupBy(x => (x.PayrollRunEmployeeId, x.AttendanceDate))
                    .ToDictionary(x => x.Key, x => x.OrderByDescending(y => y.ImportedAt).First())
            };
        }

        private static AttendancePayrollHandoffPreviewItemResponse MapPreviewItem(
            HrdAttendanceDaily daily,
            PayrollRunContext context,
            SupportData support)
        {
            var evaluation = EvaluateReadiness(daily, context, support);
            return new AttendancePayrollHandoffPreviewItemResponse
            {
                AttendanceDailyId = daily.Id,
                WorkforceProfileId = daily.WorkforceProfileId,
                WorkforceProfileCode = daily.WorkforceProfile?.ProfileCode,
                WorkforceDisplayName = daily.WorkforceProfile?.DisplayName,
                DepartmentId = daily.DepartmentId,
                DepartmentName = daily.Department?.DepartmentName,
                AttendanceDate = daily.AttendanceDate,
                AttendanceStatus = daily.AttendanceStatus,
                ProcessingStatus = daily.ProcessingStatus,
                PayrollInputStatus = daily.PayrollInputStatus,
                IsPayrollEligible = daily.IsPayrollEligible,
                IsCorrected = daily.IsCorrected,
                IsLocked = daily.IsLocked,
                ScheduledWorkMinutes = daily.ScheduledWorkMinutes,
                ActualWorkMinutes = daily.ActualWorkMinutes,
                PayableWorkMinutes = daily.PayableWorkMinutes,
                LateMinutes = daily.LateMinutes,
                EarlyLeaveMinutes = daily.EarlyLeaveMinutes,
                OvertimeMinutes = daily.OvertimeMinutes,
                ExceptionCount = daily.ExceptionCount,
                PayrollBlockingExceptionCount = evaluation.BlockingExceptionCount,
                HasPayrollProfile = evaluation.PayrollProfile != null,
                HasPayrollRunEmployee = evaluation.RunEmployee != null,
                PayrollRunEmployeeId = evaluation.RunEmployee?.Id,
                HasExistingPayrollInput = evaluation.ExistingInput != null,
                PayrollAttendanceInputId = evaluation.ExistingInput?.Id,
                ReadinessStatus = evaluation.Status,
                IsReady = evaluation.IsReady,
                Reasons = evaluation.Reasons
            };
        }

        private static ReadinessEvaluation EvaluateReadiness(
            HrdAttendanceDaily daily,
            PayrollRunContext context,
            SupportData support)
        {
            var result = new ReadinessEvaluation();

            if (!daily.WorkforceProfileId.HasValue)
            {
                result.Status = AttendancePayrollHandoffValueConstants.ReadinessStatus.InvalidWorkforce;
                result.Reasons.Add(Reason(
                    AttendancePayrollHandoffValueConstants.ReasonCode.MissingWorkforceProfile,
                    "Attendance belum terhubung ke workforce profile."));
                return result;
            }

            var workforceProfileId = daily.WorkforceProfileId.Value;
            support.RunEmployeeByWorkforceProfileId.TryGetValue(workforceProfileId, out var runEmployee);
            support.PayrollProfileByWorkforceProfileId.TryGetValue(workforceProfileId, out var payrollProfile);
            support.BlockingExceptionsByDailyId.TryGetValue(daily.Id, out var blockingExceptions);
            support.InputByAttendanceDailyId.TryGetValue(daily.Id, out var inputByDaily);

            TrxPayrollAttendanceInput? existingInput = inputByDaily;
            if (existingInput == null && runEmployee != null)
            {
                support.InputByRunEmployeeAndDate.TryGetValue(
                    (runEmployee.Id, daily.AttendanceDate),
                    out existingInput);
            }

            result.RunEmployee = runEmployee;
            result.PayrollProfile = payrollProfile;
            result.ExistingInput = existingInput;
            result.BlockingExceptionCount = blockingExceptions?.Count ?? 0;

            if (daily.PayrollPeriodId.HasValue && daily.PayrollPeriodId != context.Period.Id)
            {
                result.Status = AttendancePayrollHandoffValueConstants.ReadinessStatus.PeriodMismatch;
                result.Reasons.Add(Reason(
                    AttendancePayrollHandoffValueConstants.ReasonCode.PayrollPeriodMismatch,
                    "Attendance telah terhubung ke payroll period lain."));
                return result;
            }

            if (daily.ProcessingStatus != AttendanceValueConstants.AttendanceProcessingStatus.Processed)
            {
                result.Status = AttendancePayrollHandoffValueConstants.ReadinessStatus.Unprocessed;
                result.Reasons.Add(Reason(
                    AttendancePayrollHandoffValueConstants.ReasonCode.AttendanceNotProcessed,
                    "Attendance harus selesai diproses sebelum dikirim ke payroll."));
                return result;
            }

            if (!daily.IsPayrollEligible || daily.PayrollInputStatus == AttendanceValueConstants.PayrollInputStatus.Excluded)
            {
                result.Status = AttendancePayrollHandoffValueConstants.ReadinessStatus.Excluded;
                result.Reasons.Add(Reason(
                    daily.IsPayrollEligible
                        ? AttendancePayrollHandoffValueConstants.ReasonCode.PayrollInputExcluded
                        : AttendancePayrollHandoffValueConstants.ReasonCode.AttendanceNotPayrollEligible,
                    "Attendance dikecualikan dari payroll."));
                return result;
            }

            if (daily.PayrollInputStatus == AttendanceValueConstants.PayrollInputStatus.Blocked)
            {
                result.Status = AttendancePayrollHandoffValueConstants.ReadinessStatus.PayrollBlocked;
                result.Reasons.Add(Reason(
                    AttendancePayrollHandoffValueConstants.ReasonCode.PayrollInputBlocked,
                    "Attendance berstatus blocked untuk payroll."));
                return result;
            }

            if (daily.PayrollInputStatus != AttendanceValueConstants.PayrollInputStatus.Ready &&
                daily.PayrollInputStatus != AttendanceValueConstants.PayrollInputStatus.Processed)
            {
                result.Status = AttendancePayrollHandoffValueConstants.ReadinessStatus.PayrollBlocked;
                result.Reasons.Add(Reason(
                    AttendancePayrollHandoffValueConstants.ReasonCode.PayrollInputNotReady,
                    $"PayrollInputStatus masih {daily.PayrollInputStatus}."));
                return result;
            }

            if (!IsPayrollProfileEffective(payrollProfile, daily.AttendanceDate))
            {
                result.Status = AttendancePayrollHandoffValueConstants.ReadinessStatus.MissingPayrollProfile;
                result.Reasons.Add(Reason(
                    AttendancePayrollHandoffValueConstants.ReasonCode.MissingPayrollProfile,
                    "Profil payroll aktif dan eligible tidak ditemukan pada tanggal attendance."));
                return result;
            }

            if (runEmployee == null)
            {
                result.Status = AttendancePayrollHandoffValueConstants.ReadinessStatus.MissingPayrollRunEmployee;
                result.Reasons.Add(Reason(
                    AttendancePayrollHandoffValueConstants.ReasonCode.MissingPayrollRunEmployee,
                    "Workforce belum tersedia pada TrxPayrollRunEmployee untuk payroll run ini."));
                return result;
            }

            if (blockingExceptions != null && blockingExceptions.Count > 0)
            {
                result.Status = AttendancePayrollHandoffValueConstants.ReadinessStatus.PayrollBlocked;
                result.Reasons.Add(Reason(
                    AttendancePayrollHandoffValueConstants.ReasonCode.PayrollBlockingException,
                    $"Terdapat {blockingExceptions.Count} attendance exception yang memblokir payroll."));
                return result;
            }

            if (daily.IsLocked && existingInput == null)
            {
                result.Status = AttendancePayrollHandoffValueConstants.ReadinessStatus.Locked;
                result.Reasons.Add(Reason(
                    AttendancePayrollHandoffValueConstants.ReasonCode.AttendanceLockedByOtherRun,
                    "Attendance terkunci tetapi snapshot untuk payroll run ini tidak ditemukan."));
                return result;
            }

            if (existingInput != null &&
                existingInput.AttendanceDailyId.HasValue &&
                existingInput.AttendanceDailyId.Value != daily.Id)
            {
                result.Status = AttendancePayrollHandoffValueConstants.ReadinessStatus.PayrollBlocked;
                result.Reasons.Add(Reason(
                    AttendancePayrollHandoffValueConstants.ReasonCode.ExistingInputKeyConflict,
                    "Kombinasi payroll run employee dan tanggal sudah digunakan oleh attendance lain."));
                return result;
            }

            result.IsReady = true;
            result.Status = existingInput == null
                ? AttendancePayrollHandoffValueConstants.ReadinessStatus.Ready
                : AttendancePayrollHandoffValueConstants.ReadinessStatus.AlreadyImported;
            return result;
        }

        private static bool IsPayrollProfileEffective(WfpPayroll? payrollProfile, DateOnly attendanceDate)
        {
            if (payrollProfile == null || payrollProfile.IsDelete || !payrollProfile.IsActive || !payrollProfile.IsPayrollEligible)
            {
                return false;
            }

            var date = attendanceDate.ToDateTime(TimeOnly.MinValue);
            if (payrollProfile.EffectiveStartDate.HasValue && payrollProfile.EffectiveStartDate.Value.Date > date.Date)
            {
                return false;
            }

            if (payrollProfile.EffectiveEndDate.HasValue && payrollProfile.EffectiveEndDate.Value.Date < date.Date)
            {
                return false;
            }

            return true;
        }

        private static void ApplySnapshot(
            TrxPayrollAttendanceInput input,
            HrdAttendanceDaily daily,
            Guid payrollRunEmployeeId,
            Guid actorUserId,
            DateTime now,
            string? notes)
        {
            input.PayrollRunEmployeeId = payrollRunEmployeeId;
            input.AttendanceDailyId = daily.Id;
            input.AttendanceDate = daily.AttendanceDate;
            input.AttendanceStatusSnapshot = daily.AttendanceStatus;
            input.ScheduledWorkMinutes = daily.ScheduledWorkMinutes;
            input.ActualWorkMinutes = daily.ActualWorkMinutes;
            input.PayableWorkMinutes = daily.PayableWorkMinutes;
            input.LateMinutes = daily.LateMinutes;
            input.EarlyLeaveMinutes = daily.EarlyLeaveMinutes;
            input.OvertimeMinutes = daily.OvertimeMinutes;
            input.PaidLeaveDays = 0m;
            input.UnpaidLeaveDays = 0m;
            input.AbsentDays = daily.IsAbsent || daily.AttendanceStatus == AttendanceValueConstants.AttendanceStatus.Absent ? 1m : 0m;
            input.AttendanceAllowanceAmount = 0m;
            input.AttendanceDeductionAmount = 0m;
            input.IsHoliday = daily.IsHoliday;
            input.IsRestDay = daily.IsRestDay;
            input.IsBusinessTravel = daily.IsBusinessTrip;
            input.IsCorrectionApplied = daily.IsCorrected;
            input.AttendanceSnapshotJson = JsonSerializer.Serialize(new
            {
                attendanceDailyId = daily.Id,
                daily.WorkforceProfileId,
                daily.EmployeeId,
                daily.DoctorId,
                userType = daily.UserType.ToString(),
                daily.HospitalSiteId,
                daily.OrganizationUnitId,
                daily.DepartmentId,
                daily.PositionId,
                daily.WorkLocationId,
                daily.WorkScheduleId,
                daily.WorkScheduleAssignmentId,
                daily.PrimaryShiftAssignmentId,
                daily.ShiftId,
                daily.AttendancePolicyId,
                daily.GracePeriodPolicyId,
                attendanceDate = daily.AttendanceDate.ToString("yyyy-MM-dd"),
                daily.ScheduleSource,
                daily.ScheduledCheckInAt,
                daily.ScheduledCheckOutAt,
                daily.FirstCheckInAt,
                daily.LastCheckOutAt,
                daily.IsOvernightSchedule,
                daily.IsHoliday,
                daily.IsRestDay,
                daily.IsPresent,
                daily.IsAbsent,
                daily.IsLate,
                daily.IsEarlyLeave,
                daily.HasMissingPunch,
                daily.IsBusinessTrip,
                daily.IsRemoteAttendance,
                daily.IsCorrected,
                daily.ScheduledWorkMinutes,
                daily.ActualWorkMinutes,
                daily.BreakMinutes,
                daily.PayableWorkMinutes,
                daily.LateMinutes,
                daily.EarlyLeaveMinutes,
                daily.OvertimeMinutes,
                daily.NightWorkMinutes,
                daily.SourceLogCount,
                daily.ExceptionCount,
                daily.AttendanceStatus,
                daily.ProcessingStatus,
                daily.ProcessingVersion,
                daily.ProcessedAt,
                generatedAt = now
            });
            input.ImportedAt = now;
            input.ImportedByUserId = actorUserId;
            input.Notes = LimitMessage(notes, 1000);
            input.IsActive = true;
            input.IsDelete = false;
            input.DeleteDateTime = null;
            input.UpdateDateTime = now;
            input.UpdateBy = actorUserId;
        }

        private static bool HasSnapshotChanged(
            HrdAttendanceDaily daily,
            TrxPayrollAttendanceInput input)
        {
            return input.AttendanceDailyId != daily.Id ||
                   input.AttendanceDate != daily.AttendanceDate ||
                   input.AttendanceStatusSnapshot != daily.AttendanceStatus ||
                   input.ScheduledWorkMinutes != daily.ScheduledWorkMinutes ||
                   input.ActualWorkMinutes != daily.ActualWorkMinutes ||
                   input.PayableWorkMinutes != daily.PayableWorkMinutes ||
                   input.LateMinutes != daily.LateMinutes ||
                   input.EarlyLeaveMinutes != daily.EarlyLeaveMinutes ||
                   input.OvertimeMinutes != daily.OvertimeMinutes ||
                   input.IsHoliday != daily.IsHoliday ||
                   input.IsRestDay != daily.IsRestDay ||
                   input.IsBusinessTravel != daily.IsBusinessTrip ||
                   input.IsCorrectionApplied != daily.IsCorrected ||
                   (daily.UpdateDateTime.HasValue && input.ImportedAt < daily.UpdateDateTime.Value);
        }

        private static AttendancePayrollHandoffReconciliationItemResponse BuildMissingInputIssue(
            HrdAttendanceDaily daily)
        {
            return new AttendancePayrollHandoffReconciliationItemResponse
            {
                IssueType = AttendancePayrollHandoffValueConstants.ReconciliationIssueType.MissingInput,
                Severity = "High",
                AttendanceDailyId = daily.Id,
                WorkforceProfileId = daily.WorkforceProfileId,
                WorkforceProfileCode = daily.WorkforceProfile?.ProfileCode,
                WorkforceDisplayName = daily.WorkforceProfile?.DisplayName,
                AttendanceDate = daily.AttendanceDate,
                AttendanceStatus = daily.AttendanceStatus,
                IsAttendanceCorrected = daily.IsCorrected,
                AttendanceUpdatedAt = daily.UpdateDateTime,
                Message = "Attendance ditandai sudah diproses payroll tetapi snapshot TrxPayrollAttendanceInput tidak ditemukan.",
                SuggestedAction = "Jalankan endpoint repair atau execute untuk attendance ini."
            };
        }

        private static AttendancePayrollHandoffReconciliationItemResponse BuildChangedInputIssue(
            HrdAttendanceDaily daily,
            TrxPayrollAttendanceInput input)
        {
            return new AttendancePayrollHandoffReconciliationItemResponse
            {
                IssueType = AttendancePayrollHandoffValueConstants.ReconciliationIssueType.ChangedAfterImport,
                Severity = "High",
                AttendanceDailyId = daily.Id,
                PayrollAttendanceInputId = input.Id,
                PayrollRunEmployeeId = input.PayrollRunEmployeeId,
                WorkforceProfileId = daily.WorkforceProfileId,
                WorkforceProfileCode = daily.WorkforceProfile?.ProfileCode,
                WorkforceDisplayName = daily.WorkforceProfile?.DisplayName,
                AttendanceDate = daily.AttendanceDate,
                AttendanceStatus = daily.AttendanceStatus,
                AttendanceStatusSnapshot = input.AttendanceStatusSnapshot,
                IsCorrectionApplied = input.IsCorrectionApplied,
                IsAttendanceCorrected = daily.IsCorrected,
                ImportedAt = input.ImportedAt,
                AttendanceUpdatedAt = daily.UpdateDateTime,
                Message = "Nilai attendance saat ini berbeda dengan snapshot yang sudah dikirim ke payroll.",
                SuggestedAction = "Jalankan endpoint repair sebelum payroll run dihitung atau dikunci."
            };
        }

        private static AttendancePayrollHandoffReconciliationItemResponse BuildOrphanInputIssue(
            TrxPayrollAttendanceInput input,
            IReadOnlyDictionary<Guid, Guid> runEmployeeProfileMap)
        {
            runEmployeeProfileMap.TryGetValue(input.PayrollRunEmployeeId, out var workforceProfileId);
            return new AttendancePayrollHandoffReconciliationItemResponse
            {
                IssueType = AttendancePayrollHandoffValueConstants.ReconciliationIssueType.OrphanInput,
                Severity = "Critical",
                AttendanceDailyId = input.AttendanceDailyId,
                PayrollAttendanceInputId = input.Id,
                PayrollRunEmployeeId = input.PayrollRunEmployeeId,
                WorkforceProfileId = workforceProfileId == Guid.Empty ? null : workforceProfileId,
                AttendanceDate = input.AttendanceDate,
                AttendanceStatusSnapshot = input.AttendanceStatusSnapshot,
                IsCorrectionApplied = input.IsCorrectionApplied,
                ImportedAt = input.ImportedAt,
                Message = "Payroll attendance input tidak memiliki attendance daily aktif yang sesuai pada periode ini.",
                SuggestedAction = "Audit data sumber lalu rollback input yang orphan bila belum dipakai kalkulasi payroll."
            };
        }

        private static AttendancePayrollHandoffReconciliationItemResponse BuildOutsidePeriodIssue(
            HrdAttendanceDaily daily,
            TrxPayrollAttendanceInput input)
        {
            return new AttendancePayrollHandoffReconciliationItemResponse
            {
                IssueType = AttendancePayrollHandoffValueConstants.ReconciliationIssueType.OutsidePeriod,
                Severity = "Critical",
                AttendanceDailyId = daily.Id,
                PayrollAttendanceInputId = input.Id,
                PayrollRunEmployeeId = input.PayrollRunEmployeeId,
                WorkforceProfileId = daily.WorkforceProfileId,
                WorkforceProfileCode = daily.WorkforceProfile?.ProfileCode,
                WorkforceDisplayName = daily.WorkforceProfile?.DisplayName,
                AttendanceDate = daily.AttendanceDate,
                AttendanceStatus = daily.AttendanceStatus,
                AttendanceStatusSnapshot = input.AttendanceStatusSnapshot,
                ImportedAt = input.ImportedAt,
                Message = "Snapshot attendance berada di luar rentang payroll period.",
                SuggestedAction = "Rollback input dan kirim ke payroll run dengan periode yang benar."
            };
        }

        private static List<AttendancePayrollHandoffReconciliationItemResponse> ApplyReconciliationSorting(
            List<AttendancePayrollHandoffReconciliationItemResponse> items,
            string? sortBy,
            string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            IOrderedEnumerable<AttendancePayrollHandoffReconciliationItemResponse> ordered =
                (sortBy ?? "attendanceDate").Trim().ToLowerInvariant() switch
                {
                    "issuetype" => desc
                        ? items.OrderByDescending(x => x.IssueType)
                        : items.OrderBy(x => x.IssueType),
                    "workforcedisplayname" => desc
                        ? items.OrderByDescending(x => x.WorkforceDisplayName)
                        : items.OrderBy(x => x.WorkforceDisplayName),
                    "importedat" => desc
                        ? items.OrderByDescending(x => x.ImportedAt)
                        : items.OrderBy(x => x.ImportedAt),
                    _ => desc
                        ? items.OrderByDescending(x => x.AttendanceDate)
                        : items.OrderBy(x => x.AttendanceDate)
                };
            return ordered.ThenBy(x => x.WorkforceDisplayName).ToList();
        }

        private static List<string> BuildContextBlockingReasons(PayrollRunContext context)
        {
            var reasons = new List<string>();
            if (!context.Run.IsActive)
            {
                reasons.Add("Payroll run tidak aktif.");
            }
            if (context.Run.IsLocked)
            {
                reasons.Add("Payroll run sudah dikunci.");
            }
            if (AttendancePayrollHandoffValueConstants.TerminalPayrollRunStatuses.Contains(
                context.Run.RunStatus,
                StringComparer.OrdinalIgnoreCase))
            {
                reasons.Add($"Payroll run berstatus {context.Run.RunStatus} dan tidak dapat menerima perubahan attendance.");
            }
            if (!context.Period.IsActive)
            {
                reasons.Add("Payroll period tidak aktif.");
            }
            if (context.Period.IsLocked)
            {
                reasons.Add("Payroll period sudah dikunci.");
            }
            if (AttendancePayrollHandoffValueConstants.TerminalPayrollPeriodStatuses.Contains(
                context.Period.PayrollPeriodStatus,
                StringComparer.OrdinalIgnoreCase))
            {
                reasons.Add($"Payroll period berstatus {context.Period.PayrollPeriodStatus} dan tidak dapat menerima perubahan attendance.");
            }
            if (context.EndDate < context.StartDate)
            {
                reasons.Add("Rentang tanggal payroll period tidak valid.");
            }
            return reasons;
        }

        private static AttendancePayrollHandoffStringOptionResponse Option(
            string value,
            string label,
            string? description = null)
        {
            return new AttendancePayrollHandoffStringOptionResponse
            {
                Value = value,
                Label = label,
                Description = description
            };
        }

        private static AttendancePayrollHandoffReadinessReasonResponse Reason(
            string code,
            string message,
            string severity = "Warning")
        {
            return new AttendancePayrollHandoffReadinessReasonResponse
            {
                Code = code,
                Severity = severity,
                Message = message
            };
        }

        private static string? LimitMessage(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            var text = value.Trim();
            return text.Length <= maxLength ? text : text[..maxLength];
        }

        private static bool ContainsIgnoreCase(string? value, string keyword)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   value.Contains(keyword, StringComparison.OrdinalIgnoreCase);
        }

        private sealed class PayrollRunContext
        {
            public TrxPayrollRun Run { get; set; } = null!;
            public MstPayrollPeriod Period { get; set; } = null!;
            public DateOnly StartDate { get; set; }
            public DateOnly EndDate { get; set; }
        }

        private sealed class ContextLoadResult
        {
            public bool Success { get; private set; }
            public int StatusCode { get; private set; }
            public string Message { get; private set; } = string.Empty;
            public PayrollRunContext? Context { get; private set; }

            public static ContextLoadResult Ok(PayrollRunContext context)
            {
                return new ContextLoadResult
                {
                    Success = true,
                    StatusCode = StatusCodes.Status200OK,
                    Context = context
                };
            }

            public static ContextLoadResult Fail(int statusCode, string message)
            {
                return new ContextLoadResult
                {
                    Success = false,
                    StatusCode = statusCode,
                    Message = message
                };
            }
        }

        private sealed class ReadinessQueries
        {
            public IQueryable<Guid> PayrollRunEmployeeIds { get; set; } = null!;
            public IQueryable<Guid> PayrollRunWorkforceProfileIds { get; set; } = null!;
            public IQueryable<Guid> EligiblePayrollProfileIds { get; set; } = null!;
            public IQueryable<Guid> BlockingAttendanceDailyIds { get; set; } = null!;
            public IQueryable<Guid> ImportedAttendanceDailyIds { get; set; } = null!;
        }

        private sealed class SupportData
        {
            public Dictionary<Guid, TrxPayrollRunEmployee> RunEmployeeByWorkforceProfileId { get; set; } = new();
            public Dictionary<Guid, WfpPayroll> PayrollProfileByWorkforceProfileId { get; set; } = new();
            public Dictionary<Guid, List<HrdAttendanceException>> BlockingExceptionsByDailyId { get; set; } = new();
            public Dictionary<Guid, TrxPayrollAttendanceInput> InputByAttendanceDailyId { get; set; } = new();
            public Dictionary<(Guid PayrollRunEmployeeId, DateOnly AttendanceDate), TrxPayrollAttendanceInput> InputByRunEmployeeAndDate { get; set; } = new();
        }

        private sealed class ReadinessEvaluation
        {
            public bool IsReady { get; set; }
            public string Status { get; set; } = AttendancePayrollHandoffValueConstants.ReadinessStatus.Ready;
            public int BlockingExceptionCount { get; set; }
            public TrxPayrollRunEmployee? RunEmployee { get; set; }
            public WfpPayroll? PayrollProfile { get; set; }
            public TrxPayrollAttendanceInput? ExistingInput { get; set; }
            public List<AttendancePayrollHandoffReadinessReasonResponse> Reasons { get; set; } = new();
        }
    }
}
