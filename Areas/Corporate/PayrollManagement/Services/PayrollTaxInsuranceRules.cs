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

/// <summary>
/// Fakta konfigurasi asuransi swasta satu orang (PrlInsurance). Kolom BPJS pada PrlInsurance (IsBpjsKesehatanEnabled, IsBpjsKetenagakerjaanEnabled,
/// BpjsEmploymentPrograms, tarif BPJS per pegawai) dipertahankan sebagai data historis dan sengaja TIDAK dibaca di sini.
/// </summary>
public sealed record PayrollInsuranceConfigFacts(
    Guid Id,
    bool IsActive,
    string? ParticipationStatus,
    string? ParticipationBasis,
    bool PrivateEnabled,
    decimal PrivateEmployeeContribution,
    decimal PrivateEmployerContribution,
    DateTime? EffectiveStartDate,
    DateTime? EffectiveEndDate)
{
    public static PayrollInsuranceConfigFacts From(PrlInsurance x) => new(
        x.Id, x.IsActive, x.ParticipationStatus, x.ParticipationBasis, x.IsPrivateInsuranceEnabled,
        x.PrivateInsuranceEmployeeContribution, x.PrivateInsuranceEmployerContribution,
        x.EffectiveStartDate, x.EffectiveEndDate);
}

/// <summary>
/// Ketersediaan kebijakan tarif PPh yang disahkan pada satu tanggal. Dipakai kelengkapan data untuk menyatakan "Menunggu Verifikasi
/// Kebijakan" sebagai keadaan terpisah dari konfigurasi per orang.
/// </summary>
public sealed record PayrollPolicyAvailability(bool TaxApproved, string? TaxVersion);

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

/// <summary>
/// Hasil penilaian asuransi swasta satu penerima pada satu periode. Tidak memuat komponen BPJS: iuran BPJS tidak lagi dihitung.
/// Tidak pernah menahan payroll run (asuransi swasta opsional), sehingga tidak ada penahan atau isu konfigurasi.
/// </summary>
public sealed record PayrollInsuranceAssessment(
    string ConfigurationStatus,
    string ResultStatus,
    string Source,
    string? ParticipationStatus,
    decimal EmployeeTotal,
    decimal EmployerTotal,
    IReadOnlyList<PayrollInsuranceLine> Lines,
    IReadOnlyList<string> Warnings,
    Guid? ConfigId)
{
    /// <summary>
    /// Bentuk snapshot disimpan pada PrlPayrollRunEmployee.InsuranceSnapshotJson. Kolom khusus BPJS pada snapshot (program, versi
    /// kebijakan, dasar upah, kelompok risiko JKK) dibiarkan kosong pada snapshot baru; snapshot lama tetap terbaca karena tipenya dipertahankan.
    /// </summary>
    public PayrollInsuranceSnapshot ToSnapshot() => new(
        Source, ConfigurationStatus, ResultStatus, ParticipationStatus, new List<string>(), null, 0m, EmployeeTotal,
        EmployerTotal,
        Lines.Select(l => new PayrollInsuranceSnapshotLine(l.Code, l.Program, l.EmployeePaid ? "Employee" : "Employer", l.BaseAmount, l.Rate, l.Amount)).ToList(),
        new List<string>(), Warnings.ToList(), false, ConfigId);
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

/// <summary>
/// DATA HISTORIS (LEGACY_PERSISTED): satu komponen penghasilan beserta klasifikasinya terhadap dasar upah iuran BPJS pada snapshot run lama.
/// Tipe dipertahankan hanya supaya snapshot lama tetap terbaca; mesin hitung tidak lagi mengisinya.
/// </summary>
public sealed record PayrollWageBaseItem(string Source, string Name, decimal Amount, string Classification, string? Reason);

public sealed record PayrollInsuranceSnapshotLine(string Code, string Program, string PaidBy, decimal BaseAmount, decimal Rate, decimal Amount);

/// <summary>
/// Snapshot asuransi yang disimpan pada PrlPayrollRunEmployee.InsuranceSnapshotJson. Snapshot run lama dapat memuat baris BPJS, program,
/// versi kebijakan, dasar upah, dan kelompok risiko JKK; tipe ini dipertahankan agar snapshot lama tetap terbaca (akumulasi PPh tahun berjalan
/// dan laporan historis). Snapshot baru hanya memuat baris asuransi swasta.
/// </summary>
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
    /// <summary>Historis: kebijakan iuran BPJS yang dipakai pada snapshot lama (id, dasar penetapan, kelompok risiko JKK). Tidak diisi snapshot baru.</summary>
    public Guid? PolicyId { get; init; }
    public string? PolicyLegalBasis { get; init; }
    public int? JkkRiskClass { get; init; }

    /// <summary>Historis: dasar upah iuran BPJS per komponen pada snapshot lama. Tidak diisi snapshot baru.</summary>
    public List<PayrollWageBaseItem>? WageBaseItems { get; init; }
}

