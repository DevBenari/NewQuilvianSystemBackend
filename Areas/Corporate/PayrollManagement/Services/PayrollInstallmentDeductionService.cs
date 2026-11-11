using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Options;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// Pemotongan cicilan piutang RS di dalam payroll run HRD (BE-FIN-110, kontrak direvisi mengikuti keputusan Finance-HRD).
///
/// Dua langkah yang sengaja dipisah:
/// 1. <see cref="CalculateRunAsync"/>: dipanggil saat HRD MENGHITUNG run. Membaca jadwal angsuran dari input variabel, menegakkan
///    urutan prioritas (pajak dan iuran wajib, potongan lain yang berjalan) dan batas persen gaji bersih, lalu menyimpan potongan
///    RENCANA pada komponen slip. TIDAK menyentuh Finance: saldo piutang belum berubah.
/// 2. <see cref="ConfirmRunAsync"/>: dipanggil HANYA saat run DISAHKAN. Mengirim hasil potongan yang sudah final ke Finance
///    (ApplyResultCoreAsync, idempoten per angsuran dan periode) sehingga saldo piutang berkurang setelah hasilnya terkonfirmasi.
///
/// Tidak ada eksekusi manual per pegawai untuk proses normal. Koreksi sesudah run disahkan lewat pembalikan Finance
/// (ApplyCorrectionCoreAsync) dengan approval dan jejak audit, bukan menjalankan ulang potongan.
/// Kedua method berjalan di dalam transaksi pemanggil (PayrollRunService); keduanya tidak membuka transaksi sendiri.
/// </summary>
public sealed class PayrollInstallmentDeductionService
{
    private const string LogCategory = "Corporate.HumanResource.PayrollManagement.InstallmentDeduction";
    private const string InstallmentSourceType = "FinanceReceivableInstallment";
    private const string ComponentSourceType = "VariableInput";
    private const string ComponentCode = "FINANCE_INSTALLMENT_DEDUCTION";
    private const string InputDraft = "Draft";
    private const string InputApplied = "Applied";

    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;
    private readonly FinanceReceivablePayrollSyncService _syncService;
    private readonly PayrollPolicyOptions _policy;

