using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

public sealed class FinalizeInvoiceRequest
{
    public Guid ExpectedRowVersion { get; set; }
    [MaxLength(30)] public string? DepartureReason { get; set; }
    [MaxLength(200)] public string? DebtorIdentity { get; set; }
    [MaxLength(100)] public string? DebtorRelationship { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }

    /// <summary>BE-FIN-107, FIN-DEC-208, FIN-INTEGRATION-1.9 R.4.3: nominal pilihan pegawai untuk sisa tagihan pasien (D).</summary>
    public decimal? EmployeeReceivableChoiceAmount { get; set; }

    /// <summary>BE-FIN-107, BE-FIN-119, FIN-DEC-218, FIN-INTEGRATION-1.10 S.4: nama pemberi persetujuan bila pasien bukan pegawai.</summary>
    [MaxLength(200)] public string? EmployeeReceivableConsentGivenBy { get; set; }
}

public sealed class FinalizationPreviewResponse
{
    public Guid InvoiceId { get; set; }
    public bool AllOrdersComplete { get; set; }
    public bool CalculationCurrent { get; set; }
    public decimal Outstanding { get; set; }
    public bool IsReadyForNormalFinalization { get; set; }
    public IReadOnlyList<string> BlockingReasons { get; set; } = [];

    /// <summary>
    /// Kode penahan finalisasi yang dapat dibaca mesin — kontrak <c>integrasi-billing</c>
    /// <c>1.1.0</c> API 3.9: <c>BIL-FIN-020</c> invoice "perlu diperiksa",
    /// <c>BIL-FIN-021</c> masih ada layanan "tarif belum ada".
    /// </summary>
    public IReadOnlyList<string> BlockingCodes { get; set; } = [];

    public int CalculationVersion { get; set; }
    public Guid InvoiceRowVersion { get; set; }

    /// <summary>BE-FIN-107, FIN-INTEGRATION-1.9 R.4: rincian pembagian porsi RS Benefit dan utang pegawai.</summary>
    public EmployeeBenefitSplitResponse? EmployeeBenefitSplit { get; set; }
}

public sealed class FinalizationResponse
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public int CalculationVersion { get; set; }
    public decimal OutstandingAtFinalization { get; set; }
    public bool IsDepartureException { get; set; }
    public string? DepartureReason { get; set; }
    public string InvoiceStatus { get; set; } = string.Empty;
    public DateTimeOffset FinalizedAt { get; set; }
    public Guid InvoiceRowVersion { get; set; }
    public Guid CorrelationId { get; set; }
    public bool IsReplay { get; set; }

    /// <summary>BE-FIN-107, FIN-INTEGRATION-1.9 R.4: rincian pembagian porsi RS Benefit dan utang pegawai.</summary>
    public EmployeeBenefitSplitResponse? EmployeeBenefitSplit { get; set; }
}

/// <summary>
/// Respons pratinjau dan hasil pembagian porsi benefit karyawan saat finalisasi tagihan
/// (BE-FIN-107; FIN-INTEGRATION-1.9 R.4.2; FIN-API-1.10 R.5).
/// Seluruh angka dihitung authoritative di backend (kebijakan zero calculation di klien).
/// </summary>
public sealed class EmployeeBenefitSplitResponse
{
    public bool IsEmployeeBenefitVisit { get; set; }
    public string? BenefitOwnerName { get; set; }
    public string? RelationshipCode { get; set; }
    public decimal CoveredByRuleAmount { get; set; }
    public decimal RemainingAllowanceAmount { get; set; }
    public decimal RsBenefitAmount { get; set; }
    public decimal ExcessOverAllowanceAmount { get; set; }
    public decimal PatientPortionAmount { get; set; }
    public decimal ChoiceAmount { get; set; }
    public decimal EmployeeReceivableAmount { get; set; }
    public decimal CashAtCashierAmount { get; set; }
    public bool IsUsable { get; set; }
    public bool RequiresConsent { get; set; }
    public string? BlockingReasonCode { get; set; }
    public string? BlockingMessage { get; set; }
}
