using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// Resolusi kebijakan dari banyak sumber (PAY-BE-FIX-027). Kebijakan tunjangan tidak berasal dari satu tabel: umum (PrlAllowancePolicy),
/// transportasi lama (PrlTransportAllowancePolicy), Shift, Siaga, dan Risiko (tabel master masing-masing). Tabel-tabel itu TIDAK digabung
/// dan identitasnya tidak pernah dipertukarkan: setiap pilihan membawa <c>Source</c>, dan penyimpanan hanya menulis ke tabel penetapan
/// yang kunci asingnya memang menunjuk tabel kebijakan itu.
///
/// Satu aturan cakupan (<see cref="ScopeMatches"/>) dipakai pemuatan pilihan, pratinjau, dan penyimpanan. Memilih jenis tidak pernah
/// menjadi perintah bayar; jalur simpan hanya ada untuk yang punya consumer payroll yang benar.
/// </summary>
public sealed partial class PayrollWorkforceAllowanceService
{
    private const string LegacyFixedMonthly = "FixedMonthly";

    private const string ActivityBlockReason =
        "Dibayar hanya dari aktivitas aktual yang terverifikasi. Penghubung aktivitas ke payroll belum tersedia, sehingga tidak ada penetapan yang dibuat dari popup ini.";

    private const string OvertimeBlockReason =
        "Lembur dibayar dari realisasi lembur yang sudah diverifikasi lewat HRD Lembur dan diserahkan ke payroll, bukan dari penetapan bulanan.";

    private const string VariableBlockReason =
        "Dibayar per periode lewat Payroll Run, Penghasilan Variabel (butuh Payroll Run dan persetujuan); belum dapat dibuat dari popup profil.";

    // ------------------------------------------------------------------------------------------------------------------------
    // Aturan cakupan bersama
    // ------------------------------------------------------------------------------------------------------------------------

    private static bool Lenient(Guid? policy, Guid? employee) => !policy.HasValue || !employee.HasValue || policy.Value == employee.Value;

    /// <summary>
    /// Cakupan kebijakan terhadap pegawai. Grade Gaji: bila <paramref name="requireGrade"/> kebijakan WAJIB menyebut grade yang sama
    /// (kebijakan tanpa grade bukan untuk grade mana pun), bila tidak, kosong berarti semua grade. Struktur Gaji: kosong berarti semua
    /// struktur pada grade itu, terisi harus sama. Legal entity, fasilitas, unit, kategori, dan tipe kepegawaian hanya menolak bila
    /// keduanya terisi dan berbeda (nilai pegawai diambil dari Struktur Gaji efektif; profil tidak menyimpannya sendiri).
    /// </summary>
    private static bool ScopeMatches(
        EffectiveGrade grade,
        Guid? policyGrade,
        Guid? policyStructure,
        Guid? policyLegalEntity,
        Guid? policySite,
        Guid? policyUnit,
        Guid? policyEmployeeCategory,
        Guid? policyEmploymentType,
        bool requireGrade)
    {
        if (requireGrade ? policyGrade != grade.SalaryGradeId : policyGrade.HasValue && policyGrade.Value != grade.SalaryGradeId) return false;
        if (policyStructure.HasValue && policyStructure != grade.SalaryStructureId) return false;

        return Lenient(policyLegalEntity, grade.LegalEntityId)
            && Lenient(policySite, grade.HospitalSiteId)
            && Lenient(policyUnit, grade.OrganizationUnitId)
            && Lenient(policyEmployeeCategory, grade.EmployeeCategoryId)
            && Lenient(policyEmploymentType, grade.EmploymentTypeId);
    }

    /// <summary>"Sumber" dari klien: kosong = General; nilai tak dikenal ditolak, bukan ditebak.</summary>
    private static string NormalizePolicySource(string? source)
    {
        if (string.IsNullOrWhiteSpace(source)) return WorkforceAllowancePolicySources.General;
        var value = source.Trim();

        foreach (var known in new[]
        {
            WorkforceAllowancePolicySources.General,
            WorkforceAllowancePolicySources.TransportLegacy,
            WorkforceAllowancePolicySources.ShiftActivity,
            WorkforceAllowancePolicySources.OnCallActivity,
            WorkforceAllowancePolicySources.HazardActivity
        })
        {
            if (string.Equals(known, value, StringComparison.OrdinalIgnoreCase)) return known;
        }

        throw new ArgumentException("Sumber kebijakan tidak dikenali.");
    }

