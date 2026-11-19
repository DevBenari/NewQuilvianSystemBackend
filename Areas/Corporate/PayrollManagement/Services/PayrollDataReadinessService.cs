using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// Kelengkapan data payroll per pegawai dan dokter aktif. Daftar bersumber dari POPULASI pegawai dan dokter (master HRD), bukan dari
/// profil payroll, sehingga orang yang belum punya profil tetap tampil.
///
/// Layanan ini HANYA MEMBACA: tidak membuat atau mengubah data apa pun, termasuk profil payroll. Profil operasional payroll (PrlPayroll)
/// disiapkan otomatis oleh <see cref="PayrollProfileProvisioningService"/> saat payroll run dipersiapkan; tidak ada impor. Penetapan
/// gaji, rekening, dan penempatan organisasi milik HRD dan hanya dibaca lewat <see cref="PayrollHrdSourceReader"/>, dengan pilihan
/// sumber dan aturan profil yang sama dengan penyiapan otomatis dan mesin hitung (<see cref="PayrollProfileRules"/>).
///
/// Tiga hal dinilai terpisah (lihat <see cref="PayrollDataReadinessItemResponse"/>): kelengkapan sumber HRD, status profil operasional
/// internal Payroll, dan kesiapan mengikuti proses Payroll. Daftar dan ringkasan memakai satu fungsi (<see cref="LoadReadinessAsync"/>)
/// sehingga angkanya tidak dapat berselisih.
/// </summary>
public sealed class PayrollDataReadinessService
{
    private const string CompleteFilter = "Lengkap";
    private const string IncompleteFilter = "Belum lengkap";
    private const int MaxPageSize = 100;

    private readonly ApplicationDbContext _dbContext;
    private readonly PayrollHrdSourceReader _hrdSource;
    private readonly PayrollStatutoryPolicyResolver _policyResolver;

    public PayrollDataReadinessService(
        ApplicationDbContext dbContext, PayrollHrdSourceReader hrdSource, PayrollStatutoryPolicyResolver policyResolver)
    {
        _dbContext = dbContext;
        _hrdSource = hrdSource;
        _policyResolver = policyResolver;
    }

