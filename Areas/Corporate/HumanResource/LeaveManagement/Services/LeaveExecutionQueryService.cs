using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    public class LeaveExecutionQueryService
    {
        private const decimal Tolerance = 0.0001m;
        private readonly ApplicationDbContext _dbContext;

        public LeaveExecutionQueryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public LeaveExecutionFilterMetadataResponse GetMetadata()
        {
            return new LeaveExecutionFilterMetadataResponse
            {
                ExecutionStatuses = new()
                {
                    Option(LeaveExecutionValueConstants.ExecutionStatus.Scheduled),
                    Option(LeaveExecutionValueConstants.ExecutionStatus.Active),
                    Option(LeaveExecutionValueConstants.ExecutionStatus.Completed),
                    Option(LeaveExecutionValueConstants.ExecutionStatus.Failed),
                    Option(LeaveExecutionValueConstants.ExecutionStatus.Cancelled),
                    Option(LeaveExecutionValueConstants.ExecutionStatus.Reversed)
                },
                IntegrationStatuses = new()
                {
                    Option(LeaveExecutionValueConstants.AttendanceIntegrationStatus.Pending),
                    Option(LeaveExecutionValueConstants.AttendanceIntegrationStatus.Applied),
                    Option(LeaveExecutionValueConstants.AttendanceIntegrationStatus.Conflict),
                    Option(LeaveExecutionValueConstants.AttendanceIntegrationStatus.Failed),
                    Option(LeaveExecutionValueConstants.AttendanceIntegrationStatus.Reversed),
                    Option(LeaveExecutionValueConstants.AttendanceIntegrationStatus.Skipped)
                },
                MonitoringStatuses = new()
                {
                    Option(LeaveExecutionValueConstants.MonitoringStatus.MissingExecution),
                    Option(LeaveExecutionValueConstants.MonitoringStatus.Scheduled),
                    Option(LeaveExecutionValueConstants.MonitoringStatus.StartDue),
                    Option(LeaveExecutionValueConstants.MonitoringStatus.Active),
                    Option(LeaveExecutionValueConstants.MonitoringStatus.CompletionDue),
                    Option(LeaveExecutionValueConstants.MonitoringStatus.Completed),
                    Option(LeaveExecutionValueConstants.MonitoringStatus.AttendanceConflict),
                    Option(LeaveExecutionValueConstants.MonitoringStatus.BalancePending),
                    Option(LeaveExecutionValueConstants.MonitoringStatus.Failed),
                    Option(LeaveExecutionValueConstants.MonitoringStatus.Cancelled),
                    Option(LeaveExecutionValueConstants.MonitoringStatus.Reversed)
                },
                SortOptions = new()
                {
                    new() { Value = "startDate", Label = "Tanggal mulai" },
                    new() { Value = "endDate", Label = "Tanggal selesai" },
                    new() { Value = "requestNumber", Label = "Nomor pengajuan" },
                    new() { Value = "employeeName", Label = "Nama employee" },
                    new() { Value = "status", Label = "Status execution" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                }
            };
        }

        public async Task<LeaveRequestServiceResult<LeaveExecutionSummaryResponse>> GetSummaryAsync(
            LeaveExecutionQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var rows = await BuildRowsAsync(request, cancellationToken);
            rows = ApplyComputedFilters(rows, request);
            return LeaveRequestServiceResult<LeaveExecutionSummaryResponse>.Ok(
                BuildSummary(rows),
                "Ringkasan leave execution berhasil diambil.");
        }

        public async Task<LeaveRequestServiceResult<LeaveExecutionPagedResponse>> GetPagedAsync(
            LeaveExecutionQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            request.PageNumber = Math.Max(1, request.PageNumber);
            request.PageSize = Math.Clamp(request.PageSize, 1, 100);

            var all = await BuildRowsAsync(request, cancellationToken);
            var filtered = ApplyComputedFilters(all, request);
            var sorted = ApplySort(filtered, request.SortBy, request.SortDirection);
            var total = sorted.Count;
            var items = sorted
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return LeaveRequestServiceResult<LeaveExecutionPagedResponse>.Ok(
                new LeaveExecutionPagedResponse
                {
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalData = total,
                    TotalPage = (int)Math.Ceiling(total / (double)request.PageSize),
                    Items = items
                },
                "Daftar leave execution berhasil diambil.");
        }

        public async Task<LeaveRequestServiceResult<LeaveExecutionDetailResponse>> GetByLeaveRequestIdAsync(
            Guid leaveRequestId,
            CancellationToken cancellationToken = default)
        {
            var request = await _dbContext.Set<WfpLeaveRequest>()
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.Employee)
                .Include(x => x.LeaveType)
                .Include(x => x.LeavePolicy)
                .FirstOrDefaultAsync(x => x.Id == leaveRequestId && !x.IsDelete, cancellationToken);

            if (request == null)
            {
                return LeaveRequestServiceResult<LeaveExecutionDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan cuti tidak ditemukan.");
            }

            var execution = await _dbContext.Set<TrxLeaveExecution>()
                .AsNoTracking()
                .Include(x => x.AttendanceIntegrations)
                .FirstOrDefaultAsync(x => x.LeaveRequestId == leaveRequestId && !x.IsDelete, cancellationToken);

            var item = await MapRowAsync(request, execution, cancellationToken);
            var detail = new LeaveExecutionDetailResponse
            {
                LeaveRequestId = item.LeaveRequestId,
                RequestNumber = item.RequestNumber,
                LeaveExecutionId = item.LeaveExecutionId,
                ExecutionNumber = item.ExecutionNumber,
                WorkforceProfileId = item.WorkforceProfileId,
                WorkforceProfileCode = item.WorkforceProfileCode,
                WorkforceDisplayName = item.WorkforceDisplayName,
                EmployeeNumber = item.EmployeeNumber,
                LeaveTypeId = item.LeaveTypeId,
                LeaveTypeName = item.LeaveTypeName,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                RequestedDays = item.RequestedDays,
                LeaveRequestStatus = item.LeaveRequestStatus,
                ExecutionStatus = item.ExecutionStatus,
                AttendanceIntegrationStatus = item.AttendanceIntegrationStatus,
                BalanceExecutionStatus = item.BalanceExecutionStatus,
                MonitoringStatus = item.MonitoringStatus,
                ExpectedAttendanceDayCount = item.ExpectedAttendanceDayCount,
                AppliedAttendanceDayCount = item.AppliedAttendanceDayCount,
                ConflictAttendanceDayCount = item.ConflictAttendanceDayCount,
                FailedAttendanceDayCount = item.FailedAttendanceDayCount,
                DeductionTiming = item.DeductionTiming,
                EstimatedBalanceDeduction = item.EstimatedBalanceDeduction,
                ActualBalanceDeduction = item.ActualBalanceDeduction,
                RequiresAttention = item.RequiresAttention,
                Issues = item.Issues,
                AvailableActions = item.AvailableActions,
                LeaveBalanceId = request.LeaveBalanceId,
                LeavePolicyId = request.LeavePolicyId,
                LeavePolicyCode = request.LeavePolicy?.LeavePolicyCode,
                IsPaidLeave = request.LeaveType?.IsPaidLeave ?? false,
                IsHalfDay = request.IsHalfDay,
                IsHourly = request.IsHourly,
                StartedAt = execution?.StartedAt,
                CompletedAt = execution?.CompletedAt,
                ReversedAt = execution?.ReversedAt,
                LastAttemptAt = execution?.LastAttemptAt,
                RetryCount = execution?.RetryCount ?? 0,
                CorrelationId = execution?.CorrelationId,
                ErrorSummary = execution?.ErrorSummary,
                ExecutionSnapshotJson = execution?.ExecutionSnapshotJson,
                ResultSnapshotJson = execution?.ResultSnapshotJson,
                AttendanceIntegrations = execution?.AttendanceIntegrations
                    .OrderBy(x => x.LeaveDate)
                    .Select(x => new LeaveAttendanceIntegrationResponse
                    {
                        Id = x.Id,
                        LeaveDate = x.LeaveDate,
                        AttendanceDailyId = x.AttendanceDailyId,
                        RequestedLeaveDays = x.RequestedLeaveDays,
                        RequestedMinutes = x.RequestedMinutes,
                        IsHalfDay = x.IsHalfDay,
                        IsHourly = x.IsHourly,
                        IsPaidLeave = x.IsPaidLeave,
                        ScheduledMinutes = x.ScheduledMinutes,
                        PayableLeaveMinutes = x.PayableLeaveMinutes,
                        IntegrationStatus = x.IntegrationStatus,
                        AttendanceStatusBefore = x.AttendanceStatusBefore,
                        AttendanceStatusAfter = x.AttendanceStatusAfter,
                        AppliedAt = x.AppliedAt,
                        ReversedAt = x.ReversedAt,
                        ErrorMessage = x.ErrorMessage
                    })
                    .ToList() ?? new()
            };

            return LeaveRequestServiceResult<LeaveExecutionDetailResponse>.Ok(
                detail,
                "Detail leave execution berhasil diambil.");
        }

        public async Task<LeaveRequestServiceResult<LeaveExecutionReconciliationResponse>> ReconcileAsync(
            Guid leaveRequestId,
            CancellationToken cancellationToken = default)
        {
            var request = await _dbContext.Set<WfpLeaveRequest>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == leaveRequestId && !x.IsDelete, cancellationToken);

            if (request == null)
            {
                return LeaveRequestServiceResult<LeaveExecutionReconciliationResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan cuti tidak ditemukan.");
            }

            var execution = await _dbContext.Set<TrxLeaveExecution>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.LeaveRequestId == leaveRequestId && !x.IsDelete, cancellationToken);

            var integrations = await _dbContext.Set<TrxLeaveAttendanceIntegration>()
                .AsNoTracking()
                .Where(x => x.LeaveRequestId == leaveRequestId && !x.IsDelete)
                .ToListAsync(cancellationToken);

            var ledger = await _dbContext.Set<TrxLeaveBalanceTransaction>()
                .AsNoTracking()
                .Where(x =>
                    x.LeaveRequestId == leaveRequestId &&
                    x.TransactionStatus == LeaveValueConstants.TransactionStatus.Posted &&
                    !x.IsDelete)
                .GroupBy(_ => 1)
                .Select(x => new
                {
                    Used = x.Sum(y => y.UsedDelta),
                    Reserved = x.Sum(y => y.ReservedDelta)
                })
                .FirstOrDefaultAsync(cancellationToken);

            var response = new LeaveExecutionReconciliationResponse
            {
                LeaveRequestId = request.Id,
                LeaveExecutionId = execution?.Id,
                LeaveRequestStatus = request.LeaveRequestStatus,
                ExecutionStatus = execution?.ExecutionStatus,
                ExpectedLeaveDays = request.RequestedDays,
                IntegratedLeaveDays = integrations
                    .Where(x => x.IntegrationStatus == LeaveExecutionValueConstants.AttendanceIntegrationStatus.Applied)
                    .Sum(x => x.RequestedLeaveDays),
                EstimatedBalanceDeduction = request.EstimatedBalanceDeduction,
                ActualBalanceDeduction = request.ActualBalanceDeduction,
                LedgerUsedDays = ledger?.Used ?? 0,
                LedgerReservedDays = Math.Max(0, ledger?.Reserved ?? 0),
                ExpectedAttendanceDayCount = integrations.Count,
                AppliedAttendanceDayCount = integrations.Count(x => x.IntegrationStatus == LeaveExecutionValueConstants.AttendanceIntegrationStatus.Applied),
                ConflictAttendanceDayCount = integrations.Count(x => x.IntegrationStatus == LeaveExecutionValueConstants.AttendanceIntegrationStatus.Conflict),
                FailedAttendanceDayCount = integrations.Count(x => x.IntegrationStatus == LeaveExecutionValueConstants.AttendanceIntegrationStatus.Failed)
            };

            if (execution == null)
            {
                response.Issues.Add("Leave execution belum tersedia.");
            }
            if (Math.Abs(response.ExpectedLeaveDays - integrations.Sum(x => x.RequestedLeaveDays)) > Tolerance)
            {
                response.Issues.Add("Total hari integration tidak sama dengan RequestedDays.");
            }
            if (response.ConflictAttendanceDayCount > 0)
            {
                response.Issues.Add("Masih terdapat attendance integration conflict.");
            }
            if (response.FailedAttendanceDayCount > 0)
            {
                response.Issues.Add("Masih terdapat attendance integration failed.");
            }
            if (request.ActualBalanceDeduction > Tolerance &&
                Math.Abs(request.ActualBalanceDeduction - response.LedgerUsedDays) > Tolerance)
            {
                response.Issues.Add("ActualBalanceDeduction tidak sama dengan total UsedDelta ledger.");
            }
            if (request.LeaveRequestStatus == LeaveRequestValueConstants.Status.Completed &&
                execution?.ExecutionStatus != LeaveExecutionValueConstants.ExecutionStatus.Completed)
            {
                response.Issues.Add("Request Completed tetapi execution belum Completed.");
            }

            response.IsBalanced = response.Issues.Count == 0;
            return LeaveRequestServiceResult<LeaveExecutionReconciliationResponse>.Ok(
                response,
                "Reconciliation leave execution berhasil dihitung.");
        }

        private async Task<List<LeaveExecutionListResponse>> BuildRowsAsync(
            LeaveExecutionQueryRequest request,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.Set<WfpLeaveRequest>()
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.Employee)
                .Include(x => x.LeaveType)
                .Include(x => x.LeavePolicy)
                .Where(x =>
                    (x.LeaveRequestStatus == LeaveRequestValueConstants.Status.Approved ||
                     x.LeaveRequestStatus == LeaveRequestValueConstants.Status.Taken ||
                     x.LeaveRequestStatus == LeaveRequestValueConstants.Status.Completed ||
                     x.LeaveRequestStatus == LeaveRequestValueConstants.Status.Cancelled) &&
                    !x.IsDelete);

            if (request.StartDate.HasValue) query = query.Where(x => x.EndDate >= request.StartDate.Value);
            if (request.EndDate.HasValue) query = query.Where(x => x.StartDate <= request.EndDate.Value);
            if (request.WorkforceProfileId.HasValue) query = query.Where(x => x.WorkforceProfileId == request.WorkforceProfileId.Value);
            if (request.LeaveTypeId.HasValue) query = query.Where(x => x.LeaveTypeId == request.LeaveTypeId.Value);
            if (request.HospitalSiteId.HasValue) query = query.Where(x => x.HospitalSiteId == request.HospitalSiteId.Value);
            if (request.OrganizationUnitId.HasValue) query = query.Where(x => x.OrganizationUnitId == request.OrganizationUnitId.Value);
            if (request.DepartmentId.HasValue) query = query.Where(x => x.DepartmentId == request.DepartmentId.Value);
            if (!string.IsNullOrWhiteSpace(request.LeaveRequestStatus)) query = query.Where(x => x.LeaveRequestStatus == request.LeaveRequestStatus);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim().ToLower();
                query = query.Where(x =>
                    x.RequestNumber.ToLower().Contains(keyword) ||
                    (x.WorkforceProfile != null && x.WorkforceProfile.DisplayName.ToLower().Contains(keyword)) ||
                    (x.LeaveType != null && x.LeaveType.LeaveTypeName.ToLower().Contains(keyword)));
            }

            var requests = await query
                .OrderBy(x => x.StartDate)
                .Take(5000)
                .ToListAsync(cancellationToken);

            var requestIds = requests.Select(x => x.Id).ToList();
            var executions = requestIds.Count == 0
                ? new List<TrxLeaveExecution>()
                : await _dbContext.Set<TrxLeaveExecution>()
                    .AsNoTracking()
                    .Where(x => requestIds.Contains(x.LeaveRequestId) && !x.IsDelete)
                    .ToListAsync(cancellationToken);
            var executionMap = executions.ToDictionary(x => x.LeaveRequestId);

            var rows = new List<LeaveExecutionListResponse>();
            foreach (var leaveRequest in requests)
            {
                executionMap.TryGetValue(leaveRequest.Id, out var execution);
                rows.Add(await MapRowAsync(leaveRequest, execution, cancellationToken));
            }
            return rows;
        }

        private async Task<LeaveExecutionListResponse> MapRowAsync(
            WfpLeaveRequest request,
            TrxLeaveExecution? execution,
            CancellationToken cancellationToken)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
            var integrations = execution == null
                ? new List<TrxLeaveAttendanceIntegration>()
                : await _dbContext.Set<TrxLeaveAttendanceIntegration>()
                    .AsNoTracking()
                    .Where(x => x.LeaveExecutionId == execution.Id && !x.IsDelete)
                    .ToListAsync(cancellationToken);

            var monitoringStatus = ResolveMonitoringStatus(request, execution, integrations, today);
            var issues = ResolveIssues(request, execution, integrations, monitoringStatus);
            var actions = new List<string> { "View", "Reconcile" };

            if (execution == null) actions.Add("Execute");
            if (monitoringStatus == LeaveExecutionValueConstants.MonitoringStatus.StartDue ||
                monitoringStatus == LeaveExecutionValueConstants.MonitoringStatus.CompletionDue ||
                monitoringStatus == LeaveExecutionValueConstants.MonitoringStatus.Failed ||
                monitoringStatus == LeaveExecutionValueConstants.MonitoringStatus.AttendanceConflict ||
                monitoringStatus == LeaveExecutionValueConstants.MonitoringStatus.BalancePending)
            {
                actions.Add("Retry");
            }
            if (execution != null &&
                execution.ExecutionStatus != LeaveExecutionValueConstants.ExecutionStatus.Reversed &&
                execution.ExecutionStatus != LeaveExecutionValueConstants.ExecutionStatus.Cancelled)
            {
                actions.Add("Reverse");
            }

            return new LeaveExecutionListResponse
            {
                LeaveRequestId = request.Id,
                RequestNumber = request.RequestNumber,
                LeaveExecutionId = execution?.Id,
                ExecutionNumber = execution?.ExecutionNumber,
                WorkforceProfileId = request.WorkforceProfileId,
                WorkforceProfileCode = request.WorkforceProfile?.ProfileCode,
                WorkforceDisplayName = request.WorkforceProfile?.DisplayName,
                EmployeeNumber = request.Employee?.EmployeeNumber,
                LeaveTypeId = request.LeaveTypeId,
                LeaveTypeName = request.LeaveType?.LeaveTypeName ?? string.Empty,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                RequestedDays = request.RequestedDays,
                LeaveRequestStatus = request.LeaveRequestStatus,
                ExecutionStatus = execution?.ExecutionStatus,
                AttendanceIntegrationStatus = execution?.AttendanceIntegrationStatus,
                BalanceExecutionStatus = execution?.BalanceExecutionStatus,
                MonitoringStatus = monitoringStatus,
                ExpectedAttendanceDayCount = execution?.ExpectedAttendanceDayCount ?? integrations.Count,
                AppliedAttendanceDayCount = integrations.Count(x => x.IntegrationStatus == LeaveExecutionValueConstants.AttendanceIntegrationStatus.Applied),
                ConflictAttendanceDayCount = integrations.Count(x => x.IntegrationStatus == LeaveExecutionValueConstants.AttendanceIntegrationStatus.Conflict),
                FailedAttendanceDayCount = integrations.Count(x => x.IntegrationStatus == LeaveExecutionValueConstants.AttendanceIntegrationStatus.Failed),
                DeductionTiming = request.LeavePolicy?.DeductionTiming,
                EstimatedBalanceDeduction = request.EstimatedBalanceDeduction,
                ActualBalanceDeduction = request.ActualBalanceDeduction,
                RequiresAttention = issues.Count > 0,
                Issues = issues,
                AvailableActions = actions.Distinct().ToList()
            };
        }

        private static string ResolveMonitoringStatus(
            WfpLeaveRequest request,
            TrxLeaveExecution? execution,
            List<TrxLeaveAttendanceIntegration> integrations,
            DateOnly today)
        {
            if (execution == null) return LeaveExecutionValueConstants.MonitoringStatus.MissingExecution;
            if (execution.ExecutionStatus == LeaveExecutionValueConstants.ExecutionStatus.Reversed) return LeaveExecutionValueConstants.MonitoringStatus.Reversed;
            if (execution.ExecutionStatus == LeaveExecutionValueConstants.ExecutionStatus.Cancelled) return LeaveExecutionValueConstants.MonitoringStatus.Cancelled;
            if (execution.ExecutionStatus == LeaveExecutionValueConstants.ExecutionStatus.Failed ||
                integrations.Any(x => x.IntegrationStatus == LeaveExecutionValueConstants.AttendanceIntegrationStatus.Failed))
                return LeaveExecutionValueConstants.MonitoringStatus.Failed;
            if (integrations.Any(x => x.IntegrationStatus == LeaveExecutionValueConstants.AttendanceIntegrationStatus.Conflict))
                return LeaveExecutionValueConstants.MonitoringStatus.AttendanceConflict;
            if (request.LeaveBalanceId.HasValue &&
                request.LeavePolicy?.DeductionTiming == LeaveValueConstants.DeductionTiming.OnLeaveStart &&
                today >= request.StartDate &&
                request.ActualBalanceDeduction + Tolerance < request.EstimatedBalanceDeduction)
                return LeaveExecutionValueConstants.MonitoringStatus.BalancePending;
            if (request.LeaveBalanceId.HasValue &&
                request.LeavePolicy?.DeductionTiming == LeaveValueConstants.DeductionTiming.OnCompletion &&
                today > request.EndDate &&
                request.ActualBalanceDeduction + Tolerance < request.EstimatedBalanceDeduction)
                return LeaveExecutionValueConstants.MonitoringStatus.BalancePending;
            if (execution.ExecutionStatus == LeaveExecutionValueConstants.ExecutionStatus.Completed)
                return LeaveExecutionValueConstants.MonitoringStatus.Completed;
            if (today > request.EndDate)
                return LeaveExecutionValueConstants.MonitoringStatus.CompletionDue;
            if (request.StartDate == today && execution.ExecutionStatus == LeaveExecutionValueConstants.ExecutionStatus.Scheduled)
                return LeaveExecutionValueConstants.MonitoringStatus.StartDue;
            if (today >= request.StartDate)
                return LeaveExecutionValueConstants.MonitoringStatus.Active;
            return LeaveExecutionValueConstants.MonitoringStatus.Scheduled;
        }

        private static List<string> ResolveIssues(
            WfpLeaveRequest request,
            TrxLeaveExecution? execution,
            List<TrxLeaveAttendanceIntegration> integrations,
            string monitoringStatus)
        {
            var issues = new List<string>();
            if (execution == null) issues.Add("Approved leave belum mempunyai execution record.");
            if (integrations.Any(x => x.IntegrationStatus == LeaveExecutionValueConstants.AttendanceIntegrationStatus.Conflict))
                issues.Add("Terdapat attendance integration conflict.");
            if (integrations.Any(x => x.IntegrationStatus == LeaveExecutionValueConstants.AttendanceIntegrationStatus.Failed))
                issues.Add("Terdapat attendance integration failed.");
            if (request.HasRosterConflict) issues.Add("Pengajuan mempunyai roster conflict snapshot.");
            if (request.HasTrainingConflict) issues.Add("Pengajuan mempunyai training conflict.");
            if (request.HasCriticalStaffingImpact) issues.Add("Pengajuan ditandai berdampak pada critical staffing.");
            if (monitoringStatus == LeaveExecutionValueConstants.MonitoringStatus.BalancePending)
                issues.Add("Dampak balance pada tahap execution belum selesai.");
            if (monitoringStatus == LeaveExecutionValueConstants.MonitoringStatus.CompletionDue)
                issues.Add("Tanggal leave sudah selesai tetapi execution belum completed.");
            return issues;
        }

        private static List<LeaveExecutionListResponse> ApplyComputedFilters(
            List<LeaveExecutionListResponse> rows,
            LeaveExecutionQueryRequest request)
        {
            IEnumerable<LeaveExecutionListResponse> query = rows;
            if (!string.IsNullOrWhiteSpace(request.ExecutionStatus))
                query = query.Where(x => x.ExecutionStatus == request.ExecutionStatus);
            if (!string.IsNullOrWhiteSpace(request.IntegrationStatus))
                query = query.Where(x => x.AttendanceIntegrationStatus == request.IntegrationStatus);
            if (!string.IsNullOrWhiteSpace(request.MonitoringStatus))
                query = query.Where(x => x.MonitoringStatus == request.MonitoringStatus);
            if (request.RequiresAttention.HasValue)
                query = query.Where(x => x.RequiresAttention == request.RequiresAttention.Value);
            return query.ToList();
        }

        private static List<LeaveExecutionListResponse> ApplySort(
            List<LeaveExecutionListResponse> rows,
            string? sortBy,
            string? direction)
        {
            var desc = !string.Equals(direction, "asc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "enddate" => desc ? rows.OrderByDescending(x => x.EndDate).ToList() : rows.OrderBy(x => x.EndDate).ToList(),
                "requestnumber" => desc ? rows.OrderByDescending(x => x.RequestNumber).ToList() : rows.OrderBy(x => x.RequestNumber).ToList(),
                "employeename" => desc ? rows.OrderByDescending(x => x.WorkforceDisplayName).ToList() : rows.OrderBy(x => x.WorkforceDisplayName).ToList(),
                "status" => desc ? rows.OrderByDescending(x => x.MonitoringStatus).ToList() : rows.OrderBy(x => x.MonitoringStatus).ToList(),
                "createdatetime" => desc ? rows.OrderByDescending(x => x.ExecutionNumber).ToList() : rows.OrderBy(x => x.ExecutionNumber).ToList(),
                _ => desc ? rows.OrderByDescending(x => x.StartDate).ToList() : rows.OrderBy(x => x.StartDate).ToList()
            };
        }

        private static LeaveExecutionSummaryResponse BuildSummary(List<LeaveExecutionListResponse> rows)
        {
            return new LeaveExecutionSummaryResponse
            {
                TotalApprovedRequest = rows.Count,
                MissingExecution = rows.Count(x => x.MonitoringStatus == LeaveExecutionValueConstants.MonitoringStatus.MissingExecution),
                Scheduled = rows.Count(x => x.MonitoringStatus == LeaveExecutionValueConstants.MonitoringStatus.Scheduled),
                StartDue = rows.Count(x => x.MonitoringStatus == LeaveExecutionValueConstants.MonitoringStatus.StartDue),
                Active = rows.Count(x => x.MonitoringStatus == LeaveExecutionValueConstants.MonitoringStatus.Active),
                CompletionDue = rows.Count(x => x.MonitoringStatus == LeaveExecutionValueConstants.MonitoringStatus.CompletionDue),
                Completed = rows.Count(x => x.MonitoringStatus == LeaveExecutionValueConstants.MonitoringStatus.Completed),
                AttendanceConflict = rows.Count(x => x.MonitoringStatus == LeaveExecutionValueConstants.MonitoringStatus.AttendanceConflict),
                BalancePending = rows.Count(x => x.MonitoringStatus == LeaveExecutionValueConstants.MonitoringStatus.BalancePending),
                Failed = rows.Count(x => x.MonitoringStatus == LeaveExecutionValueConstants.MonitoringStatus.Failed),
                RequiresAttention = rows.Count(x => x.RequiresAttention),
                PendingAttendanceDay = rows.Sum(x => Math.Max(0, x.ExpectedAttendanceDayCount - x.AppliedAttendanceDayCount - x.ConflictAttendanceDayCount - x.FailedAttendanceDayCount)),
                AppliedAttendanceDay = rows.Sum(x => x.AppliedAttendanceDayCount),
                ConflictAttendanceDay = rows.Sum(x => x.ConflictAttendanceDayCount),
                FailedAttendanceDay = rows.Sum(x => x.FailedAttendanceDayCount)
            };
        }

        private static LeaveExecutionOptionResponse Option(string value)
        {
            return new LeaveExecutionOptionResponse { Value = value, Label = value };
        }
    }
}