    private static bool IsActivitySource(string source) =>
        source is WorkforceAllowancePolicySources.ShiftActivity
            or WorkforceAllowancePolicySources.OnCallActivity
            or WorkforceAllowancePolicySources.HazardActivity;

    // ------------------------------------------------------------------------------------------------------------------------
    // Profil jenis: jalur, persyaratan, dan kemampuan simpan (metadata additive)
    // ------------------------------------------------------------------------------------------------------------------------

    private sealed record TypeProfile(
        string Mode,
        bool RequiresPolicy,
        List<string> RequiredInputs,
        bool RequiresVerification,
        string Hint,
        string? StaticBlockReason);

    private static TypeProfile BuildProfile(string? category, string kind, bool isOvertimeComponent)
    {
        var normalized = (category ?? string.Empty).Trim().ToLowerInvariant();

        if (normalized == "transport")
            return new TypeProfile(WorkforceAllowanceAssignmentModes.TransportPolicy, true, new List<string>(), false, "Berdasarkan kebijakan transportasi", null);

        if (normalized == "shift")
            return new TypeProfile(WorkforceAllowanceAssignmentModes.ActivityPolicy, true,
                new List<string> { "Periode", "Jumlah shift yang memenuhi syarat", "Data roster dan kehadiran" }, true, "Memerlukan data aktivitas", ActivityBlockReason);

        if (normalized == "oncall")
            return new TypeProfile(WorkforceAllowanceAssignmentModes.ActivityPolicy, true,
                new List<string> { "Periode", "Referensi penugasan siaga", "Jumlah jam siaga atau panggilan aktual", "Verifikasi atasan" }, true, "Memerlukan data aktivitas", ActivityBlockReason);

        if (normalized == "hazard")
            return new TypeProfile(WorkforceAllowanceAssignmentModes.ActivityPolicy, true,
                new List<string> { "Periode", "Unit atau lokasi kerja", "Hari paparan aktual", "Clearance kesehatan kerja" }, true, "Memerlukan data aktivitas", ActivityBlockReason);

        if (isOvertimeComponent)
            return new TypeProfile(WorkforceAllowanceAssignmentModes.OvertimeHandoff, false,
                new List<string> { "Periode lembur", "Realisasi lembur terverifikasi", "Jumlah jam lembur" }, true, "Memerlukan data lembur", OvertimeBlockReason);

        if (kind == WorkforceAllowanceKinds.Variable || kind == WorkforceAllowanceKinds.Manual)
            return new TypeProfile(WorkforceAllowanceAssignmentModes.VariableInput, false,
                new List<string> { "Payroll Run", "Nominal", "Keterangan" }, false, "Memerlukan nominal manual", VariableBlockReason);

        return new TypeProfile(WorkforceAllowanceAssignmentModes.Policy, true, new List<string>(), false, "Berdasarkan kebijakan tunjangan", null);
    }

    /// <summary>Komponen payroll yang dipakai handoff lembur (IsOvertimeBased atau metode Overtime): pembayarannya milik jalur lembur.</summary>
    private async Task<bool> IsOvertimeComponentAsync(Guid? payrollComponentId, CancellationToken cancellationToken) =>
        payrollComponentId.HasValue
        && await _dbContext.MstPayrollComponents.AsNoTracking().AnyAsync(
            c => c.Id == payrollComponentId.Value && !c.IsDelete && (c.IsOvertimeBased || c.CalculationMethod == "Overtime"),
            cancellationToken);

