using System.Globalization;
using System.Text.Json;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>Satu baris tabel PTKP pada kebijakan: status, nilai setahun, dan kategori TER yang dipetakan kebijakan.</summary>
public sealed record PtkpEntry(string Status, decimal AnnualAmount, string TerCategory);

/// <summary>Satu lapisan tabel TER. Batas bawah tidak termasuk (kecuali 0), batas atas termasuk; batas atas kosong = tanpa batas.</summary>
public sealed record TerRateEntry(string Category, string PeriodKind, decimal LowerBound, decimal? UpperBound, decimal RatePercent);

/// <summary>Satu lapisan tarif progresif Pasal 17.</summary>
public sealed record TaxBracketEntry(decimal LowerBound, decimal? UpperBound, decimal RatePercent);

/// <summary>
/// Pandangan baca kebijakan tarif PPh yang disahkan (PrlTaxPolicy berstatus APPROVED). Tidak memuat angka regulasi bawaan: semua nilai
/// berasal dari kebijakan yang dimasukkan HRD dan disahkan Finance.
/// </summary>
public sealed record PayrollTaxPolicy(
    Guid Id,
    string Version,
    string LegalBasis,
    DateTime EffectiveStartDate,
    DateTime? EffectiveEndDate,
    decimal PositionCostRatePercent,
    decimal PositionCostMonthlyCap,
    decimal PositionCostAnnualCap,
    decimal NoNpwpSurchargePercent,
    decimal PkpRoundingUnit,
    bool RoundTaxToWholeRupiah,
    IReadOnlyList<string> EmployerPremiumPrograms,
    IReadOnlyList<string> PensionDeductionPrograms,
    decimal? NonEmployeeBasisPercent,
    string? NonEmployeeMethod,
    decimal? Pph26RatePercent,
    IReadOnlyList<PtkpEntry> Ptkp,
    IReadOnlyList<TerRateEntry> TerRates,
    IReadOnlyList<TaxBracketEntry> Brackets)
{
    public static PayrollTaxPolicy FromEntity(PrlTaxPolicy e) => new(
        e.Id, e.PolicyVersion, e.LegalBasis, e.EffectiveStartDate, e.EffectiveEndDate,
        e.PositionCostRatePercent, e.PositionCostMonthlyCap, e.PositionCostAnnualCap, e.NoNpwpSurchargePercent, e.PkpRoundingUnit,
        e.RoundTaxToWholeRupiah,
        PayrollTaxPolicyEngine.ParseProgramList(e.EmployerPremiumPrograms), PayrollTaxPolicyEngine.ParseProgramList(e.PensionDeductionPrograms),
        e.NonEmployeeBasisPercent, e.NonEmployeeMethod, e.Pph26RatePercent,
        PayrollTaxPolicyEngine.ParseTable<PtkpEntry>(e.PtkpJson),
        PayrollTaxPolicyEngine.ParseTable<TerRateEntry>(e.TerRatesJson),
        PayrollTaxPolicyEngine.ParseTable<TaxBracketEntry>(e.BracketsJson));
}

/// <summary>
/// Akumulasi penghasilan dan PPh tahun berjalan sebelum periode ini: gabungan snapshot payroll run yang sudah disahkan dan saldo awal
/// (Opening Tax Balance) yang sudah disetujui. <see cref="PriorPeriodCount"/> adalah jumlah MASA PAJAK yang tercakup (bukan jumlah
/// baris sumber). Hanya diperlukan pada masa pajak terakhir dan skema kumulatif bukan pegawai; TER bulanan tidak memerlukannya.
/// </summary>
public sealed record PayrollTaxYtd(
    int PriorPeriodCount,
    decimal PriorTaxableGross,
    decimal PriorEmployerPremium,
    decimal PriorPension,
    decimal PriorTaxWithheld,
    bool IsComplete,
    IReadOnlyList<string> Issues)
{
    public static PayrollTaxYtd Empty { get; } = new(0, 0m, 0m, 0m, 0m, true, Array.Empty<string>());

    /// <summary>Bulan (1..12) yang tercakup, dari snapshot run disahkan dan saldo awal. Kosong pada keadaan tanpa penelusuran.</summary>
    public IReadOnlyList<int> CoveredMonths { get; init; } = Array.Empty<int>();

    /// <summary>Bulan yang tercakup oleh saldo awal yang disetujui (bagian dari <see cref="CoveredMonths"/>).</summary>
    public IReadOnlyList<int> OpeningBalanceMonths { get; init; } = Array.Empty<int>();

    /// <summary>Bulan yang seharusnya ada (sejak mulai bekerja sampai sebelum masa pajak ini) tetapi belum tercakup sumber mana pun.</summary>
    public IReadOnlyList<int> MissingMonths { get; init; } = Array.Empty<int>();

    /// <summary>False bila penyimpanan saldo awal belum tersedia: ketiadaan saldo awal bukan berarti saldo nol yang terverifikasi.</summary>
    public bool OpeningBalanceFeatureAvailable { get; init; }

    /// <summary>
    /// PAY-BE-FIX-030: true HANYA bila satu-satunya alasan akumulasi tidak lengkap adalah bulan histori yang belum tercakup run disahkan
    /// maupun saldo awal yang disetujui. Bernilai false bila ada alasan lain (saldo awal di luar rentang, cakupan beririsan, run lama tanpa
    /// rincian akumulasi): hal-hal itu berisiko menghitung pajak dua kali dan tidak boleh dilonggarkan.
    /// </summary>
    public bool HistoryGapOnly { get; init; }
}

