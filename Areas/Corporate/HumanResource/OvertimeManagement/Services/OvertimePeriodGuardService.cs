using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public class OvertimePeriodGuardResult
    {
        public bool IsWritable { get; set; }
        public Guid? OvertimePeriodId { get; set; }
        public string? PeriodCode { get; set; }
        public string? PeriodStatus { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class OvertimePeriodGuardService
    {
        private readonly ApplicationDbContext _dbContext;

        public OvertimePeriodGuardService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<OvertimePeriodGuardResult> CheckDateAsync(
            DateOnly date,
            Guid? legalEntityId,
            Guid? hospitalSiteId,
            Guid? organizationUnitId,
            Guid? departmentId,
            CancellationToken cancellationToken = default)
        {
            var resolvedLegalEntityId = NormalizeGuid(legalEntityId);
            var siteId = NormalizeGuid(hospitalSiteId);
            var unitId = NormalizeGuid(organizationUnitId);
            var deptId = NormalizeGuid(departmentId);

            if (!resolvedLegalEntityId.HasValue && siteId.HasValue)
            {
                resolvedLegalEntityId = await _dbContext.MstHospitalSites
                    .AsNoTracking()
                    .Where(x => x.Id == siteId.Value && !x.IsDelete)
                    .Select(x => (Guid?)x.LegalEntityId)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            var periods = await _dbContext.Set<TrxOvertimePeriod>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive &&
                    x.StartDate <= date &&
                    x.EndDate >= date &&
                    (!x.LegalEntityId.HasValue || x.LegalEntityId == resolvedLegalEntityId) &&
                    (!x.HospitalSiteId.HasValue || x.HospitalSiteId == siteId) &&
                    (!x.OrganizationUnitId.HasValue || x.OrganizationUnitId == unitId) &&
                    (!x.DepartmentId.HasValue || x.DepartmentId == deptId))
                .ToListAsync(cancellationToken);

            var period = periods
                .OrderByDescending(GetSpecificity)
                .ThenByDescending(x => x.StartDate)
                .FirstOrDefault();

            if (period == null)
            {
                return new OvertimePeriodGuardResult
                {
                    IsWritable = true,
                    Message = "Tidak ada overtime period yang membatasi tanggal tersebut."
                };
            }

            var blocked = string.Equals(period.PeriodStatus, OvertimeValueConstants.PeriodStatus.Closing, StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(period.PeriodStatus, OvertimeValueConstants.PeriodStatus.Closed, StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(period.PeriodStatus, OvertimeValueConstants.PeriodStatus.Cancelled, StringComparison.OrdinalIgnoreCase);

            return new OvertimePeriodGuardResult
            {
                IsWritable = !blocked,
                OvertimePeriodId = period.Id,
                PeriodCode = period.PeriodCode,
                PeriodStatus = period.PeriodStatus,
                Message = blocked
                    ? $"Overtime period {period.PeriodCode} berstatus {period.PeriodStatus}. Reopen period sebelum mengubah transaksi pada tanggal tersebut."
                    : $"Overtime period {period.PeriodCode} masih dapat ditulis."
            };
        }

        public async Task<OvertimePeriodGuardResult> CheckRangeAsync(
            DateOnly startDate,
            DateOnly endDate,
            Guid? legalEntityId,
            Guid? hospitalSiteId,
            Guid? organizationUnitId,
            Guid? departmentId,
            CancellationToken cancellationToken = default)
        {
            if (endDate < startDate)
            {
                return new OvertimePeriodGuardResult
                {
                    IsWritable = false,
                    Message = "Tanggal akhir tidak boleh lebih kecil dari tanggal mulai."
                };
            }

            var resolvedLegalEntityId = NormalizeGuid(legalEntityId);
            var siteId = NormalizeGuid(hospitalSiteId);
            var unitId = NormalizeGuid(organizationUnitId);
            var deptId = NormalizeGuid(departmentId);

            if (!resolvedLegalEntityId.HasValue && siteId.HasValue)
            {
                resolvedLegalEntityId = await _dbContext.MstHospitalSites
                    .AsNoTracking()
                    .Where(x => x.Id == siteId.Value && !x.IsDelete)
                    .Select(x => (Guid?)x.LegalEntityId)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            var periods = await _dbContext.Set<TrxOvertimePeriod>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive &&
                    x.StartDate <= endDate &&
                    x.EndDate >= startDate &&
                    (!x.LegalEntityId.HasValue || x.LegalEntityId == resolvedLegalEntityId) &&
                    (!x.HospitalSiteId.HasValue || x.HospitalSiteId == siteId) &&
                    (!x.OrganizationUnitId.HasValue || x.OrganizationUnitId == unitId) &&
                    (!x.DepartmentId.HasValue || x.DepartmentId == deptId))
                .ToListAsync(cancellationToken);

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var resolvedPeriod = periods
                    .Where(x => x.StartDate <= date && x.EndDate >= date)
                    .OrderByDescending(GetSpecificity)
                    .ThenByDescending(x => x.StartDate)
                    .FirstOrDefault();

                if (resolvedPeriod == null) continue;

                var blocked =
                    string.Equals(resolvedPeriod.PeriodStatus, OvertimeValueConstants.PeriodStatus.Closing, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(resolvedPeriod.PeriodStatus, OvertimeValueConstants.PeriodStatus.Closed, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(resolvedPeriod.PeriodStatus, OvertimeValueConstants.PeriodStatus.Cancelled, StringComparison.OrdinalIgnoreCase);
                if (!blocked) continue;

                return new OvertimePeriodGuardResult
                {
                    IsWritable = false,
                    OvertimePeriodId = resolvedPeriod.Id,
                    PeriodCode = resolvedPeriod.PeriodCode,
                    PeriodStatus = resolvedPeriod.PeriodStatus,
                    Message = $"Overtime period {resolvedPeriod.PeriodCode} berstatus {resolvedPeriod.PeriodStatus}. Reopen period sebelum mengubah transaksi pada tanggal {date:yyyy-MM-dd}."
                };
            }

            return new OvertimePeriodGuardResult
            {
                IsWritable = true,
                Message = "Seluruh rentang tanggal masih dapat ditulis."
            };
        }

        private static int GetSpecificity(TrxOvertimePeriod value) =>
            (value.LegalEntityId.HasValue ? 1 : 0) +
            (value.HospitalSiteId.HasValue ? 2 : 0) +
            (value.OrganizationUnitId.HasValue ? 4 : 0) +
            (value.DepartmentId.HasValue ? 8 : 0);

        private static Guid? NormalizeGuid(Guid? value) =>
            value.HasValue && value.Value != Guid.Empty ? value.Value : null;
    }
}
