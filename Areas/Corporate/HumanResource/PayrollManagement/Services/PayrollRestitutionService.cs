using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services;

/// <summary>
/// BE-FIN-111 (Revisi 21, EPIC FIN-04, FIN-DEC-195, FIN-INTEGRATION-1.8 §P.6 butir 3).
/// Layanan restitusi salah potong cicilan piutang RS di HR Payroll:
/// 1. Mengembalikan potongan yang terlanjur diambil dari pegawai yang salah ke slip gaji periode berikutnya (AC 1).
/// 2. Menghasilkan TrxPayrollAdjustment (tipe 'Add', status 'Approved') dan TrxPayrollEmployeeComponent (tipe 'Earning').
/// 3. Menghubungkan pembalikan dengan TrxPayrollReversal menunjuk FinanceReversalId.
/// 4. Finance TIDAK mengeluarkan kas manual untuk restitusi ini (AC 2, FIN-DEC-195).
/// 5. Menjamin sifat idempoten per SourceId/CorrelationId jika event dikirim berulang (AC 3).
/// </summary>
public sealed class PayrollRestitutionService
{
    private const string LogCategory = "Corporate.HumanResource.PayrollManagement.Restitution";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public PayrollRestitutionService(
        ApplicationDbContext dbContext,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    /// <summary>
    /// Memproses penyesuaian restitusi salah potong cicilan ke slip gaji periode berikutnya di HR (AC 1, AC 2, AC 3).
    /// </summary>
    public async Task<PayrollRestitutionResponse> ProcessRestitutionAsync(
        PayrollRestitutionRequest request, CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
        {
            throw new ArgumentException("Nominal restitusi harus lebih besar dari 0.", nameof(request.Amount));
        }

        // AC 3: Idempotency check berbasis (SourceType, SourceId)
        var existingAdjustment = await _dbContext.TrxPayrollAdjustments
            .Include(a => a.PayrollRun)
                .ThenInclude(r => r!.PayrollPeriod)
            .FirstOrDefaultAsync(a => a.SourceType == request.SourceType && a.SourceId == request.SourceId && !a.IsDelete, cancellationToken);

        if (existingAdjustment is not null)
        {
            var existingReversal = await _dbContext.TrxPayrollReversals
                .FirstOrDefaultAsync(r => r.FinanceReversalId == request.SourceId && !r.IsDelete, cancellationToken);

            var existingRunEmployee = await _dbContext.TrxPayrollRunEmployees
                .FirstOrDefaultAsync(e => e.Id == existingAdjustment.PayrollRunEmployeeId && !e.IsDelete, cancellationToken);

            return new PayrollRestitutionResponse
            {
                RestitutionAdjustmentId = existingAdjustment.Id,
                RestitutionReversalId = existingReversal?.Id ?? Guid.Empty,
                PayrollRunId = existingAdjustment.PayrollRunId,
                PayrollRunEmployeeId = existingAdjustment.PayrollRunEmployeeId ?? Guid.Empty,
                EmployeeId = request.EmployeeId,
                EmployeeName = existingRunEmployee?.EmployeeNameSnapshot ?? string.Empty,
                EmployeeNumber = existingRunEmployee?.EmployeeNumberSnapshot ?? string.Empty,
                Amount = existingAdjustment.Amount,
                TargetPeriodCode = existingAdjustment.PayrollRun?.PayrollPeriod?.PayrollPeriodCode ?? string.Empty,
                IsIdempotentReplay = true,
                Status = "IDEMPOTENT",
                Message = "Restitusi untuk koreksi ini sudah pernah diproses sebelumnya (idempoten)."
            };
        }

        // Pemilik slip: WorkforceProfileId adalah identitas utama; EmployeeId lama diterjemahkan. Dokter tetap tanpa
        // MstEmployee dikenali lewat MstDoctor dan TIDAK PERNAH dibuatkan data karyawan palsu.
        var employee = await ResolvePersonAsync(request, cancellationToken)
            ?? throw new KeyNotFoundException("Pegawai atau dokter pemilik potongan tidak ditemukan.");

        // Dokter tanpa profil payroll tidak pernah dipotong gajinya, sehingga tidak ada potongan yang dapat
        // dikembalikan lewat slip. Tidak membuat run, adjustment, atau komponen fiktif; alasan dikembalikan jelas.
        if (employee.EmployeeId is null && !await HasPayrollProfileAsync(employee.WorkforceProfileId, cancellationToken))
        {
            return new PayrollRestitutionResponse
            {
                EmployeeId = Guid.Empty,
                WorkforceProfileId = employee.WorkforceProfileId,
                EmployeeName = employee.Name,
                EmployeeNumber = employee.Number,
                Amount = request.Amount,
                IsIdempotentReplay = false,
                Status = "TIDAK_ADA_PAYROLL",
                Message = "SDM ini belum memiliki profil payroll yang dikonfigurasi di HR, sehingga tidak ada potongan gaji yang dikembalikan lewat slip. Selesaikan pengembalian lewat proses refund yang disetujui."
            };
        }

        // Cari periode berikutnya
        var targetPeriod = await ResolveTargetPeriodAsync(request.TargetPeriodCode, cancellationToken)
            ?? throw new InvalidOperationException("Periode penggajian berikutnya tidak ditemukan untuk alokasi restitusi.");

        // Resolve or create TrxPayrollRun
        var run = await ResolveOrCreateRunAsync(targetPeriod, request.ActorUserId, cancellationToken);

        // Resolve or create TrxPayrollRunEmployee
        var runEmployee = await ResolveOrCreateRunEmployeeAsync(run, employee, request.ActorUserId, cancellationToken);

        // AC 1: Buat TrxPayrollAdjustment bertipe 'Add'
        var adjustment = new TrxPayrollAdjustment
        {
            PayrollRunId = run.Id,
            PayrollRunEmployeeId = runEmployee.Id,
            AdjustmentNumber = $"ADJ-REST-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}",
            AdjustmentType = "Add",
            AdjustmentStatus = "Approved",
            CurrencyCode = "IDR",
            Quantity = 1m,
            Rate = request.Amount,
            Amount = request.Amount,
            Reason = string.IsNullOrWhiteSpace(request.Reason)
                ? "Penyesuaian restitusi salah potong cicilan piutang RS (Koreksi Debitur)"
                : request.Reason,
            SourceType = request.SourceType,
            SourceId = request.SourceId,
            ApprovedAt = DateTime.UtcNow,
            ApprovedByUserId = request.ActorUserId,
            AppliedAt = DateTime.UtcNow,
            AppliedByUserId = request.ActorUserId,
            ApprovalNotes = "Disetujui otomatis oleh sistem restitusi piutang RS (FIN-DEC-195)",
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = request.ActorUserId
        };
        _dbContext.TrxPayrollAdjustments.Add(adjustment);

        // AC 1 & AC 2: Buat TrxPayrollReversal menunjuk FinanceReversalId (tanpa kas keluar manual Finance)
        var reversal = new TrxPayrollReversal
        {
            PayrollRunId = run.Id,
            PayrollRunEmployeeId = runEmployee.Id,
            FinanceReversalId = request.SourceId,
            ReversalNumber = $"REV-REST-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}",
            ReversalType = "Component",
            ReversalStatus = "Approved",
            CurrencyCode = "IDR",
            ReversalAmount = request.Amount,
            ReversalReason = adjustment.Reason,
            ReversedAt = DateTime.UtcNow,
            ReversedByUserId = request.ActorUserId,
            Notes = $"Restitusi potongan cicilan piutang RS dikembalikan via slip gaji HR tanpa kas keluar Finance (FIN-DEC-195). CorrelationId: {request.CorrelationId}",
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = request.ActorUserId
        };
        _dbContext.TrxPayrollReversals.Add(reversal);

        // AC 1: Tambahkan TrxPayrollEmployeeComponent penambah gaji pada slip pegawai bulan depan
        var component = new TrxPayrollEmployeeComponent
        {
            PayrollRunEmployeeId = runEmployee.Id,
            SourceType = "Adjustment",
            SourceId = adjustment.Id,
            ComponentCodeSnapshot = "RESTITUSI_POTONGAN_PIUTANG",
            ComponentNameSnapshot = "Penyesuaian Restitusi Potongan Piutang RS",
            ComponentTypeSnapshot = "Earning",
            CalculationMethodSnapshot = "Fixed",
            CurrencyCode = "IDR",
            Quantity = 1m,
            Rate = request.Amount,
            BaseAmount = request.Amount,
            Amount = request.Amount,
            IsTaxable = false,
            IsProrated = false,
            IsManual = false,
            IsDisplayedOnPayslip = true,
            SortOrder = 90,
            Notes = "Pengembalian potongan cicilan salah potong periode sebelumnya (FIN-DEC-195)",
            CreateDateTime = DateTime.UtcNow,
            CreateBy = request.ActorUserId
        };
        _dbContext.TrxPayrollEmployeeComponents.Add(component);

        // Naikkan pendapatan slip pegawai (slip bulan depan A bertambah Rp request.Amount)
        runEmployee.TotalVariableEarning += request.Amount;
        runEmployee.GrossPay += request.Amount;
        runEmployee.NetPay += request.Amount;
        runEmployee.PaymentAmount += request.Amount;
        runEmployee.UpdateDateTime = DateTime.UtcNow;
        runEmployee.UpdateBy = request.ActorUserId;

        // Naikkan akumulasi run
        run.TotalEarning += request.Amount;
        run.TotalGrossPay += request.Amount;
        run.TotalNetPay += request.Amount;
        run.UpdateDateTime = DateTime.UtcNow;
        run.UpdateBy = request.ActorUserId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "Restitution.Completed",
            $"Restitusi salah potong cicilan Rp {request.Amount:N2} berhasil disematkan pada slip periode {targetPeriod.PayrollPeriodCode} pegawai {employee.Name} (NIP: {employee.Number}). Tanpa kas keluar Finance.",
            new { PayrollRunId = run.Id, PayrollRunEmployeeId = runEmployee.Id, EmployeeId = employee.EmployeeId, WorkforceProfileId = employee.WorkforceProfileId, Amount = request.Amount, SourceId = request.SourceId, ActorUserId = request.ActorUserId });

        return new PayrollRestitutionResponse
        {
            RestitutionAdjustmentId = adjustment.Id,
            RestitutionReversalId = reversal.Id,
            PayrollRunId = run.Id,
            PayrollRunEmployeeId = runEmployee.Id,
            EmployeeId = employee.EmployeeId ?? Guid.Empty,
            WorkforceProfileId = employee.WorkforceProfileId,
            EmployeeName = employee.Name,
            EmployeeNumber = employee.Number,
            Amount = request.Amount,
            TargetPeriodCode = targetPeriod.PayrollPeriodCode,
            IsIdempotentReplay = false,
            Status = "BERHASIL",
            Message = $"Restitusi Rp {request.Amount:N2} berhasil dijadwalkan pada slip gaji periode {targetPeriod.PayrollPeriodCode}."
        };
    }

