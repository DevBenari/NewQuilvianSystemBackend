using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

/// <summary>
/// Tanda terima Billing atas satu ketukan pintu (event outbox) Rawat Inap — kontrak
/// <c>integrasi-billing</c> <c>1.1.0</c> kamus data 6.7.
/// </summary>
/// <remarks>
/// Baris ini sekaligus kunci idempotensi Billing: <see cref="IdempotencyKey"/> unik, sama dengan
/// kunci outbox Rawat Inap (<c>INPATIENT:&lt;SourceType&gt;:&lt;SourceId&gt;:&lt;Version&gt;</c>).
/// Pesan yang sama dikirim ulang dijawab <c>DUPLICATE</c> tanpa efek kedua
/// (<c>INV-RWF-06</c>). <see cref="EpisodeId"/> adalah rujukan logis tanpa FK lintas modul.
/// </remarks>
[Table("BilInpatientEventReceipt", Schema = "public")]
public sealed class BilInpatientEventReceipt : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(255)]
    public string IdempotencyKey { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string EventType { get; set; } = string.Empty;

    public Guid EpisodeId { get; set; }

    public Guid EncounterId { get; set; }

    [Required, MaxLength(100)]
    public string SourceId { get; set; } = string.Empty;

    public int SourceVersion { get; set; }

    /// <summary>Waktu kejadian di Rawat Inap.</summary>
    public DateTime OccurredAtUtc { get; set; }

    public DateTime ReceivedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Hasil pemrosesan; kosakata pada <see cref="BillingInpatientEventOutcomes"/>.</summary>
    [Required, MaxLength(50)]
    public string Outcome { get; set; } = string.Empty;

    public Guid? InvoiceId { get; set; }

    /// <summary>Versi hitungan invoice yang dibuat oleh event ini, bila ada.</summary>
    public int? CalculationVersionNo { get; set; }

    /// <summary>Keterangan teknis singkat. Tanpa data klinis dan tanpa rupiah.</summary>
    [MaxLength(500)]
    public string? Message { get; set; }

    public BilInvoice? Invoice { get; set; }
}

/// <summary>
/// Kosakata hasil tanda terima event Rawat Inap — kontrak <c>integrasi-billing</c> <c>1.1.0</c>
/// bagian 9.7 dan state 5.6.
/// </summary>
public static class BillingInpatientEventOutcomes
{
    /// <summary><c>ADMISSION_CONFIRMED</c> dan invoice <c>RANAP</c> baru dibuka.</summary>
    public const string InvoiceOpened = "INVOICE_OPENED";

    /// <summary><c>ADMISSION_CONFIRMED</c> padahal invoice kunjungan sudah ada; tidak ada invoice kedua.</summary>
    public const string InvoiceAlreadyOpen = "INVOICE_ALREADY_OPEN";

    /// <summary><c>BED_OCCUPIED</c>, <c>OCCUPANCY_CORRECTED</c>, atau <c>BED_RELEASED</c>: versi hitungan baru dibuat.</summary>
    public const string Recalculated = "RECALCULATED";

    /// <summary>Kunci idempotensi sudah pernah diterima; tidak ada efek kedua.</summary>
    public const string Duplicate = "DUPLICATE";

    /// <summary>Kunjungan tidak dikenal Billing; pesan ditolak.</summary>
    public const string RejectedUnknownEncounter = "REJECTED_UNKNOWN_ENCOUNTER";
}
