using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// Model baca "Data Penghasilan Pegawai &amp; Dokter" (PAY-BE-03). Populasi BERANGKAT DARI HRD (pegawai dan dokter aktif lewat
/// <see cref="PayrollHrdSourceReader"/>), lalu profil operasional payroll (PrlPayroll), penetapan gaji, rekening, pajak, dan iuran
/// digabungkan secara opsional. Orang yang sudah terdaftar di HRD tetap tampil walaupun profil payroll-nya belum ada; ia ditandai
/// belum disiapkan, bukan disembunyikan dan bukan dipalsukan sebagai Active.
///
/// Tiga hal dinilai terpisah dan tidak boleh dianggap sama: terdaftar di HRD, punya profil payroll operasional, dan siap diproses.
/// Aturan kelengkapan HRD dan status profil dinilai oleh <see cref="PayrollReadinessEvaluator"/>, yang SAMA dengan Kelengkapan Data
/// Payroll, sehingga kedua layar tidak dapat berselisih tentang hal yang sama.
///
/// Layanan ini HANYA MEMBACA. Tidak ada tulis ke database di sini (termasuk tidak membuat PrlPayroll); profil operasional disiapkan
/// otomatis oleh <see cref="PayrollProfileProvisioningService"/> saat payroll run dipersiapkan. Persetujuan penetapan gaji dan
/// verifikasi rekening tetap milik HRD.
///
/// Pengamanan: nominal gaji hanya dikirim bila pemanggil berwenang ViewAmount dan tidak pernah diganti nol bila tidak tersedia;
/// nomor rekening dan NPWP selalu disamarkan.
///
/// Kesiapan memproses (<see cref="PayrollCompensationListItemResponse.IsReady"/>) lebih ketat daripada "Lengkap" pada Kelengkapan Data:
/// profil payroll harus sudah ada dan operasional, serta konfigurasi PPh dan iuran yang ditetapkan HRD harus sah dan berlaku (dinilai
/// eksplisit oleh PayrollTaxInsuranceRules; nominal nol pada profil payroll bukan penentu).
/// </summary>
public sealed class PayrollCompensationService
{
    private const string NotPreparedFilter = "NotPrepared";
    private const string NotPreparedFilterDisplay = "Belum Disiapkan";
    private const string ReadyFilter = "Siap";
    private const string HrdCompleteLabel = "Lengkap";
    private const string HrdIncompleteLabel = "Belum lengkap";
    private const string HrdInactiveLabel = "Tidak aktif di HRD";
    private const string SalaryNone = "Belum Ada Penetapan Gaji";
    private const string SalaryPendingApproval = "Menunggu Persetujuan HRD";
    private const string SalaryApprovedLabel = "Disetujui";
    private const string SalaryNotYetEffective = "Belum Berlaku";
    private const string SalaryEnded = "Sudah Berakhir";
    private const string SalaryConflict = "Konflik Penetapan Gaji";
    private const int MaxPageSize = 100;

    private readonly ApplicationDbContext _dbContext;
    private readonly PayrollHrdSourceReader _hrdSource;
    private readonly PayrollStatutoryPolicyResolver _policyResolver;

    public PayrollCompensationService(
        ApplicationDbContext dbContext, PayrollHrdSourceReader hrdSource, PayrollStatutoryPolicyResolver policyResolver)
    {
        _dbContext = dbContext;
        _hrdSource = hrdSource;
        _policyResolver = policyResolver;
    }

    private sealed record ProfileRow(
        Guid Id,
        string PayrollStatus,
        bool IsActive,
        bool IsPayrollEligible,
        string? PayrollGroupCode,
        string? PayrollNumber,
        string PaymentFrequency,
        DateTime? EffectiveStartDate,
        DateTime? EffectiveEndDate,
        decimal TaxAmount,
        decimal InsuranceAmount,
        decimal TotalAllowance,
        string? Description,
        bool ActiveConflict = false);

    private static PayrollProfileFacts ToFacts(ProfileRow profile) => new(
        profile.IsActive, profile.IsPayrollEligible, profile.PayrollStatus, profile.TaxAmount, profile.InsuranceAmount,
        profile.EffectiveStartDate, profile.EffectiveEndDate);

