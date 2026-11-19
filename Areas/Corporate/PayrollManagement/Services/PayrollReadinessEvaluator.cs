using System.Globalization;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// Fakta profil operasional payroll (PrlPayroll) yang dinilai kesiapannya. Tidak membawa nominal gaji. Masa berlaku profil opsional:
/// bila diisi dan penilai diberi tanggal, profil yang baru berlaku di masa depan atau sudah berakhir tidak dianggap operasional.
/// </summary>
public sealed record PayrollProfileFacts(
    bool IsActive,
    bool IsPayrollEligible,
    string? PayrollStatus,
    decimal TaxAmount,
    decimal InsuranceAmount,
    DateTime? EffectiveStartDate = null,
    DateTime? EffectiveEndDate = null);

/// <summary>
/// Penilaian sumber HRD satu orang: penetapan gaji utama yang berlaku dan rekening payroll. Seluruh pemilik datanya HRD.
/// <see cref="Issues"/> hanya memuat kekurangan yang benar-benar ditemukan, bukan pesan generik.
/// </summary>
public sealed record PayrollHrdAssessment(
    bool HasSalary,
    bool SalaryApproved,
    bool SalaryPending,
    bool SalaryConflict,
    string BankStatus,
    bool HrdComplete,
    List<string> Issues);

/// <summary>
/// Penilaian profil operasional internal Payroll satu orang, termasuk status konfigurasi PPh dan iuran yang ditetapkan HRD.
/// <see cref="TaxInsuranceStatus"/> dan <see cref="TaxInsuranceUnset"/> diturunkan dari status konfigurasi eksplisit (PrlTax dan
/// PrlInsurance), bukan dari perbandingan nominal nol.
/// </summary>
public sealed record PayrollProfileAssessment(
    string ProfileStatus,
    string TaxInsuranceStatus,
    bool IsOperational,
    bool TaxInsuranceUnset,
    List<string> Issues,
    List<string> Notes,
    string TaxConfigurationStatus = "",
    string InsuranceConfigurationStatus = "",
    List<string>? TaxIssues = null,
    List<string>? InsuranceIssues = null);

/// <summary>
/// Satu-satunya tempat aturan kelengkapan sumber HRD dan status profil payroll. Dipakai Kelengkapan Data Payroll dan Profil Penggajian
/// (PayrollCompensation) supaya keduanya tidak dapat berselisih tentang hal yang sama. Murni (tanpa akses data, tanpa efek samping).
///
/// Tiga hal dinilai terpisah:
/// (1) kelengkapan sumber HRD (<see cref="AssessHrd"/>): gaji utama berlaku dan disetujui, tidak konflik, rekening payroll
///     terverifikasi;
/// (2) profil operasional internal Payroll (<see cref="AssessProfile"/>): ada atau belum, aktif/eligible/Active, dan apakah tetapan PPh
///     dan iuran sudah ditetapkan;
/// (3) kesiapan memproses, yang disusun masing-masing layar dari (1) dan (2) menurut definisinya sendiri (lihat kontrak layar).
/// </summary>
public static class PayrollReadinessEvaluator
{
    public const string BankNone = "Belum ada";
    public const string BankUnverified = "Belum terverifikasi";
    public const string BankVerified = "Terverifikasi";

    public const string ProfileReady = "Siap";
    public const string ProfileAutoPrepare = "Akan disiapkan otomatis";
    public const string ProfileWaitingHrd = "Menunggu data HRD";
    public const string ProfileNeedsAttention = "Perlu perhatian";

    public const string TaxSet = "Sudah ditetapkan";
    public const string TaxNotSet = "Belum ditetapkan";
    public const string TaxWaitingProfile = "Menunggu profil";

    public const string NoSalaryMessage = "Belum ada penetapan gaji utama di HRD yang berlaku saat ini.";

    public static string BankStatusOf(PayrollHrdBank? bank) =>
        bank is null ? BankNone : bank.HasVerified ? BankVerified : BankUnverified;

