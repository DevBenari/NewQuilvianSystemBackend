using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services;

/// <summary>
/// BE-FIN-110 (Revisi 21, EPIC FIN-04, FIN-DEC-214..217).
/// Menjalankan pemotongan cicilan piutang RS di dalam mesin payroll run HR:
/// 1. Membaca jadwal angsuran (TrxPayrollVariableInput dari adapter BE-FIN-094) per pegawai.
/// 2. Menegakkan urutan prioritas: potongan wajib (pajak, BPJS) dan potongan berjalan dipotong lebih dulu (AC 2, FIN-DEC-216).
/// 3. Menerapkan batas persentase gaji bersih (MaxNetSalaryPercentage pada MstDeductionType kategori Loan, FIN-DEC-217).
/// 4. Memotong sebagian bila terutang melebihi batas (AC 1).
/// 5. Menghasilkan TrxPayrollEmployeeComponent pada slip/run pegawai.
/// 6. Mengirimkan hasil potongan secara otomatis ke Finance dengan menyertakan PayrollRunId.
/// 7. Aman dari kiriman ulang ganda (idempoten, AC 3).
/// 8. Menolak hasil mesin untuk periode sebelum peralihan (AC 4, FIN-VAL-288).
/// </summary>
public sealed class PayrollInstallmentDeductionService
{
    private const string LogCategory = "Corporate.HumanResource.PayrollManagement.InstallmentDeduction";
    private const string InstallmentSourceType = "FinanceReceivableInstallment";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;
    private readonly FinanceReceivablePayrollSyncService _syncService;

    public PayrollInstallmentDeductionService(
        ApplicationDbContext dbContext,
        LoggerService loggerService,
        FinanceReceivablePayrollSyncService syncService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
        _syncService = syncService;
    }

