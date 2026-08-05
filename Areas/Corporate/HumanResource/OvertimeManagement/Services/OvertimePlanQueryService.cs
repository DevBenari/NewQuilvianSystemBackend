using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public class OvertimePlanQueryService
    {
        private readonly ApplicationDbContext _dbContext;

        public OvertimePlanQueryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public OvertimePlanFilterMetadataResponse GetMetadata() => new()
        {
            PlanStatusOptions = ToOptions(OvertimeValueConstants.PlanStatus.All),
            DetailStatusOptions = ToOptions(OvertimeValueConstants.PlanDetailStatus.All),
            DayTypeOptions = ToOptions(OvertimeValueConstants.DayType.All),
            OvertimeCategoryOptions = ToOptions(OvertimeValueConstants.OvertimeCategory.All),
            SortOptions = new List<OvertimePlanSortOptionResponse>
            {
                new() { Value = "planStartDate", Label = "Tanggal mulai rencana" },
                new() { Value = "planEndDate", Label = "Tanggal selesai rencana" },
                new() { Value = "planNumber", Label = "Nomor rencana" },
                new() { Value = "planTitle", Label = "Judul rencana" },
                new() { Value = "planStatus", Label = "Status rencana" },
                new() { Value = "totalDetail", Label = "Jumlah detail" },
                new() { Value = "totalPlannedMinutes", Label = "Total menit" },
                new() { Value = "createDateTime", Label = "Tanggal dibuat" }
            },
            SortDirections = new List<string> { "asc", "desc" },
            PageSizeOptions = new List<int> { 10, 25, 50, 100 }
        };

        public async Task<OvertimePlanSummaryResponse> GetSummaryAsync(
            OvertimePlanQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter(BuildBaseQuery(), request);
            var planIds = query.Select(x => x.Id);
            var details = _dbContext.TrxOvertimePlanDetails
                .AsNoTracking()
                .Where(x => planIds.Contains(x.OvertimePlanId) && !x.IsDelete && !x.IsCancel);

            return new OvertimePlanSummaryResponse
            {
                TotalPlan = await query.CountAsync(cancellationToken),
                DraftPlan = await query.CountAsync(x => x.PlanStatus == OvertimeValueConstants.PlanStatus.Draft, cancellationToken),
                ValidatedPlan = await query.CountAsync(x => x.PlanStatus == OvertimeValueConstants.PlanStatus.Validated, cancellationToken),
                PublishedPlan = await query.CountAsync(x => x.PlanStatus == OvertimeValueConstants.PlanStatus.Published, cancellationToken),
                PartiallyConvertedPlan = await query.CountAsync(x => x.PlanStatus == OvertimeValueConstants.PlanStatus.PartiallyConverted, cancellationToken),
                ConvertedPlan = await query.CountAsync(x => x.PlanStatus == OvertimeValueConstants.PlanStatus.Converted, cancellationToken),
                CancelledPlan = await query.CountAsync(x => x.PlanStatus == OvertimeValueConstants.PlanStatus.Cancelled, cancellationToken),
                ClosedPlan = await query.CountAsync(x => x.PlanStatus == OvertimeValueConstants.PlanStatus.Closed, cancellationToken),
                TotalDetail = await details.CountAsync(cancellationToken),
                ValidDetail = await details.CountAsync(x => x.IsPolicyCompliant && !x.HasScheduleConflict && !x.HasLeaveConflict && !x.HasTrainingConflict && !x.HasMinimumRestConflict && !x.HasWorkHourLimitConflict, cancellationToken),
                ConflictDetail = await details.CountAsync(x => !x.IsPolicyCompliant || x.HasScheduleConflict || x.HasLeaveConflict || x.HasTrainingConflict || x.HasMinimumRestConflict || x.HasWorkHourLimitConflict, cancellationToken),
                GeneratedRequest = await details.CountAsync(x => x.GeneratedOvertimeRequest != null && !x.GeneratedOvertimeRequest.IsDelete, cancellationToken),
                TotalPlannedMinutes = await details.SumAsync(x => (int?)x.PlannedMinutes, cancellationToken) ?? 0
            };
        }

        public async Task<PagedResult<OvertimePlanListResponse>> GetPagedAsync(
            OvertimePlanQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            NormalizePaging(request);
            var query = ApplyFilter(BuildBaseQuery(), request);
            var totalData = await query.CountAsync(cancellationToken);

            var items = await ApplySorting(query, request.SortBy, request.SortDirection)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new OvertimePlanListResponse
                {
                    Id = x.Id,
                    PlanNumber = x.PlanNumber,
                    PlanTitle = x.PlanTitle,
                    PlanStartDate = x.PlanStartDate,
                    PlanEndDate = x.PlanEndDate,
                    PlanStatus = x.PlanStatus,
                    LegalEntityId = x.LegalEntityId,
                    LegalEntityName = x.LegalEntity != null ? x.LegalEntity.LegalEntityName : null,
                    HospitalSiteId = x.HospitalSiteId,
                    HospitalSiteName = x.HospitalSite != null ? x.HospitalSite.SiteName : null,
                    OrganizationUnitId = x.OrganizationUnitId,
                    OrganizationUnitName = x.OrganizationUnit != null ? x.OrganizationUnit.UnitName : null,
                    DepartmentId = x.DepartmentId,
                    DepartmentName = x.Department != null ? x.Department.DepartmentName : null,
                    CostCenterId = x.CostCenterId,
                    CostCenterName = x.CostCenter != null ? x.CostCenter.CostCenterName : null,
                    WorkLocationId = x.WorkLocationId,
                    WorkLocationName = x.WorkLocation != null ? x.WorkLocation.LocationName : null,
                    RosterPeriodId = x.RosterPeriodId,
                    TotalDetail = x.Details.Count(d => !d.IsDelete && !d.IsCancel),
                    ConflictDetail = x.Details.Count(d => !d.IsDelete && !d.IsCancel && (!d.IsPolicyCompliant || d.HasScheduleConflict || d.HasLeaveConflict || d.HasTrainingConflict || d.HasMinimumRestConflict || d.HasWorkHourLimitConflict)),
                    GeneratedRequest = x.Details.Count(d => !d.IsDelete && !d.IsCancel && d.GeneratedOvertimeRequest != null && !d.GeneratedOvertimeRequest.IsDelete),
                    TotalPlannedMinutes = x.Details.Where(d => !d.IsDelete && !d.IsCancel).Sum(d => (int?)d.PlannedMinutes) ?? 0,
                    IsActive = x.IsActive,
                    ValidatedAt = x.ValidatedAt,
                    PublishedAt = x.PublishedAt,
                    CreateDateTime = x.CreateDateTime,
                    UpdateDateTime = x.UpdateDateTime
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<OvertimePlanListResponse>
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)request.PageSize),
                Items = items
            };
        }

        public async Task<PagedResult<OvertimePlanOptionResponse>> GetOptionsAsync(
            string? search,
            string? planStatus,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            pageNumber = Math.Max(pageNumber, 1);
            pageSize = Math.Clamp(pageSize, 1, 200);
            var query = BuildBaseQuery().Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(planStatus))
            {
                var normalized = NormalizeToken(planStatus, OvertimeValueConstants.PlanStatus.All);
                if (normalized != null) query = query.Where(x => x.PlanStatus == normalized);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x => x.PlanNumber.ToLower().Contains(keyword) || x.PlanTitle.ToLower().Contains(keyword));
            }

            var totalData = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(x => x.PlanStartDate)
                .ThenBy(x => x.PlanNumber)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new OvertimePlanOptionResponse
                {
                    Id = x.Id,
                    PlanNumber = x.PlanNumber,
                    PlanTitle = x.PlanTitle,
                    PlanStartDate = x.PlanStartDate,
                    PlanEndDate = x.PlanEndDate,
                    PlanStatus = x.PlanStatus,
                    TotalDetail = x.Details.Count(d => !d.IsDelete && !d.IsCancel)
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<OvertimePlanOptionResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task<OvertimePlanResponse?> GetDetailAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.TrxOvertimePlans
                .AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.LegalEntity)
                .Include(x => x.HospitalSite)
                .Include(x => x.OrganizationUnit)
                .Include(x => x.Department)
                .Include(x => x.CostCenter)
                .Include(x => x.WorkLocation)
                .Include(x => x.ValidatedByUser)
                .Include(x => x.PublishedByUser)
                .Include(x => x.ClosedByUser)
                .Include(x => x.Details.Where(d => !d.IsDelete))
                    .ThenInclude(d => d.WorkforceProfile)
                .Include(x => x.Details.Where(d => !d.IsDelete))
                    .ThenInclude(d => d.Employee)
                .Include(x => x.Details.Where(d => !d.IsDelete))
                    .ThenInclude(d => d.HospitalSite)
                .Include(x => x.Details.Where(d => !d.IsDelete))
                    .ThenInclude(d => d.OrganizationUnit)
                .Include(x => x.Details.Where(d => !d.IsDelete))
                    .ThenInclude(d => d.Department)
                .Include(x => x.Details.Where(d => !d.IsDelete))
                    .ThenInclude(d => d.Position)
                .Include(x => x.Details.Where(d => !d.IsDelete))
                    .ThenInclude(d => d.CostCenter)
                .Include(x => x.Details.Where(d => !d.IsDelete))
                    .ThenInclude(d => d.WorkLocation)
                .Include(x => x.Details.Where(d => !d.IsDelete))
                    .ThenInclude(d => d.WorkSchedule)
                .Include(x => x.Details.Where(d => !d.IsDelete))
                    .ThenInclude(d => d.Shift)
                .Include(x => x.Details.Where(d => !d.IsDelete))
                    .ThenInclude(d => d.OvertimePolicy)
                .Include(x => x.Details.Where(d => !d.IsDelete))
                    .ThenInclude(d => d.GeneratedOvertimeRequest)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null) return null;
            var activeDetails = entity.Details.Where(d => !d.IsDelete && !d.IsCancel).ToList();
            return new OvertimePlanResponse
            {
                Id = entity.Id,
                PlanNumber = entity.PlanNumber,
                PlanTitle = entity.PlanTitle,
                PlanStartDate = entity.PlanStartDate,
                PlanEndDate = entity.PlanEndDate,
                PlanStatus = entity.PlanStatus,
                LegalEntityId = entity.LegalEntityId,
                LegalEntityName = entity.LegalEntity?.LegalEntityName,
                HospitalSiteId = entity.HospitalSiteId,
                HospitalSiteName = entity.HospitalSite?.SiteName,
                OrganizationUnitId = entity.OrganizationUnitId,
                OrganizationUnitName = entity.OrganizationUnit?.UnitName,
                DepartmentId = entity.DepartmentId,
                DepartmentName = entity.Department?.DepartmentName,
                CostCenterId = entity.CostCenterId,
                CostCenterName = entity.CostCenter?.CostCenterName,
                WorkLocationId = entity.WorkLocationId,
                WorkLocationName = entity.WorkLocation?.LocationName,
                RosterPeriodId = entity.RosterPeriodId,
                TotalDetail = activeDetails.Count,
                ConflictDetail = activeDetails.Count(d => !d.IsPolicyCompliant || d.HasScheduleConflict || d.HasLeaveConflict || d.HasTrainingConflict || d.HasMinimumRestConflict || d.HasWorkHourLimitConflict),
                GeneratedRequest = activeDetails.Count(d => d.GeneratedOvertimeRequest != null && !d.GeneratedOvertimeRequest.IsDelete),
                TotalPlannedMinutes = activeDetails.Sum(d => d.PlannedMinutes),
                IsActive = entity.IsActive,
                ValidatedAt = entity.ValidatedAt,
                PublishedAt = entity.PublishedAt,
                ClosedAt = entity.ClosedAt,
                CreateDateTime = entity.CreateDateTime,
                UpdateDateTime = entity.UpdateDateTime,
                Reason = entity.Reason,
                Notes = entity.Notes,
                ValidatedByUserId = entity.ValidatedByUserId,
                ValidatedByUserName = GetUserName(entity.ValidatedByUser),
                PublishedByUserId = entity.PublishedByUserId,
                PublishedByUserName = GetUserName(entity.PublishedByUser),
                ClosedByUserId = entity.ClosedByUserId,
                ClosedByUserName = GetUserName(entity.ClosedByUser),
                Details = entity.Details.Where(d => !d.IsDelete).OrderBy(d => d.SequenceNumber).Select(MapDetail).ToList()
            };
        }

        public async Task<OvertimePlanDetailResponse?> GetPlanDetailAsync(
            Guid planId,
            Guid detailId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.TrxOvertimePlanDetails
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.Employee)
                .Include(x => x.HospitalSite)
                .Include(x => x.OrganizationUnit)
                .Include(x => x.Department)
                .Include(x => x.Position)
                .Include(x => x.CostCenter)
                .Include(x => x.WorkLocation)
                .Include(x => x.WorkSchedule)
                .Include(x => x.Shift)
                .Include(x => x.OvertimePolicy)
                .Include(x => x.GeneratedOvertimeRequest)
                .FirstOrDefaultAsync(x => x.Id == detailId && x.OvertimePlanId == planId && !x.IsDelete, cancellationToken);
            return entity == null ? null : MapDetail(entity);
        }

        private IQueryable<TrxOvertimePlan> BuildBaseQuery() =>
            _dbContext.TrxOvertimePlans.AsNoTracking().Where(x => !x.IsDelete);

        private static IQueryable<TrxOvertimePlan> ApplyFilter(
            IQueryable<TrxOvertimePlan> query,
            OvertimePlanQueryRequest request)
        {
            if (request.StartDate.HasValue) query = query.Where(x => x.PlanEndDate >= request.StartDate.Value);
            if (request.EndDate.HasValue) query = query.Where(x => x.PlanStartDate <= request.EndDate.Value);
            if (request.LegalEntityId.HasValue && request.LegalEntityId != Guid.Empty) query = query.Where(x => x.LegalEntityId == request.LegalEntityId);
            if (request.HospitalSiteId.HasValue && request.HospitalSiteId != Guid.Empty) query = query.Where(x => x.HospitalSiteId == request.HospitalSiteId);
            if (request.OrganizationUnitId.HasValue && request.OrganizationUnitId != Guid.Empty) query = query.Where(x => x.OrganizationUnitId == request.OrganizationUnitId);
            if (request.DepartmentId.HasValue && request.DepartmentId != Guid.Empty) query = query.Where(x => x.DepartmentId == request.DepartmentId);
            if (request.CostCenterId.HasValue && request.CostCenterId != Guid.Empty) query = query.Where(x => x.CostCenterId == request.CostCenterId);
            if (request.WorkLocationId.HasValue && request.WorkLocationId != Guid.Empty) query = query.Where(x => x.WorkLocationId == request.WorkLocationId);
            if (request.RosterPeriodId.HasValue && request.RosterPeriodId != Guid.Empty) query = query.Where(x => x.RosterPeriodId == request.RosterPeriodId);
            if (request.WorkforceProfileId.HasValue && request.WorkforceProfileId != Guid.Empty) query = query.Where(x => x.Details.Any(d => !d.IsDelete && d.WorkforceProfileId == request.WorkforceProfileId));
            if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);

            var status = NormalizeToken(request.PlanStatus, OvertimeValueConstants.PlanStatus.All);
            if (status != null) query = query.Where(x => x.PlanStatus == status);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim().ToLower();
                query = query.Where(x =>
                    x.PlanNumber.ToLower().Contains(keyword) ||
                    x.PlanTitle.ToLower().Contains(keyword) ||
                    x.Reason.ToLower().Contains(keyword) ||
                    (x.Department != null && x.Department.DepartmentName.ToLower().Contains(keyword)) ||
                    x.Details.Any(d => !d.IsDelete && ((d.WorkforceProfile != null && d.WorkforceProfile.DisplayName.ToLower().Contains(keyword)) || (d.Employee != null && (d.Employee.EmployeeCode.ToLower().Contains(keyword) || d.Employee.FullName.ToLower().Contains(keyword))))));
            }

            return query;
        }

        private static IOrderedQueryable<TrxOvertimePlan> ApplySorting(
            IQueryable<TrxOvertimePlan> query,
            string? sortBy,
            string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "planStartDate").Trim().ToLowerInvariant() switch
            {
                "planenddate" => desc ? query.OrderByDescending(x => x.PlanEndDate).ThenBy(x => x.PlanNumber) : query.OrderBy(x => x.PlanEndDate).ThenBy(x => x.PlanNumber),
                "plannumber" => desc ? query.OrderByDescending(x => x.PlanNumber) : query.OrderBy(x => x.PlanNumber),
                "plantitle" => desc ? query.OrderByDescending(x => x.PlanTitle) : query.OrderBy(x => x.PlanTitle),
                "planstatus" => desc ? query.OrderByDescending(x => x.PlanStatus).ThenByDescending(x => x.PlanStartDate) : query.OrderBy(x => x.PlanStatus).ThenBy(x => x.PlanStartDate),
                "totaldetail" => desc ? query.OrderByDescending(x => x.Details.Count(d => !d.IsDelete && !d.IsCancel)) : query.OrderBy(x => x.Details.Count(d => !d.IsDelete && !d.IsCancel)),
                "totalplannedminutes" => desc ? query.OrderByDescending(x => x.Details.Where(d => !d.IsDelete && !d.IsCancel).Sum(d => (int?)d.PlannedMinutes) ?? 0) : query.OrderBy(x => x.Details.Where(d => !d.IsDelete && !d.IsCancel).Sum(d => (int?)d.PlannedMinutes) ?? 0),
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => desc ? query.OrderByDescending(x => x.PlanStartDate).ThenBy(x => x.PlanNumber) : query.OrderBy(x => x.PlanStartDate).ThenBy(x => x.PlanNumber)
            };
        }

        private static OvertimePlanDetailResponse MapDetail(TrxOvertimePlanDetail d) => new()
        {
            Id = d.Id,
            OvertimePlanId = d.OvertimePlanId,
            SequenceNumber = d.SequenceNumber,
            WorkforceProfileId = d.WorkforceProfileId,
            WorkforceProfileCode = d.WorkforceProfile != null ? d.WorkforceProfile.ProfileCode : null,
            WorkforceDisplayName = d.WorkforceProfile != null ? d.WorkforceProfile.DisplayName : null,
            EmployeeId = d.EmployeeId,
            EmployeeCode = d.Employee != null ? d.Employee.EmployeeCode : null,
            EmployeeName = d.Employee != null ? d.Employee.FullName : null,
            OrganizationAssignmentId = d.OrganizationAssignmentId,
            HospitalSiteId = d.HospitalSiteId,
            HospitalSiteName = d.HospitalSite != null ? d.HospitalSite.SiteName : null,
            OrganizationUnitId = d.OrganizationUnitId,
            OrganizationUnitName = d.OrganizationUnit != null ? d.OrganizationUnit.UnitName : null,
            DepartmentId = d.DepartmentId,
            DepartmentName = d.Department != null ? d.Department.DepartmentName : null,
            PositionId = d.PositionId,
            PositionName = d.Position != null ? d.Position.PositionName : null,
            CostCenterId = d.CostCenterId,
            CostCenterName = d.CostCenter != null ? d.CostCenter.CostCenterName : null,
            WorkLocationId = d.WorkLocationId,
            WorkLocationName = d.WorkLocation != null ? d.WorkLocation.LocationName : null,
            WorkScheduleAssignmentId = d.WorkScheduleAssignmentId,
            RosterPeriodId = d.RosterPeriodId,
            ShiftAssignmentId = d.ShiftAssignmentId,
            WorkScheduleId = d.WorkScheduleId,
            WorkScheduleCode = d.WorkSchedule != null ? d.WorkSchedule.ScheduleCode : null,
            WorkScheduleName = d.WorkSchedule != null ? d.WorkSchedule.ScheduleName : null,
            ShiftId = d.ShiftId,
            ShiftCode = d.Shift != null ? d.Shift.ShiftCode : null,
            ShiftName = d.Shift != null ? d.Shift.ShiftName : null,
            OvertimePolicyId = d.OvertimePolicyId,
            OvertimePolicyCode = d.OvertimePolicy != null ? d.OvertimePolicy.OvertimePolicyCode : null,
            OvertimePolicyName = d.OvertimePolicy != null ? d.OvertimePolicy.OvertimePolicyName : null,
            OvertimeDate = d.OvertimeDate,
            PlannedEndDate = d.PlannedEndDate,
            PlannedStartAt = d.PlannedStartAt,
            PlannedEndAt = d.PlannedEndAt,
            PlannedMinutes = d.PlannedMinutes,
            EstimatedBreakMinutes = d.EstimatedBreakMinutes,
            DayType = d.DayType,
            OvertimeCategory = d.OvertimeCategory,
            WorkDescription = d.WorkDescription,
            Notes = d.Notes,
            HasScheduleConflict = d.HasScheduleConflict,
            HasLeaveConflict = d.HasLeaveConflict,
            HasTrainingConflict = d.HasTrainingConflict,
            HasMinimumRestConflict = d.HasMinimumRestConflict,
            HasWorkHourLimitConflict = d.HasWorkHourLimitConflict,
            IsPolicyCompliant = d.IsPolicyCompliant,
            ValidationResultJson = d.ValidationResultJson,
            DetailStatus = d.DetailStatus,
            GeneratedOvertimeRequestId = d.GeneratedOvertimeRequest != null ? d.GeneratedOvertimeRequest.Id : null,
            GeneratedOvertimeRequestNumber = d.GeneratedOvertimeRequest != null ? d.GeneratedOvertimeRequest.RequestNumber : null,
            GeneratedOvertimeRequestStatus = d.GeneratedOvertimeRequest != null ? d.GeneratedOvertimeRequest.OvertimeRequestStatus : null,
            IsActive = d.IsActive,
            CreateDateTime = d.CreateDateTime,
            UpdateDateTime = d.UpdateDateTime
        };

        private static List<OvertimePlanStringOptionResponse> ToOptions(IEnumerable<string> values) =>
            values.Select(x => new OvertimePlanStringOptionResponse { Value = x, Label = SplitLabel(x) }).ToList();

        private static string SplitLabel(string value) =>
            string.Concat(value.Select((c, i) => i > 0 && char.IsUpper(c) ? " " + c : c.ToString()));

        private static string? GetUserName(QuilvianSystemBackend.Models.ApplicationUser? user) =>
            user == null ? null : (user.DisplayName ?? user.UserName ?? user.Email ?? user.UserCode);

        private static string? NormalizeToken(string? value, IReadOnlyCollection<string> allowed)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            return allowed.FirstOrDefault(x => x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static void NormalizePaging(OvertimePlanQueryRequest request)
        {
            request.PageNumber = Math.Max(request.PageNumber, 1);
            request.PageSize = Math.Clamp(request.PageSize, 1, 200);
        }
    }
}
