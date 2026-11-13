namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;

public sealed class PayrollRecapQuery
{
    public Guid? PayrollPeriodId { get; set; }
    public Guid? PayrollRunId { get; set; }

    /// <summary>Karyawan | Dokter.</summary>
    public string? PersonType { get; set; }
    public Guid? CostCenterId { get; set; }
}

/// <summary>Satu baris kelompok pada rekap. Nominal null bila pemanggil tidak berwenang ViewAmount (jumlah penerima tetap tampil).</summary>
public sealed class PayrollRecapGroupResponse
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public decimal? GrossPay { get; set; }
    public decimal? TotalDeduction { get; set; }
    public decimal? NetPay { get; set; }
}

public sealed class PayrollRecapComponentResponse
{
    public string ComponentCode { get; set; } = string.Empty;
    public string ComponentName { get; set; } = string.Empty;

    /// <summary>Earning | Deduction | Tax | Insurance.</summary>
    public string ComponentType { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public decimal? Amount { get; set; }
}

public sealed class PayrollRecapRunResponse
{
    public Guid PayrollRunId { get; set; }
    public string RunNumber { get; set; } = string.Empty;
    public string RunStatus { get; set; } = string.Empty;
    public string PeriodCode { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public decimal? GrossPay { get; set; }
    public decimal? TotalDeduction { get; set; }
    public decimal? NetPay { get; set; }
    public decimal? PaidAmount { get; set; }
}

/// <summary>
/// Rekap penggajian teragregasi (periode, kategori penerima, pusat biaya, komponen). TIDAK memuat gaji per orang: hanya jumlah
/// penerima dan agregat. Nominal agregat hanya terisi bagi pemanggil dengan izin ViewAmount.
/// </summary>
public sealed class PayrollRecapResponse
{
    public bool AmountsVisible { get; set; }
    public int RunCount { get; set; }
    public int EmployeeCount { get; set; }
    public decimal? GrossPay { get; set; }
    public decimal? TotalDeduction { get; set; }
    public decimal? NetPay { get; set; }
    public decimal? PaidAmount { get; set; }
    public List<PayrollRecapRunResponse> Runs { get; set; } = new();
    public List<PayrollRecapGroupResponse> ByCategory { get; set; } = new();
    public List<PayrollRecapGroupResponse> ByCostCenter { get; set; } = new();
    public List<PayrollRecapComponentResponse> ByComponent { get; set; } = new();
}

public static class PayrollReconciliationStatuses
{
    public const string Ok = "OK";
    public const string Mismatch = "SELISIH";
    public const string Info = "INFO";
    public const string NotAvailable = "BELUM_TERSEDIA";
}

/// <summary>Satu pemeriksaan rekonsiliasi: nilai harapan, nilai aktual, selisih, dan penjelasannya.</summary>
public sealed class PayrollReconciliationCheckResponse
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal? Expected { get; set; }
    public decimal? Actual { get; set; }
    public decimal? Difference { get; set; }

    /// <summary>OK | SELISIH | INFO | BELUM_TERSEDIA (<see cref="PayrollReconciliationStatuses"/>).</summary>
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<string> Details { get; set; } = new();
}

/// <summary>
/// Rekonsiliasi satu run: total run = rincian penerima = rincian komponen = instruksi pembayaran = hasil bank = potongan piutang di
/// Finance, dan status event Accounting. Hanya baca. Setiap selisih menunjuk penerima/baris sumbernya pada <c>Details</c>.
/// </summary>
public sealed class PayrollReconciliationResponse
{
    public Guid PayrollRunId { get; set; }
    public string RunNumber { get; set; } = string.Empty;
    public string RunStatus { get; set; } = string.Empty;
    public string PeriodCode { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }

    /// <summary>OK bila tidak ada pemeriksaan berstatus SELISIH; selain itu ADA_SELISIH.</summary>
    public string OverallStatus { get; set; } = string.Empty;
    public int MismatchCount { get; set; }
    public List<PayrollReconciliationCheckResponse> Checks { get; set; } = new();
}

// ------------------------------------------------------------------------------------
// PAY-BE-22 (blueprint payroll-gaji PGJ-C1, PRL-DEC-27/33/41): laporan beban gaji per Cost Center dan laporan PPh/BPJS.
// Hanya dari run yang sudah disahkan. Seluruh nominal sensitif (PayrollReport : ViewAmount).
// ------------------------------------------------------------------------------------

/// <summary>Salah satu dari PayrollPeriodId atau PayrollRunId wajib diisi.</summary>
public sealed class PayrollExpenseSummaryQuery
{
    public Guid? PayrollPeriodId { get; set; }
    public Guid? PayrollRunId { get; set; }
    public Guid? CostCenterId { get; set; }
}

public sealed class PayrollExpenseSummaryRowResponse
{
    public string CostCenterCode { get; set; } = string.Empty;
    public string CostCenterName { get; set; } = string.Empty;
    public string ComponentCode { get; set; } = string.Empty;
    public string ComponentName { get; set; } = string.Empty;
    public string ComponentType { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public decimal Amount { get; set; }
}

public sealed class PayrollExpenseSummaryResponse
{
    public string PeriodCode { get; set; } = string.Empty;
    public List<string> RunNumbers { get; set; } = new();
    public List<PayrollExpenseSummaryRowResponse> Rows { get; set; } = new();

    /// <summary>Jumlah baris penghasilan (Earning, Benefit); sama dengan total bruto pegawai run.</summary>
    public decimal TotalEarning { get; set; }

    /// <summary>Jumlah baris potongan (Deduction, Tax, Insurance), termasuk cicilan; sama dengan total potongan pegawai run.</summary>
    public decimal TotalDeduction { get; set; }

    /// <summary>Keterangan laporan; berisi RPT-01 bila belum ada run yang disahkan.</summary>
    public string Note { get; set; } = string.Empty;
}

/// <summary>Salah satu dari PayrollPeriodId atau PayrollRunId wajib diisi.</summary>
public sealed class PayrollTaxContributionQuery
{
    public Guid? PayrollPeriodId { get; set; }
    public Guid? PayrollRunId { get; set; }

    /// <summary>Nama atau nomor pegawai.</summary>
    public string? Search { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class PayrollTaxContributionRowResponse
{
    public string RunNumber { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string CostCenterName { get; set; } = string.Empty;

    /// <summary>Komponen PAY_TAX run.</summary>
    public decimal TaxAmount { get; set; }

    /// <summary>Komponen PAY_INSURANCE_EMPLOYEE run.</summary>
    public decimal InsuranceEmployeeAmount { get; set; }

    /// <summary>Selalu TETAPAN_PROFIL selama rumus PPh/BPJS belum disahkan (PRL-DEC-33).</summary>
    public string Source { get; set; } = PayrollTaxContributionSources.ProfileFixed;

    /// <summary>Versi kebijakan dari snapshot kebijakan run.</summary>
    public string PolicyVersion { get; set; } = string.Empty;
}

/// <summary>PagedResult baku ditambah keterangan, supaya RPT-01 dapat disampaikan bersama daftar kosong.</summary>
public sealed class PayrollTaxContributionPagedResult : QuilvianSystemBackend.Responses.PagedResult<PayrollTaxContributionRowResponse>
{
    public string PeriodCode { get; set; } = string.Empty;
    public decimal TotalTaxAmount { get; set; }
    public decimal TotalInsuranceEmployeeAmount { get; set; }
    public string Note { get; set; } = string.Empty;
}

public static class PayrollTaxContributionSources
{
    public const string ProfileFixed = "TETAPAN_PROFIL";
}
