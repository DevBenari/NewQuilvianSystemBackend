using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// Rekap dan rekonsiliasi penggajian (PAY-BE-10). MURNI BACA.
///
/// Rekap: agregat per periode/kategori penerima/pusat biaya/komponen. Tidak ada gaji per orang; nominal agregat hanya dikirim bila
/// pemanggil berwenang ViewAmount, sehingga rekap tidak membocorkan data gaji.
/// Rekonsiliasi: memeriksa bahwa total run = rincian penerima = rincian komponen = instruksi pembayaran = hasil bank = potongan piutang
/// di Finance, dan melaporkan apa adanya bahwa event Accounting untuk Payroll BELUM tersedia (mapping COA belum disahkan,
/// NEEDS CONFIRMATION). Tidak ada jurnal yang dibuat atau diposting dari sini.
/// </summary>
public sealed class PayrollReportService
{
    private const decimal Tolerance = 0.01m;
    private const int MaxDetails = 20;
    private const string InstallmentComponentCode = "FINANCE_INSTALLMENT_DEDUCTION";
    private const string PersonEmployee = "Karyawan";
    private const string PersonDoctor = "Dokter";

    private readonly ApplicationDbContext _dbContext;

    public PayrollReportService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // ------------------------------------------------------------------------------------
    // Rekap
    // ------------------------------------------------------------------------------------

    public async Task<PayrollRecapResponse> GetRecapAsync(PayrollRecapQuery query, bool includeAmounts, CancellationToken cancellationToken)
    {
        var runQuery = _dbContext.TrxPayrollRuns.AsNoTracking().Include(x => x.PayrollPeriod)
            .Where(x => !x.IsDelete && x.RunStatus != PayrollRunStatuses.Cancelled && x.RunStatus != PayrollRunStatuses.Reversed);
        if (query.PayrollRunId.HasValue) runQuery = runQuery.Where(x => x.Id == query.PayrollRunId.Value);
        if (query.PayrollPeriodId.HasValue) runQuery = runQuery.Where(x => x.PayrollPeriodId == query.PayrollPeriodId.Value);
        var runs = await runQuery.OrderByDescending(x => x.CreateDateTime).ToListAsync(cancellationToken);
        var runIds = runs.Select(r => r.Id).ToList();

        var employeeQuery = _dbContext.TrxPayrollRunEmployees.AsNoTracking()
            .Where(e => !e.IsDelete && runIds.Contains(e.PayrollRunId));
        if (query.CostCenterId.HasValue) employeeQuery = employeeQuery.Where(e => e.CostCenterId == query.CostCenterId.Value);
        var employees = await employeeQuery
            .Select(e => new { e.Id, e.PayrollRunId, e.EmployeeId, e.CostCenterId, e.GrossPay, e.TotalDeduction, e.NetPay })
            .ToListAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(query.PersonType))
        {
            var wantEmployee = string.Equals(query.PersonType, PersonEmployee, StringComparison.OrdinalIgnoreCase);
            employees = employees.Where(e => e.EmployeeId.HasValue == wantEmployee).ToList();
        }

        decimal? Amount(decimal value) => includeAmounts ? (decimal?)value : null;

        var response = new PayrollRecapResponse
        {
            AmountsVisible = includeAmounts,
            RunCount = runs.Count,
            EmployeeCount = employees.Count,
            GrossPay = Amount(employees.Sum(e => e.GrossPay)),
            TotalDeduction = Amount(employees.Sum(e => e.TotalDeduction)),
            NetPay = Amount(employees.Sum(e => e.NetPay))
        };

        foreach (var run in runs)
        {
            var list = employees.Where(e => e.PayrollRunId == run.Id).ToList();
            response.Runs.Add(new PayrollRecapRunResponse
            {
                PayrollRunId = run.Id,
                RunNumber = run.RunNumber,
                RunStatus = run.RunStatus,
                PeriodCode = run.PayrollPeriod?.PayrollPeriodCode ?? string.Empty,
                EmployeeCount = list.Count,
                GrossPay = Amount(list.Sum(e => e.GrossPay)),
                TotalDeduction = Amount(list.Sum(e => e.TotalDeduction)),
                NetPay = Amount(list.Sum(e => e.NetPay)),
                PaidAmount = Amount(run.TotalPaidAmount)
            });
        }
        response.PaidAmount = Amount(runs.Sum(r => r.TotalPaidAmount));

