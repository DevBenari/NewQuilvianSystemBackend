using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public class OvertimeCompensatoryLeaveQueryService
    {
        private readonly ApplicationDbContext _dbContext;

        public OvertimeCompensatoryLeaveQueryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public OvertimeCompensatoryLeaveFilterMetadataResponse GetMetadata() => new()
        {
            CompensatoryStatuses = OvertimeValueConstants.CompensatoryStatus.All,
            SortFields = new[]
            {
                "creditNumber",
                "workforceDisplayName",
                "earnedDate",
                "effectiveStartDate",
                "expiryDate",
                "earnedMinutes",
                "remainingMinutes",
                "compensatoryStatus",
                "createDateTime"
            },
            PageSizeOptions = new[] { 10, 20, 25, 50, 100, 200 }
        };

        public async Task<OvertimeCompensatoryLeaveSummaryResponse> GetSummaryAsync(
            OvertimeCompensatoryLeaveQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter(BuildBaseQuery(), request);
            var soon = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(30));
            return new OvertimeCompensatoryLeaveSummaryResponse
            {
                TotalCredit = await query.CountAsync(cancellationToken),
                Pending = await query.CountAsync(x => x.CompensatoryStatus == OvertimeValueConstants.CompensatoryStatus.Pending, cancellationToken),
                Available = await query.CountAsync(x => x.CompensatoryStatus == OvertimeValueConstants.CompensatoryStatus.Available, cancellationToken),
                PartiallyUsed = await query.CountAsync(x => x.CompensatoryStatus == OvertimeValueConstants.CompensatoryStatus.PartiallyUsed, cancellationToken),
                Used = await query.CountAsync(x => x.CompensatoryStatus == OvertimeValueConstants.CompensatoryStatus.Used, cancellationToken),
                Expired = await query.CountAsync(x => x.CompensatoryStatus == OvertimeValueConstants.CompensatoryStatus.Expired, cancellationToken),
                Cancelled = await query.CountAsync(x => x.CompensatoryStatus == OvertimeValueConstants.CompensatoryStatus.Cancelled, cancellationToken),
                WithoutLedger = await query.CountAsync(x => !x.LeaveBalanceTransactionId.HasValue, cancellationToken),
                ExpiringSoon = await query.CountAsync(x => x.ExpiryDate.HasValue && x.ExpiryDate.Value <= soon && x.RemainingMinutes > 0, cancellationToken),
                TotalSourceMinutes = await query.SumAsync(x => (int?)x.SourceOvertimeMinutes, cancellationToken) ?? 0,
                TotalEarnedMinutes = await query.SumAsync(x => (int?)x.EarnedMinutes, cancellationToken) ?? 0,
                TotalRemainingMinutes = await query.SumAsync(x => (int?)x.RemainingMinutes, cancellationToken) ?? 0
            };
        }

        public async Task<PagedResult<OvertimeCompensatoryLeaveListResponse>> GetPagedAsync(
            OvertimeCompensatoryLeaveQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            NormalizePaging(request);
            var query = ApplySort(ApplyFilter(BuildBaseQuery(), request), request.SortBy, request.SortDirection);
            var totalData = await query.CountAsync(cancellationToken);
            var rows = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<OvertimeCompensatoryLeaveListResponse>
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)request.PageSize),
                Items = rows.Select(MapList).ToList()
            };
        }

        public async Task<PagedResult<OvertimeCompensatoryLeaveOptionResponse>> GetOptionsAsync(
            string? search,
            string? compensatoryStatus,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var request = new OvertimeCompensatoryLeaveQueryRequest
            {
                Search = search,
                CompensatoryStatus = compensatoryStatus,
                IsActive = true,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = "creditNumber",
                SortDirection = "asc"
            };
            var page = await GetPagedAsync(request, cancellationToken);
            return new PagedResult<OvertimeCompensatoryLeaveOptionResponse>
            {
                PageNumber = page.PageNumber,
                PageSize = page.PageSize,
                TotalData = page.TotalData,
                TotalPage = page.TotalPage,
                Items = page.Items.Select(x => new OvertimeCompensatoryLeaveOptionResponse
                {
                    Id = x.Id,
                    CreditNumber = x.CreditNumber,
                    WorkforceDisplayName = x.WorkforceDisplayName,
                    LeaveTypeName = x.LeaveTypeName,
                    CompensatoryStatus = x.CompensatoryStatus,
                    RemainingMinutes = x.RemainingMinutes,
                    ExpiryDate = x.ExpiryDate
                }).ToList()
            };
        }

        public async Task<OvertimeCompensatoryLeaveDetailResponse?> GetDetailAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity == null) return null;

            var response = MapDetail(entity);
            if (entity.LeaveBalanceTransactionId.HasValue)
            {
                var ledger = await _dbContext.TrxLeaveBalanceTransactions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == entity.LeaveBalanceTransactionId.Value && !x.IsDelete, cancellationToken);
                if (ledger != null)
                {
                    response.LedgerTransactionNumber = ledger.TransactionNumber;
                    response.LedgerTransactionStatus = ledger.TransactionStatus;
                    response.LedgerTransactionDays = ledger.TransactionDays;
                    response.LedgerAvailableDelta = ledger.AvailableDelta;
                    response.LeaveBalanceId = ledger.LeaveBalanceId;
                    var balance = await _dbContext.WfpLeaveBalances
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == ledger.LeaveBalanceId && !x.IsDelete, cancellationToken);
                    if (balance != null)
                    {
                        response.BalanceCompensatoryDays = balance.CompensatoryDays;
                        response.BalanceRemainingDays = balance.RemainingDays;
                        response.BalanceAvailableDays = balance.AvailableDays;
                    }
                    response.IsLedgerConsistent =
                        ledger.SourceReferenceId == entity.Id &&
                        string.Equals(ledger.SourceType, OvertimeValueConstants.CompensatoryLedger.SourceTypeCredit, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(ledger.TransactionStatus, OvertimeValueConstants.CompensatoryLedger.Posted, StringComparison.OrdinalIgnoreCase);
                }
            }
            return response;
        }

        private IQueryable<TrxCompensatoryTimeOff> BuildBaseQuery() =>
            _dbContext.TrxCompensatoryTimeOffs
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.OvertimeRequest)
                .Include(x => x.OvertimeRealization)
                .Include(x => x.OvertimeVerification)
                .Include(x => x.LeaveType)
                .Include(x => x.LeaveBalanceTransaction)
                .Include(x => x.ApprovedByUser)
                .Where(x => !x.IsDelete && !x.IsCancel);

        private static IQueryable<TrxCompensatoryTimeOff> ApplyFilter(
            IQueryable<TrxCompensatoryTimeOff> query,
            OvertimeCompensatoryLeaveQueryRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim().ToLower();
                query = query.Where(x =>
                    x.CreditNumber.ToLower().Contains(keyword) ||
                    (x.WorkforceProfile != null && x.WorkforceProfile.ProfileCode.ToLower().Contains(keyword)) ||
                    (x.WorkforceProfile != null && x.WorkforceProfile.DisplayName.ToLower().Contains(keyword)) ||
                    (x.OvertimeRequest != null && x.OvertimeRequest.RequestNumber.ToLower().Contains(keyword)) ||
                    (x.OvertimeRealization != null && x.OvertimeRealization.RealizationNumber.ToLower().Contains(keyword)) ||
                    (x.LeaveType != null && x.LeaveType.LeaveTypeName.ToLower().Contains(keyword)));
            }
            if (!string.IsNullOrWhiteSpace(request.CompensatoryStatus)) query = query.Where(x => x.CompensatoryStatus == request.CompensatoryStatus.Trim());
            if (request.WorkforceProfileId.HasValue) query = query.Where(x => x.WorkforceProfileId == request.WorkforceProfileId.Value);
            if (request.LeaveTypeId.HasValue) query = query.Where(x => x.LeaveTypeId == request.LeaveTypeId.Value);
            if (request.OvertimeRealizationId.HasValue) query = query.Where(x => x.OvertimeRealizationId == request.OvertimeRealizationId.Value);
            if (request.EarnedStartDate.HasValue) query = query.Where(x => x.EarnedDate >= request.EarnedStartDate.Value);
            if (request.EarnedEndDate.HasValue) query = query.Where(x => x.EarnedDate <= request.EarnedEndDate.Value);
            if (request.ExpiryStartDate.HasValue) query = query.Where(x => x.ExpiryDate.HasValue && x.ExpiryDate.Value >= request.ExpiryStartDate.Value);
            if (request.ExpiryEndDate.HasValue) query = query.Where(x => x.ExpiryDate.HasValue && x.ExpiryDate.Value <= request.ExpiryEndDate.Value);
            if (request.HasLedger.HasValue) query = request.HasLedger.Value ? query.Where(x => x.LeaveBalanceTransactionId.HasValue) : query.Where(x => !x.LeaveBalanceTransactionId.HasValue);
            if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
            return query;
        }

        private static IOrderedQueryable<TrxCompensatoryTimeOff> ApplySort(
            IQueryable<TrxCompensatoryTimeOff> query,
            string? sortBy,
            string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "createDateTime").Trim().ToLowerInvariant() switch
            {
                "creditnumber" => desc ? query.OrderByDescending(x => x.CreditNumber) : query.OrderBy(x => x.CreditNumber),
                "workforcedisplayname" => desc ? query.OrderByDescending(x => x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty) : query.OrderBy(x => x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty),
                "earneddate" => desc ? query.OrderByDescending(x => x.EarnedDate) : query.OrderBy(x => x.EarnedDate),
                "effectivestartdate" => desc ? query.OrderByDescending(x => x.EffectiveStartDate) : query.OrderBy(x => x.EffectiveStartDate),
                "expirydate" => desc ? query.OrderByDescending(x => x.ExpiryDate) : query.OrderBy(x => x.ExpiryDate),
                "earnedminutes" => desc ? query.OrderByDescending(x => x.EarnedMinutes) : query.OrderBy(x => x.EarnedMinutes),
                "remainingminutes" => desc ? query.OrderByDescending(x => x.RemainingMinutes) : query.OrderBy(x => x.RemainingMinutes),
                "compensatorystatus" => desc ? query.OrderByDescending(x => x.CompensatoryStatus) : query.OrderBy(x => x.CompensatoryStatus),
                _ => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime)
            };
        }

        private static OvertimeCompensatoryLeaveListResponse MapList(TrxCompensatoryTimeOff x) => new()
        {
            Id = x.Id,
            CreditNumber = x.CreditNumber,
            WorkforceProfileId = x.WorkforceProfileId,
            WorkforceProfileCode = x.WorkforceProfile?.ProfileCode ?? string.Empty,
            WorkforceDisplayName = x.WorkforceProfile?.DisplayName ?? string.Empty,
            OvertimeRequestId = x.OvertimeRequestId,
            RequestNumber = x.OvertimeRequest?.RequestNumber ?? string.Empty,
            OvertimeRealizationId = x.OvertimeRealizationId,
            RealizationNumber = x.OvertimeRealization?.RealizationNumber ?? string.Empty,
            RealizationVersion = x.OvertimeRealization?.RealizationVersion ?? 0,
            OvertimeVerificationId = x.OvertimeVerificationId,
            LeaveTypeId = x.LeaveTypeId,
            LeaveTypeCode = x.LeaveType?.LeaveTypeCode ?? string.Empty,
            LeaveTypeName = x.LeaveType?.LeaveTypeName ?? string.Empty,
            LeaveBalanceTransactionId = x.LeaveBalanceTransactionId,
            EarnedDate = x.EarnedDate,
            EffectiveStartDate = x.EffectiveStartDate,
            ExpiryDate = x.ExpiryDate,
            SourceOvertimeMinutes = x.SourceOvertimeMinutes,
            ConversionRate = x.ConversionRate,
            EarnedMinutes = x.EarnedMinutes,
            ReservedMinutes = x.ReservedMinutes,
            UsedMinutes = x.UsedMinutes,
            ExpiredMinutes = x.ExpiredMinutes,
            RemainingMinutes = x.RemainingMinutes,
            CompensatoryStatus = x.CompensatoryStatus,
            GeneratedAt = x.GeneratedAt,
            ApprovedAt = x.ApprovedAt,
            ExpiredAt = x.ExpiredAt,
            IsActive = x.IsActive,
            CreateDateTime = x.CreateDateTime
        };

        private static OvertimeCompensatoryLeaveDetailResponse MapDetail(TrxCompensatoryTimeOff x)
        {
            var item = MapList(x);
            return new OvertimeCompensatoryLeaveDetailResponse
            {
                Id = item.Id,
                CreditNumber = item.CreditNumber,
                WorkforceProfileId = item.WorkforceProfileId,
                WorkforceProfileCode = item.WorkforceProfileCode,
                WorkforceDisplayName = item.WorkforceDisplayName,
                OvertimeRequestId = item.OvertimeRequestId,
                RequestNumber = item.RequestNumber,
                OvertimeRealizationId = item.OvertimeRealizationId,
                RealizationNumber = item.RealizationNumber,
                RealizationVersion = item.RealizationVersion,
                OvertimeVerificationId = item.OvertimeVerificationId,
                LeaveTypeId = item.LeaveTypeId,
                LeaveTypeCode = item.LeaveTypeCode,
                LeaveTypeName = item.LeaveTypeName,
                LeaveBalanceTransactionId = item.LeaveBalanceTransactionId,
                EarnedDate = item.EarnedDate,
                EffectiveStartDate = item.EffectiveStartDate,
                ExpiryDate = item.ExpiryDate,
                SourceOvertimeMinutes = item.SourceOvertimeMinutes,
                ConversionRate = item.ConversionRate,
                EarnedMinutes = item.EarnedMinutes,
                ReservedMinutes = item.ReservedMinutes,
                UsedMinutes = item.UsedMinutes,
                ExpiredMinutes = item.ExpiredMinutes,
                RemainingMinutes = item.RemainingMinutes,
                CompensatoryStatus = item.CompensatoryStatus,
                GeneratedAt = item.GeneratedAt,
                ApprovedAt = item.ApprovedAt,
                ExpiredAt = item.ExpiredAt,
                IsActive = item.IsActive,
                CreateDateTime = item.CreateDateTime,
                Notes = x.Notes,
                ApprovedByUserId = x.ApprovedByUserId,
                ApprovedByName = x.ApprovedByUser?.DisplayName
            };
        }

        private static void NormalizePaging(OvertimeCompensatoryLeaveQueryRequest request)
        {
            request.PageNumber = Math.Max(1, request.PageNumber);
            request.PageSize = Math.Clamp(request.PageSize, 1, 200);
        }
    }
}
