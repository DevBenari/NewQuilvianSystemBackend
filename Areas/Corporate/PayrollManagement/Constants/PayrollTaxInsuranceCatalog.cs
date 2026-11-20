namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;

// Kosakata konfigurasi PPh dan asuransi swasta yang dikelola HRD (PrlTax, PrlInsurance) dan hasilnya pada payroll run. Iuran BPJS tidak
// lagi dikelola; konstanta BPJS yang tersisa hanya dipertahankan untuk membaca data historis (lihat catatan pada tiap kelas). Kolom string tanpa
// check constraint: nilai dijaga validasi controller dan penilai (PayrollTaxInsuranceRules). Tidak ada tarif atau rumus di sini.

/// <summary>Jenis pasal perpajakan (PrlTax.TaxArticle, PrlTaxPolicy.TaxArticle). Dipisah dari metode pajak (Gross/GrossUp/Net) dan status PTKP.</summary>
public static class TaxArticles
{
    public const string Pph21 = "PPH21";
    public const string Pph23 = "PPH23";
    public const string Pph26 = "PPH26";

    /// <summary>Pasal PPh yang berlaku untuk profil pajak SDM (WfpTax / PrlTax).</summary>
    public static readonly IReadOnlyList<string> All = new[] { Pph21, Pph26 };

    /// <summary>Pasal perpajakan yang dikelola pada kebijakan regulasi (PrlTaxPolicy).</summary>
    public static readonly IReadOnlyList<string> StatutoryPolicyArticles = new[] { Pph21, Pph23 };
}

/// <summary>
/// Kategori penerima penghasilan (PrlTax.RecipientTaxCategory). Ditentukan HRD dari status hubungan kerja aktual, tidak diturunkan dari
/// jabatan atau jenis SDM (dokter tidak otomatis pegawai tetap). Kategori lain hanya ditambahkan bila kebijakan mendukungnya.
/// </summary>
public static class TaxRecipientCategories
{
    public const string PermanentEmployee = "PERMANENT_EMPLOYEE";
    public const string NonPermanentEmployee = "NON_PERMANENT_EMPLOYEE";
    public const string NonEmployeeExpert = "NON_EMPLOYEE_EXPERT";

    public static readonly IReadOnlyList<string> All = new[] { PermanentEmployee, NonPermanentEmployee, NonEmployeeExpert };
}

/// <summary>Skema perhitungan PPh (PrlTax.CalculationMode).</summary>
public static class TaxCalculationModes
{
    public const string AutoRegulatory = "AUTO_REGULATORY";
    public const string ApprovedManualAmount = "APPROVED_MANUAL_AMOUNT";
    public const string ExemptOrNotApplicable = "EXEMPT_OR_NOT_APPLICABLE";

    public static readonly IReadOnlyList<string> All = new[] { AutoRegulatory, ApprovedManualAmount, ExemptOrNotApplicable };
}

/// <summary>Metode pajak (PrlTax.TaxMethod). Bukan jenis PPh.</summary>
public static class TaxMethodKinds
{
    public const string Gross = "Gross";
    public const string GrossUp = "GrossUp";
    public const string Net = "Net";

    public static readonly IReadOnlyList<string> All = new[] { Gross, GrossUp, Net };
}

/// <summary>Status konfigurasi yang ditetapkan HRD (turunan dari isi profil, tidak disimpan).</summary>
public static class PayrollConfigurationStatuses
{
    public const string NotConfigured = "NOT_CONFIGURED";
    public const string Configured = "CONFIGURED";
    public const string NotEffective = "NOT_EFFECTIVE";
    public const string Error = "ERROR";

    /// <summary>Konfigurasi memang tidak diwajibkan (asuransi swasta opsional): bukan kekurangan data dan tidak menahan payroll.</summary>
    public const string NotRequired = "NOT_REQUIRED";
}

/// <summary>
/// Status hasil PPh atau asuransi pada satu penerima. VALID_ZERO, EXEMPT, dan NOT_APPLICABLE adalah nol yang sah dan berbeda dari
/// NOT_CONFIGURED (belum ada keputusan HRD) dan ERROR (konfigurasi ada tetapi tidak dapat dihitung secara sah). Nilai
/// TESTING_MODE_DISABLED pada snapshot run lama adalah data historis berupa teks biasa dan tidak lagi ditulis.
/// </summary>
public static class PayrollComponentResultStatuses
{
    public const string NotConfigured = "NOT_CONFIGURED";
    public const string Calculated = "CALCULATED";
    public const string ValidZero = "VALID_ZERO";
    public const string Exempt = "EXEMPT";
    public const string NotApplicable = "NOT_APPLICABLE";
    public const string Error = "ERROR";
}

