using System.Globalization;
using System.Text.Json;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>Fakta konfigurasi PPh satu orang (PrlTax) yang dinilai. Tanpa NPWP dan tanpa data pribadi lain.</summary>
public sealed record PayrollTaxConfigFacts(
    Guid Id,
    bool IsActive,
    string? Article,
    string? RecipientCategory,
    string? CalculationMode,
    string TaxMethod,
    string? PtkpStatus,
    bool IsTaxResident,
    DateTime? EffectiveStartDate,
    DateTime? EffectiveEndDate,
    string? PolicyVersion,
    string? ExemptionReason,
    string? DecisionBasis,
    decimal? ApprovedManualAmount,
    string? ManualAmountApprovalStatus,
    decimal? TreatyRatePercent,
    string? TreatyBasis,
    bool IsNpwpRegistered,
    bool HasNpwpNumber,
    bool HasPreviousEmployer)
{
    public static PayrollTaxConfigFacts From(PrlTax x) => new(
        x.Id, x.IsActive, x.TaxArticle, x.RecipientTaxCategory, x.CalculationMode, x.TaxMethod, x.TaxStatus, x.IsTaxResident,
        x.EffectiveStartDate, x.EffectiveEndDate, x.PolicyVersion, x.ExemptionReason, x.DecisionBasis, x.ApprovedManualAmount,
        x.ManualAmountApprovalStatus, x.TreatyRatePercent, x.TreatyBasis, x.IsNpwpRegistered, !string.IsNullOrWhiteSpace(x.NpwpNumber),
        x.HasPreviousEmployer);
}

/// <summary>
/// Masukan perhitungan PPh otomatis pada satu penerima untuk satu periode run. <see cref="Policy"/> null berarti belum ada kebijakan
/// tarif PPh yang disahkan dan berlaku ("Menunggu Verifikasi Kebijakan"): perhitungan otomatis menjadi penahan, bukan Rp0.
/// </summary>
public sealed record PayrollTaxRunContext(
    PayrollTaxPolicy? Policy,
    int TaxYear,
    int TaxMonth,
    bool IsLastTaxPeriod,
    decimal TaxableGross,
    decimal EmployerPremium,
    decimal EmployeePension,
    PayrollTaxYtd Ytd);

/// <summary>Fakta konfigurasi iuran satu orang (PrlInsurance). Tarif per pegawai pada profil tidak dipakai mesin; hanya penandanya.</summary>
public sealed record PayrollInsuranceConfigFacts(
    Guid Id,
    bool IsActive,
    string? ParticipationStatus,
    string? ParticipationBasis,
    bool HealthEnabled,
    bool EmploymentEnabled,
    string? EmploymentPrograms,
    bool PrivateEnabled,
    decimal PrivateEmployeeContribution,
    decimal PrivateEmployerContribution,
    bool HasPerEmployeeRates,
    DateTime? EffectiveStartDate,
    DateTime? EffectiveEndDate)
{
    public static PayrollInsuranceConfigFacts From(PrlInsurance x) => new(
        x.Id, x.IsActive, x.ParticipationStatus, x.ParticipationBasis, x.IsBpjsKesehatanEnabled, x.IsBpjsKetenagakerjaanEnabled,
        x.BpjsEmploymentPrograms, x.IsPrivateInsuranceEnabled, x.PrivateInsuranceEmployeeContribution,
        x.PrivateInsuranceEmployerContribution,
        x.BpjsHealthEmployeeRate > 0m || x.BpjsHealthEmployerRate > 0m || x.BpjsEmploymentEmployeeRate > 0m || x.BpjsEmploymentEmployerRate > 0m,
        x.EffectiveStartDate, x.EffectiveEndDate);
}

/// <summary>
/// Parameter iuran BPJS yang berlaku pada satu payroll run: kebijakan iuran BPJS berstatus APPROVED (PrlBpjsPolicy) yang berlaku pada
/// periode. Tidak ada tarif yang ditanam di kode, dan nilai bawaan Pengaturan Penggajian tidak pernah dipakai langsung.
/// </summary>
public sealed record PayrollInsurancePolicy(
    string Version,
    decimal HealthCap, decimal HealthEmployeeRate, decimal HealthEmployerRate,
    decimal JpCap, decimal JpEmployeeRate, decimal JpEmployerRate,
    decimal JhtEmployeeRate, decimal JhtEmployerRate,
    decimal JkkRate, decimal JkmRate,
    Guid? PolicyId = null, string? LegalBasis = null, int? JkkRiskClass = null)
{
    public static PayrollInsurancePolicy FromEntity(PrlBpjsPolicy e) => new(
        e.PolicyVersion,
        e.HealthCapAmount, e.HealthEmployeeRate, e.HealthEmployerRate,
        e.JpCapAmount, e.JpEmployeeRate, e.JpEmployerRate,
        e.JhtEmployeeRate, e.JhtEmployerRate,
        e.JkkRate, e.JkmRate,
        e.Id, e.LegalBasis, e.JkkRiskClass);
}

/// <summary>
/// Ketersediaan kebijakan yang disahkan pada satu tanggal. Dipakai kelengkapan data untuk menyatakan "Menunggu Verifikasi Kebijakan"
/// sebagai keadaan terpisah dari konfigurasi per orang, sekaligus menandai mode pengujian ketika BPJS dinonaktifkan sementara.
/// </summary>
public sealed record PayrollPolicyAvailability(
    bool TaxApproved,
    string? TaxVersion,
    bool BpjsApproved,
    string? BpjsVersion,
    bool EnableBpjsInPayroll = true);

public sealed record PayrollInsuranceLine(string Code, string Name, string Program, bool EmployeePaid, decimal BaseAmount, decimal Rate, decimal Amount);

/// <summary>Hasil penilaian PPh satu penerima pada satu periode: status konfigurasi, status hasil, nominal, dan alasannya.</summary>
public sealed record PayrollTaxAssessment(
    string ConfigurationStatus,
    string ResultStatus,
    string Source,
    string? Article,
    string? RecipientCategory,
    string? CalculationMode,
    string? TaxMethod,
    string? PtkpStatus,
    string? PolicyVersion,
    string Treatment,
    decimal EmployeeAmount,
    decimal EmployerBorneAmount,
    bool IsBlocking,
    IReadOnlyList<string> Issues,
    IReadOnlyList<string> Warnings,
    Guid? ConfigId,
    DateTime? EffectiveStartDate,
    DateTime? EffectiveEndDate,
    string? DecisionBasis,
    string? ExemptionReason)
{
    public bool IsConfigured => ConfigurationStatus == PayrollConfigurationStatuses.Configured;

    // ---- Penelusuran perhitungan otomatis (kosong untuk nominal manual/pengecualian). Diisi lewat `with` setelah penilaian. ----

    /// <summary>Kebijakan tarif PPh yang dipakai (id, dasar peraturan); versinya ada pada <see cref="PolicyVersion"/>.</summary>
    public Guid? PolicyId { get; init; }
    public string? PolicyLegalBasis { get; init; }

    /// <summary>Status persetujuan nominal manual yang berlaku saat dihitung.</summary>
    public string? ManualAmountApprovalStatus { get; init; }

    /// <summary>Masa pajak dan komponen yang menjadi dasar hitung; juga sumber akumulasi tahun berjalan untuk masa pajak berikutnya.</summary>
    public int? TaxYear { get; init; }
    public int? TaxMonth { get; init; }
    public bool IsLastTaxPeriod { get; init; }
    public decimal? TaxableGross { get; init; }
    public decimal EmployerPremium { get; init; }
    public decimal EmployeePension { get; init; }

    /// <summary>Rincian langkah hitung (kategori TER, tarif, PKP, dst.) sebagai bukti penelusuran.</summary>
    public IReadOnlyDictionary<string, object?>? Detail { get; init; }

    public PayrollTaxSnapshot ToSnapshot() => new(
        Source, ConfigurationStatus, ResultStatus, Article, RecipientCategory, CalculationMode, TaxMethod, PtkpStatus, PolicyVersion,
        Treatment, EmployeeAmount, EmployerBorneAmount, ConfigId, EffectiveStartDate, EffectiveEndDate, DecisionBasis, ExemptionReason,
        Issues.ToList(), Warnings.ToList(), IsBlocking)
    {
        PolicyId = PolicyId,
        PolicyLegalBasis = PolicyLegalBasis,
        ManualAmountApprovalStatus = ManualAmountApprovalStatus,
        TaxYear = TaxYear,
        TaxMonth = TaxMonth,
        IsLastTaxPeriod = IsLastTaxPeriod,
        TaxableGross = TaxableGross,
        EmployerPremium = EmployerPremium,
        EmployeePension = EmployeePension,
        Detail = Detail?.ToDictionary(x => x.Key, x => x.Value)
    };
}