/// <summary>Kode klasifikasi penahan PPh yang terstruktur (PAY-BE-FIX-030). Keputusan kelayakan tidak boleh bergantung pada teks pesan.</summary>
public static class PayrollTaxBlockCodes
{
    /// <summary>Histori PPh tahunan Januari sampai bulan sebelum masa pajak terakhir belum tercakup run disahkan atau saldo awal disetujui.</summary>
    public const string YtdHistoryIncomplete = "TAX_YTD_HISTORY_INCOMPLETE";
}

/// <summary>
/// PAY-BE-FIX-032: status PPh per penerima pada run. FINAL_VALID = hasil resmi yang sah (termasuk VALID_ZERO); PROVISIONAL_ESTIMATE = hanya
/// ada estimasi proksi; INCOMPLETE_NO_ESTIMATE = tidak ada angka yang dapat dipercaya (BUKAN Rp0); NOT_APPLICABLE = pengecualian yang sah.
/// </summary>
public static class PayrollEmployeeTaxStatuses
{
    public const string FinalValid = "FINAL_VALID";
    public const string ProvisionalEstimate = "PROVISIONAL_ESTIMATE";
    public const string IncompleteNoEstimate = "INCOMPLETE_NO_ESTIMATE";
    public const string NotApplicable = "NOT_APPLICABLE";
}

/// <summary>Status kefinalan pajak pada run. BELUM_FINAL = masih memuat PPh sementara, tidak boleh disahkan.</summary>
public static class PayrollTaxFinality
{
    public const string Final = "FINAL";
    public const string NotFinal = "BELUM_FINAL";
}

/// <summary>
/// PPh SEMENTARA (estimasi) satu penerima. Dipisahkan dari nominal PPh resmi: <see cref="Amount"/> tidak pernah ditulis ke EmployeeAmount
/// snapshot pajak, tidak menjadi TotalTax run, dan bukan dasar pemotongan, pelaporan, ataupun Finance. <see cref="IsAvailable"/> false
/// berarti "Estimasi Tidak Tersedia" (bukan Rp0).
/// </summary>
public sealed record PayrollProvisionalTaxEstimate(
    bool IsAvailable,
    decimal? Amount,
    string Method,
    string Treatment,
    string? PolicyVersion,
    string? PolicyLegalBasis,
    decimal? BasisGross,
    string? TerCategory,
    decimal? TerRatePercent,
    decimal NoNpwpSurchargePercent,
    string TaxFinality,
    string Explanation,
    string? UnavailableReason)
{
    /// <summary>Estimasi memakai TER bulanan atas penghasilan periode berjalan sebagai proksi pengujian.</summary>
    public const string MethodTerMonthlyProxy = "TER_MONTHLY_PROXY";

    /// <summary>Keterangan wajib: nilai ini bukan hasil rekonsiliasi akhir tahun dan bukan pajak final.</summary>
    public const string ExplanationText =
        "Estimasi PPh sementara berdasarkan penghasilan periode berjalan. Bukan rekonsiliasi PPh 21 akhir tahun dan tidak dapat digunakan sebagai pajak final.";
}

/// <summary>Masukan perhitungan PPh satu penerima pada satu masa pajak. Semua nominal berasal dari komponen payroll dan snapshot, bukan layar.</summary>
public sealed record PayrollTaxInput(
    string Article,
    string Category,
    string? PtkpStatus,
    bool HasNpwp,
    int TaxYear,
    int TaxMonth,
    bool IsLastTaxPeriod,
    decimal TaxableGross,
    decimal EmployerPremium,
    decimal EmployeePension,
    PayrollTaxYtd Ytd,
    bool HasPreviousEmployer,
    decimal? TreatyRatePercent,
    string? TreatyBasis);

/// <summary>
/// Hasil hitung PPh sebelum perlakuan beban (Gross/Net). <see cref="Issues"/> tidak kosong berarti perhitungan tidak sah dan
/// <see cref="Amount"/> tidak boleh dipakai (penahan, bukan Rp0).
/// </summary>
public sealed record PayrollTaxComputation(decimal Amount, IReadOnlyList<string> Issues, IReadOnlyDictionary<string, object?> Detail)
{
    public bool IsValid => Issues.Count == 0;

