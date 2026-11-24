namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Services;

/// <summary>Satu kategori komponen penggajian operasional beserta kelompok teknis yang diturunkan darinya.</summary>
public sealed record PayrollComponentCategoryCatalogEntry(
    string Key,
    string Name,
    string ComponentGroup,
    bool AffectsGrossPay,
    bool AffectsTaxableIncome,
    bool AffectsTakeHomePay,
    int SortOrder,
    string Description);

/// <summary>
/// Katalog lima Kategori Komponen Penggajian operasional (PAY-BE-FIX-023): Gaji Pokok, Tunjangan, Insentif, Potongan, dan Bonus.
/// Satu-satunya tempat yang memetakan kategori ke <c>ComponentGroup</c> teknis (Earning/Deduction); pengguna dan frontend tidak pernah
/// memilih atau menentukan kelompok itu.
///
/// Identitas kategori: tabel <c>MstPayrollComponentCategory</c> belum punya kolom kunci stabil, sehingga katalog dikenali lewat NAMA
/// KANONIK (dinormalkan: spasi dirapikan, tanpa beda huruf besar/kecil). Agar label tidak melenceng, kategori yang cocok dengan katalog
/// namanya dikunci (tidak dapat diubah) dan kategori lama yang bukan katalog tidak boleh diganti namanya menjadi nama katalog. Kunci
/// identitas permanen (kolom <c>CategoryKey</c>) adalah BLOCKED_BY_SCHEMA dan tidak dibuat di sini.
///
/// Kategori di luar katalog (legacy, termasuk kelompok EmployerContribution/Information) tetap dibaca dan dipakai komponen yang sudah
/// ada; hanya tidak boleh dipilih untuk komponen baru. Katalog tidak menentukan kapan komponen dibayar atau dihitung.
/// </summary>
public static class PayrollComponentCategoryCatalog
{
    public const string GroupEarning = "Earning";
    public const string GroupDeduction = "Deduction";
    public const string GroupEmployerContribution = "EmployerContribution";
    public const string GroupInformation = "Information";

    public static class Keys
    {
        public const string BasicSalary = "BASIC_SALARY";
        public const string Allowance = "ALLOWANCE";
        public const string Incentive = "INCENTIVE";
        public const string Deduction = "DEDUCTION";
        public const string Bonus = "BONUS";
    }

    // Bendera dampak hanyalah nilai awal formulir; pengguna dapat mengubahnya. Mesin payroll belum membacanya (PAY-BE-FIX-023 bagian 8).
    private static readonly PayrollComponentCategoryCatalogEntry[] EntriesInternal =
    {
        new(Keys.BasicSalary, "Gaji Pokok", GroupEarning, true, true, true, 10, "Komponen penghasilan dasar pegawai."),
        new(Keys.Allowance, "Tunjangan", GroupEarning, true, true, true, 20, "Komponen tunjangan rutin maupun tunjangan lain yang memenuhi kebijakan."),
        new(Keys.Incentive, "Insentif", GroupEarning, true, true, true, 30, "Komponen penghasilan berbasis insentif."),
        new(Keys.Deduction, "Potongan", GroupDeduction, false, false, true, 40, "Komponen pengurang penghasilan yang sah."),
        new(Keys.Bonus, "Bonus", GroupEarning, true, true, true, 50, "Komponen bonus sesuai kebijakan dan periode pembayaran.")
    };

    public static IReadOnlyList<PayrollComponentCategoryCatalogEntry> All => EntriesInternal;

    /// <summary>Merapikan spasi di tepi dan di dalam nama agar perbandingan tidak bergantung pada spasi berlebih.</summary>
    public static string NormalizeName(string? name) =>
        string.Join(' ', (name ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    public static bool NamesMatch(string? left, string? right) =>
        string.Equals(NormalizeName(left), NormalizeName(right), StringComparison.OrdinalIgnoreCase);

    public static PayrollComponentCategoryCatalogEntry? FindByName(string? name)
    {
        var normalized = NormalizeName(name);
        if (normalized.Length == 0) return null;
        return EntriesInternal.FirstOrDefault(e => string.Equals(e.Name, normalized, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Kategori operasional = namanya katalog DAN kelompok tersimpannya sama dengan kelompok katalog. Kategori bernama katalog tetapi
    /// berkelompok lain adalah data lama yang bermasalah: dilaporkan, tidak diubah, dan tidak ditawarkan untuk komponen baru.
    /// </summary>
    public static bool IsOperational(string? name, string? storedComponentGroup)
    {
        var entry = FindByName(name);
        return entry is not null
            && string.Equals(entry.ComponentGroup, (storedComponentGroup ?? string.Empty).Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