    private async Task<TypeFacts> LoadTypeFactsAsync(Guid allowanceTypeId, CancellationToken cancellationToken) =>
        await _dbContext.MstAllowanceTypes.AsNoTracking()
            .Where(x => x.Id == allowanceTypeId && !x.IsDelete && x.IsActive)
            .Select(x => new TypeFacts(
                x.Id, x.PayrollComponentId, x.AllowanceTypeCode, x.AllowanceTypeName, x.AllowanceCategory, x.CalculationMethod,
                x.CurrencyCode, x.DefaultAmount, x.DefaultPercentage, x.MaximumAmount, x.IsRecurring, x.IsTaxable, x.IsProrated,
                x.RequiresApproval, x.EffectiveStartDate, x.EffectiveEndDate))
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw new ArgumentException("Jenis tunjangan tidak ditemukan atau tidak aktif di Master Tunjangan.");

    // ------------------------------------------------------------------------------------------------------------------------
    // Kebijakan aktivitas (Shift, Siaga, Risiko): hanya dibaca
    // ------------------------------------------------------------------------------------------------------------------------

    private sealed record ActivityPolicyRow(
        Guid Id,
        Guid AllowanceTypeId,
        string Code,
        string Name,
        string Source,
        string Method,
        DateTime? Start,
        DateTime? End,
        decimal Rate,
        decimal Percentage,
        decimal? MaximumPerPeriod,
        string CurrencyCode,
        List<string> Requirements);

    /// <summary>Kebijakan aktivitas aktif yang cakupannya cocok dengan pegawai. <paramref name="allowanceTypeId"/> null = semua jenis.</summary>
    private async Task<List<ActivityPolicyRow>> LoadActivityPoliciesAsync(
        Guid? allowanceTypeId, EffectiveGrade grade, DateTime today, CancellationToken cancellationToken)
    {
        var rows = new List<ActivityPolicyRow>();

        var shift = await _dbContext.Set<MstShiftAllowancePolicy>().AsNoTracking()
            .Where(p => !p.IsDelete && p.IsActive && (allowanceTypeId == null || p.AllowanceTypeId == allowanceTypeId)
                && (p.EffectiveEndDate == null || p.EffectiveEndDate >= today))
            .ToListAsync(cancellationToken);
        rows.AddRange(shift
            .Where(p => ScopeMatches(grade, p.SalaryGradeId, p.SalaryStructureId, null, p.HospitalSiteId, p.OrganizationUnitId, p.EmployeeCategoryId, p.EmploymentTypeId, requireGrade: false))
            .Select(p =>
            {
                var requirements = new List<string>();
                if (p.RequireAttendanceMatch) requirements.Add("Kehadiran harus cocok dengan shift");
                if (p.RequireCompletedShift) requirements.Add("Shift harus selesai");
                if (p.MinimumEligibleMinutes > 0) requirements.Add($"Minimal {p.MinimumEligibleMinutes} menit");
                if (p.ApplyOnlyNightShift) requirements.Add("Hanya shift malam");
                return new ActivityPolicyRow(p.Id, p.AllowanceTypeId, p.ShiftAllowancePolicyCode, p.ShiftAllowancePolicyName,
                    WorkforceAllowancePolicySources.ShiftActivity, p.CalculationMethod, p.EffectiveStartDate, p.EffectiveEndDate,
                    p.RateAmount, p.PercentageOfBaseSalary, p.MaximumAmountPerPeriod, p.CurrencyCode, requirements);
            }));

        var onCall = await _dbContext.Set<MstOnCallAllowancePolicy>().AsNoTracking()
            .Where(p => !p.IsDelete && p.IsActive && (allowanceTypeId == null || p.AllowanceTypeId == allowanceTypeId)
                && (p.EffectiveEndDate == null || p.EffectiveEndDate >= today))
            .ToListAsync(cancellationToken);
        rows.AddRange(onCall
            .Where(p => ScopeMatches(grade, p.SalaryGradeId, p.SalaryStructureId, null, p.HospitalSiteId, p.OrganizationUnitId, p.EmployeeCategoryId, p.EmploymentTypeId, requireGrade: false))
            .Select(p =>
            {
                var requirements = new List<string>();
                if (p.RequireAttendanceEvidence) requirements.Add("Bukti kehadiran");
                if (p.RequireSupervisorVerification) requirements.Add("Verifikasi atasan");
                if (p.MinimumOnCallHours > 0) requirements.Add($"Minimal {p.MinimumOnCallHours} jam siaga");
                return new ActivityPolicyRow(p.Id, p.AllowanceTypeId, p.OnCallAllowancePolicyCode, p.OnCallAllowancePolicyName,
                    WorkforceAllowancePolicySources.OnCallActivity, p.CalculationMethod, p.EffectiveStartDate, p.EffectiveEndDate,
                    p.BaseRateAmount, p.PercentageOfBaseSalary, p.MaximumAmountPerPeriod, p.CurrencyCode, requirements);
            }));

        var hazard = await _dbContext.Set<MstHazardAllowancePolicy>().AsNoTracking()
            .Where(p => !p.IsDelete && p.IsActive && (allowanceTypeId == null || p.AllowanceTypeId == allowanceTypeId)
                && (p.EffectiveEndDate == null || p.EffectiveEndDate >= today))
            .ToListAsync(cancellationToken);
        rows.AddRange(hazard
            .Where(p => ScopeMatches(grade, p.SalaryGradeId, p.SalaryStructureId, p.LegalEntityId, p.HospitalSiteId, p.OrganizationUnitId, p.EmployeeCategoryId, p.EmploymentTypeId, requireGrade: false))
            .Select(p =>
            {
                var requirements = new List<string> { $"Tingkat risiko: {p.HazardLevel}" };
                if (p.RequireOccupationalHealthClearance) requirements.Add("Clearance kesehatan kerja");
                if (p.RequireActiveAssignment) requirements.Add("Penugasan aktif");
                if (p.MinimumExposureDays > 0) requirements.Add($"Minimal {p.MinimumExposureDays} hari paparan");
                return new ActivityPolicyRow(p.Id, p.AllowanceTypeId, p.HazardAllowancePolicyCode, p.HazardAllowancePolicyName,
                    WorkforceAllowancePolicySources.HazardActivity, p.CalculationMethod, p.EffectiveStartDate, p.EffectiveEndDate,
                    p.RateAmount, p.PercentageOfBaseSalary, p.MaximumAmountPerPeriod, p.CurrencyCode, requirements);
            }));

        return rows;
    }