    private sealed record AssignmentRow(
        Guid Id,
        DateTime EffectiveStartDate,
        DateTime? EffectiveEndDate,
        bool IsPrimary,
        bool IsActive,
        DateTime? ApprovedAt,
        decimal BaseSalary,
        string? SalaryStructureName,
        string? SalaryGradeName);

    /// <summary>Hasil penilaian satu orang; <see cref="Issues"/> memuat setiap penghalang kesiapan yang benar-benar ditemukan.</summary>
    private sealed record Evaluated(
        PayrollHrdPerson Person,
        bool IsActiveInHrd,
        ProfileRow? Profile,
        PayrollHrdSalary? Salary,
        PayrollHrdAssessment Hrd,
        PayrollProfileAssessment ProfileAssessment,
        bool HasTaxProfile,
        List<string> Issues,
        List<string> Notes)
    {
        public bool IsReady => Issues.Count == 0;
    }

    public async Task<PagedResult<PayrollCompensationListItemResponse>> GetPagedAsync(
        PayrollCompensationQuery query, bool includeAmounts, CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);
        var today = DateTime.UtcNow.Date;

        // Penyaringan identitas (jenis, pencarian) dilakukan di database lewat pembaca HRD. Status payroll dan kesiapan bergantung
        // pada penggabungan beberapa tabel (profil, gaji, rekening, pajak), sehingga dinilai di memori atas proyeksi ramping yang
        // jumlahnya dibatasi jumlah SDM rumah sakit; hanya baris satu halaman yang memuat detail tambahan.
        IEnumerable<Evaluated> rows = await LoadEvaluatedAsync(query.PersonType, query.Search, today, cancellationToken);

        rows = ApplyStatusFilter(rows, query.PayrollStatus);

        var readiness = query.Readiness?.Trim();
        if (!string.IsNullOrWhiteSpace(readiness))
        {
            rows = string.Equals(readiness, ReadyFilter, StringComparison.OrdinalIgnoreCase)
                ? rows.Where(r => r.IsReady)
                : rows.Where(r => !r.IsReady);
        }

        var filtered = rows
            .OrderBy(r => r.Person.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(r => r.Person.Number, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var pageRows = filtered.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        var pageIds = pageRows.Select(r => r.Person.WorkforceProfileId).ToList();

        var assignments = await LoadAssignmentsAsync(pageIds, primaryActiveOnly: true, cancellationToken);
        var insuranceIds = await LoadInsuranceIdsAsync(pageIds, cancellationToken);

        var items = pageRows.Select(r => BuildItem(
                new PayrollCompensationListItemResponse(),
                r,
                assignments.GetValueOrDefault(r.Person.WorkforceProfileId) ?? new List<AssignmentRow>(),
                insuranceIds.Contains(r.Person.WorkforceProfileId),
                includeAmounts,
                today))
            .ToList();

        return new PagedResult<PayrollCompensationListItemResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = filtered.Count,
            TotalPage = (int)Math.Ceiling(filtered.Count / (double)pageSize),
            Items = items
        };
    }

