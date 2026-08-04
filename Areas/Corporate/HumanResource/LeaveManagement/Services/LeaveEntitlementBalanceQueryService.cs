using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    public class LeaveEntitlementBalanceQueryService
    {
        private const decimal ReconciliationTolerance = 0.0001m;
        private readonly ApplicationDbContext _dbContext;

        public LeaveEntitlementBalanceQueryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public LeaveEntitlementPeriodFilterMetadataResponse GetPeriodMetadata()
        {
            return new LeaveEntitlementPeriodFilterMetadataResponse
            {
                DefaultFilter = new LeaveEntitlementPeriodQueryRequest(),
                CustomPeriods = BuildCustomPeriods(),
                PeriodStatusOptions = new List<LeaveQueryOptionResponse>
                {
                    new() { Value = LeaveValueConstants.PeriodStatus.Open, Label = "Open" },
                    new() { Value = LeaveValueConstants.PeriodStatus.Processing, Label = "Processing" },
                    new() { Value = LeaveValueConstants.PeriodStatus.Closed, Label = "Closed" },
                    new() { Value = LeaveValueConstants.PeriodStatus.Reopened, Label = "Reopened" },
                    new() { Value = LeaveValueConstants.PeriodStatus.Cancelled, Label = "Cancelled" }
                },
                PeriodBasisOptions = new List<LeaveQueryOptionResponse>
                {
                    new() { Value = LeaveValueConstants.PeriodBasis.CalendarYear, Label = "Calendar Year" },
                    new() { Value = LeaveValueConstants.PeriodBasis.AnniversaryYear, Label = "Anniversary Year" },
                    new() { Value = LeaveValueConstants.PeriodBasis.FiscalYear, Label = "Fiscal Year" },
                    new() { Value = LeaveValueConstants.PeriodBasis.ContractPeriod, Label = "Contract Period" },
                    new() { Value = LeaveValueConstants.PeriodBasis.Custom, Label = "Custom" }
                },
                SortOptions = new List<LeaveQueryOptionResponse>
                {
                    new() { Value = "startDate", Label = "Tanggal mulai" },
                    new() { Value = "endDate", Label = "Tanggal selesai" },
                    new() { Value = "periodCode", Label = "Kode periode" },
                    new() { Value = "periodName", Label = "Nama periode" },
                    new() { Value = "periodYear", Label = "Tahun" },
                    new() { Value = "periodStatus", Label = "Status" },
                    new() { Value = "balanceCount", Label = "Jumlah saldo" },
                    new() { Value = "availableDays", Label = "Total tersedia" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };
        }

        public LeaveBalanceFilterMetadataResponse GetBalanceMetadata()
        {
            return new LeaveBalanceFilterMetadataResponse
            {
                DefaultFilter = new LeaveBalanceQueryRequest(),
                BalanceStatusOptions = new List<LeaveQueryOptionResponse>
                {
                    new() { Value = LeaveValueConstants.BalanceStatus.Active, Label = "Aktif" },
                    new() { Value = LeaveValueConstants.BalanceStatus.Locked, Label = "Terkunci" },
                    new() { Value = LeaveValueConstants.BalanceStatus.Closed, Label = "Ditutup" },
                    new() { Value = LeaveValueConstants.BalanceStatus.Expired, Label = "Kedaluwarsa" },
                    new() { Value = LeaveValueConstants.BalanceStatus.Cancelled, Label = "Dibatalkan" }
                },
                ReconciliationOptions = new List<LeaveQueryOptionResponse>
                {
                    new() { Value = "balanced", Label = "Sesuai formula" },
                    new() { Value = "mismatch", Label = "Terdapat selisih" }
                },
                SortOptions = new List<LeaveQueryOptionResponse>
                {
                    new() { Value = "workforceDisplayName", Label = "Nama workforce" },
                    new() { Value = "profileCode", Label = "Kode workforce" },
                    new() { Value = "leaveTypeName", Label = "Jenis cuti" },
                    new() { Value = "year", Label = "Tahun" },
                    new() { Value = "availableDays", Label = "Saldo tersedia" },
                    new() { Value = "remainingDays", Label = "Saldo tersisa" },
                    new() { Value = "reservedDays", Label = "Saldo ditahan" },
                    new() { Value = "usedDays", Label = "Saldo terpakai" },
                    new() { Value = "balanceStatus", Label = "Status saldo" },
                    new() { Value = "lastCalculatedAt", Label = "Terakhir dihitung" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };
        }

        public async Task<LeaveEntitlementPeriodSummaryResponse> GetPeriodSummaryAsync(
            LeaveEntitlementPeriodQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var query = BuildPeriodQuery(request);
            var ids = query.Select(x => x.Id);
            var balances = _dbContext.Set<WfpLeaveBalance>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.LeaveEntitlementPeriodId.HasValue && ids.Contains(x.LeaveEntitlementPeriodId.Value));

            return new LeaveEntitlementPeriodSummaryResponse
            {
                TotalPeriod = await query.CountAsync(cancellationToken),
                OpenPeriod = await query.CountAsync(x => x.PeriodStatus == LeaveValueConstants.PeriodStatus.Open, cancellationToken),
                ProcessingPeriod = await query.CountAsync(x => x.PeriodStatus == LeaveValueConstants.PeriodStatus.Processing, cancellationToken),
                ClosedPeriod = await query.CountAsync(x => x.PeriodStatus == LeaveValueConstants.PeriodStatus.Closed, cancellationToken),
                ReopenedPeriod = await query.CountAsync(x => x.PeriodStatus == LeaveValueConstants.PeriodStatus.Reopened, cancellationToken),
                CancelledPeriod = await query.CountAsync(x => x.PeriodStatus == LeaveValueConstants.PeriodStatus.Cancelled, cancellationToken),
                LockedPeriod = await query.CountAsync(x => x.IsLocked, cancellationToken),
                ActivePeriod = await query.CountAsync(x => x.IsActive, cancellationToken),
                TotalBalance = await balances.CountAsync(cancellationToken),
                TotalWorkforce = await balances.Select(x => x.WorkforceProfileId).Distinct().CountAsync(cancellationToken),
                TotalRemainingDays = await balances.SumAsync(x => (decimal?)x.RemainingDays, cancellationToken) ?? 0,
                TotalAvailableDays = await balances.SumAsync(x => (decimal?)x.AvailableDays, cancellationToken) ?? 0,
                TotalReservedDays = await balances.SumAsync(x => (decimal?)x.ReservedDays, cancellationToken) ?? 0
            };
        }

        public async Task<LeaveEntitlementPeriodPagedResponse> GetPeriodPagedAsync(
            LeaveEntitlementPeriodQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, 100);
            var query = BuildPeriodQuery(request);
            var totalData = await query.CountAsync(cancellationToken);

            var items = await ProjectPeriods(ApplyPeriodSorting(query, request.SortBy, request.SortDirection))
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new LeaveEntitlementPeriodPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task<List<LeaveEntitlementPeriodOptionResponse>> GetPeriodOptionsAsync(
            Guid? leaveTypeId,
            int? periodYear,
            bool onlyOpen,
            string? search,
            int take,
            CancellationToken cancellationToken = default)
        {
            take = take < 1 ? 25 : Math.Min(take, 100);
            var query = _dbContext.Set<TrxLeaveEntitlementPeriod>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive);

            if (leaveTypeId.HasValue && leaveTypeId.Value != Guid.Empty)
                query = query.Where(x => x.LeaveTypeId == leaveTypeId.Value || x.LeaveTypeId == null);
            if (periodYear.HasValue)
                query = query.Where(x => x.PeriodYear == periodYear.Value);
            if (onlyOpen)
                query = query.Where(x =>
                    !x.IsLocked &&
                    (x.PeriodStatus == LeaveValueConstants.PeriodStatus.Open ||
                     x.PeriodStatus == LeaveValueConstants.PeriodStatus.Reopened));
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.PeriodCode.ToLower().Contains(keyword) ||
                    x.PeriodName.ToLower().Contains(keyword) ||
                    (x.LeaveType != null && x.LeaveType.LeaveTypeName.ToLower().Contains(keyword)));
            }

            return await query
                .OrderByDescending(x => x.StartDate)
                .ThenBy(x => x.PeriodName)
                .Take(take)
                .Select(x => new LeaveEntitlementPeriodOptionResponse
                {
                    Id = x.Id,
                    PeriodCode = x.PeriodCode,
                    PeriodName = x.PeriodName,
                    PeriodYear = x.PeriodYear,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    PeriodStatus = x.PeriodStatus,
                    LeaveTypeId = x.LeaveTypeId,
                    LeaveTypeName = x.LeaveType != null ? x.LeaveType.LeaveTypeName : null,
                    IsLocked = x.IsLocked
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<LeaveEntitlementPeriodDetailResponse?> GetPeriodDetailAsync(
            Guid periodId,
            CancellationToken cancellationToken = default)
        {
            var item = await _dbContext.Set<TrxLeaveEntitlementPeriod>()
                .AsNoTracking()
                .Where(x => x.Id == periodId && !x.IsDelete)
                .Select(x => new LeaveEntitlementPeriodDetailResponse
                {
                    Id = x.Id,
                    LeaveTypeId = x.LeaveTypeId,
                    LeaveTypeCode = x.LeaveType != null ? x.LeaveType.LeaveTypeCode : null,
                    LeaveTypeName = x.LeaveType != null ? x.LeaveType.LeaveTypeName : null,
                    LegalEntityId = x.LegalEntityId,
                    LegalEntityName = x.LegalEntity != null ? x.LegalEntity.LegalEntityName : null,
                    HospitalSiteId = x.HospitalSiteId,
                    HospitalSiteName = x.HospitalSite != null ? x.HospitalSite.SiteName : null,
                    OrganizationUnitId = x.OrganizationUnitId,
                    OrganizationUnitName = x.OrganizationUnit != null ? x.OrganizationUnit.UnitName : null,
                    DepartmentId = x.DepartmentId,
                    DepartmentName = x.Department != null ? x.Department.DepartmentName : null,
                    PeriodCode = x.PeriodCode,
                    PeriodName = x.PeriodName,
                    PeriodBasis = x.PeriodBasis,
                    PeriodYear = x.PeriodYear,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    PeriodStatus = x.PeriodStatus,
                    IsLocked = x.IsLocked,
                    IsActive = x.IsActive,
                    LeaveBalanceCount = x.LeaveBalances.Count(b => !b.IsDelete),
                    WorkforceCount = x.LeaveBalances.Where(b => !b.IsDelete).Select(b => b.WorkforceProfileId).Distinct().Count(),
                    EntitlementCount = x.Entitlements.Count(e => !e.IsDelete),
                    AccrualRunCount = x.AccrualRuns.Count(r => !r.IsDelete),
                    SourceCarryForwardRunCount = x.SourceCarryForwardRuns.Count(r => !r.IsDelete),
                    DestinationCarryForwardRunCount = x.DestinationCarryForwardRuns.Count(r => !r.IsDelete),
                    AdjustmentCount = x.Adjustments.Count(a => !a.IsDelete),
                    TotalRemainingDays = x.LeaveBalances.Where(b => !b.IsDelete).Sum(b => (decimal?)b.RemainingDays) ?? 0,
                    TotalAvailableDays = x.LeaveBalances.Where(b => !b.IsDelete).Sum(b => (decimal?)b.AvailableDays) ?? 0,
                    ProcessingStartedAt = x.ProcessingStartedAt,
                    ProcessingStartedByUserId = x.ProcessingStartedByUserId,
                    ProcessingStartedByName = x.ProcessingStartedByUser != null
                        ? x.ProcessingStartedByUser.DisplayName ?? x.ProcessingStartedByUser.UserName ?? x.ProcessingStartedByUser.Email ?? x.ProcessingStartedByUser.UserCode
                        : null,
                    ClosedAt = x.ClosedAt,
                    ClosedByUserId = x.ClosedByUserId,
                    ClosedByName = x.ClosedByUser != null
                        ? x.ClosedByUser.DisplayName ?? x.ClosedByUser.UserName ?? x.ClosedByUser.Email ?? x.ClosedByUser.UserCode
                        : null,
                    CloseReason = x.CloseReason,
                    ReopenedAt = x.ReopenedAt,
                    ReopenedByUserId = x.ReopenedByUserId,
                    ReopenedByName = x.ReopenedByUser != null
                        ? x.ReopenedByUser.DisplayName ?? x.ReopenedByUser.UserName ?? x.ReopenedByUser.Email ?? x.ReopenedByUser.UserCode
                        : null,
                    ReopenReason = x.ReopenReason,
                    ReopenCount = x.ReopenCount,
                    LastReconciledAt = x.LastReconciledAt,
                    ValidationSnapshotJson = x.ValidationSnapshotJson,
                    Description = x.Description,
                    CreateDateTime = x.CreateDateTime,
                    UpdateDateTime = x.UpdateDateTime
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (item == null)
                return null;

            item.BalanceBreakdown = await _dbContext.Set<WfpLeaveBalance>()
                .AsNoTracking()
                .Where(x => x.LeaveEntitlementPeriodId == periodId && !x.IsDelete)
                .GroupBy(x => new
                {
                    x.LeaveTypeId,
                    x.LeaveType!.LeaveTypeCode,
                    x.LeaveType.LeaveTypeName
                })
                .Select(x => new LeavePeriodBalanceBreakdownResponse
                {
                    LeaveTypeId = x.Key.LeaveTypeId,
                    LeaveTypeCode = x.Key.LeaveTypeCode,
                    LeaveTypeName = x.Key.LeaveTypeName,
                    BalanceCount = x.Count(),
                    WorkforceCount = x.Select(y => y.WorkforceProfileId).Distinct().Count(),
                    OpeningBalanceDays = x.Sum(y => y.OpeningBalanceDays),
                    EntitlementDays = x.Sum(y => y.EntitlementDays),
                    AccruedDays = x.Sum(y => y.AccruedDays),
                    CarriedForwardDays = x.Sum(y => y.CarriedForwardDays),
                    AdjustmentDays = x.Sum(y => y.AdjustmentDays),
                    UsedDays = x.Sum(y => y.UsedDays),
                    ReservedDays = x.Sum(y => y.ReservedDays),
                    RemainingDays = x.Sum(y => y.RemainingDays),
                    AvailableDays = x.Sum(y => y.AvailableDays)
                })
                .OrderBy(x => x.LeaveTypeName)
                .ToListAsync(cancellationToken);

            return item;
        }

        public async Task<LeaveBalanceSummaryResponse> GetBalanceSummaryAsync(
            LeaveBalanceQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var query = BuildBalanceQuery(request);
            var list = await query
                .Select(x => new
                {
                    x.BalanceStatus,
                    x.IsLocked,
                    x.WorkforceProfileId,
                    x.LeaveTypeId,
                    x.OpeningBalanceDays,
                    x.EntitlementDays,
                    x.AccruedDays,
                    x.CarriedForwardDays,
                    x.AdjustmentDays,
                    x.ReservedDays,
                    x.UsedDays,
                    x.ExpiredDays,
                    x.RemainingDays,
                    x.AvailableDays,
                    ExpectedRemaining =
                        x.OpeningBalanceDays + x.EntitlementDays + x.AccruedDays +
                        x.CarriedForwardDays + x.AdjustmentDays + x.CompensatoryDays +
                        x.RecalledDays - x.UsedDays - x.ExpiredDays - x.EncashmentDays,
                    ExpectedAvailable = x.RemainingDays - x.ReservedDays
                })
                .ToListAsync(cancellationToken);

            return new LeaveBalanceSummaryResponse
            {
                TotalBalance = list.Count,
                ActiveBalance = list.Count(x => x.BalanceStatus == LeaveValueConstants.BalanceStatus.Active),
                LockedBalance = list.Count(x => x.IsLocked || x.BalanceStatus == LeaveValueConstants.BalanceStatus.Locked),
                ClosedBalance = list.Count(x => x.BalanceStatus == LeaveValueConstants.BalanceStatus.Closed),
                ExpiredBalance = list.Count(x => x.BalanceStatus == LeaveValueConstants.BalanceStatus.Expired),
                WorkforceCount = list.Select(x => x.WorkforceProfileId).Distinct().Count(),
                LeaveTypeCount = list.Select(x => x.LeaveTypeId).Distinct().Count(),
                BalanceWithAvailableDays = list.Count(x => x.AvailableDays > 0),
                BalanceWithReservedDays = list.Count(x => x.ReservedDays > 0),
                BalanceWithMismatch = list.Count(x =>
                    Math.Abs(x.RemainingDays - x.ExpectedRemaining) > ReconciliationTolerance ||
                    Math.Abs(x.AvailableDays - x.ExpectedAvailable) > ReconciliationTolerance),
                TotalOpeningBalanceDays = list.Sum(x => x.OpeningBalanceDays),
                TotalEntitlementDays = list.Sum(x => x.EntitlementDays),
                TotalAccruedDays = list.Sum(x => x.AccruedDays),
                TotalCarriedForwardDays = list.Sum(x => x.CarriedForwardDays),
                TotalAdjustmentDays = list.Sum(x => x.AdjustmentDays),
                TotalReservedDays = list.Sum(x => x.ReservedDays),
                TotalUsedDays = list.Sum(x => x.UsedDays),
                TotalExpiredDays = list.Sum(x => x.ExpiredDays),
                TotalRemainingDays = list.Sum(x => x.RemainingDays),
                TotalAvailableDays = list.Sum(x => x.AvailableDays)
            };
        }

        public async Task<LeaveBalancePagedResponse> GetBalancePagedAsync(
            LeaveBalanceQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, 100);
            var query = BuildBalanceQuery(request);
            var totalData = await query.CountAsync(cancellationToken);

            var items = await ProjectBalances(ApplyBalanceSorting(query, request.SortBy, request.SortDirection))
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            CompleteFormulaState(items);

            return new LeaveBalancePagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task<LeaveBalanceDetailResponse?> GetBalanceDetailAsync(
            Guid balanceId,
            Guid? scopedWorkforceProfileId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Set<WfpLeaveBalance>()
                .AsNoTracking()
                .Where(x => x.Id == balanceId && !x.IsDelete);

            if (scopedWorkforceProfileId.HasValue)
                query = query.Where(x => x.WorkforceProfileId == scopedWorkforceProfileId.Value);

            var item = await query
                .Select(x => new LeaveBalanceDetailResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : null,
                    WorkforceDisplayName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty,
                    EmployeeNumber = x.WorkforceProfile != null && x.WorkforceProfile.Employee != null ? x.WorkforceProfile.Employee.EmployeeNumber : null,
                    DepartmentId = x.WorkforceProfile != null ? x.WorkforceProfile.PrimaryDepartmentId : null,
                    DepartmentName = x.WorkforceProfile != null && x.WorkforceProfile.PrimaryDepartment != null ? x.WorkforceProfile.PrimaryDepartment.DepartmentName : null,
                    PositionId = x.WorkforceProfile != null ? x.WorkforceProfile.PrimaryPositionId : null,
                    PositionName = x.WorkforceProfile != null && x.WorkforceProfile.PrimaryPosition != null ? x.WorkforceProfile.PrimaryPosition.PositionName : null,
                    LeaveTypeId = x.LeaveTypeId,
                    LeaveTypeCode = x.LeaveType != null ? x.LeaveType.LeaveTypeCode : string.Empty,
                    LeaveTypeName = x.LeaveType != null ? x.LeaveType.LeaveTypeName : string.Empty,
                    LeaveCategory = x.LeaveType != null ? x.LeaveType.LeaveCategory : string.Empty,
                    LeaveEntitlementPeriodId = x.LeaveEntitlementPeriodId,
                    PeriodCode = x.LeaveEntitlementPeriod != null ? x.LeaveEntitlementPeriod.PeriodCode : null,
                    PeriodName = x.LeaveEntitlementPeriod != null ? x.LeaveEntitlementPeriod.PeriodName : null,
                    Year = x.Year,
                    PeriodStartDate = x.PeriodStartDate,
                    PeriodEndDate = x.PeriodEndDate,
                    OpeningBalanceDays = x.OpeningBalanceDays,
                    EntitlementDays = x.EntitlementDays,
                    AccruedDays = x.AccruedDays,
                    CarriedForwardDays = x.CarriedForwardDays,
                    AdjustmentDays = x.AdjustmentDays,
                    CompensatoryDays = x.CompensatoryDays,
                    ReservedDays = x.ReservedDays,
                    PendingDays = x.PendingDays,
                    UsedDays = x.UsedDays,
                    RecalledDays = x.RecalledDays,
                    ExpiredDays = x.ExpiredDays,
                    EncashmentDays = x.EncashmentDays,
                    RemainingDays = x.RemainingDays,
                    AvailableDays = x.AvailableDays,
                    BalanceStatus = x.BalanceStatus,
                    IsLocked = x.IsLocked,
                    IsActive = x.IsActive,
                    BalanceVersion = x.BalanceVersion,
                    LastTransactionSequence = x.LastTransactionSequence,
                    LastCalculatedAt = x.LastCalculatedAt,
                    LastReconciledAt = x.LastReconciledAt,
                    CarryForwardExpiryDate = x.CarryForwardExpiryDate,
                    LeavePolicyId = x.LeavePolicyId,
                    LeavePolicyCode = x.LeavePolicy != null ? x.LeavePolicy.LeavePolicyCode : null,
                    LeavePolicyName = x.LeavePolicy != null ? x.LeavePolicy.LeavePolicyName : null,
                    LeaveEntitlementPolicyId = x.LeaveEntitlementPolicyId,
                    EntitlementPolicyCode = x.LeaveEntitlementPolicy != null ? x.LeaveEntitlementPolicy.EntitlementPolicyCode : null,
                    EntitlementPolicyName = x.LeaveEntitlementPolicy != null ? x.LeaveEntitlementPolicy.EntitlementPolicyName : null,
                    LastTransactionId = x.LastTransactionId,
                    Description = x.Description,
                    LockedAt = x.LockedAt,
                    LockedByUserId = x.LockedByUserId,
                    LockedByName = x.LockedByUser != null
                        ? x.LockedByUser.DisplayName ?? x.LockedByUser.UserName ?? x.LockedByUser.Email ?? x.LockedByUser.UserCode
                        : null,
                    CreateDateTime = x.CreateDateTime,
                    UpdateDateTime = x.UpdateDateTime,
                    HistorySummary = new LeaveBalanceHistorySummaryResponse
                    {
                        LedgerCount = x.Transactions.Count(t => !t.IsDelete),
                        EntitlementCount = x.Entitlements.Count(t => !t.IsDelete),
                        AccrualCount = x.Accruals.Count(t => !t.IsDelete),
                        SourceCarryForwardCount = _dbContext.Set<TrxLeaveCarryForward>().Count(c => !c.IsDelete && c.SourceLeaveBalanceId == x.Id),
                        DestinationCarryForwardCount = _dbContext.Set<TrxLeaveCarryForward>().Count(c => !c.IsDelete && c.DestinationLeaveBalanceId == x.Id),
                        AdjustmentCount = x.Adjustments.Count(t => !t.IsDelete)
                    }
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (item == null)
                return null;

            CompleteFormulaState(new[] { item });
            item.Reconciliation = await GetReconciliationAsync(balanceId, scopedWorkforceProfileId, cancellationToken)
                ?? new LeaveBalanceReconciliationResponse { LeaveBalanceId = balanceId };
            return item;
        }

        public async Task<LeaveBalanceTransactionPagedResponse?> GetLedgerAsync(
            Guid balanceId,
            LeaveBalanceLedgerQueryRequest request,
            Guid? scopedWorkforceProfileId = null,
            CancellationToken cancellationToken = default)
        {
            if (!await BalanceExistsAsync(balanceId, scopedWorkforceProfileId, cancellationToken))
                return null;

            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, 100);
            var query = _dbContext.Set<TrxLeaveBalanceTransaction>()
                .AsNoTracking()
                .Where(x => x.LeaveBalanceId == balanceId && !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(request.TransactionType))
                query = query.Where(x => x.TransactionType == request.TransactionType.Trim());
            if (!string.IsNullOrWhiteSpace(request.TransactionStatus))
                query = query.Where(x => x.TransactionStatus == request.TransactionStatus.Trim());
            if (request.EffectiveStartDate.HasValue)
                query = query.Where(x => x.EffectiveDate.HasValue && x.EffectiveDate.Value >= request.EffectiveStartDate.Value);
            if (request.EffectiveEndDate.HasValue)
                query = query.Where(x => x.EffectiveDate.HasValue && x.EffectiveDate.Value <= request.EffectiveEndDate.Value);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim().ToLower();
                query = query.Where(x =>
                    x.TransactionNumber.ToLower().Contains(keyword) ||
                    x.TransactionType.ToLower().Contains(keyword) ||
                    (x.SourceReferenceNumber != null && x.SourceReferenceNumber.ToLower().Contains(keyword)) ||
                    (x.Remarks != null && x.Remarks.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync(cancellationToken);
            query = string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(x => x.TransactionSequence).ThenBy(x => x.TransactionDateTime)
                : query.OrderByDescending(x => x.TransactionSequence).ThenByDescending(x => x.TransactionDateTime);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new LeaveBalanceTransactionResponse
                {
                    Id = x.Id,
                    TransactionNumber = x.TransactionNumber,
                    TransactionDateTime = x.TransactionDateTime,
                    EffectiveDate = x.EffectiveDate,
                    TransactionSequence = x.TransactionSequence,
                    TransactionType = x.TransactionType,
                    Direction = x.Direction,
                    TransactionDays = x.TransactionDays,
                    OpeningBalanceDelta = x.OpeningBalanceDelta,
                    EntitlementDelta = x.EntitlementDelta,
                    AccruedDelta = x.AccruedDelta,
                    CarryForwardDelta = x.CarryForwardDelta,
                    AdjustmentDelta = x.AdjustmentDelta,
                    CompensatoryDelta = x.CompensatoryDelta,
                    PendingDelta = x.PendingDelta,
                    ReservedDelta = x.ReservedDelta,
                    UsedDelta = x.UsedDelta,
                    RecalledDelta = x.RecalledDelta,
                    ExpiredDelta = x.ExpiredDelta,
                    EncashmentDelta = x.EncashmentDelta,
                    AvailableDelta = x.AvailableDelta,
                    PreviousAvailableDays = x.PreviousAvailableDays,
                    NewAvailableDays = x.NewAvailableDays,
                    PreviousReservedDays = x.PreviousReservedDays,
                    NewReservedDays = x.NewReservedDays,
                    NewUsedDays = x.NewUsedDays,
                    TransactionStatus = x.TransactionStatus,
                    PostingBatchType = x.PostingBatchType,
                    PostingBatchId = x.PostingBatchId,
                    SourceType = x.SourceType,
                    SourceReferenceId = x.SourceReferenceId,
                    SourceReferenceNumber = x.SourceReferenceNumber,
                    OriginalTransactionId = x.OriginalTransactionId,
                    ReversedTransactionId = x.ReversedTransactionId,
                    PostedAt = x.PostedAt,
                    PostedByUserId = x.PostedByUserId,
                    PostedByName = x.PostedByUser != null
                        ? x.PostedByUser.DisplayName ?? x.PostedByUser.UserName ?? x.PostedByUser.Email ?? x.PostedByUser.UserCode
                        : null,
                    Remarks = x.Remarks
                })
                .ToListAsync(cancellationToken);

            return new LeaveBalanceTransactionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task<List<LeaveEntitlementHistoryResponse>?> GetEntitlementsAsync(
            Guid balanceId,
            Guid? scopedWorkforceProfileId = null,
            CancellationToken cancellationToken = default)
        {
            if (!await BalanceExistsAsync(balanceId, scopedWorkforceProfileId, cancellationToken))
                return null;

            return await _dbContext.Set<TrxLeaveEntitlement>()
                .AsNoTracking()
                .Where(x => x.LeaveBalanceId == balanceId && !x.IsDelete)
                .OrderByDescending(x => x.GrantDate)
                .ThenByDescending(x => x.CreateDateTime)
                .Select(x => new LeaveEntitlementHistoryResponse
                {
                    Id = x.Id,
                    EntitlementNumber = x.EntitlementNumber,
                    EntitlementYear = x.EntitlementYear,
                    PeriodStartDate = x.PeriodStartDate,
                    PeriodEndDate = x.PeriodEndDate,
                    GrantDate = x.GrantDate,
                    AvailableFromDate = x.AvailableFromDate,
                    ExpiryDate = x.ExpiryDate,
                    BaseEntitlementDays = x.BaseEntitlementDays,
                    ProratedEntitlementDays = x.ProratedEntitlementDays,
                    AdditionalEntitlementDays = x.AdditionalEntitlementDays,
                    CarryForwardEntitlementDays = x.CarryForwardEntitlementDays,
                    TotalEntitlementDays = x.TotalEntitlementDays,
                    IsProrated = x.IsProrated,
                    EntitlementStatus = x.EntitlementStatus,
                    SourceType = x.SourceType,
                    GeneratedAt = x.GeneratedAt,
                    PostedAt = x.PostedAt,
                    Notes = x.Notes
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<LeaveAccrualHistoryResponse>?> GetAccrualsAsync(
            Guid balanceId,
            Guid? scopedWorkforceProfileId = null,
            CancellationToken cancellationToken = default)
        {
            if (!await BalanceExistsAsync(balanceId, scopedWorkforceProfileId, cancellationToken))
                return null;

            return await _dbContext.Set<TrxLeaveAccrual>()
                .AsNoTracking()
                .Where(x => x.LeaveBalanceId == balanceId && !x.IsDelete)
                .OrderByDescending(x => x.AccrualDate)
                .ThenByDescending(x => x.AccrualSequence)
                .Select(x => new LeaveAccrualHistoryResponse
                {
                    Id = x.Id,
                    AccrualNumber = x.AccrualNumber,
                    LeaveAccrualRunId = x.LeaveAccrualRunId,
                    RunNumber = x.LeaveAccrualRun != null ? x.LeaveAccrualRun.RunNumber : null,
                    AccrualDate = x.AccrualDate,
                    ScheduledAccrualDate = x.ScheduledAccrualDate,
                    AccrualPeriodStartDate = x.AccrualPeriodStartDate,
                    AccrualPeriodEndDate = x.AccrualPeriodEndDate,
                    AccrualSequence = x.AccrualSequence,
                    AccrualAmountDays = x.AccrualAmountDays,
                    BalanceBeforeAccrual = x.BalanceBeforeAccrual,
                    BalanceAfterAccrual = x.BalanceAfterAccrual,
                    IsProrated = x.IsProrated,
                    AccrualStatus = x.AccrualStatus,
                    AccrualFrequency = x.AccrualFrequency,
                    SourceType = x.SourceType,
                    CalculatedAt = x.CalculatedAt,
                    PostedAt = x.PostedAt,
                    Notes = x.Notes
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<LeaveCarryForwardHistoryResponse>?> GetCarryForwardsAsync(
            Guid balanceId,
            Guid? scopedWorkforceProfileId = null,
            CancellationToken cancellationToken = default)
        {
            if (!await BalanceExistsAsync(balanceId, scopedWorkforceProfileId, cancellationToken))
                return null;

            return await _dbContext.Set<TrxLeaveCarryForward>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    (x.SourceLeaveBalanceId == balanceId || x.DestinationLeaveBalanceId == balanceId))
                .OrderByDescending(x => x.CalculationDate)
                .ThenByDescending(x => x.CreateDateTime)
                .Select(x => new LeaveCarryForwardHistoryResponse
                {
                    Id = x.Id,
                    CarryForwardNumber = x.CarryForwardNumber,
                    Direction = x.SourceLeaveBalanceId == balanceId ? "Out" : "In",
                    SourceLeaveEntitlementPeriodId = x.SourceLeaveEntitlementPeriodId,
                    SourcePeriodCode = x.SourceLeaveEntitlementPeriod != null ? x.SourceLeaveEntitlementPeriod.PeriodCode : null,
                    DestinationLeaveEntitlementPeriodId = x.DestinationLeaveEntitlementPeriodId,
                    DestinationPeriodCode = x.DestinationLeaveEntitlementPeriod != null ? x.DestinationLeaveEntitlementPeriod.PeriodCode : null,
                    SourceLeaveTypeId = x.SourceLeaveTypeId,
                    SourceLeaveTypeName = x.SourceLeaveType != null ? x.SourceLeaveType.LeaveTypeName : null,
                    DestinationLeaveTypeId = x.DestinationLeaveTypeId,
                    DestinationLeaveTypeName = x.DestinationLeaveType != null ? x.DestinationLeaveType.LeaveTypeName : null,
                    CalculationDate = x.CalculationDate,
                    CarryForwardExpiryDate = x.CarryForwardExpiryDate,
                    SourceAvailableDays = x.SourceAvailableDays,
                    EligibleDays = x.EligibleDays,
                    CarryForwardDays = x.CarryForwardDays,
                    ExpiredDays = x.ExpiredDays,
                    ExcessDays = x.ExcessDays,
                    PayoutDays = x.PayoutDays,
                    CarryForwardStatus = x.CarryForwardStatus,
                    SkipReasonCode = x.SkipReasonCode,
                    SkipReason = x.SkipReason,
                    CalculatedAt = x.CalculatedAt,
                    PostedAt = x.PostedAt,
                    Notes = x.Notes
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<LeaveAdjustmentHistoryResponse>?> GetAdjustmentsAsync(
            Guid balanceId,
            Guid? scopedWorkforceProfileId = null,
            CancellationToken cancellationToken = default)
        {
            if (!await BalanceExistsAsync(balanceId, scopedWorkforceProfileId, cancellationToken))
                return null;

            return await _dbContext.Set<TrxLeaveAdjustment>()
                .AsNoTracking()
                .Where(x => x.LeaveBalanceId == balanceId && !x.IsDelete)
                .OrderByDescending(x => x.EffectiveDate)
                .ThenByDescending(x => x.RequestedAt)
                .Select(x => new LeaveAdjustmentHistoryResponse
                {
                    Id = x.Id,
                    AdjustmentNumber = x.AdjustmentNumber,
                    LeaveAdjustmentReasonId = x.LeaveAdjustmentReasonId,
                    ReasonCode = x.LeaveAdjustmentReason != null ? x.LeaveAdjustmentReason.ReasonCode : null,
                    ReasonName = x.LeaveAdjustmentReason != null ? x.LeaveAdjustmentReason.ReasonName : null,
                    AdjustmentType = x.AdjustmentType,
                    Direction = x.Direction,
                    RequestedDays = x.RequestedDays,
                    ApprovedDays = x.ApprovedDays,
                    PostedDays = x.PostedDays,
                    EffectiveDate = x.EffectiveDate,
                    AdjustmentStatus = x.AdjustmentStatus,
                    Reason = x.Reason,
                    RequestedAt = x.RequestedAt,
                    SubmittedAt = x.SubmittedAt,
                    ApprovedAt = x.ApprovedAt,
                    PostedAt = x.PostedAt,
                    ReversedAt = x.ReversedAt,
                    RejectionReason = x.RejectionReason,
                    ReversalReason = x.ReversalReason
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<LeaveBalanceReconciliationResponse?> GetReconciliationAsync(
            Guid balanceId,
            Guid? scopedWorkforceProfileId = null,
            CancellationToken cancellationToken = default)
        {
            var balanceQuery = _dbContext.Set<WfpLeaveBalance>()
                .AsNoTracking()
                .Where(x => x.Id == balanceId && !x.IsDelete);
            if (scopedWorkforceProfileId.HasValue)
                balanceQuery = balanceQuery.Where(x => x.WorkforceProfileId == scopedWorkforceProfileId.Value);

            var balance = await balanceQuery
                .Select(x => new
                {
                    x.Id,
                    x.OpeningBalanceDays,
                    x.EntitlementDays,
                    x.AccruedDays,
                    x.CarriedForwardDays,
                    x.AdjustmentDays,
                    x.CompensatoryDays,
                    x.ReservedDays,
                    x.UsedDays,
                    x.RecalledDays,
                    x.ExpiredDays,
                    x.EncashmentDays,
                    x.RemainingDays,
                    x.AvailableDays,
                    x.LastTransactionSequence
                })
                .FirstOrDefaultAsync(cancellationToken);
            if (balance == null)
                return null;

            var transactions = await _dbContext.Set<TrxLeaveBalanceTransaction>()
                .AsNoTracking()
                .Where(x =>
                    x.LeaveBalanceId == balanceId &&
                    !x.IsDelete &&
                    x.TransactionStatus == LeaveValueConstants.TransactionStatus.Posted)
                .Select(x => new
                {
                    x.TransactionSequence,
                    x.OpeningBalanceDelta,
                    x.EntitlementDelta,
                    x.AccruedDelta,
                    x.CarryForwardDelta,
                    x.AdjustmentDelta,
                    x.CompensatoryDelta,
                    x.ReservedDelta,
                    x.UsedDelta,
                    x.ExpiredDelta,
                    x.AvailableDelta
                })
                .ToListAsync(cancellationToken);

            var expectedRemaining =
                balance.OpeningBalanceDays + balance.EntitlementDays + balance.AccruedDays +
                balance.CarriedForwardDays + balance.AdjustmentDays + balance.CompensatoryDays +
                balance.RecalledDays - balance.UsedDays - balance.ExpiredDays - balance.EncashmentDays;
            var expectedAvailable = balance.RemainingDays - balance.ReservedDays;
            var actualSequence = transactions.Count == 0 ? 0 : transactions.Max(x => x.TransactionSequence);

            var result = new LeaveBalanceReconciliationResponse
            {
                LeaveBalanceId = balance.Id,
                FormulaExpectedRemainingDays = expectedRemaining,
                FormulaActualRemainingDays = balance.RemainingDays,
                FormulaDifferenceDays = balance.RemainingDays - expectedRemaining,
                FormulaExpectedAvailableDays = expectedAvailable,
                FormulaActualAvailableDays = balance.AvailableDays,
                FormulaAvailableDifferenceDays = balance.AvailableDays - expectedAvailable,
                IsFormulaBalanced =
                    Math.Abs(balance.RemainingDays - expectedRemaining) <= ReconciliationTolerance &&
                    Math.Abs(balance.AvailableDays - expectedAvailable) <= ReconciliationTolerance,
                LastTransactionSequence = balance.LastTransactionSequence,
                ActualMaximumTransactionSequence = actualSequence,
                LedgerOpeningBalanceDays = transactions.Sum(x => x.OpeningBalanceDelta),
                LedgerEntitlementDays = transactions.Sum(x => x.EntitlementDelta),
                LedgerAccruedDays = transactions.Sum(x => x.AccruedDelta),
                LedgerCarryForwardDays = transactions.Sum(x => x.CarryForwardDelta),
                LedgerAdjustmentDays = transactions.Sum(x => x.AdjustmentDelta),
                LedgerReservedDays = transactions.Sum(x => x.ReservedDelta),
                LedgerUsedDays = transactions.Sum(x => x.UsedDelta),
                LedgerExpiredDays = transactions.Sum(x => x.ExpiredDelta),
                LedgerAvailableDays = transactions.Sum(x => x.AvailableDelta)
            };

            result.IsLedgerBalanced =
                Math.Abs(result.LedgerOpeningBalanceDays - balance.OpeningBalanceDays) <= ReconciliationTolerance &&
                Math.Abs(result.LedgerEntitlementDays - balance.EntitlementDays) <= ReconciliationTolerance &&
                Math.Abs(result.LedgerAccruedDays - balance.AccruedDays) <= ReconciliationTolerance &&
                Math.Abs(result.LedgerCarryForwardDays - balance.CarriedForwardDays) <= ReconciliationTolerance &&
                Math.Abs(result.LedgerAdjustmentDays - balance.AdjustmentDays) <= ReconciliationTolerance &&
                Math.Abs(result.LedgerReservedDays - balance.ReservedDays) <= ReconciliationTolerance &&
                Math.Abs(result.LedgerUsedDays - balance.UsedDays) <= ReconciliationTolerance &&
                Math.Abs(result.LedgerExpiredDays - balance.ExpiredDays) <= ReconciliationTolerance &&
                Math.Abs(result.LedgerAvailableDays - balance.AvailableDays) <= ReconciliationTolerance &&
                result.LastTransactionSequence == result.ActualMaximumTransactionSequence;

            if (!result.IsFormulaBalanced)
                result.Issues.Add("Saldo ringkas tidak sesuai dengan formula bucket leave balance.");
            if (Math.Abs(result.LedgerOpeningBalanceDays - balance.OpeningBalanceDays) > ReconciliationTolerance)
                result.Issues.Add("Opening balance tidak sesuai dengan total ledger.");
            if (Math.Abs(result.LedgerEntitlementDays - balance.EntitlementDays) > ReconciliationTolerance)
                result.Issues.Add("Entitlement tidak sesuai dengan total ledger.");
            if (Math.Abs(result.LedgerAccruedDays - balance.AccruedDays) > ReconciliationTolerance)
                result.Issues.Add("Accrual tidak sesuai dengan total ledger.");
            if (Math.Abs(result.LedgerCarryForwardDays - balance.CarriedForwardDays) > ReconciliationTolerance)
                result.Issues.Add("Carry forward tidak sesuai dengan total ledger.");
            if (Math.Abs(result.LedgerAdjustmentDays - balance.AdjustmentDays) > ReconciliationTolerance)
                result.Issues.Add("Adjustment tidak sesuai dengan total ledger.");
            if (result.LastTransactionSequence != result.ActualMaximumTransactionSequence)
                result.Issues.Add("LastTransactionSequence tidak sesuai dengan sequence ledger terakhir.");

            return result;
        }

        public async Task<LeaveSelfServiceSummaryResponse?> GetMySummaryAsync(
            Guid currentUserId,
            int? year,
            CancellationToken cancellationToken = default)
        {
            var workforceProfileId = await ResolveWorkforceProfileIdAsync(currentUserId, cancellationToken);
            if (!workforceProfileId.HasValue)
                return null;

            var query = _dbContext.Set<WfpLeaveBalance>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    x.WorkforceProfileId == workforceProfileId.Value);
            if (year.HasValue)
                query = query.Where(x => x.Year == year.Value);

            var cards = await query
                .OrderByDescending(x => x.Year)
                .ThenBy(x => x.LeaveType!.LeaveTypeName)
                .Select(x => new LeaveSelfServiceBalanceCardResponse
                {
                    LeaveBalanceId = x.Id,
                    LeaveTypeId = x.LeaveTypeId,
                    LeaveTypeCode = x.LeaveType != null ? x.LeaveType.LeaveTypeCode : string.Empty,
                    LeaveTypeName = x.LeaveType != null ? x.LeaveType.LeaveTypeName : string.Empty,
                    ColorCode = x.LeaveType != null ? x.LeaveType.ColorCode : null,
                    Year = x.Year,
                    PeriodName = x.LeaveEntitlementPeriod != null ? x.LeaveEntitlementPeriod.PeriodName : null,
                    RemainingDays = x.RemainingDays,
                    AvailableDays = x.AvailableDays,
                    ReservedDays = x.ReservedDays,
                    PendingDays = x.PendingDays,
                    UsedDays = x.UsedDays,
                    IsLocked = x.IsLocked,
                    BalanceStatus = x.BalanceStatus
                })
                .ToListAsync(cancellationToken);

            return new LeaveSelfServiceSummaryResponse
            {
                WorkforceProfileId = workforceProfileId.Value,
                TotalBalance = cards.Count,
                LeaveTypeCount = cards.Select(x => x.LeaveTypeId).Distinct().Count(),
                ActiveBalance = cards.Count(x => x.BalanceStatus == LeaveValueConstants.BalanceStatus.Active),
                LockedBalance = cards.Count(x => x.IsLocked || x.BalanceStatus == LeaveValueConstants.BalanceStatus.Locked),
                TotalRemainingDays = cards.Sum(x => x.RemainingDays),
                TotalAvailableDays = cards.Sum(x => x.AvailableDays),
                TotalReservedDays = cards.Sum(x => x.ReservedDays),
                TotalPendingDays = cards.Sum(x => x.PendingDays),
                TotalUsedDays = cards.Sum(x => x.UsedDays),
                BalanceCards = cards
            };
        }

        public async Task<LeaveBalancePagedResponse?> GetMyBalancesAsync(
            Guid currentUserId,
            LeaveBalanceQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var workforceProfileId = await ResolveWorkforceProfileIdAsync(currentUserId, cancellationToken);
            if (!workforceProfileId.HasValue)
                return null;

            request.WorkforceProfileId = workforceProfileId.Value;
            return await GetBalancePagedAsync(request, cancellationToken);
        }

        public async Task<LeaveBalanceDetailResponse?> GetMyBalanceDetailAsync(
            Guid currentUserId,
            Guid balanceId,
            CancellationToken cancellationToken = default)
        {
            var workforceProfileId = await ResolveWorkforceProfileIdAsync(currentUserId, cancellationToken);
            if (!workforceProfileId.HasValue)
                return null;

            return await GetBalanceDetailAsync(balanceId, workforceProfileId.Value, cancellationToken);
        }

        public async Task<LeaveBalanceTransactionPagedResponse?> GetMyLedgerAsync(
            Guid currentUserId,
            Guid balanceId,
            LeaveBalanceLedgerQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var workforceProfileId = await ResolveWorkforceProfileIdAsync(currentUserId, cancellationToken);
            if (!workforceProfileId.HasValue)
                return null;

            return await GetLedgerAsync(balanceId, request, workforceProfileId.Value, cancellationToken);
        }

        private IQueryable<TrxLeaveEntitlementPeriod> BuildPeriodQuery(
            LeaveEntitlementPeriodQueryRequest request)
        {
            var query = _dbContext.Set<TrxLeaveEntitlementPeriod>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
            var range = ResolvePeriodDateRange(request.StartDate, request.EndDate, request.CustomPeriod);

            if (range.Start.HasValue)
                query = query.Where(x => x.EndDate >= range.Start.Value);
            if (range.End.HasValue)
                query = query.Where(x => x.StartDate <= range.End.Value);
            if (request.LeaveTypeId.HasValue && request.LeaveTypeId.Value != Guid.Empty)
                query = query.Where(x => x.LeaveTypeId == request.LeaveTypeId.Value);
            if (request.LegalEntityId.HasValue && request.LegalEntityId.Value != Guid.Empty)
                query = query.Where(x => x.LegalEntityId == request.LegalEntityId.Value);
            if (request.HospitalSiteId.HasValue && request.HospitalSiteId.Value != Guid.Empty)
                query = query.Where(x => x.HospitalSiteId == request.HospitalSiteId.Value);
            if (request.OrganizationUnitId.HasValue && request.OrganizationUnitId.Value != Guid.Empty)
                query = query.Where(x => x.OrganizationUnitId == request.OrganizationUnitId.Value);
            if (request.DepartmentId.HasValue && request.DepartmentId.Value != Guid.Empty)
                query = query.Where(x => x.DepartmentId == request.DepartmentId.Value);
            if (request.PeriodYear.HasValue)
                query = query.Where(x => x.PeriodYear == request.PeriodYear.Value);
            if (!string.IsNullOrWhiteSpace(request.PeriodBasis))
                query = query.Where(x => x.PeriodBasis == request.PeriodBasis.Trim());
            if (!string.IsNullOrWhiteSpace(request.PeriodStatus))
                query = query.Where(x => x.PeriodStatus == request.PeriodStatus.Trim());
            if (request.IsLocked.HasValue)
                query = query.Where(x => x.IsLocked == request.IsLocked.Value);
            if (request.IsActive.HasValue)
                query = query.Where(x => x.IsActive == request.IsActive.Value);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim().ToLower();
                query = query.Where(x =>
                    x.PeriodCode.ToLower().Contains(keyword) ||
                    x.PeriodName.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.LeaveType != null && x.LeaveType.LeaveTypeName.ToLower().Contains(keyword)));
            }

            return query;
        }

        private IQueryable<WfpLeaveBalance> BuildBalanceQuery(LeaveBalanceQueryRequest request)
        {
            var query = _dbContext.Set<WfpLeaveBalance>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (request.LeaveEntitlementPeriodId.HasValue && request.LeaveEntitlementPeriodId.Value != Guid.Empty)
                query = query.Where(x => x.LeaveEntitlementPeriodId == request.LeaveEntitlementPeriodId.Value);
            if (request.WorkforceProfileId.HasValue && request.WorkforceProfileId.Value != Guid.Empty)
                query = query.Where(x => x.WorkforceProfileId == request.WorkforceProfileId.Value);
            if (request.LeaveTypeId.HasValue && request.LeaveTypeId.Value != Guid.Empty)
                query = query.Where(x => x.LeaveTypeId == request.LeaveTypeId.Value);
            if (request.Year.HasValue)
                query = query.Where(x => x.Year == request.Year.Value);
            if (!string.IsNullOrWhiteSpace(request.BalanceStatus))
                query = query.Where(x => x.BalanceStatus == request.BalanceStatus.Trim());
            if (request.IsLocked.HasValue)
                query = query.Where(x => x.IsLocked == request.IsLocked.Value);
            if (request.IsActive.HasValue)
                query = query.Where(x => x.IsActive == request.IsActive.Value);
            if (request.HasAvailableBalance.HasValue)
                query = request.HasAvailableBalance.Value
                    ? query.Where(x => x.AvailableDays > 0)
                    : query.Where(x => x.AvailableDays <= 0);
            if (request.HasReservedBalance.HasValue)
                query = request.HasReservedBalance.Value
                    ? query.Where(x => x.ReservedDays > 0)
                    : query.Where(x => x.ReservedDays <= 0);
            if (request.HasExpiredBalance.HasValue)
                query = request.HasExpiredBalance.Value
                    ? query.Where(x => x.ExpiredDays > 0)
                    : query.Where(x => x.ExpiredDays <= 0);

            if (request.DepartmentId.HasValue && request.DepartmentId.Value != Guid.Empty)
                query = query.Where(x => x.WorkforceProfile != null && x.WorkforceProfile.PrimaryDepartmentId == request.DepartmentId.Value);
            if (request.PositionId.HasValue && request.PositionId.Value != Guid.Empty)
                query = query.Where(x => x.WorkforceProfile != null && x.WorkforceProfile.PrimaryPositionId == request.PositionId.Value);

            var now = DateTime.UtcNow;
            if (request.LegalEntityId.HasValue && request.LegalEntityId.Value != Guid.Empty)
                query = query.Where(x => _dbContext.Set<WfpOrganizationAssignment>().Any(a =>
                    !a.IsDelete && a.IsActive && a.WorkforceProfileId == x.WorkforceProfileId &&
                    a.LegalEntityId == request.LegalEntityId.Value &&
                    a.EffectiveStartDate <= now && (!a.EffectiveEndDate.HasValue || a.EffectiveEndDate.Value >= now)));
            if (request.HospitalSiteId.HasValue && request.HospitalSiteId.Value != Guid.Empty)
                query = query.Where(x => _dbContext.Set<WfpOrganizationAssignment>().Any(a =>
                    !a.IsDelete && a.IsActive && a.WorkforceProfileId == x.WorkforceProfileId &&
                    a.HospitalSiteId == request.HospitalSiteId.Value &&
                    a.EffectiveStartDate <= now && (!a.EffectiveEndDate.HasValue || a.EffectiveEndDate.Value >= now)));
            if (request.OrganizationUnitId.HasValue && request.OrganizationUnitId.Value != Guid.Empty)
                query = query.Where(x => _dbContext.Set<WfpOrganizationAssignment>().Any(a =>
                    !a.IsDelete && a.IsActive && a.WorkforceProfileId == x.WorkforceProfileId &&
                    a.OrganizationUnitId == request.OrganizationUnitId.Value &&
                    a.EffectiveStartDate <= now && (!a.EffectiveEndDate.HasValue || a.EffectiveEndDate.Value >= now)));

            if (!string.IsNullOrWhiteSpace(request.ReconciliationStatus))
            {
                if (string.Equals(request.ReconciliationStatus, "balanced", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(x =>
                        x.RemainingDays ==
                            x.OpeningBalanceDays + x.EntitlementDays + x.AccruedDays +
                            x.CarriedForwardDays + x.AdjustmentDays + x.CompensatoryDays +
                            x.RecalledDays - x.UsedDays - x.ExpiredDays - x.EncashmentDays &&
                        x.AvailableDays == x.RemainingDays - x.ReservedDays);
                }
                else if (string.Equals(request.ReconciliationStatus, "mismatch", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(x =>
                        x.RemainingDays !=
                            x.OpeningBalanceDays + x.EntitlementDays + x.AccruedDays +
                            x.CarriedForwardDays + x.AdjustmentDays + x.CompensatoryDays +
                            x.RecalledDays - x.UsedDays - x.ExpiredDays - x.EncashmentDays ||
                        x.AvailableDays != x.RemainingDays - x.ReservedDays);
                }
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim().ToLower();
                query = query.Where(x =>
                    (x.WorkforceProfile != null &&
                        (x.WorkforceProfile.ProfileCode.ToLower().Contains(keyword) ||
                         x.WorkforceProfile.DisplayName.ToLower().Contains(keyword) ||
                         (x.WorkforceProfile.Employee != null && x.WorkforceProfile.Employee.EmployeeNumber.ToLower().Contains(keyword)))) ||
                    (x.LeaveType != null &&
                        (x.LeaveType.LeaveTypeCode.ToLower().Contains(keyword) ||
                         x.LeaveType.LeaveTypeName.ToLower().Contains(keyword))) ||
                    (x.LeaveEntitlementPeriod != null &&
                        (x.LeaveEntitlementPeriod.PeriodCode.ToLower().Contains(keyword) ||
                         x.LeaveEntitlementPeriod.PeriodName.ToLower().Contains(keyword))));
            }

            return query;
        }

        private IQueryable<LeaveEntitlementPeriodResponse> ProjectPeriods(IQueryable<TrxLeaveEntitlementPeriod> query)
        {
            return query.Select(x => new LeaveEntitlementPeriodResponse
            {
                Id = x.Id,
                LeaveTypeId = x.LeaveTypeId,
                LeaveTypeCode = x.LeaveType != null ? x.LeaveType.LeaveTypeCode : null,
                LeaveTypeName = x.LeaveType != null ? x.LeaveType.LeaveTypeName : null,
                LegalEntityId = x.LegalEntityId,
                LegalEntityName = x.LegalEntity != null ? x.LegalEntity.LegalEntityName : null,
                HospitalSiteId = x.HospitalSiteId,
                HospitalSiteName = x.HospitalSite != null ? x.HospitalSite.SiteName : null,
                OrganizationUnitId = x.OrganizationUnitId,
                OrganizationUnitName = x.OrganizationUnit != null ? x.OrganizationUnit.UnitName : null,
                DepartmentId = x.DepartmentId,
                DepartmentName = x.Department != null ? x.Department.DepartmentName : null,
                PeriodCode = x.PeriodCode,
                PeriodName = x.PeriodName,
                PeriodBasis = x.PeriodBasis,
                PeriodYear = x.PeriodYear,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                PeriodStatus = x.PeriodStatus,
                IsLocked = x.IsLocked,
                IsActive = x.IsActive,
                LeaveBalanceCount = x.LeaveBalances.Count(b => !b.IsDelete),
                WorkforceCount = x.LeaveBalances.Where(b => !b.IsDelete).Select(b => b.WorkforceProfileId).Distinct().Count(),
                EntitlementCount = x.Entitlements.Count(e => !e.IsDelete),
                AccrualRunCount = x.AccrualRuns.Count(r => !r.IsDelete),
                SourceCarryForwardRunCount = x.SourceCarryForwardRuns.Count(r => !r.IsDelete),
                DestinationCarryForwardRunCount = x.DestinationCarryForwardRuns.Count(r => !r.IsDelete),
                AdjustmentCount = x.Adjustments.Count(a => !a.IsDelete),
                TotalRemainingDays = x.LeaveBalances.Where(b => !b.IsDelete).Sum(b => (decimal?)b.RemainingDays) ?? 0,
                TotalAvailableDays = x.LeaveBalances.Where(b => !b.IsDelete).Sum(b => (decimal?)b.AvailableDays) ?? 0,
                CreateDateTime = x.CreateDateTime
            });
        }

        private IQueryable<LeaveBalanceResponse> ProjectBalances(IQueryable<WfpLeaveBalance> query)
        {
            return query.Select(x => new LeaveBalanceResponse
            {
                Id = x.Id,
                WorkforceProfileId = x.WorkforceProfileId,
                ProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : null,
                WorkforceDisplayName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty,
                EmployeeNumber = x.WorkforceProfile != null && x.WorkforceProfile.Employee != null ? x.WorkforceProfile.Employee.EmployeeNumber : null,
                DepartmentId = x.WorkforceProfile != null ? x.WorkforceProfile.PrimaryDepartmentId : null,
                DepartmentName = x.WorkforceProfile != null && x.WorkforceProfile.PrimaryDepartment != null ? x.WorkforceProfile.PrimaryDepartment.DepartmentName : null,
                PositionId = x.WorkforceProfile != null ? x.WorkforceProfile.PrimaryPositionId : null,
                PositionName = x.WorkforceProfile != null && x.WorkforceProfile.PrimaryPosition != null ? x.WorkforceProfile.PrimaryPosition.PositionName : null,
                LeaveTypeId = x.LeaveTypeId,
                LeaveTypeCode = x.LeaveType != null ? x.LeaveType.LeaveTypeCode : string.Empty,
                LeaveTypeName = x.LeaveType != null ? x.LeaveType.LeaveTypeName : string.Empty,
                LeaveCategory = x.LeaveType != null ? x.LeaveType.LeaveCategory : string.Empty,
                LeaveEntitlementPeriodId = x.LeaveEntitlementPeriodId,
                PeriodCode = x.LeaveEntitlementPeriod != null ? x.LeaveEntitlementPeriod.PeriodCode : null,
                PeriodName = x.LeaveEntitlementPeriod != null ? x.LeaveEntitlementPeriod.PeriodName : null,
                Year = x.Year,
                PeriodStartDate = x.PeriodStartDate,
                PeriodEndDate = x.PeriodEndDate,
                OpeningBalanceDays = x.OpeningBalanceDays,
                EntitlementDays = x.EntitlementDays,
                AccruedDays = x.AccruedDays,
                CarriedForwardDays = x.CarriedForwardDays,
                AdjustmentDays = x.AdjustmentDays,
                CompensatoryDays = x.CompensatoryDays,
                ReservedDays = x.ReservedDays,
                PendingDays = x.PendingDays,
                UsedDays = x.UsedDays,
                RecalledDays = x.RecalledDays,
                ExpiredDays = x.ExpiredDays,
                EncashmentDays = x.EncashmentDays,
                RemainingDays = x.RemainingDays,
                AvailableDays = x.AvailableDays,
                BalanceStatus = x.BalanceStatus,
                IsLocked = x.IsLocked,
                IsActive = x.IsActive,
                BalanceVersion = x.BalanceVersion,
                LastTransactionSequence = x.LastTransactionSequence,
                LastCalculatedAt = x.LastCalculatedAt,
                LastReconciledAt = x.LastReconciledAt,
                CarryForwardExpiryDate = x.CarryForwardExpiryDate,
                CreateDateTime = x.CreateDateTime
            });
        }

        private static IOrderedQueryable<TrxLeaveEntitlementPeriod> ApplyPeriodSorting(
            IQueryable<TrxLeaveEntitlementPeriod> query,
            string? sortBy,
            string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "startDate").Trim().ToLowerInvariant() switch
            {
                "enddate" => desc ? query.OrderByDescending(x => x.EndDate) : query.OrderBy(x => x.EndDate),
                "periodcode" => desc ? query.OrderByDescending(x => x.PeriodCode) : query.OrderBy(x => x.PeriodCode),
                "periodname" => desc ? query.OrderByDescending(x => x.PeriodName) : query.OrderBy(x => x.PeriodName),
                "periodyear" => desc ? query.OrderByDescending(x => x.PeriodYear) : query.OrderBy(x => x.PeriodYear),
                "periodstatus" => desc ? query.OrderByDescending(x => x.PeriodStatus) : query.OrderBy(x => x.PeriodStatus),
                "balancecount" => desc ? query.OrderByDescending(x => x.LeaveBalances.Count(b => !b.IsDelete)) : query.OrderBy(x => x.LeaveBalances.Count(b => !b.IsDelete)),
                "availabledays" => desc ? query.OrderByDescending(x => x.LeaveBalances.Where(b => !b.IsDelete).Sum(b => (decimal?)b.AvailableDays) ?? 0) : query.OrderBy(x => x.LeaveBalances.Where(b => !b.IsDelete).Sum(b => (decimal?)b.AvailableDays) ?? 0),
                _ => desc ? query.OrderByDescending(x => x.StartDate).ThenByDescending(x => x.PeriodCode) : query.OrderBy(x => x.StartDate).ThenBy(x => x.PeriodCode)
            };
        }

        private static IOrderedQueryable<WfpLeaveBalance> ApplyBalanceSorting(
            IQueryable<WfpLeaveBalance> query,
            string? sortBy,
            string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "workforceDisplayName").Trim().ToLowerInvariant() switch
            {
                "profilecode" => desc ? query.OrderByDescending(x => x.WorkforceProfile!.ProfileCode) : query.OrderBy(x => x.WorkforceProfile!.ProfileCode),
                "leavetypename" => desc ? query.OrderByDescending(x => x.LeaveType!.LeaveTypeName) : query.OrderBy(x => x.LeaveType!.LeaveTypeName),
                "year" => desc ? query.OrderByDescending(x => x.Year) : query.OrderBy(x => x.Year),
                "availabledays" => desc ? query.OrderByDescending(x => x.AvailableDays) : query.OrderBy(x => x.AvailableDays),
                "remainingdays" => desc ? query.OrderByDescending(x => x.RemainingDays) : query.OrderBy(x => x.RemainingDays),
                "reserveddays" => desc ? query.OrderByDescending(x => x.ReservedDays) : query.OrderBy(x => x.ReservedDays),
                "useddays" => desc ? query.OrderByDescending(x => x.UsedDays) : query.OrderBy(x => x.UsedDays),
                "balancestatus" => desc ? query.OrderByDescending(x => x.BalanceStatus) : query.OrderBy(x => x.BalanceStatus),
                "lastcalculatedat" => desc ? query.OrderByDescending(x => x.LastCalculatedAt) : query.OrderBy(x => x.LastCalculatedAt),
                _ => desc
                    ? query.OrderByDescending(x => x.WorkforceProfile!.DisplayName).ThenByDescending(x => x.LeaveType!.LeaveTypeName)
                    : query.OrderBy(x => x.WorkforceProfile!.DisplayName).ThenBy(x => x.LeaveType!.LeaveTypeName)
            };
        }

        private async Task<bool> BalanceExistsAsync(
            Guid balanceId,
            Guid? scopedWorkforceProfileId,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.Set<WfpLeaveBalance>()
                .AsNoTracking()
                .Where(x => x.Id == balanceId && !x.IsDelete);
            if (scopedWorkforceProfileId.HasValue)
                query = query.Where(x => x.WorkforceProfileId == scopedWorkforceProfileId.Value);
            return await query.AnyAsync(cancellationToken);
        }

        private async Task<Guid?> ResolveWorkforceProfileIdAsync(
            Guid currentUserId,
            CancellationToken cancellationToken)
        {
            if (currentUserId == Guid.Empty)
                return null;

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => x.Id == currentUserId)
                .Select(x => x.WorkforceProfileId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private static void CompleteFormulaState(IEnumerable<LeaveBalanceResponse> items)
        {
            foreach (var item in items)
            {
                var expectedRemaining =
                    item.OpeningBalanceDays + item.EntitlementDays + item.AccruedDays +
                    item.CarriedForwardDays + item.AdjustmentDays + item.CompensatoryDays +
                    item.RecalledDays - item.UsedDays - item.ExpiredDays - item.EncashmentDays;
                var expectedAvailable = item.RemainingDays - item.ReservedDays;
                var remainingDifference = item.RemainingDays - expectedRemaining;
                var availableDifference = item.AvailableDays - expectedAvailable;
                item.FormulaDifferenceDays = Math.Abs(remainingDifference) + Math.Abs(availableDifference);
                item.IsFormulaBalanced =
                    Math.Abs(remainingDifference) <= ReconciliationTolerance &&
                    Math.Abs(availableDifference) <= ReconciliationTolerance;
            }
        }

        private static (DateOnly? Start, DateOnly? End) ResolvePeriodDateRange(
            DateOnly? startDate,
            DateOnly? endDate,
            string? customPeriod)
        {
            if (startDate.HasValue || endDate.HasValue)
                return (startDate, endDate);

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            return customPeriod?.Trim().ToLowerInvariant() switch
            {
                "currentyear" => (new DateOnly(today.Year, 1, 1), new DateOnly(today.Year, 12, 31)),
                "nextyear" => (new DateOnly(today.Year + 1, 1, 1), new DateOnly(today.Year + 1, 12, 31)),
                "previousyear" => (new DateOnly(today.Year - 1, 1, 1), new DateOnly(today.Year - 1, 12, 31)),
                "active" => (today, today),
                _ => (null, null)
            };
        }

        private static List<LeaveQueryCustomPeriodResponse> BuildCustomPeriods()
        {
            return new List<LeaveQueryCustomPeriodResponse>
            {
                new() { Value = "active", Label = "Periode yang mencakup hari ini" },
                new() { Value = "currentyear", Label = "Tahun berjalan" },
                new() { Value = "nextyear", Label = "Tahun berikutnya" },
                new() { Value = "previousyear", Label = "Tahun sebelumnya" }
            };
        }
    }
}