    public PayrollInstallmentDeductionService(
        ApplicationDbContext dbContext,
        LoggerService loggerService,
        FinanceReceivablePayrollSyncService syncService,
        IOptions<PayrollPolicyOptions> policyOptions)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
        _syncService = syncService;
        _policy = policyOptions.Value;
    }

    /// <summary>Potongan rencana yang disimpan pada komponen slip (CalculationDetailJson) dan dikonfirmasi saat run disahkan.</summary>
    private sealed record PlannedDeduction(
        Guid InstallmentId, string DeductionPeriod, decimal DueAmount, decimal AvailableNetSalary, decimal MaxAllowed,
        decimal DeductedAmount, string Status);

    // ------------------------------------------------------------------------------------
    // 1. HITUNG (tanpa Finance)
    // ------------------------------------------------------------------------------------

    public async Task<PayrollRunInstallmentDeductionSummaryResponse> CalculateRunAsync(
        PrlPayrollRun run, Guid actorUserId, CancellationToken cancellationToken)
    {
        var employees = await _dbContext.PrlPayrollRunEmployees
            .Include(x => x.Components)
            .Include(x => x.VariableInputs)
            .Where(x => x.PayrollRunId == run.Id && !x.IsDelete)
            .ToListAsync(cancellationToken);

        var summary = NewSummary(run, employees.Count);

        var cutover = await _dbContext.MstPayrollDeductionCutovers.AsNoTracking()
            .FirstOrDefaultAsync(x => x.IsActive && !x.IsDelete, cancellationToken);

        var deductionType = await _dbContext.MstDeductionTypes.AsNoTracking()
            .FirstOrDefaultAsync(x => !x.IsDelete && x.IsActive
                && (x.DeductionCategory == "Loan" || x.DeductionTypeCode == "LOAN_HOSPITAL_AR" || x.DeductionTypeName.Contains("Piutang RS")), cancellationToken);

        foreach (var employee in employees)
        {
            var input = employee.VariableInputs
                .FirstOrDefault(x => x.SourceType == InstallmentSourceType && !x.IsDelete && x.SourceId.HasValue);
            if (input is null) continue;

            var installment = await _dbContext.FinReceivableInstallments.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == input.SourceId!.Value && !x.IsDelete, cancellationToken);
            if (installment is null)
            {
                summary.Blockers.Add($"{employee.EmployeeNameSnapshot}: angsuran Finance {input.SourceId} tidak ditemukan.");
                continue;
            }

            // Periode sebelum peralihan tetap memakai pencatatan manual Finance: tidak boleh diproses mesin payroll.
            if (cutover is not null && string.CompareOrdinal(installment.DeductionPeriod, cutover.AutomaticFromPeriod) < 0)
            {
                summary.Blockers.Add($"{employee.EmployeeNameSnapshot}: periode {installment.DeductionPeriod} masih memakai pencatatan manual.");
                continue;
            }

            // Gaji bruto belum dihitung: batas potong Rp0 hanya karena DATA belum ada. Tidak dicatat sebagai GAGAL (itu menandai
            // angsuran tertunggak di Finance); pengajuan run diblokir sampai data gaji lengkap.
            var grossPay = employee.GrossPay > 0m
                ? employee.GrossPay
                : employee.BaseSalary + employee.TotalRecurringEarning + employee.TotalVariableEarning;
            if (grossPay <= 0m)
            {
                summary.Blockers.Add($"{employee.EmployeeNameSnapshot}: gaji bruto belum terhitung sehingga potongan cicilan tidak dapat direncanakan.");
                continue;
            }

            var statutoryDeductions = employee.TotalTax + employee.TotalEmployeeInsuranceContribution;
            var otherPriorDeductions = employee.Components
                .Where(c => !c.IsDelete
                    && c.ComponentTypeSnapshot == "Deduction"
                    && c.ComponentCodeSnapshot != ComponentCode
                    && c.ComponentCodeSnapshot != "LOAN_HOSPITAL_AR")
                .Sum(c => c.Amount);

            var availableNetSalary = Math.Max(0m, grossPay - statutoryDeductions - otherPriorDeductions);

            // Batas persen gaji bersih dari master jenis potongan. Bila master tidak menentukan, dipakai persentase kebijakan terversi
            // (Payroll:Policy:InstallmentDefaultMaxNetSalaryPercentage; nilai bawaan 50 = perilaku sebelumnya, NEEDS CONFIRMATION),
            // bukan angka yang ditanam di kode. Batas nominal master tetap berlaku bila ada.
            var maxAllowed = availableNetSalary * ((deductionType?.MaxNetSalaryPercentage ?? _policy.InstallmentDefaultMaxNetSalaryPercentage) / 100m);
            if (deductionType?.MaximumAmount is > 0m)
                maxAllowed = Math.Min(maxAllowed, deductionType.MaximumAmount.Value);
            maxAllowed = Math.Max(0m, Math.Min(maxAllowed, availableNetSalary));

            var dueAmount = input.Amount;
            decimal deducted;
            string status;
            if (dueAmount <= maxAllowed) { deducted = dueAmount; status = PayrollResultStatuses.Berhasil; }
            else if (maxAllowed > 0m) { deducted = maxAllowed; status = PayrollResultStatuses.Sebagian; }
            else { deducted = 0m; status = PayrollResultStatuses.Gagal; }

            var planned = new PlannedDeduction(installment.Id, installment.DeductionPeriod, dueAmount, availableNetSalary, maxAllowed, deducted, status);
            var detailJson = JsonSerializer.Serialize(planned);

            var component = employee.Components
                .FirstOrDefault(c => !c.IsDelete && c.SourceType == ComponentSourceType && c.SourceId == input.Id);
            if (component is not null)
            {
                component.Amount = deducted;
                component.CalculationDetailJson = detailJson;
                component.UpdateDateTime = DateTime.UtcNow;
                component.UpdateBy = actorUserId;
            }
            else
            {
                _dbContext.PrlPayrollEmployeeComponents.Add(new PrlPayrollEmployeeComponent
                {
                    PayrollRunEmployeeId = employee.Id,
                    PayrollComponentId = input.PayrollComponentId,
                    SourceType = ComponentSourceType,
                    SourceId = input.Id,
                    ComponentCodeSnapshot = ComponentCode,
                    ComponentNameSnapshot = "Potongan Cicilan Piutang RS",
                    ComponentTypeSnapshot = "Deduction",
                    CalculationMethodSnapshot = "BalanceBased",
                    CurrencyCode = employee.CurrencyCode,
                    Amount = deducted,
                    IsDisplayedOnPayslip = true,
                    CalculationDetailJson = detailJson,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                });
            }

            employee.TotalDeduction = statutoryDeductions + otherPriorDeductions + deducted;
            employee.NetPay = grossPay - employee.TotalDeduction;
            employee.UpdateDateTime = DateTime.UtcNow;
            employee.UpdateBy = actorUserId;

            AddItem(summary, employee, planned);
        }

        return summary;
    }

    // ------------------------------------------------------------------------------------
    // 2. KONFIRMASI ke Finance (hanya saat run disahkan)
    // ------------------------------------------------------------------------------------

    public async Task<PayrollRunInstallmentDeductionSummaryResponse> ConfirmRunAsync(
        PrlPayrollRun run, Guid actorUserId, CancellationToken cancellationToken)
    {
        var employees = await _dbContext.PrlPayrollRunEmployees
            .Include(x => x.Components)
            .Include(x => x.VariableInputs)
            .Where(x => x.PayrollRunId == run.Id && !x.IsDelete)
            .ToListAsync(cancellationToken);

        var summary = NewSummary(run, employees.Count);

        foreach (var employee in employees.OrderBy(x => x.Id))
        {
            var input = employee.VariableInputs
                .FirstOrDefault(x => x.SourceType == InstallmentSourceType && !x.IsDelete && x.SourceId.HasValue);
            if (input is null) continue;

            var component = employee.Components
                .FirstOrDefault(c => !c.IsDelete && c.SourceType == ComponentSourceType && c.SourceId == input.Id);
            var planned = ReadPlanned(component);
            if (planned is null)
                throw new PayrollResultValidationException(
                    $"Potongan cicilan {employee.EmployeeNameSnapshot} belum dihitung. Hitung ulang payroll run sebelum mengesahkan.");

            // Kunci yang sama dengan penerima hasil Finance (ProcessResultAsync), supaya jalur otomatis dan batch manual tidak
            // memproses angsuran yang sama bersamaan.
            if (_dbContext.Database.IsRelational())
            {
                await _dbContext.Database.ExecuteSqlRawAsync(
                    "SELECT pg_advisory_xact_lock(hashtext({0}));",
                    [$"FIN_INSTALLMENT_PAYROLL_RESULT_{planned.InstallmentId:N}"], cancellationToken);
            }

            var financeResult = await _syncService.ApplyResultCoreAsync(new PayrollResultRequest
            {
                InstallmentId = planned.InstallmentId,
                PayrollPeriodId = run.PayrollPeriodId,
                Status = planned.Status,
                DeductedAmount = planned.DeductedAmount,
                ExecutionTimestamp = DateTimeOffset.UtcNow,
                PayrollRunId = run.Id,
                Notes = $"Potongan payroll run {run.RunNumber} disahkan ({planned.Status})"
            }, actorUserId, cancellationToken);

            input.InputStatus = InputApplied;
            input.UpdateDateTime = DateTime.UtcNow;
            input.UpdateBy = actorUserId;

            AddItem(summary, employee, planned, financeResult.IsIdempotentReplay);
        }

        await _loggerService.AuditAsync(LogCategory, "RunDeductions.Confirmed",
            $"Hasil potongan cicilan run {run.RunNumber} dikonfirmasi ke Finance: {summary.TotalInstallmentsProcessed} angsuran, total terpotong Rp {summary.TotalDeductedAmount:N2}.",
            new { run.Id, summary.TotalInstallmentsProcessed, summary.TotalDeductedAmount, ActorUserId = actorUserId });

        return summary;
    }

    // ------------------------------------------------------------------------------------
    // Pembantu
    // ------------------------------------------------------------------------------------

    private static PlannedDeduction? ReadPlanned(PrlPayrollEmployeeComponent? component)
    {
        if (component is null || string.IsNullOrWhiteSpace(component.CalculationDetailJson)) return null;
        try { return JsonSerializer.Deserialize<PlannedDeduction>(component.CalculationDetailJson); }
        catch (JsonException) { return null; }
    }

    /// <summary>Baris potongan cicilan pada slip untuk tampilan HRD: rencana dari hitung terakhir, dan apakah sudah dikonfirmasi ke Finance.</summary>
    public static PayrollRunInstallmentLineResponse? ReadLine(PrlPayrollRunEmployee employee)
    {
        var input = employee.VariableInputs
            .FirstOrDefault(x => x.SourceType == InstallmentSourceType && !x.IsDelete && x.SourceId.HasValue);
        if (input is null) return null;

        var component = employee.Components
            .FirstOrDefault(c => !c.IsDelete && c.SourceType == ComponentSourceType && c.SourceId == input.Id);
        return BuildLine(input, component);
    }

    /// <summary>
    /// Baris potongan dari satu input angsuran dan komponen rencananya. Dipakai layar run, daftar potongan, dan rekonsiliasi supaya
    /// ketiganya membaca angka yang sama persis.
    /// </summary>
    public static PayrollRunInstallmentLineResponse BuildLine(PrlPayrollVariableInput input, PrlPayrollEmployeeComponent? component)
    {
        var planned = ReadPlanned(component);
        return new PayrollRunInstallmentLineResponse
        {
            InstallmentId = input.SourceId!.Value,
            DueAmount = planned?.DueAmount ?? input.Amount,
            DeductionPeriod = planned?.DeductionPeriod,
            AvailableNetSalary = planned?.AvailableNetSalary,
            MaxAllowedDeduction = planned?.MaxAllowed,
            PlannedDeduction = planned?.DeductedAmount,
            PlannedStatus = planned?.Status,
            IsCalculated = planned is not null,
            IsConfirmedToFinance = string.Equals(input.InputStatus, InputApplied, StringComparison.OrdinalIgnoreCase)
        };
    }

    private static PayrollRunInstallmentDeductionSummaryResponse NewSummary(PrlPayrollRun run, int employeeCount) => new()
    {
        PayrollRunId = run.Id,
        RunNumber = run.RunNumber,
        PeriodCode = run.PayrollPeriod?.PayrollPeriodCode ?? string.Empty,
        TotalEmployeesProcessed = employeeCount
    };

    private static void AddItem(
        PayrollRunInstallmentDeductionSummaryResponse summary, PrlPayrollRunEmployee employee, PlannedDeduction planned, bool replay = false)
    {
        summary.TotalInstallmentsProcessed++;
        summary.TotalDueAmount += planned.DueAmount;
        summary.TotalDeductedAmount += planned.DeductedAmount;
        if (planned.Status == PayrollResultStatuses.Berhasil) summary.TotalSuccessCount++;
        else if (planned.Status == PayrollResultStatuses.Sebagian) summary.TotalPartialCount++;
        else if (planned.Status == PayrollResultStatuses.Gagal) summary.TotalFailedCount++;

        summary.Items.Add(new PayrollEmployeeInstallmentDeductionResponse
        {
            PayrollRunEmployeeId = employee.Id,
            PayrollRunId = employee.PayrollRunId,
            EmployeeId = employee.EmployeeId ?? Guid.Empty,
            EmployeeName = employee.EmployeeNameSnapshot,
            EmployeeNumber = employee.EmployeeNumberSnapshot ?? string.Empty,
            InstallmentId = planned.InstallmentId,
            DeductionPeriod = planned.DeductionPeriod,
            DueAmount = planned.DueAmount,
            AvailableNetSalary = planned.AvailableNetSalary,
            MaxAllowedDeduction = planned.MaxAllowed,
            DeductedAmount = planned.DeductedAmount,
            RemainingUnpaidAmount = planned.DueAmount - planned.DeductedAmount,
            Status = planned.Status,
            IsIdempotentReplay = replay
        });
    }
}