    private sealed record PayrollPerson(Guid WorkforceProfileId, Guid? EmployeeId, string Number, string Name);

    private async Task<PayrollPerson?> ResolvePersonAsync(PayrollRestitutionRequest request, CancellationToken cancellationToken)
    {
        MstEmployee? employee = null;
        if (request.EmployeeId != Guid.Empty)
        {
            employee = await _dbContext.MstEmployees
                .FirstOrDefaultAsync(e => e.Id == request.EmployeeId && !e.IsDelete, cancellationToken);
        }
        if (employee is null && request.WorkforceProfileId.HasValue && request.WorkforceProfileId.Value != Guid.Empty)
        {
            var profileId = request.WorkforceProfileId.Value;
            employee = await _dbContext.MstEmployees
                .FirstOrDefaultAsync(e => e.WorkforceProfileId == profileId && !e.IsDelete, cancellationToken);
        }
        if (employee is not null)
        {
            // Bila keduanya dikirim, harus orang yang sama.
            if (request.WorkforceProfileId.HasValue && request.WorkforceProfileId.Value != Guid.Empty
                && employee.WorkforceProfileId != request.WorkforceProfileId.Value)
                throw new ArgumentException("EmployeeId dan WorkforceProfileId tidak menunjuk orang yang sama.");
            return new PayrollPerson(employee.WorkforceProfileId, employee.Id, employee.EmployeeNumber, employee.FullName);
        }

        if (request.WorkforceProfileId.HasValue && request.WorkforceProfileId.Value != Guid.Empty)
        {
            var profileId = request.WorkforceProfileId.Value;
            var doctor = await _dbContext.MstDoctors.AsNoTracking()
                .Where(d => d.WorkforceProfileId == profileId && !d.IsDelete)
                .Select(d => new { d.DoctorNumber, d.FullName })
                .FirstOrDefaultAsync(cancellationToken);
            if (doctor is not null) return new PayrollPerson(profileId, null, doctor.DoctorNumber, doctor.FullName);
        }

        return null;
    }

