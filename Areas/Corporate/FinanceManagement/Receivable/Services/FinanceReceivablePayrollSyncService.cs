using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;
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
    private readonly FinanceReceivableInvoiceBatchStatusSynchronizer _batchStatusSynchronizer;

    public FinanceReceivablePayrollSyncService(
        ApplicationDbContext dbContext, LoggerService loggerService, FinanceSubledgerMovementService subledgerMovementService,
        IOptions<FinanceReceivablePayrollSyncOptions> options,
        FinanceReceivableInvoiceBatchStatusSynchronizer batchStatusSynchronizer)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
        _subledgerMovementService = subledgerMovementService;
        _options = options.Value;
        _batchStatusSynchronizer = batchStatusSynchronizer;
    }

    /// <summary>
    /// Mengisi FinReceivableInstallment.TargetPayrollRunEmployeeId belum ada pada skema BE-FIN-092
    /// (temuan drift kontrak, lihat laporan BE-FIN-094 §0) — hasil sinkron karena itu dilaporkan
    /// lewat nilai balik ini, BUKAN disimpan ke kolom Finance mana pun. Dipanggil DI DALAM transaksi
    /// yang sama dengan persetujuan; tidak membuka transaksi sendiri, tidak memanggil SaveChangesAsync
    /// sendiri — caller (ApproveAsync) yang melakukan commit tunggal untuk plan + installment +
    /// seluruh baris HR sekaligus.
    /// </summary>
    public Task<FinanceReceivablePayrollSyncResult> SyncAsync(
        Guid benefitOwnerId,
        IReadOnlyList<FinReceivableInstallment> installments,
        Guid actorUserId,
        CancellationToken cancellationToken) =>
        SyncAsync(benefitOwnerId, null, installments, actorUserId, cancellationToken);

    /// <summary>
    /// Versi berbasis identitas Workforce. <paramref name="benefitOwnerId"/> adalah MstEmployee.Id lama (boleh kosong
    /// untuk dokter tetap); <paramref name="workforceProfileId"/> adalah pemilik canonical. Dokter tanpa data karyawan
    /// hanya disinkronkan bila HR benar-benar memiliki profil payroll aktif untuknya; bila tidak, tidak ada input
    /// potongan fiktif yang dibuat dan alasannya dicatat pada <c>SkipReasons</c>.
    /// </summary>
    public Task<FinanceReceivablePayrollSyncResult> SyncAsync(
        Guid? benefitOwnerId,
        Guid? workforceProfileId,
        IReadOnlyList<FinReceivableInstallment> installments,
        Guid actorUserId,
        CancellationToken cancellationToken) =>
        EvaluateAsync(benefitOwnerId, workforceProfileId, installments, actorUserId, apply: true, cancellationToken);

    /// <summary>
    /// Diagnosis BACA-SAJA: memeriksa prasyarat yang SAMA persis dengan sinkron (konfigurasi, pemilik, komponen, periode, run) tanpa
    /// membuat atau mengubah apa pun. Hasilnya per angsuran: <c>Gap</c> terisi bila ada prasyarat yang gagal; kosong berarti angsuran
    /// itu layak dikirim. Dipakai detail perjanjian supaya alasan "Belum Sinkron" selalu mencerminkan keadaan terkini.
    /// </summary>
    public Task<FinanceReceivablePayrollSyncResult> DiagnoseAsync(
        Guid? benefitOwnerId,
        Guid? workforceProfileId,
        IReadOnlyList<FinReceivableInstallment> installments,
        CancellationToken cancellationToken) =>
        EvaluateAsync(benefitOwnerId, workforceProfileId, installments, Guid.Empty, apply: false, cancellationToken);

    /// <summary>
    /// Satu jalur untuk sinkron dan diagnosis, supaya alasan yang ditampilkan tidak pernah menyimpang dari yang dipakai sinkron.
    /// <paramref name="apply"/> = false tidak menambah entitas apa pun ke change tracker.
    /// </summary>
    private async Task<FinanceReceivablePayrollSyncResult> EvaluateAsync(
        Guid? benefitOwnerId,
        Guid? workforceProfileId,
        IReadOnlyList<FinReceivableInstallment> installments,
        Guid actorUserId,
        bool apply,
        CancellationToken cancellationToken)
    {
        var result = new FinanceReceivablePayrollSyncResult();

        void SkipAll(PayrollSyncGap gap)
        {
            foreach (var installment in installments)
                result.Items.Add(new FinanceReceivablePayrollSyncItemResult(installment.Id, installment.InstallmentNumber, installment.DeductionPeriod, false, gap));
            result.SkipReasons.Add(gap.Message);
            result.SkippedCount = installments.Count;
        }

        if (!_options.Enabled)
        {
            SkipAll(new PayrollSyncGap(
                FinanceInstallmentPayrollSyncReasonCodes.Disabled,
                "Sinkron payroll dimatikan lewat konfigurasi (FinanceReceivablePayrollSync:Enabled = false).",
                "Aktifkan konfigurasi Finance:ReceivablePayrollSync:Enabled (administrator sistem), lalu Kirim Ulang ke Payroll."));
            return result;
        }

        var (target, ownerGap) = await ResolveTargetAsync(benefitOwnerId, workforceProfileId, cancellationToken);
        if (target is null)
        {
            SkipAll(ownerGap!);
            return result;
        }

        var (component, componentGap) = await ResolveComponentAsync(cancellationToken);
        if (component is null)
        {
            SkipAll(componentGap!);
            return result;
        }

        foreach (var installment in installments)
        {
            var (period, periodGap) = await ResolvePeriodWithGapAsync(installment.DeductionPeriod, cancellationToken);
            if (period is null)
            {
                result.Items.Add(new FinanceReceivablePayrollSyncItemResult(installment.Id, installment.InstallmentNumber, installment.DeductionPeriod, false, periodGap));
                result.SkipReasons.Add($"{periodGap!.Message} Angsuran ke-{installment.InstallmentNumber} dilewati.");
                result.SkippedCount++;
                continue;
            }

            // Payroll run adalah milik HRD: Finance TIDAK membuat run maupun baris pegawai. Tanpa run yang valid, angsuran berstatus
            // "Menunggu Payroll Run" dan ditarik otomatis saat HRD membuka run periode itu.
            var existingRun = await FindRunAsync(period, track: apply, cancellationToken);
            if (existingRun is null)
            {
                var awaitingGap = new PayrollSyncGap(
                    FinanceInstallmentPayrollSyncReasonCodes.AwaitingPayrollRun,
                    $"Menunggu Payroll Run: HRD belum membuat payroll run periode {FormatPeriodLabel(installment.DeductionPeriod)}.",
                    "HRD perlu membuka dan menghitung payroll run periode ini. Jadwal angsuran ditarik otomatis ke run; tidak ada run yang dibuat dari Finance.");
                result.Items.Add(new FinanceReceivablePayrollSyncItemResult(installment.Id, installment.InstallmentNumber, installment.DeductionPeriod, false, awaitingGap));
                result.SkipReasons.Add($"{awaitingGap.Message} Angsuran ke-{installment.InstallmentNumber} dilewati.");
                result.SkippedCount++;
                continue;
            }

            // Run yang terkunci, sudah diajukan, atau sudah disahkan tidak menerima input baru: dilewati dengan alasan, bukan ditulis diam-diam.
            if (existingRun.IsLocked || !PayrollRunStatuses.AcceptsInputs(existingRun.RunStatus))
            {
                var lockedGap = new PayrollSyncGap(
                    FinanceInstallmentPayrollSyncReasonCodes.RunLocked,
                    $"Payroll run periode {FormatPeriodLabel(installment.DeductionPeriod)} tidak menerima input baru ({existingRun.RunNumber}, status {existingRun.RunStatus}).",
                    "HRD perlu mengembalikan run ke tahap hitung (tolak/kembalikan) sebelum disahkan, lalu Kirim Ulang ke Payroll. Run yang sudah disahkan dikoreksi lewat pembalikan.");
                result.Items.Add(new FinanceReceivablePayrollSyncItemResult(installment.Id, installment.InstallmentNumber, installment.DeductionPeriod, false, lockedGap));
                result.SkipReasons.Add($"{lockedGap.Message} Angsuran ke-{installment.InstallmentNumber} dilewati.");
                result.SkippedCount++;
                continue;
            }

            var runEmployee = await FindRunEmployeeAsync(existingRun, target, track: apply, cancellationToken);
            if (runEmployee is null)
            {
                var notInRunGap = new PayrollSyncGap(
                    FinanceInstallmentPayrollSyncReasonCodes.EmployeeNotInRun,
                    $"{target.Name} ({target.Number}) tidak termasuk dalam payroll run {existingRun.RunNumber}; tidak ada baris slip fiktif yang dibuat.",
                    "HRD perlu memastikan pegawai memiliki profil payroll aktif dan eligible pada periode ini, lalu Kirim Ulang ke Payroll.");
                result.Items.Add(new FinanceReceivablePayrollSyncItemResult(installment.Id, installment.InstallmentNumber, installment.DeductionPeriod, false, notInRunGap));
                result.SkipReasons.Add($"{notInRunGap.Message} Angsuran ke-{installment.InstallmentNumber} dilewati.");
                result.SkippedCount++;
                continue;
            }

            if (!apply)
            {
                // Layak dikirim; belum ada yang ditulis pada mode diagnosis.
                result.Items.Add(new FinanceReceivablePayrollSyncItemResult(installment.Id, installment.InstallmentNumber, installment.DeductionPeriod, false, null));
                continue;
            }

            await UpsertVariableInputAsync(runEmployee, component, installment, actorUserId, cancellationToken);
            result.Items.Add(new FinanceReceivablePayrollSyncItemResult(installment.Id, installment.InstallmentNumber, installment.DeductionPeriod, true, null));
            result.SyncedCount++;
        }

        if (apply && result.SkipReasons.Count > 0)
        {
            await _loggerService.AuditAsync(LogCategory, "Sync.PartialGap",
                $"Sinkron payroll sebagian terlewat. BenefitOwnerId={benefitOwnerId} Disinkron={result.SyncedCount} Dilewati={result.SkippedCount}",
                new
                {
                    BenefitOwnerId = benefitOwnerId, WorkforceProfileId = workforceProfileId, result.SyncedCount, result.SkippedCount,
                    result.SkipReasons,
                    ReasonCodes = result.Items.Where(x => x.Gap is not null).Select(x => x.Gap!.Code).Distinct().ToList()
                });
        }

        return result;
    }

    /// <summary>
    /// benefitOwnerId = FinReceivable.BenefitOwnerId, secara logis menunjuk MstEmployee.Id (data-dictionary.md §7, FIN-DEC-196/199)
    /// — BUKAN WorkforceProfileId langsung. Pemilik tanpa data karyawan (dokter tetap): slip dibangun dari identitas Workforce dan
    /// hanya bila HR memiliki profil payroll aktif untuknya; tanpa itu tidak ada potongan fiktif yang dibuat.
    /// </summary>
    private async Task<(PayrollTarget? Target, PayrollSyncGap? Gap)> ResolveTargetAsync(
        Guid? benefitOwnerId, Guid? workforceProfileId, CancellationToken cancellationToken)
    {
        var hasOwnerId = benefitOwnerId.HasValue && benefitOwnerId.Value != Guid.Empty;
        if (!hasOwnerId && !workforceProfileId.HasValue)
        {
            return (null, new PayrollSyncGap(
                FinanceInstallmentPayrollSyncReasonCodes.OwnerNotIdentified,
                "Pemilik benefit piutang ini tidak teridentifikasi (BenefitOwnerId dan WorkforceProfileId kosong) sehingga jadwal tidak dapat dikirim ke payroll.",
                "Finance perlu melengkapi pemilik benefit pada kartu piutang, lalu HRD melakukan Kirim Ulang ke Payroll."));
        }

        MstEmployee? employee = null;
        if (hasOwnerId)
        {
            employee = await _dbContext.MstEmployees.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == benefitOwnerId!.Value && !x.IsDelete, cancellationToken);
        }
        if (employee is null && workforceProfileId.HasValue)
        {
            var profileId = workforceProfileId.Value;
            employee = await _dbContext.MstEmployees.AsNoTracking()
                .SingleOrDefaultAsync(x => x.WorkforceProfileId == profileId && !x.IsDelete, cancellationToken);
        }

        if (employee is not null)
            return (new PayrollTarget(employee.WorkforceProfileId, employee.Id, employee.EmployeeNumber, employee.FullName), null);

        if (workforceProfileId.HasValue)
        {
            var profileId = workforceProfileId.Value;
            var doctor = await _dbContext.MstDoctors.AsNoTracking()
                .Where(x => x.WorkforceProfileId == profileId && !x.IsDelete)
                .Select(x => new { x.DoctorNumber, x.FullName })
                .FirstOrDefaultAsync(cancellationToken);
            if (doctor is not null)
            {
                var hasPayroll = await _dbContext.WfpPayrolls.AsNoTracking()
                    .AnyAsync(x => x.WorkforceProfileId == profileId && !x.IsDelete && x.IsPayrollEligible, cancellationToken);
                if (!hasPayroll)
                {
                    return (null, new PayrollSyncGap(
                        FinanceInstallmentPayrollSyncReasonCodes.DoctorProfileMissing,
                        $"Dokter {doctor.FullName} ({doctor.DoctorNumber}) belum memiliki profil payroll yang dikonfigurasi di HR — " +
                        "potongan gaji tidak dibuat.",
                        "HRD perlu melengkapi profil payroll dokter (eligible), lalu Kirim Ulang. Bila tidak berlaku, gunakan mekanisme pembayaran piutang lain."));
                }
                return (new PayrollTarget(profileId, null, doctor.DoctorNumber, doctor.FullName), null);
            }
        }

        return (null, new PayrollSyncGap(
            FinanceInstallmentPayrollSyncReasonCodes.EmployeeNotFound,
            $"Profil pegawai atau dokter untuk BenefitOwnerId {benefitOwnerId} / WorkforceProfileId {workforceProfileId} tidak ditemukan di data HR.",
            "Periksa identitas pemilik benefit pada kartu piutang dan data pegawai/dokter di HR, lalu Kirim Ulang ke Payroll."));
    }

    /// <summary>
    /// Komponen potongan dicari lewat kode yang dikonfigurasi dan MUST aktif berjenis Deduction. Jenis lain dianggap salah
    /// konfigurasi (komponen Earning akan MENAMBAH gaji, bukan memotongnya) dan tidak dipakai. Finance tidak pernah membuatnya.
    /// </summary>
    private async Task<(MstPayrollComponent? Component, PayrollSyncGap? Gap)> ResolveComponentAsync(CancellationToken cancellationToken)
    {
        var code = _options.PayrollComponentCode;
        var candidates = await _dbContext.MstPayrollComponents.AsNoTracking()
            .Where(x => x.PayrollComponentCode == code && !x.IsDelete)
            .ToListAsync(cancellationToken);

        if (candidates.Count == 0)
        {
            return (null, new PayrollSyncGap(
                FinanceInstallmentPayrollSyncReasonCodes.ComponentNotFound,
                $"Komponen payroll '{code}' belum dikonfigurasi di Master Komponen Payroll (MstPayrollComponent).",
                $"HRD perlu membuat komponen payroll dengan kode '{code}' berjenis Deduction pada master data payroll, lalu Kirim Ulang ke Payroll."));
        }

        var component = candidates.FirstOrDefault(x => x.IsActive);
        if (component is null)
        {
            return (null, new PayrollSyncGap(
                FinanceInstallmentPayrollSyncReasonCodes.ComponentInactive,
                $"Komponen payroll '{code}' ada tetapi tidak aktif.",
                $"HRD perlu mengaktifkan komponen '{code}' pada master data payroll, lalu Kirim Ulang ke Payroll."));
        }

        if (!string.Equals(component.ComponentType, "Deduction", StringComparison.OrdinalIgnoreCase))
        {
            return (null, new PayrollSyncGap(
                FinanceInstallmentPayrollSyncReasonCodes.ComponentNotDeduction,
                $"Komponen payroll '{code}' berjenis '{component.ComponentType}', bukan Deduction, sehingga tidak dapat dipakai untuk potongan gaji.",
                $"HRD perlu mengubah jenis komponen '{code}' menjadi Deduction (atau mengoreksi kode komponen pada konfigurasi), lalu Kirim Ulang ke Payroll."));
        }

        return (component, null);
    }

    /// <summary>Find-only — MstPayrollPeriod adalah kalender HR, Finance tidak membuatnya. Periode nonaktif/terkunci tidak menerima input.</summary>
    private async Task<(MstPayrollPeriod? Period, PayrollSyncGap? Gap)> ResolvePeriodWithGapAsync(
        string periodText, CancellationToken cancellationToken)
    {
        var period = await ResolvePeriodAsync(periodText, cancellationToken);
        var label = FormatPeriodLabel(periodText);

        if (period is null)
        {
            return (null, new PayrollSyncGap(
                FinanceInstallmentPayrollSyncReasonCodes.PeriodNotFound,
                $"Periode Payroll {label} belum dikonfigurasi (MstPayrollPeriod, jenis Monthly).",
                $"HRD perlu membuat master Periode Payroll {label} (jenis Monthly), lalu Kirim Ulang ke Payroll."));
        }
        if (!period.IsActive)
        {
            return (null, new PayrollSyncGap(
                FinanceInstallmentPayrollSyncReasonCodes.PeriodInactive,
                $"Periode Payroll {label} ada tetapi tidak aktif.",
                $"HRD perlu mengaktifkan Periode Payroll {label}, lalu Kirim Ulang ke Payroll."));
        }
        if (period.IsLocked)
        {
            return (null, new PayrollSyncGap(
                FinanceInstallmentPayrollSyncReasonCodes.PeriodLocked,
                $"Periode Payroll {label} sudah terkunci.",
                $"HRD perlu membuka kunci Periode Payroll {label} atau menangani potongan angsuran ini secara terpisah, lalu Kirim Ulang."));
        }
        return (period, null);
    }

    /// <summary>"2026-12" -> "Desember 2026" (bahasa Indonesia, tanpa bergantung pada budaya server).</summary>
    private static string FormatPeriodLabel(string period)
    {
        string[] months =
        {
            "Januari", "Februari", "Maret", "April", "Mei", "Juni", "Juli", "Agustus", "September", "Oktober", "November", "Desember"
        };
        if (period.Length >= 7 && int.TryParse(period[..4], out var year) && int.TryParse(period[5..7], out var month) && month is >= 1 and <= 12)
            return $"{months[month - 1]} {year}";
        return period;
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

    /// <summary>Run Regular terbaru pada periode itu (dilacak hanya saat sinkron akan memakainya).</summary>
    private async Task<TrxPayrollRun?> FindRunAsync(MstPayrollPeriod period, bool track, CancellationToken cancellationToken)
    {
        var query = track ? _dbContext.TrxPayrollRuns.AsQueryable() : _dbContext.TrxPayrollRuns.AsNoTracking();
        return await query
            .Where(x => !x.IsDelete && x.PayrollPeriodId == period.Id && x.RunType == "Regular"
                && x.RunStatus != PayrollRunStatuses.Cancelled && x.RunStatus != PayrollRunStatuses.Reversed)
            .OrderByDescending(x => x.CreateDateTime)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private sealed record PayrollTarget(Guid WorkforceProfileId, Guid? EmployeeId, string Number, string Name);

    /// <summary>
    /// Find-only. Baris pegawai pada payroll run dibuat HRD dari profil payroll aktif; Finance tidak membuat baris slip (tanpa gaji dan
    /// profil) sebagai jalan pintas.
    /// </summary>
    private async Task<TrxPayrollRunEmployee?> FindRunEmployeeAsync(
        TrxPayrollRun run, PayrollTarget employee, bool track, CancellationToken cancellationToken)
    {
        var query = track ? _dbContext.TrxPayrollRunEmployees.AsQueryable() : _dbContext.TrxPayrollRunEmployees.AsNoTracking();
        return await query
            .Where(x => !x.IsDelete && x.PayrollRunId == run.Id && x.WorkforceProfileId == employee.WorkforceProfileId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Menarik jadwal angsuran periode run ke baris pegawai pada run HRD yang SUDAH ada (arah tarik, bukan dorong). Hanya angsuran
    /// DIJADWALKAN dari perjanjian DISETUJUI, dan hanya untuk periode sejak peralihan (AutomaticFromPeriod); periode sebelum
    /// peralihan tetap lewat Potong Gaji Manual Finance supaya kedua jalur tidak memproses angsuran yang sama. Idempoten: input
    /// variabel di-upsert per angsuran. Dipanggil di dalam transaksi pemanggil; tidak menyimpan perubahan sendiri.
    /// </summary>
    public async Task<FinanceReceivablePayrollSyncResult> SyncScheduledForRunAsync(
        MstPayrollPeriod period, Guid actorUserId, CancellationToken cancellationToken)
    {
        var aggregate = new FinanceReceivablePayrollSyncResult();
        var periodCode = $"{period.FiscalYear:D4}-{period.PeriodNumber:D2}";

        var cutover = await _dbContext.MstPayrollDeductionCutovers.AsNoTracking()
            .FirstOrDefaultAsync(x => x.IsActive && !x.IsDelete, cancellationToken);
        if (cutover is not null && string.CompareOrdinal(periodCode, cutover.AutomaticFromPeriod) < 0) return aggregate;

        var scheduled = await _dbContext.FinReceivableInstallments
            .Include(x => x.Plan)
            .Where(x => !x.IsDelete
                && x.DeductionPeriod == periodCode
                && x.Status == FinReceivableInstallmentStatuses.Dijadwalkan
                && x.Plan != null && !x.Plan.IsDelete
                && x.Plan.Status == FinReceivableInstallmentPlanStatuses.Disetujui)
            .ToListAsync(cancellationToken);
        if (scheduled.Count == 0) return aggregate;

        var receivableIds = scheduled.Select(x => x.Plan!.ReceivableId).Distinct().ToList();
        var owners = await _dbContext.FinReceivables.AsNoTracking()
            .Where(r => receivableIds.Contains(r.Id))
            .Select(r => new { r.Id, r.BenefitOwnerId, r.BenefitOwnerWorkforceProfileId })
            .ToDictionaryAsync(r => r.Id, cancellationToken);

        foreach (var group in scheduled.GroupBy(x => x.Plan!.ReceivableId))
        {
            owners.TryGetValue(group.Key, out var owner);
            var partial = await EvaluateAsync(
                owner?.BenefitOwnerId, owner?.BenefitOwnerWorkforceProfileId, group.ToList(), actorUserId, apply: true, cancellationToken);
            aggregate.SyncedCount += partial.SyncedCount;
            aggregate.SkippedCount += partial.SkippedCount;
            aggregate.SkipReasons.AddRange(partial.SkipReasons);
            aggregate.Items.AddRange(partial.Items);
        }

        return aggregate;
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
            // Input yang sudah keluar dari Draft (sudah diproses/disetujui payroll) tidak disentuh lagi: kirim ulang tidak boleh
            // mengubah angka yang sudah dipakai perhitungan gaji.
            if (!string.Equals(existing.InputStatus, "Draft", StringComparison.OrdinalIgnoreCase)) return;

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
        // FIN-VAL-287: Panggilan langsung ke penerima hasil potongan tanpa PayrollRunId MUST ditolak; hasil manual hanya lewat batch.
        if (request.PayrollRunId is null || request.PayrollRunId == Guid.Empty)
        {
            throw new PayrollResultValidationException("Hasil potongan manual wajib melalui batch hasil potongan.");
        }

        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_INSTALLMENT_PAYROLL_RESULT_{request.InstallmentId:N}", cancellationToken);

            var result = await ApplyResultCoreAsync(request, actorUserId, cancellationToken);

            await CommitAsync(transaction, cancellationToken);
            return result;
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

    /// <summary>
    /// Inti penerapan hasil potongan gaji ke angsuran Finance (BE-FIN-110, BE-FIN-114).
    /// Dapat dipanggil di dalam transaksi mesin payroll HR (in-process) maupun dibungkus ProcessResultAsync.
    /// Menegakkan FIN-VAL-288 (penolakan periode sebelum peralihan), FIN-VAL-240 (idempotensi),
    /// FIN-VAL-239 (batas nominal), FIN-VAL-248 (nominal sebagian), mutasi subledger, dan pelimpahan sisa tunggakan.
    /// </summary>
    public async Task<PayrollResultResponse> ApplyResultCoreAsync(
        PayrollResultRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var installment = await _dbContext.FinReceivableInstallments
            .SingleOrDefaultAsync(x => x.Id == request.InstallmentId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("InstallmentId tidak ditemukan di Finance.");

        // FIN-VAL-288: Hasil mesin (PayrollRunId terisi) untuk periode sebelum peralihan ditolak (AC 4).
        if (request.PayrollRunId.HasValue && request.PayrollRunId.Value != Guid.Empty)
        {
            var cutover = await _dbContext.MstPayrollDeductionCutovers.AsNoTracking()
                .FirstOrDefaultAsync(x => x.IsActive && !x.IsDelete, cancellationToken);
            if (cutover is not null && string.CompareOrdinal(installment.DeductionPeriod, cutover.AutomaticFromPeriod) < 0)
            {
                throw new PayrollResultValidationException($"Periode {installment.DeductionPeriod} masih memakai pencatatan manual.");
            }
        }

        // FIN-VAL-240: kiriman ulang PERSIS (InstallmentId, PayrollPeriodId) yang sama -> 200 idempoten, nol perubahan.
        if (installment.LastPayrollPeriodId == request.PayrollPeriodId)
        {
            return new PayrollResultResponse
            {
                InstallmentId = installment.Id,
                InstallmentStatus = installment.Status,
                OutstandingAmount = installment.OutstandingAmount,
                PaidAmount = installment.PaidAmount,
                CarriedOverToNextPeriod = 0m,
                IsIdempotentReplay = true,
                ProcessedAt = DateTimeOffset.UtcNow
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

        // Sisa piutang Finance bisa lebih kecil daripada sisa angsuran bila ada pembayaran langsung, penyesuaian, atau write-off
        // setelah perjanjian disetujui (jadwal tidak ikut menyesuaikan). Tanpa guard ini potongan lolos pemeriksaan angsuran di atas
        // lalu gagal sebagai pelanggaran CK_FinReceivable_Outstanding saat SaveChanges (500 dan transaksi batch/run gagal tanpa
        // penjelasan). Ditolak bersih sebelum ada perubahan apa pun; rekonsiliasi dilakukan oleh petugas.
        if (deductedAmount > receivable.OutstandingAmount)
            throw new PayrollResultValidationException(
                $"Nominal potongan {deductedAmount:N2} melebihi sisa piutang Finance {receivable.OutstandingAmount:N2} untuk {receivable.ReceivableNumber}. " +
                "Kemungkinan ada pembayaran langsung atau penyesuaian lain setelah perjanjian disetujui; rekonsiliasi diperlukan sebelum potongan dicatat.");

        if (deductedAmount > 0)
        {
            var balanceBefore = receivable.OutstandingAmount;
            receivable.OutstandingAmount -= deductedAmount;
            // CK_FinReceivable_Balance: OriginalAmount = Outstanding + Allocated + Adjusted + WrittenOff.
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

        // Pelimpahan tepat sekali, hanya saat baris ini PERTAMA KALI meninggalkan DIJADWALKAN (AC 1).
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
                    $"Sisa Rp {installment.OutstandingAmount:N2} tidak dapat dilimpahkan — ini angsuran terakhir pada perjanjian. InstallmentId={installment.Id}",
                    new { InstallmentId = installment.Id, Remaining = installment.OutstandingAmount });
            }
        }

        // Invoice AR piutang karyawan (bila sudah dibuat) mengikuti saldo piutang: ISSUED -> PARTIALLY_PAID -> PAID. Tanpa batch = tanpa efek.
        await _batchStatusSynchronizer.SynchronizeByReceivableIdsAsync(new[] { receivable.Id }, actorUserId, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _loggerService.AuditAsync(LogCategory, "Result.Processed",
            $"Hasil potongan payroll dicatat. InstallmentId={installment.Id} Status={request.Status} Deducted={deductedAmount:N2}",
            new { InstallmentId = installment.Id, request.Status, Deducted = deductedAmount, ActorUserId = actorUserId });

        return new PayrollResultResponse
        {
            InstallmentId = installment.Id,
            InstallmentStatus = installment.Status,
            OutstandingAmount = installment.OutstandingAmount,
            PaidAmount = installment.PaidAmount,
            CarriedOverToNextPeriod = (wasDijadwalkan && installment.OutstandingAmount > 0) ? installment.OutstandingAmount : 0m,
            IsIdempotentReplay = false,
            ProcessedAt = now
        };
    }

    /// <summary>
    /// Inti penerapan koreksi hasil potongan gaji (BE-FIN-118, FIN-DEC-222, FIN-DES-107, FIN-VAL-285).
    /// Menerapkan selisih (positif POTONGAN-GAJI, negatif PEMBALIKAN-POTONGAN-GAJI), menyesuaikan saldo kartu piutang
    /// dan angsuran, serta menyesuaikan CarriedOverAmount angsuran berikutnya bila belum menerima hasil.
    /// Tidak membuka transaksi sendiri agar dapat dipanggil di dalam transaksi atomik batch pengesahan koreksi.
    ///
    /// <paramref name="fromPayrollRun"/> membedakan dua jalur yang tidak boleh tumpang tindih (satu jalur per periode):
    /// koreksi MANUAL (batch koreksi Finance) hanya untuk periode sebelum peralihan; koreksi dari PAYROLL RUN HRD (pembalikan
    /// bertanda tangan atas hasil yang sudah disahkan) hanya untuk periode sejak peralihan. Koreksi bersifat idempoten karena
    /// dihitung dari selisih terhadap nominal terpotong saat ini.
    /// </summary>
    public async Task<PayrollResultResponse> ApplyCorrectionCoreAsync(
        Guid installmentId,
        decimal newDeductedAmount,
        string newResultStatus,
        Guid actorUserId,
        string? notes,
        CancellationToken cancellationToken,
        bool fromPayrollRun = false)
    {
        var installment = await _dbContext.FinReceivableInstallments
            .Include(x => x.Plan)
            .SingleOrDefaultAsync(x => x.Id == installmentId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("InstallmentId tidak ditemukan di Finance.");

        // FIN-VAL-286: Koreksi manual untuk periode >= peralihan mesin ditolak
        var cutover = await _dbContext.MstPayrollDeductionCutovers.AsNoTracking()
            .FirstOrDefaultAsync(x => x.IsActive && !x.IsDelete, cancellationToken);
        if (!fromPayrollRun && cutover is not null && string.CompareOrdinal(installment.DeductionPeriod, cutover.AutomaticFromPeriod) >= 0)
        {
            throw new PayrollResultValidationException(
                $"Periode {installment.DeductionPeriod} sudah ditangani mesin payroll; pencatatan manual tidak diizinkan.");
        }
        if (fromPayrollRun && cutover is not null && string.CompareOrdinal(installment.DeductionPeriod, cutover.AutomaticFromPeriod) < 0)
        {
            throw new PayrollResultValidationException(
                $"Periode {installment.DeductionPeriod} masih memakai pencatatan manual; koreksinya lewat batch koreksi Finance.");
        }

        // FIN-VAL-285: Angsuran berikutnya pada perjanjian yang sama MUST belum menerima hasil (AC 2)
        var next = await _dbContext.FinReceivableInstallments
            .Where(x => !x.IsDelete && x.PlanId == installment.PlanId && x.InstallmentNumber == installment.InstallmentNumber + 1)
            .SingleOrDefaultAsync(cancellationToken);

        if (next is not null && (next.Status != FinReceivableInstallmentStatuses.Dijadwalkan || next.PaidAmount > 0))
        {
            var code = installment.Plan is not null ? $"{installment.Plan.PlanNumber}-{installment.InstallmentNumber}" : installment.Id.ToString();
            throw new PayrollResultValidationException(
                $"Koreksi angsuran {code} tidak dapat dilakukan karena angsuran berikutnya sudah menerima hasil. Gunakan penyesuaian piutang.");
        }

        var maxDue = installment.ScheduledAmount + installment.CarriedOverAmount;
        if (newDeductedAmount > maxDue)
        {
            throw new PayrollResultValidationException(
                $"Nominal potongan {newDeductedAmount:N2} melebihi sisa angsuran {maxDue:N2} pada periode {installment.DeductionPeriod}.");
        }

        if (newResultStatus == PayrollResultStatuses.Sebagian && newDeductedAmount <= 0)
        {
            throw new PayrollResultValidationException(
                "Hasil pemotongan sebagian wajib memiliki nominal terpotong lebih besar dari nol.");
        }

        if (newResultStatus == PayrollResultStatuses.Gagal)
        {
            newDeductedAmount = 0m;
        }

        var previousDeducted = installment.PaidAmount;
        var delta = newDeductedAmount - previousDeducted;
        var receivable = await ResolveReceivableAsync(installment, cancellationToken);
        var now = DateTimeOffset.UtcNow;

        if (delta < 0)
        {
            // Pembalikan potongan (AC 1): saldo piutang naik kembali
            var reversalAmount = -delta;
            var balanceBefore = receivable.OutstandingAmount;
            receivable.OutstandingAmount += reversalAmount;
            receivable.AllocatedAmount -= reversalAmount;
            if (receivable.Status == FinReceivableStatuses.Settled)
            {
                receivable.Status = FinReceivableStatuses.Partial;
            }
            receivable.UpdateDateTime = DateTime.UtcNow;
            receivable.UpdateBy = actorUserId;
            receivable.RowVersion = Guid.NewGuid();

            // Mutasi subledger PEMBALIKAN-POTONGAN-GAJI (AC 1)
            await _subledgerMovementService.RecordReceivableMovementAsync(
                receivable: receivable,
                movementType: FinReceivableMovementTypes.PembalikanPotonganGaji,
                deltaAmount: reversalAmount,
                balanceBefore: balanceBefore,
                occurredAt: now,
                actorUserId: actorUserId,
                correlationId: receivable.CorrelationId,
                causationId: installment.Id,
                referenceNumber: receivable.ReceivableNumber,
                notes: $"Pembalikan potongan gaji angsuran ke-{installment.InstallmentNumber} ({newResultStatus}) — {notes}",
                cancellationToken: cancellationToken);

            // Tunggakan periode berikutnya naik menyesuaikan pembalikan (AC 1)
            if (next is not null)
            {
                next.CarriedOverAmount += reversalAmount;
                next.OutstandingAmount += reversalAmount;
                next.UpdateDateTime = DateTime.UtcNow;
                next.UpdateBy = actorUserId;
                next.RowVersion = Guid.NewGuid();
            }
        }
        else if (delta > 0)
        {
            // Tambahan potongan: saldo piutang turun
            var additionalAmount = delta;
            var balanceBefore = receivable.OutstandingAmount;
            receivable.OutstandingAmount -= additionalAmount;
            receivable.AllocatedAmount += additionalAmount;
            if (receivable.Status == FinReceivableStatuses.Outstanding || receivable.Status == FinReceivableStatuses.Partial)
            {
                receivable.Status = receivable.OutstandingAmount == 0 ? FinReceivableStatuses.Settled : FinReceivableStatuses.Partial;
            }
            receivable.UpdateDateTime = DateTime.UtcNow;
            receivable.UpdateBy = actorUserId;
            receivable.RowVersion = Guid.NewGuid();

            // Mutasi subledger POTONGAN-GAJI
            await _subledgerMovementService.RecordReceivableMovementAsync(
                receivable: receivable,
                movementType: FinReceivableMovementTypes.PotonganGaji,
                deltaAmount: -additionalAmount,
                balanceBefore: balanceBefore,
                occurredAt: now,
                actorUserId: actorUserId,
                correlationId: receivable.CorrelationId,
                causationId: installment.Id,
                referenceNumber: receivable.ReceivableNumber,
                notes: $"Tambahan potongan gaji angsuran ke-{installment.InstallmentNumber} ({newResultStatus}) — {notes}",
                cancellationToken: cancellationToken);

            // Tunggakan periode berikutnya berkurang
            if (next is not null)
            {
                next.CarriedOverAmount = Math.Max(0m, next.CarriedOverAmount - additionalAmount);
                next.OutstandingAmount = Math.Max(0m, next.OutstandingAmount - additionalAmount);
                next.UpdateDateTime = DateTime.UtcNow;
                next.UpdateBy = actorUserId;
                next.RowVersion = Guid.NewGuid();
            }
        }

        installment.PaidAmount = newDeductedAmount;
        installment.OutstandingAmount = installment.ScheduledAmount + installment.CarriedOverAmount - installment.PaidAmount;
        installment.LastResultAt = now;
        installment.UpdateDateTime = DateTime.UtcNow;
        installment.UpdateBy = actorUserId;
        installment.RowVersion = Guid.NewGuid();

        installment.Status = installment.OutstandingAmount == 0
            ? FinReceivableInstallmentStatuses.Terbayar
            : newDeductedAmount > 0
                ? FinReceivableInstallmentStatuses.TerbayarSebagian
                : FinReceivableInstallmentStatuses.Tertunggak;

        await _batchStatusSynchronizer.SynchronizeByReceivableIdsAsync(new[] { receivable.Id }, actorUserId, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "Correction.Processed",
            $"Koreksi hasil potongan dicatat. InstallmentId={installment.Id} Status={newResultStatus} NewDeducted={newDeductedAmount:N2} Delta={delta:N2}",
            new { InstallmentId = installment.Id, Status = newResultStatus, NewDeducted = newDeductedAmount, Delta = delta, ActorUserId = actorUserId });

        return new PayrollResultResponse
        {
            InstallmentId = installment.Id,
            InstallmentStatus = installment.Status,
            OutstandingAmount = installment.OutstandingAmount,
            PaidAmount = installment.PaidAmount,
            CarriedOverToNextPeriod = next is not null ? next.CarriedOverAmount : 0m,
            IsIdempotentReplay = false,
            ProcessedAt = now
        };
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

    /// <summary>Hasil per angsuran (aditif; SkipReasons tetap ada untuk pemanggil lama).</summary>
    public List<FinanceReceivablePayrollSyncItemResult> Items { get; set; } = new();
}

/// <summary>Satu angsuran: Synced = input payroll ditulis pada pemanggilan ini; Gap = prasyarat yang gagal (null bila layak/berhasil).</summary>
public sealed record FinanceReceivablePayrollSyncItemResult(
    Guid InstallmentId, int InstallmentNumber, string DeductionPeriod, bool Synced, PayrollSyncGap? Gap);

/// <summary>Alasan yang dapat ditindaklanjuti: kode stabil, pesan untuk pengguna, dan tindakan yang diperlukan.</summary>
public sealed record PayrollSyncGap(string Code, string Message, string Action);

/// <summary>Kode alasan sinkron payroll. Nilainya dihasilkan validasi, bukan ditulis ke setiap respons secara tetap.</summary>
public static class FinanceInstallmentPayrollSyncReasonCodes
{
    public const string Disabled = "PAYROLL_SYNC_DISABLED";
    public const string OwnerNotIdentified = "PAYROLL_OWNER_NOT_IDENTIFIED";
    public const string EmployeeNotFound = "PAYROLL_EMPLOYEE_NOT_FOUND";
    public const string DoctorProfileMissing = "PAYROLL_DOCTOR_PROFILE_MISSING";
    public const string ComponentNotFound = "PAYROLL_COMPONENT_NOT_FOUND";
    public const string ComponentInactive = "PAYROLL_COMPONENT_INACTIVE";
    public const string ComponentNotDeduction = "PAYROLL_COMPONENT_NOT_DEDUCTION";
    public const string PeriodNotFound = "PAYROLL_PERIOD_NOT_FOUND";
    public const string PeriodInactive = "PAYROLL_PERIOD_INACTIVE";
    public const string PeriodLocked = "PAYROLL_PERIOD_LOCKED";
    public const string RunLocked = "PAYROLL_RUN_LOCKED";

    /// <summary>HRD belum membuat payroll run periode itu. Finance tidak pernah membuat run; jadwal ditarik otomatis saat HRD membuka run.</summary>
    public const string AwaitingPayrollRun = "PAYROLL_AWAITING_RUN";

    /// <summary>Run ada, tetapi pegawai tidak termasuk dalam run (mis. tidak punya profil payroll aktif pada periode itu).</summary>
    public const string EmployeeNotInRun = "PAYROLL_EMPLOYEE_NOT_IN_RUN";

    /// <summary>Semua prasyarat terpenuhi tetapi input belum tersimpan (belum pernah dikirim, atau pengiriman terdahulu tidak tersimpan).</summary>
    public const string NotSent = "PAYROLL_NOT_SENT";

    public const string InstallmentNotScheduled = "INSTALLMENT_NOT_SCHEDULED";
}

public sealed class PayrollResultValidationException(string message) : Exception(message);