    /// <param name="salary">Penetapan gaji utama yang berlaku pada tanggal penilaian; null bila tidak ada.</param>
    /// <param name="bank">Rekening payroll aktif yang berlaku; null bila tidak ada.</param>
    /// <param name="noSalaryMessage">Pengganti pesan "belum ada penetapan" bila pemanggil tahu alasan yang lebih tepat (mis. baru berlaku
    /// mulai tanggal tertentu atau sudah berakhir). Hanya mengubah kalimat, tidak mengubah penilaian.</param>
    public static PayrollHrdAssessment AssessHrd(PayrollHrdSalary? salary, PayrollHrdBank? bank, string? noSalaryMessage = null)
    {
        var issues = new List<string>();

        var hasSalary = salary is not null;
        var approved = salary is not null && salary.HasApproved;
        var pending = salary is not null && salary.HasPending;
        var conflict = salary is not null && salary.ApprovedCount > 1;

        if (salary is null) issues.Add(string.IsNullOrWhiteSpace(noSalaryMessage) ? NoSalaryMessage : noSalaryMessage);
        else if (!approved) issues.Add("Penetapan gaji utama belum disetujui HRD.");
        else if (conflict) issues.Add("Terdapat lebih dari satu penetapan gaji utama yang disetujui HRD dan berlaku bersamaan.");

        var bankStatus = BankStatusOf(bank);
        if (bank is null) issues.Add("Belum ada rekening payroll di HRD.");
        else if (!bank.HasVerified) issues.Add("Rekening payroll belum diverifikasi HRD.");

        return new PayrollHrdAssessment(hasSalary, approved, pending, conflict, bankStatus, issues.Count == 0, issues);
    }

    /// <summary>Tanggal <paramref name="asOf"/> berada di dalam masa berlaku [start, end]; batas kosong berarti tidak dibatasi.</summary>
    public static bool IsEffectiveOn(DateTime? start, DateTime? end, DateTime asOf) =>
        (!start.HasValue || start.Value.Date <= asOf.Date) && (!end.HasValue || end.Value.Date >= asOf.Date);

    /// <summary>
    /// Memilih satu profil dari semua profil payroll yang belum dihapus milik satu orang. Urutan: profil aktif yang berlaku pada
    /// <paramref name="asOf"/> lebih dulu, lalu profil aktif lainnya (masa depan atau sudah berakhir), lalu profil nonaktif; sama-sama
    /// itu, tanggal mulai terbaru. Profil nonaktif tetap dipilih bila hanya itu yang ada (berbeda dari belum dibuat), dan profil masa
    /// depan tidak pernah dipilih mengalahkan profil yang sedang berlaku. <paramref name="activeConflict"/> benar bila lebih dari satu
    /// profil aktif berlaku bersamaan (masalah integritas data; biasanya dicegah indeks unik).
    /// </summary>
    public static TItem? SelectProfile<TItem>(
        IReadOnlyCollection<TItem> candidates,
        Func<TItem, PayrollProfileFacts> factsOf,
        DateTime asOf,
        out bool activeConflict) where TItem : class
    {
        activeConflict = false;
        if (candidates.Count == 0) return null;

        var ranked = candidates.Select(c => (Item: c, Facts: factsOf(c))).ToList();
        bool ActiveNow(PayrollProfileFacts f) => f.IsActive && IsEffectiveOn(f.EffectiveStartDate, f.EffectiveEndDate, asOf);

        activeConflict = ranked.Count(x => ActiveNow(x.Facts)) > 1;
        return ranked
            .OrderBy(x => ActiveNow(x.Facts) ? 0 : x.Facts.IsActive ? 1 : 2)
            .ThenByDescending(x => x.Facts.EffectiveStartDate ?? DateTime.MinValue)
            .First()
            .Item;
    }

