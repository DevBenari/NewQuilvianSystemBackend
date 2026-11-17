namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;

/// <summary>
/// Kelengkapan data payroll per pegawai dan dokter aktif. Berbeda dari Profil Penggajian (PayrollCompensation) yang bersumber dari
/// profil payroll, daftar ini bersumber dari POPULASI pegawai dan dokter (master HRD), sehingga orang yang belum punya profil tetap
/// tampil. Payroll hanya MEMBACA data HRD (penetapan gaji dan rekening); satu-satunya yang ditulis Payroll adalah profil payroll.
/// </summary>
public sealed class PayrollDataReadinessQuery
{
    public string? Search { get; set; }

    /// <summary>"Karyawan" atau "Dokter" (nilai sama dengan layar Profil Penggajian).</summary>
    public string? PersonType { get; set; }

    /// <summary>"Lengkap" atau "Belum lengkap"; kosong berarti semua.</summary>
    public string? Completeness { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class PayrollDataReadinessItemResponse
{
    public Guid WorkforceProfileId { get; set; }
    public string PersonType { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    /// <summary>Profil payroll ada, aktif, eligible, dan berstatus Active (milik Payroll).</summary>
    public bool HasPayrollProfile { get; set; }

    /// <summary>Ada penetapan gaji utama aktif yang sudah disetujui (data HRD, hanya dibaca).</summary>
    public bool HasApprovedSalaryAssignment { get; set; }

    /// <summary>Ada penetapan gaji utama aktif yang masih menunggu persetujuan HRD.</summary>
    public bool HasPendingSalaryAssignment { get; set; }

    /// <summary>"Belum ada", "Belum terverifikasi", atau "Terverifikasi" (rekening bertanda rekening payroll di HRD).</summary>
    public string BankStatus { get; set; } = string.Empty;

    public bool IsComplete { get; set; }
    public List<string> Issues { get; set; } = new();
}

public sealed class PayrollDataReadinessSummaryResponse
{
    public int TotalPeople { get; set; }
    public int WithPayrollProfile { get; set; }
    public int WithApprovedSalaryAssignment { get; set; }
    public int WithVerifiedBankAccount { get; set; }
    public int Complete { get; set; }
}

/// <summary>
/// Impor profil payroll: satu baris per orang membuat profil payroll bila belum ada. Struktur, grade, penetapan gaji, dan rekening
/// DIAMBIL dari data HRD (hanya dibaca); baris yang data HRD-nya belum lengkap ditolak. Semua atau tidak sama sekali: satu baris
/// bermasalah membatalkan seluruh impor. <see cref="DryRun"/> hanya memeriksa.
/// </summary>
public sealed class ImportPayrollDataRequest
{
    public bool DryRun { get; set; }
    public List<ImportPayrollDataRow> Rows { get; set; } = new();
}

public sealed class ImportPayrollDataRow
{
    /// <summary>Nomor pegawai (MstEmployee.EmployeeNumber) atau nomor dokter (MstDoctor.DoctorNumber).</summary>
    public string? PersonNumber { get; set; }

    /// <summary>Opsional. Kode Cost Center untuk laporan beban gaji.</summary>
    public string? CostCenterCode { get; set; }

    /// <summary>Tetapan PPh bulanan pada profil payroll (mesin tidak punya rumus tarif).</summary>
    public decimal TaxAmount { get; set; }

    /// <summary>Tetapan iuran karyawan (BPJS/asuransi) bulanan pada profil payroll.</summary>
    public decimal InsuranceAmount { get; set; }
}

public sealed class PayrollDataImportResult
{
    public bool DryRun { get; set; }
    public int RowCount { get; set; }
    public int PayrollProfilesCreated { get; set; }

    /// <summary>Profil yang sudah ada dan dilewati.</summary>
    public int PayrollProfilesSkipped { get; set; }

    /// <summary>Hasil per baris saat impor sah (atau uji kering lolos).</summary>
    public List<PayrollDataImportRowResult> Rows { get; set; } = new();

    /// <summary>Baris bermasalah saat impor dibatalkan (400). Kosong pada impor yang sah.</summary>
    public List<PayrollDataImportFailedRow> FailedRows { get; set; } = new();
}

public sealed class PayrollDataImportRowResult
{
    /// <summary>Nomor baris pada kiriman, mulai dari 1.</summary>
    public int RowNumber { get; set; }

    public string PersonNumber { get; set; } = string.Empty;
    public string PersonName { get; set; } = string.Empty;

    /// <summary>"Dibuat" atau "Sudah ada (dilewati)".</summary>
    public string PayrollProfile { get; set; } = string.Empty;

    /// <summary>Status penetapan gaji utama di HRD saat profil dibuat; kosong bila profil dilewati.</summary>
    public string SalaryAssignment { get; set; } = string.Empty;

    /// <summary>Status rekening payroll di HRD saat profil dibuat; kosong bila profil dilewati.</summary>
    public string BankAccount { get; set; } = string.Empty;

    public List<string> Warnings { get; set; } = new();
}

public sealed class PayrollDataImportFailedRow
{
    /// <summary>Nomor baris pada kiriman, mulai dari 1.</summary>
    public int RowNumber { get; set; }

    public string Reason { get; set; } = string.Empty;
}

public sealed class PayrollDataImportException(string message, PayrollDataImportResult result) : Exception(message)
{
    public PayrollDataImportResult Result { get; } = result;
}
