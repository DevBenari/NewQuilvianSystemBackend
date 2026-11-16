using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Options;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Data;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// Siklus payroll run milik modul Payroll: buka (Draft) -> impor HRD (CollectingInput) -> hitung (Calculated) -> ajukan (Submitted)
/// -> sahkan (Approved, terkunci) -> kirim ke Finance (SentToFinance) -> hasil bank (PartiallyPaid/Paid) -> tutup (Closed).
///
/// Prinsip (keputusan Finance-HRD yang dipertahankan):
/// - Payroll pemilik run dan hasil potongan; Finance pemilik piutang dan jadwal angsuran. Finance tidak membuat run.
/// - Saat run dibuka dan dihitung, jadwal angsuran periode itu DITARIK dari Finance (dan hanya periode sejak peralihan).
/// - Potongan dihitung saat payroll dihitung, tetapi saldo piutang HANYA berkurang saat run DISAHKAN, atomik dalam satu
///   transaksi serializable: bila satu hasil gagal diterima Finance, pengesahan dibatalkan seluruhnya. Titik pengakuan penyelesaian
///   ini sengaja tidak diubah; apakah seharusnya menunggu konfirmasi pembayaran adalah NEEDS CONFIRMATION (pemilik Finance).
/// - Run yang sudah disahkan terkunci. Koreksi lewat pembalikan Finance dengan approval, bukan menghitung ulang.
///
/// Perhitungan gaji didelegasikan ke <see cref="PayrollCalculationService"/> (gaji pokok dari penetapan gaji disetujui dengan prorata,
/// tunjangan, lembur, kehadiran, input variabel, pajak/iuran dari tetapan profil). Cicilan piutang dipotong sesudahnya.
/// </summary>
public sealed class PayrollRunService
{
    // String kategori audit sengaja dipertahankan (bukan namespace) agar riwayat audit payroll run tidak terpecah.
    private const string LogCategory = "Corporate.HumanResource.PayrollManagement.PayrollRun";
    private const string RegularRunType = "Regular";
    private const string EmployeeCalculated = "Calculated";
    private const string EmployeeError = "Error";
    private const string EmployeeFinalized = "Finalized";
    private const string InstallmentComponentCode = "FINANCE_INSTALLMENT_DEDUCTION";

    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;
    private readonly FinanceReceivablePayrollSyncService _syncService;
    private readonly PayrollInstallmentDeductionService _deductionService;
    private readonly PayrollCalculationService _calculationService;
    private readonly PayrollPolicyOptions _policy;

