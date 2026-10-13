using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Data;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;

/// <summary>
/// BE-FIN-094, FIN-DEC-190, INT-FIN-HR-001 (02-backend-architecture.md §P.2). Menerbitkan jadwal
/// angsuran yang disetujui sebagai <c>TrxPayrollVariableInput</c> milik HR Payroll, dipanggil DI
/// DALAM transaksi persetujuan perjanjian (<c>FinanceReceivableInstallmentPlanService.ApproveAsync</c>),
/// sesuai kontrak ("Waktu Tulis: Ditulis otomatis di dalam transaksi persetujuan perjanjian").
///
/// ADAPTER SEMPIT (BE-FIN-094, wewenang cross-module eksplisit pengguna) — ditemukan saat task ini
/// bahwa HR SAMA SEKALI belum punya service/controller yang membuka payroll run: TrxPayrollRun dan
/// TrxPayrollRunEmployee hanya pernah DIBACA (LeavePayrollIntegrationService selalu mengasumsikan
/// keduanya sudah ada, tidak pernah membuatnya). Baris jadwal instalmen bisa menunjuk periode
/// berbulan-bulan ke depan, yang run-nya belum tentu dibuka HR saat perjanjian disetujui — karena
/// itu service ini resolve-or-create TrxPayrollRun dan TrxPayrollRunEmployee dengan default
/// konservatif (status Draft), BUKAN membangun mesin payroll run HR yang sesungguhnya (buka run,
/// kumpulkan seluruh pegawai aktif, dst — itu epic tersendiri, di luar cakupan task ini).
///
/// MstPayrollPeriod dan MstPayrollComponent SENGAJA find-only, TIDAK PERNAH dibuat di sini —
/// keduanya master data/konfigurasi bisnis HR (kalender periode, definisi komponen payroll
/// termasuk perlakuan pajaknya) yang Finance tidak berwenang mengarang. Baris yang periodenya atau
/// komponennya belum dikonfigurasi HR dilewati dan dicatat sebagai GAP, bukan memblokir persetujuan
/// perjanjian Finance — gagalnya sinkron payroll BUKAN alasan menahan hak Finance menyetujui
/// perjanjian cicilannya sendiri (keputusan desain, dicatat laporan task, bukan di kontrak).
/// </summary>
public sealed class FinanceReceivablePayrollSyncService
{
    private const string LogCategory = "Corporate.FinanceManagement.Receivable.PayrollSync";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;
    private readonly FinanceSubledgerMovementService _subledgerMovementService;
    private readonly FinanceReceivablePayrollSyncOptions _options;

