using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.PatientRefund.Dtos;

public sealed class CreatePatientRefundRequestDto
{
    /// <summary>
    /// Saldo refund yang sudah ada (baris AVAILABLE pada daftar). Identitas ini berasal dari baris yang
    /// dipilih pengguna, tidak pernah diketik; server memvalidasi bahwa saldonya ada dan masih tersedia.
    /// Pengajuan dari sisa deposit rawat inap tidak lewat sini — kasir memakai pengajuan refund deposit
    /// pada Billing, yang membuat saldonya sendiri secara atomik.
    /// </summary>
    public Guid RefundableCreditId { get; set; }

    [Range(
        typeof(decimal),
        "0.01",
        "9999999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)]
    public decimal RequestedAmount { get; set; }

    [Required, MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    /// <summary>Selalu <c>TRANSFER</c>. Nilai lain ditolak server.</summary>
    [MaxLength(30)]
    public string? PaymentMethod { get; set; } = "TRANSFER";

    /// <summary>Rekening pengirim rumah sakit (<c>MstBankAccount</c>). Wajib; diperiksa ulang server.</summary>
    public Guid? BankAccountId { get; set; }

    /// <summary>
    /// Bank pengirim yang dipilih pengguna. Opsional; bila dikirim, server memastikan rekening pengirim
    /// memang milik bank ini. Tidak disimpan — bank diturunkan dari relasi master rekening.
    /// </summary>
    public Guid? SenderBankId { get; set; }

    /// <summary>Rekening tujuan pasien — data terpisah dari rekening pengirim.</summary>
    [MaxLength(100)]
    public string? PayeeBankName { get; set; }

    [MaxLength(50)]
    public string? PayeeAccountNumber { get; set; }

    [MaxLength(150)]
    public string? PayeeAccountName { get; set; }

    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
}

public sealed class ApprovePatientRefundRequestDto
{
    public decimal? ApprovedAmount { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }

    /// <summary>
    /// Revisi rekening pengirim oleh penyetuju (checker). Kosong berarti rekening pada pengajuan
    /// dipertahankan. Revisi dicatat pada audit; setelah persetujuan rekening terkunci.
    /// </summary>
    public Guid? BankAccountId { get; set; }

    /// <summary>Bank pengirim yang dipilih penyetuju; diperiksa terhadap rekening, tidak disimpan.</summary>
    public Guid? SenderBankId { get; set; }

    public Guid ExpectedRowVersion { get; set; }
}

public sealed class RejectPatientRefundRequestDto
{
    [Required, MaxLength(500)]
    public string RejectionReason { get; set; } = string.Empty;

    public Guid ExpectedRowVersion { get; set; }
}

public sealed class ExecutePatientRefundRequestDto
{
    [Range(
        typeof(decimal),
        "0.01",
        "9999999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)]
    public decimal PaidAmount { get; set; }

    /// <summary>Kosong atau <c>TRANSFER</c>. Nilai lain ditolak server.</summary>
    [MaxLength(30)]
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Rekening pengirim. Kosong berarti memakai rekening pada pengajuan yang disetujui. Bila diisi harus
    /// sama dengan rekening itu; satu-satunya pengecualian adalah pengajuan lama yang belum punya rekening
    /// pengirim, yang wajib menetapkannya di sini.
    /// </summary>
    public Guid? BankAccountId { get; set; }

    /// <summary>Nomor referensi bukti transfer bank. Wajib.</summary>
    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public Guid ExpectedRowVersion { get; set; }
}

public sealed class PatientRefundItemDto
{
    public Guid Id { get; set; }
    public Guid? RefundRequestId { get; set; }
    public Guid RefundableCreditId { get; set; }
    public Guid PatientId { get; set; }
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid EncounterId { get; set; }
    public string EncounterNumber { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public Guid SourceId { get; set; }
    public DateTimeOffset RecognizedAt { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal AvailableAmount { get; set; }
    public decimal RequestedAmount { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? PaymentMethod { get; set; }

    /// <summary>Rekening pengirim rumah sakit (master). Terpisah dari rekening tujuan pasien di bawah.</summary>
    public Guid? BankAccountId { get; set; }
    public Guid? SenderBankId { get; set; }
    public string? SenderBankName { get; set; }
    public string? SenderAccountNumber { get; set; }
    public string? SenderAccountName { get; set; }

    public string? PayeeBankName { get; set; }
    public string? PayeeAccountNumber { get; set; }
    public string? PayeeAccountName { get; set; }
    public string? ReferenceNumber { get; set; }
    public Guid? RequestedBy { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public Guid RowVersion { get; set; }
}

public sealed class PatientRefundSummaryDto
{
    public decimal TotalAvailableRefundAmount { get; set; }
    public decimal TotalRequestedRefundAmount { get; set; }
    public decimal TotalPendingApprovalAmount { get; set; }
    public decimal TotalApprovedWaitingPaymentAmount { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public int PendingCount { get; set; }
    public int ApprovedCount { get; set; }
    public int PaidCount { get; set; }
    public int AvailableCreditCount { get; set; }
}

public sealed class PatientRefundDetailDto
{
    public Guid Id { get; set; }
    public Guid? RefundRequestId { get; set; }
    public Guid RefundableCreditId { get; set; }
    public Guid PatientId { get; set; }
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid EncounterId { get; set; }
    public string EncounterNumber { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string? DepositAccountNumber { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public Guid SourceId { get; set; }
    public DateTimeOffset RecognizedAt { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal AvailableAmount { get; set; }
    public decimal RequestedAmount { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? PaymentMethod { get; set; }
    public Guid? BankAccountId { get; set; }

    /// <summary>Nama pemilik rekening pengirim. Dipertahankan demi kompatibilitas; lihat <c>SenderAccountName</c>.</summary>
    public string? BankAccountName { get; set; }

    /// <summary>Rekening pengirim rumah sakit, diturunkan dari master rekening bank dan master bank.</summary>
    public Guid? SenderBankId { get; set; }
    public string? SenderBankName { get; set; }
    public string? SenderAccountNumber { get; set; }
    public string? SenderAccountName { get; set; }

    /// <summary>Rekening tujuan pasien — data terpisah dari rekening pengirim di atas.</summary>
    public string? PayeeBankName { get; set; }
    public string? PayeeAccountNumber { get; set; }
    public string? PayeeAccountName { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? ExecutionNotes { get; set; }
    public string? RejectionReason { get; set; }
    public string? FailedReason { get; set; }
    public Guid? RequestedBy { get; set; }
    public string? RequestedByName { get; set; }
    public Guid? ApprovedBy { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public Guid RowVersion { get; set; }
}

public sealed class PatientRefundFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public string? Search { get; set; }
    public string? Status { get; set; }
    public string? SourceType { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? SortField { get; set; }
    public string? SortDirection { get; set; }
}

