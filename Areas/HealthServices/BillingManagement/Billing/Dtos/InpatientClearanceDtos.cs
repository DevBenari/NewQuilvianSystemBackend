using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

/// <summary>
/// Permintaan evaluasi atau re-evaluasi kelayakan pemulangan rawat inap (BKC-DEC-115, BKC-DES-045, BIL-API-1.4).
/// </summary>
public sealed class EvaluateInpatientClearanceRequest
{
    public Guid EncounterId { get; set; }
    public string ReasonCode { get; set; } = InpatientClearanceReasonCodes.DischargeOrderInitiated;
    public string? CustomReason { get; set; }
    public string? RevocationReason { get; set; }
}

/// <summary>
/// Permintaan evaluasi ulang kelayakan pemulangan (POST /inpatient-clearance/reevaluate).
/// </summary>
public sealed class ReevaluateInpatientClearanceRequest
{
    public Guid EncounterId { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// Permintaan pengakuan surat handoff oleh staf rawat inap (PATCH /consumer-handoffs/{id}/acknowledge).
/// </summary>
public sealed class AcknowledgeInpatientClearanceRequest
{
    public Guid HandoffId { get; set; }
}

/// <summary>
/// Respon surat fakta kelayakan kepulangan rawat inap (BilInpatientClearanceHandoff).
/// </summary>
public sealed class InpatientClearanceHandoffResponse
{
    public Guid HandoffId { get; set; }
    public Guid EncounterId { get; set; }
    public Guid InvoiceId { get; set; }
    public string ClearanceStatus { get; set; } = string.Empty;
    public string? FinancialOutcome { get; set; }
    public decimal OutstandingBalance { get; set; }
    public decimal TotalPatientResponsibility { get; set; }
    public decimal TotalPaidOrAllocated { get; set; }
    public string ReasonCode { get; set; } = string.Empty;
    public string? RevocationReason { get; set; }
    public long FinancialVersion { get; set; }
    public DateTimeOffset EffectiveAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset? AcknowledgedAt { get; set; }
    public string? Reason { get; set; }
    public DateTimeOffset? ClearedAt { get; set; }
    public string? ClearedByUserName { get; set; }
    public bool CanDischarge => ClearanceStatus == InpatientClearanceStatuses.Cleared;
}

/// <summary>
/// Permintaan validasi deposit tindakan/operasi besar (BKC-DEC-114, BKC-DES-047, BIL-VAL-121).
/// </summary>
public sealed class MajorProcedureDepositValidationRequest
{
    public Guid EncounterId { get; set; }
    public decimal EstimatedCost { get; set; }
    public decimal? GuarantorCoverageAmount { get; set; }
}

/// <summary>
/// Hasil validasi kecukupan deposit tindakan besar 100% atas porsi ekses pasien.
/// </summary>
public sealed class MajorProcedureDepositValidationResult
{
    public Guid EncounterId { get; set; }
    public decimal EstimatedCost { get; set; }
    public decimal GuarantorCoverageAmount { get; set; }
    public decimal PatientExcess { get; set; }
    public decimal AvailableDepositBalance { get; set; }
    public decimal DepositShortfall { get; set; }
    public bool IsSufficient { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Ringkasan status finansial rawat inap untuk bangsal (GET /invoices/encounter/{encounterId}/inpatient-summary).
/// Jika perawat tanpa izin finansial, nominal disembunyikan (null).
/// </summary>
public sealed class InpatientBillingSummaryResponse
{
    public Guid EncounterId { get; set; }
    public Guid? InvoiceId { get; set; }
    public string? InvoiceNumber { get; set; }
    public string BillingStatus { get; set; } = string.Empty;
    public string FinancialClearanceStatus { get; set; } = InpatientClearanceStatuses.Pending;
    public bool CanDischarge { get; set; }
    public List<string> BlockerReasons { get; set; } = new();
    public decimal? DepositRequired { get; set; }
    public decimal? DepositBalance { get; set; }
    public decimal? DepositShortfall { get; set; }
    public decimal? TotalCharges { get; set; }
    public decimal? Outstanding { get; set; }
}