    /// <summary>
    /// Mengeksekusi potongan cicilan piutang RS untuk seluruh pegawai pada satu payroll run.
    /// </summary>
    public async Task<PayrollRunInstallmentDeductionSummaryResponse> ProcessRunInstallmentDeductionsAsync(
        Guid payrollRunId, Guid actorUserId, CancellationToken cancellationToken)
    {
        var run = await _dbContext.TrxPayrollRuns
            .Include(r => r.PayrollPeriod)
            .SingleOrDefaultAsync(r => r.Id == payrollRunId && !r.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException($"PayrollRun dengan Id {payrollRunId} tidak ditemukan.");

        EnsureRunAcceptsDeduction(run);

        // Seluruh pegawai yang punya angsuran harus sudah memiliki gaji bruto terhitung SEBELUM satu pun hasil dikirim ke Finance.
        // Tanpa ini, pemrosesan berhenti di tengah run (hasil pegawai sebelumnya sudah tercatat di Finance, sisanya belum).
        var notComputed = await _dbContext.TrxPayrollRunEmployees.AsNoTracking()
            .Where(e => e.PayrollRunId == payrollRunId && !e.IsDelete
                && e.VariableInputs.Any(v => v.SourceType == InstallmentSourceType && !v.IsDelete)
                && e.GrossPay <= 0m
                && (e.BaseSalary + e.TotalRecurringEarning + e.TotalVariableEarning) <= 0m)
            .Select(e => e.EmployeeNameSnapshot)
            .Take(5)
            .ToListAsync(cancellationToken);
        if (notComputed.Count > 0)
            throw new PayrollResultValidationException(GrossPayNotComputedMessage(string.Join(", ", notComputed)));

        var employeeIds = await _dbContext.TrxPayrollRunEmployees
            .Where(e => e.PayrollRunId == payrollRunId && !e.IsDelete)
            .Select(e => e.Id)
            .ToListAsync(cancellationToken);

        var summary = new PayrollRunInstallmentDeductionSummaryResponse
        {
            PayrollRunId = run.Id,
            RunNumber = run.RunNumber,
            PeriodCode = run.PayrollPeriod?.PayrollPeriodCode ?? string.Empty,
            TotalEmployeesProcessed = employeeIds.Count
        };

        foreach (var employeeId in employeeIds)
        {
            var empResult = await ProcessEmployeeInstallmentDeductionAsync(employeeId, actorUserId, cancellationToken);
            if (empResult.Status != "NONE")
            {
                summary.TotalInstallmentsProcessed++;
                summary.TotalDueAmount += empResult.DueAmount;
                summary.TotalDeductedAmount += empResult.DeductedAmount;

                if (empResult.Status == PayrollResultStatuses.Berhasil)
                    summary.TotalSuccessCount++;
                else if (empResult.Status == PayrollResultStatuses.Sebagian)
                    summary.TotalPartialCount++;
                else if (empResult.Status == PayrollResultStatuses.Gagal)
                    summary.TotalFailedCount++;

                summary.Items.Add(empResult);
            }
        }

        await _loggerService.AuditAsync(LogCategory, "RunDeductions.Completed",
            $"Pemotongan cicilan run {run.RunNumber} selesai: {summary.TotalInstallmentsProcessed} angsuran diproses, total terpotong Rp {summary.TotalDeductedAmount:N2}.",
            new { run.Id, summary.TotalInstallmentsProcessed, summary.TotalDeductedAmount, ActorUserId = actorUserId });

        return summary;
    }

    /// <summary>
    /// Mengeksekusi potongan cicilan piutang RS untuk satu baris pegawai di payroll run (AC 1, AC 2, AC 3, AC 4).
    /// </summary>
    public async Task<PayrollEmployeeInstallmentDeductionResponse> ProcessEmployeeInstallmentDeductionAsync(
        Guid payrollRunEmployeeId, Guid actorUserId, CancellationToken cancellationToken)
    {
        var runEmployee = await _dbContext.TrxPayrollRunEmployees
            .Include(x => x.PayrollRun)
                .ThenInclude(r => r!.PayrollPeriod)
            .Include(x => x.Components)
            .Include(x => x.VariableInputs)
            .SingleOrDefaultAsync(x => x.Id == payrollRunEmployeeId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException($"PayrollRunEmployee dengan Id {payrollRunEmployeeId} tidak ditemukan.");

        var variableInput = runEmployee.VariableInputs
            .FirstOrDefault(x => x.SourceType == InstallmentSourceType && !x.IsDelete);

        if (variableInput is null || !variableInput.SourceId.HasValue)
        {
            return new PayrollEmployeeInstallmentDeductionResponse
            {
                PayrollRunEmployeeId = runEmployee.Id,
                PayrollRunId = runEmployee.PayrollRunId,
                EmployeeId = runEmployee.EmployeeId ?? Guid.Empty,
                EmployeeName = runEmployee.EmployeeNameSnapshot,
                EmployeeNumber = runEmployee.EmployeeNumberSnapshot ?? string.Empty,
                Status = "NONE",
                Message = "Tidak ada input variabel cicilan piutang RS untuk pegawai ini."
            };
        }

        // Run yang terkunci/dibatalkan tidak boleh lagi mengubah potongan, dan hasil ke Finance tidak boleh dikirim darinya.
        EnsureRunAcceptsDeduction(runEmployee.PayrollRun);

        var installment = await _dbContext.FinReceivableInstallments
            .SingleOrDefaultAsync(x => x.Id == variableInput.SourceId.Value && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException($"Angsuran Finance dengan Id {variableInput.SourceId.Value} tidak ditemukan.");

        // AC 4 & FIN-VAL-288: Validasi masa peralihan (cutover)
        var cutover = await _dbContext.MstPayrollDeductionCutovers.AsNoTracking()
            .FirstOrDefaultAsync(x => x.IsActive && !x.IsDelete, cancellationToken);
        if (cutover is not null && string.CompareOrdinal(installment.DeductionPeriod, cutover.AutomaticFromPeriod) < 0)
        {
            throw new PayrollResultValidationException($"Periode {installment.DeductionPeriod} masih memakai pencatatan manual.");
        }

        // AC 2 & FIN-DEC-216: Potongan wajib (pajak, BPJS) dan potongan non-loan berjalan dipotong lebih dulu.
        var grossPay = runEmployee.GrossPay > 0
            ? runEmployee.GrossPay
            : (runEmployee.BaseSalary + runEmployee.TotalRecurringEarning + runEmployee.TotalVariableEarning);

        // Gaji bruto belum dihitung (mis. baris run dibuat adapter jadwal Finance dan mesin payroll belum mengisinya): batas potong
        // = Rp0 hanya karena DATA belum ada. Mencatatnya sebagai GAGAL akan menandai angsuran TERTUNGGAK, melimpahkan sisanya ke
        // periode berikutnya, dan mengunci periode itu (idempotensi) padahal tidak ada potongan yang pernah dicoba. Ditolak
        // sebelum hasil apa pun dikirim ke Finance.
        if (grossPay <= 0m)
            throw new PayrollResultValidationException(GrossPayNotComputedMessage(runEmployee.EmployeeNameSnapshot));

        var statutoryDeductions = runEmployee.TotalTax + runEmployee.TotalEmployeeInsuranceContribution;

        var otherPriorDeductions = runEmployee.Components
            .Where(c => !c.IsDelete
                && c.ComponentTypeSnapshot == "Deduction"
                && c.ComponentCodeSnapshot != "FINANCE_INSTALLMENT_DEDUCTION"
                && c.ComponentCodeSnapshot != "LOAN_HOSPITAL_AR")
            .Sum(c => c.Amount);

        // Gaji bersih sebelum potongan cicilan piutang RS
        var availableNetSalary = Math.Max(0m, grossPay - statutoryDeductions - otherPriorDeductions);

        // FIN-DEC-217: Batas persentase dari gaji bersih yang diatur di master jenis potongan (MstDeductionType).
        var deductionType = await _dbContext.MstDeductionTypes.AsNoTracking()
            .FirstOrDefaultAsync(x => !x.IsDelete && x.IsActive
                && (x.DeductionCategory == "Loan" || x.DeductionTypeCode == "LOAN_HOSPITAL_AR" || x.DeductionTypeName.Contains("Piutang RS")), cancellationToken);

        decimal maxAllowedByPercentage;
        if (deductionType?.MaxNetSalaryPercentage.HasValue == true)
        {
            maxAllowedByPercentage = availableNetSalary * (deductionType.MaxNetSalaryPercentage.Value / 100m);
        }
        else
        {
            // Default batas wajar jika belum diset: 50% dari gaji bersih
            maxAllowedByPercentage = availableNetSalary * 0.5m;
        }

        var maxAllowedLimit = maxAllowedByPercentage;
        if (deductionType?.MaximumAmount.HasValue == true && deductionType.MaximumAmount.Value > 0)
        {
            maxAllowedLimit = Math.Min(maxAllowedLimit, deductionType.MaximumAmount.Value);
        }

        maxAllowedLimit = Math.Min(maxAllowedLimit, availableNetSalary);
        maxAllowedLimit = Math.Max(0m, maxAllowedLimit);

        // AC 1: Nominal terutang (ScheduledAmount + CarriedOverAmount)
        var dueAmount = variableInput.Amount;
        decimal deductedAmount;
        string status;

        if (dueAmount <= maxAllowedLimit)
        {
            deductedAmount = dueAmount;
            status = PayrollResultStatuses.Berhasil;
        }
        else if (maxAllowedLimit > 0)
        {
            deductedAmount = maxAllowedLimit;
            status = PayrollResultStatuses.Sebagian;
        }
        else
        {
            deductedAmount = 0m;
            status = PayrollResultStatuses.Gagal;
        }

        // Catat/perbarui TrxPayrollEmployeeComponent pada slip/run karyawan
        var existingComponent = runEmployee.Components
            .FirstOrDefault(c => !c.IsDelete && c.SourceType == "VariableInput" && c.SourceId == variableInput.Id);

        if (existingComponent is not null)
        {
            existingComponent.Amount = deductedAmount;
            existingComponent.UpdateDateTime = DateTime.UtcNow;
            existingComponent.UpdateBy = actorUserId;
        }
        else
        {
            var newComponent = new TrxPayrollEmployeeComponent
            {
                PayrollRunEmployeeId = runEmployee.Id,
                PayrollComponentId = variableInput.PayrollComponentId,
                SourceType = "VariableInput",
                SourceId = variableInput.Id,
                ComponentCodeSnapshot = "FINANCE_INSTALLMENT_DEDUCTION",
                ComponentNameSnapshot = "Potongan Cicilan Piutang RS",
                ComponentTypeSnapshot = "Deduction",
                CalculationMethodSnapshot = "BalanceBased",
                CurrencyCode = runEmployee.CurrencyCode,
                Amount = deductedAmount,
                IsDisplayedOnPayslip = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _dbContext.TrxPayrollEmployeeComponents.Add(newComponent);
        }

        variableInput.InputStatus = "Applied";
        variableInput.UpdateDateTime = DateTime.UtcNow;
        variableInput.UpdateBy = actorUserId;

        // Perbarui total potongan dan gaji bersih pegawai
        runEmployee.TotalDeduction = statutoryDeductions + otherPriorDeductions + deductedAmount;
        runEmployee.NetPay = grossPay - runEmployee.TotalDeduction;
        runEmployee.UpdateDateTime = DateTime.UtcNow;
        runEmployee.UpdateBy = actorUserId;

        // Kirim hasil pemotongan ke Finance via ApplyResultCoreAsync (membawa PayrollRunId)
        var financeResult = await _syncService.ApplyResultCoreAsync(new PayrollResultRequest
        {
            InstallmentId = installment.Id,
            PayrollPeriodId = runEmployee.PayrollRun?.PayrollPeriodId ?? installment.LastPayrollPeriodId ?? Guid.Empty,
            Status = status,
            DeductedAmount = deductedAmount,
            ExecutionTimestamp = DateTimeOffset.UtcNow,
            PayrollRunId = runEmployee.PayrollRunId,
            Notes = $"Potongan otomatis mesin payroll HR run {runEmployee.PayrollRun?.RunNumber} ({status})"
        }, actorUserId, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "InstallmentDeduction.Executed",
            $"Potongan cicilan dieksekusi: Employee={runEmployee.EmployeeNameSnapshot}, Terutang=Rp {dueAmount:N2}, Terpotong=Rp {deductedAmount:N2}, Status={status}.",
            new { runEmployee.Id, InstallmentId = installment.Id, Status = status, DeductedAmount = deductedAmount, ActorUserId = actorUserId });

        return new PayrollEmployeeInstallmentDeductionResponse
        {
            PayrollRunEmployeeId = runEmployee.Id,
            PayrollRunId = runEmployee.PayrollRunId,
            EmployeeId = runEmployee.EmployeeId ?? Guid.Empty,
            EmployeeName = runEmployee.EmployeeNameSnapshot,
            EmployeeNumber = runEmployee.EmployeeNumberSnapshot ?? string.Empty,
            InstallmentId = installment.Id,
            DeductionPeriod = installment.DeductionPeriod,
            DueAmount = dueAmount,
            AvailableNetSalary = availableNetSalary,
            MaxAllowedDeduction = maxAllowedLimit,
            DeductedAmount = deductedAmount,
            RemainingUnpaidAmount = dueAmount - deductedAmount,
            Status = status,
            IsIdempotentReplay = financeResult.IsIdempotentReplay,
            Message = financeResult.IsIdempotentReplay
                ? "Hasil potongan untuk periode ini sudah tercatat sebelumnya (idempoten)."
                : $"Potongan cicilan {status} sebesar Rp {deductedAmount:N2} berhasil dieksekusi."
        };
    }

    private static string GrossPayNotComputedMessage(string employees) =>
        $"Gaji bruto belum dihitung pada payroll run untuk: {employees}. Potongan cicilan tidak dapat dieksekusi sebelum gaji dihitung; " +
        "hasil GAGAL tidak dicatat karena tidak menggambarkan kegagalan pemotongan yang sebenarnya.";

    /// <summary>
    /// Run yang terkunci atau sudah dibatalkan/dibalik tidak menerima eksekusi potongan. Titik realisasi di antara DRAFT dan
    /// APPROVED/PAID belum ditetapkan sebagai keputusan bisnis, jadi status lain sengaja tidak dibatasi di sini.
    /// </summary>
    private static void EnsureRunAcceptsDeduction(TrxPayrollRun? run)
    {
        if (run is null) return;

        if (run.IsLocked)
            throw new PayrollResultValidationException(
                $"Payroll run {run.RunNumber} terkunci sehingga potongan cicilan tidak dapat dieksekusi.");

        if (string.Equals(run.RunStatus, "Cancelled", StringComparison.OrdinalIgnoreCase)
            || string.Equals(run.RunStatus, "Reversed", StringComparison.OrdinalIgnoreCase))
            throw new PayrollResultValidationException(
                $"Payroll run {run.RunNumber} berstatus {run.RunStatus} sehingga potongan cicilan tidak dapat dieksekusi.");
    }
}

