using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    /// <summary>
    /// Layanan rekonsiliasi dan eksekusi pelunasan utang pinjaman pegawai pada pesangon penutupan Exit Clearance Sub-20 (BE-HRD-174, HRD-DEC-132, INT-HR-60, DED-B7, DED-W5, VR-DED-07, VR-DED-21).
    /// </summary>
    public class ExitClearanceReceivableService : IExitClearanceReceivableService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<ExitClearanceReceivableService> _logger;

        public ExitClearanceReceivableService(
            ApplicationDbContext dbContext,
            ILogger<ExitClearanceReceivableService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<ExitClearanceReceivableCalculationResultDto> CalculateExitClearanceDeductionAsync(
            ExitClearanceReceivableCalculationRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var emp = await _dbContext.MstEmployees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == request.EmployeeId && !e.IsDelete, cancellationToken);

            if (emp == null)
            {
                throw new KeyNotFoundException($"Pegawai dengan ID {request.EmployeeId} tidak ditemukan.");
            }

            // Ambil seluruh jadwal cicilan aktif atau tertunda (diurutkan prioritas 1 s.d. 4)
            var schedules = await _dbContext.TrxPayrollDeductionSchedules
                .AsNoTracking()
                .Where(s => s.EmployeeId == request.EmployeeId &&
                            (s.ScheduleStatus == "Active" || s.ScheduleStatus == "Paused") &&
                            !s.IsDelete)
                .OrderBy(s => s.PriorityLevel)
                .ThenBy(s => s.StartDate)
                .ToListAsync(cancellationToken);

            decimal totalDebt = schedules.Sum(s => s.RemainingPrincipal);
            decimal grossSeverance = request.SeverancePayAmount + request.UnusedLeaveCompensationAmount + request.OtherAllowancesAmount;

            // Pengecualian batas 50% mutlak berlaku hingga 100% pada Final Clearance Payslip (DED-B7, VR-DED-07)
            decimal totalDeducted = Math.Min(grossSeverance, totalDebt);
            decimal netPayableSeverance = grossSeverance - totalDeducted;
            decimal deficit = Math.Max(0m, totalDebt - grossSeverance);
            bool isFullySettled = deficit == 0m;
            string clearanceStatus = isFullySettled ? "Cleared" : "BlockedPendingSettlement";

            string? statementNumber = null;
            if (!isFullySettled)
            {
                // Terbitkan Outstanding Receivable Statement resmi ke Keuangan (DED-W5, INT-HR-60)
                statementNumber = $"STATEMENT-{DateTime.UtcNow:yyyyMM}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
            }

            decimal remainingPool = totalDeducted;
            var settlementDetails = new List<ExitClearanceScheduleSettlementItemDto>();

            foreach (var sch in schedules)
            {
                decimal allocated = Math.Min(remainingPool, sch.RemainingPrincipal);
                remainingPool -= allocated;
                decimal remainingBal = sch.RemainingPrincipal - allocated;

                string statusAfter = remainingBal == 0m
                    ? "ClearedAtSeparation"
                    : (allocated > 0m ? "PartiallySettled" : "Unsettled");

                settlementDetails.Add(new ExitClearanceScheduleSettlementItemDto
                {
                    ScheduleId = sch.Id,
                    ScheduleNumber = sch.ScheduleNumber,
                    DeductionCategory = sch.DeductionCategory,
                    PriorityLevel = sch.PriorityLevel,
                    ExternalReferenceId = sch.ExternalReferenceId,
                    OriginalRemainingPrincipal = sch.RemainingPrincipal,
                    DeductedFromSeverance = allocated,
                    RemainingBalanceAfterSettlement = remainingBal,
                    StatusAfterSettlement = statusAfter
                });
            }

            return new ExitClearanceReceivableCalculationResultDto
            {
                EmployeeId = emp.Id,
                EmployeeName = emp.FullName,
                EmployeeNik = emp.EmployeeNumber,
                TotalGrossSeverancePackage = grossSeverance,
                TotalOutstandingDebt = totalDebt,
                DebtDeductionCapPercentage = 100.00m,
                TotalDeductedAmount = totalDeducted,
                NetPayableSeverance = netPayableSeverance,
                DeficitResidualDebt = deficit,
                IsFullySettled = isFullySettled,
                ExitClearanceStatus = clearanceStatus,
                OutstandingReceivableStatementNumber = statementNumber,
                SettlementDetails = settlementDetails,
                Message = isFullySettled
                    ? "Seluruh kewajiban pinjaman pegawai berhasil dilunasi dari hak pesangon akhir (100% Cap diizinkan). Status Exit Clearance: Cleared."
                    : $"PERINGATAN FINANSIAL (DED-W5): Hak pesangon tidak mencukupi untuk melunasi seluruh pinjaman RS. Terdapat sisa defisit utang Rp {deficit:N2}. Surat tagihan {statementNumber} diterbitkan dan Exit Clearance DITAHAN (BlockedPendingSettlement)."
            };
        }

        public async Task<ExitClearanceReceivableCalculationResultDto> ConfirmExitClearanceSettlementAsync(
            ConfirmExitClearanceSettlementRequestDto request,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var emp = await _dbContext.MstEmployees
                .FirstOrDefaultAsync(e => e.Id == request.EmployeeId && !e.IsDelete, cancellationToken);

            if (emp == null)
            {
                throw new KeyNotFoundException($"Pegawai dengan ID {request.EmployeeId} tidak ditemukan.");
            }

            var schedules = await _dbContext.TrxPayrollDeductionSchedules
                .Where(s => s.EmployeeId == request.EmployeeId &&
                            (s.ScheduleStatus == "Active" || s.ScheduleStatus == "Paused") &&
                            !s.IsDelete)
                .OrderBy(s => s.PriorityLevel)
                .ThenBy(s => s.StartDate)
                .ToListAsync(cancellationToken);

            decimal totalDebt = schedules.Sum(s => s.RemainingPrincipal);
            decimal grossSeverance = request.SeverancePayAmount + request.UnusedLeaveCompensationAmount + request.OtherAllowancesAmount;

            decimal totalDeducted = Math.Min(grossSeverance, totalDebt);
            decimal netPayableSeverance = grossSeverance - totalDeducted;
            decimal deficit = Math.Max(0m, totalDebt - grossSeverance);
            bool isFullySettled = deficit == 0m;
            string clearanceStatus = isFullySettled ? "Cleared" : "BlockedPendingSettlement";

            string? statementNumber = null;
            if (!isFullySettled)
            {
                statementNumber = $"STATEMENT-{DateTime.UtcNow:yyyyMM}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
            }

            decimal remainingPool = totalDeducted;
            var settlementDetails = new List<ExitClearanceScheduleSettlementItemDto>();
            var now = DateTime.UtcNow;

            foreach (var sch in schedules)
            {
                decimal allocated = Math.Min(remainingPool, sch.RemainingPrincipal);
                remainingPool -= allocated;
                decimal remainingBal = sch.RemainingPrincipal - allocated;

                sch.RemainingPrincipal = remainingBal;
                sch.UpdateDateTime = now;
                sch.UpdateBy = userId;

                string statusAfter;
                if (remainingBal == 0m)
                {
                    statusAfter = "ClearedAtSeparation";
                    sch.ScheduleStatus = "ClearedAtSeparation";
                    sch.EndDate = request.EffectiveSeparationDate;
                }
                else
                {
                    statusAfter = allocated > 0m ? "PartiallySettled" : "Unsettled";
                }

                settlementDetails.Add(new ExitClearanceScheduleSettlementItemDto
                {
                    ScheduleId = sch.Id,
                    ScheduleNumber = sch.ScheduleNumber,
                    DeductionCategory = sch.DeductionCategory,
                    PriorityLevel = sch.PriorityLevel,
                    ExternalReferenceId = sch.ExternalReferenceId,
                    OriginalRemainingPrincipal = sch.RemainingPrincipal + allocated,
                    DeductedFromSeverance = allocated,
                    RemainingBalanceAfterSettlement = remainingBal,
                    StatusAfterSettlement = statusAfter
                });
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            if (isFullySettled)
            {
                _logger.LogInformation("Exit clearance untuk pegawai {EmployeeId} ({Nik}) disahkan lunas. Total pesangon Rp {Gross}, dipotong utang Rp {Deducted}, hak bersih Rp {Net}",
                    emp.Id, emp.EmployeeNumber, grossSeverance, totalDeducted, netPayableSeverance);
            }
            else
            {
                _logger.LogWarning("Exit clearance pegawai {EmployeeId} ({Nik}) DITAHAN (BlockedPendingSettlement). Sisa defisit: Rp {Deficit}. Surat tagihan: {StatementNo}",
                    emp.Id, emp.EmployeeNumber, deficit, statementNumber);
            }

            return new ExitClearanceReceivableCalculationResultDto
            {
                EmployeeId = emp.Id,
                EmployeeName = emp.FullName,
                EmployeeNik = emp.EmployeeNumber,
                TotalGrossSeverancePackage = grossSeverance,
                TotalOutstandingDebt = totalDebt,
                DebtDeductionCapPercentage = 100.00m,
                TotalDeductedAmount = totalDeducted,
                NetPayableSeverance = netPayableSeverance,
                DeficitResidualDebt = deficit,
                IsFullySettled = isFullySettled,
                ExitClearanceStatus = clearanceStatus,
                OutstandingReceivableStatementNumber = statementNumber,
                SettlementDetails = settlementDetails,
                Message = isFullySettled
                    ? "Eksekusi pelunasan pesangon berhasil. Seluruh jadwal cicilan pegawai ditutup berstatus ClearedAtSeparation. Exit Clearance disahkan."
                    : $"Eksekusi potongan pesangon selesai sebagian. Sisa utang Rp {deficit:N2} diterbitkan ke Keuangan via {statementNumber}. Exit Clearance berstatus BlockedPendingSettlement."
            };
        }
    }
}
