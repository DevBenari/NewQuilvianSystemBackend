using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public class OvertimeRealizationQueryService
    {
        private readonly ApplicationDbContext _dbContext;

        public OvertimeRealizationQueryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public OvertimeRealizationFilterMetadataResponse GetMetadata() => new()
        {
            RealizationStatuses = OvertimeValueConstants.RealizationStatus.All,
            RequestStatuses = OvertimeValueConstants.RequestStatus.All,
            DayTypes = OvertimeValueConstants.DayType.All,
            OvertimeCategories = OvertimeValueConstants.OvertimeCategory.All,
            SortFields = new[]
            {
                "realizationNumber",
                "requestNumber",
                "workforceDisplayName",
                "actualStartDate",
                "eligibleMinutes",
                "realizationStatus",
                "createDateTime"
            }
        };

        public async Task<OvertimeRealizationSummaryResponse> GetSummaryAsync(
            OvertimeRealizationQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter(BuildBaseQuery(), request);

            return new OvertimeRealizationSummaryResponse
            {
                TotalRealization = await query.CountAsync(cancellationToken),
                Draft = await query.CountAsync(
                    x => x.RealizationStatus == OvertimeValueConstants.RealizationStatus.Draft,
                    cancellationToken),
                WaitingVerification = await query.CountAsync(
                    x => x.RealizationStatus == OvertimeValueConstants.RealizationStatus.WaitingVerification,
                    cancellationToken),
                NeedRevision = await query.CountAsync(
                    x => x.RealizationStatus == OvertimeValueConstants.RealizationStatus.NeedRevision,
                    cancellationToken),
                Verified = await query.CountAsync(
                    x => x.RealizationStatus == OvertimeValueConstants.RealizationStatus.Verified,
                    cancellationToken),
                Rejected = await query.CountAsync(
                    x => x.RealizationStatus == OvertimeValueConstants.RealizationStatus.Rejected,
                    cancellationToken),
                PostedToPayroll = await query.CountAsync(
                    x => x.RealizationStatus == OvertimeValueConstants.RealizationStatus.PostedToPayroll,
                    cancellationToken),
                Cancelled = await query.CountAsync(
                    x => x.RealizationStatus == OvertimeValueConstants.RealizationStatus.Cancelled,
                    cancellationToken),
                TotalActualMinutes = await query.SumAsync(x => (int?)x.ActualMinutes, cancellationToken) ?? 0,
                TotalBreakMinutes = await query.SumAsync(x => (int?)x.ActualBreakMinutes, cancellationToken) ?? 0,
                TotalEligibleMinutes = await query.SumAsync(x => (int?)x.EligibleMinutes, cancellationToken) ?? 0,
                TotalVerifiedMinutes = await query.SumAsync(x => (int?)x.VerifiedMinutes, cancellationToken) ?? 0
            };
        }

        public async Task<PagedResult<OvertimeRealizationListResponse>> GetPagedAsync(
            OvertimeRealizationQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            NormalizePaging(request);
            var query = ApplyFilter(BuildBaseQuery(), request);
            var totalData = await query.CountAsync(cancellationToken);

            var items = await ApplySorting(query, request.SortBy, request.SortDirection)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new OvertimeRealizationListResponse
                {
                    Id = x.Id,
                    RealizationNumber = x.RealizationNumber,
                    RealizationVersion = x.RealizationVersion,
                    RealizationStatus = x.RealizationStatus,
                    OvertimeRequestId = x.OvertimeRequestId,
                    RequestNumber = x.OvertimeRequest != null ? x.OvertimeRequest.RequestNumber : string.Empty,
                    RequestStatus = x.OvertimeRequest != null ? x.OvertimeRequest.OvertimeRequestStatus : string.Empty,
                    WorkforceProfileId = x.WorkforceProfileId,
                    WorkforceProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : string.Empty,
                    WorkforceDisplayName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty,
                    ActualStartDate = x.ActualStartDate,
                    ActualEndDate = x.ActualEndDate,
                    ActualStartAt = x.ActualStartAt,
                    ActualEndAt = x.ActualEndAt,
                    RequestedMinutesSnapshot = x.RequestedMinutesSnapshot,
                    ApprovedMinutesSnapshot = x.ApprovedMinutesSnapshot,
                    ActualMinutes = x.ActualMinutes,
                    ActualBreakMinutes = x.ActualBreakMinutes,
                    EligibleMinutes = x.EligibleMinutes,
                    VerifiedMinutes = x.VerifiedMinutes,
                    VarianceMinutes = x.VarianceMinutes,
                    IsPayrollPosted = x.IsPayrollPosted,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    UpdateDateTime = x.UpdateDateTime
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<OvertimeRealizationListResponse>
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)request.PageSize),
                Items = items
            };
        }

        public async Task<PagedResult<OvertimeRealizationOptionResponse>> GetOptionsAsync(
            string? search,
            string? realizationStatus,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var query = BuildBaseQuery()
                .Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(realizationStatus))
            {
                var normalizedStatus = NormalizeToken(
                    realizationStatus,
                    OvertimeValueConstants.RealizationStatus.All);

                if (normalizedStatus != null)
                {
                    query = query.Where(x => x.RealizationStatus == normalizedStatus);
                }
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.RealizationNumber.ToLower().Contains(keyword) ||
                    (x.OvertimeRequest != null && x.OvertimeRequest.RequestNumber.ToLower().Contains(keyword)) ||
                    (x.WorkforceProfile != null && x.WorkforceProfile.DisplayName.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(x => x.ActualStartDate)
                .ThenByDescending(x => x.RealizationVersion)
                .ThenBy(x => x.RealizationNumber)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new OvertimeRealizationOptionResponse
                {
                    Id = x.Id,
                    RealizationNumber = x.RealizationNumber,
                    RequestNumber = x.OvertimeRequest != null ? x.OvertimeRequest.RequestNumber : string.Empty,
                    WorkforceDisplayName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty,
                    RealizationStatus = x.RealizationStatus,
                    ActualStartDate = x.ActualStartDate,
                    EligibleMinutes = x.EligibleMinutes
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<OvertimeRealizationOptionResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task<OvertimeRealizationDetailResponse?> GetDetailAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await BuildBaseQuery()
                .Include(x => x.Details.Where(d => !d.IsDelete))
                    .ThenInclude(x => x.OvertimeRate)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity == null) return null;

            return new OvertimeRealizationDetailResponse
            {
                Header = MapHeader(entity),
                EmployeeId = entity.EmployeeId,
                OrganizationAssignmentId = entity.OrganizationAssignmentId,
                HospitalSiteId = entity.HospitalSiteId,
                OrganizationUnitId = entity.OrganizationUnitId,
                DepartmentId = entity.DepartmentId,
                PositionId = entity.PositionId,
                CostCenterId = entity.CostCenterId,
                AttendanceDailyId = entity.AttendanceDailyId,
                CurrencyCode = entity.CurrencyCode,
                RealizationNotes = entity.RealizationNotes,
                EvidenceSummaryJson = entity.EvidenceSummaryJson,
                CalculationResultJson = entity.CalculationResultJson,
                SubmittedAt = entity.SubmittedAt,
                VerifiedAt = entity.VerifiedAt,
                PostedToPayrollAt = entity.PostedToPayrollAt,
                CancelledAt = entity.CancelledAt,
                Details = entity.Details
                    .OrderBy(x => x.SequenceNumber)
                    .Select(x => new OvertimeRealizationDetailItemResponse
                    {
                        Id = x.Id,
                        OvertimeRequestDetailId = x.OvertimeRequestDetailId,
                        SequenceNumber = x.SequenceNumber,
                        OvertimeDate = x.OvertimeDate,
                        AttendanceDailyId = x.AttendanceDailyId,
                        AttendanceId = x.AttendanceId,
                        ShiftAssignmentId = x.ShiftAssignmentId,
                        AttendanceCheckInAt = x.AttendanceCheckInAt,
                        AttendanceCheckOutAt = x.AttendanceCheckOutAt,
                        ActualStartAt = x.ActualStartAt,
                        ActualEndAt = x.ActualEndAt,
                        ActualMinutes = x.ActualMinutes,
                        BreakMinutes = x.BreakMinutes,
                        EligibleMinutes = x.EligibleMinutes,
                        VerifiedMinutes = x.VerifiedMinutes,
                        VarianceFromApprovedMinutes = x.VarianceFromApprovedMinutes,
                        DayType = x.DayType,
                        OvertimeRateId = x.OvertimeRateId,
                        OvertimeRateCode = x.OvertimeRate != null ? x.OvertimeRate.OvertimeRateCode : null,
                        RateBandSnapshot = x.RateBandSnapshot,
                        CalculationMethodSnapshot = x.CalculationMethodSnapshot,
                        RateMultiplierSnapshot = x.RateMultiplierSnapshot,
                        FixedAmountSnapshot = x.FixedAmountSnapshot,
                        DetailStatus = x.DetailStatus,
                        Notes = x.Notes
                    })
                    .ToList()
            };
        }

        private IQueryable<TrxOvertimeRealization> BuildBaseQuery() =>
            _dbContext.TrxOvertimeRealizations
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .Include(x => x.OvertimeRequest)
                .Include(x => x.WorkforceProfile);

        private static IQueryable<TrxOvertimeRealization> ApplyFilter(
            IQueryable<TrxOvertimeRealization> query,
            OvertimeRealizationQueryRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim().ToLower();
                query = query.Where(x =>
                    x.RealizationNumber.ToLower().Contains(keyword) ||
                    (x.OvertimeRequest != null && x.OvertimeRequest.RequestNumber.ToLower().Contains(keyword)) ||
                    (x.WorkforceProfile != null &&
                     (x.WorkforceProfile.ProfileCode.ToLower().Contains(keyword) ||
                      x.WorkforceProfile.DisplayName.ToLower().Contains(keyword))));
            }

            var realizationStatus = NormalizeToken(
                request.RealizationStatus,
                OvertimeValueConstants.RealizationStatus.All);
            if (realizationStatus != null)
            {
                query = query.Where(x => x.RealizationStatus == realizationStatus);
            }

            var requestStatus = NormalizeToken(
                request.RequestStatus,
                OvertimeValueConstants.RequestStatus.All);
            if (requestStatus != null)
            {
                query = query.Where(x =>
                    x.OvertimeRequest != null &&
                    x.OvertimeRequest.OvertimeRequestStatus == requestStatus);
            }

            if (request.WorkforceProfileId.HasValue && request.WorkforceProfileId != Guid.Empty)
            {
                query = query.Where(x => x.WorkforceProfileId == request.WorkforceProfileId);
            }

            if (request.HospitalSiteId.HasValue && request.HospitalSiteId != Guid.Empty)
            {
                query = query.Where(x => x.HospitalSiteId == request.HospitalSiteId);
            }

            if (request.OrganizationUnitId.HasValue && request.OrganizationUnitId != Guid.Empty)
            {
                query = query.Where(x => x.OrganizationUnitId == request.OrganizationUnitId);
            }

            if (request.DepartmentId.HasValue && request.DepartmentId != Guid.Empty)
            {
                query = query.Where(x => x.DepartmentId == request.DepartmentId);
            }

            if (request.StartDate.HasValue)
            {
                query = query.Where(x => x.ActualStartDate >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                query = query.Where(x => x.ActualStartDate <= request.EndDate.Value);
            }

            if (request.IsPayrollPosted.HasValue)
            {
                query = query.Where(x => x.IsPayrollPosted == request.IsPayrollPosted.Value);
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive.Value);
            }
            else
            {
                query = query.Where(x => x.IsActive);
            }

            return query;
        }

        private static IOrderedQueryable<TrxOvertimeRealization> ApplySorting(
            IQueryable<TrxOvertimeRealization> query,
            string? sortBy,
            string? sortDirection)
        {
            var descending = !string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);
            var field = string.IsNullOrWhiteSpace(sortBy)
                ? "createDateTime"
                : sortBy.Trim();

            return field.ToLowerInvariant() switch
            {
                "realizationnumber" => descending
                    ? query.OrderByDescending(x => x.RealizationNumber)
                    : query.OrderBy(x => x.RealizationNumber),
                "requestnumber" => descending
                    ? query.OrderByDescending(x => x.OvertimeRequest!.RequestNumber)
                    : query.OrderBy(x => x.OvertimeRequest!.RequestNumber),
                "workforcedisplayname" => descending
                    ? query.OrderByDescending(x => x.WorkforceProfile!.DisplayName)
                    : query.OrderBy(x => x.WorkforceProfile!.DisplayName),
                "actualstartdate" => descending
                    ? query.OrderByDescending(x => x.ActualStartDate).ThenByDescending(x => x.RealizationVersion)
                    : query.OrderBy(x => x.ActualStartDate).ThenBy(x => x.RealizationVersion),
                "eligibleminutes" => descending
                    ? query.OrderByDescending(x => x.EligibleMinutes)
                    : query.OrderBy(x => x.EligibleMinutes),
                "realizationstatus" => descending
                    ? query.OrderByDescending(x => x.RealizationStatus)
                    : query.OrderBy(x => x.RealizationStatus),
                _ => descending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime)
            };
        }

        private static OvertimeRealizationListResponse MapHeader(
            TrxOvertimeRealization x) => new()
        {
            Id = x.Id,
            RealizationNumber = x.RealizationNumber,
            RealizationVersion = x.RealizationVersion,
            RealizationStatus = x.RealizationStatus,
            OvertimeRequestId = x.OvertimeRequestId,
            RequestNumber = x.OvertimeRequest?.RequestNumber ?? string.Empty,
            RequestStatus = x.OvertimeRequest?.OvertimeRequestStatus ?? string.Empty,
            WorkforceProfileId = x.WorkforceProfileId,
            WorkforceProfileCode = x.WorkforceProfile?.ProfileCode ?? string.Empty,
            WorkforceDisplayName = x.WorkforceProfile?.DisplayName ?? string.Empty,
            ActualStartDate = x.ActualStartDate,
            ActualEndDate = x.ActualEndDate,
            ActualStartAt = x.ActualStartAt,
            ActualEndAt = x.ActualEndAt,
            RequestedMinutesSnapshot = x.RequestedMinutesSnapshot,
            ApprovedMinutesSnapshot = x.ApprovedMinutesSnapshot,
            ActualMinutes = x.ActualMinutes,
            ActualBreakMinutes = x.ActualBreakMinutes,
            EligibleMinutes = x.EligibleMinutes,
            VerifiedMinutes = x.VerifiedMinutes,
            VarianceMinutes = x.VarianceMinutes,
            IsPayrollPosted = x.IsPayrollPosted,
            IsActive = x.IsActive,
            CreateDateTime = x.CreateDateTime,
            UpdateDateTime = x.UpdateDateTime
        };

        private static string? NormalizeToken(
            string? value,
            IReadOnlyCollection<string> allowed) =>
            string.IsNullOrWhiteSpace(value)
                ? null
                : allowed.FirstOrDefault(x =>
                    string.Equals(x, value.Trim(), StringComparison.OrdinalIgnoreCase));

        private static void NormalizePaging(OvertimeRealizationQueryRequest request)
        {
            request.PageNumber = Math.Max(1, request.PageNumber);
            request.PageSize = Math.Clamp(request.PageSize, 1, 200);
        }
    }
}
