using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    public class LeaveCalendarService
    {
        private static readonly string[] PublicCalendarStatuses =
        {
            LeaveRequestValueConstants.Status.Approved,
            LeaveRequestValueConstants.Status.Taken,
            LeaveRequestValueConstants.Status.Completed
        };

        private static readonly string[] PendingStatuses =
        {
            LeaveRequestValueConstants.Status.Submitted,
            LeaveRequestValueConstants.Status.WaitingApproval
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LeaveRequestCalculationService _calculationService;

        public LeaveCalendarService(
            ApplicationDbContext dbContext,
            LeaveRequestCalculationService calculationService)
        {
            _dbContext = dbContext;
            _calculationService = calculationService;
        }

        public async Task<LeaveRequestServiceResult<LeaveCalendarResponse>> GetAdminCalendarAsync(
            LeaveCalendarQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            return await GetCalendarCoreAsync(request, null, cancellationToken);
        }

        public async Task<LeaveRequestServiceResult<LeaveCalendarResponse>> GetMyCalendarAsync(
            Guid actorUserId,
            LeaveCalendarQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = await _calculationService.GetActorContextAsync(actorUserId, cancellationToken);
            if (!actor.Success || actor.Data == null)
            {
                return LeaveRequestServiceResult<LeaveCalendarResponse>.Fail(actor.StatusCode, actor.Message);
            }

            request.WorkforceProfileId = actor.Data.WorkforceProfileId;
            return await GetCalendarCoreAsync(request, new HashSet<Guid> { actor.Data.WorkforceProfileId }, cancellationToken);
        }

        public async Task<LeaveRequestServiceResult<LeaveCalendarResponse>> GetTeamCalendarAsync(
            Guid actorUserId,
            LeaveCalendarQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = await _calculationService.GetActorContextAsync(actorUserId, cancellationToken);
            if (!actor.Success || actor.Data == null)
            {
                return LeaveRequestServiceResult<LeaveCalendarResponse>.Fail(actor.StatusCode, actor.Message);
            }

            var now = DateTime.UtcNow;
            var directReports = await _dbContext.Set<WfpManagerAssignment>()
                .AsNoTracking()
                .Where(x =>
                    x.ManagerWorkforceProfileId == actor.Data.WorkforceProfileId &&
                    x.IsActive &&
                    x.IsPrimaryManager &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.EffectiveStartDate <= now &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= now))
                .Select(x => x.WorkforceProfileId)
                .Distinct()
                .ToListAsync(cancellationToken);

            var allowed = directReports.ToHashSet();
            allowed.Add(actor.Data.WorkforceProfileId);

            if (request.WorkforceProfileId.HasValue && !allowed.Contains(request.WorkforceProfileId.Value))
            {
                return LeaveRequestServiceResult<LeaveCalendarResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Workforce yang dipilih bukan bagian dari tim manager login.");
            }

            return await GetCalendarCoreAsync(request, allowed, cancellationToken);
        }

        private async Task<LeaveRequestServiceResult<LeaveCalendarResponse>> GetCalendarCoreAsync(
            LeaveCalendarQueryRequest request,
            HashSet<Guid>? allowedWorkforceIds,
            CancellationToken cancellationToken)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
            var startDate = request.StartDate ?? new DateOnly(today.Year, today.Month, 1);
            var endDate = request.EndDate ?? startDate.AddMonths(1).AddDays(-1);

            if (endDate < startDate)
            {
                return LeaveRequestServiceResult<LeaveCalendarResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tanggal selesai tidak boleh lebih kecil daripada tanggal mulai.");
            }

            if (endDate.DayNumber - startDate.DayNumber + 1 > 366)
            {
                return LeaveRequestServiceResult<LeaveCalendarResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Rentang leave calendar maksimal 366 hari.");
            }

            request.MaximumItem = Math.Clamp(request.MaximumItem, 1, 2000);

            var acceptedStatuses = request.IncludePending
                ? PublicCalendarStatuses.Concat(PendingStatuses).ToArray()
                : PublicCalendarStatuses;

            var query = _dbContext.Set<WfpLeaveRequest>()
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.Employee)
                .Include(x => x.LeaveType)
                .Include(x => x.Department)
                .Include(x => x.Position)
                .Include(x => x.ReplacementWorkforceProfile)
                .Where(x =>
                    acceptedStatuses.Contains(x.LeaveRequestStatus) &&
                    x.StartDate <= endDate &&
                    x.EndDate >= startDate &&
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel);

            if (allowedWorkforceIds != null)
            {
                query = query.Where(x => allowedWorkforceIds.Contains(x.WorkforceProfileId));
            }
            if (request.WorkforceProfileId.HasValue) query = query.Where(x => x.WorkforceProfileId == request.WorkforceProfileId.Value);
            if (request.LeaveTypeId.HasValue) query = query.Where(x => x.LeaveTypeId == request.LeaveTypeId.Value);
            if (request.HospitalSiteId.HasValue) query = query.Where(x => x.HospitalSiteId == request.HospitalSiteId.Value);
            if (request.OrganizationUnitId.HasValue) query = query.Where(x => x.OrganizationUnitId == request.OrganizationUnitId.Value);
            if (request.DepartmentId.HasValue) query = query.Where(x => x.DepartmentId == request.DepartmentId.Value);
            if (request.PositionId.HasValue) query = query.Where(x => x.PositionId == request.PositionId.Value);
            if (!string.IsNullOrWhiteSpace(request.LeaveRequestStatus)) query = query.Where(x => x.LeaveRequestStatus == request.LeaveRequestStatus);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim().ToLower();
                query = query.Where(x =>
                    x.RequestNumber.ToLower().Contains(keyword) ||
                    (x.WorkforceProfile != null && x.WorkforceProfile.DisplayName.ToLower().Contains(keyword)) ||
                    (x.LeaveType != null && x.LeaveType.LeaveTypeName.ToLower().Contains(keyword)) ||
                    (x.Department != null && x.Department.DepartmentName.ToLower().Contains(keyword)));
            }

            var rows = await query
                .OrderBy(x => x.StartDate)
                .ThenBy(x => x.WorkforceProfile!.DisplayName)
                .Take(request.MaximumItem)
                .ToListAsync(cancellationToken);

            var requestIds = rows.Select(x => x.Id).ToList();
            var executions = requestIds.Count == 0
                ? new List<TrxLeaveExecution>()
                : await _dbContext.Set<TrxLeaveExecution>()
                    .AsNoTracking()
                    .Where(x => requestIds.Contains(x.LeaveRequestId) && !x.IsDelete)
                    .ToListAsync(cancellationToken);
            var executionMap = executions.ToDictionary(x => x.LeaveRequestId);

            var items = rows.Select(x =>
            {
                executionMap.TryGetValue(x.Id, out var execution);
                return new LeaveCalendarEntryResponse
                {
                    LeaveRequestId = x.Id,
                    RequestNumber = x.RequestNumber,
                    WorkforceProfileId = x.WorkforceProfileId,
                    WorkforceProfileCode = x.WorkforceProfile?.ProfileCode,
                    WorkforceDisplayName = x.WorkforceProfile?.DisplayName,
                    EmployeeNumber = x.Employee?.EmployeeNumber,
                    LeaveTypeId = x.LeaveTypeId,
                    LeaveTypeCode = x.LeaveType?.LeaveTypeCode ?? string.Empty,
                    LeaveTypeName = x.LeaveType?.LeaveTypeName ?? string.Empty,
                    LeaveCategory = x.LeaveType?.LeaveCategory ?? string.Empty,
                    ColorCode = x.LeaveType?.ColorCode,
                    IsPaidLeave = x.LeaveType?.IsPaidLeave ?? false,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    RequestedDays = x.RequestedDays,
                    IsHalfDay = x.IsHalfDay,
                    HalfDayPeriod = x.HalfDayPeriod,
                    IsHourly = x.IsHourly,
                    RequestedMinutes = x.RequestedMinutes,
                    LeaveRequestStatus = x.LeaveRequestStatus,
                    ExecutionStatus = execution?.ExecutionStatus,
                    AttendanceIntegrationStatus = execution?.AttendanceIntegrationStatus,
                    DepartmentId = x.DepartmentId,
                    DepartmentName = x.Department?.DepartmentName,
                    PositionId = x.PositionId,
                    PositionName = x.Position?.PositionName,
                    ReplacementWorkforceProfileId = x.ReplacementWorkforceProfileId,
                    ReplacementWorkforceName = x.ReplacementWorkforceProfile?.DisplayName,
                    HasRosterConflict = x.HasRosterConflict,
                    HasTrainingConflict = x.HasTrainingConflict,
                    HasCriticalStaffingImpact = x.HasCriticalStaffingImpact,
                    IsPendingApproval = PendingStatuses.Contains(x.LeaveRequestStatus),
                    IsCurrentLeave = x.StartDate <= today && x.EndDate >= today,
                    IsUpcomingLeave = x.StartDate > today
                };
            }).ToList();

            var summary = new LeaveCalendarSummaryResponse
            {
                TotalLeaveRequest = items.Count,
                DistinctEmployee = items.Select(x => x.WorkforceProfileId).Distinct().Count(),
                Approved = items.Count(x => x.LeaveRequestStatus == LeaveRequestValueConstants.Status.Approved),
                Active = items.Count(x => x.LeaveRequestStatus == LeaveRequestValueConstants.Status.Taken || x.IsCurrentLeave),
                Completed = items.Count(x => x.LeaveRequestStatus == LeaveRequestValueConstants.Status.Completed),
                PendingApproval = items.Count(x => x.IsPendingApproval),
                PaidLeave = items.Count(x => x.IsPaidLeave),
                UnpaidLeave = items.Count(x => !x.IsPaidLeave),
                RosterConflict = items.Count(x => x.HasRosterConflict),
                CriticalStaffingImpact = items.Count(x => x.HasCriticalStaffingImpact),
                TotalLeaveDays = items.Sum(x => x.RequestedDays),
                ByLeaveType = items
                    .GroupBy(x => new { x.LeaveTypeCode, x.LeaveTypeName })
                    .Select(x => new LeaveCalendarBreakdownResponse
                    {
                        Key = x.Key.LeaveTypeCode,
                        Label = x.Key.LeaveTypeName,
                        Count = x.Count(),
                        TotalDays = x.Sum(y => y.RequestedDays)
                    })
                    .OrderByDescending(x => x.TotalDays)
                    .ToList(),
                ByDepartment = items
                    .GroupBy(x => new { x.DepartmentId, Name = x.DepartmentName ?? "Tanpa Departemen" })
                    .Select(x => new LeaveCalendarBreakdownResponse
                    {
                        Key = x.Key.DepartmentId?.ToString() ?? "NONE",
                        Label = x.Key.Name,
                        Count = x.Count(),
                        TotalDays = x.Sum(y => y.RequestedDays)
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList()
            };

            return LeaveRequestServiceResult<LeaveCalendarResponse>.Ok(
                new LeaveCalendarResponse
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalItem = items.Count,
                    IsTruncated = items.Count >= request.MaximumItem,
                    Summary = summary,
                    Items = items
                },
                "Leave calendar berhasil diambil.");
        }
    }
}
