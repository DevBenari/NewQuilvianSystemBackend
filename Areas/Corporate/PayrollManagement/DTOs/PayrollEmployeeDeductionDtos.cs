using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;

public sealed class PayrollEmployeeDeductionQuery
{
    public Guid? PayrollPeriodId { get; set; }
    public Guid? PayrollRunId { get; set; }

    /// <summary>Salah satu <see cref="PayrollInstallmentProcessStatuses"/> (BELUM_DIHITUNG, DIRENCANAKAN, TERKONFIRMASI).</summary>
    public string? ProcessStatus { get; set; }
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Satu angsuran piutang karyawan/dokter pada satu payroll run: jumlah sesuai perjanjian Finance, batas potong, rencana potongan,
/// hasil yang sudah dikonfirmasi ke Finance, dan sisa piutang menurut Finance. Hanya baca; tidak mengubah saldo piutang.
/// </summary>
public sealed class PayrollEmployeeDeductionItemResponse
{
    public Guid PayrollRunId { get; set; }
    public string RunNumber { get; set; } = string.Empty;
    public string RunStatus { get; set; } = string.Empty;
    public string PeriodCode { get; set; } = string.Empty;
    public string DeductionPeriod { get; set; } = string.Empty;

    public Guid PayrollRunEmployeeId { get; set; }
    public Guid WorkforceProfileId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;

    public Guid InstallmentId { get; set; }
    public string PlanNumber { get; set; } = string.Empty;
    public string ReceivableNumber { get; set; } = string.Empty;
    public int InstallmentNumber { get; set; }

    public decimal ScheduledAmount { get; set; }
    public decimal CarriedOverAmount { get; set; }

    /// <summary>Terjadwal ditambah tunggakan terbawa: nominal yang wajib dipotong periode ini sesuai perjanjian.</summary>
    public decimal DueAmount { get; set; }
    public decimal? AvailableNetSalary { get; set; }
    public decimal? MaxAllowedDeduction { get; set; }
    public decimal? PlannedDeduction { get; set; }

    /// <summary>BERHASIL | SEBAGIAN | GAGAL (rencana). Kosong bila belum dihitung.</summary>
    public string? PlannedStatus { get; set; }

    /// <summary>Alasan dalam bahasa Indonesia bila rencana sebagian/gagal; kosong bila penuh atau belum dihitung.</summary>
    public string? ShortfallReason { get; set; }

    /// <summary><see cref="PayrollInstallmentProcessStatuses"/>.</summary>
    public string ProcessStatus { get; set; } = string.Empty;
    public bool IsConfirmedToFinance { get; set; }

    /// <summary>Nominal terpotong menurut Finance saat ini (setelah konfirmasi dan pembalikan).</summary>
    public decimal FinancePaidAmount { get; set; }
    public decimal FinanceOutstandingAmount { get; set; }
    public string InstallmentStatus { get; set; } = string.Empty;
    public decimal ReversedAmount { get; set; }
}

public sealed class PayrollDeductionReconcileRequest
{
    [Required] public Guid PayrollPeriodId { get; set; }
}

/// <summary>Satu temuan rekonsiliasi angsuran: kode, penjelasan, dan angka dari kedua sisi.</summary>
public sealed class PayrollDeductionReconciliationItemResponse
{
    public Guid InstallmentId { get; set; }
    public string PlanNumber { get; set; } = string.Empty;
    public string ReceivableNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
    public string? RunNumber { get; set; }

    /// <summary>NOT_IN_RUN | NOT_CALCULATED | PLANNED | CONFIRMED_OK | CONFIRMED_MISMATCH | APPLIED_WITHOUT_PLAN.</summary>
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public decimal DueAmount { get; set; }
    public decimal PlannedDeduction { get; set; }
    public decimal ConfirmedDeduction { get; set; }
    public decimal ReversedAmount { get; set; }
    public decimal ExpectedFinancePaid { get; set; }
    public decimal FinancePaidAmount { get; set; }
    public decimal FinanceOutstandingAmount { get; set; }
}

/// <summary>
/// Rekonsiliasi potongan cicilan satu periode: terjadwal, layak dipotong (rencana), dipotong payroll (terkonfirmasi), pembalikan, dan
/// saldo menurut Finance. Setiap angka dapat ditelusuri ke baris sumbernya pada <see cref="Items"/>.
/// </summary>
public sealed class PayrollDeductionReconciliationResponse
{
    public Guid PayrollPeriodId { get; set; }
    public string PeriodCode { get; set; } = string.Empty;
    public string DeductionPeriod { get; set; } = string.Empty;
    public bool IsAutomaticPeriod { get; set; }
    public int InstallmentCount { get; set; }
    public decimal TotalDueAmount { get; set; }
    public decimal TotalPlannedDeduction { get; set; }
    public decimal TotalConfirmedDeduction { get; set; }
    public decimal TotalReversedAmount { get; set; }
    public decimal TotalExpectedFinancePaid { get; set; }
    public decimal TotalFinancePaidAmount { get; set; }
    public decimal TotalFinanceOutstandingAmount { get; set; }
    public int DiscrepancyCount { get; set; }
    public Dictionary<string, int> CountByCode { get; set; } = new();
    public List<PayrollDeductionReconciliationItemResponse> Items { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

public sealed class PayrollRestitutionQuery
{
    public Guid? PayrollPeriodId { get; set; }
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>Satu restitusi salah potong yang disematkan sebagai komponen penghasilan pada slip periode berikutnya.</summary>
public sealed class PayrollRestitutionListItemResponse
{
    public Guid AdjustmentId { get; set; }
    public string AdjustmentNumber { get; set; } = string.Empty;
    public Guid? ReversalId { get; set; }
    public string? ReversalNumber { get; set; }
    public Guid? SourceId { get; set; }
    public string? SourceType { get; set; }
    public Guid PayrollRunId { get; set; }
    public string RunNumber { get; set; } = string.Empty;
    public string RunStatus { get; set; } = string.Empty;
    public bool IsRunLocked { get; set; }
    public string PeriodCode { get; set; } = string.Empty;
    public Guid? PayrollRunEmployeeId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;

    /// <summary>MENUNGGU_HITUNG | SUDAH_DIHITUNG | TERKUNCI — posisi restitusi pada siklus run tujuannya.</summary>
    public string PlacementStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public static class PayrollRestitutionPlacementStatuses
{
    public const string AwaitingCalculation = "MENUNGGU_HITUNG";
    public const string Calculated = "SUDAH_DIHITUNG";
    public const string Locked = "TERKUNCI";
}
