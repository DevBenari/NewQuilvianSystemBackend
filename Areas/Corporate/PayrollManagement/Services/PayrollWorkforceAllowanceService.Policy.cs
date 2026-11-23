using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// Penetapan tunjangan pegawai berbasis Kebijakan Tunjangan umum (PAY-BE-FIX-021): Master Tunjangan → Kebijakan → tarif per Grade Gaji →
/// penetapan. Memakai aturan, penjaga tumpang-tindih, transaksi Serializable + advisory lock, dan audit yang SAMA dengan penetapan
/// langsung (satu aturan, bukan salinan); hanya sumber nilainya yang berbeda: nominal, metode, batas, dan masa berlaku diambil ulang
/// dari kebijakan di server. Klien hanya mengirim identitas.
///
/// Yang SENGAJA tidak dilakukan di sini (kondisi henti tugas, bukan kelalaian):
/// - Shift, Siaga, Risiko, dan Penghasilan Variabel tidak dapat ditetapkan lewat jalur ini. Mesin hitung membayar setiap penetapan
///   Approved tiap periode, sehingga penetapan jenis berbasis aktivitas akan terbayar otomatis tanpa aktivitas.
/// - Transportasi (PAY-BE-FIX-022) dapat ditetapkan: mesin hitung membayar penetapan Transport hanya bila AssignmentSource = Policy, dan
///   pagar TransportAllowanceSourceGuard mencegah penetapan ini hidup bersamaan dengan transport bulanan lama (FixedMonthly) yang
///   beririsan. Transportasi per kehadiran/per hari tetap hanya di jalur lama (tidak dihitung mesin hitung).
/// </summary>
public sealed partial class PayrollWorkforceAllowanceService
{
    private sealed record EffectiveGrade(
        Guid AssignmentId,
        Guid SalaryGradeId,
        string? SalaryGradeName,
        Guid? LegalEntityId,
        Guid? HospitalSiteId,
        DateTime EffectiveStartDate,
        DateTime? EffectiveEndDate);

    private sealed record PolicyResolution(
        TypeFacts Type,
        PrlAllowancePolicy Policy,
        PrlAllowancePolicyGradeRate Rate,
        EffectiveGrade Grade,
        DateTime Start,
        DateTime? End,
        string StartSource,
        string? EndSource,
        string Method,
        decimal Amount,
        decimal Percentage,
        decimal? Maximum,
        string CurrencyCode,
        Guid? PayrollComponentId);

    // ------------------------------------------------------------------------------------------------------------------------
    // Klasifikasi jenis
    // ------------------------------------------------------------------------------------------------------------------------

    /// <summary>Satu-satunya definisi cara bayar jenis tunjangan; dipakai daftar jenis, daftar kebijakan, preview, dan penyimpanan.</summary>
    internal static string ClassifyKind(string? category, string? method)
    {
        var normalized = (category ?? string.Empty).Trim().ToLowerInvariant();
        if (normalized == "variable") return WorkforceAllowanceKinds.Variable;
        if (normalized == "transport") return WorkforceAllowanceKinds.TransportPolicy;
        if (normalized is "shift" or "oncall" or "hazard") return WorkforceAllowanceKinds.Activity;
        if (string.Equals(method, WorkforceAllowanceEligibilityRules.MasterManualInput, StringComparison.OrdinalIgnoreCase))
            return WorkforceAllowanceKinds.Manual;
        return WorkforceAllowanceKinds.RecurringPolicy;
    }

    /// <summary>
    /// Kode Kebijakan umum hasil pemindahan kebijakan transportasi lama: awalan MIG- + kode lama (maks 46 karakter, agar muat 50).
    /// Harus sama dengan skrip pemindahan SQL (PAY-BE-FIX-022, lampiran).
    /// </summary>
    internal static string MigratedPolicyCode(string legacyCode)
    {
        var code = legacyCode ?? string.Empty;
        return "MIG-" + (code.Length > 46 ? code[..46] : code);
    }

