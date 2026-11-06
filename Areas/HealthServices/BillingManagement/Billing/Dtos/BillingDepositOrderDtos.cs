using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

public class CreateDepositOrderRequest
{
    [Required]
    public Guid EpisodeId { get; set; }

    [Required]
    public Guid EncounterId { get; set; }

    [Required]
    [Range(0.01, 9999999999999999.99, ErrorMessage = "Nominal deposit order harus lebih besar dari 0.")]
    public decimal RequestedAmount { get; set; }

    [MaxLength(30)]
    public string OrderType { get; set; } = "INITIAL_DEPOSIT";

    [MaxLength(50)]
    public string SourceType { get; set; } = "INPATIENT_ADMISSION";

    public Guid? SourceReferenceId { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }

    public Guid IdempotencyKey { get; set; } = Guid.NewGuid();

    public DateTimeOffset? DueDate { get; set; }

    public Guid? PolicyId { get; set; }
}

public class DepositOrderQuery
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    public string? OrderType { get; set; }
    public Guid? EpisodeId { get; set; }
    public Guid? EncounterId { get; set; }
    public Guid? PatientId { get; set; }
    public DateTimeOffset? DateFrom { get; set; }
    public DateTimeOffset? DateTo { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "CreateDateTime";
    public string? SortDirection { get; set; } = "desc";
}

