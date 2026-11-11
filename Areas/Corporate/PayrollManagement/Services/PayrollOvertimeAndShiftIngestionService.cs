using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    public class PayrollOvertimeAndShiftIngestionService : IPayrollOvertimeAndShiftIngestionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PayrollOvertimeAndShiftIngestionService> _logger;
        private const decimal DefaultNightShiftRate = 50000m; // Rp 50.000 per dinas shift malam

        public PayrollOvertimeAndShiftIngestionService(
            ApplicationDbContext context,
            ILogger<PayrollOvertimeAndShiftIngestionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PayrollServiceResult<IngestOvertimeAndShiftResultDto>> IngestOvertimeAndShiftAsync(
            Guid runId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Memulai penyerapan data lembur Sub-09 dan premi shift malam Sub-07 untuk batch penggajian ID: {RunId}. Actor: {ActorId}",
                runId, actorUserId);

            var run = await _context.PrlPayrollRuns
                .Include(r => r.PayrollPeriod)
                .Include(r => r.Employees.Where(e => e.IsActive))
                    .ThenInclude(e => e.Components.Where(c => c.IsActive))
                .FirstOrDefaultAsync(r => r.Id == runId && r.IsActive, cancellationToken);

            if (run == null)
            {
                return PayrollServiceResult<IngestOvertimeAndShiftResultDto>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Batch penggajian dengan ID {runId} tidak ditemukan.");
            }

            if (run.IsLocked || run.RunStatus is "Approved" or "PaymentProcessing" or "Paid" or "Posted" or "Closed")
            {
                return PayrollServiceResult<IngestOvertimeAndShiftResultDto>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Batch penggajian {run.RunNumber} telah berstatus '{run.RunStatus}' (Locked={run.IsLocked}) dan tidak dapat dimodifikasi.");
            }

            var startDate = run.PeriodStartDateSnapshot;
            var endDate = run.PeriodEndDateSnapshot;
            var startDateOnly = DateOnly.FromDateTime(startDate);
            var endDateOnly = DateOnly.FromDateTime(endDate);

            // 1. Ambil data realisasi lembur sah berbayar uang (FinancialPayout) dari Sub-modul 09
            var overtimeRealizations = await _context.TrxOvertimeRealizations
                .Where(ot => ot.IsActive
                    && !ot.IsDelete
                    && ot.CompensationType == "FinancialPayout"
                    && (ot.RealizationStatus == "Verified" || ot.RealizationStatus == "PostedToPayroll")
                    && ((ot.ActualStartDate >= startDateOnly && ot.ActualStartDate <= endDateOnly)
                        || (ot.IsRetrospectiveAdjustment && !ot.IsPayrollPosted)))
                .ToListAsync(cancellationToken);

            var overtimeByEmployee = overtimeRealizations
                .GroupBy(ot => ot.WorkforceProfileId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // 2. Ambil data presensi dinas malam dari Sub-modul 07
            var nightAttendances = await _context.HrdAttendanceDailies
                .Where(att => att.IsActive
                    && !att.IsDelete
                    && att.IsPresent
                    && att.AttendanceDate >= startDateOnly
                    && att.AttendanceDate <= endDateOnly
                    && (att.IsOvernightSchedule || att.NightWorkMinutes > 0))
                .ToListAsync(cancellationToken);

            var nightAttendanceByEmployee = nightAttendances
                .Where(att => att.WorkforceProfileId.HasValue)
                .GroupBy(att => att.WorkforceProfileId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            var employees = run.Employees.ToList();
            var summaries = new List<EmployeeVariableEarningSummaryDto>();

            foreach (var emp in employees)
            {
                overtimeByEmployee.TryGetValue(emp.WorkforceProfileId, out var empOvertimes);
                nightAttendanceByEmployee.TryGetValue(emp.WorkforceProfileId, out var empNightAttendances);

                var summary = ProcessEmployeeVariableEarnings(
                    emp,
                    empOvertimes ?? new List<TrxOvertimeRealization>(),
                    empNightAttendances ?? new List<HrdAttendanceDaily>(),
                    run.PayrollPeriodId,
                    actorUserId);

                summaries.Add(summary);
            }

            // Agregasi Ringkasan Run
            run.TotalEarning = employees.Sum(e => e.TotalRecurringEarning + e.TotalVariableEarning);
            run.TotalGrossPay = employees.Sum(e => e.GrossPay);
            run.TotalNetPay = employees.Sum(e => e.NetPay);
            run.UpdateDateTime = DateTime.UtcNow;
            run.UpdateBy = actorUserId;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Penyerapan lembur dan premi shift malam berhasil untuk {Count} pegawai pada batch {RunNumber}.",
                employees.Count, run.RunNumber);

            return PayrollServiceResult<IngestOvertimeAndShiftResultDto>.Ok(
                new IngestOvertimeAndShiftResultDto
                {
                    RunId = run.Id,
                    RunNumber = run.RunNumber,
                    TotalEmployeesProcessed = employees.Count,
                    TotalOvertimeHours = summaries.Sum(s => s.OvertimeMinutes) / 60,
                    TotalOvertimeAmount = summaries.Sum(s => s.OvertimeRegularAmount + s.OvertimeRetrospectiveAmount),
                    TotalNightShifts = summaries.Sum(s => s.NightShiftCount),
                    TotalNightShiftAllowance = summaries.Sum(s => s.NightShiftAllowance),
                    TotalVariableEarning = summaries.Sum(s => s.TotalVariableEarning),
                    Message = $"Penyerapan variabel lembur dan premi shift malam untuk {employees.Count} pegawai berhasil diproses.",
                    Details = summaries
                },
                "Penyerapan lembur dan premi shift malam berhasil.");
        }

        public async Task<PayrollServiceResult<EmployeeVariableEarningSummaryDto>> IngestEmployeeOvertimeAndShiftAsync(
            Guid runId,
            Guid employeeRunId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var emp = await _context.PrlPayrollRunEmployees
                .Include(e => e.Components.Where(c => c.IsActive))
                .Include(e => e.PayrollRun)
                .FirstOrDefaultAsync(e => e.Id == employeeRunId && e.PayrollRunId == runId && e.IsActive, cancellationToken);

            if (emp == null || emp.PayrollRun == null)
            {
                return PayrollServiceResult<EmployeeVariableEarningSummaryDto>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Data pegawai penggajian ID {employeeRunId} pada batch {runId} tidak ditemukan.");
            }

            if (emp.PayrollRun.IsLocked || emp.PayrollRun.RunStatus is "Approved" or "Paid" or "Closed")
            {
                return PayrollServiceResult<EmployeeVariableEarningSummaryDto>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Batch penggajian telah berstatus final dan tidak dapat diperbarui.");
            }

            var startDateOnly = DateOnly.FromDateTime(emp.PayrollRun.PeriodStartDateSnapshot);
            var endDateOnly = DateOnly.FromDateTime(emp.PayrollRun.PeriodEndDateSnapshot);

            var empOvertimes = await _context.TrxOvertimeRealizations
                .Where(ot => ot.WorkforceProfileId == emp.WorkforceProfileId
                    && ot.IsActive
                    && !ot.IsDelete
                    && ot.CompensationType == "FinancialPayout"
                    && (ot.RealizationStatus == "Verified" || ot.RealizationStatus == "PostedToPayroll")
                    && ((ot.ActualStartDate >= startDateOnly && ot.ActualStartDate <= endDateOnly)
                        || (ot.IsRetrospectiveAdjustment && !ot.IsPayrollPosted)))
                .ToListAsync(cancellationToken);

            var empNightAttendances = await _context.HrdAttendanceDailies
                .Where(att => att.WorkforceProfileId == emp.WorkforceProfileId
                    && att.IsActive
                    && !att.IsDelete
                    && att.IsPresent
                    && att.AttendanceDate >= startDateOnly
                    && att.AttendanceDate <= endDateOnly
                    && (att.IsOvernightSchedule || att.NightWorkMinutes > 0))
                .ToListAsync(cancellationToken);

            var summary = ProcessEmployeeVariableEarnings(
                emp,
                empOvertimes,
                empNightAttendances,
                emp.PayrollRun.PayrollPeriodId,
                actorUserId);

            await _context.SaveChangesAsync(cancellationToken);

            return PayrollServiceResult<EmployeeVariableEarningSummaryDto>.Ok(
                summary,
                $"Penyerapan lembur dan premi shift malam untuk {emp.EmployeeNameSnapshot} berhasil.");
        }

        private EmployeeVariableEarningSummaryDto ProcessEmployeeVariableEarnings(
            PrlPayrollRunEmployee emp,
            List<TrxOvertimeRealization> overtimes,
            List<HrdAttendanceDaily> nightAttendances,
            Guid payrollPeriodId,
            Guid actorUserId)
        {
            // 1. Bersihkan komponen variabel lama (Overtime & Attendance Shift Allowance)
            var existingVariableComponents = emp.Components
                .Where(c => c.SourceType is "Overtime" or "Attendance")
                .ToList();

            foreach (var comp in existingVariableComponents)
            {
                _context.PrlPayrollEmployeeComponents.Remove(comp);
            }

            // 2. Evaluasi Upah Sejam Lembur sesuai PP 35/2021: 1/173 x (Gaji Pokok + Tunjangan Tetap)
            var fixedBase = emp.BaseSalary + emp.TotalRecurringEarning;
            var hourlyRate = fixedBase > 0 ? Math.Round(fixedBase / 173m, 2, MidpointRounding.AwayFromZero) : 0m;

            var regularOvertimes = overtimes.Where(ot => !ot.IsRetrospectiveAdjustment).ToList();
            var retroOvertimes = overtimes.Where(ot => ot.IsRetrospectiveAdjustment).ToList();

            // Lembur Reguler
            var regularMinutes = regularOvertimes.Sum(ot => ot.VerifiedMinutes > 0 ? ot.VerifiedMinutes : ot.ActualMinutes);
            var regularAmount = regularOvertimes.Sum(ot => ot.VerifiedAmount > 0 ? ot.VerifiedAmount : ot.CalculatedAmount);

            // Jika belum ada verified amount, hitung berdasarkan formula PP 35/2021 (estimasi 1.5x flat sbg fallback)
            if (regularAmount == 0 && regularMinutes > 0 && hourlyRate > 0)
            {
                regularAmount = Math.Round(((decimal)regularMinutes / 60m) * hourlyRate * 1.5m, 2, MidpointRounding.AwayFromZero);
            }

            // Lembur Retrospektif (Rapel)
            var retroMinutes = retroOvertimes.Sum(ot => ot.VerifiedMinutes > 0 ? ot.VerifiedMinutes : ot.ActualMinutes);
            var retroAmount = retroOvertimes.Sum(ot => ot.VerifiedAmount > 0 ? ot.VerifiedAmount : ot.CalculatedAmount);
            if (retroAmount == 0 && retroMinutes > 0 && hourlyRate > 0)
            {
                retroAmount = Math.Round(((decimal)retroMinutes / 60m) * hourlyRate * 1.5m, 2, MidpointRounding.AwayFromZero);
            }

            // Tandai status lembur sebagai ter-posting ke payroll
            foreach (var ot in overtimes)
            {
                ot.IsPayrollPosted = true;
                ot.RealizationStatus = "PostedToPayroll";
                ot.PostedToPayrollAt = DateTime.UtcNow;
                ot.PostedToPayrollByUserId = actorUserId;
                ot.PayrollPeriodId = payrollPeriodId;
                ot.PostedMinutes = ot.VerifiedMinutes > 0 ? ot.VerifiedMinutes : ot.ActualMinutes;
                ot.PostedAmount = ot.VerifiedAmount > 0 ? ot.VerifiedAmount : ot.CalculatedAmount;
                ot.UpdateDateTime = DateTime.UtcNow;
                ot.UpdateBy = actorUserId;
            }

            var newComponents = new List<PrlPayrollEmployeeComponent>();

            // Komponen: Uang Lembur Reguler PP 35/2021 (OT01)
            if (regularAmount > 0)
            {
                var otRegularComp = new PrlPayrollEmployeeComponent
                {
                    Id = Guid.NewGuid(),
                    PayrollRunEmployeeId = emp.Id,
                    SourceType = "Overtime",
                    ComponentCodeSnapshot = "OT01",
                    ComponentNameSnapshot = "Uang Lembur Resmi PP 35/2021",
                    ComponentTypeSnapshot = "Earning",
                    CalculationMethodSnapshot = "Overtime",
                    CurrencyCode = "IDR",
                    Quantity = Math.Round((decimal)regularMinutes / 60m, 2),
                    Rate = hourlyRate,
                    Amount = regularAmount,
                    TaxableAmount = regularAmount,
                    IsTaxable = true,
                    IsDisplayedOnPayslip = true,
                    SortOrder = 5,
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = DateTime.UtcNow
                };
                newComponents.Add(otRegularComp);
            }

            // Komponen: Rapel Lembur Retrospektif (OT02)
            if (retroAmount > 0)
            {
                var otRetroComp = new PrlPayrollEmployeeComponent
                {
                    Id = Guid.NewGuid(),
                    PayrollRunEmployeeId = emp.Id,
                    SourceType = "Overtime",
                    ComponentCodeSnapshot = "OT02",
                    ComponentNameSnapshot = "Rapel Lembur Retrospektif Periode Lampau",
                    ComponentTypeSnapshot = "Earning",
                    CalculationMethodSnapshot = "Overtime",
                    CurrencyCode = "IDR",
                    Quantity = Math.Round((decimal)retroMinutes / 60m, 2),
                    Rate = hourlyRate,
                    Amount = retroAmount,
                    TaxableAmount = retroAmount,
                    IsTaxable = true,
                    IsDisplayedOnPayslip = true,
                    SortOrder = 6,
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = DateTime.UtcNow
                };
                newComponents.Add(otRetroComp);
            }

            // 3. Evaluasi Premi Dinas Shift Malam Sub-07 (SH01)
            var nightShiftCount = nightAttendances.Count;
            var nightShiftAllowance = nightShiftCount * DefaultNightShiftRate;

            if (nightShiftAllowance > 0)
            {
                var shiftComp = new PrlPayrollEmployeeComponent
                {
                    Id = Guid.NewGuid(),
                    PayrollRunEmployeeId = emp.Id,
                    SourceType = "Attendance",
                    ComponentCodeSnapshot = "SH01",
                    ComponentNameSnapshot = "Premi Shift Malam (21.00 - 07.00)",
                    ComponentTypeSnapshot = "Earning",
                    CalculationMethodSnapshot = "Attendance",
                    CurrencyCode = "IDR",
                    Quantity = nightShiftCount,
                    Rate = DefaultNightShiftRate,
                    Amount = nightShiftAllowance,
                    TaxableAmount = nightShiftAllowance,
                    IsTaxable = true,
                    IsDisplayedOnPayslip = true,
                    SortOrder = 7,
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = DateTime.UtcNow
                };
                newComponents.Add(shiftComp);
            }

            _context.PrlPayrollEmployeeComponents.AddRange(newComponents);

            // 4. Update Saldo Pegawai
            emp.OvertimeMinutes = regularMinutes + retroMinutes;
            emp.TotalOvertimeAmount = regularAmount + retroAmount;
            emp.TotalAttendanceAllowance = nightShiftAllowance;
            emp.TotalVariableEarning = emp.TotalOvertimeAmount + emp.TotalAttendanceAllowance;
            emp.GrossPay = emp.BaseSalary + emp.TotalRecurringEarning + emp.TotalVariableEarning;
            emp.NetPay = emp.GrossPay - emp.TotalDeduction - emp.TotalTax - emp.TotalEmployeeInsuranceContribution;
            emp.UpdateDateTime = DateTime.UtcNow;
            emp.UpdateBy = actorUserId;

            return new EmployeeVariableEarningSummaryDto
            {
                PayrollRunEmployeeId = emp.Id,
                WorkforceProfileId = emp.WorkforceProfileId,
                EmployeeName = emp.EmployeeNameSnapshot,
                DepartmentName = emp.DepartmentNameSnapshot ?? "-",
                OvertimeMinutes = emp.OvertimeMinutes,
                OvertimeRegularAmount = regularAmount,
                OvertimeRetrospectiveAmount = retroAmount,
                NightShiftCount = nightShiftCount,
                NightShiftAllowance = nightShiftAllowance,
                TotalVariableEarning = emp.TotalVariableEarning,
                UpdatedGrossPay = emp.GrossPay
            };
        }
    }
}