    public PayrollRunService(
        ApplicationDbContext dbContext,
        LoggerService loggerService,
        FinanceReceivablePayrollSyncService syncService,
        PayrollInstallmentDeductionService deductionService,
        PayrollCalculationService calculationService,
        IOptions<PayrollPolicyOptions> policyOptions)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
        _syncService = syncService;
        _deductionService = deductionService;
        _calculationService = calculationService;
        _policy = policyOptions.Value;
    }

    /// <summary>Warnings bertipe opsional supaya JSON lama (tanpa Warnings) tetap terbaca.</summary>
    private sealed record ValidationSummary(List<string> Blockers, List<string> SyncNotes, List<string>? Warnings = null);

    // ------------------------------------------------------------------------------------
    // Baca
    // ------------------------------------------------------------------------------------

    public async Task<PagedResult<PayrollRunListItemResponse>> GetPagedAsync(PayrollRunQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = _dbContext.PrlPayrollRuns.AsNoTracking().Include(x => x.PayrollPeriod).Where(x => !x.IsDelete);
        if (request.PayrollPeriodId.HasValue) query = query.Where(x => x.PayrollPeriodId == request.PayrollPeriodId.Value);
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var statuses = request.Status.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (statuses.Length == 1) query = query.Where(x => x.RunStatus == statuses[0]);
            else if (statuses.Length > 1) query = query.Where(x => statuses.Contains(x.RunStatus));
        }
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(x => EF.Functions.ILike(x.RunNumber, $"%{term}%"));
        }

        var total = await query.CountAsync(cancellationToken);
        var runs = await query.OrderByDescending(x => x.CreateDateTime)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedResult<PayrollRunListItemResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = runs.Select(r => MapListItem(r, new PayrollRunListItemResponse())).ToList()
        };
    }

    public async Task<PayrollRunDetailResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var run = await _dbContext.PrlPayrollRuns.AsNoTracking()
            .Include(x => x.PayrollPeriod)
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Payroll run tidak ditemukan.");

        var employees = await _dbContext.PrlPayrollRunEmployees.AsNoTracking()
            .Include(x => x.Components)
            .Include(x => x.VariableInputs)
            .Where(x => x.PayrollRunId == id && !x.IsDelete)
            .OrderBy(x => x.EmployeeNameSnapshot)
            .ToListAsync(cancellationToken);

        var detail = MapListItem(run, new PayrollRunDetailResponse());
        detail.Notes = run.Notes;
        detail.PeriodStartDate = run.PeriodStartDateSnapshot;
        detail.PeriodEndDate = run.PeriodEndDateSnapshot;
        detail.PaymentDate = run.PaymentDateSnapshot;
        detail.PolicyVersion = ReadPolicyVersion(run.PolicySnapshotJson);

        var validation = ReadValidation(run.ValidationSummaryJson);
        detail.Blockers = validation.Blockers;
        detail.InstallmentSyncNotes = validation.SyncNotes;
        detail.Warnings = validation.Warnings ?? new List<string>();

        var reversals = await _dbContext.PrlPayrollReversals.AsNoTracking()
            .Where(x => x.PayrollRunId == id && !x.IsDelete)
            .OrderByDescending(x => x.CreateDateTime)
            .ToListAsync(cancellationToken);

        var employeeLines = employees.ToDictionary(e => e.Id, e => PayrollInstallmentDeductionService.ReadLine(e));
        var confirmedIds = employeeLines.Values.Where(l => l is { IsConfirmedToFinance: true }).Select(l => l!.InstallmentId).Distinct().ToList();
        var financeDeducted = confirmedIds.Count == 0
            ? new Dictionary<Guid, decimal>()
            : await _dbContext.FinReceivableInstallments.AsNoTracking()
                .Where(x => confirmedIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.PaidAmount, cancellationToken);
        foreach (var line in employeeLines.Values)
        {
            if (line is not null && financeDeducted.TryGetValue(line.InstallmentId, out var paid)) line.FinanceDeductedAmount = paid;
        }

        detail.Employees = employees.Select(e => new PayrollRunEmployeeResponse
        {
            Id = e.Id,
            WorkforceProfileId = e.WorkforceProfileId,
            EmployeeNumber = e.EmployeeNumberSnapshot ?? string.Empty,
            EmployeeName = e.EmployeeNameSnapshot,
            EmployeePayrollStatus = e.EmployeePayrollStatus,
            BaseSalary = e.BaseSalary,
            TotalRecurringEarning = e.TotalRecurringEarning,
            TotalVariableEarning = e.TotalVariableEarning,
            TotalOvertimeAmount = e.TotalOvertimeAmount,
            TotalAttendanceAllowance = e.TotalAttendanceAllowance,
            GrossPay = e.GrossPay,
            TotalTax = e.TotalTax,
            TotalEmployeeInsuranceContribution = e.TotalEmployeeInsuranceContribution,
            TotalDeduction = e.TotalDeduction,
            NetPay = e.NetPay,
            ValidationMessage = e.ValidationResultJson,
            Installment = employeeLines[e.Id],
            ReversedAmount = reversals
                .Where(r => r.PayrollRunEmployeeId == e.Id && r.ReversalStatus == PayrollReversalStatuses.Approved)
                .Sum(r => r.ReversalAmount),
            HasPendingReversal = reversals.Any(r => r.PayrollRunEmployeeId == e.Id && r.ReversalStatus == PayrollReversalStatuses.Requested)
        }).ToList();

        var employeeById = employees.ToDictionary(e => e.Id);
        detail.Reversals = reversals.Select(r =>
        {
            employeeById.TryGetValue(r.PayrollRunEmployeeId ?? Guid.Empty, out var owner);
            return new PayrollReversalResponse
            {
                Id = r.Id,
                ReversalNumber = r.ReversalNumber,
                PayrollRunEmployeeId = r.PayrollRunEmployeeId,
                EmployeeName = owner?.EmployeeNameSnapshot ?? string.Empty,
                EmployeeNumber = owner?.EmployeeNumberSnapshot ?? string.Empty,
                ReversalType = r.ReversalType,
                ReversalStatus = r.ReversalStatus,
                ReversalAmount = r.ReversalAmount,
                Reason = r.ReversalReason,
                Notes = r.Notes,
                RequestedAt = r.CreateDateTime,
                PostedAt = r.PostedAt
            };
        }).ToList();

        var lines = detail.Employees.Where(e => e.Installment is not null).Select(e => e.Installment!).ToList();
        detail.InstallmentCount = lines.Count;
        detail.TotalInstallmentDue = lines.Sum(l => l.DueAmount);
        detail.TotalInstallmentPlanned = lines.Sum(l => l.PlannedDeduction ?? 0m);

        var payments = await _dbContext.PrlPayrollPayments.AsNoTracking()
            .Where(x => x.PayrollRunId == id && !x.IsDelete)
            .Select(x => new { x.PaymentStatus, x.PaymentAmount, x.NetPaidAmount })
            .ToListAsync(cancellationToken);
        if (payments.Count > 0)
        {
            var live = payments.Where(p => PayrollPaymentStatuses.IsLive(p.PaymentStatus)).ToList();
            detail.PaymentSummary = new PayrollRunPaymentSummaryResponse
            {
                InstructionCount = live.Count,
                ScheduledCount = live.Count(p => p.PaymentStatus == PayrollPaymentStatuses.Scheduled || p.PaymentStatus == PayrollPaymentStatuses.Processing),
                PaidCount = live.Count(p => p.PaymentStatus == PayrollPaymentStatuses.Paid || p.PaymentStatus == PayrollPaymentStatuses.PartiallyReversed),
                FailedCount = payments.Count(p => p.PaymentStatus == PayrollPaymentStatuses.Failed),
                TotalInstructionAmount = live.Sum(p => p.PaymentAmount),
                TotalPaidAmount = live.Sum(p => p.NetPaidAmount)
            };

            // PAY-BE-11: potongan piutang yang sudah dikonfirmasi saat run disahkan, tetapi gaji penerimanya belum lunas atau gagal.
            var deductionAttention = await PayrollSalaryPaymentStatusResolver.CountConfirmedDeductionAttentionAsync(_dbContext, id, cancellationToken);
            detail.PaymentSummary.ConfirmedDeductionPaymentPendingCount = deductionAttention.Pending;
            detail.PaymentSummary.ConfirmedDeductionPaymentFailedCount = deductionAttention.Failed;
        }

        detail.AvailableActions = BuildAvailableActions(run, detail);
        return detail;
    }

    /// <summary>
    /// Aksi yang boleh dijalankan menurut status run. Hanya bantuan tampilan: setiap endpoint aksi memeriksa ulang status dan
    /// kewenangan di backend. Izin Akses Role per aksi diperiksa terpisah oleh [AccessPermission].
    /// </summary>
    private static List<string> BuildAvailableActions(PrlPayrollRun run, PayrollRunDetailResponse detail)
    {
        var actions = new List<string>();
        var status = run.RunStatus;
        var ready = detail.Blockers.Count == 0 && detail.ErrorEmployeeCount == 0 && detail.TotalEmployeeCount > 0;

        if (!run.IsLocked && PayrollRunStatuses.AcceptsInputs(status))
        {
            actions.Add("ImportHrd");
            actions.Add("Calculate");
        }
        if (string.Equals(status, PayrollRunStatuses.Calculated, StringComparison.OrdinalIgnoreCase) && ready) actions.Add("Submit");
        if (string.Equals(status, PayrollRunStatuses.Submitted, StringComparison.OrdinalIgnoreCase))
        {
            actions.Add("Return");
            if (ready) actions.Add("Approve");
        }
        if (!run.IsLocked && !PayrollRunStatuses.IsFinalized(status)
            && !string.Equals(status, PayrollRunStatuses.Cancelled, StringComparison.OrdinalIgnoreCase)) actions.Add("Cancel");
        if (PayrollRunStatuses.AllowsFinanceHandoff(status)) actions.Add("FinanceHandoff");
        if (PayrollRunStatuses.AllowsDeductionReversal(status)) actions.Add("RequestReversal");
        if (PayrollRunStatuses.AllowsPayslipGeneration(status)) actions.Add("GeneratePayslips");
        if (string.Equals(status, PayrollRunStatuses.Paid, StringComparison.OrdinalIgnoreCase)) actions.Add("Close");
        return actions;
    }

    private static string? ReadPolicyVersion(string? policySnapshotJson)
    {
        if (string.IsNullOrWhiteSpace(policySnapshotJson)) return null;
        try
        {
            using var document = JsonDocument.Parse(policySnapshotJson);
            return document.RootElement.TryGetProperty("policyVersion", out var value) && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>Ringkasan jumlah run per status. Hanya jumlah; tidak ada nominal gaji.</summary>
    public async Task<PayrollRunSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var rows = await _dbContext.PrlPayrollRuns.AsNoTracking()
            .Where(x => !x.IsDelete)
            .GroupBy(x => x.RunStatus)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);
        var byStatus = rows.ToDictionary(r => r.Status, r => r.Count);
        int Count(params string[] statuses) => statuses.Sum(s => byStatus.GetValueOrDefault(s));

        return new PayrollRunSummaryResponse
        {
            TotalRuns = rows.Sum(r => r.Count),
            AwaitingActionCount = Count(PayrollRunStatuses.Draft, PayrollRunStatuses.CollectingInput,
                PayrollRunStatuses.Calculated, PayrollRunStatuses.Submitted),
            InPaymentCount = Count(PayrollRunStatuses.SentToFinance, PayrollRunStatuses.PartiallyPaid),
            CountByStatus = byStatus
        };
    }

    public static PayrollRunFilterMetadataResponse GetFilterMetadata() => new()
    {
        Statuses = new List<string>
        {
            PayrollRunStatuses.Draft, PayrollRunStatuses.CollectingInput, PayrollRunStatuses.Calculated, PayrollRunStatuses.Submitted,
            PayrollRunStatuses.Approved, PayrollRunStatuses.SentToFinance, PayrollRunStatuses.PartiallyPaid, PayrollRunStatuses.Paid,
            PayrollRunStatuses.Closed, PayrollRunStatuses.Cancelled, PayrollRunStatuses.Reversed
        },
        RunTypes = new List<string> { RegularRunType },
        DefaultPageSize = 20,
        PageSizeOptions = new List<int> { 10, 20, 50, 100 }
    };

    /// <summary>Periode bulanan aktif terbaru, untuk memilih periode pada layar Payroll Run.</summary>
    public async Task<List<PayrollRunPeriodOptionResponse>> GetPeriodOptionsAsync(CancellationToken cancellationToken)
    {
        var periods = await _dbContext.MstPayrollPeriods.AsNoTracking()
            .Where(x => !x.IsDelete && x.PeriodType == "Monthly")
            .OrderByDescending(x => x.FiscalYear).ThenByDescending(x => x.PeriodNumber)
            .Take(36)
            .ToListAsync(cancellationToken);
        return periods.Select(x => new PayrollRunPeriodOptionResponse
        {
            Id = x.Id,
            Code = x.PayrollPeriodCode,
            Name = x.PayrollPeriodName,
            DeductionPeriod = $"{x.FiscalYear:D4}-{x.PeriodNumber:D2}",
            IsLocked = x.IsLocked,
            IsActive = x.IsActive
        }).ToList();
    }

    /// <summary>
    /// Ringkasan satu periode untuk HRD: karyawan yang punya angsuran pada periode itu, jumlah sesuai perjanjian Finance, dan status
    /// pemrosesannya. Tanpa payroll run, periode berstatus "Menunggu Payroll Run" (tidak ada run palsu dari Finance).
    /// </summary>
    public async Task<PayrollRunPeriodOverviewResponse> GetPeriodOverviewAsync(Guid payrollPeriodId, CancellationToken cancellationToken)
    {
        var period = await _dbContext.MstPayrollPeriods.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == payrollPeriodId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Periode payroll tidak ditemukan.");
        var code = $"{period.FiscalYear:D4}-{period.PeriodNumber:D2}";

        var cutover = await _dbContext.MstPayrollDeductionCutovers.AsNoTracking()
            .FirstOrDefaultAsync(x => x.IsActive && !x.IsDelete, cancellationToken);
        var isAutomatic = cutover is null || string.CompareOrdinal(code, cutover.AutomaticFromPeriod) >= 0;

        var run = await _dbContext.PrlPayrollRuns.AsNoTracking()
            .Where(x => !x.IsDelete && x.PayrollPeriodId == period.Id && x.RunType == RegularRunType
                && x.RunStatus != PayrollRunStatuses.Cancelled && x.RunStatus != PayrollRunStatuses.Reversed)
            .OrderByDescending(x => x.CreateDateTime)
            .FirstOrDefaultAsync(cancellationToken);

        var overview = new PayrollRunPeriodOverviewResponse
        {
            PayrollPeriodId = period.Id,
            PeriodCode = period.PayrollPeriodCode,
            PeriodName = period.PayrollPeriodName,
            DeductionPeriod = code,
            IsLocked = period.IsLocked,
            AutomaticFromPeriod = cutover?.AutomaticFromPeriod,
            IsAutomaticPeriod = isAutomatic,
            RunId = run?.Id,
            RunNumber = run?.RunNumber,
            RunStatus = run?.RunStatus,
            PeriodStatus = !isAutomatic
                ? PayrollPeriodProcessStatuses.ManualFinance
                : run?.RunStatus ?? PayrollPeriodProcessStatuses.AwaitingPayrollRun
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
        if (installments.Count == 0) return overview;

        var receivableIds = installments.Select(x => x.Plan!.ReceivableId).Distinct().ToList();
        var receivables = await _dbContext.FinReceivables.AsNoTracking()
            .Where(r => receivableIds.Contains(r.Id))
            .Select(r => new { r.Id, r.ReceivableNumber, r.BenefitOwnerId, r.BenefitOwnerWorkforceProfileId })
            .ToDictionaryAsync(r => r.Id, cancellationToken);

        var ownerIds = receivables.Values.Where(r => r.BenefitOwnerId.HasValue).Select(r => r.BenefitOwnerId!.Value).Distinct().ToList();
        var workforceIds = receivables.Values.Where(r => r.BenefitOwnerWorkforceProfileId.HasValue)
            .Select(r => r.BenefitOwnerWorkforceProfileId!.Value).Distinct().ToList();
        var employees = await _dbContext.MstEmployees.AsNoTracking()
            .Where(x => !x.IsDelete && (ownerIds.Contains(x.Id) || workforceIds.Contains(x.WorkforceProfileId)))
            .Select(x => new { x.Id, x.WorkforceProfileId, x.EmployeeNumber, x.FullName })
            .ToListAsync(cancellationToken);
        var doctors = await _dbContext.MstDoctors.AsNoTracking()
            .Where(x => !x.IsDelete && workforceIds.Contains(x.WorkforceProfileId))
            .Select(x => new { x.WorkforceProfileId, x.DoctorNumber, x.FullName })
            .ToListAsync(cancellationToken);

        // Rencana/hasil potongan dibaca dari baris pegawai pada run (bila ada); tidak dihitung ulang di sini.
        var lines = new Dictionary<Guid, PayrollRunInstallmentLineResponse>();
        if (run is not null)
        {
            var runEmployees = await _dbContext.PrlPayrollRunEmployees.AsNoTracking()
                .Include(x => x.Components).Include(x => x.VariableInputs)
                .Where(x => x.PayrollRunId == run.Id && !x.IsDelete)
                .ToListAsync(cancellationToken);
            foreach (var runEmployee in runEmployees)
            {
                var line = PayrollInstallmentDeductionService.ReadLine(runEmployee);
                if (line is not null) lines[line.InstallmentId] = line;
            }
        }

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

            lines.TryGetValue(installment.Id, out var line);
            string process;
            if (!isAutomatic) process = PayrollInstallmentProcessStatuses.ManualFinance;
            else if (run is null) process = PayrollInstallmentProcessStatuses.AwaitingPayrollRun;
            else if (line is null) process = PayrollInstallmentProcessStatuses.NotInRun;
            else if (line.IsConfirmedToFinance) process = PayrollInstallmentProcessStatuses.Confirmed;
            else if (!line.IsCalculated) process = PayrollInstallmentProcessStatuses.NotCalculated;
            else process = PayrollInstallmentProcessStatuses.Planned;

            overview.Items.Add(new PayrollPeriodInstallmentResponse
            {
                InstallmentId = installment.Id,
                PlanNumber = installment.Plan.PlanNumber,
                ReceivableNumber = receivable?.ReceivableNumber ?? string.Empty,
                EmployeeName = name,
                EmployeeNumber = number,
                InstallmentNumber = installment.InstallmentNumber,
                ScheduledAmount = installment.ScheduledAmount,
                CarriedOverAmount = installment.CarriedOverAmount,
                DueAmount = installment.ScheduledAmount + installment.CarriedOverAmount,
                PaidAmount = installment.PaidAmount,
                InstallmentStatus = installment.Status,
                PlannedDeduction = line?.PlannedDeduction,
                PlannedStatus = line?.PlannedStatus,
                ProcessStatus = process
            });
        }

        overview.EmployeeCount = overview.Items.Select(x => x.EmployeeNumber + x.EmployeeName).Distinct().Count();
        overview.InstallmentCount = overview.Items.Count;
        overview.TotalDueAmount = overview.Items.Sum(x => x.DueAmount);
        overview.TotalPlannedDeduction = overview.Items.Sum(x => x.PlannedDeduction ?? 0m);
        return overview;
    }

    // ------------------------------------------------------------------------------------
    // Buka run
    // ------------------------------------------------------------------------------------

    public async Task<PayrollRunDetailResponse> CreateAsync(CreatePayrollRunRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var period = await _dbContext.MstPayrollPeriods.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == request.PayrollPeriodId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Periode payroll tidak ditemukan.");
        if (!period.IsActive) throw new PayrollRunValidationException("Periode payroll tidak aktif.");
        if (period.IsLocked) throw new PayrollRunValidationException("Periode payroll sudah terkunci.");

        var runId = await InTransactionAsync($"HR_PAYROLL_RUN_PERIOD_{period.Id:N}", async () =>
        {
            var exists = await _dbContext.PrlPayrollRuns.AnyAsync(x => !x.IsDelete
                && x.PayrollPeriodId == period.Id && x.RunType == RegularRunType
                && x.RunStatus != PayrollRunStatuses.Cancelled && x.RunStatus != PayrollRunStatuses.Reversed, cancellationToken);
            if (exists) throw new PayrollRunConflictException("Sudah ada payroll run Regular yang aktif untuk periode ini.");

            var run = new PrlPayrollRun
            {
                PayrollPeriodId = period.Id,
                LegalEntityId = period.LegalEntityId,
                HospitalSiteId = period.HospitalSiteId,
                RunNumber = $"PR-{period.FiscalYear}{period.PeriodNumber:D2}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}",
                RunType = RegularRunType,
                RunStatus = PayrollRunStatuses.Draft,
                CurrencyCode = "IDR",
                PeriodStartDateSnapshot = period.StartDate,
                PeriodEndDateSnapshot = period.EndDate,
                AttendanceCutoffDateSnapshot = period.AttendanceCutoffDate,
                VariableInputCutoffDateSnapshot = period.VariableInputCutoffDate,
                PaymentDateSnapshot = period.PaymentDate,
                Notes = request.Notes,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _dbContext.PrlPayrollRuns.Add(run);

            var seed = await SeedEmployeesAsync(run, period, actorUserId, cancellationToken);
            run.TotalEmployeeCount = seed.Added;

            // Baris run dan pegawai harus tersimpan dulu agar jadwal angsuran dapat ditautkan ke baris pegawai yang sah.
            await _dbContext.SaveChangesAsync(cancellationToken);

            var sync = await _syncService.SyncScheduledForRunAsync(period, actorUserId, cancellationToken);
            var notes = sync.SkipReasons.Distinct().ToList();
            if (seed.SkippedNoIdentity > 0)
                notes.Add($"{seed.SkippedNoIdentity} profil payroll aktif dilewati karena identitas pegawai/dokter tidak ditemukan.");
            run.ValidationSummaryJson = JsonSerializer.Serialize(new ValidationSummary(new List<string>(), notes));

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _loggerService.AuditAsync(LogCategory, "PayrollRun.Created",
                $"Payroll run {run.RunNumber} dibuka: {run.TotalEmployeeCount} pegawai, {sync.SyncedCount} angsuran ditarik dari Finance.",
                new { run.Id, run.RunNumber, PeriodId = period.Id, sync.SyncedCount, sync.SkippedCount, ActorUserId = actorUserId });
            return run.Id;
        }, cancellationToken);

        return await GetByIdAsync(runId, cancellationToken);
    }

    private sealed record PayrollSeedResult(int Added, int SkippedNoIdentity, int Repaired);

    /// <summary>
    /// Melengkapi baris pegawai dari profil payroll aktif dan eligible yang berlaku pada periode. Idempoten: pegawai yang sudah ada
    /// pada run ini TIDAK digandakan (indeks unik run+pegawai juga menjaganya), dan baris yang dibuat jalur lain tanpa profil
    /// (mis. restitusi) dilengkapi dengan profil payroll aktifnya. Dipanggil saat membuka dan saat menghitung run, sehingga run
    /// penampung tidak pernah disahkan dengan populasi yang tidak lengkap.
    /// </summary>
    private async Task<PayrollSeedResult> SeedEmployeesAsync(PrlPayrollRun run, MstPayrollPeriod period, Guid actorUserId, CancellationToken cancellationToken)
    {
        var profiles = await _dbContext.PrlPayrolls.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive && x.IsPayrollEligible && x.PayrollStatus == "Active"
                && (x.EffectiveStartDate == null || x.EffectiveStartDate <= period.EndDate)
                && (x.EffectiveEndDate == null || x.EffectiveEndDate >= period.StartDate))
            .OrderByDescending(x => x.EffectiveStartDate)
            .ToListAsync(cancellationToken);

        var chosen = profiles.GroupBy(x => x.WorkforceProfileId).Select(g => g.First()).ToList();
        var workforceIds = chosen.Select(x => x.WorkforceProfileId).ToList();

        var employees = await _dbContext.MstEmployees.AsNoTracking()
            .Where(x => !x.IsDelete && workforceIds.Contains(x.WorkforceProfileId))
            .Select(x => new { x.WorkforceProfileId, x.Id, x.EmployeeNumber, x.FullName })
            .ToListAsync(cancellationToken);
        var doctors = await _dbContext.MstDoctors.AsNoTracking()
            .Where(x => !x.IsDelete && workforceIds.Contains(x.WorkforceProfileId))
            .Select(x => new { x.WorkforceProfileId, x.DoctorNumber, x.FullName })
            .ToListAsync(cancellationToken);
        var employeeMap = employees.GroupBy(x => x.WorkforceProfileId).ToDictionary(g => g.Key, g => g.First());
        var doctorMap = doctors.GroupBy(x => x.WorkforceProfileId).ToDictionary(g => g.Key, g => g.First());

        var existingRows = await _dbContext.PrlPayrollRunEmployees
            .Where(x => x.PayrollRunId == run.Id && !x.IsDelete)
            .ToListAsync(cancellationToken);
        var existingByWorkforce = existingRows.GroupBy(x => x.WorkforceProfileId).ToDictionary(g => g.Key, g => g.First());

        var skipped = 0;
        var added = 0;
        var repaired = 0;
        foreach (var profile in chosen)
        {
            if (existingByWorkforce.TryGetValue(profile.WorkforceProfileId, out var existingRow))
            {
                if (!existingRow.PayrollProfileId.HasValue)
                {
                    existingRow.OrganizationAssignmentId = profile.OrganizationAssignmentId;
                    existingRow.PayrollProfileId = profile.Id;
                    existingRow.SalaryAssignmentId = profile.SalaryAssignmentId;
                    existingRow.BankAccountId = profile.BankAccountId;
                    existingRow.CostCenterId = profile.CostCenterId;
                    existingRow.SalaryStructureId = profile.SalaryStructureId;
                    existingRow.SalaryGradeId = profile.SalaryGradeId;
                    existingRow.UpdateDateTime = DateTime.UtcNow;
                    existingRow.UpdateBy = actorUserId;
                    repaired++;
                }
                continue;
            }

            Guid? employeeId = null;
            string number, name;
            if (employeeMap.TryGetValue(profile.WorkforceProfileId, out var emp)) { employeeId = emp.Id; number = emp.EmployeeNumber; name = emp.FullName; }
            else if (doctorMap.TryGetValue(profile.WorkforceProfileId, out var doc)) { number = doc.DoctorNumber; name = doc.FullName; }
            else { skipped++; continue; }
            added++;

            _dbContext.PrlPayrollRunEmployees.Add(new PrlPayrollRunEmployee
            {
                PayrollRunId = run.Id,
                WorkforceProfileId = profile.WorkforceProfileId,
                EmployeeId = employeeId,
                OrganizationAssignmentId = profile.OrganizationAssignmentId,
                PayrollProfileId = profile.Id,
                SalaryAssignmentId = profile.SalaryAssignmentId,
                BankAccountId = profile.BankAccountId,
                CostCenterId = profile.CostCenterId,
                SalaryStructureId = profile.SalaryStructureId,
                SalaryGradeId = profile.SalaryGradeId,
                EmployeePayrollStatus = "Pending",
                CurrencyCode = string.IsNullOrWhiteSpace(profile.CurrencyCode) ? "IDR" : profile.CurrencyCode,
                EmployeeNumberSnapshot = number,
                EmployeeNameSnapshot = name,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            });
        }
        return new PayrollSeedResult(added, skipped, repaired);
    }

    // ------------------------------------------------------------------------------------
    // Hitung
    // ------------------------------------------------------------------------------------

    public async Task<PayrollRunDetailResponse> CalculateAsync(Guid id, string? expectedRowVersion, Guid actorUserId, CancellationToken cancellationToken)
    {
        await InTransactionAsync($"HR_PAYROLL_RUN_{id:N}", async () =>
        {
            var run = await LoadRunAsync(id, cancellationToken);
            PayrollVersionToken.EnsureMatches(expectedRowVersion, run.CreateDateTime, run.UpdateDateTime, "Payroll run");
            if (run.IsLocked || !PayrollRunStatuses.AcceptsInputs(run.RunStatus))
                throw new PayrollRunValidationException($"Payroll run {run.RunNumber} berstatus {run.RunStatus} dan tidak dapat dihitung ulang.");
            if (run.PayrollPeriod is null || run.PayrollPeriod.IsLocked)
                throw new PayrollRunValidationException("Periode payroll sudah terkunci.");

            run.CalculationStartedAt = DateTime.UtcNow;

            // Populasi dilengkapi lebih dulu (pegawai baru yang eligible, baris penampung dari restitusi) supaya angsuran dan input
            // tertaut ke baris pegawai yang sah dan run tidak disahkan dengan populasi yang tidak lengkap.
            var seed = await SeedEmployeesAsync(run, run.PayrollPeriod, actorUserId, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Jadwal angsuran terbaru (mis. perjanjian baru disetujui setelah run dibuka) ditarik lagi; idempoten.
            var sync = await _syncService.SyncScheduledForRunAsync(run.PayrollPeriod, actorUserId, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Gross-to-net per penerima (gaji pokok prorata, tunjangan, lembur, kehadiran, input variabel, pajak/iuran).
            var calculation = await _calculationService.CalculateRunAsync(run, run.PayrollPeriod, actorUserId, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var blockers = new List<string>(calculation.Blockers);
            var warnings = new List<string>(calculation.Warnings);
            if (seed.SkippedNoIdentity > 0)
                warnings.Add($"{seed.SkippedNoIdentity} profil payroll aktif dilewati karena identitas pegawai/dokter tidak ditemukan.");

            // Potongan cicilan dihitung setelah gaji bruto dan potongan wajib ada. Belum menyentuh Finance.
            var deductions = await _deductionService.CalculateRunAsync(run, actorUserId, cancellationToken);
            blockers.AddRange(deductions.Blockers);

            var employees = await _dbContext.PrlPayrollRunEmployees
                .Where(x => x.PayrollRunId == run.Id && !x.IsDelete)
                .ToListAsync(cancellationToken);
            foreach (var employee in employees)
            {
                // Jumlah yang akan dibayarkan = gaji bersih setelah cicilan; penerima bermasalah tidak membawa nominal bayar.
                employee.PaymentAmount = employee.EmployeePayrollStatus == EmployeeCalculated ? employee.NetPay : 0m;
            }

            run.TotalEmployeeCount = employees.Count;
            run.ProcessedEmployeeCount = employees.Count(x => x.EmployeePayrollStatus == EmployeeCalculated);
            run.ErrorEmployeeCount = employees.Count(x => x.EmployeePayrollStatus == EmployeeError) + deductions.Blockers.Count;
            run.TotalBaseSalary = employees.Sum(x => x.BaseSalary);
            run.TotalEarning = employees.Sum(x => x.GrossPay);
            run.TotalGrossPay = employees.Sum(x => x.GrossPay);
            run.TotalTax = employees.Sum(x => x.TotalTax);
            run.TotalEmployeeContribution = employees.Sum(x => x.TotalEmployeeInsuranceContribution);
            run.TotalEmployerContribution = employees.Sum(x => x.TotalEmployerInsuranceContribution);
            run.TotalDeduction = employees.Sum(x => x.TotalDeduction);
            run.TotalNetPay = employees.Sum(x => x.NetPay);
            run.RunStatus = PayrollRunStatuses.Calculated;
            run.CalculatedAt = DateTime.UtcNow;
            run.CalculatedByUserId = actorUserId;
            run.PolicySnapshotJson = _calculationService.BuildPolicySnapshotJson();
            run.ValidationSummaryJson = JsonSerializer.Serialize(new ValidationSummary(
                blockers.Distinct().ToList(), sync.SkipReasons.Distinct().ToList(), warnings.Distinct().ToList()));
            run.UpdateDateTime = DateTime.UtcNow;
            run.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _loggerService.AuditAsync(LogCategory, "PayrollRun.Calculated",
                $"Payroll run {run.RunNumber} dihitung: {run.ProcessedEmployeeCount} pegawai, {deductions.TotalInstallmentsProcessed} potongan cicilan direncanakan, {blockers.Count} penahan.",
                new { run.Id, deductions.TotalDeductedAmount, Blockers = blockers.Count, ActorUserId = actorUserId });
            return run.Id;
        }, cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    // ------------------------------------------------------------------------------------
    // Ajukan, kembalikan, sahkan, batalkan
    // ------------------------------------------------------------------------------------

    public async Task<PayrollRunDetailResponse> SubmitAsync(Guid id, string? expectedRowVersion, Guid actorUserId, CancellationToken cancellationToken)
    {
        await InTransactionAsync($"HR_PAYROLL_RUN_{id:N}", async () =>
        {
            var run = await LoadRunAsync(id, cancellationToken);
            PayrollVersionToken.EnsureMatches(expectedRowVersion, run.CreateDateTime, run.UpdateDateTime, "Payroll run");
            if (run.RunStatus != PayrollRunStatuses.Calculated)
                throw new PayrollRunValidationException($"Hanya payroll run berstatus Calculated yang dapat diajukan (saat ini {run.RunStatus}).");
            await EnsureManualInputsSettledAsync(run, cancellationToken);
            EnsureReadyToDecide(run);

            run.RunStatus = PayrollRunStatuses.Submitted;
            run.SubmittedAt = DateTime.UtcNow;
            run.SubmittedByUserId = actorUserId;
            run.UpdateDateTime = DateTime.UtcNow;
            run.UpdateBy = actorUserId;
            AddApproval(run, actorUserId, "Submit", "Submitted", null);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _loggerService.AuditAsync(LogCategory, "PayrollRun.Submitted", $"Payroll run {run.RunNumber} diajukan untuk review.",
                new { run.Id, ActorUserId = actorUserId });
            return run.Id;
        }, cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    /// <summary>Mengembalikan run yang diajukan ke tahap hitung (ditolak reviewer). Belum ada efek ke Finance pada tahap ini.</summary>
    public async Task<PayrollRunDetailResponse> ReturnAsync(Guid id, string? reason, string? expectedRowVersion, Guid actorUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(reason)) throw new PayrollRunValidationException("Alasan pengembalian wajib diisi.");

        await InTransactionAsync($"HR_PAYROLL_RUN_{id:N}", async () =>
        {
            var run = await LoadRunAsync(id, cancellationToken);
            PayrollVersionToken.EnsureMatches(expectedRowVersion, run.CreateDateTime, run.UpdateDateTime, "Payroll run");
            if (run.RunStatus != PayrollRunStatuses.Submitted)
                throw new PayrollRunValidationException($"Hanya payroll run berstatus Submitted yang dapat dikembalikan (saat ini {run.RunStatus}).");

            run.RunStatus = PayrollRunStatuses.Calculated;
            run.SubmittedAt = null;
            run.SubmittedByUserId = null;
            run.UpdateDateTime = DateTime.UtcNow;
            run.UpdateBy = actorUserId;
            AddApproval(run, actorUserId, "Return", "Rejected", reason.Trim());

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _loggerService.AuditAsync(LogCategory, "PayrollRun.Returned", $"Payroll run {run.RunNumber} dikembalikan ke tahap hitung: {reason.Trim()}",
                new { run.Id, ActorUserId = actorUserId });
            return run.Id;
        }, cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    /// <summary>
    /// Mengesahkan run. Satu transaksi serializable: hasil potongan cicilan dikonfirmasi ke Finance (saldo piutang berkurang) dan run
    /// dikunci bersamaan. Bila satu hasil gagal diterima Finance, seluruh pengesahan dibatalkan. Idempoten: run yang sudah Approved
    /// dikembalikan apa adanya, tanpa mengirim ulang hasil.
    /// </summary>
    public async Task<PayrollRunDetailResponse> ApproveAsync(Guid id, string? expectedRowVersion, Guid actorUserId, CancellationToken cancellationToken)
    {
        await InTransactionAsync($"HR_PAYROLL_RUN_{id:N}", async () =>
        {
            var run = await LoadRunAsync(id, cancellationToken);
            // Idempoten: run yang sudah disahkan (atau sudah melewati pengesahan) dikembalikan apa adanya, tanpa mengirim ulang hasil.
            if (run.ApprovedAt.HasValue && PayrollRunStatuses.IsFinalized(run.RunStatus)) return run.Id;
            PayrollVersionToken.EnsureMatches(expectedRowVersion, run.CreateDateTime, run.UpdateDateTime, "Payroll run");
            if (run.RunStatus != PayrollRunStatuses.Submitted)
                throw new PayrollRunValidationException($"Hanya payroll run berstatus Submitted yang dapat disahkan (saat ini {run.RunStatus}).");
            // NEEDS CONFIRMATION (alur approval berjenjang): pemisahan pengaju dan pengesah baru berlaku bila kebijakan menyalakannya.
            if (_policy.RequireDifferentApprover && run.SubmittedByUserId.HasValue && run.SubmittedByUserId.Value == actorUserId)
                throw new PayrollForbiddenActionException("Pengaju run tidak boleh mengesahkan run yang sama (kebijakan pemisahan kewenangan).");
            EnsureReadyToDecide(run);

            var confirmed = await _deductionService.ConfirmRunAsync(run, actorUserId, cancellationToken);

            var now = DateTime.UtcNow;
            var employees = await _dbContext.PrlPayrollRunEmployees
                .Where(x => x.PayrollRunId == run.Id && !x.IsDelete).ToListAsync(cancellationToken);
            foreach (var employee in employees)
            {
                employee.PaymentAmount = employee.NetPay;
                employee.EmployeePayrollStatus = EmployeeFinalized;
                employee.IsFinalized = true;
                employee.FinalizedAt = now;
                employee.FinalizedByUserId = actorUserId;
                employee.IsSnapshotFrozen = true;
                employee.SnapshotFrozenAt = now;
                employee.SnapshotFrozenByUserId = actorUserId;
            }

            // Input manual yang disetujui menjadi Applied di transaksi pengesahan yang sama (PAY-BE-20, state-transition-matrix bagian 3).
            var appliedManualInputs = await MoveManualInputsAsync(run.Id,
                new[] { PayrollManualInputStatuses.Approved }, PayrollManualInputStatuses.Applied, reason: null, actorUserId, now, cancellationToken);

            run.RunStatus = PayrollRunStatuses.Approved;
            run.ApprovedAt = now;
            run.ApprovedByUserId = actorUserId;
            run.IsLocked = true;
            run.LockedAt = now;
            run.LockedByUserId = actorUserId;
            run.UpdateDateTime = now;
            run.UpdateBy = actorUserId;
            AddApproval(run, actorUserId, "Approve", "Approved", null);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _loggerService.AuditAsync(LogCategory, "PayrollRun.Approved",
                $"Payroll run {run.RunNumber} disahkan; {confirmed.TotalInstallmentsProcessed} hasil potongan cicilan (Rp {confirmed.TotalDeductedAmount:N2}) dikonfirmasi ke Finance.",
                new { run.Id, confirmed.TotalInstallmentsProcessed, confirmed.TotalDeductedAmount, AppliedManualInputs = appliedManualInputs, ActorUserId = actorUserId });
            return run.Id;
        }, cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    /// <summary>Membatalkan run SEBELUM disahkan. Run yang sudah disahkan tidak dibatalkan; koreksinya lewat pembalikan.</summary>
    public async Task<PayrollRunDetailResponse> CancelAsync(Guid id, string? reason, string? expectedRowVersion, Guid actorUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(reason)) throw new PayrollRunValidationException("Alasan pembatalan wajib diisi.");

        await InTransactionAsync($"HR_PAYROLL_RUN_{id:N}", async () =>
        {
            var run = await LoadRunAsync(id, cancellationToken);
            PayrollVersionToken.EnsureMatches(expectedRowVersion, run.CreateDateTime, run.UpdateDateTime, "Payroll run");
            if (PayrollRunStatuses.IsFinalized(run.RunStatus) || run.IsLocked)
                throw new PayrollRunValidationException("Payroll run yang sudah disahkan tidak dapat dibatalkan; gunakan pembalikan dengan approval.");
            if (run.RunStatus == PayrollRunStatuses.Cancelled) return run.Id;

            // Input manual yang belum Applied ikut dibatalkan di transaksi yang sama (PAY-BE-20). Rejected tetap Rejected (final).
            var cancelledManualInputs = await MoveManualInputsAsync(run.Id,
                new[] { PayrollManualInputStatuses.Draft, PayrollManualInputStatuses.Submitted, PayrollManualInputStatuses.Approved },
                PayrollManualInputStatuses.Cancelled, ManualInputRunCancelledReason, actorUserId, DateTime.UtcNow, cancellationToken);

            run.RunStatus = PayrollRunStatuses.Cancelled;
            run.Notes = string.IsNullOrWhiteSpace(run.Notes) ? $"Dibatalkan: {reason.Trim()}" : $"{run.Notes}\nDibatalkan: {reason.Trim()}";
            run.UpdateDateTime = DateTime.UtcNow;
            run.UpdateBy = actorUserId;
            AddApproval(run, actorUserId, "Cancel", "Cancelled", reason.Trim());

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _loggerService.AuditAsync(LogCategory, "PayrollRun.Cancelled", $"Payroll run {run.RunNumber} dibatalkan: {reason.Trim()}",
                new { run.Id, CancelledManualInputs = cancelledManualInputs, ActorUserId = actorUserId });
            return run.Id;
        }, cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    // ------------------------------------------------------------------------------------
    // Pembalikan potongan cicilan atas run yang sudah disahkan
    //
    // Koreksi sesudah pengesahan TIDAK menjalankan ulang potongan dan TIDAK mengubah slip yang sudah dibekukan. Pembalikan diajukan
    // (dengan alasan), disetujui, lalu Finance memulihkan saldo piutang lewat jalur koreksi bertanda "dari payroll run" dengan mutasi
    // PEMBALIKAN-POTONGAN-GAJI. Pengaju boleh sama dengan penyetuju; approval bertingkat menyusul. Pengembalian dana kepada pegawai
    // (restitusi lewat slip bulan berikutnya) bukan bagian langkah ini.
    // ------------------------------------------------------------------------------------

    public async Task<PayrollRunDetailResponse> RequestReversalAsync(
        Guid runId, Guid runEmployeeId, RequestPayrollReversalRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request?.Reason)) throw new PayrollRunValidationException("Alasan pembalikan wajib diisi.");

        await InTransactionAsync($"HR_PAYROLL_RUN_{runId:N}", async () =>
        {
            var run = await LoadRunAsync(runId, cancellationToken);
            if (!PayrollRunStatuses.AllowsDeductionReversal(run.RunStatus))
                throw new PayrollRunValidationException("Pembalikan hanya untuk payroll run yang sudah disahkan (belum ditutup atau dibatalkan). Run yang belum disahkan cukup dikembalikan atau dihitung ulang.");

            var employee = await _dbContext.PrlPayrollRunEmployees
                .Include(x => x.Components).Include(x => x.VariableInputs)
                .SingleOrDefaultAsync(x => x.Id == runEmployeeId && x.PayrollRunId == runId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pegawai tidak ditemukan pada payroll run ini.");

            var line = PayrollInstallmentDeductionService.ReadLine(employee);
            if (line is null || !line.IsConfirmedToFinance)
                throw new PayrollRunValidationException("Pegawai ini tidak memiliki potongan cicilan yang sudah dikonfirmasi ke Finance pada run ini.");

            var installment = await _dbContext.FinReceivableInstallments.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == line.InstallmentId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Angsuran Finance tidak ditemukan.");

            var pending = await _dbContext.PrlPayrollReversals.AnyAsync(x => !x.IsDelete
                && x.PayrollRunEmployeeId == runEmployeeId && x.ReversalStatus == PayrollReversalStatuses.Requested, cancellationToken);
            if (pending) throw new PayrollRunConflictException("Pembalikan untuk pegawai ini masih menunggu persetujuan.");

            var deducted = installment.PaidAmount;
            if (deducted <= 0m) throw new PayrollRunValidationException("Tidak ada potongan terpotong yang dapat dibalik.");
            var amount = request!.Amount ?? deducted;
            if (amount <= 0m || amount > deducted)
                throw new PayrollRunValidationException($"Nominal pembalikan harus lebih dari nol dan tidak melebihi nominal terpotong saat ini (Rp {deducted:N0}).");

            var reversal = new PrlPayrollReversal
            {
                PayrollRunId = run.Id,
                PayrollRunEmployeeId = employee.Id,
                ReversalNumber = $"REV-{run.RunNumber}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}",
                ReversalType = amount == deducted ? "Full" : "Partial",
                ReversalStatus = PayrollReversalStatuses.Requested,
                CurrencyCode = "IDR",
                ReversalAmount = amount,
                ReversalReason = request.Reason.Trim(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _dbContext.PrlPayrollReversals.Add(reversal);
            AddApproval(run, actorUserId, "ReversalRequest", "Requested", $"{reversal.ReversalNumber}: {reversal.ReversalReason}");

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _loggerService.AuditAsync(LogCategory, "PayrollRun.ReversalRequested",
                $"Pembalikan potongan {reversal.ReversalNumber} (Rp {amount:N2}) diajukan untuk {employee.EmployeeNameSnapshot} pada run {run.RunNumber}.",
                new { run.Id, ReversalId = reversal.Id, amount, ActorUserId = actorUserId });
            return reversal.Id;
        }, cancellationToken);

        return await GetByIdAsync(runId, cancellationToken);
    }

    /// <summary>
    /// Menyetujui pembalikan: Finance memulihkan saldo piutang (satu transaksi serializable dengan kunci yang sama dengan penerima hasil
    /// Finance). Idempoten: pembalikan yang sudah Approved dikembalikan apa adanya tanpa memulihkan dua kali.
    /// </summary>
    public async Task<PayrollRunDetailResponse> ApproveReversalAsync(Guid runId, Guid reversalId, Guid actorUserId, CancellationToken cancellationToken)
    {
        await InTransactionAsync($"HR_PAYROLL_RUN_{runId:N}", async () =>
        {
            var run = await LoadRunAsync(runId, cancellationToken);
            var reversal = await _dbContext.PrlPayrollReversals
                .SingleOrDefaultAsync(x => x.Id == reversalId && x.PayrollRunId == runId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pembalikan tidak ditemukan pada payroll run ini.");
            if (reversal.ReversalStatus == PayrollReversalStatuses.Approved) return reversal.Id;
            if (reversal.ReversalStatus != PayrollReversalStatuses.Requested)
                throw new PayrollRunValidationException($"Pembalikan berstatus {reversal.ReversalStatus} dan tidak dapat disetujui.");

            var employee = await _dbContext.PrlPayrollRunEmployees
                .Include(x => x.Components).Include(x => x.VariableInputs)
                .SingleOrDefaultAsync(x => x.Id == reversal.PayrollRunEmployeeId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pegawai pada pembalikan ini tidak ditemukan.");
            var line = PayrollInstallmentDeductionService.ReadLine(employee)
                ?? throw new PayrollRunValidationException("Potongan cicilan pegawai ini tidak ditemukan pada run.");

            if (_dbContext.Database.IsRelational())
            {
                await _dbContext.Database.ExecuteSqlRawAsync(
                    "SELECT pg_advisory_xact_lock(hashtext({0}));", [$"FIN_INSTALLMENT_PAYROLL_RESULT_{line.InstallmentId:N}"], cancellationToken);
            }

            var current = await _dbContext.FinReceivableInstallments.AsNoTracking()
                .Where(x => x.Id == line.InstallmentId && !x.IsDelete).Select(x => (decimal?)x.PaidAmount).SingleOrDefaultAsync(cancellationToken)
                ?? throw new KeyNotFoundException("Angsuran Finance tidak ditemukan.");
            var newAmount = current - reversal.ReversalAmount;
            if (newAmount < 0m)
                throw new PayrollRunValidationException($"Nominal terpotong saat ini (Rp {current:N0}) lebih kecil dari nominal pembalikan (Rp {reversal.ReversalAmount:N0}).");

            await _syncService.ApplyCorrectionCoreAsync(
                line.InstallmentId,
                newAmount,
                newAmount <= 0m ? PayrollResultStatuses.Gagal : PayrollResultStatuses.Sebagian,
                actorUserId,
                $"Pembalikan {reversal.ReversalNumber} run {run.RunNumber}: {reversal.ReversalReason}",
                cancellationToken,
                fromPayrollRun: true);

            var now = DateTime.UtcNow;
            reversal.ReversalStatus = PayrollReversalStatuses.Approved;
            reversal.ReversedAt = now;
            reversal.ReversedByUserId = actorUserId;
            reversal.PostedAt = now;
            reversal.PostedByUserId = actorUserId;
            reversal.UpdateDateTime = now;
            reversal.UpdateBy = actorUserId;
            AddApproval(run, actorUserId, "ReversalApprove", "Approved", reversal.ReversalNumber);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _loggerService.AuditAsync(LogCategory, "PayrollRun.ReversalApproved",
                $"Pembalikan potongan {reversal.ReversalNumber} (Rp {reversal.ReversalAmount:N2}) disetujui; saldo piutang dipulihkan di Finance.",
                new { run.Id, ReversalId = reversal.Id, reversal.ReversalAmount, ActorUserId = actorUserId });
            return reversal.Id;
        }, cancellationToken);

        return await GetByIdAsync(runId, cancellationToken);
    }

    public async Task<PayrollRunDetailResponse> RejectReversalAsync(
        Guid runId, Guid reversalId, string? reason, Guid actorUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(reason)) throw new PayrollRunValidationException("Alasan penolakan wajib diisi.");

        await InTransactionAsync($"HR_PAYROLL_RUN_{runId:N}", async () =>
        {
            var run = await LoadRunAsync(runId, cancellationToken);
            var reversal = await _dbContext.PrlPayrollReversals
                .SingleOrDefaultAsync(x => x.Id == reversalId && x.PayrollRunId == runId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pembalikan tidak ditemukan pada payroll run ini.");
            if (reversal.ReversalStatus != PayrollReversalStatuses.Requested)
                throw new PayrollRunValidationException($"Pembalikan berstatus {reversal.ReversalStatus} dan tidak dapat ditolak.");

            reversal.ReversalStatus = PayrollReversalStatuses.Rejected;
            reversal.Notes = $"Ditolak: {reason.Trim()}";
            reversal.UpdateDateTime = DateTime.UtcNow;
            reversal.UpdateBy = actorUserId;
            AddApproval(run, actorUserId, "ReversalReject", "Rejected", $"{reversal.ReversalNumber}: {reason.Trim()}");

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _loggerService.AuditAsync(LogCategory, "PayrollRun.ReversalRejected",
                $"Pembalikan potongan {reversal.ReversalNumber} ditolak: {reason.Trim()}", new { run.Id, ReversalId = reversal.Id, ActorUserId = actorUserId });
            return reversal.Id;
        }, cancellationToken);

        return await GetByIdAsync(runId, cancellationToken);
    }

    // ------------------------------------------------------------------------------------
    // Riwayat status, rincian penerima, dan penutupan
    // ------------------------------------------------------------------------------------

    /// <summary>
    /// Riwayat perpindahan status run (standar transaksi 5.4): diturunkan dari jejak persetujuan (<c>PrlPayrollApproval</c>) yang
    /// ditulis setiap aksi. Nama aktor dipetakan dari pengguna terautentikasi; tidak ada data yang diubah.
    /// </summary>
    public async Task<List<PayrollRunStatusHistoryItemResponse>> GetStatusHistoryAsync(Guid id, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.PrlPayrollRuns.AsNoTracking().AnyAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
        if (!exists) throw new KeyNotFoundException("Payroll run tidak ditemukan.");

        var rows = await _dbContext.PrlPayrollApprovals.AsNoTracking()
            .Where(x => x.PayrollRunId == id && !x.IsDelete)
            .OrderBy(x => x.CreateDateTime)
            .ToListAsync(cancellationToken);

        var actorIds = rows.Where(r => r.ActualActionByUserId.HasValue).Select(r => r.ActualActionByUserId!.Value).Distinct().ToList();
        var names = actorIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _dbContext.Users.AsNoTracking()
                .Where(u => actorIds.Contains(u.Id))
                .Select(u => new { u.Id, u.DisplayName })
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName, cancellationToken);

        return rows.Select(r => new PayrollRunStatusHistoryItemResponse
        {
            OccurredAt = r.ActionAt ?? r.CreateDateTime,
            Action = r.ActionType ?? string.Empty,
            Status = r.ApprovalStatus,
            ActorUserId = r.ActualActionByUserId,
            ActorName = r.ActualActionByUserId.HasValue && names.TryGetValue(r.ActualActionByUserId.Value, out var name) ? name : null,
            Comments = r.Comments
        }).ToList();
    }

    private const int MaxOvertimePageSize = 200;

    /// <summary>
    /// PAY-BE-23 (PGJ-C1, PRL-DEC-42): rekap lembur terverifikasi HR yang diimpor ke run, per pegawai per tanggal. Murni baca.
    /// Saringan baris sama dengan yang dipakai mesin hitung (aktif dan tidak terhapus), sehingga yang tampil adalah yang dihitung.
    /// Run tanpa lembur menghasilkan daftar kosong.
    /// </summary>
    public async Task<PagedResult<PayrollRunOvertimeInputResponse>> GetOvertimeInputsAsync(
        Guid id, PayrollRunOvertimeInputQuery query, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.PrlPayrollRuns.AsNoTracking().AnyAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
        if (!exists) throw new KeyNotFoundException("Payroll run tidak ditemukan.");

        var source = _dbContext.PrlPayrollOvertimeInputs.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive
                && x.PayrollRunEmployee != null
                && !x.PayrollRunEmployee.IsDelete
                && x.PayrollRunEmployee.PayrollRunId == id);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            source = source.Where(x =>
                x.PayrollRunEmployee!.EmployeeNameSnapshot.ToLower().Contains(term)
                || (x.PayrollRunEmployee.EmployeeNumberSnapshot != null && x.PayrollRunEmployee.EmployeeNumberSnapshot.ToLower().Contains(term)));
        }

        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxOvertimePageSize);
        var total = await source.CountAsync(cancellationToken);

        var items = await source
            .OrderBy(x => x.PayrollRunEmployee!.EmployeeNameSnapshot)
            .ThenBy(x => x.OvertimeDate)
            .ThenBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new PayrollRunOvertimeInputResponse
            {
                Id = x.Id,
                PayrollRunEmployeeId = x.PayrollRunEmployeeId,
                EmployeeNumber = x.PayrollRunEmployee!.EmployeeNumberSnapshot ?? string.Empty,
                EmployeeName = x.PayrollRunEmployee.EmployeeNameSnapshot,
                OvertimeDate = x.OvertimeDate,
                VerifiedMinutes = x.VerifiedMinutes,
                RateMultiplier = x.RateMultiplier,
                HourlyRate = x.HourlyRate,
                OvertimeAmount = x.OvertimeAmount
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<PayrollRunOvertimeInputResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    /// <summary>Rincian satu penerima: komponen gross-to-net dan agregat kehadiran. Rekening disamarkan; NPWP tidak dikirim.</summary>
    public async Task<PayrollRunEmployeeDetailResponse> GetEmployeeDetailAsync(Guid runId, Guid runEmployeeId, CancellationToken cancellationToken)
    {
        var employee = await _dbContext.PrlPayrollRunEmployees.AsNoTracking()
            .Include(x => x.Components)
            .Include(x => x.VariableInputs)
            .SingleOrDefaultAsync(x => x.Id == runEmployeeId && x.PayrollRunId == runId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Pegawai tidak ditemukan pada payroll run ini.");

        var line = PayrollInstallmentDeductionService.ReadLine(employee);
        var account = employee.BankAccountNumberSnapshot;
        return new PayrollRunEmployeeDetailResponse
        {
            Id = employee.Id,
            WorkforceProfileId = employee.WorkforceProfileId,
            EmployeeNumber = employee.EmployeeNumberSnapshot ?? string.Empty,
            EmployeeName = employee.EmployeeNameSnapshot,
            EmployeePayrollStatus = employee.EmployeePayrollStatus,
            BaseSalary = employee.BaseSalary,
            TotalRecurringEarning = employee.TotalRecurringEarning,
            TotalVariableEarning = employee.TotalVariableEarning,
            TotalOvertimeAmount = employee.TotalOvertimeAmount,
            TotalAttendanceAllowance = employee.TotalAttendanceAllowance,
            GrossPay = employee.GrossPay,
            TotalTax = employee.TotalTax,
            TotalEmployeeInsuranceContribution = employee.TotalEmployeeInsuranceContribution,
            TotalDeduction = employee.TotalDeduction,
            NetPay = employee.NetPay,
            ValidationMessage = employee.ValidationResultJson,
            Installment = line,
            ScheduledWorkMinutes = employee.ScheduledWorkMinutes,
            ActualWorkMinutes = employee.ActualWorkMinutes,
            LateMinutes = employee.LateMinutes,
            OvertimeMinutes = employee.OvertimeMinutes,
            PaidLeaveDays = employee.PaidLeaveDays,
            UnpaidLeaveDays = employee.UnpaidLeaveDays,
            AbsentDays = employee.AbsentDays,
            BankName = employee.BankNameSnapshot,
            BankAccountMasked = MaskAccount(account),
            TaxStatus = employee.TaxStatusSnapshot,
            IsSnapshotFrozen = employee.IsSnapshotFrozen,
            Components = employee.Components
                .Where(c => !c.IsDelete)
                .OrderBy(c => c.SortOrder).ThenBy(c => c.ComponentCodeSnapshot)
                .Select(c => new PayrollRunComponentLineResponse
                {
                    Id = c.Id,
                    ComponentCode = c.ComponentCodeSnapshot,
                    ComponentName = c.ComponentNameSnapshot,
                    ComponentType = c.ComponentTypeSnapshot,
                    SourceType = c.SourceType,
                    Quantity = c.Quantity,
                    Amount = c.Amount,
                    IsProrated = c.IsProrated,
                    IsManual = c.IsManual,
                    Formula = c.FormulaSnapshot,
                    SortOrder = c.SortOrder
                }).ToList()
        };
    }

    /// <summary>
    /// Menutup run yang sudah lunas dibayar. Syarat: status Paid dan tidak ada pembalikan yang masih menunggu keputusan.
    /// Closed tidak menghapus apa pun; ia hanya menandai run selesai sehingga tidak ada aksi pembayaran lagi.
    /// </summary>
    public async Task<PayrollRunDetailResponse> CloseAsync(Guid id, string? expectedRowVersion, Guid actorUserId, CancellationToken cancellationToken)
    {
        await InTransactionAsync($"HR_PAYROLL_RUN_{id:N}", async () =>
        {
            var run = await LoadRunAsync(id, cancellationToken);
            if (string.Equals(run.RunStatus, PayrollRunStatuses.Closed, StringComparison.OrdinalIgnoreCase)) return run.Id;
            PayrollVersionToken.EnsureMatches(expectedRowVersion, run.CreateDateTime, run.UpdateDateTime, "Payroll run");
            if (!string.Equals(run.RunStatus, PayrollRunStatuses.Paid, StringComparison.OrdinalIgnoreCase))
                throw new PayrollInvalidTransitionException($"Hanya payroll run berstatus Paid yang dapat ditutup (saat ini {run.RunStatus}).");

            var pendingReversal = await _dbContext.PrlPayrollReversals.AnyAsync(x => !x.IsDelete
                && x.PayrollRunId == run.Id && x.ReversalStatus == PayrollReversalStatuses.Requested, cancellationToken);
            if (pendingReversal)
                throw new PayrollInvalidTransitionException("Masih ada pembalikan yang menunggu keputusan; selesaikan sebelum menutup run.");

            var now = DateTime.UtcNow;
            run.RunStatus = PayrollRunStatuses.Closed;
            run.ClosedAt = now;
            run.ClosedByUserId = actorUserId;
            run.UpdateDateTime = now;
            run.UpdateBy = actorUserId;
            AddApproval(run, actorUserId, "Close", "Approved", null);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _loggerService.AuditAsync(LogCategory, "PayrollRun.Closed", $"Payroll run {run.RunNumber} ditutup.",
                new { run.Id, ActorUserId = actorUserId });
            return run.Id;
        }, cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    private static string? MaskAccount(string? account)
    {
        if (string.IsNullOrWhiteSpace(account)) return null;
        var digits = account.Trim();
        return digits.Length <= 4 ? new string('*', digits.Length) : new string('*', digits.Length - 4) + digits[^4..];
    }

    // ------------------------------------------------------------------------------------
    // Pembantu
    // ------------------------------------------------------------------------------------

    private async Task<PrlPayrollRun> LoadRunAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.PrlPayrollRuns.Include(x => x.PayrollPeriod)
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
        ?? throw new KeyNotFoundException("Payroll run tidak ditemukan.");

    /// <summary>Run siap diajukan/disahkan hanya bila tidak ada penahan dan ada pegawai yang terhitung.</summary>
    private static void EnsureReadyToDecide(PrlPayrollRun run)
    {
        var blockers = ReadValidation(run.ValidationSummaryJson).Blockers;
        if (blockers.Count > 0)
            throw new PayrollRunValidationException($"Payroll run memiliki {blockers.Count} penahan: {string.Join("; ", blockers.Take(3))}.");
        if (run.ErrorEmployeeCount > 0 || run.ProcessedEmployeeCount <= 0)
            throw new PayrollRunValidationException("Payroll run belum siap: masih ada pegawai bermasalah atau belum ada pegawai yang terhitung.");
    }

    /// <summary>Alasan sistem pada input manual yang ikut dibatalkan karena run-nya dibatalkan (state-transition-matrix bagian 1).</summary>
    private const string ManualInputRunCancelledReason = "Run dibatalkan";

    private IQueryable<PrlPayrollVariableInput> ManualInputsOfRun(Guid runId) =>
        _dbContext.PrlPayrollVariableInputs.Where(x =>
            !x.IsDelete
            && x.SourceType == PayrollVariableInputSources.ManualInput
            && x.PayrollRunEmployee != null
            && x.PayrollRunEmployee.PayrollRunId == runId);

    /// <summary>
    /// Penjaga pengajuan run (PAY-BE-20): VI-10 tidak boleh ada input manual Submitted; VI-11 tidak boleh ada input manual yang dibuat
    /// atau diubah sesudah perhitungan terakhir. Keduanya 409.
    /// </summary>
    private async Task EnsureManualInputsSettledAsync(PrlPayrollRun run, CancellationToken cancellationToken)
    {
        var pending = await ManualInputsOfRun(run.Id)
            .CountAsync(x => x.InputStatus == PayrollManualInputStatuses.Submitted, cancellationToken);
        if (pending > 0)
            throw new PayrollRunConflictException(
                $"Masih ada {pending} penghasilan variabel yang menunggu keputusan. Setujui atau tolak dulu.");

        if (run.CalculatedAt.HasValue)
        {
            var calculatedAt = run.CalculatedAt.Value;
            var changedAfterCalculation = await ManualInputsOfRun(run.Id)
                .AnyAsync(x => (x.UpdateDateTime ?? x.CreateDateTime) > calculatedAt, cancellationToken);
            if (changedAfterCalculation)
                throw new PayrollRunConflictException("Penghasilan variabel berubah setelah perhitungan terakhir. Hitung ulang run dulu.");
        }
    }

    /// <summary>Memindahkan status input manual run oleh sistem, di dalam transaksi pemanggil. Mengembalikan jumlah input yang dipindah.</summary>
    private async Task<int> MoveManualInputsAsync(
        Guid runId, string[] fromStatuses, string toStatus, string? reason, Guid actorUserId, DateTime now, CancellationToken cancellationToken)
    {
        var inputs = await ManualInputsOfRun(runId)
            .Where(x => fromStatuses.Contains(x.InputStatus))
            .ToListAsync(cancellationToken);

        foreach (var input in inputs)
        {
            input.InputStatus = toStatus;
            if (toStatus == PayrollManualInputStatuses.Cancelled)
            {
                input.IsCancel = true;
                input.CancelDateTime = now;
                input.CancelBy = actorUserId;
                input.DecisionReason = reason;
            }
            input.UpdateDateTime = now;
            input.UpdateBy = actorUserId;
        }

        return inputs.Count;
    }

    private void AddApproval(PrlPayrollRun run, Guid actorUserId, string actionType, string status, string? comments) =>
        _dbContext.PrlPayrollApprovals.Add(new PrlPayrollApproval
        {
            PayrollRunId = run.Id,
            ApprovalLevel = "Payroll",
            ApprovalStatus = status,
            ActionType = actionType,
            ActionAt = DateTime.UtcNow,
            ActualActionByUserId = actorUserId,
            Comments = comments,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        });

    private static ValidationSummary ReadValidation(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new ValidationSummary(new List<string>(), new List<string>());
        try { return JsonSerializer.Deserialize<ValidationSummary>(json) ?? new ValidationSummary(new List<string>(), new List<string>()); }
        catch (JsonException) { return new ValidationSummary(new List<string>(), new List<string>()); }
    }

    private static T MapListItem<T>(PrlPayrollRun run, T target) where T : PayrollRunListItemResponse
    {
        target.Id = run.Id;
        target.RunNumber = run.RunNumber;
        target.PayrollPeriodId = run.PayrollPeriodId;
        target.PeriodCode = run.PayrollPeriod?.PayrollPeriodCode ?? string.Empty;
        target.PeriodName = run.PayrollPeriod?.PayrollPeriodName ?? string.Empty;
        target.RunType = run.RunType;
        target.RunStatus = run.RunStatus;
        target.IsLocked = run.IsLocked;
        target.TotalEmployeeCount = run.TotalEmployeeCount;
        target.ErrorEmployeeCount = run.ErrorEmployeeCount;
        target.TotalGrossPay = run.TotalGrossPay;
        target.TotalDeduction = run.TotalDeduction;
        target.TotalNetPay = run.TotalNetPay;
        target.CalculatedAt = run.CalculatedAt;
        target.SubmittedAt = run.SubmittedAt;
        target.ApprovedAt = run.ApprovedAt;
        target.RowVersion = PayrollVersionToken.From(run.CreateDateTime, run.UpdateDateTime);
        return target;
    }

    /// <summary>Satu transaksi serializable dengan advisory lock per kunci; rollback penuh bila pekerjaan gagal.</summary>
    private async Task<T> InTransactionAsync<T>(string lockKey, Func<Task<T>> work, CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
                await _dbContext.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(hashtext({0}));", [lockKey], cancellationToken);
            }

            var result = await work();
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }
}