public class DepositOrderSummaryDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid EpisodeId { get; set; }
    public string EpisodeNumber { get; set; } = string.Empty;
    public Guid EncounterId { get; set; }
    public string EncounterNumber { get; set; } = string.Empty;
    public Guid PatientId { get; set; }
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string IdentityNumberMasked { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public string OrderTypeName { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public decimal RequestedAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public DateTimeOffset? DueDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
}

/// <summary>
/// Alias kompatibilitas mundur untuk <see cref="DepositOrderSummaryDto"/>.
/// </summary>
public class BilDepositPaymentOrderSummaryDto : DepositOrderSummaryDto
{
}

public class DepositOrderDetailDto : DepositOrderSummaryDto
{
    public Guid? PolicyId { get; set; }
    public string? PolicyName { get; set; }
    public decimal? MinimumPolicyAmount { get; set; }
    public decimal? PolicyShortfall { get; set; }
    public string? Remarks { get; set; }
    public Guid RowVersion { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public List<DepositOrderPaymentItemDto> PaymentHistory { get; set; } = new();
    public DepositAccountBriefDto? DepositAccountSummary { get; set; }
}

public class DepositOrderPaymentItemDto
{
    public Guid PaymentId { get; set; }
    public Guid DepositMovementId { get; set; }
    public Guid? SettlementId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public string PaymentMethodName { get; set; } = string.Empty;
    public bool IsCash { get; set; }
    public decimal Amount { get; set; }
    public string? ReferenceNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset PaidAt { get; set; }
    public string CashierName { get; set; } = string.Empty;
    public string? CashierShiftNumber { get; set; }
}

public class DepositAccountBriefDto
{
    public Guid? AccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public decimal AvailableBalance { get; set; }
    public decimal TotalReceived { get; set; }
    public decimal TotalAllocated { get; set; }
}

public class PayDepositOrderRequest
{
    [Required]
    [Range(0.01, 9999999999999999.99, ErrorMessage = "Nominal pembayaran harus lebih besar dari 0.")]
    public decimal Amount { get; set; }

    [Required]
    public Guid PaymentMethodId { get; set; }

    public Guid? PaymentMethodAccountId { get; set; }

    [MaxLength(150)]
    public string? ReferenceNumber { get; set; }

    public Guid? ExpectedRowVersion { get; set; }

    public Guid CorrelationId { get; set; } = Guid.NewGuid();

    public Guid CausationId { get; set; } = Guid.NewGuid();

    public Guid IdempotencyKey { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = "Pembayaran deposit rawat inap";
}

public class PayDepositOrderResponse
{
    public Guid DepositOrderId { get; set; }
    /// <summary>Id pembayaran order (BilDepositOrderPayment); dipakai untuk mengambil kwitansi.</summary>
    public Guid PaymentId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string OrderStatus { get; set; } = string.Empty;
    public decimal RequestedAmount { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public decimal PaymentAmount { get; set; }
    public SettlementResponse Settlement { get; set; } = null!;
    public DepositReceiptDto Receipt { get; set; } = null!;
    public bool IsReplay { get; set; }
}

public class CancelDepositOrderRequest
{
    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}

public class DepositReceiptDto
{
    /// <summary>Pembayaran (BilDepositOrderPayment.Id) yang menjadi dasar kwitansi ini. Satu kwitansi = satu pembayaran.</summary>
    public Guid PaymentId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string EpisodeNumber { get; set; } = string.Empty;
    public string EncounterNumber { get; set; } = string.Empty;

    /// <summary>
    /// Total Tagihan Deposit: nominal order deposit yang diminta (BilDepositPaymentOrder.RequestedAmount). Untuk order
    /// <c>INITIAL_DEPOSIT</c> inilah Total Tagihan Deposit Awal yang diminta saat admisi rawat inap. Tidak pernah
    /// diganti oleh nominal pembayaran transaksi.
    /// </summary>
    public decimal RequestedAmount { get; set; }

    /// <summary>
    /// Terbilang <see cref="RequestedAmount"/> (Terbilang Total Tagihan Deposit Awal). Dibentuk dari nilai yang sama
    /// dengan <see cref="RequestedAmount"/> dan terpisah dari <see cref="AmountInWords"/> (terbilang pembayaran).
    /// </summary>
    public string RequestedAmountInWords { get; set; } = string.Empty;

    /// <summary>
    /// Jenis order (<c>INITIAL_DEPOSIT</c> = deposit awal admisi, <c>ADDITIONAL_TOP_UP</c> = top-up tambahan) beserta
    /// namanya, agar layar hanya menyebut "Deposit Awal" untuk order yang memang deposit awal.
    /// </summary>
    public string OrderType { get; set; } = string.Empty;
    public string OrderTypeName { get; set; } = string.Empty;

    /// <summary>
    /// Jumlah Diterima: nominal pembayaran aktual pada transaksi kwitansi ini (BilDepositOrderPayment.Amount).
    /// Selalu nominal satu pembayaran, bukan akumulasi order. <see cref="AmountInWords"/> dibentuk dari nilai yang sama.
    /// </summary>
    public decimal Amount { get; set; }
    public string AmountInWords { get; set; } = string.Empty;

    /// <summary>Akumulasi pembayaran SUCCESS order ini sampai dengan transaksi kwitansi ini (nilai historis, bukan saldo terkini).</summary>
    public decimal PaidAmountAfterPayment { get; set; }

    /// <summary>Sisa tagihan order setelah transaksi kwitansi ini (nilai historis, tidak berubah oleh pembayaran berikutnya).</summary>
    public decimal OutstandingAfterPayment { get; set; }

    /// <summary>Status pembayaran (SUCCESS atau REVERSED). Kwitansi pembayaran yang dibalik tidak berlaku.</summary>
    public string PaymentStatus { get; set; } = string.Empty;
    public string PaymentMethodName { get; set; } = string.Empty;
    public bool IsCash { get; set; }
    public bool IsBankTransfer { get; set; }

    /// <summary>
    /// Identitas rekening bank rumah sakit yang menerima transfer, dibaca dari master rekening metode pembayaran
    /// (<c>MstPaymentMethodAccount</c>) lewat <c>PaymentMethodAccountId</c> pada mutasi deposit. Kosong untuk
    /// pembayaran tanpa rekening (mis. tunai). Dibaca saat kwitansi dibuka, bukan salinan beku saat transaksi.
    /// </summary>
    public string? RecipientBankName { get; set; }
    public string? RecipientAccountNumber { get; set; }
    public string? RecipientAccountHolderName { get; set; }

    /// <summary>Nomor referensi yang dicatat kasir pada transaksi (mis. referensi transfer). Bukan bukti verifikasi bank.</summary>
    public string? ReferenceNumber { get; set; }
    public DateTimeOffset PaymentDateTime { get; set; }
    public string CashierName { get; set; } = string.Empty;
    public string? CashierShiftNumber { get; set; }
    public string Notes { get; set; } = string.Empty;
}