    private async Task<WorkforceAllowancePreviewResponse> PreviewActivityAsync(
        Guid workforceProfileId, Guid allowanceTypeId, Guid allowancePolicyId, string source, WorkforceAllowanceCaller caller, CancellationToken cancellationToken)
    {
        var today = PayrollDateNormalizer.ToUtcDate(DateTime.UtcNow);
        var type = await LoadTypeFactsAsync(allowanceTypeId, cancellationToken);
        var grade = await LoadEffectiveGradeAsync(workforceProfileId, today, cancellationToken)
            ?? throw new ArgumentException("Pegawai belum memiliki Grade Gaji aktif pada penetapan gaji utama, sehingga kebijakan belum dapat dipilih.");

        var row = (await LoadActivityPoliciesAsync(allowanceTypeId, grade, today, cancellationToken))
            .FirstOrDefault(r => r.Id == allowancePolicyId && r.Source == source)
            ?? throw new ArgumentException("Kebijakan tidak ditemukan, tidak aktif, atau tidak berlaku untuk Grade Gaji, Struktur Gaji, dan cakupan pegawai ini.");

        var visible = caller.CanViewAmount;
        var start = row.Start.HasValue ? PayrollDateNormalizer.ToUtcDate(row.Start.Value) : today;
        return new WorkforceAllowancePreviewResponse
        {
            AllowanceTypeId = type.Id,
            AllowancePolicyId = row.Id,
            PolicyName = row.Name,
            SalaryGradeId = grade.SalaryGradeId,
            SalaryGradeName = grade.SalaryGradeName,
            CalculationMethod = row.Method,
            CurrencyCode = string.IsNullOrWhiteSpace(row.CurrencyCode) ? type.CurrencyCode : row.CurrencyCode,
            AmountsVisible = visible,
            Amount = visible ? row.Rate : null,
            Percentage = visible ? row.Percentage : null,
            MaximumAmount = visible ? row.MaximumPerPeriod : null,
            IsTaxable = type.IsTaxable,
            IsProrated = type.IsProrated,
            RequiresApproval = type.RequiresApproval,
            EffectiveStartDate = start > today ? start : today,
            EffectiveEndDate = row.End.HasValue ? PayrollDateNormalizer.ToUtcDate(row.End.Value) : null,
            IsOpenEnded = !row.End.HasValue,
            EffectiveStartSource = "Kebijakan",
            EffectiveEndSource = row.End.HasValue ? "Kebijakan" : null,
            PolicySource = source,
            CanSave = false,
            BlockedReason = ActivityBlockReason,
            RequiresVerification = true,
            Requirements = row.Requirements
        };
    }

