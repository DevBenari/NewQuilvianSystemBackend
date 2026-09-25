using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.DTOs;

/// <summary>
/// Permintaan perhitungan biaya administrasi berbasis aturan deklaratif (BKC-DEC-113, BKC-DEC-121, BKC-DEC-122).
/// </summary>
public sealed class AdminFeeCalculationRequest
{
    public Guid InvoiceId { get; set; }
    public Guid EncounterId { get; set; }
    public Guid PatientId { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public decimal EligibleBaseAmount { get; set; }
    public DateTimeOffset EffectiveAt { get; set; }
    public DateTimeOffset? DischargeTime { get; set; }
    public string? GuarantorName { get; set; }
    public string? PaymentType { get; set; }
}

/// <summary>
/// Hasil perhitungan biaya administrasi dengan evaluasi persentase ber-cap dan jaminan paket (BKC-DEC-113, BKC-DEC-121).
/// </summary>
public sealed class AdminFeeCalculationResult
{
    public Guid? PolicyId { get; set; }
    public string? PolicyCode { get; set; }
    public string CalculationType { get; set; } = "FLAT";
    public decimal? Percentage { get; set; }
    public decimal? CapAmount { get; set; }
    public decimal EligibleBaseAmount { get; set; }
    public decimal RawCalculatedAmount { get; set; }
    public decimal CalculatedAmount { get; set; }
    public decimal AppliedAmount { get; set; }
    public decimal PatientResponsibilityAmount { get; set; }
    public bool IsCapApplied { get; set; }
    public bool IsPackageGuaranteed { get; set; }
    public decimal PriorAppliedAmount { get; set; }
    public bool ReplacesEarlierFee { get; set; }
    public bool ReferredOutpatientAdminVoided { get; set; }
    public decimal ReferredOutpatientAdminCreditedAmount { get; set; }
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// Permintaan rekonsiliasi biaya administrasi alihan rawat jalan ke rawat inap (BKC-DEC-119, BKC-DES-049, BIL-VAL-126).
/// </summary>
public sealed class ReconcileReferredOutpatientAdminRequest
{
    [Required]
    public Guid InpatientInvoiceId { get; set; }

    [Required]
    public Guid InpatientEncounterId { get; set; }

    [Required]
    public Guid PatientId { get; set; }

    public Guid ActorUserId { get; set; } = Guid.Empty;
}

/// <summary>
/// Hasil rekonsiliasi pembatalan atau pengkreditan biaya administrasi rajal.
/// </summary>
public sealed class ReconcileReferredOutpatientAdminResult
{
    public bool HasReferredOutpatient { get; set; }
    public Guid? OutpatientInvoiceId { get; set; }
    public bool WasVoided { get; set; }
    public int VoidedItemCount { get; set; }
    public bool WasCredited { get; set; }
    public decimal CreditedAmount { get; set; }
    public Guid? RefundableCreditId { get; set; }
    public string Message { get; set; } = string.Empty;
}