/// <summary>Kepesertaan asuransi swasta (PrlInsurance.ParticipationStatus). Semua flag false bukan bukti tidak wajib; harus eksplisit.</summary>
public static class InsuranceParticipationStatuses
{
    public const string Active = "ACTIVE";
    public const string NotParticipating = "NOT_PARTICIPATING";
    public const string NotApplicable = "NOT_APPLICABLE";

    public static readonly IReadOnlyList<string> All = new[] { Active, NotParticipating, NotApplicable };
}

/// <summary>Asal nominal pajak/iuran pada hasil hitung.</summary>
public static class PayrollTaxInsuranceSources
{
    public const string HrdProfile = "HRD_PROFILE";
    public const string LegacyProfileAmount = "LEGACY_PROFILE_AMOUNT";
    public const string None = "NONE";
}

/// <summary>Perlakuan beban pajak pada hasil hitung.</summary>
public static class TaxTreatments
{
    public const string EmployeeDeduction = "EmployeeDeduction";
    public const string EmployerBorne = "EmployerBorne";
    public const string None = "None";
}

/// <summary>
/// Kode komponen asuransi milik mesin (awalan PAY_, sehingga komponen yang tidak dihasilkan lagi di-soft-delete saat hitung ulang).
/// Kode BPJS (PAY_INS_BPJS_*) HISTORIS: komponen run lama yang tetap terbaca di laporan; mesin tidak lagi membuatnya. Sisi
/// pegawai bertipe Insurance dan mengurangi gaji bersih; sisi pemberi kerja bertipe EmployerContribution, nominalnya di EmployerAmount,
/// dan TIDAK pernah mengurangi gaji bersih.
/// </summary>
public static class PayrollInsuranceComponentCodes
{
    public const string BpjsHealthEmployee = "PAY_INS_BPJS_KES_EE";
    public const string BpjsHealthEmployer = "PAY_INS_BPJS_KES_ER";
    public const string BpjsJhtEmployee = "PAY_INS_BPJS_JHT_EE";
    public const string BpjsJhtEmployer = "PAY_INS_BPJS_JHT_ER";
    public const string BpjsJpEmployee = "PAY_INS_BPJS_JP_EE";
    public const string BpjsJpEmployer = "PAY_INS_BPJS_JP_ER";
    public const string BpjsJkkEmployer = "PAY_INS_BPJS_JKK_ER";
    public const string BpjsJkmEmployer = "PAY_INS_BPJS_JKM_ER";
    public const string PrivateEmployee = "PAY_INS_PRIVATE_EE";
    public const string PrivateEmployer = "PAY_INS_PRIVATE_ER";

    public const string TypeInsurance = "Insurance";
    public const string TypeEmployerContribution = "EmployerContribution";
}

/// <summary>
/// Kode komponen yang dibuat jalur perhitungan lama (modul "Modern": PayrollTaxCalculationService dan layanan kalkulasi BPJS lama yang
/// sudah dihapus). Mesin payroll run resmi tidak memakainya; bila ada pada run yang sama, komponen itu dibuang supaya pajak dan iuran
/// tidak terpotong dua kali. Kode BPJS lama (DED_BPJS_*, ER_BPJS_*) SENGAJA dipertahankan di sini sebagai pengaman: tanpanya, komponen
/// lama pada run yang belum final akan terhitung sebagai potongan manual.
/// </summary>
public static class LegacyTaxInsuranceComponentCodes
{
    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        "DED_PPH21", "DED_BPJS_KES", "DED_BPJS_JHT", "DED_BPJS_JP",
        "ER_BPJS_KES", "ER_BPJS_JKK", "ER_BPJS_JKM", "ER_BPJS_JHT", "ER_BPJS_JP"
    };
}

/// <summary>
/// Kode komponen penghasilan yang dibuat jalur perhitungan lama (modul "Modern": PayrollEarningsCalculationService dan
/// PayrollOvertimeAndShiftIngestionService). Mesin payroll run resmi menghitung gaji pokok, tunjangan, dan lembur sendiri; komponen
/// ini bukan milik mesin (tanpa awalan PAY_) sehingga bila ikut terhitung sebagai penghasilan manual, penghasilan yang sama terhitung
/// dua kali. Karena itu mesin membuangnya saat menghitung run.
/// </summary>
public static class LegacyEarningComponentCodes
{
    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        "GP01", "TJ01", "TF01", "TR01", "OT01", "OT02", "SH01"
    };
}
