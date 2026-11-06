using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;

public sealed class CreatePayrollRunRequest
{
    [Required] public Guid PayrollPeriodId { get; set; }
    [MaxLength(2000)] public string? Notes { get; set; }
}

public sealed class PayrollRunDecisionRequest
{
    [MaxLength(2000)] public string? Reason { get; set; }

    /// <summary>Token versi run yang dilihat klien (<c>rowVersion</c>). Kosong = tanpa pemeriksaan; berbeda = 409.</summary>
    [MaxLength(40)] public string? ExpectedRowVersion { get; set; }
}

/// <summary>Badan opsional untuk aksi run tanpa alasan (hitung, ajukan, sahkan, tutup).</summary>
public sealed class PayrollRunActionRequest
{
    /// <summary>Token versi run yang dilihat klien (<c>rowVersion</c>). Kosong = tanpa pemeriksaan; berbeda = 409.</summary>
    [MaxLength(40)] public string? ExpectedRowVersion { get; set; }
}

public sealed class PayrollRunQuery
{
    public Guid? PayrollPeriodId { get; set; }

    /// <summary>Satu status run, atau beberapa status dipisah koma (mis. <c>Draft,CollectingInput,Calculated</c>).</summary>
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

    /// <summary>
    /// Token konkurensi optimistik turunan (tick waktu pembaruan). Kirim kembali sebagai <c>expectedRowVersion</c> pada aksi run
    /// agar aksi atas data yang sudah berubah ditolak dengan 409.
    /// </summary>
    public string RowVersion { get; set; } = string.Empty;
}

/// <summary>Potongan cicilan piutang RS pada satu slip: rencana dari hitung terakhir dan status konfirmasi ke Finance.</summary>
public sealed class PayrollRunInstallmentLineResponse
{
    public Guid InstallmentId { get; set; }
    public string? DeductionPeriod { get; set; }

    /// <summary>Nominal sesuai perjanjian angsuran (terjadwal ditambah tunggakan terbawa).</summary>
    public decimal DueAmount { get; set; }

    /// <summary>Gaji bersih yang tersedia untuk cicilan setelah pajak, iuran, dan potongan lain (dasar batas potong).</summary>
    public decimal? AvailableNetSalary { get; set; }
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

public class PayrollRunEmployeeResponse
{
    public Guid Id { get; set; }
    public Guid WorkforceProfileId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeePayrollStatus { get; set; } = string.Empty;
    public decimal BaseSalary { get; set; }
    public decimal TotalRecurringEarning { get; set; }
    public decimal TotalVariableEarning { get; set; }
    public decimal TotalOvertimeAmount { get; set; }
    public decimal TotalAttendanceAllowance { get; set; }
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

/// <summary>Satu baris komponen penghasilan/potongan pada slip penerima (gross-to-net yang dapat ditelusuri).</summary>
public sealed class PayrollRunComponentLineResponse
{
    public Guid Id { get; set; }
    public string ComponentCode { get; set; } = string.Empty;
    public string ComponentName { get; set; } = string.Empty;

    /// <summary>Earning | Deduction | Tax | Insurance.</summary>
    public string ComponentType { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Amount { get; set; }
    public bool IsProrated { get; set; }
    public bool IsManual { get; set; }
    public string? Formula { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>Rincian satu penerima pada run: komponen, agregat kehadiran/lembur, dan identitas pembayaran yang disamarkan.</summary>
public sealed class PayrollRunEmployeeDetailResponse : PayrollRunEmployeeResponse
{
    public int ScheduledWorkMinutes { get; set; }
    public int ActualWorkMinutes { get; set; }
    public int LateMinutes { get; set; }
    public int OvertimeMinutes { get; set; }
    public decimal PaidLeaveDays { get; set; }
    public decimal UnpaidLeaveDays { get; set; }
    public decimal AbsentDays { get; set; }
    public string? BankName { get; set; }

    /// <summary>Nomor rekening disamarkan (empat digit terakhir). Nomor lengkap tidak dikirim ke layar.</summary>
    public string? BankAccountMasked { get; set; }
    public string? TaxStatus { get; set; }
    public bool IsSnapshotFrozen { get; set; }
    public List<PayrollRunComponentLineResponse> Components { get; set; } = new();
}

/// <summary>Ringkasan pembayaran run (instruksi ke Finance dan hasil bank). Hanya terisi setelah handoff.</summary>
public sealed class PayrollRunPaymentSummaryResponse
{
    public int InstructionCount { get; set; }
    public int ScheduledCount { get; set; }
    public int PaidCount { get; set; }
    public int FailedCount { get; set; }
    public decimal TotalInstructionAmount { get; set; }
    public decimal TotalPaidAmount { get; set; }
}

/// <summary>Satu baris riwayat perpindahan status run (jejak audit dari persetujuan dan cap waktu aksi).</summary>
public sealed class PayrollRunStatusHistoryItemResponse
{
    public DateTime OccurredAt { get; set; }

    /// <summary>Submit | Return | Approve | Cancel | ReversalRequest | ReversalApprove | ReversalReject | Close | FinanceHandoff.</summary>
    public string Action { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid? ActorUserId { get; set; }
    public string? ActorName { get; set; }
    public string? Comments { get; set; }
}

public sealed class PayrollRunDetailResponse : PayrollRunListItemResponse
{
    public string? Notes { get; set; }
    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }
    public DateTime? PaymentDate { get; set; }

    /// <summary>Versi kebijakan yang dipakai hitung terakhir (kosong bila belum dihitung).</summary>
    public string? PolicyVersion { get; set; }

    /// <summary>
    /// Aksi yang sedang boleh dijalankan menurut status run (bantuan tampilan, BUKAN pengaman: setiap aksi memeriksa ulang di backend).
    /// Nilai: Calculate, ImportHrd, Submit, Return, Approve, Cancel, FinanceHandoff, Close, RequestReversal, GeneratePayslips.
    /// </summary>
    public List<string> AvailableActions { get; set; } = new();

    /// <summary>Catatan tidak menahan (mis. rekening belum terverifikasi, input Draft tidak dihitung).</summary>
    public List<string> Warnings { get; set; } = new();
    public PayrollRunPaymentSummaryResponse? PaymentSummary { get; set; }

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

/// <summary>Ringkasan jumlah payroll run per status (baseline standar transaksi). Hanya jumlah, tanpa nominal gaji.</summary>
public sealed class PayrollRunSummaryResponse
{
    public int TotalRuns { get; set; }

    /// <summary>Run yang menunggu tindakan pengguna: Draft, CollectingInput, Calculated, Submitted.</summary>
    public int AwaitingActionCount { get; set; }

    /// <summary>Run yang sudah di tahap pembayaran: SentToFinance, PartiallyPaid.</summary>
    public int InPaymentCount { get; set; }
    public Dictionary<string, int> CountByStatus { get; set; } = new();
}

/// <summary>Metadata filter daftar payroll run (baseline standar transaksi).</summary>
public sealed class PayrollRunFilterMetadataResponse
{
    public List<string> Statuses { get; set; } = new();
    public List<string> RunTypes { get; set; } = new();
    public int DefaultPageSize { get; set; }
    public List<int> PageSizeOptions { get; set; } = new();
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

public sealed class PayrollRunPeriodOptionResponse
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