    public async Task<PagedResult<PayrollDataReadinessItemResponse>> ListAsync(
        PayrollDataReadinessQuery query, CancellationToken cancellationToken)
    {
        IEnumerable<PayrollDataReadinessItemResponse> rows = await LoadReadinessAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(query.PersonType))
        {
            var type = query.PersonType.Trim();
            rows = rows.Where(x => string.Equals(x.PersonType, type, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            rows = rows.Where(x =>
                x.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                || x.Number.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        var completeness = query.Completeness?.Trim();
        if (string.Equals(completeness, CompleteFilter, StringComparison.OrdinalIgnoreCase))
            rows = rows.Where(x => x.IsComplete);
        else if (string.Equals(completeness, IncompleteFilter, StringComparison.OrdinalIgnoreCase))
            rows = rows.Where(x => !x.IsComplete);

        var filtered = rows
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Number, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

        return new PagedResult<PayrollDataReadinessItemResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = filtered.Count,
            TotalPage = (int)Math.Ceiling(filtered.Count / (double)pageSize),
            Items = filtered.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList()
        };
    }

    public async Task<PayrollDataReadinessSummaryResponse> SummaryAsync(CancellationToken cancellationToken)
    {
        var all = await LoadReadinessAsync(cancellationToken);
        var availability = await _policyResolver.GetAvailabilityAsync(DateTime.UtcNow.Date, cancellationToken);
        var unconfiguredTaxCount = all.Count(x => x.TaxConfigurationStatus != PayrollConfigurationStatuses.Configured);
        return new PayrollDataReadinessSummaryResponse
        {
            TaxPolicyApproved = availability.TaxApproved,
            TaxPolicyVersion = availability.TaxVersion,
            TaxPolicyStatusLabel = availability.TaxApproved ? "Kebijakan tarif PPh disahkan" : StatutoryPolicyLabels.AwaitingVerification,
            EnableBpjsInPayroll = availability.EnableBpjsInPayroll,
            BpjsPolicyApproved = availability.BpjsApproved,
            BpjsPolicyVersion = availability.BpjsVersion,
            BpjsPolicyStatusLabel = !availability.EnableBpjsInPayroll
                ? StatutoryPolicyLabels.BpjsTestingModeDisabled
                : (availability.BpjsApproved ? "Kebijakan iuran BPJS disahkan" : StatutoryPolicyLabels.AwaitingVerification),
            TotalPeople = all.Count,
            WithPayrollProfile = all.Count(x => x.HasPayrollProfile),
            WithApprovedSalaryAssignment = all.Count(x => x.HasApprovedSalaryAssignment),
            WithVerifiedBankAccount = all.Count(x => x.BankStatus == PayrollReadinessEvaluator.BankVerified),
            Complete = all.Count(x => x.IsComplete),
            HrdComplete = all.Count(x => x.HrdComplete),
            ProfilesToPrepare = all.Count(x => x.PayrollProfileStatus == PayrollReadinessEvaluator.ProfileAutoPrepare),
            ProfilesNeedAttention = all.Count(x => x.PayrollProfileStatus == PayrollReadinessEvaluator.ProfileNeedsAttention),
            TaxNotSet = unconfiguredTaxCount,
            TaxInsuranceNotSet = availability.EnableBpjsInPayroll
                ? all.Count(x => x.TaxInsuranceStatus == PayrollReadinessEvaluator.TaxNotSet)
                : unconfiguredTaxCount
        };
    }

    /// <summary>
    /// Satu-satunya tempat aturan kelengkapan. Populasi (pegawai dan dokter aktif dari master HRD) dipertemukan dengan profil payroll,
    /// penetapan gaji, dan rekening yang berlaku HARI INI di memori. Jumlahnya dibatasi jumlah SDM rumah sakit, dan penyaringan lewat
    /// kelengkapan tidak dapat dipaginasi di database tanpa menggabungkan beberapa tabel. Perubahan data HRD tercermin pada pembacaan
    /// berikutnya menurut masa berlakunya.
    /// </summary>
    private async Task<List<PayrollDataReadinessItemResponse>> LoadReadinessAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var availability = await _policyResolver.GetAvailabilityAsync(today, cancellationToken);

        var people = await _hrdSource.LoadPopulationAsync(cancellationToken);
        var salaries = await _hrdSource.LoadSalariesAsync(today, today, cancellationToken);
        var banks = await _hrdSource.LoadBanksAsync(today, cancellationToken);

        // Profil yang belum dihapus dalam keadaan apa pun (indeks unik WorkforceProfileId menjamin paling banyak satu per orang). Bila
        // tetap ada beberapa, pemilihannya memakai aturan yang sama dengan layar Profil Penggajian (PayrollReadinessEvaluator.SelectProfile).
        var profiles = (await _dbContext.PrlPayrolls.AsNoTracking()
                .Where(x => !x.IsDelete)
                .Select(x => new
                {
                    x.WorkforceProfileId,
                    x.IsActive,
                    x.IsPayrollEligible,
                    x.PayrollStatus,
                    x.TaxAmount,
                    x.InsuranceAmount,
                    x.EffectiveStartDate,
                    x.EffectiveEndDate
                })
                .ToListAsync(cancellationToken))
            .GroupBy(x => x.WorkforceProfileId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var candidates = g
                        .Select(p => new PayrollProfileFacts(
                            p.IsActive, p.IsPayrollEligible, p.PayrollStatus, p.TaxAmount, p.InsuranceAmount,
                            p.EffectiveStartDate, p.EffectiveEndDate))
                        .ToList();
                    var chosen = PayrollReadinessEvaluator.SelectProfile(candidates, f => f, today, out var conflict)!;
                    return (Facts: chosen, Conflict: conflict);
                });

        // Konfigurasi PPh dan iuran milik HRD (satu baris per orang menurut indeks unik). Bila tetap ada beberapa (data legacy),
        // record aktif yang berlaku hari ini dipilih secara deterministik dan konflik dilaporkan secara eksplisit.
        var taxConfigs = (await _dbContext.PrlTaxes.AsNoTracking().Where(x => !x.IsDelete).ToListAsync(cancellationToken))
            .GroupBy(x => x.WorkforceProfileId)
            .ToDictionary(
                g => g.Key,
                g => SelectTaxConfig(g, today));
        var insuranceConfigs = availability.EnableBpjsInPayroll
            ? (await _dbContext.PrlInsurances.AsNoTracking().Where(x => !x.IsDelete).ToListAsync(cancellationToken))
                .GroupBy(x => x.WorkforceProfileId)
                .ToDictionary(
                    g => g.Key,
                    g => PayrollInsuranceConfigFacts.From(g.OrderByDescending(t => t.IsActive).ThenByDescending(t => t.EffectiveStartDate).First()))
            : new Dictionary<Guid, PayrollInsuranceConfigFacts>();

        var items = new List<PayrollDataReadinessItemResponse>(people.Count);
        foreach (var person in people)
        {
            salaries.TryGetValue(person.WorkforceProfileId, out var salary);
            banks.TryGetValue(person.WorkforceProfileId, out var bank);
            PayrollTaxConfigFacts? taxConfig = null;
            var taxConflict = false;
            if (taxConfigs.TryGetValue(person.WorkforceProfileId, out var taxSelected))
            {
                taxConfig = taxSelected.Facts;
                taxConflict = taxSelected.Conflict;
            }
            insuranceConfigs.TryGetValue(person.WorkforceProfileId, out var insuranceConfig);

            // (1) Sumber HRD dan (2) profil operasional internal Payroll dinilai oleh evaluator yang SAMA dengan layar Profil
            // Penggajian. Profil tidak pernah dibuat di sini.
            var hrd = PayrollReadinessEvaluator.AssessHrd(salary, bank);
            PayrollProfileFacts? profile = null;
            var profileConflict = false;
            if (profiles.TryGetValue(person.WorkforceProfileId, out var found))
            {
                profile = found.Facts;
                profileConflict = found.Conflict;
            }

            // Kelengkapan PPh dan iuran dinilai dari konfigurasi eksplisit HRD; tetapan lama pada profil payroll hanya cadangan bertanda.
            var taxInsurance = PayrollTaxInsuranceRules.AssessReadiness(
                taxConfig, insuranceConfig, profile?.TaxAmount ?? 0m, profile?.InsuranceAmount ?? 0m, today, availability);
            var profileAssessment = PayrollReadinessEvaluator.AssessProfile(
                profile, PayrollProfileRules.CanAutoPrepare(salary is not null, bank is not null), taxInsurance, today, profileConflict);

            var taxIssues = new List<string>(profileAssessment.TaxIssues ?? new List<string>());
            var effectiveTaxConfigStatus = profileAssessment.TaxConfigurationStatus;
            if (taxConflict)
            {
                effectiveTaxConfigStatus = PayrollConfigurationStatuses.Error;
                taxIssues.Add("Terdapat lebih dari satu profil pajak aktif yang berlaku bersamaan di HRD. Perlu diperiksa HRD.");
            }

            var issues = new List<string>(hrd.Issues);
            issues.AddRange(profileAssessment.Issues);
            if (taxConflict)
            {
                issues.Add("Terdapat lebih dari satu profil pajak aktif yang berlaku bersamaan di HRD.");
            }

            var taxStatus = effectiveTaxConfigStatus == PayrollConfigurationStatuses.Configured
                ? PayrollReadinessEvaluator.TaxSet
                : PayrollReadinessEvaluator.TaxNotSet;

            var itemNotes = availability.EnableBpjsInPayroll
                ? profileAssessment.Notes
                : profileAssessment.Notes
                    .Where(n => !n.Contains("BPJS", StringComparison.OrdinalIgnoreCase)
                             && !n.Contains("iuran", StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (!availability.EnableBpjsInPayroll
                && effectiveTaxConfigStatus == PayrollConfigurationStatuses.Configured
                && itemNotes.Count == 0)
            {
                itemNotes.Add("Konfigurasi PPh lengkap.");
            }

            items.Add(new PayrollDataReadinessItemResponse
            {
                WorkforceProfileId = person.WorkforceProfileId,
                PersonType = person.Type,
                Number = person.Number,
                Name = person.Name,
                HasPayrollProfile = profileAssessment.IsOperational,
                HasApprovedSalaryAssignment = hrd.SalaryApproved,
                HasPendingSalaryAssignment = hrd.SalaryPending,
                BankStatus = hrd.BankStatus,
                HrdComplete = hrd.HrdComplete,
                PayrollProfileStatus = profileAssessment.ProfileStatus,
                TaxStatus = taxStatus,
                TaxInsuranceStatus = availability.EnableBpjsInPayroll
                    ? (taxConflict ? PayrollReadinessEvaluator.TaxNotSet : profileAssessment.TaxInsuranceStatus)
                    : taxStatus,
                TaxConfigurationStatus = effectiveTaxConfigStatus,
                InsuranceConfigurationStatus = availability.EnableBpjsInPayroll
                    ? profileAssessment.InsuranceConfigurationStatus
                    : string.Empty,
                TaxConfigurationIssues = taxIssues,
                InsuranceConfigurationIssues = availability.EnableBpjsInPayroll
                    ? (profileAssessment.InsuranceIssues ?? new List<string>())
                    : new List<string>(),
                // (3) Siap mengikuti proses Payroll: tidak ada penghalang. Tetapan PPh dilaporkan terpisah.
                IsComplete = issues.Count == 0,
                Issues = issues,
                Notes = itemNotes
            });
        }

        return items;
    }

    /// <summary>
    /// Memilih satu konfigurasi PPh HRD secara deterministik dari seluruh record non-delete milik satu orang.
    /// Record aktif yang berlaku pada hari ini didahulukan, lalu record aktif lainnya, lalu record nonaktif.
    /// Bila lebih dari satu record aktif berlaku bersamaan pada hari ini, ditandai sebagai konflik integritas data.
    /// </summary>
    private static (PayrollTaxConfigFacts? Facts, bool Conflict) SelectTaxConfig(
        IEnumerable<PrlTax> candidates, DateTime asOf)
    {
        var list = candidates.ToList();
        if (list.Count == 0) return (null, false);
        if (list.Count == 1) return (PayrollTaxConfigFacts.From(list[0]), false);

        bool ActiveNow(PrlTax t) =>
            t.IsActive &&
            (!t.EffectiveStartDate.HasValue || t.EffectiveStartDate.Value.Date <= asOf.Date) &&
            (!t.EffectiveEndDate.HasValue || t.EffectiveEndDate.Value.Date >= asOf.Date);

        var conflict = list.Count(ActiveNow) > 1;
        var chosen = list
            .OrderBy(t => ActiveNow(t) ? 0 : t.IsActive ? 1 : 2)
            .ThenByDescending(t => t.EffectiveStartDate ?? DateTime.MinValue)
            .ThenByDescending(t => t.UpdateDateTime ?? t.CreateDateTime)
            .First();

        return (PayrollTaxConfigFacts.From(chosen), conflict);
    }
}
