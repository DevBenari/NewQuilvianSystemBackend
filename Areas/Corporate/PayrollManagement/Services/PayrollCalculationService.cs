using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Options;
using QuilvianSystemBackend.Repositories;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// Mesin kalkulasi gaji per penerima (PAY-BE-04). Deterministik: hasil hanya bergantung pada snapshot yang sudah diimpor ke run
/// (penetapan gaji disetujui, profil payroll, input kehadiran/lembur/variabel dari HRD), parameter kebijakan terversi, dan periode.
///
/// Rumus (semua nominal dibulatkan 2 desimal per komponen, total = jumlah komponen):
///   Bruto  = gaji pokok (prorata) + tunjangan tetap + lembur + tunjangan kehadiran + penghasilan variabel/manual
///   Potongan (sebelum cicilan) = pajak + iuran karyawan (nilai tetapan profil) + potongan kehadiran/cuti tanpa gaji + potongan variabel/manual
///   Bersih = Bruto - Potongan. Cicilan piutang dipotong SESUDAH ini oleh PayrollInstallmentDeductionService (urutan prioritas).
///
/// PPh dan asuransi swasta: sumbernya konfigurasi HRD (PrlTax dan PrlInsurance), dinilai per periode oleh PayrollTaxInsuranceRules. Tarif
/// PPh (TER, PTKP, lapisan Pasal 17) HANYA dari kebijakan berversi berstatus APPROVED yang berlaku pada periode (PrlTaxPolicy); nilai
/// bawaan Pengaturan Penggajian tidak dipakai. Tanpa kebijakan yang disahkan, atau untuk kombinasi tanpa formula yang sah, hasilnya
/// penahan, bukan Rp0. PPh juga mendukung nominal manual yang sudah disetujui pengguna lain dan pengecualian berdasar. Versi kebijakan,
/// dasar hitung, dan akumulasi tahun berjalan disimpan pada snapshot penerima. Tidak ada persentase atau tarif yang ditanam di kode.
/// Tetapan lama PrlPayroll.TaxAmount hanya cadangan bertanda bila HRD belum mengonfigurasi. Iuran BPJS Kesehatan dan BPJS
/// Ketenagakerjaan TIDAK dihitung (PAY-BE-FIX-007): tidak ada kebijakan tarif BPJS, dasar upah iuran, atau potongan/beban BPJS baru.
/// Asuransi swasta (opsional) tetap dihitung dari profil HRD. Kontribusi pemberi kerja dipisahkan dan tidak mengurangi gaji bersih.
///
/// Berjalan di dalam transaksi pemanggil (PayrollRunService.CalculateAsync); tidak membuka transaksi dan tidak menyimpan perubahan.
/// Baris komponen milik mesin (awalan PAY_) di-upsert per kode; baris yang tidak lagi dihasilkan di-soft-delete, sehingga hitung
/// ulang tidak pernah menggandakan nominal. Komponen non-mesin (mis. restitusi) dihitung SEKALI sebagai penghasilan/potongan.
/// </summary>
public sealed class PayrollCalculationService
{
    public const string EngineVersion = "PAYROLL-CALC-V1";

    private const string EmployeeCalculated = "Calculated";
    private const string EmployeeError = "Error";
    private const string InstallmentComponentCode = "FINANCE_INSTALLMENT_DEDUCTION";
    private const string LegacyLoanComponentCode = "LOAN_HOSPITAL_AR";
    private const string TypeEarning = "Earning";
    private const string TypeDeduction = "Deduction";

