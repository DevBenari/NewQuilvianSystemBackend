using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.DTOs;

/// <summary>
/// Konstanta kebijakan tarif sewa kamar rawat inap bertingkat dan alokasi transfer (BKC-DEC-112, BKC-DES-043, BIL-VAL-118, BIL-VAL-119).
/// </summary>
public static class RoomChargeTierPolicies
{
    public const string TarifPenuhSebelum18 = "TARIF_PENUH_SEBELUM_18";
    public const string Potongan50PersenJam18Sd22 = "POTONGAN_50_PERSEN_JAM_18_SD_22";
    public const string Potongan80PersenJam22Sd24 = "POTONGAN_80_PERSEN_JAM_22_SD_24";
    public const string HariBaruTidakDitagih = "HARI_BARU_TIDAK_DITAGIH";
    public const string PenaltiLateCheckout50Persen = "PENALTI_LATE_CHECKOUT_50_PERSEN";
    public const string ProRataTransferMenit = "PRO_RATA_TRANSFER_MENIT";
    public const string TarifStandarHarian = "TARIF_STANDAR_HARIAN";
}

/// <summary>
/// Masukan segmen penempatan kamar untuk kalkulasi pro-rata harian.
/// </summary>
public sealed class PlacementSegmentInput
{
    public Guid PlacementId { get; set; }
    public Guid RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public Guid BedId { get; set; }
    public string BedCode { get; set; } = string.Empty;
    public Guid PatientClassId { get; set; }
    public string PatientClassName { get; set; } = string.Empty;
    public decimal DailyRate { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
}

/// <summary>
/// Hasil kalkulasi satu segmen penempatan kamar pada satu hari kalender tertentu.
/// </summary>
public sealed class RoomPlacementSegmentCalculation
{
    public Guid PlacementId { get; set; }
    public Guid RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public Guid BedId { get; set; }
    public string BedCode { get; set; } = string.Empty;
    public Guid PatientClassId { get; set; }
    public string PatientClassName { get; set; } = string.Empty;
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public int DurationMinutes { get; set; }
    public int TotalDayMinutes { get; set; }
    public decimal DailyRate { get; set; }
    public decimal ProportionPercentage { get; set; }
    public decimal CalculatedAmount { get; set; }
    public string AppliedPolicy { get; set; } = string.Empty;
}

/// <summary>
/// Hasil kalkulasi sewa kamar untuk satu hari kalender.
/// </summary>
public sealed class DailyRoomChargeCalculationResult
{
    public DateOnly Date { get; set; }
    public bool IsFirstDay { get; set; }
    public bool IsDischargeDay { get; set; }
    public string PrimaryPolicy { get; set; } = string.Empty;
    public decimal DayMultiplier { get; set; } = 1.00m;
    public decimal BaseRoomChargeAmount { get; set; }
    public bool IsLateCheckoutApplied { get; set; }
    public decimal LateCheckoutFee { get; set; }
    public decimal TotalDayChargeAmount { get; set; }
    public List<RoomPlacementSegmentCalculation> Segments { get; set; } = new();
}

/// <summary>
/// Ringkasan komprehensif sewa kamar rawat inap untuk satu episode perawatan.
/// </summary>
public sealed class InpatientEpisodeRoomChargeSummary
{
    public Guid EpisodeId { get; set; }
    public Guid EncounterId { get; set; }
    public DateTimeOffset AdmissionTime { get; set; }
    public DateTimeOffset? DischargeTime { get; set; }
    public int TotalCareDays { get; set; }
    public decimal TotalBaseRoomCharge { get; set; }
    public decimal TotalLateCheckoutFee { get; set; }
    public decimal GrandTotalRoomCharge { get; set; }
    public List<DailyRoomChargeCalculationResult> DailyBreakdowns { get; set; } = new();
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// DTO Request penerimaan event sewa kamar harian dari outbox Rawat Inap (BIL-API-1.4, POST /invoices/occupancy-charges).
/// </summary>
public sealed class OccupancyChargeRequest
{
    [Required]
    public Guid EncounterId { get; set; }

    [Required]
    public Guid PlacementId { get; set; }

    [Required]
    public string RoomId { get; set; } = string.Empty;

    public string RoomName { get; set; } = string.Empty;

    [Required]
    public string BedId { get; set; } = string.Empty;

    public string BedCode { get; set; } = string.Empty;

    [Required]
    public string PatientClassId { get; set; } = string.Empty;

    [Required]
    public DateTimeOffset OccupancyStartAt { get; set; }

    public DateTimeOffset? OccupancyEndAt { get; set; }

    public string ChangeType { get; set; } = "BED_OCCUPIED";

    public int Version { get; set; } = 1;
}

/// <summary>
/// DTO Response kalkulasi sewa kamar (BIL-API-1.4, POST /invoices/occupancy-charges).
/// </summary>
public sealed class OccupancyChargeResponse
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal CurrentChargeAmount { get; set; }
    public string AppliedPolicy { get; set; } = string.Empty;
    public decimal RoomChargeAmount { get; set; }
    public long VersionNo { get; set; }
}
