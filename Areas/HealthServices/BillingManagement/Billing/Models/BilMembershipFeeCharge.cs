using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

/// <summary>
/// Tagihan iuran membership non-kunjungan (crm-membership CRM-DEC-019). Berdiri sendiri dan terhubung ke
/// tepat satu membership dan satu pasien, tanpa <c>EncounterId</c> dan tanpa kunjungan semu.
/// Billing memiliki tagihan dan kewajiban bayar; CRM memiliki siklus hidup membership. Nominal
/// disalin dari <c>FeeAmountSnapshot</c> membership saat tagihan dibuat, bukan dari master tingkat terkini.
/// </summary>
[Table("BilMembershipFeeCharge", Schema = "public")]
public sealed class BilMembershipFeeCharge : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Nomor tagihan dari Number Series Billing (unik).</summary>
    [Required, MaxLength(50)] public string ChargeNumber { get; set; } = string.Empty;

    /// <summary>Satu membership paling banyak satu tagihan (unique index = idempotency).</summary>
    public Guid MembershipId { get; set; }

    public Guid PatientId { get; set; }

    /// <summary>Nominal iuran yang harus dibayar, salinan <c>FeeAmountSnapshot</c>.</summary>
    public decimal FeeAmount { get; set; }

    /// <summary>
    /// Proyeksi tersimpan dari alokasi bersih (alokasi dikurangi pembalikan). Sumber otoritatifnya adalah
    /// <see cref="BilMembershipFeeAllocation"/>; kolom ini hanya mempercepat pembacaan.
    /// </summary>
    public decimal PaidAmount { get; set; }

    [Required, MaxLength(30)] public string Status { get; set; } = BillingMembershipFeeChargeStatuses.Unpaid;

    /// <summary>Batas bayar, salinan <c>PaymentDueAt</c> membership. Lewat dari ini tagihan tidak lagi dapat dibayar.</summary>
    public DateTimeOffset PaymentDueAt { get; set; }

    /// <summary>Waktu pembayaran terakhir yang menuntaskan tagihan.</summary>
    public DateTimeOffset? SettledAt { get; set; }

    /// <summary>Waktu CRM mengonfirmasi bahwa pelunasan sudah diterapkan ke membership. Kosong = belum.</summary>
    public DateTimeOffset? MembershipConfirmedAt { get; set; }

    public DateTimeOffset? CancelledAt { get; set; }

    [MaxLength(500)] public string? CancellationReason { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public ICollection<BilMembershipFeeAllocation> Allocations { get; set; } = new List<BilMembershipFeeAllocation>();
}

/// <summary>
/// Alokasi dana settlement ke tagihan iuran. Tabel terpisah dari <c>BilPaymentAllocation</c> karena tabel itu
/// terikat FK keras ke <c>BilInvoice</c> dan check <c>TargetType = 'INVOICE'</c>. Semantiknya sama: baris
/// bersifat append-only, dan pembalikan dicatat sebagai baris baru yang menunjuk baris aslinya.
/// </summary>
[Table("BilMembershipFeeAllocation", Schema = "public")]
public sealed class BilMembershipFeeAllocation : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MembershipFeeChargeId { get; set; }
    public Guid SettlementId { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset AllocatedAt { get; set; }
    public Guid? ReversesAllocationId { get; set; }
    public BilMembershipFeeCharge MembershipFeeCharge { get; set; } = null!;
}

public static class BillingMembershipFeeChargeStatuses
{
    public const string Unpaid = "UNPAID";
    public const string PartiallyPaid = "PARTIALLY_PAID";
    public const string Paid = "PAID";
    public const string Cancelled = "CANCELLED";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>([Unpaid, PartiallyPaid, Paid, Cancelled], StringComparer.OrdinalIgnoreCase);
}