    /// <summary>Alasan jenis ini tidak dapat ditetapkan lewat jalur kebijakan; null bila dapat (sebelum memeriksa grade dan kebijakan).</summary>
    internal static string? PolicyFlowBlockReason(string kind, bool isRecurring, bool invalidComponent) => kind switch
    {
        WorkforceAllowanceKinds.Variable =>
            "Penghasilan insidental dibayar lewat Payroll Run, Penghasilan Variabel; bukan tunjangan bulanan pegawai.",
        WorkforceAllowanceKinds.Activity =>
            "Tunjangan berbasis aktivitas dibayar hanya bila aktivitas aktual terpenuhi; belum dapat ditetapkan sebagai tunjangan bulanan.",
        WorkforceAllowanceKinds.Manual => "Tunjangan ini diisi manual per periode, bukan ditetapkan dari kebijakan.",
        _ when !isRecurring => "Jenis tunjangan ini tidak berulang.",
        _ when invalidComponent => "Komponen payroll jenis tunjangan ini tidak aktif.",
        _ => null
    };

    // ------------------------------------------------------------------------------------------------------------------------
    // Grade efektif dan kebijakan yang cocok
    // ------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Grade Gaji efektif = penetapan gaji utama yang aktif dan berlaku pada tanggal itu (WfpSalaryAssignment.SalaryGradeId). Tidak
    /// memerlukan persetujuan penetapan gaji tambahan; BUKAN EmployeeGradeId (grade kepegawaian).
    /// </summary>
    private async Task<EffectiveGrade?> LoadEffectiveGradeAsync(Guid workforceProfileId, DateTime today, CancellationToken cancellationToken) =>
        await _dbContext.WfpSalaryAssignments.AsNoTracking()
            .Where(a => !a.IsDelete && a.IsActive && a.IsPrimary && a.WorkforceProfileId == workforceProfileId
                && a.EffectiveStartDate <= today && (a.EffectiveEndDate == null || a.EffectiveEndDate >= today))
            .OrderByDescending(a => a.ApprovedAt != null && a.ApprovedByUserId != null)
            .ThenByDescending(a => a.EffectiveStartDate)
            .Select(a => new EffectiveGrade(
                a.Id,
                a.SalaryGradeId,
                a.SalaryGrade != null ? a.SalaryGrade.SalaryGradeName : null,
                a.SalaryStructure != null ? a.SalaryStructure.LegalEntityId : null,
                a.SalaryStructure != null ? a.SalaryStructure.HospitalSiteId : null,
                a.EffectiveStartDate,
                a.EffectiveEndDate))
            .FirstOrDefaultAsync(cancellationToken);

    /// <summary>
    /// Kebijakan umum aktif yang punya tarif aktif untuk Grade Gaji pegawai dan belum berakhir pada tanggal itu. Cakupan legal entity dan
    /// fasilitas dicocokkan dengan Struktur Gaji pegawai dan hanya menolak bila keduanya terisi dan berbeda (aturan yang sama dengan
    /// kebijakan transportasi).
    /// </summary>
    private IQueryable<PrlAllowancePolicy> MatchingPolicies(EffectiveGrade grade, DateTime today)
    {
        var gradeId = grade.SalaryGradeId;
        var legalEntityId = grade.LegalEntityId;
        var hospitalSiteId = grade.HospitalSiteId;
        return _dbContext.PrlAllowancePolicies.AsNoTracking().Where(p =>
            !p.IsDelete && p.IsActive
            && (p.EffectiveEndDate == null || p.EffectiveEndDate >= today)
            && (p.LegalEntityId == null || legalEntityId == null || p.LegalEntityId == legalEntityId)
            && (p.HospitalSiteId == null || hospitalSiteId == null || p.HospitalSiteId == hospitalSiteId)
            && p.GradeRates.Any(r => !r.IsDelete && r.IsActive && r.SalaryGradeId == gradeId
                && (r.EffectiveEndDate == null || r.EffectiveEndDate >= today)));
    }