/// <summary>Hasil penilaian iuran satu penerima pada satu periode.</summary>
public sealed record PayrollInsuranceAssessment(
    string ConfigurationStatus,
    string ResultStatus,
    string Source,
    string? ParticipationStatus,
    IReadOnlyList<string> Programs,
    string? PolicyVersion,
    decimal WageBase,
    decimal EmployeeTotal,
    decimal EmployerTotal,
    IReadOnlyList<PayrollInsuranceLine> Lines,
    bool IsBlocking,
    IReadOnlyList<string> Issues,
    IReadOnlyList<string> Warnings,
    Guid? ConfigId)
{
    public bool IsConfigured => ConfigurationStatus == PayrollConfigurationStatuses.Configured;

    public Guid? PolicyId { get; init; }
    public string? PolicyLegalBasis { get; init; }
    public int? JkkRiskClass { get; init; }
    public IReadOnlyList<PayrollWageBaseItem>? WageBaseItems { get; init; }

    public PayrollInsuranceSnapshot ToSnapshot() => new(
        Source, ConfigurationStatus, ResultStatus, ParticipationStatus, Programs.ToList(), PolicyVersion, WageBase, EmployeeTotal,
        EmployerTotal,
        Lines.Select(l => new PayrollInsuranceSnapshotLine(l.Code, l.Program, l.EmployeePaid ? "Employee" : "Employer", l.BaseAmount, l.Rate, l.Amount)).ToList(),
        Issues.ToList(), Warnings.ToList(), IsBlocking, ConfigId)
    {
        PolicyId = PolicyId,
        PolicyLegalBasis = PolicyLegalBasis,
        JkkRiskClass = JkkRiskClass,
        WageBaseItems = WageBaseItems?.ToList()
    };
}

/// <summary>Snapshot PPh yang disimpan pada PrlPayrollRunEmployee.TaxSnapshotJson. Tidak memuat NPWP atau data pribadi lain.</summary>
public sealed record PayrollTaxSnapshot(
    string Source,
    string ConfigurationStatus,
    string ResultStatus,
    string? Article,
    string? RecipientCategory,
    string? CalculationMode,
    string? TaxMethod,
    string? PtkpStatus,
    string? PolicyVersion,
    string Treatment,
    decimal EmployeeAmount,
    decimal EmployerBorneAmount,
    Guid? ConfigId,
    DateTime? EffectiveStartDate,
    DateTime? EffectiveEndDate,
    string? DecisionBasis,
    string? ExemptionReason,
    List<string> Issues,
    List<string> Warnings,
    bool IsBlocking)
{
    /// <summary>Kebijakan tarif PPh yang dipakai (id dan dasar peraturan); versinya ada pada PolicyVersion.</summary>
    public Guid? PolicyId { get; init; }
    public string? PolicyLegalBasis { get; init; }
    public string? ManualAmountApprovalStatus { get; init; }

    /// <summary>Masa pajak, dasar hitung, dan komponennya. Snapshot lama tidak memuatnya (TaxableGross null) dan tidak dapat menjadi sumber akumulasi.</summary>
    public int? TaxYear { get; init; }
    public int? TaxMonth { get; init; }
    public bool IsLastTaxPeriod { get; init; }
    public decimal? TaxableGross { get; init; }
    public decimal EmployerPremium { get; init; }
    public decimal EmployeePension { get; init; }

    /// <summary>Rincian langkah hitung sebagai bukti penelusuran (kategori TER, tarif, PKP, dst.).</summary>
    public Dictionary<string, object?>? Detail { get; init; }
}

/// <summary>Satu komponen penghasilan beserta klasifikasinya terhadap dasar upah iuran BPJS (traceability dasar upah).</summary>
public sealed record PayrollWageBaseItem(string Source, string Name, decimal Amount, string Classification, string? Reason);

public sealed record PayrollInsuranceSnapshotLine(string Code, string Program, string PaidBy, decimal BaseAmount, decimal Rate, decimal Amount);

/// <summary>Snapshot iuran yang disimpan pada PrlPayrollRunEmployee.InsuranceSnapshotJson.</summary>
public sealed record PayrollInsuranceSnapshot(
    string Source,
    string ConfigurationStatus,
    string ResultStatus,
    string? ParticipationStatus,
    List<string> Programs,
    string? PolicyVersion,
    decimal WageBase,
    decimal EmployeeTotal,
    decimal EmployerTotal,
    List<PayrollInsuranceSnapshotLine> Lines,
    List<string> Issues,
    List<string> Warnings,
    bool IsBlocking,
    Guid? ConfigId)
{
    /// <summary>Kebijakan iuran BPJS yang dipakai: id, dasar penetapan, dan kelompok risiko JKK. Versinya ada pada PolicyVersion.</summary>
    public Guid? PolicyId { get; init; }
    public string? PolicyLegalBasis { get; init; }
    public int? JkkRiskClass { get; init; }

    /// <summary>Dasar upah per komponen beserta klasifikasinya (masuk, tidak masuk, atau belum diklasifikasikan).</summary>
    public List<PayrollWageBaseItem>? WageBaseItems { get; init; }
}

/// <summary>Kesiapan konfigurasi PPh dan iuran satu orang untuk layar kelengkapan data (tanpa periode run).</summary>
public sealed record PayrollTaxInsuranceReadiness(
    string TaxConfigurationStatus,
    string InsuranceConfigurationStatus,
    IReadOnlyList<string> TaxIssues,
    IReadOnlyList<string> InsuranceIssues,
    IReadOnlyList<string> Notes)
{
    public bool IsComplete =>
        TaxConfigurationStatus == PayrollConfigurationStatuses.Configured
        && (InsuranceConfigurationStatus == PayrollConfigurationStatuses.Configured
            || InsuranceConfigurationStatus == PayrollConfigurationStatuses.TestingModeDisabled);
}