    /// <param name="profile">Profil payroll yang belum dihapus; null bila belum ada.</param>
    /// <param name="canAutoPrepare">Penetapan gaji dan rekening payroll sudah ada di HRD, jadi profil dapat disiapkan otomatis.</param>
    /// <param name="taxInsurance">Kesiapan konfigurasi PPh dan iuran yang ditetapkan HRD (PayrollTaxInsuranceRules.AssessReadiness).
    /// Konfigurasi ini milik HRD dan tidak bergantung pada ada atau tidaknya profil payroll; nol bukan penentu kelengkapan.</param>
    /// <param name="asOf">Tanggal penilaian. Bila diberikan, profil yang belum berlaku atau sudah berakhir pada tanggal itu tidak
    /// operasional. Kosong berarti masa berlaku profil tidak dinilai.</param>
    /// <param name="hasActiveConflict">Ada lebih dari satu profil aktif yang berlaku bersamaan untuk orang ini.</param>
    public static PayrollProfileAssessment AssessProfile(
        PayrollProfileFacts? profile, bool canAutoPrepare, PayrollTaxInsuranceReadiness taxInsurance, DateTime? asOf = null,
        bool hasActiveConflict = false)
    {
        var issues = new List<string>();
        var notes = new List<string>();
        var taxStatus = taxInsurance.IsComplete ? TaxSet : TaxNotSet;
        var taxUnset = !taxInsurance.IsComplete;
        var taxIssues = taxInsurance.TaxIssues.ToList();
        var insuranceIssues = taxInsurance.InsuranceIssues.ToList();

        PayrollProfileAssessment Build(string resultProfileStatus, bool resultOperational) => new(
            resultProfileStatus, taxStatus, resultOperational, taxUnset, issues, notes,
            taxInsurance.TaxConfigurationStatus, taxInsurance.InsuranceConfigurationStatus, taxIssues, insuranceIssues);

        if (profile is null)
        {
            notes.AddRange(taxInsurance.Notes);
            if (canAutoPrepare)
            {
                notes.Add("Profil payroll akan disiapkan otomatis dari data HRD saat payroll run dipersiapkan. Tidak perlu impor.");
                return Build(ProfileAutoPrepare, false);
            }

            notes.Add("Profil payroll belum dapat disiapkan otomatis sampai penetapan gaji utama dan rekening payroll tersedia di HRD.");
            return Build(ProfileWaitingHrd, false);
        }

        var operationalByStatus = PayrollProfileRules.IsOperational(profile.IsActive, profile.IsPayrollEligible, profile.PayrollStatus);
        if (!operationalByStatus)
        {
            var status = string.IsNullOrWhiteSpace(profile.PayrollStatus) ? "tidak diketahui" : profile.PayrollStatus.Trim();
            if (!profile.IsActive) issues.Add("Profil payroll tidak aktif.");
            if (!profile.IsPayrollEligible) issues.Add("Profil payroll tidak eligible.");
            if (!string.Equals(profile.PayrollStatus, PayrollProfileRules.ActiveStatus, StringComparison.OrdinalIgnoreCase))
                issues.Add($"Status payroll {status}, bukan Active.");
        }

        // Masa berlaku profil: profil masa depan bukan profil yang sedang berlaku, dan profil yang sudah berakhir juga bukan.
        var notYetEffective = asOf.HasValue && profile.EffectiveStartDate.HasValue && profile.EffectiveStartDate.Value.Date > asOf.Value.Date;
        var alreadyEnded = asOf.HasValue && profile.EffectiveEndDate.HasValue && profile.EffectiveEndDate.Value.Date < asOf.Value.Date;
        if (notYetEffective)
            issues.Add($"Profil payroll baru berlaku mulai {profile.EffectiveStartDate!.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}; belum berlaku saat ini.");
        if (alreadyEnded)
            issues.Add($"Profil payroll sudah berakhir pada {profile.EffectiveEndDate!.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}.");
        if (hasActiveConflict)
            issues.Add("Terdapat lebih dari satu profil payroll aktif yang berlaku bersamaan untuk penerima ini. Perlu diperiksa Payroll.");

        var operational = operationalByStatus && !notYetEffective && !alreadyEnded && !hasActiveConflict;
        string profileStatus;
        if (operational)
        {
            profileStatus = ProfileReady;
        }
        else
        {
            profileStatus = ProfileNeedsAttention;
            notes.Add("Profil tidak diaktifkan otomatis; perlu keputusan Payroll.");
        }

        // Kelengkapan PPh dan iuran dinilai dari konfigurasi eksplisit HRD, bukan dari nominal nol pada profil payroll.
        notes.AddRange(taxInsurance.Notes);
        return Build(profileStatus, operational);
    }
}