        response.ByCategory = employees
            .GroupBy(e => e.EmployeeId.HasValue ? PersonEmployee : PersonDoctor)
            .OrderBy(g => g.Key)
            .Select(g => new PayrollRecapGroupResponse
            {
                Key = g.Key,
                Label = g.Key,
                EmployeeCount = g.Count(),
                GrossPay = Amount(g.Sum(e => e.GrossPay)),
                TotalDeduction = Amount(g.Sum(e => e.TotalDeduction)),
                NetPay = Amount(g.Sum(e => e.NetPay))
            }).ToList();

        var costCenterIds = employees.Where(e => e.CostCenterId.HasValue).Select(e => e.CostCenterId!.Value).Distinct().ToList();
        var costCenterNames = await _dbContext.MstCostCenters.AsNoTracking()
            .Where(c => costCenterIds.Contains(c.Id))
            .Select(c => new { c.Id, c.CostCenterCode, c.CostCenterName })
            .ToDictionaryAsync(c => c.Id, c => $"{c.CostCenterCode} - {c.CostCenterName}", cancellationToken);
        response.ByCostCenter = employees
            .GroupBy(e => e.CostCenterId)
            .Select(g => new PayrollRecapGroupResponse
            {
                Key = g.Key?.ToString() ?? "NONE",
                Label = g.Key.HasValue && costCenterNames.TryGetValue(g.Key.Value, out var name) ? name : "Tanpa pusat biaya",
                EmployeeCount = g.Count(),
                GrossPay = Amount(g.Sum(e => e.GrossPay)),
                TotalDeduction = Amount(g.Sum(e => e.TotalDeduction)),
                NetPay = Amount(g.Sum(e => e.NetPay))
            }).OrderBy(g => g.Label).ToList();

