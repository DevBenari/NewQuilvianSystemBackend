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
    public decimal ReferredOutpatientAdminCreditedAmount { get; set; }
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// Rincian invoice rawat jalan asal yang bertautan aktif dengan tagihan rawat inap (BKC-DEC-119, BKC-DES-084, BKC-DES-085, BIL-API-2.0).
/// </summary>
public sealed class ReferredAdminOriginResult
{
    public Guid OriginInvoiceId { get; set; }
    public string OriginInvoiceNumber { get; set; } = string.Empty;
    public Guid OriginEncounterId { get; set; }
    public string OriginInvoiceStatus { get; set; } = string.Empty;
    public string Treatment { get; set; } = string.Empty; // SUPERSEDED, CREDITED, NO_ADMIN
    public decimal AdminPatientAmount { get; set; }
    public decimal CreditAmount { get; set; }
}

/// <summary>
/// Hasil evaluasi penggantian atau pengkreditan biaya administrasi poli berbasis tautan aktif (BKC-DEC-119, BKC-DES-084, BKC-DES-085).
/// </summary>
public sealed class ReferredAdminReplacementResult
{
    public bool IsSupersededByInpatientAdmission { get; set; }
    public Guid? SupersededByInpatientInvoiceId { get; set; }
    public string? SupersededReasonCode { get; set; }
    public decimal ReferredOutpatientAdminCreditedAmount { get; set; }
    public decimal ReferredOutpatientAdminUnappliedCreditAmount { get; set; }
    public IReadOnlyList<ReferredAdminOriginResult> ReferredOutpatientAdminOrigins { get; set; } = [];
}
