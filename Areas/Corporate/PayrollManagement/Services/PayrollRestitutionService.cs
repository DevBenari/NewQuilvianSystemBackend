using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// BE-FIN-111 (Revisi 21, EPIC FIN-04, FIN-DEC-195, FIN-INTEGRATION-1.8 §P.6 butir 3).
/// Layanan restitusi salah potong cicilan piutang RS di HR Payroll:
/// 1. Mengembalikan potongan yang terlanjur diambil dari pegawai yang salah ke slip gaji periode berikutnya (AC 1).
/// 2. Menghasilkan PrlPayrollAdjustment (tipe 'Add', status 'Approved') dan PrlPayrollEmployeeComponent (tipe 'Earning').
/// 3. Menghubungkan pembalikan dengan PrlPayrollReversal menunjuk FinanceReversalId.
/// 4. Finance TIDAK mengeluarkan kas manual untuk restitusi ini (AC 2, FIN-DEC-195).
/// 5. Menjamin sifat idempoten per SourceId/CorrelationId jika event dikirim berulang (AC 3).
///
/// Aturan run (agar restitusi tidak mengubah gaji lewat dua jalur dan tidak memecah kepemilikan run):
/// - Restitusi hanya masuk ke run Regular yang MASIH menerima input (Draft, CollectingInput, Calculated). Tidak pernah membuat run
///   kedua untuk periode yang sudah punya run Regular aktif; bila run periode tujuan sudah diajukan/disahkan, restitusi pindah ke
///   periode berikutnya.
/// - Total gaji TIDAK dimutasi langsung. Komponen restitusi dihitung SEKALI oleh mesin kalkulasi (baris non-mesin); run yang sudah
///   Calculated dimundurkan ke CollectingInput supaya wajib dihitung ulang sebelum diajukan.
/// - Run penampung (dibuat bila periode belum punya run) dilengkapi populasinya saat run dihitung.
/// </summary>
public sealed class PayrollRestitutionService
{
    private const string LogCategory = "Corporate.HumanResource.PayrollManagement.Restitution";
    private const int MaxPeriodHops = 3;
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
        var existingAdjustment = await _dbContext.PrlPayrollAdjustments
            .Include(a => a.PayrollRun)
                .ThenInclude(r => r!.PayrollPeriod)
            .FirstOrDefaultAsync(a => a.SourceType == request.SourceType && a.SourceId == request.SourceId && !a.IsDelete, cancellationToken);

