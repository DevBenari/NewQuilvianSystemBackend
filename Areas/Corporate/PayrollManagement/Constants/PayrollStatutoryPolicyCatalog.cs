namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;

// Kosakata kebijakan regulasi penggajian berversi: tarif PPh (PrlTaxPolicy) dan tarif/batas iuran BPJS (PrlBpjsPolicy). Kolom string
// tanpa check constraint: nilai dijaga validasi layanan kebijakan. Tidak ada tarif atau nominal regulasi di sini; angkanya hanya ada
// pada kebijakan yang disahkan.

/// <summary>
/// Siklus kebijakan regulasi: DRAFT (disusun HRD) → PENDING_APPROVAL (HRD menyatakan sudah diverifikasi terhadap dasar peraturan) →
/// APPROVED (Finance mengesahkan; hanya kebijakan ini yang dipakai mesin payroll) → RETIRED. REJECTED kembali dapat disunting.
/// Kebijakan yang sudah disahkan tidak pernah disunting: perubahan tarif dibuat sebagai versi baru supaya payroll historis tetap dapat
/// ditelusuri ke versi yang dipakainya.
/// </summary>
public static class StatutoryPolicyStatuses
{
    public const string Draft = "DRAFT";
    public const string PendingApproval = "PENDING_APPROVAL";
    public const string Approved = "APPROVED";
    public const string Rejected = "REJECTED";
    public const string Retired = "RETIRED";

    public static readonly IReadOnlyList<string> All = new[] { Draft, PendingApproval, Approved, Rejected, Retired };

    /// <summary>Hanya kebijakan DRAFT atau REJECTED yang boleh disunting isinya.</summary>
    public static bool IsEditable(string? status) => status is Draft or Rejected;
}

/// <summary>Kategori tarif efektif rata-rata (TER) pada kebijakan PPh. Pemetaan status PTKP ke kategori ada pada kebijakan, bukan di kode.</summary>
public static class TaxTerCategories
{
    public const string A = "A";
    public const string B = "B";
    public const string C = "C";

    public static readonly IReadOnlyList<string> All = new[] { A, B, C };
}

/// <summary>Jenis periode tabel TER: bulanan atau harian.</summary>
public static class TaxTerPeriodKinds
{
    public const string Monthly = "MONTHLY";
    public const string Daily = "DAILY";

    public static readonly IReadOnlyList<string> All = new[] { Monthly, Daily };
}

/// <summary>
/// Cara menerapkan tarif Pasal 17 untuk penerima bukan pegawai (tenaga ahli): per pembayaran atau atas penghasilan kumulatif tahun
/// berjalan. Dipilih dan disahkan pada kebijakan; tidak ditetapkan di kode.
/// </summary>
public static class TaxNonEmployeeMethods
{
    public const string PerPayment = "PER_PAYMENT";
    public const string Cumulative = "CUMULATIVE";

    public static readonly IReadOnlyList<string> All = new[] { PerPayment, Cumulative };
}

/// <summary>Status persetujuan nominal PPh manual (PrlTax.ManualAmountApprovalStatus). Terpisah dari izin menetapkan dan melihat nominal.</summary>
public static class ManualAmountApprovalStatuses
{
    public const string Pending = "PENDING";
    public const string Approved = "APPROVED";
    public const string Rejected = "REJECTED";

    public static readonly IReadOnlyList<string> All = new[] { Pending, Approved, Rejected };
}

/// <summary>Hasil klasifikasi satu komponen penghasilan terhadap dasar upah iuran BPJS.</summary>
public static class WageBaseClassifications
{
    public const string Included = "INCLUDED";
    public const string Excluded = "EXCLUDED";
    public const string Unclassified = "UNCLASSIFIED";
}


/// <summary>Status keberlakuan kebijakan tarif PPh yang ditentukan otomatis oleh sistem.</summary>
public static class TaxPolicyEffectiveStatuses
{
    public const string Active = "Berlaku";
    public const string Scheduled = "Terjadwal";
    public const string Expired = "Berakhir";
    public const string Inactive = "Nonaktif";

    public static readonly IReadOnlyList<string> All = new[] { Active, Scheduled, Expired, Inactive };
}

/// <summary>Jenis perhitungan pada formulir Kebijakan Tarif PPh 21 dan PPh 23.</summary>
public static class TaxPolicyCalculationTypes
{
    // Kategori PPh 21
    public const string PermanentEmployeeTer = "PERMANENT_EMPLOYEE_TER";
    public const string ContractEmployeeTer = "CONTRACT_EMPLOYEE_TER";
    public const string NonPermanentMonthly = "NON_PERMANENT_MONTHLY";
    public const string NonPermanentDaily = "NON_PERMANENT_DAILY";
    public const string NonEmployeeHonorarium = "NON_EMPLOYEE_HONORARIUM";
    public const string NonEmployeeExpert = "NON_EMPLOYEE_EXPERT";
    public const string Pph21Pasal17 = "PPH21_PASAL17";

    // Kategori dan Objek PPh 23
    public const string Pph23OutsourcingServices = "PPH23_OUTSOURCING_SERVICES";
    public const string Pph23ManagementConsulting = "PPH23_MANAGEMENT_CONSULTING";
    public const string Pph23TechnicalServices = "PPH23_TECHNICAL_SERVICES";
    public const string Pph23OtherServicesPmk141 = "PPH23_OTHER_SERVICES_PMK141";
    public const string Pph23RentAsset = "PPH23_RENT_ASSET";
    public const string Pph23RoyaltyInterest = "PPH23_ROYALTY_INTEREST";

    public static readonly IReadOnlyList<string> All = new[]
    {
        PermanentEmployeeTer,
        ContractEmployeeTer,
        NonPermanentMonthly,
        NonPermanentDaily,
        NonEmployeeHonorarium,
        NonEmployeeExpert,
        Pph21Pasal17,
        Pph23OutsourcingServices,
        Pph23ManagementConsulting,
        Pph23TechnicalServices,
        Pph23OtherServicesPmk141,
        Pph23RentAsset,
        Pph23RoyaltyInterest
    };
}

/// <summary>Label status kebijakan bagi layar (Pengaturan Penggajian dan kelengkapan).</summary>
public static class StatutoryPolicyLabels
{
    public const string AwaitingVerification = "Menunggu Verifikasi Kebijakan";
    public const string BpjsTestingModeDisabled = "Mode Pengujian — BPJS belum dihitung";
}
