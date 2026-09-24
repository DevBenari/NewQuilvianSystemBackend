using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

/// <summary>
/// Surat fakta kelayakan keuangan pemulangan pasien rawat inap (BKC-DEC-115, BKC-DES-045, BIL-INT-016).
/// Aggregate root tersendiri yang diterbitkan Billing ke Inpatient Management.
/// </summary>
[Table("BilInpatientClearanceHandoff", Schema = "public")]
public sealed class BilInpatientClearanceHandoff : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Kunjungan rawat inap yang dievaluasi kelayakannya.</summary>
    public Guid EncounterId { get; set; }

    /// <summary>Tagihan Billing yang menjadi dasar evaluasi kelayakan. FK ke BilInvoice.</summary>
    public Guid InvoiceId { get; set; }

    /// <summary>Status kelayakan: PENDING, BLOCKED, CLEARED, REVOKED.</summary>
    [Required, MaxLength(20)]
    public string ClearanceStatus { get; set; } = string.Empty;

    /// <summary>Hasil finansial: FULLY_PAID, INSURANCE_GUARANTEED, SETTLED_WITH_DEPOSIT, DISCHARGED_WITH_AR. Kosong saat REVOKED/BLOCKED/PENDING.</summary>
    [MaxLength(30)]
    public string? FinancialOutcome { get; set; }

    /// <summary>Sisa tagihan pasien saat surat ini diterbitkan. Wajib &lt;= 0 untuk status CLEARED.</summary>
    [Column(TypeName = "numeric(18,2)")]
    public decimal OutstandingBalance { get; set; }

    /// <summary>Total nilai tagihan yang menjadi kewajiban pasien (ekses).</summary>
    [Column(TypeName = "numeric(18,2)")]
    public decimal TotalPatientResponsibility { get; set; }

    /// <summary>Total dana yang sudah diterima kasir dan/atau dialokasikan dari deposit.</summary>
    [Column(TypeName = "numeric(18,2)")]
    public decimal TotalPaidOrAllocated { get; set; }

    /// <summary>Alasan penerbitan surat.</summary>
    [Required, MaxLength(40)]
    public string ReasonCode { get; set; } = string.Empty;

    /// <summary>Keterangan pembatalan status izin pulang (wajib terisi jika ClearanceStatus = 'REVOKED').</summary>
    [MaxLength(100)]
    public string? RevocationReason { get; set; }

    /// <summary>Nomor versi finansial yang naik monoton per encounter.</summary>
    public long FinancialVersion { get; set; }

    /// <summary>Waktu saat perubahan status kelayakan ini berlaku di sistem.</summary>
    public DateTimeOffset EffectiveAt { get; set; }

    /// <summary>Rantai telusur proses bisnis lintas modul.</summary>
    public Guid CorrelationId { get; set; }

    /// <summary>Peristiwa atau transaksi spesifik pemicu penerbitan surat.</summary>
    public Guid CausationId { get; set; }

    /// <summary>Status pengiriman fakta ke konsumen: CREATED atau ACKNOWLEDGED.</summary>
    [Required, MaxLength(30)]
    public string Status { get; set; } = BillingHandoffStatuses.Created;

    /// <summary>Waktu saat modul Rawat Inap mengonfirmasi penerimaan surat.</summary>
    public DateTimeOffset? AcknowledgedAt { get; set; }

    /// <summary>Token kendali konkurensi optimistik.</summary>
    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public BilInvoice Invoice { get; set; } = null!;
}

public static class InpatientClearanceStatuses
{
    public const string Pending = "PENDING";
    public const string Blocked = "BLOCKED";
    public const string Cleared = "CLEARED";
    public const string Revoked = "REVOKED";
}

public static class InpatientFinancialOutcomes
{
    public const string FullyPaid = "FULLY_PAID";
    public const string InsuranceGuaranteed = "INSURANCE_GUARANTEED";
    public const string SettledWithDeposit = "SETTLED_WITH_DEPOSIT";
    public const string DischargedWithAr = "DISCHARGED_WITH_AR";
}

public static class InpatientClearanceReasonCodes
{
    public const string InvoiceSettled = "INVOICE_SETTLED";
    public const string GuarantorApproved = "GUARANTOR_APPROVED";
    public const string DischargeOrderInitiated = "DISCHARGE_ORDER_INITIATED";
    public const string LateChargePosted = "LATE_CHARGE_POSTED";
    public const string PaymentReversed = "PAYMENT_REVERSED";
    public const string CorrectionApplied = "CORRECTION_APPLIED";
    public const string ReevaluatedByCashier = "REEVALUATED_BY_CASHIER";
}