        if (existingAdjustment is not null)
        {
            var existingReversal = await _dbContext.PrlPayrollReversals
                .FirstOrDefaultAsync(r => r.FinanceReversalId == request.SourceId && !r.IsDelete, cancellationToken);

            var existingRunEmployee = await _dbContext.PrlPayrollRunEmployees
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

        // Penerima tanpa profil payroll eligible (termasuk dokter tetap tanpa MstEmployee) tidak pernah dipotong gajinya lewat run,
        // sehingga tidak ada potongan yang dapat dikembalikan lewat slip. Tidak membuat run, adjustment, baris pegawai, atau
        // komponen fiktif (juga agar baris tanpa profil tidak menahan seluruh run); alasan dikembalikan jelas.
        if (!await HasPayrollProfileAsync(employee.WorkforceProfileId, cancellationToken))
        {
            return new PayrollRestitutionResponse
            {
                EmployeeId = employee.EmployeeId ?? Guid.Empty,
                WorkforceProfileId = employee.WorkforceProfileId,
                EmployeeName = employee.Name,
                EmployeeNumber = employee.Number,
                Amount = request.Amount,
                IsIdempotentReplay = false,
                Status = "TIDAK_ADA_PAYROLL",
                Message = "SDM ini belum memiliki profil payroll eligible, sehingga restitusi tidak dapat disematkan pada slip. Selesaikan pengembalian lewat proses refund yang disetujui."
            };
        }

        // Cari periode tujuan; bila run Regular periode itu sudah tidak menerima input, pindah ke periode berikutnya (maksimal 3 kali).
        var targetPeriod = await ResolveTargetPeriodAsync(request.TargetPeriodCode, cancellationToken)
            ?? throw new PayrollRunConflictException("Periode penggajian berikutnya tidak ditemukan untuk alokasi restitusi.");

        PrlPayrollRun? run = null;
        for (var hop = 0; hop < MaxPeriodHops && run is null; hop++)
        {
            run = await ResolveOrCreateRunAsync(targetPeriod, request.ActorUserId, cancellationToken);
            if (run is not null) break;
            var following = await ResolveFollowingPeriodAsync(targetPeriod, cancellationToken);
            if (following is null) break;
            targetPeriod = following;
        }
        if (run is null)
            throw new PayrollRunConflictException(
                "Payroll run periode tujuan sudah tidak menerima penyesuaian dan tidak ada periode berikutnya. Buka periode berikutnya lalu ulangi restitusi.");

        // Resolve or create PrlPayrollRunEmployee
        var runEmployee = await ResolveOrCreateRunEmployeeAsync(run, employee, request.ActorUserId, cancellationToken);

        // AC 1: Buat PrlPayrollAdjustment bertipe 'Add'
        var adjustment = new PrlPayrollAdjustment
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
        _dbContext.PrlPayrollAdjustments.Add(adjustment);

        // AC 1 & AC 2: Buat PrlPayrollReversal menunjuk FinanceReversalId (tanpa kas keluar manual Finance)
        var reversal = new PrlPayrollReversal
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
        _dbContext.PrlPayrollReversals.Add(reversal);

        // AC 1: Tambahkan PrlPayrollEmployeeComponent penambah gaji pada slip pegawai bulan depan
        var component = new PrlPayrollEmployeeComponent
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
        _dbContext.PrlPayrollEmployeeComponents.Add(component);

        // Penjaga hitung ulang: nominal restitusi sudah menjadi komponen non-mesin yang dihitung SEKALI oleh mesin kalkulasi saat
        // run dihitung. Total TIDAK dimutasi di sini (itu akan menggandakan nominal pada hitung ulang). Run yang sudah dihitung
        // dimundurkan ke CollectingInput agar wajib dihitung ulang sebelum dapat diajukan.
        if (string.Equals(run.RunStatus, PayrollRunStatuses.Calculated, StringComparison.OrdinalIgnoreCase))
            run.RunStatus = PayrollRunStatuses.CollectingInput;
        runEmployee.UpdateDateTime = DateTime.UtcNow;
        runEmployee.UpdateBy = request.ActorUserId;
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
        _dbContext.PrlPayrolls.AsNoTracking()
            .AnyAsync(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete && x.IsPayrollEligible, cancellationToken);

    /// <summary>
    /// Mencari status restitusi berdasarkan SourceId (misal ReceivableId kartu piutang lama).
    /// </summary>
    public async Task<PayrollRestitutionResponse?> GetRestitutionBySourceIdAsync(
        Guid sourceId, CancellationToken cancellationToken)
    {
        var adjustment = await _dbContext.PrlPayrollAdjustments
            .Include(a => a.PayrollRun)
                .ThenInclude(r => r!.PayrollPeriod)
            .FirstOrDefaultAsync(a => a.SourceId == sourceId && !a.IsDelete, cancellationToken);

        if (adjustment is null) return null;

        var reversal = await _dbContext.PrlPayrollReversals
            .FirstOrDefaultAsync(r => r.FinanceReversalId == sourceId && !r.IsDelete, cancellationToken);

        var runEmployee = await _dbContext.PrlPayrollRunEmployees
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

    /// <summary>
    /// Run Regular aktif yang masih menerima input untuk periode itu; bila periode belum punya run Regular aktif sama sekali, dibuat run
    /// penampung (Draft) agar kewajiban restitusi tetap tercatat. Mengembalikan null bila run Regular aktifnya sudah tidak menerima input
    /// (diajukan/disahkan/terkunci) sehingga pemanggil pindah ke periode berikutnya. Tidak pernah membuat run Regular kedua.
    /// </summary>
    private async Task<PrlPayrollRun?> ResolveOrCreateRunAsync(MstPayrollPeriod period, Guid actorUserId, CancellationToken cancellationToken)
    {
        var tracked = _dbContext.ChangeTracker.Entries<PrlPayrollRun>()
            .Select(x => x.Entity)
            .FirstOrDefault(x => x.PayrollPeriodId == period.Id && x.RunType == "Regular" && !x.IsDelete
                && !x.IsLocked && PayrollRunStatuses.AcceptsInputs(x.RunStatus));
        if (tracked is not null) return tracked;

        var activeRegular = await _dbContext.PrlPayrollRuns
            .Where(x => !x.IsDelete && x.PayrollPeriodId == period.Id && x.RunType == "Regular"
                && x.RunStatus != PayrollRunStatuses.Cancelled && x.RunStatus != PayrollRunStatuses.Reversed)
            .OrderByDescending(x => x.CreateDateTime)
            .FirstOrDefaultAsync(cancellationToken);
        if (activeRegular is not null)
            return !activeRegular.IsLocked && PayrollRunStatuses.AcceptsInputs(activeRegular.RunStatus) ? activeRegular : null;

        var run = new PrlPayrollRun
        {
            PayrollPeriodId = period.Id,
            LegalEntityId = period.LegalEntityId,
            HospitalSiteId = period.HospitalSiteId,
            RunNumber = $"PR-{period.FiscalYear}{period.PeriodNumber:D2}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}",
            RunType = "Regular",
            RunStatus = PayrollRunStatuses.Draft,
            CurrencyCode = "IDR",
            PeriodStartDateSnapshot = period.StartDate,
            PeriodEndDateSnapshot = period.EndDate,
            AttendanceCutoffDateSnapshot = period.AttendanceCutoffDate,
            VariableInputCutoffDateSnapshot = period.VariableInputCutoffDate,
            PaymentDateSnapshot = period.PaymentDate,
            Notes = "Run penampung restitusi: populasi dilengkapi otomatis saat run dihitung.",
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };
        _dbContext.PrlPayrollRuns.Add(run);
        return run;
    }

    private Task<MstPayrollPeriod?> ResolveFollowingPeriodAsync(MstPayrollPeriod period, CancellationToken cancellationToken) =>
        _dbContext.MstPayrollPeriods.AsNoTracking()
            .Where(x => !x.IsDelete && x.PeriodType == "Monthly" && x.StartDate > period.StartDate)
            .OrderBy(x => x.StartDate)
            .FirstOrDefaultAsync(cancellationToken);

    private async Task<PrlPayrollRunEmployee> ResolveOrCreateRunEmployeeAsync(
        PrlPayrollRun run, PayrollPerson employee, Guid actorUserId, CancellationToken cancellationToken)
    {
        var tracked = _dbContext.ChangeTracker.Entries<PrlPayrollRunEmployee>()
            .Select(x => x.Entity)
            .FirstOrDefault(x => x.PayrollRunId == run.Id && x.WorkforceProfileId == employee.WorkforceProfileId && !x.IsDelete);
        if (tracked is not null) return tracked;

        var existing = await _dbContext.PrlPayrollRunEmployees
            .Include(x => x.Components)
            .Include(x => x.Adjustments)
            .FirstOrDefaultAsync(x => !x.IsDelete && x.PayrollRunId == run.Id && x.WorkforceProfileId == employee.WorkforceProfileId, cancellationToken);
        if (existing is not null) return existing;

        var runEmployee = new PrlPayrollRunEmployee
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
        _dbContext.PrlPayrollRunEmployees.Add(runEmployee);
        return runEmployee;
    }
}