    // ------------------------------------------------------------------------------------------------------------------------
    // Transportasi lama (PrlTransportAllowancePolicy -> PrlTransportAllowance), metode FixedMonthly
    // ------------------------------------------------------------------------------------------------------------------------

    private static DateTime LegacyStart(PrlTransportAllowancePolicy policy, DateTime today)
    {
        var policyStart = policy.EffectiveStartDate.HasValue ? PayrollDateNormalizer.ToUtcDate(policy.EffectiveStartDate.Value) : today;
        return policyStart > today ? policyStart : today;
    }

    private static DateTime? LegacyEnd(PrlTransportAllowancePolicy policy) =>
        policy.EffectiveEndDate.HasValue ? PayrollDateNormalizer.ToUtcDate(policy.EffectiveEndDate.Value) : null;

    private static decimal LegacyMonthlyAmount(PrlTransportAllowancePolicy policy)
    {
        var monthly = policy.FixedMonthlyAmount;
        if (policy.MonthlyLimitAmount > 0m && monthly > policy.MonthlyLimitAmount) monthly = policy.MonthlyLimitAmount;
        return monthly;
    }

    /// <summary>Kebijakan transportasi lama yang aktif, belum berakhir, dan cakupannya cocok (Grade Gaji wajib sama).</summary>
    private async Task<List<PrlTransportAllowancePolicy>> LoadMatchingLegacyTransportPoliciesAsync(
        Guid payrollComponentId, EffectiveGrade grade, DateTime today, CancellationToken cancellationToken)
    {
        var rows = await _dbContext.PrlTransportAllowancePolicies.AsNoTracking()
            .Where(p => !p.IsDelete && p.IsActive && p.PayrollComponentId == payrollComponentId
                && p.SalaryGradeId == grade.SalaryGradeId
                && (p.EffectiveEndDate == null || p.EffectiveEndDate >= today))
            .OrderBy(p => p.PolicyName)
            .ToListAsync(cancellationToken);

        return rows
            .Where(p => ScopeMatches(grade, p.SalaryGradeId, p.SalaryStructureId, p.LegalEntityId, p.HospitalSiteId, null, null, null, requireGrade: true))
            .ToList();
    }

