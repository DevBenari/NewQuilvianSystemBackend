using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// Pembacaan dan rekonsiliasi potongan cicilan piutang karyawan/dokter (PAY-BE-06): jadwal Finance, rencana potongan payroll,
/// hasil yang sudah dikonfirmasi ke Finance, pembalikan, dan sisa piutang. MURNI BACA: tidak pernah memotong, mengirim ulang hasil,
/// atau mengubah saldo piutang; pemotongan dan konfirmasi tetap hanya lewat siklus run (hitung dan sahkan). Rekonsiliasi hanya
/// menulis satu catatan audit bahwa ia dijalankan.
/// </summary>
public sealed class PayrollEmployeeDeductionService
{
    private const string LogCategory = "Corporate.PayrollManagement.EmployeeDeduction";
    private const string InstallmentSourceType = PayrollVariableInputSources.FinanceReceivableInstallment;
    private const string InstallmentComponentCode = "FINANCE_INSTALLMENT_DEDUCTION";
    private const string InputApplied = "Applied";

    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public PayrollEmployeeDeductionService(ApplicationDbContext dbContext, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    // ------------------------------------------------------------------------------------
    // Daftar potongan per angsuran pada run
    // ------------------------------------------------------------------------------------

    public async Task<PagedResult<PayrollEmployeeDeductionItemResponse>> GetPagedAsync(
        PayrollEmployeeDeductionQuery query, CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var inputs = _dbContext.TrxPayrollVariableInputs.AsNoTracking()
            .Where(x => !x.IsDelete && x.SourceType == InstallmentSourceType && x.SourceId != null
                && x.PayrollRunEmployee!.PayrollRun!.RunStatus != PayrollRunStatuses.Cancelled
                && x.PayrollRunEmployee!.PayrollRun!.RunStatus != PayrollRunStatuses.Reversed);
        if (query.PayrollRunId.HasValue)
            inputs = inputs.Where(x => x.PayrollRunEmployee!.PayrollRunId == query.PayrollRunId.Value);
        if (query.PayrollPeriodId.HasValue)
            inputs = inputs.Where(x => x.PayrollRunEmployee!.PayrollRun!.PayrollPeriodId == query.PayrollPeriodId.Value);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim()}%";
            inputs = inputs.Where(x => EF.Functions.ILike(x.PayrollRunEmployee!.EmployeeNameSnapshot, term)
                || EF.Functions.ILike(x.PayrollRunEmployee!.EmployeeNumberSnapshot!, term));
        }
        if (string.Equals(query.ProcessStatus, PayrollInstallmentProcessStatuses.Confirmed, StringComparison.OrdinalIgnoreCase))
            inputs = inputs.Where(x => x.InputStatus == InputApplied);
        else if (string.Equals(query.ProcessStatus, PayrollInstallmentProcessStatuses.Planned, StringComparison.OrdinalIgnoreCase)
                 || string.Equals(query.ProcessStatus, PayrollInstallmentProcessStatuses.NotCalculated, StringComparison.OrdinalIgnoreCase))
            inputs = inputs.Where(x => x.InputStatus != InputApplied);

        var total = await inputs.CountAsync(cancellationToken);
        var page = await inputs
            .Include(x => x.PayrollRunEmployee).ThenInclude(e => e!.PayrollRun).ThenInclude(r => r!.PayrollPeriod)
            .OrderByDescending(x => x.PayrollRunEmployee!.PayrollRun!.CreateDateTime)
            .ThenBy(x => x.PayrollRunEmployee!.EmployeeNameSnapshot)
            .ThenBy(x => x.InputNumber)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = await BuildItemsAsync(page, cancellationToken);

        // Filter NOT_CALCULATED vs PLANNED hanya dapat diputuskan setelah rencana dibaca; saring pada halaman ini.
        if (string.Equals(query.ProcessStatus, PayrollInstallmentProcessStatuses.Planned, StringComparison.OrdinalIgnoreCase))
            items = items.Where(i => i.ProcessStatus == PayrollInstallmentProcessStatuses.Planned).ToList();
        else if (string.Equals(query.ProcessStatus, PayrollInstallmentProcessStatuses.NotCalculated, StringComparison.OrdinalIgnoreCase))
            items = items.Where(i => i.ProcessStatus == PayrollInstallmentProcessStatuses.NotCalculated).ToList();

