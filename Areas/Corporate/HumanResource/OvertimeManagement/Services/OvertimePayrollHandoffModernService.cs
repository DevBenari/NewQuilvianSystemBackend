using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    /// <summary>
    /// Layanan paket serah terima data lembur terkunci dan rapel retrospektif ke Modul 10 Penggajian (Payroll) (BE-HRD-144, INT-HR-48).
    /// </summary>
    public class OvertimePayrollHandoffModernService : IOvertimePayrollHandoffModernService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OvertimePayrollHandoffModernService> _logger;

        public OvertimePayrollHandoffModernService(
            ApplicationDbContext context,
            ILogger<OvertimePayrollHandoffModernService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OvertimePayrollHandoffPackageDto> PreviewHandoffPackageAsync(
            Guid periodId,
            CancellationToken cancellationToken = default)
        {
            var period = await _context.Set<TrxOvertimePeriod>()
                .FirstOrDefaultAsync(x => x.Id == periodId && !x.IsDelete, cancellationToken);

            if (period == null)
            {
                throw new KeyNotFoundException($"Periode lembur dengan ID {periodId} tidak ditemukan.");
            }

            var cutoff = period.CutoffDate ?? period.EndDate;

            // Kueri seluruh klaim finansial terverifikasi untuk periode ini + retrospektif (INT-HR-48)
            var realizations = await _context.Set<TrxOvertimeRealization>()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.Department)
                .Where(x => !x.IsDelete &&
                            x.CompensationType == "FinancialPayout" &&
                            x.RealizationStatus == "Verified" &&
                            ((x.ActualStartDate >= period.StartDate && x.ActualStartDate <= cutoff) || x.IsRetrospectiveAdjustment))
                .ToListAsync(cancellationToken);

            var groupedByEmployee = realizations
                .GroupBy(x => x.WorkforceProfileId)
                .Select(g =>
                {
                    var first = g.First();
                    var regularMinutes = g.Where(r => !r.IsRetrospectiveAdjustment).Sum(r => r.VerifiedMinutes);
                    var retroMinutes = g.Where(r => r.IsRetrospectiveAdjustment).Sum(r => r.VerifiedMinutes);
                    var totalMinutes = g.Sum(r => r.VerifiedMinutes);

                    return new OvertimePayrollEmployeeItemDto
                    {
                        WorkforceProfileId = g.Key,
                        EmployeeName = first.WorkforceProfile != null ? first.WorkforceProfile.DisplayName : string.Empty,
                        DepartmentName = first.Department != null ? first.Department.DepartmentName : string.Empty,
                        RegularMinutes = regularMinutes,
                        HolidayMinutes = 0,
                        RetrospectiveMinutes = retroMinutes,
                        TotalHours = Math.Round((decimal)totalMinutes / 60m, 2),
                        RealizationCount = g.Count()
                    };
                })
                .ToList();

            var totalRegMinutes = realizations.Where(r => !r.IsRetrospectiveAdjustment).Sum(r => r.VerifiedMinutes);
            var totalRetroMinutes = realizations.Where(r => r.IsRetrospectiveAdjustment).Sum(r => r.VerifiedMinutes);
            var totalAllMinutes = realizations.Sum(r => r.VerifiedMinutes);

            var batchNo = $"OT-HANDOFF-{period.PeriodCode}-{DateTime.UtcNow:yyyyMMddHHmmss}";

            return new OvertimePayrollHandoffPackageDto
            {
                PeriodId = period.Id,
                PeriodCode = period.PeriodCode,
                PeriodName = period.PeriodName,
                CutoffDate = cutoff,
                HandoffTimestamp = DateTime.UtcNow,
                TotalEmployeesCount = groupedByEmployee.Count,
                TotalRealizationRecords = realizations.Count,
                RegularOvertimeMinutes = totalRegMinutes,
                HolidayOvertimeMinutes = 0,
                TotalOvertimeHours = Math.Round((decimal)totalAllMinutes / 60m, 2),
                RetrospectiveRecordsCount = realizations.Count(r => r.IsRetrospectiveAdjustment),
                RetrospectiveHours = Math.Round((decimal)totalRetroMinutes / 60m, 2),
                HandoffBatchNumber = batchNo,
                EmployeeSummaries = groupedByEmployee
            };
        }

        public async Task<OvertimePlanningServiceResult<OvertimePayrollHandoffPackageDto>> ExecuteHandoffAsync(
            Guid periodId,
            ExecutePayrollHandoffCommand command,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var period = await _context.Set<TrxOvertimePeriod>()
                .FirstOrDefaultAsync(x => x.Id == periodId && !x.IsDelete, cancellationToken);

            if (period == null)
            {
                return OvertimePlanningServiceResult<OvertimePayrollHandoffPackageDto>.Fail(
                    StatusCodes.Status404NotFound, "Periode lembur tidak ditemukan.");
            }

            if (period.PeriodStatus != "Locked" && period.PeriodStatus != "Closed")
            {
                return OvertimePlanningServiceResult<OvertimePayrollHandoffPackageDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Paket serah terima Payroll hanya dapat dieksekusi setelah periode berstatus Locked atau Closed. Status saat ini: {period.PeriodStatus}.");
            }

            if (period.IsHandoffExecuted)
            {
                return OvertimePlanningServiceResult<OvertimePayrollHandoffPackageDto>.Fail(
                    StatusCodes.Status409Conflict,
                    "Serah terima lembur ke Modul Penggajian (Payroll Handoff) sudah pernah dieksekusi sebelumnya untuk periode ini.");
            }

            var cutoff = period.CutoffDate ?? period.EndDate;
            var now = DateTime.UtcNow;

            var realizations = await _context.Set<TrxOvertimeRealization>()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.Department)
                .Where(x => !x.IsDelete &&
                            x.CompensationType == "FinancialPayout" &&
                            x.RealizationStatus == "Verified" &&
                            ((x.ActualStartDate >= period.StartDate && x.ActualStartDate <= cutoff) || x.IsRetrospectiveAdjustment))
                .ToListAsync(cancellationToken);

            foreach (var r in realizations)
            {
                r.IsPayrollPosted = true;
                r.PostedToPayrollAt = now;
                r.PostedToPayrollByUserId = actorUserId;
                r.RealizationStatus = "PostedToPayroll";
                r.PostedMinutes = r.VerifiedMinutes;
                r.UpdateBy = actorUserId;
                r.UpdateDateTime = now;
            }

            period.IsHandoffExecuted = true;
            period.UpdateBy = actorUserId;
            period.UpdateDateTime = now;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Payroll Handoff untuk periode {PeriodCode} tuntas dieksekusi oleh {UserId}. {Count} realisasi diposting ke payroll.",
                period.PeriodCode, actorUserId, realizations.Count);

            var preview = await PreviewHandoffPackageAsync(periodId, cancellationToken);

            return OvertimePlanningServiceResult<OvertimePayrollHandoffPackageDto>.Ok(
                preview,
                $"Serah terima lembur periode {period.PeriodCode} ke Modul 10 Penggajian berhasil dieksekusi ({realizations.Count} berkas klaim, total {preview.TotalOvertimeHours} jam lembur berbayar).",
                StatusCodes.Status200OK);
        }
    }
}
