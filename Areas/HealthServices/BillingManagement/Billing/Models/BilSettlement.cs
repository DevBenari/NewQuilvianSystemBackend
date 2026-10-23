using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

[Table("BilSettlement", Schema = "public")]
public sealed class BilSettlement : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? InvoiceId { get; set; }
    public Guid? DepositAccountId { get; set; }
    /// <summary>Target settlement iuran membership (non-kunjungan); diisi hanya untuk purpose MEMBERSHIP_FEE.</summary>
    public Guid? MembershipFeeChargeId { get; set; }
    [Required, MaxLength(30)] public string Purpose { get; set; } = string.Empty;
    // Catatan internal kasir untuk settlement ini (mis. alasan pembayaran sebagian). Internal
    // saja - tidak pernah dikembalikan pada payload/template cetak Dokumen Kasir (BKC-DEC-048).
    [MaxLength(500)] public string? Note { get; set; }
    public decimal RequestedAmount { get; set; }
    public decimal SuccessfulAmount { get; set; }
    public decimal AllocatedAmount { get; set; }

    /// <summary>
    /// Batas dana settlement ini yang boleh dialokasikan ke kelebihan plafon Benefit Karyawan RS (E), dibaca dari HR
    /// sekali saat settlement dibuat. Kosong untuk kunjungan non-benefit. Rekonsiliasi memakai nilai ini (bukan membaca
    /// HR ulang di tengah transaksi uang): dana melebihi K dialokasikan ke BENEFIT_EXCESS sampai batas ini, dan hanya sisanya
    /// yang menjadi kredit refund.
    /// </summary>
    public decimal? BenefitExcessAuthorizedAmount { get; set; }

    [Required, MaxLength(30)] public string Status { get; set; } = BillingSettlementStatuses.Draft;
    public Guid IdempotencyKey { get; set; }
    [Required, MaxLength(64)] public string PayloadHash { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public Guid RowVersion { get; set; } = Guid.NewGuid();
    public ICollection<BilTender> Tenders { get; set; } = new List<BilTender>();
    public ICollection<BilPaymentAllocation> Allocations { get; set; } = new List<BilPaymentAllocation>();
}

public static class BillingSettlementStatuses
{
    public const string Draft = "DRAFT";
    public const string InProgress = "IN_PROGRESS";
    public const string PartiallySettled = "PARTIALLY_SETTLED";
    public const string Settled = "SETTLED";
    public const string Failed = "FAILED";
}

public static class BillingSettlementPurposes
{
    public const string DepositTopUp = "DEPOSIT_TOP_UP";
    public const string InvoicePayment = "INVOICE_PAYMENT";
    public const string MembershipFee = "MEMBERSHIP_FEE";
}
