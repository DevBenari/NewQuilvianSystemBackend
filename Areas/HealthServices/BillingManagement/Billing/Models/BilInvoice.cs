using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

[Table("BilInvoice", Schema = "public")]
public sealed class BilInvoice : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EncounterId { get; set; }
    [Required, MaxLength(50)] public string InvoiceNumber { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string ServiceType { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string Status { get; set; } = BillingInvoiceStatuses.Open;
    public int CurrentCalculationVersion { get; set; }
    public DateTimeOffset? InvoiceDate { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public Guid RowVersion { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Tanda "perlu diperiksa" sebelum finalisasi — kontrak <c>integrasi-billing</c> <c>1.1.0</c>
    /// kamus data 6.6. Invoice bertanda ini tidak dapat difinalkan (<c>INV-RWF-08</c>,
    /// <c>BIL-FIN-020</c>) sampai kasir menyelesaikan pemeriksaannya.
    /// </summary>
    public bool RequiresReview { get; set; }

    /// <summary>Alasan tanda; kosakata pada <see cref="BillingInvoiceReviewReasonCodes"/>.</summary>
    [MaxLength(50)] public string? ReviewReasonCode { get; set; }

    public DateTimeOffset? ReviewFlaggedAt { get; set; }

    public DateTimeOffset? ReviewResolvedAt { get; set; }

    public Guid? ReviewResolvedByUserId { get; set; }

    /// <summary>Catatan penyelesaian pemeriksaan oleh kasir. Kolom sensitif; tidak masuk log.</summary>
    [MaxLength(500)] public string? ReviewResolutionNote { get; set; }

    public ICollection<BilInvoiceItem> Items { get; set; } = new List<BilInvoiceItem>();
    public ICollection<BilCalculationVersion> CalculationVersions { get; set; } = new List<BilCalculationVersion>();
    public ICollection<BilDiscountApplication> DiscountApplications { get; set; } = new List<BilDiscountApplication>();
}

public static class BillingInvoiceStatuses
{
    public const string Open = "OPEN";
    public const string Final = "FINAL";
    public const string Closed = "CLOSED";
    public const string SettledByWriteOff = "SETTLED_BY_WRITE_OFF";
}

/// <summary>
/// Kode alasan tanda "perlu diperiksa" pada invoice — kontrak <c>integrasi-billing</c>
/// <c>1.1.0</c> bagian 9.7.
/// </summary>
public static class BillingInvoiceReviewReasonCodes
{
    /// <summary>
    /// Invoice memuat biaya kamar yang dicatat manual kasir <b>dan</b> hitungan tarif kamar
    /// otomatis sekaligus, sehingga berisiko tertagih dua kali.
    /// </summary>
    public const string ManualAndAutomaticRoomCharge = "MANUAL_AND_AUTOMATIC_ROOM_CHARGE";
}