    /// <summary>
    /// Mengapa kebijakan transportasi lama ini belum dapat disimpan. IsConflict = benturan data (409), selain itu penolakan aturan (400).
    /// Jalur ini hanya membuat penetapan bagi yang memang dibayar mesin hitung (FixedMonthly, ikut payroll) dan tidak pernah melewati
    /// persetujuan: jenis yang mewajibkan persetujuan diarahkan ke Kebijakan Tunjangan umum.
    /// </summary>
    private async Task<(string? Reason, bool IsConflict)> EvaluateLegacyTransportAsync(
        PrlTransportAllowancePolicy policy, TypeFacts type, Guid workforceProfileId, DateTime today, CancellationToken cancellationToken)
    {
        if (!string.Equals(policy.CalculationMethod, LegacyFixedMonthly, StringComparison.OrdinalIgnoreCase))
            return ($"Metode {policy.CalculationMethod} belum dihitung mesin payroll, sehingga tidak ada penetapan otomatis yang dibuat.", false);

        if (!policy.IsIncludedInPayroll) return ("Kebijakan ini tidak ikut payroll.", false);
        if (policy.FixedMonthlyAmount <= 0m) return ("Nominal bulanan kebijakan belum diisi.", false);

        if (type.RequiresApproval)
            return ("Jenis tunjangan ini mewajibkan persetujuan, sedangkan jalur transportasi lama tidak memiliki alur persetujuan. Gunakan Kebijakan Tunjangan umum.", false);

        var start = LegacyStart(policy, today);
        var end = LegacyEnd(policy);
        if (end.HasValue && end.Value < start) return ("Kebijakan sudah berakhir.", false);

        // Tabel penetapan transportasi lama hanya mengizinkan satu baris per pegawai (indeks unik).
        if (await _dbContext.PrlTransportAllowances.AsNoTracking().AnyAsync(x => !x.IsDelete && x.WorkforceProfileId == workforceProfileId, cancellationToken))
            return ("Pegawai sudah memiliki penetapan transportasi lama. Nonaktifkan atau minta administrator memindahkannya sebelum menetapkan yang baru.", true);

        if (await TransportAllowanceSourceGuard.HasUnifiedTransportAsync(_dbContext, workforceProfileId, start, end, cancellationToken))
            return ("Pegawai sudah memiliki penetapan transportasi dari Kebijakan Tunjangan umum pada periode yang beririsan, sehingga akan terbayar dua kali.", true);

        return (null, false);
    }

    private async Task<(TypeFacts Type, EffectiveGrade Grade, PrlTransportAllowancePolicy Policy)> ResolveLegacyTransportAsync(
        Guid workforceProfileId, Guid allowanceTypeId, Guid policyId, CancellationToken cancellationToken)
    {
        var today = PayrollDateNormalizer.ToUtcDate(DateTime.UtcNow);
        var type = await LoadTypeFactsAsync(allowanceTypeId, cancellationToken);

        if (ClassifyKind(type.Category, type.Method) != WorkforceAllowanceKinds.TransportPolicy)
            throw new ArgumentException("Kebijakan transportasi hanya berlaku untuk jenis tunjangan Transportasi.");
        if (!type.PayrollComponentId.HasValue)
            throw new ArgumentException("Jenis tunjangan Transportasi belum terhubung ke komponen payroll.");

        var grade = await LoadEffectiveGradeAsync(workforceProfileId, today, cancellationToken)
            ?? throw new ArgumentException("Pegawai belum memiliki Grade Gaji aktif pada penetapan gaji utama, sehingga kebijakan belum dapat dipilih.");

        var policy = (await LoadMatchingLegacyTransportPoliciesAsync(type.PayrollComponentId.Value, grade, today, cancellationToken))
            .FirstOrDefault(p => p.Id == policyId)
            ?? throw new ArgumentException("Kebijakan transportasi tidak ditemukan, tidak aktif, atau tidak berlaku untuk Grade Gaji dan Struktur Gaji pegawai ini.");

        return (type, grade, policy);
    }