    private Task<bool> HasPayrollProfileAsync(Guid workforceProfileId, CancellationToken cancellationToken) =>
        _dbContext.WfpPayrolls.AsNoTracking()
            .AnyAsync(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete && x.IsPayrollEligible, cancellationToken);

    /// <summary>
    /// Mencari status restitusi berdasarkan SourceId (misal ReceivableId kartu piutang lama).
    /// </summary>
    public async Task<PayrollRestitutionResponse?> GetRestitutionBySourceIdAsync(
        Guid sourceId, CancellationToken cancellationToken)
    {
        var adjustment = await _dbContext.TrxPayrollAdjustments
            .Include(a => a.PayrollRun)
                .ThenInclude(r => r!.PayrollPeriod)
            .FirstOrDefaultAsync(a => a.SourceId == sourceId && !a.IsDelete, cancellationToken);

        if (adjustment is null) return null;

        var reversal = await _dbContext.TrxPayrollReversals
            .FirstOrDefaultAsync(r => r.FinanceReversalId == sourceId && !r.IsDelete, cancellationToken);

        var runEmployee = await _dbContext.TrxPayrollRunEmployees
            .FirstOrDefaultAsync(e => e.Id == adjustment.PayrollRunEmployeeId && !e.IsDelete, cancellationToken);

        return new PayrollRestitutionResponse
        {
            RestitutionAdjustmentId = adjustment.Id,
            RestitutionReversalId = reversal?.Id ?? Guid.Empty,
            PayrollRunId = adjustment.PayrollRunId,
            PayrollRunEmployeeId = adjustment.PayrollRunEmployeeId ?? Guid.Empty,
            EmployeeId = runEmployee?.EmployeeId ?? Guid.Empty,
            EmployeeName = runEmployee?.EmployeeNameSnapshot ?? string.Empty,
            EmployeeNumber = runEmployee?.EmployeeNumberSnapshot ?? string.Empty,
            Amount = adjustment.Amount,
            TargetPeriodCode = adjustment.PayrollRun?.PayrollPeriod?.PayrollPeriodCode ?? string.Empty,
            IsIdempotentReplay = false,
            Status = "TERPROSES",
            Message = "Restitusi ditemukan.",
            ProcessedAt = adjustment.CreateDateTime
        };
    }

