using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Repositories;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    /// <summary>
    /// Layanan penerima webhook callback rekonsiliasi transfer bank dari Bagian Keuangan (INT-HR-54, BE-HRD-159, HRD-DEC-126).
    /// </summary>
    public class PayrollDisbursementReconciliationService : IPayrollDisbursementReconciliationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<PayrollDisbursementReconciliationService> _logger;

        public PayrollDisbursementReconciliationService(
            ApplicationDbContext dbContext,
            ILogger<PayrollDisbursementReconciliationService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<FinanceConfirmationResultDto> ConfirmPaymentAsync(
            FinancePaymentConfirmationCommand command,
            CancellationToken cancellationToken = default)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var batch = await _dbContext.TrxPayrollHandoffBatches
                .Include(b => b.PayrollRun)
                    .ThenInclude(r => r!.Employees)
                .FirstOrDefaultAsync(b => b.Id == command.HandoffBatchId || b.PayrollRunId == command.PayrollRunId, cancellationToken);

            if (batch == null || batch.PayrollRun == null)
            {
                throw new KeyNotFoundException($"Paket serah terima keuangan atau batch penggajian terkait tidak ditemukan.");
            }

            var run = batch.PayrollRun;

            // 1. Mutasi status batch handoff
            batch.HandoffStatus = "PaymentConfirmed";
            batch.FinanceReferenceNo = command.BankDisbursementReference;
            batch.FinanceNotes = command.Notes;

            // 2. Mutasi status siklus penggajian
            run.RunStatus = "Completed";
            run.PaymentDateSnapshot = command.ActualDisbursedAt;
            run.IsLocked = true;

            // 3. Pelepasan slip gaji digital ESS bagi seluruh pegawai aktif (VR-PAY-23, HRD-DEC-127)
            var releasedCount = 0;
            var activeEmployees = run.Employees
                .Where(e => e.EmployeePayrollStatus != "Reversed" && e.EmployeePayrollStatus != "Error")
                .ToList();

            var existingPayslips = await _dbContext.TrxPayrollPayslips
                .Where(p => p.PayrollRunId == run.Id)
                .ToDictionaryAsync(p => p.PayrollRunEmployeeId, cancellationToken);

            foreach (var emp in activeEmployees)
            {
                var nik = !string.IsNullOrWhiteSpace(emp.EmployeeNumberSnapshot)
                    ? emp.EmployeeNumberSnapshot
                    : $"EMP-{emp.Id.ToString()[..6].ToUpper()}";

                var payslipNumber = $"PS/{run.PeriodStartDateSnapshot:yyyy}/{run.PeriodStartDateSnapshot:MM}/{nik}";

                var totalEarnings = Math.Round(
                    emp.TotalRecurringEarning + emp.TotalVariableEarning + emp.TotalOvertimeAmount,
                    2, MidpointRounding.AwayFromZero);

                if (existingPayslips.TryGetValue(emp.Id, out var existingSlip))
                {
                    existingSlip.PayslipStatus = "Published";
                    existingSlip.IsEmployeeVisible = true;
                    existingSlip.PublishedAt = DateTime.UtcNow;
                    existingSlip.BaseSalary = emp.BaseSalary;
                    existingSlip.TotalEarning = totalEarnings;
                    existingSlip.TotalDeduction = emp.TotalDeduction;
                    existingSlip.TotalTax = emp.TotalTax;
                    existingSlip.GrossPay = emp.GrossPay;
                    existingSlip.NetPay = emp.NetPay;
                    existingSlip.IsSnapshotFrozen = true;
                }
                else
                {
                    var newSlip = new TrxPayrollPayslip
                    {
                        Id = Guid.NewGuid(),
                        PayrollRunId = run.Id,
                        PayrollRunEmployeeId = emp.Id,
                        PayslipNumber = payslipNumber,
                        VersionNumber = 1,
                        PayslipStatus = "Published",
                        CurrencyCode = run.CurrencyCode ?? "IDR",
                        PeriodStartDateSnapshot = run.PeriodStartDateSnapshot,
                        PeriodEndDateSnapshot = run.PeriodEndDateSnapshot,
                        EmployeeNumberSnapshot = emp.EmployeeNumberSnapshot,
                        EmployeeNameSnapshot = emp.EmployeeNameSnapshot,
                        DepartmentNameSnapshot = emp.DepartmentNameSnapshot,
                        PositionNameSnapshot = emp.PositionNameSnapshot,
                        BaseSalary = emp.BaseSalary,
                        TotalEarning = totalEarnings,
                        TotalDeduction = emp.TotalDeduction,
                        TotalTax = emp.TotalTax,
                        GrossPay = emp.GrossPay,
                        NetPay = emp.NetPay,
                        IsEmployeeVisible = true,
                        IsSnapshotFrozen = true,
                        IsActive = true,
                        PublishedAt = DateTime.UtcNow
                    };
                    _dbContext.TrxPayrollPayslips.Add(newSlip);
                }

                releasedCount++;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Konfirmasi pembayaran Finance diterima untuk Batch {BatchNumber}. Status Completed, {Count} slip gaji dirilis ke portal ESS.",
                batch.BatchNumber, releasedCount);

            return new FinanceConfirmationResultDto
            {
                HandoffBatchId = batch.Id,
                PayrollRunId = run.Id,
                Status = "Completed",
                PayslipsReleasedCount = releasedCount,
                ProcessedAt = DateTime.UtcNow,
                Message = $"Konfirmasi transfer bank berhasil diverifikasi. Status batch telah 'Completed' dan {releasedCount} slip gaji digital telah dirilis ke ESS pegawai."
            };
        }

        public async Task<FinanceRejectionResultDto> RejectPaymentAsync(
            FinanceRejectionCallbackCommand command,
            CancellationToken cancellationToken = default)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            // Validasi aturan bisnis penolakan: Alasan wajib minimal 15 karakter (PAY-B7, VR-PAY-16)
            if (string.IsNullOrWhiteSpace(command.RejectionReason) || command.RejectionReason.Trim().Length < 15)
            {
                throw new ArgumentException(
                    "Catatan alasan penolakan dari Finance wajib diisi minimal 15 karakter formal audit (PAY-B7, VR-PAY-16).");
            }

            var batch = await _dbContext.TrxPayrollHandoffBatches
                .Include(b => b.PayrollRun)
                .FirstOrDefaultAsync(b => b.Id == command.HandoffBatchId || b.PayrollRunId == command.PayrollRunId, cancellationToken);

            if (batch == null || batch.PayrollRun == null)
            {
                throw new KeyNotFoundException("Paket serah terima keuangan atau batch penggajian terkait tidak ditemukan.");
            }

            var run = batch.PayrollRun;

            // 1. Mutasi status batch handoff
            batch.HandoffStatus = "RejectedByFinance";
            batch.FinanceNotes = command.RejectionReason.Trim();

            // 2. Rollback Terkendali (HRD-DEC-126)
            // Membuka gembok immutable secara terkendali agar payroll officer dapat merevisi data rekening bank/komponen bermasalah
            run.RunStatus = "NeedCorrection";
            run.IsLocked = false;
            run.Notes = $"{run.Notes} | [REJECTED BY FINANCE oleh {command.RejectedByFinanceOfficer} pada {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC]: {command.RejectionReason.Trim()}";

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogWarning(
                "Penolakan transfer bank oleh Finance untuk Batch {BatchNumber}. Rollback ke NeedCorrection dengan gembok terbuka terkendali. Alasan: {Reason}",
                batch.BatchNumber, command.RejectionReason);

            return new FinanceRejectionResultDto
            {
                HandoffBatchId = batch.Id,
                PayrollRunId = run.Id,
                Status = "RejectedByFinance",
                RollbackStatus = "NeedCorrection",
                ProcessedAt = DateTime.UtcNow,
                Message = $"Sinyal penolakan Finance berhasil diproses. Status siklus penggajian telah dikembalikan ke 'NeedCorrection' dan gembok batch telah dibuka secara terkendali untuk perbaikan."
            };
        }
    }
}