    private async Task<WorkforceAllowancePreviewResponse> PreviewLegacyTransportAsync(
        Guid workforceProfileId, Guid allowanceTypeId, Guid policyId, WorkforceAllowanceCaller caller, CancellationToken cancellationToken)
    {
        var today = PayrollDateNormalizer.ToUtcDate(DateTime.UtcNow);
        var (type, grade, policy) = await ResolveLegacyTransportAsync(workforceProfileId, allowanceTypeId, policyId, cancellationToken);
        var (reason, _) = await EvaluateLegacyTransportAsync(policy, type, workforceProfileId, today, cancellationToken);

        var visible = caller.CanViewAmount;
        var isFixed = string.Equals(policy.CalculationMethod, LegacyFixedMonthly, StringComparison.OrdinalIgnoreCase);
        var end = LegacyEnd(policy);
        return new WorkforceAllowancePreviewResponse
        {
            AllowanceTypeId = type.Id,
            AllowancePolicyId = policy.Id,
            PolicyName = policy.PolicyName,
            SalaryGradeId = grade.SalaryGradeId,
            SalaryGradeName = grade.SalaryGradeName,
            CalculationMethod = policy.CalculationMethod,
            CurrencyCode = type.CurrencyCode,
            AmountsVisible = visible,
            Amount = visible && isFixed ? LegacyMonthlyAmount(policy) : null,
            Percentage = null,
            MaximumAmount = visible && policy.MonthlyLimitAmount > 0m ? policy.MonthlyLimitAmount : null,
            IsTaxable = policy.IsTaxable,
            IsProrated = policy.IsProrated,
            RequiresApproval = false,
            EffectiveStartDate = LegacyStart(policy, today),
            EffectiveEndDate = end,
            IsOpenEnded = end is null,
            EffectiveStartSource = policy.EffectiveStartDate.HasValue && PayrollDateNormalizer.ToUtcDate(policy.EffectiveStartDate.Value) > today ? "Kebijakan" : "Tanggal penetapan",
            EffectiveEndSource = end.HasValue ? "Kebijakan" : null,
            PolicySource = WorkforceAllowancePolicySources.TransportLegacy,
            CanSave = reason is null,
            BlockedReason = reason,
            RequiresVerification = false
        };
    }

    /// <summary>
    /// Menyimpan penetapan transportasi bulanan dari kebijakan transportasi lama. Nilai SEMUA diambil ulang dari kebijakan di server;
    /// klien hanya mengirim identitas. Kunci asing menunjuk tabel kebijakan yang benar (TransportAllowancePolicyId), status langsung
    /// Active hanya bila jenis tidak mewajibkan persetujuan (dijamin <see cref="EvaluateLegacyTransportAsync"/>).
    /// </summary>
    private Task<WorkforceAllowanceItemResponse> CreateLegacyTransportAsync(
        Guid workforceProfileId,
        CreateWorkforceAllowanceFromPolicyRequest request,
        Guid actorUserId,
        WorkforceAllowanceCaller caller,
        CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey(workforceProfileId), async () =>
        {
            await EnsureWorkforceExistsAsync(workforceProfileId, cancellationToken);
            var today = PayrollDateNormalizer.ToUtcDate(DateTime.UtcNow);
            var (type, grade, policy) = await ResolveLegacyTransportAsync(workforceProfileId, request.AllowanceTypeId, request.AllowancePolicyId, cancellationToken);

            var (reason, isConflict) = await EvaluateLegacyTransportAsync(policy, type, workforceProfileId, today, cancellationToken);
            if (reason is not null)
            {
                if (isConflict) throw new PayrollRunConflictException(reason);
                throw new ArgumentException(reason);
            }

            var now = DateTime.UtcNow;
            var start = LegacyStart(policy, today);
            var entity = new PrlTransportAllowance
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                TransportAllowancePolicyId = policy.Id,
                PayrollComponentId = policy.PayrollComponentId ?? type.PayrollComponentId,
                AllowanceStatus = "Active",
                CurrencyCode = type.CurrencyCode,
                MonthlyAmount = LegacyMonthlyAmount(policy),
                EffectiveStartDate = start,
                EffectiveEndDate = LegacyEnd(policy),
                Description = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.PrlTransportAllowances.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Audit tanpa nominal: identitas, kebijakan, grade yang dipakai, dan pelaku.
            await _loggerService.AuditAsync(LogCategory, "WorkforceAllowance.CreateFromLegacyTransportPolicy", "Penetapan transportasi dibuat dari kebijakan transportasi.", new
            {
                EntityId = entity.Id,
                WorkforceProfileId = workforceProfileId,
                AllowanceTypeId = type.Id,
                TransportAllowancePolicyId = policy.Id,
                grade.SalaryGradeId,
                ActorUserId = actorUserId
            });

            var (items, _) = await LoadTransportItemsAsync(workforceProfileId, caller, today, cancellationToken);
            return items.First(x => x.Id == entity.Id);
        }, cancellationToken);
}
