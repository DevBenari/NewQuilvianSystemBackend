using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeSelfService.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeSelfService.Services
{
    public class OvertimeSelfServiceQueryService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly OvertimeSelfServiceContextService _contextService;

        public OvertimeSelfServiceQueryService(
            ApplicationDbContext dbContext,
            OvertimeSelfServiceContextService contextService)
        {
            _dbContext = dbContext;
            _contextService = contextService;
        }

        public MyOvertimeMetadataResponse GetMetadata() => new()
        {
            StatusOptions = ToOptions(OvertimeValueConstants.RequestStatus.All),
            RequestSourceOptions = ToOptions(OvertimeValueConstants.RequestSource.All),
            OvertimeCategoryOptions = ToOptions(OvertimeValueConstants.OvertimeCategory.All),
            DayTypeOptions = ToOptions(OvertimeValueConstants.DayType.All),
            SortOptions = new List<MyOvertimeStringOptionResponse>
            {
                new() { Value = "overtimeDate", Label = "Tanggal lembur" },
                new() { Value = "requestNumber", Label = "Nomor pengajuan" },
                new() { Value = "status", Label = "Status" },
                new() { Value = "requestedMinutes", Label = "Menit diajukan" },
                new() { Value = "approvedMinutes", Label = "Menit disetujui" },
                new() { Value = "createDateTime", Label = "Tanggal dibuat" }
            },
            SortDirections = new List<string> { "asc", "desc" },
            PageSizeOptions = new List<int> { 10, 25, 50, 100 }
        };

        public async Task<OvertimeSelfServiceServiceResult<MyOvertimeSummaryResponse>> GetSummaryAsync(
            Guid actorUserId,
            MyOvertimeQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var contextResult = await _contextService.ResolveAsync(actorUserId, cancellationToken);
            if (!contextResult.Success || contextResult.Data == null)
                return OvertimeSelfServiceServiceResult<MyOvertimeSummaryResponse>.Fail(contextResult.StatusCode, contextResult.Message);

            var query = BuildBaseQuery(contextResult.Data.WorkforceProfileId);
            query = ApplyFilter(query, request);

            var data = await query
                .GroupBy(_ => 1)
                .Select(g => new MyOvertimeSummaryResponse
                {
                    TotalRequest = g.Count(),
                    Draft = g.Count(x => x.OvertimeRequestStatus == OvertimeValueConstants.RequestStatus.Draft),
                    Submitted = g.Count(x => x.OvertimeRequestStatus == OvertimeValueConstants.RequestStatus.Submitted),
                    NeedRevision = g.Count(x => x.OvertimeRequestStatus == OvertimeValueConstants.RequestStatus.NeedRevision),
                    ApprovedForWork = g.Count(x => x.OvertimeRequestStatus == OvertimeValueConstants.RequestStatus.ApprovedForWork),
                    Rejected = g.Count(x => x.OvertimeRequestStatus == OvertimeValueConstants.RequestStatus.Rejected),
                    WaitingRealization = g.Count(x => x.OvertimeRequestStatus == OvertimeValueConstants.RequestStatus.WaitingRealization),
                    WaitingVerification = g.Count(x => x.OvertimeRequestStatus == OvertimeValueConstants.RequestStatus.WaitingVerification),
                    Realized = g.Count(x => x.OvertimeRequestStatus == OvertimeValueConstants.RequestStatus.Realized),
                    PostedToPayroll = g.Count(x => x.OvertimeRequestStatus == OvertimeValueConstants.RequestStatus.PostedToPayroll),
                    Cancelled = g.Count(x => x.OvertimeRequestStatus == OvertimeValueConstants.RequestStatus.Cancelled),
                    TotalRequestedMinutes = g.Sum(x => x.RequestedMinutes),
                    TotalApprovedMinutes = g.Sum(x => x.ApprovedMinutes),
                    ManagerPlannedRequest = g.Count(x => x.RequestSource == OvertimeValueConstants.RequestSource.ManagerPlanning),
                    EmployeeSelfServiceRequest = g.Count(x => x.RequestSource == OvertimeValueConstants.RequestSource.EmployeeSelfService)
                })
                .FirstOrDefaultAsync(cancellationToken) ?? new MyOvertimeSummaryResponse();

            return OvertimeSelfServiceServiceResult<MyOvertimeSummaryResponse>.Ok(
                data,
                "Ringkasan pengajuan lembur saya berhasil diambil.");
        }

        public async Task<OvertimeSelfServiceServiceResult<PagedResult<MyOvertimeListResponse>>> GetPagedAsync(
            Guid actorUserId,
            MyOvertimeQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var contextResult = await _contextService.ResolveAsync(actorUserId, cancellationToken);
            if (!contextResult.Success || contextResult.Data == null)
                return OvertimeSelfServiceServiceResult<PagedResult<MyOvertimeListResponse>>.Fail(contextResult.StatusCode, contextResult.Message);

            NormalizePaging(request);
            var query = ApplyFilter(BuildBaseQuery(contextResult.Data.WorkforceProfileId), request);
            var totalData = await query.CountAsync(cancellationToken);
            var ordered = ApplySort(query, request.SortBy, request.SortDirection);

            var items = await ordered
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new MyOvertimeListResponse
                {
                    Id = x.Id,
                    RequestNumber = x.RequestNumber,
                    RequestSource = x.RequestSource,
                    OvertimeDate = x.OvertimeDate,
                    PlannedEndDate = x.PlannedEndDate,
                    PlannedStartAt = x.PlannedStartAt,
                    PlannedEndAt = x.PlannedEndAt,
                    RequestedMinutes = x.RequestedMinutes,
                    ApprovedMinutes = x.ApprovedMinutes,
                    Reason = x.Reason,
                    WorkDescription = x.WorkDescription,
                    Status = x.OvertimeRequestStatus,
                    OvertimePolicyId = x.OvertimePolicyId,
                    OvertimePolicyCode = x.OvertimePolicy != null ? x.OvertimePolicy.OvertimePolicyCode : null,
                    OvertimePolicyName = x.OvertimePolicy != null ? x.OvertimePolicy.OvertimePolicyName : null,
                    SourceOvertimePlanDetailId = x.SourceOvertimePlanDetailId,
                    WorkflowInstanceId = x.WorkflowInstanceId,
                    IsPolicyCompliant = x.IsPolicyCompliant,
                    HasConflict = x.HasScheduleConflict || x.HasLeaveConflict || x.HasTrainingConflict || x.HasMinimumRestConflict || x.HasWorkHourLimitConflict,
                    SubmittedAt = x.SubmittedAt,
                    ApprovedAt = x.ApprovedAt,
                    RejectedAt = x.RejectedAt,
                    CancelledAt = x.CancelledAt,
                    CreateDateTime = x.CreateDateTime,
                    UpdateDateTime = x.UpdateDateTime
                })
                .ToListAsync(cancellationToken);

            foreach (var item in items)
                ApplyPermissions(item);

            return OvertimeSelfServiceServiceResult<PagedResult<MyOvertimeListResponse>>.Ok(
                new PagedResult<MyOvertimeListResponse>
                {
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)request.PageSize),
                    Items = items
                },
                "Daftar pengajuan lembur saya berhasil diambil.");
        }

        public async Task<OvertimeSelfServiceServiceResult<MyOvertimeDetailResponse>> GetDetailAsync(
            Guid actorUserId,
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var contextResult = await _contextService.ResolveAsync(actorUserId, cancellationToken);
            if (!contextResult.Success || contextResult.Data == null)
                return OvertimeSelfServiceServiceResult<MyOvertimeDetailResponse>.Fail(contextResult.StatusCode, contextResult.Message);

            var entity = await _dbContext.WfpOvertimeRequests
                .AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.Employee)
                .Include(x => x.HospitalSite)
                .Include(x => x.OrganizationUnit)
                .Include(x => x.Department)
                .Include(x => x.Position)
                .Include(x => x.CostCenter)
                .Include(x => x.OvertimePolicy)
                .Include(x => x.WorkSchedule)
                .Include(x => x.Shift)
                .Include(x => x.RequestReason)
                .Include(x => x.Details.Where(d => !d.IsDelete))
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == contextResult.Data.WorkforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return OvertimeSelfServiceServiceResult<MyOvertimeDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan lembur tidak ditemukan atau bukan milik user login.");
            }

            var data = MapDetail(entity);
            ApplyPermissions(data);
            return OvertimeSelfServiceServiceResult<MyOvertimeDetailResponse>.Ok(
                data,
                "Detail pengajuan lembur saya berhasil diambil.");
        }

        private IQueryable<WfpOvertimeRequest> BuildBaseQuery(Guid workforceProfileId) =>
            _dbContext.WfpOvertimeRequests
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

        private static IQueryable<WfpOvertimeRequest> ApplyFilter(
            IQueryable<WfpOvertimeRequest> query,
            MyOvertimeQueryRequest request)
        {
            if (request.StartDate.HasValue)
                query = query.Where(x => x.OvertimeDate >= request.StartDate.Value);

            if (request.EndDate.HasValue)
                query = query.Where(x => x.OvertimeDate <= request.EndDate.Value);

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                var status = NormalizeToken(request.Status, OvertimeValueConstants.RequestStatus.All);
                if (status != null) query = query.Where(x => x.OvertimeRequestStatus == status);
            }

            if (!string.IsNullOrWhiteSpace(request.RequestSource))
            {
                var source = NormalizeToken(request.RequestSource, OvertimeValueConstants.RequestSource.All);
                if (source != null) query = query.Where(x => x.RequestSource == source);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim().ToLower();
                query = query.Where(x =>
                    x.RequestNumber.ToLower().Contains(keyword) ||
                    x.Reason.ToLower().Contains(keyword) ||
                    (x.WorkDescription != null && x.WorkDescription.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<WfpOvertimeRequest> ApplySort(
            IQueryable<WfpOvertimeRequest> query,
            string? sortBy,
            string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "overtimeDate").Trim().ToLowerInvariant() switch
            {
                "requestnumber" => desc ? query.OrderByDescending(x => x.RequestNumber) : query.OrderBy(x => x.RequestNumber),
                "status" => desc ? query.OrderByDescending(x => x.OvertimeRequestStatus).ThenByDescending(x => x.OvertimeDate) : query.OrderBy(x => x.OvertimeRequestStatus).ThenBy(x => x.OvertimeDate),
                "requestedminutes" => desc ? query.OrderByDescending(x => x.RequestedMinutes).ThenByDescending(x => x.OvertimeDate) : query.OrderBy(x => x.RequestedMinutes).ThenBy(x => x.OvertimeDate),
                "approvedminutes" => desc ? query.OrderByDescending(x => x.ApprovedMinutes).ThenByDescending(x => x.OvertimeDate) : query.OrderBy(x => x.ApprovedMinutes).ThenBy(x => x.OvertimeDate),
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => desc ? query.OrderByDescending(x => x.OvertimeDate).ThenByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.OvertimeDate).ThenBy(x => x.CreateDateTime)
            };
        }

        private static MyOvertimeDetailResponse MapDetail(WfpOvertimeRequest x)
        {
            var data = new MyOvertimeDetailResponse
            {
                Id = x.Id,
                RequestNumber = x.RequestNumber,
                RequestSource = x.RequestSource,
                OvertimeDate = x.OvertimeDate,
                PlannedEndDate = x.PlannedEndDate,
                PlannedStartAt = x.PlannedStartAt,
                PlannedEndAt = x.PlannedEndAt,
                RequestedMinutes = x.RequestedMinutes,
                ApprovedMinutes = x.ApprovedMinutes,
                Reason = x.Reason,
                WorkDescription = x.WorkDescription,
                Status = x.OvertimeRequestStatus,
                OvertimePolicyId = x.OvertimePolicyId,
                OvertimePolicyCode = x.OvertimePolicy?.OvertimePolicyCode,
                OvertimePolicyName = x.OvertimePolicy?.OvertimePolicyName,
                SourceOvertimePlanDetailId = x.SourceOvertimePlanDetailId,
                WorkflowInstanceId = x.WorkflowInstanceId,
                IsPolicyCompliant = x.IsPolicyCompliant,
                HasConflict = x.HasScheduleConflict || x.HasLeaveConflict || x.HasTrainingConflict || x.HasMinimumRestConflict || x.HasWorkHourLimitConflict,
                SubmittedAt = x.SubmittedAt,
                ApprovedAt = x.ApprovedAt,
                RejectedAt = x.RejectedAt,
                CancelledAt = x.CancelledAt,
                CreateDateTime = x.CreateDateTime,
                UpdateDateTime = x.UpdateDateTime,
                WorkforceProfileId = x.WorkforceProfileId,
                WorkforceProfileCode = x.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName = x.WorkforceProfile?.DisplayName ?? string.Empty,
                EmployeeId = x.EmployeeId,
                EmployeeCode = x.Employee?.EmployeeCode,
                EmployeeName = x.Employee?.FullName,
                OrganizationAssignmentId = x.OrganizationAssignmentId,
                HospitalSiteId = x.HospitalSiteId,
                HospitalSiteName = x.HospitalSite?.SiteName,
                OrganizationUnitId = x.OrganizationUnitId,
                OrganizationUnitName = x.OrganizationUnit?.UnitName,
                DepartmentId = x.DepartmentId,
                DepartmentName = x.Department?.DepartmentName,
                PositionId = x.PositionId,
                PositionName = x.Position?.PositionName,
                CostCenterId = x.CostCenterId,
                CostCenterName = x.CostCenter?.CostCenterName,
                WorkScheduleAssignmentId = x.WorkScheduleAssignmentId,
                RosterPeriodId = x.RosterPeriodId,
                ShiftAssignmentId = x.ShiftAssignmentId,
                WorkScheduleId = x.WorkScheduleId,
                WorkScheduleCode = x.WorkSchedule?.ScheduleCode,
                WorkScheduleName = x.WorkSchedule?.ScheduleName,
                ShiftId = x.ShiftId,
                ShiftCode = x.Shift?.ShiftCode,
                ShiftName = x.Shift?.ShiftName,
                RequestReasonId = x.RequestReasonId,
                RequestReasonName = x.RequestReason?.ReasonName,
                EstimatedBreakMinutes = x.EstimatedBreakMinutes,
                IsUrgent = x.IsUrgent,
                IsBeforeShift = x.IsBeforeShift,
                IsAfterShift = x.IsAfterShift,
                IsRestDay = x.IsRestDay,
                IsHoliday = x.IsHoliday,
                HasScheduleConflict = x.HasScheduleConflict,
                HasLeaveConflict = x.HasLeaveConflict,
                HasTrainingConflict = x.HasTrainingConflict,
                HasMinimumRestConflict = x.HasMinimumRestConflict,
                HasWorkHourLimitConflict = x.HasWorkHourLimitConflict,
                ValidationResultJson = x.ValidationResultJson,
                WorkflowDefinitionId = x.WorkflowDefinitionId,
                CurrentApprovalStep = x.CurrentApprovalStep,
                ApprovalNotes = x.ApprovalNotes,
                Details = x.Details
                    .Where(d => !d.IsDelete)
                    .OrderBy(d => d.SequenceNumber)
                    .Select(d => new MyOvertimeDetailItemResponse
                    {
                        Id = d.Id,
                        SequenceNumber = d.SequenceNumber,
                        OvertimeDate = d.OvertimeDate,
                        PlannedStartAt = d.PlannedStartAt,
                        PlannedEndAt = d.PlannedEndAt,
                        ApprovedStartAt = d.ApprovedStartAt,
                        ApprovedEndAt = d.ApprovedEndAt,
                        RequestedMinutes = d.RequestedMinutes,
                        ApprovedMinutes = d.ApprovedMinutes,
                        BreakMinutes = d.BreakMinutes,
                        DayType = d.DayType,
                        OvertimeCategory = d.OvertimeCategory,
                        OvertimeRateId = d.OvertimeRateId,
                        RateCodeSnapshot = d.RateCodeSnapshot,
                        RateMultiplierSnapshot = d.RateMultiplierSnapshot,
                        WorkDescription = d.WorkDescription,
                        Notes = d.Notes,
                        DetailStatus = d.DetailStatus
                    })
                    .ToList()
            };

            return data;
        }

        private static void ApplyPermissions(MyOvertimeListResponse item)
        {
            var isEmployeeDraft = item.RequestSource == OvertimeValueConstants.RequestSource.EmployeeSelfService &&
                                  (item.Status == OvertimeValueConstants.RequestStatus.Draft ||
                                   item.Status == OvertimeValueConstants.RequestStatus.NeedRevision);

            var isNeedRevision = item.Status == OvertimeValueConstants.RequestStatus.NeedRevision;
            var isDraftWithoutWorkflow = item.Status == OvertimeValueConstants.RequestStatus.Draft &&
                                         !item.WorkflowInstanceId.HasValue;

            item.CanEdit = item.RequestSource == OvertimeValueConstants.RequestSource.EmployeeSelfService &&
                           (isDraftWithoutWorkflow || isNeedRevision);
            item.CanSubmit = item.CanEdit && item.IsPolicyCompliant && !item.HasConflict;
            item.CanDelete = item.RequestSource == OvertimeValueConstants.RequestSource.EmployeeSelfService &&
                             isDraftWithoutWorkflow;
            item.CanCancel = item.RequestSource == OvertimeValueConstants.RequestSource.EmployeeSelfService &&
                             (item.Status == OvertimeValueConstants.RequestStatus.Draft ||
                              item.Status == OvertimeValueConstants.RequestStatus.Submitted ||
                              item.Status == OvertimeValueConstants.RequestStatus.NeedRevision);
            item.IsReadOnly = !item.CanEdit;
        }

        private static List<MyOvertimeStringOptionResponse> ToOptions(IEnumerable<string> values) =>
            values.Select(x => new MyOvertimeStringOptionResponse
            {
                Value = x,
                Label = SplitLabel(x)
            }).ToList();

        private static string SplitLabel(string value) =>
            string.Concat(value.Select((c, i) => i > 0 && char.IsUpper(c) ? " " + c : c.ToString()));

        private static string? NormalizeToken(string? value, IReadOnlyCollection<string> allowed)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            return allowed.FirstOrDefault(x => x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static void NormalizePaging(MyOvertimeQueryRequest request)
        {
            request.PageNumber = Math.Max(request.PageNumber, 1);
            request.PageSize = Math.Clamp(request.PageSize, 1, 200);
        }
    }
}