    /// <summary>Kode klasifikasi penahan (<see cref="PayrollTaxBlockCodes"/>); null bila tidak terklasifikasi atau hasil sah.</summary>
    public string? BlockCode { get; init; }

    public static PayrollTaxComputation Blocked(string issue, IReadOnlyDictionary<string, object?>? detail = null) =>
        new(0m, new[] { issue }, detail ?? new Dictionary<string, object?>());

    public static PayrollTaxComputation Blocked(IReadOnlyList<string> issues, IReadOnlyDictionary<string, object?>? detail = null) =>
        new(0m, issues, detail ?? new Dictionary<string, object?>());
}

/// <summary>
/// Mesin PPh yang murni dan tanpa angka regulasi: seluruh tarif, PTKP, tabel TER, dan lapisan Pasal 17 dibaca dari
/// <see cref="PayrollTaxPolicy"/> yang disahkan. Menghitung (a) PPh 21 pegawai tetap: TER pada masa pajak biasa dan perhitungan
/// tahunan Pasal 17 dikurangi PPh yang sudah dipotong pada masa pajak terakhir, (b) PPh 21 bukan pegawai (tenaga ahli) dengan dasar
/// pengenaan dan cara penerapan tarif dari kebijakan, dan (c) PPh 26. Kombinasi yang tidak punya formula yang sah (pegawai tidak tetap,
/// pemberi kerja sebelumnya pada masa pajak terakhir, kebijakan tanpa tabel yang dibutuhkan) menghasilkan penahan, bukan Rp0.
/// </summary>
public static class PayrollTaxPolicyEngine
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    // ------------------------------------------------------------------------------------------------------------------------
    // Pembacaan isi kebijakan
    // ------------------------------------------------------------------------------------------------------------------------

    /// <summary>Tabel JSON pada kebijakan; kosong atau rusak menjadi daftar kosong (kebijakan tidak lengkap terdeteksi saat diajukan).</summary>
    public static IReadOnlyList<T> ParseTable<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return Array.Empty<T>();
        try
        {
            return JsonSerializer.Deserialize<List<T>>(json, JsonOptions) ?? new List<T>();
        }
        catch (JsonException)
        {
            return Array.Empty<T>();
        }
    }

    /// <summary>True bila JSON dapat dibaca sebagai daftar bertipe T (untuk validasi tulis).</summary>
    public static bool TryParseTable<T>(string? json, out IReadOnlyList<T> rows)
    {
        rows = Array.Empty<T>();
        if (string.IsNullOrWhiteSpace(json)) return true;
        try
        {
            var parsed = JsonSerializer.Deserialize<List<T>>(json, JsonOptions);
            if (parsed is null || parsed.Any(x => x is null)) return false;
            rows = parsed;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    public static string SerializeTable<T>(IEnumerable<T> rows) => JsonSerializer.Serialize(rows, JsonOptions);

    /// <summary>Daftar kode dari teks dipisah koma/titik koma/spasi; huruf besar, unik. Kosong menjadi daftar kosong.</summary>
    public static IReadOnlyList<string> ParseProgramList(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => x.ToUpperInvariant())
                .Distinct()
                .ToList();

    // ------------------------------------------------------------------------------------------------------------------------
    // Validasi isi kebijakan (dipanggil layanan kebijakan saat diajukan)
    // ------------------------------------------------------------------------------------------------------------------------

    private static readonly string[] ProgramCodes = { "KES", "JHT", "JP", "JKK", "JKM", "PRIVATE" };
    private static readonly string[] PensionProgramCodes = { "JHT", "JP" };

    /// <summary>Kelengkapan dan konsistensi isi kebijakan PPh. Kosong berarti kebijakan lengkap dan dapat diajukan.</summary>
    public static List<string> ValidateCompleteness(PayrollTaxPolicy policy)
    {
        var errors = new List<string>();

        if (policy.PositionCostRatePercent <= 0m || policy.PositionCostRatePercent > 100m)
            errors.Add("Persen biaya jabatan wajib diisi (lebih dari 0 dan paling banyak 100).");
        if (policy.PositionCostMonthlyCap <= 0m) errors.Add("Batas biaya jabatan per bulan wajib diisi.");
        if (policy.PositionCostAnnualCap <= 0m) errors.Add("Batas biaya jabatan setahun wajib diisi.");
        if (policy.NoNpwpSurchargePercent < 0m) errors.Add("Tambahan persen tanpa NPWP tidak boleh negatif.");
        if (policy.PkpRoundingUnit <= 0m) errors.Add("Kelipatan pembulatan PKP wajib diisi (lebih dari 0).");

        if (policy.EmployerPremiumPrograms.Any(x => !ProgramCodes.Contains(x)))
            errors.Add("Program premi pemberi kerja yang termasuk bruto tidak valid. Gunakan KES, JHT, JP, JKK, JKM, PRIVATE.");
        if (policy.PensionDeductionPrograms.Any(x => !PensionProgramCodes.Contains(x)))
            errors.Add("Program iuran pensiun pengurang hanya boleh JHT dan/atau JP.");

        // PTKP
        if (policy.Ptkp.Count == 0) errors.Add("Tabel PTKP belum diisi.");
        foreach (var duplicate in policy.Ptkp.GroupBy(x => x.Status.Trim().ToUpperInvariant()).Where(g => g.Count() > 1))
            errors.Add($"Status PTKP {duplicate.Key} muncul lebih dari satu kali.");
        foreach (var row in policy.Ptkp)
        {
            if (string.IsNullOrWhiteSpace(row.Status)) errors.Add("Ada baris PTKP tanpa status.");
            if (row.AnnualAmount < 0m) errors.Add($"Nilai PTKP {row.Status} tidak boleh negatif.");
            if (!TaxTerCategories.All.Contains(row.TerCategory))
                errors.Add($"Kategori TER untuk PTKP {row.Status} tidak valid. Gunakan A, B, atau C.");
        }

        // TER bulanan wajib untuk setiap kategori yang dipetakan; TER harian opsional tetapi bila ada harus utuh.
        foreach (var category in policy.Ptkp.Select(x => x.TerCategory).Where(x => TaxTerCategories.All.Contains(x)).Distinct())
        {
            if (!policy.TerRates.Any(r => r.Category == category && r.PeriodKind == TaxTerPeriodKinds.Monthly))
                errors.Add($"Tabel TER bulanan kategori {category} belum diisi.");
        }
        foreach (var group in policy.TerRates.GroupBy(r => (r.Category, r.PeriodKind)))
        {
            if (!TaxTerCategories.All.Contains(group.Key.Category) || !TaxTerPeriodKinds.All.Contains(group.Key.PeriodKind))
            {
                errors.Add($"Tabel TER memuat kategori/jenis periode tidak valid ({group.Key.Category}/{group.Key.PeriodKind}).");
                continue;
            }
            errors.AddRange(ValidateBands(
                group.OrderBy(r => r.LowerBound).Select(r => (r.LowerBound, r.UpperBound, r.RatePercent)).ToList(),
                $"Tabel TER {group.Key.Category} {group.Key.PeriodKind}"));
        }

        // Pasal 17
        if (policy.Brackets.Count == 0) errors.Add("Lapisan tarif Pasal 17 belum diisi.");
        else errors.AddRange(ValidateBands(
            policy.Brackets.OrderBy(b => b.LowerBound).Select(b => (b.LowerBound, b.UpperBound, b.RatePercent)).ToList(), "Lapisan tarif Pasal 17"));

        // Bukan pegawai dan PPh 26: bersifat opsional per kebijakan, tetapi bila diisi harus lengkap.
        if (policy.NonEmployeeBasisPercent.HasValue || !string.IsNullOrWhiteSpace(policy.NonEmployeeMethod))
        {
            if (!policy.NonEmployeeBasisPercent.HasValue || policy.NonEmployeeBasisPercent.Value <= 0m || policy.NonEmployeeBasisPercent.Value > 100m)
                errors.Add("Persen dasar pengenaan bukan pegawai harus lebih dari 0 dan paling banyak 100.");
            if (string.IsNullOrWhiteSpace(policy.NonEmployeeMethod) || !TaxNonEmployeeMethods.All.Contains(policy.NonEmployeeMethod))
                errors.Add("Cara penerapan tarif bukan pegawai wajib dipilih (per pembayaran atau kumulatif).");
        }
        if (policy.Pph26RatePercent.HasValue && (policy.Pph26RatePercent.Value <= 0m || policy.Pph26RatePercent.Value > 100m))
            errors.Add("Tarif PPh Pasal 26 harus lebih dari 0 dan paling banyak 100.");

        return errors;
    }

    /// <summary>Lapisan harus mulai dari 0, bersambung tanpa celah atau tumpang tindih, dan lapisan terakhir tanpa batas atas.</summary>
    private static List<string> ValidateBands(List<(decimal Lower, decimal? Upper, decimal Rate)> bands, string label)
    {
        var errors = new List<string>();
        if (bands.Count == 0) return errors;

        if (bands[0].Lower != 0m) errors.Add($"{label}: lapisan pertama harus mulai dari 0.");
        for (var i = 0; i < bands.Count; i++)
        {
            var band = bands[i];
            if (band.Rate < 0m || band.Rate > 100m) errors.Add($"{label}: tarif baris {i + 1} harus antara 0 dan 100.");
            if (band.Upper.HasValue && band.Upper.Value <= band.Lower) errors.Add($"{label}: batas atas baris {i + 1} harus lebih besar dari batas bawahnya.");
            if (i < bands.Count - 1)
            {
                if (!band.Upper.HasValue) errors.Add($"{label}: hanya lapisan terakhir yang boleh tanpa batas atas.");
                else if (band.Upper.Value != bands[i + 1].Lower)
                    errors.Add($"{label}: batas atas baris {i + 1} harus sama dengan batas bawah baris berikutnya (tanpa celah atau tumpang tindih).");
            }
            else if (band.Upper.HasValue)
            {
                errors.Add($"{label}: lapisan terakhir harus tanpa batas atas.");
            }
        }

        return errors.Distinct().ToList();
    }

    // ------------------------------------------------------------------------------------------------------------------------
    // Perhitungan
    // ------------------------------------------------------------------------------------------------------------------------

    private static string Rupiah(decimal value) => "Rp " + value.ToString("N0", CultureInfo.InvariantCulture);

    private static decimal RoundTax(PayrollTaxPolicy policy, decimal value) =>
        policy.RoundTaxToWholeRupiah
            ? Math.Round(value, 0, MidpointRounding.AwayFromZero)
            : Math.Round(value, 2, MidpointRounding.AwayFromZero);

    /// <summary>Menghitung PPh satu penerima dari kebijakan yang disahkan. Mengembalikan penahan bila tidak ada formula yang sah.</summary>
    public static PayrollTaxComputation Compute(PayrollTaxPolicy policy, PayrollTaxInput input)
    {
        if (input.Article == TaxArticles.Pph26) return ComputePph26(policy, input);
        if (input.Article != TaxArticles.Pph21)
            return PayrollTaxComputation.Blocked("Jenis PPh tidak dikenali oleh mesin perhitungan.");

        return input.Category switch
        {
            TaxRecipientCategories.PermanentEmployee => ComputePermanentEmployee(policy, input),
            TaxRecipientCategories.NonEmployeeExpert => ComputeNonEmployee(policy, input),
            TaxRecipientCategories.NonPermanentEmployee => PayrollTaxComputation.Blocked(
                "Formula PPh 21 pegawai tidak tetap (harian/mingguan/satuan) belum tersedia pada mesin; gunakan Nominal Manual Disetujui atau Tidak Berlaku dengan dasar yang sah."),
            _ => PayrollTaxComputation.Blocked("Kategori penerima pajak tidak dikenali oleh mesin perhitungan.")
        };
    }

    /// <summary>Tarif TER (persen) untuk penghasilan bruto; null bila tabel tidak mencakup nilainya.</summary>
    public static decimal? LookupTerRate(PayrollTaxPolicy policy, string category, string periodKind, decimal gross)
    {
        var rows = policy.TerRates
            .Where(r => r.Category == category && r.PeriodKind == periodKind)
            .OrderBy(r => r.LowerBound)
            .ToList();
        foreach (var row in rows)
        {
            var aboveLower = row.LowerBound == 0m ? gross >= 0m : gross > row.LowerBound;
            var belowUpper = !row.UpperBound.HasValue || gross <= row.UpperBound.Value;
            if (aboveLower && belowUpper) return row.RatePercent;
        }

        return null;
    }

    /// <summary>Pajak progresif atas dasar pengenaan menurut lapisan kebijakan.</summary>
    public static decimal Progressive(PayrollTaxPolicy policy, decimal basis)
    {
        if (basis <= 0m) return 0m;
        decimal tax = 0m;
        foreach (var bracket in policy.Brackets.OrderBy(b => b.LowerBound))
        {
            var top = bracket.UpperBound.HasValue ? Math.Min(basis, bracket.UpperBound.Value) : basis;
            var slice = top - bracket.LowerBound;
            if (slice > 0m) tax += slice * bracket.RatePercent / 100m;
        }

        return tax;
    }

    private static decimal ApplyNpwpSurcharge(PayrollTaxPolicy policy, bool hasNpwp, decimal tax) =>
        hasNpwp ? tax : tax * (1m + policy.NoNpwpSurchargePercent / 100m);

    private static PayrollTaxComputation ComputePermanentEmployee(PayrollTaxPolicy policy, PayrollTaxInput input)
    {
        var gross = input.TaxableGross + input.EmployerPremium;
        var detail = new Dictionary<string, object?>
        {
            ["method"] = input.IsLastTaxPeriod ? "ANNUAL_PASAL17" : "TER",
            ["taxYear"] = input.TaxYear,
            ["taxMonth"] = input.TaxMonth,
            ["taxableGross"] = input.TaxableGross,
            ["employerPremium"] = input.EmployerPremium,
            ["gross"] = gross,
            ["employeePension"] = input.EmployeePension,
            ["hasNpwp"] = input.HasNpwp,
            // TER bulanan hanya memakai penghasilan bulan berjalan; akumulasi tahun berjalan hanya dibutuhkan pada masa pajak terakhir.
            ["ytdRequired"] = input.IsLastTaxPeriod
        };

        var ptkp = policy.Ptkp.FirstOrDefault(x => string.Equals(x.Status, input.PtkpStatus, StringComparison.OrdinalIgnoreCase));
        if (ptkp is null)
            return PayrollTaxComputation.Blocked(
                $"Status PTKP '{input.PtkpStatus ?? "-"}' tidak ada pada kebijakan tarif PPh {policy.Version}.", detail);
        detail["ptkpStatus"] = ptkp.Status;
        detail["ptkpAnnualAmount"] = ptkp.AnnualAmount;
        detail["terCategory"] = ptkp.TerCategory;

        if (!input.IsLastTaxPeriod)
        {
            // PAY-BE-FIX-015: kesiapan riwayat untuk rekonsiliasi masa pajak terakhir dicatat sebagai INFORMASI. TER bulanan tidak membacanya,
            // sehingga bulan yang belum tercakup di sini tidak pernah menjadi penahan perhitungan periode ini (ytdRequired = false).
            detail["ytdCoveredMonths"] = input.Ytd.CoveredMonths;
            detail["ytdOpeningBalanceMonths"] = input.Ytd.OpeningBalanceMonths;
            detail["ytdMissingMonths"] = input.Ytd.MissingMonths;
            detail["ytdOpeningBalanceFeatureAvailable"] = input.Ytd.OpeningBalanceFeatureAvailable;

            var rate = LookupTerRate(policy, ptkp.TerCategory, TaxTerPeriodKinds.Monthly, gross);
            if (rate is null)
                return PayrollTaxComputation.Blocked(
                    $"Tabel TER bulanan kategori {ptkp.TerCategory} pada kebijakan {policy.Version} tidak mencakup penghasilan bruto {Rupiah(gross)}.", detail);

            var tax = ApplyNpwpSurcharge(policy, input.HasNpwp, gross * rate.Value / 100m);
            detail["terRatePercent"] = rate.Value;
            detail["noNpwpSurchargePercent"] = input.HasNpwp ? 0m : policy.NoNpwpSurchargePercent;
            return new PayrollTaxComputation(RoundTax(policy, tax), Array.Empty<string>(), detail);
        }

        // Masa pajak terakhir: perhitungan tahunan Pasal 17 dikurangi PPh yang sudah dipotong pada masa pajak sebelumnya.
        // Cakupan akumulasi dicatat pada detail agar penyebab blocker (bulan yang hilang, saldo awal) terlihat dan dapat ditelusuri.
        detail["ytdCoveredMonths"] = input.Ytd.CoveredMonths;
        detail["ytdOpeningBalanceMonths"] = input.Ytd.OpeningBalanceMonths;
        detail["ytdMissingMonths"] = input.Ytd.MissingMonths;
        detail["ytdOpeningBalanceFeatureAvailable"] = input.Ytd.OpeningBalanceFeatureAvailable;
        var issues = new List<string>();
        if (input.HasPreviousEmployer)
            issues.Add("Masa pajak terakhir untuk pegawai yang punya pemberi kerja sebelumnya belum didukung mesin (perlu bukti 1721-A1 dan masa kerja sebelumnya); gunakan Nominal Manual Disetujui dengan dasar yang sah.");
        if (!input.Ytd.IsComplete) issues.AddRange(input.Ytd.Issues);
        if (issues.Count > 0)
        {
            var blocked = PayrollTaxComputation.Blocked(issues, detail);
            // PAY-BE-FIX-030: klasifikasi terstruktur. Hanya bila pemberi kerja sebelumnya tidak ada dan satu-satunya masalah adalah bulan
            // histori yang belum tercakup; masalah lain tetap penahan biasa.
            if (!input.HasPreviousEmployer && !input.Ytd.IsComplete && input.Ytd.HistoryGapOnly)
            {
                detail["blockCode"] = PayrollTaxBlockCodes.YtdHistoryIncomplete;
                return blocked with { BlockCode = PayrollTaxBlockCodes.YtdHistoryIncomplete };
            }

            return blocked;
        }

        var ytd = input.Ytd;
        var annualGross = ytd.PriorTaxableGross + ytd.PriorEmployerPremium + gross;
        var months = ytd.PriorPeriodCount + 1;
        var positionCost = Math.Min(
            annualGross * policy.PositionCostRatePercent / 100m,
            Math.Min(policy.PositionCostAnnualCap, policy.PositionCostMonthlyCap * months));
        var pension = ytd.PriorPension + input.EmployeePension;
        var net = annualGross - positionCost - pension;
        var pkp = Math.Max(0m, net - ptkp.AnnualAmount);
        pkp = Math.Floor(pkp / policy.PkpRoundingUnit) * policy.PkpRoundingUnit;
        var annualTax = RoundTax(policy, ApplyNpwpSurcharge(policy, input.HasNpwp, Progressive(policy, pkp)));
        var due = annualTax - ytd.PriorTaxWithheld;

        detail["annualGross"] = annualGross;
        detail["monthsWorked"] = months;
        detail["positionCost"] = RoundTax(policy, positionCost);
        detail["pensionDeduction"] = pension;
        detail["netIncome"] = net;
        detail["pkp"] = pkp;
        detail["annualTax"] = annualTax;
        detail["priorTaxWithheld"] = ytd.PriorTaxWithheld;
        detail["noNpwpSurchargePercent"] = input.HasNpwp ? 0m : policy.NoNpwpSurchargePercent;

        // Masa kerja kurang dari 12 bulan (mulai di tengah tahun atau berhenti sebelum Desember): mesin memakai penghasilan yang benar-benar
        // diterima dan PTKP setahun penuh tanpa penyetahunan. Perlakuan itu BELUM diverifikasi terhadap ketentuan resmi (PMK 168/2023),
        // sehingga hasilnya ditandai peringatan, bukan dinyatakan sesuai regulasi.
        if (months < 12)
        {
            detail["warnings"] = new[]
            {
                $"Masa kerja pada tahun pajak ini {months} bulan (kurang dari 12): perlakuan PTKP dan penghasilan untuk masa kerja parsial belum diverifikasi terhadap ketentuan resmi; periksa dasar perhitungan sebelum disahkan."
            };
        }

        if (due < 0m)
        {
            detail["reconciliationOutcome"] = "OVERWITHHELD";
            detail["overwithholdingAmount"] = -due;
            detail["resolutionPath"] = "OVERWITHHOLDING_ADJUSTMENT_REQUIRED";
            return PayrollTaxComputation.Blocked(
                $"Masa pajak terakhir menghasilkan kelebihan potong {Rupiah(-due)}: PPh yang sudah dipotong {Rupiah(ytd.PriorTaxWithheld)} lebih besar dari PPh terutang setahun {Rupiah(annualTax)}. "
                + "Nilai ini tidak diubah menjadi Rp0 dan tidak dikembalikan otomatis. Pastikan akumulasi dan saldo awal benar; bila benar, proses pengembalian atau kompensasi lewat mekanisme penyesuaian kelebihan potong yang disetujui "
                + "(belum tersedia pada versi ini) dan gunakan Nominal Manual Disetujui dengan dasar tertulis untuk masa pajak ini. Bukti perhitungan tersimpan pada rincian pajak.", detail);
        }

        detail["reconciliationOutcome"] = due > 0m ? "UNDERWITHHELD_CATCH_UP" : "EXACT";
        detail["shortfallAmount"] = due;
        return new PayrollTaxComputation(due, Array.Empty<string>(), detail);
    }

    /// <summary>
    /// PAY-BE-FIX-030: estimasi PPh sementara untuk masa pajak terakhir yang histori tahunannya belum lengkap. Memakai TER bulanan atas
    /// penghasilan periode berjalan dari kebijakan tarif yang SAMA dan sudah disahkan (tanpa angka bawaan). Hanya PPh 21 pegawai tetap.
    /// Bila PTKP atau tabel TER tidak mencakup, hasilnya "Estimasi Tidak Tersedia" dengan alasan, bukan Rp0.
    /// </summary>
    public static PayrollProvisionalTaxEstimate EstimateProvisionalTerProxy(PayrollTaxPolicy policy, PayrollTaxInput input, string treatment)
    {
        PayrollProvisionalTaxEstimate Unavailable(string reason) => new(
            false, null, PayrollProvisionalTaxEstimate.MethodTerMonthlyProxy, treatment, policy.Version, policy.LegalBasis, null, null, null,
            0m, PayrollTaxFinality.NotFinal, PayrollProvisionalTaxEstimate.ExplanationText, reason);

        if (input.Article != TaxArticles.Pph21 || input.Category != TaxRecipientCategories.PermanentEmployee)
            return Unavailable("Estimasi proksi TER hanya tersedia untuk PPh 21 pegawai tetap.");

        var ptkp = policy.Ptkp.FirstOrDefault(x => string.Equals(x.Status, input.PtkpStatus, StringComparison.OrdinalIgnoreCase));
        if (ptkp is null)
            return Unavailable($"Status PTKP '{input.PtkpStatus ?? "-"}' tidak ada pada kebijakan tarif PPh {policy.Version}.");

        var gross = input.TaxableGross + input.EmployerPremium;
        var rate = LookupTerRate(policy, ptkp.TerCategory, TaxTerPeriodKinds.Monthly, gross);
        if (rate is null)
            return Unavailable($"Tabel TER bulanan kategori {ptkp.TerCategory} pada kebijakan {policy.Version} tidak mencakup penghasilan bruto {Rupiah(gross)}.");

        var amount = RoundTax(policy, ApplyNpwpSurcharge(policy, input.HasNpwp, gross * rate.Value / 100m));
        return new PayrollProvisionalTaxEstimate(
            true, amount, PayrollProvisionalTaxEstimate.MethodTerMonthlyProxy, treatment, policy.Version, policy.LegalBasis, gross,
            ptkp.TerCategory, rate.Value, input.HasNpwp ? 0m : policy.NoNpwpSurchargePercent, PayrollTaxFinality.NotFinal,
            PayrollProvisionalTaxEstimate.ExplanationText, null);
    }

    private static PayrollTaxComputation ComputeNonEmployee(PayrollTaxPolicy policy, PayrollTaxInput input)
    {
        var detail = new Dictionary<string, object?>
        {
            ["method"] = "NON_EMPLOYEE",
            ["taxYear"] = input.TaxYear,
            ["taxMonth"] = input.TaxMonth,
            ["taxableGross"] = input.TaxableGross,
            ["hasNpwp"] = input.HasNpwp
        };

        if (!policy.NonEmployeeBasisPercent.HasValue || string.IsNullOrWhiteSpace(policy.NonEmployeeMethod))
            return PayrollTaxComputation.Blocked(
                $"Kebijakan tarif PPh {policy.Version} belum memuat formula penerima bukan pegawai (dasar pengenaan dan cara penerapan tarif).", detail);

        var basisPercent = policy.NonEmployeeBasisPercent.Value;
        var currentBasis = input.TaxableGross * basisPercent / 100m;
        detail["nonEmployeeBasisPercent"] = basisPercent;
        detail["nonEmployeeMethod"] = policy.NonEmployeeMethod;
        detail["currentBasis"] = currentBasis;
        detail["noNpwpSurchargePercent"] = input.HasNpwp ? 0m : policy.NoNpwpSurchargePercent;

        if (policy.NonEmployeeMethod == TaxNonEmployeeMethods.PerPayment)
            return new PayrollTaxComputation(
                RoundTax(policy, ApplyNpwpSurcharge(policy, input.HasNpwp, Progressive(policy, currentBasis))), Array.Empty<string>(), detail);

        // CUMULATIVE: tarif progresif atas dasar pengenaan kumulatif tahun berjalan dikurangi PPh yang sudah dipotong.
        detail["ytdRequired"] = true;
        detail["ytdCoveredMonths"] = input.Ytd.CoveredMonths;
        detail["ytdMissingMonths"] = input.Ytd.MissingMonths;
        if (!input.Ytd.IsComplete) return PayrollTaxComputation.Blocked(input.Ytd.Issues, detail);

        var cumulativeBasis = input.Ytd.PriorTaxableGross * basisPercent / 100m + currentBasis;
        var cumulativeTax = RoundTax(policy, ApplyNpwpSurcharge(policy, input.HasNpwp, Progressive(policy, cumulativeBasis)));
        var due = cumulativeTax - input.Ytd.PriorTaxWithheld;
        detail["cumulativeBasis"] = cumulativeBasis;
        detail["cumulativeTax"] = cumulativeTax;
        detail["priorTaxWithheld"] = input.Ytd.PriorTaxWithheld;

        if (due < 0m)
            return PayrollTaxComputation.Blocked(
                $"PPh kumulatif lebih kecil dari PPh yang sudah dipotong ({Rupiah(due)}); periksa penghasilan bulan-bulan sebelumnya.", detail);

        return new PayrollTaxComputation(due, Array.Empty<string>(), detail);
    }

    private static PayrollTaxComputation ComputePph26(PayrollTaxPolicy policy, PayrollTaxInput input)
    {
        var detail = new Dictionary<string, object?>
        {
            ["method"] = "PPH26_FLAT",
            ["taxYear"] = input.TaxYear,
            ["taxMonth"] = input.TaxMonth,
            ["taxableGross"] = input.TaxableGross
        };

        decimal rate;
        if (input.TreatyRatePercent.HasValue)
        {
            if (string.IsNullOrWhiteSpace(input.TreatyBasis))
                return PayrollTaxComputation.Blocked("Tarif P3B diisi tanpa dasar (dokumen/ketentuan P3B yang berlaku); lengkapi dasar atau kosongkan tarif P3B.", detail);
            rate = input.TreatyRatePercent.Value;
            detail["rateSource"] = "TREATY";
            detail["treatyBasis"] = input.TreatyBasis;
        }
        else if (policy.Pph26RatePercent.HasValue)
        {
            rate = policy.Pph26RatePercent.Value;
            detail["rateSource"] = "POLICY";
        }
        else
        {
            return PayrollTaxComputation.Blocked($"Kebijakan tarif PPh {policy.Version} belum memuat tarif PPh Pasal 26.", detail);
        }

        detail["ratePercent"] = rate;
        return new PayrollTaxComputation(RoundTax(policy, input.TaxableGross * rate / 100m), Array.Empty<string>(), detail);
    }
}