    // ------------------------------------------------------------------------------------------------------------------------
    // GET: jenis, kebijakan, preview (tidak pernah menulis data)
    // ------------------------------------------------------------------------------------------------------------------------

    public async Task<WorkforceAllowanceEligibleTypesResponse> GetEligibleTypesAsync(Guid workforceProfileId, CancellationToken cancellationToken)
    {
        await EnsureWorkforceExistsAsync(workforceProfileId, cancellationToken);
        var today = PayrollDateNormalizer.ToUtcDate(DateTime.UtcNow);
        var grade = await LoadEffectiveGradeAsync(workforceProfileId, today, cancellationToken);

        var types = await _dbContext.MstAllowanceTypes.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive)
            .OrderBy(x => x.AllowanceTypeName)
            .Select(x => new
            {
                x.Id,
                x.AllowanceTypeCode,
                x.AllowanceTypeName,
                x.AllowanceCategory,
                x.CalculationMethod,
                x.IsRecurring,
                x.PayrollComponentId,
                InvalidComponent = x.PayrollComponentId != null
                    && (x.PayrollComponent == null || !x.PayrollComponent.IsActive || x.PayrollComponent.IsDelete)
            })
            .ToListAsync(cancellationToken);

        var typesWithPolicy = new HashSet<Guid>();
        if (grade is not null)
            typesWithPolicy = (await MatchingPolicies(grade, today).Select(p => p.AllowanceTypeId).Distinct().ToListAsync(cancellationToken)).ToHashSet();

        // Komponen payroll jenis Transportasi yang sudah punya kebijakan LAMA (Daftar Kebijakan Tunjangan Transportasi) untuk Grade Gaji
        // pegawai: dipakai hanya untuk pesan yang jujur, bukan agar kebijakan lama dapat dipilih.
        var componentsWithLegacyPolicy = new HashSet<Guid>();
        if (grade is not null)
        {
            var legacyGradeId = grade.SalaryGradeId;
            componentsWithLegacyPolicy = (await _dbContext.PrlTransportAllowancePolicies.AsNoTracking()
                .Where(p => !p.IsDelete && p.IsActive && p.SalaryGradeId == legacyGradeId && p.PayrollComponentId != null
                    && (p.EffectiveEndDate == null || p.EffectiveEndDate >= today))
                .Select(p => p.PayrollComponentId!.Value)
                .Distinct()
                .ToListAsync(cancellationToken)).ToHashSet();
        }

        var items = types.Select(x =>
        {
            var kind = ClassifyKind(x.AllowanceCategory, x.CalculationMethod);
            var reason = PolicyFlowBlockReason(kind, x.IsRecurring, x.InvalidComponent);
            var hasPolicy = typesWithPolicy.Contains(x.Id);
            if (reason is null && grade is null) reason = "Pegawai belum memiliki Grade Gaji aktif pada penetapan gaji utama.";
            if (reason is null && !hasPolicy)
                reason = kind == WorkforceAllowanceKinds.TransportPolicy && x.PayrollComponentId.HasValue && componentsWithLegacyPolicy.Contains(x.PayrollComponentId.Value)
                    ? "Kebijakan transportasi untuk Grade Gaji ini masih di Daftar Kebijakan Tunjangan Transportasi lama dan belum dipindahkan ke Kebijakan Tunjangan umum; hubungi HRD/administrator untuk memindahkannya."
                    : "Belum tersedia kebijakan aktif untuk jenis tunjangan dan Grade Gaji ini.";
            var assignable = reason is null;
            return new WorkforceAllowanceEligibleTypeResponse
            {
                Id = x.Id,
                Code = x.AllowanceTypeCode,
                Name = x.AllowanceTypeName,
                Category = x.AllowanceCategory,
                CalculationMethod = x.CalculationMethod,
                Kind = kind,
                Selectable = true,
                Assignable = assignable,
                // Payable: hanya tunjangan rutin (termasuk Transportasi bulanan) yang sudah ditetapkan yang dihitung mesin tiap periode.
                // Jenis lain tidak pernah otomatis.
                Payable = assignable && (kind == WorkforceAllowanceKinds.RecurringPolicy || kind == WorkforceAllowanceKinds.TransportPolicy),
                HasPolicyForGrade = hasPolicy,
                Reason = reason
            };
        }).ToList();

