using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;

public sealed class CreatePayrollRunRequest
{
    [Required] public Guid PayrollPeriodId { get; set; }
    [MaxLength(2000)] public string? Notes { get; set; }
}

public sealed class PayrollRunDecisionRequest
{
    [MaxLength(2000)] public string? Reason { get; set; }
}

public sealed class PayrollRunQuery
{
    public Guid? PayrollPeriodId { get; set; }
    public string? Status { get; set; }
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class PayrollRunListItemResponse
{
    public Guid Id { get; set; }
    public string RunNumber { get; set; } = string.Empty;
    public Guid PayrollPeriodId { get; set; }
    public string PeriodCode { get; set; } = string.Empty;
    public string PeriodName { get; set; } = string.Empty;
    public string RunType { get; set; } = string.Empty;
    public string RunStatus { get; set; } = string.Empty;
    public bool IsLocked { get; set; }
    public int TotalEmployeeCount { get; set; }
    public int ErrorEmployeeCount { get; set; }
    public decimal TotalGrossPay { get; set; }
    public decimal TotalDeduction { get; set; }
    public decimal TotalNetPay { get; set; }
    public DateTime? CalculatedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
}

/// <summary>Potongan cicilan piutang RS pada satu slip: rencana dari hitung terakhir dan status konfirmasi ke Finance.</summary>
public sealed class PayrollRunInstallmentLineResponse
{
    public Guid InstallmentId { get; set; }
    public string? DeductionPeriod { get; set; }

    /// <summary>Nominal sesuai perjanjian angsuran (terjadwal ditambah tunggakan terbawa).</summary>
    public decimal DueAmount { get; set; }
    public decimal? MaxAllowedDeduction { get; set; }
    public decimal? PlannedDeduction { get; set; }

    /// <summary>BERHASIL | SEBAGIAN | GAGAL (rencana). Kosong bila belum dihitung.</summary>
    public string? PlannedStatus { get; set; }
    public bool IsCalculated { get; set; }

    /// <summary>True setelah run disahkan dan hasilnya diterima Finance (saldo piutang baru berkurang pada titik ini).</summary>
    public bool IsConfirmedToFinance { get; set; }

    /// <summary>Nominal terpotong saat ini menurut Finance (setelah konfirmasi dan pembalikan). Kosong bila belum dikonfirmasi.</summary>
    public decimal? FinanceDeductedAmount { get; set; }
}

public sealed class PayrollRunEmployeeResponse
{
    public Guid Id { get; set; }
    public Guid WorkforceProfileId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeePayrollStatus { get; set; } = string.Empty;
    public decimal BaseSalary { get; set; }
    public decimal GrossPay { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalEmployeeInsuranceContribution { get; set; }
    public decimal TotalDeduction { get; set; }
    public decimal NetPay { get; set; }
    public string? ValidationMessage { get; set; }
    public PayrollRunInstallmentLineResponse? Installment { get; set; }

    /// <summary>Total pembalikan potongan cicilan yang sudah disetujui untuk pegawai ini.</summary>
    public decimal ReversedAmount { get; set; }
    public bool HasPendingReversal { get; set; }
}

public sealed class PayrollRunDetailResponse : PayrollRunListItemResponse
{
    public string? Notes { get; set; }

    /// <summary>Penahan pengajuan: data yang belum lengkap atau tidak valid. Kosong berarti run siap diajukan.</summary>
    public List<string> Blockers { get; set; } = new();

    /// <summary>Angsuran periode ini yang belum masuk run beserta alasannya (mis. pegawai tidak termasuk run).</summary>
    public List<string> InstallmentSyncNotes { get; set; } = new();

    public int InstallmentCount { get; set; }
    public decimal TotalInstallmentDue { get; set; }
    public decimal TotalInstallmentPlanned { get; set; }
    public List<PayrollRunEmployeeResponse> Employees { get; set; } = new();
    public List<PayrollReversalResponse> Reversals { get; set; } = new();
}

public sealed class RequestPayrollReversalRequest
{
    /// <summary>Nominal yang dibalik. Kosong = seluruh potongan cicilan yang terpotong saat ini.</summary>
    [Range(0.01, 100000000000)] public decimal? Amount { get; set; }
    [Required, MaxLength(1000)] public string Reason { get; set; } = string.Empty;
}

public sealed class PayrollReversalResponse
{
    public Guid Id { get; set; }
    public string ReversalNumber { get; set; } = string.Empty;
    public Guid? PayrollRunEmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;

