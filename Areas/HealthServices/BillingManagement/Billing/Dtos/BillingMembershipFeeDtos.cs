using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

/// <summary>Parameter daftar tagihan iuran membership (server-side search, filter, dan paging).</summary>
public sealed class MembershipFeeChargeQuery
{
    /// <summary>Nomor tagihan, nomor membership, No. RM, atau nama pasien (tidak membedakan huruf besar/kecil).</summary>
    public string? Search { get; set; }

    /// <summary>UNPAID, PARTIALLY_PAID, PAID, atau CANCELLED.</summary>
    public string? Status { get; set; }

    /// <summary>Hanya tagihan yang masih dapat dibayar (belum lunas/dibatalkan, belum lewat batas bayar).</summary>
    public bool PayableOnly { get; set; }

    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 10;
}

public sealed class EnsureMembershipFeeChargeRequest
{
    [Required] public Guid MembershipId { get; set; }
}

public class MembershipFeeChargeResponse
{
    public Guid Id { get; set; }
    public string ChargeNumber { get; set; } = string.Empty;

    public Guid MembershipId { get; set; }
    public string MemberNumber { get; set; } = string.Empty;
    public string MembershipStatus { get; set; } = string.Empty;
    public string MembershipStatusName { get; set; } = string.Empty;
    public string RegistrationType { get; set; } = string.Empty;
    public string TierName { get; set; } = string.Empty;

    public Guid PatientId { get; set; }
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;

    public decimal FeeAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal OutstandingAmount { get; set; }

    public string Status { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public DateTimeOffset PaymentDueAt { get; set; }
    public bool IsOverdue { get; set; }
    public DateTimeOffset? SettledAt { get; set; }
    public DateTimeOffset? MembershipConfirmedAt { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }

    /// <summary>Tagihan boleh dibayar sekarang (ditentukan backend dari status tagihan, batas bayar, dan status membership).</summary>
    public bool CanPay { get; set; }

    /// <summary>Tagihan lunas tetapi pelunasan belum diterapkan ke membership (masih Menunggu Pembayaran).</summary>
    public bool NeedsSync { get; set; }

    public bool RequiresReview { get; set; }
    public string? ReviewReason { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public Guid RowVersion { get; set; }
}

public sealed class MembershipFeeChargeDetailResponse : MembershipFeeChargeResponse
{
    public string? CancellationReason { get; set; }

    /// <summary>
    /// Settlement terakhir yang belum lunas. Kasir melanjutkan settlement ini (menambah tender) alih-alih
    /// membuat baru; kosong bila belum ada pembayaran yang dimulai.
    /// </summary>
    public Guid? OpenSettlementId { get; set; }
    public Guid? OpenSettlementRowVersion { get; set; }
    public string? OpenSettlementStatus { get; set; }
    public decimal OpenSettlementCollectibleAmount { get; set; }

    public string? LastPaymentMethodName { get; set; }
    public string? LastPaymentReference { get; set; }
    public string? LastKwitansiNumber { get; set; }
}

public sealed class MembershipFeePaymentItem
{
    public Guid TenderId { get; set; }
    public Guid SettlementId { get; set; }
    public string? KwitansiNumber { get; set; }
    public Guid PaymentMethodId { get; set; }
    public string PaymentMethodName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CashierReferenceNote { get; set; }
    public string? ProviderReferenceMasked { get; set; }
    public DateTimeOffset AttemptedAt { get; set; }
    public DateTimeOffset? SettledAt { get; set; }
}

public sealed class MembershipFeeReconciliationItem
{
    public Guid MembershipId { get; set; }
    public string MemberNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public string MembershipStatusName { get; set; } = string.Empty;
    public Guid? ChargeId { get; set; }
    public string? ChargeNumber { get; set; }
    public string? ChargeStatusName { get; set; }
    public decimal FeeAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Dua kelompok rekonsiliasi dari kontrak integrasi CRM (INT-06): (1) membership Menunggu Pembayaran yang
/// tagihannya sudah lunas, dan (2) membership Aktif/Terjadwal tanpa tagihan lunas (kecuali data lama),
/// ditambah tagihan yang perlu ditinjau manual. Bersifat pelaporan; tidak mengubah apa pun.
/// </summary>
public sealed class MembershipFeeReconciliationResponse
{
    public IReadOnlyList<MembershipFeeReconciliationItem> PaidButNotActivated { get; set; } = [];
    public IReadOnlyList<MembershipFeeReconciliationItem> ActivatedWithoutPaidCharge { get; set; } = [];
    public IReadOnlyList<MembershipFeeReconciliationItem> NeedsReview { get; set; } = [];
}
