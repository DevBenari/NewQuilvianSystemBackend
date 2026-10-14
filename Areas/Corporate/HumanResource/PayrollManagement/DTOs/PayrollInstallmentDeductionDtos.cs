namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;

/// <summary>
/// Respons eksekusi pemotongan angsuran piutang RS untuk satu pegawai di dalam payroll run (BE-FIN-110).
/// </summary>
public sealed class PayrollEmployeeInstallmentDeductionResponse
{
    public Guid PayrollRunEmployeeId { get; set; }
    public Guid PayrollRunId { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
    public Guid InstallmentId { get; set; }
    public string DeductionPeriod { get; set; } = string.Empty;
    public decimal DueAmount { get; set; }
    public decimal AvailableNetSalary { get; set; }
    public decimal MaxAllowedDeduction { get; set; }
    public decimal DeductedAmount { get; set; }
    public decimal RemainingUnpaidAmount { get; set; }
    public string Status { get; set; } = string.Empty; // BERHASIL, SEBAGIAN, GAGAL, NONE
    public bool IsIdempotentReplay { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// Ringkasan eksekusi pemotongan angsuran piutang RS untuk seluruh pegawai pada satu payroll run (BE-FIN-110).
/// </summary>
public sealed class PayrollRunInstallmentDeductionSummaryResponse
{
    public Guid PayrollRunId { get; set; }
    public string RunNumber { get; set; } = string.Empty;
    public string PeriodCode { get; set; } = string.Empty;
    public int TotalEmployeesProcessed { get; set; }
    public int TotalInstallmentsProcessed { get; set; }
    public decimal TotalDueAmount { get; set; }
    public decimal TotalDeductedAmount { get; set; }
    public int TotalSuccessCount { get; set; }
    public int TotalPartialCount { get; set; }
    public int TotalFailedCount { get; set; }
    public List<PayrollEmployeeInstallmentDeductionResponse> Items { get; set; } = new();
}

