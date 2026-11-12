using QuilvianSystemBackend.Responses;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;

public sealed class PayrollFinanceHandoffRequest
{
    /// <summary>Token versi run yang dilihat klien (<c>rowVersion</c>). Kosong = tanpa pemeriksaan; berbeda = 409.</summary>
    [MaxLength(40)] public string? ExpectedRowVersion { get; set; }

    /// <summary>Kunci idempotensi klien. Handoff ulang pada run yang sudah dikirim selalu mengembalikan hasil pertama (tidak ada instruksi kedua).</summary>
    [MaxLength(150)] public string? IdempotencyKey { get; set; }
    [MaxLength(1000)] public string? Notes { get; set; }
}

/// <summary>
/// Hasil pengiriman instruksi pembayaran ke Finance. Instruksi dibuat satu per penerima (nominal = gaji bersih setelah cicilan) dan
/// berstatus Scheduled; BELUM berarti dibayar. Status Paid lahir dari ACK: hasil bank yang dikirim Finance, atau (PAY-BE-11) konfirmasi
/// manual oleh petugas berwenang selama belum ada koneksi bank (sumber MANUAL, lihat <see cref="PayrollPaymentAcknowledgeRequest"/>).
/// </summary>
public sealed class PayrollFinanceHandoffResponse
{
    public Guid PayrollRunId { get; set; }
    public string RunNumber { get; set; } = string.Empty;
    public string RunStatus { get; set; } = string.Empty;
    public string RowVersion { get; set; } = string.Empty;
    public bool IsIdempotentReplay { get; set; }
    public int InstructionCount { get; set; }
    public decimal TotalAmount { get; set; }
    public int SkippedCount { get; set; }
    public List<string> SkippedReasons { get; set; } = new();

    /// <summary>Penjelasan jujur tentang sisi Finance (NEEDS CONFIRMATION) yang ditampilkan apa adanya di layar.</summary>
    public string FinanceConsumptionNote { get; set; } = string.Empty;
}

public sealed class PayrollPaymentQuery
{
    /// <summary>Scheduled | Processing | Paid | Failed | Cancelled | PartiallyReversed | Reversed.</summary>
    public string? Status { get; set; }
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

/// <summary>Satu instruksi pembayaran per penerima beserta hasil bank. Nomor rekening disamarkan.</summary>
public sealed class PayrollPaymentInstructionResponse
{
    public Guid Id { get; set; }
    public Guid PayrollRunId { get; set; }
    public Guid PayrollRunEmployeeId { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string? PayeeName { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountMasked { get; set; }
    public decimal PaymentAmount { get; set; }
    public decimal NetPaidAmount { get; set; }
    public decimal ReversedAmount { get; set; }
    public string? PaymentReferenceNumber { get; set; }
    public Guid? FinancePaymentId { get; set; }
    public DateTime? ScheduledPaymentAt { get; set; }
    public DateTime? PostedToFinanceAt { get; set; }
    public DateTime? PaidAt { get; set; }

    /// <summary>Alasan gagal dari Finance/bank (terisi bila status Failed).</summary>
    public string? Note { get; set; }
    public bool CanRetry { get; set; }
}

public sealed class PayrollPaymentListResponse
{
    public Guid PayrollRunId { get; set; }
    public string RunNumber { get; set; } = string.Empty;
    public string RunStatus { get; set; } = string.Empty;
    public Guid? FinancePaymentBatchId { get; set; }
    public PayrollRunPaymentSummaryResponse Summary { get; set; } = new();
    public PagedResult<PayrollPaymentInstructionResponse> Payments { get; set; } = new();
}

public sealed class PayrollPaymentAcknowledgeItem
{
    /// <summary>Nomor instruksi (<c>PaymentNumber</c>) yang dikirim Payroll ke Finance.</summary>
    [Required, MaxLength(50)] public string PaymentNumber { get; set; } = string.Empty;

    /// <summary>Paid | Failed.</summary>
    [Required, MaxLength(20)] public string Status { get; set; } = string.Empty;

    /// <summary>Nominal yang benar-benar dibayar; wajib sama dengan nominal instruksi untuk status Paid.</summary>
    [Range(0.0, 100000000000)] public decimal? PaidAmount { get; set; }
    [MaxLength(100)] public string? PaymentReferenceNumber { get; set; }
    public Guid? FinancePaymentId { get; set; }
    public DateTime? PaidAt { get; set; }

    /// <summary>Wajib bila Failed.</summary>
    [MaxLength(500)] public string? FailureReason { get; set; }
}

public sealed class PayrollPaymentAcknowledgeRequest
{
    /// <summary>Kunci korelasi/idempotensi dari Finance untuk satu pengiriman hasil (dicatat pada audit).</summary>
    [MaxLength(150)] public string? IdempotencyKey { get; set; }

    /// <summary>
    /// Sumber konfirmasi untuk SELURUH item dalam kiriman ini (PAY-BE-11): <c>FINANCE_ACK</c> (hasil bank dari Finance, bawaan bila
    /// kosong) atau <c>MANUAL</c> (konfirmasi manual petugas berwenang selama belum ada koneksi bank). Tidak peka huruf besar-kecil.
    /// Nilai lain ditolak (400) sebelum data berubah. Dicatat pada audit dan catatan persetujuan run; untuk item Paid hasil MANUAL,
    /// <c>Notes</c> instruksi diisi "Dikonfirmasi manual (bukan hasil bank)" bila masih kosong.
    /// </summary>
    [MaxLength(20)] public string? ConfirmationSource { get; set; }

    public Guid? FinancePaymentBatchId { get; set; }
    [Required, MinLength(1), MaxLength(500)] public List<PayrollPaymentAcknowledgeItem> Items { get; set; } = new();
}

public sealed class PayrollPaymentAcknowledgeItemResult
{
    public string PaymentNumber { get; set; } = string.Empty;

    /// <summary>Diterapkan | Idempoten | Ditolak.</summary>
    public string Outcome { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public sealed class PayrollPaymentAcknowledgeResponse
{
    public Guid PayrollRunId { get; set; }
    public string RunStatus { get; set; } = string.Empty;

    /// <summary>Gema sumber konfirmasi yang dipakai (<c>FINANCE_ACK</c> atau <c>MANUAL</c>).</summary>
    public string ConfirmationSource { get; set; } = string.Empty;
    public decimal TotalPaidAmount { get; set; }
    public int AppliedCount { get; set; }
    public int IdempotentCount { get; set; }
    public int RejectedCount { get; set; }
    public List<PayrollPaymentAcknowledgeItemResult> Items { get; set; } = new();
}
