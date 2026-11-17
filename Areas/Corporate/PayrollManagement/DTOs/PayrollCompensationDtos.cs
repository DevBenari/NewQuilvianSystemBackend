namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;

public sealed class PayrollCompensationQuery
{
    public string? Search { get; set; }

    /// <summary>Karyawan | Dokter.</summary>
    public string? PersonType { get; set; }

    /// <summary>
    /// Active | Suspended | OnHold | Terminated (PrlPayroll.PayrollStatus, hanya untuk profil yang BENAR-BENAR ada) atau
    /// NotPrepared (nilai virtual: orang HRD yang belum punya profil payroll; tidak pernah disimpan ke database). Kosong berarti semua.
    /// Active hanya mengembalikan profil aktif berstatus Active, bukan semua orang yang aktif di HRD.
    /// </summary>
    public string? PayrollStatus { get; set; }

    /// <summary>Siap | Bermasalah — kesiapan memproses penghasilan. Orang tanpa profil payroll selalu Bermasalah.</summary>
    public string? Readiness { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Ringkasan penghasilan satu penerima payroll (karyawan atau dokter): profil penggajian, penetapan gaji yang berlaku, rekening, dan
/// kelengkapan. Populasi berangkat dari HRD (pegawai dan dokter aktif), BUKAN dari profil payroll, sehingga orang yang belum punya
/// profil operasional tetap tampil dengan <see cref="PayrollProfileId"/> null. Identitas penerima tetap milik HRD
/// (WorkforceProfileId); modul ini tidak membuat identitas ganda. Nominal gaji hanya terisi bila pemanggil memiliki izin ViewAmount
/// (kolom nominal null bila tidak berwenang atau tidak tersedia, tidak pernah diganti nol); rekening dan NPWP selalu disamarkan.
///
/// Tiga hal berbeda dan tidak boleh dianggap sama: terdaftar di HRD (selalu benar untuk baris di sini), punya profil payroll operasional
/// (<see cref="HasPayrollProfile"/>), dan siap diproses (<see cref="IsReady"/>).
/// </summary>
public class PayrollCompensationListItemResponse
{
    public Guid WorkforceProfileId { get; set; }

    /// <summary>Null bila profil operasional payroll belum ada.</summary>
    public Guid? PayrollProfileId { get; set; }

    /// <summary>
    /// Profil operasional payroll (PrlPayroll) sudah ada, apa pun statusnya. Berbeda dari <c>HasPayrollProfile</c> pada Kelengkapan Data
    /// Payroll, yang bernilai benar hanya bila profil operasional (aktif, eligible, Active).
    /// </summary>
    public bool HasPayrollProfile { get; set; }

    /// <summary>Karyawan | Dokter.</summary>
    public string PersonType { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    /// <summary>Status pada profil yang ada (Active | Suspended | OnHold | Terminated); null bila profil belum ada. Tidak pernah dipalsukan menjadi Active.</summary>
    public string? PayrollStatus { get; set; }

    /// <summary>Eligible pada profil yang ada; false bila profil belum ada (bukan penilaian "tidak eligible").</summary>
    public bool IsPayrollEligible { get; set; }
    public string? PayrollGroupCode { get; set; }

    /// <summary>
    /// Siap | Akan disiapkan otomatis | Menunggu data HRD | Perlu perhatian. Kosakata dan aturan sama dengan Kelengkapan Data Payroll
    /// (PayrollReadinessEvaluator).
    /// </summary>
    public string PayrollProfileStatus { get; set; } = string.Empty;

    /// <summary>
    /// Profil payroll ada DAN operasional pada hari ini: aktif, eligible, berstatus Active, sedang berlaku (bukan masa depan atau sudah
    /// berakhir), dan tidak konflik dengan profil lain. False untuk orang tanpa profil, profil nonaktif, Suspended/OnHold/Terminated,
    /// atau profil yang belum/tidak lagi berlaku. Berbeda dari <see cref="HasPayrollProfile"/> (hanya keberadaan) dan dari
    /// <see cref="IsReady"/> (kesiapan memproses seluruhnya).
    /// </summary>
    public bool OperationalProfileReady { get; set; }

    /// <summary>
    /// Status penetapan gaji utama HRD: Belum Ada Penetapan Gaji | Menunggu Persetujuan HRD | Disetujui | Belum Berlaku |
    /// Sudah Berakhir | Konflik Penetapan Gaji. "Disetujui" hanya bila penetapan yang berlaku HARI INI sudah disetujui; penetapan masa
    /// depan atau yang sudah berakhir tidak pernah dianggap berlaku.
    /// </summary>
    public string SalaryStatus { get; set; } = string.Empty;

    /// <summary>Sumber HRD lengkap: gaji utama berlaku, disetujui, tidak konflik, dan rekening payroll terverifikasi.</summary>
    public bool HrdComplete { get; set; }

    /// <summary>Lengkap | Belum lengkap | Tidak aktif di HRD (hanya pada detail orang yang sudah tidak aktif).</summary>
    public string HrdDataStatus { get; set; } = string.Empty;

    /// <summary>Sudah ditetapkan | Belum ditetapkan (PPh dan iuran pada profil sama-sama nol) | Menunggu profil.</summary>
    public string TaxInsuranceStatus { get; set; } = string.Empty;

    public Guid? SalaryAssignmentId { get; set; }
    public DateTime? SalaryEffectiveStartDate { get; set; }
    public DateTime? SalaryEffectiveEndDate { get; set; }
    public bool SalaryApproved { get; set; }
    public DateTime? SalaryApprovedAt { get; set; }

    /// <summary>True bila pemanggil boleh melihat nominal (izin ViewAmount); bila false, kolom nominal di bawah null.</summary>
    public bool AmountsVisible { get; set; }
    public decimal? BaseSalary { get; set; }
    public decimal? TotalAllowance { get; set; }

    /// <summary>Terverifikasi | Belum terverifikasi | Belum ada.</summary>
    public string BankStatus { get; set; } = string.Empty;
    public bool HasTaxProfile { get; set; }
    public bool HasInsuranceProfile { get; set; }

    /// <summary>
    /// Siap diproses: sumber HRD lengkap, profil payroll operasional (aktif, eligible, Active), profil pajak ada, dan tetapan PPh dan
    /// iuran sudah ditetapkan (bukan sekadar nol bawaan). Orang tanpa profil payroll tidak pernah siap. Lebih ketat daripada
    /// <c>IsComplete</c> pada Kelengkapan Data Payroll, yang menganggap profil yang akan disiapkan otomatis cukup untuk ikut payroll run.
    /// </summary>
    public bool IsReady { get; set; }

    /// <summary>Penghalang kesiapan yang benar-benar ditemukan pada orang ini; kosong bila <see cref="IsReady"/>.</summary>
    public List<string> Issues { get; set; } = new();

    /// <summary>Catatan yang tidak menghalangi, mis. profil akan disiapkan otomatis saat payroll run dipersiapkan.</summary>
    public List<string> Notes { get; set; } = new();
}

/// <summary>
/// Ringkasan populasi Profil Penggajian (seluruh pegawai dan dokter aktif dari HRD, tidak dipengaruhi filter daftar). Tiga angka utama
/// punya definisi berbeda dan tidak boleh disamakan: total SDM HRD, profil payroll aktif (operasional), dan siap diproses.
/// </summary>
public sealed class PayrollCompensationSummaryResponse
{
    /// <summary>Pegawai dan dokter aktif di HRD.</summary>
    public int TotalPeople { get; set; }

    /// <summary>Orang yang profil payroll operasionalnya sudah ada (apa pun statusnya).</summary>
    public int WithPayrollProfile { get; set; }

    /// <summary>Orang dengan profil payroll aktif, eligible, dan berstatus Active (ikut payroll run).</summary>
    public int ActivePayrollProfiles { get; set; }

    /// <summary>Orang yang belum punya profil payroll (belum disiapkan). Selalu <c>TotalPeople - WithPayrollProfile</c>.</summary>
    public int NotPrepared { get; set; }

    /// <summary>Orang yang sumber HRD-nya lengkap.</summary>
    public int HrdComplete { get; set; }

    /// <summary>Orang yang siap diproses (<c>IsReady</c>); jumlah yang sama dengan filter "Siap" pada daftar.</summary>
    public int Ready { get; set; }
}

public sealed class PayrollCompensationSalaryHistoryResponse
{
    public Guid Id { get; set; }
    public string? SalaryStructureName { get; set; }
    public string? SalaryGradeName { get; set; }
    public DateTime EffectiveStartDate { get; set; }
    public DateTime? EffectiveEndDate { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; }
    public bool IsApproved { get; set; }
    public DateTime? ApprovedAt { get; set; }

    /// <summary>Null bila pemanggil tidak memiliki izin ViewAmount.</summary>
    public decimal? BaseSalary { get; set; }
}

public sealed class PayrollCompensationBankResponse
{
    public Guid Id { get; set; }
    public string? BankName { get; set; }

    /// <summary>Nomor rekening disamarkan (empat digit terakhir).</summary>
    public string AccountMasked { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;
    public bool IsPayrollAccount { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
}

/// <summary>Detail penghasilan satu penerima: ringkasan ditambah riwayat penetapan gaji dan rekening (disamarkan).</summary>
public sealed class PayrollCompensationDetailResponse : PayrollCompensationListItemResponse
{
    // Informasi operasional dari profil payroll; seluruhnya null bila profil belum ada.
    public string? PayrollNumber { get; set; }
    public string? PaymentFrequency { get; set; }
    public DateTime? PayrollProfileEffectiveStartDate { get; set; }
    public DateTime? PayrollProfileEffectiveEndDate { get; set; }

    // Penempatan organisasi primer HRD yang berlaku hari ini; null bila HRD belum mengisinya. Hanya dibaca.
    public string? OrganizationUnitName { get; set; }
    public string? DepartmentName { get; set; }
    public string? PositionName { get; set; }

    public string? TaxStatus { get; set; }
    public bool IsNpwpRegistered { get; set; }

    /// <summary>NPWP disamarkan (lima digit terakhir); nomor lengkap tidak dikirim ke layar.</summary>
    public string? NpwpMasked { get; set; }
    public List<PayrollCompensationSalaryHistoryResponse> SalaryHistory { get; set; } = new();
    public List<PayrollCompensationBankResponse> BankAccounts { get; set; } = new();
}