    // Input variabel Draft belum diserahkan HRD sehingga tidak dihitung; Submitted ke atas dihitung. Rejected/Cancelled tidak dihitung.
    private static readonly HashSet<string> CountableInputStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Submitted", "Verified", "Approved", "Applied", "Processed", "Posted"
    };

    /// <summary>Status input manual (SourceType = PayrollManualInput) yang ikut dihitung: hanya yang sudah disetujui (PAY-BE-20).</summary>
    private static readonly HashSet<string> ManualCountableInputStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        PayrollManualInputStatuses.Approved, PayrollManualInputStatuses.Applied
    };

    /// <summary>Status payroll run yang hasilnya final dan dihitung sebagai akumulasi PPh tahun berjalan (tanpa Reversed dan Cancelled).</summary>
    internal static readonly string[] AccumulatedRunStatuses =
    {
        PayrollRunStatuses.Approved, PayrollRunStatuses.SentToFinance, PayrollRunStatuses.PartiallyPaid,
        PayrollRunStatuses.Paid, PayrollRunStatuses.Posted, PayrollRunStatuses.Closed
    };

    private static readonly JsonSerializerOptions SnapshotJsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly ApplicationDbContext _dbContext;
    private readonly PayrollPolicyOptions _policy;
    private readonly PayrollStatutoryPolicyResolver _policyResolver;
    private readonly IPayrollOpeningTaxBalanceSource _openingBalanceSource;

    /// <summary>
    /// Keterbatasan dasar PPh yang dicatat pada rincian pajak otomatis: iuran BPJS tidak dihitung di Payroll sehingga premi BPJS pemberi kerja
    /// dan iuran pensiun BPJS pegawai tidak ikut dalam dasar PPh. Status kesiapan BPJS pada payroll produksi menunggu keputusan pemilik.
    /// </summary>
    private const string BpjsTaxBasisLimitation =
        "Dasar PPh tidak memuat premi BPJS pemberi kerja dan tidak mengurangi iuran pensiun BPJS pegawai karena iuran BPJS tidak dihitung di Payroll; "
        + "keputusan perlakuan premi/iuran pada payroll produksi masih menunggu pemilik proses.";

    public PayrollCalculationService(
        ApplicationDbContext dbContext, IOptions<PayrollPolicyOptions> options, PayrollStatutoryPolicyResolver policyResolver,
        IPayrollOpeningTaxBalanceSource openingBalanceSource)
    {
        _dbContext = dbContext;
        _policy = options.Value;
        _policyResolver = policyResolver;
        _openingBalanceSource = openingBalanceSource;
    }

    /// <summary>Hasil snapshot satu penerima pada payroll run final sebelumnya di tahun pajak yang sama.</summary>
    private sealed record PriorTaxRow(DateTime PeriodEnd, PayrollTaxSnapshot? Tax, PayrollInsuranceSnapshot? Insurance);

    /// <summary>Kebijakan regulasi berversi yang berlaku pada periode run dan akumulasi tahun berjalan seluruh penerima run.</summary>
    private sealed record StatutoryContext(
        PayrollTaxPolicy? TaxPolicy,
        IReadOnlyDictionary<Guid, List<PriorTaxRow>> PriorRows,
        IReadOnlyDictionary<Guid, IReadOnlyList<PayrollOpeningTaxBalanceFacts>> OpeningBalances,
        bool OpeningBalanceAvailable);

    private static T? ReadSnapshot<T>(string? json) where T : class
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            return JsonSerializer.Deserialize<T>(json, SnapshotJsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Snapshot PPh dan iuran penerima pada payroll run final sebelum periode ini di tahun pajak yang sama. Dasar akumulasi penghasilan
    /// dan PPh yang sudah dipotong; hanya run yang disahkan (bukan draf) yang dihitung, dan run ini sendiri tidak pernah ikut.
    /// </summary>
    private async Task<Dictionary<Guid, List<PriorTaxRow>>> LoadPriorTaxRowsAsync(
        Guid runId, List<Guid> workforceIds, DateTime periodStart, DateTime periodEnd, CancellationToken cancellationToken)
    {
        var yearStart = new DateTime(periodEnd.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var rows = await _dbContext.PrlPayrollRunEmployees.AsNoTracking()
            .Where(x => !x.IsDelete && workforceIds.Contains(x.WorkforceProfileId) && x.PayrollRunId != runId
                && x.PayrollRun != null && !x.PayrollRun.IsDelete
                && x.PayrollRun.PeriodEndDateSnapshot >= yearStart && x.PayrollRun.PeriodEndDateSnapshot < periodStart
                && AccumulatedRunStatuses.Contains(x.PayrollRun.RunStatus))
            .Select(x => new
            {
                x.WorkforceProfileId,
                PeriodEnd = x.PayrollRun!.PeriodEndDateSnapshot,
                x.TaxSnapshotJson,
                x.InsuranceSnapshotJson
            })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(x => x.WorkforceProfileId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => new PriorTaxRow(
                    x.PeriodEnd, ReadSnapshot<PayrollTaxSnapshot>(x.TaxSnapshotJson), ReadSnapshot<PayrollInsuranceSnapshot>(x.InsuranceSnapshotJson))).ToList());
    }

    /// <summary>
    /// Konsolidasi akumulasi penghasilan, iuran, dan PPh tahun berjalan sebelum masa pajak ini:
    ///   saldo awal (Opening Tax Balance) yang DISETUJUI + snapshot payroll run yang disahkan untuk masa yang belum dicakup saldo awal.
    /// Premi pemberi kerja yang termasuk bruto dan iuran pensiun pengurang dihitung ulang dari baris iuran snapshot dengan daftar program
    /// kebijakan yang berlaku sekarang. Tidak lengkap (IsComplete false) bila: ada payroll run sebelumnya tanpa rincian akumulasi; masa pajak
    /// yang seharusnya ada (sejak mulai bekerja) belum tercakup run disahkan maupun saldo awal; atau cakupan saldo awal beririsan dengan run
    /// disahkan (akan menghitung ganda). Ketiadaan histori TIDAK pernah dianggap saldo nol yang terverifikasi, dan hasil ini hanya dipakai
    /// pada masa pajak terakhir; TER bulanan tidak bergantung padanya.
    /// </summary>
    private static PayrollTaxYtd BuildYtd(
        IReadOnlyList<PriorTaxRow> rows, IReadOnlyList<PayrollOpeningTaxBalanceFacts> openingBalances, bool openingFeatureAvailable,
        PayrollTaxPolicy? policy, string? category, DateTime? joinDate, int taxYear, int taxMonth, bool isLastTaxPeriod = false)
    {
        var issues = new List<string>();
        decimal gross = 0m, premium = 0m, pension = 0m, withheld = 0m;
        var months = new HashSet<int>();
        var openingMonths = new HashSet<int>();
        var missingBreakdown = false;

        foreach (var row in rows)
        {
            months.Add(row.PeriodEnd.Month);
            if (row.Tax is null || row.Tax.TaxableGross is null)
            {
                missingBreakdown = true;
                continue;
            }

            gross += row.Tax.TaxableGross.Value;
            withheld += row.Tax.EmployeeAmount + row.Tax.EmployerBorneAmount;
            if (policy is null || row.Insurance is null) continue;
            foreach (var line in row.Insurance.Lines)
            {
                if (line.PaidBy == "Employer" && policy.EmployerPremiumPrograms.Contains(line.Program)) premium += line.Amount;
                if (line.PaidBy == "Employee" && policy.PensionDeductionPrograms.Contains(line.Program)) pension += line.Amount;
            }
        }

        if (missingBreakdown)
            issues.Add("Akumulasi PPh tahun berjalan tidak lengkap: ada payroll run sebelumnya yang sudah disahkan tanpa rincian akumulasi (dihitung sebelum kebijakan tarif berversi). Gunakan Nominal Manual Disetujui dengan dasar yang sah untuk masa pajak ini.");

        // Saldo awal: hanya yang DISETUJUI, tahun pajak yang sama, dan berasal dari pemberi kerja yang sama sebelum cutover. Saldo dari
        // pemberi kerja sebelumnya tidak dicampur (perlakuannya terpisah dan belum didukung mesin).
        foreach (var opening in openingBalances.Where(o =>
                     o.Status == OpeningTaxBalanceStatuses.Approved && o.TaxYear == taxYear
                     && o.EmployerSource == OpeningTaxBalanceEmployerSources.SameEmployerPreCutover))
        {
            foreach (var month in opening.CoveredMonths.Distinct()) openingMonths.Add(month);
            gross += opening.TaxableGross;
            premium += opening.EmployerPremium;
            pension += opening.PensionDeduction;
            withheld += opening.TaxWithheld;
        }

        var invalidOpeningMonths = openingMonths.Where(m => m < 1 || m > 12 || m >= taxMonth).OrderBy(m => m).ToList();
        if (invalidOpeningMonths.Count > 0)
            issues.Add($"Saldo awal mencakup masa pajak {string.Join(", ", invalidOpeningMonths)} yang di luar rentang sebelum masa pajak {taxMonth} tahun {taxYear}; koreksi saldo awal (versi baru).");

        // Cakupan yang beririsan akan menghitung penghasilan dan PPh dua kali: tidak boleh dipakai sebagai data rekonsiliasi.
        var overlap = openingMonths.Intersect(months).OrderBy(m => m).ToList();
        if (overlap.Count > 0)
            issues.Add($"Cakupan saldo awal beririsan dengan payroll run yang sudah disahkan pada masa pajak {string.Join(", ", overlap)} tahun {taxYear}; penghasilan dan PPh akan terhitung dua kali. Koreksi cakupan saldo awal (versi baru) atau batalkan run yang tidak sah.");

        var covered = new HashSet<int>(months);
        covered.UnionWith(openingMonths);
        var missingMonthsList = new List<int>();
        var missingHistoryIssueAdded = false;

        if (category == TaxRecipientCategories.PermanentEmployee)
        {
            // Bulan sebelum mulai bekerja bukan bulan payroll yang hilang.
            var firstMonth = joinDate.HasValue && joinDate.Value.Year == taxYear ? joinDate.Value.Month : 1;
            missingMonthsList = Enumerable.Range(firstMonth, Math.Max(0, taxMonth - firstMonth)).Where(m => !covered.Contains(m)).ToList();
            if (missingMonthsList.Count > 0)
            {
                var listed = string.Join(", ", missingMonthsList);
                var issueMessage = openingFeatureAvailable
                    ? $"Belum ada payroll run yang disahkan maupun saldo awal (Opening Tax Balance) yang disetujui untuk masa pajak {listed} tahun {taxYear}; akumulasi penghasilan dan PPh tidak lengkap. Lengkapi saldo awal yang telah diverifikasi untuk bulan tersebut."
                    : $"Belum ada payroll run yang disahkan untuk masa pajak {listed} tahun {taxYear}; akumulasi penghasilan dan PPh tidak lengkap. Fitur saldo awal PPh historis belum tersedia (menunggu skema database); gunakan Nominal Manual Disetujui dengan dasar perhitungan dan persetujuan yang sah sebagai jalur pengecualian terkontrol.";

                // Pada masa pajak terakhir, ketiadaan histori adalah penahan rekonsiliasi tahunan Pasal 17.
                // Pada masa pajak biasa (TER bulanan), ketiadaan histori bukan penahan karena TER bulanan hanya memakai penghasilan bulan berjalan,
                // namun daftar bulan hilang tetap disimpan pada MissingMonths untuk transparansi audit dan peringatan.
                if (isLastTaxPeriod)
                {
                    issues.Add(issueMessage);
                    missingHistoryIssueAdded = true;
                }
            }
        }

        return new PayrollTaxYtd(covered.Count, gross, premium, pension, withheld, issues.Count == 0, issues)
        {
            CoveredMonths = covered.OrderBy(m => m).ToList(),
            OpeningBalanceMonths = openingMonths.OrderBy(m => m).ToList(),
            MissingMonths = missingMonthsList,
            OpeningBalanceFeatureAvailable = openingFeatureAvailable,
            // PAY-BE-FIX-030: klasifikasi terstruktur, bukan pencocokan teks: satu-satunya masalah adalah bulan histori yang belum tercakup.
            HistoryGapOnly = missingHistoryIssueAdded && issues.Count == 1
        };
    }

    public sealed class CalculationResult
    {
        public List<string> Blockers { get; } = new();
        public List<string> Warnings { get; } = new();
        public int CalculatedCount { get; set; }
        public int ErrorCount { get; set; }

        /// <summary>Peringatan dan galat per baris pegawai (PrlPayrollRunEmployee.Id), tanpa awalan nama. Dipakai simulasi (PAY-BE-21).</summary>
        public Dictionary<Guid, List<string>> EmployeeWarnings { get; } = new();
        public Dictionary<Guid, List<string>> EmployeeErrors { get; } = new();

        /// <summary>
        /// Penahan konfigurasi PPh/iuran per penerima: konfigurasi HRD belum lengkap atau tidak sah. Penerima tetap terhitung (angka
        /// ditandai), tetapi run tidak dapat diajukan sebelum HRD melengkapinya. Tanpa awalan nama.
        /// </summary>
        public Dictionary<Guid, List<string>> EmployeeBlockers { get; } = new();

        /// <summary>
        /// PAY-BE-FIX-031: jumlah entri <see cref="Blockers"/> yang berasal dari penahan PPh terklasifikasi TAX_YTD_HISTORY_INCOMPLETE
        /// (kode terstruktur dari mesin pajak, bukan teks). Selisihnya dengan jumlah seluruh penahan = penahan lain yang tetap menahan review.
        /// </summary>
        public int TaxHistoryBlockerCount { get; set; }
    }

    private sealed record IdentityInfo(Guid WorkforceProfileId, bool IsActive, DateTime? JoinDate, DateTime? ResignDate);

    /// <summary>SK Promosi/Demosi (HrdPersonnelActionOrder) yang berlaku di periode; hanya identitas dan tanggal, tanpa nominal.</summary>
    private sealed record CompensationActionHint(
        Guid Id, Guid WorkforceProfileId, string OrderNumber, string Category, DateTime EffectiveDate, Guid? LinkedSalaryAssignmentId);

    /// <summary>Snapshot kebijakan yang disalin ke run agar hasil dapat ditelusuri ke versi kebijakan yang dipakai.</summary>
    public string BuildPolicySnapshotJson() => JsonSerializer.Serialize(new
    {
        engine = EngineVersion,
        policyVersion = _policy.PolicyVersion,
        // Penahan Finance Handoff produksi dipertahankan sebagai aturan tersendiri (bukan lagi mode pengujian BPJS): run yang dihitung
        // mesin ini tidak dapat dikirim ke Finance Handoff produksi sampai pemilik proses menetapkan aturan penggantinya.
        financeHandoffHeld = true,
        financeHandoffHoldReason = PayrollFinanceHandoffGuard.HoldReasonPendingOwnerDecision,
        needsConfirmation = true,
        proration = "CalendarDays",
        salaryAssignmentSource = _policy.RequireApprovedSalaryAssignment
            ? "WfpSalaryAssignment utama, aktif, berlaku di periode, dan DISETUJUI (ApprovedAt dan ApprovedByUserId terisi); aktif saja tidak cukup; penetapan belum disetujui menjadi penahan"
            : "WfpSalaryAssignment utama, aktif, dan berlaku di periode (approval tidak disyaratkan; keputusan peniadaan approval 19 Nov 2026)",
        requireApprovedSalaryAssignment = _policy.RequireApprovedSalaryAssignment,
        allowProfileSalaryFallback = _policy.AllowProfileSalaryFallback,
        prorateFixedAllowances = _policy.ProrateFixedAllowances,
        fixedAllowanceSource = "MstSalaryStructureAllowance (PGJ-C1); cadangan PrlPayroll.TotalAllowance bila struktur kosong",
        workforceAllowanceSource = "PrlWorkforceAllowance (Approved, aktif, berlaku di periode; tidak dihitung ganda dengan tunjangan struktur)",
        allowProfileAllowanceFallback = _policy.AllowProfileAllowanceFallback,
        unpaidLeaveDeductionEnabled = _policy.UnpaidLeaveDeductionEnabled,
        requireDifferentApprover = _policy.RequireDifferentApprover,
        installmentDefaultMaxNetSalaryPercentage = _policy.InstallmentDefaultMaxNetSalaryPercentage,
        taxAndInsuranceSource = "Konfigurasi HRD: PrlTax (PPh) dan PrlInsurance (asuransi swasta saja); PrlPayroll.TaxAmount hanya cadangan bertanda; PrlPayroll.InsuranceAmount tidak dipakai",
        taxAutoRegulatory = "Dihitung dari kebijakan tarif PPh berversi berstatus APPROVED yang berlaku pada periode (PrlTaxPolicy); tanpa kebijakan atau tanpa formula yang sah menghasilkan penahan, bukan Rp0",
        taxSupportedModes = new[] { TaxCalculationModes.AutoRegulatory, TaxCalculationModes.ApprovedManualAmount, TaxCalculationModes.ExemptOrNotApplicable },
        taxManualAmountApproval = "Nominal manual dipakai hanya bila sudah disetujui pengguna lain (ManualAmountApprovalStatus = APPROVED)",
        insuranceSource = "Asuransi swasta (opsional) dari profil HRD: peserta ACTIVE, program swasta aktif, berlaku pada periode. Iuran BPJS Kesehatan dan BPJS Ketenagakerjaan tidak dihitung",
        taxableGrossBasis = "komponen penghasilan IsTaxable dikurangi potongan cuti tanpa gaji/mangkir dan potongan kehadiran (penghasilan yang tidak dibayarkan), ditambah premi pemberi kerja menurut kebijakan PPh (NEEDS CONFIRMATION)"
    });

    public async Task<CalculationResult> CalculateRunAsync(
        PrlPayrollRun run, MstPayrollPeriod period, Guid actorUserId, CancellationToken cancellationToken)
    {
        var result = new CalculationResult();

        // Tanggal kalender periode dicap UTC: dipakai juga sebagai pembanding kolom timestamptz (tunjangan transportasi), dan Npgsql
        // menolak Kind=Unspecified yang dibawa kolom date periode.
        var periodStart = PayrollDateNormalizer.ToUtcDate(period.StartDate);
        var periodEnd = PayrollDateNormalizer.ToUtcDate(period.EndDate);
        var totalDays = (periodEnd - periodStart).Days + 1;
        if (totalDays <= 0) throw new PayrollRunValidationException("Rentang tanggal periode payroll tidak valid.");

        var employees = await _dbContext.PrlPayrollRunEmployees
            .Include(x => x.Components)
            .Include(x => x.VariableInputs).ThenInclude(v => v.PayrollComponent)
            .Include(x => x.AttendanceInputs)
            .Include(x => x.OvertimeInputs)
            .Where(x => x.PayrollRunId == run.Id && !x.IsDelete)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
        if (employees.Count == 0) return result;

        var workforceIds = employees.Select(x => x.WorkforceProfileId).Distinct().ToList();
        var profileIds = employees.Where(x => x.PayrollProfileId.HasValue).Select(x => x.PayrollProfileId!.Value).Distinct().ToList();

        var profiles = await _dbContext.PrlPayrolls.AsNoTracking()
            .Where(x => !x.IsDelete && profileIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        // Sakelar Payroll:Policy:RequireApprovedSalaryAssignment (PAY-BE-FIX-008, P0-04; NEEDS CONFIRMATION, bawaan mati). Menyala: hanya
        // penetapan gaji yang DISETUJUI (ApprovedAt dan ApprovedByUserId terisi) yang dipakai menghitung gaji pokok dan menurunkan tunjangan
        // Struktur Gaji; penetapan aktif yang belum disetujui dipisahkan, tidak pernah dibayarkan, dan menjadi penahan (atau sebab gagal
        // bila tidak ada penetapan sah) pada penerima yang bersangkutan. Mati (bawaan): keputusan "peniadaan approval" 19 Nov 2026 berlaku,
        // semua penetapan aktif yang efektif dipakai dan tidak ada yang berstatus menunggu.
        var requireApproval = _policy.RequireApprovedSalaryAssignment;
        bool IsPayable(WfpSalaryAssignment x) => !requireApproval || WfpSalaryAssignmentRules.IsApproved(x.ApprovedAt, x.ApprovedByUserId);
        var periodAssignments = await _dbContext.WfpSalaryAssignments.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive && workforceIds.Contains(x.WorkforceProfileId)
                && x.EffectiveStartDate <= periodEnd
                && (x.EffectiveEndDate == null || x.EffectiveEndDate >= periodStart))
            .ToListAsync(cancellationToken);
        var assignments = periodAssignments
            .Where(IsPayable)
            .GroupBy(x => x.WorkforceProfileId)
            .ToDictionary(g => g.Key, g => g.ToList());
        var pendingAssignments = periodAssignments
            .Where(x => !IsPayable(x))
            .GroupBy(x => x.WorkforceProfileId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // SK Promosi/Demosi yang berlaku di periode: perubahan kompensasinya tidak pernah otomatis (HRD-DEC-036/054), tetapi penetapan gaji
        // barunya harus ada. Dipakai hanya untuk peringatan kesiapan; tidak mengubah angka apa pun.
        var compensationActions = (await _dbContext.HrdPersonnelActionOrders.AsNoTracking()
                .Where(o => !o.IsDelete && workforceIds.Contains(o.WorkforceProfileId)
                    && (o.OrderStatus == "Scheduled" || o.OrderStatus == "Applied")
                    && (o.ActionCategory == "Promotion" || o.ActionCategory == "Demotion")
                    && o.EffectiveDate >= periodStart && o.EffectiveDate <= periodEnd)
                .Select(o => new CompensationActionHint(o.Id, o.WorkforceProfileId, o.ActionOrderNumber, o.ActionCategory, o.EffectiveDate, o.LinkedSalaryAssignmentId))
                .ToListAsync(cancellationToken))
            .GroupBy(x => x.WorkforceProfileId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Tunjangan tetap per Struktur Gaji (PAY-BE-17, PRL-DEC-02): dimuat sekali untuk seluruh struktur penetapan di run ini.
        var structureIds = assignments.Values.SelectMany(x => x).Select(x => x.SalaryStructureId).Distinct().ToList();
        var structureAllowances = (await _dbContext.MstSalaryStructureAllowances.AsNoTracking()
                .Include(x => x.AllowanceType)
                .Where(x => !x.IsDelete && x.IsActive && structureIds.Contains(x.SalaryStructureId)
                    && x.AllowanceType != null && !x.AllowanceType.IsDelete)
                .ToListAsync(cancellationToken))
            .GroupBy(x => x.SalaryStructureId)
            .ToDictionary(g => g.Key, g => g.OrderBy(l => l.AllowanceType!.AllowanceTypeName).ThenBy(l => l.Id).ToList());

        // Tunjangan transportasi pegawai yang berlaku di periode (PAY-BE-18, PRL-DEC-46), beserta kebijakannya.
        var transportAllowances = (await _dbContext.PrlTransportAllowances.AsNoTracking()
                .Include(x => x.TransportAllowancePolicy)
                .Where(x => !x.IsDelete && x.IsActive && workforceIds.Contains(x.WorkforceProfileId)
                    && x.AllowanceStatus == "Active"
                    && (x.EffectiveStartDate == null || x.EffectiveStartDate <= periodEnd)
                    && (x.EffectiveEndDate == null || x.EffectiveEndDate >= periodStart))
                .ToListAsync(cancellationToken))
            .GroupBy(x => x.WorkforceProfileId)
            .ToDictionary(g => g.Key, g => g.OrderBy(t => t.EffectiveStartDate).ThenBy(t => t.Id).ToList());

        // Tunjangan khusus pegawai (Tunjangan Pegawai): hanya penetapan Approved yang aktif dan berlaku di periode. Penetapan yang belum
        // disetujui (Draft/PendingApproval), ditolak, atau dinonaktifkan tidak pernah dibayarkan. Jenis tunjangannya ikut dimuat untuk
        // memastikan masih aktif.
        var workforceAllowances = (await _dbContext.PrlWorkforceAllowances.AsNoTracking()
                .Include(x => x.AllowanceType)
                .Where(x => !x.IsDelete && x.IsActive && x.AllowanceStatus == WorkforceAllowanceStatuses.Approved
                    && workforceIds.Contains(x.WorkforceProfileId)
                    && x.EffectiveStartDate <= periodEnd
                    && (x.EffectiveEndDate == null || x.EffectiveEndDate >= periodStart))
                .ToListAsync(cancellationToken))
            .GroupBy(x => x.WorkforceProfileId)
            .ToDictionary(g => g.Key, g => g.OrderBy(a => a.EffectiveStartDate).ThenBy(a => a.Id).ToList());

        var bankAccounts = (await _dbContext.WfpBankAccounts.AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && workforceIds.Contains(x.WorkforceProfileId))
                .ToListAsync(cancellationToken))
            .GroupBy(x => x.WorkforceProfileId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var taxProfiles = (await _dbContext.PrlTaxes.AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && workforceIds.Contains(x.WorkforceProfileId))
                .ToListAsync(cancellationToken))
            .GroupBy(x => x.WorkforceProfileId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(t => t.EffectiveStartDate).First());

        var insuranceProfiles = (await _dbContext.PrlInsurances.AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && workforceIds.Contains(x.WorkforceProfileId))
                .ToListAsync(cancellationToken))
            .GroupBy(x => x.WorkforceProfileId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(t => t.EffectiveStartDate).First());

        // Kebijakan regulasi berversi yang disahkan dan berlaku pada akhir periode: tarif PPh. Nilai bawaan Pengaturan Penggajian tidak
        // pernah dipakai; tanpa kebijakan yang disahkan hasilnya penahan (Menunggu Verifikasi Kebijakan).
        // Versi yang dipakai disimpan pada snapshot setiap penerima sehingga perubahan kebijakan tidak mengubah payroll historis.
        var statutory = new StatutoryContext(
            await _policyResolver.GetTaxPolicyAsync(periodEnd, cancellationToken),
            await LoadPriorTaxRowsAsync(run.Id, workforceIds, periodStart, periodEnd, cancellationToken),
            await _openingBalanceSource.LoadApprovedAsync(workforceIds, periodEnd.Year, cancellationToken),
            _openingBalanceSource.IsAvailable);

        var identities = new Dictionary<Guid, IdentityInfo>();
        var employeeIdentities = await _dbContext.MstEmployees.AsNoTracking()
            .Where(x => !x.IsDelete && workforceIds.Contains(x.WorkforceProfileId))
            .Select(x => new IdentityInfo(x.WorkforceProfileId, x.IsActive, x.JoinDate, x.ResignDate))
            .ToListAsync(cancellationToken);
        foreach (var item in employeeIdentities) identities[item.WorkforceProfileId] = item;
        var doctorIdentities = await _dbContext.MstDoctors.AsNoTracking()
            .Where(x => !x.IsDelete && workforceIds.Contains(x.WorkforceProfileId))
            .Select(x => new IdentityInfo(x.WorkforceProfileId, true, x.JoinDate, x.ResignDate))
            .ToListAsync(cancellationToken);
        foreach (var item in doctorIdentities) identities.TryAdd(item.WorkforceProfileId, item);

        foreach (var employee in employees.OrderBy(x => x.EmployeeNameSnapshot).ThenBy(x => x.Id))
        {
            var errors = new List<string>();
            var warnings = new List<string>();
            var blockers = new List<string>();

            profiles.TryGetValue(employee.PayrollProfileId ?? Guid.Empty, out var profile);
            identities.TryGetValue(employee.WorkforceProfileId, out var identity);
            assignments.TryGetValue(employee.WorkforceProfileId, out var assignmentList);
            bankAccounts.TryGetValue(employee.WorkforceProfileId, out var bankList);
            taxProfiles.TryGetValue(employee.WorkforceProfileId, out var taxProfile);
            insuranceProfiles.TryGetValue(employee.WorkforceProfileId, out var insuranceProfile);
            transportAllowances.TryGetValue(employee.WorkforceProfileId, out var transportList);
            workforceAllowances.TryGetValue(employee.WorkforceProfileId, out var workforceAllowanceList);
            pendingAssignments.TryGetValue(employee.WorkforceProfileId, out var pendingAssignmentList);
            compensationActions.TryGetValue(employee.WorkforceProfileId, out var compensationActionList);

            CalculateEmployee(employee, profile, identity, assignmentList ?? new List<WfpSalaryAssignment>(),
                pendingAssignmentList ?? new List<WfpSalaryAssignment>(),
                compensationActionList ?? new List<CompensationActionHint>(),
                bankList ?? new List<WfpBankAccount>(), taxProfile, insuranceProfile, structureAllowances,
                transportList ?? new List<PrlTransportAllowance>(),
                workforceAllowanceList ?? new List<PrlWorkforceAllowance>(),
                statutory, periodStart, periodEnd, totalDays, actorUserId, errors, warnings, blockers);

            var label = employee.EmployeeNameSnapshot;

            // Semua Issues snapshot pajak ditambahkan apa adanya ke daftar penahan penerima (taxAssessment.IsBlocking), jadi jumlahnya sama.
            var classifiedTax = ReadSnapshot<PayrollTaxSnapshot>(employee.TaxSnapshotJson);
            if (classifiedTax is { IsBlocking: true, BlockCode: PayrollTaxBlockCodes.YtdHistoryIncomplete })
                result.TaxHistoryBlockerCount += classifiedTax.Issues.Count;

            foreach (var warning in warnings) result.Warnings.Add($"{label}: {warning}");
            if (warnings.Count > 0) result.EmployeeWarnings[employee.Id] = warnings;
            if (errors.Count > 0) result.EmployeeErrors[employee.Id] = errors;

            // Konfigurasi PPh/iuran yang belum lengkap atau tidak sah menahan pengajuan run (penerima tetap terhitung dan ditandai).
            // Bila penerima sudah gagal karena sebab lain, penahan konfigurasi tetap dilaporkan supaya HRD melihat semuanya sekaligus.
            foreach (var blocker in blockers) result.Blockers.Add($"{label}: {blocker}");
            if (blockers.Count > 0) result.EmployeeBlockers[employee.Id] = blockers;

            if (errors.Count > 0)
            {
                employee.EmployeePayrollStatus = EmployeeError;
                employee.ValidationResultJson = string.Join(" ", errors);
                // Hasil gagal dicatat apa adanya (alasan dan peringatan sebagai daftar); angka tidak sah dan tidak pernah dianggap hasil.
                employee.CalculationResultJson = JsonSerializer.Serialize(new
                {
                    engine = EngineVersion,
                    policyVersion = _policy.PolicyVersion,
                    calculatedAt = DateTime.UtcNow,
                    failed = true,
                    errors = errors.ToList(),
                    warnings = warnings.ToList()
                });
                employee.UpdateDateTime = DateTime.UtcNow;
                employee.UpdateBy = actorUserId;
                foreach (var error in errors) result.Blockers.Add($"{label}: {error}");
                result.ErrorCount++;
                continue;
            }

            employee.EmployeePayrollStatus = EmployeeCalculated;
            employee.ValidationResultJson = null;
            result.CalculatedCount++;
        }

        return result;
    }

    private void CalculateEmployee(
        PrlPayrollRunEmployee employee,
        PrlPayroll? profile,
        IdentityInfo? identity,
        List<WfpSalaryAssignment> assignmentList,
        List<WfpSalaryAssignment> pendingAssignmentList,
        List<CompensationActionHint> compensationActions,
        List<WfpBankAccount> bankList,
        PrlTax? taxProfile,
        PrlInsurance? insuranceProfile,
        IReadOnlyDictionary<Guid, List<MstSalaryStructureAllowance>> structureAllowances,
        List<PrlTransportAllowance> transportAllowances,
        List<PrlWorkforceAllowance> workforceAllowances,
        StatutoryContext statutory,
        DateTime periodStart,
        DateTime periodEnd,
        int totalDays,
        Guid actorUserId,
        List<string> errors,
        List<string> warnings,
        List<string> blockers)
    {
        // Snapshot PPh/iuran hasil hitung sebelumnya tidak boleh bertahan bila penerima ini gagal sebelum sampai ke penilaian pajak.
        employee.TaxSnapshotJson = null;
        employee.InsuranceSnapshotJson = null;

        if (profile is null || !profile.IsPayrollEligible || !string.Equals(profile.PayrollStatus, "Active", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("Profil payroll tidak ditemukan, tidak eligible, atau tidak berstatus Active.");
            return;
        }

        if (identity is not null && !identity.IsActive && !(identity.ResignDate.HasValue && identity.ResignDate.Value.Date >= periodStart))
        {
            errors.Add("Pegawai tidak aktif pada periode ini.");
            return;
        }

        // Jendela hari dibayarkan = irisan periode, masa kerja (bergabung/berhenti), dan masa berlaku profil payroll.
        var windowStart = MaxDate(MaxDate(periodStart, identity?.JoinDate), profile.EffectiveStartDate);
        var windowEnd = MinDate(MinDate(periodEnd, identity?.ResignDate), profile.EffectiveEndDate);
        var payableDays = windowEnd >= windowStart ? (windowEnd - windowStart).Days + 1 : 0;
        if (payableDays <= 0)
        {
            errors.Add("Tidak ada hari dibayarkan pada periode ini (belum efektif atau sudah berhenti sebelum periode).");
            return;
        }

        // ---- Gaji pokok dari penetapan gaji utama dan efektif (prorata per segmen) ----
        var activeAssignments = assignmentList.Where(a => a.IsPrimary).OrderBy(a => a.EffectiveStartDate).ToList();
        var overlap = false;
        for (var i = 0; i < activeAssignments.Count && !overlap; i++)
            for (var j = i + 1; j < activeAssignments.Count; j++)
                if (RangesOverlap(activeAssignments[i], activeAssignments[j])) { overlap = true; break; }
        if (overlap)
            errors.Add("Terdapat lebih dari satu penetapan gaji utama yang berlaku bersamaan. Perbaiki di Penetapan Gaji.");

        // Penetapan gaji utama yang aktif tetapi belum disetujui tidak pernah dibayarkan. Bila penerima masih punya penetapan sah, penetapan
        // yang menunggu itu menjadi penahan (run tidak boleh diajukan/disahkan sampai HRD memutuskan); bila tidak, ia sebab gagal di bawah.
        var pendingPrimary = pendingAssignmentList.Where(a => a.IsPrimary).OrderBy(a => a.EffectiveStartDate).ToList();
        if (pendingPrimary.Count > 0 && activeAssignments.Count > 0)
        {
            var pendingStart = pendingPrimary[0].EffectiveStartDate.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            blockers.Add(
                $"Ada penetapan gaji utama yang belum disetujui HRD (mulai {pendingStart}) dan tidak dipakai menghitung. "
                + "Setujui atau nonaktifkan di Penetapan Gaji, lalu hitung ulang.");
        }

        decimal baseAmount = 0m;
        WfpSalaryAssignment? coveringAssignment = null;
        var segments = new List<object>();
        // Segmen penetapan utama beserta jumlah harinya; dipakai juga untuk tunjangan dari struktur (PAY-BE-17).
        var salarySegments = new List<(WfpSalaryAssignment Assignment, int Days)>();
        if (!overlap)
        {
            foreach (var assignment in activeAssignments)
            {
                var segmentStart = MaxDate(windowStart, assignment.EffectiveStartDate);
                var segmentEnd = MinDate(windowEnd, assignment.EffectiveEndDate);
                if (segmentEnd < segmentStart) continue;
                var days = (segmentEnd - segmentStart).Days + 1;
                baseAmount += assignment.BaseSalary * days / totalDays;
                coveringAssignment = assignment;
                salarySegments.Add((assignment, days));
                segments.Add(new { assignmentId = assignment.Id, baseSalary = assignment.BaseSalary, startDate = segmentStart, endDate = segmentEnd, days });
            }

            if (segments.Count == 0)
            {
                if (_policy.AllowProfileSalaryFallback && profile.BaseSalary > 0m)
                {
                    baseAmount = profile.BaseSalary * payableDays / totalDays;
                    warnings.Add("Gaji pokok memakai kolom kompatibilitas profil payroll karena belum ada penetapan gaji yang aktif.");
                    segments.Add(new { assignmentId = (Guid?)null, baseSalary = profile.BaseSalary, startDate = windowStart, endDate = windowEnd, days = payableDays, source = "PrlPayroll" });
                }
                else
                {
                    errors.Add(pendingPrimary.Count > 0
                        ? "Penetapan gaji utama pada periode ini belum disetujui HRD, jadi gaji belum dapat dihitung. Setujui di Penetapan Gaji."
                        : "Belum ada penetapan gaji utama yang berlaku pada periode ini.");
                }
            }
            else
            {
                // Penetapan utama yang disetujui harus menutup seluruh hari dibayarkan. Hari yang tidak tertutup tidak dihitung (angka tidak
                // berubah); aturan prorata dan cut-off perubahan kompensasi belum disahkan, jadi ini peringatan yang dapat ditindaklanjuti.
                var coveredDays = salarySegments.Sum(s => s.Days);
                if (coveredDays < payableDays)
                {
                    var nonPrimaryApproved = assignmentList.Any(a => !a.IsPrimary);
                    warnings.Add(
                        $"Penetapan gaji utama yang disetujui hanya mencakup {coveredDays} dari {payableDays} hari dibayarkan; {payableDays - coveredDays} hari tidak dihitung."
                        + (nonPrimaryApproved
                            ? " Ada penetapan non-utama yang disetujui pada periode ini (mis. penetapan sebelumnya yang diturunkan dari Utama); periksa Penetapan Gaji."
                            : " Periksa tanggal efektif di Penetapan Gaji."));
                }
            }
        }

        // SK Promosi/Demosi di periode ini harus diikuti penetapan gaji baru (disetujui atau sedang menunggu). Hanya peringatan: wajib atau
        // tidaknya penetapan baru untuk tiap jenis SK adalah keputusan HRD yang belum disahkan.
        foreach (var action in compensationActions)
        {
            var resolved = assignmentList.Concat(pendingAssignmentList).Any(a =>
                a.PersonnelActionOrderId == action.Id
                || (action.LinkedSalaryAssignmentId.HasValue && a.Id == action.LinkedSalaryAssignmentId.Value)
                || a.EffectiveStartDate.Date >= action.EffectiveDate.Date);
            if (!resolved)
            {
                var actionDate = action.EffectiveDate.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                warnings.Add(
                    $"SK {action.Category} {action.OrderNumber} berlaku {actionDate} tetapi belum ada penetapan gaji baru yang menyertainya. "
                    + "Tinjau apakah gaji dan tunjangan perlu ditetapkan ulang di Penetapan Gaji.");
            }
        }
        baseAmount = Round(baseAmount);

        if (errors.Count > 0) return;

        var factor = (decimal)payableDays / totalDays;
        var isProrated = payableDays < totalDays;

        // ---- Tunjangan tetap: dari Struktur Gaji penetapan (PAY-BE-17, PRL-DEC-02); cadangan profil selama peralihan (PRL-DEC-04) ----
        var structureAllowanceLines = BuildStructureAllowanceLines(salarySegments, coveringAssignment, structureAllowances, totalDays);
        var useProfileAllowanceFallback = false;
        decimal fixedAllowance;
        if (structureAllowanceLines.Count > 0)
        {
            fixedAllowance = structureAllowanceLines.Sum(l => l.Amount);
            if (profile.TotalAllowance > 0m)
                warnings.Add("Angka tunjangan di profil diabaikan karena Struktur Gaji sudah memuat tunjangan.");
        }
        else if (_policy.AllowProfileAllowanceFallback && profile.TotalAllowance > 0m)
        {
            fixedAllowance = Round(profile.TotalAllowance * (_policy.ProrateFixedAllowances ? factor : 1m));
            useProfileAllowanceFallback = true;
            warnings.Add("Tunjangan tetap memakai angka profil karena Struktur Gaji belum memuat tunjangan.");
        }
        else
        {
            fixedAllowance = 0m;
        }

        // ---- Tunjangan transportasi bulanan tetap (PAY-BE-18, PRL-DEC-46) ----
        var transportLines = BuildTransportAllowanceLines(transportAllowances, windowStart, windowEnd, totalDays, errors, warnings);
        if (errors.Count > 0) return;
        var transportAmount = transportLines.Sum(l => l.Amount);

        // ---- Tunjangan khusus pegawai (Tunjangan Pegawai): penetapan Approved yang berlaku di periode. Jenis yang sudah dihitung dari
        // Struktur Gaji dilewati dengan peringatan sehingga tidak pernah dihitung dua kali. ----
        var structureTypeIds = structureAllowanceLines.Select(l => l.AllowanceTypeId).ToHashSet();
        var workforceLines = BuildWorkforceAllowanceLines(
            workforceAllowances, salarySegments, coveringAssignment, structureTypeIds, employee.CurrencyCode,
            windowStart, windowEnd, totalDays, errors, warnings);
        if (errors.Count > 0) return;

        // Pagar pembayaran ganda (PAY-BE-FIX-022): Transportasi dibayar dari SATU sumber. Bila pegawai punya tunjangan transport bulanan
        // di jalur lama (PrlTransportAllowance) sekaligus penetapan Transportasi berbasis kebijakan umum, hitungan pegawai ini digagalkan
        // dengan pesan yang jelas; tidak ada yang dipilih diam-diam dan tidak ada yang dibayar dua kali.
        if (transportLines.Count > 0 && workforceLines.Any(l => l.IsTransportFromPolicy))
        {
            errors.Add("Pegawai punya tunjangan transportasi dari dua sumber (Kelola Transportasi lama dan Tunjangan Pegawai berbasis kebijakan) pada periode yang sama. Nonaktifkan salah satunya.");
            return;
        }
        var workforceAmount = workforceLines.Sum(l => l.Amount);

        // ---- Kehadiran dan lembur (snapshot HRD) ----
        var attendance = employee.AttendanceInputs.Where(a => !a.IsDelete && a.IsActive).ToList();
        var overtime = employee.OvertimeInputs.Where(o => !o.IsDelete && o.IsActive).ToList();
        var overtimeAmount = Round(overtime.Sum(o => o.OvertimeAmount));
        var attendanceAllowance = Round(attendance.Sum(a => a.AttendanceAllowanceAmount));
        var attendanceDeduction = Round(attendance.Sum(a => a.AttendanceDeductionAmount));
        var unpaidDays = attendance.Sum(a => a.UnpaidLeaveDays);
        var absentDays = attendance.Sum(a => a.AbsentDays);

        employee.ScheduledWorkMinutes = attendance.Sum(a => a.ScheduledWorkMinutes);
        employee.ActualWorkMinutes = attendance.Sum(a => a.ActualWorkMinutes);
        employee.LateMinutes = attendance.Sum(a => a.LateMinutes);
        employee.EarlyLeaveMinutes = attendance.Sum(a => a.EarlyLeaveMinutes);
        employee.OvertimeMinutes = overtime.Count > 0 ? overtime.Sum(o => o.VerifiedMinutes) : attendance.Sum(a => a.OvertimeMinutes);
        employee.PaidLeaveDays = attendance.Sum(a => a.PaidLeaveDays);
        employee.UnpaidLeaveDays = unpaidDays;
        employee.AbsentDays = absentDays;

        decimal unpaidLeaveDeduction = 0m;
        if (_policy.UnpaidLeaveDeductionEnabled && attendanceDeduction == 0m && (unpaidDays + absentDays) > 0m)
        {
            unpaidLeaveDeduction = Round(Math.Min(baseAmount, baseAmount / payableDays * (unpaidDays + absentDays)));
        }

        // ---- Input variabel (insentif, jasa medis, denda, dst.) ----
        var variableEarningLines = new List<(PrlPayrollVariableInput Input, decimal Amount, bool Taxable)>();
        var variableDeductionLines = new List<(PrlPayrollVariableInput Input, decimal Amount)>();
        var draftInputs = 0;
        var pendingManualInputs = 0;
        foreach (var input in employee.VariableInputs.Where(v => !v.IsDelete && v.IsActive
                     && !string.Equals(v.SourceType, PayrollVariableInputSources.FinanceReceivableInstallment, StringComparison.Ordinal)))
        {
            if (string.Equals(input.InputStatus, "Draft", StringComparison.OrdinalIgnoreCase)) { draftInputs++; continue; }
            if (string.Equals(input.SourceType, PayrollVariableInputSources.ManualInput, StringComparison.Ordinal))
            {
                // Input manual (insentif, bonus, honor) dihitung hanya bila sudah diputuskan: Approved atau Applied
                // (PAY-BE-20, PRL-DEC-07). Sumber otomatis lain tetap memakai CountableInputStatuses seperti sebelumnya.
                if (string.Equals(input.InputStatus, PayrollManualInputStatuses.Submitted, StringComparison.OrdinalIgnoreCase)) pendingManualInputs++;
                if (!ManualCountableInputStatuses.Contains(input.InputStatus)) continue;
            }
            else if (!CountableInputStatuses.Contains(input.InputStatus)) continue;

            var componentType = input.PayrollComponent?.ComponentType;
            if (string.Equals(componentType, TypeDeduction, StringComparison.OrdinalIgnoreCase))
                variableDeductionLines.Add((input, Round(input.Amount)));
            else if (string.Equals(componentType, TypeEarning, StringComparison.OrdinalIgnoreCase)
                     || string.Equals(componentType, "Benefit", StringComparison.OrdinalIgnoreCase))
                variableEarningLines.Add((input, Round(input.Amount), input.PayrollComponent?.IsTaxable ?? true));
            else
                warnings.Add($"Input variabel {input.InputNumber} bertipe komponen '{componentType}' tidak dikenali dan tidak dihitung.");
        }
        if (draftInputs > 0) warnings.Add($"{draftInputs} input variabel berstatus Draft belum diserahkan dan tidak ikut dihitung.");
        if (pendingManualInputs > 0)
            warnings.Add($"{pendingManualInputs} penghasilan variabel manual masih menunggu keputusan dan tidak ikut dihitung.");

        // ---- PPh dan asuransi swasta: konfigurasi HRD (PrlTax, PrlInsurance) adalah sumber, dinilai pada periode run oleh
        // PayrollTaxInsuranceRules. Nol bukan penentu: NOT_CONFIGURED (belum ada keputusan HRD) menahan pengajuan run, sedangkan
        // VALID_ZERO/EXEMPT/NOT_APPLICABLE adalah nol yang sah. Skema otomatis PPh dihitung dari kebijakan tarif berversi yang disahkan;
        // tanpa kebijakan atau tanpa formula yang sah hasilnya penahan, bukan Rp0 dan bukan formula kategori lain. PrlPayroll.TaxAmount
        // hanya cadangan bertanda bila HRD belum mengonfigurasi. ----
        // Asuransi swasta (opsional): dinilai dari profil asuransi HRD yang berlaku pada periode. Tidak ada iuran BPJS, kebijakan tarif BPJS,
        // atau dasar upah iuran, dan konfigurasi asuransi tidak pernah menahan pengajuan run (PAY-BE-FIX-007).
        var insuranceAssessment = PayrollTaxInsuranceRules.AssessInsurance(
            insuranceProfile is null ? null : PayrollInsuranceConfigFacts.From(insuranceProfile), periodStart, periodEnd);

        // PPh: bruto kena pajak dari komponen IsTaxable dikurangi penghasilan yang tidak dibayarkan (cuti tanpa gaji, mangkir, potongan
        // kehadiran), ditambah premi pemberi kerja dan dikurangi iuran pensiun menurut kebijakan PPh, dengan akumulasi tahun berjalan dari
        // snapshot payroll run final sebelumnya. Komponen jalur lama bertipe Deduction/Contribution sehingga tidak ikut sebagai penghasilan.
        var taxPolicy = statutory.TaxPolicy;
        var manualTaxableEarning = employee.Components
            .Where(c => !c.IsDelete
                && !c.ComponentCodeSnapshot.StartsWith(PayrollEngineComponentCodes.Prefix, StringComparison.Ordinal)
                && !LegacyEarningComponentCodes.All.Contains(c.ComponentCodeSnapshot)
                && string.Equals(c.ComponentTypeSnapshot, TypeEarning, StringComparison.OrdinalIgnoreCase)
                && c.IsTaxable)
            .Sum(c => c.Amount);
        var taxableGross = Math.Max(0m, Round(
            baseAmount
            + structureAllowanceLines.Where(l => l.Taxable).Sum(l => l.Amount)
            + (useProfileAllowanceFallback ? fixedAllowance : 0m)
            + transportLines.Where(l => l.Taxable).Sum(l => l.Amount)
            + workforceLines.Where(l => l.Taxable).Sum(l => l.Amount)
            + overtimeAmount + attendanceAllowance
            + variableEarningLines.Where(l => l.Taxable).Sum(l => l.Amount)
            + manualTaxableEarning
            - unpaidLeaveDeduction - attendanceDeduction));

        decimal employerPremium = 0m;
        decimal employeePension = 0m;
        if (taxPolicy is not null)
        {
            foreach (var insuranceLine in insuranceAssessment.Lines)
            {
                if (!insuranceLine.EmployeePaid && taxPolicy.EmployerPremiumPrograms.Contains(insuranceLine.Program)) employerPremium += insuranceLine.Amount;
                if (insuranceLine.EmployeePaid && taxPolicy.PensionDeductionPrograms.Contains(insuranceLine.Program)) employeePension += insuranceLine.Amount;
            }
        }

        var taxYear = periodEnd.Year;
        var taxMonth = periodEnd.Month;
        var isLastTaxPeriod = taxMonth == 12
            || (identity?.ResignDate is DateTime resignDate && resignDate.Date >= periodStart.Date && resignDate.Date <= periodEnd.Date);
        var taxCategory = PayrollTaxInsuranceRules.NormalizeCode(taxProfile?.RecipientTaxCategory);
        statutory.PriorRows.TryGetValue(employee.WorkforceProfileId, out var priorRows);
        statutory.OpeningBalances.TryGetValue(employee.WorkforceProfileId, out var openingRows);
        var ytd = BuildYtd(
            priorRows ?? new List<PriorTaxRow>(), openingRows ?? Array.Empty<PayrollOpeningTaxBalanceFacts>(), statutory.OpeningBalanceAvailable,
            taxPolicy, taxCategory, identity?.JoinDate, taxYear, taxMonth, isLastTaxPeriod);

        var taxAssessment = PayrollTaxInsuranceRules.AssessTax(
            taxProfile is null ? null : PayrollTaxConfigFacts.From(taxProfile), profile.TaxAmount, periodStart, periodEnd,
            new PayrollTaxRunContext(taxPolicy, taxYear, taxMonth, isLastTaxPeriod, taxableGross, employerPremium, employeePension, ytd));
        // Keterbatasan dasar PPh (tanpa iuran BPJS) dicatat pada rincian hitung otomatis supaya tidak ada yang menganggap hasilnya lengkap.
        if (taxAssessment.CalculationMode == TaxCalculationModes.AutoRegulatory && taxAssessment.Detail is not null)
        {
            var detailWithLimitations = new Dictionary<string, object?>(taxAssessment.Detail) { ["limitations"] = new[] { BpjsTaxBasisLimitation } };
            taxAssessment = taxAssessment with { Detail = detailWithLimitations };
        }

        // Dasar hitung dicatat pada SEMUA hasil (otomatis, manual, pengecualian): ia menjadi sumber akumulasi masa pajak berikutnya.
        taxAssessment = taxAssessment with
        {
            TaxYear = taxYear,
            TaxMonth = taxMonth,
            IsLastTaxPeriod = isLastTaxPeriod,
            TaxableGross = taxableGross,
            EmployerPremium = employerPremium,
            EmployeePension = employeePension
        };

        // Alasan yang tidak menahan (mis. cadangan tetapan lama dipakai) tetap terlihat sebagai peringatan.
        if (taxAssessment.IsBlocking) blockers.AddRange(taxAssessment.Issues);
        else warnings.AddRange(taxAssessment.Issues);
        warnings.AddRange(taxAssessment.Warnings);
        warnings.AddRange(insuranceAssessment.Warnings);

        var taxAmount = taxAssessment.EmployeeAmount;
        var insuranceAmount = insuranceAssessment.EmployeeTotal;
        var employerInsuranceAmount = insuranceAssessment.EmployerTotal;

        // Komponen penghasilan dari jalur perhitungan lama (GP01, TJ01, TF01, TR01, OT01, OT02, SH01) dibuang: mesin ini menghitung gaji
        // pokok, tunjangan, dan lembur sendiri, sehingga komponen lama akan terhitung sebagai penghasilan manual dan menggandakannya.
        var legacyEarnings = employee.Components
            .Where(c => !c.IsDelete && LegacyEarningComponentCodes.All.Contains(c.ComponentCodeSnapshot))
            .ToList();
        foreach (var legacy in legacyEarnings)
        {
            legacy.IsDelete = true;
            legacy.DeleteDateTime = DateTime.UtcNow;
            legacy.DeleteBy = actorUserId;
        }
        if (legacyEarnings.Count > 0)
            warnings.Add("Komponen penghasilan dari jalur perhitungan lama dibuang; penghasilan dihitung hanya oleh mesin Payroll Run.");

        // Komponen pajak/iuran dari jalur perhitungan lama (modul "Modern") dibuang agar tidak terpotong dua kali; mesin ini satu-satunya.
        var legacyTaxInsurance = employee.Components
            .Where(c => !c.IsDelete && LegacyTaxInsuranceComponentCodes.All.Contains(c.ComponentCodeSnapshot))
            .ToList();
        foreach (var legacy in legacyTaxInsurance)
        {
            legacy.IsDelete = true;
            legacy.DeleteDateTime = DateTime.UtcNow;
            legacy.DeleteBy = actorUserId;
        }
        if (legacyTaxInsurance.Count > 0)
            warnings.Add("Komponen pajak/iuran dari jalur perhitungan lama dibuang; pajak dan iuran dihitung hanya oleh mesin Payroll Run.");

        // ---- Bangun/upsert komponen milik mesin ----
        var produced = new HashSet<(string Code, Guid? SourceId)>();
        var sort = 10;
        void Add(string code, string name, string type, string method, string sourceType, Guid? sourceId, decimal amount,
            bool taxable, bool prorated, decimal baseValue, decimal quantity, string? formula, object? detail)
        {
            if (amount == 0m && code != PayrollEngineComponentCodes.BaseSalary) return;
            UpsertComponent(employee, actorUserId, code, name, type, method, sourceType, sourceId, amount, taxable, prorated,
                baseValue, quantity, formula, detail is null ? null : JsonSerializer.Serialize(detail), sort);
            produced.Add((code, sourceId));
            sort += 10;
        }

        Add(PayrollEngineComponentCodes.BaseSalary, "Gaji Pokok", TypeEarning, isProrated ? "Prorated" : "Fixed",
            "SalaryAssignment", coveringAssignment?.Id, baseAmount, true, isProrated, baseAmount, 1m,
            "sum(gaji pokok segmen x hari segmen / hari periode)",
            new { totalDays, payableDays, factor = Math.Round(factor, 6), segments });
        foreach (var line in structureAllowanceLines)
        {
            Add(PayrollEngineComponentCodes.StructureAllowance, line.Name, TypeEarning, line.Method,
                PayrollEngineComponentSources.SalaryStructureAllowance, line.SourceId, line.Amount, line.Taxable, line.Prorated,
                line.MonthlyAmount, 1m, line.Formula, line.Detail);
        }
        if (useProfileAllowanceFallback)
        {
            Add(PayrollEngineComponentCodes.FixedAllowance, "Tunjangan Tetap", TypeEarning, isProrated && _policy.ProrateFixedAllowances ? "Prorated" : "Fixed",
                "Master", profile.Id, fixedAllowance, true, isProrated && _policy.ProrateFixedAllowances, profile.TotalAllowance, 1m,
                "tunjangan tetap profil x faktor prorata (cadangan masa peralihan)",
                new { source = "PrlPayroll.TotalAllowance", factor = Math.Round(factor, 6), fallback = true });
        }
        foreach (var line in transportLines)
        {
            Add(PayrollEngineComponentCodes.TransportAllowance, "Tunjangan Transportasi", TypeEarning, line.Prorated ? "Prorated" : "Fixed",
                PayrollEngineComponentSources.TransportAllowance, line.SourceId, line.Amount, line.Taxable, line.Prorated,
                line.MonthlyAmount, 1m, line.Formula, line.Detail);
        }
        foreach (var line in workforceLines)
        {
            Add(PayrollEngineComponentCodes.WorkforceAllowance, line.Name, TypeEarning, line.Prorated ? "Prorated" : "Fixed",
                PayrollEngineComponentSources.WorkforceAllowance, line.SourceId, line.Amount, line.Taxable, line.Prorated,
                line.MonthlyAmount, 1m, line.Formula, line.Detail);
        }
        Add(PayrollEngineComponentCodes.Overtime, "Lembur", TypeEarning, "Computed", "Overtime", null, overtimeAmount, true, false,
            overtimeAmount, overtime.Count, "jumlah nominal lembur terverifikasi HRD", new { items = overtime.Count, minutes = employee.OvertimeMinutes });
        Add(PayrollEngineComponentCodes.AttendanceAllowance, "Tunjangan Kehadiran", TypeEarning, "Computed", "Attendance", null,
            attendanceAllowance, true, false, attendanceAllowance, attendance.Count, "jumlah tunjangan kehadiran snapshot HRD", new { days = attendance.Count });
        foreach (var line in variableEarningLines)
        {
            Add(PayrollEngineComponentCodes.VariableEarning, line.Input.PayrollComponent?.PayrollComponentName ?? "Penghasilan Variabel",
                TypeEarning, "Computed", "VariableInput", line.Input.Id, line.Amount, line.Taxable, false, line.Amount, 1m,
                "nominal input variabel", new { line.Input.InputNumber, line.Input.InputType, line.Input.SourceType, line.Input.SourceId });
        }

        Add(PayrollEngineComponentCodes.AttendanceDeduction, "Potongan Kehadiran", TypeDeduction, "Computed", "Attendance", null,
            attendanceDeduction, false, false, attendanceDeduction, attendance.Count, "jumlah potongan kehadiran snapshot HRD", new { days = attendance.Count });
        Add(PayrollEngineComponentCodes.UnpaidLeave, "Potongan Cuti Tanpa Gaji / Mangkir", TypeDeduction, "Computed", "Attendance", null,
            unpaidLeaveDeduction, false, false, baseAmount, unpaidDays + absentDays,
            "gaji pokok dibayarkan / hari dibayarkan x hari tanpa gaji", new { unpaidDays, absentDays, payableDays, needsConfirmation = true });
        foreach (var line in variableDeductionLines)
        {
            Add(PayrollEngineComponentCodes.VariableDeduction, line.Input.PayrollComponent?.PayrollComponentName ?? "Potongan Variabel",
                TypeDeduction, "Computed", "VariableInput", line.Input.Id, line.Amount, false, false, line.Amount, 1m,
                "nominal input variabel", new { line.Input.InputNumber, line.Input.InputType });
        }

        // Pajak dan iuran memakai tipe "Tax"/"Insurance" (bukan "Deduction") supaya tidak terhitung ganda oleh layanan potongan cicilan,
        // yang menjumlahkan TotalTax + TotalEmployeeInsuranceContribution secara terpisah dari komponen bertipe Deduction.
        // PPh: hanya bagian yang menjadi beban pegawai (metode Gross). PPh yang ditanggung pemberi kerja (Net) tidak menjadi komponen
        // potongan dan tidak mengurangi gaji bersih; nominalnya ada pada snapshot pajak.
        // Label pajak pada slip diturunkan dari jenis, skema, cara hitung, dan versi kebijakan yang benar-benar dipakai.
        var taxName = PayrollTaxLabels.Build(
            taxAssessment.Article, taxAssessment.CalculationMode, taxAssessment.Source, taxAssessment.PolicyVersion, taxAssessment.Detail);
        var taxSourceLabel = taxAssessment.Source == PayrollTaxInsuranceSources.LegacyProfileAmount
            ? "tetapan lama profil payroll (cadangan; konfigurasi HRD belum lengkap)"
            : taxAssessment.CalculationMode == TaxCalculationModes.AutoRegulatory
                ? $"dihitung otomatis dari kebijakan tarif PPh {taxAssessment.PolicyVersion ?? "-"}"
                : "nominal manual disetujui HRD";
        Add(PayrollEngineComponentCodes.Tax, taxName, "Tax",
            taxAssessment.CalculationMode switch
            {
                TaxCalculationModes.ApprovedManualAmount => "ApprovedManualAmount",
                TaxCalculationModes.AutoRegulatory => "AutoRegulatory",
                _ => "Fixed"
            },
            "Tax", taxProfile?.Id, taxAmount, false, false, taxAmount, 1m, taxSourceLabel,
            new
            {
                source = taxAssessment.Source,
                taxAssessment.ResultStatus,
                taxAssessment.Article,
                taxAssessment.RecipientCategory,
                taxAssessment.CalculationMode,
                taxAssessment.TaxMethod,
                taxAssessment.PolicyVersion,
                taxAssessment.PolicyId,
                taxAssessment.Treatment,
                taxAssessment.TaxableGross
            });

        // Asuransi swasta: satu komponen per sisi pembayar. Sisi pegawai bertipe Insurance (mengurangi gaji bersih); sisi pemberi kerja
        // bertipe EmployerContribution dengan nominal di EmployerAmount (Amount 0) dan tidak tampil sebagai potongan di slip. Nominalnya
        // tetap (kontribusi pada profil asuransi HRD), bukan persentase dari dasar upah.
        foreach (var line in insuranceAssessment.Lines)
        {
            const string method = "Fixed";
            const string formula = "nominal kontribusi asuransi swasta pada profil asuransi HRD";
            var detail = new
            {
                source = insuranceAssessment.Source,
                line.Program,
                paidBy = line.EmployeePaid ? "Employee" : "Employer",
                line.Rate,
                baseAmount = line.BaseAmount
            };

            if (line.EmployeePaid)
            {
                Add(line.Code, line.Name, PayrollInsuranceComponentCodes.TypeInsurance, method, "Insurance", insuranceProfile?.Id, line.Amount,
                    false, false, line.BaseAmount, 1m, formula, detail);
            }
            else if (line.Amount != 0m)
            {
                UpsertComponent(employee, actorUserId, line.Code, line.Name, PayrollInsuranceComponentCodes.TypeEmployerContribution, method,
                    "Insurance", insuranceProfile?.Id, 0m, false, false, line.BaseAmount, 1m, formula, JsonSerializer.Serialize(detail), sort,
                    employerAmount: line.Amount, displayOnPayslip: false);
                produced.Add((line.Code, insuranceProfile?.Id));
                sort += 10;
            }
        }

        foreach (var stale in employee.Components.Where(c => !c.IsDelete
                     && c.ComponentCodeSnapshot.StartsWith(PayrollEngineComponentCodes.Prefix, StringComparison.Ordinal)
                     && !produced.Contains((c.ComponentCodeSnapshot, c.SourceId))).ToList())
        {
            stale.IsDelete = true;
            stale.DeleteDateTime = DateTime.UtcNow;
            stale.DeleteBy = actorUserId;
        }

        // ---- Total: konsisten dengan rumus PayrollInstallmentDeductionService (semua komponen Deduction non-cicilan dihitung) ----
        // Baris milik mesin dijumlah dari nilai lokal di atas (tidak bergantung pada fixup navigasi EF); baris non-mesin
        // (mis. restitusi, koreksi manual) dibaca dari koleksi dan dihitung SEKALI.
        var nonEngine = employee.Components
            .Where(c => !c.IsDelete && !c.ComponentCodeSnapshot.StartsWith(PayrollEngineComponentCodes.Prefix, StringComparison.Ordinal))
            .ToList();
        var manualEarning = Round(nonEngine
            .Where(c => string.Equals(c.ComponentTypeSnapshot, TypeEarning, StringComparison.OrdinalIgnoreCase))
            .Sum(c => c.Amount));
        var manualDeduction = Round(nonEngine
            .Where(c => string.Equals(c.ComponentTypeSnapshot, TypeDeduction, StringComparison.OrdinalIgnoreCase)
                && c.ComponentCodeSnapshot != InstallmentComponentCode && c.ComponentCodeSnapshot != LegacyLoanComponentCode)
            .Sum(c => c.Amount));
        var variableEarning = Round(variableEarningLines.Sum(l => l.Amount) + manualEarning);
        var otherDeductions = Round(attendanceDeduction + unpaidLeaveDeduction + variableDeductionLines.Sum(l => l.Amount) + manualDeduction);

        employee.BaseSalary = baseAmount;
        employee.TotalRecurringEarning = Round(fixedAllowance + transportAmount + workforceAmount);
        employee.TotalVariableEarning = variableEarning;
        employee.TotalOvertimeAmount = overtimeAmount;
        employee.TotalAttendanceAllowance = attendanceAllowance;
        employee.GrossPay = Round(baseAmount + fixedAllowance + transportAmount + workforceAmount + overtimeAmount + attendanceAllowance + variableEarning);
        employee.TotalTax = taxAmount;
        employee.TotalEmployeeInsuranceContribution = insuranceAmount;
        // Kontribusi pemberi kerja: informasi, disimpan terpisah, dan TIDAK masuk potongan maupun gaji bersih pegawai.
        employee.TotalEmployerInsuranceContribution = employerInsuranceAmount;
        employee.TotalDeduction = Round(taxAmount + insuranceAmount + otherDeductions);
        employee.NetPay = Round(employee.GrossPay - employee.TotalDeduction);

        if (employee.GrossPay <= 0m)
            errors.Add("Gaji bruto nol atau belum dikonfigurasi pada profil payroll.");
        else if (employee.NetPay < 0m)
            errors.Add($"Gaji bersih negatif (Rp {employee.NetPay:N0}): potongan melebihi penghasilan. Periksa potongan dan input variabel.");

        // ---- Snapshot identitas, rekening, pajak, dan hasil hitung (dibekukan saat run disahkan) ----
        var bank = PickPayrollBankAccount(bankList, employee.BankAccountId ?? profile.BankAccountId, periodEnd);
        if (bank is null)
        {
            warnings.Add("Belum ada rekening payroll aktif; instruksi pembayaran akan ditahan sampai rekening terverifikasi.");
        }
        else
        {
            employee.BankAccountId = bank.Id;
            employee.BankNameSnapshot = bank.BankName;
            employee.BankAccountNumberSnapshot = bank.AccountNumber;
            employee.BankAccountHolderSnapshot = bank.AccountHolderName;
            if (!bank.IsVerified) warnings.Add("Rekening payroll belum terverifikasi; instruksi pembayaran akan ditahan sampai terverifikasi.");
        }

        employee.SalaryAssignmentId = coveringAssignment?.Id ?? employee.SalaryAssignmentId;
        employee.TaxProfileId = taxProfile?.Id ?? employee.TaxProfileId;
        employee.InsuranceProfileId = insuranceProfile?.Id ?? employee.InsuranceProfileId;
        employee.TaxStatusSnapshot = taxProfile?.TaxStatus ?? employee.TaxStatusSnapshot;
        employee.NpwpNumberSnapshot = taxProfile?.NpwpNumber ?? employee.NpwpNumberSnapshot;
        employee.PaymentAmount = employee.NetPay;
        employee.CalculationResultJson = JsonSerializer.Serialize(new
        {
            engine = EngineVersion,
            policyVersion = _policy.PolicyVersion,
            calculatedAt = DateTime.UtcNow,
            totalDays,
            payableDays,
            isProrated,
            gross = employee.GrossPay,
            deductionBeforeInstallment = employee.TotalDeduction,
            netBeforeInstallment = employee.NetPay,
            taxResultStatus = taxAssessment.ResultStatus,
            insuranceResultStatus = insuranceAssessment.ResultStatus,
            // Peringatan per penerima disimpan di hasil hitung supaya rincian penerima membacanya tanpa menghitung ulang.
            warnings = warnings.ToList()
        });
        employee.SalarySnapshotJson = JsonSerializer.Serialize(new { segments, coveringAssignmentId = coveringAssignment?.Id });
        // Snapshot PPh dan iuran: konfigurasi HRD yang dipakai, status, perlakuan, dan nominal pada saat hitung. Dibekukan bersama run
        // saat disahkan; perubahan profil HRD sesudahnya tidak mengubahnya (DEC-PRL-TAX-007). Tanpa NPWP atau data pribadi lain.
        employee.TaxSnapshotJson = JsonSerializer.Serialize(taxAssessment.ToSnapshot());
        employee.InsuranceSnapshotJson = JsonSerializer.Serialize(insuranceAssessment.ToSnapshot());
        employee.UpdateDateTime = DateTime.UtcNow;
        employee.UpdateBy = actorUserId;
    }

    private void UpsertComponent(
        PrlPayrollRunEmployee employee, Guid actorUserId, string code, string name, string type, string method, string sourceType,
        Guid? sourceId, decimal amount, bool taxable, bool prorated, decimal baseValue, decimal quantity, string? formula,
        string? detailJson, int sortOrder, decimal employerAmount = 0m, bool displayOnPayslip = true)
    {
        var existing = employee.Components.FirstOrDefault(c => !c.IsDelete && c.ComponentCodeSnapshot == code && c.SourceId == sourceId);
        if (existing is null)
        {
            existing = new PrlPayrollEmployeeComponent
            {
                PayrollRunEmployeeId = employee.Id,
                ComponentCodeSnapshot = code,
                SourceId = sourceId,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _dbContext.PrlPayrollEmployeeComponents.Add(existing);
        }
        else
        {
            existing.UpdateDateTime = DateTime.UtcNow;
            existing.UpdateBy = actorUserId;
        }

        existing.SourceType = sourceType;
        existing.ComponentNameSnapshot = name;
        existing.ComponentTypeSnapshot = type;
        existing.CalculationMethodSnapshot = method;
        existing.CurrencyCode = employee.CurrencyCode;
        existing.Quantity = quantity;
        existing.Rate = quantity > 0m ? Round(amount / quantity) : amount;
        existing.BaseAmount = baseValue;
        existing.Amount = amount;
        existing.EmployerAmount = employerAmount;
        existing.TaxableAmount = taxable ? amount : 0m;
        existing.IsTaxable = taxable;
        existing.IsProrated = prorated;
        existing.IsManual = false;
        existing.IsDisplayedOnPayslip = displayOnPayslip;
        existing.FormulaSnapshot = formula;
        existing.CalculationDetailJson = detailJson;
        existing.SortOrder = sortOrder;
    }

    /// <summary>Satu baris hasil tunjangan struktur sebelum di-upsert sebagai komponen PAY_STRUCTURE_ALLOWANCE.</summary>
    private sealed record StructureAllowanceLine(
        Guid SourceId, Guid AllowanceTypeId, string Name, string Method, decimal Amount, decimal MonthlyAmount, bool Taxable, bool Prorated,
        string Formula, object Detail);

    /// <summary>
    /// Tunjangan tetap dari Struktur Gaji (blueprint payroll-gaji 02-backend-architecture bagian 5.1):
    /// nilai sebulan = nominal, atau gaji pokok penetapan x persen / 100; dipotong MaximumAmount jenis tunjangan;
    /// diprorata per segmen (hari segmen / hari periode) bila jenis tunjangan IsProrated dan kebijakan ProrateFixedAllowances;
    /// bila tidak diprorata, diambil penuh dari struktur penetapan yang menutup akhir periode. Satu baris per baris struktur
    /// (SourceId = MstSalaryStructureAllowance.Id); pada bulan tanpa pergantian struktur itu berarti satu baris per jenis tunjangan.
    /// </summary>
    private List<StructureAllowanceLine> BuildStructureAllowanceLines(
        List<(WfpSalaryAssignment Assignment, int Days)> salarySegments,
        WfpSalaryAssignment? coveringAssignment,
        IReadOnlyDictionary<Guid, List<MstSalaryStructureAllowance>> structureAllowances,
        int totalDays)
    {
        var amounts = new Dictionary<Guid, decimal>();
        var monthlyById = new Dictionary<Guid, decimal>();
        var segmentDetails = new Dictionary<Guid, List<object>>();
        var lineById = new Dictionary<Guid, MstSalaryStructureAllowance>();

        foreach (var (assignment, days) in salarySegments)
        {
            if (!structureAllowances.TryGetValue(assignment.SalaryStructureId, out var lines)) continue;

            foreach (var line in lines)
            {
                var monthly = line.CalculationMethod == SalaryStructureAllowanceCalculationMethods.PercentageOfBaseSalary
                    ? assignment.BaseSalary * line.Percentage / 100m
                    : line.Amount;
                var cap = line.AllowanceType?.MaximumAmount;
                if (cap.HasValue && cap.Value > 0m && monthly > cap.Value) monthly = cap.Value;

                var prorated = (line.AllowanceType?.IsProrated ?? true) && _policy.ProrateFixedAllowances;
                decimal portion;
                if (prorated)
                {
                    portion = monthly * days / totalDays;
                }
                else
                {
                    // Tidak diprorata: hanya struktur penetapan penutup periode, nilai sebulan penuh, dihitung sekali.
                    if (coveringAssignment is null || assignment.Id != coveringAssignment.Id) continue;
                    portion = monthly;
                }

                lineById[line.Id] = line;
                monthlyById[line.Id] = Round(monthly);
                amounts[line.Id] = (amounts.TryGetValue(line.Id, out var running) ? running : 0m) + portion;
                if (!segmentDetails.TryGetValue(line.Id, out var details)) segmentDetails[line.Id] = details = new List<object>();
                details.Add(new { assignmentId = assignment.Id, baseSalary = assignment.BaseSalary, days, monthly = Round(monthly) });
            }
        }

        var result = new List<StructureAllowanceLine>();
        foreach (var (lineId, amount) in amounts)
        {
            var line = lineById[lineId];
            var rounded = Round(amount);
            if (rounded == 0m) continue;
            var prorated = (line.AllowanceType?.IsProrated ?? true) && _policy.ProrateFixedAllowances;
            var isPercentage = line.CalculationMethod == SalaryStructureAllowanceCalculationMethods.PercentageOfBaseSalary;
            result.Add(new StructureAllowanceLine(
                SourceId: line.Id,
                AllowanceTypeId: line.AllowanceTypeId,
                Name: line.AllowanceType?.AllowanceTypeName ?? "Tunjangan Struktur",
                Method: line.CalculationMethod,
                Amount: rounded,
                MonthlyAmount: monthlyById[lineId],
                Taxable: line.AllowanceType?.IsTaxable ?? true,
                Prorated: prorated,
                Formula: (isPercentage ? "gaji pokok penetapan x persen / 100" : "nominal tetap sebulan")
                    + (line.AllowanceType?.MaximumAmount is > 0m ? ", dibatasi maksimum jenis tunjangan" : string.Empty)
                    + (prorated ? ", x hari segmen / hari periode" : ", penuh (tidak diprorata)"),
                Detail: new
                {
                    salaryStructureId = line.SalaryStructureId,
                    allowanceTypeId = line.AllowanceTypeId,
                    calculationMethod = line.CalculationMethod,
                    percentage = isPercentage ? line.Percentage : (decimal?)null,
                    maximumAmount = line.AllowanceType?.MaximumAmount,
                    totalDays,
                    segments = segmentDetails[lineId]
                }));
        }

        return result.OrderBy(l => l.Name, StringComparer.Ordinal).ThenBy(l => l.SourceId).ToList();
    }

    /// <summary>Satu baris hasil tunjangan khusus pegawai sebelum di-upsert sebagai komponen PAY_WORKFORCE_ALLOWANCE.</summary>
    private sealed record WorkforceAllowanceLine(
        Guid SourceId, Guid AllowanceTypeId, string Name, decimal Amount, decimal MonthlyAmount, bool Taxable, bool Prorated,
        string Formula, object Detail, bool IsTransportFromPolicy = false);

    /// <summary>
    /// Tunjangan khusus pegawai (PrlWorkforceAllowance, Approved dan berlaku di periode). Nilai sebulan = nominal tetap, atau gaji pokok
    /// penetapan gaji yang DISETUJUI x persen / 100; dibatasi batas maksimum snapshot penetapan; diprorata hari berlaku di jendela
    /// dibayarkan / hari periode bila IsProrated (snapshot) dan kebijakan ProrateFixedAllowances. Persentase dihitung per segmen
    /// penetapan gaji yang beririsan (gaji pokok bisa berganti di tengah periode); tanpa penetapan gaji yang disetujui, tunjangan
    /// berbasis persentase tidak dihitung (peringatan), bukan memakai angka kompatibilitas profil.
    /// Tanpa prorata, per jenis hanya penetapan yang berakhir paling akhir di jendela yang dibayar penuh (tidak bayar ganda bila dua
    /// penetapan berurutan jatuh dalam satu bulan). Dua penetapan jenis yang sama yang beririsan di periode ini adalah data
    /// tidak konsisten dan menggagalkan pegawai (bukan dibayar dua kali). Jenis yang sudah berasal dari Struktur Gaji, yang tidak aktif
    /// lagi di master, atau yang bermata uang berbeda dilewati dengan peringatan.
    /// </summary>
    private List<WorkforceAllowanceLine> BuildWorkforceAllowanceLines(
        List<PrlWorkforceAllowance> allowances,
        List<(WfpSalaryAssignment Assignment, int Days)> salarySegments,
        WfpSalaryAssignment? coveringAssignment,
        ISet<Guid> structureTypeIds,
        string currencyCode,
        DateTime windowStart,
        DateTime windowEnd,
        int totalDays,
        List<string> errors,
        List<string> warnings)
    {
        var candidates = new List<(PrlWorkforceAllowance Allowance, string Name, DateTime Start, DateTime End)>();
        foreach (var allowance in allowances)
        {
            var type = allowance.AllowanceType;
            var name = type?.AllowanceTypeName ?? "Tunjangan Pegawai";

            if (type is null || type.IsDelete || !type.IsActive)
            {
                warnings.Add($"Tunjangan {name} dilewati karena jenis tunjangannya sudah tidak aktif di Master Tunjangan.");
                continue;
            }
            if (structureTypeIds.Contains(allowance.AllowanceTypeId))
            {
                warnings.Add($"Tunjangan {name} sudah dihitung dari Struktur Gaji, jadi penetapan khusus pegawai tidak dihitung dua kali.");
                continue;
            }
            // Transportasi dari penetapan berbasis Kebijakan Tunjangan umum (PAY-BE-FIX-022) dibayar di sini. Penetapan Transport dari sumber
            // lain tetap dilewati: jalur lamanya (PrlTransportAllowance) yang membayarnya, sehingga tidak terjadi perhitungan ganda.
            if (string.Equals(type.AllowanceCategory, "Transport", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(allowance.AssignmentSource, WorkforceAllowanceAssignmentSources.Policy, StringComparison.Ordinal))
            {
                warnings.Add($"Tunjangan {name} dilewati dari Tunjangan Pegawai karena dikelola melalui Kebijakan Transportasi agar tidak terjadi perhitungan ganda.");
                continue;
            }
            if (!string.Equals(allowance.CurrencyCode, currencyCode, StringComparison.OrdinalIgnoreCase))
            {
                warnings.Add($"Tunjangan {name} dilewati karena mata uangnya ({allowance.CurrencyCode}) berbeda dari mata uang payroll ({currencyCode}).");
                continue;
            }

            var start = MaxDate(windowStart, allowance.EffectiveStartDate);
            var end = MinDate(windowEnd, allowance.EffectiveEndDate);
            if (end < start) continue;
            candidates.Add((allowance, name, start, end));
        }

        foreach (var group in candidates.GroupBy(c => c.Allowance.AllowanceTypeId))
        {
            var ordered = group.OrderBy(c => c.Start).ToList();
            for (var i = 0; i < ordered.Count; i++)
            {
                for (var j = i + 1; j < ordered.Count; j++)
                {
                    if (ordered[i].Start <= ordered[j].End && ordered[j].Start <= ordered[i].End)
                    {
                        errors.Add($"Pegawai punya lebih dari satu penetapan tunjangan {ordered[i].Name} yang berlaku bersamaan. Perbaiki di Tunjangan Pegawai.");
                        return new List<WorkforceAllowanceLine>();
                    }
                }
            }
        }

        bool IsProrated(PrlWorkforceAllowance allowance) => allowance.IsProrated && _policy.ProrateFixedAllowances;
        var lastFullEnd = candidates
            .Where(c => !IsProrated(c.Allowance))
            .GroupBy(c => c.Allowance.AllowanceTypeId)
            .ToDictionary(g => g.Key, g => g.Max(c => c.End));

        static decimal Capped(decimal value, decimal? maximum) =>
            maximum.HasValue && maximum.Value > 0m && value > maximum.Value ? maximum.Value : value;

        var result = new List<WorkforceAllowanceLine>();
        foreach (var (allowance, name, start, end) in candidates)
        {
            var prorated = IsProrated(allowance);
            if (!prorated && end != lastFullEnd[allowance.AllowanceTypeId]) continue;

            var days = (end - start).Days + 1;
            var isPercentage = allowance.CalculationMethod == SalaryStructureAllowanceCalculationMethods.PercentageOfBaseSalary;
            decimal monthlyDisplay;
            decimal amount;

            if (!isPercentage)
            {
                monthlyDisplay = Capped(allowance.Amount, allowance.MaximumAmount);
                amount = prorated ? monthlyDisplay * days / totalDays : monthlyDisplay;
            }
            else if (salarySegments.Count == 0 || coveringAssignment is null)
            {
                warnings.Add($"Tunjangan {name} berbasis persentase dilewati karena belum ada penetapan gaji yang disetujui sebagai dasar perhitungan.");
                continue;
            }
            else if (prorated)
            {
                amount = 0m;
                foreach (var (assignment, _) in salarySegments)
                {
                    var segmentStart = MaxDate(windowStart, assignment.EffectiveStartDate);
                    var segmentEnd = MinDate(windowEnd, assignment.EffectiveEndDate);
                    var overlapStart = start > segmentStart ? start : segmentStart;
                    var overlapEnd = end < segmentEnd ? end : segmentEnd;
                    if (overlapEnd < overlapStart) continue;

                    var overlapDays = (overlapEnd - overlapStart).Days + 1;
                    amount += Capped(assignment.BaseSalary * allowance.Percentage / 100m, allowance.MaximumAmount) * overlapDays / totalDays;
                }
                monthlyDisplay = Capped(coveringAssignment.BaseSalary * allowance.Percentage / 100m, allowance.MaximumAmount);
            }
            else
            {
                monthlyDisplay = Capped(coveringAssignment.BaseSalary * allowance.Percentage / 100m, allowance.MaximumAmount);
                amount = monthlyDisplay;
            }

            var rounded = Round(amount);
            if (rounded == 0m) continue;

            result.Add(new WorkforceAllowanceLine(
                SourceId: allowance.Id,
                AllowanceTypeId: allowance.AllowanceTypeId,
                Name: name,
                Amount: rounded,
                MonthlyAmount: Round(monthlyDisplay),
                Taxable: allowance.IsTaxable,
                Prorated: prorated,
                Formula: (isPercentage ? "gaji pokok penetapan x persen / 100" : "nominal tetap sebulan")
                    + (allowance.MaximumAmount is > 0m ? ", dibatasi maksimum penetapan" : string.Empty)
                    + (prorated ? ", x hari berlaku / hari periode" : ", penuh (tidak diprorata)"),
                Detail: new
                {
                    source = "PrlWorkforceAllowance",
                    // Provenance (PAY-BE-FIX-022): dari mana nilai ini berasal. Kosong untuk penetapan langsung (Direct).
                    assignmentSource = allowance.AssignmentSource,
                    allowancePolicyId = allowance.AllowancePolicyId,
                    allowancePolicyGradeRateId = allowance.AllowancePolicyGradeRateId,
                    salaryGradeId = allowance.SalaryGradeId,
                    allowanceTypeId = allowance.AllowanceTypeId,
                    calculationMethod = allowance.CalculationMethod,
                    percentage = isPercentage ? allowance.Percentage : (decimal?)null,
                    maximumAmount = allowance.MaximumAmount,
                    decidedAt = allowance.DecidedAt,
                    startDate = start,
                    endDate = end,
                    days,
                    totalDays
                },
                IsTransportFromPolicy: string.Equals(allowance.AllowanceType?.AllowanceCategory, "Transport", StringComparison.OrdinalIgnoreCase)));
        }

        return result.OrderBy(l => l.Name, StringComparer.Ordinal).ThenBy(l => l.SourceId).ToList();
    }

    /// <summary>Satu baris hasil tunjangan transport sebelum di-upsert sebagai komponen PAY_TRANSPORT_ALLOWANCE.</summary>
    private sealed record TransportAllowanceLine(
        Guid SourceId, decimal Amount, decimal MonthlyAmount, bool Taxable, bool Prorated, string Formula, object Detail);

    private const string TransportFixedMonthly = "FixedMonthly";

    /// <summary>
    /// Tunjangan transport bulanan tetap (blueprint payroll-gaji 02-backend-architecture bagian 5.3). Hanya tunjangan yang kebijakannya
    /// aktif, IsIncludedInPayroll, dan bermetode FixedMonthly. Nilai sebulan = MonthlyAmount tunjangan pegawai, atau FixedMonthlyAmount
    /// kebijakan bila nol; dibatasi MonthlyLimitAmount kebijakan bila &gt; 0; diprorata hari berlaku di jendela dibayarkan / hari periode
    /// bila kebijakan IsProrated. Tanpa prorata, hanya tunjangan yang terakhir berlaku di jendela yang dibayar penuh (mencegah
    /// bayar ganda bila dua tunjangan berurutan dalam sebulan). Dua tunjangan bulanan yang beririsan menggagalkan pegawai (CALC-01).
    /// Metode lain tidak dihitung dan menghasilkan peringatan (CALC-02).
    /// </summary>
    private static List<TransportAllowanceLine> BuildTransportAllowanceLines(
        List<PrlTransportAllowance> allowances,
        DateTime windowStart,
        DateTime windowEnd,
        int totalDays,
        List<string> errors,
        List<string> warnings)
    {
        var monthly = new List<(PrlTransportAllowance Allowance, PrlTransportAllowancePolicy Policy, DateTime Start, DateTime End)>();
        foreach (var allowance in allowances)
        {
            var policy = allowance.TransportAllowancePolicy;
            if (policy is null || policy.IsDelete || !policy.IsActive)
            {
                warnings.Add("Tunjangan transport tanpa kebijakan aktif belum dihitung otomatis. Masukkan lewat Penghasilan Variabel.");
                continue;
            }
            if (!policy.IsIncludedInPayroll) continue;
            if (!string.Equals(policy.CalculationMethod, TransportFixedMonthly, StringComparison.OrdinalIgnoreCase))
            {
                warnings.Add($"Tunjangan transport metode {policy.CalculationMethod} belum dihitung otomatis. Masukkan lewat Penghasilan Variabel.");
                continue;
            }

            var start = MaxDate(windowStart, allowance.EffectiveStartDate);
            var end = MinDate(windowEnd, allowance.EffectiveEndDate);
            if (end < start) continue;
            monthly.Add((allowance, policy, start, end));
        }

        for (var i = 0; i < monthly.Count; i++)
        {
            for (var j = i + 1; j < monthly.Count; j++)
            {
                if (monthly[i].Start <= monthly[j].End && monthly[j].Start <= monthly[i].End)
                {
                    errors.Add("Pegawai punya lebih dari satu tunjangan transport bulanan yang berlaku. Perbaiki di Tunjangan Transportasi.");
                    return new List<TransportAllowanceLine>();
                }
            }
        }

        // Tanpa prorata, hanya tunjangan yang terakhir berlaku di jendela dibayar penuh (pola coveringAssignment PAY-BE-17).
        DateTime? lastFullEnd = monthly.Where(m => !m.Policy.IsProrated).Select(m => (DateTime?)m.End).Max();

        var result = new List<TransportAllowanceLine>();
        foreach (var (allowance, policy, start, end) in monthly)
        {
            var monthlyValue = allowance.MonthlyAmount > 0m ? allowance.MonthlyAmount : policy.FixedMonthlyAmount;
            if (policy.MonthlyLimitAmount > 0m && monthlyValue > policy.MonthlyLimitAmount) monthlyValue = policy.MonthlyLimitAmount;

            var days = (end - start).Days + 1;
            decimal amount;
            if (policy.IsProrated)
            {
                amount = monthlyValue * days / totalDays;
            }
            else
            {
                if (end != lastFullEnd) continue;
                amount = monthlyValue;
            }

            var rounded = Round(amount);
            if (rounded == 0m) continue;

            result.Add(new TransportAllowanceLine(
                SourceId: allowance.Id,
                Amount: rounded,
                MonthlyAmount: Round(monthlyValue),
                Taxable: policy.IsTaxable,
                Prorated: policy.IsProrated,
                Formula: "nominal transport sebulan"
                    + (policy.MonthlyLimitAmount > 0m ? ", dibatasi batas bulanan kebijakan" : string.Empty)
                    + (policy.IsProrated ? ", x hari berlaku / hari periode" : ", penuh (tidak diprorata)"),
                Detail: new
                {
                    transportAllowancePolicyId = policy.Id,
                    policyCode = policy.PolicyCode,
                    calculationMethod = policy.CalculationMethod,
                    monthlyLimitAmount = policy.MonthlyLimitAmount,
                    startDate = start,
                    endDate = end,
                    days,
                    totalDays
                }));
        }

        return result;
    }

    private static WfpBankAccount? PickPayrollBankAccount(List<WfpBankAccount> accounts, Guid? preferredId, DateTime periodEnd)
    {
        bool Effective(WfpBankAccount a) =>
            (!a.EffectiveStartDate.HasValue || a.EffectiveStartDate.Value.Date <= periodEnd)
            && (!a.EffectiveEndDate.HasValue || a.EffectiveEndDate.Value.Date >= periodEnd);

        if (preferredId.HasValue)
        {
            var preferred = accounts.FirstOrDefault(a => a.Id == preferredId.Value);
            if (preferred is not null) return preferred;
        }
        return accounts.Where(a => a.IsPayrollAccount && Effective(a))
            .OrderByDescending(a => a.IsVerified).ThenByDescending(a => a.IsPrimary).ThenBy(a => a.Id)
            .FirstOrDefault();
    }

    private static bool RangesOverlap(WfpSalaryAssignment a, WfpSalaryAssignment b) =>
        a.EffectiveStartDate.Date <= (b.EffectiveEndDate ?? DateTime.MaxValue).Date
        && b.EffectiveStartDate.Date <= (a.EffectiveEndDate ?? DateTime.MaxValue).Date;

    private static DateTime MaxDate(DateTime a, DateTime? b) => b.HasValue && b.Value.Date > a ? b.Value.Date : a;

    private static DateTime MinDate(DateTime a, DateTime? b) => b.HasValue && b.Value.Date < a ? b.Value.Date : a;

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
