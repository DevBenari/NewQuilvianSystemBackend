using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public class OvertimePeriodQueryService
    {
        private readonly ApplicationDbContext _dbContext;

        public OvertimePeriodQueryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public OvertimePeriodFilterMetadataResponse GetMetadata() => new()
        {
            PeriodStatuses = OvertimeValueConstants.PeriodStatus.All.ToList(),
            SortFields = new List<string>
            {
                "startDate",
                "endDate",
                "periodCode",
                "periodName",
                "periodStatus",
                "scheduledCloseAt",
                "createDateTime"
            },
            SortDirections = new List<string> { "asc", "desc" },
            PageSizeOptions = new List<int> { 10, 25, 50, 100 }
        };

        public async Task<OvertimePeriodSummaryResponse> GetSummaryAsync(
            OvertimePeriodQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter(BuildBaseQuery(), request);
            var now = DateTime.UtcNow;
            var rows = await query.Select(x => new
            {
                x.PeriodStatus,
                x.ScheduledCloseAt,
                x.ValidationSnapshotJson
            }).ToListAsync(cancellationToken);

            return new OvertimePeriodSummaryResponse
            {
                TotalPeriod = rows.Count,
                OpenPeriod = rows.Count(x => x.PeriodStatus == OvertimeValueConstants.PeriodStatus.Open),
                ClosingPeriod = rows.Count(x => x.PeriodStatus == OvertimeValueConstants.PeriodStatus.Closing),
                ClosedPeriod = rows.Count(x => x.PeriodStatus == OvertimeValueConstants.PeriodStatus.Closed),
                ReopenedPeriod = rows.Count(x => x.PeriodStatus == OvertimeValueConstants.PeriodStatus.Reopened),
                ScheduledToClose = rows.Count(x => x.ScheduledCloseAt.HasValue && x.ScheduledCloseAt <= now &&
                    (x.PeriodStatus == OvertimeValueConstants.PeriodStatus.Open || x.PeriodStatus == OvertimeValueConstants.PeriodStatus.Reopened)),
                PeriodWithBlockingIssue = rows.Count(x => x.ValidationSnapshotJson != null && x.ValidationSnapshotJson.Contains("\"BlockingCount\":0") == false)
            };
        }

        public async Task<PagedResult<OvertimePeriodListResponse>> GetPagedAsync(
            OvertimePeriodQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            NormalizePaging(request);
            var query = ApplySorting(ApplyFilter(BuildBaseQuery(), request), request.SortBy, request.SortDirection);
            var totalData = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new OvertimePeriodListResponse
                {
                    Id = x.Id,
                    PeriodCode = x.PeriodCode,
                    PeriodName = x.PeriodName,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    LegalEntityId = x.LegalEntityId,
                    LegalEntityName = x.LegalEntity != null ? x.LegalEntity.LegalEntityName : null,
                    HospitalSiteId = x.HospitalSiteId,
                    HospitalSiteName = x.HospitalSite != null ? x.HospitalSite.SiteName : null,
                    OrganizationUnitId = x.OrganizationUnitId,
                    OrganizationUnitName = x.OrganizationUnit != null ? x.OrganizationUnit.UnitName : null,
                    DepartmentId = x.DepartmentId,
                    DepartmentName = x.Department != null ? x.Department.DepartmentName : null,
                    PeriodStatus = x.PeriodStatus,
                    ScheduledCloseAt = x.ScheduledCloseAt,
                    LastValidatedAt = x.LastValidatedAt,
                    LastReconciledAt = x.LastReconciledAt,
                    ClosedAt = x.ClosedAt,
                    ReopenCount = x.ReopenCount,
                    CloseVersion = x.CloseVersion,
                    IsActive = x.IsActive
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<OvertimePeriodListResponse>
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)request.PageSize),
                Items = items
            };
        }

        public async Task<PagedResult<OvertimePeriodOptionResponse>> GetOptionsAsync(
            string? search,
            string? periodStatus,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var page = await GetPagedAsync(new OvertimePeriodQueryRequest
            {
                Search = search,
                PeriodStatus = periodStatus,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = "startDate",
                SortDirection = "desc"
            }, cancellationToken);

            return new PagedResult<OvertimePeriodOptionResponse>
            {
                PageNumber = page.PageNumber,
                PageSize = page.PageSize,
                TotalData = page.TotalData,
                TotalPage = page.TotalPage,
                Items = page.Items.Select(x => new OvertimePeriodOptionResponse
                {
                    Id = x.Id,
                    Code = x.PeriodCode,
                    Label = $"{x.PeriodCode} - {x.PeriodName} ({x.StartDate:yyyy-MM-dd} s.d. {x.EndDate:yyyy-MM-dd})",
                    PeriodStatus = x.PeriodStatus,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate
                }).ToList()
            };
        }

        public async Task<OvertimePeriodDetailResponse?> GetDetailAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new OvertimePeriodDetailResponse
                {
                    Id = x.Id,
                    PeriodCode = x.PeriodCode,
                    PeriodName = x.PeriodName,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    LegalEntityId = x.LegalEntityId,
                    LegalEntityName = x.LegalEntity != null ? x.LegalEntity.LegalEntityName : null,
                    HospitalSiteId = x.HospitalSiteId,
                    HospitalSiteName = x.HospitalSite != null ? x.HospitalSite.SiteName : null,
                    OrganizationUnitId = x.OrganizationUnitId,
                    OrganizationUnitName = x.OrganizationUnit != null ? x.OrganizationUnit.UnitName : null,
                    DepartmentId = x.DepartmentId,
                    DepartmentName = x.Department != null ? x.Department.DepartmentName : null,
                    PeriodStatus = x.PeriodStatus,
                    RequireAttendanceFinal = x.RequireAttendanceFinal,
                    RequireVerificationComplete = x.RequireVerificationComplete,
                    RequireSettlementComplete = x.RequireSettlementComplete,
                    ScheduledCloseAt = x.ScheduledCloseAt,
                    LastValidatedAt = x.LastValidatedAt,
                    LastReconciledAt = x.LastReconciledAt,
                    ValidationSnapshotJson = x.ValidationSnapshotJson,
                    ReconciliationSnapshotJson = x.ReconciliationSnapshotJson,
                    ClosedAt = x.ClosedAt,
                    ClosedByUserId = x.ClosedByUserId,
                    ClosedByUserName = x.ClosedByUser != null ? x.ClosedByUser.DisplayName ?? x.ClosedByUser.UserName ?? x.ClosedByUser.Email : null,
                    CloseReason = x.CloseReason,
                    ReopenedAt = x.ReopenedAt,
                    ReopenedByUserId = x.ReopenedByUserId,
                    ReopenedByUserName = x.ReopenedByUser != null ? x.ReopenedByUser.DisplayName ?? x.ReopenedByUser.UserName ?? x.ReopenedByUser.Email : null,
                    ReopenReason = x.ReopenReason,
                    ReopenCount = x.ReopenCount,
                    CloseVersion = x.CloseVersion,
                    IsActive = x.IsActive
                })
                .FirstOrDefaultAsync(cancellationToken);

        private IQueryable<TrxOvertimePeriod> BuildBaseQuery() =>
            _dbContext.Set<TrxOvertimePeriod>()
                .AsNoTracking()
                .Include(x => x.LegalEntity)
                .Include(x => x.HospitalSite)
                .Include(x => x.OrganizationUnit)
                .Include(x => x.Department)
                .Include(x => x.ClosedByUser)
                .Include(x => x.ReopenedByUser)
                .Where(x => !x.IsDelete);

        private static IQueryable<TrxOvertimePeriod> ApplyFilter(
            IQueryable<TrxOvertimePeriod> query,
            OvertimePeriodQueryRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.PeriodStatus)) query = query.Where(x => x.PeriodStatus == request.PeriodStatus.Trim());
            if (request.LegalEntityId.HasValue && request.LegalEntityId != Guid.Empty) query = query.Where(x => x.LegalEntityId == request.LegalEntityId);
            if (request.HospitalSiteId.HasValue && request.HospitalSiteId != Guid.Empty) query = query.Where(x => x.HospitalSiteId == request.HospitalSiteId);
            if (request.OrganizationUnitId.HasValue && request.OrganizationUnitId != Guid.Empty) query = query.Where(x => x.OrganizationUnitId == request.OrganizationUnitId);
            if (request.DepartmentId.HasValue && request.DepartmentId != Guid.Empty) query = query.Where(x => x.DepartmentId == request.DepartmentId);
            if (request.StartDate.HasValue) query = query.Where(x => x.EndDate >= request.StartDate.Value);
            if (request.EndDate.HasValue) query = query.Where(x => x.StartDate <= request.EndDate.Value);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim().ToLower();
                query = query.Where(x =>
                    x.PeriodCode.ToLower().Contains(keyword) ||
                    x.PeriodName.ToLower().Contains(keyword) ||
                    (x.HospitalSite != null && x.HospitalSite.SiteName.ToLower().Contains(keyword)) ||
                    (x.Department != null && x.Department.DepartmentName.ToLower().Contains(keyword)));
            }
            return query;
        }

        private static IOrderedQueryable<TrxOvertimePeriod> ApplySorting(
            IQueryable<TrxOvertimePeriod> query,
            string? sortBy,
            string? direction)
        {
            var desc = string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "startDate").Trim().ToLowerInvariant() switch
            {
                "enddate" => desc ? query.OrderByDescending(x => x.EndDate) : query.OrderBy(x => x.EndDate),
                "periodcode" => desc ? query.OrderByDescending(x => x.PeriodCode) : query.OrderBy(x => x.PeriodCode),
                "periodname" => desc ? query.OrderByDescending(x => x.PeriodName) : query.OrderBy(x => x.PeriodName),
                "periodstatus" => desc ? query.OrderByDescending(x => x.PeriodStatus) : query.OrderBy(x => x.PeriodStatus),
                "scheduledcloseat" => desc ? query.OrderByDescending(x => x.ScheduledCloseAt) : query.OrderBy(x => x.ScheduledCloseAt),
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => desc ? query.OrderByDescending(x => x.StartDate) : query.OrderBy(x => x.StartDate)
            };
        }

        private static void NormalizePaging(OvertimePeriodQueryRequest request)
        {
            request.PageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            request.PageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, 100);
        }
    }
}
