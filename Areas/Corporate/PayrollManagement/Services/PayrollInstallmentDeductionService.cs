using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Options;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// Fakta angsuran Finance dan perjanjiannya untuk tampilan rincian penerima. Dibaca dengan SATU join untuk seluruh angsuran penerima
/// (bukan N+1). Tidak memuat nomor piutang, jalur berkas, maupun isi dokumen perjanjian: hanya penanda ada atau tidaknya berkas.
/// </summary>
public sealed record InstallmentFacts(
    Guid InstallmentId,
    Guid PlanId,
    string PlanNumber,
    string PlanStatus,
    DateTimeOffset? PlanApprovedAt,
    bool HasAgreementDocument,
    int InstallmentNumber,
    string DeductionPeriod,
    decimal ScheduledAmount,
    decimal CarriedOverAmount,
    decimal PaidAmount,
    decimal OutstandingAmount,
    string Status);

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
/// Satu penerima boleh punya BEBERAPA angsuran sah pada periode yang sama. Setiap angsuran punya komponen sendiri (SourceId = input
/// angsuran), dan batas potongan dihitung SEKALI untuk penerima (kumulatif), lalu dialokasikan berurutan: batas penuh tidak diterapkan
/// berulang untuk tiap angsuran. Urutan alokasi saat batas tidak cukup untuk semua angsuran (periode terlama, perjanjian yang lebih
/// dulu disetujui, nomor angsuran) bersifat SEMENTARA: kebijakan prioritas antar angsuran belum disahkan (NEEDS CONFIRMATION) dan
/// dilaporkan sebagai peringatan, bukan diklaim sebagai aturan resmi.
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
    private const string LegacyLoanComponentCode = "LOAN_HOSPITAL_AR";
    private const string InputDraft = "Draft";
    private const string InputApplied = "Applied";

    /// <summary>Sumber persentase batas potongan: master jenis potongan (ditetapkan) atau nilai bawaan kebijakan (belum disahkan).</summary>
    public const string CapSourceMaster = "MasterDeductionType";
    public const string CapSourcePolicyDefault = "PolicyDefault";

    public const string EvidenceDocumentRecorded = "DocumentRecorded";
    public const string EvidenceDocumentMissing = "DocumentMissing";

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

    /// <summary>
    /// Potongan rencana yang disimpan pada komponen slip (CalculationDetailJson) dan dikonfirmasi saat run disahkan. Parameter
    /// sesudah <c>Status</c> bersifat opsional agar komponen lama (tanpa penanda alokasi) tetap terbaca.
    /// </summary>
    private sealed record PlannedDeduction(
        Guid InstallmentId, string DeductionPeriod, decimal DueAmount, decimal AvailableNetSalary, decimal MaxAllowed,
        decimal DeductedAmount, string Status,
        decimal CapAvailableBefore = 0m, string CapSource = CapSourceMaster, int AllocationOrder = 1);

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

        // Seluruh angsuran Finance yang dirujuk input run ini dibaca sekali (bukan satu query per penerima).
        var installmentIds = employees.SelectMany(e => InstallmentInputs(e)).Select(x => x.SourceId!.Value).Distinct().ToList();
        var installments = installmentIds.Count == 0
            ? new Dictionary<Guid, FinReceivableInstallment>()
            : await _dbContext.FinReceivableInstallments.AsNoTracking()
                .Include(x => x.Plan)
                .Where(x => installmentIds.Contains(x.Id) && !x.IsDelete)
                .ToDictionaryAsync(x => x.Id, cancellationToken);

        // Batas potongan: persentase master jenis potongan bila ditetapkan; bila tidak, nilai bawaan kebijakan yang BELUM disahkan
        // (NEEDS CONFIRMATION). Pemakaian nilai bawaan tidak pernah diam-diam: ia tampil sebagai peringatan run dan sebagai sumber batas
        // pada setiap baris angsuran.
        var masterPercentage = deductionType?.MaxNetSalaryPercentage;
        var capSource = masterPercentage.HasValue ? CapSourceMaster : CapSourcePolicyDefault;
        var capPercentage = masterPercentage ?? _policy.InstallmentDefaultMaxNetSalaryPercentage;
        var policyDefaultUsed = false;

        foreach (var employee in employees.OrderBy(x => x.EmployeeNameSnapshot).ThenBy(x => x.Id))
        {
            var label = employee.EmployeeNameSnapshot;
            var inputs = InstallmentInputs(employee);
            var rawCount = employee.VariableInputs.Count(x => x.SourceType == InstallmentSourceType && !x.IsDelete && x.SourceId.HasValue);
            if (rawCount > inputs.Count)
                summary.Warnings.Add($"{label}: input ganda untuk angsuran yang sama diabaikan; satu angsuran hanya dipotong satu kali.");

            // Angsuran yang layak dipotong: ada di Finance, perjanjian masih DISETUJUI, belum dibatalkan atau terbayar, dan bukan periode
            // yang masih memakai pencatatan manual. Yang tidak layak menjadi penahan dan TIDAK dipotong.
            var eligible = new List<(PrlPayrollVariableInput Input, FinReceivableInstallment Installment)>();
            foreach (var input in inputs)
            {
                if (!installments.TryGetValue(input.SourceId!.Value, out var installment))
                {
                    summary.Blockers.Add($"{label}: angsuran Finance {input.SourceId} tidak ditemukan.");
                    continue;
                }

                var rejection = ExplainIneligible(installment, cutover?.AutomaticFromPeriod);
                if (rejection is not null)
                {
                    summary.Blockers.Add($"{label}: {rejection}");
                    continue;
                }

                eligible.Add((input, installment));
            }

            // Komponen cicilan yang angsurannya tidak lagi layak (dibatalkan, perjanjian berubah, input hilang) dibuang dari slip, sehingga
            // hitung ulang tidak meninggalkan potongan yatim dan tidak pernah menggandakan potongan.
            RemoveStaleComponents(employee, eligible.Select(x => x.Input.Id).ToHashSet(), actorUserId);
            if (eligible.Count == 0) continue;

            // Gaji bruto belum dihitung: batas potong Rp0 hanya karena DATA belum ada. Tidak dicatat sebagai GAGAL (itu menandai
            // angsuran tertunggak di Finance); pengajuan run diblokir sampai data gaji lengkap.
            var grossPay = employee.GrossPay > 0m
                ? employee.GrossPay
                : employee.BaseSalary + employee.TotalRecurringEarning + employee.TotalVariableEarning;
            if (grossPay <= 0m)
            {
                summary.Blockers.Add($"{label}: gaji bruto belum terhitung sehingga potongan cicilan tidak dapat direncanakan.");
                continue;
            }

            var statutoryDeductions = employee.TotalTax + employee.TotalEmployeeInsuranceContribution;
            var otherPriorDeductions = employee.Components
                .Where(c => !c.IsDelete
                    && c.ComponentTypeSnapshot == "Deduction"
                    && c.ComponentCodeSnapshot != ComponentCode
                    && c.ComponentCodeSnapshot != LegacyLoanComponentCode)
                .Sum(c => c.Amount);

            var availableNetSalary = Math.Max(0m, grossPay - statutoryDeductions - otherPriorDeductions);

            // Batas SATU kali untuk penerima (kumulatif seluruh angsuran), dibulatkan ke bawah supaya total tidak pernah melewati batas.
            var maxAllowed = availableNetSalary * (capPercentage / 100m);
            if (deductionType?.MaximumAmount is > 0m)
                maxAllowed = Math.Min(maxAllowed, deductionType.MaximumAmount.Value);
            maxAllowed = RoundDown(Math.Max(0m, Math.Min(maxAllowed, availableNetSalary)));

            if (capSource == CapSourcePolicyDefault) policyDefaultUsed = true;

            var ordered = eligible
                .OrderBy(x => x.Installment.DeductionPeriod, StringComparer.Ordinal)
                .ThenBy(x => x.Installment.Plan?.ApprovedAt ?? DateTimeOffset.MaxValue)
                .ThenBy(x => x.Installment.InstallmentNumber)
                .ThenBy(x => x.Installment.Id)
                .ToList();

            var remainingCap = maxAllowed;
            var totalDeducted = 0m;
            var order = 0;
            foreach (var (input, installment) in ordered)
            {
                order++;
                var dueAmount = input.Amount;
                var deducted = Math.Max(0m, Math.Min(dueAmount, remainingCap));
                var status = deducted >= dueAmount
                    ? PayrollResultStatuses.Berhasil
                    : deducted > 0m ? PayrollResultStatuses.Sebagian : PayrollResultStatuses.Gagal;

                var planned = new PlannedDeduction(
                    installment.Id, installment.DeductionPeriod, dueAmount, availableNetSalary, maxAllowed, deducted, status,
                    remainingCap, capSource, order);
                remainingCap -= deducted;
                totalDeducted += deducted;

                UpsertComponent(employee, input, planned, ordered.Count > 1 ? $"Potongan Cicilan Piutang RS (angsuran ke-{installment.InstallmentNumber})" : "Potongan Cicilan Piutang RS", actorUserId);
                AddItem(summary, employee, planned);
            }

            if (ordered.Count > 1)
                summary.Warnings.Add(
                    $"{label}: {ordered.Count} angsuran dipotong dengan batas kumulatif. Urutan alokasi sementara (periode, tanggal persetujuan perjanjian, nomor angsuran); kebijakan prioritas antar angsuran belum disahkan.");

            employee.TotalDeduction = statutoryDeductions + otherPriorDeductions + totalDeducted;
            employee.NetPay = grossPay - employee.TotalDeduction;
            employee.UpdateDateTime = DateTime.UtcNow;
            employee.UpdateBy = actorUserId;
        }

        if (policyDefaultUsed)
            summary.Warnings.Add(
                $"Batas potongan cicilan memakai nilai bawaan kebijakan ({capPercentage:0.##}% gaji bersih tersedia) karena Master Jenis Potongan tidak menentukan persentase. Nilai ini belum disahkan HRD/Finance; tetapkan di Master Jenis Potongan.");

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
            // SEMUA angsuran penerima dikonfirmasi, bukan hanya yang pertama: potongan setiap angsuran sudah mengurangi gaji bersih.
            foreach (var input in InstallmentInputs(employee))
            {
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
        }

        await _loggerService.AuditAsync(LogCategory, "RunDeductions.Confirmed",
            $"Hasil potongan cicilan run {run.RunNumber} dikonfirmasi ke Finance: {summary.TotalInstallmentsProcessed} angsuran, total terpotong Rp {summary.TotalDeductedAmount:N2}.",
            new { run.Id, summary.TotalInstallmentsProcessed, summary.TotalDeductedAmount, ActorUserId = actorUserId });

        return summary;
    }

    // ------------------------------------------------------------------------------------
    // Pembantu
    // ------------------------------------------------------------------------------------

    /// <summary>
    /// Seluruh input angsuran penerima, satu per angsuran Finance (input ganda untuk angsuran yang sama hanya dihitung satu kali, yang
    /// tertua), berurutan stabil. Dipakai hitung, konfirmasi, dan pembacaan supaya ketiganya melihat himpunan angsuran yang sama.
    /// </summary>
    private static List<PrlPayrollVariableInput> InstallmentInputs(PrlPayrollRunEmployee employee) =>
        employee.VariableInputs
            .Where(x => x.SourceType == InstallmentSourceType && !x.IsDelete && x.SourceId.HasValue)
            .GroupBy(x => x.SourceId!.Value)
            .Select(group => group.OrderBy(x => x.CreateDateTime).ThenBy(x => x.Id).First())
            .OrderBy(x => x.Id)
            .ToList();

    /// <summary>Alasan angsuran tidak boleh dipotong mesin payroll; null bila layak. Tidak pernah memotong karena saldo piutang positif.</summary>
    private static string? ExplainIneligible(FinReceivableInstallment installment, string? automaticFromPeriod)
    {
        // Periode sebelum peralihan tetap memakai pencatatan manual Finance: tidak boleh diproses mesin payroll.
        if (!string.IsNullOrEmpty(automaticFromPeriod) && string.CompareOrdinal(installment.DeductionPeriod, automaticFromPeriod) < 0)
            return $"periode {installment.DeductionPeriod} masih memakai pencatatan manual.";

        var planStatus = installment.Plan?.Status;
        if (!string.Equals(planStatus, FinReceivableInstallmentPlanStatuses.Disetujui, StringComparison.Ordinal))
            return $"perjanjian angsuran {installment.Plan?.PlanNumber} berstatus {planStatus ?? "tidak diketahui"}, bukan DISETUJUI, sehingga tidak dipotong.";

        if (installment.Status is FinReceivableInstallmentStatuses.Dibatalkan or FinReceivableInstallmentStatuses.Terbayar)
            return $"angsuran ke-{installment.InstallmentNumber} berstatus {installment.Status} di Finance, sehingga tidak dipotong lagi.";

        return null;
    }

    private static void RemoveStaleComponents(PrlPayrollRunEmployee employee, ISet<Guid> eligibleInputIds, Guid actorUserId)
    {
        var now = DateTime.UtcNow;
        foreach (var component in employee.Components
                     .Where(c => !c.IsDelete && c.SourceType == ComponentSourceType && c.ComponentCodeSnapshot == ComponentCode
                         && (!c.SourceId.HasValue || !eligibleInputIds.Contains(c.SourceId.Value)))
                     .ToList())
        {
            component.IsDelete = true;
            component.DeleteDateTime = now;
            component.DeleteBy = actorUserId;
        }
    }

    private void UpsertComponent(PrlPayrollRunEmployee employee, PrlPayrollVariableInput input, PlannedDeduction planned, string name, Guid actorUserId)
    {
        var detailJson = JsonSerializer.Serialize(planned);
        var component = employee.Components
            .FirstOrDefault(c => !c.IsDelete && c.SourceType == ComponentSourceType && c.SourceId == input.Id);
        if (component is not null)
        {
            component.ComponentNameSnapshot = name;
            component.Amount = planned.DeductedAmount;
            component.CalculationDetailJson = detailJson;
            component.UpdateDateTime = DateTime.UtcNow;
            component.UpdateBy = actorUserId;
            return;
        }

        _dbContext.PrlPayrollEmployeeComponents.Add(new PrlPayrollEmployeeComponent
        {
            PayrollRunEmployeeId = employee.Id,
            PayrollComponentId = input.PayrollComponentId,
            SourceType = ComponentSourceType,
            SourceId = input.Id,
            ComponentCodeSnapshot = ComponentCode,
            ComponentNameSnapshot = name,
            ComponentTypeSnapshot = "Deduction",
            CalculationMethodSnapshot = "BalanceBased",
            CurrencyCode = employee.CurrencyCode,
            Amount = planned.DeductedAmount,
            IsDisplayedOnPayslip = true,
            CalculationDetailJson = detailJson,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        });
    }

    private static decimal RoundDown(decimal value) => Math.Floor(value * 100m) / 100m;

    private static PlannedDeduction? ReadPlanned(PrlPayrollEmployeeComponent? component)
    {
        if (component is null || string.IsNullOrWhiteSpace(component.CalculationDetailJson)) return null;
        try { return JsonSerializer.Deserialize<PlannedDeduction>(component.CalculationDetailJson); }
        catch (JsonException) { return null; }
    }

    /// <summary>
    /// Baris potongan cicilan PERTAMA pada slip (urutan alokasi) untuk tampilan HRD; dipertahankan untuk pemakai lama yang mengira satu
    /// angsuran per penerima. Tampilan yang butuh seluruh angsuran memakai <see cref="ReadLines"/>.
    /// </summary>
    public static PayrollRunInstallmentLineResponse? ReadLine(PrlPayrollRunEmployee employee) => ReadLines(employee).FirstOrDefault();

    /// <summary>Seluruh baris potongan cicilan penerima: rencana dari hitung terakhir dan apakah sudah dikonfirmasi ke Finance, berurutan menurut alokasi.</summary>
    public static List<PayrollRunInstallmentLineResponse> ReadLines(
        PrlPayrollRunEmployee employee, IReadOnlyDictionary<Guid, InstallmentFacts>? facts = null)
    {
        var lines = new List<PayrollRunInstallmentLineResponse>();
        foreach (var input in InstallmentInputs(employee))
        {
            var component = employee.Components
                .FirstOrDefault(c => !c.IsDelete && c.SourceType == ComponentSourceType && c.SourceId == input.Id);
            InstallmentFacts? installmentFacts = null;
            facts?.TryGetValue(input.SourceId!.Value, out installmentFacts);
            lines.Add(BuildLine(input, component, installmentFacts));
        }

        return lines
            .OrderBy(x => x.AllocationOrder ?? int.MaxValue)
            .ThenBy(x => x.DeductionPeriod, StringComparer.Ordinal)
            .ThenBy(x => x.InstallmentNumber ?? int.MaxValue)
            .ToList();
    }

    /// <summary>Fakta angsuran dan perjanjian untuk banyak angsuran sekaligus (satu query). Murni baca.</summary>
    public async Task<Dictionary<Guid, InstallmentFacts>> LoadInstallmentFactsAsync(
        IReadOnlyCollection<Guid> installmentIds, CancellationToken cancellationToken)
    {
        if (installmentIds.Count == 0) return new Dictionary<Guid, InstallmentFacts>();

        return await _dbContext.FinReceivableInstallments.AsNoTracking()
            .Where(x => installmentIds.Contains(x.Id) && !x.IsDelete)
            .Select(x => new InstallmentFacts(
                x.Id,
                x.PlanId,
                x.Plan != null ? x.Plan.PlanNumber : string.Empty,
                x.Plan != null ? x.Plan.Status : string.Empty,
                x.Plan != null ? x.Plan.ApprovedAt : null,
                x.Plan != null && x.Plan.AgreementDocumentPath != null && x.Plan.AgreementDocumentPath != string.Empty,
                x.InstallmentNumber,
                x.DeductionPeriod,
                x.ScheduledAmount,
                x.CarriedOverAmount,
                x.PaidAmount,
                x.OutstandingAmount,
                x.Status))
            .ToDictionaryAsync(x => x.InstallmentId, cancellationToken);
    }

    /// <summary>
    /// Baris potongan dari satu input angsuran dan komponen rencananya. Dipakai layar run, daftar potongan, dan rekonsiliasi supaya
    /// ketiganya membaca angka yang sama persis. <paramref name="facts"/> (opsional) menambahkan rincian perjanjian dan status Finance.
    /// </summary>
    public static PayrollRunInstallmentLineResponse BuildLine(
        PrlPayrollVariableInput input, PrlPayrollEmployeeComponent? component, InstallmentFacts? facts = null)
    {
        var planned = ReadPlanned(component);
        var confirmed = string.Equals(input.InputStatus, InputApplied, StringComparison.OrdinalIgnoreCase);
        var line = new PayrollRunInstallmentLineResponse
        {
            InstallmentId = input.SourceId!.Value,
            DueAmount = planned?.DueAmount ?? input.Amount,
            DeductionPeriod = planned?.DeductionPeriod ?? facts?.DeductionPeriod,
            AvailableNetSalary = planned?.AvailableNetSalary,
            MaxAllowedDeduction = planned?.MaxAllowed,
            PlannedDeduction = planned?.DeductedAmount,
            PlannedStatus = planned?.Status,
            IsCalculated = planned is not null,
            IsConfirmedToFinance = confirmed,
            CapAvailableForInstallment = planned?.CapAvailableBefore,
            MaxAllowedSource = planned?.CapSource,
            AllocationOrder = planned?.AllocationOrder
        };

        if (planned is { Status: PayrollResultStatuses.Sebagian })
            line.Notes.Add("Dipotong sebagian karena batas potongan; sisanya tetap menjadi kewajiban dan dibawa Finance ke angsuran berikutnya.");
        else if (planned is { Status: PayrollResultStatuses.Gagal })
            line.Notes.Add("Tidak dipotong pada periode ini; angsuran ditandai tertunggak di Finance saat run disahkan.");
        if (planned is { CapSource: CapSourcePolicyDefault })
            line.Notes.Add("Batas potongan memakai nilai bawaan kebijakan yang belum disahkan; tetapkan di Master Jenis Potongan.");

        if (facts is null) return line;

        line.PlanId = facts.PlanId;
        line.PlanNumber = facts.PlanNumber;
        line.InstallmentNumber = facts.InstallmentNumber;
        line.AgreementStatus = facts.PlanStatus;
        line.AgreementApprovedAt = facts.PlanApprovedAt;
        line.HasAgreementDocument = facts.HasAgreementDocument;
        line.AuthorizationEvidenceStatus = facts.HasAgreementDocument ? EvidenceDocumentRecorded : EvidenceDocumentMissing;
        line.ScheduledAmount = facts.ScheduledAmount;
        line.CarriedOverAmount = facts.CarriedOverAmount;
        line.InstallmentStatus = facts.Status;
        if (confirmed)
        {
            line.FinanceDeductedAmount = facts.PaidAmount;
            line.RemainingUnpaidAmount = facts.OutstandingAmount;
        }
        else if (planned is not null)
        {
            line.RemainingUnpaidAmount = Math.Max(0m, planned.DueAmount - planned.DeductedAmount);
        }

        if (string.Equals(facts.PlanStatus, FinReceivableInstallmentPlanStatuses.Menunggu, StringComparison.Ordinal))
            line.Notes.Add("Perjanjian belum disetujui HRD.");
        else if (!string.Equals(facts.PlanStatus, FinReceivableInstallmentPlanStatuses.Disetujui, StringComparison.Ordinal))
            line.Notes.Add($"Perjanjian berstatus {facts.PlanStatus}; angsuran ini tidak boleh dipotong.");

        line.Notes.Add(facts.HasAgreementDocument
            ? "Berkas perjanjian tercatat, tetapi sistem belum memverifikasi isi dan tanda tangannya (tidak ada penanda bukti otorisasi terverifikasi)."
            : "Jalur berkas perjanjian belum tercatat; bukti otorisasi pemotongan tidak dapat diverifikasi sistem.");
        return line;
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
