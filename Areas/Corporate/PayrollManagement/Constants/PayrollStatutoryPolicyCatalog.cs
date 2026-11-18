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

/// <summary>Label status kebijakan bagi layar (Pengaturan Penggajian dan kelengkapan).</summary>
public static class StatutoryPolicyLabels
{
    public const string AwaitingVerification = "Menunggu Verifikasi Kebijakan";
}
