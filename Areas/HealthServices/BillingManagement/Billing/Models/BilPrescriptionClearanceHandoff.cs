using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

/// <summary>
/// Surat kepada Farmasi bahwa keadaan clearance sebuah resep berubah (BKC-DEC-106, BKC-DES-039, BIL-INT-014).
/// Aggregate root tersendiri tanpa foreign key ke tabel Farmasi untuk mencegah keterikatan skema lintas modul.
/// </summary>
[Table("BilPrescriptionClearanceHandoff", Schema = "public")]
public sealed class BilPrescriptionClearanceHandoff : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Resep yang keadaannya berubah. Bukan foreign key ke tabel Farmasi.</summary>
    public Guid PrescriptionId { get; set; }

    /// <summary>Tagihan kunjungan tempat resep itu ditagihkan.</summary>
    public Guid InvoiceId { get; set; }

    /// <summary>CLEARED atau REVOKED.</summary>
    [Required, MaxLength(20)]
    public string ClearanceStatus { get; set; } = string.Empty;

    /// <summary>PAID, INSURANCE_APPROVED, atau PAYMENT_WAIVED. Kosong saat REVOKED.</summary>
    [MaxLength(30)]
    public string? FinancialOutcome { get; set; }

    /// <summary>Sebab perubahan clearance resep.</summary>
    [Required, MaxLength(40)]
    public string ReasonCode { get; set; } = string.Empty;

    /// <summary>Nomor versi finansial yang naik monoton per resep.</summary>
    public long FinancialVersion { get; set; }

    /// <summary>Waktu perubahan berlaku.</summary>
    public DateTimeOffset EffectiveAt { get; set; }

    /// <summary>Rantai telusur.</summary>
    public Guid CorrelationId { get; set; }

    /// <summary>Peristiwa penyebab.</summary>
    public Guid CausationId { get; set; }

    /// <summary>Status penyerahan fakta: CREATED atau ACKNOWLEDGED.</summary>
    [Required, MaxLength(30)]
    public string Status { get; set; } = BillingHandoffStatuses.Created;

    /// <summary>Waktu saat Farmasi mengambil surat ini.</summary>
    public DateTimeOffset? AcknowledgedAt { get; set; }

    /// <summary>Kendali konkurensi optimistik.</summary>
    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public BilInvoice Invoice { get; set; } = null!;
}

public static class PrescriptionClearanceStatuses
{
    public const string Cleared = "CLEARED";
    public const string Revoked = "REVOKED";
    public const string Unknown = "UNKNOWN";
}

public static class PrescriptionFinancialOutcomes
{
    public const string Paid = "PAID";
    public const string InsuranceApproved = "INSURANCE_APPROVED";
    public const string PaymentWaived = "PAYMENT_WAIVED";
}

public static class PrescriptionClearanceReasonCodes
{
    public const string InvoiceSettled = "INVOICE_SETTLED";
    public const string InvoiceWrittenOff = "INVOICE_WRITTEN_OFF";
    public const string PrescriptionChargeIncreased = "PRESCRIPTION_CHARGE_INCREASED";
    public const string PaymentReversed = "PAYMENT_REVERSED";
    public const string WriteOffReversed = "WRITE_OFF_REVERSED";
    public const string PayerCoverageReversed = "PAYER_COVERAGE_REVERSED";
}
