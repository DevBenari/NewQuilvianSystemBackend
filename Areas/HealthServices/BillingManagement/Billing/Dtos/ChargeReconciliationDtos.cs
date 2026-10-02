using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

/// <summary>Saringan antrean rekonsiliasi (<c>BE-RJE-012</c>, <c>contracts/api-contract.md</c> V2).</summary>
public sealed class ChargeReconciliationQuery
{
    /// <summary><c>FAILED</c>, <c>RECONCILIATION_REQUIRED</c>, <c>RESOLVED</c>. Kosong = yang belum selesai.</summary>
    [MaxLength(30)] public string? Status { get; set; }
    [MaxLength(50)] public string? SourceDomain { get; set; }
    public Guid? EncounterId { get; set; }
    [MaxLength(100)] public string? ErrorCode { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    /// <summary>Nomor invoice atau nomor kunjungan.</summary>
    [MaxLength(100)] public string? Search { get; set; }
}

/// <summary>
/// Satu item antrean. <c>ItemType = CHARGE_LINE</c> menunjuk baris sinkron folio ke invoice, yang
/// sejak <c>RJ-E2E-DEC-016</c> tersimpan pada <c>BilProcessingEffect</c>; <c>Id</c>-nya karena itu
/// id efek folio. Tanpa nama pasien.
/// </summary>
public sealed class ChargeReconciliationItemResponse
{
    public string ItemType { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid EncounterId { get; set; }
    public string? EncounterNumber { get; set; }
    public string? SourceDomain { get; set; }
    public string? SourceDetailId { get; set; }
    public int SourceVersion { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public int AttemptCount { get; set; }
    public DateTime? LastAttemptAt { get; set; }
    public DateTime? NextAttemptAt { get; set; }
    public DateTime? ReconciliationRequiredAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedByUserName { get; set; }
    public string? ResolutionNote { get; set; }
    public string? InvoiceNumber { get; set; }
}

public sealed class ResolveChargeReconciliationRequest
{
    /// <summary><c>BILLED_MANUALLY</c>, <c>NOT_BILLABLE</c>, <c>DUPLICATE</c>.</summary>
    public string? Resolution { get; set; }

    /// <summary>Alasan penyelesaian, 10–500 karakter. Tidak boleh memuat isi klinis.</summary>
    public string? Note { get; set; }
}

public static class ChargeReconciliationItemTypes
{
    public const string ClinicalFact = "CLINICAL_FACT";
    public const string ChargeLine = "CHARGE_LINE";
}

public static class ChargeReconciliationStatuses
{
    public const string Failed = "FAILED";
    public const string ReconciliationRequired = "RECONCILIATION_REQUIRED";
    public const string Resolved = "RESOLVED";
}

public static class ChargeReconciliationResolutions
{
    public const string BilledManually = "BILLED_MANUALLY";
    public const string NotBillable = "NOT_BILLABLE";
    public const string Duplicate = "DUPLICATE";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.Ordinal) { BilledManually, NotBillable, Duplicate };
}