        var employeeIds = employees.Select(e => e.Id).ToList();
        if (employeeIds.Count > 0)
        {
            var components = await _dbContext.TrxPayrollEmployeeComponents.AsNoTracking()
                .Where(c => !c.IsDelete && employeeIds.Contains(c.PayrollRunEmployeeId))
                .GroupBy(c => new { c.ComponentCodeSnapshot, c.ComponentNameSnapshot, c.ComponentTypeSnapshot })
                .Select(g => new
                {
                    g.Key.ComponentCodeSnapshot,
                    g.Key.ComponentNameSnapshot,
                    g.Key.ComponentTypeSnapshot,
                    EmployeeCount = g.Select(x => x.PayrollRunEmployeeId).Distinct().Count(),
                    Amount = g.Sum(x => x.Amount)
                })
                .ToListAsync(cancellationToken);
            response.ByComponent = components
                .OrderBy(c => c.ComponentTypeSnapshot).ThenBy(c => c.ComponentNameSnapshot)
                .Select(c => new PayrollRecapComponentResponse
                {
                    ComponentCode = c.ComponentCodeSnapshot,
                    ComponentName = c.ComponentNameSnapshot,
                    ComponentType = c.ComponentTypeSnapshot,
                    EmployeeCount = c.EmployeeCount,
                    Amount = Amount(c.Amount)
                }).ToList();
        }
        return response;
    }

    // ------------------------------------------------------------------------------------
    // Rekonsiliasi satu run
    // ------------------------------------------------------------------------------------

    public async Task<PayrollReconciliationResponse> GetReconciliationAsync(Guid runId, CancellationToken cancellationToken)
    {
        var run = await _dbContext.TrxPayrollRuns.AsNoTracking().Include(x => x.PayrollPeriod)
            .SingleOrDefaultAsync(x => x.Id == runId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Payroll run tidak ditemukan.");

        var employees = await _dbContext.TrxPayrollRunEmployees.AsNoTracking()
            .Include(x => x.Components)
            .Where(x => x.PayrollRunId == runId && !x.IsDelete)
            .ToListAsync(cancellationToken);

        var response = new PayrollReconciliationResponse
        {
            PayrollRunId = run.Id,
            RunNumber = run.RunNumber,
            RunStatus = run.RunStatus,
            PeriodCode = run.PayrollPeriod?.PayrollPeriodCode ?? string.Empty,
            GeneratedAt = DateTime.UtcNow
        };

        // 1. Total run = jumlah rincian penerima.
        response.Checks.Add(Compare("RUN_GROSS", "Total bruto run = jumlah bruto penerima", run.TotalGrossPay, employees.Sum(e => e.GrossPay)));
        response.Checks.Add(Compare("RUN_DEDUCTION", "Total potongan run = jumlah potongan penerima", run.TotalDeduction, employees.Sum(e => e.TotalDeduction)));
        response.Checks.Add(Compare("RUN_NET", "Total bersih run = jumlah bersih penerima", run.TotalNetPay, employees.Sum(e => e.NetPay)));

        // 2. Rincian penerima = jumlah komponen (gross-to-net dapat ditelusuri).
        var componentCheck = new PayrollReconciliationCheckResponse
        {
            Code = "EMPLOYEE_VS_COMPONENTS",
            Title = "Bruto dan potongan setiap penerima = jumlah komponennya"
        };
        var hasComponents = employees.Any(e => e.Components.Any(c => !c.IsDelete));
        if (!hasComponents)
        {
            componentCheck.Status = PayrollReconciliationStatuses.Info;
            componentCheck.Message = "Run ini belum memiliki rincian komponen (dihitung sebelum mesin kalkulasi baru). Hitung ulang run yang masih terbuka untuk mendapatkan rincian.";
        }
        else
        {
            foreach (var employee in employees.OrderBy(e => e.EmployeeNameSnapshot))
            {
                var live = employee.Components.Where(c => !c.IsDelete).ToList();
                var earning = live.Where(c => IsType(c.ComponentTypeSnapshot, "Earning") || IsType(c.ComponentTypeSnapshot, "Benefit")).Sum(c => c.Amount);
                var deduction = live.Where(c => IsType(c.ComponentTypeSnapshot, "Deduction") || IsType(c.ComponentTypeSnapshot, "Tax") || IsType(c.ComponentTypeSnapshot, "Insurance")).Sum(c => c.Amount);
                if (Math.Abs(employee.GrossPay - earning) >= Tolerance || Math.Abs(employee.TotalDeduction - deduction) >= Tolerance)
                {
                    componentCheck.Difference = (componentCheck.Difference ?? 0m) + Math.Abs(employee.GrossPay - earning) + Math.Abs(employee.TotalDeduction - deduction);
                    if (componentCheck.Details.Count < MaxDetails)
                        componentCheck.Details.Add($"{employee.EmployeeNameSnapshot}: bruto Rp {employee.GrossPay:N0} vs komponen Rp {earning:N0}; potongan Rp {employee.TotalDeduction:N0} vs komponen Rp {deduction:N0}.");
                }
            }
            componentCheck.Status = componentCheck.Details.Count == 0 ? PayrollReconciliationStatuses.Ok : PayrollReconciliationStatuses.Mismatch;
            componentCheck.Message = componentCheck.Details.Count == 0
                ? "Seluruh penerima sama dengan jumlah komponennya."
                : $"Ada penerima yang angkanya tidak sama dengan jumlah komponennya (ditampilkan maksimal {MaxDetails}).";
        }
        response.Checks.Add(componentCheck);

        // 3 dan 4. Gaji bersih = instruksi pembayaran = hasil bank.
        var payments = await _dbContext.TrxPayrollPayments.AsNoTracking()
            .Where(p => p.PayrollRunId == runId && !p.IsDelete)
            .Select(p => new { p.PaymentStatus, p.PaymentAmount, p.NetPaidAmount })
            .ToListAsync(cancellationToken);
        var live2 = payments.Where(p => PayrollPaymentStatuses.IsLive(p.PaymentStatus)).ToList();
        var payable = employees.Sum(e => e.PaymentAmount);
        if (live2.Count == 0)
        {
            response.Checks.Add(new PayrollReconciliationCheckResponse
            {
                Code = "NET_VS_INSTRUCTION",
                Title = "Gaji bersih dibayarkan = instruksi pembayaran",
                Expected = payable,
                Status = PayrollReconciliationStatuses.Info,
                Message = "Belum ada instruksi pembayaran yang dikirim ke Finance untuk run ini."
            });
        }
        else
        {
            var instructed = live2.Sum(p => p.PaymentAmount);
            var instructionCheck = Compare("NET_VS_INSTRUCTION", "Gaji bersih dibayarkan = instruksi pembayaran", payable, instructed);
            if (instructionCheck.Status == PayrollReconciliationStatuses.Mismatch && PayrollRunStatuses.IsPaymentStage(run.RunStatus))
                instructionCheck.Message += " Mungkin ada penerima yang ditahan (rekening belum terverifikasi) atau instruksi belum dikirim.";
            response.Checks.Add(instructionCheck);

            var paid = live2.Sum(p => p.NetPaidAmount);
            var paidCheck = Compare("INSTRUCTION_VS_PAID", "Instruksi pembayaran = hasil bank terkonfirmasi", instructed, paid);
            if (paidCheck.Status == PayrollReconciliationStatuses.Mismatch && !string.Equals(run.RunStatus, PayrollRunStatuses.Paid, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(run.RunStatus, PayrollRunStatuses.Closed, StringComparison.OrdinalIgnoreCase))
            {
                paidCheck.Status = PayrollReconciliationStatuses.Info;
                paidCheck.Message = $"Pembayaran belum lunas (status run {run.RunStatus}); sisa menunggu hasil bank dari Finance.";
            }
            response.Checks.Add(paidCheck);
            response.Checks.Add(Compare("RUN_PAID_HEADER", "Total terbayar pada run = hasil bank", run.TotalPaidAmount, paid));
        }

        // 5. Potongan cicilan piutang: yang dikonfirmasi payroll = yang tercatat terpotong di Finance (setelah pembalikan).
        response.Checks.Add(await BuildDeductionCheckAsync(employees.Select(e => e.Id).ToList(), cancellationToken));

        // 6. Event Accounting untuk Payroll: belum ada (mapping COA belum disahkan). Dilaporkan jujur, tidak diposting dari sini.
        var accountingEvents = await _dbContext.FinAccountingEventOutboxes.AsNoTracking()
            .CountAsync(e => !e.IsDelete && e.SourceModule == "Payroll" && e.SourceTransactionId == run.RunNumber, cancellationToken);
        response.Checks.Add(new PayrollReconciliationCheckResponse
        {
            Code = "ACCOUNTING_EVENT",
            Title = "Event jurnal Accounting untuk Payroll",
            Actual = accountingEvents,
            Status = accountingEvents > 0 ? PayrollReconciliationStatuses.Info : PayrollReconciliationStatuses.NotAvailable,
            Message = accountingEvents > 0
                ? $"{accountingEvents} event Accounting tercatat untuk run ini."
                : "Belum ada event jurnal untuk Payroll. NEEDS CONFIRMATION: mapping COA (beban gaji, utang gaji, iuran, pajak, offset piutang) dan jenis event Finance/Accounting belum disahkan, sehingga tidak ada jurnal yang dibuat otomatis."
        });

        response.MismatchCount = response.Checks.Count(c => c.Status == PayrollReconciliationStatuses.Mismatch);
        response.OverallStatus = response.MismatchCount == 0 ? "OK" : "ADA_SELISIH";
        return response;
    }

    private async Task<PayrollReconciliationCheckResponse> BuildDeductionCheckAsync(
        List<Guid> runEmployeeIds, CancellationToken cancellationToken)
    {
        var check = new PayrollReconciliationCheckResponse
        {
            Code = "AR_DEDUCTION",
            Title = "Potongan cicilan terkonfirmasi = nominal terpotong di Finance"
        };

        var inputs = await _dbContext.TrxPayrollVariableInputs.AsNoTracking()
            .Include(x => x.PayrollRunEmployee)
            .Where(x => !x.IsDelete && runEmployeeIds.Contains(x.PayrollRunEmployeeId)
                && x.SourceType == PayrollVariableInputSources.FinanceReceivableInstallment && x.SourceId != null)
            .ToListAsync(cancellationToken);
        if (inputs.Count == 0)
        {
            check.Status = PayrollReconciliationStatuses.Info;
            check.Message = "Tidak ada angsuran piutang pada run ini.";
            return check;
        }

        var inputIds = inputs.Select(i => i.Id).ToList();
        var components = (await _dbContext.TrxPayrollEmployeeComponents.AsNoTracking()
                .Where(c => !c.IsDelete && c.ComponentCodeSnapshot == InstallmentComponentCode
                    && c.SourceId != null && inputIds.Contains(c.SourceId.Value))
                .ToListAsync(cancellationToken))
            .GroupBy(c => c.SourceId!.Value).ToDictionary(g => g.Key, g => g.First());
        var installmentIds = inputs.Select(i => i.SourceId!.Value).Distinct().ToList();
        var paidByInstallment = await _dbContext.FinReceivableInstallments.AsNoTracking()
            .Where(x => installmentIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.PaidAmount, cancellationToken);
        var reversals = (await _dbContext.TrxPayrollReversals.AsNoTracking()
                .Where(r => !r.IsDelete && r.ReversalStatus == PayrollReversalStatuses.Approved && r.FinanceReversalId == null
                    && r.PayrollRunEmployeeId != null && runEmployeeIds.Contains(r.PayrollRunEmployeeId.Value))
                .Select(r => new { r.PayrollRunEmployeeId, r.ReversalAmount })
                .ToListAsync(cancellationToken))
            .GroupBy(r => r.PayrollRunEmployeeId!.Value).ToDictionary(g => g.Key, g => g.Sum(r => r.ReversalAmount));

        decimal expected = 0m, actual = 0m;
        var confirmed = 0;
        foreach (var input in inputs.Where(i => string.Equals(i.InputStatus, "Applied", StringComparison.OrdinalIgnoreCase)))
        {
            confirmed++;
            components.TryGetValue(input.Id, out var component);
            reversals.TryGetValue(input.PayrollRunEmployeeId, out var reversed);
            var expectedPaid = Math.Max(0m, (component?.Amount ?? 0m) - reversed);
            var financePaid = paidByInstallment.GetValueOrDefault(input.SourceId!.Value);
            expected += expectedPaid;
            actual += financePaid;
            if (Math.Abs(expectedPaid - financePaid) >= Tolerance && check.Details.Count < MaxDetails)
                check.Details.Add($"{input.PayrollRunEmployee?.EmployeeNameSnapshot}: terkonfirmasi Rp {expectedPaid:N0} vs Finance Rp {financePaid:N0}.");
        }

        if (confirmed == 0)
        {
            check.Status = PayrollReconciliationStatuses.Info;
            check.Message = $"{inputs.Count} angsuran sudah masuk run tetapi belum ada yang dikonfirmasi ke Finance (run belum disahkan).";
            return check;
        }

        check.Expected = expected;
        check.Actual = actual;
        check.Difference = actual - expected;
        check.Status = check.Details.Count == 0 && Math.Abs(expected - actual) < Tolerance
            ? PayrollReconciliationStatuses.Ok : PayrollReconciliationStatuses.Mismatch;
        check.Message = check.Status == PayrollReconciliationStatuses.Ok
            ? $"{confirmed} angsuran terkonfirmasi sama dengan nominal terpotong di Finance."
            : "Ada angsuran yang nominal terkonfirmasinya tidak sama dengan nominal terpotong di Finance.";
        return check;
    }

    private static PayrollReconciliationCheckResponse Compare(string code, string title, decimal expected, decimal actual)
    {
        var difference = actual - expected;
        var ok = Math.Abs(difference) < Tolerance;
        return new PayrollReconciliationCheckResponse
        {
            Code = code,
            Title = title,
            Expected = expected,
            Actual = actual,
            Difference = difference,
            Status = ok ? PayrollReconciliationStatuses.Ok : PayrollReconciliationStatuses.Mismatch,
            Message = ok ? "Sama." : $"Selisih Rp {difference:N2} (harapan Rp {expected:N2}, aktual Rp {actual:N2})."
        };
    }

    private static bool IsType(string? type, string expected) => string.Equals(type, expected, StringComparison.OrdinalIgnoreCase);
}
