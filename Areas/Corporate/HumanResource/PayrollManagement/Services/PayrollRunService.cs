using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Data;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services;

/// <summary>
/// Siklus minimum payroll run milik HRD: buka (Draft) -> hitung (Calculated) -> ajukan (Submitted) -> sahkan (Approved).
///
/// Prinsip (keputusan Finance-HRD):
/// - HRD pemilik run dan hasil potongan; Finance pemilik piutang dan jadwal angsuran. Finance tidak membuat run.
/// - Saat run dibuka dan dihitung, jadwal angsuran periode itu DITARIK dari Finance (dan hanya periode sejak peralihan).
/// - Potongan dihitung saat HRD menghitung payroll, tetapi saldo piutang HANYA berkurang saat run DISAHKAN, atomik dalam satu
///   transaksi serializable: bila satu hasil gagal diterima Finance, pengesahan dibatalkan seluruhnya.
/// - Run yang sudah disahkan terkunci. Koreksi lewat pembalikan Finance dengan approval, bukan menghitung ulang.
///
/// Perhitungan gaji di sini bersifat MINIMUM: gaji pokok dan tunjangan tetap dari profil payroll aktif (WfpPayroll), pajak dan
/// iuran pegawai memakai nilai profil. Input variabel lain (lembur, kehadiran, dll.) belum diikutkan; lihat laporan task.
/// </summary>
public sealed class PayrollRunService
{
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

    public PayrollRunService(
        ApplicationDbContext dbContext,
        LoggerService loggerService,
        FinanceReceivablePayrollSyncService syncService,
        PayrollInstallmentDeductionService deductionService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
        _syncService = syncService;
        _deductionService = deductionService;
    }

    private sealed record ValidationSummary(List<string> Blockers, List<string> SyncNotes);

    // ------------------------------------------------------------------------------------
    // Baca
    // ------------------------------------------------------------------------------------

    public async Task<PagedResult<PayrollRunListItemResponse>> GetPagedAsync(PayrollRunQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = _dbContext.TrxPayrollRuns.AsNoTracking().Include(x => x.PayrollPeriod).Where(x => !x.IsDelete);
        if (request.PayrollPeriodId.HasValue) query = query.Where(x => x.PayrollPeriodId == request.PayrollPeriodId.Value);
        if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.RunStatus == request.Status);
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
        var run = await _dbContext.TrxPayrollRuns.AsNoTracking()
            .Include(x => x.PayrollPeriod)
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Payroll run tidak ditemukan.");

        var employees = await _dbContext.TrxPayrollRunEmployees.AsNoTracking()
            .Include(x => x.Components)
            .Include(x => x.VariableInputs)
            .Where(x => x.PayrollRunId == id && !x.IsDelete)
            .OrderBy(x => x.EmployeeNameSnapshot)
            .ToListAsync(cancellationToken);

        var detail = MapListItem(run, new PayrollRunDetailResponse());
        detail.Notes = run.Notes;

        var validation = ReadValidation(run.ValidationSummaryJson);
        detail.Blockers = validation.Blockers;
        detail.InstallmentSyncNotes = validation.SyncNotes;

