using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public class OvertimePayrollHandoffQueryService
    {
        private readonly ApplicationDbContext _dbContext;

        public OvertimePayrollHandoffQueryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public OvertimePayrollHandoffFilterMetadataResponse GetMetadata() => new()
        {
            HandoffStatuses = OvertimeValueConstants.PayrollHandoffStatus.All.ToList(),
            RealizationStatuses = new List<string>
            {
                OvertimeValueConstants.RealizationStatus.Verified,
                OvertimeValueConstants.RealizationStatus.PostedToPayroll
            },
            BlockedPayrollRunStatuses = OvertimeValueConstants.PayrollRunStatus.Blocked.ToList(),
            SortFields = new List<string>
            {
                "overtimeDate",
                "realizationNumber",
                "requestNumber",
                "employeeName",
                "verifiedMinutes",
                "handoffStatus",
                "postedAt"
            },
            DefaultPageSize = 25,
            MaximumPageSize = 200
        };

        public async Task<OvertimePayrollHandoffSummaryResponse> GetSummaryAsync(
            OvertimePayrollHandoffQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var rows = await LoadRowsAsync(request, cancellationToken);
            return new OvertimePayrollHandoffSummaryResponse
            {
                TotalVerifiedRealization = rows.Count,
                ReadyToPost = rows.Count(x => x.HandoffStatus == OvertimeValueConstants.PayrollHandoffStatus.Ready),
                PostedToPayroll = rows.Count(x => x.HandoffStatus == OvertimeValueConstants.PayrollHandoffStatus.Posted),
                ConvertedToCompensatoryLeave = rows.Count(x => x.HandoffStatus == OvertimeValueConstants.PayrollHandoffStatus.CompensatoryLeave),
                ReconciliationIssue = rows.Count(x => x.HandoffStatus == OvertimeValueConstants.PayrollHandoffStatus.ReconciliationIssue),
                TotalVerifiedMinutes = rows.Sum(x => x.VerifiedMinutes),
                TotalPostedMinutes = rows.Sum(x => x.PostedMinutes)
            };
        }

        public async Task<PagedResult<OvertimePayrollHandoffListResponse>> GetPagedAsync(
            OvertimePayrollHandoffQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            NormalizePaging(request);
            var rows = await LoadRowsAsync(request, cancellationToken);
            rows = ApplySort(rows, request.SortBy, request.SortDirection).ToList();
            var totalData = rows.Count;
            var items = rows
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new PagedResult<OvertimePayrollHandoffListResponse>
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)request.PageSize),
                Items = items
            };
        }

        public async Task<PagedResult<OvertimePayrollHandoffOptionResponse>> GetOptionsAsync(
            string? search,
            string? handoffStatus,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var page = await GetPagedAsync(new OvertimePayrollHandoffQueryRequest
            {
                Search = search,
                HandoffStatus = handoffStatus,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = "overtimeDate",
                SortDirection = "desc"
            }, cancellationToken);

            return new PagedResult<OvertimePayrollHandoffOptionResponse>
            {
                PageNumber = page.PageNumber,
                PageSize = page.PageSize,
                TotalData = page.TotalData,
                TotalPage = page.TotalPage,
                Items = page.Items.Select(x => new OvertimePayrollHandoffOptionResponse
                {
                    Id = x.OvertimeRealizationId,
                    Code = x.RealizationNumber,
                    Label = $"{x.RealizationNumber} - {x.EmployeeName} - {x.OvertimeDate:yyyy-MM-dd}",
                    HandoffStatus = x.HandoffStatus,
                    OvertimeDate = x.OvertimeDate,
                    VerifiedMinutes = x.VerifiedMinutes
                }).ToList()
            };
        }

        public async Task<OvertimePayrollHandoffDetailResponse?> GetDetailAsync(
            Guid realizationId,
            CancellationToken cancellationToken = default)
        {
            var realization = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == realizationId, cancellationToken);
            if (realization == null) return null;

            var input = await _dbContext.TrxPayrollOvertimeInputs
                .AsNoTracking()
                .Where(x => !x.IsDelete && !x.IsCancel && x.OvertimeRealizationId == realizationId)
                .OrderByDescending(x => x.ImportedAt)
                .FirstOrDefaultAsync(cancellationToken);

            TrxPayrollRunEmployee? runEmployee = null;
            TrxPayrollRun? run = null;
            if (input != null)
            {
                runEmployee = await _dbContext.TrxPayrollRunEmployees
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == input.PayrollRunEmployeeId && !x.IsDelete, cancellationToken);
                if (runEmployee != null)
                {
                    run = await _dbContext.TrxPayrollRuns
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == runEmployee.PayrollRunId && !x.IsDelete, cancellationToken);
                }
            }

            var hasComp = realization.CompensatoryTimeOffs.Any(IsActiveCompensatoryCredit);
            var row = MapList(realization, input, runEmployee, run, hasComp);
            return new OvertimePayrollHandoffDetailResponse
            {
                OvertimeRealizationId = row.OvertimeRealizationId,
                RealizationNumber = row.RealizationNumber,
                RealizationVersion = row.RealizationVersion,
                OvertimeRequestId = row.OvertimeRequestId,
                RequestNumber = row.RequestNumber,
                WorkforceProfileId = row.WorkforceProfileId,
                EmployeeNumber = row.EmployeeNumber,
                EmployeeName = row.EmployeeName,
                OvertimeDate = row.OvertimeDate,
                VerifiedMinutes = row.VerifiedMinutes,
                PostedMinutes = row.PostedMinutes,
                RealizationStatus = row.RealizationStatus,
                HandoffStatus = row.HandoffStatus,
                HasCompensatoryLeave = row.HasCompensatoryLeave,
                PayrollOvertimeInputId = row.PayrollOvertimeInputId,
                PayrollRunEmployeeId = row.PayrollRunEmployeeId,
                PayrollRunId = row.PayrollRunId,
                PayrollPeriodId = row.PayrollPeriodId,
                PayrollComponentId = row.PayrollComponentId,
                PostedToPayrollAt = row.PostedToPayrollAt,
                RequestedMinutes = realization.RequestedMinutesSnapshot,
                ApprovedMinutes = realization.ApprovedMinutesSnapshot,
                ActualMinutes = realization.ActualMinutes,
                EligibleMinutes = realization.EligibleMinutes,
                CurrencyCode = realization.CurrencyCode,
                CalculationSnapshotJson = input?.CalculationSnapshotJson,
                PayrollInputNotes = input?.Notes,
                ImportedAt = input?.ImportedAt,
                ImportedByUserId = input?.ImportedByUserId,
                RateSnapshots = realization.Details
                    .Where(x => x.IsActive && !x.IsDelete && !x.IsCancel)
                    .OrderBy(x => x.SequenceNumber)
                    .Select(x => new OvertimePayrollRateSnapshotResponse
                    {
                        OvertimeRateId = x.OvertimeRateId,
                        OvertimeDate = x.OvertimeDate,
                        DayType = x.DayType,
                        RateBand = x.RateBandSnapshot,
                        VerifiedMinutes = x.VerifiedMinutes,
                        RateMultiplier = x.RateMultiplierSnapshot,
                        HourlyRateSnapshot = x.BaseHourlyRateSnapshot
                    }).ToList()
            };
        }

        private async Task<List<OvertimePayrollHandoffListResponse>> LoadRowsAsync(
            OvertimePayrollHandoffQueryRequest request,
            CancellationToken cancellationToken)
        {
            var query = BuildBaseQuery();
            if (request.WorkforceProfileId.HasValue && request.WorkforceProfileId != Guid.Empty)
                query = query.Where(x => x.WorkforceProfileId == request.WorkforceProfileId);
            if (request.PayrollPeriodId.HasValue && request.PayrollPeriodId != Guid.Empty)
                query = query.Where(x => x.PayrollPeriodId == request.PayrollPeriodId || x.PayrollPeriodId == null);
            if (request.PayrollComponentId.HasValue && request.PayrollComponentId != Guid.Empty)
                query = query.Where(x => x.PayrollComponentId == request.PayrollComponentId || x.PayrollComponentId == null);
            if (request.StartDate.HasValue)
                query = query.Where(x => x.ActualEndDate >= request.StartDate.Value);
            if (request.EndDate.HasValue)
                query = query.Where(x => x.ActualEndDate <= request.EndDate.Value);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim().ToLower();
                query = query.Where(x =>
                    x.RealizationNumber.ToLower().Contains(keyword) ||
                    (x.OvertimeRequest != null && x.OvertimeRequest.RequestNumber.ToLower().Contains(keyword)) ||
                    (x.Employee != null && x.Employee.EmployeeNumber.ToLower().Contains(keyword)) ||
                    (x.Employee != null && x.Employee.FullName.ToLower().Contains(keyword)));
            }

            var realizations = await query.ToListAsync(cancellationToken);
            var realizationIds = realizations.Select(x => x.Id).ToList();
            var inputs = await _dbContext.TrxPayrollOvertimeInputs
                .AsNoTracking()
                .Where(x => !x.IsDelete && !x.IsCancel && x.OvertimeRealizationId.HasValue && realizationIds.Contains(x.OvertimeRealizationId.Value))
                .OrderByDescending(x => x.ImportedAt)
                .ToListAsync(cancellationToken);
            var inputMap = inputs
                .GroupBy(x => x.OvertimeRealizationId!.Value)
                .ToDictionary(x => x.Key, x => x.First());

            var runEmployeeIds = inputs.Select(x => x.PayrollRunEmployeeId).Distinct().ToList();
            var runEmployees = await _dbContext.TrxPayrollRunEmployees
                .AsNoTracking()
                .Where(x => runEmployeeIds.Contains(x.Id) && !x.IsDelete)
                .ToListAsync(cancellationToken);
            var runEmployeeMap = runEmployees.ToDictionary(x => x.Id);
            var runIds = runEmployees.Select(x => x.PayrollRunId).Distinct().ToList();
            var runs = await _dbContext.TrxPayrollRuns
                .AsNoTracking()
                .Where(x => runIds.Contains(x.Id) && !x.IsDelete)
                .ToListAsync(cancellationToken);
            var runMap = runs.ToDictionary(x => x.Id);

            var rows = realizations.Select(realization =>
            {
                inputMap.TryGetValue(realization.Id, out var input);
                TrxPayrollRunEmployee? runEmployee = null;
                TrxPayrollRun? run = null;
                if (input != null && runEmployeeMap.TryGetValue(input.PayrollRunEmployeeId, out var foundEmployee))
                {
                    runEmployee = foundEmployee;
                    runMap.TryGetValue(foundEmployee.PayrollRunId, out run);
                }
                return MapList(realization, input, runEmployee, run, realization.CompensatoryTimeOffs.Any(IsActiveCompensatoryCredit));
            }).ToList();

            if (request.PayrollRunId.HasValue && request.PayrollRunId != Guid.Empty)
                rows = rows.Where(x => x.PayrollRunId == request.PayrollRunId).ToList();
            if (!string.IsNullOrWhiteSpace(request.HandoffStatus))
                rows = rows.Where(x => string.Equals(x.HandoffStatus, request.HandoffStatus.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
            if (request.ExcludeCompensatoryLeave)
                rows = rows.Where(x => !x.HasCompensatoryLeave).ToList();
            return rows;
        }

        private IQueryable<TrxOvertimeRealization> BuildBaseQuery() =>
            _dbContext.TrxOvertimeRealizations
                .AsNoTracking()
                .Include(x => x.OvertimeRequest)
                .Include(x => x.Employee)
                .Include(x => x.Details)
                .Include(x => x.CompensatoryTimeOffs)
                .Where(x =>
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive &&
                    (x.RealizationStatus == OvertimeValueConstants.RealizationStatus.Verified ||
                     x.RealizationStatus == OvertimeValueConstants.RealizationStatus.PostedToPayroll));

        private static OvertimePayrollHandoffListResponse MapList(
            TrxOvertimeRealization realization,
            TrxPayrollOvertimeInput? input,
            TrxPayrollRunEmployee? runEmployee,
            TrxPayrollRun? run,
            bool hasCompensatoryLeave)
        {
            var handoffStatus = ResolveHandoffStatus(realization, input, hasCompensatoryLeave);
            return new OvertimePayrollHandoffListResponse
            {
                OvertimeRealizationId = realization.Id,
                RealizationNumber = realization.RealizationNumber,
                RealizationVersion = realization.RealizationVersion,
                OvertimeRequestId = realization.OvertimeRequestId,
                RequestNumber = realization.OvertimeRequest?.RequestNumber ?? string.Empty,
                WorkforceProfileId = realization.WorkforceProfileId,
                EmployeeNumber = realization.Employee?.EmployeeNumber ?? string.Empty,
                EmployeeName = realization.Employee?.FullName ?? string.Empty,
                OvertimeDate = realization.ActualEndDate,
                VerifiedMinutes = realization.VerifiedMinutes,
                PostedMinutes = realization.PostedMinutes,
                RealizationStatus = realization.RealizationStatus,
                HandoffStatus = handoffStatus,
                HasCompensatoryLeave = hasCompensatoryLeave,
                PayrollOvertimeInputId = input?.Id,
                PayrollRunEmployeeId = runEmployee?.Id,
                PayrollRunId = run?.Id,
                PayrollPeriodId = realization.PayrollPeriodId ?? run?.PayrollPeriodId,
                PayrollComponentId = realization.PayrollComponentId,
                PostedToPayrollAt = realization.PostedToPayrollAt
            };
        }

        private static string ResolveHandoffStatus(
            TrxOvertimeRealization realization,
            TrxPayrollOvertimeInput? input,
            bool hasCompensatoryLeave)
        {
            if (hasCompensatoryLeave) return OvertimeValueConstants.PayrollHandoffStatus.CompensatoryLeave;
            if (input != null && realization.IsPayrollPosted && realization.PostedMinutes == input.VerifiedMinutes)
                return OvertimeValueConstants.PayrollHandoffStatus.Posted;
            if (input != null || realization.IsPayrollPosted)
                return OvertimeValueConstants.PayrollHandoffStatus.ReconciliationIssue;
            return OvertimeValueConstants.PayrollHandoffStatus.Ready;
        }

        private static bool IsActiveCompensatoryCredit(TrxCompensatoryTimeOff credit) =>
            credit.IsActive &&
            !credit.IsDelete &&
            !credit.IsCancel &&
            !string.Equals(credit.CompensatoryStatus, OvertimeValueConstants.CompensatoryStatus.Cancelled, StringComparison.OrdinalIgnoreCase);

        private static IEnumerable<OvertimePayrollHandoffListResponse> ApplySort(
            IEnumerable<OvertimePayrollHandoffListResponse> rows,
            string? sortBy,
            string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "overtimeDate").Trim().ToLowerInvariant() switch
            {
                "realizationnumber" => desc ? rows.OrderByDescending(x => x.RealizationNumber) : rows.OrderBy(x => x.RealizationNumber),
                "requestnumber" => desc ? rows.OrderByDescending(x => x.RequestNumber) : rows.OrderBy(x => x.RequestNumber),
                "employeename" => desc ? rows.OrderByDescending(x => x.EmployeeName) : rows.OrderBy(x => x.EmployeeName),
                "verifiedminutes" => desc ? rows.OrderByDescending(x => x.VerifiedMinutes) : rows.OrderBy(x => x.VerifiedMinutes),
                "handoffstatus" => desc ? rows.OrderByDescending(x => x.HandoffStatus) : rows.OrderBy(x => x.HandoffStatus),
                "postedat" => desc ? rows.OrderByDescending(x => x.PostedToPayrollAt) : rows.OrderBy(x => x.PostedToPayrollAt),
                _ => desc ? rows.OrderByDescending(x => x.OvertimeDate).ThenByDescending(x => x.RealizationNumber) : rows.OrderBy(x => x.OvertimeDate).ThenBy(x => x.RealizationNumber)
            };
        }

        private static void NormalizePaging(OvertimePayrollHandoffQueryRequest request)
        {
            if (request.PageNumber <= 0) request.PageNumber = 1;
            if (request.PageSize <= 0) request.PageSize = 25;
            if (request.PageSize > 200) request.PageSize = 200;
        }
    }
}
