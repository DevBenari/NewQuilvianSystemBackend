namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

/// <summary>
/// Model respon pembacaan keadaan clearance terkini sebuah resep untuk integrasi dalam-proses Farmasi
/// (BKC-DEC-107, BKC-DES-040, PHA-DEC-063, BIL-INT-014, BIL-VAL-117).
/// </summary>
public sealed record PrescriptionClearanceStatusResponse(
    Guid PrescriptionId,
    string ClearanceStatus,
    string? FinancialOutcome,
    string? ReasonCode,
    long? FinancialVersion,
    DateTimeOffset? EffectiveAt,
    bool IsKnown,
    bool IsCleared);

public static class BillingHandoffTypes
{
    public const string Collection = "COLLECTION";
    public const string Prescription = "PRESCRIPTION";
}

public static class BillingHandoffTargetModules
{
    public const string Finance = "FINANCE";
    public const string Pharmacy = "PHARMACY";
}

/// <summary>
/// Parameter kueri pencarian surat handoff yang menggantung (BKC-DEC-108, BIL-API-1.3).
/// </summary>
public sealed class PendingHandoffQuery
{
    public string? HandoffType { get; set; }
    public DateTimeOffset? FromDate { get; set; }
    public DateTimeOffset? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Item daftar surat handoff yang belum diambil konsumen (BKC-DEC-108, BIL-API-1.3, BIL-SCR-41).
/// </summary>
public sealed record PendingHandoffResponse(
    Guid Id,
    string HandoffType,
    string TargetModule,
    DateTimeOffset CreatedAt,
    string ReferenceNumber,
    Guid InvoiceId,
    string? InvoiceNumber,
    string Status,
    string? Details);

/// <summary>
/// Permintaan pengakuan penerimaan surat handoff (BKC-DEC-108, BIL-API-1.3).
/// </summary>
public sealed class AcknowledgeHandoffRequest
{
    public string? HandoffType { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Respon setelah pengakuan penerimaan surat handoff dicatat (BKC-DEC-108, BIL-API-1.3).
/// </summary>
public sealed record HandoffResponse(
    Guid Id,
    string HandoffType,
    string TargetModule,
    string Status,
    DateTimeOffset? AcknowledgedAt,
    string? Message);

/// <summary>
/// Hasil pekerjaan pemulihan surat clearance untuk resep yang terlanjur tertahan (BKC-DEC-111, BIL-INTEGRATION-1.1).
/// </summary>
public sealed record PrescriptionClearanceRecoveryResult(
    int TotalInvoicesEvaluated,
    int TotalPrescriptionsEvaluated,
    int TotalPrescriptionsRecovered,
    int TotalPrescriptionsSkippedAlreadyCleared,
    int TotalPrescriptionsSkippedUnpaid,
    IReadOnlyList<Guid> RecoveredPrescriptionIds,
    IReadOnlyList<string> SummaryNotes);