/// <summary>
/// Satu-satunya tempat aturan konfigurasi PPh dan iuran yang dikelola HRD (PrlTax, PrlInsurance): validasi tulis (dipakai controller),
/// penilaian per periode (dipakai mesin payroll run), dan kesiapan (dipakai kelengkapan data dan Profil Penggajian). Murni, tanpa akses
/// data.
///
/// Prinsip yang dijaga:
/// - Jenis PPh, kategori penerima, metode pajak, dan skema hitung adalah konsep terpisah; semuanya eksplisit dari HRD.
/// - Nol bukan penentu kelengkapan. NOT_CONFIGURED (belum ada keputusan HRD) berbeda dari VALID_ZERO/EXEMPT/NOT_APPLICABLE (nol yang
///   sah dengan dasar) dan ERROR (konfigurasi ada tetapi tidak dapat dihitung secara sah).
/// - Tidak ada tarif atau angka regulasi di sini. Skema AUTO_REGULATORY dihitung oleh PayrollTaxPolicyEngine dari kebijakan tarif PPh
///   berversi yang disahkan (PrlTaxPolicy); tanpa kebijakan yang berlaku atau untuk kombinasi tanpa formula yang sah (pegawai tidak
///   tetap, GrossUp) ia menghasilkan blocker, bukan Rp0. Nominal manual hanya dipakai bila sudah disetujui pengguna lain; pengecualian
///   wajib berdasar.
/// - Iuran BPJS memakai tarif dan plafon dari kebijakan iuran BPJS berversi yang disahkan (PrlBpjsPolicy), bukan dari nilai bawaan
///   Pengaturan Penggajian dan bukan tarif per pegawai. Dasar upah mengikuti klasifikasi tunjangan HRD.
/// - Data profil payroll lama (PrlPayroll.TaxAmount/InsuranceAmount) hanya cadangan bila HRD belum mengonfigurasi; ditandai sebagai
///   belum dikonfigurasi dan tidak pernah mengalahkan konfigurasi HRD.
/// </summary>
public static class PayrollTaxInsuranceRules
{
    /// <summary>
    /// Kombinasi yang tidak punya formula pada mesin (pegawai tidak tetap, GrossUp). Skema otomatis untuk kombinasi lain mensyaratkan
    /// kebijakan tarif PPh berversi yang disahkan dan berlaku pada periode run; tanpanya hasilnya penahan, bukan Rp0.
    /// </summary>
    public const string AutoRegulatoryUnavailable =
        "Perhitungan otomatis PPh tidak tersedia untuk kombinasi ini: pegawai tidak tetap (harian/mingguan/satuan) dan metode GrossUp belum memiliki formula pada mesin. "
        + "Gunakan skema Nominal Manual Disetujui atau Tidak Berlaku dengan dasar yang sah.";

    public const string AutoPolicyMissing =
        "Belum ada kebijakan tarif PPh berversi yang aktif dan berlaku pada periode ini. "
        + "Penggajian memerlukan kebijakan tarif PPh yang berlaku sesuai tanggal periode.";

    public const string GrossUpUnavailable =
        "Metode GrossUp belum memiliki formula yang sah; gunakan metode Gross atau Net.";

    public const string LegacyTaxInsuranceBlockedMessage =
        "Kalkulasi PPh dan iuran BPJS pada jalur lama dinonaktifkan: tarifnya tertanam atau berasal dari nilai bawaan yang belum diverifikasi. "
        + "Gunakan Payroll > Hitung Payroll Run, yang memakai kebijakan tarif PPh dan iuran BPJS berversi yang sudah disahkan.";

    public const string LegacyPathDisabledMessage =
        "Jalur perhitungan payroll lama dinonaktifkan. Payroll Run resmi (Payroll > Payroll Run) adalah satu-satunya pemilik hasil gaji.";

    public const string LegacyEngineBlockedMessage =
        "Payroll run ini dihitung oleh mesin Payroll Run resmi (Payroll > Hitung Payroll Run). Jalur perhitungan lama tidak boleh menghitung "
        + "ulang pajak atau iuran pada run yang sama karena akan menggandakan potongan dan menimpa total.";

    private static readonly string[] EmployeeCategories =
        { TaxRecipientCategories.PermanentEmployee, TaxRecipientCategories.NonPermanentEmployee };