        var reversals = await _dbContext.TrxPayrollReversals.AsNoTracking()
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
        return detail;
    }

    /// <summary>Periode bulanan aktif terbaru, untuk memilih periode pada layar Payroll Run.</summary>
    public async Task<List<PayrollPeriodOptionResponse>> GetPeriodOptionsAsync(CancellationToken cancellationToken)
    {
        var periods = await _dbContext.MstPayrollPeriods.AsNoTracking()
            .Where(x => !x.IsDelete && x.PeriodType == "Monthly")
            .OrderByDescending(x => x.FiscalYear).ThenByDescending(x => x.PeriodNumber)
            .Take(36)
            .ToListAsync(cancellationToken);
        return periods.Select(x => new PayrollPeriodOptionResponse
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

        var run = await _dbContext.TrxPayrollRuns.AsNoTracking()
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
            var runEmployees = await _dbContext.TrxPayrollRunEmployees.AsNoTracking()
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
            var exists = await _dbContext.TrxPayrollRuns.AnyAsync(x => !x.IsDelete
                && x.PayrollPeriodId == period.Id && x.RunType == RegularRunType
                && x.RunStatus != PayrollRunStatuses.Cancelled && x.RunStatus != PayrollRunStatuses.Reversed, cancellationToken);
            if (exists) throw new PayrollRunConflictException("Sudah ada payroll run Regular yang aktif untuk periode ini.");

            var run = new TrxPayrollRun
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
            _dbContext.TrxPayrollRuns.Add(run);

            var skippedNoIdentity = await SeedEmployeesAsync(run, period, actorUserId, cancellationToken);
            run.TotalEmployeeCount = _dbContext.ChangeTracker.Entries<TrxPayrollRunEmployee>()
                .Count(x => x.State == EntityState.Added && x.Entity.PayrollRunId == run.Id);

            // Baris run dan pegawai harus tersimpan dulu agar jadwal angsuran dapat ditautkan ke baris pegawai yang sah.
            await _dbContext.SaveChangesAsync(cancellationToken);

            var sync = await _syncService.SyncScheduledForRunAsync(period, actorUserId, cancellationToken);
            var notes = sync.SkipReasons.Distinct().ToList();
            if (skippedNoIdentity > 0)
                notes.Add($"{skippedNoIdentity} profil payroll aktif dilewati karena identitas pegawai/dokter tidak ditemukan.");
            run.ValidationSummaryJson = JsonSerializer.Serialize(new ValidationSummary(new List<string>(), notes));

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _loggerService.AuditAsync(LogCategory, "PayrollRun.Created",
                $"Payroll run {run.RunNumber} dibuka: {run.TotalEmployeeCount} pegawai, {sync.SyncedCount} angsuran ditarik dari Finance.",
                new { run.Id, run.RunNumber, PeriodId = period.Id, sync.SyncedCount, sync.SkippedCount, ActorUserId = actorUserId });
            return run.Id;
        }, cancellationToken);

        return await GetByIdAsync(runId, cancellationToken);
    }

    /// <summary>Baris pegawai dari profil payroll aktif dan eligible yang berlaku pada periode. Mengembalikan jumlah profil yang dilewati.</summary>
    private async Task<int> SeedEmployeesAsync(TrxPayrollRun run, MstPayrollPeriod period, Guid actorUserId, CancellationToken cancellationToken)
    {
        var profiles = await _dbContext.WfpPayrolls.AsNoTracking()
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

        var skipped = 0;
        foreach (var profile in chosen)
        {
            Guid? employeeId = null;
            string number, name;
            if (employeeMap.TryGetValue(profile.WorkforceProfileId, out var emp)) { employeeId = emp.Id; number = emp.EmployeeNumber; name = emp.FullName; }
            else if (doctorMap.TryGetValue(profile.WorkforceProfileId, out var doc)) { number = doc.DoctorNumber; name = doc.FullName; }
            else { skipped++; continue; }

            _dbContext.TrxPayrollRunEmployees.Add(new TrxPayrollRunEmployee
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
        return skipped;
    }

    // ------------------------------------------------------------------------------------
    // Hitung
    // ------------------------------------------------------------------------------------

    public async Task<PayrollRunDetailResponse> CalculateAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
    {
        await InTransactionAsync($"HR_PAYROLL_RUN_{id:N}", async () =>
        {
            var run = await LoadRunAsync(id, cancellationToken);
            if (run.IsLocked || !PayrollRunStatuses.AcceptsInputs(run.RunStatus))
                throw new PayrollRunValidationException($"Payroll run {run.RunNumber} berstatus {run.RunStatus} dan tidak dapat dihitung ulang.");
            if (run.PayrollPeriod is null || run.PayrollPeriod.IsLocked)
                throw new PayrollRunValidationException("Periode payroll sudah terkunci.");

            run.CalculationStartedAt = DateTime.UtcNow;

            // Jadwal angsuran terbaru (mis. perjanjian baru disetujui setelah run dibuka) ditarik lagi; idempoten.
            var sync = await _syncService.SyncScheduledForRunAsync(run.PayrollPeriod, actorUserId, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var employees = await _dbContext.TrxPayrollRunEmployees
                .Include(x => x.Components)
                .Where(x => x.PayrollRunId == run.Id && !x.IsDelete)
                .ToListAsync(cancellationToken);

            var blockers = new List<string>();
            foreach (var employee in employees)
            {
                var profile = employee.PayrollProfileId.HasValue
                    ? await _dbContext.WfpPayrolls.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == employee.PayrollProfileId.Value && !x.IsDelete, cancellationToken)
                    : null;

                if (profile is null || !profile.IsPayrollEligible)
                {
                    MarkError(employee, "Profil payroll tidak ditemukan atau tidak eligible.", actorUserId);
                    blockers.Add($"{employee.EmployeeNameSnapshot}: profil payroll tidak ditemukan atau tidak eligible.");
                    continue;
                }

                employee.BaseSalary = profile.BaseSalary;
                employee.TotalRecurringEarning = profile.TotalAllowance;
                employee.GrossPay = employee.BaseSalary + employee.TotalRecurringEarning + employee.TotalVariableEarning;
                employee.TotalTax = profile.TaxAmount;
                employee.TotalEmployeeInsuranceContribution = profile.InsuranceAmount;

                var otherDeductions = employee.Components
                    .Where(c => !c.IsDelete && c.ComponentTypeSnapshot == "Deduction"
                        && c.ComponentCodeSnapshot != InstallmentComponentCode && c.ComponentCodeSnapshot != "LOAN_HOSPITAL_AR")
                    .Sum(c => c.Amount);
                employee.TotalDeduction = employee.TotalTax + employee.TotalEmployeeInsuranceContribution + otherDeductions;
                employee.NetPay = employee.GrossPay - employee.TotalDeduction;
                employee.UpdateDateTime = DateTime.UtcNow;
                employee.UpdateBy = actorUserId;

                if (employee.GrossPay <= 0m)
                {
                    MarkError(employee, "Gaji bruto nol atau belum dikonfigurasi pada profil payroll.", actorUserId);
                    blockers.Add($"{employee.EmployeeNameSnapshot}: gaji bruto nol atau belum dikonfigurasi pada profil payroll.");
                    continue;
                }

                employee.EmployeePayrollStatus = EmployeeCalculated;
                employee.ValidationResultJson = null;
            }
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Potongan cicilan dihitung setelah gaji bruto dan potongan wajib ada. Belum menyentuh Finance.
            var deductions = await _deductionService.CalculateRunAsync(run, actorUserId, cancellationToken);
            blockers.AddRange(deductions.Blockers);

            run.TotalEmployeeCount = employees.Count;
            run.ProcessedEmployeeCount = employees.Count(x => x.EmployeePayrollStatus == EmployeeCalculated);
            run.ErrorEmployeeCount = employees.Count(x => x.EmployeePayrollStatus == EmployeeError) + deductions.Blockers.Count;
            run.TotalBaseSalary = employees.Sum(x => x.BaseSalary);
            run.TotalEarning = employees.Sum(x => x.GrossPay);
            run.TotalGrossPay = employees.Sum(x => x.GrossPay);
            run.TotalTax = employees.Sum(x => x.TotalTax);
            run.TotalEmployeeContribution = employees.Sum(x => x.TotalEmployeeInsuranceContribution);
            run.TotalDeduction = employees.Sum(x => x.TotalDeduction);
            run.TotalNetPay = employees.Sum(x => x.NetPay);
            run.RunStatus = PayrollRunStatuses.Calculated;
            run.CalculatedAt = DateTime.UtcNow;
            run.CalculatedByUserId = actorUserId;
            run.ValidationSummaryJson = JsonSerializer.Serialize(
                new ValidationSummary(blockers.Distinct().ToList(), sync.SkipReasons.Distinct().ToList()));
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

    public async Task<PayrollRunDetailResponse> SubmitAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
    {
        await InTransactionAsync($"HR_PAYROLL_RUN_{id:N}", async () =>
        {
            var run = await LoadRunAsync(id, cancellationToken);
            if (run.RunStatus != PayrollRunStatuses.Calculated)
                throw new PayrollRunValidationException($"Hanya payroll run berstatus Calculated yang dapat diajukan (saat ini {run.RunStatus}).");
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
    public async Task<PayrollRunDetailResponse> ReturnAsync(Guid id, string? reason, Guid actorUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(reason)) throw new PayrollRunValidationException("Alasan pengembalian wajib diisi.");

        await InTransactionAsync($"HR_PAYROLL_RUN_{id:N}", async () =>
        {
            var run = await LoadRunAsync(id, cancellationToken);
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
    public async Task<PayrollRunDetailResponse> ApproveAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
    {
        await InTransactionAsync($"HR_PAYROLL_RUN_{id:N}", async () =>
        {
            var run = await LoadRunAsync(id, cancellationToken);
            if (run.RunStatus == PayrollRunStatuses.Approved) return run.Id;
            if (run.RunStatus != PayrollRunStatuses.Submitted)
                throw new PayrollRunValidationException($"Hanya payroll run berstatus Submitted yang dapat disahkan (saat ini {run.RunStatus}).");
            EnsureReadyToDecide(run);

            var confirmed = await _deductionService.ConfirmRunAsync(run, actorUserId, cancellationToken);

            var now = DateTime.UtcNow;
            var employees = await _dbContext.TrxPayrollRunEmployees
                .Where(x => x.PayrollRunId == run.Id && !x.IsDelete).ToListAsync(cancellationToken);
            foreach (var employee in employees)
            {
                employee.EmployeePayrollStatus = EmployeeFinalized;
                employee.IsFinalized = true;
                employee.FinalizedAt = now;
                employee.FinalizedByUserId = actorUserId;
                employee.IsSnapshotFrozen = true;
                employee.SnapshotFrozenAt = now;
                employee.SnapshotFrozenByUserId = actorUserId;
            }

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
                new { run.Id, confirmed.TotalInstallmentsProcessed, confirmed.TotalDeductedAmount, ActorUserId = actorUserId });
            return run.Id;
        }, cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    /// <summary>Membatalkan run SEBELUM disahkan. Run yang sudah disahkan tidak dibatalkan; koreksinya lewat pembalikan.</summary>
    public async Task<PayrollRunDetailResponse> CancelAsync(Guid id, string? reason, Guid actorUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(reason)) throw new PayrollRunValidationException("Alasan pembatalan wajib diisi.");

        await InTransactionAsync($"HR_PAYROLL_RUN_{id:N}", async () =>
        {
            var run = await LoadRunAsync(id, cancellationToken);
            if (PayrollRunStatuses.IsFinalized(run.RunStatus) || run.IsLocked)
                throw new PayrollRunValidationException("Payroll run yang sudah disahkan tidak dapat dibatalkan; gunakan pembalikan dengan approval.");
            if (run.RunStatus == PayrollRunStatuses.Cancelled) return run.Id;

            run.RunStatus = PayrollRunStatuses.Cancelled;
            run.Notes = string.IsNullOrWhiteSpace(run.Notes) ? $"Dibatalkan: {reason.Trim()}" : $"{run.Notes}\nDibatalkan: {reason.Trim()}";
            run.UpdateDateTime = DateTime.UtcNow;
            run.UpdateBy = actorUserId;
            AddApproval(run, actorUserId, "Cancel", "Cancelled", reason.Trim());

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _loggerService.AuditAsync(LogCategory, "PayrollRun.Cancelled", $"Payroll run {run.RunNumber} dibatalkan: {reason.Trim()}",
                new { run.Id, ActorUserId = actorUserId });
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
            if (run.RunStatus != PayrollRunStatuses.Approved)
                throw new PayrollRunValidationException("Pembalikan hanya untuk payroll run yang sudah disahkan. Run yang belum disahkan cukup dikembalikan atau dihitung ulang.");

            var employee = await _dbContext.TrxPayrollRunEmployees
                .Include(x => x.Components).Include(x => x.VariableInputs)
                .SingleOrDefaultAsync(x => x.Id == runEmployeeId && x.PayrollRunId == runId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pegawai tidak ditemukan pada payroll run ini.");

            var line = PayrollInstallmentDeductionService.ReadLine(employee);
            if (line is null || !line.IsConfirmedToFinance)
                throw new PayrollRunValidationException("Pegawai ini tidak memiliki potongan cicilan yang sudah dikonfirmasi ke Finance pada run ini.");

            var installment = await _dbContext.FinReceivableInstallments.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == line.InstallmentId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Angsuran Finance tidak ditemukan.");

            var pending = await _dbContext.TrxPayrollReversals.AnyAsync(x => !x.IsDelete
                && x.PayrollRunEmployeeId == runEmployeeId && x.ReversalStatus == PayrollReversalStatuses.Requested, cancellationToken);
            if (pending) throw new PayrollRunConflictException("Pembalikan untuk pegawai ini masih menunggu persetujuan.");

            var deducted = installment.PaidAmount;
            if (deducted <= 0m) throw new PayrollRunValidationException("Tidak ada potongan terpotong yang dapat dibalik.");
            var amount = request!.Amount ?? deducted;
            if (amount <= 0m || amount > deducted)
                throw new PayrollRunValidationException($"Nominal pembalikan harus lebih dari nol dan tidak melebihi nominal terpotong saat ini (Rp {deducted:N0}).");

            var reversal = new TrxPayrollReversal
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
            _dbContext.TrxPayrollReversals.Add(reversal);
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
            var reversal = await _dbContext.TrxPayrollReversals
                .SingleOrDefaultAsync(x => x.Id == reversalId && x.PayrollRunId == runId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pembalikan tidak ditemukan pada payroll run ini.");
            if (reversal.ReversalStatus == PayrollReversalStatuses.Approved) return reversal.Id;
            if (reversal.ReversalStatus != PayrollReversalStatuses.Requested)
                throw new PayrollRunValidationException($"Pembalikan berstatus {reversal.ReversalStatus} dan tidak dapat disetujui.");

            var employee = await _dbContext.TrxPayrollRunEmployees
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
            var reversal = await _dbContext.TrxPayrollReversals
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
    // Pembantu
    // ------------------------------------------------------------------------------------

    private async Task<TrxPayrollRun> LoadRunAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.TrxPayrollRuns.Include(x => x.PayrollPeriod)
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
        ?? throw new KeyNotFoundException("Payroll run tidak ditemukan.");

    /// <summary>Run siap diajukan/disahkan hanya bila tidak ada penahan dan ada pegawai yang terhitung.</summary>
    private static void EnsureReadyToDecide(TrxPayrollRun run)
    {
        var blockers = ReadValidation(run.ValidationSummaryJson).Blockers;
        if (blockers.Count > 0)
            throw new PayrollRunValidationException($"Payroll run memiliki {blockers.Count} penahan: {string.Join("; ", blockers.Take(3))}.");
        if (run.ErrorEmployeeCount > 0 || run.ProcessedEmployeeCount <= 0)
            throw new PayrollRunValidationException("Payroll run belum siap: masih ada pegawai bermasalah atau belum ada pegawai yang terhitung.");
    }

    private static void MarkError(TrxPayrollRunEmployee employee, string message, Guid actorUserId)
    {
        employee.EmployeePayrollStatus = EmployeeError;
        employee.ValidationResultJson = message;
        employee.UpdateDateTime = DateTime.UtcNow;
        employee.UpdateBy = actorUserId;
    }

    private void AddApproval(TrxPayrollRun run, Guid actorUserId, string actionType, string status, string? comments) =>
        _dbContext.TrxPayrollApprovals.Add(new TrxPayrollApproval
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

    private static T MapListItem<T>(TrxPayrollRun run, T target) where T : PayrollRunListItemResponse
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