    public FinanceReceivablePayrollSyncService(
        ApplicationDbContext dbContext, LoggerService loggerService, FinanceSubledgerMovementService subledgerMovementService,
        IOptions<FinanceReceivablePayrollSyncOptions> options)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
        _subledgerMovementService = subledgerMovementService;
        _options = options.Value;
    }

    /// <summary>
    /// Mengisi FinReceivableInstallment.TargetPayrollRunEmployeeId belum ada pada skema BE-FIN-092
    /// (temuan drift kontrak, lihat laporan BE-FIN-094 §0) — hasil sinkron karena itu dilaporkan
    /// lewat nilai balik ini, BUKAN disimpan ke kolom Finance mana pun. Dipanggil DI DALAM transaksi
    /// yang sama dengan persetujuan; tidak membuka transaksi sendiri, tidak memanggil SaveChangesAsync
    /// sendiri — caller (ApproveAsync) yang melakukan commit tunggal untuk plan + installment +
    /// seluruh baris HR sekaligus.
    /// </summary>
    public async Task<FinanceReceivablePayrollSyncResult> SyncAsync(
        Guid benefitOwnerId,
        IReadOnlyList<FinReceivableInstallment> installments,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var result = new FinanceReceivablePayrollSyncResult();
        if (!_options.Enabled)
        {
            result.SkipReasons.Add("Sinkron payroll dimatikan lewat konfigurasi (FinanceReceivablePayrollSync:Enabled = false).");
            result.SkippedCount = installments.Count;
            return result;
        }

        // benefitOwnerId = FinReceivable.BenefitOwnerId, secara logis menunjuk MstEmployee.Id
        // (data-dictionary.md §7: "Memetakan NIP ke BenefitOwnerId", FIN-DEC-196/199) — BUKAN
        // WorkforceProfileId langsung, berbeda dari field yang disebut kontrak INT-FIN-HR-001
        // P.2 (temuan drift lain, dicatat laporan task).
        var employee = await _dbContext.MstEmployees.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == benefitOwnerId && !x.IsDelete, cancellationToken);
        if (employee is null)
        {
            result.SkipReasons.Add($"Profil pegawai (MstEmployee) untuk BenefitOwnerId {benefitOwnerId} tidak ditemukan — nol baris disinkron.");
            result.SkippedCount = installments.Count;
            return result;
        }

        var component = await _dbContext.MstPayrollComponents.AsNoTracking()
            .SingleOrDefaultAsync(x => x.PayrollComponentCode == _options.PayrollComponentCode && !x.IsDelete, cancellationToken);
        if (component is null)
        {
            result.SkipReasons.Add(
                $"Komponen payroll '{_options.PayrollComponentCode}' belum dikonfigurasi di MstPayrollComponent — " +
                "HR MUST membuatnya dulu (jenis Deduction) sebelum sinkron dapat berjalan. Nol baris disinkron.");
            result.SkippedCount = installments.Count;
            return result;
        }

        foreach (var installment in installments)
        {
            var period = await ResolvePeriodAsync(installment.DeductionPeriod, cancellationToken);
            if (period is null)
            {
                result.SkipReasons.Add(
                    $"Periode payroll '{installment.DeductionPeriod}' belum dikonfigurasi HR (MstPayrollPeriod) — angsuran ke-{installment.InstallmentNumber} dilewati.");
                result.SkippedCount++;
                continue;
            }

            var run = await ResolveOrCreateRunAsync(period, actorUserId, cancellationToken);
            var runEmployee = await ResolveOrCreateRunEmployeeAsync(run, employee, actorUserId, cancellationToken);

            await UpsertVariableInputAsync(runEmployee, component, installment, actorUserId, cancellationToken);
            result.SyncedCount++;
        }

        if (result.SkipReasons.Count > 0)
        {
            await _loggerService.AuditAsync(LogCategory, "Sync.PartialGap",
                $"Sinkron payroll sebagian terlewat. BenefitOwnerId={benefitOwnerId} Disinkron={result.SyncedCount} Dilewati={result.SkippedCount}",
                new { BenefitOwnerId = benefitOwnerId, result.SyncedCount, result.SkippedCount, result.SkipReasons });
        }

        return result;
    }

    /// <summary>Find-only — MstPayrollPeriod adalah kalender HR, Finance tidak membuatnya.</summary>
    private async Task<MstPayrollPeriod?> ResolvePeriodAsync(string period, CancellationToken cancellationToken)
    {
        var year = int.Parse(period[..4]);
        var month = int.Parse(period[5..7]);
        return await _dbContext.MstPayrollPeriods.AsNoTracking()
            .Where(x => !x.IsDelete && x.FiscalYear == year && x.PeriodNumber == month && x.PeriodType == "Monthly")
            .OrderByDescending(x => x.CreateDateTime)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>Resolve-or-create — Draf minimal per periode, bukan proses buka-run HR sesungguhnya.</summary>
    private async Task<TrxPayrollRun> ResolveOrCreateRunAsync(MstPayrollPeriod period, Guid actorUserId, CancellationToken cancellationToken)
    {
        var existing = await _dbContext.TrxPayrollRuns
            .Where(x => !x.IsDelete && x.PayrollPeriodId == period.Id && x.RunType == "Regular")
            .OrderByDescending(x => x.CreateDateTime)
            .FirstOrDefaultAsync(cancellationToken);
        if (existing is not null) return existing;

        var run = new TrxPayrollRun
        {
            PayrollPeriodId = period.Id,
            LegalEntityId = period.LegalEntityId,
            HospitalSiteId = period.HospitalSiteId,
            RunNumber = GenerateRunNumber(),
            RunType = "Regular",
            RunStatus = "Draft",
            CurrencyCode = _options.CurrencyCode,
            PeriodStartDateSnapshot = period.StartDate,
            PeriodEndDateSnapshot = period.EndDate,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };
        _dbContext.TrxPayrollRuns.Add(run);
        return run;
    }

    /// <summary>Resolve-or-create — snapshot nama/NIP diisi dari MstEmployee saat baris dibuat.</summary>
    private async Task<TrxPayrollRunEmployee> ResolveOrCreateRunEmployeeAsync(
        TrxPayrollRun run, MstEmployee employee, Guid actorUserId, CancellationToken cancellationToken)
    {
        // run.Id bisa belum tersimpan (baris baru, belum SaveChangesAsync) — cari di change
        // tracker dulu sebelum menduplikasi pencarian ke database untuk run yang baru dibuat.
        var tracked = _dbContext.ChangeTracker.Entries<TrxPayrollRunEmployee>()
            .Select(x => x.Entity)
            .FirstOrDefault(x => x.PayrollRunId == run.Id && x.WorkforceProfileId == employee.WorkforceProfileId && !x.IsDelete);
        if (tracked is not null) return tracked;

        var existing = await _dbContext.TrxPayrollRunEmployees
            .Where(x => !x.IsDelete && x.PayrollRunId == run.Id && x.WorkforceProfileId == employee.WorkforceProfileId)
            .FirstOrDefaultAsync(cancellationToken);
        if (existing is not null) return existing;

        var runEmployee = new TrxPayrollRunEmployee
        {
            PayrollRunId = run.Id,
            WorkforceProfileId = employee.WorkforceProfileId,
            EmployeeId = employee.Id,
            EmployeePayrollStatus = "Pending",
            CurrencyCode = _options.CurrencyCode,
            EmployeeNumberSnapshot = employee.EmployeeNumber,
            EmployeeNameSnapshot = employee.FullName,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };
        _dbContext.TrxPayrollRunEmployees.Add(runEmployee);
        return runEmployee;
    }

    /// <summary>Upsert idempoten berbasis (PayrollRunEmployeeId, PayrollComponentId, SourceType, SourceId)
    /// — pola sama dengan UpsertVariableInputAsync pada LeavePayrollIntegrationService.</summary>
    private async Task UpsertVariableInputAsync(
        TrxPayrollRunEmployee runEmployee, MstPayrollComponent component, FinReceivableInstallment installment,
        Guid actorUserId, CancellationToken cancellationToken)
    {
        const string sourceType = "FinanceReceivableInstallment";

        var tracked = _dbContext.ChangeTracker.Entries<TrxPayrollVariableInput>()
            .Select(x => x.Entity)
            .FirstOrDefault(x => x.PayrollRunEmployeeId == runEmployee.Id && x.PayrollComponentId == component.Id
                && x.SourceType == sourceType && x.SourceId == installment.Id && !x.IsDelete);

        var existing = tracked ?? await _dbContext.TrxPayrollVariableInputs
            .FirstOrDefaultAsync(x => x.PayrollRunEmployeeId == runEmployee.Id && x.PayrollComponentId == component.Id
                && x.SourceType == sourceType && x.SourceId == installment.Id && !x.IsDelete, cancellationToken);

        // FIN-DEC-190: Amount = nominal yang wajib dipotong (ScheduledAmount + CarriedOverAmount).
        var amount = installment.ScheduledAmount + installment.CarriedOverAmount;

        if (existing is not null)
        {
            existing.Amount = amount;
            existing.Quantity = 1m;
            existing.Rate = amount;
            existing.UpdateDateTime = DateTime.UtcNow;
            existing.UpdateBy = actorUserId;
            return;
        }

        _dbContext.TrxPayrollVariableInputs.Add(new TrxPayrollVariableInput
        {
            PayrollRunEmployeeId = runEmployee.Id,
            PayrollComponentId = component.Id,
            InputNumber = GenerateVariableInputNumber(),
            InputDate = DateOnly.FromDateTime(DateTime.UtcNow),
            InputType = "Manual",
            InputStatus = "Draft",
            CurrencyCode = _options.CurrencyCode,
            Quantity = 1m,
            Rate = amount,
            Amount = amount,
            SourceType = sourceType,
            SourceId = installment.Id,
            Notes = $"Cicilan Piutang Layanan RS (Angsuran ke-{installment.InstallmentNumber})",
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        });
    }

    private static string GenerateRunNumber()
    {
        var candidate = $"RUN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
        return candidate.Length <= 50 ? candidate : candidate[..50];
    }

    private static string GenerateVariableInputNumber() =>
        $"FRI-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..31].ToUpperInvariant();

    // ------------------------------------------------------------------------------------
    // BE-FIN-095, INT-HR-FIN-001 §P.3 — HR mengirim hasil potongan (arah masuk, kebalikan dari
    // SyncAsync di atas). Idempoten berbasis (InstallmentId, PayrollPeriodId) — FIN-VAL-240.
    //
    // MODEL CARRY-FORWARD (keputusan desain task ini, kontrak tidak menetapkan mekanismenya
    // persis — dicatat laporan task, bukan diputuskan sepihak sebagai final):
    // - Setiap kiriman memperbarui PaidAmount/OutstandingAmount baris SASARAN secara langsung.
    //   CK_FinReceivableInstallment_Balance MEWAJIBKAN OutstandingAmount = Scheduled+Carried-Paid,
    //   sehingga baris TIDAK PERNAH "dikosongkan paksa" — ini sekaligus membuktikan baris TERTUNGGAK
    //   atau TERBAYAR_SEBAGIAN secara sah dapat menerima kiriman lanjutan (sesuai
    //   state-transition-matrix.md O.2), karena nilainya tetap konsisten lewat formula itu sendiri.
    // - Pelimpahan ke baris berikutnya (CarriedOverAmount) terjadi TEPAT SEKALI per baris: hanya
    //   saat baris itu PERTAMA KALI meninggalkan status DIJADWALKAN (dijaga lewat pemeriksaan
    //   status SEBELUM pembaruan). Kiriman susulan pada baris yang sama tidak melimpah lagi —
    //   mencegah penghitungan ganda ke baris berikutnya.
    // - Bila baris ini adalah angsuran TERAKHIR pada perjanjian, sisa tidak punya baris tujuan
    //   pelimpahan (FIN-OQ-092 belum terjawab penuh untuk kasus ini) — sisa dibiarkan apa adanya
    //   pada baris itu sendiri, dicatat sebagai keterbatasan di laporan task.
    // ------------------------------------------------------------------------------------

    public async Task<PayrollResultResponse> ProcessResultAsync(
        PayrollResultRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_INSTALLMENT_PAYROLL_RESULT_{request.InstallmentId:N}", cancellationToken);

            var installment = await _dbContext.FinReceivableInstallments
                .SingleOrDefaultAsync(x => x.Id == request.InstallmentId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("InstallmentId tidak ditemukan di Finance.");

            // FIN-VAL-240: kiriman ulang PERSIS (InstallmentId, PayrollPeriodId) yang sama -> 200 idempoten, nol perubahan.
            if (installment.LastPayrollPeriodId == request.PayrollPeriodId)
            {
                return new PayrollResultResponse
                {
                    InstallmentId = installment.Id,
                    InstallmentStatus = installment.Status,
                    OutstandingAmount = installment.OutstandingAmount,
                    IsIdempotentReplay = true
                };
            }

            // Kontrak: "422 - Status angsuran sudah dibatalkan atau bukan dalam status menunggu
            // pembayaran." Menunggu pembayaran = masih bersisa dan belum dibatalkan.
            if (installment.Status is FinReceivableInstallmentStatuses.Terbayar or FinReceivableInstallmentStatuses.Dibatalkan)
                throw new PayrollResultValidationException(
                    $"Status angsuran sudah {installment.Status} sehingga tidak dapat menerima hasil potongan baru.");

            if (request.Status == PayrollResultStatuses.Sebagian && request.DeductedAmount <= 0)
                throw new PayrollResultValidationException(
                    "Hasil pemotongan sebagian wajib memiliki nominal terpotong lebih besar dari nol."); // FIN-VAL-248

            var deductedAmount = request.Status == PayrollResultStatuses.Gagal ? 0m : request.DeductedAmount;

            // FIN-VAL-239: nominal terpotong MUST NOT melebihi sisa angsuran periode itu.
            if (deductedAmount > installment.OutstandingAmount)
                throw new PayrollResultValidationException(
                    $"Nominal potongan {deductedAmount:N2} melebihi sisa angsuran {installment.OutstandingAmount:N2} pada periode {installment.DeductionPeriod}.");

            var wasDijadwalkan = installment.Status == FinReceivableInstallmentStatuses.Dijadwalkan;
            var receivable = await ResolveReceivableAsync(installment, cancellationToken);
            var now = DateTimeOffset.UtcNow;

            if (deductedAmount > 0)
            {
                var balanceBefore = receivable.OutstandingAmount;
                receivable.OutstandingAmount -= deductedAmount;
                // CK_FinReceivable_Balance: OriginalAmount = Outstanding + Allocated + Adjusted +
                // WrittenOff. Potongan gaji bukan ALOKASI-PENERIMAAN (nol FinReceiptAllocation),
                // tetapi formula MEWAJIBKAN penurunan Outstanding diimbangi salah satu dari ketiga
                // bucket lain. AllocatedAmount dipilih (bukan Adjusted/WrittenOff — keduanya
                // bermakna koreksi/penghapusan, bukan pelunasan) — keputusan desain task ini,
                // kontrak tidak menetapkannya, dicatat laporan task.
                receivable.AllocatedAmount += deductedAmount;
                if (receivable.Status == FinReceivableStatuses.Outstanding || receivable.Status == FinReceivableStatuses.Partial)
                {
                    receivable.Status = receivable.OutstandingAmount == 0 ? FinReceivableStatuses.Settled : FinReceivableStatuses.Partial;
                }
                receivable.UpdateDateTime = DateTime.UtcNow;
                receivable.UpdateBy = actorUserId;
                receivable.RowVersion = Guid.NewGuid();

                // FIN-DEC-191: mutasi POTONGAN-GAJI (BE-FIN-092) mengurangi saldo piutang.
                await _subledgerMovementService.RecordReceivableMovementAsync(
                    receivable: receivable,
                    movementType: FinReceivableMovementTypes.PotonganGaji,
                    deltaAmount: -deductedAmount,
                    balanceBefore: balanceBefore,
                    occurredAt: request.ExecutionTimestamp,
                    actorUserId: actorUserId,
                    correlationId: receivable.CorrelationId,
                    causationId: installment.Id,
                    referenceNumber: receivable.ReceivableNumber,
                    notes: $"Potongan gaji angsuran ke-{installment.InstallmentNumber} ({request.Status}) — {request.Notes}",
                    cancellationToken: cancellationToken);
            }

            installment.PaidAmount += deductedAmount;
            installment.OutstandingAmount -= deductedAmount;
            installment.LastResultAt = now;
            installment.LastPayrollPeriodId = request.PayrollPeriodId;
            installment.UpdateDateTime = DateTime.UtcNow;
            installment.UpdateBy = actorUserId;
            installment.RowVersion = Guid.NewGuid();

            installment.Status = installment.OutstandingAmount == 0
                ? FinReceivableInstallmentStatuses.Terbayar
                : request.Status == PayrollResultStatuses.Sebagian
                    ? FinReceivableInstallmentStatuses.TerbayarSebagian
                    : FinReceivableInstallmentStatuses.Tertunggak;

            // Pelimpahan tepat sekali, hanya saat baris ini PERTAMA KALI meninggalkan DIJADWALKAN.
            if (wasDijadwalkan && installment.OutstandingAmount > 0)
            {
                var next = await _dbContext.FinReceivableInstallments
                    .Where(x => !x.IsDelete && x.PlanId == installment.PlanId && x.InstallmentNumber == installment.InstallmentNumber + 1)
                    .SingleOrDefaultAsync(cancellationToken);
                if (next is not null)
                {
                    next.CarriedOverAmount += installment.OutstandingAmount;
                    next.OutstandingAmount += installment.OutstandingAmount;
                    next.UpdateDateTime = DateTime.UtcNow;
                    next.UpdateBy = actorUserId;
                    next.RowVersion = Guid.NewGuid();
                }
                else
                {
                    await _loggerService.AuditAsync(LogCategory, "Result.NoNextInstallment",
                        $"Sisa Rp {installment.OutstandingAmount:N2} tidak dapat dilimpahkan — ini angsuran terakhir pada perjanjian (FIN-OQ-092 belum terjawab penuh untuk kasus ini). InstallmentId={installment.Id}",
                        new { InstallmentId = installment.Id, Remaining = installment.OutstandingAmount });
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            await _loggerService.AuditAsync(LogCategory, "Result.Processed",
                $"Hasil potongan payroll dicatat. InstallmentId={installment.Id} Status={request.Status} Deducted={deductedAmount:N2}",
                new { InstallmentId = installment.Id, request.Status, Deducted = deductedAmount, ActorUserId = actorUserId });

            return new PayrollResultResponse
            {
                InstallmentId = installment.Id,
                InstallmentStatus = installment.Status,
                OutstandingAmount = installment.OutstandingAmount,
                IsIdempotentReplay = false
            };
        }
        catch
        {
            await RollbackAsync(transaction);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    private async Task<FinReceivable> ResolveReceivableAsync(FinReceivableInstallment installment, CancellationToken cancellationToken)
    {
        var plan = installment.Plan ?? await _dbContext.FinReceivableInstallmentPlans
            .SingleOrDefaultAsync(x => x.Id == installment.PlanId, cancellationToken)
            ?? throw new InvalidOperationException("Perjanjian pemilik angsuran ini tidak ditemukan.");

        return await _dbContext.FinReceivables.SingleOrDefaultAsync(x => x.Id == plan.ReceivableId, cancellationToken)
            ?? throw new InvalidOperationException("Kartu piutang pemilik angsuran ini tidak ditemukan.");
    }

    private async Task<IDbContextTransaction?> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        if (!_dbContext.Database.IsRelational()) return null;
        return await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
    }

    private Task AcquireLockAsync(string key, CancellationToken cancellationToken) =>
        _dbContext.Database.IsRelational()
            ? _dbContext.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(hashtext({0}));", [key], cancellationToken)
            : Task.CompletedTask;

    private static Task CommitAsync(IDbContextTransaction? transaction, CancellationToken cancellationToken) =>
        transaction is null ? Task.CompletedTask : transaction.CommitAsync(cancellationToken);

    private static Task RollbackAsync(IDbContextTransaction? transaction) =>
        transaction is null ? Task.CompletedTask : transaction.RollbackAsync(CancellationToken.None);
}

public sealed class FinanceReceivablePayrollSyncResult
{
    public int SyncedCount { get; set; }
    public int SkippedCount { get; set; }
    public List<string> SkipReasons { get; set; } = new();
}

public sealed class PayrollResultValidationException(string message) : Exception(message);