        return new WorkforceAllowanceEligibleTypesResponse
        {
            SalaryGradeId = grade?.SalaryGradeId,
            SalaryGradeName = grade?.SalaryGradeName,
            Items = items
        };
    }

    public async Task<WorkforceAllowancePolicyOptionsResponse> GetPoliciesAsync(
        Guid workforceProfileId, Guid allowanceTypeId, CancellationToken cancellationToken)
    {
        await EnsureWorkforceExistsAsync(workforceProfileId, cancellationToken);
        var today = PayrollDateNormalizer.ToUtcDate(DateTime.UtcNow);
        var grade = await LoadEffectiveGradeAsync(workforceProfileId, today, cancellationToken);
        var type = await _dbContext.MstAllowanceTypes.AsNoTracking()
            .Where(x => x.Id == allowanceTypeId && !x.IsDelete && x.IsActive)
            .Select(x => new { x.AllowanceCategory, x.CalculationMethod, x.IsRecurring, x.PayrollComponentId })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ArgumentException("Jenis tunjangan tidak ditemukan atau tidak aktif di Master Tunjangan.");

        var response = new WorkforceAllowancePolicyOptionsResponse
        {
            AllowanceTypeId = allowanceTypeId,
            SalaryGradeId = grade?.SalaryGradeId,
            SalaryGradeName = grade?.SalaryGradeName
        };
        if (grade is null) return response;

        var kind = ClassifyKind(type.AllowanceCategory, type.CalculationMethod);
        if ((kind == WorkforceAllowanceKinds.RecurringPolicy || kind == WorkforceAllowanceKinds.TransportPolicy) && type.IsRecurring)
        {
            response.Items = await MatchingPolicies(grade, today)
                .Where(p => p.AllowanceTypeId == allowanceTypeId)
                .OrderBy(p => p.PolicyName)
                .Select(p => new WorkforceAllowancePolicyOptionResponse
                {
                    Id = p.Id,
                    Code = p.PolicyCode,
                    Name = p.PolicyName,
                    Source = WorkforceAllowancePolicySources.General,
                    CalculationMethod = p.CalculationMethod,
                    EffectiveStartDate = p.EffectiveStartDate,
                    EffectiveEndDate = p.EffectiveEndDate,
                    Assignable = true
                })
                .ToListAsync(cancellationToken);
        }

        if (kind == WorkforceAllowanceKinds.TransportPolicy && type.PayrollComponentId.HasValue)
        {
            // Hanya dibaca: kebijakan transportasi lama (per-EmployeeGrade/SalaryGrade, metode beragam) tampil agar petugas melihat apa yang
            // berlaku, tetapi tidak dapat dipilih di sini sampai dipetakan HRD ke kebijakan umum (NEEDS CONFIRMATION, PAY-BE-FIX-022).
            var gradeId = grade.SalaryGradeId;
            var componentId = type.PayrollComponentId.Value;
            const string reason = "Kebijakan transportasi lama belum dipindahkan ke Kebijakan Tunjangan umum; hubungi HRD untuk pemetaannya.";
            var legacy = await _dbContext.PrlTransportAllowancePolicies.AsNoTracking()
                .Where(p => !p.IsDelete && p.IsActive && p.SalaryGradeId == gradeId && p.PayrollComponentId == componentId
                    && (p.EffectiveEndDate == null || p.EffectiveEndDate >= today))
                .OrderBy(p => p.PolicyName)
                .Select(p => new WorkforceAllowancePolicyOptionResponse
                {
                    Id = p.Id,
                    Code = p.PolicyCode,
                    Name = p.PolicyName,
                    Source = WorkforceAllowancePolicySources.TransportLegacy,
                    CalculationMethod = p.CalculationMethod,
                    EffectiveStartDate = p.EffectiveStartDate ?? today,
                    EffectiveEndDate = p.EffectiveEndDate,
                    Assignable = false,
                    Reason = reason
                })
                .ToListAsync(cancellationToken);
            // Kebijakan lama yang sudah dipindahkan (kode umum = MigratedPolicyCode) tidak ditampilkan dua kali.
            var generalCodes = response.Items.Select(i => i.Code).ToHashSet(StringComparer.Ordinal);
            legacy = legacy.Where(l => !generalCodes.Contains(MigratedPolicyCode(l.Code))).ToList();
            response.Items = response.Items.Concat(legacy).ToList();
        }

        return response;
    }

    public async Task<WorkforceAllowancePreviewResponse> PreviewAsync(
        Guid workforceProfileId, Guid allowanceTypeId, Guid allowancePolicyId, WorkforceAllowanceCaller caller, CancellationToken cancellationToken)
    {
        await EnsureWorkforceExistsAsync(workforceProfileId, cancellationToken);
        var resolution = await ResolvePolicyAsync(workforceProfileId, allowanceTypeId, allowancePolicyId, cancellationToken);
        var visible = caller.CanViewAmount;
        return new WorkforceAllowancePreviewResponse
        {
            AllowanceTypeId = resolution.Type.Id,
            AllowancePolicyId = resolution.Policy.Id,
            PolicyName = resolution.Policy.PolicyName,
            SalaryGradeId = resolution.Grade.SalaryGradeId,
            SalaryGradeName = resolution.Grade.SalaryGradeName,
            CalculationMethod = resolution.Method,
            CurrencyCode = resolution.CurrencyCode,
            AmountsVisible = visible,
            Amount = visible ? resolution.Amount : null,
            Percentage = visible ? resolution.Percentage : null,
            MaximumAmount = visible ? resolution.Maximum : null,
            IsTaxable = resolution.Type.IsTaxable,
            IsProrated = resolution.Type.IsProrated,
            RequiresApproval = resolution.Type.RequiresApproval,
            EffectiveStartDate = resolution.Start,
            EffectiveEndDate = resolution.End,
            IsOpenEnded = resolution.End is null,
            EffectiveStartSource = resolution.StartSource,
            EffectiveEndSource = resolution.EndSource
        };
    }

    // ------------------------------------------------------------------------------------------------------------------------
    // Resolusi: satu-satunya tempat nilai ditentukan (preview dan penyimpanan memakainya)
    // ------------------------------------------------------------------------------------------------------------------------

    private async Task<PolicyResolution> ResolvePolicyAsync(
        Guid workforceProfileId, Guid allowanceTypeId, Guid allowancePolicyId, CancellationToken cancellationToken)
    {
        var today = PayrollDateNormalizer.ToUtcDate(DateTime.UtcNow);

        var row = await _dbContext.MstAllowanceTypes.AsNoTracking()
            .Where(x => x.Id == allowanceTypeId && !x.IsDelete && x.IsActive)
            .Select(x => new
            {
                Facts = new TypeFacts(
                    x.Id, x.PayrollComponentId, x.AllowanceTypeCode, x.AllowanceTypeName, x.AllowanceCategory, x.CalculationMethod,
                    x.CurrencyCode, x.DefaultAmount, x.DefaultPercentage, x.MaximumAmount, x.IsRecurring, x.IsTaxable, x.IsProrated,
                    x.RequiresApproval, x.EffectiveStartDate, x.EffectiveEndDate),
                InvalidComponent = x.PayrollComponentId != null
                    && (x.PayrollComponent == null || !x.PayrollComponent.IsActive || x.PayrollComponent.IsDelete)
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ArgumentException("Jenis tunjangan tidak ditemukan atau tidak aktif di Master Tunjangan.");
        var type = row.Facts;

        var blocked = PolicyFlowBlockReason(ClassifyKind(type.Category, type.Method), type.IsRecurring, row.InvalidComponent);
        if (blocked is not null) throw new ArgumentException($"Tunjangan {type.Name}: {blocked}");

        var grade = await LoadEffectiveGradeAsync(workforceProfileId, today, cancellationToken)
            ?? throw new ArgumentException("Pegawai belum memiliki Grade Gaji aktif pada penetapan gaji utama, sehingga kebijakan belum dapat dipilih.");

        var policy = await _dbContext.PrlAllowancePolicies.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == allowancePolicyId && p.AllowanceTypeId == allowanceTypeId && !p.IsDelete && p.IsActive, cancellationToken)
            ?? throw new ArgumentException("Kebijakan tunjangan tidak ditemukan, tidak aktif, atau bukan milik jenis tunjangan ini.");

        if (policy.LegalEntityId.HasValue && grade.LegalEntityId.HasValue && policy.LegalEntityId.Value != grade.LegalEntityId.Value)
            throw new ArgumentException("Kebijakan ini berlaku untuk legal entity yang berbeda dari Struktur Gaji pegawai.");
        if (policy.HospitalSiteId.HasValue && grade.HospitalSiteId.HasValue && policy.HospitalSiteId.Value != grade.HospitalSiteId.Value)
            throw new ArgumentException("Kebijakan ini berlaku untuk fasilitas yang berbeda dari Struktur Gaji pegawai.");

        // Tanggal mulai ditentukan backend: paling akhir dari hari ini, mulai kebijakan, dan mulai jenis tunjangan. Tarif harus sudah
        // berlaku pada tanggal itu.
        var start = today;
        var startSource = "Tanggal penetapan";
        var policyStart = PayrollDateNormalizer.ToUtcDate(policy.EffectiveStartDate);
        if (policyStart > start) { start = policyStart; startSource = "Kebijakan"; }
        if (type.EffectiveStartDate.HasValue)
        {
            var typeStart = PayrollDateNormalizer.ToUtcDate(type.EffectiveStartDate.Value);
            if (typeStart > start) { start = typeStart; startSource = "Jenis Tunjangan"; }
        }

        if (policy.EffectiveEndDate.HasValue && PayrollDateNormalizer.ToUtcDate(policy.EffectiveEndDate.Value) < start)
            throw new ArgumentException($"Kebijakan {policy.PolicyName} sudah berakhir pada {PayrollDateNormalizer.ToUtcDate(policy.EffectiveEndDate.Value):dd MMM yyyy}.");

        var gradeId = grade.SalaryGradeId;
        var rate = await _dbContext.PrlAllowancePolicyGradeRates.AsNoTracking()
            .Where(r => r.AllowancePolicyId == policy.Id && r.SalaryGradeId == gradeId && !r.IsDelete && r.IsActive
                && r.EffectiveStartDate <= start && (r.EffectiveEndDate == null || r.EffectiveEndDate >= start))
            .OrderByDescending(r => r.EffectiveStartDate)
            .FirstOrDefaultAsync(cancellationToken);
        if (rate is null)
        {
            var futureStart = await _dbContext.PrlAllowancePolicyGradeRates.AsNoTracking()
                .Where(r => r.AllowancePolicyId == policy.Id && r.SalaryGradeId == gradeId && !r.IsDelete && r.IsActive && r.EffectiveStartDate > start)
                .OrderBy(r => r.EffectiveStartDate)
                .Select(r => (DateTime?)r.EffectiveStartDate)
                .FirstOrDefaultAsync(cancellationToken);
            throw new ArgumentException(futureStart.HasValue
                ? $"Tarif kebijakan untuk Grade Gaji pegawai baru berlaku sejak {PayrollDateNormalizer.ToUtcDate(futureStart.Value):dd MMM yyyy}."
                : "Kebijakan ini belum memiliki tarif aktif untuk Grade Gaji pegawai.");
        }

        // Tanggal akhir: yang paling awal dari kebijakan, tarif, jenis tunjangan, dan penetapan gaji (perubahan grade tidak diikuti
        // diam-diam: penetapan baru dibuat untuk grade baru). Kosong hanya bila semuanya kosong, dan itu dinyatakan eksplisit.
        var ends = new List<(DateTime Date, string Source)>();
        if (policy.EffectiveEndDate.HasValue) ends.Add((PayrollDateNormalizer.ToUtcDate(policy.EffectiveEndDate.Value), "Kebijakan"));
        if (rate.EffectiveEndDate.HasValue) ends.Add((PayrollDateNormalizer.ToUtcDate(rate.EffectiveEndDate.Value), "Tarif Grade Gaji"));
        if (type.EffectiveEndDate.HasValue) ends.Add((PayrollDateNormalizer.ToUtcDate(type.EffectiveEndDate.Value), "Jenis Tunjangan"));
        if (grade.EffectiveEndDate.HasValue) ends.Add((PayrollDateNormalizer.ToUtcDate(grade.EffectiveEndDate.Value), "Penetapan Gaji pegawai"));
        DateTime? end = null;
        string? endSource = null;
        if (ends.Count > 0)
        {
            var earliest = ends.OrderBy(e => e.Date).First();
            end = earliest.Date;
            endSource = earliest.Source;
        }
        ValidateDates(type, start, end);

        var method = policy.CalculationMethod;
        decimal amount = 0m, percentage = 0m;
        if (string.Equals(method, SalaryStructureAllowanceCalculationMethods.FixedAmount, StringComparison.Ordinal))
        {
            amount = Math.Round(rate.Amount, 2, MidpointRounding.AwayFromZero);
            if (amount <= 0m) throw new ArgumentException("Tarif kebijakan untuk Grade Gaji ini tidak valid: nominal harus lebih dari nol.");
        }
        else if (string.Equals(method, SalaryStructureAllowanceCalculationMethods.PercentageOfBaseSalary, StringComparison.Ordinal))
        {
            percentage = Math.Round(rate.Percentage, 2, MidpointRounding.AwayFromZero);
            if (percentage <= 0m || percentage > 100m)
                throw new ArgumentException("Tarif kebijakan untuk Grade Gaji ini tidak valid: persentase harus di atas 0 dan paling tinggi 100.");
        }
        else
        {
            throw new ArgumentException("Metode perhitungan kebijakan tidak dikenali.");
        }

        decimal? maximum = null;
        foreach (var candidate in new[] { rate.MaximumAmount, type.MaximumAmount })
            if (candidate.HasValue && candidate.Value > 0m) maximum = maximum.HasValue ? Math.Min(maximum.Value, candidate.Value) : candidate.Value;
        if (amount > 0m && maximum.HasValue && amount > maximum.Value)
            throw new ArgumentException($"Nominal tarif kebijakan melebihi batas maksimum tunjangan {type.Name}.");

        var componentId = policy.PayrollComponentId ?? type.PayrollComponentId;
        if (policy.PayrollComponentId.HasValue
            && !await _dbContext.MstPayrollComponents.AsNoTracking()
                .AnyAsync(c => c.Id == policy.PayrollComponentId.Value && c.IsActive && !c.IsDelete, cancellationToken))
            throw new ArgumentException("Komponen payroll pada kebijakan ini tidak aktif.");

        return new PolicyResolution(
            type, policy, rate, grade, start, end, startSource, endSource, method, amount, percentage, maximum, policy.CurrencyCode, componentId);
    }

    // ------------------------------------------------------------------------------------------------------------------------
    // POST
    // ------------------------------------------------------------------------------------------------------------------------

    public Task<WorkforceAllowanceItemResponse> CreateFromPolicyAsync(
        Guid workforceProfileId,
        CreateWorkforceAllowanceFromPolicyRequest request,
        Guid actorUserId,
        WorkforceAllowanceCaller caller,
        CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey(workforceProfileId), async () =>
        {
            await EnsureWorkforceExistsAsync(workforceProfileId, cancellationToken);
            var resolution = await ResolvePolicyAsync(workforceProfileId, request.AllowanceTypeId, request.AllowancePolicyId, cancellationToken);

            // Idempotensi: permintaan yang sama (klik ganda, kirim ulang) mengembalikan penetapan yang sudah ada, bukan penetapan kedua.
            var existingId = await _dbContext.PrlWorkforceAllowances.AsNoTracking()
                .Where(x => !x.IsDelete && x.WorkforceProfileId == workforceProfileId && x.AllowanceTypeId == resolution.Type.Id
                    && x.AssignmentSource == WorkforceAllowanceAssignmentSources.Policy
                    && x.AllowancePolicyId == resolution.Policy.Id
                    && x.SalaryGradeId == resolution.Grade.SalaryGradeId
                    && x.EffectiveStartDate == resolution.Start
                    && (x.AllowanceStatus == WorkforceAllowanceStatuses.Draft
                        || x.AllowanceStatus == WorkforceAllowanceStatuses.PendingApproval
                        || x.AllowanceStatus == WorkforceAllowanceStatuses.Approved))
                .Select(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (existingId != Guid.Empty) return await GetItemAsync(workforceProfileId, existingId, caller, cancellationToken);

            await EnsureNoOverlapAsync(workforceProfileId, resolution.Type, excludeId: null, resolution.Start, resolution.End, cancellationToken);
            await EnsureNotInSalaryStructureAsync(workforceProfileId, resolution.Type, resolution.Start, resolution.End, cancellationToken);

            // Satu sumber pembayaran Transportasi: transport bulanan lama yang masih berlaku akan terbayar bersamaan (PAY-BE-FIX-022).
            if (ClassifyKind(resolution.Type.Category, resolution.Type.Method) == WorkforceAllowanceKinds.TransportPolicy
                && await TransportAllowanceSourceGuard.HasLegacyMonthlyTransportAsync(
                    _dbContext, workforceProfileId, resolution.Start, resolution.End, excludeId: null, cancellationToken))
                throw new PayrollRunConflictException(
                    $"Pegawai ini masih punya tunjangan transportasi bulanan di Kelola Transportasi yang berlaku pada periode yang beririsan. Nonaktifkan atau akhiri lebih dulu agar {resolution.Type.Name} tidak terbayar dua kali.");

            var now = DateTime.UtcNow;
            var entity = new PrlWorkforceAllowance
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false,
                AssignmentSource = WorkforceAllowanceAssignmentSources.Policy,
                AllowancePolicyId = resolution.Policy.Id,
                AllowancePolicyGradeRateId = resolution.Rate.Id,
                SalaryGradeId = resolution.Grade.SalaryGradeId
            };
            ApplyTerms(entity, resolution.Type, resolution.Method, resolution.Amount, resolution.Percentage, resolution.Start, resolution.End, request.Notes);
            entity.PayrollComponentId = resolution.PayrollComponentId;
            entity.CurrencyCode = resolution.CurrencyCode;
            entity.MaximumAmount = resolution.Maximum;
            StartLifecycle(entity, resolution.Type, actorUserId, now);

            _dbContext.PrlWorkforceAllowances.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Audit tanpa nominal dan persen: identitas, kebijakan, tarif, grade yang dipakai, status, dan pelaku.
            await _loggerService.AuditAsync(LogCategory, "WorkforceAllowance.CreateFromPolicy", "Penetapan tunjangan pegawai dibuat dari kebijakan.", new
            {
                EntityId = entity.Id,
                WorkforceProfileId = workforceProfileId,
                entity.AllowanceTypeId,
                AllowancePolicyId = resolution.Policy.Id,
                PolicyRevisionNo = resolution.Policy.RevisionNo,
                AllowancePolicyGradeRateId = resolution.Rate.Id,
                resolution.Grade.SalaryGradeId,
                Status = entity.AllowanceStatus,
                ActorUserId = actorUserId
            });
            return await GetItemAsync(workforceProfileId, entity.Id, caller, cancellationToken);
        }, cancellationToken);
}
