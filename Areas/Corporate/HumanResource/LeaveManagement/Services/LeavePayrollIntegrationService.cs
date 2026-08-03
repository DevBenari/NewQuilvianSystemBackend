using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Repositories;
using System.Data;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    public class LeavePayrollIntegrationService
    {
        private const decimal Tolerance = 0.0001m;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly AttendancePayrollHandoffService _attendancePayrollHandoffService;
        private readonly LeavePayrollIntegrationOptions _options;

        public LeavePayrollIntegrationService(
            ApplicationDbContext dbContext,
            AttendancePayrollHandoffService attendancePayrollHandoffService,
            IOptions<LeavePayrollIntegrationOptions> options)
        {
            _dbContext = dbContext;
            _attendancePayrollHandoffService = attendancePayrollHandoffService;
            _options = options.Value;
        }

        public LeavePayrollIntegrationMetadataResponse GetMetadata()
        {
            return new LeavePayrollIntegrationMetadataResponse
            {
                MaximumItemPerExecution = Math.Max(1, _options.MaximumItemPerExecution),
                DefaultFilter = new LeavePayrollIntegrationDefaultFilterResponse(),
                ReadinessStatusOptions = new List<LeavePayrollStringOptionResponse>
                {
                    Option(LeavePayrollIntegrationValueConstants.ReadinessStatus.Ready, "Siap"),
                    Option(LeavePayrollIntegrationValueConstants.ReadinessStatus.AlreadySynchronized, "Sudah sinkron"),
                    Option(LeavePayrollIntegrationValueConstants.ReadinessStatus.MissingPayrollRunEmployee, "Payroll employee belum tersedia"),
                    Option(LeavePayrollIntegrationValueConstants.ReadinessStatus.MissingPayrollAttendanceInput, "Payroll attendance input belum tersedia"),
                    Option(LeavePayrollIntegrationValueConstants.ReadinessStatus.PayrollEmployeeFrozen, "Snapshot payroll employee sudah dibekukan"),
                    Option(LeavePayrollIntegrationValueConstants.ReadinessStatus.Blocked, "Diblokir")
                },
                IssueTypeOptions = new List<LeavePayrollStringOptionResponse>
                {
                    Option(LeavePayrollIntegrationValueConstants.IssueType.MissingPayrollRunEmployee, "Payroll employee belum tersedia"),
                    Option(LeavePayrollIntegrationValueConstants.IssueType.MissingPayrollAttendanceInput, "Payroll attendance input belum tersedia"),
                    Option(LeavePayrollIntegrationValueConstants.IssueType.LeaveDayMismatch, "Jumlah hari cuti tidak sesuai"),
                    Option(LeavePayrollIntegrationValueConstants.IssueType.EmployeeAggregateMismatch, "Agregat payroll employee tidak sesuai"),
                    Option(LeavePayrollIntegrationValueConstants.IssueType.MissingLeaveAllowanceInput, "Input tunjangan cuti belum tersedia"),
                    Option(LeavePayrollIntegrationValueConstants.IssueType.MissingEncashmentInput, "Input encashment belum tersedia"),
                    Option(LeavePayrollIntegrationValueConstants.IssueType.VariableInputMismatch, "Quantity variable input tidak sesuai"),
                    Option(LeavePayrollIntegrationValueConstants.IssueType.TerminalVariableInput, "Variable input sudah terminal")
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                AutoCreateAttendanceInputs = _options.AutoCreateAttendanceInputs,
                AutoCreateLeaveAllowanceVariableInputs = _options.AutoCreateLeaveAllowanceVariableInputs,
                AutoCreateEncashmentVariableInputs = _options.AutoCreateEncashmentVariableInputs,
                SubmitVariableInputs = _options.SubmitVariableInputs,
                LeaveAllowancePayrollComponentCode = _options.LeaveAllowancePayrollComponentCode,
                LeaveEncashmentPayrollComponentCode = _options.LeaveEncashmentPayrollComponentCode,
                BoundaryInfo = "Leave Management mengirim hari/menit/quantity. Payroll Management menghitung nominal. Finance/Accounting menerima hasil payroll yang sudah final, bukan data cuti mentah."
            };
        }

        public async Task<List<LeavePayrollRunOptionResponse>> GetPayrollRunOptionsAsync(
            string? search,
            int take,
            CancellationToken cancellationToken = default)
        {
            take = Math.Clamp(take, 1, 200);
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

            var rows = await query
                .OrderByDescending(x => x.period.StartDate)
                .ThenByDescending(x => x.run.CreateDateTime)
                .Take(take)
                .ToListAsync(cancellationToken);

            return rows.Select(x => new LeavePayrollRunOptionResponse
            {
                Id = x.run.Id,
                PayrollPeriodId = x.period.Id,
                RunNumber = x.run.RunNumber,
                RunStatus = x.run.RunStatus,
                IsLocked = x.run.IsLocked,
                PayrollPeriodCode = x.period.PayrollPeriodCode,
                PayrollPeriodName = x.period.PayrollPeriodName,
                PeriodStartDate = x.period.StartDate,
                PeriodEndDate = x.period.EndDate,
                PayrollPeriodStatus = x.period.PayrollPeriodStatus
            }).ToList();
        }

        public async Task<List<LeavePayrollComponentOptionResponse>> GetPayrollComponentOptionsAsync(
            string? search,
            int take,
            CancellationToken cancellationToken = default)
        {
            take = Math.Clamp(take, 1, 200);
            var today = DateTime.UtcNow.Date;
            var query = _dbContext.Set<MstPayrollComponent>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= today) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= today));

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.PayrollComponentCode.ToLower().Contains(keyword) ||
                    x.PayrollComponentName.ToLower().Contains(keyword) ||
                    x.ComponentType.ToLower().Contains(keyword));
            }

            return await query
                .OrderBy(x => x.PayrollComponentCode)
                .Take(take)
                .Select(x => new LeavePayrollComponentOptionResponse
                {
                    Id = x.Id,
                    Code = x.PayrollComponentCode,
                    Name = x.PayrollComponentName,
                    ComponentType = x.ComponentType,
                    CalculationMethod = x.CalculationMethod,
                    IsTaxable = x.IsTaxable
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<LeavePayrollIntegrationServiceResult<LeavePayrollIntegrationSummaryResponse>> GetSummaryAsync(
            Guid payrollRunId,
            CancellationToken cancellationToken = default)
        {
            var contextResult = await LoadContextAsync(payrollRunId, false, cancellationToken);
            if (!contextResult.Success || contextResult.Context == null)
            {
                return LeavePayrollIntegrationServiceResult<LeavePayrollIntegrationSummaryResponse>.Fail(
                    contextResult.StatusCode,
                    contextResult.Message);
            }

            var context = contextResult.Context;
            var integrations = await BuildIntegrationQuery(context)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
            var groups = BuildAttendanceGroups(integrations);
            var support = await LoadSupportAsync(context, groups, false, cancellationToken);

            var ready = 0;
            var synchronized = 0;
            var missingRunEmployee = 0;
            var missingInput = 0;

            foreach (var group in groups)
            {
                var evaluation = Evaluate(group, support);
                switch (evaluation.Status)
                {
                    case LeavePayrollIntegrationValueConstants.ReadinessStatus.Ready:
                        ready++;
                        break;
                    case LeavePayrollIntegrationValueConstants.ReadinessStatus.AlreadySynchronized:
                        synchronized++;
                        break;
                    case LeavePayrollIntegrationValueConstants.ReadinessStatus.MissingPayrollRunEmployee:
                        missingRunEmployee++;
                        break;
                    case LeavePayrollIntegrationValueConstants.ReadinessStatus.MissingPayrollAttendanceInput:
                        missingInput++;
                        break;
                }
            }

            var encashments = await BuildEncashmentQuery(context)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
            var variableInputCount = await _dbContext.Set<TrxPayrollVariableInput>()
                .AsNoTracking()
                .CountAsync(x =>
                    !x.IsDelete &&
                    support.RunEmployeeIds.Contains(x.PayrollRunEmployeeId) &&
                    (x.SourceType == LeavePayrollIntegrationValueConstants.SourceType.LeaveAllowance ||
                     x.SourceType == LeavePayrollIntegrationValueConstants.SourceType.LeaveEncashment),
                    cancellationToken);

            var blockingReasons = BuildBlockingReasons(context);
            if (!_options.Enabled)
            {
                blockingReasons.Add("Leave payroll integration dinonaktifkan pada appsettings.");
            }
            if (groups.Count == 0 && encashments.Count == 0)
            {
                blockingReasons.Add("Tidak ada data leave atau encashment pada payroll period ini.");
            }

            var result = new LeavePayrollIntegrationSummaryResponse
            {
                PayrollRunId = context.Run.Id,
                RunNumber = context.Run.RunNumber,
                RunStatus = context.Run.RunStatus,
                IsPayrollRunLocked = context.Run.IsLocked,
                PayrollPeriodId = context.Period.Id,
                PayrollPeriodCode = context.Period.PayrollPeriodCode,
                PeriodStartDate = context.StartDate,
                PeriodEndDate = context.EndDate,
                PayrollPeriodStatus = context.Period.PayrollPeriodStatus,
                IsPayrollPeriodLocked = context.Period.IsLocked,
                LeaveIntegrationCount = integrations.Count,
                DistinctEmployeeCount = integrations.Select(x => x.WorkforceProfileId).Distinct().Count(),
                PaidLeaveDays = integrations.Where(x => x.IsPaidLeave).Sum(x => x.RequestedLeaveDays),
                UnpaidLeaveDays = integrations.Where(x => !x.IsPaidLeave).Sum(x => x.RequestedLeaveDays),
                ReadyCount = ready,
                SynchronizedCount = synchronized,
                MissingPayrollRunEmployeeCount = missingRunEmployee,
                MissingPayrollAttendanceInputCount = missingInput,
                EncashmentPayoutDays = encashments.Sum(x => x.PayoutDays),
                EncashmentCandidateCount = encashments.Count,
                LeaveAllowanceCandidateCount = integrations
                    .Where(x => x.IsPaidLeave)
                    .Select(x => x.LeaveExecutionId)
                    .Distinct()
                    .Count(),
                LeaveVariableInputCount = variableInputCount,
                CanExecute = blockingReasons.Count == 0,
                BlockingReasons = blockingReasons
            };

            return LeavePayrollIntegrationServiceResult<LeavePayrollIntegrationSummaryResponse>.Ok(
                result,
                "Ringkasan leave payroll integration berhasil diambil.");
        }

        public async Task<LeavePayrollIntegrationServiceResult<LeavePayrollIntegrationPreviewPagedResponse>> GetPreviewAsync(
            Guid payrollRunId,
            LeavePayrollIntegrationQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var contextResult = await LoadContextAsync(payrollRunId, false, cancellationToken);
            if (!contextResult.Success || contextResult.Context == null)
            {
                return LeavePayrollIntegrationServiceResult<LeavePayrollIntegrationPreviewPagedResponse>.Fail(
                    contextResult.StatusCode,
                    contextResult.Message);
            }

            var context = contextResult.Context;
            var query = BuildIntegrationQuery(context).AsNoTracking();
            if (request.WorkforceProfileId.HasValue && request.WorkforceProfileId.Value != Guid.Empty)
            {
                query = query.Where(x => x.WorkforceProfileId == request.WorkforceProfileId.Value);
            }
            if (request.LeaveTypeId.HasValue && request.LeaveTypeId.Value != Guid.Empty)
            {
                query = query.Where(x => x.LeaveTypeId == request.LeaveTypeId.Value);
            }

            var integrations = await query.ToListAsync(cancellationToken);
            var groups = BuildAttendanceGroups(integrations);
            var support = await LoadSupportAsync(context, groups, false, cancellationToken);
            var profiles = await LoadProfilesAsync(groups.Select(x => x.WorkforceProfileId), cancellationToken);

            var items = groups.Select(group =>
            {
                var evaluation = Evaluate(group, support);
                profiles.TryGetValue(group.WorkforceProfileId, out var profile);
                return MapPreview(group, evaluation, profile);
            }).ToList();

            if (!string.IsNullOrWhiteSpace(request.ReadinessStatus))
            {
                items = items.Where(x => string.Equals(
                    x.ReadinessStatus,
                    request.ReadinessStatus.Trim(),
                    StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim();
                items = items.Where(x =>
                    ContainsIgnoreCase(x.WorkforceProfileCode, keyword) ||
                    ContainsIgnoreCase(x.WorkforceDisplayName, keyword) ||
                    ContainsIgnoreCase(x.LeaveTypeCode, keyword) ||
                    ContainsIgnoreCase(x.LeaveTypeName, keyword) ||
                    ContainsIgnoreCase(x.ReadinessStatus, keyword)).ToList();
            }

            items = ApplyPreviewSort(items, request.SortBy, request.SortDirection).ToList();
            var pageNumber = Math.Max(1, request.PageNumber);
            var pageSize = Math.Clamp(request.PageSize, 1, 100);
            var totalData = items.Count;
            var pageItems = items.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return LeavePayrollIntegrationServiceResult<LeavePayrollIntegrationPreviewPagedResponse>.Ok(
                new LeavePayrollIntegrationPreviewPagedResponse
                {
                    PayrollRunId = context.Run.Id,
                    RunNumber = context.Run.RunNumber,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = pageItems
                },
                "Preview leave payroll integration berhasil diambil.");
        }

        public async Task<LeavePayrollIntegrationServiceResult<LeavePayrollIntegrationExecutionResponse>> ExecuteAsync(
            Guid payrollRunId,
            ExecuteLeavePayrollIntegrationRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty)
            {
                return LeavePayrollIntegrationServiceResult<LeavePayrollIntegrationExecutionResponse>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Identitas user login tidak valid.");
            }
            if (!_options.Enabled)
            {
                return LeavePayrollIntegrationServiceResult<LeavePayrollIntegrationExecutionResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Leave payroll integration dinonaktifkan pada appsettings.");
            }

            var contextResult = await LoadContextAsync(payrollRunId, false, cancellationToken);
            if (!contextResult.Success || contextResult.Context == null)
            {
                return LeavePayrollIntegrationServiceResult<LeavePayrollIntegrationExecutionResponse>.Fail(
                    contextResult.StatusCode,
                    contextResult.Message);
            }

            var context = contextResult.Context;
            var blocking = BuildBlockingReasons(context);
            if (blocking.Count > 0)
            {
                return LeavePayrollIntegrationServiceResult<LeavePayrollIntegrationExecutionResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    string.Join(" ", blocking));
            }

            var targetQuery = BuildIntegrationQuery(context).AsNoTracking();
            if (request.LeaveAttendanceIntegrationIds != null && request.LeaveAttendanceIntegrationIds.Count > 0)
            {
                var ids = request.LeaveAttendanceIntegrationIds.Where(x => x != Guid.Empty).Distinct().ToList();
                if (ids.Count > _options.MaximumItemPerExecution)
                {
                    return LeavePayrollIntegrationServiceResult<LeavePayrollIntegrationExecutionResponse>.Fail(
                        StatusCodes.Status400BadRequest,
                        $"Maksimal {_options.MaximumItemPerExecution} leave attendance integration per proses.");
                }
                targetQuery = targetQuery.Where(x => ids.Contains(x.Id));
            }
            if (request.WorkforceProfileIds != null && request.WorkforceProfileIds.Count > 0)
            {
                var workforceIds = request.WorkforceProfileIds.Where(x => x != Guid.Empty).Distinct().ToList();
                targetQuery = targetQuery.Where(x => workforceIds.Contains(x.WorkforceProfileId));
            }

            var targets = await targetQuery.ToListAsync(cancellationToken);
            var groups = BuildAttendanceGroups(targets);
            var startedAt = DateTime.UtcNow;
            var ensureAttendanceInputs = request.EnsureAttendanceInputs ?? _options.AutoCreateAttendanceInputs;
            var warnings = new List<string>();

            if (ensureAttendanceInputs)
            {
                var attendanceDailyIds = targets
                    .Where(x => x.AttendanceDailyId.HasValue)
                    .Select(x => x.AttendanceDailyId!.Value)
                    .Distinct()
                    .ToList();
                if (attendanceDailyIds.Count > 0)
                {
                    var attendanceResult = await _attendancePayrollHandoffService.ExecuteAsync(
                        payrollRunId,
                        new ExecuteAttendancePayrollHandoffRequest
                        {
                            AttendanceDailyIds = attendanceDailyIds,
                            ForceRefreshExistingInput = false,
                            ContinueOnValidationError = true,
                            Notes = "Dibuat otomatis dari Leave Payroll Integration."
                        },
                        actorUserId,
                        cancellationToken);

                    if (!attendanceResult.Success)
                    {
                        warnings.Add($"Attendance payroll handoff tidak seluruhnya berhasil: {attendanceResult.Message}");
                    }
                }
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                var trackedContextResult = await LoadContextAsync(payrollRunId, true, cancellationToken);
                if (!trackedContextResult.Success || trackedContextResult.Context == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return LeavePayrollIntegrationServiceResult<LeavePayrollIntegrationExecutionResponse>.Fail(
                        trackedContextResult.StatusCode,
                        trackedContextResult.Message);
                }

                var trackedContext = trackedContextResult.Context;
                var trackedBlocking = BuildBlockingReasons(trackedContext);
                if (trackedBlocking.Count > 0)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return LeavePayrollIntegrationServiceResult<LeavePayrollIntegrationExecutionResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        string.Join(" ", trackedBlocking));
                }

                var support = await LoadSupportAsync(trackedContext, groups, true, cancellationToken);
                var items = new List<LeavePayrollIntegrationExecutionItemResponse>();
                var affectedRunEmployeeIds = new HashSet<Guid>();

                foreach (var group in groups)
                {
                    var evaluation = Evaluate(group, support);
                    var item = new LeavePayrollIntegrationExecutionItemResponse
                    {
                        LeaveAttendanceIntegrationId = group.RepresentativeId,
                        SourceId = group.LeaveExecutionId,
                        SourceType = "LeaveAttendance",
                        WorkforceProfileId = group.WorkforceProfileId,
                        EffectiveDate = group.LeaveDate,
                        PayrollRunEmployeeId = evaluation.RunEmployee?.Id,
                        PayrollAttendanceInputId = evaluation.Input?.Id,
                        Quantity = group.PaidLeaveDays + group.UnpaidLeaveDays
                    };

                    if (!evaluation.CanWrite || evaluation.RunEmployee == null || evaluation.Input == null)
                    {
                        item.Success = false;
                        item.ResultStatus = evaluation.Status;
                        item.Message = evaluation.Message;
                        items.Add(item);
                        if (!request.ContinueOnValidationError)
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            return LeavePayrollIntegrationServiceResult<LeavePayrollIntegrationExecutionResponse>.Fail(
                                StatusCodes.Status409Conflict,
                                evaluation.Message);
                        }
                        continue;
                    }

                    var alreadySynchronized = ApproximatelyEqual(evaluation.Input.PaidLeaveDays, group.PaidLeaveDays) &&
                                              ApproximatelyEqual(evaluation.Input.UnpaidLeaveDays, group.UnpaidLeaveDays);

                    evaluation.Input.PaidLeaveDays = group.PaidLeaveDays;
                    evaluation.Input.UnpaidLeaveDays = group.UnpaidLeaveDays;
                    evaluation.Input.AttendanceSnapshotJson = SetLeavePayrollSnapshot(
                        evaluation.Input.AttendanceSnapshotJson,
                        trackedContext,
                        group,
                        actorUserId,
                        request.Notes);
                    evaluation.Input.UpdateDateTime = DateTime.UtcNow;
                    evaluation.Input.UpdateBy = actorUserId;
                    affectedRunEmployeeIds.Add(evaluation.RunEmployee.Id);

                    item.Success = true;
                    item.IsUpdated = !alreadySynchronized;
                    item.IsIdempotent = alreadySynchronized;
                    item.ResultStatus = alreadySynchronized ? "AlreadySynchronized" : "Updated";
                    item.Message = alreadySynchronized
                        ? "Jumlah hari cuti pada payroll attendance input sudah sesuai."
                        : "Jumlah paid dan unpaid leave berhasil disinkronkan.";
                    items.Add(item);
                }

                var submitVariableInputs = request.SubmitVariableInputs ?? _options.SubmitVariableInputs;
                var createAllowance = request.CreateLeaveAllowanceInputs ?? _options.AutoCreateLeaveAllowanceVariableInputs;
                var createEncashment = request.CreateEncashmentInputs ?? _options.AutoCreateEncashmentVariableInputs;

                if (createAllowance)
                {
                    await CreateLeaveAllowanceInputsAsync(
                        trackedContext,
                        targets,
                        support,
                        actorUserId,
                        submitVariableInputs,
                        request.Notes,
                        items,
                        affectedRunEmployeeIds,
                        warnings,
                        cancellationToken);
                }

                if (createEncashment)
                {
                    await CreateEncashmentInputsAsync(
                        trackedContext,
                        support,
                        actorUserId,
                        submitVariableInputs,
                        request.Notes,
                        items,
                        affectedRunEmployeeIds,
                        warnings,
                        cancellationToken);
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                var recalculated = await RecalculateRunEmployeeAggregatesAsync(
                    affectedRunEmployeeIds,
                    actorUserId,
                    cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                var completedAt = DateTime.UtcNow;
                var response = new LeavePayrollIntegrationExecutionResponse
                {
                    PayrollRunId = trackedContext.Run.Id,
                    RunNumber = trackedContext.Run.RunNumber,
                    PayrollPeriodId = trackedContext.Period.Id,
                    PayrollPeriodCode = trackedContext.Period.PayrollPeriodCode,
                    StartedAt = startedAt,
                    CompletedAt = completedAt,
                    TotalTarget = items.Count,
                    AttendanceInputUpdatedCount = items.Count(x => x.SourceType == "LeaveAttendance" && x.IsUpdated),
                    VariableInputCreatedCount = items.Count(x => x.PayrollVariableInputId.HasValue && x.IsCreated),
                    VariableInputUpdatedCount = items.Count(x => x.PayrollVariableInputId.HasValue && x.IsUpdated),
                    IdempotentCount = items.Count(x => x.IsIdempotent),
                    FailedCount = items.Count(x => !x.Success),
                    PaidLeaveDays = targets.Where(x => x.IsPaidLeave).Sum(x => x.RequestedLeaveDays),
                    UnpaidLeaveDays = targets.Where(x => !x.IsPaidLeave).Sum(x => x.RequestedLeaveDays),
                    EncashmentPayoutDays = items
                        .Where(x => x.SourceType == LeavePayrollIntegrationValueConstants.SourceType.LeaveEncashment && x.Success)
                        .Sum(x => x.Quantity),
                    Warnings = warnings,
                    Items = items
                };

                if (recalculated == 0 && affectedRunEmployeeIds.Count > 0)
                {
                    response.Warnings.Add("Tidak ada agregat payroll employee yang diperbarui.");
                }

                return LeavePayrollIntegrationServiceResult<LeavePayrollIntegrationExecutionResponse>.Ok(
                    response,
                    response.FailedCount == 0
                        ? "Leave payroll integration berhasil diselesaikan."
                        : "Leave payroll integration selesai dengan sebagian item gagal validasi.");
            }
            catch (OperationCanceledException)
            {
                await transaction.RollbackAsync(CancellationToken.None);
                throw;
            }
            catch (DbUpdateException exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                return LeavePayrollIntegrationServiceResult<LeavePayrollIntegrationExecutionResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Gagal menyimpan leave payroll integration. Periksa proses paralel atau data duplikat. {Limit(exception.GetBaseException().Message, 500)}");
            }
            catch (Exception exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                return LeavePayrollIntegrationServiceResult<LeavePayrollIntegrationExecutionResponse>.Fail(
                    StatusCodes.Status500InternalServerError,
                    $"Leave payroll integration gagal. {Limit(exception.GetBaseException().Message, 500)}");
            }
        }

        public async Task<LeavePayrollIntegrationServiceResult<LeavePayrollReconciliationResponse>> GetReconciliationAsync(
            Guid payrollRunId,
            LeavePayrollReconciliationQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var contextResult = await LoadContextAsync(payrollRunId, false, cancellationToken);
            if (!contextResult.Success || contextResult.Context == null)
            {
                return LeavePayrollIntegrationServiceResult<LeavePayrollReconciliationResponse>.Fail(
                    contextResult.StatusCode,
                    contextResult.Message);
            }

            var context = contextResult.Context;
            var integrations = await BuildIntegrationQuery(context).AsNoTracking().ToListAsync(cancellationToken);
            var groups = BuildAttendanceGroups(integrations);
            var support = await LoadSupportAsync(context, groups, false, cancellationToken);
            var profiles = await LoadProfilesAsync(groups.Select(x => x.WorkforceProfileId), cancellationToken);
            var issues = new List<LeavePayrollReconciliationItemResponse>();
            var matchedAttendance = 0;

            foreach (var group in groups)
            {
                profiles.TryGetValue(group.WorkforceProfileId, out var profile);
                var evaluation = Evaluate(group, support);
                if (evaluation.RunEmployee == null)
                {
                    issues.Add(Issue(
                        LeavePayrollIntegrationValueConstants.IssueType.MissingPayrollRunEmployee,
                        group,
                        profile,
                        group.PaidLeaveDays + group.UnpaidLeaveDays,
                        0,
                        "Workforce belum tersedia pada payroll run employee."));
                    continue;
                }
                if (evaluation.Input == null)
                {
                    issues.Add(Issue(
                        LeavePayrollIntegrationValueConstants.IssueType.MissingPayrollAttendanceInput,
                        group,
                        profile,
                        group.PaidLeaveDays + group.UnpaidLeaveDays,
                        0,
                        "Payroll attendance input belum tersedia."));
                    continue;
                }

                var expected = group.PaidLeaveDays + group.UnpaidLeaveDays;
                var actual = evaluation.Input.PaidLeaveDays + evaluation.Input.UnpaidLeaveDays;
                if (!ApproximatelyEqual(group.PaidLeaveDays, evaluation.Input.PaidLeaveDays) ||
                    !ApproximatelyEqual(group.UnpaidLeaveDays, evaluation.Input.UnpaidLeaveDays))
                {
                    issues.Add(Issue(
                        LeavePayrollIntegrationValueConstants.IssueType.LeaveDayMismatch,
                        group,
                        profile,
                        expected,
                        actual,
                        $"Expected paid/unpaid {group.PaidLeaveDays}/{group.UnpaidLeaveDays}, actual {evaluation.Input.PaidLeaveDays}/{evaluation.Input.UnpaidLeaveDays}.",
                        evaluation.Input.Id));
                }
                else
                {
                    matchedAttendance++;
                }
            }

            foreach (var runEmployee in support.RunEmployees)
            {
                var inputs = support.Inputs.Where(x => x.PayrollRunEmployeeId == runEmployee.Id).ToList();
                var expectedPaid = inputs.Sum(x => x.PaidLeaveDays);
                var expectedUnpaid = inputs.Sum(x => x.UnpaidLeaveDays);
                if (!ApproximatelyEqual(runEmployee.PaidLeaveDays, expectedPaid) ||
                    !ApproximatelyEqual(runEmployee.UnpaidLeaveDays, expectedUnpaid))
                {
                    profiles.TryGetValue(runEmployee.WorkforceProfileId, out var profile);
                    issues.Add(new LeavePayrollReconciliationItemResponse
                    {
                        IssueType = LeavePayrollIntegrationValueConstants.IssueType.EmployeeAggregateMismatch,
                        Severity = "Warning",
                        WorkforceProfileId = runEmployee.WorkforceProfileId,
                        WorkforceProfileCode = profile?.ProfileCode,
                        WorkforceDisplayName = profile?.DisplayName,
                        ExpectedQuantity = expectedPaid + expectedUnpaid,
                        ActualQuantity = runEmployee.PaidLeaveDays + runEmployee.UnpaidLeaveDays,
                        Message = $"Agregat expected paid/unpaid {expectedPaid}/{expectedUnpaid}, actual {runEmployee.PaidLeaveDays}/{runEmployee.UnpaidLeaveDays}."
                    });
                }
            }

            var expectedVariableInputs = 0;
            var matchedVariableInputs = 0;
            await ReconcileAllowanceInputsAsync(context, integrations, support, profiles, issues, value => expectedVariableInputs += value, value => matchedVariableInputs += value, cancellationToken);
            await ReconcileEncashmentInputsAsync(context, support, profiles, issues, value => expectedVariableInputs += value, value => matchedVariableInputs += value, cancellationToken);

            if (request.WorkforceProfileId.HasValue && request.WorkforceProfileId.Value != Guid.Empty)
            {
                issues = issues.Where(x => x.WorkforceProfileId == request.WorkforceProfileId.Value).ToList();
            }
            if (!string.IsNullOrWhiteSpace(request.IssueType))
            {
                issues = issues.Where(x => string.Equals(x.IssueType, request.IssueType.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim();
                issues = issues.Where(x =>
                    ContainsIgnoreCase(x.WorkforceProfileCode, keyword) ||
                    ContainsIgnoreCase(x.WorkforceDisplayName, keyword) ||
                    ContainsIgnoreCase(x.IssueType, keyword) ||
                    ContainsIgnoreCase(x.Message, keyword)).ToList();
            }

            var totalIssue = issues.Count;
            var pageNumber = Math.Max(1, request.PageNumber);
            var pageSize = Math.Clamp(request.PageSize, 1, 100);
            var paged = issues
                .OrderBy(x => x.EffectiveDate)
                .ThenBy(x => x.WorkforceDisplayName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return LeavePayrollIntegrationServiceResult<LeavePayrollReconciliationResponse>.Ok(
                new LeavePayrollReconciliationResponse
                {
                    PayrollRunId = context.Run.Id,
                    RunNumber = context.Run.RunNumber,
                    ExpectedAttendanceGroupCount = groups.Count,
                    MatchedAttendanceGroupCount = matchedAttendance,
                    ExpectedVariableInputCount = expectedVariableInputs,
                    MatchedVariableInputCount = matchedVariableInputs,
                    IssueCount = totalIssue,
                    IsBalanced = totalIssue == 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPage = (int)Math.Ceiling(totalIssue / (double)pageSize),
                    Issues = paged
                },
                "Reconciliation leave payroll integration berhasil diambil.");
        }

        public async Task<LeavePayrollIntegrationServiceResult<LeavePayrollIntegrationRollbackResponse>> RollbackAsync(
            Guid payrollRunId,
            RollbackLeavePayrollIntegrationRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty)
            {
                return LeavePayrollIntegrationServiceResult<LeavePayrollIntegrationRollbackResponse>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Identitas user login tidak valid.");
            }
            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return LeavePayrollIntegrationServiceResult<LeavePayrollIntegrationRollbackResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Reason wajib diisi.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);
            try
            {
                var contextResult = await LoadContextAsync(payrollRunId, true, cancellationToken);
                if (!contextResult.Success || contextResult.Context == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return LeavePayrollIntegrationServiceResult<LeavePayrollIntegrationRollbackResponse>.Fail(
                        contextResult.StatusCode,
                        contextResult.Message);
                }

                var context = contextResult.Context;
                var blocking = BuildBlockingReasons(context);
                if (blocking.Count > 0)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return LeavePayrollIntegrationServiceResult<LeavePayrollIntegrationRollbackResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        string.Join(" ", blocking));
                }

                var runEmployees = await _dbContext.Set<TrxPayrollRunEmployee>()
                    .Where(x => x.PayrollRunId == payrollRunId && !x.IsDelete)
                    .ToListAsync(cancellationToken);
                var runEmployeeIds = runEmployees.Select(x => x.Id).ToList();
                var affected = new HashSet<Guid>();
                var attendanceReset = 0;
                var variableDeleted = 0;
                var variableBlocked = 0;

                if (request.IncludeAttendanceLeaveDays)
                {
                    var inputs = await _dbContext.Set<TrxPayrollAttendanceInput>()
                        .Where(x =>
                            !x.IsDelete &&
                            runEmployeeIds.Contains(x.PayrollRunEmployeeId) &&
                            x.AttendanceDate >= context.StartDate &&
                            x.AttendanceDate <= context.EndDate &&
                            (x.PaidLeaveDays != 0 || x.UnpaidLeaveDays != 0))
                        .ToListAsync(cancellationToken);

                    foreach (var input in inputs)
                    {
                        input.PaidLeaveDays = 0;
                        input.UnpaidLeaveDays = 0;
                        input.AttendanceSnapshotJson = RemoveLeavePayrollSnapshot(input.AttendanceSnapshotJson);
                        input.Notes = AppendNote(input.Notes, $"Rollback leave payroll integration: {request.Reason}");
                        input.UpdateDateTime = DateTime.UtcNow;
                        input.UpdateBy = actorUserId;
                        affected.Add(input.PayrollRunEmployeeId);
                        attendanceReset++;
                    }
                }

                if (request.IncludeVariableInputs)
                {
                    var variableInputs = await _dbContext.Set<TrxPayrollVariableInput>()
                        .Where(x =>
                            !x.IsDelete &&
                            runEmployeeIds.Contains(x.PayrollRunEmployeeId) &&
                            (x.SourceType == LeavePayrollIntegrationValueConstants.SourceType.LeaveAllowance ||
                             x.SourceType == LeavePayrollIntegrationValueConstants.SourceType.LeaveEncashment))
                        .ToListAsync(cancellationToken);

                    foreach (var input in variableInputs)
                    {
                        if (LeavePayrollIntegrationValueConstants.TerminalVariableInputStatuses.Contains(input.InputStatus) ||
                            (input.InputStatus == LeavePayrollIntegrationValueConstants.InputStatus.Submitted && !request.AllowSubmittedVariableInputRollback))
                        {
                            variableBlocked++;
                            continue;
                        }

                        input.IsDelete = true;
                        input.IsActive = false;
                        input.DeleteDateTime = DateTime.UtcNow;
                        input.DeleteBy = actorUserId;
                        input.UpdateDateTime = DateTime.UtcNow;
                        input.UpdateBy = actorUserId;
                        input.Notes = AppendNote(input.Notes, $"Rollback leave payroll integration: {request.Reason}");
                        affected.Add(input.PayrollRunEmployeeId);
                        variableDeleted++;
                    }
                }

                if (variableBlocked > 0)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return LeavePayrollIntegrationServiceResult<LeavePayrollIntegrationRollbackResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        $"Terdapat {variableBlocked} variable input yang sudah submitted/verified/processed/posted dan tidak dapat di-rollback dengan opsi saat ini.");
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                var recalculated = await RecalculateRunEmployeeAggregatesAsync(affected, actorUserId, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return LeavePayrollIntegrationServiceResult<LeavePayrollIntegrationRollbackResponse>.Ok(
                    new LeavePayrollIntegrationRollbackResponse
                    {
                        PayrollRunId = context.Run.Id,
                        RunNumber = context.Run.RunNumber,
                        AttendanceInputResetCount = attendanceReset,
                        VariableInputDeletedCount = variableDeleted,
                        BlockedVariableInputCount = variableBlocked,
                        PayrollRunEmployeeRecalculatedCount = recalculated,
                        RolledBackAt = DateTime.UtcNow,
                        Reason = request.Reason.Trim()
                    },
                    "Leave payroll integration berhasil di-rollback.");
            }
            catch (OperationCanceledException)
            {
                await transaction.RollbackAsync(CancellationToken.None);
                throw;
            }
            catch (Exception exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                return LeavePayrollIntegrationServiceResult<LeavePayrollIntegrationRollbackResponse>.Fail(
                    StatusCodes.Status500InternalServerError,
                    $"Rollback leave payroll integration gagal. {Limit(exception.GetBaseException().Message, 500)}");
            }
        }

        private IQueryable<TrxLeaveAttendanceIntegration> BuildIntegrationQuery(PayrollContext context)
        {
            return _dbContext.Set<TrxLeaveAttendanceIntegration>()
                .Include(x => x.LeaveExecution)
                .Include(x => x.LeaveType)
                .Where(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    x.IntegrationStatus == LeavePayrollIntegrationValueConstants.IntegrationStatus.Applied &&
                    x.LeaveDate >= context.StartDate &&
                    x.LeaveDate <= context.EndDate);
        }

        private IQueryable<TrxLeaveCarryForward> BuildEncashmentQuery(PayrollContext context)
        {
            return _dbContext.Set<TrxLeaveCarryForward>()
                .Include(x => x.WorkforceProfile)
                .Where(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    x.CarryForwardStatus == "Posted" &&
                    x.PayoutDays > 0 &&
                    x.CalculationDate >= context.StartDate &&
                    x.CalculationDate <= context.EndDate);
        }

        private async Task<ContextLoadResult> LoadContextAsync(
            Guid payrollRunId,
            bool tracking,
            CancellationToken cancellationToken)
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

            return ContextLoadResult.Ok(new PayrollContext
            {
                Run = run,
                Period = period,
                StartDate = DateOnly.FromDateTime(period.StartDate),
                EndDate = DateOnly.FromDateTime(period.EndDate)
            });
        }

        private static List<string> BuildBlockingReasons(PayrollContext context)
        {
            var reasons = new List<string>();
            if (context.Run.IsLocked)
            {
                reasons.Add("Payroll run sedang dikunci.");
            }
            if (context.Period.IsLocked)
            {
                reasons.Add("Payroll period sedang dikunci.");
            }
            if (LeavePayrollIntegrationValueConstants.TerminalPayrollRunStatuses.Contains(context.Run.RunStatus))
            {
                reasons.Add($"Payroll run berstatus {context.Run.RunStatus} dan tidak dapat diubah.");
            }
            if (LeavePayrollIntegrationValueConstants.TerminalPayrollPeriodStatuses.Contains(context.Period.PayrollPeriodStatus))
            {
                reasons.Add($"Payroll period berstatus {context.Period.PayrollPeriodStatus} dan tidak dapat diubah.");
            }
            return reasons;
        }

        private static List<AttendanceGroup> BuildAttendanceGroups(IEnumerable<TrxLeaveAttendanceIntegration> integrations)
        {
            return integrations
                .GroupBy(x => new
                {
                    x.WorkforceProfileId,
                    x.LeaveDate
                })
                .Select(g => new AttendanceGroup
                {
                    RepresentativeId = g.OrderBy(x => x.Id).First().Id,
                    LeaveExecutionId = g.OrderBy(x => x.LeaveExecutionId).First().LeaveExecutionId,
                    LeaveRequestId = g.OrderBy(x => x.LeaveRequestId).First().LeaveRequestId,
                    WorkforceProfileId = g.Key.WorkforceProfileId,
                    LeaveTypeId = g.OrderBy(x => x.LeaveTypeId).First().LeaveTypeId,
                    LeaveTypeCode = g.Select(x => x.LeaveType != null ? x.LeaveType.LeaveTypeCode : null).FirstOrDefault(x => x != null),
                    LeaveTypeName = g.Select(x => x.LeaveType != null ? x.LeaveType.LeaveTypeName : null).FirstOrDefault(x => x != null),
                    LeaveDate = g.Key.LeaveDate,
                    PaidLeaveDays = g.Where(x => x.IsPaidLeave).Sum(x => x.RequestedLeaveDays),
                    UnpaidLeaveDays = g.Where(x => !x.IsPaidLeave).Sum(x => x.RequestedLeaveDays),
                    PayableLeaveMinutes = g.Sum(x => x.PayableLeaveMinutes),
                    IntegrationIds = g.Select(x => x.Id).ToList()
                })
                .OrderBy(x => x.LeaveDate)
                .ThenBy(x => x.WorkforceProfileId)
                .ToList();
        }

        private async Task<SupportData> LoadSupportAsync(
            PayrollContext context,
            IReadOnlyCollection<AttendanceGroup> groups,
            bool tracking,
            CancellationToken cancellationToken)
        {
            var workforceIds = groups.Select(x => x.WorkforceProfileId).Distinct().ToList();
            IQueryable<TrxPayrollRunEmployee> employeeQuery = _dbContext.Set<TrxPayrollRunEmployee>()
                .Where(x => x.PayrollRunId == context.Run.Id && !x.IsDelete);
            if (workforceIds.Count > 0)
            {
                employeeQuery = employeeQuery.Where(x => workforceIds.Contains(x.WorkforceProfileId));
            }
            if (!tracking)
            {
                employeeQuery = employeeQuery.AsNoTracking();
            }
            var runEmployees = await employeeQuery.ToListAsync(cancellationToken);
            var runEmployeeIds = runEmployees.Select(x => x.Id).ToList();

            IQueryable<TrxPayrollAttendanceInput> inputQuery = _dbContext.Set<TrxPayrollAttendanceInput>()
                .Where(x => !x.IsDelete && runEmployeeIds.Contains(x.PayrollRunEmployeeId));
            if (!tracking)
            {
                inputQuery = inputQuery.AsNoTracking();
            }
            var inputs = await inputQuery.ToListAsync(cancellationToken);

            IQueryable<TrxPayrollVariableInput> variableQuery = _dbContext.Set<TrxPayrollVariableInput>()
                .Where(x => !x.IsDelete && runEmployeeIds.Contains(x.PayrollRunEmployeeId));
            if (!tracking)
            {
                variableQuery = variableQuery.AsNoTracking();
            }
            var variableInputs = await variableQuery.ToListAsync(cancellationToken);

            return new SupportData
            {
                RunEmployees = runEmployees,
                Inputs = inputs,
                VariableInputs = variableInputs,
                RunEmployeeIds = runEmployeeIds,
                RunEmployeeByWorkforceId = runEmployees
                    .GroupBy(x => x.WorkforceProfileId)
                    .ToDictionary(x => x.Key, x => x.First()),
                InputByRunEmployeeAndDate = inputs
                    .GroupBy(x => (x.PayrollRunEmployeeId, x.AttendanceDate))
                    .ToDictionary(x => x.Key, x => x.OrderByDescending(y => y.ImportedAt).First())
            };
        }

        private static Evaluation Evaluate(AttendanceGroup group, SupportData support)
        {
            if (!support.RunEmployeeByWorkforceId.TryGetValue(group.WorkforceProfileId, out var runEmployee))
            {
                return Evaluation.Fail(
                    LeavePayrollIntegrationValueConstants.ReadinessStatus.MissingPayrollRunEmployee,
                    "Workforce belum tersedia pada payroll run employee.");
            }
            if (runEmployee.IsFinalized || runEmployee.IsSnapshotFrozen)
            {
                return Evaluation.Fail(
                    LeavePayrollIntegrationValueConstants.ReadinessStatus.PayrollEmployeeFrozen,
                    "Snapshot payroll employee sudah dibekukan atau difinalisasi.",
                    runEmployee);
            }
            support.InputByRunEmployeeAndDate.TryGetValue((runEmployee.Id, group.LeaveDate), out var input);
            if (input == null)
            {
                return Evaluation.Fail(
                    LeavePayrollIntegrationValueConstants.ReadinessStatus.MissingPayrollAttendanceInput,
                    "Payroll attendance input belum tersedia untuk tanggal cuti tersebut.",
                    runEmployee);
            }

            var synchronized = ApproximatelyEqual(input.PaidLeaveDays, group.PaidLeaveDays) &&
                               ApproximatelyEqual(input.UnpaidLeaveDays, group.UnpaidLeaveDays);
            return new Evaluation
            {
                CanWrite = true,
                Status = synchronized
                    ? LeavePayrollIntegrationValueConstants.ReadinessStatus.AlreadySynchronized
                    : LeavePayrollIntegrationValueConstants.ReadinessStatus.Ready,
                Message = synchronized
                    ? "Jumlah hari cuti pada payroll attendance input sudah sesuai."
                    : "Data siap disinkronkan.",
                RunEmployee = runEmployee,
                Input = input
            };
        }

        private static LeavePayrollIntegrationPreviewItemResponse MapPreview(
            AttendanceGroup group,
            Evaluation evaluation,
            MstWorkforceProfile? profile)
        {
            return new LeavePayrollIntegrationPreviewItemResponse
            {
                LeaveAttendanceIntegrationId = group.RepresentativeId,
                LeaveExecutionId = group.LeaveExecutionId,
                LeaveRequestId = group.LeaveRequestId,
                WorkforceProfileId = group.WorkforceProfileId,
                WorkforceProfileCode = profile?.ProfileCode,
                WorkforceDisplayName = profile?.DisplayName,
                LeaveTypeId = group.LeaveTypeId,
                LeaveTypeCode = group.LeaveTypeCode,
                LeaveTypeName = group.LeaveTypeName,
                LeaveDate = group.LeaveDate,
                ExpectedPaidLeaveDays = group.PaidLeaveDays,
                ExpectedUnpaidLeaveDays = group.UnpaidLeaveDays,
                PayableLeaveMinutes = group.PayableLeaveMinutes,
                PayrollRunEmployeeId = evaluation.RunEmployee?.Id,
                PayrollAttendanceInputId = evaluation.Input?.Id,
                ActualPaidLeaveDays = evaluation.Input?.PaidLeaveDays ?? 0,
                ActualUnpaidLeaveDays = evaluation.Input?.UnpaidLeaveDays ?? 0,
                IsPayrollEmployeeFrozen = evaluation.RunEmployee?.IsFinalized == true || evaluation.RunEmployee?.IsSnapshotFrozen == true,
                ReadinessStatus = evaluation.Status,
                IsReady = evaluation.CanWrite,
                Message = evaluation.Message
            };
        }

        private async Task CreateLeaveAllowanceInputsAsync(
            PayrollContext context,
            IReadOnlyCollection<TrxLeaveAttendanceIntegration> integrations,
            SupportData support,
            Guid actorUserId,
            bool submit,
            string? notes,
            List<LeavePayrollIntegrationExecutionItemResponse> items,
            HashSet<Guid> affectedRunEmployeeIds,
            List<string> warnings,
            CancellationToken cancellationToken)
        {
            var component = await ResolveComponentAsync(_options.LeaveAllowancePayrollComponentCode, cancellationToken);
            if (component == null)
            {
                warnings.Add("Payroll component untuk leave allowance tidak ditemukan. Variable input leave allowance tidak dibuat.");
                return;
            }

            var groups = integrations
                .Where(x => x.IsPaidLeave)
                .GroupBy(x => new { x.LeaveExecutionId, x.WorkforceProfileId })
                .Select(g => new
                {
                    g.Key.LeaveExecutionId,
                    g.Key.WorkforceProfileId,
                    Quantity = g.Sum(x => x.RequestedLeaveDays),
                    InputDate = g.Max(x => x.LeaveDate)
                })
                .Where(x => x.Quantity > Tolerance)
                .ToList();

            foreach (var group in groups)
            {
                if (!support.RunEmployeeByWorkforceId.TryGetValue(group.WorkforceProfileId, out var runEmployee))
                {
                    items.Add(FailedVariableItem(
                        LeavePayrollIntegrationValueConstants.SourceType.LeaveAllowance,
                        group.LeaveExecutionId,
                        group.WorkforceProfileId,
                        group.InputDate,
                        group.Quantity,
                        "Payroll run employee tidak ditemukan."));
                    continue;
                }

                var item = await UpsertVariableInputAsync(
                    context,
                    support,
                    runEmployee,
                    component,
                    LeavePayrollIntegrationValueConstants.SourceType.LeaveAllowance,
                    group.LeaveExecutionId,
                    group.InputDate,
                    group.Quantity,
                    actorUserId,
                    submit,
                    notes,
                    cancellationToken);
                items.Add(item);
                if (item.Success)
                {
                    affectedRunEmployeeIds.Add(runEmployee.Id);
                }
            }
        }

        private async Task CreateEncashmentInputsAsync(
            PayrollContext context,
            SupportData support,
            Guid actorUserId,
            bool submit,
            string? notes,
            List<LeavePayrollIntegrationExecutionItemResponse> items,
            HashSet<Guid> affectedRunEmployeeIds,
            List<string> warnings,
            CancellationToken cancellationToken)
        {
            var component = await ResolveComponentAsync(_options.LeaveEncashmentPayrollComponentCode, cancellationToken);
            if (component == null)
            {
                warnings.Add("Payroll component untuk leave encashment tidak ditemukan. Variable input encashment tidak dibuat.");
                return;
            }

            var encashments = await BuildEncashmentQuery(context)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
            foreach (var encashment in encashments)
            {
                if (!support.RunEmployeeByWorkforceId.TryGetValue(encashment.WorkforceProfileId, out var runEmployee))
                {
                    items.Add(FailedVariableItem(
                        LeavePayrollIntegrationValueConstants.SourceType.LeaveEncashment,
                        encashment.Id,
                        encashment.WorkforceProfileId,
                        encashment.CalculationDate,
                        encashment.PayoutDays,
                        "Payroll run employee tidak ditemukan."));
                    continue;
                }

                var item = await UpsertVariableInputAsync(
                    context,
                    support,
                    runEmployee,
                    component,
                    LeavePayrollIntegrationValueConstants.SourceType.LeaveEncashment,
                    encashment.Id,
                    encashment.CalculationDate,
                    encashment.PayoutDays,
                    actorUserId,
                    submit,
                    notes,
                    cancellationToken);
                items.Add(item);
                if (item.Success)
                {
                    affectedRunEmployeeIds.Add(runEmployee.Id);
                }
            }
        }

        private async Task<LeavePayrollIntegrationExecutionItemResponse> UpsertVariableInputAsync(
            PayrollContext context,
            SupportData support,
            TrxPayrollRunEmployee runEmployee,
            MstPayrollComponent component,
            string sourceType,
            Guid sourceId,
            DateOnly inputDate,
            decimal quantity,
            Guid actorUserId,
            bool submit,
            string? notes,
            CancellationToken cancellationToken)
        {
            var existing = support.VariableInputs.FirstOrDefault(x =>
                x.PayrollRunEmployeeId == runEmployee.Id &&
                x.PayrollComponentId == component.Id &&
                x.SourceType == sourceType &&
                x.SourceId == sourceId &&
                !x.IsDelete);

            if (existing == null)
            {
                existing = await _dbContext.Set<TrxPayrollVariableInput>()
                    .FirstOrDefaultAsync(x =>
                        x.PayrollRunEmployeeId == runEmployee.Id &&
                        x.PayrollComponentId == component.Id &&
                        x.SourceType == sourceType &&
                        x.SourceId == sourceId &&
                        !x.IsDelete,
                        cancellationToken);
                if (existing != null)
                {
                    support.VariableInputs.Add(existing);
                }
            }

            var response = new LeavePayrollIntegrationExecutionItemResponse
            {
                SourceId = sourceId,
                SourceType = sourceType,
                WorkforceProfileId = runEmployee.WorkforceProfileId,
                EffectiveDate = inputDate,
                PayrollRunEmployeeId = runEmployee.Id,
                Quantity = quantity
            };

            if (existing != null && LeavePayrollIntegrationValueConstants.TerminalVariableInputStatuses.Contains(existing.InputStatus))
            {
                response.PayrollVariableInputId = existing.Id;
                response.Success = false;
                response.ResultStatus = LeavePayrollIntegrationValueConstants.IssueType.TerminalVariableInput;
                response.Message = $"Variable input sudah berstatus {existing.InputStatus} dan tidak dapat diubah.";
                return response;
            }

            var created = existing == null;
            var previousQuantity = existing?.Quantity ?? 0;
            if (existing == null)
            {
                existing = new TrxPayrollVariableInput
                {
                    Id = Guid.NewGuid(),
                    InputNumber = GenerateVariableInputNumber(),
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId,
                    IsActive = true
                };
                _dbContext.Add(existing);
                support.VariableInputs.Add(existing);
            }

            existing.PayrollRunEmployeeId = runEmployee.Id;
            existing.PayrollComponentId = component.Id;
            existing.InputDate = inputDate;
            existing.InputType = LeavePayrollIntegrationValueConstants.InputType.LeaveIntegration;
            existing.InputStatus = submit
                ? LeavePayrollIntegrationValueConstants.InputStatus.Submitted
                : LeavePayrollIntegrationValueConstants.InputStatus.Draft;
            existing.CurrencyCode = NormalizeCurrency(_options.CurrencyCode);
            existing.Quantity = quantity;
            existing.Rate = 0m;
            existing.Amount = 0m;
            existing.SourceType = sourceType;
            existing.SourceId = sourceId;
            existing.SubmittedAt = submit ? DateTime.UtcNow : null;
            existing.SubmittedByUserId = submit ? actorUserId : null;
            existing.VerifiedAt = null;
            existing.VerifiedByUserId = null;
            existing.Notes = AppendNote(existing.Notes, notes ?? "Quantity leave dikirim oleh Leave Payroll Integration. Nominal dihitung pada Payroll Management.");
            existing.IsActive = true;
            existing.UpdateDateTime = DateTime.UtcNow;
            existing.UpdateBy = actorUserId;

            response.PayrollVariableInputId = existing.Id;
            response.Success = true;
            response.IsCreated = created;
            response.IsUpdated = !created && !ApproximatelyEqual(previousQuantity, quantity);
            response.IsIdempotent = !created && ApproximatelyEqual(previousQuantity, quantity);
            response.ResultStatus = created ? "Created" : response.IsUpdated ? "Updated" : "AlreadySynchronized";
            response.Message = created
                ? "Payroll variable input berhasil dibuat."
                : response.IsUpdated
                    ? "Quantity payroll variable input berhasil diperbarui."
                    : "Payroll variable input sudah sesuai.";
            return response;
        }

        private async Task<MstPayrollComponent?> ResolveComponentAsync(
            string? componentCode,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(componentCode))
            {
                return null;
            }
            var code = componentCode.Trim();
            var today = DateTime.UtcNow.Date;
            return await _dbContext.Set<MstPayrollComponent>()
                .FirstOrDefaultAsync(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    x.PayrollComponentCode == code &&
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= today) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= today),
                    cancellationToken);
        }

        private async Task<int> RecalculateRunEmployeeAggregatesAsync(
            IEnumerable<Guid> runEmployeeIds,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var ids = runEmployeeIds.Where(x => x != Guid.Empty).Distinct().ToList();
            if (ids.Count == 0)
            {
                return 0;
            }

            var employees = await _dbContext.Set<TrxPayrollRunEmployee>()
                .Where(x => ids.Contains(x.Id) && !x.IsDelete)
                .ToListAsync(cancellationToken);
            var totals = await _dbContext.Set<TrxPayrollAttendanceInput>()
                .AsNoTracking()
                .Where(x => ids.Contains(x.PayrollRunEmployeeId) && !x.IsDelete)
                .GroupBy(x => x.PayrollRunEmployeeId)
                .Select(g => new
                {
                    PayrollRunEmployeeId = g.Key,
                    PaidLeaveDays = g.Sum(x => x.PaidLeaveDays),
                    UnpaidLeaveDays = g.Sum(x => x.UnpaidLeaveDays)
                })
                .ToListAsync(cancellationToken);
            var totalMap = totals.ToDictionary(x => x.PayrollRunEmployeeId);

            foreach (var employee in employees)
            {
                if (totalMap.TryGetValue(employee.Id, out var total))
                {
                    employee.PaidLeaveDays = total.PaidLeaveDays;
                    employee.UnpaidLeaveDays = total.UnpaidLeaveDays;
                }
                else
                {
                    employee.PaidLeaveDays = 0;
                    employee.UnpaidLeaveDays = 0;
                }
                employee.UpdateDateTime = DateTime.UtcNow;
                employee.UpdateBy = actorUserId;
            }
            return employees.Count;
        }

        private async Task<Dictionary<Guid, MstWorkforceProfile>> LoadProfilesAsync(
            IEnumerable<Guid> workforceProfileIds,
            CancellationToken cancellationToken)
        {
            var ids = workforceProfileIds.Where(x => x != Guid.Empty).Distinct().ToList();
            return await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id) && !x.IsDelete)
                .ToDictionaryAsync(x => x.Id, cancellationToken);
        }

        private async Task ReconcileAllowanceInputsAsync(
            PayrollContext context,
            IReadOnlyCollection<TrxLeaveAttendanceIntegration> integrations,
            SupportData support,
            IReadOnlyDictionary<Guid, MstWorkforceProfile> profiles,
            List<LeavePayrollReconciliationItemResponse> issues,
            Action<int> addExpected,
            Action<int> addMatched,
            CancellationToken cancellationToken)
        {
            if (!_options.AutoCreateLeaveAllowanceVariableInputs || string.IsNullOrWhiteSpace(_options.LeaveAllowancePayrollComponentCode))
            {
                return;
            }
            var component = await ResolveComponentAsync(_options.LeaveAllowancePayrollComponentCode, cancellationToken);
            if (component == null)
            {
                return;
            }

            var groups = integrations
                .Where(x => x.IsPaidLeave)
                .GroupBy(x => new { x.LeaveExecutionId, x.WorkforceProfileId })
                .Select(g => new
                {
                    g.Key.LeaveExecutionId,
                    g.Key.WorkforceProfileId,
                    Quantity = g.Sum(x => x.RequestedLeaveDays),
                    InputDate = g.Max(x => x.LeaveDate)
                })
                .Where(x => x.Quantity > Tolerance)
                .ToList();
            addExpected(groups.Count);

            foreach (var group in groups)
            {
                if (!support.RunEmployeeByWorkforceId.TryGetValue(group.WorkforceProfileId, out var employee))
                {
                    continue;
                }
                var input = support.VariableInputs.FirstOrDefault(x =>
                    x.PayrollRunEmployeeId == employee.Id &&
                    x.PayrollComponentId == component.Id &&
                    x.SourceType == LeavePayrollIntegrationValueConstants.SourceType.LeaveAllowance &&
                    x.SourceId == group.LeaveExecutionId &&
                    !x.IsDelete);
                if (input == null)
                {
                    profiles.TryGetValue(group.WorkforceProfileId, out var profile);
                    issues.Add(new LeavePayrollReconciliationItemResponse
                    {
                        IssueType = LeavePayrollIntegrationValueConstants.IssueType.MissingLeaveAllowanceInput,
                        WorkforceProfileId = group.WorkforceProfileId,
                        WorkforceProfileCode = profile?.ProfileCode,
                        WorkforceDisplayName = profile?.DisplayName,
                        EffectiveDate = group.InputDate,
                        SourceId = group.LeaveExecutionId,
                        ExpectedQuantity = group.Quantity,
                        Message = "Payroll variable input leave allowance belum tersedia."
                    });
                }
                else if (!ApproximatelyEqual(input.Quantity, group.Quantity))
                {
                    profiles.TryGetValue(group.WorkforceProfileId, out var profile);
                    issues.Add(new LeavePayrollReconciliationItemResponse
                    {
                        IssueType = LeavePayrollIntegrationValueConstants.IssueType.VariableInputMismatch,
                        WorkforceProfileId = group.WorkforceProfileId,
                        WorkforceProfileCode = profile?.ProfileCode,
                        WorkforceDisplayName = profile?.DisplayName,
                        EffectiveDate = group.InputDate,
                        SourceId = group.LeaveExecutionId,
                        PayrollVariableInputId = input.Id,
                        ExpectedQuantity = group.Quantity,
                        ActualQuantity = input.Quantity,
                        Message = "Quantity leave allowance tidak sesuai."
                    });
                }
                else
                {
                    addMatched(1);
                }
            }
        }

        private async Task ReconcileEncashmentInputsAsync(
            PayrollContext context,
            SupportData support,
            IReadOnlyDictionary<Guid, MstWorkforceProfile> profiles,
            List<LeavePayrollReconciliationItemResponse> issues,
            Action<int> addExpected,
            Action<int> addMatched,
            CancellationToken cancellationToken)
        {
            if (!_options.AutoCreateEncashmentVariableInputs || string.IsNullOrWhiteSpace(_options.LeaveEncashmentPayrollComponentCode))
            {
                return;
            }
            var component = await ResolveComponentAsync(_options.LeaveEncashmentPayrollComponentCode, cancellationToken);
            if (component == null)
            {
                return;
            }

            var encashments = await BuildEncashmentQuery(context).AsNoTracking().ToListAsync(cancellationToken);
            addExpected(encashments.Count);
            foreach (var encashment in encashments)
            {
                if (!support.RunEmployeeByWorkforceId.TryGetValue(encashment.WorkforceProfileId, out var employee))
                {
                    continue;
                }
                var input = support.VariableInputs.FirstOrDefault(x =>
                    x.PayrollRunEmployeeId == employee.Id &&
                    x.PayrollComponentId == component.Id &&
                    x.SourceType == LeavePayrollIntegrationValueConstants.SourceType.LeaveEncashment &&
                    x.SourceId == encashment.Id &&
                    !x.IsDelete);
                if (input == null)
                {
                    profiles.TryGetValue(encashment.WorkforceProfileId, out var profile);
                    issues.Add(new LeavePayrollReconciliationItemResponse
                    {
                        IssueType = LeavePayrollIntegrationValueConstants.IssueType.MissingEncashmentInput,
                        WorkforceProfileId = encashment.WorkforceProfileId,
                        WorkforceProfileCode = profile?.ProfileCode,
                        WorkforceDisplayName = profile?.DisplayName,
                        EffectiveDate = encashment.CalculationDate,
                        SourceId = encashment.Id,
                        ExpectedQuantity = encashment.PayoutDays,
                        Message = "Payroll variable input leave encashment belum tersedia."
                    });
                }
                else if (!ApproximatelyEqual(input.Quantity, encashment.PayoutDays))
                {
                    profiles.TryGetValue(encashment.WorkforceProfileId, out var profile);
                    issues.Add(new LeavePayrollReconciliationItemResponse
                    {
                        IssueType = LeavePayrollIntegrationValueConstants.IssueType.VariableInputMismatch,
                        WorkforceProfileId = encashment.WorkforceProfileId,
                        WorkforceProfileCode = profile?.ProfileCode,
                        WorkforceDisplayName = profile?.DisplayName,
                        EffectiveDate = encashment.CalculationDate,
                        SourceId = encashment.Id,
                        PayrollVariableInputId = input.Id,
                        ExpectedQuantity = encashment.PayoutDays,
                        ActualQuantity = input.Quantity,
                        Message = "Quantity leave encashment tidak sesuai."
                    });
                }
                else
                {
                    addMatched(1);
                }
            }
        }

        private static LeavePayrollReconciliationItemResponse Issue(
            string issueType,
            AttendanceGroup group,
            MstWorkforceProfile? profile,
            decimal expected,
            decimal actual,
            string message,
            Guid? payrollAttendanceInputId = null)
        {
            return new LeavePayrollReconciliationItemResponse
            {
                IssueType = issueType,
                WorkforceProfileId = group.WorkforceProfileId,
                WorkforceProfileCode = profile?.ProfileCode,
                WorkforceDisplayName = profile?.DisplayName,
                EffectiveDate = group.LeaveDate,
                SourceId = group.RepresentativeId,
                PayrollAttendanceInputId = payrollAttendanceInputId,
                ExpectedQuantity = expected,
                ActualQuantity = actual,
                Message = message
            };
        }

        private static LeavePayrollIntegrationExecutionItemResponse FailedVariableItem(
            string sourceType,
            Guid sourceId,
            Guid workforceProfileId,
            DateOnly effectiveDate,
            decimal quantity,
            string message)
        {
            return new LeavePayrollIntegrationExecutionItemResponse
            {
                SourceType = sourceType,
                SourceId = sourceId,
                WorkforceProfileId = workforceProfileId,
                EffectiveDate = effectiveDate,
                Quantity = quantity,
                Success = false,
                ResultStatus = LeavePayrollIntegrationValueConstants.ReadinessStatus.MissingPayrollRunEmployee,
                Message = message
            };
        }

        private static IEnumerable<LeavePayrollIntegrationPreviewItemResponse> ApplyPreviewSort(
            IEnumerable<LeavePayrollIntegrationPreviewItemResponse> items,
            string? sortBy,
            string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "leaveDate").Trim().ToLowerInvariant() switch
            {
                "workforcedisplayname" => desc ? items.OrderByDescending(x => x.WorkforceDisplayName) : items.OrderBy(x => x.WorkforceDisplayName),
                "leavetypename" => desc ? items.OrderByDescending(x => x.LeaveTypeName) : items.OrderBy(x => x.LeaveTypeName),
                "paidleavedays" => desc ? items.OrderByDescending(x => x.ExpectedPaidLeaveDays) : items.OrderBy(x => x.ExpectedPaidLeaveDays),
                "unpaidleavedays" => desc ? items.OrderByDescending(x => x.ExpectedUnpaidLeaveDays) : items.OrderBy(x => x.ExpectedUnpaidLeaveDays),
                "readinessstatus" => desc ? items.OrderByDescending(x => x.ReadinessStatus) : items.OrderBy(x => x.ReadinessStatus),
                _ => desc ? items.OrderByDescending(x => x.LeaveDate).ThenByDescending(x => x.WorkforceDisplayName) : items.OrderBy(x => x.LeaveDate).ThenBy(x => x.WorkforceDisplayName)
            };
        }

        private static string SetLeavePayrollSnapshot(
            string? existingJson,
            PayrollContext context,
            AttendanceGroup group,
            Guid actorUserId,
            string? notes)
        {
            var root = ParseObject(existingJson);
            root["leavePayrollIntegration"] = JsonSerializer.SerializeToNode(new
            {
                payrollRunId = context.Run.Id,
                payrollPeriodId = context.Period.Id,
                group.LeaveExecutionId,
                group.LeaveRequestId,
                group.WorkforceProfileId,
                group.LeaveDate,
                paidLeaveDays = group.PaidLeaveDays,
                unpaidLeaveDays = group.UnpaidLeaveDays,
                group.PayableLeaveMinutes,
                integrationIds = group.IntegrationIds,
                synchronizedAt = DateTime.UtcNow,
                synchronizedByUserId = actorUserId,
                notes
            }, JsonOptions);
            return root.ToJsonString(JsonOptions);
        }

        private static string? RemoveLeavePayrollSnapshot(string? existingJson)
        {
            if (string.IsNullOrWhiteSpace(existingJson))
            {
                return existingJson;
            }
            var root = ParseObject(existingJson);
            root.Remove("leavePayrollIntegration");
            return root.ToJsonString(JsonOptions);
        }

        private static JsonObject ParseObject(string? json)
        {
            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    if (JsonNode.Parse(json) is JsonObject parsed)
                    {
                        return parsed;
                    }
                }
                catch
                {
                }
            }
            return new JsonObject();
        }

        private static string GenerateVariableInputNumber()
        {
            return $"LPI-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..31].ToUpperInvariant();
        }

        private static string NormalizeCurrency(string? value)
        {
            var currency = string.IsNullOrWhiteSpace(value) ? "IDR" : value.Trim().ToUpperInvariant();
            return currency.Length <= 3 ? currency : currency[..3];
        }

        private static bool ApproximatelyEqual(decimal left, decimal right)
        {
            return Math.Abs(left - right) <= Tolerance;
        }

        private static bool ContainsIgnoreCase(string? value, string keyword)
        {
            return !string.IsNullOrWhiteSpace(value) && value.Contains(keyword, StringComparison.OrdinalIgnoreCase);
        }

        private static LeavePayrollStringOptionResponse Option(string value, string label)
        {
            return new LeavePayrollStringOptionResponse { Value = value, Label = label };
        }

        private static string? AppendNote(string? current, string? addition)
        {
            if (string.IsNullOrWhiteSpace(addition)) return current;
            if (string.IsNullOrWhiteSpace(current)) return addition.Trim();
            var combined = $"{current.Trim()} | {addition.Trim()}";
            return combined.Length <= 1000 ? combined : combined[..1000];
        }

        private static string Limit(string value, int maxLength)
        {
            return value.Length <= maxLength ? value : value[..maxLength];
        }

        private sealed class PayrollContext
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
            public PayrollContext? Context { get; private set; }

            public static ContextLoadResult Ok(PayrollContext context)
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

        private sealed class AttendanceGroup
        {
            public Guid RepresentativeId { get; set; }
            public Guid LeaveExecutionId { get; set; }
            public Guid LeaveRequestId { get; set; }
            public Guid WorkforceProfileId { get; set; }
            public Guid LeaveTypeId { get; set; }
            public string? LeaveTypeCode { get; set; }
            public string? LeaveTypeName { get; set; }
            public DateOnly LeaveDate { get; set; }
            public decimal PaidLeaveDays { get; set; }
            public decimal UnpaidLeaveDays { get; set; }
            public int PayableLeaveMinutes { get; set; }
            public List<Guid> IntegrationIds { get; set; } = new();
        }

        private sealed class SupportData
        {
            public List<TrxPayrollRunEmployee> RunEmployees { get; set; } = new();
            public List<TrxPayrollAttendanceInput> Inputs { get; set; } = new();
            public List<TrxPayrollVariableInput> VariableInputs { get; set; } = new();
            public List<Guid> RunEmployeeIds { get; set; } = new();
            public Dictionary<Guid, TrxPayrollRunEmployee> RunEmployeeByWorkforceId { get; set; } = new();
            public Dictionary<(Guid PayrollRunEmployeeId, DateOnly AttendanceDate), TrxPayrollAttendanceInput> InputByRunEmployeeAndDate { get; set; } = new();
        }

        private sealed class Evaluation
        {
            public bool CanWrite { get; set; }
            public string Status { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public TrxPayrollRunEmployee? RunEmployee { get; set; }
            public TrxPayrollAttendanceInput? Input { get; set; }

            public static Evaluation Fail(
                string status,
                string message,
                TrxPayrollRunEmployee? runEmployee = null)
            {
                return new Evaluation
                {
                    CanWrite = false,
                    Status = status,
                    Message = message,
                    RunEmployee = runEmployee
                };
            }
        }
    }
}
