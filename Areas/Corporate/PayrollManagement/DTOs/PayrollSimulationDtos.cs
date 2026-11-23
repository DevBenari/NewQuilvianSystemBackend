namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;

// PAY-BE-21 (blueprint payroll-gaji PGJ-C1, api-contract.md PayrollSimulationRequest/Response, PRL-DEC-32).
// Seluruh nominal di bawah ini sensitif dan tidak boleh masuk log.

public sealed class PayrollSimulationRequest
{
    public Guid PayrollPeriodId { get; set; }

    /// <summary>Opsional. Kosong = seluruh pegawai layak hitung di periode itu, sampai batas SimulationMaxEmployees.</summary>
    public List<Guid>? WorkforceProfileIds { get; set; }
}

public sealed class PayrollSimulationResponse
{
    public string PeriodCode { get; set; } = string.Empty;
    public string PolicyVersion { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }

    /// <summary>Jumlah dari pegawai yang berhasil dihitung saja; pegawai bergalat tidak ikut dijumlah.</summary>
    public decimal TotalGrossPay { get; set; }
    public decimal TotalDeduction { get; set; }
    public decimal TotalNetPay { get; set; }

    /// <summary>
    /// Sumber yang tidak ikut disimulasikan beserta alasannya (keputusan pemilik 2026-11-12, PAY-BE-21): impor HR
    /// (kehadiran, lembur, cuti) dan potongan cicilan piutang. Delta terhadap PGJ-C1.
    /// </summary>
    public List<string> Notes { get; set; } = new();

    /// <summary>
    /// Jenis simulasi: PAYROLL_ENGINE_SIMULATION = mesin Payroll Run resmi dijalankan pada sumber yang diimpor di bawah (bukan simulasi
    /// komponen pajak saja dan bukan simulasi seluruh payroll). Frontend tidak menghitung nominal apa pun.
    /// </summary>
    public string SimulationKind { get; set; } = "PAYROLL_ENGINE_SIMULATION";

    /// <summary>Sumber yang IKUT dihitung oleh simulasi ini.</summary>
    public List<string> IncludedSources { get; set; } = new();

    /// <summary>Sumber yang TIDAK ikut dihitung; hasil simulasi tidak boleh diklaim mencakupnya.</summary>
    public List<string> ExcludedSources { get; set; } = new();

    public List<PayrollSimulationEmployeeResponse> Employees { get; set; } = new();
}

public sealed class PayrollSimulationEmployeeResponse
{
    public Guid WorkforceProfileId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public decimal GrossPay { get; set; }

    /// <summary>Potongan sebelum cicilan piutang (pajak, iuran, potongan kehadiran/variabel).</summary>
    public decimal TotalDeduction { get; set; }
    public decimal NetPay { get; set; }

    public List<PayrollSimulationComponentResponse> Components { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();

    /// <summary>Rincian PPh dari snapshot pajak hasil mesin yang sama dengan Payroll Run resmi; null bila penerima gagal sebelum penilaian pajak.</summary>
    public PayrollSimulationTaxResponse? Tax { get; set; }
}

/// <summary>
/// Rincian PPh satu penerima pada simulasi. Seluruh nilai dibaca dari snapshot pajak yang dihasilkan mesin (tidak dihitung ulang di
/// layar maupun di sini). Status VALID_ZERO dan ERROR/NOT_CONFIGURED tetap dibedakan; nominal bernilai 0 pada status penahan bukan hasil.
/// </summary>
public sealed class PayrollSimulationTaxResponse
{
    public string ConfigurationStatus { get; set; } = string.Empty;
    public string ResultStatus { get; set; } = string.Empty;
    public string Article { get; set; } = string.Empty;
    public string RecipientCategory { get; set; } = string.Empty;
    public string CalculationMode { get; set; } = string.Empty;
    public string TaxMethod { get; set; } = string.Empty;
    public string? PtkpStatus { get; set; }
    public string? PolicyVersion { get; set; }
    public string Treatment { get; set; } = string.Empty;

    /// <summary>TER | ANNUAL_PASAL17 | NON_EMPLOYEE | PPH26_FLAT; kosong bila mesin tidak menghitung (manual/pengecualian/penahan).</summary>
    public string? CalculationBasis { get; set; }
    public string? TerCategory { get; set; }
    public decimal? TerRatePercent { get; set; }
    public decimal TaxableGross { get; set; }

    /// <summary>PPh yang dipotong dari pegawai (metode Gross).</summary>
    public decimal EmployeeAmount { get; set; }

    /// <summary>PPh yang ditanggung pemberi kerja (metode Net); tidak mengurangi gaji bersih.</summary>
    public decimal EmployerBorneAmount { get; set; }

    public bool YtdRequired { get; set; }
    public List<int> YtdCoveredMonths { get; set; } = new();
    public List<int> YtdMissingMonths { get; set; } = new();
    public List<int> OpeningBalanceMonths { get; set; } = new();
    public bool OpeningBalanceFeatureAvailable { get; set; }

    /// <summary>EXACT | UNDERWITHHELD_CATCH_UP | OVERWITHHELD pada masa pajak terakhir.</summary>
    public string? ReconciliationOutcome { get; set; }
    public decimal? AnnualTax { get; set; }
    public decimal? PriorTaxWithheld { get; set; }
    public decimal? OverwithholdingAmount { get; set; }

    public bool IsBlocking { get; set; }
    public List<string> Issues { get; set; } = new();
    public List<string> Warnings { get; set; } = new();

    /// <summary>Keterbatasan dasar hitung (mis. iuran BPJS tidak dihitung); hasil tidak boleh dinyatakan lengkap tanpa membacanya.</summary>
    public List<string> Limitations { get; set; } = new();
}

public sealed class PayrollSimulationComponentResponse
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsTaxable { get; set; }
}
