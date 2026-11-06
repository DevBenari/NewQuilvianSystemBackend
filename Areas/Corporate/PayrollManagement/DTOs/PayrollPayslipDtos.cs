namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;

public sealed class PayrollPayslipQuery
{
    /// <summary>Generated | Published | Downloaded | Superseded | Cancelled.</summary>
    public string? Status { get; set; }
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

/// <summary>Satu baris pada slip (penghasilan atau potongan). Disimpan dalam snapshot slip dan tidak pernah dihitung ulang.</summary>
public sealed class PayrollPayslipLine
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsProrated { get; set; }
}

/// <summary>
/// Isi slip yang dibekukan saat dibuat (immutable setelah diterbitkan). Hanya memuat data yang boleh dilihat pemilik slip: rekening
/// disamarkan dan NPWP tidak disertakan. Sengaja TIDAK memuat cap waktu pembuatan agar isi (dan checksum) identik pada pembuatan ulang.
/// </summary>
public sealed class PayrollPayslipSnapshot
{
    public string PayslipNumber { get; set; } = string.Empty;
    public string RunNumber { get; set; } = string.Empty;
    public string PeriodCode { get; set; } = string.Empty;
    public string PeriodName { get; set; } = string.Empty;
    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public string? PositionName { get; set; }
    public string? TaxStatus { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountMasked { get; set; }
    public string CurrencyCode { get; set; } = "IDR";
    public List<PayrollPayslipLine> Earnings { get; set; } = new();
    public List<PayrollPayslipLine> Deductions { get; set; } = new();
    public decimal TotalEarning { get; set; }
    public decimal TotalDeduction { get; set; }
    public decimal NetPay { get; set; }
    public string PolicyVersion { get; set; } = string.Empty;
}

public class PayslipSummaryResponse
{
    public Guid Id { get; set; }
    public Guid PayrollRunId { get; set; }
    public string RunNumber { get; set; } = string.Empty;
    public string PeriodCode { get; set; } = string.Empty;
    public string PayslipNumber { get; set; } = string.Empty;
    public int VersionNumber { get; set; }
    public string PayslipStatus { get; set; } = string.Empty;
    public bool IsEmployeeVisible { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;

    /// <summary>Gaji bersih; null bila pemanggil administrator tanpa izin ViewAmount (pemilik slip selalu melihat miliknya).</summary>
    public decimal? NetPay { get; set; }
    public DateTime? GeneratedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int DownloadCount { get; set; }
    public DateTime? FirstDownloadedAt { get; set; }
    public DateTime? LastDownloadedAt { get; set; }
}

public sealed class PayrollPayslipDetailResponse : PayslipSummaryResponse
{
    public string? FileChecksum { get; set; }
    public PayrollPayslipSnapshot? Snapshot { get; set; }
}

public sealed class PayrollPayslipGenerateResponse
{
    public Guid PayrollRunId { get; set; }
    public string RunNumber { get; set; } = string.Empty;
    public int TotalEmployees { get; set; }
    public int GeneratedCount { get; set; }
    public int RegeneratedCount { get; set; }

    /// <summary>Slip yang isinya identik (idempoten) sehingga tidak diubah.</summary>
    public int UnchangedCount { get; set; }

    /// <summary>Slip yang sudah diterbitkan: immutable, tidak dibuat ulang. Koreksi lewat run tambahan.</summary>
    public int SkippedPublishedCount { get; set; }
}

public sealed class PayrollPayslipPublishResponse
{
    public Guid PayrollRunId { get; set; }
    public string RunNumber { get; set; } = string.Empty;

    /// <summary>PaymentPaid | RunApproved (kebijakan akses slip, NEEDS CONFIRMATION).</summary>
    public string PublishTrigger { get; set; } = string.Empty;
    public int PublishedCount { get; set; }
    public int AlreadyPublishedCount { get; set; }
    public int HeldCount { get; set; }
    public List<string> HeldReasons { get; set; } = new();
}

/// <summary>Hasil unduhan slip: berkas PDF dibangun dari snapshot yang dibekukan, bukan dari data gaji yang berjalan.</summary>
public sealed class PayrollPayslipFile
{
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/pdf";
}
