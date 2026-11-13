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
}

public sealed class PayrollSimulationComponentResponse
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsTaxable { get; set; }
}