/// <summary>Kesiapan konfigurasi PPh satu orang untuk layar kelengkapan data (tanpa periode run). Asuransi swasta dan iuran BPJS tidak menjadi syarat.</summary>
public sealed record PayrollTaxReadiness(
    string TaxConfigurationStatus,
    IReadOnlyList<string> TaxIssues,
    IReadOnlyList<string> Notes)
{
    public bool IsComplete => TaxConfigurationStatus == PayrollConfigurationStatuses.Configured;
}

/// <summary>
/// Satu-satunya tempat aturan konfigurasi PPh dan asuransi swasta yang dikelola HRD (PrlTax, PrlInsurance): validasi tulis (dipakai
/// controller), penilaian per periode (dipakai mesin payroll run), dan kesiapan PPh (dipakai kelengkapan data dan Profil Penggajian).
/// Murni, tanpa akses data.
///
/// Prinsip yang dijaga:
/// - Jenis PPh, kategori penerima, metode pajak, dan skema hitung adalah konsep terpisah; semuanya eksplisit dari HRD.
/// - Nol bukan penentu kelengkapan. NOT_CONFIGURED (belum ada keputusan HRD) berbeda dari VALID_ZERO/EXEMPT/NOT_APPLICABLE (nol yang
///   sah dengan dasar) dan ERROR (konfigurasi ada tetapi tidak dapat dihitung secara sah).
/// - Tidak ada tarif atau angka regulasi di sini. Skema AUTO_REGULATORY dihitung oleh PayrollTaxPolicyEngine dari kebijakan tarif PPh
///   berversi yang disahkan (PrlTaxPolicy); tanpa kebijakan yang berlaku atau untuk kombinasi tanpa formula yang sah (pegawai tidak
///   tetap, GrossUp) ia menghasilkan blocker, bukan Rp0. Nominal manual hanya dipakai bila sudah disetujui pengguna lain; pengecualian
///   wajib berdasar.
/// - Iuran BPJS Kesehatan dan BPJS Ketenagakerjaan TIDAK dihitung, tidak divalidasi, dan tidak menjadi syarat kelengkapan (PAY-BE-FIX-007).
///   Asuransi swasta tetap dihitung dari profil HRD dan bersifat opsional.
/// - Tetapan PPh lama pada profil payroll (PrlPayroll.TaxAmount) hanya cadangan bila HRD belum mengonfigurasi; ditandai sebagai belum
///   dikonfigurasi dan tidak pernah mengalahkan konfigurasi HRD. Tetapan iuran lama (PrlPayroll.InsuranceAmount) tidak dipakai.
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
        "Kalkulasi PPh pada jalur lama dinonaktifkan: tarifnya tertanam atau berasal dari nilai bawaan yang belum diverifikasi. "
        + "Gunakan Payroll > Hitung Payroll Run, yang memakai kebijakan tarif PPh berversi yang sudah disahkan.";

    /// <summary>
    /// Peringatan metode Net: PPh yang ditanggung pemberi kerja pada dasarnya penghasilan tambahan bagi pegawai (tunjangan pajak) kecuali
    /// ada ketentuan yang mengecualikan (mis. PPh DTP). Mesin ini TIDAK menambahkannya ke bruto kena pajak (itu butuh perhitungan berulang
    /// seperti Gross-Up yang belum didukung), sehingga hasil Net belum dapat dinyatakan sesuai regulasi tanpa keputusan pemilik pajak.
    /// </summary>
    public const string NetEmployerBorneWarning =
        "PPh ditanggung pemberi kerja (metode Net); tidak mengurangi gaji bersih pegawai. Perlakuan PPh yang ditanggung sebagai penghasilan kena pajak "
        + "pegawai (tunjangan pajak) atau PPh DTP belum diverifikasi dan tidak ditambahkan ke bruto kena pajak; tetapkan perlakuannya sebelum payroll disahkan.";

    public const string LegacyPathDisabledMessage =
        "Jalur perhitungan payroll lama dinonaktifkan. Payroll Run resmi (Payroll > Payroll Run) adalah satu-satunya pemilik hasil gaji.";

    public const string LegacyEngineBlockedMessage =
        "Payroll run ini dihitung oleh mesin Payroll Run resmi (Payroll > Hitung Payroll Run). Jalur perhitungan lama tidak boleh menghitung "
        + "ulang pajak atau iuran pada run yang sama karena akan menggandakan potongan dan menimpa total.";

    private static readonly string[] EmployeeCategories =
        { TaxRecipientCategories.PermanentEmployee, TaxRecipientCategories.NonPermanentEmployee };

    /// <summary>
    /// True bila run sudah dihitung oleh mesin Payroll Run resmi (snapshot kebijakan memuat versi mesinnya). Jalur lama (modul "Modern":
    /// PayrollTaxCalculationService dan layanan kalkulasi lama lainnya) menimpa total pegawai dan membuat komponen pajak/iuran sendiri,
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
            if (IsPtkpRelevant(article, category) && string.IsNullOrWhiteSpace(ptkp))
                return TaxBlocked(
                    cfg,
                    PayrollConfigurationStatuses.NotConfigured,
                    PayrollComponentResultStatuses.NotConfigured,
                    new[] { "Status PTKP pegawai belum ditetapkan di HRD; wajib diisi agar PPh 21 otomatis dapat dihitung." });

            // Kesiapan/ringkasan tanpa periode: yang dinilai hanya konfigurasi orangnya; ketersediaan kebijakan dilaporkan terpisah.
            if (run is null)
            {
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
            var engineWarnings = ExtractEngineWarnings(computation.Detail);
            if (string.Equals(method, TaxMethodKinds.Net, StringComparison.OrdinalIgnoreCase))
            {
                return WithTrace(Result(
                    autoKind,
                    autoAmount > 0m ? TaxTreatments.EmployerBorne : TaxTreatments.None,
                    0m,
                    autoAmount,
                    autoAmount > 0m ? engineWarnings.Append(NetEmployerBorneWarning).ToArray() : engineWarnings));
            }

            return WithTrace(Result(
                autoKind, autoAmount > 0m ? TaxTreatments.EmployeeDeduction : TaxTreatments.None, autoAmount, 0m, engineWarnings));
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

        // PAY-BE-FIX-015: konfigurasi lama yang disetujui tanpa batas masa berlaku tetap dipakai (tidak menahan payroll yang sudah berjalan),
        // tetapi diberi peringatan: nominal yang sama akan terpakai lagi pada periode berikutnya sampai tanggal berakhir diisi.
        var manualWarnings = new List<string>();
        if (!cfg.EffectiveEndDate.HasValue)
            manualWarnings.Add("Nominal PPh manual ini tidak dibatasi tanggal berakhir, sehingga akan dipakai lagi pada periode berikutnya. Isi tanggal berakhir yang mencakup masa pajak yang dimaksud (perubahan memerlukan persetujuan ulang).");

        if (string.Equals(method, TaxMethodKinds.Net, StringComparison.OrdinalIgnoreCase))
        {
            if (amount > 0m) manualWarnings.Add(NetEmployerBorneWarning);
            return Result(
                resultKind,
                amount > 0m ? TaxTreatments.EmployerBorne : TaxTreatments.None,
                0m,
                amount,
                manualWarnings.Count > 0 ? manualWarnings.ToArray() : null)
                with { ManualAmountApprovalStatus = cfg.ManualAmountApprovalStatus };
        }

        return Result(
            resultKind, amount > 0m ? TaxTreatments.EmployeeDeduction : TaxTreatments.None, amount, 0m,
            manualWarnings.Count > 0 ? manualWarnings.ToArray() : null)
            with { ManualAmountApprovalStatus = cfg.ManualAmountApprovalStatus };
    }

    /// <summary>Peringatan tidak menahan yang dicatat mesin pada rincian hitung (kunci "warnings"); kosong bila tidak ada.</summary>
    private static string[] ExtractEngineWarnings(IReadOnlyDictionary<string, object?> detail) =>
        detail.TryGetValue("warnings", out var value) && value is IEnumerable<string> list
            ? list.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray()
            : Array.Empty<string>();

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

    /// <summary>
    /// Validasi konfigurasi asuransi swasta pada saat HRD menyimpan (PrlInsurance). Status kepesertaan kosong berarti belum dikonfigurasi dan
    /// diterima (kompatibel dengan klien lama). Iuran BPJS tidak lagi dikelola di sini: kolom BPJS pada PrlInsurance hanya data historis
    /// dan tidak divalidasi, dibaca, maupun diubah oleh jalur Payroll.
    /// </summary>
    public static List<string> ValidateInsuranceConfiguration(
        string? status, string? basis, bool privateEnabled, decimal privateEmployee, decimal privateEmployer)
    {
        var errors = new List<string>();

        if (privateEmployee < 0m || privateEmployer < 0m) errors.Add("Kontribusi asuransi swasta tidak boleh negatif.");
        if (!privateEnabled && (privateEmployee > 0m || privateEmployer > 0m))
            errors.Add("Kontribusi asuransi swasta hanya boleh diisi bila asuransi swasta aktif.");

        if (status is null) return errors;

        if (!InsuranceParticipationStatuses.All.Contains(status))
        {
            errors.Add("Status kepesertaan asuransi tidak valid.");
            return errors;
        }

        if (status == InsuranceParticipationStatuses.Active)
        {
            if (!privateEnabled)
                errors.Add("Peserta aktif harus mengaktifkan asuransi swasta. Bila tidak ada asuransi swasta, pilih status tidak ikut serta atau tidak berlaku beserta dasarnya.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(basis))
                errors.Add("Dasar wajib diisi bila tidak mengikuti asuransi swasta atau tidak berlaku untuk jenis penerima.");
            if (privateEnabled)
                errors.Add("Asuransi swasta tidak boleh aktif bila status kepesertaan bukan peserta aktif.");
        }

        return errors;
    }

    /// <summary>
    /// Status konfigurasi profil asuransi swasta untuk layar HRD (Profil Asuransi), tanpa menghitung nominal dan tanpa memengaruhi
    /// kelengkapan Payroll. NOT_CONFIGURED | CONFIGURED | NOT_EFFECTIVE | ERROR beserta alasannya.
    /// </summary>
    public static (string ConfigurationStatus, IReadOnlyList<string> Issues) AssessInsuranceConfiguration(
        PayrollInsuranceConfigFacts cfg, DateTime asOf)
    {
        if (!cfg.IsActive)
            return (PayrollConfigurationStatuses.NotConfigured, new[] { "Profil asuransi HRD tidak aktif." });

        if (!OverlapsPeriod(cfg.EffectiveStartDate, cfg.EffectiveEndDate, asOf, asOf))
        {
            var message = cfg.EffectiveStartDate.HasValue && cfg.EffectiveStartDate.Value.Date > asOf.Date
                ? $"Profil asuransi tersedia tetapi baru berlaku mulai {Format(cfg.EffectiveStartDate)}; belum berlaku pada periode ini."
                : $"Profil asuransi sudah berakhir pada {Format(cfg.EffectiveEndDate)}; tidak berlaku pada periode ini.";
            return (PayrollConfigurationStatuses.NotEffective, new[] { message });
        }

        var status = NormalizeCode(cfg.ParticipationStatus);
        if (status is null)
            return (PayrollConfigurationStatuses.NotConfigured, new[] { "Status kepesertaan asuransi belum ditentukan HRD." });

        var errors = ValidateInsuranceConfiguration(
            status, cfg.ParticipationBasis, cfg.PrivateEnabled, cfg.PrivateEmployeeContribution, cfg.PrivateEmployerContribution);
        return errors.Count > 0
            ? (PayrollConfigurationStatuses.Error, errors)
            : (PayrollConfigurationStatuses.Configured, Array.Empty<string>());
    }

    // ------------------------------------------------------------------------------------------------------------------------
    // Asuransi swasta: penilaian per periode
    // ------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Menilai asuransi swasta satu orang pada periode. Satu-satunya sumber nominal adalah profil asuransi swasta HRD (peserta ACTIVE,
    /// program swasta aktif, berlaku pada periode). Kontribusi pemberi kerja dipisahkan dan tidak pernah dimasukkan ke total pegawai.
    ///
    /// Tidak ada kalkulasi iuran BPJS, tidak ada kebijakan tarif BPJS, dan tidak ada dasar upah iuran: kolom BPJS pada PrlInsurance
    /// adalah data historis yang tidak dibaca. Asuransi swasta bukan syarat kelengkapan: ketiadaannya adalah NOT_APPLICABLE yang sah dan
    /// tidak pernah menahan payroll run. Tetapan lama PrlPayroll.InsuranceAmount tidak dipakai (perilaku sama dengan saat BPJS
    /// dinonaktifkan sementara).
    /// </summary>
    public static PayrollInsuranceAssessment AssessInsurance(PayrollInsuranceConfigFacts? cfg, DateTime periodStart, DateTime periodEnd)
    {
        var lines = new List<PayrollInsuranceLine>();
        var privateApplies = cfg is { IsActive: true, PrivateEnabled: true }
            && NormalizeCode(cfg.ParticipationStatus) == InsuranceParticipationStatuses.Active
            && OverlapsPeriod(cfg.EffectiveStartDate, cfg.EffectiveEndDate, periodStart, periodEnd);
        if (privateApplies) AddPrivateInsuranceLines(cfg!, lines);

        if (lines.Count == 0)
        {
            return new PayrollInsuranceAssessment(
                PayrollConfigurationStatuses.NotRequired,
                PayrollComponentResultStatuses.NotApplicable,
                PayrollTaxInsuranceSources.None,
                cfg?.ParticipationStatus,
                0m,
                0m,
                Array.Empty<PayrollInsuranceLine>(),
                Array.Empty<string>(),
                cfg?.Id);
        }

        var employeeTotal = Round(lines.Where(l => l.EmployeePaid).Sum(l => l.Amount));
        var employerTotal = Round(lines.Where(l => !l.EmployeePaid).Sum(l => l.Amount));
        return new PayrollInsuranceAssessment(
            PayrollConfigurationStatuses.Configured,
            PayrollComponentResultStatuses.Calculated,
            PayrollTaxInsuranceSources.HrdProfile,
            cfg!.ParticipationStatus,
            employeeTotal,
            employerTotal,
            lines,
            Array.Empty<string>(),
            cfg.Id);
    }

    private static void AddPrivateInsuranceLines(PayrollInsuranceConfigFacts cfg, List<PayrollInsuranceLine> lines)
    {
        if (!cfg.PrivateEnabled) return;
        if (cfg.PrivateEmployeeContribution > 0m)
            lines.Add(new PayrollInsuranceLine(PayrollInsuranceComponentCodes.PrivateEmployee, "Iuran Asuransi Swasta (Pegawai)", "PRIVATE", true, 0m, 0m, Round(cfg.PrivateEmployeeContribution)));
        if (cfg.PrivateEmployerContribution > 0m)
            lines.Add(new PayrollInsuranceLine(PayrollInsuranceComponentCodes.PrivateEmployer, "Kontribusi Asuransi Swasta (Pemberi Kerja)", "PRIVATE", false, 0m, 0m, Round(cfg.PrivateEmployerContribution)));
    }

    // ------------------------------------------------------------------------------------------------------------------------
    // Kesiapan (kelengkapan data dan Profil Penggajian)
    // ------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Kesiapan konfigurasi PPh pada tanggal penilaian. Tidak melihat nominal hasil: yang dinilai hanya apakah HRD sudah menetapkan
    /// konfigurasi yang sah dan berlaku. Nol bukan penanda kelengkapan. Asuransi swasta dan iuran BPJS tidak menjadi syarat kelengkapan.
    /// </summary>
    public static PayrollTaxReadiness AssessReadiness(
        PayrollTaxConfigFacts? tax, decimal legacyTax, DateTime asOf, PayrollPolicyAvailability? availability = null)
    {
        var taxResult = AssessTax(tax, legacyTax, asOf, asOf);

        var notes = new List<string>();

        // Ketersediaan kebijakan berversi adalah keadaan terpisah dari konfigurasi per orang: konfigurasi lengkap tetap menahan
        // perhitungan bila kebijakan yang dibutuhkannya belum disahkan.
        if (availability is not null
            && taxResult.IsConfigured && taxResult.CalculationMode == TaxCalculationModes.AutoRegulatory && !availability.TaxApproved)
            notes.Add("Belum ada kebijakan tarif PPh yang berlaku; PPh otomatis menahan payroll run.");

        if (taxResult.Source == PayrollTaxInsuranceSources.LegacyProfileAmount)
            notes.Add("PPh masih memakai tetapan lama pada profil payroll; konfigurasi HRD belum lengkap.");
        if (taxResult.IsConfigured && notes.Count == 0)
            notes.Add("Konfigurasi PPh lengkap.");

        return new PayrollTaxReadiness(taxResult.ConfigurationStatus, taxResult.Issues, notes);
    }
}
