using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

public sealed class CreateSettlementRequest
{
    public Guid? InvoiceId { get; set; }
    public Guid? DepositAccountId { get; set; }
    /// <summary>Target iuran membership (non-kunjungan); wajib bila Purpose = MEMBERSHIP_FEE.</summary>
    public Guid? MembershipFeeChargeId { get; set; }
    [Required, MaxLength(30)] public string Purpose { get; set; } = string.Empty;
    [Range(
        typeof(decimal),
        "0.01",
        "9999999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)]
    public decimal RequestedAmount { get; set; }
    [MaxLength(500)] public string? Note { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
}

public sealed class CreateTenderRequest
{
    public Guid PaymentMethodId { get; set; }
    public Guid? PaymentMethodAccountId { get; set; }
    [Range(
        typeof(decimal),
        "0.01",
        "9999999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)]
    public decimal Amount { get; set; }

    /// <summary>
    /// Uang fisik yang diserahkan pasien. WAJIB untuk metode pembayaran tunai (MstPaymentMethod.IsCash), termasuk
    /// bila pasien membayar pas (isi sama dengan <see cref="Amount"/>), dan harus &gt;= <see cref="Amount"/>.
    /// <see cref="Amount"/> tetap nominal pembayaran NETO untuk tagihan. Kembalian dihitung backend
    /// (tidak ada field kembalian pada request). Metode non-tunai yang mengirim nilai ini ditolak.
    /// </summary>
    [Range(
        typeof(decimal),
        "0.01",
        "9999999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)]
    public decimal? CashTenderedAmount { get; set; }

    /// <summary>
    /// Konfirmasi kasir bahwa kembalian sudah diserahkan kepada pasien. Wajib true bila kembalian
    /// (CashTenderedAmount - Amount) lebih dari nol; divalidasi backend, bukan hanya frontend. Untuk metode
    /// non-tunai field ini tidak boleh dikirim.
    /// </summary>
    public bool? CashChangeGivenConfirmed { get; set; }

    [MaxLength(150)] public string? CashierReferenceNote { get; set; }
    public Guid ExpectedRowVersion { get; set; }
    public bool? IsManualCashierConfirmed { get; set; }
    [MaxLength(500)] public string? ConfirmationNote { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
}

/// <summary>
/// Pembatalan percobaan pembayaran TUNAI yang masih berstatus CREATED. Bukan reversal: tender SUCCEEDED tidak dapat
/// dibatalkan lewat request ini, dan tender PENDING non-tunai menunggu kepastian penyedia pembayaran.
/// </summary>
public sealed class CancelTenderRequest
{
    /// <summary>RowVersion tender (bukan settlement) yang dilihat kasir saat memutuskan membatalkan.</summary>
    public Guid ExpectedRowVersion { get; set; }

    /// <summary>Alasan pembatalan; wajib (bukan hanya spasi) dan tersimpan pada audit bersama petugas dan waktu.</summary>
    [Required, MaxLength(500)] public string CancellationReason { get; set; } = string.Empty;

    /// <summary>
    /// Pernyataan kasir bahwa uang pasien dari percobaan ini belum diterima atau sudah dikembalikan seluruhnya,
    /// sehingga tidak ada uang yang dipegang tanpa pencatatan. Wajib true; divalidasi backend, bukan hanya frontend.
    /// </summary>
    public bool CashCustodyConfirmed { get; set; }

    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
}

public sealed class ManualConfirmTenderRequest
{
    public Guid ExpectedRowVersion { get; set; }
    [Required, MaxLength(150)] public string CashierReferenceNote { get; set; } = string.Empty;
    public Guid? PaymentMethodAccountId { get; set; }
    [MaxLength(500)] public string? ConfirmationNote { get; set; }
    public bool UserConfirmation { get; set; } = true;
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
}

public sealed class SettlementResponse
{
    public Guid? Id { get; set; }
    public Guid? InvoiceId { get; set; }
    public Guid? DepositAccountId { get; set; }
    public Guid? MembershipFeeChargeId { get; set; }
    public string Purpose { get; set; } = BillingSettlementPurposes.DepositTopUp;
    public string? Note { get; set; }
    public string Status { get; set; } = BillingSettlementStatuses.Draft;
    public decimal RequestedAmount { get; set; }
    public decimal SuccessfulAmount { get; set; }
    public decimal AllocatedAmount { get; set; }

    /// <summary>Batas bagian settlement ini yang boleh melunasi kelebihan plafon Benefit RS (E); null untuk kunjungan non-benefit.</summary>
    public decimal? BenefitExcessAuthorizedAmount { get; set; }
    public decimal PendingAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public decimal CollectibleAmount { get; set; }
    public decimal RoundingAdjustmentAmount { get; set; }
    public decimal EffectivePayableAmount { get; set; }
    public bool IsReplay { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid? RowVersion { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public Guid? DepositMovementId { get; set; }
    public DepositResponse? Deposit { get; set; }
    public IReadOnlyList<TenderResponse> Tenders { get; set; } = [];

    // Status pembayaran invoice: "BELUM_LUNAS", "SEBAGIAN", "LUNAS"
    public string PaymentStatus { get; set; } = "BELUM_LUNAS";
    public bool IsFullyPaid { get; set; }
    public string? InvoiceStatus { get; set; }
    public Guid? EncounterId { get; set; }
    public int? EncounterStatus { get; set; }
    public string? EncounterStatusName { get; set; }
    public string? AutoCompletionIncompleteReason { get; set; }
}

public sealed class TenderResponse
{
    public Guid Id { get; set; }
    public Guid TenderId => Id;
    public Guid SettlementId { get; set; }
    public Guid PaymentMethodId { get; set; }
    public Guid? PaymentMethodAccountId { get; set; }

    /// <summary>Pembayaran NETO yang diterapkan untuk melunasi tagihan (bukan uang fisik bruto).</summary>
    public decimal Amount { get; set; }

    /// <summary>Uang fisik yang diserahkan pasien; null untuk non-tunai dan tender tunai lama (sebelum kolom ada).</summary>
    public decimal? CashTenderedAmount { get; set; }

    /// <summary>Kembalian yang diserahkan kasir; null untuk non-tunai dan data lama, 0 untuk tunai pas.</summary>
    public decimal? CashChangeAmount { get; set; }

    public string Status { get; set; } = BillingTenderStatuses.Created;
    public string? CashierReferenceNote { get; set; }
    public string? KwitansiNumber { get; set; }
    public string? ProviderReferenceMasked { get; set; }
    public string? ProviderStatusCode { get; set; }
    public DateTimeOffset AttemptedAt { get; set; }
    public DateTimeOffset? SettledAt { get; set; }

    /// <summary>
    /// Waktu percobaan dibatalkan kasir; null bila tender tidak dibatalkan. Alasan dan petugas pembatalan tersimpan
    /// pada audit, tidak diekspos di response pembayaran.
    /// </summary>
    public DateTimeOffset? CancelledAt { get; set; }
    public Guid? CashierShiftId { get; set; }
    public Guid RowVersion { get; set; }
    public bool IsReplay { get; set; }
    public string? InvoiceClosureNote { get; set; }

    // Provenance konfirmasi pembayaran: "CASH", "MANUAL_CASHIER", "PAYMENT_PROVIDER"
    public string? ConfirmationSource { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }
    public string? MembershipFeeChargeStatus { get; set; }
    public string? MembershipConfirmationStatus { get; set; }

    // Klasifikasi transaksi pembayaran: "PELUNASAN", "CICILAN", "PENDING"
    public string TransactionClassification { get; set; } = "CICILAN";
    // Status tagihan setelah transaksi ini: "LUNAS", "SEBAGIAN", "BELUM_LUNAS"
    public string PostTransactionStatus { get; set; } = "SEBAGIAN";
    public decimal RemainingOutstanding { get; set; }
    public decimal RoundingAdjustmentAmount { get; set; }
}
