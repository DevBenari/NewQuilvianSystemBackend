using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

public sealed class BillingInvoiceQuery
{
    public Guid? EncounterId { get; set; }
    public string? Status { get; set; }
    public string? ServiceType { get; set; }
    public string? Search { get; set; }
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed class UpsertChargeRequest
{
    public Guid EncounterId { get; set; }
    [Required, MaxLength(50)] public string SourceDomain { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string SourceDetailId { get; set; } = string.Empty;
    [Range(1, long.MaxValue)] public long SourceVersion { get; set; }
    [Required, MaxLength(30)] public string SourceStatus { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; }
    public Guid CategoryId { get; set; }
    [Required, MaxLength(250)] public string DescriptionSnapshot { get; set; } = string.Empty;
    [Range(
        typeof(decimal),
        "0.0001",
        "99999999999999.9999",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)]
    public decimal Quantity { get; set; }
    [Range(
        typeof(decimal),
        "0",
        "9999999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)]
    public decimal UnitPrice { get; set; }
    [Range(
        typeof(decimal),
        "0",
        "9999999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)]
    public decimal DoctorShare { get; set; }
    [Required, MaxLength(30)] public string ContractVersion { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
}

public class InvoiceSummaryResponse
{
    public Guid Id { get; set; }
    public Guid EncounterId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int CurrentCalculationVersion { get; set; }
    public decimal RunningGrossAmount { get; set; }
    public int ActiveItemCount { get; set; }
    public DateTime CreateDateTime { get; set; }
    public Guid RowVersion { get; set; }
}

public sealed class InvoiceDetailResponse : InvoiceSummaryResponse
{
    public DateTimeOffset? InvoiceDate { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public bool IsReplay { get; set; }
    public IReadOnlyList<InvoiceItemResponse> Items { get; set; } = [];
    public IReadOnlyList<DiscountResponse> Discounts { get; set; } = [];
    public IReadOnlyList<CalculationResponse> CalculationVersions { get; set; } = [];
}

public sealed class InvoiceItemResponse
{
    public Guid Id { get; set; }
    public string SourceDomain { get; set; } = string.Empty;
    public string SourceDetailId { get; set; } = string.Empty;
    public long SourceVersion { get; set; }
    public string SourceContractVersion { get; set; } = string.Empty;
    public string SourceStatus { get; set; } = string.Empty;
    public DateTimeOffset SourceOccurredAt { get; set; }
    public Guid CategoryId { get; set; }
    public string DescriptionSnapshot { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DoctorShare { get; set; }
    public decimal GrossAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? VoidReason { get; set; }
}

public sealed class RecalculateInvoiceRequest
{
    public Guid ExpectedRowVersion { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
}

public sealed class VoidInvoiceItemRequest
{
    public Guid ExpectedRowVersion { get; set; }
    [Range(1, long.MaxValue)] public long SourceVersion { get; set; }
    [Required, MaxLength(30)] public string SourceStatus { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string ContractVersion { get; set; } = string.Empty;
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
}

public sealed class CalculationResponse
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public int VersionNo { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal AdministrationFeeAmount { get; set; }
    public decimal RoomChargeAmount { get; set; }
    public decimal ItemDiscount { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal PatientAmount { get; set; }
    public decimal PrimaryAmount { get; set; }
    public decimal ExcessAmount { get; set; }
    public decimal UnresolvedCoverageAmount { get; set; }
    public decimal RoundingAmount { get; set; }
    public bool IsLocked { get; set; }
    public DateTimeOffset CalculatedAt { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid InvoiceRowVersion { get; set; }
    public CalculationBreakdownResponse Breakdown { get; set; } = new();
}

public sealed class CalculationBreakdownResponse
{
    public string ContractVersion { get; set; } = BillingCalculationContract.Version;
    public AdministrationFeeCalculationResponse AdministrationFee { get; set; } = new();
    public RoomChargeCalculationResponse RoomCharge { get; set; } = new();
    public IReadOnlyList<CalculationItemResponse> Items { get; set; } = [];
    public IReadOnlyList<DiscountCalculationResponse> Discounts { get; set; } = [];
    public IReadOnlyList<TaxCalculationResponse> Taxes { get; set; } = [];
    public CoverageCalculationResponse Coverage { get; set; } = new();
}

public sealed class AdministrationFeeCalculationResponse
{
    public DateOnly BusinessDate { get; set; }
    public Guid? PolicyId { get; set; }
    public string? PolicyCode { get; set; }
    public decimal PolicyAmount { get; set; }
    public decimal PriorAppliedAmount { get; set; }
    public decimal AppliedAmount { get; set; }
    public int ReplacementPriority { get; set; }
    public bool Coverable { get; set; }
    public bool ReplacesEarlierFee { get; set; }
}

// BKC-DEC-043: occupancy timeline (InpBedPlacement) adalah source of truth; komponen ini
// dihitung ulang penuh setiap recalculate persis seperti AdministrationFee - bukan
// BilInvoiceItem, sehingga tidak lewat IBillingChargeSourceAdapter (BKC-DEC-039 memisahkan
// room charge dari kontrak charge-source generik). LeaveRule policy SELALU diperlakukan
// seperti INCLUDE_LEAVE karena belum ada model pencatatan cuti pasien di InPatientManagement -
// ini gap yang disengaja dicatat di sini, bukan ditebak diam-diam.
public sealed class RoomChargeCalculationResponse
{
    public Guid? PolicyId { get; set; }
    public string? PolicyCode { get; set; }
    public decimal AppliedAmount { get; set; }
    public bool LeaveRuleEnforced { get; set; }
    public IReadOnlyList<RoomChargeSegmentResponse> Segments { get; set; } = [];
}

public sealed class RoomChargeSegmentResponse
{
    public Guid PlacementId { get; set; }
    public Guid RoomId { get; set; }
    public Guid ServiceUnitId { get; set; }
    public Guid PatientClassId { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
    public bool IsOngoing { get; set; }
    public int OccupiedMinutes { get; set; }
    public decimal ChargeUnits { get; set; }
    public Guid? TariffId { get; set; }
    public string? TariffCode { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SegmentAmount { get; set; }
    public bool MissingTariff { get; set; }
}

public sealed class CalculationItemResponse
{
    public Guid InvoiceItemId { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string SourceDomain { get; set; } = string.Empty;
    public long SourceVersion { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal ItemDiscount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetAmount { get; set; }
    public bool Coverable { get; set; }
}

public sealed class TaxCalculationResponse
{
    public Guid InvoiceItemId { get; set; }
    public Guid TaxRuleId { get; set; }
    public string TaxRuleCode { get; set; } = string.Empty;
    public decimal BasisAmount { get; set; }
    public decimal Rate { get; set; }
    public string RoundingMode { get; set; } = string.Empty;
    public string AllocationRule { get; set; } = string.Empty;
    public decimal UnroundedAmount { get; set; }
    public decimal TaxAmount { get; set; }
}

public sealed class CoverageCalculationResponse
{
    public string ContractVersion { get; set; } = string.Empty;
    public string PrimaryStatus { get; set; } = string.Empty;
    public string ExcessStatus { get; set; } = string.Empty;
    public decimal EligibleAmount { get; set; }
    public decimal PrimaryAmount { get; set; }
    public decimal ResidualAfterPrimary { get; set; }
    public decimal ExcessAmount { get; set; }
    public decimal ResidualAfterExcess { get; set; }
    public decimal UnresolvedAmount { get; set; }
    public decimal PatientAmount { get; set; }
    public IReadOnlyList<Guid> AppliedRuleIds { get; set; } = [];
}

public static class BillingCalculationContract
{
    public const string Version = "BIL-CALCULATION-0.4";
}