    /// <summary>
    /// Ringkasan seluruh populasi HRD (tidak dipengaruhi filter daftar) dari penilaian yang sama dengan daftar, sehingga angka kartu dan
    /// filter daftar tidak dapat berselisih.
    /// </summary>
    public async Task<PayrollCompensationSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var all = await LoadEvaluatedAsync(null, null, DateTime.UtcNow.Date, cancellationToken);
        return new PayrollCompensationSummaryResponse
        {
            TotalPeople = all.Count,
            WithPayrollProfile = all.Count(x => x.Profile is not null),
            ActivePayrollProfiles = all.Count(x => x.ProfileAssessment.IsOperational),
            NotPrepared = all.Count(x => x.Profile is null),
            HrdComplete = all.Count(x => x.Hrd.HrdComplete),
            Ready = all.Count(x => x.IsReady)
        };
    }

    public async Task<PayrollCompensationDetailResponse> GetDetailAsync(
        Guid workforceProfileId, bool includeAmounts, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;

        // Parameter rute adalah WorkforceProfileId (bukan Id profil payroll, Id pegawai/dokter, atau Id penetapan gaji). 404 hanya bila
        // identitas HRD memang tidak ada. Profil payroll yang belum ada BUKAN alasan 404. Pesan tidak memuat data pribadi apa pun.
        var found = await FindPersonAsync(workforceProfileId, cancellationToken)
            ?? throw new KeyNotFoundException("Data pegawai atau dokter tidak ditemukan di HRD.");
        var (person, isActiveInHrd) = found;

        var scope = new List<Guid> { workforceProfileId };
        var salaries = await _hrdSource.LoadSalariesAsync(today, today, cancellationToken, scope);
        var banks = await _hrdSource.LoadBanksAsync(today, cancellationToken, scope);
        var profiles = await LoadProfilesAsync(scope, today, cancellationToken);
        var assignments = (await LoadAssignmentsAsync(scope, primaryActiveOnly: false, cancellationToken))
            .GetValueOrDefault(workforceProfileId) ?? new List<AssignmentRow>();

        var bankRows = await _dbContext.WfpBankAccounts.AsNoTracking()
            .Where(b => !b.IsDelete && b.IsActive && b.WorkforceProfileId == workforceProfileId)
            .Select(b => new
            {
                b.Id,
                b.BankName,
                b.AccountNumber,
                b.AccountHolderName,
                b.IsPayrollAccount,
                b.IsPrimary,
                b.IsVerified,
                b.VerifiedAt
            })
            .ToListAsync(cancellationToken);
        var tax = await _dbContext.PrlTaxes.AsNoTracking()
            .Where(t => !t.IsDelete && t.IsActive && t.WorkforceProfileId == workforceProfileId)
            .OrderByDescending(t => t.EffectiveStartDate)
            .Select(t => new { t.TaxStatus, t.IsNpwpRegistered, t.NpwpNumber })
            .FirstOrDefaultAsync(cancellationToken);
        var taxConfig = (await LoadTaxConfigsAsync(scope, cancellationToken)).GetValueOrDefault(workforceProfileId);
        var insuranceConfig = (await LoadInsuranceConfigsAsync(scope, cancellationToken)).GetValueOrDefault(workforceProfileId);
        var hasInsurance = insuranceConfig is { IsActive: true };
        // Penempatan organisasi primer yang berlaku hari ini (hanya dibaca; kosong bila HRD belum mengisinya).
        var organization = await _dbContext.WfpOrganizationAssignments.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive && x.IsPrimary && x.WorkforceProfileId == workforceProfileId
                && x.EffectiveStartDate <= today && (x.EffectiveEndDate == null || x.EffectiveEndDate >= today))
            .OrderByDescending(x => x.EffectiveStartDate)
            .Select(x => new
            {
                UnitName = x.OrganizationUnit != null ? x.OrganizationUnit.UnitName : null,
                DepartmentName = x.Department != null ? x.Department.DepartmentName : null,
                PositionName = x.Position != null ? x.Position.PositionName : null
            })
            .FirstOrDefaultAsync(cancellationToken);

        salaries.TryGetValue(workforceProfileId, out var salary);
        banks.TryGetValue(workforceProfileId, out var bank);
        profiles.TryGetValue(workforceProfileId, out var profile);

        var availability = await _policyResolver.GetAvailabilityAsync(today, cancellationToken);
        var evaluated = Evaluate(person, isActiveInHrd, profile, salary, bank, taxConfig, insuranceConfig, today, availability);

        var detail = new PayrollCompensationDetailResponse();
        BuildItem(detail, evaluated, assignments, hasInsurance, includeAmounts, today);
        // Informasi operasional hanya dari profil yang benar-benar ada; null (bukan nilai bawaan) bila profil belum dibuat.
        detail.PayrollNumber = profile?.PayrollNumber;
        detail.PaymentFrequency = profile?.PaymentFrequency;
        detail.PayrollProfileEffectiveStartDate = profile?.EffectiveStartDate;
        detail.PayrollProfileEffectiveEndDate = profile?.EffectiveEndDate;
        detail.OrganizationUnitName = organization?.UnitName;
        detail.DepartmentName = organization?.DepartmentName;
        detail.PositionName = organization?.PositionName;
        detail.TaxStatus = tax?.TaxStatus;
        detail.IsNpwpRegistered = tax?.IsNpwpRegistered ?? false;
        detail.NpwpMasked = MaskTail(tax?.NpwpNumber, 5);
        detail.SalaryHistory = assignments.Select(a => new PayrollCompensationSalaryHistoryResponse
        {
            Id = a.Id,
            SalaryStructureName = a.SalaryStructureName,
            SalaryGradeName = a.SalaryGradeName,
            EffectiveStartDate = a.EffectiveStartDate,
            EffectiveEndDate = a.EffectiveEndDate,
            IsPrimary = a.IsPrimary,
            IsActive = a.IsActive,
            IsApproved = a.ApprovedAt.HasValue,
            ApprovedAt = a.ApprovedAt,
            BaseSalary = includeAmounts ? (decimal?)a.BaseSalary : null
        }).ToList();
        detail.BankAccounts = bankRows.Select(b => new PayrollCompensationBankResponse
        {
            Id = b.Id,
            BankName = b.BankName,
            AccountMasked = MaskTail(b.AccountNumber, 4) ?? string.Empty,
            AccountHolderName = b.AccountHolderName,
            IsPayrollAccount = b.IsPayrollAccount,
            IsPrimary = b.IsPrimary,
            IsVerified = b.IsVerified,
            VerifiedAt = b.VerifiedAt
        }).ToList();
        return detail;
    }

    // ------------------------------------------------------------------------------------------------------------------------
    // Pemuatan dan penilaian
    // ------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Populasi HRD aktif (disaring jenis/pencarian di database) dipertemukan dengan penetapan gaji yang berlaku HARI INI, rekening
    /// payroll, profil payroll, dan profil pajak, semuanya proyeksi ramping. Tidak ada tulis.
    /// </summary>
    private async Task<List<Evaluated>> LoadEvaluatedAsync(
        string? personType, string? search, DateTime today, CancellationToken cancellationToken)
    {
        var people = await _hrdSource.LoadPopulationAsync(personType, search, cancellationToken);
        if (people.Count == 0) return new List<Evaluated>();

        // Tanpa filter identitas seluruh tabel kecil dibaca sekali; dengan filter pembacaan dibatasi pada orang yang cocok.
        var hasIdentityFilter = !string.IsNullOrWhiteSpace(personType) || !string.IsNullOrWhiteSpace(search);
        List<Guid>? scope = hasIdentityFilter ? people.Select(p => p.WorkforceProfileId).ToList() : null;

        var salaries = await _hrdSource.LoadSalariesAsync(today, today, cancellationToken, scope);
        var banks = await _hrdSource.LoadBanksAsync(today, cancellationToken, scope);
        var profiles = await LoadProfilesAsync(scope, today, cancellationToken);
        var taxConfigs = await LoadTaxConfigsAsync(scope, cancellationToken);
        var insuranceConfigs = await LoadInsuranceConfigsAsync(scope, cancellationToken);

        var availability = await _policyResolver.GetAvailabilityAsync(today, cancellationToken);
        var result = new List<Evaluated>(people.Count);
        foreach (var person in people)
        {
            salaries.TryGetValue(person.WorkforceProfileId, out var salary);
            banks.TryGetValue(person.WorkforceProfileId, out var bank);
            profiles.TryGetValue(person.WorkforceProfileId, out var profile);
            taxConfigs.TryGetValue(person.WorkforceProfileId, out var taxConfig);
            insuranceConfigs.TryGetValue(person.WorkforceProfileId, out var insuranceConfig);
            result.Add(Evaluate(person, true, profile, salary, bank, taxConfig, insuranceConfig, today, availability));
        }

        return result;
    }

    private static Evaluated Evaluate(
        PayrollHrdPerson person,
        bool isActiveInHrd,
        ProfileRow? profile,
        PayrollHrdSalary? salary,
        PayrollHrdBank? bank,
        PayrollTaxConfigFacts? taxConfig,
        PayrollInsuranceConfigFacts? insuranceConfig,
        DateTime today,
        PayrollPolicyAvailability? availability)
    {
        var hrd = PayrollReadinessEvaluator.AssessHrd(salary, bank);
        // Konfigurasi PPh dan iuran milik HRD dinilai eksplisit (jenis PPh, kategori, skema, kepesertaan, masa berlaku); nominal nol pada
        // profil payroll bukan penentu. Tetapan lama pada profil hanya cadangan bertanda.
        var taxInsurance = PayrollTaxInsuranceRules.AssessReadiness(
            taxConfig, insuranceConfig, profile?.TaxAmount ?? 0m, profile?.InsuranceAmount ?? 0m, today, availability);
        // Profil dinilai pada tanggal hari ini: profil masa depan atau yang sudah berakhir bukan profil yang sedang berlaku, dan
        // beberapa profil aktif yang berlaku bersamaan dilaporkan sebagai masalah integritas data.
        var assessment = PayrollReadinessEvaluator.AssessProfile(
            profile is null ? null : ToFacts(profile),
            PayrollProfileRules.CanAutoPrepare(salary is not null, bank is not null),
            taxInsurance,
            today,
            profile?.ActiveConflict ?? false);
        var hasTaxProfile = taxConfig is { IsActive: true };

        var issues = new List<string>();
        if (!isActiveInHrd) issues.Add("Pegawai atau dokter ini tidak aktif di HRD.");
        issues.AddRange(hrd.Issues);
        if (profile is null) issues.Add("Profil Payroll belum disiapkan secara otomatis.");
        else issues.AddRange(assessment.Issues);
        // Kekurangan konfigurasi yang sebenarnya, bukan pesan generik "keduanya nol": tidak boleh membuat orang tampil siap hanya karena
        // nominal bernilai nol, dan tidak boleh menahan orang yang nol-nya sah.
        issues.AddRange(taxInsurance.TaxIssues);
        issues.AddRange(taxInsurance.InsuranceIssues);

        return new Evaluated(person, isActiveInHrd, profile, salary, hrd, assessment, hasTaxProfile, issues.Distinct().ToList(), assessment.Notes);
    }

    private static IEnumerable<Evaluated> ApplyStatusFilter(IEnumerable<Evaluated> rows, string? status)
    {
        var value = status?.Trim();
        if (string.IsNullOrWhiteSpace(value)) return rows;

        // Nilai virtual: hanya representasi untuk orang yang belum punya profil; tidak pernah disimpan sebagai status payroll.
        if (string.Equals(value, NotPreparedFilter, StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, NotPreparedFilterDisplay, StringComparison.OrdinalIgnoreCase))
            return rows.Where(r => r.Profile is null);

        // Status nyata: hanya profil yang ada dan aktif dengan status yang sama persis, bukan orang yang sekadar aktif di HRD.
        return rows.Where(r => r.Profile is not null
            && r.Profile.IsActive
            && string.Equals(r.Profile.PayrollStatus, value, StringComparison.OrdinalIgnoreCase));
    }

    private static T BuildItem<T>(
        T target,
        Evaluated ev,
        List<AssignmentRow> assignments,
        bool hasInsurance,
        bool includeAmounts,
        DateTime today) where T : PayrollCompensationListItemResponse
    {
        // Penetapan yang BERLAKU hari ini (dipilih pembaca HRD yang sama dengan kelengkapan data). Bila tidak ada, penetapan aktif
        // terbaru hanya ditampilkan tanggalnya sebagai informasi; nominalnya tidak dipakai sebagai gaji saat ini.
        var effective = ev.Salary is null ? null : assignments.FirstOrDefault(a => a.Id == ev.Salary.AssignmentId);
        var display = effective
            ?? assignments.Where(a => a.IsActive && a.IsPrimary).OrderByDescending(a => a.EffectiveStartDate).FirstOrDefault();

        var profile = ev.Profile;
        var hrdComplete = ev.IsActiveInHrd && ev.Hrd.HrdComplete;

        target.WorkforceProfileId = ev.Person.WorkforceProfileId;
        target.PayrollProfileId = profile?.Id;
        target.HasPayrollProfile = profile is not null;
        target.PersonType = ev.Person.Type;
        target.Number = ev.Person.Number;
        target.Name = ev.Person.Name;
        target.PayrollStatus = profile?.PayrollStatus;
        target.IsPayrollEligible = profile?.IsPayrollEligible ?? false;
        target.PayrollGroupCode = profile?.PayrollGroupCode;
        target.PayrollProfileStatus = ev.ProfileAssessment.ProfileStatus;
        target.HrdComplete = hrdComplete;
        target.HrdDataStatus = !ev.IsActiveInHrd ? HrdInactiveLabel : hrdComplete ? HrdCompleteLabel : HrdIncompleteLabel;
        target.TaxInsuranceStatus = ev.ProfileAssessment.TaxInsuranceStatus;
        target.TaxConfigurationStatus = ev.ProfileAssessment.TaxConfigurationStatus;
        target.InsuranceConfigurationStatus = ev.ProfileAssessment.InsuranceConfigurationStatus;
        target.TaxConfigurationIssues = ev.ProfileAssessment.TaxIssues ?? new List<string>();
        target.InsuranceConfigurationIssues = ev.ProfileAssessment.InsuranceIssues ?? new List<string>();

        target.SalaryAssignmentId = display?.Id;
        target.SalaryEffectiveStartDate = display?.EffectiveStartDate;
        target.SalaryEffectiveEndDate = display?.EffectiveEndDate;
        target.SalaryApproved = display?.ApprovedAt.HasValue ?? false;
        target.SalaryApprovedAt = display?.ApprovedAt;

        target.AmountsVisible = includeAmounts;
        // Nominal hanya dari penetapan gaji HRD yang berlaku; null (bukan nol) bila tidak berwenang atau tidak tersedia.
        target.BaseSalary = includeAmounts && effective is not null ? (decimal?)effective.BaseSalary : null;
        // Profil yang disiapkan otomatis membawa nol sebagai penanda kompatibilitas, bukan tunjangan sebenarnya.
        var allowanceIsPlaceholder = profile is not null
            && profile.TotalAllowance == 0m
            && string.Equals(profile.Description, PayrollProfileRules.AutoProvisionedDescription, StringComparison.Ordinal);
        target.TotalAllowance = includeAmounts && profile is not null && !allowanceIsPlaceholder
            ? (decimal?)profile.TotalAllowance
            : null;

        target.BankStatus = ev.Hrd.BankStatus;
        target.HasTaxProfile = ev.HasTaxProfile;
        target.HasInsuranceProfile = hasInsurance;

        target.OperationalProfileReady = ev.ProfileAssessment.IsOperational;

        var issues = new List<string>(ev.Issues);
        string salaryStatus;
        if (ev.Salary is null)
        {
            // Penilaian tidak berubah; hanya status dan kalimatnya dibuat tepat (belum berlaku atau sudah berakhir) bila datanya diketahui.
            var missing = ClassifyMissingSalary(assignments, today);
            salaryStatus = missing.Status;
            var index = issues.IndexOf(PayrollReadinessEvaluator.NoSalaryMessage);
            if (missing.Message is not null && index >= 0) issues[index] = missing.Message;
        }
        else if (!ev.Hrd.SalaryApproved)
        {
            salaryStatus = SalaryPendingApproval;
        }
        else
        {
            salaryStatus = ev.Hrd.SalaryConflict ? SalaryConflict : SalaryApprovedLabel;
        }

        target.SalaryStatus = salaryStatus;
        target.Issues = issues;
        target.Notes = new List<string>(ev.Notes);
        target.IsReady = ev.IsReady;
        return target;
    }

    /// <summary>
    /// Alasan tidak ada penetapan gaji utama yang berlaku hari ini: tidak pernah ada, baru berlaku di masa depan, atau sudah berakhir.
    /// Penetapan masa depan tidak pernah dipakai untuk menyatakan penerima siap hari ini.
    /// </summary>
    private static (string Status, string? Message) ClassifyMissingSalary(List<AssignmentRow> assignments, DateTime today)
    {
        var primaries = assignments.Where(a => a.IsActive && a.IsPrimary).ToList();
        if (primaries.Count == 0) return (SalaryNone, null);

        var upcoming = primaries
            .Where(a => a.EffectiveStartDate.Date > today)
            .OrderBy(a => a.EffectiveStartDate)
            .FirstOrDefault();
        if (upcoming is not null)
            return (
                SalaryNotYetEffective,
                "Penetapan gaji utama di HRD baru berlaku mulai "
                + upcoming.EffectiveStartDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
                + "; belum berlaku saat ini.");

        var expired = primaries
            .Where(a => a.EffectiveEndDate.HasValue && a.EffectiveEndDate.Value.Date < today)
            .OrderByDescending(a => a.EffectiveEndDate)
            .FirstOrDefault();
        if (expired is not null && expired.EffectiveEndDate.HasValue)
            return (
                SalaryEnded,
                "Penetapan gaji utama di HRD sudah berakhir pada "
                + expired.EffectiveEndDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
                + "; belum ada penggantinya.");

        return (SalaryNone, null);
    }

    // ------------------------------------------------------------------------------------------------------------------------
    // Pembacaan proyeksi ramping
    // ------------------------------------------------------------------------------------------------------------------------

    private async Task<Dictionary<Guid, ProfileRow>> LoadProfilesAsync(
        IReadOnlyCollection<Guid>? scope, DateTime today, CancellationToken cancellationToken)
    {
        // Semua profil yang belum dihapus dibaca, termasuk yang nonaktif: profil nonaktif berbeda dari profil yang belum pernah dibuat.
        var query = _dbContext.PrlPayrolls.AsNoTracking().Where(x => !x.IsDelete);
        if (scope is not null) query = query.Where(x => scope.Contains(x.WorkforceProfileId));

        var rows = await query
            .Select(x => new
            {
                x.Id,
                x.WorkforceProfileId,
                x.PayrollStatus,
                x.IsActive,
                x.IsPayrollEligible,
                x.PayrollGroupCode,
                x.PayrollNumber,
                x.PaymentFrequency,
                x.EffectiveStartDate,
                x.EffectiveEndDate,
                x.TaxAmount,
                x.InsuranceAmount,
                x.TotalAllowance,
                x.Description
            })
            .ToListAsync(cancellationToken);

        // Indeks unik WorkforceProfileId (IsDelete = false) menjamin paling banyak satu profil per orang. Bila tetap ada beberapa,
        // pemilihannya memakai aturan yang sama dengan Kelengkapan Data (profil aktif yang sedang berlaku lebih dulu, profil masa depan
        // tidak pernah mengalahkan yang sedang berlaku) dan konfliknya dilaporkan sebagai masalah integritas data.
        return rows
            .GroupBy(x => x.WorkforceProfileId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var candidates = g
                        .Select(p => new ProfileRow(
                            p.Id, p.PayrollStatus, p.IsActive, p.IsPayrollEligible, p.PayrollGroupCode, p.PayrollNumber,
                            p.PaymentFrequency, p.EffectiveStartDate, p.EffectiveEndDate, p.TaxAmount, p.InsuranceAmount,
                            p.TotalAllowance, p.Description))
                        .ToList();
                    var chosen = PayrollReadinessEvaluator.SelectProfile(candidates, ToFacts, today, out var conflict)!;
                    return chosen with { ActiveConflict = conflict };
                });
    }

    /// <summary>
    /// Konfigurasi PPh HRD per orang (proyeksi ramping tanpa NPWP). Profil nonaktif ikut dimuat supaya statusnya dinilai eksplisit,
    /// bukan dianggap tidak ada. Yang aktif dan terbaru dipilih bila tetap ada beberapa.
    /// </summary>
    private async Task<Dictionary<Guid, PayrollTaxConfigFacts>> LoadTaxConfigsAsync(
        IReadOnlyCollection<Guid>? scope, CancellationToken cancellationToken)
    {
        var query = _dbContext.PrlTaxes.AsNoTracking().Where(t => !t.IsDelete);
        if (scope is not null) query = query.Where(t => scope.Contains(t.WorkforceProfileId));
        return (await query.ToListAsync(cancellationToken))
            .GroupBy(t => t.WorkforceProfileId)
            .ToDictionary(
                g => g.Key,
                g => PayrollTaxConfigFacts.From(g.OrderByDescending(t => t.IsActive).ThenByDescending(t => t.EffectiveStartDate).First()));
    }

    /// <summary>Konfigurasi iuran HRD per orang; aturan pemilihan sama dengan konfigurasi PPh.</summary>
    private async Task<Dictionary<Guid, PayrollInsuranceConfigFacts>> LoadInsuranceConfigsAsync(
        IReadOnlyCollection<Guid>? scope, CancellationToken cancellationToken)
    {
        var query = _dbContext.PrlInsurances.AsNoTracking().Where(t => !t.IsDelete);
        if (scope is not null) query = query.Where(t => scope.Contains(t.WorkforceProfileId));
        return (await query.ToListAsync(cancellationToken))
            .GroupBy(t => t.WorkforceProfileId)
            .ToDictionary(
                g => g.Key,
                g => PayrollInsuranceConfigFacts.From(g.OrderByDescending(t => t.IsActive).ThenByDescending(t => t.EffectiveStartDate).First()));
    }

    private async Task<HashSet<Guid>> LoadInsuranceIdsAsync(IReadOnlyCollection<Guid> scope, CancellationToken cancellationToken)
    {
        if (scope.Count == 0) return new HashSet<Guid>();
        return (await _dbContext.PrlInsurances.AsNoTracking()
                .Where(t => !t.IsDelete && t.IsActive && scope.Contains(t.WorkforceProfileId))
                .Select(t => t.WorkforceProfileId)
                .Distinct()
                .ToListAsync(cancellationToken))
            .ToHashSet();
    }

    private async Task<Dictionary<Guid, List<AssignmentRow>>> LoadAssignmentsAsync(
        IReadOnlyCollection<Guid> scope, bool primaryActiveOnly, CancellationToken cancellationToken)
    {
        if (scope.Count == 0) return new Dictionary<Guid, List<AssignmentRow>>();

        var query = _dbContext.WfpSalaryAssignments.AsNoTracking()
            .Where(a => !a.IsDelete && scope.Contains(a.WorkforceProfileId));
        if (primaryActiveOnly) query = query.Where(a => a.IsActive && a.IsPrimary);

        var rows = await query
            .OrderByDescending(a => a.EffectiveStartDate)
            .Select(a => new
            {
                a.Id,
                a.WorkforceProfileId,
                a.EffectiveStartDate,
                a.EffectiveEndDate,
                a.IsPrimary,
                a.IsActive,
                a.ApprovedAt,
                a.BaseSalary,
                SalaryStructureName = a.SalaryStructure != null ? a.SalaryStructure.SalaryStructureName : null,
                SalaryGradeName = a.SalaryGrade != null ? a.SalaryGrade.SalaryGradeName : null
            })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(a => a.WorkforceProfileId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(a => new AssignmentRow(
                        a.Id, a.EffectiveStartDate, a.EffectiveEndDate, a.IsPrimary, a.IsActive, a.ApprovedAt,
                        a.BaseSalary, a.SalaryStructureName, a.SalaryGradeName))
                    .ToList());
    }

    /// <summary>
    /// Identitas satu orang di HRD (pegawai atau dokter yang belum dihapus). Orang yang aktif didahulukan; orang yang sudah tidak aktif
    /// tetap ditemukan (detail dapat dibuka) tetapi dilaporkan tidak aktif.
    /// </summary>
    private async Task<(PayrollHrdPerson Person, bool IsActive)?> FindPersonAsync(
        Guid workforceProfileId, CancellationToken cancellationToken)
    {
        // Identitas utama adalah WorkforceProfileId: profil workforce harus ada dan belum dihapus, lalu dicocokkan ke pegawai atau dokter.
        // Id tabel lain (profil payroll, pegawai, dokter, penetapan gaji) sengaja tidak diterima sebagai pengganti.
        var workforceProfileExists = await _dbContext.MstWorkforceProfiles.AsNoTracking()
            .AnyAsync(x => x.Id == workforceProfileId && !x.IsDelete, cancellationToken);
        if (!workforceProfileExists) return null;

        var employee = await _dbContext.MstEmployees.AsNoTracking()
            .Where(x => !x.IsDelete && x.WorkforceProfileId == workforceProfileId)
            .OrderByDescending(x => x.IsActive)
            .Select(x => new { x.Id, x.EmployeeNumber, x.FullName, x.IsActive })
            .FirstOrDefaultAsync(cancellationToken);
        var doctor = await _dbContext.MstDoctors.AsNoTracking()
            .Where(x => !x.IsDelete && x.WorkforceProfileId == workforceProfileId)
            .OrderByDescending(x => x.IsActive)
            .Select(x => new { x.DoctorNumber, x.FullName, x.IsActive })
            .FirstOrDefaultAsync(cancellationToken);

        if (employee is not null && (employee.IsActive || doctor is null || !doctor.IsActive))
            return (new PayrollHrdPerson(workforceProfileId, employee.Id, "Karyawan", employee.EmployeeNumber, employee.FullName), employee.IsActive);
        if (doctor is not null)
            return (new PayrollHrdPerson(workforceProfileId, null, "Dokter", doctor.DoctorNumber, doctor.FullName), doctor.IsActive);
        return null;
    }

    private static string? MaskTail(string? value, int visible)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var text = value.Trim();
        return text.Length <= visible ? new string('*', text.Length) : new string('*', text.Length - visible) + text[^visible..];
    }
}
