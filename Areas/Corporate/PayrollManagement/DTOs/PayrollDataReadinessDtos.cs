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

    /// <summary>
    /// "Sudah ditetapkan", "Belum ditetapkan" (PPh dan iuran pada profil sama-sama nol; belum ada sumber kebijakan, bukan kekurangan
    /// HRD), atau "Menunggu profil". Tidak memengaruhi <see cref="IsComplete"/>.
    /// </summary>
    public string TaxInsuranceStatus { get; set; } = string.Empty;

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

    /// <summary>Orang dengan profil payroll yang tetapan PPh dan iurannya belum ditetapkan (keduanya nol).</summary>
    public int TaxInsuranceNotSet { get; set; }
}