    /// <summary>
    /// True bila run sudah dihitung oleh mesin Payroll Run resmi (snapshot kebijakan memuat versi mesinnya). Jalur lama (modul "Modern":
    /// PayrollTaxCalculationService dan PayrollBpjsCalculationService) menimpa total pegawai dan membuat komponen pajak/iuran sendiri,
    /// sehingga tidak boleh berjalan pada run itu: satu mesin aktif, tanpa komponen ganda.
    /// </summary>
    public static bool IsCalculatedByOfficialEngine(string? policySnapshotJson)
    {
        if (string.IsNullOrWhiteSpace(policySnapshotJson)) return false;
        try
        {
            using var document = JsonDocument.Parse(policySnapshotJson);
            return document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty("engine", out var engine)
                && engine.ValueKind == JsonValueKind.String
                && string.Equals(engine.GetString(), PayrollCalculationService.EngineVersion, StringComparison.Ordinal);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// True bila snapshot kebijakan run menandai bahwa run dihitung dalam mode pengujian tanpa BPJS
    /// (<c>Payroll:Policy:EnableBpjsInPayroll = false</c>). Dipakai untuk menandai banner UI dan menahan Finance Handoff produksi.
    /// </summary>
    public static bool IsTestingModeWithoutBpjs(string? policySnapshotJson)
    {
        if (string.IsNullOrWhiteSpace(policySnapshotJson)) return false;
        try
        {
            using var document = JsonDocument.Parse(policySnapshotJson);
            if (document.RootElement.ValueKind != JsonValueKind.Object) return false;
            if (document.RootElement.TryGetProperty("testingModeWithoutBpjs", out var testingProp)
                && testingProp.ValueKind == JsonValueKind.True)
                return true;
            if (document.RootElement.TryGetProperty("enableBpjsInPayroll", out var enableProp)
                && enableProp.ValueKind == JsonValueKind.False)
                return true;
            return false;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    // ------------------------------------------------------------------------------------------------------------------------
    // Pembantu umum
    // ------------------------------------------------------------------------------------------------------------------------

    public static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    /// <summary>Nilai kode dari klien: dipangkas dan huruf besar; kosong menjadi null.</summary>
    public static string? NormalizeCode(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();

    private static string Format(DateTime? value) =>
        value.HasValue ? value.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : "-";

    /// <summary>Masa berlaku [start, end] beririsan dengan [from, to]; batas kosong berarti tidak dibatasi.</summary>
    public static bool OverlapsPeriod(DateTime? start, DateTime? end, DateTime from, DateTime to) =>
        (!start.HasValue || start.Value.Date <= to.Date) && (!end.HasValue || end.Value.Date >= from.Date);

    /// <summary>
    /// Apakah mesin punya formula untuk kombinasi jenis PPh, kategori penerima, dan metode. Tarif dan tabelnya selalu dari kebijakan
    /// berversi yang disahkan (bukan dari kode). Tidak didukung: pegawai tidak tetap (harian/mingguan/satuan) dan metode GrossUp.
    /// </summary>
    public static bool IsAutoRegulatorySupported(string? article, string? category, string? method)
    {
        if (string.Equals(method, TaxMethodKinds.GrossUp, StringComparison.OrdinalIgnoreCase)) return false;
        return article switch
        {
            TaxArticles.Pph21 => category is TaxRecipientCategories.PermanentEmployee or TaxRecipientCategories.NonEmployeeExpert,
            TaxArticles.Pph26 => category is not null,
            _ => false
        };
    }

    /// <summary>PTKP hanya relevan untuk PPh 21 penerima berstatus pegawai; selainnya diabaikan oleh perhitungan.</summary>
    public static bool IsPtkpRelevant(string? article, string? category) =>
        article == TaxArticles.Pph21 && category is not null && EmployeeCategories.Contains(category);

    // ------------------------------------------------------------------------------------------------------------------------
    // PPh: validasi tulis
    // ------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Validasi konfigurasi PPh pada saat HRD menyimpan. Ketiga atribut (jenis, kategori, skema) kosong bersamaan berarti belum
    /// dikonfigurasi dan diterima (kompatibel dengan klien lama); bila salah satu diisi, ketiganya wajib.
    /// </summary>
    public static List<string> ValidateTaxConfiguration(
        string? article, string? category, string? mode, string? method, bool isTaxResident,
        string? exemptionReason, string? decisionBasis, decimal? manualAmount,
        decimal? treatyRatePercent = null, string? treatyBasis = null)
    {
        var errors = new List<string>();
        var anyChosen = article is not null || category is not null || mode is not null;

        if (treatyRatePercent.HasValue)
        {
            if (treatyRatePercent.Value <= 0m || treatyRatePercent.Value > 100m) errors.Add("Tarif P3B harus lebih dari 0 dan paling banyak 100.");
            if (string.IsNullOrWhiteSpace(treatyBasis))
                errors.Add("Tarif P3B wajib disertai dasar (negara mitra dan nomor dokumen/SKD yang berlaku).");
            if (article is not null && article != TaxArticles.Pph26) errors.Add("Tarif P3B hanya untuk PPh Pasal 26.");
        }
        else if (!string.IsNullOrWhiteSpace(treatyBasis))
        {
            errors.Add("Dasar P3B hanya boleh diisi bersama tarif P3B.");
        }

        if (!anyChosen)
        {
            if (manualAmount.HasValue) errors.Add("Nominal PPh manual hanya boleh diisi bersama skema APPROVED_MANUAL_AMOUNT.");
            if (!string.IsNullOrWhiteSpace(exemptionReason)) errors.Add("Alasan pengecualian hanya boleh diisi bersama skema EXEMPT_OR_NOT_APPLICABLE.");
            return errors;
        }

        if (article is null || category is null || mode is null)
        {
            errors.Add("Jenis PPh, kategori penerima, dan skema perhitungan harus diisi bersamaan.");
            return errors;
        }

        if (!TaxArticles.All.Contains(article)) errors.Add("Jenis PPh tidak valid. Gunakan PPH21 atau PPH26.");
        if (!TaxRecipientCategories.All.Contains(category)) errors.Add("Kategori penerima pajak tidak valid.");
        if (!TaxCalculationModes.All.Contains(mode)) errors.Add("Skema perhitungan pajak tidak valid.");
        if (method is null || !TaxMethodKinds.All.Contains(method, StringComparer.OrdinalIgnoreCase))
            errors.Add("Metode pajak tidak valid. Gunakan Gross, GrossUp, atau Net.");
        if (errors.Count > 0) return errors;

        if (article == TaxArticles.Pph26 && isTaxResident)
            errors.Add("PPh Pasal 26 hanya untuk subjek pajak luar negeri; ubah status subjek pajak atau pilih PPH21.");
        if (article == TaxArticles.Pph21 && !isTaxResident)
            errors.Add("PPh Pasal 21 hanya untuk subjek pajak dalam negeri; subjek pajak luar negeri dipotong PPh Pasal 26.");

        switch (mode)
        {
            case TaxCalculationModes.AutoRegulatory:
                if (!IsAutoRegulatorySupported(article, category, method)) errors.Add(AutoRegulatoryUnavailable);
                if (manualAmount.HasValue) errors.Add("Nominal PPh manual hanya boleh diisi bersama skema APPROVED_MANUAL_AMOUNT.");
                break;

            case TaxCalculationModes.ApprovedManualAmount:
                if (!manualAmount.HasValue) errors.Add("Nominal PPh manual wajib diisi untuk skema APPROVED_MANUAL_AMOUNT.");
                else if (manualAmount.Value < 0m) errors.Add("Nominal PPh manual tidak boleh negatif.");
                if (string.IsNullOrWhiteSpace(decisionBasis))
                    errors.Add("Dasar penetapan nominal manual wajib diisi (nomor atau referensi persetujuan).");
                if (string.Equals(method, TaxMethodKinds.GrossUp, StringComparison.OrdinalIgnoreCase))
                    errors.Add(GrossUpUnavailable);
                break;

            case TaxCalculationModes.ExemptOrNotApplicable:
                if (string.IsNullOrWhiteSpace(exemptionReason)) errors.Add("Alasan pengecualian wajib diisi untuk skema EXEMPT_OR_NOT_APPLICABLE.");
                if (string.IsNullOrWhiteSpace(decisionBasis)) errors.Add("Dasar aturan pengecualian wajib diisi untuk skema EXEMPT_OR_NOT_APPLICABLE.");
                if (manualAmount.HasValue) errors.Add("Nominal PPh manual tidak boleh diisi untuk skema pengecualian.");
                break;
        }

        return errors;
    }

    // ------------------------------------------------------------------------------------------------------------------------
    // PPh: penilaian per periode
    // ------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Menilai konfigurasi PPh satu orang pada periode [periodStart, periodEnd]. <paramref name="legacyAmount"/> adalah tetapan lama
    /// PrlPayroll.TaxAmount: dipakai HANYA bila HRD belum mengonfigurasi dan nilainya &gt; 0 (cadangan yang ditandai), tidak pernah
    /// mengalahkan konfigurasi HRD.
    ///
    /// <paramref name="run"/> hanya diisi mesin payroll run. Tanpanya (kesiapan, ringkasan) skema otomatis dinilai sebatas konfigurasinya.
    /// Dengan <paramref name="run"/>, skema otomatis dihitung dari kebijakan tarif PPh berversi yang disahkan; bila kebijakan tidak ada
    /// atau formulanya tidak sah, hasilnya penahan (ERROR), bukan Rp0. Nominal manual hanya dipakai bila sudah disetujui pengguna lain.
    /// </summary>
    public static PayrollTaxAssessment AssessTax(
        PayrollTaxConfigFacts? cfg, decimal legacyAmount, DateTime periodStart, DateTime periodEnd, PayrollTaxRunContext? run = null)
    {
        legacyAmount = Round(legacyAmount);

        if (cfg is null) return TaxUnconfigured(null, new[] { "Profil pajak belum diisi HRD." }, legacyAmount);
        if (!cfg.IsActive) return TaxUnconfigured(cfg, new[] { "Profil pajak HRD tidak aktif." }, legacyAmount);

        if (!OverlapsPeriod(cfg.EffectiveStartDate, cfg.EffectiveEndDate, periodStart, periodEnd))
        {
            var message = cfg.EffectiveStartDate.HasValue && cfg.EffectiveStartDate.Value.Date > periodEnd.Date
                ? $"Profil pajak tersedia tetapi baru berlaku mulai {Format(cfg.EffectiveStartDate)}; belum berlaku pada periode ini."
                : $"Profil pajak sudah berakhir pada {Format(cfg.EffectiveEndDate)}; tidak berlaku pada periode ini.";
            return TaxBlocked(cfg, PayrollConfigurationStatuses.NotEffective, PayrollComponentResultStatuses.NotConfigured, new[] { message });
        }

        var article = NormalizeCode(cfg.Article);
        var category = NormalizeCode(cfg.RecipientCategory);
        var mode = NormalizeCode(cfg.CalculationMode);
        var method = string.IsNullOrWhiteSpace(cfg.TaxMethod) ? null : cfg.TaxMethod.Trim();

        var missing = new List<string>();
        if (article is null) missing.Add("Jenis PPh belum ditentukan HRD.");
        if (category is null) missing.Add("Kategori penerima pajak belum ditentukan HRD.");
        if (mode is null) missing.Add("Skema perhitungan pajak belum ditentukan HRD.");
        // Pemeriksaan null eksplisit (setara missing.Count > 0) agar analisis aliran null tahu article/category/mode tidak null di bawah.
        if (article is null || category is null || mode is null) return TaxUnconfigured(cfg, missing, legacyAmount);

        var errors = ValidateTaxConfiguration(
            article, category, mode, method, cfg.IsTaxResident, cfg.ExemptionReason, cfg.DecisionBasis, cfg.ApprovedManualAmount,
            cfg.TreatyRatePercent, cfg.TreatyBasis);
        if (errors.Count > 0)
            return TaxBlocked(cfg, PayrollConfigurationStatuses.Error, PayrollComponentResultStatuses.Error, errors);

        var ptkp = IsPtkpRelevant(article, category) ? cfg.PtkpStatus : null;
        PayrollTaxAssessment Result(string resultStatus, string treatment, decimal employee, decimal employerBorne, IReadOnlyList<string>? warnings = null) =>
            new(PayrollConfigurationStatuses.Configured, resultStatus, PayrollTaxInsuranceSources.HrdProfile, article, category, mode, method,
                ptkp, cfg.PolicyVersion, treatment, employee, employerBorne, false, Array.Empty<string>(), warnings ?? Array.Empty<string>(),
                cfg.Id, cfg.EffectiveStartDate, cfg.EffectiveEndDate, cfg.DecisionBasis, cfg.ExemptionReason);

        if (mode == TaxCalculationModes.ExemptOrNotApplicable)
            return Result(PayrollComponentResultStatuses.Exempt, TaxTreatments.None, 0m, 0m);

        if (mode == TaxCalculationModes.AutoRegulatory)
        {
            // Kesiapan/ringkasan tanpa periode: yang dinilai hanya konfigurasi orangnya; ketersediaan kebijakan dilaporkan terpisah.
            if (run is null)
            {
                if (IsPtkpRelevant(article, category) && string.IsNullOrWhiteSpace(ptkp))
                    return TaxBlocked(
                        cfg,
                        PayrollConfigurationStatuses.NotConfigured,
                        PayrollComponentResultStatuses.NotConfigured,
                        new[] { "Status PTKP pegawai belum ditetapkan di HRD; wajib diisi agar PPh 21 otomatis dapat dihitung." });
                return Result(PayrollComponentResultStatuses.Calculated, TaxTreatments.EmployeeDeduction, 0m, 0m);
            }

            if (run.Policy is null)
                return TaxBlocked(cfg, PayrollConfigurationStatuses.Configured, PayrollComponentResultStatuses.Error, new[] { AutoPolicyMissing });

            if (cfg.IsNpwpRegistered != cfg.HasNpwpNumber)
                return TaxBlocked(
                    cfg, PayrollConfigurationStatuses.Configured, PayrollComponentResultStatuses.Error,
                    new[] { "Status NPWP pada profil pajak tidak konsisten (terdaftar tanpa nomor, atau nomor tanpa status terdaftar). Lengkapi sebelum PPh dihitung otomatis." });

            var input = new PayrollTaxInput(
                article, category, ptkp, cfg.IsNpwpRegistered && cfg.HasNpwpNumber, run.TaxYear, run.TaxMonth, run.IsLastTaxPeriod,
                run.TaxableGross, run.EmployerPremium, run.EmployeePension, run.Ytd, cfg.HasPreviousEmployer, cfg.TreatyRatePercent, cfg.TreatyBasis);
            var computation = PayrollTaxPolicyEngine.Compute(run.Policy, input);

            PayrollTaxAssessment WithTrace(PayrollTaxAssessment assessment) => assessment with
            {
                PolicyId = run.Policy.Id,
                PolicyLegalBasis = run.Policy.LegalBasis,
                PolicyVersion = run.Policy.Version,
                TaxYear = run.TaxYear,
                TaxMonth = run.TaxMonth,
                IsLastTaxPeriod = run.IsLastTaxPeriod,
                TaxableGross = run.TaxableGross,
                EmployerPremium = run.EmployerPremium,
                EmployeePension = run.EmployeePension,
                Detail = computation.Detail
            };

            if (!computation.IsValid)
                return WithTrace(TaxBlocked(cfg, PayrollConfigurationStatuses.Configured, PayrollComponentResultStatuses.Error, computation.Issues));

            var autoAmount = Round(computation.Amount);
            var autoKind = autoAmount > 0m ? PayrollComponentResultStatuses.Calculated : PayrollComponentResultStatuses.ValidZero;
            if (string.Equals(method, TaxMethodKinds.Net, StringComparison.OrdinalIgnoreCase))
            {
                return WithTrace(Result(
                    autoKind,
                    autoAmount > 0m ? TaxTreatments.EmployerBorne : TaxTreatments.None,
                    0m,
                    autoAmount,
                    autoAmount > 0m ? new[] { "PPh ditanggung pemberi kerja (metode Net); tidak mengurangi gaji bersih pegawai." } : null));
            }

            return WithTrace(Result(autoKind, autoAmount > 0m ? TaxTreatments.EmployeeDeduction : TaxTreatments.None, autoAmount, 0m));
        }

        // APPROVED_MANUAL_AMOUNT: dipakai hanya bila sudah disetujui pengguna lain (persetujuan tersendiri, bukan izin menetapkan nominal).
        if (!string.Equals(cfg.ManualAmountApprovalStatus, ManualAmountApprovalStatuses.Approved, StringComparison.Ordinal))
        {
            var statusText = cfg.ManualAmountApprovalStatus switch
            {
                ManualAmountApprovalStatuses.Pending => "masih menunggu persetujuan",
                ManualAmountApprovalStatuses.Rejected => "ditolak",
                _ => "belum diajukan untuk persetujuan"
            };
            return TaxBlocked(
                cfg, PayrollConfigurationStatuses.NotConfigured, PayrollComponentResultStatuses.NotConfigured,
                new[] { $"Nominal PPh manual {statusText}; hanya nominal yang sudah disetujui yang dipakai payroll." })
                with { ManualAmountApprovalStatus = cfg.ManualAmountApprovalStatus };
        }

        var amount = Round(cfg.ApprovedManualAmount ?? 0m);
        var resultKind = amount > 0m ? PayrollComponentResultStatuses.Calculated : PayrollComponentResultStatuses.ValidZero;
        if (string.Equals(method, TaxMethodKinds.Net, StringComparison.OrdinalIgnoreCase))
        {
            return Result(
                resultKind,
                amount > 0m ? TaxTreatments.EmployerBorne : TaxTreatments.None,
                0m,
                amount,
                amount > 0m ? new[] { "PPh ditanggung pemberi kerja (metode Net); tidak mengurangi gaji bersih pegawai." } : null)
                with { ManualAmountApprovalStatus = cfg.ManualAmountApprovalStatus };
        }

        return Result(resultKind, amount > 0m ? TaxTreatments.EmployeeDeduction : TaxTreatments.None, amount, 0m)
            with { ManualAmountApprovalStatus = cfg.ManualAmountApprovalStatus };
    }

    private static PayrollTaxAssessment TaxUnconfigured(PayrollTaxConfigFacts? cfg, IReadOnlyList<string> issues, decimal legacyAmount)
    {
        if (legacyAmount > 0m)
        {
            return new PayrollTaxAssessment(
                PayrollConfigurationStatuses.NotConfigured, PayrollComponentResultStatuses.Calculated,
                PayrollTaxInsuranceSources.LegacyProfileAmount, cfg?.Article, cfg?.RecipientCategory, cfg?.CalculationMode, cfg?.TaxMethod,
                null, cfg?.PolicyVersion, TaxTreatments.EmployeeDeduction, legacyAmount, 0m, false, issues,
                new[] { "Konfigurasi PPh HRD belum lengkap; nominal PPh memakai tetapan lama pada profil payroll. Lengkapi Konfigurasi PPh di HRD." },
                cfg?.Id, cfg?.EffectiveStartDate, cfg?.EffectiveEndDate, cfg?.DecisionBasis, cfg?.ExemptionReason);
        }

        return new PayrollTaxAssessment(
            PayrollConfigurationStatuses.NotConfigured, PayrollComponentResultStatuses.NotConfigured, PayrollTaxInsuranceSources.None,
            cfg?.Article, cfg?.RecipientCategory, cfg?.CalculationMode, cfg?.TaxMethod, null, cfg?.PolicyVersion, TaxTreatments.None,
            0m, 0m, true, issues, Array.Empty<string>(), cfg?.Id, cfg?.EffectiveStartDate, cfg?.EffectiveEndDate, cfg?.DecisionBasis,
            cfg?.ExemptionReason);
    }

    private static PayrollTaxAssessment TaxBlocked(
        PayrollTaxConfigFacts cfg, string configurationStatus, string resultStatus, IReadOnlyList<string> issues) =>
        new(configurationStatus, resultStatus, PayrollTaxInsuranceSources.None, cfg.Article, cfg.RecipientCategory, cfg.CalculationMode,
            cfg.TaxMethod, null, cfg.PolicyVersion, TaxTreatments.None, 0m, 0m, true, issues, Array.Empty<string>(), cfg.Id,
            cfg.EffectiveStartDate, cfg.EffectiveEndDate, cfg.DecisionBasis, cfg.ExemptionReason);

    // ------------------------------------------------------------------------------------------------------------------------
    // Iuran: validasi tulis
    // ------------------------------------------------------------------------------------------------------------------------

    /// <summary>Daftar program BPJS Ketenagakerjaan dari teks dipisah koma; kosong berarti semua program. Token tidak dikenal diabaikan.</summary>
    public static List<string> ParsePrograms(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return InsurancePrograms.All.ToList();
        return value.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => x.ToUpperInvariant())
            .Where(x => InsurancePrograms.All.Contains(x))
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// Validasi konfigurasi iuran pada saat HRD menyimpan. Status kepesertaan kosong berarti belum dikonfigurasi dan diterima
    /// (kompatibel dengan klien lama). Semua flag false bukan bukti tidak wajib: tidak ikut serta harus eksplisit dan berdasar.
    /// </summary>
    public static List<string> ValidateInsuranceConfiguration(
        string? status, string? basis, bool healthEnabled, bool employmentEnabled, string? programs,
        bool privateEnabled, decimal privateEmployee, decimal privateEmployer)
    {
        var errors = new List<string>();

        if (privateEmployee < 0m || privateEmployer < 0m) errors.Add("Kontribusi asuransi swasta tidak boleh negatif.");
        if (!privateEnabled && (privateEmployee > 0m || privateEmployer > 0m))
            errors.Add("Kontribusi asuransi swasta hanya boleh diisi bila asuransi swasta aktif.");

        if (!string.IsNullOrWhiteSpace(programs))
        {
            var tokens = programs.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (tokens.Any(t => !InsurancePrograms.All.Contains(t.ToUpperInvariant())))
                errors.Add("Program BPJS Ketenagakerjaan tidak valid. Gunakan JHT, JP, JKK, dan/atau JKM.");
            if (!employmentEnabled) errors.Add("Program BPJS Ketenagakerjaan hanya boleh diisi bila BPJS Ketenagakerjaan aktif.");
        }

        if (status is null) return errors;

        if (!InsuranceParticipationStatuses.All.Contains(status))
        {
            errors.Add("Status kepesertaan iuran tidak valid.");
            return errors;
        }

        if (status == InsuranceParticipationStatuses.Active)
        {
            if (!healthEnabled && !employmentEnabled && !privateEnabled)
                errors.Add("Peserta aktif harus memilih sedikitnya satu program iuran (BPJS Kesehatan, BPJS Ketenagakerjaan, atau asuransi swasta).");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(basis))
                errors.Add("Dasar wajib diisi bila tidak mengikuti program iuran atau tidak berlaku untuk jenis penerima.");
            if (healthEnabled || employmentEnabled || privateEnabled)
                errors.Add("Program iuran tidak boleh aktif bila status kepesertaan bukan peserta aktif.");
        }

        return errors;
    }

    // ------------------------------------------------------------------------------------------------------------------------
    // Iuran: penilaian per periode
    // ------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Menilai konfigurasi iuran satu orang dan, bila <paramref name="calculate"/>, menghitung kontribusi dari parameter iuran run.
    /// <paramref name="wageBase"/> adalah dasar upah (gaji pokok + tunjangan tetap) yang ditentukan pemanggil. Kontribusi pemberi kerja
    /// dipisahkan dan tidak pernah dimasukkan ke total pegawai. <paramref name="legacyAmount"/> adalah tetapan lama
    /// PrlPayroll.InsuranceAmount: cadangan bertanda bila HRD belum mengonfigurasi dan nilainya &gt; 0.
    /// Bila <paramref name="enableBpjs"/> bernilai false (mode pengujian), BPJS tidak dihitung dan tidak memblokir payroll run,
    /// dengan status eksplisit <see cref="PayrollComponentResultStatuses.TestingModeDisabled"/> (bukan NOT_APPLICABLE dan bukan VALID_ZERO).
    /// </summary>
    public static PayrollInsuranceAssessment AssessInsurance(
        PayrollInsuranceConfigFacts? cfg, decimal legacyAmount, PayrollInsurancePolicy? policy, decimal wageBase,
        DateTime periodStart, DateTime periodEnd, bool calculate = true, bool enableBpjs = true)
    {
        if (!enableBpjs) return AssessWithoutBpjs(cfg, wageBase, periodStart, periodEnd, calculate);

        legacyAmount = Round(legacyAmount);

        if (cfg is null) return InsuranceUnconfigured(null, new[] { "Profil iuran belum dikonfigurasi HRD." }, legacyAmount, wageBase);
        if (!cfg.IsActive) return InsuranceUnconfigured(cfg, new[] { "Profil iuran HRD tidak aktif." }, legacyAmount, wageBase);

        if (!OverlapsPeriod(cfg.EffectiveStartDate, cfg.EffectiveEndDate, periodStart, periodEnd))
        {
            var message = cfg.EffectiveStartDate.HasValue && cfg.EffectiveStartDate.Value.Date > periodEnd.Date
                ? $"Profil iuran tersedia tetapi baru berlaku mulai {Format(cfg.EffectiveStartDate)}; belum berlaku pada periode ini."
                : $"Profil iuran sudah berakhir pada {Format(cfg.EffectiveEndDate)}; tidak berlaku pada periode ini.";
            return InsuranceBlocked(cfg, PayrollConfigurationStatuses.NotEffective, PayrollComponentResultStatuses.NotConfigured, new[] { message }, wageBase);
        }

        var status = NormalizeCode(cfg.ParticipationStatus);
        if (status is null)
            return InsuranceUnconfigured(cfg, new[] { "Status kepesertaan iuran belum ditentukan HRD." }, legacyAmount, wageBase);

        var errors = ValidateInsuranceConfiguration(
            status, cfg.ParticipationBasis, cfg.HealthEnabled, cfg.EmploymentEnabled, cfg.EmploymentPrograms, cfg.PrivateEnabled,
            cfg.PrivateEmployeeContribution, cfg.PrivateEmployerContribution);
        if (errors.Count > 0)
            return InsuranceBlocked(cfg, PayrollConfigurationStatuses.Error, PayrollComponentResultStatuses.Error, errors, wageBase);

        var warnings = new List<string>();
        if (cfg.HasPerEmployeeRates)
            warnings.Add("Tarif iuran per pegawai pada profil diabaikan; perhitungan memakai tarif dan plafon dari kebijakan iuran BPJS yang disahkan dan berlaku pada periode.");

        if (status != InsuranceParticipationStatuses.Active)
        {
            return new PayrollInsuranceAssessment(
                PayrollConfigurationStatuses.Configured, PayrollComponentResultStatuses.NotApplicable, PayrollTaxInsuranceSources.HrdProfile,
                status, Array.Empty<string>(), policy?.Version, wageBase, 0m, 0m, Array.Empty<PayrollInsuranceLine>(), false,
                Array.Empty<string>(), warnings, cfg.Id);
        }

        var programs = cfg.EmploymentEnabled ? ParsePrograms(cfg.EmploymentPrograms) : new List<string>();
        if (!calculate)
        {
            return new PayrollInsuranceAssessment(
                PayrollConfigurationStatuses.Configured, PayrollComponentResultStatuses.Calculated, PayrollTaxInsuranceSources.HrdProfile,
                status, programs, policy?.Version, wageBase, 0m, 0m, Array.Empty<PayrollInsuranceLine>(), false, Array.Empty<string>(),
                warnings, cfg.Id);
        }

        var policyIssues = new List<string>();
        var needsPolicy = cfg.HealthEnabled || cfg.EmploymentEnabled;
        if (needsPolicy && policy is null)
            policyIssues.Add(
                "Belum ada kebijakan iuran BPJS berversi yang disahkan dan berlaku pada periode ini (Menunggu Verifikasi Kebijakan). "
                + "Nilai bawaan Pengaturan Penggajian tidak dipakai sebelum diverifikasi HRD dan disahkan Finance.");
        if (policy is not null)
        {
            if (cfg.HealthEnabled && policy.HealthCap <= 0m)
                policyIssues.Add($"Plafon upah BPJS Kesehatan pada kebijakan {policy.Version} belum diisi.");
            if (cfg.EmploymentEnabled && programs.Contains(InsurancePrograms.Jp) && policy.JpCap <= 0m)
                policyIssues.Add($"Plafon upah BPJS JP pada kebijakan {policy.Version} belum diisi.");
        }

        if (policyIssues.Count > 0)
            return InsuranceBlocked(cfg, PayrollConfigurationStatuses.Configured, PayrollComponentResultStatuses.Error, policyIssues, wageBase);

        var lines = new List<PayrollInsuranceLine>();
        void Add(string code, string name, string program, bool employeePaid, decimal baseAmount, decimal rate)
        {
            var amount = Round(baseAmount * rate / 100m);
            lines.Add(new PayrollInsuranceLine(code, name, program, employeePaid, baseAmount, rate, amount));
        }

        if (cfg.HealthEnabled && policy is not null)
        {
            var healthBase = Math.Min(wageBase, policy.HealthCap);
            Add(PayrollInsuranceComponentCodes.BpjsHealthEmployee, "Iuran BPJS Kesehatan (Pegawai)", "KES", true, healthBase, policy.HealthEmployeeRate);
            Add(PayrollInsuranceComponentCodes.BpjsHealthEmployer, "Kontribusi BPJS Kesehatan (Pemberi Kerja)", "KES", false, healthBase, policy.HealthEmployerRate);
        }

        if (cfg.EmploymentEnabled && policy is not null)
        {
            if (programs.Contains(InsurancePrograms.Jht))
            {
                Add(PayrollInsuranceComponentCodes.BpjsJhtEmployee, "Iuran BPJS TK JHT (Pegawai)", InsurancePrograms.Jht, true, wageBase, policy.JhtEmployeeRate);
                Add(PayrollInsuranceComponentCodes.BpjsJhtEmployer, "Kontribusi BPJS TK JHT (Pemberi Kerja)", InsurancePrograms.Jht, false, wageBase, policy.JhtEmployerRate);
            }

            if (programs.Contains(InsurancePrograms.Jp))
            {
                var jpBase = Math.Min(wageBase, policy.JpCap);
                Add(PayrollInsuranceComponentCodes.BpjsJpEmployee, "Iuran BPJS TK JP (Pegawai)", InsurancePrograms.Jp, true, jpBase, policy.JpEmployeeRate);
                Add(PayrollInsuranceComponentCodes.BpjsJpEmployer, "Kontribusi BPJS TK JP (Pemberi Kerja)", InsurancePrograms.Jp, false, jpBase, policy.JpEmployerRate);
            }

            if (programs.Contains(InsurancePrograms.Jkk))
                Add(PayrollInsuranceComponentCodes.BpjsJkkEmployer, "Kontribusi BPJS TK JKK (Pemberi Kerja)", InsurancePrograms.Jkk, false, wageBase, policy.JkkRate);
            if (programs.Contains(InsurancePrograms.Jkm))
                Add(PayrollInsuranceComponentCodes.BpjsJkmEmployer, "Kontribusi BPJS TK JKM (Pemberi Kerja)", InsurancePrograms.Jkm, false, wageBase, policy.JkmRate);
        }

        AddPrivateInsuranceLines(cfg, lines);

        var employeeTotal = Round(lines.Where(l => l.EmployeePaid).Sum(l => l.Amount));
        var employerTotal = Round(lines.Where(l => !l.EmployeePaid).Sum(l => l.Amount));
        var resultStatus = employeeTotal + employerTotal > 0m ? PayrollComponentResultStatuses.Calculated : PayrollComponentResultStatuses.ValidZero;
        return new PayrollInsuranceAssessment(
            PayrollConfigurationStatuses.Configured, resultStatus, PayrollTaxInsuranceSources.HrdProfile, status, programs, policy?.Version,
            wageBase, employeeTotal, employerTotal, lines, false, Array.Empty<string>(), warnings, cfg.Id)
        {
            PolicyId = policy?.PolicyId,
            PolicyLegalBasis = policy?.LegalBasis,
            JkkRiskClass = policy?.JkkRiskClass
        };
    }

    private static void AddPrivateInsuranceLines(PayrollInsuranceConfigFacts cfg, List<PayrollInsuranceLine> lines)
    {
        if (!cfg.PrivateEnabled) return;
        if (cfg.PrivateEmployeeContribution > 0m)
            lines.Add(new PayrollInsuranceLine(PayrollInsuranceComponentCodes.PrivateEmployee, "Iuran Asuransi Swasta (Pegawai)", "PRIVATE", true, 0m, 0m, Round(cfg.PrivateEmployeeContribution)));
        if (cfg.PrivateEmployerContribution > 0m)
            lines.Add(new PayrollInsuranceLine(PayrollInsuranceComponentCodes.PrivateEmployer, "Kontribusi Asuransi Swasta (Pemberi Kerja)", "PRIVATE", false, 0m, 0m, Round(cfg.PrivateEmployerContribution)));
    }

    /// <summary>
    /// Penilaian iuran saat fitur BPJS nonaktif. BPJS Kesehatan dan BPJS Ketenagakerjaan TIDAK dihitung dan tidak memblokir payroll run
    /// (status eksplisit TESTING_MODE_DISABLED, berbeda dari NOT_APPLICABLE, VALID_ZERO, dan belum dikonfigurasi). Asuransi swasta BUKAN
    /// BPJS dan tetap dihitung persis seperti biasa bila HRD menetapkannya (peserta aktif, program swasta aktif, berlaku pada periode):
    /// menonaktifkan BPJS tidak boleh menghilangkan potongan atau kontribusi asuransi swasta.
    /// </summary>
    private static PayrollInsuranceAssessment AssessWithoutBpjs(
        PayrollInsuranceConfigFacts? cfg, decimal wageBase, DateTime periodStart, DateTime periodEnd, bool calculate)
    {
        const string notice = "Mode pengujian aktif: kalkulasi BPJS Kesehatan dan BPJS Ketenagakerjaan dinonaktifkan sementara (belum dihitung).";

        var lines = new List<PayrollInsuranceLine>();
        var privateApplies = calculate
            && cfg is { IsActive: true, PrivateEnabled: true }
            && NormalizeCode(cfg.ParticipationStatus) == InsuranceParticipationStatuses.Active
            && OverlapsPeriod(cfg.EffectiveStartDate, cfg.EffectiveEndDate, periodStart, periodEnd);
        if (privateApplies) AddPrivateInsuranceLines(cfg!, lines);

        if (lines.Count == 0)
        {
            return new PayrollInsuranceAssessment(
                PayrollConfigurationStatuses.TestingModeDisabled,
                PayrollComponentResultStatuses.TestingModeDisabled,
                PayrollTaxInsuranceSources.TestingModeDisabled,
                cfg?.ParticipationStatus,
                Array.Empty<string>(),
                null,
                wageBase,
                0m,
                0m,
                Array.Empty<PayrollInsuranceLine>(),
                false,
                Array.Empty<string>(),
                new[] { notice },
                cfg?.Id);
        }

        var employeeTotal = Round(lines.Where(l => l.EmployeePaid).Sum(l => l.Amount));
        var employerTotal = Round(lines.Where(l => !l.EmployeePaid).Sum(l => l.Amount));
        return new PayrollInsuranceAssessment(
            PayrollConfigurationStatuses.TestingModeDisabled,
            PayrollComponentResultStatuses.Calculated,
            PayrollTaxInsuranceSources.HrdProfile,
            cfg!.ParticipationStatus,
            Array.Empty<string>(),
            null,
            wageBase,
            employeeTotal,
            employerTotal,
            lines,
            false,
            Array.Empty<string>(),
            new[] { notice, "Asuransi swasta tetap dihitung; hanya komponen BPJS yang tidak dihitung." },
            cfg.Id);
    }

    private static PayrollInsuranceAssessment InsuranceUnconfigured(
        PayrollInsuranceConfigFacts? cfg, IReadOnlyList<string> issues, decimal legacyAmount, decimal wageBase)
    {
        if (legacyAmount > 0m)
        {
            var legacyLine = new PayrollInsuranceLine(PayrollEngineComponentCodes.InsuranceEmployee, "Iuran Karyawan (tetapan lama profil payroll)", "LEGACY", true, 0m, 0m, legacyAmount);
            return new PayrollInsuranceAssessment(
                PayrollConfigurationStatuses.NotConfigured, PayrollComponentResultStatuses.Calculated,
                PayrollTaxInsuranceSources.LegacyProfileAmount, cfg?.ParticipationStatus, Array.Empty<string>(), null, wageBase,
                legacyAmount, 0m, new[] { legacyLine }, false, issues,
                new[] { "Konfigurasi iuran HRD belum lengkap; nominal iuran memakai tetapan lama pada profil payroll. Lengkapi Konfigurasi Iuran di HRD." },
                cfg?.Id);
        }

        return new PayrollInsuranceAssessment(
            PayrollConfigurationStatuses.NotConfigured, PayrollComponentResultStatuses.NotConfigured, PayrollTaxInsuranceSources.None,
            cfg?.ParticipationStatus, Array.Empty<string>(), null, wageBase, 0m, 0m, Array.Empty<PayrollInsuranceLine>(), true, issues,
            Array.Empty<string>(), cfg?.Id);
    }

    private static PayrollInsuranceAssessment InsuranceBlocked(
        PayrollInsuranceConfigFacts cfg, string configurationStatus, string resultStatus, IReadOnlyList<string> issues, decimal wageBase) =>
        new(configurationStatus, resultStatus, PayrollTaxInsuranceSources.None, cfg.ParticipationStatus, Array.Empty<string>(), null,
            wageBase, 0m, 0m, Array.Empty<PayrollInsuranceLine>(), true, issues, Array.Empty<string>(), cfg.Id);

    // ------------------------------------------------------------------------------------------------------------------------
    // Kesiapan (kelengkapan data dan Profil Penggajian)
    // ------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Kesiapan konfigurasi PPh dan iuran pada tanggal penilaian. Tidak melihat nominal hasil: yang dinilai hanya apakah HRD sudah
    /// menetapkan konfigurasi yang sah dan berlaku. Nol bukan penanda kelengkapan.
    /// </summary>
    public static PayrollTaxInsuranceReadiness AssessReadiness(
        PayrollTaxConfigFacts? tax, PayrollInsuranceConfigFacts? insurance, decimal legacyTax, decimal legacyInsurance, DateTime asOf,
        PayrollPolicyAvailability? availability = null)
    {
        // Tanpa informasi ketersediaan kebijakan, BPJS dianggap nonaktif (gagal-tertutup): konfigurasi BPJS tidak boleh dinilai/ditampilkan
        // seolah aktif hanya karena penilai tidak diberi tahu keadaan saklar.
        var enableBpjs = availability?.EnableBpjsInPayroll ?? false;
        var taxResult = AssessTax(tax, legacyTax, asOf, asOf);
        var insuranceResult = AssessInsurance(insurance, legacyInsurance, null, 0m, asOf, asOf, calculate: false, enableBpjs: enableBpjs);

        var notes = new List<string>();

        // Ketersediaan kebijakan berversi adalah keadaan terpisah dari konfigurasi per orang: konfigurasi lengkap tetap menahan
        // perhitungan bila kebijakan yang dibutuhkannya belum disahkan.
        if (availability is not null)
        {
            if (taxResult.IsConfigured && taxResult.CalculationMode == TaxCalculationModes.AutoRegulatory && !availability.TaxApproved)
                notes.Add("Belum ada kebijakan tarif PPh yang berlaku; PPh otomatis menahan payroll run.");
            if (enableBpjs && insuranceResult.IsConfigured && insuranceResult.Programs.Count + (insurance?.HealthEnabled == true ? 1 : 0) > 0 && !availability.BpjsApproved
                && insuranceResult.ParticipationStatus == InsuranceParticipationStatuses.Active)
                notes.Add($"{StatutoryPolicyLabels.AwaitingVerification}: belum ada kebijakan iuran BPJS yang disahkan dan berlaku; iuran menahan payroll run.");
        }

        if (taxResult.Source == PayrollTaxInsuranceSources.LegacyProfileAmount)
            notes.Add("PPh masih memakai tetapan lama pada profil payroll; konfigurasi HRD belum lengkap.");
        if (enableBpjs && insuranceResult.Source == PayrollTaxInsuranceSources.LegacyProfileAmount)
            notes.Add("Iuran masih memakai tetapan lama pada profil payroll; konfigurasi HRD belum lengkap.");
        notes.AddRange(insuranceResult.Warnings.Where(w => !w.Contains("tetapan lama", StringComparison.Ordinal)));
        if (taxResult.IsConfigured && insuranceResult.IsConfigured && notes.Count == 0)
            notes.Add("Konfigurasi pajak dan iuran lengkap.");

        return new PayrollTaxInsuranceReadiness(
            taxResult.ConfigurationStatus, insuranceResult.ConfigurationStatus, taxResult.Issues, insuranceResult.Issues, notes);
    }
}
