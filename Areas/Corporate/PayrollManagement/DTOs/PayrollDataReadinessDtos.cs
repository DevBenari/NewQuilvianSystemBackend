namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;

/// <summary>
/// Kelengkapan data payroll per pegawai dan dokter aktif. Berbeda dari Profil Penggajian (PayrollCompensation) yang bersumber dari
/// profil payroll, daftar ini bersumber dari POPULASI pegawai dan dokter (master HRD), sehingga orang yang belum punya profil tetap
/// tampil. Payroll hanya MEMBACA data HRD (penetapan gaji, rekening, penempatan); profil operasional payroll (PrlPayroll) disiapkan
/// otomatis saat payroll run dipersiapkan, bukan lewat impor. Pembacaan di sini tidak pernah membuat atau mengubah data.
/// </summary>
public sealed class PayrollDataReadinessQuery
{
    public string? Search { get; set; }

    /// <summary>"Karyawan" atau "Dokter" (nilai sama dengan layar Profil Penggajian).</summary>
    public string? PersonType { get; set; }

    /// <summary>"Lengkap" atau "Belum lengkap"; kosong berarti semua. Mengikuti <c>IsComplete</c>.</summary>
    public string? Completeness { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

/// <summary>
/// Tiga hal yang dinilai terpisah: (1) kelengkapan sumber HRD (<see cref="HrdComplete"/>), (2) status profil operasional internal
/// Payroll (<see cref="PayrollProfileStatus"/>), dan (3) kesiapan mengikuti proses Payroll (<see cref="IsComplete"/>).
/// </summary>
public sealed class PayrollDataReadinessItemResponse
{
    public Guid WorkforceProfileId { get; set; }
    public string PersonType { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    /// <summary>Profil payroll ada, aktif, eligible, dan berstatus Active (milik Payroll). Sama dengan <c>PayrollProfileStatus = "Siap"</c>.</summary>
    public bool HasPayrollProfile { get; set; }

    /// <summary>Ada penetapan gaji utama aktif yang berlaku saat ini dan sudah disetujui (data HRD, hanya dibaca).</summary>
    public bool HasApprovedSalaryAssignment { get; set; }

    /// <summary>Ada penetapan gaji utama aktif yang berlaku saat ini dan masih menunggu persetujuan HRD.</summary>
    public bool HasPendingSalaryAssignment { get; set; }

    /// <summary>"Belum ada", "Belum terverifikasi", atau "Terverifikasi" (rekening payroll aktif yang berlaku di HRD).</summary>
    public string BankStatus { get; set; } = string.Empty;

    /// <summary>
    /// (1) Sumber HRD lengkap: penetapan gaji utama yang berlaku sudah disetujui dan rekening payroll sudah terverifikasi. Seluruh
    /// pemilik datanya HRD; Payroll tidak melengkapinya.
    /// </summary>
    public bool HrdComplete { get; set; }

    /// <summary>
    /// (2) Profil operasional internal Payroll: "Siap" (aktif dan ikut payroll run), "Akan disiapkan otomatis" (belum ada, dan akan
    /// dibuat saat payroll run dipersiapkan karena penetapan gaji dan rekening sudah ada di HRD), "Menunggu data HRD" (belum ada dan
    /// belum dapat disiapkan sampai penetapan gaji atau rekening ada di HRD), atau "Perlu perhatian" (profil ada tetapi tidak aktif,
    /// tidak eligible, atau bukan Active; tidak diaktifkan otomatis).
    /// </summary>
    public string PayrollProfileStatus { get; set; } = string.Empty;

    /// <summary>Status konfigurasi PPh: "Sudah ditetapkan" atau "Belum ditetapkan".</summary>
    public string TaxStatus { get; set; } = string.Empty;

    /// <summary>
    /// "Sudah ditetapkan" (konfigurasi PPh DAN iuran yang ditetapkan HRD sah dan berlaku) atau "Belum ditetapkan" (salah satunya belum
    /// ada, belum lengkap, belum/tidak berlaku, atau tidak sah). Diturunkan dari status konfigurasi eksplisit di bawah, BUKAN dari
    /// perbandingan nominal nol. Tidak memengaruhi <see cref="IsComplete"/>.
    /// </summary>
    public string TaxInsuranceStatus { get; set; } = string.Empty;

    /// <summary>NOT_CONFIGURED | CONFIGURED | NOT_EFFECTIVE | ERROR untuk konfigurasi PPh (Profil Pajak HRD).</summary>
    public string TaxConfigurationStatus { get; set; } = string.Empty;

    /// <summary>NOT_CONFIGURED | CONFIGURED | NOT_EFFECTIVE | ERROR untuk konfigurasi iuran (Profil Iuran HRD).</summary>
    public string InsuranceConfigurationStatus { get; set; } = string.Empty;

    /// <summary>Kekurangan konfigurasi PPh yang sebenarnya (mis. jenis PPh atau kategori belum ditentukan); kosong bila lengkap.</summary>
    public List<string> TaxConfigurationIssues { get; set; } = new();

    /// <summary>Kekurangan konfigurasi iuran yang sebenarnya; kosong bila lengkap.</summary>
    public List<string> InsuranceConfigurationIssues { get; set; } = new();

    /// <summary>
    /// (3) Siap mengikuti proses Payroll: tidak ada penghalang. Benar bila <see cref="HrdComplete"/> dan profil "Siap" atau "Akan
    /// disiapkan otomatis". Tidak mencakup tetapan PPh dan iuran (lihat <see cref="TaxInsuranceStatus"/>).
    /// </summary>
    public bool IsComplete { get; set; }

    /// <summary>Penghalang kesiapan; kosong bila <see cref="IsComplete"/>.</summary>
    public List<string> Issues { get; set; } = new();

    /// <summary>Catatan yang tidak menghalangi, mis. profil akan disiapkan otomatis atau tetapan PPh dan iuran belum ditetapkan.</summary>
    public List<string> Notes { get; set; } = new();
}

public sealed class PayrollDataReadinessSummaryResponse
{
    public int TotalPeople { get; set; }

    /// <summary>Orang dengan profil payroll "Siap".</summary>
    public int WithPayrollProfile { get; set; }

    public int WithApprovedSalaryAssignment { get; set; }
    public int WithVerifiedBankAccount { get; set; }

    /// <summary>Orang yang siap mengikuti proses Payroll (<c>IsComplete</c>); jumlah yang sama dengan filter "Lengkap" pada daftar.</summary>
    public int Complete { get; set; }

    /// <summary>Orang yang sumber HRD-nya lengkap.</summary>
    public int HrdComplete { get; set; }

    /// <summary>Orang yang profil payroll-nya akan disiapkan otomatis saat payroll run dipersiapkan.</summary>
    public int ProfilesToPrepare { get; set; }

    /// <summary>Orang yang profil payroll-nya ada tetapi tidak aktif, tidak eligible, atau bukan Active.</summary>
    public int ProfilesNeedAttention { get; set; }

    /// <summary>
    /// Orang yang konfigurasi PPh-nya di HRD belum ditetapkan secara sah dan berlaku (jenis PPh, kategori, atau skema
    /// perhitungan belum ditentukan, belum/tidak berlaku, atau tidak sah). Bukan perbandingan nominal nol.
    /// </summary>
    public int TaxNotSet { get; set; }

    /// <summary>
    /// Orang yang konfigurasi PPh dan/atau iurannya di HRD belum ditetapkan secara sah dan berlaku (jenis PPh, kategori, skema, atau
    /// kepesertaan iuran belum ditentukan, belum/tidak berlaku, atau tidak sah). Bukan perbandingan nominal nol.
    /// </summary>
    public int TaxInsuranceNotSet { get; set; }

    /// <summary>
    /// Ketersediaan kebijakan regulasi berversi yang disahkan dan berlaku hari ini. "Menunggu Verifikasi Kebijakan" berarti PPh otomatis
    /// dan iuran BPJS menahan payroll run walaupun konfigurasi per orang sudah lengkap.
    /// </summary>
    public bool TaxPolicyApproved { get; set; }
    public string? TaxPolicyVersion { get; set; }
    public string TaxPolicyStatusLabel { get; set; } = string.Empty;
    public bool EnableBpjsInPayroll { get; set; } = true;
    public bool BpjsPolicyApproved { get; set; }
    public string? BpjsPolicyVersion { get; set; }
    public string BpjsPolicyStatusLabel { get; set; } = string.Empty;
}