        return new PagedResult<PayrollEmployeeDeductionItemResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    private async Task<List<PayrollEmployeeDeductionItemResponse>> BuildItemsAsync(
        List<TrxPayrollVariableInput> page, CancellationToken cancellationToken)
    {
        if (page.Count == 0) return new List<PayrollEmployeeDeductionItemResponse>();

        var installmentIds = page.Select(x => x.SourceId!.Value).Distinct().ToList();
        var inputIds = page.Select(x => x.Id).ToList();
        var runEmployeeIds = page.Select(x => x.PayrollRunEmployeeId).Distinct().ToList();

        var installments = await _dbContext.FinReceivableInstallments.AsNoTracking()
            .Include(x => x.Plan)
            .Where(x => installmentIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        var receivableIds = installments.Values.Where(x => x.Plan != null).Select(x => x.Plan!.ReceivableId).Distinct().ToList();
        var receivableNumbers = await _dbContext.FinReceivables.AsNoTracking()
            .Where(r => receivableIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, r => r.ReceivableNumber, cancellationToken);
        var components = await _dbContext.TrxPayrollEmployeeComponents.AsNoTracking()
            .Where(c => !c.IsDelete && c.ComponentCodeSnapshot == InstallmentComponentCode
                && c.SourceId != null && inputIds.Contains(c.SourceId.Value))
            .ToListAsync(cancellationToken);
        var componentByInput = components.GroupBy(c => c.SourceId!.Value).ToDictionary(g => g.Key, g => g.First());
        var reversedByEmployee = await ApprovedReversalsAsync(runEmployeeIds, cancellationToken);

        var result = new List<PayrollEmployeeDeductionItemResponse>(page.Count);
        foreach (var input in page)
        {
            var employee = input.PayrollRunEmployee!;
            var run = employee.PayrollRun!;
            installments.TryGetValue(input.SourceId!.Value, out var installment);
            componentByInput.TryGetValue(input.Id, out var component);
            var line = PayrollInstallmentDeductionService.BuildLine(input, component);

            string process;
            if (line.IsConfirmedToFinance) process = PayrollInstallmentProcessStatuses.Confirmed;
            else if (!line.IsCalculated) process = PayrollInstallmentProcessStatuses.NotCalculated;
            else process = PayrollInstallmentProcessStatuses.Planned;

            var receivableNumber = installment?.Plan is not null && receivableNumbers.TryGetValue(installment.Plan.ReceivableId, out var number)
                ? number : string.Empty;
            reversedByEmployee.TryGetValue(employee.Id, out var reversed);

            result.Add(new PayrollEmployeeDeductionItemResponse
            {
                PayrollRunId = run.Id,
                RunNumber = run.RunNumber,
                RunStatus = run.RunStatus,
                PeriodCode = run.PayrollPeriod?.PayrollPeriodCode ?? string.Empty,
                DeductionPeriod = line.DeductionPeriod ?? installment?.DeductionPeriod ?? string.Empty,
                PayrollRunEmployeeId = employee.Id,
                WorkforceProfileId = employee.WorkforceProfileId,
                EmployeeNumber = employee.EmployeeNumberSnapshot ?? string.Empty,
                EmployeeName = employee.EmployeeNameSnapshot,
                InstallmentId = input.SourceId.Value,
                PlanNumber = installment?.Plan?.PlanNumber ?? string.Empty,
                ReceivableNumber = receivableNumber,
                InstallmentNumber = installment?.InstallmentNumber ?? 0,
                ScheduledAmount = installment?.ScheduledAmount ?? 0m,
                CarriedOverAmount = installment?.CarriedOverAmount ?? 0m,
                DueAmount = line.DueAmount,
                AvailableNetSalary = line.AvailableNetSalary,
                MaxAllowedDeduction = line.MaxAllowedDeduction,
                PlannedDeduction = line.PlannedDeduction,
                PlannedStatus = line.PlannedStatus,
                ShortfallReason = BuildShortfallReason(line),
                ProcessStatus = process,
                IsConfirmedToFinance = line.IsConfirmedToFinance,
                FinancePaidAmount = installment?.PaidAmount ?? 0m,
                FinanceOutstandingAmount = installment?.OutstandingAmount ?? 0m,
                InstallmentStatus = installment?.Status ?? string.Empty,
                ReversedAmount = reversed
            });
        }
        return result;
    }

    private static string? BuildShortfallReason(PayrollRunInstallmentLineResponse line)
    {
        if (!line.IsCalculated) return null;
        if (string.Equals(line.PlannedStatus, PayrollResultStatuses.Gagal, StringComparison.OrdinalIgnoreCase))
            return "Gaji bersih yang tersedia tidak memungkinkan potongan (batas potong Rp 0); seluruh angsuran tertunda ke periode berikutnya.";
        if (string.Equals(line.PlannedStatus, PayrollResultStatuses.Sebagian, StringComparison.OrdinalIgnoreCase))
        {
            var remaining = line.DueAmount - (line.PlannedDeduction ?? 0m);
            return $"Dipotong sebagian sampai batas potong (Rp {line.MaxAllowedDeduction ?? 0m:N0}); sisa Rp {remaining:N0} tertunda ke periode berikutnya.";
        }
        return null;
    }

    private async Task<Dictionary<Guid, decimal>> ApprovedReversalsAsync(List<Guid> runEmployeeIds, CancellationToken cancellationToken)
    {
        if (runEmployeeIds.Count == 0) return new Dictionary<Guid, decimal>();
        var rows = await _dbContext.TrxPayrollReversals.AsNoTracking()
            .Where(r => !r.IsDelete && r.ReversalStatus == PayrollReversalStatuses.Approved
                && r.PayrollRunEmployeeId != null && runEmployeeIds.Contains(r.PayrollRunEmployeeId.Value)
                && r.FinanceReversalId == null)
            .Select(r => new { r.PayrollRunEmployeeId, r.ReversalAmount })
            .ToListAsync(cancellationToken);
        return rows.GroupBy(r => r.PayrollRunEmployeeId!.Value).ToDictionary(g => g.Key, g => g.Sum(r => r.ReversalAmount));
    }

    // ------------------------------------------------------------------------------------
    // Rekonsiliasi periode (hanya baca)
    // ------------------------------------------------------------------------------------

    public Task<PayrollDeductionReconciliationResponse> GetSummaryAsync(Guid payrollPeriodId, CancellationToken cancellationToken) =>
        ComputeReconciliationAsync(payrollPeriodId, includeAllItems: false, cancellationToken);

    /// <summary>
    /// Rekonsiliasi periodik jadwal Finance terhadap rencana/hasil potongan payroll. Tidak membayar ulang dan tidak mengubah saldo
    /// piutang; hanya membandingkan dan mencatat bahwa rekonsiliasi dijalankan (audit).
    /// </summary>
    public async Task<PayrollDeductionReconciliationResponse> ReconcileAsync(
        Guid payrollPeriodId, Guid actorUserId, CancellationToken cancellationToken)
    {
        var result = await ComputeReconciliationAsync(payrollPeriodId, includeAllItems: true, cancellationToken);
        await _loggerService.AuditAsync(LogCategory, "Deduction.Reconciled",
            $"Rekonsiliasi potongan cicilan periode {result.DeductionPeriod}: {result.InstallmentCount} angsuran, {result.DiscrepancyCount} selisih.",
            new { payrollPeriodId, result.DiscrepancyCount, result.TotalDueAmount, result.TotalConfirmedDeduction, ActorUserId = actorUserId });
        return result;
    }

    private async Task<PayrollDeductionReconciliationResponse> ComputeReconciliationAsync(
        Guid payrollPeriodId, bool includeAllItems, CancellationToken cancellationToken)
    {
        var period = await _dbContext.MstPayrollPeriods.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == payrollPeriodId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Periode payroll tidak ditemukan.");
        var code = $"{period.FiscalYear:D4}-{period.PeriodNumber:D2}";

        var cutover = await _dbContext.MstPayrollDeductionCutovers.AsNoTracking()
            .FirstOrDefaultAsync(x => x.IsActive && !x.IsDelete, cancellationToken);
        var response = new PayrollDeductionReconciliationResponse
        {
            PayrollPeriodId = period.Id,
            PeriodCode = period.PayrollPeriodCode,
            DeductionPeriod = code,
            IsAutomaticPeriod = cutover is null || string.CompareOrdinal(code, cutover.AutomaticFromPeriod) >= 0,
            GeneratedAt = DateTime.UtcNow
        };

        var installments = await _dbContext.FinReceivableInstallments.AsNoTracking()
            .Include(x => x.Plan)
            .Where(x => !x.IsDelete && x.DeductionPeriod == code
                && x.Status != FinReceivableInstallmentStatuses.Dibatalkan
                && x.Plan != null && !x.Plan.IsDelete
                && (x.Plan.Status == FinReceivableInstallmentPlanStatuses.Disetujui
                    || x.Plan.Status == FinReceivableInstallmentPlanStatuses.Selesai))
            .OrderBy(x => x.Plan!.PlanNumber).ThenBy(x => x.InstallmentNumber)
            .ToListAsync(cancellationToken);
        if (installments.Count == 0) return response;

        var installmentIds = installments.Select(x => x.Id).ToList();
        var inputs = await _dbContext.TrxPayrollVariableInputs.AsNoTracking()
            .Include(x => x.PayrollRunEmployee).ThenInclude(e => e!.PayrollRun)
            .Where(x => !x.IsDelete && x.SourceType == InstallmentSourceType && x.SourceId != null
                && installmentIds.Contains(x.SourceId.Value)
                && x.PayrollRunEmployee!.PayrollRun!.PayrollPeriodId == period.Id
                && x.PayrollRunEmployee!.PayrollRun!.RunStatus != PayrollRunStatuses.Cancelled
                && x.PayrollRunEmployee!.PayrollRun!.RunStatus != PayrollRunStatuses.Reversed)
            .ToListAsync(cancellationToken);
        var inputByInstallment = inputs.GroupBy(i => i.SourceId!.Value)
            .ToDictionary(g => g.Key, g => g
                .OrderByDescending(i => string.Equals(i.InputStatus, InputApplied, StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(i => i.CreateDateTime).First());

        var inputIds = inputByInstallment.Values.Select(i => i.Id).ToList();
        var components = inputIds.Count == 0
            ? new List<TrxPayrollEmployeeComponent>()
            : await _dbContext.TrxPayrollEmployeeComponents.AsNoTracking()
                .Where(c => !c.IsDelete && c.ComponentCodeSnapshot == InstallmentComponentCode
                    && c.SourceId != null && inputIds.Contains(c.SourceId.Value))
                .ToListAsync(cancellationToken);
        var componentByInput = components.GroupBy(c => c.SourceId!.Value).ToDictionary(g => g.Key, g => g.First());

        var runEmployeeIds = inputByInstallment.Values.Select(i => i.PayrollRunEmployeeId).Distinct().ToList();
        var reversedByEmployee = await ApprovedReversalsAsync(runEmployeeIds, cancellationToken);

        var receivableIds = installments.Select(x => x.Plan!.ReceivableId).Distinct().ToList();
        var receivables = await _dbContext.FinReceivables.AsNoTracking()
            .Where(r => receivableIds.Contains(r.Id))
            .Select(r => new { r.Id, r.ReceivableNumber, r.BenefitOwnerId, r.BenefitOwnerWorkforceProfileId })
            .ToDictionaryAsync(r => r.Id, cancellationToken);
        var ownerEmployeeIds = receivables.Values.Where(r => r.BenefitOwnerId.HasValue).Select(r => r.BenefitOwnerId!.Value).Distinct().ToList();
        var ownerWorkforceIds = receivables.Values.Where(r => r.BenefitOwnerWorkforceProfileId.HasValue)
            .Select(r => r.BenefitOwnerWorkforceProfileId!.Value).Distinct().ToList();
        var employees = await _dbContext.MstEmployees.AsNoTracking()
            .Where(x => !x.IsDelete && (ownerEmployeeIds.Contains(x.Id) || ownerWorkforceIds.Contains(x.WorkforceProfileId)))
            .Select(x => new { x.Id, x.WorkforceProfileId, x.EmployeeNumber, x.FullName })
            .ToListAsync(cancellationToken);
        var doctors = await _dbContext.MstDoctors.AsNoTracking()
            .Where(x => !x.IsDelete && ownerWorkforceIds.Contains(x.WorkforceProfileId))
            .Select(x => new { x.WorkforceProfileId, x.DoctorNumber, x.FullName })
            .ToListAsync(cancellationToken);

        foreach (var installment in installments)
        {
            receivables.TryGetValue(installment.Plan!.ReceivableId, out var receivable);
            string name = "Pemilik belum teridentifikasi", number = string.Empty;
            var employee = employees.FirstOrDefault(e =>
                (receivable?.BenefitOwnerId.HasValue == true && e.Id == receivable.BenefitOwnerId!.Value)
                || (receivable?.BenefitOwnerWorkforceProfileId.HasValue == true && e.WorkforceProfileId == receivable.BenefitOwnerWorkforceProfileId!.Value));
            if (employee is not null) { name = employee.FullName; number = employee.EmployeeNumber; }
            else if (receivable?.BenefitOwnerWorkforceProfileId.HasValue == true)
            {
                var doctor = doctors.FirstOrDefault(d => d.WorkforceProfileId == receivable.BenefitOwnerWorkforceProfileId!.Value);
                if (doctor is not null) { name = doctor.FullName; number = doctor.DoctorNumber; }
            }

            var due = installment.ScheduledAmount + installment.CarriedOverAmount;
            var item = new PayrollDeductionReconciliationItemResponse
            {
                InstallmentId = installment.Id,
                PlanNumber = installment.Plan.PlanNumber,
                ReceivableNumber = receivable?.ReceivableNumber ?? string.Empty,
                EmployeeName = name,
                EmployeeNumber = number,
                DueAmount = due,
                FinancePaidAmount = installment.PaidAmount,
                FinanceOutstandingAmount = installment.OutstandingAmount
            };

            if (!inputByInstallment.TryGetValue(installment.Id, out var input))
            {
                item.Code = "NOT_IN_RUN";
                item.Message = "Angsuran terjadwal di Finance tetapi belum masuk payroll run periode ini.";
            }
            else
            {
                componentByInput.TryGetValue(input.Id, out var component);
                var line = PayrollInstallmentDeductionService.BuildLine(input, component);
                item.RunNumber = input.PayrollRunEmployee?.PayrollRun?.RunNumber;
                item.PlannedDeduction = line.PlannedDeduction ?? 0m;
                reversedByEmployee.TryGetValue(input.PayrollRunEmployeeId, out var reversed);
                item.ReversedAmount = reversed;

                if (!line.IsCalculated && !line.IsConfirmedToFinance)
                {
                    item.Code = "NOT_CALCULATED";
                    item.Message = "Angsuran sudah masuk run tetapi payroll belum dihitung.";
                }
                else if (!line.IsConfirmedToFinance)
                {
                    item.Code = "PLANNED";
                    item.Message = "Rencana potongan sudah dihitung; saldo piutang baru berkurang saat run disahkan.";
                }
                else if (!line.IsCalculated)
                {
                    item.Code = "APPLIED_WITHOUT_PLAN";
                    item.Message = "Hasil sudah dikonfirmasi ke Finance tetapi rencana potongan tidak ditemukan pada komponen slip.";
                }
                else
                {
                    item.ConfirmedDeduction = line.PlannedDeduction ?? 0m;
                    item.ExpectedFinancePaid = Math.Max(0m, item.ConfirmedDeduction - item.ReversedAmount);
                    if (item.ExpectedFinancePaid == installment.PaidAmount)
                    {
                        item.Code = "CONFIRMED_OK";
                        item.Message = "Terkonfirmasi dan sama dengan nominal terpotong menurut Finance.";
                    }
                    else
                    {
                        item.Code = "CONFIRMED_MISMATCH";
                        item.Message = $"Terkonfirmasi Rp {item.ConfirmedDeduction:N0} dikurangi pembalikan Rp {item.ReversedAmount:N0} = Rp {item.ExpectedFinancePaid:N0}, tetapi Finance mencatat terpotong Rp {installment.PaidAmount:N0}.";
                    }
                }
            }

            response.InstallmentCount++;
            response.TotalDueAmount += item.DueAmount;
            response.TotalPlannedDeduction += item.PlannedDeduction;
            response.TotalConfirmedDeduction += item.ConfirmedDeduction;
            response.TotalReversedAmount += item.ReversedAmount;
            response.TotalExpectedFinancePaid += item.ExpectedFinancePaid;
            response.TotalFinancePaidAmount += item.FinancePaidAmount;
            response.TotalFinanceOutstandingAmount += item.FinanceOutstandingAmount;
            response.CountByCode[item.Code] = response.CountByCode.GetValueOrDefault(item.Code) + 1;

            // Selisih nyata = apa pun selain "selesai sesuai" dan "rencana sah menunggu pengesahan".
            var isDiscrepancy = item.Code is "NOT_IN_RUN" or "NOT_CALCULATED" or "APPLIED_WITHOUT_PLAN" or "CONFIRMED_MISMATCH";
            if (isDiscrepancy) response.DiscrepancyCount++;
            if (includeAllItems) response.Items.Add(item);
        }

        return response;
    }

    // ------------------------------------------------------------------------------------
    // Daftar restitusi salah potong
    // ------------------------------------------------------------------------------------

    public async Task<PagedResult<PayrollRestitutionListItemResponse>> GetRestitutionsAsync(
        PayrollRestitutionQuery query, CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var adjustments = _dbContext.TrxPayrollAdjustments.AsNoTracking()
            .Where(a => !a.IsDelete && a.SourceType == "FinanceReceivableCorrection");
        if (query.PayrollPeriodId.HasValue)
            adjustments = adjustments.Where(a => a.PayrollRun!.PayrollPeriodId == query.PayrollPeriodId.Value);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim()}%";
            adjustments = adjustments.Where(a => EF.Functions.ILike(a.PayrollRunEmployee!.EmployeeNameSnapshot, term)
                || EF.Functions.ILike(a.PayrollRunEmployee!.EmployeeNumberSnapshot!, term)
                || EF.Functions.ILike(a.AdjustmentNumber, term));
        }

        var total = await adjustments.CountAsync(cancellationToken);
        var rows = await adjustments
            .Include(a => a.PayrollRun).ThenInclude(r => r!.PayrollPeriod)
            .Include(a => a.PayrollRunEmployee)
            .OrderByDescending(a => a.CreateDateTime)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);

        var sourceIds = rows.Where(a => a.SourceId.HasValue).Select(a => a.SourceId!.Value).Distinct().ToList();
        var reversals = sourceIds.Count == 0
            ? new Dictionary<Guid, TrxPayrollReversal>()
            : (await _dbContext.TrxPayrollReversals.AsNoTracking()
                    .Where(r => !r.IsDelete && r.FinanceReversalId != null && sourceIds.Contains(r.FinanceReversalId.Value))
                    .ToListAsync(cancellationToken))
                .GroupBy(r => r.FinanceReversalId!.Value).ToDictionary(g => g.Key, g => g.First());

        var items = rows.Select(a =>
        {
            var run = a.PayrollRun!;
            TrxPayrollReversal? reversal = null;
            if (a.SourceId.HasValue) reversals.TryGetValue(a.SourceId.Value, out reversal);
            string placement;
            if (run.IsLocked || PayrollRunStatuses.IsFinalized(run.RunStatus)) placement = PayrollRestitutionPlacementStatuses.Locked;
            else if (string.Equals(run.RunStatus, PayrollRunStatuses.Calculated, StringComparison.OrdinalIgnoreCase)
                     || string.Equals(run.RunStatus, PayrollRunStatuses.Submitted, StringComparison.OrdinalIgnoreCase))
                placement = PayrollRestitutionPlacementStatuses.Calculated;
            else placement = PayrollRestitutionPlacementStatuses.AwaitingCalculation;

            return new PayrollRestitutionListItemResponse
            {
                AdjustmentId = a.Id,
                AdjustmentNumber = a.AdjustmentNumber,
                ReversalId = reversal?.Id,
                ReversalNumber = reversal?.ReversalNumber,
                SourceId = a.SourceId,
                SourceType = a.SourceType,
                PayrollRunId = run.Id,
                RunNumber = run.RunNumber,
                RunStatus = run.RunStatus,
                IsRunLocked = run.IsLocked,
                PeriodCode = run.PayrollPeriod?.PayrollPeriodCode ?? string.Empty,
                PayrollRunEmployeeId = a.PayrollRunEmployeeId,
                EmployeeNumber = a.PayrollRunEmployee?.EmployeeNumberSnapshot ?? string.Empty,
                EmployeeName = a.PayrollRunEmployee?.EmployeeNameSnapshot ?? string.Empty,
                Amount = a.Amount,
                Reason = a.Reason,
                PlacementStatus = placement,
                CreatedAt = a.CreateDateTime
            };
        }).ToList();

        return new PagedResult<PayrollRestitutionListItemResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }
}