    /// <summary>Full | Partial.</summary>
    public string ReversalType { get; set; } = string.Empty;

    /// <summary>Requested | Approved | Rejected.</summary>
    public string ReversalStatus { get; set; } = string.Empty;
    public decimal ReversalAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? PostedAt { get; set; }
}

public static class PayrollPeriodProcessStatuses
{
    /// <summary>Periode sejak peralihan, tetapi HRD belum membuat payroll run. Jadwal angsuran menunggu.</summary>
    public const string AwaitingPayrollRun = "MENUNGGU_PAYROLL_RUN";

    /// <summary>Periode sebelum peralihan: potongan dicatat Finance lewat Potong Gaji Manual, bukan payroll run.</summary>
    public const string ManualFinance = "MANUAL_FINANCE";
}

public static class PayrollInstallmentProcessStatuses
{
    public const string AwaitingPayrollRun = "MENUNGGU_PAYROLL_RUN";
    public const string ManualFinance = "MANUAL_FINANCE";
    public const string NotInRun = "BELUM_MASUK_RUN";
    public const string NotCalculated = "BELUM_DIHITUNG";
    public const string Planned = "DIRENCANAKAN";
    public const string Confirmed = "TERKONFIRMASI";
}

public sealed class PayrollPeriodOptionResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DeductionPeriod { get; set; } = string.Empty;
    public bool IsLocked { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>Satu angsuran periode itu: sesuai perjanjian Finance, dan (bila run ada) rencana atau hasil potongan HRD.</summary>
public sealed class PayrollPeriodInstallmentResponse
{
    public Guid InstallmentId { get; set; }
    public string PlanNumber { get; set; } = string.Empty;
    public string ReceivableNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
    public int InstallmentNumber { get; set; }
    public decimal ScheduledAmount { get; set; }
    public decimal CarriedOverAmount { get; set; }

    /// <summary>Terjadwal ditambah tunggakan terbawa: nominal yang wajib dipotong periode ini sesuai perjanjian.</summary>
    public decimal DueAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string InstallmentStatus { get; set; } = string.Empty;
    public decimal? PlannedDeduction { get; set; }
    public string? PlannedStatus { get; set; }

    /// <summary><see cref="PayrollInstallmentProcessStatuses"/>.</summary>
    public string ProcessStatus { get; set; } = string.Empty;
}

public sealed class PayrollRunPeriodOverviewResponse
{
    public Guid PayrollPeriodId { get; set; }
    public string PeriodCode { get; set; } = string.Empty;
    public string PeriodName { get; set; } = string.Empty;
    public string DeductionPeriod { get; set; } = string.Empty;
    public bool IsLocked { get; set; }

    /// <summary>Periode pertama yang wajib memakai payroll run (AutomaticFromPeriod); kosong bila peralihan belum ditetapkan.</summary>
    public string? AutomaticFromPeriod { get; set; }
    public bool IsAutomaticPeriod { get; set; }

    public Guid? RunId { get; set; }
    public string? RunNumber { get; set; }
    public string? RunStatus { get; set; }

    /// <summary><see cref="PayrollPeriodProcessStatuses"/> atau status run (Draft, Calculated, Submitted, Approved).</summary>
    public string PeriodStatus { get; set; } = string.Empty;

    public int EmployeeCount { get; set; }
    public int InstallmentCount { get; set; }
    public decimal TotalDueAmount { get; set; }
    public decimal TotalPlannedDeduction { get; set; }
    public List<PayrollPeriodInstallmentResponse> Items { get; set; } = new();
}

public sealed class PayrollRunValidationException(string message) : Exception(message);

public sealed class PayrollRunConflictException(string message) : Exception(message);
