using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
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
/// Yang SENGAJA tidak dihitung otomatis karena belum disahkan pemilik kebijakan (NEEDS CONFIRMATION): rumus PPh 21 dan tarif/batas
/// BPJS. Nilai pajak dan iuran diambil dari tetapan profil payroll (PrlPayroll.TaxAmount/InsuranceAmount), tidak ada persentase
/// yang ditanam di kode.
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

    private readonly ApplicationDbContext _dbContext;
    private readonly PayrollPolicyOptions _policy;

    public PayrollCalculationService(ApplicationDbContext dbContext, IOptions<PayrollPolicyOptions> options)
    {
        _dbContext = dbContext;
        _policy = options.Value;
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
    }

    private sealed record IdentityInfo(Guid WorkforceProfileId, bool IsActive, DateTime? JoinDate, DateTime? ResignDate);

    /// <summary>Snapshot kebijakan yang disalin ke run agar hasil dapat ditelusuri ke versi kebijakan yang dipakai.</summary>
    public string BuildPolicySnapshotJson() => JsonSerializer.Serialize(new
    {
        engine = EngineVersion,
        policyVersion = _policy.PolicyVersion,
        needsConfirmation = true,
        proration = "CalendarDays",
        allowProfileSalaryFallback = _policy.AllowProfileSalaryFallback,
        prorateFixedAllowances = _policy.ProrateFixedAllowances,
        fixedAllowanceSource = "MstSalaryStructureAllowance (PGJ-C1); cadangan PrlPayroll.TotalAllowance bila struktur kosong",
        workforceAllowanceSource = "PrlWorkforceAllowance (Approved, aktif, berlaku di periode; tidak dihitung ganda dengan tunjangan struktur)",
        allowProfileAllowanceFallback = _policy.AllowProfileAllowanceFallback,
        unpaidLeaveDeductionEnabled = _policy.UnpaidLeaveDeductionEnabled,
        requireDifferentApprover = _policy.RequireDifferentApprover,
        installmentDefaultMaxNetSalaryPercentage = _policy.InstallmentDefaultMaxNetSalaryPercentage,
        taxAndInsuranceSource = "PrlPayroll.TaxAmount/InsuranceAmount (tetapan profil; rumus PPh/BPJS belum disahkan)"
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

        var assignments = (await _dbContext.WfpSalaryAssignments.AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && workforceIds.Contains(x.WorkforceProfileId)
                    && x.EffectiveStartDate <= periodEnd
                    && (x.EffectiveEndDate == null || x.EffectiveEndDate >= periodStart))
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

            profiles.TryGetValue(employee.PayrollProfileId ?? Guid.Empty, out var profile);
            identities.TryGetValue(employee.WorkforceProfileId, out var identity);
            assignments.TryGetValue(employee.WorkforceProfileId, out var assignmentList);
            bankAccounts.TryGetValue(employee.WorkforceProfileId, out var bankList);
            taxProfiles.TryGetValue(employee.WorkforceProfileId, out var taxProfile);
            insuranceProfiles.TryGetValue(employee.WorkforceProfileId, out var insuranceProfile);
            transportAllowances.TryGetValue(employee.WorkforceProfileId, out var transportList);
            workforceAllowances.TryGetValue(employee.WorkforceProfileId, out var workforceAllowanceList);

            CalculateEmployee(employee, profile, identity, assignmentList ?? new List<WfpSalaryAssignment>(),
                bankList ?? new List<WfpBankAccount>(), taxProfile, insuranceProfile, structureAllowances,
                transportList ?? new List<PrlTransportAllowance>(),
                workforceAllowanceList ?? new List<PrlWorkforceAllowance>(),
                periodStart, periodEnd, totalDays, actorUserId, errors, warnings);

            var label = employee.EmployeeNameSnapshot;
            foreach (var warning in warnings) result.Warnings.Add($"{label}: {warning}");
            if (warnings.Count > 0) result.EmployeeWarnings[employee.Id] = warnings;
            if (errors.Count > 0) result.EmployeeErrors[employee.Id] = errors;

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
        List<WfpBankAccount> bankList,
        PrlTax? taxProfile,
        PrlInsurance? insuranceProfile,
        IReadOnlyDictionary<Guid, List<MstSalaryStructureAllowance>> structureAllowances,
        List<PrlTransportAllowance> transportAllowances,
        List<PrlWorkforceAllowance> workforceAllowances,
        DateTime periodStart,
        DateTime periodEnd,
        int totalDays,
        Guid actorUserId,
        List<string> errors,
        List<string> warnings)
    {
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

        // ---- Gaji pokok dari penetapan gaji yang DISETUJUI, primer, dan efektif (prorata per segmen) ----
        var approved = assignmentList.Where(a => a.ApprovedAt.HasValue && a.IsPrimary).OrderBy(a => a.EffectiveStartDate).ToList();
        var pendingApproval = assignmentList.Count(a => !a.ApprovedAt.HasValue);
        var overlap = false;
        for (var i = 0; i < approved.Count && !overlap; i++)
            for (var j = i + 1; j < approved.Count; j++)
                if (RangesOverlap(approved[i], approved[j])) { overlap = true; break; }
        if (overlap)
            errors.Add("Terdapat lebih dari satu penetapan gaji utama yang berlaku bersamaan. Perbaiki di Penetapan Gaji.");

        decimal baseAmount = 0m;
        WfpSalaryAssignment? coveringAssignment = null;
        var segments = new List<object>();
        // Segmen penetapan yang disetujui beserta jumlah harinya; dipakai juga untuk tunjangan dari struktur (PAY-BE-17).
        var salarySegments = new List<(WfpSalaryAssignment Assignment, int Days)>();
        if (!overlap)
        {
            foreach (var assignment in approved)
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
                    warnings.Add("Gaji pokok memakai kolom kompatibilitas profil payroll karena belum ada penetapan gaji yang disetujui.");
                    segments.Add(new { assignmentId = (Guid?)null, baseSalary = profile.BaseSalary, startDate = windowStart, endDate = windowEnd, days = payableDays, source = "PrlPayroll" });
                }
                else
                {
                    errors.Add(pendingApproval > 0
                        ? "Penetapan gaji belum disetujui. Setujui penetapan gaji sebelum payroll dihitung."
                        : "Belum ada penetapan gaji yang disetujui dan berlaku pada periode ini.");
                }
            }
            else if (pendingApproval > 0)
            {
                warnings.Add($"{pendingApproval} penetapan gaji belum disetujui diabaikan.");
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

        // ---- Pajak dan iuran karyawan: nilai tetapan profil (rumus belum disahkan -> tidak dihitung otomatis) ----
        var taxAmount = Round(profile.TaxAmount);
        var insuranceAmount = Round(profile.InsuranceAmount);
        // Nol pada keduanya belum berdasar kebijakan (profil yang disiapkan otomatis dari HRD memulai dari nol). Hanya peringatan: nilai
        // dan hasil hitung tidak berubah, dan peringatan tidak menahan pengajuan run.
        if (PayrollProfileRules.IsTaxInsuranceUnset(taxAmount, insuranceAmount))
            warnings.Add("Tetapan PPh dan iuran pada profil payroll masih nol (belum ditetapkan). Nilai nol belum berdasar kebijakan; tetapkan di profil payroll sebelum run disahkan.");

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
        Add(PayrollEngineComponentCodes.Tax, "Pajak Penghasilan (PPh 21)", "Tax", "Fixed", "Tax", taxProfile?.Id, taxAmount, false, false,
            taxAmount, 1m, "tetapan profil payroll", new { source = "PrlPayroll.TaxAmount", needsConfirmation = true });
        Add(PayrollEngineComponentCodes.InsuranceEmployee, "Iuran Karyawan (BPJS/Asuransi)", "Insurance", "Fixed", "Insurance",
            insuranceProfile?.Id, insuranceAmount, false, false, insuranceAmount, 1m, "tetapan profil payroll",
            new { source = "PrlPayroll.InsuranceAmount", needsConfirmation = true });

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
        employee.TotalEmployerInsuranceContribution = 0m;
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
            // Peringatan per penerima disimpan di hasil hitung supaya rincian penerima membacanya tanpa menghitung ulang.
            warnings = warnings.ToList()
        });
        employee.SalarySnapshotJson = JsonSerializer.Serialize(new { segments, coveringAssignmentId = coveringAssignment?.Id });
        employee.UpdateDateTime = DateTime.UtcNow;
        employee.UpdateBy = actorUserId;
    }

    private void UpsertComponent(
        PrlPayrollRunEmployee employee, Guid actorUserId, string code, string name, string type, string method, string sourceType,
        Guid? sourceId, decimal amount, bool taxable, bool prorated, decimal baseValue, decimal quantity, string? formula,
        string? detailJson, int sortOrder)
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
        existing.TaxableAmount = taxable ? amount : 0m;
        existing.IsTaxable = taxable;
        existing.IsProrated = prorated;
        existing.IsManual = false;
        existing.IsDisplayedOnPayslip = true;
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
        string Formula, object Detail);

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
                    allowanceTypeId = allowance.AllowanceTypeId,
                    calculationMethod = allowance.CalculationMethod,
                    percentage = isPercentage ? allowance.Percentage : (decimal?)null,
                    maximumAmount = allowance.MaximumAmount,
                    decidedAt = allowance.DecidedAt,
                    startDate = start,
                    endDate = end,
                    days,
                    totalDays
                }));
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
