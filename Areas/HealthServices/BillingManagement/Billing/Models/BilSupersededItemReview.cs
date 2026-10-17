using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

/// <summary>
/// Kosakata keputusan tinjauan pembatalan (void) keliru (BKC-DEC-158, BKC-DEC-162, BKC-DES-089).
/// </summary>
public static class BilSupersededItemReviewDecisions
{
    public const string Reinstate = "REINSTATE";
    public const string Keep = "KEEP";
    public const string NeedsPostCloseCorrection = "NEEDS_POST_CLOSE_CORRECTION";

    public static readonly IReadOnlyList<string> All = new[]
    {
        Reinstate,
        Keep,
        NeedsPostCloseCorrection
    };
}

/// <summary>
/// Tabel append-only peninjauan void keliru pada item tagihan (BKC-DEC-158, BKC-DEC-162, BKC-DES-089).
/// </summary>
[Table("BilSupersededItemReview", Schema = "public")]
public sealed class BilSupersededItemReview : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid InvoiceItemId { get; set; }

    [Required]
    public Guid InvoiceId { get; set; }

    [Required, MaxLength(40)]
    public string Decision { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string InvoiceStatusAtDecision { get; set; } = string.Empty;

    public Guid? ReinstatedInvoiceItemId { get; set; }

    [Required, MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    [Required]
    public Guid DecidedBy { get; set; }

    public DateTimeOffset DecidedAt { get; set; } = DateTimeOffset.UtcNow;

    [Required]
    public Guid IdempotencyKey { get; set; }

    [Required, MaxLength(64)]
    public string PayloadHash { get; set; } = string.Empty;

    // Navigasi
    public BilInvoiceItem InvoiceItem { get; set; } = null!;
    public BilInvoice Invoice { get; set; } = null!;
    public BilInvoiceItem? ReinstatedInvoiceItem { get; set; }
}

