using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public class OvertimeVerificationQueryService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly OvertimeRealizationQueryService _realizationQueryService;

        public OvertimeVerificationQueryService(
            ApplicationDbContext dbContext,
            OvertimeRealizationQueryService realizationQueryService)
        {
            _dbContext = dbContext;
            _realizationQueryService = realizationQueryService;
        }

        public OvertimeVerificationFilterMetadataResponse GetMetadata() => new()
        {
            VerificationStatuses = OvertimeValueConstants.VerificationStatus.FilterAll,
            VerificationTypes = OvertimeValueConstants.VerificationType.All,
            RealizationStatuses = OvertimeValueConstants.RealizationStatus.All,
            SortFields = new[]
            {
                "realizationNumber",
                "requestNumber",
                "workforceDisplayName",
                "actualStartDate",
                "eligibleMinutes",
                "verifiedMinutes",
                "verificationStatus",
                "createDateTime"
            }
        };

        public async Task<OvertimeVerificationSummaryResponse> GetSummaryAsync(
            OvertimeVerificationQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter(BuildBaseQuery(), request);

            var realizationIds = await query
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            var verifications = await _dbContext.TrxOvertimeVerifications
                .AsNoTracking()
                .Where(x =>
                    realizationIds.Contains(x.OvertimeRealizationId) &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive)
                .ToListAsync(cancellationToken);

            var latestByRealization = verifications
                .GroupBy(x => x.OvertimeRealizationId)
                .ToDictionary(
                    x => x.Key,
                    x => x
                        .OrderByDescending(v => v.VerificationOrder)
                        .ThenByDescending(v => v.CreateDateTime)
                        .First());

            var realizations = await query
                .Select(x => new
                {
                    x.Id,
                    x.EligibleMinutes,
                    x.VerifiedMinutes,
                    x.VarianceMinutes,
                    x.RealizationStatus
                })
                .ToListAsync(cancellationToken);

            var result = new OvertimeVerificationSummaryResponse
            {
                TotalQueue = realizations.Count,
                TotalEligibleMinutes = realizations.Sum(x => x.EligibleMinutes)
            };

            foreach (var realization in realizations)
            {
                if (!latestByRealization.TryGetValue(realization.Id, out var verification))
                {
                    result.NotStarted++;
                }
                else
                {
                    switch (verification.VerificationStatus)
                    {
                        case OvertimeValueConstants.VerificationStatus.Pending:
                            result.Pending++;
                            break;
                        case OvertimeValueConstants.VerificationStatus.Approved:
                            result.Approved++;
                            break;
                        case OvertimeValueConstants.VerificationStatus.NeedRevision:
                            result.NeedRevision++;
                            break;
                        case OvertimeValueConstants.VerificationStatus.Rejected:
                            result.Rejected++;
                            break;
                    }

                    if (verification.IsFinalVerification)
                    {
                        result.Finalized++;
                    }

                    if (verification.HasVariance)
                    {
                        result.WithVariance++;
                    }

                    if (string.Equals(
                            verification.VerificationStatus,
                            OvertimeValueConstants.VerificationStatus.Approved,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        result.TotalVerifiedMinutes += verification.VerifiedMinutes;
                        result.TotalAdjustmentMinutes +=
                            verification.VerifiedMinutes - verification.EligibleMinutes;
                    }
                }
            }

            return result;
        }

        public async Task<PagedResult<OvertimeVerificationListResponse>> GetPagedAsync(
            OvertimeVerificationQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            NormalizePaging(request);
            var query = ApplyFilter(BuildBaseQuery(), request);
            var totalData = await query.CountAsync(cancellationToken);

            var ordered = ApplySorting(query, request.SortBy, request.SortDirection);
            var entities = await ordered
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var verificationMap = await LoadLatestVerificationMapAsync(
                entities.Select(x => x.Id),
                cancellationToken);

            return new PagedResult<OvertimeVerificationListResponse>
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)request.PageSize),
                Items = entities
                    .Select(x => MapList(x, verificationMap.GetValueOrDefault(x.Id)))
                    .ToList()
            };
        }

        public async Task<PagedResult<OvertimeVerificationOptionResponse>> GetOptionsAsync(
            string? search,
            string? verificationStatus,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 20 : Math.Min(pageSize, 200);

            var request = new OvertimeVerificationQueryRequest
            {
                Search = search,
                VerificationStatus = verificationStatus,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = "createDateTime",
                SortDirection = "desc"
            };

            var page = await GetPagedAsync(request, cancellationToken);

            return new PagedResult<OvertimeVerificationOptionResponse>
            {
                PageNumber = page.PageNumber,
                PageSize = page.PageSize,
                TotalData = page.TotalData,
                TotalPage = page.TotalPage,
                Items = page.Items.Select(x => new OvertimeVerificationOptionResponse
                {
                    OvertimeRealizationId = x.OvertimeRealizationId,
                    OvertimeVerificationId = x.OvertimeVerificationId,
                    RealizationNumber = x.RealizationNumber,
                    RequestNumber = x.RequestNumber,
                    WorkforceDisplayName = x.WorkforceDisplayName,
                    VerificationStatus = x.VerificationStatus,
                    EligibleMinutes = x.EligibleMinutes
                }).ToList()
            };
        }

        public async Task<OvertimeVerificationDetailResponse?> GetDetailAsync(
            Guid overtimeRealizationId,
            CancellationToken cancellationToken = default)
        {
            var realization = await _realizationQueryService.GetDetailAsync(
                overtimeRealizationId,
                cancellationToken);

            if (realization == null)
            {
                return null;
            }

            var verificationEntities = await _dbContext.TrxOvertimeVerifications
                .AsNoTracking()
                .Where(x =>
                    x.OvertimeRealizationId == overtimeRealizationId &&
                    !x.IsDelete)
                .Include(x => x.VerifierUser)
                .Include(x => x.VerifierWorkforceProfile)
                .Include(x => x.RejectionReason)
                .OrderBy(x => x.VerificationOrder)
                .ThenBy(x => x.CreateDateTime)
                .ToListAsync(cancellationToken);

            var records = verificationEntities
                .Select(MapRecord)
                .ToList();

            var current = verificationEntities
                .Where(x => x.IsActive && !x.IsCancel)
                .OrderByDescending(x => x.VerificationOrder)
                .ThenByDescending(x => x.CreateDateTime)
                .FirstOrDefault();

            var waiting = string.Equals(
                realization.Header.RealizationStatus,
                OvertimeValueConstants.RealizationStatus.WaitingVerification,
                StringComparison.OrdinalIgnoreCase);

            var pending = current == null || string.Equals(
                current.VerificationStatus,
                OvertimeValueConstants.VerificationStatus.Pending,
                StringComparison.OrdinalIgnoreCase);

            var locked = IsLockedStatus(realization.Header.RealizationStatus);

            return new OvertimeVerificationDetailResponse
            {
                Realization = realization,
                CurrentVerification = current == null ? null : MapRecord(current),
                VerificationHistory = records,
                CanStart = waiting && current == null,
                CanApprove = waiting && pending,
                CanRequestRevision = waiting && pending,
                CanReject = waiting && pending,
                IsLocked = locked
            };
        }

        private IQueryable<TrxOvertimeRealization> BuildBaseQuery() =>
            _dbContext.TrxOvertimeRealizations
                .AsNoTracking()
                .Where(x => !x.IsDelete && !x.IsCancel && x.IsActive)
                .Include(x => x.OvertimeRequest)
                .Include(x => x.WorkforceProfile);

        private static IQueryable<TrxOvertimeRealization> ApplyFilter(
            IQueryable<TrxOvertimeRealization> query,
            OvertimeVerificationQueryRequest request)
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
            else
            {
                query = query.Where(x =>
                    x.RealizationStatus == OvertimeValueConstants.RealizationStatus.WaitingVerification ||
                    x.RealizationStatus == OvertimeValueConstants.RealizationStatus.NeedRevision ||
                    x.Verifications.Any(v =>
                        !v.IsDelete &&
                        !v.IsCancel &&
                        v.IsActive));
            }

            var verificationStatus = NormalizeToken(
                request.VerificationStatus,
                OvertimeValueConstants.VerificationStatus.FilterAll);

            if (verificationStatus == OvertimeValueConstants.VerificationStatus.NotStarted)
            {
                query = query.Where(x => !x.Verifications.Any(v =>
                    !v.IsDelete && !v.IsCancel && v.IsActive));
            }
            else if (verificationStatus != null)
            {
                query = query.Where(x => x.Verifications.Any(v =>
                    !v.IsDelete &&
                    !v.IsCancel &&
                    v.IsActive &&
                    v.VerificationStatus == verificationStatus));
            }

            var verificationType = NormalizeToken(
                request.VerificationType,
                OvertimeValueConstants.VerificationType.All);

            if (verificationType != null)
            {
                query = query.Where(x => x.Verifications.Any(v =>
                    !v.IsDelete &&
                    !v.IsCancel &&
                    v.IsActive &&
                    v.VerificationType == verificationType));
            }

            if (request.WorkforceProfileId.HasValue && request.WorkforceProfileId != Guid.Empty)
            {
                query = query.Where(x => x.WorkforceProfileId == request.WorkforceProfileId);
            }

            if (request.VerifierUserId.HasValue && request.VerifierUserId != Guid.Empty)
            {
                query = query.Where(x => x.Verifications.Any(v =>
                    !v.IsDelete &&
                    !v.IsCancel &&
                    v.IsActive &&
                    v.VerifierUserId == request.VerifierUserId));
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

            if (request.HasVariance.HasValue)
            {
                query = request.HasVariance.Value
                    ? query.Where(x =>
                        x.VarianceMinutes != 0 ||
                        x.Verifications.Any(v =>
                            !v.IsDelete && !v.IsCancel && v.IsActive && v.HasVariance))
                    : query.Where(x =>
                        x.VarianceMinutes == 0 &&
                        !x.Verifications.Any(v =>
                            !v.IsDelete && !v.IsCancel && v.IsActive && v.HasVariance));
            }

            if (request.IsFinalVerification.HasValue)
            {
                query = query.Where(x => x.Verifications.Any(v =>
                    !v.IsDelete &&
                    !v.IsCancel &&
                    v.IsActive &&
                    v.IsFinalVerification == request.IsFinalVerification.Value));
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
                    ? query.OrderByDescending(x => x.ActualStartDate)
                    : query.OrderBy(x => x.ActualStartDate),
                "eligibleminutes" => descending
                    ? query.OrderByDescending(x => x.EligibleMinutes)
                    : query.OrderBy(x => x.EligibleMinutes),
                "verifiedminutes" => descending
                    ? query.OrderByDescending(x => x.VerifiedMinutes)
                    : query.OrderBy(x => x.VerifiedMinutes),
                "verificationstatus" => descending
                    ? query.OrderByDescending(x => x.Verifications
                        .Where(v => !v.IsDelete && !v.IsCancel && v.IsActive)
                        .OrderByDescending(v => v.VerificationOrder)
                        .Select(v => v.VerificationStatus)
                        .FirstOrDefault())
                    : query.OrderBy(x => x.Verifications
                        .Where(v => !v.IsDelete && !v.IsCancel && v.IsActive)
                        .OrderByDescending(v => v.VerificationOrder)
                        .Select(v => v.VerificationStatus)
                        .FirstOrDefault()),
                _ => descending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime)
            };
        }

        private async Task<Dictionary<Guid, TrxOvertimeVerification>> LoadLatestVerificationMapAsync(
            IEnumerable<Guid> realizationIds,
            CancellationToken cancellationToken)
        {
            var ids = realizationIds.Distinct().ToList();
            if (ids.Count == 0)
            {
                return new Dictionary<Guid, TrxOvertimeVerification>();
            }

            var entities = await _dbContext.TrxOvertimeVerifications
                .AsNoTracking()
                .Where(x =>
                    ids.Contains(x.OvertimeRealizationId) &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive)
                .Include(x => x.VerifierUser)
                .Include(x => x.VerifierWorkforceProfile)
                .ToListAsync(cancellationToken);

            return entities
                .GroupBy(x => x.OvertimeRealizationId)
                .ToDictionary(
                    x => x.Key,
                    x => x
                        .OrderByDescending(v => v.VerificationOrder)
                        .ThenByDescending(v => v.CreateDateTime)
                        .First());
        }

        private static OvertimeVerificationListResponse MapList(
            TrxOvertimeRealization entity,
            TrxOvertimeVerification? verification) => new()
        {
            OvertimeRealizationId = entity.Id,
            OvertimeVerificationId = verification?.Id,
            RealizationNumber = entity.RealizationNumber,
            RealizationVersion = entity.RealizationVersion,
            RealizationStatus = entity.RealizationStatus,
            OvertimeRequestId = entity.OvertimeRequestId,
            RequestNumber = entity.OvertimeRequest?.RequestNumber ?? string.Empty,
            RequestStatus = entity.OvertimeRequest?.OvertimeRequestStatus ?? string.Empty,
            WorkforceProfileId = entity.WorkforceProfileId,
            WorkforceProfileCode = entity.WorkforceProfile?.ProfileCode ?? string.Empty,
            WorkforceDisplayName = entity.WorkforceProfile?.DisplayName ?? string.Empty,
            ActualStartDate = entity.ActualStartDate,
            ActualEndDate = entity.ActualEndDate,
            ActualMinutes = entity.ActualMinutes,
            BreakMinutes = entity.ActualBreakMinutes,
            EligibleMinutes = entity.EligibleMinutes,
            VerifiedMinutes = entity.VerifiedMinutes,
            VarianceMinutes = entity.VarianceMinutes,
            VerificationType = verification?.VerificationType ?? OvertimeValueConstants.VerificationType.HR,
            VerificationStatus = verification?.VerificationStatus ?? OvertimeValueConstants.VerificationStatus.NotStarted,
            VerifierUserId = verification?.VerifierUserId,
            VerifierWorkforceProfileId = verification?.VerifierWorkforceProfileId,
            VerifierDisplayName = verification?.VerifierWorkforceProfile?.DisplayName ?? verification?.VerifierUser?.UserName,
            HasVariance = verification?.HasVariance ?? entity.VarianceMinutes != 0,
            RequiresRevision = verification?.RequiresRevision ?? false,
            IsFinalVerification = verification?.IsFinalVerification ?? false,
            ActionAt = verification?.ActionAt,
            CreateDateTime = entity.CreateDateTime
        };

        private static OvertimeVerificationRecordResponse MapRecord(
            TrxOvertimeVerification entity) => new()
        {
            Id = entity.Id,
            VerificationOrder = entity.VerificationOrder,
            VerificationType = entity.VerificationType,
            VerificationStatus = entity.VerificationStatus,
            WorkflowStepId = entity.WorkflowStepId,
            VerifierUserId = entity.VerifierUserId,
            VerifierWorkforceProfileId = entity.VerifierWorkforceProfileId,
            VerifierDisplayName = entity.VerifierWorkforceProfile?.DisplayName ?? entity.VerifierUser?.UserName,
            RejectionReasonId = entity.RejectionReasonId,
            RejectionReasonCode = entity.RejectionReason?.ReasonCode,
            RejectionReasonName = entity.RejectionReason?.ReasonName,
            SubmittedMinutes = entity.SubmittedMinutes,
            EligibleMinutes = entity.EligibleMinutes,
            VerifiedMinutes = entity.VerifiedMinutes,
            IsAttendanceMatched = entity.IsAttendanceMatched,
            IsPolicyCompliant = entity.IsPolicyCompliant,
            HasVariance = entity.HasVariance,
            RequiresRevision = entity.RequiresRevision,
            IsFinalVerification = entity.IsFinalVerification,
            VerificationResultJson = entity.VerificationResultJson,
            Comments = entity.Comments,
            ActionAt = entity.ActionAt,
            CreateDateTime = entity.CreateDateTime
        };

        private static string? NormalizeToken(
            string? value,
            IEnumerable<string> validValues)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var match = validValues.FirstOrDefault(x =>
                string.Equals(x, value.Trim(), StringComparison.OrdinalIgnoreCase));

            return match;
        }

        private static bool IsLockedStatus(string status) =>
            string.Equals(status, OvertimeValueConstants.RealizationStatus.Verified, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, OvertimeValueConstants.RealizationStatus.PostedToPayroll, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, OvertimeValueConstants.RealizationStatus.Rejected, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, OvertimeValueConstants.RealizationStatus.Cancelled, StringComparison.OrdinalIgnoreCase);

        private static void NormalizePaging(OvertimeVerificationQueryRequest request)
        {
            request.PageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            request.PageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 200);
        }
    }
}