    private async Task<MstPayrollPeriod?> ResolveTargetPeriodAsync(string? targetPeriodCode, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(targetPeriodCode))
        {
            var specified = await _dbContext.MstPayrollPeriods.AsNoTracking()
                .FirstOrDefaultAsync(x => !x.IsDelete && x.PayrollPeriodCode == targetPeriodCode, cancellationToken);
            if (specified is not null) return specified;
        }

        // Cari periode bulan berikutnya
        var nextMonth = DateTime.UtcNow.AddMonths(1);
        var nextPeriod = await _dbContext.MstPayrollPeriods.AsNoTracking()
            .Where(x => !x.IsDelete && x.PeriodType == "Monthly" && x.FiscalYear == nextMonth.Year && x.PeriodNumber == nextMonth.Month)
            .OrderByDescending(x => x.CreateDateTime)
            .FirstOrDefaultAsync(cancellationToken);

        if (nextPeriod is not null) return nextPeriod;

        // Fallback ke periode mendatang yang berstatus Draft / Open
        var futurePeriod = await _dbContext.MstPayrollPeriods.AsNoTracking()
            .Where(x => !x.IsDelete && x.PeriodType == "Monthly" && x.StartDate >= DateTime.UtcNow)
            .OrderBy(x => x.StartDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (futurePeriod is not null) return futurePeriod;

        // Fallback terakhir ke periode bulanan paling baru
        return await _dbContext.MstPayrollPeriods.AsNoTracking()
            .Where(x => !x.IsDelete && x.PeriodType == "Monthly")
            .OrderByDescending(x => x.StartDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<TrxPayrollRun> ResolveOrCreateRunAsync(MstPayrollPeriod period, Guid actorUserId, CancellationToken cancellationToken)
    {
        var tracked = _dbContext.ChangeTracker.Entries<TrxPayrollRun>()
            .Select(x => x.Entity)
            .FirstOrDefault(x => x.PayrollPeriodId == period.Id && x.RunType == "Regular"
                && (x.RunStatus == "Draft" || x.RunStatus == "CollectingInput") && !x.IsDelete);
        if (tracked is not null) return tracked;

        var existing = await _dbContext.TrxPayrollRuns
            .FirstOrDefaultAsync(x => !x.IsDelete && x.PayrollPeriodId == period.Id && x.RunType == "Regular"
                && (x.RunStatus == "Draft" || x.RunStatus == "CollectingInput"), cancellationToken);
        if (existing is not null) return existing;

        var run = new TrxPayrollRun
        {
            PayrollPeriodId = period.Id,
            LegalEntityId = period.LegalEntityId,
            HospitalSiteId = period.HospitalSiteId,
            RunNumber = $"PR-{period.FiscalYear}{period.PeriodNumber:D2}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}",
            RunType = "Regular",
            RunStatus = "Draft",
            CurrencyCode = "IDR",
            PeriodStartDateSnapshot = period.StartDate,
            PeriodEndDateSnapshot = period.EndDate,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };
        _dbContext.TrxPayrollRuns.Add(run);
        return run;
    }

    private async Task<TrxPayrollRunEmployee> ResolveOrCreateRunEmployeeAsync(
        TrxPayrollRun run, PayrollPerson employee, Guid actorUserId, CancellationToken cancellationToken)
    {
        var tracked = _dbContext.ChangeTracker.Entries<TrxPayrollRunEmployee>()
            .Select(x => x.Entity)
            .FirstOrDefault(x => x.PayrollRunId == run.Id && x.WorkforceProfileId == employee.WorkforceProfileId && !x.IsDelete);
        if (tracked is not null) return tracked;

        var existing = await _dbContext.TrxPayrollRunEmployees
            .Include(x => x.Components)
            .Include(x => x.Adjustments)
            .FirstOrDefaultAsync(x => !x.IsDelete && x.PayrollRunId == run.Id && x.WorkforceProfileId == employee.WorkforceProfileId, cancellationToken);
        if (existing is not null) return existing;

        var runEmployee = new TrxPayrollRunEmployee
        {
            PayrollRunId = run.Id,
            WorkforceProfileId = employee.WorkforceProfileId,
            EmployeeId = employee.EmployeeId,
            EmployeePayrollStatus = "Pending",
            CurrencyCode = "IDR",
            EmployeeNumberSnapshot = employee.Number,
            EmployeeNameSnapshot = employee.Name,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };
        _dbContext.TrxPayrollRunEmployees.Add(runEmployee);
        return runEmployee;
    }
}
